

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    // 
    public class setAdminSiteFieldHelpClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' setAdminSiteFieldHelp remote method
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
                if (cp.User.IsAdmin) {
                    int fieldId = cp.Doc.GetInteger("fieldId");
                    Models.Entity.ContentFieldHelpModel help = Models.Entity.ContentFieldHelpModel.createByFieldId(cpCore, fieldId);
                    if ((help == null)) {
                        help = ContentFieldHelpModel.add(cpCore);
                        help.FieldID = fieldId;
                    }
                    
                    help.HelpCustom = cp.Doc.GetText("helpcustom");
                    help.save(cpCore);
                    Models.Entity.contentFieldModel contentField = Models.Entity.contentFieldModel.create(cpCore, fieldId);
                    if (contentField) {
                        IsNot;
                        null;
                        Models.Complex.cdefModel.invalidateCache(cpCore, contentField.ContentID);
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return result;
        }
    }
}