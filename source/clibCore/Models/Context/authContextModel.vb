
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Models.Context
    '
    '====================================================================================================
    ''' <summary>
    ''' authentication context
    ''' </summary>
    Public Class authContextModel
        '
        ' -- this class stores state, so it can hold a pointer to the cpCore instance
        Private cpCore As coreClass
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
        Public Sub New(cpCore As coreClass)
            Me.cpCore = cpCore
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
                If (cpCore.serverConfig Is Nothing) Then
                    '
                    ' -- application error if no server config
                    cpCore.handleException(New ApplicationException("authorization context cannot be created without a server configuration."))
                Else
                    If (cpCore.serverConfig.appConfig Is Nothing) Then
                        '
                        ' -- no application, this is a server-only call not related to a 
                        resultAuthContext = New authContextModel(cpCore)
                    Else
                        Dim visitCookie_changes As Boolean
                        Dim TrackGuests As Boolean
                        Dim DefaultMemberName As String
                        Dim AllowOnNewVisitEvent As Boolean
                        Dim visitCookieTimestamp As Date
                        Dim SlashPosition As Integer
                        Dim MemberLinkinEID As String
                        Dim MemberLinkLoginID As Integer
                        Dim MemberLinkRecognizeID As Integer
                        Dim visitCookieNew As String
                        Dim visitCookie As String
                        Dim CookieVisitor As String
                        Dim WorkingReferer As String
                        Dim tokenDate As Date
                        Dim visit_changes As Boolean = False
                        Dim visitor_changes As Boolean = False
                        Dim user_changes As Boolean = False
                        Dim main_appNameCookiePrefix As String
                        '
                        main_appNameCookiePrefix = genericController.vbLCase(genericController.main_encodeCookieName(cpCore.serverConfig.appConfig.name))
                        '
                        resultAuthContext = New authContextModel(cpCore)
                        'resultAuthContext.visit = New Models.Entity.visitModel
                        'resultAuthContext.visitor = New Models.Entity.visitorModel
                        'resultAuthContext.user = New Models.Entity.personModel
                        '
                        visitCookieTimestamp = Date.MinValue
                        'resultAuthContext.visit.ID = 0
                        'resultAuthContext.visit.PageVisits = 0
                        'resultAuthContext.visit.LoginAttempts = 0
                        'resultAuthContext.visit_stateOK = True
                        'resultAuthContext.visit.VisitAuthenticated = False
                        'resultAuthContext.visit.ExcludeFromAnalytics = False
                        'resultAuthContext.visit.CookieSupport = False
                        'resultAuthContext.visit.VisitorNew = False
                        'resultAuthContext.visit.MemberNew = False
                        'visit_changes = False
                        '
                        'resultAuthContext.visitor.ID = 0
                        'visitor_changes = False
                        '
                        'resultAuthContext.user.ID = 0
                        'resultAuthContext.user.Name = "Guest"
                        'resultAuthContext.user.StyleFilename = ""
                        'resultAuthContext.user.ExcludeFromAnalytics = False
                        'user_changes = False
                        '
                        visitCookie = cpCore.webServer.getRequestCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit)
                        MemberLinkinEID = cpCore.docProperties.getText("eid")
                        MemberLinkLoginID = 0
                        MemberLinkRecognizeID = 0
                        If (MemberLinkinEID <> "") Then
                            '
                            ' -- attempt link authentication
                            If cpCore.siteProperties.getBoolean("AllowLinkLogin", True) Then
                                '
                                ' -- allow Link Login
                                Call cpCore.security.decodeToken(MemberLinkinEID, MemberLinkLoginID, tokenDate)
                            ElseIf cpCore.siteProperties.getBoolean("AllowLinkRecognize", True) Then
                                '
                                ' -- allow Link Recognize
                                Call cpCore.security.decodeToken(MemberLinkinEID, MemberLinkRecognizeID, tokenDate)
                            Else
                                '
                                ' -- block link login
                                MemberLinkinEID = ""
                            End If
                        End If
                        If (visitInit_allowVisitTracking) Or (visitCookie <> "") Or (MemberLinkLoginID <> 0) Or (MemberLinkRecognizeID <> 0) Then
                            '
                            ' -- Visit Tracking
                            Dim cookieVisitId As Integer = 0
                            If (visitCookie <> "") Then
                                '
                                ' -- visit cookie found
                                Call cpCore.security.decodeToken(visitCookie, cookieVisitId, visitCookieTimestamp)
                                If cookieVisitId = 0 Then
                                    '
                                    ' -- Bad Cookie, clear it so a new one will be written
                                    visitCookie = ""
                                End If
                            End If
                            If (cookieVisitId <> 0) Then
                                '
                                ' -- Visit is good, setup visit, then secondary visitor/user if possible
                                With resultAuthContext
                                    .visit = Models.Entity.visitModel.create(cpCore, cookieVisitId, New List(Of String))
                                    If (.visit Is Nothing) Then
                                        '
                                        ' -- visit record is missing, create a new visit
                                        .visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                                    ElseIf .visit.LastVisitTime.AddHours(1) < cpCore.profileStartTime Then
                                        '
                                        ' -- visit has expired, create new visit
                                        .visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                                    Else
                                        '
                                        ' -- visit object is valid, share its data with other objects
                                        'cpCore.webServer.requestRemoteIP = .visit.REMOTE_ADDR
                                        'cpCore.webServer.requestBrowser = .visit.Browser
                                        .visit.TimeToLastHit = 0
                                        If .visit.StartTime > Date.MinValue Then
                                            .visit.TimeToLastHit = CInt((cpCore.profileStartTime - .visit.StartTime).TotalSeconds)
                                        End If
                                        .visit.CookieSupport = True
                                        If (.visit.VisitorID > 0) Then
                                            '
                                            ' -- try visit's visitor object
                                            Dim testVisitor As Models.Entity.visitorModel = Models.Entity.visitorModel.create(cpCore, .visit.VisitorID, New List(Of String))
                                            If (testVisitor IsNot Nothing) Then
                                                .visitor = testVisitor
                                            End If
                                        End If
                                        If (.visit.MemberID > 0) Then
                                            '
                                            ' -- try visit's person object
                                            Dim testUser As Models.Entity.personModel = Models.Entity.personModel.create(cpCore, resultAuthContext.visit.MemberID, New List(Of String))
                                            If (testUser IsNot Nothing) Then
                                                .user = testUser
                                            End If
                                        End If
                                        If ((visitCookieTimestamp - .visit.LastVisitTime).TotalSeconds) > 2 Then
                                            .visit_stateOK = False
                                        End If
                                    End If
                                End With
                            End If
                            If (resultAuthContext.visit.id = 0) Then
                                '
                                ' -- create new visit record
                                resultAuthContext.visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                                If resultAuthContext.visit.Name = "" Then
                                    resultAuthContext.visit.Name = "User"
                                End If
                                resultAuthContext.visit.PageVisits = 0
                                resultAuthContext.visit.StartTime = cpCore.profileStartTime
                                resultAuthContext.visit.StartDateValue = CInt(cpCore.profileStartTime.ToOADate)
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
                                If (resultAuthContext.visitor.ID = 0) Then
                                    '
                                    ' -- visit.visitor not valid, create visitor from cookie
                                    CookieVisitor = genericController.encodeText(cpCore.webServer.getRequestCookie(main_appNameCookiePrefix & main_cookieNameVisitor))
                                    If cpCore.siteProperties.getBoolean("AllowAutoRecognize", True) Then
                                        '
                                        ' -- auto recognize, setup user based on visitor
                                        Dim cookieVisitorId As Integer = 0
                                        Call cpCore.security.decodeToken(CookieVisitor, cookieVisitorId, tokenDate)
                                        If cookieVisitorId <> 0 Then
                                            '
                                            ' -- visitor cookie good
                                            Dim testVisitor As Models.Entity.visitorModel = Models.Entity.visitorModel.create(cpCore, cookieVisitorId, New List(Of String))
                                            If (testVisitor IsNot Nothing) Then
                                                resultAuthContext.visitor = testVisitor
                                                visitor_changes = True
                                            End If
                                        End If
                                    End If
                                End If
                                '
                                If (resultAuthContext.visitor.ID = 0) Then
                                    '
                                    ' -- create new visitor
                                    resultAuthContext.visitor = Models.Entity.visitorModel.add(cpCore, New List(Of String))
                                    visitor_changes = False
                                    '
                                    resultAuthContext.visit.VisitorNew = True
                                    visit_changes = True
                                End If
                                '
                                ' -- find  identity from the visitor
                                If (resultAuthContext.visitor.MemberID > 0) Then
                                    '
                                    ' -- recognize by the main_VisitorMemberID
                                    If resultAuthContext.recognizeById(cpCore, resultAuthContext.visitor.MemberID, resultAuthContext) Then
                                        '
                                        ' -- if successful, now test for autologin (authentication)
                                        If (cpCore.siteProperties.AllowAutoLogin And resultAuthContext.user.AutoLogin And resultAuthContext.visit.CookieSupport) Then
                                            '
                                            ' -- they allow it, now Check if they were logged in on their last visit
                                            Dim lastVisit As Models.Entity.visitModel = Models.Entity.visitModel.getLastVisitByVisitor(cpCore, resultAuthContext.visit.id, resultAuthContext.visitor.ID)
                                            If (lastVisit IsNot Nothing) Then
                                                If (lastVisit.VisitAuthenticated And (lastVisit.MemberID = resultAuthContext.visit.id)) Then
                                                    If resultAuthContext.authenticateById(cpCore, resultAuthContext.user.id, resultAuthContext) Then
                                                        Call logController.logActivity2(cpCore, "autologin", resultAuthContext.user.id, resultAuthContext.user.OrganizationID)
                                                        visitor_changes = True
                                                        user_changes = True
                                                    End If
                                                End If
                                            End If
                                        Else
                                            '
                                            ' -- Recognized, not auto login
                                            Call logController.logActivity2(cpCore, "recognized", resultAuthContext.user.id, resultAuthContext.user.OrganizationID)
                                        End If
                                    End If
                                End If
                                '
                                ' -- mobile detect
                                Select Case resultAuthContext.visitor.ForceBrowserMobile
                                    Case 1
                                        resultAuthContext.visit.Mobile = True
                                    Case 2
                                        resultAuthContext.visit.Mobile = False
                                    Case Else
                                        If cpCore.webServer.requestxWapProfile <> "" Then
                                            '
                                            ' If x_wap, set mobile true
                                            '
                                            resultAuthContext.visit.Mobile = True
                                        ElseIf genericController.vbInstr(1, cpCore.webServer.requestHttpAccept, "wap", vbTextCompare) <> 0 Then
                                            '
                                            ' If main_HTTP_Accept, set mobile true
                                            '
                                            resultAuthContext.visit.Mobile = True
                                        Else
                                            '
                                            ' If useragent is in the list, set mobile true
                                            '
                                            Dim UserAgentSubstrings As String = main_GetMobileBrowserList(cpCore)
                                            Dim userAgentList As New List(Of String)
                                            If UserAgentSubstrings <> "" Then
                                                UserAgentSubstrings = genericController.vbReplace(UserAgentSubstrings, vbCrLf, vbLf)
                                                userAgentList.AddRange(UserAgentSubstrings.Split(CChar(vbLf)))
                                                For Each userAgent As String In userAgentList
                                                    If (cpCore.webServer.requestBrowser.IndexOf(userAgent) > 0) Then
                                                        resultAuthContext.visit.Mobile = True
                                                        Exit For
                                                    End If
                                                Next
                                            End If
                                        End If
                                End Select
                                If cpCore.webServer.requestBrowser = "" Then
                                    '
                                    ' blank browser, Blank-Browser-Bot
                                    '
                                    resultAuthContext.visit.Name = "Blank-Browser-Bot"
                                    resultAuthContext.visit.Bot = True
                                    resultAuthContext.visit_isBadBot = False
                                End If
                                '
                                ' -- detect bot list
                                Dim botFileContent As String = cpCore.cache.getObject(Of String)("DefaultBotNameList")
                                If botFileContent = "" Then
                                    Dim Filename As String = "config\VisitNameList.txt"
                                    botFileContent = cpCore.privateFiles.readFile(Filename)
                                    If botFileContent = "" Then
                                        botFileContent = "" _
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
                                        Call cpCore.privateFiles.saveFile(Filename, botFileContent)
                                    End If
                                    Call cpCore.cache.setObject("DefaultBotNameList", botFileContent, Now.AddHours(1), New List(Of String))
                                End If
                                '
                                If botFileContent <> "" Then
                                    botFileContent = genericController.vbReplace(botFileContent, vbCrLf, vbLf)
                                    Dim botList As New List(Of String)
                                    botList.AddRange(botFileContent.Split(CChar(vbLf)))
                                    resultAuthContext.visit.Bot = False
                                    resultAuthContext.visit_isBadBot = False
                                    For Each Arg As String In botList
                                        If Left(Arg, 2) <> "//" Then
                                            Dim Args As String() = Split(Arg, vbTab)
                                            If UBound(Args) > 0 Then
                                                If Trim(Args(1)) <> "" Then
                                                    If genericController.vbInstr(1, cpCore.webServer.requestBrowser, Args(1), vbTextCompare) <> 0 Then
                                                        resultAuthContext.visit.Name = Args(0)
                                                        'visitNameFound = True
                                                        Exit For
                                                    End If
                                                End If
                                                If UBound(Args) > 1 Then
                                                    If Trim(Args(2)) <> "" Then
                                                        If genericController.vbInstr(1, cpCore.webServer.requestRemoteIP, Args(2), vbTextCompare) <> 0 Then
                                                            resultAuthContext.visit.Name = Args(0)
                                                            'visitNameFound = True
                                                            Exit For
                                                        End If
                                                    End If
                                                    If UBound(Args) <= 2 Then
                                                        resultAuthContext.visit.Bot = True
                                                        resultAuthContext.visit_isBadBot = False
                                                    Else
                                                        resultAuthContext.visit_isBadBot = (LCase(Args(3)) = "b")
                                                        resultAuthContext.visit.Bot = resultAuthContext.visit_isBadBot Or (LCase(Args(3)) = "r")
                                                    End If
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                '
                                ' -- new visit, update the persistant visitor cookie
                                If visitInit_allowVisitTracking Then
                                    Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & main_cookieNameVisitor, cpCore.security.encodeToken(resultAuthContext.visitor.ID, resultAuthContext.visit.StartTime), resultAuthContext.visit.StartTime.AddYears(1), , requestAppRootPath, False)
                                End If
                                '
                                ' -- OnNewVisit Add-on call
                                AllowOnNewVisitEvent = True
                            End If
                            resultAuthContext.visit.LastVisitTime = cpCore.profileStartTime
                            '
                            ' -- verify visitor
                            If (resultAuthContext.visitor.ID = 0) Then
                                '
                                ' -- create new visitor
                                resultAuthContext.visitor = Models.Entity.visitorModel.add(cpCore, New List(Of String))
                                visitor_changes = False
                                '
                                resultAuthContext.visit.VisitorNew = True
                                visit_changes = True
                            End If
                            '
                            ' -- Attempt Link-in recognize or login
                            If (MemberLinkLoginID <> 0) Then
                                '
                                ' -- Link Login
                                If resultAuthContext.authenticateById(cpCore, MemberLinkLoginID, resultAuthContext) Then
                                    Call logController.logActivity2(cpCore, "link login with eid " & MemberLinkinEID, resultAuthContext.user.id, resultAuthContext.user.OrganizationID)
                                End If
                            ElseIf (MemberLinkRecognizeID <> 0) Then
                                '
                                ' -- Link Recognize
                                Call resultAuthContext.recognizeById(cpCore, MemberLinkRecognizeID, resultAuthContext)
                                Call logController.logActivity2(cpCore, "link recognize with eid " & MemberLinkinEID, resultAuthContext.user.id, resultAuthContext.user.OrganizationID)
                            End If
                            '
                            ' -- create guest identity if no identity
                            If (resultAuthContext.user.id < 1) Then
                                '
                                ' -- No user created
                                If (LCase(Left(resultAuthContext.visit.Name, 5)) <> "visit") Then
                                    DefaultMemberName = resultAuthContext.visit.Name
                                Else
                                    DefaultMemberName = genericController.encodeText(cpCore.metaData.GetContentFieldProperty("people", "name", "default"))
                                End If
                                'resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                'resultAuthContext.property_user_isMember_isLoaded = False
                                'resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                TrackGuests = cpCore.siteProperties.getBoolean("track guests", False)
                                If Not TrackGuests Then
                                    '
                                    ' -- do not track guests at all
                                    resultAuthContext.user = New Models.Entity.personModel
                                    resultAuthContext.user.Name = DefaultMemberName
                                    user_changes = False
                                    resultAuthContext.visitor.MemberID = 0
                                    visitor_changes = True
                                    resultAuthContext.visit.VisitAuthenticated = False
                                    resultAuthContext.visit.MemberID = 0
                                    visit_changes = True
                                    '
                                Else
                                    If resultAuthContext.visit.CookieSupport Then
                                        '
                                        ' -- cookies supported, not first hit and not spider
                                        resultAuthContext.user = Models.Entity.personModel.add(cpCore, New List(Of String))
                                        user_changes = True
                                        resultAuthContext.visitor.MemberID = resultAuthContext.user.id
                                        visitor_changes = True
                                        resultAuthContext.visit.VisitAuthenticated = False
                                        visit_changes = True
                                    Else
                                        If cpCore.siteProperties.trackGuestsWithoutCookies Then
                                            '
                                            ' -- create people for non-cookies too
                                            '
                                            resultAuthContext.user = Models.Entity.personModel.add(cpCore, New List(Of String))
                                            resultAuthContext.user.Name = resultAuthContext.visit.Name
                                            user_changes = True
                                        Else
                                            '
                                            ' set defaults for people object, but no record
                                            '
                                            resultAuthContext.user = New Models.Entity.personModel
                                            resultAuthContext.user.Name = DefaultMemberName
                                            user_changes = True
                                        End If
                                    End If
                                End If
                            End If
                            '
                            ' -- establish language for the member, if they do not have one
                            If (resultAuthContext.visit.PageVisits = 0) And (resultAuthContext.user.id > 0) Then
                                '
                                ' -- First page of this visit, verify the member language
                                If (resultAuthContext.user.LanguageID < 1) Then
                                    '
                                    ' -- No member language, set member language from browser language
                                    Call cpCore.db.web_GetBrowserLanguage(resultAuthContext.user.LanguageID, resultAuthContext.user.language)
                                    If resultAuthContext.user.LanguageID > 0 Then
                                        '
                                        ' -- Browser Language worked
                                        user_changes = True
                                    Else
                                        '
                                        ' -- Still no match, main_Get the default language
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
                                            user_changes = True
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
                            ' -- check for changes in interrelationships
                            If (resultAuthContext.visitor.MemberID <> resultAuthContext.user.id) Then
                                resultAuthContext.visitor.MemberID = resultAuthContext.user.id
                                visitor_changes = True
                            End If
                            If (resultAuthContext.visit.MemberID <> resultAuthContext.user.id) Then
                                resultAuthContext.visit.MemberID = resultAuthContext.user.id
                                visit_changes = True
                            End If
                            If (resultAuthContext.visit.VisitorID <> resultAuthContext.visitor.ID) Then
                                resultAuthContext.visit.VisitorID = resultAuthContext.visitor.ID
                                visit_changes = True
                            End If
                            '
                            ' -- Save anything that changed
                            resultAuthContext.visit.ExcludeFromAnalytics = resultAuthContext.visit.ExcludeFromAnalytics Or resultAuthContext.visit.Bot Or resultAuthContext.user.ExcludeFromAnalytics Or resultAuthContext.user.Admin Or resultAuthContext.user.Developer
                            If Not cpCore.webServer.pageExcludeFromAnalytics Then
                                resultAuthContext.visit.PageVisits += 1
                                visit_changes = True
                            End If
                            resultAuthContext.visit_initialized = True
                            If visit_changes Then resultAuthContext.visit.saveObject(cpCore)
                            If visitor_changes Then Call resultAuthContext.visitor.saveObject(cpCore)
                            If user_changes Then Call resultAuthContext.user.saveObject(cpCore)
                            visitCookieNew = cpCore.security.encodeToken(resultAuthContext.visit.id, resultAuthContext.visit.LastVisitTime)
                            If visitInit_allowVisitTracking And (visitCookie <> visitCookieNew) Then
                                visitCookie = visitCookieNew
                                visitCookie_changes = True
                            End If
                        End If
                        resultAuthContext.visit_initialized = True
                        If (AllowOnNewVisitEvent) And (True) Then
                            For Each addon As addonModel In addonModel.createList_OnNewVisitEvent(cpCore, New List(Of String))
                                Call cpCore.addon.execute_legacy5(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextOnNewVisit, "", 0, "", 0)
                            Next
                        End If
                        '
                        ' -- Write Visit Cookie
                        visitCookie = cpCore.security.encodeToken(resultAuthContext.visit.id, cpCore.profileStartTime)
                        Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit, visitCookie, , , requestAppRootPath, False)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return resultAuthContext
        End Function
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
            Dim result As Boolean = False
            Try
                result = visit.VisitAuthenticated And (user.Admin Or user.Developer)
                'If (Not isAuthenticatedAdmin_cache_isLoaded) And visit_initialized Then
                '    isAuthenticatedAdmin_cache = isAuthenticated() And (user.Admin Or user.Developer)
                '    isAuthenticatedAdmin_cache_isLoaded = True
                'End If
                'result = isAuthenticatedAdmin_cache
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        'Private isAuthenticatedAdmin_cache As Boolean = False               ' true if member is administrator
        'Private isAuthenticatedAdmin_cache_isLoaded As Boolean = False              ' true if main_IsAdminCache is initialized
        '
        '========================================================================
        '   main_IsDeveloper
        '========================================================================
        '
        Public Function isAuthenticatedDeveloper(cpCore As coreClass) As Boolean
            Dim result As Boolean = False
            Try
                result = visit.VisitAuthenticated And (user.Admin Or user.Developer)
                'If (Not isAuthenticatedDeveloper_cache_isLoaded) And visit_initialized Then
                '    isAuthenticatedDeveloper_cache = (isAuthenticated() And user.Developer)
                '    isAuthenticatedDeveloper_cache_isLoaded = True
                'End If
                'result = isAuthenticatedDeveloper_cache
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        '
        'Private isAuthenticatedDeveloper_cache As Boolean = False
        'Private isAuthenticatedDeveloper_cache_isLoaded As Boolean = False
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
                            If (Not _isAuthenticatedContentManagerAnything_loaded) Or (_isAuthenticatedContentManagerAnything_userId <> user.id) Then
                                SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(user.id) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.profileStartTime) & "))" _
                                        & ")"
                                CS = cpCore.db.cs_openSql(SQL)
                                _isAuthenticatedContentManagerAnything = cpCore.db.cs_ok(CS)
                                cpCore.db.cs_Close(CS)
                                '
                                _isAuthenticatedContentManagerAnything_userId = user.id
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
                cpCore.handleException(ex) : Throw
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
                Call logController.logActivity2(cpCore, "logout", user.id, user.OrganizationID)
                '
                ' Clear MemberID for this page
                '
                user = Models.Entity.personModel.add(cpCore, New List(Of String))
                '
                visit.VisitAuthenticated = False
                visit.saveObject(cpCore)
                '
                visitor.MemberID = user.id
                visitor.saveObject(cpCore)
                '
                'isAuthenticatedAdmin_cache_isLoaded = False
                'property_user_isMember_isLoaded = False
                'isAuthenticatedDeveloper_cache_isLoaded = False
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                        Call errorController.error_AddUserError(cpCore, "A valid login requires a non-blank username or email.")
                    Else
                        Call errorController.error_AddUserError(cpCore, "A valid login requires a non-blank username.")
                    End If
                ElseIf (Not allowNoPasswordLogin) And (iPassword = "") Then
                    '
                    ' ----- password blank, stop here
                    '
                    Call errorController.error_AddUserError(cpCore, "A valid login requires a non-blank password.")
                ElseIf (visit.LoginAttempts >= cpCore.siteProperties.maxVisitLoginAttempts) Then
                    '
                    ' ----- already tried 5 times
                    '
                    Call errorController.error_AddUserError(cpCore, badLoginUserError)
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
                        Call errorController.error_AddUserError(cpCore, badLoginUserError)
                    ElseIf (Not genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowDuplicateUsernames", False))) And (cpCore.db.cs_getRowCount(CS) > 1) Then
                        '
                        ' ----- AllowDuplicates is false, and there are more then one record
                        '
                        Call errorController.error_AddUserError(cpCore, "This user account can not be used because the username is not unique on this website. Please contact the site administrator.")
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
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.profileStartTime) & "))" _
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
                            Call errorController.error_AddUserError(cpCore, badLoginUserError)
                        End If
                    End If
                    Call cpCore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                cpCore.handleException(ex) : Throw
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
                Dim CDef As cdefModel
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
                    ElseIf isAuthenticatedDeveloper(cpCore) Then
                        '
                        ' developers are always content managers
                        '
                        returnAllowEdit = True
                        returnAllowAdd = True
                        returnAllowDelete = True
                    ElseIf isAuthenticatedAdmin(cpCore) Then
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
                        ContentID = cpCore.metaData.getContentId(ContentName)
                        Call getContentAccessRights_NonAdminByContentId(cpCore, ContentID, returnAllowEdit, returnAllowAdd, returnAllowDelete, "")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                Dim CDef As cdefModel
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
                        & " (ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(user.id) & ")" _
                        & " AND(ccMemberRules.active<>0)" _
                        & " AND(ccGroupRules.active<>0)" _
                        & " AND(ccGroupRules.ContentID=" & ContentID & ")" _
                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.profileStartTime) & "))" _
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
                cpCore.handleException(ex) : Throw
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
                    returnREsult = authenticateById(cpCore, LocalMemberID, Me)
                    If returnREsult Then
                        Call logController.logActivity2(cpCore, "successful password login", user.id, user.OrganizationID)
                        'isAuthenticatedAdmin_cache_isLoaded = False
                        'property_user_isMember_isLoaded = False
                    Else
                        Call logController.logActivity2(cpCore, "unsuccessful login (loginField:" & loginFieldValue & "/password:" & password & ")", user.id, user.OrganizationID)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   Member Login By ID
        '
        '========================================================================
        '
        Public Function authenticateById(cpCore As coreClass, ByVal userId As Integer, authContext As authContextModel) As Boolean
            Dim returnResult As Boolean = False
            Try
                returnResult = recognizeById(cpCore, userId, authContext)
                If returnResult Then
                    '
                    ' Log them in
                    '
                    authContext.visit.VisitAuthenticated = True
                    If authContext.visit.StartTime = Date.MinValue Then
                        authContext.visit.StartTime = cpCore.profileStartTime
                    End If
                    authContext.visit.saveObject(cpCore)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        '   RecognizeMember
        '
        '   the current member to be non-authenticated, but recognized
        '========================================================================
        '
        Public Function recognizeById(cpCore As coreClass, ByVal userId As Integer, ByRef authContext As authContextModel) As Boolean
            Dim returnResult As Boolean = False
            Try
                If authContext.visitor.ID = 0 Then
                    authContext.visitor = Models.Entity.visitorModel.add(cpCore, New List(Of String))
                End If
                If authContext.visit.id = 0 Then
                    authContext.visit = Models.Entity.visitModel.add(cpCore, New List(Of String))
                End If
                authContext.user = Models.Entity.personModel.create(cpCore, userId, New List(Of String))
                authContext.visitor.MemberID = authContext.user.id
                authContext.visit.MemberID = authContext.user.id
                authContext.visit.VisitAuthenticated = False
                authContext.visit.VisitorID = authContext.visitor.ID
                authContext.visit.LoginAttempts = 0                '
                authContext.user.Visits = authContext.user.Visits + 1
                If authContext.user.Visits = 1 Then
                    authContext.visit.MemberNew = True
                Else
                    authContext.visit.MemberNew = False
                End If
                authContext.user.LastVisit = cpCore.profileStartTime
                authContext.visit.ExcludeFromAnalytics = visit.ExcludeFromAnalytics Or authContext.visit.Bot Or authContext.user.ExcludeFromAnalytics Or authContext.user.Admin Or authContext.user.Developer
                Call authContext.visit.saveObject(cpCore)
                Call authContext.visitor.saveObject(cpCore)
                Call authContext.user.saveObject(cpCore)
                returnResult = True
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
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
                    iMemberID = user.id
                End If
                returnREsult = isMemberOfGroupList(cpCore, "," & groupController.group_GetGroupID(cpCore, genericController.encodeText(GroupName)), iMemberID, True)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                    iMemberID = user.id
                End If
                returnREsult = isMemberOfGroupList(cpCore, "," & groupController.group_GetGroupID(cpCore, genericController.encodeText(GroupName)), iMemberID, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                    checkMemberID = user.id
                End If
                returnREsult = isMemberOfGroupIdList(cpCore, checkMemberID, isAuthenticated(), GroupIDList, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
            Dim result As Boolean = False
            Try
                result = visit.VisitAuthenticated And (cpCore.metaData.isWithinContent(user.ContentControlID, cpCore.metaData.getContentId("members")))
                'If (Not property_user_isMember_isLoaded) And (visit_initialized) Then
                '    property_user_isMember = isAuthenticated() And cpCore.IsWithinContent(user.ContentControlID, cpCore.main_GetContentID("members"))
                '    property_user_isMember_isLoaded = True
                'End If
                'result = property_user_isMember
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
        'Private property_user_isMember As Boolean = False
        'Private property_user_isMember_isLoaded As Boolean = False
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
                cpCore.handleException(ex) : Throw
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
        Public Function isEditingAnything() As Boolean
            Return isEditing("")
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' True if editing a specific content
        ''' </summary>
        ''' <param name="ContentNameOrId"></param>
        ''' <returns></returns>
        Public Function isEditing(ByVal ContentNameOrId As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                Dim localContentNameOrId As String
                Dim cacheTestName As String
                localContentNameOrId = genericController.encodeText(ContentNameOrId)
                cacheTestName = localContentNameOrId
                If cacheTestName = "" Then
                    cacheTestName = "iseditingall"
                End If
                cacheTestName = genericController.vbLCase(cacheTestName)
                If genericController.IsInDelimitedString(main_IsEditingContentList, cacheTestName, ",") Then
                    '
                    ' -- 
                    returnResult = True
                ElseIf genericController.IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",") Then
                    '
                    ' -- 
                    'Call debugController.debug_testPoint(cpCore, "...is in main_IsNotEditingContentList")
                Else
                    If isAuthenticated() Then
                        If Not cpcore.doc.isPrintVersion Then
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
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                If (Not cpcore.doc.isPrintVersion) Then
                    If isAuthenticatedContentManager(cpCore, ContentName) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowQuickEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                If (Not cpcore.doc.isPrintVersion) Then
                    If isAuthenticatedContentManager(cpCore, ContentName) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowAdvancedEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '
        '
        Public Shared Function main_GetMobileBrowserList(cpCore As coreClass) As String
            Dim result As String = ""
            Dim Filename As String
            Dim datetext As String
            '
            result = genericController.encodeText(cpCore.cache.getObject(Of String)("MobileBrowserList"))
            If result <> "" Then
                datetext = genericController.getLine(result)
                If genericController.EncodeDate(datetext) < Now() Then
                    result = ""
                End If
            End If
            If result = "" Then
                Filename = "config\MobileBrowserList.txt"
                result = cpCore.privateFiles.readFile(Filename)
                If result = "" Then
                    result = "midp,j2me,avantg,docomo,novarra,palmos,palmsource,240x320,opwv,chtml,pda,windows ce,mmp/,blackberry,mib/,symbian,wireless,nokia,hand,mobi,phone,cdm,up.b,audio,SIE-,SEC-,samsung,HTC,mot-,mitsu,sagem,sony,alcatel,lg,erics,vx,NEC,philips,mmm,xx,panasonic,sharp,wap,sch,rover,pocket,benq,java,pt,pg,vox,amoi,bird,compal,kg,voda,sany,kdd,dbt,sendo,sgh,gradi,jb,moto"
                    result = genericController.vbReplace(result, ",", vbCrLf)
                    'Call app.publicFiles.SaveFile(Filename, result)
                End If
                datetext = DateTime.Now.AddHours(1).ToString
                Call cpCore.cache.setObject("MobileBrowserList", datetext & vbCrLf & result)
            End If
            Return result
        End Function
        '
        '   Checks the username and password
        '
        Public Function isLoginOK(cpcore As coreClass, ByVal Username As String, ByVal Password As String, Optional ByVal ErrorMessage As String = "", Optional ByVal ErrorCode As Integer = 0) As Boolean
            Dim result As Boolean = (authenticateGetId(cpcore, Username, Password) <> 0)
            If Not result Then
                ErrorMessage = errorController.error_GetUserError(cpcore)
            End If
            Return result
        End Function
        '
        ' ================================================================================================
        '   conversion pass 2
        ' ================================================================================================
        '
        Public Function main_GetAuthoringStatusMessage(cpcore As coreClass, ByVal IsContentWorkflowAuthoring As Boolean, ByVal RecordEditLocked As Boolean, ByVal main_EditLockName As String, ByVal main_EditLockExpires As Date, ByVal RecordApproved As Boolean, ByVal ApprovedBy As String, ByVal RecordSubmitted As Boolean, ByVal SubmittedBy As String, ByVal RecordDeleted As Boolean, ByVal RecordInserted As Boolean, ByVal RecordModified As Boolean, ByVal ModifiedBy As String) As String
            Dim result As String = ""
            result = ""
            '
            Dim MethodName As String
            Dim Copy As String
            Dim Delimiter As String = ""
            Dim main_EditLockExpiresMinutes As Integer
            '
            MethodName = "result"
            '
            main_EditLockExpiresMinutes = CInt((main_EditLockExpires - cpcore.profileStartTime).TotalMinutes)
            If Not cpcore.siteProperties.allowWorkflowAuthoring Then
                '
                ' ----- site does not support workflow authoring
                '
                If RecordEditLocked Then
                    Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                    result &= Delimiter & Copy
                    Delimiter = "<BR >"
                End If
                result &= Delimiter & Msg_WorkflowDisabled
                Delimiter = "<BR >"
            ElseIf Not IsContentWorkflowAuthoring Then
                '
                ' ----- content does not support workflow authoring
                '
                If RecordEditLocked Then
                    Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                    result &= Delimiter & Copy
                    Delimiter = "<BR >"
                End If
                result &= Delimiter & Msg_ContentWorkflowDisabled
                Delimiter = "<BR >"
            Else
                '
                ' ----- Workflow Authoring is supported, check deleted, inserted or modified
                '
                If RecordApproved Then
                    '
                    ' Approved
                    '
                    If isAuthenticatedAdmin(cpcore) Then
                        Copy = genericController.vbReplace(Msg_AuthoringApprovedAdmin, "<EDITNAME>", ApprovedBy)
                        result &= Delimiter & Copy
                        Delimiter = "<BR >"
                    Else
                        Copy = genericController.vbReplace(Msg_AuthoringApproved, "<EDITNAME>", ApprovedBy)
                        result &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                ElseIf RecordSubmitted Then
                    '
                    ' Submitted
                    '
                    If isAuthenticatedAdmin(cpcore) Then
                        Copy = genericController.vbReplace(Msg_AuthoringSubmittedAdmin, "<EDITNAME>", SubmittedBy)
                        result &= Delimiter & Copy
                        Delimiter = "<BR >"
                    Else
                        Copy = genericController.vbReplace(Msg_AuthoringSubmitted, "<EDITNAME>", SubmittedBy)
                        result &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                ElseIf RecordDeleted Then
                    '
                    ' deleted
                    '
                    result &= Delimiter & Msg_AuthoringDeleted
                    Delimiter = "<BR >"
                ElseIf RecordInserted Then
                    '
                    ' inserted
                    '
                    result &= Delimiter & Msg_AuthoringInserted
                    Delimiter = "<BR >"
                ElseIf RecordModified Then
                    '
                    ' modified, submitted or approved
                    '
                    If isAuthenticatedAdmin(cpcore) Then
                        If RecordEditLocked Then
                            Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                            result &= Delimiter & Copy
                            Delimiter = "<BR >"
                        End If
                        Copy = genericController.vbReplace(Msg_AuthoringRecordModifedAdmin, "<EDITNAME>", ModifiedBy)
                        result &= Delimiter & Copy
                        'result &=  Delimiter & Msg_AuthoringRecordModifedAdmin
                        Delimiter = "<BR >"
                    Else
                        If RecordEditLocked Then
                            Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                            result &= Delimiter & Copy
                            Delimiter = "<BR >"
                        End If
                        Copy = genericController.vbReplace(Msg_AuthoringRecordModifed, "<EDITNAME>", ModifiedBy)
                        result &= Delimiter & Copy
                        'result &=  Delimiter & Msg_AuthoringRecordModifed
                        Delimiter = "<BR >"
                    End If
                End If
                '
                ' ----- Check for authoring status messages if it has been modified
                '
                If result = "" Then
                    '
                    ' no changes
                    '
                    If RecordEditLocked Then
                        Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                        Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                        Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                        result &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                    result &= Delimiter & Msg_AuthoringRecordNotModifed
                    Delimiter = "<BR >"
                End If
            End If
            Return result
        End Function
        '
        '========================================================================
        ' main_IsWorkflowRendering()
        '   True if the current visitor is a content manager in workflow rendering mode
        '========================================================================
        '
        Public Function isWorkflowRendering() As Boolean
            Dim result As Boolean = False
            Try
                If isAuthenticatedContentManager(cpCore) Then
                    isWorkflowRendering = cpCore.visitProperty.getBoolean("AllowWorkflowRendering")
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
    End Class
End Namespace