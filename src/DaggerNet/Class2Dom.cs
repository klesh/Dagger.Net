using DaggerNet.Abstract;
using DaggerNet.Attributes;
using DaggerNet.DOM;
using DaggerNet.DOM.Abstract;
using Emmola.Helpers;
using Emmola.Helpers.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        foreach (var property in type.GetReadWriteProperties())
        {
          var propertyType = property.PropertyType;
          var isNullable = property.PropertyType.IsNullableType();
          var isSimpleType = property.PropertyType.IsSimpleType();
          var isBinary = property.PropertyType == typeof(byte[]);

          // try to figure many-to-many relationship
          if (propertyType.IsEnumerable())
          {
            var elementType = propertyType.GetElementTypeExt();
            var otherTable = tables.FirstOrDefault(t => t.Type == elementType);
            if (otherTable != null)
            {
              // find out other type has IEnumerable property to this type.
              var otherType = otherTable.Type;
              var otherProperties = otherType.GetReadWriteProperties()
                .Where(pi => pi.PropertyType.IsEnumerable() && pi.PropertyType.GetElementTypeExt() == type).ToArray(); 
              if (otherProperties.Length == 1) // found matched many-to-many relationship
              { // not all columns are surely added, postpond the build up.
                var m2mTable = new Table(new Type[] {type, otherTable.Type}.OrderBy(t => t.Name).ToArray());
                tables.Add(m2mTable);
              }
            }
          }

          if (!isSimpleType && !isNullable && !isBinary) 
            continue;


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
                throw new Exception("Unknow DataType:" + dataType.DataType.ToReadable());
            }
          }
          else
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
              else
                throw new Exception("Unknow DataType:" + innerType.ToString());

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

          column.NotNull = !(isNullable || propertyType == typeof(string)) || column.PrimaryKey || property.HasAttribute<RequiredAttribute>();

          property.IfAttribute<DefaultAttribute>((da) => column.Default = da.Default);

          table.Columns.Add(column);


          // Process index
          var indexAttribute = property.GetAttribute<IndexAttribute>();
          if (indexAttribute != null)
          {
            var indexName = indexAttribute.Name.OrDefault("IX_{0}", property.Name);
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

            if (referAttribute.CascadeDelete)
              foreignKey.CascadeDelete = true;
            if (referAttribute.CascadeUpdate)
              foreignKey.CascadeUpdate = true;
          }
        }

        var tmp = table.Columns.Where(c => c.PrimaryKey && c.AutoIncrement);
        if (tmp.Count() == 1)
          table.IdColumn = tmp.First();

        if (table.Columns.Any(c => c.PrimaryKey))
          table.PrimaryKey = new PrimaryKey(table);

        tables.Add(table);
      }

      foreach (var table in tables)
      {

        // Build up many-to-many tables
        if (table.ManyToMany != null)
        {
          foreach (var oneType in table.ManyToMany)
          {
            var oneTable = tables.First(t => t.Type == oneType);
            var oneFk = new ForeignKey(table, oneType);
            var i = 0;
            if (oneTable.PrimaryKey == null)
              throw new Exception("Many-To-Many fail due to Type {0} has no primary key".FormatMe(oneType.FullName));

            foreach (var pkc in oneTable.PrimaryKey.Columns)
            {
              var pk = pkc.Value;
              var oneCol = new Column(table);
              oneCol.Name = oneTable.Name + pk.Name;
              oneCol.DataType = pk.DataType;
              oneCol.Precision = pk.Precision;
              oneCol.NotNull = true;
              oneCol.PrimaryKey = true;
              table.Columns.Add(oneCol);

              oneFk.Columns.Add(new Ordered<Column>(oneCol, i++));
            }
            table.ForeignKeys.Add(oneFk);
            table.Name += oneType.Name;
          }
          table.PrimaryKey = new PrimaryKey(table);
        }

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
            throw new Exception("Foreign keys not match, Source: {0} Target: {1} ({2})".FormatMe(
              foreignKey, 
              refTable.Name,
              refKeys.Select(c => c.Name).Implode(", ")
            ));

          foreignKey.ReferTable = refTable;
          foreignKey.Name = "FK_" + foreignKey.Columns.Select(c => c.Value.Name).Implode("_");
          foreignKey.ReferColumns = refKeys;
        }
      }

      return tables;
    }
  }
}
