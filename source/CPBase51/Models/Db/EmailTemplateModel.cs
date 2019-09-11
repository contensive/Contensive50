
namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailTemplateModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "email templates";
        public const string contentTableNameLowerCase = "cctemplates";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string bodyHTML { get; set; }
        public string source { get; set; }
    }
}
