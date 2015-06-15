using DaggerNet.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DaggerNetTest.Models
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