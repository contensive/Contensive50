
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email";
        public const string contentTableName = "ccemail";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public DateTime conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public DbModel.FieldTypeTextFile copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public int emailWizardID { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime lastSendTestDate { get; set; }
        public DateTime scheduleDate { get; set; }
        public bool sent { get; set; }
        public DbModel.FieldTypeCSSFile stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
        public bool toAll { get; set; }
        // 
        //====================================================================================================
        public static EmailModel addEmpty(CoreController core) {
            return addEmpty<EmailModel>(core);
        }
        //
        //====================================================================================================
        public static EmailModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<EmailModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static EmailModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<EmailModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailModel create(CoreController core, int recordId) {
            return create<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailModel create(CoreController core, string recordGuid) {
            return create<EmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<EmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<EmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<EmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<EmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<EmailModel>(core);
        }
    }
}
