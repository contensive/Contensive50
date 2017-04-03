
'Imports Contensive.Core
'Imports Contensive.Core

Namespace Contensive.Core
    Public Class statusServerClass
        Private cmdListener As New coreIpDaemonClass
        Private cpCore As coreClass
        '
        public Sub startListening()
            Try
                '
                Dim config As New monitorConfigClass(cpcore)
                Call cmdListener.startListening(Me, config.ListenPort)
            Catch ex As Exception
                Call reportError(ex.ToString, "startListennig")
            End Try
        End Sub
        '
        public Sub stopListening()
            Try
                Call cmdListener.stopListening()
            Catch ex As Exception
                Call reportError(ex.ToString, "startListennig")
            End Try
        End Sub
        '
        '
        '
        Public Function ipDaemonCallback(Cmd As String, arguments As String, RemoteIP As String) As String
            Dim returnString As String = ""
            '
            ' can never return an error back to the calling routine
            '
            Try
                returnString = GetStatusPage(Cmd, arguments, "1")
            Catch ex As Exception
                Call reportError(ex.ToString, "ipDaemonCallback")
            End Try
            Return returnString
        End Function
        '
        '
        '
        Private Function StatusLine(Indent As Integer, Copy As String) As String
            Dim Cnt As Integer
            StatusLine = vbCrLf & "<BR>"
            If Indent > 0 Then
                For Cnt = 1 To Indent * 2
                    StatusLine = StatusLine & "&nbsp;"
                Next
            End If
            StatusLine = StatusLine & Copy
        End Function
        '
        '
        '
        Private Function GetStatusPage(RequestPath As String, RequestQuery As String, remoteHost As String) As String
            Dim returnHtml As String = ""
            Try
                '
                Dim LogFileStruct As IO.FileInfo()

                Dim Content As String = ""
                Dim FreeSpace As Double
                Dim AppLogPtr As Integer
                Dim AppName As String
                Dim Meg As Double
                Dim LoopCount As Integer
                Dim errors() As String
                Dim ErrorCount As Integer
                Dim ErrorPtr As Integer
                Dim LogCnt As Integer
                Dim LargestLogSize As Integer
                Dim ClearAppErrors As Boolean
                Dim monitorConfig As monitorConfigClass
                Dim DisplayStatusMethod As Boolean
                Dim logMessage As String
                Dim cpApp As CPClass
                Dim cp As New CPClass("")
                '
                monitorConfig = New monitorConfigClass(cpcore)
                '
                ReDim errors(0)
                '
                ClearAppErrors = (Left(LCase(RequestPath), 6) = "/reset")
                DisplayStatusMethod = ClearAppErrors Or (Left(LCase(RequestPath), Len(monitorConfig.StatusMethod) + 1) = "/" & vbLCase(monitorConfig.StatusMethod))
                logMessage = "GetStatusPage hit, RequestPath=" & RequestPath & ", from " & remoteHost
                If Not (DisplayStatusMethod Or ClearAppErrors) Then
                    '
                    ' unknown hit
                    '
                    logMessage = logMessage & ", connection reset"
                    'Call IPDaemon1.Disconnect(connId)
                Else
                    '
                    ' status or reset
                    '
                    'Control = New ControlClass
                    'Control = CreateObject("ccCSrvr3.ControlClass")
                    'KernelServices = CreateObject("ccKrnl.KernelServicesClass")
                    '
                    ' Check for clear
                    '
                    If ClearAppErrors Then
                        logMessage = logMessage & ", logs cleared"
                        If (AppLogCnt > 0) Then
                            'hint = "Set AppLog().LastCheckOK from /RESET flag"
                            For AppLogPtr = 0 To AppLogCnt - 1
                                'hint = "Set AppLog().LastCheckOK from /RESET flag, AppLogPtr=" & AppLogPtr
                                AppLog(AppLogPtr).LastCheckOK = True
                            Next
                        End If
                    End If
                    '
                    ' Check for status method
                    '
                    If DisplayStatusMethod Then
                        '
                        ' Commands
                        '
                        logMessage = logMessage & ", status displayed"
                        Content = Content & StatusLine(0, "")
                        Content = Content & StatusLine(0, "Commands")
                        Content = Content & StatusLine(1, "<a href=""/Reset"">Clear Last Application Errors</a>")
                        '
                        ' Parameters
                        '
                        Content = Content & StatusLine(0, "")
                        Content = Content & StatusLine(0, "Parameters")
                        If ClearAppErrors Then
                            Content = Content & StatusLine(1, "Clear Application Error Flag")
                        End If
                        Content = Content & StatusLine(1, "Minimum Drive Space: " & monitorConfig.DiskSpaceMinMb & " MB")
                        Content = Content & StatusLine(1, "Maximum Log Size: " & monitorConfig.LogFileSizeMax & " Bytes")
                        Content = Content & StatusLine(1, "Site Timeout: " & monitorConfig.SiteTimeout & " seconds")
                        Content = Content & StatusLine(1, "Each site must show email activity 1 time per day")
                        Content = Content & StatusLine(1, "Clear Errors every monitor check: " & CStr(monitorConfig.ClearErrorsOnMonitorHit))
                        Content = Content & StatusLine(1, "Attempt to recover app when error detected: " & CStr(monitorConfig.allowErrorRecovery))
                        Content = Content & StatusLine(1, "When recovering error, use IIS Reset (not just App Pool recycle): " & CStr(monitorConfig.allowIISReset))
                        Content = Content & StatusLine(1, "Schedule: " & CStr(monitorConfig.scheduleList))
                        '
                        ' Drive Space
                        '
                        AppName = ""
                        'hint = "Checking drives for space available"
                        Content = Content & StatusLine(0, "")
                        Content = Content & StatusLine(0, "Drive Space Check")
                        Meg = 1024
                        Meg = Meg * 1024
                        LoopCount = 0
                        Dim drives2() As System.IO.DriveInfo
                        Dim drive2 As System.IO.DriveInfo
                        drives2 = System.IO.DriveInfo.GetDrives()
                        For Each drive2 In drives2
                            'hint = "Checking drive " & drive2.Name & " for READY"
                            If drive2.IsReady Then
                                'hint = "Checking drive " & drive2.Name & " for space available"
                                FreeSpace = Int((drive2.AvailableFreeSpace / Meg) + 0.5)
                                Content = Content & StatusLine(1, "Drive " & drive2.Name & " FreeSpace " & FreeSpace & " MB")
                                If FreeSpace < monitorConfig.DiskSpaceMinMb Then
                                    ReDim Preserve errors(ErrorCount)
                                    errors(ErrorCount) = "ERROR - All ready drives must have at least " & monitorConfig.DiskSpaceMinMb & " MB free"
                                    Content = Content & StatusLine(2, errors(ErrorCount))
                                    ErrorCount = ErrorCount + 1
                                End If
                            Else
                                Content = Content & StatusLine(1, "Drive " & drive2.Name & " not Ready")
                            End If
                            LoopCount = LoopCount + 1
                            If LoopCount > 100 Then
                                ReDim Preserve errors(ErrorCount)
                                errors(ErrorCount) = "ERROR - Drive limit of 100 exceeded."
                                Content = Content & StatusLine(2, errors(ErrorCount))
                                ErrorCount = ErrorCount + 1
                                Exit For
                            End If
                        Next
                        '
                        ' Log Size
                        '
                        Content = Content & StatusLine(0, "")
                        Content = Content & StatusLine(0, "Log Check")
                        LogFileStruct = cpCore.appRootFiles.getFileList(getProgramFilesPath() & "logs")
                        For Each logFile As IO.FileInfo In LogFileStruct
                            If logFile.Length > LargestLogSize Then
                                LargestLogSize = EncodeInteger(logFile.Length)
                            End If
                            If logFile.Length > monitorConfig.LogFileSizeMax Then
                                ReDim Preserve errors(ErrorCount)
                                errors(ErrorCount) = "ERROR - A log file exceeds the limit of " & monitorConfig.LogFileSizeMax & " bytes."
                                Content = Content & StatusLine(1, errors(ErrorCount))
                                ErrorCount = ErrorCount + 1
                                Exit For
                            End If
                        Next
                        Content = Content & StatusLine(1, "Logs found " & LogCnt)
                        If monitorConfig.LogFileSizeMax <= 0 Then
                            Content = Content & StatusLine(1, "Max log size " & LargestLogSize & " bytes.")
                        Else
                            Content = Content & StatusLine(1, "Max log size " & LargestLogSize & " bytes, " & Int((LargestLogSize * 100) / monitorConfig.LogFileSizeMax) & "% of Maximum.")
                        End If
                        '
                        ' Alarms Log - if not empty ring alarm and diplay content
                        '
                        Content = Content & StatusLine(0, "")
                        Content = Content & StatusLine(0, "Alarms Log Check")
                        LogFileStruct = cpCore.appRootFiles.getFileList(getProgramFilesPath() & "\logs\alarms")
                        If LogFileStruct.Count = 0 Then
                            Content = Content & StatusLine(1, "No alarm logs.")
                        Else
                            ReDim Preserve errors(ErrorCount)
                            errors(ErrorCount) = "ERROR - Alarms log folder is not empty."
                            Content = Content & StatusLine(1, errors(ErrorCount))
                            ErrorCount = ErrorCount + 1
                        End If
                        '
                        '
                        '
                        If Not monitorConfig.isInSchedule Then
                            '
                            ' unscheduled time
                            '
                            Content = Content & StatusLine(1, "The monitor is not scheduled to run during this period.")
                        Else
                            '
                            ' Check Applications and monitor log
                            '
                            'hint = "Getting application list for Kernel"
                            Content = Content & StatusLine(0, "")
                            Content = Content & StatusLine(0, "Applications")

                            For Each kvp As KeyValuePair(Of String, Models.Entity.serverConfigModel.appConfigModel) In cpCore.serverConfig.apps
                                AppName = kvp.Value.name
                                cpApp = New CPClass(AppName)
                                If cpApp.core.serverConfig.appConfig.allowSiteMonitor Then


                                    'hint = "Checking status for application [" & AppName & "]"
                                    Select Case cpApp.core.serverConfig.appConfig.appStatus
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusConnectionObjectFailure
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Connection Object failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusConnectionStringFailure
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Connection String Failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDataSourceFailure
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Data Source Failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDbBad
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Bad Database")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusDbNotFound
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Database Failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusKernelFailure
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned Kernel Failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusLicenseFailure
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned License Failure")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusNoHostService
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned ccServer Service is not running")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusNotFound
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application not found")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusNotEnabled
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application not running")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                                'Case ccCommonModule.applicationStatusEnum.ApplicationStatusStarting
                                                '    ReDim Preserve errors(ErrorCount)
                                                '    errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application starting")
                                                '    Content = Content & StatusLine(2, errors(ErrorCount))
                                                '    ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusUpgrading
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned application upgrading")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                        Case Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady
                                            If True Then
                                                '
                                                ' access content server for this application
                                                '
                                                If True Then
                                                    Dim link As String
                                                    'hint = "Returning monitor log entry for application [" & AppName & "]"
                                                    link = AppLog(AppLogPtr).SiteLink
                                                    Content = Content & StatusLine(2, "Link [<a target=_blank href=""" & link & """>" & link & "</a>]")
                                                    link = AppLog(AppLogPtr).StatusLink
                                                    Content = Content & StatusLine(2, "status Link [<a target=_blank href=""" & link & """>" & link & "</a>]")
                                                    Content = Content & StatusLine(2, "status Checks [" & AppLog(AppLogPtr).StatusCheckCount & "]")
                                                    Content = Content & StatusLine(2, "status Response [" & AppLog(AppLogPtr).LastStatusResponse & "]")
                                                    If Not AppLog(AppLogPtr).LastCheckOK Then
                                                        ReDim Preserve errors(ErrorCount)
                                                        errors(ErrorCount) = ("[" & AppName & "] ERROR - Last status check returned [" & AppLog(AppLogPtr).LastStatusResponse & "]")
                                                        ErrorCount = ErrorCount + 1
                                                    End If
                                                    If AppLog(AppLogPtr).ErrorCount <> 0 Then
                                                        If monitorConfig.ClearErrorsOnMonitorHit Then
                                                            Content = Content & StatusLine(2, "errors detected since last monitor hit [" & AppLog(AppLogPtr).ErrorCount & "]")
                                                            AppLog(AppLogPtr).ErrorCount = 0
                                                            AppLog(AppLogPtr).LastCheckOK = True
                                                        Else
                                                            Content = Content & StatusLine(2, "errors detected since monitor started [" & AppLog(AppLogPtr).ErrorCount & "]")
                                                        End If
                                                    Else
                                                        Content = Content & StatusLine(2, "no monitor errors detected")
                                                    End If
                                                End If
                                                '
                                                ' Check Last Email drop
                                                '
                                                Dim EmailServicelastCheck As Date
                                                EmailServicelastCheck = cp.Site.GetDate("EmailServicelastCheck")
                                                If EmailServicelastCheck < (Now().AddDays(-1)) Then
                                                    '
                                                    ' Email has not sent in a day
                                                    '
                                                    ReDim Preserve errors(ErrorCount)
                                                    errors(ErrorCount) = ("[" & AppName & "] ERROR - Group email drop has not sent in the last 24 hours")
                                                    Content = Content & StatusLine(2, errors(ErrorCount))
                                                    ErrorCount = ErrorCount + 1
                                                Else
                                                    '
                                                    ' Email send is OK
                                                    '
                                                    Content = Content & StatusLine(2, "Group email drop OK [" & EmailServicelastCheck & "]")
                                                End If
                                                '
                                                ' Get Misc properties
                                                '
                                                'hint = "Get AppServices object for [" & AppName & "]"
                                                Content = Content & StatusLine(2, "Contensive Version [" & cp.Version & "]")
                                                Content = Content & StatusLine(2, "Data Build Version [" & cpCore.siteProperties.dataBuildVersion & "]")
                                                Content = Content & StatusLine(2, "Active Connections [" & "AppServices.ConnectionsActive" & "]")
                                                Content = Content & StatusLine(2, "Started [" & "AppServices.DateStarted" & "]")
                                                Content = Content & StatusLine(2, "Hits [" & "AppServices.HitCounter" & "]")
                                                Content = Content & StatusLine(2, "Errors [" & "AppServices.ErrorCount" & "]")
                                                '
                                                Call cp.Dispose()
                                                cp = Nothing
                                            End If
                                            'Case ccCommonModule.applicationStatusEnum.ApplicationStatusPaused
                                            '    '
                                            '    ' Paused
                                            '    '
                                            '    Content = Content & StatusLine(2, "Application is paused")
                                        Case Else
                                            ReDim Preserve errors(ErrorCount)
                                            errors(ErrorCount) = ("[" & AppName & "] ERROR - Contensive returned unknown error")
                                            Content = Content & StatusLine(2, errors(ErrorCount))
                                            ErrorCount = ErrorCount + 1
                                    End Select
                                End If
                            Next
                        End If
                        '
                        ' Return page
                        '
                        If ErrorCount > 0 Then
                            returnHtml = "" _
                                & vbCrLf & "<PRE>" _
                                & vbCrLf & "ERRORS " & ErrorCount
                            For ErrorPtr = 0 To ErrorCount - 1
                                returnHtml &= StatusLine(1, errors(ErrorPtr))
                            Next
                            returnHtml &= "" _
                                & vbCrLf & "<BR>" _
                                & vbCrLf & Content _
                                & vbCrLf & "</PRE>" _
                                & ""
                        Else
                            returnHtml = "" _
                                & vbCrLf & "<PRE>" _
                                & vbCrLf & "ERRORS 0" _
                                & vbCrLf & "<BR>" _
                                & vbCrLf & Content _
                                & vbCrLf & "</PRE>" _
                                & ""

                        End If
                    End If
                End If
                Call appendMonitorLog(logMessage)
            Catch ex As Exception
                Call reportError(ex.ToString, "getStatusPage")
            End Try
            Return returnHtml
        End Function
        '
        '
        '
        Private Sub appendMonitorLog(ByVal message As String)
            cpCore.log_appendLog(message, "Monitor")
        End Sub
        '
        '
        '
        Private Sub reportError(ByVal exToString As String, ByVal methodName As String)
            Call appendMonitorLog("unexepected error in " & methodName & ", " & exToString)
        End Sub

        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpcore = cpCore
        End Sub
    End Class
End Namespace
