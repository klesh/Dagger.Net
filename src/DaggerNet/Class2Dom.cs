using DaggerNet.Abstract;
using DaggerNet.Attributes;
using DaggerNet.DOM;
using DaggerNet.DOM.Abstract;
using DaggerNet.TypeHandles;
using Dapper;
using Emmola.Helpers;
using Emmola.Helpers.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;

namespace DaggerNet
{
  public class Class2Dom : DomProducer
  {
    private HashSet<Type> _types = new HashSet<Type>();

    public Class2Dom(IEnumerable<Type> types)
    {
      _types.AddRange(types);
    }

    public Class2Dom(Type type)
    {
      _types.Add(type);
    }

    public Class2Dom AddType<T>()
    {
      _types.Add(typeof(T));
      return this;
    }

    public Class2Dom AddTypes(IEnumerable<Type> types)
    {
      _types.AddRange(types);
      return this;
    }

    public override HashSet<Table> Produce()
    {
      var tables = new HashSet<Table>();

      foreach (var type in _types)
      {
        var table = new Table(type);

        var properties = type.GetReadWriteProperties().Where(p => !p.HasAttribute<NotMappedAttribute>());
        foreach (var property in properties)
        {
          var propertyType = property.PropertyType;
          var isNullable = property.PropertyType.IsNullableType();
          var isSimpleType = property.PropertyType.IsSimpleType();
          var isBinary = property.PropertyType == typeof(byte[]);

          // try to figure out many-to-many relationship
          //if (propertyType.IsEnumerable())
          //{
          //  var elementType = propertyType.GetElementTypeExt();
          //  var otherTable = tables.FirstOrDefault(t => t.Type == elementType);
          //  if (otherTable != null)
          //  {
          //    // find out other type has IEnumerable property to this type.
          //    var otherType = otherTable.Type;
          //    var otherProperties = otherType.GetReadWriteProperties()
          //      .Where(pi => pi.PropertyType.IsEnumerable() && pi.PropertyType.GetElementTypeExt() == type).ToArray(); 
          //    if (otherProperties.Length == 1) // found matched many-to-many relationship
          //    { // not all columns are surely added, postpond the build up.
          //      var m2mTable = new Table(new Type[] {type, otherTable.Type}.OrderBy(t => t.Name).ToArray());
          //      tables.Add(m2mTable);
          //    }
          //  }
          //}

          // try to resolve complex type by convert it to JSON field.
          if (propertyType.HasAttribute<ComplexTypeAttribute>())
          {
            var jsonColumn = new Column(table, property);
            jsonColumn.DataType = SqlType.Json;
            SqlMapper.AddTypeHandler(propertyType, new JsonTypeHandler());
            table.Columns.Add(jsonColumn);
            continue;
          }

          if (!isSimpleType && !isNullable && !isBinary) 
            continue;

          if (property.Name == "RowVersion" && property.PropertyType != typeof(int) && propertyType != typeof(long))
            throw new Exception("RowVersion column must be int or long " + type.FullName);


          var column = new Column(table, property);

          var dataType = property.GetAttribute<DataTypeAttribute>();
          if (dataType != null)
          {
            switch (dataType.DataType)
            {
              case DataType.Html:
              case DataType.MultilineText:
              case DataType.Text:
                column.DataType = SqlType.Text;
                break;
              case DataType.Date:
                column.DataType = SqlType.Date;
                break;
              case DataType.Time:
                column.DataType = SqlType.Time;
                break;
              case DataType.Currency:
                column.DataType = SqlType.Currency;
                break;
              default:
                dataType = null; // ignore unrecognize DataType, fallback to old routine
                break;
            }
          }

          if (dataType == null)
          {
            if (propertyType == typeof(string))
            {
              var length = 255;
              var stringLength = property.GetAttribute<StringLengthAttribute>();
              if (stringLength != null)
                length = stringLength.MaximumLength;
              else
              {
                var maxLength = property.GetAttribute<MaxLengthAttribute>();
                if (maxLength != null)
                  length = maxLength.Length;
              }
              column.DataType = SqlType.String;
              column.Precision = new object[] { length };
            }
            else
            {
              var innerType = isNullable ? propertyType.GetNullableValueType() : propertyType;
              if (innerType == typeof(int))
                column.DataType = SqlType.Int32;
              else if (innerType == typeof(long))
                column.DataType = SqlType.Int64;
              else if (innerType == typeof(bool))
                column.DataType = SqlType.Boolean;
              else if (innerType == typeof(byte[]))
                column.DataType = SqlType.Binary;
              else if (innerType == typeof(DateTime))
                column.DataType = SqlType.DateTime;
              else if (innerType == typeof(double))
                column.DataType = SqlType.Double;
              else if (innerType == typeof(decimal))
                column.DataType = SqlType.Decimal;
              else if (innerType == typeof(float))
                column.DataType = SqlType.Single;
              else if (innerType == typeof(short))
                column.DataType = SqlType.Int16;
              else if (innerType == typeof(TimeSpan))
                column.DataType = SqlType.TimeSpan;
              else if (innerType == typeof(IPAddress))
                column.DataType = SqlType.IPAddress;
              else if (innerType == typeof(Guid))
                column.DataType = SqlType.Guid;
              else if (innerType == typeof(Array))
                column.DataType = SqlType.Array;
              else if (innerType.IsEnum)
                column.DataType = SqlType.Int32;
              else
                throw new Exception(
                  "Unknow DataType {0} on {1}.{2}".FormatMe(innerType.ToString(), type.FullName, property.Name)
                  );

              //if (!isNullable)
              //  column.NotNull = true;
            }
          }

          var primaryKey = property.GetAttribute<PrimaryKeyAttribute>();
          if (primaryKey != null) 
          {
            column.PrimaryKey = true;
            column.AutoIncrement = primaryKey.AutoIncrement;
          }

          if (column.DataType == SqlType.DateTime && property.HasAttribute<UpdateTimeAttribute>())
          {
            column.UpdateTime = true;
          }

          column.NotNull = !(isNullable || propertyType == typeof(string)) || column.PrimaryKey || property.HasAttribute<RequiredAttribute>();

          property.IfAttribute<DefaultAttribute>((da) => column.Default = da.Default);

          table.Columns.Add(column);


          // Process index
          var indexAttribute = property.GetAttribute<IndexAttribute>();
          if (indexAttribute != null)
          {
            var indexName = indexAttribute.Name.OrDefault("IX_{0}_{1}", table.Name, property.Name);
            var index = table.Indexes.FindOrAdd(i => i.Name == indexName, () => new Index(table, indexName));
            index.Columns.Add(new IndexColumn(column, indexAttribute.Order, indexAttribute.Descending));
            if (indexAttribute.Unique)
              index.Unique = true;
          }

          // Collect foreign keys
          var referAttribute = property.GetAttribute<ReferenceAttribute>();
          if (referAttribute != null)
          {
            var referType = referAttribute.ReferenceType ??  type;
            var foreignKey = table.ForeignKeys.FindOrAdd(fk => fk.ReferType == referType, () => new ForeignKey(table, referType));

            foreignKey.Table = table;
            foreignKey.Columns.Add(new Ordered<Column>(column, referAttribute.Order));

            foreignKey.OnDelete = referAttribute.OnDelete;
            foreignKey.OnUpdate = referAttribute.OnUpdate;

            // try to find out suitable cacade action
            if (foreignKey.OnDelete == Cascades.Auto)
            {
              if (isNullable)
                foreignKey.OnDelete = Cascades.SetNull;
              else
                foreignKey.OnDelete = Cascades.Cascade;
            }
          }
        }

        var tmp = table.Columns.Where(c => c.PrimaryKey && c.AutoIncrement);
        if (tmp.Count() == 1)
          table.IdColumn = tmp.First();

        //if (table.Columns.Any(c => c.PrimaryKey))
          table.PrimaryKey = new PrimaryKey(table);

        if (table.Columns.Count(c => c.UpdateTime) > 1)
          throw new Exception("Multiple UpdateTimeAttribute detected on Type: " + table.Name);

        tables.Add(table);
      }

      foreach (var table in tables)
      {

        // Build up many-to-many tables
        //if (table.ManyToMany != null)
        //{
        //  int p = 0;
        //  foreach (var oneType in table.ManyToMany)
        //  {
        //    var oneTable = tables.First(t => t.Type == oneType);
        //    var oneFk = new ForeignKey(table, oneType);
        //    int i = 0;
        //    if (oneTable.PrimaryKey == null)
        //      throw new Exception("Many-To-Many fail due to Type {0} has no primary key".FormatMe(oneType.FullName));

        //    foreach (var pkc in oneTable.PrimaryKey.Columns)
        //    {
        //      var pk = pkc.Value;
        //      var oneCol = new Column(table);
        //      oneCol.Name = oneTable.Name + pk.Name;
        //      oneCol.DataType = pk.DataType;
        //      oneCol.Precision = pk.Precision;
        //      oneCol.NotNull = true;
        //      oneCol.PrimaryKey = true;
        //      oneCol.Order = p++;
        //      table.Columns.Add(oneCol);

        //      oneFk.Columns.Add(new Ordered<Column>(oneCol, i++));
        //    }
        //    oneFk.OnDelete = Cascades.Cascade;
        //    table.ForeignKeys.Add(oneFk);
        //    table.Name += oneType.Name;
        //  }
        //  table.PrimaryKey = new PrimaryKey(table);
        //}

        if (!table.ForeignKeys.Any())
          continue;

        // Build up reference relation
        foreach (var foreignKey in table.ForeignKeys)
        {
          var refTable = tables.FirstOrDefault(t => t.Type == foreignKey.ReferType);
          if (refTable == null)
            throw new Exception("Reference to an unmapped type, Source: {0} Target: {1} ".FormatMe(foreignKey, foreignKey.ReferType));

          var refKeys = refTable.PrimaryKey.Columns.Select(oc => oc.Value).ToList();

          if (foreignKey.Columns.Count != refKeys.Count)
            throw new Exception("Foreign keys are not matched, Source: {0} Target: {1} ({2})".FormatMe(
              foreignKey, 
              refTable.Name,
              refKeys.Select(c => c.Name).Implode(", ")
            ));

          foreignKey.ReferTable = refTable;
          foreignKey.Name = "FK_" + table.Name + "_" + foreignKey.Columns.Select(c => c.Value.Name).Implode("_");
          foreignKey.ReferColumns = refKeys;

          if (foreignKey.OnUpdate == Cascades.Auto &&
              (refKeys.Count() != 1 || refKeys.First().AutoIncrement == false))
            foreignKey.OnUpdate = Cascades.Cascade;
        }
      }

      return tables;
    }
  }
}
