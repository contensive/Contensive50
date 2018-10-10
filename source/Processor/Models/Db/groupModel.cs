
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class GroupModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "groups";
        public const string contentTableName = "ccgroups";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool allowBulkEmail { get; set; }
        public string caption { get; set; }        
        public string copyFilename { get; set; }
        public bool publicJoin { get; set; }
        // 
        //====================================================================================================
        public static GroupModel addEmpty(CoreController core) {
            return addEmpty<GroupModel>(core);
        }
        //
        //====================================================================================================
        public static GroupModel addDefault(CoreController core) {
            return addDefault<GroupModel>(core);
        }
        //
        //====================================================================================================
        public static GroupModel addDefault(CoreController core, ref List<string> callersCacheNameList) {
            return addDefault<GroupModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupModel create(CoreController core, int recordId) {
            return create<GroupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static GroupModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<GroupModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupModel create(CoreController core, string recordGuid) {
            return create<GroupModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static GroupModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<GroupModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<GroupModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static GroupModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<GroupModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<GroupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<GroupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<GroupModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<GroupModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<GroupModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<GroupModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<GroupModel> createList(CoreController core, string sqlCriteria) {
            return createList<GroupModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<GroupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<GroupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<GroupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<GroupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static GroupModel createDefault(CoreController core) {
            return createDefault<GroupModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<GroupModel>(core);
        }
    }
}
