using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.NetTest.MigrationModels.v3
{
  public class Blog : Entity
  {
    [MaxLength(50)]
    public string Author { get; set; }
  }
}
