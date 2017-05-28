
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Core.Controllers
    '
    Public Class pageManagerController
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        ' -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
        Private c As coreClass
        '
        Public pageManager_SiteStructure As String = ""
        Public pageManager_SiteStructure_LocalLoaded As Boolean = False
        Public Const pageManager_NavStruc_Descriptor = 1           ' Descriptors:0 = RootPage, 1 = Parent Page, 2 = Current Page, 3 = Child Page
        Public Const pageManager_NavStruc_Descriptor_CurrentPage = 2
        Public Const pageManager_NavStruc_Descriptor_ChildPage = 3
        Public Const pageManager_NavStruc_TemplateId = 7
        Public pageManager_PageContent As String = ""                    ' The page's content at the OnPageEndEvent - used to let OnPageStart and OnPageEnd Add-ons change the content
        Public pageManager_ContentPageStructure As String = ""           ' deprecated name for main_RenderedNavigationStructure
        Public pageManager_LandingPageID As Integer = 0                              ' Set from Site Property (use main_GetLandingPageID)
        Public pageManager_LandingPageID_Loaded As Boolean = False                   '   true when pageManager_LandingPageID is loaded
        Public pageManager_LandingPageName As String = ""                          ' Set from pageManager_LandingPageID (use main_GetLandingPageName)
        Public pageManager_LandingLink As String = ""                              ' Default Landing page - managed through main_GetLandingLink()
        Public pageManager_TemplateReason As String = ""                           ' a message that explains why this template was selected
        Public pageManager_TemplateName As String = ""                             ' Name of the template
        Public pageManager_TemplateLink As String = ""                             ' Link this template requires - redirect caused if this is not the current link
        Public pageManager_TemplateBody As String = ""                             ' Body field of the TemplateID
        Public pageManager_TemplateBodyTag As String = ""                          ' BodyTag - from Template record, if blank, use TemplateDefaultBodyTag
        Public pageManager_SectionMenuLinkDefault As String = ""
        Public Const pageManager_BlockMessageDefault = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
        Public pageManager_RedirectLink As String = ""                             ' If there is a problem, a redirect is forced
        Public pageManager_RedirectReason As String = ""                           ' reason for redirect
        Public pageManager_RedirectBecausePageNotFound As Boolean = False            ' if true, redirect will go through a 404 so bots will not follow
        Public pageManager_RedirectSourcePageID As Integer = 0                       ' the pageid of the page with this issue, 0 if not a page problem
        '
        Public main_RenderedPageID As Integer = 0                   ' The current page's id
        Public main_RenderedPageName As String = ""               ' The current page's name
        Public main_RenderedSectionID As Integer = 0                ' The current page's section id
        Public main_RenderedSectionName As String = ""            ' The current Section's name
        Public main_RenderedTemplateID As Integer = 0               ' The current template's Id
        Public main_RenderedTemplateName As String = ""           ' The current template's name
        Public main_RenderedNavigationStructure As String = ""    ' Public string describing the current page
        Public main_RenderedParentID As Integer = 0               '
        Public main_RenderCache_Loaded As Boolean = False                               ' true after main_loadRenderCache
        Public main_RenderCache_CurrentPage_PCCPtr As Integer = 0
        Public main_RenderCache_CurrentPage_ContentId As Integer = 0                        '
        Public main_RenderCache_CurrentPage_ContentName As String = ""                   ' set during LoadContent_CurrentPage
        Public main_RenderCache_CurrentPage_IsRenderingMode As Boolean = False             ' true if tools panel rendering is on
        Public main_RenderCache_CurrentPage_IsQuickEditing As Boolean = False              ' true if tools panel is on, and user can author current page
        Public main_RenderCache_CurrentPage_IsEditing As Boolean = False                   ' true if tools panel is on
        Public main_RenderCache_CurrentPage_IsAuthoring As Boolean = False                ' true if either editing or quickediting
        Public main_RenderCache_CurrentPage_IsRootPage As Boolean = False                  ' true after LoadContent if the current page is the root page
        Public main_RenderCache_ParentBranch_PCCPtrCnt As Integer = 0
        Public main_RenderCache_ParentBranch_PCCPtrs As Integer()
        Public main_RenderCache_ChildBranch_PCCPtrCnt As Integer = 0
        Public main_RenderCache_ChildBranch_PCCPtrs As Integer()
        Private Const main_BlockSourceDefaultMessage = 0
        Private Const main_BlockSourceCustomMessage = 1
        Private Const main_BlockSourceLogin = 2
        Private Const main_BlockSourceRegistration = 3
        Private Const main_FieldDelimiter = " , "
        Private Const main_LineDelimiter = " ,, "
        Private Const main_IPosType = 0
        Private Const main_IPosCaption = 1
        Private Const main_IPosRequired = 2
        Private Const main_IPosMax = 2       ' value checked for the line before decoding
        Private Const main_IPosPeopleField = 3
        Private Const main_IPosGroupName = 3
        '
        Private Structure main_FormPagetype_InstType
            Dim Type As Integer
            Dim Caption As String
            Dim REquired As Boolean
            Dim PeopleField As String
            Dim GroupName As String
        End Structure
        '
        Private Structure main_FormPagetype
            Dim PreRepeat As String
            Dim PostRepeat As String
            Dim RepeatCell As String
            Dim AddGroupNameList As String
            Dim AuthenticateOnFormProcess As Boolean
            Dim Inst() As main_FormPagetype_InstType
        End Structure
        '
        ' ----------------------------------------------------------------------------------------
        '   values collected from add-ons as the page progresses
        ' ----------------------------------------------------------------------------------------
        '
        ' ----- Site Section Cache
        '
        Public Const pageManager_cache_siteSection_cacheName = "cache_siteSection"
        Public cache_siteSection As String(,)
        Public pageManager_cache_siteSection_rows As Integer = 0
        Public pageManager_cache_siteSection_IDIndex As coreKeyPtrIndexClass
        Public pageManager_cache_siteSection_RootPageIDIndex As coreKeyPtrIndexClass
        Public pageManager_cache_siteSection_NameIndex As coreKeyPtrIndexClass
        '
        ' ----- Template Content store
        '
        Public Const pageManager_cache_pageTemplate_cacheName = "cache_pageTemplate"
        Public cache_pageTemplate As String(,)
        Public pageManager_cache_pageTemplate_rows As Integer = 0
        Public pageManager_cache_pageTemplate_contentIdindex As coreKeyPtrIndexClass
        '
        ' ----- Page Content store (old names for compatibility)
        '
        'Public main_pccData As Object
        Public _pageManager_cache_pageContent_rows As Integer = 0
        Public cache_pageContent As String(,)
        Public _pageManager_cache_pageContent_needsReload As Boolean = False
        Public pageManager_cache_pageContent_idIndex As coreKeyPtrIndexClass
        Public pageManager_cache_pageContent_parentIdIndex As coreKeyPtrIndexClass
        Public pageManager_cache_pageContent_nameIndex As coreKeyPtrIndexClass
        '
        '====================================================================================================
        ''' <summary>
        ''' this will eventuall be an addon, but lets do this first to keep the converstion complexity down
        ''' </summary>
        ''' <param name="cpCore"></param>

        Public Sub New(cpCore As coreClass)
            Me.c = cpCore
        End Sub
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
        Public Function main_GetSeeAlso(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer) As String
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
                    ContentID = c.main_GetContentID(iContentName)
                    If (ContentID > 0) Then
                        '
                        ' ----- Set authoring only for valid ContentName
                        '
                        IsEditingLocal = c.authContext.isEditing(cpcore, iContentName)
                    Else
                        '
                        ' ----- if iContentName was bad, maybe they put table in, no authoring
                        '
                        ContentID = c.GetContentIDByTablename(iContentName)
                    End If
                    If (ContentID > 0) Then
                        '
                        CS = c.db.cs_open("See Also", "((active<>0)AND(ContentID=" & ContentID & ")AND(RecordID=" & iRecordID & "))")
                        Do While (cpcore.db.cs_ok(CS))
                            SeeAlsoLink = (cpcore.db.cs_getText(CS, "Link"))
                            If SeeAlsoLink <> "" Then
                                result = result & cr & "<li class=""ccListItem"">"
                                If genericController.vbInstr(1, SeeAlsoLink, "://") = 0 Then
                                    SeeAlsoLink = c.webServer.webServerIO_requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & c.main_GetRecordEditLink2("See Also", (cpcore.db.cs_getInteger(CS, "ID")), False, "", c.authContext.isEditing(cpcore, "See Also"))
                                End If
                                result = result & "<a href=""" & c.htmlDoc.html_EncodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.cs_getText(CS, "Name")) & "</A>"
                                Copy = (cpcore.db.cs_getText(CS, "Brief"))
                                If Copy <> "" Then
                                    result = result & "<br >" & AddSpan(Copy, "ccListCopy")
                                End If
                                SeeAlsoCount = SeeAlsoCount + 1
                                result = result & "</li>"
                            End If
                            c.db.cs_goNext(CS)
                        Loop
                        c.db.cs_Close(CS)
                        '
                        If IsEditingLocal Then
                            SeeAlsoCount = SeeAlsoCount + 1
                            result = result & cr & "<li class=""ccListItem"">" & c.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
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
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Print the "for more information, please contact" line
        '
        '========================================================================
        '
        Public Function main_GetMoreInfo(cpcore As coreClass, ByVal contactMemberID As Integer) As String
            Dim result As String = ""
            Try
                main_GetMoreInfo = pageManager_getMoreInfoHtml(cpcore, genericController.EncodeInteger(contactMemberID))
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
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
        Public Function main_GetFeedbackForm(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, ByVal ToMemberID As Integer, Optional ByVal headline As String = "") As String
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
                FeedbackButton = c.docProperties.getText("fbb")
                Select Case FeedbackButton
                    Case FeedbackButtonSubmit
                        '
                        ' ----- form was submitted, save the note, send it and say thanks
                        '
                        NoteFromName = c.docProperties.getText("NoteFromName")
                        NoteFromEmail = c.docProperties.getText("NoteFromEmail")
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
                        Copy = c.docProperties.getText("NoteCopy")
                        If (Copy = "") Then
                            NoteCopy = NoteCopy & "[no comments entered]" & BR
                        Else
                            NoteCopy = NoteCopy & c.htmlDoc.main_EncodeCRLF(Copy) & BR
                        End If
                        '
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Content on which the comments are based</b>" & BR
                        '
                        CS = c.db.cs_open(iContentName, "ID=" & iRecordID)
                        Copy = "[the content of this page is not available]" & BR
                        If c.db.cs_ok(CS) Then
                            Copy = (cpcore.db.cs_get(CS, "copyFilename"))
                            'Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        End If
                        NoteCopy = NoteCopy & Copy & BR
                        Call c.db.cs_Close(CS)
                        '
                        Call c.email.sendPerson(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, False, True, 0, "", False)
                        '
                        ' ----- Note sent, say thanks
                        '
                        main_GetFeedbackForm = main_GetFeedbackForm & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & c.webServer.webServerIO_ServerFormActionURL & "?" & c.web_RefreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = c.authContext.user.Name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & c.htmlDoc.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = c.authContext.user.Email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & c.htmlDoc.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & c.htmlDoc.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
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
                c.handleExceptionAndContinue(ex)
            End Try
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_OpenCSWhatsNew(cpCore As coreClass, Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As Integer
            Dim result As Integer = -1
            Try
                result = main_OpenCSContentWatchList(cpCore, "What's New", SortFieldList, ActiveOnly, PageSize, PageNumber)
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Open a content set with the current whats new list
        '========================================================================
        '
        Public Function main_OpenCSContentWatchList(cpcore As coreClass, ByVal ListName As String, Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As Integer
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
                & " WHERE (((ccContentWatchLists.Name)=" & c.db.encodeSQLText(ListName) & ")" _
                    & "AND ((ccContentWatchLists.Active)<>0)" _
                    & "AND ((ccContentWatchListRules.Active)<>0)" _
                    & "AND ((ccContentWatch.Active)<>0)" _
                    & "AND (ccContentWatch.Link is not null)" _
                    & "AND (ccContentWatch.LinkLabel is not null)" _
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & c.db.encodeSQLDate(cpcore.app_startTime) & "))" _
                    & ")" _
                & " ORDER BY " & iSortFieldList & ";"
                result = c.db.cs_openSql(SQL, , PageSize, PageNumber)
                If Not c.db.cs_ok(result) Then
                    '
                    ' Check if listname exists
                    '
                    CS = c.db.cs_open("Content Watch Lists", "name=" & c.db.encodeSQLText(ListName), "ID", , , , , "ID")
                    If Not c.db.cs_ok(CS) Then
                        Call c.db.cs_Close(CS)
                        CS = c.db.cs_insertRecord("Content Watch Lists")
                        If c.db.cs_ok(CS) Then
                            Call c.db.cs_set(CS, "name", ListName)
                        End If
                    End If
                    Call c.db.cs_Close(CS)
                End If
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        ' Print Whats New
        '   Prints a linked list of new content
        '========================================================================
        '
        Public Function main_GetWhatsNew(cpcore As coreClass, Optional ByVal SortFieldList As String = "") As String
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
                If c.db.cs_ok(CSPointer) Then
                    ContentID = c.main_GetContentID("Content Watch")
                    Do While c.db.cs_ok(CSPointer)
                        Link = c.db.cs_getText(CSPointer, "link")
                        LinkLabel = c.db.cs_getText(CSPointer, "LinkLabel")
                        RecordID = c.db.cs_getInteger(CSPointer, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & c.csv_GetLinkedText("<a href=""" & c.htmlDoc.html_EncodeHTML(cpcore.webServer.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID) & """>", LinkLabel)
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call c.db.cs_goNext(CSPointer)
                    Loop
                    result = cr & "<ul class=""ccWatchList"">" & kmaIndent(result) & cr & "</ul>"
                End If
                Call c.db.cs_Close(CSPointer)
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        '
        Public Function pageManager_getMoreInfoHtml(cpCore As coreClass, ByVal PeopleID As Integer) As String
            Dim result As String = ""
            Try
                Dim CS As Integer
                Dim ContactName As String
                Dim ContactPhone As String
                Dim ContactEmail As String
                Dim Copy As String
                '
                Copy = ""
                CS = c.db.cs_openContentRecord("People", PeopleID, , , , "Name,Phone,Email")
                If c.db.cs_ok(CS) Then
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
                Call c.db.cs_Close(CS)
                '
                result = Copy
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Public Function main_GetWatchList(cpCore As coreClass, ListName As String, SortField As String, SortReverse As Boolean) As String
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
                If c.db.cs_ok(CS) Then
                    ContentID = c.main_GetContentID("Content Watch")
                    Do While c.db.cs_ok(CS)
                        Link = c.db.cs_getText(CS, "link")
                        LinkLabel = c.db.cs_getText(CS, "LinkLabel")
                        RecordID = c.db.cs_getInteger(CS, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li id=""main_ContentWatch" & RecordID & """ class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & "<a href=""http://" & c.webServer.webServerIO_requestDomain & requestAppRootPath & c.webServer.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID & """>" & LinkLabel & "</a>"
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call c.db.cs_goNext(CS)
                    Loop
                    If result <> "" Then
                        result = c.htmlDoc.html_GetContentCopy("Watch List Caption: " & ListName, ListName, c.authContext.user.ID, True, c.authContext.isAuthenticated) & cr & "<ul class=""ccWatchList"">" & kmaIndent(result) & cr & "</ul>"
                    End If
                End If
                Call c.db.cs_Close(CS)
                '
                If c.visitProperty.getBoolean("AllowAdvancedEditor") Then
                    result = c.htmlDoc.main_GetEditWrapper("Watch List [" & ListName & "]", result)
                End If
            Catch ex As Exception
                c.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        '
        '==========================================================================
        '   returns the site structure xml
        '==========================================================================
        '
        Public ReadOnly Property main_SiteStructure(cpcore As coreClass) As String
            Get
                Dim returnStatus As Boolean = False
                If Not pageManager_SiteStructure_LocalLoaded Then
                    pageManager_SiteStructure = c.addon.execute_legacy2(0, addonSiteStructureGuid, "", CPUtilsBaseClass.addonContext.ContextSimple, "", 0, "", "", False, -1, "", returnStatus, Nothing)
                    pageManager_SiteStructure_LocalLoaded = True
                End If
                main_SiteStructure = pageManager_SiteStructure

            End Get
        End Property
        '
        '========================================================================
        '   Returns the HTML body
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Function pageManager_GetHtmlBody() As String
            Dim returnHtmlBody As String = ""
            Try
                '
                Dim AddonReturn As String
                Dim Ptr As Integer
                Dim Cnt As Integer
                Dim layoutError As String
                Dim FilterStatusOK As Boolean
                Dim BlockFormatting As Boolean
                Dim IndentCnt As Integer
                Dim Result As New coreFastStringClass
                Dim Content As String
                Dim ContentIndent As String
                Dim ContentCnt As Integer
                Dim PageContent As String
                Dim Stream As New coreFastStringClass
                Dim LocalTemplateID As Integer
                Dim LocalTemplateName As String
                Dim LocalTemplateBody As String
                Dim Parse As coreHtmlParseClass
                Dim blockSiteWithLogin As Boolean
                Dim addonCachePtr As Integer
                Dim addonId As Integer
                Dim AddonName As String
                '
                Call c.addonCache.load()
                returnHtmlBody = ""
                '
                ' ----- OnBodyStart add-ons
                '
                FilterStatusOK = False
                Cnt = UBound(c.addonCache.addonCache.onBodyStartPtrs) + 1
                For Ptr = 0 To Cnt - 1
                    addonCachePtr = c.addonCache.addonCache.onBodyStartPtrs(Ptr)
                    If addonCachePtr > -1 Then
                        addonId = c.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                        'hint = hint & ",addonId=" & addonId
                        If addonId > 0 Then
                            AddonName = c.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                            'hint = hint & ",AddonName=" & AddonName
                            returnHtmlBody = returnHtmlBody & c.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextOnBodyStart, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                            If Not FilterStatusOK Then
                                Call c.handleLegacyError12("pageManager_GetHtmlBody", "There was an error processing OnAfterBody [" & c.addonCache.addonCache.addonList(addonCachePtr.ToString).name & "]. Filtering was aborted.")
                                Exit For
                            End If
                        End If
                    End If
                Next
                '
                ' ----- main_Get Content (Already Encoded)
                '
                blockSiteWithLogin = False
                PageContent = pageManager_GetHtmlBody_GetSection(True, True, False, blockSiteWithLogin)
                If blockSiteWithLogin Then
                    '
                    ' section blocked, just return the login box in the page content
                    '
                    returnHtmlBody = "" _
                        & cr & "<div class=""ccLoginPageCon"">" _
                        & genericController.kmaIndent(PageContent) _
                        & cr & "</div>" _
                        & ""
                ElseIf Not c.docOpen Then
                    '
                    ' exit if stream closed during main_GetSectionpage
                    '
                    returnHtmlBody = ""
                Else
                    '
                    ' no section block, continue
                    '
                    'PageContent = CR & "<!-- Page Content -->" & genericController.kmaIndent(pageManager_GetHtmlBody_GetSection(True, True, False)) & CR & "<!-- /Page Content -->"

                    Call pageManager_LoadTemplateGetID(main_RenderedTemplateID)
                    '
                    ' ----- main_Get Template
                    '
                    LocalTemplateID = main_RenderedTemplateID
                    LocalTemplateBody = pageManager_TemplateBody
                    If LocalTemplateBody = "" Then
                        LocalTemplateBody = TemplateDefaultBody
                    End If
                    LocalTemplateName = pageManager_TemplateName
                    If LocalTemplateName = "" Then
                        LocalTemplateName = "Template " & LocalTemplateID
                    End If
                    '
                    ' ----- Encode Template
                    '
                    If Not c.htmlDoc.pageManager_printVersion Then
                        LocalTemplateBody = c.htmlDoc.html_executeContentCommands(Nothing, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
                        returnHtmlBody = returnHtmlBody & c.htmlDoc.html_encodeContent9(LocalTemplateBody, c.authContext.user.ID, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain, False, c.siteProperties.defaultWrapperID, PageContent, CPUtilsBaseClass.addonContext.ContextTemplate)
                        'returnHtmlBody = returnHtmlBody & EncodeContent8(LocalTemplateBody, memberID, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", main_ServerProtocol, False, app.SiteProperty_DefaultWrapperID, PageContent, ContextTemplate)
                    End If
                    '
                    ' If Content was not found, add it to the end
                    '
                    If (InStr(1, returnHtmlBody, fpoContentBox) <> 0) Then
                        returnHtmlBody = genericController.vbReplace(returnHtmlBody, fpoContentBox, PageContent)
                    Else
                        returnHtmlBody = returnHtmlBody & PageContent
                    End If
                    '
                    ' ----- Add tools Panel
                    '
                    If Not c.authContext.isAuthenticated() Then
                        '
                        ' not logged in
                        '
                    Else
                        '
                        ' Add template editing
                        '
                        If c.visitProperty.getBoolean("AllowAdvancedEditor") And c.authContext.isEditing(c, "Page Templates") Then
                            returnHtmlBody = c.htmlDoc.main_GetEditWrapper("Page Template [" & LocalTemplateName & "]", c.main_GetRecordEditLink2("Page Templates", LocalTemplateID, False, LocalTemplateName, c.authContext.isEditing(c, "Page Templates")) & returnHtmlBody)
                        End If
                    End If
                    '
                    ' ----- OnBodyEnd add-ons
                    '
                    'hint = hint & ",onBodyEnd"
                    FilterStatusOK = False
                    Cnt = UBound(c.addonCache.addonCache.onBodyEndPtrs) + 1
                    'hint = hint & ",cnt=" & Cnt
                    For Ptr = 0 To Cnt - 1
                        addonCachePtr = c.addonCache.addonCache.onBodyEndPtrs(Ptr)
                        'hint = hint & ",ptr=" & Ptr & ",addonCachePtr=" & addonCachePtr
                        If addonCachePtr > -1 Then
                            addonId = c.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                            'hint = hint & ",addonId=" & addonId
                            If addonId > 0 Then
                                AddonName = c.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                                'hint = hint & ",AddonName=" & AddonName
                                c.htmlDoc.html_DocBodyFilter = returnHtmlBody
                                AddonReturn = c.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                                returnHtmlBody = c.htmlDoc.html_DocBodyFilter & AddonReturn
                                If Not FilterStatusOK Then
                                    Call c.handleLegacyError12("pageManager_GetHtmlBody", "There was an error processing OnBodyEnd for [" & AddonName & "]. Filtering was aborted.")
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    '
                    ' Make it pretty for those who care
                    '
                    If c.siteProperties.getBoolean("AutoHTMLFormatting") Then
                        IndentCnt = 0
                        Parse = New coreHtmlParseClass(c)
                        Call Parse.Load(returnHtmlBody)
                        If Parse.ElementCount > 0 Then
                            For Ptr = 0 To Parse.ElementCount - 1
                                If Not Parse.IsTag(Ptr) Then
                                    Content = Parse.Text(Ptr)
                                    If BlockFormatting Then
                                        Result.Add(Content)
                                    Else
                                        If Content <> "" Then
                                            If Trim(Content) <> "" Then
                                                Result.Add(ContentIndent)
                                                ContentIndent = ""
                                            End If
                                            Content = genericController.vbReplace(Content, vbCrLf, " ")
                                            Content = genericController.vbReplace(Content, vbTab, " ")
                                            Content = genericController.vbReplace(Content, vbCr, " ")
                                            Content = genericController.vbReplace(Content, vbLf, " ")
                                            Result.Add(Content)
                                            ContentCnt = ContentCnt + 1
                                        End If
                                    End If
                                Else
                                    Select Case genericController.vbLCase(Parse.TagName(Ptr))
                                        Case "pre", "script"
                                            '
                                            ' End block formating
                                            '
                                            Result.Add(vbCrLf & Parse.Text(Ptr))
                                            BlockFormatting = True
                                        Case "/pre", "/script"
                                            '
                                            ' end block formating
                                            '
                                            Result.Add(Parse.Text(Ptr) & vbCrLf)
                                            BlockFormatting = False
                                        Case Else
                                            If BlockFormatting Then
                                                '
                                                ' formatting is blocked
                                                '
                                                Result.Add(Parse.Text(Ptr))
                                            Else
                                                '
                                                ' format the tag
                                                '
                                                Select Case genericController.vbLCase(Parse.TagName(Ptr))
                                                    Case "p", "h1", "h2", "h3", "h4", "h5", "h6", "li", "br"
                                                        '
                                                        ' new line
                                                        '
                                                        Result.Add(vbCrLf & New String(CChar(vbTab), IndentCnt) & Parse.Text(Ptr))
                                                    Case "div", "td", "table", "tr", "tbody", "ol", "ul", "form"
                                                        '
                                                        ' new line and +indent
                                                        '
                                                        Result.Add(vbCrLf & New String(CChar(vbTab), IndentCnt) & Parse.Text(Ptr))
                                                        IndentCnt = IndentCnt + 1
                                                        ContentIndent = vbCrLf & New String(CChar(vbTab), IndentCnt)
                                                        ContentCnt = 0
                                                    Case "/div", "/td", "/table", "/tr", "/tbody", "/ol", "/ul", "/form"
                                                        '
                                                        ' new line and -indent
                                                        '
                                                        IndentCnt = IndentCnt - 1
                                                        If IndentCnt < 0 Then
                                                            IndentCnt = 0
                                                            '
                                                            ' Add to 'Asset Errors' Table - a merge with Spider Doc Errors
                                                            '
                                                        End If
                                                        If ContentCnt = 0 Then
                                                            Result.Add(Parse.Text(Ptr))
                                                        Else
                                                            Result.Add(vbCrLf & New String(CChar(vbTab), IndentCnt) & Parse.Text(Ptr))
                                                        End If
                                                        ContentCnt = ContentCnt + 1
                                                    Case Else
                                                        '
                                                        ' tag that acts like content
                                                        '
                                                        Content = Parse.Text(Ptr)
                                                        If Content <> "" Then
                                                            Result.Add(ContentIndent & Content)
                                                            ContentIndent = ""
                                                        End If
                                                        ContentCnt = ContentCnt + 1
                                                End Select
                                            End If
                                    End Select
                                End If
                            Next
                            If IndentCnt <> 0 Then
                                '
                                ' Add to 'Asset Errors' Table - a merge with Spider Doc Errors
                                '
                                'Call main_AppendClassErrorLog("cpCoreClass(" & appEnvironment.name & ").GetHTMLBody AutoIndent error. At the end of the document, the last tag was still indented (more start tags than end tags). Link=[" & genericController.decodeHtml(main_ServerLink) & "], ")
                            End If
                            returnHtmlBody = Result.Text
                        End If
                    End If
                End If
                '
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return returnHtmlBody
        End Function
        '
        '=============================================================================
        '   main_Get Section
        '       Two modes
        '           pre 3.3.613 - SectionName = RootPageName
        '           else - (IsSectionRootPageIDMode) SectionRecord has a RootPageID field
        '=============================================================================
        '
        Public Function pageManager_GetHtmlBody_GetSection(AllowChildPageList As Boolean, AllowReturnLink As Boolean, AllowEditWrapper As Boolean, ByRef return_blockSiteWithLogin As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection")
            '
            Dim returnHtml As String
            '
            Dim UseContentWatchLink As Boolean
            UseContentWatchLink = c.siteProperties.useContentWatchLink
            Dim test As String
            Dim Copy As String
            Dim allowPageWithoutSectionDislay As Boolean
            Dim pageContentControlId As Integer
            Dim pageContentName As String
            Dim domainIds() As String
            Dim setdomainId As Integer
            Dim linkDomain As String
            Dim templatedomainIdList As String
            Dim FieldRows As Integer
            Dim PCCPtr As Integer
            '
            Dim SecureLink_CurrentURL As Boolean                ' the current page starts https://
            Dim SecureLink_Template_Required As Boolean         ' the template record has 'secure' checked
            Dim SecureLink_Page_Required As Boolean             ' teh page record has 'secure' checked
            Dim SecureLink_Required As Boolean                  ' either the template or the page have secure checked
            '
            Dim templateLink As String                          ' the template record 'link' field
            '
            Dim TCPtr As Integer
            Dim hint As String
            Dim JSFilename As String
            Dim StylesFilename As String
            Dim SharedStylesIDList As String
            Dim ListSplit() As String
            Dim styleId As Integer
            Dim CSPage As Integer
            Dim CSTemplates As Integer
            Dim templateId As Integer
            Dim ParentBranchPointer As Integer
            Dim LinkQueryString As String
            Dim CurrentLink As String
            Dim CurrentLinkNoQuery As String
            Dim LinkSplit() As String
            Dim LandingLink As String
            Dim CS As Integer
            Dim CSSection As Integer
            Dim rootPageId As Integer
            Dim RootPageName As String
            Dim RootPageContentName As String
            Dim SectionContentID As Integer
            Dim SectionTemplateID As Integer
            Dim SectionBlock As Boolean
            Dim PageContentCID As Integer
            Dim PageRow() As String
            Dim PageRowCnt As Integer
            Dim Ptr As Integer
            Dim navStruc() As String
            Dim SectionFieldList As String
            Dim SectionCriteria As String
            Dim SectionName As String
            Dim IsSectionRootPageIDMode As Boolean
            Dim PageID As Integer
            Dim SectionID As Integer
            '
            '
            '
            ' ----- Determine LandingLink
            '
            'hint = "1"
            LandingLink = pageManager_GetLandingLink()
            IsSectionRootPageIDMode = True
            '
            ' Add in JS filename
            '
            SectionFieldList = "ID,Name,ContentID,TemplateID,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody,JSFilename"
            '
            '------------------------------------------------------------------------------------
            ' get page and section requests
            '------------------------------------------------------------------------------------
            '
            ' Handle PageID Request Variable
            '
            PageID = c.docProperties.getInteger("bid")
            If PageID <> 0 Then
                Call c.htmlDoc.webServerIO_addRefreshQueryString("bid", CStr(PageID))
            End If
            '
            ' Handle Section ID Request Variable
            '
            SectionID = c.docProperties.getInteger("sid")
            If SectionID <> 0 Then
                Call c.htmlDoc.webServerIO_addRefreshQueryString("sid", CStr(SectionID))
            End If
            '
            '------------------------------------------------------------------------------------
            '   Determine RootPageID, SectionID, SectionContentID, SectionTemplateID
            '       SectionID=0 means deliver the landing page section
            '------------------------------------------------------------------------------------
            '
            If (PageID = 0) And (SectionID = 0) Then
                'hint = "2"
                '
                ' Nothing specified, use the Landing Page
                '
                PageID = main_GetLandingPageID()
                If PageID = 0 Then
                    '
                    ' landing page is not valid -- display error
                    '
                    returnHtml = "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                    Call c.handleLegacyError12("PagList_GetSection", "Page could not be determined. Error message displayed.")
                End If
            End If
            If (PageID <> 0) Then
                'hint = "3"
                '
                ' ----- PageID Given, determine the section from the pageid
                '
                If pageManager_cache_siteSection_rows = 0 Then
                    Call pageManager_cache_siteSection_load()
                End If
                rootPageId = pageManager_GetHtmlBody_GetSection_GetRootPageId(PageID)
                Ptr = pageManager_cache_siteSection_RootPageIDIndex.getPtr(CStr(rootPageId))
                '
                ' Open Section Record
                '
                If Ptr < 0 Then
                    SectionID = 0
                Else
                    SectionID = genericController.EncodeInteger(cache_siteSection(SSC_ID, Ptr))
                    SectionName = genericController.encodeText(cache_siteSection(SSC_Name, Ptr))
                    SectionTemplateID = genericController.EncodeInteger(cache_siteSection(SSC_TemplateID, Ptr))
                    SectionContentID = genericController.EncodeInteger(cache_siteSection(SSC_ContentID, Ptr))
                    SectionBlock = genericController.EncodeBoolean(cache_siteSection(SSC_BlockSection, Ptr))
                    Call c.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cache_siteSection(SSC_JSOnLoad, Ptr)), "site section")
                    Call c.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cache_siteSection(SSC_JSHead, Ptr)), "site section")
                    Call c.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cache_siteSection(SSC_JSEndBody, Ptr)), "site section")
                    JSFilename = genericController.encodeText(cache_siteSection(SSC_JSFilename, Ptr))
                    If JSFilename <> "" Then
                        JSFilename = c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                        Call c.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                    End If
                End If
            ElseIf (SectionID <> 0) Then
                'hint = "4"
                '
                ' ----- pageid=0, sectionid OK -- determine the page from the sectionid
                '           main_Get SectionLink -> compare to actual Link -> redirect if mismatch
                '
                If pageManager_cache_siteSection_rows = 0 Then
                    Call pageManager_cache_siteSection_load()
                End If
                Ptr = pageManager_cache_siteSection_IDIndex.getPtr(CStr(SectionID))
                If Ptr < 0 Then
                    '
                    ' Section not found, assume Landing Page
                    '
                    Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                    'Call main_LogPageNotFound(main_ServerLink)
                    pageManager_RedirectBecausePageNotFound = True
                    pageManager_RedirectReason = "The page could not be found because the section specified was not found. The section ID is [" & SectionID & "]. This section may have been deleted or marked inactive."
                    pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                    Exit Function
                    'SectionID = 0
                Else
                    SectionName = genericController.encodeText(cache_siteSection(SSC_Name, Ptr))
                    rootPageId = genericController.EncodeInteger(cache_siteSection(SSC_RootPageID, Ptr))
                    SectionTemplateID = genericController.EncodeInteger(cache_siteSection(SSC_TemplateID, Ptr))
                    SectionContentID = genericController.EncodeInteger(cache_siteSection(SSC_ContentID, Ptr))
                    SectionBlock = genericController.EncodeBoolean(cache_siteSection(SSC_BlockSection, Ptr))
                    Call c.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cache_siteSection(SSC_JSOnLoad, Ptr)), "site section")
                    Call c.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cache_siteSection(SSC_JSHead, Ptr)), "site section")
                    Call c.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cache_siteSection(SSC_JSEndBody, Ptr)), "site section")
                    JSFilename = genericController.encodeText(cache_siteSection(SSC_JSFilename, Ptr))
                    If JSFilename <> "" Then
                        JSFilename = c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                        Call c.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                    End If
                End If
                '
                ' Test for new section that needs a new blank page
                '
                If (pageManager_RedirectLink = "") And (SectionID <> 0) Then
                    '
                    ' RootPageID Mode
                    '
                    If (rootPageId = 0) And (c.app_errorCount = 0) Then
                        '
                        ' Root Page needs to be auto created
                        ' OK to create page here because section has a good record with a 0 RootPageID (this is not AutoHomeCreate)
                        '
                        rootPageId = c.main_CreatePageGetID(SectionName, "Page Content", SystemMemberID, "")
                        Call c.db.executeSql("update ccsections set RootPageID=" & rootPageId & " where id=" & SectionID)
                        Call pageManager_cache_siteSection_clear()
                        c.htmlDoc.main_AdminWarning = "<p>This page was created automatically because the section [" & SectionName & "] was requested, and it did not reference a page. Use the links below to edit the new page.</p>"
                        c.htmlDoc.main_AdminWarningPageID = rootPageId
                        c.htmlDoc.main_AdminWarningSectionID = SectionID
                        'Call app.csv_SetCS(CSSection, "RootPageID", RootPageID)
                    End If
                End If
                If (pageManager_RedirectLink = "") And (PageID = 0) And (rootPageId <> 0) Then
                    '
                    ' if no page, set page to the root of the section
                    '
                    PageID = rootPageId
                End If
            End If
            '
            If PageID = 0 Then
                '
                '------------------------------------------------------------------------------------
                ' Problem -- Page could not be determined
                '   2011/06/15 - changes
                '   this should not be a valid case because if the pageid is either given, set by section or set to landingpageid
                '
                '   allow a page that has no section -- so you can preview it, and so you can view any page in the page content
                '       - just display it with the default content
                '       - big change is that we now assume the parent + section was not blocked, before we assumed it was blocked
                '
                '   if pageid=0 at this point then the landing page could not be determined - put up an error message is all you can do
                '
                '   if pageid<>0 and sectionid=0 then the site will just display the default template
                '------------------------------------------------------------------------------------
                '
                Copy = "" _
                    & c.main_docType _
                    & vbCrLf & "<html>" _
                    & cr & "<body>" _
                    & cr2 & "<p style=""text-align:center;margin-top:100px;"">The page you requested could not be found and no landing page is configured for this domain.</p>" _
                    & cr & "</body>" _
                    & vbCrLf & "</html>" _
                    & ""
                'Call AppendLog("call main_getEndOfBody, from pageManager_getsection")
                Call c.htmlDoc.writeAltBuffer(Copy & c.htmlDoc.html_GetEndOfBody(False, False, False, False))
                Call c.handleLegacyError12("PagList_GetSection", "The page you requested could not be found and no landing page is configured for this domain [" & c.webServer.webServerIO_requestDomain & "].")
                '--- should be disposed by caller --- Call dispose
                Exit Function
            End If
            '
            If SectionID = 0 Then
                '
                ' Orphan Page -- Section could not be determined
                '
                allowPageWithoutSectionDislay = c.siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                allowPageWithoutSectionDislay = True
                Call logController.log_appendLog(c, "hardcoded allowPageWithoutSectionDislay in getHtmlBody_getSection")
                If Not allowPageWithoutSectionDislay Then
                    '
                    ' the rootPageid is used to represent the section record's selection, and is used in main_GetPageRaw to check if the
                    '   pagelist is structured correction
                    '
                    rootPageId = -1
                End If
                If False Then
                    'hint = "5"
                    '
                    ' No section specified, use LandingSection
                    '
                    rootPageId = main_GetLandingPageID()
                    RootPageName = main_GetLandingPageName(rootPageId)
                    If True Then
                        'If IsSectionRootPageIDMode Then
                        '
                        ' Page linked by PageID
                        '
                        SectionName = DefaultLandingSectionName
                        SectionCriteria = "RootPageID=" & rootPageId
                    Else
                        '
                        ' Legacy - Page linked by PageName
                        '
                        SectionName = LegacyLandingPageName
                        SectionCriteria = "Name = " & c.db.encodeSQLText(SectionName)
                    End If
                    '
                    ' main_Get Landing Section
                    '
                    CSSection = c.db.cs_open("Site Sections", SectionCriteria, "ID", , ,, , SectionFieldList)
                    '
                    ' try something new - if no landing section, use a "dummy" section with no blocking, etc.
                    '
                    If Not c.db.cs_ok(CSSection) Then
                        SectionID = 0
                        SectionName = ""
                        SectionTemplateID = 0
                        SectionContentID = 0
                        SectionBlock = False
                    Else
                        SectionID = c.db.cs_getInteger(CSSection, "ID")
                        SectionName = c.db.cs_getText(CSSection, "name")
                        SectionTemplateID = c.db.cs_getInteger(CSSection, "TemplateID")
                        SectionContentID = c.db.cs_getInteger(CSSection, "ContentID")
                        SectionBlock = c.db.cs_getBoolean(CSSection, "BlockSection")
                        Call c.htmlDoc.main_AddOnLoadJavascript2(c.db.cs_getText(CSSection, "JSOnLoad"), "site section")
                        Call c.htmlDoc.main_AddHeadScriptCode(c.db.cs_getText(CSSection, "JSHead"), "site section")
                        Call c.htmlDoc.main_AddEndOfBodyJavascript2(c.db.cs_getText(CSSection, "JSEndBody"), "site section")
                        JSFilename = c.db.cs_getText(CSSection, "JSFilename")
                        If JSFilename <> "" Then
                            JSFilename = c.webServer.webServerIO_requestPage & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                            Call c.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                        End If
                    End If
                    Call c.db.cs_Close(CSSection)
                    '
                    ' ????? if no landing section, lets try a dummy section
                    '
                End If
            End If
            '
            ' Save the SectionID publically to Add-ons can use it (dynamic menuing)
            '
            main_RenderedSectionID = SectionID
            main_RenderedSectionName = SectionName
            '
            '------------------------------------------------------------------------------------
            ' Determine RootPageContentName from SectionContentID
            '------------------------------------------------------------------------------------
            '
            'hint = "5"
            If SectionContentID = 0 And RootPageContentName = "" Then
                RootPageContentName = "Page Content"
                SectionContentID = c.main_GetContentID(RootPageContentName)
            ElseIf SectionContentID = 0 Then
                SectionContentID = c.main_GetContentID(RootPageContentName)
            Else
                RootPageContentName = c.metaData.getContentNameByID(SectionContentID)
            End If
            '
            '------------------------------------------------------------------------------------
            '   main_RefreshQueryString before main_Getting content
            '------------------------------------------------------------------------------------
            '
            '
            ' =========================== PageId, rootPageId, and sectionId are GOOD from this point forward ==========================
            '
            'hint = "6"
            If PageID <> 0 Then
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "bid", CStr(PageID), True)
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "sid", "")
            ElseIf SectionID <> 0 Then
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "bid", "")
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "sid", CStr(SectionID), True)
            Else
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "bid", "")
                c.web_RefreshQueryString = genericController.ModifyQueryString(c.web_RefreshQueryString, "sid", "")
            End If
            '
            '------------------------------------------------------------------------------------
            '   main_Get the Content and Template
            '   main_Get the TemplateID (link,body) from the main_RenderedNavigationStructure (already loaded)
            '------------------------------------------------------------------------------------
            '
            'hint = "7"
            '
            ' ????? if no section, allow a dummy section with id=0
            '
            If pageManager_RedirectLink = "" Then
                If c.main_isSectionBlocked(SectionID, SectionBlock) Then
                    '
                    ' Fake the meta content call to block the 'head before content' error and block with login panel
                    '
                    return_blockSiteWithLogin = True
                    Call c.main_SetMetaContent(0, 0)
                    returnHtml = c.htmlDoc.getLoginPanel()
                Else
                    '
                    ' main_Get the content
                    ' if QuickEditing, the content comes back with fpo_QuickEditor where content should be because
                    '   at this point we do not know which template it will be displayed on. After template calculate, if fpo_QucikEditor there,
                    '   call the main_GetFormInputHTML and replace it out
                    '
                    returnHtml = pageManager_GetHtmlBody_GetSection_GetContent(PageID, rootPageId, RootPageContentName, "", AllowChildPageList, AllowReturnLink, False, SectionID, UseContentWatchLink, allowPageWithoutSectionDislay)
                    pageManager_TemplateReason = "The reason this template was selected could not be determined."
                    If pageManager_RedirectLink = "" Then
                        '
                        ' ----- Use the structure to Calculate the Template Link from the loaded content
                        '
                        If main_RenderedNavigationStructure = "" Then
                            Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                            pageManager_RedirectBecausePageNotFound = True
                            pageManager_RedirectReason = "Redirecting because the page selected could not be found."
                            pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                            Exit Function
                        Else
                            test = main_RenderedNavigationStructure
                            If Left(test, 2) = vbCrLf Then
                                test = Mid(test, 3)
                            End If
                            If Right(test, 2) = vbCrLf Then
                                test = Mid(test, 1, Len(test) - 2)
                            End If
                            PageRow = genericController.SplitCRLF(test)
                            PageRowCnt = UBound(PageRow) + 1
                            For Ptr = PageRowCnt - 1 To 0 Step -1
                                navStruc = Split(PageRow(Ptr), vbTab)
                                If UBound(navStruc) > 6 Then
                                    If genericController.EncodeInteger(navStruc(pageManager_NavStruc_Descriptor)) < pageManager_NavStruc_Descriptor_ChildPage Then
                                        If navStruc(pageManager_NavStruc_TemplateId) <> "0" Then
                                            templateId = genericController.EncodeInteger(navStruc(pageManager_NavStruc_TemplateId))
                                            If genericController.EncodeInteger(navStruc(pageManager_NavStruc_Descriptor)) = pageManager_NavStruc_Descriptor_CurrentPage Then
                                                pageManager_TemplateReason = "This template was used because it is selected by the current page."
                                            Else
                                                pageManager_TemplateReason = "This template was used because it is selected one of this page's parents."
                                            End If
                                            Exit For
                                        End If
                                    End If
                                End If
                            Next
                        End If
                        If templateId <> 0 Then
                            '
                            ' check for valid template, if the template record has been deleted, it needs to be forgotten
                            '
                            TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
                            If TCPtr < 0 Then
                                templateId = 0
                            Else
                                main_RenderedTemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                                main_RenderedTemplateName = genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))
                            End If
                        End If
                        If templateId = 0 Then
                            '
                            ' try section template
                            '
                            templateId = SectionTemplateID
                            If templateId <> 0 Then
                                TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
                                If TCPtr < 0 Then
                                    templateId = 0
                                Else
                                    main_RenderedTemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                                    main_RenderedTemplateName = genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))
                                    pageManager_TemplateReason = "This template [" & main_RenderedTemplateName & "] was used because it is selected by the current section [" & main_RenderedSectionName & "]."
                                End If
                            End If
                            If (templateId = 0) And (c.domains.domainDetails.defaultTemplateId <> 0) Then
                                '
                                ' try domain's default template
                                '
                                templateId = c.domains.domainDetails.defaultTemplateId
                                TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
                                If TCPtr < 0 Then
                                    templateId = 0
                                Else
                                    main_RenderedTemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                                    main_RenderedTemplateName = genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))
                                    pageManager_TemplateReason = "This template [" & main_RenderedTemplateName & "] was used because it is selected as the default template for the current domain [" & c.webServer.webServerIO_requestDomain & "]."
                                End If
                            End If
                            If templateId = 0 Then
                                '
                                ' try the template named 'default'
                                '
                                Call pageManager_cache_pageTemplate_load()
                                For TCPtr = 0 To pageManager_cache_pageTemplate_rows - 1
                                    If genericController.vbLCase(genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))) = "default" Then
                                        templateId = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                                        Exit For
                                    End If
                                Next
                                If TCPtr >= pageManager_cache_pageTemplate_rows Then
                                    TCPtr = -1
                                Else
                                    main_RenderedTemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                                    main_RenderedTemplateName = genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))
                                    pageManager_TemplateReason = "This template was used because it is the default template for the site and no other template was selected [" & main_RenderedTemplateName & "]."
                                End If
                            End If
                        End If
                        '
                        ' Set the Template buffers
                        '
                        If TCPtr >= 0 Then
                            If c.authContext.visit.Mobile Then
                                If c.siteProperties.getBoolean("AllowMobileTemplates") Then
                                    '
                                    ' set Mobile Template
                                    '
                                    pageManager_TemplateBody = genericController.encodeText(cache_pageTemplate(TC_MobileBodyHTML, TCPtr))
                                    If pageManager_TemplateBody <> "" Then
                                        StylesFilename = genericController.encodeText(cache_pageTemplate(TC_MobileStylesFilename, TCPtr))
                                    End If
                                End If
                            End If
                            If pageManager_TemplateBody = "" Then
                                '
                                ' set web template if no other template sets it
                                '
                                pageManager_TemplateBody = genericController.encodeText(cache_pageTemplate(TC_BodyHTML, TCPtr))
                                StylesFilename = genericController.encodeText(cache_pageTemplate(TC_StylesFilename, TCPtr))
                            End If
                            pageManager_TemplateLink = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
                            pageManager_TemplateName = genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))
                            Call c.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cache_pageTemplate(TC_JSOnLoad, TCPtr)), "template")
                            Call c.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cache_pageTemplate(TC_JSInHeadLegacy, TCPtr)), "template")
                            Call c.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cache_pageTemplate(TC_JSEndBody, TCPtr)), "template")
                            Call c.htmlDoc.main_AddHeadTag2(genericController.encodeText(cache_pageTemplate(TC_OtherHeadTags, TCPtr)), "template")
                            pageManager_TemplateBodyTag = genericController.encodeText(cache_pageTemplate(TC_BodyTag, TCPtr))
                            JSFilename = genericController.encodeText(cache_pageTemplate(TC_JSInHeadFilename, TCPtr))
                            If JSFilename <> "" Then
                                JSFilename = c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                                Call c.htmlDoc.main_AddHeadScriptLink(JSFilename, "template")
                            End If
                            '
                            ' Add exclusive styles
                            '
                            If StylesFilename <> "" Then
                                If genericController.vbLCase(Right(StylesFilename, 4)) <> ".css" Then
                                    Call c.handleLegacyError15("Template [" & pageManager_TemplateName & "] StylesFilename is not a '.css' file, and will not display correct. Check that the field is setup as a CSSFile.", "pageManager_GetHtmlBody_GetSection")
                                Else
                                    c.htmlDoc.main_MetaContent_TemplateStyleSheetTag = cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, StylesFilename) & """ >"
                                End If
                            End If
                            '
                            ' Add shared styles
                            '

                            SharedStylesIDList = genericController.encodeText(cache_pageTemplate(TC_SharedStylesIDList, TCPtr))
                            If SharedStylesIDList <> "" Then
                                ListSplit = Split(SharedStylesIDList, ",")
                                For Ptr = 0 To UBound(ListSplit)
                                    styleId = genericController.EncodeInteger(ListSplit(Ptr))
                                    If styleId <> 0 Then
                                        Call c.htmlDoc.main_AddSharedStyleID2(genericController.EncodeInteger(ListSplit(Ptr)), "template")
                                    End If
                                Next
                            End If
                        End If
                        ' (*B)
                        templateLink = pageManager_TemplateLink
                        '
                        ' Verify Template Link matches the current page
                        '
                        If pageManager_RedirectLink = "" Then
                            If (TCPtr >= 0) And (c.siteProperties.allowTemplateLinkVerification) Then
                                PCCPtr = pageManager_cache_pageContent_getPtr(main_RenderedPageID, pagemanager_IsWorkflowRendering, c.authContext.isQuickEditing(c, ""))
                                '$$$$$ must check for PPtr<0
                                SecureLink_CurrentURL = (Left(LCase(c.webServer.webServerIO_ServerLink), 8) = "https://")
                                SecureLink_Template_Required = genericController.EncodeBoolean(cache_pageTemplate(TC_IsSecure, TCPtr))
                                SecureLink_Page_Required = genericController.EncodeBoolean(cache_pageContent(PCC_IsSecure, PCCPtr))
                                SecureLink_Required = SecureLink_Template_Required Or SecureLink_Page_Required
                                If (templateLink = "") Then
                                    '
                                    ' ----- no TemplateLink
                                    '       test that current secure settings match the templates secure sectting
                                    '
                                    If (SecureLink_CurrentURL <> SecureLink_Required) Then
                                        '
                                        ' redirect because protocol is wrong
                                        '
                                        If SecureLink_CurrentURL Then
                                            pageManager_RedirectLink = genericController.vbReplace(c.webServer.webServerIO_ServerLink, "https://", "http://")
                                            pageManager_RedirectReason = "Redirecting because neither the page or the template requires a secure link."
                                        Else
                                            pageManager_RedirectLink = genericController.vbReplace(c.webServer.webServerIO_ServerLink, "http://", "https://")
                                            If SecureLink_Page_Required Then
                                                pageManager_RedirectReason = "Redirecting because this page [" & main_RenderedPageName & "] requires a secure link."
                                            Else
                                                pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] requires a secure link."
                                            End If
                                        End If
                                    End If
                                Else
                                    '
                                    ' ----- TemplateLink given
                                    '
                                    CurrentLink = c.webServer.webServerIO_ServerLink
                                    If genericController.vbInstr(1, templateLink, "://", vbTextCompare) <> 0 Then
                                        '
                                        ' ----- TemplateLink is full
                                        '       this includes a short template with the secure checked case
                                        '       ignore TC_IsSecure, use the link's protocol
                                        '
                                        LinkSplit = Split(CurrentLink, "?")
                                        CurrentLinkNoQuery = LinkSplit(0)
                                        If (UCase(templateLink) <> genericController.vbUCase(CurrentLinkNoQuery)) Then
                                            '
                                            ' redirect to template link
                                            '
                                            pageManager_RedirectLink = pageManager_GetSectionLink(templateLink, PageID, SectionID)
                                            If PageID <> 0 Then
                                                pageManager_RedirectBecausePageNotFound = False
                                                pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & pageManager_TemplateReason
                                            ElseIf SectionID <> 0 Then
                                                pageManager_RedirectBecausePageNotFound = False
                                                pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & pageManager_TemplateReason
                                            Else
                                                pageManager_RedirectBecausePageNotFound = False
                                                pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & pageManager_TemplateReason
                                            End If
                                        End If
                                    Else
                                        '
                                        ' ----- TemplateLink is short
                                        '       test current short link vs template short link, and protocols
                                        '
                                        CurrentLink = genericController.vbReplace(CurrentLink, "https://", "http://", 1, 99, vbTextCompare)
                                        CurrentLink = genericController.ConvertLinkToShortLink(CurrentLink, c.webServer.requestDomain, c.webServer.webServerIO_requestVirtualFilePath)
                                        CurrentLink = genericController.EncodeAppRootPath(CurrentLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                                        LinkSplit = Split(CurrentLink, "?")
                                        CurrentLinkNoQuery = LinkSplit(0)
                                        If (SecureLink_CurrentURL <> SecureLink_Required) Or (UCase(templateLink) <> genericController.vbUCase(CurrentLinkNoQuery)) Then
                                            '
                                            ' This is not the correct page for this content, redirect
                                            ' This is NOT a pagenotfound - but a correctable condition that can not be avoided
                                            ' The pageid has as hard tamplate that must be redirected
                                            '
                                            pageManager_RedirectLink = pageManager_GetPageLink4(PageID, "", True, False)
                                            If SecureLink_Required Then
                                                '
                                                ' Redirect to Secure
                                                '
                                                If genericController.vbInstr(1, pageManager_RedirectLink, "http", vbTextCompare) = 1 Then
                                                    '
                                                    ' link is full
                                                    '
                                                    pageManager_RedirectLink = genericController.vbReplace(pageManager_RedirectLink, "http://", "https://", 1, 99, vbTextCompare)
                                                Else
                                                    '
                                                    ' link is root relative
                                                    '
                                                    pageManager_RedirectLink = "https://" & c.webServer.requestDomain & pageManager_RedirectLink
                                                End If
                                            Else
                                                '
                                                ' Redirect to non-Secure
                                                '
                                                If genericController.vbInstr(1, pageManager_RedirectLink, "http", vbTextCompare) = 1 Then
                                                    '
                                                    ' link is full
                                                    '
                                                    pageManager_RedirectLink = genericController.vbReplace(pageManager_RedirectLink, "https://", "http://", 1, 99, vbTextCompare)
                                                Else
                                                    '
                                                    ' link is root relative
                                                    '
                                                    pageManager_RedirectLink = "http://" & c.webServer.requestDomain & pageManager_RedirectLink
                                                End If
                                            End If
                                            'pageManager_RedirectLink = pageManager_GetSectionLink(TemplateLink, PageID, SectionID)
                                            pageManager_RedirectBecausePageNotFound = False
                                            pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & pageManager_TemplateReason
                                        End If
                                    End If
                                End If
                                '
                                ' check template domain requirements
                                '
                                If pageManager_RedirectLink = "" Then
                                    templatedomainIdList = genericController.encodeText(cache_pageTemplate(TC_DomainIdList, TCPtr))
                                    If (c.domains.domainDetails.id = 0) Then
                                        '
                                        ' current domain not recognized or default, use current
                                        '
                                    ElseIf (templatedomainIdList = "") Then
                                        '
                                        ' current template has no domain preference, use current
                                        '
                                    ElseIf (InStr(1, "," & templatedomainIdList & ",", "," & c.domains.domainDetails.id & ",") <> 0) Then
                                        '
                                        ' current domain is in the allowed c.domains list for this template, use it
                                        '
                                    Else
                                        '
                                        ' must redirect to a new template
                                        '
                                        domainIds = Split(templatedomainIdList, ",")
                                        For Ptr = 0 To UBound(domainIds)
                                            setdomainId = genericController.EncodeInteger(domainIds(Ptr))
                                            If setdomainId <> 0 Then
                                                Exit For
                                            End If
                                        Next
                                        linkDomain = c.content_GetRecordName("domains", setdomainId)
                                        If linkDomain <> "" Then
                                            pageManager_RedirectLink = genericController.vbReplace(c.webServer.webServerIO_ServerLink, "://" & c.webServer.requestDomain, "://" & linkDomain, 1, 99, vbTextCompare)
                                            pageManager_RedirectBecausePageNotFound = False
                                            pageManager_RedirectReason = "Redirecting because this template [" & main_RenderedTemplateName & "] requires a different domain [" & linkDomain & "]." & pageManager_TemplateReason
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' if fpo_QuickEdit it there, replace it out
                    '
                    Dim Editor As String
                    Dim styleList As String
                    Dim styleOptionList As String
                    Dim addonListJSON As String

                    If pageManager_RedirectLink = "" And (InStr(1, returnHtml, html_quickEdit_fpo) <> 0) Then
                        FieldRows = genericController.EncodeInteger(c.properties_user_getText("Page Content.copyFilename.PixelHeight", "500"))
                        If FieldRows < 50 Then
                            FieldRows = 50
                            Call c.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50)
                        End If
                        styleList = c.htmlDoc.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                        addonListJSON = c.htmlDoc.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
                        Editor = c.htmlDoc.html_GetFormInputHTML3("copyFilename", c.htmlDoc.html_quickEdit_copy, CStr(FieldRows), "100%", False, True, addonListJSON, styleList, styleOptionList)
                        returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor)
                    End If
                End If
            End If
            '
            '------------------------------------------------------------------------------------
            ' Add admin warning to the top of the content
            '------------------------------------------------------------------------------------
            '
            'hint = "8"
            If c.authContext.isAuthenticatedAdmin(c) And c.htmlDoc.main_AdminWarning <> "" Then
                '
                ' Display Admin Warnings with Edits for record errors
                '
                If c.htmlDoc.main_AdminWarningPageID <> 0 Then
                    c.htmlDoc.main_AdminWarning = c.htmlDoc.main_AdminWarning & "</p>" & c.main_GetRecordEditLink2("Page Content", c.htmlDoc.main_AdminWarningPageID, True, "Page " & c.htmlDoc.main_AdminWarningPageID, c.authContext.isAuthenticatedAdmin(c)) & "&nbsp;Edit the page<p>"
                    c.htmlDoc.main_AdminWarningPageID = 0
                End If
                '
                If c.htmlDoc.main_AdminWarningSectionID <> 0 Then
                    c.htmlDoc.main_AdminWarning = c.htmlDoc.main_AdminWarning & "</p>" & c.main_GetRecordEditLink2("Site Sections", c.htmlDoc.main_AdminWarningSectionID, True, "Section " & c.htmlDoc.main_AdminWarningSectionID, c.authContext.isAuthenticatedAdmin(c)) & "&nbsp;Edit the section<p>"
                    c.htmlDoc.main_AdminWarningSectionID = 0
                End If
                returnHtml = "" _
                    & c.htmlDoc.html_GetAdminHintWrapper(c.htmlDoc.main_AdminWarning) _
                    & returnHtml _
                    & ""
                c.htmlDoc.main_AdminWarning = ""
            End If
            '
            '------------------------------------------------------------------------------------
            ' handle redirect and edit wrapper
            '------------------------------------------------------------------------------------
            '
            If pageManager_RedirectLink <> "" Then
                Call c.webServer.webServerIO_Redirect2(pageManager_RedirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
            ElseIf AllowEditWrapper Then
                returnHtml = c.htmlDoc.main_GetEditWrapper("Page Content", returnHtml)
            End If
            '
            pageManager_GetHtmlBody_GetSection = returnHtml
            '
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection, Hint=" & hint)
        End Function
        ''
        ''
        ''
        Private Function pageManager_GetHtmlBody_GetSection_GetRootPageId(ByVal PageID As Integer, Optional ByVal UsedIDString As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetRootPageId")
            '
            Dim CS As Integer
            Dim ParentID As Integer
            Dim PageFound As Boolean
            Dim Ptr As Integer
            Dim rootPageId As Integer
            '
            If (PageID = 0) Or genericController.IsInDelimitedString(UsedIDString, CStr(PageID), ",") Then
                rootPageId = PageID
            Else
                If pageManager_cache_pageContent_rows = 0 Then
                    Call pageManager_cache_pageContent_load(pagemanager_IsWorkflowRendering, c.authContext.isQuickEditing(c, ""))
                End If
                Ptr = pageManager_cache_pageContent_idIndex.getPtr(CStr(PageID))
                If Ptr >= 0 Then
                    PageFound = True
                    ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, Ptr))
                End If
                If PageFound Then
                    If ParentID = 0 Then
                        rootPageId = PageID
                    Else
                        '
                        ' This one has a parent, look it up
                        '
                        rootPageId = pageManager_GetHtmlBody_GetSection_GetRootPageId(ParentID, UsedIDString & "," & CStr(PageID))
                    End If
                End If
            End If
            '
            pageManager_GetHtmlBody_GetSection_GetRootPageId = rootPageId
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetRootPageId")
        End Function
        '
        '=============================================================================
        '   pageManager_GetHtmlBody_GetSection_GetContent
        '
        '   PageID is the page to display. If it is 0, the root page is displayed
        '   RootPageID has to be the ID of the root page for PageID
        '=============================================================================
        '
        Public Function pageManager_GetHtmlBody_GetSection_GetContent(PageID As Integer, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, SectionID As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContent")
            '
            Dim ParentPtr As Integer
            Dim returnHtml As String
            Dim AddonName As String
            Dim addonCachePtr As Integer
            Dim addonPtr As Integer
            Dim AddOnCnt As Integer
            Dim layoutError As String
            Dim addonId As Integer
            Dim AddonContent As String
            Dim Err_Number As Integer
            Dim Err_Source As String
            Dim Err_Description As String
            Dim DateModified As Date
            Dim ErrString As String
            Dim JSOnLoad As String
            Dim JSHead As String
            Dim JSFilename As String
            Dim JSCopy As String
            Dim JSEndBody As String
            Dim PageRecordID As Integer
            Dim ContentPadding As Integer
            Dim Copy As String
            Dim RQS As String
            Dim Body As String
            Dim PageName As String
            Dim AllowHitNotification As Boolean
            'Dim cacheName As String
            Dim RootPageContentCID As Integer
            Dim PageContentCID As Integer
            Dim DateExpires As Date
            Dim dateArchive As Date
            Dim BakeExpires As Date
            Dim iRootPageContentName As String
            Dim PubDate As Date
            Dim PagePointer As Integer
            Dim CS As Integer
            Dim LineBuffer As String
            Dim LineSplit() As String
            'Dim RecordID as integer
            'Dim ContentID as integer
            'Dim ContentName As String
            Dim BlockedRecordIDList As String
            Dim Pointer As Integer
            Dim SQL As String
            Dim ContentBlocked As Boolean
            Dim RecordCount As Integer
            Dim RecordSplit() As String
            Dim BakeHeader As String
            Dim Delimiter As String
            Dim BakedStructure As String
            'Dim AuthoringMode As Boolean
            Dim NewPageCreated As Boolean
            Dim LineLeft As String
            Dim LineRight As String
            Dim LinePosition As Integer
            Dim SelectFieldList As String
            'dim buildversion As String
            Dim contactMemberID As Integer
            Dim BakeVersion As String
            'Dim AllowPageBaking As Boolean
            Dim SystemEMailID As Integer
            Dim ConditionID As Integer
            Dim ConditionGroupID As Integer
            Dim main_AddGroupID As Integer
            Dim RemoveGroupID As Integer
            Dim BlockSourceID As Integer
            Dim RegistrationGroupID As Integer
            Dim CustomBlockMessageFilename As String
            Dim BlockedPages() As String
            Dim BlockedPageRecordID As Integer
            Dim BlockForm As String
            Dim BlockCopy As String
            Dim PCCPtr As Integer
            Dim pageViewings As Integer
            '
            ' BuildVersion = app.dataBuildVersion
            '
            ' If no PageRecordID, use the RootPage
            '
            PageRecordID = PageID
            If PageRecordID = 0 Then
                PageRecordID = rootPageId
            End If
            If PageRecordID = 0 Then
                '
                ' no page and no root page, redirect to landing page
                '
                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                pageManager_RedirectBecausePageNotFound = True
                pageManager_RedirectReason = "The page could not be determined from URL."
                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
            Else
                '
                ' PageRecordID and RootPageID are good
                '
                Call c.htmlDoc.main_AddHeadTag2("<meta name=""contentId"" content=""" & PageRecordID & """ >", "page content")
                '
                'main_oldCacheArray_CurrentPagePtr = -1
                If RootPageContentName = "" Then
                    iRootPageContentName = "Page Content"
                Else
                    iRootPageContentName = RootPageContentName
                End If
                RootPageContentCID = c.main_GetContentID(iRootPageContentName)
                '
                '---------------------------------------------------------------------------------
                ' ----- Build Page if needed
                '---------------------------------------------------------------------------------
                '
                returnHtml = pageManager_GetHtmlBody_GetSection_GetContentBox(PageRecordID, rootPageId, iRootPageContentName, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, SectionID, UseContentWatchLink, allowPageWithoutSectionDisplay)
                If (returnHtml <> "") And (pageManager_RedirectLink = "") Then
                    '
                    ' This page is correct, main_Get the RecordID for later
                    '
                    NewPageCreated = True
                    BlockedRecordIDList = ""
                    If main_RenderCache_CurrentPage_PCCPtr >= 0 Then
                        '
                        ' Build the BlockedRecordIDList
                        '
                        PCCPtr = main_RenderCache_CurrentPage_PCCPtr
                        If genericController.EncodeBoolean(cache_pageContent(PCC_BlockContent, PCCPtr)) Or genericController.EncodeBoolean(cache_pageContent(PCC_BlockPage, PCCPtr)) Then
                            BlockedRecordIDList = BlockedRecordIDList & "," & genericController.encodeText(cache_pageContent(PCC_ID, PCCPtr))
                        End If
                        If main_RenderCache_ParentBranch_PCCPtrCnt > 0 Then
                            For ParentPtr = 0 To main_RenderCache_ParentBranch_PCCPtrCnt - 1
                                PCCPtr = genericController.EncodeInteger(main_RenderCache_ParentBranch_PCCPtrs(ParentPtr))
                                If genericController.EncodeBoolean(cache_pageContent(PCC_BlockContent, PCCPtr)) Or genericController.EncodeBoolean(cache_pageContent(PCC_BlockPage, PCCPtr)) Then
                                    BlockedRecordIDList = BlockedRecordIDList & "," & genericController.encodeText(cache_pageContent(PCC_ID, PCCPtr))
                                End If
                            Next
                        End If
                        If BlockedRecordIDList <> "" Then
                            BlockedRecordIDList = Mid(BlockedRecordIDList, 2)
                        End If
                    End If
                End If
                '
                JSOnLoad = genericController.encodeText(cache_pageContent(PCC_JSOnLoad, main_RenderCache_CurrentPage_PCCPtr))
                JSHead = genericController.encodeText(cache_pageContent(PCC_JSHead, main_RenderCache_CurrentPage_PCCPtr))
                JSFilename = genericController.encodeText(cache_pageContent(PCC_JSFilename, main_RenderCache_CurrentPage_PCCPtr))
                JSEndBody = genericController.encodeText(cache_pageContent(PCC_JSEndBody, main_RenderCache_CurrentPage_PCCPtr))
                DateModified = genericController.EncodeDate(cache_pageContent(PCC_ModifiedDate, main_RenderCache_CurrentPage_PCCPtr))
                '
                ' Save main_RenderedNavigationStructure in the Legacy Name
                '
                pageManager_ContentPageStructure = main_RenderedNavigationStructure
                '
                '---------------------------------------------------------------------------------
                ' ----- If Link field populated, do redirect
                '---------------------------------------------------------------------------------
                '
                Dim Link As String
                If (pageManager_RedirectLink = "") Then
                    Link = genericController.encodeText(cache_pageContent(PCC_Link, main_RenderCache_CurrentPage_PCCPtr))
                    If (Link <> "") Then
                        Call c.db.executeSql("update ccpagecontent set clicks=clicks+1 where id=" & main_RenderedPageID)
                        pageManager_RedirectLink = Link
                        pageManager_RedirectReason = "Redirect required because this page (PageRecordID=" & main_RenderedPageID & ") has a Link Override [" & pageManager_RedirectLink & "]."
                    End If
                End If
                '
                '---------------------------------------------------------------------------------
                ' ----- If Redirect, exit now
                '---------------------------------------------------------------------------------
                '
                If pageManager_RedirectLink <> "" Then
                    Exit Function
                End If
                '
                '---------------------------------------------------------------------------------
                ' ----- Content Blocking
                '---------------------------------------------------------------------------------
                '
                If (BlockedRecordIDList <> "") Then
                    If c.authContext.isAuthenticatedAdmin(c) Then
                        '
                        ' Administrators are never blocked
                        '
                    ElseIf (Not c.authContext.isAuthenticated()) Then
                        '
                        ' non-authenticated are always blocked
                        '
                        ContentBlocked = True
                    Else
                        '
                        ' Check Access Groups, if in access groups, remove group from BlockedRecordIDList
                        '
                        SQL = "SELECT DISTINCT ccPageContentBlockRules.RecordID" _
                            & " FROM (ccPageContentBlockRules" _
                            & " LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID)" _
                            & " LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID" _
                            & " WHERE (((ccMemberRules.MemberID)=" & c.db.encodeSQLNumber(c.authContext.user.ID) & ")" _
                            & " AND ((ccPageContentBlockRules.RecordID) In (" & BlockedRecordIDList & "))" _
                            & " AND ((ccPageContentBlockRules.Active)<>0)" _
                            & " AND ((ccgroups.Active)<>0)" _
                            & " AND ((ccMemberRules.Active)<>0)" _
                            & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & c.db.encodeSQLDate(c.app_startTime) & "));"
                        CS = c.db.cs_openSql(SQL)
                        BlockedRecordIDList = "," & BlockedRecordIDList
                        Do While c.db.cs_ok(CS)
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & c.db.cs_getText(CS, "RecordID"), "")
                            c.db.cs_goNext(CS)
                        Loop
                        Call c.db.cs_Close(CS)
                        If BlockedRecordIDList <> "" Then
                            '
                            ' ##### remove the leading comma
                            BlockedRecordIDList = Mid(BlockedRecordIDList, 2)
                            ' Check the remaining blocked records against the members Content Management
                            ' ##### removed hardcoded mistakes from the sql
                            SQL = "SELECT DISTINCT ccPageContent.ID as RecordID" _
                                & " FROM ((ccPageContent" _
                                & " LEFT JOIN ccGroupRules ON ccPageContent.ContentControlID = ccGroupRules.ContentID)" _
                                & " LEFT JOIN ccgroups AS ManagementGroups ON ccGroupRules.GroupID = ManagementGroups.ID)" _
                                & " LEFT JOIN ccMemberRules AS ManagementMemberRules ON ManagementGroups.ID = ManagementMemberRules.GroupID" _
                                & " WHERE (((ccPageContent.ID) In (" & BlockedRecordIDList & "))" _
                                & " AND ((ccGroupRules.Active)<>0)" _
                                & " AND ((ManagementGroups.Active)<>0)" _
                                & " AND ((ManagementMemberRules.Active)<>0)" _
                                & " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" & c.db.encodeSQLDate(c.app_startTime) & ")" _
                                & " AND ((ManagementMemberRules.MemberID)=" & c.authContext.user.ID & " ));"
                            CS = c.db.cs_openSql(SQL)
                            Do While c.db.cs_ok(CS)
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & c.db.cs_getText(CS, "RecordID"), "")
                                c.db.cs_goNext(CS)
                            Loop
                            Call c.db.cs_Close(CS)
                        End If
                        If BlockedRecordIDList <> "" Then
                            ContentBlocked = True
                        End If
                        Call c.db.cs_Close(CS)
                    End If
                End If
                '
                '
                '
                If ContentBlocked Then
                    BlockSourceID = main_BlockSourceDefaultMessage
                    ContentPadding = 20
                    BlockedPages = Split(BlockedRecordIDList, ",")
                    BlockedPageRecordID = genericController.EncodeInteger(BlockedPages(UBound(BlockedPages)))
                    If True Then
                        If BlockedPageRecordID <> 0 Then
                            '$$$$$ cache this
                            CS = c.csOpen("Page Content", BlockedPageRecordID, , , "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding")
                            If c.db.cs_ok(CS) Then
                                BlockSourceID = c.db.cs_getInteger(CS, "BlockSourceID")
                                ContentPadding = c.db.cs_getInteger(CS, "ContentPadding")
                                CustomBlockMessageFilename = c.db.cs_getText(CS, "CustomBlockMessage")
                                RegistrationGroupID = c.db.cs_getInteger(CS, "RegistrationGroupID")
                            End If
                            Call c.db.cs_Close(CS)
                        End If
                    End If
                    '
                    ' Block Appropriately
                    '
                    Select Case BlockSourceID
                        Case main_BlockSourceCustomMessage
                            '
                            ' ----- Custom Message
                            '
                            returnHtml = c.cdnFiles.readFile(CustomBlockMessageFilename)
                        Case main_BlockSourceLogin
                            '
                            ' ----- Login page
                            '
                            If Not c.authContext.isAuthenticated() Then
                                If Not c.authContext.isRecognized(c) Then
                                    '
                                    ' not recognized
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                        & ""
                                    BlockForm = c.htmlDoc.getLoginForm()
                                Else
                                    '
                                    ' recognized, not authenticated
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. You were recognized as ""<b>" & c.authContext.user.Name & "</b>"", but you need to login to continue. To login to this account or another, please use this form.</p>" _
                                        & ""
                                    BlockForm = c.htmlDoc.getLoginForm()
                                End If
                            Else
                                '
                                ' authenticated
                                '
                                BlockCopy = "" _
                                    & "<p>You are currently logged in as ""<b>" & c.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & c.web_RefreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                    & "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>" _
                                    & ""
                                BlockForm = c.htmlDoc.getLoginForm()
                            End If
                            returnHtml = "" _
                                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td align=center>" _
                                & "<div style=""width:400px;text-align:left;"">" _
                                & c.error_GetUserError() _
                                & BlockCopy _
                                & BlockForm _
                                & "</div></td></tr></table>"
                        Case main_BlockSourceRegistration
                            '
                            ' ----- Registration
                            '
                            If c.docProperties.getInteger("subform") = main_BlockSourceLogin Then
                                '
                                ' login subform form
                                '
                                BlockForm = c.htmlDoc.getLoginForm()
                                BlockCopy = "" _
                                    & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                    & "<p>If you do not have an account, <a href=?" & c.web_RefreshQueryString & "&subform=0>click here to register</a>.</p>" _
                                    & ""
                            Else
                                '
                                ' Register Form
                                '
                                If Not c.authContext.isAuthenticated() And c.authContext.isRecognized(c) Then
                                    '
                                    ' Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                    '
                                    Call c.authContext.logout(c)
                                End If
                                If Not c.authContext.isAuthenticated() Then
                                    '
                                    ' Not Authenticated
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. If you have an account, <a href=?" & c.web_RefreshQueryString & "&subform=" & main_BlockSourceLogin & ">Click Here to login</a>.</p>" _
                                        & "<p>To view this content, please complete this form.</p>" _
                                        & ""
                                Else
                                    BlockCopy = "" _
                                        & "<p>You are currently logged in as ""<b>" & c.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & c.web_RefreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                        & "<p>This account does not have access to this content. To view this content, please complete this form.</p>" _
                                        & ""
                                End If
                                '
                                If False Then '.3.551" Then
                                    '
                                    ' Old Db - use Joinform
                                    '
                                    'BlockForm = main_GetJoinForm()
                                Else
                                    '
                                    ' Use Registration FormPage
                                    '
                                    Call c.main_VerifyRegistrationFormPage()
                                    BlockForm = pageManager_GetFormPage("Registration Form", RegistrationGroupID)
                                End If
                            End If
                            returnHtml = "" _
                                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td align=center>" _
                                & "<div style=""width:400px;text-align:left;"">" _
                                & c.error_GetUserError() _
                                & BlockCopy _
                                & BlockForm _
                                & "</div></td></tr></table>"
                        Case Else
                            '
                            ' ----- Content as blocked - convert from site property to content page
                            '
                            returnHtml = pageManager_GetDefaultBlockMessage(UseContentWatchLink)
                    End Select
                    '
                    ' If the output is blank, put default message in
                    '
                    If returnHtml = "" Then
                        returnHtml = pageManager_GetDefaultBlockMessage(UseContentWatchLink)
                    End If
                    '
                    ' Encode the copy
                    '
                    returnHtml = c.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
                    returnHtml = c.htmlDoc.html_encodeContent9(returnHtml, c.authContext.user.ID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "http://" & c.webServer.requestDomain, False, c.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                    'returnHtml = main_EncodeContent5(returnHtml, memberID, main_RenderCache_CurrentPage_ContentName, PageRecordID, 0, False, False, True, True, False, True, "", "", False, app.SiteProperty_DefaultWrapperID)
                    RQS = c.web_RefreshQueryString
                    If RQS <> "" Then
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" & RQS, 1, 99, vbTextCompare)
                    End If
                    '
                    ' Add in content padding required for integration with the template
                    '
                    returnHtml = pageManager_GetContentBoxWrapper(returnHtml, ContentPadding)
                End If
                '
                '---------------------------------------------------------------------------------
                ' ----- Encoding, Tracking and Triggers
                '---------------------------------------------------------------------------------
                '
                '????? test triggers and trackcontentset
                If Not ContentBlocked Then
                    'IsPrinterversion = main_GetStreamText2(RequestNameInterceptpage) = LegacyInterceptPageSNPrinterversion)
                    If c.visitProperty.getBoolean("AllowQuickEditor") Then
                        '
                        ' Quick Editor, no encoding or tracking
                        '
                    Else
                        ' $$$$$ convert to pcc cache
                        'SelectFieldList = "ID,Viewings,ContentControlID,ContactMemberID,AllowHitNotification,TriggerSendSystemEmailID,TriggerConditionID,TriggerConditionGroupID,TriggerAddGroupID,TriggerRemoveGroupID"
                        '                If (main_RenderedPageID <> 0) And (main_RenderCache_CurrentPage_ContentId <> 0) Then
                        '                    'pageManager_ContentName = metaData.getContentNameByID(main_RenderCache_CurrentPage_ContentId)
                        '                    If main_RenderCache_CurrentPage_ContentName = "" Then
                        '                        main_RenderCache_CurrentPage_ContentName = iRootPageContentName
                        '                    End If
                        '                    If main_RenderCache_CurrentPage_ContentName <> "" Then
                        '                        CS = main_OpenCSContentRecord_Internal(main_RenderCache_CurrentPage_ContentName, main_RenderedPageID, , , SelectFieldList)
                        '                    End If
                        '                ElseIf (main_RenderCache_CurrentPage_ContentName <> "") Then
                        '                    CS = main_OpenCSContentRecord_Internal(main_RenderCache_CurrentPage_ContentName, PageRecordID, , , SelectFieldList)
                        '                End If
                        'If app.csv_IsCSOK(CS) Then
                        contactMemberID = genericController.EncodeInteger(cache_pageContent(PCC_ContactMemberID, main_RenderCache_CurrentPage_PCCPtr))
                        pageViewings = genericController.EncodeInteger(cache_pageContent(PCC_Viewings, main_RenderCache_CurrentPage_PCCPtr))
                        'contactMemberID = app.csv_cs_getInteger(CS, "ContactMemberID")
                        If c.authContext.isEditing(c, main_RenderCache_CurrentPage_ContentName) Or c.visitProperty.getBoolean("AllowWorkflowRendering") Then
                            '
                            ' Link authoring, workflow rendering -> do encoding, but no tracking
                            '
                            returnHtml = c.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
                            returnHtml = c.htmlDoc.html_encodeContent9(returnHtml, c.authContext.user.ID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "http://" & c.webServer.requestDomain, False, c.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                        ElseIf c.htmlDoc.pageManager_printVersion Then
                            '
                            ' Printer Version -> personalize and count viewings, no tracking
                            '
                            returnHtml = c.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
                            returnHtml = c.htmlDoc.html_encodeContent9(returnHtml, c.authContext.user.ID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "http://" & c.webServer.requestDomain, False, c.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                            'returnHtml = main_EncodeContent5(returnHtml, memberID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "", False, app.SiteProperty_DefaultWrapperID)
                            Call c.db.executeSql("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & main_RenderedPageID)
                            'Call app.csv_SetCS(CS, "Viewings", app.csv_cs_getInteger(CS, "Viewings") + 1)
                        Else
                            '
                            ' Live content
                            '
                            '!!!!!!!!!!!!!!!!!!!!!!!!
                            ' this should be done before the contentbox is added
                            ' so a stray blocktext does not truncate the html
                            '!!!!!!!!!!!!!!!!!!!!!!!!!
                            returnHtml = c.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
                            returnHtml = c.htmlDoc.html_encodeContent9(returnHtml, c.authContext.user.ID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "http://" & c.webServer.requestDomain, False, c.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                            'returnHtml = main_EncodeContent5(returnHtml, memberID, main_RenderCache_CurrentPage_ContentName, PageRecordID, contactMemberID, False, False, True, True, False, True, "", "", False, app.SiteProperty_DefaultWrapperID)
                            'Call main_TrackContent(main_RenderCache_CurrentPage_ContentName, main_RenderedPageID)
                            'Call main_TrackContentSet(CS)
                            Call c.db.executeSql("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & main_RenderedPageID)
                            'Call app.csv_SetCS(CS, "Viewings", app.csv_cs_getInteger(CS, "Viewings") + 1)
                        End If
                        '
                        ' Page Hit Notification
                        '
                        If (Not c.authContext.visit.ExcludeFromAnalytics) And (contactMemberID <> 0) And (InStr(1, c.webServer.requestBrowser, "kmahttp", vbTextCompare) = 0) Then
                            AllowHitNotification = genericController.EncodeBoolean(cache_pageContent(PCC_AllowHitNotification, main_RenderCache_CurrentPage_PCCPtr))
                            'AllowHitNotification = app.csv_cs_getBoolean(CS, "AllowHitNotification")
                            If AllowHitNotification Then
                                PageName = genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr))
                                If PageName = "" Then
                                    PageName = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, main_RenderCache_CurrentPage_PCCPtr))
                                    If PageName = "" Then
                                        PageName = genericController.encodeText(cache_pageContent(PCC_Headline, main_RenderCache_CurrentPage_PCCPtr))
                                        If PageName = "" Then
                                            PageName = "[no name]"
                                        End If
                                    End If
                                End If
                                Body = Body & "<p><b>Page Hit Notification.</b></p>"
                                Body = Body & "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>"
                                Body = Body & genericController.StartTable(4, 1, 1)
                                Body = Body & "<tr><td align=""right"" width=""150"" Class=""ccPanelHeader"">Description<br><img alt=""image"" src=""http://" & c.webServer.requestDomain & "/ccLib/images/spacer.gif"" width=""150"" height=""1""></td><td align=""left"" width=""100%"" Class=""ccPanelHeader"">Value</td></tr>"
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Domain", c.webServer.webServerIO_requestDomain, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Link", c.webServer.webServerIO_ServerLink, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Page Name", PageName, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Member Name", c.authContext.user.Name, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Member #", CStr(c.authContext.user.ID), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Start Time", CStr(c.authContext.visit.StartTime), False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit #", CStr(c.authContext.visit.ID), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit IP", c.webServer.requestRemoteIP, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Browser ", c.webServer.requestBrowser, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visitor #", CStr(c.authContext.visitor.id), False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Authenticated", CStr(c.authContext.visit.VisitAuthenticated), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Referrer", c.authContext.visit.HTTP_REFERER, False)
                                Body = Body & kmaEndTable
                                Call c.email.sendPerson(contactMemberID, c.siteProperties.getText("EmailFromAddress", "info@" & c.webServer.webServerIO_requestDomain), "Page Hit Notification", Body, False, True, 0, "", False)
                            End If
                        End If
                        '
                        ' Process Trigger Conditions
                        '
                        '   1) If Condition w/ Trigger Group
                        '   2) Then Send Email
                        '   3) Then Add to Group
                        '   4) Then Remove From Group
                        '
                        ConditionID = genericController.EncodeInteger(cache_pageContent(PCC_TriggerConditionID, main_RenderCache_CurrentPage_PCCPtr))
                        'ConditionID = app.csv_cs_getInteger(CS, "TriggerConditionID")
                        ConditionGroupID = genericController.EncodeInteger(cache_pageContent(PCC_TriggerConditionGroupID, main_RenderCache_CurrentPage_PCCPtr))
                        'ConditionGroupID = app.csv_cs_getInteger(CS, "TriggerConditionGroupID")
                        main_AddGroupID = genericController.EncodeInteger(cache_pageContent(PCC_TriggerAddGroupID, main_RenderCache_CurrentPage_PCCPtr))
                        'main_AddGroupID = app.csv_cs_getInteger(CS, "TriggerAddGroupID")
                        RemoveGroupID = genericController.EncodeInteger(cache_pageContent(PCC_TriggerRemoveGroupID, main_RenderCache_CurrentPage_PCCPtr))
                        'RemoveGroupID = app.csv_cs_getInteger(CS, "TriggerRemoveGroupID")
                        SystemEMailID = genericController.EncodeInteger(cache_pageContent(PCC_TriggerSendSystemEmailID, main_RenderCache_CurrentPage_PCCPtr))
                        'SystemEMailID = app.csv_cs_getInteger(CS, "TriggerSendSystemEmailID")
                        Select Case ConditionID
                            Case 1
                                '
                                ' Always
                                '
                                If SystemEMailID <> 0 Then
                                    Call c.email.sendSystem_Legacy(c.content_GetRecordName("System Email", SystemEMailID), "", c.authContext.user.ID)
                                End If
                                If main_AddGroupID <> 0 Then
                                    Call c.group_AddGroupMember(c.group_GetGroupName(main_AddGroupID))
                                End If
                                If RemoveGroupID <> 0 Then
                                    Call c.group_DeleteGroupMember(c.group_GetGroupName(RemoveGroupID))
                                End If
                            Case 2
                                '
                                ' If in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If c.authContext.IsMemberOfGroup2(c, c.group_GetGroupName(ConditionGroupID)) Then
                                        If SystemEMailID <> 0 Then
                                            Call c.email.sendSystem_Legacy(c.content_GetRecordName("System Email", SystemEMailID), "", c.authContext.user.ID)
                                        End If
                                        If main_AddGroupID <> 0 Then
                                            Call c.group_AddGroupMember(c.group_GetGroupName(main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call c.group_DeleteGroupMember(c.group_GetGroupName(RemoveGroupID))
                                        End If
                                    End If
                                End If
                            Case 3
                                '
                                ' If not in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If Not c.authContext.IsMemberOfGroup2(c, c.group_GetGroupName(ConditionGroupID)) Then
                                        If main_AddGroupID <> 0 Then
                                            Call c.group_AddGroupMember(c.group_GetGroupName(main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call c.group_DeleteGroupMember(c.group_GetGroupName(RemoveGroupID))
                                        End If
                                        If SystemEMailID <> 0 Then
                                            Call c.email.sendSystem_Legacy(c.content_GetRecordName("System Email", SystemEMailID), "", c.authContext.user.ID)
                                        End If
                                    End If
                                End If
                        End Select
                        'End If
                        'Call app.closeCS(CS)
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Add in ContentPadding (a table around content with the appropriate padding added)
                    '---------------------------------------------------------------------------------
                    '
                    If True And (returnHtml <> "") Then
                        ContentPadding = genericController.EncodeInteger(cache_pageContent(PCC_ContentPadding, main_RenderCache_CurrentPage_PCCPtr))
                        returnHtml = pageManager_GetContentBoxWrapper(returnHtml, ContentPadding)
                    End If


                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Set Headers
                    '---------------------------------------------------------------------------------
                    '
                    If DateModified <> Date.MinValue Then
                        Call c.webServer.web_addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified))
                        'Date: Sun, 07 Dec 2008 21:06:14 GMT
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Store page javascript
                    '---------------------------------------------------------------------------------
                    '
                    Call c.htmlDoc.main_AddOnLoadJavascript2(JSOnLoad, "page content")
                    Call c.htmlDoc.main_AddHeadScriptCode(JSHead, "page content")
                    If JSFilename <> "" Then
                        Call c.htmlDoc.main_AddHeadScriptLink(c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, JSFilename), "page content")
                    End If
                    Call c.htmlDoc.main_AddEndOfBodyJavascript2(JSEndBody, "page content")
                    '
                    '---------------------------------------------------------------------------------
                    ' Set the Meta Content flag
                    '---------------------------------------------------------------------------------
                    '
                    Call c.main_SetMetaContent(main_RenderCache_CurrentPage_ContentId, main_RenderedPageID)
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- OnPageStartEvent
                    '---------------------------------------------------------------------------------
                    '
                    pageManager_PageContent = returnHtml
                    AddOnCnt = UBound(c.addonCache.addonCache.onPageStartPtrs) + 1
                    For addonPtr = 0 To AddOnCnt - 1
                        addonCachePtr = c.addonCache.addonCache.onPageStartPtrs(addonPtr)
                        If addonCachePtr > -1 Then
                            addonId = c.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                            If addonId > 0 Then
                                AddonName = c.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                                AddonContent = c.addon.execute_legacy5(addonId, AddonName, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                                pageManager_PageContent = AddonContent & pageManager_PageContent
                            End If
                        End If
                    Next
                    returnHtml = pageManager_PageContent
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- OnPageEndEvent
                    '---------------------------------------------------------------------------------
                    '
                    pageManager_PageContent = returnHtml
                    AddOnCnt = UBound(c.addonCache.addonCache.onPageEndPtrs) + 1
                    For addonPtr = 0 To AddOnCnt - 1
                        addonCachePtr = c.addonCache.addonCache.onPageEndPtrs(addonPtr)
                        If addonCachePtr > -1 Then
                            addonId = c.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                            If addonId > 0 Then
                                AddonName = c.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                                AddonContent = c.addon.execute_legacy5(addonId, AddonName, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                                pageManager_PageContent = pageManager_PageContent & AddonContent
                            End If
                        End If
                    Next
                    returnHtml = pageManager_PageContent
                    '
                End If
                If c.htmlDoc.main_MetaContent_Title = "" Then
                    '
                    ' Set default page title
                    '
                    c.htmlDoc.main_MetaContent_Title = main_RenderedPageName
                End If
                '
                ' add contentid and sectionid
                '
                Call c.htmlDoc.main_AddHeadTag2("<meta name=""contentId"" content=""" & main_RenderedPageID & """ >", "page content")
                Call c.htmlDoc.main_AddHeadTag2("<meta name=""sectionId"" content=""" & main_RenderedSectionID & """ >", "page content")
            End If
            '
            ' Display Admin Warnings with Edits for record errors
            '
            If c.htmlDoc.main_AdminWarning <> "" Then
                '
                If c.htmlDoc.main_AdminWarningPageID <> 0 Then
                    c.htmlDoc.main_AdminWarning = c.htmlDoc.main_AdminWarning & "</p>" & c.main_GetRecordEditLink2("Page Content", c.htmlDoc.main_AdminWarningPageID, True, "Page " & c.htmlDoc.main_AdminWarningPageID, c.authContext.isAuthenticatedAdmin(c)) & "&nbsp;Edit the page<p>"
                    c.htmlDoc.main_AdminWarningPageID = 0
                End If
                '
                If c.htmlDoc.main_AdminWarningSectionID <> 0 Then
                    c.htmlDoc.main_AdminWarning = c.htmlDoc.main_AdminWarning & "</p>" & c.main_GetRecordEditLink2("Site Sections", c.htmlDoc.main_AdminWarningSectionID, True, "Section " & c.htmlDoc.main_AdminWarningSectionID, c.authContext.isAuthenticatedAdmin(c)) & "&nbsp;Edit the section<p>"
                    c.htmlDoc.main_AdminWarningSectionID = 0
                End If

                returnHtml = "" _
                    & c.htmlDoc.html_GetAdminHintWrapper(c.htmlDoc.main_AdminWarning) _
                    & returnHtml _
                    & ""
                c.htmlDoc.main_AdminWarning = ""
            End If
            '
            pageManager_GetHtmlBody_GetSection_GetContent = returnHtml
            '
            Exit Function
            '
ErrorTrap:
            Err_Number = Err.Number
            Err_Source = Err.Source
            Err_Description = Err.Description
            ErrString = genericController.GetErrString(Err)
            Call c.handleLegacyError19("pageManager_GetHtmlBody_GetSection_GetContent", "Trap", Err_Number, Err_Source, Err_Description, True)
            Err.Clear()
            If c.authContext.isAuthenticatedAdmin(c) Then
                '
                ' Put up an admin hint
                '
                pageManager_GetHtmlBody_GetSection_GetContent = c.htmlDoc.html_GetAdminHintWrapper("<p>There was an error creating the content for this page. The details of this error follow.</p><p>" & ErrString & "</p>")
            Else
                '
                ' There was a problem
                '
                pageManager_GetHtmlBody_GetSection_GetContent = "<!-- Error creating page content -->"
            End If
        End Function
        '
        '=============================================================================
        ' pageManager_GetHtmlBody_GetSection_GetContentBox
        '   PageID is the page to display. Must be non-0
        '   RootPageID is the id of the top-most page in the tree. Must be non-0
        '   If PageID is not under RootPageID, redirect to RootPageID
        '   If PageID is not within RootPageContentName, return the RootPageID
        '=============================================================================
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContentBox(PageID As Integer, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, SectionID As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContentBox")
            '
            Dim iIsEditing As Boolean
            Dim LiveBody As String
            Dim topOfParentBranchPtr As Integer
            Dim topOfParentBranchPageId As Integer
            Dim returnHtml As String
            Dim currentPageContentName As String
            Dim pageAdminMessage As String
            Dim isPageWithoutSection As Boolean
            Dim Pointer As Integer
            Dim SelectFieldList As String
            Dim Copy As String
            Dim PagePointer As Integer
            'Dim NowDate As Date
            Dim ContentName As String
            Dim EditTag As String
            Dim ContentPadding As Integer
            Dim pagePCCPtr As Integer
            Dim hint As String
            Dim EditLink As String
            Dim PageName As String
            Dim pageMenuHeadline As String
            Dim PageLink As String
            '
            'hint = "pageManager_GetHtmlBody_GetSection_GetContentBox, enter"
            If c.docOpen Then
                '
                ' ----- Load the content
                '
                'hint = hint & ",10"
                Call main_LoadRenderCache(PageID, rootPageId, RootPageContentName, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, SectionID, UseContentWatchLink)
                '
                ' ----- Verify a valid current page was found
                '
                ' ????? test
                'hint = hint & ",20"
                If (pageManager_RedirectLink = "") And (main_RenderCache_CurrentPage_PCCPtr = -1) Then
                    'hint = hint & ",30"
                    If PageID <> 0 Then
                        '
                        ' BID was not found, redirect to RootPage
                        '
                        Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                        pageManager_RedirectBecausePageNotFound = True
                        pageManager_RedirectReason = "The page could not be found from its ID [" & PageID & "]. It may have been deleted or marked inactive. "
                        pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                        Exit Function
                    Else
                        '
                        ' Root page was requested, but not found and could not be created, this is an error
                        '
                        Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                        pageManager_RedirectBecausePageNotFound = True
                        pageManager_RedirectReason = "The page could not be found because it's ID could not be determined."
                        pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                        Exit Function
                    End If
                End If
                '
                ' ----- Verify this bid can be displayed on this RootPageName
                '
                'hint = hint & ",50"
                If (pageManager_RedirectLink = "") Then
                    'hint = hint & ",60"
                    If Not allowPageWithoutSectionDisplay Then
                        '????? test
                        'hint = hint & ",70"
                        If (main_RenderCache_ParentBranch_PCCPtrCnt > 0) Then
                            '
                            ' check top of parent branch
                            '
                            '????? test
                            'hint = hint & ",80"
                            topOfParentBranchPtr = genericController.EncodeInteger(main_RenderCache_ParentBranch_PCCPtrs(main_RenderCache_ParentBranch_PCCPtrCnt - 1))
                            topOfParentBranchPageId = genericController.EncodeInteger(cache_pageContent(PCC_ID, topOfParentBranchPtr))
                            isPageWithoutSection = (topOfParentBranchPageId <> rootPageId)
                        Else
                            '
                            ' no parent pages, check the current page name against root page name
                            '
                            '????? test
                            'hint = hint & ",90"
                            isPageWithoutSection = (genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr)) <> rootPageId)
                        End If
                        'hint = hint & ",100"
                        If isPageWithoutSection Then
                            '
                            '
                            '
                            '????? test this
                            'hint = hint & ",110"
                            currentPageContentName = c.metaData.getContentNameByID(genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, main_RenderCache_CurrentPage_PCCPtr)))
                            If c.authContext.isAuthenticatedContentManager(c, currentPageContentName) Then
                                '
                                ' allow page without section because this is a content manager -- but give them a message
                                '
                                'hint = hint & ",120"
                                pageAdminMessage = "<p>This page can only be displayed to content managers because it is not part of a valid section. You can allow this type of access by setting the site property 'Allow Page Without Section Display' but care should be taken. If a blocked page is deleted, any child pages it may have had could be available for public display.</p>"
                            Else
                                '
                                ' page without section (root page name is the legacy section) not allowed
                                '
                                'hint = hint & ",130"
                                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                                pageManager_RedirectBecausePageNotFound = True
                                '????? test
                                pageManager_RedirectReason = "The page you requested [" & PageID & "] could not be displayed because there is a problem with one of it's parent pages. All parent pages must be available to verify security permissions. A parent page may have been deleted or inactivated, or the page may have been requested from an incorrect location."
                                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                                Exit Function
                            End If
                        End If
                    End If
                End If
                'hint = hint & ",140"
                If pageManager_RedirectLink = "" Then
                    '
                    ' ----- This page can be displayed
                    '
                    '            'hint = hint & ",150"
                    '            If app.csv_IsCSOK(main_oldCacheRS_cs) Then
                    '                '
                    '                ' if this is not the first pagecontent to be opened on this page, close the previous first
                    '                '
                    '                Call app.closeCS(main_oldCacheRS_cs)
                    '            End If
                    '            'hint = hint & ",160"
                    '            main_oldCacheRS_cs = main_OpenCSContentRecord_Internal(main_RenderCache_CurrentPage_ContentName, PageID, main_RenderCache_CurrentPage_IsRenderingMode Or main_RenderCache_CurrentPage_IsQuickEditing, main_RenderCache_CurrentPage_IsQuickEditing)
                    '
                    'hint = hint & ",170"
                    '            If Not app.csv_IsCSOK(main_oldCacheRS_cs) Then
                    '                '
                    '                ' freak bug - page was not found - maybe deleted in a concurrent process
                    '                '
                    '                'hint = hint & ",180"
                    '                Call app.closeCS(main_oldCacheRS_cs)
                    '                pageManager_RedirectBecausePageNotFound = True
                    ''????? test
                    '                pageManager_RedirectReason = "The page [" & PageID & "] could not be found."
                    '                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                    '                Exit Function
                    '            Else
                    '' $$$$$ remove main_oldCacheRS_cs, use pccPtr
                    '                'hint = hint & ",190"
                    '                'main_RenderedPageName = c.db.cs_getText(main_oldCacheRS_cs, "name")
                    '' $$$$$ this should just be the pcc field list
                    '                'SelectFieldList = app.cs_getSelectFieldList(main_oldCacheRS_cs)
                    '' $$$$$ remove all uses of main_oldCacheRS_FieldNames/main_oldCacheRS_FieldValues
                    '                'main_oldCacheRS_FieldNames = Split(SelectFieldList, ",")
                    '                'main_oldCacheRS_FieldValues = c.db.cs_getRow(main_oldCacheRS_cs)
                    '                'For Pointer = 0 To UBound(main_oldCacheRS_FieldValues)
                    '                '    main_oldCacheRS_FieldValues(Pointer) = genericController.vbReplace(Replace(main_oldCacheRS_FieldValues(Pointer), vbTab, ""), vbCrLf, "")
                    '                'Next
                    '            End If
                    '
                    ' ----- all calls go through Live body routine, Quick Editor added directly to live routine
                    '
                    ' $$$$$ remove main_oldCacheRS_cs, use pccPtr
                    'hint = hint & ",200"
                    '????? test - this was a routine placed in-line
                    iIsEditing = c.authContext.isEditing(c, main_RenderCache_CurrentPage_ContentName)
                    '
                    ' ----- Render the Body
                    '
                    LiveBody = pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body(main_RenderCache_CurrentPage_ContentName, main_RenderCache_CurrentPage_ContentId, OrderByClause, AllowChildPageList, False, rootPageId, AllowReturnLink, RootPageContentName, ArchivePages)
                    If c.authContext.isAdvancedEditing(c, "") Then
                        returnHtml = returnHtml & c.main_GetRecordEditLink(main_RenderCache_CurrentPage_ContentName, PageID, (Not main_RenderCache_CurrentPage_IsRootPage)) & LiveBody
                    ElseIf iIsEditing Then
                        PageName = genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr))
                        EditLink = c.main_GetRecordEditLink2(main_RenderCache_CurrentPage_ContentName, PageID, (Not main_RenderCache_CurrentPage_IsRootPage), PageName, c.authContext.isEditing(c, ContentName))
                        returnHtml = returnHtml & c.htmlDoc.main_GetEditWrapper("", c.main_GetRecordEditLink(main_RenderCache_CurrentPage_ContentName, PageID, (Not main_RenderCache_CurrentPage_IsRootPage)) & LiveBody)
                    Else
                        returnHtml = returnHtml & LiveBody
                    End If
                    '
                    ' Build the Public main_RenderedNavigationStructure
                    '
                    '????? test all this --------------------- start
                    'hint = hint & ",210"
                    main_RenderedNavigationStructure = ""
                    If main_RenderCache_ParentBranch_PCCPtrCnt > 0 Then
                        'hint = hint & ",220"
                        For Pointer = main_RenderCache_ParentBranch_PCCPtrCnt - 1 To 0 Step -1
                            'hint = hint & ",230"
                            If Pointer = (main_RenderCache_ParentBranch_PCCPtrCnt - 1) Then
                                main_RenderedNavigationStructure = main_RenderedNavigationStructure & vbTab & "0"
                            Else
                                main_RenderedNavigationStructure = main_RenderedNavigationStructure & vbTab & "1"
                            End If
                            pagePCCPtr = genericController.EncodeInteger(main_RenderCache_ParentBranch_PCCPtrs(Pointer))
                            '
                            ' buffer text fields because this excode format does not allow them
                            '
                            PageName = genericController.encodeText(cache_pageContent(PCC_Name, pagePCCPtr))
                            PageName = genericController.vbReplace(PageName, vbCrLf, " ")
                            PageName = genericController.vbReplace(PageName, vbCr, " ")
                            PageName = genericController.vbReplace(PageName, vbLf, " ")
                            PageName = genericController.vbReplace(PageName, vbTab, " ")
                            PageName = Trim(PageName)
                            '
                            PageLink = genericController.encodeText(cache_pageContent(PCC_Link, pagePCCPtr))
                            PageLink = genericController.vbReplace(PageLink, vbCrLf, " ")
                            PageLink = genericController.vbReplace(PageLink, vbCr, " ")
                            PageLink = genericController.vbReplace(PageLink, vbLf, " ")
                            PageLink = genericController.vbReplace(PageLink, vbTab, " ")
                            PageLink = Trim(PageLink)
                            '
                            pageMenuHeadline = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, pagePCCPtr)))
                            If pageMenuHeadline <> "" Then
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCrLf, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCr, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbLf, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbTab, " ")
                                pageMenuHeadline = Trim(pageMenuHeadline)
                            Else
                                pageMenuHeadline = PageName
                                If pageMenuHeadline = "" Then
                                    pageMenuHeadline = "Related Page"
                                End If
                            End If
                            main_RenderedNavigationStructure = main_RenderedNavigationStructure _
                            & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_ID, pagePCCPtr)) _
                            & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_ParentID, pagePCCPtr)) _
                            & vbTab & pageMenuHeadline _
                            & vbTab & PageName _
                            & vbTab & PageLink _
                            & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, pagePCCPtr)) _
                            & vbTab & genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, pagePCCPtr)) _
                            & vbCrLf
                        Next
                    End If
                    'hint = hint & ",300"
                    If main_RenderCache_CurrentPage_PCCPtr > -1 Then
                        'hint = hint & ",310"
                        pagePCCPtr = main_RenderCache_CurrentPage_PCCPtr
                        main_RenderedPageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, pagePCCPtr))
                        '
                        ' buffer text fields because this excode format does not allow them
                        '
                        PageName = genericController.encodeText(cache_pageContent(PCC_Name, pagePCCPtr))
                        PageName = genericController.vbReplace(PageName, vbCrLf, " ")
                        PageName = genericController.vbReplace(PageName, vbCr, " ")
                        PageName = genericController.vbReplace(PageName, vbLf, " ")
                        PageName = genericController.vbReplace(PageName, vbTab, " ")
                        PageName = Trim(PageName)
                        '
                        PageLink = genericController.encodeText(cache_pageContent(PCC_Link, pagePCCPtr))
                        PageLink = genericController.vbReplace(PageLink, vbCrLf, " ")
                        PageLink = genericController.vbReplace(PageLink, vbCr, " ")
                        PageLink = genericController.vbReplace(PageLink, vbLf, " ")
                        PageLink = genericController.vbReplace(PageLink, vbTab, " ")
                        PageLink = Trim(PageLink)
                        '
                        pageMenuHeadline = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, pagePCCPtr)))
                        If pageMenuHeadline <> "" Then
                            pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCrLf, " ")
                            pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCr, " ")
                            pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbLf, " ")
                            pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbTab, " ")
                            pageMenuHeadline = Trim(pageMenuHeadline)
                        Else
                            pageMenuHeadline = PageName
                            If pageMenuHeadline = "" Then
                                pageMenuHeadline = "Related Page"
                            End If
                        End If
                        main_RenderedNavigationStructure = main_RenderedNavigationStructure _
                        & vbTab & "2" _
                        & vbTab & main_RenderedPageID _
                        & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_ParentID, pagePCCPtr)) _
                        & vbTab & pageMenuHeadline _
                        & vbTab & PageName _
                        & vbTab & PageLink _
                        & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, pagePCCPtr)) _
                        & vbTab & genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, pagePCCPtr)) _
                        & vbCrLf
                    End If
                    'hint = hint & ",400"
                    If main_RenderCache_ChildBranch_PCCPtrCnt > 0 Then
                        'hint = hint & ",410 main_RenderCache_ChildBranch_PCCPtrCnt=" & main_RenderCache_ChildBranch_PCCPtrCnt
                        For Pointer = main_RenderCache_ChildBranch_PCCPtrCnt - 1 To 0 Step -1
                            'hint = hint & ",420 Pointer=" & Pointer
                            pagePCCPtr = genericController.EncodeInteger(main_RenderCache_ChildBranch_PCCPtrs(Pointer))
                            '
                            ' buffer text fields because this excode format does not allow them
                            '
                            'hint = hint & ",430 pagePCCPtr=" & pagePCCPtr
                            PageName = genericController.encodeText(cache_pageContent(PCC_Name, pagePCCPtr))
                            PageName = genericController.vbReplace(PageName, vbCrLf, " ")
                            PageName = genericController.vbReplace(PageName, vbCr, " ")
                            PageName = genericController.vbReplace(PageName, vbLf, " ")
                            PageName = genericController.vbReplace(PageName, vbTab, " ")
                            PageName = Trim(PageName)
                            '
                            PageLink = genericController.encodeText(cache_pageContent(PCC_Link, pagePCCPtr))
                            PageLink = genericController.vbReplace(PageLink, vbCrLf, " ")
                            PageLink = genericController.vbReplace(PageLink, vbCr, " ")
                            PageLink = genericController.vbReplace(PageLink, vbLf, " ")
                            PageLink = genericController.vbReplace(PageLink, vbTab, " ")
                            PageLink = Trim(PageLink)
                            '
                            pageMenuHeadline = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, pagePCCPtr)))
                            If pageMenuHeadline <> "" Then
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCrLf, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbCr, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbLf, " ")
                                pageMenuHeadline = genericController.vbReplace(pageMenuHeadline, vbTab, " ")
                                pageMenuHeadline = Trim(pageMenuHeadline)
                            Else
                                pageMenuHeadline = PageName
                                If pageMenuHeadline = "" Then
                                    pageMenuHeadline = "Related Page"
                                End If
                            End If
                            'hint = hint & ",440"
                            main_RenderedNavigationStructure = main_RenderedNavigationStructure _
                            & vbTab & "3" _
                            & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_ID, pagePCCPtr)) _
                            & vbTab & main_RenderedPageID _
                            & vbTab & pageMenuHeadline _
                            & vbTab & PageName _
                            & vbTab & PageLink _
                            & vbTab & genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, pagePCCPtr)) _
                            & vbTab & genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, pagePCCPtr)) _
                            & vbCrLf
                            'hint = hint & ",450"
                        Next
                    End If
                End If
                '????? test all this --------------------- end
                'hint = hint & ",900"
                If pageAdminMessage <> "" Then
                    returnHtml = "" _
                    & c.htmlDoc.html_GetAdminHintWrapper(pageAdminMessage) _
                    & returnHtml _
                    & ""
                End If
            End If
            '
            Call c.debug_testPoint("pageManager_GetHtmlBody_GetSection_GetContentBox, hint=[" & hint & "]")
            pageManager_GetHtmlBody_GetSection_GetContentBox = returnHtml
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetContentBox, hint=[" & hint & "]")
        End Function
        '
        '========================================================================
        '   Render the Page Body.
        '
        '   CSParentofChildPages is a contentset with the page that is the parent of all child pages for this render.
        '========================================================================
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body(ContentName As String, ContentID As Integer, OrderByClause As String, AllowChildList As Boolean, Authoring As Boolean, rootPageId As Integer, AllowReturnLink As Boolean, RootPageContentName As String, ArchivePage As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body")
            '
            Dim hint As String
            Dim Cell As String
            Dim AddonStatusOK As Boolean
            Dim ChildListInstanceOptions As String
            Dim Name As String
            Dim DateReviewed As Date
            Dim ReviewedBy As Integer
            Dim Link As String
            Dim CS As Integer
            Dim IconRow As String
            Dim Filename As String
            Dim contactMemberID As Integer
            Dim s As String
            Dim QueryString As String
            Dim ParentofChildPageID As Integer
            Dim LastModified As Date
            Dim MethodName As String
            'Dim Compatibility21 As Boolean
            Dim StreamAdd As String
            Dim LoopCount As Integer
            Dim ACStart As Integer
            Dim ACStop As Integer
            Dim TagBuffer As String
            Dim TagSplit() As String
            Dim NVSplit() As String
            Dim ListName As String
            Dim ChildList As String
            Dim childListSortMethodId As Integer
            Dim ContentStarted As Boolean
            Dim AllowEmailPage As Boolean
            Dim AllowPrinterVersion As Boolean
            Dim Caption As String
            Dim PageID As Integer
            Dim parentPageID As Integer
            Dim allowChildListDisplay As Boolean
            Dim dateArchive As Date
            Dim allowChildListComposite As Boolean
            Dim allowReturnLinkComposite As Boolean
            Dim allowReturnLinkDisplay As Boolean
            Dim headline As String
            Dim copyFilename As String
            Dim Copy As String
            Dim EmailBody As String
            Dim Body As String
            Dim allowSeeAlso As Boolean
            Dim allowMoreInfo As Boolean
            Dim allowFeedback As Boolean
            Dim allowLastModifiedFooter As Boolean
            Dim ModifiedBy As Integer
            Dim allowReviewedFooter As Boolean
            Dim allowMessageFooter As Boolean
            Dim pageContentMessageFooter As String
            '
            'hint = hint & "pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body,010"
            MethodName = "pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body"
            '
            ' ContentID = genericController.EncodeInteger(main_GetContentID(ContentName))
            '
            If True Then
                'If app.csv_IsCSOK(CSPointer) Then
                '
                ' ----- A page was found
                '
                PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
                'pageID = (app.csv_cs_getInteger(CSPointer, "ID"))
                '        s = "" _
                '            & "<div>main_RenderedPageID=" & main_RenderedPageID & "</div>" _
                '            & "<div>CSPointer id=" & PageID & "</div>" _
                '            & "<div>main_RenderCache_CurrentPage_PCCPtr=" & main_RenderCache_CurrentPage_PCCPtr & "</div>" _
                '            & "<div>main_pcc(PCC_ID, main_RenderCache_CurrentPage_PCCPtr )=" & main_pcc(PCC_ID, main_RenderCache_CurrentPage_PCCPtr) & "</div>" _
                '            & ""
                parentPageID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                'ParentPageID = (app.csv_cs_getInteger(CSPointer, "parentid"))
                contactMemberID = genericController.EncodeInteger(cache_pageContent(PCC_ContactMemberID, main_RenderCache_CurrentPage_PCCPtr))
                'contactMemberID = (app.csv_cs_getInteger(CSPointer, "ContactMemberID"))
                allowChildListDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, main_RenderCache_CurrentPage_PCCPtr))
                allowChildListComposite = AllowChildList And allowChildListDisplay
                'allowChildListComposite = AllowChildList And (app.csv_cs_getBoolean(CSPointer, "AllowChildListDisplay"))
                dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, main_RenderCache_CurrentPage_PCCPtr))
                'DateArchive = app.csv_cs_getDate(CSPointer, "DateArchive")
                'Compatibility21 = genericController.EncodeBoolean(csv_GetSiteProperty("ContentPageCompatibility21", False))
                childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
                'ChildListSortMethodID = app.csv_cs_getInteger(CSPointer, "ChildListSortMethodID")
                allowReturnLinkDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_allowReturnLinkDisplay, main_RenderCache_CurrentPage_PCCPtr))
                allowReturnLinkComposite = AllowReturnLink And allowReturnLinkDisplay
                '
                AllowPrinterVersion = genericController.EncodeBoolean(cache_pageContent(pcc_allowPrinterVersion, main_RenderCache_CurrentPage_PCCPtr))
                'AllowPrinterVersion = app.csv_cs_getBoolean(CSPointer, "AllowPrinterVersion")
                AllowEmailPage = genericController.EncodeBoolean(cache_pageContent(pcc_allowEmailPage, main_RenderCache_CurrentPage_PCCPtr))
                'AllowEmailPage = app.csv_cs_getBoolean(CSPointer, "AllowEmailPage")
                headline = genericController.encodeText(cache_pageContent(PCC_Headline, main_RenderCache_CurrentPage_PCCPtr))
                'headline = app.csv_cs_get(CSPointer, "Headline")
                copyFilename = genericController.encodeText(cache_pageContent(PCC_CopyFilename, main_RenderCache_CurrentPage_PCCPtr))
                If copyFilename <> "" Then
                    Copy = c.cdnFiles.readFile(copyFilename)
                End If
                'copy = app.csv_cs_get(CSPointer, "copyFilename")
                allowSeeAlso = genericController.EncodeBoolean(cache_pageContent(pcc_allowSeeAlso, main_RenderCache_CurrentPage_PCCPtr))
                'allowSeeAlso = genericController.EncodeBoolean(app.csv_cs_getBoolean(CSPointer, "AllowSeeAlso"))        '
                allowMoreInfo = genericController.EncodeBoolean(cache_pageContent(pcc_allowMoreInfo, main_RenderCache_CurrentPage_PCCPtr))
                'allowMoreInfo = genericController.EncodeBoolean(app.csv_cs_getBoolean(CSPointer, "AllowMoreInfo"))
                allowFeedback = genericController.EncodeBoolean(cache_pageContent(pcc_allowFeedback, main_RenderCache_CurrentPage_PCCPtr))
                'AllowFeedBack = genericController.EncodeBoolean(app.csv_cs_getBoolean(CSPointer, "AllowFeedBack"))
                LastModified = genericController.EncodeDate(cache_pageContent(PCC_ModifiedDate, main_RenderCache_CurrentPage_PCCPtr))
                'LastModified = app.csv_cs_getDate(CSPointer, "ModifiedDate")
                allowLastModifiedFooter = genericController.EncodeBoolean(cache_pageContent(pcc_allowLastModifiedFooter, main_RenderCache_CurrentPage_PCCPtr))
                'AllowLastModifiedFooter = app.csv_cs_getBoolean(CSPointer, "AllowLastModifiedFooter")
                ModifiedBy = genericController.EncodeInteger(cache_pageContent(PCC_ModifiedBy, main_RenderCache_CurrentPage_PCCPtr))
                'ModifiedBy = app.cs_getInteger(CSPointer, "ModifiedBy")
                DateReviewed = genericController.EncodeDate(cache_pageContent(PCC_DateReviewed, main_RenderCache_CurrentPage_PCCPtr))
                'DateReviewed = app.csv_cs_getDate(CSPointer, "DateReviewed")
                ReviewedBy = genericController.EncodeInteger(cache_pageContent(PCC_ReviewedBy, main_RenderCache_CurrentPage_PCCPtr))
                'ReviewedBy = app.cs_getInteger(CSPointer, "ReviewedBy")
                allowReviewedFooter = genericController.EncodeBoolean(cache_pageContent(PCC_allowReviewedFooter, main_RenderCache_CurrentPage_PCCPtr))
                'allowReviewedFooter = (app.csv_cs_getBoolean(CSPointer, "AllowReviewedFooter"))
                allowMessageFooter = genericController.EncodeBoolean(cache_pageContent(PCC_allowMessageFooter, main_RenderCache_CurrentPage_PCCPtr))
                'allowMessageFooter = (app.csv_cs_getBoolean(CSPointer, "AllowMessageFooter"))
                '
                ' ----- Print Breadcrumb if not at root Page
                '
                'hint = hint & ",020"
                Dim breadCrumb As String
                Dim BreadCrumbDelimiter As String
                Dim BreadCrumbPrefix As String
                '
                If allowReturnLinkComposite And (Not main_RenderCache_CurrentPage_IsRootPage) And (Not c.htmlDoc.pageManager_printVersion) Then
                    '
                    ' ----- Print Heading if not at root Page
                    '
                    BreadCrumbPrefix = c.siteProperties.getText("BreadCrumbPrefix", "Return to")
                    BreadCrumbDelimiter = c.siteProperties.getText("BreadCrumbDelimiter", " &gt; ")
                    breadCrumb = pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink(RootPageContentName, parentPageID, rootPageId, "", ArchivePage, BreadCrumbDelimiter)
                    If breadCrumb <> "" Then
                        breadCrumb = cr & "<p class=""ccPageListNavigation"">" & BreadCrumbPrefix & " " & breadCrumb & "</p>"
                    End If
                End If
                s = s & breadCrumb
                '
                ' move print and email icons here - ASBO 5/24/2007
                '
                'hint = hint & ",030"
                If (Not c.htmlDoc.pageManager_printVersion) Then
                    IconRow = ""
                    If (Not c.authContext.visit.Bot) And (AllowPrinterVersion Or AllowEmailPage) Then
                        '
                        ' not a bot, and either print or email allowed
                        '
                        If AllowPrinterVersion Then
                            QueryString = c.web_RefreshQueryString
                            'QueryString = genericController.ModifyQueryString(QueryString, RequestNameRootPageID, CStr(RootPageID), True)
                            QueryString = genericController.ModifyQueryString(QueryString, "bid", genericController.encodeText(PageID), True)
                            'QueryString = genericController.ModifyQueryString(QueryString, RequestNameAllowChildPageList, genericController.encodeText(allowChildListComposite), True)
                            'QueryString = genericController.ModifyQueryString(QueryString, RequestNameContent, RootPageContentName, True)
                            'QueryString = genericController.ModifyQueryString(QueryString, RequestNameOrderByClause, OrderByClause, True)
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameRootPageID, CStr(RootPageID), True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, "bid", genericController.encodeText(pageId), True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameAllowChildPageList, genericController.encodeText(allowChildListComposite), True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameContent, RootPageContentName, True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameOrderByClause, OrderByClause, True)
                            '                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            Caption = c.siteProperties.getText("PagePrinterVersionCaption", "Printer Version")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a href=""" & c.htmlDoc.html_EncodeHTML(c.webServer.webServerIO_requestPage & "?" & QueryString) & """ target=""_blank""><img alt=""image"" src=""/ccLib/images/IconSmallPrinter.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp<a href=""" & c.htmlDoc.html_EncodeHTML(c.webServer.webServerIO_requestPage & "?" & QueryString) & """ target=""_blank"" style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                        If AllowEmailPage Then
                            QueryString = c.web_RefreshQueryString
                            If QueryString <> "" Then
                                QueryString = "?" & QueryString
                            End If
                            EmailBody = c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.webServer.requestPathPage & QueryString
                            Caption = c.siteProperties.getText("PageAllowEmailCaption", "Email This Page")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """><img alt=""image"" src=""/ccLib/images/IconSmallEmail.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """ style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                    End If
                    If IconRow <> "" Then
                        s = s _
                        & cr & "<div style=""text-align:right;"">" _
                        & genericController.kmaIndent(IconRow) _
                        & cr & "</div>"
                    End If
                End If
                '
                ' ----- Start Text Search
                '
                Cell = ""
                'Cell = Cell & main_GetSiteProperty2("TextSearchStartTag", TextSearchStartTagDefault)         '
                'hint = hint & ",040"
                If main_RenderCache_CurrentPage_IsQuickEditing Then
                    '
                    ' ----- Copy in Quick Editor Mode - ##### new part
                    '       ##### Put presentatin authoring right in the live body
                    '
                    'hint = hint & ",041"
                    Cell = Cell & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing(main_RenderCache_CurrentPage_ContentName, rootPageId, RootPageContentName, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, contactMemberID, ContentName, childListSortMethodId, allowChildListComposite, ArchivePage)
                Else
                    '
                    ' ----- Headline
                    '
                    'hint = hint & ",042"
                    If headline <> "" Then
                        'hint = hint & ",043"
                        ' an html field can be added to an html stream. a non-html field should be html encoded before being added.
                        headline = c.htmlDoc.main_encodeHTML(headline)
                        If c.siteProperties.getBoolean("PageHeadlineUseccHeadline") Then
                            Cell = Cell & cr & "<p>" & AddSpan(headline, "ccHeadline") & "</p>"
                        Else
                            Cell = Cell & cr & "<h1>" & headline & "</h1>"
                        End If
                        '
                        ' Add AC end here to force the end of any left over AC tags (like language)
                        '
                        Cell = Cell & ACTagEnd
                    End If
                    '
                    ' ----- Page Copy
                    '
                    'hint = hint & ",044"
                    If Copy = "" Then
                        'hint = hint & ",045"
                        '
                        ' Page copy is empty if  Links Enabled put in a blank line to separate edit from add tag
                        '
                        If c.authContext.isEditing(c, main_RenderCache_CurrentPage_ContentName) Then
                            Body = cr & "<p><!-- Empty Content Placeholder --></p>"
                        End If
                    Else
                        Body = Copy & cr & ACTagEnd
                    End If
                    '
                    ' ----- Wrap content body
                    '
                    Cell = Cell _
                    & cr & "<!-- ContentBoxBodyStart -->" _
                    & genericController.kmaIndent(Body) _
                    & cr & "<!-- ContentBoxBodyEnd -->"
                    '
                    ' ----- Child pages
                    '
                    'hint = hint & ",046"
                    If allowChildListComposite Or c.authContext.isEditingAnything(c) Then
                        'hint = hint & ",047"
                        If Not allowChildListComposite Then
                            Cell = Cell & c.htmlDoc.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.")
                        End If
                        'hint = hint & ",048"
                        ChildListInstanceOptions = genericController.encodeText(cache_pageContent(PCC_ChildListInstanceOptions, main_RenderCache_CurrentPage_PCCPtr))
                        'hint = hint & ",049"
                        Cell = Cell & c.addon.execute_legacy2(c.siteProperties.childListAddonID, "", ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, ContentName, PageID, "", PageChildListInstanceID, False, c.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    End If
                End If
                '
                ' ----- End Text Search
                '
                'hint = hint & ",050"
                s = s _
                & cr & "<!-- TextSearchStart -->" _
                & genericController.kmaIndent(Cell) _
                & cr & "<!-- TextSearchEnd -->"
                '
                ' ----- Page See Also
                '
                If allowSeeAlso Then
                    s = s _
                    & cr & "<div>" _
                    & genericController.kmaIndent(main_GetSeeAlso(c, main_RenderCache_CurrentPage_ContentName, PageID)) _
                    & cr & "</div>"
                End If
                '
                ' ----- Allow More Info
                '
                If (contactMemberID <> 0) And allowMoreInfo Then
                    s = s & cr & "<ac TYPE=""" & ACTypeContact & """>"
                    's = s &  "<p>" & main_GetMoreInfo(ContactMemberID) & "</p>"
                End If
                '
                ' ----- Feedback
                '
                If (Not c.htmlDoc.pageManager_printVersion) And (contactMemberID <> 0) And allowFeedback Then
                    's = s &  "<BR ><img alt=""image"" src=""/ccLib/images/808080.gif"" width=""100%"" height=""1"" >"
                    s = s & cr & "<ac TYPE=""" & ACTypeFeedback & """>"
                    's = s &  main_GetFeedbackForm(ContentName, PageID, ContactMemberID)
                End If
                '
                ' ----- Last Modified line
                '
                'hint = hint & ",060"
                If (LastModified <> Date.MinValue) And allowLastModifiedFooter Then
                    s = s & cr & "<p>This page was last modified " & FormatDateTime(LastModified)
                    If c.authContext.isAuthenticatedAdmin(c) Then
                        If ModifiedBy = 0 Then
                            s = s & " (admin only: modified by unknown)"
                        Else
                            Name = c.content_GetRecordName("people", ModifiedBy)
                            If Name = "" Then
                                s = s & " (admin only: modified by person with unnamed or deleted record #" & ReviewedBy & ")"
                            Else
                                s = s & " (admin only: modified by " & Name & ")"
                            End If
                        End If
                    End If
                    s = s & "</p>"
                End If
                '
                ' ----- Last Reviewed line
                '
                If True Then
                    If (DateReviewed <> Date.MinValue) And allowReviewedFooter Then
                        s = s & cr & "<p>This page was last reviewed " & FormatDateTime(DateReviewed, vbLongDate)
                        If c.authContext.isAuthenticatedAdmin(c) Then
                            If ReviewedBy = 0 Then
                                s = s & " (by unknown)"
                            Else
                                Name = c.content_GetRecordName("people", ReviewedBy)
                                If Name = "" Then
                                    s = s & " (by person with unnamed or deleted record #" & ReviewedBy & ")"
                                Else
                                    s = s & " (by " & Name & ")"
                                End If
                            End If
                            s = s & ".</p>"
                        End If
                    End If
                End If
                '
                ' ----- Page Content Message Footer
                '
                'hint = hint & ",070"
                If allowMessageFooter Then
                    pageContentMessageFooter = c.siteProperties.getText("PageContentMessageFooter", "")
                    If (pageContentMessageFooter <> "") Then
                        s = s & cr & "<p>" & pageContentMessageFooter & "</p>"
                    End If
                End If
            End If
            Call c.db.cs_Close(CS)
            'hint = hint & ",080"
            pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body = s
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body, hint=" & hint)
        End Function
        '
        '=============================================================================
        '   Content Page Authoring
        '
        '   Display Quick Editor for the first active record found
        '   Use for both Root and non-root pages
        '=============================================================================
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing(LiveRecordContentName As String, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, contactMemberID As Integer, ContentName As String, childListSortMethodId As Integer, main_AllowChildListComposite As Boolean, ArchivePage As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing")
            '
            Dim AddonStatusOK As Boolean
            Dim Link As String
            Dim Ptr As Integer
            Dim ParentID As Integer
            Dim PageList As String
            Dim Criteria As String
            Dim RecordActive As Boolean
            Dim Copy As String
            Dim RecordID As Integer
            Dim OptionsPanelAuthoringStatus As String
            Dim ButtonList As String
            'Dim CS as integer
            '
            Dim AllowInsert As Boolean
            Dim AllowCancel As Boolean
            Dim allowSave As Boolean
            Dim AllowDelete As Boolean
            Dim AllowPublish As Boolean
            Dim AllowAbort As Boolean
            Dim AllowSubmit As Boolean
            Dim AllowApprove As Boolean
            Dim AllowMarkReviewed As Boolean

            Dim CDef As coreMetaDataClass.CDefClass
            Dim readOnlyField As Boolean
            '
            Dim IsEditLocked As Boolean
            Dim main_EditLockMemberName As String
            Dim main_EditLockMemberID As Integer
            Dim main_EditLockDateExpires As Date
            '
            Dim IsSubmitted As Boolean
            Dim SubmittedMemberName As String
            Dim SubmittedDate As Date
            '
            Dim IsApproved As Boolean
            Dim ApprovedMemberName As String
            Dim ApprovedDate As Date
            '
            Dim IsModified As Boolean
            Dim ModifiedMemberName As String
            Dim ModifiedDate As Date
            '
            Dim IsDeleted As Boolean
            '
            Dim IsInserted As Boolean
            Dim IsRootPage As Boolean
            '
            Dim IsWorkflowAuthoring As Boolean
            Dim s As String
            Dim PCCPtr As Integer
            Dim ChildListInstanceOptions As String
            '
            Call c.htmlDoc.main_AddStylesheetLink2("/ccLib/styles/ccQuickEdit.css", "Quick Editor")
            '
            ' ----- First Active Record - Output Quick Editor form
            '
            RecordID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
            CDef = c.metaData.getCdef(LiveRecordContentName)
            '
            ' main_Get Authoring Status and permissions
            '
            IsEditLocked = c.workflow.GetEditLockStatus(LiveRecordContentName, RecordID)
            If IsEditLocked Then
                main_EditLockMemberName = c.workflow.GetEditLockMemberName(LiveRecordContentName, RecordID)
                main_EditLockDateExpires = genericController.EncodeDate(c.workflow.GetEditLockMemberName(LiveRecordContentName, RecordID))
            End If
            Call pageManager_GetAuthoringStatus(LiveRecordContentName, RecordID, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
            Call pageManager_GetAuthoringPermissions(LiveRecordContentName, RecordID, AllowInsert, AllowCancel, allowSave, AllowDelete, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, readOnlyField)
            AllowMarkReviewed = c.main_IsContentFieldSupported(ContentName, "DateReviewed")
            OptionsPanelAuthoringStatus = c.main_GetAuthoringStatusMessage(CDef.AllowWorkflowAuthoring, IsEditLocked, main_EditLockMemberName, main_EditLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName)
            IsRootPage = (ParentID = 0)
            If Not IsRootPage Then
                IsRootPage = genericController.EncodeInteger(pageManager_cache_siteSection_RootPageIDIndex.getPtr(CStr(RecordID))) <> -1
                'CSParent = app.csOpen("Site Sections", "RootPageID=" & RecordID, , , , , "ID")
                'IsRootPage = app.csv_IsCSOK(CSParent)
                'Call app.closeCS(CSParent)
                'Call main_testPoint("checking site section -- IsRootPage=" & IsRootPage)
            End If
            '
            ' Set Editing Authoring Control
            '
            Call c.workflow.SetEditLock(LiveRecordContentName, RecordID)
            '
            '
            ' SubPanel: Authoring Status
            '
            If AllowCancel Then
                ButtonList = ButtonList & "," & ButtonCancel
            End If
            If allowSave Then
                ButtonList = ButtonList & "," & ButtonSave & "," & ButtonOK
            End If
            If AllowDelete And Not IsRootPage Then
                ButtonList = ButtonList & "," & ButtonDelete
            End If
            If AllowInsert Then
                ButtonList = ButtonList & "," & ButtonAddChildPage
            End If
            If (ParentID <> 0) And AllowInsert Then
                ButtonList = ButtonList & "," & ButtonAddSiblingPage
            End If
            If AllowPublish Then
                ButtonList = ButtonList & "," & ButtonPublish
            End If
            If AllowAbort Then
                ButtonList = ButtonList & "," & ButtonAbortEdit
            End If
            If AllowSubmit Then
                ButtonList = ButtonList & "," & ButtonPublishSubmit
            End If
            If AllowApprove Then
                ButtonList = ButtonList & "," & ButtonPublishApprove
            End If
            If AllowMarkReviewed Then
                ButtonList = ButtonList & "," & ButtonMarkReviewed
            End If
            If ButtonList <> "" Then
                ButtonList = Mid(ButtonList, 2)
                ButtonList = c.main_GetPanelButtons(ButtonList, "Button")
            End If
            If OptionsPanelAuthoringStatus <> "" Then
                s = s & "" _
            & cr & "<tr>" _
            & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
            & cr & "</tr>"
            End If
            If c.error_IsUserError() Then
                s = s & "" _
            & cr & "<tr>" _
            & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & c.error_GetUserError() & "</div></td>" _
            & cr & "</tr>"
            End If
            s = s _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Name</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & c.htmlDoc.html_GetFormInputText2("name", genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr)), 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Headline</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & c.htmlDoc.html_GetFormInputText2("headline", genericController.encodeText(cache_pageContent(PCC_Headline, main_RenderCache_CurrentPage_PCCPtr)), 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & ""
            If readOnlyField Then
                s = s & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:34px;"">Body</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
            & cr & "</tr>"
            Else
                s = s & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:111px;"">Body</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
            & cr & "</tr>"
            End If
            '
            ' ----- Parent pages
            '
            If main_RenderCache_ParentBranch_PCCPtrCnt = 0 Then
                PageList = "&nbsp;(there are no parent pages)"
            Else
                PageList = "<ul class=""qeListUL""><li class=""qeListLI"">Current Page</li></ul>"
                For Ptr = 0 To main_RenderCache_ParentBranch_PCCPtrCnt - 1
                    PCCPtr = genericController.EncodeInteger(main_RenderCache_ParentBranch_PCCPtrs(Ptr))
                    Link = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                    If Link = "" Then
                        Link = "no name #" & genericController.encodeText(cache_pageContent(PCC_ID, PCCPtr))
                    End If
                    Link = "<a href=""" & genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr)) & """>" & Link & "</a>"
                    PageList = "<ul class=""qeListUL""><li class=""qeListLI"">" & Link & PageList & "</li></ul>"
                Next
            End If
            s = s & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:26px;"">Parent Pages</td>" _
            & cr2 & "<td class=""qeRow qeRight""><div class=""qeListCon"">" & PageList & "</div></td>" _
            & cr & "</tr>"
            '
            ' ----- Child pages
            '
            ChildListInstanceOptions = genericController.encodeText(cache_pageContent(PCC_ChildListInstanceOptions, main_RenderCache_CurrentPage_PCCPtr))
            PageList = c.addon.execute_legacy2(c.siteProperties.childListAddonID, "", ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, ContentName, RecordID, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
            If genericController.vbInstr(1, PageList, "<ul", vbTextCompare) = 0 Then
                PageList = "(there are no child pages)"
            End If
            s = s _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:36px;"">Child Pages</td>" _
            & cr2 & "<td class=""qeRow qeRight""><div class=""qeListCon"">" & PageList & "</div></td>" _
            & cr & "</tr>"
            s = "" _
            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
            & genericController.kmaIndent(s) _
            & cr & "</table>"
            s = "" _
            & ButtonList _
            & s _
            & ButtonList
            s = c.main_GetPanel(s)

            '
            ' Form Wrapper
            '
            s = "" _
            & cr & c.htmlDoc.html_GetUploadFormStart(c.webServer.requestQueryString) _
            & cr & c.htmlDoc.html_GetFormInputHidden("Type", FormTypePageAuthoring) _
            & cr & c.htmlDoc.html_GetFormInputHidden("ID", RecordID) _
            & cr & c.htmlDoc.html_GetFormInputHidden("ContentName", LiveRecordContentName) _
            & cr & c.main_GetPanelHeader("Contensive Quick Editor") _
            & cr & s _
            & cr & c.htmlDoc.html_GetUploadFormEnd()

            pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing = "" _
            & cr & "<div class=""ccCon"">" _
            & genericController.kmaIndent(s) _
            & cr & "</div>"
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(ByVal ContentName As String, ByVal OrderByClause As String, ByVal AllowChildList As Boolean, ByVal Authoring As Boolean, ByVal rootPageId As Integer, ByVal readOnlyField As Boolean, ByVal AllowReturnLink As Boolean, ByVal RootPageContentName As String, ByVal ArchivePage As Boolean, ByVal contactMemberID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body")
            '
            Dim FieldRows As Integer
            Dim PageID As Integer
            Dim parentPageID As Integer
            Dim Copy As String
            Dim copyFilename As String
            Dim Stream As String
            Dim DelimiterPosition As Integer
            Dim dateArchive As Date
            Dim addonListJSON As String
            Dim styleList As String
            Dim styleOptionList As String
            Const InputTextWidth = 60
            '
            PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
            parentPageID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
            dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, main_RenderCache_CurrentPage_PCCPtr))
            copyFilename = genericController.encodeText(cache_pageContent(PCC_CopyFilename, main_RenderCache_CurrentPage_PCCPtr))
            If copyFilename <> "" Then
                Copy = c.cdnFiles.readFile(copyFilename)
            End If
            '
            ' ----- Page Copy
            '
            FieldRows = genericController.EncodeInteger(c.properties_user_getText(ContentName & ".copyFilename.PixelHeight", "500"))
            If FieldRows < 50 Then
                FieldRows = 50
                Call c.userProperty.setProperty(ContentName & ".copyFilename.PixelHeight", 50)
            End If
            addonListJSON = c.htmlDoc.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
            '
            ' At this point we do now know the the template so we can not main_Get the stylelist.
            ' Put in main_fpo_QuickEditing to be replaced after template known
            '
            c.htmlDoc.html_quickEdit_copy = Copy
            Copy = html_quickEdit_fpo
            Stream = Stream & Copy
            '
            pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body = Stream
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body")
        End Function
        '
        '=============================================================================
        ' pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink
        '=============================================================================
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink(RootPageContentName As String, ignore As Integer, rootPageId As Integer, ParentIDPath As String, ArchivePage As Boolean, BreadCrumbDelimiter As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink")
            '
            Dim pageCaption As String
            Dim Link As String
            Dim parentBranchPtr As Integer
            Dim PCCPtr As Integer
            Dim PageID As Integer
            Dim returnHtml As String
            '
            returnHtml = ""
            '
            parentBranchPtr = 0
            Do While (parentBranchPtr < main_RenderCache_ParentBranch_PCCPtrCnt)
                PCCPtr = genericController.EncodeInteger(main_RenderCache_ParentBranch_PCCPtrs(parentBranchPtr))
                PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                pageCaption = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
                If pageCaption = "" Then
                    pageCaption = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                End If
                Link = pageManager_GetPageLink4(PageID, "", True, False)
                If returnHtml <> "" Then
                    returnHtml = BreadCrumbDelimiter & returnHtml
                End If
                returnHtml = "<a href=""" & c.htmlDoc.html_EncodeHTML(Link) & """>" & pageCaption & "</a>" & returnHtml
                parentBranchPtr = parentBranchPtr + 1
            Loop
            '
            pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink = returnHtml
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink")
        End Function
        '
        '
        '
        Private Function pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow(ByVal Caption As String, ByVal Result As String, ByVal EvenRow As Boolean) As String
            '
            Dim CopyCaption As String
            Dim CopyResult As String
            '
            CopyCaption = Caption
            If CopyCaption = "" Then
                CopyCaption = "&nbsp;"
            End If
            '
            CopyResult = Result
            If CopyResult = "" Then
                CopyResult = "&nbsp;"
            End If
            '
            pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow = genericController.GetTableCell("<nobr>" & CopyCaption & "</nobr>", "150", , EvenRow, "right") & genericController.GetTableCell(CopyResult, "100%", , EvenRow, "left") & kmaEndTableRow
        End Function
        '
        '=============================================================================
        '   Add content padding around content
        '       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
        '=============================================================================
        '
        Private Function pageManager_GetContentBoxWrapper(ByVal Content As String, ByVal ContentPadding As Integer) As String
            'dim buildversion As String
            '
            ' BuildVersion = app.dataBuildVersion
            pageManager_GetContentBoxWrapper = Content
            If c.siteProperties.getBoolean("Compatibility ContentBox Pad With Table") Then
                '
                If ContentPadding > 0 Then
                    '
                    pageManager_GetContentBoxWrapper = "" _
                        & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0>" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td style=""padding:" & ContentPadding & "px"">" _
                        & genericController.kmaIndent(genericController.kmaIndent(genericController.kmaIndent(pageManager_GetContentBoxWrapper))) _
                        & cr3 & "</td>" _
                        & cr2 & "</tr>" _
                        & cr & "</table>"
                    '            main_GetContentBoxWrapper = "" _
                    '                & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=" & ContentPadding & ">" _
                    '                & cr2 & "<tr>" _
                    '                & cr3 & "<td>" _
                    '                & genericController.kmaIndent(KmaIndent(KmaIndent(main_GetContentBoxWrapper))) _
                    '                & cr3 & "</td>" _
                    '                & cr2 & "</tr>" _
                    '                & cr & "</table>"
                End If
                pageManager_GetContentBoxWrapper = "" _
                    & cr & "<div class=""contentBox"">" _
                    & genericController.kmaIndent(pageManager_GetContentBoxWrapper) _
                    & cr & "</div>"
            Else
                '
                pageManager_GetContentBoxWrapper = "" _
                    & cr & "<div class=""contentBox"" style=""padding:" & ContentPadding & "px"">" _
                    & genericController.kmaIndent(pageManager_GetContentBoxWrapper) _
                    & cr & "</div>"
            End If
        End Function
        ' $$$$$ not used
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'Private Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_BodyInput(Caption As String, FormInput As String) As String
        '    pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_BodyInput = "" _
        '        & "<table border=""0"" cellpadding=""2"" cellspacing=""1"" width=""100%""><tr>" _
        '        & "<td width=""150"" align=""right"" valign=""middle"">" & addSpan(Caption & "<BR ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""150"" height=""1"">", "ccAdminSmall") & "</td>" _
        '        & "<td align=""LEFT"" class=""ccPanelInput"">" & addSpan(FormInput, "ccAdminNormal") & "</td>" _
        '        & "</tr></table>"
        'End Function
        '
        '========================================================================
        ' ----- Process the reply from the Authoring Tools Panel form
        '========================================================================
        '
        Public Sub pageManager_ProcessFormQuickEditing()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00363")
            '
            Dim RecordParentID As Integer
            Dim SaveButNoChanges As Boolean
            Dim RequestName As String
            Dim ParentID As Integer
            Dim Link As String
            Dim FieldName As String
            Dim CSBlock As Integer
            Dim MethodName As String
            Dim ContentName As String
            Dim Filename As String
            Dim Copy As String
            Dim Button As String
            'Dim CopyEditMode as integer
            Dim RecordID As Integer
            Dim RecordModified As Boolean
            Dim RecordName As String
            '
            Dim IsEditLocked As Boolean
            Dim IsSubmitted As Boolean
            Dim IsApproved As Boolean
            Dim IsInserted As Boolean
            Dim IsDeleted As Boolean
            Dim IsModified As Boolean
            Dim main_EditLockMemberName As String
            Dim main_EditLockMemberID As Integer
            Dim main_EditLockDateExpires As Date
            Dim SubmittedMemberName As String
            Dim ApprovedMemberName As String
            Dim ModifiedMemberName As String
            Dim ModifiedDate As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            Dim allowSave As Boolean
            Dim iIsAdmin As Boolean
            Dim main_WorkflowSupport As Boolean
            '
            MethodName = "pageManager_ProcessFormQuickEditing()"
            '
            RecordModified = False
            RecordID = (c.docProperties.getInteger("ID"))
            ContentName = c.docProperties.getText("ContentName")
            Button = c.docProperties.getText("Button")
            iIsAdmin = c.authContext.isAuthenticatedAdmin(c)
            '
            If (Button <> "") And (RecordID <> 0) And (ContentName <> "") And (c.authContext.isAuthenticatedContentManager(c, ContentName)) Then
                main_WorkflowSupport = c.siteProperties.allowWorkflowAuthoring And c.workflow.isWorkflowAuthoringCompatible(ContentName)
                Call pageManager_GetAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
                IsEditLocked = c.workflow.GetEditLockStatus(ContentName, RecordID)
                main_EditLockMemberName = c.workflow.GetEditLockMemberName(ContentName, RecordID)
                main_EditLockDateExpires = c.workflow.GetEditLockDateExpires(ContentName, RecordID)
                Call c.workflow.ClearEditLock(ContentName, RecordID)
                '
                ' tough case, in Quick mode, lets mark the record reviewed, no matter what button they push, except cancel
                '
                If Button <> ButtonCancel Then
                    Call pageManager_MarkRecordReviewed(ContentName, RecordID)
                End If
                '
                ' Determine is the record should be saved
                '
                If (Not IsApproved) And (Not c.docProperties.getBoolean("RENDERMODE")) Then
                    If iIsAdmin Then
                        '
                        ' cases that admin can save
                        '
                        allowSave = False _
                            Or (Button = ButtonAddChildPage) _
                            Or (Button = ButtonAddSiblingPage) _
                            Or (Button = ButtonSave) _
                            Or (Button = ButtonOK) _
                            Or (Button = ButtonPublish) _
                            Or (Button = ButtonPublishSubmit) _
                            Or (Button = ButtonPublishApprove)
                    Else
                        '
                        ' cases that CM can save
                        '
                        allowSave = False _
                            Or (Button = ButtonAddChildPage) _
                            Or (Button = ButtonAddSiblingPage) _
                            Or (Button = ButtonSave) _
                            Or (Button = ButtonOK) _
                            Or (Button = ButtonPublishSubmit)
                    End If
                End If
                If allowSave Then
                    '
                    ' ----- Save Changes
                    '
                    SaveButNoChanges = True
                    RequestName = c.docProperties.getText("name")
                    If Trim(RequestName) = "" Then
                        Call c.error_AddUserError("A name is required to save this page")
                    Else
                        CSBlock = c.csOpenRecord(ContentName, RecordID, True, True)
                        If c.db.cs_ok(CSBlock) Then
                            FieldName = "copyFilename"
                            Copy = c.docProperties.getText(FieldName)
                            Copy = c.htmlDoc.html_DecodeContent(Copy)
                            If Copy <> c.db.cs_get(CSBlock, "copyFilename") Then
                                Call c.db.cs_set(CSBlock, "copyFilename", Copy)
                                SaveButNoChanges = False
                            End If
                            RecordName = c.docProperties.getText("name")
                            If RecordName <> c.db.cs_get(CSBlock, "name") Then
                                Call c.db.cs_set(CSBlock, "name", RecordName)
                                SaveButNoChanges = False
                            End If
                            Call c.main_AddLinkAlias(RecordName, RecordID, "")
                            If (c.docProperties.getText("headline") <> c.db.cs_get(CSBlock, "headline")) Then
                                Call c.db.cs_set(CSBlock, "headline", c.docProperties.getText("headline"))
                                SaveButNoChanges = False
                            End If
                            RecordParentID = c.db.cs_getInteger(CSBlock, "parentid")
                        End If
                        Call c.db.cs_Close(CSBlock)
                        '
                        Call c.workflow.SetEditLock(ContentName, RecordID)
                        '
                        If Not SaveButNoChanges Then
                            Call c.main_ProcessSpecialCaseAfterSave(False, ContentName, RecordID, RecordName, RecordParentID, False)
                            Call pageManager_cache_pageContent_clear()
                            Call c.cache.invalidateContent(ContentName)
                        End If
                    End If
                End If
                If (Button = ButtonAddChildPage) Then
                    '
                    '
                    '
                    CSBlock = c.db.cs_insertRecord(ContentName)
                    If c.db.cs_ok(CSBlock) Then
                        Call c.db.cs_set(CSBlock, "active", True)
                        Call c.db.cs_set(CSBlock, "ParentID", RecordID)
                        Call c.db.cs_set(CSBlock, "contactmemberid", c.authContext.user.ID)
                        Call c.db.cs_set(CSBlock, "name", "New Page added " & c.app_startTime & " by " & c.authContext.user.Name)
                        Call c.db.cs_set(CSBlock, "copyFilename", "")
                        RecordID = c.db.cs_getInteger(CSBlock, "ID")
                        Call c.db.cs_save2(CSBlock)
                        '
                        Link = pageManager_GetPageLink4(RecordID, "", True, False)
                        'Link = main_GetPageLink(RecordID)
                        If main_WorkflowSupport Then
                            If Not pagemanager_IsWorkflowRendering() Then
                                Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                                Call c.siteProperties.setProperty("AllowWorkflowRendering", True)
                            End If
                        End If
                        Call c.webServer.webServerIO_Redirect2(Link, "Redirecting because a new page has been added with the quick editor.", False)
                    End If
                    Call c.db.cs_Close(CSBlock)
                    '
                    'Call AppendLog("pageManager_ProcessFormQuickEditor, 7-call pageManager_cache_pageContent_clear")
                    Call pageManager_cache_pageContent_clear()
                    Call c.cache.invalidateContent(ContentName)
                End If
                If (Button = ButtonAddSiblingPage) Then
                    '
                    '
                    '
                    CSBlock = c.csOpen(ContentName, RecordID, , , "ParentID")
                    If c.db.cs_ok(CSBlock) Then
                        ParentID = c.db.cs_getInteger(CSBlock, "ParentID")
                    End If
                    Call c.db.cs_Close(CSBlock)
                    If ParentID <> 0 Then
                        CSBlock = c.db.cs_insertRecord(ContentName)
                        If c.db.cs_ok(CSBlock) Then
                            Call c.db.cs_set(CSBlock, "active", True)
                            Call c.db.cs_set(CSBlock, "ParentID", ParentID)
                            Call c.db.cs_set(CSBlock, "contactmemberid", c.authContext.user.ID)
                            Call c.db.cs_set(CSBlock, "name", "New Page added " & c.app_startTime & " by " & c.authContext.user.Name)
                            Call c.db.cs_set(CSBlock, "copyFilename", "")
                            RecordID = c.db.cs_getInteger(CSBlock, "ID")
                            Call c.db.cs_save2(CSBlock)
                            '
                            Link = pageManager_GetPageLink4(RecordID, "", True, False)
                            'Link = main_GetPageLink(RecordID)
                            If main_WorkflowSupport Then
                                If Not pagemanager_IsWorkflowRendering() Then
                                    Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                                    Call c.siteProperties.setProperty("AllowWorkflowRendering", True)
                                End If
                            End If
                            Call c.webServer.webServerIO_Redirect2(Link, "Redirecting because a new page has been added with the quick editor.", False)
                        End If
                        Call c.db.cs_Close(CSBlock)
                    End If
                    '
                    'Call AppendLog("pageManager_ProcessFormQuickEditor, 8-call pageManager_cache_pageContent_clear")
                    Call pageManager_cache_pageContent_clear()
                    Call c.cache.invalidateContent(ContentName)
                End If
                If (Button = ButtonDelete) Then
                    CSBlock = c.csOpen(ContentName, RecordID)
                    If c.db.cs_ok(CSBlock) Then
                        ParentID = c.db.cs_getInteger(CSBlock, "parentid")
                    End If
                    Call c.db.cs_Close(CSBlock)
                    '
                    Call pageManager_DeleteChildRecords(ContentName, RecordID, False)
                    Call c.DeleteContentRecord(ContentName, RecordID)
                    '
                    If Not main_WorkflowSupport Then
                        'Call AppendLog("pageManager_ProcessFormQuickEditor, 9-call pageManager_cache_pageContent_clear")
                        Call pageManager_cache_pageContent_clear()
                        Call c.cache.invalidateContent(ContentName)
                    End If
                    '
                    If Not main_WorkflowSupport Then
                        Link = pageManager_GetPageLink4(ParentID, "", True, False)
                        'Link = main_GetPageLink(ParentID)
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", True)
                        Call c.webServer.webServerIO_Redirect2(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", pageManager_RedirectBecausePageNotFound)
                        Exit Sub
                    End If
                End If
                '
                If (Button = ButtonAbortEdit) Then
                    Call c.workflow.abortEdit2(ContentName, RecordID, c.authContext.user.ID)
                End If
                If (Button = ButtonPublishSubmit) Then
                    Call c.workflow.main_SubmitEdit(ContentName, RecordID)
                    Call pageManager_SendPublishSubmitNotice(ContentName, RecordID, "")
                End If
                If (Not c.error_IsUserError()) And ((Button = ButtonOK) Or (Button = ButtonCancel) Or (Button = ButtonPublish)) Then
                    '
                    ' ----- Turn off Quick Editor if not save or add child
                    '
                    Call c.visitProperty.setProperty("AllowQuickEditor", "0")
                End If
                If iIsAdmin Then
                    '
                    ' ----- Admin only functions
                    '
                    If (Button = ButtonPublish) Then
                        Call c.workflow.publishEdit(ContentName, RecordID)
                        Call c.cache.invalidateContent(ContentName)
                    End If
                    If (Button = ButtonPublishApprove) Then
                        Call c.workflow.approveEdit(ContentName, RecordID)
                    End If
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError13(MethodName)
        End Sub
        '
        '======================================================================================
        '   main_Get a dynamic menu from Page Content
        '======================================================================================
        '
        Private Function pageManager_GetSectionMenu_NameMenu(ByVal PageName As String, ByVal ContentName As String, ByVal DefaultLink As String, ByVal RootPageRecordID As Integer, ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal MenuImage As String, ByVal MenuImageOver As String, ByVal RootMenuCaption As String, ByVal SectionID As Integer, ByVal UseContentWatchLink As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00364")
            '
            Dim AllowInMenus As Boolean
            Dim PCCPtr As Integer
            Dim PageFound As Boolean
            Dim ChildPageCount As Integer
            Dim ChildPagesFoundTest As String
            Dim AddRootButton As Boolean
            Dim TopMenuCaption As String
            Dim Tier1MenuCaption As String
            '
            Dim CSPointer As Integer
            Dim MenuID As Integer
            Dim ContentID As Integer
            Dim BakeName As String
            Dim Criteria As String
            Dim MenuNamePrefix As String
            Dim childListSortMethodId As Integer
            Dim LinkWorking As String
            Dim ParentID As Integer
            Dim templateId As Integer
            Dim ContentControlID As Integer
            Dim allowChildListDisplay As Boolean
            Dim MenuLinkOverRide As String
            Dim ChildPagesFound As Boolean
            Dim FieldList As String
            Dim DateExpires As Date
            Dim dateArchive As Date
            Dim PubDate As Date
            '
            '
            '
            If (PageName = "") Or (ContentName = "") Then
                Call Err.Raise(ignoreInteger, "dll", "main_GetPageMenu requires a valid page name and content name")
            Else
                '
                ' ----- Read Bake Version
                '
                BakeName = "main_GetMenu-" & c.webServer.webServerIO_requestProtocol & "-" & c.webServer.requestDomain & "-" & PageName & "-" & ContentName & "-" & DefaultLink & "-" & RootPageRecordID & "-" & DepthLimit & "-" & MenuStyle & "-" & StyleSheetPrefix
                BakeName = genericController.vbReplace(BakeName, "/", "_")
                BakeName = genericController.vbReplace(BakeName, ":", "_")
                BakeName = genericController.vbReplace(BakeName, ".", "_")
                BakeName = genericController.vbReplace(BakeName, " ", "_")
                pageManager_GetSectionMenu_NameMenu = genericController.encodeText(c.cache.getObject(Of String)(BakeName))
                If pageManager_GetSectionMenu_NameMenu <> "" Then
                    pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu
                Else
                    '
                    ' ----- Add Root Page to Menu System
                    '
                    If RootPageRecordID > 0 Then
                        PCCPtr = pageManager_cache_pageContent_getPtr(RootPageRecordID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                        'Criteria = "(ID=" & encodeSQLNumber(RootPageRecordID) & ")"
                    Else
                        PCCPtr = pageManager_cache_pageContent_getFirstNamePtr(PageName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                        'Criteria = "(name=" & encodeSQLText(PageName) & ")"
                    End If
                    '
                    ' Skip over expired, archive and non-published
                    '
                    PageFound = False
                    Do While PCCPtr >= 0 And Not PageFound
                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
                        PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > c.app_startTime)) And ((PubDate = Date.MinValue) Or (PubDate < c.app_startTime))
                        'PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime)) And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime))
                        If (Not PageFound) Then
                            If (RootPageRecordID = 0) Then
                                PCCPtr = pageManager_cache_pageContent_nameIndex.getNextPtr
                            Else
                                PCCPtr = -1
                            End If
                        End If
                    Loop
                    If Not PageFound Then
                        '
                        ' menu root was not found, just put up what we have. If the link is there, the page will be created
                        '
                        AllowInMenus = True
                        LinkWorking = DefaultLink
                        LinkWorking = genericController.EncodeAppRootPath(LinkWorking, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                        LinkWorking = genericController.modifyLinkQuery(LinkWorking, "bid", "", False)
                        MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
                        MenuID = 0
                        childListSortMethodId = 0
                        ParentID = 0
                        templateId = 0
                        allowChildListDisplay = False
                        MenuLinkOverRide = ""
                        ChildPagesFound = False
                    Else
                        AllowInMenus = genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, PCCPtr))
                        If AllowInMenus Then
                            MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
                            MenuID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                            childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
                            Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
                            If Tier1MenuCaption = "" Then
                                Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Headline, PCCPtr))
                                If Tier1MenuCaption = "" Then
                                    Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                                    If Tier1MenuCaption = "" Then
                                        Tier1MenuCaption = "Page " & CStr(MenuID)
                                    End If
                                End If
                            End If
                            ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                            templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                            allowChildListDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
                            MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
                            ChildPagesFoundTest = cache_pageContent(PCC_ChildPagesFound, PCCPtr)
                            If ChildPagesFoundTest = "" Then
                                '
                                ' Not initialized, assume true
                                '
                                ChildPagesFound = True
                            Else
                                ChildPagesFound = genericController.EncodeBoolean(ChildPagesFoundTest)
                            End If
                            '
                            ' Use parentid to detect if this record needs to be called with the bid
                            '
                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
                            '
                            ' main_Get the Link
                            '
                            LinkWorking = pageManager_GetPageLink4(MenuID, "", True, False)
                            'LinkWorking = main_GetPageDynamicLinkWithArgs(ContentControlID, MenuID, DefaultLink, True, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
                        End If
                    End If
                    '
                    If AllowInMenus Then
                        '
                        ' ----- Set Tier1 Menu Caption (top element of the first flyout panel)
                        '
                        If Tier1MenuCaption = "" Then
                            Tier1MenuCaption = RootMenuCaption
                            If Tier1MenuCaption = "" Then
                                Tier1MenuCaption = PageName
                            End If
                        End If
                        '
                        ' ----- Set Top Menu Caption (clickable label that opens the menus)
                        '
                        TopMenuCaption = RootMenuCaption
                        If TopMenuCaption = "" Then
                            TopMenuCaption = Tier1MenuCaption
                        End If
                        '
                        If LinkWorking = "" Then
                            '
                            ' ----- Blank LinkWorking, this entry has no link
                            ' ----- Add menu header, and first entry for the root page
                            '
                            Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, "", MenuImage, MenuImageOver, , TopMenuCaption)
                            '
                            ' ----- Root menu only, add a repeat of the button to the first menu
                            '
                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
                                '
                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
                                '
                                Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID & ".entry", MenuNamePrefix & MenuID, , , , Tier1MenuCaption)
                            End If
                        Else
                            '
                            ' ----- LinkWorking is here, put MenuID on the end of it
                            ' ----- Add menu header, and first entry for the root page
                            '
                            Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
                            '
                            ' ----- Root menu only, add a repeat of the button to the first menu
                            '
                            AddRootButton = False
                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
                                '
                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
                                '
                                AddRootButton = True
                                If ParentID <> 0 Then
                                    '
                                    ' This Top-most page is not the RootPage, include the bid
                                    '
                                Else
                                    '
                                    ' This Top-most page is the RootPage, include no bid
                                    '
                                End If
                            End If
                        End If
                        ' ##### can not block, this is being used
                        If ChildPagesFound Then
                            ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(MenuID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
                            If (ChildPageCount = 0) And (True) Then
                                Call c.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & MenuID)
                            End If
                        End If
                        pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu & genericController.vbReplace(c.menuFlyout.getMenu(MenuNamePrefix & genericController.encodeText(MenuID), MenuStyle, StyleSheetPrefix), vbCrLf, "")
                        pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu & c.htmlDoc.menu_GetClose()
                        Call c.cache.setObject(BakeName, pageManager_GetSectionMenu_NameMenu, ContentName & ",Site Sections,Dynamic Menus,Dynamic Menu Section Rules")
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("main_GetSectionMenu_NameMenu")
        End Function
        '
        '======================================================================================
        '   main_Get a dynamic menu from Page Content
        '======================================================================================
        '
        Private Function pageManager_GetSectionMenu_IdMenu(ByVal RootPageRecordID As Integer, ByVal DefaultLink As String, ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal MenuImage As String, ByVal MenuImageOver As String, ByVal RootMenuCaption As String, ByVal SectionID As Integer, ByVal UseContentWatchLink As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("PageList_GetPageIDMenu")
            '
            Dim PseudoChildPagesFound As Boolean
            Dim AllowInMenus As Boolean
            Dim DateExpires As Date
            Dim dateArchive As Date
            Dim PubDate As Date
            Dim PCCPtr As Integer
            Dim PageFound As Boolean
            Dim ChildPageCount As Integer
            Dim ContentName As String
            Dim AddRootButton As Boolean
            Dim TopMenuCaption As String
            Dim Tier1MenuCaption As String
            '
            Dim CSPointer As Integer
            Dim PageID As Integer
            Dim ContentID As Integer
            Dim BakeName As String
            Dim Criteria As String
            Dim MenuNamePrefix As String
            Dim childListSortMethodId As Integer
            Dim LinkWorking As String
            Dim LinkWorkingNoRedirect As String
            Dim ParentID As Integer
            Dim templateId As Integer
            Dim ContentControlID As Integer
            Dim allowChildListDisplay As Boolean
            Dim MenuLinkOverRide As String
            Dim ChildPagesFound As Boolean
            Dim FieldList As String
            Dim ChildPagesFoundTest As String
            '
            If RootPageRecordID = 610 Then
                RootPageRecordID = RootPageRecordID
            End If
            '
            '$$$$$ cache this - somewhere in here it opens cs with contentname
            ContentName = "Page Content"
            If False Then
                Call Err.Raise(ignoreInteger, "dll", "main_GetPageMenu requires a valid page name and content name")
            Else
                '
                ' ----- Read Bake Version
                '
                BakeName = "main_GetMenu-" & c.webServer.webServerIO_requestProtocol & "-" & c.webServer.requestDomain & "-" & RootPageRecordID & "-" & DefaultLink & "-" & DepthLimit & "-" & MenuStyle & "-" & StyleSheetPrefix
                BakeName = genericController.vbReplace(BakeName, "/", "_")
                BakeName = genericController.vbReplace(BakeName, ":", "_")
                BakeName = genericController.vbReplace(BakeName, ".", "_")
                BakeName = genericController.vbReplace(BakeName, " ", "_")
                pageManager_GetSectionMenu_IdMenu = genericController.encodeText(c.cache.getObject(Of String)(BakeName))
                If pageManager_GetSectionMenu_IdMenu <> "" Then
                    pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu
                Else
                    '
                    ' ----- Add Root Page to Menu System
                    '
                    PCCPtr = pageManager_cache_pageContent_getPtr(RootPageRecordID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                    PageFound = (PCCPtr >= 0)
                    '
                    ' Skip if expired, archive and non-published
                    '
                    If PageFound Then
                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
                        PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > c.app_startTime)) And ((PubDate = Date.MinValue) Or (PubDate < c.app_startTime))
                        'PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime)) And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime))
                    End If
                    If Not PageFound Then
                        '
                        ' menu root was not found, just put up what we have. If the link is there, the page will be created
                        '
                        AllowInMenus = True
                        LinkWorking = DefaultLink
                        LinkWorkingNoRedirect = LinkWorking
                        LinkWorking = genericController.EncodeAppRootPath(LinkWorking, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                        LinkWorking = genericController.modifyLinkQuery(LinkWorking, "bid", "", False)
                        MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
                        ' ***** just want to know what would happen here
                        PageID = RootPageRecordID
                        'pageId = 0
                        childListSortMethodId = 0
                        ParentID = 0
                        templateId = 0
                        allowChildListDisplay = False
                        MenuLinkOverRide = ""
                        ChildPagesFound = False
                    Else
                        '
                        ' AllowInMenus does not work for root pages, which are the only pages being handled here. This menu is hidden from the section record
                        '
                        AllowInMenus = True
                        If AllowInMenus Then
                            MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
                            PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                            childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
                            Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
                            If Tier1MenuCaption = "" Then
                                Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                                If Tier1MenuCaption = "" Then
                                    Tier1MenuCaption = "Page " & CStr(PageID)
                                End If
                            End If
                            ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                            templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                            allowChildListDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
                            MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
                            ChildPagesFoundTest = genericController.encodeText(cache_pageContent(PCC_ChildPagesFound, PCCPtr))
                            If ChildPagesFoundTest = "" Then
                                '
                                ' Not initialized, assume true
                                '
                                ChildPagesFound = True
                            Else
                                ChildPagesFound = genericController.EncodeBoolean(ChildPagesFoundTest)
                            End If
                            '
                            ' Use parentid to detect if this record needs to be called with the bid
                            '
                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
                            '
                            ' main_Get the Link
                            '
                            '1/13/2010 - convert everything to use linkalias and issecure
                            'LinkWorkingNoRedirect = main_GetPageLink4()
                            LinkWorkingNoRedirect = pageManager_GetPageLink4(PageID, "", True, False)
                            '                    LinkWorkingNoRedirect = main_GetPageDynamicLinkWithArgs(contentcontrolid, pageId, DefaultLink, True, TemplateID, SectionID, "", UseContentWatchLink)
                            LinkWorking = LinkWorkingNoRedirect
                            '                    If MenuLinkOverRide <> "" Then
                            '                        LinkWorking = "?rc=" & contentcontrolid & "&ri=" & pageId
                            '                    End If
                            'LinkWorking = main_GetPageDynamicLinkWithArgs(ContentControlID, pageId, DefaultLink, True, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
                        End If
                    End If
                    '
                    If AllowInMenus Then
                        '
                        ' ----- Set Tier1 Menu Caption (top element of the first flyout panel)
                        '
                        If Tier1MenuCaption = "" Then
                            Tier1MenuCaption = RootMenuCaption
                        End If
                        '
                        ' ----- Set Top Menu Caption (clickable label that opens the menus)
                        '
                        TopMenuCaption = RootMenuCaption
                        If TopMenuCaption = "" Then
                            TopMenuCaption = Tier1MenuCaption
                        End If
                        '
                        If LinkWorking = "" Then
                            '
                            ' ----- Blank LinkWorking, this entry has no link
                            ' ----- Add menu header, and first entry for the root page
                            '
                            Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, , TopMenuCaption)
                            '
                            ' ----- Root menu only, add a repeat of the button to the first menu
                            '
                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
                                '
                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
                                '
                                Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID & ".entry", MenuNamePrefix & PageID, , , , Tier1MenuCaption)
                            End If
                        Else
                            '
                            ' ----- LinkWorking is here, put pageId on the end of it
                            ' ----- Add menu header, and first entry for the root page
                            '
                            If PageID <> 0 Then
                                Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
                            ElseIf (SectionID <> 0) And (RootPageRecordID <> 0) Then
                                Dim CSSection As Integer
                                Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & RootPageRecordID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
                                'Dim linkAlias
                            Else
                                Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
                            End If
                            '
                            ' ----- Root menu only, add a repeat of the button to the first menu
                            '
                            AddRootButton = False
                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
                                '
                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
                                '
                                AddRootButton = True
                                If ParentID <> 0 Then
                                    '
                                    ' This Top-most page is not the RootPage, include the bid
                                    '
                                Else
                                    '
                                    ' This Top-most page is the RootPage, include no bid
                                    '
                                End If
                            End If
                        End If
                        '
                        ' 9/18/2009 - Build Submenu if child pages found
                        '
                        If pagemanager_IsWorkflowRendering() Then
                            '
                            ' If workflow mode, just assume there are child pages
                            '
                            ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(PageID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(PageID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
                        Else
                            '
                            ' In production mode, use the ChildPagesFound field
                            '
                            PseudoChildPagesFound = ChildPagesFound
                            If Not PseudoChildPagesFound Then
                                '
                                ' Even when child pages is false, try it 10% of the time anyway
                                '
                                Randomize()
                                PseudoChildPagesFound = (Rnd() > 0.8)
                                If PseudoChildPagesFound Then
                                    TopMenuCaption = TopMenuCaption
                                End If
                            End If
                            If PseudoChildPagesFound Then
                                '
                                ' Child pages were found, create child menu
                                '
                                ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(PageID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(PageID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
                                'ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(pageId, ContentName, LinkWorkingNoRedirect, Tier1MenuCaption, "," & genericController.encodeText(pageId), MenuNamePrefix, 1, DepthLimit, ChildListSortMethodID, SectionID, AddRootButton, UseContentWatchLink)
                                If (True) Then
                                    If (ChildPageCount = 0) And (ChildPagesFound) Then
                                        '
                                        ' ChildPagesFound flag is true, but no pages were found - clear flag
                                        '
                                        Call c.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & PageID)
                                        'Call AppendLog("main_GetSectionMenu_IdMenu, 4-call pageManager_cache_pageContent_updateRow")
                                        Call pageManager_cache_pageContent_updateRow(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                                    ElseIf (ChildPageCount > 0) And (Not ChildPagesFound) Then
                                        '
                                        ' ChildPagesFlag is cleared, but pages were found -- set the flag
                                        '
                                        Call c.db.executeSql("update ccpagecontent set ChildPagesFound=1 where id=" & PageID)
                                        'Call AppendLog("main_GetSectionMenu_IdMenu, 5-call pageManager_cache_pageContent_updateRow")
                                        Call pageManager_cache_pageContent_updateRow(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                                    End If
                                End If
                            End If
                        End If
                        '
                        ' ----- main_Get the Menu Header
                        '
                        pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu & genericController.vbReplace(c.menuFlyout.getMenu(MenuNamePrefix & genericController.encodeText(PageID), MenuStyle, StyleSheetPrefix), vbCrLf, "")
                        '
                        ' ----- Add in the rest of the menu details
                        ' ##### this must be here because it must go into the bake, else a baked page fails without he menus
                        '
                        pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu & c.htmlDoc.menu_GetClose()
                        '
                        ' ----- Bake the completed menu
                        '
                        Call c.cache.setObject(BakeName, pageManager_GetSectionMenu_IdMenu, ContentName & ",Site Sections,Dynamic Menus,Dynamic Menu Section Rules")
                    End If
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("main_GetSectionMenu_IdMenu")
        End Function
        '
        '======================================================================================
        '   Add child pages to the menu system
        '       REturns the count of total child pages (with grand-child, etc)
        '       Returns -1 if child count not checked
        ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
        '======================================================================================
        '
        Private Function main_GetSectionMenu_AddChildMenu_ReturnChildCount(ByVal ParentMenuID As Integer, ByVal ContentName As String, ByVal DefaultLink As String, ByVal Tier1MenuCaption As String, ByVal UsedPageIDString As String, ByVal MenuNamePrefix As String, ByVal MenuDepth As Integer, ByVal MenuDepthMax As Integer, ByVal childListSortMethodId As Integer, ByVal SectionID As Integer, ByVal AddRootButton As Boolean, ByVal UseContentWatchLink As Boolean) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00365")
            '
            Dim Active As Boolean
            Dim PseudoChildChildPagesFound As Boolean
            Dim PCCRowPtr As Integer
            Dim SortForward As Boolean
            Dim SortFieldName As String
            Dim SortPtr As Integer
            Dim Ptr As Integer
            Dim ChildPageCount As Integer
            Dim ChildPagesFoundTest As String
            Dim FieldList As String
            Dim ChildCountWithNoPubs As Integer
            Dim MenuID As Integer
            Dim MenuCaption As String
            Dim ChildCount As Integer
            Dim ChildSize As Integer
            Dim ChildPointer As Integer
            Dim ChildID() As Integer
            Dim ChildAllowChild() As Boolean
            Dim ChildCaption() As String
            Dim ChildLink() As String
            Dim ChildSortMethodID() As Integer
            Dim ChildChildPagesFound() As Boolean
            Dim ContentID As Integer
            Dim MenuLinkOverRide As String
            Dim PageID As Integer
            Dim UsedPageIDStringLocal As String
            Dim Criteria As String
            Dim MenuDepthLocal As Integer
            Dim OrderByCriteria As String
            Dim WorkingLink As String
            Dim templateId As Integer
            Dim ContentControlID As Integer
            Dim Link As String
            Dim PubDate As Date
            Dim PCCPtr As Integer
            Dim DateExpires As Date
            Dim dateArchive As Date
            Dim IsIncludedInMenu As Boolean
            Dim PCCPtrs() As Integer
            Dim PtrCnt As Integer
            Dim SortSplit() As String
            Dim SortSplitCnt As Integer
            Dim Index As coreKeyPtrIndexClass
            Dim PCCColPtr As Integer
            Dim PCCPtrsSorted As Integer()
            Dim AllowInMenus As Boolean
            '
            ' ----- Gather all child menus
            '
            ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
            ChildCountWithNoPubs = -1
            If (ParentMenuID > 0) And (MenuDepth <= MenuDepthMax) Then
                MenuDepthLocal = MenuDepth + 1
                UsedPageIDStringLocal = UsedPageIDString
                OrderByCriteria = c.GetSortMethodByID(childListSortMethodId)
                If OrderByCriteria = "" Then
                    OrderByCriteria = c.GetContentProperty(ContentName, "defaultsortmethod")
                End If
                If OrderByCriteria = "" Then
                    OrderByCriteria = "ID"
                End If
                '
                ' Populate PCCPtrs()
                '
                PCCPtr = pageManager_cache_pageContent_getFirstChildPtr(ParentMenuID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                PtrCnt = 0
                Do While PCCPtr >= 0
                    ReDim Preserve PCCPtrs(PtrCnt)
                    PCCPtrs(PtrCnt) = PCCPtr
                    PtrCnt = PtrCnt + 1
                    PCCPtr = pageManager_cache_pageContent_parentIdIndex.getNextPtrMatch(CStr(ParentMenuID))
                Loop
                If PtrCnt > 0 Then
                    PCCPtrsSorted = pageManager_cache_pageContent_getPtrsSorted(PCCPtrs, OrderByCriteria)
                End If
                '
                Ptr = 0
                Do While Ptr < PtrCnt
                    PCCPtr = PCCPtrsSorted(Ptr)
                    DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
                    dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
                    PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
                    MenuCaption = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr)))
                    If False Then '.3.752" Then
                        AllowInMenus = (MenuCaption <> "")
                    Else
                        AllowInMenus = genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, PCCPtr))
                    End If
                    Active = genericController.EncodeBoolean(cache_pageContent(PCC_Active, PCCPtr))
                    IsIncludedInMenu = Active And AllowInMenus And ((PubDate = Date.MinValue) Or (PubDate < c.app_startTime)) And ((DateExpires = Date.MinValue) Or (DateExpires > c.app_startTime))
                    'IsIncludedInMenu = Active And AllowInMenus And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime)) And ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime))
                    If IsIncludedInMenu Then
                        If MenuCaption = "" Then
                            MenuCaption = Trim(genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr)))
                        End If
                        If MenuCaption = "" Then
                            MenuCaption = "Related Page"
                        End If
                        If (MenuCaption <> "") Then
                            PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                            If genericController.vbInstr(1, UsedPageIDStringLocal & ",", "," & genericController.encodeText(PageID) & ",") = 0 Then
                                UsedPageIDStringLocal = UsedPageIDStringLocal & "," & genericController.encodeText(PageID)
                                If ChildCount >= ChildSize Then
                                    ChildSize = ChildSize + 100
                                    ReDim Preserve ChildID(ChildSize)
                                    ReDim Preserve ChildCaption(ChildSize)
                                    ReDim Preserve ChildLink(ChildSize)
                                    ReDim Preserve ChildSortMethodID(ChildSize)
                                    ReDim Preserve ChildAllowChild(ChildSize)
                                    ReDim Preserve ChildChildPagesFound(ChildSize)
                                End If
                                ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                                MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
                                '
                                ChildCaption(ChildCount) = MenuCaption
                                ChildID(ChildCount) = PageID
                                ChildAllowChild(ChildCount) = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
                                ChildSortMethodID(ChildCount) = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
                                templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                                '
                                Link = pageManager_GetPageLink4(PageID, "", True, UseContentWatchLink)
                                'Link = main_GetPageDynamicLinkWithArgs(contentcontrolid, PageID, DefaultLink, False, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
                                ChildLink(ChildCount) = Link
                                ChildPagesFoundTest = cache_pageContent(PCC_ChildPagesFound, PCCPtr)
                                If ChildPagesFoundTest = "" Then
                                    '
                                    ' Not initialized
                                    '
                                    ChildChildPagesFound(ChildCount) = True
                                Else
                                    ChildChildPagesFound(ChildCount) = genericController.EncodeBoolean(ChildPagesFoundTest)
                                End If
                                ChildCount = ChildCount + 1
                            End If
                        End If
                    End If
                    Ptr = Ptr + 1
                Loop
                ChildCountWithNoPubs = Ptr
                '
                ' ----- Output menu entries
                '
                If ChildCount > 0 Then
                    '
                    ' menu entry has children, output menu entry, child menu entry, and group of child entries
                    '
                    If AddRootButton Then
                        '
                        ' Root Button is a redundent menu entry at the top of tier 1 panels that links to the root page
                        '
                        Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", DefaultLink, Tier1MenuCaption)
                        'Call main_AddMenuEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByPageID(ParentMenuID, "", DefaultLink), Tier1MenuCaption)
                        'Call main_AddMenuEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByLink(DefaultLink), Tier1MenuCaption)
                    End If
                    '
                    For ChildPointer = 0 To ChildCount - 1
                        MenuID = ChildID(ChildPointer)
                        MenuCaption = ChildCaption(ChildPointer)
                        WorkingLink = ChildLink(ChildPointer)
                        Call c.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", WorkingLink, MenuCaption)
                        'Call main_AddMenuEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByPageID(MenuID, "", WorkingLink), MenuCaption)
                        'Call main_AddMenuEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByLink(WorkingLink), MenuCaption)
                        '
                        ' if child pages are found, print the next menu deeper
                        '
                        If (ParentMenuID > 0) And (MenuDepthLocal <= MenuDepthMax) Then
                            If pagemanager_IsWorkflowRendering() Then
                                '
                                ' Workflow mode - go main_Get the child pages
                                '
                                ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, WorkingLink, MenuCaption, UsedPageIDStringLocal, MenuNamePrefix, MenuDepthLocal, MenuDepthMax, ChildSortMethodID(ChildPointer), SectionID, False, UseContentWatchLink)
                            Else
                                '
                                ' Production mode - main_Get them only if the parent record says there are child pages
                                '
                                PseudoChildChildPagesFound = ChildChildPagesFound(ChildPointer)
                                If Not PseudoChildChildPagesFound Then
                                    '
                                    ' Even when child pages is false, try it 10% of the time anyway
                                    '
                                    Randomize()
                                    PseudoChildChildPagesFound = (Rnd() > 0.8)
                                End If
                                If PseudoChildChildPagesFound Then
                                    '
                                    ' Child pages were found, create child menu
                                    '
                                    ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
                                    ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, WorkingLink, MenuCaption, UsedPageIDStringLocal, MenuNamePrefix, MenuDepthLocal, MenuDepthMax, ChildSortMethodID(ChildPointer), SectionID, False, UseContentWatchLink)
                                    If ChildPageCount >= 0 Then
                                        If (True) Then
                                            If ChildChildPagesFound(ChildPointer) And (ChildPageCount = 0) Then
                                                '
                                                ' no pages were found, clear the child pages found property
                                                ' child pages found property is set at admin site when a page is saved with this as the parent id
                                                '
                                                Call c.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & MenuID)
                                                'Call AppendLog("pageManager_GetHtmlBody_GetSection_GetContentMenu_AddChildMenu, 6-call pageManager_cache_pageContent_updateRow -- fix here to NOT call pageManager_cache_pageContent_updateRow()")
                                                cache_pageContent(PCC_ChildPagesFound, ChildPointer) = "0"
                                                'Call pageManager_cache_pageContent_updateRow(MenuID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                                            ElseIf (ChildPageCount > 0) And (Not ChildChildPagesFound(ChildPointer)) Then
                                                '
                                                ' pages were found, set the child pages found property
                                                '
                                                Call c.db.executeSql("update ccpagecontent set ChildPagesFound=1 where id=" & MenuID)
                                                'Call AppendLog("pageManager_GetHtmlBody_GetSection_GetContentMenu_AddChildMenu, 7-call pageManager_cache_pageContent_updateRow -- fix here to NOT call pageManager_cache_pageContent_updateRow()")
                                                cache_pageContent(PCC_ChildPagesFound, ChildPointer) = "1"
                                                'Call pageManager_cache_pageContent_updateRow(MenuID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    Next
                End If
            End If
            main_GetSectionMenu_AddChildMenu_ReturnChildCount = ChildCountWithNoPubs
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("main_GetSectionMenu_AddChildMenu_ReturnChildCount")
        End Function
        '
        '=============================================================================
        '   Creates the child page list used by PageContent
        '
        '   RequestedListName is the name of the ChildList (ActiveContent Child Page List)
        '       ----- New
        '       RequestedListName = "", same as "ORPHAN", same as "NONE"
        '           prints orphan list (child pages that have not printed so far (orphan list))
        '       AllowChildListDisplay - if false, no Child Page List is displayed, but authoring tags are still there
        '       Changed to friend, not public
        '       ----- Old
        '       "NONE" returns child pages with no RequestedListName
        '       "" same as "NONE"
        '       "ORPHAN" returns all child pages that have not been printed on this page
        '           - uses pageManager_ChildPageListTracking to track what has been seen
        '=============================================================================
        '
        Public Function pageManager_GetChildPageList(ByVal RequestedListName As String, ByVal ContentName As String, ByVal parentPageID As Integer, ByVal allowChildListDisplay As Boolean, Optional ByVal ArchivePages As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetChildPageList")
            '
            Dim ChildContent As String
            Dim Brief As String
            Dim UcaseRequestedListName As String
            Dim ChildListCount As Integer
            Dim Pointer As Integer
            Dim AddLink As String
            Dim BlockContentComposite As Boolean
            Dim Link As String
            Dim AllowInChildLists As Boolean
            Dim LinkedText As String
            Dim ActiveList As String
            Dim InactiveList As String
            Dim hint As String
            Dim childBranchPtr As Integer
            Dim PCCPtr As Integer
            Dim PageID As Integer
            Dim PageName As String
            Dim archiveLink As String
            Dim PageLink As String
            Dim pageBlockContent As Boolean
            Dim pageBlockPage As Boolean
            Dim pageContentControlId As Integer
            Dim pageMenuHeadline As String
            Dim pageParentListName As String
            Dim pageEditLink As String
            Dim pageAllowInChildLists As Boolean
            Dim pageActive As Boolean
            Dim pagePubDate As Date
            Dim pageDateExpires As Date
            Dim pageBriefFilename As String
            Dim pageAllowBrief As Boolean
            '
            ChildListCount = 0
            UcaseRequestedListName = genericController.vbUCase(RequestedListName)
            If (UcaseRequestedListName = "NONE") Or (UcaseRequestedListName = "ORPHAN") Then
                UcaseRequestedListName = ""
            End If
            If Not main_RenderCache_Loaded Then
                Call c.handleLegacyError23("Can not call main_GetChildPageList before calling getHtmlDoc() because loadRenderCache() is required.")
            End If
            '
            archiveLink = c.webServer.requestPathPage
            archiveLink = genericController.ConvertLinkToShortLink(archiveLink, c.webServer.requestDomain, c.webServer.webServerIO_requestVirtualFilePath)
            archiveLink = genericController.EncodeAppRootPath(archiveLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
            '
            For childBranchPtr = 0 To main_RenderCache_ChildBranch_PCCPtrCnt - 1
                PCCPtr = genericController.EncodeInteger(main_RenderCache_ChildBranch_PCCPtrs(childBranchPtr))
                PageName = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                PageLink = pageManager_GetPageLink4(PageID, "", True, False)
                pageBlockContent = genericController.EncodeBoolean(cache_pageContent(PCC_BlockContent, PCCPtr))
                pageBlockPage = genericController.EncodeBoolean(cache_pageContent(PCC_BlockPage, PCCPtr))
                pageContentControlId = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                pageMenuHeadline = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
                If pageMenuHeadline = "" Then
                    pageMenuHeadline = Trim(PageName)
                    If pageMenuHeadline = "" Then
                        pageMenuHeadline = "Related Page"
                    End If
                End If
                pageParentListName = genericController.encodeText(cache_pageContent(PCC_ParentListName, PCCPtr))
                pageEditLink = ""
                If c.authContext.isEditing(c, ContentName) Then
                    pageEditLink = c.main_GetRecordEditLink2(ContentName, PageID, True, PageName, True)
                End If
                pageAllowInChildLists = genericController.EncodeBoolean(cache_pageContent(PCC_AllowInChildLists, PCCPtr))
                pageActive = genericController.EncodeBoolean(cache_pageContent(PCC_Active, PCCPtr))
                pagePubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
                pageDateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
                pageBriefFilename = genericController.encodeText(cache_pageContent(PCC_BriefFilename, PCCPtr))
                pageAllowBrief = genericController.EncodeBoolean(cache_pageContent(PCC_AllowBrief, PCCPtr))
                '
                If ArchivePages Then
                    Link = genericController.modifyLinkQuery(archiveLink, "bid", CStr(PageID), True)
                Else
                    Link = PageLink
                End If
                If pageBlockContent Or pageBlockPage Then
                    BlockContentComposite = Not pageManager_BypassContentBlock(pageContentControlId, PageID)
                Else
                    BlockContentComposite = False
                End If
                LinkedText = c.csv_GetLinkedText("<a href=""" & c.htmlDoc.html_EncodeHTML(Link) & """>", pageMenuHeadline)
                If (UcaseRequestedListName = "") And (pageParentListName <> "") And (Not main_RenderCache_CurrentPage_IsAuthoring) Then
                    '
                    ' ----- Requested orphan list, and this record is in a named list, and not editing, do not display
                    '
                ElseIf (UcaseRequestedListName = "") And (pageParentListName <> "") Then
                    '
                    ' ----- Requested orphan list, and this record is in a named list, but authoring, list it
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & PageID & """ name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[from Child Page List '" & pageParentListName & "': " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (UcaseRequestedListName = "") And (Not allowChildListDisplay) And (Not main_RenderCache_CurrentPage_IsAuthoring) Then
                    '
                    ' ----- Requested orphan List, Not AllowChildListDisplay, not Authoring, do not display
                    '
                ElseIf (UcaseRequestedListName <> "") And (UcaseRequestedListName <> genericController.vbUCase(pageParentListName)) Then
                    '
                    ' ----- requested named list and wrong RequestedListName, do not display
                    '
                ElseIf (Not pageAllowInChildLists) Then
                    '
                    ' ----- Allow in Child Page Lists is false, display hint to authors
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Allow in Child Lists is not checked): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf Not pageActive Then
                    '
                    ' ----- Not active record, display hint if authoring
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Inactive): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (pagePubDate <> Date.MinValue) And (pagePubDate > c.app_startTime) Then
                    '
                    ' ----- Child page has not been published
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (To be published " & pagePubDate & "): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (pageDateExpires <> Date.MinValue) And (pageDateExpires < c.app_startTime) Then
                    '
                    ' ----- Child page has expired
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Expired " & pageDateExpires & "): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                Else
                    '
                    ' ----- display list (and authoring links)
                    '
                    ActiveList = ActiveList & cr & "<li name=""page" & PageID & """  id=""page" & PageID & """ class=""ccListItem"">"
                    If pageEditLink <> "" Then
                        ActiveList = ActiveList & pageEditLink & "&nbsp;"
                    End If
                    ActiveList = ActiveList & LinkedText
                    '
                    ' include authoring mark for content block
                    '
                    If main_RenderCache_CurrentPage_IsAuthoring Then
                        If pageBlockContent Then
                            ActiveList = ActiveList & "&nbsp;[Content Blocked]"
                        End If
                        If pageBlockPage Then
                            ActiveList = ActiveList & "&nbsp;[Page Blocked]"
                        End If
                    End If
                    '
                    ' include overview
                    ' if AllowBrief is false, BriefFilename is not loaded
                    '
                    If (pageBriefFilename <> "") And (pageAllowBrief) Then
                        Brief = Trim(c.cdnFiles.readFile(pageBriefFilename))
                        If Brief <> "" Then
                            ActiveList = ActiveList & "<div class=""ccListCopy"">" & Brief & "</div>"
                        End If
                    End If
                    ActiveList = ActiveList & "</li>"
                    ChildListCount = ChildListCount + 1
                    '.IsDisplayed = True
                End If
            Next
            '
            ' ----- Add Link
            '
            If (Not ArchivePages) Then
                ChildContent = main_RenderCache_CurrentPage_ContentName
                If ChildContent = "" Then
                    ChildContent = "Page Content"
                End If
                AddLink = c.main_GetRecordAddLink(ChildContent, "parentid=" & parentPageID & ",ParentListName=" & UcaseRequestedListName, True)
                If AddLink <> "" Then
                    InactiveList = InactiveList & cr & "<li class=""ccListItem"">" & AddLink & "</LI>"
                End If
            End If
            '
            ' ----- If there is a list, add the list start and list end
            '
            pageManager_GetChildPageList = ""
            If ActiveList <> "" Then
                pageManager_GetChildPageList = pageManager_GetChildPageList & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildList"">" & genericController.kmaIndent(ActiveList) & cr & "</ul>"
            End If
            If InactiveList <> "" Then
                pageManager_GetChildPageList = pageManager_GetChildPageList & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildListInactive"">" & genericController.kmaIndent(InactiveList) & cr & "</ul>"
            End If
            '
            ' ----- if non-orphan list, authoring and none found, print none message
            '
            If (UcaseRequestedListName <> "") And (ChildListCount = 0) And main_RenderCache_CurrentPage_IsAuthoring Then
                pageManager_GetChildPageList = "[Child Page List with no pages]</p><p>" & pageManager_GetChildPageList
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetChildPageList")
        End Function
        '
        '=============================================================================
        '   main_Get the Section Menu
        '   MenuName blank reverse menu to legacy mode (all sections on menu)
        '=============================================================================
        '
        Public Function pageManager_GetSectionMenu(ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal DefaultTemplateLink As String, ByVal MenuID As Integer, ByVal MenuName As String, ByVal UseContentWatchLink As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("PageList_GetSectionMenu")
            '
            Dim layoutError As String
            Dim pageActive As Boolean
            Dim TCPtr As Integer
            Dim PCCPtr As Integer
            Dim rootPageId As Integer
            Dim CSSections As Integer
            Dim CSTemplates As Integer
            Dim CSPage As Integer
            Dim SectionName As String
            Dim templateId As Integer
            Dim ContentID As Integer
            Dim ContentName As String
            'Dim main_oldCacheArray_ParentBranchPointer as integer
            Dim Link As String
            Dim SectionID As Integer
            Dim AuthoringTag As String
            Dim MenuImage As String
            Dim MenuImageOver As String
            'Dim SectionCount as integer
            Dim LandingLink As String
            Dim MenuString As String
            Dim SectionCaption As String
            Dim SectionTemplateID As Integer
            Dim Criteria As String
            Dim SelectFieldList As String
            Dim ShowHiddenMenu As Boolean
            Dim HideMenu As Boolean
            'dim buildversion As String
            Dim PageContentCID As Integer
            Dim BlockPage As Boolean
            Dim BlockSection As Boolean
            Dim SQL As String
            Dim IsAllSectionsMenuMode As Boolean
            '
            '
            '
            ' fixed? - !! Problem: new upgraded site with old menu object (MenuName=""). We take the third option here, but later in the
            '   routine we use RootPageID because the check is on Version only
            '
            IsAllSectionsMenuMode = (MenuName = "")
            PageContentCID = c.main_GetContentID("Page Content")
            If (True) Then
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID"
                ShowHiddenMenu = c.authContext.isEditingAnything(c)
                'ShowHiddenMenu = main_IsEditing("Site Sections")
                If IsAllSectionsMenuMode Then
                    '
                    ' Section/Page connection at RootPageID, show all sections
                    '
                    CSSections = c.db.cs_open("Site Sections", , , , , ,, SelectFieldList)
                Else
                    '
                    ' Section/Page connection at RootPageID, only show sections connected to the menu
                    '
                    SQL = "Select Distinct S.ID" _
                        & " from ((ccSections S" _
                        & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
                        & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
                        & " where M.ID=" & MenuID
                    Criteria = "ID in (" & SQL & ")"
                    CSSections = c.db.cs_open("Site Sections", Criteria, , , , , , SelectFieldList)
                End If
                '        '
                '        ' Section/Page connection at RootPageID
                '        '
                '        SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID"
                '        SQL = "Select Distinct S.ID" _
                '            & " from ((ccSections S" _
                '            & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
                '            & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
                '            & " where M.ID=" & MenuID
                '        Criteria = "ID in (" & SQL & ")"
                '        ShowHiddenMenu = main_IsEditing("Site Sections")
                '        CSSections = app.csOpen("Site Sections", Criteria, , , , , SelectFieldList)
            ElseIf (True) Then
                '
                ' Multiple Menus with ccDynamicMenuSectionRules
                '
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,0 as RootPageID"
                ShowHiddenMenu = c.authContext.isEditingAnything(c)
                'ShowHiddenMenu = main_IsEditing("Site Sections")
                If IsAllSectionsMenuMode Then
                    '
                    ' Section/Page connection at RootPageID, show all sections
                    '
                    CSSections = c.db.cs_open("Site Sections", , , , , , , SelectFieldList)
                Else
                    '
                    ' Section/Page connection at RootPageID, only show sections connected to the menu
                    '
                    SQL = "Select Distinct S.ID" _
                        & " from ((ccSections S" _
                        & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
                        & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
                        & " where M.ID=" & MenuID
                    Criteria = "ID in (" & SQL & ")"
                    CSSections = c.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
                End If
                '        SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection"
                '        SQL = "Select Distinct S.ID" _
                '            & " from ((ccSections S" _
                '            & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
                '            & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
                '            & " where M.ID=" & MenuID
                '        Criteria = "ID in (" & SQL & ")"
                '        ShowHiddenMenu = main_IsEditing("Site Sections")
                '        CSSections = app.csOpen("Site Sections", Criteria, , , , , SelectFieldList)
            ElseIf c.IsSQLTableField("Default", "ccSections", "BlockSection") Then
                '
                ' All sections menu mode with block sections
                '
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,0 as RootPageID"
                Criteria = ""
                ShowHiddenMenu = c.authContext.isEditingAnything(c)
                'ShowHiddenMenu = main_IsEditing("Site Sections")
                CSSections = c.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
            ElseIf c.IsSQLTableField("Default", "ccSections", "MenuImageOverFilename") Then
                '
                ' All sections menu mode with Image Over
                '
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,0 as BlockSection,0 as RootPageID"
                Criteria = ""
                ShowHiddenMenu = c.authContext.isEditingAnything(c)
                'ShowHiddenMenu = main_IsEditing("Site Sections")
                CSSections = c.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
            ElseIf c.IsSQLTableField("Default", "ccSections", "HideMenu") Then
                '
                ' All sections menu mode with HideMenu
                '
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,'' as MenuImageOverFilename,HideMenu,0 as BlockSection,0 as RootPageID"
                Criteria = ""
                ShowHiddenMenu = c.authContext.isEditingAnything(c)
                'ShowHiddenMenu = main_IsEditing("Site Sections")
                CSSections = c.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
            Else
                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,'' as MenuImageOverFilename,0 as HideMenu,0 as BlockSection,0 as RootPageID"
                Criteria = ""
                ShowHiddenMenu = True
                CSSections = c.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
            End If
            Do While c.db.cs_ok(CSSections)
                HideMenu = c.db.cs_getBoolean(CSSections, "HideMenu")
                BlockSection = c.db.cs_getBoolean(CSSections, "BlockSection")
                SectionID = c.db.cs_getInteger(CSSections, "ID")
                If ShowHiddenMenu Or Not (HideMenu Or c.main_isSectionBlocked(SectionID, BlockSection)) Then
                    SectionName = Trim(c.db.cs_getText(CSSections, "Name"))
                    If SectionName = "" Then
                        SectionName = "Section " & SectionID
                        Call c.db.executeSql("update ccSections set Name=" & c.db.encodeSQLText(SectionName) & " where ID=" & SectionID)
                    End If
                    SectionCaption = c.db.cs_getText(CSSections, "Caption")
                    If SectionCaption = "" Then
                        SectionCaption = SectionName
                        Call c.db.executeSql("update ccSections set Caption=" & c.db.encodeSQLText(SectionCaption) & " where ID=" & SectionID)
                    End If
                    If HideMenu Then
                        SectionCaption = "[Hidden: " & SectionCaption & "]"
                    End If
                    SectionTemplateID = c.db.cs_getInteger(CSSections, "TemplateID")
                    ContentID = c.db.cs_getInteger(CSSections, "ContentID")
                    If (ContentID <> PageContentCID) And (Not c.IsWithinContent(ContentID, PageContentCID)) Then
                        ContentID = PageContentCID
                        Call c.db.cs_set(CSSections, "ContentID", ContentID)
                    End If
                    If ContentID = PageContentCID Then
                        ContentName = "Page Content"
                    Else
                        ContentName = c.metaData.getContentNameByID(ContentID)
                        If ContentName = "" Then
                            ContentName = "Page Content"
                            ContentID = c.main_GetContentID(ContentName)
                            Call c.db.executeSql("update ccSections set ContentID=" & ContentID & " where ID=" & SectionID)
                        End If
                    End If
                    MenuImage = c.db.cs_getText(CSSections, "MenuImageFilename")
                    If MenuImage <> "" Then
                        MenuImage = c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, MenuImage)
                    End If
                    MenuImageOver = c.db.cs_getText(CSSections, "MenuImageOverFilename")
                    If MenuImageOver <> "" Then
                        MenuImageOver = c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, MenuImageOver)
                    End If
                    '
                    ' main_Get Root Page for templateID
                    '
                    templateId = 0
                    BlockPage = False
                    Link = ""
                    If False Then '.3.451" Then
                        '
                        ' no blockpage,section-page connection by name
                        '
                        PCCPtr = pageManager_cache_pageContent_getFirstNamePtr(SectionName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                        'SelectFieldList = "ID,TemplateID,0 as BlockPage"
                        'CSPage = app.csOpen(ContentName, "name=" & encodeSQLText(SectionName), "ID", , , , SelectFieldList)
                    ElseIf False Then '.3.613" Then
                        '
                        ' blockpage,section-page connection by name
                        '
                        PCCPtr = pageManager_cache_pageContent_getFirstNamePtr(SectionName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                        'SelectFieldList = "ID,TemplateID,BlockPage"
                        'CSPage = app.csOpen(ContentName, "name=" & encodeSQLText(SectionName), "ID", , , , SelectFieldList)
                    Else
                        '
                        ' section-page connection by name
                        '
                        rootPageId = c.db.cs_getInteger(CSSections, "rootpageid")
                        PCCPtr = pageManager_cache_pageContent_getPtr(rootPageId, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                        'SelectFieldList = "ID,TemplateID,BlockPage"
                        'CSPage = main_OpenCSContentRecord_Internal(ContentName, RootPageID, , , SelectFieldList)
                    End If
                    If PCCPtr >= 0 Then
                        rootPageId = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
                        templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                        BlockPage = genericController.EncodeBoolean(cache_pageContent(PCC_BlockPage, PCCPtr))
                        pageActive = genericController.EncodeBoolean(cache_pageContent(PCC_Active, PCCPtr))
                    End If
                    If pageActive Or ShowHiddenMenu Then
                        If PCCPtr < 0 Then
                            '
                            ' Page Missing
                            '
                            SectionCaption = "[Missing Page: " & SectionCaption & "]"
                        ElseIf Not pageActive Then
                            '
                            ' Page Inactive
                            '
                            SectionCaption = "[Inactive Page: " & SectionCaption & "]"
                        End If
                        If templateId = 0 Then
                            templateId = SectionTemplateID
                        End If
                        '
                        ' main_Get the link from either the template, or use the default link
                        '
                        If templateId <> 0 Then
                            TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
                            If TCPtr >= 0 Then
                                Link = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
                            End If
                            'Link = main_GetTCLink(TCPtr)
                        End If
                        If Link = "" Then
                            Link = DefaultTemplateLink
                        End If
                        AuthoringTag = c.main_GetRecordEditLink2("Site Sections", SectionID, False, SectionName, c.authContext.isEditing(c, "Site Sections"))
                        Link = genericController.modifyLinkQuery(Link, "sid", CStr(SectionID), True)
                        '
                        ' main_Get Menu, remove crlf, and parse the line with crlf
                        '
                        MenuString = pageManager_GetSectionMenu_IdMenu(rootPageId, Link, DepthLimit, MenuStyle, StyleSheetPrefix, MenuImage, MenuImageOver, SectionCaption, SectionID, UseContentWatchLink)
                        MenuString = genericController.vbReplace(AuthoringTag & MenuString, vbCrLf, "")
                        If (MenuString <> "") Then
                            If (pageManager_GetSectionMenu = "") Then
                                pageManager_GetSectionMenu = MenuString
                            Else
                                pageManager_GetSectionMenu = pageManager_GetSectionMenu & vbCrLf & MenuString
                            End If
                        End If
                    End If
                    '
                End If
                Call c.db.cs_goNext(CSSections)
            Loop
            AuthoringTag = c.main_GetRecordAddLink("Site Sections", "MenuID=" & MenuID)
            If AuthoringTag <> "" Then
                pageManager_GetSectionMenu = pageManager_GetSectionMenu & AuthoringTag
            End If
            Call c.db.cs_Close(CSSections)
            '
            pageManager_GetSectionMenu = c.htmlDoc.html_executeContentCommands(Nothing, pageManager_GetSectionMenu, CPUtilsBaseClass.addonContext.ContextPage, c.authContext.user.ID, c.authContext.isAuthenticated, layoutError)
            pageManager_GetSectionMenu = c.htmlDoc.html_encodeContent10(pageManager_GetSectionMenu, c.authContext.user.ID, "", 0, 0, False, False, True, True, False, True, "", "http://" & c.webServer.requestDomain, False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, c.authContext.isAuthenticated, Nothing, c.authContext.isEditingAnything(c))
            'pageManager_GetSectionMenu = main_EncodeContent5(pageManager_GetSectionMenu, memberID, "", 0, 0, False, False, True, True, False, True, "", "", False, 0)
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetSectionMenu")
        End Function
        '
        '=============================================================================
        '   pageManager_BypassContentBlock
        '       Is This member allowed through the content block
        '=============================================================================
        '
        Public Function pageManager_BypassContentBlock(ByVal ContentID As Integer, ByVal RecordID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00374")
            '
            Dim CS As Integer
            Dim SQL As String
            '
            If c.authContext.isAuthenticatedAdmin(c) Then
                pageManager_BypassContentBlock = True
            ElseIf c.authContext.isAuthenticatedContentManager(c, c.metaData.getContentNameByID(ContentID)) Then
                pageManager_BypassContentBlock = True
            Else
                SQL = "SELECT ccMemberRules.MemberID" _
                    & " FROM (ccPageContentBlockRules LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID) LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID" _
                    & " WHERE (((ccPageContentBlockRules.RecordID)=" & RecordID & ")" _
                    & " AND ((ccPageContentBlockRules.Active)<>0)" _
                    & " AND ((ccgroups.Active)<>0)" _
                    & " AND ((ccMemberRules.Active)<>0)" _
                    & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & c.db.encodeSQLDate(c.app_startTime) & ")" _
                    & " AND ((ccMemberRules.MemberID)=" & c.authContext.user.ID & "));"
                CS = c.db.cs_openSql(SQL)
                pageManager_BypassContentBlock = c.db.cs_ok(CS)
                Call c.db.cs_Close(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("IsContentBlocked")
        End Function
        '
        '===========================================================================
        '   Populate the parent branch
        '       PageID and RootPageID must be valid
        '
        '   I think this routine is over-written at this point. Since we now call with pageid and rootpageid, the loop is not needed -- I think
        '===========================================================================
        '
        Private Sub pageManager_LoadRenderCache_CurrentPage(PageID As Integer, rootPageId As Integer, RootPageContentName As String, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_LoadRenderCache_CurrentPage")
            '
            Dim SubMessage As String
            'Dim PCCPtr as integer
            Dim IsNotActive As Boolean
            Dim IsExpired As Boolean
            Dim IsNotPublished As Boolean
            'Dim IsArchived As Boolean
            Dim SQL As String
            Dim CS As Integer
            Dim ParentID As Integer
            Dim pageCaption As String
            Dim QueryString As String
            Dim dateArchive As Date
            Dim DateExpires As Date
            Dim PubDate As Date
            Dim Criteria As String
            Dim ContentControlID As Integer
            Dim SelectList As String
            Dim reloadPage As Boolean
            Dim Active As Boolean
            Dim loadPageCnt As Integer
            Dim ContentName As String
            Dim MenuLinkOverRide As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim RecordName As String
            Dim RecordID As Integer
            '
            If PageID = 0 And rootPageId = 0 Then
                '
                ' no page and no root page, redirect to landing page
                '
                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                pageManager_RedirectBecausePageNotFound = True
                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
            ElseIf PageID = 0 Then
                '
                ' no page, redirect to root page
                '
                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                pageManager_RedirectBecausePageNotFound = True
                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
            ElseIf rootPageId = 0 Then
                '
                ' no rootpage id
                '
                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                pageManager_RedirectBecausePageNotFound = True
                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
            Else
                '
                reloadPage = True
                Do While reloadPage And (loadPageCnt < 10)
                    reloadPage = False
                    'main_oldCacheArray_CurrentPagePtr = -1
                    'main_oldCacheArray_CurrentPageCount = 0
                    main_RenderCache_CurrentPage_PCCPtr = pageManager_cache_pageContent_getPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                    If (main_RenderCache_CurrentPage_PCCPtr < 0) Then
                        '
                        ' page was not found ?
                        '
                        main_RenderCache_CurrentPage_PCCPtr = main_RenderCache_CurrentPage_PCCPtr
                    Else
                        RecordName = Trim(genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr)))
                        ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, main_RenderCache_CurrentPage_PCCPtr))
                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, main_RenderCache_CurrentPage_PCCPtr))
                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, main_RenderCache_CurrentPage_PCCPtr))
                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, main_RenderCache_CurrentPage_PCCPtr))
                        Active = genericController.EncodeBoolean(cache_pageContent(PCC_Active, main_RenderCache_CurrentPage_PCCPtr))
                        RecordID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
                        ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                        MenuLinkOverRide = Trim(genericController.encodeText(cache_pageContent(PCC_Link, main_RenderCache_CurrentPage_PCCPtr)))
                        IsNotActive = (Not Active)
                        IsExpired = ((DateExpires > Date.MinValue) And (DateExpires < c.app_startTime))
                        IsNotPublished = ((PubDate > Date.MinValue) And (PubDate > c.app_startTime))
                        'IsArchived = ((DateArchive > Date.MinValue) And (DateArchive < main_PageStartTime))
                        '
                        RecordName = genericController.vbReplace(RecordName, vbCrLf, " ")
                        RecordName = genericController.vbReplace(RecordName, vbTab, " ")
                        '
                        If IsNotActive Or IsExpired Or IsNotPublished Then
                            pageManager_RedirectSourcePageID = PageID
                            If IsNotActive Then
                                SubMessage = " because it marked inactive"
                            ElseIf IsExpired Then
                                SubMessage = " because the expiration date has passed"
                            ElseIf IsNotPublished Then
                                SubMessage = " because the publish date has not passed"
                                'ElseIf IsArchived Then
                                '    SubMessage = " because the archive date has passed"
                            End If
                            If PageID = main_GetLandingPageID() Then
                                '
                                ' Landing Page - do not redirect, just show a blank page with the admin message
                                '
                                c.htmlDoc.main_AdminWarning = "The page requested [" & PageID & "] can not be displayed" & SubMessage & ". It is the landing page of this website so no replacement page can be displayed. To correct this page, use the link below to edit the page and mark it active. To create a different landing page, edit any other active page and check the box marked 'Set Landing Page' in the control tab."
                                c.htmlDoc.main_AdminWarningPageID = PageID
                                c.htmlDoc.main_AdminWarningSectionID = SectionID
                            ElseIf PageID = rootPageId Then
                                '
                                ' Root Page - redirect to the landing page with an admin message
                                '
                                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                                'Call main_LogPageNotFound(main_ServerLink)
                                pageManager_RedirectBecausePageNotFound = True
                                pageManager_RedirectReason = "The page requested [" & PageID & "] can not be displayed" & SubMessage & ". It is the root page of the section [" & SectionID & "]."
                                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                                Exit Sub
                            Else
                                '
                                ' non-Root Page - redirect to the root page with an admin message
                                '
                                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                                'Call main_LogPageNotFound(main_ServerLink)
                                pageManager_RedirectBecausePageNotFound = True
                                pageManager_RedirectReason = "The page requested [" & PageID & "] can not be displayed" & SubMessage & "."
                                'pageManager_RedirectReason = "The page requested [" & PageID & "] is marked inactive."
                                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                                Exit Sub
                            End If
                        End If
                        '
                        ContentName = c.metaData.getContentNameByID(ContentControlID)
                        If ContentName <> "" Then
                            If (Not main_RenderCache_CurrentPage_IsQuickEditing) And c.authContext.isQuickEditing(c, ContentName) Then
                                main_RenderCache_CurrentPage_IsQuickEditing = True
                                reloadPage = True
                            End If
                        End If
                        If (Not reloadPage) Then
                            '
                            ' ----- Store results in main_oldCacheArray_CurrentPage Storage
                            '
                            'If main_oldCacheArray_CurrentPageCount >= main_oldCacheArray_CurrentPageSize Then
                            '    main_oldCacheArray_CurrentPageSize = main_oldCacheArray_CurrentPageSize + 10
                            '    ReDim Preserve main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPageSize)
                            'End If
                            'main_oldCacheArray_CurrentPagePtr = main_oldCacheArray_CurrentPageCount
                            '                    With main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr)
                            '                        .Name = RecordName
                            '                        .Id = RecordID
                            '                        .Active = Active
                            '                        .parentId = parentId
                            '                        .headline = Trim(genericController.encodeText(main_pcc(PCC_Headline, main_RenderCache_CurrentPage_PCCPtr)))
                            '                        .headline = genericController.vbReplace(.headline, vbCrLf, " ")
                            '                        .headline = genericController.vbReplace(.headline, vbTab, " ")
                            '                        .MenuHeadline = Trim(genericController.encodeText(main_pcc(PCC_MenuHeadline, main_RenderCache_CurrentPage_PCCPtr)))
                            '                        .MenuHeadline = genericController.vbReplace(.MenuHeadline, vbCrLf, " ")
                            '                        .MenuHeadline = genericController.vbReplace(.MenuHeadline, vbTab, " ")
                            '                        .dateArchive = dateArchive
                            '                        .DateExpires = DateExpires
                            '                        .PubDate = PubDate
                            '                        .childListSortMethodId = genericController.EncodeInteger(main_pcc(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .ChildListInstanceOptions = genericController.encodeText(main_pcc(PCC_ChildListInstanceOptions, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .ContentControlID = ContentControlID
                            '                        .templateId = genericController.EncodeInteger(main_pcc(PCC_TemplateID, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .LinkOverride = MenuLinkOverRide
                            '                        '.LinkDynamic = main_GetPageDynamicLinkWithArgs(.ContentControlID, RecordID, main_ServerPathPage & "?" & main_RefreshQueryString, (.Id = RootPageID), .TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
                            '                        .Link = main_GetPageLink4(.Id, "", True, False)
                            '                        '.Link = main_GetLinkAliasByPageID(.Id, "", .LinkDynamic)
                            '                        .MetaContentNoFollow = genericController.EncodeBoolean(main_pcc(PCC_AllowMetaContentNoFollow, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .BlockSourceID = genericController.EncodeInteger(main_pcc(PCC_BlockSourceID, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .CustomBlockMessageFilename = Trim(genericController.encodeText(main_pcc(PCC_CustomBlockMessageFilename, main_RenderCache_CurrentPage_PCCPtr)))
                            '                        .RegistrationGroupID = genericController.EncodeInteger(main_pcc(PCC_RegistrationGroupID, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .JSOnLoad = genericController.encodeText(main_pcc(PCC_JSOnLoad, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .JSHead = genericController.encodeText(main_pcc(PCC_JSHead, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .JSFilename = genericController.encodeText(main_pcc(PCC_JSFilename, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .JSEndBody = genericController.encodeText(main_pcc(PCC_JSEndBody, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, main_RenderCache_CurrentPage_PCCPtr))
                            '                        .DateModified =  genericController.EncodeDate(main_pcc(PCC_ModifiedDate, main_RenderCache_CurrentPage_PCCPtr))
                            '                    End With
                            '                    main_oldCacheArray_CurrentPageCount = main_oldCacheArray_CurrentPageCount + 1
                        End If
                    End If
                    If (main_RenderCache_CurrentPage_PCCPtr < 0) Then
                        'If (main_oldCacheArray_CurrentPagePtr = -1) And (PageID <> 0) Then
                        If (SectionID <> 0) And (PageID = rootPageId) Then
                            '
                            ' This is a root page that is missing -
                            '
                            ' the redirect does the logging -- Call main_LogPageNotFound(main_ServerLinkSource)
                            'Call main_LogPageNotFound(main_ServerLink)
                            pageManager_RedirectBecausePageNotFound = True
                            pageManager_RedirectReason = "The root page [" & PageID & "] for this section [" & SectionID & "] could not be found. It may have been deleted. To correct this problem, edit this section and save it with 'none' selected for the Root Page. A new root page will be created automatically the next time the page is viewed. Alternately, you can manually select another root page in the section record."
                            pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                            Exit Sub
                        Else
                            '
                            ' ----- This PageID (bid) was not found, revert to the RootPageName and try again
                            '
                            ' the redirect does the logging -- Call main_LogPageNotFound(main_ServerLinkSource)
                            pageManager_RedirectBecausePageNotFound = True
                            pageManager_RedirectReason = "The page could not be found from its ID [" & PageID & "]. It may have been deleted."
                            pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                            Exit Sub
                        End If
                    End If
                    loadPageCnt = loadPageCnt + 1
                Loop
                '
                main_RenderedPageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
                main_RenderedParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                main_RenderedPageName = genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr))
                main_RenderCache_CurrentPage_IsRootPage = (main_RenderedPageID = rootPageId) And (main_RenderedParentID = 0)
                main_RenderCache_CurrentPage_ContentId = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, main_RenderCache_CurrentPage_PCCPtr))
                main_RenderCache_CurrentPage_ContentName = c.metaData.getContentNameByID(main_RenderCache_CurrentPage_ContentId)
                main_RenderCache_CurrentPage_IsEditing = c.authContext.isEditing(c, main_RenderCache_CurrentPage_ContentName)
                main_RenderCache_CurrentPage_IsQuickEditing = c.authContext.isQuickEditing(c, main_RenderCache_CurrentPage_ContentName)
                main_RenderCache_CurrentPage_IsAuthoring = main_RenderCache_CurrentPage_IsEditing Or main_RenderCache_CurrentPage_IsQuickEditing
                c.webServer.webServerIO_response_NoFollow = genericController.EncodeBoolean(cache_pageContent(PCC_AllowMetaContentNoFollow, main_RenderCache_CurrentPage_PCCPtr)) Or c.webServer.webServerIO_response_NoFollow
                '
                '        If main_oldCacheArray_CurrentPagePtr <> -1 Then
                '            With main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr)
                '                'main_RenderedPageID = .Id
                '                'main_RenderedPageName = .Name
                '                'main_RenderCache_CurrentPage_IsRootPage = (.Id = rootPageId) And (.ParentID = 0)
                '                'main_RenderCache_CurrentPage_ContentId = .ContentControlID
                '                'main_RenderCache_CurrentPage_ContentName = metaData.getContentNameByID(.ContentControlID)
                '                'main_RenderCache_CurrentPage_IsEditing = main_IsEditing(main_RenderCache_CurrentPage_ContentName)
                '                main_RenderCache_CurrentPage_IsQuickEditing = main_IsQuickEditing(main_RenderCache_CurrentPage_ContentName)
                '                main_RenderCache_CurrentPage_IsAuthoring = main_RenderCache_CurrentPage_IsEditing Or main_RenderCache_CurrentPage_IsQuickEditing
                '                main_MetaContent_NoFollow = .MetaContentNoFollow Or main_MetaContent_NoFollow
                '            End With
                '        End If
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError13("main_LoadRenderCache_CurrentPage")
        End Sub
        '
        '===========================================================================
        '   Populate the parent branch
        '
        '   The PageID is the id of the page being checked for a parent, not the ID of the parent
        '===========================================================================
        '
        Private Sub pageManager_LoadRenderCache_ParentBranch(PageID As Integer, rootPageId As Integer, RootPageContentName As String, ArchivePage As Boolean, ParentIDPath As String, SectionID As Integer, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_LoadRenderCache_ParentBranch")
            '
            Dim PCCPtr As Integer
            Dim IsRootPage As Boolean
            'Dim SQL As String
            'Dim CS as integer
            Dim ParentID As Integer
            Dim pageCaption As String
            Dim PageName As String
            Dim QueryString As String
            Dim dateArchive As Date
            Dim DateExpires As Date
            Dim PubDate As Date
            Dim Criteria As String
            Dim ContentControlID As Integer
            Dim templateId As Integer
            Dim MenuLinkOverRide As String
            Dim SelectFieldList As String
            Dim hint As String
            '
            'hint = "main_LoadRenderCache_ParentBranch"
            If PageID = 0 Then
                '
                ' this page does not exist, end of branch
                '
                'hint = hint & ",10"
            ElseIf PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr)) Then
                'hint = hint & ",20"
                'ElseIf PageID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).Id Then
                '
                ' This is the current page, main_Get the branch and process it depending on archivepage
                '
                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                'ParentID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).ParentID
                If ArchivePage Then
                    Call c.handleLegacyError12("main_LoadRenderCache_ParentBranch", "The Archive API is no longer supported. The current URL is [" & c.webServer.webServerIO_ServerLink & "]")

                Else
                    '
                    ' non-Archive, test if this bid goes up to this rootpagename (if it is called from the correct page)
                    '
                    'hint = hint & ",30"
                    If PageID <> ParentID Then
                        Call pageManager_LoadRenderCache_ParentBranch(ParentID, rootPageId, RootPageContentName, ArchivePage, ParentIDPath & "," & PageID, SectionID, UseContentWatchLink)
                    End If
                End If
            ElseIf genericController.IsInDelimitedString(ParentIDPath, CStr(PageID), ",") Then
                '
                ' this page has been fetched before, end the branch here
                '
                Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                'Call main_LogPageNotFound(main_ServerLink)
                pageManager_RedirectBecausePageNotFound = True
                pageManager_RedirectReason = "The requested page could not be displayed because there is a circular reference within it's parent path. The page [" & PageID & "] was found two times in the parent pages [" & ParentIDPath & "," & PageID & "]."
                pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID)
            Else
                '
                ' This is not the current page, and it is not 0, Look it up
                '
                'hint = hint & ",40"
                PCCPtr = pageManager_cache_pageContent_getPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                If PCCPtr >= 0 Then
                    'hint = hint & ",50"
                    ReDim Preserve main_RenderCache_ParentBranch_PCCPtrs(main_RenderCache_ParentBranch_PCCPtrCnt)
                    main_RenderCache_ParentBranch_PCCPtrs(main_RenderCache_ParentBranch_PCCPtrCnt) = PCCPtr
                    main_RenderCache_ParentBranch_PCCPtrCnt = main_RenderCache_ParentBranch_PCCPtrCnt + 1
                    '
                    ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
                    PageName = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
                    dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
                    DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
                    PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
                    ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                    templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                    pageCaption = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr)))
                    If pageCaption = "" Then
                        pageCaption = Trim(PageName)
                        If pageCaption = "" Then
                            pageCaption = "Related Page"
                        End If
                    End If
                    PageName = genericController.vbReplace(PageName, vbCrLf, " ")
                    PageName = genericController.vbReplace(PageName, vbTab, " ")
                    pageCaption = genericController.vbReplace(pageCaption, vbCrLf, " ")
                    pageCaption = genericController.vbReplace(pageCaption, vbTab, " ")
                    '
                    ' Store results in main_oldCacheArray_ParentBranch Storage
                    '
                    'If main_oldCacheArray_ParentBranchCount >= main_oldCacheArray_ParentBranchSize Then
                    '    main_oldCacheArray_ParentBranchSize = main_oldCacheArray_ParentBranchSize + 10
                    '    ReDim Preserve main_oldCacheArray_ParentBranch(main_oldCacheArray_ParentBranchSize)
                    'End If
                    'With main_oldCacheArray_ParentBranch(main_oldCacheArray_ParentBranchCount)
                    '    .Id = genericController.EncodeInteger(main_pcc(PCC_ID, PCCPtr))
                    '    .Name = PageName
                    '    .Caption = pageCaption
                    '    .dateArchive = dateArchive
                    '    .DateExpires = DateExpires
                    '    .PubDate = PubDate
                    '    .childListSortMethodId = genericController.EncodeInteger(main_pcc(PCC_ChildListSortMethodID, PCCPtr))
                    '    .ContentControlID = ContentControlID
                    '    .parentId = parentId
                    '    .templateId = templateId
                    '    .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, PCCPtr))
                    '    .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, PCCPtr))
                    '    .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, PCCPtr))
                    '    MenuLinkOverRide = genericController.encodeText(main_pcc(PCC_Link, PCCPtr))
                    '    IsRootPage = (.Id = rootPageId)
                    '    '.LinkDynamic = main_GetPageDynamicLinkWithArgs(.ContentControlID, .Id, main_ServerPathPage & "?" & main_RefreshQueryString, IsRootPage, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
                    '    .Link = main_GetPageLink4(.Id, "", True, False)
                    '    '.Link = main_GetLinkAliasByPageID(.Id, "", .LinkDynamic)
                    '    main_oldCacheArray_ParentBranchCount = main_oldCacheArray_ParentBranchCount + 1
                    'End With
                    '
                    'Call app.closeCS(CS)
                    '
                    IsRootPage = (genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr)) = rootPageId)
                    If IsRootPage Then
                        '
                        ' The top of a page tree
                        '
                    ElseIf ((DateExpires > Date.MinValue) And (DateExpires < c.app_startTime)) Then
                        '
                        ' this page within the Parent Branch expired. Abort here and go to RootPage
                        '
                        '            ElseIf (ArchivePage And (DateArchive < main_PageStartTime)) Then
                        '                '
                        '                ' The top of an archive tree
                        '                '
                    ElseIf (ParentID = 0) Then
                        '
                        ' The top of a page tree
                        '
                    Else
                        '
                        ' parent is a child link, bubble up
                        '
                        'hint = hint & ",60"
                        Call pageManager_LoadRenderCache_ParentBranch(ParentID, rootPageId, RootPageContentName, ArchivePage, ParentIDPath & "," & PageID, SectionID, UseContentWatchLink)
                    End If
                End If
            End If
            Call c.debug_testPoint(hint)
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError13("main_LoadRenderCache_ParentBranch")
        End Sub
        '
        '===========================================================================
        '   Populate the root branch
        '===========================================================================
        '
        Private Sub pageManager_LoadRenderCache_ChildBranch(ChildOrderByClause As String, IsRenderingMode As Boolean, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("PageList_LoadContent_ChildBranch")
            '
            Dim PCCPtrs() As Integer
            Dim PCCPtrsCnt As Integer
            Dim Ptr As Integer
            Dim PtrCnt As Integer
            Dim PCCPtrsSorted As Integer()
            Dim ContentName As String
            Dim PCCPtr As Integer
            Dim PageID As Integer
            Dim SQL As String
            Dim CS As Integer
            Dim ParentID As Integer
            Dim pageCaption As String
            Dim PageName As String
            Dim QueryString As String
            Dim dateArchive As Date
            Dim DateExpires As Date
            Dim PubDate As Date
            Dim Criteria As String
            Dim SQLOrderBy As String
            Dim ParentContentID As Integer
            Dim ParentContentName As String
            Dim IsParentEditing As Boolean
            Dim IsParentAuthoring As Boolean
            Dim SelectFieldList As String
            Dim ContentControlID As Integer
            Dim MenuLinkOverRide As String
            Dim childListSortMethodId As Integer
            Dim parentPCCPtr As Integer
            '
            If True Then
                'If main_oldCacheArray_CurrentPagePtr <> -1 Then
                '
                ' ----- main_Get the Sort Method Order By Clause, both normal and archive pages
                '
                If ChildOrderByClause <> "" Then
                    SQLOrderBy = ChildOrderByClause
                Else
                    childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
                    'childListSortMethodId = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).childListSortMethodId
                    If childListSortMethodId <> 0 Then
                        SQLOrderBy = c.GetSortMethodByID(childListSortMethodId)
                    End If
                End If
                '
                ' --- main_Get Parent Content Name
                '
                ParentContentID = 0
                ParentContentName = ""
                IsParentEditing = False
                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                If ParentID > 0 Then
                    parentPCCPtr = main_GetPCCPtr(ParentID, pagemanager_IsWorkflowRendering, False)
                    If parentPCCPtr > -1 Then
                        ParentContentID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, parentPCCPtr))
                        ParentContentName = c.metaData.getContentNameByID(ParentContentID)
                        If ParentContentName <> "" Then
                            IsParentEditing = c.authContext.isEditing(c, ParentContentName)
                        End If
                    End If
                End If
                'ParentContentID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).ContentControlID
                'ParentContentName = metaData.getContentNameByID(ParentContentID)
                'IsParentEditing = main_IsEditing(ParentContentName)
                '
                ' Child criteria is all children with main_oldCacheArray_CurrentPage as parentid
                '
                PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
                Criteria = "(ParentID=" & PageID & ")and(id<>" & PageID & ")"
                '
                'PageID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).Id
                '
                ' Populate PCCPtrs()
                '
                PCCPtr = pageManager_cache_pageContent_getFirstChildPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                PtrCnt = 0
                Do While PCCPtr >= 0
                    ReDim Preserve PCCPtrs(PtrCnt)
                    PCCPtrs(PtrCnt) = PCCPtr
                    PtrCnt = PtrCnt + 1
                    PCCPtr = pageManager_cache_pageContent_parentIdIndex.getNextPtrMatch(CStr(PageID))
                Loop
                If PtrCnt > 0 Then
                    PCCPtrsSorted = pageManager_cache_pageContent_getPtrsSorted(PCCPtrs, SQLOrderBy)
                    main_RenderCache_ChildBranch_PCCPtrs = PCCPtrsSorted
                    main_RenderCache_ChildBranch_PCCPtrCnt = PtrCnt
                End If
                For Ptr = 0 To PtrCnt - 1
                    PCCPtr = PCCPtrsSorted(Ptr)
                    If True Then
                        '
                        ' Store results in main_oldCacheArray_ChildBranch Storage
                        '
                        'If main_oldCacheArray_ChildBranchCount >= main_oldCacheArray_ChildBranchSize Then
                        '    main_oldCacheArray_ChildBranchSize = main_oldCacheArray_ChildBranchSize + 10
                        '    ReDim Preserve main_oldCacheArray_ChildBranch(main_oldCacheArray_ChildBranchSize)
                        'End If
                        'With main_oldCacheArray_ChildBranch(main_oldCacheArray_ChildBranchCount)
                        '    PageName = genericController.encodeText(main_pcc(PCC_Name, PCCPtr))
                        '    pageCaption = ""
                        '    pageCaption = genericController.encodeText(main_pcc(PCC_MenuHeadline, PCCPtr))
                        '    If pageCaption = "" Then
                        '        pageCaption = Trim(PageName)
                        '        If pageCaption = "" Then
                        '            pageCaption = "Related Page"
                        '        End If
                        '    End If
                        '    If genericController.vbInstr(1, pageCaption, "<ac", vbTextCompare) <> 0 Then
                        '        pageCaption = pageCaption & ACTagEnd
                        '    End If
                        '    '
                        '    ' remove crlf because not allowed (in main_RenderedNavigationStructure if nothing else)
                        '    '
                        '    PageName = genericController.vbReplace(PageName, vbCrLf, " ")
                        '    PageName = genericController.vbReplace(PageName, vbTab, " ")
                        '    pageCaption = genericController.vbReplace(pageCaption, vbCrLf, " ")
                        '    pageCaption = genericController.vbReplace(pageCaption, vbTab, " ")
                        '    '
                        '    .Name = PageName
                        '    .Active = genericController.EncodeBoolean(main_pcc(PCC_Active, PCCPtr))
                        '    .AllowInChildLists = genericController.EncodeBoolean(main_pcc(PCC_AllowInChildLists, PCCPtr))
                        '    .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, PCCPtr))
                        '    .MenuCaption = pageCaption
                        '    .Id = genericController.EncodeInteger(main_pcc(PCC_ID, PCCPtr))
                        '    .ListName = genericController.encodeText(main_pcc(PCC_ParentListName, PCCPtr))
                        '    .dateArchive =  genericController.EncodeDate(main_pcc(PCC_DateArchive, PCCPtr))
                        '    .DateExpires =  genericController.EncodeDate(main_pcc(PCC_DateExpires, PCCPtr))
                        '    .PubDate =  genericController.EncodeDate(main_pcc(PCC_PubDate, PCCPtr))
                        '    .copyFilename = genericController.encodeText(main_pcc(PCC_CopyFilename, PCCPtr))
                        '    .ContentControlID = genericController.EncodeInteger(main_pcc(PCC_ContentControlID, PCCPtr))
                        '    .IsDisplayed = False
                        '    .BriefFilename = genericController.encodeText(main_pcc(PCC_BriefFilename, PCCPtr))
                        '    .AllowBrief = genericController.EncodeBoolean(main_pcc(PCC_AllowBrief, PCCPtr))
                        '    ContentName = metaData.getContentNameByID(.ContentControlID)
                        '    If main_IsEditing(ContentName) Then
                        '        .RecordEditLink = main_GetRecordEditLink2(ContentName, .Id, True, .Name, True)
                        '    End If
                        '    .templateId = genericController.EncodeInteger(main_pcc(PCC_TemplateID, PCCPtr))
                        '    .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, PCCPtr))
                        '    .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, PCCPtr))
                        '    .headline = genericController.encodeText(main_pcc(PCC_Headline, PCCPtr))
                        '    MenuLinkOverRide = genericController.encodeText(main_pcc(PCC_Link, PCCPtr))
                        '
                        '    .LinkOverride = MenuLinkOverRide
                        '    .Link = main_GetPageLink4(.Id, "", True, False)
                        'End With
                        'main_oldCacheArray_ChildBranchCount = main_oldCacheArray_ChildBranchCount + 1
                    End If
                Next
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError13("main_LoadRenderCache_ChildBranch")
        End Sub
        '
        '===========================================================================
        '   Populate the root branch
        '===========================================================================
        '
        Public Sub main_LoadRenderCache(PageID As Integer, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("Proc00371")
            '
            Dim CSSort As Integer
            Dim SortOrder As String
            Dim Ptr As Integer
            Dim ParentID As Integer
            Dim childListSortMethodId As Integer
            '
            'main_oldCacheArray_ParentBranchCount = 0
            'main_oldCacheArray_ChildBranchCount = 0
            'main_oldCacheArray_CurrentPagePtr = -1
            ''
            '' ----- (NEW) Load from cache
            ''
            'main_oldCacheArray_CurrentPagePtr = pageManager_cache_pageContent_getPtr(PageID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
            '
            ' ----- Load the current page
            '
            Call pageManager_LoadRenderCache_CurrentPage(PageID, PageID, RootPageContentName, ArchivePage, SectionID, UseContentWatchLink)
            '
            If (pageManager_RedirectLink = "") And (main_RenderCache_CurrentPage_PCCPtr > -1) Then
                'If (pageManager_RedirectLink = "") And (main_oldCacheArray_CurrentPagePtr <> -1) Then
                '
                ' ----- Load Parent Branch
                '
                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
                If (Not main_RenderCache_CurrentPage_IsRootPage) Then
                    If ParentID = 0 Then
                        '
                        ' Error, current page is not the root page, but has no parent pages
                        '
                        Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                        pageManager_RedirectBecausePageNotFound = True
                        pageManager_RedirectReason = "The page could not be displayed for security reasons. All valid pages must either have a valid parent page, or be selected by a section as the section's root page. This page has neither a parent page or a section."
                        pageManager_RedirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
                    ElseIf ArchivePage Then
                        '
                        ' Archive section parent Branch
                        '
                        Call pageManager_LoadRenderCache_ParentBranch(PageID, rootPageId, RootPageContentName, ArchivePage, "", SectionID, UseContentWatchLink)
                    Else
                        '
                        ' Normal parent Branch
                        '
                        Call pageManager_LoadRenderCache_ParentBranch(PageID, rootPageId, RootPageContentName, ArchivePage, "", SectionID, UseContentWatchLink)
                    End If
                End If
                If pageManager_RedirectLink = "" Then
                    '
                    ' ----- load the child branch
                    '
                    childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
                    SortOrder = c.GetSortMethodByID(childListSortMethodId)
                    If SortOrder = "" Then
                        SortOrder = genericController.encodeText(OrderByClause)
                    End If
                    Call pageManager_LoadRenderCache_ChildBranch(SortOrder, main_RenderCache_CurrentPage_IsRenderingMode, ArchivePage, SectionID, UseContentWatchLink)
                    main_RenderCache_Loaded = True
                End If
            End If
            Exit Sub
            '
ErrorTrap:
            Call c.handleLegacyError13("main_LoadRenderCache")
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Sub pageManager_cache_pageContent_clear()
            On Error GoTo ErrorTrap 'Const Tn = "pageManager_cache_pageContent_clear" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            pageManager_cache_pageContent_rows = 0
            cache_pageContent = Nothing
            Call c.cache.setObject(pageManager_cache_pageContent_cacheName, cache_pageContent)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError4(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_clear", True)
        End Sub
        '
        '====================================================================================================
        '   page content cache
        '====================================================================================================
        '
        Public Sub pageManager_cache_pageContent_load(main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageContent_load")
            '
            Dim bag As String
            Dim Ticks As Integer
            Dim IDList2 As New coreFastStringClass
            Dim IDList As String
            Dim PageName As String
            Dim CS As Integer
            'dim dt as datatable
            Dim Ptr As Integer
            Dim SQL As String
            Dim SelectList As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim Criteria As String
            'dim buildversion As String
            Dim Id As Integer
            Dim ParentID As Integer
            Dim test As Object
            Dim arrayData() As Object
            Dim arrayTest As Object
            '
            ' Load cached PCC
            '
            pageManager_cache_pageContent_idIndex = New coreKeyPtrIndexClass
            pageManager_cache_pageContent_parentIdIndex = New coreKeyPtrIndexClass
            pageManager_cache_pageContent_nameIndex = New coreKeyPtrIndexClass
            pageManager_cache_pageContent_rows = 0
            '
            On Error Resume Next
            If Not main_IsWorkflowRendering Then
                arrayTest = c.cache.getObject(Of Object())(pageManager_cache_pageContent_cacheName)
                If Not IsNothing(arrayTest) Then
                    arrayData = DirectCast(arrayTest, Object())
                    If Not IsNothing(arrayData) Then
                        cache_pageContent = DirectCast(arrayData(0), String(,))
                        If Not IsNothing(cache_pageContent) Then
                            bag = DirectCast(arrayData(1), String)
                            If Err.Number = 0 Then
                                Call pageManager_cache_pageContent_idIndex.importPropertyBag(bag)
                                If Err.Number = 0 Then
                                    bag = DirectCast(arrayData(2), String)
                                    If Err.Number = 0 Then
                                        Call pageManager_cache_pageContent_nameIndex.importPropertyBag(bag)
                                        If Err.Number = 0 Then
                                            bag = DirectCast(arrayData(3), String)
                                            If Err.Number = 0 Then
                                                Call pageManager_cache_pageContent_parentIdIndex.importPropertyBag(bag)
                                                If Err.Number = 0 Then
                                                    pageManager_cache_pageContent_rows = UBound(cache_pageContent, 2) + 1
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            If pageManager_cache_pageContent_rows = 0 Then
                '
                Call c.debug_testPoint("pageManager_cache_pageContent_load, main_PCCCnt = 0, rebuild cache")
                '
                SelectList = pageManager_cache_pageContent_fieldList
                Criteria = ""
                cache_pageContent = c.db.GetContentRows("Page Content", Criteria, , False, SystemMemberID, (main_IsWorkflowRendering Or main_IsQuickEditing), , SelectList)
                If (cache_pageContent.Length > 0) Then
                    pageManager_cache_pageContent_rows = UBound(cache_pageContent, 2) + 1
                    If pageManager_cache_pageContent_rows > 0 Then
                        '
                        ' build id and name indexes
                        '
                        Ticks = GetTickCount
                        For Ptr = 0 To pageManager_cache_pageContent_rows - 1
                            Id = genericController.EncodeInteger(cache_pageContent(PCC_ID, Ptr))
                            PageName = genericController.encodeText(cache_pageContent(PCC_Name, Ptr))
                            IDList2.Add("," & CStr(Id))
                            Call pageManager_cache_pageContent_idIndex.setPtr(genericController.encodeText(Id), Ptr)
                            If PageName <> "" Then
                                Call pageManager_cache_pageContent_nameIndex.setPtr(PageName, Ptr)
                            End If
                            '
                            ' if menulinkoverride is encoded, decode it
                            '
                            If genericController.vbInstr(1, cache_pageContent(PCC_Link, Ptr), "%") <> 0 Then
                                cache_pageContent(PCC_Link, Ptr) = c.htmlDoc.main_DecodeUrl(cache_pageContent(PCC_Link, Ptr))
                            End If
                        Next
                        IDList = IDList2.Text
                        '
                        ' build parentid list -- after id list to check for orphas
                        '
                        For Ptr = 0 To pageManager_cache_pageContent_rows - 1
                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, Ptr))
                            If (InStr(1, "," & IDList & ",", "," & ParentID & ",") = 0) Then
                                ParentID = 0
                            End If
                            Call pageManager_cache_pageContent_parentIdIndex.setPtr(genericController.encodeText(ParentID), Ptr)
                        Next
                        Call pageManager_cache_pageContent_save()
                    End If
                End If
                '
                Call c.debug_testPoint("pageManager_cache_pageContent_load, building took [" & (GetTickCount - Ticks) & " msec]")
                '
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description & "", "pageManager_cache_pageContent_load", True, False)
        End Sub
        '
        '
        '
        Public Sub pageManager_cache_pageContent_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_pageContent_save")
            '
            Dim cacheArray() As Object
            ReDim cacheArray(3)
            '
            If Not pagemanager_IsWorkflowRendering() Then
                Call pageManager_cache_pageContent_idIndex.getPtr("test")
                Call pageManager_cache_pageContent_nameIndex.getPtr("test")
                Call pageManager_cache_pageContent_parentIdIndex.getPtr("test")
                '
                cacheArray(0) = cache_pageContent
                cacheArray(1) = pageManager_cache_pageContent_idIndex.exportPropertyBag
                cacheArray(2) = pageManager_cache_pageContent_nameIndex.exportPropertyBag
                cacheArray(3) = pageManager_cache_pageContent_parentIdIndex.exportPropertyBag
                Call c.cache.setObject(pageManager_cache_pageContent_cacheName, cacheArray)
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_pageContent_save")
        End Sub
        '
        '====================================================================================================
        '   Returns a pointer into the main_pcc(x,ptr) array
        '====================================================================================================
        '
        Public Function pageManager_cache_pageContent_getPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageContent_getPtr")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            Dim RS As Object
            '
            pageManager_cache_pageContent_getPtr = -1
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            If (PageID > 0) Then
                '
                ' pageid=0 just loads cache and returns -1 ptr
                '
                If pageManager_cache_pageContent_rows <= 0 Then
                    '
                ElseIf (pageManager_cache_pageContent_idIndex Is Nothing) Then
                    '
                Else
                    pageManager_cache_pageContent_getPtr = pageManager_cache_pageContent_idIndex.getPtr(CStr(PageID))
                    If pageManager_cache_pageContent_getPtr < 0 Then
                        '
                        ' This PageID is missing from cache - try to reload
                        '
                        Call logController.log_appendLog(c, "pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] not found in index, attempting cache reload")
                        Call pageManager_cache_pageContent_clear()
                        Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
                        If pageManager_cache_pageContent_rows > 0 Then
                            pageManager_cache_pageContent_getPtr = pageManager_cache_pageContent_idIndex.getPtr(CStr(PageID))
                        End If
                        If (pageManager_cache_pageContent_getPtr < 0) Then
                            ' do not through error, this can happen if someone deletes a page.
                            Call logController.log_appendLog(c, "pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] not found in cache after reload. ERROR")
                            'Call Err.Raise(ignoreInteger, "cpCoreClass", "pageManager_cache_pageContent_getPtr, pageID [" & PageID & "] reload failed. ERROR")
                            'Call AppendLog("pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] reload failed. ERROR")
                        End If
                        'CS = app.csOpen("page content", "id=" & PageID, "id", , , , "ID")
                        'If app.csv_IsCSOK(CS) Then
                        '    Call pageManager_cache_pageContent_updateRow(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
                        '    If (main_PCCCnt > 0) And Not (main_PCCIDIndex Is Nothing) Then
                        '        pageManager_cache_pageContent_getPtr = main_PCCIDIndex.GetPointer(CStr(PageID))
                        '    End If
                        'End If
                        'Call app.closeCS(CS)
                    End If
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getPtr", True, False)
        End Function
        '
        '====================================================================================================
        '   Returns a pointer into the pcc(x,ptr) array
        '====================================================================================================
        '
        Public Function pageManager_cache_pageContent_get(main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As String(,)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCC")
            '
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            pageManager_cache_pageContent_get = cache_pageContent
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_get", True, False)
        End Function
        '
        '====================================================================================================
        '   Returns a pointer into the pcc(x,ptr) array for the first child page
        '====================================================================================================
        '
        Public Function pageManager_cache_pageContent_getFirstChildPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCFirstChildPtr")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            '
            pageManager_cache_pageContent_getFirstChildPtr = -1
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            If pageManager_cache_pageContent_rows > 0 Then
                Ptr = pageManager_cache_pageContent_parentIdIndex.getPtr(CStr(PageID))
                If Ptr >= 0 Then
                    pageManager_cache_pageContent_getFirstChildPtr = Ptr
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getFirstChildPtr", True, False)
        End Function
        '
        '====================================================================================================
        '   Returns a pointer into the pcc(x,ptr) array for the first child page
        '====================================================================================================
        '
        Public Function pageManager_cache_pageContent_getFirstNamePtr(PageName As String, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCFirstNamePtr")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            '
            pageManager_cache_pageContent_getFirstNamePtr = -1
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            If pageManager_cache_pageContent_rows > 0 Then
                Ptr = pageManager_cache_pageContent_nameIndex.getPtr(PageName)
                If Ptr >= 0 Then
                    pageManager_cache_pageContent_getFirstNamePtr = Ptr
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getFirstNamePtr", True, False)
        End Function
        '
        '
        '
        Public Sub pageManager_cache_pageContent_updateRow(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
            ' must clear because there is no way to updae indexes
            Call pageManager_cache_pageContent_clear()
            Exit Sub
            '
            On Error GoTo ErrorTrap
            '
            Dim PageName As String
            Dim CS As Integer
            'dim dt as datatable
            Dim ColPtr As Integer
            Dim SQL As String
            Dim SelectList As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim Criteria As String
            'dim buildversion As String
            Dim Id As Integer
            Dim ParentID As Integer
            Dim test As Object
            Dim PCCRow As String(,)
            Dim RowPtr As Integer
            '
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            If pageManager_cache_pageContent_rows > 0 Then
                SelectList = pageManager_cache_pageContent_fieldList
                For RowPtr = 0 To pageManager_cache_pageContent_rows - 1
                    If genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr)) = PageID Then
                        Exit For
                    End If
                Next
                Criteria = "ID=" & PageID
                CS = c.db.cs_open("Page Content", Criteria, , False, ,,, SelectList)
                If Not c.db.cs_ok(CS) Then
                    '
                    ' Page Not Found
                    '
                    Call pageManager_cache_pageContent_removeRow(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
                Else
                    PCCRow = c.db.cs_getRows(CS)
                    '
                    ' page was found in the Db - find the entry in PCC
                    '
                    If RowPtr = pageManager_cache_pageContent_rows Then
                        '
                        ' Page not found in PCC - add a new entry
                        '
                        pageManager_cache_pageContent_rows = pageManager_cache_pageContent_rows + 1
                        ReDim Preserve cache_pageContent(PCC_ColCnt - 1, pageManager_cache_pageContent_rows - 1)
                    End If
                    '
                    ' Transfer data from Db data to the PCC
                    '
                    For ColPtr = 0 To UBound(cache_pageContent, 1)
                        cache_pageContent(ColPtr, RowPtr) = PCCRow(ColPtr, 0)
                    Next
                    '
                    ' build id and name indexes
                    '
                    Id = genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr))
                    PageName = genericController.encodeText(cache_pageContent(PCC_Name, RowPtr))
                    '
                    Call pageManager_cache_pageContent_idIndex.setPtr(genericController.encodeText(Id), RowPtr)
                    '
                    If PageName <> "" Then
                        Call pageManager_cache_pageContent_nameIndex.setPtr(PageName, RowPtr)
                    End If
                    '
                    If genericController.vbInstr(1, cache_pageContent(PCC_Link, RowPtr), "%") <> 0 Then
                        cache_pageContent(PCC_Link, RowPtr) = c.htmlDoc.main_DecodeUrl(cache_pageContent(PCC_Link, RowPtr))
                    End If
                    '
                    ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, RowPtr))
                    Call pageManager_cache_pageContent_parentIdIndex.setPtr(genericController.encodeText(ParentID), RowPtr)
                    '
                    Call pageManager_cache_pageContent_save()
                End If
                Call c.db.cs_Close(CS)
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_updateRow", True, False)
        End Sub
        '
        '
        '
        Public Sub pageManager_cache_pageContent_removeRow(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
            On Error GoTo ErrorTrap
            '
            Dim PageName As String
            Dim CS As Integer
            'dim dt as datatable
            Dim ColPtr As Integer
            Dim SQL As String
            Dim SelectList As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim Criteria As String
            'dim buildversion As String
            Dim Id As Integer
            Dim ParentID As Integer
            Dim test As Object
            Dim PCCRow As Object
            Dim RowPtr As Integer
            '
            '   can not remove rows from index - temp fix - do not remove rows from cache
            '
            Call pageManager_cache_pageContent_clear()
            Exit Sub
            '
            If pageManager_cache_pageContent_rows = 0 Then
                Call pageManager_cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
            End If
            If pageManager_cache_pageContent_rows > 0 Then
                '
                ' Find the row in the PCC
                '
                For RowPtr = 0 To pageManager_cache_pageContent_rows - 1
                    If genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr)) = PageID Then
                        Exit For
                    End If
                Next
                If RowPtr < pageManager_cache_pageContent_rows Then
                    '
                    ' Row was found
                    '
                    Do While RowPtr < (pageManager_cache_pageContent_rows - 1)
                        For ColPtr = 0 To UBound(cache_pageContent, 1)
                            cache_pageContent(ColPtr, RowPtr) = cache_pageContent(ColPtr, RowPtr + 1)
                        Next
                        RowPtr = RowPtr + 1
                    Loop
                    pageManager_cache_pageContent_rows = pageManager_cache_pageContent_rows - 1
                    ReDim Preserve cache_pageContent(PCC_ColCnt - 1, pageManager_cache_pageContent_rows - 1)
                    If False Then
                        ReDim Preserve cache_pageContent(PCC_ColCnt - 1, pageManager_cache_pageContent_rows)
                    End If
                End If
                If Not main_IsWorkflowRendering Then
                    Call c.cache.setObject("PCC", cache_pageContent)
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_removeRow", True, False)
        End Sub
        '
        '
        '
        Public Function pageManager_cache_pageContent_getPtrsSorted(PCCPtrs() As Integer, OrderByCriteria As String) As Integer()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCPtrsSorted")
            '
            Dim PtrStart As Integer
            Dim PtrEnd As Integer
            Dim PtrStep As Integer
            Dim PCCRowPtr As Integer
            Dim Ptr As Integer
            Dim Index As coreKeyPtrIndexClass
            Dim PCCSortFieldPtr As Integer
            Dim SortForward As Boolean
            Dim SortFieldName As String
            Dim PtrCnt As Integer
            Dim SortPtr As Integer
            Dim SortSplitCnt As Integer
            Dim SortSplit() As String
            Dim PCCPtrCnt As Integer
            Dim fieldType As Integer
            Dim StringValue As String
            Dim LongValue As Integer
            Dim DblValue As Double
            Dim DateValue As Date
            Dim SortFieldValue As String
            '
            ' Sort the ptrs
            '
            PCCPtrCnt = UBound(PCCPtrs) + 1
            If PCCPtrCnt > 0 Then
                SortSplit = Split(OrderByCriteria, ",")
                SortSplitCnt = UBound(SortSplit) + 1
                For SortPtr = 0 To SortSplitCnt - 1
                    SortFieldName = genericController.vbLCase(SortSplit(SortPtr))
                    SortForward = True
                    If genericController.vbInstr(1, SortFieldName, " asc", vbTextCompare) <> 0 Then
                        SortFieldName = genericController.vbReplace(SortFieldName, " asc", "")
                    ElseIf genericController.vbInstr(1, SortFieldName, " desc", vbTextCompare) <> 0 Then
                        SortFieldName = genericController.vbReplace(SortFieldName, " desc", "")
                        SortForward = False
                    End If
                    PCCSortFieldPtr = pageManager_cache_pageContent_getColPtr(SortFieldName)

                    Select Case SortFieldName
                        Case "id"
                            fieldType = FieldTypeIdInteger
                        Case "datearchive", "dateexpires", "pubdate", "dateadded", "modifieddate"
                            fieldType = FieldTypeIdDate
                        Case Else
                            fieldType = FieldTypeIdText
                    End Select
                    '
                    ' Store them in the index
                    '
                    If PCCSortFieldPtr >= 0 Then
                        Index = New coreKeyPtrIndexClass
                        For Ptr = 0 To PCCPtrCnt - 1
                            PCCRowPtr = PCCPtrs(Ptr)
                            StringValue = genericController.encodeText(cache_pageContent(PCCSortFieldPtr, PCCRowPtr))
                            Select Case fieldType
                                Case FieldTypeIdInteger
                                    LongValue = CInt(DblValue)
                                    SortFieldValue = genericController.GetIntegerString(LongValue, 10)
                                Case FieldTypeIdDate
                                    If Not IsDate(StringValue) Then
                                        SortFieldValue = "000000000000000000"
                                    Else
                                        DateValue = CDate(StringValue)
                                        DblValue = DateValue.ToOADate * CDbl(1440)
                                        LongValue = CInt(DblValue)
                                        SortFieldValue = genericController.GetIntegerString(LongValue, 10)
                                    End If
                                Case Else
                                    SortFieldValue = StringValue
                            End Select
                            Call Index.setPtr(SortFieldValue, PCCRowPtr)

                        Next
                        '
                        ' Store them back into PCCPtrs() in the correct order
                        '
                        If SortForward Then
                            PtrStart = 0
                            PtrEnd = PCCPtrCnt - 1
                            PtrStep = 1
                        Else
                            PtrStart = PCCPtrCnt - 1
                            PtrEnd = 0
                            PtrStep = -1
                        End If

                        PCCRowPtr = Index.getFirstPtr
                        For Ptr = PtrStart To PtrEnd Step PtrStep
                            PCCPtrs(Ptr) = PCCRowPtr
                            PCCRowPtr = genericController.EncodeInteger(Index.getNextPtr)
                        Next
                    End If
                Next
            End If
            pageManager_cache_pageContent_getPtrsSorted = PCCPtrs
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getPtrsSorted", True, False)
        End Function
        '
        '
        '
        Public Function pageManager_cache_pageContent_getColPtr(FieldName As String) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCColPtr")
            '
            pageManager_cache_pageContent_getColPtr = -1
            Select Case genericController.vbLCase(FieldName)
                Case "active"
                    pageManager_cache_pageContent_getColPtr = PCC_Active
                Case "allowchildlistdisplay"
                    pageManager_cache_pageContent_getColPtr = PCC_AllowChildListDisplay
                Case "allowhitnotification"
                    pageManager_cache_pageContent_getColPtr = PCC_AllowHitNotification
                Case "allowmetacontentnofollow"
                    pageManager_cache_pageContent_getColPtr = PCC_AllowMetaContentNoFollow
                Case "blockcontent"
                    pageManager_cache_pageContent_getColPtr = PCC_BlockContent
                Case "blockpage"
                    pageManager_cache_pageContent_getColPtr = PCC_BlockPage
                Case "BlockSourceID"
                    pageManager_cache_pageContent_getColPtr = PCC_BlockSourceID
                Case "brieffilename"
                    pageManager_cache_pageContent_getColPtr = PCC_BriefFilename
                Case "childlistsortmethodid"
                    pageManager_cache_pageContent_getColPtr = PCC_ChildListSortMethodID
                Case "childlistinstanceoptions"
                    pageManager_cache_pageContent_getColPtr = PCC_ChildListInstanceOptions
                Case "issecure"
                    pageManager_cache_pageContent_getColPtr = PCC_IsSecure
                Case "childpagesfound"
                    pageManager_cache_pageContent_getColPtr = PCC_ChildPagesFound
                Case "contactmemberid"
                    pageManager_cache_pageContent_getColPtr = PCC_ContactMemberID
                Case "contentcontrolid"
                    pageManager_cache_pageContent_getColPtr = PCC_ContentControlID
                Case "copyFilename"
                    pageManager_cache_pageContent_getColPtr = PCC_CopyFilename
                Case "customblockmessagefilename"
                    pageManager_cache_pageContent_getColPtr = PCC_CustomBlockMessageFilename
                Case "datearchive"
                    pageManager_cache_pageContent_getColPtr = PCC_DateArchive
                Case "dateexpires"
                    pageManager_cache_pageContent_getColPtr = PCC_DateExpires
                Case "headline"
                    pageManager_cache_pageContent_getColPtr = PCC_Headline
                Case "id"
                    pageManager_cache_pageContent_getColPtr = PCC_ID
                Case "jsendbody"
                    pageManager_cache_pageContent_getColPtr = PCC_JSEndBody
                Case "jshead"
                    pageManager_cache_pageContent_getColPtr = PCC_JSHead
                Case "jsfilename"
                    pageManager_cache_pageContent_getColPtr = PCC_JSFilename
                Case "jsonload"
                    pageManager_cache_pageContent_getColPtr = PCC_JSOnLoad
                Case "link"
                    pageManager_cache_pageContent_getColPtr = PCC_Link
                Case "menuheadline"
                    pageManager_cache_pageContent_getColPtr = PCC_MenuHeadline
                Case "name"
                    pageManager_cache_pageContent_getColPtr = PCC_Name
                Case "parentid"
                    pageManager_cache_pageContent_getColPtr = PCC_ParentID
                Case "parentlistname"
                    pageManager_cache_pageContent_getColPtr = PCC_ParentListName
                Case "pubdate"
                    pageManager_cache_pageContent_getColPtr = PCC_PubDate
                Case "sortorder"
                    pageManager_cache_pageContent_getColPtr = PCC_SortOrder
                Case "registrationgroupid"
                    pageManager_cache_pageContent_getColPtr = PCC_RegistrationGroupID
                Case "templateid"
                    pageManager_cache_pageContent_getColPtr = PCC_TemplateID
                Case "triggeraddgroupid"
                    pageManager_cache_pageContent_getColPtr = PCC_TriggerAddGroupID
                Case "triggerconditiongroupid"
                    pageManager_cache_pageContent_getColPtr = PCC_TriggerConditionGroupID
                Case "triggerconditionid"
                    pageManager_cache_pageContent_getColPtr = PCC_TriggerConditionID
                Case "triggerremovegroupid"
                    pageManager_cache_pageContent_getColPtr = PCC_TriggerRemoveGroupID
                Case "triggersendsystememailid"
                    pageManager_cache_pageContent_getColPtr = PCC_TriggerSendSystemEmailID
                Case "viewings"
                    pageManager_cache_pageContent_getColPtr = PCC_Viewings
                Case "dateadded"
                    pageManager_cache_pageContent_getColPtr = PCC_DateAdded
                Case "modifieddate"
                    pageManager_cache_pageContent_getColPtr = PCC_ModifiedDate
                Case "allowinmenus"
                    pageManager_cache_pageContent_getColPtr = PCC_AllowInMenus
                Case "allowinchildlists"
                    pageManager_cache_pageContent_getColPtr = PCC_AllowInChildLists
            End Select
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getColPtr", True, False)
        End Function
        '
        '
        '
        'Public Function main_ImportCollectionFile(CollectionFilename As String) As Boolean
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("ImportCollectionFile")
        '    '
        '        main_ImportCollectionFile = main_ImportCollection(app.publicFiles.ReadFile(CollectionFilename), False)
        '    '
        '    Exit Function
        'ErrorTrap:
        '    Call main_HandleClassError_RevArgs(Err.Number, Err.Source, Err.Description, "main_ImportCollectionFile", True, False)
        '    End Function
        '        '
        '        '
        '        '
        '        Public Function main_ImportCollection(ByVal CollectionFileData As String, IsNewBuild As Boolean) As Boolean
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("ImportCollection")
        '            '
        '            Dim builder As New builderClass(me)
        '            '
        '            Call app.publicFiles.SaveFile("Install/" & CStr(main_GetRandomLong_Internal()) & ".xml", CollectionFileData)
        '            Call builder.InstallAddons(IsNewBuild)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Call main_HandleClassError_RevArgs(Err.Number, Err.Source, Err.Description, "main_ImportCollection", True, False)
        '        End Function
        '
        '
        '
        Public Function main_IISReset() As Boolean
            Throw New NotImplementedException("iisReset not implemented, may not be needed with removal of activex")
            '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("IISReset")
            '            '

            '            Dim runAtServer As New runAtServerClass(Me)
            '            '
            '            If main_IsStreamWritten Then
            '                '
            '                ' Not as good. The IISReset page directs back to the root page after 30 seconds
            '                '
            '                Call writeAltBuffer("<script type=""text/javascript"">window.location.assign('/ccLib/Popup/WaitForIISReset.htm');</script>")
            '                Call main_FlushStream()
            '            Else
            '                '
            '                ' The IISReset page directs back to the referrer when service is alive again
            '                '
            '                Call main_Redirect2("/ccLib/Popup/WaitForIISReset.htm", "Redirecting to the 'waiting for issreset' page. If you pause before this redirect, the web server may be resetting and the next page will not be available, resulting in a 404 error. Wait 30 seconds and refresh this link.", False)
            '            End If
            '            ' added 3 seconds to the iisreset. delay here is wrong because the page needs to return and main_Get away before the delay
            '            'Call Threading.Thread.Sleep(3000)
            '            Call runAtServer.executeCmd("IISReset", "")
            '            '
            '            Exit Function
            'ErrorTrap:
            '            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "IISReset", True, False)
        End Function
        '
        '==========================================================================================================
        '
        '==========================================================================================================
        '
        Private Sub pageManager_cache_siteSection_load()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("IISReset")
            '
            Dim IDText As String
            Dim Name As String
            Dim Id As Integer
            Dim Ptr As Integer
            Dim list As String
            Dim styleId As Integer
            Dim SelectList As String
            Dim SQL As String
            Dim SSCSize As Integer
            Dim LastRecordID As Integer
            Dim RecordID As Integer
            Dim SSCArray() As String
            Dim hint As String
            Dim LoadIndexes As Boolean
            Dim SaveCache As Boolean
            '
            Dim cacheObject As Object()
            Dim cacheTest As Object
            Dim bag As String
            '
            ' Load cache
            '
            pageManager_cache_siteSection_rows = 0
            pageManager_cache_siteSection_IDIndex = New coreKeyPtrIndexClass
            pageManager_cache_siteSection_RootPageIDIndex = New coreKeyPtrIndexClass
            pageManager_cache_siteSection_NameIndex = New coreKeyPtrIndexClass
            '
            On Error Resume Next
            If Not pagemanager_IsWorkflowRendering() Then
                cacheTest = c.cache.getObject(Of Object())(pageManager_cache_siteSection_cacheName)
                If Not IsNothing(cacheTest) Then
                    cacheObject = DirectCast(cacheTest, Object())
                    If Not IsNothing(cacheObject) Then
                        cache_siteSection = DirectCast(cacheObject(0), String(,))
                        If Not IsNothing(cache_siteSection) Then
                            bag = DirectCast(cacheObject(1), String)
                            If Err.Number = 0 Then
                                Call pageManager_cache_siteSection_IDIndex.importPropertyBag(bag)
                                If Err.Number = 0 Then
                                    bag = DirectCast(cacheObject(2), String)
                                    If Err.Number = 0 Then
                                        Call pageManager_cache_siteSection_RootPageIDIndex.importPropertyBag(bag)
                                        If Err.Number = 0 Then
                                            bag = DirectCast(cacheObject(3), String)
                                            If Err.Number = 0 Then
                                                Call pageManager_cache_siteSection_NameIndex.importPropertyBag(bag)
                                                If Err.Number = 0 Then
                                                    pageManager_cache_siteSection_rows = UBound(cache_siteSection, 2) + 1
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            If pageManager_cache_siteSection_rows = 0 Then
                SelectList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody,JSFilename"
                cache_siteSection = c.db.GetContentRows("Site Sections", "(active<>0)", , False, SystemMemberID, (pagemanager_IsWorkflowRendering()), , SelectList)
                pageManager_cache_siteSection_rows = UBound(cache_siteSection, 2) + 1
                For Ptr = 0 To pageManager_cache_siteSection_rows - 1
                    '
                    ' ID Index
                    '
                    IDText = genericController.encodeText(cache_siteSection(SSC_ID, Ptr))
                    Call pageManager_cache_siteSection_IDIndex.setPtr(IDText, Ptr)
                    '
                    ' RootPageID Index
                    '
                    Id = genericController.EncodeInteger(cache_siteSection(SSC_RootPageID, Ptr))
                    If Id <> 0 Then
                        Call pageManager_cache_siteSection_RootPageIDIndex.setPtr(genericController.encodeText(Id), Ptr)
                    End If
                    '
                    ' Name Index
                    '
                    Name = genericController.encodeText(cache_siteSection(SSC_Name, Ptr))
                    If Name <> "" Then
                        Call pageManager_cache_siteSection_NameIndex.setPtr(Name, Ptr)
                    End If
                Next
                Call pageManager_cache_siteSection_save()
            End If
            '

            Exit Sub
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_siteSection_load")
        End Sub
        '
        '
        '
        Private Sub pageManager_cache_siteSection_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_siteSection_save")
            '
            Dim hint As String
            Dim cacheArray() As Object
            ReDim cacheArray(3)
            '
            Call pageManager_cache_siteSection_IDIndex.getPtr("test")
            Call pageManager_cache_siteSection_RootPageIDIndex.getPtr("test")
            Call pageManager_cache_siteSection_NameIndex.getPtr("test")
            '
            cacheArray(0) = cache_siteSection
            cacheArray(1) = pageManager_cache_siteSection_IDIndex.exportPropertyBag
            cacheArray(2) = pageManager_cache_siteSection_RootPageIDIndex.exportPropertyBag
            cacheArray(3) = pageManager_cache_siteSection_NameIndex.exportPropertyBag
            Call c.cache.setObject(pageManager_cache_siteSection_cacheName, cacheArray)
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_siteSection_save")
        End Sub
        '
        '====================================================================================================
        '
        '====================================================================================================
        '
        Public Function pageManager_cache_siteSection_getPtr(Id As Integer) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_siteSection_getPtr")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            '
            pageManager_cache_siteSection_getPtr = -1
            If Id > 0 Then
                If pageManager_cache_siteSection_rows = 0 Then
                    Call pageManager_cache_siteSection_load()
                End If
                If pageManager_cache_siteSection_rows > 0 Then
                    Ptr = pageManager_cache_siteSection_IDIndex.getPtr(CStr(Id))
                    If Ptr >= 0 Then
                        pageManager_cache_siteSection_getPtr = Ptr
                    End If
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("pageManager_cache_siteSection_getPtr")
        End Function
        '
        '====================================================================================================
        '
        '====================================================================================================
        '
        Public Sub pageManager_cache_siteSection_clear()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_siteSection_clear")
            '
            Call c.cache.invalidateContent("site sections")
            cache_siteSection = {}
            pageManager_cache_siteSection_rows = 0
            Call c.cache.setObject(pageManager_cache_siteSection_cacheName, cache_siteSection)
            'Call cmc_siteSectionCache_clear
            '
            Exit Sub
            '
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_siteSection_clear")
        End Sub
        '
        '====================================================================================================
        '
        '====================================================================================================
        '
        Public Function pageManager_cache_siteSection_get() As Object
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetSSC")
            '
            If pageManager_cache_siteSection_rows = 0 Then
                Call pageManager_cache_siteSection_load()
            End If
            pageManager_cache_siteSection_get = cache_siteSection
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_siteSection_get", True, False)
        End Function
        '
        '
        '
        Public Sub pageManager_cache_pageTemplate_load()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageTemplate_load")
            '
            ' Dim rsdomains as datatable
            Dim ruleList As String
            Dim IsSecure As Boolean
            Dim Id As Integer
            Dim Ptr As Integer
            Dim list As String
            Dim styleId As Integer
            Dim SelectList As String
            'dim dt as datatable
            Dim SQL As String
            Dim TCSize As Integer
            Dim LastTemplateID As Integer
            Dim templateId As Integer
            Dim cacheArray As String(,)
            Dim arrayData() As Object
            Dim arrayTest As Object
            Dim bag As String
            '
            ' Load cached TC
            '
            pageManager_cache_pageTemplate_rows = 0
            pageManager_cache_pageTemplate_contentIdindex = New coreKeyPtrIndexClass
            '
            On Error Resume Next
            If Not pagemanager_IsWorkflowRendering() Then
                arrayTest = c.cache.getObject(Of Object())(pageManager_cache_pageTemplate_cacheName)
                If Not IsNothing(arrayTest) Then
                    arrayData = DirectCast(arrayTest, Object())
                    If Not IsNothing(arrayData) Then
                        cache_pageTemplate = DirectCast(arrayData(0), String(,))
                        If Not IsNothing(cache_pageTemplate) Then
                            bag = DirectCast(arrayData(1), String)
                            If Err.Number = 0 Then
                                Call pageManager_cache_pageTemplate_contentIdindex.importPropertyBag(bag)
                                If Err.Number = 0 Then
                                    pageManager_cache_pageTemplate_rows = UBound(cache_pageContent, 2) + 1
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            '    If Not main_IsWorkflowRendering Then
            '        cache_pageTemplate = csv_Getcache(pageManager_cache_pageTemplate_cacheName)
            '        If IsEmpty(cache_pageTemplate) Then
            '            cache_pageTemplateCnt = 0
            '        ElseIf Not IsArray(cache_pageTemplate) Then
            '            cache_pageTemplateCnt = 0
            '        ElseIf IsNull(cache_pageTemplate) Then
            '            cache_pageTemplateCnt = 0
            '        Else
            '            cacheArray = cache_pageTemplate
            '            cache_pageTemplateCnt = UBound(cacheArray, 2) + 1
            '        End If
            '    End If
            If pageManager_cache_pageTemplate_rows = 0 Then
                '
                ' Load cache
                '
                SelectList = ""
                SQL = "select t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID,t.MobileStylesFilename,t.MobileBodyHTML,OtherHeadTags,BodyTag,t.JSFilename,t.IsSecure as IsSecure" _
                    & " from ccTemplates t" _
                    & " Left Join ccSharedStylesTemplateRules r on r.templateid=t.id" _
                    & " where (t.active<>0)" _
                    & " order by t.id"
                Dim dt As DataTable = c.db.executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    For Each rsDr As DataRow In dt.Rows
                        templateId = genericController.EncodeInteger(rsDr("ID"))
                        styleId = genericController.EncodeInteger(rsDr("styleid"))
                        If (templateId = LastTemplateID) Then
                            '
                            ' Another style for the same template
                            '
                            If styleId <> 0 Then
                                list = cacheArray(TC_SharedStylesIDList, Ptr)
                                If list <> "" Then
                                    list = list & ","
                                End If
                                cacheArray(TC_SharedStylesIDList, Ptr) = list & styleId
                            End If
                        Else
                            '
                            ' New template
                            '
                            LastTemplateID = templateId
                            If pageManager_cache_pageTemplate_rows >= TCSize Then
                                TCSize = TCSize + 100
                                ReDim Preserve cacheArray(TC_cnt, TCSize)
                            End If
                            Ptr = pageManager_cache_pageTemplate_rows
                            cacheArray(TC_ID, Ptr) = genericController.EncodeInteger(rsDr("ID")).ToString
                            cacheArray(TC_JSEndBody, Ptr) = genericController.encodeText(rsDr("JSEndBody"))
                            cacheArray(TC_JSInHeadLegacy, Ptr) = genericController.encodeText(rsDr("JSHead"))
                            cacheArray(TC_JSInHeadFilename, Ptr) = genericController.encodeText(rsDr("JSFilename"))
                            cacheArray(TC_JSOnLoad, Ptr) = genericController.encodeText(rsDr("JSOnLoad"))
                            cacheArray(TC_Name, Ptr) = genericController.encodeText(rsDr("Name"))
                            cacheArray(TC_Link, Ptr) = main_verifyTemplateLink(genericController.encodeText(rsDr("Link")))
                            '
                            cacheArray(TC_BodyHTML, Ptr) = genericController.encodeText(rsDr("BodyHTML"))
                            cacheArray(TC_SharedStylesIDList, Ptr) = styleId.ToString
                            cacheArray(TC_StylesFilename, Ptr) = genericController.encodeText(rsDr("StylesFilename"))
                            '
                            cacheArray(TC_MobileBodyHTML, Ptr) = genericController.encodeText(rsDr("MobileBodyHTML"))
                            ' do not support shared styles on Mobile templates yet
                            'cacheArray(TC_MobileSharedStylesIDList, Ptr) = StyleID
                            cacheArray(TC_MobileStylesFilename, Ptr) = genericController.encodeText(rsDr("MobileStylesFilename"))
                            cacheArray(TC_OtherHeadTags, Ptr) = genericController.encodeText(rsDr("OtherHeadTags"))
                            cacheArray(TC_BodyTag, Ptr) = genericController.encodeText(rsDr("BodyTag"))
                            cacheArray(TC_IsSecure, Ptr) = genericController.EncodeBoolean(rsDr("IsSecure")).ToString
                            '
                            ' gather c.domains for this templates
                            '
                            SQL = "select domainid from ccDomainTemplateRules where templateid=" & templateId
                            Dim dtdomains As DataTable = c.db.executeSql(SQL)
                            If dtdomains.Rows.Count > 0 Then
                                cacheArray(TC_DomainIdList, Ptr) = genericController.encodeText(dtdomains.Rows.Item(0))
                                'cacheArray(TC_DomainIdList, Ptr) = rsdomains.GetString(StringFormatEnum.adClipString, , "", ",")
                            Else
                                cacheArray(TC_DomainIdList, Ptr) = ""
                            End If
                            pageManager_cache_pageTemplate_rows = pageManager_cache_pageTemplate_rows + 1
                        End If

                    Next
                End If


                '
                cache_pageTemplate = cacheArray
                '
                If pageManager_cache_pageTemplate_rows > 0 Then
                    pageManager_cache_pageTemplate_contentIdindex = New coreKeyPtrIndexClass
                    For Ptr = 0 To pageManager_cache_pageTemplate_rows - 1
                        Id = genericController.EncodeInteger(cache_pageTemplate(TC_ID, Ptr))
                        Call pageManager_cache_pageTemplate_contentIdindex.setPtr(genericController.encodeText(Id), Ptr)
                    Next
                End If
                If Not pagemanager_IsWorkflowRendering() Then
                    Call pageManager_cache_pageTemplate_save()
                End If
            End If
            '
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_pageTemplate_load")
        End Sub
        '
        '
        '
        Public Sub pageManager_cache_pageTemplate_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_pageTemplate_save")
            '
            Dim cacheArray() As Object
            ReDim cacheArray(1)
            '
            Call pageManager_cache_pageTemplate_contentIdindex.getPtr("test")
            '
            cacheArray(0) = cache_pageTemplate
            cacheArray(1) = pageManager_cache_pageTemplate_contentIdindex.exportPropertyBag
            Call c.cache.setObject(pageManager_cache_pageTemplate_cacheName, cacheArray)
            '
            Exit Sub
ErrorTrap:
            Call c.handleLegacyError18("pageManager_cache_pageTemplate_save")
        End Sub
        '
        '
        '
        Public Sub pageManager_cache_pageTemplate_clear()
            On Error GoTo ErrorTrap 'Const Tn = "pageManager_cache_pageTemplate_clear": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            pageManager_cache_pageTemplate_rows = 0
            cache_pageTemplate = {}
            Call c.cache.setObject(pageManager_cache_pageTemplate_cacheName, cache_pageTemplate)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError4(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageTemplate_clear", True)
        End Sub
        '
        '====================================================================================================
        '   Returns a pointer into the cache_pageTemplate(x,ptr) array
        '====================================================================================================
        '
        Public Function pageManager_cache_pageTemplate_getPtr(Id As Integer) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageTemplate_getPtr")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            '
            pageManager_cache_pageTemplate_getPtr = -1
            If pageManager_cache_pageTemplate_rows = 0 Then
                Call pageManager_cache_pageTemplate_load()
            End If
            If pageManager_cache_pageTemplate_rows > 0 Then
                Ptr = pageManager_cache_pageTemplate_contentIdindex.getPtr(CStr(Id))
                If Ptr >= 0 Then
                    pageManager_cache_pageTemplate_getPtr = Ptr
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("pageManager_cache_pageTemplate_getPtr")
        End Function
        '
        '====================================================================================================
        '   main_GetTemplateLink
        '       Added to externals (aoDynamicMenu) can main_Get hard template links
        '====================================================================================================
        '
        Public Function main_GetTemplateLink(templateId As Integer) As String
            On Error GoTo ErrorTrap
            '
            Dim Ptr As Integer
            '
            If templateId > 0 Then
                If pageManager_cache_pageTemplate_rows <= 0 Then
                    Call pageManager_cache_pageTemplate_load()
                End If
                Ptr = pageManager_cache_pageTemplate_getPtr(templateId)
                If Ptr >= 0 Then
                    main_GetTemplateLink = genericController.encodeText(cache_pageTemplate(TC_Link, Ptr))
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError12("main_GetTemplateLink", "Trap")
        End Function
        '
        '=========================================================================================
        '   main_GetTCLink
        '       - try just returning the link field, and handling TC_isSecure with the PCC_IsSecure later
        '       - will also need to handle TC_domainId at some point anyway
        '
        '=========================================================================================
        '
        Private Function main_GetTCLink(TCPtr As Integer) As String
            '
            If TCPtr >= 0 Then
                main_GetTCLink = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
            End If
            Exit Function
            '
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetTCLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim Link As String
            Dim templateSecure As Boolean
            '
            If TCPtr >= 0 Then
                Link = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
                templateSecure = genericController.EncodeBoolean(cache_pageTemplate(TC_IsSecure, TCPtr))
                If Link <> "" Then
                    '
                    ' Link is included in template
                    '
                    If genericController.vbInstr(1, Link, "://", vbTextCompare) <> 0 Then
                        '
                        ' Template link is Full URL, IsSecure checkbox does nothing
                        '
                    Else
                        '
                        ' Template Link is short, verify it first
                        '
                        If Mid(Link, 1, 1) <> "/" Then
                            Link = "/" & Link
                        End If
                        Link = genericController.ConvertLinkToShortLink(Link, c.webServer.requestDomain, c.webServer.webServerIO_requestVirtualFilePath)
                        Link = genericController.EncodeAppRootPath(Link, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                        If templateSecure And (Not c.webServer.requestSecure) Then
                            '
                            ' Short Link, and IsSecure checked but current page is not secure
                            '
                            Link = "https://" & c.webServer.requestDomain & Link
                        ElseIf c.webServer.requestSecure And (Not templateSecure) Then
                            ' (*E) comment out this
                            '
                            ' Short link, Template is not secure, but current page is
                            '
                            Link = "http://" & c.webServer.requestDomain & Link
                        End If
                    End If
                Else
                    '
                    ' Link is not included in template
                    '
                    If templateSecure And (Not c.webServer.requestSecure) Then
                        '
                        ' Secure template but current page is not secure - return default link with ssl
                        '
                        Link = "https://" & c.webServer.requestDomain & requestAppRootPath & c.siteProperties.serverPageDefault
                    ElseIf c.webServer.requestSecure And (Not templateSecure) Then
                        ' (*E) comment out this
                        '
                        ' Short link, Template is not secure, but current page is  - return default link with ssl
                        '
                        ' (*D)
                        ' this is the problem
                        ' the site should hard redirect to a non-secure template if the page, parent pages AND the template are not secure
                        ' what is happening here is a page is set secure, it redirects to the secure link then this
                        ' happens during the secure page draw.
                        '
                        Link = "http://" & c.webServer.requestDomain & requestAppRootPath & c.siteProperties.serverPageDefault
                    End If
                End If
                main_GetTCLink = Link
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError18("main_GetTCLink")
        End Function
        '
        Public Function main_GetPCCPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
            main_GetPCCPtr = pageManager_cache_pageContent_getPtr(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
        End Function
        Public Property pageManager_cache_pageContent_needsReload() As Boolean
            Get
                Return _pageManager_cache_pageContent_needsReload
            End Get
            Set(ByVal value As Boolean)
                _pageManager_cache_pageContent_needsReload = value
            End Set
        End Property
        Public Property pageManager_cache_pageContent_rows() As Integer
            Get
                Return _pageManager_cache_pageContent_rows
            End Get
            Set(ByVal value As Integer)
                _pageManager_cache_pageContent_rows = value
            End Set
        End Property

        '
        '========================================================================
        '   Returns the entire HTML page based on the bid/sid stream values
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Function pageManager_execute() As String
            Dim returnDoc As String = ""
            Try
                Dim downloadId As Integer
                Dim Pos As Integer
                Dim htmlBody As String
                Dim htmlHead As String
                Dim TPMode As Integer
                Dim EndOfBody As String
                Dim bodyTag As String
                Dim ToolsPanel As String
                Dim bodyAddonId As Integer
                Dim bodyAddonStatusOK As Boolean
                '
                Dim Clip As String
                Dim ClipParentRecordID As Integer
                Dim ClipParentContentID As Integer
                Dim ClipParentContentName As String
                Dim ClipChildContentID As Integer
                Dim ClipChildContentName As String
                Dim ClipChildRecordID As Integer
                Dim ClipChildRecordName As String
                Dim CSClip As Integer
                'Dim ParentID As Integer
                'Dim BufferString As String
                Dim ClipBoardArray() As String
                Dim ClipBoard As String
                Dim ClipParentFieldList As String
                Dim Fields As String()
                Dim FieldCount As Integer
                '
                Dim NameValue As String
                Dim NameValues() As String
                Dim RedirectLink As String = ""
                Dim RedirectReason As String = ""
                Dim PageNotFoundReason As String = ""
                Dim PageNotFoundSource As String = ""
                Dim IsPageNotFound As Boolean = False
                '
                If c.docOpen Then
                    c.htmlDoc.main_AdminWarning = c.docProperties.getText("main_AdminWarningMsg")
                    c.htmlDoc.main_AdminWarningPageID = c.docProperties.getInteger("main_AdminWarningPageID")
                    c.htmlDoc.main_AdminWarningSectionID = c.docProperties.getInteger("main_AdminWarningSectionID")
                    '
                    '
                    '--------------------------------------------------------------------------
                    ' Add cookie test
                    '   Ajax and RemoteMethods do not support cookie test
                    '--------------------------------------------------------------------------
                    '
                    Dim AllowCookieTest As Boolean
                    AllowCookieTest = c.siteProperties.allowVisitTracking And (c.authContext.visit.PageVisits = 1)
                    If AllowCookieTest Then
                        Call c.htmlDoc.main_AddOnLoadJavascript2("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" & c.security.encodeToken(c.authContext.visit.ID, c.app_startTime) & "')};", "Cookie Test")
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   User form processing
                    '       if a form is created in the editor, process it by emailing and saving to the User Form Response content
                    '--------------------------------------------------------------------------
                    '
                    If c.docProperties.getInteger("ContensiveUserForm") = 1 Then
                        Dim FromAddress As String = c.siteProperties.getText("EmailFromAddress", "info@" & c.webServer.webServerIO_requestDomain)
                        Call c.email.sendForm(c.siteProperties.emailAdmin, FromAddress, "Form Submitted on " & c.webServer.webServerIO_requestReferer)
                        Dim cs As Integer = c.db.cs_insertRecord("User Form Response")
                        If c.db.cs_ok(cs) Then
                            Call c.db.cs_set(cs, "name", "Form " & c.webServer.requestReferrer)
                            Dim Copy As String = ""

                            For Each key As String In c.docProperties.getKeyList()
                                Dim docProperty As docPropertiesClass = c.docProperties.getProperty(key)
                                If (key.ToLower() <> "contensiveuserform") Then
                                    Copy &= docProperty.Name & "=" & docProperty.Value & vbCrLf
                                End If
                            Next
                            Call c.db.cs_set(cs, "copy", Copy)
                            Call c.db.cs_set(cs, "VisitId", c.authContext.visit.ID)
                        End If
                        Call c.db.cs_Close(cs)
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Contensive Form Page Processing
                    '--------------------------------------------------------------------------
                    '
                    If c.docProperties.getInteger("ContensiveFormPageID") <> 0 Then
                        Call pageManager_ProcessFormPage(c.docProperties.getInteger("ContensiveFormPageID"))
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Automatic Redirect to a full URL
                    '   If the link field of the record is an absolution address
                    '       rc = redirect contentID
                    '       ri = redirect content recordid
                    '--------------------------------------------------------------------------
                    '
                    c.htmlDoc.htmlDoc_RedirectContentID = (c.docProperties.getInteger("rc"))
                    If (c.htmlDoc.htmlDoc_RedirectContentID <> 0) Then
                        c.htmlDoc.htmlDoc_RedirectRecordID = (c.docProperties.getInteger("ri"))
                        If (c.htmlDoc.htmlDoc_RedirectRecordID <> 0) Then
                            Dim contentName As String = c.metaData.getContentNameByID(c.htmlDoc.htmlDoc_RedirectContentID)
                            If contentName <> "" Then
                                If c.main_RedirectByRecord_ReturnStatus(contentName, c.htmlDoc.htmlDoc_RedirectRecordID) Then
                                    '
                                    'Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    '
                                    c.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return c.htmlDoc.docBuffer
                                Else
                                    c.htmlDoc.main_AdminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>"
                                    c.htmlDoc.main_AdminWarningPageID = c.htmlDoc.htmlDoc_RedirectRecordID
                                End If
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Tools Panel Mode
                    '       tpmode=0 movestools panel to the bottom
                    '       tpmode=1 moves tools panel to the left
                    '       tpmode=2 moves tools panel to the right
                    '       tpmode=3 moves tools panel to the top
                    '--------------------------------------------------------------------------
                    '
                    If c.docProperties.getText("tpmode") <> "" Then
                        Call c.siteProperties.setProperty("ToolsPanelMode", c.docProperties.getInteger("tpmode"))
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Active Download hook
                    '       downloads are actually redirects back to here
                    '       RequestNameLibraryFileID has the Encoded ID in the ccLibraryFiles table
                    '--------------------------------------------------------------------------
                    '
                    If True Then
                        Dim libraryFilePtr As Integer
                        Dim libraryFileClicks As Integer
                        Dim link As String = ""
                        Dim RecordEID As String = c.docProperties.getText(RequestNameLibraryFileID)
                        If (RecordEID <> "") Then
                            Dim tokenDate As Date
                            Call c.security.decodeToken(RecordEID, downloadId, tokenDate)
                            'downloadId = main_DecodeKeyNumber(RecordEID)
                        End If
                        If downloadId <> 0 Then
                            '
                            ' ----- lookup record and set clicks
                            '
                            Call c.cache_libraryFiles_loadIfNeeded()
                            libraryFilePtr = c.cache_libraryFilesIdIndex.getPtr(CStr(downloadId))
                            If libraryFilePtr >= 0 Then
                                libraryFileClicks = genericController.EncodeInteger(c.cache_libraryFiles(LibraryFilesCache_clicks, libraryFilePtr))
                                link = genericController.encodeText(c.cache_libraryFiles(LibraryFilesCache_filename, libraryFilePtr))
                                Call c.db.executeSql("update cclibraryfiles set clicks=" & (libraryFileClicks + 1) & " where id=" & downloadId)
                            End If
                            If link <> "" Then
                                '
                                ' ----- create log entry
                                '
                                Dim CSPointer As Integer = c.db.cs_insertRecord("Library File Log")
                                If c.db.cs_ok(CSPointer) Then
                                    Call c.db.cs_set(CSPointer, "FileID", downloadId)
                                    Call c.db.cs_set(CSPointer, "VisitId", c.authContext.visit.ID)
                                    Call c.db.cs_set(CSPointer, "MemberID", c.authContext.user.ID)
                                End If
                                Call c.db.cs_Close(CSPointer)
                                '
                                ' ----- and go
                                '
                                Call c.webServer.webServerIO_Redirect2(c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, link), "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.", False)
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Process clipboard cut/paste
                    '--------------------------------------------------------------------------
                    '
                    Clip = c.docProperties.getText(RequestNameCut)
                    If (Clip <> "") Then
                        '
                        ' if a cut, load the clipboard
                        '
                        Call c.visitProperty.setProperty("Clipboard", Clip)
                        Call genericController.modifyLinkQuery(c.web_RefreshQueryString, RequestNameCut, "")
                    End If
                    ClipParentContentID = c.docProperties.getInteger(RequestNamePasteParentContentID)
                    ClipParentRecordID = c.docProperties.getInteger(RequestNamePasteParentRecordID)
                    ClipParentFieldList = c.docProperties.getText(RequestNamePasteFieldList)
                    If (ClipParentContentID <> 0) And (ClipParentRecordID <> 0) Then
                        '
                        ' Request for a paste, clear the cliboard
                        '
                        ClipBoard = c.visitProperty.getText("Clipboard", "")
                        Call c.visitProperty.setProperty("Clipboard", "")
                        Call genericController.ModifyQueryString(c.web_RefreshQueryString, RequestNamePasteParentContentID, "")
                        Call genericController.ModifyQueryString(c.web_RefreshQueryString, RequestNamePasteParentRecordID, "")
                        ClipParentContentName = c.metaData.getContentNameByID(ClipParentContentID)
                        If (ClipParentContentName = "") Then
                            ' state not working...
                        ElseIf (ClipBoard = "") Then
                            ' state not working...
                        Else
                            If Not c.authContext.isAuthenticatedContentManager(c, ClipParentContentName) Then
                                Call c.error_AddUserError("The paste operation failed because you are not a content manager of the Clip Parent")
                            Else
                                '
                                ' Current identity is a content manager for this content
                                '
                                Dim Position As Integer = genericController.vbInstr(1, ClipBoard, ".")
                                If Position = 0 Then
                                    Call c.error_AddUserError("The paste operation failed because the clipboard data is configured incorrectly.")
                                Else
                                    ClipBoardArray = Split(ClipBoard, ".")
                                    If UBound(ClipBoardArray) = 0 Then
                                        Call c.error_AddUserError("The paste operation failed because the clipboard data is configured incorrectly.")
                                    Else
                                        ClipChildContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                        ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                        If Not c.IsWithinContent(ClipChildContentID, ClipParentContentID) Then
                                            Call c.error_AddUserError("The paste operation failed because the destination location is not compatible with the clipboard data.")
                                        Else
                                            '
                                            ' the content definition relationship is OK between the child and parent record
                                            '
                                            ClipChildContentName = c.metaData.getContentNameByID(ClipChildContentID)
                                            If Not ClipChildContentName <> "" Then
                                                Call c.error_AddUserError("The paste operation failed because the clipboard data content is undefined.")
                                            Else
                                                If (ClipParentRecordID = 0) Then
                                                    Call c.error_AddUserError("The paste operation failed because the clipboard data record is undefined.")
                                                ElseIf main_IsChildRecord(ClipChildContentName, ClipParentRecordID, ClipChildRecordID) Then
                                                    Call c.error_AddUserError("The paste operation failed because the destination location is a child of the clipboard data record.")
                                                Else
                                                    '
                                                    ' the parent record is not a child of the child record (circular check)
                                                    '
                                                    ClipChildRecordName = "record " & ClipChildRecordID
                                                    CSClip = c.csOpenRecord(ClipChildContentName, ClipChildRecordID, True, True)
                                                    If Not c.db.cs_ok(CSClip) Then
                                                        Call c.error_AddUserError("The paste operation failed because the data record referenced by the clipboard could not found.")
                                                    Else
                                                        '
                                                        ' Paste the edit record record
                                                        '
                                                        ClipChildRecordName = c.db.cs_getText(CSClip, "name")
                                                        If ClipParentFieldList = "" Then
                                                            '
                                                            ' Legacy paste - go right to the parent id
                                                            '
                                                            If Not c.db.cs_isFieldSupported(CSClip, "ParentID") Then
                                                                Call c.error_AddUserError("The paste operation failed because the record you are pasting does not   support the necessary parenting feature.")
                                                            Else
                                                                Call c.db.cs_set(CSClip, "ParentID", ClipParentRecordID)
                                                            End If
                                                        Else
                                                            '
                                                            ' Fill in the Field List name values
                                                            '
                                                            Fields = Split(ClipParentFieldList, ",")
                                                            FieldCount = UBound(Fields) + 1
                                                            For FieldPointer = 0 To FieldCount - 1
                                                                Dim Pair As String
                                                                Pair = Fields(FieldPointer)
                                                                If Mid(Pair, 1, 1) = "(" And Mid(Pair, Len(Pair), 1) = ")" Then
                                                                    Pair = Mid(Pair, 2, Len(Pair) - 2)
                                                                End If
                                                                NameValues = Split(Pair, "=")
                                                                If UBound(NameValues) = 0 Then
                                                                    Call c.error_AddUserError("The paste operation failed because the clipboard data Field List is not configured correctly.")
                                                                Else
                                                                    If Not c.db.cs_isFieldSupported(CSClip, CStr(NameValues(0))) Then
                                                                        Call c.error_AddUserError("The paste operation failed because the clipboard data Field [" & CStr(NameValues(0)) & "] is not supported by the location data.")
                                                                    Else
                                                                        Call c.db.cs_set(CSClip, CStr(NameValues(0)), CStr(NameValues(1)))
                                                                    End If
                                                                End If
                                                            Next
                                                        End If
                                                        ''
                                                        '' Fixup Content Watch
                                                        ''
                                                        'ShortLink = main_ServerPathPage
                                                        'ShortLink = ConvertLinkToShortLink(ShortLink, main_ServerHost, main_ServerVirtualPath)
                                                        'ShortLink = genericController.modifyLinkQuery(ShortLink, "bid", CStr(ClipChildRecordID), True)
                                                        'Call main_TrackContentSet(CSClip, ShortLink)
                                                    End If
                                                    Call c.db.cs_Close(CSClip)
                                                    '
                                                    ' Set Child Pages Found and clear caches
                                                    '
                                                    CSClip = c.csOpen(ClipParentContentName, ClipParentRecordID, , , "ChildPagesFound")
                                                    If c.db.cs_ok(CSClip) Then
                                                        Call c.db.cs_set(CSClip, "ChildPagesFound", True.ToString)
                                                    End If
                                                    Call c.db.cs_Close(CSClip)
                                                    Call pageManager_cache_pageContent_clear()
                                                    If (c.siteProperties.allowWorkflowAuthoring And c.workflow.isWorkflowAuthoringCompatible(ClipChildContentName)) Then
                                                        '
                                                        ' Workflow editing
                                                        '
                                                    Else
                                                        '
                                                        ' Live Editing
                                                        '
                                                        Call c.cache.invalidateContent(ClipChildContentName)
                                                        Call c.cache.invalidateContent(ClipParentContentName)
                                                        Call pageManager_cache_pageContent_clear()
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    Clip = c.docProperties.getText(RequestNameCutClear)
                    If (Clip <> "") Then
                        '
                        ' if a cut clear, clear the clipboard
                        '
                        Call c.visitProperty.setProperty("Clipboard", "")
                        Clip = c.visitProperty.getText("Clipboard", "")
                        Call genericController.modifyLinkQuery(c.web_RefreshQueryString, RequestNameCutClear, "")
                    End If
                    '
                    ' link alias and link forward
                    '
                    'Dim Custom404SourcePathPage As String = main_ServerPathPage ' refactor all of this
                    'Dim Custom404SourceNoQueryString As String = main_ServerPathPage
                    ' Dim Custom404SourceQueryString As String = main_ServerQueryString
                    If True Then
                        If True Then
                            Dim LinkAliasCriteria As String = ""
                            Dim linkAliasTest1 As String = ""
                            Dim linkAliasTest2 As String
                            Dim LinkNoProtocol As String = ""
                            Dim linkDomain As String = ""
                            Dim LinkFullPath As String = ""
                            Dim LinkFullPathNoSlash As String = ""
                            Dim isLinkForward As Boolean = False
                            Dim LinkForwardCriteria As String = ""
                            Dim Sql As String = ""
                            Dim CSPointer As Integer = -1
                            Dim IsInLinkForwardTable As Boolean = False
                            Dim Viewings As Integer = 0
                            Dim LinkSplit As String()
                            Dim IsLinkAlias As Boolean = False
                            '
                            '--------------------------------------------------------------------------
                            ' try link alias
                            '--------------------------------------------------------------------------
                            '
                            LinkAliasCriteria = ""
                            linkAliasTest1 = c.webServer.requestPathPage
                            If (linkAliasTest1.Substring(0, 1) = "/") Then
                                linkAliasTest1 = linkAliasTest1.Substring(1)
                            End If
                            If linkAliasTest1.Length > 0 Then
                                If (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) = "/") Then
                                    linkAliasTest1 = linkAliasTest1.Substring(0, linkAliasTest1.Length - 1)
                                End If
                            End If

                            linkAliasTest2 = linkAliasTest1 & "/"
                            If (Not IsPageNotFound) And (c.webServer.requestPathPage <> "") Then
                                '
                                ' build link variations needed later
                                '
                                '
                                Pos = genericController.vbInstr(1, c.webServer.requestPathPage, "://", vbTextCompare)
                                If Pos <> 0 Then
                                    LinkNoProtocol = Mid(c.webServer.requestPathPage, Pos + 3)
                                    Pos = genericController.vbInstr(Pos + 3, c.webServer.requestPathPage, "/", vbBinaryCompare)
                                    If Pos <> 0 Then
                                        linkDomain = Mid(c.webServer.requestPathPage, 1, Pos - 1)
                                        LinkFullPath = Mid(c.webServer.requestPathPage, Pos)
                                        '
                                        ' strip off leading or trailing slashes, and return only the string between the leading and secton slash
                                        '
                                        If genericController.vbInstr(1, LinkFullPath, "/") <> 0 Then
                                            LinkSplit = Split(LinkFullPath, "/")
                                            LinkFullPathNoSlash = LinkSplit(0)
                                            If LinkFullPathNoSlash = "" Then
                                                If UBound(LinkSplit) > 0 Then
                                                    LinkFullPathNoSlash = LinkSplit(1)
                                                End If
                                            End If
                                        End If
                                        linkAliasTest1 = LinkFullPath
                                        linkAliasTest2 = LinkFullPathNoSlash
                                    End If
                                End If
                                '
                                '   if this has not already been recognized as a pagenot found, and the custom404source is present, try all these
                                '   Build LinkForwardCritia and LinkAliasCriteria
                                '   sample: http://www.a.com/kb/test
                                '   LinkForwardCriteria = (Sourcelink='http://www.a.com/kb/test')or(Sourcelink='http://www.a.com/kb/test/')
                                '
                                Call c.cache_linkForward_load()
                                If c.cache_linkForward <> "" Then
                                    If 0 < genericController.vbInstr(1, c.cache_linkForward, "," & c.webServer.requestPathPage & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & c.db.encodeSQLText(c.webServer.requestPathPage) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, c.cache_linkForward, "," & c.webServer.requestPathPage & "/,", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & c.db.encodeSQLText(c.webServer.requestPathPage & "/") & ")"
                                    ElseIf 0 < genericController.vbInstr(1, c.cache_linkForward, "," & LinkNoProtocol & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & c.db.encodeSQLText(LinkNoProtocol) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, c.cache_linkForward, "," & LinkFullPath & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & c.db.encodeSQLText(LinkFullPath) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, c.cache_linkForward, "," & LinkFullPathNoSlash & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & c.db.encodeSQLText(LinkFullPathNoSlash) & ")"
                                    End If
                                    If isLinkForward Then
                                        '
                                        ' if match, go look it up and verify all OK
                                        '
                                        isLinkForward = False
                                        Sql = c.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID", , 1)
                                        CSPointer = c.db.cs_openSql(Sql)
                                        If c.db.cs_ok(CSPointer) Then
                                            '
                                            ' Link Forward found - update count
                                            '
                                            Dim Link As String
                                            Dim GroupID As Integer
                                            Dim groupName As String
                                            '
                                            IsInLinkForwardTable = True
                                            Viewings = c.db.cs_getInteger(CSPointer, "Viewings") + 1
                                            Sql = "update ccLinkForwards set Viewings=" & Viewings & " where ID=" & c.db.cs_getInteger(CSPointer, "ID")
                                            Call c.db.executeSql(Sql)
                                            Link = c.db.cs_getText(CSPointer, "DestinationLink")
                                            If Link <> "" Then
                                                '
                                                ' Valid Link Forward (without link it is just a record created by the autocreate function
                                                '
                                                isLinkForward = True
                                                Link = c.db.cs_getText(CSPointer, "DestinationLink")
                                                GroupID = c.db.cs_getInteger(CSPointer, "GroupID")
                                                If GroupID <> 0 Then
                                                    groupName = c.group_GetGroupName(GroupID)
                                                    If groupName <> "" Then
                                                        Call c.group_AddGroupMember(groupName)
                                                    End If
                                                End If
                                                If Link <> "" Then
                                                    RedirectLink = Link
                                                    RedirectReason = "Redirecting because the URL is a valid Link Forward entry."
                                                End If
                                            End If
                                        End If
                                        Call c.db.cs_Close(CSPointer)
                                    End If
                                End If
                                '
                                If (RedirectLink = "") And Not isLinkForward Then
                                    '
                                    ' Test for Link Alias
                                    '
                                    If (linkAliasTest1 & linkAliasTest2 <> "") Then
                                        Dim Ptr As Integer = c.cache_linkAlias_getPtrByName(linkAliasTest1)
                                        If (Ptr < 0) Then
                                            Ptr = c.cache_linkAlias_getPtrByName(linkAliasTest2)
                                        End If
                                        If Ptr >= 0 Then
                                            '
                                            ' Link Alias Found
                                            '
                                            IsLinkAlias = True
                                            '
                                            ' New Way - use pageid and QueryStringSuffix
                                            '
                                            Dim LinkQueryString As String = "bid=" & c.cache_linkAlias(linkAliasCache_pageId, Ptr) & "&" & c.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr)
                                            c.docProperties.setProperty("bid", c.cache_linkAlias(linkAliasCache_pageId, Ptr), False)
                                            Dim nameValuePairs As String() = Split(c.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
                                            For Each nameValuePair As String In nameValuePairs
                                                Dim nameValueThing As String() = Split(nameValuePair, "=")
                                                If (nameValueThing.GetUpperBound(0) = 0) Then
                                                    c.docProperties.setProperty(nameValueThing(0), "", False)
                                                Else
                                                    c.docProperties.setProperty(nameValueThing(0), nameValueThing(1), False)
                                                End If
                                            Next
                                        End If
                                    End If
                                    '
                                    If Not IsLinkAlias Then
                                        '
                                        ' Test for favicon.ico
                                        '
                                        If (LCase(c.webServer.requestPathPage) = "/favicon.ico") Then
                                            '
                                            ' Handle Favicon.ico when the client did not recognize the meta tag
                                            '
                                            Dim Filename As String = c.siteProperties.getText("FaviconFilename", "")
                                            If Filename = "" Then
                                                '
                                                ' no favicon, 404 the call
                                                '
                                                Call c.webServer.web_setResponseStatus("404 Not Found")
                                                Call c.webServer.webServerIO_setResponseContentType("image/gif")
                                                c.docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return c.htmlDoc.docBuffer
                                            Else
                                                Call c.webServer.webServerIO_Redirect2(c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, Filename), "favicon request", False)
                                                c.docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return c.htmlDoc.docBuffer
                                            End If
                                        End If
                                        '
                                        ' Test for robots.txt
                                        '
                                        If (LCase(LinkFullPathNoSlash) = "robots.txt") Or (LCase(LinkFullPathNoSlash) = "robots_txt") Then
                                            '
                                            ' Handle Robots.txt file
                                            '
                                            Dim Filename As String = "config/RobotsTxtBase.txt"
                                            ' set this way because the preferences page needs a filename in a site property (enhance later)
                                            Call c.siteProperties.setProperty("RobotsTxtFilename", Filename)
                                            Dim Content As String = c.cdnFiles.readFile(Filename)
                                            If Content = "" Then
                                                '
                                                ' save default robots.txt
                                                '
                                                Content = "User-agent: *" & vbCrLf & "Disallow: /admin/" & vbCrLf & "Disallow: /images/"
                                                Call c.appRootFiles.saveFile(Filename, Content)
                                            End If
                                            Content = Content & c.addonCache.addonCache.robotsTxt
                                            Call c.webServer.webServerIO_setResponseContentType("text/plain")
                                            Call c.htmlDoc.writeAltBuffer(Content)
                                            c.docOpen = False '--- should be disposed by caller --- Call dispose
                                            Return c.htmlDoc.docBuffer
                                        End If
                                        '
                                        ' No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                                        '
                                        If (c.app_errorCount = 0) And c.siteProperties.getBoolean("LinkForwardAutoInsert") And (Not IsInLinkForwardTable) Then
                                            '
                                            ' Add a new Link Forward entry
                                            '
                                            CSPointer = c.InsertCSContent("Link Forwards")
                                            If c.db.cs_ok(CSPointer) Then
                                                Call c.db.cs_set(CSPointer, "Name", c.webServer.requestPathPage)
                                                Call c.db.cs_set(CSPointer, "sourcelink", c.webServer.requestPathPage)
                                                Call c.db.cs_set(CSPointer, "Viewings", 1)
                                            End If
                                            Call c.db.cs_Close(CSPointer)
                                        End If
                                        '
                                        ' real 404
                                        '
                                        IsPageNotFound = True
                                        PageNotFoundSource = c.webServer.requestPathPage
                                        PageNotFoundReason = "The page could not be displayed because the URL is not a valid page, Link Forward, Link Alias or RemoteMethod."
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' ----- do anonymous access blocking
                    '
                    If Not c.authContext.isAuthenticated() Then
                        If (c.webServer.webServerIO_requestPath <> "/") And genericController.vbInstr(1, c.siteProperties.adminURL, c.webServer.webServerIO_requestPath, vbTextCompare) <> 0 Then
                            '
                            ' admin page is excluded from custom blocking
                            '
                        Else
                            Dim AnonymousUserResponseID As Integer = genericController.EncodeInteger(c.siteProperties.getText("AnonymousUserResponseID", "0"))
                            Select Case AnonymousUserResponseID
                                Case 1
                                    '
                                    ' block with login
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3410 - exit for login block")
                                    '
                                    Call c.main_SetMetaContent(0, 0)
                                    Call c.htmlDoc.writeAltBuffer(c.htmlDoc.getLoginPage(False) & c.htmlDoc.html_GetEndOfBody(False, False, False, False))
                                    c.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return c.htmlDoc.docBuffer
                                Case 2
                                    '
                                    ' block with custom content
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3420 - exit for custom content block")
                                    '
                                    Call c.main_SetMetaContent(0, 0)
                                    Call c.htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll'", "Anonymous User Block")
                                    Dim Copy As String = cr & c.htmlDoc.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=""width:250px;margin:100px auto auto auto;"">The site is currently not available for anonymous access.</p>", c.authContext.user.ID, True, c.authContext.isAuthenticated)
                                    ' -- already encoded
                                    'Copy = EncodeContentForWeb(Copy, "copy content", 0, "", 0)
                                    Copy = "" _
                                            & c.main_docType _
                                            & vbCrLf & "<html>" _
                                            & cr & "<head>" _
                                            & genericController.kmaIndent(c.main_GetHTMLHead()) _
                                            & cr & "</head>" _
                                            & cr & TemplateDefaultBodyTag _
                                            & genericController.kmaIndent(Copy) _
                                            & cr2 & "<div>" _
                                            & cr3 & c.htmlDoc.html_GetEndOfBody(True, True, False, False) _
                                            & cr2 & "</div>" _
                                            & cr & "</body>" _
                                            & vbCrLf & "</html>"
                                    '& "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                                    Call c.htmlDoc.writeAltBuffer(Copy)
                                    c.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return c.htmlDoc.docBuffer
                            End Select
                        End If
                    End If
                    '-------------------------------------------
                    '
                    ' run the appropriate body addon
                    '
                    bodyAddonId = genericController.EncodeInteger(c.siteProperties.getText("Html Body AddonId", "0"))
                    If bodyAddonId <> 0 Then
                        htmlBody = c.addon.execute(bodyAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", bodyAddonStatusOK, Nothing, "", Nothing, "", c.authContext.user.ID, c.authContext.isAuthenticated)
                    Else
                        htmlBody = pageManager_GetHtmlBody()
                    End If
                    If c.docOpen Then
                        '
                        ' Build Body Tag
                        '
                        htmlHead = c.main_GetHTMLHead()
                        If pageManager_TemplateBodyTag <> "" Then
                            bodyTag = pageManager_TemplateBodyTag
                        Else
                            bodyTag = TemplateDefaultBodyTag
                        End If
                        '
                        ' Add tools panel to body
                        '
                        htmlBody = htmlBody & cr & "<div>" & genericController.kmaIndent(c.htmlDoc.html_GetEndOfBody(True, True, False, False)) & cr & "</div>"
                        '
                        ' build doc
                        '
                        returnDoc = c.main_assembleHtmlDoc(c.main_docType, htmlHead, bodyTag, c.responseBuffer & htmlBody)
                    End If
                End If
                '
                ' all other routes should be handled here.
                '   - this code is in initApp right now but should be migrated here.
                '   - if all other routes fail, use the defaultRoute (pagemanager at first)
                '
                If True Then
                    ' --- not reall sure what to do with this - was in appInit() and I am just sure it does not go there.
                    '
                    '--------------------------------------------------------------------------
                    ' ----- check if the custom404pathpage matches the defaultdoc
                    '       in this case, the 404 hit is a direct result of a 404 I justreturned to IIS
                    '       currently, I am redirecting to the page-not-found page with a 404 - wrong
                    '       I should realize here that this is a 404 caused by the page in the 404 custom string
                    '           and display the 404 page. Even if all I can say is "the page was not found"
                    '
                    '--------------------------------------------------------------------------
                    '
                    If genericController.vbLCase(c.webServer.requestPathPage) = genericController.vbLCase(requestAppRootPath & c.siteProperties.serverPageDefault) Then
                        '
                        ' This is a 404 caused by Contensive returning a 404
                        '   possibly because the pageid was not found or was inactive.
                        '   contensive returned a 404 error, and the IIS custom error handler is hitting now
                        '   what we returned as an error cause is lost
                        '   ( because the Custom404Source page is the default page )
                        '   send it to the 404 page
                        '
                        c.webServer.requestPathPage = c.webServer.requestPathPage
                        IsPageNotFound = True
                        PageNotFoundReason = "The page could not be displayed. The record may have been deleted, marked inactive. The page's parent pages or section may be invalid."
                    End If
                End If
                If True Then
                    '
                    ' Determine where to go next
                    '   If the current page is not the referring page, redirect to the referring page
                    '   Because...
                    '   - the page with the form (the referrer) was a link alias page. You can not post to a link alias, so internally we post to the default page, and redirect back.
                    '   - This only acts on internal Contensive forms, so developer pages are not effected
                    '   - This way, if the form post comes from a main_GetJSPage Remote Method, it posts to the Content Server,
                    '       then redirects back to the static site (with the new changed content)
                    '
                    If c.webServer.requestReferrer <> "" Then
                        Dim main_ServerReferrerURL As String
                        Dim main_ServerReferrerQs As String
                        Dim Position As Integer
                        main_ServerReferrerURL = c.webServer.requestReferrer
                        main_ServerReferrerQs = ""
                        Position = genericController.vbInstr(1, main_ServerReferrerURL, "?")
                        If Position <> 0 Then
                            main_ServerReferrerQs = Mid(main_ServerReferrerURL, Position + 1)
                            main_ServerReferrerURL = Mid(main_ServerReferrerURL, 1, Position - 1)
                        End If
                        If Right(main_ServerReferrerURL, 1) = "/" Then
                            '
                            ' Referer had no page, figure out what it should have been
                            '
                            If c.webServer.webServerIO_requestPage <> "" Then
                                '
                                ' If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
                                '
                                main_ServerReferrerURL = main_ServerReferrerURL & c.webServer.webServerIO_requestPage
                            Else
                                main_ServerReferrerURL = main_ServerReferrerURL & c.siteProperties.serverPageDefault
                            End If
                        End If
                        Dim linkDst As String
                        'main_ServerPage = main_ServerPage
                        If main_ServerReferrerURL <> c.webServer.webServerIO_ServerFormActionURL Then
                            '
                            ' remove any methods from referrer
                            '
                            Dim Copy As String
                            Copy = "Redirecting because a Contensive Form was detected, source URL [" & main_ServerReferrerURL & "] does not equal the current URL [" & c.webServer.webServerIO_ServerFormActionURL & "]. This may be from a Contensive Add-on that now needs to redirect back to the host page."
                            linkDst = c.webServer.webServerIO_requestReferer
                            If main_ServerReferrerQs <> "" Then
                                linkDst = main_ServerReferrerURL
                                main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "")
                                If main_ServerReferrerQs <> "" Then
                                    linkDst = linkDst & "?" & main_ServerReferrerQs
                                End If
                            End If
                            Call c.webServer.webServerIO_Redirect2(linkDst, Copy, False)
                            c.docOpen = False '--- should be disposed by caller --- Call dispose
                        End If
                    End If
                End If
                If True Then
                    ' - same here, this was in appInit() to prcess the pagenotfounds - maybe here (at the end, maybe in pageManager)
                    '--------------------------------------------------------------------------
                    ' ----- Process Early page-not-found
                    '--------------------------------------------------------------------------
                    '
                    If IsPageNotFound Then
                        If True Then
                            '
                            ' new way -- if a (real) 404 page is received, just convert this hit to the page-not-found page, do not redirect to it
                            '
                            Call c.log_appendLogPageNotFound(c.webServer.requestLinkSource)
                            Call c.webServer.web_setResponseStatus("404 Not Found")
                            c.docProperties.setProperty("bid", main_GetPageNotFoundPageId())
                            'Call main_mergeInStream("bid=" & main_GetPageNotFoundPageId())
                            If c.authContext.isAuthenticatedAdmin(c) Then
                                c.htmlDoc.main_AdminWarning = PageNotFoundReason
                                c.htmlDoc.main_AdminWarningPageID = 0
                                c.htmlDoc.main_AdminWarningSectionID = 0
                            End If
                        Else
                            '
                            ' old way -- if a (real) 404 page is received, redirect to it to the 404 page with content
                            '
                            RedirectReason = PageNotFoundReason
                            RedirectLink = main_ProcessPageNotFound_GetLink(PageNotFoundReason, , PageNotFoundSource)
                        End If
                    End If
                End If
                '
                ' add exception list header
                '
                returnDoc = c.getDocExceptionHtmlList() & returnDoc
                '
            Catch ex As Exception
                Call c.handleLegacyError18("main_GetHTMLDoc2")
            End Try
            Return returnDoc
        End Function
        '
        '========================================================================
        ' main_DeleteChildRecords
        '========================================================================
        '
        Public Function pageManager_DeleteChildRecords(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal ReturnListWithoutDelete As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteChildRecords")
            '
            Dim QuickEditing As Boolean
            Dim SQL As String
            'dim dt as datatable
            Dim IDList As String
            Dim IDs() As String
            Dim IDCnt As Integer
            Dim Ptr As Integer
            Dim CS As Integer
            '
            Dim ChildList As String
            Dim SingleEntry As Boolean
            '
            '
            ' For now, the child delete only works in non-workflow
            '
            CS = c.db.cs_open(ContentName, "parentid=" & RecordID, , , , ,, "ID")
            Do While c.db.cs_ok(CS)
                pageManager_DeleteChildRecords = pageManager_DeleteChildRecords & "," & c.db.cs_getInteger(CS, "ID")
                c.db.cs_goNext(CS)
            Loop
            Call c.db.cs_Close(CS)
            If pageManager_DeleteChildRecords <> "" Then
                pageManager_DeleteChildRecords = Mid(pageManager_DeleteChildRecords, 2)
                '
                ' main_Get a list of all pages, but do not delete anything yet
                '
                IDs = Split(pageManager_DeleteChildRecords, ",")
                IDCnt = UBound(IDs) + 1
                SingleEntry = (IDCnt = 1)
                For Ptr = 0 To IDCnt - 1
                    ChildList = pageManager_DeleteChildRecords(ContentName, genericController.EncodeInteger(IDs(Ptr)), True)
                    If ChildList <> "" Then
                        pageManager_DeleteChildRecords = pageManager_DeleteChildRecords & "," & ChildList
                        SingleEntry = False
                    End If
                Next
                If Not ReturnListWithoutDelete Then
                    '
                    ' Do the actual delete
                    '
                    IDs = Split(pageManager_DeleteChildRecords, ",")
                    IDCnt = UBound(IDs) + 1
                    SingleEntry = (IDCnt = 1)
                    QuickEditing = c.authContext.isQuickEditing(c, "page content")
                    For Ptr = 0 To IDCnt - 1
                        Call c.DeleteContentRecord("page content", genericController.EncodeInteger(IDs(Ptr)))
                        Call pageManager_cache_pageContent_removeRow(genericController.EncodeInteger(IDs(Ptr)), pagemanager_IsWorkflowRendering, QuickEditing)
                    Next
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError18("main_DeleteChildRecords")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Sub pageManager_GetAuthoringStatus(ByVal ContentName As String, ByVal RecordID As Integer, ByRef IsSubmitted As Boolean, ByRef IsApproved As Boolean, ByRef SubmittedName As String, ByRef ApprovedName As String, ByRef IsInserted As Boolean, ByRef IsDeleted As Boolean, ByRef IsModified As Boolean, ByRef ModifiedName As String, ByRef ModifiedDate As Date, ByRef SubmittedDate As Date, ByRef ApprovedDate As Date)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("main_GetAuthoringStatus")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_GetAuthoringStatus"
            '
            Call c.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        '   main_Get athoring permissions to determine what buttons we display, and what authoring actions we can take
        '
        '       RecordID = 0 means it is an unsaved inserted record, or this pertains to the content, not a record
        '
        '       AllowCancel - OK to exit without any action
        '       AllowInsert - OK to create new records, display ADD button
        '       AllowSave - OK to save the record, display the Save and OK Buttons
        '       etc.
        '========================================================================
        '
        Public Sub pageManager_GetAuthoringPermissions(ByVal ContentName As String, ByVal RecordID As Integer, ByRef AllowInsert As Boolean, ByRef AllowCancel As Boolean, ByRef allowSave As Boolean, ByRef AllowDelete As Boolean, ByRef AllowPublish As Boolean, ByRef AllowAbort As Boolean, ByRef AllowSubmit As Boolean, ByRef AllowApprove As Boolean, ByRef readOnlyField As Boolean)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00270")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            Dim IsEditing As Boolean
            Dim IsSubmitted As Boolean
            Dim IsApproved As Boolean
            Dim IsInserted As Boolean
            Dim IsDeleted As Boolean
            Dim IsModified As Boolean
            Dim EditingName As String
            Dim EditingExpires As Date
            Dim SubmittedName As String
            Dim ApprovedName As String
            Dim ModifiedName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim ModifiedDate As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            '
            MethodName = "main_GetAuthoringButtons"
            '
            ' main_Get Authoring Workflow Status
            '
            If RecordID <> 0 Then
                Call c.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate)
            End If
            '
            ' main_Get Content Definition
            '
            CDef = c.metaData.getCdef(ContentName)
            '
            ' Set Buttons based on Status
            '
            If IsEditing Then
                '
                ' Edit Locked
                '
                AllowCancel = True
                readOnlyField = True
            ElseIf (Not c.siteProperties.allowWorkflowAuthoring) Or (Not CDef.AllowWorkflowAuthoring) Then
                '
                ' No Workflow Authoring
                '
                AllowCancel = True
                allowSave = True
                If (CDef.AllowDelete) And (Not IsDeleted) And (RecordID <> 0) Then
                    AllowDelete = True
                End If
                If (CDef.AllowAdd) And (Not IsInserted) Then
                    AllowInsert = True
                End If
            ElseIf c.authContext.isAuthenticatedAdmin(c) Then
                '
                ' Workflow, Admin
                '
                If IsApproved Then
                    '
                    ' Workflow, Admin, Approved
                    '
                    AllowCancel = True
                    AllowPublish = True
                    AllowAbort = True
                    readOnlyField = True
                    AllowInsert = True
                ElseIf IsSubmitted Then
                    '
                    ' Workflow, Admin, Submitted (not approved)
                    '
                    AllowCancel = True
                    If Not IsDeleted Then
                        allowSave = True
                    End If
                    AllowPublish = True
                    AllowAbort = True
                    AllowApprove = True
                    If IsDeleted Then
                        readOnlyField = True
                    End If
                    AllowInsert = True
                ElseIf IsInserted Then
                    '
                    ' Workflow, Admin, Inserted (not submitted, not approved)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowPublish = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowApprove = True
                    AllowInsert = True
                ElseIf IsDeleted Then
                    '
                    ' Workflow, Admin, Deleted record (not submitted, not approved)
                    '
                    AllowCancel = True
                    AllowPublish = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowApprove = True
                    readOnlyField = True
                    AllowInsert = True
                ElseIf IsModified Then
                    '
                    ' Workflow, Admin, Modified (not submitted, not approved, not inserted, not deleted)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowPublish = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowApprove = True
                    AllowDelete = True
                    AllowInsert = True
                Else
                    '
                    ' Workflow, Admin, Not Modified (not submitted, not approved, not inserted, not deleted)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowPublish = True
                    AllowApprove = True
                    AllowSubmit = True
                    AllowDelete = True
                    AllowInsert = True
                End If
            Else
                '
                ' Workflow, Content Manager
                '
                If IsApproved Then
                    '
                    ' Workflow, Content Manager, Approved
                    '
                    AllowCancel = True
                    readOnlyField = True
                    AllowInsert = True
                ElseIf IsSubmitted Then
                    '
                    ' Workflow, Content Manager, Submitted (not approved)
                    '
                    AllowCancel = True
                    readOnlyField = True
                    AllowInsert = True
                ElseIf IsInserted Then
                    '
                    ' Workflow, Content Manager, Inserted (not submitted, not approved)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowInsert = True
                ElseIf IsDeleted Then
                    '
                    ' Workflow, Content Manager, Deleted record (not submitted, not approved)
                    '
                    AllowCancel = True
                    AllowAbort = True
                    AllowSubmit = True
                    readOnlyField = True
                    AllowInsert = True
                ElseIf IsModified Then
                    '
                    ' Workflow, Content Manager, Modified (not submitted, not approved, not inserted, not deleted)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowDelete = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowInsert = True
                Else
                    '
                    ' Workflow, Content Manager, Not Modified (not submitted, not approved, not inserted, not deleted)
                    '
                    AllowCancel = True
                    allowSave = True
                    AllowDelete = True
                    AllowAbort = True
                    AllowSubmit = True
                    AllowInsert = True
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '
        '
        Public Sub pageManager_SendPublishSubmitNotice(ByVal ContentName As String, ByVal RecordID As Integer, ByVal RecordName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00272")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim CDef As coreMetaDataClass.CDefClass
            Dim Copy As String
            Dim Link As String
            Dim FromAddress As String
            '
            MethodName = "main_SendPublishSubmitNotice"
            '
            FromAddress = c.siteProperties.getText("EmailPublishSubmitFrom", c.siteProperties.emailAdmin)
            CDef = c.metaData.getCdef(ContentName)
            Link = c.siteProperties.adminURL & "?af=" & AdminFormPublishing
            Copy = Msg_AuthoringSubmittedNotification
            Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=""" & c.htmlDoc.html_EncodeHTML(Link) & """>" & c.webServer.webServerIO_requestDomain & "</a>")
            Copy = genericController.vbReplace(Copy, "<RECORDNAME>", RecordName)
            Copy = genericController.vbReplace(Copy, "<CONTENTNAME>", ContentName)
            Copy = genericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDDATE>", c.app_startTime.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDNAME>", c.authContext.user.Name)
            '
            Call c.email.sendGroup(c.siteProperties.getText("WorkflowEditorGroup", "Content Editors"), FromAddress, "Authoring Submitted Notification", Copy, False, True)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '
        '
        Private Function pageManager_GetDefaultBlockMessage(UseContentWatchLink As Boolean) As String
            pageManager_GetDefaultBlockMessage = ""
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetDefaultBlockMessage")
            '
            Dim CS As Integer
            Dim PageID As Integer
            '
            CS = c.db.cs_open("Copy Content", "name=" & c.db.encodeSQLText(ContentBlockCopyName), "ID", , , , , "Copy,ID")
            If c.db.cs_ok(CS) Then
                pageManager_GetDefaultBlockMessage = c.db.cs_get(CS, "Copy")
            End If
            Call c.db.cs_Close(CS)
            '
            ' ----- Do not allow blank message - if still nothing, create default
            '
            If pageManager_GetDefaultBlockMessage = "" Then
                pageManager_GetDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
            End If
            '
            ' ----- Create Copy Content Record for future
            '
            CS = c.db.cs_insertRecord("Copy Content")
            If c.db.cs_ok(CS) Then
                Call c.db.cs_set(CS, "Name", ContentBlockCopyName)
                Call c.db.cs_set(CS, "Copy", pageManager_GetDefaultBlockMessage)
                Call c.db.cs_save2(CS)
                Call c.workflow.publishEdit("Copy Content", genericController.EncodeInteger(c.db.cs_get(CS, "ID")))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetDefaultBlockMessage")
        End Function
        '
        ' Instruction Format
        '   Line 1 - Version FormBuilderVersion
        '   Line 2+ instruction lines
        '   blank line
        '   FormHTML
        '
        ' Instruction Line Format
        '   Type,Caption,Required,arguments
        '
        '   Types
        '       1 = People Content field
        '           arguments = FieldName
        '       2 = Group main_MemberShip
        '           arguments = GroupName
        '
        ' FormHTML
        '   All HTML with the following:
        '   ##REPEAT></REPEAT> tags -- repeated for each instruction line
        '   {{CAPTION}} tags -- main_Gets the caption for each instruction line
        '   {{FIELD}} tags -- main_Gets the form field for each instruction line
        '
        Private Function pageManager_LoadFormPageInstructions(FormInstructions As String, Formhtml As String) As main_FormPagetype
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_LoadFormPageInstructions")
            '
            Dim RepeatBody As String
            Dim PtrFront As Integer
            Dim PtrBack As Integer
            Dim i() As String
            Dim IPtr As Integer
            Dim IStart As Integer
            Dim IArgs() As String
            Dim IArgPtr As Integer
            Dim CSPeople As Integer
            Dim Body As String
            Dim Instruction As String
            Dim CS As Integer
            Dim HasRequiredFields As Boolean
            Dim ArgCaption As String
            Dim ArgType As Integer
            Dim ArgRequired As Boolean
            Dim GroupName As String
            Dim GroupValue As Boolean
            Dim GroupRowPtr As Integer
            Dim FormPageID As Integer
            Dim f As main_FormPagetype
            '
            If True Then
                PtrFront = genericController.vbInstr(1, Formhtml, "{{REPEATSTART", vbTextCompare)
                If PtrFront > 0 Then
                    PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}")
                    If PtrBack > 0 Then
                        f.PreRepeat = Mid(Formhtml, 1, PtrFront - 1)
                        PtrFront = genericController.vbInstr(PtrBack, Formhtml, "{{REPEATEND", vbTextCompare)
                        If PtrFront > 0 Then
                            f.RepeatCell = Mid(Formhtml, PtrBack + 2, PtrFront - PtrBack - 2)
                            PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}")
                            If PtrBack > 0 Then
                                f.PostRepeat = Mid(Formhtml, PtrBack + 2)
                                '
                                ' Decode instructions and build output
                                '
                                i = genericController.SplitCRLF(FormInstructions)
                                If UBound(i) > 0 Then
                                    If Trim(i(0)) >= "1" Then
                                        '
                                        ' decode Version 1 arguments, then start instructions line at line 1
                                        '
                                        f.AddGroupNameList = genericController.encodeText(i(1))
                                        f.AuthenticateOnFormProcess = genericController.EncodeBoolean(i(2))
                                        IStart = 3
                                    End If
                                    '
                                    ' read in and compose the repeat lines
                                    '

                                    RepeatBody = ""
                                    CSPeople = -1
                                    ReDim f.Inst(UBound(i))
                                    For IPtr = 0 To UBound(i) - IStart
                                        With f.Inst(IPtr)
                                            IArgs = Split(i(IPtr + IStart), ",")
                                            If UBound(IArgs) >= main_IPosMax Then
                                                .Caption = IArgs(main_IPosCaption)
                                                .Type = genericController.EncodeInteger(IArgs(main_IPosType))
                                                .REquired = genericController.EncodeBoolean(IArgs(main_IPosRequired))
                                                Select Case .Type
                                                    Case 1
                                                        '
                                                        ' People Record
                                                        '
                                                        .PeopleField = IArgs(main_IPosPeopleField)
                                                    Case 2
                                                        '
                                                        ' Group main_MemberShip
                                                        '
                                                        .GroupName = IArgs(main_IPosGroupName)
                                                End Select
                                            End If
                                        End With
                                    Next
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            '
            pageManager_LoadFormPageInstructions = f
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError13("main_LoadFormPageInstructions")
        End Function
        '
        '
        '
        Private Function pageManager_GetFormPage(FormPageName As String, GroupIDToJoinOnSuccess As Integer) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetFormPage")
            '
            Dim RepeatBody As String
            Dim PtrFront As Integer
            Dim PtrBack As Integer
            Dim i() As String
            Dim IPtr As Integer
            Dim IStart As Integer
            Dim IArgs() As String
            Dim IArgPtr As Integer
            Dim CSPeople As Integer
            Dim Body As String
            Dim Instruction As String
            Dim Formhtml As String
            Dim FormInstructions As String
            Dim CS As Integer
            Dim HasRequiredFields As Boolean
            Dim ArgCaption As String
            Dim ArgType As Integer
            Dim ArgRequired As Boolean
            Dim GroupName As String
            Dim GroupValue As Boolean
            Dim GroupRowPtr As Integer
            Dim FormPageID As Integer
            Dim f As main_FormPagetype
            Dim IsRetry As Boolean
            Dim CaptionSpan As String
            Dim Caption As String
            Dim IsRequiredByCDef As Boolean
            Dim PeopleCDef As coreMetaDataClass.CDefClass
            '
            IsRetry = (c.docProperties.getInteger("ContensiveFormPageID") <> 0)
            '
            CS = c.db.cs_open("Form Pages", "name=" & c.db.encodeSQLText(FormPageName))
            If c.db.cs_ok(CS) Then
                FormPageID = c.db.cs_getInteger(CS, "ID")
                Formhtml = c.db.cs_getText(CS, "Body")
                FormInstructions = c.db.cs_getText(CS, "Instructions")
            End If
            Call c.db.cs_Close(CS)
            f = pageManager_LoadFormPageInstructions(FormInstructions, Formhtml)
            '
            '
            '
            RepeatBody = ""
            CSPeople = -1
            For IPtr = 0 To UBound(f.Inst)
                With f.Inst(IPtr)
                    Select Case .Type
                        Case 1
                            '
                            ' People Record
                            '
                            If IsRetry And c.docProperties.getText(.PeopleField) = "" Then
                                CaptionSpan = "<span class=""ccError"">"
                            Else
                                CaptionSpan = "<span>"
                            End If
                            If Not c.db.cs_ok(CSPeople) Then
                                CSPeople = c.csOpen("people", c.authContext.user.ID)
                            End If
                            Caption = .Caption
                            If .REquired Or genericController.EncodeBoolean(c.GetContentFieldProperty("People", .PeopleField, "Required")) Then
                                Caption = "*" & Caption
                            End If
                            If c.db.cs_ok(CSPeople) Then
                                Body = f.RepeatCell
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan & Caption & "</span>", 1, 99, vbTextCompare)
                                Body = genericController.vbReplace(Body, "{{FIELD}}", c.htmlDoc.html_GetFormInputCS(CSPeople, "People", .PeopleField), 1, 99, vbTextCompare)
                                RepeatBody = RepeatBody & Body
                                HasRequiredFields = HasRequiredFields Or .REquired
                            End If
                        Case 2
                            '
                            ' Group main_MemberShip
                            '
                            GroupValue = c.authContext.IsMemberOfGroup2(c, .GroupName)
                            Body = f.RepeatCell
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", c.htmlDoc.html_GetFormInputCheckBox2("Group" & .GroupName, GroupValue), 1, 99, vbTextCompare)
                            Body = genericController.vbReplace(Body, "{{FIELD}}", .Caption)
                            RepeatBody = RepeatBody & Body
                            GroupRowPtr = GroupRowPtr + 1
                            HasRequiredFields = HasRequiredFields Or .REquired
                    End Select
                End With
            Next
            Call c.db.cs_Close(CSPeople)
            If HasRequiredFields Then
                Body = f.RepeatCell
                Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, vbTextCompare)
                Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields")
                RepeatBody = RepeatBody & Body
            End If
            '
            pageManager_GetFormPage = "" _
            & c.error_GetUserError() _
            & c.htmlDoc.html_GetUploadFormStart() _
            & c.htmlDoc.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) _
            & c.htmlDoc.html_GetFormInputHidden("SuccessID", c.security.encodeToken(GroupIDToJoinOnSuccess, c.app_startTime)) _
            & f.PreRepeat _
            & RepeatBody _
            & f.PostRepeat _
            & c.htmlDoc.html_GetUploadFormEnd()
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetFormPage")
        End Function
        '
        '
        '
        Private Sub pageManager_ProcessFormPage(FormPageID As Integer)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_ProcessFormPage")
            '
            Dim CS As Integer
            Dim SQL As String
            Dim Formhtml As String
            Dim FormInstructions As String
            Dim f As main_FormPagetype
            Dim Ptr As Integer
            Dim CSPeople As Integer
            Dim IsInGroup As Boolean
            Dim WasInGroup As Boolean
            Dim FormValue As String
            Dim Success As Boolean
            Dim PeopleFirstName As String
            Dim PeopleLastName As String
            Dim PeopleUsername As String
            Dim PeoplePassword As String
            Dim PeopleName As String
            Dim PeopleEmail As String
            Dim Groups() As String
            Dim GroupName As String
            Dim GroupIDToJoinOnSuccess As Integer
            '
            ' main_Get the instructions from the record
            '
            CS = c.csOpen("Form Pages", FormPageID)
            If c.db.cs_ok(CS) Then
                Formhtml = c.db.cs_getText(CS, "Body")
                FormInstructions = c.db.cs_getText(CS, "Instructions")
            End If
            Call c.db.cs_Close(CS)
            If FormInstructions <> "" Then
                '
                ' Load the instructions
                '
                f = pageManager_LoadFormPageInstructions(FormInstructions, Formhtml)
                If f.AuthenticateOnFormProcess And Not c.authContext.isAuthenticated() And c.authContext.isRecognized(c) Then
                    '
                    ' If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
                    '
                    Call c.authContext.logout(c)
                End If
                CSPeople = -1
                Success = True
                For Ptr = 0 To UBound(f.Inst)
                    With f.Inst(Ptr)
                        Select Case .Type
                            Case 1
                                '
                                ' People Record
                                '
                                FormValue = c.docProperties.getText(.PeopleField)
                                If (FormValue <> "") And genericController.EncodeBoolean(c.GetContentFieldProperty("people", .PeopleField, "uniquename")) Then
                                    SQL = "select count(*) from ccMembers where " & .PeopleField & "=" & c.db.encodeSQLText(FormValue)
                                    CS = c.db.cs_openSql(SQL)
                                    If c.db.cs_ok(CS) Then
                                        Success = c.db.cs_getInteger(CS, "cnt") = 0
                                    End If
                                    Call c.db.cs_Close(CS)
                                    If Not Success Then
                                        c.error_AddUserError("The field [" & .Caption & "] must be unique, and the value [" & c.htmlDoc.html_EncodeHTML(FormValue) & "] has already been used.")
                                    End If
                                End If
                                If (.REquired Or genericController.EncodeBoolean(c.GetContentFieldProperty("people", .PeopleField, "required"))) And FormValue = "" Then
                                    Success = False
                                    c.error_AddUserError("The field [" & c.htmlDoc.html_EncodeHTML(.Caption) & "] is required.")
                                Else
                                    If Not c.db.cs_ok(CSPeople) Then
                                        CSPeople = c.csOpen("people", c.authContext.user.ID)
                                    End If
                                    If c.db.cs_ok(CSPeople) Then
                                        Select Case genericController.vbUCase(.PeopleField)
                                            Case "NAME"
                                                PeopleName = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case "FIRSTNAME"
                                                PeopleFirstName = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case "LASTNAME"
                                                PeopleLastName = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case "EMAIL"
                                                PeopleEmail = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case "USERNAME"
                                                PeopleUsername = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case "PASSWORD"
                                                PeoplePassword = FormValue
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            Case Else
                                                Call c.db.cs_set(CSPeople, .PeopleField, FormValue)
                                        End Select
                                    End If
                                End If
                            Case 2
                                '
                                ' Group main_MemberShip
                                '
                                IsInGroup = c.docProperties.getBoolean("Group" & .GroupName)
                                WasInGroup = c.authContext.IsMemberOfGroup2(c, .GroupName)
                                If WasInGroup And Not IsInGroup Then
                                    c.group_DeleteGroupMember(.GroupName)
                                ElseIf IsInGroup And Not WasInGroup Then
                                    c.group_AddGroupMember(.GroupName)
                                End If
                        End Select
                    End With
                Next
                '
                ' Create People Name
                '
                If PeopleName = "" And PeopleFirstName <> "" And PeopleLastName <> "" Then
                    If c.db.cs_ok(CSPeople) Then
                        Call c.db.cs_set(CSPeople, "name", PeopleFirstName & " " & PeopleLastName)
                    End If
                End If
                Call c.db.cs_Close(CSPeople)
                '
                ' AuthenticationOnFormProcess requires Username/Password and must be valid
                '
                If Success Then
                    '
                    ' Authenticate
                    '
                    If f.AuthenticateOnFormProcess Then
                        Call c.authContext.authenticateById(c, c.authContext.user.ID, c.authContext)
                    End If
                    '
                    ' Join Group requested by page that created form
                    '
                    Dim tokenDate As Date
                    Call c.security.decodeToken(c.docProperties.getText("SuccessID"), GroupIDToJoinOnSuccess, tokenDate)
                    'GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                    If GroupIDToJoinOnSuccess <> 0 Then
                        Call c.group_AddGroupMember(c.group_GetGroupName(GroupIDToJoinOnSuccess))
                    End If
                    '
                    ' Join Groups requested by pageform
                    '
                    If f.AddGroupNameList <> "" Then
                        Groups = Split(Trim(f.AddGroupNameList), ",")
                        For Ptr = 0 To UBound(Groups)
                            GroupName = Trim(Groups(Ptr))
                            If GroupName <> "" Then
                                Call c.group_AddGroupMember(GroupName)
                            End If
                        Next
                    End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError13("main_ProcessFormPage")
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Function pageManager_GetSectionMenuNamed(Optional ByVal DepthLimit As Integer = 3, Optional ByVal MenuStyle As Integer = 1, Optional ByVal StyleSheetPrefix As String = "", Optional ByVal MenuName As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetSectionMenuNamed")
            '
            'If Not (true) Then Exit Function
            '
            'Dim DepthLimit As Integer
            'Dim MenuStyle As Integer
            Dim StyleSheetPrefixLocal As String
            Dim RedirectLink As String
            Dim DefaultTemplateLink As String
            Dim MenuNameLocal As String
            Dim MenuID As Integer
            '
            'DepthLimit = encodeEmptyInteger(DepthLimit, 3)
            'MenuStyle = encodeEmptyInteger(MenuStyle, 1)
            StyleSheetPrefixLocal = genericController.encodeEmptyText(StyleSheetPrefix, "ccFlyout")
            MenuNameLocal = genericController.encodeEmptyText(MenuName, "Default")
            If MenuNameLocal = "" Then
                MenuNameLocal = "Default"
            End If
            MenuID = c.csv_VerifyDynamicMenu(MenuNameLocal)
            '
            DefaultTemplateLink = c.siteProperties.getText("SectionLandingLink", requestAppRootPath & c.siteProperties.serverPageDefault)
            pageManager_GetSectionMenuNamed = pageManager_GetSectionMenu(DepthLimit, MenuStyle, StyleSheetPrefixLocal, DefaultTemplateLink, MenuID, MenuNameLocal, c.siteProperties.useContentWatchLink)
            pageManager_GetSectionMenuNamed = c.htmlDoc.main_GetEditWrapper("Section Menu", pageManager_GetSectionMenuNamed)
            '
            If pageManager_RedirectLink <> "" Then
                Call c.webServer.webServerIO_Redirect2(pageManager_RedirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
            End If
            '
            Exit Function
            '
ErrorTrap:
            'Set PageList = Nothing
            Call c.handleLegacyError18("main_GetSectionMenuNamed")
        End Function
        '
        '=============================================================================
        ' 3.3 Compatibility
        '=============================================================================
        '
        Public Function main_GetSectionMenu(Optional ByVal DepthLimit As Integer = 3, Optional ByVal MenuStyle As Integer = 1, Optional ByVal StyleSheetPrefix As String = "") As String
            main_GetSectionMenu = pageManager_GetSectionMenuNamed(DepthLimit, MenuStyle, StyleSheetPrefix)
        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by the ContentName and RecordID
        '=============================================================================
        '
        Public Function main_GetContentWatchLinkByName(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = True) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentWatchLinkByName")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentRecordKey As String
            '
            ContentRecordKey = c.main_GetContentID(genericController.encodeText(ContentName)) & "." & genericController.EncodeInteger(RecordID)
            main_GetContentWatchLinkByName = main_GetContentWatchLinkByKey(ContentRecordKey, DefaultLink, IncrementClicks)
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError18("main_GetContentWatchLinkByName")
        End Function
        '
        Public Sub pageManager_GetPageArgs(ByVal PageID As Integer, ByVal isWorkflowRendering As Boolean, ByVal isQuickEditing As Boolean, ByRef return_CCID As Integer, ByRef return_TemplateID As Integer, ByRef return_ParentID As Integer, ByRef return_MenuLinkOverRide As String, ByRef return_IsRootPage As Boolean, ByRef return_SectionID As Integer, ByRef return_PageIsSecure As Boolean, ByVal UsedIDList As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageArgs")
            '
            Dim TCPtr As Integer
            Dim PCCPtr As Integer
            Dim PageFound As Boolean
            Dim IgnoreInteger As Integer
            Dim IgnoreString As String
            Dim IgnoreBoolean As Boolean
            Dim Ptr As Integer
            Dim SetTemplateID As Integer
            Dim pagetemplateID As Integer
            Dim SectionTemplateID As Integer

            '
            PCCPtr = pageManager_cache_pageContent_getPtr(PageID, isWorkflowRendering, isQuickEditing)
            If PCCPtr >= 0 Then
                PageFound = True
                return_CCID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
                pagetemplateID = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
                return_ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
                return_MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
                If UBound(cache_pageContent, 1) >= PCC_IsSecure Then
                    return_PageIsSecure = genericController.EncodeBoolean(cache_pageContent(PCC_IsSecure, PCCPtr))
                End If
                return_IsRootPage = (return_ParentID = 0)
            End If
            '
            If PageFound Then
                If return_ParentID = 0 Then
                    '
                    ' This is the root, main_Get the sectionID
                    '
                    If pageManager_cache_siteSection_rows = 0 Then
                        Call pageManager_cache_siteSection_getPtr(1)
                    End If
                    Ptr = pageManager_cache_siteSection_RootPageIDIndex.getPtr(CStr(PageID))
                    If Ptr >= 0 Then
                        return_SectionID = genericController.EncodeInteger(cache_siteSection(SSC_ID, Ptr))
                        SectionTemplateID = genericController.EncodeInteger(cache_siteSection(SSC_TemplateID, Ptr))
                    End If
                Else
                    '
                    ' chase further
                    '
                    If Not genericController.IsInDelimitedString(UsedIDList, CStr(return_ParentID), ",") Then
                        Call pageManager_GetPageArgs(return_ParentID, isWorkflowRendering, isQuickEditing, IgnoreInteger, return_TemplateID, IgnoreInteger, IgnoreString, IgnoreBoolean, return_SectionID, IgnoreBoolean, UsedIDList & "," & return_ParentID)
                    End If
                End If
                If pagetemplateID <> 0 Then
                    return_TemplateID = pagetemplateID
                ElseIf SectionTemplateID <> 0 Then
                    return_TemplateID = SectionTemplateID
                End If
                '
                If return_TemplateID = 0 Then
                    '
                    ' no templateid still (parent and section are all blank), main_Get default templateid
                    '
                    If pageManager_cache_pageTemplate_rows > 0 Then
                        For TCPtr = 0 To pageManager_cache_pageTemplate_rows - 1
                            If genericController.vbLCase(genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))) = "default" Then
                                Exit For
                            End If
                        Next
                        If TCPtr < pageManager_cache_pageTemplate_rows Then
                            return_TemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
                        End If
                    End If
                    'End If
                End If
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetPageArgs")
        End Sub
        '
        '====================================================================================================
        '   Return a page's Link
        '
        '   2011/6/5
        '       Always returns an absolute URL. The consumer needs to decide if the protocol and domain are
        '           needed or wanted.
        '       The results should only be used on the current domain and protocol, so if the result is included
        '           in a cache, the cache name should include the domain and protocol.
        '           - this is because the consumer may not need the abolute URL, but can not trim the domain and proto
        '               off because they don't know if it is there because it is the current one, or because
        '               it is the only domain allowed.
        '
        '   link parts:
        '       protocol
        '       domain
        '       linkPathPage
        '       linkQS
        '
        '   short templateLink (root relative to ?)
        '       pathpage = templatelink
        '       qs = calculate
        '       template domain set
        '           domain = template domain
        '       no template domain
        '           domain = current domain
        '       page secure or template secure
        '           protocol = https://
        '       else
        '           protocol = http://
        '
        '   long templateLink (protocol to ?)
        '       qs =calculate
        '
        '   empty templateLink
        '       linkalias OK
        '           pathpage = linkalias
        '           qs = empty
        '       No LinkAlias
        '           pathpage = defaultlink
        '           qs = calculate
        '           template domain set
        '               domain = template domain
        '           no template domain
        '               domain = current domain
        '           page secure or template secure
        '               protocol = https://
        '           else
        '               protocol = http://
        '
        '====================================================================================================
        '
        Public Function pageManager_GetPageLink4(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal AllowLinkAliasIfEnabled As Boolean, ByVal UseContentWatchNotDefaultPage As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageLink")
            '
            Dim main_domainIds() As String
            Dim Ptr As Integer
            Dim setdomainId As Integer
            Dim templatedomainIdList As String
            Dim linkLong As String
            Dim linkprotocol As String
            Dim linkPathPage As String
            Dim linkAlias As String
            Dim linkQS As String
            Dim linkDomain As String
            '
            Dim defaultPathPage As String
            Dim Key As String
            Dim TCPtr As Integer
            Dim ContentControlID As Integer
            Dim IsRootPage As Boolean
            Dim SectionID As Integer
            Dim MenuLinkOverRide As String
            '
            Dim ParentID As Integer
            Dim PageIsSecure As Boolean
            Dim Pos As Integer
            '
            Dim templateLink As String
            Dim templateLinkIncludesProtocol As Boolean
            Dim templateSecure As Boolean
            Dim templateId As Integer
            Dim templateDomain As String
            '
            Call pageManager_GetPageArgs(PageID, pagemanager_IsWorkflowRendering, c.authContext.isQuickEditing(c, ""), ContentControlID, templateId, ParentID, MenuLinkOverRide, IsRootPage, SectionID, PageIsSecure, "")
            '
            ' main_Get defaultpathpage
            '
            defaultPathPage = c.siteProperties.serverPageDefault
            If defaultPathPage <> "" Then
                Pos = genericController.vbInstr(1, defaultPathPage, "?")
                If Pos <> 0 Then
                    defaultPathPage = Mid(defaultPathPage, 1, Pos - 1)
                End If
                If Left(defaultPathPage, 1) <> "/" Then
                    defaultPathPage = "/" & defaultPathPage
                End If
            Else
                defaultPathPage = "/" & main_guessDefaultPage()
            End If
            '
            ' main_Get TemplateLink and secure setting
            '
            TCPtr = -1
            If pageManager_cache_pageTemplate_rows = 0 Then
                Call pageManager_cache_pageTemplate_load()
            End If
            If templateId <> 0 Then
                TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
            Else
                If pageManager_cache_pageTemplate_rows > 0 Then
                    For TCPtr = 0 To pageManager_cache_pageTemplate_rows - 1
                        If genericController.vbLCase(genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))) = "default" Then
                            Exit For
                        End If
                    Next
                    If TCPtr = pageManager_cache_pageTemplate_rows Then
                        TCPtr = -1
                    End If
                End If
                'End If
            End If
            If TCPtr >= 0 Then
                templateLink = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
                templateSecure = genericController.EncodeBoolean(cache_pageTemplate(TC_IsSecure, TCPtr))
                templateLinkIncludesProtocol = (InStr(1, templateLink, "://") <> 0)
                templatedomainIdList = genericController.encodeText(cache_pageTemplate(TC_DomainIdList, TCPtr))
            End If
            '
            ' calc linkQS (cleared in come cases later)
            '
            If IsRootPage And (SectionID <> 0) Then
                linkQS = "sid=" & SectionID
            Else
                linkQS = "bid=" & PageID
            End If
            If QueryStringSuffix <> "" Then
                linkQS = linkQS & "&" & QueryStringSuffix
            End If
            '
            ' calculate depends on the template provided
            '
            If templateLink = "" Then
                '
                ' ----- templateLink is blank
                '
                If AllowLinkAliasIfEnabled And c.siteProperties.allowLinkAlias Then
                    If c.cache_linkAliasCnt = 0 Then
                        Call c.cache_linkAlias_load()
                    End If
                    If c.cache_linkAliasCnt > 0 Then
                        Key = genericController.vbLCase(CStr(PageID) & QueryStringSuffix)
                        Ptr = c.cache_linkAlias_PageIdQSSIndex.getPtr(Key)
                        If Ptr >= 0 Then
                            linkAlias = genericController.encodeText(c.cache_linkAlias(1, Ptr))
                            If Mid(linkAlias, 1, 1) <> "/" Then
                                linkAlias = "/" & linkAlias
                            End If
                        End If
                    End If
                End If
                If (linkAlias = "") Then
                    linkPathPage = defaultPathPage
                Else
                    linkPathPage = linkAlias
                    linkQS = ""
                End If
                '
                ' domain (fake for now)
                '
                If (templatedomainIdList = "") Then
                    '
                    ' this template has no domain preference, use current domain
                    '
                    linkDomain = c.webServer.requestDomain
                ElseIf (c.domains.domainDetails.id = 0) Then
                    '
                    ' the current domain is not recognized, or is default - use it
                    '
                    linkDomain = c.webServer.requestDomain
                ElseIf (InStr(1, "," & templatedomainIdList & ",", "," & c.domains.domainDetails.id & ",") <> 0) Then
                    '
                    ' current domain is in the allowed domain list
                    '
                    linkDomain = c.webServer.requestDomain
                Else
                    '
                    ' there is an allowed domain list and current domain is not on it, or use first
                    '
                    main_domainIds = Split(templatedomainIdList, ",")
                    For Ptr = 0 To UBound(main_domainIds)
                        setdomainId = genericController.EncodeInteger(main_domainIds(Ptr))
                        If setdomainId <> 0 Then
                            Exit For
                        End If
                    Next
                    linkDomain = c.content_GetRecordName("domains", setdomainId)
                    If linkDomain = "" Then
                        linkDomain = c.webServer.requestDomain
                    End If
                End If
                '
                ' protocol
                '
                If PageIsSecure Or templateSecure Then
                    linkprotocol = "https://"
                Else
                    linkprotocol = "http://"
                End If
                linkLong = linkprotocol & linkDomain & linkPathPage
            ElseIf Not templateLinkIncludesProtocol Then
                '
                ' ----- Short TemplateLink
                '
                linkPathPage = templateLink
                '
                ' domain (fake for now)
                '
                If templateDomain <> "" Then
                    linkDomain = c.webServer.requestDomain
                Else
                    linkDomain = c.webServer.requestDomain
                End If
                '
                ' protocol
                '
                If PageIsSecure Or templateSecure Then
                    linkprotocol = "https://"
                Else
                    linkprotocol = "http://"
                End If
                linkLong = linkprotocol & linkDomain & linkPathPage
            Else
                '
                ' ----- Long TemplateLink
                '
                linkLong = templateLink
            End If
            '
            ' assemble
            '
            pageManager_GetPageLink4 = linkLong
            If linkQS <> "" Then
                pageManager_GetPageLink4 = pageManager_GetPageLink4 & "?" & linkQS
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetPageLink4")
        End Function
        '
        '====================================================================================================
        ' main_Get a page link if you know nothing about the page
        '   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        '====================================================================================================
        '
        Public Function main_GetPageLink3(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal AllowLinkAlias As Boolean) As String
            main_GetPageLink3 = pageManager_GetPageLink4(PageID, QueryStringSuffix, AllowLinkAlias, False)
        End Function
        '
        Public Function main_GetPageLink2(ByVal PageID As Integer, ByVal QueryStringSuffix As String) As String
            main_GetPageLink2 = pageManager_GetPageLink4(PageID, QueryStringSuffix, True, False)
            'main_GetPageLink2 = main_GetPageLink3(PageID, QueryStringSuffix, True)
        End Function
        '
        Public Function main_GetPageLink(ByVal PageID As Integer) As String
            main_GetPageLink = pageManager_GetPageLink4(PageID, "", True, False)
            'main_GetPageLink = main_GetPageLink3(PageID, "", True)
        End Function
        '
        '
        '
        Private Function pageManager_GetSectionLink(ByVal ShortLink As String, ByVal PageID As Integer, ByVal SectionID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00378")
            '
            Dim QSplit() As String
            Dim QSPlitCount As Integer
            Dim QSplitPointer As Integer
            Dim NVSplit() As String
            '
            pageManager_GetSectionLink = ShortLink
            If c.web_RefreshQueryString <> "" Then
                QSplit = Split(c.web_RefreshQueryString, "&")
                QSPlitCount = UBound(QSplit) + 1
                For QSplitPointer = 0 To QSPlitCount - 1
                    NVSplit = Split(QSplit(QSplitPointer), "=")
                    If UBound(NVSplit) > 0 Then
                        pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, NVSplit(0), NVSplit(1), True)
                    End If
                Next
            End If
            If PageID = 0 Then
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "bid", "", False)
                If SectionID = 0 Then
                    pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", "", False)
                Else
                    pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", CStr(SectionID), True)
                End If
            Else
                '
                ' If I have a pageID, block the sectionID
                '
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "bid", CStr(PageID), True)
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", "", False)
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("pageManager_GetSectionLink")
        End Function
        '
        '===================================================================================================
        '   Load Template from TemplateID
        '       Template is loaded by ID
        '       If it is not found, default template is loaded
        '       If default template is not found, it is created
        '       Loaded TemplateID is returned - so you know if it loaded correctly
        '
        '       If Link is provided with protocol, it is returned with protocol
        '       If link is just a page, it is converted to a short link
        '       If link is blank, blank is returned and a redirect is not required
        '===================================================================================================
        '
        Public Function pageManager_LoadTemplateGetID(ByVal templateId As Integer) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00379")
            '
            Dim CS As Integer
            Dim FieldList As String
            Const ContentName = "Page Templates"
            '
            If (templateId <> 0) And (main_RenderedTemplateID = templateId) Then
                '
                ' Use the previous values already loaded
                '
            Else
                main_RenderedTemplateID = templateId
                pageManager_TemplateBody = ""
                pageManager_TemplateLink = ""
                If True Then
                    FieldList = "ID,Link,BodyHTML"
                Else
                    FieldList = "ID,Link"
                End If
                CS = -1
                If templateId <> 0 Then
                    CS = c.csOpenRecord(ContentName, templateId, , , FieldList)
                End If
                If (templateId = 0) Or (Not c.db.cs_ok(CS)) Then
                    '
                    ' ----- if template not found, return default template
                    '       if this operation fails, exit now -- do not continue and create new template
                    '
                    main_RenderedTemplateID = 0
                    If c.domains.domainDetails.defaultTemplateId <> 0 Then
                        '
                        ' ----- attempt to use the domain's default template
                        '
                        Call c.db.cs_Close(CS)
                        CS = c.csOpenRecord(ContentName, c.domains.domainDetails.defaultTemplateId, , , FieldList)
                        If Not c.db.cs_ok(CS) Then
                            '
                            ' the defaultemplateid in the domain is not valid
                            '
                            Call c.db.executeSql("update ccdomains set defaulttemplateid=0 where defaulttemplateid=" & c.domains.domainDetails.defaultTemplateId)
                            Call c.cache.invalidateContent("domains")
                        End If
                    End If
                    If Not c.db.cs_ok(CS) Then
                        '
                        ' ----- attempt to use the site's default template
                        '
                        Call c.db.cs_Close(CS)
                        CS = c.db.cs_open(ContentName, "name=" & c.db.encodeSQLText(TemplateDefaultName), "ID", , , , , FieldList)
                    End If
                    If c.db.cs_ok(CS) Then
                        main_RenderedTemplateID = c.db.cs_getInteger(CS, "ID")
                        main_RenderedTemplateName = c.db.cs_getText(CS, "name")
                        pageManager_TemplateName = main_RenderedTemplateName
                        pageManager_TemplateLink = main_verifyTemplateLink(c.db.cs_get(CS, "Link"))
                        'pageManager_TemplateLink = app.csv_cs_get(CS, "Link")
                        If True Then
                            pageManager_TemplateBody = c.db.cs_get(CS, "BodyHTML")
                        End If
                    End If
                    Call c.db.cs_Close(CS)
                    '
                    ' ----- if default template not found, create a simple default template
                    '
                    If main_RenderedTemplateID = 0 Then
                        pageManager_TemplateName = TemplateDefaultName
                        pageManager_TemplateBody = TemplateDefaultBody
                        CS = c.db.cs_insertRecord("Page Templates")
                        If c.db.cs_ok(CS) Then
                            main_RenderedTemplateID = c.db.cs_getInteger(CS, "ID")
                            main_RenderedTemplateName = TemplateDefaultName
                            Call c.db.cs_set(CS, "name", TemplateDefaultName)
                            Call c.db.cs_set(CS, "Link", "")
                            If True Then
                                Call c.db.cs_set(CS, "BodyHTML", pageManager_TemplateBody)
                            End If
                            If True Then
                                Call c.db.cs_set(CS, "ccGuid", DefaultTemplateGuid)
                            End If
                            Call c.db.cs_Close(CS)
                        End If
                        Call pageManager_cache_pageTemplate_clear()
                    End If
                    pageManager_TemplateLink = ""
                Else
                    '
                    ' ----- load template
                    '
                    If True Then
                        pageManager_TemplateBody = c.db.cs_get(CS, "BodyHTML")
                    Else
                        pageManager_TemplateBody = "<!-- Template Body support requires a Contensive database upgrade through the Application Manager. -->" & TemplateDefaultBody
                    End If
                    pageManager_TemplateName = c.db.cs_get(CS, "name")
                    pageManager_TemplateLink = main_verifyTemplateLink(c.db.cs_get(CS, "Link"))
                End If
                Call c.db.cs_Close(CS)
                pageManager_TemplateLink = main_verifyTemplateLink(pageManager_TemplateLink)
            End If
            '
            pageManager_LoadTemplateGetID = main_RenderedTemplateID
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("pageManager_LoadTemplateGetID")
        End Function
        '
        '
        '
        Public Function pageManager_GetDynamicMenu(addonOption_String As String, UseContentWatchLink As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetDynamicMenu")
            '
            'If Not (true) Then Exit Function
            '
            Dim EditLink As String
            Dim StylesFilename As String
            Dim MenuDepth As Integer
            Dim MenuStyle As Integer
            Dim MenuName As String
            Dim MenuStylePrefix As String
            Dim MenuDelimiter As String
            Dim DefaultTemplateLink As String
            Dim FlyoutDirection As String
            Dim FlyoutOnHover As String
            Dim Layout As String
            Dim PreButton As String
            Dim PostButton As String
            Dim MenuID As Integer
            Dim IsAuthoring As Boolean
            Dim Menu As String
            Dim MenuNew As String
            Dim CS As Integer
            Dim IsOldMenu As Boolean
            Dim CompatibilitySpanAroundButton As Boolean
            '
            IsAuthoring = c.authContext.isEditing(c, "Dynamic Menus")
            DefaultTemplateLink = requestAppRootPath & c.webServer.webServerIO_requestPage
            If False Then '.292" Then
                CompatibilitySpanAroundButton = True
            Else
                CompatibilitySpanAroundButton = c.siteProperties.getBoolean("Compatibility Dynamic Menu Span Around Button", False)
            End If
            '
            ' Check for MenuID - if present, arguments are in the Dynamic Menu content - else it is old, and they are in the addonOption_String
            '
            If True And genericController.vbInstr(1, addonOption_String, "menu=", vbTextCompare) <> 0 Then
                MenuNew = c.main_GetAddonOption("menunew", addonOption_String)
                'MenuNew = Trim( genericController.DecodeResponseVariable(main_GetArgument("menunew", addonOption_String, "", "&")))
                If MenuNew <> "" Then
                    '
                    ' Create New Menu
                    '
                    Menu = MenuNew
                End If
                If Menu = "" Then
                    '
                    ' No new menu, try a selected menu
                    '
                    Menu = c.main_GetAddonOption("menu", addonOption_String)
                    'Menu = Trim( genericController.DecodeResponseVariable(main_GetArgument("menu", addonOption_String, "", "&")))
                    If Menu = "" Then
                        '
                        ' No selected, use Default
                        '
                        Menu = "Default"
                    End If
                End If
                MenuID = c.menu_VerifyDynamicMenu(Menu)
                '
                ' Open the Menu
                '
                CS = c.csOpen("Dynamic Menus", MenuID)
                If Not c.db.cs_ok(CS) Then
                    '
                    ' ID was given, but no found in Db
                    '
                    Call c.db.cs_Close(CS)
                    CS = c.csOpen("Dynamic Menus", c.menu_VerifyDynamicMenu("Default"))
                End If
                If c.db.cs_ok(CS) Then
                    '
                    ' setup arguments from Content
                    '
                    EditLink = c.cs_cs_getRecordEditLink(CS)
                    MenuName = c.db.cs_getText(CS, "Name")
                    MenuDepth = c.db.cs_getInteger(CS, "Depth")
                    MenuStylePrefix = c.db.cs_getText(CS, "StylePrefix")
                    MenuDelimiter = c.db.cs_getText(CS, "Delimiter")
                    FlyoutOnHover = c.db.cs_getBoolean(CS, "FlyoutOnHover").ToString
                    ' LookupList should return the text for the value saved - to be compatible with the old hardcoded text
                    FlyoutDirection = c.db.cs_get(CS, "FlyoutDirection")
                    Layout = c.db.cs_get(CS, "Layout")
                    MenuStyle = 0
                    '
                    ' Add exclusive styles
                    '
                    If True Then
                        StylesFilename = c.db.cs_getText(CS, "StylesFilename")
                        If StylesFilename <> "" Then
                            If genericController.vbLCase(Right(StylesFilename, 4)) <> ".css" Then
                                Call c.handleLegacyError15("Dynamic Menu [" & MenuName & "] StylesFilename is not a '.css' file, and will not display correct. Check that the field is setup as a CSSFile.", "main_GetDynamicMenu")
                            Else
                                Call c.htmlDoc.main_AddStylesheetLink2(c.webServer.webServerIO_requestProtocol & c.webServer.requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, StylesFilename), "dynamic menu")
                            End If
                        End If
                    End If
                End If
                Call c.db.cs_Close(CS)
            Else
                '
                ' Old style menu - main_Get arguments from AC tag
                '   MenuName="" is legacy mode (all sections show)
                '
                IsOldMenu = True
                MenuName = ""
                '
                MenuDepth = genericController.EncodeInteger(c.main_GetAddonOption("DEPTH", addonOption_String))
                MenuStylePrefix = Trim(c.main_GetAddonOption("STYLEPREFIX", addonOption_String))
                MenuDelimiter = c.main_GetAddonOption("DELIMITER", addonOption_String)
                FlyoutOnHover = c.main_GetAddonOption("FlyoutOnHover", addonOption_String)
                FlyoutDirection = c.main_GetAddonOption("FlyoutDirection", addonOption_String)
                Layout = c.main_GetAddonOption("Layout", addonOption_String)
                '
                ' really old value
                '
                MenuStyle = genericController.EncodeInteger(c.main_GetAddonOption("FORMAT", addonOption_String))
            End If
            '
            ' Check values
            '
            If MenuStylePrefix = "" Then
                MenuStylePrefix = "ccFlyout"
            End If
            '
            ' determine MenuStyle from input
            '
            If MenuStyle = 0 Then
                If genericController.EncodeBoolean(FlyoutOnHover) Then
                    MenuStyle = 8
                Else
                    MenuStyle = 4
                End If
                Select Case genericController.vbUCase(FlyoutDirection)
                    Case "RIGHT"
                        MenuStyle = MenuStyle + 1
                    Case "UP"
                        MenuStyle = MenuStyle + 2
                    Case "LEFT"
                        MenuStyle = MenuStyle + 3
                End Select
            End If
            pageManager_GetDynamicMenu = pageManager_GetSectionMenu(MenuDepth, MenuStyle, MenuStylePrefix, DefaultTemplateLink, MenuID, MenuName, UseContentWatchLink)
            '
            ' Now adjust results using arguments
            '
            If genericController.vbUCase(Layout) = "VERTICAL" Then
                '
                ' vertical menu: Set dislay block
                '
                pageManager_GetDynamicMenu = genericController.vbReplace(pageManager_GetDynamicMenu, "class=""" & MenuStylePrefix & "Button""", "style=""display:block;"" class=""" & MenuStylePrefix & "Button""")
                '
                PreButton = "<div style=""WHITE-SPACE: nowrap;"">"
                PostButton = "</div>"
                '
                If MenuDelimiter <> "" Then
                    MenuDelimiter = "<div style=""WHITE-SPACE: nowrap;"" class=""" & MenuStylePrefix & "Delimiter"">" & MenuDelimiter & "</div>"
                End If
            Else
                '
                ' horizontal menu: Set dislay inline
                '
                pageManager_GetDynamicMenu = genericController.vbReplace(pageManager_GetDynamicMenu, "class=""" & MenuStylePrefix & "Button""", "style=""display:inline;"" class=""" & MenuStylePrefix & "Button""")
                '
                If CompatibilitySpanAroundButton Then
                    PreButton = "<span style=""WHITE-SPACE: nowrap"">"
                    PostButton = "</span>"
                End If
                '
                If MenuDelimiter <> "" Then
                    MenuDelimiter = "<span style=""WHITE-SPACE: nowrap;"" class=""" & MenuStylePrefix & "Delimiter"">" & MenuDelimiter & "</span>"
                End If
            End If
            pageManager_GetDynamicMenu = PreButton & genericController.vbReplace(pageManager_GetDynamicMenu, vbCrLf, PostButton & MenuDelimiter & PreButton) & PostButton
            If c.authContext.isAdvancedEditing(c, "") Then
                pageManager_GetDynamicMenu = "<div style=""border-bottom:1px dashed #404040; padding:5px;margin-bottom:5px;"">Dynamic Menu [" & MenuName & "]" & EditLink & "</div><div>" & pageManager_GetDynamicMenu & "</div>"
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError13("main_GetDynamicMenu")
        End Function
        '
        '
        '
        Private Function pageManager_GetLandingLink() As String
            If pageManager_LandingLink = "" Then
                pageManager_LandingLink = c.siteProperties.getText("SectionLandingLink", requestAppRootPath & c.siteProperties.serverPageDefault)
                pageManager_LandingLink = genericController.ConvertLinkToShortLink(pageManager_LandingLink, c.webServer.requestDomain, c.webServer.webServerIO_requestVirtualFilePath)
                pageManager_LandingLink = genericController.EncodeAppRootPath(pageManager_LandingLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
            End If
            pageManager_GetLandingLink = pageManager_LandingLink
        End Function

        '
        '========================================================================
        ' ----- Ends an HTML page
        '========================================================================
        '
        Public Function pagemanager_GetPageEnd() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageEnd")
            '
            'If Not (true) Then Exit Function
            '
            pagemanager_GetPageEnd = "" _
                & cr & "</body>" _
                & vbCrLf & "</html>"
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError18("main_GetPageEnd")
        End Function
        '
        '
        '
        Public Function pageManager_GetStyleSheet() As String
            pageManager_GetStyleSheet = pageManager_GetStyleSheet2()
        End Function
        '
        '===========================================================================================================
        '   returns the Contensive styles inline
        '===========================================================================================================
        '
        Public Function pageManager_GetStyleSheetDefault() As String
            pageManager_GetStyleSheetDefault = c.htmlDoc.pageManager_GetStyleSheetDefault2()
        End Function
        '        '
        '        '
        '        '
        '        Public Function main_ProcessReplacement(ByVal NameValueLines As Object, ByVal Source As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00398")
        '            '
        '            main_ProcessReplacement = processReplacement(NameValueLines, Source)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Call c.handleLegacyError18("ProcessReplacement")
        '        End Function
        '
        '
        '
        Public Function pageManager_GetStyleTagPublic() As String
            Dim StyleSN As Integer
            '
            If c.siteProperties.getBoolean("Allow CSS Reset") Then
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & c.webServer.webServerIO_requestProtocol & c.webServer.webServerIO_requestDomain & "/ccLib/styles/ccreset.css"" >"
            End If
            StyleSN = genericController.EncodeInteger(c.siteProperties.getText("StylesheetSerialNumber", "0"))
            If StyleSN < 0 Then
                '
                ' Linked Styles
                ' Bump the Style Serial Number so next fetch is not cached
                '
                StyleSN = 1
                Call c.siteProperties.setProperty("StylesheetSerialNumber", CStr(StyleSN))
                '
                ' Save new public stylesheet
                '
                'Dim kmafs As New fileSystemClass
                Call c.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), c.csv_getStyleSheetProcessed)
                Call c.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css"), c.htmlDoc.pageManager_GetStyleSheetDefault2)

            End If
            If (StyleSN = 0) Then
                '
                ' Put styles inline if requested, and if there has been an upgrade
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & StyleSheetStart & pageManager_GetStyleSheet() & cr & StyleSheetEnd
            ElseIf (c.siteProperties.dataBuildVersion <> c.codeVersion()) Then
                '
                ' Put styles inline if requested, and if there has been an upgrade
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<!-- styles forced inline because database upgrade needed -->" & StyleSheetStart & pageManager_GetStyleSheet() & cr & StyleSheetEnd
            Else
                '
                ' cached stylesheet
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & c.webServer.webServerIO_requestProtocol & c.webServer.webServerIO_requestDomain & c.csv_getVirtualFileLink(c.serverConfig.appConfig.cdnFilesNetprefix, "templates/Public" & StyleSN & ".css") & """ >"
            End If
        End Function
        '
        '=======================================================================================================
        '   deprecated, use csv_getStyleSheet2
        '=======================================================================================================
        '
        Public Function pageManager_GetStyleSheet2() As String
            pageManager_GetStyleSheet2 = c.htmlDoc.html_getStyleSheet2(0, 0)
        End Function
        '
        '========================================================================
        ' main_IsWorkflowRendering()
        '   True if the current visitor is a content manager in workflow rendering mode
        '========================================================================
        '
        Public Function pagemanager_IsWorkflowRendering() As Boolean
            Dim returnIs As Boolean = False
            Try
                If c.authContext.isAuthenticatedContentManager(c) Then
                    pagemanager_IsWorkflowRendering = c.visitProperty.getBoolean("AllowWorkflowRendering")
                End If
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        '
        '
        '
        Public Sub pageManager_MarkRecordReviewed(ContentName As String, RecordID As Integer)
            '
            Dim SQL As String
            'Dim SQLNow As String
            Dim DataSourceName As String
            Dim TableName As String

            '
            If c.main_IsContentFieldSupported(ContentName, "DateReviewed") Then
                'SQLNow = encodeSQLDate(Now)
                DataSourceName = c.main_GetContentDataSource(ContentName)
                TableName = c.GetContentTablename(ContentName)
                '
                SQL = "update " & TableName & " set DateReviewed=" & c.db.encodeSQLDate(c.app_startTime)
                If c.main_IsContentFieldSupported(ContentName, "ReviewedBy") Then
                    SQL &= ",ReviewedBy=" & c.authContext.user.ID
                End If
                '
                ' Mark the live record
                '
                Call c.db.executeSql(SQL, DataSourceName & " where id=" & RecordID)
                '
                ' Mark the edit record if in workflow
                '
                If c.main_IsContentFieldSupported(ContentName, "editsourceid") Then
                    Call c.db.executeSql(SQL, DataSourceName & " where (editsourceid=" & RecordID & ")and(editarchive=0)")
                End If
            End If
        End Sub
        Public Function main_GetPageNotFoundPageId() As Integer
            Dim pageId As Integer
            Try
                pageId = c.domains.domainDetails.pageNotFoundPageId
                If pageId = 0 Then
                    '
                    ' no domain page not found, use site default
                    '
                    pageId = c.siteProperties.getinteger("PageNotFoundPageID", 0)
                End If
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return pageId
        End Function
        '
        '
        '
        Private Function main_guessDefaultPage() As String
            Return "default.aspx"
        End Function
        '
        '========================================================================
        '   main_IsChildRecord
        '
        '   Tests if this record is in the ParentID->ID chain for this content
        '========================================================================
        '
        Public Function main_IsChildRecord(ByVal ContentName As String, ByVal ChildRecordID As Integer, ByVal ParentRecordID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsChildRecord")
            '
            Dim CDef As coreMetaDataClass.CDefClass
            '
            main_IsChildRecord = (ChildRecordID = ParentRecordID)
            If Not main_IsChildRecord Then
                CDef = c.metaData.getCdef(ContentName)
                If genericController.IsInDelimitedString(UCase(CDef.SelectCommaList), "PARENTID", ",") Then
                    main_IsChildRecord = main_IsChildRecord_Recurse(CDef.ContentDataSourceName, CDef.ContentTableName, ChildRecordID, ParentRecordID, "")
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call c.handleLegacyError18("cpCoreClass.IsChildRecord")
            '
        End Function
        '
        '========================================================================
        '   main_IsChildRecord
        '
        '   Tests if this record is in the ParentID->ID chain for this content
        '========================================================================
        '
        Private Function main_IsChildRecord_Recurse(ByVal DataSourceName As String, ByVal TableName As String, ByVal ChildRecordID As Integer, ByVal ParentRecordID As Integer, ByVal History As String) As Boolean
            '
            Dim SQL As String
            Dim CS As Integer
            Dim ChildRecordParentID As Integer
            '
            SQL = "select ParentID from " & TableName & " where id=" & ChildRecordID
            CS = c.db.cs_openSql(SQL)
            If c.db.cs_ok(CS) Then
                ChildRecordParentID = c.db.cs_getInteger(CS, "ParentID")
            End If
            Call c.db.cs_Close(CS)
            If (ChildRecordParentID <> 0) And (Not genericController.IsInDelimitedString(History, CStr(ChildRecordID), ",")) Then
                main_IsChildRecord_Recurse = (ParentRecordID = ChildRecordParentID)
                If Not main_IsChildRecord_Recurse Then
                    main_IsChildRecord_Recurse = main_IsChildRecord_Recurse(DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History & "," & CStr(ChildRecordID))
                End If
            End If
            '
        End Function
        '
        '
        '
        Private Function main_ProcessPageNotFound_GetLink(ByVal adminMessage As String, Optional ByVal BackupPageNotFoundLink As String = "", Optional ByVal PageNotFoundLink As String = "", Optional ByVal EditPageID As Integer = 0, Optional ByVal EditSectionID As Integer = 0) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessPageNotFound_GetLink")
            '
            Dim Pos As Integer
            Dim DefaultLink As String
            Dim PageNotFoundPageID As Integer
            Dim PCCPtr As Integer
            Dim Link As String
            '
            PageNotFoundPageID = main_GetPageNotFoundPageId()
            If PageNotFoundPageID = 0 Then
                '
                ' No PageNotFound was set -- use the backup link
                '
                If BackupPageNotFoundLink = "" Then
                    adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used."
                    Link = pageManager_GetLandingLink()
                Else
                    adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set."
                    Link = BackupPageNotFoundLink
                End If
            Else
                '
                ' main_Get the PageID for the PageNotFound
                '
                PCCPtr = pageManager_cache_pageContent_getPtr(PageNotFoundPageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                If PCCPtr < 0 Then
                    '
                    ' Page Not Found was not found -- go with landing link
                    '
                    adminMessage = adminMessage & "</p><p>The current 'Page Not Found' could not be used because it could not be found. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used."
                    Link = pageManager_GetLandingLink()
                Else
                    '
                    ' Set link
                    '
                    Link = pageManager_GetPageLink4(PageNotFoundPageID, "", True, False)
                    DefaultLink = pageManager_GetPageLink4(0, "", True, False)
                    If Link <> DefaultLink Then
                    Else
                        adminMessage = adminMessage & "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used."
                    End If
                End If
            End If
            '
            ' Add the Admin Message to the link
            '
            If c.authContext.isAuthenticatedAdmin(c) Then
                If PageNotFoundLink = "" Then
                    PageNotFoundLink = c.webServer.webServerIO_ServerLink
                End If
                '
                ' Add the Link to the Admin Msg
                '
                adminMessage = adminMessage & "<p>The URL was " & PageNotFoundLink & "."
                '
                ' Add the Referrer to the Admin Msg
                '
                If c.webServer.webServerIO_requestReferer <> "" Then
                    Pos = genericController.vbInstr(1, c.webServer.requestReferrer, "main_AdminWarningPageID=", vbTextCompare)
                    If Pos <> 0 Then
                        c.webServer.requestReferrer = Left(c.webServer.requestReferrer, Pos - 2)
                    End If
                    Pos = genericController.vbInstr(1, c.webServer.requestReferrer, "main_AdminWarningMsg=", vbTextCompare)
                    If Pos <> 0 Then
                        c.webServer.requestReferrer = Left(c.webServer.requestReferrer, Pos - 2)
                    End If
                    Pos = genericController.vbInstr(1, c.webServer.requestReferrer, "blockcontenttracking=", vbTextCompare)
                    If Pos <> 0 Then
                        c.webServer.requestReferrer = Left(c.webServer.requestReferrer, Pos - 2)
                    End If
                    adminMessage = adminMessage & " The referring page was " & c.webServer.requestReferrer & "."
                End If
                '
                adminMessage = adminMessage & "</p>"
                '
                If EditPageID <> 0 Then
                    Link = genericController.modifyLinkQuery(Link, "main_AdminWarningPageID", CStr(EditPageID), True)
                End If
                '
                If EditSectionID <> 0 Then
                    Link = genericController.modifyLinkQuery(Link, "main_AdminWarningSectionID", CStr(EditSectionID), True)
                End If
                '
                Link = genericController.modifyLinkQuery(Link, RequestNameBlockContentTracking, "1", True)
                Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "<p>" & adminMessage & "</p>", True)
            End If
            '
            main_ProcessPageNotFound_GetLink = Link
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError18("main_ProcessPageNotFound_GetLink")
        End Function
        '
        '---------------------------------------------------------------------------
        '
        '---------------------------------------------------------------------------
        '
        Public Function main_GetLandingPageID() As Integer
            Dim landingPageid As Integer = 0
            Try
                Dim CS As Integer
                '
                If Not pageManager_LandingPageID_Loaded Then
                    pageManager_LandingPageID_Loaded = True
                    '
                    ' try the domain landing page first
                    '
                    CS = c.db.cs_open("Domains", "(name=" & c.db.encodeSQLText(c.webServer.requestDomain) & ")", , , , , , "RootPageID")
                    If c.db.cs_ok(CS) Then
                        pageManager_LandingPageID = genericController.EncodeInteger(c.db.cs_getText(CS, "RootPageID"))
                    End If
                    Call c.db.cs_Close(CS)
                    If pageManager_LandingPageID = 0 Then
                        '
                        ' try the site property landing page id
                        '
                        pageManager_LandingPageID = c.siteProperties.getinteger("LandingPageID", 0)
                    End If
                    If pageManager_LandingPageID = 0 Then
                        '
                        ' landing page could not be determined
                        '
                        c.htmlDoc.main_AdminWarning = c.htmlDoc.main_AdminWarning _
                            & "<p>This page is being displayed because the Landing Page was requested, but has not been configured." _
                            & " To configure any page as your landing page, edit the page, select the 'Control' tab, and check the checkbox marked 'Set Landing Page'.</p>" _
                            & "<p>The Landing Page is the page that is displayed when the domain name is requested without a specific page.</p>"
                        '
                        pageManager_LandingPageID = main_GetPageNotFoundPageId()
                    End If
                End If
                landingPageid = pageManager_LandingPageID
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return landingPageid
        End Function
        '
        '---------------------------------------------------------------------------
        '
        '---------------------------------------------------------------------------
        '
        Public Function main_GetLandingPageName(LandingPageID As Integer) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLandingPageName")
            '
            Dim PCCPtr As Integer
            '
            If pageManager_LandingPageName = "" Then
                PCCPtr = pageManager_cache_pageContent_getPtr(LandingPageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
                If PCCPtr < 0 Then
                    '
                    ' This case should have been covered in main_GetLandingPageID -- and should not be possible
                    '
                    pageManager_LandingPageName = DefaultNewLandingPageName
                Else
                    pageManager_LandingPageName = cache_pageContent(PCC_Name, PCCPtr)
                End If
            End If
            main_GetLandingPageName = pageManager_LandingPageName
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError18("main_GetLandingPageName")
        End Function
        '
        ' Verify a link from the template link field to be used as a Template Link
        '
        Private Function main_verifyTemplateLink(ByVal linkSrc As String) As String
            On Error GoTo ErrorTrap
            '
            '
            ' ----- Check Link Format
            '
            main_verifyTemplateLink = linkSrc
            If main_verifyTemplateLink <> "" Then
                If genericController.vbInstr(1, main_verifyTemplateLink, "://") <> 0 Then
                    '
                    ' protocol provided, do not fixup
                    '
                    main_verifyTemplateLink = genericController.EncodeAppRootPath(main_verifyTemplateLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                Else
                    '
                    ' no protocol, convert to short link
                    '
                    If Left(main_verifyTemplateLink, 1) <> "/" Then
                        '
                        ' page entered without path, assume it is in root path
                        '
                        main_verifyTemplateLink = "/" & main_verifyTemplateLink
                    End If
                    main_verifyTemplateLink = genericController.ConvertLinkToShortLink(main_verifyTemplateLink, c.webServer.requestDomain, c.webServer.webServerIO_requestVirtualFilePath)
                    main_verifyTemplateLink = genericController.EncodeAppRootPath(main_verifyTemplateLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call c.handleLegacyError11("main_verifyTemplateLink", "trap")
        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by its ContentRecordKey
        '=============================================================================
        '
        Public Function main_GetContentWatchLinkByKey(ByVal ContentRecordKey As String, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentWatchLinkByKey")
            '
            'If Not (true) Then Exit Function
            '
            Dim CSPointer As Integer
            '
            ' Lookup link in main_ContentWatch
            '
            CSPointer = c.db.cs_open("Content Watch", "ContentRecordKey=" & c.db.encodeSQLText(ContentRecordKey), , , , , , "Link,Clicks")
            If c.db.cs_ok(CSPointer) Then
                main_GetContentWatchLinkByKey = c.db.cs_getText(CSPointer, "Link")
                If genericController.EncodeBoolean(IncrementClicks) Then
                    Call c.db.cs_set(CSPointer, "Clicks", c.db.cs_getInteger(CSPointer, "clicks") + 1)
                End If
            Else
                main_GetContentWatchLinkByKey = genericController.encodeText(DefaultLink)
            End If
            Call c.db.cs_Close(CSPointer)
            '
            main_GetContentWatchLinkByKey = genericController.EncodeAppRootPath(main_GetContentWatchLinkByKey, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError18("main_GetContentWatchLinkByKey")
        End Function
        '
        '====================================================================================================
        ' Replace with main_GetPageArgs()
        '
        ' Used Interally by main_GetPageLink to main_Get the SectionID of the parents
        ' Dim siteSectionRootPageIndex As Dictionary(Of Integer, Integer) = siteSectionModel.getRootPageIdIndex(Me)
        '====================================================================================================
        '
        Private Function getPageSectionId(ByVal PageID As Integer, ByRef UsedIDList As List(Of Integer), siteSectionRootPageIndex As Dictionary(Of Integer, Integer)) As Integer
            Dim sectionId As Integer = 0
            Try
                Dim page As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(c, PageID, New List(Of String))
                If (page IsNot Nothing) Then
                    If (page.ParentID = 0) And (Not UsedIDList.Contains(page.ParentID)) Then
                        UsedIDList.Add(page.ParentID)
                        If siteSectionRootPageIndex.ContainsKey(page.ParentID) Then
                            sectionId = siteSectionRootPageIndex(page.ParentID)
                        End If
                    Else
                        sectionId = getPageSectionId(page.ParentID, UsedIDList, siteSectionRootPageIndex)
                    End If
                End If
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return sectionId
        End Function
        '
        '====================================================================================================
        ' Replace with main_GetPageArgs()
        '
        ' Used Interally by main_GetPageLink to main_Get the TemplateID of the parents
        '====================================================================================================
        '
        Private Function main_GetPageDynamicLink_GetTemplateID(ByVal PageID As Integer, ByVal UsedIDList As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageDynamicLink_GetTemplateID")
            '
            Dim CS As Integer
            Dim ParentID As Integer
            Dim templateId As Integer
            '
            '
            CS = c.csOpen("Page Content", PageID, , , "TemplateID,ParentID")
            If c.db.cs_ok(CS) Then
                templateId = c.db.cs_getInteger(CS, "TemplateID")
                ParentID = c.db.cs_getInteger(CS, "ParentID")
            End If
            Call c.db.cs_Close(CS)
            '
            ' Chase page tree to main_Get templateid
            '
            If templateId = 0 And ParentID <> 0 Then
                If Not genericController.IsInDelimitedString(UsedIDList, CStr(ParentID), ",") Then
                    main_GetPageDynamicLink_GetTemplateID = main_GetPageDynamicLink_GetTemplateID(ParentID, UsedIDList & "," & ParentID)
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetPageDynamicLink_GetTemplateID")
        End Function
        '
        '====================================================================================================
        ' main_Get a page link if you know nothing about the page
        '   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        '====================================================================================================
        '
        Public Function main_GetPageDynamicLink(ByVal PageID As Integer, ByVal UseContentWatchLink As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageDynamicLink")
            '
            Dim CS As Integer
            Dim CCID As Integer
            Dim DefaultLink As String
            Dim SectionID As Integer
            Dim IsRootPage As Boolean
            Dim templateId As Integer
            Dim MenuLinkOverRide As String
            Dim ParentID As Integer
            Dim PageFound As Boolean
            Dim PCCPtr As Integer
            Dim PageIsSecure As Boolean
            '
            Call pageManager_GetPageArgs(PageID, pagemanager_IsWorkflowRendering, c.authContext.isQuickEditing(c, ""), CCID, templateId, ParentID, MenuLinkOverRide, IsRootPage, SectionID, PageIsSecure, "")
            '    PCCPtr = pageManager_cache_pageContent_getPtr(PageID, main_IsWorkflowRendering, main_IsQuickEditing(""))
            '    If PCCPtr >= 0 Then
            '        PageFound = True
            '        CCID = genericController.EncodeInteger(main_pcc(PCC_ContentControlID, PCCPtr))
            '        TemplateID = genericController.EncodeInteger(main_pcc(PCC_TemplateID, PCCPtr))
            '        ParentID = genericController.EncodeInteger(main_pcc(PCC_ParentID, PCCPtr))
            '        MenuLinkOverRide = genericController.encodeText(main_pcc(PCC_Link, PCCPtr))
            '        IsRootPage = (ParentID = 0)
            '    End If
            '    If TemplateID = 0 And ParentID <> 0 Then
            '        '
            '        ' Chase page tree to main_Get templateid
            '        '
            '        TemplateID = main_GetPageDynamicLink_GetTemplateID(ParentID, "")
            '    End If
            '    '
            '    ' Chase page tree after PageName, then main_Get sectionID ***** really need a page cache
            '    '
            '    SectionID = getPageSectionId(PageID, "")
            '
            ' Convert default page to default link
            '
            DefaultLink = c.siteProperties.serverPageDefault
            If Mid(DefaultLink, 1, 1) <> "/" Then
                DefaultLink = "/" & c.siteProperties.serverPageDefault
            End If
            '
            main_GetPageDynamicLink = main_GetPageDynamicLinkWithArgs(CCID, PageID, DefaultLink, IsRootPage, templateId, SectionID, MenuLinkOverRide, UseContentWatchLink)
            '
            Exit Function
            '
ErrorTrap:
            Call c.handleLegacyError13("main_GetPageDynamicLink")
        End Function
        '====================================================================================================
        ''' <summary>
        ''' main_GetPageDynamicLinkWithArgs
        ''' </summary>
        ''' <param name="ContentControlID"></param>
        ''' <param name="PageID"></param>
        ''' <param name="DefaultLink"></param>
        ''' <param name="IsRootPage"></param>
        ''' <param name="templateId"></param>
        ''' <param name="SectionID"></param>
        ''' <param name="MenuLinkOverRide"></param>
        ''' <param name="UseContentWatchLink"></param>
        ''' <returns></returns>
        Private Function main_GetPageDynamicLinkWithArgs(ByVal ContentControlID As Integer, ByVal PageID As Integer, ByVal DefaultLink As String, ByVal IsRootPage As Boolean, ByVal templateId As Integer, ByVal SectionID As Integer, ByVal MenuLinkOverRide As String, ByVal UseContentWatchLink As Boolean) As String
            Dim resultLink As String = ""
            Try
                If MenuLinkOverRide <> "" Then
                    '
                    ' -- redirect to this page record
                    resultLink = "?rc=" & ContentControlID & "&ri=" & PageID
                Else
                    If UseContentWatchLink Then
                        '
                        ' -- Legacy method - lookup link from a table set during the last page hit
                        resultLink = main_GetContentWatchLinkByID(ContentControlID, PageID, DefaultLink, False)
                    Else
                        '
                        ' -- Current method - all pages are in the Template, Section, Page structure
                        If templateId <> 0 Then
                            Dim template As pageTemplateModel = pageTemplateModel.create(c, templateId, New List(Of String))
                            If (template IsNot Nothing) Then
                                resultLink = template.Link
                            End If
                        End If
                        If String.IsNullOrEmpty(resultLink) Then
                            '
                            ' -- not found, use default
                            If Not String.IsNullOrEmpty(DefaultLink) Then
                                '
                                ' if default given, use that
                                resultLink = DefaultLink
                            Else
                                '
                                ' -- fallback, use content watch
                                resultLink = main_GetContentWatchLinkByID(ContentControlID, PageID, , False)
                            End If
                        End If
                        If (PageID = 0) Or (IsRootPage) Then
                            '
                            ' -- Link to Root Page, no bid, and include sectionid if not 0
                            If IsRootPage And (SectionID <> 0) Then
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", CStr(SectionID), True)
                            End If
                            resultLink = genericController.modifyLinkQuery(resultLink, "bid", "", False)
                        Else
                            resultLink = genericController.modifyLinkQuery(resultLink, "bid", genericController.encodeText(PageID), True)
                            If PageID <> 0 Then
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", "", False)
                            End If
                        End If
                    End If
                End If
                resultLink = genericController.EncodeAppRootPath(resultLink, c.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, c.webServer.requestDomain)
            Catch ex As Exception
                c.handleExceptionAndRethrow(ex)
            End Try
            Return resultLink
        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by the ContentID and RecordID
        '=============================================================================
        '
        Public Function main_GetContentWatchLinkByID(ByVal ContentID As Integer, ByVal RecordID As Integer, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = True) As String
            main_GetContentWatchLinkByID = main_GetContentWatchLinkByKey(genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), DefaultLink, IncrementClicks)
        End Function
    End Class
End Namespace
