﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Models.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contensive.Processor;
using static Tests.testConstants;

namespace Contensive.Models.Db.Tests {
    [TestClass()]
    public class DbBaseModelTests {
        [TestMethod()]
        public void derivedTableNameTest() {
            Assert.AreEqual("ccaddoncollections", DbBaseModel.derivedTableName(typeof(AddonCollectionModel)).ToLower());
        }

        [TestMethod()]
        public void derivedDataSourceNameTest() {
            Assert.AreEqual("default", DbBaseModel.derivedDataSourceName(typeof(AddonCollectionModel)).ToLower());
        }

        [TestMethod()]
        public void derivedNameFieldIsUniqueTest() {
            Assert.AreEqual(true, DbBaseModel.derivedNameFieldIsUnique(typeof(AddonCollectionModel)));
            Assert.AreEqual(false, DbBaseModel.derivedNameFieldIsUnique(typeof(AddonContentFieldTypeRulesModel)));
        }

        [TestMethod()]
        public void addDefaultTest() {
            using (CPClass cp = new CPClass(testAppName)) {
                var defaultValues = new Dictionary<string, string>();
                //
                // -- bool
                defaultValues.Add("admin", "true");
                defaultValues.Add("asAjax", "1");
                defaultValues.Add("htmlDocument", "false");
                defaultValues.Add("onPageStartEvent", "");
                defaultValues.Add("onPageEndEvent", "0");
                //
                // int
                defaultValues.Add("navTypeID", "1");
                defaultValues.Add("scriptingLanguageID", "");
                //
                // int nullable
                defaultValues.Add("processInterval", "");
                //
                // string
                defaultValues.Add("pageTitle", "asdf");
                defaultValues.Add("otherHeadTags", "");
                //
                // double
                //defaultValues.Add("", "");
                //
                // double nullable
                //defaultValues.Add("", "");
                //
                // date
                defaultValues.Add("processNextRun", "");
                //
                //
                AddonModel test = DbBaseModel.addDefault<AddonModel>(cp, defaultValues);
                //
                //
                Assert.AreEqual(true, test.admin);
                Assert.AreEqual(true, test.asAjax);
                Assert.AreEqual(false, test.htmlDocument);
                Assert.AreEqual(false, test.onPageStartEvent);
                Assert.AreEqual(false, test.onPageEndEvent);
                //
                Assert.AreEqual(1, test.navTypeID);
                Assert.AreEqual(0, test.scriptingLanguageID);
                //
                Assert.AreEqual(null, test.processInterval);
                //
                Assert.AreEqual("asdf", test.pageTitle);
                Assert.AreEqual("", test.otherHeadTags);
                //
                //Assert.AreEqual("", test.pageTitle);
                //
                //Assert.AreEqual("", test.pageTitle);
                //
                Assert.AreEqual(null, test.processNextRun);
            }
        }

        [TestMethod()]
        public void addDefaultTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void addDefaultTest2() {
            Assert.Fail();
        }

        [TestMethod()]
        public void addEmptyTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void addEmptyTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createTest2() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createTest3() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createByUniqueNameTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createByUniqueNameTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void saveTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void saveTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void saveTest2() {
            Assert.Fail();
        }

        [TestMethod()]
        public void deleteTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void deleteTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest2() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest3() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest4() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createListTest5() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createFirstOfListTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getRecordNameTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getRecordNameTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getRecordIdTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createEmptyTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createEmptyTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSelectSqlTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSelectSqlTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSelectSqlTest2() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getSelectSqlTest3() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getCountSqlTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void deleteRowsTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void containsFieldTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void isParentOfTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void isParentOfTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void isChildOfTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void isChildOfTest1() {
            Assert.Fail();
        }

        [TestMethod()]
        public void invalidateCacheOfRecordTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void invalidateCacheOfTableTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void createDependencyKeyInvalidateOnChangeTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getCountTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void isNullableTest() {
            Assert.Fail();
        }

        [TestMethod()]
        public void getCountTest1() {
            Assert.Fail();
        }
    }
}