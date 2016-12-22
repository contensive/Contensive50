
using Contensive.Core;
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
    }
}
