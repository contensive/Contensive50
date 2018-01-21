
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
                CPClass processor = (CPClass)cp;
                coreController cpCore = processor.core;
                //
                // -- Robots.txt
                string Filename = "config/RobotsTxtBase.txt";
                // 
                // set this way because the preferences page needs a filename in a site property (enhance later)
                cpCore.siteProperties.setProperty("RobotsTxtFilename", Filename);
                result = cpCore.cdnFiles.readFile(Filename);
                if (string.IsNullOrEmpty(result)) {
                    //
                    // save default robots.txt
                    //
                    result = "User-agent: *\r\nDisallow: /admin/\r\nDisallow: /images/";
                    cpCore.appRootFiles.saveFile(Filename, result);
                }
                result = result + cpCore.addonCache.robotsTxt;
                cpCore.webServer.setResponseContentType("text/plain");
                cpCore.doc.continueProcessing = false;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
