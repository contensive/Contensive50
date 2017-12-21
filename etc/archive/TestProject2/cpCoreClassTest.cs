using Contensive.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Contensive.Core
{
    
    
    /// <summary>
    ///This is a test class for cpCoreClassTest and is intended
    ///to contain all cpCoreClassTest Unit Tests
    ///</summary>
    [TestClass()]
    public class cpCoreClassTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for pattern_CpFreeFormUse
        ///</summary>
        [TestMethod()]
        [DeploymentItem("cfw.dll")]
        public void pattern_CpFreeFormUseTest() {
            //PrivateObject param0 = null; // TODO: Initialize to an appropriate value
            cpCoreClass_Accessor target = new cpCoreClass_Accessor(new Contensive.Core.CPClass()); // TODO: Initialize to an appropriate value
            string appName = "test44"; // TODO: Initialize to an appropriate value
            string expected = string.Empty; // TODO: Initialize to an appropriate value
            string actual;
            actual = target.pattern_CpFreeFormUse(appName);
            Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }
    }
}
