
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPUserClass.ClassId, CPUserClass.InterfaceId, CPUserClass.EventsId)> _
    Public Class CPUserClass
        Inherits BaseClasses.CPUserBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "08EF64C6-9C51-4B32-84D9-0D3BDAF42A28"
        Public Const InterfaceId As String = "B1A95B1F-A00D-4AC6-B3A0-B1619568C2EA"
        Public Const EventsId As String = "DBE2B6CB-6339-4FFB-92D7-BE37AEA841CC"
#End Region
        '
        Private cpCore As Contensive.Core.cpCoreClass
        Private CP As CPClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.cpCoreClass, ByVal CPParent As CPClass)
            MyBase.New()
            cpCore = cpCoreObj
            CP = CPParent
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference cp, main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    CP = Nothing
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
        Public Overrides ReadOnly Property Email() As String 'Inherits BaseClasses.CPUserBaseClass.Email
            Get
                If True Then
                    Return cpCore.userEmail
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function GetIdByLogin(ByVal Username As String, ByVal Password As String) As Integer 'Inherits BaseClasses.CPUserBaseClass.GetIdByLogin
            If True Then
                Return cpCore.main_GetLoginMemberID(Username, Password)
            Else
                Return 0
            End If
        End Function
        '
        ' verifies the user record is initialize and returns the Id
        '

        Public Overrides ReadOnly Property Id() As Integer 'Inherits BaseClasses.CPUserBaseClass.Id
            Get
                Dim localId As Integer = 0
                '
                If True Then
                    localId = cpCore.userId
                    If (localId = 0) Then
                        localId = cpCore.db.metaData_InsertContentRecordGetID("people", 0)
                        Call cpCore.useer_RecognizeMemberByID(localId)
                    End If
                End If
                Return localId
            End Get
        End Property

        Public Overrides ReadOnly Property IsAdmin() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAdmin
            Get
                If True Then
                    Return cpCore.user_isAdmin
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function IsAdvancedEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAdvancedEditing
            If True Then
                Return cpCore.user_IsAdvancedEditing(ContentName)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property IsAuthenticated() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAuthenticated
            Get
                If True Then
                    Return (cpCore.user_isAuthenticated())
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function IsAuthoring(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAuthoring
            If True Then
                Return cpCore.user_IsAuthoring(ContentName)
            Else
                Return False
            End If
        End Function

        Public Overrides Function IsContentManager(Optional ByVal ContentName As String = "Page Content") As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsContentManager
            If True Then
                Return cpCore.main_IsContentManager(ContentName)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property IsDeveloper() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsDeveloper
            Get
                If True Then
                    Return cpCore.user_isDeveloper()
                Else
                    Return False
                End If
            End Get
        End Property


        Public Overrides Function IsEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsEditing
            If True Then
                Return cpCore.user_isEditing(ContentName)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property IsEditingAnything() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsEditingAnything
            Get
                If True Then
                    Return cpCore.user_isEditingAnything()
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property IsGuest() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsGuest
            Get
                If True Then
                    Return cpCore.user_IsGuest
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides Function IsInGroup(ByVal groupName As String, Optional ByVal userId As Integer = 0) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsInGroup
            Dim groupId As Integer
            Dim result As Boolean
            '
            Try
                groupId = CP.Group.GetId(groupName)
                If userId = 0 Then
                    userId = Id
                End If
                If groupId = 0 Then
                    result = False
                Else
                    result = IsInGroupList(groupId.ToString, userId)
                End If
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in cs.user.IsInGroup")
                result = False
            End Try
            Return result
        End Function
        '
        '
        '
        Public Overrides Function IsInGroupList(ByVal GroupIDList As String, Optional ByVal userId As Integer = 0) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsInGroup
            Dim result As Boolean
            '
            Try
                If userId = 0 Then
                    userId = Id
                End If
                result = cpCore.user_isMemberOfGroupIdList2(userId, IsAuthenticated(), GroupIDList, False)
            Catch ex As Exception
                Call cp.core.handleExceptionAndRethrow(ex, "Unexpected error in cs.user.IsInGroupList")
                result = False
            End Try
            Return result
            '
        End Function

        Public Overrides ReadOnly Property IsMember() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsMember
            Get
                If True Then
                    Return cpCore.user_IsMember
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function IsQuickEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsQuickEditing
            If True Then
                Return cpCore.user_isQuickEditing(ContentName)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property IsRecognized() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsRecognized
            Get
                If True Then
                    Return cpCore.user_isRecognized
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property IsWorkflowRendering() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsWorkflowRendering
            Get
                If True Then
                    Return cpCore.pagemanager_IsWorkflowRendering
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Language() As String 'Inherits BaseClasses.CPUserBaseClass.Language
            Get
                If True Then
                    Return cpCore.userLanguage
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property LanguageID() As Integer 'Inherits BaseClasses.CPUserBaseClass.LanguageId
            Get
                If True Then
                    Return cpCore.userLanguageId
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides Function Login(ByVal UsernameOrEmail As String, ByVal Password As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean 'Inherits BaseClasses.CPUserBaseClass.Login
            If True Then
                Return cpCore.user_LoginMember(UsernameOrEmail, Password, SetAutoLogin)
            Else
                Return False
            End If
        End Function

        Public Overrides Function LoginByID(ByVal RecordID As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean 'Inherits BaseClasses.CPUserBaseClass.LoginById
            If True Then
                Return cpCore.user_LoginMemberByID(RecordID, SetAutoLogin)
            Else
                Return False
            End If
        End Function

        Public Overrides Function LoginIsOK(ByVal UsernameOrEmail As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.LoginIsOK
            If True Then
                Return cpCore.main_IsLoginOK(UsernameOrEmail, Password)
            Else
                Return False
            End If
        End Function

        Public Overrides Sub Logout() 'Inherits BaseClasses.CPUserBaseClass.Logout
            If True Then
                Call cpCore.security_LogoutMember()
            End If
        End Sub

        Public Overrides ReadOnly Property Name() As String 'Inherits BaseClasses.CPUserBaseClass.Name
            Get
                If True Then
                    Return cpCore.userName
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property IsNew() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsNew
            Get
                If True Then
                    Return cpCore.userIsNew
                Else
                    Return False
                End If
            End Get
        End Property

        Public Overrides Function IsNewLoginOK(ByVal Username As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.NewLoginIsOK
            If True Then
                Return cpCore.main_IsNewLoginOK(Username, Password)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property OrganizationID() As Integer 'Inherits BaseClasses.CPUserBaseClass.OrganizationId
            Get
                If True Then
                    Return cpCore.userOrganizationId
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property Password() As String 'Inherits BaseClasses.CPUserBaseClass.Password
            Get
                If True Then
                    Return cpCore.userPassword
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Function Recognize(ByVal UserID As Integer) As Boolean 'Inherits BaseClasses.CPUserBaseClass.Recognize
            If True Then
                Return cpCore.useer_RecognizeMemberByID(UserID)
            Else
                Return False
            End If
        End Function

        Public Overrides ReadOnly Property Username() As String 'Inherits BaseClasses.CPUserBaseClass.Username
            Get
                If True Then
                    Return cpCore.userUsername
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetMemberId As Integer = 0) As String
            If True Then
                Return cpCore.properties_user_getText(PropertyName, DefaultValue, TargetMemberId)
            Else
                Return ""
            End If
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetMemberId As Integer = 0)
            If True Then
                Call cpCore.properties_SetMemberProperty2(PropertyName, Value, TargetMemberId)
            End If
        End Sub
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return CP.Utils.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return CP.Utils.EncodeDate(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return CP.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return CP.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(FieldName As String, Optional DefaultValue As String = "") As String
            Return GetProperty(FieldName, DefaultValue)
        End Function
        '
        '
        '
        Public Overrides Sub track()
            Dim localId As Integer
            If True Then
                '
                ' get the id property, which triggers a track() if it returns o
                '
                localId = Id
                'If Id = 0 Then
                '    localId = cmc.csv_InsertContentRecordGetID("people", 0)
                '    Call cmc.main_RecognizeMemberByID(localId)
                'End If
            End If
        End Sub

        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.user, " & copy & vbCrLf, True)
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