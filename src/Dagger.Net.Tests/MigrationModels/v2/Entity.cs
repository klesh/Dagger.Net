using Dagger.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.NetTest.MigrationModels.v2
{
  public abstract class Entity
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }
  }
}
