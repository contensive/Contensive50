

using System;
using System.Collections.Generic;
using Contensive.BaseClasses;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Models.Domain;
//
namespace Contensive.Addons.AddonListEditor {
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
        public override object Execute(CPBaseClass CP) {
            try {
                CoreController core = ((CPClass)CP).core;
                // 

                GetAddonPanel_RequestClass request = Newtonsoft.Json.JsonConvert.DeserializeObject<GetAddonPanel_RequestClass>(CP.Request.Body);
                if (request == null) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request is invalid" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentContentGuid)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request parent content guid is not valid [" + request.parentContentGuid + "]" }
                    });
                }
                if (String.IsNullOrWhiteSpace(request.parentRecordGuid)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The request parent record guid is not valid [" + request.parentRecordGuid + "]" }
                    });
                }
                var metadata = Contensive.Processor.Models.Domain.ContentMetadataModel.create(core, request.parentContentGuid);
                if (metadata == null) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "The parent content could not be determined from the guid [" + request.parentContentGuid + "]" }
                    });
                }
                if (!core.session.isEditing(metadata.name)) {
                    return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
                        errorList = new List<string> { "Your account does not have permission to edit [" + metadata.name + "]" }
                    });
                }
                List<AddonModel> addonModelList = null;
                switch (metadata.name.ToLower()) {
                    case "page content":
                        addonModelList = AddonModel.createList(core, "(active>0)and(content>0)");
                        break;
                    case "page templates":
                        addonModelList = AddonModel.createList(core, "(active>0)and(template>0)");
                        break;
                    default:
                        return Newtonsoft.Json.JsonConvert.SerializeObject(new GetAddonPanel_ResponseClass() {
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
                    var item = new AddonPanelListItemModel() {
                        name = addonModel.name,
                        guid = addonModel.ccguid,
                        html = "",
                        children = new List<string>(),
                        image = (string.IsNullOrWhiteSpace(addonModel.iconFilename) ? "" : CP.Site.FilePath + addonModel.iconFilename )
                    };
                    
                    result.addonPanelList.Add(item);
                    // -- hack a structural work-around for now. If this works, build into addon record
                    switch (addonModel.ccguid.ToLower()) {
                        case guidDesignBlockFourColumn:
                            item.isStructural = true;
                            item.columnSpacing = new List<int> { 3, 3, 3, 3 };
                            break;
                        case guidDesignBlockTwoColumn:
                            item.isStructural = true;
                            item.columnSpacing = new List<int> { 6, 6 };
                            break;
                        case guidDesignBlockThreeColumn:
                            item.isStructural = true;
                            item.columnSpacing = new List<int> { 4, 4, 4 };
                            break;
                        case guidDesignBlockTwoColumnLeft:
                            item.isStructural = true;
                            item.columnSpacing = new List<int> { 8, 4 };
                            break;
                        case guidDesignBlockTwoColumnRight:
                            item.isStructural = true;
                            item.columnSpacing = new List<int> { 4, 8 };
                            break;
                        default:
                            item.isStructural = false;
                            item.columnSpacing = new List<int> { 12 };
                            break;
                    }
                }
                return result;
            }
            // 
            catch (Exception ex) {
                CP.Site.ErrorReport(ex);
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
