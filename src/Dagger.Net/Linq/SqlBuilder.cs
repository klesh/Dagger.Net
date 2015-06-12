using Dagger.Net.DOM;
using System;
using System.Collections.Generic;
using Dagger.Net.Abstract;
using System.Linq.Expressions;
using Emmola.Helpers;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dagger.Net.Linq
{
  public interface ISqlBuilder
  {
    DbServer Server { get; }
    Type Type { get; }
    bool IsRoot { get; }
    ISqlBuilder Parent { get; }
    string Build();
  }

  public class SqlBuilder<T> : ISqlBuilder
     where T : class
  {
    public DbServer Server { get; protected set; }
    public Type Type { get; protected set; }
    public bool IsRoot { get; protected set; }
    public ISqlBuilder Parent { get; protected set; }
    public LambdaExpression Lambda { get; protected set; }

    public SqlBuilder(DbServer server)
      : this(server, typeof(T))
    {
      IsRoot = true;
    }

    public SqlBuilder(DbServer server, Type type)
    {
      Server = server;
      Type = type;
    }

    protected SqlBuilder(ISqlBuilder parent, LambdaExpression lambda)
      : this (parent.Server, parent.Type)
    {
      Parent = parent;
      Lambda = lambda;
    }

    public SqlBuilder<TN> Select<TN>(Expression<Func<T, TN>> select)
      where TN : class
    {
      return new SqlBuilder<TN>(this, select);
    }

    public SqlWhereBuilder<T> Where(Expression<Func<T, bool>> where)
    {
      return new SqlWhereBuilder<T>(this, where);
    }

    public OrderedQueryBuilder<T> OrderBy(Expression<Func<T, object>> orderBy)
    {
      return new OrderedQueryBuilder<T>(this, orderBy);
    }

    public OrderedQueryBuilder<T> OrderByDescending(Expression<Func<T, object>> orderBy)
    {
      return new OrderedQueryBuilder<T>(this, orderBy, true);
    }

    public LongPagedQueryBuilder<T> LongPage(int pageSize, long pageNo)
    {
      return new LongPagedQueryBuilder<T>(this, pageSize, pageNo);
    }

    public SqlJoinBuilder<T, TJ> Join<TJ>()
      where TJ : class
    {
      return new SqlJoinBuilder<T, TJ>(this);
    }

    public SqlGroupBuilder<T> GroupBy(Expression<Func<T, object>> groupBy)
    {
      return new SqlGroupBuilder<T>(this, groupBy);
    }

    protected virtual string QuoteTable()
    {
      return QuoteTable(Type);
    }

    protected virtual string QuoteTable(Type type)
    {
      return Server.Generator.QuoteTable(Server.GetTable(type));
    }

    protected virtual string QuoteProperty(MemberExpression memberExp)
    {
      return Server.Generator.Quote(memberExp.Member.Name);
    }

    public virtual string Convert()
    {
      return Lambda == null ? "*" : Convert(Lambda.Body);
    }

    protected virtual string ConvertProperty(Expression exp)
    {
      var memberExp = (MemberExpression)exp;
      var propertyInfo = memberExp.Member as PropertyInfo;
      if (propertyInfo == null)
        throw new Exception("Supports only property of class " + exp.ToString());

      if (memberExp.Expression is ParameterExpression)
      {
        return QuoteProperty(memberExp);
      }
      else
      {
        return "@" + memberExp.Member.Name;
      }
    }

    protected virtual string ConvertMethod(Expression exp)
    {
      var callExp = (MethodCallExpression)exp;
      var type = callExp.Method.DeclaringType;
      if (type == typeof(string))
      {
        var likeIndex = Array.IndexOf<string>(Sql.LIKE_METHODS, callExp.Method.Name);
        if (likeIndex > -1)
        {
          var arg = callExp.Arguments[0];
          string formated;
          if (arg.NodeType == ExpressionType.Constant)
          {
            var constExp = (ConstantExpression)arg;
            formated = Server.Generator.QuoteText(Sql.Like(constExp.Value.ToString(), likeIndex));
          }
          else
          {
            formated = Convert(arg);
          }
          return "{0} LIKE {1} ESCAPE '!'".FormatMe(ConvertProperty(callExp.Object), formated);
        }
      }
      else if (type == typeof(Sql))
      {
        return "{0}({1})".FormatMe(callExp.Method.Name, callExp.Arguments.Select(arg => Convert(arg)).Implode(", "));
      }
      throw new NotImplementedException(callExp.ToString());
    }

    protected virtual string Convert(Expression exp)
    {
      switch (exp.NodeType)
      {
        case ExpressionType.Add:
        case ExpressionType.AddChecked: return ConvertBinary(exp, "+");
        case ExpressionType.And: return ConvertBinary(exp, "&");
        case ExpressionType.Divide: return ConvertBinary(exp, "/");
        case ExpressionType.ExclusiveOr: return ConvertBinary(exp, "^");
        case ExpressionType.Modulo: return ConvertBinary(exp, "%");
        case ExpressionType.Multiply:
        case ExpressionType.MultiplyChecked: return ConvertBinary(exp, "*");
        case ExpressionType.Or: return ConvertBinary(exp, "|");
        case ExpressionType.Power: return ConvertBinFunc(exp, "Power");
        case ExpressionType.Subtract:
        case ExpressionType.SubtractChecked: return ConvertBinary(exp, "-");
        case ExpressionType.Coalesce: return ConvertBinFunc(exp, "Coalesce");

        case ExpressionType.Equal: return ConvertBinary(exp, "=");
        case ExpressionType.NotEqual: return ConvertBinary(exp, "<>");
        case ExpressionType.OrElse: return ConvertBinary(exp, "OR");
        case ExpressionType.AndAlso: return ConvertBinary(exp, "AND");
        case ExpressionType.Not: return ConvertUnary(exp, "NOT");
        case ExpressionType.GreaterThan: return ConvertBinary(exp, ">");
        case ExpressionType.GreaterThanOrEqual: return ConvertBinary(exp, ">=");
        case ExpressionType.LessThan: return ConvertBinary(exp, "<");
        case ExpressionType.LessThanOrEqual: return ConvertBinary(exp, "<=");


        case ExpressionType.New: return ConvertNew(exp);
        case ExpressionType.Constant: return ConvertConstant(exp);
        case ExpressionType.Convert: return Convert(((UnaryExpression)exp).Operand);
        case ExpressionType.MemberAccess: return ConvertProperty(exp);
        case ExpressionType.Call: return ConvertMethod(exp);

        case ExpressionType.Parameter: return "*";

        default:
          throw new NotImplementedException(exp.ToString());
      }
    }

    protected virtual string ConvertBinary(Expression exp, string op)
    {
      var binExp = (BinaryExpression)exp;
      return "{0} {1} {2}".FormatMe(Convert(binExp.Left), op, Convert(binExp.Right));
    }

    protected virtual string ConvertBinFunc(Expression exp, string func)
    {
      var binExp = (BinaryExpression)exp;
      return "{0}({1}, {2})".FormatMe(func, Convert(binExp.Left), Convert(binExp.Right));
    }

    protected virtual string ConvertUnary(Expression exp, string op)
    {
      var unaryExp = (UnaryExpression)exp;
      return "{0} {1}".FormatMe(op, Convert(unaryExp.Operand));
    }

    protected virtual string ConvertConstant(Expression exp)
    {
      var constExp = (ConstantExpression)exp;
      if (constExp.Type == typeof(string))
        return Server.Generator.QuoteText((string)constExp.Value);


      return constExp.Value.ToString();
    }

    protected virtual string ConvertNew(Expression exp)
    {
      var newExp = (NewExpression)exp;
      return newExp.Arguments.Select((arg, i) =>
      {
        var left = Server.Generator.Quote(newExp.Members[i].Name);
        var right = Convert(arg);
        return left == right ? left : "{0} AS {1}".FormatMe(right, left);
      }).Implode(", ");
    }

    public string Count()
    {
      return "SELECT COUNT(*) FROM {0}".FormatMe(QuoteTable()) + Build() + BuildParents();
    }

    public override string ToString()
    {
      if (this is ISqlWhereBuilder)
        return Parent.ToString() + Build();
      else
        return BuildRoot() + BuildParents();
    }

    public virtual string Build()
    {
      if (this is ISqlGroupBuilder)
      {
        return " GROUP BY " + Convert();
      }
      else if (this is ISqlHavingBuilder)
      {
        return " HAVING " + Convert();
      }
      if (this is ISqlWhereBuilder)
      {
        if (Parent is ISqlWhereBuilder)
          return " AND ({0})".FormatMe(Convert());
        else
          return " WHERE ({0})".FormatMe(Convert());
      }
      return "";
    }

    protected virtual string BuildRoot()
    {
      return "SELECT {0} FROM {1}".FormatMe(Convert(), QuoteTable());
    }

    protected virtual string BuildParents()
    {
      var parentSql = "";
      var parent = Parent;
      while (parent is ISqlWhereBuilder)
      {
        parentSql = parent.Build() + parentSql;
        parent = Parent.Parent;
      }
      return parentSql;
    }
  }
}
