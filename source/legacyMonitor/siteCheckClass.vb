
Option Explicit On
Option Strict On

Imports System.Reflection
Imports System.Timers
Imports System.Threading

Namespace Contensive.Core
    Public Class siteCheckClass
        '========================================================================
        '
        ' This page and its contents are copyright by Kidwell McGowan Associates.
        '
        '==================================================================================================
        '
        '   Timer runs while the form is loaded
        '       DisplayStatus runs on Timer
        '   NTService is installed with -install option
        '   NTService is uninstalled with teh -uninstall option
        '   NTService Start event starts ContentServer features
        '   NTService Pause and stop stops the contentserver
        '
        '==================================================================================================
        '
        ' ----- global scope variables
        '
        Private cpCore As cpCoreClass
        Private Const serviceDisplayName As String = "Contensive Monitor"
        Private ClassInitialized As Boolean        ' if true, the module has been
        Private DebugMode As Boolean
        Private Testing As Boolean
        '
        Private TimerIntervalSec As Integer
        Private allowErrorRecovery As Boolean
        Private allowIISReset As Boolean
        Private DiskSpaceMinMb As Integer
        Private ClearErrorsOnMonitorHit As Boolean
        Private AlarmEmailList As String
        Private AlarmEmailServer As String
        Private ServerName As String
        Private LogFileSizeMax As Integer
        Private SiteTimeout As Integer
        Private ScheduleList As String
        Private ListenPort As Integer
        Private StatusMethod As String
        Private version As String
        '
        '------------------------------------------------------------------------------------------
        '   Timer
        '------------------------------------------------------------------------------------------
        '
        Const TimerMsecPerTick As Integer = 1000
        Private processTimer As New System.Timers.Timer(10000)
        Private ProcessTimerTickCnt As Integer
        Private ServiceStartTime As Date
        '
        '------------------------------------------------------------------------------------------
        '   Process status flags
        '------------------------------------------------------------------------------------------
        '
        Private ServiceRunning As Boolean
        Private ServiceInProcess As Boolean
        Private HTTPInProcess As Boolean
        Private AbortProcess As Boolean
        Private HTTPLastError As Integer
        'Private needsErrorRecovery As Boolean
        '
        ' Document constants
        '
        Private Const DocLengthMax As Integer = 500000         ' maximum size of a document to be parsed
        '
        '------------------------------------------------------------------------------------------
        '   HTTP Request
        '------------------------------------------------------------------------------------------
        '
        Private HTTPAuthUsername As String               '
        Private HTTPAuthPassword As String               '
        Private ContensiveUsername As String               '
        Private ContensivePassword As String               '
        '
        '------------------------------------------------------------------------------------------
        '   HTTP Response
        '------------------------------------------------------------------------------------------
        '
        'Private HTTPResponse As String                  ' the entire response
        Private HTTPResponseHeader As String            '   header part
        Private HTTPResponseEntity As String            '   entity part
        Private HTTPResponseFilename As String          ' the server response
        Private HTTPResponseStatus As String            ' the whole firstline
        Private HTTPResponseCode As String              ' HTTP response Code (200, etc)
        Private HTTPResponseContentType As String       ' HTTP response header value
        Private HTTPResponseTime As Double              ' time to fetch this page
        Private HTTPResponseTickCountStart As Integer
        Const HTTPResponseHeaderCountMax As Integer = 100
        Private HTTPResponseContentLength As Integer       ' length of the body (from header)
        Private HTTPBodyText As String                  ' all the text only body copy
        Private HTTPSpellCheckText As String            ' all the text only body copy
        '
        Private ResponseHeaderCount As Integer
        Private ResponseHeaderSize As Integer
        'Private ResponseHeaders() As NameValuePairType
        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Private Sub HandleMonitorError(ByVal MethodName As String, ByVal ErrorMessage As String)
            Dim ErrNumber As Integer
            Dim ErrSource As String
            Dim ErrDescription As String
            '
            ErrNumber = Err.Number
            ErrSource = Err.Source
            ErrDescription = Err.Description
            '
            Call appendMonitorLog("siteMonitorClass." & MethodName & "[" & ErrorMessage & "]-[" & ErrSource & " #" & ErrNumber & ", " & ErrDescription & "]")
            Err.Clear()
        End Sub
        '
        '=========================================================================================
        '   Use the MS Winsock to get a page and return it in the page buffer
        '=========================================================================================
        '
        Private Function GetDoc(ByVal Link As String, ByVal AppName As String, ByVal RequestTimeout As Integer, ByRef return_needsErrorRecovery As Boolean) As String
            On Error GoTo ErrorTrap
            '
            Dim kmaHTTP As New coreHttpRequestClass()
            Dim CookiePointer As Integer
            Dim CookieGood As Boolean
            Dim CookieMatchCount As Integer
            Dim CSPointer As Integer
            Dim CookieDomain As String
            Dim CookieExpires As String
            Dim CookieName As String
            Dim CookiePath As String
            Dim CookieValue As String
            Dim CookieString As String
            Dim CookieDateAdded As Date
            Dim CookieID As Integer
            Dim VisitID As Integer
            Dim SQL As String
            Dim Hint As String
            Dim AnyLanguageID As Integer
            Dim CSLanguage As Integer
            Dim CSVisit As Integer
            Dim CSMember As Integer
            Dim VisitMemberID As Integer
            Dim URLWorking As String
            Dim ResponseHeader As String
            Dim ResponseLines() As String
            Dim ResponseStatus As String
            '
            Call PrintDebugMessage("Requesting [" & Link & "]", AppName)
            '
            '   Get the document
            '
            return_needsErrorRecovery = False
            kmaHTTP.userAgent = "Contensive Monitor"
            'hint = "Getting Document"
            HTTPResponseTickCountStart = CInt(GetTickCount)
            HTTPInProcess = True
            HTTPLastError = 0
            URLWorking = EncodeURL(Link)
            On Error Resume Next
            kmaHTTP.timeout = RequestTimeout
            GetDoc = kmaHTTP.getURL(URLWorking)
            If Err.Number <> 0 Then
                HTTPLastError = Err.Number
                Call HandleMonitorError("MonitorForm", "GetDoc getting URL [" & Link & "]")
                Select Case HTTPLastError
                    Case 20302
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - timeout waiting for server]")
                        return_needsErrorRecovery = True
                    Case 25061
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - timeout waiting for server]")
                        return_needsErrorRecovery = True
                    Case 25065
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - host is down]")
                        return_needsErrorRecovery = True
                    Case 26002, 26002, 26002, 26002
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - host not found]")
                    Case 25069, 25068
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - server too busy]")
                        return_needsErrorRecovery = True
                    Case 26005
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - invalid domain name]")
                    Case Else
                        Call appendMonitorLog("GetDoc(" & URLWorking & ") returned error [" & HTTPLastError & " - error requesting document]")
                        return_needsErrorRecovery = True
                End Select
            Else
                ResponseHeader = kmaHTTP.responseHeader
                ResponseLines = Split(ResponseHeader, vbCrLf)
                ResponseStatus = ""
                If UBound(ResponseLines) > 0 Then
                    ResponseStatus = ResponseLines(0)
                End If
                If InStr(1, ResponseStatus, "200 OK", vbTextCompare) = 0 Then
                    '
                    ' Not a 200, this is an error
                    '
                    Call appendMonitorLog("GetDoc(" & URLWorking & ") returned Response Status [" & ResponseStatus & "]")
                    GetDoc = "Response Status [" & ResponseStatus & "]"
                    Select Case Mid(ResponseStatus, 1, 1)
                        Case "5"
                            '
                            ' Server error, allow IIS resetr
                            '
                            return_needsErrorRecovery = True
                    End Select
                End If
            End If
            HTTPInProcess = False
            HTTPResponseTime = GetTickCount - HTTPResponseTickCountStart
            '
            'hint = "Done"
            '
            Call PrintDebugMessage("Request complete [" & Link & "]", AppName)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            kmaHTTP = Nothing
            Call HandleMonitorError("GetDoc", "(Hint=" & Hint & ") getting URL [" & Link & "]")
        End Function
        '
        '==========================================================================================
        '   Update the status form
        '==========================================================================================
        '
        Private Sub PrintDebugMessage(ByVal CurrentActivity As String, ByVal AppName As String)
            On Error GoTo ErrorTrap
            '
            Dim Copy As String
            '
            ' ----- if debugging, print the current status
            '
            If DebugMode And (CurrentActivity <> "") Then
                Call SaveToLogFile("PrintDebugMessage", AppName, CurrentActivity)
            End If
            Exit Sub
            '
ErrorTrap:
            Call HandleMonitorError("PrintDebugMessage", "TrapError")
        End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Private Sub SaveToLogFile(ByVal MethodName As String, ByVal AppName As String, ByVal LogCopy As String)
            On Error GoTo ErrorTrap
            '
            Call appendMonitorLog(getAppExeName() & ".MonitorForm." & MethodName & "[" & AppName & "], " & LogCopy)
            '
            Exit Sub
            '
ErrorTrap:
            Call HandleMonitorError("SaveToLogFile", "TrapError")
        End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Public Sub SaveToEventLogAndLogFile(ByVal MethodName As String, ByVal AppName As String, ByVal LogCopy As String)
            On Error GoTo ErrorTrap
            '
            'Call App.LogEvent(LogCopy)
            Call SaveToLogFile(MethodName, AppName, LogCopy)
            ' Call NTService.LogEvent(NTServiceEventInformation, NTServiceIDInfo, LogCopy)
            Exit Sub
            '
ErrorTrap:
            Call HandleMonitorError("SaveToEventLogAndLogFile", "TrapError")
        End Sub
        '
        '
        '
        Friend Sub StartMonitoring()
            On Error GoTo ErrorTrap
            '
            ' convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
            Dim config As New monitorConfigClass(cpCore)
            '
            AlarmEmailList = config.AlarmEmailList
            AlarmEmailServer = config.AlarmEmailServer
            ClearErrorsOnMonitorHit = config.ClearErrorsOnMonitorHit
            DiskSpaceMinMb = config.DiskSpaceMinMb
            allowErrorRecovery = config.allowErrorRecovery
            allowIISReset = config.allowIISReset
            LogFileSizeMax = config.LogFileSizeMax
            ScheduleList = config.scheduleList
            ServerName = config.ServerName
            SiteTimeout = config.SiteTimeout
            TimerIntervalSec = config.TimerIntervalSec
            ListenPort = config.ListenPort
            StatusMethod = config.StatusMethod
            '
            ServiceStartTime = Date.Now
            AbortProcess = False
            '
            ' Setup Timer
            '
            ServiceInProcess = False
            processTimer.Interval = TimerMsecPerTick
            If DebugMode Then
                config.TimerIntervalSec = 10
            End If
            processTimer.Enabled = True
            '
            ' Setup Debug Panel
            '
            ServiceRunning = True
            '
            ' Log
            '
            Call SaveToLogFile("StartMonitoring", "-", serviceDisplayName & " Started")
            Call PrintDebugMessage(serviceDisplayName & " Started", "-")
            Call config.Save()
            'cp.Dispose()
            Exit Sub
            '
ErrorTrap:
            Call HandleMonitorError("StartMonitoring", "TrapError")
        End Sub
        '
        '   Begin the stop service action.
        '       It can complete by 3 different ways
        '       1 - if it is not busy, call complete from here
        '       2 - if busy and it shuts down correctly, complete is at end of GetDoc
        '       3 - if busy and TopTimeoutTimer goes off, abort GetDoc
        '
        Friend Sub StopMonitoring()
            On Error GoTo ErrorTrap
            '
            Dim Timeout As Integer
            '
            processTimer.Enabled = False
            '
            Call SaveToLogFile("StopMonitoring", "-", "Stopping " & serviceDisplayName)
            Call PrintDebugMessage("Stopping " & serviceDisplayName, "-")
            '
            ' set abort and the timeout timer
            '
            AbortProcess = True
            '
            Exit Sub
            '
ErrorTrap:
            Call HandleMonitorError("StopMonitoring", "TrapError")
        End Sub
        '
        '==========================================================================================
        '   Event Timer Tick
        '==========================================================================================
        '
        Public Sub ProcessTimerTick(ByVal source As Object, ByVal e As ElapsedEventArgs)
            On Error GoTo ErrorTrap
            '
            Dim AlarmEmailBody As String
            'Dim Mailer As New SMTP5Class
            Dim Emails() As String
            Dim Ptr As Integer
            Dim emailstatus As String
            Dim ToAddress As String
            Dim cp As CPClass
            Dim config As monitorConfigClass
            '
            ProcessTimerTickCnt = ProcessTimerTickCnt + 1
            If (Not AbortProcess) And (ProcessTimerTickCnt >= TimerIntervalSec) Then
                If ServiceInProcess Then
                    '
                    ' Overlapping request - ignore it and reset count
                    '
                    ProcessTimerTickCnt = 0
                    Call appendMonitorLog("Monitor is attempting a check, but a previous check is not complete. Timer will be reset.")
                Else
                    '
                    ' service the request
                    '
                    ProcessTimerTickCnt = ProcessTimerTickCnt - TimerIntervalSec
                    ServiceInProcess = True
                    ' convert to cluster-level object, then do applicaiton work by enumerating applications and using cp for each app
                    cp = New CPClass()
                    config = New monitorConfigClass(cpCore)
                    If Not config.isInSchedule Then
                        '
                        ' not scueduled
                        '
                        Call appendMonitorLog("Monitor is not schedule for testing now, " & Now())
                    Else
                        AlarmEmailBody = monitorAllSites_returnEmailBody()
                        '
                        ' send alarm email if there was a problem
                        '
                        If AlarmEmailBody <> "" Then
                            Call appendMonitorLog("Monitor detected a problem")
                            If AlarmEmailServer = "" Then
                                Call appendMonitorLog("Alarm not sent because AlarmEmailServer is not configured in " & getProgramFilesPath() & "\config\MonitorConfig.txt")
                            Else
                                If AlarmEmailList = "" Then
                                    Call appendMonitorLog("Alarm not sent because AlarmEmailList is not configured in " & getProgramFilesPath() & "\config\MonitorConfig.txt")
                                Else
                                    AlarmEmailBody = "" _
                                        & "Errors Found on Server [" & ServerName & "]" _
                                        & vbCrLf & AlarmEmailBody
                                    Emails = Split(AlarmEmailList, ",")
                                    For Ptr = 0 To UBound(Emails)
                                        ToAddress = Emails(Ptr)
                                        If ToAddress <> "" Then
                                            'emailstatus = Mailer.sendEmail5(AlarmEmailServer, Emails(Ptr), "Monitor@Contensive.com", "Monitor Alarm [" & ServerName & "]", AlarmEmailBody)
                                            'Call appendMonitorLog("Sending Alarm Notification to " & ToAddress & ", status=" & emailstatus)
                                        End If
                                    Next
                                End If
                            End If
                        End If
                    End If
                    ServiceInProcess = False
                    cp.Dispose()
                End If
            End If
            Exit Sub
ErrorTrap:
            ServiceInProcess = False
            Call HandleMonitorError("ProcessTimerTick", "TrapError")
            Err.Clear()
        End Sub
        '
        '==========================================================================================
        '   Returns "" if no errors
        '   Returns the AlarmEmail message if there were errors
        '==========================================================================================
        '
        Private Function monitorAllSites_returnEmailBody() As String
            On Error GoTo ErrorTrap
            '
            Dim needsErrorRecovery As Boolean
            Dim AppList As String
            Dim AppLine() As String
            Dim AppCnt As Integer
            Dim AppPtr As Integer
            Dim AppDetails() As String
            Dim AppDetailsCnt As Integer
            Dim SiteStatus As Integer
            Dim AppName As String
            Dim AppStatus As Integer
            Dim Hint As String
            Dim DomainName As String
            Dim ListSplit() As String
            Dim Response As String
            Dim AppRootPath As String
            Dim DomainNameList As String
            Dim DefaultPageName As String
            Dim ResponseStatusLine As String
            Dim AppLogPtr As Integer
            Dim ErrorMessageResponse As String
            'Dim cp As New CPClass
            'Dim Ctrl As New controlClass
            '
            Throw New NotImplementedException
            'AppList = ctrl.GetApplicationList
            monitorAllSites_returnEmailBody = ""
            If AppList = "" Then
                '
                ' Problem
                '
                monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody & vbCrLf & " Error, no Contensive Applications were found"
                Call appendMonitorLog("GetApplicationList call returned no Contensive Applications were found")
            Else
                AppLine = Split(AppList, vbCrLf)
                AppCnt = UBound(AppLine) + 1
                'hint = ""
                Do While AppPtr < AppCnt And (Not AbortProcess)
                    AppDetails = Split(AppLine(AppPtr), vbTab)
                    AppDetailsCnt = UBound(AppDetails) + 1
                    If AppDetailsCnt < AppListCount Then
                        '
                        ' Problem - can not monitor
                        '
                        Call appendMonitorLog("GetApplicationList call return less than " & AppListCount & " arguments, aborting monitor pass for this application [" & AppLine(AppPtr) & "]")
                    Else
                        AppName = AppDetails(AppList_Name)
                        If Not EncodeBoolean(AppDetails(AppList_AllowSiteMonitor)) Then
                            '
                            ' Monitoring Disabled
                            '
                            Call appendMonitorLog(AppName & " monitoring disabled")
                        Else
                            '
                            ' Monitor site
                            '
                            AppStatus = EncodeInteger(AppDetails(AppList_Status))
                            If False Then
                                '
                                'ElseIf AppStatus = applicationStatusEnum.ApplicationStatusPaused Then
                                '    '
                                '    ' Paused
                                '    '
                                '    Call appendMonitorLog(AppName & " paused")
                            ElseIf AppStatus = applicationStatusEnum.ApplicationStatusReady Then
                                '
                                ' Running
                                '
                                If AppLogCnt > 0 Then
                                    For AppLogPtr = 0 To AppLogCnt - 1
                                        If AppLog(AppLogPtr).Name = AppName Then
                                            Exit For
                                        End If
                                    Next
                                End If
                                If AppLogPtr >= AppLogCnt Then
                                    '
                                    ' App not found in log, add a new log entry
                                    '
                                    AppLogPtr = AppLogCnt
                                    AppLogCnt = AppLogCnt + 1
                                    ReDim Preserve AppLog(AppLogPtr)
                                    AppLog(AppLogPtr).Name = AppName
                                End If
                                '
                                ' Check Site
                                '
                                AppLog(AppLogPtr).LastStatusResponse = ""
                                AppLog(AppLogPtr).LastCheckOK = True
                                DomainName = ""
                                DomainNameList = AppDetails(AppList_DomainName)
                                If DomainNameList <> "" Then
                                    ListSplit = Split(DomainNameList, ",")
                                    DomainName = ListSplit(0)
                                End If
                                DefaultPageName = AppDetails(AppList_DefaultPage)
                                AppRootPath = AppDetails(AppList_RootPath)
                                If DomainName = "" Then
                                    AppLog(AppLogPtr).LastCheckOK = False
                                    monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody & vbCrLf & "Contensive application [" & AppName & "] has no valid domain name"
                                    AppLog(AppLogPtr).StatusCheckCount = AppLog(AppLogPtr).StatusCheckCount + 1
                                    AppLog(AppLogPtr).ErrorCount = AppLog(AppLogPtr).ErrorCount + 1
                                    Call appendMonitorLog(AppName & " has no valid domain name")
                                Else
                                    If InStr(1, DomainName, "http://", CompareMethod.Text) = 0 Then
                                        DomainName = "http://" & DomainName
                                    End If
                                    DomainName = DomainName & AppRootPath & DefaultPageName
                                    AppLog(AppLogPtr).SiteLink = DomainName
                                    DomainName = DomainName & "?method=status"
                                    AppLog(AppLogPtr).StatusLink = DomainName
                                    ErrorMessageResponse = ""
                                    AppLog(AppLogPtr).StatusCheckCount = AppLog(AppLogPtr).StatusCheckCount + 1
                                    '
                                    ' test
                                    '
                                    Response = GetDoc(DomainName, AppName, SiteTimeout, needsErrorRecovery)
                                    ResponseStatusLine = getLine(Response)
                                    AppLog(AppLogPtr).LastStatusResponse = ResponseStatusLine
                                    If InStr(1, ResponseStatusLine, "Contensive OK", vbTextCompare) = 1 Then
                                        '
                                        ' no error
                                        '
                                        Call appendMonitorLog(AppName & " returned OK")
                                        AppLog(AppLogPtr).LastCheckOK = True
                                    Else
                                        '
                                        ' error
                                        '
                                        ErrorMessageResponse = ResponseStatusLine
                                        Call appendMonitorLog(AppName & " returned Error [" & ErrorMessageResponse & "]")
                                        If False Then
                                            '
                                            ' this is a problem -- an internal contensive error problem would not be recovered (?)
                                            '   ACNM errored for 30 minutes before someone restarted.
                                            '
                                            '                                If Not needsErrorRecovery Then
                                            '                                    '
                                            '                                    ' This error does not require recovery
                                            '                                    '
                                            '                                    AppLog(AppLogPtr).LastCheckOK = False
                                            '                                    AppLog(AppLogPtr).ErrorCount = AppLog(AppLogPtr).ErrorCount + 1
                                            '                                    monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                            '                                        & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], no error recovery run because this error would not be affected."
                                            '                                    Call AppendMonitorLog("This error does not require recovery")
                                        ElseIf Not allowErrorRecovery Then
                                            '
                                            ' recoverable  error, but no error recovery allowed
                                            '
                                            AppLog(AppLogPtr).LastCheckOK = False
                                            AppLog(AppLogPtr).ErrorCount = AppLog(AppLogPtr).ErrorCount + 1
                                            If Not allowIISReset Then
                                                '
                                                ' no reset allowed
                                                '
                                                monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                                    & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], error recovery and iisreset disabled, abort testing and setting alarm."
                                                Call appendMonitorLog("Error recovery and iisreset disabled, aborting test and setting alarm.")
                                            Else
                                                '
                                                ' iis reset and abort
                                                '
                                                monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                                    & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], error recovery disabled but iisreset disabled, calling iisreset, setting alarm and aborting test."
                                                Call appendMonitorLog("Recovery failed, allowIISReset true, flag monitor error, calling iisreset, setting alarm and aborting test.")
                                                Call recoverError(allowErrorRecovery, True, AppName)
                                                Exit Do
                                            End If
                                        Else
                                            '
                                            ' recoverable error, attempt app pool recycle and retry
                                            '
                                            Call appendMonitorLog("Attempting app pool recycle and retry")
                                            Call recoverError(True, False, AppName)
                                            Response = GetDoc(DomainName, AppName, SiteTimeout, needsErrorRecovery)
                                            ResponseStatusLine = getLine(Response)
                                            AppLog(AppLogPtr).LastStatusResponse = "[" & AppLog(AppLogPtr).LastStatusResponse & "], after recovery [" & ResponseStatusLine & "]"
                                            If InStr(1, ResponseStatusLine, "Contensive OK", vbTextCompare) = 1 Then
                                                '
                                                ' recovered, continue without error
                                                '
                                                AppLog(AppLogPtr).LastCheckOK = True
                                                Call appendMonitorLog("Recycle and retry returned OK, no alarm.")
                                            Else
                                                '
                                                ' did not recover, try iisreset and abort
                                                '
                                                AppLog(AppLogPtr).LastCheckOK = False
                                                AppLog(AppLogPtr).ErrorCount = AppLog(AppLogPtr).ErrorCount + 1
                                                If Not allowIISReset Then
                                                    '
                                                    ' no reset allowed
                                                    '
                                                    monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                                        & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], recycle failed to recover, IISReset disabled and alarm set."
                                                    Call appendMonitorLog("Recovery failed, allowIISReset false, flag monitor error, skip app and continue testing.")
                                                Else
                                                    '
                                                    ' iis reset and abort
                                                    '
                                                    monitorAllSites_returnEmailBody = monitorAllSites_returnEmailBody _
                                                        & vbCrLf & DomainName & " Error [" & ErrorMessageResponse & "], recycle failed to recover, IISReset called and alarm set."
                                                    Call appendMonitorLog("Recovery failed, allowIISReset true, flag monitor error, iisreset and abort pass.")
                                                    Call recoverError(allowErrorRecovery, True, AppName)
                                                    Exit Do
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Else
                                Call appendMonitorLog(AppName & " is not running, ApplicationStatus=" & AppDetails(1))
                            End If
                        End If
                    End If
                    AppPtr = AppPtr + 1
                Loop
            End If
            '
            If monitorAllSites_returnEmailBody <> "" Then
                monitorAllSites_returnEmailBody = Mid(monitorAllSites_returnEmailBody, 3)
            End If
            '
            Exit Function
ErrorTrap:
            ServiceInProcess = False
            Call HandleMonitorError("monitorAllSites_returnEmailBody", "TrapError (hint=" & Hint & ")")
            Err.Clear()
        End Function
        ''
        ''==========================================================================================
        ''
        ''==========================================================================================
        ''
        'Private Function IsSiteOK(DomainNameList As String, DefaultPageName As String, AppName As String) As Integer
        '    On Error GoTo ErrorTrap
        '    '
        '    Dim DomainName As String
        '    Dim ListSplit() As String
        '    Dim Response As String
        '    Dim ErrorMessageResponse As String
        '    '
        '    ListSplit = Split(DomainNameList, ",")
        '    DomainName = ListSplit(0)
        '    If DomainName <> "" Then
        '        If InStr(1, DomainName, "http://", 1) = 0 Then
        '            DomainName = "http://" & DomainName
        '        End If
        '        DomainName = DomainName & "/" & DefaultPageName & "?method=status"
        '        Response = GetDoc(DomainName, AppName, SiteTimeout, ErrorMessageResponse)
        '        If InStr(1, Response, "Contensive OK", vbTextCompare) <> 0 Then
        '            IsSiteOK = True
        '        Else
        '            IsSiteOK = False
        '        End If
        '    End If
        '
        '    '
        '    Exit Function
        'ErrorTrap:
        '    ServiceInProcess = False
        '    Call HandleMonitorError("IsSiteOK", "TrapError")
        '    Err.Clear
        'End Function
        '
        '==========================================================================================
        '
        '==========================================================================================
        '
        Private Sub recoverError(ByVal allowErrorRecovery As Boolean, ByVal allowIISReset As Boolean, ByVal appPoolName As String)
            Try
                Dim Cmd As String
                'Dim Filename As String
                'Dim Content As String
                '
                ' Run IISReset
                '
                If Not allowErrorRecovery Then
                    '
                    ' error recover not enabled
                    '
                    Call appendMonitorLog("Error condition detected but no recovery attempted because AllowErrorRecover is false.")
                Else
                    If allowIISReset Or (appPoolName = "") Then
                        '
                        ' iisreset the server
                        '
                        If (Not allowIISReset) And (appPoolName = "") Then
                            Call appendMonitorLog("Error condition detected and IISReset called to recover, allowIISReset is false but AppName is blank")
                        Else
                            Call appendMonitorLog("Error condition detected and IISReset called to recover.")
                        End If
                        Cmd = "%comspec% /c IISReset"
                        Try
                            Call executeCommandSync(Cmd)
                        Catch ex As Exception
                            Call appendMonitorLog("IISReset attempt failed, error=" & ex.ToString())
                        End Try
                    Else
                        '
                        ' recycle the app
                        '
                        Call appendMonitorLog("attempting IIS app pool recycle on " & appPoolName & ".")
                        Cmd = "%comspec% /c %systemroot%\system32\inetsrv\appcmd recycle apppool " & appPoolName
                        Try
                            Call executeCommandSync(Cmd)
                        Catch ex As Exception
                            Call appendMonitorLog("IIS app pool recycle attempt failed, error=" & ex.ToString())
                        End Try
                    End If
                End If
            Catch ex As Exception
                Call appendMonitorLog("recoverError, unexpected exception=" & ex.ToString())
            Finally
                ServiceInProcess = False
            End Try
        End Sub
        '
        '
        '
        Private Sub appendMonitorLog(ByVal message As String)
            cpCore.appendLog(message, "Monitor")
        End Sub

        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.cpcore = cpCore
            version = Assembly.GetEntryAssembly().GetName().Version.ToString()
            AddHandler processTimer.Elapsed, AddressOf ProcessTimerTick
        End Sub
        '
        '
        '
        Public Function executeCommandSync(ByVal command As String) As String
            Dim result As String = ""
            Try
                'create the ProcessStartInfo using "cmd" as the program to be run,
                'and "/c " as the parameters.
                'Incidentally, /c tells cmd that we want it to execute the command that follows,
                'and then exit.
                Dim procStartInfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo("%comspec%", "/c " + command)
                'Dim procStartInfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo("cmd", "/c " + command)
                'The following commands are needed to redirect the standard output.
                'This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = True
                procStartInfo.UseShellExecute = False
                'Do not create the black window.
                procStartInfo.CreateNoWindow = True
                'Now we create a process, assign its ProcessStartInfo and start it
                Dim proc As System.Diagnostics.Process = New System.Diagnostics.Process()
                proc.StartInfo = procStartInfo
                proc.Start()
                'Get the output into a string
                result = proc.StandardOutput.ReadToEnd()
                'Display the command output.
            Catch ex As Exception

            End Try
            Return result
        End Function
        ''
        ''
        ''
        'Public Sub executeCommandAsync(ByVal Command As String)
        '    Try
        '        '//Asynchronously start the Thread to process the Execute command request.
        '        Dim objThread As Thread = New Thread(New ParameterizedThreadStart(AddressOf executeCommandSync))
        '        '//Make the thread as background thread.
        '        objThread.IsBackground = True
        '        '//Set the Priority of the thread.
        '        objThread.Priority = ThreadPriority.AboveNormal
        '        '//Start the thread.
        '        objThread.Start(Command)
        '    Catch ex As ThreadStartException
        '        '
        '        '
        '        '
        '    Catch ex As ThreadAbortException
        '        '
        '        '
        '        '
        '    Catch ex As Exception
        '        '
        '        '
        '        '
        '    End Try
        'End Sub
    End Class

End Namespace