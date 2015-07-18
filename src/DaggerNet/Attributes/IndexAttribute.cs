using System;

namespace DaggerNet.Attributes
{
  [AttributeUsage(AttributeTargets.Property)]
  public class IndexAttribute : Attribute
  {
    public IndexAttribute()
    {
    }

    public IndexAttribute(string name)
    {
      Name = name;
    }

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