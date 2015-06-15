using DaggerNet.PostgresSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using DaggerNetTest.Models;

namespace DaggerNet.Tests
{
  public class DataServer : PostgresSQLServer
  {
    public DataServer()
      : base(
          typeof(Entity).FindSubTypesInMe(), 
          "Server=localhost;Port=5432;Database={0};User Id=super;Password=info123;Pooling=true;MinPoolSize=10;MaxPoolSize=100;Protocol=3;",
          "Emmola")
    {
    }
  }
}
