'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    ''' <summary>
    ''' CP.CS - The primary interface to the database. This object is similar to a recordset. It includes features of the content meta data. When a record is inserted, the default values of the record are available to read.
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class CPCSBaseClass
        'public Sub New(ByVal cmcObj As Contensive.Processor.cpCoreClass, ByRef CPParent As CPBaseClass)
        ''' <summary>
        ''' Inserts a new content row
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function Insert(ByVal ContentName As String) As Boolean
        ''' <summary>
        ''' Opens a record set with the record specified by the recordId
        ''' </summary>
        ''' <param name="contentName"></param>
        ''' <param name="recordId"></param>
        ''' <param name="SelectFieldList"></param>
        ''' <param name="activeOnly"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenRecord(ByVal contentName As String, ByVal recordId As Integer, Optional ByVal SelectFieldList As String = "", Optional ByVal activeOnly As Boolean = True) As Boolean
        ''' <summary>
        ''' Opens a record set with the records specified by the sqlCriteria
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="SQLCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="SelectFieldList"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function Open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        'Public MustOverride Function Open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal ignore As Integer = 10, Optional ByVal PageNumber As Integer = 1, Optional ByVal PageSize As Integer = 0) As Boolean
        ''' <summary>
        ''' Opens a record set with user records that are in a Group
        ''' </summary>
        ''' <param name="GroupName"></param>
        ''' <param name="SQLCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenGroupUsers(ByVal GroupName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        ''' <summary>
        ''' Opens a record set with user records that are in a Group
        ''' </summary>
        ''' <param name="GroupList"></param>
        ''' <param name="SQLCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenGroupUsers(ByVal GroupList As List(Of String), Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        ''' <summary>
        ''' deprecated. Use OpenGroupUsers.
        ''' </summary>
        ''' <param name="GroupCommaList"></param>
        ''' <param name="SQLCriteria"></param>
        ''' <param name="SortFieldList"></param>
        ''' <param name="ActiveOnly"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Use OpenGroupUsers instead. The GroupCommaList is a comma delimited list of groups and cannot handle group names that include a comma.", False)>
        Public MustOverride Function OpenGroupListUsers(ByVal GroupCommaList As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        ''' <summary>
        ''' Opens a record set based on an sql statement
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenSQL(ByVal SQL As String) As Boolean
        ''' <summary>
        ''' Opens a record set based on an sql statement
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenSQL(ByVal SQL As String, ByVal DataSourcename As String, Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        ''' <summary>
        ''' Opens a record set based on an sql statement, (polymorphism is not supported by active scripting)
        ''' </summary>
        ''' <param name="SQL"></param>
        ''' <param name="DataSourcename"></param>
        ''' <param name="PageSize"></param>
        ''' <param name="PageNumber"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OpenSQL2(ByVal SQL As String, Optional ByVal DataSourcename As String = "default", Optional ByVal PageSize As Integer = 10, Optional ByVal PageNumber As Integer = 1) As Boolean
        ''' <summary>
        '''  Closes an open record set
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub Close()
        ''' <summary>
        ''' Returns a form input element based on a content field definition
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="Height"></param>
        ''' <param name="Width"></param>
        ''' <param name="HtmlId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetFormInput(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlId As String = "") As Object
        ''' <summary>
        ''' Deletes the current row
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub Delete()
        ''' <summary>
        ''' Returns true if the given field is valid for this record set
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function FieldOK(ByVal FieldName As String) As Boolean
        ''' <summary>
        ''' Move to the first record in the current record set
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub GoFirst()
        ''' <summary>
        ''' Returns an icon linked to the add function in the admin site for this content
        ''' </summary>
        ''' <param name="PresetNameValueList"></param>
        ''' <param name="AllowPaste"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetAddLink(Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
        ''' <summary>
        ''' Returns the field value cast as a boolean
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetBoolean(ByVal FieldName As String) As Boolean
        ''' <summary>
        ''' Returns the field value cast as a date
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetDate(ByVal FieldName As String) As Date
        ''' <summary>
        ''' Returns an icon linked to the edit function in the admin site for this content
        ''' </summary>
        ''' <param name="AllowCut"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetEditLink(Optional ByVal AllowCut As Boolean = False) As String
        ''' <summary>
        ''' Returns the filename for the field, if a filename is related to the field type. Use this call to create the appropriate filename when a new file is added. The filename with the appropriate path is created or returned. This file and path is relative to the site's content file path and does not include a leading slash. To use this file in a URL, prefix with cp.site.filepath.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <param name="OriginalFilename"></param>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetFilename(ByVal FieldName As String, Optional ByVal OriginalFilename As String = "", Optional ByVal ContentName As String = "") As String
        ''' <summary>
        ''' Returns the field value cast as an integer
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetInteger(ByVal FieldName As String) As Integer
        ''' <summary>
        ''' Returns the field value cast as a number (double)
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetNumber(ByVal FieldName As String) As Double
        ''' <summary>
        ''' Returns the number of rows in the result.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetRowCount() As Integer
        ''' <summary>
        ''' returns the query used to generate the results
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetSQL() As String
        ''' <summary>
        ''' Returns the result and converts it to a text type. For field types that store text in files, the text is returned instead of the filename. These include textfile, cssfile, javascriptfile. For file types that do not contain text, the filename is returned. These include filetype and imagefiletype.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetText(ByVal FieldName As String) As String
        ''' <summary>
        ''' Returns the result of getText() after verifying it's content is valid for use in Html content. If the field is a fieldTypeHtml the content is returned without conversion. If the field is any other type, the content is HtmlEncoded first (> converted to &gt;, etc)
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetHtml(ByVal FieldName As String) As String
        ''' <summary>
        ''' Deprecated. Returns the filename for field types that store text in files.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Use getText to get copy, getFilename to get file.", False)> Public MustOverride Function GetTextFile(ByVal FieldName As String) As String
        ''' <summary>
        ''' Move to the next record in a result set.
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub GoNext()
        ''' <summary>
        ''' Move to the next record in a result set and return true if the row is valid.
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Function NextOK() As Boolean
        ''' <summary>
        ''' Returns true if there is valid data in the current row of the result set.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function OK() As Boolean
        ''' <summary>
        ''' Forces a save of any changes made to the current row. A save occurs automatically when the content set is closed or when it moves to another row.
        ''' </summary>
        ''' <remarks></remarks>
        Public MustOverride Sub Save()
        ''' <summary>
        ''' Sets a value in a field of the current row.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <param name="FieldValue"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetField(ByVal FieldName As String, ByVal FieldValue As Object)
        Public MustOverride Sub SetField(ByVal FieldName As String, ByVal FieldValue As String)
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <param name="Copy"></param>
        ''' <param name="ContentName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetFile(ByVal FieldName As String, ByVal Copy As String, ByVal ContentName As String)
        ''' <summary>
        ''' Processes a value from the incoming request to a field in the current row.
        ''' </summary>
        ''' <param name="FieldName"></param>
        ''' <param name="RequestName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetFormInput(ByVal FieldName As String, Optional ByVal RequestName As String = "")
        ''' <summary>
        ''' Return the value directly from the field, without the conversions associated with GetText().
        ''' </summary>
        ''' <param name="fieldName"></param>
        ''' <returns></returns>
        Public MustOverride Function GetValue(ByVal fieldName As String) As String

    End Class

End Namespace

