
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.DbModels {
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
        public static emailQueueModel add(coreController core) {
            return add<emailQueueModel>(core);
        }
        //
        //====================================================================================================
        public static emailQueueModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<emailQueueModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController core, int recordId) {
            return create<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController core, string recordGuid) {
            return create<emailQueueModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(coreController core, string recordName) {
            return createByName<emailQueueModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailQueueModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailQueueModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailQueueModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController core, string sqlCriteria) {
            return createList<emailQueueModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<emailQueueModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<emailQueueModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel createDefault(coreController core) {
            return createDefault<emailQueueModel>(core);
        }
    }
}
