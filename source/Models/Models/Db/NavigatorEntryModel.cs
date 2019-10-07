
namespace Contensive.Models.Db {
    [System.Serializable]
    public class NavigatorEntryModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Navigator Entries", "ccmenuentries", "default", false);
        //
        //====================================================================================================
        public int ParentID { get; set; }
        public string NavIconTitle { get; set; }
        public int NavIconType { get; set; }
        public int AddonID { get; set; }
        public bool AdminOnly { get; set; }
        public int ContentID { get; set; }
        public bool DeveloperOnly { get; set; }
        public int HelpAddonID { get; set; }
        public int HelpCollectionID { get; set; }
        public int InstalledByCollectionID { get; set; }
        public string LinkPage { get; set; }
        public bool NewWindow { get; set; }
    }
}
