
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
    Public Class docController
        ''' <summary>
        ''' Assemble a document, html or otherwise. maintain all the parts and output the results. Constructor initializes the object. Call initDoc() to setup pages
        ''' </summary>
        ' -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
        '
        Public Const htmlDoc_JavaStreamChunk = 100
        Public Const htmlDoc_OutStreamStandard = 0
        Public Const htmlDoc_OutStreamJavaScript = 1
        Public Const main_BakeHeadDelimiter = "#####MultilineFlag#####"
        Public Const navStruc_Descriptor = 1           ' Descriptors:0 = RootPage, 1 = Parent Page, 2 = Current Page, 3 = Child Page
        Public Const navStruc_Descriptor_CurrentPage = 2
        Public Const navStruc_Descriptor_ChildPage = 3
        Public Const navStruc_TemplateId = 7
        Public Const blockMessageDefault = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
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
        '
        ' Cache the input selects (admin uses the same ones over and over)
        '
        Public Structure main_InputSelectCacheType
            Dim SelectRaw As String
            Dim ContentName As String
            Dim Criteria As String
            Dim CurrentValue As String
        End Structure
        '
        ' block of js code that goes into a script tag
        '
        Public Structure main_HeadScriptType
            Dim IsLink As Boolean
            Dim Text As String
            Dim addedByMessage As String
        End Structure
        '
        Private cpcore As coreClass
        Public Property domain As Models.Entity.domainModel
        Public Property page As Models.Entity.pageContentModel
        Public Property pageToRootList As List(Of Models.Entity.pageContentModel)
        Friend Property headTags As String = ""
        Public Property template As Models.Entity.pageTemplateModel
        Public Property templateReason As String = ""
        Public Property editWrapperCnt As Integer = 0
        Public Property docBodyFilter As String = ""
        Public Property legacySiteStyles_Loaded As Boolean = False
        Public Property menuSystemCloseCount As Integer = 0
        Friend Property helpCodeCount As Integer = 0
        Friend Property helpCodeSize As Integer = 0
        Friend Property helpCodes As String() = {}
        Friend Property helpCaptions As String() = {}
        Friend Property helpDialogCnt As Integer = 0
        Public Property htmlForEndOfBody As String = ""             ' Anything that needs to be written to the Page during main_GetClosePage
        Public Property isPrintVersion As Boolean = False
        Public Property refreshQueryString As String = ""      ' the querystring required to return to the current state (perform a refresh)
        Public Property redirectContentID As Integer = 0
        Public Property redirectRecordID As Integer = 0
        Public Property javascriptStreamHolder As String() = {}
        Public Property javascriptStreamSize As Integer = 0
        Public Property javascriptStreamCount As Integer = 0
        Public Property isStreamWritten As Boolean = False       ' true when anything has been writeAltBuffered.
        Public Property outputBufferEnabled As Boolean = True          ' when true (default), stream is buffered until page is done
        Public Property docBuffer As String = ""                   ' if any method calls writeAltBuffer, string concatinates here. If this is not empty at exit, it is used instead of returned string
        Public Property metaContent_Title As String = ""
        Public Property metaContent_Description As String = ""
        Public Property metaContent_OtherHeadTags As String = ""
        Public Property metaContent_KeyWordList As String = ""
        Public Property metaContent_StyleSheetTags As String = ""
        Public Property metaContent_TemplateStyleSheetTag As String = ""
        Public Property metaContent_SharedStyleIDList As String = ""
        Public Property menuComboTab As menuComboTabController
        Public Property menuLiveTab As menuLiveTabController
        Public Property adminWarning As String = ""                                      ' Message - when set displays in an admin hint box in the page
        Public Property adminWarningPageID As Integer = 0                                  ' PageID that goes with the warning
        Public Property checkListCnt As Integer = 0                    ' cnt of the main_GetFormInputCheckList calls - used for javascript
        Public Property includedAddonIDList As String = ""
        Public Property onLoadJavascript As String = ""
        Public Property endOfBodyJavascript As String = ""           ' javascript that goes at the end of the close page
        Public Property endOfBodyString As String = ""
        Public Property headScripts As main_HeadScriptType() = {}
        Public Property inputDateCnt As Integer = 0
        Public Property inputSelectCacheCnt As Integer = 0
        Public Property inputSelectCache As main_InputSelectCacheType()
        Public Property formInputTextCnt As Integer = 0
        Public Property quickEditCopy As String = ""
        Private Property javascriptOnLoad As String() = {}
        Private Property javascriptReferenceFilename_Cnt As Integer
        Private Property javascriptReferenceFilename As String() = {}
        Friend Property javascriptBodyEnd As String() = {}
        Friend Property styleFilenames_Cnt As Integer
        Friend Property styleFilenames As String() = {}
        Public Property siteStructure As String = ""
        Public Property siteStructure_LocalLoaded As Boolean = False
        Public Property bodyContent As String = ""
        Public Property landingPageID As Integer = 0
        Public Property redirectLink As String = ""
        Public Property redirectReason As String = ""
        Public Property redirectBecausePageNotFound As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' this will eventuall be an addon, but lets do this first to keep the converstion complexity down
        ''' </summary>
        ''' <param name="cpCore"></param>

        Public Sub New(cpCore As coreClass)
            Me.cpcore = cpCore
            '
            domain = New Models.Entity.domainModel()
            page = New pageContentModel()
            pageToRootList = New List(Of pageContentModel)
            template = New pageTemplateModel()
            '' -- setup domain
            'domain = Models.Entity.domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
            'If (domain Is Nothing) Then
            '    '
            '    ' -- domain not configured
            '    cpCore.handleExceptionAndContinue(New ApplicationException("Domain [" & cpCore.webServer.requestDomain & "] has not been configured."))
            'Else
            '    If (pageId = 0) Then
            '        '
            '        ' -- Nothing specified, use the Landing Page
            '        pageId = getLandingPageID()
            '    End If
            '    Call cpCore.doc.addRefreshQueryString(rnPageId, CStr(pageId))
            '    '
            '    ' -- build parentpageList (first = current page, last = root)
            '    ' -- add a 0, then repeat until another 0 is found, or there is a repeat
            '    pageToRootList = New List(Of Models.Entity.pageContentModel)()
            '    Dim usedPageIdList As New List(Of Integer)()
            '    Dim targetPageId = pageId
            '    usedPageIdList.Add(0)
            '    Do While (Not usedPageIdList.Contains(targetPageId))
            '        usedPageIdList.Add(targetPageId)
            '        Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpCore, targetPageId, New List(Of String))
            '        If (targetpage Is Nothing) Then
            '            Exit Do
            '        Else
            '            pageToRootList.Add(targetpage)
            '            targetPageId = targetpage.ParentID
            '        End If
            '    Loop
            '    If (pageToRootList.Count = 0) Then
            '        '
            '        page = New pageContentModel()
            '    Else
            '        page = pageToRootList.First
            '    End If
            '    '
            '    ' -- get template from pages
            '    template = Nothing
            '    For Each page As Models.Entity.pageContentModel In pageToRootList
            '        If page.TemplateID > 0 Then
            '            template = Models.Entity.pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
            '            If (template IsNot Nothing) Then
            '                If (page Is pageToRootList.First) Then
            '                    templateReason = "This template was used because it is selected by the current page."
            '                Else
            '                    templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
            '                End If
            '                Exit For
            '            End If
            '        End If
            '    Next
            '    '
            '    If (template Is Nothing) Then
            '        '
            '        ' -- get template from domain
            '        If (domain IsNot Nothing) Then
            '            template = Models.Entity.pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
            '        End If
            '        If (template Is Nothing) Then
            '            '
            '            ' -- get template named Default
            '            template = Models.Entity.pageTemplateModel.createByName(cpCore, defaultTemplateName, New List(Of String))
            '        End If
            '        If (template Is Nothing) Then
            '            '
            '            ' -- ceate new template named Default
            '            template = Models.Entity.pageTemplateModel.add(cpCore, New List(Of String))
            '            template.Name = defaultTemplateName
            '            template.BodyHTML = defaultTemplateHtml
            '            template.save(cpCore)
            '        End If
            '    End If
            'End If
        End Sub
        '
        Public Sub initDoc(pageId As Integer, Optional domainName As String = "")
            Try
                '
                ' -- setup domain
                domain = Models.Entity.domainModel.createByName(cpcore, domainName, New List(Of String))
                If (domain Is Nothing) Then
                    '
                    ' -- domain not configured
                    cpcore.handleException(New ApplicationException("Domain [" & cpcore.webServer.requestDomain & "] has not been configured."))
                Else
                    If (pageId = 0) Then
                        '
                        ' -- Nothing specified, use the Landing Page
                        pageId = getLandingPageID()
                    End If
                    Call cpcore.doc.addRefreshQueryString(rnPageId, CStr(pageId))
                    '
                    ' -- build parentpageList (first = current page, last = root)
                    ' -- add a 0, then repeat until another 0 is found, or there is a repeat
                    pageToRootList = New List(Of Models.Entity.pageContentModel)()
                    Dim usedPageIdList As New List(Of Integer)()
                    Dim targetPageId = pageId
                    usedPageIdList.Add(0)
                    Do While (Not usedPageIdList.Contains(targetPageId))
                        usedPageIdList.Add(targetPageId)
                        Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpcore, targetPageId, New List(Of String))
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
                            template = Models.Entity.pageTemplateModel.create(cpcore, page.TemplateID, New List(Of String))
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
                            template = Models.Entity.pageTemplateModel.create(cpcore, domain.DefaultTemplateId, New List(Of String))
                        End If
                        If (template Is Nothing) Then
                            '
                            ' -- get template named Default
                            template = Models.Entity.pageTemplateModel.createByName(cpcore, defaultTemplateName, New List(Of String))
                        End If
                        If (template Is Nothing) Then
                            '
                            ' -- ceate new template named Default
                            template = Models.Entity.pageTemplateModel.add(cpcore, New List(Of String))
                            template.Name = defaultTemplateName
                            template.BodyHTML = defaultTemplateHtml
                            template.save(cpcore)
                        End If
                    End If
                End If
            Catch ex As Exception

            End Try
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
                Dim AddonReturn As String
                Dim Ptr As Integer
                Dim Cnt As Integer
                Dim layoutError As String = String.Empty
                Dim FilterStatusOK As Boolean
                Dim Result As New stringBuilderLegacyController
                Dim PageContent As String
                Dim Stream As New stringBuilderLegacyController
                Dim LocalTemplateID As Integer
                Dim LocalTemplateName As String
                Dim LocalTemplateBody As String
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
                                Throw New ApplicationException("Unexpected exception")
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
                    If Not cpCore.doc.isPrintVersion Then
                        LocalTemplateBody = cpCore.html.html_executeContentCommands(Nothing, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.authContext.user.id, cpCore.authContext.isAuthenticated, layoutError)
                        returnBody = returnBody & cpCore.html.encodeContent9(LocalTemplateBody, cpCore.authContext.user.id, "Page Templates", LocalTemplateID, 0, False, False, True, True, False, True, "", cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain, False, cpCore.siteProperties.defaultWrapperID, PageContent, CPUtilsBaseClass.addonContext.ContextTemplate)
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
                        If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") And cpCore.authContext.isEditing("Page Templates") Then
                            returnBody = cpCore.html.main_GetEditWrapper("Page Template [" & LocalTemplateName & "]", cpCore.html.main_GetRecordEditLink2("Page Templates", LocalTemplateID, False, LocalTemplateName, cpCore.authContext.isEditing("Page Templates")) & returnBody)
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
                                cpCore.doc.docBodyFilter = returnBody
                                AddonReturn = cpCore.addon.execute_legacy2(addonId, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", FilterStatusOK, Nothing)
                                returnBody = cpCore.doc.docBodyFilter & AddonReturn
                                If Not FilterStatusOK Then
                                    Throw New ApplicationException("Unexpected exception")
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
                cpCore.handleException(ex) : Throw
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
                    cpCore.doc.adminWarning = cpCore.docProperties.getText("main_AdminWarningMsg")
                    cpCore.doc.adminWarningPageID = cpCore.docProperties.getInteger("main_AdminWarningPageID")
                    '
                    ' todo move cookie test to htmlDoc controller
                    ' -- Add cookie test
                    Dim AllowCookieTest As Boolean
                    AllowCookieTest = cpCore.siteProperties.allowVisitTracking And (cpCore.authContext.visit.PageVisits = 1)
                    If AllowCookieTest Then
                        Call cpCore.html.addOnLoadJavascript("if (document.cookie && document.cookie != null){cj.ajax.qs('f92vo2a8d=" & cpCore.security.encodeToken(cpCore.authContext.visit.id, cpCore.app_startTime) & "')};", "Cookie Test")
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
                    cpCore.doc.redirectContentID = (cpCore.docProperties.getInteger(rnRedirectContentId))
                    If (cpCore.doc.redirectContentID <> 0) Then
                        cpCore.doc.redirectRecordID = (cpCore.docProperties.getInteger(rnRedirectRecordId))
                        If (cpCore.doc.redirectRecordID <> 0) Then
                            Dim contentName As String = cpCore.metaData.getContentNameByID(cpCore.doc.redirectContentID)
                            If contentName <> "" Then
                                If iisController.main_RedirectByRecord_ReturnStatus(cpCore, contentName, cpCore.doc.redirectRecordID) Then
                                    '
                                    'Call AppendLog("main_init(), 3210 - exit for rc/ri redirect ")
                                    '
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.doc.docBuffer
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
                                        log.VisitID = cpCore.authContext.visit.id
                                        log.MemberID = cpCore.authContext.user.id
                                    End If
                                    '
                                    ' -- and go
                                    Dim link As String = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, file.Filename)
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
                                        Return cpCore.doc.docBuffer
                                    Else
                                        Call cpCore.webServer.redirect(genericController.getCdnFileLink(cpCore, Filename), "favicon request", False)
                                        cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                        Return cpCore.doc.docBuffer
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
                                    Call cpCore.html.writeAltBuffer(Content)
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.doc.docBuffer
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
                        If (cpCore.webServer.requestPath <> "/") And genericController.vbInstr(1, cpCore.siteProperties.adminURL, cpCore.webServer.requestPath, vbTextCompare) <> 0 Then
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
                                    Call cpCore.html.main_SetMetaContent(0, 0)
                                    Dim login As New Addons.addon_loginClass(cpCore)
                                    Call cpCore.html.writeAltBuffer(login.getLoginPage(False))
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.doc.docBuffer
                                Case 2
                                    '
                                    ' block with custom content
                                    '
                                    '
                                    'Call AppendLog("main_init(), 3420 - exit for custom content block")
                                    '
                                    Call cpCore.html.main_SetMetaContent(0, 0)
                                    Call cpCore.html.addOnLoadJavascript("document.body.style.overflow='scroll'", "Anonymous User Block")
                                    Dim Copy As String = cr & cpCore.html.html_GetContentCopy("AnonymousUserResponseCopy", "<p style=""width:250px;margin:100px auto auto auto;"">The site is currently not available for anonymous access.</p>", cpCore.authContext.user.id, True, cpCore.authContext.isAuthenticated)
                                    ' -- already encoded
                                    'Copy = EncodeContentForWeb(Copy, "copy content", 0, "", 0)
                                    Copy = "" _
                                            & cpCore.siteProperties.docTypeDeclaration() _
                                            & vbCrLf & "<html>" _
                                            & cr & "<head>" _
                                            & genericController.htmlIndent(cpCore.html.getHtmlDocHead(False)) _
                                            & cr & "</head>" _
                                            & cr & TemplateDefaultBodyTag _
                                            & genericController.htmlIndent(Copy) _
                                            & cr2 & "<div>" _
                                            & cr3 & cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(True, True, False, False) _
                                            & cr2 & "</div>" _
                                            & cr & "</body>" _
                                            & vbCrLf & "</html>"
                                    '& "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">"
                                    Call cpCore.html.writeAltBuffer(Copy)
                                    cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
                                    Return cpCore.doc.docBuffer
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
                    Call cpCore.html.addOnLoadJavascript(template.JSOnLoad, "template")
                    Call cpCore.html.addHeadJavascriptCode(template.JSHead, "template")
                    Call cpCore.html.addBodyJavascriptCode(template.JSEndBody, "template")
                    Call cpCore.html.main_AddHeadTag2(template.OtherHeadTags, "template")
                    If template.StylesFilename <> "" Then
                        cpCore.doc.metaContent_TemplateStyleSheetTag = cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, template.StylesFilename) & """ >"
                    End If
                    ''
                    '' -- add shared styles
                    'Dim sqlCriteria As String = "(templateId=" & template.ID & ")"
                    'Dim styleList As List(Of Models.Entity.SharedStylesTemplateRuleModel) = Models.Entity.SharedStylesTemplateRuleModel.createList(cpCore, sqlCriteria, "sortOrder,id")
                    'For Each rule As SharedStylesTemplateRuleModel In styleList
                    '    Call cpCore.html.addSharedStyleID2(rule.StyleID, "template")
                    'Next
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
                        Me.redirectReason = "Redirecting because neither the page or the template requires a secure link."
                        Return ""
                    ElseIf ((Not SecureLink_CurrentURL) And SecureLink_Required) Then
                        '
                        ' -- redirect to secure
                        RedirectLink = genericController.vbReplace(cpCore.webServer.requestUrl, "http://", "https://")
                        If SecureLink_Page_Required Then
                            Me.redirectReason = "Redirecting because this page [" & pageToRootList(0).name & "] requires a secure link."
                        Else
                            Me.redirectReason = "Redirecting because this template [" & template.Name & "] requires a secure link."
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
                            RedirectLink = cpCore.webServer.requestProtocol & domain.Name
                            redirectBecausePageNotFound = False
                            Me.redirectReason = "Redirecting because this domain has template requiements set, and this template is not configured [" & template.Name & "]."
                            Return ""
                        End If
                    End If
                    If cpCore.continueProcessing Then
                        '
                        ' Build Body Tag
                        '
                        htmlDocHead = cpCore.html.getHtmlDocHead(False)
                        If template.BodyTag <> "" Then
                            bodyTag = template.BodyTag
                        Else
                            bodyTag = TemplateDefaultBodyTag
                        End If
                        '
                        ' Add tools panel to body
                        '
                        htmlDocBody = htmlDocBody & cr & "<div>" & genericController.htmlIndent(cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(True, True, False, False)) & cr & "</div>"
                        '
                        ' build doc
                        '
                        returnHtml = cpCore.html.assembleHtmlDoc(cpCore.siteProperties.docTypeDeclaration(), htmlDocHead, bodyTag, cpCore.doc.docBuffer & htmlDocBody)
                    End If
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
                            Call cpCore.webServer.redirect(linkDst, Copy, False)
                            cpCore.continueProcessing = False '--- should be disposed by caller --- Call dispose
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
                            cpCore.docProperties.setProperty(rnPageId, main_GetPageNotFoundPageId())
                            'Call main_mergeInStream(rnPageId & "=" & main_GetPageNotFoundPageId())
                            If cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                cpCore.doc.adminWarning = PageNotFoundReason
                                cpCore.doc.adminWarningPageID = 0
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
                Dim Formhtml As String = String.Empty
                Dim FormInstructions As String = String.Empty
                Dim f As docController.main_FormPagetype
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
                If cpcore.db.cs_ok(CS) Then
                    Formhtml = cpcore.db.cs_getText(CS, "Body")
                    FormInstructions = cpcore.db.cs_getText(CS, "Instructions")
                End If
                Call cpcore.db.cs_Close(CS)
                If FormInstructions <> "" Then
                    '
                    ' Load the instructions
                    '
                    f = loadFormPageInstructions(FormInstructions, Formhtml)
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
                                            errorController.error_AddUserError(cpcore, "The field [" & .Caption & "] must be unique, and the value [" & genericController.encodeHTML(FormValue) & "] has already been used.")
                                        End If
                                    End If
                                    If (.REquired Or genericController.EncodeBoolean(cpcore.metaData.GetContentFieldProperty("people", .PeopleField, "required"))) And FormValue = "" Then
                                        Success = False
                                        errorController.error_AddUserError(cpcore, "The field [" & genericController.encodeHTML(.Caption) & "] is required.")
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
                cpcore.handleException(ex)
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
                Dim FieldRows As Integer
                'Dim templateId As Integer
                Dim RootPageContentName As String
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
                    redirectBecausePageNotFound = True
                    redirectReason = "Redirecting because the page selected could not be found."
                    redirectLink = main_ProcessPageNotFound_GetLink(redirectReason, , , PageID, 0)
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
                Dim styleOptionList As String = String.Empty
                Dim addonListJSON As String
                If redirectLink = "" And (InStr(1, returnHtml, html_quickEdit_fpo) <> 0) Then
                    FieldRows = genericController.EncodeInteger(cpCore.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"))
                    If FieldRows < 50 Then
                        FieldRows = 50
                        Call cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50)
                    End If
                    Dim stylesheetCommaList As String = "" 'cpCore.html.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
                    addonListJSON = cpCore.html.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb)
                    Editor = cpCore.html.html_GetFormInputHTML3("copyFilename", quickEditCopy, CStr(FieldRows), "100%", False, True, addonListJSON, stylesheetCommaList, styleOptionList)
                    returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor)
                End If
                '
                ' -- Add admin warning to the top of the content
                If cpCore.authContext.isAuthenticatedAdmin(cpCore) And cpCore.doc.adminWarning <> "" Then
                    '
                    ' Display Admin Warnings with Edits for record errors
                    '
                    If cpCore.doc.adminWarningPageID <> 0 Then
                        cpCore.doc.adminWarning = cpCore.doc.adminWarning & "</p>" & cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, True, "Page " & cpCore.doc.adminWarningPageID, cpCore.authContext.isAuthenticatedAdmin(cpCore)) & "&nbsp;Edit the page<p>"
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
                If redirectLink <> "" Then
                    Call cpCore.webServer.redirect(redirectLink, redirectReason, redirectBecausePageNotFound)
                ElseIf AllowEditWrapper Then
                    returnHtml = cpCore.html.main_GetEditWrapper("Page Content", returnHtml)
                End If
                '
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        '   getContentBox
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
                Call cpcore.html.main_AddHeadTag2("<meta name=""contentId"" content=""" & page.id & """ >", "page content")
                '
                returnHtml = getContentBox_content(OrderByClause, AllowChildPageList, AllowReturnLink, ArchivePages, ignoreme, UseContentWatchLink, allowPageWithoutSectionDisplay)
                '
                ' ----- If Link field populated, do redirect
                If (page.PageLink <> "") Then
                    page.Clicks += 1
                    page.save(cpcore)
                    redirectLink = page.PageLink
                    redirectReason = "Redirect required because this page (PageRecordID=" & page.id & ") has a Link Override [" & page.PageLink & "]."
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
                                    Dim loginAddon As New Addons.addon_loginClass(cpcore)
                                    BlockForm = loginAddon.getLoginForm()
                                Else
                                    '
                                    ' recognized, not authenticated
                                    '
                                    BlockCopy = "" _
                                        & "<p>This content has limited access. You were recognized as ""<b>" & cpcore.authContext.user.Name & "</b>"", but you need to login to continue. To login to this account or another, please use this form.</p>" _
                                        & ""
                                    Dim loginAddon As New Addons.addon_loginClass(cpcore)
                                    BlockForm = loginAddon.getLoginForm()
                                End If
                            Else
                                '
                                ' authenticated
                                '
                                BlockCopy = "" _
                                    & "<p>You are currently logged in as ""<b>" & cpcore.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & cpcore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
                                    & "<p>This account does not have access to this content. If you want to login with a different account, please use this form.</p>" _
                                    & ""
                                Dim loginAddon As New Addons.addon_loginClass(cpcore)
                                BlockForm = loginAddon.getLoginForm()
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
                                Dim loginAddon As New Addons.addon_loginClass(cpcore)
                                BlockForm = loginAddon.getLoginForm()
                                BlockCopy = "" _
                                    & "<p>This content has limited access. If you have an account, please login using this form.</p>" _
                                    & "<p>If you do not have an account, <a href=?" & cpcore.doc.refreshQueryString & "&subform=0>click here to register</a>.</p>" _
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
                                        & "<p>This content has limited access. If you have an account, <a href=?" & cpcore.doc.refreshQueryString & "&subform=" & main_BlockSourceLogin & ">Click Here to login</a>.</p>" _
                                        & "<p>To view this content, please complete this form.</p>" _
                                        & ""
                                Else
                                    BlockCopy = "" _
                                        & "<p>You are currently logged in as ""<b>" & cpcore.authContext.user.Name & "</b>"". If this is not you, please <a href=""?" & cpcore.doc.refreshQueryString & "&method=logout"" rel=""nofollow"">Click Here</a>.</p>" _
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
                                    BlockForm = getFormPage("Registration Form", RegistrationGroupID)
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
                            returnHtml = getDefaultBlockMessage(UseContentWatchLink)
                    End Select
                    '
                    ' If the output is blank, put default message in
                    '
                    If returnHtml = "" Then
                        returnHtml = getDefaultBlockMessage(UseContentWatchLink)
                    End If
                    '
                    ' Encode the copy
                    '
                    returnHtml = cpcore.html.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                    returnHtml = cpcore.html.encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                    If cpcore.doc.refreshQueryString <> "" Then
                        returnHtml = genericController.vbReplace(returnHtml, "?method=login", "?method=Login&" & cpcore.doc.refreshQueryString, 1, 99, vbTextCompare)
                    End If
                    '
                    ' Add in content padding required for integration with the template
                    returnHtml = getContentBoxWrapper(returnHtml, ContentPadding)
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
                        If cpcore.authContext.isEditing(pageContentModel.contentName) Or cpcore.visitProperty.getBoolean("AllowWorkflowRendering") Then
                            '
                            ' Link authoring, workflow rendering -> do encoding, but no tracking
                            '
                            returnHtml = cpcore.html.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.html.encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                        ElseIf cpcore.doc.isPrintVersion Then
                            '
                            ' Printer Version -> personalize and count viewings, no tracking
                            '
                            returnHtml = cpcore.html.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.html.encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
                            Call cpcore.db.executeSql("update ccpagecontent set viewings=" & (pageViewings + 1) & " where id=" & page.id)
                        Else
                            '
                            ' Live content
                            returnHtml = cpcore.html.html_executeContentCommands(Nothing, returnHtml, CPUtilsBaseClass.addonContext.ContextPage, cpcore.authContext.user.id, cpcore.authContext.isAuthenticated, layoutError)
                            returnHtml = cpcore.html.encodeContent9(returnHtml, cpcore.authContext.user.id, pageContentModel.contentName, PageRecordID, page.ContactMemberID, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, cpcore.siteProperties.defaultWrapperID, "", CPUtilsBaseClass.addonContext.ContextPage)
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
                                Body = Body & getTableRow("Domain", cpcore.webServer.requestDomain, True)
                                Body = Body & getTableRow("Link", cpcore.webServer.requestUrl, False)
                                Body = Body & getTableRow("Page Name", PageName, True)
                                Body = Body & getTableRow("Member Name", cpcore.authContext.user.Name, False)
                                Body = Body & getTableRow("Member #", CStr(cpcore.authContext.user.id), True)
                                Body = Body & getTableRow("Visit Start Time", CStr(cpcore.authContext.visit.StartTime), False)
                                Body = Body & getTableRow("Visit #", CStr(cpcore.authContext.visit.id), True)
                                Body = Body & getTableRow("Visit IP", cpcore.webServer.requestRemoteIP, False)
                                Body = Body & getTableRow("Browser ", cpcore.webServer.requestBrowser, True)
                                Body = Body & getTableRow("Visitor #", CStr(cpcore.authContext.visitor.ID), False)
                                Body = Body & getTableRow("Visit Authenticated", CStr(cpcore.authContext.visit.VisitAuthenticated), True)
                                Body = Body & getTableRow("Visit Referrer", cpcore.authContext.visit.HTTP_REFERER, False)
                                Body = Body & kmaEndTable
                                Call cpcore.email.sendPerson(page.ContactMemberID, cpcore.siteProperties.getText("EmailFromAddress", "info@" & cpcore.webServer.requestDomain), "Page Hit Notification", Body, False, True, 0, "", False)
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
                    returnHtml = getContentBoxWrapper(returnHtml, page.ContentPadding)
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
                    Call cpcore.html.addOnLoadJavascript(page.JSOnLoad, "page content")
                    Call cpcore.html.addHeadJavascriptCode(page.JSHead, "page content")
                    If page.JSFilename <> "" Then
                        Call cpcore.html.addJavaScriptLinkHead(genericController.getCdnFileLink(cpcore, page.JSFilename), "page content")
                    End If
                    Call cpcore.html.addBodyJavascriptCode(page.JSEndBody, "page content")
                    '
                    '---------------------------------------------------------------------------------
                    ' Set the Meta Content flag
                    '---------------------------------------------------------------------------------
                    '
                    Call cpcore.html.main_SetMetaContent(page.ContentControlID, page.id)
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
                If cpcore.doc.metaContent_Title = "" Then
                    '
                    ' Set default page title
                    '
                    cpcore.doc.metaContent_Title = page.name
                End If
                '
                ' add contentid and sectionid
                '
                Call cpcore.html.main_AddHeadTag2("<meta name=""contentId"" content=""" & page.id & """ >", "page content")
                '
                ' Display Admin Warnings with Edits for record errors
                '
                If cpcore.doc.adminWarning <> "" Then
                    '
                    If cpcore.doc.adminWarningPageID <> 0 Then
                        cpcore.doc.adminWarning = cpcore.doc.adminWarning & "</p>" & cpcore.html.main_GetRecordEditLink2("Page Content", cpcore.doc.adminWarningPageID, True, "Page " & cpcore.doc.adminWarningPageID, cpcore.authContext.isAuthenticatedAdmin(cpcore)) & "&nbsp;Edit the page<p>"
                        cpcore.doc.adminWarningPageID = 0
                    End If
                    returnHtml = "" _
                    & cpcore.html.html_GetAdminHintWrapper(cpcore.doc.adminWarning) _
                    & returnHtml _
                    & ""
                    cpcore.doc.adminWarning = ""
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
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
        Friend Function getContentBox_content(OrderByClause As String, AllowChildPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, ignoreMe As Integer, UseContentWatchLink As Boolean, allowPageWithoutSectionDisplay As Boolean) As String
            Dim result As String = ""
            Try
                Dim isEditing As Boolean
                Dim LiveBody As String
                '
                If cpcore.continueProcessing Then
                    If redirectLink = "" Then
                        isEditing = cpcore.authContext.isEditing(pageContentModel.contentName)
                        '
                        ' ----- Render the Body
                        LiveBody = getContentBox_content_Body(OrderByClause, AllowChildPageList, False, pageToRootList.Last.id, AllowReturnLink, pageContentModel.contentName, ArchivePages)
                        Dim isRootPage As Boolean = (pageToRootList.Count = 1)
                        If cpcore.authContext.isAdvancedEditing(cpcore, "") Then
                            result = result & cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, page.id, (Not isRootPage)) & LiveBody
                        ElseIf isEditing Then
                            result = result & cpcore.html.main_GetEditWrapper("", cpcore.html.main_GetRecordEditLink(pageContentModel.contentName, page.id, (Not isRootPage)) & LiveBody)
                        Else
                            result = result & LiveBody
                        End If
                    End If
                End If
                '
                Call debugController.debug_testPoint(cpcore, "getContentBox_content")
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

        Friend Function getContentBox_content_Body(OrderByClause As String, AllowChildList As Boolean, Authoring As Boolean, rootPageId As Integer, AllowReturnLink As Boolean, RootPageContentName As String, ArchivePage As Boolean) As String
            Dim result As String = ""
            Try
                Dim allowChildListComposite As Boolean = AllowChildList And page.AllowChildListDisplay
                Dim allowReturnLinkComposite As Boolean = AllowReturnLink And page.AllowReturnLinkDisplay
                Dim bodyCopy As String = page.Copyfilename.copy
                Dim breadCrumb As String = ""
                Dim BreadCrumbDelimiter As String
                Dim BreadCrumbPrefix As String
                Dim isRootPage As Boolean = pageToRootList.Count.Equals(1)
                '
                If allowReturnLinkComposite And (Not isRootPage) And (Not isPrintVersion) Then
                    '
                    ' ----- Print Heading if not at root Page
                    '
                    BreadCrumbPrefix = cpcore.siteProperties.getText("BreadCrumbPrefix", "Return to")
                    BreadCrumbDelimiter = cpcore.siteProperties.getText("BreadCrumbDelimiter", " &gt; ")
                    breadCrumb = getReturnBreadcrumb(RootPageContentName, page.ParentID, rootPageId, "", ArchivePage, BreadCrumbDelimiter)
                    If breadCrumb <> "" Then
                        breadCrumb = cr & "<p class=""ccPageListNavigation"">" & BreadCrumbPrefix & " " & breadCrumb & "</p>"
                    End If
                End If
                result = result & breadCrumb
                '
                If (Not cpcore.doc.isPrintVersion) Then
                    Dim IconRow As String = ""
                    If (Not cpcore.authContext.visit.Bot) And (page.AllowPrinterVersion Or page.AllowEmailPage) Then
                        '
                        ' not a bot, and either print or email allowed
                        '
                        If page.AllowPrinterVersion Then
                            Dim QueryString As String = cpcore.doc.refreshQueryString
                            QueryString = genericController.ModifyQueryString(QueryString, rnPageId, genericController.encodeText(page.id), True)
                            QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, HardCodedPagePrinterVersion, True)
                            Dim Caption As String = cpcore.siteProperties.getText("PagePrinterVersionCaption", "Printer Version")
                            Caption = genericController.vbReplace(Caption, " ", "&nbsp;")
                            IconRow = IconRow & cr & "&nbsp;&nbsp;<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank""><img alt=""image"" src=""/ccLib/images/IconSmallPrinter.gif"" width=""13"" height=""13"" border=""0"" align=""absmiddle""></a>&nbsp<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?" & QueryString) & """ target=""_blank"" style=""text-decoration:none! important;font-family:sanserif,verdana,helvetica;font-size:11px;"">" & Caption & "</a>"
                        End If
                        If page.AllowEmailPage Then
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
                If cpcore.authContext.isQuickEditing(cpcore, pageContentModel.contentName) Then
                    Cell = Cell & getQuickEditing(rootPageId, OrderByClause, AllowChildList, AllowReturnLink, ArchivePage, page.ContactMemberID, page.ChildListSortMethodID, allowChildListComposite, ArchivePage)
                Else
                    '
                    ' ----- Headline
                    '
                    If page.Headline <> "" Then
                        Dim headline As String = cpcore.html.main_encodeHTML(page.Headline)
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
                        If cpcore.authContext.isEditing(pageContentModel.contentName) Then
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
                    If allowChildListComposite Or cpcore.authContext.isEditingAnything() Then
                        If Not allowChildListComposite Then
                            Cell = Cell & cpcore.html.html_GetAdminHintWrapper("Automatic Child List display is disabled for this page. It is displayed here because you are in editing mode. To enable automatic child list display, see the features tab for this page.")
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
                        & genericController.htmlIndent(getSeeAlso(cpcore, pageContentModel.contentName, page.id)) _
                        & cr & "</div>"
                End If
                '
                ' ----- Allow More Info
                If (page.ContactMemberID <> 0) And page.AllowMoreInfo Then
                    result = result & cr & "<ac TYPE=""" & ACTypeContact & """>"
                End If
                '
                ' ----- Feedback
                If (Not cpcore.doc.isPrintVersion) And (page.ContactMemberID <> 0) And page.AllowFeedback Then
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
        Public Function getSeeAlso(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer) As String
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
                        IsEditingLocal = Me.cpcore.authContext.isEditing(iContentName)
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
                                    SeeAlsoLink = Me.cpcore.webServer.requestProtocol & SeeAlsoLink
                                End If
                                If IsEditingLocal Then
                                    result = result & Me.cpcore.html.main_GetRecordEditLink2("See Also", (cpcore.db.cs_getInteger(CS, "ID")), False, "", Me.cpcore.authContext.isEditing("See Also"))
                                End If
                                result = result & "<a href=""" & genericController.encodeHTML(SeeAlsoLink) & """ target=""_blank"">" & (cpcore.db.cs_getText(CS, "Name")) & "</A>"
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
                            result = result & cr & "<li class=""ccListItem"">" & Me.cpcore.html.main_GetRecordAddLink("See Also", "RecordID=" & iRecordID & ",ContentID=" & ContentID) & "</LI>"
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
                Me.cpcore.handleException(ex)
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
                Me.cpcore.handleException(ex)
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
                            NoteCopy = NoteCopy & Me.cpcore.html.main_EncodeCRLF(Copy) & BR
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
                        result = result & "<p>Thank you. Your feedback was received.</p>"
                    Case Else
                        '
                        ' ----- print the feedback submit form
                        '
                        Panel = "<form Action=""" & Me.cpcore.webServer.serverFormActionURL & "?" & Me.cpcore.doc.refreshQueryString & """ Method=""post"">"
                        Panel = Panel & "<table border=""0"" cellpadding=""4"" cellspacing=""0"" width=""100%"">"
                        Panel = Panel & "<tr>"
                        Panel = Panel & "<td colspan=""2""><p>Your feedback is welcome</p></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Name
                        '
                        Copy = Me.cpcore.authContext.user.Name
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Name</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromName"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- From Email address
                        '
                        Copy = Me.cpcore.authContext.user.Email
                        Panel = Panel & "<td align=""right"" width=""100""><p>Your Email</p></td>"
                        Panel = Panel & "<td align=""left""><input type=""text"" name=""NoteFromEmail"" value=""" & genericController.encodeHTML(Copy) & """></span></td>"
                        Panel = Panel & "</tr><tr>"
                        '
                        ' ----- Message
                        '
                        Copy = ""
                        Panel = Panel & "<td align=""right"" width=""100"" valign=""top""><p>Feedback</p></td>"
                        Panel = Panel & "<td>" & Me.cpcore.html.html_GetFormInputText2("NoteCopy", Copy, 4, 40, "TextArea", False) & "</td>"
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
                Me.cpcore.handleException(ex)
            End Try
            Return result
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
                Me.cpcore.handleException(ex)
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
                Me.cpcore.handleException(ex)
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
                                result = result & genericController.csv_GetLinkedText("<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?rc=" & ContentID & "&ri=" & RecordID) & """>", LinkLabel)
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
                Me.cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        '
        '
        Public Function getMoreInfoHtml(cpCore As coreClass, ByVal PeopleID As Integer) As String
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
                Me.cpcore.handleException(ex)
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
                                result = result & "<a href=""http://" & Me.cpcore.webServer.requestDomain & requestAppRootPath & Me.cpcore.webServer.requestPage & "?rc=" & ContentID & "&ri=" & RecordID & """>" & LinkLabel & "</a>"
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call Me.cpcore.db.cs_goNext(CS)
                    Loop
                    If result <> "" Then
                        result = Me.cpcore.html.html_GetContentCopy("Watch List Caption: " & ListName, ListName, Me.cpcore.authContext.user.id, True, Me.cpcore.authContext.isAuthenticated) & cr & "<ul class=""ccWatchList"">" & htmlIndent(result) & cr & "</ul>"
                    End If
                End If
                Call Me.cpcore.db.cs_Close(CS)
                '
                If Me.cpcore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                    result = Me.cpcore.html.main_GetEditWrapper("Watch List [" & ListName & "]", result)
                End If
            Catch ex As Exception
                Me.cpcore.handleException(ex)
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
        Friend Function getQuickEditing(rootPageId As Integer, OrderByClause As String, AllowPageList As Boolean, AllowReturnLink As Boolean, ArchivePages As Boolean, contactMemberID As Integer, childListSortMethodId As Integer, main_AllowChildListComposite As Boolean, ArchivePage As Boolean) As String
            Dim result As String = String.Empty
            '
            Dim RootPageContentName As String = Models.Entity.pageContentModel.contentName
            Dim LiveRecordContentName As String = Models.Entity.pageContentModel.contentName
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
            Dim main_EditLockMemberName As String = String.Empty
            Dim main_EditLockDateExpires As Date
            Dim SubmittedDate As Date
            Dim ApprovedDate As Date
            Dim ModifiedDate As Date
            '
            Call cpcore.html.addStyleLink("/quickEditor/styles.css", "Quick Editor")
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
            Call getAuthoringStatus(LiveRecordContentName, page.id, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
            Call getAuthoringPermissions(LiveRecordContentName, page.id, AllowInsert, AllowCancel, allowSave, AllowDelete, AllowPublish, AllowAbort, AllowSubmit, AllowApprove, readOnlyField)
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
                ButtonList = cpcore.html.main_GetPanelButtons(ButtonList, "Button")
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
            & cr2 & "<td class=""qeRow qeRight"">" & cpcore.html.html_GetFormInputText2("name", page.name, 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & cr & "<tr>" _
            & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Headline</td>" _
            & cr2 & "<td class=""qeRow qeRight"">" & cpcore.html.html_GetFormInputText2("headline", page.Headline, 1, , , , readOnlyField) & "</td>" _
            & cr & "</tr>" _
            & ""
            If readOnlyField Then
                result = result & "" _
                    & cr & "<tr>" _
                    & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:34px;"">Body</td>" _
                    & cr2 & "<td class=""qeRow qeRight"">" & getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
                    & cr & "</tr>"
            Else
                result = result & "" _
                    & cr & "<tr>" _
                    & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:111px;"">Body</td>" _
                    & cr2 & "<td class=""qeRow qeRight"">" & getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
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
            result = cpcore.html.main_GetPanel(result)

            '
            ' Form Wrapper
            '
            result = "" _
            & cr & cpcore.html.html_GetUploadFormStart(cpcore.webServer.requestQueryString) _
            & cr & cpcore.html.html_GetFormInputHidden("Type", FormTypePageAuthoring) _
            & cr & cpcore.html.html_GetFormInputHidden("ID", page.id) _
            & cr & cpcore.html.html_GetFormInputHidden("ContentName", LiveRecordContentName) _
            & cr & cpcore.html.main_GetPanelHeader("Contensive Quick Editor") _
            & cr & result _
            & cr & cpcore.html.html_GetUploadFormEnd()

            result = "" _
            & cr & "<div class=""ccCon"">" _
            & genericController.htmlIndent(result) _
            & cr & "</div>"
            Return result
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Friend Function getQuickEditingBody(ByVal ContentName As String, ByVal OrderByClause As String, ByVal AllowChildList As Boolean, ByVal Authoring As Boolean, ByVal rootPageId As Integer, ByVal readOnlyField As Boolean, ByVal AllowReturnLink As Boolean, ByVal RootPageContentName As String, ByVal ArchivePage As Boolean, ByVal contactMemberID As Integer) As String
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
            quickEditCopy = pageCopy
            Return html_quickEdit_fpo
        End Function
        '
        '=============================================================================
        '
        Friend Function getReturnBreadcrumb(RootPageContentName As String, ignore As Integer, rootPageId As Integer, ParentIDPath As String, ArchivePage As Boolean, BreadCrumbDelimiter As String) As String
            Dim returnHtml As String = ""
            '
            For Each testpage As pageContentModel In pageToRootList
                Dim pageCaption As String = testpage.MenuHeadline
                If pageCaption = "" Then
                    pageCaption = genericController.encodeText(testpage.name)
                End If
                If returnHtml = "" Then
                    returnHtml = pageCaption
                Else
                    returnHtml = "<a href=""" & genericController.encodeHTML(getPageLink(testpage.id, "", True, False)) & """>" & pageCaption & "</a>" & BreadCrumbDelimiter & returnHtml
                End If
            Next
            Return returnHtml
        End Function
        '
        '
        '
        Friend Function getTableRow(ByVal Caption As String, ByVal Result As String, ByVal EvenRow As Boolean) As String
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
        '=============================================================================
        '   Add content padding around content
        '       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
        '=============================================================================
        '
        Friend Function getContentBoxWrapper(ByVal Content As String, ByVal ContentPadding As Integer) As String
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
        '========================================================================
        ' ----- Process the reply from the Authoring Tools Panel form
        '========================================================================
        '
        Public Sub processFormQuickEditing()
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
            Dim RecordName As String = String.Empty
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
                Call getAuthoringStatus(pageContentModel.contentName, RecordID, IsSubmitted, IsApproved, SubmittedMemberName, ApprovedMemberName, IsInserted, IsDeleted, IsModified, ModifiedMemberName, ModifiedDate, SubmittedDate, ApprovedDate)
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
                            Copy = cpcore.html.decodeContent(Copy)
                            If Copy <> cpcore.db.cs_get(CSBlock, "copyFilename") Then
                                Call cpcore.db.cs_set(CSBlock, "copyFilename", Copy)
                                SaveButNoChanges = False
                            End If
                            RecordName = cpcore.docProperties.getText("name")
                            If RecordName <> cpcore.db.cs_get(CSBlock, "name") Then
                                Call cpcore.db.cs_set(CSBlock, "name", RecordName)
                                SaveButNoChanges = False
                            End If
                            Call docController.app_addLinkAlias2(cpcore, RecordName, RecordID, "")
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
                        Link = getPageLink(RecordID, "", True, False)
                        'Link = main_GetPageLink(RecordID)
                        If main_WorkflowSupport Then
                            If Not cpcore.authContext.isWorkflowRendering() Then
                                Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                                Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
                            End If
                        End If
                        Call cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", False)
                    End If
                    Call cpcore.db.cs_Close(CSBlock)
                    '
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
                            Link = getPageLink(RecordID, "", True, False)
                            'Link = main_GetPageLink(RecordID)
                            If main_WorkflowSupport Then
                                If Not cpcore.authContext.isWorkflowRendering() Then
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
                    Call deleteChildRecords(pageContentModel.contentName, RecordID, False)
                    Call cpcore.db.deleteContentRecord(pageContentModel.contentName, RecordID)
                    '
                    If Not main_WorkflowSupport Then
                        Call cpcore.cache.invalidateContent(pageContentModel.contentName)
                    End If
                    '
                    If Not main_WorkflowSupport Then
                        Link = getPageLink(ParentID, "", True, False)
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", True)
                        Call cpcore.webServer.redirect(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", redirectBecausePageNotFound)
                        Exit Sub
                    End If
                End If
                '
                If (Button = ButtonAbortEdit) Then
                    Call cpcore.workflow.abortEdit2(pageContentModel.contentName, RecordID, cpcore.authContext.user.id)
                End If
                If (Button = ButtonPublishSubmit) Then
                    Call cpcore.workflow.main_SubmitEdit(pageContentModel.contentName, RecordID)
                    Call sendPublishSubmitNotice(pageContentModel.contentName, RecordID, "")
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
        '           - uses ChildPageListTracking to track what has been seen
        '=============================================================================
        '
        Public Function getChildPageList(ByVal RequestedListName As String, ByVal ContentName As String, ByVal parentPageID As Integer, ByVal allowChildListDisplay As Boolean, Optional ByVal ArchivePages As Boolean = False) As String
            Dim result As String = ""
            Try
                Dim ChildContent As String
                Dim Brief As String
                Dim UcaseRequestedListName As String
                Dim ChildListCount As Integer
                Dim AddLink As String
                Dim BlockContentComposite As Boolean
                Dim Link As String
                Dim LinkedText As String
                Dim ActiveList As String = ""
                Dim InactiveList As String = String.Empty
                Dim archiveLink As String
                Dim PageLink As String
                Dim pageMenuHeadline As String
                Dim pageEditLink As String
                Dim isAuthoring = cpcore.authContext.isEditing(pageContentModel.contentName)
                '
                ChildListCount = 0
                UcaseRequestedListName = genericController.vbUCase(RequestedListName)
                If (UcaseRequestedListName = "NONE") Or (UcaseRequestedListName = "ORPHAN") Then
                    UcaseRequestedListName = ""
                End If
                '
                archiveLink = cpcore.webServer.requestPathPage
                archiveLink = genericController.ConvertLinkToShortLink(archiveLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath)
                archiveLink = genericController.EncodeAppRootPath(archiveLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
                '
                Dim sqlCriteria As String = "(parentId=" & page.id & ")"
                Dim sqlOrderBy As String = "sortOrder"
                Dim childPageList As List(Of pageContentModel) = pageContentModel.createList(cpcore, sqlCriteria, sqlOrderBy)
                For Each childPage As pageContentModel In childPageList
                    PageLink = getPageLink(childPage.id, "", True, False)
                    pageMenuHeadline = childPage.MenuHeadline
                    If pageMenuHeadline = "" Then
                        pageMenuHeadline = Trim(childPage.name)
                        If pageMenuHeadline = "" Then
                            pageMenuHeadline = "Related Page"
                        End If
                    End If
                    pageEditLink = ""
                    If cpcore.authContext.isEditing(ContentName) Then
                        pageEditLink = cpcore.html.main_GetRecordEditLink2(ContentName, childPage.id, True, childPage.name, True)
                    End If
                    '
                    If ArchivePages Then
                        Link = genericController.modifyLinkQuery(archiveLink, rnPageId, CStr(childPage.id), True)
                    Else
                        Link = PageLink
                    End If
                    If childPage.BlockContent Or childPage.BlockPage Then
                        BlockContentComposite = Not bypassContentBlock(childPage.ContentControlID, childPage.id)
                    Else
                        BlockContentComposite = False
                    End If
                    LinkedText = genericController.csv_GetLinkedText("<a href=""" & genericController.encodeHTML(Link) & """>", pageMenuHeadline)
                    If (UcaseRequestedListName = "") And (childPage.ParentListName <> "") And (Not isAuthoring) Then
                        '
                        ' ----- Requested orphan list, and this record is in a named list, and not editing, do not display
                        '
                    ElseIf (UcaseRequestedListName = "") And (childPage.ParentListName <> "") Then
                        '
                        ' ----- Requested orphan list, and this record is in a named list, but authoring, list it
                        '
                        If isAuthoring Then
                            InactiveList = InactiveList & cr & "<li name=""page" & childPage.id & """ name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            InactiveList = InactiveList & pageEditLink
                            InactiveList = InactiveList & "[from Child Page List '" & childPage.ParentListName & "': " & LinkedText & "]"
                            InactiveList = InactiveList & "</li>"
                        End If
                    ElseIf (UcaseRequestedListName = "") And (Not allowChildListDisplay) And (Not isAuthoring) Then
                        '
                        ' ----- Requested orphan List, Not AllowChildListDisplay, not Authoring, do not display
                        '
                    ElseIf (UcaseRequestedListName <> "") And (UcaseRequestedListName <> genericController.vbUCase(childPage.ParentListName)) Then
                        '
                        ' ----- requested named list and wrong RequestedListName, do not display
                        '
                    ElseIf (Not childPage.AllowInChildLists) Then
                        '
                        ' ----- Allow in Child Page Lists is false, display hint to authors
                        '
                        If isAuthoring Then
                            InactiveList = InactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            InactiveList = InactiveList & pageEditLink
                            InactiveList = InactiveList & "[Hidden (Allow in Child Lists is not checked): " & LinkedText & "]"
                            InactiveList = InactiveList & "</li>"
                        End If
                    ElseIf Not childPage.Active Then
                        '
                        ' ----- Not active record, display hint if authoring
                        '
                        If isAuthoring Then
                            InactiveList = InactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            InactiveList = InactiveList & pageEditLink
                            InactiveList = InactiveList & "[Hidden (Inactive): " & LinkedText & "]"
                            InactiveList = InactiveList & "</li>"
                        End If
                    ElseIf (childPage.PubDate <> Date.MinValue) And (childPage.PubDate > cpcore.app_startTime) Then
                        '
                        ' ----- Child page has not been published
                        '
                        If isAuthoring Then
                            InactiveList = InactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            InactiveList = InactiveList & pageEditLink
                            InactiveList = InactiveList & "[Hidden (To be published " & childPage.PubDate & "): " & LinkedText & "]"
                            InactiveList = InactiveList & "</li>"
                        End If
                    ElseIf (childPage.DateExpires <> Date.MinValue) And (childPage.DateExpires < cpcore.app_startTime) Then
                        '
                        ' ----- Child page has expired
                        '
                        If isAuthoring Then
                            InactiveList = InactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            InactiveList = InactiveList & pageEditLink
                            InactiveList = InactiveList & "[Hidden (Expired " & childPage.DateExpires & "): " & LinkedText & "]"
                            InactiveList = InactiveList & "</li>"
                        End If
                    Else
                        '
                        ' ----- display list (and authoring links)
                        '
                        ActiveList = ActiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                        If pageEditLink <> "" Then
                            ActiveList = ActiveList & pageEditLink & "&nbsp;"
                        End If
                        ActiveList = ActiveList & LinkedText
                        '
                        ' include authoring mark for content block
                        '
                        If isAuthoring Then
                            If childPage.BlockContent Then
                                ActiveList = ActiveList & "&nbsp;[Content Blocked]"
                            End If
                            If childPage.BlockPage Then
                                ActiveList = ActiveList & "&nbsp;[Page Blocked]"
                            End If
                        End If
                        '
                        ' include overview
                        ' if AllowBrief is false, BriefFilename is not loaded
                        '
                        If (childPage.BriefFilename <> "") And (childPage.AllowBrief) Then
                            Brief = Trim(cpcore.cdnFiles.readFile(childPage.BriefFilename))
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
                    AddLink = cpcore.html.main_GetRecordAddLink(ChildContent, "parentid=" & parentPageID & ",ParentListName=" & UcaseRequestedListName, True)
                    If AddLink <> "" Then
                        InactiveList = InactiveList & cr & "<li class=""ccListItem"">" & AddLink & "</LI>"
                    End If
                End If
                '
                ' ----- If there is a list, add the list start and list end
                '
                result = ""
                If ActiveList <> "" Then
                    result = result & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildList"">" & genericController.htmlIndent(ActiveList) & cr & "</ul>"
                End If
                If InactiveList <> "" Then
                    result = result & cr & "<ul id=""childPageList" & parentPageID & "_" & RequestedListName & """ class=""ccChildListInactive"">" & genericController.htmlIndent(InactiveList) & cr & "</ul>"
                End If
                '
                ' ----- if non-orphan list, authoring and none found, print none message
                '
                If (UcaseRequestedListName <> "") And (ChildListCount = 0) And isAuthoring Then
                    result = "[Child Page List with no pages]</p><p>" & result
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '=============================================================================
        '   BypassContentBlock
        '       Is This member allowed through the content block
        '=============================================================================
        '
        Public Function bypassContentBlock(ByVal ContentID As Integer, ByVal RecordID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00374")
            '
            Dim CS As Integer
            Dim SQL As String
            '
            If cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                bypassContentBlock = True
            ElseIf cpcore.authContext.isAuthenticatedContentManager(cpcore, cpcore.metaData.getContentNameByID(ContentID)) Then
                bypassContentBlock = True
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
                bypassContentBlock = cpcore.db.cs_ok(CS)
                Call cpcore.db.cs_Close(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("IsContentBlocked")
        End Function
        '
        '====================================================================================================
        '   main_GetTemplateLink
        '       Added to externals (aoDynamicMenu) can main_Get hard template links
        '====================================================================================================
        '
        Public Function getTemplateLink(templateId As Integer) As String
            Dim template As pageTemplateModel = pageTemplateModel.create(cpcore, templateId, New List(Of String))
            If template IsNot Nothing Then
                Return template.Link
            End If
            Return ""
        End Function
        '
        '========================================================================
        ' main_DeleteChildRecords
        '========================================================================
        '
        Public Function deleteChildRecords(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal ReturnListWithoutDelete As Boolean = False) As String
            Dim result As String = String.Empty
            Try
                Dim QuickEditing As Boolean
                Dim IDs() As String
                Dim IDCnt As Integer
                Dim Ptr As Integer
                Dim CS As Integer
                Dim ChildList As String
                Dim SingleEntry As Boolean
                '
                ' For now, the child delete only works in non-workflow
                '
                CS = cpcore.db.cs_open(ContentName, "parentid=" & RecordID, , , , ,, "ID")
                Do While cpcore.db.cs_ok(CS)
                    result = result & "," & cpcore.db.cs_getInteger(CS, "ID")
                    cpcore.db.cs_goNext(CS)
                Loop
                Call cpcore.db.cs_Close(CS)
                If result <> "" Then
                    result = Mid(result, 2)
                    '
                    ' main_Get a list of all pages, but do not delete anything yet
                    '
                    IDs = Split(result, ",")
                    IDCnt = UBound(IDs) + 1
                    SingleEntry = (IDCnt = 1)
                    For Ptr = 0 To IDCnt - 1
                        ChildList = deleteChildRecords(ContentName, genericController.EncodeInteger(IDs(Ptr)), True)
                        If ChildList <> "" Then
                            result = result & "," & ChildList
                            SingleEntry = False
                        End If
                    Next
                    If Not ReturnListWithoutDelete Then
                        '
                        ' Do the actual delete
                        '
                        IDs = Split(result, ",")
                        IDCnt = UBound(IDs) + 1
                        SingleEntry = (IDCnt = 1)
                        QuickEditing = cpcore.authContext.isQuickEditing(cpcore, "page content")
                        For Ptr = 0 To IDCnt - 1
                            Call cpcore.db.deleteContentRecord("page content", genericController.EncodeInteger(IDs(Ptr)))
                        Next
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Sub getAuthoringStatus(ByVal ContentName As String, ByVal RecordID As Integer, ByRef IsSubmitted As Boolean, ByRef IsApproved As Boolean, ByRef SubmittedName As String, ByRef ApprovedName As String, ByRef IsInserted As Boolean, ByRef IsDeleted As Boolean, ByRef IsModified As Boolean, ByRef ModifiedName As String, ByRef ModifiedDate As Date, ByRef SubmittedDate As Date, ByRef ApprovedDate As Date)
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
        Public Sub getAuthoringPermissions(ByVal ContentName As String, ByVal RecordID As Integer, ByRef AllowInsert As Boolean, ByRef AllowCancel As Boolean, ByRef allowSave As Boolean, ByRef AllowDelete As Boolean, ByRef AllowPublish As Boolean, ByRef AllowAbort As Boolean, ByRef AllowSubmit As Boolean, ByRef AllowApprove As Boolean, ByRef readOnlyField As Boolean)
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
            Dim SubmittedName As String = String.Empty
            Dim ApprovedName As String = String.Empty
            Dim ModifiedName As String = String.Empty
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
        Public Sub sendPublishSubmitNotice(ByVal ContentName As String, ByVal RecordID As Integer, ByVal RecordName As String)
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
            Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=""" & genericController.encodeHTML(Link) & """>" & cpcore.webServer.requestDomain & "</a>")
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
        Friend Function getDefaultBlockMessage(UseContentWatchLink As Boolean) As String
            getDefaultBlockMessage = ""
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetDefaultBlockMessage")
            '
            Dim CS As Integer
            Dim PageID As Integer
            '
            CS = cpcore.db.cs_open("Copy Content", "name=" & cpcore.db.encodeSQLText(ContentBlockCopyName), "ID", , , , , "Copy,ID")
            If cpcore.db.cs_ok(CS) Then
                getDefaultBlockMessage = cpcore.db.cs_get(CS, "Copy")
            End If
            Call cpcore.db.cs_Close(CS)
            '
            ' ----- Do not allow blank message - if still nothing, create default
            '
            If getDefaultBlockMessage = "" Then
                getDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=""?method=login"" rel=""nofollow"">Click Here</a>. For more information, please contact the administrator.</p>"
            End If
            '
            ' ----- Create Copy Content Record for future
            '
            CS = cpcore.db.cs_insertRecord("Copy Content")
            If cpcore.db.cs_ok(CS) Then
                Call cpcore.db.cs_set(CS, "Name", ContentBlockCopyName)
                Call cpcore.db.cs_set(CS, "Copy", getDefaultBlockMessage)
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
        Friend Function loadFormPageInstructions(FormInstructions As String, Formhtml As String) As main_FormPagetype
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
        Friend Function getFormPage(FormPageName As String, GroupIDToJoinOnSuccess As Integer) As String
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
            f = loadFormPageInstructions(FormInstructions, Formhtml)
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
                                Body = genericController.vbReplace(Body, "{{FIELD}}", cpcore.html.html_GetFormInputCS(CSPeople, "People", .PeopleField), 1, 99, vbTextCompare)
                                RepeatBody = RepeatBody & Body
                                HasRequiredFields = HasRequiredFields Or .REquired
                            End If
                        Case 2
                            '
                            ' Group main_MemberShip
                            '
                            GroupValue = cpcore.authContext.IsMemberOfGroup2(cpcore, .GroupName)
                            Body = f.RepeatCell
                            Body = genericController.vbReplace(Body, "{{CAPTION}}", cpcore.html.html_GetFormInputCheckBox2("Group" & .GroupName, GroupValue), 1, 99, vbTextCompare)
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
            getFormPage = "" _
            & errorController.error_GetUserError(cpcore) _
            & cpcore.html.html_GetUploadFormStart() _
            & cpcore.html.html_GetFormInputHidden("ContensiveFormPageID", FormPageID) _
            & cpcore.html.html_GetFormInputHidden("SuccessID", cpcore.security.encodeToken(GroupIDToJoinOnSuccess, cpcore.app_startTime)) _
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
        '
        '====================================================================================================
        '
        Public Function getPageLink(ByVal PageID As Integer, ByVal QueryStringSuffix As String, Optional ByVal AllowLinkAliasIfEnabled As Boolean = True, Optional ByVal UseContentWatchNotDefaultPage As Boolean = False) As String
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
            main_GetPageLink3 = getPageLink(PageID, QueryStringSuffix, AllowLinkAlias, False)
        End Function
        ''
        'Public Function getPageLink2(ByVal PageID As Integer, ByVal QueryStringSuffix As String) As String
        '    getPageLink2 = getPageLink4(PageID, QueryStringSuffix, True, False)
        '    'main_GetPageLink2 = main_GetPageLink3(PageID, QueryStringSuffix, True)
        'End Function
        ''
        'Public Function main_GetPageLink(ByVal PageID As Integer) As String
        '    main_GetPageLink = getPageLink4(PageID, "", True, False)
        '    'main_GetPageLink = main_GetPageLink3(PageID, "", True)
        'End Function
        '
        '
        '
        Friend Function getLandingLink() As String
            If _landingLink = "" Then
                _landingLink = cpcore.siteProperties.getText("SectionLandingLink", requestAppRootPath & cpcore.siteProperties.serverPageDefault)
                _landingLink = genericController.ConvertLinkToShortLink(_landingLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath)
                _landingLink = genericController.EncodeAppRootPath(_landingLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            End If
            getLandingLink = _landingLink
        End Function
        Private Property _landingLink As String = ""                              ' Default Landing page - managed through main_GetLandingLink()        '

        ''
        ''
        'Public Function getStyleTagPublic() As String
        '    Dim StyleSN As Integer
        '    '
        '    getStyleTagPublic = ""
        '    'If cpcore.siteProperties.getBoolean("Allow CSS Reset") Then
        '    '    getStyleTagPublic = getStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.webServerIO_requestDomain & "/ccLib/styles/ccreset.css"" >"
        '    'End If
        '    StyleSN = genericController.EncodeInteger(cpcore.siteProperties.getText("StylesheetSerialNumber", "0"))
        '    If StyleSN < 0 Then
        '        '
        '        ' Linked Styles
        '        ' Bump the Style Serial Number so next fetch is not cached
        '        '
        '        StyleSN = 1
        '        Call cpcore.siteProperties.setProperty("StylesheetSerialNumber", CStr(StyleSN))
        '        '
        '        ' Save new public stylesheet
        '        '
        '        'Dim kmafs As New fileSystemClass
        '        'Call cpcore.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), cpcore.html.html_getStyleSheet2(0, 0))
        '        'Call cpcore.cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css"), cpcore.html.getStyleSheetDefault)

        '    End If
        '    If (StyleSN = 0) Then
        '        '
        '        ' Put styles inline if requested, and if there has been an upgrade
        '        '
        '        'getStyleTagPublic = getStyleTagPublic & cr & StyleSheetStart & cpcore.html.html_getStyleSheet2(0, 0) & cr & StyleSheetEnd
        '    ElseIf (cpcore.siteProperties.dataBuildVersion <> cpcore.codeVersion()) Then
        '        '
        '        ' Put styles inline if requested, and if there has been an upgrade
        '        '
        '        'getStyleTagPublic = getStyleTagPublic & cr & "<!-- styles forced inline because database upgrade needed -->" & StyleSheetStart & cpcore.html.html_getStyleSheet2(0, 0) & cr & StyleSheetEnd
        '    Else
        '        '
        '        ' cached stylesheet
        '        '
        '        getStyleTagPublic = getStyleTagPublic & cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.webServerIO_requestDomain & genericController.getCdnFileLink(cpcore, "templates/Public" & StyleSN & ".css") & """ >"
        '    End If
        'End Function
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
                cpcore.handleException(ex) : Throw
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
            Dim result As Boolean = False
            Try
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
                    result = (ParentRecordID = ChildRecordParentID)
                    If Not result Then
                        result = main_IsChildRecord_Recurse(DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History & "," & CStr(ChildRecordID))
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
        Friend Function main_ProcessPageNotFound_GetLink(ByVal adminMessage As String, Optional ByVal BackupPageNotFoundLink As String = "", Optional ByVal PageNotFoundLink As String = "", Optional ByVal EditPageID As Integer = 0, Optional ByVal EditSectionID As Integer = 0) As String
            Dim result As String = String.Empty
            Try
                Dim Pos As Integer
                Dim DefaultLink As String
                Dim PageNotFoundPageID As Integer
                Dim Link As String
                '
                PageNotFoundPageID = main_GetPageNotFoundPageId()
                If PageNotFoundPageID = 0 Then
                    '
                    ' No PageNotFound was set -- use the backup link
                    '
                    If BackupPageNotFoundLink = "" Then
                        adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used."
                        Link = getLandingLink()
                    Else
                        adminMessage = adminMessage & " The Site Property 'PageNotFoundPageID' is not set."
                        Link = BackupPageNotFoundLink
                    End If
                Else
                    '
                    ' Set link
                    '
                    Link = getPageLink(PageNotFoundPageID, "", True, False)
                    DefaultLink = getPageLink(0, "", True, False)
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
        '---------------------------------------------------------------------------
        '
        '---------------------------------------------------------------------------
        '
        Public Function getLandingPageID() As Integer
            Dim landingPageid As Integer = 0
            Try
                If (domain Is Nothing) Then
                    '
                    ' -- domain not available
                    cpcore.handleException(New ApplicationException("Landing page could not be determined because the domain was not recognized."))
                Else
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
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return landingPageid
        End Function
        '
        ' Verify a link from the template link field to be used as a Template Link
        '
        Friend Function verifyTemplateLink(ByVal linkSrc As String) As String
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
            getContentWatchLinkByKey = genericController.EncodeAppRootPath(getContentWatchLinkByKey, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
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
                cpcore.handleException(ex) : Throw
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
            Dim MenuLinkOverRide As String = String.Empty
            '
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
                resultLink = genericController.EncodeAppRootPath(resultLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
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
                        & vbCrLf & "<tr><td colspan=2>&nbsp;<br>" & cpcore.html.main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>" _
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
                cpcore.handleException(ex)
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
            Dim AllowLinkAlias As Boolean
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
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function getNextStyleFilenames() As String
            Dim result As String = ""
            Dim Ptr As Integer
            If styleFilenames_Cnt >= 0 Then
                For Ptr = 0 To styleFilenames_Cnt - 1
                    If styleFilenames(Ptr) <> "" Then
                        result = styleFilenames(Ptr)
                        styleFilenames(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            Return result
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function getJavascriptOnLoad() As String
            Dim result As String = ""
            Dim Ptr As Integer
            If javascriptOnLoad.Count >= 0 Then
                For Ptr = 0 To javascriptOnLoad.Count - 1
                    If javascriptOnLoad(Ptr) <> "" Then
                        result = javascriptOnLoad(Ptr)
                        javascriptOnLoad(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            Return result
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function getNextJavascriptBodyEnd() As String
            Dim result As String = ""
            Dim Ptr As Integer
            If javascriptBodyEnd.Count >= 0 Then
                For Ptr = 0 To javascriptBodyEnd.Count - 1
                    If javascriptBodyEnd(Ptr) <> "" Then
                        result = javascriptBodyEnd(Ptr)
                        javascriptBodyEnd(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            Return result
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function getNextJSFilename() As String
            Dim result As String = ""
            Dim Ptr As Integer
            If javascriptReferenceFilename_Cnt >= 0 Then
                For Ptr = 0 To javascriptReferenceFilename_Cnt - 1
                    If javascriptReferenceFilename(Ptr) <> "" Then
                        result = javascriptReferenceFilename(Ptr)
                        javascriptReferenceFilename(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            Return result
        End Function
        '
        '
        '
        Public Sub addRefreshQueryString(ByVal Name As String, Optional ByVal Value As String = "")
            Try
                Dim temp As String()
                '
                If (InStr(1, Name, "=") > 0) Then
                    temp = Split(Name, "=")
                    refreshQueryString = genericController.ModifyQueryString(cpcore.doc.refreshQueryString, temp(0), temp(1), True)
                Else
                    refreshQueryString = genericController.ModifyQueryString(cpcore.doc.refreshQueryString, Name, Value, True)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try

        End Sub
        '
        '========================================================================================================
        '   Add a block on html to the head
        '       if this is called from cpCoreClass activeContent
        '       probably should find a better place in cpCoreClass to pick it up
        '       or screw it and maybe everything will migrate to one class anyway
        '       this was added to let contentCmdClass in aoPrimitives import an html file
        '       all the others (javascript, css, etc) may be added later if this works
        '========================================================================================================
        '
        Public Sub addHeadTags(headTags As String)
            Me.headTags &= vbCrLf & vbTab & headTags
        End Sub
    End Class
End Namespace
