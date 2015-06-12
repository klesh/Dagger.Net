using Dagger.Net.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Dagger.NetTest.RenameModels
{
  public class Sku : Entity
  {
    [Unique]
    public string ModelNo { get; set; }

    [Reference(typeof(Product))]
    [Required]
    public long ProductId { get; set; }

    [MaxLength(100)]
    public string Properties { get; set; }
  }
}