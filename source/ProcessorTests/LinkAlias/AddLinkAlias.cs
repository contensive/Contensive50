
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
    public class Test_LinkAliasController {
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Test_AddLinkAlias_simple() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test", 1, "");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "(name='/test')");

                // assert
                Assert.AreEqual(1, linkAliasList.Count);
                Assert.AreEqual("/test", linkAliasList[0].name);
                Assert.AreEqual(1, linkAliasList[0].pageID);
                Assert.AreEqual("", linkAliasList[0].queryStringSuffix);

            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Adding the same link alias with different pageId - the second replaces the first
        /// </summary>
        [TestMethod]
        public void Test_AddLinkAlias_SameLink_NewPage() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test", 1, "");
                LinkAliasController.addLinkAlias(core, "test", 2, "");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "(name='/test')");
                // assert
                Assert.AreEqual(1, linkAliasList.Count);
                Assert.AreEqual("/test", linkAliasList[0].name);
                Assert.AreEqual(2, linkAliasList[0].pageID);
                Assert.AreEqual("", linkAliasList[0].queryStringSuffix);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add two, same link, same page, different qs -- second overrides first
        /// </summary>
        [TestMethod]
        public void Test_AddLinkAlias_SameLink_SamePage_NewQS() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test", 1, "a=1");
                LinkAliasController.addLinkAlias(core, "test", 1, "a=2");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "(name='/test')");
                // assert
                Assert.AreEqual(1, linkAliasList.Count);
                Assert.AreEqual("/test", linkAliasList[0].name);
                Assert.AreEqual(1, linkAliasList[0].pageID);
                Assert.AreEqual("a=2", linkAliasList[0].queryStringSuffix);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add two, different link, same page, different qs -- link an addon on a page
        /// </summary>
        [TestMethod]
        public void Test_AddLinkAlias_NewLink_SamePage_NewQS() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test1", 1, "a=1");
                LinkAliasController.addLinkAlias(core, "test2", 1, "a=2");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "","id");
                // assert
                Assert.AreEqual(2, linkAliasList.Count);
                //
                Assert.AreEqual("/test1", linkAliasList[0].name);
                Assert.AreEqual(1, linkAliasList[0].pageID);
                Assert.AreEqual("a=1", linkAliasList[0].queryStringSuffix);
                //
                Assert.AreEqual("/test2", linkAliasList[1].name);
                Assert.AreEqual(1, linkAliasList[1].pageID);
                Assert.AreEqual("a=2", linkAliasList[1].queryStringSuffix);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a second link to the same page/qs -> adds the newest as the second as highest id
        /// </summary>
        [TestMethod]
        public void Test_AddLinkAlias_NewLink_SamePage_SameQS() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test1", 1, "a=1");
                LinkAliasController.addLinkAlias(core, "test2", 1, "a=1");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "");
                // assert
                Assert.AreEqual(2, linkAliasList.Count);
                Assert.AreEqual("/test1", linkAliasList[0].name);
                Assert.AreEqual(1, linkAliasList[0].pageID);
                Assert.AreEqual("a=1", linkAliasList[0].queryStringSuffix);
                //
                Assert.AreEqual("/test2", linkAliasList[1].name);
                Assert.AreEqual(1, linkAliasList[1].pageID);
                Assert.AreEqual("a=1", linkAliasList[1].queryStringSuffix);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Re-adding a link moves it to the highest id order
        /// </summary>
        [TestMethod]
        public void Test_AddLinkAlias_ReAddLink_SamePageAndQS() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = cp.core;
                core.db.executeNonQuery("delete from " + Models.Db.LinkAliasModel.contentTableNameLowerCase);
                // act
                LinkAliasController.addLinkAlias(core, "test1", 1, "a=1");
                LinkAliasController.addLinkAlias(core, "test2", 1, "a=1");
                LinkAliasController.addLinkAlias(core, "test1", 1, "a=1");
                var linkAliasList = Models.Db.DbBaseModel.createList<Models.Db.LinkAliasModel>(cp, "");
                // assert
                Assert.AreEqual(2, linkAliasList.Count);
                //
                Assert.AreEqual("/test2", linkAliasList[0].name);
                Assert.AreEqual(1, linkAliasList[0].pageID);
                Assert.AreEqual("a=1", linkAliasList[0].queryStringSuffix);
                //
                Assert.AreEqual("/test1", linkAliasList[1].name);
                Assert.AreEqual(1, linkAliasList[1].pageID);
                Assert.AreEqual("a=1", linkAliasList[1].queryStringSuffix);
            }
        }


    }
}