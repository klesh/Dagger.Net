using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Linq
{
  public interface ISqlHavingBuilder : ISqlWhereBuilder
  {
  }

  public class SqlHavingBuilder<T> : SqlBuilder<T>, ISqlHavingBuilder
    where T : class
  {
    public SqlHavingBuilder(SqlGroupBuilder<T> parent, LambdaExpression having)
      : base (parent, having)
    {
    }
  }
}
