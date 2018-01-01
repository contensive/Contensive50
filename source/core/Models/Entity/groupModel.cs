
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
    public class groupModel {
        //
        //-- const
        public const string primaryContentName = "groups"; //<------ set content name
        private const string primaryContentTableName = "ccgroups"; //<------ set to tablename for the primary content (used for cache names)
        private const string primaryContentDataSource = "default"; //<----- set to datasource if not default
                                                                   //
                                                                   // -- instance properties
        public int ID;
        public bool Active;
        public bool AllowBulkEmail;
        public string Caption;
        public string ccGuid;

        public int ContentControlID;
        public string CopyFilename;
        public int CreatedBy;
        public int CreateKey;
        public DateTime DateAdded;



        public int ModifiedBy;
        public DateTime ModifiedDate;
        public string Name;
        public bool PublicJoin;
        public string SortOrder;
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization
        /// </summary>
        public groupModel() {
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
        public static groupModel add(coreClass cpCore, ref List<string> cacheNameList) {
            groupModel result = null;
            try {
                result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, 0), ref cacheNameList);
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
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public static groupModel create(coreClass cpCore, int recordId, ref List<string> cacheNameList) {
            groupModel result = null;
            try {
                if (recordId > 0) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
                    result = cpCore.cache.getObject<groupModel>(cacheName);
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
        public static groupModel create(coreClass cpCore, string recordGuid, ref List<string> cacheNameList) {
            groupModel result = null;
            try {
                if (!string.IsNullOrEmpty(recordGuid)) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
                    result = cpCore.cache.getObject<groupModel>(cacheName);
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
        /// template for open an existing object with multiple keys (like a rule)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="foreignKey1Id"></param>
        public static groupModel create(coreClass cpCore, int foreignKey1Id, int foreignKey2Id, ref List<string> cacheNameList) {
            groupModel result = null;
            try {
                if ((foreignKey1Id > 0) && (foreignKey2Id > 0)) {
                    result = cpCore.cache.getObject<groupModel>(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "foreignKey1", foreignKey1Id, "foreignKey2", foreignKey2Id));
                    if (result == null) {
                        result = loadObject(cpCore, "(foreignKey1=" + foreignKey1Id.ToString() + ")and(foreignKey1=" + foreignKey1Id.ToString() + ")", ref cacheNameList);
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
        private static groupModel loadObject(coreClass cpCore, string sqlCriteria, ref List<string> callersCacheNameList) {
            groupModel result = null;
            try {
                csController cs = new csController(cpCore);
                if (cs.open(primaryContentName, sqlCriteria)) {
                    result = new groupModel();
                    //
                    // -- populate result model
                    result.ID = cs.getInteger("ID");
                    result.Active = cs.getBoolean("Active");
                    result.AllowBulkEmail = cs.getBoolean("AllowBulkEmail");
                    result.Caption = cs.getText("Caption");
                    result.ccGuid = cs.getText("ccGuid");
                    //
                    result.ContentControlID = cs.getInteger("ContentControlID");
                    result.CopyFilename = cs.getText("CopyFilename");
                    result.CreatedBy = cs.getInteger("CreatedBy");
                    result.CreateKey = cs.getInteger("CreateKey");
                    result.DateAdded = cs.getDate("DateAdded");
                    //
                    //
                    //
                    result.ModifiedBy = cs.getInteger("ModifiedBy");
                    result.ModifiedDate = cs.getDate("ModifiedDate");
                    result.Name = cs.getText("Name");
                    result.PublicJoin = cs.getBoolean("PublicJoin");
                    result.SortOrder = cs.getText("SortOrder");
                    if (result != null) {
                        //
                        // -- set primary cache to the object created
                        // -- set secondary caches to the primary cache
                        // -- add all cachenames to the injected cachenamelist
                        string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.ID.ToString());
                        callersCacheNameList.Add(cacheName0);
                        cpCore.cache.setObject(cacheName0, result);
                        //
                        string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
                        callersCacheNameList.Add(cacheName1);
                        cpCore.cache.setAlias(cacheName1, cacheName0);
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
                        ID = 0;
                        cs.Close();
                        throw new ApplicationException("Unable to open record in content [" + primaryContentName + "], with id [" + ID + "]");
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
                    cs.setField("Active", Active.ToString());
                    cs.setField("AllowBulkEmail", AllowBulkEmail.ToString());
                    cs.setField("Caption", Caption);
                    cs.setField("ccGuid", ccGuid);
                    //
                    cs.setField("ContentControlID", ContentControlID.ToString());
                    cs.setField("CopyFilename", CopyFilename);
                    cs.setField("CreatedBy", CreatedBy.ToString());
                    cs.setField("CreateKey", CreateKey.ToString());
                    cs.setField("DateAdded", DateAdded.ToString());
                    //
                    //
                    //
                    cs.setField("ModifiedBy", ModifiedBy.ToString());
                    cs.setField("ModifiedDate", ModifiedDate.ToString());
                    cs.setField("Name", Name);
                    cs.setField("PublicJoin", PublicJoin.ToString());
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
                cpCore.cache.setObject(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.ID.ToString()), this);
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
        /// delete an existing database record by id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete(coreClass cpCore, int recordId) {
            try {
                if (recordId > 0) {
                    cpCore.db.deleteContentRecords(primaryContentName, "id=" + recordId.ToString());
                    cpCore.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
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
        /// delete an existing database record by guid
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete(coreClass cpCore, string ccguid) {
            try {
                if (!string.IsNullOrEmpty(ccguid)) {
                    cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" + cpCore.db.encodeSQLText(ccguid) + ")");
                    cpCore.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", ccguid));
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
        /// pattern to delete an existing object based on multiple criteria (like a rule record)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="foreignKey1Id"></param>
        /// <param name="foreignKey2Id"></param>
        public static void delete(coreClass cpCore, int foreignKey1Id, int foreignKey2Id) {
            try {
                if ((foreignKey2Id > 0) && (foreignKey1Id > 0)) {
                    var tempVar = new List<string>();
                    groupModel rule = create(cpCore, foreignKey1Id, foreignKey2Id, ref tempVar);
                    if (rule != null) {
                        cpCore.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString()) + Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "foreignKey2", foreignKey1Id.ToString()));
                        cpCore.db.deleteTableRecord(primaryContentTableName, rule.ID, primaryContentDataSource);
                    }
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
        /// pattern get a list of objects from this model
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="someCriteria"></param>
        /// <returns></returns>
        public static List<groupModel> getObjectList(coreClass cpCore, int someCriteria) {
            List<groupModel> result = new List<groupModel>();
            try {
                csController cs = new csController(cpCore);
                List<string> ignoreCacheNames = new List<string>();
                if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id")) {
                    groupModel instance = null;
                    do {
                        instance = groupModel.create(cpCore, cs.getInteger("id"), ref ignoreCacheNames);
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
        public static void invalidatePrimaryCache(coreClass cpCore, int recordId) {
            cpCore.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
            //
            // -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
            cpCore.cache.invalidate(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", "0"));
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
        //    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
        //End Function
        //Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
        //    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        //End Function
    }
}
