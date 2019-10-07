
using Contensive.BaseClasses;
using System.Collections.Generic;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class ContentModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("Content", "cccontent", "default", true);
        //
        //====================================================================================================
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
        //
        //====================================================================================================
        /// <summary>
        /// Create a list of content records that are assocated to a collection, alphabetically by content name
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="collectionId"></param>
        /// <returns></returns>
        public static List<ContentModel> createListFromCollection(CPBaseClass cp, int collectionId) {
            return createList<ContentModel>(cp, "id in (select distinct contentId from ccAddonCollectionCDefRules where collectionid=" + collectionId + ")", "name");

        }
    }
}
