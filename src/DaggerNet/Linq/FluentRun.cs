using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DaggerNet.Linq
{
  public class FluentRoot<TParam>
    where TParam : class
  {
    public Dagger Dagger { get; set; }
    public TParam Parameters { get; set; }

    public FluentRoot(Dagger dagger, TParam parameters)
    {
      Dagger = dagger;
      Parameters = parameters;
    }

    public FluentFrom<TParam, TTable> From<TTable>(Func<SqlBuilder<TTable>, TParam, object> buildSql)
      where TTable : class
    {
      return new FluentFrom<TParam, TTable>(this, buildSql);
    }
  }

  public class FluentFrom<TParam, TTable, TReturn>
    where TParam : class
    where TTable : class
    where TReturn : class
  {
    protected FluentRoot<TParam> _root;
    protected Func<SqlBuilder<TTable>, TParam, object> _buildSql;

    public FluentFrom(FluentRoot<TParam> root, Func<SqlBuilder<TTable>, TParam, object> buildSql)
    {
      _root = root;
      _buildSql = buildSql;
    }

    public string BuildSql()
    {
      return _buildSql(new SqlBuilder<TTable>(_root.Dagger.Base.Model, _root.Dagger.Base.Sql), _root.Parameters).ToString();
    }

    public TReturn FirstOrDefault()
    {
      return _root.Dagger.Query<TReturn>(BuildSql(), _root.Parameters).FirstOrDefault();
    }

    public async Task<TReturn> FirstOrDefaultAsync()
    {
      var returns = await _root.Dagger.QueryAsync<TReturn>(BuildSql(), _root.Parameters);
      return returns.FirstOrDefault();
    }

    public IEnumerable<TReturn> GetAll()
    {
      return _root.Dagger.Query<TReturn>(BuildSql(), _root.Parameters);
    }

    public async Task<IEnumerable<TReturn>> GetAllAsync()
    {
      return await _root.Dagger.QueryAsync<TReturn>(BuildSql(), _root.Parameters);
    }

    public int Execute()
    {
      return _root.Dagger.Execute(BuildSql(), _root.Parameters);
    }

    public async Task<int> ExecuteAsync()
    {
      return await _root.Dagger.ExecuteAsync(BuildSql(), _root.Parameters);
    }

    public T ExecuteScalar<T>()
    {
      return _root.Dagger.ExecuteScalar<T>(BuildSql(), _root.Parameters);
    }

    public async Task<T> ExecuteScalarAsync<T>()
    {
      return await _root.Dagger.ExecuteScalarAsync<T>(BuildSql(), _root.Parameters);
    }
  }

  public class FluentFrom<TParam, TTable> : FluentFrom<TParam, TTable, TTable>
    where TParam : class
    where TTable : class
  {
    public FluentFrom(FluentRoot<TParam> root, Func<SqlBuilder<TTable>, TParam, object> buildSql)
      : base(root, buildSql)
    {
    }

    public FluentFrom<TParam, TTable, TReturn> As<TReturn>()
      where TReturn : class
    {
      return new FluentFrom<TParam, TTable, TReturn>(_root, _buildSql);
    }
  }
}
