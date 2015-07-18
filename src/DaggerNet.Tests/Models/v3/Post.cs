using DaggerNet.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v3
{
  public class Post : Entity
  {
    [MaxLength(255)]
    public string Title { get; set; }

    [DataType(DataType.Html)]
    [Index]
    public string Content1 { get; set; }

    [Reference(typeof(Blog))]
    public int BlogId { get; set; }

    public DateTime CreatedAt { get; set; }
  }
}
