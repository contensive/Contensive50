
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass]
    public class genericControllerUnitTests {
        [TestMethod]
        public void Controllers_AddSpan_test1() {
            // arrange
            // act
            string result = genericController.AddSpan("test", "testClass");
            string resultNoClass = genericController.AddSpan("test");
            string resultNeither = genericController.AddSpan();
            // assert
            Assert.AreEqual("<span class=\"testClass\">test</span>", result);
            Assert.AreEqual("<span>test</span>", resultNoClass);
            Assert.AreEqual("<span/>", resultNeither);
        }
    }
}
