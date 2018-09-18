
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool addLinkEID { get; set; }
        public bool allowSpamFooter { get; set; }
        public bool blockSiteStyles { get; set; }
        public DateTime conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public BaseModel.fieldTypeTextFile copyFilename { get; set; }
        public int emailTemplateID { get; set; }
        public int emailWizardID { get; set; }
        public string fromAddress { get; set; }
        public string inlineStyles { get; set; }
        public DateTime lastSendTestDate { get; set; }
        public DateTime scheduleDate { get; set; }
        public bool sent { get; set; }
        public BaseModel.fieldTypeCSSFile stylesFilename { get; set; }
        public string subject { get; set; }
        public bool submitted { get; set; }
        public int testMemberID { get; set; }
        public bool toAll { get; set; }
        //
        //====================================================================================================
        public static EmailModel add(CoreController core) {
            return add<EmailModel>(core);
        }
        //
        //====================================================================================================
        public static EmailModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<EmailModel>(core, ref callersCacheNameList);
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
        public static EmailModel createByName(CoreController core, string recordName) {
            return createByName<EmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<EmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
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
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<EmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<EmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<EmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static EmailModel createDefault(CoreController core) {
            return createDefault<EmailModel>(core);
        }
    }
}
