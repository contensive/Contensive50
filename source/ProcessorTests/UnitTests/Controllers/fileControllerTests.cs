
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Contensive.Processor.Tests.testConstants;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    //
    //====================================================================================================
    //
    [TestClass()]
    public class fileControllerTests {
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_CdnFiles_AppendTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string tmpFilename = "tmp" + genericController.GetRandomInteger(cp.core).ToString() + ".txt";
                string content = genericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.cdnFiles.append(tmpFilename, content);
                // assert
                Assert.AreEqual(content, cp.cdnFiles.read(tmpFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_CdnFiles_SaveTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string tmpFilename = "tmp" + genericController.GetRandomInteger(cp.core).ToString() + ".txt";
                string content = genericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.cdnFiles.save(tmpFilename, content);
                // assert
                Assert.AreEqual(content, cp.cdnFiles.read(tmpFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_CdnFiles_CopyTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string srcFilename = "src" + genericController.GetRandomInteger(cp.core).ToString() + ".txt";
                string tmpContent = genericController.GetRandomInteger(cp.core).ToString();
                string dstFilename = "dst" + genericController.GetRandomInteger(cp.core).ToString() + ".txt";
                // act
                cp.cdnFiles.save(srcFilename, tmpContent);
                cp.cdnFiles.copy(srcFilename, dstFilename);
                // assert
                Assert.AreEqual(tmpContent, cp.cdnFiles.read(dstFilename));
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod()]
        public void Controllers_CdnFiles_DeleteTest() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string srcFilename = "src" + genericController.GetRandomInteger(cp.core).ToString() + ".txt";
                string tmpContent = genericController.GetRandomInteger(cp.core).ToString();
                // act
                cp.cdnFiles.save(srcFilename, tmpContent);
                cp.cdnFiles.deleteFile(srcFilename);
                // assert
                Assert.AreEqual("", cp.cdnFiles.read(srcFilename));
            }
        }
    }
}