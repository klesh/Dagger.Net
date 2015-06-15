using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaggerNet.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class IndexAttribute : Attribute
  {
    public string Name { get; set; }

    public int Order { get; set; }

    public bool Unique { get; set; }

    public bool Descending { get; set; }
  }

  [AttributeUsage(AttributeTargets.Property)]
  public class UniqueAttribute : IndexAttribute
  {
    public UniqueAttribute()
    {
      this.Unique = true;
    }
  }
}