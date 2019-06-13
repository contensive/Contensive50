

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
        public override object Execute(CPBaseClass cp) {
            try {
                CoreController core = ((CPClass)cp).core;
                //
                SetAddonList_RequestClass request = DeserializeObject<SetAddonList_RequestClass>(cp.Request.Body);
                //if (request == null) {
                //    request = new SetAddonList_RequestClass();
                //    //
                //    request.parentContentGuid = cp.Doc.GetText("parentContentGuid");
                //    if (string.IsNullOrWhiteSpace(request.parentContentGuid)) {
                //        //
                //        // -- parentContentGuid blank
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "Form Entity is not a valid json object, and request key [parentContentGuid] is empty" }
                //        });
                //    }
                //    request.parentRecordGuid = cp.Doc.GetText("parentRecordGuid");
                //    if (string.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                //        //
                //        // -- parentRecordGuid blank
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "Form Entity is not a valid json object, and request key [parentRecordGuid] is empty" }
                //        });
                //    }
                //    string addonListJson = cp.Doc.GetText("addonList");
                //    if (string.IsNullOrWhiteSpace(addonListJson)) {
                //        //
                //        // -- addonList blank
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "Form Entity is not a valid json object, and request key [addonList] is empty" }
                //        });
                //    }
                //    try {
                //        request.addonList = DeserializeObject<List<AddonListItemModel>>(addonListJson);
                //    } catch (Exception) {
                //        //
                //        // -- addonList did not deserialize correctly
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "Form Entity is not a valid json object, and request key [addonList] did not deserialize into an addonList object. addonList [" + addonListJson + "]" }
                //        });
                //    }
                //    if (request.addonList == null) {
                //        //
                //        // -- addonList did not deserialize correctly
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "Form Entity is not a valid json object, and request key [addonList] is did not deserialize into an addonList object. addonList [" + addonListJson + "]" }
                //        });
                //    }
                //    //
                //    // -- attempt the tmp data format then convert the resulting object to the request type
                //    SetAddonList_TmpRequestClass tmpRequest = DeserializeObject<SetAddonList_TmpRequestClass>(p.Request.Body);
                //    if (tmpRequest != null) {
                //        request = new SetAddonList_RequestClass();
                //        request.parentContentGuid = tmpRequest.parentContentGuid;
                //        request.parentRecordGuid = tmpRequest.parentRecordGuid;
                //        request.addonList = convertTmpAddonList(cp, tmpRequest.addonList);
                //    }
                //    if (request == null) {
                //        //
                //        // -- request not valid
                //        return SerializeObject(new SetAddonList_ResponseClass() {
                //            errorList = new List<string> { "The request is invalid" }
                //        });
                //    }
                //}
                if (request == null) {
                    //
                    // -- request not valid
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
                if (!core.session.isAuthenticatedContentManager(metadata)) {
                    cp.Response.SetStatus(WebServerController.httpResponseStatus401_Unauthorized);
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
                    });
                }
                //
                // -- validate addonList from UI and set back into a string
                AddonListItemModel addonListItem = null;
                switch (metadata.name.ToLower()) {
                    case "page content":
                        var page = Contensive.Processor.Models.Db.PageContentModel.create(core, request.parentRecordGuid);
                        if (page == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Content] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        addonListItem = renderNewAddonInList(cp, request.addonList);
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
                        addonListItem = renderNewAddonInList(cp, request.addonList);
                        template.addonList = SerializeObject(request.addonList);
                        template.save(core);
                        break;
                    case "email":
                        var email = Contensive.Processor.Models.Db.EmailModel.create(core, request.parentRecordGuid);
                        if (email == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Email] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        addonListItem = renderNewAddonInList(cp, request.addonList);
                        email.addonList = SerializeObject(request.addonList);
                        email.save(core);
                        break;
                    default:
                        return SerializeObject(new SetAddonList_ResponseClass() {
                            errorList = new List<string> { "The parent content is not valid addonList container [" + metadata.name + "]" }
                        });
                }
                var result = new SetAddonList_ResponseClass() {
                    errorList = new List<string>()
                };
                if ( addonListItem != null ) {
                    result.renderedHtml = addonListItem.renderedHtml;
                    result.renderedAssets = addonListItem.renderedAssets;
                };
                return SerializeObject(result);
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        /// <summary>
        /// The request object
        /// </summary>
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
        /// <summary>
        /// The object returned from this method
        /// </summary>
        public class SetAddonList_ResponseClass {
            /// <summary>
            /// The guid of the content where this list is stored (content + record define location)
            /// </summary>
            public List<string> errorList;
            //
            public string renderedHtml;
            //
            public AddonAssetsModel renderedAssets;
        }
        // ==========================================================================================
        /// <summary>
        /// Find the new addon in the list and render the html, return the addonListItem with rendered Html
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="addonList"></param>
        /// <returns></returns>
        public static AddonListItemModel renderNewAddonInList(CPBaseClass cp, List<AddonListItemModel> addonList) {
            AddonListItemModel renderedAddonItem = null;
            foreach (var addon in addonList) {
                if (string.IsNullOrWhiteSpace(addon.instanceGuid)) {
                    //
                    // -- found the missing instance, create guid, save list, execute the addon and return
                    renderedAddonItem = new AddonListItemModel {
                        instanceGuid = Guid.NewGuid().ToString(),
                        designBlockTypeGuid = addon.designBlockTypeGuid,
                        designBlockTypeName = addon.designBlockTypeName,
                        columns = addon.columns,
                    };
                    AddonListController.renderEdit(cp, renderedAddonItem);
                    return renderedAddonItem;
                }
                if (addon.columns != null) {
                    foreach (var column in addon.columns) {
                        renderedAddonItem = renderNewAddonInList(cp, column.addonList);
                        if (renderedAddonItem != null) { return renderedAddonItem; }
                    }
                }
            }
            return renderedAddonItem;
        }
    }
}
