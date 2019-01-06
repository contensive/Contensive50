
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Contensive.BaseClasses {
    /// <summary>
    /// CP.Db - This object references the database directly
    /// </summary>
    /// <remarks></remarks>
    public abstract class CPDbModelBaseClass {
        /// <summary>
        /// constructor for deserialization
        /// </summary>
        public CPDbModelBaseClass() { }
        //-- field types
        //
        public abstract class FieldTypeFileBase {
            //
            public abstract string filename { get; set; }
            public abstract string content { get; set; }
            public abstract bool contentLoaded { get; set; }
            public abstract bool contentUpdated { get; set; }
            //public CoreController internalcore { get; set; }
        }
        //
        public abstract class FieldTypeTextFile : FieldTypeFileBase { }
        public abstract class FieldTypeJavascriptFile : FieldTypeFileBase { }
        public abstract class FieldTypeCSSFile : FieldTypeFileBase { }
        public abstract class FieldTypeHTMLFile : FieldTypeFileBase { }
        //
        //====================================================================================================
        // -- instance properties
        /// <summary>
        /// identity integer, primary key for every table
        /// </summary>
        public abstract int id { get; set; }
        /// <summary>
        /// name of the record used for lookup lists
        /// </summary>
        public abstract string name { get; set; }
        /// <summary>
        /// optional guid, created automatically in the model
        /// </summary>
        public abstract string ccguid { get; set; }
        /// <summary>
        /// optionally can be used to disable a record. Must be implemented in each query
        /// </summary>
        public abstract bool active { get; set; }
        /// <summary>
        /// id of the metadata record in ccContent that controls the display and handing for this record
        /// </summary>
        public abstract int contentControlID { get; set; }
        /// <summary>
        /// foreign key to ccmembers table, populated by admin when record added.
        /// </summary>
        public abstract int createdBy { get; set; }
        /// <summary>
        /// used when creating new record
        /// </summary>
        public abstract int createKey { get; set; }
        /// <summary>
        /// date record added, populated by admin when record added.
        /// </summary>
        public abstract DateTime dateAdded { get; set; }
        /// <summary>
        /// foreign key to ccmembers table set to user who modified the record last in the admin site
        /// </summary>
        public abstract int modifiedBy { get; set; }
        /// <summary>
        /// date when the record was last modified in the admin site
        /// </summary>
        public abstract DateTime modifiedDate { get; set; }
        /// <summary>
        /// optionally used to sort recrods in the table
        /// </summary>
        public abstract string sortOrder { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the database table associated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public abstract string DerivedTableName(Type derivedType);
        //
        //====================================================================================================
        /// <summary>
        /// return the name of the datasource assocated to the database table assocated to the derived content
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public abstract string DerivedDataSourceName(Type derivedType);
        //
        //====================================================================================================
        /// <summary>
        /// returns the boolean value of the constant nameIsUnique in the derived class. Setting true enables a name cache ptr.
        /// </summary>
        /// <param name="derivedType"></param>
        /// <returns></returns>
        public abstract bool DerivedNameFieldIsUnique(Type derivedType);
        //
        //
        // todo implement Meta Domain Model in abstract
        //
        //====================================================================================================
        /// <summary>
        /// Add a new recod to the db and open it. Starting a new model with this method will use the default values in Contensive metadata (active, contentcontrolid, etc).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        //public abstract T AddDefault<T>(CPBaseClass cp, Domain.MetaModel metaData) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// Add a new record to the db populated with default values from the content definition and return an object of it
        /// </summary>
        /// <param name="core"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        //public abstract T AddDefault<T>(CPBaseClass cp, Domain.MetaModel metaData, ref List<string> callersCacheNameList) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// Add a new empty record to the db and return an object of it.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="callersCacheNameList"></param>
        /// <returns></returns>
        public abstract T AddEmpty<T>(CPBaseClass cp) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected.
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        /// <returns></returns>
        public abstract T Create<T>(CPBaseClass cp, int recordId) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId">The id of the record to be read into the new object</param>
        /// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
        public abstract T Create<T>(CPBaseClass cp, int recordId, ref List<string> callersCacheNameList) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with matching ccGuid
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="recordGuid"></param>
        /// <returns></returns>
        public abstract T create<T>(CPBaseClass cp, string recordGuid) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create an object from a record with a matching ccguid, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordGuid"></param>
        public abstract T Create<T>(CPBaseClass cp, string recordGuid, ref List<string> callersCacheNameList) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
        public abstract T CreateByUniqueName<T>(CPBaseClass cp, string recordName) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create an object from the first record found from a list created with matching name records, ordered by id, add an object cache name to the argument list
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordName"></param>
        /// <param name="callersCacheNameList">method will add the cache name to this list.</param>
        public abstract T CreateByUniqueName<T>(CPBaseClass cp, string recordName, ref List<string> callersCacheNameList) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// save the instance properties to a record with matching id. If id is not provided, a new record is created.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public abstract int Save(CPBaseClass cp, bool asyncSave = false);
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record by id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>
        public abstract void Delete<T>(CPBaseClass cp, int recordId) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// delete an existing database record by guid
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>
        public abstract void Delete<T>(CPBaseClass cp, string guid) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria and sort order, and add a cache object to an argument
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        public abstract List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) where T : CPDbModelBaseClass;
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
        public abstract List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria, string sqlOrderBy) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create a list of objects based on the sql criteria
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="sqlCriteria"></param>
        /// <returns></returns>
        public abstract List<T> CreateList<T>(CPBaseClass cp, string sqlCriteria) where T : CPDbModelBaseClass;
        //
        public abstract List<T> CreateList<T>(CPBaseClass cp) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// invalidate the cache entry for a record
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        public abstract void InvalidateRecordCache<T>(CPBaseClass cp, int recordId) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// invalidate all cache entries for the table
        /// </summary>
        /// <param name="core"></param>
        /// <param name="recordId"></param>
        public abstract void InvalidateTableCache<T>(CPBaseClass cp) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's id
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="recordId"></param>record
        /// <returns></returns>
        public abstract string GetRecordName<T>(CPBaseClass cp, int recordId) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// get the name of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        public abstract string GetRecordName<T>(CPBaseClass cp, string guid) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// get the id of the record by it's guid 
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="guid"></param>record
        /// <returns></returns>
        public abstract int GetRecordId<T>(CPBaseClass cp, string guid) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// Create an empty model object, populating only control fields (guid, active, created/modified)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public abstract T CreateEmpty<T>(CPBaseClass cp) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create an sql select for this model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="fieldList"></param>
        /// <param name="sqlCriteria"></param>
        /// <param name="sqlOrderBy"></param>
        /// <returns></returns>
        public abstract string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList, string sqlCriteria, string sqlOrderBy) where T : CPDbModelBaseClass;
        //
        public abstract string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList, string criteria) where T : CPDbModelBaseClass;
        //
        public abstract string GetSelectSql<T>(CPBaseClass cp, List<string> fieldList) where T : CPDbModelBaseClass;
        //
        public abstract string GetSelectSql<T>(CPBaseClass cp) where T : CPDbModelBaseClass;
        //
        //====================================================================================================
        /// <summary>
        /// create the cache key for the table cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <returns></returns>
        public abstract string GetTableCacheKey<T>(CPBaseClass cp);
        //
        //====================================================================================================
        /// <summary>
        /// Delete a selection of records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="core"></param>
        /// <param name="sqlCriteria"></param>
        public abstract void DeleteSelection<T>(CPBaseClass cp, string sqlCriteria) where T : CPDbModelBaseClass;
    }

}

