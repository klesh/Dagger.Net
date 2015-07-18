using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

namespace DaggerNet.Linq
{
  public interface ISqlSetBuilder
  {
    string BuildParents();
  }

  public class SqlSetBuilder<T> : SqlBuilderBase, ISqlSetBuilder
  {
    protected ISqlBuilder _root;
    protected ISqlSetBuilder _parent;
    protected LambdaExpression _leftExp;
    protected LambdaExpression _rightExp;

    public SqlSetBuilder(ISqlBuilder root, ISqlSetBuilder parent, LambdaExpression leftExp, LambdaExpression rightExp)
      : base(root.Model, root.Sql, root.Type)
    {
      _root = root;
      _parent = parent;
      _leftExp = leftExp;
      _rightExp = rightExp;
    }

    public SqlSetBuilder<T> Set(Expression<Func<T, object>> leftExp, Expression<Func<T, object>> rightExp)
    {
      return new SqlSetBuilder<T>(_root, this, leftExp, rightExp);
    }

    public string BuildParents()
    {
      var setSql = "{0} = {1}".FormatMe(Convert(_leftExp.Body), Convert(_rightExp.Body));
      if (_parent != null)
        setSql = "{0}, {1}".FormatMe(_parent.BuildParents(), setSql);
      return setSql;
    }

    public override string ToString()
    {
      var sql = "UPDATE {0} SET {1}".FormatMe(
        QuoteTable(),
        BuildParents());
      var where = _root.Build();
      if (where.IsValid())
        sql += where;
      return sql;
    }
  }
}
