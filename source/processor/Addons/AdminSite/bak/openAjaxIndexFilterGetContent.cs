

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    public class openAjaxIndexFilterGetContentClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' getFieldEditorPreference remote method
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <returns></returns>
        public override object execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CPClass processor;
                cp;
                CPClass;
                coreClass cpCore = processor.core;
                // 
                cpCore.visitProperty.setProperty("IndexFilterOpen", "1");
                Contensive.Addons.AdminSite.getAdminSiteClass adminSite = new Contensive.Addons.AdminSite.getAdminSiteClass(cpCore.cp_forAddonExecutionOnly);
                int ContentID = cpCore.docProperties.getInteger("cid");
                if ((ContentID == 0)) {
                    result = "No filter is available";
                }
                else {
                    Models.Complex.cdefModel cdef = Models.Complex.cdefModel.getCdef(cpCore, ContentID);
                    result = adminSite.GetForm_IndexFilterContent(cdef);
                }
                
                adminSite = null;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return result;
        }
    }
}