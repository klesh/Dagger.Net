using Dagger.Net.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Dagger.NetTest.Models
{
  public class Item : Entity
  {
    [Unique]
    public string ModelNo { get; set; }

    [Reference(typeof(Product))]
    [Required]
    public long ProductId { get; set; }
  }
}