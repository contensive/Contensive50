
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
        'Private cpCore As coreClass
        ''
        '' not IDisposable - not contained classes that need to be disposed
        ''
        ''Private ManagerAddonNavID_Local As Integer
        ''
        'Public Sub New(cpCore As coreClass)
        '    MyBase.New()
        '    Me.cpCore = cpCore
        'End Sub
        ''
        ''====================================================================================================
        ''
        'Private Function GetNonRootNavigatorID(ByVal EntryName As String, ByVal ParentID As Integer, ByVal addonId As Integer, ByVal ContentID As Integer, ByVal NavIconType As Integer, ByVal NavIconTitle As String, ByVal DeveloperOnly As Boolean, ByVal ignore As Integer, ByVal LinkPage As String, ByVal HelpCollectionID As Integer, ByVal HelpAddonID As Integer, ByVal InstalledByCollectionID As Integer, ByVal AdminOnly As Boolean) As Integer
        '    Dim result As Integer = 0
        '    Try
        '        If ParentID <= 0 Then
        '            '
        '            ' Block entries to the root node - this is to block entries made for system collections I may have missed
        '            '
        '            Throw (New Exception("Adding root navigator entry [" & EntryName & "] by collection [" & cpCore.db.getRecordName("content", InstalledByCollectionID) & "]. This Is Not allowed."))
        '        Else
        '            result = GetNavigatorID(EntryName, ParentID, addonId, ContentID, NavIconType, NavIconTitle, DeveloperOnly, ignore, LinkPage, HelpCollectionID, HelpAddonID, InstalledByCollectionID, AdminOnly)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleException(ex) : Throw
        '    End Try
        '    Return result
        'End Function
        ''
        ''====================================================================================================
        ''
        'Private Function GetNavigatorID(ByVal EntryName As String, ByVal ParentID As Integer, ByVal addonId As Integer, ByVal ContentID As Integer, ByVal NavIconType As Integer, ByVal NavIconTitle As String, ByVal DeveloperOnly As Boolean, ByVal ignore As Integer, ByVal LinkPage As String, ByVal HelpCollectionID As Integer, ByVal HelpAddonID As Integer, ByVal InstalledByCollectionID As Integer, ByVal AdminOnly As Boolean) As Integer
        '    Dim result As Integer = 0
        '    Try
        '        Dim CS As Integer
        '        Dim Criteria As String
        '        ''dim buildversion As String
        '        '
        '        'BuildVersion = cpCore.app.DataBuildVersion_DontUseThis
        '        Criteria = "(Name = " & cpCore.db.encodeSQLText(EntryName) & ")"
        '        If ParentID = 0 Then
        '            Criteria = Criteria & "And((parentid=0)Or(parentid Is null))"
        '        Else
        '            Criteria = Criteria & "And(parentid=" & ParentID & ")"
        '        End If
        '        CS = cpCore.db.cs_open(cnNavigatorEntries, Criteria, "ID")
        '        If Not cpCore.db.cs_ok(CS) Then
        '            Call cpCore.db.cs_Close(CS)
        '            CS = cpCore.db.cs_insertRecord(cnNavigatorEntries, SystemMemberID)
        '            If cpCore.db.cs_ok(CS) Then
        '                'Call cmc.cpCore.app.csv_SetCSRecordDefaults(CS)
        '                Call cpCore.db.cs_set(CS, "name", EntryName)
        '                Call cpCore.db.cs_set(CS, "parentid", ParentID)
        '                Call cpCore.db.cs_set(CS, "AddonID", addonId)
        '                Call cpCore.db.cs_set(CS, "ContentID", ContentID)
        '                Call cpCore.db.cs_set(CS, "NavIconType", NavIconType)
        '                Call cpCore.db.cs_set(CS, "AdminOnly", AdminOnly)
        '                Call cpCore.db.cs_set(CS, "DeveloperOnly", DeveloperOnly)
        '                Call cpCore.db.cs_set(CS, "LinkPage", LinkPage)
        '                If True Then
        '                    Call cpCore.db.cs_set(CS, "HelpAddonID", HelpAddonID)
        '                    Call cpCore.db.cs_set(CS, "HelpCollectionID", HelpCollectionID)
        '                End If
        '                If True Then
        '                    Call cpCore.db.cs_set(CS, "InstalledByCollectionID", InstalledByCollectionID)
        '                End If
        '                '
        '                '
        '                ' set initial caps because some content definitions were not named with this in mind -- it looks bad
        '                '
        '                Call cpCore.db.cs_set(CS, "NavIconTitle", EncodeInitialCaps(NavIconTitle))
        '                '
        '                ' if there are more, these are a errors - move their child nodes to the first and delete them
        '                '
        '                Call cpCore.db.cs_goNext(CS)
        '                Do While cpCore.db.cs_ok(CS)
        '                    Call cpCore.db.executeSql("update ccmenuentries set parentid=" & "0" & " where parentid=" & cpCore.db.cs_getInteger(CS, "ID"))
        '                    Call cpCore.db.executeSql("delete from ccmenuentries where id=" & cpCore.db.cs_getInteger(CS, "ID"))
        '                    Call cpCore.db.cs_goNext(CS)
        '                Loop
        '            End If
        '        End If
        '        If cpCore.db.cs_ok(CS) Then
        '            result = cpCore.db.cs_getInteger(CS, "ID")
        '        End If
        '        Call cpCore.db.cs_Close(CS)
        '    Catch ex As Exception
        '        cpCore.handleException(ex) : Throw
        '    End Try
        '    Return result
        'End Function
        '
        '====================================================================================================
        '
        '   DownloadCollectionFiles
        '
        '   Download Library collection files into a folder
        '       Download Collection file and all attachments (DLLs) into working folder
        '       Unzips any collection files
        '       Returns true if it all downloads OK
        '
        Private Shared Function DownloadCollectionFiles(cpCore As coreClass, ByVal workingPath As String, ByVal CollectionGuid As String, ByRef return_CollectionLastChangeDate As Date, ByRef return_ErrorMessage As String) As Boolean
            DownloadCollectionFiles = False
            Try
                '
                Dim CollectionFileCnt As Integer
                Dim CollectionFilePath As String
                Dim Doc As New XmlDocument
                Dim URL As String
                Dim ResourceFilename As String
                Dim ResourceLink As String
                Dim CollectionVersion As String
                Dim CollectionFileLink As String
                Dim CollectionFile As New XmlDocument
                Dim Collectionname As String
                Dim Pos As Integer
                Dim UserError As String
                Dim CDefSection As XmlNode
                Dim CDefInterfaces As XmlNode
                Dim ActiveXNode As XmlNode
                Dim errorPrefix As String
                Dim downloadRetry As Integer
                Const downloadRetryMax As Integer = 3
                '
                Call logController.appendInstallLog(cpCore, "downloading collection [" & CollectionGuid & "]")
                '
                '---------------------------------------------------------------------------------------------------------------
                ' Request the Download file for this collection
                '---------------------------------------------------------------------------------------------------------------
                '
                Doc = New XmlDocument
                URL = "http://support.contensive.com/GetCollection?iv=" & cpCore.codeVersion() & "&guid=" & CollectionGuid
                errorPrefix = "DownloadCollectionFiles, Error reading the collection library status file from the server for Collection [" & CollectionGuid & "], download URL [" & URL & "]. "
                downloadRetry = 0
                Dim downloadDelay As Integer = 2000
                Do
                    Try
                        DownloadCollectionFiles = True
                        return_ErrorMessage = ""
                        '
                        ' -- pause for a second between fetches to pace the server (<10 hits in 10 seconds)
                        Threading.Thread.Sleep(downloadDelay)
                        '
                        ' -- download file
                        Dim rq As System.Net.WebRequest = System.Net.WebRequest.Create(URL)
                        rq.Timeout = 60000
                        Dim response As System.Net.WebResponse = rq.GetResponse()
                        Dim responseStream As IO.Stream = response.GetResponseStream()
                        Dim reader As XmlTextReader = New XmlTextReader(responseStream)
                        Doc.Load(reader)
                        Exit Do
                        'Call Doc.Load(URL)
                    Catch ex As Exception
                        '
                        ' this error could be data related, and may not be critical. log issue and continue
                        '
                        downloadDelay += 2000
                        return_ErrorMessage = "There was an error while requesting the download details for collection [" & CollectionGuid & "]"
                        DownloadCollectionFiles = False
                        Call logController.appendInstallLog(cpCore, errorPrefix & "There was a parse error reading the response [" & ex.ToString() & "]")
                    End Try
                    downloadRetry += 1
                Loop While (downloadRetry < downloadRetryMax)
                If (String.IsNullOrEmpty(return_ErrorMessage)) Then
                    '
                    ' continue if no errors
                    '
                    With Doc.DocumentElement
                        If (LCase(Doc.DocumentElement.Name) <> genericController.vbLCase(DownloadFileRootNode)) Then
                            return_ErrorMessage = "The collection file from the server was Not valid for collection [" & CollectionGuid & "]"
                            DownloadCollectionFiles = False
                            Call logController.appendInstallLog(cpCore, errorPrefix & "The response has a basename [" & Doc.DocumentElement.Name & "] but [" & DownloadFileRootNode & "] was expected.")
                        Else
                            '
                            '------------------------------------------------------------------
                            ' Parse the Download File and download each file into the working folder
                            '------------------------------------------------------------------
                            '
                            If Doc.DocumentElement.ChildNodes.Count = 0 Then
                                return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes."
                                Call logController.appendInstallLog(cpCore, errorPrefix & "The collection library status file from the server has a valid basename, but no childnodes. The collection was probably Not found")
                                DownloadCollectionFiles = False
                            Else
                                With Doc.DocumentElement
                                    For Each CDefSection In .ChildNodes
                                        Select Case genericController.vbLCase(CDefSection.Name)
                                            Case "collection"
                                                '
                                                ' Read in the interfaces and save to Add-ons
                                                '
                                                ResourceFilename = ""
                                                ResourceLink = ""
                                                Collectionname = ""
                                                CollectionGuid = ""
                                                CollectionVersion = ""
                                                CollectionFileLink = ""
                                                For Each CDefInterfaces In CDefSection.ChildNodes
                                                    Select Case genericController.vbLCase(CDefInterfaces.Name)
                                                        Case "name"
                                                            Collectionname = CDefInterfaces.InnerText
                                                        Case "help"
                                                            'CollectionHelp = CDefInterfaces.innerText
                                                            Call cpCore.privateFiles.saveFile(workingPath & "Collection.hlp", CDefInterfaces.InnerText)
                                                        Case "guid"
                                                            CollectionGuid = CDefInterfaces.InnerText
                                                        Case "lastchangedate"
                                                            return_CollectionLastChangeDate = genericController.EncodeDate(CDefInterfaces.InnerText)
                                                        Case "version"
                                                            CollectionVersion = CDefInterfaces.InnerText
                                                        Case "collectionfilelink"
                                                            CollectionFileLink = CDefInterfaces.InnerText
                                                            CollectionFileCnt = CollectionFileCnt + 1
                                                            If CollectionFileLink <> "" Then
                                                                Pos = InStrRev(CollectionFileLink, "/")
                                                                If (Pos <= 0) And (Pos < Len(CollectionFileLink)) Then
                                                                    '
                                                                    ' Skip this file because the collecion file link has no slash (no file)
                                                                    '
                                                                    Call logController.appendInstallLog(cpCore, errorPrefix & "Collection [" & Collectionname & "] was Not installed because the Collection File Link does Not point to a valid file [" & CollectionFileLink & "]")
                                                                Else
                                                                    CollectionFilePath = workingPath & Mid(CollectionFileLink, Pos + 1)
                                                                    Call cpCore.privateFiles.SaveRemoteFile(CollectionFileLink, CollectionFilePath)
                                                                    ' BuildCollectionFolder takes care of the unzipping.
                                                                    'If genericController.vbLCase(Right(CollectionFilePath, 4)) = ".zip" Then
                                                                    '    Call UnzipAndDeleteFile_AndWait(CollectionFilePath)
                                                                    'End If
                                                                    'DownloadCollectionFiles = True
                                                                End If
                                                            End If
                                                        Case "activexdll", "resourcelink"
                                                            '
                                                            ' save the filenames and download them only if OKtoinstall
                                                            '
                                                            ResourceFilename = ""
                                                            ResourceLink = ""
                                                            For Each ActiveXNode In CDefInterfaces.ChildNodes
                                                                Select Case genericController.vbLCase(ActiveXNode.Name)
                                                                    Case "filename"
                                                                        ResourceFilename = ActiveXNode.InnerText
                                                                    Case "link"
                                                                        ResourceLink = ActiveXNode.InnerText
                                                                End Select
                                                            Next
                                                            If ResourceLink = "" Then
                                                                UserError = "There was an error processing a collection in the download file [" & Collectionname & "]. An ActiveXDll node with filename [" & ResourceFilename & "] contained no 'Link' attribute."
                                                                Call logController.appendInstallLog(cpCore, errorPrefix & UserError)
                                                            Else
                                                                If ResourceFilename = "" Then
                                                                    '
                                                                    ' Take Filename from Link
                                                                    '
                                                                    Pos = InStrRev(ResourceLink, "/")
                                                                    If Pos <> 0 Then
                                                                        ResourceFilename = Mid(ResourceLink, Pos + 1)
                                                                    End If
                                                                End If
                                                                If ResourceFilename = "" Then
                                                                    UserError = "There was an error processing a collection in the download file [" & Collectionname & "]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" & ResourceLink & "]."
                                                                    Call logController.appendInstallLog(cpCore, errorPrefix & UserError)
                                                                Else
                                                                    Call cpCore.privateFiles.SaveRemoteFile(ResourceLink, workingPath & ResourceFilename)
                                                                End If
                                                            End If
                                                    End Select
                                                Next
                                        End Select
                                    Next
                                End With
                                If CollectionFileCnt = 0 Then
                                    Call logController.appendInstallLog(cpCore, errorPrefix & "The collection was requested and downloaded, but was not installed because the download file did not have a collection root node.")
                                End If
                            End If
                        End If
                    End With
                End If
                '
                ' no - register anything that downloaded correctly - if this collection contains an import, and one of the imports has a problem, all the rest need to continue
                '
                ''
                'If Not DownloadCollectionFiles Then
                '    '
                '    ' Must clear these out, if there is an error, a reset will keep the error message from making it to the page
                '    '
                '    Return_IISResetRequired = False
                '    Return_RegisterList = ""
                'End If
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return DownloadCollectionFiles
        End Function
        '
        '====================================================================================================
        '
        '   Upgrade all Apps from a Library Collection
        '
        '   If the collection is not in the local collection, download it, otherwise, use what is in local (do not check for updates)
        '     -Does not check if local collections are up-to-date, assume they are. (builderAllLocalCollectionsFromLib2 upgrades local collections)
        '
        '   If TargetAppName is blank, force install on all apps. (force install means install if missing)
        '
        '   Go through each app and call UpgradeAllAppsFromLocalCollect with allowupgrade FALSE (if found in app already, skip the app)
        '       If Collection is already installed on an App, do not builder.
        '
        '   Returns true if no errors during upgrade
        '
        '=========================================================================================================================
        '
        Public Shared Function installCollectionFromRemoteRepo(cpCore As coreClass, ByVal CollectionGuid As String, ByRef return_ErrorMessage As String, ByVal ImportFromCollectionsGuidList As String, IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim UpgradeOK As Boolean = True
            Try
                '
                Dim CollectionVersionFolderName As String
                Dim workingPath As String
                Dim CollectionLastChangeDate As Date
                Dim collectionGuidList As New List(Of String)
                'Dim builder As New coreBuilderClass(cpCore)
                'Dim isBaseCollection As Boolean = (CollectionGuid = baseCollectionGuid)
                '
                ' normalize guid
                '
                If Len(CollectionGuid) < 38 Then
                    If Len(CollectionGuid) = 32 Then
                        CollectionGuid = Mid(CollectionGuid, 1, 8) & "-" & Mid(CollectionGuid, 9, 4) & "-" & Mid(CollectionGuid, 13, 4) & "-" & Mid(CollectionGuid, 17, 4) & "-" & Mid(CollectionGuid, 21)
                    End If
                    If Len(CollectionGuid) = 36 Then
                        CollectionGuid = "{" & CollectionGuid & "}"
                    End If
                End If
                '
                ' Install it if it is not already here
                '
                CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid)
                If CollectionVersionFolderName = "" Then
                    '
                    ' Download all files for this collection and build the collection folder(s)
                    '
                    workingPath = cpCore.addon.getPrivateFilesAddonPath() & "temp_" & genericController.GetRandomInteger() & "\"
                    Call cpCore.privateFiles.createPath(workingPath)
                    '
                    UpgradeOK = DownloadCollectionFiles(cpCore, workingPath, CollectionGuid, CollectionLastChangeDate, return_ErrorMessage)
                    If Not UpgradeOK Then
                        UpgradeOK = UpgradeOK
                    Else
                        UpgradeOK = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, collectionGuidList, return_ErrorMessage, False)
                        If Not UpgradeOK Then
                            UpgradeOK = UpgradeOK
                        End If
                    End If
                    '
                    Call cpCore.privateFiles.DeleteFileFolder(workingPath)
                End If
                '
                ' Upgrade the server from the collection files
                '
                If UpgradeOK Then
                    UpgradeOK = installCollectionFromLocalRepo(cpCore, CollectionGuid, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, nonCriticalErrorList)
                    If Not UpgradeOK Then
                        UpgradeOK = UpgradeOK
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
                Throw ex
            End Try
            Return UpgradeOK
        End Function
        '
        '====================================================================================================
        '
        ' Upgrades all collections, registers and resets the server if needed
        '
        Public Shared Function UpgradeLocalCollectionRepoFromRemoteCollectionRepo(cpCore As coreClass, ByRef return_ErrorMessage As String, ByRef return_RegisterList As String, ByRef return_IISResetRequired As Boolean, IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim returnOk As Boolean = True
            Try
                Dim localCollectionUpToDate As Boolean
                Dim GuidArray As String() = {}
                Dim GuidCnt As Integer
                Dim GuidPtr As Integer
                Dim RequestPtr As Integer
                Dim SupportURL As String
                Dim GuidList As String
                Dim CollectionLastChangeDate As Date
                Dim workingPath As String
                Dim LocalFile As String
                Dim LocalGuid As String
                Dim LocalLastChangeDateStr As String
                Dim LocalLastChangeDate As Date
                Dim LibName As String = String.Empty
                Dim LibSystem As Boolean
                Dim LibGUID As String
                Dim LibLastChangeDateStr As String
                Dim LibContensiveVersion As String = String.Empty
                Dim LibLastChangeDate As Date
                Dim LibListNode As XmlNode
                Dim LocalListNode As XmlNode
                Dim CollectionNode As XmlNode
                Dim LocalLastChangeNode As XmlNode
                Dim LibraryCollections As New XmlDocument
                Dim LocalCollections As New XmlDocument
                Dim Doc As New XmlDocument
                Dim Copy As String
                Dim allowLogging As Boolean
                'Dim builder As New coreBuilderClass(cpCore)
                '
                '-----------------------------------------------------------------------------------------------
                '   Load LocalCollections from the Collections.xml file
                '-----------------------------------------------------------------------------------------------
                '
                allowLogging = False
                '
                If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Enter")
                LocalFile = getCollectionListFile(cpCore)
                If LocalFile <> "" Then
                    LocalCollections = New XmlDocument
                    Try
                        LocalCollections.LoadXml(LocalFile)
                    Catch ex As Exception
                        If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), parse error reading collections.xml")
                        Copy = "Error loading privateFiles\addons\Collections.xml"
                        Call logController.appendInstallLog(cpCore, Copy)
                        return_ErrorMessage = return_ErrorMessage & "<P>" & Copy & "</P>"
                        returnOk = False
                    End Try
                    If returnOk Then
                        If genericController.vbLCase(LocalCollections.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
                            If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The addons\Collections.xml file has an invalid root node")
                            Copy = "The addons\Collections.xml has an invalid root node, [" & LocalCollections.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected."
                            'Copy = "The LocalCollections file [" & App.Path & "\Addons\Collections.xml] has an invalid root node, [" & LocalCollections.DocumentElement.name & "] was received and [" & CollectionListRootNode & "] was expected."
                            Call logController.appendInstallLog(cpCore, Copy)
                            return_ErrorMessage = return_ErrorMessage & "<P>" & Copy & "</P>"
                            returnOk = False
                        Else
                            '
                            ' Get a list of the collection guids on this server
                            '

                            GuidCnt = 0
                            With LocalCollections.DocumentElement
                                If genericController.vbLCase(.Name) = "collectionlist" Then
                                    For Each LocalListNode In .ChildNodes
                                        Select Case genericController.vbLCase(LocalListNode.Name)
                                            Case "collection"
                                                For Each CollectionNode In LocalListNode.ChildNodes
                                                    If genericController.vbLCase(CollectionNode.Name) = "guid" Then
                                                        ReDim Preserve GuidArray(GuidCnt)
                                                        GuidArray(GuidCnt) = CollectionNode.InnerText
                                                        GuidCnt = GuidCnt + 1
                                                        Exit For
                                                    End If
                                                Next
                                        End Select
                                    Next
                                End If
                            End With
                            If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), collection.xml file has " & GuidCnt & " collection nodes.")
                            If GuidCnt > 0 Then
                                '
                                ' Request collection updates 10 at a time
                                '
                                GuidPtr = 0
                                Do While GuidPtr < GuidCnt
                                    RequestPtr = 0
                                    GuidList = ""
                                    Do While (GuidPtr < GuidCnt) And RequestPtr < 10
                                        GuidList = GuidList & "," & GuidArray(GuidPtr)
                                        GuidPtr = GuidPtr + 1
                                        RequestPtr = RequestPtr + 1
                                    Loop
                                    '
                                    ' Request these 10 from the support library
                                    '
                                    'If genericController.vbInstr(1, GuidList, "58c9", vbTextCompare) <> 0 Then
                                    '    GuidList = GuidList
                                    'End If
                                    If GuidList <> "" Then
                                        GuidList = Mid(GuidList, 2)
                                        '
                                        '-----------------------------------------------------------------------------------------------
                                        '   Load LibraryCollections from the Support Site
                                        '-----------------------------------------------------------------------------------------------
                                        '
                                        If allowLogging Then
                                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), requesting Library updates for [" & GuidList & "]")
                                        End If
                                        'hint = "Getting CollectionList"
                                        LibraryCollections = New XmlDocument
                                        SupportURL = "http://support.contensive.com/GetCollectionList?iv=" & cpCore.codeVersion() & "&guidlist=" & EncodeRequestVariable(GuidList)
                                        Dim loadOK As Boolean = True
                                        Try
                                            LibraryCollections.Load(SupportURL)
                                        Catch ex As Exception
                                            If allowLogging Then
                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Error downloading or loading GetCollectionList from Support.")
                                            End If
                                            Copy = "Error downloading or loading GetCollectionList from Support."
                                            Call logController.appendInstallLog(cpCore, Copy & ", the request was [" & SupportURL & "]")
                                            return_ErrorMessage = return_ErrorMessage & "<P>" & Copy & "</P>"
                                            returnOk = False
                                            loadOK = False
                                        End Try
                                        If loadOK Then
                                            If True Then
                                                If genericController.vbLCase(LibraryCollections.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
                                                    Copy = "The GetCollectionList support site remote method returned an xml file with an invalid root node, [" & LibraryCollections.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected."
                                                    If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), " & Copy)
                                                    Call logController.appendInstallLog(cpCore, Copy & ", the request was [" & SupportURL & "]")
                                                    return_ErrorMessage = return_ErrorMessage & "<P>" & Copy & "</P>"
                                                    returnOk = False
                                                Else
                                                    With LocalCollections.DocumentElement
                                                        If genericController.vbLCase(.Name) <> "collectionlist" Then
                                                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The Library response did not have a collectioinlist top node, the request was [" & SupportURL & "]")
                                                        Else
                                                            '
                                                            '-----------------------------------------------------------------------------------------------
                                                            ' Search for Collection Updates Needed
                                                            '-----------------------------------------------------------------------------------------------
                                                            '
                                                            For Each LocalListNode In .ChildNodes
                                                                localCollectionUpToDate = False
                                                                If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Process local collection.xml node [" & LocalListNode.Name & "]")
                                                                Select Case genericController.vbLCase(LocalListNode.Name)
                                                                    Case "collection"
                                                                        LocalGuid = ""
                                                                        LocalLastChangeDateStr = ""
                                                                        LocalLastChangeDate = Date.MinValue
                                                                        LocalLastChangeNode = Nothing
                                                                        For Each CollectionNode In LocalListNode.ChildNodes
                                                                            Select Case genericController.vbLCase(CollectionNode.Name)
                                                                                Case "guid"
                                                                                    '
                                                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText)
                                                                                    'LocalGUID = genericController.vbReplace(LocalGUID, "{", "")
                                                                                    'LocalGUID = genericController.vbReplace(LocalGUID, "}", "")
                                                                                    'LocalGUID = genericController.vbReplace(LocalGUID, "-", "")
                                                                                Case "lastchangedate"
                                                                                    '
                                                                                    LocalLastChangeDateStr = CollectionNode.InnerText
                                                                                    LocalLastChangeNode = CollectionNode
                                                                            End Select
                                                                        Next
                                                                        If LocalGuid <> "" Then
                                                                            If Not IsDate(LocalLastChangeDateStr) Then
                                                                                LocalLastChangeDate = Date.MinValue
                                                                            Else
                                                                                LocalLastChangeDate = genericController.EncodeDate(LocalLastChangeDateStr)
                                                                            End If
                                                                        End If
                                                                        If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), node is collection, LocalGuid [" & LocalGuid & "], LocalLastChangeDateStr [" & LocalLastChangeDateStr & "]")
                                                                        '
                                                                        ' go through each collection on the Library and find the local collection guid
                                                                        '
                                                                        For Each LibListNode In LibraryCollections.DocumentElement.ChildNodes
                                                                            If localCollectionUpToDate Then
                                                                                Exit For
                                                                            End If
                                                                            Select Case genericController.vbLCase(LibListNode.Name)
                                                                                Case "collection"
                                                                                    LibGUID = ""
                                                                                    LibLastChangeDateStr = ""
                                                                                    LibLastChangeDate = Date.MinValue
                                                                                    For Each CollectionNode In LibListNode.ChildNodes
                                                                                        Select Case genericController.vbLCase(CollectionNode.Name)
                                                                                            Case "name"
                                                                                                '
                                                                                                LibName = genericController.vbLCase(CollectionNode.InnerText)
                                                                                            Case "system"
                                                                                                '
                                                                                                LibSystem = genericController.EncodeBoolean(CollectionNode.InnerText)
                                                                                            Case "guid"
                                                                                                '
                                                                                                LibGUID = genericController.vbLCase(CollectionNode.InnerText)
                                                                                                'LibGUID = genericController.vbReplace(LibGUID, "{", "")
                                                                                                'LibGUID = genericController.vbReplace(LibGUID, "}", "")
                                                                                                'LibGUID = genericController.vbReplace(LibGUID, "-", "")
                                                                                            Case "lastchangedate"
                                                                                                '
                                                                                                LibLastChangeDateStr = CollectionNode.InnerText
                                                                                                LibLastChangeDateStr = LibLastChangeDateStr
                                                                                            Case "contensiveversion"
                                                                                                '
                                                                                                LibContensiveVersion = CollectionNode.InnerText
                                                                                        End Select
                                                                                    Next
                                                                                    If LibGUID <> "" Then
                                                                                        If genericController.vbInstr(1, LibGUID, "58c9", vbTextCompare) <> 0 Then
                                                                                            LibGUID = LibGUID
                                                                                        End If
                                                                                        If (LibGUID <> "") And (LibGUID = LocalGuid) And ((LibContensiveVersion = "") Or (LibContensiveVersion <= cpCore.codeVersion())) Then
                                                                                            '
                                                                                            ' LibCollection matches the LocalCollection - process the upgrade
                                                                                            '
                                                                                            If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Library collection node found that matches")
                                                                                            If genericController.vbInstr(1, LibGUID, "58c9", vbTextCompare) <> 0 Then
                                                                                                LibGUID = LibGUID
                                                                                            End If
                                                                                            If Not IsDate(LibLastChangeDateStr) Then
                                                                                                LibLastChangeDate = Date.MinValue
                                                                                            Else
                                                                                                LibLastChangeDate = genericController.EncodeDate(LibLastChangeDateStr)
                                                                                            End If
                                                                                            ' TestPoint 1.1 - Test each collection for upgrade
                                                                                            If LibLastChangeDate > LocalLastChangeDate Then
                                                                                                '
                                                                                                ' LibLastChangeDate <>0, and it is > local lastchangedate
                                                                                                '
                                                                                                workingPath = cpCore.addon.getPrivateFilesAddonPath() & "\temp_" & genericController.GetRandomInteger() & "\"
                                                                                                If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), matching library collection is newer, start upgrade [" & workingPath & "].")
                                                                                                Call logController.appendInstallLog(cpCore, "Upgrading Collection [" & LibGUID & "], Library name [" & LibName & "], because LocalChangeDate [" & LocalLastChangeDate & "] < LibraryChangeDate [" & LibLastChangeDate & "]")
                                                                                                '
                                                                                                ' Upgrade Needed
                                                                                                '
                                                                                                Call cpCore.privateFiles.createPath(workingPath)
                                                                                                '
                                                                                                returnOk = DownloadCollectionFiles(cpCore, workingPath, LibGUID, CollectionLastChangeDate, return_ErrorMessage)
                                                                                                If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), DownloadCollectionFiles returned " & returnOk)
                                                                                                If returnOk Then
                                                                                                    Dim listGuidList As New List(Of String)
                                                                                                    returnOk = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, listGuidList, return_ErrorMessage, allowLogging)
                                                                                                    If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), BuildLocalCollectionFolder returned " & returnOk)
                                                                                                End If
                                                                                                '
                                                                                                If allowLogging Then
                                                                                                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), working folder not deleted because debugging. Delete tmp folders when finished.")
                                                                                                Else
                                                                                                    Call cpCore.privateFiles.DeleteFileFolder(workingPath)
                                                                                                End If
                                                                                                '
                                                                                                ' Upgrade the apps from the collection files, do not install on any apps
                                                                                                '
                                                                                                If returnOk Then
                                                                                                    returnOk = installCollectionFromLocalRepo(cpCore, LibGUID, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, "", IsNewBuild, nonCriticalErrorList)
                                                                                                    If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), UpgradeAllAppsFromLocalCollection returned " & returnOk)
                                                                                                End If
                                                                                                '
                                                                                                ' make sure this issue is logged and clear the flag to let other local collections install
                                                                                                '
                                                                                                If Not returnOk Then
                                                                                                    If allowLogging Then logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), for this local collection, process returned " & returnOk)
                                                                                                    Call logController.appendInstallLog(cpCore, "There was a problem upgrading Collection [" & LibGUID & "], Library name [" & LibName & "], error message [" & return_ErrorMessage & "], will clear error and continue with the next collection, the request was [" & SupportURL & "]")
                                                                                                    returnOk = True
                                                                                                End If
                                                                                            End If
                                                                                            '
                                                                                            ' this local collection has been resolved, go to the next local collection
                                                                                            '
                                                                                            localCollectionUpToDate = True
                                                                                            '
                                                                                            If Not returnOk Then
                                                                                                Call logController.appendInstallLog(cpCore, "There was a problem upgrading Collection [" & LibGUID & "], Library name [" & LibName & "], error message [" & return_ErrorMessage & "], will clear error and continue with the next collection")
                                                                                                returnOk = True
                                                                                            End If
                                                                                        End If
                                                                                    End If
                                                                            End Select
                                                                        Next
                                                                End Select
                                                            Next
                                                        End If
                                                    End With
                                                End If
                                            End If
                                        End If
                                    End If
                                Loop
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        '
        '   Upgrade a collection from the files in a working folder
        '
        '   This routine always installs the Collection
        '       Builds Add-on folder, copies Resourcefiles (XML, DLLs, images) into the folder, and returns a list of DLLs to register
        '       Then goes from app to app to find either the TargetInstallApp, or any with this Collection already installed
        '       ImportCollections should call the UpgradeFromLibraryGUID
        '
        '   First, it unzips any zip file in the working folder
        '   The collection file is in the WorkingFolder.
        '   If there are attachments (DLLs), they should be in the WorkingFolder also
        '
        '   This is the routine that updates the Collections.xml file
        '       - if it parses ok
        '
        Public Shared Function BuildLocalCollectionReposFromFolder(cpCore As coreClass, ByVal sourcePrivateFolderPath As String, ByVal CollectionLastChangeDate As Date, ByRef return_CollectionGUIDList As List(Of String), ByRef return_ErrorMessage As String, ByVal allowLogging As Boolean) As Boolean
            Dim success As Boolean = False
            Try
                If cpCore.privateFiles.pathExists(sourcePrivateFolderPath) Then
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" & sourcePrivateFolderPath & "]")
                    Dim SrcFileNamelist As IO.FileInfo() = cpCore.privateFiles.getFileList(sourcePrivateFolderPath)
                    For Each file As IO.FileInfo In SrcFileNamelist
                        If (file.Extension = ".zip") Or (file.Extension = ".xml") Then
                            Dim collectionGuid As String = ""
                            success = BuildLocalCollectionRepoFromFile(cpCore, sourcePrivateFolderPath & file.Name, CollectionLastChangeDate, collectionGuid, return_ErrorMessage, allowLogging)
                            return_CollectionGUIDList.Add(collectionGuid)
                        End If
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return success
        End Function
        '
        '
        '
        Public Shared Function BuildLocalCollectionRepoFromFile(cpCore As coreClass, ByVal collectionPathFilename As String, ByVal CollectionLastChangeDate As Date, ByRef return_CollectionGUID As String, ByRef return_ErrorMessage As String, ByVal allowLogging As Boolean) As Boolean
            Dim result As Boolean = True
            Try
                Dim ResourceType As String
                Dim CollectionVersionFolderName As String = String.Empty
                Dim ChildCollectionLastChangeDate As Date
                Dim ChildWorkingPath As String
                Dim ChildCollectionGUID As String
                Dim ChildCollectionName As String
                Dim Found As Boolean
                Dim CollectionFile As New XmlDocument
                Dim UpdatingCollection As Boolean
                Dim Collectionname As String = String.Empty
                Dim NowTime As Date
                Dim NowPart As Integer
                Dim SrcFileNamelist As IO.FileInfo()
                Dim TimeStamp As String
                Dim Pos As Integer
                Dim CollectionFolder As String
                Dim CollectionGuid As String = String.Empty
                Dim AOGuid As String
                Dim AOName As String
                Dim IsFound As Boolean
                Dim Filename As String
                Dim CDefSection As XmlNode
                Dim Doc As New XmlDocument
                Dim CDefInterfaces As XmlNode
                Dim StatusOK As Boolean
                Dim CollectionFileBaseName As String
                Dim XMLTools As New xmlController(cpCore)
                Dim CollectionFolderName As String = ""
                Dim CollectionFileFound As Boolean = False
                Dim ZipFileFound As Boolean = False
                Dim collectionPath As String = ""
                Dim collectionFilename As String = ""
                '
                ' process all xml files in this workingfolder
                '
                If allowLogging Then logController.appendLog(cpCore, "BuildLocalCollectionFolder(), Enter")
                '
                cpCore.privateFiles.splitPathFilename(collectionPathFilename, collectionPath, collectionFilename)
                If Not cpCore.privateFiles.pathExists(collectionPath) Then
                    '
                    ' The working folder is not there
                    '
                    result = False
                    return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>"
                    If allowLogging Then logController.appendLog(cpCore, "BuildLocalCollectionFolder(), " & return_ErrorMessage)
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" & collectionPath & "]")
                Else
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" & collectionPath & "]")
                    '
                    ' move collection file to a temp directory
                    '
                    Dim tmpInstallPath As String = "tmpInstallCollection" & genericController.createGuid().Replace("{", "").Replace("}", "").Replace("-", "") & "\"
                    cpCore.privateFiles.copyFile(collectionPathFilename, tmpInstallPath & collectionFilename)
                    If (collectionFilename.ToLower().Substring(collectionFilename.Length - 4) = ".zip") Then
                        Call cpCore.privateFiles.UnzipFile(tmpInstallPath & collectionFilename)
                        Call cpCore.privateFiles.deleteFile(tmpInstallPath & collectionFilename)
                    End If
                    '
                    ' install the individual files
                    '
                    SrcFileNamelist = cpCore.privateFiles.getFileList(tmpInstallPath)
                    If True Then
                        '
                        ' Process all non-zip files
                        '
                        For Each file As IO.FileInfo In SrcFileNamelist
                            Filename = file.Name
                            Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files, filename=[" & Filename & "]")
                            If genericController.vbLCase(Right(Filename, 4)) = ".xml" Then
                                '
                                Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing xml file [" & Filename & "]")
                                'hint = hint & ",320"
                                CollectionFile = New XmlDocument

                                Dim loadOk = True
                                Try
                                    Call CollectionFile.LoadXml(cpCore.privateFiles.readFile(tmpInstallPath & Filename))
                                Catch ex As Exception
                                    '
                                    ' There was a parse error in this xml file. Set the return message and the flag
                                    ' If another xml files shows up, and process OK it will cover this error
                                    '
                                    'hint = hint & ",330"
                                    return_ErrorMessage = "<p>There was a problem with the Collection File for this cpcore.addon.</p>"
                                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, error reading collection [" & collectionPathFilename & "]")
                                    'StatusOK = False
                                    loadOk = False
                                End Try
                                If loadOk Then
                                    'hint = hint & ",400"
                                    CollectionFileBaseName = genericController.vbLCase(CollectionFile.DocumentElement.Name)
                                    If (CollectionFileBaseName <> "contensivecdef") And (CollectionFileBaseName <> CollectionFileRootNode) And (CollectionFileBaseName <> genericController.vbLCase(CollectionFileRootNodeOld)) Then
                                        '
                                        ' Not a problem, this is just not a collection file
                                        '
                                        Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, xml base name wrong [" & CollectionFileBaseName & "]")
                                    Else
                                        '
                                        ' Collection File
                                        '
                                        'hint = hint & ",420"
                                        With CollectionFile.DocumentElement
                                            Collectionname = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "name", "")
                                            If Collectionname = "" Then
                                                '
                                                ' ----- Error condition -- it must have a collection name
                                                '
                                                result = False
                                                return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>"
                                                Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, collection has no name")
                                            Else
                                                '
                                                '------------------------------------------------------------------
                                                ' Build Collection folder structure in /Add-ons folder
                                                '------------------------------------------------------------------
                                                '
                                                'hint = hint & ",440"
                                                CollectionFileFound = True
                                                CollectionGuid = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "guid", Collectionname)
                                                If CollectionGuid = "" Then
                                                    '
                                                    ' I hope I do not regret this
                                                    '
                                                    CollectionGuid = Collectionname
                                                End If

                                                CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid)
                                                If CollectionVersionFolderName <> "" Then
                                                    '
                                                    ' This is an upgrade
                                                    '
                                                    'hint = hint & ",450"
                                                    UpdatingCollection = True
                                                    Pos = genericController.vbInstr(1, CollectionVersionFolderName, "\")
                                                    If Pos > 0 Then
                                                        CollectionFolderName = Mid(CollectionVersionFolderName, 1, Pos - 1)
                                                    End If
                                                Else
                                                    '
                                                    ' This is an install
                                                    '
                                                    'hint = hint & ",460"
                                                    CollectionFolderName = CollectionGuid
                                                    CollectionFolderName = genericController.vbReplace(CollectionFolderName, "{", "")
                                                    CollectionFolderName = genericController.vbReplace(CollectionFolderName, "}", "")
                                                    CollectionFolderName = genericController.vbReplace(CollectionFolderName, "-", "")
                                                    CollectionFolderName = genericController.vbReplace(CollectionFolderName, " ", "")
                                                    CollectionFolderName = Collectionname & "_" & CollectionFolderName
                                                End If
                                                CollectionFolder = cpCore.addon.getPrivateFilesAddonPath() & CollectionFolderName & "\"
                                                If Not cpCore.privateFiles.pathExists(CollectionFolder) Then
                                                    '
                                                    ' Create collection folder
                                                    '
                                                    'hint = hint & ",470"
                                                    Call cpCore.privateFiles.createPath(CollectionFolder)
                                                End If
                                                '
                                                ' create a collection 'version' folder for these new files
                                                '
                                                TimeStamp = ""
                                                NowTime = DateTime.Now()
                                                NowPart = NowTime.Year
                                                TimeStamp &= NowPart.ToString()
                                                NowPart = NowTime.Month
                                                If (NowPart < 10) Then TimeStamp &= "0"
                                                TimeStamp &= NowPart.ToString()
                                                NowPart = NowTime.Day
                                                If (NowPart < 10) Then TimeStamp &= "0"
                                                TimeStamp &= NowPart.ToString()
                                                NowPart = NowTime.Hour
                                                If (NowPart < 10) Then TimeStamp &= "0"
                                                TimeStamp &= NowPart.ToString()
                                                NowPart = NowTime.Minute
                                                If (NowPart < 10) Then TimeStamp &= "0"
                                                TimeStamp &= NowPart.ToString()
                                                NowPart = NowTime.Second
                                                If (NowPart < 10) Then TimeStamp &= "0"
                                                TimeStamp &= NowPart.ToString()
                                                CollectionVersionFolderName = CollectionFolderName & "\" & TimeStamp
                                                Dim CollectionVersionFolder As String = cpCore.addon.getPrivateFilesAddonPath() & CollectionVersionFolderName
                                                Dim CollectionVersionPath As String = CollectionVersionFolder & "\"
                                                Call cpCore.privateFiles.createPath(CollectionVersionPath)

                                                Call cpCore.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder)
                                                'StatusOK = True
                                                '
                                                ' Install activeX and search for importcollections
                                                '
                                                'hint = hint & ",500"
                                                For Each CDefSection In CollectionFile.DocumentElement.ChildNodes
                                                    Select Case genericController.vbLCase(CDefSection.Name)
                                                        Case "resource"
                                                            ''
                                                            '' resource node, if executable node, save to RegisterList
                                                            ''
                                                            ''hint = hint & ",510"
                                                            'ResourceType = genericController.vbLCase(GetXMLAttribute(cpCore, IsFound, CDefSection, "type", ""))
                                                            'Dim resourceFilename As String = Trim(GetXMLAttribute(cpCore, IsFound, CDefSection, "name", ""))
                                                            'Dim resourcePathFilename As String = CollectionVersionPath & resourceFilename
                                                            'If resourceFilename = "" Then
                                                            '    '
                                                            '    ' filename is blank
                                                            '    '
                                                            '    'hint = hint & ",511"
                                                            'ElseIf Not cpCore.privateFiles.fileExists(resourcePathFilename) Then
                                                            '    '
                                                            '    ' resource is not here
                                                            '    '
                                                            '    'hint = hint & ",513"
                                                            '    result = False
                                                            '    return_ErrorMessage = "<p>There was a problem with the Collection File. The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.</p>"
                                                            '    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.")
                                                            '    'StatusOK = False
                                                            'Else
                                                            '    Select Case ResourceType
                                                            '        Case "executable"
                                                            '            '
                                                            '            ' Executable resources - add to register list
                                                            '            '
                                                            '            'hint = hint & ",520"
                                                            '            If False Then
                                                            '                '
                                                            '                ' file is already installed
                                                            '                '
                                                            '                'hint = hint & ",521"
                                                            '            Else
                                                            '                '
                                                            '                ' Add the file to be registered
                                                            '                '
                                                            '            End If
                                                            '        Case "www"
                                                            '        Case "file"
                                                            '    End Select
                                                            'End If
                                                        Case "interfaces"
                                                            ''
                                                            '' Compatibility only - this is deprecated - Install ActiveX found in Add-ons
                                                            ''
                                                            ''hint = hint & ",530"
                                                            'For Each CDefInterfaces In CDefSection.ChildNodes
                                                            '    AOName = GetXMLAttribute(cpCore, IsFound, CDefInterfaces, "name", "No Name")
                                                            '    If AOName = "" Then
                                                            '        AOName = "No Name"
                                                            '    End If
                                                            '    AOGuid = GetXMLAttribute(cpCore, IsFound, CDefInterfaces, "guid", AOName)
                                                            '    If AOGuid = "" Then
                                                            '        AOGuid = AOName
                                                            '    End If
                                                            'Next
                                                        Case "getcollection", "importcollection"
                                                            '
                                                            ' -- Download Collection file into install folder
                                                            ChildCollectionName = GetXMLAttribute(cpCore, Found, CDefSection, "name", "")
                                                            ChildCollectionGUID = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText)
                                                            If ChildCollectionGUID = "" Then
                                                                ChildCollectionGUID = CDefSection.InnerText
                                                            End If
                                                            Dim statusMsg As String = "Installing collection [" & ChildCollectionName & ", " & ChildCollectionGUID & "] referenced from collection [" & Collectionname & "]"
                                                            Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" & ChildCollectionName & "], childCollectionGuid [" & ChildCollectionGUID & "]")
                                                            If genericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, vbTextCompare) = 0 Then
                                                                If ChildCollectionGUID = "" Then
                                                                    '
                                                                    ' -- Needs a GUID to install
                                                                    result = False
                                                                    return_ErrorMessage = statusMsg & ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID."
                                                                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, return message [" & return_ErrorMessage & "]")
                                                                ElseIf GetCollectionPath(cpCore, ChildCollectionGUID) = "" Then
                                                                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" & ChildCollectionGUID & "], not found so needs to be installed")
                                                                    '
                                                                    ' If it is not already installed, download and install it also
                                                                    '
                                                                    ChildWorkingPath = CollectionVersionPath & "\" & ChildCollectionGUID & "\"
                                                                    '
                                                                    ' down an imported collection file
                                                                    '
                                                                    StatusOK = DownloadCollectionFiles(cpCore, ChildWorkingPath, ChildCollectionGUID, ChildCollectionLastChangeDate, return_ErrorMessage)
                                                                    If Not StatusOK Then

                                                                        Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" & statusMsg & "], downloadCollectionFiles returned error state, message [" & return_ErrorMessage & "]")
                                                                        If return_ErrorMessage = "" Then
                                                                            return_ErrorMessage = statusMsg & ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" & ChildCollectionGUID & "]."
                                                                        Else
                                                                            return_ErrorMessage = statusMsg & ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" & ChildCollectionGUID & "]. The error was [" & return_ErrorMessage & "]"
                                                                        End If
                                                                    Else
                                                                        Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" & ChildCollectionGUID & "], downloadCollectionFiles returned OK")
                                                                        '
                                                                        ' install the downloaded file
                                                                        '
                                                                        Dim ChildCollectionGUIDList As New List(Of String)
                                                                        StatusOK = BuildLocalCollectionReposFromFolder(cpCore, ChildWorkingPath, ChildCollectionLastChangeDate, ChildCollectionGUIDList, return_ErrorMessage, allowLogging)
                                                                        If Not StatusOK Then
                                                                            Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" & statusMsg & "], BuildLocalCollectionFolder returned error state, message [" & return_ErrorMessage & "]")
                                                                            If return_ErrorMessage = "" Then
                                                                                return_ErrorMessage = statusMsg & ". The installation can not continue because there was an unknown error installing the included collection file, guid [" & ChildCollectionGUID & "]."
                                                                            Else
                                                                                return_ErrorMessage = statusMsg & ". The installation can not continue because there was an unknown error installing the included collection file, guid [" & ChildCollectionGUID & "]. The error was [" & return_ErrorMessage & "]"
                                                                            End If
                                                                        End If
                                                                    End If
                                                                    '
                                                                    ' -- remove child installation working folder
                                                                    cpCore.privateFiles.DeleteFileFolder(ChildWorkingPath)
                                                                Else
                                                                    '
                                                                    '
                                                                    '
                                                                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" & ChildCollectionGUID & "], already installed")
                                                                End If
                                                            End If
                                                    End Select
                                                    If (return_ErrorMessage <> "") Then
                                                        '
                                                        ' if error, no more nodes in this collection file
                                                        '
                                                        result = False
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End With
                                    End If
                                End If
                            End If
                            If (return_ErrorMessage <> "") Then
                                '
                                ' if error, no more files
                                '
                                result = False
                                Exit For
                            End If
                        Next
                        If (return_ErrorMessage = "") And (Not CollectionFileFound) Then
                            '
                            ' no errors, but the collection file was not found
                            '
                            If ZipFileFound Then
                                '
                                ' zip file found but did not qualify
                                '
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not include a valid collection xml file.</p>"
                            Else
                                '
                                ' zip file not found
                                '
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip was not downloaded successfully.</p>"
                            End If
                            'StatusOK = False
                        End If
                    End If
                    '
                    ' delete the working folder
                    '
                    Call cpCore.privateFiles.DeleteFileFolder(tmpInstallPath)
                End If
                '
                ' If the collection parsed correctly, update the Collections.xml file
                '
                If (return_ErrorMessage = "") Then
                    Call UpdateConfig(cpCore, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName)
                Else
                    '
                    ' there was an error processing the collection, be sure to save description in the log
                    '
                    result = False
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" & return_ErrorMessage & "]")
                End If
                '
                Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, Exiting with ErrorMessage [" & return_ErrorMessage & "]")
                '
                BuildLocalCollectionRepoFromFile = (return_ErrorMessage = "")
                return_CollectionGUID = CollectionGuid
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '=========================================================================================================================
        '   Upgrade Application from a local collection
        '
        '   Either Upgrade or Install the collection in the Application. - no checks
        '
        '   ImportFromCollectionsGuidList - If this collection is from an import, this is the guid of the import.
        '=========================================================================================================================
        '
        Public Shared Function installCollectionFromLocalRepo(cpCore As coreClass, ByVal CollectionGuid As String, ByVal ignore_BuildVersion As String, ByRef return_ErrorMessage As String, ByVal ImportFromCollectionsGuidList As String, IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim result As Boolean = False
            Try
                Dim CollectionVersionFolderName As String = ""
                Dim CollectionLastChangeDate As Date = Nothing
                Call GetCollectionConfig(cpCore, CollectionGuid, CollectionVersionFolderName, CollectionLastChangeDate, "")
                If String.IsNullOrEmpty(CollectionVersionFolderName) Then
                    result = False
                    return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>"
                Else
                    '
                    ' Search Local Collection Folder for collection config file (xml file)
                    '
                    Dim CollectionVersionFolder As String = cpCore.addon.getPrivateFilesAddonPath() & CollectionVersionFolderName & "\"
                    Dim srcFileInfoArray As IO.FileInfo() = cpCore.privateFiles.getFileList(CollectionVersionFolder)
                    If srcFileInfoArray.Count = 0 Then
                        result = False
                        return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>"
                    Else
                        '
                        ' collect list of DLL files and add them to the exec files if they were missed
                        Dim assembliesInZip As New List(Of String)
                        For Each file As IO.FileInfo In srcFileInfoArray
                            If (file.Extension.ToLower() = "dll") Then
                                If Not assembliesInZip.Contains(file.Name.ToLower()) Then
                                    assembliesInZip.Add(file.Name.ToLower())
                                End If
                            End If
                        Next
                        '
                        ' -- Process the other files
                        For Each file As IO.FileInfo In srcFileInfoArray
                            If (genericController.vbLCase(Right(file.Name, 4)) = ".xml") Then
                                '
                                ' -- XML file -- open it to figure out if it is one we can use
                                Dim Doc As New XmlDocument
                                Dim CollectionFilename As String = file.Name
                                Dim loadOK As Boolean = True
                                Try
                                    Call Doc.Load(cpCore.privateFiles.rootLocalPath & CollectionVersionFolder & file.Name)
                                Catch ex As Exception
                                    '
                                    ' error - Need a way to reach the user that submitted the file
                                    '
                                    Call logController.appendInstallLog(cpCore, "There was an error reading the Meta data file [" & cpCore.privateFiles.rootLocalPath & CollectionVersionFolder & file.Name & "].")
                                    result = False
                                    return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the xml collection file has an error</P>"
                                    loadOK = False
                                End Try
                                If loadOK Then
                                    With Doc.DocumentElement
                                        If (LCase(.Name) = genericController.vbLCase(CollectionFileRootNode)) Or (LCase(.Name) = genericController.vbLCase(CollectionFileRootNodeOld)) Then
                                            '
                                            '------------------------------------------------------------------------------------------------------
                                            ' Collection File - import from sub so it can be re-entrant
                                            '------------------------------------------------------------------------------------------------------
                                            '
                                            Dim IsFound As Boolean = False
                                            Dim Collectionname As String = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "name", "")
                                            If Collectionname = "" Then
                                                '
                                                ' ----- Error condition -- it must have a collection name
                                                '
                                                'Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
                                                Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, collection has no name")
                                                result = False
                                                return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the collection name in the xml collection file is blank</P>"
                                            Else
                                                Dim CollectionSystem_fileValueOK As Boolean = False
                                                Dim CollectionUpdatable_fileValueOK As Boolean = False
                                                Dim CollectionblockNavigatorNode_fileValueOK As Boolean
                                                Dim CollectionSystem As Boolean = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""))
                                                Dim Parent_NavID As Integer = appBuilderController.verifyNavigatorEntry(cpCore, addonGuidManageAddon, "", "Manage Add-ons", "", "", "", False, False, False, True, "", "", "", 0)
                                                Dim CollectionUpdatable As Boolean = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""))
                                                Dim CollectionblockNavigatorNode As Boolean = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""))
                                                Dim FileGuid As String = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "guid", Collectionname)
                                                If FileGuid = "" Then
                                                    FileGuid = Collectionname
                                                End If
                                                If (LCase(CollectionGuid) <> genericController.vbLCase(FileGuid)) Then
                                                    '
                                                    '
                                                    '
                                                    result = False
                                                    Call logController.appendInstallLog(cpCore, "Local Collection file contains a different GUID for [" & Collectionname & "] then Collections.xml")
                                                    return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>"
                                                Else
                                                    If CollectionGuid = "" Then
                                                        '
                                                        ' I hope I do not regret this
                                                        '
                                                        CollectionGuid = Collectionname
                                                    End If
                                                    '
                                                    '-------------------------------------------------------------------------------
                                                    ' ----- Pass 1
                                                    ' Go through all collection nodes
                                                    ' Process ImportCollection Nodes - so includeaddon nodes will work
                                                    ' these must be processes regardless of the state of this collection in this app
                                                    ' Get Resource file list
                                                    '-------------------------------------------------------------------------------
                                                    '
                                                    'CollectionAddOnCnt = 0
                                                    Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 1")
                                                    Dim wwwFileList As String = ""
                                                    Dim ContentFileList As String = ""
                                                    Dim ExecFileList As String = ""
                                                    For Each CDefSection As XmlNode In .ChildNodes
                                                        Select Case CDefSection.Name.ToLower()
                                                            Case "resource"
                                                                '
                                                                ' set wwwfilelist, contentfilelist, execfilelist
                                                                '
                                                                Dim resourceType As String = GetXMLAttribute(cpCore, IsFound, CDefSection, "type", "")
                                                                Dim resourcePath As String = GetXMLAttribute(cpCore, IsFound, CDefSection, "path", "")
                                                                Dim filename As String = GetXMLAttribute(cpCore, IsFound, CDefSection, "name", "")
                                                                '
                                                                logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 1, resource found, name [" & filename & "], type [" & resourceType & "], path [" & resourcePath & "]")
                                                                '
                                                                filename = genericController.convertToDosSlash(filename)
                                                                Dim SrcPath As String = ""
                                                                Dim DstPath As String = resourcePath
                                                                Dim Pos As Integer = genericController.vbInstr(1, filename, "\")
                                                                If Pos <> 0 Then
                                                                    '
                                                                    ' Source path is in filename
                                                                    '
                                                                    SrcPath = Mid(filename, 1, Pos - 1)
                                                                    filename = Mid(filename, Pos + 1)
                                                                    If resourcePath = "" Then
                                                                        '
                                                                        ' No Resource Path give, use the same folder structure from source
                                                                        '
                                                                        DstPath = SrcPath
                                                                    Else
                                                                        '
                                                                        ' Copy file to resource path
                                                                        '
                                                                        DstPath = resourcePath
                                                                    End If
                                                                End If

                                                                Dim DstFilePath As String = genericController.vbReplace(DstPath, "/", "\")
                                                                If DstFilePath = "\" Then
                                                                    DstFilePath = ""
                                                                End If
                                                                If DstFilePath <> "" Then
                                                                    If Left(DstFilePath, 1) = "\" Then
                                                                        DstFilePath = Mid(DstFilePath, 2)
                                                                    End If
                                                                    If Right(DstFilePath, 1) <> "\" Then
                                                                        DstFilePath = DstFilePath & "\"
                                                                    End If
                                                                End If

                                                                Select Case resourceType.ToLower()
                                                                    Case "www"
                                                                        wwwFileList = wwwFileList & vbCrLf & DstFilePath & filename
                                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, copying file to www, src [" & CollectionVersionFolder & SrcPath & "], dst [" & cpCore.serverConfig.appConfig.appRootFilesPath & DstFilePath & "].")
                                                                        'Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, copying file to www, src [" & CollectionVersionFolder & SrcPath & "], dst [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & "].")
                                                                        Call cpCore.privateFiles.copyFile(CollectionVersionFolder & SrcPath & filename, DstFilePath & filename, cpCore.appRootFiles)
                                                                        If genericController.vbLCase(Right(filename, 4)) = ".zip" Then
                                                                            Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, unzipping www file [" & cpCore.serverConfig.appConfig.appRootFilesPath & DstFilePath & filename & "].")
                                                                            'Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, unzipping www file [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & Filename & "].")
                                                                            Call cpCore.appRootFiles.UnzipFile(DstFilePath & filename)
                                                                        End If
                                                                    Case "file", "content"
                                                                        ContentFileList = ContentFileList & vbCrLf & DstFilePath & filename
                                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, copying file to content, src [" & CollectionVersionFolder & SrcPath & "], dst [" & DstFilePath & "].")
                                                                        Call cpCore.privateFiles.copyFile(CollectionVersionFolder & SrcPath & filename, DstFilePath & filename, cpCore.cdnFiles)
                                                                        If genericController.vbLCase(Right(filename, 4)) = ".zip" Then
                                                                            Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, unzipping content file [" & DstFilePath & filename & "].")
                                                                            Call cpCore.cdnFiles.UnzipFile(DstFilePath & filename)
                                                                        End If
                                                                    Case Else
                                                                        If (assembliesInZip.Contains(filename.ToLower())) Then
                                                                            assembliesInZip.Remove(filename.ToLower())
                                                                        End If
                                                                        ExecFileList = ExecFileList & vbCrLf & filename
                                                                End Select
                                                            Case "getcollection", "importcollection"
                                                                '
                                                                ' Get path to this collection and call into it
                                                                '
                                                                Dim Found As Boolean = False
                                                                Dim ChildCollectionName As String = GetXMLAttribute(cpCore, Found, CDefSection, "name", "")
                                                                Dim ChildCollectionGUID As String = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText)
                                                                If ChildCollectionGUID = "" Then
                                                                    ChildCollectionGUID = CDefSection.InnerText
                                                                End If
                                                                If (InStr(1, ImportFromCollectionsGuidList & "," & CollectionGuid, ChildCollectionGUID, vbTextCompare) <> 0) Then
                                                                    '
                                                                    ' circular import detected, this collection is already imported
                                                                    '
                                                                    Call logController.appendInstallLog(cpCore, "Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1. The collection to be imported is [" & ChildCollectionName & "], GUID [" & ChildCollectionGUID & "]")
                                                                Else
                                                                    Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 1, import collection found, name [" & ChildCollectionName & "], guid [" & ChildCollectionGUID & "]")
                                                                    If True Then
                                                                        Call installCollectionFromRemoteRepo(cpCore, ChildCollectionGUID, return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, nonCriticalErrorList)
                                                                    Else
                                                                        If ChildCollectionGUID = "" Then
                                                                            Call logController.appendInstallLog(cpCore, "The importcollection node [" & ChildCollectionName & "] can not be upgraded because it does not include a valid guid.")
                                                                        Else
                                                                            '
                                                                            ' This import occurred while upgrading an application from the local collections (Db upgrade or AddonManager)
                                                                            ' Its OK to install it if it is missing, but you do not need to upgrade the local collections from the Library
                                                                            '
                                                                            ' 5/18/2008 -----------------------------------
                                                                            ' See if it is in the local collections storage. If yes, just upgrade this app with it. If not,
                                                                            ' it must be downloaded and the entire server must be upgraded
                                                                            '
                                                                            Dim ChildCollectionVersionFolderName As String = ""
                                                                            Dim ChildCollectionLastChangeDate As Date = Nothing
                                                                            Call GetCollectionConfig(cpCore, ChildCollectionGUID, ChildCollectionVersionFolderName, ChildCollectionLastChangeDate, "")
                                                                            If ChildCollectionVersionFolderName <> "" Then
                                                                                '
                                                                                ' It is installed in the local collections, update just this site
                                                                                '
                                                                                result = result And installCollectionFromLocalRepo(cpCore, ChildCollectionGUID, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, ImportFromCollectionsGuidList & "," & CollectionGuid, IsNewBuild, nonCriticalErrorList)
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                        End Select
                                                    Next
                                                    '
                                                    ' -- any assemblies found in the zip that were not part of the resources section need to be added
                                                    For Each filename As String In assembliesInZip
                                                        ExecFileList = ExecFileList & vbCrLf & filename
                                                    Next
                                                    '
                                                    '-------------------------------------------------------------------------------
                                                    ' create an Add-on Collection record
                                                    '-------------------------------------------------------------------------------
                                                    '
                                                    Dim OKToInstall As Boolean = False
                                                    Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 1 done, create collection record.")
                                                    Dim collection As Models.Entity.AddonCollectionModel = Models.Entity.AddonCollectionModel.create(cpCore, CollectionGuid)
                                                    If (collection IsNot Nothing) Then
                                                        '
                                                        ' Upgrade addon
                                                        '
                                                        If CollectionLastChangeDate = Date.MinValue Then
                                                            Call logController.appendInstallLog(cpCore, "collection [" & Collectionname & "], GUID [" & CollectionGuid & "], App has the collection, but the new version has no lastchangedate, so it will upgrade to this unknown (manual) version.")
                                                            OKToInstall = True
                                                        ElseIf (collection.LastChangeDate < CollectionLastChangeDate) Then
                                                            Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], App has an older version of collection. It will be upgraded.")
                                                            OKToInstall = True
                                                        Else
                                                            Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.")
                                                            OKToInstall = False
                                                        End If
                                                    Else
                                                        '
                                                        ' Install new on this application
                                                        '
                                                        collection = Models.Entity.AddonCollectionModel.add(cpCore)
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], App does not have this collection so it will be installed.")
                                                        OKToInstall = True
                                                    End If
                                                    Dim DataRecordList As String = ""
                                                    If Not OKToInstall Then
                                                        '
                                                        ' Do not install, but still check all imported collections to see if they need to be installed
                                                        ' imported collections moved in front this check
                                                        '
                                                    Else
                                                        '
                                                        ' ----- gather help nodes
                                                        '
                                                        Dim CollectionHelpLink As String = ""
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            If (CDefSection.Name.ToLower() = "helplink") Then
                                                                '
                                                                ' only save the first
                                                                CollectionHelpLink = CDefSection.InnerText
                                                                Exit For
                                                            End If
                                                        Next
                                                        '
                                                        ' ----- set or clear all fields
                                                        collection.name = Collectionname
                                                        collection.Help = ""
                                                        collection.ccguid = CollectionGuid
                                                        collection.LastChangeDate = CollectionLastChangeDate
                                                        If CollectionSystem_fileValueOK Then
                                                            collection.System = CollectionSystem
                                                        End If
                                                        If CollectionUpdatable_fileValueOK Then
                                                            collection.Updatable = CollectionUpdatable
                                                        End If
                                                        If CollectionblockNavigatorNode_fileValueOK Then
                                                            collection.blockNavigatorNode = CollectionblockNavigatorNode
                                                        End If
                                                        collection.helpLink = CollectionHelpLink
                                                        '
                                                        Call cpCore.db.deleteContentRecords("Add-on Collection CDef Rules", "CollectionID=" & collection.id)
                                                        Call cpCore.db.deleteContentRecords("Add-on Collection Parent Rules", "ParentID=" & collection.id)
                                                        '
                                                        ' Store all resource found, new way and compatibility way
                                                        '
                                                        collection.ContentFileList = ContentFileList
                                                        collection.ExecFileList = ExecFileList
                                                        collection.wwwFileList = wwwFileList
                                                        '
                                                        ' ----- remove any current navigator nodes installed by the collection previously
                                                        '
                                                        If (collection.id <> 0) Then
                                                            Call cpCore.db.deleteContentRecords(cnNavigatorEntries, "installedbycollectionid=" & collection.id)
                                                        End If
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Pass 2
                                                        ' Go through all collection nodes
                                                        ' Process all cdef related nodes to the old upgrade
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        Dim CollectionWrapper As String = ""
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 2")
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            Select Case genericController.vbLCase(CDefSection.Name)
                                                                Case "contensivecdef"
                                                                    '
                                                                    ' old cdef xection -- take the inner
                                                                    '
                                                                    For Each ChildNode As XmlNode In CDefSection.ChildNodes
                                                                        CollectionWrapper &= vbCrLf & ChildNode.OuterXml
                                                                    Next
                                                                Case "cdef", "sqlindex", "style", "styles", "stylesheet", "adminmenu", "menuentry", "navigatorentry"
                                                                    '
                                                                    ' handled by Upgrade class
                                                                    CollectionWrapper &= CDefSection.OuterXml
                                                            End Select
                                                        Next
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Post Pass 2
                                                        ' if cdef were found, import them now
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        If CollectionWrapper <> "" Then
                                                            '
                                                            ' -- Use the upgrade code to import this part
                                                            CollectionWrapper = "<" & CollectionFileRootNode & ">" & CollectionWrapper & "</" & CollectionFileRootNode & ">"
                                                            Dim isBaseCollection As Boolean = (baseCollectionGuid = CollectionGuid)
                                                            Call installCollectionFromLocalRepo_BuildDbFromXmlData(cpCore, CollectionWrapper, IsNewBuild, isBaseCollection, nonCriticalErrorList)
                                                            '
                                                            ' -- Process nodes to save Collection data
                                                            Dim NavDoc As New XmlDocument
                                                            loadOK = True
                                                            Try
                                                                NavDoc.LoadXml(CollectionWrapper)
                                                            Catch ex As Exception
                                                                '
                                                                ' error - Need a way to reach the user that submitted the file
                                                                '
                                                                Call logController.appendInstallLog(cpCore, "Creating navigator entries, there was an error parsing the portion of the collection that contains cdef. Navigator entry creation was aborted. [There was an error reading the Meta data file.]")
                                                                result = False
                                                                return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the xml collection file has an error.</P>"
                                                                loadOK = False
                                                            End Try
                                                            If loadOK Then
                                                                With NavDoc.DocumentElement
                                                                    For Each CDefNode As XmlNode In .ChildNodes
                                                                        Select Case genericController.vbLCase(CDefNode.Name)
                                                                            Case "cdef"
                                                                                Dim ContentName As String = GetXMLAttribute(cpCore, IsFound, CDefNode, "name", "")
                                                                                '
                                                                                ' setup cdef rule
                                                                                '
                                                                                Dim ContentID As Integer = cpCore.metaData.getContentId(ContentName)
                                                                                If ContentID > 0 Then
                                                                                    Dim CS As Integer = cpCore.db.csInsertRecord("Add-on Collection CDef Rules", 0)
                                                                                    If cpCore.db.csOk(CS) Then
                                                                                        Call cpCore.db.csSet(CS, "Contentid", ContentID)
                                                                                        Call cpCore.db.csSet(CS, "CollectionID", collection.id)
                                                                                    End If
                                                                                    Call cpCore.db.csClose(CS)
                                                                                End If
                                                                        End Select
                                                                    Next
                                                                End With
                                                            End If
                                                        End If
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Pass3
                                                        ' create any data records
                                                        '
                                                        '   process after cdef builds
                                                        '   process seperate so another pass can create any lookup data from these records
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 3")
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            Select Case genericController.vbLCase(CDefSection.Name)
                                                                Case "data"
                                                                    '
                                                                    ' import content
                                                                    '   This can only be done with matching guid
                                                                    '
                                                                    For Each ContentNode As XmlNode In CDefSection.ChildNodes
                                                                        If genericController.vbLCase(ContentNode.Name) = "record" Then
                                                                            '
                                                                            ' Data.Record node
                                                                            '
                                                                            Dim ContentName As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "")
                                                                            If ContentName = "" Then
                                                                                Call logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.")
                                                                                result = False
                                                                                return_ErrorMessage = return_ErrorMessage & "<P>Collection file contains a data.record node with a blank content attribute.</P>"
                                                                            Else
                                                                                Dim ContentRecordGuid As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "")
                                                                                Dim ContentRecordName As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "")
                                                                                If (ContentRecordGuid = "") And (ContentRecordName = "") Then
                                                                                    Call logController.appendInstallLog(cpCore, "install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" & ContentName & "]")
                                                                                    result = False
                                                                                    return_ErrorMessage = return_ErrorMessage & "<P>The collection was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" & ContentName & "].</P>"
                                                                                Else
                                                                                    '
                                                                                    ' create or update the record
                                                                                    '
                                                                                    Dim CDef As Models.Complex.cdefModel = cpCore.metaData.getCdef(ContentName)
                                                                                    Dim cs As Integer = -1
                                                                                    If ContentRecordGuid <> "" Then
                                                                                        cs = cpCore.db.csOpen(ContentName, "ccguid=" & cpCore.db.encodeSQLText(ContentRecordGuid))
                                                                                    Else
                                                                                        cs = cpCore.db.csOpen(ContentName, "name=" & cpCore.db.encodeSQLText(ContentRecordName))
                                                                                    End If
                                                                                    Dim recordfound As Boolean = True
                                                                                    If Not cpCore.db.csOk(cs) Then
                                                                                        '
                                                                                        ' Insert the new record
                                                                                        '
                                                                                        recordfound = False
                                                                                        Call cpCore.db.csClose(cs)
                                                                                        cs = cpCore.db.csInsertRecord(ContentName, 0)
                                                                                    End If
                                                                                    If cpCore.db.csOk(cs) Then
                                                                                        '
                                                                                        ' Update the record
                                                                                        '
                                                                                        If recordfound And (ContentRecordGuid <> "") Then
                                                                                            '
                                                                                            ' found by guid, use guid in list and save name
                                                                                            '
                                                                                            Call cpCore.db.csSet(cs, "name", ContentRecordName)
                                                                                            DataRecordList = DataRecordList & vbCrLf & ContentName & "," & ContentRecordGuid
                                                                                        ElseIf recordfound Then
                                                                                            '
                                                                                            ' record found by name, use name is list but do not add guid
                                                                                            '
                                                                                            DataRecordList = DataRecordList & vbCrLf & ContentName & "," & ContentRecordName
                                                                                        Else
                                                                                            '
                                                                                            ' record was created
                                                                                            '
                                                                                            Call cpCore.db.csSet(cs, "ccguid", ContentRecordGuid)
                                                                                            Call cpCore.db.csSet(cs, "name", ContentRecordName)
                                                                                            DataRecordList = DataRecordList & vbCrLf & ContentName & "," & ContentRecordGuid
                                                                                        End If
                                                                                    End If
                                                                                    Call cpCore.db.csClose(cs)
                                                                                End If
                                                                            End If
                                                                        End If
                                                                    Next
                                                            End Select
                                                        Next
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Pass 4, process fields in data nodes
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 4")
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            Select Case genericController.vbLCase(CDefSection.Name)
                                                                Case "data"
                                                                    '
                                                                    ' import content
                                                                    '   This can only be done with matching guid
                                                                    '
                                                                    'OtherXML = OtherXML & vbCrLf & CDefSection.xml
                                                                    '
                                                                    For Each ContentNode As XmlNode In CDefSection.ChildNodes
                                                                        If genericController.vbLCase(ContentNode.Name) = "record" Then
                                                                            '
                                                                            ' Data.Record node
                                                                            '
                                                                            Dim ContentName As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "")
                                                                            If ContentName = "" Then
                                                                                Call logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.")
                                                                                result = False
                                                                                return_ErrorMessage = return_ErrorMessage & "<P>Collection file contains a data.record node with a blank content attribute.</P>"
                                                                            Else
                                                                                Dim ContentRecordGuid As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "")
                                                                                Dim ContentRecordName As String = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "")
                                                                                If (ContentRecordGuid <> "") Or (ContentRecordName <> "") Then
                                                                                    Dim CDef As Models.Complex.cdefModel = cpCore.metaData.getCdef(ContentName)
                                                                                    Dim cs As Integer = -1
                                                                                    If ContentRecordGuid <> "" Then
                                                                                        cs = cpCore.db.csOpen(ContentName, "ccguid=" & cpCore.db.encodeSQLText(ContentRecordGuid))
                                                                                    Else
                                                                                        cs = cpCore.db.csOpen(ContentName, "name=" & cpCore.db.encodeSQLText(ContentRecordName))
                                                                                    End If
                                                                                    If cpCore.db.csOk(cs) Then
                                                                                        '
                                                                                        ' Update the record
                                                                                        For Each FieldNode As XmlNode In ContentNode.ChildNodes
                                                                                            If FieldNode.Name.ToLower() = "field" Then
                                                                                                Dim IsFieldFound As Boolean = False
                                                                                                Dim FieldName As String = GetXMLAttribute(cpCore, IsFound, FieldNode, "name", "").ToLower()
                                                                                                Dim fieldTypeId As Integer = -1
                                                                                                Dim FieldLookupContentID As Integer = -1
                                                                                                For Each keyValuePair In CDef.fields
                                                                                                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                                                                                                    If genericController.vbLCase(field.nameLc) = FieldName Then
                                                                                                        fieldTypeId = field.fieldTypeId
                                                                                                        FieldLookupContentID = field.lookupContentID
                                                                                                        IsFieldFound = True
                                                                                                        Exit For
                                                                                                    End If
                                                                                                Next
                                                                                                'For Ptr = 0 To CDef.fields.count - 1
                                                                                                '    CDefField = CDef.fields(Ptr)
                                                                                                '    If genericController.vbLCase(CDefField.Name) = FieldName Then
                                                                                                '        fieldType = CDefField.fieldType
                                                                                                '        FieldLookupContentID = CDefField.LookupContentID
                                                                                                '        IsFieldFound = True
                                                                                                '        Exit For
                                                                                                '    End If
                                                                                                'Next
                                                                                                If IsFieldFound Then
                                                                                                    Dim FieldValue As String = FieldNode.InnerText
                                                                                                    Select Case fieldTypeId
                                                                                                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect
                                                                                                                        '
                                                                                                                        ' not supported
                                                                                                                        '
                                                                                                        Case FieldTypeIdLookup
                                                                                                            '
                                                                                                            ' read in text value, if a guid, use it, otherwise assume name
                                                                                                            '
                                                                                                            If FieldLookupContentID <> 0 Then
                                                                                                                Dim FieldLookupContentName As String = cpCore.metaData.getContentNameByID(FieldLookupContentID)
                                                                                                                If FieldLookupContentName <> "" Then
                                                                                                                    If (Left(FieldValue, 1) = "{") And (Right(FieldValue, 1) = "}") And cpCore.metaData.isContentFieldSupported(FieldLookupContentName, "ccguid") Then
                                                                                                                        '
                                                                                                                        ' Lookup by guid
                                                                                                                        '
                                                                                                                        Dim fieldLookupId As Integer = genericController.EncodeInteger(cpCore.db.GetRecordIDByGuid(FieldLookupContentName, FieldValue))
                                                                                                                        If fieldLookupId <= 0 Then
                                                                                                                            return_ErrorMessage = return_ErrorMessage & "<P>Warning: There was a problem translating field [" & FieldName & "] in record [" & ContentName & "] because the record it refers to was not found in this site.</P>"
                                                                                                                        Else
                                                                                                                            Call cpCore.db.csSet(cs, FieldName, fieldLookupId)
                                                                                                                        End If
                                                                                                                    Else
                                                                                                                        '
                                                                                                                        ' lookup by name
                                                                                                                        '
                                                                                                                        If FieldValue <> "" Then
                                                                                                                            Dim fieldLookupId As Integer = cpCore.db.getRecordID(FieldLookupContentName, FieldValue)
                                                                                                                            If fieldLookupId <= 0 Then
                                                                                                                                return_ErrorMessage = return_ErrorMessage & "<P>Warning: There was a problem translating field [" & FieldName & "] in record [" & ContentName & "] because the record it refers to was not found in this site.</P>"
                                                                                                                            Else
                                                                                                                                Call cpCore.db.csSet(cs, FieldName, fieldLookupId)
                                                                                                                            End If
                                                                                                                        End If
                                                                                                                    End If
                                                                                                                End If
                                                                                                            ElseIf (genericController.vbIsNumeric(FieldValue)) Then
                                                                                                                '
                                                                                                                ' must be lookup list
                                                                                                                '
                                                                                                                Call cpCore.db.csSet(cs, FieldName, FieldValue)
                                                                                                            End If
                                                                                                        Case Else
                                                                                                            Call cpCore.db.csSet(cs, FieldName, FieldValue)
                                                                                                    End Select
                                                                                                End If
                                                                                            End If
                                                                                        Next
                                                                                    End If
                                                                                    Call cpCore.db.csClose(cs)
                                                                                End If
                                                                            End If
                                                                        End If
                                                                    Next
                                                            End Select
                                                        Next
                                                        ' --- end of pass
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Pass 5, all other collection nodes
                                                        '
                                                        ' Process all non-import <Collection> nodes
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 5")
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            Select Case genericController.vbLCase(CDefSection.Name)
                                                                Case "cdef", "data", "help", "resource", "helplink"
                                                                            '
                                                                            ' ignore - processed in previous passes
                                                                Case "getcollection", "importcollection"
                                                                    '
                                                                    ' processed, but add rule for collection record
                                                                    Dim Found As Boolean = False
                                                                    Dim ChildCollectionName As String = GetXMLAttribute(cpCore, Found, CDefSection, "name", "")
                                                                    Dim ChildCollectionGUID As String = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText)
                                                                    If ChildCollectionGUID = "" Then
                                                                        ChildCollectionGUID = CDefSection.InnerText
                                                                    End If
                                                                    If ChildCollectionGUID <> "" Then
                                                                        Dim ChildCollectionID As Integer = 0
                                                                        Dim cs As Integer = -1
                                                                        cs = cpCore.db.csOpen("Add-on Collections", "ccguid=" & cpCore.db.encodeSQLText(ChildCollectionGUID), , , , , , "id")
                                                                        If cpCore.db.csOk(cs) Then
                                                                            ChildCollectionID = cpCore.db.csGetInteger(cs, "id")
                                                                        End If
                                                                        Call cpCore.db.csClose(cs)
                                                                        If ChildCollectionID <> 0 Then
                                                                            cs = cpCore.db.csInsertRecord("Add-on Collection Parent Rules", 0)
                                                                            If cpCore.db.csOk(cs) Then
                                                                                Call cpCore.db.csSet(cs, "ParentID", collection.id)
                                                                                Call cpCore.db.csSet(cs, "ChildID", ChildCollectionID)
                                                                            End If
                                                                            Call cpCore.db.csClose(cs)
                                                                        End If
                                                                    End If
                                                                Case "scriptingmodule", "scriptingmodules"
                                                                    result = False
                                                                    return_ErrorMessage = return_ErrorMessage & "<P>Collection includes a scripting module which is no longer supported. Move scripts to the code tab.</P>"
                                                                            '    '
                                                                            '    ' Scripting modules
                                                                            '    '
                                                                            '    ScriptingModuleID = 0
                                                                            '    ScriptingName = GetXMLAttribute(cpcore,IsFound, CDefSection, "name", "No Name")
                                                                            '    If ScriptingName = "" Then
                                                                            '        ScriptingName = "No Name"
                                                                            '    End If
                                                                            '    ScriptingGuid = GetXMLAttribute(cpcore,IsFound, CDefSection, "guid", AOName)
                                                                            '    If ScriptingGuid = "" Then
                                                                            '        ScriptingGuid = ScriptingName
                                                                            '    End If
                                                                            '    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(ScriptingGuid) & ")"
                                                                            '    ScriptingModuleID = 0
                                                                            '    CS = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                                            '    If cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' Update the Addon
                                                                            '        '
                                                                            '        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing scripting module, Updating module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                            '    Else
                                                                            '        '
                                                                            '        ' not found by GUID - search name against name to update legacy Add-ons
                                                                            '        '
                                                                            '        Call cpCore.db.cs_Close(CS)
                                                                            '        Criteria = "(name=" & cpCore.db.encodeSQLText(ScriptingName) & ")and(ccguid is null)"
                                                                            '        CS = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                                            '        If cpCore.db.cs_ok(CS) Then
                                                                            '            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Scripting Module matched an existing Module that has no GUID, Updating to [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                            '        End If
                                                                            '    End If
                                                                            '    If Not cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' not found by GUID or by name, Insert a new
                                                                            '        '
                                                                            '        Call cpCore.db.cs_Close(CS)
                                                                            '        CS = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                                            '        If cpCore.db.cs_ok(CS) Then
                                                                            '            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                            '        End If
                                                                            '    End If
                                                                            '    If Not cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' Could not create new
                                                                            '        '
                                                                            '        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Scripting Module could not be created, skipping Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                            '    Else
                                                                            '        ScriptingModuleID = cpCore.db.cs_getInteger(CS, "ID")
                                                                            '        Call cpCore.db.cs_set(CS, "code", CDefSection.InnerText)
                                                                            '        Call cpCore.db.cs_set(CS, "name", ScriptingName)
                                                                            '        Call cpCore.db.cs_set(CS, "ccguid", ScriptingGuid)
                                                                            '    End If
                                                                            '    Call cpCore.db.cs_Close(CS)
                                                                            '    If ScriptingModuleID <> 0 Then
                                                                            '        '
                                                                            '        ' Add Add-on Collection Module Rule
                                                                            '        '
                                                                            '        CS = cpCore.db.cs_insertRecord("Add-on Collection Module Rules", 0)
                                                                            '        If cpCore.db.cs_ok(CS) Then
                                                                            '            Call cpCore.db.cs_set(CS, "Collectionid", CollectionID)
                                                                            '            Call cpCore.db.cs_set(CS, "ScriptingModuleID", ScriptingModuleID)
                                                                            '        End If
                                                                            '        Call cpCore.db.cs_Close(CS)
                                                                            '    End If
                                                                Case "sharedstyle"
                                                                    result = False
                                                                    return_ErrorMessage = return_ErrorMessage & "<P>Collection includes a shared style which is no longer supported. Move styles to the default styles tab.</P>"

                                                                            '    '
                                                                            '    ' added 9/3/2012
                                                                            '    ' Shared Style
                                                                            '    '
                                                                            '    sharedStyleId = 0
                                                                            '    NodeName = GetXMLAttribute(cpcore,IsFound, CDefSection, "name", "No Name")
                                                                            '    If NodeName = "" Then
                                                                            '        NodeName = "No Name"
                                                                            '    End If
                                                                            '    nodeGuid = GetXMLAttribute(cpcore,IsFound, CDefSection, "guid", AOName)
                                                                            '    If nodeGuid = "" Then
                                                                            '        nodeGuid = NodeName
                                                                            '    End If
                                                                            '    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(nodeGuid) & ")"
                                                                            '    ScriptingModuleID = 0
                                                                            '    CS = cpCore.db.cs_open("Shared Styles", Criteria)
                                                                            '    If cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' Update the Addon
                                                                            '        '
                                                                            '        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing shared style, Updating [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                            '    Else
                                                                            '        '
                                                                            '        ' not found by GUID - search name against name to update legacy Add-ons
                                                                            '        '
                                                                            '        Call cpCore.db.cs_Close(CS)
                                                                            '        Criteria = "(name=" & cpCore.db.encodeSQLText(NodeName) & ")and(ccguid is null)"
                                                                            '        CS = cpCore.db.cs_open("shared styles", Criteria)
                                                                            '        If cpCore.db.cs_ok(CS) Then
                                                                            '            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, shared style matched an existing Module that has no GUID, Updating to [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                            '        End If
                                                                            '    End If
                                                                            '    If Not cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' not found by GUID or by name, Insert a new
                                                                            '        '
                                                                            '        Call cpCore.db.cs_Close(CS)
                                                                            '        CS = cpCore.db.cs_insertRecord("shared styles", 0)
                                                                            '        If cpCore.db.cs_ok(CS) Then
                                                                            '            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                            '        End If
                                                                            '    End If
                                                                            '    If Not cpCore.db.cs_ok(CS) Then
                                                                            '        '
                                                                            '        ' Could not create new
                                                                            '        '
                                                                            '        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, shared style could not be created, skipping shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                            '    Else
                                                                            '        sharedStyleId = cpCore.db.cs_getInteger(CS, "ID")
                                                                            '        Call cpCore.db.cs_set(CS, "StyleFilename", CDefSection.InnerText)
                                                                            '        Call cpCore.db.cs_set(CS, "name", NodeName)
                                                                            '        Call cpCore.db.cs_set(CS, "ccguid", nodeGuid)
                                                                            '        Call cpCore.db.cs_set(CS, "alwaysInclude", GetXMLAttribute(cpcore,IsFound, CDefSection, "alwaysinclude", "0"))
                                                                            '        Call cpCore.db.cs_set(CS, "prefix", GetXMLAttribute(cpcore,IsFound, CDefSection, "prefix", ""))
                                                                            '        Call cpCore.db.cs_set(CS, "suffix", GetXMLAttribute(cpcore,IsFound, CDefSection, "suffix", ""))
                                                                            '        Call cpCore.db.cs_set(CS, "suffix", GetXMLAttribute(cpcore,IsFound, CDefSection, "suffix", ""))
                                                                            '        Call cpCore.db.cs_set(CS, "sortOrder", GetXMLAttribute(cpcore,IsFound, CDefSection, "sortOrder", ""))
                                                                            '    End If
                                                                            '    Call cpCore.db.cs_Close(CS)
                                                                Case "addon", "add-on"
                                                                    '
                                                                    ' Add-on Node, do part 1 of 2
                                                                    '   (include add-on node must be done after all add-ons are installed)
                                                                    '
                                                                    Call InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore, CDefSection, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage)
                                                                    If Not result Then
                                                                        result = result
                                                                    End If
                                                                Case "interfaces"
                                                                    '
                                                                    ' Legacy Interface Node
                                                                    '
                                                                    For Each CDefInterfaces As XmlNode In CDefSection.ChildNodes
                                                                        Call InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage)
                                                                        If Not result Then
                                                                            result = result
                                                                        End If
                                                                    Next
                                                                    'Case "otherxml", "importcollection", "sqlindex", "style", "styles", "stylesheet", "adminmenu", "menuentry", "navigatorentry"
                                                                    '    '
                                                                    '    ' otherxml
                                                                    '    '
                                                                    '    If genericController.vbLCase(CDefSection.OuterXml) <> "<otherxml></otherxml>" Then
                                                                    '        OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                    '    End If
                                                                    '    'Case Else
                                                                    '    '    '
                                                                    '    '    ' Unknown node in collection file
                                                                    '    '    '
                                                                    '    '    OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                    '    '    Call logcontroller.appendInstallLog(cpCore, "Addon Collection for [" & Collectionname & "] contained an unknown node [" & CDefSection.Name & "]. This node will be ignored.")
                                                            End Select
                                                        Next
                                                        '
                                                        ' --- end of pass
                                                        '
                                                        '-------------------------------------------------------------------------------
                                                        ' ----- Pass 6
                                                        '
                                                        ' process include add-on node of add-on nodes
                                                        '-------------------------------------------------------------------------------
                                                        '
                                                        Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], pass 6")
                                                        For Each CDefSection As XmlNode In .ChildNodes
                                                            Select Case genericController.vbLCase(CDefSection.Name)
                                                                Case "addon", "add-on"
                                                                    '
                                                                    ' Add-on Node, do part 1 of 2
                                                                    '   (include add-on node must be done after all add-ons are installed)
                                                                    '
                                                                    Call InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefSection, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage)
                                                                    If Not result Then
                                                                        result = result
                                                                    End If
                                                                Case "interfaces"
                                                                    '
                                                                    ' Legacy Interface Node
                                                                    '
                                                                    For Each CDefInterfaces As XmlNode In CDefSection.ChildNodes
                                                                        Call InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage)
                                                                        If Not result Then
                                                                            result = result
                                                                        End If
                                                                    Next
                                                            End Select
                                                        Next
                                                        '
                                                        ' --- end of pass
                                                        '
                                                    End If
                                                    collection.DataRecordList = DataRecordList
                                                    collection.save(cpCore)
                                                End If
                                                '
                                                Call logController.appendInstallLog(cpCore, "install collection [" & Collectionname & "], upgrade complete, flush cache")
                                                '
                                                ' -- import complete, flush caches
                                                Call cpCore.cache.invalidateAll()
                                            End If
                                        End If
                                    End With
                                End If
                            End If
                        Next
                    End If
                End If
            Catch ex As Exception
                '
                ' Log error and exit with failure. This way any other upgrading will still continue
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return the collectionList file stored in the root of the addon folder.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function getCollectionListFile(cpCore As coreClass) As String
            Dim returnXml As String = ""
            Try
                Dim LastChangeDate As String = String.Empty
                Dim SubFolder As IO.DirectoryInfo
                Dim SubFolderList As IO.DirectoryInfo()
                Dim FolderName As String
                Dim collectionFilePathFilename As String
                Dim CollectionGuid As String
                Dim Collectionname As String
                Dim Pos As Integer
                Dim FolderList As IO.DirectoryInfo()
                '
                collectionFilePathFilename = cpCore.addon.getPrivateFilesAddonPath & "Collections.xml"
                returnXml = cpCore.privateFiles.readFile(collectionFilePathFilename)
                If returnXml = "" Then
                    FolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath)
                    If FolderList.Count > 0 Then
                        For Each folder As IO.DirectoryInfo In FolderList
                            FolderName = folder.Name
                            Pos = genericController.vbInstr(1, FolderName, vbTab)
                            If Pos > 1 Then
                                'hint = hint & ",800"
                                FolderName = Mid(FolderName, 1, Pos - 1)
                                If Len(FolderName) > 34 Then
                                    If genericController.vbLCase(Left(FolderName, 4)) <> "temp" Then
                                        CollectionGuid = Right(FolderName, 32)
                                        Collectionname = Left(FolderName, Len(FolderName) - Len(CollectionGuid) - 1)
                                        CollectionGuid = Mid(CollectionGuid, 1, 8) & "-" & Mid(CollectionGuid, 9, 4) & "-" & Mid(CollectionGuid, 13, 4) & "-" & Mid(CollectionGuid, 17, 4) & "-" & Mid(CollectionGuid, 21)
                                        CollectionGuid = "{" & CollectionGuid & "}"
                                        SubFolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath() & "\" & FolderName)
                                        If SubFolderList.Count > 0 Then
                                            SubFolder = SubFolderList(SubFolderList.Count - 1)
                                            FolderName = FolderName & "\" & SubFolder.Name
                                            LastChangeDate = Mid(SubFolder.Name, 5, 2) & "/" & Mid(SubFolder.Name, 7, 2) & "/" & Mid(SubFolder.Name, 1, 4)
                                            If Not IsDate(LastChangeDate) Then
                                                LastChangeDate = ""
                                            End If
                                        End If
                                        returnXml = returnXml & vbCrLf & vbTab & "<Collection>"
                                        returnXml = returnXml & vbCrLf & vbTab & vbTab & "<name>" & Collectionname & "</name>"
                                        returnXml = returnXml & vbCrLf & vbTab & vbTab & "<guid>" & CollectionGuid & "</guid>"
                                        returnXml = returnXml & vbCrLf & vbTab & vbTab & "<lastchangedate>" & LastChangeDate & "</lastchangedate>"
                                        returnXml &= vbCrLf & vbTab & vbTab & "<path>" & FolderName & "</path>"
                                        returnXml = returnXml & vbCrLf & vbTab & "</Collection>"
                                    End If
                                End If
                            End If
                        Next
                    End If
                    returnXml = "<CollectionList>" & returnXml & vbCrLf & "</CollectionList>"
                    Call cpCore.privateFiles.saveFile(collectionFilePathFilename, returnXml)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnXml
        End Function
        '
        '
        '
        Private Shared Sub UpdateConfig(cpCore As coreClass, ByVal Collectionname As String, ByVal CollectionGuid As String, ByVal CollectionUpdatedDate As Date, ByVal CollectionVersionFolderName As String)
            Try
                '
                Dim loadOK As Boolean = True
                Dim LocalFilename As String
                Dim LocalGuid As String
                Dim Doc As New XmlDocument
                Dim CollectionNode As XmlNode
                Dim LocalListNode As XmlNode
                Dim NewCollectionNode As XmlNode
                Dim NewAttrNode As XmlNode
                Dim CollectionFound As Boolean
                '
                loadOK = True
                Try
                    Call Doc.LoadXml(getCollectionListFile(cpCore))
                Catch ex As Exception
                    Call logController.appendInstallLog(cpCore, "UpdateConfig, Error loading Collections.xml file.")
                End Try
                If loadOK Then
                    If genericController.vbLCase(Doc.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
                        Call logController.appendInstallLog(cpCore, "UpdateConfig, The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.")
                    Else
                        With Doc.DocumentElement
                            If genericController.vbLCase(.Name) = "collectionlist" Then
                                CollectionFound = False
                                For Each LocalListNode In .ChildNodes
                                    Select Case genericController.vbLCase(LocalListNode.Name)
                                        Case "collection"
                                            LocalGuid = ""
                                            For Each CollectionNode In LocalListNode.ChildNodes
                                                Select Case genericController.vbLCase(CollectionNode.Name)
                                                    Case "guid"
                                                        '
                                                        LocalGuid = genericController.vbLCase(CollectionNode.InnerText)
                                                        Exit For
                                                End Select
                                            Next
                                            If genericController.vbLCase(LocalGuid) = genericController.vbLCase(CollectionGuid) Then
                                                CollectionFound = True
                                                For Each CollectionNode In LocalListNode.ChildNodes
                                                    Select Case genericController.vbLCase(CollectionNode.Name)
                                                        Case "name"
                                                            CollectionNode.InnerText = Collectionname
                                                        Case "lastchangedate"
                                                            CollectionNode.InnerText = CollectionUpdatedDate.ToString()
                                                        Case "path"
                                                            CollectionNode.InnerText = CollectionVersionFolderName
                                                    End Select
                                                Next
                                                Exit For
                                            End If
                                    End Select
                                Next
                                If Not CollectionFound Then
                                    NewCollectionNode = Doc.CreateNode(XmlNodeType.Element, "collection", "")
                                    '
                                    NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "name", "")
                                    NewAttrNode.InnerText = Collectionname
                                    Call NewCollectionNode.AppendChild(NewAttrNode)
                                    '
                                    NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "lastchangedate", "")
                                    NewAttrNode.InnerText = CollectionUpdatedDate.ToString()
                                    Call NewCollectionNode.AppendChild(NewAttrNode)
                                    '
                                    NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "guid", "")
                                    NewAttrNode.InnerText = CollectionGuid
                                    Call NewCollectionNode.AppendChild(NewAttrNode)
                                    '
                                    NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "path", "")
                                    NewAttrNode.InnerText = CollectionVersionFolderName
                                    Call NewCollectionNode.AppendChild(NewAttrNode)
                                    '
                                    Call Doc.DocumentElement.AppendChild(NewCollectionNode)
                                End If
                                '
                                ' Save the result
                                '
                                LocalFilename = cpCore.addon.getPrivateFilesAddonPath() & "Collections.xml"
                                'LocalFilename = GetProgramPath & "\Addons\Collections.xml"
                                Call Doc.Save(cpCore.privateFiles.rootLocalPath & LocalFilename)
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '
        '
        Private Shared Function GetCollectionPath(cpCore As coreClass, ByVal CollectionGuid As String) As String
            Dim result As String = String.Empty
            Try
                Dim LastChangeDate As Date = Nothing
                Dim Collectionname As String = ""
                Call GetCollectionConfig(cpCore, CollectionGuid, result, LastChangeDate, Collectionname)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return the collection path, lastChangeDate, and collectionName given the guid
        ''' </summary>
        ''' <param name="CollectionGuid"></param>
        ''' <param name="return_CollectionPath"></param>
        ''' <param name="return_LastChagnedate"></param>
        ''' <param name="return_CollectionName"></param>
        Public Shared Sub GetCollectionConfig(cpCore As coreClass, ByVal CollectionGuid As String, ByRef return_CollectionPath As String, ByRef return_LastChagnedate As Date, ByRef return_CollectionName As String)
            Try
                Dim LocalPath As String
                Dim LocalGuid As String = String.Empty
                Dim Doc As New XmlDocument
                Dim CollectionNode As XmlNode
                Dim LocalListNode As XmlNode
                Dim CollectionFound As Boolean
                Dim CollectionPath As String = String.Empty
                Dim LastChangeDate As Date
                Dim hint As String = String.Empty
                Dim MatchFound As Boolean
                Dim LocalName As String
                Dim loadOK As Boolean
                '
                MatchFound = False
                return_CollectionPath = ""
                return_LastChagnedate = Date.MinValue
                loadOK = True
                Try
                    Call Doc.LoadXml(getCollectionListFile(cpCore))
                Catch ex As Exception
                    'hint = hint & ",parse error"
                    Call logController.appendInstallLog(cpCore, "GetCollectionConfig, Hint=[" & hint & "], Error loading Collections.xml file.")
                    loadOK = False
                End Try
                If loadOK Then
                    If genericController.vbLCase(Doc.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
                        Call logController.appendInstallLog(cpCore, "Hint=[" & hint & "], The Collections.xml file has an invalid root node")
                    Else
                        With Doc.DocumentElement
                            If True Then
                                'If genericController.vbLCase(.name) <> "collectionlist" Then
                                '    Call AppendClassLogFile("Server", "GetCollectionConfig", "Collections.xml file error, root node was not collectionlist, [" & .name & "].")
                                'Else
                                CollectionFound = False
                                'hint = hint & ",checking nodes [" & .ChildNodes.Count & "]"
                                For Each LocalListNode In .ChildNodes
                                    LocalName = "no name found"
                                    LocalPath = ""
                                    Select Case genericController.vbLCase(LocalListNode.Name)
                                        Case "collection"
                                            LocalGuid = ""
                                            For Each CollectionNode In LocalListNode.ChildNodes
                                                Select Case genericController.vbLCase(CollectionNode.Name)
                                                    Case "name"
                                                        '
                                                        LocalName = genericController.vbLCase(CollectionNode.InnerText)
                                                    Case "guid"
                                                        '
                                                        LocalGuid = genericController.vbLCase(CollectionNode.InnerText)
                                                    Case "path"
                                                        '
                                                        CollectionPath = genericController.vbLCase(CollectionNode.InnerText)
                                                    Case "lastchangedate"
                                                        LastChangeDate = genericController.EncodeDate(CollectionNode.InnerText)
                                                End Select
                                            Next
                                    End Select
                                    'hint = hint & ",checking node [" & LocalName & "]"
                                    If genericController.vbLCase(CollectionGuid) = LocalGuid Then
                                        return_CollectionPath = CollectionPath
                                        return_LastChagnedate = LastChangeDate
                                        return_CollectionName = LocalName
                                        'Call AppendClassLogFile("Server", "GetCollectionConfigArg", "GetCollectionConfig, match found, CollectionName=" & LocalName & ", CollectionPath=" & CollectionPath & ", LastChangeDate=" & LastChangeDate)
                                        MatchFound = True
                                        Exit For
                                    End If
                                Next
                            End If
                        End With
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '======================================================================================================
        ' Installs Addons in a source folder
        '======================================================================================================
        '
        Public Shared Function InstallCollectionsFromPrivateFolder(cpCore As coreClass, ByVal privateFolder As String, ByRef return_ErrorMessage As String, ByRef return_CollectionGUIDList As List(Of String), IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim returnSuccess As Boolean = False
            Try
                Dim CollectionLastChangeDate As Date
                '
                CollectionLastChangeDate = DateTime.Now()
                returnSuccess = BuildLocalCollectionReposFromFolder(cpCore, privateFolder, CollectionLastChangeDate, return_CollectionGUIDList, return_ErrorMessage, False)
                If Not returnSuccess Then
                    '
                    ' BuildLocal failed, log it and do not upgrade
                    '
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" & return_ErrorMessage & "], exiting without calling UpgradeAllAppsFromLocalCollection")
                Else
                    For Each collectionGuid As String In return_CollectionGUIDList
                        If Not installCollectionFromLocalRepo(cpCore, collectionGuid, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, "", IsNewBuild, nonCriticalErrorList) Then
                            Call logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" & return_ErrorMessage & "].")
                            Exit For
                        End If
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
                returnSuccess = False
                If (String.IsNullOrEmpty(return_ErrorMessage)) Then
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" & ex.Message & "]"
                End If
            End Try
            Return returnSuccess
        End Function
        '
        '======================================================================================================
        ' Installs Addons in a source file
        '======================================================================================================
        '
        Public Shared Function InstallCollectionsFromPrivateFile(cpCore As coreClass, ByVal pathFilename As String, ByRef return_ErrorMessage As String, ByRef return_CollectionGUID As String, IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim returnSuccess As Boolean = False
            Try
                Dim CollectionLastChangeDate As Date
                '
                CollectionLastChangeDate = DateTime.Now()
                returnSuccess = BuildLocalCollectionRepoFromFile(cpCore, pathFilename, CollectionLastChangeDate, return_CollectionGUID, return_ErrorMessage, False)
                If Not returnSuccess Then
                    '
                    ' BuildLocal failed, log it and do not upgrade
                    '
                    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" & return_ErrorMessage & "], exiting without calling UpgradeAllAppsFromLocalCollection")
                Else
                    returnSuccess = installCollectionFromLocalRepo(cpCore, return_CollectionGUID, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, "", IsNewBuild, nonCriticalErrorList)
                    If Not returnSuccess Then
                        '
                        ' Upgrade all apps failed
                        '
                        Call logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" & return_ErrorMessage & "].")
                    Else
                        returnSuccess = True
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
                returnSuccess = False
                If (String.IsNullOrEmpty(return_ErrorMessage)) Then
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" & ex.Message & "]"
                End If
            End Try
            Return returnSuccess
        End Function
        '
        '
        '
        Private Shared Function GetNavIDByGuid(cpCore As coreClass, ByVal ccGuid As String) As Integer
            Dim navId As Integer = 0
            Try
                Dim CS As Integer
                '
                CS = cpCore.db.csOpen(cnNavigatorEntries, "ccguid=" & cpCore.db.encodeSQLText(ccGuid), "ID", , , , , "ID")
                If cpCore.db.csOk(CS) Then
                    navId = cpCore.db.csGetInteger(CS, "id")
                End If
                Call cpCore.db.csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return navId
        End Function
        '        '
        '===============================================================================================
        '   copy resources from install folder to www folder
        '       block some file extensions
        '===============================================================================================
        '
        Private Shared Sub CopyInstallToDst(cpCore As coreClass, ByVal SrcPath As String, ByVal DstPath As String, ByVal BlockFileList As String, ByVal BlockFolderList As String)
            Try
                Dim FileInfoArray As IO.FileInfo()
                Dim FolderInfoArray As IO.DirectoryInfo()
                Dim SrcFolder As String
                Dim DstFolder As String
                '
                SrcFolder = SrcPath
                If Right(SrcFolder, 1) = "\" Then
                    SrcFolder = Left(SrcFolder, Len(SrcFolder) - 1)
                End If
                '
                DstFolder = DstPath
                If Right(DstFolder, 1) = "\" Then
                    DstFolder = Left(DstFolder, Len(DstFolder) - 1)
                End If
                '
                If cpCore.privateFiles.pathExists(SrcFolder) Then
                    FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder)
                    For Each file As IO.FileInfo In FileInfoArray
                        If (file.Extension = "dll") Or (file.Extension = "exe") Or (file.Extension = "zip") Then
                            '
                            ' can not copy dll or exe
                            '
                            'Filename = Filename
                        ElseIf (InStr(1, "," & BlockFileList & ",", "," & file.Name & ",", vbTextCompare) <> 0) Then
                            '
                            ' can not copy the current collection file
                            '
                            'file.Name = file.Name
                        Else
                            '
                            ' copy this file to destination
                            '
                            Call cpCore.privateFiles.copyFile(SrcPath & file.Name, DstPath & file.Name, cpCore.appRootFiles)
                        End If
                    Next
                    '
                    ' copy folders to dst
                    '
                    FolderInfoArray = cpCore.privateFiles.getFolderList(SrcFolder)
                    For Each folder As IO.DirectoryInfo In FolderInfoArray
                        If (InStr(1, "," & BlockFolderList & ",", "," & folder.Name & ",", vbTextCompare) = 0) Then
                            Call CopyInstallToDst(cpCore, SrcPath & folder.Name & "\", DstPath & folder.Name & "\", BlockFileList, "")
                        End If
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '
        '
        Private Shared Function GetCollectionFileList(cpCore As coreClass, ByVal SrcPath As String, ByVal SubFolder As String, ByVal ExcludeFileList As String) As String
            Dim result As String = ""
            Try
                Dim FileInfoArray As IO.FileInfo()
                Dim FolderInfoArray As IO.DirectoryInfo()
                Dim SrcFolder As String
                '
                SrcFolder = SrcPath & SubFolder
                If Right(SrcFolder, 1) = "\" Then
                    SrcFolder = Left(SrcFolder, Len(SrcFolder) - 1)
                End If
                '
                If cpCore.privateFiles.pathExists(SrcFolder) Then
                    FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder)
                    For Each file As IO.FileInfo In FileInfoArray
                        If (InStr(1, "," & ExcludeFileList & ",", "," & file.Name & ",", vbTextCompare) <> 0) Then
                            '
                            ' can not copy the current collection file
                            '
                            'Filename = Filename
                        Else
                            '
                            ' copy this file to destination
                            '
                            result = result & vbCrLf & SubFolder & file.Name
                            'runAtServer.IPAddress = "127.0.0.1"
                            'runAtServer.Port = "4531"
                            'QS = "SrcFile=" & encodeRequestVariable(SrcPath & Filename) & "&DstFile=" & encodeRequestVariable(DstPath & Filename)
                            'Call runAtServer.ExecuteCmd("CopyFile", QS)
                            'Call cpCore.app.privateFiles.CopyFile(SrcPath & Filename, DstPath & Filename)
                        End If
                    Next
                    '
                    ' copy folders to dst
                    '
                    FolderInfoArray = cpCore.privateFiles.getFolderList(SrcFolder)
                    For Each folder As IO.DirectoryInfo In FolderInfoArray
                        result = result & GetCollectionFileList(cpCore, SrcPath, SubFolder & folder.Name & "\", ExcludeFileList)
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        '
        '
        Private Shared Sub InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore As coreClass, ByVal AddonNode As XmlNode, ByVal AddonGuidFieldName As String, ByVal ignore_BuildVersion As String, ByVal CollectionID As Integer, ByRef return_UpgradeOK As Boolean, ByRef return_ErrorMessage As String)
            Try
                '
                Dim fieldTypeID As Integer
                Dim fieldType As String
                Dim test As String
                Dim TriggerContentID As Integer
                Dim ContentNameorGuid As String
                Dim navTypeId As Integer
                Dim scriptinglanguageid As Integer
                Dim ScriptingCode As String
                Dim FieldName As String
                Dim NodeName As String
                Dim NewValue As String
                Dim NavIconTypeString As String
                Dim menuNameSpace As String
                Dim FieldValue As String = String.Empty
                Dim CS2 As Integer
                Dim ScriptingNameorGuid As String = String.Empty
                '  Dim ScriptingModuleID As Integer
                Dim ScriptingEntryPoint As String
                Dim ScriptingTimeout As Integer
                Dim ScriptingLanguage As String
                Dim ScriptingNode As XmlNode
                Dim PageInterface As XmlNode
                Dim TriggerNode As XmlNode
                Dim NavDeveloperOnly As Boolean
                Dim StyleSheet As String
                Dim ArgumentList As String
                Dim CS As Integer
                Dim Criteria As String
                Dim IsFound As Boolean
                Dim addonName As String
                Dim addonGuid As String
                Dim navTypeName As String
                Dim addonId As Integer
                Dim Basename As String
                '
                Basename = genericController.vbLCase(AddonNode.Name)
                If (Basename = "page") Or (Basename = "process") Or (Basename = "addon") Or (Basename = "add-on") Then
                    addonName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name")
                    If addonName = "" Then
                        addonName = "No Name"
                    End If
                    addonGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", addonName)
                    If addonGuid = "" Then
                        addonGuid = addonName
                    End If
                    navTypeName = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "")
                    navTypeId = GetListIndex(navTypeName, navTypeIDList)
                    If navTypeId = 0 Then
                        navTypeId = NavTypeIDAddon
                    End If
                    Criteria = "(" & AddonGuidFieldName & "=" & cpCore.db.encodeSQLText(addonGuid) & ")"
                    CS = cpCore.db.csOpen(cnAddons, Criteria, , False)
                    If cpCore.db.csOk(CS) Then
                        '
                        ' Update the Addon
                        '
                        Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" & addonName & "], Guid [" & addonGuid & "]")
                    Else
                        '
                        ' not found by GUID - search name against name to update legacy Add-ons
                        '
                        Call cpCore.db.csClose(CS)
                        Criteria = "(name=" & cpCore.db.encodeSQLText(addonName) & ")and(" & AddonGuidFieldName & " is null)"
                        CS = cpCore.db.csOpen(cnAddons, Criteria, , False)
                        If cpCore.db.csOk(CS) Then
                            Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" & addonName & "], Guid [" & addonGuid & "]")
                        End If
                    End If
                    If Not cpCore.db.csOk(CS) Then
                        '
                        ' not found by GUID or by name, Insert a new addon
                        '
                        Call cpCore.db.csClose(CS)
                        CS = cpCore.db.csInsertRecord(cnAddons, 0)
                        If cpCore.db.csOk(CS) Then
                            Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new Add-on [" & addonName & "], Guid [" & addonGuid & "]")
                        End If
                    End If
                    If Not cpCore.db.csOk(CS) Then
                        '
                        ' Could not create new Add-on
                        '
                        Call logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" & addonName & "], Guid [" & addonGuid & "]")
                    Else
                        addonId = cpCore.db.csGetInteger(CS, "ID")
                        '
                        ' Initialize the add-on
                        ' Delete any existing related records - so if this is an update with removed relationships, those are removed
                        '
                        'Call cpCore.db.deleteContentRecords("Shared Styles Add-on Rules", "addonid=" & addonId)
                        'Call cpCore.db.deleteContentRecords("Add-on Scripting Module Rules", "addonid=" & addonId)
                        Call cpCore.db.deleteContentRecords("Add-on Include Rules", "addonid=" & addonId)
                        Call cpCore.db.deleteContentRecords("Add-on Content Trigger Rules", "addonid=" & addonId)
                        '
                        Call cpCore.db.csSet(CS, "collectionid", CollectionID)
                        Call cpCore.db.csSet(CS, AddonGuidFieldName, addonGuid)
                        Call cpCore.db.csSet(CS, "name", addonName)
                        Call cpCore.db.csSet(CS, "navTypeId", navTypeId)
                        ArgumentList = ""
                        StyleSheet = ""
                        NavDeveloperOnly = True
                        If AddonNode.ChildNodes.Count > 0 Then
                            For Each PageInterface In AddonNode.ChildNodes
                                Select Case genericController.vbLCase(PageInterface.Name)
                                    Case "activexdll"
                                        '
                                        ' This is handled in BuildLocalCollectionFolder
                                        '
                                    Case "editors"
                                        '
                                        ' list of editors
                                        '
                                        For Each TriggerNode In PageInterface.ChildNodes
                                            Select Case genericController.vbLCase(TriggerNode.Name)
                                                Case "type"
                                                    fieldType = TriggerNode.InnerText
                                                    fieldTypeID = cpCore.db.getRecordID("Content Field Types", fieldType)
                                                    If fieldTypeID > 0 Then
                                                        Criteria = "(addonid=" & addonId & ")and(contentfieldTypeID=" & fieldTypeID & ")"
                                                        CS2 = cpCore.db.csOpen("Add-on Content Field Type Rules", Criteria)
                                                        If Not cpCore.db.csOk(CS2) Then
                                                            Call cpCore.db.csClose(CS2)
                                                            CS2 = cpCore.db.csInsertRecord("Add-on Content Field Type Rules", 0)
                                                        End If
                                                        If cpCore.db.csOk(CS2) Then
                                                            Call cpCore.db.csSet(CS2, "addonid", addonId)
                                                            Call cpCore.db.csSet(CS2, "contentfieldTypeID", fieldTypeID)
                                                        End If
                                                        Call cpCore.db.csClose(CS2)
                                                    End If
                                            End Select
                                        Next
                                    Case "processtriggers"
                                        '
                                        ' list of events that trigger a process run for this addon
                                        '
                                        For Each TriggerNode In PageInterface.ChildNodes
                                            Select Case genericController.vbLCase(TriggerNode.Name)
                                                Case "contentchange"
                                                    TriggerContentID = 0
                                                    ContentNameorGuid = TriggerNode.InnerText
                                                    If ContentNameorGuid = "" Then
                                                        ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "guid", "")
                                                        If ContentNameorGuid = "" Then
                                                            ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "name", "")
                                                        End If
                                                    End If
                                                    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(ContentNameorGuid) & ")"
                                                    CS2 = cpCore.db.csOpen("Content", Criteria)
                                                    If Not cpCore.db.csOk(CS2) Then
                                                        Call cpCore.db.csClose(CS2)
                                                        Criteria = "(ccguid is null)and(name=" & cpCore.db.encodeSQLText(ContentNameorGuid) & ")"
                                                        CS2 = cpCore.db.csOpen("content", Criteria)
                                                    End If
                                                    If cpCore.db.csOk(CS2) Then
                                                        TriggerContentID = cpCore.db.csGetInteger(CS2, "ID")
                                                    End If
                                                    Call cpCore.db.csClose(CS2)
                                                    'If TriggerContentID = 0 Then
                                                    '    CS2 = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                    '    If cpCore.db.cs_ok(CS2) Then
                                                    '        Call cpCore.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    '        Call cpCore.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    '        TriggerContentID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    '    End If
                                                    '    Call cpCore.db.cs_Close(CS2)
                                                    'End If
                                                    If TriggerContentID = 0 Then
                                                        '
                                                        ' could not find the content
                                                        '
                                                    Else
                                                        Criteria = "(addonid=" & addonId & ")and(contentid=" & TriggerContentID & ")"
                                                        CS2 = cpCore.db.csOpen("Add-on Content Trigger Rules", Criteria)
                                                        If Not cpCore.db.csOk(CS2) Then
                                                            Call cpCore.db.csClose(CS2)
                                                            CS2 = cpCore.db.csInsertRecord("Add-on Content Trigger Rules", 0)
                                                            If cpCore.db.csOk(CS2) Then
                                                                Call cpCore.db.csSet(CS2, "addonid", addonId)
                                                                Call cpCore.db.csSet(CS2, "contentid", TriggerContentID)
                                                            End If
                                                        End If
                                                        Call cpCore.db.csClose(CS2)
                                                    End If
                                            End Select
                                        Next
                                    Case "scripting"
                                        '
                                        ' include add-ons - NOTE - import collections must be run before interfaces
                                        ' when importing a collectin that will be used for an include
                                        '
                                        ScriptingLanguage = GetXMLAttribute(cpCore, IsFound, PageInterface, "language", "")
                                        scriptinglanguageid = cpCore.db.getRecordID("scripting languages", ScriptingLanguage)
                                        Call cpCore.db.csSet(CS, "scriptinglanguageid", scriptinglanguageid)
                                        ScriptingEntryPoint = GetXMLAttribute(cpCore, IsFound, PageInterface, "entrypoint", "")
                                        Call cpCore.db.csSet(CS, "ScriptingEntryPoint", ScriptingEntryPoint)
                                        ScriptingTimeout = genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterface, "timeout", "5000"))
                                        Call cpCore.db.csSet(CS, "ScriptingTimeout", ScriptingTimeout)
                                        ScriptingCode = ""
                                        'Call cpCore.app.csv_SetCS(CS, "ScriptingCode", ScriptingCode)
                                        For Each ScriptingNode In PageInterface.ChildNodes
                                            Select Case genericController.vbLCase(ScriptingNode.Name)
                                                Case "code"
                                                    ScriptingCode = ScriptingCode & ScriptingNode.InnerText
                                                    'Case "includemodule"

                                                    '    ScriptingModuleID = 0
                                                    '    ScriptingNameorGuid = ScriptingNode.InnerText
                                                    '    If ScriptingNameorGuid = "" Then
                                                    '        ScriptingNameorGuid = GetXMLAttribute(cpcore,IsFound, ScriptingNode, "guid", "")
                                                    '        If ScriptingNameorGuid = "" Then
                                                    '            ScriptingNameorGuid = GetXMLAttribute(cpcore,IsFound, ScriptingNode, "name", "")
                                                    '        End If
                                                    '    End If
                                                    '    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    '    CS2 = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                    '    If Not cpCore.db.cs_ok(CS2) Then
                                                    '        Call cpCore.db.cs_Close(CS2)
                                                    '        Criteria = "(ccguid is null)and(name=" & cpCore.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    '        CS2 = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                    '    End If
                                                    '    If cpCore.db.cs_ok(CS2) Then
                                                    '        ScriptingModuleID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    '    End If
                                                    '    Call cpCore.db.cs_Close(CS2)
                                                    '    If ScriptingModuleID = 0 Then
                                                    '        CS2 = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                    '        If cpCore.db.cs_ok(CS2) Then
                                                    '            Call cpCore.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    '            Call cpCore.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    '            ScriptingModuleID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    '        End If
                                                    '        Call cpCore.db.cs_Close(CS2)
                                                    '    End If
                                                    '    Criteria = "(addonid=" & addonId & ")and(scriptingmoduleid=" & ScriptingModuleID & ")"
                                                    '    CS2 = cpCore.db.cs_open("Add-on Scripting Module Rules", Criteria)
                                                    '    If Not cpCore.db.cs_ok(CS2) Then
                                                    '        Call cpCore.db.cs_Close(CS2)
                                                    '        CS2 = cpCore.db.cs_insertRecord("Add-on Scripting Module Rules", 0)
                                                    '        If cpCore.db.cs_ok(CS2) Then
                                                    '            Call cpCore.db.cs_set(CS2, "addonid", addonId)
                                                    '            Call cpCore.db.cs_set(CS2, "scriptingmoduleid", ScriptingModuleID)
                                                    '        End If
                                                    '    End If
                                                    '    Call cpCore.db.cs_Close(CS2)
                                            End Select
                                        Next
                                        Call cpCore.db.csSet(CS, "ScriptingCode", ScriptingCode)
                                    Case "activexprogramid"
                                        '
                                        ' save program id
                                        '
                                        FieldValue = PageInterface.InnerText
                                        Call cpCore.db.csSet(CS, "ObjectProgramID", FieldValue)
                                    Case "navigator"
                                        '
                                        ' create a navigator entry with a parent set to this
                                        '
                                        Call cpCore.db.csSave2(CS)
                                        menuNameSpace = GetXMLAttribute(cpCore, IsFound, PageInterface, "NameSpace", "")
                                        If menuNameSpace <> "" Then
                                            NavIconTypeString = GetXMLAttribute(cpCore, IsFound, PageInterface, "type", "")
                                            If NavIconTypeString = "" Then
                                                NavIconTypeString = "Addon"
                                            End If
                                            'Dim builder As New coreBuilderClass(cpCore)
                                            Call appBuilderController.verifyNavigatorEntry(cpCore, "", menuNameSpace, addonName, "", "", "", False, False, False, True, addonName, NavIconTypeString, addonName, CollectionID)
                                        End If
                                    Case "argument", "argumentlist"
                                        '
                                        ' multiple argumentlist elements are concatinated with crlf
                                        '
                                        NewValue = Trim(PageInterface.InnerText)
                                        If NewValue <> "" Then
                                            If ArgumentList = "" Then
                                                ArgumentList = NewValue
                                            ElseIf NewValue <> FieldValue Then
                                                ArgumentList = ArgumentList & vbCrLf & NewValue
                                            End If
                                        End If
                                    Case "style"
                                        '
                                        ' import exclusive style
                                        '
                                        NodeName = GetXMLAttribute(cpCore, IsFound, PageInterface, "name", "")
                                        NewValue = Trim(PageInterface.InnerText)
                                        If Left(NewValue, 1) <> "{" Then
                                            NewValue = "{" & NewValue
                                        End If
                                        If Right(NewValue, 1) <> "}" Then
                                            NewValue = NewValue & "}"
                                        End If
                                        StyleSheet = StyleSheet & vbCrLf & NodeName & " " & NewValue
                                    'Case "includesharedstyle"
                                    '    '
                                    '    ' added 9/3/2012
                                    '    '
                                    '    sharedStyleId = 0
                                    '    nodeNameOrGuid = GetXMLAttribute(cpcore,IsFound, PageInterface, "guid", "")
                                    '    If nodeNameOrGuid = "" Then
                                    '        nodeNameOrGuid = GetXMLAttribute(cpcore,IsFound, PageInterface, "name", "")
                                    '    End If
                                    '    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(nodeNameOrGuid) & ")"
                                    '    CS2 = cpCore.db.cs_open("shared styles", Criteria)
                                    '    If Not cpCore.db.cs_ok(CS2) Then
                                    '        Call cpCore.db.cs_Close(CS2)
                                    '        Criteria = "(ccguid is null)and(name=" & cpCore.db.encodeSQLText(nodeNameOrGuid) & ")"
                                    '        CS2 = cpCore.db.cs_open("shared styles", Criteria)
                                    '    End If
                                    '    If cpCore.db.cs_ok(CS2) Then
                                    '        sharedStyleId = cpCore.db.cs_getInteger(CS2, "ID")
                                    '    End If
                                    '    Call cpCore.db.cs_Close(CS2)
                                    '    If sharedStyleId = 0 Then
                                    '        CS2 = cpCore.db.cs_insertRecord("shared styles", 0)
                                    '        If cpCore.db.cs_ok(CS2) Then
                                    '            Call cpCore.db.cs_set(CS2, "name", nodeNameOrGuid)
                                    '            Call cpCore.db.cs_set(CS2, "ccguid", nodeNameOrGuid)
                                    '            sharedStyleId = cpCore.db.cs_getInteger(CS2, "ID")
                                    '        End If
                                    '        Call cpCore.db.cs_Close(CS2)
                                    '    End If
                                    '    Criteria = "(addonid=" & addonId & ")and(StyleId=" & sharedStyleId & ")"
                                    '    CS2 = cpCore.db.cs_open("Shared Styles Add-on Rules", Criteria)
                                    '    If Not cpCore.db.cs_ok(CS2) Then
                                    '        Call cpCore.db.cs_Close(CS2)
                                    '        CS2 = cpCore.db.cs_insertRecord("Shared Styles Add-on Rules", 0)
                                    '        If cpCore.db.cs_ok(CS2) Then
                                    '            Call cpCore.db.cs_set(CS2, "addonid", addonId)
                                    '            Call cpCore.db.cs_set(CS2, "StyleId", sharedStyleId)
                                    '        End If
                                    '    End If
                                    '    Call cpCore.db.cs_Close(CS2)
                                    Case "stylesheet", "styles"
                                        '
                                        ' import exclusive stylesheet if more then whitespace
                                        '
                                        test = PageInterface.InnerText
                                        test = genericController.vbReplace(test, " ", "")
                                        test = genericController.vbReplace(test, vbCr, "")
                                        test = genericController.vbReplace(test, vbLf, "")
                                        test = genericController.vbReplace(test, vbTab, "")
                                        If test <> "" Then
                                            StyleSheet = StyleSheet & vbCrLf & PageInterface.InnerText
                                        End If
                                    Case "template", "content", "admin"
                                        '
                                        ' these add-ons will be "non-developer only" in navigation
                                        '
                                        FieldName = PageInterface.Name
                                        FieldValue = PageInterface.InnerText
                                        If Not cpCore.db.cs_isFieldSupported(CS, FieldName) Then
                                            '
                                            ' Bad field name - need to report it somehow
                                            '
                                        Else
                                            Call cpCore.db.csSet(CS, FieldName, FieldValue)
                                            If genericController.EncodeBoolean(PageInterface.InnerText) Then
                                                '
                                                ' if template, admin or content - let non-developers have navigator entry
                                                '
                                                NavDeveloperOnly = False
                                            End If
                                        End If
                                    Case "icon"
                                        '
                                        ' icon
                                        '
                                        FieldValue = GetXMLAttribute(cpCore, IsFound, PageInterface, "link", "")
                                        If FieldValue <> "" Then
                                            '
                                            ' Icons can be either in the root of the website or in content files
                                            '
                                            FieldValue = genericController.vbReplace(FieldValue, "\", "/")   ' make it a link, not a file
                                            If genericController.vbInstr(1, FieldValue, "://") <> 0 Then
                                                '
                                                ' the link is an absolute URL, leave it link this
                                                '
                                            Else
                                                If Left(FieldValue, 1) <> "/" Then
                                                    '
                                                    ' make sure it starts with a slash to be consistance
                                                    '
                                                    FieldValue = "/" & FieldValue
                                                End If
                                                If Left(FieldValue, 17) = "/contensivefiles/" Then
                                                    '
                                                    ' in content files, start link without the slash
                                                    '
                                                    FieldValue = Mid(FieldValue, 18)
                                                End If
                                            End If
                                            Call cpCore.db.csSet(CS, "IconFilename", FieldValue)
                                            If True Then
                                                Call cpCore.db.csSet(CS, "IconWidth", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterface, "width", "0")))
                                                Call cpCore.db.csSet(CS, "IconHeight", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterface, "height", "0")))
                                                Call cpCore.db.csSet(CS, "IconSprites", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterface, "sprites", "0")))
                                            End If
                                        End If
                                    Case "includeaddon", "includeadd-on", "include addon", "include add-on"
                                        '
                                        ' processed in phase2 of this routine, after all the add-ons are installed
                                        '
                                    Case "form"
                                        '
                                        ' The value of this node is the xml instructions to create a form. Take then
                                        '   entire node, children and all, and save them in the formxml field.
                                        '   this replaces the settings add-on type, and soo to be report add-on types as well.
                                        '   this removes the ccsettingpages and settingcollectionrules, etc.
                                        '
                                        If True Then
                                            NavDeveloperOnly = False
                                            Call cpCore.db.csSet(CS, "formxml", PageInterface.InnerXml)
                                        End If
                                    Case "javascript", "javascriptinhead"
                                        '
                                        ' these all translate to JSFilename
                                        '
                                        FieldName = "jsfilename"
                                        Call cpCore.db.csSet(CS, FieldName, PageInterface.InnerText)

                                    Case "iniframe"
                                        '
                                        ' typo - field is inframe
                                        '
                                        FieldName = "inframe"
                                        Call cpCore.db.csSet(CS, FieldName, PageInterface.InnerText)
                                    Case Else
                                        '
                                        ' All the other fields should match the Db fields
                                        '
                                        FieldName = PageInterface.Name
                                        FieldValue = PageInterface.InnerText
                                        If Not cpCore.db.cs_isFieldSupported(CS, FieldName) Then
                                            '
                                            ' Bad field name - need to report it somehow
                                            '
                                            cpCore.handleException(New ApplicationException("bad field found [" & FieldName & "], in addon node [" & addonName & "], of collection [" & cpCore.db.getRecordName("add-on collections", CollectionID) & "]"))
                                        Else
                                            Call cpCore.db.csSet(CS, FieldName, FieldValue)
                                        End If
                                End Select
                            Next
                        End If
                        Call cpCore.db.csSet(CS, "ArgumentList", ArgumentList)
                        Call cpCore.db.csSet(CS, "StylesFilename", StyleSheet)
                        ' these are dynamic now
                        '            '
                        '            ' Setup special setting/tool/report Navigator Entry
                        '            '
                        '            If navTypeId = NavTypeIDTool Then
                        '                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{801F1F07-20E6-4A5D-AF26-71007CCB834F}"), addonid, 0, NavIconTypeTool, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        '            End If
                        '            If navTypeId = NavTypeIDReport Then
                        '                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{2ED078A2-6417-46CB-8572-A13F64C4BF18}"), addonid, 0, NavIconTypeReport, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        '            End If
                        '            If navTypeId = NavTypeIDSetting Then
                        '                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{5FDDC758-4A15-4F98-8333-9CE8B8BFABC4}"), addonid, 0, NavIconTypeSetting, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        '            End If
                    End If
                    Call cpCore.db.csClose(CS)
                    '
                    ' -- if this is needed, the installation xml files are available in the addon install folder. - I do not believe this is important
                    '       as if a collection is missing a dependancy, there is an error and you would expect to have to reinstall.
                    ''
                    '' Addon is now fully installed
                    '' Go through all collection files on this site and see if there are
                    '' any Dependencies on this add-on that need to be attached
                    '' src args are those for the addon that includes the current addon
                    ''   - if this addon is the target of another add-on's  "includeaddon" node
                    ''
                    'Doc = New XmlDocument
                    'CS = cpCore.db.cs_open("Add-on Collections")
                    'Do While cpCore.db.cs_ok(CS)
                    '    CollectionFile = cpCore.db.cs_get(CS, "InstallFile")
                    '    If CollectionFile <> "" Then
                    '        Try
                    '            Call Doc.LoadXml(CollectionFile)
                    '            If Doc.DocumentElement.HasChildNodes Then
                    '                For Each TestObject In Doc.DocumentElement.ChildNodes
                    '                    '
                    '                    ' 20161002 - maybe this should be testing for an xmlElemetn, not node
                    '                    '
                    '                    If (TypeOf (TestObject) Is XmlElement) Then
                    '                        SrcMainNode = DirectCast(TestObject, XmlElement)
                    '                        If genericController.vbLCase(SrcMainNode.Name) = "addon" Then
                    '                            SrcAddonGuid = SrcMainNode.GetAttribute("guid")
                    '                            SrcAddonName = SrcMainNode.GetAttribute("name")
                    '                            If SrcMainNode.HasChildNodes Then
                    '                                'On Error Resume Next
                    '                                For Each TestObject2 In SrcMainNode.ChildNodes
                    '                                    'For Each SrcAddonNode In SrcMainNode.childNodes
                    '                                    If TypeOf TestObject2 Is XmlNode Then
                    '                                        SrcAddonNode = DirectCast(TestObject2, XmlElement)
                    '                                        If True Then
                    '                                            'If Err.Number <> 0 Then
                    '                                            '    ' this is to catch nodes that are not elements
                    '                                            '    Err.Clear
                    '                                            'Else
                    '                                            'On Error GoTo ErrorTrap
                    '                                            If genericController.vbLCase(SrcAddonNode.Name) = "includeaddon" Then
                    '                                                TestGuid = SrcAddonNode.GetAttribute("guid")
                    '                                                TestName = SrcAddonNode.GetAttribute("name")
                    '                                                Criteria = ""
                    '                                                If TestGuid <> "" Then
                    '                                                    If TestGuid = addonGuid Then
                    '                                                        Criteria = "(" & AddonGuidFieldName & "=" & cpCore.db.encodeSQLText(SrcAddonGuid) & ")"
                    '                                                    End If
                    '                                                ElseIf TestName <> "" Then
                    '                                                    If TestName = addonName Then
                    '                                                        Criteria = "(name=" & cpCore.db.encodeSQLText(SrcAddonName) & ")"
                    '                                                    End If
                    '                                                End If
                    '                                                If Criteria <> "" Then
                    '                                                    '$$$$$ cache this
                    '                                                    CS2 = cpCore.db.cs_open(cnAddons, Criteria, "ID")
                    '                                                    If cpCore.db.cs_ok(CS2) Then
                    '                                                        SrcAddonID = cpCore.db.cs_getInteger(CS2, "ID")
                    '                                                    End If
                    '                                                    Call cpCore.db.cs_Close(CS2)
                    '                                                    AddRule = False
                    '                                                    If SrcAddonID = 0 Then
                    '                                                        UserError = "The add-on being installed is referenced by another add-on in collection [], but this add-on could not be found by the respoective criteria [" & Criteria & "]"
                    '                                                        Call logcontroller.appendInstallLog(cpCore,  "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" & UserError & "]")
                    '                                                    Else
                    '                                                        CS2 = cpCore.db.cs_openCsSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" & SrcAddonID & " and IncludedAddonID=" & addonId)
                    '                                                        AddRule = Not cpCore.db.cs_ok(CS2)
                    '                                                        Call cpCore.db.cs_Close(CS2)
                    '                                                    End If
                    '                                                    If AddRule Then
                    '                                                        CS2 = cpCore.db.cs_insertRecord("Add-on Include Rules", 0)
                    '                                                        If cpCore.db.cs_ok(CS2) Then
                    '                                                            Call cpCore.db.cs_set(CS2, "Addonid", SrcAddonID)
                    '                                                            Call cpCore.db.cs_set(CS2, "IncludedAddonID", addonId)
                    '                                                        End If
                    '                                                        Call cpCore.db.cs_Close(CS2)
                    '                                                    End If
                    '                                                End If
                    '                                            End If
                    '                                        End If
                    '                                    End If
                    '                                Next
                    '                            End If
                    '                        End If
                    '                    Else
                    '                        CS = CS
                    '                    End If
                    '                Next
                    '            End If
                    '        Catch ex As Exception
                    '            cpCore.handleExceptionAndContinue(ex) : Throw
                    '        End Try
                    '    End If
                    '    Call cpCore.db.cs_goNext(CS)
                    'Loop
                    'Call cpCore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
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
                        cpCore.metaData.clear()
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
                    cpCore.metaData.clear()
                    cpCore.cache.invalidateAll()
                    '
                    '----------------------------------------------------------------------------------------------------------------------
                    Call logController.appendInstallLog(cpCore, "CDef Load, stage 4: Verify content records required for Content Server")
                    '----------------------------------------------------------------------------------------------------------------------
                    '
                    Call VerifySortMethods(cpCore)
                    Call VerifyContentFieldTypes(cpCore)
                    cpCore.metaData.clear()
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
                    cpCore.metaData.clear()
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
                    cpCore.metaData.clear()
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
                    cpCore.metaData.clear()
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
                    cpCore.metaData.clear()
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
                        FieldHelpCID = cpCore.metaData.getContentId("Content Field Help")
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
                            Call cpCore.metaData.createContent(True _
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
                                        fieldId = cpCore.metaData.verifyCDefField_ReturnID(ContentName, field)
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
                            cpCore.metaData.clear()
                            cpCore.cache.invalidateAll()
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '==========================================================================================================================
        '   Overlay a Src CDef on to the current one (Dst)
        '       Any Src CDEf entries found in Src are added to Dst.
        '       if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
        '
        '       isBaseContent
        '           if dst not found, it is created to match src
        '           if dst found, it is updated only if isBase matches
        '               content attributes updated if .isBaseContent matches
        '               field attributes updated if .isBaseField matches
        '==========================================================================================================================
        '
        Private Shared Function installCollection_AddMiniCollectionSrcToDst(cpCore As coreClass, ByRef dstCollection As miniCollectionModel, ByVal srcCollection As miniCollectionModel, ByVal SrcIsUserCDef As Boolean) As Boolean
            Dim returnOk As Boolean = True
            Try
                Dim HelpSrc As String
                Dim HelpCustomChanged As Boolean
                Dim HelpDefaultChanged As Boolean
                Dim HelpChanged As Boolean
                Dim Copy As String
                Dim n As String
                Dim srcCollectionCdefField As Models.Complex.CDefFieldModel
                Dim dstCollectionCdef As Models.Complex.cdefModel
                Dim dstCollectionCdefField As Models.Complex.CDefFieldModel
                Dim IsMatch As Boolean
                Dim DstKey As String
                Dim SrcKey As String
                Dim DataBuildVersion As String
                Dim SrcIsNavigator As Boolean
                Dim DstIsNavigator As Boolean
                Dim SrcContentName As String
                Dim DstName As String
                Dim SrcFieldName As String
                Dim okToUpdateDstFromSrc As Boolean
                Dim srcCollectionCdef As Models.Complex.cdefModel
                Dim DebugSrcFound As Boolean
                Dim DebugDstFound As Boolean
                '
                ' If the Src is the BaseCollection, the Dst must be the Application Collectio
                '   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                '
                If srcCollection.isBaseCollection Then
                    For Each dstKeyValuePair In dstCollection.CDef
                        Dim dstWorkingCdef As Models.Complex.cdefModel = dstKeyValuePair.Value
                        Dim contentName As String
                        contentName = dstWorkingCdef.Name
                        If dstCollection.CDef(contentName.ToLower).IsBaseContent Then
                            '
                            ' this application collection Cdef is marked base, verify it is in the base collection
                            '
                            If Not srcCollection.CDef.ContainsKey(contentName.ToLower) Then
                                '
                                ' cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                '
                                With dstCollection.CDef(contentName.ToLower)
                                    .IsBaseContent = False
                                    .dataChanged = True
                                    For Each dstFieldKeyValuePair In .fields
                                        Dim field As Models.Complex.CDefFieldModel = dstFieldKeyValuePair.Value
                                        If field.isBaseField Then
                                            field.isBaseField = False
                                            'field.Changed = True
                                        End If
                                    Next
                                End With
                            End If
                        End If
                    Next
                End If
                '
                '
                ' -------------------------------------------------------------------------------------------------
                ' Go through all CollectionSrc and find the CollectionDst match
                '   if it is an exact match, do nothing
                '   if the cdef does not match, set cdefext(ptr).CDefChanged true
                '   if any field does not match, set cdefext...field...CDefChanged
                '   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                ' -------------------------------------------------------------------------------------------------
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name & ", UpgradeCDef_AddSrcToDst")
                '
                For Each srcKeyValuePair In srcCollection.CDef
                    srcCollectionCdef = srcKeyValuePair.Value

                    SrcContentName = srcCollectionCdef.Name
                    'If genericController.vbLCase(SrcContentName) = "site sections" Then
                    '    SrcContentName = SrcContentName
                    'End If
                    DebugSrcFound = False
                    If genericController.vbInstr(1, SrcContentName, cnNavigatorEntries, vbTextCompare) <> 0 Then
                        DebugSrcFound = True
                    End If
                    '
                    ' Search for this cdef in the Dst
                    '
                    okToUpdateDstFromSrc = False
                    If Not dstCollection.CDef.ContainsKey(SrcContentName.ToLower) Then
                        '
                        ' add src to dst
                        '
                        dstCollection.CDef.Add(SrcContentName.ToLower, New Models.Complex.cdefModel)
                        okToUpdateDstFromSrc = True
                    Else
                        dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower)
                        DstName = SrcContentName
                        '
                        ' found a match between Src and Dst
                        '
                        If dstCollectionCdef.IsBaseContent = srcCollectionCdef.IsBaseContent Then
                            '
                            ' Allow changes to user cdef only from user cdef, changes to base only from base
                            '
                            With dstCollectionCdef
                                n = "ActiveOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.ActiveOnly <> srcCollectionCdef.ActiveOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AdminOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AdminOnly <> srcCollectionCdef.AdminOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "DeveloperOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.DeveloperOnly <> srcCollectionCdef.DeveloperOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowAdd"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowAdd <> srcCollectionCdef.AllowAdd)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowCalendarEvents"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowCalendarEvents <> srcCollectionCdef.AllowCalendarEvents)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowContentTracking"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowContentTracking <> srcCollectionCdef.AllowContentTracking)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowDelete"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowDelete <> srcCollectionCdef.AllowDelete)
                                '
                                'If Not okToUpdateDstFromSrc Then n = "AllowMetaContent"
                                'okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowMetaContent <> srcCollectionCdef.AllowMetaContent)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowTopicRules"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowTopicRules <> srcCollectionCdef.AllowTopicRules)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ContentDataSourceName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentDataSourceName, srcCollectionCdef.ContentDataSourceName)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ContentTableName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentTableName, srcCollectionCdef.ContentTableName)
                                '
                                If DebugDstFound Then
                                    DebugDstFound = DebugDstFound
                                End If
                                If Not okToUpdateDstFromSrc Then n = "DefaultSortMethod"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .DefaultSortMethod, srcCollectionCdef.DefaultSortMethod)
                                '
                                If Not okToUpdateDstFromSrc Then n = "DropDownFieldList"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .DropDownFieldList, srcCollectionCdef.DropDownFieldList)
                                '
                                If Not okToUpdateDstFromSrc Then n = "EditorGroupName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .EditorGroupName, srcCollectionCdef.EditorGroupName)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IgnoreContentControl"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IgnoreContentControl <> srcCollectionCdef.IgnoreContentControl)
                                If okToUpdateDstFromSrc Then
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                End If
                                '
                                If Not okToUpdateDstFromSrc Then n = "Active"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Active <> srcCollectionCdef.Active)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowContentChildTool"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowContentChildTool <> srcCollectionCdef.AllowContentChildTool)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ParentId"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.parentID <> srcCollectionCdef.parentID)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconLink"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .IconLink, srcCollectionCdef.IconLink)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconHeight"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconHeight <> srcCollectionCdef.IconHeight)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconWidth"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconWidth <> srcCollectionCdef.IconWidth)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconSprites"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconSprites <> srcCollectionCdef.IconSprites)
                                '
                                If Not okToUpdateDstFromSrc Then n = "installedByCollectionGuid"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .installedByCollectionGuid, srcCollectionCdef.installedByCollectionGuid)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ccGuid"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .guid, srcCollectionCdef.guid)
                                '
                                ' IsBaseContent
                                '   if Dst IsBase, and Src is not, this change will be blocked following the changes anyway
                                '   if Src IsBase, and Dst is not, Dst should be changed, and IsBaseContent can be treated like any other field
                                '
                                If Not okToUpdateDstFromSrc Then n = "IsBaseContent"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IsBaseContent <> srcCollectionCdef.IsBaseContent)
                                If okToUpdateDstFromSrc Then
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                End If
                            End With
                            If okToUpdateDstFromSrc Then
                                If dstCollectionCdef.IsBaseContent And Not srcCollectionCdef.IsBaseContent Then
                                    '
                                    ' Dst is a base CDef, Src is not. This update is not allowed. Log it and skip the Add
                                    '
                                    Copy = "An attempt was made to update a Base Content Definition [" & DstName & "] from a collection that is not the Base Collection. This is not allowed."
                                    Call logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, " & Copy)
                                    Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, Copy, "dll", "builderClass", "UpgradeCDef_AddSrcToDst", 0, "", "", False, True, "")
                                    okToUpdateDstFromSrc = False
                                Else
                                    '
                                    ' Just log the change for tracking
                                    '
                                    Call logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, (Logging only) While merging two collections (probably application and an upgrade), one or more attributes for a content definition or field were different, first change was CDef=" & SrcContentName & ", field=" & n)
                                End If
                            End If
                        End If
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.CDef(SrcContentName.ToLower)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .Active = srcCollectionCdef.Active
                            .ActiveOnly = srcCollectionCdef.ActiveOnly
                            .AdminOnly = srcCollectionCdef.AdminOnly
                            '.adminColumns = srcCollectionCdef.adminColumns
                            .AliasID = srcCollectionCdef.AliasID
                            .AliasName = srcCollectionCdef.AliasName
                            .AllowAdd = srcCollectionCdef.AllowAdd
                            .AllowCalendarEvents = srcCollectionCdef.AllowCalendarEvents
                            .AllowContentChildTool = srcCollectionCdef.AllowContentChildTool
                            .AllowContentTracking = srcCollectionCdef.AllowContentTracking
                            .AllowDelete = srcCollectionCdef.AllowDelete
                            .AllowTopicRules = srcCollectionCdef.AllowTopicRules
                            .guid = srcCollectionCdef.guid
                            .dataChanged = True
                            .ContentControlCriteria = srcCollectionCdef.ContentControlCriteria
                            .ContentDataSourceName = srcCollectionCdef.ContentDataSourceName
                            .ContentTableName = srcCollectionCdef.ContentTableName
                            .dataSourceId = srcCollectionCdef.dataSourceId
                            .DefaultSortMethod = srcCollectionCdef.DefaultSortMethod
                            .DeveloperOnly = srcCollectionCdef.DeveloperOnly
                            .DropDownFieldList = srcCollectionCdef.DropDownFieldList
                            .EditorGroupName = srcCollectionCdef.EditorGroupName
                            '.fields
                            .IconHeight = srcCollectionCdef.IconHeight
                            .IconLink = srcCollectionCdef.IconLink
                            .IconSprites = srcCollectionCdef.IconSprites
                            .IconWidth = srcCollectionCdef.IconWidth
                            '.Id
                            .IgnoreContentControl = srcCollectionCdef.IgnoreContentControl
                            .includesAFieldChange = True
                            .installedByCollectionGuid = srcCollectionCdef.installedByCollectionGuid
                            .IsBaseContent = srcCollectionCdef.IsBaseContent
                            .IsModifiedSinceInstalled = srcCollectionCdef.IsModifiedSinceInstalled
                            .Name = srcCollectionCdef.Name
                            .parentID = srcCollectionCdef.parentID
                            .parentName = srcCollectionCdef.parentName
                            .SelectCommaList = srcCollectionCdef.SelectCommaList
                            '.selectList
                            '.TimeStamp
                            .WhereClause = srcCollectionCdef.WhereClause
                        End With
                    End If
                    '
                    ' Now check each of the field records for an addition, or a change
                    ' DstPtr is still set to the Dst CDef
                    '
                    'Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef(SrcPtr).fields.count=" & CollectionSrc.CDef(SrcPtr).fields.count)
                    With srcCollectionCdef
                        For Each srcFieldKeyValuePair In .fields
                            srcCollectionCdefField = srcFieldKeyValuePair.Value
                            SrcFieldName = srcCollectionCdefField.nameLc
                            okToUpdateDstFromSrc = False
                            If Not dstCollection.CDef.ContainsKey(SrcContentName.ToLower) Then
                                '
                                ' should have been the collection
                                '
                                Throw (New ApplicationException("ERROR - cannot update destination content because it was not found after being added."))
                            Else
                                dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower)
                                If dstCollectionCdef.fields.ContainsKey(SrcFieldName.ToLower) Then
                                    '
                                    ' Src field was found in Dst fields
                                    '

                                    dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower)
                                    okToUpdateDstFromSrc = False
                                    If dstCollectionCdefField.isBaseField = srcCollectionCdefField.isBaseField Then
                                        With srcCollectionCdefField
                                            n = "Active"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.active <> dstCollectionCdefField.active)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "AdminOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.adminOnly <> dstCollectionCdefField.adminOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Authorable"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.authorable <> dstCollectionCdefField.authorable)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Caption"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .caption, dstCollectionCdefField.caption)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.contentId <> dstCollectionCdefField.contentId)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "DeveloperOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.developerOnly <> dstCollectionCdefField.developerOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "EditSortPriority"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.editSortPriority <> dstCollectionCdefField.editSortPriority)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "EditTab"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .editTabName, dstCollectionCdefField.editTabName)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "FieldType"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.fieldTypeId <> dstCollectionCdefField.fieldTypeId)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "HTMLContent"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.htmlContent <> dstCollectionCdefField.htmlContent)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexColumn"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.indexColumn <> dstCollectionCdefField.indexColumn)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexSortDirection"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.indexSortDirection <> dstCollectionCdefField.indexSortDirection)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexSortOrder"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (EncodeInteger(.indexSortOrder) <> genericController.EncodeInteger(dstCollectionCdefField.indexSortOrder))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexWidth"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .indexWidth, dstCollectionCdefField.indexWidth)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.lookupContentID <> dstCollectionCdefField.lookupContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupList"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .lookupList, dstCollectionCdefField.lookupList)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.manyToManyContentID <> dstCollectionCdefField.manyToManyContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.manyToManyRuleContentID <> dstCollectionCdefField.manyToManyRuleContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRulePrimaryField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRulePrimaryField, dstCollectionCdefField.ManyToManyRulePrimaryField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleSecondaryField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRuleSecondaryField, dstCollectionCdefField.ManyToManyRuleSecondaryField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "MemberSelectGroupID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.MemberSelectGroupID <> dstCollectionCdefField.MemberSelectGroupID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "NotEditable"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.NotEditable <> dstCollectionCdefField.NotEditable)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Password"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Password <> dstCollectionCdefField.Password)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ReadOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.ReadOnly <> dstCollectionCdefField.ReadOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RedirectContentID <> dstCollectionCdefField.RedirectContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectID, dstCollectionCdefField.RedirectID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectPath"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectPath, dstCollectionCdefField.RedirectPath)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Required"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Required <> dstCollectionCdefField.Required)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RSSDescriptionField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RSSDescriptionField <> dstCollectionCdefField.RSSDescriptionField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RSSTitleField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RSSTitleField <> dstCollectionCdefField.RSSTitleField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Scramble"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Scramble <> dstCollectionCdefField.Scramble)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "TextBuffered"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.TextBuffered <> dstCollectionCdefField.TextBuffered)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "DefaultValue"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (genericController.encodeText(.defaultValue) <> genericController.encodeText(dstCollectionCdefField.defaultValue))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "UniqueName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.UniqueName <> dstCollectionCdefField.UniqueName)
                                            If okToUpdateDstFromSrc Then
                                                okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                            End If
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IsBaseField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.isBaseField <> dstCollectionCdefField.isBaseField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .lookupContentName(cpCore), dstCollectionCdefField.lookupContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyContentName(cpCore), dstCollectionCdefField.ManyToManyContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRuleContentName(cpCore), dstCollectionCdefField.ManyToManyRuleContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectContentName(cpCore), dstCollectionCdefField.RedirectContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "installedByCollectionid"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .installedByCollectionGuid, dstCollectionCdefField.installedByCollectionGuid)
                                            '
                                            If okToUpdateDstFromSrc Then
                                                okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                            End If
                                        End With
                                    End If
                                    '
                                    ' Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
                                    '
                                    HelpSrc = srcCollectionCdefField.HelpCustom
                                    HelpCustomChanged = Not TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpCustom)
                                    '
                                    HelpSrc = srcCollectionCdefField.HelpDefault
                                    HelpDefaultChanged = Not TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpDefault)
                                    '
                                    HelpChanged = HelpDefaultChanged Or HelpCustomChanged
                                Else
                                    '
                                    ' field was not found in dst, add it and populate
                                    '
                                    dstCollectionCdef.fields.Add(SrcFieldName.ToLower, New Models.Complex.CDefFieldModel)
                                    dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower)
                                    okToUpdateDstFromSrc = True
                                    HelpChanged = True
                                End If
                                '
                                ' If okToUpdateDstFromSrc, update the Dst record with the Src record
                                '
                                If okToUpdateDstFromSrc Then
                                    '
                                    ' Update Fields
                                    '
                                    With dstCollectionCdefField
                                        .active = srcCollectionCdefField.active
                                        .adminOnly = srcCollectionCdefField.adminOnly
                                        .authorable = srcCollectionCdefField.authorable
                                        .caption = srcCollectionCdefField.caption
                                        .contentId = srcCollectionCdefField.contentId
                                        .defaultValue = srcCollectionCdefField.defaultValue
                                        .developerOnly = srcCollectionCdefField.developerOnly
                                        .editSortPriority = srcCollectionCdefField.editSortPriority
                                        .editTabName = srcCollectionCdefField.editTabName
                                        .fieldTypeId = srcCollectionCdefField.fieldTypeId
                                        .htmlContent = srcCollectionCdefField.htmlContent
                                        .indexColumn = srcCollectionCdefField.indexColumn
                                        .indexSortDirection = srcCollectionCdefField.indexSortDirection
                                        .indexSortOrder = srcCollectionCdefField.indexSortOrder
                                        .indexWidth = srcCollectionCdefField.indexWidth
                                        .lookupContentID = srcCollectionCdefField.lookupContentID
                                        .lookupList = srcCollectionCdefField.lookupList
                                        .manyToManyContentID = srcCollectionCdefField.manyToManyContentID
                                        .manyToManyRuleContentID = srcCollectionCdefField.manyToManyRuleContentID
                                        .ManyToManyRulePrimaryField = srcCollectionCdefField.ManyToManyRulePrimaryField
                                        .ManyToManyRuleSecondaryField = srcCollectionCdefField.ManyToManyRuleSecondaryField
                                        .MemberSelectGroupID = srcCollectionCdefField.MemberSelectGroupID
                                        .nameLc = srcCollectionCdefField.nameLc
                                        .NotEditable = srcCollectionCdefField.NotEditable
                                        .Password = srcCollectionCdefField.Password
                                        .ReadOnly = srcCollectionCdefField.ReadOnly
                                        .RedirectContentID = srcCollectionCdefField.RedirectContentID
                                        .RedirectID = srcCollectionCdefField.RedirectID
                                        .RedirectPath = srcCollectionCdefField.RedirectPath
                                        .Required = srcCollectionCdefField.Required
                                        .RSSDescriptionField = srcCollectionCdefField.RSSDescriptionField
                                        .RSSTitleField = srcCollectionCdefField.RSSTitleField
                                        .Scramble = srcCollectionCdefField.Scramble
                                        .TextBuffered = srcCollectionCdefField.TextBuffered
                                        .UniqueName = srcCollectionCdefField.UniqueName
                                        .isBaseField = srcCollectionCdefField.isBaseField
                                        .lookupContentName(cpCore) = srcCollectionCdefField.lookupContentName(cpCore)
                                        .ManyToManyContentName(cpCore) = srcCollectionCdefField.ManyToManyContentName(cpCore)
                                        .ManyToManyRuleContentName(cpCore) = srcCollectionCdefField.ManyToManyRuleContentName(cpCore)
                                        .RedirectContentName(cpCore) = srcCollectionCdefField.RedirectContentName(cpCore)
                                        .installedByCollectionGuid = srcCollectionCdefField.installedByCollectionGuid
                                        .dataChanged = True
                                        If HelpChanged Then
                                            .HelpCustom = srcCollectionCdefField.HelpCustom
                                            .HelpDefault = srcCollectionCdefField.HelpDefault
                                            .HelpChanged = True
                                        End If
                                    End With
                                    dstCollectionCdef.includesAFieldChange = True
                                End If
                                '
                            End If
                        Next
                    End With
                Next
                '
                ' -------------------------------------------------------------------------------------------------
                ' Check SQL Indexes
                ' -------------------------------------------------------------------------------------------------
                '
                Dim dstSqlIndexPtr As Integer
                Dim SrcsSqlIndexPtr As Integer
                For SrcsSqlIndexPtr = 0 To srcCollection.SQLIndexCnt - 1
                    SrcContentName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName & "-" & srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName & "-" & srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName
                    okToUpdateDstFromSrc = False
                    '
                    ' Search for this name in the Dst
                    '
                    For dstSqlIndexPtr = 0 To dstCollection.SQLIndexCnt - 1
                        DstName = dstCollection.SQLIndexes(dstSqlIndexPtr).DataSourceName & "-" & dstCollection.SQLIndexes(dstSqlIndexPtr).TableName & "-" & dstCollection.SQLIndexes(dstSqlIndexPtr).IndexName
                        If TextMatch(cpCore, DstName, SrcContentName) Then
                            '
                            ' found a match between Src and Dst
                            '
                            With dstCollection.SQLIndexes(dstSqlIndexPtr)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .FieldNameList, srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList)
                            End With
                            Exit For
                        End If
                    Next
                    If dstSqlIndexPtr = dstCollection.SQLIndexCnt Then
                        '
                        ' CDef was not found, add it
                        '
                        ReDim Preserve dstCollection.SQLIndexes(dstCollection.SQLIndexCnt)
                        dstCollection.SQLIndexCnt = dstSqlIndexPtr + 1
                        okToUpdateDstFromSrc = True
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.SQLIndexes(dstSqlIndexPtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .DataSourceName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName
                            .FieldNameList = srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList
                            .IndexName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName
                            .TableName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName
                        End With
                    End If
                Next
                '
                '-------------------------------------------------------------------------------------------------
                ' Check menus
                '-------------------------------------------------------------------------------------------------
                '
                Dim DstMenuPtr As Integer
                Dim SrcNameSpace As String
                Dim SrcParentName As String
                DataBuildVersion = cpCore.siteProperties.dataBuildVersion
                For SrcMenuPtr = 0 To srcCollection.MenuCnt - 1
                    DstMenuPtr = 0
                    SrcContentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Name)
                    SrcParentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).ParentName)
                    SrcNameSpace = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).menuNameSpace)
                    SrcIsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator
                    If SrcIsNavigator Then
                        If (SrcContentName = "manage add-ons") Then
                            SrcContentName = SrcContentName
                        End If
                    End If
                    okToUpdateDstFromSrc = False
                    '
                    SrcKey = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Key)
                    '
                    ' Search for match using guid
                    '
                    IsMatch = False
                    For DstMenuPtr = 0 To dstCollection.MenuCnt - 1
                        DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name)
                        If DstName = SrcContentName Then
                            DstName = DstName
                            DstIsNavigator = dstCollection.Menus(DstMenuPtr).IsNavigator
                            DstKey = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Key)
                            If genericController.vbLCase(DstName) = "settings" Then
                                DstName = DstName
                            End If
                            IsMatch = (DstKey = SrcKey) And (SrcIsNavigator = DstIsNavigator)
                            If IsMatch Then
                                Exit For
                            End If
                        End If
                    Next
                    If Not IsMatch Then
                        '
                        ' no match found on guid, try name and ( either namespace or parentname )
                        '
                        For DstMenuPtr = 0 To dstCollection.MenuCnt - 1
                            DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name)
                            If genericController.vbLCase(DstName) = "settings" Then
                                DstName = DstName
                            End If
                            If ((SrcContentName = DstName) And (SrcIsNavigator = DstIsNavigator)) Then
                                If SrcIsNavigator Then
                                    '
                                    ' Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    '
                                    IsMatch = (SrcNameSpace = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).menuNameSpace)) And (dstCollection.Menus(DstMenuPtr).Guid = "")
                                Else
                                    '
                                    ' AdminMenu - check parentname
                                    '
                                    IsMatch = (SrcParentName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).ParentName))
                                End If
                                If IsMatch Then
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                    If Not IsMatch Then
                        'If DstPtr = CollectionDst.MenuCnt Then
                        '
                        ' menu was not found, add it
                        '
                        ReDim Preserve dstCollection.Menus(dstCollection.MenuCnt)
                        dstCollection.MenuCnt = dstCollection.MenuCnt + 1
                        okToUpdateDstFromSrc = True
                        'End If
                    Else
                        'If IsMatch Then
                        '
                        ' found a match between Src and Dst
                        '
                        If SrcIsUserCDef Or SrcIsNavigator Then
                            '
                            ' Special case -- Navigators update from all upgrade sources so Base migrates changes
                            ' test for cdef attribute changes
                            '
                            With dstCollection.Menus(DstMenuPtr)
                                'With dstCollection.Menus(dstCollection.MenuCnt)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Active <> srcCollection.Menus(SrcMenuPtr).Active)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AdminOnly <> srcCollection.Menus(SrcMenuPtr).AdminOnly)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentName, srcCollection.Menus(SrcMenuPtr).ContentName)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.DeveloperOnly <> srcCollection.Menus(SrcMenuPtr).DeveloperOnly)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .LinkPage, srcCollection.Menus(SrcMenuPtr).LinkPage)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Name, srcCollection.Menus(SrcMenuPtr).Name)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.NewWindow <> srcCollection.Menus(SrcMenuPtr).NewWindow)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .SortOrder, srcCollection.Menus(SrcMenuPtr).SortOrder)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .AddonName, srcCollection.Menus(SrcMenuPtr).AddonName)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .NavIconType, srcCollection.Menus(SrcMenuPtr).NavIconType)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .NavIconTitle, srcCollection.Menus(SrcMenuPtr).NavIconTitle)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .menuNameSpace, srcCollection.Menus(SrcMenuPtr).menuNameSpace)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Guid, srcCollection.Menus(SrcMenuPtr).Guid)
                                'okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ParentName, CollectionSrc.Menus(SrcPtr).ParentName)
                            End With
                        End If
                        'Exit For
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.Menus(DstMenuPtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .Guid = srcCollection.Menus(SrcMenuPtr).Guid
                            .Name = srcCollection.Menus(SrcMenuPtr).Name
                            .IsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator
                            .Active = srcCollection.Menus(SrcMenuPtr).Active
                            .AdminOnly = srcCollection.Menus(SrcMenuPtr).AdminOnly
                            .ContentName = srcCollection.Menus(SrcMenuPtr).ContentName
                            .DeveloperOnly = srcCollection.Menus(SrcMenuPtr).DeveloperOnly
                            .LinkPage = srcCollection.Menus(SrcMenuPtr).LinkPage
                            .NewWindow = srcCollection.Menus(SrcMenuPtr).NewWindow
                            .ParentName = srcCollection.Menus(SrcMenuPtr).ParentName
                            .menuNameSpace = srcCollection.Menus(SrcMenuPtr).menuNameSpace
                            .SortOrder = srcCollection.Menus(SrcMenuPtr).SortOrder
                            .AddonName = srcCollection.Menus(SrcMenuPtr).AddonName
                            .NavIconType = srcCollection.Menus(SrcMenuPtr).NavIconType
                            .NavIconTitle = srcCollection.Menus(SrcMenuPtr).NavIconTitle
                        End With
                    End If
                Next
                ''
                ''-------------------------------------------------------------------------------------------------
                '' Check addons -- yes, this should be done.
                ''-------------------------------------------------------------------------------------------------
                ''
                'If False Then
                '    '
                '    ' remove this for now -- later add ImportCollections to track the collections (not addons)
                '    '
                '    '
                '    '
                '    For SrcPtr = 0 To srcCollection.AddOnCnt - 1
                '        SrcContentName = genericController.vbLCase(srcCollection.AddOns(SrcPtr).Name)
                '        okToUpdateDstFromSrc = False
                '        '
                '        ' Search for this name in the Dst
                '        '
                '        For DstPtr = 0 To dstCollection.AddOnCnt - 1
                '            DstName = genericController.vbLCase(dstCollection.AddOns(DstPtr).Name)
                '            If DstName = SrcContentName Then
                '                '
                '                ' found a match between Src and Dst
                '                '
                '                If SrcIsUserCDef Then
                '                    '
                '                    ' test for cdef attribute changes
                '                    '
                '                    With dstCollection.AddOns(DstPtr)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ArgumentList, srcCollection.AddOns(SrcPtr).ArgumentList)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Copy, srcCollection.AddOns(SrcPtr).Copy)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Link, srcCollection.AddOns(SrcPtr).Link)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Name, srcCollection.AddOns(SrcPtr).Name)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ObjectProgramID, srcCollection.AddOns(SrcPtr).ObjectProgramID)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.SortOrder, srcCollection.AddOns(SrcPtr).SortOrder)
                '                    End With
                '                End If
                '                Exit For
                '            End If
                '        Next
                '        If DstPtr = dstCollection.AddOnCnt Then
                '            '
                '            ' CDef was not found, add it
                '            '
                '            ReDim Preserve dstCollection.AddOns(dstCollection.AddOnCnt)
                '            dstCollection.AddOnCnt = DstPtr + 1
                '            okToUpdateDstFromSrc = True
                '        End If
                '        If okToUpdateDstFromSrc Then
                '            With dstCollection.AddOns(DstPtr)
                '                '
                '                ' It okToUpdateDstFromSrc, update the Dst with the Src
                '                '
                '                .CDefChanged = True
                '                .ArgumentList = srcCollection.AddOns(SrcPtr).ArgumentList
                '                .Copy = srcCollection.AddOns(SrcPtr).Copy
                '                .Link = srcCollection.AddOns(SrcPtr).Link
                '                .Name = srcCollection.AddOns(SrcPtr).Name
                '                .ObjectProgramID = srcCollection.AddOns(SrcPtr).ObjectProgramID
                '                .SortOrder = srcCollection.AddOns(SrcPtr).SortOrder
                '            End With
                '        End If
                '    Next
                'End If
                '
                '-------------------------------------------------------------------------------------------------
                ' Check styles
                '-------------------------------------------------------------------------------------------------
                '
                Dim srcStylePtr As Integer
                Dim dstStylePtr As Integer
                For srcStylePtr = 0 To srcCollection.StyleCnt - 1
                    SrcContentName = genericController.vbLCase(srcCollection.Styles(srcStylePtr).Name)
                    okToUpdateDstFromSrc = False
                    '
                    ' Search for this name in the Dst
                    '
                    For dstStylePtr = 0 To dstCollection.StyleCnt - 1
                        DstName = genericController.vbLCase(dstCollection.Styles(dstStylePtr).Name)
                        If DstName = SrcContentName Then
                            '
                            ' found a match between Src and Dst
                            '
                            If SrcIsUserCDef Then
                                '
                                ' test for cdef attribute changes
                                '
                                With dstCollection.Styles(dstStylePtr)
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Copy, srcCollection.Styles(srcStylePtr).Copy)
                                End With
                            End If
                            Exit For
                        End If
                    Next
                    If dstStylePtr = dstCollection.StyleCnt Then
                        '
                        ' CDef was not found, add it
                        '
                        ReDim Preserve dstCollection.Styles(dstCollection.StyleCnt)
                        dstCollection.StyleCnt = dstStylePtr + 1
                        okToUpdateDstFromSrc = True
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.Styles(dstStylePtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .Copy = srcCollection.Styles(srcStylePtr).Copy
                            .Name = srcCollection.Styles(srcStylePtr).Name
                        End With
                    End If
                Next
                '
                '-------------------------------------------------------------------------------------------------
                ' Add Collections
                '-------------------------------------------------------------------------------------------------
                '
                Dim dstPtr As Integer = 0
                For SrcPtr = 0 To srcCollection.ImportCnt - 1
                    dstPtr = dstCollection.ImportCnt
                    ReDim Preserve dstCollection.collectionImports(dstPtr)
                    dstCollection.collectionImports(dstPtr) = srcCollection.collectionImports(SrcPtr)
                    dstCollection.ImportCnt = dstPtr + 1
                Next
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
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOk
        End Function
        ''
        ''===========================================================================
        ''   Error handler
        ''===========================================================================
        ''
        'Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ResumeNext As Boolean = False)
        '    '
        '    'Call App.LogEvent("addonInstallClass.HandleClassTrapError called from " & MethodName)
        '    '
        '   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError3(ApplicationName, "unknown", "dll", "AddonInstallClass", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        '    '
        'End Sub
        '
        '
        '
        Private Shared Function installCollection_GetApplicationMiniCollection(cpCore As coreClass, isNewBuild As Boolean) As miniCollectionModel
            Dim returnColl As New miniCollectionModel
            Try
                '
                Dim ExportFilename As String
                Dim ExportPathPage As String
                Dim CollectionData As String
                '
                If Not isNewBuild Then
                    '
                    ' if this is not an empty database, get the application collection, else return empty
                    '
                    ExportFilename = "cdef_export_" & CStr(genericController.GetRandomInteger()) & ".xml"
                    ExportPathPage = "tmp\" & ExportFilename
                    Call exportApplicationCDefXml(cpCore, ExportPathPage, True)
                    CollectionData = cpCore.privateFiles.readFile(ExportPathPage)
                    Call cpCore.privateFiles.deleteFile(ExportPathPage)
                    returnColl = installCollection_LoadXmlToMiniCollection(cpCore, CollectionData, False, False, isNewBuild, New miniCollectionModel)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnColl
        End Function
        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Shared Function GetXMLAttribute(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            Dim returnAttr As String = ""
            Try
                Dim NodeAttribute As XmlAttribute
                Dim ResultNode As XmlNode
                Dim UcaseName As String
                '
                Found = False
                ResultNode = Node.Attributes.GetNamedItem(Name)
                If (ResultNode Is Nothing) Then
                    UcaseName = genericController.vbUCase(Name)
                    For Each NodeAttribute In Node.Attributes
                        If genericController.vbUCase(NodeAttribute.Name) = UcaseName Then
                            returnAttr = NodeAttribute.Value
                            Found = True
                            Exit For
                        End If
                    Next
                    If Not Found Then
                        returnAttr = DefaultIfNotFound
                    End If
                Else
                    returnAttr = ResultNode.Value
                    Found = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnAttr
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeNumber(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As Double
            GetXMLAttributeNumber = EncodeNumber(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeBoolean(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Boolean) As Boolean
            GetXMLAttributeBoolean = genericController.EncodeBoolean(GetXMLAttribute(cpCore, Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeInteger(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Integer) As Integer
            GetXMLAttributeInteger = genericController.EncodeInteger(GetXMLAttribute(cpCore, Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '==================================================================================================================
        '
        '==================================================================================================================
        '
        Private Shared Function TextMatch(cpCore As coreClass, ByVal Source1 As String, ByVal Source2 As String) As Boolean
            TextMatch = (LCase(Source1) = genericController.vbLCase(Source2))
        End Function
        '
        '
        '
        Private Shared Function GetMenuNameSpace(cpCore As coreClass, ByVal Collection As miniCollectionModel, ByVal MenuPtr As Integer, ByVal IsNavigator As Boolean, ByVal UsedIDList As String) As String
            Dim returnAttr As String = ""
            Try
                Dim ParentName As String
                Dim Ptr As Integer
                Dim Prefix As String
                Dim LCaseParentName As String

                '
                With Collection
                    ParentName = .Menus(MenuPtr).ParentName
                    If ParentName <> "" Then
                        LCaseParentName = genericController.vbLCase(ParentName)
                        For Ptr = 0 To .MenuCnt - 1
                            If genericController.vbInstr(1, "," & UsedIDList & ",", "," & CStr(Ptr) & ",") = 0 Then
                                If LCaseParentName = genericController.vbLCase(.Menus(Ptr).Name) And (IsNavigator = .Menus(Ptr).IsNavigator) Then
                                    Prefix = GetMenuNameSpace(cpCore, Collection, Ptr, IsNavigator, UsedIDList & "," & MenuPtr)
                                    If Prefix = "" Then
                                        returnAttr = ParentName
                                    Else
                                        returnAttr = Prefix & "." & ParentName
                                    End If
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnAttr
        End Function
        '
        '=============================================================================
        '   Create an entry in the Sort Methods Table
        '=============================================================================
        '
        Private Shared Sub VerifySortMethod(cpCore As coreClass, ByVal Name As String, ByVal OrderByCriteria As String)
            Try
                '
                Dim dt As DataTable
                Dim sqlList As New sqlFieldListClass
                '
                Call sqlList.add("name", cpCore.db.encodeSQLText(Name))
                Call sqlList.add("CreatedBy", "0")
                Call sqlList.add("OrderByClause", cpCore.db.encodeSQLText(OrderByCriteria))
                Call sqlList.add("active", SQLTrue)
                Call sqlList.add("ContentControlID", cpCore.metaData.getContentId("Sort Methods").ToString())
                '
                dt = cpCore.db.openTable("Default", "ccSortMethods", "Name=" & cpCore.db.encodeSQLText(Name), "ID", "ID", 1)
                If dt.Rows.Count > 0 Then
                    '
                    ' update sort method
                    '
                    Call cpCore.db.updateTableRecord("Default", "ccSortMethods", "ID=" & genericController.EncodeInteger(dt.Rows(0).Item("ID")).ToString, sqlList)
                Else
                    '
                    ' Create the new sort method
                    '
                    Call cpCore.db.insertTableRecord("Default", "ccSortMethods", sqlList)
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
        Public Shared Sub VerifySortMethods(cpCore As coreClass)
            Try
                '
                Call logController.appendInstallLog(cpCore, "Verify Sort Records")
                '
                Call VerifySortMethod(cpCore, "By Name", "Name")
                Call VerifySortMethod(cpCore, "By Alpha Sort Order Field", "SortOrder")
                Call VerifySortMethod(cpCore, "By Date", "DateAdded")
                Call VerifySortMethod(cpCore, "By Date Reverse", "DateAdded Desc")
                Call VerifySortMethod(cpCore, "By Alpha Sort Order Then Oldest First", "SortOrder,ID")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Shared Sub VerifyContentFieldTypes(cpCore As coreClass)
            Try
                '
                Dim RowsFound As Integer
                Dim CID As Integer
                Dim TableBad As Boolean
                Dim RowsNeeded As Integer
                '
                ' ----- make sure there are enough records
                '
                TableBad = False
                RowsFound = 0
                Using rs As DataTable = cpCore.db.executeQuery("Select ID from ccFieldTypes order by id")
                    If (Not isDataTableOk(rs)) Then
                        '
                        ' problem
                        '
                        TableBad = True
                    Else
                        '
                        ' Verify the records that are there
                        '
                        RowsFound = 0
                        For Each dr As DataRow In rs.Rows
                            RowsFound = RowsFound + 1
                            If RowsFound <> genericController.EncodeInteger(dr.Item("ID")) Then
                                '
                                ' Bad Table
                                '
                                TableBad = True
                                Exit For
                            End If
                        Next
                    End If

                End Using
                '
                ' ----- Replace table if needed
                '
                If TableBad Then
                    Call cpCore.db.deleteTable("Default", "ccFieldTypes")
                    Call cpCore.db.createSQLTable("Default", "ccFieldTypes")
                    RowsFound = 0
                End If
                '
                ' ----- Add the number of rows needed
                '
                RowsNeeded = FieldTypeIdMax - RowsFound
                If RowsNeeded > 0 Then
                    CID = cpCore.metaData.getContentId("Content Field Types")
                    If CID <= 0 Then
                        '
                        ' Problem
                        '
                        cpCore.handleException(New ApplicationException("Content Field Types content definition was not found"))
                    Else
                        Do While RowsNeeded > 0
                            Call cpCore.db.executeQuery("Insert into ccFieldTypes (active,contentcontrolid)values(1," & CID & ")")
                            RowsNeeded = RowsNeeded - 1
                        Loop
                    End If
                End If
                '
                ' ----- Update the Names of each row
                '
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Integer' where ID=1;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Text' where ID=2;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='LongText' where ID=3;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Boolean' where ID=4;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Date' where ID=5;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='File' where ID=6;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Lookup' where ID=7;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Redirect' where ID=8;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Currency' where ID=9;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='TextFile' where ID=10;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Image' where ID=11;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Float' where ID=12;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='AutoIncrement' where ID=13;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='ManyToMany' where ID=14;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Member Select' where ID=15;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='CSS File' where ID=16;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='XML File' where ID=17;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Javascript File' where ID=18;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Link' where ID=19;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Resource Link' where ID=20;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML' where ID=21;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML File' where ID=22;")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateScript(cpCore As coreClass, ByVal Name As String, ByVal Link As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateScript"
                '
                ContentName = "Aggregate Function Scripts"
                CSEntry = cpCore.db.csOpen(ContentName, "(name=" & cpCore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Link,ObjectProgramID,ArgumentList,SortOrder")
                '
                ' If no current entry, create one
                '
                If Not cpCore.db.csOk(CSEntry) Then
                    cpCore.db.csClose(CSEntry)
                    CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpCore.db.csOk(CSEntry) Then
                        Call cpCore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpCore.db.csOk(CSEntry) Then
                    Call cpCore.db.csSet(CSEntry, "Link", Link)
                    Call cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                    Call cpCore.db.csSet(CSEntry, "SortOrder", SortOrder)
                End If
                Call cpCore.db.csClose(CSEntry)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateReplacement(cpcore As coreClass, ByVal Name As String, ByVal Copy As String, ByVal SortOrder As String)
            Call csv_VerifyAggregateReplacement2(cpcore, Name, Copy, "", SortOrder)
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Shared Sub csv_VerifyAggregateReplacement2(cpCore As coreClass, ByVal Name As String, ByVal Copy As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateReplacement2"
                '
                ContentName = "Aggregate Function Replacements"
                CSEntry = cpCore.db.csOpen(ContentName, "(name=" & cpCore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Copy,SortOrder,ArgumentList")
                '
                ' If no current entry, create one
                '
                If Not cpCore.db.csOk(CSEntry) Then
                    cpCore.db.csClose(CSEntry)
                    CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpCore.db.csOk(CSEntry) Then
                        Call cpCore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpCore.db.csOk(CSEntry) Then
                    Call cpCore.db.csSet(CSEntry, "Copy", Copy)
                    Call cpCore.db.csSet(CSEntry, "SortOrder", SortOrder)
                    Call cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                End If
                Call cpCore.db.csClose(CSEntry)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub        '
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateObject(cpcore As coreClass, ByVal Name As String, ByVal ObjectProgramID As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateObject"
                '
                ' Locate current entry
                '
                ContentName = "Aggregate Function Objects"
                CSEntry = cpcore.db.csOpen(ContentName, "(name=" & cpcore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Link,ObjectProgramID,ArgumentList,SortOrder")
                '
                ' If no current entry, create one
                '
                If Not cpcore.db.csOk(CSEntry) Then
                    cpcore.db.csClose(CSEntry)
                    CSEntry = cpcore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpcore.db.csOk(CSEntry) Then
                        Call cpcore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpcore.db.csOk(CSEntry) Then
                    Call cpcore.db.csSet(CSEntry, "ObjectProgramID", ObjectProgramID)
                    Call cpcore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                    Call cpcore.db.csSet(CSEntry, "SortOrder", SortOrder)
                End If
                Call cpcore.db.csClose(CSEntry)
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Shared Sub exportApplicationCDefXml(cpCore As coreClass, ByVal privateFilesPathFilename As String, ByVal IncludeBaseFields As Boolean)
            Try
                Dim XML As xmlController
                Dim Content As String
                '
                XML = New xmlController(cpCore)
                Content = XML.GetXMLContentDefinition3("", IncludeBaseFields)
                Call cpCore.privateFiles.saveFile(privateFilesPathFilename, Content)
                XML = Nothing
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
    End Class
End Namespace
