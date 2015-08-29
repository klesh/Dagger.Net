using DaggerNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Tests.Models.v3
{
  public class PostContributor : IEntity
  {
    [PrimaryKey(Order = 0)]
    [Reference(typeof(Post))]
    public long PostId { get; set; }

    [PrimaryKey(Order = 1)]
    [Reference(typeof(User))]
    public long UserId { get; set; }
  }
}
