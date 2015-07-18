using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using DaggerNet.Abstract;

namespace DaggerNet
{
  /// <summary>
  /// Build and cache Insert/Update/Delete statements
  /// </summary>
  public class SqlManager
  {

    private Dictionary<string, string> _selects = new Dictionary<string, string>();
    private Dictionary<string, string> _inserts = new Dictionary<string, string>();
    private Dictionary<string, string> _updates = new Dictionary<string, string>();
    private Dictionary<string, string> _deletes = new Dictionary<string, string>();
    private Dictionary<string, string> _updateTimes = new Dictionary<string, string>();

    public SqlManager(DataModel model, SqlGenerator sql)
    {
      // TODO: Complete member initialization
      Model = model;
      Sql = sql;
    }

    public DataModel Model { get; protected set; }

    public SqlGenerator Sql { get; protected set; }

    protected string GetCachedSQL(Table table, Type actualType, ref Dictionary<string, string> cache, Func<string> build)
    {
      var key = table.Name;
      if (actualType != null)
        key += actualType.FullName;

      if (cache.ContainsKey(key))
        return cache[key];

      lock (cache)
      {
        if (cache.ContainsKey(key))
          return cache[key];

        var sql = build();
        cache.Add(key, sql);
        return sql;
      }
    }

    public string GetInsertSQL(Table table, Type actualType = null)
    {
      return GetCachedSQL(table, actualType, ref _inserts, () =>
      {
        var columns = table.Columns.Where(c => c.AutoIncrement == false);
        if (actualType != null && actualType != table.Type)
        {
          var propertyNames = actualType.GetReadProperties().Select(p => p.Name).ToArray();
          columns = columns.Where(c => propertyNames.Contains(c.Name));
        }
        var arg1 = columns.Select(c => Sql.Quote(c.Name)).Implode(", ");
        var arg2 = columns.Select(c => "@" + c.Name).Implode(", ");
        if (!arg1.IsValid())
          throw new ArgumentException("Can not find any column for type: " + (actualType ?? table.Type).FullName);
        var sql = "INSERT INTO {0} ({1}) VALUES ({2})".FormatMe(Sql.QuoteTable(table), arg1, arg2);
        //if (table.IdColumn != null && table.Type == actualType)
        //  sql += ";" + Sql.GetInsertIdSql() + ";";
        return sql;
      });
    }

    public string GetUpdateSQL(Table table, Type actualType = null)
    {
      return GetCachedSQL(table, actualType, ref _updates, () =>
      {
        var rowVersionCol = table.Columns.FirstOrDefault(c => c.Name == "RowVersion");
        var columnsToUpdate = table.Columns.Where(c => c.AutoIncrement == false && c.Name != "RowVersion");
        var wheresToUpdate = table.PrimaryKey.Columns.Select(c => c.Value).ToList();
        if (actualType != null && actualType != table.Type)
        { // anomynous type supplied
          var propertyNames = actualType.GetPublicProperties().Select(p => p.Name).ToArray();

          // check RowVersion is supplied
          if (rowVersionCol != null && !propertyNames.Contains(rowVersionCol.Name))
            throw new Exception("RowVersion property is missing Table: " + table.Name);

          var primaryKeyNames = table.PrimaryKey.Columns.Select(c => c.Value.Name).ToArray();

          // check actualType has all primary keys
          if (primaryKeyNames.Any(cn => !propertyNames.Contains(cn)))
            throw new Exception("Table {0} requires primary key properties: {1}".FormatMe(table.Name, primaryKeyNames.Implode(", ")));

          // narrow down updating columns
          columnsToUpdate = columnsToUpdate.Where(c => propertyNames.Contains(c.Name)).ToList();
        }
        Func<Column, string> equal = c => "{0}=@{1}".FormatMe(Sql.Quote(c.Name), c.Name);
        var updateStatements = columnsToUpdate.Select(equal).ToList();
        if (rowVersionCol != null)
        { // update row version and add restraint to werhe clause.
          updateStatements.Add("{0}={0} + 1".FormatMe(Sql.QuoteColumn(rowVersionCol)));
          wheresToUpdate.Add(rowVersionCol);
        }
        var whereStatements = wheresToUpdate.Select(equal).ToList();
        return "UPDATE {0} SET {1} WHERE {2}".FormatMe(
          Sql.QuoteTable(table),
          updateStatements.Implode(", "),
          whereStatements.Implode(", ")
        );
      });
    }

    public string GetUpdateTimeSql(Table table, Column updateTimeCol)
    {
      return GetCachedSQL(table, null, ref _updateTimes, () =>
      {
        var whereStatment = table.PrimaryKey.Columns.Select(oc => "{0}=@{1}".FormatMe(
            Sql.QuoteColumn(oc.Value),
            oc.Value.Name
          )).Implode(", ");
        return "UPDATE {2} SET {0}=@UpdateTime WHERE {1}".FormatMe(
          Sql.QuoteColumn(updateTimeCol),
          whereStatment,
          Sql.QuoteTable(table));
      });
    }


    public string GetDeleteSQL(Table table, Type actualType = null)
    {
      return GetCachedSQL(table, actualType, ref _deletes, () =>
      {
        IEnumerable<Column> whereToDelete;
        if (actualType != null && actualType != table.Type)
        {
          var propertyNames = actualType.GetReadProperties().Select(p => p.Name).ToArray();
          whereToDelete = table.Columns.Where(c => propertyNames.Contains(c.Name));
        }
        else
        {
          whereToDelete = table.PrimaryKey.Columns.Select(pc => pc.Value);
        }
        var whereClause = whereToDelete.Select(c => "{0}={1}".FormatMe(Sql.QuoteColumn(c), "@" + c.Name)).Implode(", ");
        if (!whereClause.IsValid())
          throw new ArgumentException("Can not find any column for type: " + (actualType ?? table.Type).FullName);
        return "DELETE FROM {0} WHERE {1}".FormatMe(Sql.QuoteTable(table), whereClause);
      });
    }
  }
}
