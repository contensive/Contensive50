
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
//
namespace Contensive.Addons.Primitives {
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
                CoreController core = ((CPClass)cp).core;
                string Filename = core.siteProperties.getText("FaviconFilename", "");
                if (string.IsNullOrEmpty(Filename)) {
                    //
                    // no favicon, 404 the call
                    //
                    core.webServer.setResponseStatus(WebServerController.httpResponseStatus404);
                    core.webServer.setResponseContentType("image/gif");
                    core.doc.continueProcessing = false;
                    return string.Empty;
                } else {
                    core.doc.continueProcessing = false;
                    return core.webServer.redirect(GenericController.getCdnFileLink(core, Filename), "favicon request", false, false);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
