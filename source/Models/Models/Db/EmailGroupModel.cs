
namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailGroupModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Email Groups", "ccEmailGroups", "default", false);
        //
        //====================================================================================================
        /// <summary>
        /// field properties
        /// </summary>
        public int emailID { get; set; }
        public int groupID { get; set; }

    }
}
