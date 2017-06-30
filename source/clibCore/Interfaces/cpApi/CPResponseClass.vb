Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPResponseClass.ClassId, CPResponseClass.InterfaceId, CPResponseClass.EventsId)>
    Public Class CPResponseClass
        Inherits BaseClasses.CPResponseBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "054CD625-A12A-4D21-A581-84EC0D604E65"
        Public Const InterfaceId As String = "130395BA-EF1A-4B1D-B43C-01356127660A"
        Public Const EventsId As String = "C7FCA224-8542-46F2-9019-52A7B5BAE4DB"
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
        '
        '
        '
        Public Overrides Property ContentType() As String
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_bufferContentType
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    Call cpCore.webServer.setResponseContentType(value)
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property Cookies() As String
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_bufferCookies
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Header() As String 'Inherits BaseClasses.CPResponseBaseClass.Header
            Get
                If True Then
                    Return cpCore.webServer.webServerIO_bufferResponseHeader
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides Sub Clear() 'Inherits BaseClasses.CPResponseBaseClass.Clear
            If True Then
                Call cpCore.main_ClearStream()
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub Close() 'Inherits BaseClasses.CPResponseBaseClass.Close
            If True Then
                Call cpCore.doc_close()
            End If
        End Sub

        Public Overrides Sub AddHeader(ByVal HeaderName As String, ByVal HeaderValue As String) 'Inherits BaseClasses.CPResponseBaseClass.AddHeader
            If True Then
                Call cpCore.webServer.addResponseHeader(HeaderName, HeaderValue)
            End If
        End Sub

        Public Overrides Sub Flush()
            If True Then
                Call cpCore.webServer.flushStream()
            End If
        End Sub
        Public Overrides Sub Redirect(ByVal Link As String)
            If True Then
                Call cpCore.main_Redirect(Link)
            End If
        End Sub

        Public Overrides Sub SetBuffer(ByVal BufferOn As Boolean)
            If True Then
                Call cpCore.htmlDoc.enableOutputBuffer(BufferOn)
            End If
        End Sub

        Public Overrides Sub SetStatus(ByVal status As String) 'Inherits BaseClasses.CPResponseBaseClass.SetStatus
            If True Then
                Call cpCore.webServer.setResponseStatus(status)
            End If
        End Sub

        Public Overrides Sub SetTimeout(ByVal TimeoutSeconds As String)
            If True Then
                'Call cmc.main_SetStreamTimeout(TimeoutSeconds)
            End If
        End Sub

        Public Overrides Sub SetType(ByVal ContentType As String) 'Inherits BaseClasses.CPResponseBaseClass.SetType
            If True Then
                Call cpCore.webServer.setResponseContentType(ContentType)
            End If
        End Sub

        Public Overrides Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, Optional ByVal DateExpires As Date = #12:00:00 AM#, Optional ByVal Domain As String = "", Optional ByVal Path As String = "", Optional ByVal Secure As Boolean = False) 'Inherits BaseClasses.CPResponseBaseClass.SetCookie
            If True Then
                Call cpCore.webServer.addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure)
            End If
        End Sub

        Public Overrides Sub Write(ByVal message As String)
            If True Then
                Call cpCore.htmlDoc.writeAltBuffer(message)
            End If
        End Sub
        '
        '
        '
        Public Overrides ReadOnly Property isOpen() As Boolean
            Get
                If True Then
                    Return cpCore.docOpen
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.response, " & copy & vbCrLf, True)
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