using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using Dagger.Net.DOM;

namespace Dagger.Net.Linq
{
  public interface ISqlJoinBuilder : ISqlBuilder
  {
    Type JoinType { get; }
    int Index { get; }
  }

  public class SqlJoinBuilder<T> : SqlBuilder<T>, ISqlJoinBuilder
    where T : class
  {
    public Type JoinType { get; protected set; }
    public int Index { get; protected set; }
    public string AliasLeft { get; protected set; }
    public string AliasRight { get; protected set; }

    public SqlJoinBuilder(ISqlBuilder parent, Type joinType, LambdaExpression lambda, int index)
       : base(parent, lambda)
    {
      JoinType = joinType;
      Index = index;
      AliasLeft = new String(new char[] { (char)(Index + 97) });
      AliasRight = new String(new char[] { (char)(Index + 98) });
    }

    public SqlJoinBuilder(ISqlJoinBuilder parent, LambdaExpression lambda)
      : this(parent, parent.JoinType, lambda, parent.Index)
    {
    }

    protected override string QuoteProperty(MemberExpression memberExp)
    {
      var parameterExp = (ParameterExpression)memberExp.Expression;
      var isLeft = Lambda.Parameters[0].Name == parameterExp.Name;
      var alias = isLeft ? AliasLeft : AliasRight;
      return  alias + "." + base.QuoteProperty(memberExp);
    }

    protected override string QuoteTable()
    {
      return "{0} AS {1}".FormatMe(base.QuoteTable(), AliasLeft);
    }

    protected string BuildJoin(Table leftTable, Table rightTable, string leftAlias, string rightAlias)
    {
      var leftFK = leftAlias;
      var rightFk = rightAlias;
      var foreignKey = leftTable.ForeignKeys.FirstOrDefault(fk => fk.ReferTable == rightTable);
      if (foreignKey == null)
      {
        foreignKey = rightTable.ForeignKeys.FirstOrDefault(fk => fk.ReferTable == leftTable);
        leftFK = rightAlias;
        rightFk = leftAlias;
      }
      if (foreignKey == null)
        throw new Exception("Foreign can not be found: {0} {1}".FormatMe(leftTable.Name, rightTable.Name));

      return " JOIN {0} AS {1} ON ({2})".FormatMe(
        Server.Generator.QuoteTable(rightTable),
        rightAlias,
        foreignKey.Columns.Select((c, i) =>
        {
          var cl = c.Value;
          var cr = foreignKey.ReferColumns[i];
          return "{0}.{1} = {2}.{3}".FormatMe(
            leftFK, Server.Generator.QuoteColumn(cl),
            rightFk, Server.Generator.QuoteColumn(cr));
        }).Implode(", ")
      );
    }

    protected string BuildJoin2()
    {
      var sql = "";

      var tableLeft = Server.GetTable(Type);
      var tableRight = Server.GetTable(JoinType);
      var foreignKey = tableLeft.ForeignKeys.FirstOrDefault(fk => fk.ReferTable == tableRight);
      if (foreignKey == null)
      {
        var tableMiddle = Server.GetTable(Type, JoinType);
        var middleAlias = AliasLeft + AliasRight;
        sql += BuildJoin(tableLeft, tableMiddle, AliasLeft, middleAlias);
        sql += BuildJoin(tableMiddle, tableRight, middleAlias, AliasRight);
      }
      else
      {
        sql = BuildJoin(tableLeft, tableRight, AliasLeft, AliasRight);
      }

      return sql;
    }

    protected override string BuildRoot()
    {
      return base.BuildRoot() + BuildJoin2();
    }
  }


  public class SqlJoinBuilder<T1, T2> : SqlJoinBuilder<T1>
    where T1 : class
    where T2 : class
  {
    public SqlJoinBuilder(SqlBuilder<T1> parent, int index = 0)
      : base(parent, typeof(T2), null, index)
    {
    }

    public SqlJoinBuilder(SqlJoinBuilder<T1> parent, LambdaExpression lambda)
      : base(parent, lambda)
    {
    }

    public SqlJoinBuilder<TN> Select<TN>(Expression<Func<T1, T2, TN>> select)
      where TN : class
    {
      return new SqlJoinBuilder<TN>(this, select);
    }

    public SqlJoinWhereBuilder<T1, T2> Where(Expression<Func<T1, T2, bool>> where)
    {
      return new SqlJoinWhereBuilder<T1, T2>(this, where);
    }

    public SqlJoinGroupBuilder<T1, T2> GroupBy(Expression<Func<T1, T2, object>> groupBy)
    {
      return new SqlJoinGroupBuilder<T1, T2>(this, groupBy);
    }
  }
}
