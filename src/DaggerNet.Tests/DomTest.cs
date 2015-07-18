using DaggerNet.DOM.Abstract;
using Emmola.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using DaggerNet.Tests.Models;
using v1 = DaggerNet.Tests.Models.v1;
using v2 = DaggerNet.Tests.Models.v2;
using v3 = DaggerNet.Tests.Models.v3;

namespace DaggerNet.Tests
{
  [TestClass]
  public class DomTest
  {
    /// <summary>
    /// Ensure dom created correctly
    /// </summary>
    [TestMethod]
    public void CreateDomFromTypes()
    {
      var c2d = new Class2Dom(typeof(Entity).FindSubTypesInIt());
      var dom = c2d.Produce();
      var category = dom.First(t => t.Name == "Category");
      var pks = category.PrimaryKey.Columns;

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

    /// <summary>
    /// Make dom diff result as expected
    /// </summary>
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
      Assert.AreEqual(1, diff3.Comparison.Count);
      Assert.AreEqual(0, diff3.Deletion.Count());

      var diff4 = dom3.Diff(dom2);
      Assert.AreEqual(0, diff4.Creation.Count());
      Assert.AreEqual(1, diff4.Comparison.Count);
      Assert.AreEqual(1, diff4.Deletion.Count());
    }
  }
}
