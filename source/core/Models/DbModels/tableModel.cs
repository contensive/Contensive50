
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
    public class tableModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tables"; //<------ set content name
        public const string contentTableName = "ccTables"; //<------ set to tablename for the primary content (used for cache names)
        private const string contentDataSource = "default"; //<----- set to datasource if not default
        //
        //====================================================================================================
        // -- instance properties
        public int DataSourceID { get; set; }
        //
        //====================================================================================================
        public static tableModel add(coreController cpCore) {
            return add<tableModel>(cpCore);
        }
        //
        //====================================================================================================
        public static tableModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<tableModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static tableModel create(coreController cpCore, int recordId) {
            return create<tableModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static tableModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<tableModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static tableModel create(coreController cpCore, string recordGuid) {
            return create<tableModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static tableModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<tableModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static tableModel createByName(coreController cpCore, string recordName) {
            return createByName<tableModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static tableModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<tableModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<tableModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<tableModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<tableModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<tableModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<tableModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<tableModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<tableModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<tableModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<tableModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<tableModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<tableModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<tableModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static tableModel createDefault(coreController cpcore) {
            return createDefault<tableModel>(cpcore);
        }
    }
}
