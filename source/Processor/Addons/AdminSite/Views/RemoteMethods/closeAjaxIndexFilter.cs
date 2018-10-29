
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;

namespace Contensive.Addons.AdminSite {
    //
    public class CloseAjaxIndexFilterClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// CloseAjaxIndexFilterClass remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                //
                core.visitProperty.setProperty("IndexFilterOpen", "0");
                //
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
