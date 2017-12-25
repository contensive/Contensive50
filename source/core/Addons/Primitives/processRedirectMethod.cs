
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
                coreClass cpCore = processor.core;
                //
                // ----- Redirect with RC and RI
                //
                cpCore.doc.redirectContentID = cpCore.docProperties.getInteger(rnRedirectContentId);
                cpCore.doc.redirectRecordID = cpCore.docProperties.getInteger(rnRedirectRecordId);
                if (cpCore.doc.redirectContentID != 0 & cpCore.doc.redirectRecordID != 0) {
                    string ContentName = cdefModel.getContentNameByID(cpCore, cpCore.doc.redirectContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        iisController.main_RedirectByRecord_ReturnStatus(cpCore, ContentName, cpCore.doc.redirectRecordID);
                        result = "";
                        cpCore.doc.continueProcessing = false;
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
