using Microsoft.VisualStudio.TestTools.UnitTesting;
using VersionUpdater;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace VersionUpdater.Tests
{
    [TestClass()]
    public class VersionUpdaterTests
    {
        [TestMethod()]
        public void UpdateTest()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    VersionUpdater.Update();
                    Debug.WriteLine(VersionUpdater.CurrentVersion);
                }


                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
        }
    }
}