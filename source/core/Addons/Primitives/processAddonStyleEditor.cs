
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
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.Primitives {
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
                CPClass processor = (CPClass)cp;
                coreClass cpCore = processor.core;
                //
                // save custom styles
                if (cpCore.doc.sessionContext.isAuthenticated & cpCore.doc.sessionContext.isAuthenticatedAdmin(cpCore)) {
                    int addonId = cpCore.docProperties.getInteger("AddonID");
                    if (addonId > 0) {
                       addonModel styleAddon =addonModel.create(cpCore, addonId);
                        if (styleAddon.StylesFilename.content != cpCore.docProperties.getText("CustomStyles")) {
                            styleAddon.StylesFilename.content = cpCore.docProperties.getText("CustomStyles");
                            styleAddon.save(cpCore);
                            //
                            // Clear Caches
                            //
                            cpCore.cache.invalidateAllInContent(addonModel.contentName);
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
