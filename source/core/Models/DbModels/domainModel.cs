
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
        /// <summary>
        /// the template used for this domain. Can be overridden by page
        /// </summary>
        public int defaultTemplateId { get; set; }
        /// <summary>
        /// forward traffic to this domain to another domain
        /// </summary>
        public int forwardDomainId { get; set; }
        /// <summary>
        /// forward traffic to this url
        /// </summary>
        public string forwardUrl { get; set; }
        /// <summary>
        /// set response header to noFollow for this domain
        /// </summary>
        public bool noFollow { get; set; }
        /// <summary>
        /// for this domain, display this page not found
        /// </summary>
        public int pageNotFoundPageId { get; set; }
        /// <summary>
        /// for this domain, the home/landing page
        /// </summary>
        public int rootPageId { get; set; }
        /// <summary>
        /// determines the type of response
        /// </summary>
        public int typeId { get; set; }
        /// <summary>
        /// true if this domain has received traffic
        /// </summary>
        public bool visited { get; set; }
        /// <summary>
        /// the default code to execute for this domain
        /// </summary>
        public int defaultRouteId { get; set; }
        /// <summary>
        /// if true, add the default CORS headers
        /// </summary>
        public bool allowCORS { get; set; }
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
