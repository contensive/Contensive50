
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Processor.Tests.testConstants;
using Contensive.BaseClasses;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class dbControllerTests {
        [TestMethod]
        public void Controllers_db_encodeSqlTableNameTest() {
            // arrange
            // act
            // assert
            Assert.AreEqual("", DbController.encodeSqlTableName(""));
            Assert.AreEqual("", DbController.encodeSqlTableName("-----"));
            Assert.AreEqual("", DbController.encodeSqlTableName("01234567879"));
            Assert.AreEqual("a", DbController.encodeSqlTableName("a"));
            Assert.AreEqual("aa", DbController.encodeSqlTableName("a a"));
            Assert.AreEqual("aA", DbController.encodeSqlTableName(" aA"));
            Assert.AreEqual("aA", DbController.encodeSqlTableName(" aA "));
            Assert.AreEqual("aA", DbController.encodeSqlTableName("aA "));
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", DbController.encodeSqlTableName("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"));
            Assert.AreEqual("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#", DbController.encodeSqlTableName("0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#"));
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a value, close, open, and get it back
        /// </summary>
        [TestMethod]
        public void Controllers_db_csSetCloseOpenGetTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.csXfer.csInsert("people");
                int testId_test1 = cp.csXfer.csGetInteger(cs1, "id");
                cp.csXfer.csClose(ref cs1);
                //
                int cs2 = cp.csXfer.csInsert("people");
                int testId_test2 = cp.csXfer.csGetInteger(cs2, "id");
                cp.csXfer.csClose(ref cs2);
                // act
                int cs3 = cp.csXfer.csOpen("people", "(id=" + testId_test1 + ")or(id=" + testId_test2 + ")");
                if (!cp.csXfer.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    cp.csXfer.csSet(cs3, "username", "a");
                    cp.csXfer.csSet(cs3, "password", "b");
                    cp.csXfer.csGoNext(cs3);
                }
                if (!cp.csXfer.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    cp.csXfer.csSet(cs3, "username", "c");
                    cp.csXfer.csGoNext(cs3);
                    cp.csXfer.csClose(ref cs3);
                }
                // assert
                var person_test1 = Contensive.Processor.Models.Db.PersonModel.create(cp.core, testId_test1);
                var person_test2 = Contensive.Processor.Models.Db.PersonModel.create(cp.core, testId_test2);
                //
                Assert.AreEqual("a", person_test1.username);
                Assert.AreEqual("b", person_test1.password);
                Assert.AreEqual("c", person_test2.username);
                Assert.AreEqual("", person_test2.password); // original error this=b
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a value and get it back
        /// </summary>
        [TestMethod]
        public void Controllers_db_csSetGetTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.csXfer.csInsert("people");
                if (!cp.csXfer.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", cp.csXfer.csGet(cs1, "username"));
                    cp.csXfer.csSet(cs1, "username", "a");
                    Assert.AreEqual("a", cp.csXfer.csGet(cs1, "username"));
                }
                cp.csXfer.csClose(ref cs1);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a value and get it back
        /// </summary>
        [TestMethod]
        public void Controllers_db_csSetSaveGetTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.csXfer.csInsert("people");
                if (!cp.csXfer.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", cp.csXfer.csGet(cs1, "username"));
                    cp.csXfer.csSet(cs1, "username", "a");
                    cp.csXfer.csSave(cs1);
                    string result1 = cp.csXfer.csGet(cs1, "username");
                    Assert.AreEqual("a", result1);
                }
                cp.csXfer.csClose(ref cs1);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_db_csGetRowCount() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                CPCSBaseClass cs = cp.CSNew();
                string impossibleName = cp.Utils.CreateGuid();
                // act
                cs.Open(Contensive.Processor.Models.Db.PersonModel.contentName, "(name=" + cp.Db.EncodeSQLText(impossibleName) + ")");
                int resultNoData = cs.GetRowCount();
                cs.Close();
                // assert
                Assert.AreEqual(0, resultNoData);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_db_isSqlTableField() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.IsTrue(cp.core.db.isSQLTableField("", "ccmembers", "name"));
                Assert.IsTrue(cp.core.db.isSQLTableField("", "ccmembers", "NaMe"));
                Assert.IsFalse(cp.core.db.isSQLTableField("", "ccmembers", "namex"));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_db_csGoNext() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string name = GenericController.getGUID();
                //
                // -- add three records
                int cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id0 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id1 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id2 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                // act
                cs = cp.csXfer.csOpen("people", "name=" + DbController.encodeSQLText(name),"id");
                Assert.IsTrue(cp.csXfer.csOk(), "csOpen");
                Assert.AreEqual(id0, cp.csXfer.csGetInteger(cs, "id"),"correct id0 after open");
                cp.csXfer.csGoNext(cs);
                Assert.AreEqual(id1, cp.csXfer.csGetInteger(cs, "id"), "goNext id1, id correct");
                cp.csXfer.csGoNext(cs);
                Assert.AreEqual(id2, cp.csXfer.csGetInteger(cs, "id"), "goNext id2, id correct");
                cp.csXfer.csGoNext(cs);
                Assert.IsFalse(cp.csXfer.csOk(),"csOk false after all records");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_db_csGoFirst() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string name = GenericController.getGUID();
                //
                // -- add three records
                int cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id0 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id1 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                cs = cp.csXfer.csInsert("people");
                cp.csXfer.csSet(cs, "name", name);
                cp.csXfer.csSave(cs);
                int id2 = cp.csXfer.csGetInteger(cs, "id");
                cp.csXfer.csClose();
                //
                // act
                cs = cp.csXfer.csOpen("people", "name=" + DbController.encodeSQLText(name), "id");
                Assert.IsTrue(cp.csXfer.csOk(), "csOpen");
                cp.csXfer.csGoNext(cs);
                cp.csXfer.csGoNext(cs);
                cp.csXfer.csGoNext(cs);
                Assert.IsFalse(cp.csXfer.csOk(), "csOK false after last record");
                cp.csXfer.csGoFirst(cs);
                Assert.IsTrue(cp.csXfer.csOk(), "csOK true after goFirst (back at first record");
                //
                Assert.AreEqual(id0, cp.csXfer.csGetInteger(cs, "id"), "correct id0 after goFirst");
                cp.csXfer.csGoNext(cs);
                Assert.AreEqual(id1, cp.csXfer.csGetInteger(cs, "id"), "goNext id1, id correct");
                cp.csXfer.csGoNext(cs);
                Assert.AreEqual(id2, cp.csXfer.csGetInteger(cs, "id"), "goNext id2, id correct");
                cp.csXfer.csGoNext(cs);
                Assert.IsFalse(cp.csXfer.csOk(), "csOk false after all records");
            }
        }
    }
}