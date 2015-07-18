using DaggerNet.Postgres;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Should;

namespace DaggerNet.Tests
{
  [TestClass]
  public class DataServerTest
  {
    internal const string ConnectionStringFormat = "Server=localhost;Port=5432;Database={0};User Id=postgres;Password=info123;Pooling=true;MinPoolSize=10;MaxPoolSize=100;Protocol=3;Enlist=true";

    const string DB_NAME = "UnitTest";

    DataServer _dataServer;

    [TestInitialize]
    public void Setup()
    {
      _dataServer = new PostgresServer();
      _dataServer.ConnectionStringFormat = ConnectionStringFormat;
    }

    [TestMethod]
    public void CreateDropTest()
    {
      if (_dataServer.Exists(DB_NAME))
        _dataServer.Drop(DB_NAME);

      _dataServer.Exists(DB_NAME)
        .ShouldBeFalse();

      _dataServer.Create(DB_NAME);
      _dataServer.Exists(DB_NAME)
        .ShouldBeTrue();

      _dataServer.Drop(DB_NAME);
      _dataServer.Exists(DB_NAME)
        .ShouldBeFalse();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void IllegalDatabaseNameTest()
    {
      _dataServer.Open("'maybe sql injection");
    }
  }
}
