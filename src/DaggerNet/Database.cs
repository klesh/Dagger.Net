using DaggerNet.Abstract;
using Dapper;
using System;
using System.Data.Common;
using System.Linq;
using Emmola.Helpers;
using DaggerNet.Migrations;

namespace DaggerNet
{
  /// <summary>
  /// Represent a database in a data server.
  /// </summary>
  public class DataBase
  {
    public DataBase(DataFactory factory, string name)
    {
      if (factory == null)
        throw new ArgumentNullException("DataFactory can not be null");

      if (name == null)
        throw new ArgumentNullException("Database name cna not be null");


      Factory = factory;
      Name = name;
      Model = factory.Model;
      Server = factory.Server;
      Sql = Server.Sql;
      Manager = factory.Manager;
      Logger = factory.Logger;
    }

    public DataFactory Factory { get; protected set; }

    public string Name { get; protected set; }

    public DataModel Model { get; protected set; }

    public DataServer Server { get; protected set; }

    public SqlGenerator Sql { get; protected set; }

    public SqlManager Manager { get; protected set; }

    public Action<string> Logger { get; set; }

    /// <summary>
    /// Check if database initialized
    /// </summary>
    public bool IsInitialized
    {
      get
      {
        using (var cnt = Server.Open(Name))
        {
          return cnt.ExecuteScalar<bool>(Sql.GetTableExistsSql(), new
          {
            Schema = Sql.DefaultSchema,
            Table = Sql.ConvertTableName(Model.GetTable(typeof(MigrationHistory)))
          });
        }
      }
    }
    
    /// <summary>
    /// Initialize a database according to DatabaseModel
    /// </summary>
    public void Initialize()
    {
      using (var dagger = Draw())
      {
        dagger.ExecuteNonQuery(Factory.InitialScript);
        dagger.Insert<MigrationHistory>(new MigrationHistory(ObjectHelper.ToBinary(Model.Tables)));
      }
    }

    /// <summary>
    /// Migrate database to newest migration
    /// </summary>
    public void Migrate()
    {
      if (Factory.Migrator.Migrations.Any())
      {
        Factory.Migrator.Execute(this);
      }
    }

    /// <summary>
    /// Initialize database or migrate to newest version
    /// </summary>
    public void InitializeOrMigrate()
    {
      if (IsInitialized)
        Migrate();
      else
        Initialize();
    }

    /// <summary>
    /// Return database size in bytes
    /// </summary>
    /// <returns>size in bytes</returns>
    public long GetSize()
    {
      return Server.GetSize(Name);
    }

    /// <summary>
    /// Return a Dagger instance
    /// </summary>
    /// <param name="cnt"></param>
    /// <returns></returns>
    public Dagger Wrap(DbConnection cnt)
    {
      return new Dagger(this, cnt);
    }

    /// <summary>
    /// Open a wrapped connection to database
    /// </summary>
    /// <returns></returns>
    public Dagger Draw()
    {
      return new Dagger(this, Server.Connect(Name));
    }
  }
}
