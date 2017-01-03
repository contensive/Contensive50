
using Contensive.Core;
using Contensive.BaseClasses;
using Xunit;
using System;
using System.Collections.Generic;

namespace integrationTests
{

    public class cpTests
    {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [Fact]
        public void constructorWithoutApp()
        {
            // arrange
            CPClass cp = new CPClass();
            // act
            bool clusterOK = cp.clusterOk;
            bool appOK = cp.appOk;
            // assert
            Assert.Equal(clusterOK, true);
            Assert.Equal(appOK, false);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// Test 2 - cp ok with application
        /// </summary>
        [Fact]
        private void constructorWithApp()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            // act
            bool clusterOK = cp.clusterOk;
            bool appOK = cp.appOk;
            // assert
            Assert.Equal(clusterOK, true);
            Assert.Equal(appOK, true);

            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [Fact]
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
            if (cs.Insert("add-ons"))
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
            Assert.Equal(htmlText + wysiwygText + echoText, cp.executeAddon(addonName));
            //
            Assert.Equal(htmlText + wysiwygText + echoText, cp.executeAddon(addonGuid));
            //
            Assert.Equal(htmlText + wysiwygText + echoText, cp.executeAddon(recordId.ToString()));
            //dispose
            cp.Content.DeleteRecords("add-ons", "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cpExecuteAddontest
        /// </summary>
        [Fact]
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
            if (cs.Insert("add-ons"))
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
            Assert.Equal(htmlText + wysiwygText + echoText, cp.executeRoute(addonName));
            //dispose
            cp.Content.DeleteRecords("add-ons", "id=" + recordId.ToString());
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
        [Fact]
        private void cpCacheLegacySaveRead()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            // act
            cp.Cache.Save("testString", "testValue");
            // assert
            Assert.Equal(cp.Cache.Read("testString"), "testValue");
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache save read
        /// </summary>
        [Fact]
        private void cpCacheSetGet_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            DateTime testDate = new DateTime(1990, 8, 7);
            // act
            cp.Cache.setKey("testString", "testValue");
            cp.Cache.setKey("testInt", 12345);
            cp.Cache.setKey("testDate", testDate);
            cp.Cache.setKey("testTrue", true);
            cp.Cache.setKey("testFalse", false);
            // assert
            Assert.Equal(cp.Cache.getText("testString"), "testValue");
            Assert.Equal(cp.Cache.getInteger("testInt"), 12345);
            Assert.Equal(cp.Cache.getDate("testDate"), testDate);
            Assert.Equal(cp.Cache.getBoolean("testTrue"), true);
            Assert.Equal(cp.Cache.getBoolean("testFalse"), false);
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidateAll
        /// </summary>
        [Fact]
        private void cpCacheInvalidateAll_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            DateTime testDate = new DateTime(1990, 8, 7);
            // act
            cp.Cache.setKey("testString", "testValue", "a");
            cp.Cache.setKey("testInt", 12345, "a");
            cp.Cache.setKey("testDate", testDate, "a");
            cp.Cache.setKey("testTrue", true, "a");
            cp.Cache.setKey("testFalse", false, "a");
            // assert
            Assert.Equal("testValue", cp.Cache.getText("testString"));
            Assert.Equal(12345, cp.Cache.getInteger("testInt"));
            Assert.Equal(testDate, cp.Cache.getDate("testDate"));
            Assert.Equal(true, cp.Cache.getBoolean("testTrue"));
            Assert.Equal(false, cp.Cache.getBoolean("testFalse"));
            // act
            cp.Cache.InvalidateTag("a");
            // assert
            Assert.Equal(null, cp.Cache.getObject("testString"));
            Assert.Equal("", cp.Cache.getText("testString"));
            Assert.Equal(0, cp.Cache.getInteger("testInt"));
            Assert.Equal(DateTime.MinValue, cp.Cache.getDate("testDate"));
            Assert.Equal(false, cp.Cache.getBoolean("testTrue"));
            Assert.Equal(false, cp.Cache.getBoolean("testFalse"));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidateAll
        /// </summary>
        [Fact]
        private void cpCacheInvalidateList_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
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
            Assert.Equal("testValue", cp.Cache.getText("testString"));
            Assert.Equal(12345, cp.Cache.getInteger("testInt"));
            Assert.Equal(testDate, cp.Cache.getDate("testDate"));
            Assert.Equal(true, cp.Cache.getBoolean("testTrue"));
            Assert.Equal(false, cp.Cache.getBoolean("testFalse"));
            // act
            cp.Cache.InvalidateTagList(tagList);
            // assert
            Assert.Equal(null, cp.Cache.getObject("testString"));
            Assert.Equal("", cp.Cache.getText("testString"));
            Assert.Equal(0, cp.Cache.getInteger("testInt"));
            Assert.Equal(DateTime.MinValue, cp.Cache.getDate("testDate"));
            Assert.Equal(false, cp.Cache.getBoolean("testTrue"));
            Assert.Equal(false, cp.Cache.getBoolean("testFalse"));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cache invalidate on content save
        /// </summary>
        [Fact]
        private void cpCacheInvalidationOnEdit_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            string contentName = "testContent" + cp.Utils.GetRandomInteger().ToString();
            try
            {
                // arrange
                cp.Content.AddContent(contentName);
                // act
                cp.Cache.Save("keyA", "testValue", contentName);
                // assert
                Assert.Equal("testValue", cp.Cache.Read("keyA"));
                // act
                CPCSBaseClass cs = cp.CSNew();
                if (cs.Insert(contentName))
                {
                    cs.SetField("name", "test");
                } else
                {
                    Assert.False(true);
                }
                cs.Close();
                // assert
                Assert.Equal("", cp.Cache.Read("keyA"));
            }
            catch (Exception ex)
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, exception, [" + ex.Message + "]");
                Assert.True(false, "exception [" + ex.Message + "]");
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
        [Fact]
        private void cpCacheTagInvalidationString()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            // act
            cp.Cache.Save("keyA", "testValue", "a,b,c,d,e");
            // assert
            Assert.Equal(cp.Cache.Read("keyA"), "testValue");
            // act
            cp.Cache.InvalidateTag("c");
            // assert
            Assert.Equal(cp.Cache.getText("keyA"), "");
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
        [Fact]
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
            Assert.NotEqual(0, recordAId);
            Assert.NotEqual(0, recordBId);
            Assert.NotEqual(recordAId, recordBId);
            // dispose
            cp.Content.DeleteRecords("people", "id=" + recordAId);
            cp.Content.DeleteRecords("people", "id=" + recordBId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.delete
        /// </summary>
        [Fact]
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
            cp.Content.DeleteRecords("people", "id=" + peopleId.ToString());
            //
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers"))
            {
                peopleCntAfter = cs.GetInteger("cnt");
            }
            cs.Close();
            // assert
            Assert.Equal(peopleCntAfter, (peopleCntBefore - 1));
            // dispose
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.getCopy
        /// </summary>
        [Fact]
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
            Assert.Equal(testCopy, cp.Content.GetCopy(copyName));
            // dispose
            cp.Content.DeleteRecords("copy content", "id=" + recordId.ToString());
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.getCopy_withDefaultValue
        /// </summary>
        [Fact]
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
            Assert.Equal(testCopy, cp.Content.GetCopy(copyName, testCopy));
            // dispose
            cp.Content.DeleteRecords("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.setCopy
        /// </summary>
        [Fact]
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
            Assert.Equal(testCopyA, cp.Content.GetCopy(copyName, "shouldNotNeedDefault"));
            // act
            cp.Content.SetCopy(copyName, testCopyB);
            // assert
            Assert.Equal(testCopyB, cp.Content.GetCopy(copyName, "shouldNotNeedDefault"));
            // dispose
            cp.Content.DeleteRecords("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.getId
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, peopleContentId);
            Assert.Equal(peopleContentId, cp.Content.GetID(peopleContentName));
            // act
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.content.addContent_name_test
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, cp.Content.AddContent(contentName1));
            Assert.NotEqual(0, cp.Content.AddContent(contentName2, tableName2));
            Assert.NotEqual(0, cp.Content.AddContent(contentName3, tableName3, "default"));
            //
            if (cs.Insert(contentName1))
            {
                Assert.NotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName2))
            {
                Assert.NotEqual(0, cs.GetInteger("id"));
            }
            cs.Close();
            //
            if (cs.Insert(contentName3))
            {
                Assert.NotEqual(0, cs.GetInteger("id"));
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
        [Fact]
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
            Assert.NotEqual(0, cp.Content.AddContent(contentName1));
            Assert.NotEqual(0, cp.Content.AddContentField(contentName1, contentFieldTextName, coreCommonModule.FieldTypeIdText));
            Assert.NotEqual(0, cp.Content.AddContentField(contentName1, contentFieldBooleanName, coreCommonModule.FieldTypeIdBoolean));
            Assert.NotEqual(0, cp.Content.AddContentField(contentName1, contentFieldDateName, coreCommonModule.FieldTypeIdDate));
            Assert.NotEqual(0, cp.Content.AddContentField(contentName1, contentFieldIntegerName, coreCommonModule.FieldTypeIdInteger));
            //
            if (cs.Insert(contentName1))
            {
                recordId = cs.GetInteger("id");
                Assert.NotEqual(0, recordId);
                cs.SetField(contentFieldBooleanName, true);
                cs.SetField(contentFieldTextName, testText);
                cs.SetField(contentFieldDateName, testDate);
                cs.SetField(contentFieldIntegerName, testInt);
            }
            cs.Close();
            //
            if (cs.OpenRecord(contentName1,recordId ))
            {
                Assert.NotEqual(0, cs.GetInteger("id"));
                Assert.Equal(true, cs.GetBoolean(contentFieldBooleanName));
                Assert.Equal(testText, cs.GetText(contentFieldTextName));
                Assert.Equal(testDate, cs.GetDate(contentFieldDateName));
                Assert.Equal(testInt, cs.GetInteger(contentFieldIntegerName));
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
        [Fact]
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
            Assert.NotEqual(0, testuserId);
            // open with id criteria must return exactly one record
            testuserId = 0;
            if (cs.Open("people", "id=" + cp.Db.EncodeSQLNumber(newuserId))) {
                Assert.Equal(newuserId, cs.GetInteger("id"));
                Assert.Equal(true, cs.OK());
                cs.GoNext();
                Assert.Equal(false, cs.OK());
            }
            cs.Close();
            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.insert
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, newuserId);
            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.insert
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, newuserId);
            if (cs.OpenRecord("people", newuserId))
            {
                Assert.Equal(newuserId, cs.GetInteger("id"));
            }
            cs.Close();
            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + newuserId);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.OpenGroupUsers
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, user1Id);
            Assert.NotEqual(0, user2Id);
            // order by id will be 2 then 1
            Assert.True(cs.OpenGroupUsers(groupName, "", "id"));
            Assert.Equal(user2Name, cs.GetText("name"));
            Assert.True(cs.NextOK());
            Assert.Equal(user1Name, cs.GetText("name"));
            Assert.False(cs.NextOK());
            cs.Close();
            // order by name will be 1 then 2
            Assert.True(cs.OpenGroupUsers(groupName, "", "name"));
            Assert.Equal(user1Name, cs.GetText("name"));
            Assert.True(cs.NextOK());
            Assert.Equal(user2Name, cs.GetText("name"));
            Assert.False(cs.NextOK());
            cs.Close();
            //
            // dispose
            //
            cp.Content.DeleteRecords("groups", "name=" + cp.Db.EncodeSQLText(groupName));
            cp.Content.DeleteRecords("people", "id=" + user1Id);
            cp.Content.DeleteRecords("people", "id=" + user2Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.FieldOK
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, user1Id);
            //
            Assert.True(cs.OpenRecord("people", user1Id));
            Assert.True(cs.FieldOK("id"));
            Assert.True(cs.FieldOK("ID"));
            Assert.True(cs.FieldOK("name"));
            Assert.True(cs.FieldOK("firstname"));
            cs.Close();
            //
            Assert.True(cs.OpenRecord("people", user1Id, "id,name"));
            Assert.True(cs.FieldOK("id"));
            Assert.True(cs.FieldOK("ID"));
            Assert.True(cs.FieldOK("name"));
            Assert.True(cs.FieldOK("NaMe"));
            Assert.False(cs.FieldOK("firstname"));
            cs.Close();
            //
            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, user1Id);
            //
            Assert.True(cs.OpenRecord("people", user1Id));
            cs.Delete();
            cs.Close();
            Assert.False(cs.OpenRecord("people", user1Id));
            cs.Close();
            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, user1Id);
            //
            Assert.True(cs.OpenRecord("people", user1Id));
            cs.SetField("name", user1Name);
            cs.SetField("excludefromanalytics", true);
            cs.SetField("lastVisit", testDate);
            cs.SetField("birthdayyear", testinteger);
            // eventually fill out all the rest
            cs.Close();
            //
            Assert.True(cs.OpenRecord("people", user1Id));
            Assert.Equal(user1Name, cs.GetText("name"));
            Assert.Equal(true, cs.GetBoolean("excludefromanalytics"));
            Assert.Equal(testDate, cs.GetDate("lastVisit"));
            Assert.Equal(testinteger, cs.GetInteger("birthdayyear"));
            cs.Close();
            // eventually fill out all the rest

            //
            // dispose
            //
            cp.Content.DeleteRecords("people", "id=" + user1Id);
            cp.Dispose();
        }
        //====================================================================================================
        /// <summary>
        /// cp.cs.Delete
        /// </summary>
        [Fact]
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
            Assert.NotEqual(0, user1Id);
            user2Id = cp.Content.AddRecord("people", user2Name);
            Assert.NotEqual(0, user2Id);
            user3Id = cp.Content.AddRecord("people", user3Name);
            Assert.NotEqual(0, user3Id);
            user4Id = cp.Content.AddRecord("people", user4Name);
            Assert.NotEqual(0, user4Id);
            //
            Assert.True(cs.Open("people", "(id in (" + user1Id + "," + user2Id + "," + user3Id + "," + user4Id + "))"));
            Assert.Equal(4, cs.GetRowCount());
            cs.Delete();
            Assert.True(cs.NextOK());
            cs.Delete();
            Assert.True(cs.NextOK());
            cs.Delete();
            Assert.True(cs.NextOK());
            cs.Delete();
            Assert.False(cs.NextOK());
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
        [Fact]
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
        [Fact]
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
            Assert.Equal(testNumber, resultNumber);
            Assert.Equal(testDate, resultDate);
            //
            // dispose
            //
            cp.Dispose();
        }
    }
}
