using Dagger.Net.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.NetTest.MigrationModels.v3
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
