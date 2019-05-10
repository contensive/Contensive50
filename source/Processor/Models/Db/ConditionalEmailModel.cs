
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class ConditionalEmailModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "conditional email";
        public const string contentTableName = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        //
        public DateTime conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public bool sent { get; set; }
        public bool submitted { get; set; }
        // 
        //====================================================================================================
        public static ConditionalEmailModel addEmpty(CoreController core) {
            return addEmpty<ConditionalEmailModel>(core);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<ConditionalEmailModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<ConditionalEmailModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, int recordId) {
            return create<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ConditionalEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, string recordGuid) {
            return create<ConditionalEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ConditionalEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<ConditionalEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<ConditionalEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ConditionalEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ConditionalEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<ConditionalEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateCacheOfRecord<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbBaseModel.getRecordName<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordName<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordId<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<ConditionalEmailModel>(core);
        }
    }
}
