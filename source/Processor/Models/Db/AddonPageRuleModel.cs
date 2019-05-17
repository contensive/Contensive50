
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonPageRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on page rules";
        public const string contentTableNameLowerCase = "ccaddonpagerules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int pageId { get; set; }
        // 
        //====================================================================================================
        public static AddonPageRuleModel addEmpty(CoreController core) {
            return addEmpty<AddonPageRuleModel>(core);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<AddonPageRuleModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<AddonPageRuleModel>(core, metaData, ref callersCacheNameList);
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
        public static AddonPageRuleModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<AddonPageRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonPageRuleModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<AddonPageRuleModel>(core, recordName, ref callersCacheNameList);
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
            invalidateCacheOfRecord<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbBaseModel.getRecordName<AddonPageRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordName<AddonPageRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordId<AddonPageRuleModel>(core, ccGuid);
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
