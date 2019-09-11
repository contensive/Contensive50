﻿
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contensive.Processor.Controllers;

using static Tests.testConstants;
using Contensive.BaseClasses;
using Contensive.Processor;

namespace Contensive.ProcessorTests.UnitTests.ControllerTests {
    [TestClass()]
    public class contentCmdControllerTests {
        //
        /// <summary>
        /// simple syntax, one command, run addon
        /// </summary>
        [TestMethod]
        public void contentCmdController_SimpleSyntax_runAddon() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addon = AddonModel.addEmpty(cp.core);
                addon.name = "testaddon-4-" + GenericController.GetRandomInteger(cp.core).ToString() ;
                addon.copyText = "foo";
                addon.save(cp.core);
                string cmd = "<div>{% \"" + addon.name + "\" %}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div>foo</div>", result);
            }
        }
        //
        /// <summary>
        /// simple syntax, two commands, run addon
        /// </summary>
        [TestMethod]
        public void contentCmdController_SimpleSyntax_mulipleContextSwitch() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addonFoo = AddonModel.addEmpty(cp.core);
                addonFoo.name = "addonFoo" + GenericController.GetRandomInteger(cp.core).ToString();
                addonFoo.copyText = "foo";
                addonFoo.save(cp.core);
                //
                AddonModel addonBar = AddonModel.addEmpty(cp.core);
                addonBar.name = "addonBar" + GenericController.GetRandomInteger(cp.core).ToString();
                addonBar.copyText = "Bar";
                addonBar.save(cp.core);
                string cmd = "<div>{% \"" + addonFoo.name + "\" %}{% \"" + addonBar.name + "\" %}+{% \"" + addonFoo.name + "\" %}\n{% \"" + addonBar.name + "\" %}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div>fooBar+foo\nBar</div>", result);
            }
        }
        //
        /// <summary>
        /// Multiple Command Syntax, one command, {%[{"command1":"commandArgument"},{"command2":"commandArgument"}]%}
        /// </summary>
        [TestMethod]
        public void contentCmdController_SimpleSyntax_CommandList() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addonFoo = AddonModel.addEmpty(cp.core);
                addonFoo.name = "addonFoo" + GenericController.GetRandomInteger(cp.core).ToString();
                addonFoo.copyText = "foo";
                addonFoo.save(cp.core);
                //
                AddonModel addonBar = AddonModel.addEmpty(cp.core);
                addonBar.name = "addonBar" + GenericController.GetRandomInteger(cp.core).ToString();
                addonBar.copyText = "Bar";
                addonBar.save(cp.core);
                string cmd = "<div>{%[{\"" + addonFoo.name + "\":\"commandArgument\"}]%}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div>foo</div>", result);
                //throw new NotImplementedException();
            }
        }
        //
        /// <summary>
        /// Multiple Command Syntax, two commands, {%[{"command1":"commandArgument"},{"command2":"commandArgument"}]%}
        /// </summary>
        [TestMethod]
        public void contentCmdController_SimpleSyntax_CommandList_MultipleCommands() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addonFoo = AddonModel.addEmpty(cp.core);
                addonFoo.name = "addonFoo" + GenericController.GetRandomInteger(cp.core).ToString();
                addonFoo.copyText = "foo";
                addonFoo.save(cp.core);
                //
                AddonModel addonBar = AddonModel.addEmpty(cp.core);
                addonBar.name = "addonBar" + GenericController.GetRandomInteger(cp.core).ToString();
                addonBar.copyText = "$cmdAccumulator$Bar";
                addonBar.save(cp.core);
                string cmd = "<div>{%[{\"" + addonFoo.name + "\":\"commandArgument\"},{\"" + addonBar.name + "\":\"commandArgument\"}]%}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div>fooBar</div>", result);
            }
        }
        //
        /// <summary>
        /// json syntax, one command, run addon
        /// </summary>
        [TestMethod]
        public void contentCmdController_JsonSyntax() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addonFoo = AddonModel.addEmpty(cp.core);
                addonFoo.name = "addonFoo" + GenericController.GetRandomInteger(cp.core).ToString();
                addonFoo.copyText = "foo";
                addonFoo.save(cp.core);
                string cmd = "<div class=\"sample\">{% {\"addon\":{\"addon\":\"" + addonFoo.name + "\"}} %}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div class=\"sample\">foo</div>", result);
            }
        }
        //
        /// <summary>
        /// json syntax, one command, run addon
        /// </summary>
        [TestMethod]
        public void contentCmdController_JsonSyntax_WithArgument() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addonFoo = AddonModel.addEmpty(cp.core);
                addonFoo.name = "addonFoo" + GenericController.GetRandomInteger(cp.core).ToString();
                addonFoo.copyText = "foo$ReplaceMe$";
                addonFoo.save(cp.core);
                string cmd = "<div class=\"sample\">{% {\"addon\":{\"addon\":\"" + addonFoo.name + "\",\"ReplaceMe\":\"BAR\"}} %}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div class=\"sample\">fooBAR</div>", result);
            }
        }
        //
        /// <summary>
        /// json syntax, one command, run addon
        /// </summary>
        [TestMethod]
        public void Controllers_JsonSyntax_executeAddonTest_argumentReplacement() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                AddonModel addon = AddonModel.addEmpty(cp.core);
                addon.name = "testaddon-3-" + GenericController.GetRandomInteger(cp.core).ToString();
                addon.copyText = "foo$insert$";
                addon.save(cp.core);
                string cmd = "<div class=\"sample\">{% {\"addon\":{\"addon\":\"" + addon.name + "\",\"insert\":\"bar\"}} %}</div>";
                CPUtilsBaseClass.addonContext context = CPUtilsBaseClass.addonContext.ContextSimple;
                // act
                string result = ContentCmdController.executeContentCommands(cp.core, cmd, context, 0, false);
                // assert
                Assert.AreEqual("<div class=\"sample\">foobar</div>", result);
                //throw new NotImplementedException();
            }
        }

    }
}