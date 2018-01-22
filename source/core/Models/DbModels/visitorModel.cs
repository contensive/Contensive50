
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
        public static visitorModel add(coreController core) {
            return add<visitorModel>(core);
        }
        //
        //====================================================================================================
        public static visitorModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<visitorModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreController core, int recordId) {
            return create<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<visitorModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreController core, string recordGuid) {
            return create<visitorModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<visitorModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(coreController core, string recordName) {
            return createByName<visitorModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<visitorModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<visitorModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<visitorModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreController core, string sqlCriteria) {
            return createList<visitorModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<visitorModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<visitorModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static visitorModel createDefault(coreController core) {
            return createDefault<visitorModel>(core);
        }
    }
}
