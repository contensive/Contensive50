
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
        public static _blankModel add(coreController core) {
            return add<_blankModel>(core);
        }
        //
        //====================================================================================================
        public static _blankModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<_blankModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController core, int recordId) {
            return create<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<_blankModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController core, string recordGuid) {
            return create<_blankModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static _blankModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<_blankModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(coreController core, string recordName) {
            return createByName<_blankModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<_blankModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<_blankModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<_blankModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(coreController core, string sqlCriteria) {
            return createList<_blankModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static _blankModel createDefault(coreController core) {
            return createDefault<_blankModel>(core);
        }
    }
}
