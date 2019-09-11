﻿
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Contensive.BaseClasses;
using System.Linq;
using Contensive.Exceptions;
//using Contensive.CPBase.Controllers;
//
namespace Contensive.Models.Db {
    //
    //====================================================================================================
    // db model pattern
    //   new() - empty constructor to allow deserialization
    //   saveObject() - saves instance properties (nonstatic method)
    //   create() - loads instance properties and returns a model 
    //   delete() - deletes the record that matches the argument
    //   getObjectList() - a pattern for creating model lists.
    //   invalidateFIELDNAMEcache() - method to invalide the model cache. One per cache
    //
    //	1) set the primary content name in const cnPrimaryContent. avoid constants Like cnAddons used outside model
    //	2) find-And-replace "_blankModel" with the name for this model
    //	3) when adding model fields, add in three places: the Public Property, the saveObject(), the loadObject()
    //	4) when adding create() methods to support other fields/combinations of fields, 
    //       - add a secondary cache For that new create method argument in loadObjec()
    //       - add it to the injected cachename list in loadObject()
    //       - add an invalidate
    //
    // Model Caching
    //   caching applies to model objects only, not lists of models (for now)
    //       - this is because of the challenge of invalidating the list object when individual records are added or deleted
    //
    //   a model should have 1 primary cache object which stores the data and can have other secondary cacheObjects which do not hold data
    //    the cacheName of the 'primary' cacheObject for models and db records (cacheNamePrefix + ".id." + #id)
    //    'secondary' cacheName is (cacheNamePrefix + . + fieldName + . + #)
    //
    //   cacheobjects can be used to hold data (primary cacheobjects), or to hold only metadata (secondary cacheobjects)
    //       - primary cacheobjects are like 'personModel.id.99' that holds the model for id=99
    //           - it is primary because the .primaryobject is null
    //           - invalidationData. This cacheobject is invalid after this datetime
    //           - dependentobjectlist() - this object is invalid if any of those objects are invalid
    //       - secondary cachobjects are like 'person.ccguid.12345678'. It does not hold data, just a reference to the primary cacheobject
    //
    //   cacheNames spaces are replaced with underscores, so "addon collections" should be addon_collections
    //
    //   cacheNames that match content names are treated as caches of "any" record in the content, so invalidating "people" can be used to invalidate
    //       any non-specific cache in the people table, by including "people" as a dependant cachename. the "people" cachename should not clear
    //       specific people caches, like people.id.99, but can be used to clear lists of records like "staff_list_group"
    //       - this can be used as a fallback strategy to cache record lists: a remote method list can be cached with a dependancy on "add-ons".
    //       - models should always clear this content name cache entry on all cache clears
    //
    //   when a model is created, the code first attempts to read the model's cacheobject. if it fails, it builds it and saves the cache object and tags
    //       - when building the model, is writes object to the primary cacheobject, and writes all the secondaries to be used
    //       - when building the model, if a database record is opened, a dependantObject Tag is created for the tablename+'id'+id
    //       - when building the model, if another model is added, that model returns its cachenames in the cacheNameList to be added as dependentObjects
    //
    //
    [System.Serializable]
    public class DbBaseModel : DbBaseFieldsModel {
        //
        //====================================================================================================
        //-- const must be set in derived clases
        //
        // -Public Const contentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
        // -Public Const contentDataSource As String = "" '<----- set to datasource if not default
        //
        //====================================================================================================
        //-- field types
        //
        [Serializable]
        public class FieldTypeFileBase {
            //
            // -- 
            // during load
            //   -- The filename is loaded into the model (blank or not). No content Is read from the file during load.
            //   -- the internalcp must be set
            //
            // during a cache load, the internalcp must be set
            //
            // content property read:
            //   -- If the filename Is blank, a blank Is returned
            //   -- if the filename exists, the content is read into the model and returned to the consumer
            //
            // content property written:
            //   -- content is stored in the model until save(). contentUpdated is set.
            //
            // filename property read: nothing special
            //
            // filename property written:
            //   -- contentUpdated set true if it was previously set (content was written), or if the content is not empty
            //
            // contentLoaded property means the content in the model is valid
            // contentUpdated property means the content needs to be saved on the next save
            //
            public string filename {
                set {
                    _filename = value;
                    //
                    // -- mark content updated if the content was updated, or if the content is not blank (so old content is written to the new updated filename)
                    contentUpdated = contentUpdated || (!string.IsNullOrEmpty(_content));
                }
                get {
                    return _filename;
                }
            }
            private string _filename = "";
            //
            // -- content in the file. loaded as needed, not during model create. 
            public string content {
                set {
                    _content = value;
                    contentUpdated = true;
                    contentLoaded = true;
                }
                get {
                    if (!contentLoaded) {
                        // todo if internalcp is not set, throw an error
                        if ((!string.IsNullOrEmpty(filename)) && (cpInternal != null)) {
                            contentLoaded = true;
                            _content = cpInternal.CdnFiles.Read(filename);
                        }
                    }
                    return _content;
                }
            }
            //
            // -- internal storage for content
            private string _content { get; set; } = "";
            //
            // -- When field is deserialized from cache, contentLoaded flag is used to deferentiate between unloaded content and blank conent.
            public bool contentLoaded { get; set; } = false;
            //
            // -- When content is updated, the model.save() writes the file
            public bool contentUpdated { get; set; } = false;
            //
            // -- set by load(). Used by field to read content from filename when needed
            [NonSerialized] public CPBaseClass cpInternal = null;
        }
        //
        [Serializable]
        public class FieldTypeTextFile : FieldTypeFileBase {
        }
        [Serializable]
        public class FieldTypeJavascriptFile : FieldTypeFileBase {
        }
        [Serializable]
        public class FieldTypeCSSFile : FieldTypeFileBase {
        }
        [Serializable]
        public class FieldTypeHTMLFile : FieldTypeFileBase {
        }
        //
        //====================================================================================================
        // -- instance properties (inherits basefieldmodel)
        ///// <summary>
        ///// identity integer, primary key for every table
        ///// </summary>
        //public int id { get; set; }
        ///// <summary>
        ///// name of the record used for lookup lists
        ///// </summary>
        //public string name { get; set; }
        ///// <summary>
        ///// optional guid, created automatically in the model
        ///// </summary>
        //public string ccguid { get; set; }
        ///// <summary>
        ///// optionally can be used to disable a record. Must be implemented in each query
        ///// </summary>
        //public bool active { get; set; }
        ///// <summary>
        ///// id of the metadata record in ccContent that controls the display and handing for this record
        ///// </summary>
        //public int contentControlID { get; set; }
        ///// <summary>
        ///// foreign key to ccmembers table, populated by admin when record added.
        ///// </summary>
        //public int createdBy { get; set; }
        ///// <summary>
        ///// used when creating new record
        ///// </summary>
        //public int createKey { get; set; }
        ///// <summary>
        ///// date record added, populated by admin when record added.
        ///// </summary>
        //public DateTime dateAdded { get; set; }
        ///// <summary>
        ///// foreign key to ccmembers table set to user who modified the record last in the admin site
        ///// </summary>
        //public int modifiedBy { get; set; }
        ///// <summary>
        ///// date when the record was last modified in the admin site
        ///// </summary>
        //public DateTime modifiedDate { get; set; }
        ///// <summary>
        ///// optionally used to sort recrods in the table
        ///// </summary>
        //public string sortOrder { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// simple constructor needed for deserialization
        /// </summary>
        public DbBaseModel() { }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the database table associated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public static string derivedTableName(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("contentTableNameLowerCase");
            if (fieldInfo == null) { throw new GenericException("Class [" + derivedType.Name + "] must declare constant [contentTableName]."); }
            return fieldInfo.GetRawConstantValue().ToString();
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the datasource assocated to the database table assocated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public static string derivedDataSourceName(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("contentDataSource");
            if (fieldInfo == null) { throw new GenericException("Class [" + derivedType.Name + "] must declare public constant [contentDataSource]."); }
            return fieldInfo.GetRawConstantValue().ToString();
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the boolean value of the constant nameIsUnique in the derived class. Setting true enables a name cache ptr.
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public static bool derivedNameFieldIsUnique(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("nameFieldIsUnique");
            if (fieldInfo == null) { throw new GenericException("Class [" + derivedType.Name + "] must declare public constant [nameFieldIsUnique]."); }
            return (bool)fieldInfo.GetRawConstantValue();
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static T addDefault<T>(CPBaseClass cp, Dictionary<string, string> DefaultValues) where T : DbBaseModel {
            var callersCacheNameList = new List<string>();
            return addDefault<T>(cp, DefaultValues, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new record to the db populated with default values from the content definition and return an object of it
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        public static T addDefault<T>(CPBaseClass cp, Dictionary<string, string> defaultValues, ref List<string> callersCacheNameList) where T : DbBaseModel {
            T result = default(T);
            try {
                result = addEmpty<T>(cp);
                if (result != null) {
                    foreach (PropertyInfo modelProperty in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        string propertyName = modelProperty.Name;
                        string propertyValue = "";
                        if (defaultValues.ContainsKey(propertyName.ToLowerInvariant())) {
                            propertyValue = defaultValues[propertyName.ToLowerInvariant()];
                            switch (propertyName.ToLowerInvariant()) {
                                case "id":
                                    // -- leave id as-is
                                    break;
                                case "ccguid":
                                    // -- leave guid created during addEmpty
                                    break;
                                case "active":
                                    // -- set true
                                    modelProperty.SetValue(result, true, null);
                                    break;
                                //case "contentcontrolid":
                                //    // -- set to content's id
                                //    modelProperty.SetValue(result, metaData.id, null);
                                //    break;
                                case "createdby":
                                    // -- set to current user if available
                                    if (cp.User != null) { modelProperty.SetValue(result, cp.User.Id, null); }
                                    break;
                                case "dateadded":
                                    // -- set to now
                                    modelProperty.SetValue(result, DateTime.Now, null);
                                    break;
                                case "modifiedby":
                                    // -- set to current user if available
                                    if (cp.User != null) { modelProperty.SetValue(result, cp.User.Id, null); }
                                    break;
                                case "modifieddate":
                                    // -- set to now
                                    modelProperty.SetValue(result, DateTime.Now, null);
                                    break;
                                default:
                                    switch (modelProperty.PropertyType.Name) {
                                        case "Int32": {
                                                modelProperty.SetValue(result, cp.Utils.EncodeInteger(propertyValue), null);
                                                break;
                                            }
                                        case "Boolean": {
                                                modelProperty.SetValue(result, cp.Utils.EncodeBoolean(propertyValue), null);
                                                break;
                                            }
                                        case "DateTime": {
                                                modelProperty.SetValue(result, cp.Utils.EncodeDate(propertyValue), null);
                                                break;
                                            }
                                        case "Double": {
                                                modelProperty.SetValue(result, cp.Utils.EncodeNumber(propertyValue), null);
                                                break;
                                            }
                                        case "String": {
                                                modelProperty.SetValue(result, propertyValue, null);
                                                break;
                                            }
                                        case "FieldTypeTextFile": {
                                                //
                                                // -- cdn files
                                                FieldTypeTextFile instanceFileType = new FieldTypeTextFile { filename = propertyValue };
                                                modelProperty.SetValue(result, instanceFileType);
                                                break;
                                            }
                                        case "FieldTypeJavascriptFile": {
                                                //
                                                // -- cdn files
                                                FieldTypeJavascriptFile instanceFileType = new FieldTypeJavascriptFile { filename = propertyValue };
                                                modelProperty.SetValue(result, instanceFileType);
                                                break;
                                            }
                                        case "FieldTypeCSSFile": {
                                                //
                                                // -- cdn files
                                                FieldTypeCSSFile instanceFileType = new FieldTypeCSSFile { filename = propertyValue };
                                                modelProperty.SetValue(result, instanceFileType);
                                                break;
                                            }
                                        case "FieldTypeHTMLFile": {
                                                //
                                                // -- private files
                                                FieldTypeHTMLFile instanceFileType = new FieldTypeHTMLFile { filename = propertyValue };
                                                modelProperty.SetValue(result, instanceFileType);
                                                break;
                                            }
                                        default: {
                                                modelProperty.SetValue(result, propertyValue, null);
                                                break;
                                            }
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new empty record to the db and return an object of it.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        public static T addEmpty<T>(CPBaseClass cp) where T : DbBaseModel {
            try {
                T result = default(T);
                if (isAppInvalid(cp)) { return result; }
                return create<T>(cp, cp.Db.Add(derivedTableName(typeof(T)), cp.User.Id));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public static T create<T>(CPBaseClass cp, int recordId) where T : DbBaseModel {
            var tempVar = new List<string>();
            return create<T>(cp, recordId, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public static T create<T>(CPBaseClass cp, int recordId, ref List<string> callersCacheNameList) where T : DbBaseModel {
            try {
                T result = default(T);
                if (isAppInvalid(cp)) { return result; }
                if (recordId <= 0) { return result; }
                result = readRecordCache<T>(cp, recordId);
                if (result == null) {
                    using (var dt = cp.Db.ExecuteQuery(getSelectSql<T>(cp, null, "(id=" + recordId + ")", ""))) {
                        if (dt != null) {
                            if (dt.Rows.Count > 0) { result = loadRecord<T>(cp, dt.Rows[0], ref callersCacheNameList); }
                        }
                    }
                }
                //
                // -- store cp in all extended fields that need it (file fields so content read can happen on demand instead of at load)
                if (result != null) {
                    foreach (PropertyInfo instanceProperty in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        switch (instanceProperty.PropertyType.Name) {
                            case "FieldTypeJavascriptFile": {
                                    FieldTypeJavascriptFile fileProperty = (FieldTypeJavascriptFile)instanceProperty.GetValue(result);
                                    fileProperty.cpInternal = cp;
                                    break;
                                }
                            case "FieldTypeCSSFile": {
                                    FieldTypeCSSFile fileProperty = (FieldTypeCSSFile)instanceProperty.GetValue(result);
                                    fileProperty.cpInternal = cp;
                                    break;
                                }
                            case "FieldTypeHTMLFile": {
                                    FieldTypeHTMLFile fileProperty = (FieldTypeHTMLFile)instanceProperty.GetValue(result);
                                    fileProperty.cpInternal = cp;
                                    break;
                                }
                            case "FieldTypeTextFile": {
                                    FieldTypeTextFile fileProperty = (FieldTypeTextFile)instanceProperty.GetValue(result);
                                    fileProperty.cpInternal = cp;
                                    break;
                                }
                        }
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with matching ccGuid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        /// <returns></returns>
        public static T create<T>(CPBaseClass cp, string recordGuid) where T : DbBaseModel {
            var cacheNameList = new List<string>();
            return create<T>(cp, recordGuid, ref cacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with a matching ccguid, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        public static T create<T>(CPBaseClass cp, string recordGuid, ref List<string> callersCacheNameList) where T : DbBaseModel {
            try {
                T result = default(T);
                if (isAppInvalid(cp)) { return result; }
                if (string.IsNullOrEmpty(recordGuid)) { return result; }
                result = readRecordCacheByGuidPtr<T>(cp, recordGuid);
                if (result != null) { return result; }
                using (var dt = cp.Db.ExecuteQuery(getSelectSql<T>(cp, null, "(ccGuid=" + cp.Db.EncodeSQLText(recordGuid) + ")", ""))) {
                    if (dt != null) {
                        if (dt.Rows.Count > 0) { return loadRecord<T>(cp, dt.Rows[0], ref callersCacheNameList); }
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public static T createByUniqueName<T>(CPBaseClass cp, string recordName) where T : DbBaseModel {
            var cacheNameList = new List<string>();
            return createByUniqueName<T>(cp, recordName, ref cacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordName"></param>
        /// <param name="callersCacheNameList">method will add the cache name to this list.</param>
        public static T createByUniqueName<T>(CPBaseClass cp, string recordName, ref List<string> callersCacheNameList) where T : DbBaseModel {
            try {
                T result = default(T);
                if (isAppInvalid(cp)) { return result; }
                if (!string.IsNullOrEmpty(recordName)) {
                    //
                    // -- if allowCache, then this subclass is for a content that has a unique name. read the name pointer
                    result = (derivedNameFieldIsUnique(typeof(T))) ? readRecordCacheByUniqueNamePtr<T>(cp, recordName) : null;
                    if (result == null) {
                        using (var dt = cp.Db.ExecuteQuery(getSelectSql<T>(cp, null, "(name=" + cp.Db.EncodeSQLText(recordName) + ")", ""))) {
                            if (dt != null) {
                                if (dt.Rows.Count > 0) {
                                    return loadRecord<T>(cp, dt.Rows[0], ref callersCacheNameList);
                                }
                            }
                        }
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// open an existing object
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="callersCacheKeyList"></param>
        private static T loadRecord<T>(CPBaseClass cp, DataRow row, ref List<string> callersCacheKeyList) where T : DbBaseModel {
            try {
                T modelInstance = default(T);
                if (row != null) {
                    Type instanceType = typeof(T);
                    modelInstance = (T)Activator.CreateInstance(instanceType);
                    foreach (PropertyInfo modelProperty in modelInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        string propertyName = modelProperty.Name;
                        string propertyValue = row[propertyName].ToString();
                        switch (propertyName.ToLowerInvariant()) {
                            case "specialcasefield":
                                break;
                            default:
                                switch (modelProperty.PropertyType.Name) {
                                    case "Int32": {
                                            modelProperty.SetValue(modelInstance, cp.Utils.EncodeInteger(propertyValue), null);
                                            break;
                                        }
                                    case "Boolean": {
                                            modelProperty.SetValue(modelInstance, cp.Utils.EncodeBoolean(propertyValue), null);
                                            break;
                                        }
                                    case "DateTime": {
                                            modelProperty.SetValue(modelInstance, cp.Utils.EncodeDate(propertyValue), null);
                                            break;
                                        }
                                    case "Double": {
                                            modelProperty.SetValue(modelInstance, cp.Utils.EncodeNumber(propertyValue), null);
                                            break;
                                        }
                                    case "String": {
                                            modelProperty.SetValue(modelInstance, propertyValue, null);
                                            break;
                                        }
                                    case "FieldTypeTextFile": {
                                            //
                                            // -- cdn files
                                            FieldTypeTextFile instanceFileType = new FieldTypeTextFile {
                                                filename = propertyValue,
                                                cpInternal = cp
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeJavascriptFile": {
                                            //
                                            // -- cdn files
                                            FieldTypeJavascriptFile instanceFileType = new FieldTypeJavascriptFile {
                                                filename = propertyValue,
                                                cpInternal = cp
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeCSSFile": {
                                            //
                                            // -- cdn files
                                            FieldTypeCSSFile instanceFileType = new FieldTypeCSSFile {
                                                filename = propertyValue,
                                                cpInternal = cp
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeHTMLFile": {
                                            //
                                            // -- private files
                                            FieldTypeHTMLFile instanceFileType = new FieldTypeHTMLFile {
                                                filename = propertyValue,
                                                cpInternal = cp
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    default: {
                                            modelProperty.SetValue(modelInstance, propertyValue, null);
                                            break;
                                        }
                                }
                                break;
                        }
                    }

                    if (modelInstance != null) {
                        //
                        // -- set primary cache to the object created
                        // -- set secondary caches to the primary cache
                        // -- add all cachenames to the injected cachenamelist
                        if (modelInstance is DbBaseModel) {
                            //DbBaseModel baseInstance;
                            string datasourceName = derivedDataSourceName(instanceType);
                            string tableName = derivedTableName(instanceType);
                            string cacheKey = cp.Cache.CreateKeyForDbRecord(modelInstance.id, tableName, datasourceName);
                            callersCacheKeyList.Add(cacheKey);
                            cp.Cache.Store(cacheKey, modelInstance);
                            //
                            string cachePtr = cp.Cache.CreatePtrKeyforDbRecordGuid(modelInstance.ccguid, tableName, datasourceName);
                            cp.Cache.StorePtr(cachePtr, cacheKey);
                            //
                            if (derivedNameFieldIsUnique(instanceType)) {
                                cachePtr = cp.Cache.CreatePtrKeyforDbRecordUniqueName(modelInstance.name, tableName, datasourceName);
                                cp.Cache.StorePtr(cachePtr, cacheKey);
                            }
                        }
                    }
                }
                return modelInstance;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public int save(CPBaseClass cp, bool asyncSave = false) {
            try {
                if (isAppInvalid(cp)) { return 0; }
                Type instanceType = this.GetType();
                string tableName = derivedTableName(instanceType);
                string datasourceName = derivedDataSourceName(instanceType);
                var sqlPairs = new SqlFieldListClass();
                foreach (PropertyInfo instanceProperty in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                    switch (instanceProperty.Name.ToLowerInvariant()) {
                        case "id":
                            break;
                        case "ccguid":
                            if (string.IsNullOrEmpty(ccguid)) { ccguid = cp.Utils.CreateGuid(); }
                            sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLText(instanceProperty.GetValue(this, null).ToString()));
                            break;
                        default:
                            CPContentBaseClass.fileTypeIdEnum fieldTypeId = 0;
                            bool fileFieldContentUpdated = false;
                            FieldTypeFileBase fileProperty = null;
                            switch (instanceProperty.PropertyType.Name) {
                                case "Int32":
                                    Int32 valueInt32 = (int)instanceProperty.GetValue(this);
                                    sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLNumber(valueInt32));
                                    break;
                                case "Boolean":
                                    bool valueBool = (bool)instanceProperty.GetValue(this);
                                    sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLBoolean(valueBool));
                                    break;
                                case "DateTime":
                                    DateTime valueDate = (DateTime)instanceProperty.GetValue(this, null);
                                    sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLDate(valueDate));
                                    break;
                                case "Double":
                                    double valueDbl = (double)instanceProperty.GetValue(this);
                                    sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLNumber(valueDbl));
                                    break;
                                case "String":
                                    string valueString = (string)instanceProperty.GetValue(this, null);
                                    sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLText(valueString));
                                    break;
                                case "FieldTypeTextFile": {
                                        fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileText;
                                        fileProperty = (FieldTypeTextFile)instanceProperty.GetValue(this);
                                    }
                                    break;
                                case "FieldTypeJavascriptFile": {
                                        fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileJavascript;
                                        fileProperty = (FieldTypeJavascriptFile)instanceProperty.GetValue(this);
                                    }
                                    break;
                                case "FieldTypeCSSFile": {
                                        fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileCSS;
                                        fileProperty = (FieldTypeCSSFile)instanceProperty.GetValue(this);
                                    }
                                    break;
                                case "FieldTypeHTMLFile": {
                                        fieldTypeId = CPContentBaseClass.fileTypeIdEnum.FileHTML;
                                        fileProperty = (FieldTypeHTMLFile)instanceProperty.GetValue(this);
                                    }
                                    break;
                                default:
                                    //
                                    // -- invalid field type
                                    throw new GenericException("Unsupported type [" + instanceProperty.PropertyType.Name + "] in Model [" + instanceType.Name + "] Property [" + instanceProperty.Name + "]");
                                    //sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLText(instanceProperty.GetValue(this, null)?.ToString()));
                                    //break;
                            }
                            if (!((int)fieldTypeId).Equals(0)) {
                                fileProperty.cpInternal = cp;
                                PropertyInfo fileFieldContentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                fileFieldContentUpdated = (bool)fileFieldContentUpdatedProperty.GetValue(fileProperty);
                                if (fileFieldContentUpdated) {
                                    PropertyInfo fileFieldFilenameProperty = instanceProperty.PropertyType.GetProperty("filename");
                                    string fileFieldFilename = (string)fileFieldFilenameProperty.GetValue(fileProperty);
                                    if ((String.IsNullOrEmpty(fileFieldFilename)) && (id != 0)) {
                                        // 
                                        // -- if record exists and file property's filename is not set, get the filename from the Db
                                        using (DataTable dt = cp.Db.ExecuteQuery("select " + instanceProperty.Name + " from " + tableName + " where (id=" + id + ")")) {
                                            if (dt.Rows.Count > 0) {
                                                fileFieldFilename = cp.Utils.EncodeText(dt.Rows[0][instanceProperty.Name]);
                                            }
                                        }
                                    }
                                    PropertyInfo fileFieldContentProperty = instanceProperty.PropertyType.GetProperty("content");
                                    string fileFieldContent = (string)fileFieldContentProperty.GetValue(fileProperty);
                                    if ((string.IsNullOrEmpty(fileFieldContent)) && (!string.IsNullOrEmpty(fileFieldFilename))) {
                                        //
                                        // -- empty content and valid filename, delete the file and clear the filename
                                        sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLText(string.Empty));
                                        cp.CdnFiles.DeleteFile(fileFieldFilename);
                                        fileFieldFilenameProperty.SetValue(fileProperty, string.Empty);
                                    } else {
                                        //
                                        // -- save content
                                        if (string.IsNullOrEmpty(fileFieldFilename)) {
                                            fileFieldFilename = cp.Db.CreateFieldPathFilename(tableName, instanceProperty.Name.ToLowerInvariant(), id, fieldTypeId);
                                            fileFieldFilenameProperty.SetValue(fileProperty, fileFieldFilename);
                                        }
                                        cp.CdnFiles.Save(fileFieldFilename, fileFieldContent);
                                        sqlPairs.add(instanceProperty.Name, cp.Db.EncodeSQLText(fileFieldFilename));
                                    }
                                }

                            }
                            break;
                    }
                }
                if (sqlPairs.count > 0) {
                    if (id == 0) { id = cp.Db.Add(tableName, cp.User.Id); }
                    cp.Db.Update(tableName, "(id=" + id.ToString() + ")", sqlPairs, asyncSave);
                }
                //
                // -- store the cache object referenced by id
                string cacheKey = cp.Cache.CreateKeyForDbRecord(id, tableName, datasourceName);
                cp.Cache.Store(cacheKey, this);
                //
                // -- store the cache object ptr so this object can be referenced from its guid (as well as id)
                cp.Cache.StorePtr(cp.Cache.CreatePtrKeyforDbRecordGuid(ccguid, tableName, datasourceName), cacheKey);
                //
                // -- if the name for this table is unique, store the cache object ptr for name so this object can be referenced by name
                if (derivedNameFieldIsUnique(instanceType)) cp.Cache.StorePtr(cp.Cache.CreatePtrKeyforDbRecordUniqueName(name, tableName, datasourceName), cacheKey);
                //
                // -- update the cache Last-Record-Modified-Date
                cp.Cache.UpdateLastModified(tableName);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
            return id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record by id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void delete<T>(CPBaseClass cp, int recordId) where T : DbBaseModel {
            try {
                if (isAppInvalid(cp)) { return; }
                if (recordId <= 0) { return; }
                string dataSourceName = derivedDataSourceName(typeof(T));
                string tableName = derivedTableName(typeof(T));
                cp.Db.Delete(tableName, recordId);
                cp.Cache.Invalidate(cp.Cache.CreateKeyForDbRecord(recordId, tableName, dataSourceName));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record by guid
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>
        public static void delete<T>(CPBaseClass cp, string guid) where T : DbBaseModel {
            try {
                if (isAppInvalid(cp)) { return; }
                if (string.IsNullOrEmpty(guid)) { return; }
                // todo change cache invalidate to key ptr, and we do not need to open the record first
                DbBaseModel instance = create<DbBaseModel>(cp, guid);
                if (instance == null) { return; }
                invalidateCacheOfRecord<T>(cp, instance.id);
                cp.Db.Delete(derivedTableName(typeof(T)), guid);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria and sort order, and add a cache object to an argument
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        public static List<T> createList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) where T : DbBaseModel {
            try {
                List<T> result = new List<T>();
                if (isAppInvalid(cp)) { return result; }
                using (var dt = cp.Db.ExecuteQuery(getSelectSql<T>(cp, null, sqlCriteria, sqlOrderBy))) {
                    foreach (DataRow row in dt.Rows) {
                        T instance = loadRecord<T>(cp, row, ref callersCacheNameList);
                        if (instance != null) { result.Add(instance); }
                    }
                }
                return result;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria and sort order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        public static List<T> createList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy) where T : DbBaseModel => createList<T>(cp, sqlCriteria, sqlOrderBy, new List<string> { });
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        public static List<T> createList<T>(CPBaseClass cp, string sqlCriteria) where T : DbBaseModel => createList<T>(cp, sqlCriteria, "id", new List<string> { });
        //
        //====================================================================================================
        //
        public static List<T> createList<T>(CPBaseClass cp) where T : DbBaseModel => createList<T>(cp, "", "id", new List<string> { });
        //
        //====================================================================================================
        //
        public static T createFirstOfList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy) where T : DbBaseModel {
            var list = createList<T>(cp, sqlCriteria, sqlOrderBy, new List<string>());
            if (list.Count == 0) { return null; }
            return list.First();
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record, returning empty string if the record is null
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>record
        /// <returns></returns>
        private static string getRecordName<T>(CPBaseClass cp, T record) where T : DbBaseModel {
            return (record != null) ? record.name : string.Empty;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>record
        /// <returns></returns>
        public static string getRecordName<T>(CPBaseClass cp, int recordId) where T : DbBaseModel {
            try {
                return getRecordName<T>(cp, create<T>(cp, recordId));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        public static string getRecordName<T>(CPBaseClass cp, string guid) where T : DbBaseModel {
            try {
                return getRecordName<T>(cp, create<T>(cp, guid));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the id of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        public static int getRecordId<T>(CPBaseClass cp, string guid) where T : DbBaseModel {
            try {
                var record = create<T>(cp, guid);
                if (record != null) { return record.id; }
                return 0;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty model object, populating only control fields (guid, active, created/modified)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static T createEmpty<T>(CPBaseClass cp) where T : DbBaseModel {
            try {
                T instance = (T)Activator.CreateInstance(typeof(T));
                if (isAppInvalid(cp)) { return instance; }
                DateTime rightNow = DateTime.Now;
                instance.GetType().GetProperty("active", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, 0, null);
                instance.GetType().GetProperty("ccguid", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, cp.Utils.CreateGuid(), null);
                instance.GetType().GetProperty("dateadded", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, rightNow, null);
                instance.GetType().GetProperty("modifieddate", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, rightNow, null);
                instance.GetType().GetProperty("createdby", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, cp.User.Id, null);
                instance.GetType().GetProperty("modifiedby", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, cp.User.Id, null);
                return instance;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an sql select for this model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="fieldList"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        public static string getSelectSql<T>(CPBaseClass cp, List<string> fieldList, string sqlCriteria, string sqlOrderBy) where T : DbBaseModel {
            try {
                if (fieldList == null) {
                    fieldList = new List<string>();
                    T instance = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo modelProperty in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        fieldList.Add(modelProperty.Name);
                    }
                }
                var sb = (new StringBuilder("select ")).Append(string.Join(",", fieldList.ToArray()))
                    .Append(" from ").Append(derivedTableName(typeof(T)))
                    .Append(" where (active>0)");
                if (!string.IsNullOrEmpty(sqlCriteria)) { sb.Append("and(" + sqlCriteria + ")"); }
                if (!string.IsNullOrEmpty(sqlOrderBy)) { sb.Append(" order by " + sqlOrderBy); }
                return sb.ToString();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        public static string getSelectSql<T>(CPBaseClass cp, List<string> fieldList, string criteria) where T : DbBaseModel => getSelectSql<T>(cp, fieldList, criteria, null);
        //
        public static string getSelectSql<T>(CPBaseClass cp, List<string> fieldList) where T : DbBaseModel => getSelectSql<T>(cp, fieldList, null, null);
        //
        public static string getSelectSql<T>(CPBaseClass cp) where T : DbBaseModel => getSelectSql<T>(cp, null, null, null);
        ////
        ////====================================================================================================
        ///// <summary>
        ///// create the cache key for the table cache
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="cp"></param>
        ///// <returns></returns>
        //public static string getTableCacheKey<T>(CPBaseClass cp) {
        //    try {
        //        return cp.Cache. CacheController.createCacheKey_TableObjectsInvalidationDate(derivedTableName(typeof(T)));
        //    } catch (Exception ex) {
        //        cp.Site.ErrorReport( ex);
        //        throw;
        //    }
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Delete a selection of records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        public static void deleteRows<T>(CPBaseClass cp, string sqlCriteria) where T : DbBaseModel {
            try {
                if (isAppInvalid(cp)) { return; }
                if (string.IsNullOrEmpty(sqlCriteria)) { return; }
                cp.Db.DeleteRows(derivedTableName(typeof(T)), sqlCriteria);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// if invalid application, log the error and return true
        /// </summary>
        /// <returns></returns>
        private static bool isAppInvalid(CPBaseClass cp) {
            return false;
            //
            //if ((cp.serverConfig != null) && (cp.appConfig != null)) { return false; }
            //cp.Site.ErrorReport( new GenericException("Cannot use data models without a valid server and application configuration."));
            //return true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if this type contains the field specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static bool containsField<T>(string fieldName) {
            foreach (PropertyInfo instanceProperty in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                if (instanceProperty.Name.ToLower() == "parentid") { return true; }
            }
            return false;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if this instance is a parent record of the provided child record, using this table's parentId
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="childRecordId"></param>
        /// <returns></returns>
        public bool isParentOf<T>(CPBaseClass cp, int childRecordId) {
            if (id == childRecordId) { return true; }
            var usedIdList = new List<int>();
            return isParentOf<T>(cp, id, childRecordId, new List<int>());
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if the parent record provided is heriarctical parent of hte child record provided.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="parentRecordId"></param>
        /// <param name="childRecordId"></param>
        /// <param name="childIdList"></param>
        public static bool isParentOf<T>(CPBaseClass cp, int parentRecordId, int childRecordId, List<int> childIdList) {
            if (parentRecordId == childRecordId) return true;
            if (!containsField<T>("parentid")) { return false; }
            if (childIdList.Contains(childRecordId)) { return false; }
            using (DataTable dt = cp.Db.ExecuteQuery("select parentId from " + derivedTableName(typeof(T)) + " where id=" + childRecordId)) {
                if (dt != null) {
                    if (dt.Rows.Count > 0) {
                        childIdList.Add(parentRecordId);
                        return isParentOf<T>(cp, parentRecordId, cp.Utils.EncodeInteger(dt.Rows[0]["parentid"]), childIdList);
                    }
                }
                return false;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if this instance is the child record of the provided parentid. (the parentid record is in the parent hierarchy)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="parentRecordId"></param>
        /// <returns></returns>
        public bool isChildOf<T>(CPBaseClass cp, int parentRecordId) {
            if (id == parentRecordId) { return true; }
            var usedIdList = new List<int>();
            return isChildOf<T>(cp, id, parentRecordId, new List<int>());
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if the parent record provided is heriarctical parent of hte child record provided.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="parentRecordId"></param>
        /// <param name="childRecordId"></param>
        /// <param name="parentIdList"></param>
        public static bool isChildOf<T>(CPBaseClass cp, int parentRecordId, int childRecordId, List<int> parentIdList) {
            if (!containsField<T>("parentid")) { return false; }
            if (parentIdList.Contains(childRecordId)) { return false; }
            if (parentRecordId == childRecordId) return true;
            using (DataTable dt = cp.Db.ExecuteQuery("select id from " + derivedTableName(typeof(T)) + " where parentId=" + parentRecordId)) {
                if (dt != null) {
                    if (dt.Rows.Count > 0) {
                        parentIdList.Add(parentRecordId);
                        return isChildOf<T>(cp, parentRecordId, cp.Utils.EncodeInteger(dt.Rows[0]["parentid"]), parentIdList);
                    }
                }
                return false;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the cache entry for a record
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void invalidateCacheOfRecord<T>(CPBaseClass cp, int recordId) where T : DbBaseModel {
            try {
                if (isAppInvalid(cp)) { return; }
                cp.Cache.InvalidateTableRecord(derivedTableName(typeof(T)), recordId);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate all cache entries for the table (set invalidate-table-objects-date for this table -- see cache controller for definitions)
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public static void invalidateCacheOfTable<T>(CPBaseClass cp) where T : DbBaseModel {
            try {
                if (isAppInvalid(cp)) { return; }
                cp.Cache.InvalidateTable(derivedTableName(typeof(T)));
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create the Cache key used to invalidate all records in this table. All record cache stores should use this as a dependency, and
        /// in the event that a change is made across undeterminate records, invalidate this key to clear all cache from this table.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <returns></returns>
        public static string createDependencyKeyInvalidateOnChange<T>(CPBaseClass cp) where T : DbBaseModel {
            return cp.Cache.CreateDependencyKeyInvalidateOnChange(derivedTableName(typeof(T)));
        }
        //
        //====================================================================================================
        /// <summary>
        /// Read record cache by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        private static T readRecordCache<T>(CPBaseClass cp, int recordId) where T : DbBaseModel {
            T result = cp.Cache.GetObject<T>(cp.Cache.CreateKeyForDbRecord(recordId, derivedTableName(typeof(T)), derivedDataSourceName(typeof(T))));
            restoreCacheDataObjects(cp, result);
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Read a record cache by guid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="ccGuid"></param>
        /// <returns></returns>
        private static T readRecordCacheByGuidPtr<T>(CPBaseClass cp, string ccGuid) where T : DbBaseModel {
            T result = cp.Cache.GetObject<T>(cp.Cache.CreatePtrKeyforDbRecordGuid(ccGuid, derivedTableName(typeof(T)), derivedDataSourceName(typeof(T))));
            restoreCacheDataObjects(cp, result);
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Read a record cache using unique name (valid only if hasUniqueName is true)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cp"></param>
        /// <param name="uniqueName"></param>
        /// <returns></returns>
        private static T readRecordCacheByUniqueNamePtr<T>(CPBaseClass cp, string uniqueName) where T : DbBaseModel {
            T result = cp.Cache.GetObject<T>(cp.Cache.CreatePtrKeyforDbRecordUniqueName(uniqueName, derivedTableName(typeof(T)), derivedDataSourceName(typeof(T))));
            restoreCacheDataObjects(cp, result);
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// After reading a cached Db object, go through instance properties and verify internal cp objects populated
        /// </summary>
        private static void restoreCacheDataObjects<T>(CPBaseClass cp, T restoredInstance) {
            if (restoredInstance == null) { return; }
            foreach (PropertyInfo instanceProperty in restoredInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                // todo change test to is-subsclass-of-fieldCdnFile
                switch (instanceProperty.PropertyType.Name) {
                    case "FieldTypeTextFile":
                    case "FieldTypeCSSFile":
                    case "FieldTypeHTMLFile":
                    case "FieldTypeJavascriptFile":
                        FieldTypeFileBase fileProperty = (FieldTypeFileBase)instanceProperty.GetValue(restoredInstance);
                        fileProperty.cpInternal = cp;
                        break;
                }
            }
        }
    }
}
