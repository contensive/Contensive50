
'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPFileBaseClass
        '
        '
        '
        Public MustOverride ReadOnly Property cdnFiles() As CPFileSystemBaseClass
        Public MustOverride ReadOnly Property appRootFiles() As CPFileSystemBaseClass
        Public MustOverride ReadOnly Property privateFiles() As CPFileSystemBaseClass
        Public MustOverride ReadOnly Property serverFiles() As CPFileSystemBaseClass
        ''' <summary>
        ''' Append content to a text file in the content files. If the file does not exist it will be created.
        ''' </summary>
        ''' <param name="Filename">The filename of the file to be appended. May include subfolders in the content file area. It should not include a leading slash. Folder slashes should be \.</param>
        ''' <param name="FileContent">Test appended to the file</param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdn, cp.File.private, cp.File.appRoot, or cp.File.server instead.", False)>
        Public MustOverride Sub AppendVirtual(ByVal Filename As String, ByVal FileContent As String)
        ''' <summary>
        ''' Copies a file in the content file area to another. If the destination does not exist it is created. Filenames may include subfolders but should not include a leading slash.
        ''' </summary>
        ''' <param name="SourceFilename"></param>
        ''' <param name="DestinationFilename"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub CopyVirtual(ByVal SourceFilename As String, ByVal DestinationFilename As String)
        ''' <summary>
        ''' Create a folder given a physical folder path.
        ''' </summary>
        ''' <param name="FolderPath"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub CreateFolder(ByVal FolderPath As String)
        ''' <summary>
        ''' Delete a file within the file space.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub Delete(ByVal Filename As String)
        ''' <summary>
        ''' Delete a folder within the file space
        ''' </summary>
        ''' <param name="folderPath"></param>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub DeleteFolder(ByVal folderPath As String)
        ''' <summary>
        ''' Delete a file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub DeleteVirtual(ByVal Filename As String)
        ''' <summary>
        ''' Read a text file within the file space.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function Read(ByVal Filename As String) As String
        ''' <summary>
        ''' Read a text file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function ReadVirtual(ByVal Filename As String) As String
        ''' <summary>
        ''' Save or create a text file within the file space.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <param name="FileContent"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub Save(ByVal Filename As String, ByVal FileContent As String)
        ''' <summary>
        ''' Save a text file in the content file area. The filename may contain subfolders and should not begin with a leading slash.
        ''' </summary>
        ''' <param name="Filename"></param>
        ''' <param name="FileContent"></param>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Sub SaveVirtual(ByVal Filename As String, ByVal FileContent As String)
        ''' <summary>
        ''' Get a crlf delimited list of files in a given path. Each row is a tab delimited list of attributes for each file. The attributes are:
        ''' FileName
        ''' Attributes
        ''' DateCreated
        ''' DateLastAccessed
        ''' DateLastModified
        ''' Size
        ''' Type
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function fileList(ByVal folderName As String, Optional ByVal pageSize As Integer = 0, Optional ByVal pageNumber As Integer = 1) As String
        ''' <summary>
        ''' Get a crlf delimited list of folders in a given path. Each row is a tab delimited list of attributes for each folder. The attributes are:
        ''' Name
        ''' Attributes
        ''' DateCreated
        ''' DateLastAccessed
        ''' DateLastModified
        ''' Type
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function folderList(ByVal folderName As String) As String
        ''' <summary>
        ''' Returns true if a file exists
        ''' </summary>
        ''' <param name="pathFileName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function fileExists(ByVal pathFileName As String) As Boolean
        ''' <summary>
        ''' Returns true if a folder exists
        ''' </summary>
        ''' <param name="folderName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function folderExists(ByVal folderName As String) As Boolean
        ''' <summary>
        ''' Returns a URL to a file in the File.cdn store
        ''' </summary>
        ''' <param name="virtualFilename"></param>
        ''' <returns></returns>
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", False)>
        Public MustOverride Function getVirtualFileLink(ByVal virtualFilename As String) As String
    End Class

End Namespace

