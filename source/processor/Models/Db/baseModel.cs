
using System;
using System.Reflection;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using static Contensive.Processor.constants;
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
        public abstract class fieldCdnFile {
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
        public class fieldTypeTextFile : fieldCdnFile {
        }
        public class fieldTypeJavascriptFile : fieldCdnFile {
        }
        public class fieldTypeCSSFile : fieldCdnFile {
        }
        public class fieldTypeHTMLFile : fieldCdnFile {
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
        private static string derivedContentTableName(Type derivedType) {
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
        private static string contentDataSource(Type derivedType) {
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
        protected static T add<T>(CoreController core) where T : BaseModel {
            var tempVar = new List<string>();
            return add<T>(core, ref tempVar);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        /// include callersCacheNameList to get a list of cacheNames used to assemble this response
        /// </summary>
        /// <param name="core"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        protected static T add<T>(CoreController core, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    Type instanceType = typeof(T);
                    string contentName = derivedContentName(instanceType);
                    result = create<T>(core, core.db.insertContentRecordGetID(contentName, core.session.user.id), ref callersCacheNameList);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
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
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (recordId > 0) {
                        Type instanceType = typeof(T);
                        string contentName = derivedContentName(instanceType);
                        result = readModelCache<T>(core, "id", recordId.ToString());
                        if (result == null) {
                            using (csController cs = new csController(core)) {
                                if (cs.open(contentName, "(id=" + recordId.ToString() + ")")) {
                                    result = loadRecord<T>(core, cs, ref callersCacheNameList);
                                }
                            }
                        }
                        //
                        // -- store core in all extended fields that need it (file fields so content read can happen on demand instead of at load)
                        if (result != null) {
                            foreach (PropertyInfo instanceProperty in result.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                                switch (instanceProperty.PropertyType.Name) {
                                    case "fieldTypeTextFile":
                                    case "fieldTypeJavascriptFile":
                                    case "fieldTypeCSSFile":
                                    case "fieldTypeHTMLFile":
                                        switch (instanceProperty.PropertyType.Name) {
                                            case "fieldTypeJavascriptFile": {
                                                    fieldTypeJavascriptFile fileProperty = (fieldTypeJavascriptFile)instanceProperty.GetValue(result);
                                                    fileProperty.internalcore = core;
                                                    break;
                                                }
                                            case "fieldTypeCSSFile": {
                                                    fieldTypeCSSFile fileProperty = (fieldTypeCSSFile)instanceProperty.GetValue(result);
                                                    fileProperty.internalcore = core;
                                                    break;
                                                }
                                            case "fieldTypeHTMLFile": {
                                                    fieldTypeHTMLFile fileProperty = (fieldTypeHTMLFile)instanceProperty.GetValue(result);
                                                    fileProperty.internalcore = core;
                                                    break;
                                                }
                                            default: {
                                                    fieldTypeTextFile fileProperty = (fieldTypeTextFile)instanceProperty.GetValue(result);
                                                    fileProperty.internalcore = core;
                                                    break;
                                                }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(recordGuid)) {
                        Type instanceType = typeof(T);
                        string contentName = derivedContentName(instanceType);
                        result = readModelCache<T>(core, "ccguid", recordGuid);
                        if (result == null) {
                            using (csController cs = new csController(core)) {
                                if (cs.open(contentName, "(ccGuid=" + core.db.encodeSQLText(recordGuid) + ")")) {
                                    result = loadRecord<T>(core, cs, ref callersCacheNameList);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        protected static T createByName<T>(CoreController core, string recordName) where T : BaseModel {
            var cacheNameList = new List<string>();
            return createByName<T>(core, recordName, ref cacheNameList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordName"></param>
        protected static T createByName<T>(CoreController core, string recordName, ref List<string> callersCacheNameList) where T : BaseModel {
            T result = default(T);
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(recordName)) {
                        Type instanceType = typeof(T);
                        string contentName = derivedContentName(instanceType);
                        result = readModelCache<T>(core, "name", recordName);
                        if (result == null) {
                            using (csController cs = new csController(core)) {
                                if (cs.open(contentName, "(name=" + core.db.encodeSQLText(recordName) + ")", "id")) {
                                    result = loadRecord<T>(core, cs, ref callersCacheNameList);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        private static T loadRecord<T>(CoreController core, csController cs, ref List<string> callersCacheNameList) where T : BaseModel {
            T modelInstance = default(T);
            try {
                if (cs.ok()) {
                    Type instanceType = typeof(T);
                    string contentName = derivedContentName(instanceType);
                    string tableName = derivedContentTableName(instanceType);
                    int recordId = cs.getInteger("id");
                    modelInstance = (T)Activator.CreateInstance(instanceType);
                    foreach (PropertyInfo modelProperty in modelInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        switch (modelProperty.Name.ToLower()) {
                            case "specialcasefield":
                                break;
                            default:
                                switch (modelProperty.PropertyType.Name) {
                                    case "Int32": {
                                            modelProperty.SetValue(modelInstance, cs.getInteger(modelProperty.Name), null);
                                            break;
                                        }
                                    case "Boolean": {
                                            modelProperty.SetValue(modelInstance, cs.getBoolean(modelProperty.Name), null);
                                            break;
                                        }
                                    case "DateTime": {
                                            modelProperty.SetValue(modelInstance, cs.getDate(modelProperty.Name), null);
                                            break;
                                        }
                                    case "Double": {
                                            modelProperty.SetValue(modelInstance, cs.getNumber(modelProperty.Name), null);
                                            break;
                                        }
                                    case "String": {
                                            modelProperty.SetValue(modelInstance, cs.getText(modelProperty.Name), null);
                                            break;
                                        }
                                    case "fieldTypeTextFile": {
                                            //
                                            // -- cdn files
                                            fieldTypeTextFile instanceFileType = new fieldTypeTextFile();
                                            instanceFileType.filename = cs.getValue(modelProperty.Name);
                                            //If (Not String.IsNullOrEmpty(instanceFileType.filename)) Then
                                            //    instanceFileType.content = core.cdnFiles.readFile(instanceFileType.filename)
                                            //End If
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "fieldTypeJavascriptFile": {
                                            //
                                            // -- cdn files
                                            fieldTypeJavascriptFile instanceFileType = new fieldTypeJavascriptFile();
                                            instanceFileType.filename = cs.getValue(modelProperty.Name);
                                            //If (Not String.IsNullOrEmpty(instanceFileType.filename)) Then
                                            //    instanceFileType.content = core.cdnFiles.readFile(instanceFileType.filename)
                                            //End If
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "fieldTypeCSSFile": {
                                            //
                                            // -- cdn files
                                            fieldTypeCSSFile instanceFileType = new fieldTypeCSSFile();
                                            instanceFileType.filename = cs.getValue(modelProperty.Name);
                                            //If (Not String.IsNullOrEmpty(instanceFileType.filename)) Then
                                            //    instanceFileType.content = core.cdnFiles.readFile(instanceFileType.filename)
                                            //End If
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    case "fieldTypeHTMLFile": {
                                            //
                                            // -- private files
                                            fieldTypeHTMLFile instanceFileType = new fieldTypeHTMLFile();
                                            instanceFileType.filename = cs.getValue(modelProperty.Name);
                                            //If (Not String.IsNullOrEmpty(instanceFileType.filename)) Then
                                            //    instanceFileType.content = core.cdnFiles.readFile(instanceFileType.filename)
                                            //End If
                                            modelProperty.SetValue(modelInstance, instanceFileType);
                                            break;
                                        }
                                    default: {
                                            modelProperty.SetValue(modelInstance, cs.getText(modelProperty.Name), null);
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
                        BaseModel baseInstance = modelInstance as BaseModel;
                        if (baseInstance != null) {
                            string cacheName0 = Controllers.CacheController.getCacheKey_Entity(tableName, "id", baseInstance.id.ToString());
                            callersCacheNameList.Add(cacheName0);
                            core.cache.setObject(cacheName0, modelInstance);
                            //
                            string cacheName1 = Controllers.CacheController.getCacheKey_Entity(tableName, "ccguid", baseInstance.ccguid);
                            callersCacheNameList.Add(cacheName1);
                            core.cache.setAlias(cacheName1, cacheName0);
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        protected int save(CoreController core) {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    csController cs = new csController(core);
                    Type instanceType = this.GetType();
                    string contentName = derivedContentName(instanceType);
                    string tableName = derivedContentTableName(instanceType);
                    if (id > 0) {
                        if (!cs.open(contentName, "id=" + id)) {
                            string message = "Unable to open record in content [" + contentName + "], with id [" + id + "]";
                            cs.close();
                            id = 0;
                            throw new ApplicationException(message);
                        }
                    } else {
                        if (!cs.insert(contentName)) {
                            cs.close();
                            id = 0;
                            throw new ApplicationException("Unable to insert record in content [" + contentName + "]");
                        }
                    }
                    int recordId = cs.getInteger("id");
                    foreach (PropertyInfo instanceProperty in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                        switch (instanceProperty.Name.ToLower()) {
                            case "id":
                                id = cs.getInteger("id");
                                break;
                            case "ccguid":
                                if (string.IsNullOrEmpty(ccguid)) {
                                    ccguid = Controllers.genericController.getGUID();
                                }
                                //string value = instanceProperty.GetValue(this, null).ToString();
                                cs.setField(instanceProperty.Name, instanceProperty.GetValue(this, null).ToString());
                                break;
                            default:
                                switch (instanceProperty.PropertyType.Name) {
                                    case "Int32":
                                        Int32 valueInt32;
                                        int.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueInt32);
                                        cs.setField(instanceProperty.Name, valueInt32);
                                        break;
                                    case "Boolean":
                                        bool valueBool;
                                        bool.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueBool);
                                        cs.setField(instanceProperty.Name, valueBool);
                                        break;
                                    case "DateTime":
                                        DateTime valueDate;
                                        DateTime.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueDate);
                                        cs.setField(instanceProperty.Name, valueDate);
                                        break;
                                    case "Double":
                                        double valueDbl;
                                        double.TryParse(instanceProperty.GetValue(this, null).ToString(), out valueDbl);
                                        cs.setField(instanceProperty.Name, valueDbl);
                                        break;
                                    case "fieldTypeTextFile":
                                    case "fieldTypeJavascriptFile":
                                    case "fieldTypeCSSFile":
                                    case "fieldTypeHTMLFile":
                                        int fieldTypeId = 0;
                                        PropertyInfo contentProperty = null;
                                        PropertyInfo contentUpdatedProperty = null;
                                        bool contentUpdated = false;
                                        string content = "";
                                        switch (instanceProperty.PropertyType.Name) {
                                            case "fieldTypeJavascriptFile": {
                                                    fieldTypeId = FieldTypeIdFileJavascript;
                                                    fieldTypeJavascriptFile fileProperty = (fieldTypeJavascriptFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            case "fieldTypeCSSFile": {
                                                    fieldTypeId = FieldTypeIdFileCSS;
                                                    fieldTypeCSSFile fileProperty = (fieldTypeCSSFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            case "fieldTypeHTMLFile": {
                                                    fieldTypeId = FieldTypeIdFileHTML;
                                                    fieldTypeHTMLFile fileProperty = (fieldTypeHTMLFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                            default: {
                                                    fieldTypeId = FieldTypeIdFileText;
                                                    fieldTypeTextFile fileProperty = (fieldTypeTextFile)instanceProperty.GetValue(this);
                                                    fileProperty.internalcore = core;
                                                    contentProperty = instanceProperty.PropertyType.GetProperty("content");
                                                    contentUpdatedProperty = instanceProperty.PropertyType.GetProperty("contentUpdated");
                                                    contentUpdated = (bool)contentUpdatedProperty.GetValue(fileProperty);
                                                    content = (string)contentProperty.GetValue(fileProperty);
                                                    break;
                                                }
                                        }
                                        if (contentUpdated) {
                                            string filename = cs.getValue(instanceProperty.Name);
                                            if (string.IsNullOrEmpty(content)) {
                                                //
                                                // -- empty content
                                                if (!string.IsNullOrEmpty(filename)) {
                                                    cs.setField(instanceProperty.Name, "");
                                                    core.cdnFiles.deleteFile(filename);
                                                }
                                            } else {
                                                //
                                                // -- save content
                                                if (string.IsNullOrEmpty(filename)) {
                                                    filename = FileController.getVirtualRecordUnixPathFilename(tableName, instanceProperty.Name.ToLower(), recordId, fieldTypeId);
                                                }
                                                core.cdnFiles.saveFile(filename, content);
                                                cs.setFieldFilename(instanceProperty.Name, filename);
                                            }
                                        }
                                        //Case "fieldTypeJavascriptFile"
                                        //    Dim textFileProperty As fieldTypeJavascriptFile = DirectCast(instanceProperty.GetValue(Me), fieldTypeJavascriptFile)
                                        //    textFileProperty.internalcore = core
                                        //    Dim copyProperty As PropertyInfo = instanceProperty.PropertyType.GetProperty("content")
                                        //    Dim copy As String = DirectCast(copyProperty.GetValue(textFileProperty), String)
                                        //    If (String.IsNullOrEmpty(copy)) Then
                                        //        '
                                        //        ' -- empty content
                                        //        Dim filename As String = cs.getValue(instanceProperty.Name) ' = DirectCast(filenameProperty.GetValue(propertyInstance), String)
                                        //        If (Not String.IsNullOrEmpty(filename)) Then
                                        //            cs.setField(instanceProperty.Name, "")
                                        //            core.cdnFiles.deleteFile(filename)
                                        //        End If
                                        //    Else
                                        //        '
                                        //        ' -- save content
                                        //        cs.setFile(instanceProperty.Name, copy, contentName)
                                        //    End If
                                        //Case "fieldTypeCSSFile"
                                        //    Dim textFileProperty As fieldTypeCSSFile = DirectCast(instanceProperty.GetValue(Me), fieldTypeCSSFile)
                                        //    textFileProperty.internalcore = core
                                        //    Dim copyProperty As PropertyInfo = instanceProperty.PropertyType.GetProperty("content")
                                        //    Dim copy As String = DirectCast(copyProperty.GetValue(textFileProperty), String)
                                        //    If (String.IsNullOrEmpty(copy)) Then
                                        //        '
                                        //        ' -- empty content
                                        //        Dim filename As String = cs.getValue(instanceProperty.Name) ' = DirectCast(filenameProperty.GetValue(propertyInstance), String)
                                        //        If (Not String.IsNullOrEmpty(filename)) Then
                                        //            cs.setField(instanceProperty.Name, "")
                                        //            core.cdnFiles.deleteFile(filename)
                                        //        End If
                                        //    Else
                                        //        '
                                        //        ' -- save content
                                        //        cs.setFile(instanceProperty.Name, copy, contentName)
                                        //    End If
                                        //Case "fieldTypeHTMLFile"
                                        //    Dim textFileProperty As fieldTypeHTMLFile = DirectCast(instanceProperty.GetValue(Me), fieldTypeHTMLFile)
                                        //    textFileProperty.internalcore = core
                                        //    Dim copyProperty As PropertyInfo = instanceProperty.PropertyType.GetProperty("content")
                                        //    Dim copy As String = DirectCast(copyProperty.GetValue(textFileProperty), String)
                                        //    If (String.IsNullOrEmpty(copy)) Then
                                        //        '
                                        //        ' -- empty content
                                        //        Dim filename As String = cs.getValue(instanceProperty.Name) ' = DirectCast(filenameProperty.GetValue(propertyInstance), String)
                                        //        If (Not String.IsNullOrEmpty(filename)) Then
                                        //            cs.setField(instanceProperty.Name, "")
                                        //            core.cdnFiles.deleteFile(filename)
                                        //        End If
                                        //    Else
                                        //        '
                                        //        ' -- save content
                                        //        cs.setFile(instanceProperty.Name, copy, contentName)
                                        //    End If
                                        break;
                                    default:
                                        //string value = instanceProperty.GetValue(this, null).ToString();
                                        cs.setField(instanceProperty.Name, instanceProperty.GetValue(this, null).ToString());
                                        break;
                                }
                                break;
                        }
                    }
                    cs.close();
                    //
                    // -- invalidate objects
                    // -- no, the primary is invalidated by the cs.save()
                    //core.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
                    // -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
                    //core.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
                    //
                    // -- object is here, but the cache was invalidated, setting
                    core.cache.setObject(Controllers.CacheController.getCacheKey_Entity(tableName, "id", this.id.ToString()), this);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (recordId > 0) {
                        Type instanceType = typeof(T);
                        string contentName = derivedContentName(instanceType);
                        string tableName = derivedContentTableName(instanceType);
                        core.db.deleteContentRecords(contentName, "id=" + recordId.ToString());
                        core.cache.invalidate(Controllers.CacheController.getCacheKey_Entity(tableName, recordId));
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(ccguid)) {
                        Type instanceType = typeof(T);
                        string contentName = derivedContentName(instanceType);
                        BaseModel instance = create<BaseModel>(core, ccguid);
                        if (instance != null) {
                            invalidateCache<T>(core, instance.id);
                            core.db.deleteContentRecords(contentName, "(ccguid=" + core.db.encodeSQLText(ccguid) + ")");
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    csController cs = new csController(core);
                    string sql = getSelectSql<T>(null, sqlCriteria, sqlOrderBy);
                    //Dim ignoreCacheNames As New List(Of String)
                    //Dim instanceType As Type = GetType(T)
                    //Dim contentName As String = derivedContentName(instanceType)
                    if (cs.openSQL(sql)) {
                        //End If
                        //If (cs.open(contentName, sqlCriteria, sqlOrderBy)) Then
                        T instance = default(T);
                        do {
                            instance = loadRecord<T>(core, cs, ref callersCacheNameList);
                            if (instance != null) {
                                result.Add(instance);
                            }
                            cs.goNext();
                        } while (cs.ok());
                    }
                    cs.close();
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
        protected static void invalidateCache<T>(CoreController core, int recordId) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    object instanceType = typeof(T);
                    string tableName = derivedContentTableName((Type)instanceType);
                    core.cache.invalidate(Controllers.CacheController.getCacheKey_Entity(tableName, recordId));
                    //
                    // -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
                    core.cache.invalidate(Controllers.CacheController.getCacheKey_Entity(tableName, "id", "0"));
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (recordId > 0) {
                        Type instanceType = typeof(T);
                        string tableName = derivedContentTableName(instanceType);
                        using (csController cs = new csController(core)) {
                            if (cs.openSQL("select name from " + tableName + " where id=" + recordId.ToString())) {
                                return cs.getText("name");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ccGuid"></param>record
        /// <returns></returns>
        protected static string getRecordName<T>(CoreController core, string ccGuid) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(ccGuid)) {
                        Type instanceType = typeof(T);
                        string tableName = derivedContentTableName(instanceType);
                        using (csController cs = new csController(core)) {
                            if (cs.openSQL("select name from " + tableName + " where ccguid=" + core.db.encodeSQLText(ccGuid))) {
                                return cs.getText("name");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return "";
        }
        //
        //====================================================================================================
        /// <summary>
        /// get the id of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="ccGuid"></param>record
        /// <returns></returns>
        protected static int getRecordId<T>(CoreController core, string ccGuid) where T : BaseModel {
            try {
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    if (!string.IsNullOrEmpty(ccGuid)) {
                        Type instanceType = typeof(T);
                        string tableName = derivedContentTableName(instanceType);
                        using (csController cs = new csController(core)) {
                            if (cs.openSQL("select id from " + tableName + " where ccguid=" + core.db.encodeSQLText(ccGuid))) {
                                return cs.getInteger("id");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return 0;
        }
        //
        //====================================================================================================
        //
        protected static T createDefault<T>(CoreController core) where T : BaseModel {
            T instance = default(T);
            try {
                Type instanceType = typeof(T);
                instance = (T)Activator.CreateInstance(instanceType);
                if (core.serverConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid server configuration."));
                } else if (core.appConfig == null) {
                    //
                    // -- cannot use models without an application
                    logController.handleError( core,new ApplicationException("Cannot use data models without a valid application configuration."));
                } else {
                    string contentName = derivedContentName(instanceType);
                    Models.Domain.CDefModel CDef = Models.Domain.CDefModel.getCdef(core, contentName);
                    if (CDef == null) {
                        throw new ApplicationException("content [" + contentName + "] could Not be found.");
                    } else if (CDef.id <= 0) {
                        throw new ApplicationException("content [" + contentName + "] could Not be found.");
                    } else {
                        foreach (PropertyInfo resultProperty in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)) {
                            switch (resultProperty.Name.ToLower()) {
                                case "id":
                                    resultProperty.SetValue(instance, 0);
                                    break;
                                default:
                                    switch (resultProperty.PropertyType.Name) {
                                        case "Int32":
                                            resultProperty.SetValue(instance, genericController.encodeInteger(CDef.fields[resultProperty.Name].defaultValue), null);
                                            break;
                                        case "Boolean":
                                            resultProperty.SetValue(instance, genericController.encodeBoolean(CDef.fields[resultProperty.Name].defaultValue), null);
                                            break;
                                        case "DateTime":
                                            resultProperty.SetValue(instance, genericController.encodeDate(CDef.fields[resultProperty.Name].defaultValue), null);
                                            break;
                                        case "Double":
                                            resultProperty.SetValue(instance, genericController.encodeNumber(CDef.fields[resultProperty.Name].defaultValue), null);
                                            break;
                                        default:
                                            resultProperty.SetValue(instance, CDef.fields[resultProperty.Name].defaultValue, null);
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
            return instance;
        }
        //
        //====================================================================================================
        //
        private static T readModelCache<T>(CoreController core, string fieldName, string fieldValue) where T : BaseModel {
            Type instanceType = typeof(T);
            string tableName = derivedContentTableName(instanceType);
            string cacheName = Controllers.CacheController.getCacheKey_Entity(tableName, fieldName, fieldValue);
            return core.cache.getObject<T>(cacheName);
        }
        //
        //====================================================================================================
        //
        private static string getSelectSql<T>(List<string> fieldList = null, string criteria = "", string orderBy = "") where T : BaseModel {
            string result = "";
            Type instanceType = typeof(T);
            string tableName = derivedContentTableName(instanceType);
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
    }
}
