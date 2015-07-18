using DaggerNet.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Linq
{
  public class SkipQueryBuilder<T> 
    where T : class
  {
    private ISqlBuilder _parent;
    private long _skip;

    public SkipQueryBuilder(ISqlBuilder parent, long skip)
    {
      _parent = parent;
      _skip = skip;
    }

    public string Limit(int limit)
    {
      return _parent.Sql.MakeLimitSql(this.ToString(), limit);
    }

    public override string ToString()
    {
      return _parent.Sql.MakeSkipSql(_parent.ToString(), _skip);
    }
  }
}
