
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
    public class visitorModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "visitors";
        public const string contentTableName = "ccvisitors";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int MemberID { get; set; }
        public int ForceBrowserMobile { get; set; }
        //
        //====================================================================================================
        public static visitorModel add(CoreController core) {
            return add<visitorModel>(core);
        }
        //
        //====================================================================================================
        public static visitorModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<visitorModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(CoreController core, int recordId) {
            return create<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static visitorModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<visitorModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(CoreController core, string recordGuid) {
            return create<visitorModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static visitorModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<visitorModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(CoreController core, string recordName) {
            return createByName<visitorModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<visitorModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<visitorModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<visitorModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(CoreController core, string sqlCriteria) {
            return createList<visitorModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return baseModel.getRecordName<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return baseModel.getRecordName<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return baseModel.getRecordId<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static visitorModel createDefault(CoreController core) {
            return createDefault<visitorModel>(core);
        }
    }
}
