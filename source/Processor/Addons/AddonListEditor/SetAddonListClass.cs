

using System;
using System.Collections.Generic;
using System.Globalization;
using Contensive.BaseClasses;
using Contensive.BaseModels;
using Contensive.Models.Db;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
//
namespace Contensive.Processor.Addons.AddonListEditor {
    public class SetAddonListClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass cp) {
            try {
                CoreController core = ((CPClass)cp).core;
                //
                SetAddonList_RequestClass request = DeserializeObject<SetAddonList_RequestClass>(cp.Request.Body);
                //
                cp.Utils.AppendLog("SetAddonListClass, request.body [" + cp.Request.Body + "]");
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
                // -- normalize the list to clean up what might be confusing data
                AddonListController.normalizeAddonList(cp, request.addonList);
                //
                // -- validate addonList from UI and set back into a string
                AddonListItemModel addonListItem = null;
                switch (metadata.name.ToLower(CultureInfo.InvariantCulture)) {
                    case "page content":
                        var page = DbBaseModel.create<PageContentModel>(core.cpParent, request.parentRecordGuid);
                        if (page == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Content] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        //
                        cp.Utils.AppendLog("SetAddonListClass, render addonList and update page [" + page.id + "," + page.name + "]");
                        //
                        addonListItem = renderNewAddonInList(cp, request.addonList);
                        page.addonList = SerializeObject(request.addonList);
                        page.save(cp);
                        break;
                    case "page templates":
                        var template = DbBaseModel.create<PageTemplateModel>(cp, request.parentRecordGuid);
                        if (template == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Page Templates] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        addonListItem = renderNewAddonInList(cp, request.addonList);
                        template.addonList = SerializeObject(request.addonList);
                        template.save(cp);
                        break;
                    case "email":
                        var email = EmailModel.create<EmailModel>(cp, request.parentRecordGuid);
                        if (email == null) {
                            return SerializeObject(new SetAddonList_ResponseClass() {
                                errorList = new List<string> { "The parent record in [Email] could not be determined from the guid [" + request.parentRecordGuid + "]" }
                            });
                        }
                        addonListItem = renderNewAddonInList(cp, request.addonList);
                        email.addonList = SerializeObject(request.addonList);
                        email.save(cp);
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
            try {
                //
                cp.Utils.AppendLog("SetAddonListClass.renderNewAddonInList enter");
                //
                AddonListItemModel renderedAddonItem = null;
                foreach (var addon in addonList) {
                    if (string.IsNullOrWhiteSpace(addon.instanceGuid)) {
                        //
                        // -- found the missing instance, create guid, save list, execute the addon and return
                        //
                        cp.Utils.AppendLog("SetAddonListClass.renderNewAddonInList found addon in list with the missing instance [typename:" + addon.designBlockTypeName + ",typeGuid:" + addon.designBlockTypeGuid + "]");
                        //
                        addon.instanceGuid = Guid.NewGuid().ToString();
                        renderedAddonItem = new AddonListItemModel {
                            instanceGuid = addon.instanceGuid,
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
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
