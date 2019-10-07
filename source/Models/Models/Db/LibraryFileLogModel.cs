
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LibraryFileLogModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("library File log", "cclibrarydownloadlog", "default", false);
        //
        //====================================================================================================
        public int fileID { get; set; }
        public int memberID { get; set; }
        public int visitID { get; set; }
        public string FromUrl { get; set; }
    }
}
