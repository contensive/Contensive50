
using System.Collections.Generic;
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Models.Db {
    public class AddonIncludeRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Add-on Include Rules";
        public const string contentTableName = "ccAddonIncludeRules";
        public const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int addonID { get; set; }
        public int includedAddonID { get; set; }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel add(CoreController core) {
            return add<AddonIncludeRuleModel>(core);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<AddonIncludeRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel create(CoreController core, int recordId) {
            return create<AddonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonIncludeRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel create(CoreController core, string recordGuid) {
            return create<AddonIncludeRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonIncludeRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel createByName(CoreController core, string recordName) {
            return createByName<AddonIncludeRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<AddonIncludeRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonIncludeRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonIncludeRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonIncludeRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonIncludeRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonIncludeRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonIncludeRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonIncludeRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static AddonIncludeRuleModel createDefault(CoreController core) {
            return createDefault<AddonIncludeRuleModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonIncludeRuleModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a dictionary of addonId each with a list of included addon Ids
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static  Dictionary<int, List<int>> getIncludeRuleDict( CoreController core) {
            var result = new Dictionary<int, List<int>>();
            foreach ( AddonIncludeRuleModel addonRule  in createList( core , "" )) {
                if (!result.ContainsKey(addonRule.addonID)) {
                    result.Add(addonRule.addonID, new List<int>() { addonRule.includedAddonID });
                } else {
                    result[addonRule.addonID].Add(addonRule.includedAddonID);
                }
            }
            return result;
        }
    }
}
