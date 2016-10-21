'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPUserErrorBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride Sub Add(ByVal Message As String) 'Implements BaseClasses.CPUserErrorBaseClass.Add
        Public MustOverride Function GetList() As String 'Implements BaseClasses.CPUserErrorBaseClass.GetList
        Public MustOverride Function OK() As Boolean 'Implements BaseClasses.CPUserErrorBaseClass.OK
    End Class

End Namespace

