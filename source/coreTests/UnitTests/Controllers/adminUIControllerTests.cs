
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class adminUIControllerTests {
        [TestMethod()]
        //
        //====================================================================================================
        //
        public void Controllers_Addon_blank() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.Fail("adminUIControllerTests Not Implemented");
            }
        }

    }
}