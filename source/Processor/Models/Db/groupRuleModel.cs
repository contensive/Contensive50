
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class GroupRuleModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group rules";
        public const string contentTableName = "ccgrouprules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public bool allowAdd { get; set; }
        public bool allowDelete { get; set; }        
        public int contentID { get; set; }
        public int groupID { get; set; }
        // 
        //====================================================================================================
        public static GroupRuleModel addEmpty(CoreController core) {
            return addEmpty<GroupRuleModel>(core);
        }
        //
        //====================================================================================================
        public static GroupRuleModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<GroupRuleModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static GroupRuleModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<GroupRuleModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, int recordId) {
            return create<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<GroupRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, string recordGuid) {
            return create<GroupRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<GroupRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<GroupRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static GroupRuleModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<GroupRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<GroupRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<GroupRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<GroupRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<GroupRuleModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<GroupRuleModel>(core);
        }
    }
}
