
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
        public static libraryFileLogModel add(coreController cpCore) {
            return add<libraryFileLogModel>(cpCore);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<libraryFileLogModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(coreController cpCore, int recordId) {
            return create<libraryFileLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<libraryFileLogModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(coreController cpCore, string recordGuid) {
            return create<libraryFileLogModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<libraryFileLogModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createByName(coreController cpCore, string recordName) {
            return createByName<libraryFileLogModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<libraryFileLogModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<libraryFileLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<libraryFileLogModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<libraryFileLogModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<libraryFileLogModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<libraryFileLogModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<libraryFileLogModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<libraryFileLogModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<libraryFileLogModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<libraryFileLogModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<libraryFileLogModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static libraryFileLogModel createDefault(coreController cpcore) {
            return createDefault<libraryFileLogModel>(cpcore);
        }
    }
}
