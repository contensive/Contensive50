using Contensive.Processor;
using Contensive.Processor.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contensive.Addons.AdminSite {
    class ToolClearCache {
        //
        //
        //========================================================================
        //
        //========================================================================
        //
        public static string GetForm_ClearCache(CoreController core) {
            string returnHtml = "";
            try {
                StringBuilderLegacyController Content = new StringBuilderLegacyController();
                string Button = null;
                ////adminUIController Adminui = new adminUIController(core);
                string Description = null;
                string ButtonList = null;
                //
                Button = core.docProperties.getText(Processor.constants.RequestNameButton);
                if (Button == Processor.constants.ButtonCancel) {
                    //
                    // Cancel just exits with no content
                    //
                    return "";
                } else if (!core.session.isAuthenticatedAdmin(core)) {
                    //
                    // Not Admin Error
                    //
                    ButtonList = Processor.constants.ButtonCancel;
                    Content.Add(AdminUIController.getFormBodyAdminOnly());
                } else {
                    Content.Add(AdminUIController.editTableOpen);
                    //
                    // Set defaults
                    //
                    //
                    // Process Requests
                    //
                    switch (Button) {
                        case Processor.constants.ButtonApply:
                        case Processor.constants.ButtonOK:
                            //
                            // Clear the cache
                            //
                            core.cache.invalidateAll();
                            break;
                    }
                    if (Button == Processor.constants.ButtonOK) {
                        //
                        // Exit on OK or cancel
                        //
                        return "";
                    }
                    //
                    // Buttons
                    //
                    ButtonList = Processor.constants.ButtonCancel + "," + Processor.constants.ButtonApply + "," + Processor.constants.ButtonOK;
                    //
                    // Close Tables
                    //
                    Content.Add(AdminUIController.editTableClose);
                    Content.Add(HtmlController.inputHidden(Processor.constants.rnAdminSourceForm, Processor.constants.AdminFormClearCache));
                }
                //
                Description = "Hit Apply or OK to clear all current content caches";
                returnHtml = AdminUIController.getBody(core, "Clear Cache", ButtonList, "", true, true, Description, "", 0, Content.Text);
                Content = null;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
    }
}
