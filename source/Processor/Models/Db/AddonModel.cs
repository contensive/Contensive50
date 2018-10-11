
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-ons";
        public const string contentTableName = "ccaggregatefunctions";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        //
        public bool admin { get; set; }
        public string argumentList { get; set; }
        public bool asAjax { get; set; }
        public bool blockEditTools { get; set; }
        public int collectionID { get; set; }
        public bool content { get; set; }
        public string copy { get; set; }
        public string copyText { get; set; }
        public string dotNetClass { get; set; }
        public bool email { get; set; }
        public bool filter { get; set; }
        public string formXML { get; set; }
        public string help { get; set; }
        public string helpLink { get; set; }
        public string iconFilename { get; set; }
        public int iconHeight { get; set; }
        public int iconSprites { get; set; }
        public int iconWidth { get; set; }
        public bool inFrame { get; set; }
        public bool isInline { get; set; }
        public bool javascriptForceHead { get; set; }
        public string jsHeadScriptSrc { get; set; }
        public FieldTypeJavascriptFile jsFilename { get; set; }
        //public string JavaScriptBodyEnd { get; set; }
        //Public Property JavaScriptOnLoad As String
        //public string JSBodyScriptSrc { get; set; }
        public string link { get; set; }
        public string metaDescription { get; set; }
        public string metaKeywordList { get; set; }
        public int navTypeID { get; set; }
        public string objectProgramID { get; set; }
        public bool onBodyEnd { get; set; }
        public bool onBodyStart { get; set; }
        public bool onNewVisitEvent { get; set; }
        public bool onPageEndEvent { get; set; }
        public bool onPageStartEvent { get; set; }
        public bool htmlDocument { get; set; }
        public string otherHeadTags { get; set; }
        public string pageTitle { get; set; }
        public int processInterval { get; set; }
        public DateTime processNextRun { get; set; }
        public bool processRunOnce { get; set; }
        public string processServerKey { get; set; }
        public string remoteAssetLink { get; set; }
        public bool remoteMethod { get; set; }
        public string robotsTxt { get; set; }
        public string scriptingCode { get; set; }
        public string scriptingEntryPoint { get; set; }
        public int scriptingLanguageID { get; set; }
        public string scriptingTimeout { get; set; }
        public FieldTypeCSSFile stylesFilename { get; set; }
        public string stylesLinkHref { get; set; }
        public bool template { get; set; }
        // 
        //====================================================================================================
        public static AddonModel addEmpty(CoreController core) {
            return addEmpty<AddonModel>(core);
        }
        //
        //====================================================================================================
        public static AddonModel addDefault(CoreController core) {
            return addDefault<AddonModel>(core);
        }
        //
        //====================================================================================================
        public static AddonModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<AddonModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonModel create(CoreController core, int recordId) {
            AddonModel result = create<AddonModel>(core, recordId);
            if (result != null) {
                if (string.IsNullOrEmpty(result.ccguid)) {
                    result.ccguid = GenericController.getGUID();
                }
            }
            return result;
        }
        //
        //====================================================================================================
        public static AddonModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            AddonModel result = create<AddonModel>(core, recordId, ref callersCacheNameList);
            if (result != null) {
                if (string.IsNullOrEmpty(result.ccguid)) {
                    result.ccguid = GenericController.getGUID();
                }
            }
            return result;
        }
        //
        //====================================================================================================
        public static AddonModel create(CoreController core, string recordGuid) {
            return create<AddonModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonModel createByUniqueName(CoreController core, string recordName ) {
            return createByUniqueName<AddonModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<AddonModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonModel>(core, recordId);
            Models.Domain.RouteDictionaryModel.invalidateCache(core);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<AddonModel>(core);
            Models.Domain.RouteDictionaryModel.invalidateCache(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static AddonModel createDefault(CoreController core) {
        //    return createDefault<AddonModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Get a list of addons to run on new visit
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<AddonModel> createList_OnNewVisitEvent(CoreController core, List<string> callersCacheNameList) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(OnNewVisitEvent<>0)");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get list of addons that run on page start event
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<AddonModel> createList_OnPageStartEvent(CoreController core, List<string> callersCacheNameList) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(OnPageStartEvent<>0)");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// pattern get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<AddonModel> createList_RemoteMethods(CoreController core, List<string> callersCacheNameList) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(remoteMethod=1)");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        public static List<AddonModel> createList_pageDependencies(CoreController core, int pageId) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(id in (select addonId from ccAddonPageRules where (pageId=" + pageId + ")))");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        public static List<AddonModel> createList_templateDependencies(CoreController core, int templateId) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(id in (select addonId from ccAddonTemplateRules where (templateId=" + templateId + ")))");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        public class AddonCacheClass {
            private Dictionary<int, AddonModel> dictIdAddon = new Dictionary<int, AddonModel>();
            private Dictionary<string, int> dictGuidId = new Dictionary<string, int>();
            private Dictionary<string, int> dictNameId = new Dictionary<string, int>();
            //
            private List<int> onBodyEndIdList = new List<int>();
            private List<int> onBodyStartIdList = new List<int>();
            private List<int> onNewVisitIdList = new List<int>();
            private List<int> OnPageEndIdList = new List<int>();
            private List<int> OnPageStartIdList = new List<int>();
            public string robotsTxt = "";
            //
            public void add(CoreController core, AddonModel addon) {
                if (!dictIdAddon.ContainsKey(addon.id)) {
                    dictIdAddon.Add(addon.id, addon);
                    if (string.IsNullOrEmpty(addon.ccguid)) {
                        addon.ccguid = GenericController.getGUID();
                        addon.save(core);
                    }
                    if (!dictGuidId.ContainsKey(addon.ccguid.ToLower())) {
                        dictGuidId.Add(addon.ccguid.ToLower(), addon.id);
                        if (string.IsNullOrEmpty(addon.name.Trim())) {
                            addon.name = "addon " + addon.id.ToString();
                            addon.save(core);
                        }
                        if (!dictNameId.ContainsKey(addon.name.ToLower())) {
                            dictNameId.Add(addon.name.ToLower(), addon.id);
                        }
                    }
                }
                if ((addon.onBodyEnd) && (!onBodyEndIdList.Contains(addon.id))) {
                    onBodyEndIdList.Add(addon.id);
                }
                if ((addon.onBodyStart) && (!onBodyStartIdList.Contains(addon.id))) {
                    onBodyStartIdList.Add(addon.id);
                }
                if ((addon.onNewVisitEvent) && (!onNewVisitIdList.Contains(addon.id))) {
                    onNewVisitIdList.Add(addon.id);
                }
                if ((addon.onPageEndEvent) && (!OnPageEndIdList.Contains(addon.id))) {
                    OnPageEndIdList.Add(addon.id);
                }
                if ((addon.onPageStartEvent) && (!OnPageStartIdList.Contains(addon.id))) {
                    OnPageStartIdList.Add(addon.id);
                }
                robotsTxt += "\r\n" + addon.robotsTxt;
            }
            public AddonModel getAddonByGuid(string guid) {
                if (this.dictGuidId.ContainsKey(guid.ToLower())) {
                    return getAddonById(this.dictGuidId[guid.ToLower()]);
                }
                return null;
            }
            public AddonModel getAddonByName(string name) {
                if (this.dictNameId.ContainsKey(name.ToLower())) {
                    return getAddonById(this.dictNameId[name.ToLower()]);
                }
                return null;
            }
            public AddonModel getAddonById(int addonId) {
                if (this.dictIdAddon.ContainsKey(addonId)) {
                    return this.dictIdAddon[addonId];
                }
                return null;
            }
            //
            private List<AddonModel> getAddonList(List<int> addonIdList) {
                List<AddonModel> result = new List<AddonModel>();
                foreach (int addonId in addonIdList) {
                    result.Add(getAddonById(addonId));
                }
                return result;
            }
            //
            public List<AddonModel> getOnBodyEndAddonList() {
                return getAddonList(onBodyEndIdList);
            }
            //
            public List<AddonModel> getOnBodyStartAddonList() {
                return getAddonList(onBodyStartIdList);
            }
            //
            public List<AddonModel> getOnNewVisitAddonList() {
                return getAddonList(onNewVisitIdList);
            }
            //
            public List<AddonModel> getOnPageEndAddonList() {
                return getAddonList(OnPageEndIdList);
            }
            //
            public List<AddonModel> getOnPageStartAddonList() {
                return getAddonList(OnPageStartIdList);
            }
            //
            //====================================================================================================
            /// <summary>
            /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
            /// </summary>
            /// <param name="core"></param>
            /// <returns></returns>
            public static string getTableInvalidationKey(CoreController core) {
                return getTableCacheKey<AddonModel>(core);
            }
        }
    }
}
