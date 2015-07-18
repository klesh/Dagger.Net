using DaggerNet.Abstract;
using Dapper;
using Emmola.Helpers;
using System;

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

    /// <summary>
    /// Indicate migration belong to which concreate DataFactory
    /// </summary>
    public abstract Type DataFactoryType { get; }

    /// <summary>
    /// Migration version
    /// </summary>
    public abstract long Id { get;  }

    /// <summary>
    /// Readable title
    /// </summary>
    public string Title { get { return this.GetType().Name; } }

    /// <summary>
    /// Run creation script
    /// </summary>
    /// <param name="dagger"></param>
    /// <param name="sql"></param>
    protected virtual void RunCreationScript(Dagger dagger, string sql)
    {
      dagger.ExecuteNonQuery(sql);
    }

    /// <summary>
    /// Between creation and deletion, we can do programatic convertion here
    /// </summary>
    /// <param name="dagger"></param>
    protected virtual void RunConvertion(Dagger dagger)
    {
    }

    /// <summary>
    /// Run deletion script
    /// </summary>
    /// <param name="dagger"></param>
    /// <param name="sql"></param>
    protected virtual void RunDeletionScript(Dagger dagger, string sql)
    {
      dagger.ExecuteNonQuery(sql);
    }

    /// <summary>
    /// Run migration
    /// </summary>
    /// <param name="dagger"></param>
    public void Execute(Dagger dagger)
    {
      if (_flag == false)
      {
        lock (_locker)
        {
          if (_flag == false)
          {
            var parts = GetSqlScripts();
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

    public string[] GetSqlScripts()
    {
      var sqlScript = this.GetType().Assembly.GetResourceText(GetSqlName());
      return sqlScript.Split(new string[] { SqlGenerator.DELETION_SEPARATOR }, StringSplitOptions.RemoveEmptyEntries);
    }

    public string GetSqlName()
    {
      return "{0}.{1}".FormatMe(this.GetType().Namespace, this.Title);
    }

    public static long GenerateId()
    {
      return long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"));
    }
  }
}
