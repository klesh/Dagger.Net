using DaggerNet.Abstract;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Emmola.Helpers;
using Dapper;
using DaggerNet.Migrations;
using DaggerNet.DOM;

namespace DaggerNet.PowerShell
{
  public class MigrateCommand
  {
    public MigrateCommand(string name)
    {
      Execute(name);
    }

    protected Project Project
    {
      get
      {
        return (Project)AppDomain.CurrentDomain.GetData("project");
      }
    }

    public void Execute(string name)
    {
      if (!name.IsValid())
        throw new ArgumentException("Migration name could not be empty");

      var assembly = Assembly.LoadFile(Project.GetTargetPath());
      var dbServerTypes = typeof(DbServer).FindSubTypesIn(assembly);

      if (!dbServerTypes.Any())
        throw new Exception("None of DbServer sub classs found, please create an class inherit from DbServer to perform migrate");

      if (dbServerTypes.Count() > 1)
        throw new Exception("More than one DbServer sub classes found!");

      var dbServerType = dbServerTypes.First();

      var dbServer = (DbServer)Activator.CreateInstance(dbServerType);

      MigrationHistory lastMigration;
      using (var dagger = dbServer.Draw())
      {
        lastMigration = dagger.From<MigrationHistory>(s => s.OrderByDescending(mh => mh.Id).Limit(1)).First();
      }
      var lastModel = ObjectHelper.FromBinary<IEnumerable<Table>>(lastMigration.Model);
      var sqlScript = dbServer.Generator.CreateMigration(lastModel, dbServer.Model);
      var csTemplate = Assembly.GetExecutingAssembly().GetResourceText("DaggerNet.PowerShell.Templates.Migration");
      var defaultNs = Project.Properties.Item("DefaultNamespace").Value.ToString();
      var nameSpace = "{0}.Migrations".FormatMe(defaultNs);
      var migrationId = Migration.GenerateId().ToString();
      var csPath = "{0}.{1}.cs".FormatMe(defaultNs, name);
      var sqlPath = "{0}.{1}.sql".FormatMe(defaultNs, name);
      csTemplate = csTemplate.Replace("_nameSpace_", nameSpace).Replace("_migrationName_", name).Replace("_migrationId_", migrationId);

      Project.AddFile(csPath, csTemplate);
      Project.AddSubResource(csPath, sqlPath, sqlScript);
    }
  }
}
