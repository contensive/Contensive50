
using System;
using System.Linq;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;

namespace Contensive.Processor {
    public class CPCacheClass : BaseClasses.CPCacheBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "D522F0F5-53DF-4C6C-88E5-75CDAB91D286";
        public const string InterfaceId = "9FED1031-1637-4002-9B08-4A40FDF13236";
        public const string EventsId = "11B23802-CBD3-48E6-9C3E-1DC26ED8775A";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core { get; set; }
        private CPClass cp { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cpParent"></param>
        /// <remarks></remarks>
        public CPCacheClass(CPClass cpParent) : base() {
            cp = cpParent;
            core = cp.core;
        }
        //
        //====================================================================================================
        //
        public override void Clear(List<string> keyList) => core.cache.invalidate(keyList);
        //
        //====================================================================================================
        //
        public override object GetObject(string key) => core.cache.getObject<object>(key);
        //
        //====================================================================================================
        //
        public override string GetText(string key) => core.cache.getObject<string>(key);
        //
        //====================================================================================================
        //
        public override int GetInteger(string key) => core.cache.getObject<int>(key);
        //
        //====================================================================================================
        //
        public override double GetNumber(string key) => core.cache.getObject<double>(key);
        //
        //====================================================================================================
        //
        public override DateTime GetDate(string key) => core.cache.getObject<DateTime>(key);
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string key) => core.cache.getObject<bool>(key);
        //
        //====================================================================================================
        //
        public override void Invalidate(string key) => core.cache.invalidate(key);
        //
        //====================================================================================================
        //
        public override string CreateKeyForDbTable(string tableName) => CacheController.createCacheKey_forDbTable(tableName);
        //
        public override string CreateKeyForDbTable(string tableName, string dataSourceName) => CacheController.createCacheKey_forDbTable(tableName, dataSourceName);
        //
        //====================================================================================================
        //
        public override string CreateKeyForDbRecord(int recordId, string tableName, string dataSourceName) => CacheController.createCacheKey_forDbRecord(recordId, tableName, dataSourceName);

        public override string CreateKeyForDbRecord(int recordId, string tableName) => CacheController.createCacheKey_forDbRecord(recordId, tableName);
        //
        //====================================================================================================
        //
        public override string CreateKey(string objectName) => CacheController.createCacheKey_forObject(objectName);

        //
        public override string CreateKey(string objectName, string objectUniqueIdentifier = "") => CacheController.createCacheKey_forObject(objectName, objectUniqueIdentifier);
        //
        //====================================================================================================
        //
        public override string CreatePtrKeyforDbRecordGuid(string guid, string tableName, string dataSourceName) => CacheController.createCachePtr_forDbRecord_guid(guid, tableName, dataSourceName);
        //
        public override string CreatePtrKeyforDbRecordGuid(string guid, string tableName) => CacheController.createCachePtr_forDbRecord_guid(guid, tableName);
        //
        //====================================================================================================
        //
        public override string CreatePtrKeyforDbRecordUniqueName(string name, string tableName, string dataSourceName) => CacheController.createCachePtr_forDbRecord_uniqueName(name, tableName, dataSourceName);

        public override string CreatePtrKeyforDbRecordUniqueName(string name, string tableName) => CacheController.createCachePtr_forDbRecord_uniqueName(name, tableName);
        //
        //====================================================================================================
        //
        public override T GetObject<T>(string key) => core.cache.getObject<T>(key);
        //
        //====================================================================================================
        //
        public override void Store(string key, object value) => core.cache.storeObject(key, value);
        //
        //====================================================================================================
        //
        public override void Store(string key, object value, DateTime invalidationDate) => core.cache.storeObject(key, value, invalidationDate);
        //
        //====================================================================================================
        //
        public override void Store(string key, object value, List<string> dependentKeyList) => core.cache.storeObject(key, value, dependentKeyList);
        //
        //====================================================================================================
        //
        public override void Store(string key, object value, DateTime invalidationDate, List<string> dependentKeyList) => core.cache.storeObject(key, value, invalidationDate, dependentKeyList);
        //
        //====================================================================================================
        //
        public override void Store(string key, object value, string dependentKey) => core.cache.storeObject(key, value, new List<string>() { dependentKey });
        //
        //====================================================================================================
        //
        public override void Store(string key, object value, DateTime invalidationDate, string dependentKey) => core.cache.storeObject(key, value, invalidationDate, new List<string>() { dependentKey } );
        //
        //====================================================================================================
        /// <summary>
        /// Clear all cache values
        /// </summary>
        /// <remarks></remarks>
        public override void ClearAll() => core.cache.invalidateAll();
        //
        //====================================================================================================
        /// <summary>
        /// clear all cache entries related to all tables used by a comma delimited list of content.
        /// </summary>
        /// <param name="ContentNameList"></param>
        /// <remarks></remarks>
        ///
        [Obsolete("Use Clear(dependentKeyList)", true)]
        public override void Clear(string ContentNameList) {
            if (!string.IsNullOrEmpty(ContentNameList)) {
                List<string> tableNameList = new List<string>();
                foreach (var contentName in new List<string>(ContentNameList.ToLowerInvariant().Split(','))) {
                    string tableName = CdefController.getContentTablename(core, contentName).ToLowerInvariant();
                    if (!tableNameList.Contains(tableName)) {
                        tableNameList.Add(tableName);
                        core.cache.invalidateAllKeysInTable(tableName);
                    }
                }
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// read a cache value
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Use GetText()", true)]
        public override string Read(string Name) => core.cache.getObject<string>(Name);
        //====================================================================================================
        /// <summary>
        /// save a cache value. Legacy. Use object value.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="Value"></param>
        /// <param name="invalidationTagCommaList"></param>
        /// <param name="ClearOnDate"></param>
        /// <remarks></remarks>
        /// 
        [Obsolete("Use StoreObject()", true)]
        public override void Save(string key, string Value) => Store(key, Value);
        //
        [Obsolete("Use StoreObject()", true)]
        public override void Save(string key, string Value, string invalidationTagCommaList) => Save(key, Value, invalidationTagCommaList, DateTime.MinValue);
        //
        [Obsolete("Use StoreObject()", true)]
        public override void Save(string key, string Value, string invalidationTagCommaList, DateTime invalidationDate) {
            try {
                List<string> invalidationTagList = new List<string>();
                if (!string.IsNullOrEmpty(invalidationTagCommaList.Trim())) {
                    invalidationTagList.AddRange(invalidationTagCommaList.Split(','));
                }
                if (invalidationDate.isOld()) {
                    core.cache.storeObject(key, Value, invalidationTagList);
                } else {
                    core.cache.storeObject(key, Value, invalidationDate, invalidationTagList);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void InvalidateAll() {
            core.cache.invalidateAll();
        }
        //
        //====================================================================================================
        //
        public override void InvalidateTagList(List<string> tagList) {
            core.cache.invalidate(tagList);
        }
        //
        //====================================================================================================
        //
        public override void InvalidateTag(string tag) {
            core.cache.invalidate(tag);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save a value to a cache key. It will invalidate after the default invalidation days
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public override void setKey(string key, object value) {
            core.cache.storeObject(key, value);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save a value to a cache key and specify when it will be invalidated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidationDate"></param>
        public override void setKey(string key, object value, DateTime invalidationDate) {
            core.cache.storeObject(key, value, invalidationDate, new List<string> { });
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save a value to a cachekey and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tagList"></param>
        public override void setKey(string key, object value, List<string> tagList) {
            core.cache.storeObject(key, value, tagList);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Save a value to a cachekey with an invalidate date, and associate it to one of more tags. This key will be invalidated if any of the tags are invalidated.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="tagList"></param>
        /// <param name="invalidationDate"></param>
        public override void setKey(string key, object value, DateTime invalidationDate, List<string> tagList) {
            core.cache.storeObject(key, value, invalidationDate, tagList);
        }
        //
        //====================================================================================================
        //
        public override void setKey(string key, object value, string tag) {
            core.cache.storeObject(key, value, tag);
        }
        //
        //====================================================================================================
        //
        public override void setKey(string key, object Value, DateTime invalidationDate, string tag) {
            List<string> depKeyList = (string.IsNullOrWhiteSpace(tag) ? new List<string> { } : tag.Split(',').ToList());
            core.cache.storeObject(key, Value, invalidationDate, depKeyList);
        }
        //
        public override void InvalidateContentRecord(string contentName, int recordId) {
            core.cache.invalidateDbRecord(recordId, CdefController.getContentTablename( core,  contentName));
        }
        #region  IDisposable Support 
        //
        //====================================================================================================
        // dispose
        //
        protected bool disposed = false;
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cp = null;
                    core = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~CPCacheClass() {
            Dispose(false);
            
            
        }
        #endregion
    }
}