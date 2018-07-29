
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
    public class groupModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "groups";
        public const string contentTableName = "ccgroups";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool AllowBulkEmail { get; set; }
        public string Caption { get; set; }
        
        public string CopyFilename { get; set; }
        public bool PublicJoin { get; set; }
        //
        //====================================================================================================
        public static groupModel add(CoreController core) {
            return add<groupModel>(core);
        }
        //
        //====================================================================================================
        public static groupModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<groupModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupModel create(CoreController core, int recordId) {
            return create<groupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static groupModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<groupModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupModel create(CoreController core, string recordGuid) {
            return create<groupModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static groupModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<groupModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static groupModel createByName(CoreController core, string recordName) {
            return createByName<groupModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static groupModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<groupModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<groupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<groupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<groupModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<groupModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<groupModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<groupModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<groupModel> createList(CoreController core, string sqlCriteria) {
            return createList<groupModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCacheSingleRecord<groupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<groupModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<groupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<groupModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static groupModel createDefault(CoreController core) {
            return createDefault<groupModel>(core);
        }
    }
}
