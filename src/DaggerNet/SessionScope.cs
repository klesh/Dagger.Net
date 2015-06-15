using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet
{
  public class SessionScope : IDisposable
  {
    private IDbConnection _connection;

    public SessionScope(IDbConnection connection)
    {
      _connection = connection;
      if (_connection.State != ConnectionState.Open)
        _connection.Open();
    }

    public void Dispose()
    {
      _connection.Close();
    }
  }

  public static class SessionScopeExtension
  {
    public static SessionScope OpenScope(this IDbConnection connection)
    {
      return new SessionScope(connection);
    }
  }
}
