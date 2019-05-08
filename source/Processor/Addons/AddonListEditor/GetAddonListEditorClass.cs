

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
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
                // 
                return new GetAddonListEditor_ResponseClass() {
                    errorList = new List<string> {
                        "Not Implemented Yet"
                    }
                };
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        // 
        public class GetAddonListEditor_ResponseClass {
            /// <summary>
            /// The guid of the content where this list is stored (content + record define location)
            /// </summary>
            public List<string> errorList;
            /// <summary>
            /// The guid of the content where this list is stored (content + record define location)
            /// </summary>
            public string parentContentGuid;
            /// <summary>
            /// The guid of the record where this list is stored (content + record define location)
            /// </summary>
            public string parentRecordGuid;
            /// <summary>
            /// A list of positions. Positions are the slots where a design block goes
            /// </summary>
            public List<AddonListItemModel> addonList;
        }
    }
}
