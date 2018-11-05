
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	/// <summary>
	/// CP.Cache - contains features to perform simple caching functions
	/// </summary>
	/// <remarks></remarks>
	public abstract class CPCacheBaseClass
	{
        /// <summary>
        /// Clear one or more cache objects by key
        /// </summary>
        /// <param name="keyList"></param>
        public abstract void Clear(List<string> keyList);
        /// <summary>
        /// Read the value of a cache. If empty or invalid, returns null.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks></remarks>
		public abstract object GetObject(string key);
        /// <summary>
        /// get a string from cache. If empty or invalid, returns empty string.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract string GetText(string key);
        /// <summary>
        /// get an integer from cache. If empty or invalid, returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public abstract int GetInteger(string key);
        /// <summary>
        /// get a double from cache. If empty or invalid, returns 0.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public abstract double GetNumber(string key);
        /// <summary>
        /// get an date from cache. If empty or invalid, returns Date.MinValue.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public abstract DateTime GetDate(string key);
        /// <summary>
        /// get an integer from cache. If empty or invalid, returns false.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		public abstract bool GetBoolean(string key);
		/// <summary>
		/// Clear all system caches. Use this call to flush all internal caches.
		/// </summary>
		/// <remarks></remarks>
		public abstract void ClearAll();
        //
        public abstract void Invalidate(string key);
		public abstract void InvalidateAll();
		public abstract void InvalidateTagList(List<string> tagList);
		public abstract void InvalidateContentRecord(string contentName, int recordId);
        /// <summary>
        /// get cache key for a table. A table key is used as a dependent key to invalidate all record cache objects from a table.
        /// When storing a record object, add the tablekey as a dependent key
        /// If a db operation modifies records and you cant invalidate the individual records, invalidate the table with this key
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public abstract string CreateKeyForDbTable(string tableName, string dataSourceName = "default");
        /// <summary>
        /// get a cache key for a database model object
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public abstract string CreateKeyForDbRecord(int recordId, string tableName, string dataSourceName = "default");
        /// <summary>
        /// get a cache key for a domain model
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="objectUniqueIdentifier"></param>
        /// <returns></returns>
        public abstract string CreateKey(string objectName, string objectUniqueIdentifier = "");
        /// <summary>
        /// return a Ptr Key for a db model object. 
        /// A Ptr key doesnt contain the object, but points to a key that does create the object.
        /// When you get a cache object from a Ptr Key, the object it points to is returned.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public abstract string CreatePtrKeyforDbRecordGuid(string guid, string tableName, string dataSourceName = "default");
        /// <summary>
        /// return a Ptr key for a db model object based on the record name. Only for tables where the name is unique
        /// </summary>
        /// <param name="name"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        /// <returns></returns>
        public abstract string CreatePtrKeyforDbRecordUniqueName(string name, string tableName, string dataSourceName = "default");
        /// <summary>
        /// Store a Db Model with a guid ptr
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        public abstract void StoreDbModel<T>(string guid, int recordId);
        /// <summary>
        /// Store a Db Model without a guid pointer
        /// </summary>
        /// <param name="recordId"></param>
        /// <param name="tableName"></param>
        /// <param name="dataSourceName"></param>
        public abstract void StoreDbModel<T>(int recordId);
        /// <summary>
        /// return a model of type T from its id. If not valid, returns Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public abstract T GetDbModel<T>(int recordId);
        /// <summary>
        /// return a model of type T from its guid. If not valid, returns Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="guid"></param>
        /// <returns></returns>
        public abstract T GetDbModel<T>(string guid);
        /// <summary>
        /// Get an object of type T from cache. If empty or invalid type, returns Null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public abstract T GetObject<T>(string key);
        /// <summary>
        /// Store an object to a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public abstract void StoreObject(string key, object value);
        /// <summary>
        /// Store an object to a key. Invalidate at the date and time specified.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidationDate"></param>
        public abstract void StoreObject(string key, object value, DateTime invalidationDate);
        /// <summary>
        /// Store an object to a key.
        ///  Invalidate the object if a dependentKey is updated after this object is stored.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="dependentKeyList"></param>
        public abstract void StoreObject(string key, object value, List<string> dependentKeyList);
        /// <summary>
        /// Store an object to a key. Invalidate at the date and time specified.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidationDate"></param>
        /// <param name="dependentKeyList"></param>
        public abstract void StoreObject(string key, object value, DateTime invalidationDate, List<string> dependentKeyList);
        /// <summary>
        /// Store an object to a key. 
        /// Invalidate the object if a dependentKey is updated after this object is stored.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tag"></param>
        public abstract void StoreObject(string key, object value, string dependentKey);
        /// <summary>
        /// Store an object to a key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidationDate"></param>
        /// <param name="dependentKey"></param>
        public abstract void StoreObject(string key, object value, DateTime invalidationDate, string dependentKey);
        //
        //public abstract void Save(string key, string Value);
        //public abstract void Save(string key, string Value, string tagCommaList);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value, DateTime invalidationDate);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value, List<string> tagList);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value, DateTime invalidationDate, List<string> tagList);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value, string tag);
        //
        [Obsolete("use StoreObject() instead", true)]
        public abstract void setKey(string key, object Value, DateTime invalidationDate, string tag);
        //
        [Obsolete("Use Clear(dependentKeyList)", true)]
        public abstract void Clear(string ContentNameList);
        //
        [Obsolete("Use GetText(key) instead",false)]
        public abstract string Read(string key);
        //
        [Obsolete("Use Invalidate(key)", true)]
        public abstract void InvalidateTag(string tag);
        //
        [Obsolete("Use StoreObject()",true)]
        public abstract void Save(string key, string Value);
        //
        [Obsolete("Use StoreObject()", true)]
        public abstract void Save(string key, string Value, string tagCommaList);
        //
        [Obsolete("Use StoreObject()", true)]
        public abstract void Save(string key, string Value, string tagCommaList, DateTime ClearOnDate);
    }

}

