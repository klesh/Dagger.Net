using DaggerNet.Postgres;
using DaggerNet.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dapper;
using System.Linq;
using DaggerNet.Migrations;
using System.Threading.Tasks;
using System.Transactions;
using Should;
using System.Diagnostics;

namespace DaggerNet.Tests
{
  [TestClass]
  public class DataFactoryTest : DataFactory<PostgresServer>
  {
    public DataFactoryTest()
      : base (typeof(Entity), DataServerTest.ConnectionStringFormat, "DaggerTest")
    {
    }

    [TestInitialize]
    public void Initialize()
    {
      _dataServer.ReCreate(this._defaultDbName);
      this.Logger = Console.WriteLine;
    }

    [TestMethod]
    public void IntializedTest()
    {
      this.Default.IsInitialized.ShouldBeTrue();
    }

    [TestMethod]
    public void CurdTest()
    {
      // Anomynous type should apply DefaultAttribute
      var demo = new
      {
        Title = "Demo"
      };
      using (var dagger = Draw())
      {
        dagger.Insert<Category>(demo);

        var dbDemo = dagger.Query<Category>("SELECT * FROM \"Categories\"").First();

        Assert.AreEqual("Demo", dbDemo.Title);
        Assert.AreEqual(1, dbDemo.Id);
        Assert.IsNull(dbDemo.ParentId);
        Assert.IsNotNull(dbDemo.CreatedAt);
      }

      // Exact type should be able to assign accurate value
      var now = DateTime.Now.AddDays(-1);

      var bicycle = new Category { Title = "Bicycle", CreatedAt = now };

      using (var dagger = Draw())
      {
        dagger.Insert<Category>(bicycle);

        var dbBicycle = dagger.Query<Category>("SELECT * FROM \"Categories\" ORDER BY \"Id\" DESC LIMIT 1").First();
        Assert.AreEqual("Bicycle", dbBicycle.Title);
        Assert.IsTrue(dbBicycle.Id > 0);
        Assert.AreEqual(now.ToString(), dbBicycle.CreatedAt.Value.ToString());
        Assert.AreEqual(bicycle.Id, dbBicycle.Id);

        var mtb = new Category { ParentId = dbBicycle.Id, Title = "MTB" };
        dagger.Insert<Category>(mtb);

        var dbMtb = dagger.Query<Category>("SELECT * FROM \"Categories\" ORDER BY \"Id\" DESC LIMIT 1").First();
        Assert.AreEqual("MTB", dbMtb.Title);
        Assert.AreEqual(dbBicycle.Id, dbMtb.ParentId.Value);

        // update test
        dbBicycle.Title = "Bicycle2";
        dagger.Update<Category>(new { dbBicycle.Id, Title = "Bicycle2" });
        var title = dagger.ExecuteScalar<string>("SELECT \"Title\" FROM \"Categories\" WHERE \"Id\"=@Id", dbBicycle);
        Assert.AreEqual("Bicycle2", title);

        // Cascacde delete test
        dagger.Delete<Category>(bicycle);
        var total = dagger.ExecuteScalar<long>("SELECT COUNT(*) FROM \"Categories\"");
        Assert.AreEqual(1, total);
      }
    }

    [TestMethod]
    public void SqlLogTest()
    {
      var oldLog = this.Default.Logger;
      string log = null;
      this.Default.Logger = s => log = s;
      var sql = "SELECT 1";
      using (var dagger = Draw())
      {
        dagger.ExecuteNonQuery(sql);
      }
      Assert.AreEqual(sql, log);
      this.Default.Logger = oldLog;
    }

    [TestMethod]
    public async Task AsyncTest()
    {
      using (var dagger = Draw())
      {
        var mhs = await dagger.From<MigrationHistory>(s => s.Limit(1)).GetAllAsync();
        Assert.IsTrue(mhs.Count() > 0);
      }
    }

    [TestMethod]
    public void TransactionTest()
    {
      using (var dagger = Draw())
      {
        var oldCategoryCount = dagger.ExecuteScalar<long>("SELECT COUNT(*) FROM \"Categories\"");
        var category = new Category { Title = "Transaction Category" };

        // rollback test
        using (var tx = new TransactionScope())
        {
          using (var dagger1 = Draw())
          {
            dagger1.Insert<Category>(category);
          }
          category.Id
            .ShouldBeGreaterThan(0);
        }

        dagger.ExecuteScalar<long>("SELECT COUNT(*) FROM \"Categories\"")
          .ShouldEqual(oldCategoryCount);

        // commit test
        using (var tx = new TransactionScope())
        {
          using (var dagger1 = Draw())
          {
            dagger1.Insert<Category>(category);
          }
          category.Id
            .ShouldBeGreaterThan(0);
          tx.Complete();
        }
        dagger.ExecuteScalar<long>("SELECT COUNT(*) FROM \"Categories\"")
         .ShouldEqual(oldCategoryCount + 1);
      }
    }

    [TestMethod]
    public async Task FluentQueryAsyncTest()
    {
      var category = new Category { Title = "Async Category" };
      using (var dagger = this.Draw())
      {
        await dagger.InsertAsync<Category>(category);
        var product = new Product { Title = "Async Product", ModelNo = "AsyncProduct", CategoryId = category.Id, Content = "Async Content" };
        await dagger.InsertAsync<Product>(product);
        var result = await dagger.By(new { Id = product.Id })
          .From<Product>((build, param) => build.Where(p => p.Id == param.Id))
          .As<ProductDto>()
          .FirstOrDefaultAsync();
        result.ShouldNotBeNull();
        result.ModelNo.ShouldEqual("AsyncProduct");
      }
    }

    [TestMethod]
    public void FluentQueryTest()
    {
      var category = new Category { Title = "Sync Category" };
      using (var dagger = this.Draw())
      {
        dagger.Insert<Category>(category);
        var product = new Product { Title = "Sync Product", ModelNo = "SyncProduct", CategoryId = category.Id, Content = "Sync Content" };
        dagger.Insert<Product>(product);
        var result = dagger.By(new { Id = product.Id })
          .From<Product>((build, param) => build.Where(p => p.Id == param.Id))
          .As<ProductDto>()
          .FirstOrDefault();
        result.ShouldNotBeNull();
        result.ModelNo.ShouldEqual("SyncProduct");
      }
    }

    [TestMethod]
    public void UpdateCellTest()
    {
      var tmp = this.Default;
      Stopwatch sw = new Stopwatch();
      var category = new Category { Title = "Update Cell Test" };
      using (var dagger = this.Draw())
      {
        sw.Start();
        dagger.Insert<Category>(category);
        sw.Stop();
        Console.WriteLine("Insert Cost: {0}", sw.Elapsed.TotalMilliseconds);

        sw.Restart();
        dagger.UpdateCell("Title", "Update Cell Updated", "Category", category.Id.ToString())
          .ShouldEqual(1);
        sw.Stop();
        Console.WriteLine("Update cell Cost: {0}", sw.Elapsed.TotalMilliseconds);

        sw.Restart();
        var dbCategory2 = dagger.Query<Category>("SELECT * FROM \"Categories\" WHERE \"Id\" = @Id", category).First();
        sw.Stop();
        Console.WriteLine("Pure Select Cost: {0}", sw.Elapsed.TotalMilliseconds);

        sw.Restart();
        dagger.By(category)
          .From<Category>((_b, _p) => _b.Where(c => c.Id == _p.Id))
          .FirstOrDefault()
          .ShouldNotBeNull()
          .Title.ShouldEqual("Update Cell Updated");
        sw.Stop();
        Console.WriteLine("Select Cost: {0}", sw.Elapsed.TotalMilliseconds);


        sw.Restart();
        var dbCategory3 = dagger.Query<Category>("SELECT * FROM \"Categories\" WHERE \"Id\" = @Id", category).First();
        sw.Stop();
        Console.WriteLine("Pure Select 2 Cost: {0}", sw.Elapsed.TotalMilliseconds);

        sw.Restart();
        dagger.By(category)
          .From<Category>((_b, _p) => _b.Where(c => c.Id == _p.Id))
          .FirstOrDefault()
          .ShouldNotBeNull()
          .Title.ShouldEqual("Update Cell Updated");
        sw.Stop();
        Console.WriteLine("Select 2 Cost: {0}", sw.Elapsed.TotalMilliseconds);
      }
    }

    [TestMethod]
    public void ListSupportTest()
    {
      using (var dagger = this.Draw())
      {
        var c1 = new Category { Title = "C1" };
        var c2 = new Category { Title = "C2" };
        var c3 = new Category { Title = "C3" };

        dagger.Insert<Category>(c1);
        dagger.Insert<Category>(c2);
        dagger.Insert<Category>(c3);

        c1.Id.ShouldBeGreaterThan(0);
        c2.Id.ShouldBeGreaterThan(0);
        c3.Id.ShouldBeGreaterThan(0);

        //var categories = dagger.By(new { Ids = new long[] { c1.Id, c2.Id, c3.Id } })
        //  .From<Category>((_b, _p) => _b.Where(c => _p.Ids.Contains(c.Id)))
        //  .GetAll();

        //categories.Count()
        //  .ShouldEqual(3);

        var categories = dagger.Query<Category>(
          "select * from (select 1 as Id union all select 2 union all select 3) as X where Id in @Ids", 
          new { Ids = new int[] { 1, 2, 3 } }
          );
      }
    }
  }
}
