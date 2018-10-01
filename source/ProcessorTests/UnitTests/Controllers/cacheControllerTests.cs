
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// cache controller unit tests
    /// </summary>
    [TestClass()]
    public class cacheControllerTests {
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_cache_blank() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_cache_SetGetString() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string key = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string value = "value" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(key, value);
                string readBack = cp.core.cache.getObject<string>(key);
                // assert
                Assert.AreEqual(value, readBack);
            }
        }
        //
        public class cacheTestClass {
            public string prop1 = "a";
            public int prop2 = 99;
            public DateTime prop3 = new DateTime(1999, 2, 2);
            public bool prop4 = true;
            public bool prop5 = false;
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectDefault() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var srcTest = new cacheTestClass();
                string key = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(key, srcTest);
                var dstTest = cp.core.cache.getObject<cacheTestClass>(key);
                // assert
                Assert.AreEqual(srcTest, dstTest);
            }
        }
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectNonDefault() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var srcTest = new cacheTestClass() {
                    prop1 = "b",
                    prop2 = 22,
                    prop3 = new DateTime(2003, 8, 5),
                    prop4 = false,
                    prop5 = true
                };
                string key = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(key, srcTest);
                var dstTest = cp.core.cache.getObject<cacheTestClass>(key);
                // assert
                Assert.AreEqual(srcTest, dstTest);
            }
        }
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithDependency() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTest = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string keyDependency = "dependencyKey1" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(keyDependency, "1");
                cp.Utils.Sleep(1);
                cp.core.cache.setObject(keyTest, originalObject, keyDependency);
                cp.Utils.Sleep(1);
                var valueBeforeDependencySave = cp.core.cache.getObject<cacheTestClass>(keyTest);
                cp.core.cache.setObject(keyDependency, "2");
                cp.Utils.Sleep(1);
                var valueAfterDependencySave = cp.core.cache.getObject<cacheTestClass>(keyTest);
                // assert
                Assert.AreEqual(originalObject, valueBeforeDependencySave);
                Assert.IsNull(valueAfterDependencySave);
            }
        }
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithDependencyList() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTest = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string keyDependency = "dependencyKey1" + GenericController.GetRandomInteger(cp.core).ToString();
                var dependencyList = new List<string>();
                dependencyList.Add(keyDependency);
                dependencyList.Add("fake");
                // act
                cp.core.cache.setObject(keyDependency, "1");
                cp.Utils.Sleep(1);
                cp.core.cache.setObject(keyTest, originalObject, dependencyList);
                cp.Utils.Sleep(1);
                var valueBeforeDependencySave = cp.core.cache.getObject<cacheTestClass>(keyTest);
                cp.core.cache.setObject(keyDependency, "2");
                cp.Utils.Sleep(1);
                var valueAfterDependencySave = cp.core.cache.getObject<cacheTestClass>(keyTest);
                // assert
                Assert.AreEqual(originalObject, valueBeforeDependencySave);
                Assert.IsNull(valueAfterDependencySave);
            }
        }
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithInvalidationDate() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTestWithDate = "testWithDate" + GenericController.GetRandomInteger(cp.core).ToString();
                string keyTestNoDate = "testNoDate" + GenericController.GetRandomInteger(cp.core).ToString();
                DateTime invalidateDate = DateTime.Now.AddMilliseconds(1);
                // act
                cp.core.cache.setObject(keyTestNoDate, originalObject);
                cp.core.cache.setObject(keyTestWithDate, originalObject, invalidateDate);
                cp.Utils.Sleep(1);
                var valueWithDate = cp.core.cache.getObject<cacheTestClass>(keyTestWithDate);
                var valueNoDate = cp.core.cache.getObject<cacheTestClass>(keyTestNoDate);
                // assert
                Assert.AreEqual(originalObject, valueNoDate);
                Assert.IsNull(valueWithDate);
            }
        }
        //
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithInvalidate() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTest = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(keyTest, originalObject);
                cp.Utils.Sleep(1);
                var valueBeforeInvalidate = cp.core.cache.getObject<cacheTestClass>(keyTest);
                cp.core.cache.invalidate(keyTest);
                cp.Utils.Sleep(1);
                var valueAfterDependencySave = cp.core.cache.getObject<cacheTestClass>(keyTest);
                // assert
                Assert.AreEqual(originalObject, valueBeforeInvalidate);
                Assert.IsNull(valueAfterDependencySave);
            }
        }
        //
        /// <summary>
        /// set a key with a dependency on a content. insert a record into content, key should be invalidated
        /// </summary>
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithInvalidateContent() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTest = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string content = PersonModel.contentName;
                // act
                cp.core.cache.setObject(keyTest, originalObject, content);
                cp.Utils.Sleep(1);
                var valueBefore = cp.core.cache.getObject<cacheTestClass>(keyTest);
                var person = PersonModel.add(cp.core );
                cp.Utils.Sleep(1);
                var valueAfter = cp.core.cache.getObject<cacheTestClass>(keyTest);
                // assert
                Assert.AreEqual(originalObject, valueBefore);
                Assert.IsNull(valueAfter);
            }
        }
        //
        /// <summary>
        /// set a key with a dependency on a content. insert a record into content, key should be invalidated
        /// </summary>
        [TestMethod()]
        public void Controllers_cache_SetGetObjectWithInvalidateTable() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string keyTest = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string table = PersonModel.contentTableName;
                // act
                cp.core.cache.setObject(keyTest, originalObject, table);
                cp.Utils.Sleep(1);
                var valueBefore = cp.core.cache.getObject<cacheTestClass>(keyTest);
                var person = PersonModel.add(cp.core);
                cp.Utils.Sleep(1);
                var valueAfter = cp.core.cache.getObject<cacheTestClass>(keyTest);
                // assert
                Assert.AreEqual(originalObject, valueBefore);
                Assert.IsNull(valueAfter);
            }
        }
        //
        /// <summary>
        /// set a key with a dependency on a content. insert a record into content, key should be invalidated
        /// </summary>
        [TestMethod()]
        public void Controllers_cache_SetGetAlias() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string testKey = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string testAlias = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(testKey, originalObject);
                cp.core.cache.setPtr(testAlias, testKey);
                var valueFromAlias = cp.core.cache.getObject<cacheTestClass>(testAlias);
                // assert
                Assert.AreEqual(originalObject, valueFromAlias);
            }
        }
        //
        /// <summary>
        /// if you invalidate a key, any alias pointing to it should be invalidated
        /// </summary>
        [TestMethod()]
        public void Controllers_cache_SetGetAliasInvalidateKey() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string testKey = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string testAlias = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(testKey, originalObject);
                cp.core.cache.setPtr(testAlias, testKey);
                cp.core.cache.invalidate(testAlias);
                var valueFromKey = cp.core.cache.getObject<cacheTestClass>(testKey);
                var valueFromAlias = cp.core.cache.getObject<cacheTestClass>(testAlias);
                // assert
                Assert.IsNull(valueFromKey);
                Assert.IsNull(valueFromAlias);
            }
        }
        //
        /// <summary>
        /// if you invalidate an alias, the parent key should be invalidated
        /// </summary>
        [TestMethod()]
        public void Controllers_cache_SetGetAliasInvalidateAlias() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                var originalObject = new cacheTestClass();
                string testKey = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                string testAlias = "test" + GenericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.core.cache.setObject(testKey, originalObject);
                cp.core.cache.setPtr(testAlias, testKey);
                cp.core.cache.invalidate(testAlias);
                var valueFromKey = cp.core.cache.getObject<cacheTestClass>(testKey);
                var valueFromAlias = cp.core.cache.getObject<cacheTestClass>(testAlias);
                // assert
                Assert.IsNull(valueFromKey);
                Assert.IsNull(valueFromAlias);
            }
        }

    }
}