using DaggerNet.DOM;
using DaggerNet.DOM.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using DaggerNet.Abstract;

namespace DaggerNet.PostgresSQL
{
  public class PostgreSQLGenerator : SqlGenerator
  {
    readonly static IDictionary<SqlType, string> _type_map = new Dictionary<SqlType, string>();
    public const string TEXT = "text";
    public const string TIME = "time";
    public const string DATE = "date";

    static PostgreSQLGenerator()
    {
      _type_map.Add(SqlType.Int64, "bigint");
      _type_map.Add(SqlType.Int32, "int");
      _type_map.Add(SqlType.Boolean, "bool");
      _type_map.Add(SqlType.Binary, "bytea");
      _type_map.Add(SqlType.Double, "float8");
      _type_map.Add(SqlType.Currency, "money");
      _type_map.Add(SqlType.Single, "float4");
      _type_map.Add(SqlType.Int16, "int2");
      _type_map.Add(SqlType.Decimal, "numeric");

      _type_map.Add(SqlType.VarNumeric, "numeric");

      _type_map.Add(SqlType.DateTime, "timestamptz");
      _type_map.Add(SqlType.DateTime2, "timestamptz");
      _type_map.Add(SqlType.DateTimeOffset, "timestamptz");
      _type_map.Add(SqlType.Date, "date");
      _type_map.Add(SqlType.Time, "time");
      _type_map.Add(SqlType.TimeSpan, "interval");

      _type_map.Add(SqlType.Text, "text");
      _type_map.Add(SqlType.String, "character varying");

      _type_map.Add(SqlType.Guid, "uuid");
      _type_map.Add(SqlType.IPAddress, "inet");
      _type_map.Add(SqlType.StringFixedLength, "character");
    }

    public override string GetSingleUserModeSql(string dbname)
    {
      var sql = "SELECT pg_terminate_backend(pg_stat_activity.pid) FROM pg_stat_activity WHERE pg_stat_activity.datname = '";
      sql += dbname;
      sql += "' AND pid <> pg_backend_pid();";
      return sql;
    }

    public override string GetInsertIdSql()
    {
      return "SELECT LASTVAL()";
    }

    public override string Quote(string sqlObject)
    {
      return "\"" + sqlObject + "\"";
    }

    public override string MapType(Column column)
    {
      if (column.AutoIncrement)
      {
        if (column.DataType == SqlType.Int32)
          return "serial";
        else if (column.DataType == SqlType.Int64)
          return "bigserial";
        else
          throw new Exception("AutoIncrement supports only int/long. info: {0}.{1}".FormatMe(column.Table.Name, column.Name));
      }

      var db_type = _type_map[column.DataType];
      if (db_type == null)
        throw new Exception("Source type not mapped: {0}".FormatMe(column.DataType.ToReadable()));

      if (column.Precision != null && column.Precision.Any())
        db_type += "({0})".FormatMe(column.Precision.Implode(", "));

      return db_type;
    }

    public override string DefaultDatabase
    {
      get { return "postgres"; }
    }

    public override string MakeLimitSql(string sql, int limit)
    {
      return "{0} LIMIT {1}".FormatMe(sql, limit);
    }

    public override string MakeSkipSql(string sql, long skip)
    {
      return "{0} OFFSET {1}".FormatMe(sql, skip);
    }
  }
}
