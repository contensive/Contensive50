
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
    public class libraryFileLogModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library File log";
        public const string contentTableName = "cclibraryFileLog";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int FileID { get; set; }
        public int MemberID { get; set; }
        public int VisitID { get; set; }
        //
        //====================================================================================================
        public static libraryFileLogModel add(CoreController core) {
            return add<libraryFileLogModel>(core);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<libraryFileLogModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(CoreController core, int recordId) {
            return create<libraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<libraryFileLogModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(CoreController core, string recordGuid) {
            return create<libraryFileLogModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<libraryFileLogModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createByName(CoreController core, string recordName) {
            return createByName<libraryFileLogModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<libraryFileLogModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<libraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<libraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<libraryFileLogModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<libraryFileLogModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(CoreController core, string sqlCriteria) {
            return createList<libraryFileLogModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<libraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<libraryFileLogModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<libraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<libraryFileLogModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createDefault(CoreController core) {
            return createDefault<libraryFileLogModel>(core);
        }
    }
}
