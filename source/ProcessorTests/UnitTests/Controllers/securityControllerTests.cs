
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
    public class securityControllerTests {

        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_Security_blank() {
            using (CPClass cp = new CPClass(testAppName)) {
                // arrange
                // act
                // assert
                Assert.AreEqual("", "");
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_Security_twoWayEncode_des() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string source = "All work and no play makes johnny a dull boy.";
                // act
                string resultEncrypted = SecurityController.twoWayEncrypt(cp.core, source, SecurityController.TwoWayCiphers.des);
                string resultEncryptedAes = SecurityController.twoWayEncrypt(cp.core, source, SecurityController.TwoWayCiphers.aes);
                string resultDecrypted = SecurityController.twoWayDecrypt(cp.core, resultEncrypted, SecurityController.TwoWayCiphers.des);
                string blankEncrypted = SecurityController.twoWayEncrypt(cp.core, "", SecurityController.TwoWayCiphers.des);
                string blankDecrypted = SecurityController.twoWayDecrypt(cp.core, "", SecurityController.TwoWayCiphers.des);
                string invalidDecrypted = SecurityController.twoWayDecrypt(cp.core, source, SecurityController.TwoWayCiphers.des);
                // assert
                Assert.AreEqual(source, resultDecrypted);
                Assert.AreNotEqual(resultEncrypted, resultEncryptedAes);
                Assert.AreNotEqual("", blankEncrypted);
                Assert.AreNotEqual("", blankEncrypted);
                Assert.AreEqual("", blankDecrypted);
                Assert.AreEqual("", invalidDecrypted);
            }
        }
        //
        //====================================================================================================
        //
        [TestMethod]
        public void Controllers_Security_twoWayEncode_aes() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                // arrange
                string source = "All work and no play makes johnny a dull boy again.";
                // act
                string resultEncrypted = SecurityController.twoWayEncrypt(cp.core, source, SecurityController.TwoWayCiphers.aes);
                string resultEncryptedDes = SecurityController.twoWayEncrypt(cp.core, source, SecurityController.TwoWayCiphers.des);
                string resultDecrypted = SecurityController.twoWayDecrypt(cp.core, resultEncrypted, SecurityController.TwoWayCiphers.aes);
                string blankEncrypted = SecurityController.twoWayEncrypt(cp.core, "", SecurityController.TwoWayCiphers.aes);
                string blankDecrypted = SecurityController.twoWayDecrypt(cp.core, "", SecurityController.TwoWayCiphers.aes);
                string invalidDecrypted = SecurityController.twoWayDecrypt(cp.core, source, SecurityController.TwoWayCiphers.aes);
                // assert
                Assert.AreEqual(source, resultDecrypted);
                Assert.AreNotEqual(resultEncrypted, resultEncryptedDes);
                Assert.AreEqual("", invalidDecrypted);
            }
        }
        //====================================================================================================
        /// <summary>
        /// coreSecurity
        /// </summary>
        [TestMethod]
        public void Controllers_Security_EncryptDecrypt() {
            // arrange
            using (CPClass cp = new CPClass(testAppName)) {
                int testNumber = 12345;
                DateTime testDate = new DateTime(1990, 8, 7);
                //
                // act
                string token = SecurityController.encodeToken(cp.core, testNumber, testDate);
                //
                // assert
                var result = SecurityController.decodeToken(cp.core, token);
                Assert.AreEqual(testNumber, result.id);
                Assert.AreEqual(testDate, result.timeStamp);
            }
        }

    }
}