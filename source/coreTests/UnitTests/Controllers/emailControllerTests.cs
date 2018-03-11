
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Core.Controllers;
using Microsoft.Web.Administration;
using static Contensive.Core.Tests.testConstants;
using System.Linq;

namespace Contensive.Core.Tests.UnitTests.Controllers {
    [TestClass]
    public class emailControllerTests {
        //
        [TestMethod]
        public void Controllers_Email_GetBlockedList_test1() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string test1 = genericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                string test2 = genericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                // act
                emailController.addToBlockList(cp.core, test1);
                string blockList = Contensive.Core.Controllers.emailController.getBlockList(cp.core);
                // assert
                Assert.IsTrue(emailController.isOnBlockedList(cp.core, test1));
                Assert.IsFalse(emailController.isOnBlockedList(cp.core, test2));
            }
        }
        //
        [TestMethod]
        public void Controllers_Email_VerifyEmailAddress_test1() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                // act
                // assert
                Assert.IsFalse(emailController.verifyEmailAddress(cp.core, "123"));
                Assert.IsFalse(emailController.verifyEmailAddress(cp.core, "123@"));
                Assert.IsFalse(emailController.verifyEmailAddress(cp.core, "123@2"));
                Assert.IsFalse(emailController.verifyEmailAddress(cp.core, "123@2."));
                Assert.IsTrue(emailController.verifyEmailAddress(cp.core, "123@2.com"));
            }
        }
        //
        [TestMethod]
        public void Controllers_Email_queueAdHocEmail_test1() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string body = genericController.GetRandomInteger(cp.core).ToString() ;
                string sendStatus = "";
                string ResultLogFilename = "";
                // act
                emailController.queueAdHocEmail(cp.core, "to@kma.net", "from@kma.net", "subject", body,"bounce@kma.net","replyTo@kma.net", ResultLogFilename, true, true, 0, ref sendStatus);
                Contensive.BaseClasses.AddonBaseClass addon = new Contensive.Core.Addons.Email.processEmailClass();
                addon.Execute(cp);
                // assert
                Assert.AreEqual(1, cp.core.mockSmtpList.Count);
                coreController.smtpEmailClass sentEmail = cp.core.mockSmtpList.First();
                Assert.AreEqual("", sentEmail.AttachmentFilename);
                Assert.AreEqual("to@kma.net", sentEmail.email.toAddress);
                Assert.AreEqual("from@kma.net", sentEmail.email.fromAddress);
                Assert.AreEqual("bounce@kma.net", sentEmail.email.BounceAddress);
                Assert.AreEqual("replyTo@kma.net", sentEmail.email.replyToAddress);
                Assert.AreEqual("subject", sentEmail.email.subject);
                Assert.AreEqual(body, sentEmail.email.textBody);
            }
        }
        //
        [TestMethod]
        public void Controllers_Email_queuePersonEmail_test1() {
            using (Contensive.Core.CPClass cp = new Contensive.Core.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string body = genericController.GetRandomInteger(cp.core).ToString();
                var toPerson = Core.Models.DbModels.personModel.add(cp.core);
                Assert.IsNotNull(toPerson);
                toPerson.Email = genericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                toPerson.FirstName = genericController.GetRandomInteger(cp.core).ToString();
                toPerson.LastName = genericController.GetRandomInteger(cp.core).ToString();
                toPerson.save(cp.core);
                string sendStatus = "";
                // act
                Assert.IsTrue( emailController.queuePersonEmail(cp.core, toPerson, "from@kma.net", "subject", body, "bounce@kma.net", "replyTo@kma.net", true, true, 0, "", true, ref sendStatus, ""));
                Contensive.BaseClasses.AddonBaseClass addon = new Contensive.Core.Addons.Email.processEmailClass();
                addon.Execute(cp);
                // assert
                Assert.AreEqual(1, cp.core.mockSmtpList.Count);
                coreController.smtpEmailClass sentEmail = cp.core.mockSmtpList.First();
                Assert.AreEqual("", sentEmail.AttachmentFilename);
                Assert.AreEqual( toPerson.Email, sentEmail.email.toAddress);
                Assert.AreEqual("from@kma.net", sentEmail.email.fromAddress);
                Assert.AreEqual("bounce@kma.net", sentEmail.email.BounceAddress);
                Assert.AreEqual("replyTo@kma.net", sentEmail.email.replyToAddress);
                Assert.AreEqual("subject", sentEmail.email.subject);
                Assert.AreEqual(body, sentEmail.email.textBody);
            }
        }
    }
}
