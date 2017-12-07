
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Addons.Core {
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
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;
                //
                // -- Email click detected
                emailDropModel emailDrop = emailDropModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailClickFlag));
                if (emailDrop != null) {
                    personModel recipient = personModel.create(cpCore, cpCore.docProperties.getInteger(rnEmailMemberID));
                    if (recipient != null) {
                        emailLogModel log = new emailLogModel() {
                            name = "User " + recipient.name + " clicked link from email drop " + emailDrop.name + " at " + cpCore.doc.profileStartTime.ToString(),
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
