﻿
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
    public class visitModel : BaseModel {
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
        public static visitModel add(CoreController core) {
            return add<visitModel>(core);
        }
        //
        //====================================================================================================
        public static visitModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<visitModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel create(CoreController core, int recordId) {
            return create<visitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static visitModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<visitModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel create(CoreController core, string recordGuid) {
            return create<visitModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static visitModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<visitModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static visitModel createByName(CoreController core, string recordName) {
            return createByName<visitModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static visitModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<visitModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<visitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<visitModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<visitModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<visitModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<visitModel> createList(CoreController core, string sqlCriteria) {
            return createList<visitModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<visitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<visitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<visitModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<visitModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static visitModel createDefault(CoreController core) {
            return createDefault<visitModel>(core);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a visit object for the visitor's last visit before the provided id
        /// </summary>
        /// <param name="core"></param>
        /// <param name="visitId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public static visitModel getLastVisitByVisitor(CoreController core, int visitId, int visitorId) {
            var visitList = createList(core, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", "id desc");
            if ( visitList.Count>0) {
                return visitList.First();
            }
            return null;
        }
    }
}