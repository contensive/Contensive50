'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses


    Public MustInherit Class CPSiteBaseClass
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride ReadOnly Property Name() As String
        Public MustOverride Sub SetProperty(ByVal FieldName As String, ByVal FieldValue As String)
        Public MustOverride Function GetProperty(ByVal FieldName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
        Public MustOverride Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
        Public MustOverride Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
        Public MustOverride Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
        Public MustOverride ReadOnly Property MultiDomainMode() As Boolean
        Public MustOverride ReadOnly Property PhysicalFilePath() As String
        Public MustOverride ReadOnly Property PhysicalInstallPath() As String
        Public MustOverride ReadOnly Property PhysicalWWWPath() As String
        Public MustOverride ReadOnly Property TrapErrors() As Boolean
        Public MustOverride ReadOnly Property AppPath() As String
        Public MustOverride ReadOnly Property AppRootPath() As String
        Public MustOverride ReadOnly Property DomainPrimary() As String
        Public MustOverride ReadOnly Property Domain() As String
        Public MustOverride ReadOnly Property DomainList() As String
        Public MustOverride ReadOnly Property FilePath() As String
        Public MustOverride ReadOnly Property PageDefault() As String
        Public MustOverride ReadOnly Property VirtualPath() As String
        Public MustOverride Function EncodeAppRootPath(ByVal Link As String) As String
        Public MustOverride Function IsTesting() As Boolean
        Public MustOverride Sub LogActivity(ByVal Message As String, ByVal UserID As Integer, ByVal OrganizationID As Integer)
        Public MustOverride Sub ErrorReport(ByVal Message As String)
        Public MustOverride Sub ErrorReport(ByVal Ex As System.Exception, Optional ByVal Message As String = "")
        ' 20151121 - not needed, removed to resolve compile issue with com compatibility
        'Public MustOverride Sub ErrorReport(ByVal Err As Microsoft.VisualBasic.ErrObject, Optional ByVal Message As String = "")
        Public MustOverride Sub RequestTask(ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String)
        Public MustOverride Sub TestPoint(ByVal Message As String)
        Public MustOverride Function LandingPageId(Optional ByVal DomainName As String = "") As Integer
        Public MustOverride Sub LogWarning(ByVal name As String, ByVal description As String, ByVal typeOfWarningKey As String, ByVal instanceKey As String)
        Public MustOverride Sub LogAlarm(ByVal cause As String)
        Public MustOverride Sub addLinkAlias(ByVal linkAlias As String, ByVal pageId As Integer, Optional ByVal queryStringSuffix As String = "")
        Public MustOverride Function ThrowEvent(ByVal eventNameIdOrGuid As String) As String
    End Class

End Namespace
