'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPGroupBaseClass
        '
        ' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
        '
        Public MustOverride Sub Add(ByVal GroupName As String, Optional ByVal GroupCaption As String = "")
        Public MustOverride Sub AddUser(ByVal GroupNameIdOrGuid As String)
        Public MustOverride Sub AddUser(ByVal GroupNameIdOrGuid As String, ByVal UserId As Integer)
        Public MustOverride Sub AddUser(ByVal GroupNameIdOrGuid As String, ByVal UserId As Integer, ByVal DateExpires As Date)
        Public MustOverride Sub Delete(ByVal GroupNameIdOrGuid As String)
        Public MustOverride Function GetId(ByVal GroupNameIdOrGuid As String) As Integer
        Public MustOverride Function GetName(ByVal GroupNameIdOrGuid As String) As String
        Public MustOverride Sub RemoveUser(ByVal GroupNameIdOrGuid As String, Optional ByVal UserId As Integer = 0)
    End Class

End Namespace

