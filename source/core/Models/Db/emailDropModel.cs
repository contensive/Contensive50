
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class EmailDropModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Email Drops";
        public const string contentTableName = "ccEmailDrops";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int emailID { get; set; }
        //
        //====================================================================================================
        public static EmailDropModel add(CoreController core) {
            return add<EmailDropModel>(core);
        }
        //
        //====================================================================================================
        public static EmailDropModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<EmailDropModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, int recordId) {
            return create<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<EmailDropModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, string recordGuid) {
            return create<EmailDropModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static EmailDropModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<EmailDropModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static EmailDropModel createByName(CoreController core, string recordName) {
            return createByName<EmailDropModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static EmailDropModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<EmailDropModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<EmailDropModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<EmailDropModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<EmailDropModel> createList(CoreController core, string sqlCriteria) {
            return createList<EmailDropModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<EmailDropModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<EmailDropModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static EmailDropModel createDefault(CoreController core) {
            return createDefault<EmailDropModel>(core);
        }
    }
}
