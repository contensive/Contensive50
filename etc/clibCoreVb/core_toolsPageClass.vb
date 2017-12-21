

Imports Contensive.Core.ccCommonModule
Imports Contensive.Processor
'
Namespace Contensive.Core
    Public Class toolsPageClass
        Private cp As CPClass
        Private Structure ToolType
            Dim Header As String
            Dim Name As String
            Dim Action As String
        End Structure
        Private Tools() As ToolType
        Private ToolCnt As Integer
        '
        Private Structure EditRowType
            Dim Caption As String
            Dim Editor As String
            Dim Description As String
        End Structure
        Private EditRows() As EditRowType
        Private EditRowCnt As Integer
        '
        Private Body As String
        '
        '
        '
        Public Sub AddButton(ByVal ButtonLabel As String)
            Call Page.AddElement("buttons", cp.core.main_GetFormButton(ButtonLabel))
        End Sub
        '
        '
        '
        Public Sub AddTool(ByVal Header As String, ByVal Name As String, ByVal Action As String)
            ReDim Preserve Tools(ToolCnt)
            Tools(ToolCnt).Action = Action
            Tools(ToolCnt).Name = Name
            Tools(ToolCnt).Header = Header
            ToolCnt = ToolCnt + 1
        End Sub
        '
        '
        '
        Public Sub AddEditRow(ByVal Caption As String, ByVal Editor As String, ByVal Description As String)
            ReDim Preserve EditRows(EditRowCnt)
            EditRows(EditRowCnt).Caption = Caption
            EditRows(EditRowCnt).Editor = Editor
            EditRows(EditRowCnt).Description = Description
            EditRowCnt = EditRowCnt + 1
        End Sub
        '
        '
        '
        Public Sub AddHeader(ByVal HeaderTitle As String, ByVal HeaderDescription As String)
            '
            Dim Copy As String
            Dim TitleBar As String
            Dim ElementBody As String
            '
            ' TitleBar part
            '
            ElementBody = HeaderTitle
            '
            ' Errors
            '
            If cp.core.main_IsUserError Then
                ElementBody = ElementBody & "<Div>" & cp.core.main_GetUserError & "</Div>"
            End If
            '
            If HeaderDescription <> "" Then
                ElementBody = ElementBody & "<div>&nbsp;</div><div class=""ccAdminInfoBar ccPanel3DReverse"">" & HeaderDescription & "</div>"
            End If
            '
            Call Page.AddElement("PageHeader", ElementBody)
        End Sub
        '
        '
        '
        Public Sub AddElement(ByVal ElementName As String, ByVal ElementBody As String)
            If LCase(ElementName) = "body" Then
                Body = Body & ElementBody
            Else
                Call Page.AddElement(ElementName, ElementBody)
            End If
        End Sub
        '
        '
        '
        Public Function GetPage() As String
            '
            Dim EditTable As String
            Dim Ptr As Integer
            Dim PtrInner As Integer
            Dim Copy As String
            Dim Header As String
            Dim LcaseHeader As String
            Dim TestHeader As String
            Dim Filename As String
            Dim Layout As String
            '
            If ToolCnt > 0 Then
                For Ptr = 0 To ToolCnt - 1
                    With Tools(Ptr)
                        '
                        ' Add the header and the tool
                        '
                        Header = Trim(.Header)
                        LcaseHeader = LCase(Header)
                        If Header <> "" Then
                            Copy = Copy & vbCrLf & vbTab & "<div class=""ToolHeader"">" & Header & "</div>"
                            Copy = Copy & vbCrLf & vbTab & vbTab & "<div class=""ToolActionRow""><a class=""ToolAction"" href=""" & .Action & """>" & .Name & "</a></div>"
                            '
                            ' Find any other tools in this header
                            '
                            If Ptr < (ToolCnt - 1) Then
                                For PtrInner = Ptr + 1 To ToolCnt - 1
                                    TestHeader = Trim(LCase(Tools(PtrInner).Header))
                                    If TestHeader <> "" Then
                                        If LcaseHeader = TestHeader Then
                                            With Tools(PtrInner)
                                                Copy = Copy & vbCrLf & vbTab & vbTab & "<div class=""ToolActionRow""><a href=""" & .Action & """>" & .Name & "</a></div>"
                                                .Header = ""
                                            End With
                                        End If
                                    End If
                                Next
                            End If
                        End If
                    End With
                Next
                Call Page.AddElement("Tools", Copy)
            End If
            '
            ' Edit Rows
            '
            If EditRowCnt > 0 Then
                For Ptr = 0 To EditRowCnt - 1
                    With EditRows(Ptr)
                        '
                        ' Add the header and the EditRow
                        '
                        If .Caption <> "" And .Editor <> "" Then
                            EditTable = EditTable & "<tr><td class=ccAdminEditCaption><nobr>" & .Caption & "<img alt=""-"" src=""/ccLib/images/spacer.gif"" width=1 height=15></nobr></td>"
                            If .Description <> "" Then
                                .Editor = .Editor & "<div style=""padding:10px;"">" & .Description & "</div>"
                            End If
                            EditTable = EditTable & "<td class=ccAdminEditField>" & .Editor & "</td></tr>"
                        End If
                    End With
                Next
            End If
            If EditTable <> "" Then
                Body = Body _
                    & "<table border=0 cellpadding=3 cellspacing=0 width=""100%"">" _
                    & EditTable _
                    & "</table>"
                Body = Body
            End If
            Call Page.AddElement("Body", Body)

            Filename = getProgramFilesPath() & "\cclib\templates\AdminToolLayout.htm"
            Layout = cp.core.main_ReadFile(Filename)
            GetPage = Page.GetPage(Layout)
        End Function



        Public Sub New(cp As CPClass)
            MyBase.New()
            Me.cp = cp
        End Sub
    End Class
End Namespace
