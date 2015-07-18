using DaggerNet.DOM;
using DaggerNet.Migrations;
using System;
using System.Collections.Generic;
using System.Linq;
using Emmola.Helpers;
using DaggerNet.Abstract;

namespace DaggerNet
{
  /// <summary>
  /// Holds data tables, provide get table by type functionality
  /// </summary>
  public class DataModel
  {
    private IDictionary<string, Table> _tables;

    /// <summary>
    /// Find all subtypes in baseType's assembly as Models, and link up to concreate dataserver
    /// </summary>
    /// <param name="baseType">entity base type</param>
    /// <param name="dataServer">concreate data server</param>
    public DataModel(Type baseType)
    {
      _tables = new Class2Dom(baseType.FindSubTypesInIt()).AddType<MigrationHistory>()
        .Produce().ToDictionary(
          t => t.Type == null ? GetTableKey(t.ManyToMany) : GetTableKey(t.Type),
          t => t
        );
    }

    /// <summary>
    /// Make table key for Table or Many-to-Many Table
    /// </summary>
    /// <param name="types">Type/Two Types(M2M)</param>
    /// <returns>Table Key</returns>
    public string GetTableKey(params Type[] types)
    {
      if (!types.Any() || types.Length > 2)
        throw new ArgumentException("Accept only One or Two types");

      return types.Length == 1 ? types[0].Name : types.Select(t => t.Name).OrderBy(n => n).Implode("|");
    }


    /// <summary>
    /// Return table for Type or Types(many-to-many)
    /// </summary>
    public Table GetTable(params Type[] types)
    {
      var tableKey = GetTableKey(types);

      if (!_tables.ContainsKey(tableKey))
        throw new Exception("Table not found for " + tableKey);

      return _tables[tableKey];
    }

    /// <summary>
    /// Return tables
    /// </summary>
    public IEnumerable<Table> Tables
    {
      get { return _tables.Values; }
    }
  }
}
