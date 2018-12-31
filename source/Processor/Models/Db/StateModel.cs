
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class StateModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "states";           
        public const string contentTableName = "ccstates";     
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string abbreviation { get; set; }
        public int countryID { get; set; }
        public double salesTax { get; set; }
        //
        // todo -- add addEmpty to all models
        //====================================================================================================
        public static StateModel addEmpty(CoreController core) {
            return addEmpty<StateModel>(core);
        }
        //
        //====================================================================================================
        public static StateModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<StateModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static StateModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<StateModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static StateModel create(CoreController core, int recordId) {
            return create<StateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static StateModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<StateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static StateModel create(CoreController core, string recordGuid) {
            return create<StateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static StateModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<StateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static StateModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<StateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static StateModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<StateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<StateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<StateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<StateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<StateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<StateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<StateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<StateModel> createList(CoreController core, string sqlCriteria) {
            return createList<StateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<StateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateTableCache<StateModel>(core);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<StateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<StateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<StateModel>(core, ccGuid);
        }
        ////
        ////====================================================================================================
        //public static StateModel createDefault(CoreController core) {
        //    return createDefault<StateModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<StateModel>(core);
        }
    }
}
