
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
    public class robotsTxtClass : Contensive.BaseClasses.AddonBaseClass {
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
                // -- Robots.txt
                string Filename = "config/RobotsTxtBase.txt";
                // 
                // set this way because the preferences page needs a filename in a site property (enhance later)
                core.siteProperties.setProperty("RobotsTxtFilename", Filename);
                result = core.cdnFiles.readFileText(Filename);
                if (string.IsNullOrEmpty(result)) {
                    //
                    // save default robots.txt
                    //
                    result = "User-agent: *\r\nDisallow: /admin/\r\nDisallow: /images/";
                    core.appRootFiles.saveFile(Filename, result);
                }
                result += core.addonCache.robotsTxt;
                core.webServer.setResponseContentType("text/plain");
                core.doc.continueProcessing = false;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
