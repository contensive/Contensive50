
using Contensive.Processor.Controllers;
using System.Collections.Generic;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// The structure of an included addon list, stored in page and template records
    /// get and put remote methods in Processor addons
    /// </summary>
    [System.Serializable]
    public class AddonPanelListItemModel {
        /// <summary>
        /// The guid of the design block addon
        /// </summary>
        public string guid;
        /// <summary>
        /// The name of the design block addon
        /// </summary>
        public string name;
        /// <summary>
        /// The full URI of an image that shows the user what it does. Not an icon, but a representation
        /// </summary>
        public string image;
        /// <summary>
        /// a placeholder for a future feature
        /// </summary>
        public List<string> children;
        /// <summary>
        /// placeholder for future feature
        /// </summary>
        public string html;
        /// <summary>
        /// if true, this addon represents a hole where other addons will go
        /// </summary>
        public bool isStructural;
        /// <summary>
        /// if the addon is structural, this is the 12-column spacing. ex - 2 columns should be [6,6], 2 column right [4,8], four column [3,3,3,3]
        /// </summary>
        public List<int> columnSpacing;
    }
}