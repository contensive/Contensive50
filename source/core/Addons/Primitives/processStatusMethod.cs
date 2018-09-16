
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
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.Primitives {
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
                CoreController core = ((CPClass)cp).core;
                //
                // test default data connection
                //
                bool InsertTestOK = false;
                int TrapID = 0;
                int CS = core.db.csInsertRecord("Trap Log");
                if (!core.db.csOk(CS)) {
                    //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
                } else {
                    InsertTestOK = true;
                    TrapID = core.db.csGetInteger(CS, "ID");
                }
                core.db.csClose(ref CS);
                if (InsertTestOK) {
                    if (TrapID == 0) {
                        //throw new ApplicationException("Unexpected exception"); // todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
                    } else {
                        core.db.deleteContentRecord("Trap Log", TrapID);
                    }
                }
                //
                // Close page
                //
                core.webServer.clearResponseBuffer();
                if (core.doc.errorCount == 0) {
                    result = "Contensive OK";
                } else {
                    result = "Contensive Error Count = " + core.doc.errorCount;
                }
                result = core.html.getHtmlBodyEnd(false, false);
                core.doc.continueProcessing = false;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
