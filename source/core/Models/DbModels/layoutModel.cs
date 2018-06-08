
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
    public class layoutModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "layouts";
        public const string contentTableName = "ccLayouts";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string Layout { get; set; }
        public string StylesFilename { get; set; }
        //
        //====================================================================================================
        public static layoutModel add(coreController core) {
            return add<layoutModel>(core);
        }
        //
        //====================================================================================================
        public static layoutModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<layoutModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController core, int recordId) {
            return create<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<layoutModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController core, string recordGuid) {
            return create<layoutModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static layoutModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<layoutModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(coreController core, string recordName) {
            return createByName<layoutModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<layoutModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<layoutModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<layoutModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(coreController core, string sqlCriteria) {
            return createList<layoutModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static layoutModel createDefault(coreController core) {
            return createDefault<layoutModel>(core);
        }
    }
}
