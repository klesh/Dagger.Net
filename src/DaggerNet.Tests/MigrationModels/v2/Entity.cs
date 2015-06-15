using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNetTest.MigrationModels.v2
{
  public abstract class Entity
  {
    [PrimaryKey(AutoIncrement = true)]
    public int Id { get; set; }
  }
}
