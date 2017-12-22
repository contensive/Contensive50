
'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPFileSystemBaseClass
        '
        Public MustOverride Sub append(ByVal Filename As String, ByVal FileContent As String)
        Public MustOverride Sub copy(ByVal SourceFilename As String, ByVal DestinationFilename As String)
        Public MustOverride Sub createFolder(ByVal FolderPath As String)
        Public MustOverride Sub deleteFile(ByVal Filename As String)
        Public MustOverride Sub deleteFolder(ByVal folderPath As String)
        Public MustOverride Function read(ByVal Filename As String) As String
        Public MustOverride Function readBinary(ByVal Filename As String) As Byte()
        Public MustOverride Sub save(ByVal Filename As String, ByVal FileContent As String)
        Public MustOverride Sub saveBinary(ByVal Filename As String, ByVal FileContent As Byte())
        Public MustOverride Function fileList(ByVal folderName As String, Optional ByVal pageSize As Integer = 0, Optional ByVal pageNumber As Integer = 1) As IO.FileInfo()
        Public MustOverride Function folderList(ByVal folderName As String) As IO.DirectoryInfo()
        Public MustOverride Function fileExists(ByVal pathFileName As String) As Boolean
        Public MustOverride Function folderExists(ByVal folderName As String) As Boolean
        Public MustOverride Function saveUpload(ByVal htmlformName As String, ByRef returnFilename As String) As Boolean
        Public MustOverride Function saveUpload(ByVal htmlformName As String, ByVal folderpath As String, ByRef returnFilename As String) As Boolean
    End Class
End Namespace

