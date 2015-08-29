using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using DaggerNet.Linq;
using System.Data.Common;
using System.ComponentModel;
using DaggerNet.Abstract;
using System.Linq;
using Emmola.Helpers;

namespace DaggerNet
{
  /// <summary>
  /// Dagger represent a wrapped DbConnection
  /// Provide Insert/Update/Delete and a StrongType lambda query
  /// Connection must be Open, then we can run transaction during dagger lifecycle.
  /// </summary>
  [DesignerCategory("")]
  public class Dagger : DbConnection
  {
    private DbConnection _connection;

    public Dagger(DataBase database, DbConnection connection)
    {
      if (database == null)
        throw new Exception("Database can not be null");

      if (connection == null)
        throw new Exception("An active connection can not be null");

      Base = database;
      Model = database.Model;
      Sql = database.Sql;
      Manager = database.Manager;
      Logger = database.Logger;
      _connection = connection;
    }
    
    public DataBase Base { get; protected set; }

    public DataModel Model { get; protected set; }

    public SqlGenerator Sql { get; protected set; }

    public SqlManager Manager { get; protected set; }

    public Action<string> Logger { get; set; }

    /// <summary>
    /// Insert entity to database, support partial insert by anomynous type.
    /// may need to set non-nullable column defaul value
    /// </summary>
    /// <typeparam name="T">Table Type</typeparam>
    /// <param name="entity">parameters</param>
    public virtual int Insert<T>(object entity)
    {
      var wasClosed = _connection.State == ConnectionState.Closed;
      if (wasClosed) _connection.Open();

      var tableType = typeof(T);
      var actualType = entity.GetType();
      var table = Model.GetTable(tableType);
      var sql = Manager.GetInsertSQL(table, actualType);

      int affected = this.Execute(sql, entity);
      if (table.IdColumn != null && tableType == actualType)
      {
        var insertId = this.ExecuteScalar(Sql.GetInsertIdSql());
        table.IdColumn.SetMethod.Invoke(entity, new object[] { insertId });
      }

      if (wasClosed) _connection.Close();
      return affected;
    }

    public virtual async Task<int> InsertAsync<T>(object entity)
    {
      return await Task.Run(() => Insert<T>(entity));
    }

    /// <summary>
    /// Update entity, support partial update by anomynous type
    /// </summary>
    /// <typeparam name="T">Table type</typeparam>
    /// <param name="entity">parameters</param>
    public virtual int Update<T>(object entity)
    {
      var table = Model.GetTable(typeof(T));
      var sql = Manager.GetUpdateSQL(table, entity.GetType());
      var affected = this.Execute(sql, entity);
      if (affected == 1)
      {
        var updateTimeCol = table.Columns.FirstOrDefault(c => c.UpdateTime);
        if (updateTimeCol != null)
        { // refresh update time.
          var updateTimeSql = Manager.GetUpdateTimeSql(table);
          var now = DateTime.Now;

          var dynamicParam = new DynamicParameters(entity);
          dynamicParam.Add("_UPDATE_TIME_", now);
          var ret2 = this.Execute(updateTimeSql, dynamicParam);
          if (ret2 == 1)
          {
            if (entity is T)
              updateTimeCol.SetMethod.Invoke(entity, new object[] { now });
          }
          else
          {
            throw new Exception("UpdateTime failure on :" + table.Name);
          }
        }
      }
      else if (table.Columns.Any(c => c.Name == "RowVersion"))
      {
        throw new DBConcurrencyException("Update failure due to RowVersion change by other session. Table: " + table.Name);
      }
      return affected;
    }

    public virtual async Task<int> UpdateAsync<T>(object entity)
    {
      return await Task.Run(() => Update<T>(entity));
    }

    public virtual int UpdateCell(string propertyName, object propertyValue, string typeName, params object[] primarkeyValues)
    {
      if (!primarkeyValues.Any())
        throw new ArgumentNullException("primaryKey values can not be empty");

      var table = Model.Tables.First(t => t.Name.IcEquals(typeName));
      if (primarkeyValues.Length != table.PrimaryKey.Columns.Count)
        throw new ArgumentException("primaryKey values are not match");

      var column = table.Columns.First(c => c.Name.IcEquals(propertyName));
      if (!column.PropertyInfo.PropertyType.IsNullableType())
        propertyValue = Convert.ChangeType(propertyValue, column.PropertyInfo.PropertyType);
      var primaryKeys = table.PrimaryKey.Columns.Select(c => c.Value).ToArray();
      var primaryValues = Enumerable.Range(0, primarkeyValues.Length).
        Select(index => 
          Convert.ChangeType(primarkeyValues[index], primaryKeys[index].PropertyInfo.PropertyType)).ToArray();

      var param = new DynamicParameters();

      var sql = "UPDATE {0} SET {1} = {2} WHERE {3}".FormatMe(
        Sql.QuoteTable(table),
        Sql.QuoteColumn(column), 
        "@" + column.Name,
        primaryKeys.Select(pk => "{0} = {1}".FormatMe(Sql.QuoteColumn(pk), "@" + pk.Name)).Implode(" AND ")
        );
      param.Add(column.Name, propertyValue);
      for (int i = 0, j = primaryKeys.Length; i < j; i++)
      {
        param.Add(primaryKeys[i].Name, primaryValues[i]);
      }
      var affected = this.Execute(sql, param);
      if (affected == 1)
      {
        var updateTimeSql = Manager.GetUpdateTimeSql(table);
        if (updateTimeSql.IsValid())
        {
          var updateTimeParams = new DynamicParameters(param);
          updateTimeParams.Add("_UPDATE_TIME_", DateTime.Now);
          this.Execute(updateTimeSql, updateTimeParams);
        }
      }
      return affected;
    }

    public virtual async Task<int> UpdateCellAsync(string propertyName, object propertyValue, string typeName, params object[] primaryKeyValues)
    {
      return await Task.Run(() => UpdateCell(propertyName, propertyValue, typeName, primaryKeyValues));
    }

    /// <summary>
    /// Delete entity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="entity"></param>
    public virtual int Delete<T>(object entity)
    {
      return this.Execute(Manager.GetDeleteSQL(Model.GetTable(typeof(T)), entity.GetType()), entity);
    }

    public virtual async Task<int> DeleteAsync<T>(object entity)
    {
      return await Task.Run(() => Delete<T>(entity));
    }

    //public virtual async Task<IEnumerable<T>> FromAsync<T>(Func<SqlBuilder<T>, string> buildSql, object param = null)
    //  where T : class, new()
    //{
    //  return await this.QueryAsync<T>(buildSql(new SqlBuilder<T>(Model, Sql)), param);
    //}

    public virtual FluentRoot<T> By<T>(T parameters)
      where T : class
    {
      return new FluentRoot<T>(this, parameters);
    }

    public virtual FluentFrom<object, T> From<T>(Func<SqlBuilder<T>, object> buildSql)
      where T : class, new()
    {
      return new FluentRoot<object>(this, null).From<T>((_b, _p) => buildSql(_b));
    }

    public virtual FluentFrom<object, T> From<T>()
      where T : class, new()
    {
      return new FluentRoot<object>(this, null).From<T>((_b, _p) => _b);
    }

    /// <summary>
    /// Create a SqlBuilder instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public SqlBuilder<T> BuildSql<T>()
      where T : class
    {
      return new SqlBuilder<T>(Model, Sql);
    }

    /// <summary>
    /// Execute sql script directory without Dapper to avoid sql cache
    /// </summary>
    /// <param name="sql">sql script</param>
    /// <returns>affected rows</returns>
    public virtual int ExecuteNonQuery(string sql)
    {
      using (var cmd = this.CreateCommand())
      {
        cmd.CommandText = sql;
        return cmd.ExecuteNonQuery();
      }
    }

    #region DbConnection Wrapper

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
      return _connection.BeginTransaction(isolationLevel);
    }

    public override void ChangeDatabase(string databaseName)
    {
      _connection.ChangeDatabase(databaseName);
    }

    public override void Close()
    {
      _connection.Close();
    }

    public override string ConnectionString
    {
      get
      {
        return _connection.ConnectionString;
      }
      set
      {
        _connection.ConnectionString = value;
      }
    }

    protected override DbCommand CreateDbCommand()
    {
      return new Sheath(_connection.CreateCommand(), Logger);
    }

    public override string DataSource
    {
      get { return _connection.DataSource; }
    }

    public override string Database
    {
      get { return _connection.Database; }
    }

    public override void Open()
    {
      _connection.Open();
    }

    public override string ServerVersion
    {
      get { return _connection.ServerVersion; }
    }

    public override ConnectionState State
    {
      get { return _connection.State; }
    }

    #endregion
  }
}
