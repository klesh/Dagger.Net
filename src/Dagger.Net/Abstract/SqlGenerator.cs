using Dagger.Net.DOM;
using Dagger.Net.Migrations;
using Emmola.Helpers;
using Inflector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dagger.Net.Abstract
{
  /// <summary>
  /// Generate the SQL script
  /// </summary>
  public abstract class SqlGenerator
  {
    public const string DELETION_SEPARATOR = "/*--- DELETION SEPARATION LINE , DO NOT DELETE, YOU CAN ADD YOUR OWN CONVERTION BELOW ---*/";
    private StringBuilder _builder;

    /// <summary>
    /// return a CREATE DATABASE script 
    /// </summary>
    /// <param name="dbname">name of database to be created</param>
    /// <returns></returns>
    public virtual string GetCreateDatabaseSql(string dbname)
    {
      return "CREATE DATABASE {0}".FormatMe(Quote(dbname));
    }

    /// <summary>
    /// return a DROP DATABASE script
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public virtual string GetDropDatabaseSql(string dbname)
    {
      return "DROP DATABASE {0}".FormatMe(Quote(dbname));
    }

    /// <summary>
    /// return sql script to set database in Single User Model
    /// </summary>
    /// <param name="dbname">name of database to be droped</param>
    /// <returns></returns>
    public abstract string GetSingleUserModeSql(string dbname);

    /// <summary>
    /// return sql to select inserted id
    /// </summary>
    /// <returns></returns>
    public abstract string GetInsertIdSql();

    /// <summary>
    /// Generate create data base script
    /// </summary>
    /// <param name="name"></param>
    public virtual string CreateTables(IEnumerable<Table> tables)
    {
      _builder = new StringBuilder();
      foreach (var table in tables)
      {
        Begin(true).CreateTable(table).End();
      }

      foreach (var table in tables)
      {
        if (!table.ForeignKeys.Any()) 
          continue;

        foreach (var foreignKey in table.ForeignKeys)
        {
          Begin().AddForeignKey(foreignKey).End();
        }
      }
      return _builder.ToString();
    }

    /// <summary>
    /// Create migration
    /// </summary>
    /// <param name="from">Old one</param>
    /// <param name="to">New one</param>
    public virtual string CreateMigration(IEnumerable<Table> from, IEnumerable<Table> to)
    {
      _builder = new StringBuilder();

      // divide migration script to two parts, creation and deletion, so that we can do convertion inbetween the process.
      var colDeletion = new List<Column>(); // delay column deletion
      var keyCreation = new List<PrimaryKey>(); // delay key constraints
      var indexCreation = new List<Index>(); // delay index creation
      var fkCreation = new List<ForeignKey>(); // delay foreign key creation

      var tableDiff = from.Diff(to);

      foreach (var table in tableDiff.Creation)
        Begin(true).CreateTable(table);

      foreach (var pair in tableDiff.Comparison)
      {
        if (pair.Key.Name != pair.Value.Name)
          Begin(true).RenameTable(pair.Key, pair.Value).End();

        // migrate column
        var columnDiff = pair.Key.Columns.Diff(pair.Value.Columns);
        foreach (var column in columnDiff.Creation)
          Begin().AddColumn(column).End();

        foreach (var colPair in columnDiff.Comparison)
        {
          if (colPair.Key.Name != colPair.Value.Name)
          {
            Begin().RenameColumn(colPair.Key, colPair.Value).End();
            colPair.Key.NewName = colPair.Value.Name;
          }

          if (colPair.Key.CalculateSimilarity(colPair.Value) < 1f)
            Begin().ChangeColumn(colPair.Value).End();
        }

        colDeletion.AddRange(columnDiff.Deletion);

        // migrate primar key
        var oldPk = pair.Key.PrimaryKey.Columns.Select(pko => pko.Value.Name).Implode(", ");
        var newPk = pair.Value.PrimaryKey.Columns.Select(pko => pko.Value.Name).Implode(", ");
        if (newPk != oldPk) // column changed
        {
          Begin().DropConstraint(pair.Key.PrimaryKey).End();
          keyCreation.Add(pair.Value.PrimaryKey);
        }
        else if (pair.Key.PrimaryKey.Name != pair.Value.PrimaryKey.Name) // name changed
        {
          Begin().RenameConstraint(pair.Key.PrimaryKey, pair.Value.PrimaryKey).End();
        }

        // migrate indexes
        var indexDiff = pair.Key.Indexes.Diff(pair.Value.Indexes);

        foreach (var indexPair in indexDiff.Comparison)
          if (indexPair.Key.Name != indexPair.Value.Name)
            Begin().RenameIndex(indexPair.Key, indexPair.Value).End();

        foreach (var index in indexDiff.Deletion)
          Begin().DropIndex(index).End();

        indexCreation.AddRange(indexDiff.Creation);

        // migrate foreign keys
        var fkDiff = pair.Key.ForeignKeys.Diff(pair.Value.ForeignKeys);
        foreach (var fk in fkDiff.Deletion)
          Begin().DropConstraint(fk).End();

        foreach (var fkPair in fkDiff.Comparison)
          if (fkPair.Key.Name != fkPair.Value.Name)
            Begin().RenameConstraint(fkPair.Key, fkPair.Value).End();

        fkCreation.AddRange(fkDiff.Creation);
      }

      _builder.AppendLine().AppendLine();
      _builder.Append(DELETION_SEPARATOR);
      _builder.AppendLine().AppendLine();

      foreach (var table in tableDiff.Deletion)
        Begin().DropTable(table).End();

      foreach (var key in keyCreation)
        Begin().AddPrimaryKey(key).End();

      foreach (var index in indexCreation)
        Begin().CreateIndex(index).End();

      foreach (var column in colDeletion)
        Begin().DropColumn(column).End();

      foreach (var fk in fkCreation)
        Begin().AddForeignKey(fk).End();

      return _builder.ToString();
    }

    protected virtual SqlGenerator Begin(bool bigGap = false)
    {
      if (_builder.Length > 0)
      {
        _builder.AppendLine();
        if (bigGap)
          _builder.AppendLine();
      }
      return this;
    }

    protected virtual SqlGenerator End()
    {
      _builder.Append(";");
      return this;
    }

    /// <summary>
    /// Generate create table script
    /// </summary>
    /// <param name="table">Table DOM</param>
    public virtual SqlGenerator CreateTable(Table table)
    {
      const string spaces = "    ";
      _builder.AppendFormat("CREATE TABLE {0} (", QuoteTable(table));
      var first = true;
      foreach (var column in table.Columns)
      {
        if (first)
          first = false;
        else
          _builder.Append(",");
        _builder.AppendLine().Append(spaces);
        CreateColumn(column);
      }
      _builder.AppendLine().Append(")");

      End();

      if (table.PrimaryKey != null)
        Begin().AddPrimaryKey(table.PrimaryKey).End();

      foreach (var index in table.Indexes)
        Begin().CreateIndex(index).End();

      return this;
    }

    public virtual SqlGenerator AddPrimaryKey(PrimaryKey primaryKey)
    {
      _builder.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} PRIMARY KEY ({2})",
        QuoteTable(primaryKey.Table),
        Quote(primaryKey.Name),
        primaryKey.Columns.Select(c => Quote(c.Value.Name)).Implode(", "));

      return this;
    }

    public virtual SqlGenerator DropConstraint(PrimaryKey primaryKey)
    {
      _builder.AppendLine();
      _builder.AppendFormat("ALTER TABLE {0} DROP CONSTRAINT {1}",
        QuoteTable(primaryKey.Table),
        Quote(primaryKey.Name));
      return this;
    }

    public virtual SqlGenerator RenameConstraint(PrimaryKey from, PrimaryKey to)
    {
      _builder.AppendFormat("ALTER TABLE {0} RENAME CONSTRAINT {1} TO {2}", 
        QuoteTable(from.Table),
        Quote(from.Name), 
        Quote(to.Name));
      return this;
    }

    /// <summary>
    /// Generate drop table script
    /// </summary>
    /// <param name="table">Table to drop</param>
    public virtual SqlGenerator DropTable(Table table)
    {
      _builder.AppendFormat("DROP TABLE {0}", QuoteTable(table));
      return this;
    }

    /// <summary>
    /// Generate rename talbe script
    /// </summary>
    /// <param name="from">Old one</param>
    /// <param name="to">New one</param>
    public virtual SqlGenerator RenameTable(Table from, Table to)
    {
      _builder.AppendFormat("ALTER TABLE {0} RENAME TO {1}", QuoteTable(from), QuoteTable(to));
      return this;
    }

    public virtual SqlGenerator CreateColumn(Column column)
    {
      _builder.Append(Quote(column.Name)).AppendSpaced(MapType(column));

      if (column.NotNull)
        _builder.AppendSpaced("NOT NULL");

      if (column.Default.IsValid())
        _builder.AppendSpaced("DEFAULT").AppendSpaced(column.Default);

      return this;
    }

    /// <summary>
    /// Generate add new column script
    /// </summary>
    /// <param name="column">new column to add</param>
    public virtual SqlGenerator AddColumn(Column column)
    {
      _builder.AppendFormat("ALTER TABLE {0} ADD COLUMN {1}", QuoteTable(column.Table));
      CreateColumn(column);
      return this;
    }

    /// <summary>
    /// Generate drop column script
    /// </summary>
    /// <param name="column"></param>
    public virtual SqlGenerator DropColumn(Column column)
    {
      _builder.AppendFormat("ALTER TABLE {0} DROP COLUMN {1}", QuoteTable(column.Table), QuoteColumn(column));
      return this;
    }

    /// <summary>
    /// Generate rename column script
    /// </summary>
    /// <param name="from">Old one</param>
    /// <param name="to">New one</param>
    public virtual SqlGenerator RenameColumn(Column from, Column to)
    {
      _builder.AppendFormat("ALTER TABLE {0} RENAME COLUMN {1} TO {2}", QuoteTable(from.Table), QuoteColumn(from), QuoteColumn(to));
      return this;
    }

    /// <summary>
    /// Generate change column definition script
    /// </summary>
    /// <param name="column">Column to be changed</param>
    public virtual SqlGenerator ChangeColumn(Column column)
    {
      _builder.AppendFormat("ALTER TABLE {0} ALTER COLUMN {1}", QuoteTable(column.Table));
      CreateColumn(column);
      return this;
    }

    /// <summary>
    /// Generate create index script
    /// </summary>
    /// <param name="index">Index DOM</param>
    public virtual SqlGenerator CreateIndex(Index index)
    {
      _builder.Append("CREATE");
      if (index.Unique)
        _builder.AppendSpaced("UNIQUE");
      _builder.AppendSpaced("INDEX");
      _builder.AppendSpaced(Quote(index.Name));
      _builder.AppendSpaced("ON");
      _builder.AppendSpaced(QuoteTable(index.Table));
      _builder.AppendSpaced("(");
      _builder.AppendSpaced(index.Columns.Select(c => 
        Quote(c.Value.Name) + ( c.Descending ? " DESC" : "" )
      ).Implode(", "));
      _builder.AppendSpaced(")");
      return this;
    }

    
    /// <summary>
    /// Generate drop index script
    /// </summary>
    /// <param name="index">index to drop</param>
    public virtual SqlGenerator DropIndex(Index index)
    {
      _builder.AppendFormat("DROP INDEX {0}", Quote(index.Name));
      return this;
    }

    /// <summary>
    /// Generate rename index script
    /// </summary>
    /// <param name="from">Old one</param>
    /// <param name="to">New one</param>
    public virtual SqlGenerator RenameIndex(Index from, Index to)
    {
      _builder.AppendFormat("ALTER INDEX {0} RENAME TO {1}", Quote(from.Name), Quote(to.Name));
      return this;
    }

    /// <summary>
    /// Generate foreign key script
    /// </summary>
    /// <param name="foreignKey">ForeignKey to create</param>
    public virtual SqlGenerator AddForeignKey(ForeignKey foreignKey)
    {
      _builder.AppendFormat("ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY ({2}) REFERENCES {3} ({4})",
        QuoteTable(foreignKey.Table),
        Quote(foreignKey.Name),
        foreignKey.Columns.Select(c => QuoteColumn(c.Value)).Implode(", "),
        QuoteTable(foreignKey.ReferTable),
        foreignKey.ReferColumns.Select(c => QuoteColumn(c)).Implode(", ")
        );
      return this;
    }

    /// <summary>
    /// Table name convertion.
    /// </summary>
    /// <param name="table">table to be quoted</param>
    /// <returns>Quoted table name</returns>
    public virtual string QuoteTable(Table table)
    {
      return Quote(table.Name.Pluralize());
    }

    /// <summary>
    /// Column name convertion.
    /// </summary>
    /// <param name="column">Column to be quoted</param>
    /// <returns>Quoted column name</returns>
    public virtual string QuoteColumn(Column column)
    {
      return Quote(column.Name);
    }

    /// <summary>
    /// Quote a database object
    /// </summary>
    /// <param name="sqlObject"></param>
    /// <returns>Quoted object</returns>
    public abstract string Quote(string sqlObject);

    /// <summary>
    /// Return database type string for a column
    /// </summary>
    /// <param name="column">Column DOM</param>
    /// <returns>string</returns>
    public abstract string MapType(Column column);

    /// <summary>
    /// Output scripts
    /// </summary>
    /// <returns>Sql scripts</returns>
    public override string ToString()
    {
      return _builder.Append(";").ToString();
    }

    /// <summary>
    /// Return quoted text like 'User''s note'
    /// </summary>
    /// <param name="text">text</param>
    /// <returns></returns>
    public virtual string QuoteText(string text)
    {
      return "'{0}'".FormatMe(text.Replace("'", "''"));
    }
  }
}
