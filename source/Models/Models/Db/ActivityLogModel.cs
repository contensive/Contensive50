
namespace Contensive.Models.Db {
    [System.Serializable]
    public class ActivityLogModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Activity Logs", "ccActivityLogs", "default", false);
        //
        //====================================================================================================
        //
        public string link { get; set; }
        public int memberID { get; set; }
        public string message { get; set; }
        public int organizationID { get; set; }
        public int visitID { get; set; }
        public int visitorID { get; set; }
    }
}
