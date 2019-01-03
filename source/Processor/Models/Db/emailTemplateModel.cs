
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailTemplateModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email templates";
        public const string contentTableName = "ccemailtemplates";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string bodyHTML { get; set; }
        public string source { get; set; }
        // 
        //====================================================================================================
        public static EmailTemplateModel addEmpty(CoreController core) {
            return addEmpty<EmailTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<EmailTemplateModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<EmailTemplateModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel create(CoreController core, int recordId) {
            return create<EmailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailTemplateModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel create(CoreController core, string recordGuid) {
            return create<EmailTemplateModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailTemplateModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<EmailTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<EmailTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailTemplateModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailTemplateModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailTemplateModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailTemplateModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailTemplateModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<EmailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<EmailTemplateModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<EmailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<EmailTemplateModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<EmailTemplateModel>(core);
        }
    }
}
