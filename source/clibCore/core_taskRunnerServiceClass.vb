
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports Contensive.Core

Namespace Contensive
#Const includeTracing = False
    Public Class taskRunnerServiceClass
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
            Try
                '
                appendLog("taskRunnerService.stopService")
                '
                processTimer.Enabled = False
            Catch ex As Exception
                Using cp As New CPClass
                    cp.core.handleException(ex)
                End Using
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
        Public Function StartService() As Boolean
            Dim returnStartedOk As Boolean = True
            Try
                '
                appendLog("taskRunnerService.StartService")
                '
                processTimer = New System.Timers.Timer(ProcessTimerMsecPerTick)
                AddHandler processTimer.Elapsed, AddressOf processTimerTick
                processTimer.Interval = ProcessTimerMsecPerTick
                processTimer.Enabled = True
            Catch ex As Exception
                Using cp As New CPClass
                    cp.core.handleException(ex)
                End Using
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
                        Using programDataFiles As New fileSystemClass(cpCluster.core, cpCluster.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib")
                            Dim JSONTemp = programDataFiles.ReadFile("serverConfig.json")
                            Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
                            Dim serverConfig As serverConfigClass = json_serializer.Deserialize(Of serverConfigClass)(JSONTemp)
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
        Private Sub runTasks(cpClusterCore As cpCoreClass)
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
                For Each kvp As KeyValuePair(Of String, appConfigClass) In cpClusterCore.cluster.config.apps
                    AppName = kvp.Value.name
                    '
                    appendLog("taskRunnerService.runTasks, appname=[" & AppName & "]")
                    '
                    ' query tasks that need to be run
                    '
                    Using cpSite As New CPClass(AppName)
                        If cpSite.core.app.status = applicationStatusEnum.ApplicationStatusReady Then
                            'hint &= ",app [" & AppName & "] is running, setup cp and cmc"
                            '
                            ' Execute Processes
                            '
                            Try
                                'hint &= ",go through process addons that need to be run"
                                Do
                                    '
                                    ' for now run an sql to get processes, eventually cache in variant cache
                                    '
                                    recordsRemaining = False
                                    sql = "" _
                                    & vbCrLf & " BEGIN TRANSACTION" _
                                    & vbCrLf & " update cctasks set cmdRunner=" & EncodeSQLText(runnerGuid) & " where id in (select top 1 id from cctasks where (cmdRunner is null)and(datestarted is null))" _
                                    & vbCrLf & " COMMIT TRANSACTION"
                                    cpSite.core.app.executeSql(sql)
                                    CS = cpSite.core.app.db_csOpen("tasks", "(cmdRunner=" & EncodeSQLText(runnerGuid) & ")and(datestarted is null)", "id")
                                    If cpSite.core.app.db_csOk(CS) Then
                                        Dim json As New System.Web.Script.Serialization.JavaScriptSerializer
                                        recordsRemaining = True
                                        Call cpSite.core.app.db_setCS(CS, "datestarted", Now())
                                        Call cpSite.core.app.db_SaveCS(CS)
                                        '
                                        command = cpSite.core.app.db_GetCSText(CS, "command")
                                        cmdDetailText = cpSite.core.app.db_GetCSText(CS, "cmdDetail")
                                        cmdDetail = json.Deserialize(Of cmdDetailClass)(cmdDetailText)
                                        '
                                        appendLog("taskRunnerService.runTasks, command=[" & command & "], cmdDetailText=[" & cmdDetailText & "]")
                                        '
                                        Select Case command.ToLower()
                                            Case taskQueueCommandEnumModule.runAddon
                                                Call cpSite.core.executeAddon(cmdDetail.addonId, cmdDetail.docProperties, cpCoreClass.addonContextEnum.ContextSimple)
                                        End Select
                                    End If
                                    cpSite.core.app.db_csClose(CS)
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
                    cp.core.appendLog(logText, "", "trace")
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
