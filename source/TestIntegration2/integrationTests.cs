
using Contensive.Core;
using Contensive.BaseClasses;
using Xunit;
using System;
using System.Collections.Generic;

namespace integrationTests
{

    public class cpTests
    {
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
            Assert.Equal(appOK, false );
            cp.Dispose();
        }
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
            cp.Content.Delete("add-ons", "id=" + recordId.ToString());
            cp.Dispose();
        }
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
            cp.Content.Delete("add-ons", "id=" + recordId.ToString());
            cp.Dispose();
        }
    }
    public class cpCacheTests
    {
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
            cp.Cache.setKey("testString", "testValue",  "a");
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
            cp.Cache.InvalidateTag( "a" );
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
        /// <summary>
        /// cp.cache invalidate on content save
        /// </summary>
        [Fact]
        private void cpCacheInvalidationOnEdit_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            try
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, enter");
                // act
                if (cp.Content.GetID("Content For cpCacheInvalidationOnEdit_integration")==0)
                {
                    

                }
                
                cp.Cache.Save("keyA", "testValue", "people");
                // assert
                Assert.Equal("testValue", cp.Cache.Read("keyA"));
                // act
                CPCSBaseClass cs = cp.CSNew();
                if (cs.Insert("people"))
                {
                    cs.SetField("name", "test");
                }
                cs.Close();
                // assert
                Assert.Equal("",cp.Cache.Read("keyA"));
                // dispose
            }
            catch (Exception ex)
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, exception, [" + ex.Message + "]");
                Assert.True(false, "exception [" + ex.Message + "]");
            }
            finally
            {
                cp.Utils.AppendLog("cpCacheInvalidationOnEdit_integration, exit");
                cp.Dispose();
            }
        }
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
    public class cpContentTests
    {
        /// <summary>
        /// cp.content.addRecord
        /// </summary>
        [Fact]
        private void cpContentAddTest()
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
            cp.Content.Delete("people", "id=" + recordAId);
            cp.Content.Delete("people", "id=" + recordBId);
            cp.Dispose();
        }
        /// <summary>
        /// cp.content.delete
        /// </summary>
        [Fact]
        private void cpContentDeleteTest()
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
            Assert.Equal(peopleCntAfter, (peopleCntBefore - 1));
            // dispose
            cp.Dispose();
        }
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
            string testCopy = "test copy " + cp.Utils.GetRandomInteger().ToString() ;
            string copyName = "copy record name " + cp.Utils.GetRandomInteger().ToString();
            recordId = cp.Content.AddRecord("copy content");
            cp.Db.ExecuteSQL("update ccCopyContent set name=" + cp.Db.EncodeSQLText(copyName) + ",copy=" + cp.Db.EncodeSQLText(testCopy) + " where id=" + recordId.ToString());
            // act
            //
            // assert
            Assert.Equal(testCopy,cp.Content.GetCopy(copyName));
            // dispose
            cp.Content.Delete("copy content", "id=" + recordId.ToString());
            cp.Dispose();
        }
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
            Assert.Equal(testCopy, cp.Content.GetCopy(copyName,testCopy));
            // dispose
            cp.Content.Delete("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
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
            cp.Content.Delete("copy content", "name=" + cp.Db.EncodeSQLText(copyName));
            cp.Dispose();
        }
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
            Assert.Equal(peopleContentId, cp.Content.GetID( peopleContentName));
            // act
            cp.Dispose();
        }
    }
}
