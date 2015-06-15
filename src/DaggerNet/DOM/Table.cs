using DaggerNet.DOM.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using Emmola.Helpers;
using Emmola.Helpers.Interfaces;

namespace DaggerNet.DOM
{
  [Serializable]
  public class Table : Base, ISimilarity<Table>
  {
    public Table()
    {
      Columns = new HashSet<Column>();
      Indexes = new HashSet<Index>();
      ForeignKeys = new HashSet<ForeignKey>();
    }

    public Table(string name) 
      : this()
    {
      Name = name;
    }

    internal Table(Type type)
      : this()
    {
      // TODO: Complete member initialization
      this.Type = type;
      this.Name = type.Name;
    }

    /// <summary>
    /// many to many relation table
    /// </summary>
    /// <param name="m2m">Ordered 2 types</param>
    public Table(Type[] m2m)
      :this()
    {
      ManyToMany = m2m;
    }

    /// <summary>
    /// Rlative type
    /// </summary>
    internal Type Type { get; private set; }

    /// <summary>
    /// As in many-to-many
    /// </summary>
    internal Type[] ManyToMany { get; private set; }

    /// <summary>
    /// We have an auto incremental id column in most case.
    /// </summary>
    internal Column IdColumn { get; set; }

    /// <summary>
    /// Columns
    /// </summary>
    public HashSet<Column> Columns { get; set; }

    /// <summary>
    /// Indexes
    /// </summary>
    public HashSet<Index> Indexes { get; set; }

    /// <summary>
    /// Foreign Keys
    /// </summary>
    public HashSet<ForeignKey> ForeignKeys { get; set; }

    /// <summary>
    /// Return primary key
    /// </summary>
    public PrimaryKey PrimaryKey { get; set; }

    /// <summary>
    /// Calculate similary to other Table
    /// </summary>
    /// <param name="other">Other Table</param>
    /// <returns>similarity</returns>
    public float CalculateSimilarity(Table other)
    {
      return this.Columns.SimilarityTo(other.Columns);
    }
  }
}
