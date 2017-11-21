
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
        ''' Persistence for the doc. Maintain all the parts and output the results. Constructor initializes the object. Call initDoc() to setup pages
        ''' </summary>
        ' -- not sure if this is the best plan, buts lets try this and see if we can get out of it later (to make this an addon) 
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
        'Public Property isStreamWritten As Boolean = False       ' true when anything has been writeAltBuffered.
        Public Property outputBufferEnabled As Boolean = True          ' when true (default), stream is buffered until page is done
        ' Public Property docBuffer As String = ""                   ' if any method calls writeAltBuffer, string concatinates here. If this is not empty at exit, it is used instead of returned string
        Public Property htmlMetaContent_Title As String = ""
        Public Property htmlMetaContent_Description As String = ""
        Public Property htmlMetaContent_OtherHeadTags As String = ""
        Public Property htmlMetaContent_KeyWordList As String = ""
        Public Property htmlMetaContent_StyleSheetTags As String = ""
        'Public Property metaContent_TemplateStyleSheetTag As String = ""
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
        Public Property scriptList_body As New List(Of scriptAssetClass)
        Public Property scriptList_head As scriptAssetClass() = {}
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
        Public Property bodyContent As String = ""                      ' stored here so cp.doc.content valid during bodyEnd event
        Public Property landingPageID As Integer = 0
        Public Property redirectLink As String = ""
        Public Property redirectReason As String = ""
        Public Property redirectBecausePageNotFound As Boolean = False
        '
        ' -- set true if any addon executed is set  htmlDocument=true. When true, the initial addon executed is returned in the html wrapper (html with head)
        Public Property htmlDocument As Boolean = False
        ''
        '' -- addon call depth. When an addon is called, it saves the value interanlly and increments. When level0 exits and htmlDocument is true, the output is wrapped with an html doc
        'Public Property addonDepth As Integer = 0
        Friend Property errList As List(Of String)                                   ' exceptions collected during document construction
        Public Property errorCount As Integer = 0
        Friend Property userErrorList As List(Of String)                           ' user messages
        Public Property debug_iUserError As String = ""                              ' User Error String
        Public Property trapLogMessage As String = ""                           ' The content of the current traplog (keep for popups if no Csv)
        Public Property testPointMessage As String = ""                         '
        Public Property testPointPrinting As Boolean = False                         ' if true, send main_TestPoint messages to the stream
        Public Property authContext As Models.Context.authContextModel
        Friend Property appStopWatch As Stopwatch = Stopwatch.StartNew()
        Public Property profileStartTime As Date                                        ' set in constructor
        Public Property profileStartTickCount As Integer = 0
        Public Property allowDebugLog As Boolean = False                       ' turn on in script -- use to write /debug.log in content files for whatever is needed
        Public Property blockExceptionReporting As Boolean = False                   ' used so error reporting can not call itself
        'Public Property pageErrorWithoutCsv As Boolean = False                  ' if true, the error occurred before Csv was available and main_TrapLogMessage needs to be saved and popedup
        'Public Property closePageCounter As Integer = 0
        Public Property continueProcessing As Boolean = False                                   ' when false, routines should not add to the output and immediately exit
        Public Property upgradeInProgress() As Boolean
        Public Property docGuid As String                        ' Random number (semi) unique to this hit
        Friend Property addonIdListRunInThisDoc As New List(Of Integer)
        Friend Property addonsCurrentlyRunningIdList As New List(Of Integer)
        Public Property pageAddonCnt As Integer = 0
        '
        ' -- persistant store for cdef complex model
        Friend Property cdefDictionary As Dictionary(Of String, Models.Complex.cdefModel)
        '
        ' -- persistant store for tableSchema complex mode
        Friend Property tableSchemaDictionary As Dictionary(Of String, Models.Complex.tableSchemaModel)
        '
        '====================================================================================================
        ' -- lookup contentId by contentName
        Friend ReadOnly Property contentNameIdDictionary As Dictionary(Of String, Integer)
            Get
                If (_contentNameIdDictionary Is Nothing) Then
                    _contentNameIdDictionary = New Dictionary(Of String, Integer)
                    For Each kvp As KeyValuePair(Of Integer, contentModel) In contentIdDict
                        Dim key As String = kvp.Value.name.Trim().ToLower()
                        If Not String.IsNullOrEmpty(key) Then
                            If (Not _contentNameIdDictionary.ContainsKey(key)) Then
                                _contentNameIdDictionary.Add(key, kvp.Value.id)
                            End If
                        End If
                    Next
                End If
                Return _contentNameIdDictionary
            End Get
        End Property
        Friend Sub contentNameIdDictionaryClear()
            _contentNameIdDictionary = Nothing
        End Sub
        Private _contentNameIdDictionary As Dictionary(Of String, Integer) = Nothing
        '
        '====================================================================================================
        ' -- lookup contentModel by contentId
        Friend ReadOnly Property contentIdDict As Dictionary(Of Integer, contentModel)
            Get
                If (_contentIdDict Is Nothing) Then
                    _contentIdDict = contentModel.createDict(cpcore, New List(Of String))
                End If
                Return _contentIdDict
            End Get
        End Property
        Friend Sub contentIdDictClear()
            _contentIdDict = Nothing
        End Sub
        Private _contentIdDict As Dictionary(Of Integer, contentModel) = Nothing
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
            cdefDictionary = New Dictionary(Of String, Models.Complex.cdefModel)
            tableSchemaDictionary = Nothing
        End Sub
        '
        Public Sub loadPage(pageId As Integer, Optional domainName As String = "")
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
                            template.BodyHTML = cpcore.appRootFiles.readFile(defaultTemplateHomeFilename)
                            template.save(cpcore)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
        End Sub
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
                cpCore.handleException(ex)
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
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & Me.cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) & "))" _
                    & ")" _
                & " ORDER BY " & iSortFieldList & ";"
                result = Me.cpcore.db.csOpenSql(SQL, , PageSize, PageNumber)
                If Not Me.cpcore.db.csOk(result) Then
                    '
                    ' Check if listname exists
                    '
                    CS = Me.cpcore.db.csOpen("Content Watch Lists", "name=" & Me.cpcore.db.encodeSQLText(ListName), "ID", , , , , "ID")
                    If Not Me.cpcore.db.csOk(CS) Then
                        Call Me.cpcore.db.csClose(CS)
                        CS = Me.cpcore.db.csInsertRecord("Content Watch Lists")
                        If Me.cpcore.db.csOk(CS) Then
                            Call Me.cpcore.db.csSet(CS, "name", ListName)
                        End If
                    End If
                    Call Me.cpcore.db.csClose(CS)
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
                If Me.cpcore.db.csOk(CSPointer) Then
                    ContentID = Models.Complex.cdefModel.getContentId(cpcore, "Content Watch")
                    Do While Me.cpcore.db.csOk(CSPointer)
                        Link = Me.cpcore.db.csGetText(CSPointer, "link")
                        LinkLabel = Me.cpcore.db.csGetText(CSPointer, "LinkLabel")
                        RecordID = Me.cpcore.db.csGetInteger(CSPointer, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & genericController.csv_GetLinkedText("<a href=""" & genericController.encodeHTML(cpcore.webServer.requestPage & "?rc=" & ContentID & "&ri=" & RecordID) & """>", LinkLabel)
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call Me.cpcore.db.csGoNext(CSPointer)
                    Loop
                    result = cr & "<ul class=""ccWatchList"">" & htmlIndent(result) & cr & "</ul>"
                End If
                Call Me.cpcore.db.csClose(CSPointer)
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
                If Me.cpcore.db.csOk(CS) Then
                    ContentID = Models.Complex.cdefModel.getContentId(cpCore, "Content Watch")
                    Do While Me.cpcore.db.csOk(CS)
                        Link = Me.cpcore.db.csGetText(CS, "link")
                        LinkLabel = Me.cpcore.db.csGetText(CS, "LinkLabel")
                        RecordID = Me.cpcore.db.csGetInteger(CS, "ID")
                        If (LinkLabel <> "") Then
                            result = result & cr & "<li id=""main_ContentWatch" & RecordID & """ class=""ccListItem"">"
                            If (Link <> "") Then
                                result = result & "<a href=""http://" & Me.cpcore.webServer.requestDomain & requestAppRootPath & Me.cpcore.webServer.requestPage & "?rc=" & ContentID & "&ri=" & RecordID & """>" & LinkLabel & "</a>"
                            Else
                                result = result & LinkLabel
                            End If
                            result = result & "</li>"
                        End If
                        Call Me.cpcore.db.csGoNext(CS)
                    Loop
                    If result <> "" Then
                        result = Me.cpcore.html.html_GetContentCopy("Watch List Caption: " & ListName, ListName, Me.cpcore.doc.authContext.user.id, True, Me.cpcore.doc.authContext.isAuthenticated) & cr & "<ul class=""ccWatchList"">" & htmlIndent(result) & cr & "</ul>"
                    End If
                End If
                Call Me.cpcore.db.csClose(CS)
                '
                If Me.cpcore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                    result = Me.cpcore.html.getEditWrapper("Watch List [" & ListName & "]", result)
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
                    Dim addon As addonModel = addonModel.create(cpcore, addonGuidSiteStructureGuid)
                    siteStructure = Me.cpcore.addon.execute(addon, New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextSimple})
                    'siteStructure = Me.cpcore.addon.execute_legacy2(0, addonGuidSiteStructureGuid, "", CPUtilsBaseClass.addonContext.ContextSimple, "", 0, "", "", False, -1, "", returnStatus, Nothing)
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
            Try
                Dim RootPageContentName As String = Models.Entity.pageContentModel.contentName
                Dim LiveRecordContentName As String = Models.Entity.pageContentModel.contentName
                Dim Link As String
                Dim page_ParentID As Integer
                Dim PageList As String
                Dim OptionsPanelAuthoringStatus As String
                Dim ButtonList As String
                Dim AllowInsert As Boolean
                Dim AllowCancel As Boolean
                Dim allowSave As Boolean
                Dim AllowDelete As Boolean
                Dim AllowMarkReviewed As Boolean
                Dim CDef As Models.Complex.cdefModel
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
                CDef = Models.Complex.cdefModel.getCdef(cpcore, LiveRecordContentName)
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
                Call getAuthoringPermissions(LiveRecordContentName, page.id, AllowInsert, AllowCancel, allowSave, AllowDelete, False, False, False, False, readOnlyField)
                AllowMarkReviewed = Models.Complex.cdefModel.isContentFieldSupported(cpcore, Models.Entity.pageContentModel.contentName, "DateReviewed")
                OptionsPanelAuthoringStatus = cpcore.doc.authContext.main_GetAuthoringStatusMessage(cpcore, False, IsEditLocked, main_EditLockMemberName, main_EditLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName)
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
                If AllowMarkReviewed Then
                    ButtonList = ButtonList & "," & ButtonMarkReviewed
                End If
                If ButtonList <> "" Then
                    ButtonList = Mid(ButtonList, 2)
                    ButtonList = cpcore.html.main_GetPanelButtons(ButtonList, "Button")
                End If
                'If OptionsPanelAuthoringStatus <> "" Then
                '    result = result & "" _
                '        & cr & "<tr>" _
                '        & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
                '        & cr & "</tr>"
                'End If
                If (cpcore.doc.debug_iUserError <> "") Then
                    result = result & "" _
                        & cr & "<tr>" _
                        & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & errorController.error_GetUserError(cpcore) & "</div></td>" _
                        & cr & "</tr>"
                End If
                If readOnlyField Then
                    result = result & "" _
                    & cr & "<tr>" _
                    & cr2 & "<td colspan=""2"" class=""qeRow"">" & getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
                    & cr & "</tr>"
                Else
                    result = result & "" _
                    & cr & "<tr>" _
                    & cr2 & "<td colspan=""2"" class=""qeRow"">" & getQuickEditingBody(LiveRecordContentName, OrderByClause, AllowPageList, True, rootPageId, readOnlyField, AllowReturnLink, RootPageContentName, ArchivePages, contactMemberID) & "</td>" _
                    & cr & "</tr>"
                End If
                result = result _
                    & cr & "<tr>" _
                    & cr2 & "<td class=""qeRow qeLeft"" style=""padding-top:10px;"">Name</td>" _
                    & cr2 & "<td class=""qeRow qeRight"">" & cpcore.html.html_GetFormInputText2("name", page.name, 1, , , , readOnlyField) & "</td>" _
                    & cr & "</tr>" _
                    & ""
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
                Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.create(cpcore, cpcore.siteProperties.childListAddonID)
                Dim executeContext As New CPUtilsBaseClass.addonExecuteContext With {
                    .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                        .contentName = Models.Entity.pageContentModel.contentName,
                        .fieldName = "",
                        .recordId = page.id
                    },
                        .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpcore, page.ChildListInstanceOptions),
                        .instanceGuid = PageChildListInstanceID
                    }
                PageList = cpcore.addon.execute(addon, executeContext)
                'PageList = cpcore.addon.execute_legacy2(cpcore.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, Models.Entity.pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
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
                    & cr & result _
                    & cr & cpcore.html.html_GetUploadFormEnd()

                '& cr & cpcore.html.main_GetPanelHeader("Contensive Quick Editor") _

                result = "" _
                    & cr & "<div class=""ccCon"">" _
                    & genericController.htmlIndent(result) _
                    & cr & "</div>"
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
        Friend Function getQuickEditingBody(ByVal ContentName As String, ByVal OrderByClause As String, ByVal AllowChildList As Boolean, ByVal Authoring As Boolean, ByVal rootPageId As Integer, ByVal readOnlyField As Boolean, ByVal AllowReturnLink As Boolean, ByVal RootPageContentName As String, ByVal ArchivePage As Boolean, ByVal contactMemberID As Integer) As String
            Dim pageCopy As String = page.Copyfilename.content
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
        '========================================================================
        ' ----- Process the reply from the Authoring Tools Panel form
        '========================================================================
        '
        Public Sub processFormQuickEditing()
            '
            Dim RecordParentID As Integer
            Dim SaveButNoChanges As Boolean
            Dim ParentID As Integer
            Dim Link As String
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
            ' Dim main_WorkflowSupport As Boolean
            '
            RecordModified = False
            RecordID = (cpcore.docProperties.getInteger("ID"))
            Button = cpcore.docProperties.getText("Button")
            iIsAdmin = cpcore.doc.authContext.isAuthenticatedAdmin(cpcore)
            '
            If (Button <> "") And (RecordID <> 0) And (pageContentModel.contentName <> "") And (cpcore.doc.authContext.isAuthenticatedContentManager(cpcore, pageContentModel.contentName)) Then
                ' main_WorkflowSupport = cpcore.siteProperties.allowWorkflowAuthoring And cpcore.workflow.isWorkflowAuthoringCompatible(pageContentModel.contentName)
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
                    Call cpcore.doc.markRecordReviewed(pageContentModel.contentName, RecordID)
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
                            Or (Button = ButtonOK)
                    Else
                        '
                        ' cases that CM can save
                        '
                        allowSave = False _
                            Or (Button = ButtonAddChildPage) _
                            Or (Button = ButtonAddSiblingPage) _
                            Or (Button = ButtonSave) _
                            Or (Button = ButtonOK)
                    End If
                End If
                If allowSave Then
                    '
                    ' ----- Save Changes
                    SaveButNoChanges = True
                    Dim page As pageContentModel = pageContentModel.create(cpcore, RecordID)
                    If (page IsNot Nothing) Then
                        Copy = cpcore.docProperties.getText("copyFilename")
                        Copy = cpcore.html.convertEditorResponseToActiveContent(Copy)
                        If Copy <> page.Copyfilename.content Then
                            page.Copyfilename.content = Copy
                            SaveButNoChanges = False
                        End If
                        RecordName = cpcore.docProperties.getText("name")
                        If RecordName <> page.name Then
                            page.name = RecordName
                            Call docController.addLinkAlias(cpcore, RecordName, RecordID, "")
                            SaveButNoChanges = False
                        End If
                        RecordParentID = page.ParentID
                        page.save(cpcore)
                        '
                        Call cpcore.workflow.SetEditLock(pageContentModel.contentName, page.id)
                        '
                        If Not SaveButNoChanges Then
                            Call cpcore.doc.processAfterSave(False, pageContentModel.contentName, page.id, page.name, page.ParentID, False)
                            Call cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName)
                        End If
                    End If
                End If
                If (Button = ButtonAddChildPage) Then
                    '
                    '
                    '
                    CSBlock = cpcore.db.csInsertRecord(pageContentModel.contentName)
                    If cpcore.db.csOk(CSBlock) Then
                        Call cpcore.db.csSet(CSBlock, "active", True)
                        Call cpcore.db.csSet(CSBlock, "ParentID", RecordID)
                        Call cpcore.db.csSet(CSBlock, "contactmemberid", cpcore.doc.authContext.user.id)
                        Call cpcore.db.csSet(CSBlock, "name", "New Page added " & cpcore.doc.profileStartTime & " by " & cpcore.doc.authContext.user.name)
                        Call cpcore.db.csSet(CSBlock, "copyFilename", "")
                        RecordID = cpcore.db.csGetInteger(CSBlock, "ID")
                        Call cpcore.db.csSave2(CSBlock)
                        '
                        Link = getPageLink(RecordID, "", True, False)
                        'Link = main_GetPageLink(RecordID)
                        'If main_WorkflowSupport Then
                        '    If Not cpCore.doc.authContext.isWorkflowRendering() Then
                        '        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                        '        Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
                        '    End If
                        'End If
                        Call cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", False)
                    End If
                    Call cpcore.db.csClose(CSBlock)
                    '
                    Call cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName)
                End If
                If (Button = ButtonAddSiblingPage) Then
                    '
                    '
                    '
                    CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID, , , "ParentID")
                    If cpcore.db.csOk(CSBlock) Then
                        ParentID = cpcore.db.csGetInteger(CSBlock, "ParentID")
                    End If
                    Call cpcore.db.csClose(CSBlock)
                    If ParentID <> 0 Then
                        CSBlock = cpcore.db.csInsertRecord(pageContentModel.contentName)
                        If cpcore.db.csOk(CSBlock) Then
                            Call cpcore.db.csSet(CSBlock, "active", True)
                            Call cpcore.db.csSet(CSBlock, "ParentID", ParentID)
                            Call cpcore.db.csSet(CSBlock, "contactmemberid", cpcore.doc.authContext.user.id)
                            Call cpcore.db.csSet(CSBlock, "name", "New Page added " & cpcore.doc.profileStartTime & " by " & cpcore.doc.authContext.user.name)
                            Call cpcore.db.csSet(CSBlock, "copyFilename", "")
                            RecordID = cpcore.db.csGetInteger(CSBlock, "ID")
                            Call cpcore.db.csSave2(CSBlock)
                            '
                            Link = getPageLink(RecordID, "", True, False)
                            'Link = main_GetPageLink(RecordID)
                            'If main_WorkflowSupport Then
                            '    If Not cpCore.doc.authContext.isWorkflowRendering() Then
                            '        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "This new unpublished page has been added and Workflow Rendering has been enabled so you can edit this page.", True)
                            '        Call cpcore.siteProperties.setProperty("AllowWorkflowRendering", True)
                            '    End If
                            'End If
                            Call cpcore.webServer.redirect(Link, "Redirecting because a new page has been added with the quick editor.", False)
                        End If
                        Call cpcore.db.csClose(CSBlock)
                    End If
                    Call cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName)
                End If
                If (Button = ButtonDelete) Then
                    CSBlock = cpcore.db.csOpenRecord(pageContentModel.contentName, RecordID)
                    If cpcore.db.csOk(CSBlock) Then
                        ParentID = cpcore.db.csGetInteger(CSBlock, "parentid")
                    End If
                    Call cpcore.db.csClose(CSBlock)
                    '
                    Call deleteChildRecords(pageContentModel.contentName, RecordID, False)
                    Call cpcore.db.deleteContentRecord(pageContentModel.contentName, RecordID)
                    '
                    If Not False Then
                        Call cpcore.cache.invalidateAllObjectsInContent(pageContentModel.contentName)
                    End If
                    '
                    If Not False Then
                        Link = getPageLink(ParentID, "", True, False)
                        Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "The page has been deleted, and you have been redirected to the parent of the deleted page.", True)
                        Call cpcore.webServer.redirect(Link, "Redirecting to the parent page because the page was deleted with the quick editor.", redirectBecausePageNotFound)
                        Exit Sub
                    End If
                End If
                '
                'If (Button = ButtonAbortEdit) Then
                '    Call cpcore.workflow.abortEdit2(pageContentModel.contentName, RecordID, cpCore.doc.authContext.user.id)
                'End If
                'If (Button = ButtonPublishSubmit) Then
                '    Call cpcore.workflow.main_SubmitEdit(pageContentModel.contentName, RecordID)
                '    Call sendPublishSubmitNotice(pageContentModel.contentName, RecordID, "")
                'End If
                If (Not (cpcore.doc.debug_iUserError <> "")) And ((Button = ButtonOK) Or (Button = ButtonCancel)) Then
                    '
                    ' ----- Turn off Quick Editor if not save or add child
                    '
                    Call cpcore.visitProperty.setProperty("AllowQuickEditor", "0")
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
                If (String.IsNullOrEmpty(ContentName)) Then
                    ContentName = pageContentModel.contentName
                End If
                Dim isAuthoring = cpcore.doc.authContext.isEditing(ContentName)
                '
                Dim ChildListCount As Integer = 0
                Dim UcaseRequestedListName As String = genericController.vbUCase(RequestedListName)
                If (UcaseRequestedListName = "NONE") Or (UcaseRequestedListName = "ORPHAN") Then
                    UcaseRequestedListName = ""
                End If
                '
                Dim archiveLink As String = cpcore.webServer.requestPathPage
                archiveLink = genericController.ConvertLinkToShortLink(archiveLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath)
                archiveLink = genericController.EncodeAppRootPath(archiveLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain)
                '
                Dim sqlCriteria As String = "(parentId=" & page.id & ")"
                Dim sqlOrderBy As String = "sortOrder"
                Dim childPageList As List(Of pageContentModel) = pageContentModel.createList(cpcore, sqlCriteria, sqlOrderBy)
                Dim inactiveList As String = ""
                Dim activeList As String = ""
                For Each childPage As pageContentModel In childPageList
                    Dim PageLink As String = getPageLink(childPage.id, "", True, False)
                    Dim pageMenuHeadline As String = childPage.MenuHeadline
                    If pageMenuHeadline = "" Then
                        pageMenuHeadline = Trim(childPage.name)
                        If pageMenuHeadline = "" Then
                            pageMenuHeadline = "Related Page"
                        End If
                    End If
                    Dim pageEditLink As String = ""
                    If cpcore.doc.authContext.isEditing(ContentName) Then
                        pageEditLink = cpcore.html.main_GetRecordEditLink2(ContentName, childPage.id, True, childPage.name, True)
                    End If
                    '
                    Dim link As String = PageLink
                    If ArchivePages Then
                        link = genericController.modifyLinkQuery(archiveLink, rnPageId, CStr(childPage.id), True)
                    End If
                    Dim blockContentComposite As Boolean = False
                    If childPage.BlockContent Or childPage.BlockPage Then
                        blockContentComposite = Not bypassContentBlock(childPage.ContentControlID, childPage.id)
                    End If
                    Dim LinkedText As String = genericController.csv_GetLinkedText("<a href=""" & genericController.encodeHTML(link) & """>", pageMenuHeadline)
                    If (UcaseRequestedListName = "") And (childPage.ParentListName <> "") And (Not isAuthoring) Then
                        '
                        ' ----- Requested orphan list, and this record is in a named list, and not editing, do not display
                        '
                    ElseIf (UcaseRequestedListName = "") And (childPage.ParentListName <> "") Then
                        '
                        ' ----- Requested orphan list, and this record is in a named list, but authoring, list it
                        '
                        If isAuthoring Then
                            inactiveList = inactiveList & cr & "<li name=""page" & childPage.id & """ name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            inactiveList = inactiveList & pageEditLink
                            inactiveList = inactiveList & "[from Child Page List '" & childPage.ParentListName & "': " & LinkedText & "]"
                            inactiveList = inactiveList & "</li>"
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
                            inactiveList = inactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            inactiveList = inactiveList & pageEditLink
                            inactiveList = inactiveList & "[Hidden (Allow in Child Lists is not checked): " & LinkedText & "]"
                            inactiveList = inactiveList & "</li>"
                        End If
                    ElseIf Not childPage.Active Then
                        '
                        ' ----- Not active record, display hint if authoring
                        '
                        If isAuthoring Then
                            inactiveList = inactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            inactiveList = inactiveList & pageEditLink
                            inactiveList = inactiveList & "[Hidden (Inactive): " & LinkedText & "]"
                            inactiveList = inactiveList & "</li>"
                        End If
                    ElseIf (childPage.PubDate <> Date.MinValue) And (childPage.PubDate > cpcore.doc.profileStartTime) Then
                        '
                        ' ----- Child page has not been published
                        '
                        If isAuthoring Then
                            inactiveList = inactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            inactiveList = inactiveList & pageEditLink
                            inactiveList = inactiveList & "[Hidden (To be published " & childPage.PubDate & "): " & LinkedText & "]"
                            inactiveList = inactiveList & "</li>"
                        End If
                    ElseIf (childPage.DateExpires <> Date.MinValue) And (childPage.DateExpires < cpcore.doc.profileStartTime) Then
                        '
                        ' ----- Child page has expired
                        '
                        If isAuthoring Then
                            inactiveList = inactiveList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                            inactiveList = inactiveList & pageEditLink
                            inactiveList = inactiveList & "[Hidden (Expired " & childPage.DateExpires & "): " & LinkedText & "]"
                            inactiveList = inactiveList & "</li>"
                        End If
                    Else
                        '
                        ' ----- display list (and authoring links)
                        '
                        activeList = activeList & cr & "<li name=""page" & childPage.id & """  id=""page" & childPage.id & """ class=""ccListItem"">"
                        If pageEditLink <> "" Then
                            activeList = activeList & pageEditLink & "&nbsp;"
                        End If
                        activeList = activeList & LinkedText
                        '
                        ' include authoring mark for content block
                        '
                        If isAuthoring Then
                            If childPage.BlockContent Then
                                activeList = activeList & "&nbsp;[Content Blocked]"
                            End If
                            If childPage.BlockPage Then
                                activeList = activeList & "&nbsp;[Page Blocked]"
                            End If
                        End If
                        '
                        ' include overview
                        ' if AllowBrief is false, BriefFilename is not loaded
                        '
                        If (childPage.BriefFilename <> "") And (childPage.AllowBrief) Then
                            Dim Brief As String = Trim(cpcore.cdnFiles.readFile(childPage.BriefFilename))
                            If Brief <> "" Then
                                activeList = activeList & "<div class=""ccListCopy"">" & Brief & "</div>"
                            End If
                        End If
                        activeList = activeList & "</li>"
                        ChildListCount = ChildListCount + 1
                        '.IsDisplayed = True
                    End If
                Next
                '
                ' ----- Add Link
                '
                If (Not ArchivePages) Then
                    Dim AddLink As String = cpcore.html.main_GetRecordAddLink(ContentName, "parentid=" & parentPageID & ",ParentListName=" & UcaseRequestedListName, True)
                    If AddLink <> "" Then
                        inactiveList = inactiveList & cr & "<li class=""ccListItem"">" & AddLink & "</LI>"
                    End If
                End If
                '
                ' ----- If there is a list, add the list start and list end
                '
                result = ""
                If activeList <> "" Then
                    result = result & cr & "<ul id=""childPageList_" & parentPageID & "_" & RequestedListName & """ class=""ccChildList"">" & genericController.htmlIndent(activeList) & cr & "</ul>"
                End If
                If inactiveList <> "" Then
                    result = result & cr & "<ul id=""childPageList_" & parentPageID & "_" & RequestedListName & """ class=""ccChildListInactive"">" & genericController.htmlIndent(inactiveList) & cr & "</ul>"
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
            If cpcore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                bypassContentBlock = True
            ElseIf cpcore.doc.authContext.isAuthenticatedContentManager(cpcore, Models.Complex.cdefModel.getContentNameByID(cpcore, ContentID)) Then
                bypassContentBlock = True
            Else
                SQL = "SELECT ccMemberRules.MemberID" _
                    & " FROM (ccPageContentBlockRules LEFT JOIN ccgroups ON ccPageContentBlockRules.GroupID = ccgroups.ID) LEFT JOIN ccMemberRules ON ccgroups.ID = ccMemberRules.GroupID" _
                    & " WHERE (((ccPageContentBlockRules.RecordID)=" & RecordID & ")" _
                    & " AND ((ccPageContentBlockRules.Active)<>0)" _
                    & " AND ((ccgroups.Active)<>0)" _
                    & " AND ((ccMemberRules.Active)<>0)" _
                    & " AND ((ccMemberRules.DateExpires) Is Null Or (ccMemberRules.DateExpires)>" & cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime) & ")" _
                    & " AND ((ccMemberRules.MemberID)=" & cpcore.doc.authContext.user.id & "));"
                CS = cpcore.db.csOpenSql(SQL)
                bypassContentBlock = cpcore.db.csOk(CS)
                Call cpcore.db.csClose(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError13("IsContentBlocked")
        End Function
        ''
        ''====================================================================================================
        ''   main_GetTemplateLink
        ''       Added to externals (aoDynamicMenu) can main_Get hard template links
        ''====================================================================================================
        ''
        'Public Function getTemplateLink(templateId As Integer) As String
        '    Dim template As pageTemplateModel = pageTemplateModel.create(cpcore, templateId, New List(Of String))
        '    If template IsNot Nothing Then
        '        Return template.Link
        '    End If
        '    Return ""
        'End Function
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
                CS = cpcore.db.csOpen(ContentName, "parentid=" & RecordID, , , , ,, "ID")
                Do While cpcore.db.csOk(CS)
                    result = result & "," & cpcore.db.csGetInteger(CS, "ID")
                    cpcore.db.csGoNext(CS)
                Loop
                Call cpcore.db.csClose(CS)
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
                        QuickEditing = cpcore.doc.authContext.isQuickEditing(cpcore, "page content")
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
        Public Sub getAuthoringPermissions(ByVal ContentName As String, ByVal RecordID As Integer, ByRef AllowInsert As Boolean, ByRef AllowCancel As Boolean, ByRef allowSave As Boolean, ByRef AllowDelete As Boolean, ByRef ignore1 As Boolean, ByRef ignore2 As Boolean, ByRef ignore3 As Boolean, ByRef ignore4 As Boolean, ByRef readOnlyField As Boolean)
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
            Dim CDef As Models.Complex.cdefModel
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
            CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
            '
            ' Set Buttons based on Status
            '
            If IsEditing Then
                '
                ' Edit Locked
                '
                AllowCancel = True
                readOnlyField = True
            ElseIf (Not False) Or (Not False) Then
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
                'ElseIf cpCore.doc.authContext.isAuthenticatedAdmin(cpcore) Then
                '    '
                '    ' Workflow, Admin
                '    '
                '    If IsApproved Then
                '        '
                '        ' Workflow, Admin, Approved
                '        '
                '        AllowCancel = True
                '        ignore1 = True
                '        ignore2 = True
                '        readOnlyField = True
                '        AllowInsert = True
                '    ElseIf IsSubmitted Then
                '        '
                '        ' Workflow, Admin, Submitted (not approved)
                '        '
                '        AllowCancel = True
                '        If Not IsDeleted Then
                '            allowSave = True
                '        End If
                '        ignore1 = True
                '        ignore2 = True
                '        ignore4 = True
                '        If IsDeleted Then
                '            readOnlyField = True
                '        End If
                '        AllowInsert = True
                '    ElseIf IsInserted Then
                '        '
                '        ' Workflow, Admin, Inserted (not submitted, not approved)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        ignore1 = True
                '        ignore2 = True
                '        ignore3 = True
                '        ignore4 = True
                '        AllowInsert = True
                '    ElseIf IsDeleted Then
                '        '
                '        ' Workflow, Admin, Deleted record (not submitted, not approved)
                '        '
                '        AllowCancel = True
                '        ignore1 = True
                '        ignore2 = True
                '        ignore3 = True
                '        ignore4 = True
                '        readOnlyField = True
                '        AllowInsert = True
                '    ElseIf IsModified Then
                '        '
                '        ' Workflow, Admin, Modified (not submitted, not approved, not inserted, not deleted)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        ignore1 = True
                '        ignore2 = True
                '        ignore3 = True
                '        ignore4 = True
                '        AllowDelete = True
                '        AllowInsert = True
                '    Else
                '        '
                '        ' Workflow, Admin, Not Modified (not submitted, not approved, not inserted, not deleted)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        ignore1 = True
                '        ignore4 = True
                '        ignore3 = True
                '        AllowDelete = True
                '        AllowInsert = True
                '    End If
                'Else
                '    '
                '    ' Workflow, Content Manager
                '    '
                '    If IsApproved Then
                '        '
                '        ' Workflow, Content Manager, Approved
                '        '
                '        AllowCancel = True
                '        readOnlyField = True
                '        AllowInsert = True
                '    ElseIf IsSubmitted Then
                '        '
                '        ' Workflow, Content Manager, Submitted (not approved)
                '        '
                '        AllowCancel = True
                '        readOnlyField = True
                '        AllowInsert = True
                '    ElseIf IsInserted Then
                '        '
                '        ' Workflow, Content Manager, Inserted (not submitted, not approved)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        ignore2 = True
                '        ignore3 = True
                '        AllowInsert = True
                '    ElseIf IsDeleted Then
                '        '
                '        ' Workflow, Content Manager, Deleted record (not submitted, not approved)
                '        '
                '        AllowCancel = True
                '        ignore2 = True
                '        ignore3 = True
                '        readOnlyField = True
                '        AllowInsert = True
                '    ElseIf IsModified Then
                '        '
                '        ' Workflow, Content Manager, Modified (not submitted, not approved, not inserted, not deleted)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        AllowDelete = True
                '        ignore2 = True
                '        ignore3 = True
                '        AllowInsert = True
                '    Else
                '        '
                '        ' Workflow, Content Manager, Not Modified (not submitted, not approved, not inserted, not deleted)
                '        '
                '        AllowCancel = True
                '        allowSave = True
                '        AllowDelete = True
                '        ignore2 = True
                '        ignore3 = True
                '        AllowInsert = True
                '    End If
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
            Dim CDef As Models.Complex.cdefModel
            Dim Copy As String
            Dim Link As String
            Dim FromAddress As String
            '
            MethodName = "main_SendPublishSubmitNotice"
            '
            FromAddress = cpcore.siteProperties.getText("EmailPublishSubmitFrom", cpcore.siteProperties.emailAdmin)
            CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
            Link = "/" & cpcore.serverConfig.appConfig.adminRoute & "?af=" & AdminFormPublishing
            Copy = Msg_AuthoringSubmittedNotification
            Copy = genericController.vbReplace(Copy, "<DOMAINNAME>", "<a href=""" & genericController.encodeHTML(Link) & """>" & cpcore.webServer.requestDomain & "</a>")
            Copy = genericController.vbReplace(Copy, "<RECORDNAME>", RecordName)
            Copy = genericController.vbReplace(Copy, "<CONTENTNAME>", ContentName)
            Copy = genericController.vbReplace(Copy, "<RECORDID>", RecordID.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDDATE>", cpcore.doc.profileStartTime.ToString)
            Copy = genericController.vbReplace(Copy, "<SUBMITTEDNAME>", cpcore.doc.authContext.user.name)
            '
            Call cpcore.email.sendGroup(cpcore.siteProperties.getText("WorkflowEditorGroup", "Site Managers"), FromAddress, "Authoring Submitted Notification", Copy, False, True)
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
            ContentRecordKey = Models.Complex.cdefModel.getContentId(cpcore, genericController.encodeText(ContentName)) & "." & genericController.EncodeInteger(RecordID)
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
                        linkPathPage = "/" & main_guessDefaultPage()
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
                Dim SqlCriteria As String = "(domainId=" & domain.id & ")"
                Dim allowTemplateRuleList As List(Of Models.Entity.TemplateDomainRuleModel) = Models.Entity.TemplateDomainRuleModel.createList(cpcore, SqlCriteria)
                Dim templateAllowed As Boolean = False
                For Each rule As TemplateDomainRuleModel In allowTemplateRuleList
                    If (rule.templateId = template.ID) Then
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
                If page.IsSecure Or template.IsSecure Then
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
            Dim CDef As Models.Complex.cdefModel
            '
            main_IsChildRecord = (ChildRecordID = ParentRecordID)
            If Not main_IsChildRecord Then
                CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName)
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
                CS = cpcore.db.csOpenSql(SQL)
                If cpcore.db.csOk(CS) Then
                    ChildRecordParentID = cpcore.db.csGetInteger(CS, "ParentID")
                End If
                Call cpcore.db.csClose(CS)
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
            CSPointer = cpcore.db.csOpen("Content Watch", "ContentRecordKey=" & cpcore.db.encodeSQLText(ContentRecordKey), , , , , , "Link,Clicks")
            If cpcore.db.csOk(CSPointer) Then
                getContentWatchLinkByKey = cpcore.db.csGetText(CSPointer, "Link")
                If genericController.EncodeBoolean(IncrementClicks) Then
                    Call cpcore.db.csSet(CSPointer, "Clicks", cpcore.db.csGetInteger(CSPointer, "clicks") + 1)
                End If
            Else
                getContentWatchLinkByKey = genericController.encodeText(DefaultLink)
            End If
            Call cpcore.db.csClose(CSPointer)
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
            If cpcore.db.csOk(CS) Then
                templateId = cpcore.db.csGetInteger(CS, "TemplateID")
                ParentID = cpcore.db.csGetInteger(CS, "ParentID")
            End If
            Call cpcore.db.csClose(CS)
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
                                resultLink = "" ' template.Link
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
                CS = cpcore.db.csOpen("Form Pages", "name=" & cpcore.db.encodeSQLText("Registration Form"))
                If Not cpcore.db.csOk(CS) Then
                    '
                    ' create Version 1 template - just to main_Get it started
                    '
                    Call cpcore.db.csClose(CS)
                    GroupNameList = "Registered"
                    CS = cpcore.db.csInsertRecord("Form Pages")
                    If cpcore.db.csOk(CS) Then
                        Call cpcore.db.csSet(CS, "name", "Registration Form")
                        Copy = "" _
                        & vbCrLf & "<table border=""0"" cellpadding=""2"" cellspacing=""0"" width=""100%"">" _
                        & vbCrLf & "{{REPEATSTART}}<tr><td align=right style=""height:22px;"">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}" _
                        & vbCrLf & "<tr><td align=right><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=135 height=1></td><td width=""100%"">&nbsp;</td></tr>" _
                        & vbCrLf & "<tr><td colspan=2>&nbsp;<br>" & cpcore.html.main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>" _
                        & vbCrLf & "</table>"
                        Call cpcore.db.csSet(CS, "Body", Copy)
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
                        Call cpcore.db.csSet(CS, "Instructions", Copy)
                    End If
                End If
                Call cpcore.db.csClose(CS)
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
            Dim CS As Integer = cpcore.db.csInsertRecord(ContentName, CreatedBy)
            If cpcore.db.csOk(CS) Then
                Id = cpcore.db.csGetInteger(CS, "ID")
                Call cpcore.db.csSet(CS, "name", PageName)
                Call cpcore.db.csSet(CS, "active", "1")
                If True Then
                    Call cpcore.db.csSet(CS, "ccGuid", pageGuid)
                End If
                Call cpcore.db.csSave2(CS)
                '   Call cpcore.workflow.publishEdit("Page Content", Id)
            End If
            Call cpcore.db.csClose(CS)
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
            Dim linkAliasList As List(Of Models.Entity.linkAliasModel) = Models.Entity.linkAliasModel.createList(cpcore, PageID, QueryStringSuffix)
            If linkAliasList.Count > 0 Then
                linkAlias = linkAliasList.First.name
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
        ''
        ''=================================================================================================================================================
        ''
        '' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        ''
        'Public Shared Sub app_addLinkAlias(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String)
        '    Dim return_ignoreError As String = ""
        '    Call app_addLinkAlias2(cpcore, linkAlias, PageID, QueryStringSuffix, True, False, return_ignoreError)
        'End Sub
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Shared Sub addLinkAlias(cpcore As coreClass, linkAlias As String, PageID As Integer, QueryStringSuffix As String, Optional OverRideDuplicate As Boolean = False, Optional DupCausesWarning As Boolean = False, Optional ByRef return_WarningMessage As String = "")
            Const SafeString = "0123456789abcdefghijklmnopqrstuvwxyz-_/"
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
                            CS = cpcore.db.csOpen("Link Aliases", "name=" & cpcore.db.encodeSQLText(WorkingLinkAlias), , , , , , FieldList)
                            If Not cpcore.db.csOk(CS) Then
                                '
                                ' Alias not found, create a Link Aliases
                                '
                                Call cpcore.db.csClose(CS)
                                CS = cpcore.db.csInsertRecord("Link Aliases", 0)
                                If cpcore.db.csOk(CS) Then
                                    Call cpcore.db.csSet(CS, "Name", WorkingLinkAlias)
                                    'Call app.csv_SetCS(CS, "Link", Link)
                                    Call cpcore.db.csSet(CS, "Pageid", PageID)
                                    If True Then
                                        Call cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix)
                                    End If
                                End If
                            Else
                                '
                                ' Alias found, verify the pageid & QueryStringSuffix
                                '
                                Dim CurrentLinkAliasID As Integer
                                Dim resaveLinkAlias As Boolean
                                Dim CS2 As Integer
                                LinkAliasPageID = cpcore.db.csGetInteger(CS, "pageID")
                                If (cpcore.db.csGetText(CS, "QueryStringSuffix").ToLower = QueryStringSuffix.ToLower) And (PageID = LinkAliasPageID) Then
                                    '
                                    ' it maches a current entry for this link alias, if the current entry is not the highest number id,
                                    '   remove it and add this one
                                    '
                                    CurrentLinkAliasID = cpcore.db.csGetInteger(CS, "id")
                                    CS2 = cpcore.db.csOpenSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" & LinkAliasPageID & " order by id desc")
                                    If cpcore.db.csOk(CS2) Then
                                        resaveLinkAlias = (CurrentLinkAliasID <> cpcore.db.csGetInteger(CS2, "id"))
                                    End If
                                    Call cpcore.db.csClose(CS2)
                                    If resaveLinkAlias Then
                                        Call cpcore.db.executeQuery("delete from ccLinkAliases where id=" & CurrentLinkAliasID)
                                        Call cpcore.db.csClose(CS)
                                        CS = cpcore.db.csInsertRecord("Link Aliases", 0)
                                        If cpcore.db.csOk(CS) Then
                                            Call cpcore.db.csSet(CS, "Name", WorkingLinkAlias)
                                            Call cpcore.db.csSet(CS, "Pageid", PageID)
                                            If True Then
                                                Call cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix)
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
                                        Call cpcore.db.csSet(CS, "Pageid", PageID)
                                        If True Then
                                            Call cpcore.db.csSet(CS, "QueryStringSuffix", QueryStringSuffix)
                                        End If
                                    ElseIf AllowLinkAlias Then
                                        '
                                        ' This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                        '
                                        If DupCausesWarning Then
                                            If LinkAliasPageID = 0 Then '
                                                PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website."
                                            Else
                                                PageContentCID = Models.Complex.cdefModel.getContentId(cpcore, "Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page (<a href=""?af=4&cid=" & PageContentCID & "&id=" & LinkAliasPageID & """>edit</a>)." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website."
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            Dim linkAliasId As Integer = cpcore.db.csGetInteger(CS, "id")
                            Call cpcore.db.csClose(CS)
                            cpcore.cache.invalidateContent_Entity(cpcore, linkAliasModel.contentTableName, linkAliasId)
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
        ''
        ''   Returns the next entry in the array, empty when there are no more
        ''
        'Public Function getJavascriptOnLoad() As String
        '    Dim result As String = ""
        '    Dim Ptr As Integer
        '    If javascriptOnLoad.Count >= 0 Then
        '        For Ptr = 0 To javascriptOnLoad.Count - 1
        '            If javascriptOnLoad(Ptr) <> "" Then
        '                result = javascriptOnLoad(Ptr)
        '                javascriptOnLoad(Ptr) = ""
        '                Exit For
        '            End If
        '        Next
        '    End If
        '    Return result
        'End Function
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


        ' main_Get the Head innerHTML for any page
        '
        Public Function getHtmlHead() As String
            Dim result As String = ""
            Try
                '
                ' -- meta content
                result &= cr & "<title>" & cpcore.doc.htmlMetaContent_Title & "</title>"
                If cpcore.doc.htmlMetaContent_KeyWordList <> "" Then
                    result &= cr & "<meta name=""keywords"" content=""" & cpcore.doc.htmlMetaContent_KeyWordList & """ >"
                End If
                If cpcore.doc.htmlMetaContent_Description <> "" Then
                    result &= cr & "<meta name=""description"" content=""" & cpcore.doc.htmlMetaContent_Description & """ >"
                End If
                '
                ' -- favicon
                Dim VirtualFilename As String = cpcore.siteProperties.getText("faviconfilename")
                If VirtualFilename <> "" Then
                    Dim Pos As Integer = InStrRev(VirtualFilename, ".")
                    If Pos > 0 Then
                        Select Case Mid(VirtualFilename, Pos).ToLower()
                            Case ".ico"
                                result &= cr & "<link rel=""icon"" type=""image/vnd.microsoft.icon"" href=""" & genericController.getCdnFileLink(cpcore, VirtualFilename) & """ >"
                            Case ".png"
                                result &= cr & "<link rel=""icon"" type=""image/png"" href=""" & genericController.getCdnFileLink(cpcore, VirtualFilename) & """ >"
                            Case ".gif"
                                result &= cr & "<link rel=""icon"" type=""image/gif"" href=""" & genericController.getCdnFileLink(cpcore, VirtualFilename) & """ >"
                            Case ".jpg"
                                result &= cr & "<link rel=""icon"" type=""image/jpg"" href=""" & genericController.getCdnFileLink(cpcore, VirtualFilename) & """ >"
                        End Select
                    End If
                End If
                '
                ' -- misc caching, etc
                Dim encoding As String = genericController.encodeHTML(cpcore.siteProperties.getText("Site Character Encoding", "utf-8"))
                result = result _
                    & cr & "<meta http-equiv=""content-type"" content=""text/html; charset=" & encoding & """ >" _
                    & cr & "<meta http-equiv=""content-language"" content=""en-us"" >" _
                    & cr & "<meta http-equiv=""cache-control"" content=""no-cache"" >" _
                    & cr & "<meta http-equiv=""expires"" content=""-1"" >" _
                    & cr & "<meta http-equiv=""pragma"" content=""no-cache"" >" _
                    & cr & "<meta name=""generator"" content=""Contensive"" >"
                '
                ' -- no-follow
                If cpcore.webServer.response_NoFollow Then
                    result = result _
                    & cr & "<meta name=""robots"" content=""nofollow"" >" _
                    & cr & "<meta name=""mssmarttagspreventparsing"" content=""true"" >"
                End If
                '
                ' -- base is needed for Link Alias case where a slash is in the URL (page named 1/2/3/4/5)
                If (Not String.IsNullOrEmpty(cpcore.webServer.serverFormActionURL)) Then
                    'If (Not main_IsAdminSite) And (Not String.IsNullOrEmpty(cpcore.webServer.serverFormActionURL)) Then
                    Dim BaseHref As String = cpcore.webServer.serverFormActionURL
                    If cpcore.doc.refreshQueryString <> "" Then
                        BaseHref &= "?" & cpcore.doc.refreshQueryString
                    End If
                    result &= cr & "<base href=""" & BaseHref & """ >"
                End If
                '
                ' -- Styles
                result &= cpcore.doc.htmlMetaContent_StyleSheetTags
                cpcore.doc.htmlMetaContent_StyleSheetTags = String.Empty
                '
                ' -- head Javascript
                ' -- must be added as addon. result &= cr & "<script language=""JavaScript"" type=""text/javascript""  src=""" & cpcore.webServer.requestProtocol & cpcore.webServer.requestDomain & "/ccLib/ClientSide/Core.js""></script>"
                If cpcore.doc.scriptList_head.Count > 0 Then
                    For Ptr = 0 To cpcore.doc.scriptList_head.Count - 1
                        With cpcore.doc.scriptList_head(Ptr)
                            If (.addedByMessage <> "") And cpcore.visitProperty.getBoolean("AllowDebugging") Then
                                result &= cr & "<!-- from " & .addedByMessage & " -->"
                            End If
                            If Not .IsLink Then
                                result &= cr & "<script Language=""JavaScript"" type=""text/javascript"">" & .Text & cr & "</script>"
                            Else
                                result &= cr & "<script type=""text/javascript"" src=""" & .Text & """></script>"
                            End If
                        End With
                    Next
                    cpcore.doc.scriptList_head = {}
                End If
                '
                ' -- other head tags - always last
                result &= cpcore.doc.htmlMetaContent_OtherHeadTags
                cpcore.doc.htmlMetaContent_OtherHeadTags = String.Empty
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Process manual changes needed for Page Content Special Cases
        '       If workflow, only call this routine on a publish - it changes live records
        '========================================================================
        '
        Public Sub processAfterSave(IsDelete As Boolean, ContentName As String, RecordID As Integer, RecordName As String, RecordParentID As Integer, UseContentWatchLink As Boolean)
            Dim addonId As Integer
            Dim Option_String As String
            Dim Filename As String
            Dim FilenameExt As String
            Dim FilenameNoExt As String
            Dim FilePath As String = String.Empty
            Dim Pos As Integer
            Dim AltSizeList As String
            Dim sf As imageEditController
            Dim RebuildSizes As Boolean
            Dim CS As Integer
            Dim TableName As String
            Dim ContentID As Integer
            Dim ActivityLogOrganizationID As Integer
            '
            ContentID = Models.Complex.cdefModel.getContentId(cpcore, ContentName)
            TableName = Models.Complex.cdefModel.getContentTablename(cpcore, ContentName)
            markRecordReviewed(ContentName, RecordID)
            '
            ' -- invalidate the specific cache for this record
            cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID)
            '
            ' -- invalidate the tablename -- meaning any cache consumer that cannot itemize its entity records, can depend on this, which will invalidate anytime any record clears
            cpcore.cache.invalidateContent(TableName)
            '
            Select Case genericController.vbLCase(TableName)
                Case linkForwardModel.contentTableName
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
                Case linkAliasModel.contentTableName
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
                Case addonModel.contentTableName
                    '
                    Models.Complex.routeDictionaryModel.invalidateCache(cpcore)
                Case personModel.contentTableName
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",110"
                    CS = cpcore.db.cs_open2("people", RecordID, , , "Name,OrganizationID")
                    If cpcore.db.csOk(CS) Then
                        ActivityLogOrganizationID = cpcore.db.csGetInteger(CS, "OrganizationID")
                    End If
                    Call cpcore.db.csClose(CS)
                    If IsDelete Then
                        Call logController.logActivity2(cpcore, "deleting user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    Else
                        Call logController.logActivity2(cpcore, "saving changes to user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    End If
                Case "organizations"
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",120"
                    If IsDelete Then
                        Call logController.logActivity2(cpcore, "deleting organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    Else
                        Call logController.logActivity2(cpcore, "saving changes to organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    End If
                Case "ccsetup"
                    '
                    ' Site Properties
                    '
                    'hint = hint & ",130"
                    Select Case genericController.vbLCase(RecordName)
                        Case "allowlinkalias"
                            Call cpcore.cache.invalidateAllObjectsInContent("Page Content")
                        Case "sectionlandinglink"
                            Call cpcore.cache.invalidateAllObjectsInContent("Page Content")
                        Case siteproperty_serverPageDefault_name
                            Call cpcore.cache.invalidateAllObjectsInContent("Page Content")
                    End Select
                Case "ccpagecontent"
                    '
                    ' set ChildPagesFound true for parent page
                    '
                    'hint = hint & ",140"
                    If RecordParentID > 0 Then
                        If Not IsDelete Then
                            Call cpcore.db.executeQuery("update ccpagecontent set ChildPagesfound=1 where ID=" & RecordParentID)
                        End If
                    End If
                    '
                    ' Page Content special cases for delete
                    '
                    If IsDelete Then
                        '
                        ' Clear the Landing page and page not found site properties
                        '
                        If RecordID = genericController.EncodeInteger(cpcore.siteProperties.getText("PageNotFoundPageID", "0")) Then
                            Call cpcore.siteProperties.setProperty("PageNotFoundPageID", "0")
                        End If
                        If RecordID = cpcore.siteProperties.landingPageID Then
                            cpcore.siteProperties.setProperty("landingPageId", "0")
                        End If
                        '
                        ' Delete Link Alias entries with this PageID
                        '
                        Call cpcore.db.executeQuery("delete from cclinkAliases where PageID=" & RecordID)
                    End If
                    cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID)
                'Case "cctemplates" ', "ccsharedstyles"
                '    '
                '    ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                '    '
                '    'hint = hint & ",150"
                '    If Not IsNothing(cpCore.addonStyleRulesIndex) Then
                '        Call cpCore.addonStyleRulesIndex.clear()
                '    End If

                Case "ccaggregatefunctions"
                    '
                    ' -- add-ons, rebuild addonCache
                    cpcore.cache.invalidateContent("addonCache")
                    cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID)
                Case "cclibraryfiles"
                    '
                    ' if a AltSizeList is blank, make large,medium,small and thumbnails
                    '
                    'hint = hint & ",180"
                    If (cpcore.siteProperties.getBoolean("ImageAllowSFResize", True)) Then
                        If Not IsDelete Then
                            CS = cpcore.db.csOpenRecord("library files", RecordID)
                            If cpcore.db.csOk(CS) Then
                                Filename = cpcore.db.csGet(CS, "filename")
                                Pos = InStrRev(Filename, "/")
                                If Pos > 0 Then
                                    FilePath = Mid(Filename, 1, Pos)
                                    Filename = Mid(Filename, Pos + 1)
                                End If
                                Call cpcore.db.csSet(CS, "filesize", cpcore.appRootFiles.main_GetFileSize(FilePath & Filename))
                                Pos = InStrRev(Filename, ".")
                                If Pos > 0 Then
                                    FilenameExt = Mid(Filename, Pos + 1)
                                    FilenameNoExt = Mid(Filename, 1, Pos - 1)
                                    If genericController.vbInstr(1, "jpg,gif,png", FilenameExt, vbTextCompare) <> 0 Then
                                        sf = New imageEditController
                                        If sf.load(cpcore.appRootFiles.rootLocalPath & FilePath & Filename) Then
                                            '
                                            '
                                            '
                                            Call cpcore.db.csSet(CS, "height", sf.height)
                                            Call cpcore.db.csSet(CS, "width", sf.width)
                                            AltSizeList = cpcore.db.csGetText(CS, "AltSizeList")
                                            RebuildSizes = (AltSizeList = "")
                                            If RebuildSizes Then
                                                AltSizeList = ""
                                                '
                                                ' Attempt to make 640x
                                                '
                                                If sf.width >= 640 Then
                                                    sf.height = CInt(sf.height * (640 / sf.width))
                                                    sf.width = 640
                                                    Call sf.save(cpcore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-640x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "640x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 320x
                                                '
                                                If sf.width >= 320 Then
                                                    sf.height = CInt(sf.height * (320 / sf.width))
                                                    sf.width = 320
                                                    Call sf.save(cpcore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-320x" & sf.height & "." & FilenameExt)

                                                    AltSizeList = AltSizeList & vbCrLf & "320x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 160x
                                                '
                                                If sf.width >= 160 Then
                                                    sf.height = CInt(sf.height * (160 / sf.width))
                                                    sf.width = 160
                                                    Call sf.save(cpcore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-160x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "160x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 80x
                                                '
                                                If sf.width >= 80 Then
                                                    sf.height = CInt(sf.height * (80 / sf.width))
                                                    sf.width = 80
                                                    Call sf.save(cpcore.appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-180x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "80x" & sf.height
                                                End If
                                                Call cpcore.db.csSet(CS, "AltSizeList", AltSizeList)
                                            End If
                                            Call sf.Dispose()
                                            sf = Nothing
                                        End If
                                        '                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
                                        '                                On Error Resume Next
                                        '                                sf.LoadFromFile (app.publicFiles.rootFullPath & FilePath & Filename)
                                        '                                If Err.Number = 0 Then
                                        '                                    Call app.SetCS(CS, "height", sf.Height)
                                        '                                    Call app.SetCS(CS, "width", sf.Width)
                                        '                                Else
                                        '                                    Err.Clear
                                        '                                End If
                                        '                                AltSizeList = cs_getText(CS, "AltSizeList")
                                        '                                RebuildSizes = (AltSizeList = "")
                                        '                                If RebuildSizes Then
                                        '                                    AltSizeList = ""
                                        '                                    '
                                        '                                    ' Attempt to make 640x
                                        '                                    '
                                        '                                    If sf.Width >= 640 Then
                                        '                                        sf.Width = 640
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-640x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "640x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 320x
                                        '                                    '
                                        '                                    If sf.Width >= 320 Then
                                        '                                        sf.Width = 320
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-320x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "320x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 160x
                                        '                                    '
                                        '                                    If sf.Width >= 160 Then
                                        '                                        sf.Width = 160
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-160x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "160x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 80x
                                        '                                    '
                                        '                                    If sf.Width >= 80 Then
                                        '                                        sf.Width = 80
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-80x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "80x" & sf.Height
                                        '                                    End If
                                        '                                    Call app.SetCS(CS, "AltSizeList", AltSizeList)
                                        '                                End If
                                        '                                sf = Nothing
                                    End If
                                End If
                            End If
                            Call cpcore.db.csClose(CS)
                        End If
                    End If
                Case Else
                    '
                    ' -- edit and delete for records -- clear entity cache
                    cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID)
            End Select
            '
            ' -- edit and delete for records -- clear entity cache
            cpcore.cache.invalidateContent_Entity(cpcore, TableName, RecordID)
            '
            ' Process Addons marked to trigger a process call on content change
            '
            'hint = hint & ",190"
            If True Then
                'hint = hint & ",200 content=[" & ContentID & "]"
                CS = cpcore.db.csOpen("Add-on Content Trigger Rules", "ContentID=" & ContentID, , , , , , "addonid")
                Option_String = "" _
                    & vbCrLf & "action=contentchange" _
                    & vbCrLf & "contentid=" & ContentID _
                    & vbCrLf & "recordid=" & RecordID _
                    & ""
                Do While cpcore.db.csOk(CS)
                    addonId = cpcore.db.csGetInteger(CS, "Addonid")
                    'hint = hint & ",210 addonid=[" & addonId & "]"
                    Call cpcore.addon.executeAddonAsProcess(CStr(addonId), Option_String)
                    Call cpcore.db.csGoNext(CS)
                Loop
                Call cpcore.db.csClose(CS)
            End If
        End Sub
        '
        Public Sub markRecordReviewed(ContentName As String, RecordID As Integer)
            Try
                If Models.Complex.cdefModel.isContentFieldSupported(cpcore, ContentName, "DateReviewed") Then
                    Dim DataSourceName As String = Models.Complex.cdefModel.getContentDataSource(cpcore, ContentName)
                    Dim TableName As String = Models.Complex.cdefModel.getContentTablename(cpcore, ContentName)
                    Dim SQL As String = "update " & TableName & " set DateReviewed=" & cpcore.db.encodeSQLDate(cpcore.doc.profileStartTime)
                    If Models.Complex.cdefModel.isContentFieldSupported(cpcore, ContentName, "ReviewedBy") Then
                        SQL &= ",ReviewedBy=" & cpcore.doc.authContext.user.id
                    End If
                    '
                    ' -- Mark the live record
                    Call cpcore.db.executeQuery(SQL & " where id=" & RecordID, DataSourceName)
                End If
            Catch ex As Exception
                cpcore.handleException(ex)
            End Try
        End Sub
        '
        '=============================================================================
        '   Sets the MetaContent subsystem so the next call to main_GetLastMeta... returns the correct value
        '       And neither takes much time
        '=============================================================================
        '
        Public Sub setMetaContent(ByVal ContentID As Integer, ByVal RecordID As Integer)
            Dim KeywordList As String = String.Empty
            Dim CS As Integer
            Dim Criteria As String
            Dim SQL As String
            Dim FieldList As String
            Dim iContentID As Integer
            Dim iRecordID As Integer
            Dim MetaContentID As Integer
            '
            iContentID = genericController.EncodeInteger(ContentID)
            iRecordID = genericController.EncodeInteger(RecordID)
            If (iContentID <> 0) And (iRecordID <> 0) Then
                '
                ' main_Get ID, Description, Title
                '
                Criteria = "(ContentID=" & iContentID & ")and(RecordID=" & iRecordID & ")"
                If False Then '.3.550" Then
                    FieldList = "ID,Name,MetaDescription,'' as OtherHeadTags,'' as MetaKeywordList"
                ElseIf False Then '.3.930" Then
                    FieldList = "ID,Name,MetaDescription,OtherHeadTags,'' as MetaKeywordList"
                Else
                    FieldList = "ID,Name,MetaDescription,OtherHeadTags,MetaKeywordList"
                End If
                CS = cpcore.db.csOpen("Meta Content", Criteria, , , , ,, FieldList)
                If cpcore.db.csOk(CS) Then
                    MetaContentID = cpcore.db.csGetInteger(CS, "ID")
                    Call cpcore.html.addTitle(genericController.encodeHTML(cpcore.db.csGetText(CS, "Name")), "page content")
                    Call cpcore.html.addMetaDescription(genericController.encodeHTML(cpcore.db.csGetText(CS, "MetaDescription")), "page content")
                    Call cpcore.html.addHeadTag(cpcore.db.csGetText(CS, "OtherHeadTags"), "page content")
                    If True Then
                        KeywordList = genericController.vbReplace(cpcore.db.csGetText(CS, "MetaKeywordList"), vbCrLf, ",")
                    End If
                    'main_MetaContent_Title = encodeHTML(app.csv_cs_getText(CS, "Name"))
                    'htmldoc.main_MetaContent_Description = encodeHTML(app.csv_cs_getText(CS, "MetaDescription"))
                    'main_MetaContent_OtherHeadTags = app.csv_cs_getText(CS, "OtherHeadTags")
                End If
                Call cpcore.db.csClose(CS)
                '
                ' main_Get Keyword List
                '
                SQL = "select ccMetaKeywords.Name" _
                    & " From ccMetaKeywords" _
                    & " LEFT JOIN ccMetaKeywordRules on ccMetaKeywordRules.MetaKeywordID=ccMetaKeywords.ID" _
                    & " Where ccMetaKeywordRules.MetaContentID=" & MetaContentID
                CS = cpcore.db.csOpenSql(SQL)
                Do While cpcore.db.csOk(CS)
                    KeywordList = KeywordList & "," & cpcore.db.csGetText(CS, "Name")
                    Call cpcore.db.csGoNext(CS)
                Loop
                If KeywordList <> "" Then
                    If Left(KeywordList, 1) = "," Then
                        KeywordList = Mid(KeywordList, 2)
                    End If
                    'KeyWordList = Mid(KeyWordList, 2)
                    KeywordList = genericController.encodeHTML(KeywordList)
                    Call cpcore.html.addMetaKeywordList(KeywordList, "page content")
                End If
                Call cpcore.db.csClose(CS)
            End If
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Clear all data from the metaData current instance. Next request will load from cache.
        ''' </summary>
        Public Sub clearMetaData()
            Try
                If (Not cpcore.doc.cdefDictionary Is Nothing) Then
                    cdefDictionary.Clear()
                End If
                If (Not tableSchemaDictionary Is Nothing) Then
                    tableSchemaDictionary.Clear()
                End If
                contentNameIdDictionaryClear()
                contentIdDictClear()
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
        End Sub

    End Class
End Namespace
