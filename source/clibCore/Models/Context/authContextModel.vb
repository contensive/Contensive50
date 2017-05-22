
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Context
    '
    '====================================================================================================
    ''' <summary>
    ''' authentication context
    ''' </summary>
    Public Class authContextModel
        '
        ' -- the visit is the collection of pages, constructor creates default non-authenticated instance
        Public visit As Models.Entity.visitModel
        '
        ' -- visitor represents the browser, constructor creates default non-authenticated instance
        Public visitor As Models.Entity.visitorModel
        '
        ' -- user is the person at the keyboad, constructor creates default non-authenticated instance
        Public user As Models.Entity.personModel
        '
        ' -- legacy user object -- will be refactored out, constructor creates default non-authenticated instance
        'Public authContextUser As authContextUserModel
        '
        ' -- is this user authenticated in this visit
        Public ReadOnly Property isAuthenticated As Boolean
            Get
                Return visit.VisitAuthenticated
            End Get
        End Property
        '
        ' -- legacy
        Public visit_initialized As Boolean = False                ' true when visit has been initialized
        Public visit_browserIsIE As Boolean = False          ' if detail includes msie
        Public visit_browserIsNS As Boolean = False          ' if detail or detailtail is netscape
        Public visit_browserVersion As String = ""
        Public visit_browserIsWindows As Boolean = False    ' if any browser detail includes "windows"
        Public visit_browserIsMac As Boolean = False        ' if any browser deail includes "mac"
        Public visit_browserIsLinux As Boolean = False      ' not sure
        Public visit_browserIsMobile As Boolean = False     ' if a WAP Mobile device
        Public visit_isBot As Boolean = False               '
        Public visit_isBadBot As Boolean = False            '
        Public visit_stateOK As Boolean = False             ' if false, page is out of state (sequence)
        '
        Private contentAccessRights_NotList As String = ""                  ' If ContentId in this list, they are not a content manager
        Private contentAccessRights_List As String = ""                     ' If ContentId in this list, they are a content manager
        Private contentAccessRights_AllowAddList As String = ""             ' If in _List, test this for allowAdd
        Private contentAccessRights_AllowDeleteList As String = ""          ' If in _List, test this for allowDelete
        '
        Public main_IsEditingContentList As String = ""
        Public main_IsNotEditingContentList As String = ""
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor, no arguments, created default authentication model for use without user, and before user is available
        ''' </summary>
        Public Sub New()
            visit = New Models.Entity.visitModel()
            visitor = New Models.Entity.visitorModel()
            user = New Models.Entity.personModel()
            'authContextUser = New authContextUserModel()
        End Sub
        '
        '========================================================================
        '
        Public Shared Function create(cpCore As coreClass, ByVal visitInit_allowVisitTracking As Boolean) As authContextModel
            Dim resultAuthContext As authContextModel = Nothing
            Try
                Dim NeedToWriteVisitCookie As Boolean
                Dim TrackGuests As Boolean
                Dim DefaultMemberName As String
                Dim AllowOnNewVisitEvent As Boolean
                Dim CS As Integer
                Dim visit_lastTimeFromCookie As Date
                Dim SlashPosition As Integer
                Dim MemberLinkinEID As String
                Dim MemberLinkLoginID As Integer
                Dim MemberLinkRecognizeID As Integer
                Dim CookieVisitNew As String
                Dim CookieVisit As String
                Dim CookieVisitor As String
                Dim WorkingReferer As String
                Dim tokenDate As Date
                Dim main_appNameCookiePrefix As String = genericController.vbLCase(cpCore.main_encodeCookieName(cpCore.serverConfig.appConfig.name))
                Dim visit_changes As Boolean = False
                Dim visitor_changes As Boolean = False
                Dim user_changes As Boolean = False
                '
                resultAuthContext = New authContextModel
                resultAuthContext.visit = New Models.Entity.visitModel
                resultAuthContext.visitor = New Models.Entity.visitorModel
                resultAuthContext.user = New Models.Entity.personModel
                '
                visit_lastTimeFromCookie = Date.MinValue
                resultAuthContext.visit.ID = 0
                resultAuthContext.visit.PageVisits = 0
                resultAuthContext.visit.LoginAttempts = 0
                resultAuthContext.visit_stateOK = True
                resultAuthContext.visit.VisitAuthenticated = False
                resultAuthContext.visit.ExcludeFromAnalytics = False
                resultAuthContext.visit.CookieSupport = False
                resultAuthContext.visit.VisitorNew = False
                resultAuthContext.visit.MemberNew = False
                visit_changes = False
                '
                resultAuthContext.visitor.ID = 0
                visitor_changes = False
                '
                resultAuthContext.user.ID = 0
                resultAuthContext.user.Name = "Guest"
                resultAuthContext.user.StyleFilename = ""
                resultAuthContext.user.ExcludeFromAnalytics = False
                user_changes = False
                '
                CookieVisit = cpCore.webServer.getRequestCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit)
                MemberLinkinEID = cpCore.docProperties.getText("eid")
                MemberLinkLoginID = 0
                MemberLinkRecognizeID = 0
                If (MemberLinkinEID <> "") Then
                    If cpCore.siteProperties.getBoolean("AllowLinkLogin", True) Then
                        '
                        ' Link Login
                        '
                        Call cpCore.security.decodeToken(MemberLinkinEID, MemberLinkLoginID, tokenDate)
                    ElseIf cpCore.siteProperties.getBoolean("AllowLinkRecognize", True) Then
                        '
                        ' Link Recognize
                        '
                        Call cpCore.security.decodeToken(MemberLinkinEID, MemberLinkRecognizeID, tokenDate)
                    Else
                        '
                        ' block link login
                        '
                        MemberLinkinEID = ""
                    End If
                End If
                'hint = "200"
                If (visitInit_allowVisitTracking) Or (CookieVisit <> "") Or (MemberLinkLoginID <> 0) Or (MemberLinkRecognizeID <> 0) Then
                    '
                    ' ----- try cookie main_VisitId
                    '
                    'hint = "210"
                    Dim cookieVisitId As Integer = 0
                    If (CookieVisit <> "") Then
                        Call cpCore.security.decodeToken(CookieVisit, cookieVisitId, visit_lastTimeFromCookie)
                        If cookieVisitId = 0 Then
                            '
                            ' ----- Bad Cookie, clear it so a new one will be written
                            CookieVisit = ""
                        Else
                            '
                            ' ----- good cookie
                        End If
                    End If
                    '
                    ' ----- Visit is good, read Visit/Visitor
                    '
                    'hint = "220"
                    If (cookieVisitId <> 0) Then
                        With resultAuthContext
                            .visit = Models.Entity.visitModel.create(cpCore, cookieVisitId, New List(Of String))
                            If (.visit Is Nothing) Then
                                '
                                ' -- visit record is missing, create a new visit
                                .visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                            ElseIf .visit.LastVisitTime.ToOADate + 0.041666 < cpCore.app_startTime.ToOADate Then
                                '
                                ' -- visit has expired, create new visit
                                .visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                            Else

                                cpCore.webServer.requestRemoteIP = .visit.REMOTE_ADDR
                                cpCore.webServer.requestBrowser = .visit.Browser
                                .visit.TimeToLastHit = 0
                                If .visit.StartTime > Date.MinValue Then
                                    .visit.TimeToLastHit = CInt((cpCore.app_startTime - .visit.StartTime).TotalSeconds)
                                End If
                                .visit.ExcludeFromAnalytics = .visit.ExcludeFromAnalytics
                                .visit.CookieSupport = True
                                .visit_browserIsMobile = .visit.Mobile
                                .visit_isBot = .visit.Bot
                                '
                                If (.visit.VisitorID > 0) Then
                                    .visitor = Models.Entity.visitorModel.create(cpCore, .visit.VisitorID, New List(Of String))
                                End If
                                If (.visit.MemberID > 0) Then
                                    .user = Models.Entity.personModel.create(cpCore, resultAuthContext.visit.MemberID, New List(Of String))
                                End If

                                If ((visit_lastTimeFromCookie - .visit.LastVisitTime).TotalSeconds) > 2 Then
                                    .visit_stateOK = False
                                End If
                            End If
                        End With
                    End If
                    If (resultAuthContext.visit.ID = 0) Then
                        '
                        ' -- new visit required
                        ' -- Decode Browser User-Agent string to main_VisitName, main_VisitIsBot, main_VisitIsBadBot, etc
                        '
                        Call web_init_decodeBrowserUserAgent(cpCore, resultAuthContext, cpCore.webServer.requestBrowser)
                        '
                        ' -- create new visit record
                        resultAuthContext.visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                        If resultAuthContext.visit.Name = "" Then
                            resultAuthContext.visit.Name = "User"
                        End If
                        resultAuthContext.visit.PageVisits = 0
                        resultAuthContext.visit.StartTime = cpCore.app_startTime
                        resultAuthContext.visit.StartDateValue = CInt(cpCore.app_startTime.ToOADate)
                        resultAuthContext.visit.LastVisitTime = cpCore.app_startTime
                        '
                        ' -- setup referrer
                        If (Not String.IsNullOrEmpty(cpCore.webServer.requestReferrer)) Then
                            WorkingReferer = cpCore.webServer.requestReferrer
                            SlashPosition = genericController.vbInstr(1, WorkingReferer, "//")
                            If (SlashPosition <> 0) And (Len(WorkingReferer) > (SlashPosition + 2)) Then
                                WorkingReferer = Mid(WorkingReferer, SlashPosition + 2)
                            End If
                            SlashPosition = genericController.vbInstr(1, WorkingReferer, "/")
                            If SlashPosition = 0 Then
                                resultAuthContext.visit.RefererPathPage = ""
                                resultAuthContext.visit.HTTP_REFERER = WorkingReferer
                            Else
                                resultAuthContext.visit.RefererPathPage = Mid(WorkingReferer, SlashPosition)
                                resultAuthContext.visit.HTTP_REFERER = Mid(WorkingReferer, 1, SlashPosition - 1)
                            End If
                        End If
                        '
                        ' -- create visitor from cookie
                        CookieVisitor = genericController.encodeText(cpCore.webServer.getRequestCookie(main_appNameCookiePrefix & main_cookieNameVisitor))
                        If cpCore.siteProperties.getBoolean("AllowAutoRecognize", True) Then
                            '
                            ' -- auto recognize, setup user based on visitor
                            Dim cookieVisitorId As Integer = 0
                            Call cpCore.security.decodeToken(CookieVisitor, cookieVisitorId, tokenDate)
                            If cookieVisitorId <> 0 Then
                                '
                                ' -- visitor cookie good
                                resultAuthContext.visitor = Models.Entity.visitorModel.create(cpCore, cookieVisitorId, New List(Of String))
                                If (resultAuthContext.visitor Is Nothing) Then
                                    '
                                    ' -- visitor not found, set to blank model
                                    resultAuthContext.visitor = New Models.Entity.visitorModel
                                End If
                            End If
                        End If
                        '
                        ' -- create new visitor for new visit
                        If resultAuthContext.visitor.id = 0 Then
                            '
                            ' Visitor Fields
                            resultAuthContext.visitor = Models.Entity.visitorModel.add(cpCore, New List(Of String))
                            resultAuthContext.visitor.name = "Visitor " & resultAuthContext.visitor.id
                            resultAuthContext.visitor.memberID = 0
                            resultAuthContext.visitor.orderID = 0
                            resultAuthContext.visitor.forceBrowserMobile = 0
                            visitor_changes = True
                            '
                            resultAuthContext.visit.VisitorNew = True
                            visit_changes = True
                        End If
                        '
                        ' -- find  identity from the visitor
                        If (resultAuthContext.visitor.memberID > 0) Then
                            '
                            ' -- recognize by the main_VisitorMemberID
                            If resultAuthContext.recognizeById(cpCore, resultAuthContext.visitor.memberID) Then
                                '
                                ' -- if successful, now test for autologin (authentication)
                                If (cpCore.siteProperties.AllowAutoLogin And resultAuthContext.user.AutoLogin And resultAuthContext.visit.CookieSupport) Then
                                    '
                                    ' -- they allow it, now Check if they were logged in on their last visit
                                    Dim lastVisit As Models.Entity.visitModel = Models.Entity.visitModel.getLastVisitByVisitor(cpCore, resultAuthContext.visit.ID, resultAuthContext.visitor.id)
                                    If (lastVisit IsNot Nothing) Then
                                        If (lastVisit.VisitAuthenticated And (lastVisit.MemberID = resultAuthContext.visit.ID)) Then
                                            If resultAuthContext.authenticateById(cpCore, resultAuthContext.user.ID) Then
                                                Call cpCore.log_LogActivity2("autologin", resultAuthContext.user.ID, resultAuthContext.user.OrganizationID)
                                                visitor_changes = True
                                                user_changes = True
                                            End If
                                        End If
                                    End If
                                Else
                                    '
                                    ' -- Recognized, not auto login
                                    Call cpCore.log_LogActivity2("recognized", resultAuthContext.user.ID, resultAuthContext.user.OrganizationID)
                                End If
                            End If
                        End If
                        '
                        ' -- new visit, update the persistant visitor cookie
                        If visitInit_allowVisitTracking Then
                            Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & main_cookieNameVisitor, cpCore.security.encodeToken(resultAuthContext.visitor.id, resultAuthContext.visit.StartTime), resultAuthContext.visit.StartTime.AddYears(1), , requestAppRootPath, False)
                        End If
                        '
                        ' -- OnNewVisit Add-on call
                        AllowOnNewVisitEvent = True
                    End If
                    '
                    ' -- Attempt Link-in recognize or login
                    If (MemberLinkLoginID <> 0) Then
                        '
                        ' -- Link Login
                        If resultAuthContext.authenticateById(cpCore, MemberLinkLoginID) Then
                            Call cpCore.log_LogActivity2("link login with eid " & MemberLinkinEID, resultAuthContext.user.ID, resultAuthContext.user.OrganizationID)
                        End If
                    ElseIf (MemberLinkRecognizeID <> 0) Then
                        '
                        ' -- Link Recognize
                        Call resultAuthContext.recognizeById(cpCore, MemberLinkRecognizeID)
                        Call cpCore.log_LogActivity2("link recognize with eid " & MemberLinkinEID, resultAuthContext.user.ID, resultAuthContext.user.OrganizationID)
                    End If
                    '
                    ' -- create guest identity if no identity
                    If (resultAuthContext.user.ID < 1) Then
                        '
                        ' -- No user created
                        If (LCase(Left(resultAuthContext.visit.Name, 5)) <> "visit") Then
                            DefaultMemberName = resultAuthContext.visit.Name
                        Else
                            DefaultMemberName = genericController.encodeText(cpCore.GetContentFieldProperty("people", "name", "default"))
                        End If
                        TrackGuests = cpCore.siteProperties.getBoolean("track guests", False)
                        If Not TrackGuests Then
                            '
                            ' -- do not track guests at all
                            resultAuthContext.user = Models.Entity.personModel.add(cpCore, New List(Of String))
                            resultAuthContext.visitor.memberID = resultAuthContext.user.ID
                            resultAuthContext.visitor.saveObject(cpCore)
                            '
                            resultAuthContext.visit.VisitAuthenticated = False
                            resultAuthContext.visit.saveObject(cpCore)
                            '
                            resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                            resultAuthContext.property_user_isMember_isLoaded = False
                            resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                        Else
                            If resultAuthContext.visit.CookieSupport Then
                                '
                                ' -- cookies supported, not first hit and not spider
                                resultAuthContext.user = Models.Entity.personModel.add(cpCore, New List(Of String))
                                resultAuthContext.visitor.memberID = resultAuthContext.user.ID
                                resultAuthContext.visitor.saveObject(cpCore)
                                '
                                resultAuthContext.visit.VisitAuthenticated = False
                                resultAuthContext.visit.saveObject(cpCore)
                                '
                                resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                resultAuthContext.property_user_isMember_isLoaded = False
                                resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                            Else
                                If cpCore.siteProperties.trackGuestsWithoutCookies Then
                                    '
                                    ' -- create people for non-cookies too
                                    '
                                    resultAuthContext.user = Models.Entity.personModel.add(cpCore, New List(Of String))
                                    resultAuthContext.user.Name = resultAuthContext.visit.Name
                                    resultAuthContext.user.saveObject(cpCore)
                                    resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                    resultAuthContext.property_user_isMember_isLoaded = False
                                    resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                Else
                                    '
                                    ' set defaults for people object, but no record
                                    '
                                    resultAuthContext.user = New Models.Entity.personModel
                                    resultAuthContext.user.Name = DefaultMemberName
                                    resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                    resultAuthContext.property_user_isMember_isLoaded = False
                                    resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                End If
                            End If
                        End If
                    End If
                    '
                    ' -- establish language for the member, if they do not have one
                    If resultAuthContext.visit.PageVisits = 0 Then
                        '
                        ' -- First page of this visit, verify the member language
                        If (resultAuthContext.user.LanguageID < 1) Then
                            '
                            ' -- No member language, set member language from browser language
                            Call cpCore.web_GetBrowserLanguage(resultAuthContext.user.LanguageID, resultAuthContext.user.language)
                            If resultAuthContext.user.LanguageID > 0 Then
                                '
                                ' Browser Language worked
                                '
                                user_changes = True
                            Else
                                '
                                ' Still no match, main_Get the default language
                                '
                                resultAuthContext.user.language = cpCore.siteProperties.getText("Language", "English")
                                If resultAuthContext.user.language <> "English" Then
                                    '
                                    ' Handle the non-English case first, so if there is a problem, fall back is English
                                    '
                                    resultAuthContext.user.LanguageID = cpCore.db.getRecordID("languages", resultAuthContext.user.language)
                                    If resultAuthContext.user.LanguageID = 0 Then
                                        '
                                        ' -- non-English Language is not in Language Table, set default to english
                                        resultAuthContext.user.language = "English"
                                        Call cpCore.siteProperties.setProperty("Language", resultAuthContext.user.language)
                                    End If
                                End If
                                If resultAuthContext.user.language = "English" Then
                                    resultAuthContext.user.LanguageID = cpCore.db.getRecordID("languages", resultAuthContext.user.language)
                                    If resultAuthContext.user.LanguageID < 1 Then
                                        '
                                        ' -- English is not in Language table, add it, and set it in Member
                                        Dim language As Models.Entity.LanguageModel = Models.Entity.LanguageModel.add(cpCore, New List(Of String))
                                        If (language IsNot Nothing) Then
                                            language.Name = "English"
                                            language.HTTP_Accept_Language = "en"
                                            language.saveObject(cpCore)
                                            resultAuthContext.user.LanguageID = language.ID
                                            resultAuthContext.user.language = language.Name
                                            user_changes = True
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' -- Save anything that changed
                    resultAuthContext.visit.ExcludeFromAnalytics = resultAuthContext.visit.ExcludeFromAnalytics Or resultAuthContext.visit_isBot Or resultAuthContext.user.ExcludeFromAnalytics Or resultAuthContext.user.Admin Or resultAuthContext.user.Developer
                    If Not cpCore.webServer.webServerIO_PageExcludeFromAnalytics Then
                        resultAuthContext.visit.PageVisits += 1
                    End If
                    resultAuthContext.visit_initialized = True
                    resultAuthContext.visit.saveObject(cpCore)
                    If visitor_changes Then
                        Call resultAuthContext.visitor.saveObject(cpCore)
                    End If
                    If user_changes Then
                        Call resultAuthContext.user.saveObject(cpCore)
                    End If
                    CookieVisitNew = cpCore.security.encodeToken(resultAuthContext.visit.ID, resultAuthContext.visit.LastVisitTime)
                    If visitInit_allowVisitTracking And (CookieVisit <> CookieVisitNew) Then
                        CookieVisit = CookieVisitNew
                        NeedToWriteVisitCookie = True
                    End If
                End If
                resultAuthContext.visit_initialized = True
                If (AllowOnNewVisitEvent) And (True) Then
                    For Each addon As Models.Entity.addonModel In Models.Entity.addonModel.getAddonList_OnNewVisitEvent(cpCore)
                        Call cpCore.addon.execute_legacy5(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextOnNewVisit, "", 0, "", 0)
                    Next
                End If
                '
                ' -- Write Visit Cookie
                CookieVisit = cpCore.security.encodeToken(resultAuthContext.visit.ID, cpCore.app_startTime)
                Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit, CookieVisit, , , requestAppRootPath, False)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return resultAuthContext
        End Function
        '
        '========================================================================
        '   Browser Detection
        '========================================================================
        '
        Public Shared Sub web_init_decodeBrowserUserAgent(cpcore As coreClass, authContext As authContextModel, BrowserUserAgent As String)
            Try
                '
                Dim visitNameFound As Boolean
                Dim BotList As String
                Dim Bots() As String
                Dim Args() As String
                Dim Ptr As Integer
                Dim Cnt As Integer
                Dim Arg As String
                Dim DateExpires As Date
                Dim Filename As String
                '
                Dim PositionStart As Integer
                Dim PositionEnd As Integer
                Dim Agent As String
                Dim AgentParts() As String
                Dim CompatibleAgent As String
                Dim AgentVersion, AgentMinor As String
                Dim RealAgent As String
                Dim Details As String
                Dim DetailsStart As Integer
                Dim DetailsEnd As Integer
                Dim temp As String
                Dim DetailSections() As String
                Dim DetailCount As Integer
                Dim DetailPointer As Integer
                Dim DetailsVersionSection() As String
                Dim temp3() As String
                Dim Detail As String
                Dim DetailTail As String
                Dim UserAgentSubstrings As String
                Dim Subs() As String
                '
                If True Then
                    Select Case authContext.visitor.forceBrowserMobile
                        Case 1
                            authContext.visit_browserIsMobile = True
                        Case 2
                            authContext.visit_browserIsMobile = False
                        Case Else
                            If cpcore.webServer.requestxWapProfile <> "" Then
                                '
                                ' If x_wap, set mobile true
                                '
                                authContext.visit_browserIsMobile = True
                            ElseIf genericController.vbInstr(1, cpcore.webServer.requestHttpAccept, "wap", vbTextCompare) <> 0 Then
                                '
                                ' If main_HTTP_Accept, set mobile true
                                '
                                authContext.visit_browserIsMobile = True
                            Else
                                '
                                ' If useragent is in the list, set mobile true
                                '
                                UserAgentSubstrings = cpcore.main_GetMobileBrowserList()
                                If UserAgentSubstrings <> "" Then
                                    UserAgentSubstrings = genericController.vbReplace(UserAgentSubstrings, vbCrLf, vbLf)
                                    Subs = Split(UserAgentSubstrings, vbLf)
                                    Cnt = UBound(Subs) + 1
                                    If Cnt > 0 Then
                                        For Ptr = 0 To Cnt - 1
                                            If genericController.vbInstr(1, BrowserUserAgent, Subs(Ptr), vbTextCompare) <> 0 Then
                                                authContext.visit_browserIsMobile = True
                                                Exit For
                                            End If
                                        Next
                                    End If
                                End If
                            End If
                    End Select
                Else
                    authContext.visit_browserIsMobile = False
                End If
                '
                '
                '
                If BrowserUserAgent = "" Then
                    '
                    ' blank browser, Blank-Browser-Bot
                    '
                    authContext.visit.Name = "Blank-Browser-Bot"
                    authContext.visit_isBot = True
                    authContext.visit_isBadBot = False
                Else
                    DetailsStart = genericController.vbInstr(1, BrowserUserAgent, "(")
                    '
                    If DetailsStart = 0 Then
                        '
                        ' no details, either very old, or not IE/NS
                        '
                    Else
                        '
                        '"CompatibleAgent (details) DetailTail" format
                        '
                        Details = Mid(BrowserUserAgent, DetailsStart + 1)
                        DetailsEnd = genericController.vbInstr(1, Details, ")")
                        If DetailsEnd <> 0 Then
                            If Len(Details) > DetailsEnd Then
                                DetailTail = Trim(Mid(Details, DetailsEnd + 1))
                            End If
                            Details = Mid(Details, 1, DetailsEnd - 1)
                        End If
                        CompatibleAgent = Trim(Mid(BrowserUserAgent, 1, DetailsStart - 1))
                        '
                        ' Netscape puts phrase in the DetailTail
                        '
                        PositionStart = genericController.vbInstr(1, DetailTail, "netscape", vbTextCompare)
                        If PositionStart <> 0 Then
                            authContext.visit_browserIsNS = True
                            PositionEnd = genericController.vbInstr(PositionStart, DetailTail, " ")
                            If PositionEnd = 0 Then
                                Agent = Mid(DetailTail, PositionStart)
                            Else
                                Agent = Mid(DetailTail, PositionStart, PositionEnd)
                            End If
                            AgentParts = Split(Agent, "/")
                            If UBound(AgentParts) > 0 Then
                                authContext.visit_browserVersion = Trim(AgentParts(1))
                            End If
                        End If
                        '
                        DetailSections = Split(Details, ";")
                        DetailCount = UBound(DetailSections) + 1
                        For DetailPointer = 0 To DetailCount - 1
                            Detail = Trim(DetailSections(DetailPointer))
                            '
                            If (InStr(1, Detail, "msie", vbTextCompare) >= 0) Then
                                authContext.visit_browserIsIE = True
                                DetailsVersionSection = Split(Trim(Detail), " ")
                                If UBound(DetailsVersionSection) > 0 Then
                                    authContext.visit_browserVersion = Trim(DetailsVersionSection(1))
                                End If
                            ElseIf genericController.vbInstr(1, Details, "netscape", vbTextCompare) <> 0 Then
                                '
                                authContext.visit_browserIsNS = True
                            End If
                            '
                            If genericController.vbInstr(1, Detail, "win", vbTextCompare) <> 0 Then
                                authContext.visit_browserIsWindows = True
                            End If
                            '
                            If genericController.vbInstr(1, Detail, "mac", vbTextCompare) <> 0 Then
                                authContext.visit_browserIsMac = True
                            End If
                            '
                            If genericController.vbInstr(1, Detail, "linux", vbTextCompare) <> 0 Then
                                authContext.visit_browserIsLinux = True
                            End If
                        Next
                    End If
                    '
                    BotList = genericController.encodeText(cpcore.cache.getObject(Of String)("DefaultBotNameList"))
                    If BotList <> "" Then
                        '
                        ' First line of Persistent variant is the expiration date (1 hour in the future)
                        '
                        DateExpires = genericController.EncodeDate(genericController.getLine(BotList))
                        If DateExpires = Date.MinValue Then
                            BotList = ""
                        ElseIf DateExpires < cpcore.app_startTime Then
                            BotList = ""
                        End If
                    End If
                    If BotList = "" Then
                        Filename = "config\VisitNameList.txt"
                        BotList = cpcore.privateFiles.readFile(Filename)
                        If BotList = "" Then
                            BotList = "" _
                            & vbCrLf & "//" _
                            & vbCrLf & "// Default Bot Name list" _
                            & vbCrLf & "// This file is maintained by the server. On the first hit of a visit," _
                            & vbCrLf & "// the default member name is overridden with this name if there is a match" _
                            & vbCrLf & "// in either the user agent or the ipaddress." _
                            & vbCrLf & "// format:  name -tab- browser-user-agent-substring -tab- ip-address-substring -tab- type " _
                            & vbCrLf & "// This text is cached by the server for 1 hour, so changes take" _
                            & vbCrLf & "// effect when the cache expires. It is updated daily from the" _
                            & vbCrLf & "// support site feed. Manual changes may be over written." _
                            & vbCrLf & "// type - r=robot (default), b=bad robot, u=user" _
                            & vbCrLf & "//" _
                            & vbCrLf & "Contensive MonitorContensive Monitor" & vbTab & vbTab & "r" _
                            & vbCrLf & "Google-Bot" & vbTab & "googlebot" & vbTab & vbTab & "r" _
                            & vbCrLf & "MSN-Bot" & vbTab & "msnbot" & vbTab & vbTab & "r" _
                            & vbCrLf & "Yahoo-Bot" & vbTab & "slurp" & vbTab & vbTab & "r" _
                            & vbCrLf & "SearchMe-Bot" & vbTab & "searchme.com" & vbTab & vbTab & "r" _
                            & vbCrLf & "Twiceler-Bot" & vbTab & "www.cuil.com" & vbTab & vbTab & "r" _
                            & vbCrLf & "Unknown Bot" & vbTab & "robot" & vbTab & vbTab & "r" _
                            & vbCrLf & "Unknown Bot" & vbTab & "crawl" & vbTab & vbTab & "r" _
                            & ""
                            Call cpcore.privateFiles.saveFile(Filename, BotList)
                        End If
                        DateExpires = cpcore.app_startTime.AddHours(1)
                        Call cpcore.cache.setObject("DefaultBotNameList", CStr(DateExpires) & vbCrLf & BotList)
                    End If
                    '
                    If BotList <> "" Then
                        BotList = genericController.vbReplace(BotList, vbCrLf, vbLf)
                        Bots = Split(BotList, vbLf)
                        If UBound(Bots) >= 0 Then
                            For Ptr = 0 To UBound(Bots)
                                Arg = Trim(Bots(Ptr))
                                '
                                ' remove comments
                                '
                                If Left(Arg, 2) <> "//" Then
                                    Args = Split(Arg, vbTab)
                                    If UBound(Args) > 0 Then
                                        If Trim(Args(1)) <> "" Then
                                            If genericController.vbInstr(1, BrowserUserAgent, Args(1), vbTextCompare) <> 0 Then
                                                authContext.visit.Name = Args(0)
                                                visitNameFound = True
                                                Exit For
                                            End If
                                        End If
                                        If UBound(Args) > 1 Then
                                            If Trim(Args(2)) <> "" Then
                                                If genericController.vbInstr(1, cpcore.webServer.requestRemoteIP, Args(2), vbTextCompare) <> 0 Then
                                                    authContext.visit.Name = Args(0)
                                                    visitNameFound = True
                                                    Exit For
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Next
                            If visitNameFound Then
                                If UBound(Args) < 3 Then
                                    authContext.visit_isBot = True
                                    authContext.visit_isBadBot = False
                                Else
                                    authContext.visit_isBadBot = (LCase(Args(3)) = "b")
                                    authContext.visit_isBot = authContext.visit_isBadBot Or (LCase(Args(3)) = "r")
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleExceptionAndContinue(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   main_IsAdmin
        '   true if:
        '       Is Authenticated
        '       Is Member
        '       Member has admin or developer status
        '========================================================================
        '
        Public Function isAuthenticatedAdmin(cpCore As coreClass) As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not isAuthenticatedAdmin_cache_isLoaded) And visit_initialized Then
                    isAuthenticatedAdmin_cache = isAuthenticated() And (user.Admin Or user.Developer)
                    isAuthenticatedAdmin_cache_isLoaded = True
                End If
                returnIs = isAuthenticatedAdmin_cache
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        Private isAuthenticatedAdmin_cache As Boolean = False               ' true if member is administrator
        Private isAuthenticatedAdmin_cache_isLoaded As Boolean = False              ' true if main_IsAdminCache is initialized
        '
        '========================================================================
        '   main_IsDeveloper
        '========================================================================
        '
        Public Function isAuthenticatedDeveloper(cpCore As coreClass) As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not isAuthenticatedDeveloper_cache_isLoaded) And visit_initialized Then
                    isAuthenticatedDeveloper_cache = (isAuthenticated() And user.Developer)
                    isAuthenticatedDeveloper_cache_isLoaded = True
                End If
                returnIs = isAuthenticatedDeveloper_cache
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        '
        Private isAuthenticatedDeveloper_cache As Boolean = False
        Private isAuthenticatedDeveloper_cache_isLoaded As Boolean = False
        '
        '========================================================================
        ' main_IsContentManager2
        '   If ContentName is missing, returns true if this is an authenticated member with
        '       content management over anything
        '   If ContentName is given, it only tests this content
        '========================================================================
        '
        Public Function isAuthenticatedContentManager(cpCore As coreClass, Optional ByVal ContentName As String = "") As Boolean
            Dim returnIsContentManager As Boolean = False
            Try
                Dim SQL As String
                Dim CS As Integer
                Dim notImplemented_allowAdd As Boolean
                Dim notImplemented_allowDelete As Boolean
                '
                ' REFACTOR -- add a private dictionary with contentname=>result, plus a authenticationChange flag that makes properties like this invalid
                '
                returnIsContentManager = False
                If String.IsNullOrEmpty(ContentName) Then
                    If isAuthenticated() Then
                        If isAuthenticatedAdmin(cpCore) Then
                            returnIsContentManager = True
                        Else
                            '
                            ' Is a CM for any content def
                            '
                            If (Not _isAuthenticatedContentManagerAnything_loaded) Or (_isAuthenticatedContentManagerAnything_userId <> user.ID) Then
                                SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(user.ID) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                                        & ")"
                                CS = cpCore.db.cs_openSql(SQL)
                                _isAuthenticatedContentManagerAnything = cpCore.db.cs_ok(CS)
                                cpCore.db.cs_Close(CS)
                                '
                                _isAuthenticatedContentManagerAnything_userId = user.ID
                                _isAuthenticatedContentManagerAnything_loaded = True
                            End If
                            returnIsContentManager = _isAuthenticatedContentManagerAnything
                        End If
                    End If
                Else
                    '
                    ' Specific Content called out
                    '
                    Call getContentAccessRights(cpCore, ContentName, returnIsContentManager, notImplemented_allowAdd, notImplemented_allowDelete)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIsContentManager
        End Function
        Private _isAuthenticatedContentManagerAnything_loaded As Boolean = False
        Private _isAuthenticatedContentManagerAnything_userId As Integer
        Private _isAuthenticatedContentManagerAnything As Boolean
        '
        '========================================================================
        ' Member Logout
        '   Create and assign a guest Member identity
        '========================================================================
        '
        Public Sub logout(cpCore As coreClass)
            Try
                Call cpCore.log_LogActivity2("logout", user.ID, user.OrganizationID)
                '
                ' Clear MemberID for this page
                '
                user = Models.Entity.personModel.add(cpCore, New List(Of String))
                '
                visit.VisitAuthenticated = False
                visit.saveObject(cpCore)
                '
                visitor.memberID = user.ID
                visitor.saveObject(cpCore)
                '
                isAuthenticatedAdmin_cache_isLoaded = False
                property_user_isMember_isLoaded = False
                isAuthenticatedDeveloper_cache_isLoaded = False
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        '   Returns the ID of a member given their Username and Password
        '
        '   If the Id can not be found, user errors are added with main_AddUserError and 0 is returned (false)
        '===================================================================================================
        '
        Public Function authenticateGetId(cpCore As coreClass, ByVal username As String, ByVal password As String) As Integer
            Dim returnUserId As Integer = 0
            Try
                Const badLoginUserError As String = "Your login was not successful. Please try again."
                '
                Dim SQL As String
                Dim recordIsAdmin As Boolean
                Dim recordIsDeveloper As Boolean
                Dim Criteria As String
                Dim CS As Integer
                Dim iPassword As String
                Dim allowEmailLogin As Boolean
                Dim allowNoPasswordLogin As Boolean
                Dim iLoginFieldValue As String
                '
                iLoginFieldValue = genericController.encodeText(username)
                iPassword = genericController.encodeText(password)
                '
                returnUserId = 0
                allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin")
                allowNoPasswordLogin = cpCore.siteProperties.getBoolean("allowNoPasswordLogin")
                If iLoginFieldValue = "" Then
                    '
                    ' ----- loginFieldValue blank, stop here
                    '
                    If allowEmailLogin Then
                        Call cpCore.error_AddUserError("A valid login requires a non-blank username or email.")
                    Else
                        Call cpCore.error_AddUserError("A valid login requires a non-blank username.")
                    End If
                ElseIf (Not allowNoPasswordLogin) And (iPassword = "") Then
                    '
                    ' ----- password blank, stop here
                    '
                    Call cpCore.error_AddUserError("A valid login requires a non-blank password.")
                ElseIf (visit.LoginAttempts >= cpCore.siteProperties.maxVisitLoginAttempts) Then
                    '
                    ' ----- already tried 5 times
                    '
                    Call cpCore.error_AddUserError(badLoginUserError)
                Else
                    If allowEmailLogin Then
                        '
                        ' login by username or email
                        '
                        Criteria = "((username=" & cpCore.db.encodeSQLText(iLoginFieldValue) & ")or(email=" & cpCore.db.encodeSQLText(iLoginFieldValue) & "))"
                    Else
                        '
                        ' login by username only
                        '
                        Criteria = "(username=" & cpCore.db.encodeSQLText(iLoginFieldValue) & ")"
                    End If
                    If True Then
                        Criteria = Criteria & "and((dateExpires is null)or(dateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))"
                    End If
                    CS = cpCore.db.cs_open("People", Criteria, "id", SelectFieldList:="ID ,password,admin,developer", PageSize:=2)
                    If Not cpCore.db.cs_ok(CS) Then
                        '
                        ' ----- loginFieldValue not found, stop here
                        '
                        Call cpCore.error_AddUserError(badLoginUserError)
                    ElseIf (Not genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowDuplicateUsernames", False))) And (cpCore.db.cs_getRowCount(CS) > 1) Then
                        '
                        ' ----- AllowDuplicates is false, and there are more then one record
                        '
                        Call cpCore.error_AddUserError("This user account can not be used because the username is not unique on this website. Please contact the site administrator.")
                    Else
                        '
                        ' ----- search all found records for the correct password
                        '
                        Do While cpCore.db.cs_ok(CS)
                            returnUserId = 0
                            '
                            ' main_Get Id if password good
                            '
                            If (iPassword = "") Then
                                '
                                ' no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
                                '
                                recordIsAdmin = cpCore.db.cs_getBoolean(CS, "admin")
                                recordIsDeveloper = Not cpCore.db.cs_getBoolean(CS, "admin")
                                If allowNoPasswordLogin And (cpCore.db.cs_getText(CS, "password") = "") And (Not recordIsAdmin) And (recordIsDeveloper) Then
                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID")
                                    '
                                    ' verify they are in no content manager groups
                                    '
                                    SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(returnUserId) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                                        & ");"
                                    CS = cpCore.db.cs_openSql(SQL)
                                    If cpCore.db.cs_ok(CS) Then
                                        returnUserId = 0
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                End If
                            Else
                                '
                                ' password login
                                '
                                If genericController.vbLCase(cpCore.db.cs_getText(CS, "password")) = genericController.vbLCase(iPassword) Then
                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID")
                                End If
                            End If
                            If returnUserId <> 0 Then
                                Exit Do
                            End If
                            Call cpCore.db.cs_goNext(CS)
                        Loop
                        If returnUserId = 0 Then
                            Call cpCore.error_AddUserError(badLoginUserError)
                        End If
                    End If
                    Call cpCore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnUserId
        End Function
        '
        '====================================================================================================
        '   Checks the username and password for a new login
        '       returns true if this can be used
        '       returns false, and a User Error response if it can not be used
        '
        Public Function isNewLoginOK(cpCore As coreClass, ByVal Username As String, ByVal Password As String, ByRef returnErrorMessage As String, ByRef returnErrorCode As Integer) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim CSPointer As Integer
                '
                returnOk = False
                If Username = "" Then
                    '
                    ' ----- username blank, stop here
                    '
                    returnErrorCode = 1
                    returnErrorMessage = "A valid login requires a non-blank username."
                ElseIf Password = "" Then
                    '
                    ' ----- password blank, stop here
                    '
                    returnErrorCode = 4
                    returnErrorMessage = "A valid login requires a non-blank password."
                    '    ElseIf Not main_VisitCookieSupport Then
                    '        '
                    '        ' No Cookie Support, can not log in
                    '        '
                    '        errorCode = 2
                    '        errorMessage = "You currently have cookie support disabled in your browser. Without cookies, your browser can not support the level of security required to login."
                Else

                    CSPointer = cpCore.db.cs_open("People", "username=" & cpCore.db.encodeSQLText(Username), "", False, SelectFieldList:="ID", PageSize:=2)
                    If cpCore.db.cs_ok(CSPointer) Then
                        '
                        ' ----- username was found, stop here
                        '
                        returnErrorCode = 3
                        returnErrorMessage = "The username you supplied is currently in use."
                    Else
                        returnOk = True
                    End If
                    Call cpCore.db.cs_Close(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        ' main_GetContentAccessRights( ContentIdOrName, returnAllowEdit, returnAllowAdd, returnAllowDelete )
        '
        Public Sub getContentAccessRights(cpCore As coreClass, ByVal ContentName As String, ByRef returnAllowEdit As Boolean, ByRef returnAllowAdd As Boolean, ByRef returnAllowDelete As Boolean)
            Try
                Dim ContentID As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                '
                returnAllowEdit = False
                returnAllowAdd = False
                returnAllowDelete = False
                If True Then
                    If Not isAuthenticated() Then
                        '
                        ' no authenticated, you are not a conent manager
                        '
                    ElseIf String.IsNullOrEmpty(ContentName) Then
                        '
                        ' no content given, do not handle the general case -- use authcontext.user.main_IsContentManager2()
                        '
                    ElseIf isAuthenticatedDeveloper(cpcore) Then
                        '
                        ' developers are always content managers
                        '
                        returnAllowEdit = True
                        returnAllowAdd = True
                        returnAllowDelete = True
                    ElseIf isAuthenticatedAdmin(cpcore) Then
                        '
                        ' admin is content manager if the CDef is not developer only
                        '
                        CDef = cpCore.metaData.getCdef(ContentName)
                        If CDef.Id <> 0 Then
                            If Not CDef.DeveloperOnly Then
                                returnAllowEdit = True
                                returnAllowAdd = True
                                returnAllowDelete = True
                            End If
                        End If
                    Else
                        '
                        ' Authenticated and not admin or developer
                        '
                        ContentID = cpCore.main_GetContentID(ContentName)
                        Call getContentAccessRights_NonAdminByContentId(cpCore, ContentID, returnAllowEdit, returnAllowAdd, returnAllowDelete, "")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ' main_GetContentAccessRights_NonAdminByContentId
        '   Checks if the member is a content manager for the specific content,
        '   Which includes transversing up the tree to find the next rule that applies'
        '   Member must be checked for authenticated and main_IsAdmin already
        '========================================================================
        '
        Private Sub getContentAccessRights_NonAdminByContentId(cpCore As coreClass, ByVal ContentID As Integer, ByRef returnAllowEdit As Boolean, ByRef returnAllowAdd As Boolean, ByRef returnAllowDelete As Boolean, ByVal usedContentIdList As String)
            Try
                Dim SQL As String
                Dim CSPointer As Integer
                Dim ParentID As Integer
                Dim ContentName As String
                Dim CDef As coreMetaDataClass.CDefClass
                '
                returnAllowEdit = False
                returnAllowAdd = False
                returnAllowDelete = False
                If genericController.IsInDelimitedString(usedContentIdList, CStr(ContentID), ",") Then
                    '
                    ' failed usedContentIdList test, this content id was in the child path
                    '
                    Throw New ArgumentException("ContentID [" & ContentID & "] was found to be in it's own parentid path.")
                ElseIf ContentID < 1 Then
                    '
                    ' ----- not a valid contentname
                    '
                ElseIf genericController.IsInDelimitedString(contentAccessRights_NotList, CStr(ContentID), ",") Then
                    '
                    ' ----- was previously found to not be a Content Manager
                    '
                ElseIf genericController.IsInDelimitedString(contentAccessRights_List, CStr(ContentID), ",") Then
                    '
                    ' ----- was previously found to be a Content Manager
                    '
                    returnAllowEdit = True
                    returnAllowAdd = genericController.IsInDelimitedString(contentAccessRights_AllowAddList, CStr(ContentID), ",")
                    returnAllowDelete = genericController.IsInDelimitedString(contentAccessRights_AllowDeleteList, CStr(ContentID), ",")
                Else
                    '
                    ' ----- Must test it
                    '
                    SQL = "SELECT ccGroupRules.ContentID,allowAdd,allowDelete" _
                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                    & " WHERE (" _
                        & " (ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(user.ID) & ")" _
                        & " AND(ccMemberRules.active<>0)" _
                        & " AND(ccGroupRules.active<>0)" _
                        & " AND(ccGroupRules.ContentID=" & ContentID & ")" _
                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                        & ");"
                    CSPointer = cpCore.db.cs_openSql(SQL)
                    If cpCore.db.cs_ok(CSPointer) Then
                        returnAllowEdit = True
                        returnAllowAdd = cpCore.db.cs_getBoolean(CSPointer, "allowAdd")
                        returnAllowDelete = cpCore.db.cs_getBoolean(CSPointer, "allowDelete")
                    End If
                    cpCore.db.cs_Close(CSPointer)
                    '
                    If Not returnAllowEdit Then
                        '
                        ' ----- Not a content manager for this one, check the parent
                        '
                        ContentName = cpCore.metaData.getContentNameByID(ContentID)
                        If ContentName <> "" Then
                            CDef = cpCore.metaData.getCdef(ContentName)
                            ParentID = CDef.parentID
                            If ParentID > 0 Then
                                Call getContentAccessRights_NonAdminByContentId(cpCore, ParentID, returnAllowEdit, returnAllowAdd, returnAllowDelete, usedContentIdList & "," & CStr(ContentID))
                            End If
                        End If
                    End If
                    If returnAllowEdit Then
                        '
                        ' ----- Was found to be true
                        '
                        contentAccessRights_List &= "," & CStr(ContentID)
                        If returnAllowAdd Then
                            contentAccessRights_AllowAddList &= "," & CStr(ContentID)
                        End If
                        If returnAllowDelete Then
                            contentAccessRights_AllowDeleteList &= "," & CStr(ContentID)
                        End If
                    Else
                        '
                        ' ----- Was found to be false
                        '
                        contentAccessRights_NotList &= "," & CStr(ContentID)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Member Login (by username and password)
        '
        '   See main_GetLoginMemberID and main_LoginMemberByID
        '========================================================================
        '
        Public Function authenticate(cpCore As coreClass, ByVal loginFieldValue As String, ByVal password As String, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim LocalMemberID As Integer
                '
                returnREsult = False
                LocalMemberID = authenticateGetId(cpCore, loginFieldValue, password)
                If LocalMemberID <> 0 Then
                    returnREsult = authenticateById(cpCore, LocalMemberID, AllowAutoLogin)
                    If returnREsult Then
                        Call cpCore.log_LogActivity2("successful password login", user.ID, user.OrganizationID)
                        isAuthenticatedAdmin_cache_isLoaded = False
                        property_user_isMember_isLoaded = False
                    Else
                        Call cpCore.log_LogActivity2("unsuccessful login (loginField:" & loginFieldValue & "/password:" & password & ")", user.ID, user.OrganizationID)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   Member Login By ID
        '
        '========================================================================
        '
        Public Function authenticateById(cpCore As coreClass, ByVal irecordID As Integer, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim CS As Integer
                '
                returnREsult = recognizeById(cpCore, irecordID)
                If returnREsult Then
                    '
                    ' Log them in
                    '
                    visit.VisitAuthenticated = True
                    visit.saveObject(cpCore)
                    isAuthenticatedAdmin_cache_isLoaded = False
                    property_user_isMember_isLoaded = False
                    isAuthenticatedDeveloper_cache_isLoaded = False
                    '
                    ' Write Cookies in case Visit Tracking is off
                    '
                    If visit.StartTime = Date.MinValue Then
                        visit.StartTime = cpCore.app_startTime
                    End If
                    If Not cpCore.siteProperties.allowVisitTracking Then
                        cpCore.authContext = Models.Context.authContextModel.create(cpCore, True)
                    End If
                    '
                    ' Change autologin if included, selected, and allowed
                    '
                    If AllowAutoLogin Xor user.AutoLogin Then
                        If cpCore.siteProperties.getBoolean("AllowAutoLogin") Then
                            CS = cpCore.csOpenRecord("people", irecordID)
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "AutoLogin", AllowAutoLogin)
                            End If
                            Call cpCore.db.cs_Close(CS)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   RecognizeMember
        '
        '   the current member to be non-authenticated, but recognized
        '========================================================================
        '
        Public Function recognizeById(cpCore As coreClass, ByVal RecordID As Integer) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim CS As Integer
                Dim SQL As String
                '
                SQL = "select" _
                    & " ccMembers.*" _
                    & " ,ccLanguages.name as LanguageName" _
                    & " from" _
                    & " ccMembers" _
                    & " left join ccLanguages on ccMembers.LanguageID=ccLanguages.ID" _
                    & " where" _
                    & " (ccMembers.active<>" & SQLFalse & ")" _
                    & " and(ccMembers.ID=" & RecordID & ")"
                SQL &= "" _
                    & " and((ccMembers.dateExpires is null)or(ccMembers.dateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))" _
                    & ""
                CS = cpCore.db.cs_openSql(SQL)
                If cpCore.db.cs_ok(CS) Then
                    If visit.ID = 0 Then
                        '
                        ' Visit was blocked during init, init the visit DateTime.Now
                        '
                        cpCore.authContext = Models.Context.authContextModel.create(cpCore, cpCore.siteProperties.allowVisitTracking)
                    End If
                    '
                    ' ----- Member was recognized
                    '   REFACTOR -- when the id is set, the user object is populated, so the rest of this can be removed (verify these are all set in the load
                    '
                    user.ID = (cpCore.db.cs_getInteger(CS, "ID"))
                    user.Name = (cpCore.db.cs_getText(CS, "Name"))
                    user.Username = (cpCore.db.cs_getText(CS, "username"))
                    user.Email = (cpCore.db.cs_getText(CS, "Email"))
                    user.Password = (cpCore.db.cs_getText(CS, "Password"))
                    user.OrganizationID = (cpCore.db.cs_getInteger(CS, "OrganizationID"))
                    user.LanguageID = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    user.Active = (cpCore.db.cs_getBoolean(CS, "Active"))
                    user.Company = (cpCore.db.cs_getText(CS, "Company"))
                    user.Visits = (cpCore.db.cs_getInteger(CS, "Visits"))
                    user.LastVisit = (cpCore.db.cs_getDate(CS, "LastVisit"))
                    user.AllowBulkEmail = (cpCore.db.cs_getBoolean(CS, "AllowBulkEmail"))
                    user.AllowToolsPanel = (cpCore.db.cs_getBoolean(CS, "AllowToolsPanel"))
                    user.AdminMenuModeID = (cpCore.db.cs_getInteger(CS, "AdminMenuModeID"))
                    user.AutoLogin = (cpCore.db.cs_getBoolean(CS, "AutoLogin"))
                    user.Developer = (cpCore.db.cs_getBoolean(CS, "Developer"))
                    user.Admin = (cpCore.db.cs_getBoolean(CS, "Admin"))
                    user.ContentControlID = (cpCore.db.cs_getInteger(CS, "ContentControlID"))
                    user.LanguageID = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    user.language = (cpCore.db.cs_getText(CS, "LanguageName"))
                    user.StyleFilename = cpCore.db.cs_getText(CS, "StyleFilename")
                    If user.StyleFilename <> "" Then
                        Call cpCore.htmlDoc.main_AddStylesheetLink(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, user.StyleFilename))
                    End If
                    user.ExcludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics")
                    '
                    user.Visits = user.Visits + 1
                    If user.Visits = 1 Then
                        visit.MemberNew = True
                    Else
                        visit.MemberNew = False
                    End If
                    user.LastVisit = cpCore.app_startTime
                    'cpCore.main_VisitMemberID = id
                    visit.LoginAttempts = 0
                    visitor.memberID = user.ID
                    visit.ExcludeFromAnalytics = visit.ExcludeFromAnalytics Or visit_isBot Or user.ExcludeFromAnalytics Or user.Admin Or user.Developer
                    Call visit.saveObject(cpCore)
                    Call visitor.saveObject(cpCore)
                    Call user.saveObject(cpCore)
                    returnREsult = True
                End If
                Call cpCore.db.cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group named
        '========================================================================
        '
        Public Function IsMemberOfGroup2(cpCore As coreClass, ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim iMemberID As Integer
                iMemberID = genericController.EncodeInteger(checkMemberID)
                If iMemberID = 0 Then
                    iMemberID = user.ID
                End If
                returnREsult = isMemberOfGroupList(cpCore, "," & cpCore.group_GetGroupID(genericController.encodeText(GroupName)), iMemberID, True)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Returns true if the visitor is a member, and in the group named
        '========================================================================
        '
        Public Function isMemberOfGroup(cpCore As coreClass, ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim iMemberID As Integer
                iMemberID = checkMemberID
                If iMemberID = 0 Then
                    iMemberID = user.ID
                End If
                returnREsult = isMemberOfGroupList(cpCore, "," & cpCore.group_GetGroupID(genericController.encodeText(GroupName)), iMemberID, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function

        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group list
        '========================================================================
        '
        Public Function isMemberOfGroupList(cpCore As coreClass, ByVal GroupIDList As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                If checkMemberID = 0 Then
                    checkMemberID = user.ID
                End If
                returnREsult = isMemberOfGroupIdList(cpCore, checkMemberID, isAuthenticated(), GroupIDList, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   IsMember
        '   true if the user is authenticated and is a trusted people (member content)
        '========================================================================
        '
        Public Function isAuthenticatedMember(cpCore As coreClass) As Boolean
            Dim returnREsult As Boolean = False
            Try
                If (Not property_user_isMember_isLoaded) And (visit_initialized) Then
                    property_user_isMember = isAuthenticated() And cpCore.IsWithinContent(user.ContentControlID, cpCore.main_GetContentID("members"))
                    property_user_isMember_isLoaded = True
                End If
                returnREsult = property_user_isMember
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        Private property_user_isMember As Boolean = False
        Private property_user_isMember_isLoaded As Boolean = False
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '   admins are always returned true
        '===============================================================================================================================
        '
        Public Function isMemberOfGroupIdList(cpCore As coreClass, ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String) As Boolean
            Return isMemberOfGroupIdList(cpCore, MemberID, isAuthenticated, GroupIDList, True)
        End Function
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '===============================================================================================================================
        '
        Public Function isMemberOfGroupIdList(cpCore As coreClass, ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String, ByVal adminReturnsTrue As Boolean) As Boolean
            Dim returnREsult As Boolean = False
            Try
                '
                Dim CS As Integer
                Dim SQL As String
                Dim Criteria As String
                Dim WorkingIDList As String
                '
                returnREsult = False
                If isAuthenticated Then
                    WorkingIDList = GroupIDList
                    WorkingIDList = genericController.vbReplace(WorkingIDList, " ", "")
                    Do While genericController.vbInstr(1, WorkingIDList, ",,") <> 0
                        WorkingIDList = genericController.vbReplace(WorkingIDList, ",,", ",")
                    Loop
                    If (WorkingIDList <> "") Then
                        If vbMid(WorkingIDList, 1) = "," Then
                            If vbLen(WorkingIDList) <= 1 Then
                                WorkingIDList = ""
                            Else
                                WorkingIDList = vbMid(WorkingIDList, 2)
                            End If
                        End If
                    End If
                    If (WorkingIDList <> "") Then
                        If vbRight(WorkingIDList, 1) = "," Then
                            If vbLen(WorkingIDList) <= 1 Then
                                WorkingIDList = ""
                            Else
                                WorkingIDList = genericController.vbMid(WorkingIDList, 1, vbLen(WorkingIDList) - 1)
                            End If
                        End If
                    End If
                    If (WorkingIDList = "") Then
                        If adminReturnsTrue Then
                            '
                            ' check if memberid is admin
                            '
                            SQL = "select top 1 m.id" _
                                & " from ccmembers m" _
                                & " where" _
                                & " (m.id=" & MemberID & ")" _
                                & " and(m.active<>0)" _
                                & " and(" _
                                & " (m.admin<>0)" _
                                & " or(m.developer<>0)" _
                                & " )" _
                                & " "
                            CS = cpCore.db.cs_openCsSql_rev("default", SQL)
                            returnREsult = cpCore.db.cs_ok(CS)
                            Call cpCore.db.cs_Close(CS)
                        End If
                    Else
                        '
                        ' check if they are admin or in the group list
                        '
                        If genericController.vbInstr(1, WorkingIDList, ",") <> 0 Then
                            Criteria = "r.GroupID in (" & WorkingIDList & ")"
                        Else
                            Criteria = "r.GroupID=" & WorkingIDList
                        End If
                        Criteria = "" _
                            & "(" & Criteria & ")" _
                            & " and(r.id is not null)" _
                            & " and((r.DateExpires is null)or(r.DateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))" _
                            & " "
                        If adminReturnsTrue Then
                            Criteria = "(" & Criteria & ")or(m.admin<>0)or(m.developer<>0)"
                        End If
                        Criteria = "" _
                            & "(" & Criteria & ")" _
                            & " and(m.active<>0)" _
                            & " and(m.id=" & MemberID & ")" _
                        '
                        SQL = "select top 1 m.id" _
                            & " from ccmembers m" _
                            & " left join ccMemberRules r on r.Memberid=m.id" _
                            & " where" & Criteria
                        CS = cpCore.db.cs_openCsSql_rev("default", SQL)
                        returnREsult = cpCore.db.cs_ok(CS)
                        Call cpCore.db.cs_Close(CS)
                    End If
                End If

            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' is Guest
        ''' </summary>
        ''' <returns></returns>
        Public Function isGuest(cpCore As coreClass) As Boolean
            Return Not isAuthenticatedMember(cpCore)
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Is Recognized (not new and not authenticted)
        ''' </summary>
        ''' <returns></returns>
        Public Function isRecognized(cpCore As coreClass) As Boolean
            Return Not visit.MemberNew
        End Function
        ''
        ''========================================================================
        '''' <summary>
        '''' authenticated
        '''' </summary>
        '''' <returns></returns>
        'Public Function isAuthenticated() As Boolean
        '    Return visit.VisitAuthenticated
        'End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if editing any content
        ''' </summary>
        ''' <returns></returns>
        Public Function isEditingAnything(cpCore As coreClass) As Boolean
            Return isEditing(cpCore, "")
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' True if editing a specific content
        ''' </summary>
        ''' <param name="ContentNameOrId"></param>
        ''' <returns></returns>
        Public Function isEditing(cpCore As coreClass, ByVal ContentNameOrId As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If True Then
                    Dim localContentNameOrId As String
                    Dim cacheTestName As String
                    '
                    If Not visit_initialized Then
                        Call cpCore.debug_testPoint("...visit not initialized")
                    Else
                        '
                        ' always false until visit loaded
                        '
                        localContentNameOrId = genericController.encodeText(ContentNameOrId)
                        cacheTestName = localContentNameOrId
                        If cacheTestName = "" Then
                            cacheTestName = "iseditingall"
                        End If
                        cacheTestName = genericController.vbLCase(cacheTestName)
                        If genericController.IsInDelimitedString(main_IsEditingContentList, cacheTestName, ",") Then
                            Call cpCore.debug_testPoint("...is in main_IsEditingContentList")
                            returnResult = True
                        ElseIf genericController.IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",") Then
                            Call cpCore.debug_testPoint("...is in main_IsNotEditingContentList")
                        Else
                            If isAuthenticated() Then
                                If Not cpCore.htmlDoc.pageManager_printVersion Then
                                    If (cpCore.visitProperty.getBoolean("AllowEditing") Or cpCore.visitProperty.getBoolean("AllowAdvancedEditor")) Then
                                        If localContentNameOrId <> "" Then
                                            If genericController.vbIsNumeric(localContentNameOrId) Then
                                                localContentNameOrId = cpCore.metaData.getContentNameByID(EncodeInteger(localContentNameOrId))
                                            End If
                                        End If
                                        returnResult = isAuthenticatedContentManager(cpCore, localContentNameOrId)
                                    End If
                                End If
                            End If
                            If returnResult Then
                                main_IsEditingContentList = main_IsEditingContentList & "," & cacheTestName
                            Else
                                main_IsNotEditingContentList = main_IsNotEditingContentList & "," & cacheTestName
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if editing with the quick editor
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function isQuickEditing(cpCore As coreClass, ByVal ContentName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If (Not cpCore.htmlDoc.pageManager_printVersion) Then
                    If isAuthenticatedContentManager(cpCore, ContentName) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowQuickEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' main_IsAdvancedEditing( ContentName )
        ''' <summary>
        ''' true if advanded editing
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function isAdvancedEditing(cpCore As coreClass, ByVal ContentName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If (Not cpCore.htmlDoc.pageManager_printVersion) Then
                    If isAuthenticatedContentManager(cpCore, ContentName) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowAdvancedEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
    End Class
End Namespace