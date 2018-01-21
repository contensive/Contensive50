
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
    public class _blankModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "tables";             //<------ set content name
        public const string contentTableName = "ccTables";      //<------ set to tablename for the primary content (used for cache names)
        private const string contentDataSource = "default";     //<------ set to datasource if not default
        //
        //====================================================================================================
        // -- instance properties
        public int DataSourceID { get; set; }                   //<------ replace this with a list all model fields not part of the base model
        //
        //====================================================================================================
        public static _blankModel add(coreController cpCore) {
            return add<_blankModel>(cpCore);
        }
        //
        //====================================================================================================
        public static _blankModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<_blankModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController cpCore, int recordId) {
            return create<_blankModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<_blankModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController cpCore, string recordGuid) {
            return create<_blankModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<_blankModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(coreController cpCore, string recordName) {
            return createByName<_blankModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<_blankModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<_blankModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<_blankModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<_blankModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<_blankModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<_blankModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<_blankModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<_blankModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<_blankModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<_blankModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static _blankModel createDefault(coreController cpcore) {
            return createDefault<_blankModel>(cpcore);
        }
    }
}
