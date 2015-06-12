using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.Linq
{
  public class SqlJoinGroupBuilder<T, TJ> : SqlJoinBuilder<T>, ISqlGroupBuilder
    where T : class
    where TJ : class
  {
    public SqlJoinGroupBuilder(SqlJoinBuilder<T, TJ> parent, LambdaExpression lambda)
      : base(parent, lambda)
    {
    }

    public SqlHavingBuilder<T, TJ> Having(Expression<Func<T, TJ, bool>> having)
    {
      return new SqlHavingBuilder<T, TJ>(this, having);
    }
  }
}
