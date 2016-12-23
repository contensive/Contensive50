
using Contensive.Core;
using Contensive.BaseClasses;
using Xunit;

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
        private void cpCacheSaveRead_integration()
        {
            // arrange
            CPClass cp = new CPClass("testapp");
            // act
            cp.Cache.Save("testKey", "testValue");
            // assert
            Assert.Equal(cp.Cache.Read("testKey"), "testValue");
            // dispose
            cp.Dispose();
        }
        /// <summary>
        /// cp.cache save read
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
    }
}
