
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript and compatible
    '
    ''' <summary>
    ''' cpFileClass is a legacy implementation replaced with cdnFiles, appRootFiles and privateFiles. Non-Virtual calls do not limit file destination so are not scale-mode compatible
    ''' </summary>
    <ComVisible(True)>
    <ComClass(CPFileClass.ClassId, CPFileClass.InterfaceId, CPFileClass.EventsId)>
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
            Return genericcontroller.getCdnFileLink(cpCore, virtualFilename)
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
            Call cpCore.cdnFiles.appendFile(filename, fileContent)
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
            Call cpCore.cdnFiles.copyFile(sourceFilename, destinationFilename)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Create a folder anywhere on the physical file space of the hosting server. Deprecated, use with cp.file.cdnFiles, cp.file.appRootFiles, or cp.file.privateFiles
        ''' </summary>
        ''' <param name="folderPath"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub createFolder(ByVal folderPath As String)
            If cpCore.appRootFiles.isinPhysicalPath(folderPath) Then
                Call cpCore.appRootFiles.createPath(folderPath)
            ElseIf cpCore.privateFiles.isinPhysicalPath(folderPath) Then
                Call cpCore.privateFiles.createPath(folderPath)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(folderPath) Then
                Call cpCore.cdnFiles.createPath(folderPath)
            Else
                throw (New ApplicationException("Application cannot access this path [" & folderPath & "]"))
            End If
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFilename"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub delete(ByVal pathFilename As String)
            If cpCore.appRootFiles.isinPhysicalPath(pathFilename) Then
                Call cpCore.appRootFiles.deleteFile(pathFilename)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFilename) Then
                Call cpCore.privateFiles.deleteFile(pathFilename)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFilename) Then
                Call cpCore.cdnFiles.deleteFile(pathFilename)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFilename & "]"))
            End If
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a file in the cdnFiles store.
        ''' </summary>
        ''' <param name="pathFilename"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub deleteVirtual(ByVal pathFilename As String)
            Call cpCore.cdnFiles.deleteFile(pathFilename)
        End Sub
        '
        '==========================================================================================
        ''' <summary>
        ''' Save a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function read(ByVal pathFilename As String) As String
            If cpCore.appRootFiles.isinPhysicalPath(pathFilename) Then
                Return cpCore.appRootFiles.readFile(pathFilename)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFilename) Then
                Return cpCore.privateFiles.readFile(pathFilename)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFilename) Then
                Return cpCore.cdnFiles.readFile(pathFilename)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFilename & "]"))
            End If
            Return String.Empty
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Read a file from the cdnFiles store.
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function readVirtual(ByVal pathFilename As String) As String
            Return cpCore.cdnFiles.readFile(pathFilename)
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Save a file anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFilename"></param>
        ''' <param name="fileContent"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub save(ByVal pathFilename As String, ByVal fileContent As String)
            If cpCore.appRootFiles.isinPhysicalPath(pathFilename) Then
                cpCore.appRootFiles.saveFile(pathFilename, fileContent)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFilename) Then
                cpCore.privateFiles.saveFile(pathFilename, fileContent)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFilename) Then
                cpCore.cdnFiles.saveFile(pathFilename, fileContent)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFilename & "]"))
            End If
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
            Call cpCore.cdnFiles.saveFile(filename, fileContent)
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
            Dim result As Boolean = False
            If cpCore.appRootFiles.isinPhysicalPath(pathFileName) Then
                result = cpCore.appRootFiles.fileExists(pathFileName)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFileName) Then
                result = cpCore.privateFiles.fileExists(pathFileName)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFileName) Then
                result = cpCore.cdnFiles.fileExists(pathFileName)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFileName & "]"))
            End If
            Return result
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Test if a folder exists anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFolderName"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function folderExists(ByVal pathFolderName As String) As Boolean
            Dim result As Boolean = False
            If cpCore.appRootFiles.isinPhysicalPath(pathFolderName) Then
                result = cpCore.appRootFiles.pathExists(pathFolderName)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFolderName) Then
                result = cpCore.privateFiles.pathExists(pathFolderName)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFolderName) Then
                result = cpCore.cdnFiles.pathExists(pathFolderName)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFolderName & "]"))
            End If
            Return result
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Return a parsable comma,crlf delimited string of the files available anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFolderName"></param>
        ''' <param name="pageSize"></param>
        ''' <param name="pageNumber"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function fileList(ByVal pathFolderName As String, Optional ByVal pageSize As Integer = 0, Optional ByVal pageNumber As Integer = 1) As String
            Dim result As String = ""
            If cpCore.appRootFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.FileInfo() = cpCore.appRootFiles.getFileList(pathFolderName)
                result = cpCore.appRootFiles.convertFileINfoArrayToParseString(fi)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.FileInfo() = cpCore.privateFiles.getFileList(pathFolderName)
                result = cpCore.privateFiles.convertFileINfoArrayToParseString(fi)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.FileInfo() = cpCore.cdnFiles.getFileList(pathFolderName)
                result = cpCore.cdnFiles.convertFileINfoArrayToParseString(fi)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFolderName & "]"))
            End If
            Return result
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Return a parsable comma,crlf delimited string of the folders available anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFolderName"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Function folderList(ByVal pathFolderName As String) As String
            Dim result As String = ""
            If cpCore.appRootFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.DirectoryInfo() = cpCore.appRootFiles.getFolderList(pathFolderName)
                result = cpCore.appRootFiles.convertDirectoryInfoArrayToParseString(fi)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.DirectoryInfo() = cpCore.privateFiles.getFolderList(pathFolderName)
                result = cpCore.privateFiles.convertDirectoryInfoArrayToParseString(fi)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFolderName) Then
                Dim fi As IO.DirectoryInfo() = cpCore.cdnFiles.getFolderList(pathFolderName)
                result = cpCore.cdnFiles.convertDirectoryInfoArrayToParseString(fi)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFolderName & "]"))
            End If
            Return result
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Delete a folder anywhere on the physical file space of the hosting server.
        ''' </summary>
        ''' <param name="pathFolderName"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.Files.serverFiles instead.", False)>
        Public Overrides Sub DeleteFolder(pathFolderName As String)
            If cpCore.appRootFiles.isinPhysicalPath(pathFolderName) Then
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName)
            ElseIf cpCore.privateFiles.isinPhysicalPath(pathFolderName) Then
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName)
            ElseIf cpCore.cdnFiles.isinPhysicalPath(pathFolderName) Then
                cpCore.appRootFiles.DeleteFileFolder(pathFolderName)
            Else
                throw (New ApplicationException("Application cannot access this path [" & pathFolderName & "]"))
            End If
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