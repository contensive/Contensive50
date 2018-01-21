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
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.DbModels {
    public class domainModel : baseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "domains";
        public const string contentTableName = "ccdomains";
        private const string contentDataSource = "default";
        //
        //====================================================================================================
        // -- instance properties
        public int defaultTemplateId { get; set; }
        public int forwardDomainId { get; set; }
        public string forwardUrl { get; set; }
        public bool noFollow { get; set; }
        public int pageNotFoundPageId { get; set; }
        public int rootPageId { get; set; }
        public int typeId { get; set; }
        public bool visited { get; set; }
        public int defaultRouteId { get; set; }
        //
        //====================================================================================================
        public static domainModel add(coreController cpCore) {
            return add<domainModel>(cpCore);
        }
        //
        //====================================================================================================
        public static domainModel add(coreController cpCore, ref List<string> callersCacheNameList) {
            return add<domainModel>(cpCore, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController cpCore, int recordId) {
            return create<domainModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController cpCore, int recordId, ref List<string> callersCacheNameList) {
            return create<domainModel>(cpCore, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController cpCore, string recordGuid) {
            return create<domainModel>(cpCore, recordGuid);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController cpCore, string recordGuid, ref List<string> callersCacheNameList) {
            return create<domainModel>(cpCore, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel createByName(coreController cpCore, string recordName) {
            return createByName<domainModel>(cpCore, recordName);
        }
        //
        //====================================================================================================
        public static domainModel createByName(coreController cpCore, string recordName, ref List<string> callersCacheNameList) {
            return createByName<domainModel>(cpCore, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController cpCore) {
            base.save(cpCore);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, int recordId) {
            delete<domainModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController cpCore, string ccGuid) {
            delete<domainModel>(cpCore, ccGuid);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<domainModel>(cpCore, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController cpCore, string sqlCriteria, string sqlOrderBy) {
            return createList<domainModel>(cpCore, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController cpCore, string sqlCriteria) {
            return createList<domainModel>(cpCore, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController cpCore, int recordId) {
            invalidateCacheSingleRecord<domainModel>(cpCore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, int recordId) {
            return baseModel.getRecordName<domainModel>(cpcore, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController cpcore, string ccGuid) {
            return baseModel.getRecordName<domainModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController cpcore, string ccGuid) {
            return baseModel.getRecordId<domainModel>(cpcore, ccGuid);
        }
        //
        //====================================================================================================
        public static domainModel createDefault(coreController cpcore) {
            return createDefault<domainModel>(cpcore);
        }
        //
        //====================================================================================================
        public static Dictionary<string,domainModel> createDictionary(coreController cpcore, string sqlCriteria) {
            var result = new Dictionary<string, domainModel> { };
            foreach (var domain in createList(cpcore, sqlCriteria)) {
                if (!result.ContainsKey(domain.name.ToLower())) {
                    result.Add(domain.name.ToLower(), domain);
                }
            }
            return result;
        }
        //
        public enum domainTypeEnum {
            Normal = 1,
            ForwardToUrl = 2,
            ForwardToReplacementDomain = 3
        }
    }
}
