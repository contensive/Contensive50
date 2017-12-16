
using Contensive.Core;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace integrationTests {
    [TestClass()]
    public class cpTests {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [TestMethod()]
        public void constructorWithoutApp() {
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
        [TestMethod()]
        public void constructorWithApp() {
            // arrange
            CPClass cp = new CPClass("testapp");
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
        [TestMethod()]
        public void cpExecuteAddontest() {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string addonName = "testAddon" + cp.Utils.GetRandomInteger().ToString();
            int recordId = 0;
            string htmlText = "12345";
            string wysiwygText = "<b>abcde</b>";
            string addonGuid = cp.Utils.CreateGuid();
            string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
            string echoText = "text added to document";
            //
            if (cs.Insert(Contensive.Core.constants.cnAddons)) {
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
            cp.Content.Delete(Contensive.Core.constants.cnAddons, "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [TestMethod()]
        public void cpExecuteRouteTest() {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string addonName = "testAddon" + cp.Utils.GetRandomInteger().ToString();
            int recordId = 0;
            string htmlText = "12345";
            string wysiwygText = "<b>abcde</b>";
            string addonGuid = cp.Utils.CreateGuid();
           // string activeScript = "cp.doc.getText(\"echo\")";
            string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
            string echoText = "text added to document";
            //
            if (cs.Insert(Contensive.Core.constants.cnAddons)) {
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
            //dispose
            cp.Content.Delete(Contensive.Core.constants.cnAddons, "id=" + recordId.ToString());
            cp.Dispose();
        }
    }
}
