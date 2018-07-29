
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
using Contensive.Processor.Models.Complex;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.AdminSite {
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
                CoreController core = ((CPClass)cp).core;

                //
                core.visitProperty.setProperty("IndexFilterOpen", "1");
                getHtmlBodyClass adminSite = new getHtmlBodyClass(core.cp_forAddonExecutionOnly);
                int ContentID = core.docProperties.getInteger("cid");
                if (ContentID == 0) {
                    result = "No filter is available";
                } else {
                    //cdefModel cdef = cdefModel.getCdef(core, ContentID);
                    var adminContext = new adminContextClass(core);
                    result = adminSite.GetForm_IndexFilterContent(adminContext);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
