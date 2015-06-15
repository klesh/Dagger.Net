using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Linq
{
  /// <summary>
  /// Sql helper to do function and formation
  /// </summary>
  public class Sql
  {
    public readonly static string[] LIKE_METHODS = new string[] { "Contains", "StartsWith", "EndsWith" };

    public static string Like(string text, int likeIndex = 0)
    {
      var quoted = text.Replace("%", "!%").Replace("_", "!_");
      if (likeIndex == 0)
        return "%" + quoted + "%";
      else if (likeIndex == 1)
        return quoted + "%";
      return "%" + quoted;
    }

    public static string StartsWith(string text)
    {
      return Like(text, 1);
    }

    public static string EndsWith(string text)
    {
      return Like(text, 2);
    }

    public static int Count(object column = null)
    {
      throw new NotImplementedException();
    }

    public static object Avg(object column)
    {
      throw new NotImplementedException();
    }

    public static object First(object column)
    {
      throw new NotImplementedException();
    }

    public static object Max(object column)
    {
      throw new NotImplementedException();
    }

    public static object Min(object column)
    {
      throw new NotImplementedException();
    }

    public static object Sum(object column)
    {
      throw new NotImplementedException();
    }

    public static object Lcase(object column)
    {
      throw new NotImplementedException();
    }

    public static object Ucase(object column)
    {
      throw new NotImplementedException();
    }

    public static object Mid(object column, object start, object length = null)
    {
      throw new NotImplementedException();
    }

    public static object Len(object column)
    {
      throw new NotImplementedException();
    }

    public static object Round(object column, object decimals = null)
    {
      throw new NotImplementedException();
    }

    public static object Now()
    {
      throw new NotImplementedException();
    }
  }
}
