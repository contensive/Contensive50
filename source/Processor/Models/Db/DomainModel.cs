﻿
using System.Collections.Generic;
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Db {
    public class DomainModel : BaseModel {
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
        public static DomainModel add(CoreController core) {
            return add<DomainModel>(core);
        }
        //
        //====================================================================================================
        public static DomainModel add(CoreController core, ref List<string> callersCacheNameList) {
            return add<DomainModel>(core, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, int recordId) {
            return create<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, int recordId, ref List<string> callersCacheNameList) {
            return create<DomainModel>(core, recordId, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, string recordGuid) {
            return create<DomainModel>(core, recordGuid);
        }
        //
        //====================================================================================================
        public static DomainModel create(CoreController core, string recordGuid, ref List<string> callersCacheNameList) {
            return create<DomainModel>(core, recordGuid, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public static DomainModel createByName(CoreController core, string recordName) {
            return createByName<DomainModel>(core, recordName);
        }
        //
        //====================================================================================================
        public static DomainModel createByName(CoreController core, string recordName, ref List<string> callersCacheNameList) {
            return createByName<DomainModel>(core, recordName, ref callersCacheNameList);
        }
        //
        //====================================================================================================
        public new void save(CoreController core) {
            base.save(core);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, int recordId) {
            delete<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static void delete(CoreController core, string ccGuid) {
            delete<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy, List<string> callersCacheNameList) {
            return createList<DomainModel>(core, sqlCriteria, sqlOrderBy, callersCacheNameList);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria, string sqlOrderBy) {
            return createList<DomainModel>(core, sqlCriteria, sqlOrderBy);
        }
        //
        //====================================================================================================
        public static List<DomainModel> createList(CoreController core, string sqlCriteria) {
            return createList<DomainModel>(core, sqlCriteria);
        }
        //
        //====================================================================================================
        public void invalidatePrimaryCache(CoreController core, int recordId) {
            invalidateCache<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, int recordId) {
            return BaseModel.getRecordName<DomainModel>(core, recordId);
        }
        //
        //====================================================================================================
        public static string getRecordName(CoreController core, string ccGuid) {
            return BaseModel.getRecordName<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static int getRecordId(CoreController core, string ccGuid) {
            return BaseModel.getRecordId<DomainModel>(core, ccGuid);
        }
        //
        //====================================================================================================
        public static DomainModel createDefault(CoreController core) {
            return createDefault<DomainModel>(core);
        }
        //
        //====================================================================================================
        public static Dictionary<string,DomainModel> createDictionary(CoreController core, string sqlCriteria) {
            var result = new Dictionary<string, DomainModel> { };
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