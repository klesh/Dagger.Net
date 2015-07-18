using Emmola.Helpers;
using System;
using System.Linq;

namespace DaggerNet.Migrations
{
  /// <summary>
  /// Single instance per DbServer, responsible for execute migrations
  /// </summary>
  public class Migrator
  {
    private object _locker = new object();
    private Migration[] _migrations = null;
    private DataFactory _factory;

    /// <summary>
    /// Migrator must bind to a DataFactory
    /// That we can search all migrations for this DataFactory and Run migrations.
    /// </summary>
    /// <param name="dataFactory"></param>
    public Migrator(DataFactory dataFactory)
    {
      if (dataFactory == null)
        throw new ArgumentNullException("dataFactory can not be null");

      _factory = dataFactory;
    }

    /// <summary>
    /// Get all pending migration in assembly
    /// </summary>
    public Migration[] Migrations
    {
      get
      {
        if (_migrations == null)
        {
          lock(_locker)
          {
            if (_migrations == null)
            {
              var factoryType = _factory.GetType();
              // load all migrations of dataModel's assembly
              _migrations = typeof(Migration)
                .FindSubTypesIn(_factory.GetType().Assembly)
                .Select(t => (Migration)Activator.CreateInstance(t))
                .Where(i => i.DataFactoryType == factoryType)
                .ToArray();
            }
          }
        }
        return _migrations;
      }
    }

    /// <summary>
    /// Run all pending migrations on Database base on Id
    /// </summary>
    /// <param name="dagger"></param>
    public void Execute(DataBase database)
    {
      using (var dagger = database.Draw())
      {
        var lastDatabaseMigration = dagger.From<MigrationHistory>(s => s.OrderByDescending(mh => mh.Id).Limit(1)).FirstOrDefault();
        var pendingMigrations = Migrations.Where(m => m.Id > lastDatabaseMigration.Id).OrderBy(m => m.Id);
        if (pendingMigrations.Any())
        {
          var modelBinary = ObjectHelper.ToBinary(database.Model.Tables);
          foreach (var pendingMigration in pendingMigrations)
          {
            pendingMigration.Execute(dagger);
            var migrationHistory = new MigrationHistory
            {
              Id = pendingMigration.Id,
              Title = pendingMigration.Title,
              Model = modelBinary
            };
            dagger.Insert<MigrationHistory>(migrationHistory);
          }
        }
      }
    }
  }
}
