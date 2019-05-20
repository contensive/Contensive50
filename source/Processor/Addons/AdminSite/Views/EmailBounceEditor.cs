
using System;
using Contensive.Addons.AdminSite.Controllers;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.AdminSite {
    public class EmailBounceEditor {
        //
        //========================================================================
        //
        public static string get(CoreController core, AdminDataModel adminData) {
            string result = "";
            try {
                StringBuilderLegacyController f = new StringBuilderLegacyController();
                string Copy = null;
                //
                f.Add(AdminUIController.getEditRowLegacy(core, "<a href=?" + rnAdminForm + "=28 target=_blank>Open in New Window</a>", "Email Control", "The settings in this section can be modified with the Email Control page."));
                f.Add(AdminUIController.getEditRowLegacy(core, core.siteProperties.getText("EmailBounceAddress", ""), "Bounce Email Address", "All bounced emails will be sent to this address automatically. This must be a valid email account, and you should either use Contensive Bounce processing to capture the emails, or manually remove them from the account yourself."));
                f.Add(AdminUIController.getEditRowLegacy(core, GenericController.getYesNo(GenericController.encodeBoolean(core.siteProperties.getBoolean("AllowEmailBounceProcessing", false))), "Allow Bounce Email Processing", "If checked, Contensive will periodically retrieve all the email from the POP email account and take action on the membefr account that sent the email."));
                switch (core.siteProperties.getText("EMAILBOUNCEPROCESSACTION", "0")) {
                    case "1":
                        Copy = "Clear the Allow Group Email field for all members with a matching Email address";
                        break;
                    case "2":
                        Copy = "Clear all member Email addresses that match the Email address";
                        break;
                    case "3":
                        Copy = "Delete all Members with a matching Email address";
                        break;
                    default:
                        Copy = "Do Nothing";
                        break;
                }
                f.Add(AdminUIController.getEditRowLegacy(core, Copy, "Bounce Email Action", "When an email is determined to be a bounce, this action will taken against member with that email address."));
                f.Add(AdminUIController.getEditRowLegacy(core, core.siteProperties.getText("POPServerStatus"), "Last Email Retrieve Status", "This is the status of the last POP email retrieval attempted."));
                //
                result = AdminUIController.getEditPanel(core, (!adminData.allowAdminTabs), "Bounced Email Handling", "", AdminUIController.editTable( f.Text ));
                adminData.EditSectionPanelCount = adminData.EditSectionPanelCount + 1;
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }



    }
}
