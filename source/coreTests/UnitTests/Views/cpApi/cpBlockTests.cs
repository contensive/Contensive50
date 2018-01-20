
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Contensive.Core;
using static Contensive.Core.constants;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using Contensive.BaseClasses;

namespace Contensive.Core.Tests.UnitTests.Views {

    [TestClass()]
    public class cpBlockTests {
        //
        public const string layoutContent = "content";
        public const string layoutA = "<div id=\"aid\" class=\"aclass\">" + layoutContent + "</div>";
        public const string layoutB = "<div id=\"" + rnPageId + "\" class=\"bclass\">" + layoutA + "</div>";
        public const string layoutC = "<div id=\"cid\" class=\"cclass\">" + layoutB + "</div>";
        public const string templateHeadTag = "<meta http-equiv=\"content-type\" content=\"text/html; charset=utf-8\" >";
        public const string templateA = "<html><head>" + templateHeadTag + "</head><body>" + layoutC + "</body></html>";
        //
        //====================================================================================================
        // unit test - cp.blockNew
        //
        [TestMethod()]
        public void Views_cpBlock_InnerOuterTest() {
            // arrange
            CPClass cpApp = new CPClass("testapp");
            CPBlockBaseClass block = cpApp.BlockNew();
            int layoutInnerLength = layoutA.Length;
            // act
            block.Load(layoutC);
            // assert
            Assert.AreEqual(block.GetHtml(), layoutC);
            //
            Assert.AreEqual(block.GetInner("#aid"), layoutContent);
            Assert.AreEqual(block.GetInner(".aclass"), layoutContent);
            //
            Assert.AreEqual(block.GetOuter("#aid"), layoutA);
            Assert.AreEqual(block.GetOuter(".aclass"), layoutA);
            //
            Assert.AreEqual(block.GetInner("#bid"), layoutA);
            Assert.AreEqual(block.GetInner(".bclass"), layoutA);
            //
            Assert.AreEqual(block.GetOuter("#bid"), layoutB);
            Assert.AreEqual(block.GetOuter(".bclass"), layoutB);
            //
            Assert.AreEqual(block.GetInner("#cid"), layoutB);
            Assert.AreEqual(block.GetInner(".cclass"), layoutB);
            //
            Assert.AreEqual(block.GetOuter("#cid"), layoutC);
            Assert.AreEqual(block.GetOuter(".cclass"), layoutC);
            //
            Assert.AreEqual(block.GetInner("#cid .bclass"), layoutA);
            Assert.AreEqual(block.GetInner(".cclass #bid"), layoutA);
            //
            Assert.AreEqual(block.GetOuter("#cid .bclass"), layoutB);
            Assert.AreEqual(block.GetOuter(".cclass #bid"), layoutB);
            //
            Assert.AreEqual(block.GetInner("#cid .aclass"), layoutContent);
            Assert.AreEqual(block.GetInner(".cclass #aid"), layoutContent);
            //
            Assert.AreEqual(block.GetOuter("#cid .aclass"), layoutA);
            Assert.AreEqual(block.GetOuter(".cclass #aid"), layoutA);
            //
            block.Clear();
            Assert.AreEqual(block.GetHtml(), "");
            //
            block.Clear();
            block.Load(layoutA);
            block.SetInner("#aid", "1234");
            Assert.AreEqual(block.GetHtml(), layoutA.Replace(layoutContent, "1234"));
            //
            block.Load(layoutB);
            block.SetOuter("#aid", "1234");
            Assert.AreEqual(block.GetHtml(), layoutB.Replace(layoutA, "1234"));
            //
            // dispose
            cpApp.Dispose();
        }
        //
        //====================================================================================================
        /// <summary>
        /// test block load and clear
        /// </summary>
        [TestMethod()]
        public void Views_cpBlock_ClearTest() {
            CPClass cp = new CPClass("testapp");
            CPBlockBaseClass block = cp.BlockNew();
            // act
            block.Load(layoutC);
            Assert.AreEqual(layoutC, block.GetHtml());
            block.Clear();
            // assert
            Assert.AreEqual(block.GetHtml(), "");
        }
        //
        //====================================================================================================
        /// <summary>
        /// test block import
        /// </summary>
        [TestMethod()]
        public void Views_cpBlock_ImportFileTest() {
            CPClass cp = new CPClass("testapp");
            string filename = "cpBlockTest" + GetRandomInteger(cp.core).ToString() + ".html";
            try {
                CPBlockBaseClass block = cp.BlockNew();
                // act
                cp.core.appRootFiles.saveFile(filename, templateA);
                block.ImportFile(filename);
                // assert
                Assert.AreEqual(layoutC, block.GetHtml());
            } catch (Exception) {
                //
            } finally {
                cp.core.appRootFiles.deleteFile(filename);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// test block openCopy
        /// </summary>
        [TestMethod()]
        public void Views_cpBlock_OpenCopyTest() {
            CPClass cp = new CPClass("testapp");
            string recordName = "cpBlockTest" + GetRandomInteger(cp.core).ToString();
            int recordId = 0;
            try {
                // arrange
                CPBlockBaseClass block = cp.BlockNew();
                CPCSBaseClass cs = cp.CSNew();
                // act
                if (cs.Insert("copy content")) {
                    recordId = cs.GetInteger("id");
                    cs.SetField("name", recordName);
                    cs.SetField("copy", layoutC);
                }
                cs.Close();
                block.OpenCopy(recordName);
                // assert
                Assert.AreEqual(layoutC, block.GetHtml());
            } finally {
                cp.Content.Delete("copy content", "id=" + recordId);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// test block openFile
        /// </summary>
        [TestMethod()]
        public void Views_cpBlock_OpenFileTest() {
            CPClass cp = new CPClass("testapp");
            string filename = "cpBlockTest" + GetRandomInteger(cp.core).ToString() + ".html";
            try {
                CPBlockBaseClass block = cp.BlockNew();
                // act
                cp.core.appRootFiles.saveFile(filename, layoutA);
                block.OpenFile(filename);
                // assert
                Assert.AreEqual(layoutA, block.GetHtml());
            } finally {
                cp.core.appRootFiles.deleteFile(filename);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// test block openCopy
        /// </summary>
        [TestMethod()]
        public void Views_cpBlock_OpenLayoutTest() {
            CPClass cp = new CPClass("testapp");
            string recordName = "cpBlockTest" + GetRandomInteger(cp.core).ToString();
            int recordId = 0;
            try {
                // arrange
                CPBlockBaseClass block = cp.BlockNew();
                CPCSBaseClass cs = cp.CSNew();
                // act
                if (cs.Insert("layouts")) {
                    recordId = cs.GetInteger("id");
                    cs.SetField("name", recordName);
                    cs.SetField("layout", layoutC);
                }
                cs.Close();
                block.OpenLayout(recordName);
                // assert
                Assert.AreEqual(layoutC, block.GetHtml());
            } finally {
                cp.Content.Delete("layouts", "id=" + recordId);
            }
        }
        //
        //====================================================================================================
        // unit test - cp.blockNew
        //
        [TestMethod()]
        public void Views_cpBlock_AppendPrependTest() {
            // arrange
            CPClass cpApp = new CPClass("testapp");
            CPBlockBaseClass block = cpApp.BlockNew();
            int layoutInnerLength = layoutA.Length;
            // act
            block.Clear();
            block.Append("1");
            block.Append("2");
            Assert.AreEqual(block.GetHtml(), "12");
            //
            block.Clear();
            block.Prepend("1");
            block.Prepend("2");
            Assert.AreEqual(block.GetHtml(), "21");
            //
            // dispose
            //
            cpApp.Dispose();
        }
        //
    }
    //
    public class coreCommonTests {
        //
        [TestMethod()]
        public void normalizePath_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual(fileController.normalizePath(""), "");
            Assert.AreEqual(fileController.normalizePath("c:\\"), "c:\\");
            Assert.AreEqual(fileController.normalizePath("c:\\test\\"), "c:\\test\\");
            Assert.AreEqual(fileController.normalizePath("c:\\test"), "c:\\test\\");
            Assert.AreEqual(fileController.normalizePath("c:\\test/test"), "c:\\test\\test\\");
            Assert.AreEqual(fileController.normalizePath("test"), "test\\");
            Assert.AreEqual(fileController.normalizePath("\\test"), "test\\");
            Assert.AreEqual(fileController.normalizePath("\\test\\"), "test\\");
            Assert.AreEqual(fileController.normalizePath("/test/"), "test\\");
        }
        //
        [TestMethod()]
        public void normalizeRoute_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual(normalizeRoute("TEST"), "/test");
            Assert.AreEqual(normalizeRoute("\\TEST"), "/test");
            Assert.AreEqual(normalizeRoute("\\\\TEST"), "/test");
            Assert.AreEqual(normalizeRoute("test"), "/test");
            Assert.AreEqual(normalizeRoute("/test/"), "/test");
            Assert.AreEqual(normalizeRoute("test/"), "/test");
            Assert.AreEqual(normalizeRoute("test//"), "/test");
        }
        //
        [TestMethod()]
        public void encodeText_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual(encodeText(1), "1");
        }
        //
        [TestMethod()]
        public void sample_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual(true, true);
        }
        //
        [TestMethod()]
        public void dateToSeconds_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual(dateToSeconds(new DateTime(1900, 1, 1)), 0);
            Assert.AreEqual(dateToSeconds(new DateTime(1900, 1, 2)), 86400);
        }
        //
        [TestMethod()]
        public void ModifyQueryString_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual("", ModifyQueryString("", "a", "1", false));
            Assert.AreEqual("a=1", ModifyQueryString("", "a", "1", true));
            Assert.AreEqual("a=1", ModifyQueryString("a=0", "a", "1", false));
            Assert.AreEqual("a=1", ModifyQueryString("a=0", "a", "1", true));
            Assert.AreEqual("a=1&b=2", ModifyQueryString("a=1", "b", "2", true));
        }
        //
        [TestMethod()]
        public void ModifyLinkQuery_unit() {
            // arrange
            // act
            // assert
            Assert.AreEqual("index.html", modifyLinkQuery("index.html", "a", "1", false));
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html", "a", "1", true));
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html?a=0", "a", "1", false));
            Assert.AreEqual("index.html?a=1", modifyLinkQuery("index.html?a=0", "a", "1", true));
            Assert.AreEqual("index.html?a=1&b=2", modifyLinkQuery("index.html?a=1", "b", "2", true));
        }
        //
        [TestMethod()]
        public void vbInstr_test() {
            //genericController.vbInstr(1, Link, "?")
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("d") + 1, vbInstr("abcdefgabcdefgabcdefgabcdefg", "d"));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("E") + 1, vbInstr("abcdefgabcdefgabcdefgabcdefg", "E"));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("E", 9) + 1, vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E"));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("E", 9) + 1, vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", 2));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("E", 9, System.StringComparison.OrdinalIgnoreCase) + 1, vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", 1));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("c", 9) + 1, vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", 2));
            Assert.AreEqual("abcdefgabcdefgabcdefgabcdefg".IndexOf("c", 9, System.StringComparison.OrdinalIgnoreCase) + 1, vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", 1));
            string haystack = "abcdefgabcdefgabcdefgabcdefg";
            string needle = "c";
            Assert.AreEqual("?".IndexOf("?") + 1, vbInstr(1, "?", "?"));
            //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of haystack.Length for every iteration:
            int tempVar = haystack.Length;
            for (int ptr = 1; ptr <= tempVar; ptr++) {
                Assert.AreEqual(haystack.IndexOf(needle, ptr - 1) + 1, vbInstr(ptr, haystack, needle, 2));
            }
        }
        //
        [TestMethod()]
        public void vbUCase_test() {
            Assert.AreEqual("AbCdEfG".ToUpper(), vbUCase("AbCdEfG"));
            Assert.AreEqual("ABCDEFG".ToUpper(), vbUCase("ABCDEFG"));
            Assert.AreEqual("abcdefg".ToUpper(), vbUCase("abcdefg"));
        }
        //
        [TestMethod()]
        public void vbLCase_test() {
            Assert.AreEqual("AbCdEfG".ToLower(), vbLCase("AbCdEfG"));
            Assert.AreEqual("ABCDEFG".ToLower(), vbLCase("ABCDEFG"));
            Assert.AreEqual("abcdefg".ToLower(), vbLCase("abcdefg"));
        }
        //
        [TestMethod()]
        public void vbLeft_test() {
            Assert.AreEqual("AbCdEfG".ToLower(), vbLCase("AbCdEfG"));
        }
    }
    //
    //====================================================================================================
    // unit tests
    //
    [TestClass()]
    public class CPClassUnitTests {
        //
        //====================================================================================================
        // unit test - cp.addVar
        //
        [TestMethod()]
        public void Views_cpBlock_AddVar_unit() {
            // arrange
            CPClass cp = new CPClass();
            CPClass cpApp = new CPClass("testapp");
            // act
            cp.AddVar("a", "1");
            cp.AddVar("b", "2");
            cp.AddVar("b", "3");
            cpApp.AddVar("a", "4");
            cpApp.AddVar("b", "5");
            for (var ptr = 1; ptr <= 10; ptr++) {
                cpApp.AddVar("key" + ptr.ToString(), "value" + ptr.ToString());
            }
            // assert
            Assert.AreEqual(cp.Doc.GetText("a"), "1");
            Assert.AreEqual(cp.Doc.GetText("b"), "3");
            Assert.AreEqual(cpApp.Doc.GetText("a"), "4");
            Assert.AreEqual(cpApp.Doc.GetText("b"), "5");
            for (var ptr = 1; ptr <= 10; ptr++) {
                Assert.AreEqual(cpApp.Doc.GetText("key" + ptr.ToString()), "value" + ptr.ToString());
            }
            // dispose
            cp.Dispose();
            cpApp.Dispose();
        }
        //
        //====================================================================================================
        // unit test - cp.appOk
        //
        [TestMethod()]
        public void Views_cpBlock_AppOk_unit() {
            // arrange
            CPClass cp = new CPClass();
            CPClass cpApp = new CPClass("testapp");
            // act
            // assert
            Assert.AreEqual(cp.appOk, false);
            Assert.AreEqual(cpApp.appOk, true);
            // dispose
            cp.Dispose();
            cpApp.Dispose();
        }

        //
        //====================================================================================================
        // unit test - sample
        //
        [TestMethod()]
        public void Views_cpBlock_sample_unit() {
            // arrange
            CPClass cp = new CPClass();
            // act
            //
            // assert
            Assert.AreEqual(cp.appOk, false);
            // dispose
            cp.Dispose();
        }
    }
    //
    //====================================================================================================
    //
    [TestClass()]
    public class dbControllerTests {
        [TestMethod()]
        public void Controllers_db_encodeSqlTableNameTest() {
            // arrange
            // act
            // assert
            Assert.AreEqual("", dbController.encodeSqlTableName(""));
            Assert.AreEqual("", dbController.encodeSqlTableName("-----"));
            Assert.AreEqual("", dbController.encodeSqlTableName("01234567879"));
            Assert.AreEqual("a", dbController.encodeSqlTableName("a"));
            Assert.AreEqual("aa", dbController.encodeSqlTableName("a a"));
            Assert.AreEqual("aA", dbController.encodeSqlTableName(" aA"));
            Assert.AreEqual("aA", dbController.encodeSqlTableName(" aA "));
            Assert.AreEqual("aA", dbController.encodeSqlTableName("aA "));
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", dbController.encodeSqlTableName("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#", dbController.encodeSqlTableName("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#"));
            //
        }
        [TestMethod()]
        public void Controllers_db_csSetGetTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass("testapp")) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.core.db.csInsertRecord("people");
                int testId_test1 = cp.core.db.csGetInteger(cs1, "id");
                cp.core.db.csClose(ref cs1);
                //
                int cs2 = cp.core.db.csInsertRecord("people");
                int testId_test2 = cp.core.db.csGetInteger(cs2, "id");
                cp.core.db.csClose(ref cs2);
                // act
                int cs3 = cp.core.db.csOpen("people","(id=" + testId_test1 + ")or(id=" + testId_test2 + ")");
                if (!cp.core.db.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    cp.core.db.csSet(cs3, "username", "a");
                    cp.core.db.csSet(cs3, "password", "b");
                    cp.core.db.csGoNext(cs3);
                }
                if (!cp.core.db.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    cp.core.db.csSet(cs3, "username", "c");
                    cp.core.db.csGoNext(cs3);
                    cp.core.db.csClose(ref cs3);
                }
                // assert
                var person_test1 = Contensive.Core.Models.DbModels.personModel.create(cp.core, testId_test1);
                var person_test2 = Contensive.Core.Models.DbModels.personModel.create(cp.core, testId_test2);
                //
                Assert.AreEqual("a", person_test1.Username);
                Assert.AreEqual("b", person_test1.Password);
                Assert.AreEqual("c", person_test2.Username);
                Assert.AreEqual("", person_test2.Password); // original error this=b
            }
        }

    }
}