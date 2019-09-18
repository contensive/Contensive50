
namespace Contensive.Models.Db {
    [System.Serializable]
    public class NavigatorEntryModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "Navigator Entries";
        public const string contentTableNameLowerCase = "ccmenuentries";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = false;
        //
        //====================================================================================================
        // -- instance properties
        public int ParentID { get; set; }
        public string NavIconTitle { get; set; }
        public int NavIconType { get; set; }
        public int AddonID { get; set; }
        public bool AdminOnly { get; set; }
        public int ContentID { get; set; }
        public bool DeveloperOnly { get; set; }
        public int HelpAddonID { get; set; }
        public int HelpCollectionID { get; set; }
        public int InstalledByCollectionID { get; set; }
        public string LinkPage { get; set; }
        public bool NewWindow { get; set; }
    }
}
