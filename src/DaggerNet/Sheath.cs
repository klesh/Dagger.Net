using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace DaggerNet
{
  /// <summary>
  /// provide sql loggin functionality
  /// </summary>
  [DesignerCategory("")]
  public class Sheath : DbCommand
  {
    private DbCommand _command;
    private Action<string> _logger;

    public Sheath(DbCommand command, Action<string> logger)
    {
      _command = command;
      _logger = logger;
    }

    protected void BeforeExecute()
    {
      _logger(this.CommandText);
      if (this.Connection.State == ConnectionState.Closed)
        this.Connection.Open();
    }

    public override void Cancel()
    {
      _command.Cancel();
    }

    public override string CommandText
    {
      get
      {
        return _command.CommandText;
      }
      set
      {
        _command.CommandText = value;
      }
    }

    public override int CommandTimeout
    {
      get
      {
        return _command.CommandTimeout ;
      }
      set
      {
        _command.CommandTimeout = value;
      }
    }

    public override CommandType CommandType
    {
      get
      {
        return _command.CommandType;
      }
      set
      {
        _command.CommandType = value;
      }
    }

    protected override DbParameter CreateDbParameter()
    {
      return _command.CreateParameter();
    }

    protected override DbConnection DbConnection
    {
      get
      {
        return _command.Connection;
      }
      set
      {
        _command.Connection = value;
      }
    }

    protected override DbParameterCollection DbParameterCollection
    {
      get { return _command.Parameters; }
    }

    protected override DbTransaction DbTransaction
    {
      get
      {
        return _command.Transaction;
      }
      set
      {
        _command.Transaction = value;
      }
    }

    public override bool DesignTimeVisible
    {
      get
      {
        return _command.DesignTimeVisible;
      }
      set
      {
        _command.DesignTimeVisible = value;
      }
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
      BeforeExecute();
      return _command.ExecuteReader(behavior);
    }

    public override int ExecuteNonQuery()
    {
      BeforeExecute();
      return _command.ExecuteNonQuery();
    }

    public override object ExecuteScalar()
    {
      BeforeExecute();
      return _command.ExecuteScalar();
    }

    public override void Prepare()
    {
      _command.Prepare();
    }

    public override UpdateRowSource UpdatedRowSource
    {
      get
      {
        return _command.UpdatedRowSource;
      }
      set
      {
        _command.UpdatedRowSource = value;
      }
    }
  }
}
