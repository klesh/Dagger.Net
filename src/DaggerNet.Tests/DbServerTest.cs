using DaggerNet;
using DaggerNet.PostgresSQL;
using Emmola.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaggerNetTest.Models;
using Npgsql;
using Dapper;
using DaggerNet.Migrations;
using System.Linq;
using System.Collections.Generic;
using DaggerNet.DOM;
using System.Data.Common;

namespace DaggerNetTest
{
  [TestClass]
  public class DbServerTest
  {
    DbServer dbServer;
    const string DB_NAME = "UnitTest";

    [TestInitialize]
    public void Setup()
    {
      var entityTypes = typeof(Entity).FindSubTypesInMe();
      var cntStrFormat = "Server=localhost;Port=5432;Database={0};User Id=super;Password=info123;Pooling=true;MinPoolSize=10;MaxPoolSize=100;Protocol=3;";
      dbServer = new PostgresSQLServer(entityTypes, cntStrFormat, DB_NAME);
    }

    [TestMethod]
    public void CreateDatabase()
    {
      if (dbServer.Exists(DB_NAME))
        dbServer.Drop(DB_NAME);

      dbServer.Create(DB_NAME);
      Assert.IsTrue(dbServer.Exists(DB_NAME));

      List<MigrationHistory> migrations;
      using (var dagger = dbServer.Draw(DB_NAME))
      {
        migrations = dagger.Query<MigrationHistory>("SELECT * FROM {0}".FormatMe(dbServer.Generator.Quote("MigrationHistories")))
          .ToList();
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
      using (var dagger = dbServer.Draw(DB_NAME))
      {
        dagger.Query(sqlQuery);
      }
      Assert.AreEqual(sqlQuery, log);
    }
  }
}
