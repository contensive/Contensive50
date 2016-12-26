
using Contensive.Core;
using Contensive.BaseClasses;
using Xunit;
using System;
using System.Collections.Generic;

namespace integrationTests
{

    public class testCp
    {
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [Fact]
        public void TestMethod1()
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
        private void TestMethod2()
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
    }
    public class testCpCache
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
    public class testCpContent
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
            int peopleCntBefore = 0;
            int peopleCntAfter = 0;
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers"))
            {
                peopleCntBefore = cs.GetInteger("cnt");
            }
            cs.Close();
            // act
            cp.Content.AddRecord("people");
            if (cs.OpenSQL("select count(*) as cnt  from ccmembers"))
            {
                peopleCntAfter = cs.GetInteger("cnt");
            }
            cs.Close();
            // assert
            Assert.Equal(peopleCntAfter, (peopleCntBefore+1));
            // dispose
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
    }
}
