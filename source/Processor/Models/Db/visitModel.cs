
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
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class VisitModel : BaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "visits";
        public const string contentTableName = "ccvisits";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public bool bot { get; set; }
        public string browser { get; set; }
        
        public bool cookieSupport { get; set; }
        public bool excludeFromAnalytics { get; set; }
        public string http_from { get; set; }
        public string http_referer { get; set; }
        public string http_via { get; set; }
        public DateTime lastVisitTime { get; set; }
        public int loginAttempts { get; set; }
        public int memberID { get; set; }
        public bool memberNew { get; set; }
        public bool mobile { get; set; }
        public int pageVisits { get; set; }
        public string refererPathPage { get; set; }
        public string remote_addr { get; set; }
        public string remoteName { get; set; }
        public int startDateValue { get; set; }
        public DateTime startTime { get; set; }
        public DateTime stopTime { get; set; }
        public int timeToLastHit { get; set; }
        public bool verboseReporting { get; set; }
        public bool visitAuthenticated { get; set; }
        public int visitorID { get; set; }
        public bool visitorNew { get; set; }
        // 
        //====================================================================================================
        public static VisitModel addEmpty(CoreController core) {
            return addEmpty<VisitModel>(core);
        }
        //
        //====================================================================================================
        public static VisitModel addDefault(CoreController core, Domain.MetaModel metaData) {
            return addDefault<VisitModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static VisitModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.MetaModel metaData) {
            return addDefault<VisitModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitModel create(CoreController core, int recordId) {
            return create<VisitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static VisitModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<VisitModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitModel create(CoreController core, string recordGuid) {
            return create<VisitModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static VisitModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<VisitModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static VisitModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<VisitModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static VisitModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<VisitModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<VisitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<VisitModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<VisitModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<VisitModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<VisitModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<VisitModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<VisitModel> createList(CoreController core, string sqlCriteria) {
            return createList<VisitModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateRecordCache<VisitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<VisitModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<VisitModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<VisitModel>(core, ccGuid);
        }
        ////
        ////====================================================================================================
        //public static VisitModel createDefault(CoreController core) {
        //    return createDefault<VisitModel>(core);
        //}
        //
        //====================================================================================================
        /// <summary>
        /// return a visit object for the visitor's last visit before the provided id
        /// </summary>
        /// <param name="core"></param>
        /// <param name="visitId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public static VisitModel getLastVisitByVisitor(CoreController core, int visitId, int visitorId) {
            var visitList = createList(core, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", "id desc");
            if ( visitList.Count>0) {
                return visitList.First();
            }
            return null;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<VisitModel>(core);
        }
    }
}
