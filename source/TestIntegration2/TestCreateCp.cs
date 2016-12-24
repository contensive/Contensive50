
using Contensive.Core;
using Contensive.BaseClasses;
using Xunit;
using System;

namespace TestIntegration2
{

    public class TestCreateCp
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
        /// <summary>
        /// cp.cache save read
        /// </summary>
        [Fact]
        private void cpCacheLegacySaveRead_ntegration()
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
            DateTime testDate = new DateTime(1990,8,7);
            // act
            cp.Cache.setKey("testString", "testValue");
            cp.Cache.setKey("testInt", 12345);
            cp.Cache.setKey("testDate", testDate);
            cp.Cache.setKey("testTrue", true);
            cp.Cache.setKey("testFalse", false );
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
            // act
            cp.Cache.InvalidateAll();
            // assert
            Assert.Equal(cp.Cache.getObject("testString"), null );
            Assert.Equal(cp.Cache.getText("testString"), "");
            Assert.Equal(cp.Cache.getInteger("testInt"), 0);
            Assert.Equal(cp.Cache.getDate("testDate"), DateTime.MinValue );
            Assert.Equal(cp.Cache.getBoolean("testTrue"), false );
            Assert.Equal(cp.Cache.getBoolean("testFalse"), false);
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
            // act
            cp.Cache.setKey("testString", "testValue","a");
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
            // act
            cp.Cache.InvalidateAll();
            // assert
            Assert.Equal(cp.Cache.getObject("testString"), null);
            Assert.Equal(cp.Cache.getText("testString"), "");
            Assert.Equal(cp.Cache.getInteger("testInt"), 0);
            Assert.Equal(cp.Cache.getDate("testDate"), DateTime.MinValue);
            Assert.Equal(cp.Cache.getBoolean("testTrue"), false);
            Assert.Equal(cp.Cache.getBoolean("testFalse"), false);
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
            // act
            cp.Cache.Save("keyA", "testValue","people");
            // assert
            Assert.Equal(cp.Cache.Read("keyA"), "testValue");
            // act
            CPCSBaseClass cs = cp.CSNew();
            if (cs.Insert("people"))
            {
                cs.SetField("name", "test");
            }
            cs.Close();
            // assert
            Assert.Equal(cp.Cache.Read("keyA"), "" );
            // dispose
            cp.Dispose();
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
            cp.Cache.Save("keyA", "testValue",  "a,b,c,d,e");
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
}
