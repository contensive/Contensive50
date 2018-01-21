
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
using Contensive.BaseClasses;
//
namespace Contensive.Core.Addons.Primitives {
    public class processSiteExplorerMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                cpCore.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer);
                string LinkObjectName = cpCore.docProperties.getText("LinkObjectName");
                if (!string.IsNullOrEmpty(LinkObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    //
                    cpCore.doc.addRefreshQueryString("LinkObjectName", LinkObjectName);
                    cpCore.html.addTitle("Site Explorer");
                    cpCore.doc.setMetaContent(0, 0);
                    string copy = cpCore.addon.execute(addonModel.createByName(cpCore, "Site Explorer"), new CPUtilsBaseClass.addonExecuteContext() { addonType = CPUtilsBaseClass.addonContext.ContextPage });
                    cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Site Explorer");
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
                    string htmlBody = ""
                        + genericController.htmlIndent(cpCore.html.getPanelHeader("Contensive Site Explorer")) + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + genericController.htmlIndent(copy) + "\r</td></tr></table>"
                        + "";
                    result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, false, false);
                    cpCore.doc.continueProcessing = false;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
