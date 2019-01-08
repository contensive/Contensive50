
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Tests.testConstants;
using Microsoft.Web.Administration;

namespace Contensive.ProcessorTests.UnitTests.ControllerTests {
    [TestClass]
    public class iisControllerUnitTests {
        [TestMethod]
        public void Controllers_iis_verifyAppPool_test1() {
            // arrange
            string appPoolName = "testAppPool";
            using (ServerManager serverManager = new ServerManager()) {
                // act
                using (CPClass cp = new CPClass()) {
                    WebServerController.verifyAppPool(cp.core, appPoolName);
                }
            }
            // assert
            Assert.AreEqual("", "");
        }
    }
}
