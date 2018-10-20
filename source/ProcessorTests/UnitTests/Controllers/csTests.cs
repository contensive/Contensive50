
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contensive.BaseClasses;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    // file types to test...
    // integer
    // text
    // longtext
    // bool
    // date
    // file
    // lookup
    // redirect
    // currency
    // fileText
    // fileImage
    // float (double)
    // autoIncrement
    // manytomany
    // memberselect
    // filecss
    // filexml
    // filejavascript
    // link
    // resourceLink
    // html
    // filehtml
    [TestClass()]
    public class csTests {
        //
        //====================================================================================================
        /// <summary>
        /// insert record, set (no-save), read, verify
        /// </summary>
        [TestMethod()]
        public void csTests_InsertSetReadVerify() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                CPCSBaseClass csAddon = cp.CSNew();
                if (!csAddon.Insert(Processor.Models.Db.AddonModel.contentName)) Assert.Fail("Insert addon failed");
                // integer
                csAddon.SetField("IconHeight", 123);
                Assert.AreEqual(123, csAddon.GetInteger("IconHeight"));
                // text
                csAddon.SetField("name", "abcd");
                Assert.AreEqual("abcd", csAddon.GetText("name"));
                // longtext
                csAddon.SetField("ScriptingCode", "abcde");
                Assert.AreEqual("abcde", csAddon.GetText("ScriptingCode"));
                // bool
                csAddon.SetField("IsInline", true);
                Assert.AreEqual(true, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", false);
                Assert.AreEqual(false, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", 1);
                Assert.AreEqual(true, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", 0);
                Assert.AreEqual(false, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", "true");
                Assert.AreEqual(true, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", "false");
                Assert.AreEqual(false, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", "on");
                Assert.AreEqual(true, csAddon.GetBoolean("IsInline"));
                csAddon.SetField("IsInline", "off");
                Assert.AreEqual(false, csAddon.GetBoolean("IsInline"));
                // date
                csAddon.SetField("ProcessNextRun", new DateTime(1000, 2, 3));
                Assert.AreEqual(new DateTime(1000, 2, 3), csAddon.GetDate("ProcessNextRun"));
                csAddon.SetField("ProcessNextRun", null);
                Assert.AreEqual(DateTime.MinValue, csAddon.GetDate("ProcessNextRun"));
                csAddon.SetField("ProcessNextRun", new DateTime(2000, 3, 4));
                Assert.AreEqual(new DateTime(2000, 3, 4), csAddon.GetDate("ProcessNextRun"));
                csAddon.SetField("ProcessNextRun", DateTime.MinValue);
                Assert.AreEqual(DateTime.MinValue, csAddon.GetDate("ProcessNextRun"));
                // file
                // todo - find example of this field type
                //
                // lookup
                int baseCollectionId = cp.Content.GetRecordID("Add-on Collections", "Base5");
                Assert.AreNotEqual(0, baseCollectionId, "Base5 add-on collection Id cannot be 0.");
                csAddon.SetField("CollectionID", baseCollectionId);
                Assert.AreEqual(baseCollectionId, csAddon.GetInteger("collectionId"));
                Assert.AreEqual("Base5", csAddon.GetText("collectionId"), "cs.getText of a lookup field returns the name of the referenced record");
                // redirect
                // todo - find example of this field type
                //
                // currency
                // todo - find example of this field type
                //
                // fileText
                CPCSBaseClass csGroups = cp.CSNew();
                if (!csGroups.Insert(Processor.Models.Db.GroupModel.contentName)) Assert.Fail("Insert Groups failed");
                csGroups.SetField("CopyFilename", "abcdef");
                Assert.AreEqual("abcdef", csGroups.GetText("CopyFilename"));
                csGroups.Close();
                // fileImage
                // todo - find example of this field type
                //
                // float (double)
                CPCSBaseClass csState = cp.CSNew();
                if (!csState.Insert(Processor.Models.Db.StateModel.contentName)) Assert.Fail("Insert states failed");
                csState.SetField("SalesTax", 0.045);
                Assert.AreEqual(0.045, csState.GetNumber("SalesTax"));
                csState.Close();
                //
                // autoIncrement
                // todo - find example of this field type
                //
                // manytomany
                // todo - find example of this field type
                //
                // memberselect
                // todo - find example of this field type
                //
                // filecss
                // todo - find example of this field type
                //
                // filexml
                // todo - find example of this field type
                //
                // filejavascript
                // todo - find example of this field type
                //
                // link
                // todo - find example of this field type
                //
                // resourceLink
                // todo - find example of this field type
                //
                // html
                // todo - find example of this field type
                //
                // filehtml
                // todo - find example of this field type
                //
                // act
                // assert
            }
        }

    }
}