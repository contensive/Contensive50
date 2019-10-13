
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LibraryFolderRuleModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Library Folder Rule", "ccLibraryFolderRules", "default", false);
        //
        //====================================================================================================
        //
        public int folderID { get; set; }
        public int groupID { get; set; }
    }
}
