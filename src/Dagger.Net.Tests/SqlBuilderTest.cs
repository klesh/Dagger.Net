using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Emmola.Helpers;
using Dagger.NetTest.Models;
using Dagger.Net.PostgresSQL;
using Dagger.Net;
using Dagger.Net.Linq;
using System.Diagnostics;
using System.Linq;

namespace Dagger.NetTest
{
  [TestClass]
  public class SqlBuilderTest
  {
    DbServer dbServer;

    [TestInitialize]
    public void startup()
    {
      var entityTypes = typeof(Entity).FindSubTypesInMe();
      var generator = new PostgreSQLGenerator();
      dbServer = new DbServer(entityTypes, null, generator);
    }

    [TestMethod]
    public void  BaseQueryBuildTest()
    {
      var selectFields = "SELECT \"Id\", \"Title\" FROM \"Categories\" WHERE (\"Id\" = 1)";
      Assert.AreEqual(selectFields, dbServer.BuildSql<Category>().Where(c => c.Id == 1).Select(c => new { c.Id, c.Title }).ToString());
      var selectAll = "SELECT * FROM {0}".FormatWithQuote("Categories");
      Assert.AreEqual(selectAll, dbServer.BuildSql<Category>().ToString());

    }

    [TestMethod]
    public void ConditionalQueryBuildTest()
    {

      var selectCondition = "SELECT * FROM {0} WHERE ({1} = 1 AND {1} <> 5)".FormatWithQuote("Categories", "Id");
      var selectConditionBuild = dbServer.BuildSql<Category>().Where(c => c.Id == 1 && c.Id != 5).ToString();
      Assert.AreEqual(selectCondition, selectConditionBuild);

      var selectConditions = "SELECT * FROM {0} WHERE ({1} = 1 AND {1} <> 5) AND (NOT {1} < 100)".FormatWithQuote("Categories", "Id");
      var selectConditionsBuilder = dbServer.BuildSql<Category>().Where(c => c.Id == 1 && c.Id != 5).Where(c => !(c.Id < 100));
      Assert.AreEqual(selectConditions, selectConditionsBuilder.ToString());
    }

    [TestMethod]
    public void FunctionQueryBuildTest()
    {
      var selectAsWithCoalesce = "SELECT Coalesce({0}, {1}) AS {2} FROM {3}".FormatWithQuote("ModelNo", "Title", "ModeOrTitle", "Products");
      Assert.AreEqual(selectAsWithCoalesce, dbServer.BuildSql<Product>().Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).ToString());
    }

    [TestMethod]
    public void OrderbyQueryBuildTest()
    {
      var selectOrderBy = "SELECT Coalesce({0}, {1}) AS {2} FROM {3} ORDER BY {2}".FormatWithQuote("ModelNo", "Title", "ModeOrTitle", "Products");
      Assert.AreEqual(selectOrderBy, dbServer.BuildSql<Product>().Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).OrderBy(p => p.ModeOrTitle).ToString());
    }

    [TestMethod]
    public void LongPageQueryTest()
    {
      var selectPagination = "SELECT * FROM {0} WHERE ({1} > 5) ORDER BY {1} DESC OFFSET 40 LIMIT 10".FormatWithQuote("Products", "Id");
      var hasNext = "SELECT EXISTS(SELECT * FROM {0} WHERE ({1} > 5) AND {1} < 20)".FormatWithQuote("Products", "Id");
      var tmp = dbServer.BuildSql<Product>().Where(p => p.Id > 5).LongPage(10, 5);
      Assert.AreEqual(selectPagination, tmp.ToString());
      Assert.AreEqual(hasNext, tmp.GetHasNextSql(20));
    }

    [TestMethod]
    public void LikeQueryTest()
    {
      var containsConst = "SELECT * FROM {0} WHERE ({1} LIKE '%!_hello!_%' ESCAPE '!')".FormatWithQuote("Products", "Title");
      Assert.AreEqual(containsConst, dbServer.BuildSql<Product>().Where(p => p.Title.Contains("_hello_")).ToString());

      var criteria = new { Keyword = Sql.Like("hello") };
      var likeExp = "SELECT * FROM {0} WHERE ({1} LIKE @Keyword ESCAPE '!')".FormatWithQuote("Products", "Title");
      Assert.AreEqual(likeExp, dbServer.BuildSql<Product>().Where(p => p.Title.Contains(criteria.Keyword)).ToString());

      var startsWith = "SELECT * FROM {0} WHERE ({1} LIKE 'hello%' ESCAPE '!')".FormatWithQuote("Products", "Title");
      Assert.AreEqual(startsWith, dbServer.BuildSql<Product>().Where(p => p.Title.StartsWith("hello")).ToString());
      var endsWith = "SELECT * FROM {0} WHERE ({1} LIKE '%hello' ESCAPE '!')".FormatWithQuote("Products", "Title");
      Assert.AreEqual(endsWith, dbServer.BuildSql<Product>().Where(p => p.Title.EndsWith("hello")).ToString());
    }

    [TestMethod]
    public void JoinQueryTest()
    {
      var selectByProductModel = "SELECT a.{3} AS {3}, b.{7} AS {6} FROM {0} AS a JOIN {1} AS b ON (a.{2} = b.{3}) WHERE (b.{7} = 'abc')"
        .FormatWithQuote("Items", "Products", "ProductId", "Id", "Id", "ModelNo", "ProductModelNo", "ModelNo");
      var selectByProductModelBuilder = dbServer.BuildSql<Item>()
        .Join<Product>()
        .Where((i, p) => p.ModelNo == "abc")
        .Select((i, p) => new { Id = i.Id, ProductModelNo = p.ModelNo })
        ;
      Assert.AreEqual(selectByProductModel, selectByProductModelBuilder.ToString());

      var m2mJoinSql = "SELECT * FROM \"Products\" AS a JOIN \"ProductProperties\" AS ab ON (ab.\"ProductId\" = a.\"Id\") JOIN \"Properties\" AS b ON (ab.\"PropertyId\" = b.\"Id\")";
      var m2mJoinQuery = dbServer.BuildSql<Product>().Join<Property>();
      Assert.AreEqual(m2mJoinSql, m2mJoinQuery.ToString());
    }

    [TestMethod]
    public void AggregateQueryTest()
    {
      var countQuery = dbServer.BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { total = Sql.Count(c.Id) });
      Assert.AreEqual("SELECT Count(\"Id\") AS \"total\" FROM \"Categories\" WHERE (\"Id\" > 10)", countQuery.ToString());

      var avgQuery = dbServer.BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { total = Sql.Avg(c.Id) });
      Assert.AreEqual("SELECT Avg(\"Id\") AS \"total\" FROM \"Categories\" WHERE (\"Id\" > 10)", avgQuery.ToString());

      var nowQuery = dbServer.BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { now = Sql.Now() });
      Assert.AreEqual("SELECT Now() AS \"now\" FROM \"Categories\" WHERE (\"Id\" > 10)", nowQuery.ToString());

      var roundQuery = dbServer.BuildSql<Category>().Where(c => c.Id > 10).Select(c => new { round = Sql.Round(c.Id, 1) });
      Assert.AreEqual("SELECT Round(\"Id\", 1) AS \"round\" FROM \"Categories\" WHERE (\"Id\" > 10)", roundQuery.ToString());
    }

    [TestMethod]
    public void GroupByQueryTest()
    {
      Assert.AreEqual(
        "SELECT Count(1) AS \"count\" FROM \"Items\" GROUP BY \"ModelNo\"",
        dbServer.BuildSql<Item>().GroupBy(i => i.ModelNo).Select(g => new { count = Sql.Count(1) }).ToString()
      );
      Assert.AreEqual(
        "SELECT * FROM \"Items\" GROUP BY \"ModelNo\" HAVING Count(1) > 10 AND (\"Id\" > 10)",
        dbServer.BuildSql<Item>().GroupBy(i => i.ModelNo).Having(g => Sql.Count(1) > 10).Where(i => i.Id > 10).ToString()
      );
      Assert.AreEqual(
        "SELECT * FROM \"Items\" AS a JOIN \"Products\" AS b ON (a.\"ProductId\" = b.\"Id\") GROUP BY a.\"ModelNo\" HAVING Count(b.\"Id\") > 10",
        dbServer.BuildSql<Item>().Join<Product>().GroupBy((i, p) => i.ModelNo).Having((i, p) => Sql.Count(p.Id) > 10).ToString()
      );
    }

    //[TestMethod]
    //[TestCategory("Benchmark")]
    public void SqlBuilderBenchmark()
    {
      var entityTypes = typeof(Entity).FindSubTypesInMe();
      var generator = new PostgreSQLGenerator();
      var dbServer = new DbServer(entityTypes, null, generator);

      const int loops = 50000;
      Stopwatch sw = new Stopwatch();
      sw.Start();
      for (var i = 0; i < loops; i++)
      {
        dbServer.BuildSql<Product>().Select(p => new { ModeOrTitle = p.ModelNo ?? p.Title }).OrderBy(p => p.ModeOrTitle);
      }
      sw.Stop();
      Console.WriteLine("{0} x {1}ops spend time:{2}ms", "Select ModelOrTitle", loops, sw.Elapsed.TotalMilliseconds);
    }
  }

  public static class Helper
  {
    public static string Quote(this string self)
    {
      return "\"" + self +  "\"";
    }

    public static string FormatWithQuote(this string self, params object[] args)
    {
      return self.FormatMe(args.Select(a => a.ToString().Quote()).ToArray());
    }
  }
}
