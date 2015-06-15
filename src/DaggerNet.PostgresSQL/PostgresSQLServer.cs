using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.PostgresSQL
{
  public class PostgresSQLServer : DbServer
  {
    public PostgresSQLServer(IEnumerable<Type> types, string cntStringFormat, string database)
      : base(types, cntStringFormat, database, new PostgreSQLGenerator())
    {
    }

    protected override IDbConnection CreateConnection(string connectionString)
    {
      return new NpgsqlConnection(connectionString);
    }

    public override void Backup(string path, bool incremental = false)
    {
      throw new NotImplementedException();
    }
  }
}
