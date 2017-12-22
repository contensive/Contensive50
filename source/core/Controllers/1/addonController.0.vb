
Option Explicit On
Option Strict On

Imports System.Reflection
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' - first routine should be constructor
    ''' - disposable region at end
    ''' - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
    ''' </summary>
    Public Class addonController
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '===============================================================================================================================================
        '   cpcore.main_Get the editable options bubble
        '       ACInstanceID required
        '       ACInstanceID = -1 means this Add-on does not support instance options (like end-of-page scope, etc)
        ' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        '===============================================================================================================================================
        '
        Public Function getInstanceBubble(ByVal AddonName As String, ByVal Option_String As String, ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String, ByVal ACInstanceID As String, ByVal Context As CPUtilsBaseClass.addonContext, ByRef return_DialogList As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("addon_execute_GetInstanceBubble")
            '
            Dim Dialog As String
            Dim OptionDefault As String
            Dim OptionSuffix As String
            Dim OptionCnt As Integer
            Dim OptionValue_AddonEncoded As String
            Dim OptionValue As String
            Dim OptionCaption As String
            Dim LCaseOptionDefault As String
            Dim OptionValues() As String
            Dim FormInput As String
            Dim OptionPtr As Integer
            Dim QueryString As String
            Dim LocalCode As String = String.Empty
            Dim CopyHeader As String = String.Empty
            Dim CopyContent As String = String.Empty
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            '
            If cpCore.doc.authContext.isAuthenticated() And ((ACInstanceID = "-2") Or (ACInstanceID = "-1") Or (ACInstanceID = "0") Or (RecordID <> 0)) Then
                If cpCore.doc.authContext.isEditingAnything() Then
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">Options for this instance of " & AddonName & "</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.doc.helpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                        & "</tr>" _
                        & "</table>" _
                        & "</div>"
                    If (Option_String = "") Then
                        '
                        ' no option string - no settings to display
                        '
                        CopyContent = "This Add-on has no instance options."
                        CopyContent = "<div style=""width:400px;background-color:transparent"" class=""ccAdminSmall"">" & CopyContent & "</div>"
                    ElseIf (ACInstanceID = "0") Or (ACInstanceID = "-1") Then
                        '
                        ' This addon does not support bubble option setting
                        '
                        CopyContent = "This addon does not support instance options."
                        CopyContent = "<div style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & CopyContent & "</div>"
                        'ElseIf (Context <> CPUtilsBaseClass.addonContext.ContextAdmin) And (cpCore.siteProperties.allowWorkflowAuthoring And Not cpCore.visitProperty.getBoolean("AllowWorkflowRendering")) Then
                        '    '
                        '    ' workflow with no rendering (or within admin site)
                        '    '
                        '    CopyContent = "With Workflow editing enabled, you can not edit Add-on settings for live records. To make changes to the editable version of this page, turn on Render Workflow Authoring Changes and Advanced Edit together."
                        '    CopyContent = "<div style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & CopyContent & "</div>"
                    ElseIf ACInstanceID = "" Then
                        '
                        ' No instance ID - must be edited and saved
                        '
                        CopyContent = "You can not edit instance options for Add-ons on this page until the page is upgraded. To upgrade, edit and save the page."
                        CopyContent = "<div style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & CopyContent & "</div>"
                    Else
                        '
                        ' ACInstanceID is -2 (Admin Root), or Rnd (from an instance on a page) Editable Form
                        '
                        CopyContent = CopyContent _
                            & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                            & ""
                        OptionSplit = Split(Option_String, vbCrLf)
                        For Ptr = 0 To UBound(OptionSplit)
                            '
                            ' Process each option row
                            '
                            OptionName = OptionSplit(Ptr)
                            OptionSuffix = ""
                            OptionDefault = ""
                            LCaseOptionDefault = ""
                            OptionSelector = ""
                            Pos = genericController.vbInstr(1, OptionName, "=")
                            If Pos <> 0 Then
                                If (Pos < Len(OptionName)) Then
                                    OptionSelector = Trim(Mid(OptionName, Pos + 1))
                                End If
                                OptionName = Trim(Left(OptionName, Pos - 1))
                            End If
                            OptionName = genericController.decodeNvaArgument(OptionName)
                            Pos = genericController.vbInstr(1, OptionSelector, "[")
                            If Pos <> 0 Then
                                '
                                ' List of Options, might be select, radio, checkbox, resourcelink
                                '
                                OptionDefault = Mid(OptionSelector, 1, Pos - 1)
                                OptionDefault = genericController.decodeNvaArgument(OptionDefault)
                                LCaseOptionDefault = genericController.vbLCase(OptionDefault)
                                'LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault)

                                OptionSelector = Mid(OptionSelector, Pos + 1)
                                Pos = genericController.vbInstr(1, OptionSelector, "]")
                                If Pos > 0 Then
                                    If Pos < Len(OptionSelector) Then
                                        OptionSuffix = genericController.vbLCase(Trim(Mid(OptionSelector, Pos + 1)))
                                    End If
                                    OptionSelector = Mid(OptionSelector, 1, Pos - 1)
                                End If
                                OptionValues = Split(OptionSelector, "|")
                                FormInput = ""
                                OptionCnt = UBound(OptionValues) + 1
                                For OptionPtr = 0 To OptionCnt - 1
                                    OptionValue_AddonEncoded = Trim(OptionValues(OptionPtr))
                                    If OptionValue_AddonEncoded <> "" Then
                                        Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":")
                                        If Pos = 0 Then
                                            OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded)
                                            OptionCaption = OptionValue
                                        Else
                                            OptionCaption = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, 1, Pos - 1))
                                            OptionValue = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, Pos + 1))
                                        End If
                                        Select Case OptionSuffix
                                            Case "checkbox"
                                                '
                                                ' Create checkbox FormInput
                                                '
                                                If genericController.vbInstr(1, "," & LCaseOptionDefault & ",", "," & genericController.vbLCase(OptionValue) & ",") <> 0 Then
                                                    FormInput = FormInput & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & OptionName & OptionPtr & """ value=""" & OptionValue & """ checked=""checked"">" & OptionCaption & "</div>"
                                                Else
                                                    FormInput = FormInput & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & OptionName & OptionPtr & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                                End If
                                            Case "radio"
                                                '
                                                ' Create Radio FormInput
                                                '
                                                If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                                    FormInput = FormInput & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & OptionName & """ value=""" & OptionValue & """ checked=""checked"" >" & OptionCaption & "</div>"
                                                Else
                                                    FormInput = FormInput & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & OptionName & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                                End If
                                            Case Else
                                                '
                                                ' Create select FormInput
                                                '
                                                If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                                    FormInput = FormInput & "<option value=""" & OptionValue & """ selected>" & OptionCaption & "</option>"
                                                Else
                                                    OptionCaption = genericController.vbReplace(OptionCaption, vbCrLf, " ")
                                                    FormInput = FormInput & "<option value=""" & OptionValue & """>" & OptionCaption & "</option>"
                                                End If
                                        End Select
                                    End If
                                Next
                                Select Case OptionSuffix
                                    '                            Case FieldTypeLink
                                    '                                '
                                    '                                ' ----- Link (href value
                                    '                                '
                                    '                                Return_NewFieldList = Return_NewFieldList & "," & FieldName
                                    '                                FieldValueText = genericController.encodeText(FieldValueVariant)
                                    '                                EditorString = "" _
                                    '                                    & cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                    '                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>" _
                                    '                                    & "&nbsp;<a href=""#"" onClick=""OpenSiteExplorerWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/PageLink1616.gif"" width=16 height=16 border=0 alt=""Link to a page"" title=""Link to a page""></a>"
                                    '                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                    '                            Case FieldTypeResourceLink
                                    '                                '
                                    '                                ' ----- Resource Link (src value)
                                    '                                '
                                    '                                Return_NewFieldList = Return_NewFieldList & "," & FieldName
                                    '                                FieldValueText = genericController.encodeText(FieldValueVariant)
                                    '                                EditorString = "" _
                                    '                                    & cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80, FormFieldLCaseName) _
                                    '                                    & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & FormFieldLCaseName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>"
                                    '                                'EditorString = cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
                                    '                                s.Add( "<td class=""ccAdminEditField""><nobr>" & SpanClassAdminNormal & EditorString & "</span></nobr></td>")
                                    Case "resourcelink"
                                        '
                                        ' Create text box linked to resource library
                                        '
                                        OptionDefault = genericController.decodeNvaArgument(OptionDefault)
                                        FormInput = "" _
                                            & cpCore.html.html_GetFormInputText2(OptionName, OptionDefault, 1, 20) _
                                            & "&nbsp;<a href=""#"" onClick=""OpenResourceLinkWindow( '" & OptionName & "' ) ;return false;""><img src=""/ccLib/images/ResourceLink1616.gif"" width=16 height=16 border=0 alt=""Link to a resource"" title=""Link to a resource""></a>"
                                        'EditorString = cpcore.main_GetFormInputText2(FormFieldLCaseName, FieldValueText, 1, 80)
                                    Case "checkbox"
                                        '
                                        '
                                        CopyContent = CopyContent & "<input type=""hidden"" name=""" & OptionName & "CheckBoxCnt"" value=""" & OptionCnt & """ >"
                                    Case "radio"
                                        '
                                        ' Create Radio FormInput
                                        '
                                    Case Else
                                        '
                                        ' Create select FormInput
                                        '
                                        FormInput = "<select name=""" & OptionName & """>" & FormInput & "</select>"
                                End Select
                            Else
                                '
                                ' Create Text FormInput
                                '

                                OptionSelector = genericController.decodeNvaArgument(OptionSelector)
                                FormInput = cpCore.html.html_GetFormInputText2(OptionName, OptionSelector, 1, 20)
                            End If
                            CopyContent = CopyContent _
                                & "<tr>" _
                                & "<td class=""bbLeft"">" & OptionName & "</td>" _
                                & "<td class=""bbRight"">" & FormInput & "</td>" _
                                & "</tr>"
                        Next
                        CopyContent = "" _
                            & CopyContent _
                            & "</table>" _
                            & cpCore.html.html_GetFormInputHidden("Type", FormTypeAddonSettingsEditor) _
                            & cpCore.html.html_GetFormInputHidden("ContentName", ContentName) _
                            & cpCore.html.html_GetFormInputHidden("RecordID", RecordID) _
                            & cpCore.html.html_GetFormInputHidden("FieldName", FieldName) _
                            & cpCore.html.html_GetFormInputHidden("ACInstanceID", ACInstanceID)
                    End If
                    '
                    BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.doc.helpCodeCount & "',this);return false;"""
                    QueryString = cpCore.doc.refreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & cpCore.html.html_GetUploadFormStart() _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.doc.helpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccButtonCon"">" & cpCore.html.html_GetFormButton("Update", "HelpBubbleButton") & "</td></tr>" _
                        & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                        & "</table>" _
                        & "</form>" _
                        & "</div>"
                    getInstanceBubble = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 target=""_blank""" & BubbleJS & ">" _
                        & GetIconSprite("", 0, "/ccLib/images/toolsettings.png", 22, 22, "Edit options used just for this instance of the " & AddonName & " Add-on", "Edit options used just for this instance of the " & AddonName & " Add-on", "", True, "") _
                        & "</a>" _
                        & "" _
                        & ""
                    If cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize Then
                        cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10
                        ReDim Preserve cpCore.doc.helpCodes(cpCore.doc.helpCodeSize)
                        ReDim Preserve cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize)
                    End If
                    cpCore.doc.helpCodes(cpCore.doc.helpCodeCount) = LocalCode
                    cpCore.doc.helpCaptions(cpCore.doc.helpCodeCount) = AddonName
                    cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1
                    '
                    If cpCore.doc.helpDialogCnt = 0 Then
                        Call cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("addon_execute_GetInstanceBubble")
        End Function
        '
        '===============================================================================================================================================
        '   cpcore.main_Get Addon Styles Bubble Editor
        '===============================================================================================================================================
        '
        Public Function getAddonStylesBubble(ByVal addonId As Integer, ByRef return_DialogList As String) As String
            Dim result As String = String.Empty
            Try
                'Dim DefaultStylesheet As String = String.Empty
                'Dim StyleSheet As String = String.Empty
                Dim QueryString As String
                Dim LocalCode As String = String.Empty
                Dim CopyHeader As String = String.Empty
                Dim CopyContent As String
                Dim BubbleJS As String
                'Dim AddonName As String = String.Empty
                '
                If cpCore.doc.authContext.isAuthenticated() And True Then
                    If cpCore.doc.authContext.isEditingAnything() Then
                        Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.create(cpCore, addonId)
                        CopyHeader = CopyHeader _
                            & "<div class=""ccHeaderCon"">" _
                            & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                            & "<tr>" _
                            & "<td align=left class=""bbLeft"">Stylesheet for " & addon.name & "</td>" _
                            & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.doc.helpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                            & "</tr>" _
                            & "</table>" _
                            & "</div>"
                        CopyContent = "" _
                            & "" _
                            & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                            & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccContentCon ccAdminSmall"">These stylesheets will be added to all pages that include this add-on. The default stylesheet comes with the add-on, and can not be edited.</td></tr>" _
                            & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Custom Stylesheet</b>" & cpCore.html.html_GetFormInputTextExpandable2("CustomStyles", addon.StylesFilename.content, 10, "400px") & "</td></tr>"
                        'If DefaultStylesheet = "" Then
                        '    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>There are no default styles for this add-on.</td></tr>"
                        'Else
                        '    CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>" & cpCore.html.html_GetFormInputTextExpandable2("DefaultStyles", DefaultStylesheet, 10, "400px", , , True) & "</td></tr>"
                        'End If
                        CopyContent = "" _
                        & CopyContent _
                        & "</tr>" _
                        & "</table>" _
                        & cpCore.html.html_GetFormInputHidden("Type", FormTypeAddonStyleEditor) _
                        & cpCore.html.html_GetFormInputHidden("AddonID", addonId) _
                        & ""
                        '
                        BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.doc.helpCodeCount & "',this);return false;"""
                        QueryString = cpCore.doc.refreshQueryString
                        QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                        'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                        Dim Dialog As String = String.Empty

                        Dialog = Dialog _
                            & "<div class=""ccCon helpDialogCon"">" _
                            & cpCore.html.html_GetUploadFormStart() _
                            & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.doc.helpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                            & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                            & "<tr><td class=""ccButtonCon"">" & cpCore.html.html_GetFormButton("Update", "HelpBubbleButton") & "</td></tr>" _
                            & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                            & "</table>" _
                            & "</form>" _
                            & "</div>"
                        return_DialogList = return_DialogList & Dialog
                        result = "" _
                            & "&nbsp;<a href=""#"" tabindex=-1 target=""_blank""" & BubbleJS & ">" _
                            & GetIconSprite("", 0, "/ccLib/images/toolstyles.png", 22, 22, "Edit " & addon.name & " Stylesheets", "Edit " & addon.name & " Stylesheets", "", True, "") _
                            & "</a>"
                        If cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize Then
                            cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10
                            ReDim Preserve cpCore.doc.helpCodes(cpCore.doc.helpCodeSize)
                            ReDim Preserve cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize)
                        End If
                        cpCore.doc.helpCodes(cpCore.doc.helpCodeCount) = LocalCode
                        cpCore.doc.helpCaptions(cpCore.doc.helpCodeCount) = addon.name
                        cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '===============================================================================================================================================
        '   cpcore.main_Get inner HTML viewer Bubble
        '===============================================================================================================================================
        '

        Public Function getHelpBubble(ByVal addonId As Integer, ByVal helpCopy As String, ByVal CollectionID As Integer, ByRef return_DialogList As String) As String
            Dim result As String = ""
            Dim QueryString As String
            Dim LocalCode As String = String.Empty
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim AddonName As String = String.Empty
            Dim StyleSN As Integer
            Dim InnerCopy As String
            Dim CollectionCopy As String = String.Empty
            '
            If cpCore.doc.authContext.isAuthenticated() Then
                If cpCore.doc.authContext.isEditingAnything() Then
                    StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"))
                    'cpCore.html.html_HelpViewerButtonID = "HelpBubble" & doccontroller.htmlDoc_HelpCodeCount
                    InnerCopy = helpCopy
                    If InnerCopy = "" Then
                        InnerCopy = "<p style=""text-align:center"">No help is available for this add-on.</p>"
                    End If
                    '
                    If CollectionID <> 0 Then
                        CollectionCopy = cpCore.db.getRecordName("Add-on Collections", CollectionID)
                        If CollectionCopy <> "" Then
                            CollectionCopy = "This add-on is a member of the " & CollectionCopy & " collection."
                        Else
                            CollectionID = 0
                        End If
                    End If
                    If CollectionID = 0 Then
                        CollectionCopy = "This add-on is not a member of any collection."
                    End If
                    Dim CopyHeader As String = ""
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">Help Viewer</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.doc.helpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                        & "</tr>" _
                        & "</table>" _
                        & "</div>"
                    CopyContent = "" _
                        & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                        & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccAdminSmall""><p>" & CollectionCopy & "</p></td></tr>" _
                        & "<tr><td style=""width:400px;background-color:transparent;border:1px solid #fff;padding:10px;margin:5px;"">" & InnerCopy & "</td></tr>" _
                        & "</tr>" _
                        & "</table>" _
                        & ""
                    '
                    QueryString = cpCore.doc.refreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.doc.helpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                        & "</table>" _
                        & "</div>"
                    BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.doc.helpCodeCount & "',this);return false;"""
                    If cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize Then
                        cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10
                        ReDim Preserve cpCore.doc.helpCodes(cpCore.doc.helpCodeSize)
                        ReDim Preserve cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize)
                    End If
                    cpCore.doc.helpCodes(cpCore.doc.helpCodeCount) = LocalCode
                    cpCore.doc.helpCaptions(cpCore.doc.helpCodeCount) = AddonName
                    cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1
                    '
                    If cpCore.doc.helpDialogCnt = 0 Then
                        Call cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1
                    result = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 tarGet=""_blank""" & BubbleJS & " >" _
                        & GetIconSprite("", 0, "/ccLib/images/toolhelp.png", 22, 22, "View help resources for this Add-on", "View help resources for this Add-on", "", True, "") _
                        & "</a>"
                End If
            End If
            Return result
        End Function
        '
        '===============================================================================================================================================
        '   cpcore.main_Get inner HTML viewer Bubble
        '===============================================================================================================================================
        '
        Public Function getHTMLViewerBubble(ByVal addonId As Integer, ByVal HTMLSourceID As String, ByRef return_DialogList As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("addon_execute_GetHTMLViewerBubble")
            '
            Dim DefaultStylesheet As String
            Dim StyleSheet As String
            Dim OptionDefault As String
            Dim OptionSuffix As String
            Dim OptionCnt As Integer
            Dim OptionValue_AddonEncoded As String
            Dim OptionValue As String
            Dim OptionCaption As String
            Dim LCaseOptionDefault As String
            Dim OptionValues() As String
            Dim FormInput As String
            Dim OptionPtr As Integer
            Dim QueryString As String
            Dim LocalCode As String = String.Empty
            Dim CopyHeader As String = String.Empty
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim CS As Integer
            Dim AddonName As String = String.Empty
            Dim StyleSN As Integer
            Dim HTMLViewerBubbleID As String
            '
            If cpCore.doc.authContext.isAuthenticated() Then
                If cpCore.doc.authContext.isEditingAnything() Then
                    StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"))
                    HTMLViewerBubbleID = "HelpBubble" & cpCore.doc.helpCodeCount
                    '
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">HTML viewer</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('" & HTMLViewerBubbleID & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></A></td>" _
                        & "</tr>" _
                        & "</table>" _
                        & "</div>"
                    CopyContent = "" _
                        & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                        & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">This is the HTML produced by this add-on. Carrage returns and tabs have been added or modified to enhance readability.</td></tr>" _
                        & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & cpCore.html.html_GetFormInputTextExpandable2("DefaultStyles", "", 10, "400px", HTMLViewerBubbleID & "_dst", , False) & "</td></tr>" _
                        & "</tr>" _
                        & "</table>" _
                        & ""
                    '
                    QueryString = cpCore.doc.refreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""" & HTMLViewerBubbleID & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                        & "</table>" _
                        & "</div>"
                    BubbleJS = " onClick=""var d=document.getElementById('" & HTMLViewerBubbleID & "_dst');if(d){var s=document.getElementById('" & HTMLSourceID & "');if(s){d.value=s.innerHTML;HelpBubbleOn( '" & HTMLViewerBubbleID & "',this)}};return false;"" "
                    If cpCore.doc.helpCodeCount >= cpCore.doc.helpCodeSize Then
                        cpCore.doc.helpCodeSize = cpCore.doc.helpCodeSize + 10
                        ReDim Preserve cpCore.doc.helpCodes(cpCore.doc.helpCodeSize)
                        ReDim Preserve cpCore.doc.helpCaptions(cpCore.doc.helpCodeSize)
                    End If
                    cpCore.doc.helpCodes(cpCore.doc.helpCodeCount) = LocalCode
                    cpCore.doc.helpCaptions(cpCore.doc.helpCodeCount) = AddonName
                    cpCore.doc.helpCodeCount = cpCore.doc.helpCodeCount + 1
                    'SiteStylesBubbleCache = "x"
                    '
                    If cpCore.doc.helpDialogCnt = 0 Then
                        Call cpCore.html.addScriptCode_onLoad("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.doc.helpDialogCnt = cpCore.doc.helpDialogCnt + 1
                    getHTMLViewerBubble = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 target=""_blank""" & BubbleJS & " >" _
                        & GetIconSprite("", 0, "/ccLib/images/toolhtml.png", 22, 22, "View the source HTML produced by this Add-on", "View the source HTML produced by this Add-on", "", True, "") _
                        & "</A>"
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("addon_execute_GetHTMLViewerBubble")
        End Function
        '
        '
        '
        Private Function getFormContent(ByVal FormXML As String, ByRef return_ExitRequest As Boolean) As String
            Dim result As String = ""
            Try
                Dim FieldCount As Integer
                Dim RowMax As Integer
                Dim ColumnMax As Integer
                Dim SQLPageSize As Integer
                Dim ErrorNumber As Integer
                Dim ErrorDescription As String
                Dim dataArray As String(,)
                Dim RecordID As Integer
                Dim fieldfilename As String
                Dim FieldDataSource As String
                Dim FieldSQL As String
                Dim Content As New stringBuilderLegacyController
                Dim Copy As String
                Dim Button As String
                Dim Adminui As New adminUIController(cpCore)
                Dim ButtonList As String = String.Empty
                Dim Filename As String
                Dim NonEncodedLink As String
                Dim EncodedLink As String
                Dim VirtualFilePath As String
                Dim TabName As String
                Dim TabDescription As String
                Dim TabHeading As String
                Dim TabCnt As Integer
                Dim TabCell As stringBuilderLegacyController
                Dim FieldValue As String = String.Empty
                Dim FieldDescription As String
                Dim FieldDefaultValue As String
                Dim IsFound As Boolean
                Dim Name As String = String.Empty
                Dim Description As String = String.Empty
                Dim Doc As New XmlDocument
                Dim TabNode As XmlNode
                Dim SettingNode As XmlNode
                Dim CS As Integer
                Dim FieldName As String
                Dim FieldCaption As String
                Dim FieldAddon As String
                Dim FieldReadOnly As Boolean
                Dim FieldHTML As Boolean
                Dim fieldType As String
                Dim FieldSelector As String
                Dim DefaultFilename As String
                '
                Button = cpCore.docProperties.getText(RequestNameButton)
                If Button = ButtonCancel Then
                    '
                    ' Cancel just exits with no content
                    '
                    return_ExitRequest = True
                    Return String.Empty
                ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                    '
                    ' Not Admin Error
                    '
                    ButtonList = ButtonCancel
                    Content.Add(Adminui.GetFormBodyAdminOnly())
                Else
                    If True Then
                        Dim loadOK As Boolean
                        loadOK = True
                        Try

                            Doc.LoadXml(FormXML)
                        Catch ex As Exception
                            ' error
                            '
                            ButtonList = ButtonCancel
                            Content.Add("<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">There was a problem with the Setting Page you requested.</div>")
                            loadOK = False
                        End Try
                        '        CS = cpcore.main_OpenCSContentRecord("Setting Pages", SettingPageID)
                        '        If Not app.csv_IsCSOK(CS) Then
                        '            '
                        '            ' Setting Page was not found
                        '            '
                        '            ButtonList = ButtonCancel
                        '            Content.Add( "<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">The Setting Page you requested could not be found.</div>"
                        '        Else
                        '            XMLFile = app.cs_get(CS, "xmlfile")
                        '            Doc = New XmlDocument
                        'Doc.loadXML (XMLFile)
                        If loadOK Then
                        Else
                            '
                            ' data is OK
                            '
                            If genericController.vbLCase(Doc.DocumentElement.Name) <> "form" Then
                                '
                                ' error - Need a way to reach the user that submitted the file
                                '
                                ButtonList = ButtonCancel
                                Content.Add("<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">There was a problem with the Setting Page you requested.</div>")
                            Else
                                '
                                ' ----- Process Requests
                                '
                                If (Button = ButtonSave) Or (Button = ButtonOK) Then
                                    With Doc.DocumentElement
                                        For Each SettingNode In .ChildNodes
                                            Select Case genericController.vbLCase(SettingNode.Name)
                                                Case "tab"
                                                    For Each TabNode In SettingNode.ChildNodes
                                                        Select Case genericController.vbLCase(TabNode.Name)
                                                            Case "siteproperty"
                                                                '
                                                                FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                                FieldValue = cpCore.docProperties.getText(FieldName)
                                                                fieldType = xml_GetAttribute(IsFound, TabNode, "type", "")
                                                                Select Case genericController.vbLCase(fieldType)
                                                                    Case "integer"
                                                                        '
                                                                        If FieldValue <> "" Then
                                                                            FieldValue = genericController.EncodeInteger(FieldValue).ToString
                                                                        End If
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case "boolean"
                                                                        '
                                                                        If FieldValue <> "" Then
                                                                            FieldValue = genericController.EncodeBoolean(FieldValue).ToString
                                                                        End If
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case "float"
                                                                        '
                                                                        If FieldValue <> "" Then
                                                                            FieldValue = EncodeNumber(FieldValue).ToString
                                                                        End If
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case "date"
                                                                        '
                                                                        If FieldValue <> "" Then
                                                                            FieldValue = genericController.EncodeDate(FieldValue).ToString
                                                                        End If
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case "file", "imagefile"
                                                                        '
                                                                        If cpCore.docProperties.getBoolean(FieldName & ".DeleteFlag") Then
                                                                            Call cpCore.siteProperties.setProperty(FieldName, "")
                                                                        End If
                                                                        If FieldValue <> "" Then
                                                                            Filename = FieldValue
                                                                            VirtualFilePath = "Settings/" & FieldName & "/"
                                                                            cpCore.cdnFiles.upload(FieldName, VirtualFilePath, Filename)
                                                                            Call cpCore.siteProperties.setProperty(FieldName, VirtualFilePath & "/" & Filename)
                                                                        End If
                                                                    Case "textfile"
                                                                        '
                                                                        DefaultFilename = "Settings/" & FieldName & ".txt"
                                                                        Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename)
                                                                        If Filename = "" Then
                                                                            Filename = DefaultFilename
                                                                            Call cpCore.siteProperties.setProperty(FieldName, DefaultFilename)
                                                                        End If
                                                                        Call cpCore.appRootFiles.saveFile(Filename, FieldValue)
                                                                    Case "cssfile"
                                                                        '
                                                                        DefaultFilename = "Settings/" & FieldName & ".css"
                                                                        Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename)
                                                                        If Filename = "" Then
                                                                            Filename = DefaultFilename
                                                                            Call cpCore.siteProperties.setProperty(FieldName, DefaultFilename)
                                                                        End If
                                                                        Call cpCore.appRootFiles.saveFile(Filename, FieldValue)
                                                                    Case "xmlfile"
                                                                        '
                                                                        DefaultFilename = "Settings/" & FieldName & ".xml"
                                                                        Filename = cpCore.siteProperties.getText(FieldName, DefaultFilename)
                                                                        If Filename = "" Then
                                                                            Filename = DefaultFilename
                                                                            Call cpCore.siteProperties.setProperty(FieldName, DefaultFilename)
                                                                        End If
                                                                        Call cpCore.appRootFiles.saveFile(Filename, FieldValue)
                                                                    Case "currency"
                                                                        '
                                                                        If FieldValue <> "" Then
                                                                            FieldValue = EncodeNumber(FieldValue).ToString
                                                                            FieldValue = FormatCurrency(FieldValue)
                                                                        End If
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case "link"
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                    Case Else
                                                                        Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
                                                                End Select
                                                            Case "copycontent"
                                                                '
                                                                ' A Copy Content block
                                                                '
                                                                FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                                    FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", "false"))
                                                                    If FieldHTML Then
                                                                        '
                                                                        ' treat html as active content for now.
                                                                        '
                                                                        FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName)
                                                                    Else
                                                                        FieldValue = cpCore.docProperties.getText(FieldName)
                                                                    End If

                                                                    CS = cpCore.db.csOpen("Copy Content", "name=" & cpCore.db.encodeSQLText(FieldName), "ID")
                                                                    If Not cpCore.db.csOk(CS) Then
                                                                        Call cpCore.db.csClose(CS)
                                                                        CS = cpCore.db.csInsertRecord("Copy Content")
                                                                    End If
                                                                    If cpCore.db.csOk(CS) Then
                                                                        Call cpCore.db.csSet(CS, "name", FieldName)
                                                                        '
                                                                        ' Set copy
                                                                        '
                                                                        Call cpCore.db.csSet(CS, "copy", FieldValue)
                                                                        '
                                                                        ' delete duplicates
                                                                        '
                                                                        Call cpCore.db.csGoNext(CS)
                                                                        Do While cpCore.db.csOk(CS)
                                                                            Call cpCore.db.csDeleteRecord(CS)
                                                                            Call cpCore.db.csGoNext(CS)
                                                                        Loop
                                                                    End If
                                                                    Call cpCore.db.csClose(CS)
                                                                End If

                                                            Case "filecontent"
                                                                '
                                                                ' A File Content block
                                                                '
                                                                FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                                    fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "")
                                                                    FieldValue = cpCore.docProperties.getText(FieldName)
                                                                    Call cpCore.appRootFiles.saveFile(fieldfilename, FieldValue)
                                                                End If
                                                            Case "dbquery"
                                                                '
                                                                ' dbquery has no results to process
                                                                '
                                                        End Select
                                                    Next
                                                Case Else
                                            End Select
                                        Next
                                    End With
                                End If
                                If (Button = ButtonOK) Then
                                    '
                                    ' Exit on OK or cancel
                                    '
                                    return_ExitRequest = True
                                    Return String.Empty
                                End If
                                '
                                ' ----- Display Form
                                '
                                Content.Add(Adminui.EditTableOpen)
                                Name = xml_GetAttribute(IsFound, Doc.DocumentElement, "name", "")
                                With Doc.DocumentElement
                                    For Each SettingNode In .ChildNodes
                                        Select Case genericController.vbLCase(SettingNode.Name)
                                            Case "description"
                                                Description = SettingNode.InnerText
                                            Case "tab"
                                                TabCnt = TabCnt + 1
                                                TabName = xml_GetAttribute(IsFound, SettingNode, "name", "")
                                                TabDescription = xml_GetAttribute(IsFound, SettingNode, "description", "")
                                                TabHeading = xml_GetAttribute(IsFound, SettingNode, "heading", "")
                                                TabCell = New stringBuilderLegacyController
                                                For Each TabNode In SettingNode.ChildNodes
                                                    Select Case genericController.vbLCase(TabNode.Name)
                                                        Case "heading"
                                                            '
                                                            ' Heading
                                                            '
                                                            FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "")
                                                            Call TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption))
                                                        Case "siteproperty"
                                                            '
                                                            ' Site property
                                                            '
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""))
                                                                fieldType = xml_GetAttribute(IsFound, TabNode, "type", "")
                                                                FieldSelector = xml_GetAttribute(IsFound, TabNode, "selector", "")
                                                                FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "")
                                                                FieldAddon = xml_GetAttribute(IsFound, TabNode, "EditorAddon", "")
                                                                FieldDefaultValue = TabNode.InnerText
                                                                FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue)
                                                                '                                                    If FieldReadOnly Then
                                                                '                                                        '
                                                                '                                                        ' Read only = no editor
                                                                '                                                        '
                                                                '                                                        Copy = FieldValue & cpcore.main_GetFormInputHidden( FieldName, FieldValue)
                                                                '
                                                                '                                                    ElseIf FieldAddon <> "" Then
                                                                If FieldAddon <> "" Then
                                                                    '
                                                                    ' Use Editor Addon
                                                                    '
                                                                    Dim arguments As New Dictionary(Of String, String)
                                                                    arguments.Add("FieldName", FieldName)
                                                                    arguments.Add("FieldValue", cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextAdmin}
                                                                    Dim addon As addonModel = addonModel.createByName(cpCore, FieldAddon)
                                                                    Copy = cpCore.addon.execute(addon, executeContext)
                                                                    'Option_String = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                    'Copy = execute_legacy5(0, FieldAddon, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
                                                                ElseIf FieldSelector <> "" Then
                                                                    '
                                                                    ' Use Selector
                                                                    '
                                                                    Copy = getFormContent_decodeSelector(FieldName, FieldValue, FieldSelector)
                                                                Else
                                                                    '
                                                                    ' Use default editor for each field type
                                                                    '
                                                                    Select Case genericController.vbLCase(fieldType)
                                                                        Case "integer"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue)
                                                                            End If
                                                                        Case "boolean"
                                                                            If FieldReadOnly Then
                                                                                Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                                Copy = genericController.vbReplace(Copy, ">", " disabled>")
                                                                                Copy = Copy & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                            End If
                                                                        Case "float"
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue)
                                                                            End If
                                                                        Case "date"
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputDate(FieldName, FieldValue)
                                                                            End If
                                                                        Case "file", "imagefile"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                If FieldValue = "" Then
                                                                                    Copy = cpCore.html.html_GetFormInputFile(FieldName)
                                                                                Else
                                                                                    NonEncodedLink = cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, FieldValue)
                                                                                    EncodedLink = EncodeURL(NonEncodedLink)
                                                                                    Dim FieldValuefilename As String = ""
                                                                                    Dim FieldValuePath As String = ""
                                                                                    cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename)
                                                                                    Copy = "" _
                                                                                    & "<a href=""http://" & EncodedLink & """ target=""_blank"">[" & FieldValuefilename & "]</A>" _
                                                                                    & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & cpCore.html.html_GetFormInputCheckBox2(FieldName & ".DeleteFlag", False) _
                                                                                    & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & cpCore.html.html_GetFormInputFile(FieldName)
                                                                                End If
                                                                            End If
                                                                        'Call s.Add("&nbsp;</span></nobr></td>")
                                                                        Case "currency"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                If FieldValue <> "" Then
                                                                                    FieldValue = FormatCurrency(FieldValue)
                                                                                End If
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue)
                                                                            End If
                                                                        Case "textfile"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                FieldValue = cpCore.cdnFiles.readFile(FieldValue)
                                                                                If FieldHTML Then
                                                                                    Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue)
                                                                                Else
                                                                                    Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                                End If
                                                                            End If
                                                                        Case "cssfile"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                            End If
                                                                        Case "xmlfile"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                            End If
                                                                        Case "link"
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue)
                                                                            End If
                                                                        Case Else
                                                                            '
                                                                            ' text
                                                                            '
                                                                            If FieldReadOnly Then
                                                                                Copy = FieldValue & cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Else
                                                                                If FieldHTML Then
                                                                                    Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue)
                                                                                Else
                                                                                    Copy = cpCore.html.html_GetFormInputText2(FieldName, FieldValue)
                                                                                End If
                                                                            End If
                                                                    End Select
                                                                End If
                                                                Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                            End If
                                                        Case "copycontent"
                                                            '
                                                            ' Content Copy field
                                                            '
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "")
                                                                FieldHTML = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "html", ""))
                                                                '
                                                                CS = cpCore.db.csOpen("Copy Content", "Name=" & cpCore.db.encodeSQLText(FieldName), "ID", , , , , "Copy")
                                                                If Not cpCore.db.csOk(CS) Then
                                                                    Call cpCore.db.csClose(CS)
                                                                    CS = cpCore.db.csInsertRecord("Copy Content")
                                                                    If cpCore.db.csOk(CS) Then
                                                                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                                                                        Call cpCore.db.csSet(CS, "name", FieldName)
                                                                        Call cpCore.db.csSet(CS, "copy", genericController.encodeText(TabNode.InnerText))
                                                                        Call cpCore.db.csSave2(CS)
                                                                        '   Call cpCore.workflow.publishEdit("Copy Content", RecordID)
                                                                    End If
                                                                End If
                                                                If cpCore.db.csOk(CS) Then
                                                                    FieldValue = cpCore.db.csGetText(CS, "copy")
                                                                End If
                                                                If FieldReadOnly Then
                                                                    '
                                                                    ' Read only
                                                                    '
                                                                    Copy = FieldValue
                                                                ElseIf FieldHTML Then
                                                                    '
                                                                    ' HTML
                                                                    '
                                                                    Copy = cpCore.html.getFormInputHTML(FieldName, FieldValue)
                                                                    'Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                                Else
                                                                    '
                                                                    ' Text edit
                                                                    '
                                                                    Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, FieldValue)
                                                                End If
                                                                Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                            End If
                                                        Case "filecontent"
                                                            '
                                                            ' Content from a flat file
                                                            '
                                                            FieldName = xml_GetAttribute(IsFound, TabNode, "name", "")
                                                            FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "")
                                                            fieldfilename = xml_GetAttribute(IsFound, TabNode, "filename", "")
                                                            FieldReadOnly = genericController.EncodeBoolean(xml_GetAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "")
                                                            FieldDefaultValue = TabNode.InnerText
                                                            Copy = ""
                                                            If fieldfilename <> "" Then
                                                                If cpCore.appRootFiles.fileExists(fieldfilename) Then
                                                                    Copy = FieldDefaultValue
                                                                Else
                                                                    Copy = cpCore.cdnFiles.readFile(fieldfilename)
                                                                End If
                                                                If Not FieldReadOnly Then
                                                                    Copy = cpCore.html.html_GetFormInputTextExpandable(FieldName, Copy, 10)
                                                                End If
                                                            End If
                                                            Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                        Case "dbquery", "querydb", "query", "db"
                                                            '
                                                            ' Display the output of a query
                                                            '
                                                            Copy = ""
                                                            FieldDataSource = xml_GetAttribute(IsFound, TabNode, "DataSourceName", "")
                                                            FieldSQL = TabNode.InnerText
                                                            FieldCaption = xml_GetAttribute(IsFound, TabNode, "caption", "")
                                                            FieldDescription = xml_GetAttribute(IsFound, TabNode, "description", "")
                                                            SQLPageSize = genericController.EncodeInteger(xml_GetAttribute(IsFound, TabNode, "rowmax", ""))
                                                            If SQLPageSize = 0 Then
                                                                SQLPageSize = 100
                                                            End If
                                                            '
                                                            ' Run the SQL
                                                            '
                                                            Dim dt As DataTable = Nothing

                                                            If FieldSQL <> "" Then
                                                                Try
                                                                    dt = cpCore.db.executeQuery(FieldSQL, FieldDataSource, , SQLPageSize)
                                                                Catch ex As Exception
                                                                    ErrorDescription = ex.ToString
                                                                    loadOK = False
                                                                End Try
                                                            End If
                                                            If (dt IsNot Nothing) Then
                                                                If FieldSQL = "" Then
                                                                    '
                                                                    ' ----- Error
                                                                    '
                                                                    Copy = "No Result"
                                                                ElseIf ErrorNumber <> 0 Then
                                                                    '
                                                                    ' ----- Error
                                                                    '
                                                                    Copy = "Error: " & Err.Description
                                                                ElseIf (dt.Rows.Count <= 0) Then
                                                                    '
                                                                    ' ----- no result
                                                                    '
                                                                    Copy = "No Results"
                                                                Else
                                                                    '
                                                                    ' ----- print results
                                                                    '
                                                                    'PageSize = RS.PageSize
                                                                    '
                                                                    ' --- Create the Fields for the new table
                                                                    '
                                                                    '
                                                                    'Dim dtOk As Boolean = True
                                                                    dataArray = cpCore.db.convertDataTabletoArray(dt)
                                                                    '
                                                                    RowMax = UBound(dataArray, 2)
                                                                    ColumnMax = UBound(dataArray, 1)
                                                                    If RowMax = 0 And ColumnMax = 0 Then
                                                                        '
                                                                        ' Single result, display with no table
                                                                        '
                                                                        Copy = cpCore.html.html_GetFormInputText2("result", genericController.encodeText(dataArray(0, 0)), , , , , True)
                                                                    Else
                                                                        '
                                                                        ' Build headers
                                                                        '
                                                                        FieldCount = dt.Columns.Count
                                                                        Copy = Copy & (cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;"">")
                                                                        Copy = Copy & (cr2 & "<tr>")
                                                                        For Each dc As DataColumn In dt.Columns
                                                                            Copy = Copy & (cr2 & vbTab & "<td class=""ccadminsmall"" style=""border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;"">" & dc.ColumnName & "</td>")
                                                                        Next
                                                                        Copy = Copy & (cr2 & "</tr>")
                                                                        '
                                                                        ' Build output table
                                                                        '
                                                                        Dim RowStart As String
                                                                        Dim RowEnd As String
                                                                        Dim ColumnStart As String
                                                                        Dim ColumnEnd As String
                                                                        RowStart = cr2 & "<tr>"
                                                                        RowEnd = cr2 & "</tr>"
                                                                        ColumnStart = cr2 & vbTab & "<td class=""ccadminnormal"" style=""border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px"">"
                                                                        ColumnEnd = "</td>"
                                                                        Dim RowPointer As Integer
                                                                        For RowPointer = 0 To RowMax
                                                                            Copy = Copy & (RowStart)
                                                                            Dim ColumnPointer As Integer
                                                                            For ColumnPointer = 0 To ColumnMax
                                                                                Dim CellData As Object
                                                                                CellData = dataArray(ColumnPointer, RowPointer)
                                                                                If IsNull(CellData) Then
                                                                                    Copy = Copy & (ColumnStart & "[null]" & ColumnEnd)
                                                                                ElseIf IsNothing(CellData) Then
                                                                                    Copy = Copy & (ColumnStart & "[empty]" & ColumnEnd)
                                                                                ElseIf IsArray(CellData) Then
                                                                                    Copy = Copy & ColumnStart & "[array]"
                                                                                ElseIf genericController.encodeText(CellData) = "" Then
                                                                                    Copy = Copy & (ColumnStart & "[empty]" & ColumnEnd)
                                                                                Else
                                                                                    Copy = Copy & (ColumnStart & genericController.encodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
                                                                                End If
                                                                            Next
                                                                            Copy = Copy & (RowEnd)
                                                                        Next
                                                                        Copy = Copy & (cr & "</table>")
                                                                    End If
                                                                End If
                                                            End If
                                                            Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                    End Select
                                                Next
                                                Copy = Adminui.GetEditPanel(True, TabHeading, TabDescription, Adminui.EditTableOpen & TabCell.Text & Adminui.EditTableClose)
                                                If Copy <> "" Then
                                                    Call cpCore.html.main_AddLiveTabEntry(Replace(TabName, " ", "&nbsp;"), Copy, "ccAdminTab")
                                                End If
                                                'Content.Add( cpcore.main_GetForm_Edit_AddTab(TabName, Copy, True))
                                                TabCell = Nothing
                                            Case Else
                                        End Select
                                    Next
                                End With
                                '
                                ' Buttons
                                '
                                ButtonList = ButtonCancel & "," & ButtonSave & "," & ButtonOK
                                '
                                ' Close Tables
                                '
                                If TabCnt > 0 Then
                                    Content.Add(cpCore.html.main_GetLiveTabs())
                                End If
                            End If
                        End If
                    End If
                End If
                '
                getFormContent = Adminui.GetBody(Name, ButtonList, "", True, True, Description, "", 0, Content.Text)
                Content = Nothing
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function getFormContent_decodeSelector(SitePropertyName As String, SitePropertyValue As String, selector As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("AdminClass.addon_execute_GetFormContent_decodeSelector")
            '
            Dim ExpandedSelector As String = String.Empty
            Dim addonInstanceProperties As New Dictionary(Of String, String)
            Dim OptionCaption As String
            Dim OptionValue As String
            Dim OptionValue_AddonEncoded As String
            Dim OptionPtr As Integer
            Dim OptionCnt As Integer
            Dim OptionValues() As String
            Dim OptionSuffix As String = String.Empty
            Dim LCaseOptionDefault As String
            Dim Pos As Integer
            Dim Checked As Boolean
            Dim ParentID As Integer
            Dim ParentCID As Integer
            Dim Criteria As String
            Dim RootCID As Integer
            Dim SQL As String
            Dim TableID As Integer
            Dim TableName As Integer
            Dim ChildCID As Integer
            Dim CIDList As String
            Dim TableName2 As String
            Dim RecordContentName As String
            Dim HasParentID As Boolean
            Dim CS As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim CSPointer As Integer
            Dim RecordID As Integer
            Dim FastString As stringBuilderLegacyController
            Dim FieldValueInteger As Integer
            Dim FieldRequired As Boolean
            Dim FieldHelp As String
            Dim AuthoringStatusMessage As String
            Dim Delimiter As String
            Dim Copy As String = String.Empty
            Dim Adminui As New adminUIController(cpCore)
            '
            Dim FieldName As String
            '
            FastString = New stringBuilderLegacyController
            '
            Dim instanceOptions As New Dictionary(Of String, String)
            instanceOptions.Add(SitePropertyName, SitePropertyValue)
            Call buildAddonOptionLists(addonInstanceProperties, ExpandedSelector, SitePropertyName & "=" & selector, instanceOptions, "0", True)
            Pos = genericController.vbInstr(1, ExpandedSelector, "[")
            If Pos <> 0 Then
                '
                ' List of Options, might be select, radio or checkbox
                '
                LCaseOptionDefault = genericController.vbLCase(Mid(ExpandedSelector, 1, Pos - 1))
                Dim PosEqual As Integer

                PosEqual = genericController.vbInstr(1, LCaseOptionDefault, "=")
                If PosEqual > 0 Then
                    LCaseOptionDefault = Mid(LCaseOptionDefault, PosEqual + 1)
                End If

                LCaseOptionDefault = genericController.decodeNvaArgument(LCaseOptionDefault)
                ExpandedSelector = Mid(ExpandedSelector, Pos + 1)
                Pos = genericController.vbInstr(1, ExpandedSelector, "]")
                If Pos > 0 Then
                    If Pos < Len(ExpandedSelector) Then
                        OptionSuffix = genericController.vbLCase(Trim(Mid(ExpandedSelector, Pos + 1)))
                    End If
                    ExpandedSelector = Mid(ExpandedSelector, 1, Pos - 1)
                End If
                OptionValues = Split(ExpandedSelector, "|")
                getFormContent_decodeSelector = ""
                OptionCnt = UBound(OptionValues) + 1
                For OptionPtr = 0 To OptionCnt - 1
                    OptionValue_AddonEncoded = Trim(OptionValues(OptionPtr))
                    If OptionValue_AddonEncoded <> "" Then
                        Pos = genericController.vbInstr(1, OptionValue_AddonEncoded, ":")
                        If Pos = 0 Then
                            OptionValue = genericController.decodeNvaArgument(OptionValue_AddonEncoded)
                            OptionCaption = OptionValue
                        Else
                            OptionCaption = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, 1, Pos - 1))
                            OptionValue = genericController.decodeNvaArgument(Mid(OptionValue_AddonEncoded, Pos + 1))
                        End If
                        Select Case OptionSuffix
                            Case "checkbox"
                                '
                                ' Create checkbox
                                '
                                If genericController.vbInstr(1, "," & LCaseOptionDefault & ",", "," & genericController.vbLCase(OptionValue) & ",") <> 0 Then
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ checked=""checked"">" & OptionCaption & "</div>"
                                Else
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                End If
                            Case "radio"
                                '
                                ' Create Radio
                                '
                                If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ checked=""checked"" >" & OptionCaption & "</div>"
                                Else
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                End If
                            Case Else
                                '
                                ' Create select 
                                '
                                If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<option value=""" & OptionValue & """ selected>" & OptionCaption & "</option>"
                                Else
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<option value=""" & OptionValue & """>" & OptionCaption & "</option>"
                                End If
                        End Select
                    End If
                Next
                Select Case OptionSuffix
                    Case "checkbox"
                        '
                        '
                        Copy = Copy & "<input type=""hidden"" name=""" & SitePropertyName & "CheckBoxCnt"" value=""" & OptionCnt & """ >"
                    Case "radio"
                        '
                        ' Create Radio 
                        '
                        'cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector = "<div>" & genericController.vbReplace(cpCore.htmldoc.main_Addon_execute_GetFormContent_decodeSelector, "><", "></div><div><") & "</div>"
                    Case Else
                        '
                        ' Create select 
                        '
                        getFormContent_decodeSelector = "<select name=""" & SitePropertyName & """>" & getFormContent_decodeSelector & "</select>"
                End Select
            Else
                '
                ' Create Text addon_execute_GetFormContent_decodeSelector
                '

                selector = genericController.decodeNvaArgument(selector)
                getFormContent_decodeSelector = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20)
            End If

            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("addon_execute_GetFormContent_decodeSelector")
        End Function
        '
        '===================================================================================================
        '   Build AddonOptionLists
        '
        '   On entry:
        '       AddonOptionConstructor = the addon-encoded version of the list that comes from the Addon Record
        '           It is crlf delimited and all escape characters converted
        '       AddonOptionString = addonencoded version of the list that comes from the HTML AC tag
        '           that means & delimited
        '
        '   On Exit:
        '       OptionString_ForObjectCall
        '               pass this string to the addon when it is run, crlf delimited name=value pair.
        '               This should include just the name=values pairs, with no selectors
        '               it should include names from both Addon and Instance
        '               If the Instance has a value, include it. Otherwise include Addon value
        '       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
        '===================================================================================================
        '
        Public Sub buildAddonOptionLists2(ByRef addonInstanceProperties As Dictionary(Of String, String), ByRef addonArgumentListPassToBubbleEditor As String, addonArgumentListFromRecord As String, instanceOptions As Dictionary(Of String, String), InstanceID As String, IncludeSettingsBubbleOptions As Boolean)
            Try
                '
                Dim SavePtr As Integer
                Dim ConstructorTypes() As String
                Dim ConstructorType As String
                Dim ConstructorValue As String
                Dim ConstructorSelector As String
                Dim ConstructorName As String
                Dim ConstructorPtr As Integer
                Dim Pos As Integer
                Dim InstanceCnt As Integer
                Dim InstanceName As String
                Dim InstanceValue As String
                '
                Dim ConstructorNameValues As String() = {}
                Dim ConstructorNames As String() = {}
                Dim ConstructorSelectors As String() = {}
                Dim ConstructorValues As String() = {}
                '
                Dim ConstructorCnt As Integer


                ConstructorCnt = 0
                If (addonArgumentListFromRecord <> "") Then
                    '
                    ' Initially Build Constructor from AddonOptions
                    '
                    ConstructorNameValues = Split(addonArgumentListFromRecord, vbCrLf)
                    ConstructorCnt = UBound(ConstructorNameValues) + 1
                    ReDim ConstructorNames(ConstructorCnt)
                    ReDim ConstructorSelectors(ConstructorCnt)
                    ReDim ConstructorValues(ConstructorCnt)
                    ReDim ConstructorTypes(ConstructorCnt)
                    SavePtr = 0
                    For ConstructorPtr = 0 To ConstructorCnt - 1
                        ConstructorName = ConstructorNameValues(ConstructorPtr)
                        ConstructorSelector = ""
                        ConstructorValue = ""
                        ConstructorType = "text"
                        Pos = genericController.vbInstr(1, ConstructorName, "=")
                        If Pos > 1 Then
                            ConstructorValue = Mid(ConstructorName, Pos + 1)
                            ConstructorName = Trim(Left(ConstructorName, Pos - 1))
                            Pos = genericController.vbInstr(1, ConstructorValue, "[")
                            If Pos > 0 Then
                                ConstructorSelector = Mid(ConstructorValue, Pos)
                                ConstructorValue = Mid(ConstructorValue, 1, Pos - 1)
                            End If
                        End If
                        If ConstructorName <> "" Then
                            'Pos = genericController.vbInstr(1, ConstructorName, ",")
                            'If Pos > 1 Then
                            '    ConstructorType = Mid(ConstructorName, Pos + 1)
                            '    ConstructorName = Left(ConstructorName, Pos - 1)
                            'End If

                            ConstructorNames(SavePtr) = ConstructorName
                            ConstructorValues(SavePtr) = ConstructorValue
                            ConstructorSelectors(SavePtr) = ConstructorSelector
                            'ConstructorTypes(ConstructorPtr) = ConstructorType
                            SavePtr = SavePtr + 1
                        End If
                    Next
                    ConstructorCnt = SavePtr
                End If
                InstanceCnt = 0
                '
                ' Now update the values with Instance - if a name is not found, add it
                '
                For Each kvp In instanceOptions
                    InstanceName = kvp.Key
                    InstanceValue = kvp.Value
                    If InstanceName <> "" Then
                        '
                        ' if the name is not in the Constructor, add it
                        If ConstructorCnt > 0 Then
                            For ConstructorPtr = 0 To ConstructorCnt - 1
                                If genericController.vbLCase(InstanceName) = genericController.vbLCase(ConstructorNames(ConstructorPtr)) Then
                                    Exit For
                                End If
                            Next
                        End If
                        If ConstructorPtr >= ConstructorCnt Then
                            '
                            ' not found, add this instance name and value to the Constructor values
                            '
                            ReDim Preserve ConstructorNames(ConstructorCnt)
                            ReDim Preserve ConstructorValues(ConstructorCnt)
                            ReDim Preserve ConstructorSelectors(ConstructorCnt)
                            ConstructorNames(ConstructorCnt) = InstanceName
                            ConstructorValues(ConstructorCnt) = InstanceValue
                            ConstructorCnt = ConstructorCnt + 1
                        Else
                            '
                            ' found, set the ConstructorValue to the instance value
                            '
                            ConstructorValues(ConstructorPtr) = InstanceValue
                        End If
                        SavePtr = SavePtr + 1
                    End If
                Next
                addonArgumentListPassToBubbleEditor = ""
                '
                ' Build output strings from name and value found
                '
                For ConstructorPtr = 0 To ConstructorCnt - 1
                    ConstructorName = ConstructorNames(ConstructorPtr)
                    ConstructorValue = ConstructorValues(ConstructorPtr)
                    ConstructorSelector = ConstructorSelectors(ConstructorPtr)
                    ' here goes nothing!!
                    addonInstanceProperties.Add(ConstructorName, ConstructorValue)
                    'OptionString_ForObjectCall = OptionString_ForObjectCall & csv_DecodeAddonOptionArgument(ConstructorName) & "=" & csv_DecodeAddonOptionArgument(ConstructorValue) & vbCrLf
                    If IncludeSettingsBubbleOptions Then
                        addonArgumentListPassToBubbleEditor = addonArgumentListPassToBubbleEditor & vbCrLf & cpCore.html.getAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector)
                    End If
                Next
                addonInstanceProperties.Add("InstanceID", InstanceID)
                'If OptionString_ForObjectCall <> "" Then
                '    OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 1)
                '    'OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 2)
                'End If
                If addonArgumentListPassToBubbleEditor <> "" Then
                    addonArgumentListPassToBubbleEditor = Mid(addonArgumentListPassToBubbleEditor, 3)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        '   Build AddonOptionLists
        '
        '   On entry:
        '       AddonOptionConstructor = the addon-encoded Version of the list that comes from the Addon Record
        '           It is line-delimited with &, and all escape characters converted
        '       InstanceOptionList = addonencoded Version of the list that comes from the HTML AC tag
        '           that means crlf line-delimited
        '
        '   On Exit:
        '       AddonOptionNameValueList
        '               pass this string to the addon when it is run, crlf delimited name=value pair.
        '               This should include just the name=values pairs, with no selectors
        '               it should include names from both Addon and Instance
        '               If the Instance has a value, include it. Otherwise include Addon value
        '       AddonOptionExpandedConstructor = pass this to the bubble editor to create the the selectr
        '===================================================================================================
        '
        Public Sub buildAddonOptionLists(ByRef addonInstanceProperties As Dictionary(Of String, String), ByRef addonArgumentListPassToBubbleEditor As String, addonArgumentListFromRecord As String, InstanceOptionList As Dictionary(Of String, String), InstanceID As String, IncludeEditWrapper As Boolean)
            Call buildAddonOptionLists2(addonInstanceProperties, addonArgumentListPassToBubbleEditor, addonArgumentListFromRecord, InstanceOptionList, InstanceID, IncludeEditWrapper)
        End Sub
        '
        '
        '
        Public Function getPrivateFilesAddonPath() As String
            Return "addons\"
        End Function
        '
        '========================================================================
        '   Apply a wrapper to content
        '========================================================================
        '
        Private Function addWrapperToResult(ByVal Content As String, ByVal WrapperID As Integer, Optional ByVal WrapperSourceForComment As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("WrapContent")
            '
            Dim Pos As Integer
            Dim CS As Integer
            Dim JSFilename As String
            Dim Copy As String
            Dim s As String
            Dim SelectFieldList As String
            Dim Wrapper As String
            Dim wrapperName As String
            Dim SourceComment As String
            Dim TargetString As String
            '
            s = Content
            SelectFieldList = "name,copytext,javascriptonload,javascriptbodyend,stylesfilename,otherheadtags,JSFilename,targetString"
            CS = cpCore.db.csOpenRecord("Wrappers", WrapperID, , , SelectFieldList)
            If cpCore.db.csOk(CS) Then
                Wrapper = cpCore.db.csGetText(CS, "copytext")
                wrapperName = cpCore.db.csGetText(CS, "name")
                TargetString = cpCore.db.csGetText(CS, "targetString")
                '
                SourceComment = "wrapper " & wrapperName
                If WrapperSourceForComment <> "" Then
                    SourceComment = SourceComment & " for " & WrapperSourceForComment
                End If
                Call cpCore.html.addScriptCode_onLoad(cpCore.db.csGetText(CS, "javascriptonload"), SourceComment)
                Call cpCore.html.addScriptCode_body(cpCore.db.csGetText(CS, "javascriptbodyend"), SourceComment)
                Call cpCore.html.addHeadTag(cpCore.db.csGetText(CS, "OtherHeadTags"), SourceComment)
                '
                JSFilename = cpCore.db.csGetText(CS, "jsfilename")
                If JSFilename <> "" Then
                    JSFilename = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, JSFilename)
                    Call cpCore.html.addScriptLink_Head(JSFilename, SourceComment)
                End If
                Copy = cpCore.db.csGetText(CS, "stylesfilename")
                If Copy <> "" Then
                    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                    ElseIf Left(Copy, 1) = "/" Then
                    Else
                        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                    End If
                    Call cpCore.html.addStyleLink(Copy, SourceComment)
                End If
                '
                If Wrapper <> "" Then
                    Pos = genericController.vbInstr(1, Wrapper, TargetString, vbTextCompare)
                    If Pos <> 0 Then
                        s = genericController.vbReplace(Wrapper, TargetString, s, 1, 99, vbTextCompare)
                    Else
                        s = "" _
                            & "<!-- the selected wrapper does not include the Target String marker to locate the position of the content. -->" _
                            & Wrapper _
                            & s
                    End If
                End If
            End If
            Call cpCore.db.csClose(CS)
            '
            addWrapperToResult = s
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("WrapContent")
        End Function        '
        '
        '========================================================================
        ' ----- main_Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Function xml_GetAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            Dim result As String = String.Empty
            Try
                '
                Dim NodeAttribute As XmlAttribute
                Dim ResultNode As XmlNode
                Dim UcaseName As String
                '
                Found = False
                ResultNode = Node.Attributes.GetNamedItem(Name)
                If (ResultNode Is Nothing) Then
                    UcaseName = genericController.vbUCase(Name)
                    For Each NodeAttribute In Node.Attributes
                        If genericController.vbUCase(NodeAttribute.Name) = UcaseName Then
                            result = NodeAttribute.Value
                            Found = True
                            Exit For
                        End If
                    Next
                Else
                    result = ResultNode.Value
                    Found = True
                End If
                If Not Found Then
                    result = DefaultIfNotFound
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '
        Public Shared Function main_GetDefaultAddonOption_String(cpCore As coreClass, ByVal ArgumentList As String, ByVal AddonGuid As String, ByVal IsInline As Boolean) As String
            Dim result As String = ""
            '
            Dim NameValuePair As String
            Dim Pos As Integer
            Dim OptionName As String
            Dim OptionValue As String
            Dim OptionSelector As String
            Dim QuerySplit() As String
            Dim NameValue As String
            Dim Ptr As Integer
            '
            ArgumentList = genericController.vbReplace(ArgumentList, vbCrLf, vbCr)
            ArgumentList = genericController.vbReplace(ArgumentList, vbLf, vbCr)
            ArgumentList = genericController.vbReplace(ArgumentList, vbCr, vbCrLf)
            If (InStr(1, ArgumentList, "wrapper", vbTextCompare) = 0) Then
                '
                ' Add in default constructors, like wrapper
                '
                If ArgumentList <> "" Then
                    ArgumentList = ArgumentList & vbCrLf
                End If
                If genericController.vbLCase(AddonGuid) = genericController.vbLCase(addonGuidContentBox) Then
                    ArgumentList = ArgumentList & AddonOptionConstructor_BlockNoAjax
                ElseIf IsInline Then
                    ArgumentList = ArgumentList & AddonOptionConstructor_Inline
                Else
                    ArgumentList = ArgumentList & AddonOptionConstructor_Block
                End If
            End If
            If ArgumentList <> "" Then
                '
                ' Argument list is present, translate from AddonConstructor to AddonOption format (see main_executeAddon for details)
                '
                QuerySplit = genericController.SplitCRLF(ArgumentList)
                result = ""
                For Ptr = 0 To UBound(QuerySplit)
                    NameValue = QuerySplit(Ptr)
                    If NameValue <> "" Then
                        '
                        ' Execute list functions
                        '
                        OptionName = ""
                        OptionValue = ""
                        OptionSelector = ""
                        '
                        ' split on equal
                        '
                        NameValue = genericController.vbReplace(NameValue, "\=", vbCrLf)
                        Pos = genericController.vbInstr(1, NameValue, "=")
                        If Pos = 0 Then
                            OptionName = NameValue
                        Else
                            OptionName = Mid(NameValue, 1, Pos - 1)
                            OptionValue = Mid(NameValue, Pos + 1)
                        End If
                        OptionName = genericController.vbReplace(OptionName, vbCrLf, "\=")
                        OptionValue = genericController.vbReplace(OptionValue, vbCrLf, "\=")
                        '
                        ' split optionvalue on [
                        '
                        OptionValue = genericController.vbReplace(OptionValue, "\[", vbCrLf)
                        Pos = genericController.vbInstr(1, OptionValue, "[")
                        If Pos <> 0 Then
                            OptionSelector = Mid(OptionValue, Pos)
                            OptionValue = Mid(OptionValue, 1, Pos - 1)
                        End If
                        OptionValue = genericController.vbReplace(OptionValue, vbCrLf, "\[")
                        OptionSelector = genericController.vbReplace(OptionSelector, vbCrLf, "\[")
                        '
                        ' Decode AddonConstructor format
                        '
                        OptionName = genericController.DecodeAddonConstructorArgument(OptionName)
                        OptionValue = genericController.DecodeAddonConstructorArgument(OptionValue)
                        '
                        ' Encode AddonOption format
                        '
                        'main_GetAddonSelector expects value to be encoded, but not name
                        'OptionName = encodeNvaArgument(OptionName)
                        OptionValue = genericController.encodeNvaArgument(OptionValue)
                        '
                        ' rejoin
                        '
                        NameValuePair = cpCore.html.getAddonSelector(OptionName, OptionValue, OptionSelector)
                        NameValuePair = genericController.EncodeJavascript(NameValuePair)
                        result = result & "&" & NameValuePair
                        If genericController.vbInstr(1, NameValuePair, "=") = 0 Then
                            result = result & "="
                        End If
                    End If
                Next
                If result <> "" Then
                    ' remove leading "&"
                    result = Mid(result, 2)
                End If
            End If
            Return result
        End Function
        '
        '=================================================================================================================
        '   csv_GetAddonOption
        '
        '   returns the value matching a given name in an AddonOptionConstructor
        '
        '   AddonOptionConstructor is a crlf delimited name=value[selector]descriptor list
        '
        '   See cpCoreClass.ExecuteAddon for a full description of:
        '       AddonOptionString
        '       AddonOptionConstructor
        '       AddonOptionNameValueList
        '       AddonOptionExpandedConstructor
        '=================================================================================================================
        '
        Public Shared Function getAddonOption(OptionName As String, OptionString As String) As String
            Dim result As String = ""
            Dim WorkingString As String
            Dim Options() As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim TestName As String
            Dim TargetName As String
            '
            WorkingString = OptionString
            result = ""
            If WorkingString <> "" Then
                TargetName = genericController.vbLCase(OptionName)
                Options = Split(OptionString, "&")
                For Ptr = 0 To UBound(Options)
                    Pos = genericController.vbInstr(1, Options(Ptr), "=")
                    If Pos > 0 Then
                        TestName = genericController.vbLCase(Trim(Left(Options(Ptr), Pos - 1)))
                        Do While (TestName <> "") And (Left(TestName, 1) = vbTab)
                            TestName = Trim(Mid(TestName, 2))
                        Loop
                        Do While (TestName <> "") And (Right(TestName, 1) = vbTab)
                            TestName = Trim(Mid(TestName, 1, Len(TestName) - 1))
                        Loop
                        If TestName = TargetName Then
                            result = genericController.decodeNvaArgument(Trim(Mid(Options(Ptr), Pos + 1)))
                            Exit For
                        End If
                    End If
                Next
            End If
            Return result
        End Function
        '
        Private Function getAddonDescription(cpcore As coreClass, addon As Models.Entity.addonModel) As String
            Dim addonDescription As String = "[invalid addon]"
            If (addon IsNot Nothing) Then
                Dim collectionName As String = "invalid collection or collection not set"
                Dim collection As Models.Entity.AddonCollectionModel = Models.Entity.AddonCollectionModel.create(cpcore, addon.CollectionID)
                If (collection IsNot Nothing) Then
                    collectionName = collection.name
                End If
                addonDescription = "[#" & addon.id.ToString() & ", " & addon.name & "], collection [" & collectionName & "]"
            End If
            Return addonDescription
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Special case addon as it is a required core service. This method attempts the addon call and it if fails, calls the safe-mode version, tested for this build
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function GetAddonManager(cpCore As coreClass) As String
            Dim addonManager As String = ""
            Try
                Dim AddonStatusOK As Boolean = True
                Try
                    Dim addon As addonModel = addonModel.create(cpCore, addonGuidAddonManager)
                    addonManager = cpCore.addon.execute(addon, New CPUtilsBaseClass.addonExecuteContext() With {.addonType = CPUtilsBaseClass.addonContext.ContextAdmin})
                    'addonManager = cpCore.addon.execute_legacy2(0, addonGuidAddonManager, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "0", False, -1, "", AddonStatusOK, Nothing)
                Catch ex As Exception
                    Call cpCore.handleException(New Exception("Error calling ExecuteAddon with AddonManagerGuid, will attempt Safe Mode Addon Manager. Exception=[" & ex.ToString & "]"))
                    AddonStatusOK = False
                End Try
                If addonManager = "" Then
                    Call cpCore.handleException(New Exception("AddonManager returned blank, calling Safe Mode Addon Manager."))
                    AddonStatusOK = False
                End If
                If Not AddonStatusOK Then
                    Dim AddonMan As New addon_AddonMngrSafeClass(cpCore)
                    addonManager = AddonMan.GetForm_SafeModeAddonManager()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return addonManager
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' If an addon assembly references a system assembly that is not in the gac (system.io.compression.filesystem), it does not look in the folder I did the loadfrom.
        ''' Problem is knowing where to look. No argument to pass a path...
        ''' </summary>
        ''' <param name="sender"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Shared Function myAssemblyResolve(sender As Object, args As ResolveEventArgs) As Assembly
            Dim sample_folderPath As String = IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)

            Dim assemblyPath As String = IO.Path.Combine(sample_folderPath, New AssemblyName(args.Name).Name + ".dll")
            If (Not IO.File.Exists(assemblyPath)) Then Return Nothing
            Dim assembly As Assembly = Assembly.LoadFrom(assemblyPath)
            Return assembly
        End Function

        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    'If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
End Namespace