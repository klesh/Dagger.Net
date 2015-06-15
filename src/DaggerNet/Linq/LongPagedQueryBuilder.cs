using DaggerNet.Abstract;
using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

namespace DaggerNet.Linq
{
  public class LongPagedQueryBuilder<T>
    where T : class
  {
    private SqlBuilder<T> _parent;
    private int _pageSize;
    private long _pageNo;
    private Column _pk;

    public LongPagedQueryBuilder(SqlBuilder<T> parent, int pageSize, long pageNo)
    {
      _parent = parent;
      _pageSize = pageSize;
      _pageNo = pageNo;

      var primaryKeys = _parent.Server.GetTable(typeof(T)).PrimaryKey.Columns;
      if (!primaryKeys.Any() || primaryKeys.Count > 1)
        throw new Exception("Long pagination requires one and only one primary key, composite keys are not supported");
      _pk = primaryKeys.First().Value;
    }

    public string GetHasNextSql(long lowestId)
    {
      return "SELECT EXISTS({0} {1} {2} < {3})".FormatMe(
        _parent,
        _parent is ISqlWhereBuilder ? "AND" : "WHERE",
        _parent.Server.Generator.Quote(_pk.Name),
        lowestId
        );
    }

    public override string ToString()
    {
      return "{0} ORDER BY {1} OFFSET {2} LIMIT {3}".FormatMe(
        _parent,
        _parent.Server.Generator.Quote(_pk.Name) + " DESC",
        (Math.Max(_pageNo, 1) - 1) * _pageSize,
        _pageSize
        );
    }
  }
}
