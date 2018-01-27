﻿
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
                            cpCore.doc.domain = domainModel.createByName(cpCore, domainTest, New List(Of String))
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
                    Dim templateAddonList As List(Of addonModel) = addonModel.createList_templateDependencies(cpCore, cpCore.doc.template.ID)
                    For Each addon As addonModel In templateAddonList
                        Dim AddonStatusOK As Boolean = True
                        returnHtml &= cpCore.addon.executeDependency(addon, executeContext)
                        'returnHtml &= cpCore.addon.executeDependency(addon.id, CPUtilsBaseClass.addonContext.ContextSimple, pageContentModel.contentName, cpCore.doc.page.id, "copyFilename", "", 0, AddonStatusOK, cpCore.doc.authContext.user.id, cpCore.doc.authContext.visit.VisitAuthenticated)
                    Next
                    '
                    ' -- execute page Dependencies
                    Dim pageAddonList As List(Of addonModel) = addonModel.createList_pageDependencies(cpCore, cpCore.doc.page.id)
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
                            Dim file As libraryFilesModel = libraryFilesModel.create(cpCore, downloadId)
                            If (file IsNot Nothing) Then
                                file.Clicks += 1
                                file.save(cpCore)
                                If file.Filename <> "" Then
                                    '
                                    ' -- create log entry
                                    Dim log As libraryFileLogModel = libraryFileLogModel.add(cpCore)
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
                                Dim linkAliasList As List(Of linkAliasModel) = linkAliasModel.createList(cpCore, sqlLinkAliasCriteria, "id desc")
                                If (linkAliasList.Count > 0) Then
                                    Dim linkAlias As linkAliasModel = linkAliasList.First
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
                    For Each page As pageContentModel In cpCore.doc.pageToRootList
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
                    Dim allowTemplateRuleList As List(Of TemplateDomainRuleModel) = TemplateDomainRuleModel.createList(cpCore, Sql)
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
        '
        '
        '
        Friend Shared Function getFormPage(cpcore As coreClass, FormPageName As String, GroupIDToJoinOnSuccess As Integer) As String
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
            Dim Formhtml As String = String.Empty
            Dim FormInstructions As String = String.Empty
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
            Dim PeopleCDef As Models.Complex.cdefModel
            '
            IsRetry = (cpcore.docProperties.getInteger("ContensiveFormPageID") <> 0)
            '
            CS = cpcore.db.csOpen("Form Pages", "name=" & cpcore.db.encodeSQLText(FormPageName))
            If cpcore.db.csOk(CS) Then
                FormPageID = cpcore.db.csGetInteger(CS, "ID")
                Formhtml = cpcore.db.csGetText(CS, "Body")
                FormInstructions = cpcore.db.csGetText(CS, "Instructions")
            End If
            Call cpcore.db.csClose(CS)
            f = loadFormPageInstructions(cpcore, FormInstructions, Formhtml)
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
                            If IsRetry And cpcore.docProperties.getText(.PeopleField) = "" Then
                                CaptionSpan = "<span class=""ccError"">"
                            Else
                                CaptionSpan = "<span>"
                            End If
                            If Not cpcore.db.csOk(CSPeople) Then
                                CSPeople = cpcore.db.csOpenRecord("people", cpcore.doc.authContext.user.id)
                            End If
                            Caption = .Caption
                            If .REquired Or genericController.EncodeBoolean(Models.Complex.cdefModel.GetContentFieldProperty(cpcore, "People", .PeopleField, "Required")) Then
                                Caption = "*" & Caption
                            End If
                            If cpcore.db.csOk(CSPeople) Then
                                Body = f.RepeatCell
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan & Caption & "</span>", 1, 99, vbTextCompare)
                                Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.html.html_GetFormInputCS(CSPeople, "People", .PeopleField), 1, 99, vbTextCompare)
                                RepeatBody = RepeatBody & Body
                                HasRequiredFields = HasRequiredFields Or .REquired
                            End If
                        Case 2
                            '
                            ' Group main_MemberShip
                            '
                            GroupValue = cpcore.doc.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                            Body = f.RepeatCell
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.html.html_GetFormInputCheckBox2("Group" & .GroupName, GroupValue), 1, 99, vbTextCompare)
                            Body = genericController.vbReplace(Body, "{{FIELD}}", .Caption)
                            RepeatBody = RepeatBody & Body
                            GroupRowPtr = GroupRowPtr + 1
                            HasRequiredFields = HasRequiredFields Or .REquired
                    End Select
                End With
            Next
            Call cpcore.db.csClose(CSPeople)
            If HasRequiredFields Then
                Body = f.RepeatCell
                Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, vbTextCompare)
                Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields")
                RepeatBody = RepeatBody & Body
            End If
            '
            getFormPage = "" _
            & errorController.error_GetUserError(cpcore) _
            & cpcore.html.html_GetUploadFormStart() _
            & cpcore.html.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) _
            & cpcore.html.html_GetFormInputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.doc.profileStartTime)) _
            & f.PreRepeat _
            & RepeatBody _
            & f.PostRepeat _
            & cpcore.html.html_GetUploadFormEnd()
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetFormPage")
        End Function
        '
        '=============================================================================
        '   getContentBox
        '
        '   PageID is the page to display. If it is 0, the root page is displayed
        '   RootPageID has to be the ID of the root page for PageID
        '=============================================================================
        '
        Public Shared Function getContentBox(cpCore As coreClass, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreme As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim DateModified As Date
                Dim PageRecordID As Integer
                Dim PageName As String
                Dim CS As Integer
                Dim SQL As String
                Dim ContentBlocked As Boolean
                Dim NewPageCreated As Boolean
                Dim SystemEMailID As Integer
                Dim ConditionID As Integer
                Dim ConditionGroupID As Integer
                Dim main_AddGroupID As Integer
                Dim RemoveGroupID As Integer
                Dim RegistrationGroupID As Integer
                Dim BlockedPages() As String
                Dim BlockedPageRecordID As Integer
                Dim BlockCopy As String
                Dim pageViewings As Integer
                Dim layoutError As String = ""
                '
                Call cpCore.html.addHeadTag("<meta name=""contentId"" content=""" & cpCore.doc.page.id & """ >", "page content")
                '
                returnHtml = getContentBox_content(cpCore, OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay)
                '
                ' ----- If Link field populated, do redirect
                If (cpCore.doc.page.PageLink <> "") Then
                    cpCore.doc.page.Clicks += 1
                    cpCore.doc.page.save(cpCore)
                    cpCore.doc.redirectLink = cpCore.doc.page.PageLink
                    cpCore.doc.redirectReason = "Redirect required because this page (PageRecordID=" & cpCore.doc.page.id & ") has a Link Override [" & cpCore.doc.page.PageLink & "]."
                    Return ""
                End If
                '
                ' -- build list of blocked pages
                Dim BlockedRecordIDList As String = ""
                If (returnHtml <> "") And (cpCore.doc.redirectLink = "") Then
                    NewPageCreated = True
                    For Each testPage As pageContentModel In cpCore.doc.pageToRootList
                        If testPage.BlockContent Or testPage.BlockPage Then
                            BlockedRecordIDList = BlockedRecordIDList & "," & testPage.id
                        End If
                    Next
                    If BlockedRecordIDList <> "" Then
                        BlockedRecordIDList = Mid(BlockedRecordIDList, 2)
                    End If
                End If
                '
                ' ----- Content Blocking
                If (BlockedRecordIDList <> "") Then
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                        '
                        ' Administrators are never blocked
                        '
                    ElseIf (Not cpCore.doc.authContext.isAuthenticated()) Then
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
                            & " WHERE (((ccMemberRules.MemberID)=" & cpCore.db.encodeSQLNumber(cpCore.doc.authContext.user.id) & ")" _
                            & " AND ((ccPageContentBlockRules.RecordID) In (" & BlockedRecordIDList & "))" _
                            & " AND ((ccPageContentBlockRules.Active)<>0)" _
                            & " AND ((ccgroups.Active)<>0)" _
                            & " AND ((ccMemberRules.Active)<>0)" _
                            & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & "));"
                        CS = cpCore.db.csOpenSql(SQL)
                        BlockedRecordIDList = "," & BlockedRecordIDList
                        Do While cpCore.db.csOk(CS)
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpCore.db.csGetText(CS, "RecordID"), "")
                            cpCore.db.csGoNext(CS)
                        Loop
                        Call cpCore.db.csClose(CS)
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
                                & " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" & cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime) & ")" _
                                & " AND ((ManagementMemberRules.MemberID)=" & cpCore.doc.authContext.user.id & " ));"
                            CS = cpCore.db.csOpenSql(SQL)
                            Do While cpCore.db.csOk(CS)
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpCore.db.csGetText(CS, "RecordID"), "")
                                cpCore.db.csGoNext(CS)
                            Loop
                            Call cpCore.db.csClose(CS)
                        End If
                        If BlockedRecordIDList <> "" Then
                            ContentBlocked = True
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                End If
                '
                '
                '
                If ContentBlocked Then
                    Dim CustomBlockMessageFilename As String = ""
                    Dim BlockSourceID As Integer = main_BlockSourceDefaultMessage
                    Dim ContentPadding As Integer = 20
                    BlockedPages = Split(BlockedRecordIDList, ",")
                    BlockedPageRecordID = genericController.EncodeInteger(BlockedPages(UBound(BlockedPages)))
                    If BlockedPageRecordID <> 0 Then
                        CS = cpCore.db.csOpenRecord("Page Content", BlockedPageRecordID, , , "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding")
                        If cpCore.db.csOk(CS) Then
                            BlockSourceID = cpCore.db.csGetInteger(CS, "BlockSourceID")
                            ContentPadding = cpCore.db.csGetInteger(CS, "ContentPadding")
                            CustomBlockMessageFilename = cpCore.db.csGetText(CS, "CustomBlockMessage")
                            RegistrationGroupID = cpCore.db.csGetInteger(CS, "RegistrationGroupID")
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    '
                    ' Block Appropriately
                    '
                    Select Case BlockSourceID
                        Case main_BlockSourceCustomMessage
                            '
                            ' ----- Custom Message
                            '
                            returnHtml = cpCore.cdnFiles.readFile(CustomBlockMessageFilename)
                        Case main_BlockSourceLogin
                            '
                            ' ----- Login page
                            '
                            Dim BlockForm As String = ""
                            If Not cpCore.doc.authContext.isAuthenticated() Then
                                If Not cpCore.doc.authContext.isRecognized(cpCore) Then
                                    '
                                    ' -- not recognized
                                    BlockForm = "" _
                                        & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                        & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                        & ""
                                Else
                                    '
                                    ' -- recognized, not authenticated
                                    BlockForm = "" _
                                        & "<p>This content has limited access. You were recognized as ""<b>" & cpCore.doc.authContext.user.name & "</b>"", but you need to login to continue. To login to this account or another, please use this form.</p>" _
                                        & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                        & ""
                                End If
                            Else
                                '
                                ' -- authenticated
                                BlockForm = "" _
                                    & "<p>You are currently logged in as ""<b>" & cpCore.doc.authContext.user.name & "</b>"". If this is not you, please <a href=""?" & cpCore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                    & "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>" _
                                    & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                    & ""
                            End If
                            returnHtml = "" _
                                & "<div style=""margin: 100px, auto, auto, auto;text-align:left;"">" _
                                & errorController.error_GetUserError(cpCore) _
                                & BlockForm _
                                & "</div>"
                        Case main_BlockSourceRegistration
                            '
                            ' ----- Registration
                            '
                            Dim BlockForm As String = ""
                            If cpCore.docProperties.getInteger("subform") = main_BlockSourceLogin Then
                                '
                                ' login subform form
                                BlockForm = "" _
                                    & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                    & "<p>If you do not have an account, <a href=?" & cpCore.doc.refreshQueryString & "&subform=0>click here to register</a>.</p>" _
                                    & cpCore.addon.execute(addonModel.create(cpCore, addonGuidLoginForm), New CPUtilsBaseClass.addonExecuteContext With {.addonType = CPUtilsBaseClass.addonContext.ContextPage}) _
                                    & ""
                            Else
                                '
                                ' Register Form
                                '
                                If Not cpCore.doc.authContext.isAuthenticated() And cpCore.doc.authContext.isRecognized(cpCore) Then
                                    '
                                    ' -- Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                    Call cpCore.doc.authContext.logout(cpCore)
                                End If
                                If Not cpCore.doc.authContext.isAuthenticated() Then
                                    '
                                    ' -- Not Authenticated
                                    Call cpCore.doc.verifyRegistrationFormPage(cpCore)
                                    BlockForm = "" _
                                        & "<p>This content has limited access. If you have an account, <a href=?" & cpCore.doc.refreshQueryString & "&subform=" & main_BlockSourceLogin & ">Click Here to login</a>.</p>" _
                                        & "<p>To view this content, please complete this form.</p>" _
                                        & getFormPage(cpCore, "Registration Form", RegistrationGroupID) _
                                        & ""
                                Else
                                    '
                                    ' -- Authenticated
                                    Call cpCore.doc.verifyRegistrationFormPage(cpCore)
                                    BlockCopy = "" _
                                        & "<p>You are currently logged in as ""<b>" & cpCore.doc.authContext.user.name & "</b>"". If this is not you, please <a href=""?" & cpCore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                        & "<p>This account does not have access to this content. To view this content, please complete this form.</p>" _
                                        & getFormPage(cpCore, "Registration Form", RegistrationGroupID) _
                                        & ""
                                End If
                            End If
                            returnHtml = "" _
                                & "<div style=""margin: 100px, auto, auto, auto;text-align:left;"">" _
                                & errorController.error_GetUserError(cpCore) _
                                & BlockForm _
                                & "</div>"
                        Case Else
                            '
                            ' ----- Content as blocked - convert from site property to content page
                            '
                            returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink)
                    End Select
                    '
                    ' If the output is blank, put default message in
                    '
                    If returnHtml = "" Then
                        returnHtml = getDefaultBlockMessage(cpCore, UseContentWatchLink)
                    End If
                    '
                    ' Encode the copy
                    '
                    returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                    returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                    If cpCore.doc.refreshQueryString <> "" Then
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" & cpCore.doc.refreshQueryString, 1, 99, vbTextCompare)
                    End If
                    '
                    ' Add in content padding required for integration with the template
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, ContentPadding)
                End If
                '
                ' ----- Encoding, Tracking and Triggers
                If Not ContentBlocked Then
                    If cpCore.visitProperty.getBoolean("AllowQuickEditor") Then
                        '
                        ' Quick Editor, no encoding or tracking
                        '
                    Else
                        pageViewings = cpCore.doc.page.Viewings
                        If cpCore.doc.authContext.isEditing(pageContentModel.contentName) Or cpCore.visitProperty.getBoolean("AllowWorkflowRendering") Then
                            '
                            ' Link authoring, workflow rendering -> do encoding, but no tracking
                            '
                            returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                            returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                        Else
                            '
                            ' Live content
                            returnHtml = cpCore.html.executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                            returnHtml = cpCore.html.convertActiveContentToHtmlForWebRender(returnHtml, pageContentModel.contentName, PageRecordID, cpCore.doc.page.ContactMemberID, "http://" & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextPage)
                            Call cpCore.db.executeQuery("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & cpCore.doc.page.id)
                        End If
                        '
                        ' Page Hit Notification
                        '
                        If (Not cpCore.doc.authContext.visit.ExcludeFromAnalytics) And (cpCore.doc.page.ContactMemberID <> 0) And (InStr(1, cpCore.webServer.requestBrowser, "kmahttp", vbTextCompare) = 0) Then
                            If cpCore.doc.page.AllowHitNotification Then
                                PageName = cpCore.doc.page.name
                                If PageName = "" Then
                                    PageName = cpCore.doc.page.MenuHeadline
                                    If PageName = "" Then
                                        PageName = cpCore.doc.page.Headline
                                        If PageName = "" Then
                                            PageName = "[no name]"
                                        End If
                                    End If
                                End If
                                Dim Body As String = ""
                                Body = Body & "<p><b>Page Hit Notification.</b></p>"
                                Body = Body & "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>"
                                Body = Body & genericController.StartTable(4, 1, 1)
                                Body = Body & "<tr><td align=""right"" width=""150"" Class=""ccPanelHeader"">Description<br><img alt=""image"" src=""http://" & cpCore.webServer.requestDomain & "/ccLib/images/spacer.gif"" width=""150"" height=""1""></td><td align=""left"" width=""100%"" Class=""ccPanelHeader"">Value</td></tr>"
                                Body = Body & getTableRow("Domain", cpCore.webServer.requestDomain, True)
                                Body = Body & getTableRow("Link", cpCore.webServer.requestUrl, False)
                                Body = Body & getTableRow("Page Name", PageName, True)
                                Body = Body & getTableRow("Member Name", cpCore.doc.authContext.user.name, False)
                                Body = Body & getTableRow("Member #", CStr(cpCore.doc.authContext.user.id), True)
                                Body = Body & getTableRow("Visit Start Time", CStr(cpCore.doc.authContext.visit.StartTime), False)
                                Body = Body & getTableRow("Visit #", CStr(cpCore.doc.authContext.visit.id), True)
                                Body = Body & getTableRow("Visit IP", cpCore.webServer.requestRemoteIP, False)
                                Body = Body & getTableRow("Browser ", cpCore.webServer.requestBrowser, True)
                                Body = Body & getTableRow("Visitor #", CStr(cpCore.doc.authContext.visitor.ID), False)
                                Body = Body & getTableRow("Visit Authenticated", CStr(cpCore.doc.authContext.visit.VisitAuthenticated), True)
                                Body = Body & getTableRow("Visit Referrer", cpCore.doc.authContext.visit.HTTP_REFERER, False)
                                Body = Body & kmaEndTable
                                Call cpCore.email.sendPerson(cpCore.doc.page.ContactMemberID, cpCore.siteProperties.getText("EmailFromAddress", "info@" & cpCore.webServer.requestDomain), "Page Hit Notification", Body, False, True, 0, "", False)
                            End If
                        End If
                        '
                        ' -- Process Trigger Conditions
                        ConditionID = cpCore.doc.page.TriggerConditionID
                        ConditionGroupID = cpCore.doc.page.TriggerConditionGroupID
                        main_AddGroupID = cpCore.doc.page.TriggerAddGroupID
                        RemoveGroupID = cpCore.doc.page.TriggerRemoveGroupID
                        SystemEMailID = cpCore.doc.page.TriggerSendSystemEmailID
                        Select Case ConditionID
                            Case 1
                                '
                                ' Always
                                '
                                If SystemEMailID <> 0 Then
                                    Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
                                End If
                                If main_AddGroupID <> 0 Then
                                    Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                End If
                                If RemoveGroupID <> 0 Then
                                    Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                End If
                            Case 2
                                '
                                ' If in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)) Then
                                        If SystemEMailID <> 0 Then
                                            Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
                                        End If
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                        End If
                                    End If
                                End If
                            Case 3
                                '
                                ' If not in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If Not cpCore.doc.authContext.IsMemberOfGroup2(cpCore, groupController.group_GetGroupName(cpCore, ConditionGroupID)) Then
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpCore, groupController.group_GetGroupName(cpCore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpCore, groupController.group_GetGroupName(cpCore, RemoveGroupID))
                                        End If
                                        If SystemEMailID <> 0 Then
                                            Call cpCore.email.sendSystem_Legacy(cpCore.db.getRecordName("System Email", SystemEMailID), "", cpCore.doc.authContext.user.id)
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
                    returnHtml = getContentBoxWrapper(cpCore, returnHtml, cpCore.doc.page.ContentPadding)
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Set Headers
                    '---------------------------------------------------------------------------------
                    '
                    If DateModified <> Date.MinValue Then
                        Call cpCore.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified))
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Store page javascript
                    '---------------------------------------------------------------------------------
                    '
                    Call cpCore.html.addScriptCode_onLoad(cpCore.doc.page.JSOnLoad, "page content")
                    Call cpCore.html.addScriptCode_head(cpCore.doc.page.JSHead, "page content")
                    If cpCore.doc.page.JSFilename <> "" Then
                        Call cpCore.html.addScriptLink_Head(genericController.getCdnFileLink(cpCore, cpCore.doc.page.JSFilename), "page content")
                    End If
                    Call cpCore.html.addScriptCode_body(cpCore.doc.page.JSEndBody, "page content")
                    '
                    '---------------------------------------------------------------------------------
                    ' Set the Meta Content flag
                    '---------------------------------------------------------------------------------
                    '
                    Call cpCore.html.addTitle(genericController.encodeHTML(cpCore.doc.page.pageTitle), "page content")
                    Call cpCore.html.addMetaDescription(genericController.encodeHTML(cpCore.doc.page.metaDescription), "page content")
                    Call cpCore.html.addHeadTag(cpCore.doc.page.OtherHeadTags, "page content")
                    Call cpCore.html.addMetaKeywordList(cpCore.doc.page.MetaKeywordList, "page content")
                    '
                    Dim instanceArguments As New Dictionary(Of String, String)
                    instanceArguments.Add("CSPage", "-1")
                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {.instanceGuid = "-1", .instanceArguments = instanceArguments}
                    '
                    ' -- OnPageStartEvent
                    cpCore.doc.bodyContent = returnHtml
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageStart
                    Dim addonList As List(Of addonModel) = addonModel.createList_OnPageStartEvent(cpCore, New List(Of String))
                    For Each addon As addonModel In addonList
                        cpCore.doc.bodyContent = cpCore.addon.execute(addon, executeContext) & cpCore.doc.bodyContent
                        'AddonContent = cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    Next
                    returnHtml = cpCore.doc.bodyContent
                    '
                    ' -- OnPageEndEvent / filter
                    cpCore.doc.bodyContent = returnHtml
                    executeContext.addonType = CPUtilsBaseClass.addonContext.ContextOnPageEnd
                    For Each addon As addonModel In cpCore.addonCache.getOnPageEndAddonList
                        cpCore.doc.bodyContent &= cpCore.addon.execute(addon, executeContext)
                        'cpCore.doc.bodyContent &= cpCore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                    Next
                    returnHtml = cpCore.doc.bodyContent
                    '
                End If
                '
                ' -- title
                cpCore.html.addTitle(cpCore.doc.page.name)
                '
                ' -- add contentid and sectionid
                Call cpCore.html.addHeadTag("<meta name=""contentId"" content=""" & cpCore.doc.page.id & """ >", "page content")
                '
                ' Display Admin Warnings with Edits for record errors
                '
                If cpCore.doc.adminWarning <> "" Then
                    '
                    If cpCore.doc.adminWarningPageID <> 0 Then
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning & "</p>" & cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, True, "Page " & cpCore.doc.adminWarningPageID, cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
                        cpCore.doc.adminWarningPageID = 0
                    End If
                    returnHtml = "" _
                    & cpCore.html.html_GetAdminHintWrapper(cpCore.doc.adminWarning) _
                    & returnHtml _
                    & ""
                    cpCore.doc.adminWarning = ""
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' GetHtmlBody_GetSection_GetContentBox
        ''' </summary>
        ''' <param name="PageID"></param>
        ''' <param name="rootPageId"></param>
        ''' <param name="RootPageContentName"></param>
        ''' <param name="OrderByClause"></param>
        ''' <param name="AllowChildPageList"></param>
        ''' <param name="AllowReturnLink"></param>
        ''' <param name="ArchivePages"></param>
        ''' <param name="ignoreMe"></param>
        ''' <param name="UseContentWatchLink"></param>
        ''' <param name="allowPageWithoutSectionDisplay"></param>
        ''' <returns></returns>
        '
        Friend Shared Function getContentBox_content(cpcore As coreClass, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreMe As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim result As String = ""
            Try
                Dim isEditing As Boolean
                Dim LiveBody As String
                '
                If cpCore.doc.continueProcessing Then
                    If cpcore.doc.redirectLink = "" Then
                        isEditing = cpCore.doc.authContext.isEditing(pageContentModel.contentName)
                        '
                        ' ----- Render the Body
                        LiveBody = getContentBox_content_Body(cpcore, OrderByClause, AllowChildPageList, False, cpcore.doc.pageToRootList.Last.id, AllowReturnLink, pageContentModel.contentName, ArchivePages)
                        Dim isRootPage As Boolean = (cpcore.doc.pageToRootList.Count = 1)
                        If cpCore.doc.authContext.isAdvancedEditing(cpcore, "") Then
                            result = result & cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (Not isRootPage)) & LiveBody
                        ElseIf isEditing Then
                            result = result & cpcore.html.getEditWrapper("", cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, cpcore.doc.page.id, (Not isRootPage)) & LiveBody)
                        Else
                            result = result & LiveBody
                        End If
                    End If
                End If
                '
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' render the page content
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <param name="ContentID"></param>
        ''' <param name="OrderByClause"></param>
        ''' <param name="AllowChildList"></param>
        ''' <param name="Authoring"></param>
        ''' <param name="rootPageId"></param>
        ''' <param name="AllowReturnLink"></param>
        ''' <param name="RootPageContentName"></param>
        ''' <param name="ArchivePage"></param>
        ''' <returns></returns>

        Friend Shared Function getContentBox_content_Body(cpcore As coreClass, OrderByClause As String, AllowChildList As Boolean, Authoring As Boolean, rootPageId As Integer, AllowReturnLink As Boolean, RootPageContentName As String, ArchivePage As Boolean) As String
            Dim result As String = ""
            Try
                Dim allowChildListComposite As Boolean = AllowChildList And cpcore.doc.page.AllowChildListDisplay
                Dim allowReturnLinkComposite As Boolean = AllowReturnLink And cpcore.doc.page.AllowReturnLinkDisplay
                Dim bodyCopy As String = cpcore.doc.page.Copyfilename.content
                Dim breadCrumb As String = ""
                Dim BreadCrumbDelimiter As String
                Dim BreadCrumbPrefix As String
                Dim isRootPage As Boolean = cpcore.doc.pageToRootList.Count.Equals(1)
                '
                If allowReturnLinkComposite And (Not isRootPage) Then
                    '
                    ' ----- Print Heading if not at root Page
                    '
                    BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to")
                    BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ")
                    breadCrumb = cpcore.doc.getReturnBreadcrumb(RootPageContentName, cpcore.doc.page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter)
                    If breadCrumb <> "" Then
                        breadCrumb = cr & "<p class=""ccPageListNavigation"">" & BreadCrumbPrefix & " " & breadCrumb & "</p>"
                    End If
                End If
                result = result & breadCrumb
                '
                If (True) Then
                    Dim IconRow As String = ""
                    If (Not cpcore.doc.authContext.visit.Bot) And (cpcore.doc.page.AllowPrinterVersion Or cpcore.doc.page.AllowEmailPage) Then
                        '
                        ' not a bot, and either print or email allowed
                        '
                        If cpcore.doc.page.AllowPrinterVersion Then
                            Dim QueryString As String = cpcore.doc.refreshQueryString
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(cpcore.doc.page.id), True)
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            Dim Caption As String = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank""><img alt=""image"" src=""/ccLib/images/IconSmallPrinter.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank"" style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                        If cpcore.doc.page.AllowEmailPage Then
                            Dim QueryString As String = cpcore.doc.refreshQueryString
                            If QueryString <> "" Then
                                QueryString = "?" & QueryString
                            End If
                            Dim EmailBody As String = cpcore.webServer.requestProtocol & cpcore.webServer.requestDomain & cpcore.webServer.requestPathPage & QueryString
                            Dim Caption As String = cpcore.siteProperties.getText("PageAllowEmailCaption", "Email This Page")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """><img alt=""image"" src=""/ccLib/images/IconSmallEmail.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp;<a HREF=""mailto:?SUBJECT=You might be interested in this&amp;BODY=" & EmailBody & """ style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                    End If
                    If IconRow <> "" Then
                        result = result _
                        & cr & "<div style=""text-align:right;"">" _
                        & genericController.htmlIndent(IconRow) _
                        & cr & "</div>"
                    End If
                End If
                '
                ' ----- Start Text Search
                '
                Dim Cell As String = ""
                If cpCore.doc.authContext.isQuickEditing(cpcore, pageContentModel.contentName) Then
                    Cell = Cell & cpcore.doc.getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, cpcore.doc.page.ContactMemberID, cpcore.doc.page.ChildListSortMethodID, allowChildListComposite, ArchivePage)
                Else
                    '
                    ' ----- Headline
                    '
                    If cpcore.doc.page.Headline <> "" Then
                        Dim headline As String = cpcore.html.main_encodeHTML(cpcore.doc.page.Headline)
                        Cell = Cell & cr & "<h1>" & headline & "</h1>"
                        '
                        ' Add AC end here to force the end of any left over AC tags (like language)
                        Cell = Cell & ACTagEnd
                    End If
                    '
                    ' ----- Page Copy
                    If bodyCopy = "" Then
                        '
                        ' Page copy is empty if  Links Enabled put in a blank line to separate edit from add tag
                        If cpCore.doc.authContext.isEditing(pageContentModel.contentName) Then
                            bodyCopy = cr & "<p><!-- Empty Content Placeholder --></p>"
                        End If
                    Else
                        bodyCopy = bodyCopy & cr & ACTagEnd
                    End If
                    '
                    ' ----- Wrap content body
                    Cell = Cell _
                        & cr & "<!-- ContentBoxBodyStart -->" _
                        & genericController.htmlIndent(bodyCopy) _
                        & cr & "<!-- ContentBoxBodyEnd -->"
                    '
                    ' ----- Child pages
                    If allowChildListComposite Or cpCore.doc.authContext.isEditingAnything() Then
                        If Not allowChildListComposite Then
                            Cell = Cell & cpcore.html.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.")
                        End If
                        Dim AddonStatusOK As Boolean = False
                        Dim addon As addonModel = addonModel.create(cpcore, cpcore.siteProperties.childListAddonID)
                        Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                            .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                            .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                .contentName = pageContentModel.contentName,
                                .fieldName = "",
                                .recordId = cpcore.doc.page.id
                            },
                            .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, cpcore.doc.page.ChildListInstanceOptions),
                            .instanceGuid = PageChildListInstanceID,
                            .wrapperID = cpcore.siteProperties.defaultWrapperID
                        }
                        Cell &= cpcore.addon.execute(addon, executeContext)
                        'Cell = Cell & cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", cpcore.doc.page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, cpcore.doc.page.id, "", PageChildListInstanceID, False, cpcore.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
                    End If
                End If
                '
                ' ----- End Text Search
                result = result _
                    & cr & "<!-- TextSearchStart -->" _
                    & genericController.htmlIndent(Cell) _
                    & cr & "<!-- TextSearchEnd -->"
                '
                ' ----- Page See Also
                If cpcore.doc.page.AllowSeeAlso Then
                    result = result _
                        & cr & "<div>" _
                        & genericController.htmlIndent(getSeeAlso(cpcore, pageContentModel.contentName, cpcore.doc.page.id)) _
                        & cr & "</div>"
                End If
                '
                ' ----- Allow More Info
                If (cpcore.doc.page.ContactMemberID <> 0) And cpcore.doc.page.AllowMoreInfo Then
                    result = result & cr & "<ac TYPE=""" & ACTypeContact & """>"
                End If
                '
                ' ----- Feedback
                If (cpcore.doc.page.ContactMemberID <> 0) And cpcore.doc.page.AllowFeedback Then
                    result = result & cr & "<ac TYPE=""" & ACTypeFeedback & """>"
                End If
                '
                ' ----- Last Modified line
                If (cpcore.doc.page.ModifiedDate <> Date.MinValue) And cpcore.doc.page.AllowLastModifiedFooter Then
                    result = result & cr & "<p>This page was last modified " & FormatDateTime(cpcore.doc.page.ModifiedDate)
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                        If cpcore.doc.page.ModifiedBy = 0 Then
                            result = result & " (admin only: modified by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", cpcore.doc.page.ModifiedBy)
                            If personName = "" Then
                                result = result & " (admin only: modified by person with unnamed or deleted record #" & cpcore.doc.page.ModifiedBy & ")"
                            Else
                                result = result & " (admin only: modified by " & personName & ")"
                            End If
                        End If
                    End If
                    result = result & "</p>"
                End If
                '
                ' ----- Last Reviewed line
                If (cpcore.doc.page.DateReviewed <> Date.MinValue) And cpcore.doc.page.AllowReviewedFooter Then
                    result = result & cr & "<p>This page was last reviewed " & FormatDateTime(cpcore.doc.page.DateReviewed, vbLongDate)
                    If cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                        If cpcore.doc.page.ReviewedBy = 0 Then
                            result = result & " (by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", cpcore.doc.page.ReviewedBy)
                            If personName = "" Then
                                result = result & " (by person with unnamed or deleted record #" & cpcore.doc.page.ReviewedBy & ")"
                            Else
                                result = result & " (by " & personName & ")"
                            End If
                        End If
                        result = result & ".</p>"
                    End If
                End If
                '
                ' ----- Page Content Message Footer
                If cpcore.doc.page.AllowMessageFooter Then
                    Dim pageContentMessageFooter As String = cpcore.siteProperties.getText("PageContentMessageFooter", "")
                    If (pageContentMessageFooter <> "") Then
                        result = result & cr & "<p>" & pageContentMessageFooter & "</p>"
                    End If
                End If
                'Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '=============================================================================
        ' Print the See Also listing
        '   ContentName is the name of the parent table
        '   RecordID is the parent RecordID
        '=============================================================================
        '
        Public Shared Function getSeeAlso(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer) As String
            Dim result As String = ""
            Try
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
                    ContentID = models.complex.cdefmodel.getcontentid(cpcore, iContentName)
                    If (ContentID > 0) Then
                        '
                        ' ----- Set authoring only for valid ContentName
                        '
                        IsEditingLocal = cpCore.doc.authContext.isEditing(iContentName)
                    Else
                        '
                        ' ----- if iContentName was bad, maybe they put table in, no authoring
                        '
                        ContentID = Models.Complex.cdefModel.getContentIDByTablename(cpcore, iContentName)
                    End If
                    If (ContentID > 0) Then
                        '
                        CS = cpcore.db.csOpen("See Also", "((active<>0)AND(ContentID=" & ContentID & ")AND(RecordID=" & iRecordID & "))")
                        Do While (cpcore.db.csOk(CS))
                            SeeAlsoLink = (cpcore.db.csGetText(CS, "Link"))
                            If SeeAlsoLink <> "" Then
                                result = result & cr & "<li class=""ccListItem"">"
                                If genericController.vbInstr(1, SeeAlsoLink, "://") = 0 Then
                                    SeeAlsoLink = cpcore.webServer.requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & cpcore.html.main_GetRecordEditLink2("See Also", (cpcore.db.csGetInteger(CS, "ID")), False, "", cpCore.doc.authContext.isEditing("See Also"))
                                End If
                                result = result & "<a href=""" & genericController.encodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.csGetText(CS, "Name")) & "</A>"
                                Copy = (cpcore.db.csGetText(CS, "Brief"))
                                If Copy <> "" Then
                                    result = result & "<br>" & genericController.AddSpan(Copy, "ccListCopy")
                                End If
                                SeeAlsoCount = SeeAlsoCount + 1
                                result = result & "</li>"
                            End If
                            cpcore.db.csGoNext(CS)
                        Loop
                        cpcore.db.csClose(CS)
                        '
                        If IsEditingLocal Then
                            SeeAlsoCount = SeeAlsoCount + 1
                            result = result & cr & "<li class=""ccListItem"">" & cpcore.html.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
                        End If
                    End If
                    '
                    If SeeAlsoCount = 0 Then
                        result = ""
                    Else
                        result = "<p>See Also" & cr & "<ul class=""ccList"">" & genericController.htmlIndent(result) & cr & "</ul></p>"
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
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
                main_GetMoreInfo = getMoreInfoHtml(cpcore, genericController.EncodeInteger(contactMemberID))
            Catch ex As Exception
                cpcore.handleException(ex)
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
                Dim NoteCopy As String = String.Empty
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
                iHeadline = genericController.encodeEmptyText(headline, "")
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
                            NoteCopy = NoteCopy & cpcore.html.convertCRLFToHtmlBreak(Copy) & BR
                        End If
                        '
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Content on which the comments are based</b>" & BR
                        '
                        CS = cpcore.db.csOpen(iContentName, "ID=" & iRecordID)
                        Copy = "[the content of this page is not available]" & BR
                        If cpcore.db.csOk(CS) Then
                            Copy = (cpcore.db.csGet(CS, "copyFilename"))
                            'Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        End If
                        NoteCopy = NoteCopy & Copy & BR
                        Call cpcore.db.csClose(CS)
                        '
                        Call cpcore.email.sendPerson(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, False, True, 0, "", False)
                        '
                        ' ----- Note sent, say thanks
                        '
                        result = result & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & cpcore.webServer.serverFormActionURL & "?" & cpcore.doc.refreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = cpCore.doc.authContext.user.name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = cpCore.doc.authContext.user.Email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & cpcore.html.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
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
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Returns the HTML body
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Shared Function getHtmlBodyTemplate(cpCore As coreClass) As String
            Dim returnBody As String = ""
            Try
                Dim AddonReturn As String
                Dim layoutError As String = String.Empty
                Dim Result As New stringBuilderLegacyController
                Dim PageContent As String
                Dim Stream As New stringBuilderLegacyController
                Dim LocalTemplateID As Integer
                Dim LocalTemplateName As String
                Dim LocalTemplateBody As String
                Dim blockSiteWithLogin As Boolean
                '
                ' -- OnBodyStart add-ons
                Dim bodyStartContext As New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextOnBodyStart}
                For Each addon As addonModel In cpCore.addonCache.getOnBodyStartAddonList
                    returnBody &= cpCore.addon.execute(addon, bodyStartContext)
                    'returnBody &= cpCore.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextOnBodyStart, "", 0, "", "", False, 0, "", False, Nothing)
                Next
                '
                ' ----- main_Get Content (Already Encoded)
                '
                blockSiteWithLogin = False
                PageContent = getHtmlBodyTemplateContent(cpCore, True, True, False, blockSiteWithLogin)
                If blockSiteWithLogin Then
                    '
                    ' section blocked, just return the login box in the page content
                    '
                    returnBody = "" _
                        & cr & "<div class=""ccLoginPageCon"">" _
                        & genericController.htmlIndent(PageContent) _
                        & cr & "</div>" _
                        & ""
                ElseIf Not cpCore.doc.continueProcessing Then
                    '
                    ' exit if stream closed during main_GetSectionpage
                    '
                    returnBody = ""
                Else
                    '
                    ' -- no section block, continue
                    LocalTemplateID = cpCore.doc.template.ID
                    LocalTemplateBody = cpCore.doc.template.BodyHTML
                    If LocalTemplateBody = "" Then
                        LocalTemplateBody = TemplateDefaultBody
                    End If
                    LocalTemplateName = cpCore.doc.template.Name
                    If LocalTemplateName = "" Then
                        LocalTemplateName = "Template " & LocalTemplateID
                    End If
                    '
                    ' ----- Encode Template
                    '
                    LocalTemplateBody = cpCore.html.executeContentCommands(Nothing, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError)
                    returnBody = returnBody & cpCore.html.convertActiveContentToHtmlForWebRender(LocalTemplateBody, "Page Templates", LocalTemplateID, 0, cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextTemplate)
                    '
                    ' If Content was not found, add it to the end
                    '
                    If (InStr(1, returnBody, fpoContentBox) <> 0) Then
                        returnBody = genericController.vbReplace(returnBody, fpoContentBox, PageContent)
                    Else
                        returnBody = returnBody & PageContent
                    End If
                    '
                    ' ----- Add tools Panel
                    '
                    If Not cpCore.doc.authContext.isAuthenticated() Then
                        '
                        ' not logged in
                        '
                    Else
                        '
                        ' Add template editing
                        '
                        If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") And cpCore.doc.authContext.isEditing("Page Templates") Then
                            returnBody = cpCore.html.getEditWrapper("Page Template [" & LocalTemplateName & "]", cpCore.html.main_GetRecordEditLink2("Page Templates", LocalTemplateID, False, LocalTemplateName, cpCore.doc.authContext.isEditing("Page Templates")) & returnBody)
                        End If
                    End If
                    '
                    ' ----- OnBodyEnd add-ons
                    '
                    Dim bodyEndContext As New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextFilter}
                    For Each addon In cpCore.addonCache.getOnBodyEndAddonList()
                        cpCore.doc.docBodyFilter = returnBody
                        AddonReturn = cpCore.addon.execute(addon, bodyEndContext)
                        'AddonReturn = cpCore.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", False, Nothing)
                        returnBody = cpCore.doc.docBodyFilter & AddonReturn
                    Next
                    '
                    ' -- Make it pretty for those who care
                    returnBody = htmlReflowController.reflow(cpCore, returnBody)
                End If
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnBody
        End Function
        '
        '=============================================================================
        '   main_Get Section
        '       Two modes
        '           pre 3.3.613 - SectionName = RootPageName
        '           else - (IsSectionRootPageIDMode) SectionRecord has a RootPageID field
        '=============================================================================
        '
        Public Shared Function getHtmlBodyTemplateContent(cpCore As coreClass, AllowChildPageList As Boolean, AllowReturnLink As Boolean, AllowEditWrapper As Boolean, ByRef return_blockSiteWithLogin As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim allowPageWithoutSectionDislay As Boolean
                Dim FieldRows As Integer
                'Dim templateId As Integer
                Dim RootPageContentName As String
                Dim PageID As Integer
                Dim UseContentWatchLink As Boolean = cpCore.siteProperties.useContentWatchLink
                '
                RootPageContentName = "Page Content"
                '
                ' -- validate domain
                'domain = domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
                If (cpCore.doc.domain Is Nothing) Then
                    '
                    ' -- domain not listed, this is now an error
                    Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">The domain name is not configured for this site.</div>"
                End If
                '
                ' -- validate page
                If cpCore.doc.page.id = 0 Then
                    '
                    ' -- landing page is not valid -- display error
                    Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                    cpCore.doc.redirectBecausePageNotFound = True
                    cpCore.doc.redirectReason = "Redirecting because the page selected could not be found."
                    cpCore.doc.redirectLink = pageContentController.main_ProcessPageNotFound_GetLink(cpCore, cpCore.doc.redirectReason, , , PageID, 0)
                    cpCore.handleException(New ApplicationException("Page could not be determined. Error message displayed."))
                    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                End If
                'PageID = cpCore.docProperties.getInteger(rnPageId)
                'If (PageID = 0) Then
                '    '
                '    ' -- Nothing specified, use the Landing Page
                '    PageID = main_GetLandingPageID()
                '    If PageID = 0 Then
                '        '
                '        ' -- landing page is not valid -- display error
                '        Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                '        RedirectBecausePageNotFound = True
                '        RedirectReason = "Redirecting because the page selected could not be found."
                '        redirectLink = main_ProcessPageNotFound_GetLink(RedirectReason, , , PageID, 0)

                '        cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                '        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                '    End If
                'End If
                'Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(PageID))
                'templateReason = "The reason this template was selected could not be determined."
                ''
                '' -- build parentpageList (first = current page, last = root)
                '' -- add a 0, then repeat until another 0 is found, or there is a repeat
                'pageToRootList = New List(Of pageContentModel)()
                'Dim usedPageIdList As New List(Of Integer)()
                'Dim targetPageId = PageID
                'usedPageIdList.Add(0)
                'Do While (Not usedPageIdList.Contains(targetPageId))
                '    usedPageIdList.Add(targetPageId)
                '    Dim targetpage As pageContentModel = pageContentModel.create(cpCore, targetPageId, New List(Of String))
                '    If (targetpage Is Nothing) Then
                '        Exit Do
                '    Else
                '        pageToRootList.Add(targetpage)
                '        targetPageId = targetpage.id
                '    End If
                'Loop
                'If (pageToRootList.Count = 0) Then
                '    '
                '    ' -- page is not valid -- display error
                '    cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                '    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                'End If
                'page = pageToRootList.First
                ''
                '' -- get template from pages
                'Dim template As pageTemplateModel = Nothing
                'For Each page As pageContentModel In pageToRootList
                '    If page.TemplateID > 0 Then
                '        template = pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
                '        If (template IsNot Nothing) Then
                '            If (page Is page) Then
                '                templateReason = "This template was used because it is selected by the current page."
                '            Else
                '                templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
                '            End If
                '            Exit For
                '        End If
                '    End If
                'Next
                ''
                'If (template Is Nothing) Then
                '    '
                '    ' -- get template from domain
                '    If (domain IsNot Nothing) Then
                '        template = pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
                '    End If
                '    If (template Is Nothing) Then
                '        '
                '        ' -- get template named Default
                '        template = pageTemplateModel.createByName(cpCore, "default", New List(Of String))
                '    End If
                'End If
                '
                ' -- get contentbox
                returnHtml = getContentBox(cpCore, "", AllowChildPageList, AllowReturnLink, False, 0, UseContentWatchLink, allowPageWithoutSectionDislay)
                '
                ' -- if fpo_QuickEdit it there, replace it out
                Dim Editor As String
                Dim styleOptionList As String = String.Empty
                Dim addonListJSON As String
                If cpCore.doc.redirectLink = "" And (InStr(1, returnHtml, html_quickEdit_fpo) <> 0) Then
                    FieldRows = genericController.EncodeInteger(cpCore.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"))
                    If FieldRows < 50 Then
                        FieldRows = 50
                        Call cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50)
                    End If
                    Dim stylesheetCommaList As String = "" 'cpCore.html.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                    addonListJSON = cpCore.html.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
                    Editor = cpCore.html.getFormInputHTML("copyFilename", cpCore.doc.quickEditCopy, CStr(FieldRows), "100%", False, True, addonListJSON, stylesheetCommaList, styleOptionList)
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor)
                End If
                '
                ' -- Add admin warning to the top of the content
                If cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) And cpCore.doc.adminWarning <> "" Then
                    '
                    ' Display Admin Warnings with Edits for record errors
                    '
                    If cpCore.doc.adminWarningPageID <> 0 Then
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning & "</p>" & cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, True, "Page " & cpCore.doc.adminWarningPageID, cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
                        cpCore.doc.adminWarningPageID = 0
                    End If
                    '
                    returnHtml = "" _
                    & cpCore.html.html_GetAdminHintWrapper(cpCore.doc.adminWarning) _
                    & returnHtml _
                    & ""
                    cpCore.doc.adminWarning = ""
                End If
                '
                ' -- handle redirect and edit wrapper
                '------------------------------------------------------------------------------------
                '
                If cpCore.doc.redirectLink <> "" Then
                    Return cpCore.webServer.redirect(cpCore.doc.redirectLink, cpCore.doc.redirectReason, cpCore.doc.redirectBecausePageNotFound)
                ElseIf AllowEditWrapper Then
                    returnHtml = cpCore.html.getEditWrapper("Page Content", returnHtml)
                End If
                '
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        '   Add content padding around content
        '       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
        '=============================================================================
        '
        Friend Shared Function getContentBoxWrapper(cpcore As coreClass, ByVal Content As String, ByVal ContentPadding As Integer) As String
            'dim buildversion As String
            '
            ' BuildVersion = app.dataBuildVersion
            getContentBoxWrapper = Content
            If cpcore.siteProperties.getBoolean("Compatibility ContentBox Pad With Table") Then
                '
                If ContentPadding > 0 Then
                    '
                    getContentBoxWrapper = "" _
                        & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0>" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td style=""padding:" & ContentPadding & "px"">" _
                        & genericController.htmlIndent(genericController.htmlIndent(genericController.htmlIndent(getContentBoxWrapper))) _
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
                getContentBoxWrapper = "" _
                    & cr & "<div class=""contentBox"">" _
                    & genericController.htmlIndent(getContentBoxWrapper) _
                    & cr & "</div>"
            Else
                '
                getContentBoxWrapper = "" _
                    & cr & "<div class=""contentBox"" style=""padding:" & ContentPadding & "px"">" _
                    & genericController.htmlIndent(getContentBoxWrapper) _
                    & cr & "</div>"
            End If
        End Function
        '
        '
        '
        Friend Shared Function getDefaultBlockMessage(cpcore As coreClass, UseContentWatchLink As Boolean) As String
            getDefaultBlockMessage = ""
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetDefaultBlockMessage")
            '
            Dim CS As Integer
            Dim PageID As Integer
            '
            CS = cpcore.db.csOpen("Copy Content", "name=" & cpcore.db.encodeSQLText(ContentBlockCopyName), "ID", , , , , "Copy,ID")
            If cpcore.db.csOk(CS) Then
                getDefaultBlockMessage = cpcore.db.csGet(CS, "Copy")
            End If
            Call cpcore.db.csClose(CS)
            '
            ' ----- Do not allow blank message - if still nothing, create default
            '
            If getDefaultBlockMessage = "" Then
                getDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
            End If
            '
            ' ----- Create Copy Content Record for future
            '
            CS = cpcore.db.csInsertRecord("Copy Content")
            If cpcore.db.csOk(CS) Then
                Call cpcore.db.csSet(CS, "Name", ContentBlockCopyName)
                Call cpcore.db.csSet(CS, "Copy", getDefaultBlockMessage)
                Call cpcore.db.csSave2(CS)
                'Call cpcore.workflow.publishEdit("Copy Content", genericController.EncodeInteger(cpcore.db.cs_get(CS, "ID")))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetDefaultBlockMessage")
        End Function
        '
        '
        '
        '
        Public Shared Function getMoreInfoHtml(cpCore As coreClass, ByVal PeopleID As Integer) As String
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
                If cpCore.db.csOk(CS) Then
                    ContactName = (cpCore.db.csGetText(CS, "Name"))
                    ContactPhone = (cpCore.db.csGetText(CS, "Phone"))
                    ContactEmail = (cpCore.db.csGetText(CS, "Email"))
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
                Call cpCore.db.csClose(CS)
                '
                result = Copy
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Friend Shared Function getTableRow(ByVal Caption As String, ByVal Result As String, ByVal EvenRow As Boolean) As String
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
            getTableRow = genericController.GetTableCell("<nobr>" & CopyCaption & "</nobr>", "150", , EvenRow, "right") & genericController.GetTableCell(CopyResult, "100%", , EvenRow, "left") & kmaEndTableRow
        End Function
        '
        Public Shared Sub loadPage(cpcore As coreClass, requestedPageId As Integer, domain As domainModel)
            Try
                If (domain Is Nothing) Then
                    '
                    ' -- domain is not valid
                    cpcore.handleException(New ApplicationException("Page could not be determined because the domain was not recognized."))
                Else
                    '
                    ' -- attempt requested page
                    Dim requestedPage As pageContentModel = Nothing
                    If (Not requestedPageId.Equals(0)) Then
                        requestedPage = pageContentModel.create(cpcore, requestedPageId)
                        If (requestedPage Is Nothing) Then
                            '
                            ' -- requested page not found
                            requestedPage = pageContentModel.create(cpcore, getPageNotFoundPageId(cpcore))
                        End If
                    End If
                    If (requestedPage Is Nothing) Then
                        '
                        ' -- use the Landing Page
                        requestedPage = getLandingPage(cpcore, domain)
                    End If
                    Call cpcore.doc.addRefreshQueryString(rnPageId, CStr(requestedPage.id))
                    '
                    ' -- build parentpageList (first = current page, last = root)
                    ' -- add a 0, then repeat until another 0 is found, or there is a repeat
                    cpcore.doc.pageToRootList = New List(Of pageContentModel)()
                    Dim usedPageIdList As New List(Of Integer)()
                    Dim targetPageId = requestedPage.id
                    usedPageIdList.Add(0)
                    Do While (Not usedPageIdList.Contains(targetPageId))
                        usedPageIdList.Add(targetPageId)
                        Dim targetpage As pageContentModel = pageContentModel.create(cpcore, targetPageId, New List(Of String))
                        If (targetpage Is Nothing) Then
                            Exit Do
                        Else
                            cpcore.doc.pageToRootList.Add(targetpage)
                            targetPageId = targetpage.ParentID
                        End If
                    Loop
                    If (cpcore.doc.pageToRootList.Count = 0) Then
                        '
                        ' -- attempt failed, create default page
                        cpcore.doc.page = pageContentModel.add(cpcore)
                        cpcore.doc.page.name = DefaultNewLandingPageName & ", " & domain.name
                        cpcore.doc.page.Copyfilename.content = landingPageDefaultHtml
                        cpcore.doc.page.save(cpcore)
                        cpcore.doc.pageToRootList.Add(cpcore.doc.page)
                    Else
                        cpcore.doc.page = cpcore.doc.pageToRootList.First
                    End If
                    '
                    ' -- get template from pages
                    cpcore.doc.template = Nothing
                    For Each page As pageContentModel In cpcore.doc.pageToRootList
                        If page.TemplateID > 0 Then
                            cpcore.doc.template = pageTemplateModel.create(cpcore, page.TemplateID, New List(Of String))
                            If (cpcore.doc.template Is Nothing) Then
                                '
                                ' -- templateId is not valid
                                page.TemplateID = 0
                                page.save(cpcore)
                            Else
                                If (page Is cpcore.doc.pageToRootList.First) Then
                                    cpcore.doc.templateReason = "This template was used because it is selected by the current page."
                                Else
                                    cpcore.doc.templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
                                End If
                                Exit For
                            End If
                        End If
                    Next
                    '
                    If (cpcore.doc.template Is Nothing) Then
                        '
                        ' -- get template from domain
                        If (domain IsNot Nothing) Then
                            If (domain.DefaultTemplateId > 0) Then
                                cpcore.doc.template = pageTemplateModel.create(cpcore, domain.DefaultTemplateId, New List(Of String))
                                If (cpcore.doc.template Is Nothing) Then
                                    '
                                    ' -- domain templateId is not valid
                                    domain.DefaultTemplateId = 0
                                    domain.save(cpcore)
                                End If
                            End If
                        End If
                        If (cpcore.doc.template Is Nothing) Then
                            '
                            ' -- get template named Default
                            cpcore.doc.template = pageTemplateModel.createByName(cpcore, defaultTemplateName, New List(Of String))
                            If (cpcore.doc.template Is Nothing) Then
                                '
                                ' -- ceate new template named Default
                                cpcore.doc.template = pageTemplateModel.add(cpcore, New List(Of String))
                                cpcore.doc.template.Name = defaultTemplateName
                                cpcore.doc.template.BodyHTML = cpcore.appRootFiles.readFile(defaultTemplateHomeFilename)
                                cpcore.doc.template.save(cpcore)
                            End If
                            '
                            ' -- set this new template to all domains without a template
                            For Each d As domainModel In domainModel.createList(cpcore, "((DefaultTemplateId=0)or(DefaultTemplateId is null))")
                                d.DefaultTemplateId = cpcore.doc.template.ID
                                d.save(cpcore)
                            Next
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
        End Sub
        Public Shared Function getPageNotFoundPageId(cpcore As coreClass) As Integer
            Dim pageId As Integer
            Try
                pageId = cpcore.domainLegacyCache.domainDetails.pageNotFoundPageId
                If pageId = 0 Then
                    '
                    ' no domain page not found, use site default
                    '
                    pageId = cpcore.siteProperties.getinteger("PageNotFoundPageID", 0)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return pageId
        End Function
        '
        '---------------------------------------------------------------------------
        '
        '---------------------------------------------------------------------------
        '
        Public Shared Function getLandingPage(cpcore As coreClass, domain As domainModel) As pageContentModel
            Dim landingPage As pageContentModel = Nothing
            Try
                If (domain Is Nothing) Then
                    '
                    ' -- domain not available
                    cpcore.handleException(New ApplicationException("Landing page could not be determined because the domain was not recognized."))
                Else
                    '
                    ' -- attempt domain landing page
                    If (Not domain.RootPageID.Equals(0)) Then
                        landingPage = pageContentModel.create(cpcore, domain.RootPageID)
                        If (landingPage Is Nothing) Then
                            domain.RootPageID = 0
                            domain.save(cpcore)
                        End If
                    End If
                    If (landingPage Is Nothing) Then
                        '
                        ' -- attempt site landing page
                        Dim siteLandingPageID As Integer = cpcore.siteProperties.getinteger("LandingPageID", 0)
                        If (Not siteLandingPageID.Equals(0)) Then
                            landingPage = pageContentModel.create(cpcore, siteLandingPageID)
                            If (landingPage Is Nothing) Then
                                cpcore.siteProperties.setProperty("LandingPageID", 0)
                                domain.RootPageID = 0
                                domain.save(cpcore)
                            End If
                        End If
                        If (landingPage Is Nothing) Then
                            '
                            ' -- create detault landing page
                            landingPage = pageContentModel.add(cpcore)
                            landingPage.name = DefaultNewLandingPageName & ", " & domain.name
                            landingPage.Copyfilename.content = landingPageDefaultHtml
                            landingPage.save(cpcore)
                            cpcore.doc.landingPageID = landingPage.id
                        End If
                        '
                        ' -- save new page to the domain
                        domain.RootPageID = landingPage.id
                        domain.save(cpcore)
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return landingPage
        End Function
        '
        ' Verify a link from the template link field to be used as a Template Link
        '
        Friend Shared Function verifyTemplateLink(cpcore As coreClass, ByVal linkSrc As String) As String
            '
            '
            ' ----- Check Link Format
            '
            verifyTemplateLink = linkSrc
            If verifyTemplateLink <> "" Then
                If genericController.vbInstr(1, verifyTemplateLink, "://") <> 0 Then
                    '
                    ' protocol provided, do not fixup
                    '
                    verifyTemplateLink = genericController.EncodeAppRootPath(verifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
                Else
                    '
                    ' no protocol, convert to short link
                    '
                    If Left(verifyTemplateLink, 1) <> "/" Then
                        '
                        ' page entered without path, assume it is in root path
                        '
                        verifyTemplateLink = "/" & verifyTemplateLink
                    End If
                    verifyTemplateLink = genericController.ConvertLinkToShortLink(verifyTemplateLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath)
                    verifyTemplateLink = genericController.EncodeAppRootPath(verifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
                End If
            End If
        End Function
        '
        '
        '
        Friend Shared Function main_ProcessPageNotFound_GetLink(cpcore As coreClass, ByVal adminMessage As String, Optional ByVal BackupPageNotFoundLink As String = "", Optional ByVal PageNotFoundLink As String = "", Optional ByVal EditPageID As Integer = 0, Optional ByVal EditSectionID As Integer = 0) As String
            Dim result As String = String.Empty
            Try
                Dim Pos As Integer
                Dim DefaultLink As String
                Dim PageNotFoundPageID As Integer
                Dim Link As String
                '
                PageNotFoundPageID = getPageNotFoundPageId(cpcore)
                If PageNotFoundPageID = 0 Then
                    '
                    ' No PageNotFound was set -- use the backup link
                    '
                    If BackupPageNotFoundLink = "" Then
                        adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used."
                        Link = cpcore.doc.landingLink
                    Else
                        adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set."
                        Link = BackupPageNotFoundLink
                    End If
                Else
                    '
                    ' Set link
                    '
                    Link = getPageLink(cpcore, PageNotFoundPageID, "", True, False)
                    DefaultLink = getPageLink(cpcore, 0, "", True, False)
                    If Link <> DefaultLink Then
                    Else
                        adminMessage = adminMessage & "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used."
                    End If
                End If
                '
                ' Add the Admin Message to the link
                '
                If cpcore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                    If PageNotFoundLink = "" Then
                        PageNotFoundLink = cpcore.webServer.requestUrl
                    End If
                    '
                    ' Add the Link to the Admin Msg
                    '
                    adminMessage = adminMessage & "<p>The URL was " & PageNotFoundLink & "."
                    '
                    ' Add the Referrer to the Admin Msg
                    '
                    If cpcore.webServer.requestReferer <> "" Then
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningPageID=", vbTextCompare)
                        If Pos <> 0 Then
                            cpcore.webServer.requestReferrer = Left(cpcore.webServer.requestReferrer, Pos - 2)
                        End If
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningMsg=", vbTextCompare)
                        If Pos <> 0 Then
                            cpcore.webServer.requestReferrer = Left(cpcore.webServer.requestReferrer, Pos - 2)
                        End If
                        Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "blockcontenttracking=", vbTextCompare)
                        If Pos <> 0 Then
                            cpcore.webServer.requestReferrer = Left(cpcore.webServer.requestReferrer, Pos - 2)
                        End If
                        adminMessage = adminMessage & " The referring page was " & cpcore.webServer.requestReferrer & "."
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
                result = Link
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Shared Function getPageLink(cpcore As coreClass, ByVal PageID As Integer, ByVal QueryStringSuffix As String, Optional ByVal AllowLinkAliasIfEnabled As Boolean = True, Optional ByVal UseContentWatchNotDefaultPage As Boolean = False) As String
            Dim result As String = ""
            Try
                Dim linkPathPage As String = Nothing
                '
                ' -- set linkPathPath to linkAlias
                If AllowLinkAliasIfEnabled And cpcore.siteProperties.allowLinkAlias Then
                    linkPathPage = docController.getLinkAlias(cpcore, PageID, QueryStringSuffix, "")
                End If
                If (String.IsNullOrEmpty(linkPathPage)) Then
                    '
                    ' -- if not linkAlis, set default page and qs
                    linkPathPage = cpcore.siteProperties.serverPageDefault
                    If String.IsNullOrEmpty(linkPathPage) Then
                        linkPathPage = "/" & getDefaultScript()
                    Else
                        Dim Pos As Integer = genericController.vbInstr(1, linkPathPage, "?")
                        If Pos <> 0 Then
                            linkPathPage = Mid(linkPathPage, 1, Pos - 1)
                        End If
                        If Left(linkPathPage, 1) <> "/" Then
                            linkPathPage = "/" & linkPathPage
                        End If
                    End If
                    '
                    ' -- calc linkQS (cleared in come cases later)
                    linkPathPage &= "?" & rnPageId & "=" & PageID
                    If QueryStringSuffix <> "" Then
                        linkPathPage &= "&" & QueryStringSuffix
                    End If
                End If
                '
                ' -- domain -- determine if the domain has any template requirements, and if so, is this template allowed
                Dim SqlCriteria As String = "(domainId=" & cpcore.doc.domain.id & ")"
                Dim allowTemplateRuleList As List(Of TemplateDomainRuleModel) = TemplateDomainRuleModel.createList(cpcore, SqlCriteria)
                Dim templateAllowed As Boolean = False
                For Each rule As TemplateDomainRuleModel In allowTemplateRuleList
                    If (rule.templateId = cpcore.doc.template.ID) Then
                        templateAllowed = True
                        Exit For
                    End If
                Next
                Dim linkDomain As String = ""
                If (allowTemplateRuleList.Count = 0) Then
                    '
                    ' this template has no domain preference, use current domain
                    '
                    linkDomain = cpcore.webServer.requestDomain
                ElseIf (cpcore.domainLegacyCache.domainDetails.id = 0) Then
                    '
                    ' the current domain is not recognized, or is default - use it
                    '
                    linkDomain = cpcore.webServer.requestDomain
                ElseIf (templateAllowed) Then
                    '
                    ' current domain is in the allowed domain list
                    '
                    linkDomain = cpcore.webServer.requestDomain
                Else
                    '
                    ' there is an allowed domain list and current domain is not on it, or use first
                    '
                    Dim setdomainId As Integer = allowTemplateRuleList.First.domainId
                    linkDomain = cpcore.db.getRecordName("domains", setdomainId)
                    If linkDomain = "" Then
                        linkDomain = cpcore.webServer.requestDomain
                    End If
                End If
                '
                ' -- protocol
                Dim linkprotocol As String = ""
                If cpcore.doc.page.IsSecure Or cpcore.doc.template.IsSecure Then
                    linkprotocol = "https://"
                Else
                    linkprotocol = "http://"
                End If
                '
                ' -- assemble
                result = linkprotocol & linkDomain & linkPathPage
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        ' main_Get a page link if you know nothing about the page
        '   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        '====================================================================================================
        '
        Public Shared Function main_GetPageLink3(cpcore As coreClass, ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal AllowLinkAlias As Boolean) As String
            main_GetPageLink3 = getPageLink(cpcore, PageID, QueryStringSuffix, AllowLinkAlias, False)
        End Function
        '
        '
        Friend Shared Function getDefaultScript() As String
            Return "default.aspx"
        End Function
        '
        '========================================================================
        '   main_IsChildRecord
        '
        '   Tests if this record is in the ParentID->ID chain for this content
        '========================================================================
        '
        Public Shared Function isChildRecord(cpcore As coreClass, ByVal ContentName As String, ByVal ChildRecordID As Integer, ByVal ParentRecordID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsChildRecord")
            '
            Dim CDef As Models.Complex.cdefModel
            '
            isChildRecord = (ChildRecordID = ParentRecordID)
            If Not isChildRecord Then
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
                If genericController.IsInDelimitedString(UCase(CDef.SelectCommaList), "PARENTID", ",") Then
                    isChildRecord = main_IsChildRecord_Recurse(cpcore, CDef.ContentDataSourceName, CDef.ContentTableName, ChildRecordID, ParentRecordID, "")
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("cpCoreClass.IsChildRecord")
            '
        End Function
        '
        '========================================================================
        '   main_IsChildRecord
        '
        '   Tests if this record is in the ParentID->ID chain for this content
        '========================================================================
        '
        Friend Shared Function main_IsChildRecord_Recurse(cpcore As coreClass, ByVal DataSourceName As String, ByVal TableName As String, ByVal ChildRecordID As Integer, ByVal ParentRecordID As Integer, ByVal History As String) As Boolean
            Dim result As Boolean = False
            Try
                Dim SQL As String
                Dim CS As Integer
                Dim ChildRecordParentID As Integer
                '
                SQL = "select ParentID from " & TableName & " where id=" & ChildRecordID
                CS = cpcore.db.csOpenSql(SQL)
                If cpcore.db.csOk(CS) Then
                    ChildRecordParentID = cpcore.db.csGetInteger(CS, "ParentID")
                End If
                Call cpcore.db.csClose(CS)
                If (ChildRecordParentID <> 0) And (Not genericController.IsInDelimitedString(History, CStr(ChildRecordID), ",")) Then
                    result = (ParentRecordID = ChildRecordParentID)
                    If Not result Then
                        result = main_IsChildRecord_Recurse(cpcore, DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History & "," & CStr(ChildRecordID))
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
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