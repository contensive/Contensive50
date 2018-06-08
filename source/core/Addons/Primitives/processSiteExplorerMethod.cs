
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
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
using Contensive.BaseClasses;
//
namespace Contensive.Addons.Primitives {
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
                coreController core = ((CPClass)cp).core;
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
                    string htmlBodyTag = "<body class=\"container-fluid ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
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
