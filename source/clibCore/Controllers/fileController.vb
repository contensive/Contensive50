
Option Explicit On
Option Strict On

Imports System.IO
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports Contensive.Core
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core.Controllers
    '
    '==============================================================================================================
    ''' <summary>
    ''' Basic file access class for all scaling targets (cden, private, approot, etc)
    ''' modes:
    '''   ephemeralFiles - local file access
    '''       Access to the local server relative to the rootLocalFolder
    '''       If no rootLocalFolder provided, full drive:filepath available
    '''       
    '''   passiveSync - local copy is updated only when needed (privateFiles, cdnFiles)
    '''       save - saves to a local filesystem      mirror, copied to S3 folder without public access
    '''       read - test s3 source for update, conditionally copied, read from local copy
    '''       
    '''   activeSync - local copy is updated on change (appRoot Files)
    '''       save - saves to a local folder, copied to S3 folder without public access, other webRoles copy changed files
    '''       read - read from local folder
    ''' </summary>
    Public Class fileController
        Implements IDisposable
        '
        '====================================================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        Public Enum fileSyncModeEnum
            ' noSync = file is written locally and read locallay
            noSync = 1
            ' passiveSync = (slow read, always consistent) FileSave - is written locally and uploaded to s3. FileRead - a check is made for current version and downloaded if neede, then read
            passiveSync = 2
            ' activeSync = (fast read, eventual consistency) FileSave - files written locally and uploaded to s3, then automatically downloaded to the other app clients for read. FileRead - read locally
            activeSync = 3
        End Enum
        '
        Private cpCore As coreClass
        Private Property isLocal As Boolean
        Public Property rootLocalPath As String                ' path ends in \, folder ends in foldername 
        Private Property clusterFileEndpoint As String
        Private Property fileSyncMode As fileSyncModeEnum
        '
        '==============================================================================================================
        ''' <summary>
        ''' Create a filesystem
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="isLocal">If true, thie object reads/saves to the local filesystem</param>
        ''' <param name="rootLocalPath"></param>
        ''' <param name="remoteFileEndpoint">If not isLocal, this endpoint is used for file sync</param>
        Public Sub New(cpCore As coreClass, isLocal As Boolean, fileSyncMode As fileSyncModeEnum, rootLocalPath As String, Optional remoteFileEndpoint As String = "")
            If (String.IsNullOrEmpty(rootLocalPath)) Then
                cpCore.handleExceptionAndContinue(New ArgumentException("Blank file system root path not permitted."))
            Else
                Me.cpCore = cpCore
                Me.isLocal = isLocal
                Me.clusterFileEndpoint = remoteFileEndpoint
                Me.fileSyncMode = fileSyncMode
                Me.rootLocalPath = normalizePath(rootLocalPath)
            End If
        End Sub
        '
        '==============================================================================================================
        '
        ' join two paths together to make a single path or filename
        '   changes / to \, and makes sure there is one and only one at the joint
        '
        Public Function joinPath(ByVal path As String, ByVal pathFilename As String) As String
            Dim returnPath As String = path & pathFilename
            Try
                path = normalizePath(path)
                pathFilename = normalizePathFilename(pathFilename)
                returnPath = IO.Path.Combine(path & pathFilename)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnPath
        End Function
        '
        '==============================================================================================================
        ''' <summary>
        ''' return a path and a filename from a pathFilename
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <param name="path"></param>
        ''' <param name="filename"></param>
        Public Sub splitPathFilename(ByVal pathFilename As String, ByRef path As String, ByRef filename As String)
            Try
                If String.IsNullOrEmpty(pathFilename) Then
                    Throw New ArgumentException("pathFilename cannot be blank")
                Else
                    pathFilename = normalizePathFilename(pathFilename)
                    Dim lastSlashPos As Integer = pathFilename.LastIndexOf("\")
                    If lastSlashPos >= 0 Then
                        path = pathFilename.Substring(0, lastSlashPos + 1)
                        filename = pathFilename.Substring(lastSlashPos + 1)
                    Else
                        path = ""
                        filename = pathFilename
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '
        '========================================================================
        '   Read in a file from a given PathFilename, return content
        '
        '   Do error trapping alittle different, always Call Err.Raise(ccObjectError_UnderlyingObject, , ErrorDescription) and print
        '   something out if there was a problem.
        '
        '========================================================================
        '
        Public Function readFile(ByVal PathFilename As String) As String
            Dim returnContent As String = ""
            Try
                If (String.IsNullOrEmpty(PathFilename)) Then
                    '
                    ' Not an error because an empty pathname returns an empty result
                    '
                Else
                    If Not isLocal Then
                        ' check local cache, download if needed
                    End If
                    If fileExists(PathFilename) Then
                        Using sr As StreamReader = File.OpenText(convertToAbsPath(PathFilename))
                            returnContent = sr.ReadToEnd()
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnContent
        End Function
        '
        '========================================================================
        '
        Public Function ReadBinaryFile(ByVal PathFilename As String) As Byte()
            Dim returnContent As Byte() = {}
            Dim bytesRead As Integer = 0
            Try
                If (PathFilename = "") Then
                    '
                    ' Not an error because an empty pathname returns an empty result
                    '
                Else
                    If Not isLocal Then
                        ' check local cache, download if needed
                    End If
                    If fileExists(PathFilename) Then
                        Using sr As FileStream = File.OpenRead(convertToAbsPath(PathFilename))
                            bytesRead = sr.Read(returnContent, 0, 1000000000)
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnContent
        End Function
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Save data to a file
        '========================================================================
        '
        Public Sub saveFile(ByVal pathFilename As String, ByVal FileContent As String)
            Call SaveDualFile(pathFilename, FileContent, Nothing, False)
        End Sub
        '
        '==============================================================================================================
        '
        Public Sub SaveFile(ByVal pathFilename As String, ByVal FileContent As Byte())
            Call SaveDualFile(pathFilename, Nothing, FileContent, True)
        End Sub
        '
        '==============================================================================================================
        '
        Private Sub SaveDualFile(ByVal pathFilename As String, ByVal textContent As String, binaryContent As Byte(), isBinary As Boolean)
            Try
                Dim path As String = ""
                Dim filename As String = ""
                '
                pathFilename = normalizePathFilename(pathFilename)
                If Not isValidPathFilename(pathFilename) Then
                    Throw New ArgumentException("PathFilename argument is not valid [" & pathFilename & "]")
                Else
                    'Dim localPathFilename As String
                    'localPathFilename = rootLocalPath & PathFilename
                    '
                    splitPathFilename(pathFilename, path, filename)
                    If Not pathExists(path) Then
                        Call createPath(path)
                    End If
                    Try
                        If isBinary Then
                            File.WriteAllBytes(convertToAbsPath(pathFilename), binaryContent)
                        Else
                            File.WriteAllText(convertToAbsPath(pathFilename), textContent)
                        End If
                    Catch ex As Exception
                        Call cpCore.handleExceptionAndContinue(ex) : Throw
                    End Try
                    If Not isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   append data to a file
        '       problem -- you cannot append with s3
        '           - move logging to simploeDb
        '           - rename this appendLocalFile + add syncLocalFile (moves it to s3)
        '========================================================================
        '
        Public Sub appendFile(ByVal PathFilename As String, ByVal FileContent As String)
            Try
                Dim path As String = ""
                Dim filename As String = ""
                Dim absFile As String = convertToAbsPath(PathFilename)
                '
                If (PathFilename = "") Then
                    Throw New ArgumentException("appendFile called with blank pathname.")
                Else
                    splitPathFilename(PathFilename, path, filename)
                    If Not pathExists(path) Then
                        Call createPath(path)
                    End If
                    If Not IO.File.Exists(absFile) Then
                        Using sw As IO.StreamWriter = IO.File.CreateText(absFile)
                            sw.Write(FileContent)
                        End Using
                    Else
                        Using sw As IO.StreamWriter = IO.File.AppendText(absFile)
                            sw.Write(FileContent)
                        End Using
                    End If
                    'File.AppendAllText(getFullPath(PathFilename), FileContent)
                End If
                'If Not clusterConfig.isLocal Then
                '    ' s3 transfer
                'End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   append data to a file
        '       problem -- you cannot append with s3
        '           - move logging to simploeDb
        '           - rename this appendLocalFile + add syncLocalFile (moves it to s3)
        '========================================================================
        '
        Public Sub syncLocalFile(ByVal PathFilename As String, ByVal FileContent As String)
            If Not isLocal Then
                ' s3 transfer
            End If
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        ' ----- Creates a file folder if it does not exist
        '========================================================================
        '
        Public Sub CreatefullPath(ByVal physicalFolderPath As String)
            Try
                Dim PartialPath As String
                Dim Position As Integer
                Dim WorkingPath As String
                '
                'MethodName = "CreateFileFolder( " & WorkingPath & " )"
                '
                If (String.IsNullOrEmpty(physicalFolderPath)) Then
                    Throw New ArgumentException("CreateLocalFileFolder called with blank path.")
                Else
                    WorkingPath = normalizePath(physicalFolderPath)
                    If Not Directory.Exists(WorkingPath) Then
                        Position = genericController.vbInstr(1, WorkingPath, "\")
                        Do While Position <> 0
                            PartialPath = Mid(WorkingPath, 1, Position - 1)
                            If Not Directory.Exists(PartialPath) Then
                                Call Directory.CreateDirectory(PartialPath)
                            End If
                            Position = genericController.vbInstr(Position + 1, WorkingPath, "\")
                        Loop
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        ' ----- Creates a file folder if it does not exist
        '========================================================================
        '
        Public Sub createPath(ByVal FolderPath As String)
            Try
                Dim abspath As String = convertToAbsPath(FolderPath)
                Call CreatefullPath(abspath)
                If Not isLocal Then
                    ' s3 transfer
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Deletes a file if it exists
        '========================================================================
        '
        Public Sub deleteFile(ByVal PathFilename As String)
            Try
                If (PathFilename = "") Then
                    '
                    ' not an error because the pathfile already does not exist
                    '
                Else
                    'Dim localPathFilename As String
                    'localPathFilename = rootLocalPath & PathFilename
                    If fileExists(PathFilename) Then
                        Call File.Delete(convertToAbsPath(PathFilename))
                    End If
                    If Not isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Deletes a file if it exists
        '========================================================================
        '
        Public Sub DeleteFileFolder(ByVal PathName As String)
            Try
                'Dim FolderObject As Folder
                'Dim ErrorDescription As String
                'Dim MethodName As String
                'Dim FolderDescription As String
                'Dim DetailArray As Object
                'Dim SubFolders As String
                'Dim ArrayPointer As Integer
                'Dim FileDetails As Object
                'Dim WorkingPath As String
                '
                'MethodName = "DeleteFolder( " & PathName & " )"
                '
                If (PathName <> "") Then
                    'WorkingPath = PathName
                    Dim localPath As String
                    localPath = joinPath(rootLocalPath, PathName)
                    If Right(localPath, 1) = "\" Then
                        localPath = Left(localPath, Len(localPath) - 1)
                    End If
                    If pathExists(PathName) Then
                        Call Directory.Delete(localPath, True)
                    End If
                    If Not isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Copies a file to another location
        '========================================================================
        '
        Public Sub copyFile(ByVal srcPathFilename As String, ByVal dstPathFilename As String, Optional dstFileSystem As fileController = Nothing)
            Try
                Dim dstPath As String = ""
                Dim dstFilename As String = ""
                Dim srcFullPathFilename As String
                Dim DstFullPathFilename As String
                '
                If dstFileSystem Is Nothing Then
                    dstFileSystem = Me
                End If
                If (String.IsNullOrEmpty(srcPathFilename)) Then
                    Throw New ArgumentException("Invalid source file.")
                ElseIf (String.IsNullOrEmpty(dstPathFilename)) Then
                    Throw New ArgumentException("Invalid destination file.")
                Else
                    srcPathFilename = normalizePathFilename(srcPathFilename)
                    dstPathFilename = normalizePathFilename(dstPathFilename)
                    If Not isLocal Then
                        ' s3 transfer
                    Else
                        If Not fileExists(srcPathFilename) Then
                            '
                            ' not an error, to minimize file use, empty files are not created, so missing files are just empty
                            '
                        Else
                            splitPathFilename(dstPathFilename, dstPath, dstFilename)
                            If Not dstFileSystem.pathExists(dstPath) Then
                                Call dstFileSystem.createPath(dstPath)
                            End If
                            srcFullPathFilename = rootLocalPath & srcPathFilename
                            DstFullPathFilename = dstFileSystem.rootLocalPath & dstPathFilename
                            If dstFileSystem.fileExists(dstPathFilename) Then
                                dstFileSystem.deleteFile(dstPathFilename)
                            End If
                            File.Copy(srcFullPathFilename, DstFullPathFilename)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        ' list of files, each row is delimited by a comma
        '
        Public Function getFileList(ByVal FolderPath As String) As FileInfo()
            Dim returnFileInfoList As FileInfo() = {}
            Try
                If Not isLocal Then
                    ' s3 transfer
                Else
                    If pathExists(FolderPath) Then
                        Dim localPath As String
                        localPath = rootLocalPath & FolderPath
                        Dim di As New IO.DirectoryInfo(localPath)
                        returnFileInfoList = di.GetFiles()
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnFileInfoList
        End Function
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Returns a list of folders in a path, comma delimited
        '========================================================================
        '
        Public Function getFolderNameList(ByVal FolderPath As String) As String
            Dim returnList As String = ""
            Try
                Dim di As System.IO.DirectoryInfo()
                di = getFolderList(FolderPath)
                For Each d As System.IO.DirectoryInfo In di
                    returnList &= "," & d.Name
                Next
                If returnList <> "" Then
                    returnList = returnList.Substring(1)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnList
        End Function
        '
        '==============================================================================================================
        '
        '
        '
        Public Function getFolderList(ByVal FolderPath As String) As IO.DirectoryInfo()
            Dim returnFolders As IO.DirectoryInfo() = {}
            Try
                If Not isLocal Then
                    returnFolders = {}
                Else
                    If Not pathExists(FolderPath) Then
                        returnFolders = {}
                    Else
                        Dim localPath As String = rootLocalPath & FolderPath
                        Dim di As New DirectoryInfo(localPath)
                        returnFolders = di.GetDirectories()
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnFolders
        End Function
        '
        '==============================================================================================================
        '
        '   Returns true if the file exists
        '
        Public Function fileExists(ByVal pathFilename As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                If Not isLocal Then
                Else
                    Dim localPathFilename As String = convertToAbsPath(pathFilename)
                    returnOK = File.Exists(localPathFilename)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnOK
        End Function
        '
        '==============================================================================================================
        '
        '   Returns true if the folder exists
        '
        Public Function pathExists(ByVal path As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                If Not isLocal Then
                Else
                    Dim absPath As String = convertToAbsPath(path)
                    returnOK = Directory.Exists(absPath)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnOK
        End Function
        '
        '==============================================================================================================
        '
        '   Rename a file
        '
        Public Sub renameFile(ByVal SourcePathFilename As String, ByVal DestinationFilename As String)
            Try
                Dim Pos As Integer
                Dim sourceFullPath As String = ""
                Dim srcFullPathFilename As String
                '
                If (SourcePathFilename = "") Then
                    Throw New ApplicationException("Invalid source file")
                Else
                    If Not isLocal Then
                    Else
                        SourcePathFilename = normalizePathFilename(SourcePathFilename)
                        srcFullPathFilename = joinPath(rootLocalPath, SourcePathFilename)
                        Pos = InStrRev(SourcePathFilename, "\")
                        If Pos >= 0 Then
                            sourceFullPath = Mid(SourcePathFilename, 1, Pos)
                        End If
                        If True Then
                            If (DestinationFilename = "") Then
                                Throw New ApplicationException("Invalid destination file []")
                            ElseIf (InStr(1, DestinationFilename, "\") <> 0) Then
                                Throw New ApplicationException("Invalid '\' character in destination filename [" & DestinationFilename & "]")
                            ElseIf (InStr(1, DestinationFilename, "/") <> 0) Then
                                Throw New ApplicationException("Invalid '/' character in destination filename [" & DestinationFilename & "]")
                            ElseIf Not fileExists(SourcePathFilename) Then
                                '
                                ' not an error, to minimize file use, empty files are not created, so missing files are just empty
                                '
                            Else
                                File.Move(srcFullPathFilename, sourceFullPath & DestinationFilename)
                            End If
                        End If
                    End If
                End If

            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '
        '
        Public Function getDriveFreeSpace() As Double
            Dim returnSize As Double = 0
            Try
                Dim scriptingDrive As IO.DriveInfo
                Dim driveLetter As String
                '
                ' Drive Space
                '
                driveLetter = rootLocalPath.Substring(0, 1)
                scriptingDrive = New IO.DriveInfo(driveLetter)
                If scriptingDrive.IsReady Then
                    returnSize = scriptingDrive.AvailableFreeSpace
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnSize
        End Function
        '
        '==============================================================================================================
        '
        ' copy one folder to another, include subfolders
        '
        Private Sub copyLocalFileFolder(ByVal physicalSrc As String, ByVal physicalDst As String)
            Try
                If Not Directory.Exists(physicalSrc) Then
                    '
                    ' -- source does not exist
                Else
                    '
                    ' -- create destination folder
                    If Not Directory.Exists(physicalDst) Then
                        CreatefullPath(physicalDst)
                    End If
                    '
                    Dim srcDirectoryInfo As DirectoryInfo = New DirectoryInfo(physicalSrc)
                    Dim dstDiretoryInfo As DirectoryInfo = New DirectoryInfo(physicalDst)
                    Dim dstCopy As DirectoryInfo
                    Dim srcCopy As DirectoryInfo
                    '
                    ' copy each file into destination
                    '
                    For Each srcFile As FileInfo In srcDirectoryInfo.GetFiles()
                        srcFile.CopyTo(Path.Combine(dstDiretoryInfo.ToString, srcFile.Name), True)
                    Next
                    '
                    ' recurse through folders
                    '
                    For Each srcCopy In srcDirectoryInfo.GetDirectories
                        dstCopy = dstDiretoryInfo.CreateSubdirectory(srcCopy.Name)
                        copyLocalFileFolder(srcCopy.FullName, dstCopy.FullName)
                    Next
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        ' copy one folder to another, include subfolders
        '
        Public Sub copyFolder(ByVal srcPath As String, ByVal dstPath As String, Optional dstFileSystem As fileController = Nothing)
            Try
                If Not isLocal Then
                    '
                    ' s3
                    '
                Else
                    '
                    ' create destination folder
                    '
                    If dstFileSystem Is Nothing Then
                        dstFileSystem = Me
                    End If
                    Call copyLocalFileFolder(rootLocalPath & srcPath, dstFileSystem.rootLocalPath & dstPath)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '
        '
        Private Function isValidPathFilename(ByVal pathFilename As String) As Boolean
            Dim returnValid As Boolean = True
            If pathFilename = "" Then
                returnValid = False
            ElseIf pathFilename.IndexOf("/") >= 0 Then
                returnValid = False
            End If
            Return returnValid
        End Function
        '
        '==============================================================================================================
        Public Function convertFileINfoArrayToParseString(FileInfo As IO.FileInfo()) As String
            Dim returnString As String = ""
            If FileInfo.Length > 0 Then
                For Each fi As IO.FileInfo In FileInfo
                    returnString &= vbCrLf & fi.Name & vbTab & fi.Attributes & vbTab & fi.CreationTime & vbTab & fi.LastAccessTime & vbTab & fi.LastWriteTime & vbTab & fi.Length & vbTab & fi.Extension
                Next
                returnString = returnString.Substring(2)
            End If
            Return returnString
        End Function
        '
        '==============================================================================================================
        Public Function convertDirectoryInfoArrayToParseString(DirectoryInfo As IO.DirectoryInfo()) As String
            Dim returnString As String = ""
            If DirectoryInfo.Length > 0 Then
                For Each di As IO.DirectoryInfo In DirectoryInfo
                    returnString &= vbCrLf & di.Name & vbTab & di.Attributes & vbTab & di.CreationTime & vbTab & di.LastAccessTime & vbTab & di.LastWriteTime & vbTab & "0" & vbTab & di.Extension
                Next
                returnString = returnString.Substring(2)
            End If
            Return returnString
        End Function
        '
        '==============================================================================================================
        '
        '=========================================================================================================
        '
        '=========================================================================================================
        '
        Public Sub SaveRemoteFile(ByVal Link As String, ByVal pathFilename As String)
            Try
                '
                Dim HTTP As New httpRequestController()
                Dim URLLink As String
                '
                If (pathFilename <> "") And (Link <> "") Then
                    pathFilename = normalizePathFilename(pathFilename)
                    URLLink = genericController.vbReplace(Link, " ", "%20")
                    HTTP.timeout = 600
                    Call HTTP.getUrlToFile(CStr(URLLink), convertToAbsPath(pathFilename))
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '=======================================================================================
        '   Unzip a zipfile
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '=======================================================================================
        '
        Public Sub UnzipFile(ByVal PathFilename As String)
            Try
                '
                Dim fastZip As FastZip = New FastZip()
                Dim fileFilter As String = Nothing
                Dim absPathFilename As String
                Dim path As String = String.Empty
                Dim filename As String = String.Empty
                '
                absPathFilename = convertToAbsPath(PathFilename)
                splitPathFilename(absPathFilename, path, filename)
                fastZip.ExtractZip(absPathFilename, path, fileFilter)                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub        '
        '
        '==============================================================================================================
        '
        '=======================================================================================
        '   Unzip a zipfile
        '
        '   Must be called from a process running as admin
        '   This can be done using the command queue, which kicks off the ccCmd process from the Server
        '=======================================================================================
        '
        Public Sub zipFile(archivePathFilename As String, ByVal addPathFilename As String)
            Try
                '
                Dim fastZip As FastZip = New FastZip()
                Dim fileFilter As String = Nothing
                Dim recurse As Boolean = True
                Dim archivepath As String = ""
                Dim archiveFilename As String = "" '
                '
                splitPathFilename(archivePathFilename, archivepath, archiveFilename)
                fastZip.CreateZip(rootLocalPath & archivePathFilename, rootLocalPath & addPathFilename, recurse, fileFilter)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
        End Sub        '
        '
        '====================================================================================================
        ''' <summary>
        ''' convert a path argument (relative to rootPath) into a full absolute path. Allow for the case where the path is incorrectly a full path within the rootpath
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <returns></returns>
        Private Function convertToAbsPath(pathFilename As String) As String
            Dim result As String = pathFilename
            Try
                Dim normalizedPathFilename As String = normalizePathFilename(pathFilename)
                If (String.IsNullOrEmpty(normalizedPathFilename)) Then
                    result = rootLocalPath
                ElseIf isinPhysicalPath(normalizedPathFilename) Then
                    result = normalizedPathFilename
                ElseIf (normalizedPathFilename.IndexOf(":\") >= 0) Then
                    Throw New ApplicationException("Attempt to access an invalid path [" & normalizedPathFilename & "] that is not within the allowed path [" & rootLocalPath & "].")
                Else
                    result = joinPath(rootLocalPath, normalizedPathFilename)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Argument can be a path (myPath/), a filename (myfile.bin), or a pathFilename (myPath/myFile.bin)
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <returns></returns>
        Public Shared Function normalizePathFilename(path As String) As String
            If (String.IsNullOrEmpty(path)) Then
                Return String.Empty
            Else
                Dim returnPath As String = path
                returnPath = returnPath.Replace("/", "\")
                Do While returnPath.IndexOf("\\") >= 0
                    returnPath = returnPath.Replace("\\", "\")
                Loop
                Return returnPath
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Ensures a path uses the correct file delimiter "\", and ends in a "\"
        ''' </summary>
        ''' <param name="path"></param>
        ''' <returns></returns>
        Public Shared Function normalizePath(ByVal path As String) As String
            If String.IsNullOrEmpty(path) Then
                Return String.Empty
            Else
                path = normalizePathFilename(path)
                If (path.Substring(0, 1) = "\") Then
                    path = path.Substring(1)
                End If
                If (path.Substring(path.Length - 1, 1) <> "\") Then
                    Return path & "\"
                Else
                    Return path
                End If
            End If
        End Function

        '
        '====================================================================================================
        Public Function isinPhysicalPath(path As String) As Boolean
            Return (normalizePath(path).ToLower().IndexOf(rootLocalPath.ToLower()) = 0)
        End Function
        '
        ' save uploaded file (used to be in html_ classes)
        '
        ''
        ''========================================================================
        '''' <summary>
        '''' process the request for an input file, storing the file system provided, in an optional filePath. Return the pathFilename uploaded. The filename is returned as a byref argument.
        '''' </summary>
        '''' <param name="TagName"></param>
        '''' <param name="files"></param>
        '''' <param name="filePath"></param>
        '''' <returns></returns>
        'Public Function saveUpload(ByVal TagName As String, ByVal filePath As String) As String
        '    Dim returnFilename As String = ""
        '    Return web_processFormInputFile(TagName, files, filePath, returnFilename)
        'End Function
        '
        '========================================================================
        ''' <summary>
        ''' save an uploaded file to a path, and return the uploaded filename
        ''' </summary>
        ''' <param name="TagName"></param>
        ''' <param name="files"></param>
        ''' <param name="filePath"></param>
        ''' <param name="returnFilename"></param>
        ''' <returns></returns>
        Public Function saveUpload(ByVal htmlTagName As String, ByVal path As String, ByRef returnFilename As String) As Boolean
            Dim success As Boolean = False
            Try
                returnFilename = String.Empty
                '
                Dim key As String = htmlTagName.ToLower()
                If cpCore.docProperties.containsKey(key) Then
                    With cpCore.docProperties.getProperty(key)
                        If (.IsFile) And (.Name.ToLower() = key) Then
                            Dim returnPathFilename As String = fileController.normalizePath(path)
                            returnFilename = encodeFilename(.Value)
                            returnPathFilename &= returnFilename
                            Call deleteFile(returnPathFilename)
                            If .tmpPrivatePathfilename <> "" Then
                                '
                                ' copy tmp private files to the appropriate folder in the destination file system
                                '
                                Call cpCore.privateFiles.copyFile(.tmpPrivatePathfilename, returnPathFilename, Me)
                                success = True
                            End If
                        End If
                    End With
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return success
        End Function
        '
        '====================================================================================================
        ' dispose
        '====================================================================================================
        '
#Region " IDisposable Support "
        '
        Protected disposed As Boolean = False
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                'Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    'CP = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
                'FileSystem = Nothing
            End If
            Me.disposed = True
        End Sub
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace
