
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
    public class visitModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "visits";
        public const string contentTableName = "ccvisits";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public bool Bot { get; set; }
        public string Browser { get; set; }
        public int ContentCategoryID { get; set; }
        public bool CookieSupport { get; set; }
        public bool ExcludeFromAnalytics { get; set; }
        public string HTTP_FROM { get; set; }
        public string HTTP_REFERER { get; set; }
        public string HTTP_VIA { get; set; }
        public DateTime LastVisitTime { get; set; }
        public int LoginAttempts { get; set; }
        public int MemberID { get; set; }
        public bool MemberNew { get; set; }
        public bool Mobile { get; set; }
        public int PageVisits { get; set; }
        public string RefererPathPage { get; set; }
        public string REMOTE_ADDR { get; set; }
        public string RemoteName { get; set; }
        public int StartDateValue { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public int TimeToLastHit { get; set; }
        public bool VerboseReporting { get; set; }
        public bool VisitAuthenticated { get; set; }
        public int VisitorID { get; set; }
        public bool VisitorNew { get; set; }
        //
        //====================================================================================================
        public static visitModel add(coreClass cpCore) {
            return add<visitModel>(cpCore);
        }
        //
        //====================================================================================================
        public static visitModel add(coreClass cpCore, ref List<string> callersCacheNameList) {
            return add<visitModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel create(coreClass cpCore, int recordId) {
            return create<visitModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static visitModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<visitModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel create(coreClass cpCore, string recordGuid) {
            return create<visitModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static visitModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<visitModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel createByName(coreClass cpCore, string recordName) {
            return createByName<visitModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static visitModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<visitModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreClass cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, int recordId) {
            delete<visitModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreClass cpCore, string ccGuid) {
            delete<visitModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<visitModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(coreClass cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<visitModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(coreClass cpCore, string sqlCriteria) {
            return createList<visitModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            invalidateCacheSingleRecord<visitModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, int recordId) {
            return baseModel.getRecordName<visitModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordName<visitModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreClass cpcore, string ccGuid) {
            return baseModel.getRecordId<visitModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static visitModel createDefault(coreClass cpcore) {
            return createDefault<visitModel>(cpcore);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a visit object for the visitor's last visit before the provided id
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="visitId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public static visitModel getLastVisitByVisitor(coreClass cpCore, int visitId, int visitorId) {
            var visitList = createList(cpCore, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", "id desc");
            if ( visitList.Count>0) {
                return visitList.First();
            }
            return null;
        }
    }
}
