using DaggerNet.Abstract;
using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using System.Data;
using Dapper;

namespace DaggerNet
{
  public class DaggerFactory
  {
    public DbServer Server { get; protected set; }

    private Dictionary<Type, string> _selects = new Dictionary<Type, string>();
    private Dictionary<Type, string> _inserts = new Dictionary<Type, string>();
    private Dictionary<Type, string> _updates = new Dictionary<Type, string>();
    private Dictionary<Type, string> _deletes = new Dictionary<Type, string>();

    public DaggerFactory(DbServer server)
    {
      // TODO: Complete member initialization
      Server = server;
    }

    //internal T2 Find<T1, T2>(IDbConnection connection, params object[] keys)
    //{
    //  var sql = GetSelectSQL<T1>(typeof(T2));
    //  var table = _server.GetTable(typeof(T2));
    //  var parameters = new DynamicParameters();
    //  if (keys.Length != table.PrimaryKeys.Count)
    //    throw new Exception("Primary key not match: " + table.PrimaryKeys.Select(k => k.Name).Implode(", "));
    //  var keysEnum = keys.GetEnumerator();
    //  foreach (var pk in table.PrimaryKeys)
    //  {
    //    keysEnum.MoveNext();
    //    parameters.Add(pk.Name, keysEnum.Current);
    //  }
    //  return connection.ExecuteScalar<T2>(sql, parameters);
    //}

    internal int Insert<T>(object entity, IDbConnection connection)
    {
      var tableType = typeof(T);
      var innerType = entity.GetType();
      var table = Server.GetTable(tableType);
      var sql = GetInsertSQL<T>(innerType);
      var ret = connection.Execute(sql, entity);
      if (ret == 1 && table.IdColumn != null && tableType == innerType)
      {
        var id = connection.ExecuteScalar(Server.Generator.GetInsertIdSql());
        table.IdColumn.SetMethod.Invoke(entity, new object[] { id });
      }
      return ret;
    }

    internal string GetSQL<T>(Type innerType, ref Dictionary<Type, string> cache, Func<string> build)
    {
      innerType = innerType ?? typeof(T);

      if (cache.ContainsKey(innerType))
        return cache[innerType];

      lock (cache)
      {
        if (cache.ContainsKey(innerType))
          return cache[innerType];

        var sql = build();
        cache.Add(innerType, sql);
        return sql;
      }
    }

    //internal string GetSelectSQL<T>(Type innerType = null)
    //{
    //  var type = typeof(T);
    //  return GetSQL<T>(innerType, ref _selects, () =>
    //  {
    //    var table = _map[type.Name];
    //    var arg1 = "*";
    //    if (innerType != null && innerType != type)
    //    {
    //      var propertyNames = innerType.GetReadWriteProperties().Select(p => p.Name).ToArray();
    //      arg1 = table.Columns.Where(c => propertyNames.Contains(c.Name)).Select(c => _generator.Quote(c.Name)).Implode(", ");
    //    }
    //    var arg2 = _generator.Quote(table.Name);
    //    var arg3 = table.PrimaryKeys.Select(c => "{0}=@{1}".FormatMe(_generator.Quote(c.Name), c.Name)).Implode(" AND ");
    //    return "SELECT {0} FROM {1} WHERE {2}".FormatMe(arg1, arg2, arg3);
    //  });
    //}

    internal string GetInsertSQL<T>(Type innerType = null)
    {
      var type = typeof(T);
      return GetSQL<T>(innerType, ref _inserts, () =>
      {
        var table = Server.GetTable(type);
        var columns = table.Columns.Where(c => c.AutoIncrement == false);
        if (innerType != null && innerType != type)
        {
          var propertyNames = innerType.GetReadWriteProperties().Select(p => p.Name).ToArray();
          columns = columns.Where(c => propertyNames.Contains(c.Name));
        }
        var arg1 = columns.Select(c => Server.Generator.Quote(c.Name)).Implode(", ");
        var arg2 = columns.Select(c => "@" + c.Name).Implode(", ");
        return "INSERT INTO {0} ({1}) VALUES ({2})".FormatMe(Server.Generator.QuoteTable(table), arg1, arg2);
      });
    }

    //internal string GetUpdateSQL<T>(Type innerType = null)
    //{
    //  var type = typeof(T);
    //  return GetSQL<T>(innerType, ref _updates, () =>
    //  {
    //    var table = _map[type.Name];
    //    Func<Column, string> equal = c => "{0}=@{1}".FormatMe(_generator.Quote(c.Name), c.Name);
    //    var columns = table.Columns.Where(c => c.AutoIncrement == false);
    //    if (innerType != null && innerType != type)
    //    {
    //      var propertyNames = innerType.GetPublicProperties().Select(p => p.Name).ToArray();
    //      columns = columns.Where(c => propertyNames.Contains(c.Name));
    //    }
    //    var arg1 = columns.Select(equal).Implode(", ");
    //    var arg2 = table.PrimaryKeys.Select(equal).Implode(", ");
    //    return "UPDATE {0} SET {1} WHERE {2}".FormatMe(_generator.QuoteTable(table), arg1, arg2);
    //  });
    //}

    //internal string GetDeleteSQL<T>()
    //{
    //  return GetSQL<T>(null, ref _deletes, () =>
    //  {
    //    var table = _map[typeof(T).Name];
    //    var columns = table.Columns.Where(c => c.AutoIncrement == false);
    //    var arg1 = columns.Select(c => _generator.Quote(c.Name)).Implode(", ");
    //    var arg2 = columns.Select(c => "@" + c.Name).Implode(", ");
    //    return "DELETE FROM {0} WHERE {1}".FormatMe(_generator.QuoteTable(table), arg1, arg2);
    //  });
    //}
  }
}
