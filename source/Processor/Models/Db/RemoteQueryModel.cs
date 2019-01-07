
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class RemoteQueryModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "remote queries";
        public const string contentTableName = "ccRemoteQueries";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        //
        public bool AllowInactiveRecords { get; set; }
        public int ContentID { get; set; }
        public string Criteria { get; set; }
        public int DataSourceID { get; set; }
        public DateTime DateExpires { get; set; }
        public int MaxRows { get; set; }
        public int QueryTypeID { get; set; }
        public string RemoteKey { get; set; }
        public string SelectFieldList { get; set; }
        public string SortFieldList { get; set; }
        public string SQLQuery { get; set; }
        public int VisitID { get; set; }
        //
        //====================================================================================================
        public static RemoteQueryModel addEmpty(CoreController core) {
            return addEmpty<RemoteQueryModel>(core);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<RemoteQueryModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<RemoteQueryModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel create(CoreController core, int recordId) {
            return create<RemoteQueryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<RemoteQueryModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel create(CoreController core, string recordGuid) {
            return create<RemoteQueryModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<RemoteQueryModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<RemoteQueryModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static RemoteQueryModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<RemoteQueryModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<RemoteQueryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<RemoteQueryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<RemoteQueryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<RemoteQueryModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<RemoteQueryModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<RemoteQueryModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<RemoteQueryModel> createList(CoreController core, string sqlCriteria) {
            return createList<RemoteQueryModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<RemoteQueryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<RemoteQueryModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<RemoteQueryModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<RemoteQueryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<RemoteQueryModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<RemoteQueryModel>(core);
        }
    }
}
