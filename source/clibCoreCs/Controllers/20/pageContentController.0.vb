
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
                'domain = Models.Entity.domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
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
                    cpcore.doc.pageToRootList = New List(Of Models.Entity.pageContentModel)()
                    Dim usedPageIdList As New List(Of Integer)()
                    Dim targetPageId = requestedPage.id
                    usedPageIdList.Add(0)
                    Do While (Not usedPageIdList.Contains(targetPageId))
                        usedPageIdList.Add(targetPageId)
                        Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpcore, targetPageId, New List(Of String))
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
                    For Each page As Models.Entity.pageContentModel In cpcore.doc.pageToRootList
                        If page.TemplateID > 0 Then
                            cpcore.doc.template = Models.Entity.pageTemplateModel.create(cpcore, page.TemplateID, New List(Of String))
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
                                cpcore.doc.template = Models.Entity.pageTemplateModel.create(cpcore, domain.DefaultTemplateId, New List(Of String))
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
                            cpcore.doc.template = Models.Entity.pageTemplateModel.createByName(cpcore, defaultTemplateName, New List(Of String))
                            If (cpcore.doc.template Is Nothing) Then
                                '
                                ' -- ceate new template named Default
                                cpcore.doc.template = Models.Entity.pageTemplateModel.add(cpcore, New List(Of String))
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
            Dim landingPage As Models.Entity.pageContentModel = Nothing
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
                Dim allowTemplateRuleList As List(Of Models.Entity.TemplateDomainRuleModel) = Models.Entity.TemplateDomainRuleModel.createList(cpcore, SqlCriteria)
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