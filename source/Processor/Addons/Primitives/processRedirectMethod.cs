
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
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.Primitives {
    public class processRedirectMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                // ----- Redirect with RC and RI
                //
                core.doc.redirectContentID = core.docProperties.getInteger(rnRedirectContentId);
                core.doc.redirectRecordID = core.docProperties.getInteger(rnRedirectRecordId);
                if (core.doc.redirectContentID != 0 & core.doc.redirectRecordID != 0) {
                    string ContentName = CdefController.getContentNameByID(core, core.doc.redirectContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        WebServerController.main_RedirectByRecord_ReturnStatus(core, ContentName, core.doc.redirectRecordID);
                        result = "";
                        core.doc.continueProcessing = false;
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
