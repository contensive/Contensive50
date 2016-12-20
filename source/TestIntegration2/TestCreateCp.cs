
using System;
using Contensive.Core;
using Contensive.BaseClasses;

using Xunit;

namespace TestIntegration2
{

    public class TestCreateCp
    {
        [Fact]
        public void TestMethod1()
        {
            // arange
            CPClass cp = new CPClass("testapp");
            // act
            bool appOK = cp.appOk;
            // assert
            Assert.Equal(appOK,true );
            cp.Dispose();
        }
    }
}
