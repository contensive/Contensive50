
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons
    '
    Public Class pageManagerClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' pageManager addon interface
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                returnHtml = getHtmlDoc(cpCore)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        '   Returns the entire HTML page based on the bid/sid stream values
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Function getHtmlDoc(cpCore As coreClass) As String
            Dim returnHtml As String = ""
            Try
                Dim downloadId As Integer
                Dim Pos As Integer
                Dim htmlBody As String
                Dim htmlHead As String
                Dim bodyTag As String
                Dim bodyAddonId As Integer
                Dim bodyAddonStatusOK As Boolean
                Dim Clip As String
                Dim ClipParentRecordID As Integer
                Dim ClipParentContentID As Integer
                Dim ClipParentContentName As String
                Dim ClipChildContentID As Integer
                Dim ClipChildContentName As String
                Dim ClipChildRecordID As Integer
                Dim ClipChildRecordName As String
                Dim CSClip As Integer
                Dim ClipBoardArray() As String
                Dim ClipBoard As String
                Dim ClipParentFieldList As String
                Dim Fields As String()
                Dim FieldCount As Integer
                Dim NameValues() As String
                Dim RedirectLink As String = ""
                Dim RedirectReason As String = ""
                Dim PageNotFoundReason As String = ""
                Dim PageNotFoundSource As String = ""
                Dim IsPageNotFound As Boolean = False
                '
                If cpCore.docOpen Then
                    cpCore.htmlDoc.main_AdminWarning = cpCore.docProperties.getText("main_AdminWarningMsg")
                    cpCore.htmlDoc.main_AdminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID")
                    cpCore.htmlDoc.main_AdminWarningSectionID = cpCore.docProperties.getInteger("main_AdminWarningSectionID")
                    '
                    ' todo move cookie test to htmlDoc controller
                    ' -- Add cookie test
                    Dim AllowCookieTest As Boolean
                    AllowCookieTest = cpCore.siteProperties.allowVisitTracking And (cpCore.authContext.visit.PageVisits = 1)
                    If AllowCookieTest Then
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" & cpCore.security.encodeToken(cpCore.authContext.visit.ID, cpCore.app_startTime) & "')};", "Cookie Test")
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   User form processing
                    '       if a form is created in the editor, process it by emailing and saving to the User Form Response content
                    '--------------------------------------------------------------------------
                    '
                    If cpCore.docProperties.getInteger("ContensiveUserForm") = 1 Then
                        Dim FromAddress As String = cpCore.siteProperties.getText("EmailFromAddress", "info@" & cpCore.webServer.webServerIO_requestDomain)
                        Call cpCore.email.sendForm(cpCore.siteProperties.emailAdmin, FromAddress, "Form Submitted on " & cpCore.webServer.webServerIO_requestReferer)
                        Dim cs As Integer = cpCore.db.cs_insertRecord("User Form Response")
                        If cpCore.db.cs_ok(cs) Then
                            Call cpCore.db.cs_set(cs, "name", "Form " & cpCore.webServer.requestReferrer)
                            Dim Copy As String = ""

                            For Each key As String In cpCore.docProperties.getKeyList()
                                Dim docProperty As docPropertiesClass = cpCore.docProperties.getProperty(key)
                                If (key.ToLower() <> "contensiveuserform") Then
                                    Copy &= docProperty.Name & "=" & docProperty.Value & vbCrLf
                                End If
                            Next
                            Call cpCore.db.cs_set(cs, "copy", Copy)
                            Call cpCore.db.cs_set(cs, "VisitId", cpCore.authContext.visit.ID)
                        End If
                        Call cpCore.db.cs_Close(cs)
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Contensive Form Page Processing
                    '--------------------------------------------------------------------------
                    '
                    If cpCore.docProperties.getInteger("ContensiveFormPageID") <> 0 Then
                        Call pageManager_ProcessFormPage(cpCore, cpCore.docProperties.getInteger("ContensiveFormPageID"))
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Automatic Redirect to a full URL
                    '   If the link field of the record is an absolution address
                    '       rc = redirect contentID
                    '       ri = redirect content recordid
                    '--------------------------------------------------------------------------
                    '
                    cpCore.htmlDoc.pageManager_RedirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId))
                    If (cpCore.htmlDoc.pageManager_RedirectContentID <> 0) Then
                        cpCore.htmlDoc.pageManager_RedirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId))
                        If (cpCore.htmlDoc.pageManager_RedirectRecordID <> 0) Then
                            Dim contentName As String = cpCore.metaData.getContentNameByID(cpCore.htmlDoc.pageManager_RedirectContentID)
                            If contentName <> "" Then
                                If cpCore.main_RedirectByRecord_ReturnStatus(contentName, cpCore.htmlDoc.pageManager_RedirectRecordID) Then
                                    '
                                    'Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    '
                                    cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                                Else
                                    cpCore.htmlDoc.main_AdminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>"
                                    cpCore.htmlDoc.main_AdminWarningPageID = cpCore.htmlDoc.pageManager_RedirectRecordID
                                End If
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Active Download hook
                    Dim libraryFilePtr As Integer
                    Dim libraryFileClicks As Integer
                    Dim link As String = ""
                    Dim RecordEID As String = cpCore.docProperties.getText(RequestNameLibraryFileID)
                    If (RecordEID <> "") Then
                        Dim tokenDate As Date
                        Call cpCore.security.decodeToken(RecordEID, downloadId, tokenDate)
                        If downloadId <> 0 Then
                            '
                            ' ----- lookup record and set clicks
                            '
                            Call cpCore.cache_libraryFiles_loadIfNeeded()
                            libraryFilePtr = cpCore.cache_libraryFilesIdIndex.getPtr(CStr(downloadId))
                            If libraryFilePtr >= 0 Then
                                libraryFileClicks = genericController.EncodeInteger(cpCore.cache_libraryFiles(LibraryFilesCache_clicks, libraryFilePtr))
                                link = genericController.encodeText(cpCore.cache_libraryFiles(LibraryFilesCache_filename, libraryFilePtr))
                                Call cpCore.db.executeSql("update cclibraryfiles set clicks=" & (libraryFileClicks + 1) & " where id=" & downloadId)
                            End If
                            If link <> "" Then
                                '
                                ' ----- create log entry
                                '
                                Dim CSPointer As Integer = cpCore.db.cs_insertRecord("Library File Log")
                                If cpCore.db.cs_ok(CSPointer) Then
                                    Call cpCore.db.cs_set(CSPointer, "FileID", downloadId)
                                    Call cpCore.db.cs_set(CSPointer, "VisitId", cpCore.authContext.visit.ID)
                                    Call cpCore.db.cs_set(CSPointer, "MemberID", cpCore.authContext.user.ID)
                                End If
                                Call cpCore.db.cs_Close(CSPointer)
                                '
                                ' ----- and go
                                '
                                Call cpCore.webServer.webServerIO_Redirect2(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, link), "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.", False)
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Process clipboard cut/paste
                    '--------------------------------------------------------------------------
                    '
                    Clip = cpCore.docProperties.getText(RequestNameCut)
                    If (Clip <> "") Then
                        '
                        ' if a cut, load the clipboard
                        '
                        Call cpCore.visitProperty.setProperty("Clipboard", Clip)
                        Call genericController.modifyLinkQuery(cpCore.web_RefreshQueryString, RequestNameCut, "")
                    End If
                    ClipParentContentID = cpCore.docProperties.getInteger(RequestNamePasteParentContentID)
                    ClipParentRecordID = cpCore.docProperties.getInteger(RequestNamePasteParentRecordID)
                    ClipParentFieldList = cpCore.docProperties.getText(RequestNamePasteFieldList)
                    If (ClipParentContentID <> 0) And (ClipParentRecordID <> 0) Then
                        '
                        ' Request for a paste, clear the cliboard
                        '
                        ClipBoard = cpCore.visitProperty.getText("Clipboard", "")
                        Call cpCore.visitProperty.setProperty("Clipboard", "")
                        Call genericController.ModifyQueryString(cpCore.web_RefreshQueryString, RequestNamePasteParentContentID, "")
                        Call genericController.ModifyQueryString(cpCore.web_RefreshQueryString, RequestNamePasteParentRecordID, "")
                        ClipParentContentName = cpCore.metaData.getContentNameByID(ClipParentContentID)
                        If (ClipParentContentName = "") Then
                            ' state not working...
                        ElseIf (ClipBoard = "") Then
                            ' state not working...
                        Else
                            If Not cpCore.authContext.isAuthenticatedContentManager(cpCore, ClipParentContentName) Then
                                Call cpCore.error_AddUserError("The paste operation failed because you are not a content manager of the Clip Parent")
                            Else
                                '
                                ' Current identity is a content manager for this content
                                '
                                Dim Position As Integer = genericController.vbInstr(1, ClipBoard, ".")
                                If Position = 0 Then
                                    Call cpCore.error_AddUserError("The paste operation failed because the clipboard data is configured incorrectly.")
                                Else
                                    ClipBoardArray = Split(ClipBoard, ".")
                                    If UBound(ClipBoardArray) = 0 Then
                                        Call cpCore.error_AddUserError("The paste operation failed because the clipboard data is configured incorrectly.")
                                    Else
                                        ClipChildContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                        ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                        If Not cpCore.IsWithinContent(ClipChildContentID, ClipParentContentID) Then
                                            Call cpCore.error_AddUserError("The paste operation failed because the destination location is not compatible with the clipboard data.")
                                        Else
                                            '
                                            ' the content definition relationship is OK between the child and parent record
                                            '
                                            ClipChildContentName = cpCore.metaData.getContentNameByID(ClipChildContentID)
                                            If Not ClipChildContentName <> "" Then
                                                Call cpCore.error_AddUserError("The paste operation failed because the clipboard data content is undefined.")
                                            Else
                                                If (ClipParentRecordID = 0) Then
                                                    Call cpCore.error_AddUserError("The paste operation failed because the clipboard data record is undefined.")
                                                ElseIf cpCore.pages.main_IsChildRecord(ClipChildContentName, ClipParentRecordID, ClipChildRecordID) Then
                                                    Call cpCore.error_AddUserError("The paste operation failed because the destination location is a child of the clipboard data record.")
                                                Else
                                                    '
                                                    ' the parent record is not a child of the child record (circular check)
                                                    '
                                                    ClipChildRecordName = "record " & ClipChildRecordID
                                                    CSClip = cpCore.csOpenRecord(ClipChildContentName, ClipChildRecordID, True, True)
                                                    If Not cpCore.db.cs_ok(CSClip) Then
                                                        Call cpCore.error_AddUserError("The paste operation failed because the data record referenced by the clipboard could not found.")
                                                    Else
                                                        '
                                                        ' Paste the edit record record
                                                        '
                                                        ClipChildRecordName = cpCore.db.cs_getText(CSClip, "name")
                                                        If ClipParentFieldList = "" Then
                                                            '
                                                            ' Legacy paste - go right to the parent id
                                                            '
                                                            If Not cpCore.db.cs_isFieldSupported(CSClip, "ParentID") Then
                                                                Call cpCore.error_AddUserError("The paste operation failed because the record you are pasting does not   support the necessary parenting feature.")
                                                            Else
                                                                Call cpCore.db.cs_set(CSClip, "ParentID", ClipParentRecordID)
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
                                                                    Call cpCore.error_AddUserError("The paste operation failed because the clipboard data Field List is not configured correctly.")
                                                                Else
                                                                    If Not cpCore.db.cs_isFieldSupported(CSClip, CStr(NameValues(0))) Then
                                                                        Call cpCore.error_AddUserError("The paste operation failed because the clipboard data Field [" & CStr(NameValues(0)) & "] is not supported by the location data.")
                                                                    Else
                                                                        Call cpCore.db.cs_set(CSClip, CStr(NameValues(0)), CStr(NameValues(1)))
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
                                                    Call cpCore.db.cs_Close(CSClip)
                                                    '
                                                    ' Set Child Pages Found and clear caches
                                                    '
                                                    CSClip = cpCore.csOpen(ClipParentContentName, ClipParentRecordID, , , "ChildPagesFound")
                                                    If cpCore.db.cs_ok(CSClip) Then
                                                        Call cpCore.db.cs_set(CSClip, "ChildPagesFound", True.ToString)
                                                    End If
                                                    Call cpCore.db.cs_Close(CSClip)
                                                    Call cpCore.pages.cache_pageContent_clear()
                                                    If (cpCore.siteProperties.allowWorkflowAuthoring And cpCore.workflow.isWorkflowAuthoringCompatible(ClipChildContentName)) Then
                                                        '
                                                        ' Workflow editing
                                                        '
                                                    Else
                                                        '
                                                        ' Live Editing
                                                        '
                                                        Call cpCore.cache.invalidateContent(ClipChildContentName)
                                                        Call cpCore.cache.invalidateContent(ClipParentContentName)
                                                        Call cpCore.pages.cache_pageContent_clear()
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    Clip = cpCore.docProperties.getText(RequestNameCutClear)
                    If (Clip <> "") Then
                        '
                        ' if a cut clear, clear the clipboard
                        '
                        Call cpCore.visitProperty.setProperty("Clipboard", "")
                        Clip = cpCore.visitProperty.getText("Clipboard", "")
                        Call genericController.modifyLinkQuery(cpCore.web_RefreshQueryString, RequestNameCutClear, "")
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
                            linkAliasTest1 = cpCore.webServer.requestPathPage
                            If (linkAliasTest1.Substring(0, 1) = "/") Then
                                linkAliasTest1 = linkAliasTest1.Substring(1)
                            End If
                            If linkAliasTest1.Length > 0 Then
                                If (linkAliasTest1.Substring(linkAliasTest1.Length - 1, 1) = "/") Then
                                    linkAliasTest1 = linkAliasTest1.Substring(0, linkAliasTest1.Length - 1)
                                End If
                            End If

                            linkAliasTest2 = linkAliasTest1 & "/"
                            If (Not IsPageNotFound) And (cpCore.webServer.requestPathPage <> "") Then
                                '
                                ' build link variations needed later
                                '
                                '
                                Pos = genericController.vbInstr(1, cpCore.webServer.requestPathPage, "://", vbTextCompare)
                                If Pos <> 0 Then
                                    LinkNoProtocol = Mid(cpCore.webServer.requestPathPage, Pos + 3)
                                    Pos = genericController.vbInstr(Pos + 3, cpCore.webServer.requestPathPage, "/", vbBinaryCompare)
                                    If Pos <> 0 Then
                                        linkDomain = Mid(cpCore.webServer.requestPathPage, 1, Pos - 1)
                                        LinkFullPath = Mid(cpCore.webServer.requestPathPage, Pos)
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
                                Call cpCore.cache_linkForward_load()
                                If cpCore.cache_linkForward <> "" Then
                                    If 0 < genericController.vbInstr(1, cpCore.cache_linkForward, "," & cpCore.webServer.requestPathPage & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPathPage) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, cpCore.cache_linkForward, "," & cpCore.webServer.requestPathPage & "/,", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPathPage & "/") & ")"
                                    ElseIf 0 < genericController.vbInstr(1, cpCore.cache_linkForward, "," & LinkNoProtocol & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & cpCore.db.encodeSQLText(LinkNoProtocol) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, cpCore.cache_linkForward, "," & LinkFullPath & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & cpCore.db.encodeSQLText(LinkFullPath) & ")"
                                    ElseIf 0 < genericController.vbInstr(1, cpCore.cache_linkForward, "," & LinkFullPathNoSlash & ",", vbTextCompare) Then
                                        isLinkForward = True
                                        LinkForwardCriteria = "(active<>0)and(SourceLink=" & cpCore.db.encodeSQLText(LinkFullPathNoSlash) & ")"
                                    End If
                                    If isLinkForward Then
                                        '
                                        ' if match, go look it up and verify all OK
                                        '
                                        isLinkForward = False
                                        Sql = cpCore.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID", , 1)
                                        CSPointer = cpCore.db.cs_openSql(Sql)
                                        If cpCore.db.cs_ok(CSPointer) Then
                                            '
                                            ' Link Forward found - update count
                                            '
                                            Dim tmpLink As String
                                            Dim GroupID As Integer
                                            Dim groupName As String
                                            '
                                            IsInLinkForwardTable = True
                                            Viewings = cpCore.db.cs_getInteger(CSPointer, "Viewings") + 1
                                            Sql = "update ccLinkForwards set Viewings=" & Viewings & " where ID=" & cpCore.db.cs_getInteger(CSPointer, "ID")
                                            Call cpCore.db.executeSql(Sql)
                                            tmpLink = cpCore.db.cs_getText(CSPointer, "DestinationLink")
                                            If tmpLink <> "" Then
                                                '
                                                ' Valid Link Forward (without link it is just a record created by the autocreate function
                                                '
                                                isLinkForward = True
                                                tmpLink = cpCore.db.cs_getText(CSPointer, "DestinationLink")
                                                GroupID = cpCore.db.cs_getInteger(CSPointer, "GroupID")
                                                If GroupID <> 0 Then
                                                    groupName = cpCore.group_GetGroupName(GroupID)
                                                    If groupName <> "" Then
                                                        Call cpCore.group_AddGroupMember(groupName)
                                                    End If
                                                End If
                                                If tmpLink <> "" Then
                                                    RedirectLink = tmpLink
                                                    RedirectReason = "Redirecting because the URL is a valid Link Forward entry."
                                                End If
                                            End If
                                        End If
                                        Call cpCore.db.cs_Close(CSPointer)
                                    End If
                                End If
                                '
                                If (RedirectLink = "") And Not isLinkForward Then
                                    '
                                    ' Test for Link Alias
                                    '
                                    If (linkAliasTest1 & linkAliasTest2 <> "") Then
                                        Dim Ptr As Integer = cpCore.cache_linkAlias_getPtrByName(linkAliasTest1)
                                        If (Ptr < 0) Then
                                            Ptr = cpCore.cache_linkAlias_getPtrByName(linkAliasTest2)
                                        End If
                                        If Ptr >= 0 Then
                                            '
                                            ' Link Alias Found
                                            '
                                            IsLinkAlias = True
                                            '
                                            ' New Way - use pageid and QueryStringSuffix
                                            '
                                            Dim LinkQueryString As String = "bid=" & cpCore.cache_linkAlias(linkAliasCache_pageId, Ptr) & "&" & cpCore.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr)
                                            cpCore.docProperties.setProperty("bid", cpCore.cache_linkAlias(linkAliasCache_pageId, Ptr), False)
                                            Dim nameValuePairs As String() = Split(cpCore.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
                                            For Each nameValuePair As String In nameValuePairs
                                                Dim nameValueThing As String() = Split(nameValuePair, "=")
                                                If (nameValueThing.GetUpperBound(0) = 0) Then
                                                    cpCore.docProperties.setProperty(nameValueThing(0), "", False)
                                                Else
                                                    cpCore.docProperties.setProperty(nameValueThing(0), nameValueThing(1), False)
                                                End If
                                            Next
                                        End If
                                    End If
                                    '
                                    If Not IsLinkAlias Then
                                        '
                                        ' Test for favicon.ico
                                        '
                                        If (LCase(cpCore.webServer.requestPathPage) = "/favicon.ico") Then
                                            '
                                            ' Handle Favicon.ico when the client did not recognize the meta tag
                                            '
                                            Dim Filename As String = cpCore.siteProperties.getText("FaviconFilename", "")
                                            If Filename = "" Then
                                                '
                                                ' no favicon, 404 the call
                                                '
                                                Call cpCore.webServer.web_setResponseStatus("404 Not Found")
                                                Call cpCore.webServer.webServerIO_setResponseContentType("image/gif")
                                                cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return cpCore.htmlDoc.docBuffer
                                            Else
                                                Call cpCore.webServer.webServerIO_Redirect2(cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Filename), "favicon request", False)
                                                cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return cpCore.htmlDoc.docBuffer
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
                                            Call cpCore.siteProperties.setProperty("RobotsTxtFilename", Filename)
                                            Dim Content As String = cpCore.cdnFiles.readFile(Filename)
                                            If Content = "" Then
                                                '
                                                ' save default robots.txt
                                                '
                                                Content = "User-agent: *" & vbCrLf & "Disallow: /admin/" & vbCrLf & "Disallow: /images/"
                                                Call cpCore.appRootFiles.saveFile(Filename, Content)
                                            End If
                                            Content = Content & cpCore.addonCache.addonCache.robotsTxt
                                            Call cpCore.webServer.webServerIO_setResponseContentType("text/plain")
                                            Call cpCore.htmlDoc.writeAltBuffer(Content)
                                            cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                            Return cpCore.htmlDoc.docBuffer
                                        End If
                                        '
                                        ' No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                                        '
                                        If (cpCore.app_errorCount = 0) And cpCore.siteProperties.getBoolean("LinkForwardAutoInsert") And (Not IsInLinkForwardTable) Then
                                            '
                                            ' Add a new Link Forward entry
                                            '
                                            CSPointer = cpCore.InsertCSContent("Link Forwards")
                                            If cpCore.db.cs_ok(CSPointer) Then
                                                Call cpCore.db.cs_set(CSPointer, "Name", cpCore.webServer.requestPathPage)
                                                Call cpCore.db.cs_set(CSPointer, "sourcelink", cpCore.webServer.requestPathPage)
                                                Call cpCore.db.cs_set(CSPointer, "Viewings", 1)
                                            End If
                                            Call cpCore.db.cs_Close(CSPointer)
                                        End If
                                        '
                                        ' real 404
                                        '
                                        IsPageNotFound = True
                                        PageNotFoundSource = cpCore.webServer.requestPathPage
                                        PageNotFoundReason = "The page could not be displayed because the URL is not a valid page, Link Forward, Link Alias or RemoteMethod."
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' ----- do anonymous access blocking
                    '
                    If Not cpCore.authContext.isAuthenticated() Then
                        If (cpCore.webServer.webServerIO_requestPath <> "/") And genericController.vbInstr(1, cpCore.siteProperties.adminURL, cpCore.webServer.webServerIO_requestPath, vbTextCompare) <> 0 Then
                            '
                            ' admin page is excluded from custom blocking
                            '
                        Else
                            Dim AnonymousUserResponseID As Integer = genericController.EncodeInteger(cpCore.siteProperties.getText("AnonymousUserResponseID", "0"))
                            Select Case AnonymousUserResponseID
                                Case 1
                                    '
                                    ' block with login
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3410 - exit for login block")
                                    '
                                    Call cpCore.main_SetMetaContent(0, 0)
                                    Call cpCore.htmlDoc.writeAltBuffer(cpCore.htmlDoc.getLoginPage(False) & cpCore.htmlDoc.html_GetEndOfBody(False, False, False, False))
                                    cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                                Case 2
                                    '
                                    ' block with custom content
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3420 - exit for custom content block")
                                    '
                                    Call cpCore.main_SetMetaContent(0, 0)
                                    Call cpCore.htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll'", "Anonymous User Block")
                                    Dim Copy As String = cr & cpCore.htmlDoc.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=""width:250px;margin:100px auto auto auto;"">The site is currently not available for anonymous access.</p>", cpCore.authContext.user.ID, True, cpCore.authContext.isAuthenticated)
                                    ' -- already encoded
                                    'Copy = EncodeContentForWeb(Copy, "copy content", 0, "", 0)
                                    Copy = "" _
                                            & cpCore.main_docType _
                                            & vbCrLf & "<html>" _
                                            & cr & "<head>" _
                                            & genericController.kmaIndent(cpCore.main_GetHTMLHead()) _
                                            & cr & "</head>" _
                                            & cr & TemplateDefaultBodyTag _
                                            & genericController.kmaIndent(Copy) _
                                            & cr2 & "<div>" _
                                            & cr3 & cpCore.htmlDoc.html_GetEndOfBody(True, True, False, False) _
                                            & cr2 & "</div>" _
                                            & cr & "</body>" _
                                            & vbCrLf & "</html>"
                                    '& "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                                    Call cpCore.htmlDoc.writeAltBuffer(Copy)
                                    cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                            End Select
                        End If
                    End If
                    '-------------------------------------------
                    '
                    ' run the appropriate body addon
                    '
                    bodyAddonId = genericController.EncodeInteger(cpCore.siteProperties.getText("Html Body AddonId", "0"))
                    If bodyAddonId <> 0 Then
                        htmlBody = cpCore.addon.execute(bodyAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", bodyAddonStatusOK, Nothing, "", Nothing, "", cpCore.authContext.user.ID, cpCore.authContext.isAuthenticated)
                    Else
                        htmlBody = pageManager_GetHtmlBody(cpCore)
                    End If
                    If cpCore.docOpen Then
                        '
                        ' Build Body Tag
                        '
                        htmlHead = cpCore.main_GetHTMLHead()
                        If cpCore.pages.templateBodyTag <> "" Then
                            bodyTag = cpCore.pages.templateBodyTag
                        Else
                            bodyTag = TemplateDefaultBodyTag
                        End If
                        '
                        ' Add tools panel to body
                        '
                        htmlBody = htmlBody & cr & "<div>" & genericController.kmaIndent(cpCore.htmlDoc.html_GetEndOfBody(True, True, False, False)) & cr & "</div>"
                        '
                        ' build doc
                        '
                        returnHtml = cpCore.main_assembleHtmlDoc(cpCore.main_docType, htmlHead, bodyTag, cpCore.responseBuffer & htmlBody)
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
                    If genericController.vbLCase(cpCore.webServer.requestPathPage) = genericController.vbLCase(requestAppRootPath & cpCore.siteProperties.serverPageDefault) Then
                        '
                        ' This is a 404 caused by Contensive returning a 404
                        '   possibly because the pageid was not found or was inactive.
                        '   contensive returned a 404 error, and the IIS custom error handler is hitting now
                        '   what we returned as an error cause is lost
                        '   ( because the Custom404Source page is the default page )
                        '   send it to the 404 page
                        '
                        cpCore.webServer.requestPathPage = cpCore.webServer.requestPathPage
                        IsPageNotFound = True
                        PageNotFoundReason = "The page could not be displayed. The record may have been deleted, marked inactive. The page's parent pages or section may be invalid."
                    End If
                End If
                If False Then
                    '
                    'todo consider if we will keep this. It is not straightforward, and and more straightforward method may exist
                    '
                    ' Determine where to go next
                    '   If the current page is not the referring page, redirect to the referring page
                    '   Because...
                    '   - the page with the form (the referrer) was a link alias page. You can not post to a link alias, so internally we post to the default page, and redirect back.
                    '   - This only acts on internal Contensive forms, so developer pages are not effected
                    '   - This way, if the form post comes from a main_GetJSPage Remote Method, it posts to the Content Server,
                    '       then redirects back to the static site (with the new changed content)
                    '
                    If cpCore.webServer.requestReferrer <> "" Then
                        Dim main_ServerReferrerURL As String
                        Dim main_ServerReferrerQs As String
                        Dim Position As Integer
                        main_ServerReferrerURL = cpCore.webServer.requestReferrer
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
                            If cpCore.webServer.webServerIO_requestPage <> "" Then
                                '
                                ' If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
                                '
                                main_ServerReferrerURL = main_ServerReferrerURL & cpCore.webServer.webServerIO_requestPage
                            Else
                                main_ServerReferrerURL = main_ServerReferrerURL & cpCore.siteProperties.serverPageDefault
                            End If
                        End If
                        Dim linkDst As String
                        'main_ServerPage = main_ServerPage
                        If main_ServerReferrerURL <> cpCore.webServer.webServerIO_ServerFormActionURL Then
                            '
                            ' remove any methods from referrer
                            '
                            Dim Copy As String
                            Copy = "Redirecting because a Contensive Form was detected, source URL [" & main_ServerReferrerURL & "] does not equal the current URL [" & cpCore.webServer.webServerIO_ServerFormActionURL & "]. This may be from a Contensive Add-on that now needs to redirect back to the host page."
                            linkDst = cpCore.webServer.webServerIO_requestReferer
                            If main_ServerReferrerQs <> "" Then
                                linkDst = main_ServerReferrerURL
                                main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "")
                                If main_ServerReferrerQs <> "" Then
                                    linkDst = linkDst & "?" & main_ServerReferrerQs
                                End If
                            End If
                            Call cpCore.webServer.webServerIO_Redirect2(linkDst, Copy, False)
                            cpCore.docOpen = False '--- should be disposed by caller --- Call dispose
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
                            Call cpCore.log_appendLogPageNotFound(cpCore.webServer.requestLinkSource)
                            Call cpCore.webServer.web_setResponseStatus("404 Not Found")
                            cpCore.docProperties.setProperty("bid", cpCore.pages.main_GetPageNotFoundPageId())
                            'Call main_mergeInStream("bid=" & main_GetPageNotFoundPageId())
                            If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                cpCore.htmlDoc.main_AdminWarning = PageNotFoundReason
                                cpCore.htmlDoc.main_AdminWarningPageID = 0
                                cpCore.htmlDoc.main_AdminWarningSectionID = 0
                            End If
                        Else
                            '
                            ' old way -- if a (real) 404 page is received, redirect to it to the 404 page with content
                            '
                            RedirectReason = PageNotFoundReason
                            RedirectLink = cpCore.pages.main_ProcessPageNotFound_GetLink(PageNotFoundReason, , PageNotFoundSource)
                        End If
                    End If
                End If
                '
                ' add exception list header
                '
                returnHtml = cpCore.getDocExceptionHtmlList() & returnHtml
                '
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetHTMLDoc2")
            End Try
            Return returnHtml
        End Function
        '
        '
        '
        Private Sub pageManager_ProcessFormPage(cpcore As coreClass, FormPageID As Integer)
            Try
                '
                Dim CS As Integer
                Dim SQL As String
                Dim Formhtml As String
                Dim FormInstructions As String
                Dim f As pagesController.main_FormPagetype
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
                CS = cpcore.csOpen("Form Pages", FormPageID)
                If cpcore.db.cs_ok(CS) Then
                    Formhtml = cpcore.db.cs_getText(CS, "Body")
                    FormInstructions = cpcore.db.cs_getText(CS, "Instructions")
                End If
                Call cpcore.db.cs_Close(CS)
                If FormInstructions <> "" Then
                    '
                    ' Load the instructions
                    '
                    f = cpcore.pages.pageManager_LoadFormPageInstructions(FormInstructions, Formhtml)
                    If f.AuthenticateOnFormProcess And Not cpcore.authContext.isAuthenticated() And cpcore.authContext.isRecognized(cpcore) Then
                        '
                        ' If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
                        '
                        Call cpcore.authContext.logout(cpcore)
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
                                    FormValue = cpcore.docProperties.getText(.PeopleField)
                                    If (FormValue <> "") And genericController.EncodeBoolean(cpcore.GetContentFieldProperty("people", .PeopleField, "uniquename")) Then
                                        SQL = "select count(*) from ccMembers where " & .PeopleField & "=" & cpcore.db.encodeSQLText(FormValue)
                                        CS = cpcore.db.cs_openSql(SQL)
                                        If cpcore.db.cs_ok(CS) Then
                                            Success = cpcore.db.cs_getInteger(CS, "cnt") = 0
                                        End If
                                        Call cpcore.db.cs_Close(CS)
                                        If Not Success Then
                                            cpcore.error_AddUserError("The field [" & .Caption & "] must be unique, and the value [" & cpcore.htmlDoc.html_EncodeHTML(FormValue) & "] has already been used.")
                                        End If
                                    End If
                                    If (.REquired Or genericController.EncodeBoolean(cpcore.GetContentFieldProperty("people", .PeopleField, "required"))) And FormValue = "" Then
                                        Success = False
                                        cpcore.error_AddUserError("The field [" & cpcore.htmlDoc.html_EncodeHTML(.Caption) & "] is required.")
                                    Else
                                        If Not cpcore.db.cs_ok(CSPeople) Then
                                            CSPeople = cpcore.csOpen("people", cpcore.authContext.user.ID)
                                        End If
                                        If cpcore.db.cs_ok(CSPeople) Then
                                            Select Case genericController.vbUCase(.PeopleField)
                                                Case "NAME"
                                                    PeopleName = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case "FIRSTNAME"
                                                    PeopleFirstName = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case "LASTNAME"
                                                    PeopleLastName = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case "EMAIL"
                                                    PeopleEmail = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case "USERNAME"
                                                    PeopleUsername = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case "PASSWORD"
                                                    PeoplePassword = FormValue
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                                Case Else
                                                    Call cpcore.db.cs_set(CSPeople, .PeopleField, FormValue)
                                            End Select
                                        End If
                                    End If
                                Case 2
                                    '
                                    ' Group main_MemberShip
                                    '
                                    IsInGroup = cpcore.docProperties.getBoolean("Group" & .GroupName)
                                    WasInGroup = cpcore.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                                    If WasInGroup And Not IsInGroup Then
                                        cpcore.group_DeleteGroupMember(.GroupName)
                                    ElseIf IsInGroup And Not WasInGroup Then
                                        cpcore.group_AddGroupMember(.GroupName)
                                    End If
                            End Select
                        End With
                    Next
                    '
                    ' Create People Name
                    '
                    If PeopleName = "" And PeopleFirstName <> "" And PeopleLastName <> "" Then
                        If cpcore.db.cs_ok(CSPeople) Then
                            Call cpcore.db.cs_set(CSPeople, "name", PeopleFirstName & " " & PeopleLastName)
                        End If
                    End If
                    Call cpcore.db.cs_Close(CSPeople)
                    '
                    ' AuthenticationOnFormProcess requires Username/Password and must be valid
                    '
                    If Success Then
                        '
                        ' Authenticate
                        '
                        If f.AuthenticateOnFormProcess Then
                            Call cpcore.authContext.authenticateById(cpcore, cpcore.authContext.user.ID, cpcore.authContext)
                        End If
                        '
                        ' Join Group requested by page that created form
                        '
                        Dim tokenDate As Date
                        Call cpcore.security.decodeToken(cpcore.docProperties.getText("SuccessID"), GroupIDToJoinOnSuccess, tokenDate)
                        'GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                        If GroupIDToJoinOnSuccess <> 0 Then
                            Call cpcore.group_AddGroupMember(cpcore.group_GetGroupName(GroupIDToJoinOnSuccess))
                        End If
                        '
                        ' Join Groups requested by pageform
                        '
                        If f.AddGroupNameList <> "" Then
                            Groups = Split(Trim(f.AddGroupNameList), ",")
                            For Ptr = 0 To UBound(Groups)
                                GroupName = Trim(Groups(Ptr))
                                If GroupName <> "" Then
                                    Call cpcore.group_AddGroupMember(GroupName)
                                End If
                            Next
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
                Throw
            End Try
        End Sub
        '
        '========================================================================
        '   Returns the HTML body
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Function pageManager_GetHtmlBody(cpCore As coreClass) As String
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
                Dim Result As New stringBuilderLegacyController
                Dim Content As String
                Dim ContentIndent As String
                Dim ContentCnt As Integer
                Dim PageContent As String
                Dim Stream As New stringBuilderLegacyController
                Dim LocalTemplateID As Integer
                Dim LocalTemplateName As String
                Dim LocalTemplateBody As String
                Dim Parse As htmlParserController
                Dim blockSiteWithLogin As Boolean
                Dim addonCachePtr As Integer
                Dim addonId As Integer
                Dim AddonName As String
                '
                Call cpCore.addonCache.load()
                returnHtmlBody = ""
                '
                ' ----- OnBodyStart add-ons
                '
                FilterStatusOK = False
                Cnt = UBound(cpCore.addonCache.addonCache.onBodyStartPtrs) + 1
                For Ptr = 0 To Cnt - 1
                    addonCachePtr = cpCore.addonCache.addonCache.onBodyStartPtrs(Ptr)
                    If addonCachePtr > -1 Then
                        addonId = cpCore.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                        'hint = hint & ",addonId=" & addonId
                        If addonId > 0 Then
                            AddonName = cpCore.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                            'hint = hint & ",AddonName=" & AddonName
                            returnHtmlBody = returnHtmlBody & cpCore.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextOnBodyStart, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                            If Not FilterStatusOK Then
                                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError12("pageManager_GetHtmlBody", "There was an error processing OnAfterBody [" & cpcore.addonCache.addonCache.addonList(addonCachePtr.ToString).name & "]. Filtering was aborted.")
                                Exit For
                            End If
                        End If
                    End If
                Next
                '
                ' ----- main_Get Content (Already Encoded)
                '
                blockSiteWithLogin = False
                PageContent = getHtmlBody_GetSection(cpCore, True, True, False, blockSiteWithLogin)
                If blockSiteWithLogin Then
                    '
                    ' section blocked, just return the login box in the page content
                    '
                    returnHtmlBody = "" _
                        & cr & "<div class=""ccLoginPageCon"">" _
                        & genericController.kmaIndent(PageContent) _
                        & cr & "</div>" _
                        & ""
                ElseIf Not cpCore.docOpen Then
                    '
                    ' exit if stream closed during main_GetSectionpage
                    '
                    returnHtmlBody = ""
                Else
                    '
                    ' no section block, continue
                    '
                    'PageContent = CR & "<!-- Page Content -->" & genericController.kmaIndent(pageManager_GetHtmlBody_GetSection(True, True, False)) & CR & "<!-- /Page Content -->"

                    Call cpCore.pages.pageManager_LoadTemplateGetID(cpCore.pages.currentTemplateID)
                    '
                    ' ----- main_Get Template
                    '
                    LocalTemplateID = cpCore.pages.currentTemplateID
                    LocalTemplateBody = cpCore.pages.templateBody
                    If LocalTemplateBody = "" Then
                        LocalTemplateBody = TemplateDefaultBody
                    End If
                    LocalTemplateName = cpCore.pages.templateName
                    If LocalTemplateName = "" Then
                        LocalTemplateName = "Template " & LocalTemplateID
                    End If
                    '
                    ' ----- Encode Template
                    '
                    If Not cpCore.htmlDoc.pageManager_printVersion Then
                        LocalTemplateBody = cpCore.htmlDoc.html_executeContentCommands(Nothing, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.authContext.user.ID, cpCore.authContext.isAuthenticated, layoutError)
                        returnHtmlBody = returnHtmlBody & cpCore.htmlDoc.html_encodeContent9(LocalTemplateBody, cpCore.authContext.user.ID, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain, False, cpCore.siteProperties.defaultWrapperID, PageContent, CPUtilsBaseClass.addonContext.ContextTemplate)
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
                    If Not cpCore.authContext.isAuthenticated() Then
                        '
                        ' not logged in
                        '
                    Else
                        '
                        ' Add template editing
                        '
                        If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") And cpCore.authContext.isEditing(cpCore, "Page Templates") Then
                            returnHtmlBody = cpCore.htmlDoc.main_GetEditWrapper("Page Template [" & LocalTemplateName & "]", cpCore.main_GetRecordEditLink2("Page Templates", LocalTemplateID, False, LocalTemplateName, cpCore.authContext.isEditing(cpCore, "Page Templates")) & returnHtmlBody)
                        End If
                    End If
                    '
                    ' ----- OnBodyEnd add-ons
                    '
                    'hint = hint & ",onBodyEnd"
                    FilterStatusOK = False
                    Cnt = UBound(cpCore.addonCache.addonCache.onBodyEndPtrs) + 1
                    'hint = hint & ",cnt=" & Cnt
                    For Ptr = 0 To Cnt - 1
                        addonCachePtr = cpCore.addonCache.addonCache.onBodyEndPtrs(Ptr)
                        'hint = hint & ",ptr=" & Ptr & ",addonCachePtr=" & addonCachePtr
                        If addonCachePtr > -1 Then
                            addonId = cpCore.addonCache.addonCache.addonList(addonCachePtr.ToString).id
                            'hint = hint & ",addonId=" & addonId
                            If addonId > 0 Then
                                AddonName = cpCore.addonCache.addonCache.addonList(addonCachePtr.ToString).name
                                'hint = hint & ",AddonName=" & AddonName
                                cpCore.htmlDoc.html_DocBodyFilter = returnHtmlBody
                                AddonReturn = cpCore.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                                returnHtmlBody = cpCore.htmlDoc.html_DocBodyFilter & AddonReturn
                                If Not FilterStatusOK Then
                                    Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError12("pageManager_GetHtmlBody", "There was an error processing OnBodyEnd for [" & AddonName & "]. Filtering was aborted.")
                                    Exit For
                                End If
                            End If
                        End If
                    Next
                    '
                    ' Make it pretty for those who care
                    '
                    If cpCore.siteProperties.getBoolean("AutoHTMLFormatting") Then
                        IndentCnt = 0
                        Parse = New htmlParserController(cpCore)
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
                cpCore.handleExceptionAndContinue(ex) : Throw
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
        Public Function getHtmlBody_GetSection(cpCore As coreClass, AllowChildPageList As Boolean, AllowReturnLink As Boolean, AllowEditWrapper As Boolean, ByRef return_blockSiteWithLogin As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim test As String
                Dim Copy As String
                Dim allowPageWithoutSectionDislay As Boolean
                Dim domainIds() As String
                Dim setdomainId As Integer
                Dim linkDomain As String
                Dim templatedomainIdList As String
                Dim FieldRows As Integer
                Dim PCCPtr As Integer
                Dim SecureLink_CurrentURL As Boolean                ' the current page starts https://
                Dim SecureLink_Template_Required As Boolean         ' the template record has 'secure' checked
                Dim SecureLink_Page_Required As Boolean             ' teh page record has 'secure' checked
                Dim SecureLink_Required As Boolean                  ' either the template or the page have secure checked
                Dim templateLink As String                          ' the template record 'link' field
                Dim TCPtr As Integer
                Dim JSFilename As String
                Dim StylesFilename As String
                Dim SharedStylesIDList As String
                Dim ListSplit() As String
                Dim styleId As Integer
                Dim templateId As Integer
                Dim CurrentLink As String
                Dim CurrentLinkNoQuery As String
                Dim LinkSplit() As String
                Dim LandingLink As String
                Dim CSSection As Integer
                Dim rootPageId As Integer
                Dim RootPageName As String
                Dim RootPageContentName As String
                Dim SectionContentID As Integer
                Dim SectionTemplateID As Integer
                Dim SectionBlock As Boolean
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
                Dim UseContentWatchLink As Boolean = cpCore.siteProperties.useContentWatchLink
                '
                ' ----- Determine LandingLink
                '
                LandingLink = cpCore.pages.pageManager_GetLandingLink()
                IsSectionRootPageIDMode = True
                SectionFieldList = "ID,Name,ContentID,TemplateID,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody,JSFilename"
                '
                ' -- get page and section requests
                PageID = cpCore.docProperties.getInteger("bid")
                If PageID <> 0 Then
                    Call cpCore.htmlDoc.webServerIO_addRefreshQueryString("bid", CStr(PageID))
                End If
                '
                ' Handle Section ID Request Variable
                '
                SectionID = cpCore.docProperties.getInteger("sid")
                If SectionID <> 0 Then
                    Call cpCore.htmlDoc.webServerIO_addRefreshQueryString("sid", CStr(SectionID))
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
                    PageID = cpCore.pages.main_GetLandingPageID()
                    If PageID = 0 Then
                        '
                        ' landing page is not valid -- display error
                        '
                        cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                    End If
                End If
                If (PageID <> 0) Then
                    'hint = "3"
                    '
                    ' ----- PageID Given, determine the section from the pageid
                    '
                    If cpCore.pages.cache_siteSection_rows = 0 Then
                        Call cpCore.pages.pageManager_cache_siteSection_load()
                    End If
                    rootPageId = cpCore.pages.pageManager_GetHtmlBody_GetSection_GetRootPageId(PageID)
                    Ptr = cpCore.pages.cache_siteSection_RootPageIDIndex.getPtr(CStr(rootPageId))
                    '
                    ' Open Section Record
                    '
                    If Ptr < 0 Then
                        SectionID = 0
                    Else
                        SectionID = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_ID, Ptr))
                        SectionName = genericController.encodeText(cpCore.pages.cache_siteSection(SSC_Name, Ptr))
                        SectionTemplateID = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_TemplateID, Ptr))
                        SectionContentID = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_ContentID, Ptr))
                        SectionBlock = genericController.EncodeBoolean(cpCore.pages.cache_siteSection(SSC_BlockSection, Ptr))
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSOnLoad, Ptr)), "site section")
                        Call cpCore.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSHead, Ptr)), "site section")
                        Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSEndBody, Ptr)), "site section")
                        JSFilename = genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSFilename, Ptr))
                        If JSFilename <> "" Then
                            JSFilename = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                            Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                        End If
                    End If
                ElseIf (SectionID <> 0) Then
                    'hint = "4"
                    '
                    ' ----- pageid=0, sectionid OK -- determine the page from the sectionid
                    '           main_Get SectionLink -> compare to actual Link -> redirect if mismatch
                    '
                    If cpCore.pages.cache_siteSection_rows = 0 Then
                        Call cpCore.pages.pageManager_cache_siteSection_load()
                    End If
                    Ptr = cpCore.pages.cache_siteSection_IDIndex.getPtr(CStr(SectionID))
                    If Ptr < 0 Then
                        '
                        ' Section not found, assume Landing Page
                        '
                        Call cpCore.log_appendLogPageNotFound(cpCore.webServer.requestLinkSource)
                        'Call main_LogPageNotFound(main_ServerLink)
                        cpCore.pages.pageManager_RedirectBecausePageNotFound = True
                        cpCore.pages.pageManager_RedirectReason = "The page could not be found because the section specified was not found. The section ID is [" & SectionID & "]. This section may have been deleted or marked inactive."
                        cpCore.pages.redirectLink = cpCore.pages.main_ProcessPageNotFound_GetLink(cpCore.pages.pageManager_RedirectReason, , , PageID, SectionID)
                        Exit Function
                        'SectionID = 0
                    Else
                        SectionName = genericController.encodeText(cpCore.pages.cache_siteSection(SSC_Name, Ptr))
                        rootPageId = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_RootPageID, Ptr))
                        SectionTemplateID = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_TemplateID, Ptr))
                        SectionContentID = genericController.EncodeInteger(cpCore.pages.cache_siteSection(SSC_ContentID, Ptr))
                        SectionBlock = genericController.EncodeBoolean(cpCore.pages.cache_siteSection(SSC_BlockSection, Ptr))
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSOnLoad, Ptr)), "site section")
                        Call cpCore.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSHead, Ptr)), "site section")
                        Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSEndBody, Ptr)), "site section")
                        JSFilename = genericController.encodeText(cpCore.pages.cache_siteSection(SSC_JSFilename, Ptr))
                        If JSFilename <> "" Then
                            JSFilename = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                            Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                        End If
                    End If
                    '
                    ' Test for new section that needs a new blank page
                    '
                    If (cpCore.pages.redirectLink = "") And (SectionID <> 0) Then
                        '
                        ' RootPageID Mode
                        '
                        If (rootPageId = 0) And (cpCore.app_errorCount = 0) Then
                            '
                            ' Root Page needs to be auto created
                            ' OK to create page here because section has a good record with a 0 RootPageID (this is not AutoHomeCreate)
                            '
                            rootPageId = cpCore.main_CreatePageGetID(SectionName, "Page Content", SystemMemberID, "")
                            Call cpCore.db.executeSql("update ccsections set RootPageID=" & rootPageId & " where id=" & SectionID)
                            Call cpCore.pages.pageManager_cache_siteSection_clear()
                            cpCore.htmlDoc.main_AdminWarning = "<p>This page was created automatically because the section [" & SectionName & "] was requested, and it did not reference a page. Use the links below to edit the new page.</p>"
                            cpCore.htmlDoc.main_AdminWarningPageID = rootPageId
                            cpCore.htmlDoc.main_AdminWarningSectionID = SectionID
                            'Call app.csv_SetCS(CSSection, "RootPageID", RootPageID)
                        End If
                    End If
                    If (cpCore.pages.redirectLink = "") And (PageID = 0) And (rootPageId <> 0) Then
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
                    & cpCore.main_docType _
                    & vbCrLf & "<html>" _
                    & cr & "<body>" _
                    & cr2 & "<p style=""text-align:center;margin-top:100px;"">The page you requested could not be found and no landing page is configured for this domain.</p>" _
                    & cr & "</body>" _
                    & vbCrLf & "</html>" _
                    & ""
                    'Call AppendLog("call main_getEndOfBody, from pageManager_getsection")
                    Call cpCore.htmlDoc.writeAltBuffer(Copy & cpCore.htmlDoc.html_GetEndOfBody(False, False, False, False))
                    Throw New ApplicationException("Unexpected exception") ' throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError12("PagList_GetSection", "The page you requested could not be found and no landing page is configured for this domain [" & cpcore.webServer.webServerIO_requestDomain & "].")
                    '--- should be disposed by caller --- Call dispose
                    Exit Function
                End If
                '
                If SectionID = 0 Then
                    '
                    ' Orphan Page -- Section could not be determined
                    '
                    allowPageWithoutSectionDislay = cpCore.siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                    allowPageWithoutSectionDislay = True
                    Call logController.appendLog(cpCore, "hardcoded allowPageWithoutSectionDislay in getHtmlBody_getSection")
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
                        rootPageId = cpCore.pages.main_GetLandingPageID()
                        RootPageName = cpCore.pages.main_GetLandingPageName(rootPageId)
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
                            SectionCriteria = "Name = " & cpCore.db.encodeSQLText(SectionName)
                        End If
                        '
                        ' main_Get Landing Section
                        '
                        CSSection = cpCore.db.cs_open("Site Sections", SectionCriteria, "ID", , ,, , SectionFieldList)
                        '
                        ' try something new - if no landing section, use a "dummy" section with no blocking, etc.
                        '
                        If Not cpCore.db.cs_ok(CSSection) Then
                            SectionID = 0
                            SectionName = ""
                            SectionTemplateID = 0
                            SectionContentID = 0
                            SectionBlock = False
                        Else
                            SectionID = cpCore.db.cs_getInteger(CSSection, "ID")
                            SectionName = cpCore.db.cs_getText(CSSection, "name")
                            SectionTemplateID = cpCore.db.cs_getInteger(CSSection, "TemplateID")
                            SectionContentID = cpCore.db.cs_getInteger(CSSection, "ContentID")
                            SectionBlock = cpCore.db.cs_getBoolean(CSSection, "BlockSection")
                            Call cpCore.htmlDoc.main_AddOnLoadJavascript2(cpCore.db.cs_getText(CSSection, "JSOnLoad"), "site section")
                            Call cpCore.htmlDoc.main_AddHeadScriptCode(cpCore.db.cs_getText(CSSection, "JSHead"), "site section")
                            Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(cpCore.db.cs_getText(CSSection, "JSEndBody"), "site section")
                            JSFilename = cpCore.db.cs_getText(CSSection, "JSFilename")
                            If JSFilename <> "" Then
                                JSFilename = cpCore.webServer.webServerIO_requestPage & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                                Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, "site section")
                            End If
                        End If
                        Call cpCore.db.cs_Close(CSSection)
                        '
                        ' ????? if no landing section, lets try a dummy section
                        '
                    End If
                End If
                '
                ' Save the SectionID publically to Add-ons can use it (dynamic menuing)
                '
                cpCore.pages.currentSectionID = SectionID
                cpCore.pages.currentSectionName = SectionName
                '
                '------------------------------------------------------------------------------------
                ' Determine RootPageContentName from SectionContentID
                '------------------------------------------------------------------------------------
                '
                'hint = "5"
                If SectionContentID = 0 And RootPageContentName = "" Then
                    RootPageContentName = "Page Content"
                    SectionContentID = cpCore.main_GetContentID(RootPageContentName)
                ElseIf SectionContentID = 0 Then
                    SectionContentID = cpCore.main_GetContentID(RootPageContentName)
                Else
                    RootPageContentName = cpCore.metaData.getContentNameByID(SectionContentID)
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
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "bid", CStr(PageID), True)
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "sid", "")
                ElseIf SectionID <> 0 Then
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "bid", "")
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "sid", CStr(SectionID), True)
                Else
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "bid", "")
                    cpCore.web_RefreshQueryString = genericController.ModifyQueryString(cpCore.web_RefreshQueryString, "sid", "")
                End If
                '
                '------------------------------------------------------------------------------------
                '   main_Get the Content and Template
                '   main_Get the TemplateID (link,body) from the currentNavigationStructure (already loaded)
                '------------------------------------------------------------------------------------
                '
                'hint = "7"
                '
                ' ????? if no section, allow a dummy section with id=0
                '
                If cpCore.pages.redirectLink = "" Then
                    If cpCore.main_isSectionBlocked(SectionID, SectionBlock) Then
                        '
                        ' Fake the meta content call to block the 'head before content' error and block with login panel
                        '
                        return_blockSiteWithLogin = True
                        Call cpCore.main_SetMetaContent(0, 0)
                        returnHtml = cpCore.htmlDoc.getLoginPanel()
                    Else
                        '
                        ' main_Get the content
                        ' if QuickEditing, the content comes back with fpo_QuickEditor where content should be because
                        '   at this point we do not know which template it will be displayed on. After template calculate, if fpo_QucikEditor there,
                        '   call the main_GetFormInputHTML and replace it out
                        '
                        returnHtml = cpCore.pages.pageManager_GetHtmlBody_GetSection_GetContent(PageID, rootPageId, RootPageContentName, "", AllowChildPageList, AllowReturnLink, False, SectionID, UseContentWatchLink, allowPageWithoutSectionDislay)
                        cpCore.pages.templateReason = "The reason this template was selected could not be determined."
                        If cpCore.pages.redirectLink = "" Then
                            '
                            ' ----- Use the structure to Calculate the Template Link from the loaded content
                            '
                            If cpCore.pages.currentNavigationStructure = "" Then
                                Call cpCore.log_appendLogPageNotFound(cpCore.webServer.requestLinkSource)
                                cpCore.pages.pageManager_RedirectBecausePageNotFound = True
                                cpCore.pages.pageManager_RedirectReason = "Redirecting because the page selected could not be found."
                                cpCore.pages.redirectLink = cpCore.pages.main_ProcessPageNotFound_GetLink(cpCore.pages.pageManager_RedirectReason, , , PageID, SectionID)
                                Exit Function
                            Else
                                test = cpCore.pages.currentNavigationStructure
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
                                        If genericController.EncodeInteger(navStruc(cpCore.pages.navStruc_Descriptor)) < cpCore.pages.navStruc_Descriptor_ChildPage Then
                                            If navStruc(cpCore.pages.navStruc_TemplateId) <> "0" Then
                                                templateId = genericController.EncodeInteger(navStruc(cpCore.pages.navStruc_TemplateId))
                                                If genericController.EncodeInteger(navStruc(cpCore.pages.navStruc_Descriptor)) = cpCore.pages.navStruc_Descriptor_CurrentPage Then
                                                    cpCore.pages.templateReason = "This template was used because it is selected by the current page."
                                                Else
                                                    cpCore.pages.templateReason = "This template was used because it is selected one of this page's parents."
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
                                TCPtr = cpCore.pages.pageManager_cache_pageTemplate_getPtr(templateId)
                                If TCPtr < 0 Then
                                    templateId = 0
                                Else
                                    cpCore.pages.currentTemplateID = genericController.EncodeInteger(cpCore.pages.cache_pageTemplate(TC_ID, TCPtr))
                                    cpCore.pages.currentTemplateName = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))
                                End If
                            End If
                            If templateId = 0 Then
                                '
                                ' try section template
                                '
                                templateId = SectionTemplateID
                                If templateId <> 0 Then
                                    TCPtr = cpCore.pages.pageManager_cache_pageTemplate_getPtr(templateId)
                                    If TCPtr < 0 Then
                                        templateId = 0
                                    Else
                                        cpCore.pages.currentTemplateID = genericController.EncodeInteger(cpCore.pages.cache_pageTemplate(TC_ID, TCPtr))
                                        cpCore.pages.currentTemplateName = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))
                                        cpCore.pages.templateReason = "This template [" & cpCore.pages.currentTemplateName & "] was used because it is selected by the current section [" & cpCore.pages.currentSectionName & "]."
                                    End If
                                End If
                                If (templateId = 0) And (cpCore.domains.domainDetails.defaultTemplateId <> 0) Then
                                    '
                                    ' try domain's default template
                                    '
                                    templateId = cpCore.domains.domainDetails.defaultTemplateId
                                    TCPtr = cpCore.pages.pageManager_cache_pageTemplate_getPtr(templateId)
                                    If TCPtr < 0 Then
                                        templateId = 0
                                    Else
                                        cpCore.pages.currentTemplateID = genericController.EncodeInteger(cpCore.pages.cache_pageTemplate(TC_ID, TCPtr))
                                        cpCore.pages.currentTemplateName = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))
                                        cpCore.pages.templateReason = "This template [" & cpCore.pages.currentTemplateName & "] was used because it is selected as the default template for the current domain [" & cpCore.webServer.webServerIO_requestDomain & "]."
                                    End If
                                End If
                                If templateId = 0 Then
                                    '
                                    ' try the template named 'default'
                                    '
                                    Call cpCore.pages.pageManager_cache_pageTemplate_load()
                                    For TCPtr = 0 To cpCore.pages.cache_pageTemplate_rows - 1
                                        If genericController.vbLCase(genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))) = "default" Then
                                            templateId = genericController.EncodeInteger(cpCore.pages.cache_pageTemplate(TC_ID, TCPtr))
                                            Exit For
                                        End If
                                    Next
                                    If TCPtr >= cpCore.pages.cache_pageTemplate_rows Then
                                        TCPtr = -1
                                    Else
                                        cpCore.pages.currentTemplateID = genericController.EncodeInteger(cpCore.pages.cache_pageTemplate(TC_ID, TCPtr))
                                        cpCore.pages.currentTemplateName = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))
                                        cpCore.pages.templateReason = "This template was used because it is the default template for the site and no other template was selected [" & cpCore.pages.currentTemplateName & "]."
                                    End If
                                End If
                            End If
                            '
                            ' Set the Template buffers
                            '
                            If TCPtr >= 0 Then
                                If cpCore.authContext.visit.Mobile Then
                                    If cpCore.siteProperties.getBoolean("AllowMobileTemplates") Then
                                        '
                                        ' set Mobile Template
                                        '
                                        cpCore.pages.templateBody = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_MobileBodyHTML, TCPtr))
                                        If cpCore.pages.templateBody <> "" Then
                                            StylesFilename = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_MobileStylesFilename, TCPtr))
                                        End If
                                    End If
                                End If
                                If cpCore.pages.templateBody = "" Then
                                    '
                                    ' set web template if no other template sets it
                                    '
                                    cpCore.pages.templateBody = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_BodyHTML, TCPtr))
                                    StylesFilename = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_StylesFilename, TCPtr))
                                End If
                                cpCore.pages.templateLink = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Link, TCPtr))
                                cpCore.pages.templateName = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_Name, TCPtr))
                                Call cpCore.htmlDoc.main_AddOnLoadJavascript2(genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_JSOnLoad, TCPtr)), "template")
                                Call cpCore.htmlDoc.main_AddHeadScriptCode(genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_JSInHeadLegacy, TCPtr)), "template")
                                Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_JSEndBody, TCPtr)), "template")
                                Call cpCore.htmlDoc.main_AddHeadTag2(genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_OtherHeadTags, TCPtr)), "template")
                                cpCore.pages.templateBodyTag = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_BodyTag, TCPtr))
                                JSFilename = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_JSInHeadFilename, TCPtr))
                                If JSFilename <> "" Then
                                    JSFilename = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                                    Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, "template")
                                End If
                                '
                                ' Add exclusive styles
                                '
                                If StylesFilename <> "" Then
                                    If genericController.vbLCase(Right(StylesFilename, 4)) <> ".css" Then
                                        Throw New ApplicationException("Unexpected exception") ' throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError15("Template [" & pageManager_TemplateName & "] StylesFilename is not a '.css' file, and will not display correct. Check that the field is setup as a CSSFile.", "pageManager_GetHtmlBody_GetSection")
                                    Else
                                        cpCore.htmlDoc.main_MetaContent_TemplateStyleSheetTag = cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, StylesFilename) & """ >"
                                    End If
                                End If
                                '
                                ' Add shared styles
                                '

                                SharedStylesIDList = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_SharedStylesIDList, TCPtr))
                                If SharedStylesIDList <> "" Then
                                    ListSplit = Split(SharedStylesIDList, ",")
                                    For Ptr = 0 To UBound(ListSplit)
                                        styleId = genericController.EncodeInteger(ListSplit(Ptr))
                                        If styleId <> 0 Then
                                            Call cpCore.htmlDoc.main_AddSharedStyleID2(genericController.EncodeInteger(ListSplit(Ptr)), "template")
                                        End If
                                    Next
                                End If
                            End If
                            ' (*B)
                            templateLink = cpCore.pages.templateLink
                            '
                            ' Verify Template Link matches the current page
                            '
                            If cpCore.pages.redirectLink = "" Then
                                If (TCPtr >= 0) And (cpCore.siteProperties.allowTemplateLinkVerification) Then
                                    PCCPtr = cpCore.pages.cache_pageContent_getPtr(cpCore.pages.currentPageID, cpCore.pages.pagemanager_IsWorkflowRendering, cpCore.authContext.isQuickEditing(cpCore, ""))
                                    '$$$$$ must check for PPtr<0
                                    SecureLink_CurrentURL = (Left(LCase(cpCore.webServer.webServerIO_ServerLink), 8) = "https://")
                                    SecureLink_Template_Required = genericController.EncodeBoolean(cpCore.pages.cache_pageTemplate(TC_IsSecure, TCPtr))
                                    SecureLink_Page_Required = genericController.EncodeBoolean(cpCore.pages.cache_pageContent(PCC_IsSecure, PCCPtr))
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
                                                cpCore.pages.redirectLink = genericController.vbReplace(cpCore.webServer.webServerIO_ServerLink, "https://", "http://")
                                                cpCore.pages.pageManager_RedirectReason = "Redirecting because neither the page or the template requires a secure link."
                                            Else
                                                cpCore.pages.redirectLink = genericController.vbReplace(cpCore.webServer.webServerIO_ServerLink, "http://", "https://")
                                                If SecureLink_Page_Required Then
                                                    cpCore.pages.pageManager_RedirectReason = "Redirecting because this page [" & cpCore.pages.currentPageName & "] requires a secure link."
                                                Else
                                                    cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] requires a secure link."
                                                End If
                                            End If
                                        End If
                                    Else
                                        '
                                        ' ----- TemplateLink given
                                        '
                                        CurrentLink = cpCore.webServer.webServerIO_ServerLink
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
                                                cpCore.pages.redirectLink = cpCore.pages.pageManager_GetSectionLink(templateLink, PageID, SectionID)
                                                If PageID <> 0 Then
                                                    cpCore.pages.pageManager_RedirectBecausePageNotFound = False
                                                    cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & cpCore.pages.templateReason
                                                ElseIf SectionID <> 0 Then
                                                    cpCore.pages.pageManager_RedirectBecausePageNotFound = False
                                                    cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & cpCore.pages.templateReason
                                                Else
                                                    cpCore.pages.pageManager_RedirectBecausePageNotFound = False
                                                    cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & cpCore.pages.templateReason
                                                End If
                                            End If
                                        Else
                                            '
                                            ' ----- TemplateLink is short
                                            '       test current short link vs template short link, and protocols
                                            '
                                            CurrentLink = genericController.vbReplace(CurrentLink, "https://", "http://", 1, 99, vbTextCompare)
                                            CurrentLink = genericController.ConvertLinkToShortLink(CurrentLink, cpCore.webServer.requestDomain, cpCore.webServer.webServerIO_requestVirtualFilePath)
                                            CurrentLink = genericController.EncodeAppRootPath(CurrentLink, cpCore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
                                            LinkSplit = Split(CurrentLink, "?")
                                            CurrentLinkNoQuery = LinkSplit(0)
                                            If (SecureLink_CurrentURL <> SecureLink_Required) Or (UCase(templateLink) <> genericController.vbUCase(CurrentLinkNoQuery)) Then
                                                '
                                                ' This is not the correct page for this content, redirect
                                                ' This is NOT a pagenotfound - but a correctable condition that can not be avoided
                                                ' The pageid has as hard tamplate that must be redirected
                                                '
                                                cpCore.pages.redirectLink = cpCore.pages.getPageLink4(PageID, "", True, False)
                                                If SecureLink_Required Then
                                                    '
                                                    ' Redirect to Secure
                                                    '
                                                    If genericController.vbInstr(1, cpCore.pages.redirectLink, "http", vbTextCompare) = 1 Then
                                                        '
                                                        ' link is full
                                                        '
                                                        cpCore.pages.redirectLink = genericController.vbReplace(cpCore.pages.redirectLink, "http://", "https://", 1, 99, vbTextCompare)
                                                    Else
                                                        '
                                                        ' link is root relative
                                                        '
                                                        cpCore.pages.redirectLink = "https://" & cpCore.webServer.requestDomain & cpCore.pages.redirectLink
                                                    End If
                                                Else
                                                    '
                                                    ' Redirect to non-Secure
                                                    '
                                                    If genericController.vbInstr(1, cpCore.pages.redirectLink, "http", vbTextCompare) = 1 Then
                                                        '
                                                        ' link is full
                                                        '
                                                        cpCore.pages.redirectLink = genericController.vbReplace(cpCore.pages.redirectLink, "https://", "http://", 1, 99, vbTextCompare)
                                                    Else
                                                        '
                                                        ' link is root relative
                                                        '
                                                        cpCore.pages.redirectLink = "http://" & cpCore.webServer.requestDomain & cpCore.pages.redirectLink
                                                    End If
                                                End If
                                                'pageManager_RedirectLink = pageManager_GetSectionLink(TemplateLink, PageID, SectionID)
                                                cpCore.pages.pageManager_RedirectBecausePageNotFound = False
                                                cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] is configured to use the link [" & templateLink & "]. This is may be a normal condition." & cpCore.pages.templateReason
                                            End If
                                        End If
                                    End If
                                    '
                                    ' check template domain requirements
                                    '
                                    If cpCore.pages.redirectLink = "" Then
                                        templatedomainIdList = genericController.encodeText(cpCore.pages.cache_pageTemplate(TC_DomainIdList, TCPtr))
                                        If (cpCore.domains.domainDetails.id = 0) Then
                                            '
                                            ' current domain not recognized or default, use current
                                            '
                                        ElseIf (templatedomainIdList = "") Then
                                            '
                                            ' current template has no domain preference, use current
                                            '
                                        ElseIf (InStr(1, "," & templatedomainIdList & ",", "," & cpCore.domains.domainDetails.id & ",") <> 0) Then
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
                                            linkDomain = cpCore.content_GetRecordName("domains", setdomainId)
                                            If linkDomain <> "" Then
                                                cpCore.pages.redirectLink = genericController.vbReplace(cpCore.webServer.webServerIO_ServerLink, "://" & cpCore.webServer.requestDomain, "://" & linkDomain, 1, 99, vbTextCompare)
                                                cpCore.pages.pageManager_RedirectBecausePageNotFound = False
                                                cpCore.pages.pageManager_RedirectReason = "Redirecting because this template [" & cpCore.pages.currentTemplateName & "] requires a different domain [" & linkDomain & "]." & cpCore.pages.templateReason
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

                        If cpCore.pages.redirectLink = "" And (InStr(1, returnHtml, html_quickEdit_fpo) <> 0) Then
                            FieldRows = genericController.EncodeInteger(cpCore.properties_user_getText("Page Content.copyFilename.PixelHeight", "500"))
                            If FieldRows < 50 Then
                                FieldRows = 50
                                Call cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50)
                            End If
                            styleList = cpCore.htmlDoc.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                            addonListJSON = cpCore.htmlDoc.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
                            Editor = cpCore.htmlDoc.html_GetFormInputHTML3("copyFilename", cpCore.htmlDoc.html_quickEdit_copy, CStr(FieldRows), "100%", False, True, addonListJSON, styleList, styleOptionList)
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
                If cpCore.authContext.isAuthenticatedAdmin(cpCore) And cpCore.htmlDoc.main_AdminWarning <> "" Then
                    '
                    ' Display Admin Warnings with Edits for record errors
                    '
                    If cpCore.htmlDoc.main_AdminWarningPageID <> 0 Then
                        cpCore.htmlDoc.main_AdminWarning = cpCore.htmlDoc.main_AdminWarning & "</p>" & cpCore.main_GetRecordEditLink2("Page Content", cpCore.htmlDoc.main_AdminWarningPageID, True, "Page " & cpCore.htmlDoc.main_AdminWarningPageID, cpCore.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
                        cpCore.htmlDoc.main_AdminWarningPageID = 0
                    End If
                    '
                    If cpCore.htmlDoc.main_AdminWarningSectionID <> 0 Then
                        cpCore.htmlDoc.main_AdminWarning = cpCore.htmlDoc.main_AdminWarning & "</p>" & cpCore.main_GetRecordEditLink2("Site Sections", cpCore.htmlDoc.main_AdminWarningSectionID, True, "Section " & cpCore.htmlDoc.main_AdminWarningSectionID, cpCore.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the section<p>"
                        cpCore.htmlDoc.main_AdminWarningSectionID = 0
                    End If
                    returnHtml = "" _
                    & cpCore.htmlDoc.html_GetAdminHintWrapper(cpCore.htmlDoc.main_AdminWarning) _
                    & returnHtml _
                    & ""
                    cpCore.htmlDoc.main_AdminWarning = ""
                End If
                '
                '------------------------------------------------------------------------------------
                ' handle redirect and edit wrapper
                '------------------------------------------------------------------------------------
                '
                If cpCore.pages.redirectLink <> "" Then
                    Call cpCore.webServer.webServerIO_Redirect2(cpCore.pages.redirectLink, cpCore.pages.pageManager_RedirectReason, cpCore.pages.pageManager_RedirectBecausePageNotFound)
                ElseIf AllowEditWrapper Then
                    returnHtml = cpCore.htmlDoc.main_GetEditWrapper("Page Content", returnHtml)
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return returnHtml
        End Function
    End Class
End Namespace
