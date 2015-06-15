using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNetTest.MigrationModels.v2
{
  public class Post : Entity
  {
    [MaxLength(255)]
    public string Title { get; set; }

    [DataType(DataType.Html)]
    [Index]
    public string Content { get; set; }

    [Reference(typeof(Blog))]
    public int BlogId { get; set; }
  }
}
