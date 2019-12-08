
using System;
using Contensive.Processor.Addons.AdminSite.Controllers;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.AdminSite {
    class ToolClearCache {
        //
        //====================================================================================================
        //
        public static string get(CoreController core) {
            string returnHtml = "";
            try {
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Button = null;
                string Description = null;
                string ButtonList = null;
                //
                Button = core.docProperties.getText(Processor.Constants.RequestNameButton);
                if (Button == Processor.Constants.ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return "";
                } else if (!core.session.isAuthenticatedAdmin()) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = Processor.Constants.ButtonCancel;
                    Content.add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case Processor.Constants.ButtonApply:
                        case Processor.Constants.ButtonOK:
                            //
                            // Clear the cache
                            //
                            core.cache.invalidateAll();
                            break;
                    }
                    if (Button == Processor.Constants.ButtonOK) {
                        //
                        // Exit on OK or cancel
                        //
                        return "";
                    }
                    //
                    // Buttons
                    //
                    ButtonList = Processor.Constants.ButtonCancel + "," + Processor.Constants.ButtonApply + "," + Processor.Constants.ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.add(AdminUIController.editTable("<p>Click OK or Apply to invalidate all local and remote cache data.</p>"));
                    Content.add(HtmlController.inputHidden(Processor.Constants.rnAdminSourceForm, Processor.Constants.AdminFormClearCache));
                }
                //
                Description = "Hit Apply or OK to clear all current content caches";
                returnHtml = AdminUIController.getToolBody(core, "Clear Cache", ButtonList, "", true, true, Description, "", 0, Content.text);
                Content = null;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}
