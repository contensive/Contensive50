
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LibraryFileLogModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library File log";
        public const string contentTableNameLowerCase = "cclibrarydownloadlog";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int fileID { get; set; }
        public int memberID { get; set; }
        public int visitID { get; set; }
        public string FromUrl { get; set; }
    }
}
