
using System;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;
using Contensive.Models.Db;
//
namespace Contensive.Addons.Primitives {
    public class BlockEmailClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                //
                // -- click spam block detected
                if (true) {
                    //
                    string recipientEmailToBlock = core.docProperties.getText(rnEmailBlockRecipientEmail);
                    if (string.IsNullOrEmpty(recipientEmailToBlock)) {
                        List<PersonModel> recipientList = DbBaseModel.createList<PersonModel>(core.cpParent, "(email=" + DbController.encodeSQLText(recipientEmailToBlock) + ")");
                        foreach (var recipient in recipientList) {
                            recipient.allowBulkEmail = false;
                            recipient.save(cp);
                            //
                            // -- Email spam footer was clicked, clear the AllowBulkEmail field
                            EmailController.addToBlockList(core, recipientEmailToBlock);
                            //
                            // -- log entry to track the result of this email drop
                            int emailDropId = core.docProperties.getInteger(rnEmailBlockRequestDropID);
                            if (emailDropId != 0) {
                                EmailDropModel emailDrop = DbBaseModel.create<EmailDropModel>(cp, emailDropId);
                                if (emailDrop != null) {
                                    EmailLogModel log = new EmailLogModel() {
                                        name = "User " + recipient.name + " clicked linked spam block from email drop " + emailDrop.name + " at " + core.doc.profileStartTime.ToString(),
                                        emailDropID = emailDrop.id,
                                        memberID = recipient.id,
                                        logType = EmailLogTypeBlockRequest
                                    };
                                }
                            }
                            return core.webServer.redirect(core.webServer.requestProtocol + core.webServer.requestDomain + "https://s3.amazonaws.com/cdn.contensive.com/assets/20190729/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.");
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
