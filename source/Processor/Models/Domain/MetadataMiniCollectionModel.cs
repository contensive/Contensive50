
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
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.Processor.Exceptions;
//
namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    /// <summary>
    /// miniCollection - This is an old collection object used in part to load the metadata part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    /// </summary>
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
        ////
        //internal static void installmetadataMiniCollectionFromXml_2(CoreController core, string srcXml, bool isNewBuild, bool isBaseCollection, bool isRepairMode, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
        //    metadataMiniCollectionModel baseCollection = installmetadataMiniCollection_LoadXml(core, srcXml, true, true, isNewBuild, new metadataMiniCollectionModel(), logPrefix, ref installedCollections);
        //    installmetadataMiniCollection_BuildDb(core, baseCollection, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix, ref installedCollections);

        //}
        //
        //======================================================================================================
        //
        internal static void installMetaDataMiniCollectionFromXml(bool quick, CoreController core, string srcXml, bool isNewBuild, bool isRepairMode, bool isBaseCollection, ref List<string> nonCriticalErrorList, string logPrefix) {
            try {
                if (quick) {
                    //
                    LogController.logInfo(core, "installmetadataMiniCollectionFromXML");
                    // 
                    // -- method 1, just install
                    MetadataMiniCollectionModel baseCollection = loadXML(core, srcXml, true, true, isNewBuild, new MetadataMiniCollectionModel(), logPrefix);
                    
                    installMetaDataMiniCollection_BuildDb(core, baseCollection, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix);
                } else {
                    //
                    // -- method 2, merge with application collection and install
                    //
                    MetadataMiniCollectionModel miniCollectionWorking = getApplicationMetaDataMiniCollection(core, isNewBuild, logPrefix);
                    MetadataMiniCollectionModel miniCollectionToAdd = loadXML(core, srcXml, isBaseCollection, false, isNewBuild, miniCollectionWorking, logPrefix);
                    addMiniCollectionSrcToDst(core, ref miniCollectionWorking, miniCollectionToAdd);
                    installMetaDataMiniCollection_BuildDb(core, miniCollectionWorking, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// create a collection class from a collection xml file, metadata are added to the metadatas in the application collection
        /// </summary>
        public static MetadataMiniCollectionModel loadXML(CoreController core, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, MetadataMiniCollectionModel defaultCollection, string logPrefix) {
            MetadataMiniCollectionModel result = null;
            try {
                ContentMetadataModel DefaultMetaData = null;
                ContentFieldMetadataModel DefaultMetaDataField = null;
                string contentNameLc = null;
                string Collectionname = null;
                string ContentTableName = null;
                bool IsNavigator = false;
                string ActiveText = null;
                string Name = "";
                string MenuName = null;
                string IndexName = null;
                string TableName = null;
                int Ptr = 0;
                string FieldName = null;
                string ContentName = null;
                bool Found = false;
                string menuNameSpace = null;
                string MenuGuid = null;

                XmlNode metaData_Node = null;
                string DataSourceName = null;
                XmlDocument srcXmlDom = new XmlDocument();
                string NodeName = null;
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", Upgrademetadata_LoadDataToCollection");
                //
                result = new MetadataMiniCollectionModel();
                //
                if (string.IsNullOrEmpty(srcCollecionXml)) {
                    //
                    // -- empty collection is an error
                    throw (new GenericException("Upgrademetadata_LoadDataToCollection, srcCollectionXml is blank or null"));
                } else {
                    try {
                        srcXmlDom.LoadXml(srcCollecionXml);
                    } catch (Exception ex) {
                        //
                        // -- xml load error
                        LogController.logError(core, "Upgrademetadata_LoadDataToCollection Error reading xml archive, ex=[" + ex.ToString() + "]");
                        throw new Exception("Error in Upgrademetadata_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    if ((srcXmlDom.DocumentElement.Name.ToLowerInvariant() != CollectionFileRootNode) && (srcXmlDom.DocumentElement.Name.ToLowerInvariant() != "contensivecdef")) {
                        //
                        // -- root node must be collection (or legacy contensivemetadata)
                        LogController.handleError(core, new GenericException("the archive file has a syntax error. Application name must be the first node."));
                    } else {
                        result.isBaseCollection = IsccBaseFile;
                        //
                        // Get Collection Name for logs
                        //
                        //hint = "get collection name"
                        Collectionname = XmlController.GetXMLAttribute(core, Found, srcXmlDom.DocumentElement, "name", "");
                        if (string.IsNullOrEmpty(Collectionname)) {
                            LogController.logInfo(core, "Upgrademetadata_LoadDataToCollection, Application: " + core.appConfig.name + ", Collection has no name");
                        } else {
                            //Call AppendClassLogFile(core.app.config.name,"Upgrademetadata_LoadDataToCollection", "Upgrademetadata_LoadDataToCollection, Application: " & core.app.appEnvironment.name & ", Collection: " & Collectionname)
                        }
                        result.name = Collectionname;
                        //
                        // Load possible DefaultSortMethods
                        //
                        //hint = "preload sort methods"
                        //SortMethodList = vbTab & "By Name" & vbTab & "By Alpha Sort Order Field" & vbTab & "By Date" & vbTab & "By Date Reverse"
                        //If core.app.csv_IsContentFieldSupported("Sort Methods", "ID") Then
                        //    CS = core.app.OpenCSContent("Sort Methods", , , , , , , "Name")
                        //    Do While core.app.IsCSOK(CS)
                        //        SortMethodList = SortMethodList & vbTab & core.app.cs_getText(CS, "name")
                        //        core.app.nextCSRecord(CS)
                        //    Loop
                        //    Call core.app.closeCS(CS)
                        //End If
                        //SortMethodList = SortMethodList & vbTab
                        //
                        foreach (XmlNode metaData_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes) {
                            metaData_Node = metaData_NodeWithinLoop;
                            NodeName = GenericController.vbLCase(metaData_NodeWithinLoop.Name);
                            switch (NodeName) {
                                case "cdef":
                                    //
                                    // Content Definitions
                                    //
                                    ContentName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "name", "");
                                    contentNameLc = GenericController.vbLCase(ContentName);
                                    if (string.IsNullOrEmpty(ContentName)) {
                                        throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "collection file contains a metadata node with no name attribute. This is not allowed.", "dll", "builderClass", "Upgrademetadata_LoadDataToCollection", 0, "", "", False, True, "")
                                    } else {
                                        //
                                        // setup a metadata from the application collection to use as a default for missing attributes (for inherited metadata)
                                        //
                                        if (defaultCollection.metaData.ContainsKey(contentNameLc)) {
                                            DefaultMetaData = defaultCollection.metaData[contentNameLc];
                                        } else {
                                            DefaultMetaData = new Models.Domain.ContentMetadataModel() {
                                                active = true,
                                                activeOnly = true,
                                                aliasID = "id",
                                                aliasName = "name",
                                                adminOnly = false,
                                                allowAdd = true,
                                                allowDelete = false,
                                                dataSourceName = "",
                                                dataSourceId = 0,
                                                developerOnly = false,
                                                dropDownFieldList = "name",
                                                isBaseContent = IsccBaseFile
                                            };
                                        }
                                        //
                                        ContentTableName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentTableName", DefaultMetaData.tableName);
                                        if (!string.IsNullOrEmpty(ContentTableName)) {
                                            //
                                            // These two fields are needed to import the row
                                            //
                                            DataSourceName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "dataSource", DefaultMetaData.dataSourceName);
                                            if (string.IsNullOrEmpty(DataSourceName)) {
                                                DataSourceName = "Default";
                                            }
                                            //
                                            // ----- Add metadata if not already there
                                            //
                                            if (!result.metaData.ContainsKey(ContentName.ToLowerInvariant())) {
                                                result.metaData.Add(ContentName.ToLowerInvariant(), new Models.Domain.ContentMetadataModel());
                                            }
                                            //
                                            // Get metadata attributes
                                            //
                                            Models.Domain.ContentMetadataModel targetMetaData = result.metaData[ContentName.ToLowerInvariant()];
                                            string activeDefaultText = "1";
                                            if (!(DefaultMetaData.active)) {
                                                activeDefaultText = "0";
                                            }
                                            ActiveText = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Active", activeDefaultText);
                                            if (string.IsNullOrEmpty(ActiveText)) {
                                                ActiveText = "1";
                                            }
                                            targetMetaData.active = GenericController.encodeBoolean(ActiveText);
                                            targetMetaData.activeOnly = true;
                                            //.adminColumns = ?
                                            targetMetaData.adminOnly = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AdminOnly", DefaultMetaData.adminOnly);
                                            targetMetaData.aliasID = "id";
                                            targetMetaData.aliasName = "name";
                                            targetMetaData.allowAdd = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowAdd", DefaultMetaData.allowAdd);
                                            targetMetaData.allowCalendarEvents = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowCalendarEvents", DefaultMetaData.allowCalendarEvents);
                                            targetMetaData.allowContentChildTool = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowContentChildTool", DefaultMetaData.allowContentChildTool);
                                            targetMetaData.allowContentTracking = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowContentTracking", DefaultMetaData.allowContentTracking);
                                            targetMetaData.allowDelete = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowDelete", DefaultMetaData.allowDelete);
                                            targetMetaData.allowTopicRules = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AllowTopicRules", DefaultMetaData.allowTopicRules);
                                            targetMetaData.guid = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", DefaultMetaData.guid);
                                            targetMetaData.dataChanged = setAllDataChanged;
                                            targetMetaData.legacyContentControlCriteria = "";
                                            targetMetaData.dataSourceName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentDataSourceName", DefaultMetaData.dataSourceName);
                                            targetMetaData.tableName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentTableName", DefaultMetaData.tableName);
                                            targetMetaData.dataSourceId = 0;
                                            targetMetaData.defaultSortMethod = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "DefaultSortMethod", DefaultMetaData.defaultSortMethod);
                                            if ((targetMetaData.defaultSortMethod == "") || (targetMetaData.defaultSortMethod.ToLowerInvariant() == "name")) {
                                                targetMetaData.defaultSortMethod = "By Name";
                                            } else if (GenericController.vbLCase(targetMetaData.defaultSortMethod) == "sortorder") {
                                                targetMetaData.defaultSortMethod = "By Alpha Sort Order Field";
                                            } else if (GenericController.vbLCase(targetMetaData.defaultSortMethod) == "date") {
                                                targetMetaData.defaultSortMethod = "By Date";
                                            }
                                            targetMetaData.developerOnly = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "DeveloperOnly", DefaultMetaData.developerOnly);
                                            targetMetaData.dropDownFieldList = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "DropDownFieldList", DefaultMetaData.dropDownFieldList);
                                            targetMetaData.editorGroupName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "EditorGroupName", DefaultMetaData.editorGroupName);
                                            targetMetaData.fields = new Dictionary<string, Models.Domain.ContentFieldMetadataModel>();
                                            targetMetaData.iconLink = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "IconLink", DefaultMetaData.iconLink);
                                            targetMetaData.iconHeight = XmlController.GetXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconHeight", DefaultMetaData.iconHeight);
                                            targetMetaData.iconWidth = XmlController.GetXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconWidth", DefaultMetaData.iconWidth);
                                            targetMetaData.iconSprites = XmlController.GetXMLAttributeInteger(core, Found, metaData_NodeWithinLoop, "IconSprites", DefaultMetaData.iconSprites);
                                            targetMetaData.includesAFieldChange = false;
                                            targetMetaData.installedByCollectionGuid = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "installedByCollection", DefaultMetaData.installedByCollectionGuid);
                                            targetMetaData.isBaseContent = IsccBaseFile || XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "IsBaseContent", false);
                                            targetMetaData.isModifiedSinceInstalled = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "IsModified", DefaultMetaData.isModifiedSinceInstalled);
                                            targetMetaData.name = ContentName;
                                            targetMetaData.parentName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Parent", DefaultMetaData.parentName);
                                            targetMetaData.whereClause = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "WhereClause", DefaultMetaData.whereClause);
                                            //
                                            // -- determine id
                                            targetMetaData.id = DbController.getContentId(core, ContentName);
                                            //
                                            // Get metadata field nodes
                                            //
                                            foreach (XmlNode MetaDataChildNode in metaData_NodeWithinLoop.ChildNodes) {
                                                //
                                                // ----- process metadata Field
                                                //
                                                if (textMatch(MetaDataChildNode.Name, "field")) {
                                                    FieldName = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "Name", "");
                                                    if (FieldName.ToLowerInvariant() == "middlename") {
                                                        //FieldName = FieldName;
                                                    }
                                                    //
                                                    // try to find field in the defaultmetadata
                                                    //
                                                    if (DefaultMetaData.fields.ContainsKey(FieldName)) {
                                                        DefaultMetaDataField = DefaultMetaData.fields[FieldName];
                                                    } else {
                                                        DefaultMetaDataField = new Models.Domain.ContentFieldMetadataModel();
                                                    }
                                                    //
                                                    if (!(result.metaData[ContentName.ToLowerInvariant()].fields.ContainsKey(FieldName.ToLowerInvariant()))) {
                                                        result.metaData[ContentName.ToLowerInvariant()].fields.Add(FieldName.ToLowerInvariant(), new Models.Domain.ContentFieldMetadataModel());
                                                    }
                                                    var metaDataField = result.metaData[ContentName.ToLowerInvariant()].fields[FieldName.ToLowerInvariant()];
                                                    metaDataField.nameLc = FieldName.ToLowerInvariant();
                                                    ActiveText = "0";
                                                    if (DefaultMetaDataField.active) {
                                                        ActiveText = "1";
                                                    }
                                                    ActiveText = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "Active", ActiveText);
                                                    if (string.IsNullOrEmpty(ActiveText)) {
                                                        ActiveText = "1";
                                                    }
                                                    metaDataField.active = GenericController.encodeBoolean(ActiveText);
                                                    //
                                                    // Convert Field Descriptor (text) to field type (integer)
                                                    //
                                                    string defaultFieldTypeName = ContentFieldMetadataModel.getFieldTypeNameFromFieldTypeId(core, DefaultMetaDataField.fieldTypeId);
                                                    string fieldTypeName = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "FieldType", defaultFieldTypeName);
                                                    metaDataField.fieldTypeId = core.db.getFieldTypeIdFromFieldTypeName(fieldTypeName);
                                                    metaDataField.editSortPriority = XmlController.GetXMLAttributeInteger(core, Found, MetaDataChildNode, "EditSortPriority", DefaultMetaDataField.editSortPriority);
                                                    metaDataField.authorable = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "Authorable", DefaultMetaDataField.authorable);
                                                    metaDataField.caption = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "Caption", DefaultMetaDataField.caption);
                                                    metaDataField.defaultValue = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "DefaultValue", DefaultMetaDataField.defaultValue);
                                                    metaDataField.notEditable = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "NotEditable", DefaultMetaDataField.notEditable);
                                                    metaDataField.indexColumn = XmlController.GetXMLAttributeInteger(core, Found, MetaDataChildNode, "IndexColumn", DefaultMetaDataField.indexColumn);
                                                    metaDataField.indexWidth = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "IndexWidth", DefaultMetaDataField.indexWidth);
                                                    metaDataField.indexSortOrder = XmlController.GetXMLAttributeInteger(core, Found, MetaDataChildNode, "IndexSortOrder", DefaultMetaDataField.indexSortOrder);
                                                    metaDataField.redirectID = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "RedirectID", DefaultMetaDataField.redirectID);
                                                    metaDataField.redirectPath = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "RedirectPath", DefaultMetaDataField.redirectPath);
                                                    metaDataField.htmlContent = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "HTMLContent", DefaultMetaDataField.htmlContent);
                                                    metaDataField.uniqueName = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "UniqueName", DefaultMetaDataField.uniqueName);
                                                    metaDataField.password = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "Password", DefaultMetaDataField.password);
                                                    metaDataField.adminOnly = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "AdminOnly", DefaultMetaDataField.adminOnly);
                                                    metaDataField.developerOnly = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "DeveloperOnly", DefaultMetaDataField.developerOnly);
                                                    metaDataField.readOnly = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "ReadOnly", DefaultMetaDataField.readOnly);
                                                    metaDataField.required = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "Required", DefaultMetaDataField.required);
                                                    metaDataField.rssTitleField = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "RSSTitle", DefaultMetaDataField.rssTitleField);
                                                    metaDataField.rssDescriptionField = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "RSSDescriptionField", DefaultMetaDataField.rssDescriptionField);
                                                    metaDataField.memberSelectGroupName_set(core, XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "MemberSelectGroup", ""));
                                                    metaDataField.editTabName = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "EditTab", DefaultMetaDataField.editTabName);
                                                    metaDataField.scramble = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "Scramble", DefaultMetaDataField.scramble);
                                                    metaDataField.lookupList = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "LookupList", DefaultMetaDataField.lookupList);
                                                    metaDataField.manyToManyRulePrimaryField = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRulePrimaryField", DefaultMetaDataField.manyToManyRulePrimaryField);
                                                    metaDataField.manyToManyRuleSecondaryField = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRuleSecondaryField", DefaultMetaDataField.manyToManyRuleSecondaryField);
                                                    metaDataField.set_lookupContentName(core, XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "LookupContent", DefaultMetaDataField.get_lookupContentName(core)));
                                                    // isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                    // case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                    // case 2 - when the base file is loaded, all fields must include the attribute
                                                    metaDataField.isBaseField = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "IsBaseField", false) || IsccBaseFile;
                                                    metaDataField.set_redirectContentName(core, XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "RedirectContent", DefaultMetaDataField.get_redirectContentName(core)));
                                                    metaDataField.set_manyToManyContentName(core, XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyContent", DefaultMetaDataField.get_manyToManyContentName(core)));
                                                    metaDataField.set_manyToManyRuleContentName(core, XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "ManyToManyRuleContent", DefaultMetaDataField.get_manyToManyRuleContentName(core)));
                                                    metaDataField.isModifiedSinceInstalled = XmlController.GetXMLAttributeBoolean(core, Found, MetaDataChildNode, "IsModified", DefaultMetaDataField.isModifiedSinceInstalled);
                                                    metaDataField.installedByCollectionGuid = XmlController.GetXMLAttribute(core, Found, MetaDataChildNode, "installedByCollectionId", DefaultMetaDataField.installedByCollectionGuid);
                                                    metaDataField.id = DbController.getContentFieldId( core, targetMetaData.id, metaDataField.nameLc);
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
                                        }
                                    }
                                    break;
                                case "sqlindex":
                                    //
                                    // SQL Indexes
                                    //
                                    IndexName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "indexname", "");
                                    TableName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "tableName", "");
                                    DataSourceName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "DataSourceName", "");
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
                                        FieldNameList = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "FieldNameList", "")
                                    };
                                    result.sqlIndexes.Add(newIndex);
                                    break;
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry":
                                    //
                                    // Admin Menus / Navigator Entries
                                    MenuName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Name", "");
                                    menuNameSpace = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "NameSpace", "");
                                    MenuGuid = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", "");
                                    IsNavigator = (NodeName == "navigatorentry");
                                    string MenuKey = null;
                                    if (!IsNavigator) {
                                        MenuKey = GenericController.vbLCase(MenuName);
                                    } else {
                                        MenuKey = MenuGuid;
                                    }
                                    if (!result.menus.ContainsKey(MenuKey)) {
                                        ActiveText = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Active", "1");
                                        if (string.IsNullOrEmpty(ActiveText)) {
                                            ActiveText = "1";
                                        }
                                        result.menus.Add(MenuKey, new MetadataMiniCollectionModel.MiniCollectionMenuModel() {
                                            dataChanged = setAllDataChanged,
                                            name = MenuName,
                                            Guid = MenuGuid,
                                            Key = MenuKey,
                                            Active = GenericController.encodeBoolean(ActiveText),
                                            menuNameSpace = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "NameSpace", ""),
                                            ParentName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "ParentName", ""),
                                            ContentName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "ContentName", ""),
                                            LinkPage = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "LinkPage", ""),
                                            SortOrder = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "SortOrder", ""),
                                            AdminOnly = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "AdminOnly", false),
                                            DeveloperOnly = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "DeveloperOnly", false),
                                            NewWindow = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "NewWindow", false),
                                            AddonName = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "AddonName", ""),
                                            AddonGuid = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "AddonGuid", ""),
                                            NavIconType = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "NavIconType", ""),
                                            NavIconTitle = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "NavIconTitle", ""),
                                            IsNavigator = IsNavigator
                                        });
                                    }
                                    break;
                                case "aggregatefunction":
                                case "addon":
                                    //
                                    // 20181026 - no longer needed -- Aggregate Objects (just make them -- there are not too many
                                    //
                                    //Name =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "Name", "");
                                    //MiniCollectionModel.miniCollectionAddOnModel addon;
                                    //if (result.legacyAddOns.ContainsKey(Name.ToLowerInvariant())) {
                                    //    addon = result.legacyAddOns[Name.ToLowerInvariant()];
                                    //} else {
                                    //    addon = new MiniCollectionModel.miniCollectionAddOnModel();
                                    //    result.legacyAddOns.Add(Name.ToLowerInvariant(), addon);
                                    //}
                                    //addon.dataChanged = setAllDataChanged;
                                    //addon.Link =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "Link", "");
                                    //addon.ObjectProgramID =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "ObjectProgramID", "");
                                    //addon.ArgumentList =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "ArgumentList", "");
                                    //addon.SortOrder =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "SortOrder", "");
                                    //addon.Copy =XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "copy", "");
                                    break;
                                case "style":
                                    //
                                    // style sheet entries
                                    //
                                    Name = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Name", "");
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
                                    tempVar5.Overwrite = XmlController.GetXMLAttributeBoolean(core, Found, metaData_NodeWithinLoop, "Overwrite", false);
                                    tempVar5.Copy = metaData_NodeWithinLoop.InnerText;
                                    break;
                                case "stylesheet":
                                    //
                                    // style sheet in one entry
                                    //
                                    result.styleSheet = metaData_NodeWithinLoop.InnerText;
                                    break;
                                case "getcollection":
                                case "importcollection":
                                    //if (true) {
                                    //    //If Not UpgradeDbOnly Then
                                    //    //
                                    //    // Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                    //    //
                                    //    Collectionname = XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "name", "");
                                    //    CollectionGuid = XmlController.GetXMLAttribute(core, Found, metadata_NodeWithinLoop, "guid", "");
                                    //    if (string.IsNullOrEmpty(CollectionGuid)) {
                                    //        CollectionGuid = metadata_NodeWithinLoop.InnerText;
                                    //    }
                                    //    if (string.IsNullOrEmpty(CollectionGuid)) {
                                    //        status = "The collection you selected [" + Collectionname + "] can not be downloaded because it does not include a valid GUID.";
                                    //        //core.AppendLog("builderClass.Upgrademetadata_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
                                    //    } else {
                                    //        result.collectionImports.Add(new metadataMiniCollectionModel.ImportCollectionType() {
                                    //            Name = Collectionname,
                                    //            Guid = CollectionGuid
                                    //        });
                                    //    }
                                    //}
                                    break;
                                case "pagetemplate":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Page Templates
                                    //-------------------------------------------------------------------------------------------------
                                    // *********************************************************************************
                                    // Page Template - started, but Return_Collection and LoadDataTometadata are all that is done do far
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
                                    tempVar6.Copy = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "Copy", "");
                                    tempVar6.Guid = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "guid", "");
                                    tempVar6.Style = XmlController.GetXMLAttribute(core, Found, metaData_NodeWithinLoop, "style", "");
                                    break;
                                case "pagecontent":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Page Content
                                    //-------------------------------------------------------------------------------------------------
                                    //
                                    break;
                                case "copycontent":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Copy Content
                                    //-------------------------------------------------------------------------------------------------
                                    //
                                    break;
                            }
                        }
                        //hint = "nodes done"
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
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Verify ccContent and ccFields records from the metadata nodes of a a collection file. This is the last step of loading teh metadata nodes of a collection file. ParentId field is set based on ParentName node.
        /// </summary>
        private static void installMetaDataMiniCollection_BuildDb(CoreController core, MetadataMiniCollectionModel Collection, string BuildVersion, bool isNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix) {
            try {
                //
                logPrefix += ", installCollection_BuildDbFromMiniCollection";
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", Upgrademetadata_BuildDbFromCollection");
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 1: verify core sql tables");
                //----------------------------------------------------------------------------------------------------------------------
                //
                NewAppController.verifyBasicTables(core, logPrefix);
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 2: create SQL tables in default datasource");
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (true) {
                    string UsedTables = "";
                    foreach (var keypairvalue in Collection.metaData) {
                        ContentMetadataModel contentMetaData = keypairvalue.Value;
                        if (contentMetaData.dataChanged) {
                            LogController.logInfo(core, "creating sql table [" + contentMetaData.tableName + "], datasource [" + contentMetaData.dataSourceName + "]");
                            using (var db = new DbController(core, contentMetaData.dataSourceName)) {
                                if (GenericController.vbLCase(contentMetaData.dataSourceName) == "default" || contentMetaData.dataSourceName == "") {
                                    if (GenericController.vbInstr(1, "," + UsedTables + ",", "," + contentMetaData.tableName + ",", 1) != 0) {
                                        //TableName = TableName;
                                    } else {
                                        UsedTables = UsedTables + "," + contentMetaData.tableName;
                                        db.createSQLTable(contentMetaData.tableName);
                                    }
                                    foreach (var fieldNvp in contentMetaData.fields) {
                                        ContentFieldMetadataModel field = fieldNvp.Value;
                                        db.createSQLTableField(contentMetaData.tableName, field.nameLc, field.fieldTypeId);
                                    }
                                }
                            }
                        }
                    }
                    core.clearMetaData();
                    core.cache.invalidateAll();
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
                            core.db.executeQuery("Insert into ccContent (name,ccguid,active,createkey)values(" + DbController.encodeSQLText(keypairvalue.Value.name) + "," + DbController.encodeSQLText(keypairvalue.Value.guid) + ",1,0);");
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
                NewAppController.verifySortMethods(core);
                NewAppController.verifyContentFieldTypes(core);
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 5: verify 'Content' content definition");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.metaData) {
                    if (keypairvalue.Value.name.ToLowerInvariant() == "content") {
                        installMetaDataMiniCollection_buildDb_saveMetaDataToDb(core, keypairvalue.Value, BuildVersion);
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
                        installMetaDataMiniCollection_buildDb_saveMetaDataToDb(core, metaData, BuildVersion);
                    }
                }
                core.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, stage 7: Verify all field help");
                //----------------------------------------------------------------------------------------------------------------------
                //
                int FieldHelpCID = MetadataController.getRecordIdByUniqueName(core, "content", "Content Field Help");
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
                                int FieldHelpID = 0;
                                using (var rs = core.db.executeQuery("select id from ccfieldhelp where fieldid=" + fieldId + " order by id")) {
                                    if (DbController.isDataTableOk(rs)) {
                                        FieldHelpID = GenericController.encodeInteger(rs.Rows[0]["id"]);
                                    } else {
                                        FieldHelpID = core.db.insertTableRecordGetId("ccfieldhelp", 0);
                                    }
                                }
                                if (FieldHelpID != 0) {
                                    string Copy = workingField.helpCustom;
                                    if (string.IsNullOrEmpty(Copy)) { Copy = workingField.helpDefault; }
                                    core.db.executeQuery("update ccfieldhelp set active=1,contentcontrolid=" + FieldHelpCID + ",fieldid=" + fieldId + ",helpdefault=" + DbController.encodeSQLText(Copy) + " where id=" + FieldHelpID);
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
                        NewAppController.verifyNavigatorEntry(core, menu, 0);
                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "metadata Load, Upgrade collections added during upgrade process");
                //----------------------------------------------------------------------------------------------------------------------
                //\//
                // 
                // 20181028 - metadata upgrades shuld not include addon upgrdes
                //LogController.logInfo(core, "Installing Add-on Collections gathered during upgrade");
                //foreach (var import in Collection.collectionImports) {
                //    string CollectionPath = "";
                //    DateTime lastChangeDate = new DateTime();
                //    string emptyString = "";
                //    CollectionController.getCollectionConfig(core, import.Guid, ref CollectionPath, ref lastChangeDate, ref emptyString);
                //    string errorMessage = "";
                //    if (!string.IsNullOrEmpty(CollectionPath)) {
                //        //
                //        // This collection is installed locally, install from local collections
                //        //
                //        CollectionController.installCollectionFromLocalRepo(core, import.Guid, core.codeVersion(), ref errorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                //    } else {
                //        //
                //        // This is a new collection, install to the server and force it on this site
                //        //
                //        bool addonInstallOk = CollectionController.installCollectionFromRemoteRepo(core, import.Guid, ref errorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                //        if (!addonInstallOk) {
                //            throw (new GenericException("Failure to install addon collection from remote repository. Collection [" + import.Guid + "] was referenced in collection [" + Collection.name + "]")); //core.handleLegacyError3(core.appConfig.name, "Error upgrading Addon Collection [" & Guid & "], " & errorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                //        }
                //    }
                //}
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
                    //Dim AddonClass As addonInstallClass
                    string StyleSheetAdd = "";
                    for (var Ptr = 0; Ptr < Collection.styleCnt; Ptr++) {
                        bool Found = false;
                        var tempVar4 = Collection.styles[Ptr];
                        if (tempVar4.dataChanged) {
                            string NewStyleName = tempVar4.Name;
                            string NewStyleValue = tempVar4.Copy;
                            NewStyleValue = GenericController.vbReplace(NewStyleValue, "}", "");
                            NewStyleValue = GenericController.vbReplace(NewStyleValue, "{", "");
                            if (SiteStyleCnt > 0) {
                                int SiteStylePtr = 0;
                                for (SiteStylePtr = 0; SiteStylePtr < SiteStyleCnt; SiteStylePtr++) {
                                    string StyleLine = SiteStyleSplit[SiteStylePtr];
                                    int PosNameLineEnd = StyleLine.LastIndexOf("{") + 1;
                                    if (PosNameLineEnd > 0) {
                                        int PosNameLineStart = StyleLine.LastIndexOf("\r\n", PosNameLineEnd - 1) + 1;
                                        if (PosNameLineStart > 0) {
                                            //
                                            // Check this site style for a match with the NewStyleName
                                            //
                                            PosNameLineStart = PosNameLineStart + 2;
                                            string TestStyleName = (StyleLine.Substring(PosNameLineStart - 1, PosNameLineEnd - PosNameLineStart)).Trim(' ');
                                            if (GenericController.vbLCase(TestStyleName) == GenericController.vbLCase(NewStyleName)) {
                                                Found = true;
                                                if (tempVar4.Overwrite) {
                                                    //
                                                    // Found - Update style
                                                    //
                                                    SiteStyleSplit[SiteStylePtr] = "\r\n" + tempVar4.Name + " {" + NewStyleValue;
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
                                StyleSheetAdd = StyleSheetAdd + "\r\n" + NewStyleName + " {" + NewStyleValue + "}";
                            }
                        }
                    }
                    SiteStyles = string.Join("}", SiteStyleSplit);
                    if (!string.IsNullOrEmpty(StyleSheetAdd)) {
                        SiteStyles = SiteStyles
                            + "\r\n\r\n/*"
                            + "\r\nStyles added " + DateTime.Now + "\r\n*/"
                            + "\r\n" + StyleSheetAdd;
                    }
                    core.wwwFiles.saveFile("templates/styles.css", SiteStyles);
                    //
                    // -- Update stylesheet cache
                    core.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                                        //field.Changed = True
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
                            updateDst |= (dstMetaData.parentID != srcMetaData.parentID);
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
                        dstMetaData.aliasID = srcMetaData.aliasID;
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
                        dstMetaData.parentID = srcMetaData.parentID;
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
                    //Call AppendClassLogFile(core.app.config.name,"Upgrademetadata_AddSrcToDst", "CollectionSrc.metadata[SrcPtr].fields.count=" & CollectionSrc.metadata[SrcPtr].fields.count)
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
                                    updateDst |= (srcMetaDataField.lookupContentID != dstMetaDataField.lookupContentID);
                                    updateDst |= !textMatch(srcMetaDataField.lookupList, dstMetaDataField.lookupList);
                                    updateDst |= (srcMetaDataField.manyToManyContentID != dstMetaDataField.manyToManyContentID);
                                    updateDst |= (srcMetaDataField.manyToManyRuleContentID != dstMetaDataField.manyToManyRuleContentID);
                                    updateDst |= !textMatch(srcMetaDataField.manyToManyRulePrimaryField, dstMetaDataField.manyToManyRulePrimaryField);
                                    updateDst |= !textMatch(srcMetaDataField.manyToManyRuleSecondaryField, dstMetaDataField.manyToManyRuleSecondaryField);
                                    updateDst |= (srcMetaDataField.memberSelectGroupId_get(core) != dstMetaDataField.memberSelectGroupId_get(core));
                                    updateDst |= (srcMetaDataField.notEditable != dstMetaDataField.notEditable);
                                    updateDst |= (srcMetaDataField.password != dstMetaDataField.password);
                                    updateDst |= (srcMetaDataField.readOnly != dstMetaDataField.readOnly);
                                    updateDst |= (srcMetaDataField.redirectContentID != dstMetaDataField.redirectContentID);
                                    updateDst |= !textMatch(srcMetaDataField.redirectID, dstMetaDataField.redirectID);
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
                                dstMetaDataField.lookupContentID = srcMetaDataField.lookupContentID;
                                dstMetaDataField.lookupList = srcMetaDataField.lookupList;
                                dstMetaDataField.manyToManyContentID = srcMetaDataField.manyToManyContentID;
                                dstMetaDataField.manyToManyRuleContentID = srcMetaDataField.manyToManyRuleContentID;
                                dstMetaDataField.manyToManyRulePrimaryField = srcMetaDataField.manyToManyRulePrimaryField;
                                dstMetaDataField.manyToManyRuleSecondaryField = srcMetaDataField.manyToManyRuleSecondaryField;
                                dstMetaDataField.memberSelectGroupId_set(core, srcMetaDataField.memberSelectGroupId_get(core));
                                dstMetaDataField.nameLc = srcMetaDataField.nameLc;
                                dstMetaDataField.notEditable = srcMetaDataField.notEditable;
                                dstMetaDataField.password = srcMetaDataField.password;
                                dstMetaDataField.readOnly = srcMetaDataField.readOnly;
                                dstMetaDataField.redirectContentID = srcMetaDataField.redirectContentID;
                                dstMetaDataField.redirectID = srcMetaDataField.redirectID;
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
                    string SrcParentName = GenericController.vbLCase(srcMenu.ParentName);
                    string SrcNameSpace = GenericController.vbLCase(srcMenu.menuNameSpace);
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
                            DstKey = GenericController.vbLCase(dstMenu.Key);
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
                            dstName = GenericController.vbLCase(dstMenu.name);
                            if ((srcName == dstName) && (SrcIsNavigator == DstIsNavigator)) {
                                if (SrcIsNavigator) {
                                    //
                                    // Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    IsMatch = (SrcNameSpace == GenericController.vbLCase(dstMenu.menuNameSpace)) && (dstMenu.Guid == "");
                                } else {
                                    //
                                    // AdminMenu - check parentname
                                    IsMatch = (SrcParentName == GenericController.vbLCase(dstMenu.ParentName));
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
                // Check addons -- yes, this should be done.
                //-------------------------------------------------------------------------------------------------
                //
                //If False Then
                //    '
                //    ' remove this for now -- later add ImportCollections to track the collections (not addons)
                //    '
                //    '
                //    '
                //    For SrcPtr = 0 To srcCollection.AddOnCnt - 1
                //        SrcContentName = genericController.vbLCase(srcCollection.AddOns[SrcPtr].Name)
                //        okToUpdateDstFromSrc = False
                //        '
                //        ' Search for this name in the Dst
                //        '
                //        For DstPtr = 0 To dstCollection.AddOnCnt - 1
                //            DstName = genericController.vbLCase(dstCollection.AddOns[dstPtr].Name)
                //            If DstName = SrcContentName Then
                //                '
                //                ' found a match between Src and Dst
                //                '
                //                If SrcIsUsermetadata Then
                //                    '
                //                    ' test for metadata attribute changes
                //                    '
                //                    With dstCollection.AddOns[dstPtr]
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.ArgumentList, srcCollection.AddOns[SrcPtr].ArgumentList)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Copy, srcCollection.AddOns[SrcPtr].Copy)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Link, srcCollection.AddOns[SrcPtr].Link)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Name, srcCollection.AddOns[SrcPtr].Name)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.ObjectProgramID, srcCollection.AddOns[SrcPtr].ObjectProgramID)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.SortOrder, srcCollection.AddOns[SrcPtr].SortOrder)
                //                    End With
                //                End If
                //                Exit For
                //            End If
                //        Next
                //        If DstPtr = dstCollection.AddOnCnt Then
                //            '
                //            ' metadata was not found, add it
                //            '
                //            Array.Resize( ref asdf,asdf) // redim preserve  dstCollection.AddOns(dstCollection.AddOnCnt)
                //            dstCollection.AddOnCnt = DstPtr + 1
                //            okToUpdateDstFromSrc = True
                //        End If
                //        If okToUpdateDstFromSrc Then
                //            With dstCollection.AddOns[dstPtr]
                //                '
                //                ' It okToUpdateDstFromSrc, update the Dst with the Src
                //                '
                //                .metadataChanged = True
                //                .ArgumentList = srcCollection.AddOns[SrcPtr].ArgumentList
                //                .Copy = srcCollection.AddOns[SrcPtr].Copy
                //                .Link = srcCollection.AddOns[SrcPtr].Link
                //                .Name = srcCollection.AddOns[SrcPtr].Name
                //                .ObjectProgramID = srcCollection.AddOns[SrcPtr].ObjectProgramID
                //                .SortOrder = srcCollection.AddOns[SrcPtr].SortOrder
                //            End With
                //        End If
                //    Next
                //End If
                //
                //-------------------------------------------------------------------------------------------------
                // Check styles
                //-------------------------------------------------------------------------------------------------
                //
                int srcStylePtr = 0;
                int dstStylePtr = 0;
                for (srcStylePtr = 0; srcStylePtr < srcCollection.styleCnt; srcStylePtr++) {
                    string srcName = GenericController.vbLCase(srcCollection.styles[srcStylePtr].Name);
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    //
                    for (dstStylePtr = 0; dstStylePtr < dstCollection.styleCnt; dstStylePtr++) {
                        dstName = GenericController.vbLCase(dstCollection.styles[dstStylePtr].Name);
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
                //foreach (var import in srcCollection.collectionImports) {
                //    dstCollection.collectionImports.Add(import);
                //}
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
                LogController.handleError(core, ex);
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
                    string applicationMetaDataMiniCollectionXml = ApplicationMetaDataMiniCollection.get(core, true);
                    result = MetadataMiniCollectionModel.loadXML(core, applicationMetaDataMiniCollectionXml, false, false, isNewBuild, new MetadataMiniCollectionModel(), logPrefix);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Update a table from a collection metadata node
        /// </summary>
        internal static void installMetaDataMiniCollection_buildDb_saveMetaDataToDb(CoreController core, ContentMetadataModel contentMetadata, string BuildVersion) {
            try {
                //
                LogController.logInfo(core, "Update db metadata [" + contentMetadata.name + "]");
                //
                // -- get contentid and protect content with IsBaseContent true
                {
                    if (contentMetadata.dataChanged) {
                        //
                        // -- update definition (use SingleRecord as an update flag)
                        var datasource = DataSourceModel.createByUniqueName(core, contentMetadata.dataSourceName);
                        ContentMetadataModel.verifyContent_returnId(core, contentMetadata);
                    }
                    //
                    // -- update Content Field Records and Content Field Help records
                    ContentMetadataModel metaDataFieldHelp = ContentMetadataModel.createByUniqueName(core, ContentFieldHelpModel.contentName);
                    foreach (var nameValuePair in contentMetadata.fields) {
                        ContentFieldMetadataModel fieldMetadata = nameValuePair.Value;
                        if (fieldMetadata.dataChanged) {
                            contentMetadata.verifyContentField(core, fieldMetadata, false);
                        }
                        //
                        // -- update content field help records
                        if (fieldMetadata.helpChanged) {
                            //int FieldHelpID = 0;
                            ContentFieldHelpModel fieldHelp = null;
                            var fieldHelpList = ContentFieldHelpModel.createList(core, "fieldid=" + fieldMetadata.id);
                            if (fieldHelpList.Count == 0) {
                                //
                                // -- no current field help record, if adding help, create record
                                if ((!string.IsNullOrWhiteSpace(fieldMetadata.helpDefault)) || (!string.IsNullOrWhiteSpace(fieldMetadata.helpCustom))) {
                                    fieldHelp = ContentFieldHelpModel.addEmpty(core);
                                    fieldHelp.helpDefault = fieldMetadata.helpDefault;
                                    fieldHelp.helpCustom = fieldMetadata.helpCustom;
                                    fieldHelp.save(core);

                                }
                            } else {
                                //
                                // -- if help changed, save it
                                fieldHelp = fieldHelpList.First();
                                if ((!fieldHelp.helpCustom.Equals(fieldMetadata.helpCustom)) || !fieldHelp.helpDefault.Equals(fieldMetadata.helpDefault)) {
                                    fieldHelp.helpDefault = fieldMetadata.helpDefault;
                                    fieldHelp.helpCustom = fieldMetadata.helpCustom;
                                    fieldHelp.save(core);
                                }
                            }
                        }
                    }
                    //
                    // -- save changes
                    //content.save(core, true);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
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
                    LCaseParentName = GenericController.vbLCase(ParentName);
                    foreach (var kvp in menus) {
                        MetadataMiniCollectionModel.MiniCollectionMenuModel testMenu = kvp.Value;
                        if (GenericController.vbInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0) {
                            if (LCaseParentName == GenericController.vbLCase(testMenu.name) && (menu.IsNavigator == testMenu.IsNavigator)) {
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
                LogController.handleError(core, ex);
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
