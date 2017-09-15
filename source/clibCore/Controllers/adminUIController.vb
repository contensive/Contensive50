
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' UI rendering for Admin
    ''' REFACTOR - add  try-catch
    ''' not IDisposable - not contained classes that need to be disposed
    ''' </summary>
    Public Class adminUIController
        '
        '========================================================================
        '
        Private Enum SortingStateEnum
            NotSortable = 0
            SortableSetAZ = 1
            SortableSetza = 2
            SortableNotSet = 3
        End Enum
        '
        Private cpCore As coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '===========================================================================
        '
        Public Function GetTitleBar(ByVal Title As String, ByVal Description As String) As String
            On Error GoTo ErrorTrap
            '
            Dim Copy As String
            '
            GetTitleBar = "<div class=""ccAdminTitleBar"">" & Title
            'GetTitleBar = "<div class=""ccAdminTitleBar"">" & Title & "</div>"
            Copy = Description
            If genericController.vbInstr(1, Copy, "<p>", vbTextCompare) = 1 Then
                Copy = Mid(Copy, 4)
                If InStrRev(Copy, "</p>", , vbTextCompare) = (Len(Copy) - 4) Then
                    Copy = Mid(Copy, 1, Len(Copy) - 4)
                End If
            End If
            '
            ' Add Errors
            '
            If (cpcore.debug_iUserError <> "") Then
                Copy = Copy & "<div>" & errorController.error_GetUserError(cpCore) & "</div>"
            End If
            '
            If Copy <> "" Then
                GetTitleBar = GetTitleBar & "<div>&nbsp;</div><div class=""ccAdminInfoBar ccPanel3DReverse"">" & Copy & "</div>"
            End If
            GetTitleBar = GetTitleBar & "</div>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError("GetTitleBar")
        End Function
        '
        '========================================================================
        ' Get the Normal Edit Button Bar String
        '
        '   used on Normal Edit and others
        '========================================================================
        '
        Public Function GetEditButtonBar2(ByVal MenuDepth As Integer, ByVal AllowDelete As Boolean, ByVal AllowCancel As Boolean, ByVal allowSave As Boolean, ByVal AllowSpellCheck As Boolean, ByVal AllowPublish As Boolean, ByVal AllowAbort As Boolean, ByVal AllowSubmit As Boolean, ByVal AllowApprove As Boolean, ByVal AllowAdd As Boolean, ByVal ignore_AllowReloadCDef As Boolean, ByVal HasChildRecords As Boolean, ByVal IsPageContent As Boolean, ByVal AllowMarkReviewed As Boolean, ByVal AllowRefresh As Boolean, ByVal AllowCreateDuplicate As Boolean) As String
            On Error GoTo ErrorTrap
            '
            Dim JSOnClick As String
            '
            GetEditButtonBar2 = ""
            '
            If AllowCancel Then
                If MenuDepth = 1 Then
                    '
                    ' Close if this is the root depth of a popup window
                    '
                    GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonClose & """ OnClick=""window.close();"">"
                Else
                    '
                    ' Cancel
                    '
                    GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonCancel & """ onClick=""return processSubmit(this);"">"
                End If
            End If
            If allowSave Then
                '
                ' Save
                '
                GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonSave & """ onClick=""return processSubmit(this);"">"
                '
                ' OK
                '
                GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonOK & """ onClick=""return processSubmit(this);"">"
                If AllowAdd Then
                    '
                    ' OK
                    '
                    GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonSaveAddNew & """ onClick=""return processSubmit(this);"">"
                End If
            End If
            If AllowDelete Then
                '
                ' Delete
                '
                If IsPageContent Then
                    JSOnClick = "if(!DeletePageCheck())return false;"
                ElseIf HasChildRecords Then
                    JSOnClick = "if(!DeleteCheckWithChildren())return false;"
                Else
                    JSOnClick = "if(!DeleteCheck())return false;"
                End If
                GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonDelete & """ onClick=""" & JSOnClick & """>"
            Else
                GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=SUBMIT NAME=BUTTON disabled=""disabled"" VALUE=""" & ButtonDelete & """>"
            End If
            '    If AllowSpellCheck Then
            '        '
            '        ' Spell Check
            '        '
            '        GetEditButtonBar2 = GetEditButtonBar2 & "<input TYPE=""SUBMIT"" NAME=""BUTTON"" VALUE=""" & ButtonSpellCheck & """ onClick=""return processSubmit(this);"">"
            '    End If
            If AllowPublish Then
                '
                ' Publish
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublish, RequestNameButton)
            End If
            If AllowAbort Then
                '
                ' Abort
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonAbortEdit, RequestNameButton)
            End If
            If AllowSubmit Then
                '
                ' Submit for Publishing
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishSubmit, RequestNameButton)
            End If
            If AllowApprove Then
                '
                ' Approve Publishing
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonPublishApprove, RequestNameButton)
            End If
            If ignore_AllowReloadCDef Then
                '
                ' Reload Content Definitions
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonSaveandInvalidateCache, RequestNameButton)
            End If
            If AllowMarkReviewed Then
                '
                ' Reload Content Definitions
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonMarkReviewed, RequestNameButton)
            End If
            If AllowRefresh Then
                '
                ' just like a save, but don't save jsut redraw
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonRefresh, RequestNameButton)
            End If
            If AllowCreateDuplicate Then
                '
                ' just like a save, but don't save jsut redraw
                '
                GetEditButtonBar2 = GetEditButtonBar2 & cpCore.html.html_GetFormButton(ButtonCreateDuplicate, RequestNameButton, , "return processSubmit(this)")
            End If
            '
            GetEditButtonBar2 = "" _
                & vbCrLf & vbTab & GetHTMLComment("ButtonBar") _
                & vbCrLf & vbTab & "<div class=""ccButtonCon"">" _
                & htmlIndent(GetEditButtonBar2) _
                & vbCrLf & vbTab & "</div><!-- ButtonBar End -->" _
                & ""
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError("GetEditButtonBar2")
            '
        End Function
        '
        '========================================================================
        ' Return a panel header with the header message reversed out of the left
        '========================================================================
        '
        Public Function GetHeader(ByVal HeaderMessage As String, Optional ByVal RightSideMessage As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim s As String
            '
            If RightSideMessage = "" Then
                RightSideMessage = FormatDateTime(cpCore.profileStartTime)
            End If
            '
            If isInStr(1, HeaderMessage & RightSideMessage, vbCrLf) Then
                s = "" _
                    & cr & "<td width=""50%"" valign=Middle class=""cchLeft"">" _
                    & htmlIndent(HeaderMessage) _
                    & cr & "</td>" _
                    & cr & "<td width=""50%"" valign=Middle class=""cchRight"">" _
                    & htmlIndent(RightSideMessage) _
                    & cr & "</td>"
            Else
                s = "" _
                    & cr & "<td width=""50%"" valign=Middle class=""cchLeft"">" & HeaderMessage & "</td>" _
                    & cr & "<td width=""50%"" valign=Middle class=""cchRight"">" & RightSideMessage & "</td>"
            End If
            s = "" _
                & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr>" _
                & htmlIndent(s) _
                & cr & "</tr></table>" _
                & ""
            s = "" _
                & cr & "<div class=""ccHeaderCon"">" _
                & htmlIndent(s) _
                & cr & "</div>" _
                & ""
            GetHeader = s
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetHeader")
        End Function
        '
        '
        '
        Public Function GetButtonsFromList(ByVal ButtonList As String, ByVal AllowDelete As Boolean, ByVal AllowAdd As Boolean, ByVal ButtonName As String) As String
            On Error GoTo ErrorTrap
            '
            Dim s As String = String.Empty
            Dim Buttons() As String
            Dim Ptr As Integer
            '
            If Trim(ButtonList) <> "" Then
                Buttons = Split(ButtonList, ",")
                For Ptr = 0 To UBound(Buttons)
                    Select Case Trim(Buttons(Ptr))
                        Case Trim(ButtonDelete)
                            If AllowDelete Then
                                s = s & "<input TYPE=SUBMIT NAME=""" & ButtonName & """ VALUE=""" & Buttons(Ptr) & """ onClick=""if(!DeleteCheck())return false;"">"
                            Else
                                s = s & "<input TYPE=SUBMIT NAME=""" & ButtonName & """ DISABLED VALUE=""" & Buttons(Ptr) & """>"
                            End If
                        Case Trim(ButtonClose)
                            s = s & cpCore.html.html_GetFormButton(Buttons(Ptr), , , "window.close();")
                        Case Trim(ButtonAdd)
                            If AllowAdd Then
                                s = s & "<input TYPE=SUBMIT NAME=""" & ButtonName & """ VALUE=""" & Buttons(Ptr) & """ onClick=""return processSubmit(this);"">"
                            Else
                                s = s & "<input TYPE=SUBMIT NAME=""" & ButtonName & """ DISABLED VALUE=""" & Buttons(Ptr) & """ onClick=""return processSubmit(this);"">"
                            End If
                        Case ""
                        Case Else
                            s = s & cpCore.html.html_GetFormButton(Buttons(Ptr), ButtonName)
                    End Select
                Next
            End If

            GetButtonsFromList = s

            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetButtonsFromList")
        End Function
        '
        '
        '
        Public Function GetButtonBar(ByVal LeftButtons As String, ByVal RightButtons As String) As String
            On Error GoTo ErrorTrap
            '
            If LeftButtons & RightButtons = "" Then
                '
                ' nothing there
                '
            ElseIf RightButtons = "" Then
                GetButtonBar = "" _
                    & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td align=left  class=""ccButtonCon"">" & LeftButtons & "</td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>"
            Else
                GetButtonBar = "" _
                    & cr & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td align=left  class=""ccButtonCon"">" _
                    & cr4 & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                    & cr5 & "<tr>" _
                    & cr6 & "<td width=""50%"" align=left>" & LeftButtons & "</td>" _
                    & cr6 & "<td width=""50%"" align=right>" & RightButtons & "</td>" _
                    & cr5 & "</tr>" _
                    & cr4 & "</table>" _
                    & cr3 & "</td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>"
            End If
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetButtonBar")
        End Function
        '
        '
        '
        Public Function GetButtonBarForIndex(ByVal LeftButtons As String, ByVal RightButtons As String, ByVal PageNumber As Integer, ByVal RecordsPerPage As Integer, ByVal PageCount As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim Ptr As Integer
            Dim Nav As String = String.Empty
            Dim NavStart As Integer
            Dim NavEnd As Integer
            '
            GetButtonBarForIndex = GetButtonBar(LeftButtons, RightButtons)
            NavStart = PageNumber - 9
            If NavStart < 1 Then
                NavStart = 1
            End If
            NavEnd = NavStart + 20
            If NavEnd > PageCount Then
                NavEnd = PageCount
                NavStart = NavEnd - 20
                If NavStart < 1 Then
                    NavStart = 1
                End If
            End If
            If NavStart > 1 Then
                Nav = Nav & "<li onclick=""bbj(this);"">1</li><li class=""delim"">&#171;</li>"
            End If
            For Ptr = NavStart To NavEnd
                Nav = Nav & "<li onclick=""bbj(this);"">" & Ptr & "</li>"
            Next
            If NavEnd < PageCount Then
                Nav = Nav & "<li class=""delim"">&#187;</li><li onclick=""bbj(this);"">" & PageCount & "</li>"
            End If
            Nav = genericController.vbReplace(Nav, ">" & PageNumber & "<", " class=""hit"">" & PageNumber & "<")
            GetButtonBarForIndex = GetButtonBarForIndex _
                & cr & "<script language=""javascript"">function bbj(p){document.getElementsByName('indexGoToPage')[0].value=p.innerHTML;document.adminForm.submit();}</script>" _
                & cr & "<div class=""ccJumpCon"">" _
                & cr2 & "<ul><li class=""caption"">Page</li>" _
                & cr3 & Nav _
                & cr2 & "</ul>" _
                & cr & "</div>"
            '    GetButtonBarForIndex = GetButtonBarForIndex _
            '        & CR & "<script language=""javascript"">function bbj(p){document.getElementsByName('indexGoToPage')[0].value=p.innerHTML;document.adminForm.submit();}</script>" _
            '        & CR & "<div class=""ccJumpCon"">" _
            '        & cr2 & "<ul>Page&nbsp;" _
            '        & cr3 & Nav _
            '        & cr2 & "</ul>" _
            '        & CR & "</div>"
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetButtonBar")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function GetBody(ByVal Caption As String, ByVal ButtonListLeft As String, ByVal ButtonListRight As String, ByVal AllowAdd As Boolean, ByVal AllowDelete As Boolean, ByVal Description As String, ByVal ContentSummary As String, ByVal ContentPadding As Integer, ByVal Content As String) As String
            Dim result As String = String.Empty
            Try
                Dim ContentCell As String = String.Empty
                Dim Stream As New stringBuilderLegacyController
                Dim ButtonBar As String
                Dim LeftButtons As String = String.Empty
                Dim RightButtons As String = String.Empty
                Dim CellContentSummary As String = String.Empty
                '
                ' Build ButtonBar
                '
                If Trim(ButtonListLeft) <> "" Then
                    LeftButtons = GetButtonsFromList(ButtonListLeft, AllowDelete, AllowAdd, "Button")
                End If
                If Trim(ButtonListRight) <> "" Then
                    RightButtons = GetButtonsFromList(ButtonListRight, AllowDelete, AllowAdd, "Button")
                End If
                ButtonBar = GetButtonBar(LeftButtons, RightButtons)
                If ContentSummary <> "" Then
                    CellContentSummary = "" _
                    & cr & "<div class=""ccPanelBackground"" style=""padding:10px;"">" _
                    & htmlIndent(cpCore.html.main_GetPanel(ContentSummary, "ccPanel", "ccPanelShadow", "ccPanelHilite", "100%", 5)) _
                    & cr & "</div>"
                End If
                '
                ContentCell = "" _
                & cr & "<div style=""padding:" & ContentPadding & "px;"">" _
                & htmlIndent(Content) _
                & cr & "</div>"
                result = result _
                & htmlIndent(ButtonBar) _
                & htmlIndent(GetTitleBar(Caption, Description)) _
                & htmlIndent(CellContentSummary) _
                & htmlIndent(ContentCell) _
                & htmlIndent(ButtonBar) _
                & ""

                result = "" _
                & cr & cpCore.html.html_GetUploadFormStart() _
                & htmlIndent(result) _
                & cr & cpCore.html.html_GetUploadFormEnd
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Public Function GetEditRow(ByVal HTMLFieldString As String, ByVal Caption As String, Optional ByVal HelpMessage As String = "", Optional ByVal FieldRequired As Boolean = False, Optional ByVal AllowActiveEdit As Boolean = False, Optional ByVal ignore0 As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim FastString As New stringBuilderLegacyController
            Dim Copy As String
            Dim FormInputName As String
            '
            ' Left Side
            '
            Copy = Caption
            If Copy = "" Then
                Copy = "&nbsp;"
            End If
            GetEditRow = "<tr><td class=""ccEditCaptionCon""><nobr>" & Copy & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=15 >"
            'GetEditRow = "<tr><td class=""ccAdminEditCaption""><nobr>" & Copy & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=15 >"
            'If cpCore.visitProperty.getboolean("AllowHelpIcon") Then
            '    'If HelpMessage <> "" Then
            '        GetEditRow = GetEditRow & "&nbsp;" & cpCore.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
            '    'Else
            '    '    GetEditRow = GetEditRow & "&nbsp;<img alt=""space"" src=""/ccLib/images/spacer.gif"" " & IconWidthHeight & ">"
            '    'End If
            'End If
            GetEditRow = GetEditRow & "</nobr></td>"
            '
            ' Right Side
            '
            Copy = HTMLFieldString
            If Copy = "" Then
                Copy = "&nbsp;"
            End If
            Copy = "<div class=""ccEditorCon"">" & Copy & "</div>"
            'If HelpMessage <> "" Then
            Copy = Copy & "<div class=""ccEditorHelpCon""><div class=""closed"">" & HelpMessage & "</div></div>"
            'Copy = Copy & "<div style=""padding:10px;white-space:normal"">" & HelpMessage & "</div>"
            'End If
            GetEditRow = GetEditRow & "<td class=""ccEditFieldCon"">" & Copy & "</td></tr>"
            'GetEditRow = GetEditRow & "<td class=""ccAdminEditField"">" & Copy & "</td></tr>"
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetEditRow")
        End Function
        '
        '
        '
        Public Function GetEditRowWithHelpEdit(ByVal HTMLFieldString As String, ByVal Caption As String, Optional ByVal HelpMessage As String = "", Optional ByVal FieldRequired As Boolean = False, Optional ByVal AllowActiveEdit As Boolean = False, Optional ByVal ignore0 As String = "") As String
            On Error GoTo ErrorTrap
            '
            Dim FastString As New stringBuilderLegacyController
            Dim Copy As String
            Dim FormInputName As String
            '
            Copy = Caption
            If Copy = "" Then
                Copy = "&nbsp;"
            End If
            GetEditRowWithHelpEdit = "<tr><td class=""ccAdminEditCaption""><nobr>" & Copy
            If cpCore.visitProperty.getBoolean("AllowHelpIcon") Then
                'If HelpMessage <> "" Then
                'GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "&nbsp;" & cpCore.main_GetHelpLinkEditable(0, Caption, HelpMessage, FormInputName)
                'Else
                '    GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "&nbsp;<img alt=""space"" src=""/ccLib/images/spacer.gif"" " & IconWidthHeight & ">"
                'End If
            End If
            GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=15 ></nobr></td>"
            Copy = HTMLFieldString
            If Copy = "" Then
                Copy = "&nbsp;"
            End If
            GetEditRowWithHelpEdit = GetEditRowWithHelpEdit & "<td class=""ccAdminEditField"">" & Copy & "</td></tr>"
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetEditRowWithHelpEdit")
        End Function
        '
        '
        '
        Public Function GetEditSubheadRow(ByVal Caption As String) As String
            On Error GoTo ErrorTrap
            '
            GetEditSubheadRow = "<tr><td colspan=2 class=""ccAdminEditSubHeader"">" & Caption & "</td></tr>"
            'GetEditSubheadRow = "<tr><td colspan=2 class=""ccPanel3D ccAdminEditSubHeader""><b>" & Caption & "</b></td></tr>"
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError("GetEditSubheadRow")
        End Function
        '
        '========================================================================
        ' GetEditPanel
        '
        '   An edit panel is a section of an admin page, under a subhead.
        '   When in tab mode, the subhead is blocked, and the panel is assumed to
        '   go in its own tab windows
        '========================================================================
        '
        Public Function GetEditPanel(ByVal AllowHeading As Boolean, ByVal PanelHeading As String, ByVal PanelDescription As String, ByVal PanelBody As String) As String
            Dim result As String = String.Empty
            Try
                Dim FastString As New stringBuilderLegacyController
                '
                result = result & "<div class=""ccPanel3DReverse ccAdminEditBody"">"
                '
                ' ----- Subhead
                '
                If AllowHeading And (PanelHeading <> "") Then
                    result = result & "<div class=""ccAdminEditHeading"">" & PanelHeading & "</div>"
                End If
                '
                ' ----- Description
                '
                If PanelDescription <> "" Then
                    result = result & "<div class=""ccAdminEditDescription"">" & PanelDescription & "</div>"
                End If
                '
                result = result & PanelBody & "</div>"
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Edit Table Open
        '========================================================================
        '
        Public ReadOnly Property EditTableOpen() As String
            Get
                EditTableOpen = "<table border=0 cellpadding=3 cellspacing=0 width=""100%"">"
            End Get
        End Property
        '
        '========================================================================
        ' Edit Table Close
        '========================================================================
        '
        Public ReadOnly Property EditTableClose() As String
            Get
                EditTableClose = "<tr>" _
                    & "<td width=20%><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=1 ></td>" _
                    & "<td width=80%><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=1 ></td>" _
                    & "</tr>" _
                    & "</table>"
            End Get

        End Property
        '
        '==========================================================================================
        '   Report Cell
        '==========================================================================================
        '
        Private Function GetReport_Cell(ByVal Copy As String, ByVal Align As String, ByVal Columns As Integer, ByVal RowPointer As Integer) As String
            Dim iAlign As String
            Dim Style As String
            Dim CellCopy As String
            '
            iAlign = encodeEmptyText(Align, "left")
            '
            If (RowPointer Mod 2) > 0 Then
                Style = "ccAdminListRowOdd"
            Else
                Style = "ccAdminListRowEven"
            End If
            '
            GetReport_Cell = vbCrLf & "<td valign=""middle"" align=""" & iAlign & """ class=""" & Style & """"
            If Columns > 1 Then
                GetReport_Cell = GetReport_Cell & " colspan=""" & Columns & """"
            End If
            '
            CellCopy = Copy
            If CellCopy = "" Then
                CellCopy = "&nbsp;"
            End If
            GetReport_Cell = GetReport_Cell & "><span class=""ccSmall"">" & CellCopy & "</span></td>"
        End Function
        '
        '==========================================================================================
        '   Report Cell Header
        '       ColumnPtr       :   0 based column number
        '       Title
        '       Width           :   if just a number, assumed to be px in style and an image is added
        '                       :   if 00px, image added with the numberic part
        '                       :   if not number, added to style as is
        '       align           :   style value
        '       ClassStyle      :   class
        '       RQS
        '       SortingState
        '==========================================================================================
        '
        Private Function GetReport_CellHeader(ByVal ColumnPtr As Integer, ByVal Title As String, ByVal Width As String, ByVal Align As String, ByVal ClassStyle As String, ByVal RefreshQueryString As String, ByVal SortingState As SortingStateEnum) As String
            '
            ' See new GetReportOrderBy for the method to setup sorting links
            '
            On Error GoTo ErrorTrap
            '
            Dim Style As String
            Dim Copy As String
            Dim QS As String
            Dim WidthTest As Integer
            Dim LinkTitle As String
            '
            If Title = "" Then
                Copy = "&nbsp;"
            Else
                Copy = genericController.vbReplace(Title, " ", "&nbsp;")
                'Copy = "<nobr>" & Title & "</nobr>"
            End If
            Style = "VERTICAL-ALIGN:bottom;"
            If Align = "" Then
            Else
                Style = Style & "TEXT-ALIGN:" & Align & ";"
            End If
            '
            Select Case SortingState
                Case SortingStateEnum.SortableNotSet
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", CStr(SortingStateEnum.SortableSetAZ), True)
                    QS = genericController.ModifyQueryString(QS, "ColPtr", CStr(ColumnPtr), True)
                    Copy = "<a href=""?" & QS & """ title=""Sort A-Z"" class=""ccAdminListCaption"">" & Copy & "</a>"
                Case SortingStateEnum.SortableSetza
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", CStr(SortingStateEnum.SortableSetAZ), True)
                    QS = genericController.ModifyQueryString(QS, "ColPtr", CStr(ColumnPtr), True)
                    Copy = "<a href=""?" & QS & """ title=""Sort A-Z"" class=""ccAdminListCaption"">" & Copy & "<img src=""/ccLib/images/arrowup.gif"" width=8 height=8 border=0></a>"
                Case SortingStateEnum.SortableSetAZ
                    QS = genericController.ModifyQueryString(RefreshQueryString, "ColSort", CStr(SortingStateEnum.SortableSetza), True)
                    QS = genericController.ModifyQueryString(QS, "ColPtr", CStr(ColumnPtr), True)
                    Copy = "<a href=""?" & QS & """ title=""Sort Z-A"" class=""ccAdminListCaption"">" & Copy & "<img src=""/ccLib/images/arrowdown.gif"" width=8 height=8 border=0></a>"
            End Select
            '
            If Width <> "" Then
                WidthTest = genericController.EncodeInteger(Replace(Width, "px", "", vbTextCompare))
                If WidthTest <> 0 Then
                    Style = Style & "width:" & WidthTest & "px;"
                    Copy = Copy & "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & WidthTest & """ height=1 border=0>"
                    'Copy = Copy & "<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & WidthTest & """ height=1 border=0>"
                Else
                    Style = Style & "width:" & Width & ";"
                End If
            End If
            '
            GetReport_CellHeader = vbCrLf & "<td style=""" & Style & """ class=""" & ClassStyle & """>" & Copy & "</td>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError("GetReport_CellHeader")
        End Function
        '
        '=============================================================================
        '   Get Sort Column Ptr
        '
        '   returns the integer column ptr of the column last selected
        '=============================================================================
        '
        Public Function GetReportSortColumnPtr(ByVal DefaultSortColumnPtr As Integer) As Integer
            Dim VarText As String
            '
            VarText = cpCore.docProperties.getText("ColPtr")
            GetReportSortColumnPtr = genericController.EncodeInteger(VarText)
            If (GetReportSortColumnPtr = 0) And (VarText <> "0") Then
                GetReportSortColumnPtr = DefaultSortColumnPtr
            End If
        End Function
        '
        '=============================================================================
        '   Get Sort Column Type
        '
        '   returns the integer for the type of sorting last requested
        '       0 = nothing selected
        '       1 = sort A-Z
        '       2 = sort Z-A
        '
        '   Orderby is generated by the selection of headers captions by the user
        '   It is up to the calling program to call GetReportOrderBy to get the orderby and use it in the query to generate the cells
        '   This call returns a comma delimited list of integers representing the columns to sort
        '=============================================================================
        '
        Public Function GetReportSortType() As Integer
            Dim VarText As String
            '
            VarText = cpCore.docProperties.getText("ColPtr")
            If (EncodeInteger(VarText) <> 0) Or (VarText = "0") Then
                '
                ' A valid ColPtr was found
                '
                GetReportSortType = cpCore.docProperties.getInteger("ColSort")
            Else
                GetReportSortType = SortingStateEnum.SortableSetAZ
            End If
        End Function
        '
        '=============================================================================
        '   Translate the old GetReport to the new GetReport2
        '=============================================================================
        '
        Public Function GetReport(ByVal RowCount As Integer, ByVal ColCaption() As String, ByVal ColAlign() As String, ByVal ColWidth() As String, ByVal Cells(,) As String, ByVal PageSize As Integer, ByVal PageNumber As Integer, ByVal PreTableCopy As String, ByVal PostTableCopy As String, ByVal DataRowCount As Integer, ByVal ClassStyle As String) As String
            On Error GoTo ErrorTrap
            '
            Dim ColSortable() As Boolean
            Dim ColCnt As Integer
            Dim Ptr As Integer
            '
            ColCnt = UBound(Cells, 2)
            ReDim ColSortable(ColCnt)
            For Ptr = 0 To ColCnt - 1
                ColSortable(Ptr) = False
            Next
            '
            GetReport = GetReport2(RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle, ColSortable, 0)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError("GetReport")
        End Function

        '
        '=============================================================================
        '   Report
        '
        '   This is a list report that you have to fill in all the cells and pass them in arrays.
        '   The column headers are always assumed to include the orderby options. they are linked. To get the correct orderby, the calling program
        '   has to call GetReport2orderby
        '
        '
        '=============================================================================
        '
        Public Function GetReport2(ByVal RowCount As Integer, ByVal ColCaption() As String, ByVal ColAlign() As String, ByVal ColWidth() As String, ByVal Cells As String(,), ByVal PageSize As Integer, ByVal PageNumber As Integer, ByVal PreTableCopy As String, ByVal PostTableCopy As String, ByVal DataRowCount As Integer, ByVal ClassStyle As String, ByVal ColSortable() As Boolean, ByVal DefaultSortColumnPtr As Integer) As String
            Dim result As String = String.Empty
            Try
                Dim RQS As String
                Dim RowBAse As Integer
                Dim Content As New stringBuilderLegacyController
                Dim Stream As New stringBuilderLegacyController
                Dim ColumnCount As Integer
                Dim ColumnPtr As Integer
                Dim ColumnWidth As String
                Dim RowPointer As Integer
                Dim WorkingQS As String
                '
                Dim PageCount As Integer
                Dim PagePointer As Integer
                Dim LinkCount As Integer
                Dim ReportPageNumber As Integer
                Dim ReportPageSize As Integer
                Dim iClassStyle As String
                Dim SortColPtr As Integer
                Dim SortColType As Integer
                '
                ReportPageNumber = PageNumber
                If ReportPageNumber = 0 Then
                    ReportPageNumber = 1
                End If
                ReportPageSize = PageSize
                If ReportPageSize < 1 Then
                    ReportPageSize = 50
                End If
                '
                iClassStyle = ClassStyle
                If iClassStyle = "" Then
                    iClassStyle = "ccPanel"
                End If
                'If IsArray(Cells) Then
                ColumnCount = UBound(Cells, 2)
                'End If
                RQS = cpCore.doc.refreshQueryString
                '
                SortColPtr = GetReportSortColumnPtr(DefaultSortColumnPtr)
                SortColType = GetReportSortType()
                '
                ' ----- Start the table
                '
                Call Content.Add(StartTable(3, 1, 0))
                '
                ' ----- Header
                '
                Call Content.Add(vbCrLf & "<tr>")
                Call Content.Add(GetReport_CellHeader(0, "Row", "50", "Right", "ccAdminListCaption", RQS, SortingStateEnum.NotSortable))
                For ColumnPtr = 0 To ColumnCount - 1
                    ColumnWidth = ColWidth(ColumnPtr)
                    If Not ColSortable(ColumnPtr) Then
                        '
                        ' not sortable column
                        '
                        Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.NotSortable))
                    ElseIf ColumnPtr = SortColPtr Then
                        '
                        ' This is the current sort column
                        '
                        Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, DirectCast(SortColType, SortingStateEnum)))
                    Else
                        '
                        ' Column is sortable, but not selected
                        '
                        Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet))
                    End If

                    'If ColumnPtr = SortColPtr Then
                    '    '
                    '    ' This column is currently the active sort
                    '    '
                    '    Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortColType))
                    'Else
                    '    Call Content.Add(GetReport_CellHeader(ColumnPtr, ColCaption(ColumnPtr), ColumnWidth, ColAlign(ColumnPtr), "ccAdminListCaption", RQS, SortingStateEnum.SortableNotSet))
                    'End If
                Next
                Call Content.Add(vbCrLf & "</tr>")
                '
                ' ----- Data
                '
                If RowCount = 0 Then
                    Call Content.Add(vbCrLf & "<tr>")
                    Call Content.Add(GetReport_Cell((RowBAse + RowPointer).ToString(), "right", 1, RowPointer))
                    Call Content.Add(GetReport_Cell("-- End --", "left", ColumnCount, 0))
                    Call Content.Add(vbCrLf & "</tr>")
                Else
                    RowBAse = (ReportPageSize * (ReportPageNumber - 1)) + 1
                    For RowPointer = 0 To RowCount - 1
                        Call Content.Add(vbCrLf & "<tr>")
                        Call Content.Add(GetReport_Cell((RowBAse + RowPointer).ToString(), "right", 1, RowPointer))
                        For ColumnPtr = 0 To ColumnCount - 1
                            Call Content.Add(GetReport_Cell(Cells(RowPointer, ColumnPtr), ColAlign(ColumnPtr), 1, RowPointer))
                        Next
                        Call Content.Add(vbCrLf & "</tr>")
                    Next
                End If
                '
                ' ----- End Table
                '
                Call Content.Add(kmaEndTable)
                result = result & Content.Text
                '
                ' ----- Post Table copy
                '
                If (DataRowCount / ReportPageSize) <> Int((DataRowCount / ReportPageSize)) Then
                    PageCount = CInt((DataRowCount / ReportPageSize) + 0.5)
                Else
                    PageCount = CInt(DataRowCount / ReportPageSize)
                End If
                If PageCount > 1 Then
                    result = result & "<br>Page " & ReportPageNumber & " (Row " & (RowBAse) & " of " & DataRowCount & ")"
                    If PageCount > 20 Then
                        PagePointer = ReportPageNumber - 10
                    End If
                    If PagePointer < 1 Then
                        PagePointer = 1
                    End If
                    If PageCount > 1 Then
                        result = result & "<br>Go to Page "
                        If PagePointer <> 1 Then
                            WorkingQS = cpCore.doc.refreshQueryString
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, "GotoPage", "1", True)
                            result = result & "<a href=""" & cpCore.webServer.requestPage & "?" & WorkingQS & """>1</A>...&nbsp;"
                        End If
                        WorkingQS = cpCore.doc.refreshQueryString
                        WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageSize, CStr(ReportPageSize), True)
                        Do While (PagePointer <= PageCount) And (LinkCount < 20)
                            If PagePointer = ReportPageNumber Then
                                result = result & PagePointer & "&nbsp;"
                            Else
                                WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, CStr(PagePointer), True)
                                result = result & "<a href=""" & cpCore.webServer.requestPage & "?" & WorkingQS & """>" & PagePointer & "</A>&nbsp;"
                            End If
                            PagePointer = PagePointer + 1
                            LinkCount = LinkCount + 1
                        Loop
                        If PagePointer < PageCount Then
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, CStr(PageCount), True)
                            result = result & "...<a href=""" & cpCore.webServer.requestPage & "?" & WorkingQS & """>" & PageCount & "</A>&nbsp;"
                        End If
                        If ReportPageNumber < PageCount Then
                            WorkingQS = genericController.ModifyQueryString(WorkingQS, RequestNamePageNumber, CStr(ReportPageNumber + 1), True)
                            result = result & "...<a href=""" & cpCore.webServer.requestPage & "?" & WorkingQS & """>next</A>&nbsp;"
                        End If
                        result = result & "<br>&nbsp;"
                    End If
                End If
                '
                result = "" _
                & PreTableCopy _
                & "<table border=0 cellpadding=0 cellspacing=0 width=""100%""><tr><td style=""padding:10px;"">" _
                & result _
                & "</td></tr></table>" _
                & PostTableCopy _
                & ""
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Get the Normal Edit Button Bar String
        '
        '   used on Normal Edit and others
        '========================================================================
        '
        Public Function GetEditButtonBar(ByVal MenuDepth As Integer, ByVal AllowDelete As Boolean, ByVal AllowCancel As Boolean, ByVal allowSave As Boolean, ByVal AllowSpellCheck As Boolean, ByVal AllowPublish As Boolean, ByVal AllowAbort As Boolean, ByVal AllowSubmit As Boolean, ByVal AllowApprove As Boolean) As String
            GetEditButtonBar = GetEditButtonBar2(MenuDepth, AllowDelete, AllowCancel, allowSave, AllowSpellCheck, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, False, False, False, False, False, False, False)
        End Function
        '
        '========================================================================
        ' Get the Normal Edit Button Bar String
        '
        '   used on Normal Edit and others
        '========================================================================
        '
        Public Function GetFormBodyAdminOnly() As String
            GetFormBodyAdminOnly = "<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">This page requires administrator permissions.</div>"
        End Function
        '
        '========================================================================
        '   Builds a single entry in the ReportFilter at the bottom of the page
        '========================================================================
        '
        Public Function GetReportFilterRow(ByVal FormInput As String, ByVal Caption As String) As String
            '
            GetReportFilterRow = "" _
                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                & "<tr><td width=""200"" align=""right"" class=RowInput>" & FormInput & "</td>" _
                & "<td width=""100%"" align=""left"" class=RowCaption>&nbsp;" & Caption & "</td></tr>" _
                & "</table>"
            '
        End Function
        '
        '=============================================================================
        '   Builds the panel around the filters at the bottom of the report
        '=============================================================================
        '
        Public Function GetReportFilter(ByVal Title As String, ByVal Body As String) As String
            Dim result As String = ""
            result = result & "<div class=""ccReportFilter"">"
            result = result & "<div class=""Title"">" & Title & "</div>"
            result = result & "<div class=""Body"">" & Body & "</div>"
            result = result & "</div>"
            Return result
        End Function
        '
        '===========================================================================
        ''' <summary>
        ''' handle legacy errors for this class, v1
        ''' </summary>
        ''' <param name="MethodName"></param>
        ''' <remarks></remarks>
        Private Sub handleLegacyClassError(ByVal MethodName As String)
            '
            Throw (New Exception("unexpected exception in method [" & MethodName & "]"))
            '
        End Sub
    End Class
End Namespace
