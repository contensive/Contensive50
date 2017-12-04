

using System.Xml;
using Controllers;

using Models;
using Models.Entity;
// 

namespace Contensive.Core {
    
    public class addonInstallClass {
        
        // 
        // ============================================================================
        //    process the include add-on node of the add-on nodes
        //        this is the second pass, so all add-ons should be added
        //        no errors for missing addones, except the include add-on case
        // ============================================================================
        // 
        private static string InstallCollectionFromLocalRepo_addonNode_Phase2(coreClass cpCore, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage) {
            string result = "";
            try {
                bool AddRule;
                string IncludeAddonName;
                string IncludeAddonGuid;
                int IncludeAddonID;
                string UserError;
                int CS2;
                XmlNode PageInterface;
                bool NavDeveloperOnly;
                string StyleSheet;
                string ArgumentList;
                int CS;
                string Criteria;
                bool IsFound;
                string AOName;
                string AOGuid;
                string AddOnType;
                int addonId;
                string Basename;
                // 
                Basename = genericController.vbLCase(AddonNode.Name);
                if (((Basename == "page") 
                            || ((Basename == "process") 
                            || ((Basename == "addon") 
                            || (Basename == "add-on"))))) {
                    AOName = addonInstallClass.GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name");
                    if ((AOName == "")) {
                        AOName = "No Name";
                    }
                    
                    AOGuid = addonInstallClass.GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", AOName);
                    if ((AOGuid == "")) {
                        AOGuid = AOName;
                    }
                    
                    AddOnType = addonInstallClass.GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "");
                    Criteria = ("(" 
                                + (AddonGuidFieldName + ("=" 
                                + (cpCore.db.encodeSQLText(AOGuid) + ")"))));
                    CS = cpCore.db.csOpen(cnAddons, Criteria, ,, false);
                    if (cpCore.db.csOk(CS)) {
                        // 
                        //  Update the Addon
                        // 
                        logController.appendInstallLog(cpCore, ("UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" 
                                        + (AOName + ("], Guid [" 
                                        + (AOGuid + "]")))));
                    }
                    else {
                        // 
                        //  not found by GUID - search name against name to update legacy Add-ons
                        // 
                        cpCore.db.csClose(CS);
                        Criteria = ("(name=" 
                                    + (cpCore.db.encodeSQLText(AOName) + (")and(" 
                                    + (AddonGuidFieldName + " is null)"))));
                        CS = cpCore.db.csOpen(cnAddons, Criteria, ,, false);
                    }
                    
                    if (!cpCore.db.csOk(CS)) {
                        // 
                        //  Could not find add-on
                        // 
                        logController.appendInstallLog(cpCore, ("UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" 
                                        + (AOName + ("], Guid [" 
                                        + (AOGuid + "]")))));
                    }
                    else {
                        addonId = cpCore.db.csGetInteger(CS, "ID");
                        ArgumentList = "";
                        StyleSheet = "";
                        NavDeveloperOnly = true;
                        if ((AddonNode.ChildNodes.Count > 0)) {
                            foreach (PageInterface in AddonNode.ChildNodes) {
                                switch (genericController.vbLCase(PageInterface.Name)) {
                                    case "includeaddon":
                                    case "includeadd-on":
                                    case "include addon":
                                    case "include add-on":
                                        if (true) {
                                            IncludeAddonName = addonInstallClass.GetXMLAttribute(cpCore, IsFound, PageInterface, "name", "");
                                            IncludeAddonGuid = addonInstallClass.GetXMLAttribute(cpCore, IsFound, PageInterface, "guid", IncludeAddonName);
                                            IncludeAddonID = 0;
                                            Criteria = "";
                                            if ((IncludeAddonGuid != "")) {
                                                Criteria = (AddonGuidFieldName + ("=" + cpCore.db.encodeSQLText(IncludeAddonGuid)));
                                                if ((IncludeAddonName == "")) {
                                                    IncludeAddonName = ("Add-on " + IncludeAddonGuid);
                                                }
                                                
                                            }
                                            else if ((IncludeAddonName != "")) {
                                                Criteria = ("(name=" 
                                                            + (cpCore.db.encodeSQLText(IncludeAddonName) + ")"));
                                            }
                                            
                                            if ((Criteria != "")) {
                                                CS2 = cpCore.db.csOpen(cnAddons, Criteria);
                                                if (cpCore.db.csOk(CS2)) {
                                                    IncludeAddonID = cpCore.db.csGetInteger(CS2, "ID");
                                                }
                                                
                                                cpCore.db.csClose(CS2);
                                                AddRule = false;
                                                if ((IncludeAddonID == 0)) {
                                                    UserError = ("The include add-on [" 
                                                                + (IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must ap" +
                                                                "pear before any add-ons that include it."));
                                                    logController.appendInstallLog(cpCore, ("UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" 
                                                                    + (UserError + "]")));
                                                    ReturnUpgradeOK = false;
                                                    ReturnErrorMessage = (ReturnErrorMessage + ("<P>The collection was not installed because the add-on [" 
                                                                + (AOName + ("] requires an included add-on [" 
                                                                + (IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add" +
                                                                "-ons that include it.</P>")))));
                                                }
                                                else {
                                                    CS2 = cpCore.db.csOpenSql_rev("default", ("select ID from ccAddonIncludeRules where Addonid=" 
                                                                    + (addonId + (" and IncludedAddonID=" + IncludeAddonID))));
                                                    AddRule = !cpCore.db.csOk(CS2);
                                                    cpCore.db.csClose(CS2);
                                                }
                                                
                                                if (AddRule) {
                                                    CS2 = cpCore.db.csInsertRecord("Add-on Include Rules", 0);
                                                    if (cpCore.db.csOk(CS2)) {
                                                        cpCore.db.csSet(CS2, "Addonid", addonId);
                                                        cpCore.db.csSet(CS2, "IncludedAddonID", IncludeAddonID);
                                                    }
                                                    
                                                    cpCore.db.csClose(CS2);
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                        break;
                                }
                            }
                            
                        }
                        
                    }
                    
                    cpCore.db.csClose(CS);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
            // 
        }
        
        // '
        // '===========================================================================
        // '   Append Log File
        // '===========================================================================
        // '
        // Private Shared Sub logcontroller.appendInstallLog(cpCore As coreClass, ByVal ignore As String, ByVal Method As String, ByVal LogMessage As String)
        //     Try
        //         Console.WriteLine("install, " & LogMessage)
        //         logController.appendLog(cpCore, LogMessage, "install")
        //     Catch ex As Exception
        //         cpCore.handleException(ex)
        //     End Try
        // End Sub
        // 
        // =========================================================================================
        //    Import CDef on top of current configuration and the base configuration
        // 
        // =========================================================================================
        // 
        public static void installBaseCollection(coreClass cpCore, bool isNewBuild, ref List<string> nonCriticalErrorList) {
            try {
                string ignoreString = "";
                string returnErrorMessage = "";
                bool ignoreBoolean = false;
                bool isBaseCollection = true;
                string baseCollectionXml = cpCore.programFiles.readFile("aoBase5.xml");
                if (string.IsNullOrEmpty(baseCollectionXml)) {
                    // 
                    //  -- base collection notfound
                    throw new ApplicationException(("Cannot load aoBase5.xml [" 
                                    + (cpCore.programFiles.rootLocalPath + "aoBase5.xml]")));
                }
                else {
                    logController.appendInstallLog(cpCore, "Verify base collection -- new build");
                    miniCollectionModel baseCollection = addonInstallClass.installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, true, true, isNewBuild, new miniCollectionModel());
                    addonInstallClass.installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList);
                    // If isNewBuild Then
                    //     '
                    //     ' -- new build
                    //     Call logcontroller.appendInstallLog(cpCore,  "Verify base collection -- new build")
                    //     Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    //     Call installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    //     'Else
                    //     '    '
                    //     '    ' -- verify current build
                    //     '    Call logcontroller.appendInstallLog(cpCore,  "Verify base collection - existing build")
                    //     '    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    //     '    Dim workingCollection As miniCollectionModel = installCollection_GetApplicationMiniCollection(cpCore, False)
                    //     '    Call installCollection_AddMiniCollectionSrcToDst(cpCore, workingCollection, baseCollection, False)
                    //     '    Call installCollection_BuildDbFromMiniCollection(cpCore, workingCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    // End If
                    // 
                    //  now treat as a regular collection and install - to pickup everything else 
                    // 
                    string tmpFolderPath = ("tmp" 
                                + (genericController.GetRandomInteger().ToString + "\\"));
                    cpCore.privateFiles.createPath(tmpFolderPath);
                    cpCore.programFiles.copyFile("aoBase5.xml", (tmpFolderPath + "aoBase5.xml"), cpCore.privateFiles);
                    List<string> ignoreList = new List<string>();
                    if (!InstallCollectionsFromPrivateFolder(cpCore, tmpFolderPath, returnErrorMessage, ignoreList, isNewBuild, nonCriticalErrorList)) {
                        throw new ApplicationException(returnErrorMessage);
                    }
                    
                    cpCore.privateFiles.DeleteFileFolder(tmpFolderPath);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // =========================================================================================
        // 
        // =========================================================================================
        // 
        public static void installCollectionFromLocalRepo_BuildDbFromXmlData(coreClass cpCore, string XMLText, bool isNewBuild, bool isBaseCollection, ref List<string> nonCriticalErrorList) {
            try {
                // 
                logController.appendInstallLog(cpCore, ("Application: " + cpCore.serverConfig.appConfig.name));
                // 
                //  ----- Import any CDef files, allowing for changes
                miniCollectionModel miniCollectionToAdd = new miniCollectionModel();
                miniCollectionModel miniCollectionWorking = addonInstallClass.installCollection_GetApplicationMiniCollection(cpCore, isNewBuild);
                miniCollectionToAdd = addonInstallClass.installCollection_LoadXmlToMiniCollection(cpCore, XMLText, isBaseCollection, false, isNewBuild, miniCollectionWorking);
                addonInstallClass.installCollection_AddMiniCollectionSrcToDst(cpCore, miniCollectionWorking, miniCollectionToAdd, true);
                addonInstallClass.installCollection_BuildDbFromMiniCollection(cpCore, miniCollectionWorking, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        //         '
        //         '=========================================================================================
        //         '
        //         '=========================================================================================
        //         '
        //         Private Sub UpgradeCDef_LoadFileToCollection(ByVal FilePathPage As String, byref return_Collection As CollectionType, ByVal ForceChanges As Boolean, IsNewBuild As Boolean)
        //             On Error GoTo ErrorTrap
        //             '
        //             'Dim fs As New fileSystemClass
        //             Dim IsccBaseFile As Boolean
        //             '
        //             IsccBaseFile = (InStr(1, FilePathPage, "ccBase.xml", vbTextCompare) <> 0)
        //             Call AppendClassLogFile(cpcore.app.config.name, "UpgradeCDef_LoadFileToCollection", "Application: " & cpcore.app.config.name & ", loading [" & FilePathPage & "]")
        //             Call UpgradeCDef_LoadDataToCollection(cpcore.app.publicFiles.ReadFile(FilePathPage), Return_Collection, IsccBaseFile, ForceChanges, IsNewBuild)
        //             '
        //             Exit Sub
        //             '
        //             ' ----- Error Trap
        //             '
        // ErrorTrap:
        //             Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection", True, True)
        //             'dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection hint=[" & Hint & "]", True, True)
        //             Resume Next
        //         End Sub
        // 
        // =========================================================================================
        //    create a collection class from a collection xml file
        //        - cdef are added to the cdefs in the application collection
        // =========================================================================================
        // 
        private static miniCollectionModel installCollection_LoadXmlToMiniCollection(coreClass cpCore, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, miniCollectionModel defaultCollection) {
            miniCollectionModel result;
            try {
                Models.Complex.cdefModel DefaultCDef;
                Models.Complex.CDefFieldModel DefaultCDefField;
                string contentNameLc;
                xmlController XMLTools = new xmlController(cpCore);
                // Dim AddonClass As New addonInstallClass(cpCore)
                string status;
                string CollectionGuid;
                string Collectionname;
                string ContentTableName;
                bool IsNavigator;
                string ActiveText;
                string Name = String.Empty;
                string MenuName;
                string IndexName;
                string TableName;
                int Ptr;
                string FieldName;
                string ContentName;
                bool Found;
                string menuNameSpace;
                string MenuGuid;
                string MenuKey;
                XmlNode CDef_Node;
                XmlNode CDefChildNode;
                string DataSourceName;
                XmlDocument srcXmlDom = new XmlDocument();
                string NodeName;
                XmlNode FieldChildNode;
                // 
                logController.appendInstallLog(cpCore, ("Application: " 
                                + (cpCore.serverConfig.appConfig.name + ", UpgradeCDef_LoadDataToCollection")));
                // 
                result = new miniCollectionModel();
                // 
                if (string.IsNullOrEmpty(srcCollecionXml)) {
                    // 
                    //  -- empty collection is an error
                    throw new ApplicationException("UpgradeCDef_LoadDataToCollection, srcCollectionXml is blank or null");
                }
                else {
                    try {
                        srcXmlDom.LoadXml(srcCollecionXml);
                    }
                    catch (Exception ex) {
                        // 
                        //  -- xml load error
                        logController.appendLog(cpCore, ("UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" 
                                        + (ex.ToString + "]")));
                        throw new Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    
                    // With...
                    if (((srcXmlDom.DocumentElement.Name.ToLower() != CollectionFileRootNode) 
                                && (srcXmlDom.DocumentElement.Name.ToLower() != "contensivecdef"))) {
                        // 
                        //  -- root node must be collection (or legacy contensivecdef)
                        cpCore.handleException(new ApplicationException("the archive file has a syntax error. Application name must be the first node."));
                    }
                    else {
                        result.isBaseCollection = IsccBaseFile;
                        // 
                        //  Get Collection Name for logs
                        // 
                        // hint = "get collection name"
                        Collectionname = addonInstallClass.GetXMLAttribute(cpCore, Found, srcXmlDom.DocumentElement, "name", "");
                        if ((Collectionname == "")) {
                            logController.appendInstallLog(cpCore, ("UpgradeCDef_LoadDataToCollection, Application: " 
                                            + (cpCore.serverConfig.appConfig.name + ", Collection has no name")));
                        }
                        else {
                            // Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & cpcore.app.appEnvironment.name & ", Collection: " & Collectionname)
                        }
                        
                        result.name = Collectionname;
                        // '
                        // ' Load possible DefaultSortMethods
                        // '
                        // 'hint = "preload sort methods"
                        // SortMethodList = vbTab & "By Name" & vbTab & "By Alpha Sort Order Field" & vbTab & "By Date" & vbTab & "By Date Reverse"
                        // If cpCore.app.csv_IsContentFieldSupported("Sort Methods", "ID") Then
                        //     CS = cpCore.app.OpenCSContent("Sort Methods", , , , , , , "Name")
                        //     Do While cpCore.app.IsCSOK(CS)
                        //         SortMethodList = SortMethodList & vbTab & cpCore.app.cs_getText(CS, "name")
                        //         cpCore.app.nextCSRecord(CS)
                        //     Loop
                        //     Call cpCore.app.closeCS(CS)
                        // End If
                        // SortMethodList = SortMethodList & vbTab
                        // 
                        foreach (CDef_Node in srcXmlDom.DocumentElement.ChildNodes) {
                            // isCdefTarget = False
                            NodeName = genericController.vbLCase(CDef_Node.Name);
                            // hint = "read node " & NodeName
                            switch (NodeName) {
                                case "cdef":
                                    ContentName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "name", "");
                                    contentNameLc = genericController.vbLCase(ContentName);
                                    if ((ContentName == "")) {
                                        throw new ApplicationException("Unexpected exception");
                                    }
                                    else {
                                        // 
                                        //  setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
                                        // 
                                        if (defaultCollection.CDef.ContainsKey(contentNameLc)) {
                                            DefaultCDef = defaultCollection.CDef(contentNameLc);
                                        }
                                        else {
                                            DefaultCDef = new Models.Complex.cdefModel();
                                        }
                                        
                                        // 
                                        ContentTableName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ContentTableName", DefaultCDef.ContentTableName);
                                        if ((ContentTableName != "")) {
                                            // 
                                            //  These two fields are needed to import the row
                                            // 
                                            DataSourceName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "dataSource", DefaultCDef.ContentDataSourceName);
                                            if ((DataSourceName == "")) {
                                                DataSourceName = "Default";
                                            }
                                            
                                            // 
                                            //  ----- Add CDef if not already there
                                            // 
                                            if (!result.CDef.ContainsKey(ContentName.ToLower)) {
                                                result.CDef.Add(ContentName.ToLower, new Models.Complex.cdefModel());
                                            }
                                            
                                            // 
                                            //  Get CDef attributes
                                            // 
                                            // With...
                                            string activeDefaultText = "1";
                                            if (!DefaultCDef.Active) {
                                                activeDefaultText = "0";
                                            }
                                            
                                            ActiveText = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Active", activeDefaultText);
                                            if ((ActiveText == "")) {
                                                ActiveText = "1";
                                            }
                                            
                                            setAllDataChanged.childIdList(cpCore) = new List<int>();
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "guid", DefaultCDef.guid).dataChanged = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowTopicRules", DefaultCDef.AllowTopicRules).guid = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowDelete", DefaultCDef.AllowDelete).AllowTopicRules = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowContentTracking", DefaultCDef.AllowContentTracking).AllowDelete = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowContentChildTool", DefaultCDef.AllowContentChildTool).AllowContentTracking = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowCalendarEvents", DefaultCDef.AllowCalendarEvents).AllowContentChildTool = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowAdd", DefaultCDef.AllowAdd).AllowCalendarEvents = new List<int>();
                                            "name".AllowAdd = new List<int>();
                                            "id".AliasName = new List<int>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AdminOnly", DefaultCDef.AdminOnly).AliasID = new List<int>();
                                            true.AdminOnly = new List<int>();
                                            genericController.EncodeBoolean(ActiveText).ActiveOnly = new List<int>();
                                            result.CDef(ContentName.ToLower).Active = new List<int>();
                                            0.DefaultSortMethod = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ContentTableName", DefaultCDef.ContentTableName).dataSourceId = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ContentDataSourceName", DefaultCDef.ContentDataSourceName).ContentTableName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            "".ContentDataSourceName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            result.CDef(ContentName.ToLower).ContentControlCriteria = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            if (((result.CDef(ContentName.ToLower).DefaultSortMethod == "") 
                                                        || (result.CDef(ContentName.ToLower).DefaultSortMethod.ToLower() == "name"))) {
                                                result.CDef(ContentName.ToLower).DefaultSortMethod = "By Name";
                                            }
                                            else if ((genericController.vbLCase(result.CDef(ContentName.ToLower).DefaultSortMethod) == "sortorder")) {
                                                result.CDef(ContentName.ToLower).DefaultSortMethod = "By Alpha Sort Order Field";
                                            }
                                            else if ((genericController.vbLCase(result.CDef(ContentName.ToLower).DefaultSortMethod) == "date")) {
                                                result.CDef(ContentName.ToLower).DefaultSortMethod = "By Date";
                                            }
                                            
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "EditorGroupName", DefaultCDef.EditorGroupName).fields = new Dictionary<string, Models.Complex.CDefFieldModel>();
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DropDownFieldList", DefaultCDef.DropDownFieldList).EditorGroupName = new Dictionary<string, Models.Complex.CDefFieldModel>();
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "DeveloperOnly", DefaultCDef.DeveloperOnly).DropDownFieldList = new Dictionary<string, Models.Complex.CDefFieldModel>();
                                            result.CDef(ContentName.ToLower).DeveloperOnly = new Dictionary<string, Models.Complex.CDefFieldModel>();
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Parent", DefaultCDef.parentName).WhereClause = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            ContentName.parentName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IsModified", DefaultCDef.IsModifiedSinceInstalled).Name = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            (IsccBaseFile | addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IsBaseContent", false).IsModifiedSinceInstalled) = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "installedByCollection", DefaultCDef.installedByCollectionGuid).IsBaseContent = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            false.installedByCollectionGuid = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IgnoreContentControl", DefaultCDef.IgnoreContentControl).includesAFieldChange = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconSprites", DefaultCDef.IconSprites).IgnoreContentControl = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconWidth", DefaultCDef.IconWidth).IconSprites = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconHeight", DefaultCDef.IconHeight).IconWidth = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "IconLink", DefaultCDef.IconLink).IconHeight = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            result.CDef(ContentName.ToLower).IconLink = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause);
                                            // 
                                            //  Get CDef field nodes
                                            // 
                                            foreach (CDefChildNode in CDef_Node.ChildNodes) {
                                                // 
                                                //  ----- process CDef Field
                                                // 
                                                if (addonInstallClass.TextMatch(cpCore, CDefChildNode.Name, "field")) {
                                                    FieldName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "Name", "");
                                                    if ((FieldName.ToLower == "middlename")) {
                                                        FieldName = FieldName;
                                                    }
                                                    
                                                    // 
                                                    //  try to find field in the defaultcdef
                                                    // 
                                                    if (DefaultCDef.fields.ContainsKey(FieldName)) {
                                                        DefaultCDefField = DefaultCDef.fields(FieldName);
                                                    }
                                                    else {
                                                        DefaultCDefField = new Models.Complex.CDefFieldModel();
                                                    }
                                                    
                                                    // 
                                                    if (!result.CDef(ContentName.ToLower).fields.ContainsKey(FieldName.ToLower())) {
                                                        result.CDef(ContentName.ToLower).fields.Add(FieldName.ToLower, new Models.Complex.CDefFieldModel());
                                                    }
                                                    
                                                    // With...
                                                    ActiveText = "0";
                                                    if (DefaultCDefField.active) {
                                                        ActiveText = "1";
                                                    }
                                                    
                                                    ActiveText = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "Active", ActiveText);
                                                    if ((ActiveText == "")) {
                                                        ActiveText = "1";
                                                    }
                                                    
                                                    active = genericController.EncodeBoolean(ActiveText);
                                                    // 
                                                    //  Convert Field Descriptor (text) to field type (integer)
                                                    // 
                                                    string defaultFieldTypeName = cpCore.db.getFieldTypeNameFromFieldTypeId(DefaultCDefField.fieldTypeId);
                                                    string fieldTypeName;
                                                    // FieldTypeDescriptor = GetXMLAttribute(cpcore,Found, CDefChildNode, "FieldType", DefaultCDefField.fieldType)
                                                    // If genericController.vbIsNumeric(FieldTypeDescriptor) Then
                                                    //     .fieldType = genericController.EncodeInteger(FieldTypeDescriptor)
                                                    // Else
                                                    //     .fieldType = cpCore.app.csv_GetFieldTypeByDescriptor(FieldTypeDescriptor)
                                                    // End If
                                                    // If .fieldType = 0 Then
                                                    //     .fieldType = FieldTypeText
                                                    // End If
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleSecondaryField", DefaultCDefField.ManyToManyRuleSecondaryField).lookupContentName(cpCore) = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRulePrimaryField", DefaultCDefField.ManyToManyRulePrimaryField).ManyToManyRuleSecondaryField = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupList", DefaultCDefField.lookupList).ManyToManyRulePrimaryField = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Scramble", DefaultCDefField.Scramble).lookupList = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "EditTab", DefaultCDefField.editTabName).Scramble = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "MemberSelectGroupID", DefaultCDefField.MemberSelectGroupID).editTabName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSDescriptionField", DefaultCDefField.RSSDescriptionField).MemberSelectGroupID = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSTitle", DefaultCDefField.RSSTitleField).RSSDescriptionField = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Required", DefaultCDefField.Required).RSSTitleField = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "ReadOnly", DefaultCDefField.ReadOnly).Required = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "DeveloperOnly", DefaultCDefField.developerOnly).ReadOnly = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "AdminOnly", DefaultCDefField.adminOnly).developerOnly = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Password", DefaultCDefField.Password).adminOnly = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "UniqueName", DefaultCDefField.UniqueName).Password = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "HTMLContent", DefaultCDefField.htmlContent).UniqueName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectPath", DefaultCDefField.RedirectPath).htmlContent = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectID", DefaultCDefField.RedirectID).RedirectPath = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexSortOrder", DefaultCDefField.indexSortOrder).RedirectID = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "IndexWidth", DefaultCDefField.indexWidth).indexSortOrder = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexColumn", DefaultCDefField.indexColumn).indexWidth = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "NotEditable", DefaultCDefField.NotEditable).indexColumn = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "DefaultValue", DefaultCDefField.defaultValue).NotEditable = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "Caption", DefaultCDefField.caption).defaultValue = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Authorable", DefaultCDefField.authorable).caption = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    addonInstallClass.GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "EditSortPriority", DefaultCDefField.editSortPriority).authorable = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    editSortPriority = addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
                                                    //  isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                    //  case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                    //  case 2 - when the base file is loaded, all fields must include the attribute
                                                    // Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid).dataChanged = setAllDataChanged;
                                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled).installedByCollectionGuid = setAllDataChanged;
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.ManyToManyRuleContentName(cpCore)).isModifiedSinceInstalled = setAllDataChanged;
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.ManyToManyContentName(cpCore)).ManyToManyRuleContentName(cpCore) = setAllDataChanged;
                                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectContent", DefaultCDefField.RedirectContentName(cpCore)).ManyToManyContentName(cpCore) = setAllDataChanged;
                                                    (addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsBaseField", false) | IsccBaseFile.RedirectContentName(cpCore)) = setAllDataChanged;
                                                    isBaseField = setAllDataChanged;
                                                    // 
                                                    //  ----- handle child nodes (help node)
                                                    // 
                                                    "".HelpDefault = "";
                                                    HelpCustom = "";
                                                    foreach (FieldChildNode in CDefChildNode.ChildNodes) {
                                                        // 
                                                        //  ----- process CDef Field
                                                        // 
                                                        if (addonInstallClass.TextMatch(cpCore, FieldChildNode.Name, "HelpDefault")) {
                                                            HelpDefault = FieldChildNode.InnerText;
                                                        }
                                                        
                                                        if (addonInstallClass.TextMatch(cpCore, FieldChildNode.Name, "HelpCustom")) {
                                                            HelpCustom = FieldChildNode.InnerText;
                                                        }
                                                        
                                                        HelpChanged = setAllDataChanged;
                                                    }
                                                    
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    break;
                                case "sqlindex":
                                    // With...
                                    IndexName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "indexname", "");
                                    TableName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "TableName", "");
                                    DataSourceName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "DataSourceName", "");
                                    if ((DataSourceName == "")) {
                                        DataSourceName = "default";
                                    }
                                    
                                    if ((result.SQLIndexCnt > 0)) {
                                        for (Ptr = 0; (Ptr 
                                                    <= (result.SQLIndexCnt - 1)); Ptr++) {
                                            if ((addonInstallClass.TextMatch(cpCore, result.SQLIndexes, Ptr.IndexName, IndexName) 
                                                        && (addonInstallClass.TextMatch(cpCore, result.SQLIndexes, Ptr.TableName, TableName) && addonInstallClass.TextMatch(cpCore, result.SQLIndexes, Ptr.DataSourceName, DataSourceName)))) {
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if ((Ptr >= result.SQLIndexCnt)) {
                                        result.SQLIndexCnt.SQLIndexCnt = (result.SQLIndexCnt + 1);
                                        Ptr = (result.SQLIndexCnt + 1);
                                        object Preserve.SQLIndexes;
                                        result.SQLIndexes;
                                        TableName.SQLIndexes(Ptr).DataSourceName = DataSourceName;
                                        IndexName.SQLIndexes(Ptr).TableName = DataSourceName;
                                        Ptr.IndexName = DataSourceName;
                                    }
                                    
                                    // With...
                                    Ptr.FieldNameList = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "FieldNameList", "");
                                    break;
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry":
                                    MenuName = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "");
                                    menuNameSpace = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "NameSpace", "");
                                    MenuGuid = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "");
                                    IsNavigator = (NodeName == "navigatorentry");
                                    if (!IsNavigator) {
                                        MenuKey = genericController.vbLCase(MenuName);
                                    }
                                    else if (false) {
                                        MenuKey = genericController.vbLCase(("nav." 
                                                        + (menuNameSpace + ("." + MenuName))));
                                    }
                                    else {
                                        MenuKey = MenuGuid;
                                    }
                                    
                                    // With...
                                    // 
                                    //  Go through all current menus and check for duplicates
                                    // 
                                    if ((result.MenuCnt > 0)) {
                                        for (Ptr = 0; (Ptr 
                                                    <= (result.MenuCnt - 1)); Ptr++) {
                                            //  1/16/2009 - JK - empty keys should not be allowed
                                            if (result.Menus) {
                                                (Ptr.Key != "");
                                                if (addonInstallClass.TextMatch(cpCore, result.Menus, Ptr.Key, MenuKey)) {
                                                    break;
                                                }
                                                
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if ((Ptr >= result.MenuCnt)) {
                                        // 
                                        //  Add new entry
                                        // 
                                        result.MenuCnt.MenuCnt = (result.MenuCnt + 1);
                                        Ptr = (result.MenuCnt + 1);
                                        object Preserve.Menus;
                                    }
                                    
                                    // With...
                                    Ptr;
                                    ActiveText = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Active", "1");
                                    if ((ActiveText == "")) {
                                        ActiveText = "1";
                                    }
                                    
                                    // 
                                    //  Update Entry
                                    // 
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "NavIconTitle", "").IsNavigator = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "NavIconType", "").NavIconTitle = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "AddonName", "").NavIconType = IsNavigator;
                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "NewWindow", false).AddonName = IsNavigator;
                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "DeveloperOnly", false).NewWindow = IsNavigator;
                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AdminOnly", false).DeveloperOnly = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "SortOrder", "").AdminOnly = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "LinkPage", "").SortOrder = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ContentName", "").LinkPage = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ParentName", "").ContentName = IsNavigator;
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "NameSpace", "").ParentName = IsNavigator;
                                    genericController.EncodeBoolean(ActiveText).menuNameSpace = IsNavigator;
                                    MenuKey.Active = IsNavigator;
                                    MenuGuid.Key = IsNavigator;
                                    MenuName.Guid = IsNavigator;
                                    setAllDataChanged.Name = IsNavigator;
                                    result.Menus.dataChanged = IsNavigator;
                                    break;
                                case "aggregatefunction":
                                case "addon":
                                    Name = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "");
                                    // With...
                                    if ((result.AddOnCnt > 0)) {
                                        for (Ptr = 0; (Ptr 
                                                    <= (result.AddOnCnt - 1)); Ptr++) {
                                            if (addonInstallClass.TextMatch(cpCore, result.AddOns, Ptr.Name, Name)) {
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if ((Ptr >= result.AddOnCnt)) {
                                        result.AddOnCnt.AddOnCnt = (result.AddOnCnt + 1);
                                        Ptr = (result.AddOnCnt + 1);
                                        object Preserve.AddOns;
                                        result.AddOns;
                                        Ptr.Name = Name;
                                    }
                                    
                                    // With...
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "SortOrder", "").Copy = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ArgumentList", "").SortOrder = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "ObjectProgramID", "").ArgumentList = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Link", "").ObjectProgramID = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    setAllDataChanged.Link = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    Ptr.dataChanged = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "");
                                    result.AddOns;
                                    Ptr.Copy = CDef_Node.InnerText;
                                    break;
                                case "style":
                                    Name = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "");
                                    // With...
                                    if ((result.StyleCnt > 0)) {
                                        for (Ptr = 0; (Ptr 
                                                    <= (result.StyleCnt - 1)); Ptr++) {
                                            if (addonInstallClass.TextMatch(cpCore, result.Styles, Ptr.Name, Name)) {
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if ((Ptr >= result.StyleCnt)) {
                                        result.StyleCnt.StyleCnt = (result.StyleCnt + 1);
                                        Ptr = (result.StyleCnt + 1);
                                        object Preserve.Styles;
                                        result.Styles;
                                        Ptr.Name = Name;
                                    }
                                    
                                    // With...
                                    addonInstallClass.GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "Overwrite", false).Copy = CDef_Node.InnerText;
                                    setAllDataChanged.Overwrite = CDef_Node.InnerText;
                                    Ptr.dataChanged = CDef_Node.InnerText;
                                    break;
                                case "stylesheet":
                                    result.StyleSheet = CDef_Node.InnerText;
                                    break;
                                case "getcollection":
                                case "importcollection":
                                    if (true) {
                                        // If Not UpgradeDbOnly Then
                                        // 
                                        //  Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                        // 
                                        Collectionname = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "name", "");
                                        CollectionGuid = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "");
                                        if ((CollectionGuid == "")) {
                                            CollectionGuid = CDef_Node.InnerText;
                                        }
                                        
                                        if ((CollectionGuid == "")) {
                                            status = ("The collection you selected [" 
                                                        + (Collectionname + "] can not be downloaded because it does not include a valid GUID."));
                                        }
                                        else {
                                            object Preserve;
                                            result.collectionImports(result.ImportCnt);
                                            result.collectionImports(result.ImportCnt).Guid = CollectionGuid;
                                            result.collectionImports(result.ImportCnt).Name = Collectionname;
                                            result.ImportCnt = (result.ImportCnt + 1);
                                        }
                                        
                                    }
                                    
                                    break;
                                case "pagetemplate":
                                    // With...
                                    if ((result.PageTemplateCnt > 0)) {
                                        for (Ptr = 0; (Ptr 
                                                    <= (result.PageTemplateCnt - 1)); Ptr++) {
                                            if (addonInstallClass.TextMatch(cpCore, result.PageTemplates, Ptr.Name, Name)) {
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    if ((Ptr >= result.PageTemplateCnt)) {
                                        result.PageTemplateCnt.PageTemplateCnt = (result.PageTemplateCnt + 1);
                                        Ptr = (result.PageTemplateCnt + 1);
                                        object Preserve.PageTemplates;
                                        result.PageTemplates;
                                        Ptr.Name = Name;
                                    }
                                    
                                    // With...
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "").Style = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "style", "");
                                    addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "Copy", "").Guid = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "style", "");
                                    Ptr.Copy = addonInstallClass.GetXMLAttribute(cpCore, Found, CDef_Node, "style", "");
                                    // Case "sitesection"
                                    //     '
                                    //     '-------------------------------------------------------------------------------------------------
                                    //     ' Site Sections
                                    //     '-------------------------------------------------------------------------------------------------
                                    //     '
                                    // Case "dynamicmenu"
                                    //     '
                                    //     '-------------------------------------------------------------------------------------------------
                                    //     ' Dynamic Menus
                                    //     '-------------------------------------------------------------------------------------------------
                                    //     '
                                    break;
                                case "pagecontent":
                                    break;
                                case "copycontent":
                                    break;
                            }
                        }
                        
                        // hint = "nodes done"
                        // 
                        //  Convert Menus.ParentName to Menu.menuNameSpace
                        // 
                        // With...
                        if ((result.MenuCnt > 0)) {
                            for (Ptr = 0; (Ptr 
                                        <= (result.MenuCnt - 1)); Ptr++) {
                                if (result.Menus) {
                                    (Ptr.ParentName != "");
                                    result.Menus;
                                    Ptr.menuNameSpace = addonInstallClass.GetMenuNameSpace(cpCore, result, Ptr, result.Menus, Ptr.IsNavigator, "");
                                    // .Menus(Ptr).ParentName = ""
                                    Ptr = Ptr;
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ========================================================================
        // '' <summary>
        // '' Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
        // '' </summary>
        // '' <param name="Collection"></param>
        // '' <param name="return_IISResetRequired"></param>
        // '' <param name="BuildVersion"></param>
        private static void installCollection_BuildDbFromMiniCollection(coreClass cpCore, miniCollectionModel Collection, string BuildVersion, bool isNewBuild, ref List<string> nonCriticalErrorList) {
            try {
                // 
                int FieldHelpID;
                int FieldHelpCID;
                int fieldId;
                string FieldName;
                // Dim AddonClass As addonInstallClass
                string StyleSheetAdd = String.Empty;
                string NewStyleValue;
                string SiteStyles;
                int PosNameLineEnd;
                int PosNameLineStart;
                int SiteStylePtr;
                string StyleLine;
                string[] SiteStyleSplit;
                int SiteStyleCnt;
                string NewStyleName;
                string TestStyleName;
                string SQL;
                DataTable rs;
                string Copy;
                string ContentName;
                int NodeCount;
                string TableName;
                bool RequireReload;
                bool Found;
                //  Dim builder As New coreBuilderClass(cpCore)
                string InstallCollectionList = "";
                logController.appendInstallLog(cpCore, ("Application: " 
                                + (cpCore.serverConfig.appConfig.name + ", UpgradeCDef_BuildDbFromCollection")));
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 0.5: verify core sql tables");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                appBuilderController.VerifyBasicTables(cpCore);
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 1: create SQL tables in default datasource");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                // With...
                if (true) {
                    string UsedTables = "";
                    foreach (keypairvalue in Collection.CDef) {
                        Models.Complex.cdefModel workingCdef = keypairvalue.Value;
                        ContentName = workingCdef.Name;
                        // With...
                        if (workingCdef.dataChanged) {
                            logController.appendInstallLog(cpCore, ("creating sql table [" 
                                            + (workingCdef.ContentTableName + ("], datasource [" 
                                            + (workingCdef.ContentDataSourceName + "]")))));
                            if (((genericController.vbLCase(workingCdef.ContentDataSourceName) == "default") 
                                        || (workingCdef.ContentDataSourceName == ""))) {
                                TableName = workingCdef.ContentTableName;
                                if ((genericController.vbInstr(1, ("," 
                                                + (UsedTables + ",")), ("," 
                                                + (TableName + ",")), vbTextCompare) != 0)) {
                                    TableName = TableName;
                                }
                                else {
                                    UsedTables = (UsedTables + ("," + TableName));
                                    cpCore.db.createSQLTable(workingCdef.ContentDataSourceName, TableName);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    cpCore.doc.clearMetaData();
                    cpCore.cache.invalidateAll();
                }
                
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 2: Verify all CDef names in ccContent so GetContentID calls will succeed");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                NodeCount = 0;
                List<string> installedContentList = new List<string>();
                rs = cpCore.db.executeQuery("SELECT Name from ccContent where (active<>0)");
                if (isDataTableOk(rs)) {
                    installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
                }
                
                rs.Dispose();
                // 
                foreach (keypairvalue in Collection.CDef) {
                    // With...
                    if (keypairvalue.Value.dataChanged) {
                        logController.appendInstallLog(cpCore, ("adding cdef name [" 
                                        + (keypairvalue.Value.Name + "]")));
                        if (!installedContentList.Contains(keypairvalue.Value.Name.ToLower, (Unknown)) {
                            SQL = ("Insert into ccContent (name,ccguid,active,createkey)values(" 
                                        + (cpCore.db.encodeSQLText(keypairvalue.Value.Name) + ("," 
                                        + (cpCore.db.encodeSQLText(keypairvalue.Value.guid) + ",1,0);"))));
                            cpCore.db.executeQuery(SQL);
                            installedContentList.Add(keypairvalue.Value.Name.ToLower, (Unknown);
                            RequireReload = true;
                        }
                        
                    }
                    
                }
                
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 4: Verify content records required for Content Server");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                addonInstallClass.VerifySortMethods(cpCore);
                addonInstallClass.VerifyContentFieldTypes(cpCore);
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 5: verify \'Content\' content definition");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                foreach (keypairvalue in Collection.CDef) {
                    // With...
                    if (keypairvalue.Value.Name.ToLower) {
                        "content";
                        logController.appendInstallLog(cpCore, ("adding cdef [" 
                                        + (keypairvalue.Value.Name + "]")));
                        addonInstallClass.installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion);
                        RequireReload = true;
                        break;
                    }
                    
                }
                
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 6.1: Verify all definitions and fields");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                RequireReload = false;
                foreach (keypairvalue in Collection.CDef) {
                    // With...
                    // 
                    //  todo tmp fix, changes to field caption in base.xml do not set fieldChange
                    if (true) {
                        //  If .dataChanged Or .includesAFieldChange Then
                        if ((keypairvalue.Value.Name.ToLower[] != "content")) {
                            logController.appendInstallLog(cpCore, ("adding cdef [" 
                                            + (keypairvalue.Value.Name + "]")));
                            addonInstallClass.installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion);
                            RequireReload = true;
                        }
                        
                    }
                    
                }
                
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 6.2: Verify all field help");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                FieldHelpCID = cpCore.db.getRecordID("content", "Content Field Help");
                foreach (keypairvalue in Collection.CDef) {
                    Models.Complex.cdefModel workingCdef = keypairvalue.Value;
                    ContentName = workingCdef.Name;
                    foreach (fieldKeyValuePair in workingCdef.fields) {
                        Models.Complex.CDefFieldModel field = fieldKeyValuePair.Value;
                        FieldName = field.nameLc;
                        // With...
                        ContentName.ToLower.fields(FieldName.ToLower);
                        if (Collection.CDef.HelpChanged) {
                            fieldId = 0;
                            SQL = ("select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" 
                                        + (cpCore.db.encodeSQLText(FieldName) + (")and(c.name=" 
                                        + (cpCore.db.encodeSQLText(ContentName) + ") order by f.id"))));
                            rs = cpCore.db.executeQuery(SQL);
                            if (isDataTableOk(rs)) {
                                fieldId = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "id"));
                            }
                            
                            rs.Dispose();
                            if ((fieldId == 0)) {
                                throw new ApplicationException("Unexpected exception");
                            }
                            else {
                                SQL = ("select id from ccfieldhelp where fieldid=" 
                                            + (fieldId + " order by id"));
                                rs = cpCore.db.executeQuery(SQL);
                                if (isDataTableOk(rs)) {
                                    FieldHelpID = genericController.EncodeInteger(rs.Rows[0].Item["id"]);
                                }
                                else {
                                    FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccfieldhelp", 0);
                                }
                                
                                rs.Dispose();
                                if ((FieldHelpID != 0)) {
                                    Copy = Collection.CDef.HelpCustom;
                                    if ((Copy == "")) {
                                        Copy = Collection.CDef.HelpDefault;
                                        if ((Copy != "")) {
                                            Copy = Copy;
                                        }
                                        
                                    }
                                    
                                    SQL = ("update ccfieldhelp set active=1,contentcontrolid=" 
                                                + (FieldHelpCID + (",fieldid=" 
                                                + (fieldId + (",helpdefault=" 
                                                + (cpCore.db.encodeSQLText(Copy) + (" where id=" + FieldHelpID)))))));
                                    cpCore.db.executeQuery(SQL);
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 7: create SQL indexes");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                for (Ptr = 0; (Ptr 
                            <= (Collection.SQLIndexCnt - 1)); Ptr++) {
                    // With...
                    Ptr;
                    if (Collection.SQLIndexes.dataChanged) {
                        logController.appendInstallLog(cpCore, ("creating index [" 
                                        + (Collection.SQLIndexes.IndexName + ("], fields [" 
                                        + (Collection.SQLIndexes.FieldNameList + ("], on table [" 
                                        + (Collection.SQLIndexes.TableName + "]")))))));
                        // 
                        //  stop the errors here, so a bad field does not block the upgrade
                        // 
                        // On Error Resume Next
                        cpCore.db.createSQLIndex(Collection.SQLIndexes.DataSourceName, Collection.SQLIndexes.TableName, Collection.SQLIndexes.IndexName, Collection.SQLIndexes.FieldNameList);
                    }
                    
                }
                
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 8a: Verify All Menu Names, then all Menus");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                for (Ptr = 0; (Ptr 
                            <= (Collection.MenuCnt - 1)); Ptr++) {
                    // With...
                    Ptr;
                    if ((Ptr == 140)) {
                        Ptr = Ptr;
                    }
                    
                    if (((genericController.vbLCase(Collection.Menus.Name) == "manage add-ons") 
                                && Collection.Menus.IsNavigator)) {
                        Collection.Menus.Name = Collection.Menus.Name;
                    }
                    
                    if (Collection.Menus.dataChanged) {
                        logController.appendInstallLog(cpCore, ("creating navigator entry [" 
                                        + (Collection.Menus.Name + ("], namespace [" 
                                        + (Collection.Menus.menuNameSpace + ("], guid [" 
                                        + (Collection.Menus.Guid + "]")))))));
                        appBuilderController.verifyNavigatorEntry(cpCore, Collection.Menus.Guid, Collection.Menus.menuNameSpace, Collection.Menus.Name, Collection.Menus.ContentName, Collection.Menus.LinkPage, Collection.Menus.SortOrder, Collection.Menus.AdminOnly, Collection.Menus.DeveloperOnly, Collection.Menus.NewWindow, Collection.Menus.Active, Collection.Menus.AddonName, Collection.Menus.NavIconType, Collection.Menus.NavIconTitle, 0);
                        // If .IsNavigator Then
                        // Else
                        //     ContentName = cnNavigatorEntries
                        //     Call logcontroller.appendInstallLog(cpCore,  "creating menu entry [" & .Name & "], parentname [" & .ParentName & "]")
                        //     Call Controllers.appBuilderController.admin_VerifyMenuEntry(cpCore, .ParentName, .Name, .ContentName, .LinkPage, .SortOrder, .AdminOnly, .DeveloperOnly, .NewWindow, .Active, ContentName, .AddonName)
                        // End If
                    }
                    
                }
                
                //  20160710 - this is old code (aggregatefunctions, etc are not in cdef anymore. Use the CollectionX methods to install addons
                // '
                // '----------------------------------------------------------------------------------------------------------------------
                // Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "CDef Load, stage 8c: Verify Add-ons")
                // '----------------------------------------------------------------------------------------------------------------------
                // '
                // NodeCount = 0
                // If .AddOnCnt > 0 Then
                //     For Ptr = 0 To .AddOnCnt - 1
                //         With .AddOns(Ptr)
                //             If .dataChanged Then
                //                 Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "creating add-on [" & .Name & "]")
                //                 'Dim 
                //                 'InstallCollectionFromLocalRepo_addonNode_Phase1(cpcore,"crap - this takes an xml node and I have a collection object...")
                //                 If .Link <> "" Then
                //                     Call csv_VerifyAggregateScript(.Name, .Link, .ArgumentList, .SortOrder)
                //                 ElseIf .ObjectProgramID <> "" Then
                //                     Call csv_VerifyAggregateObject(.Name, .ObjectProgramID, .ArgumentList, .SortOrder)
                //                 Else
                //                     Call csv_VerifyAggregateReplacement2(.Name, .Copy, .ArgumentList, .SortOrder)
                //                 End If
                //             End If
                //         End With
                //     Next
                // End If
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 8d: Verify Import Collections");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                if ((Collection.ImportCnt > 0)) {
                    // AddonClass = New addonInstallClass(cpCore)
                    for (Ptr = 0; (Ptr 
                                <= (Collection.ImportCnt - 1)); Ptr++) {
                        InstallCollectionList = (InstallCollectionList + ("," + Collection.collectionImports(Ptr).Guid));
                    }
                    
                }
                
                // 
                // ---------------------------------------------------------------------
                //  ----- Upgrade collections added during upgrade process
                // ---------------------------------------------------------------------
                // 
                string errorMessage = "";
                string[] Guids;
                string Guid;
                string CollectionPath = "";
                DateTime lastChangeDate = new DateTime();
                bool ignoreRefactor = false;
                logController.appendInstallLog(cpCore, "Installing Add-on Collections gathered during upgrade");
                if ((InstallCollectionList == "")) {
                    logController.appendInstallLog(cpCore, "No Add-on collections added during upgrade");
                }
                else {
                    errorMessage = "";
                    Guids = InstallCollectionList.Split(",");
                    for (Ptr = 0; (Ptr <= UBound(Guids)); Ptr++) {
                        errorMessage = "";
                        Guid = Guids[Ptr];
                        if ((Guid != "")) {
                            GetCollectionConfig(cpCore, Guid, CollectionPath, lastChangeDate, "");
                            if ((CollectionPath != "")) {
                                // 
                                //  This collection is installed locally, install from local collections
                                // 
                                installCollectionFromLocalRepo(cpCore, Guid, cpCore.codeVersion, errorMessage, "", isNewBuild, nonCriticalErrorList);
                            }
                            else {
                                // 
                                //  This is a new collection, install to the server and force it on this site
                                // 
                                bool addonInstallOk;
                                addonInstallOk = installCollectionFromRemoteRepo(cpCore, Guid, errorMessage, "", isNewBuild, nonCriticalErrorList);
                                if (!addonInstallOk) {
                                    throw new ApplicationException(("Failure to install addon collection from remote repository. Collection [" 
                                                    + (Guid + ("] was referenced in collection [" 
                                                    + (Collection.name + "]")))));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                }
                
                // 
                // ----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 9: Verify Styles");
                // ----------------------------------------------------------------------------------------------------------------------
                // 
                NodeCount = 0;
                if ((Collection.StyleCnt > 0)) {
                    SiteStyles = cpCore.cdnFiles.readFile("templates/styles.css");
                    if ((SiteStyles.Trim() != "")) {
                        // 
                        //  Split with an extra character at the end to guarantee there is an extra split at the end
                        // 
                        SiteStyleSplit = (SiteStyles + " ").Split("}");
                        SiteStyleCnt = (UBound(SiteStyleSplit) + 1);
                    }
                    
                    for (Ptr = 0; (Ptr 
                                <= (Collection.StyleCnt - 1)); Ptr++) {
                        Found = false;
                        // With...
                        Ptr;
                        if (Collection.Styles.dataChanged) {
                            NewStyleName = Collection.Styles.Name;
                            NewStyleValue = Collection.Styles.Copy;
                            NewStyleValue = genericController.vbReplace(NewStyleValue, "}", "");
                            NewStyleValue = genericController.vbReplace(NewStyleValue, "{", "");
                            if ((SiteStyleCnt > 0)) {
                                for (SiteStylePtr = 0; (SiteStylePtr 
                                            <= (SiteStyleCnt - 1)); SiteStylePtr++) {
                                    StyleLine = SiteStyleSplit[SiteStylePtr];
                                    PosNameLineEnd = InStrRev(StyleLine, "{");
                                    if ((PosNameLineEnd > 0)) {
                                        PosNameLineStart = InStrRev(StyleLine, "\r\n", PosNameLineEnd);
                                        if ((PosNameLineStart > 0)) {
                                            // 
                                            //  Check this site style for a match with the NewStyleName
                                            // 
                                            PosNameLineStart = (PosNameLineStart + 2);
                                            TestStyleName = StyleLine.Substring((PosNameLineStart - 1), (PosNameLineEnd - PosNameLineStart)).Trim();
                                            if ((genericController.vbLCase(TestStyleName) == genericController.vbLCase(NewStyleName))) {
                                                Found = true;
                                                if (Collection.Styles.Overwrite) {
                                                    // 
                                                    //  Found - Update style
                                                    // 
                                                    SiteStyleSplit[SiteStylePtr] = ("\r\n" 
                                                                + (Collection.Styles.Name + (" {" + NewStyleValue)));
                                                }
                                                
                                                break;
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                }
                                
                            }
                            
                            // 
                            //  Add or update the stylesheet
                            // 
                            if (!Found) {
                                StyleSheetAdd = (StyleSheetAdd + ("\r\n" 
                                            + (NewStyleName + (" {" 
                                            + (NewStyleValue + "}")))));
                            }
                            
                        }
                        
                    }
                    
                    SiteStyles = Join(SiteStyleSplit, "}");
                    if ((StyleSheetAdd != "")) {
                        SiteStyles = (SiteStyles + ("\r\n" + ("\r\n" + ("/*" + ("\r\n" + ("Styles added " 
                                    + (Now() + ("\r\n" + ("*/" + ("\r\n" + StyleSheetAdd))))))))));
                    }
                    
                    cpCore.appRootFiles.saveFile("templates/styles.css", SiteStyles);
                    // 
                    //  Update stylesheet cache
                    // 
                    cpCore.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                }
                
                // 
                // -------------------------------------------------------------------------------------------------
                //  Page Templates
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Site Sections
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Dynamic Menus
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Page Content
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Copy Content
                // -------------------------------------------------------------------------------------------------
                // 
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ========================================================================
        //  ----- Load the archive file application
        // ========================================================================
        // 
        private static void installCollection_BuildDbFromCollection_AddCDefToDb(coreClass cpCore, Models.Complex.cdefModel cdef, string BuildVersion) {
            try {
                // 
                int FieldHelpCID;
                int FieldHelpID;
                int fieldId;
                int ContentID;
                DataTable rs;
                int EditorGroupID;
                int FieldCount;
                int FieldSize;
                string ContentName;
                // Dim DataSourceName As String
                string SQL;
                bool ContentIsBaseContent;
                // 
                logController.appendInstallLog(cpCore, ("Application: " 
                                + (cpCore.serverConfig.appConfig.name + ", UpgradeCDef_BuildDbFromCollection_AddCDefToDb")));
                // 
                if (!false) {
                    // With...
                    // 
                    logController.appendInstallLog(cpCore, ("Upgrading CDef [" 
                                    + (cdef.Name + "]")));
                    // 
                    ContentID = 0;
                    ContentName = cdef.Name;
                    ContentIsBaseContent = false;
                    FieldHelpCID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Help");
                    Contensive.Core.Models.Entity.dataSourceModel datasource = Models.Entity.dataSourceModel.createByName(cpCore, cdef.ContentDataSourceName, new List<string>());
                    // 
                    //  get contentid and protect content with IsBaseContent true
                    // 
                    SQL = cpCore.db.GetSQLSelect("default", "ccContent", "ID,IsBaseContent", ("name=" + cpCore.db.encodeSQLText(ContentName)), "ID", ,, 1);
                    rs = cpCore.db.executeQuery(SQL);
                    if (isDataTableOk(rs)) {
                        if ((rs.Rows.Count > 0)) {
                            // EditorGroupID = cpcore.app.getDataRowColumnName(RS.rows(0), "ID")
                            ContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                            ContentIsBaseContent = genericController.EncodeBoolean(cpCore.db.getDataRowColumnName(rs.Rows[0], "IsBaseContent"));
                        }
                        
                    }
                    
                    rs.Dispose();
                    // 
                    //  ----- Update Content Record
                    // 
                    if (cdef.dataChanged) {
                        // 
                        //  Content needs to be updated
                        // 
                        if ((ContentIsBaseContent 
                                    && !cdef.IsBaseContent)) {
                            // 
                            //  Can not update a base content with a non-base content
                            // 
                            cpCore.handleException(new ApplicationException(("Warning: An attempt was made to update Content Definition [" 
                                                + (cdef.Name + "] from base to non-base. This should only happen when a base cdef is removed from the base collection" +
                                                ". The update was ignored.")))).IsBaseContent = ContentIsBaseContent;
                            // cpCore.handleLegacyError3( "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
                        }
                        
                        // 
                        //  ----- update definition (use SingleRecord as an update flag)
                        // 
                        Models.Complex.cdefModel.addContent(cpCore, true, datasource, cdef.ContentTableName, ContentName, cdef.AdminOnly, cdef.DeveloperOnly, cdef.AllowAdd, cdef.AllowDelete, cdef.parentName, cdef.DefaultSortMethod, cdef.DropDownFieldList, false, cdef.AllowCalendarEvents, cdef.AllowContentTracking, cdef.AllowTopicRules, cdef.AllowContentChildTool, false, cdef.IconLink, cdef.IconWidth, cdef.IconHeight, cdef.IconSprites, cdef.guid, cdef.IsBaseContent, cdef.installedByCollectionGuid);
                        if ((ContentID == 0)) {
                            logController.appendInstallLog(cpCore, ("Could not determine contentid after createcontent3 for [" 
                                            + (ContentName + "], upgrade for this cdef aborted.")));
                        }
                        else {
                            // 
                            //  ----- Other fields not in the csv call
                            // 
                            EditorGroupID = 0;
                            if ((cdef.EditorGroupName != "")) {
                                rs = cpCore.db.executeQuery(("select ID from ccGroups where name=" + cpCore.db.encodeSQLText(cdef.EditorGroupName)));
                                if (isDataTableOk(rs)) {
                                    if ((rs.Rows.Count > 0)) {
                                        EditorGroupID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                                    }
                                    
                                }
                                
                                rs.Dispose();
                            }
                            
                            SQL = ("update ccContent" + (" set EditorGroupID=" 
                                        + (EditorGroupID + (",isbasecontent=" 
                                        + (cpCore.db.encodeSQLBoolean(cdef.IsBaseContent) + (" where id=" 
                                        + (ContentID + "")))))));
                            cpCore.db.executeQuery(SQL);
                        }
                        
                    }
                    
                    // 
                    //  ----- update Content Field Records and Content Field Help records
                    // 
                    if (((ContentID == 0) 
                                && (cdef.fields.Count > 0))) {
                        // 
                        //  CAn not add fields if there is no content record
                        // 
                        throw new ApplicationException("Unexpected exception");
                    }
                    else {
                        // 
                        // 
                        // 
                        FieldSize = 0;
                        FieldCount = 0;
                        foreach (nameValuePair in cdef.fields) {
                            Models.Complex.CDefFieldModel field = nameValuePair.Value;
                            // With...
                            if (field.dataChanged) {
                                fieldId = Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field);
                            }
                            
                            // 
                            //  ----- update content field help records
                            // 
                            if (field.HelpChanged) {
                                rs = cpCore.db.executeQuery(("select ID from ccFieldHelp where fieldid=" + fieldId));
                                if (isDataTableOk(rs)) {
                                    if ((rs.Rows.Count > 0)) {
                                        FieldHelpID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                                    }
                                    
                                }
                                
                                rs.Dispose();
                                // 
                                if ((FieldHelpID == 0)) {
                                    FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccFieldHelp", 0);
                                }
                                
                                if ((FieldHelpID != 0)) {
                                    SQL = ("update ccfieldhelp" + (" set fieldid=" 
                                                + (fieldId + (",active=1" + (",contentcontrolid=" 
                                                + (FieldHelpCID + (",helpdefault=" 
                                                + (cpCore.db.encodeSQLText(field.HelpDefault) + (",helpcustom=" 
                                                + (cpCore.db.encodeSQLText(field.HelpCustom) + (" where id=" + FieldHelpID)))))))))));
                                    cpCore.db.executeQuery(SQL);
                                }
                                
                            }
                            
                        }
                        
                        // '
                        // ' started doing something here -- research it.!!!!!
                        // '
                        // For FieldPtr = 0 To .fields.Count - 1
                        //     fieldId = 0
                        //     With .fields(FieldPtr)
                        //     End With
                        // Next
                        // 
                        //  clear the cdef cache and list
                        // 
                        cpCore.doc.clearMetaData();
                        cpCore.cache.invalidateAll();
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ==========================================================================================================================
        //    Overlay a Src CDef on to the current one (Dst)
        //        Any Src CDEf entries found in Src are added to Dst.
        //        if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
        // 
        //        isBaseContent
        //            if dst not found, it is created to match src
        //            if dst found, it is updated only if isBase matches
        //                content attributes updated if .isBaseContent matches
        //                field attributes updated if .isBaseField matches
        // ==========================================================================================================================
        // 
        private static bool installCollection_AddMiniCollectionSrcToDst(coreClass cpCore, ref miniCollectionModel dstCollection, miniCollectionModel srcCollection, bool SrcIsUserCDef) {
            bool returnOk = true;
            try {
                string HelpSrc;
                bool HelpCustomChanged;
                bool HelpDefaultChanged;
                bool HelpChanged;
                string Copy;
                string n;
                Models.Complex.CDefFieldModel srcCollectionCdefField;
                Models.Complex.cdefModel dstCollectionCdef;
                Models.Complex.CDefFieldModel dstCollectionCdefField;
                bool IsMatch;
                string DstKey;
                string SrcKey;
                string DataBuildVersion;
                bool SrcIsNavigator;
                bool DstIsNavigator;
                string SrcContentName;
                string DstName;
                string SrcFieldName;
                bool okToUpdateDstFromSrc;
                Models.Complex.cdefModel srcCollectionCdef;
                bool DebugSrcFound;
                bool DebugDstFound;
                // 
                //  If the Src is the BaseCollection, the Dst must be the Application Collectio
                //    in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                // 
                if (srcCollection.isBaseCollection) {
                    foreach (dstKeyValuePair in dstCollection.CDef) {
                        Models.Complex.cdefModel dstWorkingCdef = dstKeyValuePair.Value;
                        string contentName;
                        contentName = dstWorkingCdef.Name;
                        if (dstCollection.CDef(contentName.ToLower).IsBaseContent) {
                            // 
                            //  this application collection Cdef is marked base, verify it is in the base collection
                            // 
                            if (!srcCollection.CDef.ContainsKey(contentName.ToLower)) {
                                // 
                                //  cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                // 
                                // With...
                                foreach (dstFieldKeyValuePair in // TODO: Warning!!!! NULL EXPRESSION DETECTED...
                                ) {
                                    fields;
                                    dstCollection.CDef(contentName.ToLower).IsBaseContent = true;
                                    Models.Complex.CDefFieldModel field = dstFieldKeyValuePair.Value;
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
                //  -------------------------------------------------------------------------------------------------
                //  Go through all CollectionSrc and find the CollectionDst match
                //    if it is an exact match, do nothing
                //    if the cdef does not match, set cdefext(ptr).CDefChanged true
                //    if any field does not match, set cdefext...field...CDefChanged
                //    if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                //  -------------------------------------------------------------------------------------------------
                // 
                logController.appendInstallLog(cpCore, ("Application: " 
                                + (cpCore.serverConfig.appConfig.name + ", UpgradeCDef_AddSrcToDst")));
                // 
                foreach (srcKeyValuePair in srcCollection.CDef) {
                    srcCollectionCdef = srcKeyValuePair.Value;
                    SrcContentName = srcCollectionCdef.Name;
                    // If genericController.vbLCase(SrcContentName) = "site sections" Then
                    //     SrcContentName = SrcContentName
                    // End If
                    DebugSrcFound = false;
                    if ((genericController.vbInstr(1, SrcContentName, cnNavigatorEntries, vbTextCompare) != 0)) {
                        DebugSrcFound = true;
                    }
                    
                    // 
                    //  Search for this cdef in the Dst
                    // 
                    okToUpdateDstFromSrc = false;
                    if (!dstCollection.CDef.ContainsKey(SrcContentName.ToLower)) {
                        // 
                        //  add src to dst
                        // 
                        dstCollection.CDef.Add(SrcContentName.ToLower, new Models.Complex.cdefModel());
                        okToUpdateDstFromSrc = true;
                    }
                    else {
                        dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower);
                        DstName = SrcContentName;
                        // 
                        //  found a match between Src and Dst
                        // 
                        if ((dstCollectionCdef.IsBaseContent == srcCollectionCdef.IsBaseContent)) {
                            // 
                            //  Allow changes to user cdef only from user cdef, changes to base only from base
                            // 
                            // With...
                            n = "ActiveOnly";
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.ActiveOnly != srcCollectionCdef.ActiveOnly));
                            if (!okToUpdateDstFromSrc) {
                                n = "AdminOnly";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AdminOnly != srcCollectionCdef.AdminOnly));
                            if (!okToUpdateDstFromSrc) {
                                n = "DeveloperOnly";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.DeveloperOnly != srcCollectionCdef.DeveloperOnly));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowAdd";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowAdd != srcCollectionCdef.AllowAdd));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowCalendarEvents";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowCalendarEvents != srcCollectionCdef.AllowCalendarEvents));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowContentTracking";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowContentTracking != srcCollectionCdef.AllowContentTracking));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowDelete";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowDelete != srcCollectionCdef.AllowDelete));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowTopicRules";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowTopicRules != srcCollectionCdef.AllowTopicRules));
                            if (!okToUpdateDstFromSrc) {
                                n = "ContentDataSourceName";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.ContentDataSourceName, srcCollectionCdef.ContentDataSourceName));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "ContentTableName";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.ContentTableName, srcCollectionCdef.ContentTableName));
                            // 
                            if (DebugDstFound) {
                                DebugDstFound = DebugDstFound;
                            }
                            
                            if (!okToUpdateDstFromSrc) {
                                n = "DefaultSortMethod";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.DefaultSortMethod, srcCollectionCdef.DefaultSortMethod));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "DropDownFieldList";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.DropDownFieldList, srcCollectionCdef.DropDownFieldList));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "EditorGroupName";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.EditorGroupName, srcCollectionCdef.EditorGroupName));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "IgnoreContentControl";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.IgnoreContentControl != srcCollectionCdef.IgnoreContentControl));
                            if (okToUpdateDstFromSrc) {
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                            }
                            
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "Active";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.Active != srcCollectionCdef.Active));
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowContentChildTool";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.AllowContentChildTool != srcCollectionCdef.AllowContentChildTool));
                            if (!okToUpdateDstFromSrc) {
                                n = "ParentId";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.parentID != srcCollectionCdef.parentID));
                            if (!okToUpdateDstFromSrc) {
                                n = "IconLink";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.IconLink, srcCollectionCdef.IconLink));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "IconHeight";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.IconHeight != srcCollectionCdef.IconHeight));
                            if (!okToUpdateDstFromSrc) {
                                n = "IconWidth";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.IconWidth != srcCollectionCdef.IconWidth));
                            if (!okToUpdateDstFromSrc) {
                                n = "IconSprites";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.IconSprites != srcCollectionCdef.IconSprites));
                            if (!okToUpdateDstFromSrc) {
                                n = "installedByCollectionGuid";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.installedByCollectionGuid, srcCollectionCdef.installedByCollectionGuid));
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "ccGuid";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollectionCdef.guid, srcCollectionCdef.guid));
                            // 
                            //  IsBaseContent
                            //    if Dst IsBase, and Src is not, this change will be blocked following the changes anyway
                            //    if Src IsBase, and Dst is not, Dst should be changed, and IsBaseContent can be treated like any other field
                            // 
                            if (!okToUpdateDstFromSrc) {
                                n = "IsBaseContent";
                            }
                            
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollectionCdef.IsBaseContent != srcCollectionCdef.IsBaseContent));
                            if (okToUpdateDstFromSrc) {
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                            }
                            
                            if (okToUpdateDstFromSrc) {
                                if ((dstCollectionCdef.IsBaseContent 
                                            && !srcCollectionCdef.IsBaseContent)) {
                                    // 
                                    //  Dst is a base CDef, Src is not. This update is not allowed. Log it and skip the Add
                                    // 
                                    Copy = ("An attempt was made to update a Base Content Definition [" 
                                                + (DstName + "] from a collection that is not the Base Collection. This is not allowed."));
                                    logController.appendInstallLog(cpCore, ("UpgradeCDef_AddSrcToDst, " + Copy));
                                    throw new ApplicationException("Unexpected exception");
                                    okToUpdateDstFromSrc = false;
                                }
                                else {
                                    // 
                                    //  Just log the change for tracking
                                    // 
                                    logController.appendInstallLog(cpCore, ("UpgradeCDef_AddSrcToDst, (Logging only) While merging two collections (probably application and an up" +
                                        "grade), one or more attributes for a content definition or field were different, first change was CD" +
                                        "ef=" 
                                                    + (SrcContentName + (", field=" + n))));
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    if (okToUpdateDstFromSrc) {
                        // With...
                        // 
                        //  It okToUpdateDstFromSrc, update the Dst with the Src
                        // 
                        srcCollectionCdef.ActiveOnly.AdminOnly = srcCollectionCdef.AdminOnly;
                        srcCollectionCdef.Active.ActiveOnly = srcCollectionCdef.AdminOnly;
                        dstCollection.CDef(SrcContentName.ToLower).Active = srcCollectionCdef.AdminOnly;
                        // .adminColumns = srcCollectionCdef.adminColumns
                        srcCollectionCdef.DropDownFieldList.EditorGroupName = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.DeveloperOnly.DropDownFieldList = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.DefaultSortMethod.DeveloperOnly = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.dataSourceId.DefaultSortMethod = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.ContentTableName.dataSourceId = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.ContentDataSourceName.ContentTableName = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.ContentControlCriteria.ContentDataSourceName = srcCollectionCdef.EditorGroupName;
                        true.ContentControlCriteria = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.guid.dataChanged = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowTopicRules.guid = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowDelete.AllowTopicRules = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowContentTracking.AllowDelete = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowContentChildTool.AllowContentTracking = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowCalendarEvents.AllowContentChildTool = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AllowAdd.AllowCalendarEvents = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AliasName.AllowAdd = srcCollectionCdef.EditorGroupName;
                        srcCollectionCdef.AliasID.AliasName = srcCollectionCdef.EditorGroupName;
                        dstCollection.CDef(SrcContentName.ToLower).AliasID = srcCollectionCdef.EditorGroupName;
                        // .fields
                        srcCollectionCdef.IconSprites.IconWidth = srcCollectionCdef.IconWidth;
                        srcCollectionCdef.IconLink.IconSprites = srcCollectionCdef.IconWidth;
                        srcCollectionCdef.IconHeight.IconLink = srcCollectionCdef.IconWidth;
                        dstCollection.CDef(SrcContentName.ToLower).IconHeight = srcCollectionCdef.IconWidth;
                        // .Id
                        srcCollectionCdef.parentName.SelectCommaList = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.parentID.parentName = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.Name.parentID = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.IsModifiedSinceInstalled.Name = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.IsBaseContent.IsModifiedSinceInstalled = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.installedByCollectionGuid.IsBaseContent = srcCollectionCdef.SelectCommaList;
                        true.installedByCollectionGuid = srcCollectionCdef.SelectCommaList;
                        srcCollectionCdef.IgnoreContentControl.includesAFieldChange = srcCollectionCdef.SelectCommaList;
                        dstCollection.CDef(SrcContentName.ToLower).IgnoreContentControl = srcCollectionCdef.SelectCommaList;
                        // .selectList
                        // .TimeStamp
                        dstCollection.CDef(SrcContentName.ToLower).WhereClause = srcCollectionCdef.WhereClause;
                    }
                    
                    // 
                    //  Now check each of the field records for an addition, or a change
                    //  DstPtr is still set to the Dst CDef
                    // 
                    // Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef(SrcPtr).fields.count=" & CollectionSrc.CDef(SrcPtr).fields.count)
                    // With...
                    foreach (srcFieldKeyValuePair in srcCollectionCdef.fields) {
                        srcCollectionCdefField = srcFieldKeyValuePair.Value;
                        SrcFieldName = srcCollectionCdefField.nameLc;
                        okToUpdateDstFromSrc = false;
                        if (!dstCollection.CDef.ContainsKey(SrcContentName.ToLower)) {
                            // 
                            //  should have been the collection
                            // 
                            throw new ApplicationException("ERROR - cannot update destination content because it was not found after being added.");
                        }
                        else {
                            dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower);
                            if (dstCollectionCdef.fields.ContainsKey(SrcFieldName.ToLower)) {
                                // 
                                //  Src field was found in Dst fields
                                // 
                                dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower);
                                okToUpdateDstFromSrc = false;
                                if ((dstCollectionCdefField.isBaseField == srcCollectionCdefField.isBaseField)) {
                                    // With...
                                    n = "Active";
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.active != dstCollectionCdefField.active));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "AdminOnly";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.adminOnly != dstCollectionCdefField.adminOnly));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Authorable";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.authorable != dstCollectionCdefField.authorable));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Caption";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.caption, dstCollectionCdefField.caption));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ContentID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.contentId != dstCollectionCdefField.contentId));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "DeveloperOnly";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.developerOnly != dstCollectionCdefField.developerOnly));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "EditSortPriority";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.editSortPriority != dstCollectionCdefField.editSortPriority));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "EditTab";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.editTabName, dstCollectionCdefField.editTabName));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "FieldType";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.fieldTypeId != dstCollectionCdefField.fieldTypeId));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "HTMLContent";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.htmlContent != dstCollectionCdefField.htmlContent));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexColumn";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.indexColumn != dstCollectionCdefField.indexColumn));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexSortDirection";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.indexSortDirection != dstCollectionCdefField.indexSortDirection));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexSortOrder";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (EncodeInteger(srcCollectionCdefField.indexSortOrder) != genericController.EncodeInteger(dstCollectionCdefField.indexSortOrder)));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexWidth";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.indexWidth, dstCollectionCdefField.indexWidth));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupContentID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.lookupContentID != dstCollectionCdefField.lookupContentID));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupList";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.lookupList, dstCollectionCdefField.lookupList));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyContentID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.manyToManyContentID != dstCollectionCdefField.manyToManyContentID));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleContentID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.manyToManyRuleContentID != dstCollectionCdefField.manyToManyRuleContentID));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRulePrimaryField";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.ManyToManyRulePrimaryField, dstCollectionCdefField.ManyToManyRulePrimaryField));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleSecondaryField";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.ManyToManyRuleSecondaryField, dstCollectionCdefField.ManyToManyRuleSecondaryField));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "MemberSelectGroupID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.MemberSelectGroupID != dstCollectionCdefField.MemberSelectGroupID));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "NotEditable";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.NotEditable != dstCollectionCdefField.NotEditable));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Password";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.Password != dstCollectionCdefField.Password));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ReadOnly";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.ReadOnly != dstCollectionCdefField.ReadOnly));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectContentID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.RedirectContentID != dstCollectionCdefField.RedirectContentID));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectID";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.RedirectID, dstCollectionCdefField.RedirectID));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectPath";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.RedirectPath, dstCollectionCdefField.RedirectPath));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Required";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.Required != dstCollectionCdefField.Required));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RSSDescriptionField";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.RSSDescriptionField != dstCollectionCdefField.RSSDescriptionField));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RSSTitleField";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.RSSTitleField != dstCollectionCdefField.RSSTitleField));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Scramble";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.Scramble != dstCollectionCdefField.Scramble));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "TextBuffered";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.TextBuffered != dstCollectionCdefField.TextBuffered));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "DefaultValue";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (genericController.encodeText(srcCollectionCdefField.defaultValue) != genericController.encodeText(dstCollectionCdefField.defaultValue)));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "UniqueName";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.UniqueName != dstCollectionCdefField.UniqueName));
                                    if (okToUpdateDstFromSrc) {
                                        okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                                    }
                                    
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IsBaseField";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | (srcCollectionCdefField.isBaseField != dstCollectionCdefField.isBaseField));
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupContentName";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.lookupContentName, cpCore, dstCollectionCdefField.lookupContentName(cpCore)));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyContentName";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.ManyToManyContentName, cpCore, dstCollectionCdefField.ManyToManyContentName(cpCore)));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleContentName";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.ManyToManyRuleContentName, cpCore, dstCollectionCdefField.ManyToManyRuleContentName(cpCore)));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectContentName";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.RedirectContentName, cpCore, dstCollectionCdefField.RedirectContentName(cpCore)));
                                    // 
                                    if (!okToUpdateDstFromSrc) {
                                        n = "installedByCollectionid";
                                    }
                                    
                                    okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                                | !addonInstallClass.TextMatch(cpCore, srcCollectionCdefField.installedByCollectionGuid, dstCollectionCdefField.installedByCollectionGuid));
                                    // 
                                    if (okToUpdateDstFromSrc) {
                                        okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                                    }
                                    
                                }
                                
                                // 
                                //  Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
                                // 
                                HelpSrc = srcCollectionCdefField.HelpCustom;
                                HelpCustomChanged = !addonInstallClass.TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpCustom);
                                // 
                                HelpSrc = srcCollectionCdefField.HelpDefault;
                                HelpDefaultChanged = !addonInstallClass.TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpDefault);
                                // 
                                HelpChanged = (HelpDefaultChanged | HelpCustomChanged);
                            }
                            else {
                                // 
                                //  field was not found in dst, add it and populate
                                // 
                                dstCollectionCdef.fields.Add(SrcFieldName.ToLower, new Models.Complex.CDefFieldModel());
                                dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower);
                                okToUpdateDstFromSrc = true;
                                HelpChanged = true;
                            }
                            
                            // 
                            //  If okToUpdateDstFromSrc, update the Dst record with the Src record
                            // 
                            if (okToUpdateDstFromSrc) {
                                // 
                                //  Update Fields
                                // 
                                // With...
                                if (HelpChanged) {
                                    srcCollectionCdefField.HelpDefault.HelpChanged = true;
                                    srcCollectionCdefField.RedirectContentName(cpCore).installedByCollectionGuid = true;
                                    srcCollectionCdefField.ManyToManyRuleContentName(cpCore).RedirectContentName(cpCore) = true;
                                    srcCollectionCdefField.ManyToManyContentName(cpCore).ManyToManyRuleContentName(cpCore) = true;
                                    srcCollectionCdefField.lookupContentName(cpCore).ManyToManyContentName(cpCore) = true;
                                    srcCollectionCdefField.isBaseField.lookupContentName(cpCore) = true;
                                    srcCollectionCdefField.UniqueName.isBaseField = true;
                                    srcCollectionCdefField.TextBuffered.UniqueName = true;
                                    srcCollectionCdefField.Scramble.TextBuffered = true;
                                    srcCollectionCdefField.RSSTitleField.Scramble = true;
                                    srcCollectionCdefField.RSSDescriptionField.RSSTitleField = true;
                                    srcCollectionCdefField.Required.RSSDescriptionField = true;
                                    srcCollectionCdefField.RedirectPath.Required = true;
                                    srcCollectionCdefField.RedirectID.RedirectPath = true;
                                    srcCollectionCdefField.RedirectContentID.RedirectID = true;
                                    srcCollectionCdefField.ReadOnly.RedirectContentID = true;
                                    srcCollectionCdefField.Password.ReadOnly = true;
                                    srcCollectionCdefField.NotEditable.Password = true;
                                    srcCollectionCdefField.nameLc.NotEditable = true;
                                    srcCollectionCdefField.MemberSelectGroupID.nameLc = true;
                                    srcCollectionCdefField.ManyToManyRuleSecondaryField.MemberSelectGroupID = true;
                                    srcCollectionCdefField.ManyToManyRulePrimaryField.ManyToManyRuleSecondaryField = true;
                                    srcCollectionCdefField.manyToManyRuleContentID.ManyToManyRulePrimaryField = true;
                                    srcCollectionCdefField.manyToManyContentID.manyToManyRuleContentID = true;
                                    srcCollectionCdefField.lookupList.manyToManyContentID = true;
                                    srcCollectionCdefField.lookupContentID.lookupList = true;
                                    srcCollectionCdefField.indexWidth.lookupContentID = true;
                                    srcCollectionCdefField.indexSortOrder.indexWidth = true;
                                    srcCollectionCdefField.indexSortDirection.indexSortOrder = true;
                                    srcCollectionCdefField.indexColumn.indexSortDirection = true;
                                    srcCollectionCdefField.htmlContent.indexColumn = true;
                                    srcCollectionCdefField.fieldTypeId.htmlContent = true;
                                    srcCollectionCdefField.editTabName.fieldTypeId = true;
                                    srcCollectionCdefField.editSortPriority.editTabName = true;
                                    srcCollectionCdefField.developerOnly.editSortPriority = true;
                                    srcCollectionCdefField.defaultValue.developerOnly = true;
                                    srcCollectionCdefField.contentId.defaultValue = true;
                                    srcCollectionCdefField.caption.contentId = true;
                                    srcCollectionCdefField.authorable.caption = true;
                                    srcCollectionCdefField.adminOnly.authorable = true;
                                    srcCollectionCdefField.active.adminOnly = true;
                                    dstCollectionCdefField.active = true;
                                    srcCollectionCdefField.HelpCustom.HelpDefault = true;
                                    HelpCustom = true;
                                }
                                
                                dstCollectionCdef.includesAFieldChange = true;
                            }
                            
                            // 
                        }
                        
                    }
                    
                }
                
                // 
                //  -------------------------------------------------------------------------------------------------
                //  Check SQL Indexes
                //  -------------------------------------------------------------------------------------------------
                // 
                int dstSqlIndexPtr;
                int SrcsSqlIndexPtr;
                for (SrcsSqlIndexPtr = 0; (SrcsSqlIndexPtr 
                            <= (srcCollection.SQLIndexCnt - 1)); SrcsSqlIndexPtr++) {
                    SrcContentName = (srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName + ("-" 
                                + (srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName + ("-" + srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName))));
                    okToUpdateDstFromSrc = false;
                    for (dstSqlIndexPtr = 0; (dstSqlIndexPtr 
                                <= (dstCollection.SQLIndexCnt - 1)); dstSqlIndexPtr++) {
                        DstName = (dstCollection.SQLIndexes(dstSqlIndexPtr).DataSourceName + ("-" 
                                    + (dstCollection.SQLIndexes(dstSqlIndexPtr).TableName + ("-" + dstCollection.SQLIndexes(dstSqlIndexPtr).IndexName))));
                        if (addonInstallClass.TextMatch(cpCore, DstName, SrcContentName)) {
                            // 
                            //  found a match between Src and Dst
                            // 
                            // With...
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.SQLIndexes(dstSqlIndexPtr).FieldNameList, srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList));
                            break;
                        }
                        
                    }
                    
                    if ((dstSqlIndexPtr == dstCollection.SQLIndexCnt)) {
                        // 
                        //  CDef was not found, add it
                        // 
                        object Preserve;
                        dstCollection.SQLIndexes(dstCollection.SQLIndexCnt);
                        dstCollection.SQLIndexCnt = (dstSqlIndexPtr + 1);
                        okToUpdateDstFromSrc = true;
                    }
                    
                    if (okToUpdateDstFromSrc) {
                        // With...
                        // 
                        //  It okToUpdateDstFromSrc, update the Dst with the Src
                        // 
                        srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName.TableName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
                        srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList.IndexName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
                        srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName.FieldNameList = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
                        true.DataSourceName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
                        dstCollection.SQLIndexes(dstSqlIndexPtr).dataChanged = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
                    }
                    
                }
                
                // 
                // -------------------------------------------------------------------------------------------------
                //  Check menus
                // -------------------------------------------------------------------------------------------------
                // 
                int DstMenuPtr;
                string SrcNameSpace;
                string SrcParentName;
                DataBuildVersion = cpCore.siteProperties.dataBuildVersion;
                for (SrcMenuPtr = 0; (SrcMenuPtr 
                            <= (srcCollection.MenuCnt - 1)); SrcMenuPtr++) {
                    DstMenuPtr = 0;
                    SrcContentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Name);
                    SrcParentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).ParentName);
                    SrcNameSpace = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).menuNameSpace);
                    SrcIsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator;
                    if (SrcIsNavigator) {
                        if ((SrcContentName == "manage add-ons")) {
                            SrcContentName = SrcContentName;
                        }
                        
                    }
                    
                    okToUpdateDstFromSrc = false;
                    SrcKey = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Key);
                    // 
                    //  Search for match using guid
                    // 
                    IsMatch = false;
                    for (DstMenuPtr = 0; (DstMenuPtr 
                                <= (dstCollection.MenuCnt - 1)); DstMenuPtr++) {
                        DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name);
                        if ((DstName == SrcContentName)) {
                            DstName = DstName;
                            DstIsNavigator = dstCollection.Menus(DstMenuPtr).IsNavigator;
                            DstKey = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Key);
                            if ((genericController.vbLCase(DstName) == "settings")) {
                                DstName = DstName;
                            }
                            
                            IsMatch = ((DstKey == SrcKey) 
                                        & (SrcIsNavigator == DstIsNavigator));
                            if (IsMatch) {
                                break;
                            }
                            
                        }
                        
                    }
                    
                    if (!IsMatch) {
                        // 
                        //  no match found on guid, try name and ( either namespace or parentname )
                        // 
                        for (DstMenuPtr = 0; (DstMenuPtr 
                                    <= (dstCollection.MenuCnt - 1)); DstMenuPtr++) {
                            DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name);
                            if ((genericController.vbLCase(DstName) == "settings")) {
                                DstName = DstName;
                            }
                            
                            if (((SrcContentName == DstName) 
                                        && (SrcIsNavigator == DstIsNavigator))) {
                                if (SrcIsNavigator) {
                                    // 
                                    //  Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    // 
                                    IsMatch = ((SrcNameSpace == genericController.vbLCase(dstCollection.Menus(DstMenuPtr).menuNameSpace)) 
                                                & (dstCollection.Menus(DstMenuPtr).Guid == ""));
                                }
                                else {
                                    // 
                                    //  AdminMenu - check parentname
                                    // 
                                    IsMatch = (SrcParentName == genericController.vbLCase(dstCollection.Menus(DstMenuPtr).ParentName));
                                }
                                
                                if (IsMatch) {
                                    break;
                                }
                                
                            }
                            
                        }
                        
                    }
                    
                    if (!IsMatch) {
                        // If DstPtr = CollectionDst.MenuCnt Then
                        // 
                        //  menu was not found, add it
                        // 
                        object Preserve;
                        dstCollection.Menus(dstCollection.MenuCnt);
                        dstCollection.MenuCnt = (dstCollection.MenuCnt + 1);
                        okToUpdateDstFromSrc = true;
                    }
                    else {
                        // If IsMatch Then
                        // 
                        //  found a match between Src and Dst
                        // 
                        if ((SrcIsUserCDef || SrcIsNavigator)) {
                            // 
                            //  Special case -- Navigators update from all upgrade sources so Base migrates changes
                            //  test for cdef attribute changes
                            // 
                            // With...
                            // With dstCollection.Menus(dstCollection.MenuCnt)
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollection.Menus(DstMenuPtr).Active != srcCollection.Menus(SrcMenuPtr).Active));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollection.Menus(DstMenuPtr).AdminOnly != srcCollection.Menus(SrcMenuPtr).AdminOnly));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).ContentName, srcCollection.Menus(SrcMenuPtr).ContentName));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollection.Menus(DstMenuPtr).DeveloperOnly != srcCollection.Menus(SrcMenuPtr).DeveloperOnly));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).LinkPage, srcCollection.Menus(SrcMenuPtr).LinkPage));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).Name, srcCollection.Menus(SrcMenuPtr).Name));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | (dstCollection.Menus(DstMenuPtr).NewWindow != srcCollection.Menus(SrcMenuPtr).NewWindow));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).SortOrder, srcCollection.Menus(SrcMenuPtr).SortOrder));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).AddonName, srcCollection.Menus(SrcMenuPtr).AddonName));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).NavIconType, srcCollection.Menus(SrcMenuPtr).NavIconType));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).NavIconTitle, srcCollection.Menus(SrcMenuPtr).NavIconTitle));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).menuNameSpace, srcCollection.Menus(SrcMenuPtr).menuNameSpace));
                            okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                        | !addonInstallClass.TextMatch(cpCore, dstCollection.Menus(DstMenuPtr).Guid, srcCollection.Menus(SrcMenuPtr).Guid));
                            // okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ParentName, CollectionSrc.Menus(SrcPtr).ParentName)
                        }
                        
                        // Exit For
                    }
                    
                    if (okToUpdateDstFromSrc) {
                        // With...
                        // 
                        //  It okToUpdateDstFromSrc, update the Dst with the Src
                        // 
                        srcCollection.Menus(SrcMenuPtr).NavIconType.NavIconTitle = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).AddonName.NavIconType = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).SortOrder.AddonName = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).menuNameSpace.SortOrder = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).ParentName.menuNameSpace = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).NewWindow.ParentName = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).LinkPage.NewWindow = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).DeveloperOnly.LinkPage = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).ContentName.DeveloperOnly = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).AdminOnly.ContentName = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).Active.AdminOnly = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).IsNavigator.Active = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).Name.IsNavigator = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        srcCollection.Menus(SrcMenuPtr).Guid.Name = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        true.Guid = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                        dstCollection.Menus(DstMenuPtr).dataChanged = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
                    }
                    
                }
                
                // '
                // '-------------------------------------------------------------------------------------------------
                // 
                // -------------------------------------------------------------------------------------------------
                //  Check styles
                // -------------------------------------------------------------------------------------------------
                // 
                int srcStylePtr;
                int dstStylePtr;
                for (srcStylePtr = 0; (srcStylePtr 
                            <= (srcCollection.StyleCnt - 1)); srcStylePtr++) {
                    SrcContentName = genericController.vbLCase(srcCollection.Styles(srcStylePtr).Name);
                    okToUpdateDstFromSrc = false;
                    for (dstStylePtr = 0; (dstStylePtr 
                                <= (dstCollection.StyleCnt - 1)); dstStylePtr++) {
                        DstName = genericController.vbLCase(dstCollection.Styles(dstStylePtr).Name);
                        if ((DstName == SrcContentName)) {
                            // 
                            //  found a match between Src and Dst
                            // 
                            if (SrcIsUserCDef) {
                                // 
                                //  test for cdef attribute changes
                                // 
                                // With...
                                okToUpdateDstFromSrc = (okToUpdateDstFromSrc 
                                            | !addonInstallClass.TextMatch(cpCore, dstCollection.Styles(dstStylePtr).Copy, srcCollection.Styles(srcStylePtr).Copy));
                            }
                            
                            break;
                        }
                        
                    }
                    
                    if ((dstStylePtr == dstCollection.StyleCnt)) {
                        // 
                        //  CDef was not found, add it
                        // 
                        object Preserve;
                        dstCollection.Styles(dstCollection.StyleCnt);
                        dstCollection.StyleCnt = (dstStylePtr + 1);
                        okToUpdateDstFromSrc = true;
                    }
                    
                    if (okToUpdateDstFromSrc) {
                        // With...
                        // 
                        //  It okToUpdateDstFromSrc, update the Dst with the Src
                        // 
                        srcCollection.Styles(srcStylePtr).Copy.Name = srcCollection.Styles(srcStylePtr).Name;
                        true.Copy = srcCollection.Styles(srcStylePtr).Name;
                        dstCollection.Styles(dstStylePtr).dataChanged = srcCollection.Styles(srcStylePtr).Name;
                    }
                    
                }
                
                // 
                // -------------------------------------------------------------------------------------------------
                //  Add Collections
                // -------------------------------------------------------------------------------------------------
                // 
                int dstPtr = 0;
                for (SrcPtr = 0; (SrcPtr 
                            <= (srcCollection.ImportCnt - 1)); SrcPtr++) {
                    dstPtr = dstCollection.ImportCnt;
                    object Preserve;
                    dstCollection.collectionImports(dstPtr);
                    dstCollection.collectionImports(dstPtr) = srcCollection.collectionImports(SrcPtr);
                    dstCollection.ImportCnt = (dstPtr + 1);
                }
                
                // 
                // -------------------------------------------------------------------------------------------------
                //  Page Templates
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Site Sections
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Dynamic Menus
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Page Content
                // -------------------------------------------------------------------------------------------------
                // 
                // 
                // -------------------------------------------------------------------------------------------------
                //  Copy Content
                // -------------------------------------------------------------------------------------------------
                // 
                // 
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnOk;
        }
        
        // '
        // '===========================================================================
        // '   Error handler
        // '===========================================================================
        // '
        // Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ResumeNext As Boolean = False)
        //     '
        //     'Call App.LogEvent("addonInstallClass.HandleClassTrapError called from " & MethodName)
        //     '
        //    throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError3(ApplicationName, "unknown", "dll", "AddonInstallClass", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        //     '
        // End Sub
        // 
        // 
        // 
        private static miniCollectionModel installCollection_GetApplicationMiniCollection(coreClass cpCore, bool isNewBuild) {
            miniCollectionModel returnColl = new miniCollectionModel();
            try {
                // 
                string ExportFilename;
                string ExportPathPage;
                string CollectionData;
                // 
                if (!isNewBuild) {
                    // 
                    //  if this is not an empty database, get the application collection, else return empty
                    // 
                    ExportFilename = ("cdef_export_" 
                                + (genericController.GetRandomInteger().ToString() + ".xml"));
                    ExportPathPage = ("tmp\\" + ExportFilename);
                    exportApplicationCDefXml(cpCore, ExportPathPage, true);
                    CollectionData = cpCore.privateFiles.readFile(ExportPathPage);
                    cpCore.privateFiles.deleteFile(ExportPathPage);
                    returnColl = addonInstallClass.installCollection_LoadXmlToMiniCollection(cpCore, CollectionData, false, false, isNewBuild, new miniCollectionModel());
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnColl;
        }
        
        // 
        // ========================================================================
        //  ----- Get an XML nodes attribute based on its name
        // ========================================================================
        // 
        public static string GetXMLAttribute(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string returnAttr = "";
            try {
                XmlAttribute NodeAttribute;
                XmlNode ResultNode;
                string UcaseName;
                // 
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if ((ResultNode == null)) {
                    UcaseName = genericController.vbUCase(Name);
                    foreach (NodeAttribute in Node.Attributes) {
                        if ((genericController.vbUCase(NodeAttribute.Name) == UcaseName)) {
                            returnAttr = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                        
                    }
                    
                    if (!Found) {
                        returnAttr = DefaultIfNotFound;
                    }
                    
                }
                else {
                    returnAttr = ResultNode.Value;
                    Found = true;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnAttr;
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private static double GetXMLAttributeNumber(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            return EncodeNumber(addonInstallClass.GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound));
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private static bool GetXMLAttributeBoolean(coreClass cpCore, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return genericController.EncodeBoolean(addonInstallClass.GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private static int GetXMLAttributeInteger(coreClass cpCore, bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return genericController.EncodeInteger(addonInstallClass.GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        
        // 
        // ==================================================================================================================
        // 
        // ==================================================================================================================
        // 
        private static bool TextMatch(coreClass cpCore, string Source1, string Source2) {
            return (Source1.ToLower() == genericController.vbLCase(Source2));
        }
        
        // 
        // 
        // 
        private static string GetMenuNameSpace(coreClass cpCore, miniCollectionModel Collection, int MenuPtr, bool IsNavigator, string UsedIDList) {
            string returnAttr = "";
            try {
                string ParentName;
                int Ptr;
                string Prefix;
                string LCaseParentName;
                // 
                // With...
                ParentName = Collection.Menus;
                MenuPtr.ParentName;
                if ((ParentName != "")) {
                    LCaseParentName = genericController.vbLCase(ParentName);
                    for (Ptr = 0; (Ptr 
                                <= (Collection.MenuCnt - 1)); Ptr++) {
                        if ((genericController.vbInstr(1, ("," 
                                        + (UsedIDList + ",")), ("," 
                                        + (Ptr.ToString() + ","))) == 0)) {
                            if (((LCaseParentName == genericController.vbLCase(Collection.Menus, Ptr.Name)) 
                                        && (IsNavigator == Collection.Menus)[Ptr].IsNavigator)) {
                                Prefix = addonInstallClass.GetMenuNameSpace(cpCore, Collection, Ptr, IsNavigator, (UsedIDList + ("," + MenuPtr)));
                                if ((Prefix == "")) {
                                    returnAttr = ParentName;
                                }
                                else {
                                    returnAttr = (Prefix + ("." + ParentName));
                                }
                                
                                break;
                            }
                            
                        }
                        
                    }
                    
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return returnAttr;
        }
        
        // 
        // =============================================================================
        //    Create an entry in the Sort Methods Table
        // =============================================================================
        // 
        private static void VerifySortMethod(coreClass cpCore, string Name, string OrderByCriteria) {
            try {
                // 
                DataTable dt;
                sqlFieldListClass sqlList = new sqlFieldListClass();
                // 
                sqlList.add("name", cpCore.db.encodeSQLText(Name));
                sqlList.add("CreatedBy", "0");
                sqlList.add("OrderByClause", cpCore.db.encodeSQLText(OrderByCriteria));
                sqlList.add("active", SQLTrue);
                sqlList.add("ContentControlID", Models.Complex.cdefModel.getContentId(cpCore, "Sort Methods").ToString());
                // 
                dt = cpCore.db.openTable("Default", "ccSortMethods", ("Name=" + cpCore.db.encodeSQLText(Name)), "ID", "ID", 1);
                if ((dt.Rows.Count > 0)) {
                    // 
                    //  update sort method
                    // 
                    cpCore.db.updateTableRecord("Default", "ccSortMethods", ("ID=" + genericController.EncodeInteger(dt.Rows[0].Item["ID"]).ToString), sqlList);
                }
                else {
                    // 
                    //  Create the new sort method
                    // 
                    cpCore.db.insertTableRecord("Default", "ccSortMethods", sqlList);
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // =========================================================================================
        // 
        // =========================================================================================
        // 
        public static void VerifySortMethods(coreClass cpCore) {
            try {
                // 
                logController.appendInstallLog(cpCore, "Verify Sort Records");
                // 
                addonInstallClass.VerifySortMethod(cpCore, "By Name", "Name");
                addonInstallClass.VerifySortMethod(cpCore, "By Alpha Sort Order Field", "SortOrder");
                addonInstallClass.VerifySortMethod(cpCore, "By Date", "DateAdded");
                addonInstallClass.VerifySortMethod(cpCore, "By Date Reverse", "DateAdded Desc");
                addonInstallClass.VerifySortMethod(cpCore, "By Alpha Sort Order Then Oldest First", "SortOrder,ID");
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // =============================================================================
        //    Get a ContentID from the ContentName using just the tables
        // =============================================================================
        // 
        private static void VerifyContentFieldTypes(coreClass cpCore) {
            try {
                // 
                int RowsFound;
                int CID;
                bool TableBad;
                int RowsNeeded;
                // 
                //  ----- make sure there are enough records
                // 
                TableBad = false;
                RowsFound = 0;
                Using;
                ((DataTable)(rs)) = cpCore.db.executeQuery("Select ID from ccFieldTypes order by id");
                if (!isDataTableOk(rs)) {
                    // 
                    //  problem
                    // 
                    TableBad = true;
                }
                else {
                    // 
                    //  Verify the records that are there
                    // 
                    RowsFound = 0;
                    foreach (DataRow dr in rs.Rows) {
                        RowsFound = (RowsFound + 1);
                        if ((RowsFound != genericController.EncodeInteger(dr.Item["ID"]))) {
                            // 
                            //  Bad Table
                            // 
                            TableBad = true;
                            break;
                        }
                        
                    }
                    
                }
                
            }
            
            // 
            //  ----- Replace table if needed
            // 
            if (TableBad) {
                cpCore.db.deleteTable("Default", "ccFieldTypes");
                cpCore.db.createSQLTable("Default", "ccFieldTypes");
                RowsFound = 0;
            }
            
            // 
            //  ----- Add the number of rows needed
            // 
            RowsNeeded = (FieldTypeIdMax - RowsFound);
            if ((RowsNeeded > 0)) {
                CID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Types");
                if ((CID <= 0)) {
                    // 
                    //  Problem
                    // 
                    cpCore.handleException(new ApplicationException("Content Field Types content definition was not found"));
                }
                else {
                    while ((RowsNeeded > 0)) {
                        cpCore.db.executeQuery(("Insert into ccFieldTypes (active,contentcontrolid)values(1," 
                                        + (CID + ")")));
                        RowsNeeded = (RowsNeeded - 1);
                    }
                    
                }
                
            }
            
            // 
            //  ----- Update the Names of each row
            // 
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Integer\' where ID=1;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Text\' where ID=2;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'LongText\' where ID=3;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Boolean\' where ID=4;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Date\' where ID=5;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'File\' where ID=6;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Lookup\' where ID=7;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Redirect\' where ID=8;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Currency\' where ID=9;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'TextFile\' where ID=10;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Image\' where ID=11;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Float\' where ID=12;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'AutoIncrement\' where ID=13;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'ManyToMany\' where ID=14;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Member Select\' where ID=15;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'CSS File\' where ID=16;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'XML File\' where ID=17;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Javascript File\' where ID=18;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Link\' where ID=19;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'Resource Link\' where ID=20;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'HTML\' where ID=21;");
            cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name=\'HTML File\' where ID=22;");
            ((Exception)(ex));
            cpCore.handleException(ex);
            throw;
        }
    }
}
EndIfcpCore.db.csOk(CSEntry);
ThencpCore.db.csSet(CSEntry, "Link", Link);
cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
EndIfcpCore.db.csClose(CSEntry);
CatchException ex;
cpCore.handleException(ex);
throw;
Endtry {
}

Endclass End {
}

    
    // 
    // =============================================================================
    // 
    // =============================================================================
    // 
    public void csv_VerifyAggregateReplacement(coreClass cpcore, string Name, string Copy, string SortOrder) {
        csv_VerifyAggregateReplacement2(cpcore, Name, Copy, "", SortOrder);
    }
    
    // 
    // =============================================================================
    // 
    // =============================================================================
    // 
    public static void csv_VerifyAggregateReplacement2(coreClass cpCore, string Name, string Copy, string ArgumentList, string SortOrder) {
        try {
            // 
            int CSEntry;
            string ContentName;
            string MethodName;
            // 
            MethodName = "csv_VerifyAggregateReplacement2";
            ContentName = "Aggregate Function Replacements";
            CSEntry = cpCore.db.csOpen(ContentName, ("(name=" 
                            + (cpCore.db.encodeSQLText(Name) + ")")), ,, false, ,, ,, "Name,Copy,SortOrder,ArgumentList");
            // 
            //  If no current entry, create one
            // 
            if (!cpCore.db.csOk(CSEntry)) {
                cpCore.db.csClose(CSEntry);
                CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID);
                if (cpCore.db.csOk(CSEntry)) {
                    cpCore.db.csSet(CSEntry, "name", Name);
                }
                
            }
            
            if (cpCore.db.csOk(CSEntry)) {
                cpCore.db.csSet(CSEntry, "Copy", Copy);
                cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
                cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
            }
            
            cpCore.db.csClose(CSEntry);
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // 
    // =============================================================================
    // 
    // =============================================================================
    // 
    public void csv_VerifyAggregateObject(coreClass cpcore, string Name, string ObjectProgramID, string ArgumentList, string SortOrder) {
        try {
            // 
            int CSEntry;
            string ContentName;
            string MethodName;
            // 
            MethodName = "csv_VerifyAggregateObject";
            ContentName = "Aggregate Function Objects";
            CSEntry = cpcore.db.csOpen(ContentName, ("(name=" 
                            + (cpcore.db.encodeSQLText(Name) + ")")), ,, false, ,, ,, "Name,Link,ObjectProgramID,ArgumentList,SortOrder");
            // 
            //  If no current entry, create one
            // 
            if (!cpcore.db.csOk(CSEntry)) {
                cpcore.db.csClose(CSEntry);
                CSEntry = cpcore.db.csInsertRecord(ContentName, SystemMemberID);
                if (cpcore.db.csOk(CSEntry)) {
                    cpcore.db.csSet(CSEntry, "name", Name);
                }
                
            }
            
            if (cpcore.db.csOk(CSEntry)) {
                cpcore.db.csSet(CSEntry, "ObjectProgramID", ObjectProgramID);
                cpcore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
                cpcore.db.csSet(CSEntry, "SortOrder", SortOrder);
            }
            
            cpcore.db.csClose(CSEntry);
        }
        catch (Exception ex) {
            cpcore.handleException(ex);
            throw;
        }
        
    }
    
    // 
    // ========================================================================
    // 
    // ========================================================================
    // 
    public static void exportApplicationCDefXml(coreClass cpCore, string privateFilesPathFilename, bool IncludeBaseFields) {
        try {
            xmlController XML;
            string Content;
            // 
            XML = new xmlController(cpCore);
            Content = XML.GetXMLContentDefinition3("", IncludeBaseFields);
            cpCore.privateFiles.saveFile(privateFilesPathFilename, Content);
            XML = null;
        }
        catch (Exception ex) {
            cpCore.handleException(ex);
            throw;
        }
        
    }