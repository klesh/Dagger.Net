using Emmola.Helpers.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.DOM
{
  [Serializable]
  public class IndexColumn : Ordered<Column>
  {
    public IndexColumn(Column column, int order, bool descending)
      : base (column, order)
    {
      Descending = descending;
    }

    public bool Descending { get; set; }
  }
}
