

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    // 
    public class getAjaxVisitPropertyClass : Contensive.BaseClasses.AddonBaseClass {
        
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
                gd.IsEmpty = false;
                object gd.row;
                for (Ptr = 0; (Ptr <= UBound(Args)); Ptr++) {
                    object Preserve;
                    gd.col(Ptr);
                    object Preserve;
                    gd.row[0].Cell(Ptr);
                    string[] ArgNameValue = Args[Ptr].Split("=");
                    string PropertyName = ArgNameValue[0];
                    gd.col(Ptr).Id = PropertyName;
                    gd.col(Ptr).Label = PropertyName;
                    gd.col(Ptr).Type = "string";
                    string PropertyValue = "";
                    if ((UBound(ArgNameValue) > 0)) {
                        PropertyValue = ArgNameValue[1];
                    }
                    
                    gd.row[0].Cell(Ptr).v = cpCore.visitProperty.getText(PropertyName, PropertyValue);
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
