
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;

namespace Contensive.Addons.AdminSite {
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
                int ContentID = core.docProperties.getInteger("cid");
                if (ContentID == 0) {
                    result = "No filter is available";
                } else {
                    //cdefModel cdef = cdefModel.getCdef(core, ContentID);
                    var adminData = new AdminDataModel(core);
                    result = FormIndex.getForm_IndexFilterContent(core, adminData);
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
