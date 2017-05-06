
using Contensive.Core;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace integrationTests
{

    public class cpTests
    {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [TestMethod()]
        public void constructorWithoutApp()
        {
            // arrange
            CPClass cp = new CPClass();
            // act
            bool clusterOK = cp.serverOk;
            bool appOK = cp.appOk;
            // assert
            Assert.AreEqual(clusterOK, true);
            Assert.AreEqual(appOK, false);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// Test 2 - cp ok with application
        /// </summary>
        [TestMethod()]
        private void constructorWithApp()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            // act
            bool clusterOK = cp.serverOk;
            bool appOK = cp.appOk;
            // assert
            Assert.AreEqual(clusterOK, true);
            Assert.AreEqual(appOK, true);

            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [TestMethod()]
        private void cpExecuteAddontest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string addonName = "testAddon" + cp.Utils.GetRandomInteger().ToString();
            int recordId = 0;
            string htmlText = "12345";
            string wysiwygText = "<b>abcde</b>";
            string addonGuid = cp.Utils.CreateGuid();
            string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
            string echoText = "text added to document";
            //
            if (cs.Insert( Contensive.Core.constants.cnAddons))
            {
                recordId = cs.GetInteger("id");
                cs.SetField("name", addonName);
                cs.SetField("copytext", htmlText);
                cs.SetField("copy", wysiwygText);
                cs.SetField("ccGuid", addonGuid);
                cs.SetField("scriptingcode", activeScript);
            }
            cs.Close();
            cp.Doc.SetProperty("echo", echoText);
            // act
            // assert
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(addonName));
            //
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(addonGuid));
            //
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeAddon(recordId.ToString()));
            //dispose
            cp.Content.Delete(Contensive.Core.constants.cnAddons, "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [TestMethod()]
        private void cpExecuteRouteTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string addonName = "testAddon" + cp.Utils.GetRandomInteger().ToString();
            int recordId = 0;
            string htmlText = "12345";
            string wysiwygText = "<b>abcde</b>";
            string addonGuid = cp.Utils.CreateGuid();
            string activeScript = "function m\nm=cp.doc.getText(\"echo\")\nend function";
            string echoText = "text added to document";
            //
            if (cs.Insert(Contensive.Core.constants.cnAddons))
            {
                recordId = cs.GetInteger("id");
                cs.SetField("name", addonName);
                cs.SetField("copytext", htmlText);
                cs.SetField("copy", wysiwygText);
                cs.SetField("ccGuid", addonGuid);
                cs.SetField("scriptingcode", activeScript);
                cs.SetField("remotemethod", "1");
            }
            cs.Close();
            cp.Doc.SetProperty("echo", echoText);
            // act
            // assert
            Assert.AreEqual(htmlText + wysiwygText + echoText, cp.executeRoute(addonName));
            //dispose
            cp.Content.Delete(Contensive.Core.constants.cnAddons, "id=" + recordId.ToString());
            cp.Dispose();
        }
    }
    //====================================================================================================
    //====================================================================================================
    public class cpCacheTests
    {
        //====================================================================================================
        /// <summary>
        /// cp.cache save read
        /// </summary>
        [TestMethod()]
        private void cpCacheLegacySaveRead()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            cp.core.siteProperties.setProperty("AllowBake", true);
            // act
            cp.Cache.Save("testString", "testValue");
            // assert
            Assert.AreEqual(cp.Cache.Read("testString"), "testValue");
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache save read
        /// </summary>
        [TestMethod()]
        private void cpCacheSetGet_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            DateTime testDate = new DateTime(1990, 8, 7);
            cp.core.siteProperties.setProperty("AllowBake", true);
            // act
            cp.Cache.setKey("testString", "testValue");
            cp.Cache.setKey("testInt", 12345);
            cp.Cache.setKey("testDate", testDate);
            cp.Cache.setKey("testTrue", true);
            cp.Cache.setKey("testFalse", false);
            // assert
            Assert.AreEqual(cp.Cache.getText("testString"), "testValue");
            Assert.AreEqual(cp.Cache.getInteger("testInt"), 12345);
            Assert.AreEqual(cp.Cache.getDate("testDate"), testDate);
            Assert.AreEqual(cp.Cache.getBoolean("testTrue"), true);
            Assert.AreEqual(cp.Cache.getBoolean("testFalse"), false);
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidateAll
        /// </summary>
        [TestMethod()]
        private void cpCacheInvalidateAll_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            DateTime testDate = new DateTime(1990, 8, 7);
            cp.core.siteProperties.setProperty("AllowBake", true);
            // act
            cp.Cache.setKey("testString", "testValue", "a");
            cp.Cache.setKey("testInt", 12345, "a");
            cp.Cache.setKey("testDate", testDate, "a");
            cp.Cache.setKey("testTrue", true, "a");
            cp.Cache.setKey("testFalse", false, "a");
            // assert
            Assert.AreEqual("testValue", cp.Cache.getText("testString"));
            Assert.AreEqual(12345, cp.Cache.getInteger("testInt"));
            Assert.AreEqual(testDate, cp.Cache.getDate("testDate"));
            Assert.AreEqual(true, cp.Cache.getBoolean("testTrue"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testFalse"));
            // act
            cp.Cache.InvalidateTag("a");
            // assert
            Assert.AreEqual(null, cp.Cache.getObject("testString"));
            Assert.AreEqual("", cp.Cache.getText("testString"));
            Assert.AreEqual(0, cp.Cache.getInteger("testInt"));
            Assert.AreEqual(DateTime.MinValue, cp.Cache.getDate("testDate"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testTrue"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testFalse"));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidateAll
        /// </summary>
        [TestMethod()]
        private void cpCacheInvalidateList_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            cp.core.siteProperties.setProperty("AllowBake", true);
            DateTime testDate = new DateTime(1990, 8, 7);
            List<string> tagList = new List<string>();
            tagList.Add("a");
            tagList.Add("b");
            tagList.Add("c");
            tagList.Add("d");
            tagList.Add("e");
            // act
            cp.Cache.setKey("testString", "testValue", "a");
            cp.Cache.setKey("testInt", 12345, "b");
            cp.Cache.setKey("testDate", testDate, "c");
            cp.Cache.setKey("testTrue", true, "d");
            cp.Cache.setKey("testFalse", false, "e");
            // assert
            Assert.AreEqual("testValue", cp.Cache.getText("testString"));
            Assert.AreEqual(12345, cp.Cache.getInteger("testInt"));
            Assert.AreEqual(testDate, cp.Cache.getDate("testDate"));
            Assert.AreEqual(true, cp.Cache.getBoolean("testTrue"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testFalse"));
            // act
            cp.Cache.InvalidateTagList(tagList);
            // assert
            Assert.AreEqual(null, cp.Cache.getObject("testString"));
            Assert.AreEqual("", cp.Cache.getText("testString"));
            Assert.AreEqual(0, cp.Cache.getInteger("testInt"));
            Assert.AreEqual(DateTime.MinValue, cp.Cache.getDate("testDate"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testTrue"));
            Assert.AreEqual(false, cp.Cache.getBoolean("testFalse"));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidate on content save
        /// </summary>
        [TestMethod()]
        private void cpCacheInvalidationOnEdit_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            cp.core.siteProperties.setProperty("AllowBake", true);
            string contentName = "testContent" + cp.Utils.GetRandomInteger().ToString();
            try
            {
                // arrange
                cp.Content.AddContent(contentName);
                // act
                cp.Cache.Save("keyA", "testValue", contentName);
                // assert
                Assert.AreEqual("testValue", cp.Cache.Read("keyA"));
                // act
                CPCSBaseClass cs = cp.CSNew();
                if (cs.Insert(contentName))
                {
                    cs.SetField("name", "test");
                } else
                {
                    Assert.Fail();
                }
                cs.Close();
                // assert
                Assert.AreEqual("", cp.Cache.Read("keyA"));
            }
            catch (Exception ex)
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, exception, [" + ex.Message + "]");
                Assert.Fail( "exception [" + ex.Message + "]");
            }
            finally
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, exit");
                cp.Content.DeleteContent(contentName);
                cp.Dispose();
            }
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidate on content save
        /// </summary>
        [TestMethod()]
        private void cpCacheTagInvalidationString()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            cp.core.siteProperties.setProperty("AllowBake", true);
            // act
            cp.Cache.Save("keyA", "testValue", "a,b,c,d,e");
            // assert
            Assert.AreEqual(cp.Cache.Read("keyA"), "testValue");
            // act
            cp.Cache.InvalidateTag("c");
            // assert
            Assert.AreEqual(cp.Cache.getText("keyA"), "");
            // dispose
            cp.Dispose();
        }
    }
    //====================================================================================================
    //====================================================================================================
    public class cpContentTests
    {
        //====================================================================================================
        /// <summary>
        /// cp.content.addRecord
        /// </summary>
        [TestMethod()]
        private void cpContentAddRecordTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
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
        [TestMethod()]
        private void cpContentDeleteRecordTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int peopleCntBefore = 0;
            int peopleCntAfter = 0;
            int peopleId = 0;
            peopleId = cp.Content.AddRecord("people");
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers"))
            {
                peopleCntBefore = cs.GetInteger("cnt");
            }
            cs.Close();
            // act
            cp.Content.Delete("people", "id=" + peopleId.ToString());
            //
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers"))
            {
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
        [TestMethod()]
        private void cpContentGetCopyTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int recordId = 0;
            string testCopy = "test copy " + cp.Utils.GetRandomInteger().ToString();
            string copyName = "copy record name " + cp.Utils.GetRandomInteger().ToString();
            recordId = cp.Content.AddRecord("copy content");
            cp.Db.ExecuteSQL("update ccCopyContent set name=" + cp.Db.EncodeSQLText(copyName) + ",copy=" + cp.Db.EncodeSQLText(testCopy) + " where id=" + recordId.ToString());
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
        /// cp.content.getCopy_withDefaultValue
        /// </summary>
        [TestMethod()]
        private void cpContentGetCopyWithDefaultTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string testCopy = "test copy " + cp.Utils.GetRandomInteger().ToString();
            string copyName = "copy record name " + cp.Utils.GetRandomInteger().ToString();
            // act
            //
            // assert
            Assert.AreEqual(testCopy, cp.Content.GetCopy(copyName, testCopy));
            // dispose
            cp.Content.Delete("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.setCopy
        /// </summary>
        [TestMethod()]
        private void cpContentSetCopyTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
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
        [TestMethod()]
        private void cpContentGetId()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int peopleContentId = 0;
            string peopleContentName = "people";
            if (cs.Open("content", "name=" + cp.Db.EncodeSQLText(peopleContentName)))
            {
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
        [TestMethod()]
        private void cpContentAddContentTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
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
            if (cs.Insert(contentName1))
            {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName2))
            {
                Assert.AreNotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName3))
            {
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
        [TestMethod()]
        private void cpContentAddContentFieldTest()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
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
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldTextName, constants.FieldTypeIdText));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldBooleanName, constants.FieldTypeIdBoolean));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldDateName, constants.FieldTypeIdDate));
            Assert.AreNotEqual(0, cp.Content.AddContentField(contentName1, contentFieldIntegerName, constants.FieldTypeIdInteger));
            //
            if (cs.Insert(contentName1))
            {
                recordId = cs.GetInteger("id");
                Assert.AreNotEqual(0, recordId);
                cs.SetField(contentFieldBooleanName, true);
                cs.SetField(contentFieldTextName, testText);
                cs.SetField(contentFieldDateName, testDate);
                cs.SetField(contentFieldIntegerName, testInt);
            }
            cs.Close();
            //
            if (cs.OpenRecord(contentName1,recordId ))
            {
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
    //====================================================================================================
    //====================================================================================================
    public class cpCSTests
    {
        //====================================================================================================
        /// <summary>
        /// cp.cs
        /// </summary>
        [TestMethod()]
        private void cpCsOpenClose_test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int newuserId = cp.Content.AddRecord("people");
            int testuserId;
            //
            // act
            //
            // open without criteria must return at least one record
            testuserId = 0;
            if (cs.Open("people"))
            {
                testuserId = cs.GetInteger("id");
            }
            cs.Close();
            Assert.AreNotEqual(0, testuserId);
            // open with id criteria must return exactly one record
            testuserId = 0;
            if (cs.Open("people", "id=" + cp.Db.EncodeSQLNumber(newuserId))) {
                Assert.AreEqual(newuserId, cs.GetInteger("id"));
                Assert.AreEqual(true, cs.OK());
                cs.GoNext();
                Assert.AreEqual(false, cs.OK());
            }
            cs.Close();
            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.insert
        /// </summary>
        [TestMethod()]
        private void cpCsInsert_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int newuserId = 0;
            //
            // act
            //
            if (cs.Insert("people"))
            {
                newuserId = cs.GetInteger("id");
            }
            cs.Close();
            Assert.AreNotEqual(0, newuserId);
            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.insert
        /// </summary>
        [TestMethod()]
        private void cpCsOpenRecord_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int newuserId = 0;
            //
            // act
            //
            if (cs.Insert("people"))
            {
                newuserId = cs.GetInteger("id");
            }
            cs.Close();
            Assert.AreNotEqual(0, newuserId);
            if (cs.OpenRecord("people", newuserId))
            {
                Assert.AreEqual(newuserId, cs.GetInteger("id"));
            }
            cs.Close();
            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.OpenGroupUsers
        /// </summary>
        [TestMethod()]
        private void cpCsOpenGroupUsers_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string groupName = "testGroup" + cp.Utils.GetRandomInteger().ToString();
            string user1Name = "testUser1" + cp.Utils.GetRandomInteger().ToString();
            string user2Name = "testUser2" + cp.Utils.GetRandomInteger().ToString();
            int user1Id = 0;
            int user2Id = 0;
            //
            // act
            //
            user2Id = cp.Content.AddRecord("people", user2Name);
            cp.Group.AddUser(groupName, user2Id);
            user1Id = cp.Content.AddRecord("people", user1Name);
            cp.Group.AddUser(groupName, user1Id);
            // assert
            Assert.AreNotEqual(0, user1Id);
            Assert.AreNotEqual(0, user2Id);
            // order by id will be 2 then 1
            Assert.IsTrue(cs.OpenGroupUsers(groupName, "", "id"));
            Assert.AreEqual(user2Name, cs.GetText("name"));
            Assert.IsTrue(cs.NextOK());
            Assert.AreEqual(user1Name, cs.GetText("name"));
            Assert.IsFalse(cs.NextOK());
            cs.Close();
            // order by name will be 1 then 2
            Assert.IsTrue(cs.OpenGroupUsers(groupName, "", "name"));
            Assert.AreEqual(user1Name, cs.GetText("name"));
            Assert.IsTrue(cs.NextOK());
            Assert.AreEqual(user2Name, cs.GetText("name"));
            Assert.IsFalse(cs.NextOK());
            cs.Close();
            //
            // dispose
            //
            cp.Content.Delete("groups", "name=" + cp.Db.EncodeSQLText(groupName));
            cp.Content.Delete("people", "id=" + user1Id);
            cp.Content.Delete("people", "id=" + user2Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.FieldOK
        /// </summary>
        [TestMethod()]
        private void cpCsFieldOK_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string user1Name = "testUser1" + cp.Utils.GetRandomInteger().ToString();
            int user1Id = 0;
            //
            // act
            //
            user1Id = cp.Content.AddRecord("people", user1Name);
            Assert.AreNotEqual(0, user1Id);
            //
            Assert.IsTrue(cs.OpenRecord("people", user1Id));
            Assert.IsTrue(cs.FieldOK("id"));
            Assert.IsTrue(cs.FieldOK("ID"));
            Assert.IsTrue(cs.FieldOK("name"));
            Assert.IsTrue(cs.FieldOK("firstname"));
            cs.Close();
            //
            Assert.IsTrue(cs.OpenRecord("people", user1Id, "id,name"));
            Assert.IsTrue(cs.FieldOK("id"));
            Assert.IsTrue(cs.FieldOK("ID"));
            Assert.IsTrue(cs.FieldOK("name"));
            Assert.IsTrue(cs.FieldOK("NaMe"));
            Assert.IsFalse(cs.FieldOK("firstname"));
            cs.Close();
            //
            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [TestMethod()]
        private void cpCsDelete_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string user1Name = "testUser1" + cp.Utils.GetRandomInteger().ToString();
            int user1Id = 0;
            //
            // act
            //
            user1Id = cp.Content.AddRecord("people", user1Name);
            Assert.AreNotEqual(0, user1Id);
            //
            Assert.IsTrue(cs.OpenRecord("people", user1Id));
            cs.Delete();
            cs.Close();
            Assert.IsFalse(cs.OpenRecord("people", user1Id));
            cs.Close();
            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [TestMethod()]
        private void cpCsSetGetField_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string user1Name = "testUser1" + cp.Utils.GetRandomInteger().ToString();
            int user1Id = 0;
            Random rnd = new Random();
            DateTime testDate = new DateTime(1900 + rnd.Next(1, 100), rnd.Next(1, 13), rnd.Next(1, 28));
            int testinteger = rnd.Next(1, 2000);
            //
            // act
            //
            user1Id = cp.Content.AddRecord("people", user1Name);
            Assert.AreNotEqual(0, user1Id);
            //
            Assert.IsTrue(cs.OpenRecord("people", user1Id));
            cs.SetField("name", user1Name);
            cs.SetField("excludefromanalytics", true);
            cs.SetField("lastVisit", testDate);
            cs.SetField("birthdayyear", testinteger);
            // eventually fill out all the rest
            cs.Close();
            //
            Assert.IsTrue(cs.OpenRecord("people", user1Id));
            Assert.AreEqual(user1Name, cs.GetText("name"));
            Assert.AreEqual(true, cs.GetBoolean("excludefromanalytics"));
            Assert.AreEqual(testDate, cs.GetDate("lastVisit"));
            Assert.AreEqual(testinteger, cs.GetInteger("birthdayyear"));
            cs.Close();
            // eventually fill out all the rest

            //
            // dispose
            //
            cp.Content.Delete("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [TestMethod()]
        private void cpCsGetRowCount_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            string user1Name = "testUser1" + cp.Utils.GetRandomInteger().ToString();
            string user2Name = "testUser2" + cp.Utils.GetRandomInteger().ToString();
            string user3Name = "testUser3" + cp.Utils.GetRandomInteger().ToString();
            string user4Name = "testUser4" + cp.Utils.GetRandomInteger().ToString();
            int user1Id = 0;
            int user2Id = 0;
            int user3Id = 0;
            int user4Id = 0;
            //
            // act
            //
            user1Id = cp.Content.AddRecord("people", user1Name);
            Assert.AreNotEqual(0, user1Id);
            user2Id = cp.Content.AddRecord("people", user2Name);
            Assert.AreNotEqual(0, user2Id);
            user3Id = cp.Content.AddRecord("people", user3Name);
            Assert.AreNotEqual(0, user3Id);
            user4Id = cp.Content.AddRecord("people", user4Name);
            Assert.AreNotEqual(0, user4Id);
            //
            Assert.IsTrue(cs.Open("people", "(id in (" + user1Id + "," + user2Id + "," + user3Id + "," + user4Id + "))"));
            Assert.AreEqual(4, cs.GetRowCount());
            cs.Delete();
            Assert.IsTrue(cs.NextOK());
            cs.Delete();
            Assert.IsTrue(cs.NextOK());
            cs.Delete();
            Assert.IsTrue(cs.NextOK());
            cs.Delete();
            Assert.IsFalse(cs.NextOK());
            cs.Close();
            //
            // dispose
            //
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.getSql
        /// </summary>
        [TestMethod()]
        private void cpCsGetSql_Test()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            //
            // act
            //
            // assert
            //
            // dispose
            //
            cp.Dispose();
        }
    }
    public class cpSecurityTests
    {

        //====================================================================================================
        /// <summary>
        /// coreSecurity
        /// </summary>
        [TestMethod()]
        private void coreSecurityEncryptDecrypt()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            CPCSBaseClass cs = cp.CSNew();
            int testNumber = 12345;
            DateTime testDate = new DateTime(1990, 8, 7);
            //
            // act
            //
            string token = cp.core.security.encodeToken(testNumber, testDate);
            //
            // assert
            //
            int resultNumber = 0;
            DateTime resultDate = DateTime.MinValue;
            cp.core.security.decodeToken(token, ref resultNumber, ref resultDate);
            Assert.AreEqual(testNumber, resultNumber);
            Assert.AreEqual(testDate, resultDate);
            //
            // dispose
            //
            cp.Dispose();
        }
    }
}
