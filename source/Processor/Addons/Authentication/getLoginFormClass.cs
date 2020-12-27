
using Contensive.Processor.Controllers;
using System;
//
namespace Contensive.Processor.Addons.Login {
    //
    //====================================================================================================
    /// <summary>
    /// Execute the current login form. This is the default form, or another addon if configured.
    /// </summary>
    public class GetLoginFormClass : BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Execute the current login form. This is the default form, or another addon if configured.
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(BaseClasses.CPBaseClass cp) {
            try {
                bool forceDefaultLogin = cp.Doc.GetBoolean("Force Default Login");
                return LoginController.getLoginForm(((CPClass)cp).core, forceDefaultLogin);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
