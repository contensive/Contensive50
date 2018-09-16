
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
    public class processAddonStyleEditorClass : Contensive.BaseClasses.AddonBaseClass {
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
                // save custom styles
                if (core.session.isAuthenticated & core.session.isAuthenticatedAdmin(core)) {
                    int addonId = core.docProperties.getInteger("AddonID");
                    if (addonId > 0) {
                       AddonModel styleAddon =AddonModel.create(core, addonId);
                        if (styleAddon.stylesFilename.content != core.docProperties.getText("CustomStyles")) {
                            styleAddon.stylesFilename.content = core.docProperties.getText("CustomStyles");
                            styleAddon.save(core);
                            //
                            // Clear Caches
                            //
                            core.cache.invalidateAllInContent(AddonModel.contentName);
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
