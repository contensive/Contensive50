
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPUserClass.ClassId, CPUserClass.InterfaceId, CPUserClass.EventsId)>
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
                    Return CP.core.authContext.user.Email
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
                Return CP.core.authContext.authenticateGetId(cpCore, Username, Password)
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
                    localId = CP.core.authContext.user.ID
                    If (localId = 0) Then
                        localId = CP.core.db.metaData_InsertContentRecordGetID("people", 0)
                        Call CP.core.authContext.recognizeById(cpCore, localId, CP.core.authContext)
                    End If
                End If
                Return localId
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property IsAdmin() As Boolean 'Inherits BaseClasses.CPUserBaseClass.admin
            Get
                If True Then
                    Return CP.core.authContext.isAuthenticatedAdmin(cpCore)
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
                Return CP.core.authContext.isAdvancedEditing(cpCore, ContentName)
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
                    Return (CP.core.authContext.isAuthenticated())
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
                Return CP.core.authContext.isEditing(cpCore, ContentName)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function IsContentManager(Optional ByVal ContentName As String = "Page Content") As Boolean 'Inherits BaseClasses.CPUserBaseClass.IsContentManager
            If True Then
                Return CP.core.authContext.isAuthenticatedContentManager(cpCore, ContentName)
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
                    Return CP.core.authContext.isAuthenticatedDeveloper(cpCore)
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
                Return CP.core.authContext.isEditing(cpCore, ContentName)
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
                    Return CP.core.authContext.isEditingAnything(cpCore)
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
                    Return CP.core.authContext.isGuest(cpCore)
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
                Call CP.core.handleExceptionAndContinue(ex, "Unexpected error in cs.user.IsInGroup")
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
                result = CP.core.authContext.isMemberOfGroupIdList(cpCore, userId, IsAuthenticated(), GroupIDList, False)
            Catch ex As Exception
                Call CP.core.handleExceptionAndContinue(ex, "Unexpected error in cs.user.IsInGroupList")
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
                    Return CP.core.authContext.isAuthenticatedMember(cpCore)
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
                Return CP.core.authContext.isQuickEditing(cpCore, ContentName)
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
                    Return CP.core.authContext.isRecognized(cpCore)
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
                    Return CP.core.pages.pagemanager_IsWorkflowRendering
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
                    Return CP.core.authContext.user.language
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
                    Return CP.core.authContext.user.LanguageID
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
                Return CP.core.authContext.authenticate(cpCore, UsernameOrEmail, Password, SetAutoLogin)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        <Obsolete("Use LoginById(integer) instead", False)>
        Public Overrides Function LoginByID(ByVal RecordID As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean
            Return CP.core.authContext.authenticateById(cpCore, EncodeInteger(RecordID), CP.core.authContext)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function LoginByID(ByVal RecordID As Integer) As Boolean
            Return CP.core.authContext.authenticateById(cpCore, RecordID, CP.core.authContext)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function LoginByID(ByVal RecordID As Integer, ByVal SetAutoLogin As Boolean) As Boolean
            Return CP.core.authContext.authenticateById(cpCore, RecordID, CP.core.authContext)
            If Not CP.core.authContext.user.AutoLogin Then
                CP.core.authContext.user.AutoLogin = True
                CP.core.authContext.user.saveObject(cpCore)
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function LoginIsOK(ByVal UsernameOrEmail As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.LoginIsOK
            If True Then
                Return CP.core.authContext.main_IsLoginOK(cpCore, UsernameOrEmail, Password)
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub Logout() 'Inherits BaseClasses.CPUserBaseClass.Logout
            Call CP.core.authContext.logout(cpCore)
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Name() As String 'Inherits BaseClasses.CPUserBaseClass.Name
            Get
                If True Then
                    Return CP.core.authContext.user.Name
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
                Return CP.core.authContext.visit.MemberNew
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function IsNewLoginOK(ByVal Username As String, ByVal Password As String) As Boolean 'Inherits BaseClasses.CPUserBaseClass.NewLoginIsOK
            Dim errorMessage As String = ""
            Dim errorCode As Integer = 0
            Return CP.core.authContext.isNewLoginOK(cpCore, Username, Password, errorMessage, errorCode)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property OrganizationID() As Integer 'Inherits BaseClasses.CPUserBaseClass.OrganizationId
            Get
                If True Then
                    Return CP.core.authContext.user.OrganizationID
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
                Return CP.core.authContext.user.Password
            End Get
        End Property
        '
        '====================================================================================================
        '
        Public Overrides Function Recognize(ByVal UserID As Integer) As Boolean 'Inherits BaseClasses.CPUserBaseClass.Recognize
            Return CP.core.authContext.recognizeById(cpCore, UserID, CP.core.authContext)
        End Function
        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property Username() As String 'Inherits BaseClasses.CPUserBaseClass.Username
            Get
                Return CP.core.authContext.user.Username
            End Get
        End Property
        '
        '=======================================================================================================
        '
        Public Overrides Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetMemberId As Integer = 0) As String
            If (TargetMemberId = 0) Then
                Return CP.core.userProperty.getText(PropertyName, DefaultValue)
            Else
                Return CP.core.userProperty.getText(PropertyName, DefaultValue, TargetMemberId)
            End If
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetMemberId As Integer = 0)
            If (TargetMemberId = 0) Then
                Call CP.core.userProperty.setProperty(PropertyName, Value)
            Else
                Call CP.core.userProperty.setProperty(PropertyName, Value, TargetMemberId)
            End If
        End Sub
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return CP.core.userProperty.getBoolean(PropertyName, genericController.EncodeBoolean(DefaultValue))
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return CP.core.userProperty.getDate(PropertyName, genericController.EncodeDate(DefaultValue))
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return CP.core.userProperty.getInteger(PropertyName, genericController.EncodeInteger(DefaultValue))
        End Function
        '
        '=======================================================================================================
        ' REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return CP.core.userProperty.getNumber(PropertyName, EncodeNumber(DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(PropertyName As String, Optional DefaultValue As String = "") As String
            Return CP.core.userProperty.getText(PropertyName, DefaultValue)
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