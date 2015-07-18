using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v2
{
  public class Blog : Entity
  {
    [MaxLength(50)]
    public string Author { get; set; }
  }
}
