
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
    public class emailLogModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email log";
        public const string contentTableName = "ccEmailLog";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public DateTime DateBlockExpires { get; set; }
        public int EmailDropID { get; set; }
        public int EmailID { get; set; }
        public string FromAddress { get; set; }
        public int LogType { get; set; }
        public int MemberID { get; set; }
        public string SendStatus { get; set; }
        public string Subject { get; set; }
        public string ToAddress { get; set; }
        public int VisitID { get; set; }
        //
        //====================================================================================================
        public static emailLogModel add(coreController cpCore) {
            return add<emailLogModel>(cpCore);
        }
        //
        //====================================================================================================
        public static emailLogModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<emailLogModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController cpCore, int recordId) {
            return create<emailLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<emailLogModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController cpCore, string recordGuid) {
            return create<emailLogModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailLogModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel createByName(coreController cpCore, string recordName) {
            return createByName<emailLogModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static emailLogModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailLogModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<emailLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<emailLogModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailLogModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<emailLogModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<emailLogModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<emailLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<emailLogModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<emailLogModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<emailLogModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static emailLogModel createDefault(coreController cpcore) {
            return createDefault<emailLogModel>(cpcore);
        }
    }
}
