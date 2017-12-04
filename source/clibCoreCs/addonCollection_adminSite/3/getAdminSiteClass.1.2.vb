
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Partial Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'Private Function GetForm_EmailControl() As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_EmailControl")
        '    '
        '    Dim Content As New fastStringClass
        '    Dim Copy As String
        '    Dim Button As String
        '    Dim ButtonList As String
        '    Dim SaveAction As Boolean
        '    Dim HelpCopy As String
        '    Dim FieldValue As String
        '    Dim PaymentProcessMethod as integer
        '    Dim Adminui As New adminUIclass(cpcore)
        '    Dim Description As String
        '    '
        '    if true then ' 3.3.009" Then
        '        SettingPageID = cpcore.htmldoc.main_GetRecordID_Internal("Setting Pages", "Email Settings")
        '    End If
        '    If SettingPageID <> 0 Then
        '        Call cpCore.htmldoc.main_AddRefreshQueryString(RequestNameOpenSettingPage, SettingPageID)
        '        GetForm_EmailControl = GetSettingPage(SettingPageID)
        '    Else
        '        Button = cpCore.main_GetStreamText2(RequestNameButton)
        '        If Button = ButtonCancel Then
        '            '
        '            '
        '            '
        '            Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "Email Control, Cancel Button Pressed", False)
        '        ElseIf Not cpCore.main_IsAdmin Then
        '            '
        '            '
        '            '
        '            ButtonList = ButtonCancel
        '            Content.Add( AdminUI.GetFormBodyAdminOnly()
        '        Else
        '            '
        '            ' Process Requests
        '            '
        '            SaveAction = (Button = ButtonSave) Or (Button = ButtonOK)
        '            '
        '            ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
        '            Content.Add( AdminUI.EditTableOpen)
        '            '
        '            ' Common email addresses
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>General Email Addresses</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to contact the site administrator."
        '            Copy = (GetPropertyControl("EmailAdmin", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Admin Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address displayed throughout the site when a visitor is prompted to send site comments."
        '            Copy = (GetPropertyControl("EmailComments", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Comment Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address used on out-going Emails when no other address is available. For your Email to get to its destination, this Email address must be a valid Email account on a mail server."
        '            Copy = (GetPropertyControl("EmailFromAddress", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "General Email From Address", HelpCopy, False, False))
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Trap Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "When checked, all system errors (called traps errors) generate an Email to the Trap Email address."
        '            Copy = (GetPropertyControl("AllowTrapemail", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Trap Error Email", HelpCopy, False, False))
        '            '
        '            HelpCopy = "This is the Email address to which all systems errors (called trap errors) are sent when Allow Trap Error Email is checked."
        '            Copy = (GetPropertyControl("TrapEmail", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Trap Error Email Address", HelpCopy, False, False))
        '            '
        '            ' Email Sending
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Sending Email</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "This is the domain name or IP address of the SMTP mail server you will use to send. If you are using the MS SMTP in IIS on this machine, use 127.0.0.1."
        '            Copy = (GetPropertyControl("SMTPServer", FieldTypeText, SaveAction, "127.0.0.1"))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "SMTP Email Server", HelpCopy, False, False))
        '            '
        '            HelpCopy = "When checked, the login box includes a section for users to enter their Email addresses. When submitted, all username and password matches for that Email address are sent to the Email address."
        '            Copy = (GetPropertyControl("AllowPasswordEmail", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Allow Password Email", HelpCopy, False, False))
        '    '
        '    ' read-only - no longer user configurable
        '    '
        '    '        '
        '            HelpCopy = "This text is included at the bottom of each group, system, and conditional email. It contains a link that the Email recipient can click to block them from future emails from this site. Only site developers can modify this text."
        '            If cpCore.main_IsDeveloper Then
        '                HelpCopy = "<br><br>Developer: This text should conform to standards set by both local and federal law, as well as those required by your email server administrator. To create the clickable link, include link tags around your text (&lt%link&gt;click here&lt%/link&gt;). If you omit the link tag, a (click here) will be added to the end."
        '            End If
        '            Copy = (GetPropertyHTMLControl("EmailSpamFooter", SaveAction, DefaultSpamFooter, (Not cpCore.main_IsDeveloper)))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Email SpamFooter", HelpCopy, False, True))
        '            '
        '            HelpCopy = "Group and Conditional Email are delivered from another program that checks in about every minute. This is the time and date of the last check."
        '            Copy = cpCore.main_GetSiteProperty2("EmailServiceLastCheck")
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Send Email Status", HelpCopy, False, False))
        '            '
        '            ' Bounce Email Handling
        '            '
        '            Call Content.Add(startTableRow & "<td colspan=""3"" class=""ccPanel3D ccAdminEditSubHeader""><b>Bounce Email Handling</b>" & kmaEndTableCell & kmaEndTableRow)
        '            '
        '            HelpCopy = "If present, all outbound Emails that can not be delivered will be returned to this address. This should be a valid Email address on an Email server."
        '            Copy = (GetPropertyControl("EmailBounceAddress", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Address", HelpCopy, False, False))
        '            '
        '            HelpCopy = "When checked, the system will attempt to retrieve bounced Emails from the following Email account and mark the members according to the processing rules included here."
        '            Copy = (GetPropertyControl("AllowEmailBounceProcessing", FieldTypeBoolean, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Process Bounced Emails", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The POP Email server where Emails will be retrieved and processed."
        '            Copy = (GetPropertyControl("POPServer", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Server", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The account username to retrieve Emails for processing."
        '            Copy = (GetPropertyControl("POPServerUsername", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Username", HelpCopy, False, False))
        '            '
        '            HelpCopy = "The account password to retrieve Emails for processing."
        '            Copy = (GetPropertyControl("POPServerPassword", FieldTypeText, SaveAction, ""))
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "POP Email Password", HelpCopy, False, False))
        '            '
        '            HelpCopy = "Set the action to be performed when an Email address is identified as invalid by the bounce process."
        '            If Not SaveAction Then
        '                FieldValue = genericController.EncodeInteger(cpCore.main_GetSiteProperty2("EMAILBOUNCEPROCESSACTION"))
        '            Else
        '                FieldValue = genericController.EncodeInteger(cpCore.main_GetStreamText2("EMAILBOUNCEPROCESSACTION"))
        '                Call cpCore.app.setSiteProperty("EMAILBOUNCEPROCESSACTION", FieldValue)
        '            End If
        '            Copy = "<select size=1 name=EMAILBOUNCEPROCESSACTION>" _
        '                & "<option value=0>Do Nothing</option>" _
        '                & "<option value=1>Clear the Allow Group Email field for all members with a matching Email address</option>" _
        '                & "<option value=2>Clear all member Email addresses that match the Email address</option>" _
        '                & "<option value=3>Delete all Members with a matching Email address</option>" _
        '                & "</select>"
        '            Copy = genericController.vbReplace(Copy, "value=" & FieldValue, "selected value=" & FieldValue)
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Bounce Email Action", HelpCopy, False, False))
        '            '
        '            HelpCopy = "Bounce emails are retrieved about every minute. This is the status of the last check."
        '            Copy = cpCore.main_GetSiteProperty2("POPServerStatus")
        '            Call Content.Add(AdminUI.GetEditRow( Copy, "Last Receive Email Status", HelpCopy, False, False))
        '            '
        '            Content.Add( AdminUI.EditTableClose)
        '            '
        '            ' Close form
        '            '
        '            If Button = ButtonOK Then
        '                Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "EmailControl, OK Button Pressed", False)
        '                'Call cpCore.main_Redirect2(encodeAppRootPath(cpCore.main_GetSiteProperty2("AdminURL"), cpCore.main_ServerVirtualPath, cpCore.app.RootPath, cpCore.main_ServerHost))
        '            End If
        '            Content.Add( cpCore.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEmailControl))
        '        End If
        '        '
        '        Description = "This tool is used to control the Contensive Email processes."
        '        GetForm_EmailControl = AdminUI.GetBody( "Email Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
        '    End If
        '    '
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call HandleClassTrapErrorBubble("GetForm_EmailControl")
        'End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_Downloads() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Downloads")
            '
            Dim IsEmptyList As Boolean
            Dim ResultMessage As String
            Dim LinkPrefix As String
            Dim LinkSuffix As String
            Dim RemoteKey As String
            Dim Copy As String
            Dim Button As String
            Dim ButtonPanel As String
            Dim SaveAction As Boolean
            Dim helpCopy As String
            Dim FieldValue As String
            Dim PaymentProcessMethod As Integer
            Dim Argument1 As String
            Dim CS As Integer
            Dim ContactGroupCriteria As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim GroupChecked As Boolean
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim RowEven As Boolean
            Dim SQL As String
            Dim RQS As String
            Dim SubTab As Integer
            Dim FormSave As Boolean
            Dim FormClear As Boolean
            Dim ContactContentID As Integer
            Dim Criteria As String
            Dim ContentGorupCriteria As String
            Dim ContactSearchCriteria As String
            Dim FieldParms() As String
            Dim CriteriaValues As Object
            Dim CriteriaCount As Integer
            Dim CriteriaPointer As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim TopCount As Integer
            Dim RowPointer As Integer
            Dim DataRowCount As Integer
            Dim PreTableCopy As String = ""
            Dim PostTableCopy As String = ""
            Dim ColumnPtr As Integer
            Dim ColCaption() As String
            Dim ColAlign() As String
            Dim ColWidth() As String
            Dim Cells As String(,)
            Dim GroupID As Integer
            Dim GroupToolAction As Integer
            Dim ActionPanel As String
            Dim RowCount As Integer
            Dim GroupName As String
            Dim MemberID As Integer
            Dim QS As String
            Dim VisitsCell As String
            Dim VisitCount As Integer
            Dim AdminURL As String
            Dim CCID As Integer
            Dim SQLValue As String
            Dim DefaultName As String
            Dim SearchCaption As String
            Dim BlankPanel As String
            Dim RowPageSize As String
            Dim RowGroups As String
            Dim GroupIDs() As String
            Dim GroupPtr As Integer
            Dim GroupDelimiter As String
            Dim DateCompleted As Date
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            Dim ContentID As Integer
            Dim Format As String
            Dim TableName As String
            Dim Filename As String
            Dim Name As String
            Dim Caption As String
            Dim Description As String = ""
            Dim ButtonListLeft As String
            Dim ButtonListRight As String
            Dim ContentPadding As Integer
            Dim ContentSummary As String = ""
            Dim Tab0 As New stringBuilderLegacyController
            Dim Tab1 As New stringBuilderLegacyController
            Dim Content As String = ""
            Dim Cell As String
            Dim Adminui As New adminUIController(cpCore)
            Dim SQLFieldName As String
            '
            Const ColumnCnt = 5
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            If Button = ButtonCancel Then
                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "Downloads, Cancel Button Pressed")
            End If
            '
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' Must be a developer
                '
                ButtonListLeft = ButtonCancel
                ButtonListRight = ""
                Content = Content & Adminui.GetFormBodyAdminOnly()
            Else
                ContentID = cpCore.docProperties.getInteger("ContentID")
                Format = cpCore.docProperties.getText("Format")
                If False Then
                    SQLFieldName = "SQL"
                Else
                    SQLFieldName = "SQLQuery"
                End If
                '
                ' Process Requests
                '
                If Button <> "" Then
                    Select Case Button
                        Case ButtonDelete
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Call cpCore.db.deleteContentRecord("Tasks", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                    End If
                                Next
                            End If
                        Case ButtonRequestDownload
                            '
                            ' Request the download again
                            '
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Dim CSSrc As Integer
                                        Dim CSDst As Integer

                                        CSSrc = cpCore.db.csOpenRecord("Tasks", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                        If cpCore.db.csOk(CSSrc) Then
                                            CSDst = cpCore.db.csInsertRecord("Tasks")
                                            If cpCore.db.csOk(CSDst) Then
                                                Call cpCore.db.csSet(CSDst, "Name", cpCore.db.csGetText(CSSrc, "name"))
                                                Call cpCore.db.csSet(CSDst, SQLFieldName, cpCore.db.csGetText(CSSrc, SQLFieldName))
                                                If genericController.vbLCase(cpCore.db.csGetText(CSSrc, "command")) = "xml" Then
                                                    Call cpCore.db.csSet(CSDst, "Filename", "DupDownload_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".xml")
                                                    Call cpCore.db.csSet(CSDst, "Command", "BUILDXML")
                                                Else
                                                    Call cpCore.db.csSet(CSDst, "Filename", "DupDownload_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv")
                                                    Call cpCore.db.csSet(CSDst, "Command", "BUILDCSV")
                                                End If
                                            End If
                                            Call cpCore.db.csClose(CSDst)
                                        End If
                                        Call cpCore.db.csClose(CSSrc)
                                    End If
                                Next
                            End If
                            '
                            '
                            '
                            If (Format <> "") And (ContentID = 0) Then
                                Description = Description & "<p>Please select a Content before requesting a download</p>"
                            ElseIf (Format = "") And (ContentID <> 0) Then
                                Description = Description & "<p>Please select a Format before requesting a download</p>"
                            ElseIf Format = "CSV" Then
                                CS = cpCore.db.csInsertRecord("Tasks")
                                If cpCore.db.csOk(CS) Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                    Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                                    Name = "CSV Download, " & ContentName
                                    Filename = genericController.vbReplace(ContentName, " ", "") & "_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv"
                                    Call cpCore.db.csSet(CS, "Name", Name)
                                    Call cpCore.db.csSet(CS, "Filename", Filename)
                                    Call cpCore.db.csSet(CS, "Command", "BUILDCSV")
                                    Call cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " & TableName & " where " & Criteria)
                                    Description = Description & "<p>Your CSV Download has been requested.</p>"
                                End If
                                Call cpCore.db.csClose(CS)
                                Format = ""
                                ContentID = 0
                            ElseIf Format = "XML" Then
                                CS = cpCore.db.csInsertRecord("Tasks")
                                If cpCore.db.csOk(CS) Then
                                    ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                                    TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName)
                                    Criteria = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName)
                                    Name = "XML Download, " & ContentName
                                    Filename = genericController.vbReplace(ContentName, " ", "") & "_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".xml"
                                    Call cpCore.db.csSet(CS, "Name", Name)
                                    Call cpCore.db.csSet(CS, "Filename", Filename)
                                    Call cpCore.db.csSet(CS, "Command", "BUILDXML")
                                    Call cpCore.db.csSet(CS, SQLFieldName, "SELECT * from " & TableName & " where " & Criteria)
                                    Description = Description & "<p>Your XML Download has been requested.</p>"
                                End If
                                Call cpCore.db.csClose(CS)
                                Format = ""
                                ContentID = 0
                            End If
                    End Select
                End If
                '
                ' Build Tab0
                '
                'Tab0.Add( "<p>The following is a list of available downloads</p>")
                ''
                RQS = cpCore.doc.refreshQueryString
                PageSize = cpCore.docProperties.getInteger(RequestNamePageSize)
                If PageSize = 0 Then
                    PageSize = 50
                End If
                PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber)
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                AdminURL = "/" & cpCore.serverConfig.appConfig.adminRoute
                TopCount = PageNumber * PageSize
                '
                ' Setup Headings
                '
                ReDim ColCaption(ColumnCnt)
                ReDim ColAlign(ColumnCnt)
                ReDim ColWidth(ColumnCnt)
                ReDim Cells(PageSize, ColumnCnt)
                '
                ColCaption(ColumnPtr) = "Select<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=10 height=1>"
                ColAlign(ColumnPtr) = "center"
                ColWidth(ColumnPtr) = "10"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Name"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100%"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "For<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Requested<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=150 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "150"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "File<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "Left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                '   Get Data
                '
                SQL = "select M.Name as CreatedByName, T.* from ccTasks T left join ccMembers M on M.ID=T.CreatedBy where (T.Command='BuildCSV')or(T.Command='BuildXML') order by T.DateAdded Desc"
                'Call cpCore.main_TestPoint("Selection SQL=" & SQL)
                CS = cpCore.db.csOpenSql_rev("default", SQL, PageSize, PageNumber)
                RowPointer = 0
                If Not cpCore.db.csOk(CS) Then
                    Cells(0, 1) = "There are no download requests"
                    RowPointer = 1
                Else
                    DataRowCount = cpCore.db.csGetRowCount(CS)
                    LinkPrefix = "<a href=""" & cpCore.serverConfig.appConfig.cdnFilesNetprefix
                    LinkSuffix = """ target=_blank>Available</a>"
                    Do While cpCore.db.csOk(CS) And (RowPointer < PageSize)
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        DateCompleted = cpCore.db.csGetDate(CS, "DateCompleted")
                        ResultMessage = cpCore.db.csGetText(CS, "ResultMessage")
                        Cells(RowPointer, 0) = cpCore.html.html_GetFormInputCheckBox2("Row" & RowPointer) & cpCore.html.html_GetFormInputHidden("RowID" & RowPointer, RecordID)
                        Cells(RowPointer, 1) = cpCore.db.csGetText(CS, "name")
                        Cells(RowPointer, 2) = cpCore.db.csGetText(CS, "CreatedByName")
                        Cells(RowPointer, 3) = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString
                        If DateCompleted = Date.MinValue Then
                            RemoteKey = remoteQueryController.main_GetRemoteQueryKey(cpCore, "select DateCompleted,filename,resultMessage from cctasks where id=" & RecordID, "default", 1)
                            Cell = ""
                            Cell = Cell & vbCrLf & "<div id=""pending" & RowPointer & """>Pending <img src=""/ccLib/images/ajax-loader-small.gif"" width=16 height=16></div>"
                            '
                            Cell = Cell & vbCrLf & "<script>"
                            Cell = Cell & vbCrLf & "function statusHandler" & RowPointer & "(results) {"
                            Cell = Cell & vbCrLf & " var jo,isDone=false;"
                            Cell = Cell & vbCrLf & " eval('jo='+results);"
                            Cell = Cell & vbCrLf & " if (jo){"
                            Cell = Cell & vbCrLf & "  if(jo.DateCompleted) {"
                            Cell = Cell & vbCrLf & "    var dst=document.getElementById('pending" & RowPointer & "');"
                            Cell = Cell & vbCrLf & "    isDone=true;"
                            Cell = Cell & vbCrLf & "    if(jo.resultMessage=='OK') {"
                            Cell = Cell & vbCrLf & "      dst.innerHTML='" & LinkPrefix & "'+jo.filename+'" & LinkSuffix & "';"
                            Cell = Cell & vbCrLf & "    }else{"
                            Cell = Cell & vbCrLf & "      dst.innerHTML='error';"
                            Cell = Cell & vbCrLf & "    }"
                            Cell = Cell & vbCrLf & "  }"
                            Cell = Cell & vbCrLf & " }"
                            Cell = Cell & vbCrLf & " if(!isDone) setTimeout(""requestStatus" & RowPointer & "()"",5000)"
                            Cell = Cell & vbCrLf & "}"
                            '
                            Cell = Cell & vbCrLf & "function requestStatus" & RowPointer & "() {"
                            Cell = Cell & vbCrLf & "  cj.ajax.getNameValue(statusHandler" & RowPointer & ",'" & RemoteKey & "');"
                            Cell = Cell & vbCrLf & "}"
                            Cell = Cell & vbCrLf & "requestStatus" & RowPointer & "();"
                            Cell = Cell & vbCrLf & "</script>"
                            '
                            Cells(RowPointer, 4) = Cell
                        ElseIf ResultMessage = "ok" Then
                            Cells(RowPointer, 4) = "<div id=""pending" & RowPointer & """>" & LinkPrefix & cpCore.db.csGetText(CS, "filename") & LinkSuffix & "</div>"
                        Else
                            Cells(RowPointer, 4) = "<div id=""pending" & RowPointer & """><a href=""javascript:alert('" & genericController.EncodeJavascript(ResultMessage) & ";return false');"">error</a></div>"
                        End If
                        RowPointer = RowPointer + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                End If
                Call cpCore.db.csClose(CS)
                Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer))
                Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel")
                Tab0.Add(Cell)
                'Tab0.Add( "<div style=""height:200px;"">" & Cell & "</div>"
                '        '
                '        ' Build RequestContent Form
                '        '
                '        Tab1.Add( "<p>Use this form to request a download. Select the criteria for the download and click the [Request Download] button. The request should then appear on the requested download list in the other tab. When the download has been created, it will be become available.</p>")
                '        '
                '        Tab1.Add( "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                '        '
                '        Call Tab1.Add("<tr>")
                '        Call Tab1.Add("<td align=right>Content</td>")
                '        Call Tab1.Add("<td>" & cpCore.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content", "", "", "", IsEmptyList) & "</td>")
                '        Call Tab1.Add("</tr>")
                '        '
                '        Call Tab1.Add("<tr>")
                '        Call Tab1.Add("<td align=right>Format</td>")
                '        Call Tab1.Add("<td><select name=Format value=""" & Format & """><option value=CSV>CSV</option><option name=XML value=XML>XML</option></select></td>")
                '        Call Tab1.Add("</tr>")
                '        '
                '        Call Tab1.Add("" _
                '            & "<tr>" _
                '            & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
                '            & "<td width=""100%"">&nbsp;</td>" _
                '            & "</tr>" _
                '            & "</table>")
                '        '
                '        ' Build and add tabs
                '        '
                '        Call cpCore.htmldoc.main_AddLiveTabEntry("Current&nbsp;Downloads", Tab0.Text, "ccAdminTab")
                '        Call cpCore.htmldoc.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Download", Tab1.Text, "ccAdminTab")
                '        Content = cpCore.htmldoc.main_GetLiveTabs()
                Content = Tab0.Text
                '
                ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete
                'ButtonListLeft = ButtonCancel & "," & ButtonRefresh & "," & ButtonDelete & "," & ButtonRequestDownload
                ButtonListRight = ""
                Content = Content & cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormDownloads)
            End If
            '
            Caption = "Download Manager"
            Description = "" _
                & "<p>The Download Manager holds all downloads requested from anywhere on the website. It also provides tools to request downloads from any Content.</p>" _
                & "<p>To add a new download of any content in Contensive, click Export on the filter tab of the content listing page. To add a new download from a SQL statement, use Custom Reports under Reports on the Navigator.</p>"
            ContentPadding = 0
            GetForm_Downloads = Adminui.GetBody(Caption, ButtonListLeft, ButtonListRight, True, True, Description, ContentSummary, ContentPadding, Content)
            '
            Call cpCore.html.addTitle(Caption)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Downloads")
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function GetForm_Edit_AddTab(ByVal Caption As String, ByVal Content As String, ByVal AllowAdminTabs As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_AddTab")
            '
            If Content <> "" Then
                If Not AllowAdminTabs Then
                    GetForm_Edit_AddTab = Content
                Else
                    Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", "", Content, False, "ccAdminTab")
                    'Call cpCore.htmldoc.main_AddLiveTabEntry(Replace(Caption, " ", "&nbsp;"), Content, "ccAdminTab")
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_AddTab")
        End Function
        '
        '========================================================================
        '   Creates Tabbed content that is either Live (all content on page) or Ajax (click and ajax in the content)
        '========================================================================
        '
        Private Function GetForm_Edit_AddTab2(ByVal Caption As String, ByVal Content As String, ByVal AllowAdminTabs As Boolean, ByVal AjaxLink As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_Edit_AddTab2")
            '
            If Not AllowAdminTabs Then
                '
                ' non-tab mode
                '
                GetForm_Edit_AddTab2 = Content
            ElseIf AjaxLink <> "" Then
                '
                ' Ajax Tab
                '
                Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", AjaxLink, "", False, "ccAdminTab")
            Else
                '
                ' Live Tab
                '
                Call cpCore.html.menu_AddComboTabEntry(Replace(Caption, " ", "&nbsp;"), "", "", Content, False, "ccAdminTab")
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_AddTab2")
        End Function
        '
        '=============================================================================
        ' Create a child content
        '=============================================================================
        '
        Private Function GetForm_PageContentMap() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_PageContentMap")
            '
            GetForm_PageContentMap = "<p>The Page Content Map has been replaced with the Site Explorer, available as an Add-on through the Add-on Manager.</p>"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_PageContentMap")
        End Function
        '
        '
        '
        Private Function GetForm_Edit_Tabs(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal readOnlyField As Boolean, ByVal IsLandingPage As Boolean, ByVal IsRootPage As Boolean, ByVal EditorContext As csv_contentTypeEnum, ByVal allowAjaxTabs As Boolean, ByVal TemplateIDForStyles As Integer, ByVal fieldTypeDefaultEditors As String(), ByVal fieldEditorPreferenceList As String, ByVal styleList As String, ByVal styleOptionList As String, ByVal emailIdForStyles As Integer, ByVal IsTemplateTable As Boolean, ByVal editorAddonListJSON As String) As String
            Dim returnHtml As String = ""
            Try
                '
                Dim tabContent As String
                Dim AjaxLink As String
                Dim TabsFound As New List(Of String)
                Dim editTabCaption As String
                Dim NewFormFieldList As String
                Dim FormFieldList As String
                Dim AllowHelpMsgCustom As Boolean
                Dim IDList As String
                Dim dt As DataTable
                Dim TempVar As String(,)
                Dim HelpCnt As Integer
                Dim fieldId As Integer
                Dim LastFieldID As Integer
                Dim HelpPtr As Integer
                Dim HelpIDCache() As Integer = {}
                Dim helpDefaultCache() As String = {}
                Dim HelpCustomCache() As String = {}
                Dim helpIdIndex As New keyPtrController
                Dim fieldNameLc As String
                '
                ' ----- read in help
                '
                IDList = ""
                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                    IDList = IDList & "," & field.id
                Next
                If IDList <> "" Then
                    IDList = Mid(IDList, 2)
                End If
                '
                dt = cpCore.db.executeQuery("select fieldid,helpdefault,helpcustom from ccfieldhelp where fieldid in (" & IDList & ") order by fieldid,id")
                TempVar = cpCore.db.convertDataTabletoArray(dt)
                If TempVar.GetLength(0) > 0 Then
                    HelpCnt = UBound(TempVar, 2) + 1
                    ReDim HelpIDCache(HelpCnt)
                    ReDim helpDefaultCache(HelpCnt)
                    ReDim HelpCustomCache(HelpCnt)
                    fieldId = -1
                    For HelpPtr = 0 To HelpCnt - 1
                        fieldId = genericController.EncodeInteger(TempVar(0, HelpPtr))
                        If fieldId <> LastFieldID Then
                            LastFieldID = fieldId
                            HelpIDCache(HelpPtr) = fieldId
                            Call helpIdIndex.setPtr(CStr(fieldId), HelpPtr)
                            helpDefaultCache(HelpPtr) = genericController.encodeText(TempVar(1, HelpPtr))
                            HelpCustomCache(HelpPtr) = genericController.encodeText(TempVar(2, HelpPtr))
                        End If
                    Next
                    AllowHelpMsgCustom = True
                End If
                '
                FormFieldList = ","
                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In adminContent.fields
                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                    If (field.authorable) And (field.active) And (Not TabsFound.Contains(field.editTabName.ToLower())) Then
                        TabsFound.Add(field.editTabName.ToLower())
                        fieldNameLc = field.nameLc
                        editTabCaption = field.editTabName
                        If editTabCaption = "" Then
                            editTabCaption = "Details"
                        End If
                        NewFormFieldList = ""
                        If (Not allowAdminTabs) Or (Not allowAjaxTabs) Or (editTabCaption.ToLower() = "details") Then
                            '
                            ' Live Tab (non-tab mode, non-ajax mode, or details tab
                            '
                            tabContent = GetForm_Edit_Tab(adminContent, editRecord, editRecord.id, adminContent.Id, readOnlyField, IsLandingPage, IsRootPage, field.editTabName, EditorContext, NewFormFieldList, TemplateIDForStyles, HelpCnt, HelpIDCache, helpDefaultCache, HelpCustomCache, AllowHelpMsgCustom, helpIdIndex, fieldTypeDefaultEditors, fieldEditorPreferenceList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON)
                            If tabContent <> "" Then
                                returnHtml &= GetForm_Edit_AddTab2(editTabCaption, tabContent, allowAdminTabs, "")
                            End If
                        Else
                            '
                            ' Ajax Tab
                            '
                            'AjaxLink = "/admin/index.asp?"
                            AjaxLink = "/" & cpCore.serverConfig.appConfig.adminRoute & "?" _
                            & RequestNameAjaxFunction & "=" & AjaxGetFormEditTabContent _
                            & "&ID=" & editRecord.id _
                            & "&CID=" & adminContent.Id _
                            & "&ReadOnly=" & readOnlyField _
                            & "&IsLandingPage=" & IsLandingPage _
                            & "&IsRootPage=" & IsRootPage _
                            & "&EditTab=" & genericController.EncodeRequestVariable(field.editTabName) _
                            & "&EditorContext=" & EditorContext _
                            & "&NewFormFieldList=" & genericController.EncodeRequestVariable(NewFormFieldList)
                            returnHtml &= GetForm_Edit_AddTab2(editTabCaption, "", True, AjaxLink)
                        End If
                        If NewFormFieldList <> "" Then
                            FormFieldList = NewFormFieldList & FormFieldList
                        End If
                    End If
                Next
                '
                ' ----- add the FormFieldList hidden - used on read to make sure all fields are returned
                '       this may not be needed, but we are having a problem with forms coming back without values
                '
                '
                ' moved this to GetEditTabContent - so one is added for each tab.
                '
                returnHtml &= cpCore.html.html_GetFormInputHidden("FormFieldList", FormFieldList)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '        '
        '        ' Delete this when I can verify the Csvr patch to the instream process works
        '        '
        '        Private Sub VerifyDynamicMenuStyleSheet(ByVal MenuID As Integer)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogAdminMethodEnter("VerifyDynamicMenuStyleSheet")
        '            '
        '            Dim StyleSN As String
        '            Dim EditTabCaption As String
        '            Dim ACTags() As String
        '            Dim TagPtr As Integer
        '            Dim QSPos As Integer
        '            Dim QSPosEnd As Integer
        '            Dim QS As String
        '            Dim MenuName As String
        '            Dim StylePrefix As String
        '            Dim CS As Integer
        '            Dim IsFound As Boolean
        '            Dim StyleSheet As String
        '            Dim DefaultStyles As String
        '            Dim DynamicStyles As String
        '            Dim AddStyles As String
        '            Dim StyleSplit() As String
        '            Dim StylePtr As Integer
        '            Dim StyleLine As String
        '            Dim Filename As String
        '            Dim NewStyleLine As String
        '            Dim TestSTyles As String

        '            '
        '            CS = cpCore.main_OpenCSContentRecord("Dynamic Menus", MenuID)
        '            If cpCore.app.IsCSOK(CS) Then
        '                StylePrefix = cpCore.db.cs_getText(CS, "StylePrefix")
        '                If StylePrefix <> "" And genericController.vbUCase(StylePrefix) <> "CCFLYOUT" Then
        '                    if true then ' 3.3.951" Then
        '                        TestSTyles = cpCore.app.cs_get(CS, "StylesFilename")
        '                    Else
        '                        TestSTyles = cpCore.main_GetStyleSheet
        '                    End If
        '                    If genericController.vbInstr(1, TestSTyles, "." & StylePrefix, vbTextCompare) = 0 Then
        '                        '
        '                        ' style not found, get the default ccFlyout styles
        '                        '
        '                        DefaultStyles = RemoveStyleTags(cpCore.cluster.programDataFiles.ReadFile("ccLib\" & "Styles\" & defaultStyleFilename))
        '                        'DefaultStyles = genericController.vbReplace(DefaultStyles, vbCrLf, " ")
        '                        Do While genericController.vbInstr(1, DefaultStyles, "  ") <> 0
        '                            DefaultStyles = genericController.vbReplace(DefaultStyles, "  ", " ")
        '                        Loop
        '                        StyleSplit = Split(DefaultStyles, "}")
        '                        For StylePtr = 0 To UBound(StyleSplit)
        '                            StyleLine = StyleSplit(StylePtr)
        '                            If StyleLine <> "" Then
        '                                If genericController.vbInstr(1, StyleLine, ".ccflyout", vbTextCompare) <> 0 Then
        '                                    StyleLine = genericController.vbReplace(StyleLine, vbCrLf, " ")
        '                                    StyleLine = genericController.vbReplace(StyleLine, ".ccflyout", "." & StylePrefix, vbTextCompare)
        '                                    Do While Left(StyleLine, 1) = " "
        '                                        StyleLine = Mid(StyleLine, 2)
        '                                    Loop
        '                                    AddStyles = AddStyles & StyleLine & "}" & vbCrLf
        '                                End If
        '                            End If
        '                        Next
        '                        If AddStyles <> "" Then
        '                            '
        '                            '
        '                            '
        '                            if true then ' 3.3.951" Then
        '                                '
        '                                ' Add new styles to the StylesFilename field
        '                                '
        '                                DynamicStyles = "" _
        '                                    & cpCore.app.cs_get(CS, "StylesFilename") _
        '                                    & vbCrLf & "" _
        '                                    & vbCrLf & "/* Menu Styles for Style Prefix [" & StylePrefix & "] created " & nt(cpCore.main_PageStartTime.toshortdateString & " */" _
        '                                    & vbCrLf & "" _
        '                                    & vbCrLf & AddStyles _
        '                                    & ""
        '                                Call cpCore.app.SetCS(CS, "StylesFilename", DynamicStyles)
        '                            Else
        '                                '
        '                                ' Legacy - add styles to the site stylesheet
        '                                '
        '                                Filename = cpCore.app.confxxxig.physicalFilePath & DynamicStylesFilename
        '                                DynamicStyles = RemoveStyleTags(cpCore.app.publicFiles.ReadFile(Filename)) & vbCrLf & AddStyles
        '                                Call cpCore.app.publicFiles.SaveFile(Filename, DynamicStyles)
        '                                '
        '                                ' Now create admin and public stylesheets from the styles.css styles
        '                                '
        '                                StyleSN = (cpCore.app.siteProperty_getInteger("StylesheetSerialNumber", "0"))
        '                                If StyleSN <> 0 Then
        '                                    ' mark to rebuild next fetch
        '                                    Call cpCore.app.siteProperty_set("StylesheetSerialNumber", "-1")
        '                                    '' Linked Styles
        '                                    '' Bump the Style Serial Number so next fetch is not cached
        '                                    ''
        '                                    'StyleSN = StyleSN + 1
        '                                    'Call cpCore.app.setSiteProperty("StylesheetSerialNumber", StyleSN)
        '                                    ''
        '                                    '' Save new public stylesheet
        '                                    ''
        '                                    '' 11/24/2009 - style sheet processing deprecated
        '                                    'Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheet)
        '                                    ''Call cpCore.app.publicFiles.SaveFile("templates\Public" & StyleSN & ".css", cpCore.main_GetStyleSheetProcessed)
        '                                    'Call cpCore.app.publicFiles.SaveFile("templates\Admin" & StyleSN & ".css", cpCore.main_GetStyleSheetDefault)
        '                                End If
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Call cpCore.app.closeCS(CS)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("GetForm_Edit_UserFieldTabs")
        '        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function GetForm_CustomReports() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter("GetForm_CustomReports")
            '
            Dim Copy As String
            Dim Button As String
            Dim ButtonPanel As String
            Dim SaveAction As Boolean
            Dim helpCopy As String
            Dim FieldValue As String
            Dim PaymentProcessMethod As Integer
            Dim Argument1 As String
            Dim CS As Integer
            Dim ContactGroupCriteria As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim GroupChecked As Boolean
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim RowEven As Boolean
            Dim SQL As String
            Dim RQS As String
            Dim SubTab As Integer
            Dim FormSave As Boolean
            Dim FormClear As Boolean
            Dim ContactContentID As Integer
            Dim Criteria As String
            Dim ContentGorupCriteria As String
            Dim ContactSearchCriteria As String
            Dim FieldParms() As String
            Dim CriteriaValues As Object
            Dim CriteriaCount As Integer
            Dim CriteriaPointer As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim TopCount As Integer
            Dim RowPointer As Integer
            Dim DataRowCount As Integer
            Dim PreTableCopy As String = ""
            Dim PostTableCopy As String = ""
            Dim ColumnPtr As Integer
            Dim ColCaption() As String
            Dim ColAlign() As String
            Dim ColWidth() As String
            Dim Cells As String(,)
            Dim GroupID As Integer
            Dim GroupToolAction As Integer
            Dim ActionPanel As String
            Dim RowCount As Integer
            Dim GroupName As String
            Dim MemberID As Integer
            Dim QS As String
            Dim VisitsCell As String
            Dim VisitCount As Integer
            Dim AdminURL As String
            Dim CCID As Integer
            Dim SQLValue As String
            Dim DefaultName As String
            Dim SearchCaption As String
            Dim BlankPanel As String
            Dim RowPageSize As String
            Dim RowGroups As String
            Dim GroupIDs() As String
            Dim GroupPtr As Integer
            Dim GroupDelimiter As String
            Dim DateCompleted As Date
            Dim RowCnt As Integer
            Dim RowPtr As Integer
            Dim ContentID As Integer
            Dim Format As String
            Dim TableName As String
            Dim Filename As String
            Dim Name As String
            Dim Caption As String
            Dim Description As String
            Dim ButtonListLeft As String
            Dim ButtonListRight As String
            Dim ContentPadding As Integer
            Dim ContentSummary As String = ""
            Dim Tab0 As New stringBuilderLegacyController
            Dim Tab1 As New stringBuilderLegacyController
            Dim Content As String = ""
            Dim SQLFieldName As String
            '
            Const ColumnCnt = 4
            '
            Button = cpCore.docProperties.getText(RequestNameButton)
            ContentID = cpCore.docProperties.getInteger("ContentID")
            Format = cpCore.docProperties.getText("Format")
            '
            Caption = "Custom Report Manager"
            Description = "Custom Reports are a way for you to create a snapshot of data to view or download. To request a report, select the Custom Reports tab, check the report(s) you want, and click the [Request Download] Button. When your report is ready, it will be available in the <a href=""?" & RequestNameAdminForm & "=30"">Download Manager</a>. To create a new custom report, select the Request New Report tab, enter a name and SQL statement, and click the Apply button."
            ContentPadding = 0
            ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload
            'ButtonListLeft = ButtonCancel & "," & ButtonDelete & "," & ButtonRequestDownload & "," & ButtonApply
            ButtonListRight = ""
            If False Then
                SQLFieldName = "SQL"
            Else
                SQLFieldName = "SQLQuery"
            End If
            '
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                '
                ' Must be a developer
                '
                Description = Description & "You can not access the Custom Report Manager because your account is not configured as an administrator."
            Else
                '
                ' Process Requests
                '
                If Button <> "" Then
                    Select Case Button
                        Case ButtonCancel
                            Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "CustomReports, Cancel Button Pressed")
                            'Call cpCore.main_Redirect2(encodeAppRootPath(cpCore.main_GetSiteProperty2("AdminURL"), cpCore.main_ServerVirtualPath, cpCore.app.RootPath, cpCore.main_ServerHost))
                        Case ButtonDelete
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        Call cpCore.db.deleteContentRecord("Custom Reports", cpCore.docProperties.getInteger("RowID" & RowPtr))
                                    End If
                                Next
                            End If
                        Case ButtonRequestDownload, ButtonApply
                            '
                            Name = cpCore.docProperties.getText("name")
                            SQL = cpCore.docProperties.getText(SQLFieldName)
                            If Name <> "" Or SQL <> "" Then
                                If (Name = "") Or (SQL = "") Then
                                    errorController.error_AddUserError(cpCore, "A name and SQL Query are required to save a new custom report.")
                                Else
                                    CS = cpCore.db.csInsertRecord("Custom Reports")
                                    If cpCore.db.csOk(CS) Then
                                        Call cpCore.db.csSet(CS, "Name", Name)
                                        Call cpCore.db.csSet(CS, SQLFieldName, SQL)
                                    End If
                                    Call cpCore.db.csClose(CS)
                                End If
                            End If
                            '
                            RowCnt = cpCore.docProperties.getInteger("RowCnt")
                            If RowCnt > 0 Then
                                For RowPtr = 0 To RowCnt - 1
                                    If cpCore.docProperties.getBoolean("Row" & RowPtr) Then
                                        RecordID = cpCore.docProperties.getInteger("RowID" & RowPtr)
                                        CS = cpCore.db.csOpenRecord("Custom Reports", RecordID)
                                        If cpCore.db.csOk(CS) Then
                                            SQL = cpCore.db.csGetText(CS, SQLFieldName)
                                            Name = cpCore.db.csGetText(CS, "Name")
                                        End If
                                        Call cpCore.db.csClose(CS)
                                        '
                                        CS = cpCore.db.csInsertRecord("Tasks")
                                        If cpCore.db.csOk(CS) Then
                                            RecordName = "CSV Download, Custom Report [" & Name & "]"
                                            Filename = "CustomReport_" & CStr(genericController.dateToSeconds(cpCore.doc.profileStartTime)) & CStr(genericController.GetRandomInteger()) & ".csv"
                                            Call cpCore.db.csSet(CS, "Name", RecordName)
                                            Call cpCore.db.csSet(CS, "Filename", Filename)
                                            If Format = "XML" Then
                                                Call cpCore.db.csSet(CS, "Command", "BUILDXML")
                                            Else
                                                Call cpCore.db.csSet(CS, "Command", "BUILDCSV")
                                            End If
                                            Call cpCore.db.csSet(CS, SQLFieldName, SQL)
                                            Description = Description & "<p>Your Download [" & Name & "] has been requested, and will be available in the <a href=""?" & RequestNameAdminForm & "=30"">Download Manager</a> when it is complete. This may take a few minutes depending on the size of the report.</p>"
                                        End If
                                        Call cpCore.db.csClose(CS)
                                    End If
                                Next
                            End If
                    End Select
                End If
                '
                ' Build Tab0
                '
                Tab0.Add("<p>The following is a list of available custom reports.</p>")
                '
                RQS = cpCore.doc.refreshQueryString
                PageSize = cpCore.docProperties.getInteger(RequestNamePageSize)
                If PageSize = 0 Then
                    PageSize = 50
                End If
                PageNumber = cpCore.docProperties.getInteger(RequestNamePageNumber)
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                AdminURL = "/" & cpCore.serverConfig.appConfig.adminRoute
                TopCount = PageNumber * PageSize
                '
                ' Setup Headings
                '
                ReDim ColCaption(ColumnCnt)
                ReDim ColAlign(ColumnCnt)
                ReDim ColWidth(ColumnCnt)
                ReDim Cells(PageSize, ColumnCnt)
                '
                ColCaption(ColumnPtr) = "Select<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=10 height=1>"
                ColAlign(ColumnPtr) = "center"
                ColWidth(ColumnPtr) = "10"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Name"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100%"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Created By<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "100"
                ColumnPtr = ColumnPtr + 1
                '
                ColCaption(ColumnPtr) = "Date Created<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=150 height=1>"
                ColAlign(ColumnPtr) = "left"
                ColWidth(ColumnPtr) = "150"
                ColumnPtr = ColumnPtr + 1
                ''
                'ColCaption(ColumnPtr) = "?<br><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=100 height=1>"
                'ColAlign(ColumnPtr) = "Left"
                'ColWidth(ColumnPtr) = "100"
                'ColumnPtr = ColumnPtr + 1
                '
                '   Get Data
                '
                CS = cpCore.db.csOpen("Custom Reports")
                RowPointer = 0
                If Not cpCore.db.csOk(CS) Then
                    Cells(0, 1) = "There are no custom reports defined"
                    RowPointer = 1
                Else
                    DataRowCount = cpCore.db.csGetRowCount(CS)
                    Do While cpCore.db.csOk(CS) And (RowPointer < PageSize)
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        'DateCompleted = cpCore.db.cs_getDate(CS, "DateCompleted")
                        Cells(RowPointer, 0) = cpCore.html.html_GetFormInputCheckBox2("Row" & RowPointer) & cpCore.html.html_GetFormInputHidden("RowID" & RowPointer, RecordID)
                        Cells(RowPointer, 1) = cpCore.db.csGetText(CS, "name")
                        Cells(RowPointer, 2) = cpCore.db.csGet(CS, "CreatedBy")
                        Cells(RowPointer, 3) = cpCore.db.csGetDate(CS, "DateAdded").ToShortDateString
                        'Cells(RowPointer, 4) = "&nbsp;"
                        RowPointer = RowPointer + 1
                        Call cpCore.db.csGoNext(CS)
                    Loop
                End If
                Call cpCore.db.csClose(CS)
                Dim Cell As String
                Tab0.Add(cpCore.html.html_GetFormInputHidden("RowCnt", RowPointer))
                Dim Adminui As New adminUIController(cpCore)
                Cell = Adminui.GetReport(RowPointer, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, "ccPanel")
                Tab0.Add("<div>" & Cell & "</div>")
                '
                ' Build RequestContent Form
                '
                Tab1.Add("<p>Use this form to create a new custom report. Enter the SQL Query for the report, and a name that will be used as a caption.</p>")
                '
                Tab1.Add("<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
                '
                Call Tab1.Add("<tr>")
                Call Tab1.Add("<td align=right>Name</td>")
                Call Tab1.Add("<td>" & cpCore.html.html_GetFormInputText2("Name", "", 1, 40) & "</td>")
                Call Tab1.Add("</tr>")
                '
                Call Tab1.Add("<tr>")
                Call Tab1.Add("<td align=right>SQL Query</td>")
                Call Tab1.Add("<td>" & cpCore.html.html_GetFormInputText2(SQLFieldName, "", 8, 40) & "</td>")
                Call Tab1.Add("</tr>")
                '
                Call Tab1.Add("" _
                    & "<tr>" _
                    & "<td width=""120""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""120"" height=""1""></td>" _
                    & "<td width=""100%"">&nbsp;</td>" _
                    & "</tr>" _
                    & "</table>")
                '
                ' Build and add tabs
                '
                Call cpCore.html.main_AddLiveTabEntry("Custom&nbsp;Reports", Tab0.Text, "ccAdminTab")
                Call cpCore.html.main_AddLiveTabEntry("Request&nbsp;New&nbsp;Report", Tab1.Text, "ccAdminTab")
                Content = cpCore.html.main_GetLiveTabs()
                '
            End If
            '
            GetForm_CustomReports = admin_GetAdminFormBody(Caption, ButtonListLeft, ButtonListRight, True, True, Description, ContentSummary, ContentPadding, Content)
            '
            Call cpCore.html.addTitle("Custom Reports")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_CustomReports")
        End Function


    End Class
End Namespace
