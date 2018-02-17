
using Contensive.Core;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Views {
    [TestClass]
    public class cpSecurityTests {
        //====================================================================================================
        /// <summary>
        /// coreSecurity
        /// </summary>
        [TestMethod]
        public void Controllers_Security_EncryptDecrypt() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int testNumber = 12345;
            DateTime testDate = new DateTime(1990, 8, 7);
            //
            // act
            //
            string token = Core.Controllers.securityController.encodeToken(cp.core,testNumber, testDate);
            //
            // assert
            //
            int resultNumber = 0;
            DateTime resultDate = DateTime.MinValue;
            Core.Controllers.securityController.decodeToken(cp.core,token, ref resultNumber, ref resultDate);
            Assert.AreEqual(testNumber, resultNumber);
            Assert.AreEqual(testDate, resultDate);
            //
            // dispose
            //
            cp.Dispose();
        }
    }
}
