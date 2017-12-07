
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
    public class visitorModel {
        //
        //-- const
        public const string primaryContentName = "visitors"; //<------ set content name
        private const string primaryContentTableName = "ccvisitors"; //<------ set to tablename for the primary content (used for cache names)
        private const string primaryContentDataSource = "default"; //<----- set to datasource if not default
                                                                   //
                                                                   // -- instance properties
        public int ID;
        public bool Active;
        public string ccGuid;
        public int ContentControlID;
        public int CreatedBy;
        public int CreateKey;
        public DateTime DateAdded;
        public int ForceBrowserMobile;
        public int MemberID;
        public int ModifiedBy;
        public DateTime ModifiedDate;
        public string Name;
        public int OrderID;
        public string SortOrder;
        //Public id As Integer
        //Public name As String
        //Public guid As String
        //'
        //Public memberID As Integer = 0              ' the last member account this visitor used (memberid=0 means untracked guest)
        //Public orderID As Integer = 0               ' the current shopping cart (non-complete order)
        //Public newVisitor As Boolean = False               ' stored in visit record - Is this the first visit for this visitor
        //Public forceBrowserMobile As Integer = 0           ' 0 = not set -- use Browser detect each time, 1 = Force Mobile, 2 = Force not Mobile
        //'
        //' -- publics not exposed to the UI (test/internal data)
        //<JsonIgnore> Public createKey As Integer
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization
        /// </summary>
        public visitorModel() {
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// add a new recod to the db and open it. Starting a new model with this method will use the default
        /// values in Contensive metadata (active, contentcontrolid, etc)
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="cacheNameList"></param>
        /// <returns></returns>
        public static visitorModel add(coreClass cpCore, ref List<string> cacheNameList) {
            visitorModel result = null;
            try {
                result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, 0), ref cacheNameList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return result;
        }
        public static visitorModel add(coreClass cpCore) { var tmpList = new List<string> { }; return add(cpCore, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public static visitorModel create(coreClass cpCore, int recordId, ref List<string> cacheNameList) {
            visitorModel result = null;
            try {
                if (recordId > 0) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
                    result = cpCore.cache.getObject<visitorModel>(cacheName);
                    if (result == null) {
                        result = loadObject(cpCore, "id=" + recordId.ToString(), ref cacheNameList);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
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
        /// <param name="recordGuid"></param>
        public static visitorModel create(coreClass cpCore, string recordGuid, ref List<string> cacheNameList) {
            visitorModel result = null;
            try {
                if (!string.IsNullOrEmpty(recordGuid)) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
                    result = cpCore.cache.getObject<visitorModel>(cacheName);
                    if (result == null) {
                        result = loadObject(cpCore, "ccGuid=" + cpCore.db.encodeSQLText(recordGuid), ref cacheNameList);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
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
        private static visitorModel loadObject(coreClass cpCore, string sqlCriteria, ref List<string> callersCacheNameList) {
            visitorModel result = null;
            try {
                csController cs = new csController(cpCore);
                if (cs.open(primaryContentName, sqlCriteria)) {
                    result = new visitorModel();
                    //
                    // -- populate result model
                    result.ID = cs.getInteger("ID");
                    result.Active = cs.getBoolean("Active");
                    result.ccGuid = cs.getText("ccGuid");
                    //
                    result.ContentControlID = cs.getInteger("ContentControlID");
                    result.CreatedBy = cs.getInteger("CreatedBy");
                    result.CreateKey = cs.getInteger("CreateKey");
                    result.DateAdded = cs.getDate("DateAdded");
                    //
                    //
                    //
                    result.ForceBrowserMobile = cs.getInteger("ForceBrowserMobile");
                    result.MemberID = cs.getInteger("MemberID");
                    result.ModifiedBy = cs.getInteger("ModifiedBy");
                    result.ModifiedDate = cs.getDate("ModifiedDate");
                    result.Name = cs.getText("Name");
                    result.OrderID = cs.getInteger("OrderID");
                    result.SortOrder = cs.getText("SortOrder");
                    if (string.IsNullOrEmpty(result.ccGuid)) {
                        result.ccGuid = Controllers.genericController.getGUID();
                    }
                    if (result != null) {
                        //
                        // -- set primary and secondary caches
                        // -- add all cachenames to the injected cachenamelist
                        string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.ID.ToString());
                        callersCacheNameList.Add(cacheName0);
                        cpCore.cache.setContent(cacheName0, result);
                        //
                        string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
                        callersCacheNameList.Add(cacheName1);
                        cpCore.cache.setPointer(cacheName1, cacheName0);
                    }
                }
                cs.Close();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="cpCore"></param>
        /// <returns></returns>
        public int saveObject(coreClass cpCore) {
            try {
                csController cs = new csController(cpCore);
                if (ID > 0) {
                    if (!cs.open(primaryContentName, "id=" + ID)) {
                        string message = "Unable to open record in content [" + primaryContentName + "], with id [" + ID + "]";
                        cs.Close();
                        ID = 0;
                        throw new ApplicationException(message);
                    }
                } else {
                    if (!cs.Insert(primaryContentName)) {
                        cs.Close();
                        ID = 0;
                        throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
                    }
                }
                if (cs.OK()) {
                    ID = cs.getInteger("id");
                    if (string.IsNullOrEmpty(ccGuid)) {
                        ccGuid = Controllers.genericController.getGUID();
                    }
                    if (string.IsNullOrEmpty(Name)) {
                        Name = "Visitor " + ID.ToString();
                    }
                    cs.setField("Active", Active.ToString());
                    cs.setField("ccGuid", ccGuid);
                    //
                    cs.setField("ContentControlID", ContentControlID.ToString());
                    cs.setField("CreatedBy", CreatedBy.ToString());
                    cs.setField("CreateKey", CreateKey.ToString());
                    cs.setField("DateAdded", DateAdded.ToString());
                    //
                    //
                    //
                    cs.setField("ForceBrowserMobile", ForceBrowserMobile.ToString());
                    cs.setField("MemberID", MemberID.ToString());
                    cs.setField("ModifiedBy", ModifiedBy.ToString());
                    cs.setField("ModifiedDate", ModifiedDate.ToString());
                    cs.setField("Name", Name);
                    cs.setField("OrderID", OrderID.ToString());
                    cs.setField("SortOrder", SortOrder);
                }
                cs.Close();
                //
                // -- invalidate objects
                // -- no, the primary is invalidated by the cs.save()
                //cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
                // -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
                //cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
                //
                // -- object is here, but the cache was invalidated, setting
                cpCore.cache.setContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.ID.ToString()), this);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return ID;
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete(coreClass cpCore, int recordId) {
            try {
                if (recordId > 0) {
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" + recordId.ToString());
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static void delete(coreClass cpCore, string guid) {
            try {
                if (!string.IsNullOrEmpty(guid)) {
                    cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" + cpCore.db.encodeSQLText(guid) + ")");
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        public static List<visitorModel> getObjectList(coreClass cpCore, int someCriteria) {
            List<visitorModel> result = new List<visitorModel>();
            try {
                csController cs = new csController(cpCore);
                List<string> ignoreCacheNames = new List<string>();
                if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id")) {
                    visitorModel instance = null;
                    do {
                        instance = visitorModel.create(cpCore, cs.getInteger("id"), ref ignoreCacheNames);
                        if (instance != null) {
                            result.Add(instance);
                        }
                        cs.goNext();
                    } while (cs.OK());
                }
                cs.Close();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the primary key (which depends on all secondary keys)
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="recordId"></param>
        public static void invalidateIdCache(coreClass cpCore, int recordId) {
            cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
            //
            // -- always clear the cache with the content name
            //?? cpCore.cache.invalidateObject(primaryContentName)
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate a secondary key (ccGuid field).
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="guid"></param>
        public static void invalidateGuidCache(coreClass cpCore, string guid) {
            cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", guid));
            //
            // -- always clear the cache with the content name
            //?? cpCore.cache.invalidateObject(primaryContentName)
        }
        //'
        //'====================================================================================================
        //''' <summary>
        //''' produce a standard format cachename for this model
        //''' </summary>
        //''' <param name="fieldName"></param>
        //''' <param name="fieldValue"></param>
        //''' <returns></returns>
        //Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
        //    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        //End Function
        //
        //====================================================================================================
        //
        public static visitorModel getDefault(coreClass cpcore) {
            visitorModel instance = new visitorModel();
            try {
                Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpcore, primaryContentName);
                if (CDef == null) {
                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
                } else if (CDef.Id <= 0) {
                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
                } else {
                    instance.Active = genericController.EncodeBoolean(CDef.fields("Active").defaultValue);
                    instance.ccGuid = genericController.encodeText(CDef.fields("ccGuid").defaultValue);
                    instance.ContentControlID = genericController.EncodeInteger(CDef.fields("ContentControlID").defaultValue);
                    instance.CreatedBy = genericController.EncodeInteger(CDef.fields("CreatedBy").defaultValue);
                    instance.CreateKey = genericController.EncodeInteger(CDef.fields("CreateKey").defaultValue);
                    instance.DateAdded = genericController.EncodeDate(CDef.fields("DateAdded").defaultValue);
                    instance.ForceBrowserMobile = genericController.EncodeInteger(CDef.fields("ForceBrowserMobile").defaultValue);
                    instance.MemberID = genericController.EncodeInteger(CDef.fields("MemberID").defaultValue);
                    instance.ModifiedBy = genericController.EncodeInteger(CDef.fields("ModifiedBy").defaultValue);
                    instance.ModifiedDate = genericController.EncodeDate(CDef.fields("ModifiedDate").defaultValue);
                    instance.Name = genericController.encodeText(CDef.fields("Name").defaultValue);
                    instance.OrderID = genericController.EncodeInteger(CDef.fields("OrderID").defaultValue);
                    instance.SortOrder = genericController.encodeText(CDef.fields("SortOrder").defaultValue);
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return instance;
        }
        //        '
        //        ' LEGACY CODE =============================================================================
        //        '   Save Visitor
        //        '
        //        '   Saves changes to the visitor record back to the database. Should be called
        //        '   before exit of anypage if anything here changes
        //        '=============================================================================
        //        '
        //        Public Sub saveObject(cpcore As coreClass)
        //            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveVisitor")
        //            '
        //            'If Not (true) Then Exit Sub
        //            '
        //            Dim SQL As String
        //            Dim MethodName As String
        //            '
        //            MethodName = "main_SaveVisitor"
        //            '
        //            If cpcore.visit.visit_initialized Then
        //                If True Then
        //                    SQL = "UPDATE ccVisitors SET " _
        //                        & " Name = " & cpcore.db.encodeSQLText(name) _
        //                        & ",MemberID = " & cpcore.db.encodeSQLNumber(memberID) _
        //                        & ",OrderID = " & cpcore.db.encodeSQLNumber(orderID) _
        //                        & ",ForceBrowserMobile = " & cpcore.db.encodeSQLNumber(forceBrowserMobile) _
        //                        & " WHERE ID=" & id & ";"
        //                Else
        //                    SQL = "UPDATE ccVisitors SET " _
        //                        & " Name = " & cpcore.db.encodeSQLText(name) _
        //                        & ",MemberID = " & cpcore.db.encodeSQLNumber(memberID) _
        //                        & ",OrderID = " & cpcore.db.encodeSQLNumber(orderID) _
        //                        & " WHERE ID=" & id & ";"
        //                End If
        //                Call cpcore.db.executeSql(SQL)
        //            End If
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            //throw new ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
        //            '
        //        End Sub
    }
}
