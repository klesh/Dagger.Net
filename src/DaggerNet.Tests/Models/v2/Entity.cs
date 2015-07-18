using DaggerNet.Attributes;

namespace DaggerNet.Tests.Models.v2
{
  public abstract class Entity
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }
  }
}
