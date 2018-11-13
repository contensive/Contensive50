
using Contensive.Processor;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Views {
    [TestClass()]
    public class cpUserTests {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [TestMethod]
        public void Views_cpUser_GetDate() {
            using (var cp = new CPClass(testAppName)) {
                // arrange
                string testInput = DateTime.MinValue.ToString();
                cp.User.SetProperty("testDate", DateTime.MinValue.ToString());
                // act
                DateTime testoutput = cp.User.GetDate("testDate");
                // assert
                Assert.AreEqual(DateTime.MinValue, testoutput);
            }
        }
    }
}
