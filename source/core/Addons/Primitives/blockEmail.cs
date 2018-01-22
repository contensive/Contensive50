
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.Primitives {
    public class blockEmailClass : Contensive.BaseClasses.AddonBaseClass {
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
                CPClass processor = (CPClass)cp;
                coreController core = processor.core;
                //
                // -- click spam block detected
                if (true) {
                    //
                    string recipientEmailToBlock = core.docProperties.getText(rnEmailBlockRecipientEmail);
                    if (string.IsNullOrEmpty(recipientEmailToBlock)) {
                        List<personModel> recipientList = personModel.createList(core, "(email=" + core.db.encodeSQLText(recipientEmailToBlock) + ")");
                        foreach (var recipient in recipientList) {
                            recipient.AllowBulkEmail = false;
                            recipient.save(core);
                            //
                            // -- Email spam footer was clicked, clear the AllowBulkEmail field
                            emailController.addToBlockList(core, recipientEmailToBlock);
                            //
                            // -- log entry to track the result of this email drop
                            int emailDropId = core.docProperties.getInteger(rnEmailBlockRequestDropID);
                            if (emailDropId != 0) {
                                emailDropModel emailDrop = emailDropModel.create(core, emailDropId);
                                if (emailDrop != null) {
                                    emailLogModel log = new emailLogModel() {
                                        name = "User " + recipient.name + " clicked linked spam block from email drop " + emailDrop.name + " at " + core.doc.profileStartTime.ToString(),
                                        EmailDropID = emailDrop.id,
                                        MemberID = recipient.id,
                                        LogType = EmailLogTypeBlockRequest
                                    };
                                }
                            }
                            return core.webServer.redirect(core.webServer.requestProtocol + core.webServer.requestDomain + "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.");
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
