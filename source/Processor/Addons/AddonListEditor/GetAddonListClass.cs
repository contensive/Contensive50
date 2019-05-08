

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.AddonListEditor {
    public class GetAddonListClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            try {
                CoreController core = ((CPClass)CP).core;
                // 
                GetAddonList_RequestClass request = Newtonsoft.Json.JsonConvert.DeserializeObject<GetAddonList_RequestClass>(CP.Request.Body);
                if (request == null) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request is invalid" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentContentGuid)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent content guid is not valid [" + request.parentContentGuid + "]" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent record guid is not valid [" + request.parentRecordGuid + "]" }
                    });
                }
                var metadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, request.parentContentGuid);
                if (metadata == null) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The parent content could not be determined from the guid [" + request.parentContentGuid + "]" }
                    });
                }
                if (!core.session.isEditing(metadata.name)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
                    });
                }
                List<AddonListItemModel> addonList;
                switch (metadata.name.ToLower()) {
                    case "page content":
                        var page = Contensive.Processor.Models.Db.PageContentModel.create(core, request.parentRecordGuid);
                        if (page == null) {
                            return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Content] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        try {
                            if (string.IsNullOrWhiteSpace(page.addonList)) {
                                addonList = new List<AddonListItemModel>();
                            } else {
                                addonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AddonListItemModel>>(page.addonList);
                            }
                        } catch (Exception) {
                            return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The data stored in the parent record is not valid" }
                            });
                        }
                        break;
                    case "page templates":
                        var template = Contensive.Processor.Models.Db.PageTemplateModel.create(core, request.parentRecordGuid);
                        if (template == null) {
                            return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Templates] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        try {
                            if (string.IsNullOrWhiteSpace(template.addonList)) {
                                addonList = new List<AddonListItemModel>();
                            } else {
                                addonList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AddonListItemModel>>(template.addonList);
                            }
                        } catch (Exception) {
                            return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The data stored in the parent record is not valid" }
                            });
                        }
                        break;
                    default:
                        return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonList_ResponseClass() {
                            errorList = new List<string> { "The parent content is not valid addonList container [" + metadata.name + "]" }
                        });
                }
                return new GetAddonList_ResponseClass() {
                    errorList = new List<string>(),
                    addonList = addonList,
                    parentContentGuid = request.parentContentGuid,
                    parentRecordGuid = request.parentRecordGuid
                };
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        // 
        public class GetAddonList_RequestClass {
            /// <summary>
            /// The guid of the content where this list is stored (content + record define location)
            /// </summary>
            public string parentContentGuid;
            /// <summary>
            /// The guid of the record where this list is stored (content + record define location)
            /// </summary>
            public string parentRecordGuid;
        }
        // 
        public class GetAddonList_ResponseClass {
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
