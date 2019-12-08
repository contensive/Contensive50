
using System;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Contensive.Processor.Constants;
using Contensive.Processor.Addons.AdminSite.Controllers;
using System.Text;
using System.Collections.Generic;
//
namespace Contensive.Processor.Addons.Tools {
    //
    public class CreateGUIDToolClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon method, deliver complete Html admin site
        /// blank return on OK or cancel button
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return get(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        // GUID Tools
        //=============================================================================
        //
        public static string get(CoreController core) {
            string result = "";
            try {
                string Button = null;
                StringBuilderLegacyController s;
                s = new StringBuilderLegacyController();
                s.add(AdminUIController.getHeaderTitleDescription("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."));
                //
                // Process the form
                Button = core.docProperties.getText("button");
                //
                s.add(HtmlController.inputText_Legacy(core, "GUID", GenericController.getGUID(), 1, 80));
                //
                // Display form
                result = AdminUIController.getToolForm(core, s.text, ButtonCancel + "," + ButtonCreateGUId);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
    }
}

