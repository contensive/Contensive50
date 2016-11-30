
Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports Contensive.Core

Namespace Contensive
#Const includeTracing = False
    Public Class taskSchedulerServiceClass
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
        Public Sub stopService()
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
        Public Function StartService(ByVal setVerbose As Boolean, ByVal singleThreaded As Boolean) As Boolean
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
        Private Sub processTimerTick()
            Try
                'Using cp As New CPClass()
                Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick")
                    'appendLog(cp.core, "taskScheduleServiceClass.processTimerTick")
                    '
                    If (ProcessTimerInProcess) Then
                        Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick, skipped because timerInProcess")
                        'appendLog(cp.core, "taskScheduleServiceClass.processTimerTick, skipped because timerInProcess")
                    Else
                        ProcessTimerInProcess = True
                        '
                        ' schedule tasks
                        '
                        'Using programDataFiles As New fileSystemClass(cp.core, cp.core.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib")
                        Dim JSONTemp As String = "{""clusterPath"":""c:\\inetPub"",""allowTaskRunnerService"":true,""allowTaskSchedulerService"":true}"
                        'Dim JSONTemp = programDataFiles.ReadFile("serverConfig.json")
                        Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
                        Dim serverConfig As serverConfigClass = json_serializer.Deserialize(Of serverConfigClass)(JSONTemp)
                        If (Not serverConfig.allowTaskSchedulerService) Then
                            Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick, skipped because serviceConfig.allowTaskSchedulerService false.")
                            'appendLog(cp.core, "taskScheduleServiceClass.processTimerTick, skipped because serviceConfig.allowTaskSchedulerService false.")
                        Else
                            Console.WriteLine("tmp-taskScheduleServiceClass.processTimerTick, call to scheduleTasks commented for debug.")
                            'appendLog(cp.core, "taskScheduleServiceClass.processTimerTick, call to scheduleTasks commented for debug.")
                            'Call scheduleTasks(cp.core)
                        End If
                        'End Using
                        ProcessTimerInProcess = False
                    End If
                ' End Using
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
        Private Sub scheduleTasks(cpClusterCore As cpCoreClass)
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
                '
                appendLog(cpClusterCore, "taskScheduler.scheduleTasks")
                '
                RightNow = Now
                SQLNow = EncodeSQLDate(RightNow)
                For Each kvp As KeyValuePair(Of String, appConfigClass) In cpClusterCore.cluster.config.apps
                    AppName = kvp.Value.name
                    '
                    ' schedule tasks for this app
                    '
                    appendLog(cpClusterCore, "taskScheduler.scheduleTasks, app=[" & AppName & "]")
                    '
                    Using cpSite As New CPClass(AppName)
                        If cpSite.core.app.status = applicationStatusEnum.ApplicationStatusReady Then
                            '
                            ' Execute Processes
                            '
                            Try
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
        ''' Add a command task to the taskQueue to be run by the taskRunner
        ''' </summary>
        ''' <param name="cpSiteCore"></param>
        ''' <param name="Command"></param>
        ''' <param name="cmdDetail"></param>
        ''' <param name="BlockDuplicates"></param>
        ''' <returns></returns>
        Private Function addTaskToQueue(cpSiteCore As cpCoreClass, ByVal Command As String, ByVal cmdDetail As cmdDetailClass, ByVal BlockDuplicates As Boolean) As Boolean
            Dim returnTaskAdded As Boolean = True
            Try
                Dim LcaseCommand As String
                Dim sql As String
                Dim cmdDetailJson As String = cpSiteCore.jsonSerialize(cmdDetail)
                Dim cs As Integer
                '
                appendLog(cpSiteCore, "taskScheduler.addTaskToQueue, application=[" & cpSiteCore.app.config.name & "], command=[" & Command & "], cmdDetail=[" & cmdDetailJson & "]")
                '
                returnTaskAdded = True
                LcaseCommand = LCase(Command)
                If BlockDuplicates Then
                    '
                    ' Search for a duplicate
                    '
                    sql = "select top 1 id from cctasks where ((command=" & EncodeSQLText(Command) & ")and(cmdDetail=" & cmdDetailJson & "))"
                    cs = cpSiteCore.app.db_csOpenSql(sql)
                    If cpSiteCore.app.db_csOk(cs) Then
                        returnTaskAdded = False
                    End If
                    Call cpSiteCore.app.db_csClose(cs)
                End If
                '
                ' Add it to the queue and shell out to the command
                '
                If returnTaskAdded Then
                    cs = cpSiteCore.app.db_csInsertRecord("tasks")
                    If cpSiteCore.app.db_csOk(cs) Then
                        cpSiteCore.app.db_SetCSField(cs, "name", "")
                        cpSiteCore.app.db_SetCSField(cs, "command", Command)
                        cpSiteCore.app.db_SetCSField(cs, "cmdDetail", cmdDetailJson)
                        returnTaskAdded = True
                    End If
                    Call cpSiteCore.app.db_csClose(cs)
                End If
            Catch ex As Exception
                cpSiteCore.handleException(ex)
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
        Private Sub appendLog(cpCore As cpCoreClass, ByVal logText As String, Optional isImportant As Boolean = False)
            If (isImportant Or allowVerboseLogging) Then
                cpCore.appendLog(logText, "", "trace")
            End If
            If (allowConsoleWrite) Then
                Console.WriteLine(logText)
            End If
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
