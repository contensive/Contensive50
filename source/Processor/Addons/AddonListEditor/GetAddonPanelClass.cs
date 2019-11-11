

using System;
using System.Collections.Generic;
using System.Globalization;
using Contensive.BaseClasses;
using Contensive.Models.Db;
using Contensive.Processor;
using Contensive.Processor.Controllers;

using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
//
namespace Contensive.Processor.Addons.AddonListEditor {
    public class GetAddonPanelClass : AddonBaseClass {
        //
        public const string guidDesignBlockFourColumn = "{a9915665-3287-4c14-8fb5-039cc4c115de}";
        public const string guidDesignBlockTwoColumn = "{48b9c19d-4b78-441e-8fab-d2b00c8a0e60}";
        public const string guidDesignBlockThreeColumn = "{2270dff7-9043-43b4-87ce-cf1dddbe99a1}";
        public const string guidDesignBlockTwoColumnLeft = "{a7d36b3a-c2c9-4985-8ba9-0a0db65e7879}";
        public const string guidDesignBlockTwoColumnRight = "{81d90418-ac66-45bd-b756-64d748d1c504}";
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass cp) {
            try {
                CoreController core = ((CPClass)cp).core;
                // 

                GetAddonPanel_RequestClass request = DeserializeObject<GetAddonPanel_RequestClass>(cp.Request.Body);
                //
                // -- allow simple querystring values
                if (request == null) {
                    request = new GetAddonPanel_RequestClass() {
                        parentRecordGuid = cp.Doc.GetText("parentRecordGuid"),
                        parentContentGuid = cp.Doc.GetText("parentContentGuid")
                    };
                }
                if (request == null) {
                    return SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request is invalid" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentContentGuid)) {
                    return SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request parent content guid is not valid [" + request.parentContentGuid + "]" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                    return SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request parent record guid is not valid [" + request.parentRecordGuid + "]" }
                    });
                }
                var metadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, request.parentContentGuid);
                if (metadata == null) {
                    return SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The parent content could not be determined from the guid [" + request.parentContentGuid + "]" }
                    });
                }
                if (!core.session.isAuthenticatedContentManager(metadata)) {
                    cp.Response.SetStatus(WebServerController.httpResponseStatus401_Unauthorized);
                    return SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
                    });
                }
                List<AddonModel> addonModelList = null;
                switch (metadata.name.ToLower(CultureInfo.InvariantCulture)) {
                    case "page content":
                        addonModelList = DbBaseModel.createList<AddonModel>(core.cpParent, "(active>0)and(content>0)");
                        break;
                    case "page templates":
                        addonModelList = DbBaseModel.createList<AddonModel>(core.cpParent, "(active>0)and(template>0)");
                        break;
                    default:
                        return SerializeObject(new GetAddonPanel_ResponseClass() {
                            errorList = new List<string> { "The parent content is not valid addonList container [" + metadata.name + "]" }
                        });
                }
                var result = new GetAddonPanel_ResponseClass() {
                    errorList = new List<string>(),
                    parentContentGuid = request.parentContentGuid,
                    parentRecordGuid = request.parentRecordGuid,
                    addonPanelList = new List<AddonPanelListItemModel>()
                };
                foreach (var addonModel in addonModelList) {
                    //
                    // -- fix iconfilename because it is user typed
                    string iconFilename = string.Empty;
                    if (!string.IsNullOrWhiteSpace(addonModel.iconFilename)) {
                        iconFilename = addonModel.iconFilename.Replace("\\", "/");
                        iconFilename = cp.Site.FilePath + ((iconFilename.Left(1).Equals("/")) ? iconFilename.Substring(1) : iconFilename);
                    }
                    var item = new AddonPanelListItemModel() {
                        name = addonModel.name,
                        designBlockTypeGuid = addonModel.ccguid,
                        renderedHtml = "",
                        image = iconFilename
                    };
                    result.addonPanelList.Add(item);
                    // -- hack a structural work-around for now. If this works, build into addon record
                    switch (addonModel.ccguid.ToLower(CultureInfo.InvariantCulture)) {
                        case guidDesignBlockFourColumn:
                            item.columns = new List<AddonPanelListItemColumnModel> {
                                new AddonPanelListItemColumnModel() { className="col-3-md", col=3 },
                                new AddonPanelListItemColumnModel() { className="col-3-md", col=3 },
                                new AddonPanelListItemColumnModel() { className="col-3-md", col=3 },
                                new AddonPanelListItemColumnModel() { className="col-3-md", col=3 }
                            };
                            break;
                        case guidDesignBlockTwoColumn:
                            item.columns = new List<AddonPanelListItemColumnModel> {
                                new AddonPanelListItemColumnModel() { className="col-6-md", col=6 },
                                new AddonPanelListItemColumnModel() { className="col-6-md", col=6 }
                            };
                            break;
                        case guidDesignBlockThreeColumn:
                            item.columns = new List<AddonPanelListItemColumnModel> {
                                new AddonPanelListItemColumnModel() { className="col-4-md", col=4 },
                                new AddonPanelListItemColumnModel() { className="col-4-md", col=4 },
                                new AddonPanelListItemColumnModel() { className="col-4-md", col=4 }
                            };
                            break;
                        case guidDesignBlockTwoColumnLeft:
                            item.columns = new List<AddonPanelListItemColumnModel> {
                                new AddonPanelListItemColumnModel() { className="col-8-md", col=8 },
                                new AddonPanelListItemColumnModel() { className="col-4-md", col=4 }
                            };
                            break;
                        case guidDesignBlockTwoColumnRight:
                            item.columns = new List<AddonPanelListItemColumnModel> {
                                new AddonPanelListItemColumnModel() { className="col-4-md", col=4 },
                                new AddonPanelListItemColumnModel() { className="col-8-md", col=8 }
                            };
                            break;
                        default:
                            break;
                    }
                }
                return result;
            }
            // 
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
        // 
        public class GetAddonPanel_RequestClass {
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
        public class GetAddonPanel_ResponseClass {
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
            public List<AddonPanelListItemModel> addonPanelList;
        }
    }
}
