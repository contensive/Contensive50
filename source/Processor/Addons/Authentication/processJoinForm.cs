////
//using System;
//using Contensive.Processor.Controllers;
////
//namespace Contensive.Processor.Addons.Primitives {
//    public class processJoinFormClass : Contensive.BaseClasses.AddonBaseClass {
//        //
//        //====================================================================================================
//        /// <summary>
//        /// getFieldEditorPreference remote method
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <returns></returns>
//        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
//            string result = "";
//            try {
//                CoreController core = ((CPClass)cp).core;
//                //
//                RegisterController.processRegisterForm(core);
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//    }
//}
