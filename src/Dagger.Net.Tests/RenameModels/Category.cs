using Dagger.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dagger.NetTest.RenameModels
{
  public class Category : Entity
  {
    [Reference]
    public long? ParentId { get; set; }

    public HashSet<Category> Children { get; set; }

    public Category Parent { get; set; }
  }
}