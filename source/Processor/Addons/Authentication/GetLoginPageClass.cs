
using Contensive.Processor.Controllers;
using System;

namespace Contensive.Processor.Addons.Login {
    /// <summary>
    /// Returns a full login page (form plus complete html doc)
    /// </summary>
    public class GetLoginPageClass : BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Returns a full login page (form plus complete html doc)
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            try {
                return LoginController.getLoginPage(((CPClass)cp).core, false);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
