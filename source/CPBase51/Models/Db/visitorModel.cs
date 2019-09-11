
namespace Contensive.Models.Db {
    [System.Serializable]
    public class VisitorModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "visitors";
        public const string contentTableNameLowerCase = "ccvisitors";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int MemberID { get; set; }
        public int ForceBrowserMobile { get; set; }
    }
}
