
using Contensive.BaseClasses;
using System;
using System.Linq;

namespace Contensive.Models.Db {
    /// <summary>
    /// Auditing table to track each visit (=session)
    /// </summary>
    public class VisitModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("visits", "ccvisits", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// if true, this visit is from a bot
        /// </summary>
        public bool bot { get; set; }
        /// <summary>
        /// the browser string prsented by the client browser during the visit
        /// </summary>
        public string browser { get; set; }        
        /// <summary>
        /// true if cookie support verified (second hit)
        /// </summary>
        public bool cookieSupport { get; set; }
        /// <summary>
        /// flag can be used to exclude this visit from analytics
        /// </summary>
        public bool excludeFromAnalytics { get; set; }
        /// <summary>
        /// the refering page on the first hit from the visit
        /// </summary>
        public string http_referer { get; set; }
        /// <summary>
        /// The datetime of the last hit during the visit
        /// </summary>
        public DateTime? lastVisitTime { get; set; }
        /// <summary>
        /// count of login attempts during this visit
        /// </summary>
        public int loginAttempts { get; set; }
        /// <summary>
        /// The people record associated with the visit. Can change during authentication
        /// </summary>
        public int memberId { get; set; }
        /// <summary>
        /// true if this visit created a new people record
        /// </summary>
        public bool memberNew { get; set; }
        public bool mobile { get; set; }
        public int pageVisits { get; set; }
        public string refererPathPage { get; set; }
        public string remote_addr { get; set; }
        //public string remoteName { get; set; }
        public int startDateValue { get; set; }
        public DateTime? startTime { get; set; }
        //public DateTime? stopTime { get; set; }
        public int timeToLastHit { get; set; }
        //public bool verboseReporting { get; set; }
        public bool visitAuthenticated { get; set; }
        public int visitorId { get; set; }
        public bool visitorNew { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// return a visit object for the visitor's last visit before the provided id
        /// </summary>
        /// <param name="core"></param>
        /// <param name="visitId"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public static VisitModel getLastVisitByVisitor(CPBaseClass cp, int visitId, int visitorId) {
            var visitList = DbBaseModel.createList<VisitModel>(cp, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", "id desc");
            if ( visitList.Count>0) {
                return visitList.First();
            }
            return null;
        }
    }
}
