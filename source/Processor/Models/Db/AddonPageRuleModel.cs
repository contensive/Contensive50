﻿
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonPageRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on page rules";
        public const string contentTableName = "ccAddonPageRules";
        public const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int pageId { get; set; }
        //
        //====================================================================================================
        public static AddonPageRuleModel add(CoreController core) {
            return add<AddonPageRuleModel>(core);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonPageRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel create(CoreController core, int recordId) {
            return create<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonPageRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel create(CoreController core, string recordGuid) {
            return create<AddonPageRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonPageRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel createByName(CoreController core, string recordName) {
            return createByName<AddonPageRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonPageRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonPageRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonPageRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonPageRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonPageRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel createDefault(CoreController core) {
            return createDefault<AddonPageRuleModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonPageRuleModel>(core);
        }
    }
}
