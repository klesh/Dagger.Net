using DaggerNet.Abstract;
using Emmola.Helpers;
using Npgsql;
using System.Data.Common;

namespace DaggerNet.Postgres
{
  public class PostgresServer : DataServer
  {
    protected override SqlGenerator GetSqlGenerator()
    {
      return new PostgresGenerator();
    }

    protected override DbConnection CreateConnection(string connectionString)
    {
      return new NpgsqlConnection(connectionString);
    }

    protected override long GetDatabaseSize(string name)
    {
      using (var cnt = Open(name))
      {
        var cmd = cnt.CreateCommand();
        cmd.CommandText = "SELECT pg_database_size('{0}');".FormatMe(name);
        return (long)cmd.ExecuteScalar();
      }
    }
  }
}