
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
using Contensive.Core.Models.Complex;
//
namespace Contensive.Core.Addons.Primitives {
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
                CPClass processor = (CPClass)cp;
                coreController core = processor.core;
                //
                // ----- Redirect with RC and RI
                //
                core.doc.redirectContentID = core.docProperties.getInteger(rnRedirectContentId);
                core.doc.redirectRecordID = core.docProperties.getInteger(rnRedirectRecordId);
                if (core.doc.redirectContentID != 0 & core.doc.redirectRecordID != 0) {
                    string ContentName = cdefModel.getContentNameByID(core, core.doc.redirectContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        iisController.main_RedirectByRecord_ReturnStatus(core, ContentName, core.doc.redirectRecordID);
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
