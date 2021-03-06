﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaggerNet.Attributes
{
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
  public class ReferenceAttribute : Attribute
  {
    public ReferenceAttribute()
    {
    }

    public ReferenceAttribute(Type refType)
    {
      ReferenceType = refType;
    }

    /// <summary>
    /// Refered type.
    /// </summary>
    public Type ReferenceType { get; set; }

    /// <summary>
    /// on delete cascade
    /// </summary>
    public Cascades OnDelete { get; set; }

    /// <summary>
    /// On update cascade
    /// </summary>
    public Cascades OnUpdate { get; set; }

    /// <summary>
    /// on delete 
    /// </summary>
    public bool SetNull { get; set; }

    /// <summary>
    /// For composite primary key
    /// </summary>
    public int Order { get; set; }
  }

  public enum Cascades
  {
    Auto,
    NoAction,
    Cascade,
    Restrict,
    SetNull,
    SetDefault
  }
}