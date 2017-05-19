Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Context
    '
    '====================================================================================================
    ''' <summary>
    ''' Site Properties
    ''' </summary>
    Public Class visitModel
        '
        Private cpCore As coreClass
        '
        Public visit_initialized As Boolean = False                ' true when visit has been initialized
        '
        Public visit_Id As Integer = 0                      ' Index into the visits table
        Public visit_name As String = ""                  '
        Public visit_startDateValue As Integer = 0          ' Long Integer representing date
        Public visit_startTime As Date = Date.MinValue            ' Date/Time when visit started
        Public visit_lastTime As Date = Date.MinValue                ' The date/time when the last page was created
        Public visit_cookieSupport As Boolean = False        ' true if this visit has cookie support
        Public visit_pages As Integer = 0                   ' how many pages have been viewed this visit
        Public visit_referer As String = ""               ' the referer to this site
        Public visit_refererHost As String = ""           '   derived during init
        Public visit_refererPathPage As String = ""       '   derived during init
        Public visit_loginAttempts As Integer = 0           ' number of times this visit/visitor has attempted member-login this visit
        Public visit_isAuthenticated As Boolean = False
        Public visit_browserIsIE As Boolean = False          ' if detail includes msie
        Public visit_browserIsNS As Boolean = False          ' if detail or detailtail is netscape
        Public visit_browserVersion As String = ""
        Public visit_browserIsWindows As Boolean = False    ' if any browser detail includes "windows"
        Public visit_browserIsMac As Boolean = False        ' if any browser deail includes "mac"
        Public visit_browserIsLinux As Boolean = False      ' not sure
        Public visit_browserIsMobile As Boolean = False     ' if a WAP Mobile device
        Public visit_excludeFromAnalytics As Boolean = False ' if true, this visit is excluded from all reporting, like page hit notification, graphic, etc.
        Public visit_isBot As Boolean = False               '
        Public visit_isBadBot As Boolean = False            '
        Public visit_stateOK As Boolean = False             ' if false, page is out of state (sequence)
        Public visit_timeToLastHit As Integer = 0          ' seconds from first hit to last hit
        '
        '====================================================================================================
        ''' <summary>
        ''' new
        ''' </summary>
        Public Sub New()
            '
        End Sub
        '
        '
        '
        '
        '========================================================================
        ' Initilize the Current Page using just the WebClient
        '   main_AllowVisitTracking - if true a cookie will be written and the visit tracked
        '========================================================================
        '
        Public Sub visit_init(ByVal visitInit_allowVisitTracking As Boolean)
            Try
                Dim NeedToWriteVisitCookie As Boolean
                Dim TrackGuests As Boolean
                Dim TrackGuestsWithoutCookies As Boolean
                Dim DefaultMemberName As String
                Dim AllowOnNewVisitEvent As Boolean
                Dim CS As Integer
                'Dim CSNewVisit As Integer
                'Dim CSNewVisitor As Integer
                Dim visitor_changes As Boolean
                Dim user_changes As Boolean
                Dim visit_lastTimeFromCookie As Date
                Dim SQL As String
                Dim SlashPosition As Integer
                Dim MemberLinkinEID As String
                Dim MemberLinkLoginID As Integer
                Dim MemberLinkRecognizeID As Integer
                Dim CookieVisitNew As String
                Dim CookieVisit As String
                Dim CookieVisitor As String
                Dim WorkingReferer As String
                Dim MethodName As String
                Dim Copy As String
                Dim AuthUsername As String
                Dim AuthPassword As String
                Dim AuthDomain As String
                Dim main_appNameCookiePrefix As String
                '
                main_appNameCookiePrefix = genericController.vbLCase(cpCore.main_encodeCookieName(cpCore.serverConfig.appConfig.name))

                ' ----- Visit Defaults
                '
                visit_Id = 0
                visit_pages = 0
                visit_loginAttempts = 0
                visit_lastTimeFromCookie = Date.MinValue
                visit_stateOK = True
                visit_isAuthenticated = False
                visit_excludeFromAnalytics = False
                visit_cookieSupport = False
                '
                ' ----- Visitor Defaults
                '
                'hint = "010"
                'CSNewVisitor = -1
                cpCore.visitor.id = 0
                cpCore.visitor.newVisitor = False
                visitor_changes = False
                '
                ' ----- Member Defaults
                '
                'hint = "020"
                cpCore.user.id = 0
                cpCore.user.name = "Guest"
                cpCore.user.userAdded = False
                cpCore.user.isNew = False
                user_changes = False
                cpCore.user.styleFilename = ""
                cpCore.user.excludeFromAnalytics = False
                '
                ' ----- Decode Browser User-Agent string to main_VisitName, main_VisitIsBot, main_VisitIsBadBot, etc
                '
                ' moved to only run on first page (new visit) hit
                '    Call main_decodeBrowserUserAgent(main_VisitBrowser)
                '    '
                '    ' ----- Determine if Visit Tracking is allowed
                '    '
                '
                'Call AppendLog("main_InitVisit(), 2420")
                '
                Dim tokenDate As Date
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
                        'MemberLinkLoginID = main_DecodeKeyNumber(MemberLinkinEID)
                    ElseIf cpCore.siteProperties.getBoolean("AllowLinkRecognize", True) Then
                        '
                        ' Link Recognize
                        '
                        Call cpCore.security.decodeToken(MemberLinkinEID, MemberLinkRecognizeID, tokenDate)
                        'MemberLinkRecognizeID = main_DecodeKeyNumber(MemberLinkinEID)
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
                    If (CookieVisit <> "") Then
                        Call cpCore.security.decodeToken(CookieVisit, visit_Id, visit_lastTimeFromCookie)
                        'main_VisitId = main_DecodeKeyNumber(CookieVisit)
                        If visit_Id = 0 Then
                            '
                            ' ----- Bad Cookie, clear it so a new one will be written
                            '
                            CookieVisit = ""
                        Else
                            '
                            ' ----- good cookie
                            '
                            'main_VisitLastTimeFromCookie = main_DecodeKeyTime(CookieVisit)
                        End If
                    End If
                    '
                    ' ----- Visit is good, read Visit/Visitor
                    '
                    'hint = "220"
                    If (visit_Id <> 0) Then
                        SQL = "SELECT" _
                            & " ccVisits.ID AS VisitId" _
                            & ",ccVisits.Name AS VisitName" _
                            & ",ccVisits.VisitAuthenticated AS VisitAuthenticated" _
                            & ",ccVisits.StartTime AS VisitStartTime" _
                            & ",ccVisits.StartDateValue AS VisitStartDateValue" _
                            & ",ccVisits.LastVisitTime AS VisitLastVisitTime" _
                            & ",ccVisits.StopTime AS VisitStopTime" _
                            & ",ccVisits.PageVisits AS VisitPageVisits" _
                            & ",ccVisits.CookieSupport AS VisitCookieSupport" _
                            & ",ccVisits.LoginAttempts AS VisitLoginAttempts" _
                            & ",ccVisits.VisitorNew AS VisitVisitorNew" _
                            & ",ccVisits.HTTP_REFERER AS VisitHTTP_REFERER" _
                            & ",ccVisits.REMOTE_ADDR AS VisitREMOTE_ADDR" _
                            & ",ccVisits.Browser AS VisitBrowser" _
                            & ",ccVisits.MemberNew AS VisitMemberNew"
                        SQL &= "" _
                            & ",ccVisitors.ID AS VisitorID" _
                            & ",ccVisitors.Name AS VisitorName" _
                            & ",ccVisitors.MemberID AS VisitorMemberID" _
                            & ",ccVisitors.OrderID AS VisitorOrderID"
                        SQL &= ",ccVisitors.ForceBrowserMobile AS VisitorForceBrowserMobile"
                        SQL &= ",ccVisits.bot AS VisitBot"
                        SQL &= ",ccVisits.mobile AS VisitMobile"
                        SQL &= "" _
                            & ",m.ID AS MemberID" _
                            & ",m.Name AS MemberName" _
                            & ",m.admin AS MemberAdmin" _
                            & ",m.developer AS MemberDeveloper" _
                            & ",m.ContentControlID AS MemberContentControlID" _
                            & ",m.AllowBulkEmail AS MemberAllowBulkEmail" _
                            & ",m.AllowToolsPanel AS MemberAllowToolsPanel" _
                            & ",m.AdminMenuModeID AS MemberAdminMenuModeID" _
                            & ",m.AutoLogin AS MemberAutoLogin" _
                            & ",m.username AS MemberUsername" _
                            & ",m.password AS MemberPassword" _
                            & ",m.LanguageID AS MemberLanguageID" _
                            & ",ccLanguages.name AS MemberLanguage"
                        SQL &= "" _
                            & ",m.OrganizationID AS MemberOrganizationID" _
                            & ",m.Active AS MemberActive" _
                            & ",m.Visits AS MemberVisits" _
                            & ",m.LastVisit AS MemberLastVisit" _
                            & ",m.Company AS MemberCompany" _
                            & ",m.Email AS MemberEmail"
                        SQL &= ",m.StyleFilename as MemberStyleFilename"
                        SQL &= ",m.ExcludeFromAnalytics as MemberExcludeFromAnalytics"
                        SQL &= ",ccvisits.ExcludeFromAnalytics as VisitExcludeFromAnalytics"
                        SQL &= "" _
                            & " FROM ((ccVisits" _
                                & " LEFT JOIN ccVisitors ON ccVisits.VisitorID = ccVisitors.ID)" _
                                & " LEFT JOIN ccMembers as m ON ccVisits.MemberID = m.ID)" _
                                & " LEFT JOIN ccLanguages ON m.LanguageID = ccLanguages.ID" _
                            & " WHERE (((ccVisits.ID)=" & visit_Id & "))"
                        '
                        CS = cpCore.db.cs_openSql(SQL)
                        If Not cpCore.db.cs_ok(CS) Then
                            '
                            ' Bad visit cookie, kill main_VisitId
                            '
                            visit_Id = 0
                            cpCore.visitor.id = 0
                        Else
                            '
                            '--------------------------------------------------------------------------
                            ' ----- Visit found, read visitor info first
                            '--------------------------------------------------------------------------
                            '
                            'hint = "240"
                            cpCore.visitor.id = cpCore.db.cs_getInteger(CS, "VisitorID")
                            cpCore.visitor.name = cpCore.db.cs_getText(CS, "VisitorName")
                            cpCore.visitor.memberID = (cpCore.db.cs_getInteger(CS, "VisitorMemberID"))
                            cpCore.visitor.forceBrowserMobile = (cpCore.db.cs_getInteger(CS, "VisitorForceBrowserMobile"))
                            cpCore.visitor.orderID = (cpCore.db.cs_getInteger(CS, "VisitorOrderID"))
                            '
                            '--------------------------------------------------------------------------
                            ' ----- test visit age
                            '--------------------------------------------------------------------------
                            '
                            'hint = "250"
                            visit_lastTime = cpCore.db.cs_getDate(CS, "VisitLastVisitTime")
                            If visit_lastTime.ToOADate + 0.041666 < cpCore.app_startTime.ToOADate Then
                                '--------------------------------------------------------------------------
                                ' ----- kill visit (no activity for over 1 hour)
                                ' changed time to 60 minutes from 30 minutes - multiple client request (Toll Brothers, etc)
                                '--------------------------------------------------------------------------
                                '
                                'hint = "251"
                                Call cpCore.debug_testPoint("main_InitVisit Last visit was more than an hour old, kill the visit")
                                NeedToWriteVisitCookie = True
                                visit_Id = 0
                                CookieVisit = ""
                            Else
                                '--------------------------------------------------------------------------
                                ' -----  visit OK, capture visit and Member info
                                '--------------------------------------------------------------------------
                                '
                                'hint = "252"
                                visit_Id = (cpCore.db.cs_getInteger(CS, "VisitId"))
                                visit_name = (cpCore.db.cs_getText(CS, "VisitName"))
                                visit_cookieSupport = (cpCore.db.cs_getBoolean(CS, "VisitCookieSupport"))
                                visit_pages = (cpCore.db.cs_getInteger(CS, "VisitPageVisits"))
                                visit_isAuthenticated = (cpCore.db.cs_getBoolean(CS, "VisitAuthenticated"))
                                visit_startTime = (cpCore.db.cs_getDate(CS, "VisitStartTime"))
                                visit_startDateValue = (cpCore.db.cs_getInteger(CS, "VisitStartDateValue"))
                                visit_referer = (cpCore.db.cs_getText(CS, "VisitHTTP_REFERER"))
                                visit_loginAttempts = (cpCore.db.cs_getInteger(CS, "VisitLoginAttempts"))
                                cpCore.visitor.newVisitor = (cpCore.db.cs_getBoolean(CS, "VisitVisitorNew"))
                                '
                                cpCore.webServer.requestRemoteIP = (cpCore.db.cs_getText(CS, "VisitREMOTE_ADDR"))
                                cpCore.webServer.requestBrowser = (cpCore.db.cs_getText(CS, "VisitBrowser"))
                                visit_timeToLastHit = 0
                                If visit_startTime > Date.MinValue Then
                                    visit_timeToLastHit = CInt((cpCore.app_startTime - visit_startTime).TotalSeconds)
                                End If
                                visit_excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "VisitExcludeFromAnalytics")
                                If ((Not visit_cookieSupport) And (CookieVisit <> "")) Then
                                    visit_cookieSupport = True
                                End If
                                visit_browserIsMobile = cpCore.db.cs_getBoolean(CS, "VisitMobile")
                                visit_isBot = cpCore.db.cs_getBoolean(CS, "VisitBot")
                                '
                                '--------------------------------------------------------------------------
                                ' -----  Member info
                                '   20170104 - set id in user object populates the record
                                ' REFACTOR -- this is loading the user twice, when refactored remove these fields from the visit state and just set the user.id
                                '--------------------------------------------------------------------------
                                '
                                Dim testActive As Boolean
                                Dim testId As Integer
                                testActive = cpCore.db.cs_getBoolean(CS, "MemberActive")
                                testId = cpCore.db.cs_getInteger(CS, "MemberID")
                                If ((Not testActive) Or (testId = 0)) Then
                                    cpCore.user.id = 0
                                Else
                                    cpCore.user.id = testId
                                    cpCore.user.active = testActive
                                    cpCore.user.isNew = cpCore.db.cs_getBoolean(CS, "VisitMemberNew")
                                    cpCore.user.name = (cpCore.db.cs_getText(CS, "MemberName"))
                                    cpCore.user.isDeveloper = (cpCore.db.cs_getBoolean(CS, "MemberDeveloper"))
                                    cpCore.user.isAdmin = (cpCore.db.cs_getBoolean(CS, "MemberAdmin"))
                                    cpCore.user.contentControlID = (cpCore.db.cs_getInteger(CS, "MemberContentControlID"))
                                    cpCore.user.allowBulkEmail = (cpCore.db.cs_getBoolean(CS, "MemberAllowBulkEmail"))
                                    cpCore.user.allowToolsPanel = (cpCore.db.cs_getBoolean(CS, "MemberAllowToolsPanel"))
                                    cpCore.user.adminMenuModeID = cpCore.db.cs_getInteger(CS, "MemberAdminMenuModeID")
                                    cpCore.user.autoLogin = (cpCore.db.cs_getBoolean(CS, "MemberAutoLogin"))
                                    cpCore.user.username = (cpCore.db.cs_getText(CS, "MemberUsername"))
                                    cpCore.user.password = (cpCore.db.cs_getText(CS, "MemberPassword"))
                                    cpCore.user.languageId = (cpCore.db.cs_getInteger(CS, "MemberLanguageID"))
                                    cpCore.user.language = (cpCore.db.cs_getText(CS, "MemberLanguage"))
                                    cpCore.user.organizationId = (cpCore.db.cs_getInteger(CS, "MemberOrganizationID"))
                                    cpCore.user.styleFilename = cpCore.db.cs_getText(CS, "MemberStyleFilename")
                                    cpCore.user.excludeFromAnalytics = (cpCore.db.cs_getBoolean(CS, "MemberExcludeFromAnalytics"))
                                    '
                                    ' ----- consider removing
                                    '
                                    cpCore.user.email = (cpCore.db.cs_getText(CS, "MemberEmail"))
                                    cpCore.user.company = (cpCore.db.cs_getText(CS, "MemberCompany"))
                                    cpCore.user.visits = (cpCore.db.cs_getInteger(CS, "MemberVisits"))
                                    cpCore.user.lastVisit = (cpCore.db.cs_getDate(CS, "MemberLastVisit"))
                                End If
                                '
                                '--------------------------------------------------------------------------
                                ' ----- set main_VisitStateOK if Dbase main_VisitLastTime matches main_VisitLastTimeFromCookie
                                '--------------------------------------------------------------------------
                                '
                                'hint = "270"
                                If ((visit_lastTimeFromCookie - visit_lastTime).TotalSeconds) > 2 Then
                                    visit_stateOK = False
                                    cpCore.debug_testPoint("VisitState is false, main_VisitLastTime <> Database main_VisitLastTime, this page is out of order (back button), set main_VisitStateOK false")
                                End If
                            End If
                        End If
                        Call cpCore.db.cs_Close(CS)
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- new visit required
                    '--------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2470")
                    '
                    'hint = "300"
                    ' 1/15/2010
                    If (visit_Id = 0) Then
                        '
                        ' ----- Decode Browser User-Agent string to main_VisitName, main_VisitIsBot, main_VisitIsBadBot, etc
                        '
                        Call cpCore.web_init_decodeBrowserUserAgent(cpCore.webServer.requestBrowser)
                        '
                        ' ----- create new visit record
                        '
                        'hint = "310"
                        visit_Id = cpCore.db.metaData_InsertContentRecordGetID("Visits", cpCore.user.id)
                        If (visit_Id < 1) Then
                            visit_Id = 0
                            cpCore.handleExceptionAndRethrow(New Exception("Internal error, new visit record could not be selected."))
                        End If
                        If visit_name = "" Then
                            visit_name = "User"
                        End If
                        visit_pages = 0
                        visit_startTime = cpCore.app_startTime
                        visit_startDateValue = CInt(cpCore.app_startTime.ToOADate)
                        visit_lastTime = cpCore.app_startTime
                        '
                        ' ----- main_Get visit referer
                        '
                        'hint = "320"
                        If cpCore.webServer.requestReferrer <> "" Then
                            WorkingReferer = cpCore.webServer.requestReferrer
                            SlashPosition = genericController.vbInstr(1, WorkingReferer, "//")
                            If (SlashPosition <> 0) And (Len(WorkingReferer) > (SlashPosition + 2)) Then
                                WorkingReferer = Mid(WorkingReferer, SlashPosition + 2)
                            End If
                            SlashPosition = genericController.vbInstr(1, WorkingReferer, "/")
                            If SlashPosition = 0 Then
                                visit_refererPathPage = ""
                                visit_refererHost = WorkingReferer
                            Else
                                visit_refererPathPage = Mid(WorkingReferer, SlashPosition)
                                visit_refererHost = Mid(WorkingReferer, 1, SlashPosition - 1)
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' ----- create visitor from cookie
                        '--------------------------------------------------------------------------
                        '
                        'hint = "330"
                        CookieVisitor = genericController.encodeText(cpCore.webServer.getRequestCookie(main_appNameCookiePrefix & main_cookieNameVisitor))
                        '
                        'Call AppendLog("main_InitVisit(), 2480")
                        '
                        If cpCore.siteProperties.getBoolean("AllowAutoRecognize", True) Then
                            '
                            'Call AppendLog("main_InitVisit(), 2485")
                            '
                            'hint = "340"
                            Call cpCore.security.decodeToken(CookieVisitor, cpCore.visitor.id, tokenDate)
                            'main_VisitorID = main_DecodeKeyNumber(CookieVisitor)
                            If cpCore.visitor.id <> 0 Then
                                '
                                ' ----- cookie found, open visitor
                                '
                                'hint = "350"
                                visit_cookieSupport = True
                                If True Then
                                    SQL = "SELECT ID,Name,MemberID,OrderID,ForceBrowserMobile from ccVisitors WHERE ID=" & cpCore.visitor.id & ";"
                                Else
                                    SQL = "SELECT ID,Name,MemberID,OrderID,0 as ForceBrowserMobile from ccVisitors WHERE ID=" & cpCore.visitor.id & ";"
                                End If
                                CS = cpCore.db.cs_openSql(SQL)
                                If Not cpCore.db.cs_ok(CS) Then
                                    '
                                    ' ----- bad cookie, kill main_VisitorID
                                    '
                                    cpCore.visitor.id = 0
                                Else
                                    '
                                    ' ----- set visitor values
                                    '
                                    visitor_changes = False
                                    cpCore.visitor.id = (cpCore.db.cs_getInteger(CS, "ID"))
                                    cpCore.visitor.name = (cpCore.db.cs_getText(CS, "Name"))
                                    cpCore.visitor.memberID = (cpCore.db.cs_getInteger(CS, "MemberID"))
                                    cpCore.visitor.forceBrowserMobile = (cpCore.db.cs_getInteger(CS, "ForceBrowserMobile"))
                                    cpCore.visitor.orderID = (cpCore.db.cs_getInteger(CS, "OrderID"))
                                End If
                                Call cpCore.db.cs_Close(CS)
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' ----- create new visitor for new visit
                        '--------------------------------------------------------------------------
                        '
                        'Call AppendLog("main_InitVisit(), 2490")
                        '
                        'hint = "400"
                        If cpCore.visitor.id = 0 Then
                            '
                            ' Visitor Fields
                            '
                            cpCore.visitor.id = cpCore.db.metaData_InsertContentRecordGetID("Visitors", cpCore.user.id)
                            If (cpCore.visitor.id < 1) Then
                                Call cpCore.handleLegacyError14(MethodName, "main_InitVisit, could not create new visitor")
                                cpCore.visitor.id = 0
                            End If
                            cpCore.visitor.name = "Visitor " & cpCore.visitor.id
                            cpCore.visitor.memberID = 0
                            cpCore.visitor.orderID = 0
                            visitor_changes = True
                            cpCore.visitor.forceBrowserMobile = 0
                            '
                            ' Visit Fields
                            '
                            cpCore.visitor.newVisitor = True
                        End If
                        '
                        '-----------------------------------------------------------------------------------
                        ' ----- find  identity from the visitor
                        '-----------------------------------------------------------------------------------
                        '
                        'Call AppendLog("main_InitVisit(), 2492")
                        '
                        'hint = "500"
                        cpCore.user.id = cpCore.visitor.memberID
                        If (cpCore.visitor.memberID > 0) Then
                            '
                            ' ----- recognize by the main_VisitorMemberID
                            '
                            'hint = "510"
                            If cpCore.user.recognizeById(cpCore.visitor.memberID) Then
                                '
                                ' ----- if successful, now test for autologin (authentication)
                                '
                                'hint = "520"

                                If (cpCore.siteProperties.getBoolean("AllowAutoLogin", False)) And (cpCore.user.autoLogin) And visit_cookieSupport Then
                                    '
                                    ' ----- they allow it, now Check if they were logged in on their last visit
                                    '
                                    'hint = "530"
                                    SQL = "select top 1 V.VisitAuthenticated from ccVisits V where (V.ID<>" & visit_Id & ")and(V.VisitorID=" & cpCore.visitor.id & ") order by id desc"
                                    CS = cpCore.db.cs_openSql(SQL)
                                    If cpCore.db.cs_ok(CS) Then
                                        If cpCore.db.cs_getBoolean(CS, "VisitAuthenticated") Then
                                            '
                                            ' ----- yes, go ahead with autologin
                                            '
                                            If cpCore.user.authenticateById(cpCore.user.id) Then
                                                Call cpCore.log_LogActivity2("autologin", cpCore.user.id, cpCore.user.organizationId)
                                                visitor_changes = True
                                                user_changes = True
                                            End If
                                        End If
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                Else
                                    '
                                    ' Recognized, not auto login
                                    '
                                    'hint = "540"
                                    Call cpCore.log_LogActivity2("recognized", cpCore.user.id, cpCore.user.organizationId)
                                End If
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' ----- new visit, update the persistant visitor cookie
                        '--------------------------------------------------------------------------
                        '
                        'hint = "600"
                        If visitInit_allowVisitTracking Then
                            Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & main_cookieNameVisitor, cpCore.security.encodeToken(cpCore.visitor.id, visit_startTime), visit_startTime.AddYears(1), , requestAppRootPath, False)
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' ----- OnNewVisit Add-on call
                        '--------------------------------------------------------------------------
                        '
                        AllowOnNewVisitEvent = True
                    End If
                    '
                    '-----------------------------------------------------------------------------------
                    ' ----- Attempt Link-in recognize or login
                    ' ----- This is allowed even if main_AllowVisitTracking is off
                    '-----------------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2494")
                    '
                    'hint = "700"
                    If (MemberLinkLoginID <> 0) Then
                        '
                        ' Link Login
                        '
                        If cpCore.user.authenticateById(MemberLinkLoginID) Then
                            Call cpCore.log_LogActivity2("link login with eid " & MemberLinkinEID, cpCore.user.id, cpCore.user.organizationId)
                        End If
                    ElseIf (MemberLinkRecognizeID <> 0) Then
                        '
                        ' Link Recognize
                        '
                        Call cpCore.user.recognizeById(MemberLinkRecognizeID)
                        Call cpCore.log_LogActivity2("link recognize with eid " & MemberLinkinEID, cpCore.user.id, cpCore.user.organizationId)
                    End If
                    '
                    '-----------------------------------------------------------------------------------
                    ' ----- create guest identity if no identity
                    '-----------------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2496")
                    '
                    'hint = "800"
                    If (cpCore.user.id < 1) Then
                        '
                        ' No user created
                        '
                        If (LCase(Left(visit_name, 5)) <> "visit") Then
                            DefaultMemberName = visit_name
                        Else
                            DefaultMemberName = genericController.encodeText(cpCore.GetContentFieldProperty("people", "name", "default"))
                        End If
                        If (False) Then
                            '
                            ' not upgraded, just create user like it did before
                            '
                            Call cpCore.user.createUser()
                        Else
                            '
                            ' upgraded, determine the kind of tracking - experimental build set to true
                            '
                            TrackGuests = cpCore.siteProperties.getBoolean("track guests", False)
                            If Not TrackGuests Then
                                '
                                ' do not track guests at all
                                '
                                Call cpCore.user.createUserDefaults(DefaultMemberName)
                            Else
                                If visit_cookieSupport Then
                                    '
                                    ' cookies supported, not first hit and not spider
                                    '
                                    Call cpCore.user.createUser()
                                Else
                                    '
                                    ' upgraded, set it to the site property - experimental build set to true
                                    '
                                    TrackGuestsWithoutCookies = cpCore.siteProperties.getBoolean("track guests without cookies")
                                    If TrackGuestsWithoutCookies Then
                                        '
                                        ' compatibiltiy mode - create people for non-cookies too
                                        '
                                        Call cpCore.user.createUser()
                                    Else
                                        '
                                        ' set defaults for people record
                                        '
                                        Call cpCore.user.createUserDefaults(DefaultMemberName)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    '--------------------------------------------------------------------------
                    ' ----- establish language for the member, if they do not have one
                    '--------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2498")
                    '
                    'hint = "900"
                    If visit_pages = 0 Then
                        '
                        ' First page of this visit, verify the member language
                        '
                        If (cpCore.user.languageId < 1) Or (cpCore.user.language = "") Then
                            '
                            ' No member language, set member language from browser language
                            '
                            Call cpCore.web_GetBrowserLanguage(cpCore.user.languageId, cpCore.user.language)
                            If cpCore.user.languageId > 0 Then
                                '
                                ' Browser Language worked
                                '
                                user_changes = True
                            Else
                                '
                                ' Still no match, main_Get the default language
                                '
                                cpCore.user.language = cpCore.siteProperties.getText("Language", "English")
                                If cpCore.user.language <> "English" Then
                                    '
                                    ' Handle the non-English case first, so if there is a problem, fall back is English
                                    '
                                    CS = cpCore.db.cs_open("Languages", "name=" & cpCore.db.encodeSQLText(cpCore.user.language))
                                    If cpCore.db.cs_ok(CS) Then
                                        cpCore.user.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                        user_changes = True
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                    If cpCore.user.languageId = 0 Then
                                        '
                                        ' non-English Language is not in Language Table, set default to english
                                        '
                                        cpCore.user.language = "English"
                                        Call cpCore.siteProperties.setProperty("Language", cpCore.user.language)
                                    End If
                                End If
                                If cpCore.user.language = "English" Then
                                    CS = cpCore.db.cs_open("Languages", "name=" & cpCore.db.encodeSQLText(cpCore.user.language))
                                    If cpCore.db.cs_ok(CS) Then
                                        cpCore.user.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                        user_changes = True
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                    If cpCore.user.languageId < 1 Then
                                        '
                                        ' English is not in Language table, add it, and set it in Member
                                        '
                                        CS = cpCore.db.cs_insertRecord("Languages")
                                        If cpCore.db.cs_ok(CS) Then
                                            cpCore.user.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                            cpCore.user.language = "English"
                                            Call cpCore.db.cs_set(CS, "Name", cpCore.user.language)
                                            Call cpCore.db.cs_set(CS, "HTTP_Accept_LANGUAGE", "en")
                                            user_changes = True
                                        End If
                                        Call cpCore.db.cs_Close(CS)
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    '-----------------------------------------------------------------------------------
                    ' ----- Save anything that changed
                    '-----------------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2499")
                    '
                    ' can not count main_VisitCookieSupport yet, since a new visit will not show cookie support until the ajax hit
                    'hint = "910"
                    visit_excludeFromAnalytics = visit_excludeFromAnalytics Or visit_isBot Or cpCore.user.excludeFromAnalytics Or cpCore.user.isAdmin Or cpCore.user.isDeveloper
                    '
                    ' Update Page count
                    '
                    If Not cpCore.webServer.webServerIO_PageExcludeFromAnalytics Then
                        visit_pages = visit_pages + 1
                    End If
                    '
                    ' Update the Visit
                    ' set main_visitInitialized true allows main_SaveVisit, main_SaveVisitor, etc to work
                    '
                    visit_initialized = True
                    Call visit_save()
                    '
                    ' ----- Save visitor record
                    '
                    'hint = "940"
                    If visitor_changes Then
                        Call cpCore.visitor.save(cpCore)
                    End If
                    '
                    ' ----- Save Member record
                    '
                    'hint = "950"
                    If user_changes Then
                        Call cpCore.user.saveMemberBase()
                    End If
                    '
                    ' ----- send visit cookie if supported or first page
                    '       no, always send the cookie. There are too many exceptions to try being tricky here.
                    '
                    'hint = "960"
                    CookieVisitNew = cpCore.security.encodeToken(visit_Id, visit_lastTime)
                    'CookieVisitNew = encodeToken(main_VisitId, main_VisitStartTime)
                    If visitInit_allowVisitTracking And (CookieVisit <> CookieVisitNew) Then
                        CookieVisit = CookieVisitNew
                        NeedToWriteVisitCookie = True
                    End If
                End If
                ' set visitinitialized - for the cases where the earlier set was bypassed, like now allowvisittracking
                visit_initialized = True
                'hint = "970"
                If (AllowOnNewVisitEvent) And (True) Then
                    '
                    '---------------------------------------------------------------------------------
                    ' ----- OnNewVisitEvent
                    '---------------------------------------------------------------------------------
                    '
                    'hint = "980"
                    '$$$$$ cache this
                    ' $$$$$ make ptr list on load
                    CS = cpCore.db.cs_open(cnAddons, "(OnNewVisitEvent<>0)", "Name", , , , , "id")
                    Do While cpCore.db.cs_ok(CS)
                        Call cpCore.addon.execute_legacy5(cpCore.db.cs_getInteger(CS, "ID"), "", "", CPUtilsBaseClass.addonContext.ContextOnNewVisit, "", 0, "", 0)
                        cpCore.db.cs_goNext(CS)
                    Loop
                    Call cpCore.db.cs_Close(CS)
                End If
                '
                '---------------------------------------------------------------------------------
                ' ----- Write Visit Cookie
                '---------------------------------------------------------------------------------
                '
                CookieVisit = cpCore.security.encodeToken(visit_Id, cpCore.app_startTime)
                Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit, CookieVisit, , , requestAppRootPath, False)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub


        '
        '=============================================================================
        ' Save Visit
        '   If main_VisitId = 0, create new visit entry and set main_VisitId
        '=============================================================================
        '
        Public Sub visit_save()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveVisit")
            '
            'If Not (true) Then Exit Sub
            '
            Dim SQL As String
            Dim MethodName As String
            ' 'dim buildversion As String
            '
            MethodName = "main_SaveVisit"
            '
            If visit_initialized Then
                '
                ' initialized means the main_InitVisit has completed, so all the other visit values are real
                '
                'BuildVersion = app.dataBuildVersion
                '
                ' ----- set the default Visit Name if nothing else
                '
                If visit_name = "" Then
                    visit_name = "Visit" & visit_Id
                End If
                '
                ' ----- save existing visit
                '
                SQL = "UPDATE ccVisits SET " _
                    & " LastVisitTime=" & cpCore.db.encodeSQLDate(cpCore.app_startTime) _
                    & ",PageVisits=" & (visit_pages) _
                    & ",CookieSupport=" & cpCore.db.encodeSQLBoolean(visit_cookieSupport) _
                    & ",LoginAttempts=" & visit_loginAttempts _
                    & ",VisitAuthenticated=" & cpCore.db.encodeSQLBoolean(visit_isAuthenticated) _
                    & ",MemberID=" & cpCore.user.id _
                    & ",MemberNew=" & cpCore.db.encodeSQLBoolean(cpCore.user.isNew) _
                    & ",TimeToLastHit=" & cpCore.db.encodeSQLNumber(visit_timeToLastHit) _
                    & ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(visit_excludeFromAnalytics) _
                    & ",Mobile=" & cpCore.db.encodeSQLBoolean(visit_browserIsMobile) _
                    & ",Bot=" & cpCore.db.encodeSQLBoolean(visit_isBot Or visit_isBadBot) _
                    & ""
                If visit_pages <= 1 Then
                    '
                    ' First page of the visit, save everything
                    '
                    SQL &= "" _
                        & ",VisitorID=" & cpCore.visitor.id _
                        & ",Name=" & cpCore.db.encodeSQLText(visit_name) _
                        & ",VisitorNew=" & cpCore.db.encodeSQLBoolean(cpCore.visitor.newVisitor) _
                        & ",StartTime=" & cpCore.db.encodeSQLDate(visit_startTime) _
                        & ",StartDateValue=" & visit_startDateValue _
                        & ",DateAdded=" & cpCore.db.encodeSQLDate(visit_startTime) _
                        & ",Remote_Addr=" & cpCore.db.encodeSQLText(cpCore.webServer.requestRemoteIP) _
                        & ",Browser=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestBrowser, 255)) _
                        & ",HTTP_Via=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestHTTPVia, 255)) _
                        & ",HTTP_From=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestHTTPFrom, 255)) _
                        & ",HTTP_REFERER=" & cpCore.db.encodeSQLText(Left(visit_refererHost, 255)) _
                        & ",RefererPathPage=" & cpCore.db.encodeSQLText(Left(visit_refererPathPage, 255)) _
                        & ""
                End If
                SQL &= " WHERE (ID=" & visit_Id & ");"
                Call cpCore.db.executeSql(SQL)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18(MethodName)
            '
        End Sub
    End Class
End Namespace