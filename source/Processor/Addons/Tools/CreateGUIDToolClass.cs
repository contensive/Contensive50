
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
            return getTool(((CPClass)cpBase).core);
        }
        //
        //=============================================================================
        // GUID Tools
        //=============================================================================
        //
        private string getTool(CoreController core) {
            string result = "";
            try {
                string Button = null;
                StringBuilderLegacyController s;
                s = new StringBuilderLegacyController();
                s.Add(AdminUIController.getHeaderTitleDescription("Create GUID", "Use this tool to create a GUID. This is useful when creating new Addons."));
                //
                // Process the form
                Button = core.docProperties.getText("button");
                //
                s.Add(HtmlController.inputText_Legacy(core, "GUID", GenericController.getGUID(), 1, 80));
                //
                // Display form
                result = AdminUIController.getToolForm(core, s.Text, ButtonCancel + "," + ButtonCreateGUId);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
            return result;
        }
    }
}

