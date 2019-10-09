

//
using System.Collections.Generic;

namespace Contensive.Models.Db {
    [System.Serializable]
    public class ContentFieldModel : DbBaseModel {
        //
        //====================================================================================================
        /// <summary>
        /// table definition
        /// </summary>
        public static DbBaseTableMetadataModel tableMetadata { get; } = new DbBaseTableMetadataModel("content fields", "ccfields", "default", false);
        //
        //====================================================================================================
        public bool adminOnly { get; set; }
        public bool authorable { get; set; }
        public string caption { get; set; }
        public int contentID { get; set; }
        public string defaultValue { get; set; }
        public bool developerOnly { get; set; }
        public int editorAddonID { get; set; }
        public int editSortPriority { get; set; }
        public string editTab { get; set; }
        public bool htmlContent { get; set; }
        public int indexColumn { get; set; }
        public int indexSortDirection { get; set; }
        public int indexSortPriority { get; set; }
        public string indexWidth { get; set; }
        public int installedByCollectionID { get; set; }
        public int lookupContentID { get; set; }
        public string lookupList { get; set; }
        public int manyToManyContentID { get; set; }
        public int manyToManyRuleContentID { get; set; }
        public string manyToManyRulePrimaryField { get; set; }
        public string manyToManyRuleSecondaryField { get; set; }
        public int memberSelectGroupID { get; set; }
        public bool notEditable { get; set; }
        public bool password { get; set; }
        public bool readOnly { get; set; }
        public int redirectContentID { get; set; }
        public string redirectID { get; set; }
        public string redirectPath { get; set; }
        public bool required { get; set; }
        public bool rssDescriptionField { get; set; }
        public bool rssTitleField { get; set; }
        public bool scramble { get; set; }
        public bool textBuffered { get; set; }
        public int type { get; set; }
        public bool uniqueName { get; set; }
        public bool isBaseField { get; set; }
    }
}
