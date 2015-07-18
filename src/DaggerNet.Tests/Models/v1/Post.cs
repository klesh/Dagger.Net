using DaggerNet.Attributes;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v1
{
  public class Post
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }

    [MaxLength(255)]
    public string Title { get; set; }

    [DataType(DataType.Html)]
    public string Content { get; set; }
  }
}
