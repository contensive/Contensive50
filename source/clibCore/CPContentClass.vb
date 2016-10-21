Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPContentClass.ClassId, CPContentClass.InterfaceId, CPContentClass.EventsId)> _
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
        Friend Sub New(ByVal cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
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

        Public Overrides Sub Delete(ByVal ContentName As String, ByVal SQLCriteria As String)
            Call cpCore.app.db_DeleteContentRecords(ContentName, SQLCriteria)
        End Sub
        '
        '
        '
        Public Overrides Function GetCopy(ByVal CopyName As String, Optional ByVal DefaultContent As String = "") As String
            Return GetCopy(CopyName, DefaultContent, 0)
        End Function
        '
        '
        '
        Public Overrides Function GetCopy(ByVal CopyName As String, ByVal DefaultContent As String, ByVal personalizationPeopleId As Integer) As String
            If True Then
                GetCopy = cpCore.main_GetContentCopy2(CopyName, "Copy Content", DefaultContent)
            Else
                GetCopy = cpCore.csv_GetContentCopy(CopyName, "Copy Content", DefaultContent, personalizationPeopleId)
            End If
        End Function

        Public Overrides Sub SetCopy(ByVal CopyName As String, ByVal Content As String)
            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            If True Then
                Call cpCore.main_SetContentCopy(CopyName, Content)
            End If
        End Sub


        Public Overrides Function GetAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            If True Then
                Return cpCore.main_GetRecordAddLink2(ContentName, PresetNameValueList, AllowPaste, IsEditing)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetContentControlCriteria(ByVal ContentName As String) As String
            Return cpCore.csv_GetContentControlCriteria(ContentName)
        End Function

        Public Overrides Function GetFieldProperty(ByVal ContentName As String, ByVal FieldName As String, ByVal PropertyName As String) As String
            If True Then
                Return cpCore.main_GetContentFieldProperty(ContentName, FieldName, PropertyName)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetID(ByVal ContentName As String) As Integer
            Return cpCore.csv_GetContentID(ContentName)
        End Function

        Public Overrides Function GetProperty(ByVal ContentName As String, ByVal PropertyName As String) As String
            If True Then
                Return cpCore.main_GetContentProperty(ContentName, PropertyName)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetDataSource(ByVal ContentName As String) As String
            Return cpCore.csv_GetContentDataSource(ContentName)
        End Function

        Public Overrides Function GetEditLink(ByVal ContentName As String, ByVal RecordID As String, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            If True Then
                Return cpCore.main_GetRecordEditLink2(ContentName, RecordID, AllowCut, RecordName, IsEditing)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetLinkAliasByPageID(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal DefaultLink As String) As String
            If True Then
                Return cpCore.main_GetLinkAliasByPageID(PageID, QueryStringSuffix, DefaultLink)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function GetPageLink(ByVal PageID As Integer, Optional ByVal QueryStringSuffix As String = "", Optional ByVal AllowLinkAlias As Boolean = True) As String
            If True Then
                Return cpCore.main_GetPageLink3(PageID, QueryStringSuffix, AllowLinkAlias)
            Else
                Return ""
            End If
        End Function



        Public Overrides Function GetRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Dim expression As Integer

            GetRecordID = 0
            Try
                expression = cpCore.csv_GetRecordID(ContentName, RecordName)
                If IsNumeric(expression) Then
                    GetRecordID = CInt(expression)
                Else
                    GetRecordID = 0
                End If
            Catch ex As Exception
                GetRecordID = 0
            End Try


        End Function

        Public Overrides Function GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            Dim Expression As String
            GetRecordName = ""
            Try
                Expression = cpCore.csv_GetRecordName(ContentName, RecordID)
                If (Expression Is Nothing) Then
                    GetRecordName = ""
                Else
                    GetRecordName = CStr(Expression)
                End If
            Catch
                GetRecordName = ""
            End Try

        End Function

        Public Overrides Function GetTable(ByVal ContentName As String) As String
            Return cpCore.csv_GetContentTablename(ContentName)
        End Function

        Public Overrides Function GetTemplateLink(ByVal TemplateID As Integer) As String
            If True Then
                Return cpCore.main_GetTemplateLink(TemplateID)
            Else
                Return ""
            End If
        End Function

        Public Overrides Function IsField(ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Return cpCore.csv_IsContentFieldSupported(ContentName, FieldName)
        End Function

        Public Overrides Function IsLocked(ByVal ContentName As String, ByVal RecordID As String) As Boolean 'Inherits CPContentBaseClass.IsLocked
            Return cpCore.app.csv_IsRecordLocked(ContentName, RecordID, 0)
        End Function

        Public Overrides Function IsChildContent(ByVal ChildContentID As String, ByVal ParentContentID As String) As Boolean 'Inherits CPContentBaseClass.IsChildContent
            Return cpCore.app.metaData.isChildContent(ChildContentID, ParentContentID)
        End Function

        Public Overrides Function IsWorkflow(ByVal ContentName As String) As Boolean 'Inherits CPContentBaseClass.IsWorkflow
            ' ****************************************************
            ' this needs to be added to cmc
            ' ****************************************************
            Return False
        End Function

        Public Overrides Sub PublishEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Inherits CPContentBaseClass.PublishEdit
            Call cpCore.app.csv_PublishEdit(ContentName, RecordID, 0)
        End Sub

        Public Overrides Sub SubmitEdit(ByVal ContentName As String, ByVal RecordID As Integer) 'Inherits CPContentBaseClass.SubmitEdit
            Call cpCore.app.csv_PublishEdit(ContentName, RecordID, 0)
        End Sub

        Public Overrides Sub AbortEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Inherits CPContentBaseClass.AbortEdit
            Call cpCore.app.csv_AbortEdit(ContentName, RecordId, 0)
        End Sub

        Public Overrides Sub ApproveEdit(ByVal ContentName As String, ByVal RecordId As Integer) 'Inherits CPContentBaseClass.ApproveEdit
            Call cpCore.app.csv_ApproveEdit(ContentName, RecordId, 0)
        End Sub
        '
        '
        '
        Public Overrides Function getLayout(ByVal layoutName As String) As String
            Dim result As String = ""
            Try
                Dim cs As CPCSClass
                '
                cs = New CPCSClass(cp)
                cs.Open("layouts", "name=" & cp.Db.EncodeSQLText(layoutName))
                If cs.OK Then
                    result = cs.GetText("layout")
                End If
                Call cs.Close()
            Catch ex As Exception
                Call cp.core.handleException(ex, "Unexpected error in getLayout")
            End Try
            Return result
        End Function
        '
        '
        '
        Public Overrides Function AddRecord(ContentName As Object) As Integer
            Dim recordId As Integer = 0
            Try
                Dim cs As CPCSClass = cp.CSNew()
                If cs.Insert(ContentName) Then
                    recordId = cs.GetInteger("id")
                End If
                Call cs.Close()
            Catch ex As Exception
                Call cp.core.handleException(ex, "Unexpected error in AddRecord")
            End Try
            Return recordId
        End Function
        '
        '
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
                Call cp.core.handleException(ex, "Unexpected error in GetListLink")
            End Try
            Return returnHtml
        End Function
        'Public Overrides Function GetFormInput(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlId As String = "") As Object
        '    'Dim result As String = ""
        '    ''
        '    'Try
        '    ' If false Then
        '    ' Call cp.core.handleException("cs.GetFormInput does not support non-web calls.")
        '    ' Else
        '    ' result = cmc.main_GetFormInputCS(CSPointer, ContentName, FieldName, Height, Width, HtmlId)
        '    ' End If
        '    ' Catch ex As Exception
        '    ' Call cp.core.handleException(ex, "Unexpected error in cs.GetFormInput")
        '    ' End Try
        '    Return ""
        'End Function
        '
        ' append to logfile
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.content, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
        '
        '
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
