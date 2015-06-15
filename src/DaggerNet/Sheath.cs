using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet
{
  /// <summary>
  /// provide sql loggin functionality
  /// </summary>
  public class Sheath : IDbCommand
  {
    private IDbCommand _command;
    private Action<string> _logger;

    public Sheath(IDbCommand command, Action<string> logger)
    {
      _command = command;
      _logger = logger;
    }

    public void Cancel()
    {
      _command.Cancel();
    }

    public string CommandText
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

    public int CommandTimeout
    {
      get
      {
        return _command.CommandTimeout;
      }
      set
      {
        _command.CommandTimeout = value;
      }
    }

    public CommandType CommandType
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

    public IDbConnection Connection
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

    public IDbDataParameter CreateParameter()
    {
      return _command.CreateParameter();
    }

    public int ExecuteNonQuery()
    {
      _logger(this.CommandText);
      return _command.ExecuteNonQuery();
    }

    public IDataReader ExecuteReader(CommandBehavior behavior)
    {
      _logger(this.CommandText);
      return _command.ExecuteReader(behavior);
    }

    public IDataReader ExecuteReader()
    {
      _logger(this.CommandText);
      return _command.ExecuteReader();
    }

    public object ExecuteScalar()
    {
      _logger(this.CommandText);
      return _command.ExecuteScalar();
    }

    public IDataParameterCollection Parameters
    {
      get { return _command.Parameters; }
    }

    public void Prepare()
    {
      _command.Prepare();
    }

    public IDbTransaction Transaction
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

    public UpdateRowSource UpdatedRowSource
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

    public void Dispose()
    {
      _command.Dispose();
    }
  }
}
