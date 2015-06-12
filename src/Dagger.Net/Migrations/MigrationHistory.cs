using Dagger.Net.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dagger.Net.Migrations
{
  public class MigrationHistory
  {
    static readonly string _version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

    public MigrationHistory()
    {
      ProductVersion = _version;
    }

    [PrimaryKey]
    public long Id { get; set; }

    [MaxLength(50)]
    [Required]
    public string Title { get; set; }

    [Required]
    public byte[] Model { get; set; }

    [MaxLength(20)]
    [Required]
    public string ProductVersion { get; set; }
  }
}
