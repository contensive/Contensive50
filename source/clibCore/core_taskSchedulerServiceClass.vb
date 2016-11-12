
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
        Private SiteProcessAlarmTime As Date            ' Run Site Processes every 30 seconds
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
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
            verboseLogging = False
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
        Public Sub stopService()
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
        Public Function StartService(ByVal setVerbose As Boolean, ByVal singleThreaded As Boolean) As Boolean
            Dim returnStartedOk As Boolean = False
            Try
                '
                Dim StatusMessage As String
                '
                returnStartedOk = False
                '
                If StartServiceInProgress Then
                    StatusMessage = "Attempting a service start retry, but it is still starting"
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
                Dim rightNow As Date = Now
                '
                If (Not ProcessTimerInProcess) Then
                    '
                    ' If not debug and not inprocess, run processes
                    '
                    ProcessTimerInProcess = True
                    If SiteProcessAlarmTime = Date.MinValue Then
                        '
                        ' start process timer 1 minute after service starts
                        '
                        SiteProcessAlarmTime = rightNow.AddMinutes(1)
                    ElseIf rightNow > SiteProcessAlarmTime Then
                        '
                        ' schedule tasks
                        '
                        Dim programDataFiles As New fileSystemClass(cpCore, cpCore.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib")
                        Dim JSONTemp = programDataFiles.ReadFile("serverConfig.json")
                        Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
                        Dim serverConfig As serverConfigClass = json_serializer.Deserialize(Of serverConfigClass)(JSONTemp)
                        If (serverConfig.allowTaskSchedulerService) Then
                            Call scheduleTasks()
                            SiteProcessAlarmTime = SiteProcessAlarmTime.AddMinutes(1)
                            If rightNow > SiteProcessAlarmTime Then
                                SiteProcessAlarmTime = rightNow.AddMinutes(1)
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
