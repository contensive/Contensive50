
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LinkForwardModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("link forwards", "cclinkforwards", "default", true);
        //
        //====================================================================================================
        public string DestinationLink { get; set; }
        public int GroupID { get; set; }
        public string SourceLink { get; set; }
        public int Viewings { get; set; }
    }
}
