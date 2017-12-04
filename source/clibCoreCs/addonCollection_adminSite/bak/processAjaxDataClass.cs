﻿

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Controllers;

// 

namespace Contensive.Addons.AdminSite {
    
    // 
    public class processAjaxDataClass : Contensive.BaseClasses.AddonBaseClass {
        
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
                result = cpCore.executeRoute_ProcessAjaxData();
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            
            return result;
        }
    }
}