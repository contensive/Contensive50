
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.Primitives {
    public class clickEmailClass : Contensive.BaseClasses.AddonBaseClass {
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
                coreController core = ((CPClass)cp).core;
                //
                // -- Email click detected
                emailDropModel emailDrop = emailDropModel.create(core, core.docProperties.getInteger(rnEmailClickFlag));
                if (emailDrop != null) {
                    personModel recipient = personModel.create(core, core.docProperties.getInteger(rnEmailMemberID));
                    if (recipient != null) {
                        emailLogModel log = new emailLogModel() {
                            name = "User " + recipient.name + " clicked link from email drop " + emailDrop.name + " at " + core.doc.profileStartTime.ToString(),
                            EmailDropID = emailDrop.id,
                            MemberID = recipient.id,
                            LogType = EmailLogTypeOpen
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
