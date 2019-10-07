
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
        public string DestinationLink;
        public int GroupID;
        public string SourceLink;
        public int Viewings;
    }
}
