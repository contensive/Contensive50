
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using Microsoft.Web.Administration;

namespace coreTests {
    [TestClass]
    public class iisControllerUnitTests {
        [TestMethod]
        public void verifyAppPool_test1() {
            // arrange
            string appPoolName = "testAppPool";
            using (ServerManager serverManager = new ServerManager()) {
                // act
                using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass()) {
                    iisController.verifyAppPool(cp.core, appPoolName);
                }
            }
            // assert
            Assert.AreEqual("", "");
        }
    }
}
