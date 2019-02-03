
using System;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
using Contensive.Processor.Controllers;

namespace Contensive.Addons.AdminSite {
    //
    public class SetAdminSiteFieldHelpClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// setAdminSiteFieldHelp remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;
                if (cp.User.IsAdmin) {
                    int fieldId = cp.Doc.GetInteger("fieldId");
                    ContentFieldHelpModel help = ContentFieldHelpModel.createByFieldId(core, fieldId);
                    if (help == null) {
                        help = ContentFieldHelpModel.addDefault(core, Processor.Models.Domain.ContentMetadataModel.createByUniqueName(core, ContentFieldHelpModel.contentName));
                        help.fieldID = fieldId;
                    }
                    help.helpCustom = cp.Doc.GetText("helpcustom");
                    help.save(core);
                    ContentFieldModel contentField = ContentFieldModel.create(core, fieldId);
                    if (contentField != null) {
                        ContentMetadataModel.invalidateCache(core, contentField.contentID);
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
