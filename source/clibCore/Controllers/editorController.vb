
Option Explicit On
Option Strict On
'
Imports System.Text.RegularExpressions
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' static class controller
    ''' </summary>
    Public Class editorController
        Implements IDisposable
        '
        '
        '========================================================================
        ' ----- Process the active editor form
        '========================================================================
        '
        Public Shared Sub processActiveEditor(cpCore As coreClass)
            '
            Dim CS As Integer
            Dim Button As String
            Dim ContentID As Integer
            Dim ContentName As String
            Dim RecordID As Integer
            Dim FieldName As String
            Dim ContentCopy As String
            '
            Button = cpCore.docProperties.getText("Button")
            Select Case Button
                Case ButtonCancel
                    '
                    ' ----- Do nothing, the form will reload with the previous contents
                    '
                Case ButtonSave
                    '
                    ' ----- read the form fields
                    '
                    ContentID = cpCore.docProperties.getInteger("cid")
                    RecordID = cpCore.docProperties.getInteger("id")
                    FieldName = cpCore.docProperties.getText("fn")
                    ContentCopy = cpCore.docProperties.getText("ContentCopy")
                    '
                    ' ----- convert editor active edit icons
                    '
                    ContentCopy = cpCore.html.decodeContent(ContentCopy)
                    '
                    ' ----- save the content
                    '
                    ContentName = cpCore.metaData.getContentNameByID(ContentID)
                    If ContentName <> "" Then
                        CS = cpCore.db.cs_open(ContentName, "ID=" & cpCore.db.encodeSQLNumber(RecordID), , False)
                        If cpCore.db.cs_ok(CS) Then
                            Call cpCore.db.cs_set(CS, FieldName, ContentCopy)
                        End If
                        Call cpCore.db.cs_Close(CS)
                    End If
            End Select
        End Sub
        '
        '========================================================================
        ' Print the active editor form
        '========================================================================
        '
        Public Shared Function main_GetActiveEditor(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String, Optional ByVal FormElements As String = "") As String
            '
            Dim ContentID As Integer
            Dim CSPointer As Integer
            Dim Copy As String
            Dim Stream As String = ""
            Dim ButtonPanel As String
            Dim EditorPanel As String
            Dim PanelCopy As String
            '
            Dim intContentName As String
            Dim intRecordId As Integer
            Dim strFieldName As String
            '
            intContentName = genericController.encodeText(ContentName)
            intRecordId = genericController.EncodeInteger(RecordID)
            strFieldName = genericController.encodeText(FieldName)
            '
            EditorPanel = ""
            ContentID = cpcore.metaData.getContentId(intContentName)
            If (ContentID < 1) Or (intRecordId < 1) Or (strFieldName = "") Then
                PanelCopy = SpanClassAdminNormal & "The information you have selected can not be accessed.</span>"
                EditorPanel = EditorPanel & cpcore.html.main_GetPanel(PanelCopy)
            Else
                intContentName = cpcore.metaData.getContentNameByID(ContentID)
                If intContentName <> "" Then
                    CSPointer = cpcore.db.cs_open(intContentName, "ID=" & intRecordId)
                    If Not cpcore.db.cs_ok(CSPointer) Then
                        PanelCopy = SpanClassAdminNormal & "The information you have selected can not be accessed.</span>"
                        EditorPanel = EditorPanel & cpcore.html.main_GetPanel(PanelCopy)
                    Else
                        Copy = cpcore.db.cs_get(CSPointer, strFieldName)
                        EditorPanel = EditorPanel & cpcore.html.html_GetFormInputHidden("Type", FormTypeActiveEditor)
                        EditorPanel = EditorPanel & cpcore.html.html_GetFormInputHidden("cid", ContentID)
                        EditorPanel = EditorPanel & cpcore.html.html_GetFormInputHidden("ID", intRecordId)
                        EditorPanel = EditorPanel & cpcore.html.html_GetFormInputHidden("fn", strFieldName)
                        EditorPanel = EditorPanel & genericController.encodeText(FormElements)
                        EditorPanel = EditorPanel & cpcore.html.html_GetFormInputHTML3("ContentCopy", Copy, "3", "45", False, True)
                        'EditorPanel = EditorPanel & main_GetFormInputActiveContent( "ContentCopy", Copy, 3, 45)
                        ButtonPanel = cpcore.html.main_GetPanelButtons(ButtonCancel & "," & ButtonSave, "button")
                        EditorPanel = EditorPanel & ButtonPanel
                    End If
                    cpcore.db.cs_Close(CSPointer)
                End If
            End If
            Stream = Stream & cpcore.html.main_GetPanelHeader("Contensive Active Content Editor")
            Stream = Stream & cpcore.html.main_GetPanel(EditorPanel)
            Stream = cpcore.html.html_GetFormStart() & Stream & cpcore.html.html_GetFormEnd()
            Return Stream
        End Function
        '
        '========================================================================
        ' main_Get FieldEditorList
        '
        '   FieldEditorList is a comma delmited list of addonids, one for each fieldtype
        '   to use it, split the list on comma and use the fieldtype as index
        '========================================================================
        '
        Public Shared Function getFieldTypeDefaultEditorAddonIdList(cpCore As coreClass) As String
            Dim result As String = ""
            Try
                Dim editorAddonIds() As String
                Dim RS As DataTable
                Dim fieldTypeID As Integer
                '
                ' load default editors into editors() - these are the editors used when there is no editorPreference
                '   editors(fieldtypeid) = addonid
                ReDim editorAddonIds(FieldTypeIdMax)
                Dim SQL As String = "" _
                    & " select" _
                    & " t.id as contentfieldtypeid" _
                    & " ,t.editorAddonId" _
                    & " from ccFieldTypes t" _
                    & " left join ccaggregatefunctions a on a.id=t.editorAddonId" _
                    & " where (t.active<>0)and(a.active<>0) order by t.id"
                RS = cpCore.db.executeSql(SQL)
                For Each dr As DataRow In RS.Rows
                    fieldTypeID = genericController.EncodeInteger(dr("contentfieldtypeid"))
                    If (fieldTypeID <= FieldTypeIdMax) Then
                        editorAddonIds(fieldTypeID) = genericController.encodeText(dr("editorAddonId"))
                    End If
                Next
                '
                ' -- set any editors not specifically requested in fieldtype
                SQL = "select contentfieldtypeid, max(addonId) as editorAddonId from ccAddonContentFieldTypeRules group by contentfieldtypeid"
                RS = cpCore.db.executeSql(SQL)
                For Each dr As DataRow In RS.Rows
                    fieldTypeID = genericController.EncodeInteger(dr("contentfieldtypeid"))
                    If (fieldTypeID <= FieldTypeIdMax) Then
                        If (editorAddonIds(fieldTypeID) = "") Then
                            editorAddonIds(fieldTypeID) = genericController.encodeText(dr("editorAddonId"))
                        End If

                    End If
                Next
                result = Join(editorAddonIds, ",")
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    'If (cacheClient IsNot Nothing) Then
                    '    cacheClient.Dispose()
                    'End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
End Namespace