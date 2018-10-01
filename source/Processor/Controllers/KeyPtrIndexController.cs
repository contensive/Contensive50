
//using System;
//using System.Reflection;
//using System.Xml;
//using System.Diagnostics;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using Contensive.Processor;
//using Contensive.Processor.Models.Db;
//using Contensive.Processor.Controllers;
//using static Contensive.Processor.Controllers.GenericController;
//using static Contensive.Processor.constants;
//using System.Data;
////

//namespace Contensive.Processor.Controllers {
//    //
//    //====================================================================================================
//    /// <summary>
//    /// Creates a simple keyValue cache system
//    /// not IDisposable - not contained classes that need to be disposed
//    /// constructor includes query to load with two fields, key (string) and value (string)
//    /// values are stored in a List, referenced by a pointer
//    /// keys are stored in an keyPtrINdex with the pointer to the value
//    /// loads with cache. if cache is empty, it loads on demand
//    /// when cleared, loads on demand
//    ///  combines a key ptr index and a value store, referenced by ptr
//    /// </summary>
//    public class KeyPtrIndexController {
//        //
//        // ----- objects passed in constructor, do not dispose
//        //
//        private CoreController core;
//        //
//        // ----- private globals
//        //
//        private string sqlLoadKeyValue;
//        private string cacheName;
//        private string cacheInvalidationTagCommaList;
//        //
//        //====================================================================================================
//        //
//        public class dataStoreClass {
//            public List<string> dataList;
//            public KeyPtrController keyPtrIndex;
//            public bool loaded;
//        }
//        private dataStoreClass dataStore = new dataStoreClass();
//        //
//        //====================================================================================================
//        //
//        public KeyPtrIndexController(CoreController core, string cacheName, string sqlLoadKeyValue, string cacheInvalidationTagCommaList) : base() {
//            this.core = core;
//            this.cacheName = cacheName;
//            this.sqlLoadKeyValue = sqlLoadKeyValue;
//            this.cacheInvalidationTagCommaList = cacheInvalidationTagCommaList;
//            dataStore = new dataStoreClass();
//            dataStore.dataList = new List<string>();
//            dataStore.keyPtrIndex = new KeyPtrController();
//            dataStore.loaded = false;
//        }
//        //
//        //====================================================================================================
//        //
//        //   clear sharedStylesAddonRules cache
//        //
//        public void clear() {
//            try {
//                dataStore.loaded = false;
//                dataStore.dataList.Clear();
//                dataStore.keyPtrIndex = new KeyPtrController();
//                core.cache.setObject(cacheName + "-dataList", dataStore.dataList);
//            } catch (Exception ex) {
//                LogController.handleError( core,ex);
//                throw;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// get an integer pointer to the array that contains the keys value. Returns -1 if not found
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        public int getFirstPtr(string key) {
//            int returnPtr = -1;
//            try {
//                if (string.IsNullOrEmpty(key)) {
//                    throw new ArgumentException("blank key is not valid.");
//                } else {
//                    if (!dataStore.loaded) {
//                        load();
//                    }
//                    if (!dataStore.loaded) {
//                        throw new ApplicationException("datastore could not be loaded");
//                    } else {
//                        returnPtr = dataStore.keyPtrIndex.getPtr(key);
//                    }
//                }
//            } catch (Exception ex) {
//                LogController.handleError( core,ex, "key[" + key + "]");
//            }
//            return returnPtr;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Returns the next pointer in the array, sorted by value. Returns -1 if there are no more values
//        /// </summary>
//        /// <returns></returns>
//        public int getNextPtr() {
//            int returnPtr = -1;
//            try {
//                if (!dataStore.loaded) {
//                    load();
//                }
//                if (!dataStore.loaded) {
//                    throw new ApplicationException("datastore could not be loaded");
//                } else {
//                    returnPtr = dataStore.keyPtrIndex.getNextPtr();
//                }
//            } catch (Exception ex) {
//                LogController.handleError( core,ex);
//                throw;
//            }
//            return returnPtr;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// return an integer pointer to the value for this key. -1 if not found
//        /// </summary>
//        /// <param name="key"></param>
//        /// <returns></returns>
//        public int getPtr(string key) {
//            int returnPtr = -1;
//            try {
//                if (string.IsNullOrEmpty(key)) {
//                    throw new ArgumentException("blank key is not valid.");
//                } else {
//                    if (!dataStore.loaded) {
//                        load();
//                    }
//                    if (!dataStore.loaded) {
//                        throw new ApplicationException("datastore could not be loaded");
//                    } else {
//                        returnPtr = dataStore.keyPtrIndex.getPtr(key);
//                    }
//                }
//            } catch (Exception ex) {
//                LogController.handleError( core,ex, "key[" + key + "]");
//            }
//            return returnPtr;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Returns the value stored with this pointer.
//        /// </summary>
//        /// <param name="ptr"></param>
//        /// <returns></returns>
//        public string getValue(int ptr) {
//            string returnValue = "";
//            try {
//                if (ptr < 0) {
//                    throw new ArgumentException("ptr must be >= 0");
//                } else {
//                    if (!dataStore.loaded) {
//                        load();
//                    }
//                    if (!dataStore.loaded) {
//                        throw new ApplicationException("datastore could not be loaded");
//                    } else {
//                        returnValue = dataStore.dataList[ptr];
//                    }
//                }
//            } catch (Exception ex) {
//                LogController.handleError( core,ex, "ptr[" + ptr.ToString() + "]");
//            }
//            return returnValue;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// load the values based on the sql statement loaded during constructor sqlLoadKeyVAlue.
//        /// If already loaded, does nothing
//        /// Attempts to read the cache from the cache, invalidated by the contentname
//        /// </summary>
//        private void load() {
//            try {
//                //
//                int Ptr = 0;
//                string RecordIdTextValue = null;
//                bool needsToReload = false;
//                //
//                if (!dataStore.loaded) {
//                    try {
//                        needsToReload = true;
//                        dataStore = (dataStoreClass)core.cache.getObject<dataStoreClass>(cacheName);
//                    } catch (Exception) {
//                        needsToReload = true;
//                    }
//                    if (dataStore == null) {
//                        needsToReload = true;
//                    } else if (dataStore.dataList == null) {
//                        needsToReload = true;
//                    }
//                    if (needsToReload) {
//                        //
//                        // cache is empty, build it from scratch
//                        //
//                        dataStore = new dataStoreClass();
//                        dataStore.dataList = new List<string>();
//                        dataStore.keyPtrIndex = new KeyPtrController();
//                        using (DataTable dt = core.db.executeQuery(sqlLoadKeyValue)) {
//                            Ptr = 0;
//                            foreach (DataRow dr in dt.Rows) {
//                                dataStore.keyPtrIndex.setPtr(dr[0].ToString(), Ptr);
//                                dataStore.dataList.Add(dr[1].ToString());
//                                Ptr += 1;
//                            }
//                        }
//                        //
//                        if (dataStore.dataList.Count > 0) {
//                            dataStore.keyPtrIndex = new KeyPtrController();
//                            for (Ptr = 0; Ptr < dataStore.dataList.Count; Ptr++) {
//                                RecordIdTextValue = dataStore.dataList[Ptr];
//                                dataStore.keyPtrIndex.setPtr(RecordIdTextValue, Ptr);
//                            }
//                            updateCache();
//                        }
//                        needsToReload = false;
//                    }
//                    dataStore.loaded = true;
//                }
//            } catch (Exception ex) {
//                throw new ApplicationException("Exception in cacheKeyPtrClass.load", ex);
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// 
//        /// </summary>
//        public void updateCache() {
//            try {
//                if (dataStore.loaded) {
//                    dataStore.keyPtrIndex.getPtr("test");
//                    core.cache.setObject(cacheName, dataStore, cacheInvalidationTagCommaList);
//                }
//            } catch (Exception ex) {
//                throw new ApplicationException("Exception in cacheKeyPtrClass.save", ex);
//            }
//        }
//    }

//}