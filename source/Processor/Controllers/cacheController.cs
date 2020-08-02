
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using System.Runtime.Caching;
using Contensive.Processor.Exceptions;
using System.Reflection;
using Enyim.Caching;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Contensive.Processor.Models.Domain;
using static Newtonsoft.Json.JsonConvert;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Interface to cache systems. Cache objects are saved to dotnet cache, remotecache, filecache. 
    /// 
    /// Cache Methods
    /// 
    ///   1) primary key -- cache key holding the object.
    ///         -- A key can be any string - a name for the cache
    ///         -- A key can be created in a standard format to represent a record in a database RecordKey()
    ///         -- A key can be created in a standard format to represent a dependency on Any record in a table - use
    ///         -- use createKey() to create a primary key
    ///         
    ///   2) dependent key -- an optional argument passed to a store() that invalidates the key if it is invalidated
    ///         -- it may hold content that the primary cache depends on. 
    ///                 -- for example a cache of Team may be saved with a dependency key list of people keys. if any people key is invalidated, the team invalidates
    ///         -- TableDependencyKey is invalidated each time a record is saved to the table. primary keys saved with the dependency are then also invalidated
    ///                 -- for example a cache of catalog items set to depend on the tableDependecyKey for items is invalidated if any item is updated.
    ///         -- use createKey() to create a dependency key if the primary key depends on the dependency
    ///         -- use create
    ///                 
    ///    3) pointer key -- a cache entry that holds a primary key (or another pointer key). 
    ///         -- When read, the cache returns the primary value. (primary="ccmembers/id/10", pointer="ccmembers/ccguid/{1234}"
    ///         -- pointer keys are read-only, as the content is saved in the primary key
    ///         -- set a pointer key with storePtr()
    ///         -- use storeRecord() to store the content to the key and all the ptrs
    /// 
    /// Cache Data Types
    /// 
    ///    1) object cache -- cache of any unstructured data
    ///         -- use createKey() to generate the key
    ///         -- use store() to store the object to the key
    ///         -- use get() to retrieve the object from the key
    ///         -- use invalidate() to invalidate the key
    ///         -- is invalidated by invalidateAll()
    ///         
    ///    2) record cache -- cache holding one record in a table
    ///         -- use createRecordKey() to generate the key
    ///         -- use createRecordGuidPtrKey() to generate a Ptr Key for the record that will retrieve the record based on the Guid
    ///         -- use createRecordNamePtrKey() to generate a Ptr Key for the record that will retrieve the record based on the Unique Name
    ///         -- use store() to store the object to the key
    ///         -- use get() to rerieve the object from the key
    ///         -- use invalidate() to invalidate the RecordKey, the RecordGuidPtr, or the RecordNamePtr
    ///         -- use invalidateRecordKey() to invalidate the key from the record/table/datasource names
    ///         
    /// Table Dependency Key
    ///         -- any cache object set to depend on this key will invalidate if the save date-time is newer than the table dependency key
    ///         -- saves the date-time when it was invalidated.
    ///         -- the /admin site invalidates this key on changes
    ///         -- models invalidate this key on change
    ///         -- database updates in the application should invalidate the key when a record is changed in a table
    ///         
    /// </summary>
    public class CacheController : IDisposable {
        //
        // ====================================================================================================
        // ----- objects passed in constructor, do not dispose
        /// <summary>
        /// local storage. Do not dispose
        /// </summary>
        private readonly CoreController core;
        //
        // ====================================================================================================
        // ----- objects constructed that must be disposed
        /// <summary>
        /// AWS client. Dispose on close
        /// </summary>
        private Enyim.Caching.MemcachedClient cacheClient = null;
        //
        // ====================================================================================================
        // ----- private instance storage
        /// <summary>
        /// true if cacheClient initialized correctly
        /// </summary>
        private readonly bool remoteCacheInitialized;
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
#if NETFRAMEWORK
                if (core.serverConfig.enableRemoteCache) {
                    //
                    // -- leave off, it causes a performance hit
                    if (core.serverConfig.enableEnyimNLog) { Enyim.Caching.LogManager.AssignFactory(new NLogFactory()); }
                    //
                    // -- initialize memcached drive (Enyim)
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
#endif
            } catch (Exception ex) {
                //
                // -- client does not throw its own errors, so try to differentiate by message
                throw (new GenericException("Exception initializing remote cache, will continue with cache disabled.", ex));
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get an object of type TData from cache. If the cache misses or is invalidated, null object is returned
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public TData getObject<TData>(string key) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                if (string.IsNullOrEmpty(key)) { return default; }
                //
                // -- read cacheDocument (the object that holds the data object plus control fields)
                CacheDocumentClass cacheDocument = getCacheDocument(key);
                if (cacheDocument == null) { return default; }
                //
                // -- test for global invalidation
                int dateCompare = globalInvalidationDate.CompareTo(cacheDocument.saveDate);
                if (dateCompare >= 0) {
                    //
                    // -- global invalidation
                    LogController.logTrace(core, "key [" + key + "], invalidated because cacheObject saveDate [" + cacheDocument.saveDate + "] is before the globalInvalidationDate [" + globalInvalidationDate + "]");
                    return default;
                }
                //
                // -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
                bool cacheMiss = false;
                foreach (string dependentKey in cacheDocument.dependentKeyList) {
                    CacheDocumentClass dependantCacheDocument = getCacheDocument(dependentKey);
                    if (dependantCacheDocument == null) {
                        // create dummy cache to validate future cache requests, fake saveDate as last globalinvalidationdate
                        storeCacheDocument(dependentKey, new CacheDocumentClass(core.dateTimeNowMockable) {
                            keyPtr = null,
                            content = "",
                            saveDate = globalInvalidationDate
                        });
                    } else {
                        dateCompare = dependantCacheDocument.saveDate.CompareTo(cacheDocument.saveDate);
                        if (dateCompare >= 0) {
                            //
                            // -- invalidate because a dependent document was changed after the cacheDocument was saved
                            cacheMiss = true;
                            LogController.logTrace(core, "[" + key + "], invalidated because the dependantKey [" + dependentKey + "] was modified [" + dependantCacheDocument.saveDate + "] after the cacheDocument's saveDate [" + cacheDocument.saveDate + "]");
                            break;
                        }
                    }
                }
                TData result = default;
                if (!cacheMiss) {
                    if (!string.IsNullOrEmpty(cacheDocument.keyPtr)) {
                        //
                        // -- this is a pointer key, load the primary
                        result = getObject<TData>(cacheDocument.keyPtr);
                    } else if (cacheDocument.content is Newtonsoft.Json.Linq.JObject dataJObject) {
                        //
                        // -- newtonsoft types
                        result = dataJObject.ToObject<TData>();
                    } else if (cacheDocument.content is Newtonsoft.Json.Linq.JArray dataJArray) {
                        //
                        // -- newtonsoft types
                        result = dataJArray.ToObject<TData>();
                    } else if (cacheDocument.content == null) {
                        //
                        // -- if cache data was left as a string (might be empty), and return object is not string, there was an error
                        result = default;
                    } else {
                        //
                        // -- all worked, but if the class is unavailable let it return default like a miss
                        try {
                            result = (TData)cacheDocument.content;
                        } catch (Exception ex) {
                            //
                            // -- object value did not match. return as miss
                            LogController.logWarn(core, "cache getObject failed to cast value as type, key [" + key + "], type requested [" +  typeof(TData).FullName + "], ex [" + ex + "]");
                            result = default;
                        }
                    }
                }
                return result;
            } catch (Exception ex) {
                LogController.logError(core, ex);
                return default;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, return ""
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string getText(string key) => getObject<string>(key) ?? "";
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, return 0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int getInteger(string key) => getObject<int>(key);
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, return 0.0
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public double getNumber(string key) => getObject<double>(key);
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, return minDate
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public DateTime getDate(string key) => getObject<DateTime>(key);
        //
        //========================================================================
        /// <summary>
        /// get an object from cache. If the cache misses or is invalidated, return false
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool getBoolean(string key) => getObject<bool>(key);
        //
        //====================================================================================================
        /// <summary>
        /// get a cache object from the cache. returns the cacheObject that wraps the object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private CacheDocumentClass getCacheDocument(string key) {
            CacheDocumentClass result = null;
            try {
                // - verified in createServerKey() -- key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("cache key cannot be blank");
                }
                string serverKey = createServerKey(key);
                string typeMessage = "";
                if (remoteCacheInitialized) {
                    //
                    // -- use remote cache
                    typeMessage = "remote";
                    try {
                        result = cacheClient.Get<CacheDocumentClass>(serverKey);
                    } catch (Exception ex) {
                        //
                        // --client does not throw its own errors, so try to differentiate by message
                        if (ex.Message.ToLowerInvariant().IndexOf("unable to load type") >= 0) {
                            //
                            // -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                            LogController.logWarn(core, ex);
                            cacheClient.Remove(serverKey);
                            result = null;
                        } else {
                            //
                            // -- some other error
                            LogController.logError(core, ex);
                            throw;
                        }
                    }
                }
                if ((result == null) && core.serverConfig.enableLocalMemoryCache) {
                    //
                    // -- local memory cache
                    typeMessage = "local-memory";
                    result = (CacheDocumentClass)MemoryCache.Default[serverKey];
                }
                if ((result == null) && core.serverConfig.enableLocalFileCache) {
                    //
                    // -- local file cache
                    typeMessage = "local-file";
                    string serializedDataObject = null;
                    using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, serverKey)) {
                        mutex.WaitOne();
                        serializedDataObject = core.privateFiles.readFileText("appCache\\" + FileController.encodeDosFilename(serverKey + ".txt"));
                        mutex.ReleaseMutex();
                    }
                    if (!string.IsNullOrEmpty(serializedDataObject)) {
                        result = DeserializeObject<CacheDocumentClass>(serializedDataObject);
                        storeCacheDocument_MemoryCache(serverKey, result);
                    }
                }
                string returnContentSegment = SerializeObject(result);
                returnContentSegment = (returnContentSegment.Length > 50) ? returnContentSegment.Substring(0, 50) : returnContentSegment;
                //
                // -- log result
                if (result == null) {
                    LogController.logTrace(core, "miss, cacheType [" + typeMessage + "], key [" + key + "]");
                } else {
                    if (result.content == null) {
                        LogController.logTrace(core, "hit, cacheType [" + typeMessage + "], key [" + key + "], saveDate [" + result.saveDate + "], content [null]");
                    } else {
                        string content = result.content.ToString();
                        content = (content.Length > 50) ? (content.left(50) + "...") : content;
                        LogController.logTrace(core, "hit, cacheType [" + typeMessage + "], key [" + key + "], saveDate [" + result.saveDate + "], content [" + content + "]");
                    }
                }
                //
                // if dependentKeyList is null, return an empty list, not null
                if (result != null) {
                    //
                    // -- empty objects return nothing, empty lists return count=0
                    if (result.dependentKeyList == null) {
                        result.dependentKeyList = new List<string>();
                    }
                }

            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                var cacheDocument = new CacheDocumentClass(core.dateTimeNowMockable) {
                    content = content,
                    saveDate = core.dateTimeNowMockable,
                    invalidationDate = invalidationDate,
                    dependentKeyList = dependentKeyList
                };
                storeCacheDocument(key, cacheDocument);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
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
            storeObject(key, content, core.dateTimeNowMockable.AddDays(invalidationDaysDefault), dependentKeyList);
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
            storeObject(key, content, core.dateTimeNowMockable.AddDays(invalidationDaysDefault), new List<string> { dependantKey });
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
            storeObject(key, content, core.dateTimeNowMockable.AddDays(invalidationDaysDefault), new List<string> { });
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
        /// <param name="content"></param>
        public void storeRecord(string guid, int recordId, string tableName, string datasourceName, object content) {
            string key = createRecordKey(recordId, tableName, datasourceName);
            storeObject(key, content);
            string keyPtr = createRecordGuidPtrKey(guid, tableName, datasourceName);
            storePtr(keyPtr, key);
        }
        //
        //====================================================================================================
        /// <summary>
        /// future method. To support a cpbase implementation, but wait until BaseModel is exposed
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guid"></param>
        /// <param name="recordId"></param>
        /// <param name="content"></param>
        public void storeRecord<T>(string guid, int recordId, object content) where T : DbBaseModel {
            Type derivedType = this.GetType();
            FieldInfo fieldInfoTable = derivedType.GetField("tableNameLower");
            if (fieldInfoTable == null) {
                throw new GenericException("Class [" + derivedType.Name + "] must declare constant [contentTableName].");
            } else {
                string tableName = fieldInfoTable.GetRawConstantValue().ToString();
                FieldInfo fieldInfoDatasource = derivedType.GetField("contentDataSource");
                if (fieldInfoDatasource == null) {
                    throw new GenericException("Class [" + derivedType.Name + "] must declare public constant [contentDataSource].");
                } else {
                    string datasourceName = fieldInfoDatasource.GetRawConstantValue().ToString();
                    string key = createRecordKey(recordId, tableName, datasourceName);
                    storeObject(key, content);
                    string keyPtr = createRecordGuidPtrKey(guid, tableName, datasourceName);
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
                CacheDocumentClass cacheDocument = new CacheDocumentClass(core.dateTimeNowMockable) {
                    saveDate = core.dateTimeNowMockable,
                    invalidationDate = core.dateTimeNowMockable.AddDays(invalidationDaysDefault),
                    keyPtr = key
                };
                storeCacheDocument(keyPtr, cacheDocument);
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidates the entire cache (except those entires written with saveRaw)
        /// </summary>
        /// <remarks></remarks>
        public void invalidateAll()  {
            try {
                string key = Regex.Replace(cacheNameGlobalInvalidationDate, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                storeCacheDocument(key, new CacheDocumentClass(core.dateTimeNowMockable) { saveDate = core.dateTimeNowMockable });
                _globalInvalidationDate = null;
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
                Controllers.LogController.logTrace(core, "invalidate, key [" + key + "], recursionLimit [" + recursionLimit + "]");
                if ((recursionLimit > 0) && (!string.IsNullOrWhiteSpace(key.Trim()))) {
                    key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
                    // if key is a ptr, we need to invalidate the real key
                    CacheDocumentClass cacheDocument = getCacheDocument(key);
                    if (cacheDocument == null) {
                        // no cache for this key, if this is a dependency for another key, save invalidated
                        storeCacheDocument(key, new CacheDocumentClass(core.dateTimeNowMockable) { saveDate = core.dateTimeNowMockable });
                    } else {
                        if (!string.IsNullOrWhiteSpace(cacheDocument.keyPtr)) {
                            // this key is an alias, invalidate it's parent key
                            invalidate(cacheDocument.keyPtr, --recursionLimit);
                        } else {
                            // key is a valid cache, invalidate it
                            storeCacheDocument(key, new CacheDocumentClass(core.dateTimeNowMockable) { saveDate = core.dateTimeNowMockable });
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
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
        public void invalidateRecordKey(int recordId, string tableName, string dataSourceName = "default") {
            invalidate(createRecordKey(recordId, tableName, dataSourceName));
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
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert a key to be used in the server cache. Normalizes name, adds app name and code version
        /// </summary>
        /// <param name="key">The cache key to be converted</param>
        /// <returns></returns>
        private string createServerKey(string key) {
            string result = core.appConfig.name + "-" + key;
            result = Regex.Replace(result, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the standard key for records. Store for this key should only be the object model for this id. Get for this key should return null or an object of the model.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createRecordKey(int recordId, string tableName, string dataSourceName) {
            string key = (String.IsNullOrWhiteSpace(dataSourceName)) ? "dbtable/default/" : "dbtable/" + dataSourceName.Trim() + "/";
            key += tableName.Trim() + "/id/" + recordId + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return the standard key for records. Store for this key should only be the object model for this id. Get for this key should return null or an object of the model.
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string createRecordKey(int recordId, string tableName) => createRecordKey(recordId, tableName, "default");
        //
        //====================================================================================================
        /// <summary>
        /// return pointer key used in storePtr() to associate a guid to a record cache
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createRecordGuidPtrKey(string guid, string tableName, string dataSourceName) {
            string key = "dbptr/" + dataSourceName + "/" + tableName + "/ccguid/" + guid + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return pointer key used in storePtr() to associate a guid to a record cache
        /// </summary>
        /// <param name="recordGuid"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string createRecordGuidPtrKey(string recordGuid, string tableName) => createRecordGuidPtrKey(recordGuid, tableName, "default");
        //
        //====================================================================================================
        /// <summary>
        /// return pointer key used in storePtr() to associate a name to a record cache
        /// </summary>
        /// <param name="recordName"></param>
        /// <param name="tableName"></param>
        /// <param name="datasourceName"></param>
        /// <returns></returns>
        public static string createRecordNamePtrKey(string recordName, string tableName, string datasourceName) {
            string key = "dbptr/" + datasourceName + "/" + tableName + "/name/" + recordName + "/";
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return pointer key used in storePtr() to associate a name to a record cache
        /// </summary>
        /// <param name="recordName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string createRecordNamePtrKey(string recordName, string tableName) => createRecordNamePtrKey(recordName, tableName, "default");
        //
        //====================================================================================================
        /// <summary>
        /// create a key to store unmanaged content with store(). Managed content like database model should use createRecordKey and createTableDependencyKey.
        /// </summary>
        /// <param name="objectUniqueName">The unique key that describes the object. Ex. catalogitemList, or metadata-134</param>
        /// <param name="objectUniqueIdentifier"></param>
        /// <returns></returns>
        public static string createKey(string objectUniqueName) {
            string key = "obj/" + objectUniqueName;
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
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
                    CacheDocumentClass dataObject = getCacheDocument(cacheNameGlobalInvalidationDate);
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
                    storeCacheDocument(cacheNameGlobalInvalidationDate, new CacheDocumentClass(core.dateTimeNowMockable) { saveDate = encodeDate(_globalInvalidationDate) });
                }
                return encodeDate(_globalInvalidationDate);
            }
        }
        private DateTime? _globalInvalidationDate;
        //
        //====================================================================================================
        /// <summary>
        /// save object directly to cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheDocument">Either a string, a date, or a serializable object</param>
        /// <param name="invalidationDate"></param>
        /// <remarks></remarks>
        private void storeCacheDocument(string key, CacheDocumentClass cacheDocument) {
            try {
                //
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("cache key cannot be blank");
                }
                string typeMessage = "";
                string serverKey = createServerKey(key);
                if (core.serverConfig.enableLocalMemoryCache) {
                    //
                    // -- save local memory cache
                    typeMessage = "local-memory";
                    storeCacheDocument_MemoryCache(serverKey, cacheDocument);
                }
                if (core.serverConfig.enableLocalFileCache) {
                    //
                    // -- save local file cache
                    typeMessage = "local-file";
                    string serializedData = SerializeObject(cacheDocument);
                    using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, serverKey)) {
                        mutex.WaitOne();
                        core.privateFiles.saveFile("appCache\\" + FileController.encodeDosFilename(serverKey + ".txt"), serializedData);
                        mutex.ReleaseMutex();
                    }
                }
                if (core.serverConfig.enableRemoteCache) {
                    typeMessage = "remote";
                    if (remoteCacheInitialized) {
                        //
                        // -- save remote cache
                        if( !cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, serverKey, cacheDocument, cacheDocument.invalidationDate)) {
                            //
                            // -- store failed
                            LogController.logError(core, "Enyim cacheClient.Store failed, no details available.");
                        }
                    }
                }
                //
                LogController.logTrace(core, "cacheType [" + typeMessage + "], key [" + key + "], expires [" + cacheDocument.invalidationDate + "], depends on [" + string.Join(",", cacheDocument.dependentKeyList) + "], points to [" + string.Join(",", cacheDocument.keyPtr) + "]");
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save cacheDocument to memory cache
        /// </summary>
        /// <param name="serverKey">key converted to serverKey with app name and code version</param>
        /// <param name="cacheDocument"></param>
        public void storeCacheDocument_MemoryCache(string serverKey, CacheDocumentClass cacheDocument) {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy {
                AbsoluteExpiration = cacheDocument.invalidationDate // core.dateTimeMockable.AddMinutes(100);
            };
            cache.Set(serverKey, cacheDocument, policy);
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the Table Dependency Key. This will invalidate any key set to be dependent on this key
        /// </summary>
        /// <param name="core"></param>
        public void invalidateTableDependencyKey(string tableName) {
            storeObject(createTableDependencyKey(tableName), core.dateTimeNowMockable);
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a table dependency key, used in store() to invalidate the content if any record in the table is modified
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public static string createTableDependencyKey(string tableName, string dataSourceName) {
            string key = "tabledependency/" + ((String.IsNullOrWhiteSpace(dataSourceName)) ? "default/" + tableName.Trim().ToLowerInvariant() + "/" : dataSourceName.Trim().ToLowerInvariant() + "/" + tableName.Trim().ToLowerInvariant() + "/");
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLowerInvariant().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a table dependency key, used in store() to invalidate the content if any record in the table is modified
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string createTableDependencyKey(string tableName) => createTableDependencyKey(tableName, "default");
        //
        //====================================================================================================
        //
        private static TData DeserializeFromString<TData>(string settings) {
            byte[] b = Convert.FromBase64String(settings);
            using (var stream = new MemoryStream(b)) {
                var formatter = new BinaryFormatter();
                stream.Seek(0, SeekOrigin.Begin);
                return (TData)formatter.Deserialize(stream);
            }
        }
        //
        //====================================================================================================
        //
        private static string SerializeToString<TData>(TData settings) {
            using (var stream = new MemoryStream()) {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, settings);
                stream.Flush();
                stream.Position = 0;
                return Convert.ToBase64String(stream.ToArray());
            }
        }
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed;
        //
        public void Dispose()  {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~CacheController()  {
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