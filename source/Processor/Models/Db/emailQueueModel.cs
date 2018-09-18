
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailQueueModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email queue";
        public const string contentTableName = "ccEmailQueue";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        //
        public string toAddress { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
        public bool immediate { get; set;  }
        public int attempts { get; set; }
        //
        //====================================================================================================
        public static EmailQueueModel add(CoreController core) {
            return add<EmailQueueModel>(core);
        }
        //
        //====================================================================================================
        public static EmailQueueModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<EmailQueueModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, int recordId) {
            return create<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailQueueModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, string recordGuid) {
            return create<EmailQueueModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailQueueModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailQueueModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailQueueModel createByName(CoreController core, string recordName) {
            return createByName<EmailQueueModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailQueueModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<EmailQueueModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailQueueModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailQueueModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailQueueModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailQueueModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<EmailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<EmailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static EmailQueueModel createDefault(CoreController core) {
            return createDefault<EmailQueueModel>(core);
        }
    }
}
