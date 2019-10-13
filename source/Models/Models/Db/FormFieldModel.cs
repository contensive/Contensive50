
namespace Contensive.Models.Db {
    [System.Serializable]
    public class FormFieldModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Form Fields", "ccFormFields", "default", false);
        //
        //====================================================================================================
        //
        public int buttonActionID { get; set; }
        public string caption { get; set; }
        public int contentFieldID { get; set; }
        public int formID { get; set; }
        public string inputType { get; set; }
        public string replaceText { get; set; }
        public bool required { get; set; }
    }
}
