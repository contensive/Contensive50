
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
        Public authContextUser As authContextUserModel
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
        '====================================================================================================
        ''' <summary>
        ''' constructor, no arguments, created default authentication model for use without user, and before user is available
        ''' </summary>
        Public Sub New()
            visit = New Models.Entity.visitModel()
            visitor = New Models.Entity.visitorModel()
            user = New Models.Entity.personModel()
            authContextUser = New authContextUserModel()
        End Sub
        '
        '========================================================================
        '
        Public Shared Function create(cpCore As coreClass, ByVal visitInit_allowVisitTracking As Boolean) As authContextModel
            Dim resultAuthContext As authContextModel = Nothing
            Try
                Dim NeedToWriteVisitCookie As Boolean
                Dim TrackGuests As Boolean
                Dim TrackGuestsWithoutCookies As Boolean
                Dim DefaultMemberName As String
                Dim AllowOnNewVisitEvent As Boolean
                Dim CS As Integer
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
                Dim tokenDate As Date
                Dim main_appNameCookiePrefix As String = genericController.vbLCase(cpCore.main_encodeCookieName(cpCore.serverConfig.appConfig.name))
                '
                resultAuthContext = New authContextModel
                resultAuthContext.visit = New Models.Entity.visitModel
                resultAuthContext.visitor = New Models.Entity.visitorModel
                resultAuthContext.authContextUser = New authContextUserModel()
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
                '
                visitor_changes = False
                resultAuthContext.visitor.id = 0
                resultAuthContext.visitor.newVisitor = False
                '
                user_changes = False
                resultAuthContext.authContextUser.id = 0
                resultAuthContext.authContextUser.name = "Guest"
                resultAuthContext.authContextUser.userAdded = False
                resultAuthContext.authContextUser.isNew = False
                resultAuthContext.authContextUser.styleFilename = ""
                resultAuthContext.authContextUser.excludeFromAnalytics = False
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
                    If (CookieVisit <> "") Then
                        Call cpCore.security.decodeToken(CookieVisit, resultAuthContext.visit.ID, visit_lastTimeFromCookie)
                        'main_VisitId = main_DecodeKeyNumber(CookieVisit)
                        If resultAuthContext.visit.ID = 0 Then
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
                    If (resultAuthContext.visit.ID <> 0) Then
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
                            & " WHERE (((ccVisits.ID)=" & resultAuthContext.visit.ID & "))"
                        '
                        CS = cpCore.db.cs_openSql(SQL)
                        If Not cpCore.db.cs_ok(CS) Then
                            '
                            ' Bad visit cookie, kill main_VisitId
                            '
                            resultAuthContext.visit.ID = 0
                            resultAuthContext.visitor.id = 0
                        Else
                            '
                            '--------------------------------------------------------------------------
                            ' ----- Visit found, read visitor info first
                            '--------------------------------------------------------------------------
                            '
                            'hint = "240"
                            resultAuthContext.visitor.id = cpCore.db.cs_getInteger(CS, "VisitorID")
                            resultAuthContext.visitor.name = cpCore.db.cs_getText(CS, "VisitorName")
                            resultAuthContext.visitor.memberID = (cpCore.db.cs_getInteger(CS, "VisitorMemberID"))
                            resultAuthContext.visitor.forceBrowserMobile = (cpCore.db.cs_getInteger(CS, "VisitorForceBrowserMobile"))
                            resultAuthContext.visitor.orderID = (cpCore.db.cs_getInteger(CS, "VisitorOrderID"))
                            '
                            '--------------------------------------------------------------------------
                            ' ----- test visit age
                            '--------------------------------------------------------------------------
                            '
                            'hint = "250"
                            resultAuthContext.visit.LastVisitTime = cpCore.db.cs_getDate(CS, "VisitLastVisitTime")
                            If resultAuthContext.visit.LastVisitTime.ToOADate + 0.041666 < cpCore.app_startTime.ToOADate Then
                                '--------------------------------------------------------------------------
                                ' ----- kill visit (no activity for over 1 hour)
                                ' changed time to 60 minutes from 30 minutes - multiple client request (Toll Brothers, etc)
                                '--------------------------------------------------------------------------
                                '
                                'hint = "251"
                                Call cpCore.debug_testPoint("main_InitVisit Last visit was more than an hour old, kill the visit")
                                NeedToWriteVisitCookie = True
                                resultAuthContext.visit.ID = 0
                                CookieVisit = ""
                            Else
                                '--------------------------------------------------------------------------
                                ' -----  visit OK, capture visit and Member info
                                '--------------------------------------------------------------------------
                                '
                                'hint = "252"
                                resultAuthContext.visit.ID = (cpCore.db.cs_getInteger(CS, "VisitId"))
                                resultAuthContext.visit.Name = (cpCore.db.cs_getText(CS, "VisitName"))
                                resultAuthContext.visit.CookieSupport = (cpCore.db.cs_getBoolean(CS, "VisitCookieSupport"))
                                resultAuthContext.visit.PageVisits = (cpCore.db.cs_getInteger(CS, "VisitPageVisits"))
                                resultAuthContext.visit.VisitAuthenticated = (cpCore.db.cs_getBoolean(CS, "VisitAuthenticated"))
                                resultAuthContext.visit.StartTime = (cpCore.db.cs_getDate(CS, "VisitStartTime"))
                                resultAuthContext.visit.StartDateValue = (cpCore.db.cs_getInteger(CS, "VisitStartDateValue"))
                                resultAuthContext.visit.HTTP_REFERER = (cpCore.db.cs_getText(CS, "VisitHTTP_REFERER"))
                                resultAuthContext.visit.LoginAttempts = (cpCore.db.cs_getInteger(CS, "VisitLoginAttempts"))
                                resultAuthContext.visitor.newVisitor = (cpCore.db.cs_getBoolean(CS, "VisitVisitorNew"))
                                '
                                cpCore.webServer.requestRemoteIP = (cpCore.db.cs_getText(CS, "VisitREMOTE_ADDR"))
                                cpCore.webServer.requestBrowser = (cpCore.db.cs_getText(CS, "VisitBrowser"))
                                resultAuthContext.visit.TimeToLastHit = 0
                                If resultAuthContext.visit.StartTime > Date.MinValue Then
                                    resultAuthContext.visit.TimeToLastHit = CInt((cpCore.app_startTime - resultAuthContext.visit.StartTime).TotalSeconds)
                                End If
                                resultAuthContext.visit.ExcludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "VisitExcludeFromAnalytics")
                                If ((Not resultAuthContext.visit.CookieSupport) And (CookieVisit <> "")) Then
                                    resultAuthContext.visit.CookieSupport = True
                                End If
                                resultAuthContext.visit_browserIsMobile = cpCore.db.cs_getBoolean(CS, "VisitMobile")
                                resultAuthContext.visit_isBot = cpCore.db.cs_getBoolean(CS, "VisitBot")
                                '
                                '--------------------------------------------------------------------------
                                ' -----  Member info
                                '   20170104 - set id in user object populates the record
                                ' REFACTOR -- this is loading the user twice, when refactored remove these fields from the visit state and just set the authcontext.user.id
                                '--------------------------------------------------------------------------
                                '
                                Dim testActive As Boolean
                                Dim testId As Integer
                                testActive = cpCore.db.cs_getBoolean(CS, "MemberActive")
                                testId = cpCore.db.cs_getInteger(CS, "MemberID")
                                If ((Not testActive) Or (testId = 0)) Then
                                    resultAuthContext.authContextUser.id = 0
                                Else
                                    resultAuthContext.authContextUser.id = testId
                                    resultAuthContext.authContextUser.active = testActive
                                    resultAuthContext.authContextUser.isNew = cpCore.db.cs_getBoolean(CS, "VisitMemberNew")
                                    resultAuthContext.authContextUser.name = (cpCore.db.cs_getText(CS, "MemberName"))
                                    resultAuthContext.authContextUser.isDeveloper = (cpCore.db.cs_getBoolean(CS, "MemberDeveloper"))
                                    resultAuthContext.authContextUser.isAdmin = (cpCore.db.cs_getBoolean(CS, "MemberAdmin"))
                                    resultAuthContext.authContextUser.contentControlID = (cpCore.db.cs_getInteger(CS, "MemberContentControlID"))
                                    resultAuthContext.authContextUser.allowBulkEmail = (cpCore.db.cs_getBoolean(CS, "MemberAllowBulkEmail"))
                                    resultAuthContext.authContextUser.allowToolsPanel = (cpCore.db.cs_getBoolean(CS, "MemberAllowToolsPanel"))
                                    resultAuthContext.authContextUser.adminMenuModeID = cpCore.db.cs_getInteger(CS, "MemberAdminMenuModeID")
                                    resultAuthContext.authContextUser.autoLogin = (cpCore.db.cs_getBoolean(CS, "MemberAutoLogin"))
                                    resultAuthContext.authContextUser.username = (cpCore.db.cs_getText(CS, "MemberUsername"))
                                    resultAuthContext.authContextUser.password = (cpCore.db.cs_getText(CS, "MemberPassword"))
                                    resultAuthContext.authContextUser.languageId = (cpCore.db.cs_getInteger(CS, "MemberLanguageID"))
                                    resultAuthContext.authContextUser.language = (cpCore.db.cs_getText(CS, "MemberLanguage"))
                                    resultAuthContext.authContextUser.organizationId = (cpCore.db.cs_getInteger(CS, "MemberOrganizationID"))
                                    resultAuthContext.authContextUser.styleFilename = cpCore.db.cs_getText(CS, "MemberStyleFilename")
                                    resultAuthContext.authContextUser.excludeFromAnalytics = (cpCore.db.cs_getBoolean(CS, "MemberExcludeFromAnalytics"))
                                    '
                                    ' ----- consider removing
                                    '
                                    resultAuthContext.authContextUser.email = (cpCore.db.cs_getText(CS, "MemberEmail"))
                                    resultAuthContext.authContextUser.company = (cpCore.db.cs_getText(CS, "MemberCompany"))
                                    resultAuthContext.authContextUser.visits = (cpCore.db.cs_getInteger(CS, "MemberVisits"))
                                    resultAuthContext.authContextUser.lastVisit = (cpCore.db.cs_getDate(CS, "MemberLastVisit"))
                                End If
                                '
                                '--------------------------------------------------------------------------
                                ' ----- set main_VisitStateOK if Dbase main_VisitLastTime matches main_VisitLastTimeFromCookie
                                '--------------------------------------------------------------------------
                                '
                                'hint = "270"
                                If ((visit_lastTimeFromCookie - resultAuthContext.visit.LastVisitTime).TotalSeconds) > 2 Then
                                    resultAuthContext.visit_stateOK = False
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
                    If (resultAuthContext.visit.ID = 0) Then
                        '
                        ' ----- Decode Browser User-Agent string to main_VisitName, main_VisitIsBot, main_VisitIsBadBot, etc
                        '
                        Call web_init_decodeBrowserUserAgent(cpCore, resultAuthContext, cpCore.webServer.requestBrowser)
                        '
                        ' ----- create new visit record
                        '
                        'hint = "310"
                        resultAuthContext.visit.ID = cpCore.db.metaData_InsertContentRecordGetID("Visits", resultAuthContext.authContextUser.id)
                        If (resultAuthContext.visit.ID < 1) Then
                            resultAuthContext.visit.ID = 0
                            cpCore.handleExceptionAndRethrow(New Exception("Internal error, new visit record could not be selected."))
                        End If
                        If resultAuthContext.visit.Name = "" Then
                            resultAuthContext.visit.Name = "User"
                        End If
                        resultAuthContext.visit.PageVisits = 0
                        resultAuthContext.visit.StartTime = cpCore.app_startTime
                        resultAuthContext.visit.StartDateValue = CInt(cpCore.app_startTime.ToOADate)
                        resultAuthContext.visit.LastVisitTime = cpCore.app_startTime
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
                                resultAuthContext.visit.RefererPathPage = ""
                                resultAuthContext.visit.HTTP_REFERER = WorkingReferer
                            Else
                                resultAuthContext.visit.RefererPathPage = Mid(WorkingReferer, SlashPosition)
                                resultAuthContext.visit.HTTP_REFERER = Mid(WorkingReferer, 1, SlashPosition - 1)
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
                            Call cpCore.security.decodeToken(CookieVisitor, resultAuthContext.visitor.id, tokenDate)
                            'main_VisitorID = main_DecodeKeyNumber(CookieVisitor)
                            If resultAuthContext.visitor.id <> 0 Then
                                '
                                ' ----- cookie found, open visitor
                                '
                                'hint = "350"
                                resultAuthContext.visit.CookieSupport = True
                                If True Then
                                    SQL = "SELECT ID,Name,MemberID,OrderID,ForceBrowserMobile from ccVisitors WHERE ID=" & resultAuthContext.visitor.id & ";"
                                Else
                                    SQL = "SELECT ID,Name,MemberID,OrderID,0 as ForceBrowserMobile from ccVisitors WHERE ID=" & resultAuthContext.visitor.id & ";"
                                End If
                                CS = cpCore.db.cs_openSql(SQL)
                                If Not cpCore.db.cs_ok(CS) Then
                                    '
                                    ' ----- bad cookie, kill main_VisitorID
                                    '
                                    resultAuthContext.visitor.id = 0
                                Else
                                    '
                                    ' ----- set visitor values
                                    '
                                    visitor_changes = False
                                    resultAuthContext.visitor.id = (cpCore.db.cs_getInteger(CS, "ID"))
                                    resultAuthContext.visitor.name = (cpCore.db.cs_getText(CS, "Name"))
                                    resultAuthContext.visitor.memberID = (cpCore.db.cs_getInteger(CS, "MemberID"))
                                    resultAuthContext.visitor.forceBrowserMobile = (cpCore.db.cs_getInteger(CS, "ForceBrowserMobile"))
                                    resultAuthContext.visitor.orderID = (cpCore.db.cs_getInteger(CS, "OrderID"))
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
                        If resultAuthContext.visitor.id = 0 Then
                            '
                            ' Visitor Fields
                            '
                            resultAuthContext.visitor.id = cpCore.db.metaData_InsertContentRecordGetID("Visitors", resultAuthContext.authContextUser.id)
                            If (resultAuthContext.visitor.id < 1) Then
                                Call cpCore.handleLegacyError14(MethodName, "main_InitVisit, could not create new visitor")
                                resultAuthContext.visitor.id = 0
                            End If
                            resultAuthContext.visitor.name = "Visitor " & resultAuthContext.visitor.id
                            resultAuthContext.visitor.memberID = 0
                            resultAuthContext.visitor.orderID = 0
                            visitor_changes = True
                            resultAuthContext.visitor.forceBrowserMobile = 0
                            '
                            ' Visit Fields
                            '
                            resultAuthContext.visitor.newVisitor = True
                        End If
                        '
                        '-----------------------------------------------------------------------------------
                        ' ----- find  identity from the visitor
                        '-----------------------------------------------------------------------------------
                        '
                        'Call AppendLog("main_InitVisit(), 2492")
                        '
                        'hint = "500"
                        resultAuthContext.authContextUser.id = resultAuthContext.visitor.memberID
                        If (resultAuthContext.visitor.memberID > 0) Then
                            '
                            ' ----- recognize by the main_VisitorMemberID
                            '
                            'hint = "510"
                            If resultAuthContext.recognizeById(cpCore, resultAuthContext.visitor.memberID) Then
                                '
                                ' ----- if successful, now test for autologin (authentication)
                                '
                                'hint = "520"

                                If (cpCore.siteProperties.getBoolean("AllowAutoLogin", False)) And (resultAuthContext.authContextUser.autoLogin) And resultAuthContext.visit.CookieSupport Then
                                    '
                                    ' ----- they allow it, now Check if they were logged in on their last visit
                                    '
                                    'hint = "530"
                                    SQL = "select top 1 V.VisitAuthenticated from ccVisits V where (V.ID<>" & resultAuthContext.visit.ID & ")and(V.VisitorID=" & resultAuthContext.visitor.id & ") order by id desc"
                                    CS = cpCore.db.cs_openSql(SQL)
                                    If cpCore.db.cs_ok(CS) Then
                                        If cpCore.db.cs_getBoolean(CS, "VisitAuthenticated") Then
                                            '
                                            ' ----- yes, go ahead with autologin
                                            '
                                            If resultAuthContext.authenticateById(cpCore, resultAuthContext.authContextUser.id) Then
                                                Call cpCore.log_LogActivity2("autologin", resultAuthContext.authContextUser.id, resultAuthContext.authContextUser.organizationId)
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
                                    Call cpCore.log_LogActivity2("recognized", resultAuthContext.authContextUser.id, resultAuthContext.authContextUser.organizationId)
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
                            Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & main_cookieNameVisitor, cpCore.security.encodeToken(resultAuthContext.visitor.id, resultAuthContext.visit.StartTime), resultAuthContext.visit.StartTime.AddYears(1), , requestAppRootPath, False)
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
                        If resultAuthContext.authenticateById(cpCore, MemberLinkLoginID) Then
                            Call cpCore.log_LogActivity2("link login with eid " & MemberLinkinEID, resultAuthContext.authContextUser.id, resultAuthContext.authContextUser.organizationId)
                        End If
                    ElseIf (MemberLinkRecognizeID <> 0) Then
                        '
                        ' Link Recognize
                        '
                        Call resultAuthContext.recognizeById(cpCore, MemberLinkRecognizeID)
                        Call cpCore.log_LogActivity2("link recognize with eid " & MemberLinkinEID, resultAuthContext.authContextUser.id, resultAuthContext.authContextUser.organizationId)
                    End If
                    '
                    '-----------------------------------------------------------------------------------
                    ' ----- create guest identity if no identity
                    '-----------------------------------------------------------------------------------
                    '
                    'Call AppendLog("main_InitVisit(), 2496")
                    '
                    'hint = "800"
                    If (resultAuthContext.authContextUser.id < 1) Then
                        '
                        ' No user created
                        '
                        If (LCase(Left(resultAuthContext.visit.Name, 5)) <> "visit") Then
                            DefaultMemberName = resultAuthContext.visit.Name
                        Else
                            DefaultMemberName = genericController.encodeText(cpCore.GetContentFieldProperty("people", "name", "default"))
                        End If
                        If (False) Then
                            '
                            ' not upgraded, just create user like it did before
                            '
                            resultAuthContext.authContextUser = authContextUserModel.createDefault(cpCore, resultAuthContext.visit.Name)
                            resultAuthContext.visitor.memberID = resultAuthContext.authContextUser.id
                            resultAuthContext.visitor.saveObject(cpCore)
                            '
                            resultAuthContext.visit.VisitAuthenticated = False
                            resultAuthContext.visit.saveObject(cpCore)
                            '
                            resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                            resultAuthContext.property_user_isMember_isLoaded = False
                            resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                        Else
                            '
                            ' upgraded, determine the kind of tracking - experimental build set to true
                            '
                            TrackGuests = cpCore.siteProperties.getBoolean("track guests", False)
                            If Not TrackGuests Then
                                '
                                ' do not track guests at all
                                '
                                resultAuthContext.authContextUser = authContextUserModel.createDefault(cpCore, resultAuthContext.visit.Name)
                                resultAuthContext.visitor.memberID = resultAuthContext.authContextUser.id
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
                                    ' cookies supported, not first hit and not spider
                                    '
                                    resultAuthContext.authContextUser = authContextUserModel.createDefault(cpCore, resultAuthContext.visit.Name)
                                    resultAuthContext.visitor.memberID = resultAuthContext.authContextUser.id
                                    resultAuthContext.visitor.saveObject(cpCore)
                                    '
                                    resultAuthContext.visit.VisitAuthenticated = False
                                    resultAuthContext.visit.saveObject(cpCore)
                                    '
                                    resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                    resultAuthContext.property_user_isMember_isLoaded = False
                                    resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                Else
                                    '
                                    ' upgraded, set it to the site property - experimental build set to true
                                    '
                                    TrackGuestsWithoutCookies = cpCore.siteProperties.getBoolean("track guests without cookies")
                                    If TrackGuestsWithoutCookies Then
                                        '
                                        ' compatibiltiy mode - create people for non-cookies too
                                        '
                                        Call resultAuthContext.authContextUser.createDefault(cpCore, resultAuthContext.visit.Name)
                                        resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                        resultAuthContext.property_user_isMember_isLoaded = False
                                        resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
                                    Else
                                        '
                                        ' set defaults for people record
                                        '
                                        Call resultAuthContext.authContextUser.createUserDefaults(cpCore, DefaultMemberName)
                                        resultAuthContext.isAuthenticatedAdmin_cache_isLoaded = False
                                        resultAuthContext.property_user_isMember_isLoaded = False
                                        resultAuthContext.isAuthenticatedDeveloper_cache_isLoaded = False
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
                    If resultAuthContext.visit.PageVisits = 0 Then
                        '
                        ' First page of this visit, verify the member language
                        '
                        If (resultAuthContext.authContextUser.languageId < 1) Or (resultAuthContext.authContextUser.language = "") Then
                            '
                            ' No member language, set member language from browser language
                            '
                            Call cpCore.web_GetBrowserLanguage(resultAuthContext.authContextUser.languageId, resultAuthContext.authContextUser.language)
                            If resultAuthContext.authContextUser.languageId > 0 Then
                                '
                                ' Browser Language worked
                                '
                                user_changes = True
                            Else
                                '
                                ' Still no match, main_Get the default language
                                '
                                resultAuthContext.authContextUser.language = cpCore.siteProperties.getText("Language", "English")
                                If resultAuthContext.authContextUser.language <> "English" Then
                                    '
                                    ' Handle the non-English case first, so if there is a problem, fall back is English
                                    '
                                    CS = cpCore.db.cs_open("Languages", "name=" & cpCore.db.encodeSQLText(resultAuthContext.authContextUser.language))
                                    If cpCore.db.cs_ok(CS) Then
                                        resultAuthContext.authContextUser.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                        user_changes = True
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                    If resultAuthContext.authContextUser.languageId = 0 Then
                                        '
                                        ' non-English Language is not in Language Table, set default to english
                                        '
                                        resultAuthContext.authContextUser.language = "English"
                                        Call cpCore.siteProperties.setProperty("Language", resultAuthContext.authContextUser.language)
                                    End If
                                End If
                                If resultAuthContext.authContextUser.language = "English" Then
                                    CS = cpCore.db.cs_open("Languages", "name=" & cpCore.db.encodeSQLText(resultAuthContext.authContextUser.language))
                                    If cpCore.db.cs_ok(CS) Then
                                        resultAuthContext.authContextUser.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                        user_changes = True
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                    If resultAuthContext.authContextUser.languageId < 1 Then
                                        '
                                        ' English is not in Language table, add it, and set it in Member
                                        '
                                        CS = cpCore.db.cs_insertRecord("Languages")
                                        If cpCore.db.cs_ok(CS) Then
                                            resultAuthContext.authContextUser.languageId = cpCore.db.cs_getInteger(CS, "ID")
                                            resultAuthContext.authContextUser.language = "English"
                                            Call cpCore.db.cs_set(CS, "Name", resultAuthContext.authContextUser.language)
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
                    resultAuthContext.visit.ExcludeFromAnalytics = resultAuthContext.visit.ExcludeFromAnalytics Or resultAuthContext.visit_isBot Or resultAuthContext.authContextUser.excludeFromAnalytics Or resultAuthContext.authContextUser.isAdmin Or resultAuthContext.authContextUser.isDeveloper
                    '
                    ' Update Page count
                    '
                    If Not cpCore.webServer.webServerIO_PageExcludeFromAnalytics Then
                        resultAuthContext.visit.PageVisits = resultAuthContext.visit.PageVisits + 1
                    End If
                    '
                    ' Update the Visit
                    ' set main_visitInitialized true allows main_SaveVisit, main_SaveVisitor, etc to work
                    '
                    resultAuthContext.visit_initialized = True
                    resultAuthContext.saveObject(cpCore)
                    '
                    ' ----- Save visitor record
                    '
                    'hint = "940"
                    If visitor_changes Then
                        Call resultAuthContext.visitor.saveObject(cpCore)
                    End If
                    '
                    ' ----- Save Member record
                    '
                    'hint = "950"
                    If user_changes Then
                        Call resultAuthContext.authContextUser.saveObject(cpCore)
                    End If
                    '
                    ' ----- send visit cookie if supported or first page
                    '       no, always send the cookie. There are too many exceptions to try being tricky here.
                    '
                    'hint = "960"
                    CookieVisitNew = cpCore.security.encodeToken(resultAuthContext.visit.ID, resultAuthContext.visit.LastVisitTime)
                    'CookieVisitNew = encodeToken(main_VisitId, main_VisitStartTime)
                    If visitInit_allowVisitTracking And (CookieVisit <> CookieVisitNew) Then
                        CookieVisit = CookieVisitNew
                        NeedToWriteVisitCookie = True
                    End If
                End If
                ' set visitinitialized - for the cases where the earlier set was bypassed, like now allowvisittracking
                resultAuthContext.visit_initialized = True
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
                CookieVisit = cpCore.security.encodeToken(resultAuthContext.visit.ID, cpCore.app_startTime)
                Call cpCore.webServer.addResponseCookie(main_appNameCookiePrefix & constants.main_cookieNameVisit, CookieVisit, , , requestAppRootPath, False)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return resultAuthContext
        End Function


        '
        '=============================================================================
        ' Save Visit
        '   If main_VisitId = 0, create new visit entry and set main_VisitId
        '=============================================================================
        '
        Public Sub saveObject(cpCore As coreClass)
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
                If visit.Name = "" Then
                    visit.Name = "Visit" & visit.ID
                End If
                '
                ' ----- save existing visit
                '
                SQL = "UPDATE ccVisits SET " _
                    & " LastVisitTime=" & cpCore.db.encodeSQLDate(cpCore.app_startTime) _
                    & ",PageVisits=" & (visit.PageVisits) _
                    & ",CookieSupport=" & cpCore.db.encodeSQLBoolean(visit.CookieSupport) _
                    & ",LoginAttempts=" & visit.LoginAttempts _
                    & ",VisitAuthenticated=" & cpCore.db.encodeSQLBoolean(visit.VisitAuthenticated) _
                    & ",MemberID=" & authContextUser.id _
                    & ",MemberNew=" & cpCore.db.encodeSQLBoolean(authContextUser.isNew) _
                    & ",TimeToLastHit=" & cpCore.db.encodeSQLNumber(visit.TimeToLastHit) _
                    & ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(visit.ExcludeFromAnalytics) _
                    & ",Mobile=" & cpCore.db.encodeSQLBoolean(visit_browserIsMobile) _
                    & ",Bot=" & cpCore.db.encodeSQLBoolean(visit_isBot Or visit_isBadBot) _
                    & ""
                If visit.PageVisits <= 1 Then
                    '
                    ' First page of the visit, save everything
                    '
                    SQL &= "" _
                        & ",VisitorID=" & visitor.id _
                        & ",Name=" & cpCore.db.encodeSQLText(visit.Name) _
                        & ",VisitorNew=" & cpCore.db.encodeSQLBoolean(visitor.newVisitor) _
                        & ",StartTime=" & cpCore.db.encodeSQLDate(visit.StartTime) _
                        & ",StartDateValue=" & visit.StartDateValue _
                        & ",DateAdded=" & cpCore.db.encodeSQLDate(visit.StartTime) _
                        & ",Remote_Addr=" & cpCore.db.encodeSQLText(cpCore.webServer.requestRemoteIP) _
                        & ",Browser=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestBrowser, 255)) _
                        & ",HTTP_Via=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestHTTPVia, 255)) _
                        & ",HTTP_From=" & cpCore.db.encodeSQLText(Left(cpCore.webServer.requestHTTPFrom, 255)) _
                        & ",HTTP_REFERER=" & cpCore.db.encodeSQLText(Left(visit.HTTP_REFERER, 255)) _
                        & ",RefererPathPage=" & cpCore.db.encodeSQLText(Left(visit.RefererPathPage, 255)) _
                        & ""
                End If
                SQL &= " WHERE (ID=" & visit.ID & ");"
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
                            If (Not _isAuthenticatedContentManagerAnything_loaded) Or (_isAuthenticatedContentManagerAnything_userId <> authContextUser.id) Then
                                SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(authContextUser.id) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                                        & ")"
                                CS = cpCore.db.cs_openSql(SQL)
                                _isAuthenticatedContentManagerAnything = cpCore.db.cs_ok(CS)
                                cpCore.db.cs_Close(CS)
                                '
                                _isAuthenticatedContentManagerAnything_userId = authContextUser.id
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
                Call cpCore.log_LogActivity2("logout", authContextUser.id, authContextUser.organizationId)
                '
                ' Clear MemberID for this page
                '
                authContextUser = authContextUserModel.createDefault(cpCore, visit.Name)
                '
                visit.VisitAuthenticated = False
                visit.saveObject(cpCore)
                '
                visitor.memberID = authContextUser.id
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
                ElseIf (visit.LoginAttempts >= authContextUsermodel.main_maxVisitLoginAttempts) Then
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
                        & " (ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(authContextUser.id) & ")" _
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
                        Call cpCore.log_LogActivity2("successful password login", authContextUser.id, authContextUser.organizationId)
                        isAuthenticatedAdmin_cache_isLoaded = False
                        property_user_isMember_isLoaded = False
                    Else
                        Call cpCore.log_LogActivity2("unsuccessful login (loginField:" & loginFieldValue & "/password:" & password & ")", authContextUser.id, authContextUser.organizationId)
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
                    Call saveObject(cpCore)
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
                    If AllowAutoLogin Xor authContextUser.autoLogin Then
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
                    authContextUser.id = (cpCore.db.cs_getInteger(CS, "ID"))
                    authContextUser.name = (cpCore.db.cs_getText(CS, "Name"))
                    authContextUser.username = (cpCore.db.cs_getText(CS, "username"))
                    authContextUser.email = (cpCore.db.cs_getText(CS, "Email"))
                    authContextUser.password = (cpCore.db.cs_getText(CS, "Password"))
                    authContextUser.organizationId = (cpCore.db.cs_getInteger(CS, "OrganizationID"))
                    authContextUser.languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    authContextUser.active = (cpCore.db.cs_getBoolean(CS, "Active"))
                    authContextUser.company = (cpCore.db.cs_getText(CS, "Company"))
                    authContextUser.visits = (cpCore.db.cs_getInteger(CS, "Visits"))
                    authContextUser.lastVisit = (cpCore.db.cs_getDate(CS, "LastVisit"))
                    authContextUser.allowBulkEmail = (cpCore.db.cs_getBoolean(CS, "AllowBulkEmail"))
                    authContextUser.allowToolsPanel = (cpCore.db.cs_getBoolean(CS, "AllowToolsPanel"))
                    authContextUser.adminMenuModeID = (cpCore.db.cs_getInteger(CS, "AdminMenuModeID"))
                    authContextUser.autoLogin = (cpCore.db.cs_getBoolean(CS, "AutoLogin"))
                    authContextUser.isDeveloper = (cpCore.db.cs_getBoolean(CS, "Developer"))
                    authContextUser.isAdmin = (cpCore.db.cs_getBoolean(CS, "Admin"))
                    authContextUser.contentControlID = (cpCore.db.cs_getInteger(CS, "ContentControlID"))
                    authContextUser.languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    authContextUser.language = (cpCore.db.cs_getText(CS, "LanguageName"))
                    authContextUser.styleFilename = cpCore.db.cs_getText(CS, "StyleFilename")
                    If authContextUser.styleFilename <> "" Then
                        Call cpCore.htmlDoc.main_AddStylesheetLink(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, authContextUser.styleFilename))
                    End If
                    authContextUser.excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics")
                    '
                    authContextUser.visits = authContextUser.visits + 1
                    If authContextUser.visits = 1 Then
                        authContextUser.isNew = True
                    Else
                        authContextUser.isNew = False
                    End If
                    authContextUser.lastVisit = cpCore.app_startTime
                    'cpCore.main_VisitMemberID = id
                    visit.LoginAttempts = 0
                    visitor.memberID = authContextUser.id
                    visit.ExcludeFromAnalytics = visit.ExcludeFromAnalytics Or visit_isBot Or authContextUser.excludeFromAnalytics Or authContextUser.isAdmin Or authContextUser.isDeveloper
                    Call saveObject(cpCore)
                    Call visitor.saveObject(cpCore)
                    Call authContextUser.saveObject(cpCore)
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
                    iMemberID = authContextUser.id
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
                    iMemberID = authContextUser.id
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
                    checkMemberID = authContextUser.id
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
                    property_user_isMember = isAuthenticated() And cpCore.IsWithinContent(authContextUser.contentControlID, cpCore.main_GetContentID("members"))
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
            Return Not authContextUser.isNew
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
                        If genericController.IsInDelimitedString(authContextUser.main_IsEditingContentList, cacheTestName, ",") Then
                            Call cpCore.debug_testPoint("...is in main_IsEditingContentList")
                            returnResult = True
                        ElseIf genericController.IsInDelimitedString(authContextUser.main_IsNotEditingContentList, cacheTestName, ",") Then
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
                                authContextUser.main_IsEditingContentList = authContextUser.main_IsEditingContentList & "," & cacheTestName
                            Else
                                authContextUser.main_IsNotEditingContentList = authContextUser.main_IsNotEditingContentList & "," & cacheTestName
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