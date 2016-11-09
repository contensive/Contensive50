Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPContextClass.ClassId, CPContextClass.InterfaceId, CPContextClass.EventsId)> _
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
        '
        'Friend globalResponseRedirect As String             ' if not empty, this is the URL to which the parent page needs to redirect
        'Friend globalResponseBuffer As String               ' content added to the end of the content after Contensive call
        'Friend globalResponseContentType As String          ' content type set on parent page after Contensive Call
        'Friend globalResponseCookies As String              ' cookies set on parent page after Contensive Call
        'Friend globalResponseHeaders As String              ' 
        'Friend globalResponseStatus As String               ' 
        '
        Private localallowProfileLog As Boolean = False
        '
        '
        ' Constructor - Initialize the Main and Csv objects
        '
        Friend Sub New(ByRef cpParent As CPClass)
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
                Return cp.core.allowDebugLog
            End Get
            Set(ByVal value As Boolean)
                cp.core.allowDebugLog = value
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
                Return cp.core.requestPathPage
            End Get
            Set(ByVal value As String)
                cp.core.requestPathPage = value
            End Set
        End Property
        '
        '
        '
        Public Property referrer() As String
            Get
                Return cp.core.requestReferrer
            End Get
            Set(ByVal value As String)
                cp.core.requestReferrer = value
            End Set
        End Property
        '
        '
        '
        Public Property domain() As String
            Get
                Return cp.core.requestDomain
            End Get
            Set(ByVal value As String)
                cp.core.requestDomain = value
            End Set
        End Property
        '
        '
        '
        Public Property queryString() As String
            Get
                Return cp.core.requestQueryString
            End Get
            Set(ByVal value As String)
                cp.core.requestQueryString = value
            End Set
        End Property
        '
        '
        '
        Public Property isSecure() As Boolean
            Get
                Return cp.core.requestSecure
            End Get
            Set(ByVal value As Boolean)
                cp.core.requestSecure = value
            End Set
        End Property
        '
        '
        '
        Public Property remoteIp() As String
            Get
                Return cp.core.requestRemoteIP
            End Get
            Set(ByVal value As String)
                cp.core.requestRemoteIP = value
            End Set
        End Property
        '
        '
        '
        Public Property browserUserAgent() As String
            Get
                Return cp.core.requestBrowser
            End Get
            Set(ByVal value As String)
                cp.core.requestBrowser = value
            End Set
        End Property
        '
        '
        '
        Public Property acceptLanguage() As String
            Get
                Return cp.core.requestLanguage
            End Get
            Set(ByVal value As String)
                cp.core.requestLanguage = value
            End Set
        End Property
        '
        '
        '
        Public Property accept() As String
            Get
                Return cp.core.requestHttpAccept
            End Get
            Set(ByVal value As String)
                cp.core.requestHttpAccept = value
            End Set
        End Property
        '
        '
        '
        Public Property acceptCharSet() As String
            Get
                Return cp.core.requestHttpAcceptCharset
            End Get
            Set(ByVal value As String)
                cp.core.requestHttpAcceptCharset = value
            End Set
        End Property
        '
        '
        '
        Public Property profileUrl() As String
            Get
                Return cp.core.requestHttpProfile
            End Get
            Set(ByVal value As String)
                cp.core.requestHttpProfile = value
            End Set
        End Property
        '
        '
        '
        Public Property xWapProfile() As String
            Get
                Return cp.core.requestxWapProfile
            End Get
            Set(ByVal value As String)
                cp.core.requestxWapProfile = value
            End Set
        End Property
        '
        '
        '
        Public Property isBinaryRequest() As Boolean
            Get
                Return cp.core.requestFormUseBinaryHeader
            End Get
            Set(ByVal value As Boolean)
                cp.core.requestFormUseBinaryHeader = value
            End Set
        End Property
        '
        '
        '
        Public Property binaryRequest() As Object
            Get
                Return cp.core.requestFormBinaryHeader
            End Get
            Set(ByVal value As Object)
                cp.core.requestFormBinaryHeader = value
            End Set
        End Property
        '
        '
        '
        Public Property cookies() As String
            Get
                Return cp.core.requestCookies
            End Get
            Set(ByVal value As String)
                cp.core.requestCookies = value
            End Set
        End Property
        '
        '
        '
        Public Property form() As String
            Get
                Return cp.core.requestForm
            End Get
            Set(ByVal value As String)
                cp.core.requestForm = value
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
                Return cp.core.requestFormFiles
            End Get
            Set(ByVal value As String)
                cp.core.requestFormFiles = value
            End Set
        End Property
        '
        '
        '
        Public Property requestNameSpaceAsUnderscore() As Boolean
            Get
                Return cp.core.requestSpaceAsUnderscore
            End Get
            Set(ByVal value As Boolean)
                cp.core.requestSpaceAsUnderscore = value
            End Set
        End Property
        '
        '
        '
        Public Property requestNameDotAsUnderscore() As Boolean
            Get
                Return cp.core.requestDotAsUnderscore
            End Get
            Set(ByVal value As Boolean)
                cp.core.requestDotAsUnderscore = value
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