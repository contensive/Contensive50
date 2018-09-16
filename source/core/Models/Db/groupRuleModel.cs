
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class GroupRuleModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group rules";
        public const string contentTableName = "ccgrouprules";
        private const string contentDataSource = "default"; 
        //
        //====================================================================================================
        // -- instance properties
        public bool allowAdd { get; set; }
        public bool allowDelete { get; set; }        
        public int contentID { get; set; }
        public int groupID { get; set; }
        //
        //====================================================================================================
        public static GroupRuleModel add(CoreController core) {
            return add<GroupRuleModel>(core);
        }
        //
        //====================================================================================================
        public static GroupRuleModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<GroupRuleModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, int recordId) {
            return create<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<GroupRuleModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, string recordGuid) {
            return create<GroupRuleModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static GroupRuleModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<GroupRuleModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static GroupRuleModel createByName(CoreController core, string recordName) {
            return createByName<GroupRuleModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static GroupRuleModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<GroupRuleModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<GroupRuleModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<GroupRuleModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<GroupRuleModel> createList(CoreController core, string sqlCriteria) {
            return createList<GroupRuleModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<GroupRuleModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<GroupRuleModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static GroupRuleModel createDefault(CoreController core) {
            return createDefault<GroupRuleModel>(core);
        }
    }
}
