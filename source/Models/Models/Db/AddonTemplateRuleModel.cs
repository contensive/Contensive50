

namespace Contensive.Models.Db {
    [System.Serializable]
    public class AddonTemplateRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "add-on template rules";
        public const string contentTableNameLowerCase = "ccAddontemplaterules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int addonId { get; set; }
        public int templateId { get; set; }
    }
}
