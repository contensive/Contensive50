
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Models.Complex
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

        Public Overrides ReadOnly Property Browser() As String
            Get
                If True Then
                    Return cpCore.webServer.requestBrowser
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsIE() As Boolean
            Get
                If True Then
                    Return cpCore.doc.authContext.visit_browserIsIE
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMac() As Boolean
            Get
                If True Then
                    Return cpCore.doc.authContext.visit_browserIsMac
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsMobile() As Boolean
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.Mobile
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserIsWindows() As Boolean
            Get
                If True Then
                    Return cpCore.doc.authContext.visit_browserIsWindows
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property BrowserVersion() As String
            Get
                If True Then
                    Return cpCore.doc.authContext.visit_browserVersion
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function Cookie(ByVal CookieName As String) As String
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
        Public Overrides ReadOnly Property CookieString() As String
            Get
                Dim returnCookies As String = ""
                For Each kvp As KeyValuePair(Of String, iisController.cookieClass) In cpCore.webServer.requestCookies
                    returnCookies &= "&" & kvp.Key & "=" & kvp.Value.value
                Next
                If returnCookies.Length > 0 Then
                    returnCookies = returnCookies.Substring(1)
                End If
                Return returnCookies
            End Get
        End Property

        Public Overrides ReadOnly Property Form() As String
            Get
                Return Controllers.genericController.convertNameValueDictToREquestString(cpCore.webServer.requestFormDict)
            End Get
        End Property

        Public Overrides ReadOnly Property FormAction() As String
            Get
                Return cpCore.webServer.serverFormActionURL
            End Get
        End Property

        Public Overrides Function GetBoolean(ByVal RequestName As String) As Boolean
            Return cpCore.docProperties.getBoolean(RequestName)
        End Function

        Public Overrides Function GetDate(ByVal RequestName As String) As Date
            Return cpCore.docProperties.getDate(RequestName)
        End Function

        Public Overrides Function GetInteger(ByVal RequestName As String) As Integer
            Return cpCore.docProperties.getInteger(RequestName)
        End Function

        Public Overrides Function GetNumber(ByVal RequestName As String) As Double
            Return cpCore.docProperties.getNumber(RequestName)
        End Function

        Public Overrides Function GetText(ByVal RequestName As String) As String
            Return cpCore.docProperties.getText(RequestName)
        End Function

        Public Overrides ReadOnly Property Host() As String
            Get
                Return cpCore.webServer.requestDomain
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAccept() As String
            Get
                Return cpCore.webServer.requestHttpAccept
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPAcceptCharset() As String
            Get
                Return cpCore.webServer.requestHttpAcceptCharset
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPProfile() As String
            Get
                Return cpCore.webServer.requestHttpProfile
            End Get
        End Property

        Public Overrides ReadOnly Property HTTPXWapProfile() As String
            Get
                Return cpCore.webServer.requestxWapProfile
            End Get
        End Property

        Public Overrides ReadOnly Property Language() As String
            Get
                If (cpCore.doc.authContext.userLanguage Is Nothing) Then
                    Return ""
                End If
                Dim userLanguage As LanguageModel = LanguageModel.create(cpCore, cpCore.doc.authContext.user.LanguageID, New List(Of String))
                If (userLanguage IsNot Nothing) Then
                    Return userLanguage.Name
                End If
                Return "English"
            End Get
        End Property

        Public Overrides ReadOnly Property Link() As String
            Get
                Return cpCore.webServer.requestUrl
            End Get
        End Property

        Public Overrides ReadOnly Property LinkForwardSource() As String
            Get
                Return cpCore.webServer.linkForwardSource
            End Get
        End Property

        Public Overrides ReadOnly Property LinkSource() As String
            Get
                Return cpCore.webServer.requestUrlSource
            End Get
        End Property

        Public Overrides ReadOnly Property Page() As String
            Get
                Return cpCore.webServer.requestPage
            End Get
        End Property

        Public Overrides ReadOnly Property Path() As String
            Get
                Return cpCore.webServer.requestPath
            End Get
        End Property

        Public Overrides ReadOnly Property PathPage() As String
            Get
                Return cpCore.webServer.requestPathPage
            End Get
        End Property

        Public Overrides ReadOnly Property Protocol() As String
            Get
                Return cpCore.webServer.requestProtocol
            End Get
        End Property

        Public Overrides ReadOnly Property QueryString() As String
            Get
                Return cpCore.webServer.requestQueryString
            End Get
        End Property

        Public Overrides ReadOnly Property Referer() As String
            Get
                Return cpCore.webServer.requestReferer
            End Get
        End Property

        Public Overrides ReadOnly Property RemoteIP() As String
            Get
                Return cpCore.webServer.requestRemoteIP
            End Get
        End Property

        Public Overrides ReadOnly Property Secure() As Boolean
            Get
                Return cpCore.webServer.requestSecure
            End Get
        End Property

        Public Overrides Function OK(ByVal RequestName As String) As Boolean
            Return cpCore.docProperties.containsKey(RequestName)
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