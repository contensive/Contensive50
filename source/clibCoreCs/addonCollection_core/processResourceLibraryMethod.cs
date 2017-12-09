
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
//
namespace Contensive.Addons.Core {
    public class processResourceLibraryMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                // -- resource library
                cpCore.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary);
                string EditorObjectName = cpCore.docProperties.getText("EditorObjectName");
                string LinkObjectName = cpCore.docProperties.getText("LinkObjectName");
                if (!string.IsNullOrEmpty(EditorObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    //
                    cpCore.doc.addRefreshQueryString("EditorObjectName", EditorObjectName);
                    cpCore.html.addScriptLink_Head("/ccLib/ClientSide/dialogs.js", "Resource Library");
                    //Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                    cpCore.doc.setMetaContent(0, 0);
                    cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Resource Library");
                    string Copy = cpCore.html.main_GetResourceLibrary2("", true, EditorObjectName, LinkObjectName, true);
                    string htmlBody = ""
                        + genericController.htmlIndent(cpCore.html.main_GetPanelHeader("Contensive Resource Library")) + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + cr2 + "<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"spacer\" src=\"/ccLib/images/spacer.gif\" width=1 height=1></div>"
                        + genericController.htmlIndent(Copy) + "\r</td></tr>"
                        + "\r<tr><td>"
                        + genericController.htmlIndent(cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(false, false)) + "\r</td></tr></table>"
                        + "\r<script language=javascript type=\"text/javascript\">fixDialog();</script>"
                        + "";
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
                    result = cpCore.html.getHtmlDoc(htmlBody, htmlBodyTag, false, false);
                    cpCore.doc.continueProcessing = false;
                } else if (!string.IsNullOrEmpty(LinkObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    cpCore.doc.addRefreshQueryString("LinkObjectName", LinkObjectName);
                    cpCore.html.addScriptLink_Head("/ccLib/ClientSide/dialogs.js", "Resource Library");
                    cpCore.doc.setMetaContent(0, 0);
                    cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Resource Library");
                    string htmlBody = ""
                        + cpCore.html.main_GetPanelHeader("Contensive Resource Library") + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + cpCore.html.main_GetResourceLibrary2("", true, EditorObjectName, LinkObjectName, true) + "\r</td></tr></table>"
                        + "\r<script language=javascript type=text/javascript>fixDialog();</script>"
                        + "";
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
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
