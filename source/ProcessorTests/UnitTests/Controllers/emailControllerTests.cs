
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Tests.testConstants;
using System.Linq;

namespace Contensive.Processor.Tests.UnitTests.Controllers {
    [TestClass]
    public class emailControllerTests {
        //
        [TestMethod]
        public void Controllers_Email_GetBlockedList_test1() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string test1 = GenericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                string test2 = GenericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                // act
                EmailController.addToBlockList(cp.core, test1);
                string blockList = Contensive.Processor.Controllers.EmailController.getBlockList(cp.core);
                // assert
                Assert.IsTrue(EmailController.isOnBlockedList(cp.core, test1));
                Assert.IsFalse(EmailController.isOnBlockedList(cp.core, test2));
            }
        }
        //
        [TestMethod]
        public void Controllers_Email_VerifyEmailAddress_test1() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                // act
                // assert
                Assert.IsFalse(EmailController.verifyEmailAddress(cp.core, "123"));
                Assert.IsFalse(EmailController.verifyEmailAddress(cp.core, "123@"));
                Assert.IsFalse(EmailController.verifyEmailAddress(cp.core, "123@2"));
                Assert.IsFalse(EmailController.verifyEmailAddress(cp.core, "123@2."));
                Assert.IsTrue(EmailController.verifyEmailAddress(cp.core, "123@2.com"));
            }
        }
        //
        [TestMethod]
        public void Controllers_Email_queueAdHocEmail_test1() {
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string body = GenericController.GetRandomInteger(cp.core).ToString() ;
                string sendStatus = "";
                string ResultLogFilename = "";
                // act
                EmailController.queueAdHocEmail(cp.core, "to@kma.net", "from@kma.net", "subject", body,"bounce@kma.net","replyTo@kma.net", ResultLogFilename, true, true, 0, ref sendStatus);
                Contensive.BaseClasses.AddonBaseClass addon = new Contensive.Addons.Email.ProcessEmailClass();
                addon.Execute(cp);
                // assert
                Assert.AreEqual(1, cp.core.mockSmtpList.Count);
                CoreController.SmtpEmailClass sentEmail = cp.core.mockSmtpList.First();
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
            using (Contensive.Processor.CPClass cp = new Contensive.Processor.CPClass(testAppName)) {
                cp.core.mockSmtp = true;
                // arrange
                string body = GenericController.GetRandomInteger(cp.core).ToString();
                var toPerson = Processor.Models.Db.PersonModel.addDefault(cp.core, Processor.Models.Domain.CDefDomainModel.create(cp.core, Processor.Models.Db.PersonModel.contentName));
                Assert.IsNotNull(toPerson);
                toPerson.email = GenericController.GetRandomInteger(cp.core).ToString() + "@kma.net";
                toPerson.firstName = GenericController.GetRandomInteger(cp.core).ToString();
                toPerson.lastName = GenericController.GetRandomInteger(cp.core).ToString();
                toPerson.save(cp.core);
                string sendStatus = "";
                // act
                Assert.IsTrue( EmailController.queuePersonEmail(cp.core, toPerson, "from@kma.net", "subject", body, "bounce@kma.net", "replyTo@kma.net", true, true, 0, "", true, ref sendStatus, ""));
                Contensive.BaseClasses.AddonBaseClass addon = new Contensive.Addons.Email.ProcessEmailClass();
                addon.Execute(cp);
                // assert
                Assert.AreEqual(1, cp.core.mockSmtpList.Count);
                CoreController.SmtpEmailClass sentEmail = cp.core.mockSmtpList.First();
                Assert.AreEqual("", sentEmail.AttachmentFilename);
                Assert.AreEqual( toPerson.email, sentEmail.email.toAddress);
                Assert.AreEqual("from@kma.net", sentEmail.email.fromAddress);
                Assert.AreEqual("bounce@kma.net", sentEmail.email.BounceAddress);
                Assert.AreEqual("replyTo@kma.net", sentEmail.email.replyToAddress);
                Assert.AreEqual("subject", sentEmail.email.subject);
                Assert.AreEqual(body, sentEmail.email.textBody);
            }
        }
    }
}
