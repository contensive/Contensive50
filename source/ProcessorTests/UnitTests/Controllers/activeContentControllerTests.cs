
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class activeContentControllerTests {
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_Content() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"content\">";
                // act
                DateTime dateBefore = DateTime.Now.AddSeconds(-1);
                string resultString = Contensive.Processor.Controllers.ActiveContentController.renderHtmlForWeb(
                    core: cp.core,
                    source: source,
                    contextContentName: "",
                    ContextRecordID: 0,
                    ContextContactPeopleID: 0,
                    ProtocolHostString: "",
                    DefaultWrapperID: 0,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                Assert.AreEqual(Contensive.Processor.constants.fpoContentBox,resultString, "The result string was not the content fpo guid [" + resultString + "]");
            }
        }

    }
}