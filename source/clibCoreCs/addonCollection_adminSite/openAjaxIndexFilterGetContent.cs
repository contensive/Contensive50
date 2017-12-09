
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
using Contensive.Core.Models.Complex;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
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
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;

                //
                cpCore.visitProperty.setProperty("IndexFilterOpen", "1");
                Contensive.Addons.AdminSite.getAdminSiteClass adminSite = new Contensive.Addons.AdminSite.getAdminSiteClass(cpCore.cp_forAddonExecutionOnly);
                int ContentID = cpCore.docProperties.getInteger("cid");
                if (ContentID == 0) {
                    result = "No filter is available";
                } else {
                    cdefModel cdef = cdefModel.getCdef(cpCore, ContentID);
                    result = adminSite.GetForm_IndexFilterContent(cdef);
                }
                adminSite = null;


            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
