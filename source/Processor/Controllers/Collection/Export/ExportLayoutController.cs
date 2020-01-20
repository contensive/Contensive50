
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Models.Db;

namespace Contensive.Processor.Controllers {
    public static class ExportLayoutController {
        // 
        // ====================================================================================================

        public static string get(CPBaseClass cp, LayoutModel layout) {
            try {
                return ""
                    + "\r\n\t" + "<Layout"
                        + " name=\"" + System.Net.WebUtility.HtmlEncode(layout.name) + "\""
                        + " guid=\"" + layout.ccguid + "\""
                        + " >"
                        + ExportController.tabIndent(cp, ExportController.EncodeCData(cp, layout.layout.content ))
                    + "\r\n\t" + "</Layout>";
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex, "GetAddonNode");
                return string.Empty;
            }
        }
        // 
    }
}
