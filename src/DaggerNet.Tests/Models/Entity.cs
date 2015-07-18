using DaggerNet.Attributes;
using System;

namespace DaggerNet.Tests.Models
{
  public abstract class Entity
  {
    [PrimaryKey(AutoIncrement = true, Descending = true)]
    public virtual long Id { get; set; }

    public string Title { get; set; }

    [Default("CURRENT_TIMESTAMP")]
    public DateTime? CreatedAt { get; set; }

    [UpdateTime]
    public DateTime? UpdatedAt { get; set; }
  }
}