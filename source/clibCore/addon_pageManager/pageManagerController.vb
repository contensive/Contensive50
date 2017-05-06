
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.coreCommonModule
Imports System.Xml
Imports Contensive.Core
'
Namespace Contensive.Core.Controllers
    '
    Public Class pageManagerController
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ' unless state is being saved in this class, consider it static
        '====================================================================================================
        '
        '====================================================================================================
        ''' <summary>
        ''' future pageManager addon interface
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                '
                '
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        ' Print the See Also listing
        '   ContentName is the name of the parent table
        '   RecordID is the parent RecordID
        '=============================================================================
        '
        Public Shared Function main_GetSeeAlso(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer) As String
            Dim result As String = ""
            Try
                '
                'If Not (true) Then Exit Function
                '
                Dim SQL As String
                Dim CS As Integer
                Dim SeeAlsoLink As String
                Dim ContentID As Integer
                Dim SeeAlsoCount As Integer
                Dim Copy As String
                Dim MethodName As String
                Dim iContentName As String
                Dim iRecordID As Integer
                Dim IsEditingLocal As Boolean
                '
                iContentName = genericController.encodeText(ContentName)
                iRecordID = genericController.EncodeInteger(RecordID)
                '
                MethodName = "result"
                '
                SeeAlsoCount = 0
                If iRecordID > 0 Then
                    ContentID = cpcore.main_GetContentID(iContentName)
                    If (ContentID > 0) Then
                        '
                        ' ----- Set authoring only for valid ContentName
                        '
                        IsEditingLocal = cpcore.user.isEditing(iContentName)
                    Else
                        '
                        ' ----- if iContentName was bad, maybe they put table in, no authoring
                        '
                        ContentID = cpcore.GetContentIDByTablename(iContentName)
                    End If
                    If (ContentID > 0) Then
                        '
                        CS = cpcore.db.cs_open("See Also", "((active<>0)AND(ContentID=" & ContentID & ")AND(RecordID=" & iRecordID & "))")
                        Do While (cpcore.db.cs_ok(CS))
                            SeeAlsoLink = (cpcore.db.cs_getText(CS, "Link"))
                            If SeeAlsoLink <> "" Then
                                result = result & cr & "<li class=""ccListItem"">"
                                If genericController.vbInstr(1, SeeAlsoLink, "://") = 0 Then
                                    SeeAlsoLink = cpcore.webServerIO_requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & cpcore.main_GetRecordEditLink2("See Also", (cpcore.db.cs_getInteger(CS, "ID")), False, "", cpcore.user.isEditing("See Also"))
                                End If
                                result = result & "<a href=""" & cpcore.html.html_EncodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.cs_getText(CS, "Name")) & "</A>"
                                Copy = (cpcore.db.cs_getText(CS, "Brief"))
                                If Copy <> "" Then
                                    result = result & "<br >" & AddSpan(Copy, "ccListCopy")
                                End If
                                SeeAlsoCount = SeeAlsoCount + 1
                                result = result & "</li>"
                            End If
                            cpcore.db.cs_goNext(CS)
                        Loop
                        cpcore.db.cs_Close(CS)
                        '
                        If IsEditingLocal Then
                            SeeAlsoCount = SeeAlsoCount + 1
                            result = result & cr & "<li class=""ccListItem"">" & cpcore.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
                        End If
                    End If
                    '
                    If SeeAlsoCount = 0 Then
                        result = ""
                    Else
                        result = "<p>See Also" & cr & "<ul class=""ccList"">" & kmaIndent(result) & cr & "</ul></p>"
                    End If
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Print the "for more information, please contact" line
        '
        '========================================================================
        '
        Public Shared Function main_GetMoreInfo(cpcore As coreClass, ByVal contactMemberID As Integer) As String
            Dim result As String = ""
            Try
                main_GetMoreInfo = pageManager_getMoreInfoHtml(cpcore, genericController.EncodeInteger(contactMemberID))
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' ----- prints a link to the feedback popup form
        '
        '   Creates a sub-form that when submitted, is logged by the notes
        '   system (in MembersLib right now). When submitted, it prints a thank you
        '   message.
        '
        '========================================================================
        '
        Public Shared Function main_GetFeedbackForm(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, ByVal ToMemberID As Integer, Optional ByVal headline As String = "") As String
            Dim result As String = ""
            Try
                Dim Panel As String
                Dim Copy As String
                Dim FeedbackButton As String
                Dim NoteCopy As String
                Dim NoteFromEmail As String
                Dim NoteFromName As String
                Dim CS As Integer
                Dim iContentName As String
                Dim iRecordID As Integer
                Dim iToMemberID As Integer
                Dim iHeadline As String
                '
                iContentName = genericController.encodeText(ContentName)
                iRecordID = genericController.EncodeInteger(RecordID)
                iToMemberID = genericController.EncodeInteger(ToMemberID)
                iHeadline = encodeEmptyText(headline, "")
                '
                Const FeedbackButtonSubmit = "Submit"
                '
                FeedbackButton = cpcore.docProperties.getText("fbb")
                Select Case FeedbackButton
                    Case FeedbackButtonSubmit
                        '
                        ' ----- form was submitted, save the note, send it and say thanks
                        '
                        NoteFromName = cpcore.docProperties.getText("NoteFromName")
                        NoteFromEmail = cpcore.docProperties.getText("NoteFromEmail")
                        '
                        NoteCopy = NoteCopy & "Feedback Submitted" & BR
                        NoteCopy = NoteCopy & "From " & NoteFromName & " at " & NoteFromEmail & BR
                        NoteCopy = NoteCopy & "Replying to:" & BR
                        If iHeadline <> "" Then
                            NoteCopy = NoteCopy & "    Article titled [" & iHeadline & "]" & BR
                        End If
                        NoteCopy = NoteCopy & "    Record [" & iRecordID & "] in Content Definition [" & iContentName & "]" & BR
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Comments</b>" & BR
                        '
                        Copy = cpcore.docProperties.getText("NoteCopy")
                        If (Copy = "") Then
                            NoteCopy = NoteCopy & "[no comments entered]" & BR
                        Else
                            NoteCopy = NoteCopy & cpcore.main_EncodeCRLF(Copy) & BR
                        End If
                        '
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Content on which the comments are based</b>" & BR
                        '
                        CS = cpcore.db.cs_open(iContentName, "ID=" & iRecordID)
                        Copy = "[the content of this page is not available]" & BR
                        If cpcore.db.cs_ok(CS) Then
                            Copy = (cpcore.db.cs_get(CS, "copyFilename"))
                            'Copy = main_EncodeContent5(Copy, user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        End If
                        NoteCopy = NoteCopy & Copy & BR
                        Call cpcore.db.cs_Close(CS)
                        '
                        Call cpcore.email_sendMemberEmail3(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, False, True, 0, "", False)
                        '
                        ' ----- Note sent, say thanks
                        '
                        main_GetFeedbackForm = main_GetFeedbackForm & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & cpcore.webServerIO_ServerFormActionURL & "?" & cpcore.web_RefreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = cpcore.user.name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & cpcore.html.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = cpcore.user.email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & cpcore.html.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & cpcore.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
                        'Panel = Panel & "<td><textarea ID=""TextArea"" rows=""4"" cols=""40"" name=""NoteCopy"">" & Copy & "</textarea></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- submit button
                        '
                        Panel = Panel & "<td>&nbsp;</td>"
                        Panel = Panel & "<td><input type=""submit"" name=""fbb"" value=""" & FeedbackButtonSubmit & """></td>"
                        Panel = Panel & "</tr></table>"
                        Panel = Panel & "</form>"
                        '
                        result = Panel
                End Select
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Shared Function main_OpenCSWhatsNew(cpCore As coreClass, Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim result As Integer = -1
            Try
                result = main_OpenCSContentWatchList(cpCore, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Open a content set with the current whats new list
        '========================================================================
        '
        Public Shared Function main_OpenCSContentWatchList(cpcore As coreClass, ByVal ListName As String, Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim result As Integer = -1
            Try
                Dim SQL As String
                Dim iSortFieldList As String
                Dim MethodName As String
                Dim CS As Integer
                '
                iSortFieldList = Trim(encodeEmptyText(SortFieldList, ""))
                'iSortFieldList = encodeMissingText(SortFieldList, "DateAdded")
                If iSortFieldList = "" Then
                    iSortFieldList = "DateAdded"
                End If
                '
                MethodName = "main_OpenCSContentWatchList( " & ListName & ", " & iSortFieldList & ", " & ActiveOnly & " )"
                '
                ' ----- Add tablename to the front of SortFieldList fieldnames
                '
                iSortFieldList = " " & genericController.vbReplace(iSortFieldList, ",", " , ") & " "
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ID ", " ccContentWatch.ID ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " Link ", " ccContentWatch.Link ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " SortOrder ", " ccContentWatch.SortOrder ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " DateAdded ", " ccContentWatch.DateAdded ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ContentID ", " ccContentWatch.ContentID ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " RecordID ", " ccContentWatch.RecordID ", 1, 99, vbTextCompare)
                iSortFieldList = genericController.vbReplace(iSortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ", 1, 99, vbTextCompare)
                '
                ' ----- Special case
                '
                iSortFieldList = genericController.vbReplace(iSortFieldList, " name ", " ccContentWatch.LinkLabel ", 1, 99, vbTextCompare)
                '
                SQL = "SELECT" _
                    & " ccContentWatch.ID AS ID" _
                    & ",ccContentWatch.Link as Link" _
                    & ",ccContentWatch.LinkLabel as LinkLabel" _
                    & ",ccContentWatch.SortOrder as SortOrder" _
                    & ",ccContentWatch.DateAdded as DateAdded" _
                    & ",ccContentWatch.ContentID as ContentID" _
                    & ",ccContentWatch.RecordID as RecordID" _
                    & ",ccContentWatch.ModifiedDate as ModifiedDate" _
                & " FROM (ccContentWatchLists" _
                    & " LEFT JOIN ccContentWatchListRules ON ccContentWatchLists.ID = ccContentWatchListRules.ContentWatchListID)" _
                    & " LEFT JOIN ccContentWatch ON ccContentWatchListRules.ContentWatchID = ccContentWatch.ID" _
                & " WHERE (((ccContentWatchLists.Name)=" & cpcore.db.encodeSQLText(ListName) & ")" _
                    & "AND ((ccContentWatchLists.Active)<>0)" _
                    & "AND ((ccContentWatchListRules.Active)<>0)" _
                    & "AND ((ccContentWatch.Active)<>0)" _
                    & "AND (ccContentWatch.Link is not null)" _
                    & "AND (ccContentWatch.LinkLabel is not null)" _
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & cpcore.db.encodeSQLDate(cpcore.app_startTime) & "))" _
                    & ")" _
                & " ORDER BY " & iSortFieldList & ";"
                result = cpcore.db.cs_openSql(SQL, , PageSize, PageNumber)
                If Not cpcore.db.cs_ok(result) Then
                    '
                    ' Check if listname exists
                    '
                    CS = cpcore.db.cs_open("Content Watch Lists", "name=" & cpcore.db.encodeSQLText(ListName), "ID", , , , , "ID")
                    If Not cpcore.db.cs_ok(CS) Then
                        Call cpcore.db.cs_Close(CS)
                        CS = cpcore.db.cs_insertRecord("Content Watch Lists")
                        If cpcore.db.cs_ok(CS) Then
                            Call cpcore.db.cs_set(CS, "name", ListName)
                        End If
                    End If
                    Call cpcore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Print Whats New
        '   Prints a linked list of new content
        '========================================================================
        '
        Public Shared Function main_GetWhatsNew(cpcore As coreClass, Optional ByVal SortFieldList As String = "") As String
            Dim result As String = ""
            Try
                Dim CSPointer As Integer
                Dim ContentID As Integer
                Dim RecordID As Integer
                Dim LinkLabel As String
                Dim Link As String
                '
                CSPointer = main_OpenCSWhatsNew(cpcore, SortFieldList)
                '
                If cpcore.db.cs_ok(CSPointer) Then
                    ContentID = cpcore.main_GetContentID("Content Watch")
                    Do While cpcore.db.cs_ok(CSPointer)
                        Link = cpcore.db.cs_getText(CSPointer, "link")
                        LinkLabel = cpcore.db.cs_getText(CSPointer, "LinkLabel")
                        RecordID = cpcore.db.cs_getInteger(CSPointer, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & cpcore.csv_GetLinkedText("<a href=""" & cpcore.html.html_EncodeHTML(cpcore.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID) & """>", LinkLabel)
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call cpcore.db.cs_goNext(CSPointer)
                    Loop
                    result = cr & "<ul class=""ccWatchList"">" & kmaIndent(result) & cr & "</ul>"
                End If
                Call cpcore.db.cs_Close(CSPointer)
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        '
        Public Shared Function pageManager_getMoreInfoHtml(cpCore As coreClass, ByVal PeopleID As Integer) As String
            Dim result As String = ""
            Try
                Dim CS As Integer
                Dim ContactName As String
                Dim ContactPhone As String
                Dim ContactEmail As String
                Dim Copy As String
                '
                Copy = ""
                CS = cpCore.db.cs_openContentRecord("People", PeopleID, , , , "Name,Phone,Email")
                If cpCore.db.cs_ok(CS) Then
                    ContactName = (cpCore.db.cs_getText(CS, "Name"))
                    ContactPhone = (cpCore.db.cs_getText(CS, "Phone"))
                    ContactEmail = (cpCore.db.cs_getText(CS, "Email"))
                    If ContactName <> "" Then
                        Copy = Copy & "For more information, please contact " & ContactName
                        If ContactEmail = "" Then
                            If ContactPhone <> "" Then
                                Copy = Copy & " by phone at " & ContactPhone
                            End If
                        Else
                            Copy = Copy & " by <A href=""mailto:" & ContactEmail & """>email</A>"
                            If ContactPhone <> "" Then
                                Copy = Copy & " or by phone at " & ContactPhone
                            End If
                        End If
                        Copy = Copy
                    Else
                        If ContactEmail = "" Then
                            If ContactPhone <> "" Then
                                Copy = Copy & "For more information, please call " & ContactPhone
                            End If
                        Else
                            Copy = Copy & "For more information, please <A href=""mailto:" & ContactEmail & """>email</A>"
                            If ContactPhone <> "" Then
                                Copy = Copy & ", or call " & ContactPhone
                            End If
                            Copy = Copy
                        End If
                    End If
                End If
                Call cpCore.db.cs_Close(CS)
                '
                result = Copy
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Public Shared Function main_GetWatchList(cpCore As coreClass, ListName As String, SortField As String, SortReverse As Boolean) As String
            Dim result As String = ""
            Try
                Dim CS As Integer
                Dim ContentID As Integer
                Dim RecordID As Integer
                Dim Link As String
                Dim LinkLabel As String
                '
                If SortReverse And (SortField <> "") Then
                    CS = main_OpenCSContentWatchList(cpCore, ListName, SortField & " Desc", True)
                Else
                    CS = main_OpenCSContentWatchList(cpCore, ListName, SortField, True)
                End If
                '
                If cpCore.db.cs_ok(CS) Then
                    ContentID = cpCore.main_GetContentID("Content Watch")
                    Do While cpCore.db.cs_ok(CS)
                        Link = cpCore.db.cs_getText(CS, "link")
                        LinkLabel = cpCore.db.cs_getText(CS, "LinkLabel")
                        RecordID = cpCore.db.cs_getInteger(CS, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li id=""main_ContentWatch" & RecordID & """ class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & "<a href=""http://" & cpCore.webServerIO_requestDomain & requestAppRootPath & cpCore.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID & """>" & LinkLabel & "</a>"
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call cpCore.db.cs_goNext(CS)
                    Loop
                    If result <> "" Then
                        result = cpCore.html_GetContentCopy("Watch List Caption: " & ListName, ListName, cpCore.user.id, True, cpCore.user.isAuthenticated) & cr & "<ul class=""ccWatchList"">" & kmaIndent(result) & cr & "</ul>"
                    End If
                End If
                Call cpCore.db.cs_Close(CS)
                '
                If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                    result = cpCore.main_GetEditWrapper("Watch List [" & ListName & "]", result)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
