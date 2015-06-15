using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DaggerNetTest.Models
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
  }
}