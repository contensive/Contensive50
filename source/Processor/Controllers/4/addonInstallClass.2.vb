
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
        '=========================================================================================================================
        '   Upgrade Application from a local collection
        '
        '   Either Upgrade or Install the collection in the Application. - no checks
        '
        '   ImportFromCollectionsGuidList - If this collection is from an import, this is the guid of the import.
        '=========================================================================================================================
        '
        Public Shared Function installCollectionFromLocalRepo(cpCore As coreClass, ByVal CollectionGuid As String, ByVal ignore_BuildVersion As String, ByRef return_ErrorMessage As String, ByVal ImportFromCollectionsGuidList As String, IsNewBuild As Boolean, ByRef nonCriticalErrorList As List(Of String)) As Boolean
            Dim result As Boolean = True
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
                                                                                Dim ContentID As Integer = Models.Complex.cdefModel.getContentId(cpCore, ContentName)
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
                                                                                    Dim CDef As Models.Complex.cdefModel = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
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
                                                                                    Dim CDef As Models.Complex.cdefModel = Models.Complex.cdefModel.getCdef(cpCore, ContentName)
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
                                                                                                                Dim FieldLookupContentName As String = models.complex.cdefmodel.getContentNameByID(cpcore,FieldLookupContentID)
                                                                                                                If FieldLookupContentName <> "" Then
                                                                                                                    If (Left(FieldValue, 1) = "{") And (Right(FieldValue, 1) = "}") And Models.Complex.cdefModel.isContentFieldSupported(cpCore, FieldLookupContentName, "ccguid") Then
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










    End Class
End Namespace
