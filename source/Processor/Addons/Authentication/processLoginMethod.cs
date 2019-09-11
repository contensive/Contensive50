
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

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
using Contensive.BaseClasses;
using Contensive.Models.Db;
//
namespace Contensive.Addons.Primitives {
    public class ProcessLoginMethodClass : Contensive.BaseClasses.AddonBaseClass {
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
                // -- login
                core.doc.continueProcessing = false;
                Dictionary<string, string> addonArguments = new Dictionary<string, string>();
                addonArguments.Add("Force Default Login", "false");
                return core.addon.execute(DbBaseModel.create<AddonModel>(core.cpParent, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext() {
                    addonType = CPUtilsBaseClass.addonContext.ContextPage,
                    argumentKeyValuePairs = addonArguments,
                    forceHtmlDocument = true,
                     errorContextMessage = "get field editor preference remote"
                });
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
