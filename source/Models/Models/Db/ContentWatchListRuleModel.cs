
namespace Contensive.Models.Db {
    [System.Serializable]
    public class ContentWatchListRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Content Watch List Rules", "ccContentWatchListRules", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int contentWatchID { get; set; }
        public int contentWatchListID { get; set; }
    }
}
