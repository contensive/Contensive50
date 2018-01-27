
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
    public class activeContentControllerTests {
        [TestMethod()]
        //
        //====================================================================================================
        //
        public void Controllers_ActiveContent_blank() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"Date\">";
                // act
                string result = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    ContextContentName: "",
                    ContextRecordID: 0,
                    ContextContactPeopleID: 0,
                    ProtocolHostString: "",
                    DefaultWrapperID: 0,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );

                    
                // assert
                Assert.Fail("activeContentControllerTests Not Implemented");
            }
        }

    }
}