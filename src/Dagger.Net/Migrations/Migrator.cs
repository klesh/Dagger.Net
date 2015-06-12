using Dagger.Net.DOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;

namespace Dagger.Net.Migrations
{
  /// <summary>
  /// Responsible for create a new database or update database structure, and even delete one
  /// </summary>
  public class Migrator
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbname">Database to be migrated</param>
    /// <param name="types">All entity types</param>
    /// <param name="connection">Connection to default database with privillege to Create/Remove/Update database</param>
    /// <param name="generator">Dialected sql generator</param>
    public Migrator()
    {
    }
  }
}
