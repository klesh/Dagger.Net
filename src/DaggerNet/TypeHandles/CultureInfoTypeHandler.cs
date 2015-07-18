using Dapper;
using System;
using System.Data;
using System.Globalization;

namespace DaggerNet.TypeHandles
{
  /// <summary>
  /// Serialize CultureInfo as its name and Deserialize from name.
  /// </summary>
  public class CultureInfoTypeHandler : SqlMapper.TypeHandler<CultureInfo>
  {
    public override CultureInfo Parse(object value)
    {
      if (value == null)
        return null;
      return CultureInfo.GetCultureInfo((string)value);
    }

    public override void SetValue(IDbDataParameter parameter, CultureInfo value)
    {
      if (value == null)
        parameter.Value = null;
      else
        parameter.Value = value.Name;
    }
  }
}
