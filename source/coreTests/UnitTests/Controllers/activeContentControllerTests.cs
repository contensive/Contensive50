
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
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_date() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"Date\">";
                // act
                DateTime dateBefore = DateTime.Now.AddSeconds(-1);
                string resultString = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    ContextContentName: "",
                    ContextRecordID: 0,
                    ContextContactPeopleID: 0,
                    ProtocolHostString: "",
                    DefaultWrapperID: 0,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                DateTime dateAfter = DateTime.Now.AddSeconds(1);
                // assert
                DateTime dateResult;
                Assert.IsTrue(DateTime.TryParse(resultString, out dateResult),"The result string was not a date [" + dateResult + "]");
                Assert.IsTrue(dateBefore.CompareTo(dateResult) <= 0,"The date returned was before the start of the test, return: [" + dateResult + "], test start [" + dateBefore + "]");
                Assert.IsTrue(dateAfter.CompareTo(dateResult) >= 0, "The date returned was after the end of the test, return: [" + dateResult + "], test end [" + dateAfter + "]");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_memberName() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<Ac Type=\"Member\" Field=\"Name\">";
                string testPersonName = "test" + genericController.GetRandomInteger(cp.core);
                var testPerson = Contensive.Core.Models.DbModels.personModel.add(cp.core);
                testPerson.name = testPersonName;
                testPerson.save(cp.core);
                cp.User.LoginByID(testPerson.id);
                // act
                DateTime dateBefore = DateTime.Now;
                string resultString = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                Assert.AreEqual(testPersonName, resultString);
            }
        }


        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_organizationName() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<Ac Type=\"Organization\" Field=\"Name\">";
                string testOrgName = "testOrg" + genericController.GetRandomInteger(cp.core);
                var testOrg = Contensive.Core.Models.DbModels.organizationModel.add(cp.core);
                testOrg.name = testOrgName;
                testOrg.save(cp.core);
                var testPerson = Contensive.Core.Models.DbModels.personModel.add(cp.core);
                testPerson.OrganizationID = testOrg.id;
                string testPersonName = "testPerson" + genericController.GetRandomInteger(cp.core);
                testPerson.name = testPersonName;
                testPerson.save(cp.core);
                cp.User.LoginByID(testPerson.id);

                // act
                DateTime dateBefore = DateTime.Now;
                string resultString = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                Assert.AreEqual(testOrgName, resultString);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_visit() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"visit\" Field=\"Name\">";
                // act
                DateTime dateBefore = DateTime.Now;
                string resultString = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                // will be deprecated
                Assert.AreNotEqual(source, resultString);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_ActiveContent_visitor() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string source = "<ac Type=\"visitor\" Field=\"Name\">";
                // act
                DateTime dateBefore = DateTime.Now;
                string resultString = Contensive.Core.Controllers.activeContentController.renderHtmlForWeb(
                    core: cp.core,
                    Source: source,
                    addonContext: BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                );
                // assert
                // will be deprecated, but old content might include it
                Assert.AreNotEqual(source, resultString);
            }
        }



    }
}