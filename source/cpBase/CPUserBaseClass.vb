'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPUserBaseClass
        '
        ' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
        '
        Public MustOverride Function GetIdByLogin(ByVal Username As String, ByVal Password As String) As Integer 'Implements BaseClasses.CPUserBaseClass.GetIdByLogin
        Public MustOverride Sub Track()
        Public MustOverride Function IsAdvancedEditing(ByVal ContentName As String) As Boolean
        Public MustOverride ReadOnly Property IsAuthenticated() As Boolean
        Public MustOverride Function IsAuthoring(ByVal ContentName As String) As Boolean
        Public MustOverride Function IsEditing(ByVal ContentName As String) As Boolean
        Public MustOverride ReadOnly Property IsEditingAnything() As Boolean
        Public MustOverride ReadOnly Property IsGuest() As Boolean
        Public MustOverride Function IsQuickEditing(ByVal ContentName As String) As Boolean
        Public MustOverride ReadOnly Property IsRecognized() As Boolean
        Public MustOverride ReadOnly Property IsWorkflowRendering() As Boolean
        Public MustOverride ReadOnly Property IsNew() As Boolean
        '
        Public MustOverride ReadOnly Property IsAdmin() As Boolean
        Public MustOverride Function IsContentManager(Optional ByVal ContentName As String = "Page Content") As Boolean
        Public MustOverride ReadOnly Property IsDeveloper() As Boolean
        Public MustOverride Function IsInGroup(ByVal GroupName As String, Optional ByVal CheckUserID As Integer = 0) As Boolean
        Public MustOverride Function IsInGroupList(ByVal GroupIDList As String, Optional ByVal CheckUserID As Integer = 0) As Boolean
        Public MustOverride ReadOnly Property IsMember() As Boolean
        Public MustOverride Function Recognize(ByVal UserID As Integer) As Boolean
        '
        Public MustOverride Function Login(ByVal UsernameOrEmail As String, ByVal Password As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean
        Public MustOverride Function LoginByID(ByVal RecordID As String, Optional ByVal SetAutoLogin As Boolean = False) As Boolean
        Public MustOverride Function LoginByID(ByVal RecordID As Integer) As Boolean
        Public MustOverride Function LoginByID(ByVal RecordID As Integer, ByVal SetAutoLogin As Boolean) As Boolean
        Public MustOverride Function LoginIsOK(ByVal UsernameOrEmail As String, ByVal Password As String) As Boolean
        Public MustOverride Sub Logout()
        Public MustOverride Function IsNewLoginOK(ByVal Username As String, ByVal Password As String) As Boolean
        '
        Public MustOverride ReadOnly Property Language() As String
        Public MustOverride ReadOnly Property LanguageID() As Integer
        Public MustOverride ReadOnly Property Email() As String
        Public MustOverride ReadOnly Property Id() As Integer
        Public MustOverride ReadOnly Property Name() As String
        Public MustOverride ReadOnly Property OrganizationID() As Integer
        Public MustOverride ReadOnly Property Password() As String
        Public MustOverride ReadOnly Property Username() As String
        '
        Public MustOverride Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitId As Integer = 0) As String
        Public MustOverride Function GetText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
        Public MustOverride Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
        Public MustOverride Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
        Public MustOverride Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
        'Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
        Public MustOverride Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitId As Integer = 0)
    End Class


End Namespace

