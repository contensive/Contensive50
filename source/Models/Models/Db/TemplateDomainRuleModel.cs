
namespace Contensive.Models.Db {
    [System.Serializable]
    public class TemplateDomainRuleModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Template Domain Rules";
        public const string contentTableNameLowerCase = "ccdomaintemplaterules";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int domainId { get; set; }
        public int templateId { get; set; }
    }
}
