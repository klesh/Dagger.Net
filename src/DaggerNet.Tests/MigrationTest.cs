using DaggerNet.Abstract;
using DaggerNet.Postgres;
using Emmola.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using v1 = DaggerNet.Tests.Models.v1;
using v2 = DaggerNet.Tests.Models.v2;
using v3 = DaggerNet.Tests.Models.v3;
using Should;
using DaggerNet.Tests.Migrations;

namespace DaggerNet.Tests
{
  [TestClass]
  public class MigrationTest
  {
    /// <summary>
    /// Make sure SQL SCRIPT come out as expected
    /// </summary>
    [TestMethod]
    public void MigrationDownScriptTest()
    {
      //var dom1 = new Class2Dom(typeof(v1.Post)).Produce();
      var dom2 = new Class2Dom(typeof(v2.Entity).FindSubTypesInMe()).Produce();
      var dom3 = new Class2Dom(typeof(v3.IEntity).FindSubTypesInMe()).Produce();

      var generator = new PostgresGenerator();

      //Console.WriteLine(generator.CreateMigration(dom1, dom2));
      var sql = generator.CreateMigration(dom3, dom2);
      Console.WriteLine("Sql Script:");
      Console.WriteLine(sql);

      var scripts = sql.Split(new string[] { SqlGenerator.DELETION_SEPARATOR }, StringSplitOptions.None);

      var creation_part = scripts[0].Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
      var deletion_part = scripts[1].Trim().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

      //Assert.AreEqual(3, creation_part.Length);
      creation_part.ShouldContain("ALTER TABLE \"Posts\" ALTER COLUMN \"Title\" SET DEFAULT 'test';");
      creation_part.ShouldContain("ALTER TABLE \"Posts\" RENAME COLUMN \"Content1\" TO \"Content\";");
      creation_part.ShouldContain("ALTER INDEX \"IX_Post_Content1\" RENAME TO \"IX_Post_Content\";");

      Assert.AreEqual(3, deletion_part.Length);
      deletion_part.ShouldContain("ALTER TABLE \"Posts\" DROP COLUMN \"CreatedAt\";");
      deletion_part.ShouldContain("DROP TABLE \"Users\";");
    }

    [TestMethod]
    public void MigrationUpScriptTest()
    {
      var dom1 = new Class2Dom(typeof(v1.IEntity).FindSubTypesInMe()).Produce();
      var dom2 = new Class2Dom(typeof(v2.Entity).FindSubTypesInMe()).Produce();
      var dom3 = new Class2Dom(typeof(v3.IEntity).FindSubTypesInMe()).Produce();

      var post_v1 = dom1.First(t => t.Name == "Post");

      var generator = new PostgresGenerator();

      //Console.WriteLine(generator.CreateMigration(dom1, dom2));
      var sql1to2 = generator.CreateMigration(dom1, dom2);
      Console.WriteLine("V1 to V2 Sql Script:");
      Console.WriteLine(sql1to2);
      sql1to2.ShouldContain("ALTER TABLE \"Posts\" ADD CONSTRAINT \"FK_Post_BlogId\" FOREIGN KEY (\"BlogId\") REFERENCES \"Blogs\" (\"Id\") ON DELETE CASCADE;");
      sql1to2.ShouldContain("ALTER TABLE \"Posts\" ADD CONSTRAINT \"PK_Post\" PRIMARY KEY (\"Id\");");


      var sql2to3 = generator.CreateMigration(dom2, dom3);
      Console.WriteLine("V2 to V3 Sql Script:");
      Console.WriteLine(sql2to3);
    }

    [TestMethod]
    public void MigrateTest()
    {
      var dom1 = new Class2Dom(typeof(v1.Post)).Produce();
      var dom2 = new Class2Dom(typeof(v2.Entity).FindSubTypesInMe()).Produce();

      var postgresServer = new PostgresServer();
      postgresServer.ConnectionStringFormat = DataServerTest.ConnectionStringFormat;
      var initialScript = postgresServer.Sql.CreateTables(dom1);
      var migrationScript = postgresServer.Sql.CreateMigration(dom1, dom2);
      Console.WriteLine("Initial Script:");
      Console.WriteLine(initialScript);

      Console.WriteLine("**************************************************************");
      Console.WriteLine("Migration Scripts:");
      Console.WriteLine(migrationScript);


      postgresServer.ReCreate("DaggerMigrate");

      using (var cnt = postgresServer.Open("DaggerMigrate"))
      {
        var cmd = cnt.CreateCommand();
        cmd.CommandText = initialScript;
        cmd.ExecuteNonQuery();

        cmd.CommandText = "select count(*) from information_schema.tables where table_schema='public';";
        cmd.ExecuteScalar()
          .ShouldEqual(1L);

        cmd.CommandText = migrationScript;
        cmd.ExecuteNonQuery();

        cmd.CommandText = "select count(*) from information_schema.tables where table_schema='public';";
        cmd.ExecuteScalar()
          .ShouldEqual(2L);
      }
    }

    [TestMethod]
    public void MockMigrationTest()
    {
      var mockMigration = new MockMigration();
      Console.WriteLine(mockMigration.GetSqlName());
      var parts = mockMigration.GetSqlScripts();
      Console.WriteLine(parts[0]);
      Console.WriteLine(parts[1]);
    }
  }
}
