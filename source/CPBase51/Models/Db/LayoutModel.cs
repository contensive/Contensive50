
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LayoutModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const (must be public const, not property)
        public const string contentName = "layouts";
        public const string contentTableNameLowerCase = "cclayouts";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties (must be properties not fields)
        public DbBaseModel.FieldTypeTextFile layout { get; set; }
        public string stylesFilename { get; set; }
    }
}
