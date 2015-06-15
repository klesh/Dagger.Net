﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNetTest.MigrationModels.v2
{
  public class Blog : Entity
  {
    [MaxLength(50)]
    public string Author { get; set; }
  }
}