
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
    public class linkForwardModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link forwards";
        public const string contentTableName = "cclinkforwards";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public string DestinationLink;
        public int GroupID;
        public string SourceLink;
        public int Viewings;
        //
        //====================================================================================================
        public static linkForwardModel add(CoreController core) {
            return add<linkForwardModel>(core);
        }
        //
        //====================================================================================================
        public static linkForwardModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<linkForwardModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(CoreController core, int recordId) {
            return create<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(CoreController core, string recordGuid) {
            return create<linkForwardModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<linkForwardModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(CoreController core, string recordName) {
            return createByName<linkForwardModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static linkForwardModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<linkForwardModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<linkForwardModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<linkForwardModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<linkForwardModel> createList(CoreController core, string sqlCriteria) {
            return createList<linkForwardModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<linkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<linkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static linkForwardModel createDefault(CoreController core) {
            return createDefault<linkForwardModel>(core);
        }
    }
}
