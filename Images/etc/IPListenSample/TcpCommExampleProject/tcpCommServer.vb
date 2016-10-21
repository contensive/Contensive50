Imports System
Imports System.Threading
Imports System.Net
Imports System.Net.Sockets
Imports System.IO
Imports System.Reflection
Imports System.Runtime.InteropServices

Public Class TcpCommServer
    '
    ' Define the callback delegate type
    '
    Public Delegate Sub ServerCallbackDelegate(ByVal bytes() As Byte, ByVal connectionId As Int32, ByVal dataChannel As Integer)
    '
    ' Create Delegate object
    '
    Public ServerCallbackObject As ServerCallbackDelegate
    '
    ' the listener
    '
    Private Listener As TcpListener
    '
    Public errMsg As String
    Private allowConnectionsToContinue As Boolean = False
    Private blockSize As UInt16
    Private Port As Integer
    Private localIpAddress As IPAddress
    Private Mbps As UInt32
    Public listenerIsRunning As Boolean = False
    Public connectionStoreCollection As New ArrayList
    '
    ' Class for Object to pass data between threads(?)
    '
    Private Class connectionStoreClass
        Public connectionID As Int32
        Public UserBytesToBeSentAvailable As Boolean = False
        Public UserBytesToBeSent As New MemoryStream
        Public UserOutputChannel As Byte
        Public SystemBytesToBeSentAvailable As Boolean = False
        Public SystemBytesToBeSent() As Byte
        Public SystemOutputChannel As Byte
        Public client As TcpClient
        Public IsRunning As Boolean = False
        Public remoteIpAddress As System.Net.IPAddress
        Public bytesRecieved() As Byte
        Public disConnect As Boolean = False
        Public bytesSentThisSecond As Int32 = 0
        Public bytesRecievedThisSecond As Int32 = 0
        Public fileBytesRecieved As Int64 = 0
        Public filebytesSent As Int64 = 0
        Public SendingFile As Boolean = False
        Public FileBeingSentPath As String
        Public IncomingFileSize As Int64
        Public IncomingFileName As String
        Public ReceivingFile As Boolean = False
        Public sendPacketSize As Boolean = False
        Public fileReader As FileStream
        Public fileWriter As clsAsyncUnbuffWriter
        Public ReceivedFilesFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) & "\ServerReceivedFiles"
        Public userName As String
        Public password As String

        Public Sub New(ByVal _client As TcpClient, ByVal _connectionId As Int32)
            client = _client
            connectionID = _connectionId
        End Sub

        Public Sub Close()
            disConnect = True

            Try
                client.Client.Blocking = False
                client.Client.Close()
            Catch ex As Exception
                IsRunning = False
            End Try
        End Sub
    End Class
    '
    ' StrToByteArray - util, convert str to byte array
    '
    Private Function StrToByteArray(ByVal text As String) As Byte()
        Dim encoding As New System.Text.UTF8Encoding()
        StrToByteArray = encoding.GetBytes(text)
    End Function
    '
    ' util convert string to byte array
    '
    Private Function BytesToString(ByVal data() As Byte) As String
        Dim enc As New System.Text.UTF8Encoding()
        BytesToString = enc.GetString(data)
    End Function
    '
    ' TcpCommServer constructor
    ' CallbackForm must implement an UpdateUI Sub.
    '
    Public Sub New(ByVal callbackMethod As ServerCallbackDelegate, Optional ByVal _throttledBytesPerSecond As UInt32 = 9000000)

        Mbps = _throttledBytesPerSecond

        ' BlockSize should be 62500 or 63100, depending on requested speed. 
        ' Excellent performance, and works great with throttling.
        Dim _blockSize As UInt16

        ' Get corrected blocksize for throttling.
        If Mbps < 300000 Then
            If Mbps > 16000 Then
                blockSize = 4000
            Else
                blockSize = CUShort((Mbps / 4))
            End If
        ElseIf Mbps > 300000 And Mbps < 500000 Then
            blockSize = 16000
        ElseIf Mbps > 500000 And Mbps < 1000000 Then
            blockSize = 32000
        Else
            Dim count As UInt32 = 0
            Dim aFourth As Decimal = 0

            If Mbps > 25000000 Then
                _blockSize = 63100
            Else
                _blockSize = 62500
            End If

            aFourth = CDec(Mbps / 4)

            Do
                count += _blockSize
                If (count + _blockSize) > aFourth Then
                    Mbps = CUInt(count * 4)
                    blockSize = _blockSize
                    Exit Do
                End If
            Loop

        End If

        ' Initialize the delegate object to point to the user's callback method.
        ServerCallbackObject = callbackMethod

    End Sub
    '
    ' set the connectioin bandwidth
    '
    Public Sub ThrottleNetworkBps(ByVal bytesPerSecond As UInteger)
        ' Default value is 9000000 Mbps. Ok throughput, and 
        ' good performance for the server (low CPU usage).
        Mbps = bytesPerSecond
    End Sub
    '
    ' Start Listening
    '   create listenerThread and call start
    '
    Public Sub startListening(ByVal prt As Integer)
        Port = prt
        localIpAddress = GetLocalIpAddress()
        allowConnectionsToContinue = True
        listenerIsRunning = True

        Dim listenerThread As New Thread(AddressOf listenerThread_startListening)
        listenerThread.Name = "Server Listener Thread"
        listenerThread.Start()
    End Sub
    '
    ' stop listening and shut down all open connections
    '
    Public Sub stopListeningCloseConnections()
        Dim connectionsMightBeOpen As Boolean = True
        '
        allowConnectionsToContinue = False
        '
        While connectionsMightBeOpen
            Try
                For Each connectionStore As connectionStoreClass In connectionStoreCollection
                    connectionStore.Close()
                Next
            Catch ex As Exception
                ' wtf
            End Try
            Try
                For Each connectionStore As connectionStoreClass In connectionStoreCollection
                    If connectionStore.IsRunning Then Exit Try
                Next
                connectionsMightBeOpen = False
            Catch ex As Exception
                ' wtf
            End Try
        End While
        Try
            Listener.Stop()
        Catch ex As Exception
            ' wtf
        End Try
        listenerIsRunning = False
    End Sub
    '
    ' util, get local ip address
    '
    Private Function GetLocalIpAddress() As System.Net.IPAddress
        Dim strHostName As String
        Dim addresses() As System.Net.IPAddress

        strHostName = System.Net.Dns.GetHostName()
        addresses = System.Net.Dns.GetHostAddresses(strHostName)

        ' Find an IpV4 address
        For Each address As System.Net.IPAddress In addresses
            If address.ToString.Contains(".") Then
                Return address
            End If
        Next

        ' No IpV4 address? Return the loopback address.
        Return System.Net.IPAddress.Loopback
    End Function
    '
    ' Property getBlockSize (but written as function)
    '
    Public Function GetBlocksize() As UInt16
        Return blockSize
    End Function
    '
    ' getFile - not sure
    '
    Public Function GetFile(ByVal _path As String, ByVal connectionId As Int32) As Boolean
        Dim foundConnection As Boolean = False
        GetFile = True

        ' Find the connection we want to talk to and send it a Get File Request
        For Each connectionStore As connectionStoreClass In connectionStoreCollection
            If connectionStore.connectionID = connectionId Then
                ' we found it.
                foundConnection = True
                Do
                    If Not connectionStore.UserBytesToBeSentAvailable Then
                        SyncLock connectionStore.UserBytesToBeSent
                            connectionStore.UserBytesToBeSent.Close()
                            connectionStore.UserBytesToBeSent = Nothing
                            connectionStore.UserBytesToBeSent = New MemoryStream(StrToByteArray("GFR:" & _path))
                            connectionStore.UserOutputChannel = 254 ' Text messages / commands on channel 254
                            connectionStore.UserBytesToBeSentAvailable = True
                        End SyncLock
                        Exit Do
                    End If

                    If Not connectionStore.IsRunning Then Exit Do
                    Application.DoEvents()
                Loop
            End If
        Next

        If Not foundConnection Then Return False
    End Function
    '
    ' sendFile - not sure
    '
    Public Function SendFile(ByVal _path As String, ByVal connectionId As Int32) As Boolean
        Dim foundConnection As Boolean = False
        SendFile = True

        ' Find the connection we want to talk to and send it a Send File Request
        For Each connectionStore As connectionStoreClass In connectionStoreCollection
            If connectionStore.connectionID = connectionId Then
                ' we found it.
                foundConnection = True
                Do
                    If Not connectionStore.UserBytesToBeSentAvailable Then
                        SyncLock connectionStore.UserBytesToBeSent
                            connectionStore.UserBytesToBeSent.Close()
                            connectionStore.UserBytesToBeSent = Nothing
                            connectionStore.UserBytesToBeSent = New MemoryStream(StrToByteArray("SFR:" & _path))
                            connectionStore.UserOutputChannel = 254 ' Text messages / commands on channel 254
                            connectionStore.UserBytesToBeSentAvailable = True
                        End SyncLock
                        Exit Do
                    End If

                    If Not connectionStore.IsRunning Then Exit Do
                    Application.DoEvents()
                Loop
            End If
        Next

        If Not foundConnection Then Return False
    End Function
    '
    ' sendBytes, not sure
    '
    Public Function SendBytes(ByVal bytes() As Byte, Optional ByVal channel As Byte = 1, Optional ByVal connectionId As Int32 = -1) As Boolean

        Dim foundConnection As Boolean = False
        SendBytes = True

        If channel = 0 Or channel > 250 Then
            MsgBox("Data can not be sent using channel numbers less then 1 or greater then 250.", MsgBoxStyle.Critical, "TCP_Server")
            Exit Function
        End If

        If connectionId > -1 Then
            ' Find the connection we want to talk to and send it the message
            For Each connectionStore As connectionStoreClass In connectionStoreCollection
                If connectionStore.connectionID = connectionId Then
                    ' we found it.
                    foundConnection = True
                    Do
                        If Not connectionStore.UserBytesToBeSentAvailable Then
                            SyncLock connectionStore.UserBytesToBeSent
                                connectionStore.UserBytesToBeSent.Close()
                                connectionStore.UserBytesToBeSent = Nothing
                                connectionStore.UserBytesToBeSent = New MemoryStream(bytes)
                                connectionStore.UserOutputChannel = channel
                                connectionStore.UserBytesToBeSentAvailable = True
                            End SyncLock
                            Exit Do
                        End If

                        If Not connectionStore.IsRunning Then Exit Do
                        Application.DoEvents()
                    Loop
                End If
            Next

            If Not foundConnection Then Return False
        ElseIf connectionId = -1 Then
            ' Send our message to everyone connected
            For Each connectionStore As connectionStoreClass In connectionStoreCollection
                If connectionStore.IsRunning Then
                    Do
                        If Not connectionStore.UserBytesToBeSentAvailable Then
                            SyncLock connectionStore.UserBytesToBeSent
                                connectionStore.UserBytesToBeSent.Close()
                                connectionStore.UserBytesToBeSent = Nothing
                                connectionStore.UserBytesToBeSent = New MemoryStream(bytes)
                                connectionStore.UserOutputChannel = channel
                                connectionStore.UserBytesToBeSentAvailable = True
                            End SyncLock
                            Exit Do
                        End If

                        If Not connectionStore.IsRunning Then Exit Do
                        Application.DoEvents()
                    Loop
                End If
            Next

        Else
            Return False
        End If

    End Function
    '
    ' receive bytes, not sure
    '
    Private Function RcvBytes(ByVal data() As Byte, ByVal connectionId As Int32, Optional ByVal dataChannel As Integer = 1) As Boolean
        ' dataType: >0 = data channel, > 250 = internal messages. 0 is an invalid channel number (it's the puck)

        If dataChannel < 1 Then
            RcvBytes = False
            Exit Function
        End If

        Try
            ' Check to see if our app is closing
            If Not allowConnectionsToContinue Then Exit Function

            ServerCallbackObject(data, connectionId, dataChannel)

        Catch ex As Exception
            RcvBytes = False

            ' An unexpected error.
            Debug.WriteLine("Unexpected error in server\RcvBytes: " & ex.Message)
        End Try
    End Function
    '
    ' send some kind of message, not sure
    '
    Private Function SendExternalSystemMessage(ByVal message As String, ByVal connectionStore As connectionStoreClass) As Boolean

        connectionStore.SystemBytesToBeSent = StrToByteArray(message)
        connectionStore.SystemOutputChannel = 254 ' Text messages / commands on channel 254
        connectionStore.SystemBytesToBeSentAvailable = True

    End Function
    '
    ' security for connections
    '
    Private Function CheckConnectionPermissions(ByVal connectionStore As connectionStoreClass, ByVal cmd As String) As Boolean
        ' Your security code here...

        Return True
    End Function
    '
    ' begin to send file
    '
    Private Function BeginFileSend(ByVal _path As String, ByVal connectionStore As connectionStoreClass, ByVal fileLength As Long) As Boolean

        Try

            connectionStore.fileReader = New FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.None, clsAsyncUnbuffWriter.GetPageSize)
            connectionStore.SendingFile = True
            BeginFileSend = True

        Catch ex As Exception
            BeginFileSend = False
            _path = ex.Message
            connectionStore.SendingFile = False
        End Try

        Try
            If Not BeginFileSend Then connectionStore.fileReader.Close()
        Catch ex As Exception
        End Try

    End Function
    '
    ' GetMoreFileBytesIfAvailable, not sure
    '
    Private Sub GetMoreFileBytesIfAvailable(ByVal connectionStore As connectionStoreClass)
        Dim bytesRead As Int32 = 0

        If connectionStore.SendingFile And Not connectionStore.SystemBytesToBeSentAvailable Then
            Try
                If connectionStore.SystemBytesToBeSent.Length <> blockSize Then ReDim connectionStore.SystemBytesToBeSent(blockSize - 1)
                bytesRead = connectionStore.fileReader.Read(connectionStore.SystemBytesToBeSent, 0, blockSize)
                If bytesRead <> blockSize Then ReDim Preserve connectionStore.SystemBytesToBeSent(bytesRead - 1)

                If bytesRead > 0 Then
                    connectionStore.SystemOutputChannel = 253 ' File transfer from server to client
                    connectionStore.SystemBytesToBeSentAvailable = True
                Else

                    ReDim connectionStore.SystemBytesToBeSent(blockSize - 1)
                    SendExternalSystemMessage("->Done", connectionStore) ' Send the client a completion notice.
                    connectionStore.SendingFile = False

                    ' Clean up
                    connectionStore.fileReader.Close()
                    connectionStore.fileReader = Nothing
                    GC.GetTotalMemory(True)
                End If
            Catch ex As Exception
                SendExternalSystemMessage("ERR: " & ex.Message, connectionStore)

                ' We're finished.
                ReDim connectionStore.SystemBytesToBeSent(blockSize - 1)
                connectionStore.SendingFile = False
                connectionStore.fileReader.Close()
            End Try
        End If

    End Sub
    '
    ' util - get filename from path
    '
    Private Function GetFilenameFromPath(ByRef filePath As String) As String
        Dim filePathParts() As String

        If filePath.Trim = "" Then Return ""

        Try
            filePathParts = Split(filePath, "\")
            GetFilenameFromPath = filePathParts(filePathParts.Length - 1)
        Catch ex As Exception
            filePath = ex.Message
            Return ""
        End Try

    End Function
    '
    ' util - create folder path
    '
    Private Function CreateFolders(ByVal _path As String) As Boolean
        CreateFolders = True

        Dim parts() As String
        Dim path As String = ""
        Dim count As Int32
        parts = Split(_path, "\")

        path = parts(0)
        For count = 1 To parts.Length - 2
            path += "\" & parts(count)
            Try
                If Not Directory.Exists(path) Then
                    Directory.CreateDirectory(path)
                End If
            Catch ex As Exception
            End Try
        Next

    End Function
    '
    ' begin to receive a file
    '
    Private Function BeginToReceiveAFile(ByVal _path As String, ByVal connectionStore As connectionStoreClass) As Boolean
        Dim readBuffer As Int32 = 0
        connectionStore.ReceivingFile = True
        BeginToReceiveAFile = True
        connectionStore.fileBytesRecieved = 0

        Try

            CreateFolders(_path) ' Just a 256k write buffer for the server. Let's try to avoid memory problems...
            connectionStore.fileWriter = New clsAsyncUnbuffWriter(_path, True, 1024 * 256, connectionStore.IncomingFileSize)

        Catch ex As Exception
            _path = ex.Message
            connectionStore.ReceivingFile = False
        End Try

        If Not connectionStore.ReceivingFile Then
            Try
                connectionStore.fileWriter.Close()
            Catch ex As Exception
            End Try
            Return False
        End If
    End Function
    '
    ' handle incoming bytes
    '
    Private Function HandleIncomingFileBytes(ByRef bytes() As Byte, ByVal connectionStore As connectionStoreClass) As Boolean

        Try
            connectionStore.fileWriter.Write(bytes, bytes.Length)
            HandleIncomingFileBytes = True
        Catch ex As Exception
            HandleIncomingFileBytes = False
        End Try

    End Function
    '
    ' finish receiving files
    '
    Private Sub FinishReceivingTheFile(ByVal connectionStore As connectionStoreClass)
        Try
            connectionStore.fileWriter.Close()
            connectionStore.fileWriter = Nothing
            connectionStore.ReceivingFile = False
        Catch ex As Exception
            connectionStore.ReceivingFile = False
        End Try
    End Sub
    '
    ' handle incoming messages
    '
    Private Sub HandleIncomingSystemMessages(ByVal bytes() As Byte, ByVal channel As Int32, ByVal connectionStore As connectionStoreClass)

        If channel = 254 Then ' Text commands / messages passed between server and client
            Dim message As String = BytesToString(bytes)
            Dim filePath As String
            Dim tmp As String = ""

            ' Get File Request: The client wants us to send them a file.
            If message.Length > 4 Then tmp = message.Substring(0, 4)
            If tmp = "GFR:" Then
                ' Get file path...
                filePath = message.Substring(4, message.Length - 4)

                ' Does it exist?
                If File.Exists(filePath) Then
                    ' Do they have permission to get this file?
                    If CheckConnectionPermissions(connectionStore, "GFR") Then
                        ' Are we already busy sending them a file?
                        If Not connectionStore.SendingFile Then
                            Dim _theFilesInfo As New FileInfo(filePath)
                            If BeginFileSend(filePath, connectionStore, _theFilesInfo.Length) Then
                                ' Send only the file NAME. It will have a different path on the other side.
                                SendExternalSystemMessage("Sending:" & GetFilenameFromPath(filePath) & _
                                                          ":" & _theFilesInfo.Length, connectionStore)
                            Else
                                ' FilePath contains the error message.
                                SendExternalSystemMessage("ERR: " & filePath, connectionStore)
                            End If
                        Else
                            ' There's already a GFR in progress.
                            SendExternalSystemMessage("ERR: File: ''" & _
                                                      connectionStore.FileBeingSentPath & _
                                                      "'' is still in progress. Only one file " & _
                                                      "may be transfered (from server to client) at a time.", connectionStore)
                        End If
                    Else
                        ' This user doesn't have rights to "get" this file. Send an error.
                        SendExternalSystemMessage("ERR: You do not have permission to receive files. Access Denied.", connectionStore)
                    End If
                Else
                    ' File doesn't exist. Send an error.
                    SendExternalSystemMessage("ERR: The requested file can not be found by the server.", connectionStore)
                End If
            End If

            ' We're being informed that we will be receiving a file:
            If message.Length > 7 Then tmp = message.Substring(0, 8)
            If tmp = "Sending:" Then
                ' Strip away the headder...
                Dim msgParts() As String = Split(message, ":")
                connectionStore.IncomingFileSize = Convert.ToInt64(msgParts(2))
                connectionStore.IncomingFileName = msgParts(1)
                tmp = connectionStore.ReceivedFilesFolder & "\" & connectionStore.IncomingFileName
                SystemMessage("Receiving file: " & connectionStore.IncomingFileName)
                If Not BeginToReceiveAFile(tmp, connectionStore) Then
                    SystemMessage("ERR: " & tmp)
                    SendExternalSystemMessage("Abort->", connectionStore)
                End If
            End If

            If message = "<-Done" Then
                FinishReceivingTheFile(connectionStore)
                SystemMessage("<-Done")
            End If

            ' We've been notified that no file data will be forthcoming.
            If message = "Abort<-" Then
                WrapUpIncomingFile(connectionStore)
                SystemMessage("<-Aborted.")
                SendExternalSystemMessage("<-Aborted.", connectionStore)
            End If

            ' Send File Request: The client wants to send us a file.
            If message.Length > 4 Then tmp = message.Substring(0, 4)
            If tmp = "SFR:" Then
                If CheckConnectionPermissions(connectionStore, "SFR") Then
                    Dim parts() As String
                    parts = Split(message, "SFR:")
                    SendExternalSystemMessage("GFR:" & parts(1), connectionStore)
                Else
                    ' This user doesn't have rights to send us a file. Send an error.
                    SendExternalSystemMessage("ERR: You do not have permission to send files. Access Denied.", connectionStore)
                End If
            End If

            If message.Length > 4 Then tmp = message.Substring(0, 4)
            If tmp = "GDR:" Then ' Get Directory Request
                ' Send each file in the directory and all subdirectories.
                ' To be implemented in the future.
            End If

            If message.Length > 4 Then tmp = message.Substring(0, 4)
            If tmp = "ERR:" Then ' The client has sent us an error message.
                ' Pass it on up to the user.
                SystemMessage(message)
            End If

            If message = "Abort->" Then
                Try
                    connectionStore.SendingFile = False
                    ReDim connectionStore.SystemBytesToBeSent(blockSize - 1)
                    SendExternalSystemMessage("->Aborted.", connectionStore)
                    SystemMessage("->Aborted.")
                    connectionStore.fileReader.Close()
                Catch ex As Exception
                End Try
            End If
        ElseIf channel = 253 Then ' File transfer from server to client

        ElseIf channel = 252 Then ' File transfer from client to server
            Try
                If connectionStore.ReceivingFile Then
                    HandleIncomingFileBytes(bytes, connectionStore)
                    connectionStore.fileBytesRecieved += bytes.Length
                End If
            Catch ex As Exception
            End Try
        ElseIf channel = 251 Then ' reserved.

        End If
    End Sub
    '
    ' handle outgong messages
    '
    Private Function HandleOutgoingInternalSystemMessage(ByVal Stream As NetworkStream, _
                                                         ByVal connectionStore As connectionStoreClass) As Boolean
        Dim tmp(1) As Byte
        Dim _size As UShort
        'Static OurTurn As Boolean = False
        HandleOutgoingInternalSystemMessage = False

        ' Create a one time outgoing system message to syncronize packet size.
        If Not connectionStore.sendPacketSize Then
            SendExternalSystemMessage("blocksize:" & blockSize.ToString, connectionStore)
            connectionStore.sendPacketSize = True
        End If

        GetMoreFileBytesIfAvailable(connectionStore)

        ' Handle outgoing system stuff here
        If connectionStore.SystemBytesToBeSentAvailable = True Then
            HandleOutgoingInternalSystemMessage = True
            If connectionStore.SystemBytesToBeSent.Length > blockSize Then
                ' Send Channel
                tmp(0) = connectionStore.SystemOutputChannel
                Stream.Write(tmp, 0, 1)

                ' Send packet size
                _size = blockSize
                tmp = BitConverter.GetBytes(_size)
                Stream.Write(tmp, 0, 2)

                ' Send packet
                Stream.Write(GetSome(connectionStore.SystemBytesToBeSent, blockSize, connectionStore.SystemBytesToBeSentAvailable, connectionStore), 0, _size)
                connectionStore.bytesSentThisSecond += 3 + blockSize
            Else
                ' Send Channel
                tmp(0) = connectionStore.SystemOutputChannel
                Stream.Write(tmp, 0, 1)

                ' Send packet size
                _size = Convert.ToUInt16(connectionStore.SystemBytesToBeSent.Length)
                tmp = BitConverter.GetBytes(_size)
                Stream.Write(tmp, 0, 2)

                ' Send packet
                Stream.Write(connectionStore.SystemBytesToBeSent, 0, _size)
                connectionStore.bytesSentThisSecond += 3 + _size
                connectionStore.SystemBytesToBeSentAvailable = False
            End If
        End If

    End Function
    '
    ' handle outgoing data
    '
    Private Function HandleOutgoingUserData(ByVal Stream As NetworkStream, ByVal connectionStore As connectionStoreClass) As Boolean
        Dim tmp(1) As Byte
        Dim _size As UShort
        Dim notify As Boolean = False
        Static packet(0) As Byte

        If connectionStore.UserBytesToBeSentAvailable = True Then
            SyncLock connectionStore.UserBytesToBeSent
                Try
                    If (connectionStore.UserBytesToBeSent.Length - connectionStore.UserBytesToBeSent.Position) > blockSize Then
                        ' Send Channel
                        tmp(0) = connectionStore.UserOutputChannel
                        Stream.Write(tmp, 0, 1)

                        ' Send packet size
                        _size = blockSize
                        tmp = BitConverter.GetBytes(_size)
                        Stream.Write(tmp, 0, 2)

                        ' Send packet
                        If packet.Length <> _size Then ReDim packet(_size - 1)
                        connectionStore.UserBytesToBeSent.Read(packet, 0, _size)
                        'connectionStore.client.NoDelay = True
                        Stream.Write(packet, 0, _size)
                        connectionStore.bytesSentThisSecond += 3 + _size

                        ' Check to see if we've sent it all...
                        If connectionStore.UserBytesToBeSent.Length = connectionStore.UserBytesToBeSent.Position Then
                            connectionStore.UserBytesToBeSentAvailable = False
                            notify = True
                        End If
                    Else
                        ' Send Channel
                        tmp(0) = connectionStore.UserOutputChannel
                        Stream.Write(tmp, 0, 1)

                        ' Send packet size
                        _size = Convert.ToUInt16(connectionStore.UserBytesToBeSent.Length - connectionStore.UserBytesToBeSent.Position)
                        tmp = BitConverter.GetBytes(_size)
                        Stream.Write(tmp, 0, 2)

                        ' Send packet
                        If packet.Length <> _size Then ReDim packet(_size - 1)
                        connectionStore.UserBytesToBeSent.Read(packet, 0, _size)
                        'connectionStore.Client.NoDelay = True
                        Stream.Write(packet, 0, _size)
                        connectionStore.bytesSentThisSecond += 3 + _size

                        connectionStore.UserBytesToBeSentAvailable = False
                        notify = True
                    End If
                Catch ex As Exception
                    ' Report error attempting to send user data.
                    Debug.WriteLine("Unexpected error in TcpCommServer\HandleOutgoingUserData: " & ex.Message)
                End Try
            End SyncLock

            ' Notify the user that the packet has been sent.
            If notify Then SystemMessage("UBS:" & connectionStore.connectionID & ":" & connectionStore.UserOutputChannel)

            Return True
        Else
            Return False
        End If

        'If connectionStore.UserBytesToBeSentAvailable = True Then
        '    If (connectionStore.UserBytesToBeSent.Length - connectionStore.UserBytesToBeSent.Position) > blockSize Then
        '        ' Send Channel
        '        tmp(0) = connectionStore.UserOutputChannel
        '        Stream.Write(tmp, 0, 1)

        '        ' Send packet size
        '        _size = blockSize
        '        tmp = BitConverter.GetBytes(_size)
        '        Stream.Write(tmp, 0, 2)

        '        ' Send packet
        '        If packet.Length <> _size Then ReDim packet(_size - 1)
        '        connectionStore.UserBytesToBeSent.Read(packet, 0, _size)
        '        Stream.Write(packet, 0, _size)
        '        connectionStore.bytesSentThisSecond += 3 + _size

        '        ' Check to see if we've sent it all...
        '        If connectionStore.UserBytesToBeSent.Length = connectionStore.UserBytesToBeSent.Position Then
        '            connectionStore.UserBytesToBeSentAvailable = False
        '            SystemMessage("UBS:" & connectionStore.connectionId & ":" & connectionStore.UserOutputChannel)
        '        End If
        '    Else
        '        ' Send Channel
        '        tmp(0) = connectionStore.UserOutputChannel
        '        Stream.Write(tmp, 0, 1)

        '        ' Send packet size
        '        _size = (connectionStore.UserBytesToBeSent.Length - connectionStore.UserBytesToBeSent.Position)
        '        tmp = BitConverter.GetBytes(_size)
        '        Stream.Write(tmp, 0, 2)

        '        ' Send packet
        '        If packet.Length <> _size Then ReDim packet(_size - 1)
        '        connectionStore.UserBytesToBeSent.Read(packet, 0, _size)
        '        Stream.Write(packet, 0, _size)
        '        connectionStore.bytesSentThisSecond += 3 + _size

        '        connectionStore.UserBytesToBeSentAvailable = False
        '        SystemMessage("UBS:" & connectionStore.connectionId & ":" & connectionStore.UserOutputChannel)
        '    End If

        '    Return True
        'Else
        '    Return False
        'End If

    End Function
    '
    ' not sure - getSome
    '
    Private Function GetSome(ByRef bytes() As Byte, ByVal chunkToBreakOff As Integer, _
                             ByRef bytesToBeSentAvailable As Boolean, ByVal connectionStore As connectionStoreClass, _
                             Optional ByVal theseAreUserBytes As Boolean = False) As Byte()

        Dim tmp(chunkToBreakOff - 1) As Byte
        Array.Copy(bytes, 0, tmp, 0, chunkToBreakOff)
        GetSome = tmp

        If bytes.Length = chunkToBreakOff Then
            bytesToBeSentAvailable = False
            If theseAreUserBytes Then SystemMessage("UBS:" & connectionStore.connectionID & ":" & connectionStore.UserOutputChannel)
        Else
            Dim tmp2(bytes.Length - chunkToBreakOff - 1) As Byte
            Array.Copy(bytes, chunkToBreakOff, tmp2, 0, bytes.Length - chunkToBreakOff)
            bytes = tmp2
        End If

    End Function
    '
    ' not sure - looks like it just wraps rcvBytes
    '
    Private Sub SystemMessage(ByVal MsgText As String)
        RcvBytes(StrToByteArray(MsgText), -1, 255)
    End Sub
    '
    ' isServerStopping - Check to see if our app is closing (set in FormClosing event)
    '
    Private Function isServerStopping(ByVal Server As TcpClient, ByVal connectionStore As connectionStoreClass) As Boolean

        Try
            If Not allowConnectionsToContinue Or connectionStore.disConnect Then
                isServerStopping = True
            Else
                isServerStopping = False
            End If
        Catch ex As Exception
            ' An unexpected error.
            Debug.WriteLine("Unexpected error in server\theServerIsStopping: " & ex.Message)
        End Try

    End Function
    '
    ' start the listener
    '
    Private Sub listenerThread_startListening()
        SystemMessage("Listening...")
        Listener = New TcpListener(localIpAddress, Port)
        Listener.Start()
        enableNextConnectionAttempt()
    End Sub
    '
    ' initiate the listener to allow another connection
    '
    Private Function enableNextConnectionAttempt() As Boolean
        Try
            Listener.BeginAcceptTcpClient(AddressOf handleConnectionAttempt, Listener)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function
    '
    ' handle the connection attempt that a client started
    '
    Private Sub handleConnectionAttempt(ByVal asyncResult As IAsyncResult)
        Static connectionId As Int32 = 0
        Dim tcpClient As TcpClient
        Dim connectionThread As Thread
        '
        ' enable the next connection attempt, also testing status of listent
        '
        If Not enableNextConnectionAttempt() Then
            '
            ' connections closed, stop this attempt
            '
        Else
            '
            ' connections are still allowed
            '
            connectionId += 1
            If connectionId > 2000000000 Then
                '
                ' 2 billion connections before the ID cycles
                '
                connectionId = 1
            End If
            tcpClient = Listener.EndAcceptTcpClient(asyncResult)
            connectionStoreCollection.Insert(0, New connectionStoreClass(tcpClient, connectionId))
            SystemMessage("Connected.")
            '
            'ThreadPool.QueueUserWorkItem(New WaitCallback(AddressOf Run), connectionStoreCollection.Item(0))
            '
            connectionThread = New Thread(AddressOf connectionThreadMethod)
            connectionThread.IsBackground = True
            connectionThread.Name = "Connection #" & connectionId
            connectionThread.Start(connectionStoreCollection.Item(0))
        End If
    End Sub
    '
    ' dont know
    '
    Private Sub WrapUpIncomingFile(ByVal connectionStore As connectionStoreClass)

        If connectionStore.ReceivingFile Then
            Try
                connectionStore.fileWriter.Close()
                connectionStore.fileWriter = Nothing
                GC.GetTotalMemory(True)
            Catch ex As Exception
            End Try

            Try
                File.Delete(connectionStore.ReceivedFilesFolder & "\" & connectionStore.IncomingFileName)
            Catch ex As Exception
            End Try
        End If
    End Sub
    '
    ' Execute this code when the connection thread is created.
    '
    Private Sub connectionThreadMethod(ByVal _connectionStore As Object)
        Dim connectionStore As connectionStoreClass = DirectCast(_connectionStore, connectionStoreClass)
        Dim tcpClient As TcpClient
        Dim Stream As NetworkStream
        Dim IpEndPoint As IPEndPoint
        Dim puck(1) As Byte : puck(0) = 0
        Dim theBuffer(blockSize - 1) As Byte
        Dim tmp(1) As Byte
        Dim dataChannel As Integer = 0
        Dim packetSize As UShort = 0
        Dim idleTimer, bandwidthTimer As Date
        Dim bytesread As Integer = 0
        Dim weHaveThePuck As Boolean = True
        Dim bandwidthUsedThisSecond As Int32 = 0
        Dim userOrSystemSwitcher As Integer = 0
        '
        Try
            '
            ' Create a local tcpClient object and Stream object for clarity.
            '
            tcpClient = connectionStore.client
            Stream = tcpClient.GetStream()
        Catch ex As Exception
            '
            ' An unexpected error.
            '
            Debug.WriteLine("Could not create local tcpClient or Stream object in tcpClient. Message: " & ex.Message)
            Exit Sub
        End Try
        Try
            '
            ' Get the remote machine's IP address.
            '
            IpEndPoint = CType(tcpClient.Client.RemoteEndPoint, Net.IPEndPoint)
            connectionStore.remoteIpAddress = IpEndPoint.Address
            '
            ' Set the send and receive buffers to the maximum size allowable in this application.
            '
            tcpClient.Client.ReceiveBufferSize = 65535
            tcpClient.Client.SendBufferSize = 65535
            '
            ' no delay on partially filled packets Send it all as fast as possible.
            '
            tcpClient.NoDelay = True
            '
            ' Set the timers...
            '
            idleTimer = Now
            bandwidthTimer = Now
            connectionStore.IsRunning = True
            '
            ' Start the communication loop
            '
            Do
                '
                ' Check to see if our app is shutting down.
                '
                If isServerStopping(tcpClient, connectionStore) Then
                    Exit Do
                End If
                '
                ' Throttle network Mbps...
                '
                bandwidthUsedThisSecond = connectionStore.bytesSentThisSecond + connectionStore.bytesRecievedThisSecond
                If bandwidthTimer.AddMilliseconds(250) >= Now And bandwidthUsedThisSecond >= (Mbps / 4) Then
                    While bandwidthTimer.AddMilliseconds(250) > Now
                        Thread.Sleep(1)
                    End While
                End If
                If bandwidthTimer.AddMilliseconds(250) <= Now Then
                    bandwidthTimer = Now
                    connectionStore.bytesRecievedThisSecond = 0
                    connectionStore.bytesSentThisSecond = 0
                    bandwidthUsedThisSecond = 0
                End If
                '
                ' Normal communications...
                '
                If weHaveThePuck Then
                    '
                    ' Send data if there is any to be sent...
                    '
                    userOrSystemSwitcher += 1
                    Select Case userOrSystemSwitcher
                        Case 1
                            If HandleOutgoingUserData(Stream, connectionStore) Then idleTimer = Now
                        Case 2
                            If HandleOutgoingInternalSystemMessage(Stream, connectionStore) Then idleTimer = Now
                    End Select
                    If userOrSystemSwitcher > 1 Then userOrSystemSwitcher = 0

                    ' After sending out data, send the puck
                    Stream.Write(puck, 0, 1)
                    weHaveThePuck = False
                End If

                If theBuffer.Length < 2 Then
                    ReDim theBuffer(1)
                End If
                '
                ' Read in the control byte.
                '
                Stream.Read(theBuffer, 0, 1)
                dataChannel = theBuffer(0)
                '
                ' If it's just the puck (communictaion syncronization byte),
                ' set weHaveThePuck true, record the byte read for throttling,
                ' and that's all. dataChannel 0 is reserved for the puck.
                '
                If dataChannel = 0 Then
                    weHaveThePuck = True
                    connectionStore.bytesRecievedThisSecond += 1
                Else
                    '
                    ' It's not the puck: It's an incoming packet. Get the packet size:
                    '
                    tmp(0) = Convert.ToByte(Stream.ReadByte)
                    tmp(1) = Convert.ToByte(Stream.ReadByte)
                    packetSize = BitConverter.ToUInt16(tmp, 0)
                    connectionStore.bytesRecievedThisSecond += 2
                    '
                    ' Get the packet:
                    '
                    If theBuffer.Length <> packetSize Then
                        ReDim theBuffer(packetSize - 1)
                    End If
                    Do
                        ' Check to see if we're stopping...
                        If isServerStopping(tcpClient, connectionStore) Then Exit Do
                        ' Read bytes in...
                        bytesread += Stream.Read(theBuffer, bytesread, (packetSize - bytesread))
                    Loop While bytesread < packetSize
                    bytesread = 0

                    ' Record bytes read for throttling...
                    connectionStore.bytesRecievedThisSecond += packetSize

                    ' Handle the packet...
                    If dataChannel > 250 Then
                        ' this is an internal system packet
                        If Not isServerStopping(tcpClient, connectionStore) Then HandleIncomingSystemMessages(theBuffer, dataChannel, connectionStore)
                    Else
                        ' Hand user data off to the calling thread.
                        If Not isServerStopping(tcpClient, connectionStore) Then RcvBytes(theBuffer, connectionStore.connectionID, dataChannel)
                    End If

                    idleTimer = Now
                End If
                '
                ' Throttle CPU usage when idle.
                '
                If Now > idleTimer.AddMilliseconds(500) Then
                    Thread.Sleep(50)
                End If
            Loop
        Catch ex As Exception
            '
            ' An unexpected error in connection loop
            '
            Debug.WriteLine("Unexpected error in server: " & ex.Message)
        End Try

        Try
            connectionStore.fileReader.Close()
        Catch ex As Exception
        End Try

        Try
            tcpClient.Client.Close()
            tcpClient.Client.Blocking = False
        Catch ex As Exception
        End Try

        ' If we're in the middle of receiving a file,
        ' close the filestream, release the memory and
        ' delete the partial file.
        WrapUpIncomingFile(connectionStore)

        connectionStore.IsRunning = False
        SystemMessage("Connection " & connectionStore.connectionID.ToString & " Stopped.")
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class