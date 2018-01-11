
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    //
    //====================================================================================================
    /// <summary>
    /// miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    /// </summary>
    public class miniCollectionModel : ICloneable {
        //
        //====================================================================================================
        /// <summary>
        /// Name of miniCollection
        /// </summary>
        public string name;
        //
        //====================================================================================================
        /// <summary>
        /// True only for the one collection created from the base file. This property does not transfer during addSrcToDst
        /// Assets created from a base collection can only be modifed by the base collection.
        /// </summary>
        public bool isBaseCollection;
        //
        //====================================================================================================
        /// <summary>
        /// Name dictionary of content definitions in the collection
        /// </summary>
        public Dictionary<string, Models.Complex.cdefModel> cdef = new Dictionary<string, Models.Complex.cdefModel>() { };
        //
        //====================================================================================================
        /// <summary>
        /// Name dictionary of all addons in the miniCollection
        /// </summary>
        public Dictionary<string, collectionAddOnModel> addOns = new Dictionary<string, collectionAddOnModel>() { };
        //
        //====================================================================================================
        /// <summary>
        /// Model of addons in the minicollection
        /// </summary>
        public class collectionAddOnModel {
            public string Name;
            public string Link;
            public string ObjectProgramID;
            public string ArgumentList;
            public string SortOrder;
            public string Copy;
            public bool dataChanged;
        }
        //
        //====================================================================================================
        /// <summary>
        /// List of sql indexes for the minicollection
        /// </summary>
        public List<collectionSQLIndexModel> sqlIndexes = new List<collectionSQLIndexModel> { };
        //
        //====================================================================================================
        /// <summary>
        /// Model of sqlIndexes for the collection
        /// </summary>
        public class collectionSQLIndexModel {
            public string DataSourceName;
            public string TableName;
            public string IndexName;
            public string FieldNameList;
            public bool dataChanged;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Name dictionary for admin navigator menus in the minicollection
        /// </summary>
        public Dictionary<string, collectionMenuModel> menus = new Dictionary<string, collectionMenuModel> { };
        //
        //====================================================================================================
        /// <summary>
        /// Model for menu dictionary
        /// </summary>
        public class collectionMenuModel {
            public string Name;
            public bool IsNavigator;
            public string menuNameSpace;
            public string ParentName;
            public string ContentName;
            public string LinkPage;
            public string SortOrder;
            public bool AdminOnly;
            public bool DeveloperOnly;
            public bool NewWindow;
            public bool Active;
            public string AddonName;
            public bool dataChanged;
            public string Guid;
            public string NavIconType;
            public string NavIconTitle;
            public string Key;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Array of styles for the minicollection
        /// </summary>
       // [Obsolete("Shared styles deprecated")]
        public StyleType[] styles;
        //
        //====================================================================================================
        /// <summary>
        /// Model for style array
        /// </summary>
    //    [Obsolete("Shared styles deprecated")]
        public struct StyleType {
            public string Name;
            public bool Overwrite;
            public string Copy;
            public bool dataChanged;
        }
        //
        //====================================================================================================
        /// <summary>
        /// count of styles
        /// </summary>
    //    [Obsolete("Shared styles deprecated")]
        public int styleCnt;
        //
        // todo
        //====================================================================================================
        /// <summary>
        /// Site style sheet
        /// </summary>
     //   [Obsolete("Shared styles deprecated")]
        public string styleSheet;
        //
        //====================================================================================================
        /// <summary>
        /// List of collections that must be installed before this collection can be installed
        /// </summary>
        public List<ImportCollectionType> collectionImports = new List<ImportCollectionType>() { };
        //
        //====================================================================================================
        /// <summary>
        /// Model for imported collections
        /// </summary>
        public struct ImportCollectionType {
            public string Name;
            public string Guid;
        }
        //
        //====================================================================================================
        /// <summary>
        /// count of page templates in collection
        /// </summary>
        public int pageTemplateCnt;
        //
        //====================================================================================================
        /// <summary>
        /// Array of page templates
        /// </summary>
        public PageTemplateType[] pageTemplates;
        //
        //====================================================================================================
        /// <summary>
        /// Model for page templates
        /// </summary>
        public struct PageTemplateType {
            public string Name;
            public string Copy;
            public string Guid;
            public string Style;
        }
        //
        //====================================================================================================
        /// <summary>
        /// clone object
        /// </summary>
        /// <returns></returns>
        public object Clone() {
            return this.MemberwiseClone();
        }
    }
}
