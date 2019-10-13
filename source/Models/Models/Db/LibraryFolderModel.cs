
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LibraryFolderModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Library Folder", "ccLibraryFolder", "default", false);
        //
        //====================================================================================================
        //
        public string Description { get; set; }
        public int ParentID { get; set; }
    }
}
