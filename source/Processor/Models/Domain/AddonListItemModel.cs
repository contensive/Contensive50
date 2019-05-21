
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
        /// the Guid of the data instance in this position
        /// </summary>
        public string instanceGuid;
        /// <summary>
        /// if true, this addon
        /// </summary>
        public bool isStructural;
        /// <summary>
        /// If this design block contains a list of design blocks
        /// </summary>
        public List<AddonListItemModel> addonList;
    }
}