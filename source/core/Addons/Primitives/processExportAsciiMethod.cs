
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
    public class processExportAsciiMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                coreController cpCore = processor.core;
                //
                // -- Should be a remote method in commerce
                if (!cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) {
                    //
                    // Administrator required
                    //
                    cpCore.doc.userErrorList.Add("Error: You must be an administrator to use the ExportAscii method");
                } else {
                    string ContentName = cpCore.docProperties.getText("content");
                    int PageSize = cpCore.docProperties.getInteger("PageSize");
                    if (PageSize == 0) {
                        PageSize = 20;
                    }
                    int PageNumber = cpCore.docProperties.getInteger("PageNumber");
                    if (PageNumber == 0) {
                        PageNumber = 1;
                    }
                    if (string.IsNullOrEmpty(ContentName)) {
                        cpCore.doc.userErrorList.Add("Error: ExportAscii method requires ContentName");
                    } else {
                        result = exportAsciiController.exportAscii_GetAsciiExport(cpCore, ContentName, PageSize, PageNumber);
                        cpCore.doc.continueProcessing = false;
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
