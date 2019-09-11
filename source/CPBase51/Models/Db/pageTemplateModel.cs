
namespace Contensive.Models.Db {
    [System.Serializable]
    public class PageTemplateModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "page templates";
        public const string contentTableNameLowerCase = "cctemplates";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        /// <summary>
        /// A structured json that holds the list of addons to run on this page. Each element in this list includes
        /// - addonGuid
        /// - instanceId (guid of content, etc)
        /// - childList
        /// </summary>
        public string addonList { get; set; }
        public string bodyHTML { get; set; }
        public bool isSecure { get; set; }
    }
}
