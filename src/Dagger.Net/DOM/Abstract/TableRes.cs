using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.DOM.Abstract
{
  /// <summary>
  /// Represent a Table Resource
  /// </summary>
  [Serializable]
  public class TableRes : Base
  {
    /// <summary>
    /// Belongs to
    /// </summary>
    public virtual Table Table { get; set; }
  }
}
