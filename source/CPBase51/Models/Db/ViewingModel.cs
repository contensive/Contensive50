
using System;

namespace Contensive.Models.Db {
    [Serializable]
    public class ViewingModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const (must be const not property)
        /// <summary>
        /// The content metadata name for this table
        /// </summary>
        public const string contentName = "viewings";
        /// <summary>
        /// The sql server table name
        /// </summary>
        public const string contentTableNameLowerCase = "ccviewings";
        /// <summary>
        /// The Contensive datasource. Use "default" or blank for the default datasource stored in the server config file
        /// </summary>
        public const string contentDataSource = "default";
        /// <summary>
        /// set true if the name field's value for all records must be unique (no duplicates). Used for cache ptr generation
        /// </summary>
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties (must be properties not fields)
        /// <summary>
        /// do not count this record in analytics, for example if it for a background process, or an administrator visit
        /// </summary>
        public bool excludeFromAnalytics { get; set; }
        public string form { get; set; }
        public string host { get; set; }
        public int memberID { get; set; }
        public string page { get; set; }
        public int pageTime { get; set; }
        public string pageTitle { get; set; }
        public string path { get; set; }
        public string queryString { get; set; }
        public int recordID { get; set; }
        public string referer { get; set; }
        public bool stateOK { get; set; }
        public int visitID { get; set; }
        public int visitorID { get; set; }
    }
}
