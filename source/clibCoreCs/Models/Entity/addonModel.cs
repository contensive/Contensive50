
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Entity {
    public class addonModel : baseModel {
        //
        //====================================================================================================
        //-- const
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
        public string JavaScriptBodyEnd { get; set; }
        //Public Property JavaScriptOnLoad As String
        public string JSHeadScriptSrc { get; set; }
        public string JSBodyScriptSrc { get; set; }
        public fieldTypeJavascriptFile JSFilename { get; set; }
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
        public static addonModel add(coreClass cpCore) {
            return add<addonModel>(cpCore);
        }
        //
        //====================================================================================================
        public static addonModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<addonModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonModel create(coreClass cpCore, int recordId) {
            addonModel result = create<addonModel>(cpCore, recordId);
            if (result != null) {
                if (string.IsNullOrEmpty(result.ccguid)) {
                    result.ccguid = genericController.createGuid();
                }
            }
            return result;
        }
        //
        //====================================================================================================
        public static addonModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            addonModel result = create<addonModel>(cpCore, recordId, ref callersCacheNameList);
            if (result != null) {
                if (string.IsNullOrEmpty(result.ccguid)) {
                    result.ccguid = genericController.createGuid();
                }
            }
            return result;
        }
        //
        //====================================================================================================
        public static addonModel create(coreClass cpCore, string recordGuid) {
            return create<addonModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static addonModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<addonModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static addonModel createByName(coreClass cpCore, string recordName) {
            return createByName<addonModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static addonModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<addonModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<addonModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<addonModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<addonModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<addonModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<addonModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<addonModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<addonModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<addonModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<addonModel>(cpCore, recordId);
            Models.Complex.routeDictionaryModel.invalidateCache(cpCore);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<addonModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<addonModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<addonModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static addonModel createDefault(coreClass cpcore) {
            return createDefault<addonModel>(cpcore);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get a list of addons to run on new visit
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<addonModel> createList_OnNewVisitEvent(coreClass cpCore, List<string> callersCacheNameList) {
            List<addonModel> result = new List<addonModel>();
            try {
                result = createList(cpCore, "(OnNewVisitEvent<>0)");
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static List<addonModel> createList_OnPageStartEvent(coreClass cpCore, List<string> callersCacheNameList) {
            List<addonModel> result = new List<addonModel>();
            try {
                result = createList(cpCore, "(OnPageStartEvent<>0)");
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static List<addonModel> createList_RemoteMethods(coreClass cpCore, List<string> callersCacheNameList) {
            List<addonModel> result = new List<addonModel>();
            try {
                result = createList(cpCore, "(remoteMethod=1)");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        public static List<addonModel> createList_pageDependencies(coreClass cpCore, int pageId) {
            List<addonModel> result = new List<addonModel>();
            try {
                result = createList(cpCore, "(id in (select addonId from ccAddonPageRules where (pageId=" + pageId + ")))");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        public static List<addonModel> createList_templateDependencies(coreClass cpCore, int templateId) {
            List<addonModel> result = new List<addonModel>();
            try {
                result = createList(cpCore, "(id in (select addonId from ccAddonTemplateRules where (templateId=" + templateId + ")))");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        public class addonCacheClass {
            private Dictionary<int, addonModel> dictIdAddon = new Dictionary<int, addonModel>();
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
            public void add(coreClass cpcore, addonModel addon) {
                if (!dictIdAddon.ContainsKey(addon.id)) {
                    dictIdAddon.Add(addon.id, addon);
                    if (string.IsNullOrEmpty(addon.ccguid)) {
                        addon.ccguid = genericController.createGuid();
                        addon.save(cpcore);
                    }
                    if (!dictGuidId.ContainsKey(addon.ccguid.ToLower())) {
                        dictGuidId.Add(addon.ccguid.ToLower(), addon.id);
                        if (string.IsNullOrEmpty(addon.name.Trim())) {
                            addon.name = "addon " + addon.id.ToString();
                            addon.save(cpcore);
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
            public addonModel getAddonByGuid(string guid) {
                if (this.dictGuidId.ContainsKey(guid.ToLower())) {
                    return getAddonById(this.dictGuidId[guid.ToLower()]);
                }
                return null;
            }
            public addonModel getAddonByName(string name) {
                if (this.dictNameId.ContainsKey(name.ToLower())) {
                    return getAddonById(this.dictNameId[name.ToLower()]);
                }
                return null;
            }
            public addonModel getAddonById(int addonId) {
                if (this.dictIdAddon.ContainsKey(addonId)) {
                    return this.dictIdAddon[addonId];
                }
                return null;
            }
            //
            private List<addonModel> getAddonList(List<int> addonIdList) {
                List<addonModel> result = new List<addonModel>();
                foreach (int addonId in addonIdList) {
                    result.Add(getAddonById(addonId));
                }
                return result;
            }
            //
            public List<addonModel> getOnBodyEndAddonList() {
                return getAddonList(onBodyEndIdList);
            }
            //
            public List<addonModel> getOnBodyStartAddonList() {
                return getAddonList(onBodyStartIdList);
            }
            //
            public List<addonModel> getOnNewVisitAddonList() {
                return getAddonList(onNewVisitIdList);
            }
            //
            public List<addonModel> getOnPageEndAddonList() {
                return getAddonList(OnPageEndIdList);
            }
            //
            public List<addonModel> getOnPageStartAddonList() {
                return getAddonList(OnPageStartIdList);
            }
        }
    }
}
