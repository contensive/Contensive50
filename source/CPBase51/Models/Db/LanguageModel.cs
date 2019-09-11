
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LanguageModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "languages";
        public const string contentTableNameLowerCase = "cclanguages";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string http_Accept_Language { get; set; }
    }
}
