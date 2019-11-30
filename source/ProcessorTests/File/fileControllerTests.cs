
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Tests.TestConstants;
using Contensive.Processor;

namespace Contensive.ProcessorTests.UnitTests.ControllerTests {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class fileControllerTests {
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_CdnFiles_AppendTest() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                string tmpFilename = "tmp" + GenericController.getRandomInteger(cp.core).ToString() + ".txt";
                string content = GenericController.getRandomInteger(cp.core).ToString();
                // act
                cp.CdnFiles.Append(tmpFilename, content);
                // assert
                Assert.AreEqual(content, cp.CdnFiles.Read(tmpFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_CdnFiles_SaveTest() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                string tmpFilename = "tmp" + GenericController.getRandomInteger(cp.core).ToString() + ".txt";
                string content = GenericController.getRandomInteger(cp.core).ToString();
                // act
                cp.CdnFiles.Save(tmpFilename, content);
                // assert
                Assert.AreEqual(content, cp.CdnFiles.Read(tmpFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_CdnFiles_CopyTest() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                string srcFilename = "src" + GenericController.getRandomInteger(cp.core).ToString() + ".txt";
                string tmpContent = GenericController.getRandomInteger(cp.core).ToString();
                string dstFilename = "dst" + GenericController.getRandomInteger(cp.core).ToString() + ".txt";
                // act
                cp.CdnFiles.Save(srcFilename, tmpContent);
                cp.CdnFiles.Copy(srcFilename, dstFilename);
                // assert
                Assert.AreEqual(tmpContent, cp.CdnFiles.Read(dstFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_CdnFiles_DeleteTest() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                string srcFilename = "src" + GenericController.getRandomInteger(cp.core).ToString() + ".txt";
                string tmpContent = GenericController.getRandomInteger(cp.core).ToString();
                // act
                cp.CdnFiles.Save(srcFilename, tmpContent);
                cp.CdnFiles.DeleteFile(srcFilename);
                // assert
                Assert.AreEqual("", cp.CdnFiles.Read(srcFilename));
            }
        }
    }
}