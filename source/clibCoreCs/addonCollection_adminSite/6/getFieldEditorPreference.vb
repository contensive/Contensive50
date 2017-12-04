
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.AdminSite
    '
    Public Class getFieldEditorPreference
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' getFieldEditorPreference remote method - cut-paste from legacy init()
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim result As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core

                '
                ' When editing in admin site, if a field has multiple editors (addons as editors), you main_Get an icon
                '   to click to select the editor. When clicked, a fancybox opens to display a form. The onStart of
                '   he fancybox calls this ajax call and puts the return in the div that is displayed. Return a list
                '   of addon editors compatible with the field type.
                '
                Dim addonDefaultEditorName As String = ""
                Dim addonDefaultEditorId As Integer = 0
                Dim fieldId As Integer = cpCore.docProperties.getInteger("fieldid")
                '
                ' main_Get name of default editor
                '
                Dim Sql As String = "select top 1" _
                                        & " a.name,a.id" _
                                        & " from ccfields f left join ccAggregateFunctions a on a.id=f.editorAddonId" _
                                        & " where" _
                                        & " f.ID = " & fieldId _
                                        & ""
                Dim dt As DataTable
                dt = cpCore.db.executeQuery(Sql)
                If dt.Rows.Count > 0 Then
                    For Each rsDr As DataRow In dt.Rows
                        addonDefaultEditorName = "&nbsp;(" & genericController.encodeText(rsDr("name")) & ")"
                        addonDefaultEditorId = genericController.EncodeInteger(rsDr("id"))
                    Next
                End If
                '
                Dim radioGroupName As String = "setEditorPreference" & fieldId
                Dim currentEditorAddonId As Integer = cpCore.docProperties.getInteger("currentEditorAddonId")
                Dim submitFormId As Integer = cpCore.docProperties.getInteger("submitFormId")
                Sql = "select f.name,c.name,r.addonid,a.name as addonName" _
                                        & " from (((cccontent c" _
                                        & " left join ccfields f on f.contentid=c.id)" _
                                        & " left join ccAddonContentFieldTypeRules r on r.contentFieldTypeID=f.type)" _
                                        & " left join ccAggregateFunctions a on a.id=r.AddonId)" _
                                        & " where f.id=" & fieldId

                dt = cpCore.db.executeQuery(Sql)
                If dt.Rows.Count > 0 Then
                    For Each rsDr As DataRow In dt.Rows
                        Dim addonId As Integer = genericController.EncodeInteger(rsDr("addonid"))
                        If (addonId <> 0) And (addonId <> addonDefaultEditorId) Then
                            result = result _
                                                    & vbCrLf & vbTab & "<div class=""radioCon"">" & cpCore.html.html_GetFormInputRadioBox(radioGroupName, genericController.encodeText(addonId), CStr(currentEditorAddonId)) & "&nbsp;Use " & genericController.encodeText(rsDr("addonName")) & "</div>" _
                                                    & ""
                        End If

                    Next
                End If

                Dim OnClick As String = "" _
                                        & "var a=document.getElementsByName('" & radioGroupName & "');" _
                                        & "for(i=0;i<a.length;i++) {" _
                                        & "if(a[i].checked){var v=a[i].value}" _
                                        & "}" _
                                        & "document.getElementById('fieldEditorPreference').value='" & fieldId & ":'+v;" _
                                        & "cj.admin.saveEmptyFieldList('" & "FormEmptyFieldList');" _
                                        & "document.getElementById('adminEditForm').submit();" _
                                        & ""

                result = "" _
                                        & vbCrLf & vbTab & "<h1>Editor Preference</h1>" _
                                        & vbCrLf & vbTab & "<p>Select the editor you will use for this field. Select default if you want to use the current system default.</p>" _
                                        & vbCrLf & vbTab & "<div class=""radioCon"">" & cpCore.html.html_GetFormInputRadioBox("setEditorPreference" & fieldId, "0", "0") & "&nbsp;Use Default Editor" & addonDefaultEditorName & "</div>" _
                                        & vbCrLf & vbTab & result _
                                        & vbCrLf & vbTab & "<div class=""buttonCon"">" _
                                        & vbCrLf & vbTab & "<button type=""button"" onclick=""" & OnClick & """>Select</button>" _
                                        & vbCrLf & vbTab & "</div>" _
                                        & ""


            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
