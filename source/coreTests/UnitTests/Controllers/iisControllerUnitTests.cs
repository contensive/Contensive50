
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using Microsoft.Web.Administration;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    [TestClass]
    public class iisControllerUnitTests {
        [TestMethod]
        public void Controllers_iis_verifyAppPool_test1() {
            // arrange
            string appPoolName = "testAppPool";
            using (ServerManager serverManager = new ServerManager()) {
                // act
                using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass()) {
                    WebServerController.verifyAppPool(cp.core, appPoolName);
                }
            }
            // assert
            Assert.AreEqual("", "");
        }
    }
}
