
'Imports System
'Imports System.Diagnostics
'Imports System.Net
'Imports System.Net.Sockets
'Imports System.Text
'Imports System.Threading
'Imports System.Web
''Imports Microsoft.VisualBasic
'Imports System.IO
'Imports System.Runtime.InteropServices
''Imports Contensive.BaseClasses

'Namespace Contensive.Core
'    '<ComVisible(True)> _
'    '<ProgId("runAtServerClass")> _
'    '<ComClass(runAtServerClass.ClassId, runAtServerClass.InterfaceId, runAtServerClass.EventsId)> _
'    Public Class runAtServerClass
'        '
'        '#Region "COM GUIDs"
'        '        ' These  GUIDs provide the COM identity for this class 
'        '        ' and its COM interfaces. If you change them, existing 
'        '        ' clients will no longer be able to access the class.
'        '        Public Const ClassId As String = "52F31E20-93BC-4E02-82D6-195B616678F1"
'        '        Public Const InterfaceId As String = "487EC25A-D40D-47D7-8624-A829B83153BD"
'        '        Public Const EventsId As String = "2DE2594C-2A7D-4BF7-A99B-454B7D8E9C2B"
'        '#End Region
'        '
'        '
'        '
'        Private cpCore As cpCoreClass
'        Private _IPAddress As String
'        Private _port As Integer
'        Private _username As String
'        Private _password As String
'        Private _userAgent As String
'        Private _timeout As Integer
'        '
'        '====================================================================================================
'        ''' <summary>
'        ''' constructor
'        ''' </summary>
'        ''' <param name="cpCore"></param>
'        ''' <remarks></remarks>
'        Public Sub New(cpCore As cpCoreClass)
'            Me.cpCore = cpCore
'            _IPAddress = "127.0.0.1"
'            _port = "4531"
'            _username = ""
'            _password = ""
'            _timeout = 30
'        End Sub
'        '
'        Public Property timeout As String
'            Get
'                Return _timeout
'            End Get
'            Set(ByVal value As String)
'                _timeout = value
'            End Set
'        End Property
'        '
'        Public Property userAgent As String
'            Get
'                Return _userAgent
'            End Get
'            Set(ByVal value As String)
'                _userAgent = value
'            End Set
'        End Property
'        '
'        Public Property password As String
'            Get
'                Return _password
'            End Get
'            Set(ByVal value As String)
'                _password = value
'            End Set
'        End Property
'        '
'        Public Property username As String
'            Get
'                Return _username
'            End Get
'            Set(ByVal value As String)
'                _username = value
'            End Set
'        End Property
'        '
'        Public Property ipAddress As String
'            Get
'                Return _IPAddress
'            End Get
'            Set(ByVal value As String)
'                _IPAddress = value
'            End Set
'        End Property
'        '
'        Public Property port As String
'            Get
'                Return _port
'            End Get
'            Set(ByVal value As String)
'                _port = value
'            End Set
'        End Property
'        '
'        '
'        '
'        Public Function executeCmd(ByVal cmd As String, Optional ByVal querystring As String = "") As String
'            Dim returnResult As String = "No Reply From Server"
'            Try
'                Const bufferSize As Integer = 65535
'                Dim client As New TcpClient
'                Dim serverEndPoint As New IPEndPoint(System.Net.IPAddress.Parse(_IPAddress), _port)
'                Dim clientStream As NetworkStream
'                Dim encoder As New ASCIIEncoding
'                Dim buffer() As Byte
'                Dim query As String = ""
'                Dim bytesRead As Integer
'                '
'                appendTcpLog("executeCmd, enter, cmd=[" & cmd & "], querystring=[" & querystring & "]")
'                '


'                client.Connect(serverEndPoint)
'                clientStream = client.GetStream()
'                If querystring <> "" Then
'                    query = "&" & querystring
'                End If
'                If _username <> "" Then
'                    query &= "&un=" & _username
'                End If
'                If _password <> "" Then
'                    query &= "&un=" & _password
'                End If
'                If query <> "" Then
'                    query = query.Substring(1)
'                End If
'                query = cmd & "?" & query
'                '
'                appendTcpLog("executeCmd, query=[" & query & "]")
'                '
'                buffer = encoder.GetBytes(query)
'                Call clientStream.Write(buffer, 0, buffer.Length)
'                Call clientStream.Flush()
'                '
'                returnResult = ""
'                ReDim buffer(bufferSize)
'                bytesRead = clientStream.Read(buffer, 0, bufferSize)
'                '
'                appendTcpLog("executeCmd, bytesRead=[" & bytesRead & "]")
'                '
'                If bytesRead > 0 Then
'                    ReDim Preserve buffer(bytesRead - 1)
'                    returnResult &= BytesToString(buffer)
'                End If
'                '
'                appendTcpLog("executeCmd, returnResult=[" & returnResult & "]")
'                '
'            Catch ex As Exception
'                '
'                '
'                '
'                Throw New connectException("Error in executeCmd(" & cmd & "," & querystring & ")", ex)
'            End Try
'            Return returnResult
'        End Function

'        Private Function StrToByteArray(ByVal text As String) As Byte()
'            Dim encoding As New System.Text.UTF8Encoding()
'            StrToByteArray = encoding.GetBytes(text)
'        End Function

'        Private Function BytesToString(ByVal data() As Byte) As String
'            Dim enc As New System.Text.UTF8Encoding()
'            BytesToString = enc.GetString(data)
'        End Function
'        '
'        ' exception classes
'        '
'        Public Class connectException
'            Inherits ApplicationException
'            '
'            Public Sub New(ByVal context As String, ByVal innerEx As Exception)
'                MyBase.New("Unknown error in runAtServerClass, " & context & ", innerException [" & innerEx.ToString() & "]")
'            End Sub
'        End Class
'        '
'        ' append log
'        '
'        Private Sub appendTcpLog(ByVal logText As String)
'            If False Then
'                Dim retryCnt As Integer = 0
'                Dim success As Boolean = False
'                Do While retryCnt < 5 And Not success
'                    Try
'                        Using outfile As New StreamWriter("c:\clibTcpDebug.log", True)
'                            outfile.Write(vbCrLf & Now.ToString() & " " & logText)
'                        End Using
'                        success = True
'                    Catch ex As Exception
'                        Call Thread.Sleep(10)
'                    End Try
'                    retryCnt += 1
'                Loop
'            End If
'        End Sub
'    End Class


'End Namespace
