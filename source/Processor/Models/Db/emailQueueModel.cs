
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailQueueModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email queue";
        public const string contentTableName = "ccEmailQueue";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        //
        public string toAddress { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public bool immediate { get; set;  }
        public int attempts { get; set; }
        // 
        //====================================================================================================
        public static EmailQueueModel addEmpty(CoreController core) {
            return addEmpty<EmailQueueModel>(core);
        }
        //
        //====================================================================================================
        public static EmailQueueModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<EmailQueueModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static EmailQueueModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<EmailQueueModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, int recordId) {
            return create<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailQueueModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, string recordGuid) {
            return create<EmailQueueModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailQueueModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<EmailQueueModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailQueueModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<EmailQueueModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailQueueModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailQueueModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailQueueModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static EmailQueueModel createDefault(CoreController core) {
        //    return createDefault<EmailQueueModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<EmailQueueModel>(core);
        }
    }
}
