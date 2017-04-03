
Option Explicit On
Option Strict On

Imports System.Xml

Namespace Contensive.Core
    Public Class addon_AddonMngrSafeClass
        '
        ' constructor sets cp from argument for use in calls to other objects, then cpCore because cp cannot be uses since that would be a circular depenancy
        '
        Private cpCore As coreClass
        '
        ' To interigate Add-on Collections to check for re-use
        '
        Private Structure DeleteType
            Dim Name As String
            Dim ParentID As Integer
        End Structure
        Private Structure NavigatorType
            Dim Name As String
            Dim menuNameSpace As String
        End Structure
        Private Structure Collection2Type
            Dim AddOnCnt As Integer
            Dim AddonGuid() As String
            Dim AddonName() As String
            Dim MenuCnt As Integer
            Dim Menus() As String
            Dim NavigatorCnt As Integer
            Dim Navigators() As NavigatorType
        End Structure
        Private CollectionCnt As Integer
        Private Collections() As Collection2Type
        '
        '==================================================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '==========================================================================================================================================
        '   Addon Manager
        '       This is a form that lets you upload an addon
        '       Eventually, this should be substituted with a "Addon Manager Addon" - so the interface can be improved with Contensive recompile
        '==========================================================================================================================================
        '
        public Function GetForm_SafeModeAddonManager() As String
            Dim addonManager As String = ""
            Try
                Dim LocalCollectionXML As String
                Dim DisplaySystem As Boolean
                Dim DbUpToDate As Boolean
                Dim XMLTools As New coreXmlToolsClass(cpCore)
                Dim GuidFieldName As String
                Dim InstalledCollectionIDList As New List(Of Integer)
                Dim InstalledCollectionGuidList As New List(Of String)
                Dim DateValue As Date
                Dim ErrorMessage As String = ""
                Dim OnServerGuidList As String = ""
                Dim UpgradeOK As Boolean
                Dim RegisterList As String = ""
                Dim LocalCollections As XmlDocument
                Dim LibCollections As XmlDocument
                Dim InstallFolder As String
                Dim LibGuids() As String
                Dim IISResetRequired As Boolean
                Dim AddonNavigatorID As Integer
                Dim TargetCollectionName As String
                Dim Collectionname As String = ""
                Dim CollectionGuid As String = ""
                Dim CollectionVersion As String
                Dim CollectionDescription As String = ""
                Dim CollectionContensiveVersion As String = ""
                Dim CollectionLastChangeDate As String = ""
                Dim Cells3 As String(,)
                Dim FormInput As String
                Dim Cnt As Integer
                Dim Ptr As Integer
                Dim UploadTab As New coreFastStringClass
                Dim ModifyTab As New coreFastStringClass
                Dim RowPtr As Integer
                Dim Body As coreFastStringClass
                Dim Cells As String(,)
                Dim PageNumber As Integer
                Dim ColumnCnt As Integer
                Dim ColCaption() As String
                Dim ColAlign() As String
                Dim ColWidth() As String
                Dim ColSortable() As Boolean
                Dim PreTableCopy As String
                Dim PostTableCopy As String = ""
                Dim BodyHTML As String
                Dim CS As Integer
                Dim UserError As String
                Dim Content As New coreFastStringClass
                Dim Button As String
                Dim Caption As String
                Dim Description As String
                Dim ButtonList As String = ""
                Dim CollectionFilename As String
                Dim CollectionFilePathPage As String
                Dim CDef_Node As XmlNode
                'Dim Doc As New XmlDocument
                Dim CollectionNode As XmlNode
                Dim status As String = ""
                Dim AllowInstallFromFolder As Boolean
                Dim InstallLibCollectionList As String = ""
                Dim TargetCollectionID As Integer
                Dim privateFilesInstallPath As String
                Dim Adminui As New coreAdminUIClass(cpCore)
                Dim addonInstall As New coreAddonInstallClass(cpCore)
                Dim builder As New coreBuilderClass(cpCore)
                '
                ' BuildVersion = cpcore.app.dataBuildVersion
                Dim dataBuildVersion As String = cpCore.siteProperties.dataBuildVersion
                Dim coreVersion As String = cpCore.common_version()

                DbUpToDate = (dataBuildVersion = coreVersion)
                '
                Button = cpCore.docProperties.getText(RequestNameButton)
                AllowInstallFromFolder = False
                If True Then
                    GuidFieldName = "ccguid"
                Else
                    GuidFieldName = "aoguid"
                End If
                If Button = ButtonCancel Then
                    '
                    ' ----- redirect back to the root
                    '
                    Call cpCore.webServerIO_Redirect2(cpCore.siteProperties.adminURL, "Addon Manager, Cancel Button Pressed", False)
                Else
                    If Not cpCore.user.isAuthenticatedAdmin Then
                        '
                        ' ----- Put up error message
                        '
                        ButtonList = ButtonCancel
                        Content.Add(Adminui.GetFormBodyAdminOnly())
                    Else
                        '
                        PreTableCopy = "Use this form to upload an add-on collection. If the GUID of the add-on matches one already installed on this server, it will be updated. If the GUID is new, it will be added."
                        InstallFolder = "temp\CollectionUpload" & CStr(GetRandomInteger())
                        privateFilesInstallPath = InstallFolder & "\"
                        If (Button = ButtonOK) Then
                            '
                            '---------------------------------------------------------------------------------------------
                            ' Download and install Collections from the Collection Library
                            '---------------------------------------------------------------------------------------------
                            '
                            If cpCore.docProperties.getText("LibraryRow") <> "" Then
                                Ptr = cpCore.docProperties.getInteger("LibraryRow")
                                'If cpcore.main_GetStreamBoolean2("LibraryRow" & Ptr) Then
                                CollectionGuid = cpCore.docProperties.getText("LibraryRowguid" & Ptr)
                                InstallLibCollectionList = InstallLibCollectionList & "," & CollectionGuid
                            End If

                            '                Cnt = cpcore.main_GetStreamInteger2("LibraryCnt")
                            '                If Cnt > 0 Then
                            '                    For Ptr = 0 To Cnt - 1
                            '                        If cpcore.main_GetStreamText2("LibraryRow") <> "" Then
                            '                        'If cpcore.main_GetStreamBoolean2("LibraryRow" & Ptr) Then
                            '                            CollectionGUID = cpcore.main_GetStreamText2("LibraryRowguid" & Ptr)
                            '                            InstallLibCollectionList = InstallLibCollectionList & "," & CollectionGUID
                            '                        End If
                            '                    Next
                            '                End If
                            '
                            '---------------------------------------------------------------------------------------------
                            ' Delete collections
                            '   Before deleting each addon, make sure it is not in another collection
                            '---------------------------------------------------------------------------------------------
                            '
                            Cnt = cpCore.docProperties.getInteger("accnt")
                            If Cnt > 0 Then
                                For Ptr = 0 To Cnt - 1
                                    If cpCore.doc_getBoolean2("ac" & Ptr) Then
                                        TargetCollectionID = cpCore.doc_getInteger("acID" & Ptr)
                                        TargetCollectionName = cpCore.content_GetRecordName("Add-on Collections", TargetCollectionID)
                                        '
                                        ' Delete any addons from this collection
                                        '
                                        Call cpCore.db.deleteContentRecords("add-ons", "collectionid=" & TargetCollectionID)

                                        '                            '
                                        '                            ' Load all collections into local collection storage
                                        '                            '
                                        '                            TargetCollectionID = cpcore.main_GetStreamInteger2("acID" & Ptr)
                                        '                            CS = cpcore.app.csOpen("Add-on Collections")
                                        '                            CollectionCnt = 0
                                        '                            TargetCollectionPtr = -1
                                        '                            Do While cpcore.asv.csv_IsCSOK(CS)
                                        '                                ReDim Preserve Collections(CollectionCnt)
                                        '                                CollectionID = cpcore.app.cs_getInteger(CS, "ID")
                                        '                                CollectionName = cpcore.db.cs_getText(CS, "Name")
                                        '                                If CollectionID = TargetCollectionID Then
                                        '                                    TargetCollectionPtr = CollectionCnt
                                        '                                    'TargetCollectionPtr = Ptr
                                        '                                    TargetCollectionName = CollectionName
                                        '                                End If
                                        '                                '
                                        '                                ' Get collection addons
                                        '                                '
                                        '                                If true Then
                                        '                                    SQL = "select A.ID,A.name,A." & GuidFieldName _
                                        '                                        & " from ccAggregateFunctions A" _
                                        '                                        & " where a.CollectionID=" & CollectionID
                                        '                                Else
                                        '                                    SQL = "select A.ID,A.name,A." & GuidFieldName _
                                        '                                        & " from ccAggregateFunctions A" _
                                        '                                        & " left join ccAddonCollectionRules R on R.AddonID=A.ID" _
                                        '                                        & " where R.CollectionID=" & CollectionID
                                        '                                End If
                                        '                                CSAddons = cpcore.app.openCsSql_rev("default", SQL)
                                        '                                Do While cpcore.asv.csv_IsCSOK(CSAddons)
                                        '                                    AddonCnt = Collections(CollectionCnt).AddonCnt
                                        '                                    ReDim Preserve Collections(CollectionCnt).AddonName(AddonCnt)
                                        '                                    ReDim Preserve Collections(CollectionCnt).AddonGuid(AddonCnt)
                                        '                                    addonid = cpcore.app.cs_getInteger(CSAddons, "ID")
                                        '                                    Collections(CollectionCnt).AddonGuid(AddonCnt) = cpcore.db.cs_getText(CSAddons, GuidFieldName)
                                        '                                    Collections(CollectionCnt).AddonName(AddonCnt) = cpcore.db.cs_getText(CSAddons, "Name")
                                        '                                    Collections(CollectionCnt).AddonCnt = AddonCnt + 1
                                        '                                    Call cpcore.app.nextCSRecord(CSAddons)
                                        '                                Loop
                                        '                                Call cpcore.app.closeCS(CSAddons)
                                        '                                '
                                        '                                ' GetCDefs from Collection File to remove cdefs and navigators
                                        '                                '
                                        '                                CollectionFile = cpcore.app.cs_get(CS, "InstallFile")
                                        '                                If CollectionFile <> "" Then
                                        '                                    Call Doc.loadXML(CollectionFile)
                                        '                                    If Doc.parseError.ErrorCode <> 0 Then
                                        '                                        '
                                        '                                        ' ********************** add status message here
                                        '                                        '
                                        '                                    Else
                                        '                                        With Doc.documentElement
                                        '                                            If vbLCase(.baseName) = "collection" Then
                                        '                                                CollectionName = GetXMLAttribute(IsFound, Doc.documentElement, "name", "")
                                        '                                                CollectionSystem = encodeBoolean(GetXMLAttribute(IsFound, Doc.documentElement, "system", ""))
                                        '                                                For Each CDef_Node In .childNodes
                                        '                                                    Select Case vbLCase(CDef_Node.name)
                                        '                                                        Case "interfaces"
                                        '                                                            For Each InterfaceNode In CDef_Node.childNodes
                                        '                                                                Select Case vbLCase(InterfaceNode.name)
                                        '                                                                    Case "page"
                                        '                                                                        For Each PageNode In InterfaceNode.childNodes
                                        '                                                                            If vbLCase(PageNode.name) = "navigator" Then
                                        '                                                                                NavigatorCnt = Collections(CollectionCnt).NavigatorCnt
                                        '                                                                                ReDim Preserve Collections(CollectionCnt).Navigators(NavigatorCnt)
                                        '                                                                                Collections(CollectionCnt).Navigators(NavigatorCnt).name = GetXMLAttribute(IsFound, PageNode, "name", "")
                                        '                                                                                Collections(CollectionCnt).Navigators(NavigatorCnt).NameSpace = GetXMLAttribute(IsFound, PageNode, "NameSpace", "")
                                        '                                                                                Collections(CollectionCnt).NavigatorCnt = NavigatorCnt + 1
                                        '                                                                            End If
                                        '                                                                        Next
                                        '                                                                    Case "setting"
                                        '                                                                End Select
                                        '                                                            Next
                                        '                                                        Case "contensivecdef"
                                        '                                                            '
                                        '                                                            ' load menu entries
                                        '                                                            '
                                        '                                                            For Each CDefNode In CDef_Node.childNodes
                                        '                                                                If vbLCase(CDefNode.name) = "adminmenu" Then
                                        '                                                                    MenuCnt = Collections(CollectionCnt).MenuCnt
                                        '                                                                    ReDim Preserve Collections(CollectionCnt).Menus(MenuCnt)
                                        '                                                                    Collections(CollectionCnt).Menus(MenuCnt) = GetXMLAttribute(IsFound, CDefNode, "name", "")
                                        '                                                                    Collections(CollectionCnt).MenuCnt = MenuCnt + 1
                                        '                                                                End If
                                        '                                                                If vbLCase(CDefNode.name) = "navigatorentry" Then
                                        '                                                                    NavigatorCnt = Collections(CollectionCnt).NavigatorCnt
                                        '                                                                    ReDim Preserve Collections(CollectionCnt).Navigators(NavigatorCnt)
                                        '                                                                    Collections(CollectionCnt).Navigators(NavigatorCnt).name = GetXMLAttribute(IsFound, CDefNode, "name", "")
                                        '                                                                    Collections(CollectionCnt).Navigators(NavigatorCnt).NameSpace = GetXMLAttribute(IsFound, CDefNode, "NameSpace", "")
                                        '                                                                    Collections(CollectionCnt).NavigatorCnt = NavigatorCnt + 1
                                        '                                                                End If
                                        '                                                            Next
                                        '                                                    End Select
                                        '                                                Next
                                        '                                            End If
                                        '                                        End With
                                        '                                    End If
                                        '                                End If
                                        '                                CollectionCnt = CollectionCnt + 1
                                        '                                cpcore.main_NextCSRecord (CS)
                                        '                            Loop
                                        '                            Call cpcore.app.closeCS(CS)
                                        '                            '
                                        '                            ' Search through the local collection storage for the addons in the one we want to delete
                                        '                            '   if not in any other collections, delete the addon from the system
                                        '                            '
                                        '                            If true Then
                                        '                                '
                                        '                                ' delete all addons associated to this collection
                                        '                                '
                                        '                                Call cpcore.app.DeleteContentRecords("Add-ons", "collectionid=" & TargetCollectionID)
                                        '                            Else
                                        '                                ' deprecated the addoncollectionrules for collectionid in addon
                                        '                                If (TargetCollectionPtr >= 0) And (CollectionCnt <> 0) Then
                                        '                                    TargetAddonCnt = Collections(TargetCollectionPtr).AddonCnt
                                        '                                    For TargetAddonPtr = 0 To TargetAddonCnt - 1
                                        '                                        TargetAddonName = Collections(TargetCollectionPtr).AddonName(TargetAddonPtr)
                                        '                                        TargetAddonGUID = Collections(TargetCollectionPtr).AddonGuid(TargetAddonPtr)
                                        '                                        UseGUID = (TargetAddonGUID <> "")
                                        '                                        KeepTarget = False
                                        '                                        For SearchCPtr = 0 To CollectionCnt - 1
                                        '                                            If SearchCPtr <> TargetCollectionPtr Then
                                        '                                                With Collections(SearchCPtr)
                                        '                                                    For SearchAPtr = 0 To .AddonCnt - 1
                                        '                                                        If UseGUID Then
                                        '                                                            If TargetAddonGUID = .AddonGuid(SearchAPtr) Then
                                        '                                                                KeepTarget = True
                                        '                                                                Exit For
                                        '                                                            End If
                                        '                                                        Else
                                        '                                                            If TargetAddonName = .AddonName(SearchAPtr) Then
                                        '                                                                KeepTarget = True
                                        '                                                                Exit For
                                        '                                                            End If
                                        '                                                        End If
                                        '                                                    Next
                                        '                                                End With
                                        '                                            End If
                                        '                                        Next
                                        '                                        If Not KeepTarget Then
                                        '                                            '
                                        '                                            ' OK to delete the target addon
                                        '                                            '
                                        '                                            If UseGUID Then
                                        '                                                Criteria = "(" & GuidFieldName & "=" & encodeSQLText(TargetAddonGUID) & ")"
                                        '                                            Else
                                        '                                                Criteria = "(name=" & encodeSQLText(TargetAddonName) & ")"
                                        '                                            End If
                                        '                                            Call cpcore.app.DeleteContentRecords("Add-ons", Criteria)
                                        '                                        End If
                                        '                                    Next
                                        '                                End If
                                        '                                '
                                        '                                ' Delete Menu Entries not used by other Collections
                                        '                                '
                                        '                                TargetCnt = Collections(TargetCollectionPtr).MenuCnt
                                        '                                For TargetPtr = 0 To TargetCnt - 1
                                        '                                    TargetName = Collections(TargetCollectionPtr).Menus(TargetPtr)
                                        '                                    KeepTarget = False
                                        '                                    For SearchCPtr = 0 To CollectionCnt - 1
                                        '                                        If SearchCPtr <> TargetCollectionPtr Then
                                        '                                            With Collections(SearchCPtr)
                                        '                                                For SearchMPtr = 0 To .MenuCnt - 1
                                        '                                                    If TargetName = .Menus(SearchMPtr) Then
                                        '                                                        KeepTarget = True
                                        '                                                        Exit For
                                        '                                                    End If
                                        '                                                Next
                                        '                                            End With
                                        '                                        End If
                                        '                                    Next
                                        '                                    If Not KeepTarget Then
                                        '                                        '
                                        '                                        ' OK to delete the target addon
                                        '                                        '
                                        '                                        Call cpcore.app.DeleteContentRecords("Menu Entries", "(name=" & encodeSQLText(TargetName) & ")")
                                        '                                    End If
                                        '                                Next
                                        '                                '
                                        '                                ' Delete Navigator Entries not used by other Collections
                                        '                                '
                                        '                                TargetCnt = Collections(TargetCollectionPtr).NavigatorCnt
                                        '                                For TargetPtr = 0 To TargetCnt - 1
                                        '                                    KeepTarget = False
                                        '                                    TargetName = Collections(TargetCollectionPtr).Navigators(TargetPtr).name
                                        '                                    TargetNameSpace = Collections(TargetCollectionPtr).Navigators(TargetPtr).NameSpace
                                        '                                    If TargetNameSpace = "" Then
                                        '                                        '
                                        '                                        ' Can not delete root nodes
                                        '                                        '
                                        '                                        KeepTarget = True
                                        '                                    Else
                                        '                                        For SearchCPtr = 0 To CollectionCnt - 1
                                        '                                            If SearchCPtr <> TargetCollectionPtr Then
                                        '                                                With Collections(SearchCPtr)
                                        '                                                    For SearchMPtr = 0 To .NavigatorCnt - 1
                                        '                                                        If (TargetName = .Navigators(SearchMPtr).name) And (TargetNameSpace = .Navigators(SearchMPtr).NameSpace) Then
                                        '                                                            KeepTarget = True
                                        '                                                            Exit For
                                        '                                                        End If
                                        '                                                    Next
                                        '                                                End With
                                        '                                            End If
                                        '                                        Next
                                        '                                    End If
                                        '                                    If Not KeepTarget Then
                                        '                                        '
                                        '                                        ' OK to delete the target addon
                                        '                                        '
                                        '                                        ParentID = GetParentIDFromNameSpace("Navigator Entries", TargetNameSpace)
                                        '                                        ReDim Preserve Deletes(DeleteCnt)
                                        '                                        Deletes(DeleteCnt).name = TargetName
                                        '                                        Deletes(DeleteCnt).ParentID = ParentID
                                        '                                        DeleteCnt = DeleteCnt + 1
                                        '                                    End If
                                        '                                Next
                                        '                            End If
                                        '                            '
                                        '                            ' Delete any navigator entries found
                                        '                            '
                                        '                            If DeleteCnt > 0 Then
                                        '                                For DeletePtr = 0 To DeleteCnt - 1
                                        '                                    Call GetForm_SafeModeAddonManager_DeleteNavigatorBranch(Deletes(DeletePtr).name, Deletes(DeletePtr).ParentID)
                                        '                                Next
                                        '                            End If
                                        '
                                        ' Delete the navigator entry for the collection under 'Add-ons'
                                        '
                                        If TargetCollectionID > 0 Then
                                            AddonNavigatorID = 0
                                            CS = cpCore.db.cs_open("Navigator Entries", "name='Manage Add-ons' and ((parentid=0)or(parentid is null))")
                                            If cpCore.db.cs_ok(CS) Then
                                                AddonNavigatorID = cpCore.db.cs_getInteger(CS, "ID")
                                            End If
                                            Call cpCore.db.cs_Close(CS)
                                            If AddonNavigatorID > 0 Then
                                                Call GetForm_SafeModeAddonManager_DeleteNavigatorBranch(TargetCollectionName, AddonNavigatorID)
                                            End If
                                            '
                                            ' Now delete the Collection record
                                            '
                                            Call cpCore.DeleteContentRecord("Add-on Collections", TargetCollectionID)
                                            '
                                            ' Delete Navigator Entries set as installed by the collection (this may be all that is needed)
                                            '
                                            Call cpCore.db.deleteContentRecords("Navigator Entries", "installedbycollectionid=" & TargetCollectionID)
                                        End If
                                    End If
                                Next
                            End If
                            '
                            '---------------------------------------------------------------------------------------------
                            ' Delete Add-ons
                            '---------------------------------------------------------------------------------------------
                            '
                            Cnt = cpCore.docProperties.getInteger("aocnt")
                            If Cnt > 0 Then
                                For Ptr = 0 To Cnt - 1
                                    If cpCore.doc_getBoolean2("ao" & Ptr) Then
                                        Call cpCore.DeleteContentRecord("Add-ons", cpCore.docProperties.getInteger("aoID" & Ptr))
                                    End If
                                Next
                            End If
                            '
                            '---------------------------------------------------------------------------------------------
                            ' Reinstall core collection
                            '---------------------------------------------------------------------------------------------
                            '
                            If cpCore.user.isAuthenticatedDeveloper() And cpCore.doc_getBoolean("InstallCore") Then
                                UpgradeOK = addonInstall.installCollectionFromRemoteRepo("{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}", ErrorMessage, "", False)
                            End If
                            '
                            '---------------------------------------------------------------------------------------------
                            ' Upload new collection files
                            '---------------------------------------------------------------------------------------------
                            '
                            Dim uploadedCollectionPathFilenames As New List(Of String)
                            CollectionFilename = ""
                            If (cpCore.privateFiles.saveUpload("MetaFile", InstallFolder, CollectionFilename)) Then
                                status &= "<br>Uploaded collection file [" & CollectionFilename & "]"
                                uploadedCollectionPathFilenames.Add(InstallFolder & CollectionFilename)
                                AllowInstallFromFolder = True
                            End If
                            '
                            For Ptr = 0 To cpCore.docProperties.getInteger("UploadCount") - 1
                                If (cpCore.privateFiles.saveUpload("Upload" & Ptr, InstallFolder, CollectionFilename)) Then
                                    status &= "<br>Uploaded collection file [" & CollectionFilename & "]"
                                    uploadedCollectionPathFilenames.Add(InstallFolder & CollectionFilename)
                                    AllowInstallFromFolder = True
                                End If
                            Next
                        End If
                        ''
                        '' --------------------------------------------------------------------------------
                        ''   Install Library Collections
                        '' --------------------------------------------------------------------------------
                        ''
                        'If InstallLibCollectionList <> "" Then
                        '    InstallLibCollectionList = Mid(InstallLibCollectionList, 2)
                        '    LibGuids = Split(InstallLibCollectionList, ",")
                        '    Cnt = UBound(LibGuids) + 1
                        '    For Ptr = 0 To Cnt - 1
                        '        RegisterList = ""
                        '        UpgradeOK = addonInstall.installCollectionFromRemoteRepo(LibGuids(Ptr), ErrorMessage, "", False)
                        '        If Not UpgradeOK Then
                        '            '
                        '            ' block the reset because we will loose the error message
                        '            '
                        '            IISResetRequired = False
                        '            cpCore.error_AddUserError("This Add-on Collection did not install correctly, " & ErrorMessage)
                        '        Else
                        '            '
                        '            ' Save the first collection as the installed collection
                        '            '
                        '            If InstalledCollectionGuid = "" Then
                        '                InstalledCollectionGuid = LibGuids(Ptr)
                        '            End If
                        '        End If
                        '    Next
                        'End If
                        '
                        ' --------------------------------------------------------------------------------
                        '   Install Manual Collections
                        ' --------------------------------------------------------------------------------
                        '
                        If AllowInstallFromFolder Then
                            'InstallFolder = cpcore.asv.config.physicalFilePath & InstallFolderName & "\"
                            If cpCore.privateFiles.pathExists(privateFilesInstallPath) Then
                                UpgradeOK = addonInstall.InstallCollectionsFromPrivateFolder(privateFilesInstallPath, ErrorMessage, InstalledCollectionGuidList, False)
                                If Not UpgradeOK Then
                                    If ErrorMessage = "" Then
                                        cpCore.error_AddUserError("The Add-on Collection did not install correctly, but no detailed error message was given.")
                                    Else
                                        cpCore.error_AddUserError("The Add-on Collection did not install correctly, " & ErrorMessage)
                                    End If
                                Else
                                    For Each installedCollectionGuid As String In InstalledCollectionGuidList
                                        CS = cpCore.db.cs_open("Add-on Collections", GuidFieldName & "=" & cpCore.db.encodeSQLText(installedCollectionGuid))
                                        If cpCore.db.cs_ok(CS) Then
                                            InstalledCollectionIDList.Add(cpCore.db.cs_getInteger(CS, "ID"))
                                        End If
                                        Call cpCore.db.cs_Close(CS)
                                    Next
                                End If
                            End If
                        End If
                        '
                        ' --------------------------------------------------------------------------------
                        '   Register ActiveX files
                        ' --------------------------------------------------------------------------------
                        '
                        If RegisterList <> "" Then
                            '  Call addonInstall.RegisterActiveXFiles(RegisterList)
                            '  Call addonInstall.RegisterDotNet(RegisterList)
                            RegisterList = ""
                        End If
                        '
                        ' --------------------------------------------------------------------------------
                        '   Forward to help page
                        ' --------------------------------------------------------------------------------
                        '
                        If (InstalledCollectionIDList.Count > 0) And (Not cpCore.error_IsUserError) Then
                            Call cpCore.webServerIO_Redirect2(cpCore.siteProperties.adminURL & "?helpcollectionid=" & InstalledCollectionIDList(0).ToString(), "Redirecting to help page after collection installation", False)
                        End If
                        '
                        ' --------------------------------------------------------------------------------
                        ' Get Form
                        ' --------------------------------------------------------------------------------
                        '
                        If True Then
                                If True Then
                                    '
                                    ' --------------------------------------------------------------------------------
                                    ' Get the Collection Library tab
                                    ' --------------------------------------------------------------------------------
                                    '
                                    ColumnCnt = 4
                                    PageNumber = 1
                                    ReDim ColCaption(3)
                                    ReDim ColAlign(3)
                                    ReDim ColWidth(3)
                                    ReDim ColSortable(3)
                                    ReDim Cells3(1000, 4)
                                    '
                                    ColCaption(0) = "Install"
                                    ColAlign(0) = "center"
                                    ColWidth(0) = "50"
                                    ColSortable(0) = False
                                    '
                                    ColCaption(1) = "Name"
                                    ColAlign(1) = "left"
                                    ColWidth(1) = "200"
                                    ColSortable(1) = False
                                    '
                                    ColCaption(2) = "Last&nbsp;Updated"
                                    ColAlign(2) = "right"
                                    ColWidth(2) = "200"
                                    ColSortable(2) = False
                                    '
                                    ColCaption(3) = "Description"
                                    ColAlign(3) = "left"
                                    ColWidth(3) = "99%"
                                    ColSortable(3) = False
                                    '
                                    LocalCollections = New XmlDocument
                                    LocalCollectionXML = addonInstall.getCollectionListFile()
                                    Call LocalCollections.LoadXml(LocalCollectionXML)
                                    For Each CDef_Node In LocalCollections.DocumentElement.ChildNodes
                                        If vbLCase(CDef_Node.Name) = "collection" Then
                                            For Each CollectionNode In CDef_Node.ChildNodes
                                                If vbLCase(CollectionNode.Name) = "guid" Then
                                                    OnServerGuidList &= "," & CollectionNode.InnerText
                                                    Exit For
                                                End If
                                            Next
                                        End If
                                    Next
                                    '
                                    LibCollections = New XmlDocument
                                    Dim parseError As Boolean = False
                                    Try
                                        LibCollections.Load("http://support.contensive.com/GetCollectionList?iv=" & cpCore.common_version())
                                    Catch ex As Exception
                                        UserError = "There was an error reading the Collection Library. The site may be unavailable."
                                        Call HandleClassAppendLog("AddonManager", UserError)
                                        status &= "<br>" & UserError
                                        cpCore.error_AddUserError(UserError)
                                        parseError = True
                                    End Try
                                    Ptr = 0
                                    If Not parseError Then
                                        If vbLCase(LibCollections.DocumentElement.Name) <> vbLCase(CollectionListRootNode) Then
                                            UserError = "There was an error reading the Collection Library file. The '" & CollectionListRootNode & "' element was not found."
                                            Call HandleClassAppendLog("AddonManager", UserError)
                                            status &= "<br>" & UserError
                                            cpCore.error_AddUserError(UserError)
                                        Else
                                            '
                                            ' Go through file to validate the XML, and build status message -- since service process can not communicate to user
                                            '
                                            RowPtr = 0
                                            For Each CDef_Node In LibCollections.DocumentElement.ChildNodes
                                                Select Case vbLCase(CDef_Node.Name)
                                                    Case "collection"
                                                        '
                                                        ' Read the collection
                                                        '
                                                        For Each CollectionNode In CDef_Node.ChildNodes
                                                            Select Case vbLCase(CollectionNode.Name)
                                                                Case "name"
                                                                    '
                                                                    ' Name
                                                                    '
                                                                    Collectionname = CollectionNode.InnerText
                                                                Case "guid"
                                                                    '
                                                                    ' Guid
                                                                    '
                                                                    CollectionGuid = CollectionNode.InnerText
                                                                Case "version"
                                                                    '
                                                                    ' Version
                                                                    '
                                                                    CollectionVersion = CollectionNode.InnerText
                                                                Case "description"
                                                                    '
                                                                    ' Version
                                                                    '
                                                                    CollectionDescription = CollectionNode.InnerText
                                                                Case "contensiveversion"
                                                                    '
                                                                    ' Version
                                                                    '
                                                                    CollectionContensiveVersion = CollectionNode.InnerText
                                                                Case "lastchangedate"
                                                                    '
                                                                    ' Version
                                                                    '
                                                                    CollectionLastChangeDate = CollectionNode.InnerText
                                                                    If IsDate(CollectionLastChangeDate) Then
                                                                        DateValue = CDate(CollectionLastChangeDate)
                                                                        CollectionLastChangeDate = DateValue.ToShortDateString
                                                                    End If
                                                                    If CollectionLastChangeDate = "" Then
                                                                        CollectionLastChangeDate = "unknown"
                                                                    End If
                                                            End Select
                                                        Next
                                                        Dim IsOnServer As Boolean
                                                        Dim IsOnSite As Boolean
                                                        If RowPtr >= UBound(Cells3, 1) Then
                                                            ReDim Preserve Cells3(RowPtr + 100, ColumnCnt)
                                                        End If
                                                        If Collectionname = "" Then
                                                            Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow"" VALUE=""" & RowPtr & """ disabled>"
                                                            Cells3(RowPtr, 1) = "no name"
                                                            Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                            Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                        Else
                                                            If CollectionGuid = "" Then
                                                                Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow"" VALUE=""" & RowPtr & """ disabled>"
                                                                Cells3(RowPtr, 1) = Collectionname & " (no guid)"
                                                                Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                                Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                            Else
                                                                IsOnServer = EncodeBoolean(InStr(1, OnServerGuidList, CollectionGuid, vbTextCompare))
                                                                CS = cpCore.db.cs_open("Add-on Collections", GuidFieldName & "=" & cpCore.db.encodeSQLText(CollectionGuid), , , , , , "ID")
                                                                IsOnSite = cpCore.db.cs_ok(CS)
                                                                Call cpCore.db.cs_Close(CS)
                                                                If IsOnSite Then
                                                                    '
                                                                    ' Already installed
                                                                    '
                                                                    Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow" & RowPtr & """ VALUE=""1"" disabled>"
                                                                    Cells3(RowPtr, 1) = Collectionname & "&nbsp;(installed already)"
                                                                    Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                                    Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                                ElseIf ((CollectionContensiveVersion <> "") And (CollectionContensiveVersion > cpCore.common_version())) Then
                                                                    '
                                                                    ' wrong version
                                                                    '
                                                                    Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow"" VALUE=""" & RowPtr & """ disabled>"
                                                                    Cells3(RowPtr, 1) = Collectionname & "&nbsp;(Contensive v" & CollectionContensiveVersion & " needed)"
                                                                    Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                                    Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                                ElseIf Not DbUpToDate Then
                                                                    '
                                                                    ' Site needs to by upgraded
                                                                    '
                                                                    Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow"" VALUE=""" & RowPtr & """ disabled>"
                                                                    Cells3(RowPtr, 1) = Collectionname & "&nbsp;(install disabled)"
                                                                    Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                                    Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                                Else
                                                                    '
                                                                    ' Not installed yet
                                                                    '
                                                                    Cells3(RowPtr, 0) = "<input TYPE=""CheckBox"" NAME=""LibraryRow"" VALUE=""" & RowPtr & """ onClick=""clearLibraryRows('" & RowPtr & "');"">" & cpCore.html_GetFormInputHidden("LibraryRowGuid" & RowPtr, CollectionGuid) & cpCore.html_GetFormInputHidden("LibraryRowName" & RowPtr, Collectionname)
                                                                    'Cells3(RowPtr, 0) = cpcore.main_GetFormInputCheckBox2("LibraryRow" & RowPtr) & cpcore.main_GetFormInputHidden("LibraryRowGuid" & RowPtr, CollectionGUID) & cpcore.main_GetFormInputHidden("LibraryRowName" & RowPtr, CollectionName)
                                                                    Cells3(RowPtr, 1) = Collectionname & "&nbsp;"
                                                                    Cells3(RowPtr, 2) = CollectionLastChangeDate & "&nbsp;"
                                                                    Cells3(RowPtr, 3) = CollectionDescription & "&nbsp;"
                                                                End If
                                                            End If
                                                        End If
                                                        RowPtr = RowPtr + 1
                                                End Select
                                            Next
                                        End If
                                        BodyHTML = "" _
                                        & "<input type=hidden name=LibraryCnt value=""" & RowPtr & """>" _
                                        & "<script language=""JavaScript"">" _
                                        & "function clearLibraryRows(r) {" _
                                        & "var c,p;" _
                                        & "c=document.getElementsByName('LibraryRow');" _
                                            & "for (p=0;p<c.length;p++){" _
                                                & "if(c[p].value!=r)c[p].checked=false;" _
                                            & "}" _
                                        & "" _
                                        & "}" _
                                        & "</script>" _
                                        & "<div style=""width:100%"">" & Adminui.GetReport2(RowPtr, ColCaption, ColAlign, ColWidth, Cells3, RowPtr, 1, "", PostTableCopy, RowPtr, "ccAdmin", ColSortable, 0) & "</div>" _
                                        & ""
                                        BodyHTML = Adminui.GetEditPanel(True, "Add-on Collection Library", "Select an Add-on to install from the Contensive Add-on Library. Please select only one at a time. Click OK to install the selected Add-on. The site may need to be stopped during the installation, but will be available again in approximately one minute.", BodyHTML)
                                        BodyHTML = BodyHTML & cpCore.html_GetFormInputHidden("AOCnt", RowPtr)
                                        Call cpCore.main_AddLiveTabEntry("<nobr>Collection&nbsp;Library</nobr>", BodyHTML, "ccAdminTab")
                                    End If
                                    '
                                    ' --------------------------------------------------------------------------------
                                    ' Current Collections Tab
                                    ' --------------------------------------------------------------------------------
                                    '
                                    ColumnCnt = 2
                                    'ColumnCnt = 3
                                    PageNumber = 1
                                    ReDim ColCaption(2)
                                    ReDim ColAlign(2)
                                    ReDim ColWidth(2)
                                    ReDim ColSortable(2)
                                    'ReDim ColCaption(3)
                                    'ReDim ColAlign(3)
                                    'ReDim ColWidth(3)
                                    'ReDim ColSortable(3)
                                    '
                                    ColCaption(0) = "Del"
                                    ColAlign(0) = "center"
                                    ColWidth(0) = "50"
                                    ColSortable(0) = False
                                    ''
                                    'ColCaption(1) = "Edit"
                                    'ColAlign(1) = "center"
                                    'ColWidth(1) = "50"
                                    'ColSortable(1) = False
                                    '
                                    ColCaption(1) = "Name"
                                    ColAlign(1) = "left"
                                    ColWidth(1) = ""
                                    ColSortable(1) = False
                                    '
                                    DisplaySystem = False
                                    If False Then
                                        '
                                        ' before system attribute
                                        '
                                        CS = cpCore.db.cs_open("Add-on Collections", , "Name")
                                    ElseIf Not cpCore.user.isAuthenticatedDeveloper Then
                                        '
                                        ' non-developers
                                        '
                                        CS = cpCore.db.cs_open("Add-on Collections", "((system is null)or(system=0))", "Name")
                                    Else
                                        '
                                        ' developers
                                        '
                                        DisplaySystem = True
                                        CS = cpCore.db.cs_open("Add-on Collections", , "Name")
                                    End If
                                    ReDim Preserve Cells(cpCore.db.cs_getRowCount(CS), ColumnCnt)
                                    RowPtr = 0
                                    Do While cpCore.db.cs_ok(CS)
                                        Cells(RowPtr, 0) = cpCore.html_GetFormInputCheckBox2("AC" & RowPtr) & cpCore.html_GetFormInputHidden("ACID" & RowPtr, cpCore.db.cs_getInteger(CS, "ID"))
                                        'Cells(RowPtr, 1) = "<a href=""" & cpcore.app.SiteProperty_AdminURL & "?id=" & cpcore.app.cs_getInteger(CS, "ID") & "&cid=" & cpcore.app.cs_getInteger(CS, "ContentControlID") & "&af=4""><img src=""/ccLib/images/IconContentEdit.gif"" border=0></a>"
                                        Cells(RowPtr, 1) = cpCore.db.cs_getText(CS, "name")
                                        If DisplaySystem Then
                                            If cpCore.db.cs_getBoolean(CS, "system") Then
                                                Cells(RowPtr, 1) = Cells(RowPtr, 1) & " (system)"
                                            End If
                                        End If
                                        Call cpCore.db.cs_goNext(CS)
                                        RowPtr = RowPtr + 1
                                    Loop
                                    Call cpCore.db.cs_Close(CS)
                                    BodyHTML = "<div style=""width:100%"">" & Adminui.GetReport2(RowPtr, ColCaption, ColAlign, ColWidth, Cells, RowPtr, 1, "", PostTableCopy, RowPtr, "ccAdmin", ColSortable, 0) & "</div>"
                                    BodyHTML = Adminui.GetEditPanel(True, "Add-on Collections", "Use this form to review and delete current add-on collections.", BodyHTML)
                                    BodyHTML = BodyHTML & cpCore.html_GetFormInputHidden("accnt", RowPtr)
                                    Call cpCore.main_AddLiveTabEntry("Installed&nbsp;Collections", BodyHTML, "ccAdminTab")
                                    '
                                    ' --------------------------------------------------------------------------------
                                    ' Get the Upload Add-ons tab
                                    ' --------------------------------------------------------------------------------
                                    '
                                    Body = New coreFastStringClass
                                    If Not DbUpToDate Then
                                        Call Body.Add("<p>Add-on upload is disabled because your site database needs to be updated.</p>")
                                    Else
                                        Call Body.Add(Adminui.EditTableOpen)
                                        If cpCore.user.isAuthenticatedDeveloper Then
                                            Call Body.Add(Adminui.GetEditRow(cpCore.html_GetFormInputCheckBox2("InstallCore"), "Reinstall Core Collection", "", False, False, ""))
                                        End If
                                        Call Body.Add(Adminui.GetEditRow(cpCore.html_GetFormInputFile("MetaFile"), "Add-on Collection File(s)", "", True, False, ""))
                                        FormInput = "" _
                                        & "<table id=""UploadInsert"" border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"">" _
                                        & "</table>" _
                                        & "<table border=""0"" cellpadding=""0"" cellspacing=""1"" width=""100%"">" _
                                        & "<tr><td align=""left""><a href=""#"" onClick=""InsertUpload(); return false;"">+ Add more files</a></td></tr>" _
                                        & "</table>" _
                                        & cpCore.html_GetFormInputHidden("UploadCount", 1, "UploadCount") _
                                        & ""
                                        Call Body.Add(Adminui.GetEditRow(FormInput, "&nbsp;", "", True, False, ""))
                                        Call Body.Add(Adminui.EditTableClose)
                                    End If
                                    Call cpCore.main_AddLiveTabEntry("Add&nbsp;Manually", Adminui.GetEditPanel(True, "Install or Update an Add-on Collection.", "Use this form to upload a new or updated Add-on Collection to your site. A collection file can be a single xml configuration file, a single zip file containing the configuration file and other resource files, or a configuration with other resource files uploaded separately. Use the 'Add more files' link to add as many files as you need. When you hit OK, the Collection will be checked, and only submitted if all files are uploaded.", Body.Text), "ccAdminTab")
                                    '
                                    ' --------------------------------------------------------------------------------
                                    ' Build Page from tabs
                                    ' --------------------------------------------------------------------------------
                                    '
                                    Content.Add(cpCore.main_GetLiveTabs())
                                    '
                                    ButtonList = ButtonCancel & "," & ButtonOK
                                    Content.Add(cpCore.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormLegacyAddonManager))
                                End If
                            End If
                        End If
                        '
                        ' Output the Add-on
                        '
                        Caption = "Add-on Manager (Safe Mode)"
                    Description = "<div>Use the add-on manager to add and remove Add-ons from your Contensive installation.</div>"
                    If Not DbUpToDate Then
                        Description = Description & "<div style=""Margin-left:50px"">The Add-on Manager is disabled because this site's Database needs to be upgraded.</div>"
                    End If
                    If status <> "" Then
                        Description = Description & "<div style=""Margin-left:50px"">" & status & "</div>"
                    End If
                    addonManager = Adminui.GetBody(Caption, ButtonList, "", False, False, Description, "", 0, Content.Text)
                    Call cpCore.main_AddPagetitle("Add-on Manager")
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return addonManager
        End Function
        '
        '
        '
        Private Sub GetForm_SafeModeAddonManager_DeleteNavigatorBranch(ByVal EntryName As String, ByVal EntryParentID As Integer)
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            Dim EntryID As Integer
            '
            If EntryParentID = 0 Then
                CS = cpCore.db.cs_open("Navigator Entries", "(name=" & cpCore.db.encodeSQLText(EntryName) & ")and((parentID is null)or(parentid=0))")
            Else
                CS = cpCore.db.cs_open("Navigator Entries", "(name=" & cpCore.db.encodeSQLText(EntryName) & ")and(parentID=" & cpCore.db.encodeSQLNumber(EntryParentID) & ")")
            End If
            If cpCore.db.cs_ok(CS) Then
                EntryID = cpCore.db.cs_getInteger(CS, "ID")
            End If
            Call cpCore.db.cs_Close(CS)
            '
            If EntryID <> 0 Then
                CS = cpCore.db.cs_open("Navigator Entries", "(parentID=" & cpCore.db.encodeSQLNumber(EntryID) & ")")
                Do While cpCore.db.cs_ok(CS)
                    Call GetForm_SafeModeAddonManager_DeleteNavigatorBranch(cpCore.db.cs_getText(CS, "name"), EntryID)
                    Call cpCore.db.cs_goNext(CS)
                Loop
                Call cpCore.db.cs_Close(CS)
                Call cpCore.DeleteContentRecord("Navigator Entries", EntryID)
            End If
            '
            Exit Sub
ErrorTrap:
            Call HandleClassTrapError("GetForm_SafeModeAddonManager_DeleteNavigatorBranch")
        End Sub
        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Private Function GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            On Error GoTo ErrorTrap
            '
            Dim NodeAttribute As XmlAttribute
            Dim ResultNode As XmlNode
            Dim UcaseName As String
            '
            Found = False
            ResultNode = Node.Attributes.GetNamedItem(Name)
            If (ResultNode Is Nothing) Then
                UcaseName = vbUCase(Name)
                For Each NodeAttribute In Node.Attributes
                    If vbUCase(NodeAttribute.Name) = UcaseName Then
                        GetXMLAttribute = NodeAttribute.Value
                        Found = True
                        Exit For
                    End If
                Next
            Else
                GetXMLAttribute = ResultNode.Value
                Found = True
            End If
            If Not Found Then
                GetXMLAttribute = DefaultIfNotFound
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassTrapError("GetXMLAttribute")
        End Function
        '
        '
        '
        Private Sub HandleClassAppendLog(ByVal MethodName As String, ByVal Context As String)
            cpCore.appendLogWithLegacyRow(cpCore.serverConfig.appConfig.name, Context, "dll", "AddonManClass", MethodName, 0, "", "", False, True, cpCore.webServerIO_ServerLink, "", "")

        End Sub
        '
        '===========================================================================
        '
        '===========================================================================
        '
        Private Sub HandleClassTrapError(ByVal MethodName As String, Optional ByVal Context As String = "context unknown")
            '
            cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, Context, "dll", "AddonManClass", MethodName, Err.Number, Err.Source, Err.Description, True, False, cpCore.webServerIO_ServerLink)
            '
        End Sub
        '
        '
        '
        Public Function GetParentIDFromNameSpace(ByVal ContentName As String, ByVal menuNameSpace As String) As Integer
            On Error GoTo ErrorTrap
            '
            Dim NameSplit() As String
            Dim ParentNameSpace As String
            Dim ParentName As String
            Dim ParentID As Integer
            Dim Pos As Integer
            Dim CS As Integer
            '
            GetParentIDFromNameSpace = 0
            If menuNameSpace <> "" Then
                'ParentName = ParentNameSpace
                Pos = vbInstr(1, menuNameSpace, ".")
                If Pos = 0 Then
                    ParentName = menuNameSpace
                    ParentNameSpace = ""
                Else
                    ParentName = Mid(menuNameSpace, Pos + 1)
                    ParentNameSpace = Mid(menuNameSpace, 1, Pos - 1)
                End If
                If ParentNameSpace = "" Then
                    CS = cpCore.db.cs_open(ContentName, "(name=" & cpCore.db.encodeSQLText(ParentName) & ")and((parentid is null)or(parentid=0))", "ID", , , , , "ID")
                    If cpCore.db.cs_ok(CS) Then
                        GetParentIDFromNameSpace = cpCore.db.cs_getInteger(CS, "ID")
                    End If
                    Call cpCore.db.cs_Close(CS)
                Else
                    ParentID = GetParentIDFromNameSpace(ContentName, ParentNameSpace)
                    CS = cpCore.db.cs_open(ContentName, "(name=" & cpCore.db.encodeSQLText(ParentName) & ")and(parentid=" & ParentID & ")", "ID", , , , , "ID")
                    If cpCore.db.cs_ok(CS) Then
                        GetParentIDFromNameSpace = cpCore.db.cs_getInteger(CS, "ID")
                    End If
                    Call cpCore.db.cs_Close(CS)
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call HandleClassTrapError("GetParentIDFromNameSpace")
        End Function
    End Class
End Namespace
