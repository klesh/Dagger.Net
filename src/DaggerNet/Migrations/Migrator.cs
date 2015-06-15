using DaggerNet.DOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using System.Reflection;

namespace DaggerNet.Migrations
{
  /// <summary>
  /// Single instance per DbServer, responsible for execute migrations
  /// </summary>
  public class Migrator
  {
    private object _locker = new object();
    private Migration[] _migrations = null;
    private DbServer _dbServer;

    public Migrator(DbServer dbServer)
    {
      _dbServer = dbServer;
    }

    protected Migration[] Migrations
    {
      get
      {
        if (_migrations == null)
        {
          lock(_locker)
          {
            if (_migrations == null)
            {
              // load all migrations base on DbServer's assembly
              _migrations = typeof(Migration)
                .FindSubTypesIn(_dbServer.GetType().Assembly)
                .Select(t => (Migration)Activator.CreateInstance(t))
                .ToArray();
            }
          }
        }
        return _migrations;
      }
    }

    public void Execute(Dagger dagger)
    {
      var lastDatabaseMigration = dagger.From<MigrationHistory>(s => s.OrderByDescending(mh => mh.Id).Limit(1)).First();
      var pendingMigrations = Migrations.Where(m => m.Id > lastDatabaseMigration.Id);
      foreach (var pendingMigration in pendingMigrations)
        pendingMigration.Execute(dagger);
    }
  }
}
