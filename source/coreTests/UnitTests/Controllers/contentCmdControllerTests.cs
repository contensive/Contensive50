﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contensive.Core.Controllers;
using Contensive.Core.Models.DbModels;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass()]
    public class contentCmdControllerTests {
        [TestMethod()]
        public void Controllers_ContentCmd_ExecuteCmd_simpleAddonTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                addonModel addon = addonModel.add(cp.core);
                addon.name = "testaddon" + genericController.GetRandomInteger(cp.core).ToString() ;
                addon.CopyText = "foo";
                addon.save(cp.core);
                string cmd = "<div>{% \"" + addon.name + "\" %}</div>";
                BaseClasses.CPUtilsBaseClass.addonContext context = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = Contensive.Core.Controllers.contentCmdController.ExecuteCmd(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div>foo</div>", result);
                //throw new NotImplementedException();
            }
        }
        [TestMethod()]
        public void Controllers_ContentCmd_ExecuteCmd_executeAddonTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                addonModel addon = addonModel.add(cp.core);
                addon.name = "testaddon" + genericController.GetRandomInteger(cp.core).ToString();
                addon.CopyText = "foo";
                addon.save(cp.core);
                string cmd = "<div class=\"sample\">{% {\"addon\":{\"addon\":\"" + addon.name + "\"}} %}</div>";
                BaseClasses.CPUtilsBaseClass.addonContext context = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = Contensive.Core.Controllers.contentCmdController.ExecuteCmd(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div class=\"sample\">foo</div>", result);
                //throw new NotImplementedException();
            }
        }
        [TestMethod()]
        public void Controllers_ContentCmd_ExecuteCmd_executeAddonTest2() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                addonModel addon = addonModel.add(cp.core);
                addon.name = "testaddon" + genericController.GetRandomInteger(cp.core).ToString();
                addon.CopyText = "foo$insert$";
                addon.save(cp.core);
                string cmd = "<div class=\"sample\">{% {\"addon\":{\"addon\":\"" + addon.name + "\",\"insert\":\"bar\"}} %}</div>";
                BaseClasses.CPUtilsBaseClass.addonContext context = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = Contensive.Core.Controllers.contentCmdController.ExecuteCmd(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div class=\"sample\">foobar</div>", result);
                //throw new NotImplementedException();
            }
        }

    }
}