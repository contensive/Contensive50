
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class TaskModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tasks";
        public const string contentTableName = "ccTasks";
        public const string contentDataSource = "default";
        /// <summary>
        /// enum of all possible commands in task model
        /// </summary>
        public static class taskQueueCommandEnumModule {
            public const string runAddon = "runaddon";
        }
        /// <summary>
        /// model for cmdDetail field. Field contains a JSON serialization of this class
        /// </summary>
        public class cmdDetailClass {
            public int addonId;
            public string addonName;
            public Dictionary<string, string> args;
        }
        //
        //====================================================================================================
        /// <summary>
        /// JSON serialization of the cmdDetailClass containing information on how to run the task
        /// </summary>
        public string cmdDetail { get; set; }
        /// <summary>
        /// The file where the output of the command is stored. 
        /// </summary>
        public BaseModel.FieldTypeTextFile filename { get; set; }
        /// <summary>
        /// datetime when the task completes
        /// </summary>
        public DateTime dateCompleted { get; set; }
        /// <summary>
        /// datetime when the task is started
        /// </summary>
        public DateTime dateStarted { get; set; }
        //
        // -- deprecated fields. This information should be stored in cmdDetails
        //
        //public string command { get; set; }
        //public string dataSource { get; set; }
        //public string importMapFilename { get; set; }
        //public string notifyEmail { get; set; }
        //public string resultMessage { get; set; }
        //public string sqlQuery { get; set; }
        //
        //====================================================================================================
        public static TaskModel add(CoreController core) {
            return add<TaskModel>(core);
        }
        //
        //====================================================================================================
        public static TaskModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<TaskModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TaskModel create(CoreController core, int recordId) {
            return create<TaskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static TaskModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<TaskModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TaskModel create(CoreController core, string recordGuid) {
            return create<TaskModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static TaskModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<TaskModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static TaskModel createByName(CoreController core, string recordName) {
            return createByName<TaskModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TaskModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<TaskModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<TaskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<TaskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<TaskModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<TaskModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<TaskModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<TaskModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<TaskModel> createList(CoreController core, string sqlCriteria) {
            return createList<TaskModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<TaskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<TaskModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<TaskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<TaskModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static TaskModel createDefault(CoreController core) {
            return createDefault<TaskModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<TaskModel>(core);
        }
    }
}
