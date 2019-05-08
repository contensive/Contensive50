
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Db {
    public class LinkForwardModel : DbModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link forwards";
        public const string contentTableName = "cclinkforwards";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string DestinationLink;
        public int GroupID;
        public string SourceLink;
        public int Viewings;
        // 
        //====================================================================================================
        public static LinkForwardModel addEmpty(CoreController core) {
            return addEmpty<LinkForwardModel>(core);
        }
        //
        //====================================================================================================
        public static LinkForwardModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<LinkForwardModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static LinkForwardModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<LinkForwardModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkForwardModel create(CoreController core, int recordId) {
            return create<LinkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LinkForwardModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LinkForwardModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkForwardModel create(CoreController core, string recordGuid) {
            return create<LinkForwardModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LinkForwardModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LinkForwardModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkForwardModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LinkForwardModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static LinkForwardModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LinkForwardModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LinkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LinkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LinkForwardModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LinkForwardModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LinkForwardModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LinkForwardModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LinkForwardModel> createList(CoreController core, string sqlCriteria) {
            return createList<LinkForwardModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateCacheOfRecord<LinkForwardModel>(core, recordId);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateCacheOfTable<LinkForwardModel>(core);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbModel.getRecordName<LinkForwardModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbModel.getRecordName<LinkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbModel.getRecordId<LinkForwardModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LinkForwardModel>(core);
        }
    }
}
