
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class _BlankModel : DbModel {
        //
        //====================================================================================================
        //-- const (must be const not property)
        /// <summary>
        /// The content metadata name for this table
        /// </summary>
        public const string contentName = "tables";
        /// <summary>
        /// The sql server table name
        /// </summary>
        public const string contentTableName = "ccTables";
        /// <summary>
        /// The Contensive datasource. Use "default" or blank for the default datasource stored in the server config file
        /// </summary>
        public const string contentDataSource = "default";
        /// <summary>
        /// set true if the name field's value for all records must be unique (no duplicates). Used for cache ptr generation
        /// </summary>
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties (must be properties not fields)
        //
        public int ReplaceMeWithThisTablesFields { get; set; }
        //
        //
        // ##### NOTE: deprecate these after we verify the best practice is to call methods from DbModel, like DbModel.create<PersonModel>( core, 1 );
        ////
        ////
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel addEmpty(CoreController core) => addEmpty<_BlankModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel addDefault(CoreController core, Domain.MetaModel metaData) => addDefault<_BlankModel>(core, metaData);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) => addDefault<_BlankModel>(core, metaData, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel create(CoreController core, int recordId) => create<_BlankModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) => create<_BlankModel>(core, recordId, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel create(CoreController core, string recordGuid) => create<_BlankModel>(core, recordGuid);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) => create<_BlankModel>(core, recordGuid, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static _BlankModel createByUniqueName(CoreController core, string recordName) => createByUniqueName<_BlankModel>(core, recordName);
        ////
        ////====================================================================================================
        //public static _BlankModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) => createByUniqueName<_BlankModel>(core, recordName, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, int recordId) => delete<_BlankModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, string ccGuid) => delete<_BlankModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static List<_BlankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) => createList<_BlankModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static List<_BlankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) => createList<_BlankModel>(core, sqlCriteria, sqlOrderBy);
        ////
        ////====================================================================================================
        ////
        //public static List<_BlankModel> createList(CoreController core, string sqlCriteria) => createList<_BlankModel>(core, sqlCriteria);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateRecordCache(CoreController core, int recordId) => invalidateRecordCache<_BlankModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateTableCache(CoreController core) => invalidateTableCache<_BlankModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, int recordId) => DbModel.getRecordName<_BlankModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, string ccGuid) => DbModel.getRecordName<_BlankModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static int getRecordId(CoreController core, string ccGuid) => DbModel.getRecordId<_BlankModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ///// <summary>
        ///// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        ///// </summary>
        ///// <param name="core"></param>
        ///// <returns></returns>
        //public static string getTableInvalidationKey(CoreController core) => getTableCacheKey<_BlankModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static void deleteSelection(CoreController core, string sqlCriteria) => DbModel.deleteSelection<_BlankModel>(core, sqlCriteria);
    }
}
