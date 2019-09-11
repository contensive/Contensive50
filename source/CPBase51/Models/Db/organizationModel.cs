
namespace Contensive.Models.Db {
    [System.Serializable]
    public class OrganizationModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "organizations";
        public const string contentTableNameLowerCase = "organizations";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string briefFilename { get; set; }
        public string city { get; set; }
        public int clicks { get; set; }
        public int contactMemberID { get; set; }
        public string copyFilename { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string fax { get; set; }
        public string imageFilename { get; set; }
        public string link { get; set; }
        public string phone { get; set; }
        public string state { get; set; }
        public string thumbNailFilename { get; set; }
        public int viewings { get; set; }
        public string web { get; set; }
        public string zip { get; set; }
    }
}
