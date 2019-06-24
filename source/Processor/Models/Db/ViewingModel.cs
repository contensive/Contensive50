
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    [Serializable]
    public class ViewingModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const (must be const not property)
        /// <summary>
        /// The content metadata name for this table
        /// </summary>
        public const string contentName = "viewings";
        /// <summary>
        /// The sql server table name
        /// </summary>
        public const string contentTableNameLowerCase = "ccviewings";
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
        /// <summary>
        /// do not count this record in analytics, for example if it for a background process, or an administrator visit
        /// </summary>
        public bool excludeFromAnalytics { get; set; }
        public string form { get; set; }
        public string host { get; set; }
        public int memberID { get; set; }
        public string page { get; set; }
        public int pageTime { get; set; }
        public string pageTitle { get; set; }
        public string path { get; set; }
        public string queryString { get; set; }
        public int recordID { get; set; }
        public string referer { get; set; }
        public bool stateOK { get; set; }
        public int visitID { get; set; }
        public int visitorID { get; set; }
        //
        // ##### NOTE: deprecate these after we verify the best practice is to call methods from DbModel, like DbModel.create<PersonModel>( core, 1 );
        ////
        ////
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel addEmpty(CoreController core) => addEmpty<ViewingModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel addDefault(CoreController core, Domain.MetaModel metaData) => addDefault<ViewingModel>(core, metaData);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) => addDefault<ViewingModel>(core, metaData, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel create(CoreController core, int recordId) => create<ViewingModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) => create<ViewingModel>(core, recordId, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel create(CoreController core, string recordGuid) => create<ViewingModel>(core, recordGuid);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) => create<ViewingModel>(core, recordGuid, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static ViewingModel createByUniqueName(CoreController core, string recordName) => createByUniqueName<ViewingModel>(core, recordName);
        ////
        ////====================================================================================================
        //public static ViewingModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) => createByUniqueName<ViewingModel>(core, recordName, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, int recordId) => delete<ViewingModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, string ccGuid) => delete<ViewingModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static List<ViewingModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) => createList<ViewingModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static List<ViewingModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) => createList<ViewingModel>(core, sqlCriteria, sqlOrderBy);
        ////
        ////====================================================================================================
        ////
        //public static List<ViewingModel> createList(CoreController core, string sqlCriteria) => createList<ViewingModel>(core, sqlCriteria);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateRecordCache(CoreController core, int recordId) => invalidateRecordCache<ViewingModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateTableCache(CoreController core) => invalidateTableCache<ViewingModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, int recordId) => DbModel.getRecordName<ViewingModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, string ccGuid) => DbModel.getRecordName<ViewingModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static int getRecordId(CoreController core, string ccGuid) => DbModel.getRecordId<ViewingModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ///// <summary>
        ///// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        ///// </summary>
        ///// <param name="core"></param>
        ///// <returns></returns>
        //public static string getTableInvalidationKey(CoreController core) => getTableCacheKey<ViewingModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static void deleteSelection(CoreController core, string sqlCriteria) => DbModel.deleteSelection<ViewingModel>(core, sqlCriteria);
    }
}
