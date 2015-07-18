using DaggerNet.DOM;
using System.Collections.Generic;

namespace DaggerNet.Abstract
{
  public abstract class DomProducer
  {
    public abstract HashSet<Table> Produce();
  }
}
