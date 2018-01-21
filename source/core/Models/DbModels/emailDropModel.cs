
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
    public class emailDropModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Email Drops";
        public const string contentTableName = "ccEmailDrops";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int EmailID { get; set; }
        //
        //====================================================================================================
        public static emailDropModel add(coreController cpCore) {
            return add<emailDropModel>(cpCore);
        }
        //
        //====================================================================================================
        public static emailDropModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<emailDropModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController cpCore, int recordId) {
            return create<emailDropModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController cpCore, string recordGuid) {
            return create<emailDropModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<emailDropModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(coreController cpCore, string recordName) {
            return createByName<emailDropModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static emailDropModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<emailDropModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<emailDropModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<emailDropModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<emailDropModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<emailDropModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<emailDropModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<emailDropModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<emailDropModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<emailDropModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<emailDropModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<emailDropModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static emailDropModel createDefault(coreController cpcore) {
            return createDefault<emailDropModel>(cpcore);
        }
    }
}
