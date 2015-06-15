using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaggerNetTest.Models
{
  public class Category : Entity
  {
    [Reference]
    public long? ParentId { get; set; }

    public HashSet<Category> Children { get; set; }

    public Category Parent { get; set; }
  }
}