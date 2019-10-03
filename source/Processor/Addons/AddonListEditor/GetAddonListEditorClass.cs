

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor;
using Contensive.Processor.Controllers;

using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.AddonListEditor {
    public class GetAddonListEditorClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            try {
                CoreController core = ((CPClass)CP).core;
                var content = ContentModel.create<ContentModel>(core.cpParent, CP.Doc.GetInteger("contentId"));
                if (content == null) { return "<!-- contentId not valid -->"; }
                string recordGuid = "";
                using (var cs = CP.CSNew() ) {
                    if ( cs.Open( content.name, "id=" + CP.Doc.GetInteger("recordId"))) {
                        recordGuid = cs.GetText("ccguid");
                    }
                }
                if (string.IsNullOrWhiteSpace(recordGuid)) { return "<!-- recordid not valid -->"; }
                CP.Doc.AddHeadJavascript("var parentContentGuid='" + content.ccguid + "';var parentRecordGuid='" + recordGuid+ "';");
                return string.Empty;
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
    }
}
