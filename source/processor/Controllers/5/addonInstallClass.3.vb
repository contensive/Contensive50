
Option Explicit On
Option Strict On

Imports System.Xml
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' install addon collections
    ''' </summary>
    Public Class addonInstallClass


        '
        '============================================================================
        '   process the include add-on node of the add-on nodes
        '       this is the second pass, so all add-ons should be added
        '       no errors for missing addones, except the include add-on case
        '============================================================================
        '
        Private Shared Function InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore As coreClass, ByVal AddonNode As XmlNode, ByVal AddonGuidFieldName As String, ByVal ignore_BuildVersion As String, ByVal CollectionID As Integer, ByRef ReturnUpgradeOK As Boolean, ByRef ReturnErrorMessage As String) As String
            Dim result As String = ""
            Try
                Dim AddRule As Boolean
                Dim IncludeAddonName As String
                Dim IncludeAddonGuid As String
                Dim IncludeAddonID As Integer
                Dim UserError As String
                Dim CS2 As Integer
                Dim PageInterface As XmlNode
                Dim NavDeveloperOnly As Boolean
                Dim StyleSheet As String
                Dim ArgumentList As String
                Dim CS As Integer
                Dim Criteria As String
                Dim IsFound As Boolean
                Dim AOName As String
                Dim AOGuid As String
                Dim AddOnType As String
                Dim addonId As Integer
                Dim Basename As String
                '
                Basename = genericController.vbLCase(AddonNode.Name)
                If (Basename = "page") Or (Basename = "process") Or (Basename = "addon") Or (Basename = "add-on") Then
                    AOName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name")
                    If AOName = "" Then
                        AOName = "No Name"
                    End If
                    AOGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", AOName)
                    If AOGuid = "" Then
                        AOGuid = AOName
                    End If
                    AddOnType = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "")
                    Criteria = "(" & AddonGuidFieldName & "=" & cpCore.db.encodeSQLText(AOGuid) & ")"
                    CS = cpCore.db.csOpen(cnAddons, Criteria, , False)
                    If cpCore.db.csOk(CS) Then
                        '
                        ' Update the Addon
                        '
                        Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" & AOName & "], Guid [" & AOGuid & "]")
                    Else
                        '
                        ' not found by GUID - search name against name to update legacy Add-ons
                        '
                        Call cpCore.db.csClose(CS)
                        Criteria = "(name=" & cpCore.db.encodeSQLText(AOName) & ")and(" & AddonGuidFieldName & " is null)"
                        CS = cpCore.db.csOpen(cnAddons, Criteria, , False)
                    End If
                    If Not cpCore.db.csOk(CS) Then
                        '
                        ' Could not find add-on
                        '
                        Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" & AOName & "], Guid [" & AOGuid & "]")
                    Else
                        addonId = cpCore.db.csGetInteger(CS, "ID")
                        ArgumentList = ""
                        StyleSheet = ""
                        NavDeveloperOnly = True
                        If AddonNode.ChildNodes.Count > 0 Then
                            For Each PageInterface In AddonNode.ChildNodes
                                Select Case genericController.vbLCase(PageInterface.Name)
                                    Case "includeaddon", "includeadd-on", "include addon", "include add-on"
                                        '
                                        ' include add-ons - NOTE - import collections must be run before interfaces
                                        ' when importing a collectin that will be used for an include
                                        '
                                        If True Then
                                            IncludeAddonName = GetXMLAttribute(cpCore, IsFound, PageInterface, "name", "")
                                            IncludeAddonGuid = GetXMLAttribute(cpCore, IsFound, PageInterface, "guid", IncludeAddonName)
                                            IncludeAddonID = 0
                                            Criteria = ""
                                            If IncludeAddonGuid <> "" Then
                                                Criteria = AddonGuidFieldName & "=" & cpCore.db.encodeSQLText(IncludeAddonGuid)
                                                If IncludeAddonName = "" Then
                                                    IncludeAddonName = "Add-on " & IncludeAddonGuid
                                                End If
                                            ElseIf IncludeAddonName <> "" Then
                                                Criteria = "(name=" & cpCore.db.encodeSQLText(IncludeAddonName) & ")"
                                            End If
                                            If Criteria <> "" Then
                                                CS2 = cpCore.db.csOpen(cnAddons, Criteria)
                                                If cpCore.db.csOk(CS2) Then
                                                    IncludeAddonID = cpCore.db.csGetInteger(CS2, "ID")
                                                End If
                                                Call cpCore.db.csClose(CS2)
                                                AddRule = False
                                                If IncludeAddonID = 0 Then
                                                    UserError = "The include add-on [" & IncludeAddonName & "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it."
                                                    Call logController.appendInstallLog(cpCore, "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" & UserError & "]")
                                                    ReturnUpgradeOK = False
                                                    ReturnErrorMessage = ReturnErrorMessage & "<P>The collection was not installed because the add-on [" & AOName & "] requires an included add-on [" & IncludeAddonName & "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>"
                                                Else
                                                    CS2 = cpCore.db.csOpenSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" & addonId & " and IncludedAddonID=" & IncludeAddonID)
                                                    AddRule = Not cpCore.db.csOk(CS2)
                                                    Call cpCore.db.csClose(CS2)
                                                End If
                                                If AddRule Then
                                                    CS2 = cpCore.db.csInsertRecord("Add-on Include Rules", 0)
                                                    If cpCore.db.csOk(CS2) Then
                                                        Call cpCore.db.csSet(CS2, "Addonid", addonId)
                                                        Call cpCore.db.csSet(CS2, "IncludedAddonID", IncludeAddonID)
                                                    End If
                                                    Call cpCore.db.csClose(CS2)
                                                End If
                                            End If
                                        End If
                                End Select
                            Next
                        End If
                    End If
                    Call cpCore.db.csClose(CS)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
            '
        End Function
        ''
        ''====================================================================================================
        'Public Sub housekeepAddonFolder()
        '    Try
        '        Dim RegisterPathList As String
        '        Dim RegisterPath As String
        '        Dim RegisterPaths() As String
        '        Dim Path As String
        '        Dim NodeCnt As Integer
        '        Dim IsActiveFolder As Boolean
        '        Dim Cmd As String
        '        Dim CollectionRootPath As String
        '        Dim Pos As Integer
        '        Dim FolderList As IO.DirectoryInfo()
        '        Dim LocalPath As String
        '        Dim LocalGuid As String
        '        Dim Doc As New XmlDocument
        '        Dim CollectionNode As XmlNode
        '        Dim LocalListNode As XmlNode
        '        Dim FolderPtr As Integer
        '        Dim CollectionPath As String
        '        Dim LastChangeDate As Date
        '        Dim hint As String
        '        Dim LocalName As String
        '        Dim Ptr As Integer
        '        '
        '        Call AppendClassLogFile("Server", "RegisterAddonFolder", "Entering RegisterAddonFolder")
        '        '
        '        Dim loadOK As Boolean = True
        '        Try
        '            Call Doc.LoadXml(getCollectionListFile)
        '        Catch ex As Exception
        '            'hint = hint & ",parse error"
        '            Call AppendClassLogFile("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], Error loading Collections.xml file.")
        '            loadOK = False
        '        End Try
        '        If loadOK Then
        '            '
        '            Call AppendClassLogFile("Server", "RegisterAddonFolder", "Collection.xml loaded ok")
        '            '
        '            If genericController.vbLCase(Doc.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
        '                Call AppendClassLogFile("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.")
        '            Else
        '                '
        '                Call AppendClassLogFile("Server", "RegisterAddonFolder", "Collection.xml root name ok")
        '                '
        '                With Doc.DocumentElement
        '                    If True Then
        '                        'If genericController.vbLCase(.name) <> "collectionlist" Then
        '                        '    Call AppendClassLogFile("Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
        '                        'Else
        '                        NodeCnt = 0
        '                        RegisterPathList = ""
        '                        For Each LocalListNode In .ChildNodes
        '                            '
        '                            ' Get the collection path
        '                            '
        '                            CollectionPath = ""
        '                            LocalGuid = ""
        '                            LocalName = "no name found"
        '                            LocalPath = ""
        '                            Select Case genericController.vbLCase(LocalListNode.Name)
        '                                Case "collection"
        '                                    LocalGuid = ""
        '                                    For Each CollectionNode In LocalListNode.ChildNodes
        '                                        Select Case genericController.vbLCase(CollectionNode.Name)
        '                                            Case "name"
        '                                                '
        '                                                LocalName = genericController.vbLCase(CollectionNode.InnerText)
        '                                            Case "guid"
        '                                                '
        '                                                LocalGuid = genericController.vbLCase(CollectionNode.InnerText)
        '                                            Case "path"
        '                                                '
        '                                                CollectionPath = genericController.vbLCase(CollectionNode.InnerText)
        '                                            Case "lastchangedate"
        '                                                LastChangeDate =  genericController.EncodeDate(CollectionNode.InnerText)
        '                                        End Select
        '                                    Next
        '                            End Select
        '                            '
        '                            Call AppendClassLogFile("Server", "RegisterAddonFolder", "Node[" & NodeCnt & "], LocalName=[" & LocalName & "], LastChangeDate=[" & LastChangeDate & "], CollectionPath=[" & CollectionPath & "], LocalGuid=[" & LocalGuid & "]")
        '                            '
        '                            ' Go through all subpaths of the collection path, register the version match, unregister all others
        '                            '
        '                            'fs = New fileSystemClass
        '                            If CollectionPath = "" Then
        '                                '
        '                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "no collection path, skipping")
        '                                '
        '                            Else
        '                                CollectionPath = genericController.vbLCase(CollectionPath)
        '                                CollectionRootPath = CollectionPath
        '                                Pos = InStrRev(CollectionRootPath, "\")
        '                                If Pos <= 0 Then
        '                                    '
        '                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "CollectionPath has no '\', skipping")
        '                                    '
        '                                Else
        '                                    CollectionRootPath = Left(CollectionRootPath, Pos - 1)
        '                                    Path = cpCore.app.getAddonPath() & "\" & CollectionRootPath & "\"
        '                                    'Path = GetProgramPath & "\addons\" & CollectionRootPath & "\"
        '                                    'On Error Resume Next
        '                                    If cpCore.app.privateFiles.checkPath(Path) Then
        '                                        FolderList = cpCore.app.privateFiles.getFolders(Path)
        '                                        If Err.Number <> 0 Then
        '                                            Err.Clear()
        '                                        End If
        '                                    End If
        '                                    'On Error GoTo ErrorTrap
        '                                    If FolderList.Length = 0 Then
        '                                        '
        '                                        Call AppendClassLogFile("Server", "RegisterAddonFolder", "no subfolders found in physical path [" & Path & "], skipping")
        '                                        '
        '                                    Else
        '                                        For Each dir As IO.DirectoryInfo In FolderList
        '                                            IsActiveFolder = False
        '                                            '
        '                                            ' register or unregister all files in this folder
        '                                            '
        '                                            If dir.Name = "" Then
        '                                                '
        '                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....empty folder [" & dir.Name & "], skipping")
        '                                                '
        '                                            Else
        '                                                '
        '                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Folder [" & dir.Name & "]")
        '                                                IsActiveFolder = (CollectionRootPath & "\" & dir.Name = CollectionPath)
        '                                                If IsActiveFolder And (FolderPtr <> (FolderList.Count - 1)) Then
        '                                                    '
        '                                                    ' This one is active, but not the last
        '                                                    '
        '                                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Active addon is not the most current, this folder is the active folder, but there are more recent folders. This folder will be preserved.")
        '                                                End If
        '                                                ' 20161005 - no longer need to register activeX
        '                                                'FileList = cpCore.app.privateFiles.GetFolderFiles(Path & "\" & dir.Name)
        '                                                'For Each file As IO.FileInfo In FileList
        '                                                '    If Right(file.Name, 4) = ".dll" Then
        '                                                '        If IsActiveFolder Then
        '                                                '            '
        '                                                '            ' register this file
        '                                                '            '
        '                                                '            RegisterPathList = RegisterPathList & vbCrLf & Path & dir.Name & "\" & file.Name
        '                                                '            '                                                                Cmd = "%comspec% /c regsvr32 """ & RegisterPathList & """ /s"
        '                                                '            '                                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Regsiter DLL [" & Cmd & "]")
        '                                                '            '                                                                Call runProcess(cp.core,Cmd, , True)
        '                                                '        Else
        '                                                '            '
        '                                                '            ' unregister this file
        '                                                '            '
        '                                                '            Cmd = "%comspec% /c regsvr32 /u """ & Path & dir.Name & "\" & file.Name & """ /s"
        '                                                '            Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Unregsiter DLL [" & Cmd & "]")
        '                                                '            Call runProcess(cpCore, Cmd, , True)
        '                                                '        End If
        '                                                '    End If
        '                                                'Next
        '                                                '
        '                                                ' only keep last two non-matching folders and the active folder
        '                                                '
        '                                                If IsActiveFolder Then
        '                                                    IsActiveFolder = IsActiveFolder
        '                                                Else
        '                                                    If FolderPtr < (FolderList.Count - 3) Then
        '                                                        Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Deleting path because non-active and not one of the newest 2 [" & Path & dir.Name & "]")
        '                                                        Call cpCore.app.privateFiles.DeleteFileFolder(Path & dir.Name)
        '                                                    End If
        '                                                End If
        '                                            End If
        '                                        Next
        '                                        '
        '                                        ' register files found in the active folder last
        '                                        '
        '                                        If RegisterPathList <> "" Then
        '                                            RegisterPaths = Split(RegisterPathList, vbCrLf)
        '                                            For Ptr = 0 To UBound(RegisterPaths)
        '                                                RegisterPath = Trim(RegisterPaths(Ptr))
        '                                                If RegisterPath <> "" Then
        '                                                    Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
        '                                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
        '                                                    Call runProcess(cpCore, Cmd, , True)
        '                                                End If
        '                                            Next
        '                                            RegisterPathList = ""
        '                                        End If
        '                                    End If
        '                                End If
        '                            End If
        '                            NodeCnt = NodeCnt + 1
        '                        Next
        '                        ' 20161005 - no longer need to register activeX
        '                        ''
        '                        '' register files found in the active folder last
        '                        ''
        '                        'If RegisterPathList <> "" Then
        '                        '    RegisterPaths = Split(RegisterPathList, vbCrLf)
        '                        '    For Ptr = 0 To UBound(RegisterPaths)
        '                        '        RegisterPath = Trim(RegisterPaths(Ptr))
        '                        '        If RegisterPath <> "" Then
        '                        '            Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
        '                        '            Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
        '                        '            Call runProcess(cpCore, Cmd, , True)
        '                        '        End If
        '                        '    Next
        '                        'End If
        '                    End If
        '                End With
        '            End If
        '        End If
        '        '
        '        Call AppendClassLogFile("Server", "RegisterAddonFolder", "Exiting RegisterAddonFolder")
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        '        '
        '        '=============================================================================
        '        '   Verify an Admin Navigator Entry
        '        '       Entries are unique by their ccGuid
        '        '       Includes InstalledByCollectionID
        '        '       returns the entry id
        '        '=============================================================================
        '        '
        '        private Function csv_VerifyNavigatorEntry4(ByVal asv As appServicesClass, ByVal ccGuid As String, ByVal menuNameSpace As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal MenuContentName As String, ByVal AddonName As String, ByVal NavIconType As String, ByVal NavIconTitle As String, ByVal InstalledByCollectionID As Integer) As Integer
        '            On Error GoTo ErrorTrap : 'Const Tn = "VerifyNavigatorEntry4" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Const AddonContentName = cnAddons
        '            '
        '            Dim DupFound As Boolean
        '            Dim EntryID As Integer
        '            Dim DuplicateID As Integer
        '            Dim Parents() As String
        '            Dim ParentPtr As Integer
        '            Dim Criteria As String
        '            Dim Ptr As Integer
        '            Dim RecordID As Integer
        '            Dim RecordName As String
        '            Dim SelectList As String
        '            Dim ErrorDescription As String
        '            Dim CSEntry As Integer
        '            Dim ContentID As Integer
        '            Dim ParentID As Integer
        '            Dim addonId As Integer
        '            Dim CS As Integer
        '            Dim SupportAddonID As Boolean
        '            Dim SupportGuid As Boolean
        '            Dim SupportNavGuid As Boolean
        '            Dim SupportccGuid As Boolean
        '            Dim SupportNavIcon As Boolean
        '            Dim GuidFieldName As String
        '            Dim SupportInstalledByCollectionID As Boolean
        '            '
        '            If Trim(EntryName) <> "" Then
        '                If genericController.vbLCase(EntryName) = "manage add-ons" Then
        '                    EntryName = EntryName
        '                End If
        '                '
        '                ' Setup misc arguments
        '                '
        '                SelectList = "Name,ContentID,ParentID,LinkPage,SortOrder,AdminOnly,DeveloperOnly,NewWindow,Active,NavIconType,NavIconTitle"
        '                SupportAddonID = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "AddonID")
        '                SupportInstalledByCollectionID = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "InstalledByCollectionID")
        '                If SupportAddonID Then
        '                    SelectList = SelectList & ",AddonID"
        '                Else
        '                    SelectList = SelectList & ",0 as AddonID"
        '                End If
        '                If SupportInstalledByCollectionID Then
        '                    SelectList = SelectList & ",InstalledByCollectionID"
        '                End If
        '                If cpCore.app.csv_IsContentFieldSupported(MenuContentName, "ccGuid") Then
        '                    SupportGuid = True
        '                    SupportccGuid = True
        '                    GuidFieldName = "ccguid"
        '                    SelectList = SelectList & ",ccGuid"
        '                ElseIf cpCore.app.csv_IsContentFieldSupported(MenuContentName, "NavGuid") Then
        '                    SupportGuid = True
        '                    SupportNavGuid = True
        '                    GuidFieldName = "navguid"
        '                    SelectList = SelectList & ",NavGuid"
        '                Else
        '                    SelectList = SelectList & ",'' as ccGuid"
        '                End If
        '                SupportNavIcon = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "NavIconType")
        '                addonId = 0
        '                If SupportAddonID And (AddonName <> "") Then
        '                    CS = cpCore.app.csOpen(AddonContentName, "name=" & EncodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        '                    If cpCore.app.csv_IsCSOK(CS) Then
        '                        addonId = cpCore.app.csv_cs_getInteger(CS, "ID")
        '                    End If
        '                    Call cpCore.app.csv_CloseCS(CS)
        '                End If
        '                ParentID = csv_GetParentIDFromNameSpace(asv, MenuContentName, menuNameSpace)
        '                ContentID = -1
        '                If ContentName <> "" Then
        '                    ContentID = cpCore.app.csv_GetContentID(ContentName)
        '                End If
        '                '
        '                ' Locate current entry(s)
        '                '
        '                CSEntry = -1
        '                Criteria = ""
        '                If True Then
        '                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        '                    If ccGuid = "" Then
        '                        '
        '                        ' ----- Find match by menuNameSpace
        '                        '
        '                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(Parentid=" & ParentID & ")and((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))", "ID", True, , , , SelectList)
        '                    Else
        '                        '
        '                        ' ----- Find match by guid
        '                        '
        '                        CSEntry = cpCore.app.csOpen(MenuContentName, "(" & GuidFieldName & "=" & EncodeSQLText(ccGuid) & ")", "ID", True, , , , SelectList)
        '                    End If
        '                    If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        '                        '
        '                        ' ----- if not found by guid, look for a name/parent match with a blank guid
        '                        '
        '                        Call cpCore.app.csv_CloseCS(CSEntry)
        '                        Criteria = "AND((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))"
        '                    End If
        '                End If
        '                If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        '                    If ParentID = 0 Then
        '                        ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        '                        Criteria = Criteria & "And(name=" & EncodeSQLText(EntryName) & ")and(ParentID is null)"
        '                    Else
        '                        ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        '                        Criteria = Criteria & "And(name=" & EncodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")"
        '                    End If
        '                    CSEntry = cpCore.app.csOpen(MenuContentName, Mid(Criteria, 4), "ID", True, , , , SelectList)
        '                End If
        '                '
        '                ' If no current entry, create one
        '                '
        '                If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        '                    cpCore.app.csv_CloseCS(CSEntry)
        '                    '
        '                    ' This entry was not found - insert a new record if there is no other name/menuNameSpace match
        '                    '
        '                    If False Then
        '                        '
        '                        ' OK - the first entry search was name/menuNameSpace
        '                        '
        '                        DupFound = False
        '                    ElseIf ParentID = 0 Then
        '                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(ParentID is null)", "ID", False, , , , SelectList)
        '                        DupFound = cpCore.app.csv_IsCSOK(CSEntry)
        '                        cpCore.app.csv_CloseCS(CSEntry)
        '                    Else
        '                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")", "ID", False, , , , SelectList)
        '                        DupFound = cpCore.app.csv_IsCSOK(CSEntry)
        '                        cpCore.app.csv_CloseCS(CSEntry)
        '                    End If
        '                    If DupFound Then
        '                        '
        '                        ' Must block this entry because a menuNameSpace duplicate exists
        '                        '
        '                        CSEntry = -1
        '                    Else
        '                        '
        '                        ' Create new entry
        '                        '
        '                        CSEntry = cpCore.app.csv_InsertCSRecord(MenuContentName, SystemMemberID)
        '                    End If
        '                End If
        '                If cpCore.app.csv_IsCSOK(CSEntry) Then
        '                    EntryID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        '                    If EntryID = 265 Then
        '                        EntryID = EntryID
        '                    End If
        '                    Call cpCore.app.csv_SetCS(CSEntry, "name", EntryName)
        '                    If ParentID = 0 Then
        '                        Call cpCore.app.csv_SetCS(CSEntry, "ParentID", Nothing)
        '                    Else
        '                        Call cpCore.app.csv_SetCS(CSEntry, "ParentID", ParentID)
        '                    End If
        '                    If (ContentID = -1) Then
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "ContentID", Nothing)
        '                    Else
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "ContentID", ContentID)
        '                    End If
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "LinkPage", LinkPage)
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "SortOrder", SortOrder)
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "AdminOnly", AdminOnly)
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "DeveloperOnly", DeveloperOnly)
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "NewWindow", NewWindow)
        '                    Call cpCore.app.csv_SetCSField(CSEntry, "Active", Active)
        '                    If SupportAddonID Then
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "AddonID", addonId)
        '                    End If
        '                    If SupportGuid Then
        '                        Call cpCore.app.csv_SetCSField(CSEntry, GuidFieldName, ccGuid)
        '                    End If
        '                    If SupportNavIcon Then
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "NavIconTitle", NavIconTitle)
        '                        Dim NavIconID As Integer
        '                        NavIconID = GetListIndex(NavIconType, NavIconTypeList)
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "NavIconType", NavIconID)
        '                    End If
        '                    If SupportInstalledByCollectionID Then
        '                        Call cpCore.app.csv_SetCSField(CSEntry, "InstalledByCollectionID", InstalledByCollectionID)
        '                    End If
        '                    '
        '                    ' merge any duplicate guid matches
        '                    '
        '                    Call cpCore.app.csv_NextCSRecord(CSEntry)
        '                    Do While cpCore.app.csv_IsCSOK(CSEntry)
        '                        DuplicateID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        '                        Call cpCore.app.ExecuteSQL( "update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        '                        Call cpCore.app.csv_DeleteContentRecord(MenuContentName, DuplicateID)
        '                        Call cpCore.app.csv_NextCSRecord(CSEntry)
        '                    Loop
        '                End If
        '                Call cpCore.app.csv_CloseCS(CSEntry)
        '                '
        '                ' Merge duplicates with menuNameSpace.Name match
        '                '
        '                If EntryID <> 0 Then
        '                    If ParentID = 0 Then
        '                        CSEntry = cpCore.app.csv_OpenCSSQL("default", "select * from ccMenuEntries where (parentid is null)and(name=" & EncodeSQLText(EntryName) & ")and(id<>" & EntryID & ")", 0)
        '                    Else
        '                        CSEntry = cpCore.app.csv_OpenCSSQL("default", "select * from ccMenuEntries where (parentid=" & ParentID & ")and(name=" & EncodeSQLText(EntryName) & ")and(id<>" & EntryID & ")", 0)
        '                    End If
        '                    Do While cpCore.app.csv_IsCSOK(CSEntry)
        '                        DuplicateID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        '                        Call cpCore.app.ExecuteSQL( "update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        '                        Call cpCore.app.csv_DeleteContentRecord(MenuContentName, DuplicateID)
        '                        Call cpCore.app.csv_NextCSRecord(CSEntry)
        '                    Loop
        '                    Call cpCore.app.csv_CloseCS(CSEntry)
        '                End If
        '            End If
        '            '
        '            csv_VerifyNavigatorEntry4 = EntryID
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_VerifyNavigatorEntry4", True, False)
        '        End Function
        '        '
        '        '
        '        '
        '        private Function csv_GetParentIDFromNameSpace(ByVal asv As appServicesClass, ByVal ContentName As String, ByVal menuNameSpace As String) As Integer
        '            On Error GoTo ErrorTrap : 'Const Tn = "GetParentIDFrommenuNameSpace" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim Parents() As String
        '            Dim ParentID As Integer
        '            Dim Ptr As Integer
        '            Dim RecordName As String
        '            Dim Criteria As String
        '            Dim CS As Integer
        '            Dim RecordID As Integer
        '            '
        '            Parents = Split(menuNameSpace, ".")
        '            ParentID = 0
        '            For Ptr = 0 To UBound(Parents)
        '                RecordName = Parents(Ptr)
        '                If ParentID = 0 Then
        '                    Criteria = "(name=" & EncodeSQLText(RecordName) & ")and(Parentid is null)"
        '                Else
        '                    Criteria = "(name=" & EncodeSQLText(RecordName) & ")and(Parentid=" & ParentID & ")"
        '                End If
        '                RecordID = 0
        '                ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to attach to the active entries
        '                CS = cpCore.app.csOpen(ContentName, Criteria, "ID", True, , , , "ID", 1)
        '                If cpCore.app.csv_IsCSOK(CS) Then
        '                    RecordID = (cpCore.app.csv_cs_getInteger(CS, "ID"))
        '                End If
        '                Call cpCore.app.csv_CloseCS(CS)
        '                If RecordID = 0 Then
        '                    CS = cpCore.app.csv_InsertCSRecord(ContentName, SystemMemberID)
        '                    If cpCore.app.csv_IsCSOK(CS) Then
        '                        RecordID = cpCore.app.csv_cs_getInteger(CS, "ID")
        '                        Call cpCore.app.csv_SetCS(CS, "name", RecordName)
        '                        If ParentID <> 0 Then
        '                            Call cpCore.app.csv_SetCS(CS, "parentID", ParentID)
        '                        End If
        '                    End If
        '                    Call cpCore.app.csv_CloseCS(CS)
        '                End If
        '                ParentID = RecordID
        '            Next
        '            '
        '            csv_GetParentIDFromNameSpace = ParentID

        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Call HandleClassTrapError(cpCore.app.config.name, Err.Number, Err.Source, Err.Description, "unknownMethodNameLegacyCall", True)
        '        End Function
        ''
        ''=========================================================================================================
        ''   Use AppendClassLogFile
        ''=========================================================================================================
        ''
        'private Sub AppendAddonFile(ByVal ApplicationName As String, ByVal Method As String, ByVal Cause As String)
        '    logController.appendLogWithLegacyRow( cpcore,ApplicationName, Cause, "dll", "AddonInstallClass", Method, 0, "", "", False, True, "", "AddonInstall", "")

        'End Sub
        ''
        ''===========================================================================
        ''   Append Log File
        ''===========================================================================
        ''
        'Private Shared Sub logcontroller.appendInstallLog(cpCore As coreClass, ByVal ignore As String, ByVal Method As String, ByVal LogMessage As String)
        '    Try
        '        Console.WriteLine("install, " & LogMessage)
        '        logController.appendLog(cpCore, LogMessage, "install")
        '    Catch ex As Exception
        '        cpCore.handleException(ex)
        '    End Try
        'End Sub
        '
        '=========================================================================================
        '   Import CDef on top of current configuration and the base configuration
        '
        '=========================================================================================
        '
        Public Shared Sub installBaseCollection(cpCore As coreClass, isNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String))
            Try
                Dim ignoreString As String = ""
                Dim returnErrorMessage As String = ""
                Dim ignoreBoolean As Boolean = False
                Dim isBaseCollection As Boolean = True
                '
                ' -- new build
                ' 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
                Dim baseCollectionXml As String = cpCore.programFiles.readFile("aoBase5.xml")
                If (String.IsNullOrEmpty(baseCollectionXml)) Then
                    '
                    ' -- base collection notfound
                    Throw New ApplicationException("Cannot load aoBase5.xml [" & cpCore.programFiles.rootLocalPath & "aoBase5.xml]")
                Else
                    Call logController.appendInstallLog(cpCore, "Verify base collection -- new build")
                    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    Call installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    'If isNewBuild Then
                    '    '
                    '    ' -- new build
                    '    Call logcontroller.appendInstallLog(cpCore,  "Verify base collection -- new build")
                    '    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    '    Call installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    '    'Else
                    '    '    '
                    '    '    ' -- verify current build
                    '    '    Call logcontroller.appendInstallLog(cpCore,  "Verify base collection - existing build")
                    '    '    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    '    '    Dim workingCollection As miniCollectionModel = installCollection_GetApplicationMiniCollection(cpCore, False)
                    '    '    Call installCollection_AddMiniCollectionSrcToDst(cpCore, workingCollection, baseCollection, False)
                    '    '    Call installCollection_BuildDbFromMiniCollection(cpCore, workingCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    'End If
                    '
                    ' now treat as a regular collection and install - to pickup everything else 
                    '
                    Dim tmpFolderPath As String = "tmp" & genericController.GetRandomInteger().ToString & "\"
                    cpCore.privateFiles.createPath(tmpFolderPath)
                    cpCore.programFiles.copyFile("aoBase5.xml", tmpFolderPath & "aoBase5.xml", cpCore.privateFiles)
                    Dim ignoreList As New List(Of String)
                    If Not InstallCollectionsFromPrivateFolder(cpCore, tmpFolderPath, returnErrorMessage, ignoreList, isNewBuild, nonCriticalErrorList) Then
                        Throw New ApplicationException(returnErrorMessage)
                    End If
                    cpCore.privateFiles.DeleteFileFolder(tmpFolderPath)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Public Shared Sub installCollectionFromLocalRepo_BuildDbFromXmlData(cpCore As coreClass, ByVal XMLText As String, isNewBuild As Boolean, isBaseCollection As Boolean, ByRef nonCriticalErrorList As List(Of String))
            Try
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name)
                '
                ' ----- Import any CDef files, allowing for changes
                Dim miniCollectionToAdd As New miniCollectionModel
                Dim miniCollectionWorking As miniCollectionModel = installCollection_GetApplicationMiniCollection(cpCore, isNewBuild)
                miniCollectionToAdd = installCollection_LoadXmlToMiniCollection(cpCore, XMLText, isBaseCollection, False, isNewBuild, miniCollectionWorking)
                Call installCollection_AddMiniCollectionSrcToDst(cpCore, miniCollectionWorking, miniCollectionToAdd, True)
                Call installCollection_BuildDbFromMiniCollection(cpCore, miniCollectionWorking, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '        '
        '        '=========================================================================================
        '        '
        '        '=========================================================================================
        '        '
        '        Private Sub UpgradeCDef_LoadFileToCollection(ByVal FilePathPage As String, byref return_Collection As CollectionType, ByVal ForceChanges As Boolean, IsNewBuild As Boolean)
        '            On Error GoTo ErrorTrap
        '            '
        '            'Dim fs As New fileSystemClass
        '            Dim IsccBaseFile As Boolean
        '            '
        '            IsccBaseFile = (InStr(1, FilePathPage, "ccBase.xml", vbTextCompare) <> 0)
        '            Call AppendClassLogFile(cpcore.app.config.name, "UpgradeCDef_LoadFileToCollection", "Application: " & cpcore.app.config.name & ", loading [" & FilePathPage & "]")
        '            Call UpgradeCDef_LoadDataToCollection(cpcore.app.publicFiles.ReadFile(FilePathPage), Return_Collection, IsccBaseFile, ForceChanges, IsNewBuild)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection", True, True)
        '            'dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection hint=[" & Hint & "]", True, True)
        '            Resume Next
        '        End Sub
        '
        '=========================================================================================
        '   create a collection class from a collection xml file
        '       - cdef are added to the cdefs in the application collection
        '=========================================================================================
        '
        Private Shared Function installCollection_LoadXmlToMiniCollection(cpCore As coreClass, ByVal srcCollecionXml As String, ByVal IsccBaseFile As Boolean, ByVal setAllDataChanged As Boolean, IsNewBuild As Boolean, defaultCollection As miniCollectionModel) As miniCollectionModel
            Dim result As miniCollectionModel
            Try
                Dim DefaultCDef As Models.Complex.cdefModel
                Dim DefaultCDefField As Models.Complex.CDefFieldModel
                Dim contentNameLc As String
                Dim XMLTools As New xmlController(cpCore)
                'Dim AddonClass As New addonInstallClass(cpCore)
                Dim status As String
                Dim CollectionGuid As String
                Dim Collectionname As String
                Dim ContentTableName As String
                Dim IsNavigator As Boolean
                Dim ActiveText As String
                Dim Name As String = String.Empty
                Dim MenuName As String
                Dim IndexName As String
                Dim TableName As String
                Dim Ptr As Integer
                Dim FieldName As String
                Dim ContentName As String
                Dim Found As Boolean
                Dim menuNameSpace As String
                Dim MenuGuid As String
                Dim MenuKey As String
                Dim CDef_Node As XmlNode
                Dim CDefChildNode As XmlNode
                Dim DataSourceName As String
                Dim srcXmlDom As New XmlDocument
                Dim NodeName As String
                Dim FieldChildNode As XmlNode
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name & ", UpgradeCDef_LoadDataToCollection")
                '
                result = New miniCollectionModel()
                '
                If String.IsNullOrEmpty(srcCollecionXml) Then
                    '
                    ' -- empty collection is an error
                    Throw (New ApplicationException("UpgradeCDef_LoadDataToCollection, srcCollectionXml is blank or null"))
                Else
                    Try
                        srcXmlDom.LoadXml(srcCollecionXml)
                    Catch ex As Exception
                        '
                        ' -- xml load error
                        logController.appendLog(cpCore, "UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" & ex.ToString & "]")
                        Throw New Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex)
                    End Try
                    With srcXmlDom.DocumentElement
                        If (LCase(.Name) <> CollectionFileRootNode) And (LCase(.Name) <> "contensivecdef") Then
                            '
                            ' -- root node must be collection (or legacy contensivecdef)
                            cpCore.handleException(New ApplicationException("the archive file has a syntax error. Application name must be the first node."))
                        Else
                            result.isBaseCollection = IsccBaseFile
                            '
                            ' Get Collection Name for logs
                            '
                            'hint = "get collection name"
                            Collectionname = GetXMLAttribute(cpCore, Found, srcXmlDom.DocumentElement, "name", "")
                            If Collectionname = "" Then
                                Call logController.appendInstallLog(cpCore, "UpgradeCDef_LoadDataToCollection, Application: " & cpCore.serverConfig.appConfig.name & ", Collection has no name")
                            Else
                                'Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & cpcore.app.appEnvironment.name & ", Collection: " & Collectionname)
                            End If
                            result.name = Collectionname
                            ''
                            '' Load possible DefaultSortMethods
                            ''
                            ''hint = "preload sort methods"
                            'SortMethodList = vbTab & "By Name" & vbTab & "By Alpha Sort Order Field" & vbTab & "By Date" & vbTab & "By Date Reverse"
                            'If cpCore.app.csv_IsContentFieldSupported("Sort Methods", "ID") Then
                            '    CS = cpCore.app.OpenCSContent("Sort Methods", , , , , , , "Name")
                            '    Do While cpCore.app.IsCSOK(CS)
                            '        SortMethodList = SortMethodList & vbTab & cpCore.app.cs_getText(CS, "name")
                            '        cpCore.app.nextCSRecord(CS)
                            '    Loop
                            '    Call cpCore.app.closeCS(CS)
                            'End If
                            'SortMethodList = SortMethodList & vbTab
                            '
                            For Each CDef_Node In .ChildNodes
                                'isCdefTarget = False
                                NodeName = genericController.vbLCase(CDef_Node.Name)
                                'hint = "read node " & NodeName
                                Select Case NodeName
                                    Case "cdef"
                                        '
                                        ' Content Definitions
                                        '
                                        ContentName = GetXMLAttribute(cpCore, Found, CDef_Node, "name", "")
                                        contentNameLc = genericController.vbLCase(ContentName)
                                        If ContentName = "" Then
                                            Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "collection file contains a CDEF node with no name attribute. This is not allowed.", "dll", "builderClass", "UpgradeCDef_LoadDataToCollection", 0, "", "", False, True, "")
                                        Else
                                            '
                                            ' setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
                                            '
                                            If defaultCollection.CDef.ContainsKey(contentNameLc) Then
                                                DefaultCDef = defaultCollection.CDef(contentNameLc)
                                            Else
                                                DefaultCDef = New Models.Complex.cdefModel
                                            End If
                                            '
                                            ContentTableName = GetXMLAttribute(cpCore, Found, CDef_Node, "ContentTableName", DefaultCDef.ContentTableName)
                                            If (ContentTableName <> "") Then
                                                '
                                                ' These two fields are needed to import the row
                                                '
                                                DataSourceName = GetXMLAttribute(cpCore, Found, CDef_Node, "dataSource", DefaultCDef.ContentDataSourceName)
                                                If DataSourceName = "" Then
                                                    DataSourceName = "Default"
                                                End If
                                                '
                                                ' ----- Add CDef if not already there
                                                '
                                                If Not result.CDef.ContainsKey(ContentName.ToLower) Then
                                                    result.CDef.Add(ContentName.ToLower, New Models.Complex.cdefModel())
                                                End If
                                                '
                                                ' Get CDef attributes
                                                '
                                                With result.CDef(ContentName.ToLower)
                                                    Dim activeDefaultText As String = "1"
                                                    If Not (DefaultCDef.Active) Then activeDefaultText = "0"
                                                    ActiveText = GetXMLAttribute(cpCore, Found, CDef_Node, "Active", activeDefaultText)
                                                    If ActiveText = "" Then
                                                        ActiveText = "1"
                                                    End If
                                                    .Active = genericController.EncodeBoolean(ActiveText)
                                                    .ActiveOnly = True
                                                    '.adminColumns = ?
                                                    .AdminOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AdminOnly", DefaultCDef.AdminOnly)
                                                    .AliasID = "id"
                                                    .AliasName = "name"
                                                    .AllowAdd = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowAdd", DefaultCDef.AllowAdd)
                                                    .AllowCalendarEvents = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowCalendarEvents", DefaultCDef.AllowCalendarEvents)
                                                    .AllowContentChildTool = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowContentChildTool", DefaultCDef.AllowContentChildTool)
                                                    .AllowContentTracking = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowContentTracking", DefaultCDef.AllowContentTracking)
                                                    .AllowDelete = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowDelete", DefaultCDef.AllowDelete)
                                                    .AllowTopicRules = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AllowTopicRules", DefaultCDef.AllowTopicRules)
                                                    .guid = GetXMLAttribute(cpCore, Found, CDef_Node, "guid", DefaultCDef.guid)
                                                    .dataChanged = setAllDataChanged
                                                    .childIdList(cpCore) = New List(Of Integer)
                                                    .ContentControlCriteria = ""
                                                    .ContentDataSourceName = GetXMLAttribute(cpCore, Found, CDef_Node, "ContentDataSourceName", DefaultCDef.ContentDataSourceName)
                                                    .ContentTableName = GetXMLAttribute(cpCore, Found, CDef_Node, "ContentTableName", DefaultCDef.ContentTableName)
                                                    .dataSourceId = 0
                                                    .DefaultSortMethod = GetXMLAttribute(cpCore, Found, CDef_Node, "DefaultSortMethod", DefaultCDef.DefaultSortMethod)
                                                    If (.DefaultSortMethod = "") Or (LCase(.DefaultSortMethod) = "name") Then
                                                        .DefaultSortMethod = "By Name"
                                                    ElseIf genericController.vbLCase(.DefaultSortMethod) = "sortorder" Then
                                                        .DefaultSortMethod = "By Alpha Sort Order Field"
                                                    ElseIf genericController.vbLCase(.DefaultSortMethod) = "date" Then
                                                        .DefaultSortMethod = "By Date"
                                                    End If
                                                    .DeveloperOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "DeveloperOnly", DefaultCDef.DeveloperOnly)
                                                    .DropDownFieldList = GetXMLAttribute(cpCore, Found, CDef_Node, "DropDownFieldList", DefaultCDef.DropDownFieldList)
                                                    .EditorGroupName = GetXMLAttribute(cpCore, Found, CDef_Node, "EditorGroupName", DefaultCDef.EditorGroupName)
                                                    .fields = New Dictionary(Of String, Models.Complex.CDefFieldModel)
                                                    .IconLink = GetXMLAttribute(cpCore, Found, CDef_Node, "IconLink", DefaultCDef.IconLink)
                                                    .IconHeight = GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconHeight", DefaultCDef.IconHeight)
                                                    .IconWidth = GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconWidth", DefaultCDef.IconWidth)
                                                    .IconSprites = GetXMLAttributeInteger(cpCore, Found, CDef_Node, "IconSprites", DefaultCDef.IconSprites)
                                                    .IgnoreContentControl = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IgnoreContentControl", DefaultCDef.IgnoreContentControl)
                                                    .includesAFieldChange = False
                                                    .installedByCollectionGuid = GetXMLAttribute(cpCore, Found, CDef_Node, "installedByCollection", DefaultCDef.installedByCollectionGuid)
                                                    .IsBaseContent = IsccBaseFile Or GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IsBaseContent", False)
                                                    .IsModifiedSinceInstalled = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "IsModified", DefaultCDef.IsModifiedSinceInstalled)
                                                    .Name = ContentName
                                                    .parentName = GetXMLAttribute(cpCore, Found, CDef_Node, "Parent", DefaultCDef.parentName)
                                                    .WhereClause = GetXMLAttribute(cpCore, Found, CDef_Node, "WhereClause", DefaultCDef.WhereClause)
                                                End With
                                                '
                                                ' Get CDef field nodes
                                                '
                                                For Each CDefChildNode In CDef_Node.ChildNodes
                                                    '
                                                    ' ----- process CDef Field
                                                    '
                                                    If TextMatch(cpCore, CDefChildNode.Name, "field") Then
                                                        FieldName = GetXMLAttribute(cpCore, Found, CDefChildNode, "Name", "")
                                                        If FieldName.ToLower = "middlename" Then
                                                            FieldName = FieldName
                                                        End If
                                                        '
                                                        ' try to find field in the defaultcdef
                                                        '
                                                        If (DefaultCDef.fields.ContainsKey(FieldName)) Then
                                                            DefaultCDefField = DefaultCDef.fields(FieldName)
                                                        Else
                                                            DefaultCDefField = New Models.Complex.CDefFieldModel()
                                                        End If
                                                        '
                                                        If Not result.CDef(ContentName.ToLower).fields.ContainsKey(FieldName.ToLower()) Then
                                                            result.CDef(ContentName.ToLower).fields.Add(FieldName.ToLower, New Models.Complex.CDefFieldModel)
                                                        End If
                                                        With result.CDef(ContentName.ToLower).fields(FieldName.ToLower)
                                                            .nameLc = FieldName.ToLower
                                                            ActiveText = "0"
                                                            If DefaultCDefField.active Then
                                                                ActiveText = "1"
                                                            End If
                                                            ActiveText = GetXMLAttribute(cpCore, Found, CDefChildNode, "Active", ActiveText)
                                                            If ActiveText = "" Then
                                                                ActiveText = "1"
                                                            End If
                                                            .active = genericController.EncodeBoolean(ActiveText)
                                                            '
                                                            ' Convert Field Descriptor (text) to field type (integer)
                                                            '
                                                            Dim defaultFieldTypeName As String = cpCore.db.getFieldTypeNameFromFieldTypeId(DefaultCDefField.fieldTypeId)
                                                            Dim fieldTypeName As String = GetXMLAttribute(cpCore, Found, CDefChildNode, "FieldType", defaultFieldTypeName)
                                                            .fieldTypeId = cpCore.db.getFieldTypeIdFromFieldTypeName(fieldTypeName)
                                                            'FieldTypeDescriptor = GetXMLAttribute(cpcore,Found, CDefChildNode, "FieldType", DefaultCDefField.fieldType)
                                                            'If genericController.vbIsNumeric(FieldTypeDescriptor) Then
                                                            '    .fieldType = genericController.EncodeInteger(FieldTypeDescriptor)
                                                            'Else
                                                            '    .fieldType = cpCore.app.csv_GetFieldTypeByDescriptor(FieldTypeDescriptor)
                                                            'End If
                                                            'If .fieldType = 0 Then
                                                            '    .fieldType = FieldTypeText
                                                            'End If
                                                            .editSortPriority = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "EditSortPriority", DefaultCDefField.editSortPriority)
                                                            .authorable = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Authorable", DefaultCDefField.authorable)
                                                            .caption = GetXMLAttribute(cpCore, Found, CDefChildNode, "Caption", DefaultCDefField.caption)
                                                            .defaultValue = GetXMLAttribute(cpCore, Found, CDefChildNode, "DefaultValue", DefaultCDefField.defaultValue)
                                                            .NotEditable = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "NotEditable", DefaultCDefField.NotEditable)
                                                            .indexColumn = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexColumn", DefaultCDefField.indexColumn)
                                                            .indexWidth = GetXMLAttribute(cpCore, Found, CDefChildNode, "IndexWidth", DefaultCDefField.indexWidth)
                                                            .indexSortOrder = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexSortOrder", DefaultCDefField.indexSortOrder)
                                                            .RedirectID = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectID", DefaultCDefField.RedirectID)
                                                            .RedirectPath = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectPath", DefaultCDefField.RedirectPath)
                                                            .htmlContent = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "HTMLContent", DefaultCDefField.htmlContent)
                                                            .UniqueName = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "UniqueName", DefaultCDefField.UniqueName)
                                                            .Password = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Password", DefaultCDefField.Password)
                                                            .adminOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "AdminOnly", DefaultCDefField.adminOnly)
                                                            .developerOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "DeveloperOnly", DefaultCDefField.developerOnly)
                                                            .ReadOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "ReadOnly", DefaultCDefField.ReadOnly)
                                                            .Required = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Required", DefaultCDefField.Required)
                                                            .RSSTitleField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSTitle", DefaultCDefField.RSSTitleField)
                                                            .RSSDescriptionField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSDescriptionField", DefaultCDefField.RSSDescriptionField)
                                                            .MemberSelectGroupID = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "MemberSelectGroupID", DefaultCDefField.MemberSelectGroupID)
                                                            .editTabName = GetXMLAttribute(cpCore, Found, CDefChildNode, "EditTab", DefaultCDefField.editTabName)
                                                            .Scramble = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Scramble", DefaultCDefField.Scramble)
                                                            .lookupList = GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupList", DefaultCDefField.lookupList)
                                                            .ManyToManyRulePrimaryField = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRulePrimaryField", DefaultCDefField.ManyToManyRulePrimaryField)
                                                            .ManyToManyRuleSecondaryField = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleSecondaryField", DefaultCDefField.ManyToManyRuleSecondaryField)
                                                            .lookupContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore))
                                                            ' isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                            ' case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                            ' case 2 - when the base file is loaded, all fields must include the attribute
                                                            'Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
                                                            .isBaseField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsBaseField", False) Or IsccBaseFile
                                                            .RedirectContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectContent", DefaultCDefField.RedirectContentName(cpCore))
                                                            .ManyToManyContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.ManyToManyContentName(cpCore))
                                                            .ManyToManyRuleContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.ManyToManyRuleContentName(cpCore))
                                                            .isModifiedSinceInstalled = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled)
                                                            .installedByCollectionGuid = GetXMLAttribute(cpCore, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid)
                                                            .dataChanged = setAllDataChanged
                                                            '
                                                            ' ----- handle child nodes (help node)
                                                            '
                                                            .HelpCustom = ""
                                                            .HelpDefault = ""
                                                            For Each FieldChildNode In CDefChildNode.ChildNodes
                                                                '
                                                                ' ----- process CDef Field
                                                                '
                                                                If TextMatch(cpCore, FieldChildNode.Name, "HelpDefault") Then
                                                                    .HelpDefault = FieldChildNode.InnerText
                                                                End If
                                                                If TextMatch(cpCore, FieldChildNode.Name, "HelpCustom") Then
                                                                    .HelpCustom = FieldChildNode.InnerText
                                                                End If
                                                                .HelpChanged = setAllDataChanged
                                                            Next
                                                        End With
                                                    End If
                                                Next
                                            End If
                                        End If
                                    Case "sqlindex"
                                        '
                                        ' SQL Indexes
                                        '
                                        With result
                                            IndexName = GetXMLAttribute(cpCore, Found, CDef_Node, "indexname", "")
                                            TableName = GetXMLAttribute(cpCore, Found, CDef_Node, "TableName", "")
                                            DataSourceName = GetXMLAttribute(cpCore, Found, CDef_Node, "DataSourceName", "")
                                            If DataSourceName = "" Then
                                                DataSourceName = "default"
                                            End If
                                            If .SQLIndexCnt > 0 Then
                                                For Ptr = 0 To .SQLIndexCnt - 1
                                                    If TextMatch(cpCore, .SQLIndexes(Ptr).IndexName, IndexName) And TextMatch(cpCore, .SQLIndexes(Ptr).TableName, TableName) And TextMatch(cpCore, .SQLIndexes(Ptr).DataSourceName, DataSourceName) Then
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            If Ptr >= .SQLIndexCnt Then
                                                Ptr = .SQLIndexCnt
                                                .SQLIndexCnt = .SQLIndexCnt + 1
                                                ReDim Preserve .SQLIndexes(Ptr)
                                                .SQLIndexes(Ptr).IndexName = IndexName
                                                .SQLIndexes(Ptr).TableName = TableName
                                                .SQLIndexes(Ptr).DataSourceName = DataSourceName
                                            End If
                                            With .SQLIndexes(Ptr)
                                                .FieldNameList = GetXMLAttribute(cpCore, Found, CDef_Node, "FieldNameList", "")
                                            End With
                                        End With
                                    Case "adminmenu", "menuentry", "navigatorentry"

                                        '
                                        ' Admin Menus / Navigator Entries
                                        '
                                        MenuName = GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "")
                                        menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_Node, "NameSpace", "")
                                        MenuGuid = GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "")
                                        IsNavigator = (NodeName = "navigatorentry")
                                        '
                                        ' Set MenuKey to what we will expect to find in the .guid
                                        '
                                        ' make a local out of getdatabuildversion
                                        '
                                        If Not IsNavigator Then
                                            MenuKey = genericController.vbLCase(MenuName)
                                        ElseIf False Then
                                            MenuKey = genericController.vbLCase("nav." & menuNameSpace & "." & MenuName)
                                        Else
                                            MenuKey = MenuGuid
                                        End If
                                        With result
                                            '
                                            ' Go through all current menus and check for duplicates
                                            '
                                            If .MenuCnt > 0 Then
                                                For Ptr = 0 To .MenuCnt - 1
                                                    ' 1/16/2009 - JK - empty keys should not be allowed
                                                    If .Menus(Ptr).Key <> "" Then
                                                        If TextMatch(cpCore, .Menus(Ptr).Key, MenuKey) Then
                                                            Exit For
                                                        End If
                                                    End If
                                                Next
                                            End If
                                            If Ptr >= .MenuCnt Then
                                                '
                                                ' Add new entry
                                                '
                                                Ptr = .MenuCnt
                                                .MenuCnt = .MenuCnt + 1
                                                ReDim Preserve .Menus(Ptr)
                                                '.Menus(Ptr).Name = MenuName
                                            End If
                                            With .Menus(Ptr)
                                                ActiveText = GetXMLAttribute(cpCore, Found, CDef_Node, "Active", "1")
                                                If ActiveText = "" Then
                                                    ActiveText = "1"
                                                End If
                                                '
                                                ' Update Entry
                                                '
                                                .dataChanged = setAllDataChanged
                                                .Name = MenuName
                                                .Guid = MenuGuid
                                                .Key = MenuKey
                                                .Active = genericController.EncodeBoolean(ActiveText)
                                                .menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_Node, "NameSpace", "")
                                                .ParentName = GetXMLAttribute(cpCore, Found, CDef_Node, "ParentName", "")
                                                .ContentName = GetXMLAttribute(cpCore, Found, CDef_Node, "ContentName", "")
                                                .LinkPage = GetXMLAttribute(cpCore, Found, CDef_Node, "LinkPage", "")
                                                .SortOrder = GetXMLAttribute(cpCore, Found, CDef_Node, "SortOrder", "")
                                                .AdminOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "AdminOnly", False)
                                                .DeveloperOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "DeveloperOnly", False)
                                                .NewWindow = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "NewWindow", False)
                                                .AddonName = GetXMLAttribute(cpCore, Found, CDef_Node, "AddonName", "")
                                                .NavIconType = GetXMLAttribute(cpCore, Found, CDef_Node, "NavIconType", "")
                                                .NavIconTitle = GetXMLAttribute(cpCore, Found, CDef_Node, "NavIconTitle", "")
                                                .IsNavigator = IsNavigator
                                            End With
                                        End With
                                    Case "aggregatefunction", "addon"
                                        '
                                        ' Aggregate Objects (just make them -- there are not too many
                                        '
                                        Name = GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "")
                                        With result
                                            If .AddOnCnt > 0 Then
                                                For Ptr = 0 To .AddOnCnt - 1
                                                    If TextMatch(cpCore, .AddOns(Ptr).Name, Name) Then
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            If Ptr >= .AddOnCnt Then
                                                Ptr = .AddOnCnt
                                                .AddOnCnt = .AddOnCnt + 1
                                                ReDim Preserve .AddOns(Ptr)
                                                .AddOns(Ptr).Name = Name
                                            End If
                                            With .AddOns(Ptr)
                                                .dataChanged = setAllDataChanged
                                                .Link = GetXMLAttribute(cpCore, Found, CDef_Node, "Link", "")
                                                .ObjectProgramID = GetXMLAttribute(cpCore, Found, CDef_Node, "ObjectProgramID", "")
                                                .ArgumentList = GetXMLAttribute(cpCore, Found, CDef_Node, "ArgumentList", "")
                                                .SortOrder = GetXMLAttribute(cpCore, Found, CDef_Node, "SortOrder", "")
                                                .Copy = GetXMLAttribute(cpCore, Found, CDef_Node, "copy", "")
                                            End With
                                            .AddOns(Ptr).Copy = CDef_Node.InnerText
                                        End With
                                    Case "style"
                                        '
                                        ' style sheet entries
                                        '
                                        Name = GetXMLAttribute(cpCore, Found, CDef_Node, "Name", "")
                                        With result
                                            If .StyleCnt > 0 Then
                                                For Ptr = 0 To .StyleCnt - 1
                                                    If TextMatch(cpCore, .Styles(Ptr).Name, Name) Then
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            If Ptr >= .StyleCnt Then
                                                Ptr = .StyleCnt
                                                .StyleCnt = .StyleCnt + 1
                                                ReDim Preserve .Styles(Ptr)
                                                .Styles(Ptr).Name = Name
                                            End If
                                            With .Styles(Ptr)
                                                .dataChanged = setAllDataChanged
                                                .Overwrite = GetXMLAttributeBoolean(cpCore, Found, CDef_Node, "Overwrite", False)
                                                .Copy = CDef_Node.InnerText
                                            End With
                                        End With
                                    Case "stylesheet"
                                        '
                                        ' style sheet in one entry
                                        '
                                        result.StyleSheet = CDef_Node.InnerText
                                    Case "getcollection", "importcollection"
                                        If True Then
                                            'If Not UpgradeDbOnly Then
                                            '
                                            ' Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                            '
                                            Collectionname = GetXMLAttribute(cpCore, Found, CDef_Node, "name", "")
                                            CollectionGuid = GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "")
                                            If CollectionGuid = "" Then
                                                CollectionGuid = CDef_Node.InnerText
                                            End If
                                            If CollectionGuid = "" Then
                                                status = "The collection you selected [" & Collectionname & "] can not be downloaded because it does not include a valid GUID."
                                                'cpCore.AppendLog("builderClass.UpgradeCDef_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
                                            Else
                                                ReDim Preserve result.collectionImports(result.ImportCnt)
                                                result.collectionImports(result.ImportCnt).Guid = CollectionGuid
                                                result.collectionImports(result.ImportCnt).Name = Collectionname
                                                result.ImportCnt = result.ImportCnt + 1
                                            End If
                                        End If
                                    Case "pagetemplate"
                                        '
                                        '-------------------------------------------------------------------------------------------------
                                        ' Page Templates
                                        '-------------------------------------------------------------------------------------------------
                                        ' *********************************************************************************
                                        ' Page Template - started, but Return_Collection and LoadDataToCDef are all that is done do far
                                        '
                                        With result
                                            If .PageTemplateCnt > 0 Then
                                                For Ptr = 0 To .PageTemplateCnt - 1
                                                    If TextMatch(cpCore, .PageTemplates(Ptr).Name, Name) Then
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                            If Ptr >= .PageTemplateCnt Then
                                                Ptr = .PageTemplateCnt
                                                .PageTemplateCnt = .PageTemplateCnt + 1
                                                ReDim Preserve .PageTemplates(Ptr)
                                                .PageTemplates(Ptr).Name = Name
                                            End If
                                            With .PageTemplates(Ptr)
                                                .Copy = GetXMLAttribute(cpCore, Found, CDef_Node, "Copy", "")
                                                .Guid = GetXMLAttribute(cpCore, Found, CDef_Node, "guid", "")
                                                .Style = GetXMLAttribute(cpCore, Found, CDef_Node, "style", "")
                                            End With
                                        End With
                                    'Case "sitesection"
                                    '    '
                                    '    '-------------------------------------------------------------------------------------------------
                                    '    ' Site Sections
                                    '    '-------------------------------------------------------------------------------------------------
                                    '    '
                                    'Case "dynamicmenu"
                                    '    '
                                    '    '-------------------------------------------------------------------------------------------------
                                    '    ' Dynamic Menus
                                    '    '-------------------------------------------------------------------------------------------------
                                    '    '
                                    Case "pagecontent"
                                        '
                                        '-------------------------------------------------------------------------------------------------
                                        ' Page Content
                                        '-------------------------------------------------------------------------------------------------
                                        '
                                    Case "copycontent"
                                        '
                                        '-------------------------------------------------------------------------------------------------
                                        ' Copy Content
                                        '-------------------------------------------------------------------------------------------------
                                        '
                                End Select
                            Next
                            'hint = "nodes done"
                            '
                            ' Convert Menus.ParentName to Menu.menuNameSpace
                            '
                            With result
                                If .MenuCnt > 0 Then
                                    For Ptr = 0 To .MenuCnt - 1
                                        If .Menus(Ptr).ParentName <> "" Then
                                            .Menus(Ptr).menuNameSpace = GetMenuNameSpace(cpCore, result, Ptr, .Menus(Ptr).IsNavigator, "")
                                            '.Menus(Ptr).ParentName = ""
                                            Ptr = Ptr
                                        End If
                                    Next
                                End If
                            End With
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
        ''' </summary>
        ''' <param name="Collection"></param>
        ''' <param name="return_IISResetRequired"></param>
        ''' <param name="BuildVersion"></param>
        Private Shared Sub installCollection_BuildDbFromMiniCollection(cpCore As coreClass, ByVal Collection As miniCollectionModel, ByVal BuildVersion As String, isNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String))
            Try
                '
                Dim FieldHelpID As Integer
                Dim FieldHelpCID As Integer
                Dim fieldId As Integer
                Dim FieldName As String
                'Dim AddonClass As addonInstallClass
                Dim StyleSheetAdd As String = String.Empty
                Dim NewStyleValue As String
                Dim SiteStyles As String
                Dim PosNameLineEnd As Integer
                Dim PosNameLineStart As Integer
                Dim SiteStylePtr As Integer
                Dim StyleLine As String
                Dim SiteStyleSplit As String() = {}
                Dim SiteStyleCnt As Integer
                Dim NewStyleName As String
                Dim TestStyleName As String
                Dim SQL As String
                Dim rs As DataTable
                Dim Copy As String
                Dim ContentName As String
                Dim NodeCount As Integer
                Dim TableName As String
                Dim RequireReload As Boolean
                Dim Found As Boolean
                ' Dim builder As New coreBuilderClass(cpCore)
                Dim InstallCollectionList As String = ""                 'Collections to Install when upgrade is complete
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name & ", UpgradeCDef_BuildDbFromCollection")
                '
                '----------------------------------------------------------------------------------------------------------------------
                Call logController.appendInstallLog(cpCore, "CDef Load, stage 0.5: verify core sql tables")
                '----------------------------------------------------------------------------------------------------------------------
                '
                Call appBuilderController.VerifyBasicTables(cpCore)
                '
                '----------------------------------------------------------------------------------------------------------------------
                Call logController.appendInstallLog(cpCore, "CDef Load, stage 1: create SQL tables in default datasource")
                '----------------------------------------------------------------------------------------------------------------------
                '
                With Collection
                    If True Then
                        Dim UsedTables As String = ""
                        For Each keypairvalue In .CDef
                            Dim workingCdef As Models.Complex.cdefModel = keypairvalue.Value
                            ContentName = workingCdef.Name
                            With workingCdef
                                If .dataChanged Then
                                    Call logController.appendInstallLog(cpCore, "creating sql table [" & .ContentTableName & "], datasource [" & .ContentDataSourceName & "]")
                                    If genericController.vbLCase(.ContentDataSourceName) = "default" Or .ContentDataSourceName = "" Then
                                        TableName = .ContentTableName
                                        If genericController.vbInstr(1, "," & UsedTables & ",", "," & TableName & ",", vbTextCompare) <> 0 Then
                                            TableName = TableName
                                        Else
                                            UsedTables = UsedTables & "," & TableName
                                            Call cpCore.db.createSQLTable(.ContentDataSourceName, TableName)
                                        End If
                                    End If
                                End If
                            End With
                        Next
                        cpCore.doc.clearMetaData()
                        cpCore.cache.invalidateAll()
                    End If
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 2: Verify all CDef names in ccContent so GetContentID calls will succeed")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    NodeCount = 0
                    Dim installedContentList As New List(Of String)
                    rs = cpCore.db.executeQuery("SELECT Name from ccContent where (active<>0)")
                    If isDataTableOk(rs) Then
                        installedContentList = New List(Of String)(convertDataTableColumntoItemList(rs))
                    End If
                    rs.Dispose()
                    '
                    For Each keypairvalue In .CDef
                        With keypairvalue.Value
                            If .dataChanged Then
                                Call logController.appendInstallLog(cpCore, "adding cdef name [" & .Name & "]")
                                If (Not installedContentList.Contains(.Name.ToLower())) Then
                                    SQL = "Insert into ccContent (name,ccguid,active,createkey)values(" & cpCore.db.encodeSQLText(.Name) & "," & cpCore.db.encodeSQLText(.guid) & ",1,0);"
                                    Call cpCore.db.executeQuery(SQL)
                                    installedContentList.Add(.Name.ToLower())
                                    RequireReload = True
                                End If
                            End If
                        End With
                    Next
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 4: Verify content records required for Content Server")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    Call VerifySortMethods(cpCore)
                    Call VerifyContentFieldTypes(cpCore)
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 5: verify 'Content' content definition")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    For Each keypairvalue In .CDef
                        With keypairvalue.Value
                            If .Name.ToLower() = "content" Then
                                Call logController.appendInstallLog(cpCore, "adding cdef [" & .Name & "]")
                                Call installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion)
                                RequireReload = True
                                Exit For
                            End If
                        End With
                    Next
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 6.1: Verify all definitions and fields")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    RequireReload = False
                    For Each keypairvalue In .CDef
                        With keypairvalue.Value
                            '
                            ' todo tmp fix, changes to field caption in base.xml do not set fieldChange
                            If (True) Then ' If .dataChanged Or .includesAFieldChange Then
                                If (.Name.ToLower() <> "content") Then
                                    Call logController.appendInstallLog(cpCore, "adding cdef [" & .Name & "]")
                                    Call installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion)
                                    RequireReload = True
                                End If
                            End If
                        End With
                    Next
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 6.2: Verify all field help")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    FieldHelpCID = cpCore.db.getRecordID("content", "Content Field Help")
                    For Each keypairvalue In .CDef
                        Dim workingCdef As Models.Complex.cdefModel = keypairvalue.Value
                        ContentName = workingCdef.Name
                        For Each fieldKeyValuePair In workingCdef.fields
                            Dim field As Models.Complex.CDefFieldModel = fieldKeyValuePair.Value
                            FieldName = field.nameLc
                            With .CDef(ContentName.ToLower).fields(FieldName.ToLower)
                                If .HelpChanged Then
                                    fieldId = 0
                                    SQL = "select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" & cpCore.db.encodeSQLText(FieldName) & ")and(c.name=" & cpCore.db.encodeSQLText(ContentName) & ") order by f.id"
                                    rs = cpCore.db.executeQuery(SQL)
                                    If isDataTableOk(rs) Then
                                        fieldId = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "id"))
                                    End If
                                    rs.Dispose()
                                    If fieldId = 0 Then
                                        Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not update help field for content [" & ContentName & "], field [" & FieldName & "] because the field was not found in the Db.", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection", 0, "", "", False, True, "")
                                    Else
                                        SQL = "select id from ccfieldhelp where fieldid=" & fieldId & " order by id"
                                        rs = cpCore.db.executeQuery(SQL)
                                        If isDataTableOk(rs) Then
                                            FieldHelpID = genericController.EncodeInteger(rs.Rows(0).Item("id"))
                                        Else
                                            FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccfieldhelp", 0)
                                        End If
                                        rs.Dispose()
                                        If FieldHelpID <> 0 Then
                                            Copy = .HelpCustom
                                            If Copy = "" Then
                                                Copy = .HelpDefault
                                                If Copy <> "" Then
                                                    Copy = Copy
                                                End If
                                            End If
                                            SQL = "update ccfieldhelp set active=1,contentcontrolid=" & FieldHelpCID & ",fieldid=" & fieldId & ",helpdefault=" & cpCore.db.encodeSQLText(Copy) & " where id=" & FieldHelpID
                                            Call cpCore.db.executeQuery(SQL)
                                        End If
                                    End If
                                End If
                            End With
                        Next
                    Next
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 7: create SQL indexes")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    For Ptr = 0 To .SQLIndexCnt - 1
                        With .SQLIndexes(Ptr)
                            If .dataChanged Then
                                Call logController.appendInstallLog(cpCore, "creating index [" & .IndexName & "], fields [" & .FieldNameList & "], on table [" & .TableName & "]")
                                '
                                ' stop the errors here, so a bad field does not block the upgrade
                                '
                                'On Error Resume Next
                                Call cpCore.db.createSQLIndex(.DataSourceName, .TableName, .IndexName, .FieldNameList)
                            End If
                        End With
                    Next
                    cpCore.doc.clearMetaData()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 8a: Verify All Menu Names, then all Menus")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    For Ptr = 0 To .MenuCnt - 1
                        With .Menus(Ptr)
                            If Ptr = 140 Then
                                Ptr = Ptr
                            End If
                            If genericController.vbLCase(.Name) = "manage add-ons" And .IsNavigator Then
                                .Name = .Name
                            End If
                            If .dataChanged Then
                                Call logController.appendInstallLog(cpCore, "creating navigator entry [" & .Name & "], namespace [" & .menuNameSpace & "], guid [" & .Guid & "]")
                                Call appBuilderController.verifyNavigatorEntry(cpCore, .Guid, .menuNameSpace, .Name, .ContentName, .LinkPage, .SortOrder, .AdminOnly, .DeveloperOnly, .NewWindow, .Active, .AddonName, .NavIconType, .NavIconTitle, 0)
                                'If .IsNavigator Then
                                'Else
                                '    ContentName = cnNavigatorEntries
                                '    Call logcontroller.appendInstallLog(cpCore,  "creating menu entry [" & .Name & "], parentname [" & .ParentName & "]")
                                '    Call Controllers.appBuilderController.admin_VerifyMenuEntry(cpCore, .ParentName, .Name, .ContentName, .LinkPage, .SortOrder, .AdminOnly, .DeveloperOnly, .NewWindow, .Active, ContentName, .AddonName)
                                'End If
                            End If
                        End With
                    Next
                    ' 20160710 - this is old code (aggregatefunctions, etc are not in cdef anymore. Use the CollectionX methods to install addons
                    ''
                    ''----------------------------------------------------------------------------------------------------------------------
                    'Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "CDef Load, stage 8c: Verify Add-ons")
                    ''----------------------------------------------------------------------------------------------------------------------
                    ''
                    'NodeCount = 0
                    'If .AddOnCnt > 0 Then
                    '    For Ptr = 0 To .AddOnCnt - 1
                    '        With .AddOns(Ptr)
                    '            If .dataChanged Then
                    '                Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "creating add-on [" & .Name & "]")
                    '                'Dim 
                    '                'InstallCollectionFromLocalRepo_addonNode_Phase1(cpcore,"crap - this takes an xml node and I have a collection object...")
                    '                If .Link <> "" Then
                    '                    Call csv_VerifyAggregateScript(.Name, .Link, .ArgumentList, .SortOrder)
                    '                ElseIf .ObjectProgramID <> "" Then
                    '                    Call csv_VerifyAggregateObject(.Name, .ObjectProgramID, .ArgumentList, .SortOrder)
                    '                Else
                    '                    Call csv_VerifyAggregateReplacement2(.Name, .Copy, .ArgumentList, .SortOrder)
                    '                End If
                    '            End If
                    '        End With
                    '    Next
                    'End If
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 8d: Verify Import Collections")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    If Collection.ImportCnt > 0 Then
                        'AddonClass = New addonInstallClass(cpCore)
                        For Ptr = 0 To Collection.ImportCnt - 1
                            InstallCollectionList = InstallCollectionList & "," & Collection.collectionImports(Ptr).Guid
                        Next
                    End If
                    '
                    '---------------------------------------------------------------------
                    ' ----- Upgrade collections added during upgrade process
                    '---------------------------------------------------------------------
                    '
                    Dim errorMessage As String = ""
                    Dim Guids As String()
                    Dim Guid As String
                    Dim CollectionPath As String = ""
                    Dim lastChangeDate As New Date
                    Dim ignoreRefactor As Boolean = False
                    Call logController.appendInstallLog(cpCore, "Installing Add-on Collections gathered during upgrade")
                    If InstallCollectionList = "" Then
                        Call logController.appendInstallLog(cpCore, "No Add-on collections added during upgrade")
                    Else
                        errorMessage = ""
                        Guids = Split(InstallCollectionList, ",")
                        For Ptr = 0 To UBound(Guids)
                            errorMessage = ""
                            Guid = Guids(Ptr)
                            If Guid <> "" Then
                                Call GetCollectionConfig(cpCore, Guid, CollectionPath, lastChangeDate, "")
                                If CollectionPath <> "" Then
                                    '
                                    ' This collection is installed locally, install from local collections
                                    '
                                    Call installCollectionFromLocalRepo(cpCore, Guid, cpCore.codeVersion, errorMessage, "", isNewBuild, nonCriticalErrorList)
                                Else
                                    '
                                    ' This is a new collection, install to the server and force it on this site
                                    '
                                    Dim addonInstallOk As Boolean
                                    addonInstallOk = installCollectionFromRemoteRepo(cpCore, Guid, errorMessage, "", isNewBuild, nonCriticalErrorList)
                                    If Not addonInstallOk Then
                                        Throw (New ApplicationException("Failure to install addon collection from remote repository. Collection [" & Guid & "] was referenced in collection [" & Collection.name & "]")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error upgrading Addon Collection [" & Guid & "], " & errorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                                    End If

                                End If
                            End If
                        Next
                    End If
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 9: Verify Styles")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    NodeCount = 0
                    If .StyleCnt > 0 Then
                        SiteStyles = cpCore.cdnFiles.readFile("templates/styles.css")
                        If Trim(SiteStyles) <> "" Then
                            '
                            ' Split with an extra character at the end to guarantee there is an extra split at the end
                            '
                            SiteStyleSplit = Split(SiteStyles & " ", "}")
                            SiteStyleCnt = UBound(SiteStyleSplit) + 1
                        End If
                        For Ptr = 0 To .StyleCnt - 1
                            Found = False
                            With .Styles(Ptr)
                                If .dataChanged Then
                                    NewStyleName = .Name
                                    NewStyleValue = .Copy
                                    NewStyleValue = genericController.vbReplace(NewStyleValue, "}", "")
                                    NewStyleValue = genericController.vbReplace(NewStyleValue, "{", "")
                                    If SiteStyleCnt > 0 Then
                                        For SiteStylePtr = 0 To SiteStyleCnt - 1
                                            StyleLine = SiteStyleSplit(SiteStylePtr)
                                            PosNameLineEnd = InStrRev(StyleLine, "{")
                                            If PosNameLineEnd > 0 Then
                                                PosNameLineStart = InStrRev(StyleLine, vbCrLf, PosNameLineEnd)
                                                If PosNameLineStart > 0 Then
                                                    '
                                                    ' Check this site style for a match with the NewStyleName
                                                    '
                                                    PosNameLineStart = PosNameLineStart + 2
                                                    TestStyleName = Trim(Mid(StyleLine, PosNameLineStart, PosNameLineEnd - PosNameLineStart))
                                                    If genericController.vbLCase(TestStyleName) = genericController.vbLCase(NewStyleName) Then
                                                        Found = True
                                                        If .Overwrite Then
                                                            '
                                                            ' Found - Update style
                                                            '
                                                            SiteStyleSplit(SiteStylePtr) = vbCrLf & .Name & " {" & NewStyleValue
                                                        End If
                                                        Exit For
                                                    End If
                                                End If
                                            End If
                                        Next
                                    End If
                                    '
                                    ' Add or update the stylesheet
                                    '
                                    If Not Found Then
                                        StyleSheetAdd = StyleSheetAdd & vbCrLf & NewStyleName & " {" & NewStyleValue & "}"
                                    End If
                                End If
                            End With
                        Next
                        SiteStyles = Join(SiteStyleSplit, "}")
                        If StyleSheetAdd <> "" Then
                            SiteStyles = SiteStyles _
                            & vbCrLf _
                            & vbCrLf & "/*" _
                            & vbCrLf & "Styles added " & Now() _
                            & vbCrLf & "*/" _
                            & vbCrLf & StyleSheetAdd
                        End If
                        Call cpCore.appRootFiles.saveFile("templates/styles.css", SiteStyles)
                        '
                        ' Update stylesheet cache
                        '
                        Call cpCore.siteProperties.setProperty("StylesheetSerialNumber", "-1")
                    End If
                    '
                    '-------------------------------------------------------------------------------------------------
                    ' Page Templates
                    '-------------------------------------------------------------------------------------------------
                    '
                    '
                    '-------------------------------------------------------------------------------------------------
                    ' Site Sections
                    '-------------------------------------------------------------------------------------------------
                    '
                    '
                    '-------------------------------------------------------------------------------------------------
                    ' Dynamic Menus
                    '-------------------------------------------------------------------------------------------------
                    '
                    '
                    '-------------------------------------------------------------------------------------------------
                    ' Page Content
                    '-------------------------------------------------------------------------------------------------
                    '
                    '
                    '-------------------------------------------------------------------------------------------------
                    ' Copy Content
                    '-------------------------------------------------------------------------------------------------
                    '
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' ----- Load the archive file application
        '========================================================================
        '
        Private Shared Sub installCollection_BuildDbFromCollection_AddCDefToDb(cpCore As coreClass, cdef As Models.Complex.cdefModel, ByVal BuildVersion As String)
            Try
                '
                Dim FieldHelpCID As Integer
                Dim FieldHelpID As Integer
                Dim fieldId As Integer
                Dim ContentID As Integer
                Dim rs As DataTable
                Dim EditorGroupID As Integer
                Dim FieldCount As Integer
                Dim FieldSize As Integer
                Dim ContentName As String
                'Dim DataSourceName As String
                Dim SQL As String
                Dim ContentIsBaseContent As Boolean
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name & ", UpgradeCDef_BuildDbFromCollection_AddCDefToDb")
                '
                If Not (False) Then
                    With cdef
                        '
                        Call logController.appendInstallLog(cpCore, "Upgrading CDef [" & .Name & "]")
                        '
                        ContentID = 0
                        ContentName = .Name
                        ContentIsBaseContent = False
                        FieldHelpCID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Help")
                        Dim datasource As Contensive.Core.Models.Entity.dataSourceModel = Models.Entity.dataSourceModel.createByName(cpCore, .ContentDataSourceName, New List(Of String))
                        '
                        ' get contentid and protect content with IsBaseContent true
                        '
                        SQL = cpCore.db.GetSQLSelect("default", "ccContent", "ID,IsBaseContent", "name=" & cpCore.db.encodeSQLText(ContentName), "ID", , 1)
                        rs = cpCore.db.executeQuery(SQL)
                        If (isDataTableOk(rs)) Then
                            If rs.Rows.Count > 0 Then
                                'EditorGroupID = cpcore.app.getDataRowColumnName(RS.rows(0), "ID")
                                ContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                                ContentIsBaseContent = genericController.EncodeBoolean(cpCore.db.getDataRowColumnName(rs.Rows(0), "IsBaseContent"))
                            End If
                        End If
                        rs.Dispose()
                        '
                        ' ----- Update Content Record
                        '
                        If .dataChanged Then
                            '
                            ' Content needs to be updated
                            '
                            If ContentIsBaseContent And Not .IsBaseContent Then
                                '
                                ' Can not update a base content with a non-base content
                                '
                                cpCore.handleException(New ApplicationException("Warning: An attempt was made to update Content Definition [" & .Name & "] from base to non-base. This should only happen when a base cdef is removed from the base collection. The update was ignored."))
                                .IsBaseContent = ContentIsBaseContent
                                'cpCore.handleLegacyError3( "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
                            End If
                            '
                            ' ----- update definition (use SingleRecord as an update flag)
                            '
                            Call Models.Complex.cdefModel.addContent(cpCore, True _
                                    , datasource _
                                    , .ContentTableName _
                                    , ContentName _
                                    , .AdminOnly _
                                    , .DeveloperOnly _
                                    , .AllowAdd _
                                    , .AllowDelete _
                                    , .parentName _
                                    , .DefaultSortMethod _
                                    , .DropDownFieldList _
                                    , False _
                                    , .AllowCalendarEvents _
                                    , .AllowContentTracking _
                                    , .AllowTopicRules _
                                    , .AllowContentChildTool _
                                    , False _
                                    , .IconLink _
                                    , .IconWidth _
                                    , .IconHeight _
                                    , .IconSprites _
                                    , .guid _
                                    , .IsBaseContent _
                                    , .installedByCollectionGuid
                                    )
                            If ContentID = 0 Then
                                Call logController.appendInstallLog(cpCore, "Could not determine contentid after createcontent3 for [" & ContentName & "], upgrade for this cdef aborted.")
                            Else
                                '
                                ' ----- Other fields not in the csv call
                                '
                                EditorGroupID = 0
                                If .EditorGroupName <> "" Then
                                    rs = cpCore.db.executeQuery("select ID from ccGroups where name=" & cpCore.db.encodeSQLText(.EditorGroupName))
                                    If (isDataTableOk(rs)) Then
                                        If rs.Rows.Count > 0 Then
                                            EditorGroupID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                                        End If
                                    End If
                                    rs.Dispose()
                                End If
                                SQL = "update ccContent" _
                                    & " set EditorGroupID=" & EditorGroupID _
                                    & ",isbasecontent=" & cpCore.db.encodeSQLBoolean(.IsBaseContent) _
                                    & " where id=" & ContentID _
                                    & ""
                                Call cpCore.db.executeQuery(SQL)
                            End If
                        End If
                        '
                        ' ----- update Content Field Records and Content Field Help records
                        '
                        If ContentID = 0 And (.fields.Count > 0) Then
                            '
                            ' CAn not add fields if there is no content record
                            '
                            Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not add field records to content [" & ContentName & "] because the content definition was not found", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
                        Else
                            '
                            '
                            '
                            FieldSize = 0
                            FieldCount = 0
                            For Each nameValuePair In .fields
                                Dim field As Models.Complex.CDefFieldModel = nameValuePair.Value
                                With field
                                    If (.dataChanged) Then
                                        fieldId = Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field)
                                    End If
                                    '
                                    ' ----- update content field help records
                                    '
                                    If (.HelpChanged) Then
                                        rs = cpCore.db.executeQuery("select ID from ccFieldHelp where fieldid=" & fieldId)
                                        If (isDataTableOk(rs)) Then
                                            If rs.Rows.Count > 0 Then
                                                FieldHelpID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"))
                                            End If
                                        End If
                                        rs.Dispose()
                                        '
                                        If FieldHelpID = 0 Then
                                            FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccFieldHelp", 0)
                                        End If
                                        If FieldHelpID <> 0 Then
                                            SQL = "update ccfieldhelp" _
                                                & " set fieldid=" & fieldId _
                                                & ",active=1" _
                                                & ",contentcontrolid=" & FieldHelpCID _
                                                & ",helpdefault=" & cpCore.db.encodeSQLText(.HelpDefault) _
                                                & ",helpcustom=" & cpCore.db.encodeSQLText(.HelpCustom) _
                                                & " where id=" & FieldHelpID
                                            Call cpCore.db.executeQuery(SQL)
                                        End If
                                    End If
                                End With
                            Next
                            ''
                            '' started doing something here -- research it.!!!!!
                            ''
                            'For FieldPtr = 0 To .fields.Count - 1
                            '    fieldId = 0
                            '    With .fields(FieldPtr)
                            '    End With
                            'Next
                            '
                            ' clear the cdef cache and list
                            '
                            cpCore.doc.clearMetaData()
                            cpCore.cache.invalidateAll()
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub






    End Class
End Namespace
