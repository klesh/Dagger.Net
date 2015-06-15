using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

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
