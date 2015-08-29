using DaggerNet.Attributes;

namespace DaggerNet.Tests.Models.v3
{
  public interface IEntity
  {

  }

  public abstract class Entity : IEntity
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }
  }
}
