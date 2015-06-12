using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net
{
  public class Paged<T>
  {
    public int Prev { get; protected set; }
    public int Next { get; protected set; }
    public bool HasPrev { get; protected set; }
    public bool HasNext { get; protected set; }
    public IEnumerable<T> List { get; protected set; }
  }
}
