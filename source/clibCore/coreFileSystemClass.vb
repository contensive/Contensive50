
Option Explicit On
Option Strict On

Imports System.IO
Imports ICSharpCode.SharpZipLib.Core
Imports ICSharpCode.SharpZipLib.Zip
Imports Contensive.Core

Namespace Contensive.Core
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
    Public Class coreFileSystemClass
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
        Private cpCore As cpCoreClass
        Private Property clusterConfig As clusterConfigClass
        Public Property rootLocalFolderPath As String                ' path ends in \, folder ends in foldername 
        Private Property clusterFileEndpoint As String
        Private Property fileSyncMode As fileSyncModeEnum
        '
        '==============================================================================================================
        ''' <summary>
        ''' Create a filesystem
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="clusterConfig"></param>
        ''' <param name="rootLocalFolder"></param>
        ''' <param name="clusterFileEndpoint"></param>
        Public Sub New(cpCore As cpCoreClass, clusterConfig As clusterConfigClass, fileSyncMode As fileSyncModeEnum, rootLocalFolder As String, Optional clusterFileEndpoint As String = "")
            ' need cp for compatibility, but cp.core etc may not be available during construction, so bootstrap with a clusterConfig
            Me.cpCore = cpCore
            Me.clusterConfig = clusterConfig
            Me.clusterFileEndpoint = clusterFileEndpoint
            Me.fileSyncMode = fileSyncMode
            Me.rootLocalFolderPath = rootLocalFolder
            If (Not String.IsNullOrEmpty(rootLocalFolder)) Then
                If (Me.rootLocalFolderPath.Substring(Me.rootLocalFolderPath.Length - 1, 1) <> "\") Then
                    Me.rootLocalFolderPath &= "\"
                End If
            End If
        End Sub
        '
        '==============================================================================================================
        '
        ' join two paths together to make a single path or filename
        '   changes / to \, and makes sure there is one and only one at the joint
        '
        Public Function joinPath(ByVal pathPrefix As String, ByVal pathSuffix As String) As String
            Dim returnPath As String = pathPrefix & pathSuffix
            Try
                pathPrefix = pathPrefix.Replace("/", "\")
                pathSuffix = pathSuffix.Replace("/", "\")
                returnPath = Path.Combine(pathPrefix & pathSuffix)
            Catch ex As Exception
                '
            End Try
            Return returnPath
        End Function
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
        Public Function ReadFile(ByVal PathFilename As String) As String
            Dim returnContent As String = ""
            Try
                If (PathFilename = "") Then
                    '
                    ' Not an error because an empty pathname returns an empty result
                    '
                Else
                    If Not clusterConfig.isLocal Then
                        ' check local cache, download if needed
                    End If
                    If checkFile(PathFilename) Then
                        Using sr As StreamReader = File.OpenText(rootLocalFolderPath & PathFilename)
                            returnContent = sr.ReadToEnd()
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
                    If Not clusterConfig.isLocal Then
                        ' check local cache, download if needed
                    End If
                    If checkFile(PathFilename) Then
                        Using sr As FileStream = File.OpenRead(rootLocalFolderPath & PathFilename)
                            bytesRead = sr.Read(returnContent, 0, 1000000000)
                        End Using
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Public Sub SaveFile(ByVal pathFilename As String, ByVal FileContent As String)
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
                Dim path As String
                '
                pathFilename = pathFilename.Replace("/", "\")
                If Not isValidPathFilename(pathFilename) Then
                    Throw New ArgumentException("PathFilename argument is not valid [" & pathFilename & "]")
                Else
                    'Dim localPathFilename As String
                    'localPathFilename = rootLocalPath & PathFilename
                    '
                    path = getPath(pathFilename)
                    If Not checkPath(path) Then
                        Call createPath(path)
                    End If
                    Try
                        If isBinary Then
                            File.WriteAllBytes(rootLocalFolderPath & pathFilename, binaryContent)
                        Else
                            File.WriteAllText(rootLocalFolderPath & pathFilename, textContent)
                        End If
                    Catch ex As Exception
                        Call cpCore.handleException(ex)
                    End Try
                    If Not clusterConfig.isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
                Dim FileFolder As String
                Dim absFile As String = rootLocalFolderPath & PathFilename
                '
                If (PathFilename = "") Then
                    Throw New ArgumentException("appendFile called with blank pathname.")
                Else
                    FileFolder = getPath(PathFilename)
                    If Not checkPath(FileFolder) Then
                        Call createPath(FileFolder)
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
                    'File.AppendAllText(rootLocalFolderPath & PathFilename, FileContent)
                End If
                'If Not clusterConfig.isLocal Then
                '    ' s3 transfer
                'End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
            If Not clusterConfig.isLocal Then
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
                If (physicalFolderPath = "") Then
                    Throw New ArgumentException("CreateLocalFileFolder called with blank pathname.")
                Else
                    WorkingPath = physicalFolderPath.Replace("/", "\")
                    If Right(WorkingPath, 1) <> "\" Then
                        WorkingPath = WorkingPath & "\"
                    End If
                    If Not Directory.Exists(WorkingPath) Then
                        Position = InStr(1, WorkingPath, "\")
                        Do While Position <> 0
                            PartialPath = Mid(WorkingPath, 1, Position - 1)
                            If Not Directory.Exists(PartialPath) Then
                                Call Directory.CreateDirectory(PartialPath)
                            End If
                            Position = InStr(Position + 1, WorkingPath, "\")
                        Loop
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) '
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
                '
                Call CreatefullPath(joinPath(rootLocalFolderPath, FolderPath))
                If Not clusterConfig.isLocal Then
                    ' s3 transfer
                End If
            Catch ex As Exception
                cpCore.handleException(ex) '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Deletes a file if it exists
        '========================================================================
        '
        Public Sub DeleteFile(ByVal PathFilename As String)
            Try
                If (PathFilename = "") Then
                    '
                    ' not an error because the pathfile already does not exist
                    '
                Else
                    'Dim localPathFilename As String
                    'localPathFilename = rootLocalPath & PathFilename
                    If checkFile(PathFilename) Then
                        Call File.Delete(rootLocalFolderPath & PathFilename)
                    End If
                    If Not clusterConfig.isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) '
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
                    localPath = joinPath(rootLocalFolderPath, PathName)
                    If Right(localPath, 1) = "\" Then
                        localPath = Left(localPath, Len(localPath) - 1)
                    End If
                    If checkPath(PathName) Then
                        Call Directory.Delete(localPath, True)
                    End If
                    If Not clusterConfig.isLocal Then
                        ' s3 transfer
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        '========================================================================
        '   Copies a file to another location
        '========================================================================
        '
        Public Sub copyFile(ByVal srcPathFilename As String, ByVal dstPathFilename As String, Optional dstFileSystem As coreFileSystemClass = Nothing)
            Try
                Dim dstPath As String
                Dim srcFullPathFilename As String
                Dim DstFullPathFilename As String
                '
                If dstFileSystem Is Nothing Then
                    dstFileSystem = Me
                End If
                If (srcPathFilename = "") Then
                    Throw New ArgumentException("Invalid source file.")
                ElseIf (dstPathFilename = "") Then
                    Throw New ArgumentException("Invalid destination file.")
                Else
                    If Not clusterConfig.isLocal Then
                        ' s3 transfer
                    Else
                        If Not checkFile(srcPathFilename) Then
                            '
                            ' not an error, to minimize file use, empty files are not created, so missing files are just empty
                            '
                        Else
                            dstPath = getPath(dstPathFilename)
                            If Not dstFileSystem.checkPath(dstPath) Then
                                Call dstFileSystem.createPath(dstPath)
                            End If
                            srcFullPathFilename = rootLocalFolderPath & srcPathFilename
                            DstFullPathFilename = dstFileSystem.rootLocalFolderPath & dstPathFilename
                            If dstFileSystem.checkFile(dstPathFilename) Then
                                dstFileSystem.DeleteFile(dstPathFilename)
                            End If
                            File.Copy(srcFullPathFilename, DstFullPathFilename)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) '
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        ' list of files, each row is delimited by a comma
        '
        Public Function GetFolderFiles(ByVal FolderPath As String) As FileInfo()
            Dim returnFileInfoList As FileInfo() = {}
            Try
                If Not clusterConfig.isLocal Then
                    ' s3 transfer
                Else
                    If checkPath(FolderPath) Then
                        Dim localPath As String
                        localPath = rootLocalFolderPath & FolderPath
                        Dim di As New IO.DirectoryInfo(localPath)
                        returnFileInfoList = di.GetFiles()
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Public Function getFolderList(ByVal FolderPath As String) As String
            Dim returnList As String = ""
            Try
                Dim di As System.IO.DirectoryInfo()
                di = getFolders(FolderPath)
                For Each d As System.IO.DirectoryInfo In di
                    returnList &= "," & d.Name
                Next
                If returnList <> "" Then
                    returnList = returnList.Substring(1)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnList
        End Function
        '
        '==============================================================================================================
        '
        '
        '
        Public Function getFolders(ByVal FolderPath As String) As IO.DirectoryInfo()
            Dim returnFolders As IO.DirectoryInfo() = {}
            Try
                If Not clusterConfig.isLocal Then
                    returnFolders = {}
                Else
                    If Not checkPath(FolderPath) Then
                        returnFolders = {}
                    Else
                        Dim localPath As String = rootLocalFolderPath & FolderPath
                        Dim di As New DirectoryInfo(localPath)
                        returnFolders = di.GetDirectories()
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnFolders
        End Function
        '
        '==============================================================================================================
        '
        '
        '
        Private Function getPath(ByVal pathFilename As String) As String
            getPath = ""
            Try
                Dim Position As Integer
                '
                Position = InStrRev(pathFilename, "\")
                If Position <> 0 Then
                    getPath = Mid(pathFilename, 1, Position)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Function
        '
        '==============================================================================================================
        '
        '   Returns true if the file exists
        '
        Public Function checkFile(ByVal pathFilename As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                If Not clusterConfig.isLocal Then
                Else
                    Dim localPathFilename As String = rootLocalFolderPath & pathFilename
                    returnOK = File.Exists(localPathFilename)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnOK
        End Function
        '
        '==============================================================================================================
        '
        '   Returns true if the folder exists
        '
        Public Function checkPath(ByVal path As String) As Boolean
            Dim returnOK As Boolean = False
            Try
                If Not clusterConfig.isLocal Then
                Else
                    Dim localFileFolder As String = rootLocalFolderPath & path
                    returnOK = Directory.Exists(localFileFolder)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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

                Dim ErrorDescription As String
                Dim FileFolder As String
                Dim Pos As Integer
                Dim sourceFullPath As String
                Dim srcFullPathFilename As String
                '
                'MethodName = "renameFile( " & SourcePathFilename & ", " & DestinationFilename & " )"
                '
                If (SourcePathFilename = "") Then
                    Throw New ApplicationException("Invalid source file")
                Else
                    If Not clusterConfig.isLocal Then
                    Else
                        SourcePathFilename = SourcePathFilename.Replace("/", "\")
                        srcFullPathFilename = rootLocalFolderPath & SourcePathFilename
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
                            ElseIf Not checkFile(SourcePathFilename) Then
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
                cpCore.handleException(ex) '
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
                driveLetter = rootLocalFolderPath.Substring(0, 1)
                scriptingDrive = New IO.DriveInfo(driveLetter)
                If scriptingDrive.IsReady Then
                    returnSize = scriptingDrive.AvailableFreeSpace
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
                '
                ' create destination folder
                '
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
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '==============================================================================================================
        '
        ' copy one folder to another, include subfolders
        '
        Public Sub copyFolder(ByVal srcPath As String, ByVal dstPath As String, Optional dstFileSystem As coreFileSystemClass = Nothing)
            Try
                If Not clusterConfig.isLocal Then
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
                    Call copyLocalFileFolder(rootLocalFolderPath & srcPath, dstFileSystem.rootLocalFolderPath & dstPath)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
        Friend Sub SaveRemoteFile(ByVal Link As String, ByVal pathFilename As String)
            Try
                '
                Dim HTTP As New coreHttpRequestClass()
                Dim URLLink As String
                '
                If (pathFilename <> "") And (Link <> "") Then
                    pathFilename = Replace(pathFilename, "/", "\")
                    URLLink = Replace(Link, " ", "%20")
                    HTTP.timeout = 600
                    Call HTTP.getUrlToFile(CStr(URLLink), rootLocalFolderPath & pathFilename)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
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

                fastZip.ExtractZip(rootLocalFolderPath & PathFilename, rootLocalFolderPath & getPath(PathFilename), fileFilter)                '
                'Call DeleteFile(PathFilename)
                'Dim Path As String
                'Dim Filename As String
                'Dim Pos As Integer
                'Dim Cmd As String
                'Path = PathFilename
                'Pos = InStrRev(PathFilename, "\")
                'If Pos > 0 Then
                '    Filename = Mid(PathFilename, Pos + 1)
                '    Path = Mid(PathFilename, 1, Pos - 1)
                '    Cmd = getProgramFilesPath() & "7-zip\7z.exe"
                '    '
                '    ' Unzip
                '    '
                '    Cmd = """" & Cmd & """ x """ & rootFullPath & PathFilename & """ -o""" & Path & """ -y "
                '    Call runProcess(cp, Cmd, vbHide, True)
                '    '
                '    ' Clear read-only attributes
                '    '
                '    Cmd = "attrib -R """ & Path & "\*.*"" /S"
                '    Call runProcess(cp, Cmd, vbHide, True)
                '    '
                '    ' Now delete the zip file - this is used by the calling program
                '    ' in case it needs to wait until the is done. This routine is called
                '    ' from teh ccCmd commandline running background tasks asynchronously.
                '    '
                '    Call DeleteFile(PathFilename)
                'End If
            Catch ex As Exception
                cpCore.handleException(ex)
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
                Dim archivepath As String = getPath(archivePathFilename)
                Dim archiveFilename As String = GetFilename(archivePathFilename)
                '
                fastZip.CreateZip(rootLocalFolderPath & archivePathFilename, rootLocalFolderPath & addPathFilename, recurse, fileFilter)
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub        '
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
