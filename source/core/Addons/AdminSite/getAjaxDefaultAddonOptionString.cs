
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
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Addons.AdminSite {
    //
    public class getAjaxDefaultAddonOptionStringClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                coreController core = ((CPClass)cp).core;
                //
                // return the addons defult AddonOption_String
                // used in wysiwyg editor - addons in select list have no defaultOption_String
                // because created it is expensive (lookuplists, etc). This is only called
                // when the addon is double-clicked in the editor after being dropped
                //
                string AddonGuid = core.docProperties.getText("guid");
                //$$$$$ cache this
                int CS = core.db.csOpen(cnAddons, "ccguid=" + core.db.encodeSQLText(AddonGuid));
                string addonArgumentList = "";
                bool addonIsInline = false;
                if (core.db.csOk(CS)) {
                    addonArgumentList = core.db.csGetText(CS, "argumentlist");
                    addonIsInline = core.db.csGetBoolean(CS, "IsInline");
                    returnHtml = addonController.getDefaultAddonOptions(core, addonArgumentList, AddonGuid, addonIsInline);
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
    }
}
