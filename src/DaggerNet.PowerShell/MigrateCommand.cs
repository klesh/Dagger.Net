using DaggerNet.DOM;
using DaggerNet.Migrations;
using Emmola.Helpers;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DaggerNet.PowerShell
{
  /// <summary>
  /// psm1 script created an AppDomain with baseDir to build folder and private bin path to this assembly tools path.
  /// </summary>
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

    /// <summary>
    /// Find DbServer inheritance class and instanciate to fetch the database model
    /// </summary>
    /// <param name="name"></param>
    public void Execute(string name)
    {
      if (!name.IsValid())
        throw new ArgumentException("Migration name could not be empty");

      var assembly = Assembly.Load(Project.GetTargetName());
      var dataFactoryTypes = typeof(DataFactory).FindSubTypesIn(assembly);
      if (!dataFactoryTypes.Any())
        throw new Exception("None of DbServer sub classs found, please create an class inherit from DbServer to perform migrate");

      if (dataFactoryTypes.Count() > 1)
        throw new Exception("More than one DbServer sub classes found!");

      var dataFactoryType = dataFactoryTypes.First();

      var dataFactory = (DataFactory)Activator.CreateInstance(dataFactoryType);

      MigrationHistory lastMigration;
      using (var dagger = dataFactory.Draw())
      {
        lastMigration = dagger.From<MigrationHistory>(s => s.OrderByDescending(mh => mh.Id).Limit(1)).FirstOrDefault();
      }

      var lastModel = ObjectHelper.FromBinary<IEnumerable<Table>>(lastMigration.Model);
      var sqlScript = dataFactory.Server.Sql.CreateMigration(lastModel, dataFactory.Model.Tables);
      var csTemplate = Assembly.GetExecutingAssembly().GetResourceText("DaggerNet.PowerShell.Templates.Migration.cs");
      var rootNameSpace = Project.GetRootNamespace();
      var migrationId = Migration.GenerateId().ToString();
      csTemplate = csTemplate.Replace("_nameSpace_", rootNameSpace)
        .Replace("_migrationName_", name)
        .Replace("_migrationId_", migrationId)
        .Replace("_factoryType_", dataFactoryType.FullName);

      var csPath = @"Migrations\{0}_{1}.cs".FormatMe(migrationId, name);
      var sqlPath = @"Migrations\{0}_{1}.sql".FormatMe(migrationId, name);
      var csFile = Project.AddFile(csPath, csTemplate);
      Project.AddSubResource(csFile, sqlPath, sqlScript);
      AppDomain.CurrentDomain.SetData("created", Path.Combine(Project.GetProjectDir(), csPath));
    }

    public static void TestHold()
    {
      // For DaggerNet.Tests to load this assembly
    }
  }
}
