Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPResponseClass.ClassId, CPResponseClass.InterfaceId, CPResponseClass.EventsId)> _
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
        Private cpCore As Contensive.Core.cpCoreClass
        Protected disposed As Boolean = False
        '
        ' Constructor
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.cpCoreClass)
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
        Public Overrides Property ContentType() As String 'Inherits BaseClasses.CPResponseBaseClass.ContentType
            Get
                If True Then
                    Return cpCore.responseContentType
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    Call cpCore.setResponseContentType(value)
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property Cookies() As String 'Inherits BaseClasses.CPResponseBaseClass.Cookies
            Get
                If True Then
                    Return cpCore.responseCookies
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Header() As String 'Inherits BaseClasses.CPResponseBaseClass.Header
            Get
                If True Then
                    Return cpCore.responseHeader
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
                Call cpCore.main_CloseStream()
            End If
        End Sub

        Public Overrides Sub AddHeader(ByVal HeaderName As String, ByVal HeaderValue As String) 'Inherits BaseClasses.CPResponseBaseClass.AddHeader
            If True Then
                Call cpCore.addResponseHeader(HeaderName, HeaderValue)
            End If
        End Sub

        Public Overrides Sub Flush() 'Inherits BaseClasses.CPResponseBaseClass.Flush
            If True Then
                Call cpCore.main_FlushStream()
            End If
        End Sub
        Public Overrides Sub Redirect(ByVal Link As String) 'Inherits BaseClasses.CPResponseBaseClass.Redirect
            If True Then
                Call cpCore.main_Redirect(Link)
            End If
        End Sub

        Public Overrides Sub SetBuffer(ByVal BufferOn As Boolean) 'Inherits BaseClasses.CPResponseBaseClass.SetBuffer
            If True Then
                Call cpCore.main_SetStreamBuffer(BufferOn)
            End If
        End Sub

        Public Overrides Sub SetStatus(ByVal status As String) 'Inherits BaseClasses.CPResponseBaseClass.SetStatus
            If True Then
                Call cpCore.setResponseStatus(status)
            End If
        End Sub

        Public Overrides Sub SetTimeout(ByVal TimeoutSeconds As String) 'Inherits BaseClasses.CPResponseBaseClass.SetTimeout
            If True Then
                'Call cmc.main_SetStreamTimeout(TimeoutSeconds)
            End If
        End Sub

        Public Overrides Sub SetType(ByVal ContentType As String) 'Inherits BaseClasses.CPResponseBaseClass.SetType
            If True Then
                Call cpCore.setResponseContentType(ContentType)
            End If
        End Sub

        Public Overrides Sub SetCookie(ByVal CookieName As String, ByVal CookieValue As String, Optional ByVal DateExpires As Date = #12:00:00 AM#, Optional ByVal Domain As String = "", Optional ByVal Path As String = "", Optional ByVal Secure As Boolean = False) 'Inherits BaseClasses.CPResponseBaseClass.SetCookie
            If True Then
                Call cpCore.inet_addResponseCookie(CookieName, CookieValue, DateExpires, Domain, Path, Secure)
            End If
        End Sub

        Public Overrides Sub Write(ByVal message As String)
            If True Then
                Call cpCore.writeAltBuffer(message)
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