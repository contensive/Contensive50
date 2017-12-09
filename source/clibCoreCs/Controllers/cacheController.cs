
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
using System.Runtime.Caching;
//
namespace Contensive.Core.Controllers {
	//
	//====================================================================================================
	/// <summary>
	/// Interface to cache systems. Cache objects are saved to dotnet cache, remotecache, filecache. 
	/// 
	/// 3 types of key
	/// -- 1) primary key -- cache key holding the content.
	/// -- 2) dependent key -- a dependent key holds content that the primary cache depends on. If the dependent cache is invalid, the primary cache is invalid
	/// -------- if an org cache includes a primary content person, then the org (org/id/10) is saved with a dependent key (ccmembers/id/99). The org content id valid only if both the primary and dependent keys are valid
	/// -- 3) pointer key -- a cache entry that holds a primary key (or another pointer key). When read, the cache returns the primary value. (primary="ccmembers/id/10", pointer="ccmembers/ccguid/{1234}"
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
		// ----- constants
		//
		private const double invalidationDaysDefault = 365;
		//
		// ----- objects passed in constructor, do not dispose
		//
		private coreClass cpCore;
		//
		// ----- objects constructed that must be disposed
		//
		private Enyim.Caching.MemcachedClient cacheClient;
		//
		// ----- private instance storage
		//
		private bool remoteCacheInitialized;
		//
		//====================================================================================================
		/// <summary>
		/// cache data wrapper to include tags and save datetime
		/// </summary>
		[Serializable()]
		public class cacheWrapperClass
		{
			public string key; // if populated, all other properties are ignored and the primary tag b
			public List<string> dependentKeyList = new List<string>(); // this object is invalidated if any of these objects are invalidated
			public DateTime saveDate; // the date this object was last saved.
			public DateTime invalidationDate; // the future date when this object self-invalidates
			public object content; // the data storage
		}
		//
		//====================================================================================================
		/// <summary>
		/// returns the system globalInvalidationDate. This is the date/time when the entire cache was last cleared. Every cache object saved before this date is considered invalid.
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		private DateTime globalInvalidationDate
		{
			get
			{
				//
				if (_globalInvalidationDate == null)
				{
					cacheWrapperClass dataObject = getWrappedContent("globalInvalidationDate");
					if (dataObject != null)
					{
						_globalInvalidationDate = dataObject.saveDate;
					}
					if (_globalInvalidationDate == null)
					{
						_globalInvalidationDate = DateTime.Now;
						setWrappedContent("globalInvalidationDate", new cacheWrapperClass {saveDate = Convert.ToDateTime(_globalInvalidationDate)});
					}
				}
				if (!_globalInvalidationDate.HasValue)
				{
					_globalInvalidationDate = DateTime.Now.AddDays(invalidationDaysDefault);
				}
				else
				{
					if ((Convert.ToDateTime(_globalInvalidationDate)).CompareTo(new DateTime(1990, 8, 7)) < 0)
					{
						_globalInvalidationDate = new DateTime(1990, 8, 7);
					}
				}
				return Convert.ToDateTime(_globalInvalidationDate);
			}
		}
		private DateTime? _globalInvalidationDate;
		//
		//====================================================================================================
		/// <summary>
		/// Initializes cache client
		/// </summary>
		/// <remarks></remarks>
		public cacheController(coreClass cpCore)
		{
			try
			{
				this.cpCore = cpCore;
				//
				_globalInvalidationDate = null;
				remoteCacheInitialized = false;
				if (cpCore.serverConfig.enableRemoteCache)
				{
					string cacheEndpoint = cpCore.serverConfig.awsElastiCacheConfigurationEndpoint;
					if (!string.IsNullOrEmpty(cacheEndpoint))
					{
						string[] cacheEndpointSplit = cacheEndpoint.Split(':');
						int cacheEndpointPort = 11211;
						if (cacheEndpointSplit.GetUpperBound(0) > 1)
						{
							cacheEndpointPort = genericController.EncodeInteger(cacheEndpointSplit[1]);
						}
						Amazon.ElastiCacheCluster.ElastiCacheClusterConfig cacheConfig = new Amazon.ElastiCacheCluster.ElastiCacheClusterConfig(cacheEndpointSplit[0], cacheEndpointPort);
						cacheConfig.Protocol = Enyim.Caching.Memcached.MemcachedProtocol.Binary;
						cacheClient = new Enyim.Caching.MemcachedClient(cacheConfig);
						if (cacheClient != null)
						{
							remoteCacheInitialized = true;
						}
					}
				}
			}
			catch (Exception ex)
			{
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
		private void setWrappedContent(string key, cacheWrapperClass wrappedContent)
		{
			try
			{
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("cache key cannot be blank");
				}
				else
				{
					string wrapperKey = getWrapperKey(cpCore.serverConfig.appConfig.name, key);
					if (cpCore.serverConfig.enableRemoteCache)
					{
						if (remoteCacheInitialized)
						{
							//
							// -- save remote cache
							cacheClient.Store(Enyim.Caching.Memcached.StoreMode.Set, wrapperKey, wrappedContent, wrappedContent.invalidationDate);
						}
						if (cpCore.serverConfig.enableLocalMemoryCache)
						{
							//
							// -- save local memory cache
							setWrappedContent_MemoryCache(wrapperKey, wrappedContent);
						}
						if (cpCore.serverConfig.enableLocalFileCache)
						{
							//
							// -- save local memory cache
							string serializedData = Newtonsoft.Json.JsonConvert.SerializeObject(wrappedContent);
							using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, wrapperKey))
							{
								mutex.WaitOne();
								cpCore.privateFiles.saveFile("appCache\\" + genericController.encodeFilename(wrapperKey + ".txt"), serializedData);
								mutex.ReleaseMutex();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// save cacheWrapper to memory cache
		/// </summary>
		/// <param name="wrapperKey"></param>
		/// <param name="wrappedContent"></param>
		public void setWrappedContent_MemoryCache(string wrapperKey, cacheWrapperClass wrappedContent)
		{
			ObjectCache cache = MemoryCache.Default;
			CacheItemPolicy policy = new CacheItemPolicy();
			policy.AbsoluteExpiration = DateTime.Now.AddMinutes(100);
			cache.Set(wrapperKey, wrappedContent, policy);
		}
		//
		//========================================================================
		/// <summary>
		/// get an object from cache. If the cache misses or is invalidated, nothing object is returned
		/// </summary>
		/// <typeparam name="objectClass"></typeparam>
		/// <param name="key"></param>
		/// <returns></returns>
		public objectClass getObject<objectClass>(string key)
		{
			objectClass result = default(objectClass);
			try
			{
				cacheWrapperClass wrappedContent = null;
				bool cacheMiss = false;
				int dateCompare = 0;
				//
				if (!(string.IsNullOrEmpty(key)))
				{
					//
					// -- read cacheWrapper
				appendCacheLog("getObject(" + key + "), enter");
					Stopwatch sw = Stopwatch.StartNew();
					wrappedContent = getWrappedContent(key);
					if (wrappedContent != null)
					{
						//
						// -- test for global invalidation
						dateCompare = globalInvalidationDate.CompareTo(wrappedContent.saveDate);
						if (dateCompare >= 0)
						{
							//
							// -- global invalidation
							appendCacheLog("GetObject(" + key + "), invalidated because the cacheObject's saveDate [" + wrappedContent.saveDate.ToString() + "] is after the globalInvalidationDate [" + globalInvalidationDate + "]");
						}
						else
						{
							//
							// -- test all dependent objects for invalidation (if they have changed since this object changed, it is invalid)
							if (wrappedContent.dependentKeyList.Count == 0)
							{
								//
								// -- no dependent objects
								appendCacheLog("GetObject(" + key + "), no dependentKeyList");
							}
							else
							{
								foreach (string dependentKey in wrappedContent.dependentKeyList)
								{
									cacheWrapperClass dependantObject = getWrappedContent(dependentKey);
									if (dependantObject != null)
									{
										dateCompare = dependantObject.saveDate.CompareTo(wrappedContent.saveDate);
										if (dateCompare >= 0)
										{
											cacheMiss = true;
											appendCacheLog("GetObject(" + key + "), invalidated because the dependantobject [" + dependentKey + "] has a saveDate [" + dependantObject.saveDate.ToString() + "] after the cacheObject's dateDate [" + wrappedContent.saveDate.ToString() + "]");
											break;
										}
									}
								}
							}
							if (!cacheMiss)
							{
								if (!string.IsNullOrEmpty(wrappedContent.key))
								{
									//
									// -- this is a pointer key, load the primary
									result = getObject<objectClass>(wrappedContent.key);
								}
								else if (wrappedContent.content is Newtonsoft.Json.Linq.JObject)
								{
									//
									// -- newtonsoft types
									Newtonsoft.Json.Linq.JObject data = (Newtonsoft.Json.Linq.JObject)wrappedContent.content;
									result = data.ToObject<objectClass>();
								}
								else if (wrappedContent.content is Newtonsoft.Json.Linq.JArray)
								{
									//
									// -- newtonsoft types
									Newtonsoft.Json.Linq.JArray data = (Newtonsoft.Json.Linq.JArray)wrappedContent.content;
									result = data.ToObject<objectClass>();
								}
								else if ((wrappedContent.content is string) && (!(result is string)))
								{
									//
									// -- if cache data was left as a string (might be empty), and return object is not string, there was an error
									result = default(objectClass);
								}
								else
								{
									//
									// -- all worked
									result = (objectClass)wrappedContent.content;
								}
							}
						}
					}
					appendCacheLog("getObject(" + key + "), exit(" + sw.ElapsedMilliseconds + "ms)");
					sw.Stop();
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
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
		private cacheWrapperClass getWrappedContent(string key)
		{
			cacheWrapperClass result = null;
			try
			{
				appendCacheLog("getWrappedContent(" + key + "), enter ");
				if (string.IsNullOrEmpty(key))
				{
					throw new ArgumentException("key cannot be blank");
				}
				else
				{
					string wrapperKey = getWrapperKey(cpCore.serverConfig.appConfig.name, key);
					if (remoteCacheInitialized)
					{
						//
						// -- use remote cache
						try
						{
							result = cacheClient.Get<cacheWrapperClass>(wrapperKey);
						}
						catch (Exception ex)
						{
							//
							// --client does not throw its own errors, so try to differentiate by message
							if (ex.Message.ToLower().IndexOf("unable to load type") >= 0)
							{
								//
								// -- trying to deserialize an object and this code does not have a matching class, clear cache and return empty
								cacheClient.Remove(wrapperKey);
								result = null;
							}
							else
							{
								//
								// -- some other error
								cpCore.handleException(ex);
								throw;
							}
						}
						if (result != null)
						{
							appendCacheLog("getCacheWrapper(" + key + "), remoteCache hit");
						}
						else
						{
							appendCacheLog("getCacheWrapper(" + key + "), remoteCache miss");
						}
					}
					if ((result == null) && cpCore.serverConfig.enableLocalMemoryCache)
					{
						//
						// -- local memory cache
						//Dim cache As ObjectCache = MemoryCache.Default
						result = (cacheWrapperClass)MemoryCache.Default[wrapperKey];
						if (result != null)
						{
							appendCacheLog("getCacheWrapper(" + key + "), memoryCache hit");
						}
						else
						{
							appendCacheLog("getCacheWrapper(" + key + "), memoryCache miss");
						}
					}
					if ((result == null) && cpCore.serverConfig.enableLocalFileCache)
					{
						//
						// -- local file cache
						string serializedDataObject = null;
						using (System.Threading.Mutex mutex = new System.Threading.Mutex(false, wrapperKey))
						{
							mutex.WaitOne();
							serializedDataObject = cpCore.privateFiles.readFile("appCache\\" + genericController.encodeFilename(wrapperKey + ".txt"));
							mutex.ReleaseMutex();
						}
						if (string.IsNullOrEmpty(serializedDataObject))
						{
							appendCacheLog("getCacheWrapper(" + key + "), file miss");
						}
						else
						{
							appendCacheLog("getCacheWrapper(" + key + "), file hit, write to memory cache");
							result = Newtonsoft.Json.JsonConvert.DeserializeObject<cacheWrapperClass>(serializedDataObject);
							setWrappedContent_MemoryCache(wrapperKey, result);
						}
						if (result != null)
						{
							appendCacheLog("getCacheWrapper(" + key + "), fileCache hit");
						}
						else
						{
							appendCacheLog("getCacheWrapper(" + key + "), fileCache miss");
						}
					}
					if (result != null)
					{
						//
						// -- empty objects return nothing, empty lists return count=0
						if (result.dependentKeyList == null)
						{
							result.dependentKeyList = new List<string>();
						}
					}
				}
				appendCacheLog("getCacheWrapper(" + key + "), exit ");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void setContent(string key, object content)
		{
			try
			{
				cacheWrapperClass wrappedContent = new cacheWrapperClass
				{
					content = content,
					saveDate = DateTime.Now,
					invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
					dependentKeyList = null
				};
				setWrappedContent(key, wrappedContent);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// save an object to cache, with invalidation
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="content"></param>
		/// <param name="invalidationDate"></param>
		/// <param name="dependentKeyList">Each tag should represent the source of data, and should be invalidated when that source changes.</param>
		/// <remarks></remarks>
		public void setContent(string key, object content, DateTime invalidationDate, List<string> dependentKeyList)
		{
			try
			{
				cacheWrapperClass wrappedContent = new cacheWrapperClass
				{
					content = content,
					saveDate = DateTime.Now,
					invalidationDate = invalidationDate,
					dependentKeyList = dependentKeyList
				};
				setWrappedContent(key, wrappedContent);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// save an object to cache, with invalidation
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <param name="content"></param>
		/// <param name="invalidationDate"></param>
		/// <param name="dependantKey">The cache name of an object that .</param>
		/// <remarks></remarks>
		public void setContent(string key, object content, DateTime invalidationDate, string dependantKey)
		{
			try
			{
				cacheWrapperClass cacheWrapper = new cacheWrapperClass
				{
					content = content,
					saveDate = DateTime.Now,
					invalidationDate = invalidationDate,
					dependentKeyList = new List<string> {dependantKey}
				};
				setWrappedContent(key, cacheWrapper);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void setContent(string key, object content, List<string> dependentKeyList)
		{
			try
			{
				cacheWrapperClass cacheWrapper = new cacheWrapperClass
				{
					content = content,
					saveDate = DateTime.Now,
					invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
					dependentKeyList = dependentKeyList
				};
				setWrappedContent(key, cacheWrapper);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void setContent(string key, object content, string dependantKey)
		{
			try
			{
				//Dim dependantObjectList As New List(Of String)
				//dependantObjectList.Add(dependantObject)
				setContent(key, content, new List<string> {dependantKey});
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// set a pointer key. A pointer key just points to a cache key, creating an altername way to get/invalidate a cache.
		/// ex - image with id=10, guid={999}. The cache key="image/id/10", the pointerKey="image/ccguid/{9999}"
		/// </summary>
		/// <param name="CP"></param>
		/// <param name="pointerKey"></param>
		/// <param name="data"></param>
		/// <param name="dependantObject"></param>
		/// <remarks></remarks>
		public void setPointer(string pointerKey, string key)
		{
			try
			{
				cacheWrapperClass cacheWrapper = new cacheWrapperClass
				{
					saveDate = DateTime.Now,
					invalidationDate = DateTime.Now.AddDays(invalidationDaysDefault),
					key = key
				};
				setWrappedContent(pointerKey, cacheWrapper);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// invalidates the entire cache (except those entires written with saveRaw)
		/// </summary>
		/// <remarks></remarks>
		public void invalidateAll()
		{
			try
			{
				setWrappedContent("globalInvalidationDate", new cacheWrapperClass {saveDate = DateTime.Now});
				_globalInvalidationDate = null;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//'
		//'====================================================================================================
		//''' <summary>
		//''' invalidates a tag
		//''' </summary>
		//''' <param name="tag"></param>
		//''' <remarks></remarks>
		public void invalidateContent(string key)
		{
			try
			{
				if (!string.IsNullOrEmpty(key.Trim()))
				{
					setWrappedContent(key, new cacheWrapperClass {saveDate = DateTime.Now});
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//========================================================================
		/// <summary>
		/// update the dbTablename cache entry. Do this anytime any record is updated in the table
		/// </summary>
		/// <param name="ContentName"></param>
		public void invalidateAllObjectsInContent(string ContentName)
		{
			try
			{
				invalidateAllObjectsInTable(Models.Complex.cdefModel.getContentTablename(cpCore, ContentName));
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void invalidateAllObjectsInTable(string dbTableName)
		{
			try
			{
				invalidateContent(dbTableName.ToLower().Replace(" ", "_"));
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
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
		public void invalidateContent(List<string> keyList)
		{
			try
			{
				foreach (var key in keyList)
				{
					invalidateContent(key);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=======================================================================
		/// <summary>
		/// Encode a string to be memCacheD compatible, removing 0x00-0x20 and space
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		private string getWrapperKey(string appName, string key)
		{
			string result = appName + "-" + key;
			result = Regex.Replace(result, "0x[a-fA-F\\d]{2}", "_");
			result = result.Replace(" ", "_");
			//
			// -- 20170515 JK tmp - makes it harder to debug. add back to ensure filenames use valid characters.
			//returnKey = coreEncodingBase64Class.UTF8ToBase64(returnKey)
			return result;
		}
		//
		//=======================================================================
		private void appendCacheLog(string line)
		{
			try
			{
				logController.appendLog(cpCore, line, "cache");
			}
			catch (Exception ex)
			{
				cpCore.handleException(new ApplicationException("appendCacheLog exception", ex));
			}
		}
		//
		//====================================================================================================
		public string getString(string key)
		{
			return genericController.encodeText(getObject<string>(key));
		}
		//
		//====================================================================================================
		/// <summary>
		/// create a cache key for an entity model (based on the Db)
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="fieldName"></param>
		/// <param name="fieldValue"></param>
		/// <returns></returns>
		public static string getCacheKey_Entity(string tableName, string fieldName, string fieldValue)
		{
			return (tableName + "/" + fieldName + "/" + fieldValue).ToLower().Replace(" ", "_");
		}
		public static string getCacheKey_Entity(string tableName, int recordId)
		{
			return (tableName + "/id/" + recordId.ToString()).ToLower().Replace(" ", "_");
		}
		//
		public static string getCacheKey_Entity(string tableName, string field1Name, int field1Value, string field2Name, int field2Value)
		{
			return (tableName + "/" + field1Name + "/" + field1Value + "/" + field2Name + "/" + field2Value).ToLower().Replace(" ", "_");
		}
		//
		//====================================================================================================
		/// <summary>
		/// create a cache name for an object composed of data not from a signel record
		/// </summary>
		/// <param name="objectName"></param>
		/// <param name="uniqueObjectIdentifier"></param>
		/// <returns></returns>
		public static string getCacheKey_ComplexObject(string objectName, string uniqueObjectIdentifier)
		{
			return ("complexobject/" + objectName + "/" + uniqueObjectIdentifier).ToLower().Replace(" ", "_");
		}
		//
		public void invalidateContent_Entity(coreClass cpcore, string tableName, int recordId)
		{
			//
			invalidateContent(getCacheKey_Entity(tableName, recordId));
			if (tableName.ToLower() == linkAliasModel.contentTableName.ToLower())
			{
				//
				Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
			}
			else if (tableName.ToLower() == linkForwardModel.contentTableName.ToLower())
			{
				//
				Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
			}
			else if (tableName.ToLower() == addonModel.contentTableName.ToLower())
			{
				//
				Models.Complex.routeDictionaryModel.invalidateCache(cpcore);
			}
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
		public void Dispose()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		//
		~cacheController()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(false);
		}
		//
		//====================================================================================================
		/// <summary>
		/// dispose.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
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