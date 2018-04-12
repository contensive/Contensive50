
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
                coreController core = processor.core;
                //
                core.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer);
                string LinkObjectName = core.docProperties.getText("LinkObjectName");
                if (!string.IsNullOrEmpty(LinkObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    //
                    core.doc.addRefreshQueryString("LinkObjectName", LinkObjectName);
                    core.html.addTitle("Site Explorer");
                    core.doc.setMetaContent(0, 0);
                    string copy = core.addon.execute(addonModel.createByName(core, "Site Explorer"), new CPUtilsBaseClass.addonExecuteContext() {
                        addonType = CPUtilsBaseClass.addonContext.ContextPage,
                        errorContextMessage = "processing site explorer response"
                    });
                    core.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Site Explorer");
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
                    string htmlBody = ""
                        + genericController.nop(core.html.getPanelHeader("Contensive Site Explorer")) + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + genericController.nop(copy) + "\r</td></tr></table>"
                        + "";
                    result = core.html.getHtmlDoc(htmlBody, htmlBodyTag, false, false);
                    core.doc.continueProcessing = false;
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
