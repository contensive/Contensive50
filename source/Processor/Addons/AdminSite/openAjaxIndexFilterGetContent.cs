﻿
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.AdminSite {
    public class OpenAjaxIndexFilterGetContentClass : Contensive.BaseClasses.AddonBaseClass {
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
                int ContentID = core.docProperties.getInteger("cid");
                if (ContentID == 0) {
                    result = "No filter is available";
                } else {
                    //cdefModel cdef = cdefModel.getCdef(core, ContentID);
                    var adminContext = new AdminInfoDomainModel(core);
                    result = BodyIndexClass.getForm_IndexFilterContent(core, adminContext);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
