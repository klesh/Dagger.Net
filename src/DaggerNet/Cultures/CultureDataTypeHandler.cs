using Dapper;
using ServiceStack.Text;
using System.Collections.Generic;
using System.Data;

namespace DaggerNet.Cultures
{
  /// <summary>
  /// Handle CultureData type
  /// Since this would support only simple types only, all TypeHandlers will be added to SqlMapper in DbServer static constractor
  /// </summary>
  /// <typeparam name="T">ValueType/String</typeparam>
  public class CultureDataHandler<T> : SqlMapper.TypeHandler<CultureData<T>>
  {
    public override void SetValue(IDbDataParameter parameter, CultureData<T> value)
    {
      if (value == null)
        parameter.Value = null;
      else
        parameter.Value = JsonSerializer.SerializeToString(value.ToDictionary());
    }

    public override CultureData<T> Parse(object value)
    {
      return new CultureData<T>(JsonSerializer.DeserializeFromString<Dictionary<string, T>>((string)value));
    }
  }
}
