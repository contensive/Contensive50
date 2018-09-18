

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    // 
    public class setAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass {
        
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
                string ArgList = cpCore.docProperties.getText("args");
                string[] Args = ArgList.Split("&");
                GoogleDataType gd = new GoogleDataType();
                gd.IsEmpty = true;
                for (Ptr = 0; (Ptr <= UBound(Args)); Ptr++) {
                    string[] ArgNameValue = Args[Ptr].Split("=");
                    string PropertyName = ArgNameValue[0];
                    string PropertyValue = "";
                    if ((UBound(ArgNameValue) > 0)) {
                        PropertyValue = ArgNameValue[1];
                    }
                    
                    cpCore.visitProperty.setProperty(PropertyName, PropertyValue);
                }
                
                result = remoteQueryController.main_FormatRemoteQueryOutput(cpCore, gd, RemoteFormatEnum.RemoteFormatJsonNameValue);
                result = cpCore.html.main_encodeHTML(result);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return result;
        }
    }
}