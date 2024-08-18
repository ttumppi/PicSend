using PicSend;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicSend.Tests
{
    [TestClass()]
    public class VersionUpdaterTests
    {
        [TestMethod()]
        public void UpdateTest()
        {
            try
            {
                for (int i = 0; i < 30000; i++)
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