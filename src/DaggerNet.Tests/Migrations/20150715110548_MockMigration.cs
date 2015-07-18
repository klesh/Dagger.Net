using DaggerNet;
using DaggerNet.Migrations;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Tests.Migrations
{
  public class MockMigration : Migration
  {
    public override Type DataFactoryType
    {
      get { return typeof(DaggerNet.Tests.DataFactoryTest); }
    }

    public override long Id
    {
      get { return 20150715110548; }
    }


    /// <summary>
    /// You can run your data convertion safely here. This method called before deletion/constaintion
    /// </summary>
    /// <param name="dagger">Dagger instance</param>
    protected override void RunConvertion(Dagger dagger)
    {
      base.RunConvertion(dagger);
    }
  }
}
