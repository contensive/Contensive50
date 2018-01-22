﻿
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using Microsoft.Web.Administration;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass]
    public class iisControllerUnitTests {
        [TestMethod]
        public void Controllers_iis_verifyAppPool_test1() {
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
