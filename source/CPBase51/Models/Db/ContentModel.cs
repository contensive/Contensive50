
namespace Contensive.Models.Db {
    [System.Serializable]
    public class ContentModel : DbBaseModel {
        //
        //====================================================================================================
        //-- const
        public const string contentName = "content";
        public const string contentTableNameLowerCase = "cccontent";
        public const string contentDataSource = "default";
        public const bool nameFieldIsUnique = true;
        //
        //====================================================================================================
        // -- instance properties
        public bool adminOnly { get; set; }
        public bool allowAdd { get; set; }
        public bool allowContentChildTool { get; set; }
        public bool allowContentTracking { get; set; }
        public bool allowDelete { get; set; }
        public bool allowTopicRules { get; set; }
        public bool allowWorkflowAuthoring { get; set; }
        public int authoringTableID { get; set; }
        public int contentTableID { get; set; }
        public int defaultSortMethodID { get; set; }
        public bool developerOnly { get; set; }
        public string dropDownFieldList { get; set; }
        public int editorGroupID { get; set; }
        public int iconHeight { get; set; }
        public string iconLink { get; set; }
        public int iconSprites { get; set; }
        public int iconWidth { get; set; }
        public int installedByCollectionID { get; set; }
        public bool isBaseContent { get; set; }
        public int parentID { get; set; }
    }
}
