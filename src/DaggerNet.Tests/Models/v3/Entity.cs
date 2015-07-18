using DaggerNet.Attributes;

namespace DaggerNet.Tests.Models.v3
{
  public abstract class Entity
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }
  }
}
