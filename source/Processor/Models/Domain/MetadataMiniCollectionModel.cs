
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;

using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
using Contensive.Models.Db;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// miniCollection - This is an old collection object used in part to load the metadata part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    /// </summary>
    [System.Serializable]
    public class MetadataMiniCollectionModel : ICloneable {
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
        public Dictionary<string, Models.Domain.ContentMetadataModel> metaData = new Dictionary<string, Models.Domain.ContentMetadataModel>();
        //
        //====================================================================================================
        /// <summary>
        /// List of sql indexes for the minicollection
        /// </summary>
        public List<MiniCollectionSQLIndexModel> sqlIndexes = new List<MiniCollectionSQLIndexModel> { };
        //
        //====================================================================================================
        /// <summary>
        /// Model of sqlIndexes for the collection
        /// </summary>
        [Serializable]
        public class MiniCollectionSQLIndexModel {
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
        public Dictionary<string, MiniCollectionMenuModel> menus = new Dictionary<string, MiniCollectionMenuModel> { };
        //
        //====================================================================================================
        /// <summary>
        /// Model for menu dictionary
        /// </summary>
        [Serializable]
        public class MiniCollectionMenuModel {
            public string name;
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
            public string AddonGuid;
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
        // [Obsolete("Shared styles deprecated")]
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
        // [Obsolete("Shared styles deprecated")]
        public int styleCnt;
        //
        // todo
        //====================================================================================================
        /// <summary>
        /// Site style sheet
        /// </summary>
        // [Obsolete("Shared styles deprecated")]
        public string styleSheet;
        //
        //====================================================================================================
        /// <summary>
        /// count of page templates in collection
        /// </summary>
        public int pageTemplateCnt { get; set; }
        //
        //====================================================================================================
        /// <summary>
        /// Array of page templates in collection
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
        //======================================================================================================
        //
        internal static void installMetaDataMiniCollectionFromXml(bool quick, CoreController core, string srcXml, bool isNewBuild, bool reinstallDependencies, bool isBaseCollection, ref List<string> nonCriticalErrorList, string logPrefix) {
            try {
                {
                    MetadataMiniCollectionModel newCollection = loadXML(core, srcXml, isBaseCollection, true, isNewBuild, logPrefix);
                    installMetaDataMiniCollection_BuildDb(core, isBaseCollection, newCollection, isNewBuild, reinstallDependencies, ref nonCriticalErrorList, logPrefix);
                    return;
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// create a collection class from a collection xml file, metadata are added to the metadatas in the application collection
        /// </summary>
        public static MetadataMiniCollectionModel loadXML(CoreController core, string srcCollecionXml, bool isBaseCollection, bool setAllDataChanged, bool IsNewBuild, string logPrefix) {
            MetadataMiniCollectionModel result = null;
            try {
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", Upgrademetadata_LoadDataToCollection");
                //
                result = new MetadataMiniCollectionModel();
                if (string.IsNullOrEmpty(srcCollecionXml)) {
                    //
                    // -- empty collection is an error
                    throw (new GenericException("Upgrademetadata_LoadDataToCollection, srcCollectionXml is blank or null"));
                } else {
                    XmlDocument srcXmlDom = new XmlDocument();
                    try {
                        srcXmlDom.LoadXml(srcCollecionXml);
                    } catch (Exception ex) {
                        //
                        // -- xml load error
                        LogController.logError(core, "Upgrademetadata_LoadDataToCollection Error reading xml archive, ex=[" + ex + "]");
                        throw new Exception("Error in Upgrademetadata_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    if ((srcXmlDom.DocumentElement.Name.ToLowerInvariant() != CollectionFileRootNode) && (srcXmlDom.DocumentElement.Name.ToLowerInvariant() != "contensivecdef")) {
                        //
                        // -- root node must be collection (or legacy contensivemetadata)
                        LogController.logError(core, new GenericException("the archive file has a syntax error. Application name must be the first node."));
                    } else {
                        result.isBaseCollection = isBaseCollection;
                        bool Found = false;
                        //
                        // Get Collection Name for logs
                        //
                        string Collectionname = XmlController.getXMLAttribute(core, Found, srcXmlDom.DocumentElement, "name", "");
                        if (string.IsNullOrEmpty(Collectionname)) {
                            LogController.logInfo(core, "Upgrademetadata_LoadDataToCollection, Application: " + core.appConfig.name + ", Collection has no name");
                        }
                        result.name = Collectionname;
                        //
                        foreach (XmlNode metaData_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes) {
                            XmlNode metaData_Node = metaData_NodeWithinLoop;
                            string NodeName = GenericController.toLCase(metaData_NodeWithinLoop.Name);
                            bool IsNavigator = false;
                            string Name = "";
                            string MenuName = null;
                            string IndexName = null;
                            string TableName = null;
                            int Ptr = 0;
                            string menuNameSpace = null;
                            string MenuGuid = null;
                            string DataSourceName = null;
                            string ActiveText = null;
                            switch (NodeName) {
                                case "cdef": {
                                        //
                                        // Content Definitions
                                        //
                                        string contentName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "name", "");
                                        if (string.IsNullOrEmpty(contentName)) {
                                            throw (new GenericException("Collection xml file load includes a content metadata node with no name."));
                                        }
                                        string contentGuid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", "");
                                        ContentMetadataModel DefaultMetaData = null;
                                        if (!isBaseCollection) {
                                            //
                                            // -- if base collection loaded, attmpt load. Some cases during startup happen before content is available so exceptions should be skipped
                                            if (!string.IsNullOrWhiteSpace(contentGuid)) {
                                                DefaultMetaData = ContentMetadataModel.create(core, contentGuid);
                                            } else {
                                                DefaultMetaData = ContentMetadataModel.createByUniqueName(core, contentName);
                                            }
                                        }
                                        if (DefaultMetaData == null) {
                                            DefaultMetaData = new ContentMetadataModel {
                                                guid = contentGuid,
                                                name = contentName,
                                                active = true
                                            };
                                        }
                                        //
                                        // These two fields are needed to import the row
                                        //
                                        DataSourceName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "dataSource", DefaultMetaData.dataSourceName);
                                        if (string.IsNullOrEmpty(DataSourceName)) {
                                            DataSourceName = "Default";
                                        }
                                        //
                                        // ----- Add metadata if not already there
                                        //
                                        if (!result.metaData.ContainsKey(contentName.ToLowerInvariant())) {
                                            result.metaData.Add(contentName.ToLowerInvariant(), new Models.Domain.ContentMetadataModel());
                                        }
                                        //
                                        // Get metadata attributes
                                        //
                                        ContentMetadataModel targetMetaData = result.metaData[contentName.ToLowerInvariant()];
                                        string activeDefaultText = "1";
                                        if (!(DefaultMetaData.active)) {
                                            activeDefaultText = "0";
                                        }
                                        ActiveText = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Active", activeDefaultText);
                                        if (string.IsNullOrEmpty(ActiveText)) {
                                            ActiveText = "1";
                                        }
                                        targetMetaData.active = GenericController.encodeBoolean(ActiveText);
                                        targetMetaData.activeOnly = true;
                                        //.adminColumns = ?
                                        targetMetaData.adminOnly = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AdminOnly", DefaultMetaData.adminOnly);
                                        targetMetaData.aliasId = "id";
                                        targetMetaData.aliasName = "name";
                                        targetMetaData.allowAdd = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowAdd", DefaultMetaData.allowAdd);
                                        targetMetaData.allowCalendarEvents = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowCalendarEvents", DefaultMetaData.allowCalendarEvents);
                                        targetMetaData.allowContentChildTool = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowContentChildTool", DefaultMetaData.allowContentChildTool);
                                        targetMetaData.allowContentTracking = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowContentTracking", DefaultMetaData.allowContentTracking);
                                        targetMetaData.allowDelete = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowDelete", DefaultMetaData.allowDelete);
                                        targetMetaData.allowTopicRules = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowTopicRules", DefaultMetaData.allowTopicRules);
                                        targetMetaData.guid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", DefaultMetaData.guid);
                                        targetMetaData.dataChanged = setAllDataChanged;
                                        targetMetaData.legacyContentControlCriteria = "";
                                        targetMetaData.dataSourceName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentDataSourceName", DefaultMetaData.dataSourceName);
                                        targetMetaData.tableName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentTableName", DefaultMetaData.tableName);
                                        targetMetaData.dataSourceId = 0;
                                        targetMetaData.defaultSortMethod = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "DefaultSortMethod", DefaultMetaData.defaultSortMethod);
                                        if ((targetMetaData.defaultSortMethod == null) || (targetMetaData.defaultSortMethod == "") || (targetMetaData.defaultSortMethod.ToLowerInvariant() == "name")) {
                                            targetMetaData.defaultSortMethod = "By Name";
                                        } else if (GenericController.toLCase(targetMetaData.defaultSortMethod) == "sortorder") {
                                            targetMetaData.defaultSortMethod = "By Alpha Sort Order Field";
                                        } else if (GenericController.toLCase(targetMetaData.defaultSortMethod) == "date") {
                                            targetMetaData.defaultSortMethod = "By Date";
                                        }
                                        targetMetaData.developerOnly = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "DeveloperOnly", DefaultMetaData.developerOnly);
                                        targetMetaData.dropDownFieldList = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "DropDownFieldList", DefaultMetaData.dropDownFieldList);
                                        targetMetaData.editorGroupName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "EditorGroupName", DefaultMetaData.editorGroupName);
                                        targetMetaData.fields = new Dictionary<string, Models.Domain.ContentFieldMetadataModel>();
                                        targetMetaData.iconLink = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "IconLink", DefaultMetaData.iconLink);
                                        targetMetaData.iconHeight = XmlController.getXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconHeight", DefaultMetaData.iconHeight);
                                        targetMetaData.iconWidth = XmlController.getXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconWidth", DefaultMetaData.iconWidth);
                                        targetMetaData.iconSprites = XmlController.getXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconSprites", DefaultMetaData.iconSprites);
                                        targetMetaData.includesAFieldChange = false;
                                        targetMetaData.installedByCollectionGuid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "installedByCollection", DefaultMetaData.installedByCollectionGuid);
                                        targetMetaData.isBaseContent = isBaseCollection || XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "IsBaseContent", false);
                                        targetMetaData.isModifiedSinceInstalled = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "IsModified", DefaultMetaData.isModifiedSinceInstalled);
                                        targetMetaData.name = contentName;
                                        targetMetaData.parentName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Parent", DefaultMetaData.parentName);
                                        targetMetaData.whereClause = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "WhereClause", DefaultMetaData.whereClause);
                                        //
                                        // -- determine id
                                        targetMetaData.id = DbController.getContentId(core, contentName);
                                        //
                                        // Get metadata field nodes
                                        //
                                        foreach (XmlNode MetaDataChildNode in metaData_NodeWithinLoop.ChildNodes) {
                                            //
                                            // ----- process metadata Field
                                            //
                                            if (textMatch(MetaDataChildNode.Name, "field")) {
                                                string FieldName = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "Name", "");
                                                ContentFieldMetadataModel DefaultMetaDataField = null;
                                                //
                                                // try to find field in the defaultmetadata
                                                //
                                                if (DefaultMetaData.fields.ContainsKey(FieldName)) {
                                                    DefaultMetaDataField = DefaultMetaData.fields[FieldName];
                                                } else {
                                                    DefaultMetaDataField = new Models.Domain.ContentFieldMetadataModel();
                                                }
                                                //
                                                if (!(result.metaData[contentName.ToLowerInvariant()].fields.ContainsKey(FieldName.ToLowerInvariant()))) {
                                                    result.metaData[contentName.ToLowerInvariant()].fields.Add(FieldName.ToLowerInvariant(), new Models.Domain.ContentFieldMetadataModel());
                                                }
                                                var metaDataField = result.metaData[contentName.ToLowerInvariant()].fields[FieldName.ToLowerInvariant()];
                                                metaDataField.nameLc = FieldName.ToLowerInvariant();
                                                ActiveText = "0";
                                                if (DefaultMetaDataField.active) {
                                                    ActiveText = "1";
                                                }
                                                ActiveText = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "Active", ActiveText);
                                                if (string.IsNullOrEmpty(ActiveText)) {
                                                    ActiveText = "1";
                                                }
                                                metaDataField.active = GenericController.encodeBoolean(ActiveText);
                                                //
                                                // Convert Field Descriptor (text) to field type (integer)
                                                //
                                                string defaultFieldTypeName = ContentFieldMetadataModel.getFieldTypeNameFromFieldTypeId(core, DefaultMetaDataField.fieldTypeId);
                                                string fieldTypeName = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "FieldType", defaultFieldTypeName);
                                                metaDataField.fieldTypeId = core.db.getFieldTypeIdFromFieldTypeName(fieldTypeName);
                                                metaDataField.editSortPriority = XmlController.getXMLAttributeInteger(core, Found, MetaDataChildNode, "EditSortPriority", DefaultMetaDataField.editSortPriority);
                                                metaDataField.authorable = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "Authorable", DefaultMetaDataField.authorable);
                                                metaDataField.caption = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "Caption", DefaultMetaDataField.caption);
                                                metaDataField.defaultValue = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "DefaultValue", DefaultMetaDataField.defaultValue);
                                                metaDataField.notEditable = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "NotEditable", DefaultMetaDataField.notEditable);
                                                metaDataField.indexColumn = XmlController.getXMLAttributeInteger(core, Found, MetaDataChildNode, "IndexColumn", DefaultMetaDataField.indexColumn);
                                                metaDataField.indexWidth = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "IndexWidth", DefaultMetaDataField.indexWidth);
                                                metaDataField.indexSortOrder = XmlController.getXMLAttributeInteger(core, Found, MetaDataChildNode, "IndexSortOrder", DefaultMetaDataField.indexSortOrder);
                                                metaDataField.redirectId = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "RedirectID", DefaultMetaDataField.redirectId);
                                                metaDataField.redirectPath = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "RedirectPath", DefaultMetaDataField.redirectPath);
                                                metaDataField.htmlContent = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "HTMLContent", DefaultMetaDataField.htmlContent);
                                                metaDataField.uniqueName = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "UniqueName", DefaultMetaDataField.uniqueName);
                                                metaDataField.password = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "Password", DefaultMetaDataField.password);
                                                metaDataField.adminOnly = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "AdminOnly", DefaultMetaDataField.adminOnly);
                                                metaDataField.developerOnly = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "DeveloperOnly", DefaultMetaDataField.developerOnly);
                                                metaDataField.readOnly = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "ReadOnly", DefaultMetaDataField.readOnly);
                                                metaDataField.required = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "Required", DefaultMetaDataField.required);
                                                metaDataField.rssTitleField = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "RSSTitle", DefaultMetaDataField.rssTitleField);
                                                metaDataField.rssDescriptionField = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "RSSDescriptionField", DefaultMetaDataField.rssDescriptionField);
                                                metaDataField.memberSelectGroupName_set(core, XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "MemberSelectGroup", ""));
                                                metaDataField.editTabName = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "EditTab", DefaultMetaDataField.editTabName);
                                                metaDataField.scramble = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "Scramble", DefaultMetaDataField.scramble);
                                                metaDataField.lookupList = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "LookupList", DefaultMetaDataField.lookupList);
                                                metaDataField.manyToManyRulePrimaryField = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRulePrimaryField", DefaultMetaDataField.manyToManyRulePrimaryField);
                                                metaDataField.manyToManyRuleSecondaryField = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRuleSecondaryField", DefaultMetaDataField.manyToManyRuleSecondaryField);
                                                metaDataField.set_lookupContentName(core, XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "LookupContent", DefaultMetaDataField.get_lookupContentName(core)));
                                                // isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                // case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                // case 2 - when the base file is loaded, all fields must include the attribute
                                                metaDataField.isBaseField = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "IsBaseField", false) || isBaseCollection;
                                                metaDataField.set_redirectContentName(core, XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "RedirectContent", DefaultMetaDataField.get_redirectContentName(core)));
                                                metaDataField.set_manyToManyContentName(core, XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyContent", DefaultMetaDataField.get_manyToManyContentName(core)));
                                                metaDataField.set_manyToManyRuleContentName(core, XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRuleContent", DefaultMetaDataField.get_manyToManyRuleContentName(core)));
                                                metaDataField.isModifiedSinceInstalled = XmlController.getXMLAttributeBoolean(core, Found, MetaDataChildNode, "IsModified", DefaultMetaDataField.isModifiedSinceInstalled);
                                                metaDataField.installedByCollectionGuid = XmlController.getXMLAttribute(core, Found, MetaDataChildNode, "installedByCollectionId", DefaultMetaDataField.installedByCollectionGuid);
                                                metaDataField.id = DbController.getContentFieldId(core, targetMetaData.id, metaDataField.nameLc);
                                                metaDataField.dataChanged = setAllDataChanged;
                                                //
                                                // ----- handle child nodes (help node)
                                                //
                                                metaDataField.helpCustom = "";
                                                metaDataField.helpDefault = "";
                                                foreach (XmlNode FieldChildNode in MetaDataChildNode.ChildNodes) {
                                                    //
                                                    // ----- process metadata Field
                                                    //
                                                    if (textMatch(FieldChildNode.Name, "HelpDefault")) {
                                                        metaDataField.helpDefault = FieldChildNode.InnerText;
                                                    }
                                                    if (textMatch(FieldChildNode.Name, "HelpCustom")) {
                                                        metaDataField.helpCustom = FieldChildNode.InnerText;
                                                    }
                                                    metaDataField.helpChanged = setAllDataChanged;
                                                }
                                            }
                                        }
                                        break;
                                    }
                                case "sqlindex": {
                                        //
                                        // SQL Indexes
                                        //
                                        IndexName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "indexname", "");
                                        TableName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "tableName", "");
                                        DataSourceName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "DataSourceName", "");
                                        if (string.IsNullOrEmpty(DataSourceName)) {
                                            DataSourceName = "default";
                                        }
                                        bool removeDup = false;
                                        MetadataMiniCollectionModel.MiniCollectionSQLIndexModel dupToRemove = new MetadataMiniCollectionModel.MiniCollectionSQLIndexModel();
                                        foreach (MetadataMiniCollectionModel.MiniCollectionSQLIndexModel index in result.sqlIndexes) {
                                            if (textMatch(index.IndexName, IndexName) & textMatch(index.TableName, TableName) & textMatch(index.DataSourceName, DataSourceName)) {
                                                dupToRemove = index;
                                                removeDup = true;
                                                break;
                                            }
                                        }
                                        if (removeDup) {
                                            result.sqlIndexes.Remove(dupToRemove);
                                        }
                                        MetadataMiniCollectionModel.MiniCollectionSQLIndexModel newIndex = new MetadataMiniCollectionModel.MiniCollectionSQLIndexModel {
                                            IndexName = IndexName,
                                            TableName = TableName,
                                            DataSourceName = DataSourceName,
                                            FieldNameList = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "FieldNameList", "")
                                        };
                                        result.sqlIndexes.Add(newIndex);
                                        break;
                                    }
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry": {
                                        //
                                        // Admin Menus / Navigator Entries
                                        MenuName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Name", "");
                                        menuNameSpace = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "NameSpace", "");
                                        MenuGuid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", "");
                                        IsNavigator = (NodeName == "navigatorentry");
                                        string MenuKey = null;
                                        if (!IsNavigator) {
                                            MenuKey = GenericController.toLCase(MenuName);
                                        } else {
                                            MenuKey = MenuGuid;
                                        }
                                        if (!result.menus.ContainsKey(MenuKey)) {
                                            ActiveText = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Active", "1");
                                            if (string.IsNullOrEmpty(ActiveText)) {
                                                ActiveText = "1";
                                            }
                                            result.menus.Add(MenuKey, new MetadataMiniCollectionModel.MiniCollectionMenuModel {
                                                dataChanged = setAllDataChanged,
                                                name = MenuName,
                                                Guid = MenuGuid,
                                                Key = MenuKey,
                                                Active = GenericController.encodeBoolean(ActiveText),
                                                menuNameSpace = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "NameSpace", ""),
                                                ParentName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "ParentName", ""),
                                                ContentName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentName", ""),
                                                LinkPage = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "LinkPage", ""),
                                                SortOrder = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "SortOrder", ""),
                                                AdminOnly = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AdminOnly", false),
                                                DeveloperOnly = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "DeveloperOnly", false),
                                                NewWindow = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "NewWindow", false),
                                                AddonName = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "AddonName", ""),
                                                AddonGuid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "AddonGuid", ""),
                                                NavIconType = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "NavIconType", ""),
                                                NavIconTitle = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "NavIconTitle", ""),
                                                IsNavigator = IsNavigator
                                            });
                                        }
                                        break;
                                    }
                                case "aggregatefunction":
                                case "addon": {
                                        // do nothing
                                        break;
                                    }
                                case "style": {
                                        //
                                        // style sheet entries
                                        //
                                        Name = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Name", "");
                                        if (result.styleCnt > 0) {
                                            for (Ptr = 0; Ptr < result.styleCnt; Ptr++) {
                                                if (textMatch(result.styles[Ptr].Name, Name)) {
                                                    break;
                                                }
                                            }
                                        }
                                        if (Ptr >= result.styleCnt) {
                                            Ptr = result.styleCnt;
                                            result.styleCnt = result.styleCnt + 1;
                                            Array.Resize(ref result.styles, Ptr);
                                            result.styles[Ptr].Name = Name;
                                        }
                                        var tempVar5 = result.styles[Ptr];
                                        tempVar5.dataChanged = setAllDataChanged;
                                        tempVar5.Overwrite = XmlController.getXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "Overwrite", false);
                                        tempVar5.Copy = metaData_NodeWithinLoop.InnerText;
                                        break;
                                    }
                                case "stylesheet": {
                                        //
                                        // style sheet in one entry
                                        //
                                        result.styleSheet = metaData_NodeWithinLoop.InnerText;
                                        break;
                                    }
                                case "pagetemplate": {
                                        //
                                        if (result.pageTemplateCnt > 0) {
                                            for (Ptr = 0; Ptr < result.pageTemplateCnt; Ptr++) {
                                                if (textMatch(result.pageTemplates[Ptr].Name, Name)) {
                                                    break;
                                                }
                                            }
                                        }
                                        if (Ptr >= result.pageTemplateCnt) {
                                            Ptr = result.pageTemplateCnt;
                                            result.pageTemplateCnt = result.pageTemplateCnt + 1;
                                            Array.Resize(ref result.pageTemplates, Ptr);
                                            result.pageTemplates[Ptr].Name = Name;
                                        }
                                        var tempVar6 = result.pageTemplates[Ptr];
                                        tempVar6.Copy = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "Copy", "");
                                        tempVar6.Guid = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", "");
                                        tempVar6.Style = XmlController.getXMLAttribute(core, Found, metaData_NodeWithinLoop, "style", "");
                                        break;
                                    }
                                default: {
                                        // do nothing
                                        break;
                                    }
                            }
                        }
                        //
                        // Convert Menus.ParentName to Menu.menuNameSpace
                        //
                        foreach (var kvp in result.menus) {
                            MetadataMiniCollectionModel.MiniCollectionMenuModel menu = kvp.Value;
                            if (!string.IsNullOrEmpty(menu.ParentName)) {
                                menu.menuNameSpace = GetMenuNameSpace(core, result.menus, menu, "");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Verify ccContent and ccFields records from the metadata nodes of a a collection file. This is the last step of loading teh metadata nodes of a collection file. ParentId field is set based on ParentName node.
        /// </summary>
        private static void installMetaDataMiniCollection_BuildDb(CoreController core, bool isBaseCollection, MetadataMiniCollectionModel Collection, bool isNewBuild, bool reinstallDependencies, ref List<string> nonCriticalErrorList, string logPrefix) {
            try {
                //
                logPrefix += ", installCollection_BuildDbFromMiniCollection";
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", Upgrademetadata_BuildDbFromCollection");
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 1: create SQL tables in default datasource");
                //----------------------------------------------------------------------------------------------------------------------
                //
                {
                    foreach (KeyValuePair<string, ContentMetadataModel> metaKvp in Collection.metaData) {
                        if (string.IsNullOrWhiteSpace(metaKvp.Value.tableName)) {
                            LogController.logWarn(core, "Content [" + metaKvp.Value.name + "] in collection [" + Collection.name + "] cannot be added because the content tablename is empty.");
                            continue;
                        }
                        core.db.createSQLTable(metaKvp.Value.tableName);
                        foreach (KeyValuePair<string, ContentFieldMetadataModel> fieldKvp in metaKvp.Value.fields) {
                            if (string.IsNullOrWhiteSpace(fieldKvp.Value.nameLc)) {
                                LogController.logWarn(core, "Field [# " + fieldKvp.Value.id + "] in content [" + metaKvp.Value.name + "] in collection [" + Collection.name + "] cannot be added because the content tablename is empty.");
                                continue;
                            }
                            core.db.createSQLTableField(metaKvp.Value.tableName, fieldKvp.Value.nameLc, fieldKvp.Value.fieldTypeId);
                        }
                    }
                    core.clearMetaData();
                    core.cache.invalidateAll();
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 2: if baseCollection, reset isBaseContent and isBaseField");
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (isBaseCollection) {
                    core.db.executeNonQuery("update ccfields set isBaseField=0");
                    core.db.executeNonQuery("update ccContent set isBaseContent=0");
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 3: Verify all metadata names in ccContent so GetContentID calls will succeed");
                //----------------------------------------------------------------------------------------------------------------------
                //
                List<string> installedContentList = new List<string>();
                using (DataTable rs = core.db.executeQuery("SELECT Name from ccContent where (active<>0)")) {
                    if (DbController.isDataTableOk(rs)) {
                        installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
                    }
                }
                //
                foreach (var keypairvalue in Collection.metaData) {
                    if (keypairvalue.Value.dataChanged) {
                        LogController.logInfo(core, "adding metadata name [" + keypairvalue.Value.name + "]");
                        if (!installedContentList.Contains(keypairvalue.Value.name.ToLowerInvariant())) {
                            core.db.executeNonQuery("Insert into ccContent (name,ccguid,active,createkey)values(" + DbController.encodeSQLText(keypairvalue.Value.name) + "," + DbController.encodeSQLText(keypairvalue.Value.guid) + ",1,0);");
                            installedContentList.Add(keypairvalue.Value.name.ToLowerInvariant());
                        }
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 4: Verify content records required for Content Server");
                //----------------------------------------------------------------------------------------------------------------------
                //
                BuildController.verifySortMethods(core);
                BuildController.verifyContentFieldTypes(core);
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 5: verify 'Content' content definition");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.metaData) {
                    if (keypairvalue.Value.name.ToLowerInvariant() == "content") {
                        installMetaDataMiniCollection_buildDb_saveMetaDataToDb(core, keypairvalue.Value);
                        break;
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 6: Verify all definitions and fields");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.metaData) {
                    ContentMetadataModel metaData = keypairvalue.Value;
                    bool fieldChanged = false;
                    if (!metaData.dataChanged) {
                        foreach (var field in metaData.fields) {
                            fieldChanged = field.Value.dataChanged;
                            if (fieldChanged) break;
                        }
                    }
                    if ((fieldChanged || metaData.dataChanged) && (metaData.name.ToLowerInvariant() != "content")) {
                        installMetaDataMiniCollection_buildDb_saveMetaDataToDb(core, metaData);
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 7: Verify all field help");
                //----------------------------------------------------------------------------------------------------------------------
                //
                int FieldHelpCId = MetadataController.getRecordIdByUniqueName(core, "content", "Content Field Help");
                foreach (var keypairvalue in Collection.metaData) {
                    ContentMetadataModel workingMetaData = keypairvalue.Value;
                    foreach (var fieldKeyValuePair in workingMetaData.fields) {
                        ContentFieldMetadataModel workingField = fieldKeyValuePair.Value;
                        if (workingField.helpChanged) {
                            int fieldId = 0;
                            using (var rs = core.db.executeQuery("select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" + DbController.encodeSQLText(workingField.nameLc) + ")and(c.name=" + DbController.encodeSQLText(workingMetaData.name) + ") order by f.id")) {
                                if (DbController.isDataTableOk(rs)) {
                                    fieldId = GenericController.encodeInteger(DbController.getDataRowFieldText(rs.Rows[0], "id"));
                                }
                            }
                            if (fieldId == 0) {
                                LogController.logWarn(core, "Field help specified for a field that cannot be found, field [" + workingField.nameLc + "], content [" + workingMetaData.name + "]");
                            } else {
                                int FieldHelpId = 0;
                                using (var rs = core.db.executeQuery("select id from ccfieldhelp where fieldid=" + fieldId + " order by id")) {
                                    if (DbController.isDataTableOk(rs)) {
                                        FieldHelpId = GenericController.encodeInteger(rs.Rows[0]["id"]);
                                    } else {
                                        FieldHelpId = core.db.insertGetId("ccfieldhelp", 0);
                                    }
                                }
                                if (FieldHelpId != 0) {
                                    string Copy = workingField.helpCustom;
                                    if (string.IsNullOrEmpty(Copy)) { Copy = workingField.helpDefault; }
                                    core.db.executeNonQuery("update ccfieldhelp set active=1,contentcontrolid=" + FieldHelpCId + ",fieldid=" + fieldId + ",helpdefault=" + DbController.encodeSQLText(Copy) + " where id=" + FieldHelpId);
                                }
                            }
                        }
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 8: create SQL indexes");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (MetadataMiniCollectionModel.MiniCollectionSQLIndexModel index in Collection.sqlIndexes) {
                    if (index.dataChanged) {
                        using (var db = new DbController(core, index.DataSourceName)) {
                            LogController.logInfo(core, "creating index [" + index.IndexName + "], fields [" + index.FieldNameList + "], on table [" + index.TableName + "]");
                            db.createSQLIndex(index.TableName, index.IndexName, index.FieldNameList);
                        }
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 9: Verify All Menu Names, then all Menus");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var kvp in Collection.menus) {
                    var menu = kvp.Value;
                    if (menu.dataChanged) {
                        LogController.logInfo(core, "creating navigator entry [" + menu.name + "], namespace [" + menu.menuNameSpace + "], guid [" + menu.Guid + "]");
                        BuildController.verifyNavigatorEntry(core, menu, 0);
                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 9: Verify Styles");
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (Collection.styleCnt > 0) {
                    string SiteStyles = core.cdnFiles.readFileText("templates/styles.css");
                    string[] SiteStyleSplit = { };
                    int SiteStyleCnt = 0;
                    if (!string.IsNullOrEmpty(SiteStyles.Trim(' '))) {
                        //
                        // Split with an extra character at the end to guarantee there is an extra split at the end
                        //
                        SiteStyleSplit = (SiteStyles + " ").Split('}');
                        SiteStyleCnt = SiteStyleSplit.GetUpperBound(0) + 1;
                    }
                    string StyleSheetAdd = "";
                    for (var Ptr = 0; Ptr < Collection.styleCnt; Ptr++) {
                        bool Found = false;
                        var tempVar4 = Collection.styles[Ptr];
                        if (tempVar4.dataChanged) {
                            string NewStyleName = tempVar4.Name;
                            string NewStyleValue = tempVar4.Copy;
                            NewStyleValue = GenericController.strReplace(NewStyleValue, "}", "");
                            NewStyleValue = GenericController.strReplace(NewStyleValue, "{", "");
                            if (SiteStyleCnt > 0) {
                                int SiteStylePtr = 0;
                                for (SiteStylePtr = 0; SiteStylePtr < SiteStyleCnt; SiteStylePtr++) {
                                    string StyleLine = SiteStyleSplit[SiteStylePtr];
                                    int PosNameLineEnd = StyleLine.LastIndexOf("{") + 1;
                                    if (PosNameLineEnd > 0) {
                                        int PosNameLineStart = StyleLine.LastIndexOf(Environment.NewLine, PosNameLineEnd - 1) + 1;
                                        if (PosNameLineStart > 0) {
                                            //
                                            // Check this site style for a match with the NewStyleName
                                            //
                                            PosNameLineStart = PosNameLineStart + 2;
                                            string TestStyleName = (StyleLine.Substring(PosNameLineStart - 1, PosNameLineEnd - PosNameLineStart)).Trim(' ');
                                            if (GenericController.toLCase(TestStyleName) == GenericController.toLCase(NewStyleName)) {
                                                Found = true;
                                                if (tempVar4.Overwrite) {
                                                    //
                                                    // Found - Update style
                                                    //
                                                    SiteStyleSplit[SiteStylePtr] = Environment.NewLine + tempVar4.Name + " {" + NewStyleValue;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            //
                            // Add or update the stylesheet
                            //
                            if (!Found) {
                                StyleSheetAdd = StyleSheetAdd + Environment.NewLine + NewStyleName + " {" + NewStyleValue + "}";
                            }
                        }
                    }
                    SiteStyles = string.Join("}", SiteStyleSplit);
                    if (!string.IsNullOrEmpty(StyleSheetAdd)) {
                        SiteStyles = SiteStyles
                            + Environment.NewLine + "\r\n/*"
                            + Environment.NewLine + "Styles added " + DateTime.Now + Environment.NewLine + "*/"
                            + Environment.NewLine + StyleSheetAdd;
                    }
                    core.wwwFiles.saveFile("templates/styles.css", SiteStyles);
                    //
                    // -- Update stylesheet cache
                    core.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Overlay a Src metadata on to the current one (Dst). Any Src metadata entries found in Src are added to Dst.
        /// if SrcIsUsermetadata is true, then the Src is overlayed on the Dst if there are any changes -- and .metadataChanged flag set
        /// </summary>
        private static bool addMiniCollectionSrcToDst(CoreController core, ref MetadataMiniCollectionModel dstCollection, MetadataMiniCollectionModel srcCollection) {
            bool returnOk = true;
            try {
                string SrcFieldName = null;
                bool updateDst = false;
                ContentMetadataModel srcMetaData = null;
                //
                // If the Src is the BaseCollection, the Dst must be the Application Collectio
                //   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                //
                if (srcCollection.isBaseCollection) {
                    foreach (var dstKeyValuePair in dstCollection.metaData) {
                        Models.Domain.ContentMetadataModel dstWorkingMetaData = dstKeyValuePair.Value;
                        string contentName = dstWorkingMetaData.name;
                        if (dstCollection.metaData[contentName.ToLowerInvariant()].isBaseContent) {
                            //
                            // this application collection metadata is marked base, verify it is in the base collection
                            //
                            if (!srcCollection.metaData.ContainsKey(contentName.ToLowerInvariant())) {
                                //
                                // metadata in dst is marked base, but it is not in the src collection, reset the metadata.isBaseContent and all field.isbasefield
                                //
                                var tempVar = dstCollection.metaData[contentName.ToLowerInvariant()];
                                tempVar.isBaseContent = false;
                                tempVar.dataChanged = true;
                                foreach (var dstFieldKeyValuePair in tempVar.fields) {
                                    Models.Domain.ContentFieldMetadataModel field = dstFieldKeyValuePair.Value;
                                    if (field.isBaseField) {
                                        field.isBaseField = false;
                                    }
                                }
                            }
                        }
                    }
                }
                //
                //
                // -------------------------------------------------------------------------------------------------
                // Go through all CollectionSrc and find the CollectionDst match
                //   if it is an exact match, do nothing
                //   if the metadata does not match, set metadataext[Ptr].metadataChanged true
                //   if any field does not match, set metadataext...field...metadataChanged
                //   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                // -------------------------------------------------------------------------------------------------
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", Upgrademetadata_AddSrcToDst");
                string dstName = null;
                //
                foreach (var srcKeyValuePair in srcCollection.metaData) {
                    srcMetaData = srcKeyValuePair.Value;
                    string srcName = srcMetaData.name;
                    //
                    // Search for this metadata in the Dst
                    //
                    updateDst = false;
                    ContentMetadataModel dstMetaData = null;
                    if (!dstCollection.metaData.ContainsKey(srcName.ToLowerInvariant())) {
                        //
                        // add src to dst
                        //
                        dstMetaData = new Models.Domain.ContentMetadataModel();
                        dstCollection.metaData.Add(srcName.ToLowerInvariant(), dstMetaData);
                        updateDst = true;
                    } else {
                        dstMetaData = dstCollection.metaData[srcName.ToLowerInvariant()];
                        dstName = srcName;
                        //
                        // found a match between Src and Dst
                        //
                        if (dstMetaData.isBaseContent == srcMetaData.isBaseContent) {
                            //
                            // Allow changes to user metadata only from user metadata, changes to base only from base
                            updateDst |= (dstMetaData.activeOnly != srcMetaData.activeOnly);
                            updateDst |= (dstMetaData.adminOnly != srcMetaData.adminOnly);
                            updateDst |= (dstMetaData.developerOnly != srcMetaData.developerOnly);
                            updateDst |= (dstMetaData.allowAdd != srcMetaData.allowAdd);
                            updateDst |= (dstMetaData.allowCalendarEvents != srcMetaData.allowCalendarEvents);
                            updateDst |= (dstMetaData.allowContentTracking != srcMetaData.allowContentTracking);
                            updateDst |= (dstMetaData.allowDelete != srcMetaData.allowDelete);
                            updateDst |= (dstMetaData.allowTopicRules != srcMetaData.allowTopicRules);
                            updateDst |= !textMatch(dstMetaData.dataSourceName, srcMetaData.dataSourceName);
                            updateDst |= !textMatch(dstMetaData.tableName, srcMetaData.tableName);
                            updateDst |= !textMatch(dstMetaData.defaultSortMethod, srcMetaData.defaultSortMethod);
                            updateDst |= !textMatch(dstMetaData.dropDownFieldList, srcMetaData.dropDownFieldList);
                            updateDst |= !textMatch(dstMetaData.editorGroupName, srcMetaData.editorGroupName);
                            updateDst |= (dstMetaData.active != srcMetaData.active);
                            updateDst |= (dstMetaData.allowContentChildTool != srcMetaData.allowContentChildTool);
                            updateDst |= (dstMetaData.parentId != srcMetaData.parentId);
                            updateDst |= !textMatch(dstMetaData.iconLink, srcMetaData.iconLink);
                            updateDst |= (dstMetaData.iconHeight != srcMetaData.iconHeight);
                            updateDst |= (dstMetaData.iconWidth != srcMetaData.iconWidth);
                            updateDst |= (dstMetaData.iconSprites != srcMetaData.iconSprites);
                            updateDst |= !textMatch(dstMetaData.installedByCollectionGuid, srcMetaData.installedByCollectionGuid);
                            updateDst |= !textMatch(dstMetaData.guid, srcMetaData.guid);
                            updateDst |= (dstMetaData.isBaseContent != srcMetaData.isBaseContent);
                        }
                    }
                    if (updateDst) {
                        //
                        // update the Dst with the Src
                        dstMetaData.active = srcMetaData.active;
                        dstMetaData.activeOnly = srcMetaData.activeOnly;
                        dstMetaData.adminOnly = srcMetaData.adminOnly;
                        dstMetaData.aliasId = srcMetaData.aliasId;
                        dstMetaData.aliasName = srcMetaData.aliasName;
                        dstMetaData.allowAdd = srcMetaData.allowAdd;
                        dstMetaData.allowCalendarEvents = srcMetaData.allowCalendarEvents;
                        dstMetaData.allowContentChildTool = srcMetaData.allowContentChildTool;
                        dstMetaData.allowContentTracking = srcMetaData.allowContentTracking;
                        dstMetaData.allowDelete = srcMetaData.allowDelete;
                        dstMetaData.allowTopicRules = srcMetaData.allowTopicRules;
                        dstMetaData.guid = srcMetaData.guid;
                        dstMetaData.legacyContentControlCriteria = srcMetaData.legacyContentControlCriteria;
                        dstMetaData.dataSourceName = srcMetaData.dataSourceName;
                        dstMetaData.tableName = srcMetaData.tableName;
                        dstMetaData.dataSourceId = srcMetaData.dataSourceId;
                        dstMetaData.defaultSortMethod = srcMetaData.defaultSortMethod;
                        dstMetaData.developerOnly = srcMetaData.developerOnly;
                        dstMetaData.dropDownFieldList = srcMetaData.dropDownFieldList;
                        dstMetaData.editorGroupName = srcMetaData.editorGroupName;
                        dstMetaData.iconHeight = srcMetaData.iconHeight;
                        dstMetaData.iconLink = srcMetaData.iconLink;
                        dstMetaData.iconSprites = srcMetaData.iconSprites;
                        dstMetaData.iconWidth = srcMetaData.iconWidth;
                        dstMetaData.installedByCollectionGuid = srcMetaData.installedByCollectionGuid;
                        dstMetaData.isBaseContent = srcMetaData.isBaseContent;
                        dstMetaData.isModifiedSinceInstalled = srcMetaData.isModifiedSinceInstalled;
                        dstMetaData.name = srcMetaData.name;
                        dstMetaData.parentId = srcMetaData.parentId;
                        dstMetaData.parentName = srcMetaData.parentName;
                        dstMetaData.selectCommaList = srcMetaData.selectCommaList;
                        dstMetaData.whereClause = srcMetaData.whereClause;
                        dstMetaData.includesAFieldChange = true;
                        dstMetaData.dataChanged = true;
                    }
                    //
                    // Now check each of the field records for an addition, or a change
                    // DstPtr is still set to the Dst metadata
                    //
                    foreach (var srcFieldKeyValuePair in srcMetaData.fields) {
                        Models.Domain.ContentFieldMetadataModel srcMetaDataField = srcFieldKeyValuePair.Value;
                        SrcFieldName = srcMetaDataField.nameLc;
                        updateDst = false;
                        if (!dstCollection.metaData.ContainsKey(srcName.ToLowerInvariant())) {
                            //
                            // should have been the collection
                            //
                            throw (new GenericException("ERROR - cannot update destination content because it was not found after being added."));
                        } else {
                            dstMetaData = dstCollection.metaData[srcName.ToLowerInvariant()];
                            bool HelpChanged = false;
                            Models.Domain.ContentFieldMetadataModel dstMetaDataField = null;
                            if (dstMetaData.fields.ContainsKey(SrcFieldName.ToLowerInvariant())) {
                                //
                                // Src field was found in Dst fields
                                //
                                dstMetaDataField = dstMetaData.fields[SrcFieldName.ToLowerInvariant()];
                                updateDst = false;
                                if (dstMetaDataField.isBaseField == srcMetaDataField.isBaseField) {
                                    updateDst |= (srcMetaDataField.active != dstMetaDataField.active);
                                    updateDst |= (srcMetaDataField.adminOnly != dstMetaDataField.adminOnly);
                                    updateDst |= (srcMetaDataField.authorable != dstMetaDataField.authorable);
                                    updateDst |= !textMatch(srcMetaDataField.caption, dstMetaDataField.caption);
                                    updateDst |= (srcMetaDataField.contentId != dstMetaDataField.contentId);
                                    updateDst |= (srcMetaDataField.developerOnly != dstMetaDataField.developerOnly);
                                    updateDst |= (srcMetaDataField.editSortPriority != dstMetaDataField.editSortPriority);
                                    updateDst |= !textMatch(srcMetaDataField.editTabName, dstMetaDataField.editTabName);
                                    updateDst |= (srcMetaDataField.fieldTypeId != dstMetaDataField.fieldTypeId);
                                    updateDst |= (srcMetaDataField.htmlContent != dstMetaDataField.htmlContent);
                                    updateDst |= (srcMetaDataField.indexColumn != dstMetaDataField.indexColumn);
                                    updateDst |= (srcMetaDataField.indexSortDirection != dstMetaDataField.indexSortDirection);
                                    updateDst |= (encodeInteger(srcMetaDataField.indexSortOrder) != GenericController.encodeInteger(dstMetaDataField.indexSortOrder));
                                    updateDst |= !textMatch(srcMetaDataField.indexWidth, dstMetaDataField.indexWidth);
                                    updateDst |= (srcMetaDataField.lookupContentId != dstMetaDataField.lookupContentId);
                                    updateDst |= !textMatch(srcMetaDataField.lookupList, dstMetaDataField.lookupList);
                                    updateDst |= (srcMetaDataField.manyToManyContentId != dstMetaDataField.manyToManyContentId);
                                    updateDst |= (srcMetaDataField.manyToManyRuleContentId != dstMetaDataField.manyToManyRuleContentId);
                                    updateDst |= !textMatch(srcMetaDataField.manyToManyRulePrimaryField, dstMetaDataField.manyToManyRulePrimaryField);
                                    updateDst |= !textMatch(srcMetaDataField.manyToManyRuleSecondaryField, dstMetaDataField.manyToManyRuleSecondaryField);
                                    updateDst |= (srcMetaDataField.memberSelectGroupId_get(core) != dstMetaDataField.memberSelectGroupId_get(core));
                                    updateDst |= (srcMetaDataField.notEditable != dstMetaDataField.notEditable);
                                    updateDst |= (srcMetaDataField.password != dstMetaDataField.password);
                                    updateDst |= (srcMetaDataField.readOnly != dstMetaDataField.readOnly);
                                    updateDst |= (srcMetaDataField.redirectContentId != dstMetaDataField.redirectContentId);
                                    updateDst |= !textMatch(srcMetaDataField.redirectId, dstMetaDataField.redirectId);
                                    updateDst |= !textMatch(srcMetaDataField.redirectPath, dstMetaDataField.redirectPath);
                                    updateDst |= (srcMetaDataField.required != dstMetaDataField.required);
                                    updateDst |= (srcMetaDataField.rssDescriptionField != dstMetaDataField.rssDescriptionField);
                                    updateDst |= (srcMetaDataField.rssTitleField != dstMetaDataField.rssTitleField);
                                    updateDst |= (srcMetaDataField.scramble != dstMetaDataField.scramble);
                                    updateDst |= (srcMetaDataField.textBuffered != dstMetaDataField.textBuffered);
                                    updateDst |= (GenericController.encodeText(srcMetaDataField.defaultValue) != GenericController.encodeText(dstMetaDataField.defaultValue));
                                    updateDst |= (srcMetaDataField.uniqueName != dstMetaDataField.uniqueName);
                                    updateDst |= (srcMetaDataField.isBaseField != dstMetaDataField.isBaseField);
                                    updateDst |= !textMatch(srcMetaDataField.get_lookupContentName(core), dstMetaDataField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcMetaDataField.get_lookupContentName(core), dstMetaDataField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcMetaDataField.get_manyToManyRuleContentName(core), dstMetaDataField.get_manyToManyRuleContentName(core));
                                    updateDst |= !textMatch(srcMetaDataField.get_redirectContentName(core), dstMetaDataField.get_redirectContentName(core));
                                    updateDst |= !textMatch(srcMetaDataField.installedByCollectionGuid, dstMetaDataField.installedByCollectionGuid);
                                }
                                //
                                // Check Help fields, track changed independantly so frequent help changes will not force timely metadata loads
                                //
                                bool HelpCustomChanged = !textMatch(srcMetaDataField.helpCustom, dstMetaDataField.helpCustom);
                                bool HelpDefaultChanged = !textMatch(srcMetaDataField.helpDefault, dstMetaDataField.helpDefault);
                                HelpChanged = HelpDefaultChanged || HelpCustomChanged;
                            } else {
                                //
                                // field was not found in dst, add it and populate
                                //
                                dstMetaData.fields.Add(SrcFieldName.ToLowerInvariant(), new Models.Domain.ContentFieldMetadataModel());
                                dstMetaDataField = dstMetaData.fields[SrcFieldName.ToLowerInvariant()];
                                updateDst = true;
                                HelpChanged = true;
                            }
                            //
                            // If okToUpdateDstFromSrc, update the Dst record with the Src record
                            //
                            if (updateDst) {
                                //
                                // Update Fields
                                //
                                dstMetaDataField.active = srcMetaDataField.active;
                                dstMetaDataField.adminOnly = srcMetaDataField.adminOnly;
                                dstMetaDataField.authorable = srcMetaDataField.authorable;
                                dstMetaDataField.caption = srcMetaDataField.caption;
                                dstMetaDataField.contentId = srcMetaDataField.contentId;
                                dstMetaDataField.defaultValue = srcMetaDataField.defaultValue;
                                dstMetaDataField.developerOnly = srcMetaDataField.developerOnly;
                                dstMetaDataField.editSortPriority = srcMetaDataField.editSortPriority;
                                dstMetaDataField.editTabName = srcMetaDataField.editTabName;
                                dstMetaDataField.fieldTypeId = srcMetaDataField.fieldTypeId;
                                dstMetaDataField.htmlContent = srcMetaDataField.htmlContent;
                                dstMetaDataField.indexColumn = srcMetaDataField.indexColumn;
                                dstMetaDataField.indexSortDirection = srcMetaDataField.indexSortDirection;
                                dstMetaDataField.indexSortOrder = srcMetaDataField.indexSortOrder;
                                dstMetaDataField.indexWidth = srcMetaDataField.indexWidth;
                                dstMetaDataField.lookupContentId = srcMetaDataField.lookupContentId;
                                dstMetaDataField.lookupList = srcMetaDataField.lookupList;
                                dstMetaDataField.manyToManyContentId = srcMetaDataField.manyToManyContentId;
                                dstMetaDataField.manyToManyRuleContentId = srcMetaDataField.manyToManyRuleContentId;
                                dstMetaDataField.manyToManyRulePrimaryField = srcMetaDataField.manyToManyRulePrimaryField;
                                dstMetaDataField.manyToManyRuleSecondaryField = srcMetaDataField.manyToManyRuleSecondaryField;
                                dstMetaDataField.memberSelectGroupId_set(core, srcMetaDataField.memberSelectGroupId_get(core));
                                dstMetaDataField.nameLc = srcMetaDataField.nameLc;
                                dstMetaDataField.notEditable = srcMetaDataField.notEditable;
                                dstMetaDataField.password = srcMetaDataField.password;
                                dstMetaDataField.readOnly = srcMetaDataField.readOnly;
                                dstMetaDataField.redirectContentId = srcMetaDataField.redirectContentId;
                                dstMetaDataField.redirectId = srcMetaDataField.redirectId;
                                dstMetaDataField.redirectPath = srcMetaDataField.redirectPath;
                                dstMetaDataField.required = srcMetaDataField.required;
                                dstMetaDataField.rssDescriptionField = srcMetaDataField.rssDescriptionField;
                                dstMetaDataField.rssTitleField = srcMetaDataField.rssTitleField;
                                dstMetaDataField.scramble = srcMetaDataField.scramble;
                                dstMetaDataField.textBuffered = srcMetaDataField.textBuffered;
                                dstMetaDataField.uniqueName = srcMetaDataField.uniqueName;
                                dstMetaDataField.isBaseField = srcMetaDataField.isBaseField;
                                dstMetaDataField.set_lookupContentName(core, srcMetaDataField.get_lookupContentName(core));
                                dstMetaDataField.set_manyToManyContentName(core, srcMetaDataField.get_manyToManyContentName(core));
                                dstMetaDataField.set_manyToManyRuleContentName(core, srcMetaDataField.get_manyToManyRuleContentName(core));
                                dstMetaDataField.set_redirectContentName(core, srcMetaDataField.get_redirectContentName(core));
                                dstMetaDataField.installedByCollectionGuid = srcMetaDataField.installedByCollectionGuid;
                                dstMetaDataField.dataChanged = true;
                                dstMetaData.includesAFieldChange = true;
                            }
                            if (HelpChanged) {
                                dstMetaDataField.helpCustom = srcMetaDataField.helpCustom;
                                dstMetaDataField.helpDefault = srcMetaDataField.helpDefault;
                                dstMetaDataField.helpChanged = true;
                            }
                        }
                    }
                }
                //
                // -------------------------------------------------------------------------------------------------
                // Check SQL Indexes
                // -------------------------------------------------------------------------------------------------
                //
                foreach (MetadataMiniCollectionModel.MiniCollectionSQLIndexModel srcSqlIndex in srcCollection.sqlIndexes) {
                    string srcName = (srcSqlIndex.DataSourceName + "-" + srcSqlIndex.TableName + "-" + srcSqlIndex.IndexName).ToLowerInvariant();
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    bool indexFound = false;
                    bool indexChanged = false;
                    MetadataMiniCollectionModel.MiniCollectionSQLIndexModel indexToUpdate = new MetadataMiniCollectionModel.MiniCollectionSQLIndexModel();
                    foreach (MetadataMiniCollectionModel.MiniCollectionSQLIndexModel dstSqlIndex in dstCollection.sqlIndexes) {
                        dstName = (dstSqlIndex.DataSourceName + "-" + dstSqlIndex.TableName + "-" + dstSqlIndex.IndexName).ToLowerInvariant();
                        if (textMatch(dstName, srcName)) {
                            //
                            // found a match between Src and Dst
                            indexFound = true;
                            indexToUpdate = dstSqlIndex;
                            indexChanged = !textMatch(dstSqlIndex.FieldNameList, srcSqlIndex.FieldNameList);
                            break;
                        }
                    }
                    if (!indexFound) {
                        //
                        // add src to dst
                        dstCollection.sqlIndexes.Add(srcSqlIndex);
                    } else if (indexChanged && (indexToUpdate != null)) {
                        //
                        // update dst to src

                        indexToUpdate.dataChanged = true;
                        indexToUpdate.DataSourceName = srcSqlIndex.DataSourceName;
                        indexToUpdate.FieldNameList = srcSqlIndex.FieldNameList;
                        indexToUpdate.IndexName = srcSqlIndex.IndexName;
                        indexToUpdate.TableName = srcSqlIndex.TableName;
                    }
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Check menus
                //-------------------------------------------------------------------------------------------------
                //
                string DataBuildVersion = core.siteProperties.dataBuildVersion;
                foreach (var srcKvp in srcCollection.menus) {
                    string srcKey = srcKvp.Key.ToLowerInvariant();
                    MetadataMiniCollectionModel.MiniCollectionMenuModel srcMenu = srcKvp.Value;
                    string srcName = srcMenu.name.ToLowerInvariant();
                    string srcGuid = srcMenu.Guid;
                    string SrcParentName = GenericController.toLCase(srcMenu.ParentName);
                    string SrcNameSpace = GenericController.toLCase(srcMenu.menuNameSpace);
                    bool SrcIsNavigator = srcMenu.IsNavigator;
                    updateDst = false;
                    //
                    // Search for match using guid
                    MetadataMiniCollectionModel.MiniCollectionMenuModel dstMenuMatch = new MetadataMiniCollectionModel.MiniCollectionMenuModel();
                    bool IsMatch = false;
                    string DstKey = null;
                    bool DstIsNavigator = false;
                    foreach (var dstKvp in dstCollection.menus) {
                        string dstKey = dstKvp.Key.ToLowerInvariant();
                        MetadataMiniCollectionModel.MiniCollectionMenuModel dstMenu = dstKvp.Value;
                        string dstGuid = dstMenu.Guid;
                        if (dstGuid == srcGuid) {
                            DstIsNavigator = dstMenu.IsNavigator;
                            DstKey = GenericController.toLCase(dstMenu.Key);
                            string SrcKey = null;
                            IsMatch = (DstKey == SrcKey) && (SrcIsNavigator == DstIsNavigator);
                            if (IsMatch) {
                                dstMenuMatch = dstMenu;
                                break;
                            }

                        }
                    }
                    if (!IsMatch) {
                        //
                        // no match found on guid, try name and ( either namespace or parentname )
                        foreach (var dstKvp in dstCollection.menus) {
                            string dstKey = dstKvp.Key.ToLowerInvariant();
                            MetadataMiniCollectionModel.MiniCollectionMenuModel dstMenu = dstKvp.Value;
                            dstName = GenericController.toLCase(dstMenu.name);
                            if ((srcName == dstName) && (SrcIsNavigator == DstIsNavigator)) {
                                if (SrcIsNavigator) {
                                    //
                                    // Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    IsMatch = (SrcNameSpace == GenericController.toLCase(dstMenu.menuNameSpace)) && (dstMenu.Guid == "");
                                } else {
                                    //
                                    // AdminMenu - check parentname
                                    IsMatch = (SrcParentName == GenericController.toLCase(dstMenu.ParentName));
                                }
                                if (IsMatch) {
                                    dstMenuMatch = dstMenu;
                                    break;
                                }
                            }
                        }
                    }
                    if (IsMatch) {
                        updateDst |= (dstMenuMatch.Active != srcMenu.Active);
                        updateDst |= (dstMenuMatch.AdminOnly != srcMenu.AdminOnly);
                        updateDst |= !textMatch(dstMenuMatch.ContentName, srcMenu.ContentName);
                        updateDst |= (dstMenuMatch.DeveloperOnly != srcMenu.DeveloperOnly);
                        updateDst |= !textMatch(dstMenuMatch.LinkPage, srcMenu.LinkPage);
                        updateDst |= !textMatch(dstMenuMatch.name, srcMenu.name);
                        updateDst |= (dstMenuMatch.NewWindow != srcMenu.NewWindow);
                        updateDst |= !textMatch(dstMenuMatch.SortOrder, srcMenu.SortOrder);
                        updateDst |= !textMatch(dstMenuMatch.AddonName, srcMenu.AddonName);
                        updateDst |= !textMatch(dstMenuMatch.AddonGuid, srcMenu.AddonGuid);
                        updateDst |= !textMatch(dstMenuMatch.NavIconType, srcMenu.NavIconType);
                        updateDst |= !textMatch(dstMenuMatch.NavIconTitle, srcMenu.NavIconTitle);
                        updateDst |= !textMatch(dstMenuMatch.menuNameSpace, srcMenu.menuNameSpace);
                        updateDst |= !textMatch(dstMenuMatch.Guid, srcMenu.Guid);
                        dstCollection.menus.Remove(DstKey);
                    }
                    dstCollection.menus.Add(srcKey, srcMenu);
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Check styles
                //-------------------------------------------------------------------------------------------------
                //
                int srcStylePtr = 0;
                int dstStylePtr = 0;
                for (srcStylePtr = 0; srcStylePtr < srcCollection.styleCnt; srcStylePtr++) {
                    string srcName = GenericController.toLCase(srcCollection.styles[srcStylePtr].Name);
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    //
                    for (dstStylePtr = 0; dstStylePtr < dstCollection.styleCnt; dstStylePtr++) {
                        dstName = GenericController.toLCase(dstCollection.styles[dstStylePtr].Name);
                        if (dstName == srcName) {
                            //
                            // found a match between Src and Dst
                            updateDst |= !textMatch(dstCollection.styles[dstStylePtr].Copy, srcCollection.styles[srcStylePtr].Copy);
                            break;
                        }
                    }
                    if (dstStylePtr == dstCollection.styleCnt) {
                        //
                        // metadata was not found, add it
                        //
                        Array.Resize(ref dstCollection.styles, dstCollection.styleCnt);
                        dstCollection.styleCnt = dstStylePtr + 1;
                        updateDst = true;
                    }
                    if (updateDst) {
                        var tempVar6 = dstCollection.styles[dstStylePtr];
                        //
                        // It okToUpdateDstFromSrc, update the Dst with the Src
                        //
                        tempVar6.dataChanged = true;
                        tempVar6.Copy = srcCollection.styles[srcStylePtr].Copy;
                        tempVar6.Name = srcCollection.styles[srcStylePtr].Name;
                    }
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Add Collections
                //-------------------------------------------------------------------------------------------------
                //
                //
                //-------------------------------------------------------------------------------------------------
                // Page Templates
                //-------------------------------------------------------------------------------------------------
                //
                //
                //-------------------------------------------------------------------------------------------------
                // Page Content
                //-------------------------------------------------------------------------------------------------
                //
                //
                //-------------------------------------------------------------------------------------------------
                // Copy Content
                //-------------------------------------------------------------------------------------------------
                //
                //
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        //
        private static MetadataMiniCollectionModel getApplicationMetaDataMiniCollection(CoreController core, bool isNewBuild, string logPrefix) {
            var result = new MetadataMiniCollectionModel();
            try {
                if (!isNewBuild) {
                    //
                    // if this is not an empty database, get the application collection, else return empty
                    string applicationMetaDataMiniCollectionXml = CollectionExportCDefController.get(core, true);
                    result = MetadataMiniCollectionModel.loadXML(core, applicationMetaDataMiniCollectionXml, false, false, isNewBuild, logPrefix);
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Update a table from a collection metadata node
        /// </summary>
        internal static void installMetaDataMiniCollection_buildDb_saveMetaDataToDb(CoreController core, ContentMetadataModel contentMetadata) {
            try {
                //
                LogController.logInfo(core, "Update db metadata [" + contentMetadata.name + "]");
                //
                // -- get contentid and protect content with IsBaseContent true
                {
                    if (contentMetadata.dataChanged) {
                        //
                        // -- update definition (use SingleRecord as an update flag)
                        var datasource = DataSourceModel.createByUniqueName(core.cpParent, contentMetadata.dataSourceName);
                        ContentMetadataModel.verifyContent_returnId(core, contentMetadata);
                    }
                    //
                    // -- update Content Field Records and Content Field Help records
                    ContentMetadataModel metaDataFieldHelp = ContentMetadataModel.createByUniqueName(core, ContentFieldHelpModel.tableMetadata.contentName);
                    foreach (var nameValuePair in contentMetadata.fields) {
                        ContentFieldMetadataModel fieldMetadata = nameValuePair.Value;
                        if (fieldMetadata.dataChanged) {
                            contentMetadata.verifyContentField(core, fieldMetadata, false);
                        }
                        //
                        // -- update content field help records
                        if (fieldMetadata.helpChanged) {
                            ContentFieldHelpModel fieldHelp = null;
                            var fieldHelpList = DbBaseModel.createList<ContentFieldHelpModel>(core.cpParent, "fieldid=" + fieldMetadata.id);
                            if (fieldHelpList.Count == 0) {
                                //
                                // -- no current field help record, if adding help, create record
                                if ((!string.IsNullOrWhiteSpace(fieldMetadata.helpDefault)) || (!string.IsNullOrWhiteSpace(fieldMetadata.helpCustom))) {
                                    fieldHelp = DbBaseModel.addEmpty<ContentFieldHelpModel>(core.cpParent);
                                    fieldHelp.helpDefault = fieldMetadata.helpDefault;
                                    fieldHelp.helpCustom = fieldMetadata.helpCustom;
                                    fieldHelp.save(core.cpParent);
                                }
                            } else {
                                //
                                // -- if help changed, save it
                                fieldHelp = fieldHelpList.First();
                                if ((!fieldHelp.helpCustom.Equals(fieldMetadata.helpCustom)) || !fieldHelp.helpDefault.Equals(fieldMetadata.helpDefault)) {
                                    fieldHelp.helpDefault = fieldMetadata.helpDefault;
                                    fieldHelp.helpCustom = fieldMetadata.helpCustom;
                                    fieldHelp.save(core.cpParent);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private static string GetMenuNameSpace(CoreController core, Dictionary<string, MetadataMiniCollectionModel.MiniCollectionMenuModel> menus, MetadataMiniCollectionModel.MiniCollectionMenuModel menu, string UsedIDList) {
            string returnAttr = "";
            try {
                string ParentName = null;
                int Ptr = 0;
                string Prefix = null;
                string LCaseParentName = null;

                //
                ParentName = menu.ParentName;
                if (!string.IsNullOrEmpty(ParentName)) {
                    LCaseParentName = GenericController.toLCase(ParentName);
                    foreach (var kvp in menus) {
                        MetadataMiniCollectionModel.MiniCollectionMenuModel testMenu = kvp.Value;
                        if (GenericController.strInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0) {
                            if (LCaseParentName == GenericController.toLCase(testMenu.name) && (menu.IsNavigator == testMenu.IsNavigator)) {
                                Prefix = GetMenuNameSpace(core, menus, testMenu, UsedIDList + "," + menu.Guid);
                                if (string.IsNullOrEmpty(Prefix)) {
                                    returnAttr = ParentName;
                                } else {
                                    returnAttr = Prefix + "." + ParentName;
                                }
                                break;
                            }
                        }

                    }
                }
            } catch (Exception ex) {
                LogController.logError(core, ex);
                throw;
            }
            return returnAttr;
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
