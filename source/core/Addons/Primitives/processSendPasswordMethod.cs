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
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.Primitives {
    public class processSendPasswordMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                // -- send password
                string Emailtext = core.docProperties.getText("email");
                if (!string.IsNullOrEmpty(Emailtext)) {
                    string sendStatus = "";
                    loginController.sendPassword(core, Emailtext, ref sendStatus);
                    result += ""
                        + "<div style=\"width:300px;margin:100px auto 0 auto;\">"
                        + "<p>An attempt to send login information for email address '" + Emailtext + "' has been made.</p>"
                        + "<p><a href=\"?" + core.doc.refreshQueryString + "\">Return to the Site.</a></p>"
                        + "</div>";
                    core.doc.continueProcessing = false;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
