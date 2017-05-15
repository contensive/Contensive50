
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPContextClass.ClassId, CPContextClass.InterfaceId, CPContextClass.EventsId)>
    Public Class CPContextClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "9FC3B58E-F6A4-4DEA-BE39-B40B09FBE0B7"
        Public Const InterfaceId As String = "4CD84EB3-175C-4004-8811-8257849F549A"
        Public Const EventsId As String = "8C6AC359-68B4-49A3-A3BC-7A53CA16EA45"
#End Region
        '
        Protected disposed As Boolean = False
        Private cp As CPClass
        Private localallowProfileLog As Boolean = False
        '
        '
        ' Constructor - Initialize the Main and Csv objects
        '
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
                Me.disposed = True
            End If
        End Sub
        '
        '
        Public Property allowDebugLog() As Boolean
            Get
                Return cp.core.debug_allowDebugLog
            End Get
            Set(ByVal value As Boolean)
                cp.core.debug_allowDebugLog = value
            End Set
        End Property
        ''
        '' appName is set as the argument of init( appName ) - not really a context
        ''
        'Public Property appName() As String
        '    Get
        '        Return localappName
        '    End Get
        '    Set(ByVal value As String)
        '        localappName = value
        '    End Set
        'End Property
        '
        '
        '
        Public Property pathPage() As String
            Get
                Return cp.core.webServerIO.requestPathPage
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestPathPage = value
            End Set
        End Property
        '
        '
        '
        Public Property referrer() As String
            Get
                Return cp.core.webServerIO.requestReferrer
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestReferrer = value
            End Set
        End Property
        '
        '
        '
        Public Property domain() As String
            Get
                Return cp.core.webServerIO.requestDomain
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestDomain = value
            End Set
        End Property
        '
        '
        '
        Public Property queryString() As String
            Get
                Return cp.core.webServerIO.requestQueryString
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestQueryString = value
            End Set
        End Property
        '
        '
        '
        Public Property isSecure() As Boolean
            Get
                Return cp.core.webServerIO.requestSecure
            End Get
            Set(ByVal value As Boolean)
                cp.core.webServerIO.requestSecure = value
            End Set
        End Property
        '
        '
        '
        Public Property remoteIp() As String
            Get
                Return cp.core.webServerIO.requestRemoteIP
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestRemoteIP = value
            End Set
        End Property
        '
        '
        '
        Public Property browserUserAgent() As String
            Get
                Return cp.core.webServerIO.requestBrowser
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestBrowser = value
            End Set
        End Property
        '
        '
        '
        Public Property acceptLanguage() As String
            Get
                Return cp.core.webServerIO.RequestLanguage
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.RequestLanguage = value
            End Set
        End Property
        '
        '
        '
        Public Property accept() As String
            Get
                Return cp.core.webServerIO.requestHttpAccept
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestHttpAccept = value
            End Set
        End Property
        '
        '
        '
        Public Property acceptCharSet() As String
            Get
                Return cp.core.webServerIO.requestHttpAcceptCharset
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestHttpAcceptCharset = value
            End Set
        End Property
        '
        '
        '
        Public Property profileUrl() As String
            Get
                Return cp.core.webServerIO.requestHttpProfile
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestHttpProfile = value
            End Set
        End Property
        '
        '
        '
        Public Property xWapProfile() As String
            Get
                Return cp.core.webServerIO.requestxWapProfile
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestxWapProfile = value
            End Set
        End Property
        '
        '
        '
        Public Property isBinaryRequest() As Boolean
            Get
                Return cp.core.webServerIO.requestFormUseBinaryHeader
            End Get
            Set(ByVal value As Boolean)
                cp.core.webServerIO.requestFormUseBinaryHeader = value
            End Set
        End Property
        '
        '
        '
        Public Property binaryRequest() As Object
            Get
                Return cp.core.webServerIO.requestFormBinaryHeader
            End Get
            Set(ByVal value As Object)
                cp.core.webServerIO.requestFormBinaryHeader = value
            End Set
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' get or set request cookies using a name value string similar to a querystring 
        ''' </summary>
        ''' <returns></returns>
        Public Property cookies() As String
            Get
                Dim cookieString As String = ""
                For Each kvp As KeyValuePair(Of String, webServerIOController.cookieClass) In cp.core.webServerIO.requestCookies
                    cookieString &= "&" & kvp.Key & "=" & kvp.Value.value
                Next
                If cookieString.Length > 0 Then
                    cookieString.Substring(1)
                End If
                Return cookieString
            End Get
            Set(ByVal value As String)
                Dim ampSplit As String()
                Dim ampSplitCount As Integer
                If value <> "" Then
                    ampSplit = Split(value, "&")
                    ampSplitCount = UBound(ampSplit) + 1
                    For ampSplitPointer = 0 To ampSplitCount - 1
                        Dim newCookie As New webServerIOController.cookieClass
                        Dim cookieName As String
                        With newCookie
                            Dim NameValue As String = ampSplit(ampSplitPointer)
                            Dim ValuePair As String()
                            ValuePair = Split(NameValue, "=")
                            cookieName = DecodeResponseVariable(CStr(ValuePair(0)))
                            .name = cookieName
                            If UBound(ValuePair) > 0 Then
                                .value = DecodeResponseVariable(CStr(ValuePair(1)))
                            End If
                        End With
                        If cp.core.webServerIO.requestCookies.ContainsKey(cookieName) Then
                            cp.core.webServerIO.requestCookies.Remove(cookieName)
                        End If
                        cp.core.webServerIO.requestCookies.Add(cookieName, newCookie)
                    Next
                End If
            End Set
        End Property
        '
        '
        '
        Public Property form() As String
            Get
                Return cp.core.webServerIO.requestFormString
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requestFormString = value
            End Set
        End Property
        '==========================================================================================
        ''' <summary>
        ''' A URL encoded name=value string that contains the context for uploaded files. Each file contains five nameValues. The names are prefixed with a counter. The format is as follows: 0formname=formname&0filename=filename&0type=fileType&0tmpFile=tempfilename&0error=errors&0size=fileSize
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property formFiles() As String
            Get
                Return cp.core.webServerIO.requesFilesString
            End Get
            Set(ByVal value As String)
                cp.core.webServerIO.requesFilesString = value
            End Set
        End Property
        '
        '
        '
        Public Property requestNameSpaceAsUnderscore() As Boolean
            Get
                Return cp.core.webServerIO.requestSpaceAsUnderscore
            End Get
            Set(ByVal value As Boolean)
                cp.core.webServerIO.requestSpaceAsUnderscore = value
            End Set
        End Property
        '
        '
        '
        Public Property requestNameDotAsUnderscore() As Boolean
            Get
                Return cp.core.webServerIO.requestDotAsUnderscore
            End Get
            Set(ByVal value As Boolean)
                cp.core.webServerIO.requestDotAsUnderscore = value
            End Set
        End Property

        '
        '
        '
        Public ReadOnly Property responseRedirect() As String
            Get
                Return cp.core.responseRedirect
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseBuffer() As String
            Get
                Return cp.core.responseBuffer
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseContentType() As String
            Get
                Return cp.core.responseContentType
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseCookies() As String
            Get
                Return cp.core.responseCookies
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseHeaders() As String
            Get
                Return cp.core.responseHeader
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseStatus() As String
            Get
                Return cp.core.responseStatus
            End Get
        End Property
        '
        '
        ' append to logfile
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.content, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
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