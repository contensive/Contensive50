
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tests.testConstants;
using Contensive.Processor;

namespace Contensive.ProcessorTests.UnitTests.ControllerTests {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class _blank {
        [TestMethod]
        //
        //====================================================================================================
        //
        public void controllers_blank() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }

    }
}