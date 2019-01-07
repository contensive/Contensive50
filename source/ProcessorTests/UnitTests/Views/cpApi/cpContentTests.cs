
using Contensive.Processor;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Views {

    [TestClass()]
    public class cpContentTests {
        //====================================================================================================
        /// <summary>
        /// cp.content.addRecord
        /// </summary>
        [TestMethod]
        public void Views_cpContent_AddRecordTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int recordAId = 0;
            int recordBId = 0;
            // act
            recordAId = cp.Content.AddRecord("people");
            recordBId = cp.Content.AddRecord("people");
            // assert
            Assert.AreNotEqual(0, recordAId);
            Assert.AreNotEqual(0, recordBId);
            Assert.AreNotEqual(recordAId, recordBId);
            // dispose
            cp.Content.Delete("people", "id=" + recordAId);
            cp.Content.Delete("people", "id=" + recordBId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.delete
        /// </summary>
        [TestMethod]
        public void Views_cpContent_DeleteRecordTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int peopleCntBefore = 0;
            int peopleCntAfter = 0;
            int peopleId = 0;
            peopleId = cp.Content.AddRecord("people");
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers")) {
                peopleCntBefore = cs.GetInteger("cnt");
            }
            cs.Close();
            // act
            cp.Content.Delete("people", "id=" + peopleId.ToString());
            //
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers")) {
                peopleCntAfter = cs.GetInteger("cnt");
            }
            cs.Close();
            // assert
            Assert.AreEqual(peopleCntAfter, (peopleCntBefore - 1));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.getCopy
        /// </summary>
        [TestMethod]
        public void Views_cpContent_GetCopyTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int recordId = 0;
            string testCopy = "test copy " + cp.Utils.GetRandomInteger().ToString();
            string copyName = "copy record name " + cp.Utils.GetRandomInteger().ToString();
            recordId = cp.Content.AddRecord("copy content");
            cp.core.db.executeNonQuery("update ccCopyContent set name=" + cp.Db.EncodeSQLText(copyName) + ",copy=" + cp.Db.EncodeSQLText(testCopy) + " where id=" + recordId.ToString());
            // act
            //
            // assert
            Assert.AreEqual(testCopy, cp.Content.GetCopy(copyName));
            // dispose
            cp.Content.Delete("copy content", "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.setCopy
        /// </summary>
        [TestMethod]
        public void Views_cpContent_SetCopyTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            string testCopyA = "test copy A " + cp.Utils.GetRandomInteger().ToString();
            string testCopyB = "test copy B " + cp.Utils.GetRandomInteger().ToString();
            string copyName = "copy record name " + cp.Utils.GetRandomInteger().ToString();
            // act
            cp.Content.SetCopy(copyName, testCopyA);
            // assert
            Assert.AreEqual(testCopyA, cp.Content.GetCopy(copyName, "shouldNotNeedDefault"));
            // act
            cp.Content.SetCopy(copyName, testCopyB);
            // assert
            Assert.AreEqual(testCopyB, cp.Content.GetCopy(copyName, "shouldNotNeedDefault"));
            // dispose
            cp.Content.Delete("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.getId
        /// </summary>
        [TestMethod]
        public void Views_cpContent_GetId() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int peopleContentId = 0;
            string peopleContentName = "people";
            if (cs.Open("content", "name=" + cp.Db.EncodeSQLText(peopleContentName))) {
                peopleContentId = cs.GetInteger("id");
            }
            cs.Close();
            //
            // assert
            Assert.AreNotEqual(0, peopleContentId);
            Assert.AreEqual(peopleContentId, cp.Content.GetID(peopleContentName));
            // act
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.addContent_name_test
        /// </summary>
        [TestMethod]
        public void Views_cpContent_AddContentTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            string contentName1 = "testContent" + cp.Utils.GetRandomInteger().ToString();
            string contentName2 = "testContent" + cp.Utils.GetRandomInteger().ToString();
            string contentName3 = "testContent" + cp.Utils.GetRandomInteger().ToString();
            string tableName2 = "testTable" + cp.Utils.GetRandomInteger().ToString();
            string tableName3 = "testTable" + cp.Utils.GetRandomInteger().ToString();
            //
            // act
            // assert
            //
            Assert.AreNotEqual(0, cp.Content.AddContent(contentName1));
            Assert.AreNotEqual(0, cp.Content.AddContent(contentName2, tableName2));
            Assert.AreNotEqual(0, cp.Content.AddContent(contentName3, tableName3, "default"));
            //
            if (cs.Insert(contentName1)) {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName2)) {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName3)) {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            // dispose
            cp.Content.DeleteContent(contentName1);
            cp.Content.DeleteContent(contentName2);
            cp.Content.DeleteContent(contentName2);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.addContent_name_test
        /// </summary>
        [TestMethod]
        public void Views_cpContent_AddContentFieldTest() {
            // arrange
            CPClass cp = new CPClass(testAppName);
            CPCSBaseClass cs = cp.CSNew();
            int recordId = 0;
            string contentName1 = "testContent" + cp.Utils.GetRandomInteger().ToString();
            string contentFieldBooleanName = "testFieldBoolean" + cp.Utils.GetRandomInteger().ToString();
            string contentFieldTextName = "testFieldText" + cp.Utils.GetRandomInteger().ToString();
            string contentFieldDateName = "testFieldDate" + cp.Utils.GetRandomInteger().ToString();
            string contentFieldIntegerName = "testFieldInteger" + cp.Utils.GetRandomInteger().ToString();
            string testText = "testText" + cp.Utils.GetRandomInteger().ToString();
            int testInt = cp.Utils.GetRandomInteger();
            Random rnd = new Random(cp.Utils.GetRandomInteger());
            DateTime testDate = new DateTime(rnd.Next(1900, 2000), rnd.Next(1, 13), rnd.Next(1, 28));
            //
            // act
            // assert
            //
            Assert.AreNotEqual(0, cp.Content.AddContent(contentName1));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldTextName, CPContentBaseClass.fileTypeIdEnum.Text));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldBooleanName, CPContentBaseClass.fileTypeIdEnum.Boolean));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldDateName, CPContentBaseClass.fileTypeIdEnum.Date));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldIntegerName, CPContentBaseClass.fileTypeIdEnum.Integer));
            //
            if (cs.Insert(contentName1)) {
                recordId = cs.GetInteger("id");
                Assert.AreNotEqual(0, recordId);
                cs.SetField(contentFieldBooleanName, true);
                cs.SetField(contentFieldTextName, testText);
                cs.SetField(contentFieldDateName, testDate);
                cs.SetField(contentFieldIntegerName, testInt);
            }
            cs.Close();
            //
            if (cs.OpenRecord(contentName1, recordId)) {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
                Assert.AreEqual(true, cs.GetBoolean(contentFieldBooleanName));
                Assert.AreEqual(testText, cs.GetText(contentFieldTextName));
                Assert.AreEqual(testDate, cs.GetDate(contentFieldDateName));
                Assert.AreEqual(testInt, cs.GetInteger(contentFieldIntegerName));
            }
            cs.Close();
            // dispose
            cp.Content.DeleteContent(contentName1);
            cp.Dispose();
        }
    }
}
