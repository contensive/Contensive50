
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Processor.Controllers {
    public static class ExportTemplateController {
        // 
        // ====================================================================================================

        public static string get(CPBaseClass cp, PageTemplateModel template) {
            try {
                return ""
                    + "\r\n\t" + "<Template"
                        + " name=\"" + System.Net.WebUtility.HtmlEncode(template.name) + "\""
                        + " guid=\"" + template.ccguid + "\""
                        + " issecure=\"" + GenericController.getYesNo( template.isSecure )+ "\""
                        + " >"
                        + ExportController.tabIndent(cp, ExportController.EncodeCData(template.bodyHTML ))
                    + "\r\n\t" + "</Template>";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetAddonNode");
                return string.Empty;
            }
        }
        // 
    }
}
