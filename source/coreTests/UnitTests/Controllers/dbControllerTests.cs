
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Core.Tests.testConstants;
using Contensive.BaseClasses;

namespace Contensive.Core.Tests.UnitTests.Controllers {
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
        //
        //====================================================================================================
        /// <summary>
        /// set a value, close, open, and get it back
        /// </summary>
        [TestMethod()]
        public void Controllers_db_csSetCloseOpenGetTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
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
                int cs3 = cp.core.db.csOpen("people", "(id=" + testId_test1 + ")or(id=" + testId_test2 + ")");
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
        //
        //====================================================================================================
        /// <summary>
        /// set a value and get it back
        /// </summary>
        [TestMethod()]
        public void Controllers_db_csSetGetTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.core.db.csInsertRecord("people");
                if (!cp.core.db.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", cp.core.db.csGet(cs1, "username"));
                    cp.core.db.csSet(cs1, "username", "a");
                    Assert.AreEqual("a", cp.core.db.csGet(cs1, "username"));
                }
                cp.core.db.csClose(ref cs1);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a value and get it back
        /// </summary>
        [TestMethod()]
        public void Controllers_db_csSetSaveGetTest() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                int cs1 = cp.core.db.csInsertRecord("people");
                if (!cp.core.db.csOk(cs1)) {
                    Assert.Fail("new people records not valid");
                } else {
                    Assert.AreNotEqual("a", cp.core.db.csGet(cs1, "username"));
                    cp.core.db.csSet(cs1, "username", "a");
                    cp.core.db.csSave(cs1);
                    string result1 = cp.core.db.csGet(cs1, "username");
                    Assert.AreEqual("a", result1);
                }
                cp.core.db.csClose(ref cs1);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_db_csGetRowCount() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                CPCSBaseClass cs = cp.CSNew();
                string impossibleName = cp.Utils.CreateGuid();
                // act
                cs.Open(Contensive.Core.Models.DbModels.personModel.contentName, "(name=" + cp.Db.EncodeSQLText(impossibleName) + ")");
                int resultNoData = cs.GetRowCount();
                cs.Close();
                // assert
                Assert.AreEqual(0, resultNoData);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_db_isSqlTableField() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
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
        [TestMethod()]
        public void Controllers_db_csGoNext() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string name = genericController.createGuid();
                //
                // -- add three records
                int cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id0 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id1 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id2 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                // act
                cs = cp.core.db.csOpen("people", "name=" + cp.core.db.encodeSQLText(name),"id");
                Assert.IsTrue(cp.core.db.csOk(cs), "csOpen");
                Assert.AreEqual(id0, cp.core.db.csGetInteger(cs, "id"),"correct id0 after open");
                cp.core.db.csGoNext(cs);
                Assert.AreEqual(id1, cp.core.db.csGetInteger(cs, "id"), "goNext id1, id correct");
                cp.core.db.csGoNext(cs);
                Assert.AreEqual(id2, cp.core.db.csGetInteger(cs, "id"), "goNext id2, id correct");
                cp.core.db.csGoNext(cs);
                Assert.IsFalse(cp.core.db.csOk(cs),"csOk false after all records");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_db_csGoFirst() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                // arrange
                string name = genericController.createGuid();
                //
                // -- add three records
                int cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id0 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id1 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                cs = cp.core.db.csInsertRecord("people");
                cp.core.db.csSet(cs, "name", name);
                cp.core.db.csSave(cs);
                int id2 = cp.core.db.csGetInteger(cs, "id");
                cp.core.db.csClose(ref cs);
                //
                // act
                cs = cp.core.db.csOpen("people", "name=" + cp.core.db.encodeSQLText(name), "id");
                Assert.IsTrue(cp.core.db.csOk(cs), "csOpen");
                cp.core.db.csGoNext(cs);
                cp.core.db.csGoNext(cs);
                cp.core.db.csGoNext(cs);
                Assert.IsFalse(cp.core.db.csOk(cs), "csOK false after last record");
                cp.core.db.csGoFirst(cs);
                Assert.IsTrue(cp.core.db.csOk(cs), "csOK true after goFirst (back at first record");
                //
                Assert.AreEqual(id0, cp.core.db.csGetInteger(cs, "id"), "correct id0 after goFirst");
                cp.core.db.csGoNext(cs);
                Assert.AreEqual(id1, cp.core.db.csGetInteger(cs, "id"), "goNext id1, id correct");
                cp.core.db.csGoNext(cs);
                Assert.AreEqual(id2, cp.core.db.csGetInteger(cs, "id"), "goNext id2, id correct");
                cp.core.db.csGoNext(cs);
                Assert.IsFalse(cp.core.db.csOk(cs), "csOk false after all records");
            }
        }
    }
}