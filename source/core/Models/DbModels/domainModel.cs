
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
        public static domainModel add(coreController core) {
            return add<domainModel>(core);
        }
        //
        //====================================================================================================
        public static domainModel add(coreController core, ref List<string> callersCacheNameList) {
            return add<domainModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController core, int recordId) {
            return create<domainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<domainModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController core, string recordGuid) {
            return create<domainModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static domainModel create(coreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<domainModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static domainModel createByName(coreController core, string recordName) {
            return createByName<domainModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static domainModel createByName(coreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<domainModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(coreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, int recordId) {
            delete<domainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(coreController core, string ccGuid) {
            delete<domainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<domainModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<domainModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<domainModel> createList(coreController core, string sqlCriteria) {
            return createList<domainModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(coreController core, int recordId) {
            invalidateCacheSingleRecord<domainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, int recordId) {
            return baseModel.getRecordName<domainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(coreController core, string ccGuid) {
            return baseModel.getRecordName<domainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(coreController core, string ccGuid) {
            return baseModel.getRecordId<domainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static domainModel createDefault(coreController core) {
            return createDefault<domainModel>(core);
        }
        //
        //====================================================================================================
        public static Dictionary<string,domainModel> createDictionary(coreController core, string sqlCriteria) {
            var result = new Dictionary<string, domainModel> { };
            foreach (var domain in createList(core, sqlCriteria)) {
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
