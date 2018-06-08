
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.DbModels;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
//
using System.Runtime.Caching;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// Interface to cache systems. Cache objects are saved to dotnet cache, remotecache, filecache. 
    /// 
    /// 3 types of key
    /// -- 1) primary key -- cache key holding the content.
    /// -- 2) dependent key -- a dependent key holds content that the primary cache depends on. If the dependent cache is invalid, the primary cache is invalid
    /// -------- if an org cache includes a primary content person, then the org (org/id/10) is saved with a dependent key (ccmembers/id/99). The org content id valid only if both the primary and dependent keys are valid
    /// -- 3) alias (pointer) key -- a cache entry that holds a primary key (or another pointer key). When read, the cache returns the primary value. (primary="ccmembers/id/10", pointer="ccmembers/ccguid/{1234}"
    /// ------- pointer keys are read-only, as the content is saved in the primary key
    /// 
    /// three  types of cache entries:
    ///  -- simple entity cache -- cache of an entity model (one record)
    ///     .. saveCache, reachCache in the entity model
    ///     .. invalidateCache in the entity model, includes invalication for complex objects that include the entity
    ///      
    ///  -- complex entity cache -- cache of an object with mixed data based on the entity data model (an org object that contains a person object)
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
    public class cacheController : IDisposable {
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
        private coreController core;
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
        /// get an object from cache. If the cache misses or is invalidated, nothing object is returned
        /// </summary>
        /// <typeparam name="objectClass"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public objectClass getObject<objectClass>(string key) {
            objectClass result = default(objectClass);
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                if (!(string.IsNullOrEmpty(key))) {
                    //
                    // -- read cacheWrapper
                    cacheWrapperClass wrappedContent = getWrappedContent(key);
                    if (wrappedContent != null) {
                        //
                        // -- test for global invalidation
                        int dateCompare = globalInvalidationDate.CompareTo(wrappedContent.saveDate);
                        if (dateCompare >= 0) {
                            //
                            // -- global invalidation
                            logController.logTrace( core,"GetObject(" + key + "), invalidated because the cacheObject's saveDate [" + wrappedContent.saveDate.ToString() + "] is after the globalInvalidationDate [" + globalInvalidationDate + "]");
                        } else {
                            //
                            // -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
                            bool cacheMiss = false;
                            foreach (string dependentKey in wrappedContent.dependentKeyList) {
                                cacheWrapperClass dependantObject = getWrappedContent(dependentKey);
                                if (dependantObject == null) {
                                    // create dummy cache to validate future cache requests, fake saveDate as last globalinvalidationdate
                                    setWrappedContent(dependentKey, new cacheWrapperClass() {
                                        aliasPointsToKey = null,
                                        content = "",
                                        saveDate = globalInvalidationDate
                                    });
                                } else {
                                    dateCompare = dependantObject.saveDate.CompareTo(wrappedContent.saveDate);
                                    if (dateCompare >= 0) {
                                        cacheMiss = true;
                                        logController.logTrace(core, "GetObject(" + key + "), invalidated because the dependantobject [" + dependentKey + "] has a saveDate [" + dependantObject.saveDate.ToString() + "] after the cacheObject's saveDate [" + wrappedContent.saveDate.ToString() + "]");
                                        break;
                                    }
                                }
                            }
                            if (!cacheMiss) {
                                if (!string.IsNullOrEmpty(wrappedContent.aliasPointsToKey)) {
                                    //
                                    // -- this is a pointer key, load the primary
                                    result = getObject<objectClass>(wrappedContent.aliasPointsToKey);
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
                logController.handleError( core,ex);
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
        private cacheWrapperClass getWrappedContent(string key) {
            cacheWrapperClass result = null;
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("key cannot be blank");
                } else {
                    string wrapperKey = getWrapperKey(key);
                    if (remoteCacheInitialized) {
                        //
                        // -- use remote cache
                        try {
                            result = cacheClient.Get<cacheWrapperClass>(wrapperKey);
                        } catch (Exception ex) {
                            //
                            // --client does not throw its own errors, so try to differentiate by message
                            if (ex.Message.ToLower().IndexOf("unable to load type") >= 0) {
                                //
                                // -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
                                cacheClient.Remove(wrapperKey);
                                result = null;
                            } else {
                                //
                                // -- some other error
                                logController.handleError( core,ex);
                                throw;
                            }
                        }
                        if (result != null) {
                            logController.logTrace(core, "getCacheWrapper(" + key + "), remoteCache hit");
                        } else {
                            logController.logTrace(core,"getCacheWrapper(" + key + "), remoteCache miss");
                        }
                    }
                    if ((result == null) && core.serverConfig.enableLocalMemoryCache) {
                        //
                        // -- local memory cache
                        //Dim cache As ObjectCache = MemoryCache.Default
                        result = (cacheWrapperClass)MemoryCache.Default[wrapperKey];
                        if (result != null) {
                            logController.logTrace(core, "getCacheWrapper(" + key + "), memoryCache hit");
                        } else {
                            logController.logTrace(core,"getCacheWrapper(" + key + "), memoryCache miss");
                        }
                    }
                    if ((result == null) && core.serverConfig.enableLocalFileCache) {
                        //
                        // -- local file cache
                        string serializedDataObject = null;
                        using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, wrapperKey)) {
                            mutex.WaitOne();
                            serializedDataObject = core.privateFiles.readFileText("appCache\\" + fileController.encodeDosFilename(wrapperKey + ".txt"));
                            mutex.ReleaseMutex();
                        }
                        if (string.IsNullOrEmpty(serializedDataObject)) {
                            logController.logTrace(core,"getCacheWrapper(" + key + "), file miss");
                        } else {
                            //logController.appendCacheLog(core,"getCacheWrapper(" + key + "), file hit, write to memory cache");
                            result = Newtonsoft.Json.JsonConvert.DeserializeObject<cacheWrapperClass>(serializedDataObject);
                            setWrappedContent_MemoryCache(wrapperKey, result);
                        }
                        if (result != null) {
                            //logController.appendCacheLog(core,"getCacheWrapper(" + key + "), fileCache hit");
                        } else {
                            logController.logTrace(core,"getCacheWrapper(" + key + "), fileCache miss");
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
                //logController.appendCacheLog(core,"getCacheWrapper(" + key + "), exit ");
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// save a string to cache, for compatibility with existing site. (no objects, no invalidation, yet)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <remarks></remarks>
        public void setObject(string key, object content) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass wrappedContent = new cacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
                    dependentKeyList = null
                };
                setWrappedContent(key, wrappedContent);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
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
        /// <param name="dependentKeyList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
        /// <remarks></remarks>
        public void setObject(string key, object content, DateTime invalidationDate) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass wrappedContent = new cacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = invalidationDate
                };
                setWrappedContent(key, wrappedContent);
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
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
        public void setObject(string key, object content, DateTime invalidationDate, List<string> dependentKeyList) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass wrappedContent = new cacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = invalidationDate,
                    dependentKeyList = dependentKeyList
                };
                setWrappedContent(key, wrappedContent);
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save an object to cache, with invalidation date and dependentKey
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="invalidationDate"></param>
        /// <param name="dependantKey">The cache name of an object that .</param>
        /// <remarks></remarks>
        public void setObject(string key, object content, DateTime invalidationDate, string dependantKey) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass cacheWrapper = new cacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = invalidationDate,
                    dependentKeyList = new List<string> { dependantKey }
                };
                setWrappedContent(key, cacheWrapper);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
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
        public void setObject(string key, object content, List<string> dependentKeyList) {
            try {
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass cacheWrapper = new cacheWrapperClass {
                    content = content,
                    saveDate = DateTime.Now,
                    invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
                    dependentKeyList = dependentKeyList
                };
                setWrappedContent(key, cacheWrapper);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
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
        /// <param name="dependantKey"></param>
        /// <remarks></remarks>
        public void setObject(string key, object content, string dependantKey) {
            try {
                setObject(key, content, new List<string> { dependantKey });
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// set a key alias. An alias points to a normal key, creating an altername way to get/invalidate a cache.
        /// ex - image with id=10, guid={999}. The normal key="image/id/10", the alias Key="image/ccguid/{9999}"
        /// </summary>
        /// <param name="CP"></param>
        /// <param name="keyAlias"></param>
        /// <param name="data"></param>
        /// <param name="dependantObject"></param>
        /// <remarks></remarks>
        public void setAlias(string keyAlias, string key) {
            try {
                keyAlias = Regex.Replace(keyAlias, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                cacheWrapperClass cacheWrapper = new cacheWrapperClass {
                    saveDate = DateTime.Now,
                    invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
                    aliasPointsToKey = key
                };
                setWrappedContent(keyAlias, cacheWrapper);
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                string key = Regex.Replace(cacheNameGlobalInvalidationDate, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                setWrappedContent(key, new cacheWrapperClass { saveDate = DateTime.Now });
                _globalInvalidationDate = null;
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                    // if key is an alias, we need to invalidate the real key
                    cacheWrapperClass wrapper = getWrappedContent(key);
                    if (wrapper == null) {
                        // no cache for this key, if this is a dependency for another key, save invalidated
                        setWrappedContent(key, new cacheWrapperClass { saveDate = DateTime.Now });
                    } else { 
                        if (!string.IsNullOrWhiteSpace(wrapper.aliasPointsToKey)) {
                            // this key is an alias, invalidate it's parent key
                            invalidate(wrapper.aliasPointsToKey, --recursionLimit);
                        } else {
                            // key is a valid cache, invalidate it
                            setWrappedContent(key, new cacheWrapperClass { saveDate = DateTime.Now });
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// update the dbTablename cache entry. Do this anytime any record is updated in the table
        /// </summary>
        /// <param name="ContentName"></param>
        public void invalidateAllInContent(string ContentName) {
            try {
                //
                logController.logTrace(core,"invalidateAllObjectsInContent(" + ContentName + ")");
                //
                // -- save the cache key that represents any record in the content, set as a dependent key for saves
                invalidate(ContentName);
                invalidate(Models.Complex.cdefModel.getContentTablename(core, ContentName));
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //========================================================================
        /// <summary>
        /// invalidate all cache entries to this table. "dbTableName"
        /// when any cache is saved, it should include a dependancy on a cachename=dbtablename
        /// </summary>
        /// <param name="dbTableName"></param>
        public void invalidateAllInTable(string dbTableName) {
            try {
                string key = Regex.Replace(dbTableName, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
                invalidate(key);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// invalidates a list of tags 
        /// </summary>
        /// <param name="keyList"></param>
        /// <remarks></remarks>
        public void invalidate(List<string> keyList) {
            try {
                foreach (var key in keyList) {
                    invalidate(key);
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //=======================================================================
        /// <summary>
        /// Encode a string to be memCacheD compatible, removing 0x00-0x20 and space
        /// </summary>
        private string getWrapperKey(string key) {
            string result = core.appConfig.name + "-" + core.codeVersion() + "-" + key;
            result = Regex.Replace(result, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// create a cache key for an entity model (based on the Db)
        /// </summary>
        public static string getCacheKey_Entity(string tableName, string fieldName, string fieldValue) {
            string key = tableName + "/" + fieldName + "/" + fieldValue;
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
            return key;
        }
        //
        public static string getCacheKey_Entity(string tableName, int recordId) {
            string key = tableName + "/id/" + recordId.ToString();
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
            return key;
        }
        ////
        //public static string getCacheKey_Entity(string tableName, string field1Name, int field1Value, string field2Name, int field2Value) {
        //    string key = tableName + "/" + field1Name + "/" + field1Value + "/" + field2Name + "/" + field2Value;
        //    key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
        //    return key;
        //}
        //
        //====================================================================================================
        /// <summary>
        /// create a cache name for an object composed of data not from a signel record
        /// </summary>
        public static string getCacheKey_ComplexObject(string objectName, string uniqueObjectIdentifier) {
            string key = "complexobject/" + objectName + "/" + uniqueObjectIdentifier;
            key = Regex.Replace(key, "0x[a-fA-F\\d]{2}", "_").ToLower().Replace(" ", "_");
            return key;
        }
        //
        //====================================================================================================
        //
        public void invalidateContent_Entity(coreController core, string tableName, int recordId) {
            //
            invalidate(getCacheKey_Entity(tableName, recordId));
            if (tableName.ToLower() == linkAliasModel.contentTableName.ToLower()) {
                //
                Models.Complex.routeDictionaryModel.invalidateCache(core);
            } else if (tableName.ToLower() == linkForwardModel.contentTableName.ToLower()) {
                //
                Models.Complex.routeDictionaryModel.invalidateCache(core);
            } else if (tableName.ToLower() == addonModel.contentTableName.ToLower()) {
                //
                Models.Complex.routeDictionaryModel.invalidateCache(core);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// cache data wrapper to include tags and save datetime
        /// </summary>
        [Serializable()]
        public class cacheWrapperClass {
            //
            // if populated, all other properties are ignored and the primary tag b
            public string aliasPointsToKey;
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
                    cacheWrapperClass dataObject = getWrappedContent(cacheNameGlobalInvalidationDate);
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
                    setWrappedContent(cacheNameGlobalInvalidationDate, new cacheWrapperClass { saveDate = encodeDate(_globalInvalidationDate) });
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
        public cacheController(coreController core) {
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
                            cacheEndpointPort = genericController.encodeInteger(cacheEndpointSplit[1]);
                        }
                        Amazon.ElastiCacheCluster.ElastiCacheClusterConfig cacheConfig = new Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(cacheEndpointSplit[0], cacheEndpointPort);
                        cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary;
                        cacheClient = new Enyim.Caching.MemcachedClient(cacheConfig);
                        if (cacheClient != null) {
                            remoteCacheInitialized = true;
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // -- client does not throw its own errors, so try to differentiate by message
                throw (new ApplicationException("Exception initializing remote cache, will continue with cache disabled.", ex));
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save object directly to cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="wrappedContent">Either a string, a date, or a serializable object</param>
        /// <param name="invalidationDate"></param>
        /// <remarks></remarks>
        private void setWrappedContent(string key, cacheWrapperClass wrappedContent) {
            try {
                //
                //logController.appendCacheLog(core,"setWrappedContent(" + key + ")");
                //
                if (string.IsNullOrEmpty(key)) {
                    throw new ArgumentException("cache key cannot be blank");
                } else {
                    string wrapperKey = getWrapperKey(key);
                    if (core.serverConfig.enableLocalMemoryCache) {
                        //
                        // -- save local memory cache
                        setWrappedContent_MemoryCache(wrapperKey, wrappedContent);
                    }
                    if (core.serverConfig.enableLocalFileCache) {
                        //
                        // -- save local file cache
                        string serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(wrappedContent);
                        using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, wrapperKey)) {
                            mutex.WaitOne();
                            core.privateFiles.saveFile("appCache\\" + fileController.encodeDosFilename(wrapperKey + ".txt"), serializedData);
                            mutex.ReleaseMutex();
                        }
                    }
                    if (core.serverConfig.enableRemoteCache) {
                        if (remoteCacheInitialized) {
                            //
                            // -- save remote cache
                            cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, wrapperKey, wrappedContent, wrappedContent.invalidationDate);
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// save cacheWrapper to memory cache
        /// </summary>
        /// <param name="wrapperKey"></param>
        /// <param name="wrappedContent"></param>
        public void setWrappedContent_MemoryCache(string wrapperKey, cacheWrapperClass wrappedContent) {
            ObjectCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = wrappedContent.invalidationDate; // DateTime.Now.AddMinutes(100);
            cache.Set(wrapperKey, wrappedContent, policy);
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
        ~cacheController() {
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