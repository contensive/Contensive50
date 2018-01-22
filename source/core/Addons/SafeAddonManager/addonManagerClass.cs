﻿
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
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Addons.SafeAddonManager {
    public class addonManagerClass {
        //
        // constructor sets cp from argument for use in calls to other objects, then core because cp cannot be uses since that would be a circular depenancy
        //
        private coreController core;
        //
        // To interigate Add-on Collections to check for re-use
        //
        //private struct DeleteType {
        //    public string Name;
        //    public int ParentID;
        //}
        //private struct NavigatorType {
        //    public string Name;
        //    public string menuNameSpace;
        //}
        //private struct Collection2Type {
        //    public int AddOnCnt;
        //    public string[] AddonGuid;
        //    public string[] AddonName;
        //    public int MenuCnt;
        //    public string[] Menus;
        //    public int NavigatorCnt;
        //    public NavigatorType[] Navigators;
        //}
        //private int CollectionCnt;
        //private Collection2Type[] Collections;
        //
        //==================================================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <remarks></remarks>
        public addonManagerClass(coreController core) : base() {
            this.core = core;
        }
        //
        //==========================================================================================================================================
        //   Addon Manager
        //       This is a form that lets you upload an addon
        //       Eventually, this should be substituted with a "Addon Manager Addon" - so the interface can be improved with Contensive recompile
        //==========================================================================================================================================
        //
        public string GetForm_SafeModeAddonManager() {
            string addonManager = "";
            try {
                string LocalCollectionXML = null;
                bool DisplaySystem = false;
                bool DbUpToDate = false;
                collectionXmlController XMLTools = new collectionXmlController(core);
                string GuidFieldName = null;
                List<int> InstalledCollectionIDList = new List<int>();
                List<string> InstalledCollectionGuidList = new List<string>();
                DateTime DateValue = default(DateTime);
                string ErrorMessage = "";
                string OnServerGuidList = "";
                bool UpgradeOK = false;
                string RegisterList = "";
                XmlDocument LocalCollections = null;
                XmlDocument LibCollections = null;
                string InstallFolder = null;
                //Dim LibGuids() As String
                //Dim IISResetRequired As Boolean
                int AddonNavigatorID = 0;
                string TargetCollectionName = null;
                string Collectionname = "";
                string CollectionGuid = "";
                string CollectionVersion = null;
                string CollectionDescription = "";
                string CollectionContensiveVersion = "";
                string CollectionLastChangeDate = "";
                string[,] Cells3 = null;
                string FormInput = null;
                int Cnt = 0;
                int Ptr = 0;
                stringBuilderLegacyController UploadTab = new stringBuilderLegacyController();
                stringBuilderLegacyController ModifyTab = new stringBuilderLegacyController();
                int RowPtr = 0;
                stringBuilderLegacyController Body = null;
                string[,] Cells = null;
                int ColumnCnt = 0;
                string[] ColCaption = null;
                string[] ColAlign = null;
                string[] ColWidth = null;
                bool[] ColSortable = null;
                string PostTableCopy = "";
                string BodyHTML = null;
                int CS = 0;
                string UserError = null;
                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                string Button = null;
                string Caption = null;
                string Description = null;
                string ButtonList = "";
                string CollectionFilename = null;
                string status = "";
                bool AllowInstallFromFolder = false;
                string InstallLibCollectionList = "";
                int TargetCollectionID = 0;
                string privateFilesInstallPath = null;
                adminUIController Adminui = new adminUIController(core);
                List<string> nonCriticalErrorList = new List<string>();
                //
                // BuildVersion = core.app.dataBuildVersion
                string dataBuildVersion = core.siteProperties.dataBuildVersion;
                string coreVersion = core.codeVersion();

                DbUpToDate = (dataBuildVersion == coreVersion);
                //
                Button = core.docProperties.getText(constants.RequestNameButton);
                AllowInstallFromFolder = false;
                GuidFieldName = "ccguid";
                if (Button == constants.ButtonCancel) {
                    //
                    // ----- redirect back to the root
                    //
                    addonManager = core.webServer.redirect("/" + core.serverConfig.appConfig.adminRoute, "Addon Manager, Cancel Button Pressed");
                } else {
                    if (!core.doc.sessionContext.isAuthenticatedAdmin(core)) {
                        //
                        // ----- Put up error message
                        //
                        ButtonList = constants.ButtonCancel;
                        Content.Add(Adminui.GetFormBodyAdminOnly());
                    } else {
                        //
                        InstallFolder = "temp\\CollectionUpload" + encodeText(genericController.GetRandomInteger(core));
                        privateFilesInstallPath = InstallFolder + "\\";
                        if (Button == constants.ButtonOK) {
                            //
                            //---------------------------------------------------------------------------------------------
                            // Download and install Collections from the Collection Library
                            //---------------------------------------------------------------------------------------------
                            //
                            if (core.docProperties.getText("LibraryRow") != "") {
                                Ptr = core.docProperties.getInteger("LibraryRow");
                                //If core.main_GetStreamBoolean2("LibraryRow" & Ptr) Then
                                CollectionGuid = core.docProperties.getText("LibraryRowguid" + Ptr);
                                InstallLibCollectionList = InstallLibCollectionList + "," + CollectionGuid;
                            }

                            //                Cnt = core.main_GetStreamInteger2("LibraryCnt")
                            //                If Cnt > 0 Then
                            //                    For Ptr = 0 To Cnt - 1
                            //                        If core.main_GetStreamText2("LibraryRow") <> "" Then
                            //                        'If core.main_GetStreamBoolean2("LibraryRow" & Ptr) Then
                            //                            CollectionGUID = core.main_GetStreamText2("LibraryRowguid" & Ptr)
                            //                            InstallLibCollectionList = InstallLibCollectionList & "," & CollectionGUID
                            //                        End If
                            //                    Next
                            //                End If
                            //
                            //---------------------------------------------------------------------------------------------
                            // Delete collections
                            //   Before deleting each addon, make sure it is not in another collection
                            //---------------------------------------------------------------------------------------------
                            //
                            Cnt = core.docProperties.getInteger("accnt");
                            if (Cnt > 0) {
                                for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                    if (core.docProperties.getBoolean("ac" + Ptr)) {
                                        TargetCollectionID = core.docProperties.getInteger("acID" + Ptr);
                                        TargetCollectionName = core.db.getRecordName("Add-on Collections", TargetCollectionID);
                                        //
                                        // Delete any addons from this collection
                                        //
                                        core.db.deleteContentRecords(constants.cnAddons, "collectionid=" + TargetCollectionID);

                                        //                            '
                                        //                            ' Load all collections into local collection storage
                                        //                            '
                                        //                            TargetCollectionID = core.main_GetStreamInteger2("acID" & Ptr)
                                        //                            CS = core.app.csOpen("Add-on Collections")
                                        //                            CollectionCnt = 0
                                        //                            TargetCollectionPtr = -1
                                        //                            Do While core.asv.csv_IsCSOK(CS)
                                        //                                ReDim Preserve Collections(CollectionCnt)
                                        //                                CollectionID = core.app.cs_getInteger(CS, "ID")
                                        //                                CollectionName = core.db.cs_getText(CS, "Name")
                                        //                                If CollectionID = TargetCollectionID Then
                                        //                                    TargetCollectionPtr = CollectionCnt
                                        //                                    'TargetCollectionPtr = Ptr
                                        //                                    TargetCollectionName = CollectionName
                                        //                                End If
                                        //                                '
                                        //                                ' Get collection addons
                                        //                                '
                                        //                                If true Then
                                        //                                    SQL = "select A.ID,A.name,A." & GuidFieldName _
                                        //                                        & " from ccAggregateFunctions A" _
                                        //                                        & " where a.CollectionID=" & CollectionID
                                        //                                Else
                                        //                                    SQL = "select A.ID,A.name,A." & GuidFieldName _
                                        //                                        & " from ccAggregateFunctions A" _
                                        //                                        & " left join ccAddonCollectionRules R on R.AddonID=A.ID" _
                                        //                                        & " where R.CollectionID=" & CollectionID
                                        //                                End If
                                        //                                CSAddons = core.app.openCsSql_rev("default", SQL)
                                        //                                Do While core.asv.csv_IsCSOK(CSAddons)
                                        //                                    AddonCnt = Collections(CollectionCnt).AddonCnt
                                        //                                    ReDim Preserve Collections(CollectionCnt).AddonName(AddonCnt)
                                        //                                    ReDim Preserve Collections(CollectionCnt).AddonGuid(AddonCnt)
                                        //                                    addonid = core.app.cs_getInteger(CSAddons, "ID")
                                        //                                    Collections(CollectionCnt).AddonGuid(AddonCnt) = core.db.cs_getText(CSAddons, GuidFieldName)
                                        //                                    Collections(CollectionCnt).AddonName(AddonCnt) = core.db.cs_getText(CSAddons, "Name")
                                        //                                    Collections(CollectionCnt).AddonCnt = AddonCnt + 1
                                        //                                    Call core.app.nextCSRecord(CSAddons)
                                        //                                Loop
                                        //                                Call core.app.closeCS(CSAddons)
                                        //                                '
                                        //                                ' GetCDefs from Collection File to remove cdefs and navigators
                                        //                                '
                                        //                                CollectionFile = core.app.cs_get(CS, "InstallFile")
                                        //                                If CollectionFile <> "" Then
                                        //                                    Call Doc.loadXML(CollectionFile)
                                        //                                    If Doc.parseError.ErrorCode <> 0 Then
                                        //                                        '
                                        //                                        ' ********************** add status message here
                                        //                                        '
                                        //                                    Else
                                        //                                        With Doc.documentElement
                                        //                                            If genericController.vbLCase(.baseName) = "collection" Then
                                        //                                                CollectionName = GetXMLAttribute(IsFound, Doc.documentElement, "name", "")
                                        //                                                CollectionSystem = genericController.EncodeBoolean(GetXMLAttribute(IsFound, Doc.documentElement, "system", ""))
                                        //                                                For Each CDef_Node In .childNodes
                                        //                                                    Select Case genericController.vbLCase(CDef_Node.name)
                                        //                                                        Case "interfaces"
                                        //                                                            For Each InterfaceNode In CDef_Node.childNodes
                                        //                                                                Select Case genericController.vbLCase(InterfaceNode.name)
                                        //                                                                    Case "page"
                                        //                                                                        For Each PageNode In InterfaceNode.childNodes
                                        //                                                                            If genericController.vbLCase(PageNode.name) = "navigator" Then
                                        //                                                                                NavigatorCnt = Collections(CollectionCnt).NavigatorCnt
                                        //                                                                                ReDim Preserve Collections(CollectionCnt).Navigators(NavigatorCnt)
                                        //                                                                                Collections(CollectionCnt).Navigators(NavigatorCnt).name = GetXMLAttribute(IsFound, PageNode, "name", "")
                                        //                                                                                Collections(CollectionCnt).Navigators(NavigatorCnt).NameSpace = GetXMLAttribute(IsFound, PageNode, "NameSpace", "")
                                        //                                                                                Collections(CollectionCnt).NavigatorCnt = NavigatorCnt + 1
                                        //                                                                            End If
                                        //                                                                        Next
                                        //                                                                    Case "setting"
                                        //                                                                End Select
                                        //                                                            Next
                                        //                                                        Case "contensivecdef"
                                        //                                                            '
                                        //                                                            ' load Navigator Entries
                                        //                                                            '
                                        //                                                            For Each CDefNode In CDef_Node.childNodes
                                        //                                                                If genericController.vbLCase(CDefNode.name) = "adminmenu" Then
                                        //                                                                    MenuCnt = Collections(CollectionCnt).MenuCnt
                                        //                                                                    ReDim Preserve Collections(CollectionCnt).Menus(MenuCnt)
                                        //                                                                    Collections(CollectionCnt).Menus(MenuCnt) = GetXMLAttribute(IsFound, CDefNode, "name", "")
                                        //                                                                    Collections(CollectionCnt).MenuCnt = MenuCnt + 1
                                        //                                                                End If
                                        //                                                                If genericController.vbLCase(CDefNode.name) = "navigatorentry" Then
                                        //                                                                    NavigatorCnt = Collections(CollectionCnt).NavigatorCnt
                                        //                                                                    ReDim Preserve Collections(CollectionCnt).Navigators(NavigatorCnt)
                                        //                                                                    Collections(CollectionCnt).Navigators(NavigatorCnt).name = GetXMLAttribute(IsFound, CDefNode, "name", "")
                                        //                                                                    Collections(CollectionCnt).Navigators(NavigatorCnt).NameSpace = GetXMLAttribute(IsFound, CDefNode, "NameSpace", "")
                                        //                                                                    Collections(CollectionCnt).NavigatorCnt = NavigatorCnt + 1
                                        //                                                                End If
                                        //                                                            Next
                                        //                                                    End Select
                                        //                                                Next
                                        //                                            End If
                                        //                                        End With
                                        //                                    End If
                                        //                                End If
                                        //                                CollectionCnt = CollectionCnt + 1
                                        //                                core.main_NextCSRecord (CS)
                                        //                            Loop
                                        //                            Call core.app.closeCS(CS)
                                        //                            '
                                        //                            ' Search through the local collection storage for the addons in the one we want to delete
                                        //                            '   if not in any other collections, delete the addon from the system
                                        //                            '
                                        //                            If true Then
                                        //                                '
                                        //                                ' delete all addons associated to this collection
                                        //                                '
                                        //                                Call core.app.DeleteContentRecords(cnAddons, "collectionid=" & TargetCollectionID)
                                        //                            Else
                                        //                                ' deprecated the addoncollectionrules for collectionid in addon
                                        //                                If (TargetCollectionPtr >= 0) And (CollectionCnt <> 0) Then
                                        //                                    TargetAddonCnt = Collections(TargetCollectionPtr).AddonCnt
                                        //                                    For TargetAddonPtr = 0 To TargetAddonCnt - 1
                                        //                                        TargetAddonName = Collections(TargetCollectionPtr).AddonName(TargetAddonPtr)
                                        //                                        TargetAddonGUID = Collections(TargetCollectionPtr).AddonGuid(TargetAddonPtr)
                                        //                                        UseGUID = (TargetAddonGUID <> "")
                                        //                                        KeepTarget = False
                                        //                                        For SearchCPtr = 0 To CollectionCnt - 1
                                        //                                            If SearchCPtr <> TargetCollectionPtr Then
                                        //                                                With Collections(SearchCPtr)
                                        //                                                    For SearchAPtr = 0 To .AddonCnt - 1
                                        //                                                        If UseGUID Then
                                        //                                                            If TargetAddonGUID = .AddonGuid(SearchAPtr) Then
                                        //                                                                KeepTarget = True
                                        //                                                                Exit For
                                        //                                                            End If
                                        //                                                        Else
                                        //                                                            If TargetAddonName = .AddonName(SearchAPtr) Then
                                        //                                                                KeepTarget = True
                                        //                                                                Exit For
                                        //                                                            End If
                                        //                                                        End If
                                        //                                                    Next
                                        //                                                End With
                                        //                                            End If
                                        //                                        Next
                                        //                                        If Not KeepTarget Then
                                        //                                            '
                                        //                                            ' OK to delete the target addon
                                        //                                            '
                                        //                                            If UseGUID Then
                                        //                                                Criteria = "(" & GuidFieldName & "=" & encodeSQLText(TargetAddonGUID) & ")"
                                        //                                            Else
                                        //                                                Criteria = "(name=" & encodeSQLText(TargetAddonName) & ")"
                                        //                                            End If
                                        //                                            Call core.app.DeleteContentRecords(cnAddons, Criteria)
                                        //                                        End If
                                        //                                    Next
                                        //                                End If
                                        //                                '
                                        //                                ' Delete Navigator Entries not used by other Collections
                                        //                                '
                                        //                                TargetCnt = Collections(TargetCollectionPtr).MenuCnt
                                        //                                For TargetPtr = 0 To TargetCnt - 1
                                        //                                    TargetName = Collections(TargetCollectionPtr).Menus(TargetPtr)
                                        //                                    KeepTarget = False
                                        //                                    For SearchCPtr = 0 To CollectionCnt - 1
                                        //                                        If SearchCPtr <> TargetCollectionPtr Then
                                        //                                            With Collections(SearchCPtr)
                                        //                                                For SearchMPtr = 0 To .MenuCnt - 1
                                        //                                                    If TargetName = .Menus(SearchMPtr) Then
                                        //                                                        KeepTarget = True
                                        //                                                        Exit For
                                        //                                                    End If
                                        //                                                Next
                                        //                                            End With
                                        //                                        End If
                                        //                                    Next
                                        //                                    If Not KeepTarget Then
                                        //                                        '
                                        //                                        ' OK to delete the target addon
                                        //                                        '
                                        //                                        Call core.app.DeleteContentRecords(cnNavigatorEntries, "(name=" & encodeSQLText(TargetName) & ")")
                                        //                                    End If
                                        //                                Next
                                        //                                '
                                        //                                ' Delete Navigator Entries not used by other Collections
                                        //                                '
                                        //                                TargetCnt = Collections(TargetCollectionPtr).NavigatorCnt
                                        //                                For TargetPtr = 0 To TargetCnt - 1
                                        //                                    KeepTarget = False
                                        //                                    TargetName = Collections(TargetCollectionPtr).Navigators(TargetPtr).name
                                        //                                    TargetNameSpace = Collections(TargetCollectionPtr).Navigators(TargetPtr).NameSpace
                                        //                                    If TargetNameSpace = "" Then
                                        //                                        '
                                        //                                        ' Can not delete root nodes
                                        //                                        '
                                        //                                        KeepTarget = True
                                        //                                    Else
                                        //                                        For SearchCPtr = 0 To CollectionCnt - 1
                                        //                                            If SearchCPtr <> TargetCollectionPtr Then
                                        //                                                With Collections(SearchCPtr)
                                        //                                                    For SearchMPtr = 0 To .NavigatorCnt - 1
                                        //                                                        If (TargetName = .Navigators(SearchMPtr).name) And (TargetNameSpace = .Navigators(SearchMPtr).NameSpace) Then
                                        //                                                            KeepTarget = True
                                        //                                                            Exit For
                                        //                                                        End If
                                        //                                                    Next
                                        //                                                End With
                                        //                                            End If
                                        //                                        Next
                                        //                                    End If
                                        //                                    If Not KeepTarget Then
                                        //                                        '
                                        //                                        ' OK to delete the target addon
                                        //                                        '
                                        //                                        ParentID = GetParentIDFromNameSpace(cnNavigatorEntries, TargetNameSpace)
                                        //                                        ReDim Preserve Deletes(DeleteCnt)
                                        //                                        Deletes(DeleteCnt).name = TargetName
                                        //                                        Deletes(DeleteCnt).ParentID = ParentID
                                        //                                        DeleteCnt = DeleteCnt + 1
                                        //                                    End If
                                        //                                Next
                                        //                            End If
                                        //                            '
                                        //                            ' Delete any navigator entries found
                                        //                            '
                                        //                            If DeleteCnt > 0 Then
                                        //                                For DeletePtr = 0 To DeleteCnt - 1
                                        //                                    Call GetForm_SafeModeAddonManager_DeleteNavigatorBranch(Deletes(DeletePtr).name, Deletes(DeletePtr).ParentID)
                                        //                                Next
                                        //                            End If
                                        //
                                        // Delete the navigator entry for the collection under 'Add-ons'
                                        //
                                        if (TargetCollectionID > 0) {
                                            AddonNavigatorID = 0;
                                            CS = core.db.csOpen(constants.cnNavigatorEntries, "name='Manage Add-ons' and ((parentid=0)or(parentid is null))");
                                            if (core.db.csOk(CS)) {
                                                AddonNavigatorID = core.db.csGetInteger(CS, "ID");
                                            }
                                            core.db.csClose( ref CS);
                                            if (AddonNavigatorID > 0) {
                                                GetForm_SafeModeAddonManager_DeleteNavigatorBranch(TargetCollectionName, AddonNavigatorID);
                                            }
                                            //
                                            // Now delete the Collection record
                                            //
                                            core.db.deleteContentRecord("Add-on Collections", TargetCollectionID);
                                            //
                                            // Delete Navigator Entries set as installed by the collection (this may be all that is needed)
                                            //
                                            core.db.deleteContentRecords(constants.cnNavigatorEntries, "installedbycollectionid=" + TargetCollectionID);
                                        }
                                    }
                                }
                            }
                            //
                            //---------------------------------------------------------------------------------------------
                            // Delete Add-ons
                            //---------------------------------------------------------------------------------------------
                            //
                            Cnt = core.docProperties.getInteger("aocnt");
                            if (Cnt > 0) {
                                for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                    if (core.docProperties.getBoolean("ao" + Ptr)) {
                                        core.db.deleteContentRecord(constants.cnAddons, core.docProperties.getInteger("aoID" + Ptr));
                                    }
                                }
                            }
                            //
                            //---------------------------------------------------------------------------------------------
                            // Reinstall core collection
                            //---------------------------------------------------------------------------------------------
                            //
                            if (core.doc.sessionContext.isAuthenticatedDeveloper(core) & core.docProperties.getBoolean("InstallCore")) {
                                UpgradeOK = collectionController.installCollectionFromRemoteRepo(core, "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}", ref ErrorMessage, "", false, ref nonCriticalErrorList);
                            }
                            //
                            //---------------------------------------------------------------------------------------------
                            // Upload new collection files
                            //---------------------------------------------------------------------------------------------
                            //
                            List<string> uploadedCollectionPathFilenames = new List<string>();
                            CollectionFilename = "";
                            if (core.privateFiles.upload("MetaFile", InstallFolder, ref CollectionFilename)) {
                                status += "<br>Uploaded collection file [" + CollectionFilename + "]";
                                uploadedCollectionPathFilenames.Add(InstallFolder + CollectionFilename);
                                AllowInstallFromFolder = true;
                            }
                            //
                            //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of core.docProperties.getInteger("UploadCount") for every iteration:
                            int tempVar = core.docProperties.getInteger("UploadCount");
                            for (Ptr = 0; Ptr < tempVar; Ptr++) {
                                if (core.privateFiles.upload("Upload" + Ptr, InstallFolder, ref CollectionFilename)) {
                                    status += "<br>Uploaded collection file [" + CollectionFilename + "]";
                                    uploadedCollectionPathFilenames.Add(InstallFolder + CollectionFilename);
                                    AllowInstallFromFolder = true;
                                }
                            }
                        }
                        //
                        // --------------------------------------------------------------------------------
                        //   Install Library Collections
                        // --------------------------------------------------------------------------------
                        //
                        if (!string.IsNullOrEmpty(InstallLibCollectionList)) {
                            InstallLibCollectionList = InstallLibCollectionList.Substring(1);
                            string[] LibGuids = InstallLibCollectionList.Split(',');
                            Cnt = LibGuids.GetUpperBound(0) + 1;
                            for (Ptr = 0; Ptr < Cnt; Ptr++) {
                                RegisterList = "";
                                UpgradeOK = collectionController.installCollectionFromRemoteRepo(core, LibGuids[Ptr], ref ErrorMessage, "", false, ref nonCriticalErrorList);
                                if (!UpgradeOK) {
                                    //
                                    // block the reset because we will loose the error message
                                    //
                                    //IISResetRequired = False
                                    errorController.addUserError(core, "This Add-on Collection did not install correctly, " + ErrorMessage);
                                } else {
                                    //
                                    // Save the first collection as the installed collection
                                    //
                                    //If InstalledCollectionGuid = "" Then
                                    //    InstalledCollectionGuid = LibGuids[Ptr]
                                    //End If
                                }
                            }
                        }
                        //
                        // --------------------------------------------------------------------------------
                        //   Install Manual Collections
                        // --------------------------------------------------------------------------------
                        //
                        if (AllowInstallFromFolder) {
                            //InstallFolder = core.asv.config.physicalFilePath & InstallFolderName & "\"
                            if (core.privateFiles.pathExists(privateFilesInstallPath)) {
                                UpgradeOK = collectionController.InstallCollectionsFromPrivateFolder(core, privateFilesInstallPath, ref ErrorMessage, ref InstalledCollectionGuidList, false, ref nonCriticalErrorList);
                                if (!UpgradeOK) {
                                    if (string.IsNullOrEmpty(ErrorMessage)) {
                                        errorController.addUserError(core, "The Add-on Collection did not install correctly, but no detailed error message was given.");
                                    } else {
                                        errorController.addUserError(core, "The Add-on Collection did not install correctly, " + ErrorMessage);
                                    }
                                } else {
                                    foreach (string installedCollectionGuid in InstalledCollectionGuidList) {
                                        CS = core.db.csOpen("Add-on Collections", GuidFieldName + "=" + core.db.encodeSQLText(installedCollectionGuid));
                                        if (core.db.csOk(CS)) {
                                            InstalledCollectionIDList.Add(core.db.csGetInteger(CS, "ID"));
                                        }
                                        core.db.csClose(ref CS);
                                    }
                                }
                            }
                        }
                        //
                        // --------------------------------------------------------------------------------
                        //   Register ActiveX files
                        // --------------------------------------------------------------------------------
                        //
                        if (!string.IsNullOrEmpty(RegisterList)) {
                            //  Call addonInstall.RegisterActiveXFiles(RegisterList)
                            //  Call addonInstall.RegisterDotNet(RegisterList)
                            RegisterList = "";
                        }
                        //
                        // --------------------------------------------------------------------------------
                        //   Forward to help page
                        // --------------------------------------------------------------------------------
                        //
                        if ((InstalledCollectionIDList.Count > 0) && (!(core.doc.debug_iUserError != ""))) {
                            return core.webServer.redirect("/" + core.serverConfig.appConfig.adminRoute + "?helpcollectionid=" + InstalledCollectionIDList[0].ToString(), "Redirecting to help page after collection installation");
                        }
                        //
                        // --------------------------------------------------------------------------------
                        // Get Form
                        // --------------------------------------------------------------------------------
                        //
                        if (true) {
                            if (true) {
                                //
                                // --------------------------------------------------------------------------------
                                // Get the Collection Library tab
                                // --------------------------------------------------------------------------------
                                //
                                ColumnCnt = 4;
                                ColCaption = new string[4];
                                ColAlign = new string[4];
                                ColWidth = new string[4];
                                ColSortable = new bool[4];
                                Cells3 = new string[1001, 5];
                                //
                                ColCaption[0] = "Install";
                                ColAlign[0] = "center";
                                ColWidth[0] = "50";
                                ColSortable[0] = false;
                                //
                                ColCaption[1] = "Name";
                                ColAlign[1] = "left";
                                ColWidth[1] = "200";
                                ColSortable[1] = false;
                                //
                                ColCaption[2] = "Last&nbsp;Updated";
                                ColAlign[2] = "right";
                                ColWidth[2] = "200";
                                ColSortable[2] = false;
                                //
                                ColCaption[3] = "Description";
                                ColAlign[3] = "left";
                                ColWidth[3] = "99%";
                                ColSortable[3] = false;
                                //
                                LocalCollections = new XmlDocument();
                                LocalCollectionXML = collectionController.getLocalCollectionStoreListXml(core);
                                LocalCollections.LoadXml(LocalCollectionXML);
                                foreach (XmlNode CDef_Node in LocalCollections.DocumentElement.ChildNodes) {
                                    if (genericController.vbLCase(CDef_Node.Name) == "collection") {
                                        foreach (XmlNode CollectionNode in CDef_Node.ChildNodes) {
                                            if (genericController.vbLCase(CollectionNode.Name) == "guid") {
                                                OnServerGuidList += "," + CollectionNode.InnerText;
                                                break;
                                            }
                                        }
                                    }
                                }
                                //
                                LibCollections = new XmlDocument();
                                bool parseError = false;
                                try {
                                    LibCollections.Load("http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion());
                                } catch (Exception) {
                                    UserError = "There was an error reading the Collection Library. The site may be unavailable.";
                                    HandleClassAppendLog("AddonManager", UserError);
                                    status += "<br>" + UserError;
                                    errorController.addUserError(core, UserError);
                                    parseError = true;
                                }
                                Ptr = 0;
                                if (!parseError) {
                                    if (genericController.vbLCase(LibCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                                        UserError = "There was an error reading the Collection Library file. The '" + CollectionListRootNode + "' element was not found.";
                                        HandleClassAppendLog("AddonManager", UserError);
                                        status += "<br>" + UserError;
                                        errorController.addUserError(core, UserError);
                                    } else {
                                        //
                                        // Go through file to validate the XML, and build status message -- since service process can not communicate to user
                                        //
                                        RowPtr = 0;
                                        foreach (XmlNode CDef_Node in LibCollections.DocumentElement.ChildNodes) {
                                            switch (genericController.vbLCase(CDef_Node.Name)) {
                                                case "collection":
                                                    //
                                                    // Read the collection
                                                    //
                                                    foreach (XmlNode CollectionNode in CDef_Node.ChildNodes) {
                                                        switch (genericController.vbLCase(CollectionNode.Name)) {
                                                            case "name":
                                                                //
                                                                // Name
                                                                //
                                                                Collectionname = CollectionNode.InnerText;
                                                                break;
                                                            case "guid":
                                                                //
                                                                // Guid
                                                                //
                                                                CollectionGuid = CollectionNode.InnerText;
                                                                break;
                                                            case "version":
                                                                //
                                                                // Version
                                                                //
                                                                CollectionVersion = CollectionNode.InnerText;
                                                                break;
                                                            case "description":
                                                                //
                                                                // Version
                                                                //
                                                                CollectionDescription = CollectionNode.InnerText;
                                                                break;
                                                            case "contensiveversion":
                                                                //
                                                                // Version
                                                                //
                                                                CollectionContensiveVersion = CollectionNode.InnerText;
                                                                break;
                                                            case "lastchangedate":
                                                                //
                                                                // Version
                                                                //
                                                                CollectionLastChangeDate = CollectionNode.InnerText;
                                                                if (dateController.IsDate(CollectionLastChangeDate)) {
                                                                    DateValue = DateTime.Parse(CollectionLastChangeDate);
                                                                    CollectionLastChangeDate = DateValue.ToShortDateString();
                                                                }
                                                                if (string.IsNullOrEmpty(CollectionLastChangeDate)) {
                                                                    CollectionLastChangeDate = "unknown";
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    bool IsOnServer = false;
                                                    bool IsOnSite = false;
                                                    if (RowPtr >= Cells3.GetUpperBound(0)) {
                                                        //todo  NOTE: The following block reproduces what 'ReDim Preserve' does behind the scenes in VB:
                                                        //ORIGINAL LINE: ReDim Preserve Cells3(RowPtr + 100, ColumnCnt)
                                                        string[,] tempVar2 = new string[RowPtr + 101, ColumnCnt + 1];
                                                        if (Cells3 != null) {
                                                            for (int Dimension0 = 0; Dimension0 < Cells3.GetLength(0); Dimension0++) {
                                                                int CopyLength = Math.Min(Cells3.GetLength(1), tempVar2.GetLength(1));
                                                                for (int Dimension1 = 0; Dimension1 < CopyLength; Dimension1++) {
                                                                    tempVar2[Dimension0, Dimension1] = Cells3[Dimension0, Dimension1];
                                                                }
                                                            }
                                                        }
                                                        Cells3 = tempVar2;
                                                    }
                                                    if (string.IsNullOrEmpty(Collectionname)) {
                                                        Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow\" VALUE=\"" + RowPtr + "\" disabled>";
                                                        Cells3[RowPtr, 1] = "no name";
                                                        Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                        Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                    } else {
                                                        if (string.IsNullOrEmpty(CollectionGuid)) {
                                                            Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow\" VALUE=\"" + RowPtr + "\" disabled>";
                                                            Cells3[RowPtr, 1] = Collectionname + " (no guid)";
                                                            Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                            Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                        } else {
                                                            IsOnServer = genericController.encodeBoolean(OnServerGuidList.IndexOf(CollectionGuid, System.StringComparison.OrdinalIgnoreCase) + 1);
                                                            CS = core.db.csOpen("Add-on Collections", GuidFieldName + "=" + core.db.encodeSQLText(CollectionGuid));
                                                            IsOnSite = core.db.csOk(CS);
                                                            core.db.csClose(ref CS);
                                                            if (IsOnSite) {
                                                                //
                                                                // Already installed
                                                                //
                                                                Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow" + RowPtr + "\" VALUE=\"1\" disabled>";
                                                                Cells3[RowPtr, 1] = Collectionname + "&nbsp;(installed already)";
                                                                Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                                Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                            } else if ((!string.IsNullOrEmpty(CollectionContensiveVersion)) && (string.CompareOrdinal(CollectionContensiveVersion, core.codeVersion()) > 0)) {
                                                                //
                                                                // wrong version
                                                                //
                                                                Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow\" VALUE=\"" + RowPtr + "\" disabled>";
                                                                Cells3[RowPtr, 1] = Collectionname + "&nbsp;(Contensive v" + CollectionContensiveVersion + " needed)";
                                                                Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                                Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                            } else if (!DbUpToDate) {
                                                                //
                                                                // Site needs to by upgraded
                                                                //
                                                                Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow\" VALUE=\"" + RowPtr + "\" disabled>";
                                                                Cells3[RowPtr, 1] = Collectionname + "&nbsp;(install disabled)";
                                                                Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                                Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                            } else {
                                                                //
                                                                // Not installed yet
                                                                //
                                                                Cells3[RowPtr, 0] = "<input TYPE=\"CheckBox\" NAME=\"LibraryRow\" VALUE=\"" + RowPtr + "\" onClick=\"clearLibraryRows('" + RowPtr + "');\">" + core.html.inputHidden("LibraryRowGuid" + RowPtr, CollectionGuid) + core.html.inputHidden("LibraryRowName" + RowPtr, Collectionname);
                                                                //Cells3(RowPtr, 0) = core.main_GetFormInputCheckBox2("LibraryRow" & RowPtr) & core.main_GetFormInputHidden("LibraryRowGuid" & RowPtr, CollectionGUID) & core.main_GetFormInputHidden("LibraryRowName" & RowPtr, CollectionName)
                                                                Cells3[RowPtr, 1] = Collectionname + "&nbsp;";
                                                                Cells3[RowPtr, 2] = CollectionLastChangeDate + "&nbsp;";
                                                                Cells3[RowPtr, 3] = CollectionDescription + "&nbsp;";
                                                            }
                                                        }
                                                    }
                                                    RowPtr = RowPtr + 1;
                                                    break;
                                            }
                                        }
                                    }
                                    BodyHTML = ""
                                    + "<input type=hidden name=LibraryCnt value=\"" + RowPtr + "\">"
                                    + "<script language=\"JavaScript\">"
                                    + "function clearLibraryRows(r) {"
                                    + "var c,p;"
                                    + "c=document.getElementsByName('LibraryRow');"
                                        + "for (p=0;p<c.length;p++){"
                                            + "if(c[p].value!=r)c[p].checked=false;"
                                        + "}"
                                    + ""
                                    + "}"
                                    + "</script>"
                                    + "<div style=\"width:100%\">" + Adminui.GetReport2(RowPtr, ColCaption, ColAlign, ColWidth, Cells3, RowPtr, 1, "", PostTableCopy, RowPtr, "ccAdmin", ColSortable, 0) + "</div>"
                                    + "";
                                    BodyHTML = Adminui.GetEditPanel(true, "Add-on Collection Library", "Select an Add-on to install from the Contensive Add-on Library. Please select only one at a time. Click OK to install the selected Add-on. The site may need to be stopped during the installation, but will be available again in approximately one minute.", BodyHTML);
                                    BodyHTML = BodyHTML + core.html.inputHidden("AOCnt", RowPtr);
                                    core.html.addLiveTabEntry("<nobr>Collection&nbsp;Library</nobr>", BodyHTML, "ccAdminTab");
                                }
                                //
                                // --------------------------------------------------------------------------------
                                // Current Collections Tab
                                // --------------------------------------------------------------------------------
                                //
                                ColumnCnt = 2;
                                ColCaption = new string[3];
                                ColAlign = new string[3];
                                ColWidth = new string[3];
                                ColSortable = new bool[3];
                                //
                                ColCaption[0] = "Del";
                                ColAlign[0] = "center";
                                ColWidth[0] = "50";
                                ColSortable[0] = false;
                                //
                                ColCaption[1] = "Name";
                                ColAlign[1] = "left";
                                ColWidth[1] = "";
                                ColSortable[1] = false;
                                //
                                DisplaySystem = false;
                                if (!core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                                    //
                                    // non-developers
                                    //
                                    CS = core.db.csOpen("Add-on Collections", "((system is null)or(system=0))", "Name");
                                } else {
                                    //
                                    // developers
                                    //
                                    DisplaySystem = true;
                                    CS = core.db.csOpen("Add-on Collections", "", "Name");
                                }
                                string[,] tempVar3 = new string[core.db.csGetRowCount(CS) + 1, ColumnCnt + 1];
                                if (Cells != null) {
                                    for (int Dimension0 = 0; Dimension0 < Cells.GetLength(0); Dimension0++) {
                                        int CopyLength = Math.Min(Cells.GetLength(1), tempVar3.GetLength(1));
                                        for (int Dimension1 = 0; Dimension1 < CopyLength; Dimension1++) {
                                            tempVar3[Dimension0, Dimension1] = Cells[Dimension0, Dimension1];
                                        }
                                    }
                                }
                                Cells = tempVar3;
                                RowPtr = 0;
                                while (core.db.csOk(CS)) {
                                    Cells[RowPtr, 0] = core.html.inputCheckbox("AC" + RowPtr) + core.html.inputHidden("ACID" + RowPtr, core.db.csGetInteger(CS, "ID"));
                                    //Cells(RowPtr, 1) = "<a href=""" & core.app.SiteProperty_AdminURL & "?id=" & core.app.cs_getInteger(CS, "ID") & "&cid=" & core.app.cs_getInteger(CS, "ContentControlID") & "&af=4""><img src=""/ccLib/images/IconContentEdit.gif"" border=0></a>"
                                    Cells[RowPtr, 1] = core.db.csGetText(CS, "name");
                                    if (DisplaySystem) {
                                        if (core.db.csGetBoolean(CS, "system")) {
                                            Cells[RowPtr, 1] = Cells[RowPtr, 1] + " (system)";
                                        }
                                    }
                                    core.db.csGoNext(CS);
                                    RowPtr = RowPtr + 1;
                                }
                                core.db.csClose(ref CS);
                                BodyHTML = "<div style=\"width:100%\">" + Adminui.GetReport2(RowPtr, ColCaption, ColAlign, ColWidth, Cells, RowPtr, 1, "", PostTableCopy, RowPtr, "ccAdmin", ColSortable, 0) + "</div>";
                                BodyHTML = Adminui.GetEditPanel(true, "Add-on Collections", "Use this form to review and delete current add-on collections.", BodyHTML);
                                BodyHTML = BodyHTML + core.html.inputHidden("accnt", RowPtr);
                                core.html.addLiveTabEntry("Installed&nbsp;Collections", BodyHTML, "ccAdminTab");
                                //
                                // --------------------------------------------------------------------------------
                                // Get the Upload Add-ons tab
                                // --------------------------------------------------------------------------------
                                //
                                Body = new stringBuilderLegacyController();
                                if (!DbUpToDate) {
                                    Body.Add("<p>Add-on upload is disabled because your site database needs to be updated.</p>");
                                } else {
                                    Body.Add(Adminui.EditTableOpen);
                                    if (core.doc.sessionContext.isAuthenticatedDeveloper(core)) {
                                        Body.Add(Adminui.GetEditRow(core.html.inputCheckbox("InstallCore"), "Reinstall Core Collection", "", false, false, ""));
                                    }
                                    Body.Add(Adminui.GetEditRow(core.html.inputFile("MetaFile"), "Add-on Collection File(s)", "", true, false, ""));
                                    FormInput = ""
                                        + "<table id=\"UploadInsert\" border=\"0\" cellpadding=\"0\" cellspacing=\"1\" width=\"100%\">"
                                        + "</table>"
                                        + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"1\" width=\"100%\">"
                                        + "<tr><td align=\"left\"><a href=\"#\" onClick=\"InsertUpload(); return false;\">+ Add more files</a></td></tr>"
                                        + "</table>"
                                        + core.html.inputHidden("UploadCount", 1, "UploadCount") + "";
                                    Body.Add(Adminui.GetEditRow(FormInput, "&nbsp;", "", true, false, ""));
                                    Body.Add(Adminui.EditTableClose);
                                }
                                core.html.addLiveTabEntry("Add&nbsp;Manually", Adminui.GetEditPanel(true, "Install or Update an Add-on Collection.", "Use this form to upload a new or updated Add-on Collection to your site. A collection file can be a single xml configuration file, a single zip file containing the configuration file and other resource files, or a configuration with other resource files uploaded separately. Use the 'Add more files' link to add as many files as you need. When you hit OK, the Collection will be checked, and only submitted if all files are uploaded.", Body.Text), "ccAdminTab");
                                //
                                // --------------------------------------------------------------------------------
                                // Build Page from tabs
                                // --------------------------------------------------------------------------------
                                //
                                Content.Add(core.html.getLiveTabs());
                                //
                                ButtonList = ButtonCancel + "," + ButtonOK;
                                Content.Add(core.html.inputHidden(RequestNameAdminSourceForm, AdminFormLegacyAddonManager));
                            }
                        }
                    }
                    //
                    // Output the Add-on
                    //
                    Caption = "Add-on Manager (Safe Mode)";
                    Description = "<div>Use the add-on manager to add and remove Add-ons from your Contensive installation.</div>";
                    if (!DbUpToDate) {
                        Description = Description + "<div style=\"Margin-left:50px\">The Add-on Manager is disabled because this site's Database needs to be upgraded.</div>";
                    }
                    if (nonCriticalErrorList.Count > 0) {
                        status += "<ul>";
                        foreach (string item in nonCriticalErrorList) {
                            status += "<li>" + item + "</li>";
                        }
                        status += "</ul>";
                    }
                    if (!string.IsNullOrEmpty(status)) {
                        Description = Description + "<div style=\"Margin-left:50px\">" + status + "</div>";
                    }
                    addonManager = Adminui.GetBody(Caption, ButtonList, "", false, false, Description, "", 0, Content.Text);
                    core.html.addTitle("Add-on Manager");
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return addonManager;
        }
        //
        //
        //
        private void GetForm_SafeModeAddonManager_DeleteNavigatorBranch(string EntryName, int EntryParentID) {
            try {
                int CS = 0;
                int EntryID = 0;
                //
                if (EntryParentID == 0) {
                    CS = core.db.csOpen(cnNavigatorEntries, "(name=" + core.db.encodeSQLText(EntryName) + ")and((parentID is null)or(parentid=0))");
                } else {
                    CS = core.db.csOpen(cnNavigatorEntries, "(name=" + core.db.encodeSQLText(EntryName) + ")and(parentID=" + core.db.encodeSQLNumber(EntryParentID) + ")");
                }
                if (core.db.csOk(CS)) {
                    EntryID = core.db.csGetInteger(CS, "ID");
                }
                core.db.csClose(ref CS);
                //
                if (EntryID != 0) {
                    CS = core.db.csOpen(cnNavigatorEntries, "(parentID=" + core.db.encodeSQLNumber(EntryID) + ")");
                    while (core.db.csOk(CS)) {
                        GetForm_SafeModeAddonManager_DeleteNavigatorBranch(core.db.csGetText(CS, "name"), EntryID);
                        core.db.csGoNext(CS);
                    }
                    core.db.csClose(ref CS);
                    core.db.deleteContentRecord(cnNavigatorEntries, EntryID);
                }
            } catch (Exception ex) {
                core.handleException(ex);
            }
        }
        //
        //========================================================================
        // ----- Get an XML nodes attribute based on its name
        //========================================================================
        //
        private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string tempGetXMLAttribute = null;
            try {
                //
                //todo  NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = genericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
                            tempGetXMLAttribute = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                    }
                } else {
                    tempGetXMLAttribute = ResultNode.Value;
                    Found = true;
                }
                if (!Found) {
                    tempGetXMLAttribute = DefaultIfNotFound;
                }
                return tempGetXMLAttribute;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            HandleClassTrapError("GetXMLAttribute");
            return tempGetXMLAttribute;
        }
        //
        //
        //
        private void HandleClassAppendLog(string MethodName, string Context) {
            logController.appendLogWithLegacyRow(core, core.serverConfig.appConfig.name, Context, "dll", "AddonManClass", MethodName, 0, "", "", false, true, core.webServer.requestUrl, "", "");

        }
        //
        //===========================================================================
        //
        //===========================================================================
        //
        private void HandleClassTrapError(string MethodName, string Context = "context unknown") {
            throw new ApplicationException("Unexpected exception in method [" + MethodName + "], cause [" + Context + "]");
        }
        //
        //
        //
        public int GetParentIDFromNameSpace(string ContentName, string menuNameSpace) {
            int tempGetParentIDFromNameSpace = 0;
            try {
                string ParentNameSpace = null;
                string ParentName = null;
                int ParentID = 0;
                int Pos = 0;
                int CS = 0;
                //
                tempGetParentIDFromNameSpace = 0;
                if (!string.IsNullOrEmpty(menuNameSpace)) {
                    //ParentName = ParentNameSpace
                    Pos = genericController.vbInstr(1, menuNameSpace, ".");
                    if (Pos == 0) {
                        ParentName = menuNameSpace;
                        ParentNameSpace = "";
                    } else {
                        ParentName = menuNameSpace.Substring(Pos);
                        ParentNameSpace = menuNameSpace.Left( Pos - 1);
                    }
                    if (string.IsNullOrEmpty(ParentNameSpace)) {
                        CS = core.db.csOpen(ContentName, "(name=" + core.db.encodeSQLText(ParentName) + ")and((parentid is null)or(parentid=0))", "ID", false, 0, false, false, "ID");
                        if (core.db.csOk(CS)) {
                            tempGetParentIDFromNameSpace = core.db.csGetInteger(CS, "ID");
                        }
                        core.db.csClose(ref CS);
                    } else {
                        ParentID = GetParentIDFromNameSpace(ContentName, ParentNameSpace);
                        CS = core.db.csOpen(ContentName, "(name=" + core.db.encodeSQLText(ParentName) + ")and(parentid=" + ParentID + ")", "ID", false, 0, false, false, "ID");
                        if (core.db.csOk(CS)) {
                            tempGetParentIDFromNameSpace = core.db.csGetInteger(CS, "ID");
                        }
                        core.db.csClose(ref CS);
                    }
                }
                //
                return tempGetParentIDFromNameSpace;
                //
                // ----- Error Trap
                //
            } catch (Exception ex) {
                core.handleException(ex);
            }
            //ErrorTrap:
            HandleClassTrapError("GetParentIDFromNameSpace");
            return tempGetParentIDFromNameSpace;
        }
    }
}
