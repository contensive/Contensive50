
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.DbModels {
    public class conditionalEmailModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "conditional email";
        public const string contentTableName = "ccemail";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        //
        public DateTime ConditionExpireDate { get; set; }
        public int ConditionID { get; set; }
        public int ConditionPeriod { get; set; }
        public bool Sent { get; set; }
        public bool Submitted { get; set; }
        //
        //====================================================================================================
        public static conditionalEmailModel add(CoreController core) {
            return add<conditionalEmailModel>(core);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<conditionalEmailModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(CoreController core, int recordId) {
            return create<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(CoreController core, string recordGuid) {
            return create<conditionalEmailModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<conditionalEmailModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(CoreController core, string recordName) {
            return createByName<conditionalEmailModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<conditionalEmailModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<conditionalEmailModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<conditionalEmailModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<conditionalEmailModel> createList(CoreController core, string sqlCriteria) {
            return createList<conditionalEmailModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<conditionalEmailModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<conditionalEmailModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static conditionalEmailModel createDefault(CoreController core) {
            return createDefault<conditionalEmailModel>(core);
        }
    }
}
