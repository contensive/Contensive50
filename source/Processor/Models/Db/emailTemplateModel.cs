
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
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string bodyHTML { get; set; }
        public string source { get; set; }
        //
        //====================================================================================================
        public static EmailTemplateModel add(CoreController core) {
            return add<EmailTemplateModel>(core);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<EmailTemplateModel>(core, ref callersCacheNameList);
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
        public static EmailTemplateModel createByName(CoreController core, string recordName) {
            return createByName<EmailTemplateModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailTemplateModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<EmailTemplateModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
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
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<EmailTemplateModel>(core, recordId);
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
        public static EmailTemplateModel createDefault(CoreController core) {
            return createDefault<EmailTemplateModel>(core);
        }
    }
}
