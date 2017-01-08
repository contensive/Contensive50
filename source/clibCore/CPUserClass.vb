
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
        Private cpCore As Contensive.Core.coreClass
        Private CP As CPClass
        Protected disposed As Boolean = False
        '
        '====================================================================================================
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.coreClass, ByVal CPParent As CPClass)
            MyBase.New()
            cpCore = cpCoreObj
            CP = CPParent
        End Sub
        '
        '====================================================================================================
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
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Email() As String 'Inherits BaseClasses.CPUserBaseClass.Email
            Get
                If True Then
                    Return cpCore.user.email
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function GetIdByLogin(ByVal Username As String, ByVal Password As String) As Integer 'Inherits BaseClasses.CPUserBaseClass.GetIdByLogin
            If True Then
                Return cpCore.user.authenticateGetId(Username, Password)
            Else
                Return 0
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Id() As Integer 'Inherits BaseClasses.CPUserBaseClass.Id
            Get
                Dim localId As Integer = 0
                '
                If True Then
                    localId = cpCore.user.id
                    If (localId = 0) Then
                        localId = cpCore.db.metaData_InsertContentRecordGetID("people", 0)
                        Call cpCore.user.recognizeById(localId)
                    End If
                End If
                Return localId
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsAdmin() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAdmin
            Get
                If True Then
                    Return cpCore.user.isAuthenticatedAdmin
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsAdvancedEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAdvancedEditing
            If True Then
                Return cpCore.user.isAdvancedEditing(ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsAuthenticated() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAuthenticated
            Get
                If True Then
                    Return (cpCore.user.isAuthenticated())
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsAuthoring(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsAuthoring
            If True Then
                Return cpCore.user.isEditing(ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsContentManager(Optional ByVal ContentName As String = "Page Content") As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsContentManager
            If True Then
                Return cpCore.user.isAuthenticatedContentManager(ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsDeveloper() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsDeveloper
            Get
                If True Then
                    Return cpCore.user.isAuthenticatedDeveloper()
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsEditing
            If True Then
                Return cpCore.user.isEditing(ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsEditingAnything() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsEditingAnything
            Get
                If True Then
                    Return cpCore.user.isEditingAnything()
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsGuest() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsGuest
            Get
                If True Then
                    Return cpCore.user.isGuest
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
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
                Call CP.core.handleExceptionAndRethrow(ex, "Unexpected error in cs.user.IsInGroup")
                result = False
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsInGroupList(ByVal GroupIDList As String, Optional ByVal userId As Integer = 0) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsInGroup
            Dim result As Boolean
            '
            Try
                If userId = 0 Then
                    userId = Id
                End If
                result = cpCore.user.isMemberOfGroupIdList(userId, IsAuthenticated(), GroupIDList, False)
            Catch ex As Exception
                Call CP.core.handleExceptionAndRethrow(ex, "Unexpected error in cs.user.IsInGroupList")
                result = False
            End Try
            Return result
            '
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsMember() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsMember
            Get
                If True Then
                    Return cpCore.user.isAuthenticatedMember
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsQuickEditing(ByVal ContentName As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsQuickEditing
            If True Then
                Return cpCore.user.isQuickEditing(ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsRecognized() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsRecognized
            Get
                If True Then
                    Return cpCore.user.isRecognized
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsWorkflowRendering() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsWorkflowRendering
            Get
                If True Then
                    Return cpCore.pagemanager_IsWorkflowRendering
                Else
                    Return False
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Language() As String 'Inherits BaseClasses.CPUserBaseClass.Language
            Get
                If True Then
                    Return cpCore.user.language
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property LanguageID() As Integer 'Inherits BaseClasses.CPUserBaseClass.LanguageId
            Get
                If True Then
                    Return cpCore.user.languageId
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function Login(ByVal UsernameOrEmail As String, ByVal Password As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean 'Inherits BaseClasses.CPUserBaseClass.Login
            If True Then
                Return cpCore.user.authenticate(UsernameOrEmail, Password, SetAutoLogin)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function LoginByID(ByVal RecordID As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean 'Inherits BaseClasses.CPUserBaseClass.LoginById
            If True Then
                Return cpCore.user.authenticateById(RecordID, SetAutoLogin)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function LoginIsOK(ByVal UsernameOrEmail As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.LoginIsOK
            If True Then
                Return cpCore.main_IsLoginOK(UsernameOrEmail, Password)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Logout() 'Inherits BaseClasses.CPUserBaseClass.Logout
            Call cpCore.user.logout()
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Name() As String 'Inherits BaseClasses.CPUserBaseClass.Name
            Get
                If True Then
                    Return cpCore.user.name
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsNew() As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsNew
            Get
                Return cpCore.user.isNew
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsNewLoginOK(ByVal Username As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.NewLoginIsOK
            Dim errorMessage As String = ""
            Dim errorCode As Integer = 0
            Return cpCore.user.isNewLoginOK(Username, Password, errorMessage, errorCode)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property OrganizationID() As Integer 'Inherits BaseClasses.CPUserBaseClass.OrganizationId
            Get
                If True Then
                    Return cpCore.user.organizationId
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Password() As String 'Inherits BaseClasses.CPUserBaseClass.Password
            Get
                Return cpCore.user.password
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function Recognize(ByVal UserID As Integer) As Boolean 'Inherits BaseClasses.CPUserBaseClass.Recognize
            Return cpCore.user.recognizeById(UserID)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Username() As String 'Inherits BaseClasses.CPUserBaseClass.Username
            Get
                Return cpCore.user.username
            End Get
        End Property
        '
        '=======================================================================================================
        '
        Public Overrides Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetMemberId As Integer = 0) As String
            If (TargetMemberId = 0) Then
                Return cpCore.userProperty.getText(PropertyName, DefaultValue)
            Else
                Return cpCore.userProperty.getText(PropertyName, DefaultValue, TargetMemberId)
            End If
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetMemberId As Integer = 0)
            If (TargetMemberId = 0) Then
                Call cpCore.userProperty.setProperty(PropertyName, Value)
            Else
                Call cpCore.userProperty.setProperty(PropertyName, Value, TargetMemberId)
            End If
        End Sub
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return cpCore.userProperty.getBoolean(PropertyName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return cpCore.userProperty.getDate(PropertyName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return cpCore.userProperty.getInteger(PropertyName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return cpCore.userProperty.getNumber(PropertyName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(PropertyName As String, Optional DefaultValue As String = "") As String
            Return cpCore.userProperty.getText(PropertyName, DefaultValue)
        End Function
        '
        '====================================================================================================
        ' REFACTOR -- figure out what this does and rewrite
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
        '====================================================================================================
        '
        Private Sub appendDebugLog(ByVal copy As String)
            '
        End Sub
        '
        '====================================================================================================
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