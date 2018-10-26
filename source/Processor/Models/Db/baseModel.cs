
using System;
using System.Reflection;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using static Contensive.Processor.constants;
using System.Data;
//
namespace Contensive.Processor.Models.Db {
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
    public abstract class BaseModel {
        //
        //====================================================================================================
        //-- const must be set in derived clases
        //
        //Public Const contentName As String = "" '<------ set content name
        //Public Const contentTableName As String = "" '<------ set to tablename for the primary content (used for cache names)
        //Public Const contentDataSource As String = "" '<----- set to datasource if not default
        //
        //====================================================================================================
        //-- field types
        //
        public abstract class FieldCdnFile {
            //
            // -- 
            // during load
            //   -- The filename is loaded into the model (blank or not). No content Is read from the file during load.
            //   -- the internalcore must be set
            //
            // during a cache load, the internalcore must be set
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
                }
                get {
                    if (!contentLoaded) {
                        // todo if internalcore is not set, throw an error
                        if ((!string.IsNullOrEmpty(filename)) && (internalcore != null)) {
                            contentLoaded = true;
                            _content = internalcore.cdnFiles.readFileText(filename);
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
            public CoreController internalcore { get; set; } = null;
        }
        //
        public class FieldTypeTextFile : FieldCdnFile {
        }
        public class FieldTypeJavascriptFile : FieldCdnFile {
        }
        public class FieldTypeCSSFile : FieldCdnFile {
        }
        public class FieldTypeHTMLFile : FieldCdnFile {
        }
        //
        //====================================================================================================
        // -- instance properties
        /// <summary>
        /// identity integer, primary key for every table
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// name of the record used for lookup lists
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// optional guid, created automatically in the model
        /// </summary>
        public string ccguid { get; set; }
        /// <summary>
        /// optionally can be used to disable a record. Must be implemented in each query
        /// </summary>
        public bool active { get; set; }
        /// <summary>
        /// id of the metadata record in ccContent that controls the display and handing for this record
        /// </summary>
        public int contentControlID { get; set; }
        /// <summary>
        /// foreign key to ccmembers table, populated by admin when record added.
        /// </summary>
        public int createdBy { get; set; }
        /// <summary>
        /// used when creating new record
        /// </summary>
        public int createKey { get; set; }
        /// <summary>
        /// date record added, populated by admin when record added.
        /// </summary>
        public DateTime dateAdded { get; set; }
        /// <summary>
        /// foreign key to ccmembers table set to user who modified the record last in the admin site
        /// </summary>
        public int modifiedBy { get; set; }
        /// <summary>
        /// date when the record was last modified in the admin site
        /// </summary>
        public DateTime modifiedDate { get; set; }
        /// <summary>
        /// optionally used to sort recrods in the table
        /// </summary>
        public string sortOrder { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the content (metadata for the table) for the derived class
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        private static string derivedContentName(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("contentName");
            if (fieldInfo == null) {
                throw new ApplicationException("Class [" + derivedType.Name + "] must declare constant [contentName].");
            } else {
                return fieldInfo.GetRawConstantValue().ToString();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the database table associated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        private static string derivedTableName(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("contentTableName");
            if (fieldInfo == null) {
                throw new ApplicationException("Class [" + derivedType.Name + "] must declare constant [contentTableName].");
            } else {
                return fieldInfo.GetRawConstantValue().ToString();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the datasource assocated to the database table assocated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        private static string derivedDataSourceName(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("contentDataSource");
            if (fieldInfo == null) {
                throw new ApplicationException("Class [" + derivedType.Name + "] must declare public constant [contentDataSource].");
            } else {
                return fieldInfo.GetRawConstantValue().ToString();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the boolean value of the constant nameIsUnique in the derived class. Setting true enables a name cache ptr.
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        private static bool derivedNameFieldIsUnique(Type derivedType) {
            FieldInfo fieldInfo = derivedType.GetField("nameFieldIsUnique");
            if (fieldInfo == null) {
                throw new ApplicationException("Class [" + derivedType.Name + "] must declare public constant [nameFieldIsUnique].");
            } else {
                return (bool)fieldInfo.GetRawConstantValue();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// simple constructor needed for deserialization
        /// </summary>
        public BaseModel() {
            //
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        protected static T addDefault<T>(CoreController core, Domain.CDefModel cdef) where T : BaseModel {
            var callersCacheNameList = new List<string>();
            return addDefault<T>(core, cdef, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new record to the db populated with default values from the content definition and return an object of it
        /// </summary>
        /// <param name="core"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        protected static T addDefault<T>(CoreController core, Domain.CDefModel cdef, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                result = addEmpty<T>(core);
                if ( result != null ) {
                    foreach (PropertyInfo modelProperty in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        string propertyName = modelProperty.Name;
                        string propertyValue = "";
                        if (cdef.fields.ContainsKey(propertyName.ToLowerInvariant())) {
                            propertyValue = cdef.fields[propertyName.ToLowerInvariant()].defaultValue;
                            switch (propertyName.ToLowerInvariant()) {
                                case "specialcasefield":
                                    break;
                                default:
                                    switch (modelProperty.PropertyType.Name) {
                                        case "Int32": {
                                                modelProperty.SetValue(result, GenericController.encodeInteger(propertyValue), null);
                                                break;
                                            }
                                        case "Boolean": {
                                                modelProperty.SetValue(result, GenericController.encodeBoolean(propertyValue), null);
                                                break;
                                            }
                                        case "DateTime": {
                                                modelProperty.SetValue(result, GenericController.encodeDate(propertyValue), null);
                                                break;
                                            }
                                        case "Double": {
                                                modelProperty.SetValue(result, GenericController.encodeNumber(propertyValue), null);
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
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new empty record to the db and return an object of it.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        protected static T addEmpty<T>(CoreController core) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    Type instanceType = typeof(T);
                    result = create<T>(core, core.db.insertTableRecordGetId(derivedDataSourceName(instanceType), derivedTableName(instanceType), core.session.user.id));
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        protected static T create<T>(CoreController core, int recordId) where T : BaseModel {
            var tempVar = new List<string>();
            return create<T>(core, recordId, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        protected static T create<T>(CoreController core, int recordId, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (recordId > 0) {
                        result = readRecordCache<T>(core, recordId);
                        if (result == null) {
                            using (var cs = new CsController(core)) {
                                using (var dt = core.db.executeQuery(getSelectSql<T>(core, null, "(id=" + recordId + ")", ""))) {
                                    if (dt != null) {
                                        if (dt.Rows.Count > 0) {
                                            result = loadRecord<T>(core, dt.Rows[0], ref callersCacheNameList);
                                        }
                                    }
                                }
                            }
                        }
                        //
                        // -- store core in all extended fields that need it (file fields so content read can happen on demand instead of at load)
                        if (result != null) {
                            foreach (PropertyInfo instanceProperty in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                                switch (instanceProperty.PropertyType.Name) {
                                    case "FieldTypeJavascriptFile": {
                                        FieldTypeJavascriptFile fileProperty = (FieldTypeJavascriptFile)instanceProperty.GetValue(result);
                                        fileProperty.internalcore = core;
                                        break;
                                    }
                                    case "FieldTypeCSSFile": {
                                        FieldTypeCSSFile fileProperty = (FieldTypeCSSFile)instanceProperty.GetValue(result);
                                        fileProperty.internalcore = core;
                                        break;
                                    }
                                    case "FieldTypeHTMLFile": {
                                        FieldTypeHTMLFile fileProperty = (FieldTypeHTMLFile)instanceProperty.GetValue(result);
                                        fileProperty.internalcore = core;
                                        break;
                                    }
                                    case "FieldTypeTextFile": {
                                        FieldTypeTextFile fileProperty = (FieldTypeTextFile)instanceProperty.GetValue(result);
                                        fileProperty.internalcore = core;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with matching ccGuid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="recordGuid"></param>
        /// <returns></returns>
        protected static T create<T>(CoreController core, string recordGuid) where T : BaseModel {
            var tempVar = new List<string>();
            return create<T>(core, recordGuid, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with a matching ccguid, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        protected static T create<T>(CoreController core, string recordGuid, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(recordGuid)) {
                        Type instanceType = typeof(T);
                        result = readRecordCacheByGuidPtr<T>(core, recordGuid);
                        if (result == null) {
                            using (var cs = new CsController(core)) {
                                using (var dt = core.db.executeQuery(getSelectSql<T>(core, null, "(ccGuid=" + core.db.encodeSQLText(recordGuid) + ")", ""))) {
                                    if (dt != null) {
                                        if (dt.Rows.Count > 0) {
                                            result = loadRecord<T>(core, dt.Rows[0], ref callersCacheNameList);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        protected static T createByUniqueName<T>(CoreController core, string recordName) where T : BaseModel {
            var cacheNameList = new List<string>();
            return createByUniqueName<T>(core, recordName, ref cacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordName"></param>
        /// <param name="callersCacheNameList">method will add the cache name to this list.</param>
        protected static T createByUniqueName<T>(CoreController core, string recordName, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(recordName)) {
                        Type instanceType = typeof(T);
                        //
                        // -- if allowCache, then this subclass is for a content that has a unique name. read the name pointer
                        result = (derivedNameFieldIsUnique(instanceType)) ? readRecordCacheByUniqueNamePtr<T>(core, recordName) : null;
                        if (result == null) {
                            using ( var dt = core.db.executeQuery(getSelectSql<T>(core, null, "(name=" + core.db.encodeSQLText(recordName) + ")", ""))) {
                                if ( dt != null ) {
                                    if ( dt.Rows.Count>0 ) {
                                        result = loadRecord<T>(core, dt.Rows[0], ref callersCacheNameList);
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        /// <param name="callersCacheKeyList"></param>
        private static T loadRecord<T>(CoreController core, DataRow row, ref List<string> callersCacheKeyList) where T : BaseModel {
            T modelInstance = default(T);
            try {
                if (row != null ) {
                    //filename = GenericController.encodeText(dt.Rows[0][instanceProperty.Name]);
                    Type instanceType = typeof(T);
                    //string contentName = derivedContentName(instanceType);
                    string tableName = derivedTableName(instanceType);
                    int recordId = GenericController.encodeInteger(row["id"]);
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
                                            modelProperty.SetValue(modelInstance, GenericController.encodeInteger(propertyValue), null);
                                            break;
                                        }
                                    case "Boolean": {
                                            modelProperty.SetValue(modelInstance, GenericController.encodeBoolean(propertyValue), null);
                                            break;
                                        }
                                    case "DateTime": {
                                            modelProperty.SetValue(modelInstance, GenericController.encodeDate(propertyValue), null);
                                            break;
                                        }
                                    case "Double": {
                                            modelProperty.SetValue(modelInstance, GenericController.encodeNumber(propertyValue) , null);
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
                                                 internalcore = core
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeJavascriptFile": {
                                            //
                                            // -- cdn files
                                            FieldTypeJavascriptFile instanceFileType = new FieldTypeJavascriptFile {
                                                filename = propertyValue,
                                                internalcore = core
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeCSSFile": {
                                            //
                                            // -- cdn files
                                            FieldTypeCSSFile instanceFileType = new FieldTypeCSSFile {
                                                filename = propertyValue,
                                                internalcore = core
                                            };
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "FieldTypeHTMLFile": {
                                            //
                                            // -- private files
                                            FieldTypeHTMLFile instanceFileType = new FieldTypeHTMLFile {
                                                filename = propertyValue,
                                                internalcore = core
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
                        if (modelInstance is BaseModel baseInstance) {
                            string datasourceName = derivedDataSourceName(instanceType);
                            string cacheKey = CacheController.getCacheKey_forDbRecord(baseInstance.id, tableName, datasourceName);
                            callersCacheKeyList.Add(cacheKey);
                            core.cache.setObject(cacheKey, modelInstance);
                            //
                            string cachePtr = CacheController.getCachePtr_forDbRecord_guid(baseInstance.ccguid, tableName, datasourceName);
                            core.cache.setPtr(cachePtr, cacheKey);
                            //
                            if (derivedNameFieldIsUnique(instanceType)) {
                                cachePtr = CacheController.getCachePtr_forDbRecord_uniqueName(baseInstance.name, tableName, datasourceName);
                                core.cache.setPtr(cachePtr, cacheKey);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return modelInstance;
        }
        //
        //====================================================================================================
        /// <summary>
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        protected int save(CoreController core, bool asyncSave = false) {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    //CsController cs = new CsController(core);
                    Type instanceType = this.GetType();
                    string tableName = derivedTableName(instanceType);
                    string datasourceName = derivedDataSourceName(instanceType);
                    var sqlPairs = new SqlFieldListClass();
                    foreach (PropertyInfo instanceProperty in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        switch (instanceProperty.Name.ToLowerInvariant()) {
                            case "id":
                                //id = cs.getInteger("id");
                                break;
                            case "ccguid":
                                if (string.IsNullOrEmpty(ccguid)) {
                                    ccguid = Controllers.GenericController.getGUID();
                                }
                                //string value = instanceProperty.GetValue(this, null).ToString();
                                sqlPairs.add(instanceProperty.Name, core.db.encodeSQLText(instanceProperty.GetValue(this, null).ToString()));
                                //cs.setField(instanceProperty.Name, instanceProperty.GetValue(this, null).ToString());
                                break;
                            default:
                                switch (instanceProperty.PropertyType.Name) {
                                    case "Int32":
                                        Int32 valueInt32;
                                        int.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueInt32);
                                        sqlPairs.add(instanceProperty.Name, core.db.encodeSQLNumber(valueInt32));
                                        //cs.setField(instanceProperty.Name, valueInt32);
                                        break;
                                    case "Boolean":
                                        bool valueBool;
                                        bool.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueBool);
                                        sqlPairs.add(instanceProperty.Name, core.db.encodeSQLBoolean( valueBool ) );
                                        //cs.setField(instanceProperty.Name, valueBool);
                                        break;
                                    case "DateTime":
                                        DateTime valueDate;
                                        DateTime.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueDate);
                                        sqlPairs.add(instanceProperty.Name, core.db.encodeSQLDate( valueDate ));
                                        //cs.setField(instanceProperty.Name, valueDate);
                                        break;
                                    case "Double":
                                        double valueDbl;
                                        double.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueDbl);
                                        sqlPairs.add(instanceProperty.Name, core.db.encodeSQLNumber(valueDbl));
                                        //cs.setField(instanceProperty.Name, valueDbl);
                                        break;
                                    case "FieldTypeTextFile":
                                    case "FieldTypeJavascriptFile":
                                    case "FieldTypeCSSFile":
                                    case "FieldTypeHTMLFile":
                                        int fieldTypeId = 0;
                                        PropertyInfo contentProperty = null;
                                        PropertyInfo contentUpdatedProperty = null;
                                        bool contentUpdated = false;
                                        string content = "";
                                        switch (instanceProperty.PropertyType.Name) {
                                            case "FieldTypeJavascriptFile": {
                                                    fieldTypeId = fieldTypeIdFileJavascript;
                                                    FieldTypeJavascriptFile fileProperty = (FieldTypeJavascriptFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            case "FieldTypeCSSFile": {
                                                    fieldTypeId = fieldTypeIdFileCSS;
                                                    FieldTypeCSSFile fileProperty = (FieldTypeCSSFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            case "FieldTypeHTMLFile": {
                                                    fieldTypeId = fieldTypeIdFileHTML;
                                                    FieldTypeHTMLFile fileProperty = (FieldTypeHTMLFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            default: {
                                                    fieldTypeId = fieldTypeIdFileText;
                                                    FieldTypeTextFile fileProperty = (FieldTypeTextFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                        }
                                        if (contentUpdated) {
                                            string filename = "";
                                            //filename = cs.getValue(instanceProperty.Name);
                                            // 
                                            // -- if record exists, we have to get the field filename
                                            if (id==0) {
                                                using (System.Data.DataTable dt = core.db.executeQuery("select " + instanceProperty.Name + " from " + tableName + " where (id=" + id + ")")) {
                                                    if (dt.Rows.Count > 0) {
                                                        filename = GenericController.encodeText(dt.Rows[0][instanceProperty.Name]);
                                                    }
                                                }
                                            }
                                            if (string.IsNullOrEmpty(content)) {
                                                //
                                                // -- empty content
                                                if (!string.IsNullOrEmpty(filename)) {
                                                    sqlPairs.add(instanceProperty.Name, "" );
                                                    //cs.setField(instanceProperty.Name, "");
                                                    core.cdnFiles.deleteFile(filename);
                                                }
                                            } else {
                                                //
                                                // -- save content
                                                if (string.IsNullOrEmpty(filename)) {
                                                    filename = FileController.getVirtualRecordUnixPathFilename(tableName, instanceProperty.Name.ToLowerInvariant(), id, fieldTypeId);
                                                }
                                                core.cdnFiles.saveFile(filename, content);
                                                sqlPairs.add(instanceProperty.Name, core.db.encodeSQLText(filename) );
                                                //cs.setFieldFilename(instanceProperty.Name, filename);
                                            }
                                        }
                                        break;
                                    default:
                                        //string value = instanceProperty.GetValue(this, null).ToString();
                                        sqlPairs.add(instanceProperty.Name, core.db.encodeSQLText(instanceProperty.GetValue(this, null).ToString()));
                                        //cs.setField(instanceProperty.Name, instanceProperty.GetValue(this, null).ToString());
                                        break;
                                }
                                break;
                        }
                    }
                    if (sqlPairs.count>0) {
                        if (id == 0) {
                            //
                            // -- insert
                            core.db.insertTableRecord(datasourceName, tableName, sqlPairs, asyncSave);
                        } else {
                            //
                            // -- update
                            core.db.updateTableRecord(datasourceName, tableName, "(id=" + id.ToString() + ")", sqlPairs, asyncSave);
                        }
                    }
                    //cs.close(asyncSave);
                    //
                    // -- object is here, but the cache was invalidated, setting
                    string cacheKey = CacheController.getCacheKey_forDbRecord(id, tableName, datasourceName);
                    core.cache.setObject(cacheKey, this);
                    core.cache.setPtr(CacheController.getCachePtr_forDbRecord_guid(ccguid, tableName, datasourceName), cacheKey);
                    if (derivedNameFieldIsUnique(instanceType)) core.cache.setPtr(CacheController.getCachePtr_forDbRecord_uniqueName(name, tableName, datasourceName), cacheKey);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        protected static void delete<T>(CoreController core, int recordId) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (recordId > 0) {
                        Type instanceType = typeof(T);
                        string dataSourceName = derivedDataSourceName(instanceType);
                        string tableName = derivedTableName(instanceType);
                        core.db.deleteTableRecord(recordId, tableName, dataSourceName);
                        core.cache.invalidate(CacheController.getCacheKey_forDbRecord(recordId, tableName, derivedDataSourceName(instanceType)));
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        protected static void delete<T>(CoreController core, string ccguid) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(ccguid)) {
                        Type instanceType = typeof(T);
                        BaseModel instance = create<BaseModel>(core, ccguid);
                        if (instance != null) {
                            invalidateRecordCache<T>(core, instance.id);
                            core.db.deleteTableRecord(ccguid, derivedTableName(instanceType), derivedDataSourceName( instanceType ));
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        protected static List<T> createList<T>(CoreController core, string sqlCriteria) where T : BaseModel {
            return createList<T>(core, sqlCriteria, "id", new List<string> { });
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria and sort order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        protected static List<T> createList<T>(CoreController core, string sqlCriteria, string sqlOrderBy) where T : BaseModel {
            return createList<T>(core, sqlCriteria, sqlOrderBy, new List<string> { });
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria and sort order, and add a cache object to an argument
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        protected static List<T> createList<T>(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) where T : BaseModel {
            List<T> result = new List<T>();
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    using (var dt = core.db.executeQuery(getSelectSql<T>(core, null, sqlCriteria, sqlOrderBy))) {
                        T instance = default(T);
                        foreach (DataRow row in dt.Rows) {
                            instance = loadRecord<T>(core, row, ref callersCacheNameList);
                            if (instance != null) {
                                result.Add(instance);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the cache entry for a record
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        protected static void invalidateRecordCache<T>(CoreController core, int recordId) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    object instanceType = typeof(T);
                    core.cache.invalidateDbRecord( recordId, derivedTableName((Type)instanceType));
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate all cache entries for the table
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        protected static void invalidateTableCache<T>(CoreController core) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    object instanceType = typeof(T);
                    core.cache.invalidateAllKeysInTable(derivedTableName((Type)instanceType));
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>record
        /// <returns></returns>
        protected static string getRecordName<T>(CoreController core, int recordId) where T : BaseModel {
            try {
                var record = create<BaseModel>(core, recordId);
                if ( record != null ) {
                    return record.name;
                }
                return "";
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        protected static string getRecordName<T>(CoreController core, string guid) where T : BaseModel {
            try {
                var record = create<BaseModel>(core, guid);
                if (record != null) {
                    return record.name;
                }
                return "";
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the id of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        protected static int getRecordId<T>(CoreController core, string guid) where T : BaseModel {
            try {
                var record = create<BaseModel>(core, guid);
                if (record != null) {
                    return record.id;
                }
                return 0;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return 0;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty model object, populating only control fields (guid, active, created/modified)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        protected static T createEmpty<T>( CoreController core ) where T : BaseModel {
            T instance = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    LogController.handleError(core, new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    Type instanceType = typeof(T);
                    instance = (T)Activator.CreateInstance(instanceType);
                    string guid = GenericController.getGUID();
                    DateTime rightNow = DateTime.Now;
                    instance.GetType().GetProperty("ccguid", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, guid, null);
                    instance.GetType().GetProperty("dateadded", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, rightNow, null);
                    instance.GetType().GetProperty("createdby", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, core.session.user.id, null);
                    instance.GetType().GetProperty("modifieddate", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, rightNow, null);
                    instance.GetType().GetProperty("modifiedby", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, core.session.user.id, null);
                    instance.GetType().GetProperty("ccguid", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, "", null);
                    instance.GetType().GetProperty("active", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, 0, null);
                    //instance.GetType().GetProperty("contentcontrolid", BindingFlags.Instance | BindingFlags.Public).SetValue(instance, 0, null);
                }
            } catch (Exception) {
                throw;
            }
            return instance;
        }
        //
        //====================================================================================================
        //
        private static T readRecordCache<T>(CoreController core, int recordId) where T : BaseModel {
            Type instanceType = typeof(T);
            T result = core.cache.getObject<T>(CacheController.getCacheKey_forDbRecord(recordId, derivedTableName(instanceType), derivedDataSourceName(instanceType)));
            restoreCacheDataObjects(core, result);
            return result;
        }
        //
        //====================================================================================================
        //
        private static T readRecordCacheByGuidPtr<T>(CoreController core, string ccGuid) where T : BaseModel {
            Type instanceType = typeof(T);
            T result = core.cache.getObject<T>(CacheController.getCachePtr_forDbRecord_guid(ccGuid, derivedTableName(instanceType), derivedDataSourceName(instanceType)));
            restoreCacheDataObjects(core, result);
            return result;
        }
        //
        //====================================================================================================
        //
        private static T readRecordCacheByUniqueNamePtr<T>(CoreController core, string uniqueName) where T : BaseModel {
            Type instanceType = typeof(T);
            T result = core.cache.getObject<T>(CacheController.getCachePtr_forDbRecord_uniqueName(uniqueName, derivedTableName(instanceType), derivedDataSourceName(instanceType)));
            restoreCacheDataObjects(core, result);
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// After reading a cached Db object, go through instance properties and verify internal core objects populated
        /// </summary>
        private static void restoreCacheDataObjects<T>( CoreController core, T restoredInstance ) {
            if (restoredInstance != null) {
                foreach (PropertyInfo instanceProperty in restoredInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                    // todo change test to is-subsclass-of-fieldCdnFile
                    switch (instanceProperty.PropertyType.Name) {
                        case "FieldTypeTextFile":
                        case "FieldTypeCSSFile":
                        case "FieldTypeHTMLFile":
                        case "FieldTypeJavascriptFile":
                            FieldCdnFile fileProperty = (FieldCdnFile)instanceProperty.GetValue(restoredInstance);
                            fileProperty.internalcore = core;
                            break;
                    }
                }
            }
        }
        //
        //====================================================================================================
        //
        private static string getSelectSql<T>( CoreController core, List<string> fieldList = null, string criteria = "", string orderBy = "") where T : BaseModel {
            string result = "";
            Type instanceType = typeof(T);
            string tableName = derivedTableName(instanceType);
            if (fieldList == null) {
                fieldList = new List<string>();
                T modelInstance = (T)Activator.CreateInstance(instanceType);
                foreach (PropertyInfo modelProperty in modelInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                    fieldList.Add(modelProperty.Name);
                }
            }
            result = "select " + string.Join(",", fieldList.ToArray()) + " from " + tableName + " where (active>0)";
            if (!string.IsNullOrEmpty(criteria)) {
                result += "and(" + criteria + ")";
            }
            if (!string.IsNullOrEmpty(orderBy)) {
                result += " order by " + orderBy;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public static string getTableCacheKey<T>(CoreController core) {
            return CacheController.getCacheKey_forDbTable(derivedTableName(typeof(T)));
        }
    }
}
