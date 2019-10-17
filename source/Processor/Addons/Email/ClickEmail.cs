
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Models.Db;
//
namespace Contensive.Addons.Primitives {
    public class ClickEmailClass : Contensive.BaseClasses.AddonBaseClass {
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
                // -- Email click detected
                EmailDropModel emailDrop = DbBaseModel.create<EmailDropModel>(core.cpParent, core.docProperties.getInteger(rnEmailClickFlag));
                if (emailDrop != null) {
                    PersonModel recipient = DbBaseModel.create<PersonModel>(core.cpParent, core.docProperties.getInteger(rnEmailMemberId));
                    if (recipient != null) {
                        EmailLogModel log = new EmailLogModel() {
                            name = "User " + recipient.name + " clicked link from email drop " + emailDrop.name + " at " + core.doc.profileStartTime.ToString(),
                            emailDropId = emailDrop.id,
                            memberId = recipient.id,
                            logType = EmailLogTypeOpen
                        };
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
