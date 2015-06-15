using DaggerNet.DOM.Abstract;
using Emmola.Helpers.Interfaces;
using System;
using System.Reflection;
using Emmola.Helpers;

namespace DaggerNet.DOM
{
  [Serializable]
  public class Column : TableRes, ISimilarity<Column>
  {
    public Column(Table table)
    {
      Table = table;
    }

    internal Column(Table table, PropertyInfo property)
    {
      Table = table;
      Name = property.Name;
      GetMethod = property.GetGetMethod();
      SetMethod = property.GetSetMethod();
      PropertyInfo = property;
    }

    internal MethodInfo GetMethod { get; private set; }

    internal MethodInfo SetMethod { get; private set; }

    internal PropertyInfo PropertyInfo { get; private set; }

    /// <summary>
    /// Database Type
    /// </summary>
    public SqlType DataType { get; set; }

    /// <summary>
    /// Extra def info like length/precision
    /// </summary>
    public object[] Precision { get; set; }

    /// <summary>
    /// Default value
    /// </summary>
    public string Default { get; set; }

    /// <summary>
    /// Indicate if column is AutoIncrement
    /// </summary>
    public bool AutoIncrement { get; set; }

    /// <summary>
    /// Indicate not null column
    /// </summary>
    public bool NotNull { get; set; }

    /// <summary>
    /// Indicates if primarykey, support composite
    /// </summary>
    public bool PrimaryKey { get; set; }

    /// <summary>
    /// For composite primary key
    /// </summary>
    public int Order { get; set; }

    public string NewName { get; set; }

    /// <summary>
    /// Calculate similarity to another column
    /// </summary>
    /// <param name="other">Other column</param>
    /// <returns>Similarity</returns>
    public float CalculateSimilarity(Column other)
    {
      var serialize = new Func<Column, string[]>(c => new string[] 
      { 
        "AutoIncrement{0}".FormatMe(c.AutoIncrement.ToString()),
        "DataType{0}".FormatMe( c.DataType.ToString()), 
        "Precision{0}".FormatMe(c.Precision == null ? "" : c.Precision.Implode(", ")),
        "Default{0}".FormatMe( c.Default),
        "NotNull{0}".FormatMe( c.NotNull.ToString()),
        "PrimaryKey{0}" .FormatMe( c.PrimaryKey.ToString()),
        "Order{0}".FormatMe( c.Order.ToString())
      });

      var selfArray = serialize(this);
      var otherArray = serialize(other);

      return selfArray.SimilarityTo(otherArray);
    }

    //public override bool Equals(object obj)
    //{
    //  return obj is Column && ((Column)obj).CalculateSimilarity(this) == 1f;
    //}
  }
}
