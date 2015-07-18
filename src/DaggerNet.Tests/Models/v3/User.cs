using System;
using System.ComponentModel.DataAnnotations;

namespace DaggerNet.Tests.Models.v3
{
  public class User : Entity
  {
    [MaxLength(50)]
    public string Login { get; set; }

    [MaxLength(50)]
    public string Fullname { get; set; }

    public DateTime? LastLogin { get; set; }
  }
}
