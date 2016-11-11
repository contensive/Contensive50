﻿
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports Contensive.Core

Namespace Contensive
#Const includeTracing = False
    Public Class taskSchedulerClass
        Implements IDisposable
        '
        '==================================================================================================
        '   Code copied from workerClass to both taskScheduler and taskRunner - remove what does not apply
        '
        '   taskScheduler queries each application from addons that need to run and adds them to the tasks queue (tasks sql table or SQS queue)
        '       taskScheduleTimer
        '
        '   taskRunner polls the task queue and runs commands when found
        '       taskRunnerTimer
        '==================================================================================================
        '
        Private cpCore As cpCoreClass
        '
        ' ----- Log File
        '
        Private Const LogMsg = "For more information, see the Contensive Trace Log."
        Private verboseLogging As Boolean = False
        '
        ' ----- Task Timer
        '
        Private processTimer As System.Timers.Timer
        Private ProcessTimerTickCnt As Integer
        Const ProcessTimerMsecPerTick = 5000            ' Check processs every 5 seconds
        Private ProcessTimerInProcess As Boolean        '
        Private ProcessTimerProcessCount As Integer        '
        '
        ' ----- Alarms within Process Timer
        '
        Private SiteProcessAlarmTime As Date            ' Run Site Processes every 60 seconds
        Const SiteProcessIntervalSeconds = 60           '
        '
        ' ----- Debugging
        '
        Public StartServiceInProgress As Boolean
        '
        Protected disposed As Boolean = False
        '
        '========================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
            verboseLogging = False
            processTimer = New System.Timers.Timer(5000)
            AddHandler processTimer.Elapsed, AddressOf processTimerTick
        End Sub
        '
        '========================================================================================================
        ''' <summary>
        ''' dispose
        ''' </summary>
        ''' <param name="disposing"></param>
        ''' <remarks></remarks>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cpCore.dispose()
                End If
                '
                ' cp  creates and destroys cmc
                '
                GC.Collect()
            End If
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Stop all activity through the content server, but do not unload
        ''' </summary>
        Public Sub stopServer()
            Try
                processTimer.Enabled = False
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Process the Start signal from the Server Control Manager
        ''' </summary>
        ''' <param name="setVerbose"></param>
        ''' <param name="singleThreaded"></param>
        ''' <returns></returns>
        Public Function StartServer(ByVal setVerbose As Boolean, ByVal singleThreaded As Boolean) As Boolean
            Dim returnStartedOk As Boolean = False
            Dim MethodName As String = "StartServer"
            Dim ErrorHint As String = "enter"
            Try
                '
                Dim StatusMessage As String
                Dim EmailFolder As String
                Dim LogFolder As String
                '
                returnStartedOk = False
                verboseLogging = setVerbose
                '
                If StartServiceInProgress Then
                    '
                    ' Attempting to start service, and it is still starting
                    '
                    StatusMessage = "Attempting a service start retry, but it is still starting"
                    cpCore.appendLog(MethodName & ", " & StatusMessage)
                Else
                    StartServiceInProgress = True
                    '
                    ' Make sure the start timer is running
                    ' It goes off initially 1 second after the service is called to start
                    ' Then it goes off every 60 seconds until the service is started.
                    ' It is to debug a problem with the service not starting on reboot
                    '
                    If Not singleThreaded Then
                        processTimer.Interval = ProcessTimerMsecPerTick
                        processTimer.Enabled = True
                    End If
                    '
                    ' Log status
                    '
                    StatusMessage = "Starting Contensive Service"
                    'Errorhint = "Creating the Kernel Services Object - This is often a problem with DCom permissions. Check permissions with dcomcnfg. The ccKrnl object must start as a User, and everyone must have permission to start and access the object. "
                    'KernelServices = New kernelServicesClass
                    'Errorhint = ""
                    If (False) Then
                        '
                        ' ----- Kernelservices failed to initialize
                        '
                        StatusMessage = "Contensive Kernel Services failed to initialize."
                        cpCore.appendLog(MethodName & ", " & StatusMessage)
                    Else
                        If False Then
                            'If KernelServices.HostServiceProcessID <> 0 Then
                            '
                            ' ----- Already started by another HostService
                            '
                            StatusMessage = "Contensive Kernel Services is already running."
                            cpCore.appendLog(MethodName & ", " & StatusMessage)
                        Else
                            '
                            ' No license is trial license
                            '
                            If True Then
                                '
                                ' ----- Set shared Mail out folder
                                '
                                EmailFolder = "\EmailOut"
                                If Not cpCore.cluster.clusterFiles.checkPath(EmailFolder) Then
                                    Call cpCore.cluster.clusterFiles.createPath(EmailFolder)
                                End If
                                '
                                ' ----- Set shared log folder
                                '
                                LogFolder = "\Logs"
                                If Not cpCore.cluster.clusterFiles.checkPath(LogFolder) Then
                                    Call cpCore.cluster.clusterFiles.createPath(LogFolder)
                                End If
                                '
                                ' ----- Create ServerConfig object
                                '
                                'Errorhint = ""
                                If True Then
                                    '
                                    ' ----- Load the configuration file
                                    '
                                    'Errorhint = "Calling Control Object to load applications into server"
                                    'AdminUsername = cpCore.cluster.config.username
                                    'AdminPassword = cpCore.cluster.config.password
                                    'serverListenerPort = cpCore.cluster.config.serverListenerPort
                                    'maxConcurrentTasksPerServer = cpCore.cluster.config.maxConcurrentTasksPerServer
                                    'KernelServices.ServerLicense = ServerConfig.ServerLicense
                                    '
                                    ' ----- Turn on the tcpListener
                                    '
                                    'Errorhint = "Starting tcpListener"
                                    '
                                    'Call tcpListenerStart(serverListenerPort)
                                    'Call ipListener_start(serverListenerPort)
                                    'IPConn.LocalPort = serverListenerPort
                                    'IPConn.Listening = True
                                    '
                                    ' ----- Turn on the event timer
                                    '
                                    'Errorhint = "Starting Event Timers"
                                    '
                                    '!!!!! THERE IS NO LONGER ANY START SITES.
                                    '' Call AutoStartSites()
                                    'If True Then
                                    '
                                    processTimer.Interval = ProcessTimerMsecPerTick
                                    processTimer.Enabled = True
                                    '
                                    ' ----- set flags
                                    '
                                    returnStartedOk = True
                                End If
                            End If
                        End If
                    End If
                    '
                    ' Log status
                    '
                    If returnStartedOk Then
                        '
                        ' ----- Success
                        '
                        'Errorhint = "StartService Success, Logging Status"
                        'startTimer.Enabled = False
                        'StatusMessage = "Contensive v" & cpCore.version() & " started"
                        'cpCore.AppendLog("StartService", StatusMessage)
                    Else
                        '
                        ' ----- Start failed
                        '
                        'Errorhint = "StartService Failure, Logging Status"
                        StatusMessage = "Contensive Service failed to start."
                        'cpCore.AppendLog("StartService", StatusMessage)
                        Call stopServer()
                        'If Not (KernelServices Is Nothing) Then
                        '    KernelServices.HostServiceProcessID = 0
                        '    KernelServices = Nothing
                        'End If
                    End If
                    StartServiceInProgress = False
                    'Call appendTraceLog("", Now.ToLongTimeString & " " & StatusMessage)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnStartedOk
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Timer tick
        ''' </summary>
        Private Sub processTimerTick()
            Try
                '
                Dim rightNow As Date
                '
                rightNow = Now
                If Not ProcessTimerInProcess Then
                    '
                    ' If not debug and not inprocess, run processes
                    '
                    ProcessTimerInProcess = True
                    If True Then
                        '
                        ' Run addons from each app in cluster
                        '
                        If (True) Then
                            'AllSitesNotReadyCnt = 0
                            '
                            ' start process timer 1 minute after service starts
                            '
                            If SiteProcessAlarmTime = Date.MinValue Then
                                SiteProcessAlarmTime = rightNow.AddMinutes(1)
                            ElseIf rightNow > SiteProcessAlarmTime Then
                                Call scheduleTasks()
                                SiteProcessAlarmTime = SiteProcessAlarmTime.AddMinutes(1)
                                If rightNow > SiteProcessAlarmTime Then
                                    SiteProcessAlarmTime = rightNow.AddMinutes(1)
                                End If
                            End If
                        End If
                    End If
                    ProcessTimerInProcess = False
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Iterate through all apps, find addosn that need to run and add them to the task queue
        ''' </summary>
        Private Sub scheduleTasks()
            Dim hint As String = ""
            Try
                '
                Dim addonId As Integer
                Dim addonName As String
                Dim addonArguments As String
                Dim addonProcessRunOnce As Boolean
                Dim addonProcessNextRun As Date
                Dim addonProcessInterval As Integer
                Dim cmdDetail As cmdDetailClass
                '
                Dim SQLNow As String
                Dim CS As Integer
                Dim sqlAddonsCriteria As String
                Dim NextRun As Date
                Dim RightNow As Date
                Dim AppName As String
                Dim cpSite As CPClass
                '
                Console.WriteLine("taskScheduler.scheduleTasks")
                '
                RightNow = Now
                SQLNow = EncodeSQLDate(RightNow)
                For Each kvp As KeyValuePair(Of String, appConfigClass) In cpCore.cluster.config.apps
                    AppName = kvp.Value.name
                    '
                    ' schedule tasks for this app
                    '
                    cpSite = New CPClass(AppName)
                    If cpSite.core.app.status = applicationStatusEnum.ApplicationStatusReady Then
                        hint &= ",app [" & AppName & "] is running, setup cp and cmc"
                        '
                        ' Execute Processes
                        '
                        Try
                            hint &= ",go through process addons that need to be run"
                            '
                            ' for now run an sql to get processes, eventually cache in variant cache
                            '
                            sqlAddonsCriteria = "" _
                                & "(Active<>0)" _
                                & " and(name<>'')" _
                                & " and(" _
                                & "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))" _
                                & "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))" _
                                & "  or(ProcessNextRun<" & SQLNow & ")" _
                                & " )"
                            CS = cpSite.core.app.db_csOpen("add-ons", sqlAddonsCriteria)
                            Do While cpSite.core.app.db_csOk(CS)
                                addonProcessInterval = cpSite.core.app.db_GetCSInteger(CS, "ProcessInterval")
                                addonId = cpSite.core.app.db_GetCSInteger(CS, "ID")
                                addonName = cpSite.core.app.db_GetCSText(CS, "name")
                                addonArguments = cpSite.core.app.db_GetCSText(CS, "argumentlist")
                                addonProcessRunOnce = cpSite.core.app.db_GetCSBoolean(CS, "ProcessRunOnce")
                                addonProcessNextRun = cpSite.core.app.db_GetCSDate(CS, "ProcessNextRun")
                                NextRun = Date.MinValue
                                hint &= ",run addon " & addonName
                                If addonProcessInterval > 0 Then
                                    NextRun = RightNow.AddMinutes(addonProcessInterval)
                                End If
                                If addonProcessRunOnce Then
                                    '
                                    ' Run Once
                                    '
                                    Call cpSite.core.app.db_setCS(CS, "ProcessRunOnce", False)
                                    Call cpSite.core.app.db_setCS(CS, "ProcessNextRun", "")
                                    Call cpSite.core.app.db_SaveCS(CS)
                                    '
                                    cmdDetail = New cmdDetailClass
                                    cmdDetail.addonId = addonId
                                    cmdDetail.addonName = addonName
                                    cmdDetail.docProperties = convertAddonArgumentstoDocPropertiesList(cpSite.core, addonArguments)
                                    Call addTaskToQueue(cpSite.core, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
                                ElseIf cpSite.core.app.db_GetCSDate(CS, "ProcessNextRun") = Date.MinValue Then
                                    '
                                    ' Interval is OK but NextRun is 0, just set next run
                                    '
                                    Call cpSite.core.app.db_setCS(CS, "ProcessNextRun", NextRun)
                                    Call cpSite.core.app.db_SaveCS(CS)
                                ElseIf addonProcessNextRun < RightNow Then
                                    '
                                    ' All is OK, triggered on NextRun, Cycle RightNow
                                    '
                                    Call cpSite.core.app.db_setCS(CS, "ProcessNextRun", NextRun)
                                    Call cpSite.core.app.db_SaveCS(CS)
                                    '
                                    cmdDetail = New cmdDetailClass
                                    cmdDetail.addonId = addonId
                                    cmdDetail.addonName = addonName
                                    cmdDetail.docProperties = convertAddonArgumentstoDocPropertiesList(cpSite.core, addonArguments)
                                    Call addTaskToQueue(cpSite.core, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
                                End If
                                Call cpSite.core.app.db_csGoNext(CS)
                            Loop
                            Call cpSite.core.app.db_csClose(CS)
                        Catch ex As Exception
                            cpCore.handleException(ex)
                        End Try
                    End If
                    hint &= ",app done"
                    cpSite.Dispose()
                Next
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Add a command task to the taskQueue to be run by the taskRunner
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="Command"></param>
        ''' <param name="cmdDetail"></param>
        ''' <param name="BlockDuplicates"></param>
        ''' <returns></returns>
        Private Function addTaskToQueue(cpCore As cpCoreClass, ByVal Command As String, ByVal cmdDetail As cmdDetailClass, ByVal BlockDuplicates As Boolean) As Boolean
            Dim returnTaskAdded As Boolean = True
            Try
                Dim LcaseCommand As String
                Dim sql As String
                Dim cmdDetailJson As String = cpCore.jsonSerialize(cmdDetail)
                Dim cs As Integer
                '
                Console.WriteLine("taskScheduler.addTaskToQueue, application=[" & cpCore.app.config.name & "], command=[" & Command & "], cmdDetail=[" & cmdDetailJson & "]")
                '
                returnTaskAdded = True
                LcaseCommand = LCase(Command)
                If BlockDuplicates Then
                    '
                    ' Search for a duplicate
                    '
                    sql = "select top 1 id from cctasks where ((command=" & EncodeSQLText(Command) & ")and(cmdDetail=" & cmdDetailJson & "))"
                    cs = cpCore.app.db_csOpenSql(sql)
                    If cpCore.app.db_csOk(cs) Then
                        returnTaskAdded = False
                    End If
                    Call cpCore.app.db_csClose(cs)
                End If
                '
                ' Add it to the queue and shell out to the command
                '
                If returnTaskAdded Then
                    cs = cpCore.app.db_csInsertRecord("tasks")
                    If cpCore.app.db_csOk(cs) Then
                        cpCore.app.db_SetCSField(cs, "name", "")
                        cpCore.app.db_SetCSField(cs, "command", Command)
                        cpCore.app.db_SetCSField(cs, "cmdDetail", cmdDetailJson)
                        returnTaskAdded = True
                    End If
                    Call cpCore.app.db_csClose(cs)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnTaskAdded
        End Function
        '
        '======================================================================================
        ''' <summary>
        ''' Convert addon argument list to a doc property compatible dictionary of strings
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="SrcOptionList"></param>
        ''' <returns></returns>
        Private Function convertAddonArgumentstoDocPropertiesList(cpCore As cpCoreClass, SrcOptionList As String) As Dictionary(Of String, String)
            Dim returnList As New Dictionary(Of String, String)
            Try
                Dim SrcOptions As String()
                Dim key As String
                Dim value As String
                Dim Pos As Integer
                '
                If Not String.IsNullOrEmpty(SrcOptionList) Then
                    SrcOptions = Split(SrcOptionList.Replace(vbCrLf, vbCr).Replace(vbLf, vbCr), vbCr)
                    For Ptr = 0 To UBound(SrcOptions)
                        key = SrcOptions(Ptr).Replace(vbTab, "")
                        If Not String.IsNullOrEmpty(key) Then
                            value = ""
                            Pos = InStr(1, key, "=")
                            If Pos > 0 Then
                                value = Mid(key, Pos + 1)
                                key = Mid(key, 1, Pos - 1)
                            End If
                            returnList.Add(key, value)
                        End If
                    Next
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnList
        End Function
        '
        Private Sub appendTraceLog(ByVal method As String, ByVal logText As String)
            cpCore.appendLog(logText, "", "trace")
        End Sub
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class

End Namespace
