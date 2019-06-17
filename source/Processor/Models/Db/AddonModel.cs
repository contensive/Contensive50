
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    [Serializable]
    public class AddonModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-ons";
        public const string contentTableNameLowerCase = "ccaggregatefunctions";
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
        public bool diagnostic { get; set; }
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
        // -- deprecated, but for leave for now and log error
        public string javaScriptBodyEnd { get; set; }
        public string jsBodyScriptSrc { get; set; }
        //
        // -- deprecated
        // -Public Property JavaScriptOnLoad As String
        //====================================================================================================
        public static AddonModel addEmpty(CoreController core) {
            return addEmpty<AddonModel>(core);
        }
        //
        //====================================================================================================
        public static AddonModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<AddonModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static AddonModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<AddonModel>(core, metaData, ref callersCacheNameList);
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
            invalidateCacheOfRecord<AddonModel>(core, recordId);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateCacheOfTable<AddonModel>(core);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbBaseModel.getRecordName<AddonModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordName<AddonModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordId<AddonModel>(core, ccGuid);
        }
        //
        public static List<AddonModel> createList_pageDependencies(CoreController core, int pageId) {
            List<AddonModel> result = new List<AddonModel>();
            try {
                result = createList(core, "(id in (select addonId from ccAddonPageRules where (pageId=" + pageId + ")))");
            } catch (Exception ex) {
                LogController.logError( core,ex);
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
                LogController.logError( core,ex);
                throw;
            }
            return result;
        }
    }
}
