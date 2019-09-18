
namespace Contensive.Models.Db {
    [System.Serializable]
    public class CopyContentModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "copy content";
        public const string contentTableNameLowerCase = "cccopycontent";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string copy { get; set; }
    }
}
