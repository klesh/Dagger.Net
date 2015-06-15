using DaggerNet.DOM.Abstract;
using Emmola.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Emmola.Helpers;

namespace DaggerNet.DOM
{
  [Serializable]
  public class Index : TableRes, ISimilarity<Index>
  {
    public Index()
    {
      this.Columns = new SortedSet<IndexColumn>();
    }

    public Index(Table table, string name)
      : this()
    {
      Table = table;
      Name = name;
    }

    /// <summary>
    /// As in { Column: descending } pair.
    /// </summary>
    public SortedSet<IndexColumn> Columns { get; set; }

    /// <summary>
    /// Indicate if an index is unique
    /// </summary>
    public bool Unique { get; set; }

    public float CalculateSimilarity(Index other)
    {
      var thisIndex = this.Columns.Select(ic => "{0} {1}".FormatMe(ic.Value.NewName ?? ic.Value.Name, ic.Descending.ToString())).Implode(", ");
      var thatIndex = other.Columns.Select(ic => "{0} {1}".FormatMe(ic.Value.NewName ?? ic.Value.Name, ic.Descending.ToString())).Implode(", ");
      var exactlySame = Unique == other.Unique && thisIndex == thatIndex;

      return exactlySame ? 0.9f : 0f; // index columns are not modifyable, either add or drop. comparison is pointless.
    }
  }
}
