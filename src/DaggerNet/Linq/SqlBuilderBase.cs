using DaggerNet.Abstract;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Emmola.Helpers;

namespace DaggerNet.Linq
{
  public abstract class SqlBuilderBase
  {
    public SqlBuilderBase(DataModel model, SqlGenerator sql, Type type)
    {
      Model = model;
      Sql = sql;
      Type = type;
    }

    public DataModel Model { get; protected set; }
    public SqlGenerator Sql { get; protected set; }
    public Type Type { get; protected set; }

    protected virtual string QuoteTable()
    {
      return QuoteTable(Type);
    }

    protected virtual string QuoteTable(Type type)
    {
      return Sql.QuoteTable(Model.GetTable(type));
    }

    protected virtual string QuoteProperty(MemberExpression memberExp)
    {
      return Sql.Quote(memberExp.Member.Name);
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
        var likeIndex = Array.IndexOf<string>(SqlFunctions.LIKE_METHODS, callExp.Method.Name);
        if (likeIndex > -1)
        {
          var arg = callExp.Arguments[0];
          string formated;
          if (arg.NodeType == ExpressionType.Constant)
          {
            var constExp = (ConstantExpression)arg;
            formated = Sql.QuoteText(SqlFunctions.Like(constExp.Value.ToString(), likeIndex));
          }
          else
          {
            formated = Convert(arg);
          }
          return "{0} LIKE {1} ESCAPE '!'".FormatMe(ConvertProperty(callExp.Object), formated);
        }
      }
      else if (type == typeof(SqlFunctions))
      {
        return "{0}({1})".FormatMe(callExp.Method.Name, callExp.Arguments.Select(arg => Convert(arg)).Implode(", "));
      }
      else if (callExp.Method.Name == "Contains" && type == typeof(Enumerable))
      {
        return "{0} IN {1}".FormatMe(Convert(callExp.Arguments.Last()), ConvertProperty(callExp.Arguments.First()));
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
      var left = Convert(binExp.Left);
      var right = Convert(binExp.Right);
      if (right == "NULL")
        op = op == "=" ? "IS" : "IS NOT";
      return "{0} {1} {2}".FormatMe(left, op, right);
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
        return Sql.QuoteText((string)constExp.Value);

      return constExp.Value == null ? "NULL" : constExp.Value.ToString();
    }

    protected virtual string ConvertNew(Expression exp)
    {
      var newExp = (NewExpression)exp;
      return newExp.Arguments.Select((arg, i) =>
      {
        var left = Sql.Quote(newExp.Members[i].Name);
        var right = Convert(arg);
        return left == right ? left : "{0} AS {1}".FormatMe(right, left);
      }).Implode(", ");
    }
  }
}
