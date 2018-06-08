
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
    //
    public class setAdminSiteFieldHelpClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// setAdminSiteFieldHelp remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                coreController core = ((CPClass)cp).core;
                if (cp.User.IsAdmin) {
                    int fieldId = cp.Doc.GetInteger("fieldId");
                    ContentFieldHelpModel help = ContentFieldHelpModel.createByFieldId(core, fieldId);
                    if (help == null) {
                        help = ContentFieldHelpModel.add(core);
                        help.FieldID = fieldId;
                    }
                    help.HelpCustom = cp.Doc.GetText("helpcustom");
                    help.save(core);
                    contentFieldModel contentField = contentFieldModel.create(core, fieldId);
                    if (contentField != null) {
                        cdefModel.invalidateCache(core, contentField.ContentID);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
