
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using static Contensive.Core.Tests.testConstants;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass]
    public class genericControllerUnitTests {
        //
        [TestMethod]
        public void Controllers_empty_test() {
            // arrange
            // act
            // assert
        }
        //
        [TestMethod]
        public void Controllers_encodeEmptyDate_test() {
            // arrange
            DateTime zeroDate = new DateTime(1990, 8, 7);
            DateTime newBeginning = new DateTime(1999, 2, 2);
            DateTime theRenaissance = new DateTime(2003, 8, 5);
            // act
            DateTime date1 = genericController.encodeEmptyDate("", zeroDate);
            DateTime date2 = genericController.encodeEmptyDate("8/7/1990", newBeginning);
            DateTime date3 = genericController.encodeEmptyDate("123245", theRenaissance);
            // assert
            Assert.AreEqual(zeroDate, date1);
            Assert.AreEqual(zeroDate, date2);
            Assert.AreEqual(DateTime.MinValue, date3);
        }
        //
        [TestMethod]
        public void Controllers_encodeEmptyInteger_test() {
            // arrange
            // act
            int test1 = genericController.encodeEmptyInteger("1", 2);
            int test2 = genericController.encodeEmptyInteger("", 2);
            int test3 = genericController.encodeEmptyInteger(" ", 3);
            int test4 = genericController.encodeEmptyInteger("abcdefg", 4);
            // assert
            Assert.AreEqual(1, test1);
            Assert.AreEqual(2, test2);
            Assert.AreEqual(0, test3);
            Assert.AreEqual(0, test4);
        }
        //
        [TestMethod]
        public void Controllers_encodeEmpty_test() {
            // arrange
            // act
            string test1 = genericController.encodeEmpty("1", "2");
            string test2 = genericController.encodeEmpty("", "3");
            string test3 = genericController.encodeEmpty("4", "");
            // assert
            Assert.AreEqual("1", test1);
            Assert.AreEqual("3", test2);
            Assert.AreEqual("4", test3);
        }
        //
        [TestMethod]
        public void Controllers_isGUID_test() {
            // arrange
            // act
            // assert
            Assert.IsFalse(genericController.isGuid(""));
            Assert.IsFalse(genericController.isGuid(" "));
            Assert.IsTrue(genericController.isGuid("{C70BA82B-B314-466E-B29C-EAAD9C788C86}"));
            Assert.IsTrue(genericController.isGuid("C70BA82B-B314-466E-B29C-EAAD9C788C86"));
            Assert.IsTrue(genericController.isGuid("C70BA82BB314466EB29CEAAD9C788C86"));
            Assert.IsFalse(genericController.isGuid("C70BA82BB314466EB29CEAAD9C788C860"));
        }
        //
        [TestMethod]
        public void Controllers_getGUID_test() {
            // arrange
            // act
            string test1 = genericController.getGUID();
            string test2 = genericController.getGUID(true);
            string test3 = genericController.getGUID(false);
            // assert
            Assert.IsTrue(genericController.isGuid(test1));
            Assert.AreEqual(38, test1.Length);
            //
            Assert.IsTrue(genericController.isGuid(test2));
            Assert.AreEqual(38, test2.Length);
            Assert.AreEqual("{", test2.Substring(0, 1));
            Assert.AreEqual("}", test2.Substring(37, 1));
            //
            Assert.IsTrue(genericController.isGuid(test3));
            Assert.AreNotEqual("{", test3.Substring(0, 1));
        }
        //
        [TestMethod]
        public void Controllers_AddSpan_test1() {
            // arrange
            // act
            string result = genericController.AddSpan("test", "testClass");
            string resultNoClass = genericController.AddSpan("test");
            string resultNeither = genericController.AddSpan();
            // assert
            Assert.AreEqual("<span class=\"testClass\">test</span>", result);
            Assert.AreEqual("<span>test</span>", resultNoClass);
            Assert.AreEqual("<span/>", resultNeither);
        }
        //
        [TestMethod]
        public void Controllers_SplitDelimited_test1() {
            // arrange
            string in1 = "this and that";
            string in2 = "this and \"that and another\"";
            string in3 = "1,2,3,4,5,6,7,8,9,0";
            string in4 = "1,,2";
            string in5 = "1 \" 2 \" 3 \" 4\" \"5 \"";
            // act
            string[] out1 = genericController.SplitDelimited(in1, " ");
            string[] out2 = genericController.SplitDelimited(in2, " ");
            string[] out3 = genericController.SplitDelimited(in3, ",");
            string[] out4 = genericController.SplitDelimited(in4, ",");
            string[] out5 = genericController.SplitDelimited(in5, " ");
            // assert
            Assert.AreEqual(3, out1.Length);
            Assert.AreEqual(3, out2.Length);
            Assert.AreEqual(10, out3.Length);
            Assert.AreEqual(3, out4.Length);
            Assert.AreEqual(5, out5.Length);
        }
    }
}
