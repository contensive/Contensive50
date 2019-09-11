
namespace Contensive.Models.Db {
    [System.Serializable]
    public class GroupRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "group rules";
        public const string contentTableNameLowerCase = "ccgrouprules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public bool allowAdd { get; set; }
        public bool allowDelete { get; set; }        
        public int contentID { get; set; }
        public int groupID { get; set; }
    }
}
