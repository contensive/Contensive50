'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPHtmlBaseClass

        Enum EditorContentScope
            Page = 1
            Email = 2
            PageTemplate = 3
        End Enum
        '
        Enum EditorUserScope
            Developer = 1
            Administrator = 2
            ContentManager = 3
            PublicUser = 4
            CurrentUser = 5
        End Enum
        'Public Sub New(ByVal cmcObj As Contensive.Core.cpCoreClass, ByRef CPParent As CPBaseClass)
        Public MustOverride Function div(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.div
        Public MustOverride Function p(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.p
        Public MustOverride Function li(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.li
        Public MustOverride Function ul(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.ul
        Public MustOverride Function ol(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.ol
        Public MustOverride Function CheckBox(ByVal HtmlName As String, Optional ByVal HtmlValue As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.CheckBox
        Public MustOverride Function CheckList(ByVal HtmlName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordId As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectSQLCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal IsReadOnly As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.CheckList
        Public MustOverride Function Form(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal ActionQueryString As String = "", Optional ByVal Method As String = "post") As String 'Implements BaseClasses.CPHtmlBaseClass.Form
        Public MustOverride Function h1(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h1
        Public MustOverride Function h2(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h2
        Public MustOverride Function h3(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h3
        Public MustOverride Function h4(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h4
        Public MustOverride Function h5(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h5
        Public MustOverride Function h6(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.h6
        Public MustOverride Function RadioBox(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal CurrentValue As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.RadioBox
        Public MustOverride Function SelectContent(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.SelectContent
        Public MustOverride Function SelectList(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal OptionList As String, Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.SelectList
        Public MustOverride Sub ProcessCheckList(ByVal HtmlName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As String, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String) 'Implements BaseClasses.CPHtmlBaseClass.ProcessCheckList
        Public MustOverride Sub ProcessInputFile(ByVal HtmlName As String, Optional ByVal VirtualFilePath As String = "") 'Implements BaseClasses.CPHtmlBaseClass.ProcessInputFile
        Public MustOverride Function Hidden(ByVal HtmlName As String, ByVal HtmlValue As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.Hidden
        Public MustOverride Function InputDate(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.InputDate
        Public MustOverride Function InputFile(ByVal HtmlName As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.InputFile
        Public MustOverride Function InputText(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal IsPassword As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.InputText
        Public MustOverride Function InputTextExpandable(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal StyleWidth As String = "", Optional ByVal IsPassword As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.InputTextExpandable
        Public MustOverride Function SelectUser(ByVal HtmlName As String, ByVal HtmlValue As Integer, ByVal GroupId As Integer, Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.SelectUser
        Public MustOverride Function Indent(ByVal SourceHtml As String, Optional ByVal TabCnt As Integer = 1) As String 'Implements BaseClasses.CPHtmlBaseClass.Indent
        Public MustOverride Function InputWysiwyg(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal UserScope As BaseClasses.CPHtmlBaseClass.EditorUserScope = BaseClasses.CPHtmlBaseClass.EditorUserScope.CurrentUser, Optional ByVal ContentScope As BaseClasses.CPHtmlBaseClass.EditorContentScope = BaseClasses.CPHtmlBaseClass.EditorContentScope.Page, Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Implements BaseClasses.CPHtmlBaseClass.InputWysiwyg
        Public MustOverride Sub AddEvent(ByVal HtmlId As String, ByVal DOMEvent As String, ByVal JavaScript As String)
        Public MustOverride Function Button(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String
        Public MustOverride Function adminHint(ByVal innerHtml As String) As String
    End Class

End Namespace

