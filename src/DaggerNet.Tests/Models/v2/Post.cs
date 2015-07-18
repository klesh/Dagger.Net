using DaggerNet.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v2
{
  public class Post : Entity
  {
    [MaxLength(255)]
    [Default("'test'")]
    public string Title { get; set; }

    [DataType(DataType.Html)]
    [Index]
    public string Content { get; set; }

    [Reference(typeof(Blog))]
    public int BlogId { get; set; }
  }
}
