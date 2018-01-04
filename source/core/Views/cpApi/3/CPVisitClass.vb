

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPVisitClass.ClassId, CPVisitClass.InterfaceId, CPVisitClass.EventsId)>
    Public Class CPVisitClass
        Inherits BaseClasses.CPVisitBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "3562FB08-178D-4AD1-A923-EAEAAF33FE84"
        Public Const InterfaceId As String = "A1CC6FCB-810B-46C4-8232-D3166CACCBAD"
        Public Const EventsId As String = "2AFEB1A8-5B27-45AC-A9DF-F99849BE1FAE"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Private cp As CPClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.coreClass, ByVal cpParent As CPClass)
            MyBase.New()
            Me.cpCore = cpCoreObj
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
                    cp = Nothing
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub

        Public Overrides ReadOnly Property CookieSupport() As Boolean 'Inherits BaseClasses.CPVisitBaseClass.CookieSupport
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.cookieSupport
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitId As Integer = 0) As String
            If TargetVisitId = 0 Then
                Return cpCore.visitProperty.getText(PropertyName, DefaultValue)
            Else
                Return cpCore.visitProperty.getText(PropertyName, DefaultValue, TargetVisitId)
            End If
        End Function
        '
        '
        '
        Public Overrides ReadOnly Property Id() As Integer
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.id
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LastTime() As Date 'Inherits BaseClasses.CPVisitBaseClass.LastTime
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.lastvisitTime
                Else
                    Return New Date()
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LoginAttempts() As Integer 'Inherits BaseClasses.CPVisitBaseClass.LoginAttempts
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.loginAttempts
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String 'Inherits BaseClasses.CPVisitBaseClass.Name
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.name
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Pages() As Integer 'Inherits BaseClasses.CPVisitBaseClass.Pages
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.pagevisits
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Referer() As String 'Inherits BaseClasses.CPVisitBaseClass.Referer
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.http_referer
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitId As Integer = 0)
            If TargetVisitId = 0 Then
                Call cpCore.visitProperty.setProperty(PropertyName, Value)
            Else
                Call cpCore.visitProperty.setProperty(PropertyName, Value, TargetVisitId)
            End If
        End Sub
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(FieldName As String, Optional DefaultValue As String = "") As String
            Return GetProperty(FieldName, DefaultValue)
        End Function

        Public Overrides ReadOnly Property StartDateValue() As Integer 'Inherits BaseClasses.CPVisitBaseClass.StartDateValue
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.startDateValue
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property StartTime() As Date 'Inherits BaseClasses.CPVisitBaseClass.StartTime
            Get
                If True Then
                    Return cpCore.doc.authContext.visit.startTime
                Else
                    Return New Date()
                End If
            End Get
        End Property
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.visit, " & copy & vbCrLf, True)
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