
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class CopyContentModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "copy content";
        public const string contentTableName = "cccopycontent";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string copy { get; set; }
        // 
        //====================================================================================================
        public static CopyContentModel addEmpty(CoreController core) {
            return addEmpty<CopyContentModel>(core);
        }
        //
        //====================================================================================================
        public static CopyContentModel addDefault(CoreController core, Domain.MetaModel cdef) {
            return addDefault<CopyContentModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static CopyContentModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel cdef) {
            return addDefault<CopyContentModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static CopyContentModel create(CoreController core, int recordId) {
            return create<CopyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static CopyContentModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<CopyContentModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static CopyContentModel create(CoreController core, string recordGuid) {
            return create<CopyContentModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static CopyContentModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<CopyContentModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static CopyContentModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<CopyContentModel>(core, recordName );
        }
        //
        //====================================================================================================
        public static CopyContentModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<CopyContentModel>(core, recordName, ref callersCacheNameList );
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<CopyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<CopyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<CopyContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<CopyContentModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<CopyContentModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<CopyContentModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<CopyContentModel> createList(CoreController core, string sqlCriteria) {
            return createList<CopyContentModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<CopyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<CopyContentModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<CopyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<CopyContentModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static CopyContentModel createDefault(CoreController core) {
        //    return createDefault<CopyContentModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<CopyContentModel>(core);
        }
    }
}
