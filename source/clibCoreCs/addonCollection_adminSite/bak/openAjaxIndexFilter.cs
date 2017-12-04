

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    public class openAjaxIndexFilterClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' getFieldEditorPreference remote method
        // '' </summary>
        // '' <param name="cp"></param>
        // '' <returns></returns>
        public override object execute(Contensive.BaseClasses.CPBaseClass cp) {
            string returnHtml = "";
            try {
                CPClass processor;
                cp;
                CPClass;
                coreClass cpCore = processor.core;
                // 
                cpCore.visitProperty.setProperty("IndexFilterOpen", "1");
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return returnHtml;
        }
    }
}