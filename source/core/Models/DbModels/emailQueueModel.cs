
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
        public static emailQueueModel add(coreController cpCore) {
            return add<emailQueueModel>(cpCore);
        }
        //
        //====================================================================================================
        public static emailQueueModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<emailQueueModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController cpCore, int recordId) {
            return create<emailQueueModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController cpCore, string recordGuid) {
            return create<emailQueueModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailQueueModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(coreController cpCore, string recordName) {
            return createByName<emailQueueModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static emailQueueModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailQueueModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<emailQueueModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<emailQueueModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailQueueModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<emailQueueModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailQueueModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<emailQueueModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<emailQueueModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<emailQueueModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<emailQueueModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<emailQueueModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static emailQueueModel createDefault(coreController cpcore) {
            return createDefault<emailQueueModel>(cpcore);
        }
    }
}
