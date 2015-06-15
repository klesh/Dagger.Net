using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Linq
{
  public class SqlHavingBuilder<T, T2> : SqlJoinBuilder<T, T2>, ISqlHavingBuilder
    where T : class
    where T2 : class
  {
    public SqlHavingBuilder(SqlJoinGroupBuilder<T, T2> parent, LambdaExpression having)
      : base (parent, having)
    {
    }
  }
}
