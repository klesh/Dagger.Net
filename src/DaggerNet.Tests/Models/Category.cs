using DaggerNet.Attributes;
using System.Collections.Generic;

namespace DaggerNet.Tests.Models
{
  public class Category : Entity
  {
    [Reference(OnDelete = Cascades.Cascade)]
    public long? ParentId { get; set; }

    public HashSet<Category> Children { get; set; }

    public Category Parent { get; set; }
  }
}