
using Contensive.Processor;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Views {
    [TestClass()]
    public class cpTests {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [TestMethod]
        public void Views_cp_ConstructorWithoutApp() {
            // arrange
            CPClass cp = new CPClass();
            // act
            bool clusterOK = cp.serverOk;
            bool appOK = cp.appOk;
            // assert
            Assert.AreEqual(clusterOK, true);
            Assert.AreEqual(appOK, false);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// Test 2 - cp ok with application
        /// </summary>
        [TestMethod]
        public void Views_cp_ConstructorWithApp() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            // act
            bool clusterOK = cp.serverOk;
            bool appOK = cp.appOk;
            // assert
            Assert.AreEqual(clusterOK, true);
            Assert.AreEqual(appOK, true);

            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [TestMethod]
        public void Views_cp_ExecuteAddontest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            string addonName = "testAddon-1-" + cp.Utils.GetRandomInteger().ToString();
            int recordId = 0;
            string htmlText = "12345";
            string wysiwygText = "<b>abcde</b>";
            string addonGuid = cp.Utils.CreateGuid();
            string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
            string echoText = "text added to document";
            //
            if (cs.Insert(Contensive.Processor.Models.Db.AddonModel.contentName)) {
                recordId = cs.GetInteger("id");
                cs.SetField("name", addonName);
                cs.SetField("copytext", htmlText);
                cs.SetField("copy", wysiwygText);
                cs.SetField("ccGuid", addonGuid);
                cs.SetField("scriptingcode", activeScript);
            }
            cs.Close();
            cp.Doc.SetProperty("echo", echoText);
            // act
            // assert
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(addonName));
            //
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(addonGuid));
            //
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(recordId.ToString()));
            //dispose
            cp.Content.Delete(Contensive.Processor.Models.Db.AddonModel.contentName, "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [TestMethod]
        public void Views_cp_ExecuteRouteTest() {
            // todo - this method fails when run with all the tests, passes when run alone. Might be the route cache not clearing after change
            // arrange
            // todo - change all create/dispose for cp and core to using to make sure everything disposes
            using (CPClass cp = new CPClass(testAppName)) {
                // todo -- make cs constructor for most common cs.open cases, add dispose, then use using pattern (saves steps)
                CPCSBaseClass cs = cp.CSNew();
                string addonName = "testAddon-2-" + cp.Utils.GetRandomInteger().ToString();
                int recordId = 0;
                string htmlText = "12345";
                string wysiwygText = "<b>abcde</b>";
                string addonGuid = cp.Utils.CreateGuid();
                // string activeScript = "cp.doc.getText(\"echo\")";
                string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
                string echoText = "text added to document";
                //
                if (cs.Insert(Contensive.Processor.Models.Db.AddonModel.contentName)) {
                    recordId = cs.GetInteger("id");
                    cs.SetField("name", addonName);
                    cs.SetField("copytext", htmlText);
                    cs.SetField("copy", wysiwygText);
                    cs.SetField("ccGuid", addonGuid);
                    cs.SetField("scriptingcode", activeScript);
                    cs.SetField("remotemethod", "1");
                }
                cs.Close();
                cp.Doc.SetProperty("echo", echoText);
                // act
                string result = cp.executeRoute(addonName);
                // assert
                Assert.AreEqual(htmlText + wysiwygText + echoText, result);
            }
        }
    }
}
