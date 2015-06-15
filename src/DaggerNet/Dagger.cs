using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DaggerNet.Linq;

namespace DaggerNet
{
  public class Dagger : IDbConnection
  {
    private DaggerFactory _factory;
    private IDbConnection _connection;
    private Action<string> _logger;

    public Dagger(DaggerFactory factory, IDbConnection connection, Action<string> logger = null)
    {
      _factory = factory; // type -> table relation
      _connection = connection;
      _logger = logger;
    }

    ///// <summary>
    ///// Insert and renew the entity autoincremnt primarykey
    ///// Partial insert by anomynous type
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="entity"></param>
    ///// <returns></returns>
    public int Insert<T>(object entity)
    {
      return _factory.Insert<T>(entity, _connection);
    }

    ///// <summary>
    ///// Update record and take care the UpdateTimeAttribute
    ///// Partial update by anomynous type
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="entity"></param>
    ///// <returns></returns>
    //public int Update<T>(object entity)
    //{
    //  throw new NotImplementedException();
    //}

    ///// <summary>
    ///// Build up Many-To-Many relationship.
    ///// </summary>
    ///// <typeparam name="T1"></typeparam>
    ///// <typeparam name="T2"></typeparam>
    ///// <param name="entity1"></param>
    ///// <param name="entity2"></param>
    ///// <returns></returns>
    //public int Relate<T1, T2>(object entity1, object entity2)
    //{
    //  throw new NotImplementedException();
    //}

    ///// <summary>
    ///// Delete by keys
    ///// </summary>
    ///// <typeparam name="T"></typeparam>
    ///// <param name="keys"></param>
    ///// <returns></returns>
    //public int Delete<T>(params object[] keys)
    //{
    //  throw new NotImplementedException();
    //}

    ///// <summary>
    ///// Cache sqlBuilder's result sql by criteria NULLABLE properties status.
    ///// </summary>
    ///// <typeparam name="T">Mapped Type</typeparam>
    ///// <typeparam name="TR">You may want a smaller type to be returned in MOST CASE</typeparam>
    ///// <typeparam name="TC">Criteria Type</typeparam>
    ///// <param name="criteria">Parameters of sql</param>
    ///// <param name="sql"></param>
    ///// <returns></returns>
    //public Paged<TR> FastPage<T, TR, TC>(string sql, object criteria)
    //  where T : class
    //  where TR : class, new()
    //  where TC : class
    //{
    //  throw new NotImplementedException();
    //}

    public void Dispose()
    {
      _connection.Dispose();
      _connection = null;
    }

    public IDbTransaction BeginTransaction(IsolationLevel il)
    {
      return _connection.BeginTransaction(il);
    }

    public IDbTransaction BeginTransaction()
    {
      return _connection.BeginTransaction();
    }

    public void ChangeDatabase(string databaseName)
    {
      _connection.ChangeDatabase(databaseName);
    }

    public void Close()
    {
      _connection.Close();
    }

    public string ConnectionString
    {
      get
      {
        return _connection.ConnectionString;
      }
      set
      {
        _connection.ConnectionString = value;
      }
    }

    public int ConnectionTimeout
    {
      get { return _connection.ConnectionTimeout; }
    }

    public IDbCommand CreateCommand()
    {
      return new Sheath(_connection.CreateCommand(), _logger);
    }

    public string Database
    {
      get { return _connection.Database; }
    }

    public void Open()
    {
      _connection.Open();
    }

    public ConnectionState State
    {
      get { return _connection.State; }
    }

    public IEnumerable<T> From<T>(Func<SqlBuilder<T>, string> buildSql, object param = null)
      where T : class, new()
    {
      return this.Query<T>(buildSql(_factory.Server.BuildSql<T>()), param);
    }
  }
}
