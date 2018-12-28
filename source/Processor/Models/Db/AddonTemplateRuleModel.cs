
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class AddonTemplateRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on template rules";
        public const string contentTableName = "ccAddonTemplateRules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int templateId { get; set; }
        // 
        //====================================================================================================
        public static AddonTemplateRuleModel addEmpty(CoreController core) {
            return addEmpty<AddonTemplateRuleModel>(core);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel addDefault(CoreController core, Domain.ContentMetaDomainModel cdef) {
            return addDefault<AddonTemplateRuleModel>(core, cdef);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetaDomainModel cdef) {
            return addDefault<AddonTemplateRuleModel>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, int recordId) {
            return create<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<AddonTemplateRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, string recordGuid) {
            return create<AddonTemplateRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<AddonTemplateRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<AddonTemplateRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static AddonTemplateRuleModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<AddonTemplateRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<AddonTemplateRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<AddonTemplateRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<AddonTemplateRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<AddonTemplateRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        //public static AddonTemplateRuleModel createDefault(CoreController core) {
        //    return createDefault<AddonTemplateRuleModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<AddonTemplateRuleModel>(core);
        }
    }
}
