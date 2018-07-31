
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
    public class AddonModel : baseModel {
        //
        //====================================================================================================
        //-- const
        // todo contentTablename should just be tableName, contentDatasource should be DataSource
        public const string contentName = "add-ons";
        public const string contentTableName = "ccaggregatefunctions";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        //
        public bool Admin { get; set; }
        public string ArgumentList { get; set; }
        public bool AsAjax { get; set; }
        public bool BlockEditTools { get; set; }
        public int CollectionID { get; set; }
        public bool Content { get; set; }
        public string Copy { get; set; }
        public string CopyText { get; set; }
        public string DotNetClass { get; set; }
        public bool Email { get; set; }
        public bool Filter { get; set; }
        public string FormXML { get; set; }
        public string Help { get; set; }
        public string HelpLink { get; set; }
        public string IconFilename { get; set; }
        public int IconHeight { get; set; }
        public int IconSprites { get; set; }
        public int IconWidth { get; set; }
        public bool InFrame { get; set; }
        public bool IsInline { get; set; }
        public bool javascriptForceHead { get; set; }
        public string JSHeadScriptSrc { get; set; }
        public fieldTypeJavascriptFile JSFilename { get; set; }
        //public string JavaScriptBodyEnd { get; set; }
        //Public Property JavaScriptOnLoad As String
        //public string JSBodyScriptSrc { get; set; }
        public string Link { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywordList { get; set; }
        public int NavTypeID { get; set; }
        public string ObjectProgramID { get; set; }
        public bool OnBodyEnd { get; set; }
        public bool OnBodyStart { get; set; }
        public bool OnNewVisitEvent { get; set; }
        public bool OnPageEndEvent { get; set; }
        public bool OnPageStartEvent { get; set; }
        public bool htmlDocument { get; set; }
        public string OtherHeadTags { get; set; }
        public string PageTitle { get; set; }
        public int ProcessInterval { get; set; }
        public DateTime ProcessNextRun { get; set; }
        public bool ProcessRunOnce { get; set; }
        public string ProcessServerKey { get; set; }
        public string RemoteAssetLink { get; set; }
        public bool RemoteMethod { get; set; }
        public string RobotsTxt { get; set; }
        public string ScriptingCode { get; set; }
        public string ScriptingEntryPoint { get; set; }
        public int ScriptingLanguageID { get; set; }
        public string ScriptingTimeout { get; set; }
        public fieldTypeCSSFile StylesFilename { get; set; }
        public string StylesLinkHref { get; set; }
        public bool Template { get; set; }
        //====================================================================================================
        public static AddonModel add(CoreController core) {
            return add<AddonModel>(core);
        }
        //
        //====================================================================================================
        public static AddonModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonModel create(CoreController core, int recordId) {
            AddonModel result = create<AddonModel>(core, recordId);
            if (result != null) {
                if (string.IsNullOrEmpty(result.ccguid)) {
                    result.ccguid = genericController.getGUID();
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
                    result.ccguid = genericController.getGUID();
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
        public static AddonModel createByName(CoreController core, string recordName) {
            return createByName<AddonModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
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
        public static void invalidateCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<AddonModel>(core, recordId);
            Models.Complex.routeDictionaryModel.invalidateCache(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<AddonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonModel createDefault(CoreController core) {
            return createDefault<AddonModel>(core);
        }
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
                logController.handleError( core,ex);
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
                logController.handleError( core,ex);
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
                logController.handleError( core,ex);
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
                logController.handleError( core,ex);
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
                logController.handleError( core,ex);
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
                        addon.ccguid = genericController.getGUID();
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
                if ((addon.OnBodyEnd) && (!onBodyEndIdList.Contains(addon.id))) {
                    onBodyEndIdList.Add(addon.id);
                }
                if ((addon.OnBodyStart) && (!onBodyStartIdList.Contains(addon.id))) {
                    onBodyStartIdList.Add(addon.id);
                }
                if ((addon.OnNewVisitEvent) && (!onNewVisitIdList.Contains(addon.id))) {
                    onNewVisitIdList.Add(addon.id);
                }
                if ((addon.OnPageEndEvent) && (!OnPageEndIdList.Contains(addon.id))) {
                    OnPageEndIdList.Add(addon.id);
                }
                if ((addon.OnPageStartEvent) && (!OnPageStartIdList.Contains(addon.id))) {
                    OnPageStartIdList.Add(addon.id);
                }
                robotsTxt += "\r\n" + addon.RobotsTxt;
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
        }
    }
}
