
namespace Contensive.Models.Db {
    [System.Serializable]
    public class VisitorModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("visitors", "ccvisitors", "default", false);
        //
        //====================================================================================================
        public int MemberID { get; set; }
        public int ForceBrowserMobile { get; set; }
    }
}
