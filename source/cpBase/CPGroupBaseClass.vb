'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPGroupBaseClass
        '
        ' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
        '
        Public MustOverride Sub Add(ByVal GroupName As String, Optional ByVal GroupCaption As String = "") 'Implements BaseClasses.CPGroupBaseClass.Add
        Public MustOverride Sub AddUser(ByVal GroupNameIdOrGuid As String, Optional ByVal UserId As Integer = 0, Optional ByVal DateExpires As Date = #12:00:00 AM#) 'Implements BaseClasses.CPGroupBaseClass.AddUser
        Public MustOverride Sub Delete(ByVal GroupNameIdOrGuid As String) 'Implements BaseClasses.CPGroupBaseClass.Delete
        Public MustOverride Function GetId(ByVal GroupNameIdOrGuid As String) As Integer 'Implements BaseClasses.CPGroupBaseClass.GetId
        Public MustOverride Function GetName(ByVal GroupNameIdOrGuid As String) As String 'Implements BaseClasses.CPGroupBaseClass.GetName
        Public MustOverride Sub RemoveUser(ByVal GroupNameIdOrGuid As String, Optional ByVal UserId As Integer = 0) 'Implements BaseClasses.CPGroupBaseClass.RemoveUser
    End Class

End Namespace

