
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class _BlankModel : DbModel {
        //
        //====================================================================================================
        //-- const
        //
        public const string contentName = "tables";            //<------ set content name
        public const string contentTableName = "ccTables";     //<------ set to tablename for the primary content (used for cache names)
        public const string contentDataSource = "default";     //<------ set to datasource if not default
        public const bool nameFieldIsUnique = false;           //<----- set true if the name field's value for all records must be unique (no duplicates). Used for cache ptr generation
        //
        //====================================================================================================
        // -- instance properties
        //
        public int dataSourceID { get; set; }                   //<------ replace this with a list all model fields not part of the base model
        //
        //====================================================================================================
        //
        public static _BlankModel addEmpty(CoreController core) => addEmpty<_BlankModel>(core);
        //
        //====================================================================================================
        //
        public static _BlankModel addDefault(CoreController core, Domain.MetaModel metaData) => addDefault<_BlankModel>(core, metaData);
        //
        //====================================================================================================
        //
        public static _BlankModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) => addDefault<_BlankModel>(core, metaData, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static _BlankModel create(CoreController core, int recordId) => create<_BlankModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static _BlankModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) => create<_BlankModel>(core, recordId, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static _BlankModel create(CoreController core, string recordGuid) => create<_BlankModel>(core, recordGuid);
        //
        //====================================================================================================
        //
        public static _BlankModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) => create<_BlankModel>(core, recordGuid, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public static _BlankModel createByUniqueName(CoreController core, string recordName) => createByUniqueName<_BlankModel>(core, recordName);
        //
        //====================================================================================================
        public static _BlankModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) => createByUniqueName<_BlankModel>(core, recordName, ref callersCacheNameList);
        //
        //====================================================================================================
        //
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        //
        public static void delete(CoreController core, int recordId) => delete<_BlankModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static void delete(CoreController core, string ccGuid) => delete<_BlankModel>(core, ccGuid);
        //
        //====================================================================================================
        //
        public static List<_BlankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) => createList<_BlankModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        //
        //====================================================================================================
        //
        public static List<_BlankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) => createList<_BlankModel>(core, sqlCriteria, sqlOrderBy);
        //
        //====================================================================================================
        //
        public static List<_BlankModel> createList(CoreController core, string sqlCriteria) => createList<_BlankModel>(core, sqlCriteria);
        //
        //====================================================================================================
        //
        public static void invalidateRecordCache(CoreController core, int recordId) => invalidateRecordCache<_BlankModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static void invalidateTableCache(CoreController core) => invalidateTableCache<_BlankModel>(core);
        //
        //====================================================================================================
        //
        public static string getRecordName(CoreController core, int recordId) => DbModel.getRecordName<_BlankModel>(core, recordId);
        //
        //====================================================================================================
        //
        public static string getRecordName(CoreController core, string ccGuid) => DbModel.getRecordName<_BlankModel>(core, ccGuid);
        //
        //====================================================================================================
        //
        public static int getRecordId(CoreController core, string ccGuid) => DbModel.getRecordId<_BlankModel>(core, ccGuid);
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) => getTableCacheKey<_BlankModel>(core);
        //
        //====================================================================================================
        //
        public static void deleteSelection(CoreController core, string sqlCriteria) => DbModel.deleteSelection<_BlankModel>(core, sqlCriteria);
    }
}
