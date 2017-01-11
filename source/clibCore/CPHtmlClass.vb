Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPHtmlClass.ClassId, CPHtmlClass.InterfaceId, CPHtmlClass.EventsId)> _
    Public Class CPHtmlClass
        Inherits BaseClasses.CPHtmlBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "637E3815-0DA6-4672-84E9-A319D85F2101"
        Public Const InterfaceId As String = "24267471-9CE4-44F9-B4BD-8E9CE357D6E6"
        Public Const EventsId As String = "4021B791-0F55-4841-90AE-64C7FAFB9756"
#End Region
        '
        Private cp As CPClass
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        ' Constructor
        '
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference cp, main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cp = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '
        '
        Private Function BlockBase(ByVal TagName As String, ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String
            Dim s As String = ""
            '
            If HtmlName <> "" Then
                s += " name=""" & HtmlName & """"
            End If
            If HtmlClass <> "" Then
                s += " class=""" & HtmlClass & """"
            End If
            If HtmlId <> "" Then
                s += " id=""" & HtmlId & """"
            End If
            Return "<" & TagName.Trim & s & ">" & InnerHtml & "</" & TagName.Trim & ">"
        End Function
        '
        '
        '
        Public Overrides Function div(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.div
            Return BlockBase("div", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function p(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.p
            Return BlockBase("p", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function li(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.li
            Return BlockBase("li", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function ul(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.ul
            Return BlockBase("ul", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function ol(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.ol
            Return BlockBase("ol", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function CheckBox(ByVal HtmlName As String, Optional ByVal HtmlValue As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.CheckBox
            If True Then
                Return cpCore.html_GetFormInputCheckBox(HtmlName, HtmlValue.ToString, HtmlId)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function CheckList(ByVal HtmlName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordId As Integer, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String, Optional ByVal SecondaryContentSelectSQLCriteria As String = "", Optional ByVal CaptionFieldName As String = "", Optional ByVal IsReadOnly As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.CheckList
            If True Then
                Return cpCore.main_GetFormInputCheckList(HtmlName, PrimaryContentName, PrimaryRecordId, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectSQLCriteria, CaptionFieldName, IsReadOnly)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function Form(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal ActionQueryString As String = "", Optional ByVal Method As String = "post") As String 'Inherits BaseClasses.CPHtmlBaseClass.Form
            Try
                Dim FormStart As String
                '
                If Method.ToLower = "get" Then
                    If (InnerHtml.IndexOf("type=""file", 0, 1, StringComparison.OrdinalIgnoreCase) >= 0) Then
                        Call cp.core.handleExceptionAndRethrow(New ApplicationException("cp.html.form called with method=get can not contain an upload file (input type=file)"))
                    End If
                    If ActionQueryString = "" Then
                        FormStart = cpCore.html_GetFormStart(, HtmlName, HtmlId, Method)
                    Else
                        FormStart = cpCore.html_GetFormStart(ActionQueryString, HtmlName, HtmlId, Method)
                    End If

                Else
                    If ActionQueryString = "" Then
                        FormStart = cpCore.html_GetUploadFormStart()
                    Else
                        FormStart = cpCore.html_GetUploadFormStart(ActionQueryString)
                    End If
                    If HtmlName <> "" Then
                        FormStart = FormStart.Replace(">", " name=""" & HtmlName & """>")
                    End If
                    If HtmlClass <> "" Then
                        FormStart = FormStart.Replace(">", " class=""" & HtmlClass & """>")
                    End If
                    If HtmlId <> "" Then
                        FormStart = FormStart.Replace(">", " id=""" & HtmlId & """>")
                    End If
                End If
                Return "" _
                    & FormStart _
                    & InnerHtml _
                    & "</form>"
            Catch ex As Exception

            End Try
        End Function
        '
        '
        '
        Public Overrides Function h1(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h1
            Return BlockBase("h1", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function h2(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h2
            Return BlockBase("h2", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function h3(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h3
            Return BlockBase("h3", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function h4(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h4
            Return BlockBase("h4", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function h5(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h5
            Return BlockBase("h5", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function
        '
        '
        '
        Public Overrides Function h6(ByVal InnerHtml As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.h6
            Return BlockBase("h6", InnerHtml, HtmlName, HtmlClass, HtmlId)
        End Function

        Public Overrides Function RadioBox(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal CurrentValue As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.RadioBox
            If True Then
                Return cpCore.html_GetFormInputRadioBox(HtmlName, HtmlValue, CurrentValue, HtmlId)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function SelectContent(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.SelectContent
            If True Then
                SelectContent = cpCore.main_GetFormInputSelect(HtmlName, EncodeInteger(HtmlValue), ContentName, SQLCriteria, NoneCaption)
                If HtmlClass <> "" Then
                    SelectContent = SelectContent.Replace("<select ", "<select class=""" & HtmlClass & """ ")
                End If
                If HtmlId <> "" Then
                    SelectContent = SelectContent.Replace("<select ", "<select id=""" & HtmlId & """ ")
                End If
            Else
                SelectContent = ""
            End If
            Return SelectContent
        End Function

        Public Overrides Function SelectList(ByVal HtmlName As String, ByVal HtmlValue As String, ByVal OptionList As String, Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.SelectList
            If True Then
                SelectList = cpCore.main_GetFormInputSelectList(HtmlName, HtmlValue, OptionList, NoneCaption, HtmlId)
                If HtmlClass <> "" Then
                    SelectList = SelectList.Replace("<select ", "<select class=""" & HtmlClass & """ ")
                End If
                Return SelectList
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Sub ProcessCheckList(ByVal HtmlName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As String, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String) 'Inherits BaseClasses.CPHtmlBaseClass.ProcessCheckList
            If True Then
                Call cpCore.main_ProcessCheckList(HtmlName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub ProcessInputFile(ByVal HtmlName As String, Optional ByVal VirtualFilePath As String = "") 'Inherits BaseClasses.CPHtmlBaseClass.ProcessInputFile
            If True Then
                Call cpCore.web_processFormInputFile(HtmlName, VirtualFilePath)
            End If
        End Sub
        '
        '
        '
        Public Overrides Function Hidden(ByVal HtmlName As String, ByVal HtmlValue As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.Hidden
            If True Then
                Return cpCore.html_GetFormInputHidden(HtmlName, HtmlValue, HtmlId)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function InputDate(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.InputDate
            Dim returnValue As String = ""
            If True Then
                returnValue = cpCore.html_GetFormInputDate(HtmlName, HtmlValue, Width, HtmlId)
                If HtmlClass <> "" Then
                    returnValue = returnValue.Replace(">", " class=""" & HtmlClass & """>")
                End If
            End If
            Return returnValue
        End Function
        '
        '
        '
        Public Overrides Function InputFile(ByVal HtmlName As String, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String
            Dim returnValue As String = ""
            If True Then
                returnValue = cpCore.html_GetFormInputFile2(HtmlName, HtmlId, HtmlClass)
            End If
            Return returnValue
        End Function
        '
        '
        '
        Public Overrides Function InputText(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal IsPassword As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.InputText
            Dim returnValue As String = ""
            If True Then
                returnValue = cpCore.html_GetFormInputText2(HtmlName, HtmlValue, EncodeInteger(Height), EncodeInteger(Width), HtmlId, IsPassword, False, HtmlClass)
                returnValue = returnValue.Replace(" SIZE=""60""", "")
            End If
            Return returnValue
        End Function
        ''
        ''
        ''
        'Public Overrides Function InputField(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlStyle As String = "", Optional ByVal ManyToManySourceRecordID As Integer = 0) As String
        '    If True Then
        '        Return cmc.main_GetFormInputField(ContentName, FieldName, HtmlName, HtmlValue, HtmlClass, HtmlId, HtmlClass, ManyToManySourceRecordID)
        '    Else
        '        Return ""
        '    End If
        'End Function

        '
        '
        Public Overrides Function InputTextExpandable(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal Rows As Integer = 0, Optional ByVal StyleWidth As String = "", Optional ByVal IsPassword As Boolean = False, Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.InputTextExpandable
            If True Then
                Return cpCore.html_GetFormInputTextExpandable(HtmlName, HtmlValue, Rows, StyleWidth, HtmlId, IsPassword)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function SelectUser(ByVal HtmlName As String, ByVal HtmlValue As Integer, ByVal GroupId As Integer, Optional ByVal NoneCaption As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.SelectUser
            If True Then
                Return cpCore.main_GetFormInputMemberSelect(HtmlName, HtmlValue, GroupId, NoneCaption, HtmlId)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function Indent(ByVal SourceHtml As String, Optional ByVal TabCnt As Integer = 1) As String 'Inherits BaseClasses.CPHtmlBaseClass.Indent
            '
            '   Indent every line by 1 tab
            '
            Dim posStart As Integer
            Dim posEnd As Integer
            Dim pre As String
            Dim post As String
            Dim target As String
            '
            posStart = vbInstr(1, SourceHtml, "<![CDATA[", CompareMethod.Text)
            If posStart = 0 Then
                '
                ' no cdata
                '
                posStart = vbInstr(1, SourceHtml, "<textarea", CompareMethod.Text)
                If posStart = 0 Then
                    '
                    ' no textarea
                    '
                    If TabCnt > 0 And TabCnt < 99 Then
                        Indent = SourceHtml.Replace(vbCrLf, vbCrLf & New String(CChar(vbTab), TabCnt))
                    Else
                        Indent = SourceHtml.Replace(vbCrLf, vbCrLf & vbTab)
                    End If
                    'Indent = vbReplace(SourceHtml, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
                Else
                    '
                    ' text area found, isolate it and indent before and after
                    '
                    posEnd = vbInstr(posStart, SourceHtml, "</textarea>", CompareMethod.Text)
                    pre = Mid(SourceHtml, 1, posStart - 1)
                    If posEnd = 0 Then
                        target = Mid(SourceHtml, posStart)
                        post = ""
                    Else
                        target = Mid(SourceHtml, posStart, posEnd - posStart + Len("</textarea>"))
                        post = Mid(SourceHtml, posEnd + Len("</textarea>"))
                    End If
                    Indent = Indent(pre, TabCnt) & target & Indent(post, TabCnt)
                End If
            Else
                '
                ' cdata found, isolate it and indent before and after
                '
                posEnd = vbInstr(posStart, SourceHtml, "]]>", CompareMethod.Text)
                pre = Mid(SourceHtml, 1, posStart - 1)
                If posEnd = 0 Then
                    target = Mid(SourceHtml, posStart)
                    post = ""
                Else
                    target = Mid(SourceHtml, posStart, posEnd - posStart + Len("]]>"))
                    post = Mid(SourceHtml, posEnd + 3)
                End If
                Indent = Indent(pre, TabCnt) & target & Indent(post, TabCnt)
            End If
            'If TabCnt > 0 And TabCnt < 99 Then
            'Return SourceHtml.Replace(vbCrLf, vbCrLf & New String(vbTab, TabCnt))
            'Else
            'Return SourceHtml.Replace(vbCrLf, vbCrLf & vbTab)
            'End If
        End Function

        Public Overrides Function InputWysiwyg(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal UserScope As BaseClasses.CPHtmlBaseClass.EditorUserScope = BaseClasses.CPHtmlBaseClass.EditorUserScope.CurrentUser, Optional ByVal ContentScope As BaseClasses.CPHtmlBaseClass.EditorContentScope = BaseClasses.CPHtmlBaseClass.EditorContentScope.Page, Optional ByVal Height As String = "", Optional ByVal Width As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String 'Inherits BaseClasses.CPHtmlBaseClass.InputWysiwyg
            If True Then
                Return cpCore.html_GetFormInputHTML(HtmlName, HtmlValue, Height, Width)
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Sub AddEvent(ByVal HtmlId As String, ByVal DOMEvent As String, ByVal JavaScript As String)
            If True Then
                Call cpCore.html_AddEvent(HtmlId, DOMEvent, JavaScript)
            End If
        End Sub
        '
        '
        '
        Public Overrides Function Button(ByVal HtmlName As String, Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "") As String
            If True Then
                Button = cpCore.html_GetFormButton(HtmlValue, HtmlName, HtmlId, "")
                If HtmlClass <> "" Then
                    Button = Button.Replace(">", " class=""" & HtmlClass & """>")
                End If
            Else
                Return ""
            End If
        End Function
        '
        '
        '
        Public Overrides Function adminHint(innerHtml As String) As String
            Dim returnString As String = innerHtml
            Try
                If cp.User.IsEditingAnything() Or cp.User.IsAdmin() Then
                    returnString = "" _
                        & "<div class=""ccHintWrapper"">" _
                            & "<div  class=""ccHintWrapperContent"">" _
                            & "<b>Administrator</b>" _
                            & "<BR>" _
                            & "<BR>" & cp.Utils.EncodeText(innerHtml) _
                            & "</div>" _
                        & "</div>"
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected error in cp.html.adminHint()")
            End Try
            Return returnString
        End Function
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.html, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
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