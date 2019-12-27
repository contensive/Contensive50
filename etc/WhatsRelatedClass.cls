VERSION 1.0 CLASS
BEGIN
  MultiUse = -1  'True
  Persistable = 0  'NotPersistable
  DataBindingBehavior = 0  'vbNone
  DataSourceBehavior  = 0  'vbNone
  MTSTransactionMode  = 0  'NotAnMTSObject
END
Attribute VB_Name = "WhatsRelatedClass"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = True
Attribute VB_PredeclaredId = False
Attribute VB_Exposed = True

Option Explicit
'
Private csv As Object
Private main As Object
'
'=================================================================================
'   Execute Method, v3.4 Interface
'=================================================================================
'
Public Function execute(csvObject As Object, mainObject As Object, optionString As String, filterInput As String) As String
    On Error GoTo errorTrap
    '
    Dim sortFieldList As String
    '
    Set main = mainObject
    Set csv = csvObject
    '
    sortFieldList = main.GetAddonOption("SortFieldList", optionString)
    '
    If main.SiteProperty_BuildVersion < "4.1.290" Then
        '
        ' old method with old topic rules
        '
        execute = main.getContentPageWhatsRelated(sortFieldList)
    Else
        '
        ' use new simple "Page Content Topic Rules"
        '
        execute = getWhatsRelated(sortFieldList)
    End If
    '
    Exit Function
errorTrap:
    Call reportError("Execute", "trap")
End Function
'
'========================================================================
'   Get the Whats Related List
'========================================================================
'
Private Function getWhatsRelated(sortFieldList As String) As String
    On Error GoTo errorTrap
    '
    Dim CS As Long
    Dim sql As String
    Dim link As String
    Dim caption As String
    Dim items As String
    '
    sql = "select rp.id,rp.name,rp.menuheadline" _
        & " from ccpagecontenttopicrules sr" _
        & " left join ccpagecontenttopicrules rr on rr.topicid=sr.topicid" _
        & " left join ccpagecontent rp on rp.id=rr.pageid" _
        & " group by rp.id,rp.name,rp.menuheadline" _
        & " order by count(id)"
    CS = main.OpenCSContent("page content", "id in (" & sql & ")")
    items = ""
    Do While main.IsCSOK(CS)
        link = main.GetPageLink(main.GetCSInteger(CS, "id"))
        If link <> "" Then
            caption = main.GetCSText(CS, "menuheadline")
            If caption = "" Then
                caption = main.GetCSText(CS, "name")
            End If
            items = items & vbCrLf & vbTab & "<li class=""wrItem""><a href=""" & link & """>" & caption & "</a></li>"
        End If
        Call main.NextCSRecord(CS)
    Loop
    If items <> "" Then
        items = "" _
            & vbCrLf & vbTab & "<ul class=""wrList"">" _
            & kmaIndent(items) _
            & vbCrLf & vbTab & "</ul>" _
            & ""
        items = "" _
            & vbCrLf & vbTab & "<div class=""whatsRelated"">" _
            & kmaIndent(items) _
            & vbCrLf & vbTab & "</div>" _
            & ""
    End If
    '
    Exit Function
    '
    ' ----- Error Trap
    '
errorTrap:
    Call reportError("getWhatsRelated", "trap")
End Function
'
'==================================================================================================
'   report error and resume
'==================================================================================================
'
Private Sub reportErrorAndResume(method As String, cause As String)
    '
    Call main.ReportError3(App.EXEName, "WhatsRelatedClass", method, cause, err.Number, err.Source, err.Description, True)
    '
End Sub
'
'==================================================================================================
'   report error
'==================================================================================================
'
Private Sub reportError(method As String, cause As String)
    '
    Call main.ReportError3(App.EXEName, "WhatsRelatedClass", method, cause, err.Number, err.Source, err.Description, False)
    '
End Sub

