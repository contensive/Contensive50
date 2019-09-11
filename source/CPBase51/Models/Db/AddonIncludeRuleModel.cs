
namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonIncludeRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Add-on Include Rules";
        public const string contentTableNameLowerCase = "ccaddonincluderules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonID { get; set; }
        public int includedAddonID { get; set; }
    }
}
