using DaggerNet.DOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaggerNet.Abstract
{
  public abstract class DomProducer
  {
    public abstract HashSet<Table> Produce();
  }
}
