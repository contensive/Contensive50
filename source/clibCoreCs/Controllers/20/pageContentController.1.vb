
Option Explicit On
Option Strict On
'
Imports Contensive.BaseClasses
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' build page content system. Persistence is the docController.
    ''' </summary>
    Public Class pageContentController
        Implements IDisposable
        '
        '
        '
        '========================================================================
        '   Returns the entire HTML page based on the bid/sid stream values
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Shared Function getHtmlBody(cpCore As coreClass) As String
            Dim returnHtml As String = ""
            Try
                Dim downloadId As Integer
                Dim Pos As Integer
                Dim htmlDocBody As String
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
                If cpCore.doc.continueProcessing Then
                    '
                    ' -- setup domain
                    Dim domainTest As String = cpCore.webServer.requestDomain.Trim().ToLower().Replace("..", ".")
                    cpCore.doc.domain = Nothing
                    If (Not String.IsNullOrEmpty(domainTest)) Then
                        Dim posDot As Integer = 0
                        Dim loopCnt As Integer = 10
                        Do
                            cpCore.doc.domain = Models.Entity.domainModel.createByName(cpCore, domainTest, New List(Of String))
                            posDot = domainTest.IndexOf("."c)
                            If ((posDot >= 0) And (domainTest.Length > 1)) Then
                                domainTest = domainTest.Substring(posDot + 1)
                            End If
                            loopCnt -= 1
                        Loop While (cpCore.doc.domain Is Nothing) And (posDot >= 0) And (loopCnt > 0)
                    End If


                    If (cpCore.doc.domain Is Nothing) Then

                    End If
                    '
                    ' -- load requested page/template
                    pageContentController.loadPage(cpCore, cpCore.docProperties.getInteger(rnPageId), cpCore.doc.domain)
                    '
                    ' -- execute context for included addons
                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                        .addonType = CPUtilsBaseClass.addonContext.ContextSimple,
                        .cssContainerClass = "",
                        .cssContainerId = "",
                        .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                            .contentName = pageContentModel.contentName,
                            .fieldName = "copyfilename",
                            .recordId = cpCore.doc.page.id
                        },
                        .isIncludeAddon = False,
                        .personalizationAuthenticated = cpCore.doc.authContext.visit.VisitAuthenticated,
                        .personalizationPeopleId = cpCore.doc.authContext.user.id
                    }

                    '
                    ' -- execute template Dependencies
                    Dim templateAddonList As List(Of Models.Entity.addonModel) = addonModel.createList_templateDependencies(cpCore, cpCore.doc.template.ID)
                    For Each addon As addonModel In templateAddonList
                        Dim AddonStatusOK As Boolean = True
                        returnHtml &= cpCore.addon.executeDependency(addon, executeContext)
                        'returnHtml &= cpCore.addon.executeDependency(addon.id, CPUtilsBaseClass.addonContext.ContextSimple, pageContentModel.contentName, cpCore.doc.page.id, "copyFilename", "", 0, AddonStatusOK, cpCore.doc.authContext.user.id, cpCore.doc.authContext.visit.VisitAuthenticated)
                    Next
                    '
                    ' -- execute page Dependencies
                    Dim pageAddonList As List(Of Models.Entity.addonModel) = addonModel.createList_pageDependencies(cpCore, cpCore.doc.page.id)
                    For Each addon As addonModel In pageAddonList
                        Dim AddonStatusOK As Boolean = True
                        returnHtml &= cpCore.addon.executeDependency(addon, executeContext)
                        'returnHtml &= cpCore.addon.executeDependency(addon.id, CPUtilsBaseClass.addonContext.ContextSimple, pageContentModel.contentName, cpCore.doc.page.id, "copyFilename", "", 0, AddonStatusOK, cpCore.doc.authContext.user.id, cpCore.doc.authContext.visit.VisitAuthenticated)
                    Next
                    '
                    cpCore.doc.adminWarning = cpCore.docProperties.getText("main_AdminWarningMsg")
                    cpCore.doc.adminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID")
                    '
                    ' todo move cookie test to htmlDoc controller
                    ' -- Add cookie test
                    Dim AllowCookieTest As Boolean
                    AllowCookieTest = cpCore.siteProperties.allowVisitTracking And (cpCore.doc.authContext.visit.PageVisits = 1)
                    If AllowCookieTest Then
                        Call cpCore.html.addScriptCode_onLoad("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" & cpCore.security.encodeToken(cpCore.doc.authContext.visit.id, cpCore.doc.profileStartTime) & "')};", "Cookie Test")
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   User form processing
                    '       if a form is created in the editor, process it by emailing and saving to the User Form Response content
                    '--------------------------------------------------------------------------
                    '
                    If cpCore.docProperties.getInteger("ContensiveUserForm") = 1 Then
                        Dim FromAddress As String = cpCore.siteProperties.getText("EmailFromAddress", "info@" & cpCore.webServer.requestDomain)
                        Call cpCore.email.sendForm(cpCore.siteProperties.emailAdmin, FromAddress, "Form Submitted on " & cpCore.webServer.requestReferer)
                        Dim cs As Integer = cpCore.db.csInsertRecord("User Form Response")
                        If cpCore.db.csOk(cs) Then
                            Call cpCore.db.csSet(cs, "name", "Form " & cpCore.webServer.requestReferrer)
                            Dim Copy As String = ""

                            For Each key As String In cpCore.docProperties.getKeyList()
                                Dim docProperty As docPropertiesClass = cpCore.docProperties.getProperty(key)
                                If (key.ToLower() <> "contensiveuserform") Then
                                    Copy &= docProperty.Name & "=" & docProperty.Value & vbCrLf
                                End If
                            Next
                            Call cpCore.db.csSet(cs, "copy", Copy)
                            Call cpCore.db.csSet(cs, "VisitId", cpCore.doc.authContext.visit.id)
                        End If
                        Call cpCore.db.csClose(cs)
                    End If
                    '
                    '--------------------------------------------------------------------------
                    '   Contensive Form Page Processing
                    '--------------------------------------------------------------------------
                    '
                    If cpCore.docProperties.getInteger("ContensiveFormPageID") <> 0 Then
                        Call processForm(cpCore, cpCore.docProperties.getInteger("ContensiveFormPageID"))
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Automatic Redirect to a full URL
                    '   If the link field of the record is an absolution address
                    '       rc = redirect contentID
                    '       ri = redirect content recordid
                    '--------------------------------------------------------------------------
                    '
                    cpCore.doc.redirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId))
                    If (cpCore.doc.redirectContentID <> 0) Then
                        cpCore.doc.redirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId))
                        If (cpCore.doc.redirectRecordID <> 0) Then
                            Dim contentName As String = Models.Complex.cdefModel.getContentNameByID(cpCore, cpCore.doc.redirectContentID)
                            If contentName <> "" Then
                                If iisController.main_RedirectByRecord_ReturnStatus(cpCore, contentName, cpCore.doc.redirectRecordID) Then
                                    '
                                    'Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    '
                                    cpCore.doc.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return String.Empty
                                Else
                                    cpCore.doc.adminWarning = "<p>The site attempted to automatically jump to another page, but there was a problem with the page that included the link.<p>"
                                    cpCore.doc.adminWarningPageID = cpCore.doc.redirectRecordID
                                End If
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- Active Download hook
                    Dim RecordEID As String = cpCore.docProperties.getText(RequestNameLibraryFileID)
                    If (RecordEID <> "") Then
                        Dim tokenDate As Date
                        Call cpCore.security.decodeToken(RecordEID, downloadId, tokenDate)
                        If downloadId <> 0 Then
                            '
                            ' -- lookup record and set clicks
                            Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, downloadId)
                            If (file IsNot Nothing) Then
                                file.Clicks += 1
                                file.save(cpCore)
                                If file.Filename <> "" Then
                                    '
                                    ' -- create log entry
                                    Dim log As Models.Entity.libraryFileLogModel = Models.Entity.libraryFileLogModel.add(cpCore)
                                    If (log IsNot Nothing) Then
                                        log.FileID = file.id
                                        log.VisitID = cpCore.doc.authContext.visit.id
                                        log.MemberID = cpCore.doc.authContext.user.id
                                    End If
                                    '
                                    ' -- and go
                                    Dim link As String = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, file.Filename)
                                    Return cpCore.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.")
                                End If
                            End If
                            '
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
                        Call genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCut, "")
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
                        Call genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentContentID, "")
                        Call genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNamePasteParentRecordID, "")
                        ClipParentContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipParentContentID)
                        If (ClipParentContentName = "") Then
                            ' state not working...
                        ElseIf (ClipBoard = "") Then
                            ' state not working...
                        Else
                            If Not cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, ClipParentContentName) Then
                                Call errorController.error_AddUserError(cpCore, "The paste operation failed because you are not a content manager of the Clip Parent")
                            Else
                                '
                                ' Current identity is a content manager for this content
                                '
                                Dim Position As Integer = genericController.vbInstr(1, ClipBoard, ".")
                                If Position = 0 Then
                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.")
                                Else
                                    ClipBoardArray = Split(ClipBoard, ".")
                                    If UBound(ClipBoardArray) = 0 Then
                                        Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data is configured incorrectly.")
                                    Else
                                        ClipChildContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                        ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                        If Not Models.Complex.cdefModel.isWithinContent(cpCore, ClipChildContentID, ClipParentContentID) Then
                                            Call errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is not compatible with the clipboard data.")
                                        Else
                                            '
                                            ' the content definition relationship is OK between the child and parent record
                                            '
                                            ClipChildContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ClipChildContentID)
                                            If Not ClipChildContentName <> "" Then
                                                Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data content is undefined.")
                                            Else
                                                If (ClipParentRecordID = 0) Then
                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data record is undefined.")
                                                ElseIf pageContentController.isChildRecord(cpCore, ClipChildContentName, ClipParentRecordID, ClipChildRecordID) Then
                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is a child of the clipboard data record.")
                                                Else
                                                    '
                                                    ' the parent record is not a child of the child record (circular check)
                                                    '
                                                    ClipChildRecordName = "record " & ClipChildRecordID
                                                    CSClip = cpCore.db.cs_open2(ClipChildContentName, ClipChildRecordID, True, True)
                                                    If Not cpCore.db.csOk(CSClip) Then
                                                        Call errorController.error_AddUserError(cpCore, "The paste operation failed because the data record referenced by the clipboard could not found.")
                                                    Else
                                                        '
                                                        ' Paste the edit record record
                                                        '
                                                        ClipChildRecordName = cpCore.db.csGetText(CSClip, "name")
                                                        If ClipParentFieldList = "" Then
                                                            '
                                                            ' Legacy paste - go right to the parent id
                                                            '
                                                            If Not cpCore.db.cs_isFieldSupported(CSClip, "ParentID") Then
                                                                Call errorController.error_AddUserError(cpCore, "The paste operation failed because the record you are pasting does not   support the necessary parenting feature.")
                                                            Else
                                                                Call cpCore.db.csSet(CSClip, "ParentID", ClipParentRecordID)
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
                                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field List is not configured correctly.")
                                                                Else
                                                                    If Not cpCore.db.cs_isFieldSupported(CSClip, CStr(NameValues(0))) Then
                                                                        Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field [" & CStr(NameValues(0)) & "] is not supported by the location data.")
                                                                    Else
                                                                        Call cpCore.db.csSet(CSClip, CStr(NameValues(0)), CStr(NameValues(1)))
                                                                    End If
                                                                End If
                                                            Next
                                                        End If
                                                        ''
                                                        '' Fixup Content Watch
                                                        ''
                                                        'ShortLink = main_ServerPathPage
                                                        'ShortLink = ConvertLinkToShortLink(ShortLink, main_ServerHost, main_ServerVirtualPath)
                                                        'ShortLink = genericController.modifyLinkQuery(ShortLink, rnPageId, CStr(ClipChildRecordID), True)
                                                        'Call main_TrackContentSet(CSClip, ShortLink)
                                                    End If
                                                    Call cpCore.db.csClose(CSClip)
                                                    '
                                                    ' Set Child Pages Found and clear caches
                                                    '
                                                    CSClip = cpCore.db.csOpenRecord(ClipParentContentName, ClipParentRecordID, , , "ChildPagesFound")
                                                    If cpCore.db.csOk(CSClip) Then
                                                        Call cpCore.db.csSet(CSClip, "ChildPagesFound", True.ToString)
                                                    End If
                                                    Call cpCore.db.csClose(CSClip)
                                                    '
                                                    ' Live Editing
                                                    '
                                                    Call cpCore.cache.invalidateAllObjectsInContent(ClipChildContentName)
                                                    Call cpCore.cache.invalidateAllObjectsInContent(ClipParentContentName)
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
                        Call genericController.modifyLinkQuery(cpCore.doc.refreshQueryString, RequestNameCutClear, "")
                    End If
                    '
                    ' link alias and link forward
                    '
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
                        LinkForwardCriteria = "" _
                            & "(active<>0)" _
                            & "and(" _
                            & "(SourceLink=" & cpCore.db.encodeSQLText(cpCore.webServer.requestPathPage) & ")" _
                            & "or(SourceLink=" & cpCore.db.encodeSQLText(LinkNoProtocol) & ")" _
                            & "or(SourceLink=" & cpCore.db.encodeSQLText(LinkFullPath) & ")" _
                            & "or(SourceLink=" & cpCore.db.encodeSQLText(LinkFullPathNoSlash) & ")" _
                            & ")"
                        isLinkForward = False
                        Sql = cpCore.db.GetSQLSelect("", "ccLinkForwards", "ID,DestinationLink,Viewings,GroupID", LinkForwardCriteria, "ID", , 1)
                        CSPointer = cpCore.db.csOpenSql(Sql)
                        If cpCore.db.csOk(CSPointer) Then
                            '
                            ' Link Forward found - update count
                            '
                            Dim tmpLink As String
                            Dim GroupID As Integer
                            Dim groupName As String
                            '
                            IsInLinkForwardTable = True
                            Viewings = cpCore.db.csGetInteger(CSPointer, "Viewings") + 1
                            Sql = "update ccLinkForwards set Viewings=" & Viewings & " where ID=" & cpCore.db.csGetInteger(CSPointer, "ID")
                            Call cpCore.db.executeQuery(Sql)
                            tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink")
                            If tmpLink <> "" Then
                                '
                                ' Valid Link Forward (without link it is just a record created by the autocreate function
                                '
                                isLinkForward = True
                                tmpLink = cpCore.db.csGetText(CSPointer, "DestinationLink")
                                GroupID = cpCore.db.csGetInteger(CSPointer, "GroupID")
                                If GroupID <> 0 Then
                                    groupName = groupController.group_GetGroupName(cpCore, GroupID)
                                    If groupName <> "" Then
                                        Call groupController.group_AddGroupMember(cpCore, groupName)
                                    End If
                                End If
                                If tmpLink <> "" Then
                                    RedirectLink = tmpLink
                                    RedirectReason = "Redirecting because the URL Is a valid Link Forward entry."
                                End If
                            End If
                        End If
                        Call cpCore.db.csClose(CSPointer)
                        '
                        If (RedirectLink = "") And Not isLinkForward Then
                            '
                            ' Test for Link Alias
                            '
                            If (linkAliasTest1 & linkAliasTest2 <> "") Then
                                Dim sqlLinkAliasCriteria As String = "(name=" & cpCore.db.encodeSQLText(linkAliasTest1) & ")or(name=" & cpCore.db.encodeSQLText(linkAliasTest2) & ")"
                                Dim linkAliasList As List(Of Models.Entity.linkAliasModel) = Models.Entity.linkAliasModel.createList(cpCore, sqlLinkAliasCriteria, "id desc")
                                If (linkAliasList.Count > 0) Then
                                    Dim linkAlias As Models.Entity.linkAliasModel = linkAliasList.First
                                    Dim LinkQueryString As String = rnPageId & "=" & linkAlias.PageID & "&" & linkAlias.QueryStringSuffix
                                    cpCore.docProperties.setProperty(rnPageId, linkAlias.PageID.ToString(), False)
                                    Dim nameValuePairs As String() = Split(linkAlias.QueryStringSuffix, "&")
                                    'Dim nameValuePairs As String() = Split(cpCore.cache_linkAlias(linkAliasCache_queryStringSuffix, Ptr), "&")
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
                                ' No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                                '
                                If (cpCore.doc.errorCount = 0) And cpCore.siteProperties.getBoolean("LinkForwardAutoInsert") And (Not IsInLinkForwardTable) Then
                                    '
                                    ' Add a new Link Forward entry
                                    '
                                    CSPointer = cpCore.db.csInsertRecord("Link Forwards")
                                    If cpCore.db.csOk(CSPointer) Then
                                        Call cpCore.db.csSet(CSPointer, "Name", cpCore.webServer.requestPathPage)
                                        Call cpCore.db.csSet(CSPointer, "sourcelink", cpCore.webServer.requestPathPage)
                                        Call cpCore.db.csSet(CSPointer, "Viewings", 1)
                                    End If
                                    Call cpCore.db.csClose(CSPointer)
                                End If
                                ''
                                '' real 404
                                ''
                                'IsPageNotFound = True
                                'PageNotFoundSource = cpCore.webServer.requestPathPage
                                'PageNotFoundReason = "The page could Not be displayed because the URL Is Not a valid page, Link Forward, Link Alias Or RemoteMethod."
                            End If
                        End If
                    End If
                    '
                    ' ----- do anonymous access blocking
                    '
                    If Not cpCore.doc.authContext.isAuthenticated() Then
                        If (cpCore.webServer.requestPath <> "/") And genericController.vbInstr(1, "/" & cpCore.serverConfig.appConfig.adminRoute, cpCore.webServer.requestPath, vbTextCompare) <> 0 Then
                            '
                            ' admin page is excluded from custom blocking
                            '
                        Else
                            Dim AnonymousUserResponseID As Integer = genericController.EncodeInteger(cpCore.siteProperties.getText("AnonymousUserResponseID", "0"))
                            Select Case AnonymousUserResponseID
                                Case 1
                                    '
                                    ' -- block with login
                                    cpCore.doc.continueProcessing = False
                                    Return cpCore.addon.execute(
                                        addonModel.create(cpCore, addonGuidLoginPage),
                                        New CPUtilsBaseClass.addonExecuteContext() With {
                                            .addonType = CPUtilsBaseClass.addonContext.ContextPage
                                        }
                                    )
                                Case 2
                                    '
                                    ' -- block with custom content
                                    cpCore.doc.continueProcessing = False
                                    Call cpCore.doc.setMetaContent(0, 0)
                                    Call cpCore.html.addScriptCode_onLoad("document.body.style.overflow='scroll'", "Anonymous User Block")
                                    Return cpCore.html.getHtmlDoc(
                                        cr & cpCore.html.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=""width:250px;margin:100px auto auto auto;"">The site is currently not available for anonymous access.</p>", cpCore.doc.authContext.user.id, True, cpCore.doc.authContext.isAuthenticated),
                                        TemplateDefaultBodyTag,
                                        True,
                                        True
                                    )
                            End Select
                        End If
                    End If
                    '
                    ' -- build document
                    htmlDocBody = getHtmlBodyTemplate(cpCore)
                    '
                    ' -- check secure certificate required
                    Dim SecureLink_Template_Required As Boolean = cpCore.doc.template.IsSecure
                    Dim SecureLink_Page_Required As Boolean = False
                    For Each page As Models.Entity.pageContentModel In cpCore.doc.pageToRootList
                        If cpCore.doc.page.IsSecure Then
                            SecureLink_Page_Required = True
                            Exit For
                        End If
                    Next
                    Dim SecureLink_Required As Boolean = SecureLink_Template_Required Or SecureLink_Page_Required
                    Dim SecureLink_CurrentURL As Boolean = (Left(LCase(cpCore.webServer.requestUrl), 8) = "https://")
                    If (SecureLink_CurrentURL And (Not SecureLink_Required)) Then
                        '
                        ' -- redirect to non-secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "https://", "http://")
                        cpCore.doc.redirectReason = "Redirecting because neither the page or the template requires a secure link."
                        Return ""
                    ElseIf ((Not SecureLink_CurrentURL) And SecureLink_Required) Then
                        '
                        ' -- redirect to secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "http://", "https://")
                        If SecureLink_Page_Required Then
                            cpCore.doc.redirectReason = "Redirecting because this page [" & cpCore.doc.pageToRootList(0).name & "] requires a secure link."
                        Else
                            cpCore.doc.redirectReason = "Redirecting because this template [" & cpCore.doc.template.Name & "] requires a secure link."
                        End If
                        Return ""
                    End If
                    '
                    ' -- check that this template exists on this domain
                    ' -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
                    ' -- if endpoint is domain + route (link alias), the route determines the page, which may determine the cpCore.doc.template. If this template is not allowed for this domain, redirect to the domain's landingcpCore.doc.page.
                    '
                    Sql = "(domainId=" & cpCore.doc.domain.id & ")"
                    Dim allowTemplateRuleList As List(Of Models.Entity.TemplateDomainRuleModel) = Models.Entity.TemplateDomainRuleModel.createList(cpCore, Sql)
                    If (allowTemplateRuleList.Count = 0) Then
                        '
                        ' -- current template has no domain preference, use current
                    Else
                        Dim allowTemplate As Boolean = False
                        For Each rule As TemplateDomainRuleModel In allowTemplateRuleList
                            If (rule.templateId = cpCore.doc.template.ID) Then
                                allowTemplate = True
                                Exit For
                            End If
                        Next
                        If (Not allowTemplate) Then
                            '
                            ' -- must redirect to a domain's landing page
                            RedirectLink = cpCore.webServer.requestProtocol & cpCore.doc.domain.name
                            cpCore.doc.redirectBecausePageNotFound = False
                            cpCore.doc.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" & cpCore.doc.template.Name & "]."
                            Return ""
                        End If
                    End If
                    returnHtml &= htmlDocBody
                End If
                '
                ' all other routes should be handled here.
                '   - this code is in initApp right now but should be migrated here.
                '   - if all other routes fail, use the defaultRoute (page manager at first)
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
                            If cpCore.webServer.requestPage <> "" Then
                                '
                                ' If the referer had no page, and there is one here now, it must have been from an IIS redirect, use the current page as the default page
                                '
                                main_ServerReferrerURL = main_ServerReferrerURL & cpCore.webServer.requestPage
                            Else
                                main_ServerReferrerURL = main_ServerReferrerURL & cpCore.siteProperties.serverPageDefault
                            End If
                        End If
                        Dim linkDst As String
                        'main_ServerPage = main_ServerPage
                        If main_ServerReferrerURL <> cpCore.webServer.serverFormActionURL Then
                            '
                            ' remove any methods from referrer
                            '
                            Dim Copy As String
                            Copy = "Redirecting because a Contensive Form was detected, source URL [" & main_ServerReferrerURL & "] does not equal the current URL [" & cpCore.webServer.serverFormActionURL & "]. This may be from a Contensive Add-on that now needs to redirect back to the host page."
                            linkDst = cpCore.webServer.requestReferer
                            If main_ServerReferrerQs <> "" Then
                                linkDst = main_ServerReferrerURL
                                main_ServerReferrerQs = genericController.ModifyQueryString(main_ServerReferrerQs, "method", "")
                                If main_ServerReferrerQs <> "" Then
                                    linkDst = linkDst & "?" & main_ServerReferrerQs
                                End If
                            End If
                            Return cpCore.webServer.redirect(linkDst, Copy)
                            cpCore.doc.continueProcessing = False '--- should be disposed by caller --- Call dispose
                        End If
                    End If
                End If
                If True Then
                    ' - same here, this was in appInit() to prcess the pagenotfounds - maybe here (at the end, maybe in page Manager)
                    '--------------------------------------------------------------------------
                    ' ----- Process Early page-not-found
                    '--------------------------------------------------------------------------
                    '
                    If IsPageNotFound Then
                        If True Then
                            '
                            ' new way -- if a (real) 404 page is received, just convert this hit to the page-not-found page, do not redirect to it
                            '
                            Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                            Call cpCore.webServer.setResponseStatus("404 Not Found")
                            cpCore.docProperties.setProperty(rnPageId, getPageNotFoundPageId(cpCore))
                            'Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                            If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                                cpCore.doc.adminWarning = PageNotFoundReason
                                cpCore.doc.adminWarningPageID = 0
                            End If
                        Else
                            '
                            ' old way -- if a (real) 404 page is received, redirect to it to the 404 page with content
                            '
                            RedirectReason = PageNotFoundReason
                            RedirectLink = pageContentController.main_ProcessPageNotFound_GetLink(cpCore, PageNotFoundReason, , PageNotFoundSource)
                        End If
                    End If
                End If
                '
                ' add exception list header
                '
                returnHtml = errorController.getDocExceptionHtmlList(cpCore) & returnHtml
                '
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetHTMLDoc2")
            End Try
            Return returnHtml
        End Function
        '
        '
        '
        Private Shared Sub processForm(cpcore As coreClass, FormPageID As Integer)
            Try
                '
                Dim CS As Integer
                Dim SQL As String
                Dim Formhtml As String = String.Empty
                Dim FormInstructions As String = String.Empty
                Dim f As main_FormPagetype
                Dim Ptr As Integer
                Dim CSPeople As Integer
                Dim IsInGroup As Boolean
                Dim WasInGroup As Boolean
                Dim FormValue As String
                Dim Success As Boolean
                Dim PeopleFirstName As String = String.Empty
                Dim PeopleLastName As String = String.Empty
                Dim PeopleUsername As String
                Dim PeoplePassword As String
                Dim PeopleName As String = String.Empty
                Dim PeopleEmail As String = String.Empty
                Dim Groups() As String
                Dim GroupName As String
                Dim GroupIDToJoinOnSuccess As Integer
                '
                ' main_Get the instructions from the record
                '
                CS = cpcore.db.csOpenRecord("Form Pages", FormPageID)
                If cpcore.db.csOk(CS) Then
                    Formhtml = cpcore.db.csGetText(CS, "Body")
                    FormInstructions = cpcore.db.csGetText(CS, "Instructions")
                End If
                Call cpcore.db.csClose(CS)
                If FormInstructions <> "" Then
                    '
                    ' Load the instructions
                    '
                    f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml)
                    If f.AuthenticateOnFormProcess And Not cpcore.doc.authContext.isAuthenticated() And cpcore.doc.authContext.isRecognized(cpcore) Then
                        '
                        ' If this form will authenticate when done, and their is a current, non-authenticated account -- logout first
                        '
                        Call cpcore.doc.authContext.logout(cpcore)
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
                                    If (FormValue <> "") And genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", .PeopleField, "uniquename")) Then
                                        SQL = "select count(*) from ccMembers where " & .PeopleField & "=" & cpcore.db.encodeSQLText(FormValue)
                                        CS = cpcore.db.csOpenSql(SQL)
                                        If cpcore.db.csOk(CS) Then
                                            Success = cpcore.db.csGetInteger(CS, "cnt") = 0
                                        End If
                                        Call cpcore.db.csClose(CS)
                                        If Not Success Then
                                            errorController.error_AddUserError(cpcore, "The field [" & .Caption & "] must be unique, and the value [" & genericController.encodeHTML(FormValue) & "] has already been used.")
                                        End If
                                    End If
                                    If (.REquired Or genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "people", .PeopleField, "required"))) And FormValue = "" Then
                                        Success = False
                                        errorController.error_AddUserError(cpcore, "The field [" & genericController.encodeHTML(.Caption) & "] is required.")
                                    Else
                                        If Not cpcore.db.csOk(CSPeople) Then
                                            CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.authContext.user.id)
                                        End If
                                        If cpcore.db.csOk(CSPeople) Then
                                            Select Case genericController.vbUCase(.PeopleField)
                                                Case "NAME"
                                                    PeopleName = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case "FIRSTNAME"
                                                    PeopleFirstName = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case "LASTNAME"
                                                    PeopleLastName = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case "EMAIL"
                                                    PeopleEmail = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case "USERNAME"
                                                    PeopleUsername = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case "PASSWORD"
                                                    PeoplePassword = FormValue
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                                Case Else
                                                    Call cpcore.db.csSet(CSPeople, .PeopleField, FormValue)
                                            End Select
                                        End If
                                    End If
                                Case 2
                                    '
                                    ' Group main_MemberShip
                                    '
                                    IsInGroup = cpcore.docProperties.getBoolean("Group" & .GroupName)
                                    WasInGroup = cpcore.doc.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                                    If WasInGroup And Not IsInGroup Then
                                        groupController.group_DeleteGroupMember(cpcore, .GroupName)
                                    ElseIf IsInGroup And Not WasInGroup Then
                                        groupController.group_AddGroupMember(cpcore, .GroupName)
                                    End If
                            End Select
                        End With
                    Next
                    '
                    ' Create People Name
                    '
                    If PeopleName = "" And PeopleFirstName <> "" And PeopleLastName <> "" Then
                        If cpcore.db.csOk(CSPeople) Then
                            Call cpcore.db.csSet(CSPeople, "name", PeopleFirstName & " " & PeopleLastName)
                        End If
                    End If
                    Call cpcore.db.csClose(CSPeople)
                    '
                    ' AuthenticationOnFormProcess requires Username/Password and must be valid
                    '
                    If Success Then
                        '
                        ' Authenticate
                        '
                        If f.AuthenticateOnFormProcess Then
                            Call cpcore.doc.authContext.authenticateById(cpcore, cpcore.doc.authContext.user.id, cpcore.doc.authContext)
                        End If
                        '
                        ' Join Group requested by page that created form
                        '
                        Dim tokenDate As Date
                        Call cpcore.security.decodeToken(cpcore.docProperties.getText("SuccessID"), GroupIDToJoinOnSuccess, tokenDate)
                        'GroupIDToJoinOnSuccess = main_DecodeKeyNumber(main_GetStreamText2("SuccessID"))
                        If GroupIDToJoinOnSuccess <> 0 Then
                            Call groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, GroupIDToJoinOnSuccess))
                        End If
                        '
                        ' Join Groups requested by pageform
                        '
                        If f.AddGroupNameList <> "" Then
                            Groups = Split(Trim(f.AddGroupNameList), ",")
                            For Ptr = 0 To UBound(Groups)
                                GroupName = Trim(Groups(Ptr))
                                If GroupName <> "" Then
                                    Call groupController.group_AddGroupMember(cpcore, GroupName)
                                End If
                            Next
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
                Throw
            End Try
        End Sub
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
        Friend Shared Function loadFormPageInstructions(cpcore As coreClass, FormInstructions As String, Formhtml As String) As main_FormPagetype
            Dim result As New main_FormPagetype
            Try
                Dim RepeatBody As String
                Dim PtrFront As Integer
                Dim PtrBack As Integer
                Dim i() As String
                Dim IPtr As Integer
                Dim IStart As Integer
                Dim IArgs() As String
                Dim CSPeople As Integer
                '
                If True Then
                    PtrFront = genericController.vbInstr(1, Formhtml, "{{REPEATSTART", vbTextCompare)
                    If PtrFront > 0 Then
                        PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}")
                        If PtrBack > 0 Then
                            result.PreRepeat = Mid(Formhtml, 1, PtrFront - 1)
                            PtrFront = genericController.vbInstr(PtrBack, Formhtml, "{{REPEATEND", vbTextCompare)
                            If PtrFront > 0 Then
                                result.RepeatCell = Mid(Formhtml, PtrBack + 2, PtrFront - PtrBack - 2)
                                PtrBack = genericController.vbInstr(PtrFront, Formhtml, "}}")
                                If PtrBack > 0 Then
                                    result.PostRepeat = Mid(Formhtml, PtrBack + 2)
                                    '
                                    ' Decode instructions and build output
                                    '
                                    i = genericController.SplitCRLF(FormInstructions)
                                    If UBound(i) > 0 Then
                                        If Trim(i(0)) >= "1" Then
                                            '
                                            ' decode Version 1 arguments, then start instructions line at line 1
                                            '
                                            result.AddGroupNameList = genericController.encodeText(i(1))
                                            result.AuthenticateOnFormProcess = genericController.EncodeBoolean(i(2))
                                            IStart = 3
                                        End If
                                        '
                                        ' read in and compose the repeat lines
                                        '

                                        RepeatBody = ""
                                        CSPeople = -1
                                        ReDim result.Inst(UBound(i))
                                        For IPtr = 0 To UBound(i) - IStart
                                            With result.Inst(IPtr)
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
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function

		
		
    End Class
End Namespace