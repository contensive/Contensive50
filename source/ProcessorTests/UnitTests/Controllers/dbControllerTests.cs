
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
                int cs1 = csXfer.csInsert("people");
                int testId_test1 = csXfer.csGetInteger(cs1, "id");
                csXfer.csClose(ref cs1);
                //
                int cs2 = csXfer.csInsert("people");
                int testId_test2 = csXfer.csGetInteger(cs2, "id");
                csXfer.csClose(ref cs2);
                // act
                int cs3 = csXfer.csOpen("people", "(id=" + testId_test1 + ")or(id=" + testId_test2 + ")");
                if (!csXfer.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    csXfer.csSet(cs3, "username", "a");
                    csXfer.csSet(cs3, "password", "b");
                    csXfer.csGoNext(cs3);
                }
                if (!csXfer.csOk(cs3)) {
                    Assert.Fail("new people records not valid");
                } else {
                    csXfer.csSet(cs3, "username", "c");
                    csXfer.csGoNext(cs3);
                    csXfer.csClose(ref cs3);
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
                int cs1 = csXfer.csInsert("people");
                if (!csXfer.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", csXfer.csGet(cs1, "username"));
                    csXfer.csSet(cs1, "username", "a");
                    Assert.AreEqual("a", csXfer.csGet(cs1, "username"));
                }
                csXfer.csClose(ref cs1);
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
                int cs1 = csXfer.csInsert("people");
                if (!csXfer.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", csXfer.csGet(cs1, "username"));
                    csXfer.csSet(cs1, "username", "a");
                    csXfer.csSave(cs1);
                    string result1 = csXfer.csGet(cs1, "username");
                    Assert.AreEqual("a", result1);
                }
                csXfer.csClose(ref cs1);
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
                int cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id0 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id1 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id2 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                // act
                cs = csXfer.csOpen("people", "name=" + DbController.encodeSQLText(name),"id");
                Assert.IsTrue(csXfer.csOk(), "csOpen");
                Assert.AreEqual(id0, csXfer.csGetInteger(cs, "id"),"correct id0 after open");
                csXfer.csGoNext(cs);
                Assert.AreEqual(id1, csXfer.csGetInteger(cs, "id"), "goNext id1, id correct");
                csXfer.csGoNext(cs);
                Assert.AreEqual(id2, csXfer.csGetInteger(cs, "id"), "goNext id2, id correct");
                csXfer.csGoNext(cs);
                Assert.IsFalse(csXfer.csOk(),"csOk false after all records");
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
                int cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id0 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id1 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                cs = csXfer.csInsert("people");
                csXfer.csSet(cs, "name", name);
                csXfer.csSave(cs);
                int id2 = csXfer.csGetInteger(cs, "id");
                csXfer.csClose();
                //
                // act
                cs = csXfer.csOpen("people", "name=" + DbController.encodeSQLText(name), "id");
                Assert.IsTrue(csXfer.csOk(), "csOpen");
                csXfer.csGoNext(cs);
                csXfer.csGoNext(cs);
                csXfer.csGoNext(cs);
                Assert.IsFalse(csXfer.csOk(), "csOK false after last record");
                csXfer.csGoFirst(cs);
                Assert.IsTrue(csXfer.csOk(), "csOK true after goFirst (back at first record");
                //
                Assert.AreEqual(id0, csXfer.csGetInteger(cs, "id"), "correct id0 after goFirst");
                csXfer.csGoNext(cs);
                Assert.AreEqual(id1, csXfer.csGetInteger(cs, "id"), "goNext id1, id correct");
                csXfer.csGoNext(cs);
                Assert.AreEqual(id2, csXfer.csGetInteger(cs, "id"), "goNext id2, id correct");
                csXfer.csGoNext(cs);
                Assert.IsFalse(csXfer.csOk(), "csOk false after all records");
            }
        }
    }
}