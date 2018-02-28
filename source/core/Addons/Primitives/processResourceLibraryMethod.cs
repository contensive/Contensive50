
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
                coreController core = processor.core;
                //
                // -- resource library
                core.doc.addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary);
                string EditorObjectName = core.docProperties.getText("EditorObjectName");
                string LinkObjectName = core.docProperties.getText("LinkObjectName");
                if (!string.IsNullOrEmpty(EditorObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    //
                    core.doc.addRefreshQueryString("EditorObjectName", EditorObjectName);
                    core.html.addScriptLinkSrc("/ccLib/ClientSide/dialogs.js", "Resource Library");
                    //Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                    core.doc.setMetaContent(0, 0);
                    core.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Resource Library");
                    string Copy = core.html.getResourceLibrary("", true, EditorObjectName, LinkObjectName, true);
                    string htmlBody = ""
                        + genericController.htmlIndent(core.html.getPanelHeader("Contensive Resource Library")) + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + cr2 + "<div style=\"border-top:1px solid white;border-bottom:1px solid black;height:2px\"><img alt=\"spacer\" src=\"/ccLib/images/spacer.gif\" width=1 height=1></div>"
                        + genericController.htmlIndent(Copy) + "\r</td></tr>"
                        + "\r<tr><td>"
                        + genericController.htmlIndent(core.html.getHtmlBodyEnd(false, false)) + "\r</td></tr></table>"
                        + "\r<script language=javascript type=\"text/javascript\">fixDialog();</script>"
                        + "";
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
                    result = core.html.getHtmlDoc(htmlBody, htmlBodyTag, false, false);
                    core.doc.continueProcessing = false;
                } else if (!string.IsNullOrEmpty(LinkObjectName)) {
                    //
                    // Open a page compatible with a dialog
                    core.doc.addRefreshQueryString("LinkObjectName", LinkObjectName);
                    core.html.addScriptLinkSrc("/ccLib/ClientSide/dialogs.js", "Resource Library");
                    core.doc.setMetaContent(0, 0);
                    core.html.addScriptCode_onLoad("document.body.style.overflow='scroll';", "Resource Library");
                    string htmlBody = ""
                        + core.html.getPanelHeader("Contensive Resource Library") + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\"><tr><td>"
                        + core.html.getResourceLibrary("", true, EditorObjectName, LinkObjectName, true) + "\r</td></tr></table>"
                        + "\r<script language=javascript type=text/javascript>fixDialog();</script>"
                        + "";
                    string htmlBodyTag = "<body class=\"ccBodyAdmin ccCon\" style=\"overflow:scroll\">";
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
