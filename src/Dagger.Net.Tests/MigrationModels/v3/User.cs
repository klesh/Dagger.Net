using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.NetTest.MigrationModels.v3
{
  public class User : Entity
  {
    [MaxLength(50)]
    public string Login { get; set; }

    [MaxLength(50)]
    public string Fullname { get; set; }

    public DateTime? LastLogin { get; set; }
  }
}
