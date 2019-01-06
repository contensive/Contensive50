
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class TaskModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tasks";
        public const string contentTableName = "ccTasks";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        /// <summary>
        /// enum of all possible commands in task model
        /// </summary>
        public static class taskQueueCommandEnumModule {
            public const string runAddon = "runaddon";
        }
        /// <summary>
        /// model for cmdDetail field. Field contains a JSON serialization of this class
        /// </summary>
        public class CmdDetailClass {
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
        /// <summary>
        /// if 0, this field is ignore. if non-zero, the output from the task is saved to the download record with this id.
        /// To save a tasks output, create a download record and set this field with its id.
        /// </summary>
        public int resultDownloadId { get; set; }
        //
        //====================================================================================================
        public static TaskModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<TaskModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static TaskModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<TaskModel>(core, metaData, ref callersCacheNameList);
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
        public static TaskModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<TaskModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static TaskModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<TaskModel>(core, recordName, ref callersCacheNameList);
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
