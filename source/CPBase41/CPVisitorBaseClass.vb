'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPVisitorBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Processor.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride ReadOnly Property ForceBrowserMobile() As Boolean 'Implements BaseClasses.CPVisitorBaseClass.ForceBrowserMobile
        Public MustOverride Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitorId As Integer = 0) As String 'Implements BaseClasses.CPVisitorBaseClass.GetProperty
        Public MustOverride Function GetText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
        Public MustOverride Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
        Public MustOverride Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
        Public MustOverride Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
        'Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
        Public MustOverride ReadOnly Property Id() As Integer 'Implements BaseClasses.CPVisitorBaseClass.Id
        Public MustOverride ReadOnly Property IsNew() As Boolean 'Implements BaseClasses.CPVisitorBaseClass.IsNew
        Public MustOverride Sub SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitorid As Integer = 0) 'Implements BaseClasses.CPVisitorBaseClass.SetProperty
        Public MustOverride ReadOnly Property UserId() As Integer 'Implements BaseClasses.CPVisitorBaseClass.UserId
    End Class

End Namespace

