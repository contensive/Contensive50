
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
    public class processSiteStyleEditorClass : Contensive.BaseClasses.AddonBaseClass {
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
                if (core.session.isAuthenticated & core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Save the site sites
                    //
                    core.appRootFiles.saveFile(DynamicStylesFilename, core.docProperties.getText("SiteStyles"));
                    if (core.docProperties.getBoolean(RequestNameInlineStyles)) {
                        //
                        // Inline Styles
                        //
                        core.siteProperties.setProperty("StylesheetSerialNumber", "0");
                    } else {
                        //
                        // Linked Styles
                        // Bump the Style Serial Number so next fetch is not cached
                        //
                        int StyleSN = core.siteProperties.getInteger("StylesheetSerialNumber", 0);
                        StyleSN = StyleSN + 1;
                        core.siteProperties.setProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN));
                        //
                        // Save new public stylesheet
                        //
                        //Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", html.html_getStyleSheet2(0, 0))
                        //Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", html.getStyleSheetDefault())
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
