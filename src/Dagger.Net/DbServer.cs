using Dagger.Net.DOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using Dagger.Net.Abstract;
using Dagger.Net.Migrations;
using Dapper;
using System.Data;
using System.Linq.Expressions;
using Dagger.Net.Linq;
using System.Reflection;

namespace Dagger.Net
{
  /// <summary>
  /// DbServer meant to handle Database Server management.
  /// This meant to be Sigleton in app lifecyle in most case
  /// </summary>
  public class DbServer
  {
    string _intialScript;
    object _locker = new object();

    private string _default;
    private IDbConnection _connection;
    private DaggerFactory _daggers;
    private IDictionary<string, Table> _tables = new Dictionary<string, Table>();

    /// <summary>
    /// A root/admin connection is required.
    /// </summary>
    /// <param name="rootConnection"></param>
    public DbServer(IEnumerable<Type> types, IDbConnection rootConnection, SqlGenerator generator)
    {
      if (rootConnection != null)
        _default = rootConnection.Database;
      _connection = rootConnection;
      _tables = new Class2Dom(types).AddType<MigrationHistory>().Produce().ToDictionary(
        t => t.Type == null ? GetTableKey(t.ManyToMany) : GetTableKey(t.Type), 
        t => t);

      Logger = delegate(string s) { }; // null logger;
      Generator = generator;
    }

    /// <summary>
    /// Make an standard table key for fast access
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    internal static string GetTableKey(params Type[] keys)
    {
      if (keys.Length == 1)
        return keys[0].Name;
      return keys.Select(t => t.Name).OrderBy(n => n).Implode("|");
    }

    /// <summary>
    /// Return table for Type or Types(many-to-many)
    /// </summary>
    public Table GetTable(params Type[] types)
    {
      var key = GetTableKey(types);
      if (!_tables.ContainsKey(key))
        throw new Exception("Table not found for " + types.Select(t => t.FullName).Implode(", "));
      return _tables[key];
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

    protected void ResetDefault()
    {
      if (_connection.Database != _default)
        _connection.ChangeDatabase(_default);
    }

    /// <summary>
    /// Check if database exists
    /// </summary>
    /// <returns></returns>
    public bool Exists(string dbname)
    {
      try
      {
        _connection.ChangeDatabase(dbname);
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
      ResetDefault();
      using (_connection.OpenScope())
      {
        _connection.Execute(Generator.GetCreateDatabaseSql(dbname));
        _connection.ChangeDatabase(dbname);
        _connection.Execute(InitialScript);

        var initialHistory = new MigrationHistory()
        {
          Id = DateTime.Now.Ticks,
          Title = "InitialCreate",
          Model = ObjectHelper.ToBinary(_tables)
        };

        Wrap(_connection).Insert<MigrationHistory>(initialHistory);
      }
    }


    /// <summary>
    /// Remove the database
    /// </summary>
    public void Drop(string dbname)
    {
      ResetDefault();
      using (_connection.OpenScope())
      {
        _connection.Execute(Generator.GetSingleUserModeSql(dbname));
        _connection.Execute(Generator.GetDropDatabaseSql(dbname));
      }
    }

    /// <summary>
    /// Update database structure
    /// </summary>
    public void Update(string dbname)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Perform a Server level backup to specified path
    /// </summary>
    /// <param name="path">backup path</param>
    /// <param name="incremental">indicate a incremental backup</param>
    public void Backup(string path, bool incremental = false)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Wrap a IDbConnection to provide more function, like Insert/Update/Delete and Logging.
    /// </summary>
    /// <param name="connection">connection to be wrapped</param>
    /// <param name="logger">Override DbServer level logger</param>
    /// <returns></returns>

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

    public SqlBuilder<T> BuildSql<T>(Expression<Func<T, object>> columnsExp = null)
      where T : class, new()
    {
      return new SqlBuilder<T>(this);
    }
  }
}
