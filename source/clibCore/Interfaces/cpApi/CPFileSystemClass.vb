
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript and compatible
    '
    ''' <summary>
    ''' cpFileSystemClass is the api wrapper that implements cpFileSystemBaseClass using the cpCore_fileSystemClass 
    ''' </summary>
    <ComVisible(True)>
    <ComClass(CPFileSystemClass.ClassId, CPFileSystemClass.InterfaceId, CPFileSystemClass.EventsId)>
    Public Class CPFileSystemClass
        Inherits BaseClasses.CPFileSystemBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "0B73809E-F149-4262-A548-FA1E11DF63A6"
        Public Const InterfaceId As String = "4F8288A4-2854-4B60-9281-9A776DC101D0"
        Public Const EventsId As String = "987E6DDE-E9E6-46C5-9467-BAE79A129A15"
#End Region
        '
        Private core As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        Private fileSystem As fileController
        '
        '==========================================================================================
        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="core"></param>
        Public Sub New(ByVal core As Contensive.Core.coreClass, fileSystem As fileController)
            MyBase.New()
            Me.core = core
            Me.fileSystem = fileSystem
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
                    core = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '==========================================================================================
        Public Overrides Sub append(ByVal filename As String, ByVal fileContent As String)
            Call fileSystem.appendFile(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        Public Overrides Sub copy(ByVal sourceFilename As String, ByVal destinationFilename As String)
            Call fileSystem.copyFile(sourceFilename, destinationFilename)
        End Sub
        '
        '==========================================================================================
        Public Overrides Sub createFolder(ByVal folderPath As String)
            Call fileSystem.createPath(folderPath)
        End Sub
        '
        '==========================================================================================
        Public Overrides Sub deleteFile(ByVal filename As String)
            Call fileSystem.deleteFile(filename)
        End Sub
        '
        '==========================================================================================
        Public Overrides Function read(ByVal filename As String) As String
            Return fileSystem.readFile(filename)
        End Function
        '
        '==========================================================================================
        Public Overrides Function readBinary(ByVal filename As String) As Byte()
            Return fileSystem.ReadBinaryFile(filename)
        End Function
        '
        '==========================================================================================
        Public Overrides Sub save(ByVal filename As String, ByVal fileContent As String)
            Call fileSystem.saveFile(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        Public Overrides Sub saveBinary(ByVal filename As String, ByVal fileContent As Byte())
            Call fileSystem.SaveFile(filename, fileContent)
        End Sub
        '
        '==========================================================================================
        Public Overrides Function fileExists(ByVal pathFileName As String) As Boolean
            fileExists = fileSystem.fileExists(pathFileName)
        End Function
        '
        '==========================================================================================
        Public Overrides Function folderExists(ByVal folderName As String) As Boolean
            folderExists = fileSystem.pathExists(folderName)
        End Function
        '
        '==========================================================================================
        Public Overrides Function fileList(ByVal folderName As String, Optional ByVal pageSize As Integer = 0, Optional ByVal pageNumber As Integer = 1) As IO.FileInfo()
            Return fileSystem.getFileList(folderName)
        End Function
        '
        '==========================================================================================
        Public Overrides Function folderList(ByVal folderName As String) As IO.DirectoryInfo()
            Return fileSystem.getFolderList(folderName)
        End Function
        '
        '==========================================================================================
        Public Overrides Sub deleteFolder(folderPath As String)
            Call fileSystem.DeleteFileFolder(folderPath)
        End Sub
        '
        '==========================================================================================
        '
        Public Overrides Function saveUpload(htmlformName As String, ByRef returnFilename As String) As Boolean
            Return fileSystem.saveUpload(htmlformName, "\upload", returnFilename)
        End Function
        '
        '==========================================================================================
        '
        Public Overrides Function saveUpload(htmlformName As String, folderpath As String, ByRef returnFilename As String) As Boolean
            Return fileSystem.saveUpload(htmlformName, folderpath, returnFilename)
        End Function
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