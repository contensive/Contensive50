
Option Explicit On
Option Strict On

Imports Contensive.Core
Imports Contensive.Core.constants
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive
    Public Class monitorConfigClass
        '
        Public Const Port_SiteMonitorDefault As Integer = 4532
        '
        Private cpCore As coreClass
        '
        Public TimerIntervalSec As Integer
        Public allowErrorRecovery As Boolean
        Public allowIISReset As Boolean
        Public DiskSpaceMinMb As Integer
        Public ClearErrorsOnMonitorHit As Boolean
        Public AlarmEmailList As String
        Public AlarmEmailServer As String
        Public ServerName As String
        Public LogFileSizeMax As Integer
        Public SiteTimeout As Integer
        Public scheduleList As String
        Public ListenPort As Integer
        Public StatusMethod As String
        Public httpStatusOnError As String
        '
        '
        '
        Public Sub New(cpCore As coreClass)
            '
            Dim Filename As String
            Dim config As String
            'Dim fs As New fileSystemClass
            Dim Lines() As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            Dim LineBuffer As String
            Dim NameValue() As String
            Dim Pos As Integer
            'Dim utils As New Contensive.utilsClass
            '
            Me.cpCore = cpCore
            '
            ' Set default config
            '
            TimerIntervalSec = 900
            allowErrorRecovery = True
            allowIISReset = True
            DiskSpaceMinMb = 500
            ClearErrorsOnMonitorHit = False
            AlarmEmailList = ""
            AlarmEmailServer = "Mail.MyServer.com"
            ServerName = "Unnamed Server"
            LogFileSizeMax = 1000000
            SiteTimeout = 15
            scheduleList = "0:00-24:00"
            ListenPort = Port_SiteMonitorDefault
            StatusMethod = "status"
            httpStatusOnError = "500 Server Error"
            '
            ' Get config file defaults
            '
            Filename = "config\MonitorConfig.txt"
            config = cpCore.appRootFiles.readFile(Filename)
            If config = "" Then
                config = cpCore.appRootFiles.readFile("MonitorConfig.txt")
                If config <> "" Then
                    Call cpCore.appRootFiles.saveFile(Filename, config)
                End If
            End If
            If config <> "" Then
                Lines = Split(config, vbCrLf)
                LineCnt = UBound(Lines) + 1
                For LinePtr = 0 To LineCnt - 1
                    LineBuffer = Lines(LinePtr)
                    Pos = genericController.vbInstr(1, LineBuffer, "//")
                    If Pos <> 0 Then
                        LineBuffer = Mid(LineBuffer, 1, Pos - 1)
                    End If
                    Pos = genericController.vbInstr(1, LineBuffer, "=")
                    If Pos <> 0 Then
                        NameValue = Split(LineBuffer, "=")
                        If UBound(NameValue) > 0 Then
                            Select Case genericController.vbUCase(Trim(NameValue(0)))
                                Case "STATUSMETHOD"
                                    StatusMethod = Trim(NameValue(1))
                                Case "LISTENPORT"
                                    ListenPort = genericController.EncodeInteger(Trim(NameValue(1)))
                                Case "TIMERINTERVALSEC"
                                    TimerIntervalSec = genericController.EncodeInteger(Trim(NameValue(1)))
                                Case "ALLOWERRORRECOVERY"
                                    allowErrorRecovery = genericController.EncodeBoolean(Trim(NameValue(1)))
                                Case "ALLOWIISRESET"
                                    allowIISReset = genericController.EncodeBoolean(Trim(NameValue(1)))
                                Case "DISKSPACEMINMB"
                                    DiskSpaceMinMb = genericController.EncodeInteger(Trim(NameValue(1)))
                                Case "CLEARERRORSONMONITORHIT"
                                    ClearErrorsOnMonitorHit = genericController.EncodeBoolean(Trim(NameValue(1)))
                                Case "ALARMEMAILLIST"
                                    AlarmEmailList = Trim(NameValue(1))
                                Case "ALARMEMAILSERVER"
                                    AlarmEmailServer = Trim(NameValue(1))
                                Case "SERVERNAME"
                                    ServerName = Trim(NameValue(1))
                                Case "LOGFILESIZEMAX"
                                    LogFileSizeMax = genericController.EncodeInteger(NameValue(1))
                                Case "SITETIMEOUT"
                                    SiteTimeout = genericController.EncodeInteger(NameValue(1))
                                Case "SCHEDULE"
                                    scheduleList = Trim(NameValue(1))
                                Case "HTTPSTATUSONERROR"
                                    httpStatusOnError = Trim(NameValue(1))
                                    '
                                    ' support for upgrade
                                    '
                                Case "IISRESETONERROR"
                                    allowErrorRecovery = True
                                    allowIISReset = True
                            End Select
                        End If
                    End If
                Next
            End If

        End Sub
        '
        '
        '
        Public Sub Save()
            On Error GoTo ErrorTrap
            '
            Dim Filename As String
            '
            ' Load config values from file
            '
            Dim config As String = ""
            'Dim fs As New fileSystemClass
            '
            ' File not found, create file and use defaults
            '
            Filename = "config\MonitorConfig.txt"
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// ccMonitor configuration file" _
                & vbCrLf & "//"
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// How often in seconds the monitor cycles" _
                & vbCrLf & "//" _
                & vbCrLf & "TimerIntervalSec=" & TimerIntervalSec
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// If an application error is found, should error recovery be attempted" _
                & vbCrLf & "//" _
                & vbCrLf & "AllowErrorRecovery = " & allowErrorRecovery
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// If error recovery is attempted, should it IIS be reset. Otherwise an App Pool Recycle will be attempted." _
                & vbCrLf & "//" _
                & vbCrLf & "AllowIISReset = " & allowIISReset
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// Minimum disk space required on all ready drives" _
                & vbCrLf & "//" _
                & vbCrLf & "DiskSpaceMinMb = " & DiskSpaceMinMb
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// Should monitor errors be cleared when http monitor page is hit" _
                & vbCrLf & "//" _
                & vbCrLf & "ClearErrorsOnMonitorHit = " & ClearErrorsOnMonitorHit
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// On error, a text email will be sent to these comma delimited email address" _
                & vbCrLf & "//" _
                & vbCrLf & "AlarmEmailList = " & AlarmEmailList _
                & vbCrLf & "AlarmEmailServer = " & AlarmEmailServer
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// When an Alarm Email is sent, this name will identify this server" _
                & vbCrLf & "//" _
                & vbCrLf & "ServerName = " & ServerName
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// Max size of a Contensive log size in Bytes (1000000 = 1Mb)" _
                & vbCrLf & "//" _
                & vbCrLf & "LogFileSizeMax = " & LogFileSizeMax
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// Time in seconds to wait for each site to reply to the method=status call" _
                & vbCrLf & "//" _
                & vbCrLf & "SiteTimeout = " & SiteTimeout
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// Comma separated list of times to run in 24 hour notation. When not running, it returns all OK" _
                & vbCrLf & "// For example, to run from 2am to 3am, then 6am to midnight 2:00-3:00,6:00-24:00" _
                & vbCrLf & "//" _
                & vbCrLf & "Schedule = " & scheduleList
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// port used by monitor. For example, if the listenport is 4532, then you hit the monitor with http://thisurl.com:4532" _
                & vbCrLf & "//" _
                & vbCrLf & "ListenPort = " & ListenPort
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// method used by monitor to display the status. For example, if the method is 'status', then you hit the monitor with http://thisurl.com:4532/status" _
                & vbCrLf & "//" _
                & vbCrLf & "StatusMethod = " & StatusMethod
            config = config _
                & vbCrLf & "//" _
                & vbCrLf & "// If there is a problem with one of the sites, the monitor page will diplay information about that error. This code will be sent to the browser (or monitor service) for this page. For instance, '200 OK' represents a good status, '500 Server Error' might represent an error." _
                & vbCrLf & "//" _
                & vbCrLf & "httpStatusOnError = " & httpStatusOnError
            Call cpCore.appRootFiles.saveFile(Filename, config)
            '
            Exit Sub
            '
ErrorTrap:
            Call HandleConfigError("Save", "TrapError")
        End Sub
        '
        '
        '
        Public Function isInSchedule() As Boolean
            Dim return_isInSchedule As Boolean = False
            Try
                '
                Dim schedulePeriods() As String
                Dim Ptr As Integer
                Dim schedulePeriod As String
                Dim schedulePeriodTimes() As String
                Dim scheduleTimeStart As Date
                Dim scheduleTimeStop As Date
                'Dim TimeNow As Date
                Dim RightNow As Date
                Dim timeHours As Integer
                Dim timeMinutes As Integer
                Dim schedulePeriodStartTime As String
                '
                isInSchedule = True
                RightNow = DateTime.Now()
                'TimeNow = New Date(0, 1, 1, RightNow.Hour, RightNow.Minute, 1)
                If scheduleList <> "" Then
                    isInSchedule = False
                    schedulePeriods = Split(scheduleList, ",")
                    For Ptr = 0 To UBound(schedulePeriods)
                        schedulePeriod = Trim(schedulePeriods(Ptr))
                        If schedulePeriod <> "" Then
                            If genericController.vbInstr(1, schedulePeriod, "-") <> 0 Then
                                schedulePeriodTimes = Split(schedulePeriod, "-")
                                Ptr = schedulePeriodTimes(0).IndexOf(":")
                                schedulePeriodStartTime = schedulePeriodTimes(0)
                                If Ptr = -1 Then
                                    timeHours = genericController.EncodeInteger(schedulePeriodStartTime)
                                    timeMinutes = 0
                                Else
                                    timeHours = genericController.EncodeInteger(schedulePeriodStartTime.Substring(0, Ptr))
                                    timeMinutes = genericController.EncodeInteger(schedulePeriodStartTime.Substring(Ptr))
                                End If
                                If timeHours >= 24 Then
                                    timeHours = 23
                                    timeMinutes = 59
                                ElseIf timeMinutes >= 60 Then
                                    timeMinutes = 59
                                End If
                                scheduleTimeStart = New Date(RightNow.Year, RightNow.Month, RightNow.Day, timeHours, timeMinutes, 0)
                                If (RightNow > scheduleTimeStart) Then
                                    If schedulePeriodTimes.GetUpperBound(0) > 0 Then
                                        Ptr = schedulePeriodTimes(1).IndexOf(":")
                                        If Ptr = -1 Then
                                            timeHours = genericController.EncodeInteger(schedulePeriodTimes(1))
                                            timeMinutes = 0
                                        Else
                                            timeHours = genericController.EncodeInteger(schedulePeriodTimes(1).Substring(0, Ptr))
                                            timeMinutes = genericController.EncodeInteger(schedulePeriodTimes(1).Substring(Ptr))
                                        End If
                                        If timeHours >= 24 Then
                                            timeHours = 23
                                            timeMinutes = 59
                                        ElseIf timeMinutes >= 60 Then
                                            timeMinutes = 59
                                        End If
                                        scheduleTimeStop = New Date(RightNow.Year, RightNow.Month, RightNow.Day, timeHours, timeMinutes, 0)
                                        If RightNow < scheduleTimeStop Then
                                            return_isInSchedule = True
                                            Exit For
                                        End If
                                    End If

                                End If
                            End If
                        End If
                    Next
                End If
            Catch ex As Exception
                Call HandleConfigError("isInSchedule", ex.ToString())
            End Try
            Return return_isInSchedule
        End Function

        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Private Sub HandleConfigError(ByVal MethodName As String, ByVal ErrorMessage As String)
            Dim ErrNumber As Integer
            Dim ErrSource As String
            Dim ErrDescription As String
            '
            ErrNumber = Err.Number
            ErrSource = Err.Source
            ErrDescription = Err.Description
            logController.appendLog(cpCore, getAppExeName() & ".ConfigClass." & MethodName & "[" & ErrorMessage & "]-[" & ErrSource & " #" & ErrNumber & ", " & ErrDescription & "]", "Monitor")
            Err.Clear()
        End Sub
    End Class
End Namespace
