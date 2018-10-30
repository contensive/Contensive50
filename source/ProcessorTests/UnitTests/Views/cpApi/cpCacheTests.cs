
using Contensive.Processor;
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Views {

    [TestClass()]
    public class cpCacheTests {
        //====================================================================================================
        /// <summary>
        /// cp.cache save read
        /// </summary>
        [TestMethod]
        public void Views_cpCache_LegacySaveRead() {
            // arrange
            CPClass cp = new CPClass(testAppName);
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
        [TestMethod]
        public void Views_cpCache_SetGet_integration() {
            // arrange
            CPClass cp = new CPClass(testAppName);
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
        [TestMethod]
        public void Views_cpCache_InvalidateAll_integration() {
            // arrange
            CPClass cp = new CPClass(testAppName);
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
        [TestMethod]
        public void Views_cpCache_InvalidateList_integration() {
            // arrange
            CPClass cp = new CPClass(testAppName);
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
        [TestMethod]
        public void Views_cpCache_TagInvalidationString() {
            // arrange
            CPClass cp = new CPClass(testAppName);
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
}
