using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;

namespace DaggerNet.Cultures
{
  /// <summary>
  /// For supporting multiple culture content
  /// The reason why not inherit from IDictionary<,> is because Dapper wouldn't find TypeHandler correctly.
  /// </summary>
  /// <typeparam name="T">ValueType/String</typeparam>
  [ComplexType]
  public class CultureData<T>
  {
    private Dictionary<CultureInfo, T> _texts;

    public CultureData()
    {
      _texts = new Dictionary<CultureInfo, T>();
    }

    public CultureData(IDictionary<string, T> dictionary)
    {
      _texts = dictionary.ToDictionary(p => CultureInfo.GetCultureInfo(p.Key), p => p.Value);
    }

    public IDictionary<string, T> ToDictionary()
    {
      return _texts.ToDictionary(p => p.Key.Name, p => p.Value);
    }

    public T this[CultureInfo culture]
    {
      get
      {
        return _texts[culture];
      }
      set
      {
        _texts[culture] = value;
      }
    }

    public IEnumerable<CultureInfo> Cultures
    {
      get
      {
        return _texts.Keys;
      }
    }

    public T Current
    {
      get
      {
        var culture = CultureInfo.CurrentCulture;
        if (culture != null && _texts.ContainsKey(culture))
          return this[culture];
        return _texts.Values.FirstOrDefault();
      }
    }

    public override string ToString()
    {
      if (_texts.Any())
        return Current.ToString();
      else
        return null;
    }
  }
}
