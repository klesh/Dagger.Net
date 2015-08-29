using DaggerNet.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v3
{
  public class Blog : Entity
  {
    [MaxLength(50)]
    public string Author { get; set; }
  }
}
