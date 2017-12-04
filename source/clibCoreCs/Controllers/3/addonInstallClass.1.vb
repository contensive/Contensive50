
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
                                    return_ErrorMessage = "There was a problem installing the Collection File [" & tmpInstallPath & Filename & "]. The error reported was [" & ex.Message & "]."
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


    End Class
End Namespace
