
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class securityControllerTests {

        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_blank() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_twoWayEncode() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string source = "All work and no play makes johnny a dull boy.";
                // act
                string resultEncrypted = SecurityController.twoWayEncrypt(cp.core, source);
                string resultDecrypted = SecurityController.twoWayDecrypt(cp.core, resultEncrypted);
                string blankEncrypted = SecurityController.twoWayEncrypt(cp.core, "");
                string blankDecrypted = SecurityController.twoWayDecrypt(cp.core, "");
                string invalidDecrypted = SecurityController.twoWayDecrypt(cp.core, source);
                // assert
                Assert.AreEqual(source, resultDecrypted);
                Assert.AreNotEqual("", blankEncrypted);
                Assert.AreEqual("", blankDecrypted);
                Assert.AreEqual("", invalidDecrypted);
            }
        }

    }
}