using System;
using System.Linq.Expressions;

namespace DaggerNet.Linq
{
  public class SqlJoinWhereBuilder<T, TJ> : SqlJoinBuilder<T, TJ>, ISqlWhereBuilder
    where T : class
    where TJ : class
  {
    public SqlJoinWhereBuilder(SqlJoinBuilder<T, TJ> parent, Expression<Func<T, TJ, bool>> lambda)
      : base(parent, lambda)
    {
    }
  }
}
