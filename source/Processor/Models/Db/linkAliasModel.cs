﻿
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
    [System.Serializable]
    public class LinkAliasModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link aliases";
        public const string contentTableNameLowerCase = "cclinkaliases";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public int pageID { get; set; }
        public string queryStringSuffix { get; set; }
        // 
        //====================================================================================================
        public static LinkAliasModel addEmpty(CoreController core) {
            return addEmpty<LinkAliasModel>(core);
        }
        //
        //====================================================================================================
        public static LinkAliasModel addDefault(CoreController core, Domain.ContentMetadataModel metaData) {
            return addDefault<LinkAliasModel>(core, metaData);
        }
        //
        //====================================================================================================
        public static LinkAliasModel addDefault(CoreController core, ref List<string> callersCacheNameList, Domain.ContentMetadataModel metaData) {
            return addDefault<LinkAliasModel>(core, metaData, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkAliasModel create(CoreController core, int recordId) {
            return create<LinkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static LinkAliasModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<LinkAliasModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkAliasModel create(CoreController core, string recordGuid) {
            return create<LinkAliasModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static LinkAliasModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<LinkAliasModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static LinkAliasModel createByUniqueName(CoreController core, string recordName) {
            return createByUniqueName<LinkAliasModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static LinkAliasModel createByUniqueName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByUniqueName<LinkAliasModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core, bool asyncSave = false) { base.save(core, asyncSave); }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<LinkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<LinkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LinkAliasModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<LinkAliasModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<LinkAliasModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<LinkAliasModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<LinkAliasModel> createList(CoreController core, string sqlCriteria) {
            return createList<LinkAliasModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public static void invalidateRecordCache(CoreController core, int recordId) {
            invalidateCacheOfRecord<LinkAliasModel>(core, recordId);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static void invalidateTableCache(CoreController core) {
            invalidateCacheOfTable<LinkAliasModel>(core);
            Domain.RouteMapModel.invalidateCache(core);
            core.routeMapCacheClear();
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return DbBaseModel.getRecordName<LinkAliasModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordName<LinkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return DbBaseModel.getRecordId<LinkAliasModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<LinkAliasModel> createList(CoreController core, int pageId, string queryStringSuffix) {
            if ( string.IsNullOrEmpty(queryStringSuffix)) {
                return createList<LinkAliasModel>(core, "(pageId=" + pageId + ")and((QueryStringSuffix='')or(QueryStringSuffix is null))", "id desc");
            } else {
                return createList<LinkAliasModel>(core, "(pageId=" + pageId + ")and(QueryStringSuffix=" + DbController.encodeSQLText(queryStringSuffix) + ")", "id desc");
            }
        }
        //
        //====================================================================================================
        public static List<LinkAliasModel> createList(CoreController core, int pageId) {
            return createList<LinkAliasModel>(core, "(pageId=" + pageId + ")and((QueryStringSuffix='')or(QueryStringSuffix is null)", "id desc");
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return a cache key used to represent the table. ONLY used for invalidation. Add this as a dependent key if you want that key cleared when ANY record in the table is changed.
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static string getTableInvalidationKey(CoreController core) {
            return getTableCacheKey<LinkAliasModel>(core);
        }
    }
}
