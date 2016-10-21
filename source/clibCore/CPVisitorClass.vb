
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPVisitorClass.ClassId, CPVisitorClass.InterfaceId, CPVisitorClass.EventsId)> _
    Public Class CPVisitorClass
        Inherits BaseClasses.CPVisitorBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = ""
        Public Const InterfaceId As String = ""
        Public Const EventsId As String = ""
#End Region
        '
        Private cpCore As Contensive.Core.cpCoreClass
        Private cp As CPClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.cpCoreClass, ByVal cpParent As CPClass)
            MyBase.New()
            cpCore = cpCoreObj
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

        Public Overrides ReadOnly Property ForceBrowserMobile() As Boolean 'Inherits BaseClasses.CPVisitorBaseClass.ForceBrowserMobile
            Get
                If True Then
                    Return cpCore.main_VisitorForceBrowserMobile
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitorId As Integer = 0) As String 'Inherits BaseClasses.CPVisitorBaseClass.GetProperty
            If True Then
                Return cpCore.visitorPropertyCache.getProperty(PropertyName, DefaultValue, TargetVisitorId)
            Else
                Return ""
            End If
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return cp.Utils.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return cp.Utils.EncodeDate(GetProperty(PropertyName, DefaultValue))
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

        Public Overrides ReadOnly Property Id() As Integer 'Inherits BaseClasses.CPVisitorBaseClass.Id
            Get
                If True Then
                    Return cpCore.main_VisitorID
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property IsNew() As Boolean 'Inherits BaseClasses.CPVisitorBaseClass.IsNew
            Get
                If True Then
                    Return cpCore.main_VisitorNew
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitorid As Integer = 0) 'Inherits BaseClasses.CPVisitorBaseClass.SetProperty
            If True Then
                Call cpCore.visitorPropertyCache.setProperty(PropertyName, Value, TargetVisitorid)
            End If
        End Sub

        Public Overrides ReadOnly Property UserId() As Integer 'Inherits BaseClasses.CPVisitorBaseClass.UserId
            Get
                If True Then
                    Return cpCore.main_VisitorMemberID
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.visitor, " & copy & vbCrLf, True)
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