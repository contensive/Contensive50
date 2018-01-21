
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
    public class oldPageTemplateModel {
        //
        //-- const
        public const string primaryContentName = "page templates";
        private const string primaryContentTableName = "ccpagetemplates";
        private const string primaryContentDataSource = "default";
        //
        // -- instance properties
        public int ID { get; set; }
        public bool Active { get; set; }
        public string BodyHTML { get; set; }
        // Public Property BodyTag As String
        public string ccGuid { get; set; }
        public int ContentControlID { get; set; }
        public int CreatedBy { get; set; }
        public int CreateKey { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsSecure { get; set; }
        // Public Property JSEndBody As String
        // Public Property JSFilename As String
        // Public Property JSHead As String
        // Public Property JSOnLoad As String
        // Public Property Link As String
        // Public Property MobileBodyHTML As String
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Name { get; set; }
        // Public Property OtherHeadTags As String
        public string SortOrder { get; set; }
        // Public Property Source As String
        // Public Property StylesFilename As String
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty object. needed for deserialization
        /// </summary>
        public oldPageTemplateModel() {
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// add a new recod to the db and open it. Starting a new model with this method will use the default
        /// values in Contensive metadata (active, contentcontrolid, etc)
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        public static oldPageTemplateModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, cpCore.doc.sessionContext.user.id), ref callersCacheNameList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return result;
        }
        public static oldPageTemplateModel add(coreController cpCore) { var tmpList = new List<string> { }; return add(cpCore, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public static oldPageTemplateModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                if (recordId > 0) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
                    result = cpCore.cache.getObject<oldPageTemplateModel>(cacheName);
                    if (result == null) {
                        using (csController cs = new csController(cpCore)) {
                            if (cs.open(primaryContentName, "(id=" + recordId.ToString() + ")")) {
                                result = loadRecord(cpCore, cs, ref callersCacheNameList);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return result;
        }
        public static oldPageTemplateModel create(coreController cpCore, int recordId) { var tmpList = new List<string> { }; return create(cpCore, recordId, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// open an existing object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        public static oldPageTemplateModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                if (!string.IsNullOrEmpty(recordGuid)) {
                    string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
                    result = cpCore.cache.getObject<oldPageTemplateModel>(cacheName);
                    if (result == null) {
                        using (csController cs = new csController(cpCore)) {
                            if (cs.open(primaryContentName, "(ccGuid=" + cpCore.db.encodeSQLText(recordGuid) + ")")) {
                                result = loadRecord(cpCore, cs, ref callersCacheNameList);
                            }
                        }
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
        public static oldPageTemplateModel create(coreController cpCore, int foreignKey1Id, int foreignKey2Id, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                if ((foreignKey1Id > 0) && (foreignKey2Id > 0)) {
                    result = cpCore.cache.getObject<oldPageTemplateModel>(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "foreignKey1", foreignKey1Id, "foreignKey2", foreignKey2Id));
                    if (result == null) {
                        using (csController cs = new csController(cpCore)) {
                            if (cs.open(primaryContentName, "(foreignKey1=" + foreignKey1Id.ToString() + ")and(foreignKey1=" + foreignKey1Id.ToString() + ")")) {
                                result = loadRecord(cpCore, cs, ref callersCacheNameList);
                            }
                        }
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
        /// <param name="recordName"></param>
        public static oldPageTemplateModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                if (!string.IsNullOrEmpty(recordName)) {
                    string nameKey = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "name", recordName);
                    result = cpCore.cache.getObject<oldPageTemplateModel>(nameKey);
                    if (result == null) {
                        using (csController cs = new csController(cpCore)) {
                            if (cs.open(primaryContentName, "(name=" + cpCore.db.encodeSQLText(recordName) + ")", "id")) {
                                result = loadRecord(cpCore, cs, ref callersCacheNameList);
                                string primaryKey = cacheController.getCacheKey_Entity(primaryContentTableName, result.ID);
                                cpCore.cache.setObject(primaryKey, result);
                                cpCore.cache.setAlias(nameKey, primaryKey);
                            }
                        }

                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
                throw;
            }
            return result;
        }
        public static oldPageTemplateModel createByName(coreController cpCore, string recordName) { var tmpList = new List<string> { }; return createByName(cpCore, recordName, ref tmpList); }
        //
        //====================================================================================================
        /// <summary>
        /// open an existing object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        private static oldPageTemplateModel loadRecord(coreController cpCore, csController cs, ref List<string> callersCacheNameList) {
            oldPageTemplateModel result = null;
            try {
                if (cs.ok()) {
                    result = new oldPageTemplateModel();
                    //
                    // -- populate result model
                    result.ID = cs.getInteger("ID");
                    result.Active = cs.getBoolean("Active");
                    result.BodyHTML = cs.getText("BodyHTML");
                    // .BodyTag = cs.getText("BodyTag")
                    result.ccGuid = cs.getText("ccGuid");
                    //
                    result.ContentControlID = cs.getInteger("ContentControlID");
                    result.CreatedBy = cs.getInteger("CreatedBy");
                    result.CreateKey = cs.getInteger("CreateKey");
                    result.DateAdded = cs.getDate("DateAdded");
                    //
                    //
                    //
                    result.IsSecure = cs.getBoolean("IsSecure");
                    // .JSEndBody = cs.getText("JSEndBody")
                    // .JSFilename = cs.getText("JSFilename")
                    // .JSHead = cs.getText("JSHead")
                    // .JSOnLoad = cs.getText("JSOnLoad")
                    // .Link = cs.getText("Link")
                    // .MobileBodyHTML = cs.getText("MobileBodyHTML")
                    result.ModifiedBy = cs.getInteger("ModifiedBy");
                    result.ModifiedDate = cs.getDate("ModifiedDate");
                    result.Name = cs.getText("Name");
                    // .OtherHeadTags = cs.getText("OtherHeadTags")
                    result.SortOrder = cs.getText("SortOrder");
                    // .Source = cs.getText("Source")
                    // .StylesFilename = cs.getText("StylesFilename")
                    if (result != null) {
                        //
                        // -- set primary cache to the object created
                        // -- set secondary caches to the primary cache
                        // -- add all cachenames to the injected cachenamelist
                        string key = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.ID.ToString());
                        callersCacheNameList.Add(key);
                        cpCore.cache.setObject(key, result);
                        //
                        string pointerKey = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
                        callersCacheNameList.Add(pointerKey);
                        cpCore.cache.setAlias(pointerKey, key);
                        //
                        //Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
                        //callersCacheNameList.Add(cacheName2)
                        //cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
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
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="cpCore"></param>
        /// <returns></returns>
        public int save(coreController cpCore) {
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
                    if (!cs.insert(primaryContentName)) {
                        cs.Close();
                        ID = 0;
                        throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
                    }
                }
                if (cs.ok()) {
                    ID = cs.getInteger("id");
                    cs.setField("Active", Active.ToString());
                    cs.setField("BodyHTML", BodyHTML);
                    // cs.setField("BodyTag", BodyTag)
                    cs.setField("ccGuid", ccGuid);
                    //
                    cs.setField("ContentControlID", ContentControlID.ToString());
                    cs.setField("CreatedBy", CreatedBy.ToString());
                    cs.setField("CreateKey", CreateKey.ToString());
                    cs.setField("DateAdded", DateAdded.ToString());
                    //
                    //
                    //
                    cs.setField("IsSecure", IsSecure.ToString());
                    // cs.setField("JSEndBody", JSEndBody)
                    // cs.setField("JSFilename", JSFilename)
                    // cs.setField("JSHead", JSHead)
                    // cs.setField("JSOnLoad", JSOnLoad)
                    // cs.setField("Link", Link)
                    // cs.setField("MobileBodyHTML", MobileBodyHTML)
                    cs.setField("ModifiedBy", ModifiedBy.ToString());
                    cs.setField("ModifiedDate", ModifiedDate.ToString());
                    cs.setField("Name", Name);
                    // cs.setField("OtherHeadTags", OtherHeadTags)
                    cs.setField("SortOrder", SortOrder);
                    // cs.setField("Source", Source)
                    // cs.setField("StylesFilename", StylesFilename)
                    if (string.IsNullOrEmpty(ccGuid)) {
                        ccGuid = Controllers.genericController.getGUID();
                    }
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
                // -- added tablename as depedant object - any change to any template flushes this cache
                cpCore.cache.setObject(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.ID.ToString()), this, primaryContentTableName);
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
        public static void delete(coreController cpCore, int recordId) {
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
        /// <param name="ccguid"></param>
        public static void delete(coreController cpCore, string ccguid) {
            try {
                if (!string.IsNullOrEmpty(ccguid)) {
                    var tempVar = new List<string>();
                    oldPageTemplateModel instance = create(cpCore, ccguid, ref tempVar);
                    if (instance != null) {
                        invalidatePrimaryCache(cpCore, instance.ID);
                        cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" + cpCore.db.encodeSQLText(ccguid) + ")");
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
        /// pattern to delete an existing object based on multiple criteria (like a rule record)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="foreignKey1Id"></param>
        /// <param name="foreignKey2Id"></param>
        public static void delete(coreController cpCore, int foreignKey1Id, int foreignKey2Id) {
            try {
                if ((foreignKey2Id > 0) && (foreignKey1Id > 0)) {
                    var tempVar = new List<string>();
                    oldPageTemplateModel instance = create(cpCore, foreignKey1Id, foreignKey2Id, ref tempVar);
                    if (instance != null) {
                        invalidatePrimaryCache(cpCore, instance.ID);
                        cpCore.db.deleteTableRecord(primaryContentTableName, instance.ID, primaryContentDataSource);
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
        public static List<oldPageTemplateModel> createList_criteria(coreController cpCore, int someCriteria, List<string> callersCacheNameList) {
            List<oldPageTemplateModel> result = new List<oldPageTemplateModel>();
            try {
                csController cs = new csController(cpCore);
                List<string> ignoreCacheNames = new List<string>();
                if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "id")) {
                    oldPageTemplateModel instance = null;
                    do {
                        instance = oldPageTemplateModel.loadRecord(cpCore, cs, ref callersCacheNameList);
                        if (instance != null) {
                            result.Add(instance);
                        }
                        cs.goNext();
                    } while (cs.ok());
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
        public static void invalidatePrimaryCache(coreController cpCore, int recordId) {
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
        //
        //Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
        //    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
        //End Function
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>record
        /// <returns></returns>
        public static string getRecordName(coreController cpcore, int recordId) {
            var tempVar = new List<string>();
            return oldPageTemplateModel.create(cpcore, recordId, ref tempVar).Name;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ccGuid"></param>record
        /// <returns></returns>
        public static string getRecordName(coreController cpcore, string ccGuid) {
            var tempVar = new List<string>();
            return oldPageTemplateModel.create(cpcore, ccGuid, ref tempVar).Name;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the id of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ccGuid"></param>record
        /// <returns></returns>
        public static int getRecordId(coreController cpcore, string ccGuid) {
            var tempVar = new List<string>();
            return oldPageTemplateModel.create(cpcore, ccGuid, ref tempVar).ID;
        }
        //
        //====================================================================================================
        //
        public static oldPageTemplateModel createDefault(coreController cpcore) {
            oldPageTemplateModel instance = new oldPageTemplateModel();
            try {
                Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpcore, primaryContentName);
                if (CDef == null) {
                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
                } else if (CDef.Id <= 0) {
                    throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
                } else {
                    instance.Active = genericController.encodeBoolean(CDef.fields["Active"].defaultValue);
                    instance.BodyHTML = genericController.encodeText(CDef.fields["BodyHTML"].defaultValue);
                    // instance.BodyTag = genericController.encodeText(.fields["BodyTag"].defaultValue)
                    instance.ccGuid = genericController.encodeText(CDef.fields["ccGuid"].defaultValue);
                    instance.ContentControlID = CDef.Id;
                    instance.CreatedBy = genericController.encodeInteger(CDef.fields["CreatedBy"].defaultValue);
                    instance.CreateKey = genericController.encodeInteger(CDef.fields["CreateKey"].defaultValue);
                    instance.DateAdded = genericController.encodeDate(CDef.fields["DateAdded"].defaultValue);
                    instance.IsSecure = genericController.encodeBoolean(CDef.fields["IsSecure"].defaultValue);
                    // instance.JSEndBody = genericController.encodeText(.fields["JSEndBody"].defaultValue)
                    // instance.JSFilename = genericController.encodeText(.fields["JSFilename"].defaultValue)
                    // instance.JSHead = genericController.encodeText(.fields["JSHead"].defaultValue)
                    // instance.JSOnLoad = genericController.encodeText(.fields["JSOnLoad"].defaultValue)
                    // instance.Link = genericController.encodeText(.fields["Link"].defaultValue)
                    // instance.MobileBodyHTML = genericController.encodeText(.fields["MobileBodyHTML"].defaultValue)
                    instance.ModifiedBy = genericController.encodeInteger(CDef.fields["ModifiedBy"].defaultValue);
                    instance.ModifiedDate = genericController.encodeDate(CDef.fields["ModifiedDate"].defaultValue);
                    instance.Name = genericController.encodeText(CDef.fields["Name"].defaultValue);
                    // instance.OtherHeadTags = genericController.encodeText(.fields["OtherHeadTags"].defaultValue)
                    instance.SortOrder = genericController.encodeText(CDef.fields["SortOrder"].defaultValue);
                    // instance.Source = genericController.encodeText(.fields["Source"].defaultValue)
                    // instance.StylesFilename = genericController.encodeText(.fields["StylesFilename"].defaultValue)
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
            }
            return instance;
        }
    }
}
