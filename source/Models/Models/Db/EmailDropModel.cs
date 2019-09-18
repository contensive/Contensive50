

namespace Contensive.Models.Db {
    [System.Serializable]
    public class EmailDropModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Email Drops";
        public const string contentTableNameLowerCase = "ccemaildrops";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int emailID { get; set; }
    }
}
