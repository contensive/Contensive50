
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPEmailClass.ClassId, CPEmailClass.InterfaceId, CPEmailClass.EventsId)>
    Public Class CPEmailClass
        Inherits BaseClasses.CPEmailBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "7D2901F1-B5E8-4293-9373-909FDA6C7749"
        Public Const InterfaceId As String = "2DC385E8-C4E7-4BBF-AE6D-F0FC5E2AA3C1"
        Public Const EventsId As String = "32E893C5-165B-4088-8D9E-CE82524A5000"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
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

        Public Overrides ReadOnly Property fromAddressDefault() As String
            Get
                If True Then
                    Return cpCore.user.isAuthenticatedMember
                Else
                    Return False
                End If
            End Get
        End Property
        '==========================================================================================
        ''' <summary>
        ''' Send email to an email address.
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="SendImmediately"></param>
        ''' <param name="BodyIsHTML"></param>
        Public Overrides Sub Send(ByVal ToAddress As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True)
            Try
                Call cpCore.email_send3(ToAddress, FromAddress, Subject, Body, "", "", "", SendImmediately, BodyIsHTML, 0)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '====================================================================================================
        ''' <summary>
        ''' Send submitted form within an email
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        Public Overrides Sub SendForm(ByVal ToAddress As String, ByVal FromAddress As String, ByVal Subject As String)
            Try
                Call cpCore.main_SendFormEmail(ToAddress, FromAddress, Subject)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '====================================================================================================
        ''' <summary>
        ''' Send email to a list of groups
        ''' </summary>
        ''' <param name="GroupList"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="SendImmediately"></param>
        ''' <param name="BodyIsHTML"></param>
        Public Overrides Sub SendGroup(ByVal GroupList As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True)
            Try
                Call cpCore.main_SendGroupEmail(GroupList, FromAddress, Subject, Body, SendImmediately, BodyIsHTML)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub

        Public Overrides Sub SendPassword(ByVal UserEmailAddress As String) 'Inherits BaseClasses.CPEmailBaseClass.SendPassword
            If True Then
                Call cpCore.user.sendPassword(UserEmailAddress)
            End If
        End Sub

        Public Overrides Sub SendSystem(ByVal EmailName As String, Optional ByVal AdditionalCopy As String = "", Optional ByVal AdditionalUserID As Integer = 0)
            Call cpCore.csv_SendSystemEmail(EmailName, AdditionalCopy, AdditionalUserID)
        End Sub

        Public Overrides Sub SendUser(ByVal toUserId As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True) 'Inherits BaseClasses.CPEmailBaseClass.SendUser
            Dim userId As Integer = 0
            If vbIsNumeric(toUserId) Then
                userId = CInt(toUserId)
                Call cpCore.email_sendMemberEmail3(userId, FromAddress, Subject, Body, SendImmediately, BodyIsHTML, 0, "", False)
            End If
        End Sub
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.email, " & copy & vbCrLf, True)
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