using DaggerNet.Attributes;

namespace DaggerNet.Tests.Models
{
  public class ProductProperty : IEntity
  {
    [Reference(typeof(Product))]
    [PrimaryKey]
    public long ProductId { get; set; }

    [Reference(typeof(Property))]
    [PrimaryKey]
    public long PropertyId { get; set; }
  }
}
