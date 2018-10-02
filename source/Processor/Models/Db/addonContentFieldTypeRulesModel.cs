
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonContentFieldTypeRulesModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on Content Field Type Rules";
        public const string contentTableName = "ccAddonContentFieldTypeRules";
        public const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonID { get; set; }
        public int contentFieldTypeID { get; set; }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel add(CoreController core) {
            return add<AddonContentFieldTypeRulesModel>(core);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonContentFieldTypeRulesModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel create(CoreController core, int recordId) {
            return create<AddonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonContentFieldTypeRulesModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel create(CoreController core, string recordGuid) {
            return create<AddonContentFieldTypeRulesModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonContentFieldTypeRulesModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel createByName(CoreController core, string recordName) {
            return createByName<AddonContentFieldTypeRulesModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonContentFieldTypeRulesModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonContentFieldTypeRulesModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonContentFieldTypeRulesModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonContentFieldTypeRulesModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonContentFieldTypeRulesModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonContentFieldTypeRulesModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonContentFieldTypeRulesModel createDefault(CoreController core) {
            return createDefault<AddonContentFieldTypeRulesModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonContentFieldTypeRulesModel>(core);
        }
    }
}
