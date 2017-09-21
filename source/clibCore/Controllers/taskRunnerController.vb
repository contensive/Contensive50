
Option Explicit On
Option Strict On
'
Imports Contensive.Core
Imports Contensive.Core.Controllers
'
Namespace Contensive.Core.Controllers
#Const includeTracing = False
    Public Class taskRunnerController
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
        'Private cpCore As cpCoreClass
        '
        Private runnerGuid As String                    ' set in constructor. used to tag tasks assigned to this runner
        '
        ' ----- Log File
        '
        Private Const LogMsg = "For more information, see the Contensive Trace Log."
        Public allowVerboseLogging As Boolean = True
        Public allowConsoleWrite As Boolean = False
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
        'Private SiteProcessAlarmTime As Date            ' Run Site Processes every 30 seconds
        Const SiteProcessIntervalSeconds = 30           '
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
        Public Sub New()
            'Me.cpCore = cpCore
            runnerGuid = Guid.NewGuid().ToString
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
                    ' cpCore.dispose()
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
        Public Sub stopService()
            processTimer.Enabled = False
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Process the Start signal from the Server Control Manager
        ''' </summary>
        ''' <param name="setVerbose"></param>
        ''' <param name="singleThreaded"></param>
        ''' <returns></returns>
        Public Function StartService() As Boolean
            Dim returnStartedOk As Boolean = True
            processTimer = New System.Timers.Timer(ProcessTimerMsecPerTick)
            AddHandler processTimer.Elapsed, AddressOf processTimerTick
            processTimer.Interval = ProcessTimerMsecPerTick
            processTimer.Enabled = True
            Return returnStartedOk
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Timer tick
        ''' </summary>
        Protected Sub processTimerTick(sender As Object, e As EventArgs)
            Try
                '
                appendLog("taskRunnerService.processTimerTick")
                '
                If (ProcessTimerInProcess) Then
                    '
                    appendLog("taskRunnerService.processTimerTick, processTimerInProcess true, skip")
                    '
                Else
                    ProcessTimerInProcess = True
                    '
                    ' run tasks in task
                    '
                    Using cpCluster As New CPClass
                        Using programDataFiles As New fileController(cpCluster.core, cpCluster.core.serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) + "clib\")
                            Dim JSONTemp = programDataFiles.readFile("serverConfig.json")
                            Dim serverConfig As Models.Entity.serverConfigModel = cpCluster.core.json.Deserialize(Of Models.Entity.serverConfigModel)(JSONTemp)
                            If (Not serverConfig.allowTaskRunnerService) Then
                                appendLog("taskRunnerService.processTimerTick, allowTaskRunnerService false, skip")
                            Else
                                Call runTasks(cpCluster.core)
                            End If
                        End Using
                    End Using
                    ProcessTimerInProcess = False
                End If
            Catch ex As Exception
                Using cp As New CPClass
                    cp.core.handleException(ex)
                End Using
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Iterate through all apps, find addosn that need to run and add them to the task queue
        ''' </summary>
        Private Sub runTasks(cpClusterCore As coreClass)
            Try
                '
                Dim command As String
                Dim cmdDetail As cmdDetailClass
                Dim cmdDetailText As String
                Dim recordsRemaining As Boolean
                '
                'Dim SQLNow As String
                Dim CS As Integer
                Dim sql As String
                Dim AppName As String
                '
                appendLog("taskRunnerService.runTasks")
                '
                For Each kvp As KeyValuePair(Of String, Models.Entity.serverConfigModel.appConfigModel) In cpClusterCore.serverConfig.apps
                    AppName = kvp.Value.name
                    '
                    appendLog("taskRunnerService.runTasks, appname=[" & AppName & "]")
                    '
                    ' query tasks that need to be run
                    '
                    Using cpSite As New CPClass(AppName)
                        If (cpSite.core.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.ready) And (cpSite.core.serverConfig.appConfig.appMode = Models.Entity.serverConfigModel.appModeEnum.normal) Then
                            Try
                                Do
                                    '
                                    ' for now run an sql to get processes, eventually cache in variant cache
                                    recordsRemaining = False
                                    sql = "" _
                                    & vbCrLf & " BEGIN TRANSACTION" _
                                    & vbCrLf & " update cctasks set cmdRunner=" & cpSite.core.db.encodeSQLText(runnerGuid) & " where id in (select top 1 id from cctasks where (cmdRunner is null)and(datestarted is null))" _
                                    & vbCrLf & " COMMIT TRANSACTION"
                                    cpSite.core.db.executeQuery(sql)
                                    CS = cpSite.core.db.cs_open("tasks", "(cmdRunner=" & cpSite.core.db.encodeSQLText(runnerGuid) & ")and(datestarted is null)", "id")
                                    If cpSite.core.db.cs_ok(CS) Then
                                        'Dim json As New System.Web.Script.Serialization.JavaScriptSerializer
                                        recordsRemaining = True
                                        Call cpSite.core.db.cs_set(CS, "datestarted", Now())
                                        Call cpSite.core.db.cs_save2(CS)
                                        '
                                        command = cpSite.core.db.cs_getText(CS, "command")
                                        cmdDetailText = cpSite.core.db.cs_getText(CS, "cmdDetail")
                                        cmdDetail = cpSite.core.json.Deserialize(Of cmdDetailClass)(cmdDetailText)
                                        '
                                        appendLog("taskRunnerService.runTasks, command=[" & command & "], cmdDetailText=[" & cmdDetailText & "]")
                                        '
                                        Select Case command.ToLower()
                                            Case taskQueueCommandEnumModule.runAddon
                                                Call cpSite.core.addon.execute(
                                                    Models.Entity.addonModel.create(
                                                        cpSite.core, cmdDetail.addonId),
                                                        New BaseClasses.CPUtilsBaseClass.addonExecuteContext With {
                                                            .addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                            .instanceArguments = cmdDetail.docProperties
                                                        }
                                                    )
                                                'Call cpSite.core.addon.execute_legacy7(cmdDetail.addonId, cmdDetail.docProperties, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple)
                                        End Select
                                    End If
                                    cpSite.core.db.cs_Close(CS)
                                Loop While recordsRemaining
                            Catch ex As Exception
                                cpClusterCore.handleException(ex)
                            End Try
                        End If
                    End Using
                Next
            Catch ex As Exception
                cpClusterCore.handleException(ex)
            End Try
        End Sub
        '
        Private Sub appendLog(ByVal logText As String, Optional isImportant As Boolean = False)
            Using cp As New CPClass
                If (isImportant Or allowVerboseLogging) Then
                    logController.appendLog(cp.core, logText, "", "trace")
                End If
                If (allowConsoleWrite) Then
                    Console.WriteLine(logText)
                End If
            End Using
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
