
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
        public static _blankModel add(CoreController core) {
            return add<_blankModel>(core);
        }
        //
        //====================================================================================================
        public static _blankModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<_blankModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(CoreController core, int recordId) {
            return create<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static _blankModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<_blankModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel create(CoreController core, string recordGuid) {
            return create<_blankModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static _blankModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<_blankModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(CoreController core, string recordName) {
            return createByName<_blankModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static _blankModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<_blankModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<_blankModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<_blankModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<_blankModel> createList(CoreController core, string sqlCriteria) {
            return createList<_blankModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<_blankModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<_blankModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static _blankModel createDefault(CoreController core) {
            return createDefault<_blankModel>(core);
        }
    }
}
