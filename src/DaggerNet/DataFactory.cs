using DaggerNet.Abstract;
using DaggerNet.Cultures;
using DaggerNet.Linq;
using DaggerNet.Migrations;
using DaggerNet.TypeHandles;
using Dapper;
using System;
using System.Globalization;

namespace DaggerNet
{
  /// <summary>
  /// Holds a DataModel to certain type of DataServer
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public abstract class DataFactory
  {
    protected DataModel _dataModel;
    protected DataServer _dataServer;
    protected Migrator _migrator;
    protected SqlManager _sqlManager;
    protected SqlGenerator _sqlGenerator;
    protected string _initialScript;
    protected string _defaultDbName;
    protected DataBase _defaultDatabase;

    public DataFactory(DataModel model, DataServer dataServer, string connectionStringFormat, string defaultDbname)
    {
      _dataModel = model;
      _dataServer = dataServer;
      _defaultDbName = defaultDbname;
      _sqlGenerator = dataServer.Sql;
      _dataServer.ConnectionStringFormat = connectionStringFormat;
      Logger = delegate (string sql) { };
    }

    public Action<string> Logger { get; set; }

    /// <summary>
    /// Return initial sql script to create database structure
    /// </summary>
    public string InitialScript
    {
      get
      {
        if (_initialScript == null)
          _initialScript = _sqlGenerator.CreateTables(_dataModel.Tables);
        return _initialScript;
      }
    }

    /// <summary>
    /// DataMode as database structure
    /// </summary>
    public DataModel Model
    {
      get
      {
        return _dataModel;
      }
    }

    /// <summary>
    /// Concreate DataServer instance
    /// </summary>
    public DataServer Server
    {
      get
      {
        return _dataServer;
      }
    }

    /// <summary>
    /// Return migartor for this Data Model
    /// </summary>
    /// <returns></returns>
    public Migrator Migrator
    {
      get
      {
        if (_migrator == null)
          return _migrator = new Migrator(this);
        return _migrator;
      }
    }

    /// <summary>
    /// Return SqlManager instance
    /// </summary>
    public SqlManager Manager
    {
      get
      {
        if (_sqlManager == null)
          return _sqlManager = new SqlManager(_dataModel, _sqlGenerator);
        return _sqlManager;
      }
    }

    /// <summary>
    /// If you need many identical database structure, use this method to create new Database instance
    /// </summary>
    /// <param name="dbName">Other database name under same stureture</param>
    /// <returns></returns>
    public DataBase Connect(string dbName)
    {
      return new DataBase(this, dbName);
    }

    /// <summary>
    /// For creating migration, and this may be used in most cases.
    /// </summary>
    public DataBase Default
    {
      get
      {
        if (_defaultDatabase == null)
        {
          if (!Server.Exists(_defaultDbName))
            Server.Create(_defaultDbName);
          _defaultDatabase = Connect(_defaultDbName);
          if (_defaultDatabase.IsInitialized)
            _defaultDatabase.Migrate();
          else
          {
            _defaultDatabase.Initialize();
            DefaultDatabaseInitialized(_defaultDatabase);
          }
        }
        return _defaultDatabase;
      }
    }

    protected virtual void DefaultDatabaseInitialized(DataBase database)
    {
    }

    public Dagger Draw()
    {
      return Default.Draw();
    }

    static DataFactory()
    {
      // Add CultureData support for Dapper
      SqlMapper.AddTypeHandler<CultureData<string>>(new CultureDataHandler<string>());
      SqlMapper.AddTypeHandler<CultureData<bool>>(new CultureDataHandler<bool>());
      SqlMapper.AddTypeHandler<CultureData<byte>>(new CultureDataHandler<byte>());
      SqlMapper.AddTypeHandler<CultureData<char>>(new CultureDataHandler<char>());
      SqlMapper.AddTypeHandler<CultureData<decimal>>(new CultureDataHandler<decimal>());
      SqlMapper.AddTypeHandler<CultureData<double>>(new CultureDataHandler<double>());
      SqlMapper.AddTypeHandler<CultureData<float>>(new CultureDataHandler<float>());
      SqlMapper.AddTypeHandler<CultureData<int>>(new CultureDataHandler<int>());
      SqlMapper.AddTypeHandler<CultureData<long>>(new CultureDataHandler<long>());
      SqlMapper.AddTypeHandler<CultureData<sbyte>>(new CultureDataHandler<sbyte>());
      SqlMapper.AddTypeHandler<CultureData<short>>(new CultureDataHandler<short>());
      SqlMapper.AddTypeHandler<CultureData<uint>>(new CultureDataHandler<uint>());
      SqlMapper.AddTypeHandler<CultureData<ulong>>(new CultureDataHandler<ulong>());
      SqlMapper.AddTypeHandler<CultureData<ushort>>(new CultureDataHandler<ushort>());

      // Add CulatureInfo support
      SqlMapper.AddTypeHandler<CultureInfo>(new CultureInfoTypeHandler());
    }
  }

  /// <summary>
  /// Inherit this
  /// </summary>
  /// <typeparam name="TDataServer">Concreate DataServer type</typeparam>
  public abstract class DataFactory<TDataServer> : DataFactory
    where TDataServer : DataServer, new()
  {
    public DataFactory(DataModel dataModel, string connectionStringFormat, string defaultDbName)
      : base(dataModel, new TDataServer(), connectionStringFormat, defaultDbName)
    {
    }

    public DataFactory(Type baseType, string connectionStringFormat, string defaultDbName)
      : base(new DataModel(baseType), new TDataServer(), connectionStringFormat, defaultDbName)
    {
    }
  }
}
