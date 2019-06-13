
using Contensive.Processor.Controllers;
using System.Collections.Generic;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// The structure of an included addon list, stored in page and template records
    /// get and put remote methods in Processor addons
    /// </summary>
    [System.Serializable]
    public class AddonListItemModel {
        /// <summary>
        /// The guid of the design block addon that is in this position
        /// </summary>
        public string designBlockTypeGuid;
        /// <summary>
        /// The name of the design block to be used for non-rendered mode
        /// </summary>
        public string designBlockTypeName;
        /// <summary>
        /// the Guid of the data instance in this position
        /// </summary>
        public string instanceGuid;
        /// <summary>
        /// todo - create view model separate from domain model because UI mode might need it
        /// Not saved in DB, use only for view. 
        /// </summary>
        public string renderedHtml;
        /// <summary>
        /// Assets added to the html document during html rendering
        /// </summary>
        public AddonAssetsModel renderedAssets;
        /// <summary>
        /// If this design block is structural, it contains one or more addon lists
        /// </summary>
        public List<AddonListColumnItemModel> columns;
    }
    /// <summary>
    /// Assets added to the html document during html rendering
    /// </summary>
    [System.Serializable]
    public class AddonAssetsModel {
        /// <summary>
        /// css styles added to the head
        /// </summary>
        public List<string> headStyles = new List<string>();
        //
        public List<string> headStylesheetLinks = new List<string>();
        //
        public List<string> headJs = new List<string>();
        //
        public List<string> headJsLinks = new List<string>();
        //
        public List<string> bodyJs = new List<string>();
        //
        public List<string> bodyJsLinks = new List<string>();
    }
    [System.Serializable]
    public class AddonListColumnItemModel {
        /// <summary>
        /// the integer width of a column, where the row totals 12
        /// </summary>
        public int col;
        /// <summary>
        /// optional class that represents the width of the column
        /// </summary>
        public string className;
        /// <summary>
        /// Each column contains an addon list. This extra object layer was created to make it more convenient for the UI javascript
        /// </summary>
        public List<AddonListItemModel> addonList;
    }
    //
}