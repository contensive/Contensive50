
Option Explicit On
Option Strict On
'
Imports Contensive.Core
Imports Contensive.Core.Controllers
Imports Contensive.Core.Models.Complex
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
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
        Private Property runnerGuid As String                    ' set in constructor. used to tag tasks assigned to this runner
        '
        ' ----- Task Timer
        '
        Private Property processTimer As System.Timers.Timer
        Const ProcessTimerMsecPerTick = 5000            ' Check processs every 5 seconds
        Private Property ProcessTimerInProcess As Boolean        '
        '
        ' ----- Alarms within Process Timer
        '
        'Private SiteProcessAlarmTime As Date            ' Run Site Processes every 30 seconds
        Const SiteProcessIntervalSeconds = 30           '
        '
        ' ----- Debugging
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
        Public Sub stopTimerEvents()
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
        Public Function startTimerEvents() As Boolean
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
                Console.WriteLine("taskRunnerService.processTimerTick")
                '
                If (ProcessTimerInProcess) Then
                    '
                    Console.WriteLine("taskRunnerService.processTimerTick, processTimerInProcess true, skip")
                Else
                    ProcessTimerInProcess = True
                    '
                    ' run tasks in task
                    '
                    Using cpCluster As New CPClass
                        If (Not cpCluster.core.serverConfig.allowTaskRunnerService) Then
                            Console.WriteLine("taskRunnerService.processTimerTick, allowTaskRunnerService false, skip")
                        Else
                            Call runTasks(cpCluster.core)
                        End If
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
                Console.WriteLine("taskRunnerController.runTasks")
                Dim sw As New Stopwatch()
                sw.Start()
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
                Console.WriteLine("taskRunnerService.runTasks")
                '
                For Each kvp As KeyValuePair(Of String, serverConfigModel.appConfigModel) In cpClusterCore.serverConfig.apps
                    AppName = kvp.Value.name
                    '
                    Console.WriteLine("taskRunnerService.runTasks, appname=[" & AppName & "]")
                    '
                    ' query tasks that need to be run
                    '
                    Using cpSite As New CPClass(AppName)
                        If (cpSite.core.serverConfig.appConfig.appStatus = serverConfigModel.appStatusEnum.OK) And (cpSite.core.serverConfig.appConfig.appMode = serverConfigModel.appModeEnum.normal) Then
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
                                    CS = cpSite.core.db.csOpen("tasks", "(cmdRunner=" & cpSite.core.db.encodeSQLText(runnerGuid) & ")and(datestarted is null)", "id")
                                    If cpSite.core.db.csOk(CS) Then
                                        '
                                        Console.WriteLine("taskRunnerController.runTasks, execute task [" & cpSite.core.db.csGetText(CS, "name") & "]")
                                        '
                                        ' -- execute a task
                                        recordsRemaining = True
                                        Call cpSite.core.db.csSet(CS, "datestarted", Now())
                                        Call cpSite.core.db.csSave2(CS)
                                        '
                                        command = cpSite.core.db.csGetText(CS, "command")
                                        cmdDetailText = cpSite.core.db.csGetText(CS, "cmdDetail")
                                        cmdDetail = cpSite.core.json.Deserialize(Of cmdDetailClass)(cmdDetailText)
                                        '
                                        Console.WriteLine("taskRunnerService.runTasks, command=[" & command & "], cmdDetailText=[" & cmdDetailText & "]")
                                        '
                                        Select Case command.ToLower()
                                            Case taskQueueCommandEnumModule.runAddon
                                                Call cpSite.core.addon.execute(
                                                    addonModel.create(
                                                        cpSite.core, cmdDetail.addonId),
                                                        New BaseClasses.CPUtilsBaseClass.addonExecuteContext With {
                                                            .addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                            .instanceArguments = cmdDetail.docProperties
                                                        }
                                                    )
                                                'Call cpSite.core.addon.execute_legacy7(cmdDetail.addonId, cmdDetail.docProperties, Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple)
                                        End Select
                                        Call cpSite.core.db.csSet(CS, "datecompleted", Now())
                                    End If
                                    cpSite.core.db.csClose(CS)
                                Loop While recordsRemaining
                            Catch ex As Exception
                                cpClusterCore.handleException(ex)
                            End Try
                        End If
                    End Using
                Next
                Console.WriteLine("taskRunnerController.runTasks, exit (" & sw.ElapsedMilliseconds & "ms)")
            Catch ex As Exception
                cpClusterCore.handleException(ex)
            End Try
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
