
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
using Contensive.Core.Models.Complex;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.AdminSite {
    public class openAjaxIndexFilterGetContentClass : Contensive.BaseClasses.AddonBaseClass {
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
                core.visitProperty.setProperty("IndexFilterOpen", "1");
                Contensive.Core.Addons.AdminSite.getHtmlBodyClass adminSite = new Contensive.Core.Addons.AdminSite.getHtmlBodyClass(core.cp_forAddonExecutionOnly);
                int ContentID = core.docProperties.getInteger("cid");
                if (ContentID == 0) {
                    result = "No filter is available";
                } else {
                    cdefModel cdef = cdefModel.getCdef(core, ContentID);
                    result = adminSite.GetForm_IndexFilterContent(cdef);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
