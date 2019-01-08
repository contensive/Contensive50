
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;

namespace Contensive.ProcessorTests.UnitTests {
    [TestClass]
    public class extensionMethodsUnitTests {
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Extensions_String_Left_test1() {
            // arrange
            string arg1 = "1234567890";
            string arg2 = "";
            string arg3 = null;
            // act
            string result1 = arg1.Left(5);
            string result11 = arg1.Left(10);
            string result12 = arg1.Left(20);
            string result13 = arg1.Left(0);
            string result2 = arg2.Left(10);
            string result3 = arg3.Left(10);
            // assert
            Assert.AreEqual("12345", result1);
            Assert.AreEqual("1234567890", result11);
            Assert.AreEqual("1234567890", result12);
            Assert.AreEqual("", result13);
            Assert.AreEqual("", result2);
            Assert.AreEqual("", result3);
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Extensions_String_Right_Test1() {
            // arrange
            string arg1 = "1234567890";
            string arg2 = "";
            string arg3 = null;
            // act
            string result1 = arg1.Right(5);
            string result11 = arg1.Right(10);
            string result12 = arg1.Right(20);
            string result13 = arg1.Right(0);
            string result2 = arg2.Right(10);
            string result3 = arg3.Right(10);
            // assert
            Assert.AreEqual("67890", result1);
            Assert.AreEqual("1234567890", result11);
            Assert.AreEqual("1234567890", result12);
            Assert.AreEqual("", result13);
            Assert.AreEqual("", result2);
            Assert.AreEqual("", result3);
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Extensions_String_IsNumeric_Test1() {
            // arrange
            string arg1 = "1234567890";
            string arg2 = "";
            string arg3 = null;
            string arg4 = new DateTime(2017, 8, 7).ToString() ;
            string arg5 = "true";
            string arg6 = "false";
            string arg7 = "$1234";
            string arg8 = "12.34";
            string arg9 = "1,234";
            string arg10 = "a";
            // act
            // assert
            Assert.AreEqual(true, arg1.IsNumeric());
            Assert.AreEqual(false, arg2.IsNumeric());
            Assert.AreEqual(false, arg3.IsNumeric());
            Assert.AreEqual(false, arg4.IsNumeric());
            Assert.AreEqual(false, arg5.IsNumeric());
            Assert.AreEqual(false, arg6.IsNumeric());
            Assert.AreEqual(false, arg7.IsNumeric());
            Assert.AreEqual(true, arg8.IsNumeric());
            Assert.AreEqual(true, arg9.IsNumeric());
            Assert.AreEqual(false, arg10.IsNumeric());
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Extensions_DateTime_IsOld_Test1() {
            // arrange
            var arg1 = new DateTime(1, 1, 1);
            var arg2 = new DateTime(2017, 1, 1);
            var arg3 = new DateTime(1899,12,31);
            var arg4 = new DateTime(1990, 1, 1);
            // act
            // assert
            Assert.AreEqual(true, arg1.isOld());
            Assert.AreEqual(false, arg2.isOld());
            Assert.AreEqual(true, arg3.isOld());
            Assert.AreEqual(false, arg4.isOld());
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Extensions_DateTime_MinDateIfOld_Test1() {
            // arrange
            var arg1 = new DateTime(1, 1, 1);
            var arg2 = new DateTime(2017, 1, 1);
            var arg3 = new DateTime(1899, 12, 31);
            var arg4 = new DateTime(1990, 1, 1);
            // act
            // assert
            Assert.AreEqual(DateTime.MinValue, arg1.MinValueIfOld());
            Assert.AreEqual(arg2, arg2.MinValueIfOld());
            Assert.AreEqual(DateTime.MinValue, arg3.MinValueIfOld());
            Assert.AreEqual(arg4, arg4.MinValueIfOld());
        }


    }
}
