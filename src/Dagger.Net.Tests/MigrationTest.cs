using Dagger.Net;
using Dagger.Net.DOM.Abstract;
using Dagger.Net.PostgresSQL;
using Emmola.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dagger.NetTest.Models;
using System;
using System.Linq;
using v1 = Dagger.NetTest.MigrationModels.v1;
using v2 = Dagger.NetTest.MigrationModels.v2;
using v3 = Dagger.NetTest.MigrationModels.v3;

namespace Dagger.NetTest
{
  [TestClass]
  public class MigrationTest
  {
    [TestMethod]
    public void CreateDomFromTypes()
    {
      var c2d = new Class2Dom(typeof(Entity).FindSubTypesInMe());
      var dom = c2d.Produce();
      var category = dom.First(t => t.Name == "Category");
      var pks = category.PrimaryKey.Columns;

      var generator = new PostgreSQLGenerator();
      Console.WriteLine(generator.CreateTables(dom));

      Assert.AreEqual(1, pks.Count);
      Assert.AreEqual(1, category.ForeignKeys.Count);

      var pk = pks.First().Value;
      Assert.AreEqual("Id", pk.Name);
      Assert.AreEqual(SqlType.Int64, pk.DataType);
      Assert.IsTrue(pk.NotNull);
      Assert.IsTrue(pk.AutoIncrement);

      var productProperties = dom.FirstOrDefault(t => t.Name == "ProductProperty");
      Assert.IsNotNull(productProperties, "Many to Many table missed");
      Assert.AreEqual(2, productProperties.Columns.Count);
      Assert.AreEqual(2, productProperties.PrimaryKey.Columns.Count);
      Assert.AreEqual(2, productProperties.ForeignKeys.Count);
    }

    [TestMethod]
    public void DomDiffTest()
    {
      var dom1 = new Class2Dom(typeof(v1.Post)).Produce();
      var dom2 = new Class2Dom(typeof(v2.Entity).FindSubTypesInMe()).Produce();
      var dom3 = new Class2Dom(typeof(v3.Entity).FindSubTypesInMe()).Produce();

      var diff = dom1.Diff(dom2);
      Assert.AreEqual(1, diff.Creation.Count());
      Assert.AreEqual(1, diff.Comparison.Count);
      Assert.AreEqual(0, diff.Deletion.Count());

      var diff2 = dom2.Diff(dom1);
      Assert.AreEqual(0, diff2.Creation.Count());
      Assert.AreEqual(1, diff2.Comparison.Count);
      Assert.AreEqual(1, diff2.Deletion.Count());

      var diff3 = dom2.Diff(dom3);
      Assert.AreEqual(1, diff3.Creation.Count());
      Assert.AreEqual(0, diff3.Comparison.Count);
      Assert.AreEqual(0, diff3.Deletion.Count());

      var diff4 = dom3.Diff(dom2);
      Assert.AreEqual(0, diff4.Creation.Count());
      Assert.AreEqual(0, diff4.Comparison.Count);
      Assert.AreEqual(1, diff4.Deletion.Count());
    }

    [TestMethod]
    public void MigrationScriptTest()
    {
      var dom1 = new Class2Dom(typeof(v1.Post)).Produce();
      var dom2 = new Class2Dom(typeof(v2.Entity).FindSubTypesInMe()).Produce();
      var dom3 = new Class2Dom(typeof(v3.Entity).FindSubTypesInMe()).Produce();

      var generator = new PostgreSQLGenerator();

      //Console.WriteLine(generator.CreateMigration(dom1, dom2));
      Console.WriteLine(generator.CreateMigration(dom3, dom2));
    }
  }
}
