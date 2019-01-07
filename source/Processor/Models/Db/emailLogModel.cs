
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailLogModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email log";
        public const string contentTableName = "ccEmailLog";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public DateTime dateBlockExpires { get; set; }
        public int emailDropID { get; set; }
        public int emailID { get; set; }
        public string fromAddress { get; set; }
        public int logType { get; set; }
        public int memberID { get; set; }
        public string sendStatus { get; set; }
        public string subject { get; set; }
        public string toAddress { get; set; }
        public int visitID { get; set; }
        public string body { get; set; }
        // 
        //====================================================================================================
        public static EmailLogModel addEmpty(CoreController core) {
            return addEmpty<EmailLogModel>(core);
        }
        //
        //====================================================================================================
        public static EmailLogModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<EmailLogModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static EmailLogModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<EmailLogModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailLogModel create(CoreController core, int recordId) {
            return create<EmailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailLogModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailLogModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailLogModel create(CoreController core, string recordGuid) {
            return create<EmailLogModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailLogModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailLogModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailLogModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<EmailLogModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailLogModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<EmailLogModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailLogModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailLogModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailLogModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailLogModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<EmailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<EmailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<EmailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<EmailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<EmailLogModel>(core);
        }
    }
}
