Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    '====================================================================================================
    '====================================================================================================
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPContentClass.ClassId, CPContentClass.InterfaceId, CPContentClass.EventsId)>
    Public Class CPContentClass
        Inherits CPContentBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "D8D3D8F9-8459-46F7-B8AC-01B4DFAA4DB2"
        Public Const InterfaceId As String = "9B321DE5-D154-4EB1-B533-DBA2E5F2B5D2"
        Public Const EventsId As String = "6E068297-E09E-42C8-97B6-02DE591009DD"
#End Region
        '
        Private cp As CPClass
        Private cpCore As Contensive.Core.cpCoreClass
        Protected disposed As Boolean = False
        '
        '====================================================================================================
        '
        Friend Sub New(ByVal cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
        '====================================================================================================
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cpCore = Nothing
                    cp = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetCopy(ByVal CopyName As String, Optional ByVal DefaultContent As String = "") As String
            GetCopy = cpCore.html_GetContentCopy(CopyName, DefaultContent, cpCore.user.userId, True, cpCore.user.user_isAuthenticated)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetCopy(ByVal CopyName As String, ByVal DefaultContent As String, ByVal personalizationPeopleId As Integer) As String
            GetCopy = cpCore.html_GetContentCopy(CopyName, DefaultContent, personalizationPeopleId, True, cpCore.user.user_isAuthenticated)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub SetCopy(ByVal CopyName As String, ByVal Content As String)
            Call cpCore.content_SetContentCopy(CopyName, Content)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            Return cpCore.main_GetRecordAddLink2(ContentName, PresetNameValueList, AllowPaste, IsEditing)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetContentControlCriteria(ByVal ContentName As String) As String
            Return cpCore.db_GetContentControlCriteria(ContentName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetFieldProperty(ByVal ContentName As String, ByVal FieldName As String, ByVal PropertyName As String) As String
            Return cpCore.db_GetContentFieldProperty(ContentName, FieldName, PropertyName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetID(ByVal ContentName As String) As Integer
            Return cpCore.metaData.getContentId(ContentName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetProperty(ByVal ContentName As String, ByVal PropertyName As String) As String
            Return cpCore.db_GetContentProperty(ContentName, PropertyName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetDataSource(ByVal ContentName As String) As String
            Return cpCore.metaData.getContentDataSource(ContentName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetEditLink(ByVal ContentName As String, ByVal RecordID As String, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            If True Then
                Return cpCore.main_GetRecordEditLink2(ContentName, RecordID, AllowCut, RecordName, IsEditing)
            Else
                Return ""
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetLinkAliasByPageID(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal DefaultLink As String) As String
            If True Then
                Return cpCore.main_GetLinkAliasByPageID(PageID, QueryStringSuffix, DefaultLink)
            Else
                Return ""
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetPageLink(ByVal PageID As Integer, Optional ByVal QueryStringSuffix As String = "", Optional ByVal AllowLinkAlias As Boolean = True) As String
            If True Then
                Return cpCore.main_GetPageLink3(PageID, QueryStringSuffix, AllowLinkAlias)
            Else
                Return ""
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Return cpCore.db_GetRecordID(ContentName, RecordName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            Return cpCore.db_GetRecordName(ContentName, RecordID)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetTable(ByVal ContentName As String) As String
            Return cpCore.metaData.getContentTablename(ContentName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetTemplateLink(ByVal TemplateID As Integer) As String
            Return cpCore.main_GetTemplateLink(TemplateID)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsField(ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Return cpCore.metaData_IsContentFieldSupported(ContentName, FieldName)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsLocked(ByVal ContentName As String, ByVal RecordID As String) As Boolean 'Inherits CPContentBaseClass.IsLocked
            Return cpCore.workflow.isRecordLocked(ContentName, RecordID, 0)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsChildContent(ByVal ChildContentID As String, ByVal ParentContentID As String) As Boolean 'Inherits CPContentBaseClass.IsChildContent
            Return cpCore.metaData.isWithinContent(ChildContentID, ParentContentID)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsWorkflow(ByVal ContentName As String) As Boolean
            ' ****************************************************
            ' this needs to be added to cmc
            ' ****************************************************
            Return False
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub PublishEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Inherits CPContentBaseClass.PublishEdit
            Call cpCore.workflow.publishEdit(ContentName, RecordID, 0)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SubmitEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Inherits CPContentBaseClass.SubmitEdit
            Call cpCore.workflow.submitEdit2(ContentName, RecordID, 0)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub AbortEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Inherits CPContentBaseClass.AbortEdit
            Call cpCore.workflow.abortEdit2(ContentName, RecordId, 0)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub ApproveEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Inherits CPContentBaseClass.ApproveEdit
            Call cpCore.workflow.approveEdit(ContentName, RecordId, 0)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function getLayout(ByVal layoutName As String) As String
            Dim result As String = ""
            Try
                Dim cs As CPCSClass
                '
                cs = New CPCSClass(cp)
                cs.Open("layouts", "name=" & cp.Db.EncodeSQLText(layoutName), "id",, "layout")
                If cs.OK Then
                    result = cs.GetText("layout")
                End If
                Call cs.Close()
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in getLayout")
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function AddRecord(ByVal ContentName As String, ByVal recordName As String) As Integer
            Dim recordId As Integer = 0
            Try
                Dim cs As CPCSClass = cp.CSNew()
                If cs.Insert(ContentName) Then
                    cs.SetField("name", recordName)
                    recordId = cs.GetInteger("id")
                End If
                Call cs.Close()
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in AddRecord")
            End Try
            Return recordId
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function AddRecord(ContentName As String) As Integer
            Dim recordId As Integer = 0
            Try
                Dim cs As CPCSClass = cp.CSNew()
                If cs.Insert(ContentName) Then
                    recordId = cs.GetInteger("id")
                End If
                Call cs.Close()
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in AddRecord")
            End Try
            Return recordId
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Delete(ByVal ContentName As String, ByVal SQLCriteria As String)
            Call cpCore.db.db_DeleteContentRecords(ContentName, SQLCriteria)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub DeleteRecords(ByVal ContentName As String, ByVal SQLCriteria As String)
            Call cpCore.db.db_DeleteContentRecords(ContentName, SQLCriteria)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub DeleteContent(ContentName As String)
            cpCore.metaData.deleteContent(ContentName)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function AddContentField(ContentName As String, FieldName As String, FieldType As Integer) As Integer
            Dim builder As New coreBuilderClass(cpCore)
            Dim field As New coreMetaDataClass.CDefFieldClass()
            field.active = True
            field.adminOnly = False
            field.authorable = True
            field.blockAccess = False
            field.caption = FieldName
            field.contentId = cpCore.metaData.getContentId(ContentName)
            field.developerOnly = False
            field.editSortPriority = 9999
            field.editTabName = ""
            field.fieldTypeId = FieldType
            field.htmlContent = False
            field.indexColumn = 0
            field.indexSortDirection = 0
            field.indexSortOrder = 0
            field.indexWidth = 0
            field.installedByCollectionGuid = ""
            field.isBaseField = False
            field.lookupContentID = 0
            field.lookupContentName = ""
            field.lookupList = ""
            field.manyToManyContentID = 0
            field.ManyToManyContentName = ""
            field.manyToManyRuleContentID = 0
            field.ManyToManyRuleContentName = ""
            field.ManyToManyRulePrimaryField = ""
            field.ManyToManyRuleSecondaryField = ""
            field.MemberSelectGroupID = 0
            field.MemberSelectGroupName = ""
            field.nameLc = FieldName.ToLower()
            field.Password = False
            field.ReadOnly = False
            field.RedirectContentID = 0
            field.RedirectContentName = ""
            field.RedirectID = 0
            field.RedirectPath = ""
            field.Required = False
            field.Scramble = False
            field.TextBuffered = False
            field.UniqueName = False
            Return builder.metaData_VerifyCDefField_ReturnID(ContentName, field)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function AddContent(ContentName As String) As Integer
            Return AddContent(ContentName, ContentName.Replace(" "c, "").Replace(" "c, ""), "default")
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function AddContent(ContentName As String, sqlTableName As String) As Integer
            Return AddContent(ContentName, sqlTableName, "default")
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function AddContent(ContentName As String, sqlTableName As String, dataSource As String) As Integer
            Dim builder As New coreBuilderClass(cpCore)
            Return builder.metaData_CreateContent4(True _
                , dataSource _
                , sqlTableName _
                , ContentName _
                , False _
                , False _
                , True _
                , False _
                , "" _
                , "sort order" _
                , "name" _
                , False _
                , False _
                , False _
                , False _
                , False _
                , False _
                , "" _
                , 0 _
                , 0 _
                , 0 _
                , "" _
                , False _
                , "" _
                , True
                )
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetListLink(ContentName As String) As String
            Dim returnHtml As String = ""
            Try
                Dim adminUrl As String = cp.Site.GetText("adminUrl") & "?cid=" & cp.Content.GetID(ContentName)
                Dim encodedCaption As String = "List Records in " & cp.Utils.EncodeHTML(ContentName)

                returnHtml = returnHtml _
                    & "<a" _
                    & " class=""ccRecordEditLink"" " _
                    & " TabIndex=-1" _
                    & " href=""" & adminUrl & """" _
                    & "><img" _
                    & " src=""/ccLib/images/IconContentEdit.gif""" _
                    & " border=""0""" _
                    & " alt=""" & encodedCaption & """" _
                    & " title=""" & encodedCaption & """" _
                    & " align=""absmiddle""" _
                    & "></a>"
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in GetListLink")
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ' append to logfile
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.content, " & copy & vbCrLf, True)
        End Sub
        '
        '====================================================================================================
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
        '
        '====================================================================================================
        '
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
