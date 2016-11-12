
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
        Private cpCore As cpCoreClass
        '
        Private runnerGuid As String                    ' set in constructor. used to tag tasks assigned to this runner
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
        Public Sub New(cpCore As cpCoreClass)
            Me.cpCore = cpCore
            verboseLogging = False
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
        Public Function StartService() As Boolean
            Dim returnStartedOk As Boolean = True
            Try
                processTimer = New System.Timers.Timer(ProcessTimerMsecPerTick)
                AddHandler processTimer.Elapsed, AddressOf processTimerTick
                processTimer.Interval = ProcessTimerMsecPerTick
                processTimer.Enabled = True
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
                If (Not ProcessTimerInProcess) Then
                    ProcessTimerInProcess = True
                    '
                    ' run tasks in task
                    '
                    Dim programDataFiles As New fileSystemClass(cpCore, cpCore.cluster.config, fileSystemClass.fileSyncModeEnum.noSync, Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib")
                    Dim JSONTemp = programDataFiles.ReadFile("serverConfig.json")
                    Dim json_serializer As New System.Web.Script.Serialization.JavaScriptSerializer()
                    Dim serverConfig As serverConfigClass = json_serializer.Deserialize(Of serverConfigClass)(JSONTemp)
                    If (serverConfig.allowTaskRunnerService) Then
                        Call runTasks()
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
        Private Sub runTasks()
            'Dim hint As String = ""
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
                Dim cpSite As CPClass
                '
                Console.WriteLine("taskRunner.scheduleTasks")
                '
                For Each kvp As KeyValuePair(Of String, appConfigClass) In cpCore.cluster.config.apps
                    AppName = kvp.Value.name
                    '
                    ' query tasks that need to be run
                    '
                    cpSite = New CPClass(AppName)
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
                                    Select Case command.ToLower()
                                        Case taskQueueCommandEnumModule.runAddon
                                            Call cpSite.core.executeAddon(cmdDetail.addonId, cmdDetail.docProperties, cpCoreClass.addonContextEnum.ContextSimple)
                                    End Select
                                End If
                                cpSite.core.app.db_csClose(CS)
                            Loop While recordsRemaining
                        Catch ex As Exception
                            cpCore.handleException(ex)
                        End Try
                    End If
                    'hint &= ",app done"
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
