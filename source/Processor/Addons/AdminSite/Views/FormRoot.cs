
using System;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;

namespace Contensive.Addons.AdminSite {
    public class FormRoot {
        //
        //========================================================================
        //   Print the root form
        //
        public static string GetForm_Root(CoreController core) {
            string returnHtml = "";
            try {
                int CS = 0;
                StringBuilderLegacyController Stream = new StringBuilderLegacyController();
                int addonId = 0;
                string AddonIDText = null;
                //
                // This is really messy -- there must be a better way
                //
                addonId = 0;
                if (core.session.visit.id == core.docProperties.getInteger(RequestNameDashboardReset)) {
                    //$$$$$ cache this
                    csXfer.csOpen(Processor.Models.Db.AddonModel.contentName, "ccguid=" + DbController.encodeSQLText(addonGuidDashboard));
                    if (csXfer.csOk()) {
                        addonId = csXfer.csGetInteger(CS, "id");
                        core.siteProperties.setProperty("AdminRootAddonID", GenericController.encodeText(addonId));
                    }
                    csXfer.csClose();
                }
                if (addonId == 0) {
                    //
                    // Get AdminRootAddon
                    //
                    AddonIDText = core.siteProperties.getText("AdminRootAddonID", "");
                    if (string.IsNullOrEmpty(AddonIDText)) {
                        //
                        // the desktop is likely unset, auto set it to dashboard
                        //
                        addonId = -1;
                    } else if (AddonIDText == "0") {
                        //
                        // the desktop has been set to none - go with default desktop
                        //
                        addonId = 0;
                    } else if (AddonIDText.IsNumeric()) {
                        //
                        // it has been set to a non-zero number
                        //
                        addonId = GenericController.encodeInteger(AddonIDText);
                        //
                        // Verify it so there is no error when it runs
                        if (AddonModel.create(core, addonId) == null) {
                            addonId = -1;
                            core.siteProperties.setProperty("AdminRootAddonID", "");
                        }
                    }
                    if (addonId == -1) {
                        //
                        // This has never been set, try to get the dashboard ID
                        var addon = AddonModel.create(core, addonGuidDashboard);
                        if (addon != null) {
                            addonId = addon.id;
                            core.siteProperties.setProperty("AdminRootAddonID", addonId);
                        }
                    }
                }
                if (addonId != 0) {
                    //
                    // Display the Addon
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + Processor.Controllers.ErrorController.getUserError(core) + "</div>";
                    }
                    returnHtml += core.addon.execute(AddonModel.create(core, addonId), new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                        addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                        errorContextMessage = "executing addon id:" + addonId + " set as Admin Root addon"
                    });
                    //returnHtml = returnHtml & core.addon.execute_legacy4(CStr(addonId), "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                }
                if (string.IsNullOrEmpty(returnHtml)) {
                    //
                    // Nothing Displayed, show default root page
                    //
                    returnHtml = returnHtml + "\r\n<div style=\"padding:20px;height:450px\">"
                    + "\r\n<div><a href=http://www.Contensive.com target=_blank><img style=\"border:1px solid #000;\" src=\"/ContensiveBase/images/ContensiveAdminLogo.GIF\" border=0 ></A></div>"
                    + "\r\n<div><strong>Contensive/" + core.codeVersion() + "</strong></div>"
                    + "\r\n<div style=\"clear:both;height:18px;margin-top:10px\"><div style=\"float:left;width:200px;\">Domain Name</div><div style=\"float:left;\">" + core.webServer.requestDomain + "</div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Login Member Name</div><div style=\"float:left;\">" + core.session.user.name + "</div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\">Quick Reports</div><div style=\"float:left;\"><a Href=\"?" + rnAdminForm + "=" + AdminFormQuickStats + "\">Real-Time Activity</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?" + RequestNameDashboardReset + "=" + core.session.visit.id + "\">Run Dashboard</A></div></div>"
                    + "\r\n<div style=\"clear:both;height:18px;\"><div style=\"float:left;width:200px;\"><a Href=\"?addonguid=" + addonGuidAddonManager + "\">Add-on Manager</A></div></div>";
                    //
                    if (core.doc.debug_iUserError != "") {
                        returnHtml = returnHtml + "<div style=\"clear:both;margin-top:20px;\">&nbsp;</div>"
                        + "<div style=\"clear:both;margin-top:20px;\">" + Processor.Controllers.ErrorController.getUserError(core) + "</div>";
                    }
                    //
                    returnHtml = returnHtml + "\r\n</div>"
                    + "";
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnHtml;
        }
        //
    }
}
