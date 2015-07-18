using DaggerNet.Attributes;
using DaggerNet.DOM.Abstract;
using Emmola.Helpers;
using Emmola.Helpers.Classes;
using Emmola.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DaggerNet.DOM
{
  [Serializable]
  public class ForeignKey : PrimaryKey, ISimilarity<ForeignKey>
  {
    [NonSerialized]
    private Type _referType;

    public ForeignKey(Table table, string name)
    {
      Table = table;
      Name = name;
      Columns = new SortedSet<Ordered<Column>>();
    }

    public ForeignKey(Table table, Type referType)
    {
      Table = table;
      _referType = referType;
      Columns = new SortedSet<Ordered<Column>>();
    }

    /// <summary>
    /// Foreig Table Type
    /// </summary>
    public Type ReferType { get { return _referType; } }

    /// <summary>
    /// Foreig Table
    /// </summary>
    public Table ReferTable { get; set; }

    /// <summary>
    /// Cascade delete
    /// </summary>
    public Cascades OnDelete { get; set; }

    /// <summary>
    /// Cascade update
    /// </summary>
    public Cascades OnUpdate { get; set; }


    public List<Column> ReferColumns { get; set; }

    public override string ToString()
    {
      return "{0} ({1})".FormatMe(Table.Name, Columns.Select(c => c.Value.Name).Implode(", "));
    }

    public string GetIdentityString()
    {
      return "{0} {1} {2} {3}".FormatMe(
        Columns.Select(c => c.Value.Name).Implode(", "),
        ReferTable.Name,
        OnDelete.ToString(),
        OnUpdate.ToString());
    }

    public float CalculateSimilarity(ForeignKey other)
    {
      return this.GetIdentityString() == other.GetIdentityString() ? 0.9f : 0f; // change name only
    }
  }
}
