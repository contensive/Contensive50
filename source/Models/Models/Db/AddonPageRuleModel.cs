
namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonPageRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on page rules";
        public const string contentTableNameLowerCase = "ccaddonpagerules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int pageId { get; set; }
    }
}
