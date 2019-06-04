

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
//
namespace Contensive.Addons.AddonListEditor {
    public class GetAddonListClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass cp) {
            try {
                CoreController core = ((CPClass)cp).core;
                // 
                GetAddonList_RequestClass request = DeserializeObject<GetAddonList_RequestClass>(cp.Request.Body);
                if (request == null) {
                    //
                    // -- allow simple querystring values so SetSampleAddonList can reuse this
                    request = new GetAddonList_RequestClass() {
                        parentRecordGuid = cp.Doc.GetText("parentRecordGuid"),
                        parentContentGuid = cp.Doc.GetText("parentContentGuid"),
                        includeRenderedHtml = cp.Doc.GetBoolean("includeRenderedHtml")
                    };
                }
                if (String.IsNullOrWhiteSpace(request.parentContentGuid)) {
                    return SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent content guid is not valid [" + request.parentContentGuid + "]" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                    return SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The request parent record guid is not valid [" + request.parentRecordGuid + "]" }
                    });
                }
                var metadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, request.parentContentGuid);
                if (metadata == null) {
                    return SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "The parent content could not be determined from the guid [" + request.parentContentGuid + "]" }
                    });
                }
                if (!core.session.isAuthenticatedContentManager(core, metadata)) {
                    cp.Response.SetStatus(WebServerController.httpResponseStatus401_Unauthorized);
                    return SerializeObject(new GetAddonList_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
                    });
                }
                List<AddonListItemModel> addonList;
                switch (metadata.name.ToLower()) {
                    case "page content":
                        var page = Contensive.Processor.Models.Db.PageContentModel.create(core, request.parentRecordGuid);
                        if (page == null) {
                            return SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Content] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        try {
                            if (string.IsNullOrWhiteSpace(page.addonList)) {
                                addonList = new List<AddonListItemModel>();
                            } else {
                                addonList = DeserializeObject<List<AddonListItemModel>>(page.addonList);
                            }
                        } catch (Exception) {
                            return SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The data stored in the parent record is not valid" }
                            });
                        }
                        break;
                    case "page templates":
                        var template = Contensive.Processor.Models.Db.PageTemplateModel.create(core, request.parentRecordGuid);
                        if (template == null) {
                            return SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Templates] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        try {
                            if (string.IsNullOrWhiteSpace(template.addonList)) {
                                addonList = new List<AddonListItemModel>();
                            } else {
                                addonList = DeserializeObject<List<AddonListItemModel>>(template.addonList);
                            }
                        } catch (Exception) {
                            return SerializeObject(new GetAddonList_ResponseClass() {
                                errorList = new List<string> { "The data stored in the parent record is not valid" }
                            });
                        }
                        break;
                    default:
                        return SerializeObject(new GetAddonList_ResponseClass() {
                            errorList = new List<string> { "The parent content is not valid addonList container [" + metadata.name + "]" }
                        });
                }
                //
                // -- render the html for each addon in the addonList that qualifies
                if (request.includeRenderedHtml) {
                    foreach (var addon in addonList) {
                        cp.Doc.SetProperty("instanceId", addon.instanceGuid);
                        addon.renderedHtml = cp.Addon.Execute(addon.designBlockTypeGuid).ToString();
                    }
                }
                //
                // -- return the successful response
                return new GetAddonList_ResponseClass() {
                    errorList = new List<string>(),
                    addonList = addonList,
                    parentContentGuid = request.parentContentGuid,
                    parentRecordGuid = request.parentRecordGuid
                };
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
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
            /// <summary>
            /// if true, the output will include the rendered html
            /// </summary>
            public bool includeRenderedHtml;
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
