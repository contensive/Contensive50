
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Entity {
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
        public static visitorModel add(coreClass cpCore) {
            return add<visitorModel>(cpCore);
        }
        //
        //====================================================================================================
        public static visitorModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<visitorModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreClass cpCore, int recordId) {
            return create<visitorModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<visitorModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreClass cpCore, string recordGuid) {
            return create<visitorModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static visitorModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<visitorModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(coreClass cpCore, string recordName) {
            return createByName<visitorModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static visitorModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<visitorModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<visitorModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<visitorModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<visitorModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<visitorModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<visitorModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<visitorModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<visitorModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<visitorModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<visitorModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<visitorModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static visitorModel createDefault(coreClass cpcore) {
            return createDefault<visitorModel>(cpcore);
        }
    }
}
