using DaggerNet.DOM.Abstract;
using Emmola.Helpers.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.DOM
{
  public class PrimaryKey : TableRes
  {
    protected PrimaryKey()
    {
    }

    public PrimaryKey(Table table)
    {
      Table = table;
      Name = "PK_" + table.Name;
      Columns = new SortedSet<Ordered<Column>>(table.Columns.Where(c => c.PrimaryKey).Select(c => new Ordered<Column>(c, c.Order)));
    }

    public SortedSet<Ordered<Column>> Columns { get; set; }
  }
}
