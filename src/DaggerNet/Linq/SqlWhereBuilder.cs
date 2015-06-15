using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

namespace DaggerNet.Linq
{
  public interface ISqlWhereBuilder : ISqlBuilder
  {
  }

  public class SqlWhereBuilder<T> : SqlBuilder<T>, ISqlWhereBuilder
    where T : class
  {
    public SqlWhereBuilder(ISqlBuilder parent, LambdaExpression where)
      : base(parent, where)
    {
    }
  }
}
