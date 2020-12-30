////
//using System;
//using System.Collections.Generic;
//using Contensive.Processor.Controllers;
//using static Contensive.Processor.Constants;
//using Contensive.BaseClasses;
//using Contensive.Models.Db;
////
//namespace Contensive.Processor.Addons.Primitives {
//    public class ProcessLoginMethodClass : Contensive.BaseClasses.AddonBaseClass {
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Process Login
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <returns></returns>
//        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
//            string result = "";
//            try {
//                CoreController core = ((CPClass)cp).core;
//                //
//                // -- login
//                core.doc.continueProcessing = false;
//                return core.addon.execute(DbBaseModel.create<AddonModel>(core.cpParent, addonGuidLoginPage), new CPUtilsBaseClass.addonExecuteContext {
//                    addonType = CPUtilsBaseClass.addonContext.ContextPage,
//                    argumentKeyValuePairs = new Dictionary<string, string> {
//                        { "Force Default Login", "false" }
//                    },
//                    forceHtmlDocument = true,
//                    errorContextMessage = "Process Login"
//                });
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex);
//            }
//            return result;
//        }
//    }
//}
