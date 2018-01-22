
//using System;
//using System.Reflection;
//using System.Xml;
//using System.Diagnostics;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using Contensive.Core;
//using Contensive.Core.Models.DbModels;
//using Contensive.Core.Controllers;
//using static Contensive.Core.Controllers.genericController;
//using static Contensive.Core.constants;
////
//namespace Contensive.Core.Models.DbModels {
//    public class visitorModelx {
//        //
//        //-- const
//        public const string primaryContentName = "visitors"; //<------ set content name
//        private const string primaryContentTableName = "ccvisitors"; //<------ set to tablename for the primary content (used for cache names)
//        private const string primaryContentDataSource = "default"; //<----- set to datasource if not default
//                                                                   //
//                                                                   // -- instance properties
//        public int ID;
//        public bool Active;
//        public string ccGuid;
//        public int ContentControlID;
//        public int CreatedBy;
//        public int CreateKey;
//        public DateTime DateAdded;
//        public int ForceBrowserMobile;
//        public int MemberID;
//        public int ModifiedBy;
//        public DateTime ModifiedDate;
//        public string Name;
//        public int OrderID;
//        public string SortOrder;
//        //Public id As Integer
//        //Public name As String
//        //Public guid As String
//        //
//        //Public memberID As Integer = 0              ' the last member account this visitor used (memberid=0 means untracked guest)
//        //Public orderID As Integer = 0               ' the current shopping cart (non-complete order)
//        //Public newVisitor As Boolean = False               ' stored in visit record - Is this the first visit for this visitor
//        //Public forceBrowserMobile As Integer = 0           ' 0 = not set -- use Browser detect each time, 1 = Force Mobile, 2 = Force not Mobile
//        //
//        // -- publics not exposed to the UI (test/internal data)
//        //<JsonIgnore> Public createKey As Integer
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Create an empty object. needed for deserialization
//        /// </summary>
//        public visitorModelx() {
//            //
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// add a new recod to the db and open it. Starting a new model with this method will use the default
//        /// values in Contensive metadata (active, contentcontrolid, etc)
//        /// </summary>
//        /// <param name="core"></param>
//        /// <param name="cacheNameList"></param>
//        /// <returns></returns>
//        public static visitorModelx add(coreClass core, ref List<string> cacheNameList) {
//            visitorModelx result = null;
//            try {
//                result = create(core, core.db.insertContentRecordGetID(primaryContentName, 0), ref cacheNameList);
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//            return result;
//        }
//        public static visitorModelx add(coreClass core) { var tmpList = new List<string> { }; return add(core, ref tmpList); }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId">The id of the record to be read into the new object</param>
//        /// <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
//        public static visitorModelx create(coreClass core, int recordId, ref List<string> cacheNameList) {
//            visitorModelx result = null;
//            try {
//                if (recordId > 0) {
//                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
//                    result = core.cache.getObject<visitorModelx>(cacheName);
//                    if (result == null) {
//                        result = loadObject(core, "id=" + recordId.ToString(), ref cacheNameList);
//                    }
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//            return result;
//        }
//        public static visitorModelx create(coreClass core, int recordId) { var tmpList = new List<string> { }; return create(core, recordId, ref tmpList); }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// open an existing object
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordGuid"></param>
//        public static visitorModelx create(coreClass core, string recordGuid, ref List<string> cacheNameList) {
//            visitorModelx result = null;
//            try {
//                if (!string.IsNullOrEmpty(recordGuid)) {
//                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
//                    result = core.cache.getObject<visitorModelx>(cacheName);
//                    if (result == null) {
//                        result = loadObject(core, "ccGuid=" + core.db.encodeSQLText(recordGuid), ref cacheNameList);
//                    }
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//            return result;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// open an existing object
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="sqlCriteria"></param>
//        private static visitorModelx loadObject(coreClass core, string sqlCriteria, ref List<string> callersCacheNameList) {
//            visitorModelx result = null;
//            try {
//                csController cs = new csController(core);
//                if (cs.open(primaryContentName, sqlCriteria)) {
//                    result = new visitorModelx();
//                    //
//                    // -- populate result model
//                    result.ID = cs.getInteger("ID");
//                    result.Active = cs.getBoolean("Active");
//                    result.ccGuid = cs.getText("ccGuid");
//                    //
//                    result.ContentControlID = cs.getInteger("ContentControlID");
//                    result.CreatedBy = cs.getInteger("CreatedBy");
//                    result.CreateKey = cs.getInteger("CreateKey");
//                    result.DateAdded = cs.getDate("DateAdded");
//                    //
//                    //
//                    //
//                    result.ForceBrowserMobile = cs.getInteger("ForceBrowserMobile");
//                    result.MemberID = cs.getInteger("MemberID");
//                    result.ModifiedBy = cs.getInteger("ModifiedBy");
//                    result.ModifiedDate = cs.getDate("ModifiedDate");
//                    result.Name = cs.getText("Name");
//                    result.OrderID = cs.getInteger("OrderID");
//                    result.SortOrder = cs.getText("SortOrder");
//                    if (string.IsNullOrEmpty(result.ccGuid)) {
//                        result.ccGuid = Controllers.genericController.getGUID();
//                    }
//                    if (result != null) {
//                        //
//                        // -- set primary and secondary caches
//                        // -- add all cachenames to the injected cachenamelist
//                        string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.ID.ToString());
//                        callersCacheNameList.Add(cacheName0);
//                        core.cache.setObject(cacheName0, result);
//                        //
//                        string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
//                        callersCacheNameList.Add(cacheName1);
//                        core.cache.setAlias(cacheName1, cacheName0);
//                    }
//                }
//                cs.Close();
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//            return result;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
//        /// </summary>
//        /// <param name="core"></param>
//        /// <returns></returns>
//        public int saveObject(coreClass core) {
//            try {
//                csController cs = new csController(core);
//                if (ID > 0) {
//                    if (!cs.open(primaryContentName, "id=" + ID)) {
//                        string message = "Unable to open record in content [" + primaryContentName + "], with id [" + ID + "]";
//                        cs.Close();
//                        ID = 0;
//                        throw new ApplicationException(message);
//                    }
//                } else {
//                    if (!cs.Insert(primaryContentName)) {
//                        cs.Close();
//                        ID = 0;
//                        throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
//                    }
//                }
//                if (cs.OK()) {
//                    ID = cs.getInteger("id");
//                    if (string.IsNullOrEmpty(ccGuid)) {
//                        ccGuid = Controllers.genericController.getGUID();
//                    }
//                    if (string.IsNullOrEmpty(Name)) {
//                        Name = "Visitor " + ID.ToString();
//                    }
//                    cs.setField("Active", Active.ToString());
//                    cs.setField("ccGuid", ccGuid);
//                    //
//                    cs.setField("ContentControlID", ContentControlID.ToString());
//                    cs.setField("CreatedBy", CreatedBy.ToString());
//                    cs.setField("CreateKey", CreateKey.ToString());
//                    cs.setField("DateAdded", DateAdded.ToString());
//                    //
//                    //
//                    //
//                    cs.setField("ForceBrowserMobile", ForceBrowserMobile.ToString());
//                    cs.setField("MemberID", MemberID.ToString());
//                    cs.setField("ModifiedBy", ModifiedBy.ToString());
//                    cs.setField("ModifiedDate", ModifiedDate.ToString());
//                    cs.setField("Name", Name);
//                    cs.setField("OrderID", OrderID.ToString());
//                    cs.setField("SortOrder", SortOrder);
//                }
//                cs.Close();
//                //
//                // -- invalidate objects
//                // -- no, the primary is invalidated by the cs.save()
//                //core.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
//                // -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
//                //core.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
//                //
//                // -- object is here, but the cache was invalidated, setting
//                core.cache.setObject(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.ID.ToString()), this);
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//            return ID;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// delete an existing database record
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId"></param>
//        public static void delete(coreClass core, int recordId) {
//            try {
//                if (recordId > 0) {
//                    core.db.deleteContentRecords(primaryContentName, "id=" + recordId.ToString());
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// delete an existing database record
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="recordId"></param>
//        public static void delete(coreClass core, string guid) {
//            try {
//                if (!string.IsNullOrEmpty(guid)) {
//                    core.db.deleteContentRecords(primaryContentName, "(ccguid=" + core.db.encodeSQLText(guid) + ")");
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//                throw;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// get a list of objects from this model
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="someCriteria"></param>
//        /// <returns></returns>
//        public static List<visitorModelx> getObjectList(coreClass core, int someCriteria) {
//            List<visitorModelx> result = new List<visitorModelx>();
//            try {
//                csController cs = new csController(core);
//                List<string> ignoreCacheNames = new List<string>();
//                if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id")) {
//                    visitorModelx instance = null;
//                    do {
//                        instance = visitorModelx.create(core, cs.getInteger("id"), ref ignoreCacheNames);
//                        if (instance != null) {
//                            result.Add(instance);
//                        }
//                        cs.goNext();
//                    } while (cs.OK());
//                }
//                cs.Close();
//            } catch (Exception ex) {
//                core.handleException(ex);
//                throw;
//            }
//            return result;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// invalidate the primary key (which depends on all secondary keys)
//        /// </summary>
//        /// <param name="core"></param>
//        /// <param name="recordId"></param>
//        public static void invalidateIdCache(coreClass core, int recordId) {
//            core.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
//            //
//            // -- always clear the cache with the content name
//            //?? core.cache.invalidateObject(primaryContentName)
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// invalidate a secondary key (ccGuid field).
//        /// </summary>
//        /// <param name="core"></param>
//        /// <param name="guid"></param>
//        public static void invalidateGuidCache(coreClass core, string guid) {
//            core.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", guid));
//            //
//            // -- always clear the cache with the content name
//            //?? core.cache.invalidateObject(primaryContentName)
//        }
//        //
//        //====================================================================================================
//        // <summary>
//        // produce a standard format cachename for this model
//        // </summary>
//        // <param name="fieldName"></param>
//        // <param name="fieldValue"></param>
//        // <returns></returns>
//        //Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
//        //    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
//        //End Function
//        //
//        //====================================================================================================
//        //
//        public static visitorModelx getDefault(coreClass core) {
//            visitorModelx instance = new visitorModelx();
//            try {
//                Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(core, primaryContentName);
//                if (CDef == null) {
//                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
//                } else if (CDef.Id <= 0) {
//                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
//                } else {
//                    instance.Active = genericController.encodeBoolean(CDef.fields["Active"].defaultValue);
//                    instance.ccGuid = genericController.encodeText(CDef.fields["ccGuid"].defaultValue);
//                    instance.ContentControlID = genericController.encodeInteger(CDef.fields["ContentControlID"].defaultValue);
//                    instance.CreatedBy = genericController.encodeInteger(CDef.fields["CreatedBy"].defaultValue);
//                    instance.CreateKey = genericController.encodeInteger(CDef.fields["CreateKey"].defaultValue);
//                    instance.DateAdded = genericController.encodeDate(CDef.fields["DateAdded"].defaultValue);
//                    instance.ForceBrowserMobile = genericController.encodeInteger(CDef.fields["ForceBrowserMobile"].defaultValue);
//                    instance.MemberID = genericController.encodeInteger(CDef.fields["MemberID"].defaultValue);
//                    instance.ModifiedBy = genericController.encodeInteger(CDef.fields["ModifiedBy"].defaultValue);
//                    instance.ModifiedDate = genericController.encodeDate(CDef.fields["ModifiedDate"].defaultValue);
//                    instance.Name = genericController.encodeText(CDef.fields["Name"].defaultValue);
//                    instance.OrderID = genericController.encodeInteger(CDef.fields["OrderID"].defaultValue);
//                    instance.SortOrder = genericController.encodeText(CDef.fields["SortOrder"].defaultValue);
//                }
//            } catch (Exception ex) {
//                core.handleException(ex);
//            }
//            return instance;
//        }
//        //        '
//        //        ' LEGACY CODE =============================================================================
//        //        '   Save Visitor
//        //        '
//        //        '   Saves changes to the visitor record back to the database. Should be called
//        //        '   before exit of anypage if anything here changes
//        //        '=============================================================================
//        //        '
//        //        Public Sub saveObject(core As coreClass)
//        //            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveVisitor")
//        //            '
//        //            'If Not (true) Then Exit Sub
//        //            '
//        //            Dim SQL As String
//        //            Dim MethodName As String
//        //            '
//        //            MethodName = "main_SaveVisitor"
//        //            '
//        //            If core.visit.visit_initialized Then
//        //                If True Then
//        //                    SQL = "UPDATE ccVisitors SET " _
//        //                        & " Name = " & core.db.encodeSQLText(name) _
//        //                        & ",MemberID = " & core.db.encodeSQLNumber(memberID) _
//        //                        & ",OrderID = " & core.db.encodeSQLNumber(orderID) _
//        //                        & ",ForceBrowserMobile = " & core.db.encodeSQLNumber(forceBrowserMobile) _
//        //                        & " WHERE ID=" & id & ";"
//        //                Else
//        //                    SQL = "UPDATE ccVisitors SET " _
//        //                        & " Name = " & core.db.encodeSQLText(name) _
//        //                        & ",MemberID = " & core.db.encodeSQLNumber(memberID) _
//        //                        & ",OrderID = " & core.db.encodeSQLNumber(orderID) _
//        //                        & " WHERE ID=" & id & ";"
//        //                End If
//        //                Call core.db.executeSql(SQL)
//        //            End If
//        //            Exit Sub
//        //            '
//        //            ' ----- Error Trap
//        //            '
//        ////ErrorTrap:
//        //            //throw new ApplicationException("Unexpected exception") ' Call core.handleLegacyError18(MethodName)
//        //            '
//        //        End Sub
//    }
//}
