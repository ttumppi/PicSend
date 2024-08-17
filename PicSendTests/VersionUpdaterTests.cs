using PicSend;
using System;
using System.Collections.Generic;
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
                VersionUpdater.Update();

                Assert.IsTrue(true);
            }
            catch (Exception ex)
            {
                Assert.Fail();
            }
           
        }
    }
}