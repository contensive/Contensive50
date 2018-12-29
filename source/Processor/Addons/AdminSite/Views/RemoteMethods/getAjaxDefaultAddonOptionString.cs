
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Constants;
//
namespace Contensive.Addons.AdminSite {
    //
    public class GetAjaxDefaultAddonOptionStringClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// GetAjaxDefaultAddonOptionStringClass remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                using (CoreController core = ((CPClass)cp).core) {
                    using (var csXfer = new CsModel(core)) {
                        string AddonGuid = core.docProperties.getText("guid");
                        csXfer.csOpen(Processor.Models.Db.AddonModel.contentName, "ccguid=" + DbController.encodeSQLText(AddonGuid));
                        if (csXfer.csOk()) {
                            string addonArgumentList = csXfer.csGetText("argumentlist");
                            bool addonIsInline = csXfer.csGetBoolean("IsInline");
                            returnHtml = AddonController.getDefaultAddonOptions(core, addonArgumentList, AddonGuid, addonIsInline);
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return returnHtml;
        }
    }
}
