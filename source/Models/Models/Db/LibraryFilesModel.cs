
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LibraryFilesModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "library Files";
        public const string contentTableNameLowerCase = "cclibraryfiles";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string altSizeList { get; set; }
        public string altText { get; set; }
        public int clicks { get; set; }
        public string description { get; set; }
        public string filename { get; set; }
        public int fileSize { get; set; }
        public int fileTypeId { get; set; }
        public int folderId { get; set; }
        public int height { get; set; }
        public int width { get; set; }
    }
}
