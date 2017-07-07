﻿
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
    Public Class pagesController
        '
        ' -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
        Private cpcore As coreClass
        '
        Public domain As Models.Entity.domainModel
        Public page As Models.Entity.pageContentModel
        Public pageToRootList As List(Of Models.Entity.pageContentModel)
        Public template As Models.Entity.pageTemplateModel
        Public templateReason As String = ""                           ' a message that explains why this template was selected
        '
        Public siteStructure As String = ""
        Public siteStructure_LocalLoaded As Boolean = False
        Public Const navStruc_Descriptor = 1           ' Descriptors:0 = RootPage, 1 = Parent Page, 2 = Current Page, 3 = Child Page
        Public Const navStruc_Descriptor_CurrentPage = 2
        Public Const navStruc_Descriptor_ChildPage = 3
        Public Const navStruc_TemplateId = 7
        Public bodyContent As String = ""                    ' The page's content at the OnPageEndEvent - used to let OnPageStart and OnPageEnd Add-ons change the content
        'Public pageManager_ContentPageStructure As String = ""           ' deprecated name for currentNavigationStructure
        Public landingPageID As Integer = 0                              ' Set from Site Property (use main_GetLandingPageID)
        Public landingPageID_Loaded As Boolean = False                   '   true when pageManager_LandingPageID is loaded
        Public landingPageName As String = ""                          ' Set from pageManager_LandingPageID (use main_GetLandingPageName)
        Public landingLink As String = ""                              ' Default Landing page - managed through main_GetLandingLink()
        'Public templateName As String = ""                             ' Name of the template
        'Public templateLink As String = ""                             ' Link this template requires - redirect caused if this is not the current link
        'Public templateBody As String = ""                             ' Body field of the TemplateID
        'Public templateBodyTag As String = ""                          ' BodyTag - from Template record, if blank, use TemplateDefaultBodyTag
        Public sectionMenuLinkDefault As String = ""
        Public Const blockMessageDefault = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
        Public redirectLink As String = ""                             ' If there is a problem, a redirect is forced
        Public pageManager_RedirectReason As String = ""                           ' reason for redirect
        Public pageManager_RedirectBecausePageNotFound As Boolean = False            ' if true, redirect will go through a 404 so bots will not follow
        Public pageManager_RedirectSourcePageID As Integer = 0                       ' the pageid of the page with this issue, 0 if not a page problem
        '
        'Public currentPageID As Integer = 0                   ' The current page's id
        'Public currentPageName As String = ""               ' The current page's name
        'Public currentSectionID As Integer = 0                ' The current page's section id
        'Public currentSectionName As String = ""            ' The current Section's name
        'Public currentTemplateID As Integer = 0               ' The current template's Id
        'Public currentTemplateName As String = ""           ' The current template's name
        'Public currentNavigationStructure As String = ""    ' Public string describing the current page
        'Public currentParentID As Integer = 0               '
        'Public main_RenderCache_Loaded As Boolean = False                               ' true after main_loadRenderCache
        'Public main_RenderCache_CurrentPage_PCCPtr As Integer = 0
        'Public main_RenderCache_CurrentPage_ContentId As Integer = 0                        '
        'Public pageContentModel.contentName As String = ""                   ' set during LoadContent_CurrentPage
        'Public main_RenderCache_CurrentPage_IsRenderingMode As Boolean = False             ' true if tools panel rendering is on
        'Public main_RenderCache_CurrentPage_IsQuickEditing As Boolean = False              ' true if tools panel is on, and user can author current page
        'Public main_RenderCache_CurrentPage_IsEditing As Boolean = False                   ' true if tools panel is on
        'Public main_RenderCache_CurrentPage_IsAuthoring As Boolean = False                ' true if either editing or quickediting
        'Public main_RenderCache_CurrentPage_IsRootPage As Boolean = False                  ' true after LoadContent if the current page is the root page
        'Public main_RenderCache_ParentBranch_PCCPtrCnt As Integer = 0
        'Public main_RenderCache_ParentBranch_PCCPtrs As Integer()
        'Public main_RenderCache_ChildBranch_PCCPtrCnt As Integer = 0
        'Public main_RenderCache_ChildBranch_PCCPtrs As Integer()
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
        Public Structure main_FormPagetype_InstType
            Dim Type As Integer
            Dim Caption As String
            Dim REquired As Boolean
            Dim PeopleField As String
            Dim GroupName As String
        End Structure
        '
        Public Structure main_FormPagetype
            Dim PreRepeat As String
            Dim PostRepeat As String
            Dim RepeatCell As String
            Dim AddGroupNameList As String
            Dim AuthenticateOnFormProcess As Boolean
            Dim Inst() As main_FormPagetype_InstType
        End Structure
        ''
        '' ----------------------------------------------------------------------------------------
        ''   values collected from add-ons as the page progresses
        '' ----------------------------------------------------------------------------------------
        ''
        '' ----- Site Section Cache
        ''
        'Public Const cache_siteSection_cacheName = "cache_siteSection"
        'Public cache_siteSection As String(,)
        'Public cache_siteSection_rows As Integer = 0
        'Public cache_siteSection_IDIndex As keyPtrController
        'Public cache_siteSection_RootPageIDIndex As keyPtrController
        'Public cache_siteSection_NameIndex As keyPtrController
        ''
        '' ----- Template Content store
        ''
        'Public Const cache_pageTemplate_cacheName = "cache_pageTemplate"
        'Public cache_pageTemplate As String(,)
        'Public cache_pageTemplate_rows As Integer = 0
        'Public cache_pageTemplate_contentIdindex As keyPtrController
        ''
        '' -- Page Content store (old names for compatibility)
        'Public cache_pageContent_rows As Integer = 0
        'Public cache_pageContent As String(,)
        'Public cache_pageContent_idIndex As keyPtrController
        'Public cache_pageContent_parentIdIndex As keyPtrController
        'Public cache_pageContent_nameIndex As keyPtrController
        '
        '====================================================================================================
        ''' <summary>
        ''' this will eventuall be an addon, but lets do this first to keep the converstion complexity down
        ''' </summary>
        ''' <param name="cpCore"></param>

        Public Sub New(cpCore As coreClass, pageId As Integer)
            Me.cpcore = cpCore
            '
            ' -- setup domain
            domain = Models.Entity.domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
            If (pageId = 0) Then
                '
                ' -- Nothing specified, use the Landing Page
                pageId = getLandingPageID()
            End If
            Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(pageId))
            '
            ' -- build parentpageList (first = current page, last = root)
            ' -- add a 0, then repeat until another 0 is found, or there is a repeat
            pageToRootList = New List(Of Models.Entity.pageContentModel)()
            Dim usedPageIdList As New List(Of Integer)()
            Dim targetPageId = pageId
            usedPageIdList.Add(0)
            Do While (Not usedPageIdList.Contains(targetPageId))
                usedPageIdList.Add(targetPageId)
                Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpCore, targetPageId, New List(Of String))
                If (targetpage Is Nothing) Then
                    Exit Do
                Else
                    pageToRootList.Add(targetpage)
                    targetPageId = targetpage.ParentID
                End If
            Loop
            If (pageToRootList.Count = 0) Then
                '
                page = New pageContentModel()
            Else
                page = pageToRootList.First
            End If
            '
            ' -- get template from pages
            template = Nothing
            For Each page As Models.Entity.pageContentModel In pageToRootList
                If page.TemplateID > 0 Then
                    template = Models.Entity.pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
                    If (template IsNot Nothing) Then
                        If (page Is pageToRootList.First) Then
                            templateReason = "This template was used because it is selected by the current page."
                        Else
                            templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
                        End If
                        Exit For
                    End If
                End If
            Next
            '
            If (template Is Nothing) Then
                '
                ' -- get template from domain
                If (domain IsNot Nothing) Then
                    template = Models.Entity.pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
                End If
                If (template Is Nothing) Then
                    '
                    ' -- get template named Default
                    template = Models.Entity.pageTemplateModel.createByName(cpCore, defaultTemplateName, New List(Of String))
                End If
                If (template Is Nothing) Then
                    '
                    ' -- ceate new template named Default
                    template = Models.Entity.pageTemplateModel.add(cpCore, New List(Of String))
                    template.Name = defaultTemplateName
                    template.BodyHTML = defaultTemplateHtml
                    template.save(cpCore)
                End If
            End If
        End Sub
        '
        '========================================================================
        '   Returns the HTML body
        '
        '   This code is based on the GoMethod site script
        '========================================================================
        '
        Public Function getHtmlDocBody(cpCore As coreClass) As String
            Dim returnBody As String = ""
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
                Call cpCore.addonLegacyCache.load()
                returnBody = ""
                '
                ' ----- OnBodyStart add-ons
                '
                FilterStatusOK = False
                Cnt = UBound(cpCore.addonLegacyCache.addonCache.onBodyStartPtrs) + 1
                For Ptr = 0 To Cnt - 1
                    addonCachePtr = cpCore.addonLegacyCache.addonCache.onBodyStartPtrs(Ptr)
                    If addonCachePtr > -1 Then
                        addonId = cpCore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).id
                        'hint = hint & ",addonId=" & addonId
                        If addonId > 0 Then
                            AddonName = cpCore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).name
                            'hint = hint & ",AddonName=" & AddonName
                            returnBody = returnBody & cpCore.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextOnBodyStart, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
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
                PageContent = getHtmlDocBodyContent(cpCore, True, True, False, blockSiteWithLogin)
                If blockSiteWithLogin Then
                    '
                    ' section blocked, just return the login box in the page content
                    '
                    returnBody = "" _
                        & cr & "<div class=""ccLoginPageCon"">" _
                        & genericController.htmlIndent(PageContent) _
                        & cr & "</div>" _
                        & ""
                ElseIf Not cpCore.continueProcessing Then
                    '
                    ' exit if stream closed during main_GetSectionpage
                    '
                    returnBody = ""
                Else
                    '
                    ' -- no section block, continue
                    LocalTemplateID = template.ID
                    LocalTemplateBody = template.BodyHTML
                    If LocalTemplateBody = "" Then
                        LocalTemplateBody = TemplateDefaultBody
                    End If
                    LocalTemplateName = template.Name
                    If LocalTemplateName = "" Then
                        LocalTemplateName = "Template " & LocalTemplateID
                    End If
                    '
                    ' ----- Encode Template
                    '
                    If Not cpCore.htmlDoc.pageManager_printVersion Then
                        LocalTemplateBody = cpCore.htmlDoc.html_executeContentCommands(Nothing, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.authContext.user.id, cpCore.authContext.isAuthenticated, layoutError)
                        returnBody = returnBody & cpCore.htmlDoc.html_encodeContent9(LocalTemplateBody, cpCore.authContext.user.id, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain, False, cpCore.siteProperties.defaultWrapperID, PageContent, CPUtilsBaseClass.addonContext.ContextTemplate)
                        'returnHtmlBody = returnHtmlBody & EncodeContent8(LocalTemplateBody, memberID, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", main_ServerProtocol, False, app.SiteProperty_DefaultWrapperID, PageContent, ContextTemplate)
                    End If
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
                    If Not cpCore.authContext.isAuthenticated() Then
                        '
                        ' not logged in
                        '
                    Else
                        '
                        ' Add template editing
                        '
                        If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") And cpCore.authContext.isEditing(cpCore, "Page Templates") Then
                            returnBody = cpCore.htmlDoc.main_GetEditWrapper("Page Template [" & LocalTemplateName & "]", cpCore.htmlDoc.main_GetRecordEditLink2("Page Templates", LocalTemplateID, False, LocalTemplateName, cpCore.authContext.isEditing(cpCore, "Page Templates")) & returnBody)
                        End If
                    End If
                    '
                    ' ----- OnBodyEnd add-ons
                    '
                    'hint = hint & ",onBodyEnd"
                    FilterStatusOK = False
                    Cnt = UBound(cpCore.addonLegacyCache.addonCache.onBodyEndPtrs) + 1
                    'hint = hint & ",cnt=" & Cnt
                    For Ptr = 0 To Cnt - 1
                        addonCachePtr = cpCore.addonLegacyCache.addonCache.onBodyEndPtrs(Ptr)
                        'hint = hint & ",ptr=" & Ptr & ",addonCachePtr=" & addonCachePtr
                        If addonCachePtr > -1 Then
                            addonId = cpCore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).id
                            'hint = hint & ",addonId=" & addonId
                            If addonId > 0 Then
                                AddonName = cpCore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).name
                                'hint = hint & ",AddonName=" & AddonName
                                cpCore.htmlDoc.html_DocBodyFilter = returnBody
                                AddonReturn = cpCore.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                                returnBody = cpCore.htmlDoc.html_DocBodyFilter & AddonReturn
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
                    returnBody = htmlReflowController.reflow(cpCore, returnBody)
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnBody
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
                Dim htmlDocBody As String
                Dim htmlDocHead As String
                Dim bodyTag As String
                'Dim bodyAddonId As Integer
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
                If cpCore.continueProcessing Then
                    cpCore.htmlDoc.main_AdminWarning = cpCore.docProperties.getText("main_AdminWarningMsg")
                    cpCore.htmlDoc.main_AdminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID")
                    '
                    ' todo move cookie test to htmlDoc controller
                    ' -- Add cookie test
                    Dim AllowCookieTest As Boolean
                    AllowCookieTest = cpCore.siteProperties.allowVisitTracking And (cpCore.authContext.visit.PageVisits = 1)
                    If AllowCookieTest Then
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" & cpCore.security.encodeToken(cpCore.authContext.visit.id, cpCore.app_startTime) & "')};", "Cookie Test")
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
                            Call cpCore.db.cs_set(cs, "VisitId", cpCore.authContext.visit.id)
                        End If
                        Call cpCore.db.cs_Close(cs)
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
                    cpCore.htmlDoc.pageManager_RedirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId))
                    If (cpCore.htmlDoc.pageManager_RedirectContentID <> 0) Then
                        cpCore.htmlDoc.pageManager_RedirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId))
                        If (cpCore.htmlDoc.pageManager_RedirectRecordID <> 0) Then
                            Dim contentName As String = cpCore.metaData.getContentNameByID(cpCore.htmlDoc.pageManager_RedirectContentID)
                            If contentName <> "" Then
                                If iisController.main_RedirectByRecord_ReturnStatus(cpCore, contentName, cpCore.htmlDoc.pageManager_RedirectRecordID) Then
                                    '
                                    'Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    '
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
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
                                        log.VisitID = cpCore.authContext.visit.id
                                        log.MemberID = cpCore.authContext.user.id
                                    End If
                                    '
                                    ' -- and go
                                    Dim link As String = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, link)
                                    Call cpCore.webServer.redirect(link, "Redirecting because the active download request variable is set to a valid Library Files record. Library File Log has been appended.", False)
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
                        Call genericController.modifyLinkQuery(cpCore.htmlDoc.refreshQueryString, RequestNameCut, "")
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
                        Call genericController.ModifyQueryString(cpCore.htmlDoc.refreshQueryString, RequestNamePasteParentContentID, "")
                        Call genericController.ModifyQueryString(cpCore.htmlDoc.refreshQueryString, RequestNamePasteParentRecordID, "")
                        ClipParentContentName = cpCore.metaData.getContentNameByID(ClipParentContentID)
                        If (ClipParentContentName = "") Then
                            ' state not working...
                        ElseIf (ClipBoard = "") Then
                            ' state not working...
                        Else
                            If Not cpCore.authContext.isAuthenticatedContentManager(cpCore, ClipParentContentName) Then
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
                                        If Not cpCore.metaData.isWithinContent(ClipChildContentID, ClipParentContentID) Then
                                            Call errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is not compatible with the clipboard data.")
                                        Else
                                            '
                                            ' the content definition relationship is OK between the child and parent record
                                            '
                                            ClipChildContentName = cpCore.metaData.getContentNameByID(ClipChildContentID)
                                            If Not ClipChildContentName <> "" Then
                                                Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data content is undefined.")
                                            Else
                                                If (ClipParentRecordID = 0) Then
                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data record is undefined.")
                                                ElseIf main_IsChildRecord(ClipChildContentName, ClipParentRecordID, ClipChildRecordID) Then
                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the destination location is a child of the clipboard data record.")
                                                Else
                                                    '
                                                    ' the parent record is not a child of the child record (circular check)
                                                    '
                                                    ClipChildRecordName = "record " & ClipChildRecordID
                                                    CSClip = cpCore.db.cs_open2(ClipChildContentName, ClipChildRecordID, True, True)
                                                    If Not cpCore.db.cs_ok(CSClip) Then
                                                        Call errorController.error_AddUserError(cpCore, "The paste operation failed because the data record referenced by the clipboard could not found.")
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
                                                                Call errorController.error_AddUserError(cpCore, "The paste operation failed because the record you are pasting does not   support the necessary parenting feature.")
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
                                                                    Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field List is not configured correctly.")
                                                                Else
                                                                    If Not cpCore.db.cs_isFieldSupported(CSClip, CStr(NameValues(0))) Then
                                                                        Call errorController.error_AddUserError(cpCore, "The paste operation failed because the clipboard data Field [" & CStr(NameValues(0)) & "] is not supported by the location data.")
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
                                                        'ShortLink = genericController.modifyLinkQuery(ShortLink, rnPageId, CStr(ClipChildRecordID), True)
                                                        'Call main_TrackContentSet(CSClip, ShortLink)
                                                    End If
                                                    Call cpCore.db.cs_Close(CSClip)
                                                    '
                                                    ' Set Child Pages Found and clear caches
                                                    '
                                                    CSClip = cpCore.db.csOpenRecord(ClipParentContentName, ClipParentRecordID, , , "ChildPagesFound")
                                                    If cpCore.db.cs_ok(CSClip) Then
                                                        Call cpCore.db.cs_set(CSClip, "ChildPagesFound", True.ToString)
                                                    End If
                                                    Call cpCore.db.cs_Close(CSClip)
                                                    'Call cache_pageContent_clear()
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
                                                        'Call cache_pageContent_clear()
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
                        Call genericController.modifyLinkQuery(cpCore.htmlDoc.refreshQueryString, RequestNameCutClear, "")
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
                        Call cpCore.db.cs_Close(CSPointer)
                        '
                        If (RedirectLink = "") And Not isLinkForward Then
                            '
                            ' Test for Link Alias
                            '
                            If (linkAliasTest1 & linkAliasTest2 <> "") Then
                                Dim sqlLinkAliasCriteria As String = "(link=" & cpCore.db.encodeSQLText(linkAliasTest1) & ")or(link=" & cpCore.db.encodeSQLText(linkAliasTest2) & ")"
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
                                        Call cpCore.webServer.setResponseStatus("404 Not Found")
                                        Call cpCore.webServer.setResponseContentType("image/gif")
                                        cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                        Return cpCore.htmlDoc.docBuffer
                                    Else
                                        Call cpCore.webServer.redirect(genericController.getCdnFileLink(cpCore, Filename), "favicon request", False)
                                        cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
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
                                    Content = Content & cpCore.addonLegacyCache.addonCache.robotsTxt
                                    Call cpCore.webServer.setResponseContentType("text/plain")
                                    Call cpCore.htmlDoc.writeAltBuffer(Content)
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                                End If
                                '
                                ' No Link Forward, no Link Alias, no RemoteMethodFromPage, not Robots.txt
                                '
                                If (cpCore.app_errorCount = 0) And cpCore.siteProperties.getBoolean("LinkForwardAutoInsert") And (Not IsInLinkForwardTable) Then
                                    '
                                    ' Add a new Link Forward entry
                                    '
                                    CSPointer = cpCore.db.cs_insertRecord("Link Forwards")
                                    If cpCore.db.cs_ok(CSPointer) Then
                                        Call cpCore.db.cs_set(CSPointer, "Name", cpCore.webServer.requestPathPage)
                                        Call cpCore.db.cs_set(CSPointer, "sourcelink", cpCore.webServer.requestPathPage)
                                        Call cpCore.db.cs_set(CSPointer, "Viewings", 1)
                                    End If
                                    Call cpCore.db.cs_Close(CSPointer)
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
                                    Call cpCore.htmlDoc.main_SetMetaContent(0, 0)
                                    Call cpCore.htmlDoc.writeAltBuffer(cpCore.htmlDoc.getLoginPage(False) & cpCore.htmlDoc.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False))
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                                Case 2
                                    '
                                    ' block with custom content
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3420 - exit for custom content block")
                                    '
                                    Call cpCore.htmlDoc.main_SetMetaContent(0, 0)
                                    Call cpCore.htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll'", "Anonymous User Block")
                                    Dim Copy As String = cr & cpCore.htmlDoc.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=""width:250px;margin:100px auto auto auto;"">The site is currently not available for anonymous access.</p>", cpCore.authContext.user.id, True, cpCore.authContext.isAuthenticated)
                                    ' -- already encoded
                                    'Copy = EncodeContentForWeb(Copy, "copy content", 0, "", 0)
                                    Copy = "" _
                                            & cpCore.siteProperties.docTypeDeclaration() _
                                            & vbCrLf & "<html>" _
                                            & cr & "<head>" _
                                            & genericController.htmlIndent(cpCore.htmlDoc.getHtmlDocHead(False)) _
                                            & cr & "</head>" _
                                            & cr & TemplateDefaultBodyTag _
                                            & genericController.htmlIndent(Copy) _
                                            & cr2 & "<div>" _
                                            & cr3 & cpCore.htmlDoc.getHtmlDoc_beforeEndOfBodyHtml(True, True, False, False) _
                                            & cr2 & "</div>" _
                                            & cr & "</body>" _
                                            & vbCrLf & "</html>"
                                    '& "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                                    Call cpCore.htmlDoc.writeAltBuffer(Copy)
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.htmlDoc.docBuffer
                            End Select
                        End If
                    End If
                    '
                    ' -- build document
                    htmlDocBody = getHtmlDocBody(cpCore)
                    'bodyAddonId = genericController.EncodeInteger(cpCore.siteProperties.getText("Html Body AddonId", "0"))
                    'If bodyAddonId <> 0 Then
                    '    htmlBody = cpCore.addon.execute(bodyAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", bodyAddonStatusOK, Nothing, "", Nothing, "", cpCore.authContext.user.id, cpCore.authContext.isAuthenticated)
                    'Else
                    '    htmlBody = getDocBody(cpCore)
                    'End If
                    '
                    ' -- add template and page details to document
                    Call cpCore.htmlDoc.main_AddOnLoadJavascript2(template.JSOnLoad, "template")
                    Call cpCore.htmlDoc.main_AddHeadScriptCode(template.JSHead, "template")
                    Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(template.JSEndBody, "template")
                    Call cpCore.htmlDoc.main_AddHeadTag2(template.OtherHeadTags, "template")
                    If template.StylesFilename <> "" Then
                        cpCore.htmlDoc.main_MetaContent_TemplateStyleSheetTag = cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, template.StylesFilename) & """ >"
                    End If
                    '
                    ' -- add shared styles
                    Dim sqlCriteria As String = "(templateId=" & template.ID & ")"
                    Dim styleList As List(Of Models.Entity.SharedStylesTemplateRuleModel) = Models.Entity.SharedStylesTemplateRuleModel.createList(cpCore, sqlCriteria, "sortOrder,id")
                    For Each rule As SharedStylesTemplateRuleModel In styleList
                        Call cpCore.htmlDoc.main_AddSharedStyleID2(rule.StyleID, "template")
                    Next
                    '
                    ' -- check secure certificate required
                    Dim SecureLink_Template_Required As Boolean = template.IsSecure
                    Dim SecureLink_Page_Required As Boolean = False
                    For Each page As Models.Entity.pageContentModel In pageToRootList
                        If page.IsSecure Then
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
                        pageManager_RedirectReason = "Redirecting because neither the page or the template requires a secure link."
                        Return ""
                    ElseIf ((Not SecureLink_CurrentURL) And SecureLink_Required) Then
                        '
                        ' -- redirect to secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "http://", "https://")
                        If SecureLink_Page_Required Then
                            pageManager_RedirectReason = "Redirecting because this page [" & pageToRootList(0).name & "] requires a secure link."
                        Else
                            pageManager_RedirectReason = "Redirecting because this template [" & template.Name & "] requires a secure link."
                        End If
                        Return ""
                    End If
                    '
                    ' -- check that this template exists on this domain
                    ' -- if endpoint is just domain -> the template is automatically compatible by default (domain determined the landing page)
                    ' -- if endpoint is domain + route (link alias), the route determines the page, which may determine the template. If this template is not allowed for this domain, redirect to the domain's landing page.
                    '
                    Sql = "(domainId=" & domain.ID & ")"
                    Dim allowTemplateRuleList As List(Of Models.Entity.TemplateDomainRuleModel) = Models.Entity.TemplateDomainRuleModel.createList(cpCore, Sql)
                    If (allowTemplateRuleList.Count = 0) Then
                        '
                        ' -- current template has no domain preference, use current
                    Else
                        Dim allowTemplate As Boolean = False
                        For Each rule As TemplateDomainRuleModel In allowTemplateRuleList
                            If (rule.templateId = template.ID) Then
                                allowTemplate = True
                                Exit For
                            End If
                        Next
                        If (Not allowTemplate) Then
                            '
                            ' -- must redirect to a domain's landing page
                            RedirectLink = cpCore.webServer.webServerIO_requestProtocol & domain.Name
                            pageManager_RedirectBecausePageNotFound = False
                            pageManager_RedirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" & template.Name & "]."
                            Return ""
                        End If
                    End If
                    If cpCore.continueProcessing Then
                        '
                        ' Build Body Tag
                        '
                        htmlDocHead = cpCore.htmlDoc.getHtmlDocHead(False)
                        If template.BodyTag <> "" Then
                            bodyTag = template.BodyTag
                        Else
                            bodyTag = TemplateDefaultBodyTag
                        End If
                        '
                        ' Add tools panel to body
                        '
                        htmlDocBody = htmlDocBody & cr & "<div>" & genericController.htmlIndent(cpCore.htmlDoc.getHtmlDoc_beforeEndOfBodyHtml(True, True, False, False)) & cr & "</div>"
                        '
                        ' build doc
                        '
                        returnHtml = cpCore.htmlDoc.assembleHtmlDoc(cpCore.siteProperties.docTypeDeclaration(), htmlDocHead, bodyTag, cpCore.htmlDoc.docBuffer & htmlDocBody)
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
                            Call cpCore.webServer.redirect(linkDst, Copy, False)
                            cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
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
                            Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                            Call cpCore.webServer.setResponseStatus("404 Not Found")
                            cpCore.docProperties.setProperty(rnPageId, main_GetPageNotFoundPageId())
                            'Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                            If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                cpCore.htmlDoc.main_AdminWarning = PageNotFoundReason
                                cpCore.htmlDoc.main_AdminWarningPageID = 0
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
        Private Sub processForm(cpcore As coreClass, FormPageID As Integer)
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
                CS = cpcore.db.csOpenRecord("Form Pages", FormPageID)
                If cpcore.db.cs_ok(CS) Then
                    Formhtml = cpcore.db.cs_getText(CS, "Body")
                    FormInstructions = cpcore.db.cs_getText(CS, "Instructions")
                End If
                Call cpcore.db.cs_Close(CS)
                If FormInstructions <> "" Then
                    '
                    ' Load the instructions
                    '
                    f = pageManager_LoadFormPageInstructions(FormInstructions, Formhtml)
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
                                    If (FormValue <> "") And genericController.EncodeBoolean(cpcore.metaData.GetContentFieldProperty("people", .PeopleField, "uniquename")) Then
                                        SQL = "select count(*) from ccMembers where " & .PeopleField & "=" & cpcore.db.encodeSQLText(FormValue)
                                        CS = cpcore.db.cs_openSql(SQL)
                                        If cpcore.db.cs_ok(CS) Then
                                            Success = cpcore.db.cs_getInteger(CS, "cnt") = 0
                                        End If
                                        Call cpcore.db.cs_Close(CS)
                                        If Not Success Then
                                            errorController.error_AddUserError(cpcore, "The field [" & .Caption & "] must be unique, and the value [" & cpcore.htmlDoc.html_EncodeHTML(FormValue) & "] has already been used.")
                                        End If
                                    End If
                                    If (.REquired Or genericController.EncodeBoolean(cpcore.metaData.GetContentFieldProperty("people", .PeopleField, "required"))) And FormValue = "" Then
                                        Success = False
                                        errorController.error_AddUserError(cpcore, "The field [" & cpcore.htmlDoc.html_EncodeHTML(.Caption) & "] is required.")
                                    Else
                                        If Not cpcore.db.cs_ok(CSPeople) Then
                                            CSPeople = cpcore.db.csOpenRecord("people", cpcore.authContext.user.id)
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
                            Call cpcore.authContext.authenticateById(cpcore, cpcore.authContext.user.id, cpcore.authContext)
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
                cpcore.handleExceptionAndContinue(ex)
                Throw
            End Try
        End Sub
        '
        '=============================================================================
        '   main_Get Section
        '       Two modes
        '           pre 3.3.613 - SectionName = RootPageName
        '           else - (IsSectionRootPageIDMode) SectionRecord has a RootPageID field
        '=============================================================================
        '
        Public Function getHtmlDocBodyContent(cpCore As coreClass, AllowChildPageList As Boolean, AllowReturnLink As Boolean, AllowEditWrapper As Boolean, ByRef return_blockSiteWithLogin As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim allowPageWithoutSectionDislay As Boolean
                Dim domainIds() As String
                Dim setdomainId As Integer
                Dim linkDomain As String
                Dim templatedomainIdList As String
                Dim FieldRows As Integer
                Dim templateId As Integer
                Dim RootPageContentName As String
                Dim Ptr As Integer
                Dim PageID As Integer
                Dim UseContentWatchLink As Boolean = cpCore.siteProperties.useContentWatchLink
                '
                RootPageContentName = "Page Content"
                '
                ' -- validate domain
                'domain = Models.Entity.domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
                If (domain Is Nothing) Then
                    '
                    ' -- domain not listed, this is now an error
                    Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">The domain name is not configured for this site.</div>"
                End If
                '
                ' -- validate page
                If page.id = 0 Then
                    '
                    ' -- landing page is not valid -- display error
                    Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
                    pageManager_RedirectBecausePageNotFound = True
                    pageManager_RedirectReason = "Redirecting because the page selected could not be found."
                    redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, 0)
                    cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
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
                '        pageManager_RedirectBecausePageNotFound = True
                '        pageManager_RedirectReason = "Redirecting because the page selected could not be found."
                '        redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, 0)

                '        cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
                '        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
                '    End If
                'End If
                'Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(PageID))
                'templateReason = "The reason this template was selected could not be determined."
                ''
                '' -- build parentpageList (first = current page, last = root)
                '' -- add a 0, then repeat until another 0 is found, or there is a repeat
                'pageToRootList = New List(Of Models.Entity.pageContentModel)()
                'Dim usedPageIdList As New List(Of Integer)()
                'Dim targetPageId = PageID
                'usedPageIdList.Add(0)
                'Do While (Not usedPageIdList.Contains(targetPageId))
                '    usedPageIdList.Add(targetPageId)
                '    Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpCore, targetPageId, New List(Of String))
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
                'Dim template As Models.Entity.pageTemplateModel = Nothing
                'For Each page As Models.Entity.pageContentModel In pageToRootList
                '    If page.TemplateID > 0 Then
                '        template = Models.Entity.pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
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
                '        template = Models.Entity.pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
                '    End If
                '    If (template Is Nothing) Then
                '        '
                '        ' -- get template named Default
                '        template = Models.Entity.pageTemplateModel.createByName(cpCore, "default", New List(Of String))
                '    End If
                'End If
                '
                ' -- get contentbox
                returnHtml = getContentBox("", AllowChildPageList, AllowReturnLink, False, 0, UseContentWatchLink, allowPageWithoutSectionDislay)
                '
                ' -- if fpo_QuickEdit it there, replace it out
                Dim Editor As String
                Dim styleOptionList As String
                Dim addonListJSON As String
                If redirectLink = "" And (InStr(1, returnHtml, html_quickEdit_fpo) <> 0) Then
                    FieldRows = genericController.EncodeInteger(cpCore.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"))
                    If FieldRows < 50 Then
                        FieldRows = 50
                        Call cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50)
                    End If
                    Dim stylesheetCommaList As String = cpCore.htmlDoc.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                    addonListJSON = cpCore.htmlDoc.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
                    Editor = cpCore.htmlDoc.html_GetFormInputHTML3("copyFilename", cpCore.htmlDoc.html_quickEdit_copy, CStr(FieldRows), "100%", False, True, addonListJSON, stylesheetCommaList, styleOptionList)
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor)
                End If
                '
                ' -- Add admin warning to the top of the content
                If cpCore.authContext.isAuthenticatedAdmin(cpCore) And cpCore.htmlDoc.main_AdminWarning <> "" Then
                    '
                    ' Display Admin Warnings with Edits for record errors
                    '
                    If cpCore.htmlDoc.main_AdminWarningPageID <> 0 Then
                        cpCore.htmlDoc.main_AdminWarning = cpCore.htmlDoc.main_AdminWarning & "</p>" & cpCore.htmlDoc.main_GetRecordEditLink2("Page Content", cpCore.htmlDoc.main_AdminWarningPageID, True, "Page " & cpCore.htmlDoc.main_AdminWarningPageID, cpCore.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
                        cpCore.htmlDoc.main_AdminWarningPageID = 0
                    End If
                    '
                    returnHtml = "" _
                    & cpCore.htmlDoc.html_GetAdminHintWrapper(cpCore.htmlDoc.main_AdminWarning) _
                    & returnHtml _
                    & ""
                    cpCore.htmlDoc.main_AdminWarning = ""
                End If
                '
                ' -- handle redirect and edit wrapper
                '------------------------------------------------------------------------------------
                '
                If redirectLink <> "" Then
                    Call cpCore.webServer.redirect(redirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
                ElseIf AllowEditWrapper Then
                    returnHtml = cpCore.htmlDoc.main_GetEditWrapper("Page Content", returnHtml)
                End If
                '
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        '   pageManager_GetHtmlBody_GetSection_GetContent
        '
        '   PageID is the page to display. If it is 0, the root page is displayed
        '   RootPageID has to be the ID of the root page for PageID
        '=============================================================================
        '
        Public Function getContentBox(OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreme As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim returnHtml As String = ""
            Try
                Dim AddonName As String
                Dim addonCachePtr As Integer
                Dim addonPtr As Integer
                Dim AddOnCnt As Integer
                Dim addonId As Integer
                Dim AddonContent As String
                Dim DateModified As Date
                Dim PageRecordID As Integer
                Dim PageName As String
                'Dim RootPageContentCID As Integer
                'Dim iRootPageContentName As String
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
                Call cpcore.htmlDoc.main_AddHeadTag2("<meta name=""contentId"" content=""" & page.id & """ >", "page content")
                '
                returnHtml = getContentBox_content(OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay)
                '
                ' ----- If Link field populated, do redirect
                If (page.PageLink <> "") Then
                    page.Clicks += 1
                    page.save(cpcore)
                    redirectLink = page.PageLink
                    pageManager_RedirectReason = "Redirect required because this page (PageRecordID=" & page.id & ") has a Link Override [" & page.PageLink & "]."
                    Return ""
                End If
                '
                ' -- build list of blocked pages
                Dim BlockedRecordIDList As String = ""
                If (returnHtml <> "") And (redirectLink = "") Then
                    NewPageCreated = True
                    For Each testPage As pageContentModel In pageToRootList
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
                    If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                        '
                        ' Administrators are never blocked
                        '
                    ElseIf (Not cpcore.authContext.isAuthenticated()) Then
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
                            & " WHERE (((ccMemberRules.MemberID)=" & cpcore.db.encodeSQLNumber(cpcore.authContext.user.id) & ")" _
                            & " AND ((ccPageContentBlockRules.RecordID) In (" & BlockedRecordIDList & "))" _
                            & " AND ((ccPageContentBlockRules.Active)<>0)" _
                            & " AND ((ccgroups.Active)<>0)" _
                            & " AND ((ccMemberRules.Active)<>0)" _
                            & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & cpcore.db.encodeSQLDate(cpcore.app_startTime) & "));"
                        CS = cpcore.db.cs_openSql(SQL)
                        BlockedRecordIDList = "," & BlockedRecordIDList
                        Do While cpcore.db.cs_ok(CS)
                            BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpcore.db.cs_getText(CS, "RecordID"), "")
                            cpcore.db.cs_goNext(CS)
                        Loop
                        Call cpcore.db.cs_Close(CS)
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
                                & " AND ((ManagementMemberRules.DateExpires) Is Null Or (ManagementMemberRules.DateExpires)>" & cpcore.db.encodeSQLDate(cpcore.app_startTime) & ")" _
                                & " AND ((ManagementMemberRules.MemberID)=" & cpcore.authContext.user.id & " ));"
                            CS = cpcore.db.cs_openSql(SQL)
                            Do While cpcore.db.cs_ok(CS)
                                BlockedRecordIDList = genericController.vbReplace(BlockedRecordIDList, "," & cpcore.db.cs_getText(CS, "RecordID"), "")
                                cpcore.db.cs_goNext(CS)
                            Loop
                            Call cpcore.db.cs_Close(CS)
                        End If
                        If BlockedRecordIDList <> "" Then
                            ContentBlocked = True
                        End If
                        Call cpcore.db.cs_Close(CS)
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
                        CS = cpcore.db.csOpenRecord("Page Content", BlockedPageRecordID, , , "CustomBlockMessage,BlockSourceID,RegistrationGroupID,ContentPadding")
                        If cpcore.db.cs_ok(CS) Then
                            BlockSourceID = cpcore.db.cs_getInteger(CS, "BlockSourceID")
                            ContentPadding = cpcore.db.cs_getInteger(CS, "ContentPadding")
                            CustomBlockMessageFilename = cpcore.db.cs_getText(CS, "CustomBlockMessage")
                            RegistrationGroupID = cpcore.db.cs_getInteger(CS, "RegistrationGroupID")
                        End If
                        Call cpcore.db.cs_Close(CS)
                    End If
                    '
                    ' Block Appropriately
                    '
                    Select Case BlockSourceID
                        Case main_BlockSourceCustomMessage
                            '
                            ' ----- Custom Message
                            '
                            returnHtml = cpcore.cdnFiles.readFile(CustomBlockMessageFilename)
                        Case main_BlockSourceLogin
                            '
                            ' ----- Login page
                            '
                            Dim BlockForm As String = ""
                            If Not cpcore.authContext.isAuthenticated() Then
                                If Not cpcore.authContext.isRecognized(cpcore) Then
                                    '
                                    ' not recognized
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                        & ""
                                    BlockForm = cpcore.htmlDoc.getLoginForm()
                                Else
                                    '
                                    ' recognized, not authenticated
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. You were recognized as ""<b>" & cpcore.authContext.user.Name & "</b>"", but you need to login to continue. To login to this account or another, please use this form.</p>" _
                                        & ""
                                    BlockForm = cpcore.htmlDoc.getLoginForm()
                                End If
                            Else
                                '
                                ' authenticated
                                '
                                BlockCopy = "" _
                                    & "<p>You are currently logged in as ""<b>" & cpcore.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & cpcore.htmlDoc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                    & "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>" _
                                    & ""
                                BlockForm = cpcore.htmlDoc.getLoginForm()
                            End If
                            returnHtml = "" _
                                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td align=center>" _
                                & "<div style=""width:400px;text-align:left;"">" _
                                & errorController.error_GetUserError(cpcore) _
                                & BlockCopy _
                                & BlockForm _
                                & "</div></td></tr></table>"
                        Case main_BlockSourceRegistration
                            '
                            ' ----- Registration
                            '
                            Dim BlockForm As String = ""
                            If cpcore.docProperties.getInteger("subform") = main_BlockSourceLogin Then
                                '
                                ' login subform form
                                '
                                BlockForm = cpcore.htmlDoc.getLoginForm()
                                BlockCopy = "" _
                                    & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                    & "<p>If you do not have an account, <a href=?" & cpcore.htmlDoc.refreshQueryString & "&subform=0>click here to register</a>.</p>" _
                                    & ""
                            Else
                                '
                                ' Register Form
                                '
                                If Not cpcore.authContext.isAuthenticated() And cpcore.authContext.isRecognized(cpcore) Then
                                    '
                                    ' Can not take the chance, if you go to a registration page, and you are recognized but not auth -- logout first
                                    '
                                    Call cpcore.authContext.logout(cpcore)
                                End If
                                If Not cpcore.authContext.isAuthenticated() Then
                                    '
                                    ' Not Authenticated
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. If you have an account, <a href=?" & cpcore.htmlDoc.refreshQueryString & "&subform=" & main_BlockSourceLogin & ">Click Here to login</a>.</p>" _
                                        & "<p>To view this content, please complete this form.</p>" _
                                        & ""
                                Else
                                    BlockCopy = "" _
                                        & "<p>You are currently logged in as ""<b>" & cpcore.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & cpcore.htmlDoc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
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
                                    Call verifyRegistrationFormPage(cpcore)
                                    BlockForm = pageManager_GetFormPage("Registration Form", RegistrationGroupID)
                                End If
                            End If
                            returnHtml = "" _
                                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td align=center>" _
                                & "<div style=""width:400px;text-align:left;"">" _
                                & errorController.error_GetUserError(cpcore) _
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
                    returnHtml = cpcore.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                    returnHtml = cpcore.htmlDoc.html_encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                    If cpcore.htmlDoc.refreshQueryString <> "" Then
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" & cpcore.htmlDoc.refreshQueryString, 1, 99, vbTextCompare)
                    End If
                    '
                    ' Add in content padding required for integration with the template
                    returnHtml = pageManager_GetContentBoxWrapper(returnHtml, ContentPadding)
                End If
                '
                ' ----- Encoding, Tracking and Triggers
                If Not ContentBlocked Then
                    If cpcore.visitProperty.getBoolean("AllowQuickEditor") Then
                        '
                        ' Quick Editor, no encoding or tracking
                        '
                    Else
                        pageViewings = page.Viewings
                        If cpcore.authContext.isEditing(cpcore, pageContentModel.contentName) Or cpcore.visitProperty.getBoolean("AllowWorkflowRendering") Then
                            '
                            ' Link authoring, workflow rendering -> do encoding, but no tracking
                            '
                            returnHtml = cpcore.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.htmlDoc.html_encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                        ElseIf cpcore.htmlDoc.pageManager_printVersion Then
                            '
                            ' Printer Version -> personalize and count viewings, no tracking
                            '
                            returnHtml = cpcore.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.htmlDoc.html_encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                            Call cpcore.db.executeSql("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & page.id)
                        Else
                            '
                            ' Live content
                            returnHtml = cpcore.htmlDoc.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.htmlDoc.html_encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                            Call cpcore.db.executeSql("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & page.id)
                        End If
                        '
                        ' Page Hit Notification
                        '
                        If (Not cpcore.authContext.visit.ExcludeFromAnalytics) And (page.ContactMemberID <> 0) And (InStr(1, cpcore.webServer.requestBrowser, "kmahttp", vbTextCompare) = 0) Then
                            If page.AllowHitNotification Then
                                PageName = page.name
                                If PageName = "" Then
                                    PageName = page.MenuHeadline
                                    If PageName = "" Then
                                        PageName = page.Headline
                                        If PageName = "" Then
                                            PageName = "[no name]"
                                        End If
                                    End If
                                End If
                                Dim Body As String = ""
                                Body = Body & "<p><b>Page Hit Notification.</b></p>"
                                Body = Body & "<p>This email was sent to you by the Contensive Server as a notification of the following content viewing details.</p>"
                                Body = Body & genericController.StartTable(4, 1, 1)
                                Body = Body & "<tr><td align=""right"" width=""150"" Class=""ccPanelHeader"">Description<br><img alt=""image"" src=""http://" & cpcore.webServer.requestDomain & "/ccLib/images/spacer.gif"" width=""150"" height=""1""></td><td align=""left"" width=""100%"" Class=""ccPanelHeader"">Value</td></tr>"
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Domain", cpcore.webServer.webServerIO_requestDomain, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Link", cpcore.webServer.requestUrl, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Page Name", PageName, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Member Name", cpcore.authContext.user.Name, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Member #", CStr(cpcore.authContext.user.id), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Start Time", CStr(cpcore.authContext.visit.StartTime), False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit #", CStr(cpcore.authContext.visit.id), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit IP", cpcore.webServer.requestRemoteIP, False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Browser ", cpcore.webServer.requestBrowser, True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visitor #", CStr(cpcore.authContext.visitor.ID), False)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Authenticated", CStr(cpcore.authContext.visit.VisitAuthenticated), True)
                                Body = Body & pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow("Visit Referrer", cpcore.authContext.visit.HTTP_REFERER, False)
                                Body = Body & kmaEndTable
                                Call cpcore.email.sendPerson(page.ContactMemberID, cpcore.siteProperties.getText("EmailFromAddress", "info@" & cpcore.webServer.webServerIO_requestDomain), "Page Hit Notification", Body, False, True, 0, "", False)
                            End If
                        End If
                        '
                        ' -- Process Trigger Conditions
                        ConditionID = page.TriggerConditionID
                        ConditionGroupID = page.TriggerConditionGroupID
                        main_AddGroupID = page.TriggerAddGroupID
                        RemoveGroupID = page.TriggerRemoveGroupID
                        SystemEMailID = page.TriggerSendSystemEmailID
                        Select Case ConditionID
                            Case 1
                                '
                                ' Always
                                '
                                If SystemEMailID <> 0 Then
                                    Call cpcore.email.sendSystem_Legacy(cpcore.db.getRecordName("System Email", SystemEMailID), "", cpcore.authContext.user.id)
                                End If
                                If main_AddGroupID <> 0 Then
                                    Call groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, main_AddGroupID))
                                End If
                                If RemoveGroupID <> 0 Then
                                    Call groupController.group_DeleteGroupMember(cpcore, groupController.group_GetGroupName(cpcore, RemoveGroupID))
                                End If
                            Case 2
                                '
                                ' If in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If cpcore.authContext.IsMemberOfGroup2(cpcore, groupController.group_GetGroupName(cpcore, ConditionGroupID)) Then
                                        If SystemEMailID <> 0 Then
                                            Call cpcore.email.sendSystem_Legacy(cpcore.db.getRecordName("System Email", SystemEMailID), "", cpcore.authContext.user.id)
                                        End If
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpcore, groupController.group_GetGroupName(cpcore, RemoveGroupID))
                                        End If
                                    End If
                                End If
                            Case 3
                                '
                                ' If not in Condition Group
                                '
                                If ConditionGroupID <> 0 Then
                                    If Not cpcore.authContext.IsMemberOfGroup2(cpcore, groupController.group_GetGroupName(cpcore, ConditionGroupID)) Then
                                        If main_AddGroupID <> 0 Then
                                            Call groupController.group_AddGroupMember(cpcore, groupController.group_GetGroupName(cpcore, main_AddGroupID))
                                        End If
                                        If RemoveGroupID <> 0 Then
                                            Call groupController.group_DeleteGroupMember(cpcore, groupController.group_GetGroupName(cpcore, RemoveGroupID))
                                        End If
                                        If SystemEMailID <> 0 Then
                                            Call cpcore.email.sendSystem_Legacy(cpcore.db.getRecordName("System Email", SystemEMailID), "", cpcore.authContext.user.id)
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
                    returnHtml = pageManager_GetContentBoxWrapper(returnHtml, page.ContentPadding)
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Set Headers
                    '---------------------------------------------------------------------------------
                    '
                    If DateModified <> Date.MinValue Then
                        Call cpcore.webServer.addResponseHeader("LAST-MODIFIED", genericController.GetGMTFromDate(DateModified))
                    End If
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- Store page javascript
                    '---------------------------------------------------------------------------------
                    '
                    Call cpcore.htmlDoc.main_AddOnLoadJavascript2(page.JSOnLoad, "page content")
                    Call cpcore.htmlDoc.main_AddHeadScriptCode(page.JSHead, "page content")
                    If page.JSFilename <> "" Then
                        Call cpcore.htmlDoc.main_AddHeadScriptLink(genericController.getCdnFileLink(cpcore, page.JSFilename), "page content")
                    End If
                    Call cpcore.htmlDoc.main_AddEndOfBodyJavascript2(page.JSEndBody, "page content")
                    '
                    '---------------------------------------------------------------------------------
                    ' Set the Meta Content flag
                    '---------------------------------------------------------------------------------
                    '
                    Call cpcore.htmlDoc.main_SetMetaContent(page.ContentControlID, page.id)
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- OnPageStartEvent
                    '---------------------------------------------------------------------------------
                    '
                    bodyContent = returnHtml
                    Dim addonList As List(Of addonModel) = Models.Entity.addonModel.createList_OnPageStartEvent(cpcore, New List(Of String))
                    For Each addon As Models.Entity.addonModel In addonList
                        AddonContent = cpcore.addon.execute_legacy5(addon.id, addon.name, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                        bodyContent = AddonContent & bodyContent
                    Next
                    returnHtml = bodyContent
                    '
                    ' ----- OnPageEndEvent
                    bodyContent = returnHtml
                    AddOnCnt = UBound(cpcore.addonLegacyCache.addonCache.onPageEndPtrs) + 1
                    For addonPtr = 0 To AddOnCnt - 1
                        addonCachePtr = cpcore.addonLegacyCache.addonCache.onPageEndPtrs(addonPtr)
                        If addonCachePtr > -1 Then
                            addonId = cpcore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).id
                            If addonId > 0 Then
                                AddonName = cpcore.addonLegacyCache.addonCache.addonList(addonCachePtr.ToString).name
                                AddonContent = cpcore.addon.execute_legacy5(addonId, AddonName, "CSPage=-1", CPUtilsBaseClass.addonContext.ContextOnPageStart, "", 0, "", -1)
                                bodyContent = bodyContent & AddonContent
                            End If
                        End If
                    Next
                    returnHtml = bodyContent
                    '
                End If
                If cpcore.htmlDoc.main_MetaContent_Title = "" Then
                    '
                    ' Set default page title
                    '
                    cpcore.htmlDoc.main_MetaContent_Title = page.name
                End If
                '
                ' add contentid and sectionid
                '
                Call cpcore.htmlDoc.main_AddHeadTag2("<meta name=""contentId"" content=""" & page.id & """ >", "page content")
                '
                ' Display Admin Warnings with Edits for record errors
                '
                If cpcore.htmlDoc.main_AdminWarning <> "" Then
                    '
                    If cpcore.htmlDoc.main_AdminWarningPageID <> 0 Then
                        cpcore.htmlDoc.main_AdminWarning = cpcore.htmlDoc.main_AdminWarning & "</p>" & cpcore.htmlDoc.main_GetRecordEditLink2("Page Content", cpcore.htmlDoc.main_AdminWarningPageID, True, "Page " & cpcore.htmlDoc.main_AdminWarningPageID, cpcore.authContext.isAuthenticatedAdmin(cpcore)) & "&nbsp;Edit the page<p>"
                        cpcore.htmlDoc.main_AdminWarningPageID = 0
                    End If
                    returnHtml = "" _
                    & cpcore.htmlDoc.html_GetAdminHintWrapper(cpcore.htmlDoc.main_AdminWarning) _
                    & returnHtml _
                    & ""
                    cpcore.htmlDoc.main_AdminWarning = ""
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' pageManager_GetHtmlBody_GetSection_GetContentBox
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
        Friend Function getContentBox_content(OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreMe As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim result As String = ""
            Try
                Dim isEditing As Boolean
                Dim LiveBody As String
                '
                If cpcore.continueProcessing Then
                    If redirectLink = "" Then
                        isEditing = cpcore.authContext.isEditing(cpcore, pageContentModel.contentName)
                        '
                        ' ----- Render the Body
                        LiveBody = pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body(OrderByClause, AllowChildPageList, False, pageToRootList.Last.id, AllowReturnLink, pageContentModel.contentName, ArchivePages)
                        Dim isRootPage As Boolean = (pageToRootList.Count = 1)
                        If cpcore.authContext.isAdvancedEditing(cpcore, "") Then
                            result = result & cpcore.htmlDoc.main_GetRecordEditLink(pageContentModel.contentName, page.id, (Not isRootPage)) & LiveBody
                        ElseIf isEditing Then
                            result = result & cpcore.htmlDoc.main_GetEditWrapper("", cpcore.htmlDoc.main_GetRecordEditLink(pageContentModel.contentName, page.id, (Not isRootPage)) & LiveBody)
                        Else
                            result = result & LiveBody
                        End If
                    End If
                End If
                '
                Call debugController.debug_testPoint(cpcore, "pageManager_GetHtmlBody_GetSection_GetContentBox")
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
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

        Friend Function pageManager_GetHtmlBody_GetSection_GetContentBox_Live_Body(OrderByClause As String, AllowChildList As Boolean, Authoring As Boolean, rootPageId As Integer, AllowReturnLink As Boolean, RootPageContentName As String, ArchivePage As Boolean) As String
            Dim result As String = ""
            Try
                'Dim Cell As String
                'Dim AddonStatusOK As Boolean
                'Dim ChildListInstanceOptions As String
                'Dim Name As String
                'Dim DateReviewed As Date
                'Dim ReviewedBy As Integer
                'Dim CS As Integer
                'Dim IconRow As String
                ''Dim contactMemberID As Integer
                'Dim QueryString As String
                'Dim LastModified As Date
                'Dim childListSortMethodId As Integer
                'Dim AllowEmailPage As Boolean
                'Dim AllowPrinterVersion As Boolean
                'Dim Caption As String
                ''Dim PageID As Integer
                ''Dim parentPageID As Integer
                'Dim allowChildListDisplay As Boolean
                'Dim dateArchive As Date

                '
                'Dim allowReturnLinkDisplay As Boolean
                'Dim headline As String
                'Dim copyFilename As String
                'Dim Copy As String
                'Dim EmailBody As String
                'Dim Body As String
                'Dim allowSeeAlso As Boolean
                'Dim allowMoreInfo As Boolean
                'Dim allowFeedback As Boolean
                'Dim allowLastModifiedFooter As Boolean
                'Dim ModifiedBy As Integer
                'Dim allowReviewedFooter As Boolean
                'Dim allowMessageFooter As Boolean
                'Dim pageContentMessageFooter As String
                '
                Dim allowChildListComposite As Boolean = AllowChildList And page.AllowChildListDisplay
                Dim allowReturnLinkComposite As Boolean = AllowReturnLink And page.AllowReturnLinkDisplay
                Dim bodyCopy As String = page.Copyfilename.copy
                'If page.Copyfilename <> "" Then
                '    bodyCopy = cpcore.privateFiles.readFile(page.Copyfilename)
                'End If
                '
                Dim breadCrumb As String = ""
                Dim BreadCrumbDelimiter As String
                Dim BreadCrumbPrefix As String
                Dim isRootPage As Boolean = pageToRootList.Count.Equals(1)
                '
                If allowReturnLinkComposite And (Not isRootPage) And (Not cpcore.htmlDoc.pageManager_printVersion) Then
                    '
                    ' ----- Print Heading if not at root Page
                    '
                    BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to")
                    BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ")
                    breadCrumb = pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink(RootPageContentName, page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter)
                    If breadCrumb <> "" Then
                        breadCrumb = cr & "<p class=""ccPageListNavigation"">" & BreadCrumbPrefix & " " & breadCrumb & "</p>"
                    End If
                End If
                result = result & breadCrumb
                '
                If (Not cpcore.htmlDoc.pageManager_printVersion) Then
                    Dim IconRow As String = ""
                    If (Not cpcore.authContext.visit.Bot) And (page.AllowPrinterVersion Or page.AllowEmailPage) Then
                        '
                        ' not a bot, and either print or email allowed
                        '
                        If page.AllowPrinterVersion Then
                            Dim QueryString As String = cpcore.htmlDoc.refreshQueryString
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(page.id), True)
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            Dim Caption As String = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a href=""" & cpcore.htmlDoc.html_EncodeHTML(cpcore.webServer.webServerIO_requestPage & "?" & QueryString) & """ target=""_blank""><img alt=""image"" src=""/ccLib/images/IconSmallPrinter.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp<a href=""" & cpcore.htmlDoc.html_EncodeHTML(cpcore.webServer.webServerIO_requestPage & "?" & QueryString) & """ target=""_blank"" style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                        If page.AllowEmailPage Then
                            Dim QueryString As String = cpcore.htmlDoc.refreshQueryString
                            If QueryString <> "" Then
                                QueryString = "?" & QueryString
                            End If
                            Dim EmailBody As String = cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.requestDomain & cpcore.webServer.requestPathPage & QueryString
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
                If cpcore.authContext.isQuickEditing(cpcore, pageContentModel.contentName) Then
                    Cell = Cell & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing(pageContentModel.contentName, rootPageId, RootPageContentName, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, page.ContactMemberID, page.ChildListSortMethodID, allowChildListComposite, ArchivePage)
                Else
                    '
                    ' ----- Headline
                    '
                    If page.Headline <> "" Then
                        Dim headline As String = cpcore.htmlDoc.main_encodeHTML(page.Headline)
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
                        If cpcore.authContext.isEditing(cpcore, pageContentModel.contentName) Then
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
                    If allowChildListComposite Or cpcore.authContext.isEditingAnything(cpcore) Then
                        If Not allowChildListComposite Then
                            Cell = Cell & cpcore.htmlDoc.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.")
                        End If
                        Dim AddonStatusOK As Boolean = False
                        Cell = Cell & cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, cpcore.siteProperties.defaultWrapperID, "", AddonStatusOK, Nothing)
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
                If page.AllowSeeAlso Then
                    result = result _
                        & cr & "<div>" _
                        & genericController.htmlIndent(main_GetSeeAlso(cpcore, pageContentModel.contentName, page.id)) _
                        & cr & "</div>"
                End If
                '
                ' ----- Allow More Info
                If (page.ContactMemberID <> 0) And page.AllowMoreInfo Then
                    result = result & cr & "<ac TYPE=""" & ACTypeContact & """>"
                End If
                '
                ' ----- Feedback
                If (Not cpcore.htmlDoc.pageManager_printVersion) And (page.ContactMemberID <> 0) And page.AllowFeedback Then
                    result = result & cr & "<ac TYPE=""" & ACTypeFeedback & """>"
                End If
                '
                ' ----- Last Modified line
                If (page.ModifiedDate <> Date.MinValue) And page.AllowLastModifiedFooter Then
                    result = result & cr & "<p>This page was last modified " & FormatDateTime(page.ModifiedDate)
                    If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                        If page.ModifiedBy = 0 Then
                            result = result & " (admin only: modified by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", page.ModifiedBy)
                            If personName = "" Then
                                result = result & " (admin only: modified by person with unnamed or deleted record #" & page.ModifiedBy & ")"
                            Else
                                result = result & " (admin only: modified by " & personName & ")"
                            End If
                        End If
                    End If
                    result = result & "</p>"
                End If
                '
                ' ----- Last Reviewed line
                If (page.DateReviewed <> Date.MinValue) And page.AllowReviewedFooter Then
                    result = result & cr & "<p>This page was last reviewed " & FormatDateTime(page.DateReviewed, vbLongDate)
                    If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                        If page.ReviewedBy = 0 Then
                            result = result & " (by unknown)"
                        Else
                            Dim personName As String = cpcore.db.getRecordName("people", page.ReviewedBy)
                            If personName = "" Then
                                result = result & " (by person with unnamed or deleted record #" & page.ReviewedBy & ")"
                            Else
                                result = result & " (by " & personName & ")"
                            End If
                        End If
                        result = result & ".</p>"
                    End If
                End If
                '
                ' ----- Page Content Message Footer
                If page.AllowMessageFooter Then
                    Dim pageContentMessageFooter As String = cpcore.siteProperties.getText("PageContentMessageFooter", "")
                    If (pageContentMessageFooter <> "") Then
                        result = result & cr & "<p>" & pageContentMessageFooter & "</p>"
                    End If
                End If
                'Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
            Return result
        End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' future pageManager addon interface
        '''' </summary>
        '''' <param name="cp"></param>
        '''' <returns></returns>
        'Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
        '    Dim returnHtml As String = ""
        '    Try
        '        '
        '        '
        '        '
        '    Catch ex As Exception
        '        cp.Site.ErrorReport(ex)
        '    End Try
        '    Return returnHtml
        'End Function
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
                    ContentID = Me.cpcore.metaData.getContentId(iContentName)
                    If (ContentID > 0) Then
                        '
                        ' ----- Set authoring only for valid ContentName
                        '
                        IsEditingLocal = Me.cpcore.authContext.isEditing(cpcore, iContentName)
                    Else
                        '
                        ' ----- if iContentName was bad, maybe they put table in, no authoring
                        '
                        ContentID = Me.cpcore.metaData.GetContentIDByTablename(iContentName)
                    End If
                    If (ContentID > 0) Then
                        '
                        CS = Me.cpcore.db.cs_open("See Also", "((active<>0)AND(ContentID=" & ContentID & ")AND(RecordID=" & iRecordID & "))")
                        Do While (cpcore.db.cs_ok(CS))
                            SeeAlsoLink = (cpcore.db.cs_getText(CS, "Link"))
                            If SeeAlsoLink <> "" Then
                                result = result & cr & "<li class=""ccListItem"">"
                                If genericController.vbInstr(1, SeeAlsoLink, "://") = 0 Then
                                    SeeAlsoLink = Me.cpcore.webServer.webServerIO_requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & Me.cpcore.htmlDoc.main_GetRecordEditLink2("See Also", (cpcore.db.cs_getInteger(CS, "ID")), False, "", Me.cpcore.authContext.isEditing(cpcore, "See Also"))
                                End If
                                result = result & "<a href=""" & Me.cpcore.htmlDoc.html_EncodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.cs_getText(CS, "Name")) & "</A>"
                                Copy = (cpcore.db.cs_getText(CS, "Brief"))
                                If Copy <> "" Then
                                    result = result & "<br >" & AddSpan(Copy, "ccListCopy")
                                End If
                                SeeAlsoCount = SeeAlsoCount + 1
                                result = result & "</li>"
                            End If
                            Me.cpcore.db.cs_goNext(CS)
                        Loop
                        Me.cpcore.db.cs_Close(CS)
                        '
                        If IsEditingLocal Then
                            SeeAlsoCount = SeeAlsoCount + 1
                            result = result & cr & "<li class=""ccListItem"">" & Me.cpcore.htmlDoc.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
                        End If
                    End If
                    '
                    If SeeAlsoCount = 0 Then
                        result = ""
                    Else
                        result = "<p>See Also" & cr & "<ul class=""ccList"">" & htmlIndent(result) & cr & "</ul></p>"
                    End If
                End If
            Catch ex As Exception
                Me.cpcore.handleExceptionAndContinue(ex)
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
                Me.cpcore.handleExceptionAndContinue(ex)
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
                FeedbackButton = Me.cpcore.docProperties.getText("fbb")
                Select Case FeedbackButton
                    Case FeedbackButtonSubmit
                        '
                        ' ----- form was submitted, save the note, send it and say thanks
                        '
                        NoteFromName = Me.cpcore.docProperties.getText("NoteFromName")
                        NoteFromEmail = Me.cpcore.docProperties.getText("NoteFromEmail")
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
                        Copy = Me.cpcore.docProperties.getText("NoteCopy")
                        If (Copy = "") Then
                            NoteCopy = NoteCopy & "[no comments entered]" & BR
                        Else
                            NoteCopy = NoteCopy & Me.cpcore.htmlDoc.main_EncodeCRLF(Copy) & BR
                        End If
                        '
                        NoteCopy = NoteCopy & BR
                        NoteCopy = NoteCopy & "<b>Content on which the comments are based</b>" & BR
                        '
                        CS = Me.cpcore.db.cs_open(iContentName, "ID=" & iRecordID)
                        Copy = "[the content of this page is not available]" & BR
                        If Me.cpcore.db.cs_ok(CS) Then
                            Copy = (cpcore.db.cs_get(CS, "copyFilename"))
                            'Copy = main_EncodeContent5(Copy, c.authcontext.user.userid, iContentName, iRecordID, 0, False, False, True, True, False, True, "", "", False, 0)
                        End If
                        NoteCopy = NoteCopy & Copy & BR
                        Call Me.cpcore.db.cs_Close(CS)
                        '
                        Call Me.cpcore.email.sendPerson(iToMemberID, NoteFromEmail, "Feedback Form Submitted", NoteCopy, False, True, 0, "", False)
                        '
                        ' ----- Note sent, say thanks
                        '
                        main_GetFeedbackForm = main_GetFeedbackForm & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & Me.cpcore.webServer.webServerIO_ServerFormActionURL & "?" & Me.cpcore.htmlDoc.refreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = Me.cpcore.authContext.user.Name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & Me.cpcore.htmlDoc.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = Me.cpcore.authContext.user.Email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & Me.cpcore.htmlDoc.html_EncodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & Me.cpcore.htmlDoc.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
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
                Me.cpcore.handleExceptionAndContinue(ex)
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
                Me.cpcore.handleExceptionAndContinue(ex)
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
                & " WHERE (((ccContentWatchLists.Name)=" & Me.cpcore.db.encodeSQLText(ListName) & ")" _
                    & "AND ((ccContentWatchLists.Active)<>0)" _
                    & "AND ((ccContentWatchListRules.Active)<>0)" _
                    & "AND ((ccContentWatch.Active)<>0)" _
                    & "AND (ccContentWatch.Link is not null)" _
                    & "AND (ccContentWatch.LinkLabel is not null)" _
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & Me.cpcore.db.encodeSQLDate(cpcore.app_startTime) & "))" _
                    & ")" _
                & " ORDER BY " & iSortFieldList & ";"
                result = Me.cpcore.db.cs_openSql(SQL, , PageSize, PageNumber)
                If Not Me.cpcore.db.cs_ok(result) Then
                    '
                    ' Check if listname exists
                    '
                    CS = Me.cpcore.db.cs_open("Content Watch Lists", "name=" & Me.cpcore.db.encodeSQLText(ListName), "ID", , , , , "ID")
                    If Not Me.cpcore.db.cs_ok(CS) Then
                        Call Me.cpcore.db.cs_Close(CS)
                        CS = Me.cpcore.db.cs_insertRecord("Content Watch Lists")
                        If Me.cpcore.db.cs_ok(CS) Then
                            Call Me.cpcore.db.cs_set(CS, "name", ListName)
                        End If
                    End If
                    Call Me.cpcore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                Me.cpcore.handleExceptionAndContinue(ex)
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
                If Me.cpcore.db.cs_ok(CSPointer) Then
                    ContentID = Me.cpcore.metaData.getContentId("Content Watch")
                    Do While Me.cpcore.db.cs_ok(CSPointer)
                        Link = Me.cpcore.db.cs_getText(CSPointer, "link")
                        LinkLabel = Me.cpcore.db.cs_getText(CSPointer, "LinkLabel")
                        RecordID = Me.cpcore.db.cs_getInteger(CSPointer, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & genericController.csv_GetLinkedText("<a href=""" & Me.cpcore.htmlDoc.html_EncodeHTML(cpcore.webServer.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID) & """>", LinkLabel)
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call Me.cpcore.db.cs_goNext(CSPointer)
                    Loop
                    result = cr & "<ul class=""ccWatchList"">" & htmlIndent(result) & cr & "</ul>"
                End If
                Call Me.cpcore.db.cs_Close(CSPointer)
            Catch ex As Exception
                Me.cpcore.handleExceptionAndContinue(ex)
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
                CS = Me.cpcore.db.cs_openContentRecord("People", PeopleID, , , , "Name,Phone,Email")
                If Me.cpcore.db.cs_ok(CS) Then
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
                Call Me.cpcore.db.cs_Close(CS)
                '
                result = Copy
            Catch ex As Exception
                Me.cpcore.handleExceptionAndContinue(ex)
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
                If Me.cpcore.db.cs_ok(CS) Then
                    ContentID = Me.cpcore.metaData.getContentId("Content Watch")
                    Do While Me.cpcore.db.cs_ok(CS)
                        Link = Me.cpcore.db.cs_getText(CS, "link")
                        LinkLabel = Me.cpcore.db.cs_getText(CS, "LinkLabel")
                        RecordID = Me.cpcore.db.cs_getInteger(CS, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li id=""main_ContentWatch" & RecordID & """ class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & "<a href=""http://" & Me.cpcore.webServer.webServerIO_requestDomain & requestAppRootPath & Me.cpcore.webServer.webServerIO_requestPage & "?rc=" & ContentID & "&ri=" & RecordID & """>" & LinkLabel & "</a>"
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call Me.cpcore.db.cs_goNext(CS)
                    Loop
                    If result <> "" Then
                        result = Me.cpcore.htmlDoc.html_GetContentCopy("Watch List Caption: " & ListName, ListName, Me.cpcore.authContext.user.id, True, Me.cpcore.authContext.isAuthenticated) & cr & "<ul class=""ccWatchList"">" & htmlIndent(result) & cr & "</ul>"
                    End If
                End If
                Call Me.cpcore.db.cs_Close(CS)
                '
                If Me.cpcore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                    result = Me.cpcore.htmlDoc.main_GetEditWrapper("Watch List [" & ListName & "]", result)
                End If
            Catch ex As Exception
                Me.cpcore.handleExceptionAndContinue(ex)
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
                If Not siteStructure_LocalLoaded Then
                    siteStructure = Me.cpcore.addon.execute_legacy2(0, addonSiteStructureGuid, "", CPUtilsBaseClass.addonContext.ContextSimple, "", 0, "", "", False, -1, "", returnStatus, Nothing)
                    siteStructure_LocalLoaded = True
                End If
                main_SiteStructure = siteStructure

            End Get
        End Property
        '
        '=============================================================================
        '   Content Page Authoring
        '
        '   Display Quick Editor for the first active record found
        '   Use for both Root and non-root pages
        '=============================================================================
        '
        Friend Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing(LiveRecordContentName As String, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, contactMemberID As Integer, childListSortMethodId As Integer, main_AllowChildListComposite As Boolean, ArchivePage As Boolean) As String
            Dim result As String
            '
            Dim AddonStatusOK As Boolean
            Dim Link As String
            Dim page_ParentID As Integer
            Dim PageList As String
            Dim OptionsPanelAuthoringStatus As String
            Dim ButtonList As String
            Dim AllowInsert As Boolean
            Dim AllowCancel As Boolean
            Dim allowSave As Boolean
            Dim AllowDelete As Boolean
            Dim AllowPublish As Boolean
            Dim AllowAbort As Boolean
            Dim AllowSubmit As Boolean
            Dim AllowApprove As Boolean
            Dim AllowMarkReviewed As Boolean
            Dim CDef As cdefModel
            Dim readOnlyField As Boolean
            Dim IsEditLocked As Boolean
            Dim main_EditLockMemberName As String
            Dim main_EditLockDateExpires As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            Dim ModifiedDate As Date
            '
            Call cpcore.htmlDoc.main_AddStylesheetLink2("/ccLib/styles/ccQuickEdit.css", "Quick Editor")
            '
            ' ----- First Active Record - Output Quick Editor form
            '
            CDef = cpcore.metaData.getCdef(LiveRecordContentName)
            '
            ' main_Get Authoring Status and permissions
            '
            IsEditLocked = cpcore.workflow.GetEditLockStatus(LiveRecordContentName, page.id)
            If IsEditLocked Then
                main_EditLockMemberName = cpcore.workflow.GetEditLockMemberName(LiveRecordContentName, page.id)
                main_EditLockDateExpires = genericController.EncodeDate(cpcore.workflow.GetEditLockMemberName(LiveRecordContentName, page.id))
            End If
            Dim IsModified As Boolean = False
            Dim IsSubmitted As Boolean = False
            Dim IsApproved As Boolean = False
            Dim SubmittedMemberName As String = ""
            Dim ApprovedMemberName As String = ""
            Dim ModifiedMemberName As String = ""
            Dim IsDeleted As Boolean = False
            Dim IsInserted As Boolean = False
            Dim IsRootPage As Boolean = False
            Call pageManager_GetAuthoringStatus(LiveRecordContentName, page.id, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
            Call pageManager_GetAuthoringPermissions(LiveRecordContentName, page.id, AllowInsert, AllowCancel, allowSave, AllowDelete, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, readOnlyField)
            AllowMarkReviewed = cpcore.metaData.isContentFieldSupported(Models.Entity.pageContentModel.contentName, "DateReviewed")
            OptionsPanelAuthoringStatus = cpcore.authContext.main_GetAuthoringStatusMessage(cpcore, CDef.AllowWorkflowAuthoring, IsEditLocked, main_EditLockMemberName, main_EditLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName)
            '
            ' Set Editing Authoring Control
            '
            Call cpcore.workflow.SetEditLock(LiveRecordContentName, page.id)
            '
            ' SubPanel: Authoring Status
            '
            ButtonList = ""
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
            If (page_ParentID <> 0) And AllowInsert Then
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
                ButtonList = cpcore.htmlDoc.main_GetPanelButtons(ButtonList, "Button")
            End If
            If OptionsPanelAuthoringStatus <> "" Then
                result = result & "" _
            & cr & "<tr>" _
            & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
            & cr & "</tr>"
            End If
            If (cpcore.debug_iUserError <> "") Then
                result = result & "" _
            & cr & "<tr>" _
            & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & errorController.error_GetUserError(cpcore) & "</div></td>" _
            & cr & "</tr>"
            End If
            result = result _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Name</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & cpcore.htmlDoc.html_GetFormInputText2("name", page.name, 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Headline</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & cpcore.htmlDoc.html_GetFormInputText2("headline", page.Headline, 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & ""
            If readOnlyField Then
                result = result & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:34px;"">Body</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
            & cr & "</tr>"
            Else
                result = result & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:111px;"">Body</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
            & cr & "</tr>"
            End If
            '
            ' ----- Parent pages
            '
            If pageToRootList.Count = 1 Then
                PageList = "&nbsp;(there are no parent pages)"
            Else
                PageList = "<ul class=""qeListUL""><li class=""qeListLI"">Current Page</li></ul>"
                For Each testPage As pageContentModel In Enumerable.Reverse(pageToRootList)
                    Link = testPage.name
                    If Link = "" Then
                        Link = "no name #" & genericController.encodeText(testPage.id)
                    End If
                    Link = "<a href=""" & testPage.PageLink & """>" & Link & "</a>"
                    PageList = "<ul class=""qeListUL""><li class=""qeListLI"">" & Link & PageList & "</li></ul>"
                Next
            End If
            result = result & "" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:26px;"">Parent Pages</td>" _
            & cr2 & "<td class=""qeRow qeRight""><div class=""qeListCon"">" & PageList & "</div></td>" _
            & cr & "</tr>"
            '
            ' ----- Child pages
            '
            PageList = cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
            If genericController.vbInstr(1, PageList, "<ul", vbTextCompare) = 0 Then
                PageList = "(there are no child pages)"
            End If
            result = result _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:36px;"">Child Pages</td>" _
            & cr2 & "<td class=""qeRow qeRight""><div class=""qeListCon"">" & PageList & "</div></td>" _
            & cr & "</tr>"
            result = "" _
            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
            & genericController.htmlIndent(result) _
            & cr & "</table>"
            result = "" _
            & ButtonList _
            & result _
            & ButtonList
            result = cpcore.htmlDoc.main_GetPanel(result)

            '
            ' Form Wrapper
            '
            result = "" _
            & cr & cpcore.htmlDoc.html_GetUploadFormStart(cpcore.webServer.requestQueryString) _
            & cr & cpcore.htmlDoc.html_GetFormInputHidden("Type", FormTypePageAuthoring) _
            & cr & cpcore.htmlDoc.html_GetFormInputHidden("ID", page.id) _
            & cr & cpcore.htmlDoc.html_GetFormInputHidden("ContentName", LiveRecordContentName) _
            & cr & cpcore.htmlDoc.main_GetPanelHeader("Contensive Quick Editor") _
            & cr & result _
            & cr & cpcore.htmlDoc.html_GetUploadFormEnd()

            pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing = "" _
            & cr & "<div class=""ccCon"">" _
            & genericController.htmlIndent(result) _
            & cr & "</div>"
            '
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Friend Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_Body(ByVal ContentName As String, ByVal OrderByClause As String, ByVal AllowChildList As Boolean, ByVal Authoring As Boolean, ByVal rootPageId As Integer, ByVal readOnlyField As Boolean, ByVal AllowReturnLink As Boolean, ByVal RootPageContentName As String, ByVal ArchivePage As Boolean, ByVal contactMemberID As Integer) As String
            Dim pageCopy As String = page.Copyfilename.copy
            'If page.Copyfilename <> "" Then
            '    pageCopy = page.Copyfilename.copy(cpcore)
            '    'pageCopy = cpcore.cdnFiles.readFile(page.Copyfilename)
            'End If
            '
            ' ----- Page Copy
            '
            Dim FieldRows As Integer = cpcore.userProperty.getInteger(ContentName & ".copyFilename.PixelHeight", 500)
            If FieldRows < 50 Then
                FieldRows = 50
                Call cpcore.userProperty.setProperty(ContentName & ".copyFilename.PixelHeight", FieldRows)
            End If
            '
            ' At this point we do now know the the template so we can not main_Get the stylelist.
            ' Put in main_fpo_QuickEditing to be replaced after template known
            '
            cpcore.htmlDoc.html_quickEdit_copy = pageCopy
            Return html_quickEdit_fpo
        End Function
        '
        '=============================================================================
        ' pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink
        '=============================================================================
        '
        Friend Function pageManager_GetHtmlBody_GetSection_GetContentBox_ReturnLink(RootPageContentName As String, ignore As Integer, rootPageId As Integer, ParentIDPath As String, ArchivePage As Boolean, BreadCrumbDelimiter As String) As String
            Dim returnHtml As String = ""
            '
            For Each testpage As pageContentModel In Enumerable.Reverse(pageToRootList)
                Dim pageCaption As String = testpage.MenuHeadline
                If pageCaption = "" Then
                    pageCaption = genericController.encodeText(testpage.name)
                End If
                If returnHtml <> "" Then
                    returnHtml = BreadCrumbDelimiter & returnHtml
                End If
                returnHtml = "<a href=""" & cpcore.htmlDoc.html_EncodeHTML(getPageLink4(page.id, "", True, False)) & """>" & pageCaption & "</a>" & returnHtml
            Next
            Return returnHtml
        End Function
        '
        '
        '
        Friend Function pageManager_GetHtmlBody_GetSection_GetContent_GetTableRow(ByVal Caption As String, ByVal Result As String, ByVal EvenRow As Boolean) As String
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
        Friend Function pageManager_GetContentBoxWrapper(ByVal Content As String, ByVal ContentPadding As Integer) As String
            'dim buildversion As String
            '
            ' BuildVersion = app.dataBuildVersion
            pageManager_GetContentBoxWrapper = Content
            If cpcore.siteProperties.getBoolean("Compatibility ContentBox Pad With Table") Then
                '
                If ContentPadding > 0 Then
                    '
                    pageManager_GetContentBoxWrapper = "" _
                        & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0>" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td style=""padding:" & ContentPadding & "px"">" _
                        & genericController.htmlIndent(genericController.htmlIndent(genericController.htmlIndent(pageManager_GetContentBoxWrapper))) _
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
                    & genericController.htmlIndent(pageManager_GetContentBoxWrapper) _
                    & cr & "</div>"
            Else
                '
                pageManager_GetContentBoxWrapper = "" _
                    & cr & "<div class=""contentBox"" style=""padding:" & ContentPadding & "px"">" _
                    & genericController.htmlIndent(pageManager_GetContentBoxWrapper) _
                    & cr & "</div>"
            End If
        End Function
        ' $$$$$ not used
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'friend Function pageManager_GetHtmlBody_GetSection_GetContentBox_QuickEditing_BodyInput(Caption As String, FormInput As String) As String
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
            '
            Dim RecordParentID As Integer
            Dim SaveButNoChanges As Boolean
            Dim RequestName As String
            Dim ParentID As Integer
            Dim Link As String
            Dim FieldName As String
            Dim CSBlock As Integer
            Dim Copy As String
            Dim Button As String
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
            Dim main_EditLockDateExpires As Date
            Dim ModifiedDate As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            Dim allowSave As Boolean
            Dim iIsAdmin As Boolean
            Dim main_WorkflowSupport As Boolean
            '
            RecordModified = False
            RecordID = (cpcore.docProperties.getInteger("ID"))
            Button = cpcore.docProperties.getText("Button")
            iIsAdmin = cpcore.authContext.isAuthenticatedAdmin(cpcore)
            '
            If (Button <> "") And (RecordID <> 0) And (pageContentModel.contentName <> "") And (cpcore.authContext.isAuthenticatedContentManager(cpcore, pageContentModel.contentName)) Then
                main_WorkflowSupport = cpcore.siteProperties.allowWorkflowAuthoring And cpcore.workflow.isWorkflowAuthoringCompatible(pageContentModel.contentName)
                Dim SubmittedMemberName As String = ""
                Dim ApprovedMemberName As String = ""
                Dim ModifiedMemberName As String = ""
                Call pageManager_GetAuthoringStatus(pageContentModel.contentName, RecordID, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
                IsEditLocked = cpcore.workflow.GetEditLockStatus(pageContentModel.contentName, RecordID)
                main_EditLockMemberName = cpcore.workflow.GetEditLockMemberName(pageContentModel.contentName, RecordID)
                main_EditLockDateExpires = cpcore.workflow.GetEditLockDateExpires(pageContentModel.contentName, RecordID)
                Call cpcore.workflow.ClearEditLock(pageContentModel.contentName, RecordID)
                '
                ' tough case, in Quick mode, lets mark the record reviewed, no matter what button they push, except cancel
                '
                If Button <> ButtonCancel Then
                    Call cpcore.db.markRecordReviewed(pageContentModel.contentName, RecordID)
                End If
                '
                ' Determine is the record should be saved
                '
                If (Not IsApproved) And (Not cpcore.docProperties.getBoolean("RENDERMODE")) Then
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
                    RequestName = cpcore.docProperties.getText("name")
                    If Trim(RequestName) = "" Then
                        Call errorController.error_AddUserError(cpcore, "A name is required to save this page")
                    Else
                        CSBlock = cpcore.db.cs_open2(pageContentModel.contentName, RecordID, True, True)
                        If cpcore.db.cs_ok(CSBlock) Then
                            FieldName = "copyFilename"
                            Copy = cpcore.docProperties.getText(FieldName)
                            Copy = cpcore.htmlDoc.html_DecodeContent(Copy)
                            If Copy <> cpcore.db.cs_get(CSBlock, "copyFilename") Then
                                Call cpcore.db.cs_set(CSBlock, "copyFilename", Copy)
                                SaveButNoChanges = False
                            End If
                            RecordName = cpcore.docProperties.getText("name")
                            If RecordName <> cpcore.db.cs_get(CSBlock, "name") Then
                                Call cpcore.db.cs_set(CSBlock, "name", RecordName)
                                SaveButNoChanges = False
                            End If
                            Call pagesController.app_addLinkAlias2(cpcore, RecordName, RecordID, "")
                            If (cpcore.docProperties.getText("headline") <> cpcore.db.cs_get(CSBlock, "headline")) Then
                                Call cpcore.db.cs_set(CSBlock, "headline", cpcore.docProperties.getText("headline"))
                                SaveButNoChanges = False
                            End If
                            RecordParentID = cpcore.db.cs_getInteger(CSBlock, "parentid")
                        End If
                        Call cpcore.db.cs_Close(CSBlock)
                        '
                        Call cpcore.workflow.SetEditLock(pageContentModel.contentName, RecordID)
                        '
                        If Not SaveButNoChanges Then
                            Call cpcore.db.main_ProcessSpecialCaseAfterSave(False, pageContentModel.contentName, RecordID, RecordName, RecordParentID, False)
                            Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                        End If
                    End If
                End If
                If (Button = ButtonAddChildPage) Then
                    '
                    '
                    '
                    CSBlock = cpcore.db.cs_insertRecord(pageContentModel.contentName)
                    If cpcore.db.cs_ok(CSBlock) Then
                        Call cpcore.db.cs_set(CSBlock, "active", True)
                        Call cpcore.db.cs_set(CSBlock, "ParentID", RecordID)
                        Call cpcore.db.cs_set(CSBlock, "contactmemberid", cpcore.authContext.user.id)
                        Call cpcore.db.cs_set(CSBlock, "name", "New Page added " & cpcore.app_startTime & " by " & cpcore.authContext.user.Name)
                        Call cpcore.db.cs_set(CSBlock, "copyFilename", "")
                        RecordID = cpcore.db.cs_getInteger(CSBlock, "ID")
                        Call cpcore.db.cs_save2(CSBlock)
                        '
                        Link = getPageLink4(RecordID, "", True, False)
                        'Link = main_GetPageLink(RecordID)
                        If main_WorkflowSupport Then
                            If Not pagemanager_IsWorkflowRendering() Then
                                Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                                Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
                            End If
                        End If
                        Call cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", False)
                    End If
                    Call cpcore.db.cs_Close(CSBlock)
                    '
                    'Call AppendLog("pageManager_ProcessFormQuickEditor, 7-call pageManager_cache_pageContent_clear")
                    Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                End If
                If (Button = ButtonAddSiblingPage) Then
                    '
                    '
                    '
                    CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID, , , "ParentID")
                    If cpcore.db.cs_ok(CSBlock) Then
                        ParentID = cpcore.db.cs_getInteger(CSBlock, "ParentID")
                    End If
                    Call cpcore.db.cs_Close(CSBlock)
                    If ParentID <> 0 Then
                        CSBlock = cpcore.db.cs_insertRecord(pageContentModel.contentName)
                        If cpcore.db.cs_ok(CSBlock) Then
                            Call cpcore.db.cs_set(CSBlock, "active", True)
                            Call cpcore.db.cs_set(CSBlock, "ParentID", ParentID)
                            Call cpcore.db.cs_set(CSBlock, "contactmemberid", cpcore.authContext.user.id)
                            Call cpcore.db.cs_set(CSBlock, "name", "New Page added " & cpcore.app_startTime & " by " & cpcore.authContext.user.Name)
                            Call cpcore.db.cs_set(CSBlock, "copyFilename", "")
                            RecordID = cpcore.db.cs_getInteger(CSBlock, "ID")
                            Call cpcore.db.cs_save2(CSBlock)
                            '
                            Link = getPageLink4(RecordID, "", True, False)
                            'Link = main_GetPageLink(RecordID)
                            If main_WorkflowSupport Then
                                If Not pagemanager_IsWorkflowRendering() Then
                                    Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                                    Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
                                End If
                            End If
                            Call cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", False)
                        End If
                        Call cpcore.db.cs_Close(CSBlock)
                    End If
                    Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                End If
                If (Button = ButtonDelete) Then
                    CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID)
                    If cpcore.db.cs_ok(CSBlock) Then
                        ParentID = cpcore.db.cs_getInteger(CSBlock, "parentid")
                    End If
                    Call cpcore.db.cs_Close(CSBlock)
                    '
                    Call pageManager_DeleteChildRecords(pageContentModel.contentName, RecordID, False)
                    Call cpcore.db.deleteContentRecord(pageContentModel.contentName, RecordID)
                    '
                    If Not main_WorkflowSupport Then
                        Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                    End If
                    '
                    If Not main_WorkflowSupport Then
                        Link = getPageLink4(ParentID, "", True, False)
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", True)
                        Call cpcore.webServer.redirect(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", pageManager_RedirectBecausePageNotFound)
                        Exit Sub
                    End If
                End If
                '
                If (Button = ButtonAbortEdit) Then
                    Call cpcore.workflow.abortEdit2(pageContentModel.contentName, RecordID, cpcore.authContext.user.id)
                End If
                If (Button = ButtonPublishSubmit) Then
                    Call cpcore.workflow.main_SubmitEdit(pageContentModel.contentName, RecordID)
                    Call pageManager_SendPublishSubmitNotice(pageContentModel.contentName, RecordID, "")
                End If
                If (Not (cpcore.debug_iUserError <> "")) And ((Button = ButtonOK) Or (Button = ButtonCancel) Or (Button = ButtonPublish)) Then
                    '
                    ' ----- Turn off Quick Editor if not save or add child
                    '
                    Call cpcore.visitProperty.setProperty("AllowQuickEditor", "0")
                End If
                If iIsAdmin Then
                    '
                    ' ----- Admin only functions
                    '
                    If (Button = ButtonPublish) Then
                        Call cpcore.workflow.publishEdit(pageContentModel.contentName, RecordID)
                        Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                    End If
                    If (Button = ButtonPublishApprove) Then
                        Call cpcore.workflow.approveEdit(pageContentModel.contentName, RecordID)
                    End If
                End If
            End If
        End Sub
        '        '
        '        '======================================================================================
        '        '   main_Get a dynamic menu from Page Content
        '        '======================================================================================
        '        '
        '        Friend Function pageManager_GetSectionMenu_NameMenu(ByVal PageName As String, ByVal ContentName As String, ByVal DefaultLink As String, ByVal RootPageRecordID As Integer, ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal MenuImage As String, ByVal MenuImageOver As String, ByVal RootMenuCaption As String, ByVal SectionID As Integer, ByVal UseContentWatchLink As Boolean) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00364")
        '            '
        '            Dim AllowInMenus As Boolean
        '            Dim PCCPtr As Integer
        '            Dim PageFound As Boolean
        '            Dim ChildPageCount As Integer
        '            Dim ChildPagesFoundTest As String
        '            Dim AddRootButton As Boolean
        '            Dim TopMenuCaption As String
        '            Dim Tier1MenuCaption As String
        '            '
        '            Dim CSPointer As Integer
        '            Dim MenuID As Integer
        '            Dim ContentID As Integer
        '            Dim BakeName As String
        '            Dim Criteria As String
        '            Dim MenuNamePrefix As String
        '            Dim childListSortMethodId As Integer
        '            Dim LinkWorking As String
        '            Dim ParentID As Integer
        '            Dim templateId As Integer
        '            Dim ContentControlID As Integer
        '            Dim allowChildListDisplay As Boolean
        '            Dim MenuLinkOverRide As String
        '            Dim ChildPagesFound As Boolean
        '            Dim FieldList As String
        '            Dim DateExpires As Date
        '            Dim dateArchive As Date
        '            Dim PubDate As Date
        '            '
        '            '
        '            '
        '            If (PageName = "") Or (ContentName = "") Then
        '                Call Err.Raise(ignoreInteger, "dll", "main_GetPageMenu requires a valid page name and content name")
        '            Else
        '                '
        '                ' ----- Read Bake Version
        '                '
        '                BakeName = "main_GetMenu-" & cpcore.webServer.webServerIO_requestProtocol & "-" & cpcore.webServer.requestDomain & "-" & PageName & "-" & ContentName & "-" & DefaultLink & "-" & RootPageRecordID & "-" & DepthLimit & "-" & MenuStyle & "-" & StyleSheetPrefix
        '                BakeName = genericController.vbReplace(BakeName, "/", "_")
        '                BakeName = genericController.vbReplace(BakeName, ":", "_")
        '                BakeName = genericController.vbReplace(BakeName, ".", "_")
        '                BakeName = genericController.vbReplace(BakeName, " ", "_")
        '                pageManager_GetSectionMenu_NameMenu = genericController.encodeText(cpcore.cache.getObject(Of String)(BakeName))
        '                If pageManager_GetSectionMenu_NameMenu <> "" Then
        '                    pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu
        '                Else
        '                    '
        '                    ' ----- Add Root Page to Menu System
        '                    '
        '                    If RootPageRecordID > 0 Then
        '                        PCCPtr = cache_pageContent_getPtr(RootPageRecordID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                        'Criteria = "(ID=" & encodeSQLNumber(RootPageRecordID) & ")"
        '                    Else
        '                        PCCPtr = cache_pageContent_getFirstNamePtr(PageName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                        'Criteria = "(name=" & encodeSQLText(PageName) & ")"
        '                    End If
        '                    '
        '                    ' Skip over expired, archive and non-published
        '                    '
        '                    PageFound = False
        '                    Do While PCCPtr >= 0 And Not PageFound
        '                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
        '                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
        '                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
        '                        PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > cpcore.app_startTime)) And ((PubDate = Date.MinValue) Or (PubDate < cpcore.app_startTime))
        '                        'PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime)) And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime))
        '                        If (Not PageFound) Then
        '                            If (RootPageRecordID = 0) Then
        '                                PCCPtr = cache_pageContent_nameIndex.getNextPtr
        '                            Else
        '                                PCCPtr = -1
        '                            End If
        '                        End If
        '                    Loop
        '                    If Not PageFound Then
        '                        '
        '                        ' menu root was not found, just put up what we have. If the link is there, the page will be created
        '                        '
        '                        AllowInMenus = True
        '                        LinkWorking = DefaultLink
        '                        LinkWorking = genericController.EncodeAppRootPath(LinkWorking, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
        '                        LinkWorking = genericController.modifyLinkQuery(LinkWorking, rnPageId, "", False)
        '                        MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
        '                        MenuID = 0
        '                        childListSortMethodId = 0
        '                        ParentID = 0
        '                        templateId = 0
        '                        allowChildListDisplay = False
        '                        MenuLinkOverRide = ""
        '                        ChildPagesFound = False
        '                    Else
        '                        AllowInMenus = genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, PCCPtr))
        '                        If AllowInMenus Then
        '                            MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
        '                            MenuID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
        '                            childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
        '                            Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
        '                            If Tier1MenuCaption = "" Then
        '                                Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Headline, PCCPtr))
        '                                If Tier1MenuCaption = "" Then
        '                                    Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
        '                                    If Tier1MenuCaption = "" Then
        '                                        Tier1MenuCaption = "Page " & CStr(MenuID)
        '                                    End If
        '                                End If
        '                            End If
        '                            ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
        '                            templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
        '                            allowChildListDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
        '                            MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
        '                            ChildPagesFoundTest = cache_pageContent(PCC_ChildPagesFound, PCCPtr)
        '                            If ChildPagesFoundTest = "" Then
        '                                '
        '                                ' Not initialized, assume true
        '                                '
        '                                ChildPagesFound = True
        '                            Else
        '                                ChildPagesFound = genericController.EncodeBoolean(ChildPagesFoundTest)
        '                            End If
        '                            '
        '                            ' Use parentid to detect if this record needs to be called with the bid
        '                            '
        '                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
        '                            '
        '                            ' main_Get the Link
        '                            '
        '                            LinkWorking = getPageLink4(MenuID, "", True, False)
        '                            'LinkWorking = main_GetPageDynamicLinkWithArgs(ContentControlID, MenuID, DefaultLink, True, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
        '                        End If
        '                    End If
        '                    '
        '                    If AllowInMenus Then
        '                        '
        '                        ' ----- Set Tier1 Menu Caption (top element of the first flyout panel)
        '                        '
        '                        If Tier1MenuCaption = "" Then
        '                            Tier1MenuCaption = RootMenuCaption
        '                            If Tier1MenuCaption = "" Then
        '                                Tier1MenuCaption = PageName
        '                            End If
        '                        End If
        '                        '
        '                        ' ----- Set Top Menu Caption (clickable label that opens the menus)
        '                        '
        '                        TopMenuCaption = RootMenuCaption
        '                        If TopMenuCaption = "" Then
        '                            TopMenuCaption = Tier1MenuCaption
        '                        End If
        '                        '
        '                        If LinkWorking = "" Then
        '                            '
        '                            ' ----- Blank LinkWorking, this entry has no link
        '                            ' ----- Add menu header, and first entry for the root page
        '                            '
        '                            Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, "", MenuImage, MenuImageOver, , TopMenuCaption)
        '                            '
        '                            ' ----- Root menu only, add a repeat of the button to the first menu
        '                            '
        '                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
        '                                '
        '                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
        '                                '
        '                                Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID & ".entry", MenuNamePrefix & MenuID, , , , Tier1MenuCaption)
        '                            End If
        '                        Else
        '                            '
        '                            ' ----- LinkWorking is here, put MenuID on the end of it
        '                            ' ----- Add menu header, and first entry for the root page
        '                            '
        '                            Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
        '                            '
        '                            ' ----- Root menu only, add a repeat of the button to the first menu
        '                            '
        '                            AddRootButton = False
        '                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
        '                                '
        '                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
        '                                '
        '                                AddRootButton = True
        '                                If ParentID <> 0 Then
        '                                    '
        '                                    ' This Top-most page is not the RootPage, include the bid
        '                                    '
        '                                Else
        '                                    '
        '                                    ' This Top-most page is the RootPage, include no bid
        '                                    '
        '                                End If
        '                            End If
        '                        End If
        '                        ' ##### can not block, this is being used
        '                        If ChildPagesFound Then
        '                            ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(MenuID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
        '                            If (ChildPageCount = 0) And (True) Then
        '                                Call cpcore.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & MenuID)
        '                            End If
        '                        End If
        '                        pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu & genericController.vbReplace(cpcore.menuFlyout.getMenu(MenuNamePrefix & genericController.encodeText(MenuID), MenuStyle, StyleSheetPrefix), vbCrLf, "")
        '                        pageManager_GetSectionMenu_NameMenu = pageManager_GetSectionMenu_NameMenu & cpcore.htmlDoc.menu_GetClose()
        '                        Call cpcore.cache.setObject(BakeName, pageManager_GetSectionMenu_NameMenu, ContentName & ",Site Sections,Dynamic Menus,Dynamic Menu Section Rules")
        '                    End If
        '                End If
        '            End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetSectionMenu_NameMenu")
        '        End Function
        '        '
        '        '======================================================================================
        '        '   main_Get a dynamic menu from Page Content
        '        '======================================================================================
        '        '
        '        Friend Function pageManager_GetSectionMenu_IdMenu(ByVal RootPageRecordID As Integer, ByVal DefaultLink As String, ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal MenuImage As String, ByVal MenuImageOver As String, ByVal RootMenuCaption As String, ByVal SectionID As Integer, ByVal UseContentWatchLink As Boolean) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("PageList_GetPageIDMenu")
        '            '
        '            Dim PseudoChildPagesFound As Boolean
        '            Dim AllowInMenus As Boolean
        '            Dim DateExpires As Date
        '            Dim dateArchive As Date
        '            Dim PubDate As Date
        '            Dim PCCPtr As Integer
        '            Dim PageFound As Boolean
        '            Dim ChildPageCount As Integer
        '            Dim ContentName As String
        '            Dim AddRootButton As Boolean
        '            Dim TopMenuCaption As String
        '            Dim Tier1MenuCaption As String
        '            '
        '            Dim CSPointer As Integer
        '            Dim PageID As Integer
        '            Dim ContentID As Integer
        '            Dim BakeName As String
        '            Dim Criteria As String
        '            Dim MenuNamePrefix As String
        '            Dim childListSortMethodId As Integer
        '            Dim LinkWorking As String
        '            Dim LinkWorkingNoRedirect As String
        '            Dim ParentID As Integer
        '            Dim templateId As Integer
        '            Dim ContentControlID As Integer
        '            Dim allowChildListDisplay As Boolean
        '            Dim MenuLinkOverRide As String
        '            Dim ChildPagesFound As Boolean
        '            Dim FieldList As String
        '            Dim ChildPagesFoundTest As String
        '            '
        '            If RootPageRecordID = 610 Then
        '                RootPageRecordID = RootPageRecordID
        '            End If
        '            '
        '            '$$$$$ cache this - somewhere in here it opens cs with contentname
        '            ContentName = "Page Content"
        '            If False Then
        '                Call Err.Raise(ignoreInteger, "dll", "main_GetPageMenu requires a valid page name and content name")
        '            Else
        '                '
        '                ' ----- Read Bake Version
        '                '
        '                BakeName = "main_GetMenu-" & cpcore.webServer.webServerIO_requestProtocol & "-" & cpcore.webServer.requestDomain & "-" & RootPageRecordID & "-" & DefaultLink & "-" & DepthLimit & "-" & MenuStyle & "-" & StyleSheetPrefix
        '                BakeName = genericController.vbReplace(BakeName, "/", "_")
        '                BakeName = genericController.vbReplace(BakeName, ":", "_")
        '                BakeName = genericController.vbReplace(BakeName, ".", "_")
        '                BakeName = genericController.vbReplace(BakeName, " ", "_")
        '                pageManager_GetSectionMenu_IdMenu = genericController.encodeText(cpcore.cache.getObject(Of String)(BakeName))
        '                If pageManager_GetSectionMenu_IdMenu <> "" Then
        '                    pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu
        '                Else
        '                    '
        '                    ' ----- Add Root Page to Menu System
        '                    '
        '                    PCCPtr = cache_pageContent_getPtr(RootPageRecordID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                    PageFound = (PCCPtr >= 0)
        '                    '
        '                    ' Skip if expired, archive and non-published
        '                    '
        '                    If PageFound Then
        '                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
        '                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
        '                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
        '                        PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > cpcore.app_startTime)) And ((PubDate = Date.MinValue) Or (PubDate < cpcore.app_startTime))
        '                        'PageFound = ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime)) And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime))
        '                    End If
        '                    If Not PageFound Then
        '                        '
        '                        ' menu root was not found, just put up what we have. If the link is there, the page will be created
        '                        '
        '                        AllowInMenus = True
        '                        LinkWorking = DefaultLink
        '                        LinkWorkingNoRedirect = LinkWorking
        '                        LinkWorking = genericController.EncodeAppRootPath(LinkWorking, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
        '                        LinkWorking = genericController.modifyLinkQuery(LinkWorking, rnPageId, "", False)
        '                        MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
        '                        ' ***** just want to know what would happen here
        '                        PageID = RootPageRecordID
        '                        'pageId = 0
        '                        childListSortMethodId = 0
        '                        ParentID = 0
        '                        templateId = 0
        '                        allowChildListDisplay = False
        '                        MenuLinkOverRide = ""
        '                        ChildPagesFound = False
        '                    Else
        '                        '
        '                        ' AllowInMenus does not work for root pages, which are the only pages being handled here. This menu is hidden from the section record
        '                        '
        '                        AllowInMenus = True
        '                        If AllowInMenus Then
        '                            MenuNamePrefix = genericController.encodeText(genericController.GetRandomInteger) & "_"
        '                            PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
        '                            childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
        '                            Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr))
        '                            If Tier1MenuCaption = "" Then
        '                                Tier1MenuCaption = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
        '                                If Tier1MenuCaption = "" Then
        '                                    Tier1MenuCaption = "Page " & CStr(PageID)
        '                                End If
        '                            End If
        '                            ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
        '                            templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
        '                            allowChildListDisplay = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
        '                            MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
        '                            ChildPagesFoundTest = genericController.encodeText(cache_pageContent(PCC_ChildPagesFound, PCCPtr))
        '                            If ChildPagesFoundTest = "" Then
        '                                '
        '                                ' Not initialized, assume true
        '                                '
        '                                ChildPagesFound = True
        '                            Else
        '                                ChildPagesFound = genericController.EncodeBoolean(ChildPagesFoundTest)
        '                            End If
        '                            '
        '                            ' Use parentid to detect if this record needs to be called with the bid
        '                            '
        '                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
        '                            '
        '                            ' main_Get the Link
        '                            '
        '                            '1/13/2010 - convert everything to use linkalias and issecure
        '                            'LinkWorkingNoRedirect = main_GetPageLink4()
        '                            LinkWorkingNoRedirect = getPageLink4(PageID, "", True, False)
        '                            '                    LinkWorkingNoRedirect = main_GetPageDynamicLinkWithArgs(contentcontrolid, pageId, DefaultLink, True, TemplateID, SectionID, "", UseContentWatchLink)
        '                            LinkWorking = LinkWorkingNoRedirect
        '                            '                    If MenuLinkOverRide <> "" Then
        '                            '                        LinkWorking = "?rc=" & contentcontrolid & "&ri=" & pageId
        '                            '                    End If
        '                            'LinkWorking = main_GetPageDynamicLinkWithArgs(ContentControlID, pageId, DefaultLink, True, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
        '                        End If
        '                    End If
        '                    '
        '                    If AllowInMenus Then
        '                        '
        '                        ' ----- Set Tier1 Menu Caption (top element of the first flyout panel)
        '                        '
        '                        If Tier1MenuCaption = "" Then
        '                            Tier1MenuCaption = RootMenuCaption
        '                        End If
        '                        '
        '                        ' ----- Set Top Menu Caption (clickable label that opens the menus)
        '                        '
        '                        TopMenuCaption = RootMenuCaption
        '                        If TopMenuCaption = "" Then
        '                            TopMenuCaption = Tier1MenuCaption
        '                        End If
        '                        '
        '                        If LinkWorking = "" Then
        '                            '
        '                            ' ----- Blank LinkWorking, this entry has no link
        '                            ' ----- Add menu header, and first entry for the root page
        '                            '
        '                            Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, , TopMenuCaption)
        '                            '
        '                            ' ----- Root menu only, add a repeat of the button to the first menu
        '                            '
        '                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
        '                                '
        '                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
        '                                '
        '                                Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID & ".entry", MenuNamePrefix & PageID, , , , Tier1MenuCaption)
        '                            End If
        '                        Else
        '                            '
        '                            ' ----- LinkWorking is here, put pageId on the end of it
        '                            ' ----- Add menu header, and first entry for the root page
        '                            '
        '                            If PageID <> 0 Then
        '                                Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
        '                            ElseIf (SectionID <> 0) And (RootPageRecordID <> 0) Then
        '                                Dim CSSection As Integer
        '                                Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & RootPageRecordID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
        '                                'Dim linkAlias
        '                            Else
        '                                Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & PageID, "", MenuImage, MenuImageOver, LinkWorking, TopMenuCaption)
        '                            End If
        '                            '
        '                            ' ----- Root menu only, add a repeat of the button to the first menu
        '                            '
        '                            AddRootButton = False
        '                            If (MenuStyle < 8) Or (MenuStyle > 11) Then
        '                                '
        '                                ' ##### Josh says Quadrem says they dont like the repeat on hovers
        '                                '
        '                                AddRootButton = True
        '                                If ParentID <> 0 Then
        '                                    '
        '                                    ' This Top-most page is not the RootPage, include the bid
        '                                    '
        '                                Else
        '                                    '
        '                                    ' This Top-most page is the RootPage, include no bid
        '                                    '
        '                                End If
        '                            End If
        '                        End If
        '                        '
        '                        ' 9/18/2009 - Build Submenu if child pages found
        '                        '
        '                        If pagemanager_IsWorkflowRendering() Then
        '                            '
        '                            ' If workflow mode, just assume there are child pages
        '                            '
        '                            ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(PageID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(PageID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
        '                        Else
        '                            '
        '                            ' In production mode, use the ChildPagesFound field
        '                            '
        '                            PseudoChildPagesFound = ChildPagesFound
        '                            If Not PseudoChildPagesFound Then
        '                                '
        '                                ' Even when child pages is false, try it 10% of the time anyway
        '                                '
        '                                Randomize()
        '                                PseudoChildPagesFound = (Rnd() > 0.8)
        '                                If PseudoChildPagesFound Then
        '                                    TopMenuCaption = TopMenuCaption
        '                                End If
        '                            End If
        '                            If PseudoChildPagesFound Then
        '                                '
        '                                ' Child pages were found, create child menu
        '                                '
        '                                ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(PageID, ContentName, LinkWorking, Tier1MenuCaption, "," & genericController.encodeText(PageID), MenuNamePrefix, 1, DepthLimit, childListSortMethodId, SectionID, AddRootButton, UseContentWatchLink)
        '                                'ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(pageId, ContentName, LinkWorkingNoRedirect, Tier1MenuCaption, "," & genericController.encodeText(pageId), MenuNamePrefix, 1, DepthLimit, ChildListSortMethodID, SectionID, AddRootButton, UseContentWatchLink)
        '                                If (True) Then
        '                                    If (ChildPageCount = 0) And (ChildPagesFound) Then
        '                                        '
        '                                        ' ChildPagesFound flag is true, but no pages were found - clear flag
        '                                        '
        '                                        Call cpcore.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & PageID)
        '                                        'Call AppendLog("main_GetSectionMenu_IdMenu, 4-call pageManager_cache_pageContent_updateRow")
        '                                        Call cache_pageContent_updateRow(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                                    ElseIf (ChildPageCount > 0) And (Not ChildPagesFound) Then
        '                                        '
        '                                        ' ChildPagesFlag is cleared, but pages were found -- set the flag
        '                                        '
        '                                        Call cpcore.db.executeSql("update ccpagecontent set ChildPagesFound=1 where id=" & PageID)
        '                                        'Call AppendLog("main_GetSectionMenu_IdMenu, 5-call pageManager_cache_pageContent_updateRow")
        '                                        Call cache_pageContent_updateRow(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                                    End If
        '                                End If
        '                            End If
        '                        End If
        '                        '
        '                        ' ----- main_Get the Menu Header
        '                        '
        '                        pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu & genericController.vbReplace(cpcore.menuFlyout.getMenu(MenuNamePrefix & genericController.encodeText(PageID), MenuStyle, StyleSheetPrefix), vbCrLf, "")
        '                        '
        '                        ' ----- Add in the rest of the menu details
        '                        ' ##### this must be here because it must go into the bake, else a baked page fails without he menus
        '                        '
        '                        pageManager_GetSectionMenu_IdMenu = pageManager_GetSectionMenu_IdMenu & cpcore.htmlDoc.menu_GetClose()
        '                        '
        '                        ' ----- Bake the completed menu
        '                        '
        '                        Call cpcore.cache.setObject(BakeName, pageManager_GetSectionMenu_IdMenu, ContentName & ",Site Sections,Dynamic Menus,Dynamic Menu Section Rules")
        '                    End If
        '                End If
        '            End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetSectionMenu_IdMenu")
        '        End Function
        '        '
        '        '======================================================================================
        '        '   Add child pages to the menu system
        '        '       REturns the count of total child pages (with grand-child, etc)
        '        '       Returns -1 if child count not checked
        '        ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
        '        '======================================================================================
        '        '
        '        Friend Function main_GetSectionMenu_AddChildMenu_ReturnChildCount(ByVal ParentMenuID As Integer, ByVal ContentName As String, ByVal DefaultLink As String, ByVal Tier1MenuCaption As String, ByVal UsedPageIDString As String, ByVal MenuNamePrefix As String, ByVal MenuDepth As Integer, ByVal MenuDepthMax As Integer, ByVal childListSortMethodId As Integer, ByVal SectionID As Integer, ByVal AddRootButton As Boolean, ByVal UseContentWatchLink As Boolean) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00365")
        '            '
        '            Dim Active As Boolean
        '            Dim PseudoChildChildPagesFound As Boolean
        '            Dim PCCRowPtr As Integer
        '            Dim SortForward As Boolean
        '            Dim SortFieldName As String
        '            Dim SortPtr As Integer
        '            Dim Ptr As Integer
        '            Dim ChildPageCount As Integer
        '            Dim ChildPagesFoundTest As String
        '            Dim FieldList As String
        '            Dim ChildCountWithNoPubs As Integer
        '            Dim MenuID As Integer
        '            Dim MenuCaption As String
        '            Dim ChildCount As Integer
        '            Dim ChildSize As Integer
        '            Dim ChildPointer As Integer
        '            Dim ChildID() As Integer
        '            Dim ChildAllowChild() As Boolean
        '            Dim ChildCaption() As String
        '            Dim ChildLink() As String
        '            Dim ChildSortMethodID() As Integer
        '            Dim ChildChildPagesFound() As Boolean
        '            Dim ContentID As Integer
        '            Dim MenuLinkOverRide As String
        '            Dim PageID As Integer
        '            Dim UsedPageIDStringLocal As String
        '            Dim Criteria As String
        '            Dim MenuDepthLocal As Integer
        '            Dim OrderByCriteria As String
        '            Dim WorkingLink As String
        '            Dim templateId As Integer
        '            Dim ContentControlID As Integer
        '            Dim Link As String
        '            Dim PubDate As Date
        '            Dim PCCPtr As Integer
        '            Dim DateExpires As Date
        '            Dim dateArchive As Date
        '            Dim IsIncludedInMenu As Boolean
        '            Dim PCCPtrs() As Integer
        '            Dim PtrCnt As Integer
        '            Dim SortSplit() As String
        '            Dim SortSplitCnt As Integer
        '            Dim Index As keyPtrController
        '            Dim PCCColPtr As Integer
        '            Dim PCCPtrsSorted As Integer()
        '            Dim AllowInMenus As Boolean
        '            '
        '            ' ----- Gather all child menus
        '            '
        '            ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
        '            ChildCountWithNoPubs = -1
        '            If (ParentMenuID > 0) And (MenuDepth <= MenuDepthMax) Then
        '                MenuDepthLocal = MenuDepth + 1
        '                UsedPageIDStringLocal = UsedPageIDString
        '                OrderByCriteria = cpcore.GetSortMethodByID(childListSortMethodId)
        '                If OrderByCriteria = "" Then
        '                    OrderByCriteria = cpcore.GetContentProperty(ContentName, "defaultsortmethod")
        '                End If
        '                If OrderByCriteria = "" Then
        '                    OrderByCriteria = "ID"
        '                End If
        '                '
        '                ' Populate PCCPtrs()
        '                '
        '                PCCPtr = cache_pageContent_getFirstChildPtr(ParentMenuID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                PtrCnt = 0
        '                Do While PCCPtr >= 0
        '                    ReDim Preserve PCCPtrs(PtrCnt)
        '                    PCCPtrs(PtrCnt) = PCCPtr
        '                    PtrCnt = PtrCnt + 1
        '                    PCCPtr = cache_pageContent_parentIdIndex.getNextPtrMatch(CStr(ParentMenuID))
        '                Loop
        '                If PtrCnt > 0 Then
        '                    PCCPtrsSorted = cache_pageContent_getPtrsSorted(PCCPtrs, OrderByCriteria)
        '                End If
        '                '
        '                Ptr = 0
        '                Do While Ptr < PtrCnt
        '                    PCCPtr = PCCPtrsSorted(Ptr)
        '                    DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
        '                    dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
        '                    PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
        '                    MenuCaption = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr)))
        '                    If False Then '.3.752" Then
        '                        AllowInMenus = (MenuCaption <> "")
        '                    Else
        '                        AllowInMenus = genericController.EncodeBoolean(cache_pageContent(PCC_AllowInMenus, PCCPtr))
        '                    End If
        '                    Active = genericController.EncodeBoolean(cache_pageContent(PCC_Active, PCCPtr))
        '                    IsIncludedInMenu = Active And AllowInMenus And ((PubDate = Date.MinValue) Or (PubDate < cpcore.app_startTime)) And ((DateExpires = Date.MinValue) Or (DateExpires > cpcore.app_startTime))
        '                    'IsIncludedInMenu = Active And AllowInMenus And ((PubDate = Date.MinValue) Or (PubDate < main_PageStartTime)) And ((DateExpires = Date.MinValue) Or (DateExpires > main_PageStartTime)) And ((DateArchive = Date.MinValue) Or (DateArchive > main_PageStartTime))
        '                    If IsIncludedInMenu Then
        '                        If MenuCaption = "" Then
        '                            MenuCaption = Trim(genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr)))
        '                        End If
        '                        If MenuCaption = "" Then
        '                            MenuCaption = "Related Page"
        '                        End If
        '                        If (MenuCaption <> "") Then
        '                            PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
        '                            If genericController.vbInstr(1, UsedPageIDStringLocal & ",", "," & genericController.encodeText(PageID) & ",") = 0 Then
        '                                UsedPageIDStringLocal = UsedPageIDStringLocal & "," & genericController.encodeText(PageID)
        '                                If ChildCount >= ChildSize Then
        '                                    ChildSize = ChildSize + 100
        '                                    ReDim Preserve ChildID(ChildSize)
        '                                    ReDim Preserve ChildCaption(ChildSize)
        '                                    ReDim Preserve ChildLink(ChildSize)
        '                                    ReDim Preserve ChildSortMethodID(ChildSize)
        '                                    ReDim Preserve ChildAllowChild(ChildSize)
        '                                    ReDim Preserve ChildChildPagesFound(ChildSize)
        '                                End If
        '                                ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
        '                                MenuLinkOverRide = genericController.encodeText(cache_pageContent(PCC_Link, PCCPtr))
        '                                '
        '                                ChildCaption(ChildCount) = MenuCaption
        '                                ChildID(ChildCount) = PageID
        '                                ChildAllowChild(ChildCount) = genericController.EncodeBoolean(cache_pageContent(PCC_AllowChildListDisplay, PCCPtr))
        '                                ChildSortMethodID(ChildCount) = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, PCCPtr))
        '                                templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
        '                                '
        '                                Link = getPageLink4(PageID, "", True, UseContentWatchLink)
        '                                'Link = main_GetPageDynamicLinkWithArgs(contentcontrolid, PageID, DefaultLink, False, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
        '                                ChildLink(ChildCount) = Link
        '                                ChildPagesFoundTest = cache_pageContent(PCC_ChildPagesFound, PCCPtr)
        '                                If ChildPagesFoundTest = "" Then
        '                                    '
        '                                    ' Not initialized
        '                                    '
        '                                    ChildChildPagesFound(ChildCount) = True
        '                                Else
        '                                    ChildChildPagesFound(ChildCount) = genericController.EncodeBoolean(ChildPagesFoundTest)
        '                                End If
        '                                ChildCount = ChildCount + 1
        '                            End If
        '                        End If
        '                    End If
        '                    Ptr = Ptr + 1
        '                Loop
        '                ChildCountWithNoPubs = Ptr
        '                '
        '                ' ----- Output menu entries
        '                '
        '                If ChildCount > 0 Then
        '                    '
        '                    ' menu entry has children, output menu entry, child menu entry, and group of child entries
        '                    '
        '                    If AddRootButton Then
        '                        '
        '                        ' Root Button is a redundent menu entry at the top of tier 1 panels that links to the root page
        '                        '
        '                        Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", DefaultLink, Tier1MenuCaption)
        '                        'Call main_AddMenuEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByPageID(ParentMenuID, "", DefaultLink), Tier1MenuCaption)
        '                        'Call main_AddMenuEntry(MenuNamePrefix & ParentMenuID & ".entry", MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByLink(DefaultLink), Tier1MenuCaption)
        '                    End If
        '                    '
        '                    For ChildPointer = 0 To ChildCount - 1
        '                        MenuID = ChildID(ChildPointer)
        '                        MenuCaption = ChildCaption(ChildPointer)
        '                        WorkingLink = ChildLink(ChildPointer)
        '                        Call cpcore.htmlDoc.menu_AddEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", WorkingLink, MenuCaption)
        '                        'Call main_AddMenuEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByPageID(MenuID, "", WorkingLink), MenuCaption)
        '                        'Call main_AddMenuEntry(MenuNamePrefix & MenuID, MenuNamePrefix & ParentMenuID, "", "", main_GetLinkAliasByLink(WorkingLink), MenuCaption)
        '                        '
        '                        ' if child pages are found, print the next menu deeper
        '                        '
        '                        If (ParentMenuID > 0) And (MenuDepthLocal <= MenuDepthMax) Then
        '                            If pagemanager_IsWorkflowRendering() Then
        '                                '
        '                                ' Workflow mode - go main_Get the child pages
        '                                '
        '                                ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, WorkingLink, MenuCaption, UsedPageIDStringLocal, MenuNamePrefix, MenuDepthLocal, MenuDepthMax, ChildSortMethodID(ChildPointer), SectionID, False, UseContentWatchLink)
        '                            Else
        '                                '
        '                                ' Production mode - main_Get them only if the parent record says there are child pages
        '                                '
        '                                PseudoChildChildPagesFound = ChildChildPagesFound(ChildPointer)
        '                                If Not PseudoChildChildPagesFound Then
        '                                    '
        '                                    ' Even when child pages is false, try it 10% of the time anyway
        '                                    '
        '                                    Randomize()
        '                                    PseudoChildChildPagesFound = (Rnd() > 0.8)
        '                                End If
        '                                If PseudoChildChildPagesFound Then
        '                                    '
        '                                    ' Child pages were found, create child menu
        '                                    '
        '                                    ' 7/21/2009 - added -1 return if the child pages are not counted to prevent the page records from being set not ChildPagesFound
        '                                    ChildPageCount = main_GetSectionMenu_AddChildMenu_ReturnChildCount(MenuID, ContentName, WorkingLink, MenuCaption, UsedPageIDStringLocal, MenuNamePrefix, MenuDepthLocal, MenuDepthMax, ChildSortMethodID(ChildPointer), SectionID, False, UseContentWatchLink)
        '                                    If ChildPageCount >= 0 Then
        '                                        If (True) Then
        '                                            If ChildChildPagesFound(ChildPointer) And (ChildPageCount = 0) Then
        '                                                '
        '                                                ' no pages were found, clear the child pages found property
        '                                                ' child pages found property is set at admin site when a page is saved with this as the parent id
        '                                                '
        '                                                Call cpcore.db.executeSql("update ccpagecontent set ChildPagesFound=0 where id=" & MenuID)
        '                                                'Call AppendLog("pageManager_GetHtmlBody_GetSection_GetContentMenu_AddChildMenu, 6-call pageManager_cache_pageContent_updateRow -- fix here to NOT call pageManager_cache_pageContent_updateRow()")
        '                                                cache_pageContent(PCC_ChildPagesFound, ChildPointer) = "0"
        '                                                'Call pageManager_cache_pageContent_updateRow(MenuID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                                            ElseIf (ChildPageCount > 0) And (Not ChildChildPagesFound(ChildPointer)) Then
        '                                                '
        '                                                ' pages were found, set the child pages found property
        '                                                '
        '                                                Call cpcore.db.executeSql("update ccpagecontent set ChildPagesFound=1 where id=" & MenuID)
        '                                                'Call AppendLog("pageManager_GetHtmlBody_GetSection_GetContentMenu_AddChildMenu, 7-call pageManager_cache_pageContent_updateRow -- fix here to NOT call pageManager_cache_pageContent_updateRow()")
        '                                                cache_pageContent(PCC_ChildPagesFound, ChildPointer) = "1"
        '                                                'Call pageManager_cache_pageContent_updateRow(MenuID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                                            End If
        '                                        End If
        '                                    End If
        '                                End If
        '                            End If
        '                        End If
        '                    Next
        '                End If
        '            End If
        '            main_GetSectionMenu_AddChildMenu_ReturnChildCount = ChildCountWithNoPubs
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetSectionMenu_AddChildMenu_ReturnChildCount")
        '        End Function
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
            Dim ChildContent As String
            Dim Brief As String
            Dim UcaseRequestedListName As String
            Dim ChildListCount As Integer
            Dim AddLink As String
            Dim BlockContentComposite As Boolean
            Dim Link As String
            Dim LinkedText As String
            Dim ActiveList As String = ""
            Dim InactiveList As String
            Dim archiveLink As String
            Dim PageLink As String
            Dim pageMenuHeadline As String
            Dim pageEditLink As String
            Dim isAuthoring = cpcore.authContext.isEditing(cpcore, pageContentModel.contentName)
            '
            ChildListCount = 0
            UcaseRequestedListName = genericController.vbUCase(RequestedListName)
            If (UcaseRequestedListName = "NONE") Or (UcaseRequestedListName = "ORPHAN") Then
                UcaseRequestedListName = ""
            End If
            '
            archiveLink = cpcore.webServer.requestPathPage
            archiveLink = genericController.ConvertLinkToShortLink(archiveLink, cpcore.webServer.requestDomain, cpcore.webServer.webServerIO_requestVirtualFilePath)
            archiveLink = genericController.EncodeAppRootPath(archiveLink, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            '
            Dim sqlCriteria As String = "(parentId=" & page.id & ")"
            Dim sqlOrderBy As String = "sortOrder"
            Dim childPageList As List(Of pageContentModel) = pageContentModel.createList(cpcore, sqlCriteria, sqlOrderBy)
            For Each testPage As pageContentModel In childPageList
                PageLink = getPageLink4(page.id, "", True, False)
                pageMenuHeadline = page.MenuHeadline
                If pageMenuHeadline = "" Then
                    pageMenuHeadline = Trim(page.name)
                    If pageMenuHeadline = "" Then
                        pageMenuHeadline = "Related Page"
                    End If
                End If
                pageEditLink = ""
                If cpcore.authContext.isEditing(cpcore, ContentName) Then
                    pageEditLink = cpcore.htmlDoc.main_GetRecordEditLink2(ContentName, page.id, True, page.name, True)
                End If
                '
                If ArchivePages Then
                    Link = genericController.modifyLinkQuery(archiveLink, rnPageId, CStr(page.id), True)
                Else
                    Link = PageLink
                End If
                If page.BlockContent Or page.BlockPage Then
                    BlockContentComposite = Not pageManager_BypassContentBlock(page.ContentControlID, page.id)
                Else
                    BlockContentComposite = False
                End If
                LinkedText = genericController.csv_GetLinkedText("<a href=""" & cpcore.htmlDoc.html_EncodeHTML(Link) & """>", pageMenuHeadline)
                If (UcaseRequestedListName = "") And (page.ParentListName <> "") And (Not isAuthoring) Then
                    '
                    ' ----- Requested orphan list, and this record is in a named list, and not editing, do not display
                    '
                ElseIf (UcaseRequestedListName = "") And (page.ParentListName <> "") Then
                    '
                    ' ----- Requested orphan list, and this record is in a named list, but authoring, list it
                    '
                    If isAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & page.id & """ name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[from Child Page List '" & page.ParentListName & "': " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (UcaseRequestedListName = "") And (Not allowChildListDisplay) And (Not isAuthoring) Then
                    '
                    ' ----- Requested orphan List, Not AllowChildListDisplay, not Authoring, do not display
                    '
                ElseIf (UcaseRequestedListName <> "") And (UcaseRequestedListName <> genericController.vbUCase(page.ParentListName)) Then
                    '
                    ' ----- requested named list and wrong RequestedListName, do not display
                    '
                ElseIf (Not page.AllowInChildLists) Then
                    '
                    ' ----- Allow in Child Page Lists is false, display hint to authors
                    '
                    If isAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Allow in Child Lists is not checked): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf Not page.Active Then
                    '
                    ' ----- Not active record, display hint if authoring
                    '
                    If isAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Inactive): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (page.PubDate <> Date.MinValue) And (page.PubDate > cpcore.app_startTime) Then
                    '
                    ' ----- Child page has not been published
                    '
                    If isAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (To be published " & page.PubDate & "): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                ElseIf (page.DateExpires <> Date.MinValue) And (page.DateExpires < cpcore.app_startTime) Then
                    '
                    ' ----- Child page has expired
                    '
                    If isAuthoring Then
                        InactiveList = InactiveList & cr & "<li name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                        InactiveList = InactiveList & pageEditLink
                        InactiveList = InactiveList & "[Hidden (Expired " & page.DateExpires & "): " & LinkedText & "]"
                        InactiveList = InactiveList & "</li>"
                    End If
                Else
                    '
                    ' ----- display list (and authoring links)
                    '
                    ActiveList = ActiveList & cr & "<li name=""page" & page.id & """  id=""page" & page.id & """ class=""ccListItem"">"
                    If pageEditLink <> "" Then
                        ActiveList = ActiveList & pageEditLink & "&nbsp;"
                    End If
                    ActiveList = ActiveList & LinkedText
                    '
                    ' include authoring mark for content block
                    '
                    If isAuthoring Then
                        If page.BlockContent Then
                            ActiveList = ActiveList & "&nbsp;[Content Blocked]"
                        End If
                        If page.BlockPage Then
                            ActiveList = ActiveList & "&nbsp;[Page Blocked]"
                        End If
                    End If
                    '
                    ' include overview
                    ' if AllowBrief is false, BriefFilename is not loaded
                    '
                    If (page.BriefFilename <> "") And (page.AllowBrief) Then
                        Brief = Trim(cpcore.cdnFiles.readFile(page.BriefFilename))
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
                ChildContent = pageContentModel.contentName
                If ChildContent = "" Then
                    ChildContent = "Page Content"
                End If
                AddLink = cpcore.htmlDoc.main_GetRecordAddLink(ChildContent, "parentid=" & parentPageID & ",ParentListName=" & UcaseRequestedListName, True)
                If AddLink <> "" Then
                    InactiveList = InactiveList & cr & "<li class=""ccListItem"">" & AddLink & "</LI>"
                End If
            End If
            '
            ' ----- If there is a list, add the list start and list end
            '
            pageManager_GetChildPageList = ""
            If ActiveList <> "" Then
                pageManager_GetChildPageList = pageManager_GetChildPageList & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildList"">" & genericController.htmlIndent(ActiveList) & cr & "</ul>"
            End If
            If InactiveList <> "" Then
                pageManager_GetChildPageList = pageManager_GetChildPageList & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildListInactive"">" & genericController.htmlIndent(InactiveList) & cr & "</ul>"
            End If
            '
            ' ----- if non-orphan list, authoring and none found, print none message
            '
            If (UcaseRequestedListName <> "") And (ChildListCount = 0) And isAuthoring Then
                pageManager_GetChildPageList = "[Child Page List with no pages]</p><p>" & pageManager_GetChildPageList
            End If
        End Function
        '        '
        '        '=============================================================================
        '        '   main_Get the Section Menu
        '        '   MenuName blank reverse menu to legacy mode (all sections on menu)
        '        '=============================================================================
        '        '
        '        Public Function pageManager_GetSectionMenu(ByVal DepthLimit As Integer, ByVal MenuStyle As Integer, ByVal StyleSheetPrefix As String, ByVal DefaultTemplateLink As String, ByVal MenuID As Integer, ByVal MenuName As String, ByVal UseContentWatchLink As Boolean) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("PageList_GetSectionMenu")
        '            '
        '            Dim layoutError As String
        '            Dim pageActive As Boolean
        '            Dim TCPtr As Integer
        '            Dim PCCPtr As Integer
        '            Dim rootPageId As Integer
        '            Dim CSSections As Integer
        '            Dim CSTemplates As Integer
        '            Dim CSPage As Integer
        '            Dim SectionName As String
        '            Dim templateId As Integer
        '            Dim ContentID As Integer
        '            Dim ContentName As String
        '            'Dim main_oldCacheArray_ParentBranchPointer as integer
        '            Dim Link As String
        '            Dim SectionID As Integer
        '            Dim AuthoringTag As String
        '            Dim MenuImage As String
        '            Dim MenuImageOver As String
        '            'Dim SectionCount as integer
        '            Dim LandingLink As String
        '            Dim MenuString As String
        '            Dim SectionCaption As String
        '            Dim SectionTemplateID As Integer
        '            Dim Criteria As String
        '            Dim SelectFieldList As String
        '            Dim ShowHiddenMenu As Boolean
        '            Dim HideMenu As Boolean
        '            'dim buildversion As String
        '            Dim PageContentCID As Integer
        '            Dim BlockPage As Boolean
        '            Dim BlockSection As Boolean
        '            Dim SQL As String
        '            Dim IsAllSectionsMenuMode As Boolean
        '            '
        '            '
        '            '
        '            ' fixed? - !! Problem: new upgraded site with old menu object (MenuName=""). We take the third option here, but later in the
        '            '   routine we use RootPageID because the check is on Version only
        '            '
        '            IsAllSectionsMenuMode = (MenuName = "")
        '            PageContentCID = cpcore.main_GetContentID("Page Content")
        '            If (True) Then
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID"
        '                ShowHiddenMenu = cpcore.authContext.isEditingAnything(cpcore)
        '                'ShowHiddenMenu = main_IsEditing("Site Sections")
        '                If IsAllSectionsMenuMode Then
        '                    '
        '                    ' Section/Page connection at RootPageID, show all sections
        '                    '
        '                    CSSections = cpcore.db.cs_open("Site Sections", , , , , ,, SelectFieldList)
        '                Else
        '                    '
        '                    ' Section/Page connection at RootPageID, only show sections connected to the menu
        '                    '
        '                    SQL = "Select Distinct S.ID" _
        '                        & " from ((ccSections S" _
        '                        & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
        '                        & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
        '                        & " where M.ID=" & MenuID
        '                    Criteria = "ID in (" & SQL & ")"
        '                    CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , , , SelectFieldList)
        '                End If
        '                '        '
        '                '        ' Section/Page connection at RootPageID
        '                '        '
        '                '        SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID"
        '                '        SQL = "Select Distinct S.ID" _
        '                '            & " from ((ccSections S" _
        '                '            & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
        '                '            & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
        '                '            & " where M.ID=" & MenuID
        '                '        Criteria = "ID in (" & SQL & ")"
        '                '        ShowHiddenMenu = main_IsEditing("Site Sections")
        '                '        CSSections = app.csOpen("Site Sections", Criteria, , , , , SelectFieldList)
        '            ElseIf (True) Then
        '                '
        '                ' Multiple Menus with ccDynamicMenuSectionRules
        '                '
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,0 as RootPageID"
        '                ShowHiddenMenu = cpcore.authContext.isEditingAnything(cpcore)
        '                'ShowHiddenMenu = main_IsEditing("Site Sections")
        '                If IsAllSectionsMenuMode Then
        '                    '
        '                    ' Section/Page connection at RootPageID, show all sections
        '                    '
        '                    CSSections = cpcore.db.cs_open("Site Sections", , , , , , , SelectFieldList)
        '                Else
        '                    '
        '                    ' Section/Page connection at RootPageID, only show sections connected to the menu
        '                    '
        '                    SQL = "Select Distinct S.ID" _
        '                        & " from ((ccSections S" _
        '                        & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
        '                        & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
        '                        & " where M.ID=" & MenuID
        '                    Criteria = "ID in (" & SQL & ")"
        '                    CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
        '                End If
        '                '        SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection"
        '                '        SQL = "Select Distinct S.ID" _
        '                '            & " from ((ccSections S" _
        '                '            & " left join ccDynamicMenuSectionRules R on R.SectionID=S.ID)" _
        '                '            & " left join ccDynamicMenus M on M.ID=R.DynamicMenuID)" _
        '                '            & " where M.ID=" & MenuID
        '                '        Criteria = "ID in (" & SQL & ")"
        '                '        ShowHiddenMenu = main_IsEditing("Site Sections")
        '                '        CSSections = app.csOpen("Site Sections", Criteria, , , , , SelectFieldList)
        '            ElseIf cpcore.IsSQLTableField("Default", "ccSections", "BlockSection") Then
        '                '
        '                ' All sections menu mode with block sections
        '                '
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,0 as RootPageID"
        '                Criteria = ""
        '                ShowHiddenMenu = cpcore.authContext.isEditingAnything(cpcore)
        '                'ShowHiddenMenu = main_IsEditing("Site Sections")
        '                CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
        '            ElseIf cpcore.IsSQLTableField("Default", "ccSections", "MenuImageOverFilename") Then
        '                '
        '                ' All sections menu mode with Image Over
        '                '
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,0 as BlockSection,0 as RootPageID"
        '                Criteria = ""
        '                ShowHiddenMenu = cpcore.authContext.isEditingAnything(cpcore)
        '                'ShowHiddenMenu = main_IsEditing("Site Sections")
        '                CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
        '            ElseIf cpcore.IsSQLTableField("Default", "ccSections", "HideMenu") Then
        '                '
        '                ' All sections menu mode with HideMenu
        '                '
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,'' as MenuImageOverFilename,HideMenu,0 as BlockSection,0 as RootPageID"
        '                Criteria = ""
        '                ShowHiddenMenu = cpcore.authContext.isEditingAnything(cpcore)
        '                'ShowHiddenMenu = main_IsEditing("Site Sections")
        '                CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
        '            Else
        '                SelectFieldList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,'' as MenuImageOverFilename,0 as HideMenu,0 as BlockSection,0 as RootPageID"
        '                Criteria = ""
        '                ShowHiddenMenu = True
        '                CSSections = cpcore.db.cs_open("Site Sections", Criteria, , , , ,, SelectFieldList)
        '            End If
        '            Do While cpcore.db.cs_ok(CSSections)
        '                HideMenu = cpcore.db.cs_getBoolean(CSSections, "HideMenu")
        '                BlockSection = cpcore.db.cs_getBoolean(CSSections, "BlockSection")
        '                SectionID = cpcore.db.cs_getInteger(CSSections, "ID")
        '                If ShowHiddenMenu Or Not HideMenu Then
        '                    SectionName = Trim(cpcore.db.cs_getText(CSSections, "Name"))
        '                    If SectionName = "" Then
        '                        SectionName = "Section " & SectionID
        '                        Call cpcore.db.executeSql("update ccSections set Name=" & cpcore.db.encodeSQLText(SectionName) & " where ID=" & SectionID)
        '                    End If
        '                    SectionCaption = cpcore.db.cs_getText(CSSections, "Caption")
        '                    If SectionCaption = "" Then
        '                        SectionCaption = SectionName
        '                        Call cpcore.db.executeSql("update ccSections set Caption=" & cpcore.db.encodeSQLText(SectionCaption) & " where ID=" & SectionID)
        '                    End If
        '                    If HideMenu Then
        '                        SectionCaption = "[Hidden: " & SectionCaption & "]"
        '                    End If
        '                    SectionTemplateID = cpcore.db.cs_getInteger(CSSections, "TemplateID")
        '                    ContentID = cpcore.db.cs_getInteger(CSSections, "ContentID")
        '                    If (ContentID <> PageContentCID) And (Not cpcore.IsWithinContent(ContentID, PageContentCID)) Then
        '                        ContentID = PageContentCID
        '                        Call cpcore.db.cs_set(CSSections, "ContentID", ContentID)
        '                    End If
        '                    If ContentID = PageContentCID Then
        '                        ContentName = "Page Content"
        '                    Else
        '                        ContentName = cpcore.metaData.getContentNameByID(ContentID)
        '                        If ContentName = "" Then
        '                            ContentName = "Page Content"
        '                            ContentID = cpcore.main_GetContentID(ContentName)
        '                            Call cpcore.db.executeSql("update ccSections set ContentID=" & ContentID & " where ID=" & SectionID)
        '                        End If
        '                    End If
        '                    MenuImage = cpcore.db.cs_getText(CSSections, "MenuImageFilename")
        '                    If MenuImage <> "" Then
        '                        MenuImage = genericcontroller.getCdnFileLink(cpcore, MenuImage)
        '                    End If
        '                    MenuImageOver = cpcore.db.cs_getText(CSSections, "MenuImageOverFilename")
        '                    If MenuImageOver <> "" Then
        '                        MenuImageOver = genericcontroller.getCdnFileLink(cpcore, MenuImageOver)
        '                    End If
        '                    '
        '                    ' main_Get Root Page for templateID
        '                    '
        '                    templateId = 0
        '                    BlockPage = False
        '                    Link = ""
        '                    If False Then '.3.451" Then
        '                        '
        '                        ' no blockpage,section-page connection by name
        '                        '
        '                        PCCPtr = cache_pageContent_getFirstNamePtr(SectionName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                        'SelectFieldList = "ID,TemplateID,0 as BlockPage"
        '                        'CSPage = app.csOpen(ContentName, "name=" & encodeSQLText(SectionName), "ID", , , , SelectFieldList)
        '                    ElseIf False Then '.3.613" Then
        '                        '
        '                        ' blockpage,section-page connection by name
        '                        '
        '                        PCCPtr = cache_pageContent_getFirstNamePtr(SectionName, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                        'SelectFieldList = "ID,TemplateID,BlockPage"
        '                        'CSPage = app.csOpen(ContentName, "name=" & encodeSQLText(SectionName), "ID", , , , SelectFieldList)
        '                    Else
        '                        '
        '                        ' section-page connection by name
        '                        '
        '                        rootPageId = cpcore.db.cs_getInteger(CSSections, "rootpageid")
        '                        PCCPtr = cache_pageContent_getPtr(rootPageId, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                        'SelectFieldList = "ID,TemplateID,BlockPage"
        '                        'CSPage = main_OpenCSContentRecord_Internal(ContentName, RootPageID, , , SelectFieldList)
        '                    End If
        '                    If PCCPtr >= 0 Then
        '                        rootPageId = genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr))
        '                        templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
        '                        BlockPage = genericController.EncodeBoolean(cache_pageContent(PCC_BlockPage, PCCPtr))
        '                        pageActive = genericController.EncodeBoolean(cache_pageContent(PCC_Active, PCCPtr))
        '                    End If
        '                    If pageActive Or ShowHiddenMenu Then
        '                        If PCCPtr < 0 Then
        '                            '
        '                            ' Page Missing
        '                            '
        '                            SectionCaption = "[Missing Page: " & SectionCaption & "]"
        '                        ElseIf Not pageActive Then
        '                            '
        '                            ' Page Inactive
        '                            '
        '                            SectionCaption = "[Inactive Page: " & SectionCaption & "]"
        '                        End If
        '                        If templateId = 0 Then
        '                            templateId = SectionTemplateID
        '                        End If
        '                        '
        '                        ' main_Get the link from either the template, or use the default link
        '                        '
        '                        If templateId <> 0 Then
        '                            TCPtr = pageManager_cache_pageTemplate_getPtr(templateId)
        '                            If TCPtr >= 0 Then
        '                                Link = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
        '                            End If
        '                            'Link = main_GetTCLink(TCPtr)
        '                        End If
        '                        If Link = "" Then
        '                            Link = DefaultTemplateLink
        '                        End If
        '                        AuthoringTag = cpcore.htmldoc.main_GetRecordEditLink2("Site Sections", SectionID, False, SectionName, cpcore.authContext.isEditing(cpcore, "Site Sections"))
        '                        Link = genericController.modifyLinkQuery(Link, "sid", CStr(SectionID), True)
        '                        '
        '                        ' main_Get Menu, remove crlf, and parse the line with crlf
        '                        '
        '                        MenuString = pageManager_GetSectionMenu_IdMenu(rootPageId, Link, DepthLimit, MenuStyle, StyleSheetPrefix, MenuImage, MenuImageOver, SectionCaption, SectionID, UseContentWatchLink)
        '                        MenuString = genericController.vbReplace(AuthoringTag & MenuString, vbCrLf, "")
        '                        If (MenuString <> "") Then
        '                            If (pageManager_GetSectionMenu = "") Then
        '                                pageManager_GetSectionMenu = MenuString
        '                            Else
        '                                pageManager_GetSectionMenu = pageManager_GetSectionMenu & vbCrLf & MenuString
        '                            End If
        '                        End If
        '                    End If
        '                    '
        '                End If
        '                Call cpcore.db.cs_goNext(CSSections)
        '            Loop
        '            AuthoringTag = cpcore.htmldoc.main_GetRecordAddLink("Site Sections", "MenuID=" & MenuID)
        '            If AuthoringTag <> "" Then
        '                pageManager_GetSectionMenu = pageManager_GetSectionMenu & AuthoringTag
        '            End If
        '            Call cpcore.db.cs_Close(CSSections)
        '            '
        '            pageManager_GetSectionMenu = cpcore.htmlDoc.html_executeContentCommands(Nothing, pageManager_GetSectionMenu, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.ID, cpcore.authContext.isAuthenticated, layoutError)
        '            pageManager_GetSectionMenu = cpcore.htmlDoc.html_encodeContent10(pageManager_GetSectionMenu, cpcore.authContext.user.ID, "", 0, 0, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.isAuthenticated, Nothing, cpcore.authContext.isEditingAnything(cpcore))
        '            'pageManager_GetSectionMenu = main_EncodeContent5(pageManager_GetSectionMenu, memberID, "", 0, 0, False, False, True, True, False, True, "", "", False, 0)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("pageManager_GetSectionMenu")
        '        End Function
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
            If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                pageManager_BypassContentBlock = True
            ElseIf cpcore.authContext.isAuthenticatedContentManager(cpcore, cpcore.metaData.getContentNameByID(ContentID)) Then
                pageManager_BypassContentBlock = True
            Else
                SQL = "SELECT ccMemberRules.MemberID" _
                    & " FROM (ccPageContentBlockRules LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID) LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID" _
                    & " WHERE (((ccPageContentBlockRules.RecordID)=" & RecordID & ")" _
                    & " AND ((ccPageContentBlockRules.Active)<>0)" _
                    & " AND ((ccgroups.Active)<>0)" _
                    & " AND ((ccMemberRules.Active)<>0)" _
                    & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & cpcore.db.encodeSQLDate(cpcore.app_startTime) & ")" _
                    & " AND ((ccMemberRules.MemberID)=" & cpcore.authContext.user.id & "));"
                CS = cpcore.db.cs_openSql(SQL)
                pageManager_BypassContentBlock = cpcore.db.cs_ok(CS)
                Call cpcore.db.cs_Close(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("IsContentBlocked")
        End Function
        '        '
        '        '===========================================================================
        '        '   Populate the parent branch
        '        '       PageID and RootPageID must be valid
        '        '
        '        '   I think this routine is over-written at this point. Since we now call with pageid and rootpageid, the loop is not needed -- I think
        '        '===========================================================================
        '        '
        '        Friend Sub pageManager_LoadRenderCache_CurrentPage(PageID As Integer, rootPageId As Integer, RootPageContentName As String, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_LoadRenderCache_CurrentPage")
        '            '
        '            Dim SubMessage As String
        '            'Dim PCCPtr as integer
        '            Dim IsNotActive As Boolean
        '            Dim IsExpired As Boolean
        '            Dim IsNotPublished As Boolean
        '            'Dim IsArchived As Boolean
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim ParentID As Integer
        '            Dim pageCaption As String
        '            Dim QueryString As String
        '            Dim dateArchive As Date
        '            Dim DateExpires As Date
        '            Dim PubDate As Date
        '            Dim Criteria As String
        '            Dim ContentControlID As Integer
        '            Dim SelectList As String
        '            Dim reloadPage As Boolean
        '            Dim Active As Boolean
        '            Dim loadPageCnt As Integer
        '            Dim ContentName As String
        '            Dim MenuLinkOverRide As String
        '            Dim SupportMetaContentNoFollow As Boolean
        '            Dim RecordName As String
        '            Dim RecordID As Integer
        '            '
        '            If PageID = 0 And rootPageId = 0 Then
        '                '
        '                ' no page and no root page, redirect to landing page
        '                '
        '                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                pageManager_RedirectBecausePageNotFound = True
        '                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
        '                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '            ElseIf PageID = 0 Then
        '                '
        '                ' no page, redirect to root page
        '                '
        '                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                pageManager_RedirectBecausePageNotFound = True
        '                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
        '                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '            ElseIf rootPageId = 0 Then
        '                '
        '                ' no rootpage id
        '                '
        '                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                pageManager_RedirectBecausePageNotFound = True
        '                pageManager_RedirectReason = "The page could not be determined from URL. The PageID is [" & PageID & "], and the RootPageID is [" & rootPageId & "]."
        '                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '            Else
        '                '
        '                reloadPage = True
        '                Do While reloadPage And (loadPageCnt < 10)
        '                    reloadPage = False
        '                    'main_oldCacheArray_CurrentPagePtr = -1
        '                    'main_oldCacheArray_CurrentPageCount = 0
        '                    main_RenderCache_CurrentPage_PCCPtr = cache_pageContent_getPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                    If (main_RenderCache_CurrentPage_PCCPtr < 0) Then
        '                        '
        '                        ' page was not found ?
        '                        '
        '                        main_RenderCache_CurrentPage_PCCPtr = main_RenderCache_CurrentPage_PCCPtr
        '                    Else
        '                        RecordName = Trim(genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr)))
        '                        ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, main_RenderCache_CurrentPage_PCCPtr))
        '                        DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, main_RenderCache_CurrentPage_PCCPtr))
        '                        dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, main_RenderCache_CurrentPage_PCCPtr))
        '                        PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, main_RenderCache_CurrentPage_PCCPtr))
        '                        Active = genericController.EncodeBoolean(cache_pageContent(PCC_Active, main_RenderCache_CurrentPage_PCCPtr))
        '                        RecordID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
        '                        ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
        '                        MenuLinkOverRide = Trim(genericController.encodeText(cache_pageContent(PCC_Link, main_RenderCache_CurrentPage_PCCPtr)))
        '                        IsNotActive = (Not Active)
        '                        IsExpired = ((DateExpires > Date.MinValue) And (DateExpires < cpcore.app_startTime))
        '                        IsNotPublished = ((PubDate > Date.MinValue) And (PubDate > cpcore.app_startTime))
        '                        'IsArchived = ((DateArchive > Date.MinValue) And (DateArchive < main_PageStartTime))
        '                        '
        '                        RecordName = genericController.vbReplace(RecordName, vbCrLf, " ")
        '                        RecordName = genericController.vbReplace(RecordName, vbTab, " ")
        '                        '
        '                        If IsNotActive Or IsExpired Or IsNotPublished Then
        '                            pageManager_RedirectSourcePageID = PageID
        '                            If IsNotActive Then
        '                                SubMessage = " because it marked inactive"
        '                            ElseIf IsExpired Then
        '                                SubMessage = " because the expiration date has passed"
        '                            ElseIf IsNotPublished Then
        '                                SubMessage = " because the publish date has not passed"
        '                                'ElseIf IsArchived Then
        '                                '    SubMessage = " because the archive date has passed"
        '                            End If
        '                            If PageID = main_GetLandingPageID() Then
        '                                '
        '                                ' Landing Page - do not redirect, just show a blank page with the admin message
        '                                '
        '                                cpcore.htmlDoc.main_AdminWarning = "The page requested [" & PageID & "] can not be displayed" & SubMessage & ". It is the landing page of this website so no replacement page can be displayed. To correct this page, use the link below to edit the page and mark it active. To create a different landing page, edit any other active page and check the box marked 'Set Landing Page' in the control tab."
        '                                cpcore.htmlDoc.main_AdminWarningPageID = PageID
        '                                cpcore.htmlDoc.main_AdminWarningSectionID = SectionID
        '                            ElseIf PageID = rootPageId Then
        '                                '
        '                                ' Root Page - redirect to the landing page with an admin message
        '                                '
        '                                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                                'Call main_LogPageNotFound(main_ServerLink)
        '                                pageManager_RedirectBecausePageNotFound = True
        '                                pageManager_RedirectReason = "The page requested [" & PageID & "] can not be displayed" & SubMessage & ". It is the root page of the section [" & SectionID & "]."
        '                                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '                                Exit Sub
        '                            Else
        '                                '
        '                                ' non-Root Page - redirect to the root page with an admin message
        '                                '
        '                                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                                'Call main_LogPageNotFound(main_ServerLink)
        '                                pageManager_RedirectBecausePageNotFound = True
        '                                pageManager_RedirectReason = "The page requested [" & PageID & "] can not be displayed" & SubMessage & "."
        '                                'pageManager_RedirectReason = "The page requested [" & PageID & "] is marked inactive."
        '                                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '                                Exit Sub
        '                            End If
        '                        End If
        '                        '
        '                        ContentName = cpcore.metaData.getContentNameByID(ContentControlID)
        '                        If ContentName <> "" Then
        '                            If (Not main_RenderCache_CurrentPage_IsQuickEditing) And cpcore.authContext.isQuickEditing(cpcore, ContentName) Then
        '                                main_RenderCache_CurrentPage_IsQuickEditing = True
        '                                reloadPage = True
        '                            End If
        '                        End If
        '                        If (Not reloadPage) Then
        '                            '
        '                            ' ----- Store results in main_oldCacheArray_CurrentPage Storage
        '                            '
        '                            'If main_oldCacheArray_CurrentPageCount >= main_oldCacheArray_CurrentPageSize Then
        '                            '    main_oldCacheArray_CurrentPageSize = main_oldCacheArray_CurrentPageSize + 10
        '                            '    ReDim Preserve main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPageSize)
        '                            'End If
        '                            'main_oldCacheArray_CurrentPagePtr = main_oldCacheArray_CurrentPageCount
        '                            '                    With main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr)
        '                            '                        .Name = RecordName
        '                            '                        .Id = RecordID
        '                            '                        .Active = Active
        '                            '                        .parentId = parentId
        '                            '                        .headline = Trim(genericController.encodeText(main_pcc(PCC_Headline, main_RenderCache_CurrentPage_PCCPtr)))
        '                            '                        .headline = genericController.vbReplace(.headline, vbCrLf, " ")
        '                            '                        .headline = genericController.vbReplace(.headline, vbTab, " ")
        '                            '                        .MenuHeadline = Trim(genericController.encodeText(main_pcc(PCC_MenuHeadline, main_RenderCache_CurrentPage_PCCPtr)))
        '                            '                        .MenuHeadline = genericController.vbReplace(.MenuHeadline, vbCrLf, " ")
        '                            '                        .MenuHeadline = genericController.vbReplace(.MenuHeadline, vbTab, " ")
        '                            '                        .dateArchive = dateArchive
        '                            '                        .DateExpires = DateExpires
        '                            '                        .PubDate = PubDate
        '                            '                        .childListSortMethodId = genericController.EncodeInteger(main_pcc(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .ChildListInstanceOptions = genericController.encodeText(main_pcc(PCC_ChildListInstanceOptions, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .ContentControlID = ContentControlID
        '                            '                        .templateId = genericController.EncodeInteger(main_pcc(PCC_TemplateID, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .LinkOverride = MenuLinkOverRide
        '                            '                        '.LinkDynamic = main_GetPageDynamicLinkWithArgs(.ContentControlID, RecordID, main_ServerPathPage & "?" & main_RefreshQueryString, (.Id = RootPageID), .TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
        '                            '                        .Link = main_GetPageLink4(.Id, "", True, False)
        '                            '                        '.Link = main_GetLinkAliasByPageID(.Id, "", .LinkDynamic)
        '                            '                        .MetaContentNoFollow = genericController.EncodeBoolean(main_pcc(PCC_AllowMetaContentNoFollow, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .BlockSourceID = genericController.EncodeInteger(main_pcc(PCC_BlockSourceID, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .CustomBlockMessageFilename = Trim(genericController.encodeText(main_pcc(PCC_CustomBlockMessageFilename, main_RenderCache_CurrentPage_PCCPtr)))
        '                            '                        .RegistrationGroupID = genericController.EncodeInteger(main_pcc(PCC_RegistrationGroupID, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .JSOnLoad = genericController.encodeText(main_pcc(PCC_JSOnLoad, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .JSHead = genericController.encodeText(main_pcc(PCC_JSHead, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .JSFilename = genericController.encodeText(main_pcc(PCC_JSFilename, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .JSEndBody = genericController.encodeText(main_pcc(PCC_JSEndBody, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                        .DateModified =  genericController.EncodeDate(main_pcc(PCC_ModifiedDate, main_RenderCache_CurrentPage_PCCPtr))
        '                            '                    End With
        '                            '                    main_oldCacheArray_CurrentPageCount = main_oldCacheArray_CurrentPageCount + 1
        '                        End If
        '                    End If
        '                    If (main_RenderCache_CurrentPage_PCCPtr < 0) Then
        '                        'If (main_oldCacheArray_CurrentPagePtr = -1) And (PageID <> 0) Then
        '                        If (SectionID <> 0) And (PageID = rootPageId) Then
        '                            '
        '                            ' This is a root page that is missing -
        '                            '
        '                            ' the redirect does the logging -- Call main_LogPageNotFound(main_ServerLinkSource)
        '                            'Call main_LogPageNotFound(main_ServerLink)
        '                            pageManager_RedirectBecausePageNotFound = True
        '                            pageManager_RedirectReason = "The root page [" & PageID & "] for this section [" & SectionID & "] could not be found. It may have been deleted. To correct this problem, edit this section and save it with 'none' selected for the Root Page. A new root page will be created automatically the next time the page is viewed. Alternately, you can manually select another root page in the section record."
        '                            redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '                            Exit Sub
        '                        Else
        '                            '
        '                            ' ----- This PageID (bid) was not found, revert to the RootPageName and try again
        '                            '
        '                            ' the redirect does the logging -- Call main_LogPageNotFound(main_ServerLinkSource)
        '                            pageManager_RedirectBecausePageNotFound = True
        '                            pageManager_RedirectReason = "The page could not be found from its ID [" & PageID & "]. It may have been deleted."
        '                            redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '                            Exit Sub
        '                        End If
        '                    End If
        '                    loadPageCnt = loadPageCnt + 1
        '                Loop
        '                '
        '                currentPageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
        '                currentParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
        '                currentPageName = genericController.encodeText(cache_pageContent(PCC_Name, main_RenderCache_CurrentPage_PCCPtr))
        '                main_RenderCache_CurrentPage_IsRootPage = (currentPageID = rootPageId) And (currentParentID = 0)
        '                main_RenderCache_CurrentPage_ContentId = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, main_RenderCache_CurrentPage_PCCPtr))
        '                pageContentModel.contentName = cpcore.metaData.getContentNameByID(main_RenderCache_CurrentPage_ContentId)
        '                main_RenderCache_CurrentPage_IsEditing = cpcore.authContext.isEditing(cpcore, pageContentModel.contentName)
        '                main_RenderCache_CurrentPage_IsQuickEditing = cpcore.authContext.isQuickEditing(cpcore, pageContentModel.contentName)
        '                main_RenderCache_CurrentPage_IsAuthoring = main_RenderCache_CurrentPage_IsEditing Or main_RenderCache_CurrentPage_IsQuickEditing
        '                cpcore.webServer.webServerIO_response_NoFollow = genericController.EncodeBoolean(cache_pageContent(PCC_AllowMetaContentNoFollow, main_RenderCache_CurrentPage_PCCPtr)) Or cpcore.webServer.webServerIO_response_NoFollow
        '                '
        '                '        If main_oldCacheArray_CurrentPagePtr <> -1 Then
        '                '            With main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr)
        '                '                'currentPageID = .Id
        '                '                'currentPageName = .Name
        '                '                'main_RenderCache_CurrentPage_IsRootPage = (.Id = rootPageId) And (.ParentID = 0)
        '                '                'main_RenderCache_CurrentPage_ContentId = .ContentControlID
        '                '                'pageContentModel.contentName = metaData.getContentNameByID(.ContentControlID)
        '                '                'main_RenderCache_CurrentPage_IsEditing = main_IsEditing(pageContentModel.contentName)
        '                '                main_RenderCache_CurrentPage_IsQuickEditing = main_IsQuickEditing(pageContentModel.contentName)
        '                '                main_RenderCache_CurrentPage_IsAuthoring = main_RenderCache_CurrentPage_IsEditing Or main_RenderCache_CurrentPage_IsQuickEditing
        '                '                main_MetaContent_NoFollow = .MetaContentNoFollow Or main_MetaContent_NoFollow
        '                '            End With
        '                '        End If
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_LoadRenderCache_CurrentPage")
        '        End Sub
        '        '
        '        '===========================================================================
        '        '   Populate the parent branch
        '        '
        '        '   The PageID is the id of the page being checked for a parent, not the ID of the parent
        '        '===========================================================================
        '        '
        '        Friend Sub pageManager_LoadRenderCache_ParentBranch(PageID As Integer, rootPageId As Integer, RootPageContentName As String, ArchivePage As Boolean, ParentIDPath As String, SectionID As Integer, UseContentWatchLink As Boolean)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_LoadRenderCache_ParentBranch")
        '            '
        '            Dim PCCPtr As Integer
        '            Dim IsRootPage As Boolean
        '            'Dim SQL As String
        '            'Dim CS as integer
        '            Dim ParentID As Integer
        '            Dim pageCaption As String
        '            Dim PageName As String
        '            Dim QueryString As String
        '            Dim dateArchive As Date
        '            Dim DateExpires As Date
        '            Dim PubDate As Date
        '            Dim Criteria As String
        '            Dim ContentControlID As Integer
        '            Dim templateId As Integer
        '            Dim MenuLinkOverRide As String
        '            Dim SelectFieldList As String
        '            Dim hint As String
        '            '
        '            'hint = "main_LoadRenderCache_ParentBranch"
        '            If PageID = 0 Then
        '                '
        '                ' this page does not exist, end of branch
        '                '
        '                'hint = hint & ",10"
        '            ElseIf PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr)) Then
        '                'hint = hint & ",20"
        '                'ElseIf PageID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).Id Then
        '                '
        '                ' This is the current page, main_Get the branch and process it depending on archivepage
        '                '
        '                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
        '                'ParentID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).ParentID
        '                If ArchivePage Then
        '                    Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError12("main_LoadRenderCache_ParentBranch", "The Archive API is no longer supported. The current URL is [" & cpcore.webServer.webServerIO_ServerLink & "]")

        '                Else
        '                    '
        '                    ' non-Archive, test if this bid goes up to this rootpagename (if it is called from the correct page)
        '                    '
        '                    'hint = hint & ",30"
        '                    If PageID <> ParentID Then
        '                        Call pageManager_LoadRenderCache_ParentBranch(ParentID, rootPageId, RootPageContentName, ArchivePage, ParentIDPath & "," & PageID, SectionID, UseContentWatchLink)
        '                    End If
        '                End If
        '            ElseIf genericController.IsInDelimitedString(ParentIDPath, CStr(PageID), ",") Then
        '                '
        '                ' this page has been fetched before, end the branch here
        '                '
        '                Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                'Call main_LogPageNotFound(main_ServerLink)
        '                pageManager_RedirectBecausePageNotFound = True
        '                pageManager_RedirectReason = "The requested page could not be displayed because there is a circular reference within it's parent path. The page [" & PageID & "] was found two times in the parent pages [" & ParentIDPath & "," & PageID & "]."
        '                redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID)
        '            Else
        '                '
        '                ' This is not the current page, and it is not 0, Look it up
        '                '
        '                'hint = hint & ",40"
        '                PCCPtr = cache_pageContent_getPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                If PCCPtr >= 0 Then
        '                    'hint = hint & ",50"
        '                    ReDim Preserve main_RenderCache_ParentBranch_PCCPtrs(main_RenderCache_ParentBranch_PCCPtrCnt)
        '                    main_RenderCache_ParentBranch_PCCPtrs(main_RenderCache_ParentBranch_PCCPtrCnt) = PCCPtr
        '                    main_RenderCache_ParentBranch_PCCPtrCnt = main_RenderCache_ParentBranch_PCCPtrCnt + 1
        '                    '
        '                    ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, PCCPtr))
        '                    PageName = genericController.encodeText(cache_pageContent(PCC_Name, PCCPtr))
        '                    dateArchive = genericController.EncodeDate(cache_pageContent(PCC_DateArchive, PCCPtr))
        '                    DateExpires = genericController.EncodeDate(cache_pageContent(PCC_DateExpires, PCCPtr))
        '                    PubDate = genericController.EncodeDate(cache_pageContent(PCC_PubDate, PCCPtr))
        '                    ContentControlID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, PCCPtr))
        '                    templateId = genericController.EncodeInteger(cache_pageContent(PCC_TemplateID, PCCPtr))
        '                    pageCaption = Trim(genericController.encodeText(cache_pageContent(PCC_MenuHeadline, PCCPtr)))
        '                    If pageCaption = "" Then
        '                        pageCaption = Trim(PageName)
        '                        If pageCaption = "" Then
        '                            pageCaption = "Related Page"
        '                        End If
        '                    End If
        '                    PageName = genericController.vbReplace(PageName, vbCrLf, " ")
        '                    PageName = genericController.vbReplace(PageName, vbTab, " ")
        '                    pageCaption = genericController.vbReplace(pageCaption, vbCrLf, " ")
        '                    pageCaption = genericController.vbReplace(pageCaption, vbTab, " ")
        '                    '
        '                    ' Store results in main_oldCacheArray_ParentBranch Storage
        '                    '
        '                    'If main_oldCacheArray_ParentBranchCount >= main_oldCacheArray_ParentBranchSize Then
        '                    '    main_oldCacheArray_ParentBranchSize = main_oldCacheArray_ParentBranchSize + 10
        '                    '    ReDim Preserve main_oldCacheArray_ParentBranch(main_oldCacheArray_ParentBranchSize)
        '                    'End If
        '                    'With main_oldCacheArray_ParentBranch(main_oldCacheArray_ParentBranchCount)
        '                    '    .Id = genericController.EncodeInteger(main_pcc(PCC_ID, PCCPtr))
        '                    '    .Name = PageName
        '                    '    .Caption = pageCaption
        '                    '    .dateArchive = dateArchive
        '                    '    .DateExpires = DateExpires
        '                    '    .PubDate = PubDate
        '                    '    .childListSortMethodId = genericController.EncodeInteger(main_pcc(PCC_ChildListSortMethodID, PCCPtr))
        '                    '    .ContentControlID = ContentControlID
        '                    '    .parentId = parentId
        '                    '    .templateId = templateId
        '                    '    .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, PCCPtr))
        '                    '    .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, PCCPtr))
        '                    '    .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, PCCPtr))
        '                    '    MenuLinkOverRide = genericController.encodeText(main_pcc(PCC_Link, PCCPtr))
        '                    '    IsRootPage = (.Id = rootPageId)
        '                    '    '.LinkDynamic = main_GetPageDynamicLinkWithArgs(.ContentControlID, .Id, main_ServerPathPage & "?" & main_RefreshQueryString, IsRootPage, TemplateID, SectionID, MenuLinkOverRide, UseContentWatchLink)
        '                    '    .Link = main_GetPageLink4(.Id, "", True, False)
        '                    '    '.Link = main_GetLinkAliasByPageID(.Id, "", .LinkDynamic)
        '                    '    main_oldCacheArray_ParentBranchCount = main_oldCacheArray_ParentBranchCount + 1
        '                    'End With
        '                    '
        '                    'Call app.closeCS(CS)
        '                    '
        '                    IsRootPage = (genericController.EncodeInteger(cache_pageContent(PCC_ID, PCCPtr)) = rootPageId)
        '                    If IsRootPage Then
        '                        '
        '                        ' The top of a page tree
        '                        '
        '                    ElseIf ((DateExpires > Date.MinValue) And (DateExpires < cpcore.app_startTime)) Then
        '                        '
        '                        ' this page within the Parent Branch expired. Abort here and go to RootPage
        '                        '
        '                        '            ElseIf (ArchivePage And (DateArchive < main_PageStartTime)) Then
        '                        '                '
        '                        '                ' The top of an archive tree
        '                        '                '
        '                    ElseIf (ParentID = 0) Then
        '                        '
        '                        ' The top of a page tree
        '                        '
        '                    Else
        '                        '
        '                        ' parent is a child link, bubble up
        '                        '
        '                        'hint = hint & ",60"
        '                        Call pageManager_LoadRenderCache_ParentBranch(ParentID, rootPageId, RootPageContentName, ArchivePage, ParentIDPath & "," & PageID, SectionID, UseContentWatchLink)
        '                    End If
        '                End If
        '            End If
        '            Call debugController.debug_testPoint(cpcore, hint)
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_LoadRenderCache_ParentBranch")
        '        End Sub
        '        '
        '        '===========================================================================
        '        '   Populate the root branch
        '        '===========================================================================
        '        '
        '        Friend Sub pageManager_LoadRenderCache_ChildBranch(ChildOrderByClause As String, IsRenderingMode As Boolean, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("PageList_LoadContent_ChildBranch")
        '            '
        '            Dim PCCPtrs() As Integer
        '            Dim PCCPtrsCnt As Integer
        '            Dim Ptr As Integer
        '            Dim PtrCnt As Integer
        '            Dim PCCPtrsSorted As Integer()
        '            Dim ContentName As String
        '            Dim PCCPtr As Integer
        '            Dim PageID As Integer
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim ParentID As Integer
        '            Dim pageCaption As String
        '            Dim PageName As String
        '            Dim QueryString As String
        '            Dim dateArchive As Date
        '            Dim DateExpires As Date
        '            Dim PubDate As Date
        '            Dim Criteria As String
        '            Dim SQLOrderBy As String
        '            Dim ParentContentID As Integer
        '            Dim ParentContentName As String
        '            Dim IsParentEditing As Boolean
        '            Dim IsParentAuthoring As Boolean
        '            Dim SelectFieldList As String
        '            Dim ContentControlID As Integer
        '            Dim MenuLinkOverRide As String
        '            Dim childListSortMethodId As Integer
        '            Dim parentPCCPtr As Integer
        '            '
        '            If True Then
        '                'If main_oldCacheArray_CurrentPagePtr <> -1 Then
        '                '
        '                ' ----- main_Get the Sort Method Order By Clause, both normal and archive pages
        '                '
        '                If ChildOrderByClause <> "" Then
        '                    SQLOrderBy = ChildOrderByClause
        '                Else
        '                    childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
        '                    'childListSortMethodId = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).childListSortMethodId
        '                    If childListSortMethodId <> 0 Then
        '                        SQLOrderBy = cpcore.metaData.GetSortMethodByID(childListSortMethodId)
        '                    End If
        '                End If
        '                '
        '                ' --- main_Get Parent Content Name
        '                '
        '                ParentContentID = 0
        '                ParentContentName = ""
        '                IsParentEditing = False
        '                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
        '                If ParentID > 0 Then
        '                    parentPCCPtr = main_GetPCCPtr(ParentID, pagemanager_IsWorkflowRendering, False)
        '                    If parentPCCPtr > -1 Then
        '                        ParentContentID = genericController.EncodeInteger(cache_pageContent(PCC_ContentControlID, parentPCCPtr))
        '                        ParentContentName = cpcore.metaData.getContentNameByID(ParentContentID)
        '                        If ParentContentName <> "" Then
        '                            IsParentEditing = cpcore.authContext.isEditing(cpcore, ParentContentName)
        '                        End If
        '                    End If
        '                End If
        '                'ParentContentID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).ContentControlID
        '                'ParentContentName = metaData.getContentNameByID(ParentContentID)
        '                'IsParentEditing = main_IsEditing(ParentContentName)
        '                '
        '                ' Child criteria is all children with main_oldCacheArray_CurrentPage as parentid
        '                '
        '                PageID = genericController.EncodeInteger(cache_pageContent(PCC_ID, main_RenderCache_CurrentPage_PCCPtr))
        '                Criteria = "(ParentID=" & PageID & ")and(id<>" & PageID & ")"
        '                '
        '                'PageID = main_oldCacheArray_CurrentPage(main_oldCacheArray_CurrentPagePtr).Id
        '                '
        '                ' Populate PCCPtrs()
        '                '
        '                PCCPtr = cache_pageContent_getFirstChildPtr(PageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                PtrCnt = 0
        '                Do While PCCPtr >= 0
        '                    ReDim Preserve PCCPtrs(PtrCnt)
        '                    PCCPtrs(PtrCnt) = PCCPtr
        '                    PtrCnt = PtrCnt + 1
        '                    PCCPtr = cache_pageContent_parentIdIndex.getNextPtrMatch(CStr(PageID))
        '                Loop
        '                If PtrCnt > 0 Then
        '                    PCCPtrsSorted = cache_pageContent_getPtrsSorted(PCCPtrs, SQLOrderBy)
        '                    main_RenderCache_ChildBranch_PCCPtrs = PCCPtrsSorted
        '                    main_RenderCache_ChildBranch_PCCPtrCnt = PtrCnt
        '                End If
        '                For Ptr = 0 To PtrCnt - 1
        '                    PCCPtr = PCCPtrsSorted(Ptr)
        '                    If True Then
        '                        '
        '                        ' Store results in main_oldCacheArray_ChildBranch Storage
        '                        '
        '                        'If main_oldCacheArray_ChildBranchCount >= main_oldCacheArray_ChildBranchSize Then
        '                        '    main_oldCacheArray_ChildBranchSize = main_oldCacheArray_ChildBranchSize + 10
        '                        '    ReDim Preserve main_oldCacheArray_ChildBranch(main_oldCacheArray_ChildBranchSize)
        '                        'End If
        '                        'With main_oldCacheArray_ChildBranch(main_oldCacheArray_ChildBranchCount)
        '                        '    PageName = genericController.encodeText(main_pcc(PCC_Name, PCCPtr))
        '                        '    pageCaption = ""
        '                        '    pageCaption = genericController.encodeText(main_pcc(PCC_MenuHeadline, PCCPtr))
        '                        '    If pageCaption = "" Then
        '                        '        pageCaption = Trim(PageName)
        '                        '        If pageCaption = "" Then
        '                        '            pageCaption = "Related Page"
        '                        '        End If
        '                        '    End If
        '                        '    If genericController.vbInstr(1, pageCaption, "<ac", vbTextCompare) <> 0 Then
        '                        '        pageCaption = pageCaption & ACTagEnd
        '                        '    End If
        '                        '    '
        '                        '    ' remove crlf because not allowed (in currentNavigationStructure if nothing else)
        '                        '    '
        '                        '    PageName = genericController.vbReplace(PageName, vbCrLf, " ")
        '                        '    PageName = genericController.vbReplace(PageName, vbTab, " ")
        '                        '    pageCaption = genericController.vbReplace(pageCaption, vbCrLf, " ")
        '                        '    pageCaption = genericController.vbReplace(pageCaption, vbTab, " ")
        '                        '    '
        '                        '    .Name = PageName
        '                        '    .Active = genericController.EncodeBoolean(main_pcc(PCC_Active, PCCPtr))
        '                        '    .AllowInChildLists = genericController.EncodeBoolean(main_pcc(PCC_AllowInChildLists, PCCPtr))
        '                        '    .AllowInMenus = genericController.EncodeBoolean(main_pcc(PCC_AllowInMenus, PCCPtr))
        '                        '    .MenuCaption = pageCaption
        '                        '    .Id = genericController.EncodeInteger(main_pcc(PCC_ID, PCCPtr))
        '                        '    .ListName = genericController.encodeText(main_pcc(PCC_ParentListName, PCCPtr))
        '                        '    .dateArchive =  genericController.EncodeDate(main_pcc(PCC_DateArchive, PCCPtr))
        '                        '    .DateExpires =  genericController.EncodeDate(main_pcc(PCC_DateExpires, PCCPtr))
        '                        '    .PubDate =  genericController.EncodeDate(main_pcc(PCC_PubDate, PCCPtr))
        '                        '    .copyFilename = genericController.encodeText(main_pcc(PCC_CopyFilename, PCCPtr))
        '                        '    .ContentControlID = genericController.EncodeInteger(main_pcc(PCC_ContentControlID, PCCPtr))
        '                        '    .IsDisplayed = False
        '                        '    .BriefFilename = genericController.encodeText(main_pcc(PCC_BriefFilename, PCCPtr))
        '                        '    .AllowBrief = genericController.EncodeBoolean(main_pcc(PCC_AllowBrief, PCCPtr))
        '                        '    ContentName = metaData.getContentNameByID(.ContentControlID)
        '                        '    If main_IsEditing(ContentName) Then
        '                        '        .RecordEditLink = main_GetRecordEditLink2(ContentName, .Id, True, .Name, True)
        '                        '    End If
        '                        '    .templateId = genericController.EncodeInteger(main_pcc(PCC_TemplateID, PCCPtr))
        '                        '    .BlockContent = genericController.EncodeBoolean(main_pcc(PCC_BlockContent, PCCPtr))
        '                        '    .BlockPage = genericController.EncodeBoolean(main_pcc(PCC_BlockPage, PCCPtr))
        '                        '    .headline = genericController.encodeText(main_pcc(PCC_Headline, PCCPtr))
        '                        '    MenuLinkOverRide = genericController.encodeText(main_pcc(PCC_Link, PCCPtr))
        '                        '
        '                        '    .LinkOverride = MenuLinkOverRide
        '                        '    .Link = main_GetPageLink4(.Id, "", True, False)
        '                        'End With
        '                        'main_oldCacheArray_ChildBranchCount = main_oldCacheArray_ChildBranchCount + 1
        '                    End If
        '                Next
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_LoadRenderCache_ChildBranch")
        '        End Sub
        '        '
        '        '===========================================================================
        '        '   Populate the root branch
        '        '===========================================================================
        '        '
        '        Public Sub main_LoadRenderCache(PageID As Integer, rootPageId As Integer, RootPageContentName As String, OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePage As Boolean, SectionID As Integer, UseContentWatchLink As Boolean)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("Proc00371")
        '            '
        '            Dim CSSort As Integer
        '            Dim SortOrder As String
        '            Dim Ptr As Integer
        '            Dim ParentID As Integer
        '            Dim childListSortMethodId As Integer
        '            '
        '            'main_oldCacheArray_ParentBranchCount = 0
        '            'main_oldCacheArray_ChildBranchCount = 0
        '            'main_oldCacheArray_CurrentPagePtr = -1
        '            ''
        '            '' ----- (NEW) Load from cache
        '            ''
        '            'main_oldCacheArray_CurrentPagePtr = pageManager_cache_pageContent_getPtr(PageID, main_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '            '
        '            ' ----- Load the current page
        '            '
        '            Call pageManager_LoadRenderCache_CurrentPage(PageID, PageID, RootPageContentName, ArchivePage, SectionID, UseContentWatchLink)
        '            '
        '            If (redirectLink = "") And (main_RenderCache_CurrentPage_PCCPtr > -1) Then
        '                'If (pageManager_RedirectLink = "") And (main_oldCacheArray_CurrentPagePtr <> -1) Then
        '                '
        '                ' ----- Load Parent Branch
        '                '
        '                ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, main_RenderCache_CurrentPage_PCCPtr))
        '                If (Not main_RenderCache_CurrentPage_IsRootPage) Then
        '                    If ParentID = 0 Then
        '                        '
        '                        ' Error, current page is not the root page, but has no parent pages
        '                        '
        '                        Call logController.log_appendLogPageNotFound(cpcore, cpcore.webServer.requestUrlSource)
        '                        pageManager_RedirectBecausePageNotFound = True
        '                        pageManager_RedirectReason = "The page could not be displayed for security reasons. All valid pages must either have a valid parent page, or be selected by a section as the section's root page. This page has neither a parent page or a section."
        '                        redirectLink = main_ProcessPageNotFound_GetLink(pageManager_RedirectReason, , , PageID, SectionID)
        '                    ElseIf ArchivePage Then
        '                        '
        '                        ' Archive section parent Branch
        '                        '
        '                        Call pageManager_LoadRenderCache_ParentBranch(PageID, rootPageId, RootPageContentName, ArchivePage, "", SectionID, UseContentWatchLink)
        '                    Else
        '                        '
        '                        ' Normal parent Branch
        '                        '
        '                        Call pageManager_LoadRenderCache_ParentBranch(PageID, rootPageId, RootPageContentName, ArchivePage, "", SectionID, UseContentWatchLink)
        '                    End If
        '                End If
        '                If redirectLink = "" Then
        '                    '
        '                    ' ----- load the child branch
        '                    '
        '                    childListSortMethodId = genericController.EncodeInteger(cache_pageContent(PCC_ChildListSortMethodID, main_RenderCache_CurrentPage_PCCPtr))
        '                    SortOrder = cpcore.metaData.GetSortMethodByID(childListSortMethodId)
        '                    If SortOrder = "" Then
        '                        SortOrder = genericController.encodeText(OrderByClause)
        '                    End If
        '                    Call pageManager_LoadRenderCache_ChildBranch(SortOrder, main_RenderCache_CurrentPage_IsRenderingMode, ArchivePage, SectionID, UseContentWatchLink)
        '                    main_RenderCache_Loaded = True
        '                End If
        '            End If
        '            Exit Sub
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_LoadRenderCache")
        '        End Sub
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub cache_pageContent_clear()
        '            On Error GoTo ErrorTrap 'Const Tn = "pageManager_cache_pageContent_clear" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            cache_pageContent_rows = 0
        '            cache_pageContent = Nothing
        '            Call cpcore.cache.setObject(pageManager_cache_pageContent_cacheName, cache_pageContent)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError4(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_clear", True)
        '        End Sub
        '        '
        '        '====================================================================================================
        '        '   page content cache
        '        '====================================================================================================
        '        '
        '        Public Sub cache_pageContent_load(main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageContent_load")
        '            '
        '            Dim bag As String
        '            Dim Ticks As Integer
        '            Dim IDList2 As New stringBuilderLegacyController
        '            Dim IDList As String
        '            Dim PageName As String
        '            Dim CS As Integer
        '            'dim dt as datatable
        '            Dim Ptr As Integer
        '            Dim SQL As String
        '            Dim SelectList As String
        '            Dim SupportMetaContentNoFollow As Boolean
        '            Dim Criteria As String
        '            'dim buildversion As String
        '            Dim Id As Integer
        '            Dim ParentID As Integer
        '            Dim test As Object
        '            Dim arrayData() As Object
        '            Dim arrayTest As Object
        '            '
        '            ' Load cached PCC
        '            '
        '            cache_pageContent_idIndex = New keyPtrController
        '            cache_pageContent_parentIdIndex = New keyPtrController
        '            cache_pageContent_nameIndex = New keyPtrController
        '            cache_pageContent_rows = 0
        '            '
        '            On Error Resume Next
        '            If Not main_IsWorkflowRendering Then
        '                arrayTest = cpcore.cache.getObject(Of Object())(pageManager_cache_pageContent_cacheName)
        '                If Not IsNothing(arrayTest) Then
        '                    arrayData = DirectCast(arrayTest, Object())
        '                    If Not IsNothing(arrayData) Then
        '                        cache_pageContent = DirectCast(arrayData(0), String(,))
        '                        If Not IsNothing(cache_pageContent) Then
        '                            bag = DirectCast(arrayData(1), String)
        '                            If Err.Number = 0 Then
        '                                Call cache_pageContent_idIndex.importPropertyBag(bag)
        '                                If Err.Number = 0 Then
        '                                    bag = DirectCast(arrayData(2), String)
        '                                    If Err.Number = 0 Then
        '                                        Call cache_pageContent_nameIndex.importPropertyBag(bag)
        '                                        If Err.Number = 0 Then
        '                                            bag = DirectCast(arrayData(3), String)
        '                                            If Err.Number = 0 Then
        '                                                Call cache_pageContent_parentIdIndex.importPropertyBag(bag)
        '                                                If Err.Number = 0 Then
        '                                                    cache_pageContent_rows = UBound(cache_pageContent, 2) + 1
        '                                                End If
        '                                            End If
        '                                        End If
        '                                    End If
        '                                End If
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Err.Clear()
        '            On Error GoTo ErrorTrap
        '            If cache_pageContent_rows = 0 Then
        '                '
        '                Call debugController.debug_testPoint(cpcore, "pageManager_cache_pageContent_load, main_PCCCnt = 0, rebuild cache")
        '                '
        '                SelectList = pageManager_cache_pageContent_fieldList
        '                Criteria = ""
        '                cache_pageContent = cpcore.db.GetContentRows("Page Content", Criteria, , False, SystemMemberID, (main_IsWorkflowRendering Or main_IsQuickEditing), , SelectList)

        '                If (cache_pageContent.Length > 0) Then
        '                    cache_pageContent_rows = UBound(cache_pageContent, 2) + 1
        '                    If cache_pageContent_rows > 0 Then
        '                        '
        '                        ' build id and name indexes
        '                        '
        '                        Ticks = GetTickCount
        '                        For Ptr = 0 To cache_pageContent_rows - 1
        '                            Id = genericController.EncodeInteger(cache_pageContent(PCC_ID, Ptr))
        '                            PageName = genericController.encodeText(cache_pageContent(PCC_Name, Ptr))
        '                            IDList2.Add("," & CStr(Id))
        '                            Call cache_pageContent_idIndex.setPtr(genericController.encodeText(Id), Ptr)
        '                            If PageName <> "" Then
        '                                Call cache_pageContent_nameIndex.setPtr(PageName, Ptr)
        '                            End If
        '                            '
        '                            ' if menulinkoverride is encoded, decode it
        '                            '
        '                            If genericController.vbInstr(1, cache_pageContent(PCC_Link, Ptr), "%") <> 0 Then
        '                                cache_pageContent(PCC_Link, Ptr) = cpcore.htmlDoc.main_DecodeUrl(cache_pageContent(PCC_Link, Ptr))
        '                            End If
        '                        Next
        '                        IDList = IDList2.Text
        '                        '
        '                        ' build parentid list -- after id list to check for orphas
        '                        '
        '                        For Ptr = 0 To cache_pageContent_rows - 1
        '                            ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, Ptr))
        '                            If (InStr(1, "," & IDList & ",", "," & ParentID & ",") = 0) Then
        '                                ParentID = 0
        '                            End If
        '                            Call cache_pageContent_parentIdIndex.setPtr(genericController.encodeText(ParentID), Ptr)
        '                        Next
        '                        Call cache_pageContent_save()
        '                    End If
        '                End If
        '                '
        '                Call debugController.debug_testPoint(cpcore, "pageManager_cache_pageContent_load, building took [" & (GetTickCount - Ticks) & " msec]")
        '                '
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description & "", "pageManager_cache_pageContent_load", True, False)
        '        End Sub
        '        '
        '        '
        '        '
        '        Public Sub cache_pageContent_save()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_pageContent_save")
        '            '
        '            Dim cacheArray() As Object
        '            ReDim cacheArray(3)
        '            '
        '            If Not pagemanager_IsWorkflowRendering() Then
        '                Call cache_pageContent_idIndex.getPtr("test")
        '                Call cache_pageContent_nameIndex.getPtr("test")
        '                Call cache_pageContent_parentIdIndex.getPtr("test")
        '                '
        '                cacheArray(0) = cache_pageContent
        '                cacheArray(1) = cache_pageContent_idIndex.exportPropertyBag
        '                cacheArray(2) = cache_pageContent_nameIndex.exportPropertyBag
        '                cacheArray(3) = cache_pageContent_parentIdIndex.exportPropertyBag
        '                Call cpcore.cache.setObject(pageManager_cache_pageContent_cacheName, cacheArray)
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_pageContent_save")
        '        End Sub
        '        '
        '        '====================================================================================================
        '        '   Returns a pointer into the main_pcc(x,ptr) array
        '        '====================================================================================================
        '        '
        '        Public Function cache_pageContent_getPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageContent_getPtr")
        '            '
        '            Dim CS As Integer
        '            Dim Ptr As Integer
        '            Dim RS As Object
        '            '
        '            cache_pageContent_getPtr = -1
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            If (PageID > 0) Then
        '                '
        '                ' pageid=0 just loads cache and returns -1 ptr
        '                '
        '                If cache_pageContent_rows <= 0 Then
        '                    '
        '                ElseIf (cache_pageContent_idIndex Is Nothing) Then
        '                    '
        '                Else
        '                    cache_pageContent_getPtr = cache_pageContent_idIndex.getPtr(CStr(PageID))
        '                    If cache_pageContent_getPtr < 0 Then
        '                        '
        '                        ' This PageID is missing from cache - try to reload
        '                        '
        '                        Call logController.appendLog(cpcore, "pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] not found in index, attempting cache reload")
        '                        Call cache_pageContent_clear()
        '                        Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '                        If cache_pageContent_rows > 0 Then
        '                            cache_pageContent_getPtr = cache_pageContent_idIndex.getPtr(CStr(PageID))
        '                        End If
        '                        If (cache_pageContent_getPtr < 0) Then
        '                            ' do not through error, this can happen if someone deletes a page.
        '                            Call logController.appendLog(cpcore, "pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] not found in cache after reload. ERROR")
        '                            'Call Err.Raise(ignoreInteger, "cpCoreClass", "pageManager_cache_pageContent_getPtr, pageID [" & PageID & "] reload failed. ERROR")
        '                            'Call AppendLog("pageManager_cache_pageContent_getPtr, pageID[" & PageID & "] reload failed. ERROR")
        '                        End If
        '                        'CS = app.csOpen("page content", "id=" & PageID, "id", , , , "ID")
        '                        'If app.csv_IsCSOK(CS) Then
        '                        '    Call pageManager_cache_pageContent_updateRow(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
        '                        '    If (main_PCCCnt > 0) And Not (main_PCCIDIndex Is Nothing) Then
        '                        '        pageManager_cache_pageContent_getPtr = main_PCCIDIndex.GetPointer(CStr(PageID))
        '                        '    End If
        '                        'End If
        '                        'Call app.closeCS(CS)
        '                    End If
        '                End If
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getPtr", True, False)
        '        End Function
        '        '
        '        '====================================================================================================
        '        '   Returns a pointer into the pcc(x,ptr) array
        '        '====================================================================================================
        '        '
        '        Public Function cache_pageContent_get(main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As String(,)
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCC")
        '            '
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            cache_pageContent_get = cache_pageContent
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_get", True, False)
        '        End Function
        '        '
        '        '====================================================================================================
        '        '   Returns a pointer into the pcc(x,ptr) array for the first child page
        '        '====================================================================================================
        '        '
        '        Public Function cache_pageContent_getFirstChildPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCFirstChildPtr")
        '            '
        '            Dim CS As Integer
        '            Dim Ptr As Integer
        '            '
        '            cache_pageContent_getFirstChildPtr = -1
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            If cache_pageContent_rows > 0 Then
        '                Ptr = cache_pageContent_parentIdIndex.getPtr(CStr(PageID))
        '                If Ptr >= 0 Then
        '                    cache_pageContent_getFirstChildPtr = Ptr
        '                End If
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getFirstChildPtr", True, False)
        '        End Function
        '        '
        '        '====================================================================================================
        '        '   Returns a pointer into the pcc(x,ptr) array for the first child page
        '        '====================================================================================================
        '        '
        '        Public Function cache_pageContent_getFirstNamePtr(PageName As String, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCFirstNamePtr")
        '            '
        '            Dim CS As Integer
        '            Dim Ptr As Integer
        '            '
        '            cache_pageContent_getFirstNamePtr = -1
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            If cache_pageContent_rows > 0 Then
        '                Ptr = cache_pageContent_nameIndex.getPtr(PageName)
        '                If Ptr >= 0 Then
        '                    cache_pageContent_getFirstNamePtr = Ptr
        '                End If
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getFirstNamePtr", True, False)
        '        End Function
        '        '
        '        '
        '        '
        '        Public Sub cache_pageContent_updateRow(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
        '            ' must clear because there is no way to updae indexes
        '            Call cache_pageContent_clear()
        '            Exit Sub
        '            '
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim PageName As String
        '            Dim CS As Integer
        '            'dim dt as datatable
        '            Dim ColPtr As Integer
        '            Dim SQL As String
        '            Dim SelectList As String
        '            Dim SupportMetaContentNoFollow As Boolean
        '            Dim Criteria As String
        '            'dim buildversion As String
        '            Dim Id As Integer
        '            Dim ParentID As Integer
        '            Dim test As Object
        '            Dim PCCRow As String(,)
        '            Dim RowPtr As Integer
        '            '
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            If cache_pageContent_rows > 0 Then
        '                SelectList = pageManager_cache_pageContent_fieldList
        '                For RowPtr = 0 To cache_pageContent_rows - 1
        '                    If genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr)) = PageID Then
        '                        Exit For
        '                    End If
        '                Next
        '                Criteria = "ID=" & PageID
        '                CS = cpcore.db.cs_open("Page Content", Criteria, , False, ,,, SelectList)
        '                If Not cpcore.db.cs_ok(CS) Then
        '                    '
        '                    ' Page Not Found
        '                    '
        '                    Call cache_pageContent_removeRow(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
        '                Else
        '                    PCCRow = cpcore.db.cs_getRows(CS)
        '                    '
        '                    ' page was found in the Db - find the entry in PCC
        '                    '
        '                    If RowPtr = cache_pageContent_rows Then
        '                        '
        '                        ' Page not found in PCC - add a new entry
        '                        '
        '                        cache_pageContent_rows = cache_pageContent_rows + 1
        '                        ReDim Preserve cache_pageContent(PCC_ColCnt - 1, cache_pageContent_rows - 1)
        '                    End If
        '                    '
        '                    ' Transfer data from Db data to the PCC
        '                    '
        '                    For ColPtr = 0 To UBound(cache_pageContent, 1)
        '                        cache_pageContent(ColPtr, RowPtr) = PCCRow(ColPtr, 0)
        '                    Next
        '                    '
        '                    ' build id and name indexes
        '                    '
        '                    Id = genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr))
        '                    PageName = genericController.encodeText(cache_pageContent(PCC_Name, RowPtr))
        '                    '
        '                    Call cache_pageContent_idIndex.setPtr(genericController.encodeText(Id), RowPtr)
        '                    '
        '                    If PageName <> "" Then
        '                        Call cache_pageContent_nameIndex.setPtr(PageName, RowPtr)
        '                    End If
        '                    '
        '                    If genericController.vbInstr(1, cache_pageContent(PCC_Link, RowPtr), "%") <> 0 Then
        '                        cache_pageContent(PCC_Link, RowPtr) = cpcore.htmlDoc.main_DecodeUrl(cache_pageContent(PCC_Link, RowPtr))
        '                    End If
        '                    '
        '                    ParentID = genericController.EncodeInteger(cache_pageContent(PCC_ParentID, RowPtr))
        '                    Call cache_pageContent_parentIdIndex.setPtr(genericController.encodeText(ParentID), RowPtr)
        '                    '
        '                    Call cache_pageContent_save()
        '                End If
        '                Call cpcore.db.cs_Close(CS)
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_updateRow", True, False)
        '        End Sub
        '        '
        '        '
        '        '
        '        Public Sub cache_pageContent_removeRow(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim PageName As String
        '            Dim CS As Integer
        '            'dim dt as datatable
        '            Dim ColPtr As Integer
        '            Dim SQL As String
        '            Dim SelectList As String
        '            Dim SupportMetaContentNoFollow As Boolean
        '            Dim Criteria As String
        '            'dim buildversion As String
        '            Dim Id As Integer
        '            Dim ParentID As Integer
        '            Dim test As Object
        '            Dim PCCRow As Object
        '            Dim RowPtr As Integer
        '            '
        '            '   can not remove rows from index - temp fix - do not remove rows from cache
        '            '
        '            Call cache_pageContent_clear()
        '            Exit Sub
        '            '
        '            If cache_pageContent_rows = 0 Then
        '                Call cache_pageContent_load(main_IsWorkflowRendering, main_IsQuickEditing)
        '            End If
        '            If cache_pageContent_rows > 0 Then
        '                '
        '                ' Find the row in the PCC
        '                '
        '                For RowPtr = 0 To cache_pageContent_rows - 1
        '                    If genericController.EncodeInteger(cache_pageContent(PCC_ID, RowPtr)) = PageID Then
        '                        Exit For
        '                    End If
        '                Next
        '                If RowPtr < cache_pageContent_rows Then
        '                    '
        '                    ' Row was found
        '                    '
        '                    Do While RowPtr < (cache_pageContent_rows - 1)
        '                        For ColPtr = 0 To UBound(cache_pageContent, 1)
        '                            cache_pageContent(ColPtr, RowPtr) = cache_pageContent(ColPtr, RowPtr + 1)
        '                        Next
        '                        RowPtr = RowPtr + 1
        '                    Loop
        '                    cache_pageContent_rows = cache_pageContent_rows - 1
        '                    ReDim Preserve cache_pageContent(PCC_ColCnt - 1, cache_pageContent_rows - 1)
        '                    If False Then
        '                        ReDim Preserve cache_pageContent(PCC_ColCnt - 1, cache_pageContent_rows)
        '                    End If
        '                End If
        '                If Not main_IsWorkflowRendering Then
        '                    Call cpcore.cache.setObject("PCC", cache_pageContent)
        '                End If
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_removeRow", True, False)
        '        End Sub
        '        '
        '        '
        '        '
        '        Public Function cache_pageContent_getPtrsSorted(PCCPtrs() As Integer, OrderByCriteria As String) As Integer()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCPtrsSorted")
        '            '
        '            Dim PtrStart As Integer
        '            Dim PtrEnd As Integer
        '            Dim PtrStep As Integer
        '            Dim PCCRowPtr As Integer
        '            Dim Ptr As Integer
        '            Dim Index As keyPtrController
        '            Dim PCCSortFieldPtr As Integer
        '            Dim SortForward As Boolean
        '            Dim SortFieldName As String
        '            Dim PtrCnt As Integer
        '            Dim SortPtr As Integer
        '            Dim SortSplitCnt As Integer
        '            Dim SortSplit() As String
        '            Dim PCCPtrCnt As Integer
        '            Dim fieldType As Integer
        '            Dim StringValue As String
        '            Dim LongValue As Integer
        '            Dim DblValue As Double
        '            Dim DateValue As Date
        '            Dim SortFieldValue As String
        '            '
        '            ' Sort the ptrs
        '            '
        '            PCCPtrCnt = UBound(PCCPtrs) + 1
        '            If PCCPtrCnt > 0 Then
        '                SortSplit = Split(OrderByCriteria, ",")
        '                SortSplitCnt = UBound(SortSplit) + 1
        '                For SortPtr = 0 To SortSplitCnt - 1
        '                    SortFieldName = genericController.vbLCase(SortSplit(SortPtr))
        '                    SortForward = True
        '                    If genericController.vbInstr(1, SortFieldName, " asc", vbTextCompare) <> 0 Then
        '                        SortFieldName = genericController.vbReplace(SortFieldName, " asc", "")
        '                    ElseIf genericController.vbInstr(1, SortFieldName, " desc", vbTextCompare) <> 0 Then
        '                        SortFieldName = genericController.vbReplace(SortFieldName, " desc", "")
        '                        SortForward = False
        '                    End If
        '                    PCCSortFieldPtr = cache_pageContent_getColPtr(SortFieldName)

        '                    Select Case SortFieldName
        '                        Case "id"
        '                            fieldType = FieldTypeIdInteger
        '                        Case "datearchive", "dateexpires", "pubdate", "dateadded", "modifieddate"
        '                            fieldType = FieldTypeIdDate
        '                        Case Else
        '                            fieldType = FieldTypeIdText
        '                    End Select
        '                    '
        '                    ' Store them in the index
        '                    '
        '                    If PCCSortFieldPtr >= 0 Then
        '                        Index = New keyPtrController
        '                        For Ptr = 0 To PCCPtrCnt - 1
        '                            PCCRowPtr = PCCPtrs(Ptr)
        '                            StringValue = genericController.encodeText(cache_pageContent(PCCSortFieldPtr, PCCRowPtr))
        '                            Select Case fieldType
        '                                Case FieldTypeIdInteger
        '                                    LongValue = CInt(DblValue)
        '                                    SortFieldValue = genericController.GetIntegerString(LongValue, 10)
        '                                Case FieldTypeIdDate
        '                                    If Not IsDate(StringValue) Then
        '                                        SortFieldValue = "000000000000000000"
        '                                    Else
        '                                        DateValue = CDate(StringValue)
        '                                        DblValue = DateValue.ToOADate * CDbl(1440)
        '                                        LongValue = CInt(DblValue)
        '                                        SortFieldValue = genericController.GetIntegerString(LongValue, 10)
        '                                    End If
        '                                Case Else
        '                                    SortFieldValue = StringValue
        '                            End Select
        '                            Call Index.setPtr(SortFieldValue, PCCRowPtr)

        '                        Next
        '                        '
        '                        ' Store them back into PCCPtrs() in the correct order
        '                        '
        '                        If SortForward Then
        '                            PtrStart = 0
        '                            PtrEnd = PCCPtrCnt - 1
        '                            PtrStep = 1
        '                        Else
        '                            PtrStart = PCCPtrCnt - 1
        '                            PtrEnd = 0
        '                            PtrStep = -1
        '                        End If

        '                        PCCRowPtr = Index.getFirstPtr
        '                        For Ptr = PtrStart To PtrEnd Step PtrStep
        '                            PCCPtrs(Ptr) = PCCRowPtr
        '                            PCCRowPtr = genericController.EncodeInteger(Index.getNextPtr)
        '                        Next
        '                    End If
        '                Next
        '            End If
        '            cache_pageContent_getPtrsSorted = PCCPtrs
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getPtrsSorted", True, False)
        '        End Function
        '        '
        '        '
        '        '
        '        Public Function cache_pageContent_getColPtr(FieldName As String) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetPCCColPtr")
        '            '
        '            cache_pageContent_getColPtr = -1
        '            Select Case genericController.vbLCase(FieldName)
        '                Case "active"
        '                    cache_pageContent_getColPtr = PCC_Active
        '                Case "allowchildlistdisplay"
        '                    cache_pageContent_getColPtr = PCC_AllowChildListDisplay
        '                Case "allowhitnotification"
        '                    cache_pageContent_getColPtr = PCC_AllowHitNotification
        '                Case "allowmetacontentnofollow"
        '                    cache_pageContent_getColPtr = PCC_AllowMetaContentNoFollow
        '                Case "blockcontent"
        '                    cache_pageContent_getColPtr = PCC_BlockContent
        '                Case "blockpage"
        '                    cache_pageContent_getColPtr = PCC_BlockPage
        '                Case "BlockSourceID"
        '                    cache_pageContent_getColPtr = PCC_BlockSourceID
        '                Case "brieffilename"
        '                    cache_pageContent_getColPtr = PCC_BriefFilename
        '                Case "childlistsortmethodid"
        '                    cache_pageContent_getColPtr = PCC_ChildListSortMethodID
        '                Case "childlistinstanceoptions"
        '                    cache_pageContent_getColPtr = PCC_ChildListInstanceOptions
        '                Case "issecure"
        '                    cache_pageContent_getColPtr = PCC_IsSecure
        '                Case "childpagesfound"
        '                    cache_pageContent_getColPtr = PCC_ChildPagesFound
        '                Case "contactmemberid"
        '                    cache_pageContent_getColPtr = PCC_ContactMemberID
        '                Case "contentcontrolid"
        '                    cache_pageContent_getColPtr = PCC_ContentControlID
        '                Case "copyFilename"
        '                    cache_pageContent_getColPtr = PCC_CopyFilename
        '                Case "customblockmessagefilename"
        '                    cache_pageContent_getColPtr = PCC_CustomBlockMessageFilename
        '                Case "datearchive"
        '                    cache_pageContent_getColPtr = PCC_DateArchive
        '                Case "dateexpires"
        '                    cache_pageContent_getColPtr = PCC_DateExpires
        '                Case "headline"
        '                    cache_pageContent_getColPtr = PCC_Headline
        '                Case "id"
        '                    cache_pageContent_getColPtr = PCC_ID
        '                Case "jsendbody"
        '                    cache_pageContent_getColPtr = PCC_JSEndBody
        '                Case "jshead"
        '                    cache_pageContent_getColPtr = PCC_JSHead
        '                Case "jsfilename"
        '                    cache_pageContent_getColPtr = PCC_JSFilename
        '                Case "jsonload"
        '                    cache_pageContent_getColPtr = PCC_JSOnLoad
        '                Case "link"
        '                    cache_pageContent_getColPtr = PCC_Link
        '                Case "menuheadline"
        '                    cache_pageContent_getColPtr = PCC_MenuHeadline
        '                Case "name"
        '                    cache_pageContent_getColPtr = PCC_Name
        '                Case "parentid"
        '                    cache_pageContent_getColPtr = PCC_ParentID
        '                Case "parentlistname"
        '                    cache_pageContent_getColPtr = PCC_ParentListName
        '                Case "pubdate"
        '                    cache_pageContent_getColPtr = PCC_PubDate
        '                Case "sortorder"
        '                    cache_pageContent_getColPtr = PCC_SortOrder
        '                Case "registrationgroupid"
        '                    cache_pageContent_getColPtr = PCC_RegistrationGroupID
        '                Case "templateid"
        '                    cache_pageContent_getColPtr = PCC_TemplateID
        '                Case "triggeraddgroupid"
        '                    cache_pageContent_getColPtr = PCC_TriggerAddGroupID
        '                Case "triggerconditiongroupid"
        '                    cache_pageContent_getColPtr = PCC_TriggerConditionGroupID
        '                Case "triggerconditionid"
        '                    cache_pageContent_getColPtr = PCC_TriggerConditionID
        '                Case "triggerremovegroupid"
        '                    cache_pageContent_getColPtr = PCC_TriggerRemoveGroupID
        '                Case "triggersendsystememailid"
        '                    cache_pageContent_getColPtr = PCC_TriggerSendSystemEmailID
        '                Case "viewings"
        '                    cache_pageContent_getColPtr = PCC_Viewings
        '                Case "dateadded"
        '                    cache_pageContent_getColPtr = PCC_DateAdded
        '                Case "modifieddate"
        '                    cache_pageContent_getColPtr = PCC_ModifiedDate
        '                Case "allowinmenus"
        '                    cache_pageContent_getColPtr = PCC_AllowInMenus
        '                Case "allowinchildlists"
        '                    cache_pageContent_getColPtr = PCC_AllowInChildLists
        '            End Select
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageContent_getColPtr", True, False)
        '        End Function
        '        '
        '        '
        '        '
        '        'Public Function main_ImportCollectionFile(CollectionFilename As String) As Boolean
        '        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("ImportCollectionFile")
        '        '    '
        '        '        main_ImportCollectionFile = main_ImportCollection(app.publicFiles.ReadFile(CollectionFilename), False)
        '        '    '
        '        '    Exit Function
        '        'ErrorTrap:
        '        '    Call main_HandleClassError_RevArgs(Err.Number, Err.Source, Err.Description, "main_ImportCollectionFile", True, False)
        '        '    End Function
        '        '        '
        '        '        '
        '        '        '
        '        '        Public Function main_ImportCollection(ByVal CollectionFileData As String, IsNewBuild As Boolean) As Boolean
        '        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("ImportCollection")
        '        '            '
        '        '            Dim builder As New builderClass(me)
        '        '            '
        '        '            Call app.publicFiles.SaveFile("Install/" & CStr(main_getRandomInteger()) & ".xml", CollectionFileData)
        '        '            Call builder.InstallAddons(IsNewBuild)
        '        '            '
        '        '            Exit Function
        '        'ErrorTrap:
        '        '            Call main_HandleClassError_RevArgs(Err.Number, Err.Source, Err.Description, "main_ImportCollection", True, False)
        '        '        End Function
        '        '
        '        '
        '        '
        '        '
        '        '==========================================================================================================
        '        '
        '        '==========================================================================================================
        '        '
        '        Friend Sub pageManager_cache_siteSection_load()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("IISReset")
        '            '
        '            Dim IDText As String
        '            Dim Name As String
        '            Dim Id As Integer
        '            Dim Ptr As Integer
        '            Dim list As String
        '            Dim styleId As Integer
        '            Dim SelectList As String
        '            Dim SQL As String
        '            Dim SSCSize As Integer
        '            Dim LastRecordID As Integer
        '            Dim RecordID As Integer
        '            Dim SSCArray() As String
        '            Dim hint As String
        '            Dim LoadIndexes As Boolean
        '            Dim SaveCache As Boolean
        '            '
        '            Dim cacheObject As Object()
        '            Dim cacheTest As Object
        '            Dim bag As String
        '            '
        '            ' Load cache
        '            '
        '            cache_siteSection_rows = 0
        '            cache_siteSection_IDIndex = New keyPtrController
        '            cache_siteSection_RootPageIDIndex = New keyPtrController
        '            cache_siteSection_NameIndex = New keyPtrController
        '            '
        '            On Error Resume Next
        '            If Not pagemanager_IsWorkflowRendering() Then
        '                cacheTest = cpcore.cache.getObject(Of Object())(cache_siteSection_cacheName)
        '                If Not IsNothing(cacheTest) Then
        '                    cacheObject = DirectCast(cacheTest, Object())
        '                    If Not IsNothing(cacheObject) Then
        '                        cache_siteSection = DirectCast(cacheObject(0), String(,))
        '                        If Not IsNothing(cache_siteSection) Then
        '                            bag = DirectCast(cacheObject(1), String)
        '                            If Err.Number = 0 Then
        '                                Call cache_siteSection_IDIndex.importPropertyBag(bag)
        '                                If Err.Number = 0 Then
        '                                    bag = DirectCast(cacheObject(2), String)
        '                                    If Err.Number = 0 Then
        '                                        Call cache_siteSection_RootPageIDIndex.importPropertyBag(bag)
        '                                        If Err.Number = 0 Then
        '                                            bag = DirectCast(cacheObject(3), String)
        '                                            If Err.Number = 0 Then
        '                                                Call cache_siteSection_NameIndex.importPropertyBag(bag)
        '                                                If Err.Number = 0 Then
        '                                                    cache_siteSection_rows = UBound(cache_siteSection, 2) + 1
        '                                                End If
        '                                            End If
        '                                        End If
        '                                    End If
        '                                End If
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Err.Clear()
        '            On Error GoTo ErrorTrap
        '            If cache_siteSection_rows = 0 Then
        '                SelectList = "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody,JSFilename"
        '                cache_siteSection = cpcore.db.GetContentRows("Site Sections", "(active<>0)", , False, SystemMemberID, (pagemanager_IsWorkflowRendering()), , SelectList)
        '                cache_siteSection_rows = UBound(cache_siteSection, 2) + 1
        '                For Ptr = 0 To cache_siteSection_rows - 1
        '                    '
        '                    ' ID Index
        '                    '
        '                    IDText = genericController.encodeText(cache_siteSection(SSC_ID, Ptr))
        '                    Call cache_siteSection_IDIndex.setPtr(IDText, Ptr)
        '                    '
        '                    ' RootPageID Index
        '                    '
        '                    Id = genericController.EncodeInteger(cache_siteSection(SSC_RootPageID, Ptr))
        '                    If Id <> 0 Then
        '                        Call cache_siteSection_RootPageIDIndex.setPtr(genericController.encodeText(Id), Ptr)
        '                    End If
        '                    '
        '                    ' Name Index
        '                    '
        '                    Name = genericController.encodeText(cache_siteSection(SSC_Name, Ptr))
        '                    If Name <> "" Then
        '                        Call cache_siteSection_NameIndex.setPtr(Name, Ptr)
        '                    End If
        '                Next
        '                Call pageManager_cache_siteSection_save()
        '            End If
        '            '

        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_siteSection_load")
        '        End Sub
        '        '
        '        '
        '        '
        '        Friend Sub pageManager_cache_siteSection_save()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_siteSection_save")
        '            '
        '            Dim hint As String
        '            Dim cacheArray() As Object
        '            ReDim cacheArray(3)
        '            '
        '            Call cache_siteSection_IDIndex.getPtr("test")
        '            Call cache_siteSection_RootPageIDIndex.getPtr("test")
        '            Call cache_siteSection_NameIndex.getPtr("test")
        '            '
        '            cacheArray(0) = cache_siteSection
        '            cacheArray(1) = cache_siteSection_IDIndex.exportPropertyBag
        '            cacheArray(2) = cache_siteSection_RootPageIDIndex.exportPropertyBag
        '            cacheArray(3) = cache_siteSection_NameIndex.exportPropertyBag
        '            Call cpcore.cache.setObject(cache_siteSection_cacheName, cacheArray)
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_siteSection_save")
        '        End Sub
        '        '
        '        '====================================================================================================
        '        '
        '        '====================================================================================================
        '        '
        '        Public Function pageManager_cache_siteSection_getPtr(Id As Integer) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_siteSection_getPtr")
        '            '
        '            Dim CS As Integer
        '            Dim Ptr As Integer
        '            '
        '            pageManager_cache_siteSection_getPtr = -1
        '            If Id > 0 Then
        '                If cache_siteSection_rows = 0 Then
        '                    Call pageManager_cache_siteSection_load()
        '                End If
        '                If cache_siteSection_rows > 0 Then
        '                    Ptr = cache_siteSection_IDIndex.getPtr(CStr(Id))
        '                    If Ptr >= 0 Then
        '                        pageManager_cache_siteSection_getPtr = Ptr
        '                    End If
        '                End If
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("pageManager_cache_siteSection_getPtr")
        '        End Function
        '        '
        '        '====================================================================================================
        '        '
        '        '====================================================================================================
        '        '
        '        Public Sub pageManager_cache_siteSection_clear()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_siteSection_clear")
        '            '
        '            Call cpcore.cache.invalidateContent("site sections")
        '            cache_siteSection = {}
        '            cache_siteSection_rows = 0
        '            Call cpcore.cache.setObject(cache_siteSection_cacheName, cache_siteSection)
        '            'Call cmc_siteSectionCache_clear
        '            '
        '            Exit Sub
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_siteSection_clear")
        '        End Sub
        '        '
        '        '====================================================================================================
        '        '
        '        '====================================================================================================
        '        '
        '        Public Function pageManager_cache_siteSection_get() As Object
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetSSC")
        '            '
        '            If cache_siteSection_rows = 0 Then
        '                Call pageManager_cache_siteSection_load()
        '            End If
        '            pageManager_cache_siteSection_get = cache_siteSection
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "pageManager_cache_siteSection_get", True, False)
        '        End Function
        '        '
        '        '
        '        '
        '        Public Sub pageManager_cache_pageTemplate_load()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageTemplate_load")
        '            '
        '            ' Dim rsdomains as datatable
        '            Dim ruleList As String
        '            Dim IsSecure As Boolean
        '            Dim Id As Integer
        '            Dim Ptr As Integer
        '            Dim list As String
        '            Dim styleId As Integer
        '            Dim SelectList As String
        '            'dim dt as datatable
        '            Dim SQL As String
        '            Dim TCSize As Integer
        '            Dim LastTemplateID As Integer
        '            Dim templateId As Integer
        '            Dim cacheArray As String(,)
        '            Dim arrayData() As Object
        '            Dim arrayTest As Object
        '            Dim bag As String
        '            '
        '            ' Load cached TC
        '            '
        '            cache_pageTemplate_rows = 0
        '            cache_pageTemplate_contentIdindex = New keyPtrController
        '            '
        '            On Error Resume Next
        '            If Not pagemanager_IsWorkflowRendering() Then
        '                arrayTest = cpcore.cache.getObject(Of Object())(cache_pageTemplate_cacheName)
        '                If Not IsNothing(arrayTest) Then
        '                    arrayData = DirectCast(arrayTest, Object())
        '                    If Not IsNothing(arrayData) Then
        '                        cache_pageTemplate = DirectCast(arrayData(0), String(,))
        '                        If Not IsNothing(cache_pageTemplate) Then
        '                            bag = DirectCast(arrayData(1), String)
        '                            If Err.Number = 0 Then
        '                                Call cache_pageTemplate_contentIdindex.importPropertyBag(bag)
        '                                If Err.Number = 0 Then
        '                                    cache_pageTemplate_rows = UBound(cache_pageContent, 2) + 1
        '                                End If
        '                            End If
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Err.Clear()
        '            On Error GoTo ErrorTrap
        '            '    If Not main_IsWorkflowRendering Then
        '            '        cache_pageTemplate = csv_Getcache(pageManager_cache_pageTemplate_cacheName)
        '            '        If IsEmpty(cache_pageTemplate) Then
        '            '            cache_pageTemplateCnt = 0
        '            '        ElseIf Not IsArray(cache_pageTemplate) Then
        '            '            cache_pageTemplateCnt = 0
        '            '        ElseIf IsNull(cache_pageTemplate) Then
        '            '            cache_pageTemplateCnt = 0
        '            '        Else
        '            '            cacheArray = cache_pageTemplate
        '            '            cache_pageTemplateCnt = UBound(cacheArray, 2) + 1
        '            '        End If
        '            '    End If
        '            If cache_pageTemplate_rows = 0 Then
        '                '
        '                ' Load cache
        '                '
        '                SelectList = ""
        '                SQL = "select t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID,t.MobileStylesFilename,t.MobileBodyHTML,OtherHeadTags,BodyTag,t.JSFilename,t.IsSecure as IsSecure" _
        '                    & " from ccTemplates t" _
        '                    & " Left Join ccSharedStylesTemplateRules r on r.templateid=t.id" _
        '                    & " where (t.active<>0)" _
        '                    & " order by t.id"
        '                Dim dt As DataTable = cpcore.db.executeSql(SQL)
        '                If dt.Rows.Count > 0 Then
        '                    For Each rsDr As DataRow In dt.Rows
        '                        templateId = genericController.EncodeInteger(rsDr("ID"))
        '                        styleId = genericController.EncodeInteger(rsDr("styleid"))
        '                        If (templateId = LastTemplateID) Then
        '                            '
        '                            ' Another style for the same template
        '                            '
        '                            If styleId <> 0 Then
        '                                list = cacheArray(TC_SharedStylesIDList, Ptr)
        '                                If list <> "" Then
        '                                    list = list & ","
        '                                End If
        '                                cacheArray(TC_SharedStylesIDList, Ptr) = list & styleId
        '                            End If
        '                        Else
        '                            '
        '                            ' New template
        '                            '
        '                            LastTemplateID = templateId
        '                            If cache_pageTemplate_rows >= TCSize Then
        '                                TCSize = TCSize + 100
        '                                ReDim Preserve cacheArray(TC_cnt, TCSize)
        '                            End If
        '                            Ptr = cache_pageTemplate_rows
        '                            cacheArray(TC_ID, Ptr) = genericController.EncodeInteger(rsDr("ID")).ToString
        '                            cacheArray(TC_JSEndBody, Ptr) = genericController.encodeText(rsDr("JSEndBody"))
        '                            cacheArray(TC_JSInHeadLegacy, Ptr) = genericController.encodeText(rsDr("JSHead"))
        '                            cacheArray(TC_JSInHeadFilename, Ptr) = genericController.encodeText(rsDr("JSFilename"))
        '                            cacheArray(TC_JSOnLoad, Ptr) = genericController.encodeText(rsDr("JSOnLoad"))
        '                            cacheArray(TC_Name, Ptr) = genericController.encodeText(rsDr("Name"))
        '                            cacheArray(TC_Link, Ptr) = main_verifyTemplateLink(genericController.encodeText(rsDr("Link")))
        '                            '
        '                            cacheArray(TC_BodyHTML, Ptr) = genericController.encodeText(rsDr("BodyHTML"))
        '                            cacheArray(TC_SharedStylesIDList, Ptr) = styleId.ToString
        '                            cacheArray(TC_StylesFilename, Ptr) = genericController.encodeText(rsDr("StylesFilename"))
        '                            '
        '                            cacheArray(TC_MobileBodyHTML, Ptr) = genericController.encodeText(rsDr("MobileBodyHTML"))
        '                            ' do not support shared styles on Mobile templates yet
        '                            'cacheArray(TC_MobileSharedStylesIDList, Ptr) = StyleID
        '                            cacheArray(TC_MobileStylesFilename, Ptr) = genericController.encodeText(rsDr("MobileStylesFilename"))
        '                            cacheArray(TC_OtherHeadTags, Ptr) = genericController.encodeText(rsDr("OtherHeadTags"))
        '                            cacheArray(TC_BodyTag, Ptr) = genericController.encodeText(rsDr("BodyTag"))
        '                            cacheArray(TC_IsSecure, Ptr) = genericController.EncodeBoolean(rsDr("IsSecure")).ToString
        '                            '
        '                            ' gather c.domains for this templates
        '                            '
        '                            SQL = "select domainid from ccDomainTemplateRules where templateid=" & templateId
        '                            Dim dtdomains As DataTable = cpcore.db.executeSql(SQL)
        '                            If dtdomains.Rows.Count > 0 Then
        '                                cacheArray(TC_DomainIdList, Ptr) = genericController.encodeText(dtdomains.Rows.Item(0))
        '                                'cacheArray(TC_DomainIdList, Ptr) = rsdomains.GetString(StringFormatEnum.adClipString, , "", ",")
        '                            Else
        '                                cacheArray(TC_DomainIdList, Ptr) = ""
        '                            End If
        '                            cache_pageTemplate_rows = cache_pageTemplate_rows + 1
        '                        End If

        '                    Next
        '                End If


        '                '
        '                cache_pageTemplate = cacheArray
        '                '
        '                If cache_pageTemplate_rows > 0 Then
        '                    cache_pageTemplate_contentIdindex = New keyPtrController
        '                    For Ptr = 0 To cache_pageTemplate_rows - 1
        '                        Id = genericController.EncodeInteger(cache_pageTemplate(TC_ID, Ptr))
        '                        Call cache_pageTemplate_contentIdindex.setPtr(genericController.encodeText(Id), Ptr)
        '                    Next
        '                End If
        '                If Not pagemanager_IsWorkflowRendering() Then
        '                    Call pageManager_cache_pageTemplate_save()
        '                End If
        '            End If
        '            '
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_pageTemplate_load")
        '        End Sub
        '        '
        '        '
        '        '
        '        Public Sub pageManager_cache_pageTemplate_save()
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.pageManager_cache_pageTemplate_save")
        '            '
        '            Dim cacheArray() As Object
        '            ReDim cacheArray(1)
        '            '
        '            Call cache_pageTemplate_contentIdindex.getPtr("test")
        '            '
        '            cacheArray(0) = cache_pageTemplate
        '            cacheArray(1) = cache_pageTemplate_contentIdindex.exportPropertyBag
        '            Call cpcore.cache.setObject(cache_pageTemplate_cacheName, cacheArray)
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("pageManager_cache_pageTemplate_save")
        '        End Sub
        '        '
        '        '
        '        '
        '        Public Sub pageManager_cache_pageTemplate_clear()
        '            On Error GoTo ErrorTrap 'Const Tn = "pageManager_cache_pageTemplate_clear": 'Dim th as integer: th = profileLogMethodEnter(Tn)
        '            '
        '            cache_pageTemplate_rows = 0
        '            cache_pageTemplate = {}
        '            Call cpcore.cache.setObject(cache_pageTemplate_cacheName, cache_pageTemplate)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError4(Err.Number, Err.Source, Err.Description, "pageManager_cache_pageTemplate_clear", True)
        '        End Sub
        '        '
        '        '====================================================================================================
        '        '   Returns a pointer into the cache_pageTemplate(x,ptr) array
        '        '====================================================================================================
        '        '
        '        Public Function pageManager_cache_pageTemplate_getPtr(Id As Integer) As Integer
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("pageManager_cache_pageTemplate_getPtr")
        '            '
        '            Dim CS As Integer
        '            Dim Ptr As Integer
        '            '
        '            pageManager_cache_pageTemplate_getPtr = -1
        '            If cache_pageTemplate_rows = 0 Then
        '                Call pageManager_cache_pageTemplate_load()
        '            End If
        '            If cache_pageTemplate_rows > 0 Then
        '                Ptr = cache_pageTemplate_contentIdindex.getPtr(CStr(Id))
        '                If Ptr >= 0 Then
        '                    pageManager_cache_pageTemplate_getPtr = Ptr
        '                End If
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("pageManager_cache_pageTemplate_getPtr")
        '        End Function
        '
        '====================================================================================================
        '   main_GetTemplateLink
        '       Added to externals (aoDynamicMenu) can main_Get hard template links
        '====================================================================================================
        '
        Public Function main_GetTemplateLink(templateId As Integer) As String
            Dim template As pageTemplateModel = pageTemplateModel.create(cpcore, templateId, New List(Of String))
            If template IsNot Nothing Then
                Return template.Link
            End If
            Return ""
        End Function
        '        '
        '        '=========================================================================================
        '        '   main_GetTCLink
        '        '       - try just returning the link field, and handling TC_isSecure with the PCC_IsSecure later
        '        '       - will also need to handle TC_domainId at some point anyway
        '        '
        '        '=========================================================================================
        '        '
        '        Friend Function main_GetTCLink(TCPtr As Integer) As String
        '            '
        '            If TCPtr >= 0 Then
        '                main_GetTCLink = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
        '            End If
        '            Exit Function
        '            '
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetTCLink")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim Link As String
        '            Dim templateSecure As Boolean
        '            '
        '            If TCPtr >= 0 Then
        '                Link = genericController.encodeText(cache_pageTemplate(TC_Link, TCPtr))
        '                templateSecure = genericController.EncodeBoolean(cache_pageTemplate(TC_IsSecure, TCPtr))
        '                If Link <> "" Then
        '                    '
        '                    ' Link is included in template
        '                    '
        '                    If genericController.vbInstr(1, Link, "://", vbTextCompare) <> 0 Then
        '                        '
        '                        ' Template link is Full URL, IsSecure checkbox does nothing
        '                        '
        '                    Else
        '                        '
        '                        ' Template Link is short, verify it first
        '                        '
        '                        If Mid(Link, 1, 1) <> "/" Then
        '                            Link = "/" & Link
        '                        End If
        '                        Link = genericController.ConvertLinkToShortLink(Link, cpcore.webServer.requestDomain, cpcore.webServer.webServerIO_requestVirtualFilePath)
        '                        Link = genericController.EncodeAppRootPath(Link, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
        '                        If templateSecure And (Not cpcore.webServer.requestSecure) Then
        '                            '
        '                            ' Short Link, and IsSecure checked but current page is not secure
        '                            '
        '                            Link = "https://" & cpcore.webServer.requestDomain & Link
        '                        ElseIf cpcore.webServer.requestSecure And (Not templateSecure) Then
        '                            ' (*E) comment out this
        '                            '
        '                            ' Short link, Template is not secure, but current page is
        '                            '
        '                            Link = "http://" & cpcore.webServer.requestDomain & Link
        '                        End If
        '                    End If
        '                Else
        '                    '
        '                    ' Link is not included in template
        '                    '
        '                    If templateSecure And (Not cpcore.webServer.requestSecure) Then
        '                        '
        '                        ' Secure template but current page is not secure - return default link with ssl
        '                        '
        '                        Link = "https://" & cpcore.webServer.requestDomain & requestAppRootPath & cpcore.siteProperties.serverPageDefault
        '                    ElseIf cpcore.webServer.requestSecure And (Not templateSecure) Then
        '                        ' (*E) comment out this
        '                        '
        '                        ' Short link, Template is not secure, but current page is  - return default link with ssl
        '                        '
        '                        ' (*D)
        '                        ' this is the problem
        '                        ' the site should hard redirect to a non-secure template if the page, parent pages AND the template are not secure
        '                        ' what is happening here is a page is set secure, it redirects to the secure link then this
        '                        ' happens during the secure page draw.
        '                        '
        '                        Link = "http://" & cpcore.webServer.requestDomain & requestAppRootPath & cpcore.siteProperties.serverPageDefault
        '                    End If
        '                End If
        '                main_GetTCLink = Link
        '            End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTCLink")
        '        End Function
        '        '
        '        Public Function main_GetPCCPtr(PageID As Integer, main_IsWorkflowRendering As Boolean, main_IsQuickEditing As Boolean) As Integer
        '            main_GetPCCPtr = cache_pageContent_getPtr(PageID, main_IsWorkflowRendering, main_IsQuickEditing)
        '        End Function

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
            CS = cpcore.db.cs_open(ContentName, "parentid=" & RecordID, , , , ,, "ID")
            Do While cpcore.db.cs_ok(CS)
                pageManager_DeleteChildRecords = pageManager_DeleteChildRecords & "," & cpcore.db.cs_getInteger(CS, "ID")
                cpcore.db.cs_goNext(CS)
            Loop
            Call cpcore.db.cs_Close(CS)
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
                    QuickEditing = cpcore.authContext.isQuickEditing(cpcore, "page content")
                    For Ptr = 0 To IDCnt - 1
                        Call cpcore.db.deleteContentRecord("page content", genericController.EncodeInteger(IDs(Ptr)))
                        'Call cache_pageContent_removeRow(genericController.EncodeInteger(IDs(Ptr)), pagemanager_IsWorkflowRendering, QuickEditing)
                    Next
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_DeleteChildRecords")
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
            Call cpcore.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
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
            Dim CDef As cdefModel
            Dim ModifiedDate As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            '
            MethodName = "main_GetAuthoringButtons"
            '
            ' main_Get Authoring Workflow Status
            '
            If RecordID <> 0 Then
                Call cpcore.workflow.getAuthoringStatus(ContentName, RecordID, IsSubmitted, IsApproved, SubmittedName, ApprovedName, IsInserted, IsDeleted, IsModified, ModifiedName, ModifiedDate, SubmittedDate, ApprovedDate)
            End If
            '
            ' main_Get Content Definition
            '
            CDef = cpcore.metaData.getCdef(ContentName)
            '
            ' Set Buttons based on Status
            '
            If IsEditing Then
                '
                ' Edit Locked
                '
                AllowCancel = True
                readOnlyField = True
            ElseIf (Not cpcore.siteProperties.allowWorkflowAuthoring) Or (Not CDef.AllowWorkflowAuthoring) Then
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
            ElseIf cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
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
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
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
            Dim CDef As cdefModel
            Dim Copy As String
            Dim Link As String
            Dim FromAddress As String
            '
            MethodName = "main_SendPublishSubmitNotice"
            '
            FromAddress = cpcore.siteProperties.getText("EmailPublishSubmitFrom", cpcore.siteProperties.emailAdmin)
            CDef = cpcore.metaData.getCdef(ContentName)
            Link = cpcore.siteProperties.adminURL & "?af=" & AdminFormPublishing
            Copy = Msg_AuthoringSubmittedNotification
            Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=""" & cpcore.htmlDoc.html_EncodeHTML(Link) & """>" & cpcore.webServer.webServerIO_requestDomain & "</a>")
            Copy = genericController.vbReplace(Copy, "<RECORDNAME>", RecordName)
            Copy = genericController.vbReplace(Copy, "<CONTENTNAME>", ContentName)
            Copy = genericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDDATE>", cpcore.app_startTime.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDNAME>", cpcore.authContext.user.Name)
            '
            Call cpcore.email.sendGroup(cpcore.siteProperties.getText("WorkflowEditorGroup", "Content Editors"), FromAddress, "Authoring Submitted Notification", Copy, False, True)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18(MethodName)
            '
        End Sub
        '
        '
        '
        Friend Function pageManager_GetDefaultBlockMessage(UseContentWatchLink As Boolean) As String
            pageManager_GetDefaultBlockMessage = ""
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetDefaultBlockMessage")
            '
            Dim CS As Integer
            Dim PageID As Integer
            '
            CS = cpcore.db.cs_open("Copy Content", "name=" & cpcore.db.encodeSQLText(ContentBlockCopyName), "ID", , , , , "Copy,ID")
            If cpcore.db.cs_ok(CS) Then
                pageManager_GetDefaultBlockMessage = cpcore.db.cs_get(CS, "Copy")
            End If
            Call cpcore.db.cs_Close(CS)
            '
            ' ----- Do not allow blank message - if still nothing, create default
            '
            If pageManager_GetDefaultBlockMessage = "" Then
                pageManager_GetDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
            End If
            '
            ' ----- Create Copy Content Record for future
            '
            CS = cpcore.db.cs_insertRecord("Copy Content")
            If cpcore.db.cs_ok(CS) Then
                Call cpcore.db.cs_set(CS, "Name", ContentBlockCopyName)
                Call cpcore.db.cs_set(CS, "Copy", pageManager_GetDefaultBlockMessage)
                Call cpcore.db.cs_save2(CS)
                Call cpcore.workflow.publishEdit("Copy Content", genericController.EncodeInteger(cpcore.db.cs_get(CS, "ID")))
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
        Friend Function pageManager_LoadFormPageInstructions(FormInstructions As String, Formhtml As String) As main_FormPagetype
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
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_LoadFormPageInstructions")
        End Function
        '
        '
        '
        Friend Function pageManager_GetFormPage(FormPageName As String, GroupIDToJoinOnSuccess As Integer) As String
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
            Dim PeopleCDef As cdefModel
            '
            IsRetry = (cpcore.docProperties.getInteger("ContensiveFormPageID") <> 0)
            '
            CS = cpcore.db.cs_open("Form Pages", "name=" & cpcore.db.encodeSQLText(FormPageName))
            If cpcore.db.cs_ok(CS) Then
                FormPageID = cpcore.db.cs_getInteger(CS, "ID")
                Formhtml = cpcore.db.cs_getText(CS, "Body")
                FormInstructions = cpcore.db.cs_getText(CS, "Instructions")
            End If
            Call cpcore.db.cs_Close(CS)
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
                            If IsRetry And cpcore.docProperties.getText(.PeopleField) = "" Then
                                CaptionSpan = "<span class=""ccError"">"
                            Else
                                CaptionSpan = "<span>"
                            End If
                            If Not cpcore.db.cs_ok(CSPeople) Then
                                CSPeople = cpcore.db.csOpenRecord("people", cpcore.authContext.user.id)
                            End If
                            Caption = .Caption
                            If .REquired Or genericController.EncodeBoolean(cpcore.metaData.GetContentFieldProperty("People", .PeopleField, "Required")) Then
                                Caption = "*" & Caption
                            End If
                            If cpcore.db.cs_ok(CSPeople) Then
                                Body = f.RepeatCell
                                Body = genericController.vbReplace(Body, "{{CAPTION}}", CaptionSpan & Caption & "</span>", 1, 99, vbTextCompare)
                                Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.htmlDoc.html_GetFormInputCS(CSPeople, "People", .PeopleField), 1, 99, vbTextCompare)
                                RepeatBody = RepeatBody & Body
                                HasRequiredFields = HasRequiredFields Or .REquired
                            End If
                        Case 2
                            '
                            ' Group main_MemberShip
                            '
                            GroupValue = cpcore.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                            Body = f.RepeatCell
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.htmlDoc.html_GetFormInputCheckBox2("Group" & .GroupName, GroupValue), 1, 99, vbTextCompare)
                            Body = genericController.vbReplace(Body, "{{FIELD}}", .Caption)
                            RepeatBody = RepeatBody & Body
                            GroupRowPtr = GroupRowPtr + 1
                            HasRequiredFields = HasRequiredFields Or .REquired
                    End Select
                End With
            Next
            Call cpcore.db.cs_Close(CSPeople)
            If HasRequiredFields Then
                Body = f.RepeatCell
                Body = genericController.vbReplace(Body, "{{CAPTION}}", "&nbsp;", 1, 99, vbTextCompare)
                Body = genericController.vbReplace(Body, "{{FIELD}}", "*&nbsp;Required Fields")
                RepeatBody = RepeatBody & Body
            End If
            '
            pageManager_GetFormPage = "" _
            & errorController.error_GetUserError(cpcore) _
            & cpcore.htmlDoc.html_GetUploadFormStart() _
            & cpcore.htmlDoc.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) _
            & cpcore.htmlDoc.html_GetFormInputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.app_startTime)) _
            & f.PreRepeat _
            & RepeatBody _
            & f.PostRepeat _
            & cpcore.htmlDoc.html_GetUploadFormEnd()
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetFormPage")
        End Function
        '        '
        '        '=============================================================================
        '        '
        '        '=============================================================================
        '        '
        '        Public Function pageManager_GetSectionMenuNamed(Optional ByVal DepthLimit As Integer = 3, Optional ByVal MenuStyle As Integer = 1, Optional ByVal StyleSheetPrefix As String = "", Optional ByVal MenuName As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetSectionMenuNamed")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            'Dim DepthLimit As Integer
        '            'Dim MenuStyle As Integer
        '            Dim StyleSheetPrefixLocal As String
        '            Dim RedirectLink As String
        '            Dim DefaultTemplateLink As String
        '            Dim MenuNameLocal As String
        '            Dim MenuID As Integer
        '            '
        '            'DepthLimit = encodeEmptyInteger(DepthLimit, 3)
        '            'MenuStyle = encodeEmptyInteger(MenuStyle, 1)
        '            StyleSheetPrefixLocal = genericController.encodeEmptyText(StyleSheetPrefix, "ccFlyout")
        '            MenuNameLocal = genericController.encodeEmptyText(MenuName, "Default")
        '            If MenuNameLocal = "" Then
        '                MenuNameLocal = "Default"
        '            End If
        '            MenuID = cpcore.csv_VerifyDynamicMenu(MenuNameLocal)
        '            '
        '            DefaultTemplateLink = cpcore.siteProperties.getText("SectionLandingLink", requestAppRootPath & cpcore.siteProperties.serverPageDefault)
        '            pageManager_GetSectionMenuNamed = pageManager_GetSectionMenu(DepthLimit, MenuStyle, StyleSheetPrefixLocal, DefaultTemplateLink, MenuID, MenuNameLocal, cpcore.siteProperties.useContentWatchLink)
        '            pageManager_GetSectionMenuNamed = cpcore.htmlDoc.main_GetEditWrapper("Section Menu", pageManager_GetSectionMenuNamed)
        '            '
        '            If Me.redirectLink <> "" Then
        '                Call cpcore.webServer.redirect(Me.redirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
        '            End If
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            'Set PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetSectionMenuNamed")
        '        End Function
        '        '
        '        '=============================================================================
        '        ' 3.3 Compatibility
        '        '=============================================================================
        '        '
        '        Public Function main_GetSectionMenu(Optional ByVal DepthLimit As Integer = 3, Optional ByVal MenuStyle As Integer = 1, Optional ByVal StyleSheetPrefix As String = "") As String
        '            main_GetSectionMenu = pageManager_GetSectionMenuNamed(DepthLimit, MenuStyle, StyleSheetPrefix)
        '        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by the ContentName and RecordID
        '=============================================================================
        '
        Public Function getContentWatchLinkByName(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = True) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentWatchLinkByName")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentRecordKey As String
            '
            ContentRecordKey = cpcore.metaData.getContentId(genericController.encodeText(ContentName)) & "." & genericController.EncodeInteger(RecordID)
            getContentWatchLinkByName = getContentWatchLinkByKey(ContentRecordKey, DefaultLink, IncrementClicks)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetContentWatchLinkByName")
        End Function
        ''
        'Public Sub getPageArgs(ByVal PageID As Integer, ByVal isWorkflowRendering As Boolean, ByVal isQuickEditing As Boolean, ByRef page_contentcontrolid As Integer, ByRef return_TemplateID As Integer, ByRef page_ParentID As Integer, ByRef page_link As String, ByRef return_IsRootPage As Boolean, ByRef return_SectionID As Integer, ByRef page_IsSecure As Boolean, ByVal UsedIDList As String)
        '    Dim TCPtr As Integer
        '    Dim PCCPtr As Integer
        '    Dim PageFound As Boolean
        '    Dim IgnoreInteger As Integer
        '    Dim IgnoreString As String
        '    Dim IgnoreBoolean As Boolean
        '    Dim Ptr As Integer
        '    Dim SetTemplateID As Integer
        '    Dim page_templateID As Integer
        '    Dim SectionTemplateID As Integer
        '    '
        '    return_IsRootPage = pageToRootList.Count.Equals(1)
        '    '
        '    If Not return_IsRootPage Then
        '        '
        '        ' chase further
        '        '
        '        If Not genericController.IsInDelimitedString(UsedIDList, CStr(page_ParentID), ",") Then
        '            Call getPageArgs(page_ParentID, isWorkflowRendering, isQuickEditing, IgnoreInteger, return_TemplateID, IgnoreInteger, IgnoreString, IgnoreBoolean, return_SectionID, IgnoreBoolean, UsedIDList & "," & page_ParentID)
        '        End If
        '    End If
        '    If page_templateID <> 0 Then
        '        return_TemplateID = page_templateID
        '    ElseIf SectionTemplateID <> 0 Then
        '        return_TemplateID = SectionTemplateID
        '    End If
        '    '
        '    If return_TemplateID = 0 Then
        '        '
        '        ' no templateid still (parent and section are all blank), main_Get default templateid
        '        '
        '        If cache_pageTemplate_rows > 0 Then
        '            For TCPtr = 0 To cache_pageTemplate_rows - 1
        '                If genericController.vbLCase(genericController.encodeText(cache_pageTemplate(TC_Name, TCPtr))) = "default" Then
        '                    Exit For
        '                End If
        '            Next
        '            If TCPtr < cache_pageTemplate_rows Then
        '                return_TemplateID = genericController.EncodeInteger(cache_pageTemplate(TC_ID, TCPtr))
        '            End If
        '        End If
        '        'End If
        '    End If
        'End Sub
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
        Public Function getPageLink4(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal AllowLinkAliasIfEnabled As Boolean, ByVal UseContentWatchNotDefaultPage As Boolean) As String
            Dim result As String = ""
            Dim setdomainId As Integer
            Dim linkLong As String
            Dim linkprotocol As String
            Dim linkPathPage As String
            Dim linkAlias As String = ""
            Dim linkQS As String
            Dim linkDomain As String
            Dim defaultPathPage As String
            Dim PageIsSecure As Boolean
            Dim Pos As Integer
            Dim templateLinkIncludesProtocol As Boolean
            Dim templateDomain As String = ""
            '
            'Call getPageArgs(PageID, pagemanager_IsWorkflowRendering, cpcore.authContext.isQuickEditing(cpcore, ""), ContentControlID, templateId, ParentID, MenuLinkOverRide, IsRootPage, SectionID, PageIsSecure, "")
            '
            defaultPathPage = cpcore.siteProperties.serverPageDefault
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
            templateLinkIncludesProtocol = (InStr(1, template.Link, "://") <> 0)
            '
            ' calc linkQS (cleared in come cases later)
            '
            linkQS = rnPageId & "=" & PageID
            If QueryStringSuffix <> "" Then
                linkQS = linkQS & "&" & QueryStringSuffix
            End If
            '
            ' calculate depends on the template provided
            '
            If template.Link = "" Then
                '
                ' ----- templateLink is blank
                '
                If AllowLinkAliasIfEnabled And cpcore.siteProperties.allowLinkAlias Then
                    Dim linkAliasList As List(Of Models.Entity.linkAliasModel) = Models.Entity.linkAliasModel.createList(cpcore, "(PageID=" & PageID & ")and(QueryStringSuffix=" & cpcore.db.encodeSQLText(QueryStringSuffix) & ")", "id desc")
                    If linkAliasList.Count > 0 Then
                        linkAlias = linkAliasList.First.Link
                        If Mid(linkAlias, 1, 1) <> "/" Then
                            linkAlias = "/" & linkAlias
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
                Dim SqlCriteria As String = "(domainId=" & domain.ID & ")"
                Dim allowTemplateRuleList As List(Of Models.Entity.TemplateDomainRuleModel) = Models.Entity.TemplateDomainRuleModel.createList(cpcore, SqlCriteria)
                Dim templateAllowed As Boolean = False
                For Each rule As TemplateDomainRuleModel In allowTemplateRuleList
                    If (rule.templateId = template.ID) Then
                        templateAllowed = True
                        Exit For
                    End If
                Next
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
                    setdomainId = allowTemplateRuleList.First.domainId
                    linkDomain = cpcore.db.getRecordName("domains", setdomainId)
                    If linkDomain = "" Then
                        linkDomain = cpcore.webServer.requestDomain
                    End If
                End If
                '
                ' protocol
                '
                If PageIsSecure Or template.IsSecure Then
                    linkprotocol = "https://"
                Else
                    linkprotocol = "http://"
                End If
                linkLong = linkprotocol & linkDomain & linkPathPage
            ElseIf Not templateLinkIncludesProtocol Then
                '
                ' ----- Short TemplateLink
                '
                linkPathPage = template.Link
                '
                ' domain (fake for now)
                '
                If templateDomain <> "" Then
                    linkDomain = cpcore.webServer.requestDomain
                Else
                    linkDomain = cpcore.webServer.requestDomain
                End If
                '
                ' protocol
                '
                If PageIsSecure Or template.IsSecure Then
                    linkprotocol = "https://"
                Else
                    linkprotocol = "http://"
                End If
                linkLong = linkprotocol & linkDomain & linkPathPage
            Else
                '
                ' ----- Long TemplateLink
                '
                linkLong = template.Link
            End If
            '
            ' assemble
            '
            result = linkLong
            If linkQS <> "" Then
                result = result & "?" & linkQS
            End If
            Return result
        End Function
        '
        '====================================================================================================
        ' main_Get a page link if you know nothing about the page
        '   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        '====================================================================================================
        '
        Public Function main_GetPageLink3(ByVal PageID As Integer, ByVal QueryStringSuffix As String, ByVal AllowLinkAlias As Boolean) As String
            main_GetPageLink3 = getPageLink4(PageID, QueryStringSuffix, AllowLinkAlias, False)
        End Function
        '
        Public Function getPageLink2(ByVal PageID As Integer, ByVal QueryStringSuffix As String) As String
            getPageLink2 = getPageLink4(PageID, QueryStringSuffix, True, False)
            'main_GetPageLink2 = main_GetPageLink3(PageID, QueryStringSuffix, True)
        End Function
        '
        Public Function main_GetPageLink(ByVal PageID As Integer) As String
            main_GetPageLink = getPageLink4(PageID, "", True, False)
            'main_GetPageLink = main_GetPageLink3(PageID, "", True)
        End Function
        '
        '
        '
        Friend Function pageManager_GetSectionLink(ByVal ShortLink As String, ByVal PageID As Integer, ByVal SectionID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00378")
            '
            Dim QSplit() As String
            Dim QSPlitCount As Integer
            Dim QSplitPointer As Integer
            Dim NVSplit() As String
            '
            pageManager_GetSectionLink = ShortLink
            If cpcore.htmlDoc.refreshQueryString <> "" Then
                QSplit = Split(cpcore.htmlDoc.refreshQueryString, "&")
                QSPlitCount = UBound(QSplit) + 1
                For QSplitPointer = 0 To QSPlitCount - 1
                    NVSplit = Split(QSplit(QSplitPointer), "=")
                    If UBound(NVSplit) > 0 Then
                        pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, NVSplit(0), NVSplit(1), True)
                    End If
                Next
            End If
            If PageID = 0 Then
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, rnPageId, "", False)
                If SectionID = 0 Then
                    pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", "", False)
                Else
                    pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", CStr(SectionID), True)
                End If
            Else
                '
                ' If I have a pageID, block the sectionID
                '
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, rnPageId, CStr(PageID), True)
                pageManager_GetSectionLink = genericController.modifyLinkQuery(pageManager_GetSectionLink, "sid", "", False)
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("pageManager_GetSectionLink")
        End Function
        '        '
        '        '===================================================================================================
        '        '   Load Template from TemplateID
        '        '       Template is loaded by ID
        '        '       If it is not found, default template is loaded
        '        '       If default template is not found, it is created
        '        '       Loaded TemplateID is returned - so you know if it loaded correctly
        '        '
        '        '       If Link is provided with protocol, it is returned with protocol
        '        '       If link is just a page, it is converted to a short link
        '        '       If link is blank, blank is returned and a redirect is not required
        '        '===================================================================================================
        '        '
        '        Public Function pageManager_LoadTemplateGetID(ByVal templateId As Integer) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00379")
        '            '
        '            Dim CS As Integer
        '            Dim FieldList As String
        '            Const ContentName = "Page Templates"
        '            '
        '            If (templateId <> 0) And (currentTemplateID = templateId) Then
        '                '
        '                ' Use the previous values already loaded
        '                '
        '            Else
        '                currentTemplateID = templateId
        '                templateBody = ""
        '                templateLink = ""
        '                If True Then
        '                    FieldList = "ID,Link,BodyHTML"
        '                Else
        '                    FieldList = "ID,Link"
        '                End If
        '                CS = -1
        '                If templateId <> 0 Then
        '                    CS = cpcore.db.cs_open2(ContentName, templateId, , , FieldList)
        '                End If
        '                If (templateId = 0) Or (Not cpcore.db.cs_ok(CS)) Then
        '                    '
        '                    ' ----- if template not found, return default template
        '                    '       if this operation fails, exit now -- do not continue and create new template
        '                    '
        '                    currentTemplateID = 0
        '                    If cpcore.domainLegacyCache.domainDetails.defaultTemplateId <> 0 Then
        '                        '
        '                        ' ----- attempt to use the domain's default template
        '                        '
        '                        Call cpcore.db.cs_Close(CS)
        '                        CS = cpcore.db.cs_open2(ContentName, cpcore.domainLegacyCache.domainDetails.defaultTemplateId, , , FieldList)
        '                        If Not cpcore.db.cs_ok(CS) Then
        '                            '
        '                            ' the defaultemplateid in the domain is not valid
        '                            '
        '                            Call cpcore.db.executeSql("update ccdomains set defaulttemplateid=0 where defaulttemplateid=" & cpcore.domainLegacyCache.domainDetails.defaultTemplateId)
        '                            Call cpcore.cache.invalidateContent("domains")
        '                        End If
        '                    End If
        '                    If Not cpcore.db.cs_ok(CS) Then
        '                        '
        '                        ' ----- attempt to use the site's default template
        '                        '
        '                        Call cpcore.db.cs_Close(CS)
        '                        CS = cpcore.db.cs_open(ContentName, "name=" & cpcore.db.encodeSQLText(TemplateDefaultName), "ID", , , , , FieldList)
        '                    End If
        '                    If cpcore.db.cs_ok(CS) Then
        '                        currentTemplateID = cpcore.db.cs_getInteger(CS, "ID")
        '                        currentTemplateName = cpcore.db.cs_getText(CS, "name")
        '                        templateName = currentTemplateName
        '                        templateLink = main_verifyTemplateLink(cpcore.db.cs_get(CS, "Link"))
        '                        'pageManager_TemplateLink = app.csv_cs_get(CS, "Link")
        '                        If True Then
        '                            templateBody = cpcore.db.cs_get(CS, "BodyHTML")
        '                        End If
        '                    End If
        '                    Call cpcore.db.cs_Close(CS)
        '                    '
        '                    ' ----- if default template not found, create a simple default template
        '                    '
        '                    If currentTemplateID = 0 Then
        '                        templateName = TemplateDefaultName
        '                        templateBody = TemplateDefaultBody
        '                        CS = cpcore.db.cs_insertRecord("Page Templates")
        '                        If cpcore.db.cs_ok(CS) Then
        '                            currentTemplateID = cpcore.db.cs_getInteger(CS, "ID")
        '                            currentTemplateName = TemplateDefaultName
        '                            Call cpcore.db.cs_set(CS, "name", TemplateDefaultName)
        '                            Call cpcore.db.cs_set(CS, "Link", "")
        '                            If True Then
        '                                Call cpcore.db.cs_set(CS, "BodyHTML", templateBody)
        '                            End If
        '                            If True Then
        '                                Call cpcore.db.cs_set(CS, "ccGuid", DefaultTemplateGuid)
        '                            End If
        '                            Call cpcore.db.cs_Close(CS)
        '                        End If
        '                        Call pageManager_cache_pageTemplate_clear()
        '                    End If
        '                    templateLink = ""
        '                Else
        '                    '
        '                    ' ----- load template
        '                    '
        '                    If True Then
        '                        templateBody = cpcore.db.cs_get(CS, "BodyHTML")
        '                    Else
        '                        templateBody = "<!-- Template Body support requires a Contensive database upgrade through the Application Manager. -->" & TemplateDefaultBody
        '                    End If
        '                    templateName = cpcore.db.cs_get(CS, "name")
        '                    templateLink = main_verifyTemplateLink(cpcore.db.cs_get(CS, "Link"))
        '                End If
        '                Call cpcore.db.cs_Close(CS)
        '                templateLink = main_verifyTemplateLink(templateLink)
        '            End If
        '            '
        '            pageManager_LoadTemplateGetID = currentTemplateID
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("pageManager_LoadTemplateGetID")
        '        End Function
        ''
        ''
        ''
        'Public Function pageManager_GetDynamicMenu(addonOption_String As String, UseContentWatchLink As Boolean) As String
        '    Dim result As String = ""
        '    Try
        '        Dim EditLink As String
        '        Dim StylesFilename As String
        '        Dim MenuDepth As Integer
        '        Dim MenuStyle As Integer
        '        Dim MenuName As String
        '        Dim MenuStylePrefix As String
        '        Dim MenuDelimiter As String
        '        Dim DefaultTemplateLink As String
        '        Dim FlyoutDirection As String
        '        Dim FlyoutOnHover As String
        '        Dim Layout As String
        '        Dim PreButton As String
        '        Dim PostButton As String
        '        Dim MenuID As Integer
        '        Dim IsAuthoring As Boolean
        '        Dim Menu As String
        '        Dim MenuNew As String
        '        Dim CS As Integer
        '        Dim IsOldMenu As Boolean
        '        Dim CompatibilitySpanAroundButton As Boolean
        '        '
        '        IsAuthoring = cpcore.authContext.isEditing(cpcore, "Dynamic Menus")
        '        DefaultTemplateLink = requestAppRootPath & cpcore.webServer.webServerIO_requestPage
        '        If False Then '.292" Then
        '            CompatibilitySpanAroundButton = True
        '        Else
        '            CompatibilitySpanAroundButton = cpcore.siteProperties.getBoolean("Compatibility Dynamic Menu Span Around Button", False)
        '        End If
        '        '
        '        ' Check for MenuID - if present, arguments are in the Dynamic Menu content - else it is old, and they are in the addonOption_String
        '        '
        '        If True And genericController.vbInstr(1, addonOption_String, "menu=", vbTextCompare) <> 0 Then
        '            MenuNew = cpcore.getAddonOption("menunew", addonOption_String)
        '            'MenuNew = Trim( genericController.DecodeResponseVariable(main_GetArgument("menunew", addonOption_String, "", "&")))
        '            If MenuNew <> "" Then
        '                '
        '                ' Create New Menu
        '                '
        '                Menu = MenuNew
        '            End If
        '            If Menu = "" Then
        '                '
        '                ' No new menu, try a selected menu
        '                '
        '                Menu = cpcore.getAddonOption("menu", addonOption_String)
        '                'Menu = Trim( genericController.DecodeResponseVariable(main_GetArgument("menu", addonOption_String, "", "&")))
        '                If Menu = "" Then
        '                    '
        '                    ' No selected, use Default
        '                    '
        '                    Menu = "Default"
        '                End If
        '            End If
        '            MenuID = cpcore.menu_VerifyDynamicMenu(Menu)
        '            '
        '            ' Open the Menu
        '            '
        '            CS = cpcore.csOpen("Dynamic Menus", MenuID)
        '            If Not cpcore.db.cs_ok(CS) Then
        '                '
        '                ' ID was given, but no found in Db
        '                '
        '                Call cpcore.db.cs_Close(CS)
        '                CS = cpcore.csOpen("Dynamic Menus", cpcore.menu_VerifyDynamicMenu("Default"))
        '            End If
        '            If cpcore.db.cs_ok(CS) Then
        '                '
        '                ' setup arguments from Content
        '                '
        '                EditLink = cpcore.cs_cs_getRecordEditLink(CS)
        '                MenuName = cpcore.db.cs_getText(CS, "Name")
        '                MenuDepth = cpcore.db.cs_getInteger(CS, "Depth")
        '                MenuStylePrefix = cpcore.db.cs_getText(CS, "StylePrefix")
        '                MenuDelimiter = cpcore.db.cs_getText(CS, "Delimiter")
        '                FlyoutOnHover = cpcore.db.cs_getBoolean(CS, "FlyoutOnHover").ToString
        '                ' LookupList should return the text for the value saved - to be compatible with the old hardcoded text
        '                FlyoutDirection = cpcore.db.cs_get(CS, "FlyoutDirection")
        '                Layout = cpcore.db.cs_get(CS, "Layout")
        '                MenuStyle = 0
        '                '
        '                ' Add exclusive styles
        '                '
        '                If True Then
        '                    StylesFilename = cpcore.db.cs_getText(CS, "StylesFilename")
        '                    If StylesFilename <> "" Then
        '                        If genericController.vbLCase(Right(StylesFilename, 4)) <> ".css" Then
        '                            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError15("Dynamic Menu [" & MenuName & "] StylesFilename is not a '.css' file, and will not display correct. Check that the field is setup as a CSSFile.", "main_GetDynamicMenu")
        '                        Else
        '                            Call cpcore.htmlDoc.main_AddStylesheetLink2(cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.requestDomain & genericcontroller.getCdnFileLink(cpcore, StylesFilename), "dynamic menu")
        '                        End If
        '                    End If
        '                End If
        '            End If
        '            Call cpcore.db.cs_Close(CS)
        '        Else
        '            '
        '            ' Old style menu - main_Get arguments from AC tag
        '            '   MenuName="" is legacy mode (all sections show)
        '            '
        '            IsOldMenu = True
        '            MenuName = ""
        '            '
        '            MenuDepth = genericController.EncodeInteger(cpcore.getAddonOption("DEPTH", addonOption_String))
        '            MenuStylePrefix = Trim(cpcore.getAddonOption("STYLEPREFIX", addonOption_String))
        '            MenuDelimiter = cpcore.getAddonOption("DELIMITER", addonOption_String)
        '            FlyoutOnHover = cpcore.getAddonOption("FlyoutOnHover", addonOption_String)
        '            FlyoutDirection = cpcore.getAddonOption("FlyoutDirection", addonOption_String)
        '            Layout = cpcore.getAddonOption("Layout", addonOption_String)
        '            '
        '            ' really old value
        '            '
        '            MenuStyle = genericController.EncodeInteger(cpcore.getAddonOption("FORMAT", addonOption_String))
        '        End If
        '        '
        '        ' Check values
        '        '
        '        If MenuStylePrefix = "" Then
        '            MenuStylePrefix = "ccFlyout"
        '        End If
        '        '
        '        ' determine MenuStyle from input
        '        '
        '        If MenuStyle = 0 Then
        '            If genericController.EncodeBoolean(FlyoutOnHover) Then
        '                MenuStyle = 8
        '            Else
        '                MenuStyle = 4
        '            End If
        '            Select Case genericController.vbUCase(FlyoutDirection)
        '                Case "RIGHT"
        '                    MenuStyle = MenuStyle + 1
        '                Case "UP"
        '                    MenuStyle = MenuStyle + 2
        '                Case "LEFT"
        '                    MenuStyle = MenuStyle + 3
        '            End Select
        '        End If
        '        result = pageManager_GetSectionMenu(MenuDepth, MenuStyle, MenuStylePrefix, DefaultTemplateLink, MenuID, MenuName, UseContentWatchLink)
        '        '
        '        ' Now adjust results using arguments
        '        '
        '        If genericController.vbUCase(Layout) = "VERTICAL" Then
        '            '
        '            ' vertical menu: Set dislay block
        '            '
        '            result = genericController.vbReplace(result, "class=""" & MenuStylePrefix & "Button""", "style=""display:block;"" class=""" & MenuStylePrefix & "Button""")
        '            '
        '            PreButton = "<div style=""WHITE-SPACE: nowrap;"">"
        '            PostButton = "</div>"
        '            '
        '            If MenuDelimiter <> "" Then
        '                MenuDelimiter = "<div style=""WHITE-SPACE: nowrap;"" class=""" & MenuStylePrefix & "Delimiter"">" & MenuDelimiter & "</div>"
        '            End If
        '        Else
        '            '
        '            ' horizontal menu: Set dislay inline
        '            '
        '            result = genericController.vbReplace(result, "class=""" & MenuStylePrefix & "Button""", "style=""display:inline;"" class=""" & MenuStylePrefix & "Button""")
        '            '
        '            If CompatibilitySpanAroundButton Then
        '                PreButton = "<span style=""WHITE-SPACE: nowrap"">"
        '                PostButton = "</span>"
        '            End If
        '            '
        '            If MenuDelimiter <> "" Then
        '                MenuDelimiter = "<span style=""WHITE-SPACE: nowrap;"" class=""" & MenuStylePrefix & "Delimiter"">" & MenuDelimiter & "</span>"
        '            End If
        '        End If
        '        result = PreButton & genericController.vbReplace(result, vbCrLf, PostButton & MenuDelimiter & PreButton) & PostButton
        '        If cpcore.authContext.isAdvancedEditing(cpcore, "") Then
        '            result = "<div style=""border-bottom:1px dashed #404040; padding:5px;margin-bottom:5px;"">Dynamic Menu [" & MenuName & "]" & EditLink & "</div><div>" & result & "</div>"
        '        End If

        '    Catch ex As Exception
        '        cpcore.handleExceptionAndContinue(ex)
        '    End Try
        '    Return result
        'End Function
        '
        '
        '
        Friend Function pageManager_GetLandingLink() As String
            If landingLink = "" Then
                landingLink = cpcore.siteProperties.getText("SectionLandingLink", requestAppRootPath & cpcore.siteProperties.serverPageDefault)
                landingLink = genericController.ConvertLinkToShortLink(landingLink, cpcore.webServer.requestDomain, cpcore.webServer.webServerIO_requestVirtualFilePath)
                landingLink = genericController.EncodeAppRootPath(landingLink, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            End If
            pageManager_GetLandingLink = landingLink
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
            pageManager_GetStyleSheetDefault = cpcore.htmlDoc.pageManager_GetStyleSheetDefault2()
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
            If cpcore.siteProperties.getBoolean("Allow CSS Reset") Then
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.webServerIO_requestDomain & "/ccLib/styles/ccreset.css"" >"
            End If
            StyleSN = genericController.EncodeInteger(cpcore.siteProperties.getText("StylesheetSerialNumber", "0"))
            If StyleSN < 0 Then
                '
                ' Linked Styles
                ' Bump the Style Serial Number so next fetch is not cached
                '
                StyleSN = 1
                Call cpcore.siteProperties.setProperty("StylesheetSerialNumber", CStr(StyleSN))
                '
                ' Save new public stylesheet
                '
                'Dim kmafs As New fileSystemClass
                Call cpcore.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), cpcore.htmlDoc.html_getStyleSheet2(0, 0))
                Call cpcore.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css"), cpcore.htmlDoc.pageManager_GetStyleSheetDefault2)

            End If
            If (StyleSN = 0) Then
                '
                ' Put styles inline if requested, and if there has been an upgrade
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & StyleSheetStart & pageManager_GetStyleSheet() & cr & StyleSheetEnd
            ElseIf (cpcore.siteProperties.dataBuildVersion <> cpcore.codeVersion()) Then
                '
                ' Put styles inline if requested, and if there has been an upgrade
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<!-- styles forced inline because database upgrade needed -->" & StyleSheetStart & pageManager_GetStyleSheet() & cr & StyleSheetEnd
            Else
                '
                ' cached stylesheet
                '
                pageManager_GetStyleTagPublic = pageManager_GetStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.webServerIO_requestDomain & genericController.getCdnFileLink(cpcore, "templates/Public" & StyleSN & ".css") & """ >"
            End If
        End Function
        '
        '=======================================================================================================
        '   deprecated, use csv_getStyleSheet2
        '=======================================================================================================
        '
        Public Function pageManager_GetStyleSheet2() As String
            pageManager_GetStyleSheet2 = cpcore.htmlDoc.html_getStyleSheet2(0, 0)
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
                If cpcore.authContext.isAuthenticatedContentManager(cpcore) Then
                    pagemanager_IsWorkflowRendering = cpcore.visitProperty.getBoolean("AllowWorkflowRendering")
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return returnIs
        End Function
        Public Function main_GetPageNotFoundPageId() As Integer
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
                cpcore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return pageId
        End Function
        '
        '
        '
        Friend Function main_guessDefaultPage() As String
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
            Dim CDef As cdefModel
            '
            main_IsChildRecord = (ChildRecordID = ParentRecordID)
            If Not main_IsChildRecord Then
                CDef = cpcore.metaData.getCdef(ContentName)
                If genericController.IsInDelimitedString(UCase(CDef.SelectCommaList), "PARENTID", ",") Then
                    main_IsChildRecord = main_IsChildRecord_Recurse(CDef.ContentDataSourceName, CDef.ContentTableName, ChildRecordID, ParentRecordID, "")
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
        Friend Function main_IsChildRecord_Recurse(ByVal DataSourceName As String, ByVal TableName As String, ByVal ChildRecordID As Integer, ByVal ParentRecordID As Integer, ByVal History As String) As Boolean
            '
            Dim SQL As String
            Dim CS As Integer
            Dim ChildRecordParentID As Integer
            '
            SQL = "select ParentID from " & TableName & " where id=" & ChildRecordID
            CS = cpcore.db.cs_openSql(SQL)
            If cpcore.db.cs_ok(CS) Then
                ChildRecordParentID = cpcore.db.cs_getInteger(CS, "ParentID")
            End If
            Call cpcore.db.cs_Close(CS)
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
        Friend Function main_ProcessPageNotFound_GetLink(ByVal adminMessage As String, Optional ByVal BackupPageNotFoundLink As String = "", Optional ByVal PageNotFoundLink As String = "", Optional ByVal EditPageID As Integer = 0, Optional ByVal EditSectionID As Integer = 0) As String
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
                ' Set link
                '
                Link = getPageLink4(PageNotFoundPageID, "", True, False)
                DefaultLink = getPageLink4(0, "", True, False)
                If Link <> DefaultLink Then
                Else
                    adminMessage = adminMessage & "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used."
                End If
            End If
            '
            ' Add the Admin Message to the link
            '
            If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
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
                If cpcore.webServer.webServerIO_requestReferer <> "" Then
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
            main_ProcessPageNotFound_GetLink = Link
            '
        End Function
        '
        '---------------------------------------------------------------------------
        '
        '---------------------------------------------------------------------------
        '
        Public Function getLandingPageID() As Integer
            Dim landingPageid As Integer = 0
            Try
                landingPageid = domain.RootPageID
                If landingPageid = 0 Then
                    '
                    ' -- try the site property landing page id
                    landingPageid = cpcore.siteProperties.getinteger("LandingPageID", 0)
                    If landingPageid = 0 Then
                        '
                        ' -- landing page could not be determined
                        Dim landingPage As pageContentModel = pageContentModel.add(cpcore)
                        landingPage.save(cpcore)
                        landingPageid = landingPage.id
                        '
                        Me.landingPageID = main_GetPageNotFoundPageId()
                    End If
                    '
                    ' -- save new page to the domain
                    domain.RootPageID = landingPageid
                    domain.save(cpcore)
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return landingPageid
        End Function
        '        '
        '        '---------------------------------------------------------------------------
        '        '
        '        '---------------------------------------------------------------------------
        '        '
        '        Public Function main_GetLandingPageName(LandingPageID As Integer) As String
        '            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLandingPageName")
        '            '
        '            Dim PCCPtr As Integer
        '            '
        '            If landingPageName = "" Then
        '                PCCPtr = cache_pageContent_getPtr(LandingPageID, pagemanager_IsWorkflowRendering, main_RenderCache_CurrentPage_IsQuickEditing)
        '                If PCCPtr < 0 Then
        '                    '
        '                    ' This case should have been covered in main_GetLandingPageID -- and should not be possible
        '                    '
        '                    landingPageName = DefaultNewLandingPageName
        '                Else
        '                    landingPageName = cache_pageContent(PCC_Name, PCCPtr)
        '                End If
        '            End If
        '            main_GetLandingPageName = landingPageName
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetLandingPageName")
        '        End Function
        '
        ' Verify a link from the template link field to be used as a Template Link
        '
        Friend Function main_verifyTemplateLink(ByVal linkSrc As String) As String
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
                    main_verifyTemplateLink = genericController.EncodeAppRootPath(main_verifyTemplateLink, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
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
                    main_verifyTemplateLink = genericController.ConvertLinkToShortLink(main_verifyTemplateLink, cpcore.webServer.requestDomain, cpcore.webServer.webServerIO_requestVirtualFilePath)
                    main_verifyTemplateLink = genericController.EncodeAppRootPath(main_verifyTemplateLink, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError11("main_verifyTemplateLink", "trap")
        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by its ContentRecordKey
        '=============================================================================
        '
        Public Function getContentWatchLinkByKey(ByVal ContentRecordKey As String, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentWatchLinkByKey")
            '
            'If Not (true) Then Exit Function
            '
            Dim CSPointer As Integer
            '
            ' Lookup link in main_ContentWatch
            '
            CSPointer = cpcore.db.cs_open("Content Watch", "ContentRecordKey=" & cpcore.db.encodeSQLText(ContentRecordKey), , , , , , "Link,Clicks")
            If cpcore.db.cs_ok(CSPointer) Then
                getContentWatchLinkByKey = cpcore.db.cs_getText(CSPointer, "Link")
                If genericController.EncodeBoolean(IncrementClicks) Then
                    Call cpcore.db.cs_set(CSPointer, "Clicks", cpcore.db.cs_getInteger(CSPointer, "clicks") + 1)
                End If
            Else
                getContentWatchLinkByKey = genericController.encodeText(DefaultLink)
            End If
            Call cpcore.db.cs_Close(CSPointer)
            '
            getContentWatchLinkByKey = genericController.EncodeAppRootPath(getContentWatchLinkByKey, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetContentWatchLinkByKey")
        End Function
        '
        '====================================================================================================
        ' Replace with main_GetPageArgs()
        '
        ' Used Interally by main_GetPageLink to main_Get the SectionID of the parents
        ' Dim siteSectionRootPageIndex As Dictionary(Of Integer, Integer) = siteSectionModel.getRootPageIdIndex(Me)
        '====================================================================================================
        '
        Friend Function getPageSectionId(ByVal PageID As Integer, ByRef UsedIDList As List(Of Integer), siteSectionRootPageIndex As Dictionary(Of Integer, Integer)) As Integer
            Dim sectionId As Integer = 0
            Try
                Dim page As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpcore, PageID, New List(Of String))
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
                cpcore.handleExceptionAndContinue(ex) : Throw
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
        Friend Function main_GetPageDynamicLink_GetTemplateID(ByVal PageID As Integer, ByVal UsedIDList As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageDynamicLink_GetTemplateID")
            '
            Dim CS As Integer
            Dim ParentID As Integer
            Dim templateId As Integer
            '
            '
            CS = cpcore.db.csOpenRecord("Page Content", PageID, , , "TemplateID,ParentID")
            If cpcore.db.cs_ok(CS) Then
                templateId = cpcore.db.cs_getInteger(CS, "TemplateID")
                ParentID = cpcore.db.cs_getInteger(CS, "ParentID")
            End If
            Call cpcore.db.cs_Close(CS)
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
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("main_GetPageDynamicLink_GetTemplateID")
        End Function
        '
        '====================================================================================================
        ' main_Get a page link if you know nothing about the page
        '   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
        '====================================================================================================
        '
        Public Function main_GetPageDynamicLink(ByVal PageID As Integer, ByVal UseContentWatchLink As Boolean) As String
            '
            Dim CCID As Integer
            Dim DefaultLink As String
            Dim SectionID As Integer
            Dim IsRootPage As Boolean
            Dim templateId As Integer
            Dim MenuLinkOverRide As String
            '
            'Call getPageArgs(PageID, pagemanager_IsWorkflowRendering, cpcore.authContext.isQuickEditing(cpcore, ""), CCID, templateId, ParentID, MenuLinkOverRide, IsRootPage, SectionID, PageIsSecure, "")
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
            DefaultLink = cpcore.siteProperties.serverPageDefault
            If Mid(DefaultLink, 1, 1) <> "/" Then
                DefaultLink = "/" & cpcore.siteProperties.serverPageDefault
            End If
            '
            main_GetPageDynamicLink = main_GetPageDynamicLinkWithArgs(CCID, PageID, DefaultLink, IsRootPage, templateId, SectionID, MenuLinkOverRide, UseContentWatchLink)
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
        Friend Function main_GetPageDynamicLinkWithArgs(ByVal ContentControlID As Integer, ByVal PageID As Integer, ByVal DefaultLink As String, ByVal IsRootPage As Boolean, ByVal templateId As Integer, ByVal SectionID As Integer, ByVal MenuLinkOverRide As String, ByVal UseContentWatchLink As Boolean) As String
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
                        resultLink = getContentWatchLinkByID(ContentControlID, PageID, DefaultLink, False)
                    Else
                        '
                        ' -- Current method - all pages are in the Template, Section, Page structure
                        If templateId <> 0 Then
                            Dim template As pageTemplateModel = pageTemplateModel.create(cpcore, templateId, New List(Of String))
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
                                resultLink = getContentWatchLinkByID(ContentControlID, PageID, , False)
                            End If
                        End If
                        If (PageID = 0) Or (IsRootPage) Then
                            '
                            ' -- Link to Root Page, no bid, and include sectionid if not 0
                            If IsRootPage And (SectionID <> 0) Then
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", CStr(SectionID), True)
                            End If
                            resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, "", False)
                        Else
                            resultLink = genericController.modifyLinkQuery(resultLink, rnPageId, genericController.encodeText(PageID), True)
                            If PageID <> 0 Then
                                resultLink = genericController.modifyLinkQuery(resultLink, "sid", "", False)
                            End If
                        End If
                    End If
                End If
                resultLink = genericController.EncodeAppRootPath(resultLink, cpcore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex) : Throw
            End Try
            Return resultLink
        End Function
        '
        '=============================================================================
        '   main_Get the link for a Content Record by the ContentID and RecordID
        '=============================================================================
        '
        Public Function getContentWatchLinkByID(ByVal ContentID As Integer, ByVal RecordID As Integer, Optional ByVal DefaultLink As String = "", Optional ByVal IncrementClicks As Boolean = True) As String
            getContentWatchLinkByID = getContentWatchLinkByKey(genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), DefaultLink, IncrementClicks)
        End Function
        '
        '=============================================================================
        '
        Public Sub verifyRegistrationFormPage(cpcore As coreClass)
            Try
                '
                Dim CS As Integer
                Dim GroupNameList As String
                Dim Copy As String
                '
                Call cpcore.db.deleteContentRecords("Form Pages", "name=" & cpcore.db.encodeSQLText("Registration Form"))
                CS = cpcore.db.cs_open("Form Pages", "name=" & cpcore.db.encodeSQLText("Registration Form"))
                If Not cpcore.db.cs_ok(CS) Then
                    '
                    ' create Version 1 template - just to main_Get it started
                    '
                    Call cpcore.db.cs_Close(CS)
                    GroupNameList = "Registered"
                    CS = cpcore.db.cs_insertRecord("Form Pages")
                    If cpcore.db.cs_ok(CS) Then
                        Call cpcore.db.cs_set(CS, "name", "Registration Form")
                        Copy = "" _
                        & vbCrLf & "<table border=""0"" cellpadding=""2"" cellspacing=""0"" width=""100%"">" _
                        & vbCrLf & "{{REPEATSTART}}<tr><td align=right style=""height:22px;"">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}" _
                        & vbCrLf & "<tr><td align=right><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=135 height=1></td><td width=""100%"">&nbsp;</td></tr>" _
                        & vbCrLf & "<tr><td colspan=2>&nbsp;<br>" & cpcore.htmlDoc.main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>" _
                        & vbCrLf & "</table>"
                        Call cpcore.db.cs_set(CS, "Body", Copy)
                        Copy = "" _
                        & "1" _
                        & vbCrLf & GroupNameList _
                        & vbCrLf & "true" _
                        & vbCrLf & "1,First Name,true,FirstName" _
                        & vbCrLf & "1,Last Name,true,LastName" _
                        & vbCrLf & "1,Email Address,true,Email" _
                        & vbCrLf & "1,Phone,true,Phone" _
                        & vbCrLf & "2,Please keep me informed of news and events,false,Subscribers" _
                        & ""
                        Call cpcore.db.cs_set(CS, "Instructions", Copy)
                    End If
                End If
                Call cpcore.db.cs_Close(CS)
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
        End Sub
        '
        '---------------------------------------------------------------------------
        '   Create the default landing page if it is missing
        '---------------------------------------------------------------------------
        '
        Public Function createPageGetID(ByVal PageName As String, ByVal ContentName As String, ByVal CreatedBy As Integer, ByVal pageGuid As String) As Integer
            Dim Id As Integer = 0
            '
            Dim CS As Integer = cpcore.db.cs_insertRecord(ContentName, CreatedBy)
            If cpcore.db.cs_ok(CS) Then
                Id = cpcore.db.cs_getInteger(CS, "ID")
                Call cpcore.db.cs_set(CS, "name", PageName)
                Call cpcore.db.cs_set(CS, "active", "1")
                If True Then
                    Call cpcore.db.cs_set(CS, "ccGuid", pageGuid)
                End If
                Call cpcore.db.cs_save2(CS)
                Call cpcore.workflow.publishEdit("Page Content", Id)
            End If
            Call cpcore.db.cs_Close(CS)
            '
            createPageGetID = Id
        End Function
        '
        '====================================================================================================
        '   Returns the Alias link (SourceLink) from the actual link (DestinationLink)
        '
        '====================================================================================================
        '
        Public Shared Function getLinkAlias(cpcore As coreClass, PageID As Integer, QueryStringSuffix As String, DefaultLink As String) As String
            Dim linkAlias As String = DefaultLink
            Dim linkAliasList As List(Of Models.Entity.linkAliasModel) = Models.Entity.linkAliasModel.createList(cpcore, "(PageID=" & PageID & ")and(QueryStringSuffix=" & cpcore.db.encodeSQLText(QueryStringSuffix) & ")", "id desc")
            If linkAliasList.Count > 0 Then
                linkAlias = linkAliasList.First.Link
                If Mid(linkAlias, 1, 1) <> "/" Then
                    linkAlias = "/" & linkAlias
                End If
            End If
            Return linkAlias
        End Function
        '
        '=================================================================================================================================================
        '   csv_addLinkAlias
        '
        '   Link Alias
        '       A LinkAlias name is a unique string that identifies a page on the site.
        '       A page on the site is generated from the PageID, and the QueryStringSuffix
        '       PageID - obviously, this is the ID of the page
        '       QueryStringSuffix - other things needed on the Query to display the correct content.
        '           The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
        '           Link, plus the suffix needed to find the content.
        '
        '       When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
        '
        '   The Link Alias table no longer needs the Link field.
        '
        '=================================================================================================================================================
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Shared Sub app_addLinkAlias(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String)
            Dim return_ignoreError As String = ""
            Call app_addLinkAlias2(cpcore, linkAlias, PageID, QueryStringSuffix, True, False, return_ignoreError)
        End Sub
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Shared Sub app_addLinkAlias2(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String, Optional OverRideDuplicate As Boolean = False, Optional DupCausesWarning As Boolean = False, Optional ByRef return_WarningMessage As String = "")
            Const SafeString = "0123456789abcdefghijklmnopqrstuvwxyz-_/."
            Dim Ptr As Integer
            Dim TestChr As String
            Dim Src As String
            Dim FieldList As String
            Dim LinkAliasPageID As Integer
            Dim PageContentCID As Integer
            Dim WorkingLinkAlias As String
            Dim CS As Integer
            Dim LoopCnt As Integer
            'Dim fs As New fileSystemClass
            Dim FolderCheck As String
            Dim SQL As String
            Dim AllowLinkAlias As Boolean
            'dim buildversion As String
            '
            If (True) Then
                AllowLinkAlias = cpcore.siteProperties.getBoolean("allowLinkAlias", False)
                WorkingLinkAlias = linkAlias
                If (WorkingLinkAlias <> "") Then
                    '
                    ' remove nonsafe URL characters
                    '
                    Src = WorkingLinkAlias
                    Src = genericController.vbReplace(Src, "’", "'")
                    Src = genericController.vbReplace(Src, vbTab, " ")
                    WorkingLinkAlias = ""
                    For Ptr = 1 To Len(Src) + 1
                        TestChr = Mid(Src, Ptr, 1)
                        If genericController.vbInstr(1, SafeString, TestChr, vbTextCompare) <> 0 Then
                        Else
                            TestChr = vbTab
                        End If
                        WorkingLinkAlias = WorkingLinkAlias & TestChr
                    Next
                    Ptr = 0
                    Do While genericController.vbInstr(1, WorkingLinkAlias, vbTab & vbTab) <> 0 And (Ptr < 100)
                        WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab & vbTab, vbTab)
                        Ptr = Ptr + 1
                    Loop
                    If Right(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 1, Len(WorkingLinkAlias) - 1)
                    End If
                    If Left(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 2)
                    End If
                    WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab, "-")
                    If (WorkingLinkAlias <> "") Then
                        '
                        ' Make sure there is not a folder or page in the wwwroot that matches this Alias
                        '
                        If Left(WorkingLinkAlias, 1) <> "/" Then
                            WorkingLinkAlias = "/" & WorkingLinkAlias
                        End If
                        '
                        If genericController.vbLCase(WorkingLinkAlias) = genericController.vbLCase("/" & cpcore.serverConfig.appConfig.name) Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf genericController.vbLCase(WorkingLinkAlias) = "/cclib" Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf cpcore.appRootFiles.pathExists(cpcore.serverConfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            'ElseIf appRootFiles.pathExists(serverConfig.clusterPath & serverconfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            '
                            ' This alias points to a different link, call it an error
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a folder in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        Else
                            '
                            ' Make sure there is one here for this
                            '
                            If True Then
                                FieldList = "Name,PageID,QueryStringSuffix"
                            Else
                                '
                                ' must be > 33914 to run this routine
                                '
                                FieldList = "Name,PageID,'' as QueryStringSuffix"
                            End If
                            CS = cpcore.db.cs_open("Link Aliases", "name=" & cpcore.db.encodeSQLText(WorkingLinkAlias), , , , , , FieldList)
                            If Not cpcore.db.cs_ok(CS) Then
                                '
                                ' Alias not found, create a Link Aliases
                                '
                                Call cpcore.db.cs_Close(CS)
                                CS = cpcore.db.cs_insertRecord("Link Aliases", 0)
                                If cpcore.db.cs_ok(CS) Then
                                    Call cpcore.db.cs_set(CS, "Name", WorkingLinkAlias)
                                    'Call app.csv_SetCS(CS, "Link", Link)
                                    Call cpcore.db.cs_set(CS, "Pageid", PageID)
                                    If True Then
                                        Call cpcore.db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                    End If
                                End If
                            Else
                                '
                                ' Alias found, verify the pageid & QueryStringSuffix
                                '
                                Dim CurrentLinkAliasID As Integer
                                Dim resaveLinkAlias As Boolean
                                Dim CS2 As Integer
                                LinkAliasPageID = cpcore.db.cs_getInteger(CS, "pageID")
                                If (cpcore.db.cs_getText(CS, "QueryStringSuffix").ToLower = QueryStringSuffix.ToLower) And (PageID = LinkAliasPageID) Then
                                    '
                                    ' it maches a current entry for this link alias, if the current entry is not the highest number id,
                                    '   remove it and add this one
                                    '
                                    CurrentLinkAliasID = cpcore.db.cs_getInteger(CS, "id")
                                    CS2 = cpcore.db.cs_openCsSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" & LinkAliasPageID & " order by id desc")
                                    If cpcore.db.cs_ok(CS2) Then
                                        resaveLinkAlias = (CurrentLinkAliasID <> cpcore.db.cs_getInteger(CS2, "id"))
                                    End If
                                    Call cpcore.db.cs_Close(CS2)
                                    If resaveLinkAlias Then
                                        Call cpcore.db.executeSql("delete from ccLinkAliases where id=" & CurrentLinkAliasID)
                                        Call cpcore.db.cs_Close(CS)
                                        CS = cpcore.db.cs_insertRecord("Link Aliases", 0)
                                        If cpcore.db.cs_ok(CS) Then
                                            Call cpcore.db.cs_set(CS, "Name", WorkingLinkAlias)
                                            Call cpcore.db.cs_set(CS, "Pageid", PageID)
                                            If True Then
                                                Call cpcore.db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                            End If
                                        End If
                                    End If
                                Else
                                    '
                                    ' Does not match, this is either a change, or a duplicate that needs to be blocked
                                    '
                                    If OverRideDuplicate Then
                                        '
                                        ' change the Link Alias to the new link
                                        '
                                        'Call app.csv_SetCS(CS, "Link", Link)
                                        Call cpcore.db.cs_set(CS, "Pageid", PageID)
                                        If True Then
                                            Call cpcore.db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                        End If
                                    ElseIf AllowLinkAlias Then
                                        '
                                        ' This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                        '
                                        If DupCausesWarning Then
                                            If LinkAliasPageID = 0 Then '
                                                PageContentCID = cpcore.metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website."
                                            Else
                                                PageContentCID = cpcore.metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page (<a href=""?af=4&cid=" & PageContentCID & "&id=" & LinkAliasPageID & """>edit</a>)." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website."
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            Call cpcore.db.cs_Close(CS)
                        End If
                    End If
                End If
            End If
        End Sub

    End Class
End Namespace