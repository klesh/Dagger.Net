using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class PrimaryKeyAttribute : Attribute
  {
    /// <summary>
    /// Indicate a auto increment column type
    /// </summary>
    public bool AutoIncrement { get; set; }

    /// <summary>
    /// Indicate a primary key order, for composite primary key.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Descending
    /// </summary>
    public bool Descending { get; set; }
  }
}
