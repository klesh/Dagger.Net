using Dagger.Net;
using Dagger.Net.PostgresSQL;
using Emmola.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dagger.NetTest.Models;
using Npgsql;
using Dapper;
using Dagger.Net.Migrations;
using System.Linq;
using System.Collections.Generic;
using Dagger.Net.DOM;
using System.Data.Common;

namespace Dagger.NetTest
{
  [TestClass]
  public class DbServerTest
  {
    PostgreSQLGenerator generator;
    DbServer dbServer;

    [TestInitialize]
    public void Setup()
    {
      var entityTypes = typeof(Entity).FindSubTypesInMe();
      generator = new PostgreSQLGenerator();
      dbServer = new DbServer(entityTypes, Connect(), generator);
    }

    public DbConnection Connect(string dbName = null)
    {
      var connection =  new NpgsqlConnection("Server=localhost;Port=5432;Database=postgres;User Id=super;Password=info123;Pooling=true;MinPoolSize=10;MaxPoolSize=100;Protocol=3;");
      if (dbName.IsValid())
        connection.ChangeDatabase(dbName);
      return connection;
    }

    [TestMethod]
    public void CreateDatabase()
    {
      const string dbname = "UnitTest";
      if (dbServer.Exists(dbname))
        dbServer.Drop(dbname);

      dbServer.Create(dbname);
      Assert.IsTrue(dbServer.Exists(dbname));

      List<MigrationHistory> migrations;
      using (var dagger = dbServer.Wrap(Connect(dbname)))
      {
        migrations = dagger.Query<MigrationHistory>("SELECT * FROM {0}".FormatMe(generator.Quote("MigrationHistories"))).ToList();
      }
      Assert.AreEqual(1, migrations.Count);
      var initial = migrations.First();
      Assert.IsNotNull(initial.Model);
      Assert.IsTrue(initial.Model.Length > 0);
    }

    [TestMethod]
    public void SqlLogTest()
    {
      const string sqlQuery = "SELECT * FROM \"Categories\"";
      var log = "";
      dbServer.Logger = (sql) => log = sql;
      using (var dagger = dbServer.Wrap(Connect("UnitTest")))
      {
        dagger.Query(sqlQuery);
      }
      Assert.AreEqual(sqlQuery, log);
    }
  }
}
