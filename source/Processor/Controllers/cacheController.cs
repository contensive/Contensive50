
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Runtime.Caching;
using Contensive.Processor.Exceptions;
using System.Reflection;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Interface to cache systems. Cache objects are saved to dotnet cache, remotecache, filecache. 
    /// 
    /// 3 types of cache methods
    /// -- 1) primary key -- cache key holding the content.
    /// -- 2) dependent key -- a dependent key holds content that the primary cache depends on. If the dependent cache is invalid, the primary cache is invalid
    /// -------- if an org cache includes a primary content person, then the org (org/id/10) is saved with a dependent key (ccmembers/id/99). The org content id valid only if both the primary and dependent keys are valid
    /// -- 3) pointer key -- a cache entry that holds a primary key (or another pointer key). When read, the cache returns the primary value. (primary="ccmembers/id/10", pointer="ccmembers/ccguid/{1234}"
    /// ------- pointer keys are read-only, as the content is saved in the primary key
    /// 
    /// 2  types of cache data:
    ///  -- table record cache -- cache of an entity model (one record)
    ///     .. saveCache, reachCache in the entity model
    ///     .. invalidateCache in the entity model, includes invalication for complex objects that include the entity
    ///      
    ///  -- object cache -- cache of an object with mixed data based on the entity data model (an org object that contains a person object)
    ///    .. has an id for the topmost object
    ///    .. these objects are in .. ? (another model folder?)
    ///    
    /// When a record is saved in admin, an invalidation call is made for tableName/id/#
    /// 
    /// If code edits a db record, you should call invalidate for tablename/id/#
    /// 
    /// one-to-many lists: if the list is cached, it has to include dependent keys for all its included members
    /// 
    /// many-to-many lists: should have a model for the rule table, and lists should come from methods there. the delete method must invalidate the rule model cache
    /// 
    /// A cacheobject has:
    ///   key - if the object is a model of a database record, and there is only one model for that db record, use tableName+id as the cacheName. If it contains other data,
    ///     use the modelName+id as the cacheName, and include all records in the dependentObjectList.
    ///   primaryObjectKey -- a secondary cacheObject does not hold data, just a pointer to a primary object. For example, if you use both id and guid to reference objects,
    ///     save the data in the primary cacheObject (tableName+id) and save a secondary cacheObject (tablename+guid). Sve both when you update the cacheObject. requesting either
    ///     or invalidating either will effect the primary cache
    ///   dependentObjectList -- if a cacheobject contains data from multiple sources, each source should be included in the dependencyList. This includes both cached models and Db records.
    ///   
    /// Special cache entries
    ///     dbTablename - cachename = the name of the table
    ///         - when any record is saved to the table, this dbTablename cache is updated
    ///         - objects like addonList depend on it, and are flushed if ANY record in that table is updated
    ///         
    /// </summary>
    public class CacheController : IDisposable {
        //
        // ====================================================================================================
        // ----- constants
        /// <summary>
        /// default number of days that a cache is invalidated
        /// </summary>
        private const double invalidationDaysDefault = 365;
        //
        // ====================================================================================================
        // ----- objects passed in constructor, do not dispose
        //
        private CoreController core;
        //
        // ====================================================================================================
        // ----- objects constructed that must be disposed
        //
        private Enyim.Caching.MemcachedClient cacheClient;
        //
        // ====================================================================================================
        // ----- private instance storage
        //
        private bool remoteCacheInitialized;
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, null object is returned
        /// </summary>
        /// <typeparam name="objectClass"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public objectClass getObject<objectClass>(string key) {
            objectClass result = default(objectClass);
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                if (!(string.IsNullOrEmpty(key))) {
                    //
                    // -- read cacheWrapper
                    CacheWrapperClass wrappedContent = getWrappedContent(key);
                    if (wrappedContent != null) {
                        //
                        // -- test for global invalidation
                        int dateCompare = globalInvalidationDate.CompareTo(wrappedContent.saveDate);
                        if (dateCompare >= 0) {
                            //
                            // -- global invalidation
                            LogController.logTrace( core,"GetObject(" + key + "), invalidated because the cacheObject's saveDate [" + wrappedContent.saveDate.ToString() + "] is after the globalInvalidationDate [" + globalInvalidationDate + "]");
                        } else {
                            //
                            // -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
                            bool cacheMiss = false;
                            foreach (string dependentKey in wrappedContent.dependentKeyList) {
                                CacheWrapperClass dependantObject = getWrappedContent(dependentKey);
                                if (dependantObject == null) {
                                    // create dummy cache to validate future cache requests, fake saveDate as last globalinvalidationdate
                                    storeWrappedObject(dependentKey, new CacheWrapperClass() {
                                        keyPtr = null,
                                        content = "",
                                        saveDate = globalInvalidationDate
                                    });
                                } else {
                                    dateCompare = dependantObject.saveDate.CompareTo(wrappedContent.saveDate);
                                    if (dateCompare >= 0) {
                                        cacheMiss = true;
                                        LogController.logTrace(core, "GetObject(" + key + "), invalidated because the dependantobject [" + dependentKey + "] has a saveDate [" + dependantObject.saveDate.ToString() + "] after the cacheObject's saveDate [" + wrappedContent.saveDate.ToString() + "]");
                                        break;
                                    }
                                }
                            }
                            if (!cacheMiss) {
                                if (!string.IsNullOrEmpty(wrappedContent.keyPtr)) {
                                    //
                                    // -- this is a pointer key, load the primary
                                    result = getObject<objectClass>(wrappedContent.keyPtr);
                                } else if (wrappedContent.content is Newtonsoft.Json.Linq.JObject) {
                                    //
                                    // -- newtonsoft types
                                    Newtonsoft.Json.Linq.JObject data = (Newtonsoft.Json.Linq.JObject)wrappedContent.content;
                                    result = data.ToObject<objectClass>();
                                } else if (wrappedContent.content is Newtonsoft.Json.Linq.JArray) {
                                    //
                                    // -- newtonsoft types
                                    Newtonsoft.Json.Linq.JArray data = (Newtonsoft.Json.Linq.JArray)wrappedContent.content;
                                    result = data.ToObject<objectClass>();
                                } else if (wrappedContent.content==null) {
                                    //} else if ((wrappedContent.content is string) && (!(typeof(objectClass) is string))) {
                                    //
                                    // -- if cache data was left as a string (might be empty), and return object is not string, there was an error
                                    result = default(objectClass);
                                } else {
                                    //
                                    // -- all worked
                                    result = (objectClass)wrappedContent.content;
                                }
                            }
                        }
                    }
                    //logController.appendCacheLog(core,"getObject(" + key + "), exit(" + sw.ElapsedMilliseconds + "ms)");
                    //sw.Stop();
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                // 20171222 
                // -- cache errors should be warnings, not critical errors. Dont take down the application over a cache issue.
                // -- cache errors likely did not original above the cache api, so failing the caller would not be productive
                //throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a cache object from the cache. returns the cacheObject that wraps the object
        /// </summary>
        /// <typeparam name="returnType"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        private CacheWrapperClass getWrappedContent(string key) {
            CacheWrapperClass result = null;
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("key cannot be blank");
                } else {
                    string serverKey = createServerKey(key);
                    if (remoteCacheInitialized) {
                        //
                        // -- use remote cache
                        try {
                            result = cacheClient.Get<CacheWrapperClass>(serverKey);
                        } catch (Exception ex) {
                            //
                            // --client does not throw its own errors, so try to differentiate by message
                            if (ex.Message.ToLowerInvariant().IndexOf("unable to load type") >= 0) {
                                //
                                // -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                cacheClient.Remove(serverKey);
                                result = null;
                            } else {
                                //
                                // -- some other error
                                LogController.handleError( core,ex);
                                throw;
                            }
                        }
                        if (result != null) {
                            LogController.logTrace(core, "getWrappedContent(" + key + "), remoteCache hit");
                        } else {
                            LogController.logTrace(core,"getWrappedContent(" + key + "), remoteCache miss");
                        }
                    }
                    if ((result == null) && core.serverConfig.enableLocalMemoryCache) {
                        //
                        // -- local memory cache
                        //Dim cache As ObjectCache = MemoryCache.Default
                        result = (CacheWrapperClass)MemoryCache.Default[serverKey];
                        if (result != null) {
                            LogController.logTrace(core, "getWrappedContent(" + key + "), memoryCache hit");
                        } else {
                            LogController.logTrace(core,"getWrappedContent(" + key + "), memoryCache miss");
                        }
                    }
                    if ((result == null) && core.serverConfig.enableLocalFileCache) {
                        //
                        // -- local file cache
                        string serializedDataObject = null;
                        using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, serverKey)) {
                            mutex.WaitOne();
                            serializedDataObject = core.privateFiles.readFileText("appCache\\" + FileController.encodeDosFilename(serverKey + ".txt"));
                            mutex.ReleaseMutex();
                        }
                        if (string.IsNullOrEmpty(serializedDataObject)) {
                            LogController.logTrace(core,"getWrappedContent(" + key + "), file miss");
                        } else {
                            //logController.appendCacheLog(core,"getWrappedContent(" + key + "), file hit, write to memory cache");
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject<CacheWrapperClass>(serializedDataObject);
                            storeWrappedObject_MemoryCache(serverKey, result);
                        }
                        if (result != null) {
                            //logController.appendCacheLog(core,"getWrappedContent(" + key + "), fileCache hit");
                        } else {
                            LogController.logTrace(core,"getWrappedContent(" + key + "), fileCache miss");
                        }
                    }
                    if (result != null) {
                        //
                        // -- empty objects return nothing, empty lists return count=0
                        if (result.dependentKeyList == null) {
                            result.dependentKeyList = new List<string>();
                        }
                    }
                }
                //logController.appendCacheLog(core,"getWrappedContent(" + key + "), exit ");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// save an object to cache, with invalidation date and dependentKeyList
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="invalidationDate"></param>
        /// <param name="dependentKeyList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        /// <remarks></remarks>
        public void storeObject(string key, object content, DateTime invalidationDate, List<string> dependentKeyList) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                CacheWrapperClass wrappedContent = new CacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = invalidationDate,
                    dependentKeyList = dependentKeyList
                };
                storeWrappedObject(key, wrappedContent);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        ////
        ////====================================================================================================
        ///// <summary>
        ///// save an object to cache, with invalidation date and dependentKey
        ///// 
        ///// </summary>
        ///// <param name="key"></param>
        ///// <param name="content"></param>
        ///// <param name="invalidationDate"></param>
        ///// <param name="dependantKey">The cache name of an object that .</param>
        ///// <remarks></remarks>
        //public void setObject(string key, object content, DateTime invalidationDate, string dependantKey) {
        //    setObject(key, content, invalidationDate, new List<string> { dependantKey });
        //}
        //
        //====================================================================================================
        /// <summary>
        /// save a cache value, compatible with legacy method signature.
        /// </summary>
        /// <param name="CP"></param>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="dependentKeyList">List of dependent keys.</param>
        /// <remarks>If a dependent key is invalidated, it's parent key is also invalid. 
        /// ex - org/id/10 has primary contact person/id/99. if org/id/10 object includes person/id/99 object, then org/id/10 depends on person/id/99,
        /// and "person/id/99" is a dependent key for "org/id/10". When "org/id/10" is read, it checks all its dependent keys (person/id/99) and
        /// invalidates if any dependent key is invalid.</remarks>
        public void storeObject(string key, object content, List<string> dependentKeyList) {
            storeObject(key, content, DateTime.Now.AddDays(invalidationDaysDefault), dependentKeyList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// save a cache value, compatible with legacy method signature.
        /// </summary>
        /// <param name="CP"></param>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="dependantKey"></param>
        /// <remarks></remarks>
        public void storeObject(string key, object content, string dependantKey) {
            storeObject(key, content, DateTime.Now.AddDays(invalidationDaysDefault), new List<string> { dependantKey });
        }
        //
        //====================================================================================================
        /// <summary>
        /// save an object to cache, with invalidation date
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="invalidationDate"></param>
        /// <remarks></remarks>
        public void storeObject(string key, object content, DateTime invalidationDate) {
            storeObject(key, content, invalidationDate, new List<string> { });
        }
        //
        //====================================================================================================
        /// <summary>
        /// save an object to cache, for compatibility with existing site. Always use a key generated from createKey methods
        /// </summary>
        /// <param name="key">key generated from createKey methods</param>
        /// <param name="content"></param>
        public void storeObject(string key, object content) {
            storeObject(key, content, DateTime.Now.AddDays(invalidationDaysDefault), new List<string> { });
        }
        //
        //====================================================================================================
        /// <summary>
        /// save a Db Model cache.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="datasourceName"></param>
        /// <param name="modelContent"></param>
        public void StoreDbModel(string guid, int recordId, string tableName, string datasourceName, object modelContent) {
            string key = createCacheKey_forDbRecord(recordId, tableName, datasourceName);
            storeObject(key, modelContent);
            string keyPtr = createCachePtr_forDbRecord_guid(guid, tableName, datasourceName);
            storePtr(keyPtr, key);
        }
        //
        //====================================================================================================
        /// <summary>
        /// future method. To support a cpbase implementation, but wait until Models.Db.BaseModel is exposed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guid"></param>
        /// <param name="recordId"></param>
        /// <param name="content"></param>
        public void StoreDbModel<T>(string guid, int recordId, object content) where T : BaseModel {
            Type derivedType = this.GetType();
            FieldInfo fieldInfoTable = derivedType.GetField("contentTableName");
            if (fieldInfoTable == null) {
                throw new GenericException("Class [" + derivedType.Name + "] must declare constant [contentTableName].");
            } else {
                string tableName = fieldInfoTable.GetRawConstantValue().ToString();
                FieldInfo fieldInfoDatasource = derivedType.GetField("contentDataSource");
                if (fieldInfoDatasource == null) {
                    throw new GenericException("Class [" + derivedType.Name + "] must declare public constant [contentDataSource].");
                } else {
                    string datasourceName = fieldInfoDatasource.GetRawConstantValue().ToString();
                    string key = createCacheKey_forDbRecord(recordId, tableName, datasourceName);
                    storeObject(key, content);
                    string keyPtr = createCachePtr_forDbRecord_guid(guid, tableName, datasourceName);
                    storePtr(keyPtr, key);
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a key ptr. A ptr points to a normal key, creating an altername way to get/invalidate a cache.
        /// ex - image with id=10, guid={999}. The normal key="image/id/10", the alias Key="image/ccguid/{9999}"
        /// </summary>
        /// <param name="CP"></param>
        /// <param name="keyPtr"></param>
        /// <param name="data"></param>
        /// <remarks></remarks>
        public void storePtr(string keyPtr, string key) {
            try {
                keyPtr = Regex.Replace(keyPtr, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                CacheWrapperClass cacheWrapper = new CacheWrapperClass {
                    saveDate = DateTime.Now,
                    invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
                    keyPtr = key
                };
                storeWrappedObject(keyPtr, cacheWrapper);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidates the entire cache (except those entires written with saveRaw)
        /// </summary>
        /// <remarks></remarks>
        public void invalidateAll() {
            try {
                string key = Regex.Replace(cacheNameGlobalInvalidationDate, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                storeWrappedObject(key, new CacheWrapperClass { saveDate = DateTime.Now });
                _globalInvalidationDate = null;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        // <summary>
        // invalidates a tag
        // </summary>
        // <param name="tag"></param>
        // <remarks></remarks>
        public void invalidate(string key, int recursionLimit = 5) {
            try {
                if ((recursionLimit>0) && (!string.IsNullOrWhiteSpace(key.Trim()))) {
                    key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                    // if key is a ptr, we need to invalidate the real key
                    CacheWrapperClass wrapper = getWrappedContent(key);
                    if (wrapper == null) {
                        // no cache for this key, if this is a dependency for another key, save invalidated
                        storeWrappedObject(key, new CacheWrapperClass { saveDate = DateTime.Now });
                    } else { 
                        if (!string.IsNullOrWhiteSpace(wrapper.keyPtr)) {
                            // this key is an alias, invalidate it's parent key
                            invalidate(wrapper.keyPtr, --recursionLimit);
                        } else {
                            // key is a valid cache, invalidate it
                            storeWrappedObject(key, new CacheWrapperClass { saveDate = DateTime.Now });
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// invalidate a cache for a single database record. If you know the table is in a content model, call the model's invalidateRecord. Use this when the content is a variable.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="recordId"></param>
        public void invalidateDbRecord(int recordId, string tableName, string dataSourceName = "default") {
            invalidate(createCacheKey_forDbRecord(recordId, tableName, dataSourceName));
        }
        ////
        ////========================================================================
        ///// <summary>
        ///// update the dbTablename cache entry. Do this anytime any record is updated in the table
        ///// </summary>
        ///// <param name="ContentName"></param>
        //public void Deprecate_useTable_invalidateAllInContent(string ContentName) {
        //    try {
        //        //
        //        logController.logTrace(core,"invalidateAllObjectsInContent(" + ContentName + ")");
        //        //
        //        // -- save the cache key that represents any record in the content, set as a dependent key for saves
        //        invalidate(ContentName);
        //        invalidate(CdefController.getContentTablename(core, ContentName));
        //    } catch (Exception ex) {
        //        logController.handleError( core,ex);
        //        throw;
        //    }
        //}
        //
        //========================================================================
        /// <summary>
        /// invalidate all cache entries to this table. "dbTableName"
        /// when any cache is saved, it should include a dependancy on a cachename=dbtablename
        /// </summary>
        /// <param name="dbTableName"></param>
        public void invalidateAllKeysInTable(string dbTableName) {
            try {
                invalidate(createCacheKey_forDbTable(dbTableName));
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        public void legacyInvalidateAllKeysInTableList(List<string> tableNameList) {
            foreach (var tableName in tableNameList) {
                core.cache.invalidateAllKeysInTable(tableName);
            }
        }
        //
        public void legacyInvalidateAllKeysInContentList(string ContentNameList) {
            if (!string.IsNullOrEmpty(ContentNameList)) {
                List<string> tableNameList = new List<string>();
                foreach (var contentName in new List<string>(ContentNameList.ToLowerInvariant().Split(','))) {
                    string tableName = CdefController.getContentTablename(core, contentName).ToLowerInvariant();
                    if (!tableNameList.Contains(tableName)) {
                        tableNameList.Add(tableName);
                    }
                    legacyInvalidateAllKeysInTableList(tableNameList);
                }
            }

        }

        //
        //====================================================================================================
        /// <summary>
        /// invalidates a list of keys 
        /// </summary>
        /// <param name="keyList"></param>
        /// <remarks></remarks>
        public void invalidate(List<string> keyList) {
            try {
                foreach (var key in keyList) {
                    invalidate(key);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=======================================================================
        /// <summary>
        /// Convert a key to be used in the server cache. Normalizes name, adds app name and code version
        /// </summary>
        /// <param name="key">The cache key to be converted</param>
        /// <returns></returns>
        private string createServerKey(string key) {
            string result = core.appConfig.name + "-" + core.codeVersion() + "-" + key;
            result = Regex.Replace(result, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return result;
        }
        /// <summary>
        /// table keys should only be used to invalidate all records in a table by adding this key as a dependant key to all cache containing records in the table.
        /// Not automatic (yet)
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string createCacheKey_forDbTable( string tableName, string dataSourceName ) {
            string key = (String.IsNullOrWhiteSpace(dataSourceName)) ? "dbtable/default/" + tableName.Trim() + "/" : "dbtable/" + dataSourceName.Trim() + "/" + tableName.Trim() + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        public static string createCacheKey_forDbTable(string tableName) => createCacheKey_forDbTable(tableName, "default");
        /// <summary>
        /// return the standard key for Db records. 
        /// setObject for this key should only be the object model for this id
        /// getObject for this key should return null or an object of the model.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createCacheKey_forDbRecord(int recordId, string tableName, string dataSourceName) {
            string key = (String.IsNullOrWhiteSpace(dataSourceName)) ? "dbtable/default/" : "dbtable/" + dataSourceName.Trim() + "/";
            key += tableName.Trim() + "/id/" + recordId.ToString() + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        public static string createCacheKey_forDbRecord(int recordId, string tableName) => createCacheKey_forDbRecord(recordId, tableName, "default");
        /// <summary>
        /// return the standard key for cache ptr by Guid for Db records.  
        /// Only use this Ptr in setPtr. NEVER setObject for a ptr.
        /// getObject for this key should return null or an object of the model
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createCachePtr_forDbRecord_guid(string guid, string tableName, string dataSourceName) {
            string key = "dbptr/" + dataSourceName + "/" + tableName + "/ccguid/" + guid + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        public static string createCachePtr_forDbRecord_guid(string guid, string tableName) => createCachePtr_forDbRecord_guid(guid, tableName, "default");
        /// <summary>
        /// return the standard key for cache ptr by name for Db records. 
        /// ONLY use this for tables where the name is unique.
        /// Only use this Ptr in setPtr. NEVER setObject for a ptr.
        /// getObject for this key should return a list of models that match the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createCachePtr_forDbRecord_uniqueName(string name, string tableName, string dataSourceName) {
            string key = "dbptr/" + dataSourceName + "/" + tableName + "/name/" + name + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        public static string createCachePtr_forDbRecord_uniqueName(string name, string tableName) => createCachePtr_forDbRecord_uniqueName(name, tableName, "default");
        //
        //====================================================================================================
        /// <summary>
        /// create a cache name for an object composed of data not from a single record
        /// </summary>
        /// <param name="objectName">The key that describes the object. This can be more general like "person" and used with a uniqueIdentities like "5"</param>
        /// <param name="objectUniqueIdentifier"></param>
        /// <returns></returns>
        public static string createCacheKey_forObject(string objectName, string objectUniqueIdentifier = "") {
            string key = "obj/" + objectName + "/" + objectUniqueIdentifier;
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// cache data wrapper to include tags and save datetime
        /// </summary>
        [Serializable]
        public class CacheWrapperClass {
            //
            // if populated, all other properties are ignored and the primary tag b
            public string keyPtr;
            //
            // this object is invalidated if any of these objects are invalidated
            public List<string> dependentKeyList = new List<string>();
            //
            // the date this object was last saved.
            public DateTime saveDate = DateTime.Now;
            //
            // the future date when this object self-invalidates
            public DateTime invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault);
            //
            // the data storage
            public object content;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared. Every cache object saved before this date is considered invalid.
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        private DateTime globalInvalidationDate {
            get {
                bool setDefault = false;
                if (_globalInvalidationDate == null) {
                    CacheWrapperClass dataObject = getWrappedContent(cacheNameGlobalInvalidationDate);
                    if (dataObject != null) {
                        _globalInvalidationDate = dataObject.saveDate;
                    }
                    if (_globalInvalidationDate == null) {
                        setDefault = true;
                    }
                }
                if (!_globalInvalidationDate.HasValue) {
                    setDefault = true;
                } else {
                    if ((encodeDate(_globalInvalidationDate)).CompareTo(new DateTime(1990, 8, 7)) < 0) {
                        setDefault = true;
                    }
                }
                if (setDefault) {
                    _globalInvalidationDate = new DateTime(1990, 8, 7);
                    storeWrappedObject(cacheNameGlobalInvalidationDate, new CacheWrapperClass { saveDate = encodeDate(_globalInvalidationDate) });
                }
                return encodeDate(_globalInvalidationDate);
            }
        }
        private DateTime? _globalInvalidationDate;
        //
        //====================================================================================================
        /// <summary>
        /// Initializes cache client
        /// </summary>
        /// <remarks></remarks>
        public CacheController(CoreController core) {
            try {
                this.core = core;
                //
                _globalInvalidationDate = null;
                remoteCacheInitialized = false;
                if (core.serverConfig.enableRemoteCache) {
                    string cacheEndpoint = core.serverConfig.awsElastiCacheConfigurationEndpoint;
                    if (!string.IsNullOrEmpty(cacheEndpoint)) {
                        string[] cacheEndpointSplit = cacheEndpoint.Split(':');
                        int cacheEndpointPort = 11211;
                        if (cacheEndpointSplit.GetUpperBound(0) > 1) {
                            cacheEndpointPort = GenericController.encodeInteger(cacheEndpointSplit[1]);
                        }
                        Amazon.ElastiCacheCluster.ElastiCacheClusterConfig cacheConfig = new Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(cacheEndpointSplit[0], cacheEndpointPort) {
                            Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary
                        };
                        cacheClient = new Enyim.Caching.MemcachedClient(cacheConfig);
                        if (cacheClient != null) {
                            remoteCacheInitialized = true;
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // -- client does not throw its own errors, so try to differentiate by message
                throw (new GenericException("Exception initializing remote cache, will continue with cache disabled.", ex));
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save object directly to cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wrappedObject">Either a string, a date, or a serializable object</param>
        /// <param name="invalidationDate"></param>
        /// <remarks></remarks>
        private void storeWrappedObject(string key, CacheWrapperClass wrappedObject) {
            try {
                //
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("cache key cannot be blank");
                } else {
                    string serverKey = createServerKey(key);
                    //
                    LogController.logTrace(core, "storeWrappedObject(" + serverKey + "), expires [" + wrappedObject.invalidationDate.ToString() + "], depends on [" + string.Join(",", wrappedObject.dependentKeyList) + "], points to [" + string.Join(",", wrappedObject.keyPtr) + "]");
                    //
                    if (core.serverConfig.enableLocalMemoryCache) {
                        //
                        // -- save local memory cache
                        storeWrappedObject_MemoryCache(serverKey, wrappedObject);
                    }
                    if (core.serverConfig.enableLocalFileCache) {
                        //
                        // -- save local file cache
                        string serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(wrappedObject);
                        using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, serverKey)) {
                            mutex.WaitOne();
                            core.privateFiles.saveFile("appCache\\" + FileController.encodeDosFilename(serverKey + ".txt"), serializedData);
                            mutex.ReleaseMutex();
                        }
                    }
                    if (core.serverConfig.enableRemoteCache) {
                        if (remoteCacheInitialized) {
                            //
                            // -- save remote cache
                            cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, serverKey, wrappedObject, wrappedObject.invalidationDate);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save cacheWrapper to memory cache
        /// </summary>
        /// <param name="serverKey">key converted to serverKey with app name and code version</param>
        /// <param name="wrappedObject"></param>
        public void storeWrappedObject_MemoryCache(string serverKey, CacheWrapperClass wrappedObject) {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy {
                AbsoluteExpiration = wrappedObject.invalidationDate // DateTime.Now.AddMinutes(100);
            };
            cache.Set(serverKey, wrappedObject, policy);
        }
        //
        //====================================================================================================
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CacheController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    if (cacheClient != null) {
                        cacheClient.Dispose();
                    }
                    //
                    // cleanup managed objects
                }
                //
                // cleanup non-managed objects
            }
        }
        #endregion
    }
    //
}