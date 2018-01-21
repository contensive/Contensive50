
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
    public class processStatusMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                // test default data connection
                //
                bool InsertTestOK = false;
                int TrapID = 0;
                int CS = cpCore.db.csInsertRecord("Trap Log");
                if (!cpCore.db.csOk(CS)) {
                    //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
                } else {
                    InsertTestOK = true;
                    TrapID = cpCore.db.csGetInteger(CS, "ID");
                }
                cpCore.db.csClose(ref CS);
                if (InsertTestOK) {
                    if (TrapID == 0) {
                        //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
                    } else {
                        cpCore.db.deleteContentRecord("Trap Log", TrapID);
                    }
                }
                //
                // Close page
                //
                cpCore.webServer.clearResponseBuffer();
                if (cpCore.doc.errorCount == 0) {
                    result = "Contensive OK";
                } else {
                    result = "Contensive Error Count = " + cpCore.doc.errorCount;
                }
                result = cpCore.html.getHtmlBodyEnd(false, false);
                cpCore.doc.continueProcessing = false;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
