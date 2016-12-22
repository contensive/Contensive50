
Option Explicit On
Option Strict On

Imports System.IO
Imports System.Net
'
#Const DebugBuild = True

Namespace Contensive.Core
    '
    Public Class coreHttpRequestClass
        '
        Private http As webClientExt
        Private privateRequestHeaders As System.Net.WebHeaderCollection
        Private privateRequestPassword As String
        Private privateRequestUsername As String
        Private privateRequestUserAgent As String
        Private privateRequestCookie As String
        Private privateRequestTimeoutMsec As Integer
        '
        Private privateResponseFilename As String
        Private privateResponseProtocol As String = "HTTP/1.1" ' had to fake bc webClient removes first line of header
        Private privateResponseStatusDescription As String
        Private privateResponseStatusCode As Integer
        'Private privateResponseStatus As System.Net.HttpStatusCode = New System.Net.HttpStatusCode
        Private privateResponseHeaders As System.Net.WebHeaderCollection = New System.Net.WebHeaderCollection()
        Private privateResponseLength As Integer = 0
        '
        Private privateSocketResponse As String = ""
        '
        '======================================================================================
        ' constructor
        '======================================================================================
        '
        Public Sub New()
            MyBase.New()
            ' Me.cpcore = cpcore
            privateRequestTimeoutMsec = 30000
            privateRequestUserAgent = "kmaHTTP/" & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor
            http = New webClientExt()
        End Sub
        '
        '======================================================================================
        '   Requests the doc and saves the body in the file specified
        '
        '   check the HTTPResponse and SocketResponse when done
        '   If the HTTPResponse is "", Check the SocketResponse
        '======================================================================================
        '
        Public Sub getUrlToFile(ByRef URL As String, ByRef Filename As String)
            Try
                Dim cookies() As String
                Dim CookiePointer As Integer
                Dim CookiePart() As String
                Dim path As String
                Dim ptr As Integer
                '
                'cpCore.AppendLog( "http4Class.getUrlToFile, url=[" & URL & "], filename=[" & Filename & "]")
                '
                privateResponseFilename = Filename
                path = Filename.Replace("/", "\")
                ptr = path.LastIndexOf("\")
                If ptr > 0 Then
                    path = Filename.Substring(0, ptr)
                    Directory.CreateDirectory(path)
                End If
                Call File.Delete(privateResponseFilename)
                http.password = privateRequestPassword
                http.username = privateRequestUsername
                http.UserAgent = privateRequestUserAgent
                If privateRequestCookie <> "" Then
                    cookies = Split(privateRequestCookie, ";")
                    For CookiePointer = 0 To UBound(cookies)
                        CookiePart = Split(cookies(CookiePointer), "=")
                        Call http.addCookie(CookiePart(0), CookiePart(1))
                    Next
                End If
                '
                privateRequestHeaders = http.Headers()
                http.Timeout = privateRequestTimeoutMsec
                '
                privateRequestHeaders = http.Headers()
                privateResponseHeaders = New System.Net.WebHeaderCollection()
                privateResponseLength = 0
                Try
                    http.DownloadFile(URL, privateResponseFilename)
                    'privateResponseProtocol = ""
                    privateResponseStatusCode = 200
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString()
                    privateResponseHeaders = http.ResponseHeaders
                    privateResponseLength = 0
                    If (File.Exists(privateResponseFilename)) Then
                        privateResponseLength = CInt((New FileInfo(privateResponseFilename)).Length)
                    End If
                    'Catch ex As WebException
                    '    '
                    '    ' http error, not 200
                    '    '
                    '    Dim we As WebException
                    '    Dim response As HttpWebResponse
                    '    we = DirectCast(ex, WebException)
                    '    response = we.Response
                    '    privateResponseStatusCode = response.StatusCode
                    '    privateResponseStatusDescription = response.StatusDescription
                    '    privateResponseHeaders = response.Headers
                    '    privateResponseLength = 0
                Catch ex As Exception
                    '
                    '
                    '
                    privateResponseStatusCode = 0
                    privateResponseStatusDescription = ""
                    privateResponseHeaders = New System.Net.WebHeaderCollection()
                    privateResponseLength = 0
                    Throw
                End Try
                'http.DownloadFile(URL, privateResponseFilename)
                'privateResponseHeaders = http.ResponseHeaders
                'privateResponseLength = 0
                'If (File.Exists(privateResponseFilename)) Then
                '    privateResponseLength = (New FileInfo(privateResponseFilename)).Length
                'End If
            Catch ex As Exception
                '
                '
                '
                'Throw
                Throw New httpException("Error in getUrlToFile(" & URL & "," & Filename & ")", ex)
            End Try
        End Sub
        '
        '======================================================================================
        '   Returns the body of a URL requested
        '
        '   If there is an error, it returns "", and the HTTPResponse should be checked
        '   If the HTTPResponse is "", Check the SocketResponse
        '======================================================================================
        '
        Public Function getURL(ByRef URL As String) As String
            Dim returnString As String = ""
            Try
                Dim cookies() As String
                Dim CookiePointer As Integer
                Dim CookiePart() As String
                '
                'cpCore.AppendLog( "http4Class.getURL, url=[" & URL & "]")
                '
                'Dim TimeoutTime As Date
                '
                http.password = privateRequestPassword
                http.username = privateRequestUsername
                http.UserAgent = privateRequestUserAgent
                If privateRequestCookie <> "" Then
                    cookies = Split(privateRequestCookie, ";")
                    For CookiePointer = 0 To UBound(cookies)
                        CookiePart = Split(cookies(CookiePointer), "=")
                        Call http.addCookie(CookiePart(0), CookiePart(1))
                    Next
                End If
                http.Timeout = privateRequestTimeoutMsec
                'TimeoutTime = System.DateTime.FromOADate(Now.ToOADate + (privateRequestTimeoutMsec / 24 / 60 / 60))
                '
                privateRequestHeaders = http.Headers()
                privateResponseHeaders = New System.Net.WebHeaderCollection()
                privateResponseLength = 0
                privateResponseStatusCode = 0
                privateResponseStatusDescription = ""
                'privateResponseHeaders = response.Headers
                privateResponseLength = 0
                Try
                    returnString = http.DownloadString(URL)
                    'privateResponseProtocol = ""
                    privateResponseStatusCode = 200
                    privateResponseStatusDescription = HttpStatusCode.OK.ToString()
                    privateResponseHeaders = http.ResponseHeaders
                    privateResponseLength = returnString.Length
                    'Catch ex As WebException
                    '    '
                    '    ' http error, not 200
                    '    '
                    '    Dim we As WebException
                    '    Dim response As HttpWebResponse
                    '    we = DirectCast(ex, WebException)
                    '    If Not (we.Response Is Nothing) Then
                    '        response = we.Response
                    '        privateResponseStatusCode = response.StatusCode
                    '        privateResponseStatusDescription = response.StatusDescription
                    '        privateResponseHeaders = response.Headers
                    '        privateResponseLength = CInt(response.ContentLength)
                    '    End If
                Catch ex As Exception
                    '
                    '
                    '
                    Throw
                End Try
            Catch
                '
                ' general catch for the routine
                '
                Throw
            End Try
            '
            'cpCore.AppendLog( "http4Class.getURL exit, return=[" & returnString & "]")
            '
            Return returnString
        End Function
        '
        '================================================================
        '
        '================================================================
        '
        Public Property userAgent() As String
            Get
                Dim returnString As String = ""
                Try
                    returnString = http.UserAgent
                Catch ex As Exception
                    Throw New ApplicationException("Error in UserAgent Property, get Method")
                End Try
                Return returnString
            End Get
            Set(ByVal Value As String)
                Try
                    http.UserAgent = Value
                Catch ex As Exception
                    Throw New ApplicationException("Error in UserAgent Property, set Method")
                End Try
            End Set
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public Property timeout() As Integer
            Get
                Dim returnTimeout As Integer = 0
                Try
                    returnTimeout = CInt(http.Timeout / 1000)
                Catch ex As Exception
                    Throw New ApplicationException("Error in Timeout Property, get Method")
                End Try
                Return returnTimeout
            End Get
            Set(ByVal Value As Integer)
                Try
                    If Value > 65535 Then
                        Value = 65535
                    End If
                    http.Timeout = Value * 1000
                Catch ex As Exception
                    Throw New ApplicationException("Error in Timeout Property, set Method")
                End Try
            End Set
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public ReadOnly Property requestHeader() As String
            Get
                Dim returnString As String = ""
                Dim ptr As Integer
                '
                Try
                    If (privateRequestHeaders.Count > 0) Then
                        For ptr = 0 To privateRequestHeaders.Count - 1
                            returnString &= privateRequestHeaders.Item(ptr)
                        Next
                    End If
                Catch ex As Exception
                    Throw New ApplicationException("Error in requestHeader Property, get Method")
                End Try
                Return returnString
            End Get
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public ReadOnly Property responseHeader() As String
            Get
                Dim returnString As String = ""
                Dim ptr As Integer
                '
                Try
                    If privateResponseStatusCode <> 0 Then
                        returnString &= privateResponseProtocol & " " & privateResponseStatusCode & " " & privateResponseStatusDescription
                        If (privateResponseHeaders.Count > 0) Then
                            For ptr = 0 To privateResponseHeaders.Count - 1
                                returnString &= vbCrLf & privateResponseHeaders.GetKey(ptr) & ":" & privateResponseHeaders.Item(ptr)
                            Next
                        End If
                    End If
                Catch ex As Exception
                    Throw
                    'Throw New ApplicationException("Error in responseHeader Property, get Method")
                End Try
                Return returnString
            End Get
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public ReadOnly Property socketResponse() As String
            Get
                Dim returnString As String = ""
                'Dim ptr As Integer
                '
                Try
                    returnString = privateSocketResponse
                Catch ex As Exception
                    Throw New ApplicationException("Error in SocketResponse Property, get Method")
                End Try
                Return returnString
            End Get
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        'Public ReadOnly Property responseLength() As String
        '    Get
        '        Dim returnString As String = ""
        '        'Dim ptr As Integer
        '        '
        '        Try
        '            returnString = privateResponseLength
        '        Catch ex As Exception
        '            Throw New ApplicationException("Error in ResponseLength Property, get Method")
        '        End Try
        '        Return returnString
        '    End Get
        'End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public ReadOnly Property responseStatusDescription() As String
            Get
                Dim returnString As String = ""
                'Dim ptr As Integer
                '
                Try
                    returnString = privateResponseStatusDescription
                Catch ex As Exception
                    Throw
                End Try
                Return returnString
            End Get
        End Property
        '
        '================================================================
        '
        '================================================================
        '
        Public ReadOnly Property responseStatusCode() As Integer
            Get
                Dim returnCode As Integer = 0
                'Dim ptr As Integer
                '
                Try
                    returnCode = privateResponseStatusCode
                Catch ex As Exception
                    Throw
                End Try
                Return returnCode
            End Get
        End Property
        '
        '
        '
        Public WriteOnly Property setCookie() As String
            Set(ByVal Value As String)
                Try
                    privateRequestCookie = Value
                Catch ex As Exception
                    Throw
                End Try
            End Set
        End Property
        '
        '
        '
        Public WriteOnly Property username() As String
            Set(ByVal Value As String)
                Try
                    privateRequestUsername = Value
                Catch ex As Exception
                    Throw
                End Try
            End Set
        End Property
        '
        '
        '
        Public WriteOnly Property password() As String
            Set(ByVal Value As String)
                Try
                    privateRequestPassword = Value
                Catch ex As Exception
                    Throw
                End Try
            End Set
        End Property
        '
        Protected Overrides Sub Finalize()
            MyBase.Finalize()
            http.Dispose()
        End Sub
        '
        '
        '
    End Class
    '
    ' exception classes
    '
    Public Class httpException
        Inherits ApplicationException
        '
        Public Sub New(ByVal context As String, innerEx As Exception)
            MyBase.New("Unknown error in http4Class, " & context & ", innerException [" & innerEx.ToString() & "]")
        End Sub
    End Class

End Namespace

