
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
    public class faviconIcoClass : Contensive.BaseClasses.AddonBaseClass {
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
                string Filename = core.siteProperties.getText("FaviconFilename", "");
                if (string.IsNullOrEmpty(Filename)) {
                    //
                    // no favicon, 404 the call
                    //
                    core.webServer.setResponseStatus(iisController.httpResponseStatus404);
                    core.webServer.setResponseContentType("image/gif");
                    core.doc.continueProcessing = false;
                    return string.Empty;
                } else {
                    core.doc.continueProcessing = false;
                    return core.webServer.redirect(genericController.getCdnFileLink(core, Filename), "favicon request", false, false);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
