using DaggerNet.Abstract;
using DaggerNet.Linq;
using DaggerNet.Postgres;
using DaggerNet.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Should;
using System;
using System.Diagnostics;
using System.Linq;

namespace DaggerNet.Tests
{
  [TestClass]
  public class SqlBuilderTest
  {
    DataModel _dataModel;
    SqlGenerator _sqlGenerator;

    [TestInitialize]
    public void startup()
    {
      _dataModel = new DataModel(typeof(IEntity));
      _sqlGenerator = new PostgresGenerator();
    }

    protected SqlBuilder<T> BuildSql<T>()
      where T : class
    {
      return new SqlBuilder<T>(_dataModel, _sqlGenerator);
    }

    [TestMethod]
    public void  BaseQueryBuildTest()
    {
      BuildSql<Category>().Where(c => c.Id == 1).Select(c => new { c.Id, c.Title })
        .ToString()
        .ShouldEqual("SELECT \"Id\", \"Title\" FROM \"Categories\" WHERE (\"Id\" = 1)");

      BuildSql<Category>()
        .ToString()
        .ShouldEqual("SELECT * FROM \"Categories\"");
    }

    [TestMethod]
    public void IsNullTest()
    {
      BuildSql<Category>().Where(c => c.CreatedAt == null).Select(c => new { c.Id, c.Title })
        .ToString()
        .ShouldEqual("SELECT \"Id\", \"Title\" FROM \"Categories\" WHERE (\"CreatedAt\" IS NULL)");

      BuildSql<Category>().Where(c => c.CreatedAt != null).Select(c => new { c.Id, c.Title })
        .ToString()
        .ShouldEqual("SELECT \"Id\", \"Title\" FROM \"Categories\" WHERE (\"CreatedAt\" IS NOT NULL)");
    }

    [TestMethod]
    public void DeleteTest()
    {
      var _p = new { Ids = new long[0] };
      BuildSql<Category>().Where(c => _p.Ids.Contains(c.Id)).Delete()
        .ToString()
        .ShouldEqual("DELETE FROM \"Categories\" WHERE (\"Id\" IN @Ids)");
    }

    [TestMethod]
    public void ConditionalQueryBuildTest()
    {
      BuildSql<Category>().Where(c => c.Id == 1 && c.Id != 5).ToString()
        .ShouldEqual("SELECT * FROM \"Categories\" WHERE (\"Id\" = 1 AND \"Id\" <> 5)");


      BuildSql<Category>().Where(c => c.Id == 1 && c.Id != 5).Where(c => !(c.Id < 100))
        .ToString()
        .ShouldEqual("SELECT * FROM \"Categories\" WHERE (\"Id\" = 1 AND \"Id\" <> 5) AND (NOT \"Id\" < 100)");
    }

    [TestMethod]
    public void FunctionQueryBuildTest()
    {
      BuildSql<Product>().Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).ToString()
        .ShouldEqual("SELECT Coalesce(\"ModelNo\", \"Title\") AS \"ModeOrTitle\" FROM \"Products\"");
    }

    [TestMethod]
    public void OrderbyQueryBuildTest()
    {
      BuildSql<Product>().Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).OrderBy(p => p.ModeOrTitle).ToString()
        .ShouldEqual("SELECT Coalesce(\"ModelNo\", \"Title\") AS \"ModeOrTitle\" FROM \"Products\" ORDER BY \"ModeOrTitle\"");
    }

    [TestMethod]
    public void LikeQueryTest()
    {
      BuildSql<Product>().Where(p => p.Title.Contains("_hello_")).ToString()
        .ShouldEqual("SELECT * FROM \"Products\" WHERE (\"Title\" LIKE '%!_hello!_%' ESCAPE '!')");

      var criteria = new { Keyword = SqlFunctions.Like("hello") };
      BuildSql<Product>().Where(p => p.Title.Contains(criteria.Keyword)).ToString()
        .ShouldEqual("SELECT * FROM \"Products\" WHERE (\"Title\" LIKE @Keyword ESCAPE '!')");

      BuildSql<Product>().Where(p => p.Title.StartsWith("hello")).ToString()
        .ShouldEqual("SELECT * FROM \"Products\" WHERE (\"Title\" LIKE 'hello%' ESCAPE '!')");

      BuildSql<Product>().Where(p => p.Title.EndsWith("hello")).ToString()
        .ShouldEqual("SELECT * FROM \"Products\" WHERE (\"Title\" LIKE '%hello' ESCAPE '!')");
    }

    [TestMethod]
    public void JoinQueryTest()
    {
      BuildSql<Item>()
        .Join<Product>()
        .Where((i, p) => p.ModelNo == "abc")
        .Select((i, p) => new { Id = i.Id, ProductModelNo = p.ModelNo })
        .ToString()
        .ShouldEqual("SELECT a.\"Id\" AS \"Id\", b.\"ModelNo\" AS \"ProductModelNo\" FROM \"Items\" AS a JOIN \"Products\" AS b ON (a.\"ProductId\" = b.\"Id\") WHERE (b.\"ModelNo\" = 'abc')");

      var t = BuildSql<Product>()
        .Join<Category>()
        .Where((p, c) => p.Id == 2)
        .Select((p, c) => p);

      Console.WriteLine(t);
    }

    [TestMethod]
    public void AggregateQueryTest()
    {
      BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { total = SqlFunctions.Count(c.Id) })
        .ToString()
        .ShouldEqual("SELECT Count(\"Id\") AS \"total\" FROM \"Categories\" WHERE (\"Id\" > 10)");

      BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { total = SqlFunctions.Avg(c.Id) })
        .ToString()
        .ShouldEqual("SELECT Avg(\"Id\") AS \"total\" FROM \"Categories\" WHERE (\"Id\" > 10)");

      BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { now = SqlFunctions.Now() })
        .ToString()
        .ShouldEqual("SELECT Now() AS \"now\" FROM \"Categories\" WHERE (\"Id\" > 10)");

      BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { round = SqlFunctions.Round(c.Id, 1) })
        .ToString()
        .ShouldEqual("SELECT Round(\"Id\", 1) AS \"round\" FROM \"Categories\" WHERE (\"Id\" > 10)");
    }

    [TestMethod]
    public void GroupByQueryTest()
    {
      BuildSql<Item>().GroupBy(i => i.ModelNo).Select(g => new { count = SqlFunctions.Count(1) }).ToString()
        .ShouldEqual("SELECT Count(1) AS \"count\" FROM \"Items\" GROUP BY \"ModelNo\"");

      BuildSql<Item>().GroupBy(i => i.ModelNo).Having(g => SqlFunctions.Count(1) > 10).Where(i => i.Id > 10).ToString()
        .ShouldEqual("SELECT * FROM \"Items\" GROUP BY \"ModelNo\" HAVING Count(1) > 10 AND (\"Id\" > 10)");

      BuildSql<Item>().Join<Product>().GroupBy((i, p) => i.ModelNo).Having((i, p) => SqlFunctions.Count(p.Id) > 10).ToString()
        .ShouldEqual("SELECT * FROM \"Items\" AS a JOIN \"Products\" AS b ON (a.\"ProductId\" = b.\"Id\") GROUP BY a.\"ModelNo\" HAVING Count(b.\"Id\") > 10");
    }

    [TestMethod]
    public void UpdateQueryTest()
    {
      BuildSql<Product>().Where(p => p.Id == 1).Set(p => p.Stock, p => p.Stock + 2).ToString()
        .ShouldEqual("UPDATE \"Products\" SET \"Stock\" = \"Stock\" + 2 WHERE (\"Id\" = 1)");

      BuildSql<Product>().Where(p => p.Id == 1)
        .Set(p => p.Stock, p => p.Stock + 2)
        .Set(p => p.ModelNo, p => "Test")
        .ToString()
        .ShouldEqual("UPDATE \"Products\" SET \"Stock\" = \"Stock\" + 2, \"ModelNo\" = 'Test' WHERE (\"Id\" = 1)");
    }

    [TestMethod]
    //[TestCategory("Benchmark")]
    public void SqlBuilderBenchmark()
    {
      const int loops = 50000;
      Stopwatch sw = new Stopwatch();
      sw.Start();
      for (var i = 0; i < loops; i++)
      {
        BuildSql<Product>().Where(p => p.Id == 1).Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).OrderBy(p => p.ModeOrTitle);
      }
      sw.Stop();
      Console.WriteLine("{0} x {1}ops spend time:{2}ms", "Select ModelOrTitle", loops, sw.Elapsed.TotalMilliseconds);
    }
  }
}
