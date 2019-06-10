

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
                SetAddonList_RequestClass request = DeserializeObject<SetAddonList_RequestClass>(cp.Request.Form);
                if (request == null) {
                    //
                    // -- attempt the tmp data format then convert the resulting object to the request type
                    SetAddonList_TmpRequestClass tmpRequest = DeserializeObject<SetAddonList_TmpRequestClass>(cp.Request.Form);
                    if ( tmpRequest != null ) {
                        request = new SetAddonList_RequestClass();
                        request.parentContentGuid = tmpRequest.parentContentGuid;
                        request.parentRecordGuid = tmpRequest.parentRecordGuid;
                        request.addonList = convertTmpAddonList(cp, tmpRequest.addonList);
                    }
                    if (request == null) {
                        //
                        // -- request not valid
                        return SerializeObject(new SetAddonList_ResponseClass() {
                            errorList = new List<string> { "The request is invalid" }
                        });
                    }
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
                if (!core.session.isAuthenticatedContentManager(core, metadata)) {
                    cp.Response.SetStatus(WebServerController.httpResponseStatus401_Unauthorized);
                    return SerializeObject(new SetAddonList_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
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
        }
        // ==========================================================================================
        /// <summary>
        /// Convert the tmp addonList to an addonList object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="tmpAddonList"></param>
        /// <returns></returns>
        public static List<AddonListItemModel> convertTmpAddonList(CPBaseClass cp, List<TmpAddonListItemModel> tmpAddonList) {
            var addonList = new List<AddonListItemModel>();
            foreach (var tmpAddon in tmpAddonList) {
                string addonGuid = string.Empty;
                using (CPCSBaseClass cs = cp.CSNew()) {
                    if (cs.Open("add-ons", "name=" + cp.Db.EncodeSQLText(tmpAddon.guid))) {
                        addonGuid = cs.GetText("ccGuid");
                    }
                }
                var columnList = new List<AddonListColumnItemModel>();
                foreach (var tmpColumn in tmpAddon.columns) {
                    columnList.Add(new AddonListColumnItemModel() {
                        col = tmpColumn.width,
                        className = tmpColumn.className,
                         addonList = convertTmpAddonList( cp, tmpColumn.addonList )
                    });
                }
                addonList.Add(new AddonListItemModel() {
                    renderedHtml = tmpAddon.html,
                    instanceGuid = tmpAddon.guid,
                    designBlockTypeGuid = addonGuid,
                    columns = columnList
                });
            }
            return addonList;
        }
        /// <summary>
        /// object that matches the UI being sent 20190610
        /// </summary>
        public class SetAddonList_TmpRequestClass {
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
            public List<TmpAddonListItemModel> addonList;
        }
        public class TmpAddonListItemModel {
            /// <summary>
            /// map to instanceGuid
            /// </summary>
            public string guid;
            /// <summary>
            /// map to renderedHtml
            /// </summary>
            public string html;
            /// <summary>
            /// use to lookup addon instead of designBlockTypeGuid
            /// </summary>
            public string name;
            //
            public List<TmpAddonListColumnItemModel> columns;
        }
        //
        public class TmpAddonListColumnItemModel {
            //
            public string className;
            /// <summary>
            /// map to .col
            /// </summary>
            public int width;
            //
            public List<TmpAddonListItemModel> addonList;
        }
    }
}
