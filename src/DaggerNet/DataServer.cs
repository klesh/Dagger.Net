using DaggerNet.Abstract;
using Emmola.Helpers;
using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq;

namespace DaggerNet
{
  /// <summary>
  /// Represent a Data Server
  /// For database manipulation Connect/Create/Drop/GetSize
  /// </summary>
  public abstract class DataServer
  {
    private static Regex _nameRule = new Regex(@"^[0-9a-zA-Z_]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <param name="connectionStringFormat">ConnectionString with database name as {0}</param>
    public DataServer()
    {
      var sqlGenerator = GetSqlGenerator();
      if (sqlGenerator == null)
        throw new ArgumentNullException("SqlGenerator must not be null");
      Sql = sqlGenerator;
    }

    protected string MakeConnectionString(string databaseName)
    {
      if (!_nameRule.IsMatch(databaseName))
        throw new ArgumentException("Illegal database name " + databaseName);

      if (!ConnectionStringFormat.IsValid())
        throw new ArgumentException("Please setup ConnectionStringForm property first.");

      if (!ConnectionStringFormat.Contains("{0}"))
        throw new ArgumentException("ConnectionStringFormat must contains {0} as DataBase Name placehold");

      return ConnectionStringFormat.FormatMe(databaseName);
    }

    /// <summary>
    /// Sql script generator
    /// </summary>
    protected abstract SqlGenerator GetSqlGenerator();

    /// <summary>
    /// Concreate data server must implement this method to return concreate connection;
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    protected abstract DbConnection CreateConnection(string connectionString);

    /// <summary>
    /// Calculate database size in bytes
    /// </summary>
    /// <param name="name">database name</param>
    /// <returns>size in bytes</returns>
    protected abstract long GetDatabaseSize(string name);

    public virtual string ConnectionStringFormat { get; set; }

    public virtual SqlGenerator Sql { get; protected set; }

    /// <summary>
    /// Connect to specified database
    /// </summary>
    /// <param name="databaseName">database name</param>
    /// <returns>DbConnection</returns>
    public DbConnection Open(string databaseName)
    {
      var connection = Connect(databaseName);
      connection.Open();
      return connection;
    }

    /// <summary>
    /// Create closed connection to database
    /// </summary>
    /// <param name="databaseName">database name</param>
    /// <returns>Closed connection</returns>
    public DbConnection Connect(string databaseName)
    {
      return CreateConnection(MakeConnectionString(databaseName));
    }

    /// <summary>
    /// Check if database exists
    /// </summary>
    /// <param name="databaseName">database name</param>
    /// <returns>bool</returns>
    public virtual bool Exists(string databaseName)
    {
      try
      {
        Open(databaseName).Dispose();
        return true;
      }
      catch
      {
        return false;
      }
    }

    protected void CheckDbName(string dbName)
    {
      if (dbName.IcEquals(Sql.DefaultDatabase) || Sql.SystemDatabases.Any(sd => sd.IcEquals(dbName)))
        throw new ArgumentException("Can not perform requested operation on System Database");
    }

    /// <summary>
    /// Create an empty database
    /// </summary>
    /// <param name="databaseName">database name</param>
    public virtual void Create(string databaseName)
    {
      CheckDbName(databaseName);
      using (var connection = Open(Sql.DefaultDatabase))
      {
        // do not use dapper execute since it will cache all sql.
        var command = connection.CreateCommand();
        command.CommandText = Sql.GetCreateDatabaseSql(databaseName);
        command.ExecuteNonQuery();
      }
    }


    /// <summary>
    /// Drop a database
    /// </summary>
    /// <param name="databaseName">database name</param>
    public virtual void Drop(string databaseName)
    {
      CheckDbName(databaseName);
      using (var connection = Open(Sql.DefaultDatabase))
      {
        var command = connection.CreateCommand();
        command.CommandText = Sql.GetSingleUserModeSql(databaseName);
        command.ExecuteNonQuery(); // put database to single use mode
        command.CommandText = Sql.GetDropDatabaseSql(databaseName);
        command.ExecuteNonQuery(); // drop database
      }
    }

    /// <summary>
    /// ReCreate database
    /// </summary>
    /// <param name="databaseName">database name</param>
    public virtual void ReCreate(string databaseName)
    {
      if (Exists(databaseName))
        Drop(databaseName);
      Create(databaseName);
    }

    /// <summary>
    /// Return specified database size in bytes
    /// </summary>
    /// <param name="databaseName">data base name</param>
    /// <returns>bytes</returns>
    public virtual long GetSize(string databaseName)
    {
      return GetDatabaseSize(databaseName);
    }
  }
}
