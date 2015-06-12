using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dagger.NetTest.RenameModels
{
  public class Property : Entity
  {
    public HashSet<Product> Products { get; set; }
  }
}