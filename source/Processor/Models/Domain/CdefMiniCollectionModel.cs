
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
    /// miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    /// </summary>
    public class CDefMiniCollectionModel : ICloneable {
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
        public Dictionary<string, Models.Domain.CDefModel> cdef = new Dictionary<string, Models.Domain.CDefModel>() { };
        //
        //====================================================================================================
        /// <summary>
        /// 20181026 - no longer needed
        /// Name dictionary of all addons in the miniCollection
        /// </summary>
        //public Dictionary<string, miniCollectionAddOnModel> legacyAddOns = new Dictionary<string, miniCollectionAddOnModel>() { };
        //
        //====================================================================================================
        /// <summary>
        /// Model of addons in the minicollection
        /// </summary>
        //public class miniCollectionAddOnModel {
        //    public string Name;
        //    public string Link;
        //    public string ObjectProgramID;
        //    public string ArgumentList;
        //    public string SortOrder;
        //    public string Copy;
        //    public bool dataChanged;
        //}
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
        /// List of collections that must be installed before this collection can be installed
        /// </summary>
        //public List<ImportCollectionType> collectionImports = new List<ImportCollectionType>() { };
        //
        //====================================================================================================
        /// <summary>
        /// Model for imported collections
        /// </summary>
        //public struct ImportCollectionType {
        //    public string Name;
        //    public string Guid;
        //}
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
        //internal static void installCDefMiniCollectionFromXml_2(CoreController core, string srcXml, bool isNewBuild, bool isBaseCollection, bool isRepairMode, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
        //    CDefMiniCollectionModel baseCollection = installCDefMiniCollection_LoadXml(core, srcXml, true, true, isNewBuild, new CDefMiniCollectionModel(), logPrefix, ref installedCollections);
        //    installCDefMiniCollection_BuildDb(core, baseCollection, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix, ref installedCollections);

        //}
        //
        //======================================================================================================
        //
        internal static void installCDefMiniCollectionFromXml(bool quick, CoreController core, string srcXml, bool isNewBuild, bool isRepairMode, bool isBaseCollection, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            try {
                if (quick) {
                    //
                    LogController.logInfo(core, "installCDefMiniCollectionFromXML");
                    // 
                    // -- method 1, just install
                    CDefMiniCollectionModel baseCollection = installCDefMiniCollection_LoadXml(core, srcXml, true, true, isNewBuild, new CDefMiniCollectionModel(), logPrefix, ref installedCollections);
                    installCDefMiniCollection_BuildDb(core, baseCollection, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                } else {
                    //
                    // -- method 2, merge with application collection and install
                    //
                    CDefMiniCollectionModel miniCollectionWorking = getApplicationCDefMiniCollection(core, isNewBuild, logPrefix, ref installedCollections);
                    CDefMiniCollectionModel miniCollectionToAdd = installCDefMiniCollection_LoadXml(core, srcXml, isBaseCollection, false, isNewBuild, miniCollectionWorking, logPrefix, ref installedCollections);
                    addMiniCollectionSrcToDst(core, ref miniCollectionWorking, miniCollectionToAdd);
                    installCDefMiniCollection_BuildDb(core, miniCollectionWorking, core.siteProperties.dataBuildVersion, isNewBuild, isRepairMode, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                }
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// create a collection class from a collection xml file, cdef are added to the cdefs in the application collection
        /// </summary>
        private static CDefMiniCollectionModel installCDefMiniCollection_LoadXml(CoreController core, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, CDefMiniCollectionModel defaultCollection, string logPrefix, ref List<string> installedCollections) {
            CDefMiniCollectionModel result = null;
            try {
                Models.Domain.CDefModel DefaultCDef = null;
                Models.Domain.CDefFieldModel DefaultCDefField = null;
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

                XmlNode CDef_Node = null;
                string DataSourceName = null;
                XmlDocument srcXmlDom = new XmlDocument();
                string NodeName = null;
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", UpgradeCDef_LoadDataToCollection");
                //
                result = new CDefMiniCollectionModel();
                //
                if (string.IsNullOrEmpty(srcCollecionXml)) {
                    //
                    // -- empty collection is an error
                    throw (new GenericException("UpgradeCDef_LoadDataToCollection, srcCollectionXml is blank or null"));
                } else {
                    try {
                        srcXmlDom.LoadXml(srcCollecionXml);
                    } catch (Exception ex) {
                        //
                        // -- xml load error
                        LogController.logError(core, "UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" + ex.ToString() + "]");
                        throw new Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    if ((srcXmlDom.DocumentElement.Name.ToLowerInvariant() != CollectionFileRootNode) && (srcXmlDom.DocumentElement.Name.ToLowerInvariant() != "contensivecdef")) {
                        //
                        // -- root node must be collection (or legacy contensivecdef)
                        LogController.handleError(core, new GenericException("the archive file has a syntax error. Application name must be the first node."));
                    } else {
                        result.isBaseCollection = IsccBaseFile;
                        //
                        // Get Collection Name for logs
                        //
                        //hint = "get collection name"
                        Collectionname = XmlController.GetXMLAttribute(core, Found, srcXmlDom.DocumentElement, "name", "");
                        if (string.IsNullOrEmpty(Collectionname)) {
                            LogController.logInfo(core, "UpgradeCDef_LoadDataToCollection, Application: " + core.appConfig.name + ", Collection has no name");
                        } else {
                            //Call AppendClassLogFile(core.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & core.app.appEnvironment.name & ", Collection: " & Collectionname)
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
                        foreach (XmlNode CDef_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes) {
                            CDef_Node = CDef_NodeWithinLoop;
                            //isCdefTarget = False
                            NodeName = GenericController.vbLCase(CDef_NodeWithinLoop.Name);
                            //hint = "read node " & NodeName
                            switch (NodeName) {
                                case "cdef":
                                    //
                                    // Content Definitions
                                    //
                                    ContentName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "name", "");
                                    contentNameLc = GenericController.vbLCase(ContentName);
                                    if (string.IsNullOrEmpty(ContentName)) {
                                        throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "collection file contains a CDEF node with no name attribute. This is not allowed.", "dll", "builderClass", "UpgradeCDef_LoadDataToCollection", 0, "", "", False, True, "")
                                    } else {
                                        //
                                        // setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
                                        //
                                        if (defaultCollection.cdef.ContainsKey(contentNameLc)) {
                                            DefaultCDef = defaultCollection.cdef[contentNameLc];
                                        } else {
                                            DefaultCDef = new Models.Domain.CDefModel() {
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
                                        ContentTableName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.tableName);
                                        if (!string.IsNullOrEmpty(ContentTableName)) {
                                            //
                                            // These two fields are needed to import the row
                                            //
                                            DataSourceName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "dataSource", DefaultCDef.dataSourceName);
                                            if (string.IsNullOrEmpty(DataSourceName)) {
                                                DataSourceName = "Default";
                                            }
                                            //
                                            // ----- Add CDef if not already there
                                            //
                                            if (!result.cdef.ContainsKey(ContentName.ToLowerInvariant())) {
                                                result.cdef.Add(ContentName.ToLowerInvariant(), new Models.Domain.CDefModel());
                                            }
                                            //
                                            // Get CDef attributes
                                            //
                                            Models.Domain.CDefModel targetCdef = result.cdef[ContentName.ToLowerInvariant()];
                                            string activeDefaultText = "1";
                                            if (!(DefaultCDef.active)) {
                                                activeDefaultText = "0";
                                            }
                                            ActiveText = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Active", activeDefaultText);
                                            if (string.IsNullOrEmpty(ActiveText)) {
                                                ActiveText = "1";
                                            }
                                            targetCdef.active = GenericController.encodeBoolean(ActiveText);
                                            targetCdef.activeOnly = true;
                                            //.adminColumns = ?
                                            targetCdef.adminOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AdminOnly", DefaultCDef.adminOnly);
                                            targetCdef.aliasID = "id";
                                            targetCdef.aliasName = "name";
                                            targetCdef.allowAdd = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowAdd", DefaultCDef.allowAdd);
                                            targetCdef.allowCalendarEvents = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowCalendarEvents", DefaultCDef.allowCalendarEvents);
                                            targetCdef.allowContentChildTool = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowContentChildTool", DefaultCDef.allowContentChildTool);
                                            targetCdef.allowContentTracking = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowContentTracking", DefaultCDef.allowContentTracking);
                                            targetCdef.allowDelete = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowDelete", DefaultCDef.allowDelete);
                                            targetCdef.allowTopicRules = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowTopicRules", DefaultCDef.allowTopicRules);
                                            targetCdef.guid = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", DefaultCDef.guid);
                                            targetCdef.dataChanged = setAllDataChanged;
                                            targetCdef.set_childIdList(core, new List<int>());
                                            targetCdef.legacyContentControlCriteria = "";
                                            targetCdef.dataSourceName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentDataSourceName", DefaultCDef.dataSourceName);
                                            targetCdef.tableName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.tableName);
                                            targetCdef.dataSourceId = 0;
                                            targetCdef.defaultSortMethod = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DefaultSortMethod", DefaultCDef.defaultSortMethod);
                                            if ((targetCdef.defaultSortMethod == "") || (targetCdef.defaultSortMethod.ToLowerInvariant() == "name")) {
                                                targetCdef.defaultSortMethod = "By Name";
                                            } else if (GenericController.vbLCase(targetCdef.defaultSortMethod) == "sortorder") {
                                                targetCdef.defaultSortMethod = "By Alpha Sort Order Field";
                                            } else if (GenericController.vbLCase(targetCdef.defaultSortMethod) == "date") {
                                                targetCdef.defaultSortMethod = "By Date";
                                            }
                                            targetCdef.developerOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "DeveloperOnly", DefaultCDef.developerOnly);
                                            targetCdef.dropDownFieldList = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DropDownFieldList", DefaultCDef.dropDownFieldList);
                                            targetCdef.editorGroupName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "EditorGroupName", DefaultCDef.editorGroupName);
                                            targetCdef.fields = new Dictionary<string, Models.Domain.CDefFieldModel>();
                                            targetCdef.iconLink = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "IconLink", DefaultCDef.iconLink);
                                            targetCdef.iconHeight = XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconHeight", DefaultCDef.iconHeight);
                                            targetCdef.iconWidth = XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconWidth", DefaultCDef.iconWidth);
                                            targetCdef.iconSprites = XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconSprites", DefaultCDef.iconSprites);
                                            targetCdef.includesAFieldChange = false;
                                            targetCdef.installedByCollectionGuid = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "installedByCollection", DefaultCDef.installedByCollectionGuid);
                                            targetCdef.isBaseContent = IsccBaseFile || XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "IsBaseContent", false);
                                            targetCdef.isModifiedSinceInstalled = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "IsModified", DefaultCDef.isModifiedSinceInstalled);
                                            targetCdef.name = ContentName;
                                            targetCdef.parentName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Parent", DefaultCDef.parentName);
                                            targetCdef.whereClause = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "WhereClause", DefaultCDef.whereClause);
                                            //
                                            // Get CDef field nodes
                                            //
                                            foreach (XmlNode CDefChildNode in CDef_NodeWithinLoop.ChildNodes) {
                                                //
                                                // ----- process CDef Field
                                                //
                                                if (textMatch(CDefChildNode.Name, "field")) {
                                                    FieldName = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Name", "");
                                                    if (FieldName.ToLowerInvariant() == "middlename") {
                                                        //FieldName = FieldName;
                                                    }
                                                    //
                                                    // try to find field in the defaultcdef
                                                    //
                                                    if (DefaultCDef.fields.ContainsKey(FieldName)) {
                                                        DefaultCDefField = DefaultCDef.fields[FieldName];
                                                    } else {
                                                        DefaultCDefField = new Models.Domain.CDefFieldModel();
                                                    }
                                                    //
                                                    if (!(result.cdef[ContentName.ToLowerInvariant()].fields.ContainsKey(FieldName.ToLowerInvariant()))) {
                                                        result.cdef[ContentName.ToLowerInvariant()].fields.Add(FieldName.ToLowerInvariant(), new Models.Domain.CDefFieldModel());
                                                    }
                                                    var cdefField = result.cdef[ContentName.ToLowerInvariant()].fields[FieldName.ToLowerInvariant()];
                                                    cdefField.nameLc = FieldName.ToLowerInvariant();
                                                    ActiveText = "0";
                                                    if (DefaultCDefField.active) {
                                                        ActiveText = "1";
                                                    }
                                                    ActiveText = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Active", ActiveText);
                                                    if (string.IsNullOrEmpty(ActiveText)) {
                                                        ActiveText = "1";
                                                    }
                                                    cdefField.active = GenericController.encodeBoolean(ActiveText);
                                                    //
                                                    // Convert Field Descriptor (text) to field type (integer)
                                                    //
                                                    string defaultFieldTypeName = core.db.getFieldTypeNameFromFieldTypeId(DefaultCDefField.fieldTypeId);
                                                    string fieldTypeName = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "FieldType", defaultFieldTypeName);
                                                    cdefField.fieldTypeId = core.db.getFieldTypeIdFromFieldTypeName(fieldTypeName);
                                                    //FieldTypeDescriptor =xmlController.GetXMLAttribute(core,Found, CDefChildNode, "FieldType", DefaultCDefField.fieldType)
                                                    //If genericController.vbIsNumeric(FieldTypeDescriptor) Then
                                                    //    .fieldType = genericController.EncodeInteger(FieldTypeDescriptor)
                                                    //Else
                                                    //    .fieldType = core.app.csv_GetFieldTypeByDescriptor(FieldTypeDescriptor)
                                                    //End If
                                                    //If .fieldType = 0 Then
                                                    //    .fieldType = FieldTypeText
                                                    //End If
                                                    cdefField.editSortPriority = XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "EditSortPriority", DefaultCDefField.editSortPriority);
                                                    cdefField.authorable = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Authorable", DefaultCDefField.authorable);
                                                    cdefField.caption = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Caption", DefaultCDefField.caption);
                                                    cdefField.defaultValue = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "DefaultValue", DefaultCDefField.defaultValue);
                                                    cdefField.notEditable = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "NotEditable", DefaultCDefField.notEditable);
                                                    cdefField.indexColumn = XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "IndexColumn", DefaultCDefField.indexColumn);
                                                    cdefField.indexWidth = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "IndexWidth", DefaultCDefField.indexWidth);
                                                    cdefField.indexSortOrder = XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "IndexSortOrder", DefaultCDefField.indexSortOrder);
                                                    cdefField.redirectID = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectID", DefaultCDefField.redirectID);
                                                    cdefField.redirectPath = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectPath", DefaultCDefField.redirectPath);
                                                    cdefField.htmlContent = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "HTMLContent", DefaultCDefField.htmlContent);
                                                    cdefField.uniqueName = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "UniqueName", DefaultCDefField.uniqueName);
                                                    cdefField.password = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Password", DefaultCDefField.password);
                                                    cdefField.adminOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "AdminOnly", DefaultCDefField.adminOnly);
                                                    cdefField.developerOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "DeveloperOnly", DefaultCDefField.developerOnly);
                                                    cdefField.readOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "ReadOnly", DefaultCDefField.readOnly);
                                                    cdefField.required = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Required", DefaultCDefField.required);
                                                    cdefField.RSSTitleField = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "RSSTitle", DefaultCDefField.RSSTitleField);
                                                    cdefField.RSSDescriptionField = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "RSSDescriptionField", DefaultCDefField.RSSDescriptionField);
                                                    cdefField.memberSelectGroupName_set(core, XmlController.GetXMLAttribute(core, Found, CDefChildNode, "MemberSelectGroup", ""));
                                                    cdefField.editTabName = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "EditTab", DefaultCDefField.editTabName);
                                                    cdefField.Scramble = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Scramble", DefaultCDefField.Scramble);
                                                    cdefField.lookupList = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "LookupList", DefaultCDefField.lookupList);
                                                    cdefField.ManyToManyRulePrimaryField = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRulePrimaryField", DefaultCDefField.ManyToManyRulePrimaryField);
                                                    cdefField.ManyToManyRuleSecondaryField = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRuleSecondaryField", DefaultCDefField.ManyToManyRuleSecondaryField);
                                                    cdefField.set_lookupContentName(core, XmlController.GetXMLAttribute(core, Found, CDefChildNode, "LookupContent", DefaultCDefField.get_lookupContentName(core)));
                                                    // isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                    // case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                    // case 2 - when the base file is loaded, all fields must include the attribute
                                                    //Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
                                                    cdefField.isBaseField = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "IsBaseField", false) || IsccBaseFile;
                                                    cdefField.set_redirectContentName(core, XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectContent", DefaultCDefField.get_redirectContentName(core)));
                                                    cdefField.set_manyToManyContentName(core, XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.get_manyToManyContentName(core)));
                                                    cdefField.set_manyToManyRuleContentName(core, XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.get_manyToManyRuleContentName(core)));
                                                    cdefField.isModifiedSinceInstalled = XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled);
                                                    cdefField.installedByCollectionGuid = XmlController.GetXMLAttribute(core, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid);
                                                    cdefField.dataChanged = setAllDataChanged;
                                                    //
                                                    // ----- handle child nodes (help node)
                                                    //
                                                    cdefField.helpCustom = "";
                                                    cdefField.helpDefault = "";
                                                    foreach (XmlNode FieldChildNode in CDefChildNode.ChildNodes) {
                                                        //
                                                        // ----- process CDef Field
                                                        //
                                                        if (textMatch(FieldChildNode.Name, "HelpDefault")) {
                                                            cdefField.helpDefault = FieldChildNode.InnerText;
                                                        }
                                                        if (textMatch(FieldChildNode.Name, "HelpCustom")) {
                                                            cdefField.helpCustom = FieldChildNode.InnerText;
                                                        }
                                                        cdefField.HelpChanged = setAllDataChanged;
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
                                    IndexName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "indexname", "");
                                    TableName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "tableName", "");
                                    DataSourceName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DataSourceName", "");
                                    if (string.IsNullOrEmpty(DataSourceName)) {
                                        DataSourceName = "default";
                                    }
                                    bool removeDup = false;
                                    CDefMiniCollectionModel.MiniCollectionSQLIndexModel dupToRemove = new CDefMiniCollectionModel.MiniCollectionSQLIndexModel();
                                    foreach (CDefMiniCollectionModel.MiniCollectionSQLIndexModel index in result.sqlIndexes) {
                                        if (textMatch(index.IndexName, IndexName) & textMatch(index.TableName, TableName) & textMatch(index.DataSourceName, DataSourceName)) {
                                            dupToRemove = index;
                                            removeDup = true;
                                            break;
                                        }
                                    }
                                    if (removeDup) {
                                        result.sqlIndexes.Remove(dupToRemove);
                                    }
                                    CDefMiniCollectionModel.MiniCollectionSQLIndexModel newIndex = new CDefMiniCollectionModel.MiniCollectionSQLIndexModel {
                                        IndexName = IndexName,
                                        TableName = TableName,
                                        DataSourceName = DataSourceName,
                                        FieldNameList = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "FieldNameList", "")
                                    };
                                    result.sqlIndexes.Add(newIndex);
                                    break;
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry":
                                    //
                                    // Admin Menus / Navigator Entries
                                    MenuName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
                                    menuNameSpace = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NameSpace", "");
                                    MenuGuid = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                    IsNavigator = (NodeName == "navigatorentry");
                                    string MenuKey = null;
                                    if (!IsNavigator) {
                                        MenuKey = GenericController.vbLCase(MenuName);
                                    } else {
                                        MenuKey = MenuGuid;
                                    }
                                    if (!result.menus.ContainsKey(MenuKey)) {
                                        ActiveText = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Active", "1");
                                        if (string.IsNullOrEmpty(ActiveText)) {
                                            ActiveText = "1";
                                        }
                                        result.menus.Add(MenuKey, new CDefMiniCollectionModel.MiniCollectionMenuModel() {
                                            dataChanged = setAllDataChanged,
                                            Name = MenuName,
                                            Guid = MenuGuid,
                                            Key = MenuKey,
                                            Active = GenericController.encodeBoolean(ActiveText),
                                            menuNameSpace = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NameSpace", ""),
                                            ParentName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ParentName", ""),
                                            ContentName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentName", ""),
                                            LinkPage = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "LinkPage", ""),
                                            SortOrder = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "SortOrder", ""),
                                            AdminOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AdminOnly", false),
                                            DeveloperOnly = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "DeveloperOnly", false),
                                            NewWindow = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "NewWindow", false),
                                            AddonName = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "AddonName", ""),
                                            NavIconType = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NavIconType", ""),
                                            NavIconTitle = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NavIconTitle", ""),
                                            IsNavigator = IsNavigator
                                        });
                                    }
                                    break;
                                case "aggregatefunction":
                                case "addon":
                                    //
                                    // 20181026 - no longer needed -- Aggregate Objects (just make them -- there are not too many
                                    //
                                    //Name =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
                                    //MiniCollectionModel.miniCollectionAddOnModel addon;
                                    //if (result.legacyAddOns.ContainsKey(Name.ToLowerInvariant())) {
                                    //    addon = result.legacyAddOns[Name.ToLowerInvariant()];
                                    //} else {
                                    //    addon = new MiniCollectionModel.miniCollectionAddOnModel();
                                    //    result.legacyAddOns.Add(Name.ToLowerInvariant(), addon);
                                    //}
                                    //addon.dataChanged = setAllDataChanged;
                                    //addon.Link =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Link", "");
                                    //addon.ObjectProgramID =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ObjectProgramID", "");
                                    //addon.ArgumentList =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ArgumentList", "");
                                    //addon.SortOrder =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "SortOrder", "");
                                    //addon.Copy =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "copy", "");
                                    break;
                                case "style":
                                    //
                                    // style sheet entries
                                    //
                                    Name = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
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
                                    tempVar5.Overwrite = XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "Overwrite", false);
                                    tempVar5.Copy = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "stylesheet":
                                    //
                                    // style sheet in one entry
                                    //
                                    result.styleSheet = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "getcollection":
                                case "importcollection":
                                    //if (true) {
                                    //    //If Not UpgradeDbOnly Then
                                    //    //
                                    //    // Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                    //    //
                                    //    Collectionname = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "name", "");
                                    //    CollectionGuid = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                    //    if (string.IsNullOrEmpty(CollectionGuid)) {
                                    //        CollectionGuid = CDef_NodeWithinLoop.InnerText;
                                    //    }
                                    //    if (string.IsNullOrEmpty(CollectionGuid)) {
                                    //        status = "The collection you selected [" + Collectionname + "] can not be downloaded because it does not include a valid GUID.";
                                    //        //core.AppendLog("builderClass.UpgradeCDef_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
                                    //    } else {
                                    //        result.collectionImports.Add(new CDefMiniCollectionModel.ImportCollectionType() {
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
                                    // Page Template - started, but Return_Collection and LoadDataToCDef are all that is done do far
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
                                    tempVar6.Copy = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Copy", "");
                                    tempVar6.Guid = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                    tempVar6.Style = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "style", "");
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
                            CDefMiniCollectionModel.MiniCollectionMenuModel menu = kvp.Value;
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
        /// Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
        /// </summary>
        private static void installCDefMiniCollection_BuildDb(CoreController core, CDefMiniCollectionModel Collection, string BuildVersion, bool isNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            try {
                //
                logPrefix += ", installCollection_BuildDbFromMiniCollection";
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", UpgradeCDef_BuildDbFromCollection");
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 1: verify core sql tables");
                //----------------------------------------------------------------------------------------------------------------------
                //
                AppBuilderController.verifyBasicTables(core, logPrefix);
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 2: create SQL tables in default datasource");
                string ContentName = null;
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (true) {
                    string UsedTables = "";
                    foreach (var keypairvalue in Collection.cdef) {
                        Models.Domain.CDefModel workingCdef = keypairvalue.Value;
                        ContentName = workingCdef.name;
                        if (workingCdef.dataChanged) {
                            LogController.logInfo(core, "creating sql table [" + workingCdef.tableName + "], datasource [" + workingCdef.dataSourceName + "]");
                            if (GenericController.vbLCase(workingCdef.dataSourceName) == "default" || workingCdef.dataSourceName == "") {
                                string TableName = workingCdef.tableName;
                                if (GenericController.vbInstr(1, "," + UsedTables + ",", "," + TableName + ",", 1) != 0) {
                                    //TableName = TableName;
                                } else {
                                    UsedTables = UsedTables + "," + TableName;
                                    core.db.createSQLTable(workingCdef.dataSourceName, TableName);
                                }
                                foreach (var fieldNvp in workingCdef.fields) {
                                    CDefFieldModel field = fieldNvp.Value;
                                    core.db.createSQLTableField(workingCdef.dataSourceName, TableName, field.nameLc, field.fieldTypeId);
                                }
                            }
                        }
                    }
                    core.doc.clearMetaData();
                    core.cache.invalidateAll();
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 3: Verify all CDef names in ccContent so GetContentID calls will succeed");
                //----------------------------------------------------------------------------------------------------------------------
                //
                List<string> installedContentList = new List<string>();
                DataTable rs = core.db.executeQuery("SELECT Name from ccContent where (active<>0)");
                if (DbController.isDataTableOk(rs)) {
                    installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
                }
                rs.Dispose();
                string SQL = null;
                //
                foreach (var keypairvalue in Collection.cdef) {
                    if (keypairvalue.Value.dataChanged) {
                        LogController.logInfo(core, "adding cdef name [" + keypairvalue.Value.name + "]");
                        if (!installedContentList.Contains(keypairvalue.Value.name.ToLowerInvariant())) {
                            SQL = "Insert into ccContent (name,ccguid,active,createkey)values(" + core.db.encodeSQLText(keypairvalue.Value.name) + "," + core.db.encodeSQLText(keypairvalue.Value.guid) + ",1,0);";
                            core.db.executeQuery(SQL);
                            installedContentList.Add(keypairvalue.Value.name.ToLowerInvariant());
                        }
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 4: Verify content records required for Content Server");
                //----------------------------------------------------------------------------------------------------------------------
                //
                AppBuilderController.verifySortMethods(core);
                AppBuilderController.verifyContentFieldTypes(core);
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 5: verify 'Content' content definition");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.cdef) {
                    if (keypairvalue.Value.name.ToLowerInvariant() == "content") {
                        installCDefMiniCollection_buildDb_saveCDefToDb(core, keypairvalue.Value, BuildVersion);
                        break;
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 6: Verify all definitions and fields");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.cdef) {
                    CDefModel cdef = keypairvalue.Value;
                    bool fieldChanged = false;
                    if (!cdef.dataChanged) {
                        foreach (var field in cdef.fields) {
                            fieldChanged = field.Value.dataChanged;
                            if (fieldChanged) break;
                        }
                    }
                    if ((fieldChanged || cdef.dataChanged) && (cdef.name.ToLowerInvariant() != "content")) {
                        installCDefMiniCollection_buildDb_saveCDefToDb(core, cdef, BuildVersion);
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 7: Verify all field help");
                //----------------------------------------------------------------------------------------------------------------------
                //
                int FieldHelpCID = core.db.getRecordID("content", "Content Field Help");
                foreach (var keypairvalue in Collection.cdef) {
                    Models.Domain.CDefModel workingCdef = keypairvalue.Value;
                    //ContentName = workingCdef.name;
                    foreach (var fieldKeyValuePair in workingCdef.fields) {
                        Models.Domain.CDefFieldModel workingField = fieldKeyValuePair.Value;
                        //string FieldName = field.nameLc;
                        //var field2 = Collection.cdef[ContentName.ToLowerInvariant()].fields[((string)null).ToLowerInvariant()];
                        if (workingField.HelpChanged) {
                            int fieldId = 0;
                            SQL = "select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" + core.db.encodeSQLText(workingField.nameLc) + ")and(c.name=" + core.db.encodeSQLText(workingCdef.name) + ") order by f.id";
                            rs = core.db.executeQuery(SQL);
                            if (DbController.isDataTableOk(rs)) {
                                fieldId = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "id"));
                            }
                            rs.Dispose();
                            if (fieldId == 0) {
                                throw (new GenericException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Can not update help field for content [" & ContentName & "], field [" & FieldName & "] because the field was not found in the Db.", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection", 0, "", "", False, True, "")
                            } else {
                                SQL = "select id from ccfieldhelp where fieldid=" + fieldId + " order by id";
                                rs = core.db.executeQuery(SQL);
                                //
                                int FieldHelpID = 0;
                                if (DbController.isDataTableOk(rs)) {
                                    FieldHelpID = GenericController.encodeInteger(rs.Rows[0]["id"]);
                                } else {
                                    FieldHelpID = core.db.insertTableRecordGetId("default", "ccfieldhelp", 0);
                                }
                                rs.Dispose();
                                if (FieldHelpID != 0) {
                                    string Copy = workingField.helpCustom;
                                    if (string.IsNullOrEmpty(Copy)) {
                                        Copy = workingField.helpDefault;
                                        if (!string.IsNullOrEmpty(Copy)) {
                                            //Copy = Copy;
                                        }
                                    }
                                    SQL = "update ccfieldhelp set active=1,contentcontrolid=" + FieldHelpCID + ",fieldid=" + fieldId + ",helpdefault=" + core.db.encodeSQLText(Copy) + " where id=" + FieldHelpID;
                                    core.db.executeQuery(SQL);
                                }
                            }
                        }
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 8: create SQL indexes");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (CDefMiniCollectionModel.MiniCollectionSQLIndexModel index in Collection.sqlIndexes) {
                    if (index.dataChanged) {
                        LogController.logInfo(core, "creating index [" + index.IndexName + "], fields [" + index.FieldNameList + "], on table [" + index.TableName + "]");
                        core.db.createSQLIndex(index.DataSourceName, index.TableName, index.IndexName, index.FieldNameList);
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, stage 9: Verify All Menu Names, then all Menus");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var kvp in Collection.menus) {
                    var menu = kvp.Value;
                    if (menu.dataChanged) {
                        LogController.logInfo(core, "creating navigator entry [" + menu.Name + "], namespace [" + menu.menuNameSpace + "], guid [" + menu.Guid + "]");
                        AppBuilderController.verifyNavigatorEntry(core, menu.Guid, menu.menuNameSpace, menu.Name, menu.ContentName, menu.LinkPage, menu.SortOrder, menu.AdminOnly, menu.DeveloperOnly, menu.NewWindow, menu.Active, menu.AddonName, menu.NavIconType, menu.NavIconTitle, 0);
                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, Upgrade collections added during upgrade process");
                //----------------------------------------------------------------------------------------------------------------------
                //\//
                // 
                // 20181028 - cdef upgrades shuld not include addon upgrdes
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
                LogController.logInfo(core, "CDef Load, stage 9: Verify Styles");
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
                    core.appRootFiles.saveFile("templates/styles.css", SiteStyles);
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
        /// Overlay a Src CDef on to the current one (Dst). Any Src CDEf entries found in Src are added to Dst.
        /// if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
        /// </summary>
        private static bool addMiniCollectionSrcToDst(CoreController core, ref CDefMiniCollectionModel dstCollection, CDefMiniCollectionModel srcCollection) {
            bool returnOk = true;
            try {
                string SrcFieldName = null;
                bool updateDst = false;
                Models.Domain.CDefModel srcCdef = null;
                //
                // If the Src is the BaseCollection, the Dst must be the Application Collectio
                //   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                //
                if (srcCollection.isBaseCollection) {
                    foreach (var dstKeyValuePair in dstCollection.cdef) {
                        Models.Domain.CDefModel dstWorkingCdef = dstKeyValuePair.Value;
                        string contentName = dstWorkingCdef.name;
                        if (dstCollection.cdef[contentName.ToLowerInvariant()].isBaseContent) {
                            //
                            // this application collection Cdef is marked base, verify it is in the base collection
                            //
                            if (!srcCollection.cdef.ContainsKey(contentName.ToLowerInvariant())) {
                                //
                                // cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                //
                                var tempVar = dstCollection.cdef[contentName.ToLowerInvariant()];
                                tempVar.isBaseContent = false;
                                tempVar.dataChanged = true;
                                foreach (var dstFieldKeyValuePair in tempVar.fields) {
                                    Models.Domain.CDefFieldModel field = dstFieldKeyValuePair.Value;
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
                //   if the cdef does not match, set cdefext[Ptr].CDefChanged true
                //   if any field does not match, set cdefext...field...CDefChanged
                //   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                // -------------------------------------------------------------------------------------------------
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", UpgradeCDef_AddSrcToDst");
                string dstName = null;
                //
                foreach (var srcKeyValuePair in srcCollection.cdef) {
                    srcCdef = srcKeyValuePair.Value;
                    string srcName = srcCdef.name;
                    //
                    // Search for this cdef in the Dst
                    //
                    updateDst = false;
                    Models.Domain.CDefModel dstCdef = null;
                    if (!dstCollection.cdef.ContainsKey(srcName.ToLowerInvariant())) {
                        //
                        // add src to dst
                        //
                        dstCdef = new Models.Domain.CDefModel();
                        dstCollection.cdef.Add(srcName.ToLowerInvariant(), dstCdef);
                        updateDst = true;
                    } else {
                        dstCdef = dstCollection.cdef[srcName.ToLowerInvariant()];
                        dstName = srcName;
                        //
                        // found a match between Src and Dst
                        //
                        if (dstCdef.isBaseContent == srcCdef.isBaseContent) {
                            //
                            // Allow changes to user cdef only from user cdef, changes to base only from base
                            updateDst |= (dstCdef.activeOnly != srcCdef.activeOnly);
                            updateDst |= (dstCdef.adminOnly != srcCdef.adminOnly);
                            updateDst |= (dstCdef.developerOnly != srcCdef.developerOnly);
                            updateDst |= (dstCdef.allowAdd != srcCdef.allowAdd);
                            updateDst |= (dstCdef.allowCalendarEvents != srcCdef.allowCalendarEvents);
                            updateDst |= (dstCdef.allowContentTracking != srcCdef.allowContentTracking);
                            updateDst |= (dstCdef.allowDelete != srcCdef.allowDelete);
                            updateDst |= (dstCdef.allowTopicRules != srcCdef.allowTopicRules);
                            updateDst |= !textMatch(dstCdef.dataSourceName, srcCdef.dataSourceName);
                            updateDst |= !textMatch(dstCdef.tableName, srcCdef.tableName);
                            updateDst |= !textMatch(dstCdef.defaultSortMethod, srcCdef.defaultSortMethod);
                            updateDst |= !textMatch(dstCdef.dropDownFieldList, srcCdef.dropDownFieldList);
                            updateDst |= !textMatch(dstCdef.editorGroupName, srcCdef.editorGroupName);
                            updateDst |= (dstCdef.active != srcCdef.active);
                            updateDst |= (dstCdef.allowContentChildTool != srcCdef.allowContentChildTool);
                            updateDst |= (dstCdef.parentID != srcCdef.parentID);
                            updateDst |= !textMatch(dstCdef.iconLink, srcCdef.iconLink);
                            updateDst |= (dstCdef.iconHeight != srcCdef.iconHeight);
                            updateDst |= (dstCdef.iconWidth != srcCdef.iconWidth);
                            updateDst |= (dstCdef.iconSprites != srcCdef.iconSprites);
                            updateDst |= !textMatch(dstCdef.installedByCollectionGuid, srcCdef.installedByCollectionGuid);
                            updateDst |= !textMatch(dstCdef.guid, srcCdef.guid);
                            updateDst |= (dstCdef.isBaseContent != srcCdef.isBaseContent);
                        }
                    }
                    if (updateDst) {
                        //
                        // update the Dst with the Src
                        dstCdef.active = srcCdef.active;
                        dstCdef.activeOnly = srcCdef.activeOnly;
                        dstCdef.adminOnly = srcCdef.adminOnly;
                        dstCdef.aliasID = srcCdef.aliasID;
                        dstCdef.aliasName = srcCdef.aliasName;
                        dstCdef.allowAdd = srcCdef.allowAdd;
                        dstCdef.allowCalendarEvents = srcCdef.allowCalendarEvents;
                        dstCdef.allowContentChildTool = srcCdef.allowContentChildTool;
                        dstCdef.allowContentTracking = srcCdef.allowContentTracking;
                        dstCdef.allowDelete = srcCdef.allowDelete;
                        dstCdef.allowTopicRules = srcCdef.allowTopicRules;
                        dstCdef.guid = srcCdef.guid;
                        dstCdef.legacyContentControlCriteria = srcCdef.legacyContentControlCriteria;
                        dstCdef.dataSourceName = srcCdef.dataSourceName;
                        dstCdef.tableName = srcCdef.tableName;
                        dstCdef.dataSourceId = srcCdef.dataSourceId;
                        dstCdef.defaultSortMethod = srcCdef.defaultSortMethod;
                        dstCdef.developerOnly = srcCdef.developerOnly;
                        dstCdef.dropDownFieldList = srcCdef.dropDownFieldList;
                        dstCdef.editorGroupName = srcCdef.editorGroupName;
                        dstCdef.iconHeight = srcCdef.iconHeight;
                        dstCdef.iconLink = srcCdef.iconLink;
                        dstCdef.iconSprites = srcCdef.iconSprites;
                        dstCdef.iconWidth = srcCdef.iconWidth;
                        dstCdef.installedByCollectionGuid = srcCdef.installedByCollectionGuid;
                        dstCdef.isBaseContent = srcCdef.isBaseContent;
                        dstCdef.isModifiedSinceInstalled = srcCdef.isModifiedSinceInstalled;
                        dstCdef.name = srcCdef.name;
                        dstCdef.parentID = srcCdef.parentID;
                        dstCdef.parentName = srcCdef.parentName;
                        dstCdef.selectCommaList = srcCdef.selectCommaList;
                        dstCdef.whereClause = srcCdef.whereClause;
                        dstCdef.includesAFieldChange = true;
                        dstCdef.dataChanged = true;
                    }
                    //
                    // Now check each of the field records for an addition, or a change
                    // DstPtr is still set to the Dst CDef
                    //
                    //Call AppendClassLogFile(core.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef[SrcPtr].fields.count=" & CollectionSrc.CDef[SrcPtr].fields.count)
                    foreach (var srcFieldKeyValuePair in srcCdef.fields) {
                        Models.Domain.CDefFieldModel srcCdefField = srcFieldKeyValuePair.Value;
                        SrcFieldName = srcCdefField.nameLc;
                        updateDst = false;
                        if (!dstCollection.cdef.ContainsKey(srcName.ToLowerInvariant())) {
                            //
                            // should have been the collection
                            //
                            throw (new GenericException("ERROR - cannot update destination content because it was not found after being added."));
                        } else {
                            dstCdef = dstCollection.cdef[srcName.ToLowerInvariant()];
                            bool HelpChanged = false;
                            Models.Domain.CDefFieldModel dstCdefField = null;
                            if (dstCdef.fields.ContainsKey(SrcFieldName.ToLowerInvariant())) {
                                //
                                // Src field was found in Dst fields
                                //

                                dstCdefField = dstCdef.fields[SrcFieldName.ToLowerInvariant()];
                                updateDst = false;
                                if (dstCdefField.isBaseField == srcCdefField.isBaseField) {
                                    updateDst |= (srcCdefField.active != dstCdefField.active);
                                    updateDst |= (srcCdefField.adminOnly != dstCdefField.adminOnly);
                                    updateDst |= (srcCdefField.authorable != dstCdefField.authorable);
                                    updateDst |= !textMatch(srcCdefField.caption, dstCdefField.caption);
                                    updateDst |= (srcCdefField.contentId != dstCdefField.contentId);
                                    updateDst |= (srcCdefField.developerOnly != dstCdefField.developerOnly);
                                    updateDst |= (srcCdefField.editSortPriority != dstCdefField.editSortPriority);
                                    updateDst |= !textMatch(srcCdefField.editTabName, dstCdefField.editTabName);
                                    updateDst |= (srcCdefField.fieldTypeId != dstCdefField.fieldTypeId);
                                    updateDst |= (srcCdefField.htmlContent != dstCdefField.htmlContent);
                                    updateDst |= (srcCdefField.indexColumn != dstCdefField.indexColumn);
                                    updateDst |= (srcCdefField.indexSortDirection != dstCdefField.indexSortDirection);
                                    updateDst |= (encodeInteger(srcCdefField.indexSortOrder) != GenericController.encodeInteger(dstCdefField.indexSortOrder));
                                    updateDst |= !textMatch(srcCdefField.indexWidth, dstCdefField.indexWidth);
                                    updateDst |= (srcCdefField.lookupContentID != dstCdefField.lookupContentID);
                                    updateDst |= !textMatch(srcCdefField.lookupList, dstCdefField.lookupList);
                                    updateDst |= (srcCdefField.manyToManyContentID != dstCdefField.manyToManyContentID);
                                    updateDst |= (srcCdefField.manyToManyRuleContentID != dstCdefField.manyToManyRuleContentID);
                                    updateDst |= !textMatch(srcCdefField.ManyToManyRulePrimaryField, dstCdefField.ManyToManyRulePrimaryField);
                                    updateDst |= !textMatch(srcCdefField.ManyToManyRuleSecondaryField, dstCdefField.ManyToManyRuleSecondaryField);
                                    updateDst |= (srcCdefField.memberSelectGroupId_get(core) != dstCdefField.memberSelectGroupId_get(core));
                                    updateDst |= (srcCdefField.notEditable != dstCdefField.notEditable);
                                    updateDst |= (srcCdefField.password != dstCdefField.password);
                                    updateDst |= (srcCdefField.readOnly != dstCdefField.readOnly);
                                    updateDst |= (srcCdefField.redirectContentID != dstCdefField.redirectContentID);
                                    updateDst |= !textMatch(srcCdefField.redirectID, dstCdefField.redirectID);
                                    updateDst |= !textMatch(srcCdefField.redirectPath, dstCdefField.redirectPath);
                                    updateDst |= (srcCdefField.required != dstCdefField.required);
                                    updateDst |= (srcCdefField.RSSDescriptionField != dstCdefField.RSSDescriptionField);
                                    updateDst |= (srcCdefField.RSSTitleField != dstCdefField.RSSTitleField);
                                    updateDst |= (srcCdefField.Scramble != dstCdefField.Scramble);
                                    updateDst |= (srcCdefField.textBuffered != dstCdefField.textBuffered);
                                    updateDst |= (GenericController.encodeText(srcCdefField.defaultValue) != GenericController.encodeText(dstCdefField.defaultValue));
                                    updateDst |= (srcCdefField.uniqueName != dstCdefField.uniqueName);
                                    updateDst |= (srcCdefField.isBaseField != dstCdefField.isBaseField);
                                    updateDst |= !textMatch(srcCdefField.get_lookupContentName(core), dstCdefField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_lookupContentName(core), dstCdefField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_manyToManyRuleContentName(core), dstCdefField.get_manyToManyRuleContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_redirectContentName(core), dstCdefField.get_redirectContentName(core));
                                    updateDst |= !textMatch(srcCdefField.installedByCollectionGuid, dstCdefField.installedByCollectionGuid);
                                }
                                //
                                // Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
                                //
                                bool HelpCustomChanged = !textMatch(srcCdefField.helpCustom, srcCdefField.helpCustom);
                                bool HelpDefaultChanged = !textMatch(srcCdefField.helpDefault, srcCdefField.helpDefault);
                                HelpChanged = HelpDefaultChanged || HelpCustomChanged;
                            } else {
                                //
                                // field was not found in dst, add it and populate
                                //
                                dstCdef.fields.Add(SrcFieldName.ToLowerInvariant(), new Models.Domain.CDefFieldModel());
                                dstCdefField = dstCdef.fields[SrcFieldName.ToLowerInvariant()];
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
                                dstCdefField.active = srcCdefField.active;
                                dstCdefField.adminOnly = srcCdefField.adminOnly;
                                dstCdefField.authorable = srcCdefField.authorable;
                                dstCdefField.caption = srcCdefField.caption;
                                dstCdefField.contentId = srcCdefField.contentId;
                                dstCdefField.defaultValue = srcCdefField.defaultValue;
                                dstCdefField.developerOnly = srcCdefField.developerOnly;
                                dstCdefField.editSortPriority = srcCdefField.editSortPriority;
                                dstCdefField.editTabName = srcCdefField.editTabName;
                                dstCdefField.fieldTypeId = srcCdefField.fieldTypeId;
                                dstCdefField.htmlContent = srcCdefField.htmlContent;
                                dstCdefField.indexColumn = srcCdefField.indexColumn;
                                dstCdefField.indexSortDirection = srcCdefField.indexSortDirection;
                                dstCdefField.indexSortOrder = srcCdefField.indexSortOrder;
                                dstCdefField.indexWidth = srcCdefField.indexWidth;
                                dstCdefField.lookupContentID = srcCdefField.lookupContentID;
                                dstCdefField.lookupList = srcCdefField.lookupList;
                                dstCdefField.manyToManyContentID = srcCdefField.manyToManyContentID;
                                dstCdefField.manyToManyRuleContentID = srcCdefField.manyToManyRuleContentID;
                                dstCdefField.ManyToManyRulePrimaryField = srcCdefField.ManyToManyRulePrimaryField;
                                dstCdefField.ManyToManyRuleSecondaryField = srcCdefField.ManyToManyRuleSecondaryField;
                                dstCdefField.memberSelectGroupId_set(core, srcCdefField.memberSelectGroupId_get(core));
                                dstCdefField.nameLc = srcCdefField.nameLc;
                                dstCdefField.notEditable = srcCdefField.notEditable;
                                dstCdefField.password = srcCdefField.password;
                                dstCdefField.readOnly = srcCdefField.readOnly;
                                dstCdefField.redirectContentID = srcCdefField.redirectContentID;
                                dstCdefField.redirectID = srcCdefField.redirectID;
                                dstCdefField.redirectPath = srcCdefField.redirectPath;
                                dstCdefField.required = srcCdefField.required;
                                dstCdefField.RSSDescriptionField = srcCdefField.RSSDescriptionField;
                                dstCdefField.RSSTitleField = srcCdefField.RSSTitleField;
                                dstCdefField.Scramble = srcCdefField.Scramble;
                                dstCdefField.textBuffered = srcCdefField.textBuffered;
                                dstCdefField.uniqueName = srcCdefField.uniqueName;
                                dstCdefField.isBaseField = srcCdefField.isBaseField;
                                dstCdefField.set_lookupContentName(core, srcCdefField.get_lookupContentName(core));
                                dstCdefField.set_manyToManyContentName(core, srcCdefField.get_manyToManyContentName(core));
                                dstCdefField.set_manyToManyRuleContentName(core, srcCdefField.get_manyToManyRuleContentName(core));
                                dstCdefField.set_redirectContentName(core, srcCdefField.get_redirectContentName(core));
                                dstCdefField.installedByCollectionGuid = srcCdefField.installedByCollectionGuid;
                                dstCdefField.dataChanged = true;
                                dstCdef.includesAFieldChange = true;
                            }
                            if (HelpChanged) {
                                dstCdefField.helpCustom = srcCdefField.helpCustom;
                                dstCdefField.helpDefault = srcCdefField.helpDefault;
                                dstCdefField.HelpChanged = true;
                            }
                        }
                    }
                }
                //
                // -------------------------------------------------------------------------------------------------
                // Check SQL Indexes
                // -------------------------------------------------------------------------------------------------
                //
                foreach (CDefMiniCollectionModel.MiniCollectionSQLIndexModel srcSqlIndex in srcCollection.sqlIndexes) {
                    string srcName = (srcSqlIndex.DataSourceName + "-" + srcSqlIndex.TableName + "-" + srcSqlIndex.IndexName).ToLowerInvariant();
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    bool indexFound = false;
                    bool indexChanged = false;
                    CDefMiniCollectionModel.MiniCollectionSQLIndexModel indexToUpdate = new CDefMiniCollectionModel.MiniCollectionSQLIndexModel() { };
                    foreach (CDefMiniCollectionModel.MiniCollectionSQLIndexModel dstSqlIndex in dstCollection.sqlIndexes) {
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
                    CDefMiniCollectionModel.MiniCollectionMenuModel srcMenu = srcKvp.Value;
                    string srcName = srcMenu.Name.ToLowerInvariant();
                    string srcGuid = srcMenu.Guid;
                    string SrcParentName = GenericController.vbLCase(srcMenu.ParentName);
                    string SrcNameSpace = GenericController.vbLCase(srcMenu.menuNameSpace);
                    bool SrcIsNavigator = srcMenu.IsNavigator;
                    updateDst = false;
                    //
                    // Search for match using guid
                    CDefMiniCollectionModel.MiniCollectionMenuModel dstMenuMatch = new CDefMiniCollectionModel.MiniCollectionMenuModel() { };
                    bool IsMatch = false;
                    string DstKey = null;
                    bool DstIsNavigator = false;
                    foreach (var dstKvp in dstCollection.menus) {
                        string dstKey = dstKvp.Key.ToLowerInvariant();
                        CDefMiniCollectionModel.MiniCollectionMenuModel dstMenu = dstKvp.Value;
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
                            CDefMiniCollectionModel.MiniCollectionMenuModel dstMenu = dstKvp.Value;
                            dstName = GenericController.vbLCase(dstMenu.Name);
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
                        updateDst |= !textMatch(dstMenuMatch.Name, srcMenu.Name);
                        updateDst |= (dstMenuMatch.NewWindow != srcMenu.NewWindow);
                        updateDst |= !textMatch(dstMenuMatch.SortOrder, srcMenu.SortOrder);
                        updateDst |= !textMatch(dstMenuMatch.AddonName, srcMenu.AddonName);
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
                //                If SrcIsUserCDef Then
                //                    '
                //                    ' test for cdef attribute changes
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
                //            ' CDef was not found, add it
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
                //                .CDefChanged = True
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
                        // CDef was not found, add it
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
        private static CDefMiniCollectionModel getApplicationCDefMiniCollection(CoreController core, bool isNewBuild, string logPrefix, ref List<string> installedCollections) {
            var result = new CDefMiniCollectionModel();
            try {
                if (!isNewBuild) {
                    //
                    // if this is not an empty database, get the application collection, else return empty
                    string applicationCDefMiniCollectionXml = ApplicationCDefMiniCollection.get(core, true);
                    result = CDefMiniCollectionModel.installCDefMiniCollection_LoadXml(core, applicationCDefMiniCollectionXml, false, false, isNewBuild, new CDefMiniCollectionModel(), logPrefix, ref installedCollections);
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
        /// Update a table from a collection cdef node
        /// </summary>
        internal static void installCDefMiniCollection_buildDb_saveCDefToDb(CoreController core, Models.Domain.CDefModel cdef, string BuildVersion) {
            try {
                //
                LogController.logInfo(core, "Update db cdef [" + cdef.name + "]");
                //
                // -- get contentid and protect content with IsBaseContent true
                {
                    if (cdef.dataChanged) {
                        //
                        // -- update definition (use SingleRecord as an update flag)
                        var datasource = DataSourceModel.createByUniqueName(core, cdef.dataSourceName);
                        CdefController.verifyContent_returnId(core, cdef);
                    }
                    //
                    // -- update Content Field Records and Content Field Help records
                    CDefModel cdefFieldHelp = CDefModel.create(core, ContentFieldHelpModel.contentName);
                    foreach (var nameValuePair in cdef.fields) {
                        CDefFieldModel field = nameValuePair.Value;
                        int fieldId = 0;
                        if (field.dataChanged) {
                            fieldId = CdefController.verifyContentField_returnID(core, cdef.name, field);
                        }
                        //
                        // -- update content field help records
                        if (field.HelpChanged) {
                            //int FieldHelpID = 0;
                            ContentFieldHelpModel fieldHelp = null;
                            var fieldHelpList = ContentFieldHelpModel.createList(core, "fieldid=" + fieldId);
                            if (fieldHelpList.Count == 0) {
                                //
                                // -- no current field help record, if adding help, create record
                                if ((!string.IsNullOrWhiteSpace(field.helpDefault)) || (!string.IsNullOrWhiteSpace(field.helpCustom))) {
                                    fieldHelp = ContentFieldHelpModel.addEmpty(core);
                                    fieldHelp.helpDefault = field.helpDefault;
                                    fieldHelp.helpCustom = field.helpCustom;
                                    fieldHelp.save(core);

                                }
                            } else {
                                //
                                // -- if help changed, save it
                                fieldHelp = fieldHelpList.First();
                                if ((!fieldHelp.helpCustom.Equals(field.helpCustom)) || !fieldHelp.helpDefault.Equals(field.helpDefault)) {
                                    fieldHelp.helpDefault = field.helpDefault;
                                    fieldHelp.helpCustom = field.helpCustom;
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
        private static string GetMenuNameSpace(CoreController core, Dictionary<string, CDefMiniCollectionModel.MiniCollectionMenuModel> menus, CDefMiniCollectionModel.MiniCollectionMenuModel menu, string UsedIDList) {
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
                        CDefMiniCollectionModel.MiniCollectionMenuModel testMenu = kvp.Value;
                        if (GenericController.vbInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0) {
                            if (LCaseParentName == GenericController.vbLCase(testMenu.Name) && (menu.IsNavigator == testMenu.IsNavigator)) {
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
