using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Emmola.Helpers;

namespace Dagger.NetTest
{
  [TestClass]
  public class HelperTest
  {
    [TestMethod]
    public void DateTimeShouldBeSimpleType()
    {
      Assert.IsTrue(typeof(DateTime).IsSimpleType());
    }

    [TestMethod]
    public void ClassShouldNotBeSimpleType()
    {
      Assert.IsFalse(typeof(HelperTest).IsSimpleType());
    }
  }
}
