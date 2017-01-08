
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript and compatible
    '
    ''' <summary>
    ''' cpFileClass is a legacy implementation replaced with cdnFiles, appRootFiles and privateFiles. Non-Virtual calls do not limit file destination so are not scale-mode compatible
    ''' </summary>
    <ComVisible(True)> _
    <ComClass(CPFileClass.ClassId, CPFileClass.InterfaceId, CPFileClass.EventsId)> _
    Public Class CPFileClass
        Inherits BaseClasses.CPFileBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "E3310DFA-0ABF-4DC7-ABB5-4D294D30324B"
        Public Const InterfaceId As String = "44C305D8-A8C3-490D-8E79-E17F9B3D34CE"
        Public Const EventsId As String = "8757DE11-C04D-4765-B46B-458E281BAE19"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        Private _cdnFiles As CPFileSystemClass
        Private _appRootFiles As CPFileSystemClass
        Private _privateFiles As CPFileSystemClass
        Private _serverFiles As CPFileSystemClass
        '
        '==========================================================================================
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="core"></param>
        Public Sub New(ByVal core As Contensive.Core.coreClass)
            MyBase.New()
            Me.cpCore = core
        End Sub
        '
        Public Overrides ReadOnly Property cdnFiles() As CPFileSystemBaseClass
            Get
                If (_cdnFiles Is Nothing) Then
                    _cdnFiles = New CPFileSystemClass(cpCore, cpCore.cdnFiles)
                End If
                Return _cdnFiles
            End Get
        End Property
        '
        Public Overrides ReadOnly Property appRootFiles() As CPFileSystemBaseClass
            Get
                If (_appRootFiles Is Nothing) Then
                    _appRootFiles = New CPFileSystemClass(cpCore, cpCore.appRootFiles)
                End If
                Return _appRootFiles
            End Get
        End Property
        '
        Public Overrides ReadOnly Property privateFiles() As CPFileSystemBaseClass
            Get
                If (_privateFiles Is Nothing) Then
                    _privateFiles = New CPFileSystemClass(cpCore, cpCore.privateFiles)
                End If
                Return _privateFiles
            End Get
        End Property
        '
        Public Overrides ReadOnly Property serverFiles() As CPFileSystemBaseClass
            Get
                If (_serverFiles Is Nothing) Then
                    _serverFiles = New CPFileSystemClass(cpCore, cpCore.serverFiles)
                End If
                Return _serverFiles
            End Get
        End Property
        '
        '==========================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    If Not (_cdnFiles Is Nothing) Then _cdnFiles.Dispose()
                    If Not (_appRootFiles Is Nothing) Then _appRootFiles.Dispose()
                    If Not (_privateFiles Is Nothing) Then _privateFiles.Dispose()
                    If Not (_serverFiles Is Nothing) Then _serverFiles.Dispose()
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Convert a filepath in the cdnFiles store to a URL
        ''' </summary>
        ''' <param name="virtualFilename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function getVirtualFileLink(virtualFilename As String) As String
            Return cpCore.csv_getVirtualFileLink(cpCore.appConfig.cdnFilesNetprefix, virtualFilename)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Append a file in the cdnFiles store. Deprecated, use cp.file.cdn.appendFile
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <param name="fileContent"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub appendVirtual(ByVal filename As String, ByVal fileContent As String)
            Call cdnFiles.append(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Copy a file within cdnFiles.
        ''' </summary>
        ''' <param name="sourceFilename"></param>
        ''' <param name="destinationFilename"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub copyVirtual(ByVal sourceFilename As String, ByVal destinationFilename As String)
            Call cdnFiles.copy(sourceFilename, destinationFilename)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Create a folder anywhere on the physical file space of the hosting server. Deprecated, use with cp.file.cdnFiles, cp.file.appRootFiles, or cp.file.privateFiles
        ''' </summary>
        ''' <param name="folderPath"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub createFolder(ByVal folderPath As String)
            Call serverFiles.createFolder(folderPath)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="filename"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub delete(ByVal filename As String)
            Call serverFiles.delete(filename)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a file in the cdnFiles store.
        ''' </summary>
        ''' <param name="filename"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub deleteVirtual(ByVal filename As String)
            Call cdnFiles.delete(filename)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Save a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function read(ByVal filename As String) As String
            Return cpCore.appRootFiles.ReadFile(filename)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Read a file from the cdnFiles store.
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function readVirtual(ByVal filename As String) As String
            Return cdnFiles.read(filename)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Save a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <param name="fileContent"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub save(ByVal filename As String, ByVal fileContent As String)
            Call serverFiles.save(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Save a file in the cdnFiles store.
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <param name="fileContent"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub saveVirtual(ByVal filename As String, ByVal fileContent As String)
            Call cdnFiles.save(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Test if a file exists anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFileName"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function fileExists(ByVal pathFileName As String) As Boolean
            fileExists = serverFiles.fileExists(pathFileName)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Test if a folder exists anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function folderExists(ByVal folderName As String) As Boolean
            folderExists = serverFiles.folderExists(folderName)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Return a parsable comma,crlf delimited string of the files available anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <param name="pageSize"></param>
        ''' <param name="pageNumber"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function fileList(ByVal folderName As String, Optional ByVal pageSize As Integer = 0, Optional ByVal pageNumber As Integer = 1) As String
            Dim fi As IO.FileInfo() = serverFiles.fileList(folderName)
            Return cpCore.cluster.localClusterFiles.convertFileINfoArrayToParseString(fi)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Return a parsable comma,crlf delimited string of the folders available anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function folderList(ByVal folderName As String) As String
            Dim di As IO.DirectoryInfo() = serverFiles.folderList(folderName)
            Return cpCore.cluster.localClusterFiles.convertDirectoryInfoArrayToParseString(di)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a folder anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="folderPath"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub DeleteFolder(folderPath As String)
            Throw New NotImplementedException()
        End Sub
#Region " IDisposable Support "
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