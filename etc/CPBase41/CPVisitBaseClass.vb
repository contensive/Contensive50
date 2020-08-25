'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPVisitBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Processor.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride ReadOnly Property CookieSupport() As Boolean 'Implements BaseClasses.CPVisitBaseClass.CookieSupport
        Public MustOverride Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitId As Integer = 0) As String 'Implements BaseClasses.CPVisitBaseClass.GetProperty
        Public MustOverride Function GetText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
        Public MustOverride Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
        Public MustOverride Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
        Public MustOverride Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
        'Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
        Public MustOverride ReadOnly Property Id() As Integer 'Implements BaseClasses.CPVisitBaseClass.Id
        Public MustOverride ReadOnly Property LastTime() As Date 'Implements BaseClasses.CPVisitBaseClass.LastTime
        Public MustOverride ReadOnly Property LoginAttempts() As Integer 'Implements BaseClasses.CPVisitBaseClass.LoginAttempts
        Public MustOverride ReadOnly Property Name() As String 'Implements BaseClasses.CPVisitBaseClass.Name
        Public MustOverride ReadOnly Property Pages() As Integer 'Implements BaseClasses.CPVisitBaseClass.Pages
        Public MustOverride ReadOnly Property Referer() As String 'Implements BaseClasses.CPVisitBaseClass.Referer
        Public MustOverride Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitId As Integer = 0) 'Implements BaseClasses.CPVisitBaseClass.SetProperty
        Public MustOverride ReadOnly Property StartDateValue() As Integer 'Implements BaseClasses.CPVisitBaseClass.StartDateValue
        Public MustOverride ReadOnly Property StartTime() As Date 'Implements BaseClasses.CPVisitBaseClass.StartTime
    End Class

End Namespace

