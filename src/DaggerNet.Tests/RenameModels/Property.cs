﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DaggerNetTest.RenameModels
{
  public class Property : Entity
  {
    public HashSet<Product> Products { get; set; }
  }
}