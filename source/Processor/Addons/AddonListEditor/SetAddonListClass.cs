

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
//
namespace Contensive.Addons.AddonListEditor {
    public class SetAddonListClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            try {
                CoreController core = ((CPClass)CP).core;
                // 
                SetAddonList_RequestClass request = DeserializeObject<SetAddonList_RequestClass>(CP.Request.Form);
                if (request == null) {
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request is invalid" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentContentGuid)) {
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent content guid is not valid [" + request.parentContentGuid + "]" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent record guid is not valid [" + request.parentRecordGuid + "]" }
                    });
                }
                var metadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, request.parentContentGuid);
                if (metadata == null) {
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "The parent content could not be determined from the guid [" + request.parentContentGuid + "]" }
                    });
                }
                //
                // -- validate addonList from UI and set back into a string
                switch (metadata.name.ToLower()) {
                    case "page content":
                        var page = Contensive.Processor.Models.Db.PageContentModel.create(core, request.parentRecordGuid);
                        if (page == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Content] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        page.addonList = SerializeObject(request.addonList);
                        page.save(core);
                        break;
                    case "page templates":
                        var template = Contensive.Processor.Models.Db.PageTemplateModel.create(core, request.parentRecordGuid);
                        if (template == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Templates] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        template.addonList = SerializeObject(request.addonList);
                        template.save(core);
                        break;
                    default:
                        return SerializeObject(new SetAddonList_ResponseClass() {
                            errorList = new List<string> { "The parent content is not valid addonList container [" + metadata.name + "]" }
                        });
                }
                return new SetAddonList_ResponseClass() {
                    errorList = new List<string>(),
                };
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        // 
        public class SetAddonList_RequestClass {
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
        // 
        public class SetAddonList_ResponseClass {
            /// <summary>
            /// The guid of the content where this list is stored (content + record define location)
            /// </summary>
            public List<string> errorList;
        }
    }
}
