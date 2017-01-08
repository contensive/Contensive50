
Imports System
Imports System.IO
Imports System.Diagnostics
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Web
Imports Microsoft.VisualBasic
'Imports Contensive.Core
Imports Contensive.Core

Namespace Contensive
#Const includeTracing = False
    Public Class legacyWorkerClass
        Implements IDisposable
        '
        '==================================================================================================
        ' worker process to execute addons from the message queue
        '   in production this runs under a windows service
        '   in dev, it runs under the debug service app, a stand=alone that can be debugged
        '==================================================================================================
        '
        Private cpCore As coreClass
        '
        ' ----- Log File
        '
        Private Const LogMsg = "For more information, see the Contensive Trace Log."
        Private verboseLogging As Boolean = False
        '
        ' ----- Local Persistant Storage
        '
        Public EmailCheckCount As Integer
        '
        ' ----- Start Timer - returns to SCM quickly and handles startup on a thread
        '
        Private startTimer As System.Timers.Timer
        '
        ' ----- Task Timer
        '
        Private processTimer As System.Timers.Timer
        Private ProcessTimerTickCnt As Integer
        Private ServiceStartTime As Date
        Const ProcessTimerMsecPerTick = 5000            ' Check processs every 5 seconds
        Private ProcessTimerInProcess As Boolean        '
        Private ProcessTimerProcessCount As Integer        '
        Private AllSitesNotReadyCnt As Integer             ' Process Timer ticks since all sites were last ready (or last error)
        Const AllSitesNotReadyCntOneHour = 720            ' log an error after AllSitesNotReadyCnt gets to this (1-hour)
        '
        ' ----- Alarms within Process Timer
        '
        Private SiteProcessAlarmTime As Date            ' Run Site Processes every 60 seconds
        Const SiteProcessIntervalSeconds = 60           '
        Private TaskAlarmTime As Date                   ' Run Internal Tasks every 62 seconds
        Const TaskIntervalSeconds = 62                  '
        Private RemoteNameAlarmTime As Date             ' Run Remote Names every 64 seconds
        Const RemoteNameIntervalSeconds = 64            '
        Private EmailAlarmTime As Date                  ' Run Email Process every 66 seconds
        Const EmailIntervalSeconds = 66                 '
        Private AutoStartAlarmTime As Date              ' Run AutoStartSites every 10 seconds
        Const AutoStartIntervalSeconds = 10             '
        Private HouseKeepAlarmTime As Date              ' Run HouseKeep every 68 seconds, then random one in 10
        Const HouseKeepIntervalSeconds = 10                    '
        '
        ' ----- Status buffer
        '
        Private StatusMessageLast As String
        '
        ' ----- Display update
        '
        Private PrintStatusInProcess As Boolean
        Private StartTime As Date
        '
        ' ----- Debugging
        '
        Public StartServiceInProgress As Boolean
        '
        Private serverListenerPort As Integer
        Private LocalIPList As String
        Private maxCmdInstances As Integer

        Private AdminUsername As String
        Private AdminPassword As String
        '
        ' Command Queue - list of commands that will be executed serially
        '   They are added by manager interface
        '   They are executed by the ccStart application, so they all run in other processes
        '   Current command is in position 0, when cnt>0
        '
        Private Const asyncCmdQueueLimit = 100
        Private asyncCmdQueueCnt As Integer
        Private asyncCmdQueueSize As Integer
        Private asyncCmdQueue() As String
        '
        Protected disposed As Boolean = False
        '
        ' ----- tcpListener version
        '
        Private TcpListener As TcpListener
        Private tcpListenterListenThread As Thread
        Private tcpListenerListening As Boolean = False
        '
        '========================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            '
            ' Initialize
            '
            Me.cpCore = cpCore
            '
            verboseLogging = False
            '
            StartTime = DateTime.Now()
            LocalIPList = vbCrLf & getIpAddressList.Replace(",", vbCrLf) & vbCrLf & "127.0.0.1" & vbCrLf
            '
            processTimer = New System.Timers.Timer(5000)
            AddHandler processTimer.Elapsed, AddressOf processTimerTick
            '
            startTimer = New System.Timers.Timer(5000)
            AddHandler startTimer.Elapsed, AddressOf startTimerTick
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
        ' ----------------------- tcpListener version
        ' http://tech.pro/tutorial/704/csharp-tutorial-simple-threaded-tcp-server
        '
        Private Sub tcpListenerStart(ByVal serverListenerPort As Integer)
            '
            ' ----- tcplistener
            '
            'appendTcpLog("tcpListenerStart, serverListenerPort=[" & serverListenerPort & "]")
            '
            TcpListener = New TcpListener(IPAddress.Any, serverListenerPort)
            tcpListenterListenThread = New Thread(New ThreadStart(AddressOf tcpListenerListenThread_main))
            tcpListenterListenThread.Start()
        End Sub
        '
        ' 
        '
        Private Sub tcpListenerListenThread_main()
            Dim client As TcpClient
            Dim tcpListenerConnectionThread As Thread
            '
            'appendTcpLog("tcpListenerListenThread_main, enter")
            '
            TcpListener.Start()
            tcpListenerListening = True
            While (tcpListenerListening)
                '
                '//blocks until a client has connected to the server
                '
                'appendTcpLog("tcpListenerListenThread_main, wait for connect")
                '
                Try
                    client = TcpListener.AcceptTcpClient()
                    '
                    'appendTcpLog("tcpListenerListenThread_main, connected")
                    '
                    '//create a thread to handle communication with connected client
                    '
                    tcpListenerConnectionThread = New Thread(New ParameterizedThreadStart(AddressOf tcpListenerConnectionThread_HandleConnection))
                    tcpListenerConnectionThread.Start(client)
                Catch ex As Exception
                    '
                    ' 
                    '
                    'appendTcpLog("tcpListenerListenThread_main, TcpListener.AcceptTcpClient exited with error, ex=[" & ex.ToString() & "]")
                End Try
            End While
        End Sub
        '
        '
        '
        Private Sub tcpListenerConnectionThread_HandleConnection(ByVal client As Object)
            Dim tcpClient As TcpClient
            Dim clientStream As NetworkStream
            Dim requestBuffer() As Byte
            Dim bytesRead As Integer
            Dim encoder As New ASCIIEncoding
            Dim responseBuffer() As Byte
            Dim cmd As String
            Dim result As String
            Dim args As String = ""
            Dim remoteIp As String
            Dim pos As Integer
            Dim queryString As String
            '
            'appendTcpLog("tcpListenerConnectionThread_HandleConnection, enter")
            '
            tcpClient = DirectCast(client, TcpClient)
            clientStream = tcpClient.GetStream()
            remoteIp = tcpClient.Client.RemoteEndPoint.ToString
            pos = remoteIp.IndexOf(":")
            If pos > 0 Then
                remoteIp = remoteIp.Substring(0, pos)
            End If
            encoder = New ASCIIEncoding()
            ReDim requestBuffer(4096)
            '
            'While (True)
            bytesRead = 0
            Try
                '//blocks until a client sends a message
                bytesRead = clientStream.Read(requestBuffer, 0, 4096)
            Catch ex As Exception
                '//a socket error has occured
                '
                'appendTcpLog("tcpListenerConnectionThread_HandleConnection, ERROR, a socket error occured during clientStream.read, ex=[" & ex.ToString() & "]")
                '
                'Exit While
            End Try

            If (bytesRead = 0) Then
                '//the client has disconnected from the server
                'Exit While
            End If
            '//message has successfully been received
            cmd = encoder.GetString(requestBuffer, 0, bytesRead)
            System.Diagnostics.Debug.WriteLine(cmd)
            '
            'cpCore.AppendLog( "tcpListenerConnectionThread_HandleConnection, full cmd=[" & cmd & "]")
            '
            If cmd.Substring(0, 5).ToLower = "get /" Then
                cmd = cmd.Substring(5)
            End If
            '
            queryString = ""
            pos = cmd.IndexOf("?")
            If pos > 0 Then
                queryString = cmd.Substring(pos + 1)
                cmd = cmd.Substring(0, pos)
            End If
            result = tcpListenerConnectionThread_executeCmd(cpCore, cmd, queryString, remoteIp)
            '
            'cpCore.AppendLog( "tcpListenerConnectionThread_HandleConnection, result=[" & result & "]")
            '
            responseBuffer = encoder.GetBytes(result)
            Try
                clientStream.Write(responseBuffer, 0, responseBuffer.Length)
                clientStream.Flush()
            Catch ex As Exception
                'cpCore.AppendLog( "Error in tcpListenerConnectionThread_HandleConnection during write/flush, ex=[" & ex.ToString & "]")
            Finally
                tcpClient.Client.Blocking = False
                tcpClient.Client.Close()
                '
                tcpClient.Close()

            End Try
            'End While
            '
        End Sub
        ''
        '' ----------------------- ipListener version
        ''
        'Private ipListener_cmdListenPort As Integer
        'Private ipListener_ServerLicense As String
        'Private ipListener_LocalIPList As String
        ''
        '' http listener thread and communication object
        ''
        'Private threadHandle_ipListenerMain As Thread
        'Private Const ipListener_runOnParentThread As Boolean = True
        ''
        ''==========================================================================================
        ''   Start listening
        ''==========================================================================================
        ''
        'Public Sub ipListener_start(listenPort As Integer)
        '    Try
        '        '
        '        '
        '        '
        '        ipListener_cmdListenPort = listenPort
        '        'ipListener_cmdCallbackObject = callbackObject
        '        If Not ipListener_runOnParentThread Then
        '            '
        '            ' start on this thread and block
        '            '
        '            Call ipListener_mainThread()
        '        Else
        '            '
        '            ' start on a new thread and return
        '            '
        '            threadHandle_ipListenerMain = New Thread(AddressOf ipListener_mainThread)
        '            threadHandle_ipListenerMain.Name = "cmdListen"
        '            threadHandle_ipListenerMain.IsBackground = True
        '            threadHandle_ipListenerMain.Start()
        '        End If
        '    Catch ex As Exception
        '        '
        '        '
        '        '
        '        Throw New ApplicationException("Error during ipListener_start")
        '    End Try
        'End Sub
        ''
        ''==========================================================================================
        ''   Stop listening
        ''==========================================================================================
        ''
        'Public Sub ipListener_stop()
        '    Try
        '        '
        '        ' abort sockets
        '        '
        '        If Not ipListener_runOnParentThread Then
        '            '
        '            '
        '            '
        '        ElseIf Not (threadHandle_ipListenerMain Is Nothing) Then
        '            '
        '            '
        '            '
        '            threadHandle_ipListenerMain.Abort()
        '        End If
        '    Catch ex As Exception
        '        '
        '        '
        '        '
        '        Throw New ApplicationException("Error during ipListener_stop")
        '    End Try
        'End Sub
        ''
        ''===============================================================================================
        ''   thread method for http Listener
        ''       decided on http because Contensive 4 used http, and it is easy to write clients
        ''       listens for commands on cmdPort
        ''
        '' short term fix -- calling object provides a call back routine ipDaemonCallBack(cmd, queryString, remoteIP)
        ''   must be one thread because it calls back into vb6
        ''
        ''   eventual solution -- the listener goes in the server, and creates an object to call into.
        ''
        ''===============================================================================================
        ''
        'Private Sub ipListener_mainThread()
        '    '
        '    Dim cmd As String
        '    Dim queryString As String
        '    Dim remoteIP As String
        '    Dim prefixes() As String
        '    Dim prefixesCnt As Integer = 0
        '    Dim ipHostInfo As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName())
        '    Dim ipAddressInfo As IPAddress = Nothing
        '    Dim HostEntry As IPHostEntry = Dns.GetHostEntry(System.Net.Dns.GetHostName)
        '    Dim ptr As Integer
        '    Dim context As HttpListenerContext
        '    Dim request As HttpListenerRequest
        '    Dim response As HttpListenerResponse
        '    Dim responseString As String
        '    Dim buffer() As Byte
        '    Dim output As System.IO.Stream
        '    Dim Hint As String = "enter"
        '    Dim hintPrefixes As String = ""
        '    '
        '    Try
        '        '
        '        If Not HttpListener.IsSupported Then
        '            Throw New ArgumentException("This operating system does not support the required http listen methods")
        '        Else
        '            Dim cmdListener As HttpListener
        '            '
        '            ReDim prefixes(prefixesCnt)
        '            prefixes(prefixesCnt) = "http://127.0.0.1:" & ipListener_cmdListenPort & "/"
        '            prefixesCnt += 1
        '            '
        '            Hint &= ",building prefixes"
        '            For Each ipAddressInfo In HostEntry.AddressList
        '                If ipAddressInfo.AddressFamily = AddressFamily.InterNetwork Then
        '                    If ipAddressInfo.ToString <> "127.0.0.1" Then
        '                        ReDim Preserve prefixes(prefixesCnt)
        '                        prefixes(prefixesCnt) = "http://" & ipAddressInfo.ToString & ":" & ipListener_cmdListenPort & "/"
        '                        prefixesCnt += 1
        '                    End If
        '                End If
        '            Next
        '            If prefixesCnt = 0 Then
        '                Throw New ArgumentException("No ip addresses are available")
        '            Else
        '                '
        '                ' Create a listener.
        '                '
        '                Hint &= ",create listener"
        '                cmdListener = New HttpListener()
        '                For ptr = 0 To prefixesCnt - 1
        '                    cmdListener.Prefixes.Add(prefixes(ptr))
        '                    hintPrefixes &= "," & prefixes(ptr)
        '                Next
        '                Hint &= ",start"
        '                cmdListener.Start()
        '                Do
        '                    'hint = "do,get request response obj"
        '                    context = cmdListener.GetContext()
        '                    request = context.Request
        '                    response = context.Response
        '                    cmd = request.Url.LocalPath
        '                    queryString = request.Url.Query
        '                    Hint &= ",cmd=[" & cmd & "],querystring=[" & queryString & "]"
        '                    If queryString.Length > 0 Then
        '                        If queryString.Substring(0, 1) = "?" Then
        '                            queryString = queryString.Substring(1)
        '                        End If
        '                    End If
        '                    remoteIP = request.RemoteEndPoint.Address.ToString
        '                    Hint &= ",remoteIP=[" & remoteIP & "]"
        '                    Try
        '                        Hint &= ",callback enter"
        '                        responseString = ipDaemonCallback(cmd, queryString, remoteIP)
        '                        Hint &= ",callback exit"
        '                    Catch ex As Exception
        '                        '
        '                        ' should never return an error to the iDaemon
        '                        '
        '                        Call My.Computer.FileSystem.WriteAllText("C:\clibCpDebug.log", vbCrLf & Now.ToString() & " " & "Exception in callback, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
        '                        responseString = ""
        '                    End Try
        '                    Hint &= ",set buffer from responseString"
        '                    If responseString.Length <= 0 Then
        '                        buffer = System.Text.Encoding.Unicode.GetBytes("")
        '                    Else
        '                        buffer = System.Text.Encoding.UTF8.GetBytes(responseString)
        '                    End If
        '                    Hint &= ",write output from butter"
        '                    response.ContentLength64 = buffer.Length
        '                    response.ContentType = "text/HTML"
        '                    output = response.OutputStream
        '                    output.Write(buffer, 0, buffer.Length)
        '                    output.Close()
        '                Loop
        '                cmdListener.Stop()
        '                cmdListener.Abort()
        '                'cmdListener = Nothing
        '            End If
        '        End If
        '    Catch ex As HttpListenerException
        '        '
        '        '
        '        '
        '        Call My.Computer.FileSystem.WriteAllText("C:\clibCpDebug.log", vbCrLf & Now.ToString() & " " & "HttpListenerException, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
        '        'Throw
        '    Catch ex As Exception
        '        '
        '        '
        '        '
        '        Call My.Computer.FileSystem.WriteAllText("C:\clibCpDebug.log", vbCrLf & Now.ToString() & " " & "Exception, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
        '        'Throw
        '    End Try
        'End Sub
        '
        '==========================================================================================
        '
        '==========================================================================================
        '
        '        Private Sub AutoStartSites()
        '            Dim hint As String = "enter"
        '            Try
        '                'Dim AppService As appServicesClass
        '                Dim AppName As String
        '                Dim appStatus As Integer
        '                Dim AppAutoStart As Boolean
        '                '
        '                ' Check if a config file save is needed
        '                '
        '#If includeTracing Then
        '                'Call appendTraceLog("autoStartSites", "enter")
        '#End If
        '                If Not (KernelServices Is Nothing) Then
        '                    '
        '                    ' Save config files if changed
        '                    '
        '                    'hint = "check config.modified"
        '                    If KernelServices.ConfigModified Then
        '                        'hint = "save config"
        '                        Call SaveConfig()
        '                    End If
        '                    '
        '                    ' Shell out to the start process
        '                    '
        '                    'hint = "iterate appservices"
        '                    For Each AppService In KernelServices.AppServices
        '                        'DoEvents()
        '                        'hint = "get appname"
        '                        AppName = cpCore.app.Name
        '                        'hint = "get status and autostart for " & AppName
        '                        appStatus = cpCore.app.status
        '                        AppAutoStart = cpCore.app.AutoStart
        '                        If (appStatus <> ApplicationStatusRunning) And (AppAutoStart) Then
        '                            'hint = "app [" & AppName & "] not running, add start to command queue"
        '                            Call addAsyncCmd(cp,"START", True)
        '                            Exit For
        '                        End If
        '                        'hint = "finished app [" & AppName & "]"
        '                    Next
        '                End If
        '#If includeTracing Then
        '                'Call appendTraceLog("autoStartSites", "exit")
        '#End If
        '            Catch ex As Exception
        '                Call handleExceptionResume(ex, "AutoStartSites, hint=[" & hint & "]", "ErrorTrap")
        '            End Try
        '        End Sub
        '
        '==========================================================================================
        '   Stop all activity through the content server, but do not unload
        '==========================================================================================
        '
        Public Sub stopServer()
            Try
                Dim StatusMessage As String
                'Dim Controller As New controlClass
                'Dim AppService As ccKrnl42.AppServicesClass
                '
                processTimer.Enabled = False
                '
                ' Turn off the Listener
                '
                tcpListenerListening = False
                TcpListener.Stop()
                'ipListener_stop()
                '
                ' Save the current configuration
                '
                'Call SaveConfig()
                '
                ' Destroy the objects we are hosting
                '
                'If Not (KernelServices Is Nothing) Then
                '    'KernelServices.HostServiceProcessID = 0
                '    KernelServices = Nothing
                'End If
                '
                ' Log status
                '
                StatusMessage = "Contensive Service Stopped"
                'cpCore.AppendLog("StopService", StatusMessage)
            Catch ex As Exception
                Call handleExceptionResume(ex, "StopService", "ErrorTrap")
            End Try
        End Sub
        '
        '==========================================================================================
        '   Process the Start signal from the Server Control Manager
        '==========================================================================================
        '
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
                    cpCore.log_appendLog(MethodName & ", " & StatusMessage)
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
                        cpCore.log_appendLog(MethodName & ", " & StatusMessage)
                    Else
                        If False Then
                            'If KernelServices.HostServiceProcessID <> 0 Then
                            '
                            ' ----- Already started by another HostService
                            '
                            StatusMessage = "Contensive Kernel Services is already running."
                            cpCore.log_appendLog(MethodName & ", " & StatusMessage)
                        Else
                            '
                            ' No license is trial license
                            '
                            If True Then
                                '
                                ' ----- Set shared Mail out folder
                                '
                                EmailFolder = "\EmailOut"
                                If Not cpCore.cluster.localClusterFiles.checkPath(EmailFolder) Then
                                    Call cpCore.cluster.localClusterFiles.createPath(EmailFolder)
                                End If
                                '
                                ' ----- Set shared log folder
                                '
                                LogFolder = "\Logs"
                                If Not cpCore.cluster.localClusterFiles.checkPath(LogFolder) Then
                                    Call cpCore.cluster.localClusterFiles.createPath(LogFolder)
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
                                    AdminUsername = cpCore.clusterConfig.username
                                    AdminPassword = cpCore.clusterConfig.password
                                    serverListenerPort = cpCore.clusterConfig.serverListenerPort
                                    maxCmdInstances = cpCore.clusterConfig.maxConcurrentTasksPerServer
                                    'KernelServices.ServerLicense = ServerConfig.ServerLicense
                                    '
                                    ' ----- Turn on the tcpListener
                                    '
                                    'Errorhint = "Starting tcpListener"
                                    '
                                    Call tcpListenerStart(serverListenerPort)
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
                        startTimer.Enabled = False
                        StatusMessage = "Contensive v" & cpCore.common_version() & " started"
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
                Call handleExceptionResume(ex, MethodName, "Service start was interrupted by an unexpected ErrorTrap ('hint = " & ErrorHint & ")")
            End Try
            Return returnStartedOk
        End Function
        '
        '======================================================================================
        '
        '======================================================================================
        '
        Private Sub startTimerTick()
            Try
                startTimer.Enabled = False
                startTimer.Interval = 60000
                Call StartServer(False, False)
            Catch ex As Exception
                Call handleExceptionResume(ex, "NTService_Start", "Unexpected error during StartTimer event")
            End Try
        End Sub
        ' ''
        ''Public Function ipDaemonCallback(Cmd As String, Args As String, RemoteHost As String) As String
        ''    ipDaemonCallback = executeServerCmd(Cmd, Args, RemoteHost)
        ''End Function
        ''
        ''
        ''
        'Private Function xGetPrimaryDomainName(ByVal DomainNameList As String) As String
        '    Dim returnString As String = ""
        '    Try
        '        Dim CopySplit() As String
        '        '
        '        returnString = DomainNameList
        '        If vbInstr(1, returnString, ",", 1) <> 0 Then
        '            CopySplit = Split(returnString, ",")
        '            returnString = CopySplit(0)
        '        End If
        '    Catch ex As Exception
        '        Call handleExceptionResume(ex, "GetPrimaryDomainName", "Unexpected error")
        '    End Try
        '    Return returnString
        'End Function
        '
        ' -- executeCmd
        '       this is how clients run asyncCmds (addons) - by calling this with a runProcess command
        '       this is how the ccCmd processes request the next command in the queue (maybe that should be a seperate control listener)
        '
        Private Function tcpListenerConnectionThread_executeCmd(cpCore As coreClass, ByVal Method As String, ByVal queryString As String, ByVal RemoteIP As String) As String
            Dim returnString As String = ""
            Try
                '
                Dim File As String
                Dim FolderPath As String
                Dim QSPairs() As String
                Dim Cmd As String
                Dim QSName As String
                Dim QSValue As String
                Dim Pos As Integer
                'Dim fs As fileSystemClass
                Dim SrcFile As String
                Dim DstFile As String
                'Dim FindText As String
                'Dim ReplaceText As String
                'Dim CDefNameList As String
                'Dim CDefNames() As String
                Dim Ptr As Integer
                'Dim Controller As New controlClass
                'Dim AppName As String
                'Dim Name As String
                'Dim Value As String
                'Dim AppServices As appServicesClass
                Dim RemoteUsername As String
                Dim RemotePassword As String
                Dim Authenticated As Boolean
                'Dim SetUsername As String
                'Dim SetPassword As String
                Dim IsLocalRemote As Boolean
                'Dim cp As CPClass = New CPClass(appName)
                '
                cpCore.log_appendLog("serverClass.executeServerCmd, method=[" & Method & "], queryString=[" & queryString & "]")
                '
                If Mid(Method, 1, 1) = "/" Then
                    Method = Mid(Method, 2)
                End If
                '
                Authenticated = False
                IsLocalRemote = (InStr(1, LocalIPList, vbCrLf & RemoteIP & vbCrLf, vbBinaryCompare) <> 0)
                RemoteUsername = getCommandArgument("un", queryString)
                RemotePassword = getCommandArgument("pw", queryString)
                If (Not IsLocalRemote) And ((RemoteUsername = "") Or (RemoteUsername <> AdminUsername) Or (RemotePassword <> AdminPassword)) Then
                    '
                    ' Bad username and password
                    '
                    cpCore.log_appendLog("serverClass.executeServerCmd", "bad username/password.")
                    returnString = "ERROR " & ignoreString
                Else
                    '
                    ' authenticated
                    '
                    cpCore.log_appendLog("serverClass.executeServerCmd, switch on method=[" & Method & "]")
                    Select Case vbUCase(Method)
                        Case "CONNECT"
                            '
                            ' Connect
                            '
                            returnString = "ok" ' & vbCrLf & KernelServices.instanceID
                        Case "GETNEXTASYNCCMD"
                            '
                            ' GetNextAsyncCmd -- returns the next command to be executed
                            '
                            returnString = "ok" & vbCrLf & getNextAsyncCmdFromQueue()
                        Case "GETSERVERCONFIG"
                            '
                            ' GetConnectInfo (must return it if connecion is local)
                            '
                            returnString = "ok" _
                                & vbCrLf & serverListenerPort _
                                & vbCrLf & "" _
                                & vbCrLf & AdminUsername _
                                & vbCrLf & AdminPassword _
                                & vbCrLf & cpCore.common_version() _
                                & vbCrLf & maxCmdInstances _
                                & ""
                        Case "SETSERVERCONFIG"
                            '
                            ' SetConnectInfo
                            '
                            If vbInstr(1, queryString, "serverListenerPort=", vbTextCompare) <> 0 Then
                                serverListenerPort = getCommandArgument("serverListenerPort", queryString)
                            End If
                            '
                            If vbInstr(1, queryString, "AdminUsername=", vbTextCompare) <> 0 Then
                                AdminUsername = getCommandArgument("AdminUsername", queryString)
                            End If
                            '
                            If vbInstr(1, queryString, "AdminPassword=", vbTextCompare) <> 0 Then
                                AdminPassword = getCommandArgument("AdminPassword", queryString)
                            End If
                            '
                            If vbInstr(1, queryString, "maxCmdInstances=", vbTextCompare) <> 0 Then
                                maxCmdInstances = EncodeInteger(getCommandArgument("maxCmdInstances", queryString))
                                If maxCmdInstances <= 0 Then
                                    maxCmdInstances = 1
                                End If
                            End If
                            '
                            returnString = "ok"
                            '
                            ' Status and configuration functions - controller calls
                            '
                            'Case "APPLIST"
                            '    returnString = "ok" & vbCrLf & Controller.GetApplicationList()
                            'Case "APPSTATUS"
                            '    AppName = getCommandArgument("appname", queryString)
                            '    returnString = "ok" & vbCrLf & Controller.GetApplicationStatus(AppName)
                            'Case "ADDAPP"
                            '    AppName = getCommandArgument("appname", queryString)
                            '    If AppName = "" Then
                            '        returnString = "ERROR " & ccError_InvalidAppName
                            '    Else
                            '        If Not Controller.AddApplication(AppName) Then
                            '            returnString = "ERROR " & ccError_ErrorAddingApp
                            '        Else
                            '            returnString = "ok"
                            '        End If
                            '    End If
                            'Case "DELAPP"
                            '    '
                            '    AppName = getCommandArgument("appname", queryString)
                            '    If AppName = "" Then
                            '        returnString = "ERROR " & ccError_InvalidAppName
                            '    Else
                            '        If Not Controller.DeleteApplication(AppName) Then
                            '            returnString = "ERROR " & ccError_ErrorDeletingApp
                            '        Else
                            '            returnString = "ok"
                            '        End If
                            '    End If
                            'Case "MODIFYSERVER"
                            '    '
                            '    SetUsername = getCommandArgument("username", queryString)
                            '    SetPassword = getCommandArgument("password", queryString)
                            '    If (SetUsername = "") Or (SetPassword = "") Then
                            '        returnString = "ERROR " & ccError_InvalidFieldName
                            '    Else
                            '        AdminUsername = SetUsername
                            '        AdminPassword = SetPassword
                            '        Call SaveConfig()
                            '        'KernelServices.ConfigModified = True
                            '        returnString = "ok"
                            '    End If
                            'Case "MODIFYAPP"
                            '    AppName = getCommandArgument("appname", queryString)
                            '    If AppName = "" Then
                            '        returnString = "ERROR " & ccError_InvalidAppName
                            '    Else
                            '        Name = getCommandArgument("name", queryString)
                            '        If Name = "" Then
                            '            returnString = "ERROR " & ccError_InvalidFieldName
                            '        Else
                            '            Value = getCommandArgument("value", queryString)
                            '            AppServices = KernelServices.AppServices.Item(AppName)
                            '            If (AppServices Is Nothing) Then
                            '                returnString = "ERROR " & ccError_InvalidAppName
                            '            Else
                            '                Select Case vbUCase(Name)
                            '                    Case "NAME"
                            '                        appServices.config.name = Value
                            '                        returnString = "ok"
                            '                    Case "AUTOSTART"
                            '                        AppServices.AutoStart = EncodeBoolean(Value)
                            '                        returnString = "ok"
                            '                    Case "CONNECTIONSTRING"
                            '                        AppServices.ConnectionString = Value
                            '                        returnString = "ok"
                            '                    Case "PHYSICALFILEPATH"
                            '                        AppServices.PhysicalFilePath = Value
                            '                        returnString = "ok"
                            '                    Case "PHYSICALWWWPATH"
                            '                        AppServices.PhysicalWWWPath = Value
                            '                        returnString = "ok"
                            '                    Case "DOMAINNAME"
                            '                        appServices.config.domainList(0) = Value
                            '                        returnString = "ok"
                            '                    Case "ROOTPATH"
                            '                        AppServices.RootPath = Value
                            '                        returnString = "ok"
                            '                    Case "ALLOWMONITORING"
                            '                        AppServices.AllowMonitoring = EncodeBoolean(Value)
                            '                        returnString = "ok"
                            '                    Case Else
                            '                        returnString = "ERROR " & ccError_InvalidFieldName
                            '                End Select
                            '                Call SaveConfig()
                            '                'KernelServices.ConfigModified = True
                            '            End If
                            '            AppServices = Nothing
                            '        End If
                            '    End If
                        Case "FINDANDREPLACE"
                            '
                            ' Find and Replace
                            ' NOTE -- CHANGE TO CALL A CLASS ON THIS NEW THREAD - DO NOT CALL ANOTHER PROCESS
                            '
                            Throw New NotImplementedException("Find and Replace to be rewritten as an addon")
                            'FindText = getCommandArgument("FindText", queryString)
                            'If FindText <> "" Then
                            '    AppName = getCommandArgument("App", queryString)
                            '    CDefNameList = getCommandArgument("CDefNameList", queryString)
                            '    If CDefNameList <> "" Then
                            '        ReplaceText = getCommandArgument("ReplaceText", queryString)
                            '        CDefNames = Split(CDefNameList, ",")
                            '        For Ptr = 0 To UBound(CDefNames)
                            '            Dim processCmd As String
                            '            Dim processArg As String = ""
                            '            processCmd = getProgramFilesPath() & "\ccFindReplace.exe"
                            '            processArg = "app=" & AppName & " Find=""" & FindText & """ Replace=""" & ReplaceText & """  content=""" & CDefNames(Ptr) & """ "
                            '            Call runProcess(cp, processCmd, processArg)
                            '            'Call runProcess(cp,"""" & getAppPath & "\ccFindReplace.exe"" app=" & AppName & " Find=""" & FindText & """ Replace=""" & ReplaceText & """  content=""" & CDefNames(Ptr) & """ ")
                            '            'Call Shell("ccFindReplace app=" & AppName & " Find=""" & FindText & """ Replace=""" & ReplaceText & """  content=""" & CDefNames(Ptr) & """ ")
                            '        Next
                            '    End If
                            'End If
                            'returnString = "ok"
                            '
                            ' File Copy
                            '
                        Case "COPYFILE"
                            SrcFile = getCommandArgument("SrcFile", queryString)
                            If SrcFile = "" Then
                                returnString = "ERROR: Source file is not valid"
                            Else
                                DstFile = getCommandArgument("DstFile", queryString)
                                If DstFile = "" Then
                                    returnString = "ERROR: Destination file is not valid"
                                Else
                                    'fs = New fileSystemClass
                                    'Call cpCore.app.publicFiles.DeleteFile(DstFile)
                                    Call cpCore.appRootFiles.copyFile(SrcFile, DstFile)
                                    returnString = "ok"
                                End If
                            End If
                            '
                            ' Delete File
                            '

                        Case "DELETEFILE"
                            File = getCommandArgument("File", queryString)
                            If File = "" Then
                                returnString = "ERROR: file is empty"
                            Else
                                'fs = New fileSystemClass
                                Call cpCore.appRootFiles.DeleteFile(File)
                                returnString = "ok"
                            End If
                            '
                            ' Create Folder
                            '
                        Case "CREATEFOLDER"
                            FolderPath = getCommandArgument("Folder", queryString)
                            If FolderPath = "" Then
                                returnString = "ERROR: Folder is empty"
                            Else
                                'fs = New fileSystemClass
                                Call cpCore.appRootFiles.createPath(FolderPath)
                                returnString = "ok"
                            End If
                            '
                            ' Delete Folder
                            '
                        Case "DELETEFOLDER"
                            FolderPath = getCommandArgument("Folder", queryString)
                            If FolderPath = "" Then
                                returnString = "ERROR: Folder is empty"
                            Else
                                'fs = New fileSystemClass
                                Call cpCore.appRootFiles.DeleteFileFolder(FolderPath)
                                returnString = "ok"
                            End If
                            '
                            ' async cmds (run with ccCmd in a new process)
                            '
                        Case "START", "STARTALL", "UPGRADE", "UPGRADEALL", "EXPORTCDEF", "IMPORTCDEF", "RESTART", "STOP", "STOPALL", "PAUSE", "PAUSEALL", "RESUME", "RESUMEALL", "INSTALLADDONS", "INSTALLADDON", "IISRESET", "UNZIPANDDELETEFILE", "UNZIPFILE", "ZIPFILE", "REGISTERACTIVEX", "REGISTERDOTNET", "ADDSITE", "IMPORTSITE", "RUNPROCESS"
                            '
                            ' ccCmd parse the command line with a "&". Quotes in the values need to be doubled
                            '
                            cpCore.log_appendLog("serverClass.executeServerCmd, ccCMD method case")
                            '
                            Cmd = Method
                            QSPairs = Split(queryString, "&")
                            If UBound(QSPairs) >= 0 Then
                                For Ptr = 0 To UBound(QSPairs)
                                    Pos = vbInstr(1, QSPairs(Ptr), "=")
                                    QSValue = ""
                                    If Pos > 0 Then
                                        '
                                        ' encode/decode Nva is necessary because ______________________ ???
                                        '
                                        QSName = Mid(QSPairs(Ptr), 1, Pos - 1)
                                        QSName = DecodeResponseVariable(QSName)
                                        QSName = decodeNvaArgument(QSName)
                                        '
                                        QSValue = Mid(QSPairs(Ptr), Pos + 1)
                                        QSValue = DecodeResponseVariable(QSValue)
                                        QSValue = decodeNvaArgument(QSValue)
                                        '
                                        'QSValue = decodeNvaArgument(Mid(QSPairs(Ptr), Pos + 1))
                                        QSValue = vbReplace(QSValue, """", """""")
                                        '
                                        ' !!!! should have commandLine- encode/decode pair
                                        '   handle quote
                                        '   handle cr lf
                                        '
                                        Cmd = Cmd & " " & QSName & "=""" & QSValue & """"
                                    Else
                                        QSName = QSPairs(Ptr)
                                        QSName = DecodeResponseVariable(QSName)
                                        QSName = decodeNvaArgument(QSName)
                                        Cmd = Cmd & " " & QSName
                                    End If
                                Next
                            End If
                            If vbUCase(Method) = "RUNPROCESS" Then
                                Method = Method
                            End If
                            '
                            cpCore.log_appendLog("serverClass.executeServerCmd, adding command to Queue [" & Cmd & "]")
                            '
                            If Not addAsyncCmd(cpCore, Cmd, False) Then
                                returnString = "Command was blocked because there are too many commands waiting, or this is a duplicate command."
                            Else
                                returnString = "ok"
                            End If
                            '
                            ' Unknown call
                            '
                        Case Else
                            returnString = "ERROR " & ignoreString & vbCrLf & "unknown command [" & Method & "]"
                            '
                            cpCore.log_appendLog("serverClass.executeServerCmd, unknown cmd=[" & Cmd & "]")
                            '
                    End Select
                End If
            Catch ex As Exception
                Call handleExceptionResume(ex, "ExecuteServerCmd", "Unexpected error")
                returnString = "<ERROR>unknown error executing command [" & Method & "], arguments [" & queryString & "]</ERROR>"
            End Try
            Return returnString
        End Function

        Private Sub processTimerTick()
            Try
                '
                Dim Slice As Double
                Dim Tryx As Double
                Dim rightNow As Date
                'Dim memCheck As New memCheckClass
                '
                'Call memCheck.logMemCheck()
                'memCheck = Nothing
                '
                rightNow = DateTime.Now
                'Call appendTraceLog("", Now.ToLongTimeString & " processTimerTick")
                If Not ProcessTimerInProcess Then
                    '
                    ' If not debug and not inprocess, run processes
                    '
                    ProcessTimerInProcess = True
                    If True Then
                        '
                        ' AutoStart Process
                        '
                        'DoEvents()
                        'If AutoStartAlarmTime = Date.MinValue Then
                        '    AutoStartAlarmTime = rightNow.AddSeconds(10)
                        'ElseIf rightNow > AutoStartAlarmTime Then
                        '    Call AutoStartSites()
                        '    AutoStartAlarmTime = AutoStartAlarmTime.AddSeconds(10)
                        '    If rightNow > AutoStartAlarmTime Then
                        '        AutoStartAlarmTime = rightNow.AddSeconds(10)
                        '    End If
                        'End If
                        '
                        ' (skip for now)Test if any site in cluster needs to be updated
                        '
                        '
                        ' Run addons from each app in cluster
                        '
                        If (True) Then
                            AllSitesNotReadyCnt = 0
                            '
                            ' Run Site Processes
                            '
                            If SiteProcessAlarmTime = Date.MinValue Then
                                SiteProcessAlarmTime = rightNow.AddMinutes(1)
                            ElseIf rightNow > SiteProcessAlarmTime Then
                                Call RunSiteProcesses()
                                SiteProcessAlarmTime = SiteProcessAlarmTime.AddMinutes(1)
                                If rightNow > SiteProcessAlarmTime Then
                                    SiteProcessAlarmTime = rightNow.AddMinutes(1)
                                End If
                            End If
                            ''
                            '' Legacy Tasks - still handles exports
                            ''
                            ''DoEvents()
                            'If TaskAlarmTime = Date.MinValue Then
                            '    TaskAlarmTime = rightNow.AddSeconds(62)
                            'ElseIf rightNow > TaskAlarmTime Then
                            '    Call runProcess(cp,getProgramFilesPath() & "\ccTasks.exe")
                            '    TaskAlarmTime = TaskAlarmTime.AddSeconds(62)
                            '    If rightNow > TaskAlarmTime Then
                            '        TaskAlarmTime = rightNow.AddSeconds(62)
                            '    End If
                            'End If
                            '
                            ' retired. uses system bandwidth and has low user value
                            ''
                            '' Remote Name update (convert to site process)
                            ''
                            'DoEvents
                            'If RemoteNameAlarmTime = Date.MinValue Then
                            '    RemoteNameAlarmTime = NowTime + RemoteNameInterval
                            'ElseIf NowTime > RemoteNameAlarmTime Then
                            '    Call addAsyncCmd(cp,"UPDATEREMOTENAMES", True)
                            '    RemoteNameLabel.Caption = EncodeInteger(RemoteNameLabel.Caption) + 1
                            '    RemoteNameAlarmTime = RemoteNameAlarmTime + RemoteNameInterval
                            '    If NowTime > RemoteNameAlarmTime Then
                            '        RemoteNameAlarmTime = NowTime + RemoteNameInterval
                            '    End If
                            'End If
                            '
                            ' Email Process
                            '
                            'DoEvents()
                            If EmailAlarmTime = Date.MinValue Then
                                EmailAlarmTime = rightNow.AddSeconds(EmailIntervalSeconds)
                            ElseIf rightNow > EmailAlarmTime Then
                                Call runProcess(cpCore, getProgramFilesPath() & "\ccProcessEmail.exe")
                                EmailAlarmTime = EmailAlarmTime.AddSeconds(EmailIntervalSeconds)
                                If rightNow > EmailAlarmTime Then
                                    EmailAlarmTime = rightNow.AddSeconds(EmailIntervalSeconds)
                                End If
                            End If
                            '
                            ' Housekeeping
                            '
                            'DoEvents()
                            If HouseKeepAlarmTime = Date.MinValue Then
                                HouseKeepAlarmTime = rightNow.AddSeconds(HouseKeepIntervalSeconds)
                            ElseIf rightNow > HouseKeepAlarmTime Then
                                '
                                ' delay execution randomly  - to distribute hits to the support site
                                '
                                Randomize()
                                Slice = (1.0! / 10.0!)
                                Tryx = Rnd()
                                If Tryx < Slice Then
                                    Call runProcess(cpCore, getProgramFilesPath() & "\ccHouseKeep.exe")
                                End If
                                HouseKeepAlarmTime = HouseKeepAlarmTime.AddSeconds(HouseKeepIntervalSeconds)
                                If rightNow > HouseKeepAlarmTime Then
                                    HouseKeepAlarmTime = rightNow.AddSeconds(HouseKeepIntervalSeconds)
                                End If
                            End If
                        End If
                    End If
                    ProcessTimerInProcess = False
                End If
            Catch ex As Exception
                Call handleExceptionResume(ex, "ProcessTimer_Timer", "ErrorTrap")
            End Try
        End Sub

        Private Sub RunSiteProcesses()
            '  Dim CSConnection As appEnvironmentStruc
            'Dim cmc As cpCoreClass
            Dim hint As String = ""
            Try
                '
                Dim ProcessRunOnce As Boolean
                Dim ProcessNextRun As Date
                Dim ProcessID As Integer
                Dim processName As String
                Dim ProcessInterval As Integer
                'Dim KernelService As New kernelServicesClass
                Dim SQLNow As String
                Dim CS As Integer
                Dim SQL As String
                Dim NextRun As Date
                Dim RightNow As Date
                'Dim appStatus As Integer
                Dim AppName As String
                Dim clusterAppList As List(Of String)
                'Dim clusterServices As New clusterServicesClass()
                'Dim asv As appServicesClass
                Dim cpSite As CPClass
                '
                hint &= ",entering"
                '
                RightNow = DateTime.Now
                SQLNow = cpCore.db.encodeSQLDate(RightNow)
                For Each kvp As KeyValuePair(Of String, appConfigClass) In cpCore.clusterConfig.apps
                    AppName = kvp.Value.name
                    '
                    ' permissions issue -- this is a root process - maybe the token will be saved in a configuration file
                    '
                    cpSite = New CPClass(AppName)
                    If cpSite.core.appStatus = applicationStatusEnum.ApplicationStatusReady Then
                        hint &= ",app [" & AppName & "] is running, setup cp and cmc"
                        '
                        ' Execute Processes
                        '
                        Try
                            hint &= ",go through process addons that need to be run"
                            '
                            ' for now run an sql to get processes, eventually cache in variant cache
                            '
                            SQL = "(Active<>0)and(name<>'')and(" _
                                    & " ((ProcessRunOnce is not null)and(ProcessRunOnce<>0))" _
                                    & " or((ProcessInterval is not null)and(ProcessInterval<>0)and(ProcessNextRun is null))" _
                                    & " or(ProcessNextRun<" & SQLNow & ")" _
                                    & ")"
                            CS = cpSite.core.db.csOpen("add-ons", SQL)
                            Do While cpSite.core.db.cs_Ok(CS)
                                ProcessInterval = cpSite.core.db.cs_getInteger(CS, "ProcessInterval")
                                ProcessID = cpSite.core.db.cs_getInteger(CS, "ID")
                                processName = cpSite.core.db.cs_getText(CS, "name")
                                ProcessRunOnce = cpSite.core.db.cs_getBoolean(CS, "ProcessRunOnce")
                                ProcessNextRun = cpSite.core.db.db_GetCSDate(CS, "ProcessNextRun")
                                NextRun = Date.MinValue
                                hint &= ",run addon " & processName
                                If ProcessInterval > 0 Then
                                    NextRun = RightNow.AddMinutes(ProcessInterval)
                                End If
                                If False Then ' some of the servers are reporting an invalid hostprocessid
                                    'If cpSite.core.app.csv_GetCSInteger(CS, "ProcessServerKey") <> HostProcessID Then
                                    '
                                    ' Server has been restarted, reset next run
                                    '
                                    'Call cpSite.core.app.cpSite.core.app.csv_SetCS(CS, "ProcessServerKey", HostProcessID)
                                    Call cpSite.core.db.cs_set(CS, "ProcessNextRun", NextRun)
                                ElseIf ProcessRunOnce Then
                                    '
                                    ' Run Once
                                    '
                                    Call cpSite.core.db.cs_set(CS, "ProcessRunOnce", False)
                                    Call cpSite.core.db.cs_set(CS, "ProcessNextRun", NextRun)
                                    Call addAsyncCmd(cpCore, "runprocess appname=""" & cpSite.core.appConfig.name & """ addonid=""" & ProcessID & """", True)
                                    'Call addAsyncCmd(cp,"runprocess appname=""" & cpSite.core.appEnvironment.name & """ addonname=""" & ProcessName & """", True)
                                ElseIf cpSite.core.db.db_GetCSDate(CS, "ProcessNextRun") = Date.MinValue Then
                                    '
                                    ' Interval is OK but NextRun is 0, just set next run
                                    '
                                    Call cpSite.core.db.cs_set(CS, "ProcessNextRun", NextRun)
                                ElseIf ProcessNextRun < RightNow Then
                                    '
                                    ' All is OK, triggered on NextRun, Cycle RightNow
                                    '
                                    Call addAsyncCmd(cpCore, "runprocess appname=""" & cpSite.core.appConfig.name & """ addonid=""" & ProcessID & """", True)
                                    Call cpSite.core.db.cs_set(CS, "ProcessNextRun", NextRun)
                                End If
                                Call cpSite.core.db.db_csGoNext(CS)
                                cpSite.Dispose()
                            Loop
                            Call cpSite.core.db.cs_Close(CS)
                        Catch ex As Exception
                            '
                            ' error on this site, skip to next
                            '
                            Call handleExceptionResume(ex, "RunSiteProcesses", "ErrorTrap")
                        End Try
                    End If
                    hint &= ",app done"
                Next
            Catch ex As Exception
                Call handleExceptionResume(ex, "RunSiteProcesses", "ErrorTrap")
            End Try
#If includeTracing Then
            'Call appendTraceLog("RunSiteProcesses", "exit RunSiteProcesses, hint=[" & hint & "]")
#End If
        End Sub
        '
        ' Returns false if it could not be added (full or duplicate)
        '
        Private Function addAsyncCmd(cpCore As coreClass, ByVal Command As String, ByVal BlockDuplicates As Boolean) As Boolean
            Dim returnBoolean As Boolean = False
            Try
                '
                Dim Ptr As Integer
                Dim LcaseCommand As String
                '
                cpCore.log_appendLog("serverClass.addAsyncCmd, command=[" & Command & "], BlockDuplicates=[" & BlockDuplicates & "]")
                '
                returnBoolean = True
                LcaseCommand = vbLCase(Command)
                If asyncCmdQueueCnt >= asyncCmdQueueLimit Then
                    '
                    ' Server Queue is too large, block the add
                    '
                    returnBoolean = False
                    cpCore.log_appendLog("addAsyncCmd, Server Cmd was blocked because Server Queue is too long [" & asyncCmdQueueCnt & "], command was [" & Command & "]")
                ElseIf BlockDuplicates Then
                    '
                    ' Search for a duplicate
                    '
                    For Ptr = 0 To asyncCmdQueueCnt - 1
                        If vbLCase(asyncCmdQueue(Ptr)) = LcaseCommand Then
                            returnBoolean = False
                            cpCore.log_appendLog("addAsyncCmd, Server Cmd was blocked because there is a duplicate in the queue already, [" & Command & "]")
                            Exit For
                        End If
                    Next
                End If
                '
                ' Add it to the queue and shell out to the command
                '
                If returnBoolean Then
                    If asyncCmdQueueCnt >= asyncCmdQueueSize Then
                        asyncCmdQueueSize = asyncCmdQueueSize + 5
                        ReDim Preserve asyncCmdQueue(asyncCmdQueueSize)
                    End If
                    asyncCmdQueue(asyncCmdQueueCnt) = Command
                    asyncCmdQueueCnt = asyncCmdQueueCnt + 1
                    '
                    cpCore.log_appendLog("serverClass.addAsyncCmd, command added to ServerCmds, index=[" & asyncCmdQueueCnt & "], call runProcess...")
                    '
                    Call runProcess(cpCore, getProgramFilesPath() & "\ccCmd.exe", "port=" & serverListenerPort & " max=" & maxCmdInstances)
                End If
                '
                cpCore.log_appendLog("serverClass.addAsyncCmd, exit")
                '
            Catch ex As Exception
                Call handleExceptionResume(ex, "addAsyncCmd", "ErrorTrap")
            End Try
            Return returnBoolean
        End Function
        '
        '
        '
        Private Function getNextAsyncCmdFromQueue() As String
            Dim returnString As String = ""
            Try
                Dim Ptr As Integer
                '
                If asyncCmdQueueCnt > 0 Then
                    returnString = asyncCmdQueue(0)
                    If asyncCmdQueueCnt > 1 Then
                        For Ptr = 1 To asyncCmdQueueCnt
                            asyncCmdQueue(Ptr - 1) = asyncCmdQueue(Ptr)
                        Next
                    End If
                    asyncCmdQueueCnt = asyncCmdQueueCnt - 1
                End If
#If includeTracing Then
                'Call appendTraceLog("GetNextAsyncCmdFromQueue", "returning [" & returnString & "]")
#End If
            Catch ex As Exception
                Call handleExceptionResume(ex, "GetNextAsyncCmdFromQueue", "ErrorTrap")
            End Try
            Return returnString
        End Function
        '
        '
        '
        Private Function getCommandArgument(ByVal argName As String, ByVal cmdQueryString As String) As String
            getCommandArgument = decodeNvaArgument(DecodeResponseVariable(getSimpleNameValue(argName, cmdQueryString, "", "&")))
        End Function
        '
        '==========================================================================================
        '   Service Control manager calls this to start. Timer set and in 1 second, start actually starts
        '       - this was a way in vb6 to get off the main thread. - convert to a thread later
        '==========================================================================================
        '
        Private Sub scmStart()
            '
            startTimer.Interval = 1000
            startTimer.Enabled = True
            '
        End Sub
        '
        '
        '
        Public Function PrevInstance() As Boolean
            If UBound(Diagnostics.Process.GetProcessesByName _
               (Diagnostics.Process.GetCurrentProcess.ProcessName)) _
               > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
        '
        'Private Sub DoEvents()
        'End Sub
        '
        '======================================================================================
        '   Log a reported error
        '======================================================================================
        '
        Public Sub handleExceptionResume(ByVal ex As Exception, ByVal MethodName As String, ByVal LogCopy As String)
            cpCore.appendLogWithLegacyRow("(server)", LogCopy, "server", "serverClass", MethodName, -1, ex.Source, ex.ToString, True, True, "", "", "")
        End Sub
        ''
        ''======================================================================================
        ''   Log a message
        ''======================================================================================
        ''
        'Private Sub appendLog(MethodName As String, message As String, Optional forceVerbose As Boolean = False)
        '    Call AppendLog2(cpCore,"(server)", message, "server", "serverClass", MethodName, 0, "", "", False, True, "", "", "")
        '    If verboseLogging Or forceVerbose Then
        '        'Call appendTraceLog( "", Now.ToLongTimeString & " appendLog, method [" & MethodName & "], [" & message & "]")
        '    End If
        'End Sub
        ''
        ''======================================================================================
        ''   Log a message is verboseLogging true 
        ''======================================================================================
        ''
        'Private Sub appendTraceLog(methodName As String, message As String)
        '    Call appendLog(methodName, message, True)
        'End Sub
        '
        ' append log
        '
        Private Sub appendTcpLog(ByVal logText As String)
            If False Then
                Dim retryCnt As Integer = 0
                Dim success As Boolean = False
                Do While retryCnt < 5 And Not success
                    Try
                        Using outfile As New StreamWriter("c:\clibTcpDebug.log", True)
                            outfile.Write(vbCrLf & Now.ToString() & " " & logText)
                        End Using
                        success = True
                    Catch ex As Exception
                        Call Thread.Sleep(10)
                    End Try
                    retryCnt += 1
                Loop
            End If
        End Sub
        '
        Private Sub appendTraceLog(ByVal method As String, ByVal logText As String)
            cpCore.log_appendLog(logText, "", "trace")
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
