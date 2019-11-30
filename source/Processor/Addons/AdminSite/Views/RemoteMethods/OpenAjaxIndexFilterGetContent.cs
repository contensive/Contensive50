
using System;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Addons.AdminSite {
    public class OpenAjaxIndexFilterGetContentClass : Contensive.BaseClasses.AddonBaseClass {
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
                core.visitProperty.setProperty("IndexFilterOpen", "1");
                int ContentId = core.docProperties.getInteger("cid");
                if (ContentId == 0) {
                    result = "No filter is available";
                } else {
                    result = FormIndex.getForm_IndexFilterContent(core, new AdminDataModel(core));
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
