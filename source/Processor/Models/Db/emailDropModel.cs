﻿
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailDropModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Email Drops";
        public const string contentTableName = "ccEmailDrops";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int emailID { get; set; }
        // 
        //====================================================================================================
        public static EmailDropModel addEmpty(CoreController core) {
            return AddEmpty<EmailDropModel>(core);
        }
        //
        //====================================================================================================
        public static EmailDropModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<EmailDropModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static EmailDropModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<EmailDropModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, int recordId) {
            return create<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailDropModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, string recordGuid) {
            return create<EmailDropModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailDropModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<EmailDropModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailDropModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<EmailDropModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailDropModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailDropModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailDropModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<EmailDropModel>(core);
        }
    }
}
