using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

namespace DaggerNet.DOM.Abstract
{
  [Serializable]
  public class Base
  {
    /// <summary>
    /// Represent DOM name
    /// </summary>
    public virtual string Name { get; set; }

    #region HashSet Compare

    public override int GetHashCode()
    {
      if (Name.IsValid())
        return Name.GetHashCode();
      return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      return obj is Base && ((Base)obj).Name == this.Name;
    }

    #endregion
  }
}
