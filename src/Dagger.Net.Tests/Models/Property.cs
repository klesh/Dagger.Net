using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Dagger.NetTest.Models
{
  public class Property : Entity
  {
    public HashSet<Product> Products { get; set; }
  }
}