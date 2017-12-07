'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPDocBaseClass
        Public MustOverride Property Content() As String
        Public MustOverride ReadOnly Property NavigationStructure() As String
        Public MustOverride Property NoFollow() As Boolean
        Public MustOverride ReadOnly Property PageId() As Integer
        Public MustOverride ReadOnly Property PageName() As String
        Public MustOverride ReadOnly Property RefreshQueryString() As String
        Public MustOverride ReadOnly Property SectionId() As Integer
        Public MustOverride ReadOnly Property StartTime() As Date
        Public MustOverride ReadOnly Property TemplateId() As Integer
        Public MustOverride ReadOnly Property Type() As String
        Public MustOverride Sub AddHeadStyle(ByVal StyleSheet As String)
        Public MustOverride Sub AddHeadStyleLink(ByVal StyleSheetLink As String)
        Public MustOverride Sub AddHeadJavascript(ByVal NewCode As String)
        Public MustOverride Sub AddHeadTag(ByVal HeadTag As String)
        Public MustOverride Sub AddMetaDescription(ByVal MetaDescription As String)
        Public MustOverride Sub AddMetaKeywordList(ByVal MetaKeywordList As String)
        Public MustOverride Sub AddOnLoadJavascript(ByVal NewCode As String)
        Public MustOverride Sub AddTitle(ByVal PageTitle As String)
        Public MustOverride Sub AddRefreshQueryString(ByVal Name As String, ByVal Value As String)
        Public MustOverride Sub AddBodyEnd(ByVal NewCode As String)
        Public MustOverride Property Body() As String
        Public MustOverride ReadOnly Property SiteStylesheet() As String
        Public MustOverride Sub SetProperty(ByVal FieldName As String, ByVal FieldValue As String)
        Public MustOverride Function GetProperty(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As String
        Public MustOverride Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
        Public MustOverride Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
        Public MustOverride Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
        Public MustOverride Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
        Public MustOverride Function IsProperty(ByVal PropertyName As String) As Boolean
        Public MustOverride ReadOnly Property IsAdminSite() As Boolean
        ' deprecated - replaced with Setproperty because c-sharp does not support properties with arguments
        '----------------------------------------------------------------------------------------------------
        'Public MustOverride Property GlobalVar(ByVal Index As String) As String
        Public MustOverride Function get_GlobalVar(ByVal Index As String) As String
        Public MustOverride Sub set_GlobalVar(ByVal Index As String, ByVal Value As String)
        '----------------------------------------------------------------------------------------------------
        'Public MustOverride ReadOnly Property IsGlobalVar(ByVal Index As String) As Boolean
        Public MustOverride Function get_IsGlobalVar(ByVal Index As String) As Boolean
        '----------------------------------------------------------------------------------------------------
        'Public MustOverride ReadOnly Property IsVar(ByVal Index As String) As Boolean
        Public MustOverride Function get_IsVar(ByVal Index As String) As Boolean
        '----------------------------------------------------------------------------------------------------
        'Public MustOverride Property Var(ByVal Index As String) As String
        Public MustOverride Function get_Var(ByVal Index As String) As String
        Public MustOverride Sub Var(ByVal Index As String, ByVal Value As String)

    End Class

End Namespace

