using System.Collections.Generic;

namespace DaggerNet.Tests.Models
{
  public class Property : Entity
  {
    public HashSet<Product> Products { get; set; }
  }
}