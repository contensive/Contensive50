
'Option Explicit On
'Option Strict On

Imports System
Imports System.Diagnostics
Imports System.Net
Imports System.Net.Sockets
Imports System.Text
Imports System.Threading
Imports System.Web
Imports Microsoft.VisualBasic
Imports System.Runtime.InteropServices

Namespace Contensive.Core.Controllers
    '
    Public Class ipDaemonController
        '
        Private cmdCallbackObject As Object
        Private cmdListenPort As Integer
        Private ServerLicense As String
        Private LocalIPList As String
        '
        ' http listener thread and communication object
        '
        Private cmdListenThread As Thread
        Private Const onThread As Boolean = True
        '
        '===============================================================================================
        '   thread method for http Listener
        '       decided on http because Contensive 4 used http, and it is easy to write clients
        '       listens for commands on cmdPort
        '
        ' short term fix -- calling object provides a call back routine ipDaemonCallBack(cmd, queryString, remoteIP)
        '   must be one thread because it calls back into vb6
        '
        '   eventual solution -- the listener goes in the server, and creates an object to call into.
        '
        '===============================================================================================
        '
        Private Sub thread_cmdListener()
            '
            Dim cmd As String
            Dim queryString As String
            Dim remoteIP As String
            Dim prefixes() As String
            Dim prefixesCnt As Integer = 0
            Dim ipHostInfo As IPHostEntry = Dns.GetHostEntry(Dns.GetHostName())
            Dim ipAddressInfo As IPAddress = Nothing
            Dim HostEntry As IPHostEntry = Dns.GetHostEntry(System.Net.Dns.GetHostName)
            Dim ptr As Integer
            Dim context As HttpListenerContext
            Dim request As HttpListenerRequest
            Dim response As HttpListenerResponse
            Dim responseString As String
            Dim buffer() As Byte
            Dim output As System.IO.Stream
            Dim Hint As String = "enter"
            Dim hintPrefixes As String = ""
            '
            Try
                '
                If Not HttpListener.IsSupported Then
                    Throw New ArgumentException("This operating system does not support the required http listen methods")
                Else
                    Dim cmdListener As HttpListener
                    '
                    ReDim prefixes(prefixesCnt)
                    prefixes(prefixesCnt) = "http://127.0.0.1:" & cmdListenPort & "/"
                    prefixesCnt += 1
                    '
                    Hint &= ",building prefixes"
                    For Each ipAddressInfo In HostEntry.AddressList
                        If ipAddressInfo.AddressFamily = AddressFamily.InterNetwork Then
                            If ipAddressInfo.ToString <> "127.0.0.1" Then
                                ReDim Preserve prefixes(prefixesCnt)
                                prefixes(prefixesCnt) = "http://" & ipAddressInfo.ToString & ":" & cmdListenPort & "/"
                                prefixesCnt += 1
                            End If
                        End If
                    Next
                    If prefixesCnt = 0 Then
                        Throw New ArgumentException("No ip addresses are available")
                    Else
                        '
                        ' Create a listener.
                        '
                        Hint &= ",create listener"
                        cmdListener = New HttpListener()
                        For ptr = 0 To prefixesCnt - 1
                            cmdListener.Prefixes.Add(prefixes(ptr))
                            hintPrefixes &= "," & prefixes(ptr)
                        Next
                        Hint &= ",start"
                        cmdListener.Start()
                        Do
                            'hint = "do,get request response obj"
                            context = cmdListener.GetContext()
                            request = context.Request
                            response = context.Response
                            cmd = request.Url.LocalPath
                            queryString = request.Url.Query
                            Hint &= ",cmd=[" & cmd & "],querystring=[" & queryString & "]"
                            If queryString.Length > 0 Then
                                If queryString.Substring(0, 1) = "?" Then
                                    queryString = queryString.Substring(1)
                                End If
                            End If
                            remoteIP = request.RemoteEndPoint.Address.ToString
                            Hint &= ",remoteIP=[" & remoteIP & "]"
                            Try
                                Hint &= ",callback enter"
                                responseString = cmdCallbackObject.ipDaemonCallback(cmd, queryString, remoteIP)
                                Hint &= ",callback exit"
                            Catch ex As Exception
                                '
                                ' should never return an error to the iDaemon
                                '
                                Call My.Computer.FileSystem.WriteAllText("C:\clibIpDaemonDebug.log", vbCrLf & Now.ToString() & " " & "Exception in callback, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
                                responseString = ""
                            End Try
                            Hint &= ",set buffer from responseString"
                            If responseString.Length <= 0 Then
                                buffer = System.Text.Encoding.Unicode.GetBytes("")
                            Else
                                buffer = System.Text.Encoding.UTF8.GetBytes(responseString)
                            End If
                            Hint &= ",write output from butter"
                            response.ContentLength64 = buffer.Length
                            response.ContentType = "text/HTML"
                            output = response.OutputStream
                            output.Write(buffer, 0, buffer.Length)
                            output.Close()
                        Loop
                        cmdListener.Stop()
                        cmdListener.Abort()
                        'cmdListener = Nothing
                    End If
                End If
            Catch ex As HttpListenerException
                '
                '
                '
                Call My.Computer.FileSystem.WriteAllText("C:\clibIpDaemonDebug.log", vbCrLf & Now.ToString() & " " & "HttpListenerException, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
                'Throw
            Catch ex As Exception
                '
                '
                '
                Call My.Computer.FileSystem.WriteAllText("C:\clibIpDaemonDebug.log", vbCrLf & Now.ToString() & " " & "Exception, hintPrefixes=[" & hintPrefixes & "], hint=[" & Hint & "], ex=[" & ex.Message & "/" & ex.StackTrace & "]", True)
                'Throw
            End Try
        End Sub
        '
        '==========================================================================================
        '   Stop listening
        '==========================================================================================
        '
        Public Sub startListening(ByVal callbackObject As Object, ByVal listenPort As Integer)
            Try
                '
                '
                '
                cmdListenPort = listenPort
                cmdCallbackObject = callbackObject
                If Not onThread Then
                    '
                    ' start on this thread and block
                    '
                    Call thread_cmdListener()
                Else
                    '
                    ' start on a new thread and return
                    '
                    cmdListenThread = New Thread(AddressOf thread_cmdListener)
                    cmdListenThread.Name = "cmdListen"
                    cmdListenThread.IsBackground = True
                    cmdListenThread.Start()
                End If
            Catch ex As Exception
                '
                '
                '
                Throw New ApplicationException("Error during ipDaemon.startListening")
            End Try
        End Sub
        '
        '==========================================================================================
        '   Stop listening
        '==========================================================================================
        '
        Public Sub stopListening()
            Try
                '
                ' abort sockets
                '
                If Not onThread Then
                    '
                    '
                    '
                ElseIf Not (cmdListenThread Is Nothing) Then
                    '
                    '
                    '
                    cmdListenThread.Abort()
                End If
            Catch ex As Exception
                '
                '
                '
                Throw New ApplicationException("Error during ipDaemon.stopListening")
            End Try
        End Sub
    End Class
End Namespace