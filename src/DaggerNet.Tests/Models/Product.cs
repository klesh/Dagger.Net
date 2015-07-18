using DaggerNet.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models
{
  public class Product : Entity
  {
    [Reference(typeof(Category))]
    [Unique(Name = "IX_CategoryId_Id", Order = 1)]
    public long CategoryId { get; set; }

    [Unique(Name = "IX_CategoryId_Id", Order = 2, Descending = true)]
    public override long Id { get; set; }

    [Unique]
    [Required]
    public string ModelNo { get; set; }

    [DataType(DataType.Text)]
    public string Content { get; set; }

    public HashSet<Item> Items { get; set; }

    public HashSet<Property> Properties { get; set; }

    public long Stock { get; set; }
  }

  public class ProductDto
  {
    public string Id { get; set; }
    public string ModelNo { get; set; }
  }
}