
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
    public class emailQueueModel : baseModel {
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
        public static emailQueueModel add(CoreController core) {
            return add<emailQueueModel>(core);
        }
        //
        //====================================================================================================
        public static emailQueueModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<emailQueueModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(CoreController core, int recordId) {
            return create<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(CoreController core, string recordGuid) {
            return create<emailQueueModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(CoreController core, string recordName) {
            return createByName<emailQueueModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailQueueModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailQueueModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailQueueModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(CoreController core, string sqlCriteria) {
            return createList<emailQueueModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel createDefault(CoreController core) {
            return createDefault<emailQueueModel>(core);
        }
    }
}
