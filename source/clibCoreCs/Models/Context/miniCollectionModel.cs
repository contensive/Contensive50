
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
namespace Contensive.Core.Models.Entity {
    //
    //
    //====================================================================================================
    /// <summary>
    /// miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    /// </summary>
    public class miniCollectionModel : ICloneable {
        //
        // Content Definitions (some data in CDef, some in the CDef extension)
        //
        public string name;
        public bool isBaseCollection; // true only for the one collection created from the base file. This property does not transfer during addSrcToDst
        public Dictionary<string, Models.Complex.cdefModel> CDef = new Dictionary<string, Models.Complex.cdefModel>();
        //public int SQLIndexCnt;
        public List<SQLIndexType> SQLIndexes;
        public struct SQLIndexType {
            public string DataSourceName;
            public string TableName;
            public string IndexName;
            public string FieldNameList;
            public bool dataChanged;
        }
        public int MenuCnt;
        public MenusType[] Menus;
        public struct MenusType {
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
        public AddOnType[] AddOns;
        public struct AddOnType {
            public string Name;
            public string Link;
            public string ObjectProgramID;
            public string ArgumentList;
            public string SortOrder;
            public string Copy;
            public bool dataChanged;
        }
        public int AddOnCnt;
        public StyleType[] Styles;
        public struct StyleType {
            public string Name;
            public bool Overwrite;
            public string Copy;
            public bool dataChanged;
        }
        public int StyleCnt;
        public string StyleSheet;
        public int ImportCnt;
        public ImportCollectionType[] collectionImports;
        public struct ImportCollectionType {
            public string Name;
            public string Guid;
        }
        public int PageTemplateCnt;
        public PageTemplateType[] PageTemplates;
        //   Page Template - started, but CDef2 and LoadDataToCDef are all that is done do far
        public struct PageTemplateType {
            public string Name;
            public string Copy;
            public string Guid;
            public string Style;
        }
        public object Clone() {
            return this.MemberwiseClone();
        }
    }
}
