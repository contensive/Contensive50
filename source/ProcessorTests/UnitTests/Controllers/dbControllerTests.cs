
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
                int testId_test1 = 0;
                int testId_test2 = 0;
                cp.core.db.executeNonQuery("delete from ccMembers where (username='a')or(username='c')");
                //
                using (var csXfer = new CsModel(cp.core)) {
                    csXfer.insert("people");
                    testId_test1 = csXfer.getInteger("id");
                    //
                    csXfer.insert("people");
                    testId_test2 = csXfer.getInteger("id");
                    //
                    csXfer.open("people", "(id=" + testId_test1 + ")or(id=" + testId_test2 + ")");
                    if (!csXfer.ok()) {
                        Assert.Fail("new people records not valid");
                    } else {
                        csXfer.set("username", "a");
                        csXfer.set("password", "b");
                        csXfer.goNext();
                    }
                    if (!csXfer.ok()) {
                        Assert.Fail("new people records not valid");
                    } else {
                        csXfer.set("username", "c");
                        csXfer.goNext();
                    }
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
                using (var csXfer = new CsModel(cp.core)) {
                    csXfer.insert("people");
                    if (!csXfer.ok()) {
                        Assert.Fail("new people records not valid");
                    } else {
                        Assert.AreNotEqual("a", csXfer.getText("username"));
                        csXfer.set("username", "a");
                        Assert.AreEqual("a", csXfer.getText("username"));
                    }
                }
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
                using (var csXfer = new CsModel(cp.core)) {
                    csXfer.insert("people");
                    if (!csXfer.ok()) {
                        Assert.Fail("new people records not valid");
                    } else {
                        Assert.AreNotEqual("a", csXfer.getText("username"));
                        csXfer.set("username", "a");
                        csXfer.save();
                        string result1 = csXfer.getText("username");
                        Assert.AreEqual("a", result1);
                    }
                }
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
                using (var csXfer = new CsModel(cp.core)) {
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id0 = csXfer.getInteger("id");
                    //
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id1 = csXfer.getInteger("id");
                    //
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id2 = csXfer.getInteger("id");
                    //
                    // act
                    csXfer.open("people", "name=" + DbController.encodeSQLText(name), "id");
                    Assert.IsTrue(csXfer.ok(), "csOpen");
                    Assert.AreEqual(id0, csXfer.getInteger("id"), "correct id0 after open");
                    csXfer.goNext();
                    Assert.AreEqual(id1, csXfer.getInteger("id"), "goNext id1, id correct");
                    csXfer.goNext();
                    Assert.AreEqual(id2, csXfer.getInteger("id"), "goNext id2, id correct");
                    csXfer.goNext();
                    Assert.IsFalse(csXfer.ok(), "csOk false after all records");
                }
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
                using (var csXfer = new CsModel(cp.core)) {
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id0 = csXfer.getInteger("id");
                    //
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id1 = csXfer.getInteger("id");
                    //
                    csXfer.insert("people");
                    csXfer.set("name", name);
                    csXfer.save();
                    int id2 = csXfer.getInteger("id");
                    //
                    // act
                    csXfer.open("people", "name=" + DbController.encodeSQLText(name), "id");
                    Assert.IsTrue(csXfer.ok(), "csOpen");
                    csXfer.goNext();
                    csXfer.goNext();
                    csXfer.goNext();
                    Assert.IsFalse(csXfer.ok(), "csOK false after last record");
                    csXfer.goFirst();
                    Assert.IsTrue(csXfer.ok(), "csOK true after goFirst (back at first record");
                    //
                    Assert.AreEqual(id0, csXfer.getInteger("id"), "correct id0 after goFirst");
                    csXfer.goNext();
                    Assert.AreEqual(id1, csXfer.getInteger("id"), "goNext id1, id correct");
                    csXfer.goNext();
                    Assert.AreEqual(id2, csXfer.getInteger("id"), "goNext id2, id correct");
                    csXfer.goNext();
                    Assert.IsFalse(csXfer.ok(), "csOk false after all records");
                }
            }
        }
    }
}