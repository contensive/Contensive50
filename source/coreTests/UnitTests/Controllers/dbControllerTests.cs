
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
        [TestMethod()]
        public void Controllers_db_csSetGetTest() {
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
        //
        [TestClass()]
        public class dBControllersTest {
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
        }
    }
}