
Option Explicit On
Option Strict On
'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPContentBaseClass
        ''' <summary>
        ''' Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
        ''' </summary>
        ''' <param name="CopyName"></param>
        ''' <param name="DefaultContent"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetCopy(ByVal CopyName As String, Optional ByVal DefaultContent As String = "") As String
        ''' <summary>
        ''' Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
        ''' </summary>
        ''' <param name="CopyName"></param>
        ''' <param name="DefaultContent"></param>
        ''' <param name="personalizationPeopleId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetCopy(ByVal CopyName As String, ByVal DefaultContent As String, personalizationPeopleId As Integer) As String
        ''' <summary>
        ''' Set a string in a 'Copy Content' record. The record will be created or modified.
        ''' </summary>
        ''' <param name="CopyName"></param>
        ''' <param name="Content"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SetCopy(ByVal CopyName As String, ByVal Content As String)
        ''' <summary>
        ''' Get an icon linked to the administration site which adds a new record to the content.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="PresetNameValueList">A comma delimited list of name=value pairs. Each name is a field name and the value is used to prepopulate the new record.</param>
        ''' <param name="AllowPaste">If true and the content supports cut-paste from the public site, the returned string will include a cut icon.</param>
        ''' <param name="IsEditing">If false, this call returns nothing. Set it true if IsEdiing( contentname ) is true.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String 'Implements BaseClasses.CPContentBaseClass.GetAddLink
        ''' <summary>
        ''' Returns an SQL compatible where-clause which includes all the contentcontentid values allowed for this content name.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetContentControlCriteria(ByVal ContentName As String) As String 'Implements BaseClasses.CPContentBaseClass.GetContentControlCriteria
        ''' <summary>
        ''' Returns a named field property. Valid values for PropertyName are the field names of the 'Content Fields' content definition, also found as the columns in the ccfields table.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetFieldProperty(ByVal ContentName As String, ByVal FieldName As String, ByVal PropertyName As String) As String 'Implements BaseClasses.CPContentBaseClass.GetFieldProperty
        ''' <summary>
        ''' Returns the content id given its name
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetID(ByVal ContentName As String) As Integer 'Implements BaseClasses.CPContentBaseClass.GetId
        ''' <summary>
        ''' Returns a named content property. Valid values for PropertyName are the field names of the 'Content' content definition, also found as the columns in the ccfields table.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetProperty(ByVal ContentName As String, ByVal PropertyName As String) As String 'Implements BaseClasses.CPContentBaseClass.GetProperty
        ''' <summary>
        ''' Returns the datasource name of the content given.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetDataSource(ByVal ContentName As String) As String 'Implements BaseClasses.CPContentBaseClass.GetDataSource
        ''' <summary>
        ''' Get an icon linked to the administration site which edits the referenced record. The record is identified by its ID. The recordname is only used for the display caption.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <param name="AllowCut">If true and the content allows cut and paste, and cut icon will be included in the return string.</param>
        ''' <param name="RecordName">Used as a caption for the label</param>
        ''' <param name="IsEditing">If false, this call returns nothing. Set it true if IsEdiing( contentname ) is true.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetEditLink(ByVal ContentName As String, ByVal RecordID As String, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String 'Implements BaseClasses.CPContentBaseClass.GetEditLink
        ''' <summary>
        ''' Returns the primary link alias for the record id and querystringsuffix. If no link alias exists, it defaultvalue is returned.
        ''' </summary>
        ''' <param name="PageID"></param>
        ''' <param name="QueryStringSuffix">In the case where an add-on is on the page, there may be many unique documents possible from the one pageid. Each possible variation is determined by values in the querystring added by the cpcore.addon. These name=value pairs in Querystring format are used to identify additional link aliases.</param>
        ''' <param name="DefaultLink">If no link alias is found, this value is returned.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetLinkAliasByPageID(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal DefaultLink As String) As String 'Implements BaseClasses.CPContentBaseClass.GetLinkAliasByPageId
        ''' <summary>
        ''' Return the appropriate link for a page.
        ''' </summary>
        ''' <param name="PageID"></param>
        ''' <param name="QueryStringSuffix">If a link alias exists, this is used to lookup the correct alias. See GetLinkAliasByPageID for details. In other cases, this is added to the querystring.</param>
        ''' <param name="AllowLinkAlias"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetPageLink(ByVal PageID As Integer, Optional ByVal QueryStringSuffix As String = "", Optional ByVal AllowLinkAlias As Boolean = True) As String 'Implements BaseClasses.CPContentBaseClass.GetPageLink
        ''' <summary>
        ''' Return a record's ID given it's name. If duplicates exist, the first one ordered by ID is returned.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer 'Implements BaseClasses.CPContentBaseClass.GetRecordId
        ''' <summary>
        ''' Return a records name given it's ID.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String 'Implements BaseClasses.CPContentBaseClass.GetRecordName
        ''' <summary>
        ''' Get the table used for a content definition.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetTable(ByVal ContentName As String) As String 'Implements BaseClasses.CPContentBaseClass.GetTable
        ''' <summary>
        ''' If a template uses a fixed URL, the returns the link associted with a template. Otherwise it returns a blank string.
        ''' </summary>
        ''' <param name="TemplateID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetTemplateLink(ByVal TemplateID As Integer) As String 'Implements BaseClasses.CPContentBaseClass.GetTemplateLink
        ''' <summary>
        ''' Used to test if a field exists in a content definition
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function IsField(ByVal ContentName As String, ByVal FieldName As String) As Boolean 'Implements BaseClasses.CPContentBaseClass.IsField
        ''' <summary>
        ''' Returns true if the record is currently being edited.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function IsLocked(ByVal ContentName As String, ByVal RecordID As String) As Boolean 'Implements BaseClasses.CPContentBaseClass.IsLocked
        ''' <summary>
        ''' Returns true if the childcontentid is a child of the parentcontentid
        ''' </summary>
        ''' <param name="ChildContentID"></param>
        ''' <param name="ParentContentID"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function IsChildContent(ByVal ChildContentID As String, ByVal ParentContentID As String) As Boolean 'Implements BaseClasses.CPContentBaseClass.IsChildContent
        ''' <summary>
        ''' Returns true if the content is currently using workflow editing.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function IsWorkflow(ByVal ContentName As String) As Boolean 'Implements BaseClasses.CPContentBaseClass.IsWorkflow
        ''' <summary>
        ''' If Workflow editing, the record is published.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub PublishEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Implements BaseClasses.CPContentBaseClass.PublishEdit
        ''' <summary>
        ''' If workflow editing, the record is submitted for pushlishing
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordID"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub SubmitEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Implements BaseClasses.CPContentBaseClass.SubmitEdit
        ''' <summary>
        ''' If workflow editing, edits to the record are aborted and the edit record is returned to the condition of hte live record. This condition is used in the Workflow publishing tool.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordId"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub AbortEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Implements BaseClasses.CPContentBaseClass.AbortEdit
        ''' <summary>
        ''' If workflow editing, the record is marked as approved for publishing. This condition is used in the Workflow publishing tool.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="RecordId"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub ApproveEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Implements BaseClasses.CPContentBaseClass.ApproveEdit
        ''' <summary>
        ''' Returns the html layout field of a layout record
        ''' </summary>
        ''' <param name="layoutName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function getLayout(ByVal layoutName As String) As String
        ''' <summary>
        ''' Inserts a record and returns the Id for the record
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        <Obsolete("Please use AddRecord(ContentName as String)", True)>
        Public MustOverride Function AddRecord(ByVal ContentName As Object) As Integer
        Public MustOverride Function AddRecord(ByVal ContentName As String) As Integer
        Public MustOverride Function AddRecord(ByVal ContentName As String, ByVal recordName As String) As Integer
        ''' <summary>
        ''' Delete records based from a table based on a content name and SQL criteria.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <remarks></remarks>
        '''
        Public MustOverride Function AddContent(ByVal ContentName As String) As Integer
        Public MustOverride Function AddContent(ByVal ContentName As String, sqlTableName As String) As Integer
        Public MustOverride Function AddContent(ByVal ContentName As String, sqlTableName As String, dataSource As String) As Integer
        ''' <summary>
        ''' Create a new field in an existing content, return the fieldid
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="FieldName"></param>
        ''' <param name="FieldType"></param>
        ''' <returns></returns>
        Public MustOverride Function AddContentField(ByVal ContentName As String, FieldName As String, FieldType As Integer) As Integer
        ''' <summary>
        ''' Delete a content from the system, sqlTable is left intact. Use db.DeleteTable to drop the table
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub DeleteContent(ByVal ContentName As String)
        ''' <summary>
        ''' Delete records based from a table based on a content name and SQL criteria.
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="SQLCriteria"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub Delete(ByVal ContentName As String, ByVal SQLCriteria As String)
        ''' <summary>
        ''' Returns a linked icon to the admin list page for the content
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride Function GetListLink(ByVal ContentName As String) As String
    End Class

End Namespace

