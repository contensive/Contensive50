
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class MenuPageRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const (must be const not property)
        /// <summary>
        /// The content metadata name for this table
        /// </summary>
        public const string contentName = "Menu Page Rules";
        /// <summary>
        /// The sql server table name
        /// </summary>
        public const string contentTableNameLowerCase = "ccmenupagerules";
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
        public int menuId { get; set; }
        public int pageId { get; set; }
        //
        // ##### NOTE: deprecate these after we verify the best practice is to call methods from DbModel, like DbModel.create<PersonModel>( core, 1 );
        ////
        ////
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel addEmpty(CoreController core) => addEmpty<MenuPageRuleModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel addDefault(CoreController core, Domain.MetaModel metaData) => addDefault<MenuPageRuleModel>(core, metaData);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) => addDefault<MenuPageRuleModel>(core, metaData, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel create(CoreController core, int recordId) => create<MenuPageRuleModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) => create<MenuPageRuleModel>(core, recordId, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel create(CoreController core, string recordGuid) => create<MenuPageRuleModel>(core, recordGuid);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) => create<MenuPageRuleModel>(core, recordGuid, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static MenuPageRuleModel createByUniqueName(CoreController core, string recordName) => createByUniqueName<MenuPageRuleModel>(core, recordName);
        ////
        ////====================================================================================================
        //public static MenuPageRuleModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) => createByUniqueName<MenuPageRuleModel>(core, recordName, ref callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, int recordId) => delete<MenuPageRuleModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void delete(CoreController core, string ccGuid) => delete<MenuPageRuleModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static List<MenuPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) => createList<MenuPageRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        ////
        ////====================================================================================================
        ////
        //public static List<MenuPageRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) => createList<MenuPageRuleModel>(core, sqlCriteria, sqlOrderBy);
        ////
        ////====================================================================================================
        ////
        //public static List<MenuPageRuleModel> createList(CoreController core, string sqlCriteria) => createList<MenuPageRuleModel>(core, sqlCriteria);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateRecordCache(CoreController core, int recordId) => invalidateRecordCache<MenuPageRuleModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static void invalidateTableCache(CoreController core) => invalidateTableCache<MenuPageRuleModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, int recordId) => DbModel.getRecordName<MenuPageRuleModel>(core, recordId);
        ////
        ////====================================================================================================
        ////
        //public static string getRecordName(CoreController core, string ccGuid) => DbModel.getRecordName<MenuPageRuleModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ////
        //public static int getRecordId(CoreController core, string ccGuid) => DbModel.getRecordId<MenuPageRuleModel>(core, ccGuid);
        ////
        ////====================================================================================================
        ///// <summary>
        ///// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        ///// </summary>
        ///// <param name="core"></param>
        ///// <returns></returns>
        //public static string getTableInvalidationKey(CoreController core) => getTableCacheKey<MenuPageRuleModel>(core);
        ////
        ////====================================================================================================
        ////
        //public static void deleteSelection(CoreController core, string sqlCriteria) => DbModel.deleteSelection<MenuPageRuleModel>(core, sqlCriteria);
    }
}
