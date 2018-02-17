
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
    public class visitModelx {
        //
        //-- const
        public const string primaryContentName = "visits";
        private const string primaryContentTableName = "ccvisits";
        private const string primaryContentDataSource = "default";
        //
        // -- instance properties
        public int id;
        public bool Active;
        public bool Bot;
        public string Browser;
        public string ccGuid;
        public int ContentControlID;
        public bool CookieSupport;
        public int CreatedBy;
        public int CreateKey;
        public DateTime DateAdded;
        public bool ExcludeFromAnalytics;
        public string HTTP_FROM;
        public string HTTP_REFERER;
        public string HTTP_VIA;
        public DateTime LastVisitTime;
        public int LoginAttempts;
        public int MemberID;
        public bool MemberNew;
        public bool Mobile;
        public int ModifiedBy;
        public DateTime ModifiedDate;
        public string Name;
        public int PageVisits;
        public string RefererPathPage;
        public string REMOTE_ADDR;
        public string RemoteName;
        public string SortOrder;
        public int StartDateValue;
        public DateTime StartTime;
        public DateTime StopTime;
        public int TimeToLastHit;
        public bool VerboseReporting;
        public bool VisitAuthenticated;
        public int VisitorID;
        public bool VisitorNew;
        //
        // -- publics not exposed to the UI (test/internal data)
        //<JsonIgnore> Public createKey As Integer
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization
        /// </summary>
        public visitModelx() {}
        //
        //====================================================================================================
        /// <summary>
        /// add a new recod to the db and open it. Starting a new model with this method will use the default
        /// values in Contensive metadata (active, contentcontrolid, etc)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="cacheNameList"></param>
        /// <returns></returns>
        public static visitModelx add(coreController core, ref List<string> cacheNameList) {
            visitModelx result = null;
            try {
                result = create(core, core.db.insertContentRecordGetID(primaryContentName, 0), ref cacheNameList);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        public static visitModelx add(coreController core) { var tmpList = new List<string> { }; return add(core, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public static visitModelx create(coreController core, int recordId, ref List<string> cacheNameList) {
            visitModelx result = null;
            try {
                if (recordId > 0) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
                    result = core.cache.getObject<visitModelx>(cacheName);
                    if (result == null) {
                        result = loadObject(core, "id=" + recordId.ToString(), ref cacheNameList);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        public static visitModelx create(coreController core, int recordId) { var tmpList = new List<string> { }; return create(core, recordId, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// open an existing object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        public static visitModelx create(coreController core, string recordGuid, ref List<string> cacheNameList) {
            visitModelx result = null;
            try {
                if (!string.IsNullOrEmpty(recordGuid)) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
                    result = core.cache.getObject<visitModelx>(cacheName);
                    if (result == null) {
                        result = loadObject(core, "ccGuid=" + core.db.encodeSQLText(recordGuid), ref cacheNameList);
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// open an existing object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        private static visitModelx loadObject(coreController core, string sqlCriteria, ref List<string> cacheNameList, string sortOrder = "id") {
            visitModelx result = null;
            try {
                csController cs = new csController(core);
                if (cs.open(primaryContentName, sqlCriteria, sortOrder)) {
                    result = new visitModelx();
                    //
                    // -- populate result model
                    result.id = cs.getInteger("ID");
                    result.Active = cs.getBoolean("Active");
                    result.Bot = cs.getBoolean("Bot");
                    result.Browser = cs.getText("Browser");
                    result.ccGuid = cs.getText("ccGuid");
                    //
                    result.ContentControlID = cs.getInteger("ContentControlID");
                    result.CookieSupport = cs.getBoolean("CookieSupport");
                    result.CreatedBy = cs.getInteger("CreatedBy");
                    result.CreateKey = cs.getInteger("CreateKey");
                    result.DateAdded = cs.getDate("DateAdded");
                    //
                    //
                    //
                    result.ExcludeFromAnalytics = cs.getBoolean("ExcludeFromAnalytics");
                    result.HTTP_FROM = cs.getText("HTTP_FROM");
                    result.HTTP_REFERER = cs.getText("HTTP_REFERER");
                    result.HTTP_VIA = cs.getText("HTTP_VIA");
                    result.LastVisitTime = cs.getDate("LastVisitTime");
                    result.LoginAttempts = cs.getInteger("LoginAttempts");
                    result.MemberID = cs.getInteger("MemberID");
                    result.MemberNew = cs.getBoolean("MemberNew");
                    result.Mobile = cs.getBoolean("Mobile");
                    result.ModifiedBy = cs.getInteger("ModifiedBy");
                    result.ModifiedDate = cs.getDate("ModifiedDate");
                    result.Name = cs.getText("Name");
                    result.PageVisits = cs.getInteger("PageVisits");
                    result.RefererPathPage = cs.getText("RefererPathPage");
                    result.REMOTE_ADDR = cs.getText("REMOTE_ADDR");
                    result.RemoteName = cs.getText("RemoteName");
                    result.SortOrder = cs.getText("SortOrder");
                    result.StartDateValue = cs.getInteger("StartDateValue");
                    result.StartTime = cs.getDate("StartTime");
                    result.StopTime = cs.getDate("StopTime");
                    result.TimeToLastHit = cs.getInteger("TimeToLastHit");
                    result.VerboseReporting = cs.getBoolean("VerboseReporting");
                    result.VisitAuthenticated = cs.getBoolean("VisitAuthenticated");
                    result.VisitorID = cs.getInteger("VisitorID");
                    result.VisitorNew = cs.getBoolean("VisitorNew");
                    if (string.IsNullOrEmpty(result.ccGuid)) {
                        result.ccGuid = Controllers.genericController.getGUID();
                    }
                    if (result != null) {
                        //
                        // -- set primary and secondary caches
                        // -- add all cachenames to the injected cachenamelist
                        string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.id.ToString());
                        cacheNameList.Add(cacheName0);
                        core.cache.setObject(cacheName0, result);
                        //
                        string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
                        cacheNameList.Add(cacheName1);
                        core.cache.setAlias(cacheName1, cacheName0);
                    }
                }
                cs.close();
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public int saveObject(coreController core) {
            try {
                csController cs = new csController(core);
                if (id > 0) {
                    if (!cs.open(primaryContentName, "id=" + id)) {
                        id = 0;
                        cs.close();
                        throw new ApplicationException("Unable to open record in content [" + primaryContentName + "], with id [" + id + "]");
                    }
                } else {
                    if (!cs.insert(primaryContentName)) {
                        cs.close();
                        id = 0;
                        throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
                    }
                }
                if (cs.ok()) {
                    id = cs.getInteger("id");
                    if (string.IsNullOrEmpty(ccGuid)) {
                        ccGuid = Controllers.genericController.getGUID();
                    }
                    if (string.IsNullOrEmpty(Name)) {
                        Name = "Visit " + id.ToString();
                    }
                    cs.setField("Active", Active.ToString());
                    cs.setField("Bot", Bot.ToString());
                    cs.setField("Browser", Browser);
                    cs.setField("ccGuid", ccGuid);
                    //
                    cs.setField("ContentControlID", ContentControlID.ToString());
                    cs.setField("CookieSupport", CookieSupport.ToString());
                    cs.setField("CreatedBy", CreatedBy.ToString());
                    cs.setField("CreateKey", CreateKey.ToString());
                    cs.setField("DateAdded", DateAdded.ToString());
                    //
                    //
                    //
                    cs.setField("ExcludeFromAnalytics", ExcludeFromAnalytics.ToString());
                    cs.setField("HTTP_FROM", HTTP_FROM);
                    cs.setField("HTTP_REFERER", HTTP_REFERER);
                    cs.setField("HTTP_VIA", HTTP_VIA);
                    cs.setField("LastVisitTime", LastVisitTime.ToString());
                    cs.setField("LoginAttempts", LoginAttempts.ToString());
                    cs.setField("MemberID", MemberID.ToString());
                    cs.setField("MemberNew", MemberNew.ToString());
                    cs.setField("Mobile", Mobile.ToString());
                    cs.setField("ModifiedBy", ModifiedBy.ToString());
                    cs.setField("ModifiedDate", ModifiedDate.ToString());
                    cs.setField("Name", Name);
                    cs.setField("PageVisits", PageVisits.ToString());
                    cs.setField("RefererPathPage", RefererPathPage);
                    cs.setField("REMOTE_ADDR", REMOTE_ADDR);
                    cs.setField("RemoteName", RemoteName);
                    cs.setField("SortOrder", SortOrder);
                    cs.setField("StartDateValue", StartDateValue.ToString());
                    cs.setField("StartTime", StartTime.ToString());
                    cs.setField("StopTime", StopTime.ToString());
                    cs.setField("TimeToLastHit", TimeToLastHit.ToString());
                    cs.setField("VerboseReporting", VerboseReporting.ToString());
                    cs.setField("VisitAuthenticated", VisitAuthenticated.ToString());
                    cs.setField("VisitorID", VisitorID.ToString());
                    cs.setField("VisitorNew", VisitorNew.ToString());
                }
                cs.close();
                //
                // -- object is here, but the cache was invalidated, setting
                core.cache.setObject(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.id.ToString()), this);
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
                throw;
            }
            return id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete(coreController core, int recordId) {
            try {
                if (recordId > 0) {
                    core.db.deleteContentRecords(primaryContentName, "id=" + recordId.ToString());
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete(coreController core, string guid) {
            try {
                if (!string.IsNullOrEmpty(guid)) {
                    core.db.deleteContentRecords(primaryContentName, "(ccguid=" + core.db.encodeSQLText(guid) + ")");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<visitModelx> getObjectList(coreController core, int someCriteria) {
            List<visitModelx> result = new List<visitModelx>();
            try {
                csController cs = new csController(core);
                List<string> ignoreCacheNames = new List<string>();
                if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id")) {
                    visitModelx instance = null;
                    do {
                        instance = visitModelx.create(core, cs.getInteger("id"), ref ignoreCacheNames);
                        if (instance != null) {
                            result.Add(instance);
                        }
                        cs.goNext();
                    } while (cs.ok());
                }
                cs.close();
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the primary key (which depends on all secondary keys)
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        public static void invalidateIdCache(coreController core, int recordId) {
            core.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
            //
            // -- always clear the cache with the content name
            //?? '?? core.cache.invalidateObject(primaryContentName)
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate a secondary key (ccGuid field).
        /// </summary>
        /// <param name="core"></param>
        /// <param name="guid"></param>
        public static void invalidateGuidCache(coreController core, string guid) {
            core.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", guid));
            //
            // -- always clear the cache with the content name
            //?? core.cache.invalidateObject(primaryContentName)
        }
        //
        //====================================================================================================
        // <summary>
        // produce a standard format cachename for this model
        // </summary>
        // <param name="fieldName"></param>
        // <param name="fieldValue"></param>
        // <returns></returns>
        //Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
        //    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        //End Function
        //
        //====================================================================================================
        /// <summary>
        /// return a visit object for the visitor's last visit before the provided id
        /// </summary>
        /// <param name="core"></param>
        /// <param name="visitId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public static visitModelx getLastVisitByVisitor(coreController core, int visitId, int visitorId) {
            var tempVar = new List<string>();
            visitModelx result = loadObject(core, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", ref tempVar, "id desc");
            return result;
        }
    }
}
