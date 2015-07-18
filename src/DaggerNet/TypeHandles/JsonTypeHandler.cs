using Dapper;
using ServiceStack.Text;
using System;
using System.Data;

namespace DaggerNet.TypeHandles
{
  /// <summary>
  /// Add json support for dapper
  /// All types with ComplexTypeAttrite will be convert to json structure in database.
  /// Each TypeHandler add dynamically during Class2Dom process.
  /// </summary>
  public class JsonTypeHandler : SqlMapper.ITypeHandler
  {
    public object Parse(Type destinationType, object value)
    {
      if (value == null)
        return null;
      return JsonSerializer.DeserializeFromString((string)value, destinationType);
    }

    public void SetValue(IDbDataParameter parameter, object value)
    {
      if (value == null)
        parameter.Value = null;
      else
        parameter.Value = JsonSerializer.SerializeToString(value);
    }
  }
}
