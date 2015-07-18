using DaggerNet;
using DaggerNet.Migrations;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _nameSpace_.Migrations
{
  public class _migrationName_ : Migration
  {
    public override Type DataFactoryType
    {
      get { return typeof(_factoryType_); }
    }

    public override long Id
    {
      get { return _migrationId_; }
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
