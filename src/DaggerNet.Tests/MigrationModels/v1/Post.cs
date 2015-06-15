using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNetTest.MigrationModels.v1
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
