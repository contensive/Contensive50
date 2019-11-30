
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tests.TestConstants;
using Contensive.Processor;
using Contensive.BaseClasses;

namespace Contensive.ProcessorTests.UnitTests.ControllerTests {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class activeContentControllerTests {
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_ActiveContent_Content() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"content\">";
                // act
                DateTime dateBefore = DateTime.Now.AddSeconds(-1);
                string resultString = ActiveContentController.renderHtmlForWeb(
                    core: cp.core,
                    source: source,
                    contextContentName: "",
                    ContextRecordId: 0,
                    deprecated_ContextContactPeopleId: 0,
                    ProtocolHostString: "",
                    DefaultWrapperId: 0,
                    addonContext: CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                Assert.AreEqual(Constants.fpoContentBox,resultString, "The result string was not the content fpo guid [" + resultString + "]");
            }
        }

    }
}