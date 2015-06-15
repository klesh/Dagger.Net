using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class DefaultAttribute : Attribute
  {
    public DefaultAttribute(string defaultValue)
    {
      Default = defaultValue;
    }

    public string Default { get; set; }
  }
}
