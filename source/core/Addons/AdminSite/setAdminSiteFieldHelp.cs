
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
namespace Contensive.Core.Addons.AdminSite {
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
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;
                if (cp.User.IsAdmin) {
                    int fieldId = cp.Doc.GetInteger("fieldId");
                    ContentFieldHelpModel help = ContentFieldHelpModel.createByFieldId(cpCore, fieldId);
                    if (help == null) {
                        help = ContentFieldHelpModel.add(cpCore);
                        help.FieldID = fieldId;
                    }
                    help.HelpCustom = cp.Doc.GetText("helpcustom");
                    help.save(cpCore);
                    contentFieldModel contentField = contentFieldModel.create(cpCore, fieldId);
                    if (contentField != null) {
                        cdefModel.invalidateCache(cpCore, contentField.ContentID);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
