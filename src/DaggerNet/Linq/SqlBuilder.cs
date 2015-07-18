using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using DaggerNet.Abstract;
using System.Linq.Expressions;
using Emmola.Helpers;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DaggerNet.Linq
{
  public interface ISqlBuilder
  {
    DataModel Model { get; }
    SqlGenerator Sql { get; }
    Type Type { get; }
    bool IsRoot { get; }
    ISqlBuilder Parent { get; }
    string Build();
  }

  public class SqlBuilder<T> : SqlBuilderBase, ISqlBuilder
     where T : class
  {
    public bool IsRoot { get; protected set; }
    public ISqlBuilder Parent { get; protected set; }
    public LambdaExpression Lambda { get; protected set; }

    public SqlBuilder(DataModel model, SqlGenerator sql)
      : base(model, sql, typeof(T))
    {
      IsRoot = true;
    }

    protected SqlBuilder(ISqlBuilder parent, LambdaExpression lambda)
      : base(parent.Model, parent.Sql, parent.Type)
    {
      Parent = parent;
      Lambda = lambda;
    }

    public SqlBuilder<TN> Select<TN>(Expression<Func<T, TN>> select)
      where TN : class
    {
      return new SqlBuilder<TN>(this, select);
    }

    public SqlWhereBuilder<T> Where(Expression<Func<T, bool>> where)
    {
      return new SqlWhereBuilder<T>(this, where);
    }

    public OrderedQueryBuilder<T> OrderBy(Expression<Func<T, object>> orderBy)
    {
      return new OrderedQueryBuilder<T>(this, orderBy);
    }

    public OrderedQueryBuilder<T> OrderByDescending(Expression<Func<T, object>> orderBy)
    {
      return new OrderedQueryBuilder<T>(this, orderBy, true);
    }

    public SqlSetBuilder<T> Set(Expression<Func<T, object>> leftExp, Expression<Func<T, object>> rightExp)
    {
      return new SqlSetBuilder<T>(this, null, leftExp, rightExp);
    }

    public string Exists()
    {
      return "SELECT EXISTS ({0})".FormatMe(this.ToString());
    }

    public string Limit(int limit)
    {
      return Sql.MakeLimitSql(this.ToString(), limit);
    }

    public SkipQueryBuilder<T> Skip(long skip)
    {
      return new SkipQueryBuilder<T>(this, skip);
    }

    public SqlJoinBuilder<T, TJ> Join<TJ>()
      where TJ : class
    {
      return new SqlJoinBuilder<T, TJ>(this);
    }

    public SqlGroupBuilder<T> GroupBy(Expression<Func<T, object>> groupBy)
    {
      return new SqlGroupBuilder<T>(this, groupBy);
    }

    public virtual string Convert()
    {
      return Lambda == null ? "*" : Convert(Lambda.Body);
    }

    public string Count()
    {
      return "SELECT COUNT(*) FROM {0}".FormatMe(QuoteTable()) + Build() + BuildParents();
    }

    public string Delete()
    {
      return "DELETE FROM {0}".FormatMe(QuoteTable()) + Build() + BuildParents();
    }

    public override string ToString()
    {
      if (this is ISqlWhereBuilder)
        return Parent.ToString() + Build();
      else
        return BuildRoot() + BuildParents();
    }

    public virtual string Build()
    {
      if (this is ISqlGroupBuilder)
      {
        return " GROUP BY " + Convert();
      }
      else if (this is ISqlHavingBuilder)
      {
        return " HAVING " + Convert();
      }
      if (this is ISqlWhereBuilder)
      {
        if (Parent is ISqlWhereBuilder)
          return " AND ({0})".FormatMe(Convert());
        else
          return " WHERE ({0})".FormatMe(Convert());
      }
      return "";
    }

    protected virtual string BuildRoot()
    {
      return "SELECT {0} FROM {1}".FormatMe(Convert(), QuoteTable());
    }

    protected virtual string BuildParents()
    {
      var parentSql = "";
      var parent = Parent;
      while (parent is ISqlWhereBuilder)
      {
        parentSql = parent.Build() + parentSql;
        parent = Parent.Parent;
      }
      return parentSql;
    }
  }
}
