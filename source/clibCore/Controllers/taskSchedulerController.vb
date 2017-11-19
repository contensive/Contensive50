
Option Explicit On
Option Strict On
'
Imports Contensive.Core
Imports Contensive.Core.Controllers
'
Namespace Contensive.Core.Controllers
#Const includeTracing = False
    Public Class taskSchedulerController
        Implements IDisposable
        '
        'Private cpCore As cpCoreClass
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
        Const ProcessTimerMsecPerTick = 5100            ' Check processs every 5 seconds
        Private ProcessTimerInProcess As Boolean        '
        Private ProcessTimerProcessCount As Integer        '
        '
        ' ----- Debugging
        '
        Public StartServiceInProgress As Boolean
        '
        Protected disposed As Boolean = False
        ''
        ''========================================================================================================
        '''' <summary>
        '''' constructor
        '''' </summary>
        '''' <param name="cpCore"></param>
        '''' <remarks></remarks>
        'Public Sub New()
        '    MyBase.New
        'End Sub
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
                    'cpCore.dispose()
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
            Try
                'appendLog("taskScheduleServiceClass.stopService")
                processTimer.Enabled = False
            Catch ex As Exception
                Using cp As New CPClass()
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
        Public Function startTimerEvents(ByVal setVerbose As Boolean, ByVal singleThreaded As Boolean) As Boolean
            Dim returnStartedOk As Boolean = False
            Try
                'appendLog("taskScheduleServiceClass.startService")
                '
                If StartServiceInProgress Then
                    'appendLog("taskScheduleServiceClass.startService, startServiceInProgress true, skip.")
                Else
                    StartServiceInProgress = True
                    processTimer = New System.Timers.Timer(5000)
                    AddHandler processTimer.Elapsed, AddressOf processTimerTick
                    processTimer.Interval = ProcessTimerMsecPerTick
                    processTimer.Enabled = True
                    returnStartedOk = True
                    StartServiceInProgress = False
                End If
            Catch ex As Exception
                Using cp As New CPClass()
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
        Public Sub processTimerTick(sender As Object, e As EventArgs)
            Try
                Console.WriteLine("taskScheduleServiceClass.processTimerTick")
                Dim sw As New Stopwatch()
                sw.Start()
                If (ProcessTimerInProcess) Then
                    Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick, skipped because timerInProcess")
                Else
                    '
                    ' -- schedule tasks
                    ProcessTimerInProcess = True
                    Using cp As New CPClass()
                        If (Not cp.core.serverConfig.allowTaskSchedulerService) Then
                            Console.WriteLine("taskScheduleServiceClass.processTimerTick, skipped because serviceConfig.allowTaskSchedulerService false.")
                        Else
                            Console.WriteLine("taskScheduleServiceClass.processTimerTick, call scheduleTasks.")
                            Call scheduleTasks(cp.core)
                        End If
                        'End Using
                    End Using
                    ProcessTimerInProcess = False
                End If
                Console.WriteLine("taskScheduleServiceClass.processTimerTick exit (" & sw.ElapsedMilliseconds & "ms)")
            Catch ex As Exception
                Using cp As New CPClass()
                    cp.core.handleException(ex)
                End Using
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Iterate through all apps, find addosn that need to run and add them to the task queue
        ''' </summary>
        Private Sub scheduleTasks(cpClusterCore As coreClass)
            Dim hint As String = ""
            Try
                '
                logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks")
                '
                For Each kvp As KeyValuePair(Of String, Models.Entity.serverConfigModel.appConfigModel) In cpClusterCore.serverConfig.apps
                    '
                    ' schedule tasks for this app
                    '
                    logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app=[" & kvp.Value.name & "]")
                    '
                    Using cpSite As New CPClass(kvp.Value.name)
                        If (Not (cpSite.core.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.appStatusEnum.OK)) Then
                            '
                            logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app status not ok")
                            '
                        ElseIf (Not (cpSite.core.serverConfig.appConfig.appMode = Models.Entity.serverConfigModel.appModeEnum.normal)) Then
                            '
                            logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app mode not normal")
                            '
                        Else
                            '
                            ' Execute Processes
                            '
                            Try
                                '
                                logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, search for addons to run")
                                '
                                Dim RightNow As Date = Now
                                Dim SQLNow As String = cpSite.core.db.encodeSQLDate(RightNow)
                                Dim sqlAddonsCriteria As String = "" _
                                    & "(Active<>0)" _
                                    & " and(name<>'')" _
                                    & " and(" _
                                    & "  ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))" _
                                    & "  or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))" _
                                    & "  or(ProcessNextRun<" & SQLNow & ")" _
                                    & " )"
                                Dim CS As Integer = cpSite.core.db.csOpen(cnAddons, sqlAddonsCriteria)
                                Do While cpSite.core.db.csOk(CS)
                                    Dim addonProcessInterval As Integer = cpSite.core.db.csGetInteger(CS, "ProcessInterval")
                                    Dim addonName As String = cpSite.core.db.csGetText(CS, "name")
                                    Dim addonProcessRunOnce As Boolean = cpSite.core.db.csGetBoolean(CS, "ProcessRunOnce")
                                    Dim addonProcessNextRun As Date = cpSite.core.db.csGetDate(CS, "ProcessNextRun")
                                    Dim NextRun As Date = Date.MinValue
                                    hint &= ",run addon " & addonName
                                    If addonProcessInterval > 0 Then
                                        NextRun = RightNow.AddMinutes(addonProcessInterval)
                                    End If
                                    If (addonProcessNextRun < RightNow) Or (addonProcessRunOnce) Then
                                        '
                                        logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, add task for addon [" & addonName & "], addonProcessRunOnce [" & addonProcessRunOnce & "], addonProcessNextRun [" & addonProcessNextRun & "]")
                                        '
                                        ' -- resolve triggering state
                                        Call cpSite.core.db.csSet(CS, "ProcessRunOnce", False)
                                        If (addonProcessNextRun < RightNow) Then
                                            Call cpSite.core.db.csSet(CS, "ProcessNextRun", NextRun)
                                        End If
                                        Call cpSite.core.db.csSave2(CS)
                                        '
                                        ' -- add task to queue for runner
                                        Dim cmdDetail As New cmdDetailClass
                                        cmdDetail.addonId = cpSite.core.db.csGetInteger(CS, "ID")
                                        cmdDetail.addonName = addonName
                                        cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpSite.core, cpSite.core.db.csGetText(CS, "argumentlist"))
                                        Call addTaskToQueue(cpSite.core, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
                                    ElseIf cpSite.core.db.csGetDate(CS, "ProcessNextRun") = Date.MinValue Then
                                        '
                                        logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, addon [" & addonName & "], ProcessInterval set but no processNextRun, set processNextRun [" & NextRun & "]")
                                        '
                                        ' -- Interval is OK but NextRun is 0, just set next run
                                        Call cpSite.core.db.csSet(CS, "ProcessNextRun", NextRun)
                                        Call cpSite.core.db.csSave2(CS)
                                    End If
                                    Call cpSite.core.db.csGoNext(CS)
                                Loop
                                Call cpSite.core.db.csClose(CS)
                            Catch ex As Exception
                                '
                                logController.appendLog(cpClusterCore, "taskScheduler.scheduleTasks, execption scheduling addon")
                                '
                                cpClusterCore.handleException(ex)
                            End Try
                        End If
                    End Using
                    hint &= ",app done"
                Next
            Catch ex As Exception
                cpClusterCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Add a command task to the taskQueue to be run by the taskRunner. Returns false if the task was already there (dups fround by command name and cmdDetailJson)
        ''' </summary>
        ''' <param name="cpSiteCore"></param>
        ''' <param name="Command"></param>
        ''' <param name="cmdDetail"></param>
        ''' <param name="BlockDuplicates"></param>
        ''' <returns></returns>
        Public Function addTaskToQueue(cpSiteCore As coreClass, ByVal Command As String, ByVal cmdDetail As cmdDetailClass, ByVal BlockDuplicates As Boolean) As Boolean
            Dim returnTaskAdded As Boolean = True
            Try
                Dim LcaseCommand As String
                Dim sql As String
                Dim cmdDetailJson As String = cpSiteCore.json.Serialize(cmdDetail)
                Dim cs As Integer
                '
                logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, application=[" & cpSiteCore.serverConfig.appConfig.name & "], command=[" & Command & "], cmdDetail=[" & cmdDetailJson & "]")
                '
                returnTaskAdded = True
                LcaseCommand = genericController.vbLCase(Command)
                If BlockDuplicates Then
                    '
                    ' Search for a duplicate
                    '
                    sql = "select top 1 id from cctasks where ((command=" & cpSiteCore.db.encodeSQLText(Command) & ")and(cmdDetail=" & cmdDetailJson & ")and(datestarted is not null))"
                    cs = cpSiteCore.db.csOpenSql(sql)
                    If cpSiteCore.db.csOk(cs) Then
                        returnTaskAdded = False
                    End If
                    Call cpSiteCore.db.csClose(cs)
                End If
                '
                ' Add it to the queue and shell out to the command
                '
                If Not returnTaskAdded Then
                    '
                    logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, command skipped because the unstarted command and details were already in the queue.")
                    '
                Else
                    '
                    logController.appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, application=[" & cpSiteCore.serverConfig.appConfig.name & "], command=[" & Command & "], cmdDetail=[" & cmdDetailJson & "]")
                    '
                    cs = cpSiteCore.db.csInsertRecord("tasks")
                    If cpSiteCore.db.csOk(cs) Then
                        cpSiteCore.db.csSet(cs, "name", "command [" & Command & "], addon [#" & cmdDetail.addonId & "," & cmdDetail.addonName & "]")
                        cpSiteCore.db.csSet(cs, "command", Command)
                        cpSiteCore.db.csSet(cs, "cmdDetail", cmdDetailJson)
                        returnTaskAdded = True
                    End If
                    Call cpSiteCore.db.csClose(cs)
                End If
            Catch ex As Exception
                cpSiteCore.handleException(ex)
                Throw
            End Try
            Return returnTaskAdded
        End Function
        ''
        'Private Sub appendLog(cpCore As coreClass, ByVal logText As String, Optional isImportant As Boolean = False)
        '    If (isImportant Or allowVerboseLogging) Then
        '        logController.appendLog(cpCore, logText, "", "trace")
        '    End If
        '    If (allowConsoleWrite) Then
        '        Console.WriteLine(logText)
        '    End If
        'End Sub
        '
        '
        '
        Public Shared Sub tasks_RequestTask(cpCore As coreClass, ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String, ByVal RequestedByMemberID As Integer)
            Dim CS As Integer
            Dim TaskName As String
            '
            If ExportName = "" Then
                TaskName = CStr(Now()) & " snapshot of unnamed data"
            Else
                TaskName = CStr(Now()) & " snapshot of " & genericController.vbLCase(ExportName)
            End If
            CS = cpCore.db.csInsertRecord("Tasks", RequestedByMemberID)
            If cpCore.db.csOk(CS) Then
                Call cpCore.db.csGetFilename(CS, "Filename", Filename)
                Call cpCore.db.csSet(CS, "Name", TaskName)
                Call cpCore.db.csSet(CS, "Command", Command)
                Call cpCore.db.csSet(CS, "SQLQuery", SQL)
            End If
            Call cpCore.db.csClose(CS)
        End Sub
        Public Shared Sub main_RequestTask(cpCore As coreClass, ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String)
            Call tasks_RequestTask(cpCore, genericController.encodeText(Command), genericController.encodeText(SQL), genericController.encodeText(ExportName), genericController.encodeText(Filename), genericController.EncodeInteger(cpCore.doc.authContext.user.id))
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
