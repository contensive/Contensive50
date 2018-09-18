﻿
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.Primitives {
    public class openEmailClass : Contensive.BaseClasses.AddonBaseClass {
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
                int emailDropId = core.docProperties.getInteger(rnEmailOpenFlag);
                if (emailDropId != 0) {
                    //
                    // -- Email open detected. Log it and redirect to a 1x1 spacer
                    EmailDropModel emailDrop = EmailDropModel.create(core, emailDropId);
                    if (emailDrop != null) {
                        personModel recipient = personModel.create(core, core.docProperties.getInteger(rnEmailMemberID));
                        if (recipient != null) {
                           EmailLogModel log = new EmailLogModel() {
                                name = "User " + recipient.name + " opened email drop " + emailDrop.name + " at " + core.doc.profileStartTime.ToString(),
                                emailDropID = emailDrop.id,
                                memberID = recipient.id,
                                logType = EmailLogTypeOpen
                            };
                        }
                        core.webServer.redirect(NonEncodedLink: core.webServer.requestProtocol + core.webServer.requestDomain + "/ccLib/images/spacer.gif", RedirectReason: "Group Email Open hit, redirecting to a dummy image", IsPageNotFound: false, allowDebugMessage: false);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}