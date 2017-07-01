Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPRequestClass.ClassId, CPRequestClass.InterfaceId, CPRequestClass.EventsId)>
    Public Class CPRequestClass
        Inherits BaseClasses.CPRequestBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "EF7782C1-76E4-45A7-BF30-E1CEBCBC56CF"
        Public Const InterfaceId As String = "39D6A73F-C11A-44F4-8405-A4CE3FB0A486"
        Public Const EventsId As String = "C8938AB2-26F0-41D2-A282-3313FD7BA490"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        ' Constructor
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.coreClass)
            MyBase.New()
            cpCore = cpCoreObj
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
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub

        Public Overrides ReadOnly Property Browser() As String 'Inherits BaseClasses.CPRequestBaseClass.Browser
            Get
                If True Then
                    Return cpCore.webServer.requestBrowser
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsIE() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsIE
            Get
                If True Then
                    Return cpCore.authContext.visit_browserIsIE
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMac() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsMac
            Get
                If True Then
                    Return cpCore.authContext.visit_browserIsMac
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMobile() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsMobile
            Get
                If True Then
                    Return cpCore.authContext.visit.Mobile
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsWindows() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.BrowserIsWindows
            Get
                If True Then
                    Return cpCore.authContext.visit_browserIsWindows
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserVersion() As String 'Inherits BaseClasses.CPRequestBaseClass.BrowserVersion
            Get
                If True Then
                    Return cpCore.authContext.visit_browserVersion
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function Cookie(ByVal CookieName As String) As String 'Inherits BaseClasses.CPRequestBaseClass.Cookie
            If True Then
                Return cpCore.webServer.getRequestCookie(CookieName)
            Else
                Return ""
            End If
        End Function
        '====================================================================================================
        ''' <summary>
        ''' return a string that includes the simple name value pairs for all request cookies
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property CookieString() As String 'Inherits BaseClasses.CPRequestBaseClass.CookieString
            Get
                Dim returnCookies As String = ""
                For Each kvp As KeyValuePair(Of String, webServerController.cookieClass) In cpCore.webServer.requestCookies
                    returnCookies &= "&" & kvp.Key & "=" & kvp.Value.value
                Next
                If returnCookies.Length > 0 Then
                    returnCookies = returnCookies.Substring(1)
                End If
                Return returnCookies
            End Get
        End Property

        Public Overrides ReadOnly Property Form() As String 'Inherits BaseClasses.CPRequestBaseClass.Form
            Get
                If True Then
                    Return cpCore.webServer.requestFormString
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property FormAction() As String 'Inherits BaseClasses.CPRequestBaseClass.FormAction
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_ServerFormActionURL
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function GetBoolean(ByVal RequestName As String) As Boolean 'Inherits BaseClasses.CPRequestBaseClass.GetBoolean
            If True Then
                Return cpCore.docproperties.getBoolean(RequestName)
            Else
                Return False
            End If
        End Function

        Public Overrides Function GetDate(ByVal RequestName As String) As Date 'Inherits BaseClasses.CPRequestBaseClass.GetDate
            If True Then
                Return cpCore.docproperties.getDate(RequestName)
            Else
                Return New Date
            End If
        End Function

        Public Overrides Function GetInteger(ByVal RequestName As String) As Integer 'Inherits BaseClasses.CPRequestBaseClass.GetInteger
            If True Then
                Return cpCore.docProperties.getInteger(RequestName)
            Else
                Return 0
            End If
        End Function

        Public Overrides Function GetNumber(ByVal RequestName As String) As Double 'Inherits BaseClasses.CPRequestBaseClass.GetNumber
            If True Then
                Return cpCore.docProperties.getNumber(RequestName)
            Else
                Return 0
            End If
        End Function

        Public Overrides Function GetText(ByVal RequestName As String) As String 'Inherits BaseClasses.CPRequestBaseClass.GetText
            If True Then
                Return cpCore.docProperties.getText(RequestName)
            Else
                Return ""
            End If
        End Function

        Public Overrides ReadOnly Property Host() As String 'Inherits BaseClasses.CPRequestBaseClass.Host
            Get
                If True Then
                    Return cpCore.webServer.requestDomain
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAccept() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPAccept
            Get
                If True Then
                    Return cpCore.webServer.requestHttpAccept
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAcceptCharset() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPAcceptCharset
            Get
                If True Then
                    Return cpCore.webServer.requestHttpAcceptCharset
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPProfile() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPProfile
            Get
                If True Then
                    Return cpCore.webServer.requestHttpProfile
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPXWapProfile() As String 'Inherits BaseClasses.CPRequestBaseClass.HTTPXWapProfile
            Get
                If True Then
                    Return cpCore.webServer.requestxWapProfile
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Language() As String 'Inherits BaseClasses.CPRequestBaseClass.Language
            Get
                If True Then
                    Return cpcore.authContext.user.language
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Link() As String 'Inherits BaseClasses.CPRequestBaseClass.Link
            Get
                If True Then
                    Return cpCore.webServer.requestUrl
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LinkForwardSource() As String 'Inherits BaseClasses.CPRequestBaseClass.LinkForwardSource
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_LinkForwardSource
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LinkSource() As String 'Inherits BaseClasses.CPRequestBaseClass.LinkSource
            Get
                If True Then
                    Return cpCore.webServer.requestUrlSource
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Page() As String 'Inherits BaseClasses.CPRequestBaseClass.Page
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_requestPage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Path() As String 'Inherits BaseClasses.CPRequestBaseClass.Path
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_requestPath
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property PathPage() As String 'Inherits BaseClasses.CPRequestBaseClass.PathPage
            Get
                If True Then
                    Return cpCore.webServer.requestPathPage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Protocol() As String 'Inherits BaseClasses.CPRequestBaseClass.Protocol
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_requestProtocol
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property QueryString() As String 'Inherits BaseClasses.CPRequestBaseClass.QueryString
            Get
                If True Then
                    Return cpCore.webServer.requestQueryString
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Referer() As String 'Inherits BaseClasses.CPRequestBaseClass.Referer
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_requestReferer
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property RemoteIP() As String 'Inherits BaseClasses.CPRequestBaseClass.RemoteIP
            Get
                If True Then
                    Return cpCore.webServer.requestRemoteIP
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Secure() As Boolean 'Inherits BaseClasses.CPRequestBaseClass.Secure
            Get
                If True Then
                    Return cpCore.webServer.requestSecure
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function OK(ByVal RequestName As String) As Boolean 'Inherits BaseClasses.CPRequestBaseClass.OK
            If True Then
                Return cpCore.docProperties.containsKey(RequestName)
            Else
                Return False
            End If
        End Function
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.request, " & copy & vbCrLf, True)
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