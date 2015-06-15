using DaggerNet.DOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using DaggerNet.Abstract;
using DaggerNet.Migrations;
using Dapper;
using System.Data;
using System.Linq.Expressions;
using DaggerNet.Linq;
using System.Reflection;

namespace DaggerNet
{
  /// <summary>
  /// DbServer meant to handle Database Server management.
  /// This meant to be Sigleton in app lifecyle in most case
  /// </summary>
  public abstract class DbServer
  {
    private object _locker = new object();
    private string _intialScript;

    private string _cntStrFormat;
    private string _defaultDb;
    private IDictionary<string, Table> _tables;

    private DaggerFactory _daggers;
    private Migrator _migrator;


    /// <summary>
    /// A root/admin connection is required.
    /// </summary>
    /// <param name="rootConnection"></param>
    public DbServer(IEnumerable<Type> types, string cntStrFormat, string defaultDb, SqlGenerator sqlGen)
    {
      _cntStrFormat = cntStrFormat;
      _defaultDb = defaultDb;
      _tables = new Class2Dom(types).AddType<MigrationHistory>().Produce().ToDictionary(
        t => t.Type == null ? GetTableKey(t.ManyToMany) : GetTableKey(t.Type), 
        t => t);

      Generator = sqlGen;
      Logger = delegate(string s) { }; // null logger;
    }

    /// <summary>
    /// return create tabls scripts
    /// </summary>
    protected string InitialScript
    {
      get
      {
        if (_intialScript.IsValid())
          return _intialScript;

        lock (_locker)
        {
          if (_intialScript.IsValid())
            return _intialScript;

          _intialScript = Generator.CreateTables(_tables.Values);
        }
        return _intialScript;
      }
    }

    /// <summary>
    /// Make table key for Table or Many-to-Many Table
    /// </summary>
    /// <param name="types">Type/Two Types(M2M)</param>
    /// <returns>Table Key</returns>
    protected string GetTableKey(params Type[] types)
    {
      if (!types.Any() || types.Length > 2)
        throw new ArgumentException("Accept only One or Two types");

      return types.Length == 1 ? types[0].Name : types.Select(t => t.Name).OrderBy(n => n).Implode("|");
    }

    /// <summary>
    /// Return table for Type or Types(many-to-many)
    /// </summary>
    public Table GetTable(params Type[] types)
    {
      var tableKey = GetTableKey(types);

      if (!_tables.ContainsKey(tableKey))
        throw new Exception("Table not found for " + tableKey);

      return _tables[tableKey];
    }

    public IEnumerable<Table> Model
    {
      get { return _tables.Values; }
    }

    /// <summary>
    /// Setup default logger
    /// </summary>
    public Action<string> Logger { get; set; }

    /// <summary>
    /// Return SqlGenerator instance
    /// </summary>
    public SqlGenerator Generator { get; protected set; }

    /// <summary>
    /// Connect to specified database or null for default database for create/drop database
    /// </summary>
    /// <param name="dbName"></param>
    /// <returns></returns>
    public IDbConnection Connect(string dbName = null)
    {
      return CreateConnection(_cntStrFormat.FormatMe(dbName.OrDefault(_defaultDb)));
    }

    /// <summary>
    /// Create a new IDbConnection for given connectionString
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    protected abstract IDbConnection CreateConnection(string connectionString);

    /// <summary>
    /// Check if database exists
    /// </summary>
    /// <returns></returns>
    public bool Exists(string dbname)
    {
      try
      {
        Connect(dbname).Dispose();
        return true;
      }
      catch
      {
        return false;
      }
    }

    /// <summary>
    /// Create the database
    /// </summary>
    public void Create(string dbname)
    {
      if (!Exists(dbname))
      {
        using (var cnt = Connect(Generator.DefaultDatabase))
          cnt.Execute(Generator.GetCreateDatabaseSql(dbname));
      }
      using (var cnt = Connect(dbname))
      {
        Wrap(cnt).Insert<MigrationHistory>(new MigrationHistory(ObjectHelper.ToBinary(Model)));
      }
    }


    /// <summary>
    /// Remove the database
    /// </summary>
    public void Drop(string dbname)
    {
      using (var cnt = Connect(Generator.DefaultDatabase))
      {
        cnt.Execute(Generator.GetSingleUserModeSql(dbname));
        cnt.Execute(Generator.GetDropDatabaseSql(dbname));
      }
    }

    /// <summary>
    /// Update database structure
    /// </summary>
    public void Update(string dbname)
    {
      if (_migrator == null)
      {
        lock(_locker)
        {
          if (_migrator == null)
            _migrator = new Migrator(this);
        }
      }
      using (var cnt = Connect(dbname))
      {
        _migrator.Execute(Wrap(cnt));
      }
    }

    /// <summary>
    /// Perform a Server level backup to specified path
    /// </summary>
    /// <param name="path">backup path</param>
    /// <param name="incremental">indicate a incremental backup</param>
    public abstract void Backup(string path, bool incremental = false);

    /// <summary>
    /// Create a new wrapped connection
    /// </summary>
    /// <returns>Opened Dagger instance</returns>
    public Dagger Draw(string dbName = null)
    {
      var cnt = Connect(dbName);
      cnt.Open();
      return Wrap(cnt);
    }

    /// <summary>
    /// Wrap a IDbConnection to provide more function, like Insert/Update/Delete and Logging.
    /// </summary>
    /// <param name="connection">connection to be wrapped</param>
    /// <param name="logger">Override DbServer level logger</param>
    /// <returns>Dagger instance</returns>
    public Dagger Wrap(IDbConnection connection, Action<string> logger = null)
    {
      if (_daggers == null)
      {
        lock (_locker)
        {
          if (_daggers == null)
            _daggers = new DaggerFactory(this);
        }
      }
      return new Dagger(_daggers, connection, logger ?? Logger);
    }

    /// <summary>
    /// Return a SqlBuilder for building SQL in Linq style
    /// </summary>
    /// <returns>SqlBuilder</returns>
    public SqlBuilder<T> BuildSql<T>()
      where T : class, new()
    {
      return new SqlBuilder<T>(this);
    }
  }
}
