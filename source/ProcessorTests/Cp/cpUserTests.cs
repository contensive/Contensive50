
using Contensive.Processor;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Tests.TestConstants;
using Contensive.Processor.Controllers;

namespace Contensive.ProcessorTests.UnitTests.ViewTests {
    [TestClass()]
    public class cpUserTests {
        //====================================================================================================
        /// <summary>
        ///  Test 1 - cp ok without application (cluster mode).
        /// </summary>
        [TestMethod]
        public void cpUser_GetDate() {
            using (var cp = new CPClass(testAppName)) {
                // arrange
                string testInput = DateTime.MinValue.ToString();
                cp.User.SetProperty("testDate", DateTime.MinValue.ToString());
                // act
                DateTime testoutput = cp.User.GetDate("testDate");
                // assert
                Assert.AreEqual(DateTime.MinValue, testoutput);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void cpUser_IsInGroup_GroupName() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = ((CPClass)cp).core;
                core.cache.invalidateAll();
                core.session.verifyUser();
                string groupName = "Group-" + cp.Utils.GetRandomInteger().ToString();
                //
                cp.Group.Add(groupName);
                bool inGroupBeforeAddTest = cp.User.IsInGroup(groupName);
                // act
                cp.Group.AddUser(groupName);
                bool inGroupAfterAddWithoutAuth = cp.User.IsInGroup(groupName);
                // authenticate
                cp.User.LoginByID(cp.User.Id);
                // is in group after authenticating
                bool inGroupAfterAddWithAuth = cp.User.IsInGroup(groupName);
                //
                cp.Group.Delete(groupName);
                bool inGroupAfterDelete = cp.User.IsInGroup(groupName);
                // assert
                Assert.AreEqual(false, inGroupBeforeAddTest, "before add");
                Assert.AreEqual(false, inGroupAfterAddWithoutAuth, "before auth");
                Assert.AreEqual(true, inGroupAfterAddWithAuth, "after auth");
                Assert.AreEqual(false, inGroupAfterDelete, "after delete");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void cpUser_IsInGroup_2() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                CoreController core = ((CPClass)cp).core;
                core.cache.invalidateAll();
                core.session.verifyUser();
                string groupName = "Group-" + cp.Utils.GetRandomInteger().ToString();
                //
                cp.Group.Add(groupName);
                bool inGroupBeforeAddTest = cp.User.IsInGroup(groupName);
                // act
                cp.Group.AddUser(groupName);
                bool inGroupAfterTestBeforeAuth = cp.User.IsInGroup(groupName);
                //
                cp.User.LoginByID(cp.User.Id);
                bool inGroupAfterTestAfterAuth = cp.User.IsInGroup(groupName);

                // assert
                Assert.AreEqual(false, inGroupBeforeAddTest);
                Assert.AreEqual(false, inGroupAfterTestBeforeAuth);
                Assert.AreEqual(true, inGroupAfterTestAfterAuth);

            }
        }
    }
}
