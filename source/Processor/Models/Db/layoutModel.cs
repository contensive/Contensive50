
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Db {
    public class layoutModel : BaseModel {
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
        public static layoutModel add(CoreController core) {
            return add<layoutModel>(core);
        }
        //
        //====================================================================================================
        public static layoutModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<layoutModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(CoreController core, int recordId) {
            return create<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static layoutModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<layoutModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel create(CoreController core, string recordGuid) {
            return create<layoutModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static layoutModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<layoutModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(CoreController core, string recordName) {
            return createByName<layoutModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static layoutModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<layoutModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<layoutModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<layoutModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<layoutModel> createList(CoreController core, string sqlCriteria) {
            return createList<layoutModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<layoutModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<layoutModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static layoutModel createDefault(CoreController core) {
            return createDefault<layoutModel>(core);
        }
    }
}
