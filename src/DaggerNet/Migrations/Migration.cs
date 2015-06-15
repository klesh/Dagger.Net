using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using System.Reflection;
using Emmola.Helpers;
using System.IO;
using DaggerNet.Abstract;
using Dapper;

namespace DaggerNet.Migrations
{
  public abstract class Migration
  {
    private object _locker = new object();
    private bool _flag = false;
    private string _creation;
    private string _deletion;

    public Migration()
    {
    }

    public abstract long Id { get;  }

    public string Title { get { return this.GetType().Name; } }

    public virtual void RunCreationScript(Dagger dagger, string sql)
    {
      dagger.Execute(sql);
    }

    public virtual void RunConvertion(Dagger dagger)
    {
    }

    public virtual void RunDeletionScript(Dagger dagger, string sql)
    {
      dagger.Execute(sql);
    }

    public void Execute(Dagger dagger)
    {
      if (_flag == false)
      {
        lock (_locker)
        {
          if (_flag == false)
          {
            var name = "{0}.{1}.sql".FormatMe(this.GetType().Namespace, this.ToString());
            var sqlScript = Assembly.GetExecutingAssembly().GetResourceText(name);
            var parts = sqlScript.Split(new string[] { SqlGenerator.DELETION_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
            _creation = parts[0].Trim();
            _deletion = parts[1].Trim();
            _flag = true;
          }
        }
      }

      RunCreationScript(dagger, _creation);
      RunConvertion(dagger);
      RunDeletionScript(dagger, _deletion);
    }

    public static long GenerateId()
    {
      return long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
    }

    protected long _migartionId_ { get { throw new Exception("Place hold property shouldn't be called"); } }

    public override string ToString()
    {
      return "{0}_{1}".FormatMe(Id, Title);
    }
  }
}
