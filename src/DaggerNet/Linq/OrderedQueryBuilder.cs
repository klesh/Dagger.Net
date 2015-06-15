using DaggerNet.Abstract;
using System;
using System.Linq.Expressions;
using System.Text;
using Emmola.Helpers;

namespace DaggerNet.Linq
{
  public interface IOrderedQueryBuilder
  {
  }

  public class OrderedQueryBuilder<T> : SqlBuilder<T>, IOrderedQueryBuilder
    where T : class
  {
    private bool _descending;

    public OrderedQueryBuilder(ISqlBuilder parent, Expression<Func<T, object>> exp, bool desc = false)
      : base(parent, exp)
    {
      _descending = desc;
    }

    public OrderedQueryBuilder<T> ThenBy(Expression<Func<T, object>> columnExp)
    {
      return new OrderedQueryBuilder<T>(this, columnExp);
    }

    public OrderedQueryBuilder<T> ThenByDescending(Expression<Func<T, object>> columnExp)
    {
      return new OrderedQueryBuilder<T>(this, columnExp, true);
    }

    public override string ToString()
    {
      ;

      var builder = new StringBuilder(Parent.ToString());
      if (Parent is IOrderedQueryBuilder)
        builder.AppendSpaced(",");
      else
        builder.AppendSpaced("ORDER BY");
      builder.AppendSpaced(ConvertProperty(Lambda.Body));
      if (_descending)
        builder.AppendSpaced("DESC");
      return builder.ToString();
    }
  }
}
