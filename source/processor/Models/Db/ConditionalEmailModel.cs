
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class ConditionalEmailModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "conditional email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        //
        public DateTime conditionExpireDate { get; set; }
        public int conditionID { get; set; }
        public int conditionPeriod { get; set; }
        public bool sent { get; set; }
        public bool submitted { get; set; }
        //
        //====================================================================================================
        public static ConditionalEmailModel add(CoreController core) {
            return add<ConditionalEmailModel>(core);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<ConditionalEmailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, int recordId) {
            return create<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<ConditionalEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, string recordGuid) {
            return create<ConditionalEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<ConditionalEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel createByName(CoreController core, string recordName) {
            return createByName<ConditionalEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<ConditionalEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<ConditionalEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<ConditionalEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<ConditionalEmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<ConditionalEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<ConditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<ConditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static ConditionalEmailModel createDefault(CoreController core) {
            return createDefault<ConditionalEmailModel>(core);
        }
    }
}
