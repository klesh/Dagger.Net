using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.Linq
{
  public interface ISqlGroupBuilder : ISqlWhereBuilder
  {
  }

  public class SqlGroupBuilder<T> : SqlBuilder<T>, ISqlGroupBuilder
    where T : class
  {
    public SqlGroupBuilder(ISqlBuilder parent, LambdaExpression lambda)
      : base (parent, lambda)
    {
    }

    public SqlHavingBuilder<T> Having(Expression<Func<T, bool>> having)
    {
      return new SqlHavingBuilder<T>(this, having);
    }
  }
}
