﻿
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
    public class addonControllerTests {
        [TestMethod()]
        //
        //====================================================================================================
        //
        public void Controllers_Addon_simpleDoNothingAddon() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var addon = Processor.Models.Db.AddonModel.add(cp.core);
                addon.save(cp.core);
                // act
                string result = cp.core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                    backgroundProcess = false,
                    cssContainerClass = "",
                    cssContainerId = "",
                    errorContextMessage = "",
                    forceHtmlDocument = false,
                    forceJavascriptToHead = false,
                    hostRecord = new BaseClasses.CPUtilsBaseClass.addonExecuteHostRecordContext() {
                        contentName = "",
                        fieldName = "",
                        recordId = 0
                    },
                    instanceArguments = new Dictionary<string, string>(),
                    instanceGuid = "",
                    isIncludeAddon = false,
                    personalizationAuthenticated = false,
                    personalizationPeopleId = 0,
                    wrapperID = 0
                });
                // assert
                Assert.AreEqual( "", result );
                Assert.AreEqual(0, cp.core.doc.htmlAssetList.Count);
                Assert.AreEqual(false, cp.core.doc.isHtml);
                Assert.AreEqual("", cp.core.doc.htmlForEndOfBody);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_Description.Count);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_KeyWordList.Count);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_OtherTags.Count);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_TitleList.Count);
            }
        }
        //
        //====================================================================================================
        //
        public void Controllers_Addon_copy() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var addon = Processor.Models.Db.AddonModel.add(cp.core);
                addon.copy = "test" + genericController.GetRandomInteger(cp.core).ToString();
                addon.save(cp.core);
                // act
                string result = cp.core.addon.execute(addon, new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                    addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                    backgroundProcess = false,
                    cssContainerClass = "",
                    cssContainerId = "",
                    errorContextMessage = "",
                    forceHtmlDocument = false,
                    forceJavascriptToHead = false,
                    hostRecord = new BaseClasses.CPUtilsBaseClass.addonExecuteHostRecordContext() {
                        contentName = "",
                        fieldName = "",
                        recordId = 0
                    },
                    instanceArguments = new Dictionary<string, string>(),
                    instanceGuid = "",
                    isIncludeAddon = false,
                    personalizationAuthenticated = false,
                    personalizationPeopleId = 0,
                    wrapperID = 0
                });
                // assert
                Assert.AreEqual(addon.copy, result);
                Assert.AreEqual(0, cp.core.doc.htmlAssetList.Count);
                Assert.AreEqual(false, cp.core.doc.isHtml);
                Assert.AreEqual("", cp.core.doc.htmlForEndOfBody);
                Assert.AreEqual("", cp.core.doc.htmlMetaContent_Description);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_KeyWordList.Count);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_OtherTags.Count);
                Assert.AreEqual(0, cp.core.doc.htmlMetaContent_TitleList.Count);
            }
        }

    }
}