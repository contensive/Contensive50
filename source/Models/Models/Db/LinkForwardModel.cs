
namespace Contensive.Models.Db {
    [System.Serializable]
    public class LinkForwardModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "link forwards";
        public const string contentTableNameLowerCase = "cclinkforwards";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public string DestinationLink;
        public int GroupID;
        public string SourceLink;
        public int Viewings;
    }
}
