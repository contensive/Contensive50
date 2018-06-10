
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
    public class taskModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tasks";
        public const string contentTableName = "ccTasks";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        //
        public string Command { get; set; }
        public string DataSource { get; set; }
        public DateTime DateCompleted { get; set; }
        public DateTime DateStarted { get; set; }
        public string Filename { get; set; }
        public string ImportMapFilename { get; set; }
        public string NotifyEmail { get; set; }
        public string ResultMessage { get; set; }
        public string SQLQuery { get; set; }
        public string cmdDetail { get; set; }
        //
        //====================================================================================================
        public static taskModel add(coreController core) {
            return add<taskModel>(core);
        }
        //
        //====================================================================================================
        public static taskModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<taskModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static taskModel create(coreController core, int recordId) {
            return create<taskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static taskModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<taskModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static taskModel create(coreController core, string recordGuid) {
            return create<taskModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static taskModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<taskModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static taskModel createByName(coreController core, string recordName) {
            return createByName<taskModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static taskModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<taskModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<taskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<taskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<taskModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<taskModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<taskModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<taskModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<taskModel> createList(coreController core, string sqlCriteria) {
            return createList<taskModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<taskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<taskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<taskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<taskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static taskModel createDefault(coreController core) {
            return createDefault<taskModel>(core);
        }
    }
}
