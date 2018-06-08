
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
        public string body { get; set; }
        //
        //====================================================================================================
        public static emailLogModel add(coreController core) {
            return add<emailLogModel>(core);
        }
        //
        //====================================================================================================
        public static emailLogModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<emailLogModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController core, int recordId) {
            return create<emailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<emailLogModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController core, string recordGuid) {
            return create<emailLogModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static emailLogModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailLogModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailLogModel createByName(coreController core, string recordName) {
            return createByName<emailLogModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static emailLogModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailLogModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<emailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<emailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailLogModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<emailLogModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailLogModel> createList(coreController core, string sqlCriteria) {
            return createList<emailLogModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<emailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<emailLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<emailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<emailLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static emailLogModel createDefault(coreController core) {
            return createDefault<emailLogModel>(core);
        }
    }
}
