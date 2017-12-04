
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models.Entity

Namespace Contensive.Core.Controllers
    ''' <summary>
    ''' Tools used to assemble html document elements. This is not a storage for assembling a document (see docController)
    ''' </summary>
    Public Class htmlController
        '
        '====================================================================================================
        '
        Public Sub addScriptCode_onLoad(code As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(code)) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass() With {
                        .assetType = htmlAssetTypeEnum.OnLoadScript,
                        .addedByMessage = addedByMessage,
                        .isLink = False,
                        .content = code
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Sub addScriptCode_body(code As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(code)) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass() With {
                        .assetType = htmlAssetTypeEnum.script,
                        .addedByMessage = addedByMessage,
                        .isLink = False,
                        .content = genericController.removeScriptTag(code)
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Sub addScriptCode_head(code As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(code)) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass() With {
                        .assetType = htmlAssetTypeEnum.script,
                        .inHead = True,
                        .addedByMessage = addedByMessage,
                        .isLink = False,
                        .content = genericController.removeScriptTag(code)
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addScriptLink_Head(Filename As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(Filename)) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass With {
                        .assetType = htmlAssetTypeEnum.script,
                        .addedByMessage = addedByMessage,
                        .isLink = True,
                        .inHead = True,
                        .content = Filename
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addScriptLink_Body(Filename As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(Filename)) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass With {
                        .assetType = htmlAssetTypeEnum.script,
                        .addedByMessage = addedByMessage,
                        .isLink = True,
                        .content = Filename
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addTitle(pageTitle As String, Optional addedByMessage As String = "")
            Try
                If (Not String.IsNullOrEmpty(pageTitle.Trim())) Then
                    cpCore.doc.htmlMetaContent_TitleList.Add(New htmlMetaClass() With {
                        .addedByMessage = addedByMessage,
                        .content = pageTitle
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addMetaDescription(MetaDescription As String, Optional addedByMessage As String = "")
            Try
                If (Not String.IsNullOrEmpty(MetaDescription.Trim())) Then
                    cpCore.doc.htmlMetaContent_Description.Add(New htmlMetaClass() With {
                        .addedByMessage = addedByMessage,
                        .content = MetaDescription
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addStyleLink(StyleSheetLink As String, addedByMessage As String)
            Try
                If (Not String.IsNullOrEmpty(StyleSheetLink.Trim())) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass() With {
                        .addedByMessage = addedByMessage,
                        .assetType = htmlAssetTypeEnum.style,
                        .inHead = True,
                        .isLink = True,
                        .content = StyleSheetLink
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addStyleCode(code As String, Optional addedByMessage As String = "")
            Try
                If (Not String.IsNullOrEmpty(code.Trim())) Then
                    cpCore.doc.htmlAssetList.Add(New htmlAssetClass() With {
                        .addedByMessage = addedByMessage,
                        .assetType = htmlAssetTypeEnum.style,
                        .inHead = True,
                        .isLink = False,
                        .content = code
                    })
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addMetaKeywordList(MetaKeywordList As String, Optional addedByMessage As String = "")
            Try
                For Each keyword As String In MetaKeywordList.Split(","c)
                    If (Not String.IsNullOrEmpty(keyword)) Then
                        cpCore.doc.htmlMetaContent_KeyWordList.Add(New htmlMetaClass() With {
                            .addedByMessage = addedByMessage,
                            .content = keyword
                        })
                    End If
                Next
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '=========================================================================================================
        '
        Public Sub addHeadTag(HeadTag As String, Optional addedByMessage As String = "")
            Try
                cpCore.doc.htmlMetaContent_OtherTags.Add(New htmlMetaClass() With {
                    .addedByMessage = addedByMessage,
                    .content = HeadTag
                })
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        '
        Public Function getEditWrapper(ByVal Caption As String, ByVal Content As String) As String
            Dim result As String = Content
            Try
                If cpCore.doc.authContext.isEditingAnything() Then
                    result = html_GetLegacySiteStyles() _
                        & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapper"">"
                    If Caption <> "" Then
                        result &= "" _
                                & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperCaption"">" _
                                & genericController.encodeText(Caption) _
                                & "<!-- <img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=22 align=absmiddle> -->" _
                                & "</td></tr></table>"
                    End If
                    result &= "" _
                            & "<table border=0 width=""100%"" cellspacing=0 cellpadding=0><tr><td class=""ccEditWrapperContent"" id=""editWrapper" & cpCore.doc.editWrapperCnt & """>" _
                            & genericController.encodeText(Content) _
                            & "</td></tr></table>" _
                            & "</td></tr></table>"
                    cpCore.doc.editWrapperCnt = cpCore.doc.editWrapperCnt + 1
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '===================================================================================================
        ' To support the special case when the template calls this to encode itself, and the page content has already been rendered.
        '
        Private Function convertActiveContent_internal(Source As String, personalizationPeopleId As Integer, ContextContentName As String, ContextRecordID As Integer, ContextContactPeopleID As Integer, PlainText As Boolean, AddLinkEID As Boolean, EncodeActiveFormatting As Boolean, EncodeActiveImages As Boolean, EncodeActiveEditIcons As Boolean, EncodeActivePersonalization As Boolean, queryStringForLinkAppend As String, ProtocolHostLink As String, IsEmailContent As Boolean, ignore_DefaultWrapperID As Integer, ignore_TemplateCaseOnly_Content As String, Context As CPUtilsBaseClass.addonContext, personalizationIsAuthenticated As Boolean, nothingObject As Object, isEditingAnything As Boolean) As String
            Dim result As String = Source
            Try
                '
                Const StartFlag = "<!-- ADDON"
                Const EndFlag = " -->"
                '
                Dim DoAnotherPass As Boolean
                Dim ArgCnt As Integer
                Dim AddonGuid As String
                Dim ACInstanceID As String
                Dim ArgSplit() As String
                Dim AddonName As String
                Dim addonOptionString As String
                Dim LineStart As Integer
                Dim LineEnd As Integer
                Dim Copy As String
                Dim Wrapper() As String
                Dim SegmentSplit() As String
                Dim AcCmd As String
                Dim SegmentSuffix As String
                Dim AcCmdSplit() As String
                Dim ACType As String
                Dim ContentSplit() As String
                Dim ContentSplitCnt As Integer
                Dim Segment As String
                Dim Ptr As Integer
                Dim CopyName As String
                Dim ListName As String
                Dim SortField As String
                Dim SortReverse As Boolean
                Dim AdminURL As String
                '
                Dim converthtmlToText As htmlToTextControllers
                '
                Dim iPersonalizationPeopleId As Integer
                iPersonalizationPeopleId = personalizationPeopleId
                If iPersonalizationPeopleId = 0 Then
                    iPersonalizationPeopleId = cpCore.doc.authContext.user.id
                End If
                '

                'hint = "csv_EncodeContent9 enter"
                If result <> "" Then
                    AdminURL = "/" & cpCore.serverConfig.appConfig.adminRoute
                    '
                    '--------
                    ' cut-paste from csv_EncodeContent8
                    '--------
                    '
                    ' ----- Do EncodeCRLF Conversion
                    '
                    'hint = hint & ",010"
                    If cpCore.siteProperties.getBoolean("ConvertContentCRLF2BR", False) And (Not PlainText) Then
                        result = genericController.vbReplace(result, vbCr, "")
                        result = genericController.vbReplace(result, vbLf, "<br>")
                    End If
                    '
                    ' ----- Do upgrade conversions (upgrade legacy objects and upgrade old images)
                    '
                    'hint = hint & ",020"
                    result = upgradeActiveContent(result)
                    '
                    ' ----- Do Active Content Conversion
                    '
                    'hint = hint & ",030"
                    If (AddLinkEID Or EncodeActiveFormatting Or EncodeActiveImages Or EncodeActiveEditIcons) Then
                        result = convertActiveContent_Internal_activeParts(result, iPersonalizationPeopleId, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeActiveFormatting, EncodeActiveImages, EncodeActiveEditIcons, EncodeActivePersonalization, queryStringForLinkAppend, ProtocolHostLink, IsEmailContent, AdminURL, personalizationIsAuthenticated, Context)
                    End If
                    '
                    ' ----- Do Plain Text Conversion
                    '
                    'hint = hint & ",040"
                    If PlainText Then
                        converthtmlToText = New htmlToTextControllers(cpCore)
                        result = converthtmlToText.convert(result)
                        converthtmlToText = Nothing
                    End If
                    '
                    ' Process Active Content that must be run here to access webclass objects
                    '     parse as {{functionname?querystring}}
                    '
                    'hint = hint & ",110"
                    If (Not EncodeActiveEditIcons) And (InStr(1, result, "{{") <> 0) Then
                        ContentSplit = Split(result, "{{")
                        result = ""
                        ContentSplitCnt = UBound(ContentSplit) + 1
                        Ptr = 0
                        Do While Ptr < ContentSplitCnt
                            'hint = hint & ",200"
                            Segment = ContentSplit(Ptr)
                            If Ptr = 0 Then
                                '
                                ' Add in the non-command text that is before the first command
                                '
                                result = result & Segment
                            ElseIf (Segment <> "") Then
                                If genericController.vbInstr(1, Segment, "}}") = 0 Then
                                    '
                                    ' No command found, return the marker and deliver the Segment
                                    '
                                    'hint = hint & ",210"
                                    result = result & "{{" & Segment
                                Else
                                    '
                                    ' isolate the command
                                    '
                                    'hint = hint & ",220"
                                    SegmentSplit = Split(Segment, "}}")
                                    AcCmd = SegmentSplit(0)
                                    SegmentSplit(0) = ""
                                    SegmentSuffix = Mid(Join(SegmentSplit, "}}"), 3)
                                    If Trim(AcCmd) <> "" Then
                                        '
                                        ' isolate the arguments
                                        '
                                        'hint = hint & ",230"
                                        AcCmdSplit = Split(AcCmd, "?")
                                        ACType = Trim(AcCmdSplit(0))
                                        If UBound(AcCmdSplit) = 0 Then
                                            addonOptionString = ""
                                        Else
                                            addonOptionString = AcCmdSplit(1)
                                            addonOptionString = genericController.decodeHtml(addonOptionString)
                                        End If
                                        '
                                        ' execute the command
                                        '
                                        Select Case genericController.vbUCase(ACType)
                                            Case ACTypeDynamicForm
                                                '
                                                ' Dynamic Form - run the core addon replacement instead
                                                Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                                    .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                                    .cssContainerClass = "",
                                                    .cssContainerId = "",
                                                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                                        .contentName = ContextContentName,
                                                        .fieldName = "",
                                                        .recordId = ContextRecordID
                                                    },
                                                    .personalizationAuthenticated = personalizationIsAuthenticated,
                                                    .personalizationPeopleId = iPersonalizationPeopleId,
                                                    .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                                }
                                                Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.create(cpCore, addonGuidDynamicForm)
                                                result &= cpCore.addon.execute(addon, executeContext)
                                            Case ACTypeChildList
                                                '
                                                ' Child Page List
                                                '
                                                'hint = hint & ",320"
                                                ListName = addonController.getAddonOption("name", addonOptionString)
                                                result = result & cpCore.doc.getChildPageList(ListName, ContextContentName, ContextRecordID, True)
                                            Case ACTypeTemplateText
                                                '
                                                ' Text Box = copied here from gethtmlbody
                                                '
                                                CopyName = addonController.getAddonOption("new", addonOptionString)
                                                If CopyName = "" Then
                                                    CopyName = addonController.getAddonOption("name", addonOptionString)
                                                    If CopyName = "" Then
                                                        CopyName = "Default"
                                                    End If
                                                End If
                                                result = result & html_GetContentCopy(CopyName, "", iPersonalizationPeopleId, False, personalizationIsAuthenticated)
                                            Case ACTypeWatchList
                                                '
                                                ' Watch List
                                                '
                                                'hint = hint & ",330"
                                                ListName = addonController.getAddonOption("LISTNAME", addonOptionString)
                                                SortField = addonController.getAddonOption("SORTFIELD", addonOptionString)
                                                SortReverse = genericController.EncodeBoolean(addonController.getAddonOption("SORTDIRECTION", addonOptionString))
                                                result = result & cpCore.doc.main_GetWatchList(cpCore, ListName, SortField, SortReverse)
                                            Case Else
                                                '
                                                ' Unrecognized command - put all the syntax back in
                                                '
                                                'hint = hint & ",340"
                                                result = result & "{{" & AcCmd & "}}"
                                        End Select
                                    End If
                                    '
                                    ' add the SegmentSuffix back on
                                    '
                                    result = result & SegmentSuffix
                                End If
                            End If
                            '
                            ' Encode into Javascript if required
                            '
                            Ptr = Ptr + 1
                        Loop
                    End If
                    '
                    ' Process Addons
                    '   parse as <!-- Addon "Addon Name","OptionString" -->
                    '   They are handled here because Addons are written against cpCoreClass, not the Content Server class
                    '   ...so Group Email can not process addons 8(
                    '   Later, remove the csv routine that translates <ac to this, and process it directly right here
                    '   Later, rewrite so addons call csv, not cpCoreClass, so email processing can include addons
                    ' (2/16/2010) - move csv_EncodeContent to csv, or wait and move it all to CP
                    '    eventually, everything should migrate to csv and/or cp to eliminate the cpCoreClass dependancy
                    '    and all add-ons run as processes the same as they run on pages, or as remote methods
                    ' (2/16/2010) - if <!-- AC --> has four arguments, the fourth is the addon guid
                    '
                    If (InStr(1, result, StartFlag) <> 0) Then
                        Do While (InStr(1, result, StartFlag) <> 0)
                            LineStart = genericController.vbInstr(1, result, StartFlag)
                            LineEnd = genericController.vbInstr(LineStart, result, EndFlag)
                            If LineEnd = 0 Then
                                logController.appendLog(cpCore, "csv_EncodeContent9, Addon could not be inserted into content because the HTML comment holding the position is not formated correctly")
                                Exit Do
                            Else
                                AddonName = ""
                                addonOptionString = ""
                                ACInstanceID = ""
                                AddonGuid = ""
                                Copy = Mid(result, LineStart + 11, LineEnd - LineStart - 11)
                                ArgSplit = genericController.SplitDelimited(Copy, ",")
                                ArgCnt = UBound(ArgSplit) + 1
                                If ArgSplit(0) <> "" Then
                                    AddonName = Mid(ArgSplit(0), 2, Len(ArgSplit(0)) - 2)
                                    If ArgCnt > 1 Then
                                        If ArgSplit(1) <> "" Then
                                            addonOptionString = Mid(ArgSplit(1), 2, Len(ArgSplit(1)) - 2)
                                            addonOptionString = genericController.decodeHtml(Trim(addonOptionString))
                                        End If
                                        If ArgCnt > 2 Then
                                            If ArgSplit(2) <> "" Then
                                                ACInstanceID = Mid(ArgSplit(2), 2, Len(ArgSplit(2)) - 2)
                                            End If
                                            If ArgCnt > 3 Then
                                                If ArgSplit(3) <> "" Then
                                                    AddonGuid = Mid(ArgSplit(3), 2, Len(ArgSplit(3)) - 2)
                                                End If
                                            End If
                                        End If
                                    End If
                                    ' dont have any way of getting fieldname yet

                                    Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                    .addonType = CPUtilsBaseClass.addonContext.ContextPage,
                                    .cssContainerClass = "",
                                    .cssContainerId = "",
                                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                        .contentName = ContextContentName,
                                        .fieldName = "",
                                        .recordId = ContextRecordID
                                    },
                                    .personalizationAuthenticated = personalizationIsAuthenticated,
                                    .personalizationPeopleId = iPersonalizationPeopleId,
                                    .instanceGuid = ACInstanceID,
                                    .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, addonOptionString)
                                }
                                    If AddonGuid <> "" Then
                                        Copy = cpCore.addon.execute(Models.Entity.addonModel.create(cpCore, AddonGuid), executeContext)
                                        'Copy = cpCore.addon.execute_legacy6(0, AddonGuid, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                    Else
                                        Copy = cpCore.addon.execute(Models.Entity.addonModel.createByName(cpCore, AddonName), executeContext)
                                        'Copy = cpCore.addon.execute_legacy6(0, AddonName, addonOptionString, CPUtilsBaseClass.addonContext.ContextPage, ContextContentName, ContextRecordID, "", ACInstanceID, False, ignore_DefaultWrapperID, ignore_TemplateCaseOnly_Content, False, Nothing, "", Nothing, "", iPersonalizationPeopleId, personalizationIsAuthenticated)
                                    End If
                                End If
                            End If
                            result = Mid(result, 1, LineStart - 1) & Copy & Mid(result, LineEnd + 4)
                        Loop
                    End If
                    '
                    ' process out text block comments inserted by addons
                    ' remove all content between BlockTextStartMarker and the next BlockTextEndMarker, or end of copy
                    ' exception made for the content with just the startmarker because when the AC tag is replaced with
                    ' with the marker, encode content is called with the result, which is just the marker, and this
                    ' section will remove it
                    '
                    If (Not isEditingAnything) And (result <> BlockTextStartMarker) Then
                        DoAnotherPass = True
                        Do While (InStr(1, result, BlockTextStartMarker, vbTextCompare) <> 0) And DoAnotherPass
                            LineStart = genericController.vbInstr(1, result, BlockTextStartMarker, vbTextCompare)
                            If LineStart = 0 Then
                                DoAnotherPass = False
                            Else
                                LineEnd = genericController.vbInstr(LineStart, result, BlockTextEndMarker, vbTextCompare)
                                If LineEnd <= 0 Then
                                    DoAnotherPass = False
                                    result = Mid(result, 1, LineStart - 1)
                                Else
                                    LineEnd = genericController.vbInstr(LineEnd, result, " -->")
                                    If LineEnd <= 0 Then
                                        DoAnotherPass = False
                                    Else
                                        result = Mid(result, 1, LineStart - 1) & Mid(result, LineEnd + 4)
                                        'returnValue = Mid(returnValue, 1, LineStart - 1) & Copy & Mid(returnValue, LineEnd + 4)
                                    End If
                                End If
                            End If
                        Loop
                    End If
                    '
                    ' only valid for a webpage
                    '
                    If True Then
                        '
                        ' Add in EditWrappers for Aggregate scripts and replacements
                        '   This is also old -- used here because csv encode content can create replacements and links, but can not
                        '   insert wrappers. This is all done in GetAddonContents() now. This routine is left only to
                        '   handle old style calls in cache.
                        '
                        'hint = hint & ",500, Adding edit wrappers"
                        If isEditingAnything Then
                            If (InStr(1, result, "<!-- AFScript -->", vbTextCompare) <> 0) Then
                                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("returnValue", "AFScript Style edit wrappers are not supported")
                                Copy = getEditWrapper("Aggregate Script", "##MARKER##")
                                Wrapper = Split(Copy, "##MARKER##")
                                result = genericController.vbReplace(result, "<!-- AFScript -->", Wrapper(0), 1, 99, vbTextCompare)
                                result = genericController.vbReplace(result, "<!-- /AFScript -->", Wrapper(1), 1, 99, vbTextCompare)
                            End If
                            If (InStr(1, result, "<!-- AFReplacement -->", vbTextCompare) <> 0) Then
                                Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError7("returnValue", "AFReplacement Style edit wrappers are not supported")
                                Copy = getEditWrapper("Aggregate Replacement", "##MARKER##")
                                Wrapper = Split(Copy, "##MARKER##")
                                result = genericController.vbReplace(result, "<!-- AFReplacement -->", Wrapper(0), 1, 99, vbTextCompare)
                                result = genericController.vbReplace(result, "<!-- /AFReplacement -->", Wrapper(1), 1, 99, vbTextCompare)
                            End If
                        End If
                        '
                        ' Process Feedback form
                        '
                        'hint = hint & ",600, Handle webclient features"
                        If genericController.vbInstr(1, result, FeedbackFormNotSupportedComment, vbTextCompare) <> 0 Then
                            result = genericController.vbReplace(result, FeedbackFormNotSupportedComment, pageContentController.main_GetFeedbackForm(cpCore, ContextContentName, ContextRecordID, ContextContactPeopleID), 1, 99, vbTextCompare)
                        End If
                        ''
                        '' If any javascript or styles were added during encode, pick them up now
                        ''
                        'Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        'Do While Copy <> ""
                        '    Call addScriptCode_body(Copy, "embedded content")
                        '    Copy = cpCore.doc.getNextJavascriptBodyEnd()
                        'Loop
                        ''
                        '' current
                        ''
                        'Copy = cpCore.doc.getNextJSFilename()
                        'Do While Copy <> ""
                        '    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        '    ElseIf Left(Copy, 1) = "/" Then
                        '    Else
                        '        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        '    End If
                        '    Call addScriptLink_Head(Copy, "embedded content")
                        '    Copy = cpCore.doc.getNextJSFilename()
                        'Loop
                        ''
                        'Copy = cpCore.doc.getJavascriptOnLoad()
                        'Do While Copy <> ""
                        '    Call addOnLoadJs(Copy, "")
                        '    Copy = cpCore.doc.getJavascriptOnLoad()
                        'Loop
                        '
                        'Copy = cpCore.doc.getNextStyleFilenames()
                        'Do While Copy <> ""
                        '    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                        '    ElseIf Left(Copy, 1) = "/" Then
                        '    Else
                        '        Copy = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, Copy)
                        '    End If
                        '    Call addStyleLink(Copy, "")
                        '    Copy = cpCore.doc.getNextStyleFilenames()
                        'Loop
                    End If
                End If
                '
                result = result
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        ' ================================================================================================================
        '   Upgrade old objects in content, and update changed resource library images
        ' ================================================================================================================
        '
        Public Function upgradeActiveContent(Source As String) As String
            Dim result As String = Source
            Try
                Dim RecordVirtualPath As String = String.Empty
                Dim RecordVirtualFilename As String
                Dim RecordFilename As String
                Dim RecordFilenameNoExt As String
                Dim RecordFilenameExt As String = String.Empty
                Dim SizeTest() As String
                Dim RecordAltSizeList As String
                Dim TagPosEnd As Integer
                Dim TagPosStart As Integer
                Dim InTag As Boolean
                Dim Pos As Integer
                Dim FilenameSegment As String
                Dim EndPos1 As Integer
                Dim EndPos2 As Integer
                Dim LinkSplit() As String
                Dim LinkCnt As Integer
                Dim LinkPtr As Integer
                Dim TableSplit() As String
                Dim TableName As String
                Dim FieldName As String
                Dim RecordID As Integer
                Dim SaveChanges As Boolean
                Dim EndPos As Integer
                Dim Ptr As Integer
                Dim FilePrefixSegment As String
                Dim ImageAllowUpdate As Boolean
                Dim ContentFilesLinkPrefix As String
                Dim ResourceLibraryLinkPrefix As String
                Dim TestChr As String
                Dim ParseError As Boolean
                result = Source
                '
                ContentFilesLinkPrefix = "/" & cpCore.serverConfig.appConfig.name & "/files/"
                ResourceLibraryLinkPrefix = ContentFilesLinkPrefix & "ccLibraryFiles/"
                ImageAllowUpdate = cpCore.siteProperties.getBoolean("ImageAllowUpdate", True)
                ImageAllowUpdate = ImageAllowUpdate And (InStr(1, Source, ResourceLibraryLinkPrefix, vbTextCompare) <> 0)
                If ImageAllowUpdate Then
                    '
                    ' ----- Process Resource Library Images (swap in most current file)
                    '
                    '   There is a better way:
                    '   problem with replacing the images is the problem with parsing - too much work to find it
                    '   instead, use new replacement tags <ac type=image src="cclibraryfiles/filename/00001" width=0 height=0>
                    '
                    ''hint = hint & ",010"
                    ParseError = False
                    LinkSplit = Split(Source, ContentFilesLinkPrefix, , vbTextCompare)
                    LinkCnt = UBound(LinkSplit) + 1
                    For LinkPtr = 1 To LinkCnt - 1
                        '
                        ' Each LinkSplit(1...) is a segment that would have started with '/appname/files/'
                        ' Next job is to determine if this sement is in a tag (<img src="...">) or in content (&quot...&quote)
                        ' For now, skip the ones in content
                        '
                        ''hint = hint & ",020"
                        TagPosEnd = genericController.vbInstr(1, LinkSplit(LinkPtr), ">")
                        TagPosStart = genericController.vbInstr(1, LinkSplit(LinkPtr), "<")
                        If TagPosEnd = 0 And TagPosStart = 0 Then
                            '
                            ' no tags found, skip it
                            '
                            InTag = False
                        ElseIf TagPosEnd = 0 Then
                            '
                            ' no end tag, but found a start tag -> in content
                            '
                            InTag = False
                        ElseIf TagPosEnd < TagPosStart Then
                            '
                            ' Found end before start - > in tag
                            '
                            InTag = True
                        Else
                            '
                            ' Found start before end -> in content
                            '
                            InTag = False
                        End If
                        If InTag Then
                            ''hint = hint & ",030"
                            TableSplit = Split(LinkSplit(LinkPtr), "/")
                            If UBound(TableSplit) > 2 Then
                                TableName = TableSplit(0)
                                FieldName = TableSplit(1)
                                RecordID = genericController.EncodeInteger(TableSplit(2))
                                FilenameSegment = TableSplit(3)
                                If (LCase(TableName) = "cclibraryfiles") And (LCase(FieldName) = "filename") And (RecordID <> 0) Then
                                    Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, RecordID)
                                    If (file IsNot Nothing) Then
                                        ''hint = hint & ",060"
                                        FieldName = "filename"
                                        'SQL = "select filename,altsizelist from " & TableName & " where id=" & RecordID
                                        'CS = app.csv_OpenCSSQL("default", SQL)
                                        'If app.csv_IsCSOK(CS) Then
                                        If True Then
                                            '
                                            ' now figure out how the link is delimited by how it starts
                                            '   go to the left and look for:
                                            '   ' ' - ignore spaces, continue forward until we find one of these
                                            '   '=' - means space delimited (src=/image.jpg), ends in ' ' or '>'
                                            '   '"' - means quote delimited (src="/image.jpg"), ends in '"'
                                            '   '>' - means this is not in an HTML tag - skip it (<B>image.jpg</b>)
                                            '   '<' - means god knows what, but its wrong, skip it
                                            '   '(' - means it is a URL(/image.jpg), go to ')'
                                            '
                                            ' odd cases:
                                            '   URL( /image.jpg) -
                                            '
                                            RecordVirtualFilename = file.Filename
                                            RecordAltSizeList = file.AltSizeList
                                            If RecordVirtualFilename = genericController.EncodeJavascript(RecordVirtualFilename) Then
                                                '
                                                ' The javascript version of the filename must match the filename, since we have no way
                                                ' of differentiating a ligitimate file, from a block of javascript. If the file
                                                ' contains an apostrophe, the original code could have encoded it, but we can not here
                                                ' so the best plan is to skip it
                                                '
                                                ' example:
                                                ' RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test.png"
                                                '
                                                ' RecordFilename = "test.png"
                                                ' RecordFilenameAltSize = "" (does not exist - the record has the raw filename in it)
                                                ' RecordFilenameExt = "png"
                                                ' RecordFilenameNoExt = "test"
                                                '
                                                ' RecordVirtualFilename = "jay/files/cclibraryfiles/filename/000005/test-100x200.png"
                                                ' this is a specail case - most cases to not have the alt size format saved in the filename
                                                ' RecordFilename = "test-100x200.png"
                                                ' RecordFilenameAltSize (does not exist - the record has the raw filename in it)
                                                ' RecordFilenameExt = "png"
                                                ' RecordFilenameNoExt = "test-100x200"
                                                ' this is wrong
                                                '   xRecordFilenameAltSize = "100x200"
                                                '   xRecordFilenameExt = "png"
                                                '   xRecordFilenameNoExt = "test"
                                                '
                                                ''hint = hint & ",080"
                                                Pos = InStrRev(RecordVirtualFilename, "/")
                                                RecordFilename = ""
                                                If Pos > 0 Then
                                                    RecordVirtualPath = Mid(RecordVirtualFilename, 1, Pos)
                                                    RecordFilename = Mid(RecordVirtualFilename, Pos + 1)
                                                End If
                                                Pos = InStrRev(RecordFilename, ".")
                                                RecordFilenameNoExt = ""
                                                If Pos > 0 Then
                                                    RecordFilenameExt = genericController.vbLCase(Mid(RecordFilename, Pos + 1))
                                                    RecordFilenameNoExt = genericController.vbLCase(Mid(RecordFilename, 1, Pos - 1))
                                                End If
                                                FilePrefixSegment = LinkSplit(LinkPtr - 1)
                                                If Len(FilePrefixSegment) > 1 Then
                                                    '
                                                    ' Look into FilePrefixSegment and see if we are in the querystring attribute of an <AC tag
                                                    '   if so, the filename may be AddonEncoded and delimited with & (so skip it)
                                                    Pos = InStrRev(FilePrefixSegment, "<")
                                                    If Pos > 0 Then
                                                        If genericController.vbLCase(Mid(FilePrefixSegment, Pos + 1, 3)) <> "ac " Then
                                                            '
                                                            ' look back in the FilePrefixSegment to find the character before the link
                                                            '
                                                            EndPos = 0
                                                            For Ptr = Len(FilePrefixSegment) To 1 Step -1
                                                                TestChr = Mid(FilePrefixSegment, Ptr, 1)
                                                                Select Case TestChr
                                                                    Case "="
                                                                        '
                                                                        ' Ends in ' ' or '>', find the first
                                                                        '
                                                                        EndPos1 = genericController.vbInstr(1, FilenameSegment, " ")
                                                                        EndPos2 = genericController.vbInstr(1, FilenameSegment, ">")
                                                                        If EndPos1 <> 0 And EndPos2 <> 0 Then
                                                                            If EndPos1 < EndPos2 Then
                                                                                EndPos = EndPos1
                                                                            Else
                                                                                EndPos = EndPos2
                                                                            End If
                                                                        ElseIf EndPos1 <> 0 Then
                                                                            EndPos = EndPos1
                                                                        ElseIf EndPos2 <> 0 Then
                                                                            EndPos = EndPos2
                                                                        Else
                                                                            EndPos = 0
                                                                        End If
                                                                        Exit For
                                                                    Case """"
                                                                        '
                                                                        ' Quoted, ends is '"'
                                                                        '
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, """")
                                                                        Exit For
                                                                    Case "("
                                                                        '
                                                                        ' url() style, ends in ')' or a ' '
                                                                        '
                                                                        If genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 7)) = "(&quot;" Then
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "&quot;)")
                                                                        ElseIf genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 2)) = "('" Then
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, "')")
                                                                        ElseIf genericController.vbLCase(Mid(FilePrefixSegment, Ptr, 2)) = "(""" Then
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, """)")
                                                                        Else
                                                                            EndPos = genericController.vbInstr(1, FilenameSegment, ")")
                                                                        End If
                                                                        Exit For
                                                                    Case "'"
                                                                        '
                                                                        ' Delimited within a javascript pair of apostophys
                                                                        '
                                                                        EndPos = genericController.vbInstr(1, FilenameSegment, "'")
                                                                        Exit For
                                                                    Case ">", "<"
                                                                        '
                                                                        ' Skip this link
                                                                        '
                                                                        ParseError = True
                                                                        Exit For
                                                                End Select
                                                            Next
                                                            '
                                                            ' check link
                                                            '
                                                            If EndPos = 0 Then
                                                                ParseError = True
                                                                Exit For
                                                            Else
                                                                Dim ImageFilename As String
                                                                Dim SegmentAfterImage As String

                                                                Dim ImageFilenameNoExt As String
                                                                Dim ImageFilenameExt As String
                                                                Dim ImageAltSize As String

                                                                ''hint = hint & ",120"
                                                                SegmentAfterImage = Mid(FilenameSegment, EndPos)
                                                                ImageFilename = genericController.DecodeResponseVariable(Mid(FilenameSegment, 1, EndPos - 1))
                                                                ImageFilenameNoExt = ImageFilename
                                                                ImageFilenameExt = ""
                                                                Pos = InStrRev(ImageFilename, ".")
                                                                If Pos > 0 Then
                                                                    ImageFilenameNoExt = genericController.vbLCase(Mid(ImageFilename, 1, Pos - 1))
                                                                    ImageFilenameExt = genericController.vbLCase(Mid(ImageFilename, Pos + 1))
                                                                End If
                                                                '
                                                                ' Get ImageAltSize
                                                                '
                                                                ''hint = hint & ",130"
                                                                ImageAltSize = ""
                                                                If ImageFilenameNoExt = RecordFilenameNoExt Then
                                                                    '
                                                                    ' Exact match
                                                                    '
                                                                ElseIf genericController.vbInstr(1, ImageFilenameNoExt, RecordFilenameNoExt, vbTextCompare) <> 1 Then
                                                                    '
                                                                    ' There was a change and the recordfilename is not part of the imagefilename
                                                                    '
                                                                Else
                                                                    '
                                                                    ' the recordfilename is the first part of the imagefilename - Get ImageAltSize
                                                                    '
                                                                    ImageAltSize = Mid(ImageFilenameNoExt, Len(RecordFilenameNoExt) + 1)
                                                                    If Left(ImageAltSize, 1) <> "-" Then
                                                                        ImageAltSize = ""
                                                                    Else
                                                                        ImageAltSize = Mid(ImageAltSize, 2)
                                                                        SizeTest = Split(ImageAltSize, "x")
                                                                        If UBound(SizeTest) <> 1 Then
                                                                            ImageAltSize = ""
                                                                        Else
                                                                            If genericController.vbIsNumeric(SizeTest(0)) And genericController.vbIsNumeric(SizeTest(1)) Then
                                                                                ImageFilenameNoExt = RecordFilenameNoExt
                                                                                'ImageFilenameNoExt = Mid(ImageFilenameNoExt, 1, Pos - 1)
                                                                                'RecordFilenameNoExt = Mid(RecordFilename, 1, Pos - 1)
                                                                            Else
                                                                                ImageAltSize = ""
                                                                            End If
                                                                        End If
                                                                    End If
                                                                End If
                                                                '
                                                                ' problem - in the case where the recordfilename = img-100x200, the imagefilenamenoext is img
                                                                '
                                                                ''hint = hint & ",140"
                                                                If (RecordFilenameNoExt <> ImageFilenameNoExt) Or (RecordFilenameExt <> ImageFilenameExt) Then
                                                                    '
                                                                    ' There has been a change
                                                                    '
                                                                    Dim NewRecordFilename As String
                                                                    Dim ImageHeight As Integer
                                                                    Dim ImageWidth As Integer
                                                                    NewRecordFilename = RecordVirtualPath & RecordFilenameNoExt & "." & RecordFilenameExt
                                                                    '
                                                                    ' realtime image updates replace without creating new size - that is for the edit interface
                                                                    '
                                                                    ' put the New file back into the tablesplit in case there are more then 4 splits
                                                                    '
                                                                    TableSplit(0) = ""
                                                                    TableSplit(1) = ""
                                                                    TableSplit(2) = ""
                                                                    TableSplit(3) = SegmentAfterImage
                                                                    NewRecordFilename = genericController.EncodeURL(NewRecordFilename) & Mid(Join(TableSplit, "/"), 4)
                                                                    LinkSplit(LinkPtr) = NewRecordFilename
                                                                    SaveChanges = True
                                                                End If
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        If ParseError Then
                            Exit For
                        End If
                    Next
                    ''hint = hint & ",910"
                    If SaveChanges And (Not ParseError) Then
                        result = Join(LinkSplit, ContentFilesLinkPrefix)
                    End If
                End If
                ''hint = hint & ",920"
                If Not ParseError Then
                    '
                    ' Convert ACTypeDynamicForm to Add-on
                    '
                    If genericController.vbInstr(1, result, "<ac type=""" & ACTypeDynamicForm, vbTextCompare) <> 0 Then
                        result = genericController.vbReplace(result, "type=""DYNAMICFORM""", "TYPE=""aggregatefunction""", 1, 99, vbTextCompare)
                        result = genericController.vbReplace(result, "name=""DYNAMICFORM""", "name=""DYNAMIC FORM""", 1, 99, vbTextCompare)
                    End If
                End If
                ''hint = hint & ",930"
                If ParseError Then
                    result = "" _
                    & vbCrLf & "<!-- warning: parsing aborted on ccLibraryFile replacement -->" _
                    & vbCrLf & result _
                    & vbCrLf & "<!-- /warning: parsing aborted on ccLibraryFile replacement -->"
                End If
                '
                ' {{content}} should be <ac type="templatecontent" etc>
                ' the merge is now handled in csv_EncodeActiveContent, but some sites have hand {{content}} tags entered
                '
                ''hint = hint & ",940"
                If genericController.vbInstr(1, result, "{{content}}", vbTextCompare) <> 0 Then
                    result = genericController.vbReplace(result, "{{content}}", "<AC type=""" & ACTypeTemplateContent & """>", 1, 99, vbTextCompare)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '============================================================================
        '   csv_GetContentCopy3
        '       To get them, cp.content.getCopy must call the cpCoreClass version, which calls this for the content
        '============================================================================
        '
        Public Function html_GetContentCopy(ByVal CopyName As String, ByVal DefaultContent As String, ByVal personalizationPeopleId As Integer, ByVal AllowEditWrapper As Boolean, ByVal personalizationIsAuthenticated As Boolean) As String
            Dim returnCopy As String = ""
            Try
                '
                Dim CS As Integer
                Dim RecordID As Integer
                Dim contactPeopleId As Integer
                Dim Return_ErrorMessage As String = ""
                '
                ' honestly, not sure what to do with 'return_ErrorMessage'
                '
                CS = cpCore.db.csOpen("copy content", "Name=" & cpCore.db.encodeSQLText(CopyName), "ID", , 0, , , "Name,ID,Copy,modifiedBy")
                If Not cpCore.db.csOk(CS) Then
                    Call cpCore.db.csClose(CS)
                    CS = cpCore.db.csInsertRecord("copy content", 0)
                    If cpCore.db.csOk(CS) Then
                        RecordID = cpCore.db.csGetInteger(CS, "ID")
                        Call cpCore.db.csSet(CS, "name", CopyName)
                        Call cpCore.db.csSet(CS, "copy", genericController.encodeText(DefaultContent))
                        Call cpCore.db.csSave2(CS)
                        '   Call cpCore.workflow.publishEdit("copy content", RecordID)
                    End If
                End If
                If cpCore.db.csOk(CS) Then
                    RecordID = cpCore.db.csGetInteger(CS, "ID")
                    contactPeopleId = cpCore.db.csGetInteger(CS, "modifiedBy")
                    returnCopy = cpCore.db.csGet(CS, "Copy")
                    returnCopy = executeContentCommands(Nothing, returnCopy, CPUtilsBaseClass.addonContext.ContextPage, personalizationPeopleId, personalizationIsAuthenticated, Return_ErrorMessage)
                    returnCopy = convertActiveContentToHtmlForWebRender(returnCopy, "copy content", RecordID, personalizationPeopleId, "", 0, CPUtilsBaseClass.addonContext.ContextPage)
                    'returnCopy = convertActiveContent_internal(returnCopy, personalizationPeopleId, "copy content", RecordID, contactPeopleId, False, False, True, True, False, True, "", "", False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, False, Nothing, False)
                    '
                    If True Then
                        If cpCore.doc.authContext.isEditingAnything() Then
                            returnCopy = cpCore.db.csGetRecordEditLink(CS, False) & returnCopy
                            If AllowEditWrapper Then
                                returnCopy = getEditWrapper("copy content", returnCopy)
                            End If
                        End If
                    End If
                End If
                Call cpCore.db.csClose(CS)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnCopy
        End Function
        '
        '
        '
        Public Sub main_AddTabEntry(ByVal Caption As String, ByVal Link As String, ByVal IsHit As Boolean, Optional ByVal StylePrefix As String = "", Optional ByVal LiveBody As String = "")
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            Call cpCore.menuTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix))

            'Call ccAddTabEntry(genericController.encodeText(Caption), genericController.encodeText(Link), genericController.EncodeBoolean(IsHit), genericController.encodeText(StylePrefix), genericController.encodeText(LiveBody))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddTabEntry")
        End Sub
        '        '
        '        '
        '        '
        '        Public Function main_GetTabs() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTabs")
        '            '
        '            ' should use the ccNav object, no the ccCommon module for this code
        '            '
        '            '
        '            main_GetTabs = menuTab.GetTabs()
        '            '    main_GetTabs = ccGetTabs()
        '            '
        '            Exit Function
        'ErrorTrap:
        '            throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetTabs")
        '        End Function
        '
        '
        '
        Public Sub main_AddLiveTabEntry(ByVal Caption As String, ByVal LiveBody As String, Optional ByVal StylePrefix As String = "")
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddLiveTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuLiveTab Is Nothing) Then
                cpCore.doc.menuLiveTab = New menuLiveTabController
            End If
            Call cpCore.doc.menuLiveTab.AddEntry(genericController.encodeText(Caption), genericController.encodeText(LiveBody), genericController.encodeText(StylePrefix))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddLiveTabEntry")
        End Sub
        '
        '
        '
        Public Function main_GetLiveTabs() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLiveTabs")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuLiveTab Is Nothing) Then
                cpCore.doc.menuLiveTab = New menuLiveTabController
            End If
            main_GetLiveTabs = cpCore.doc.menuLiveTab.GetTabs()
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetLiveTabs")
        End Function
        '
        '
        '
        Public Sub menu_AddComboTabEntry(Caption As String, Link As String, AjaxLink As String, LiveBody As String, IsHit As Boolean, ContainerClass As String)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("AddComboTabEntry")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuComboTab Is Nothing) Then
                cpCore.doc.menuComboTab = New menuComboTabController
            End If
            Call cpCore.doc.menuComboTab.AddEntry(Caption, Link, AjaxLink, LiveBody, IsHit, ContainerClass)
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_AddComboTabEntry")
        End Sub
        '
        '
        '
        Public Function menu_GetComboTabs() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetComboTabs")
            '
            ' should use the ccNav object, no the ccCommon module for this code
            '
            If (cpCore.doc.menuComboTab Is Nothing) Then
                cpCore.doc.menuComboTab = New menuComboTabController
            End If
            menu_GetComboTabs = cpCore.doc.menuComboTab.GetTabs()
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError18("main_GetComboTabs")
        End Function
        ''
        ''================================================================================================================
        ''   main_Get SharedStyleFilelist
        ''
        ''   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
        ''   that call out the SharedFileIDList
        ''
        ''   Suffix and Prefix are for Conditional Comments around the style tag
        ''
        ''   SharedStyleFileList is
        ''       crlf filename < Prefix< Suffix
        ''       crlf filename < Prefix< Suffix
        ''       ...
        ''       Prefix and Suffix are htmlencoded
        ''
        ''   SharedStyleMap file
        ''       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        ''       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        ''       ...
        ''       StyleID is 0 if Always include is set
        ''       The Prefix and Suffix have had crlf removed, and comma replaced with &#44;
        ''================================================================================================================
        ''
        'Friend Shared Function main_GetSharedStyleFileList(cpCore As coreClass, SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
        '    Dim result As String = ""
        '    '
        '    Dim Prefix As String
        '    Dim Suffix As String
        '    Dim Files() As String
        '    Dim Pos As Integer
        '    Dim SrcID As Integer
        '    Dim Srcs() As String
        '    Dim SrcCnt As Integer
        '    Dim IncludedStyleFilename As String
        '    Dim styleId As Integer
        '    Dim LastStyleID As Integer
        '    Dim CS As Integer
        '    Dim Ptr As Integer
        '    Dim MapList As String
        '    Dim Map() As String
        '    Dim MapCnt As Integer
        '    Dim MapRow As Integer
        '    Dim Filename As String
        '    Dim FileList As String
        '    Dim SQL As String = String.Empty
        '    Dim BakeName As String
        '    '
        '    If main_IsAdminSite Then
        '        BakeName = "SharedStyleMap-Admin"
        '    Else
        '        BakeName = "SharedStyleMap-Public"
        '    End If
        '    MapList = genericController.encodeText(cpCore.cache.getObject(Of String)(BakeName))
        '    If MapList = "" Then
        '        '
        '        ' BuildMap
        '        '
        '        MapList = ""
        '        If True Then
        '            '
        '            ' add prefix and suffix conditional comments
        '            '
        '            SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
        '                & " from ((ccSharedStyles s" _
        '                & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
        '                & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
        '                & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
        '        End If
        '        CS = cpCore.db.cs_openSql(SQL)
        '        LastStyleID = 0
        '        Do While cpCore.db.cs_ok(CS)
        '            styleId = cpCore.db.cs_getInteger(CS, "ID")
        '            If styleId <> LastStyleID Then
        '                Filename = cpCore.db.cs_get(CS, "StyleFilename")
        '                Prefix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Prefix")), ",", "&#44;")
        '                Suffix = genericController.vbReplace(cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "Suffix")), ",", "&#44;")
        '                If (Not main_IsAdminSite) And cpCore.db.cs_getBoolean(CS, "alwaysinclude") Then
        '                    MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
        '                Else
        '                    MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
        '                End If
        '            End If
        '            IncludedStyleFilename = cpCore.db.cs_getText(CS, "iStylefilename")
        '            Prefix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iPrefix"))
        '            Suffix = cpCore.html.main_encodeHTML(cpCore.db.cs_get(CS, "iSuffix"))
        '            If IncludedStyleFilename <> "" Then
        '                MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
        '            End If
        '            Call cpCore.db.cs_goNext(CS)
        '        Loop
        '        If MapList = "" Then
        '            MapList = ","
        '        End If
        '        Call cpCore.cache.setObject(BakeName, MapList, "Shared Styles")
        '    End If
        '    If (MapList <> "") And (MapList <> ",") Then
        '        Srcs = Split(SharedStyleIDList, ",")
        '        SrcCnt = UBound(Srcs) + 1
        '        Map = Split(MapList, vbCrLf)
        '        MapCnt = UBound(Map) + 1
        '        '
        '        ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
        '        '
        '        FileList = ""
        '        For MapRow = 0 To MapCnt - 1
        '            If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
        '                Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        '                If Pos > 0 Then
        '                    FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        '                End If
        '            End If
        '        Next
        '        '
        '        ' create a filelist of everything that is needed, might be duplicates
        '        '
        '        For Ptr = 0 To SrcCnt - 1
        '            SrcID = genericController.EncodeInteger(Srcs(Ptr))
        '            If SrcID <> 0 Then
        '                For MapRow = 0 To MapCnt - 1
        '                    If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
        '                        Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
        '                        If Pos > 0 Then
        '                            FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
        '                        End If
        '                    End If
        '                Next
        '            End If
        '        Next
        '        '
        '        ' dedup the filelist and convert it to crlf delimited
        '        '
        '        If FileList <> "" Then
        '            Files = Split(FileList, ",")
        '            For Ptr = 0 To UBound(Files)
        '                Filename = Files(Ptr)
        '                If genericController.vbInstr(1, result, Filename, vbTextCompare) = 0 Then
        '                    result = result & vbCrLf & Filename
        '                End If
        '            Next
        '        End If
        '    End If
        '    Return result
        'End Function

        '
        '
        '
        Public Function main_GetResourceLibrary2(ByVal RootFolderName As String, ByVal AllowSelectResource As Boolean, ByVal SelectResourceEditorName As String, ByVal SelectLinkObjectName As String, ByVal AllowGroupAdd As Boolean) As String
            Dim addonGuidResourceLibrary As String = "{564EF3F5-9673-4212-A692-0942DD51FF1A}"
            Dim arguments As New Dictionary(Of String, String)
            arguments.Add("RootFolderName", RootFolderName)
            arguments.Add("AllowSelectResource", AllowSelectResource.ToString())
            arguments.Add("SelectResourceEditorName", SelectResourceEditorName)
            arguments.Add("SelectLinkObjectName", SelectLinkObjectName)
            arguments.Add("AllowGroupAdd", AllowGroupAdd.ToString())
            Return cpCore.addon.execute(
                addonModel.create(cpCore, addonGuidResourceLibrary),
                New CPUtilsBaseClass.addonExecuteContext() With {
                    .addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                    .instanceArguments = arguments
                }
            )
            'Dim Option_String As String
            'Option_String = "" _
            '    & "RootFolderName=" & RootFolderName _
            '    & "&AllowSelectResource=" & AllowSelectResource _
            '    & "&SelectResourceEditorName=" & SelectResourceEditorName _
            '    & "&SelectLinkObjectName=" & SelectLinkObjectName _
            '    & "&AllowGroupAdd=" & AllowGroupAdd _
            '    & ""

            'Return cpCore.addon.execute_legacy4(addonGuidResourceLibrary, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
        End Function
        '
        '========================================================================
        ' Read and save a main_GetFormInputCheckList
        '   see main_GetFormInputCheckList for an explaination of the input
        '========================================================================
        '
        Public Sub main_ProcessCheckList(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As String, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String)
            '
            Dim rulesTablename As String
            Dim SQL As String
            Dim currentRules As DataTable
            Dim currentRulesCnt As Integer
            Dim RuleFound As Boolean
            Dim RuleId As Integer
            Dim Ptr As Integer
            Dim TestRecordIDLast As Integer
            Dim TestRecordID As Integer
            Dim dupRuleIdList As String
            Dim GroupCnt As Integer
            Dim GroupPtr As Integer
            Dim MethodName As String
            Dim SecondaryRecordID As Integer
            Dim RuleNeeded As Boolean
            Dim CSRule As Integer
            Dim RuleContentChanged As Boolean
            Dim SupportRuleCopy As Boolean
            Dim RuleCopy As String
            '
            MethodName = "ProcessCheckList"
            '
            ' --- create Rule records for all selected
            '
            GroupCnt = cpCore.docProperties.getInteger(TagName & ".RowCount")
            If GroupCnt > 0 Then
                '
                ' Test if RuleCopy is supported
                '
                SupportRuleCopy = Models.Complex.cdefModel.isContentFieldSupported(cpCore, RulesContentName, "RuleCopy")
                If SupportRuleCopy Then
                    SupportRuleCopy = SupportRuleCopy And Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "AllowRuleCopy")
                    If SupportRuleCopy Then
                        SupportRuleCopy = SupportRuleCopy And Models.Complex.cdefModel.isContentFieldSupported(cpCore, SecondaryContentName, "RuleCopyCaption")
                    End If
                End If
                '
                ' Go through each checkbox and check for a rule
                '
                '
                ' try
                '
                currentRulesCnt = 0
                dupRuleIdList = ""
                rulesTablename = Models.Complex.cdefModel.getContentTablename(cpCore, RulesContentName)
                SQL = "select " & RulesSecondaryFieldName & ",id from " & rulesTablename & " where (" & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")and(active<>0) order by " & RulesSecondaryFieldName
                currentRulesCnt = 0
                currentRules = cpCore.db.executeQuery(SQL)
                currentRulesCnt = currentRules.Rows.Count
                For GroupPtr = 0 To GroupCnt - 1
                    '
                    ' ----- Read Response
                    '
                    SecondaryRecordID = cpCore.docProperties.getInteger(TagName & "." & GroupPtr & ".ID")
                    RuleCopy = cpCore.docProperties.getText(TagName & "." & GroupPtr & ".RuleCopy")
                    RuleNeeded = cpCore.docProperties.getBoolean(TagName & "." & GroupPtr)
                    '
                    ' ----- Update Record
                    '
                    RuleFound = False
                    RuleId = 0
                    TestRecordIDLast = 0
                    For Ptr = 0 To currentRulesCnt - 1
                        TestRecordID = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(0))
                        If TestRecordID = 0 Then
                            '
                            ' skip
                            '
                        ElseIf TestRecordID = SecondaryRecordID Then
                            '
                            ' hit
                            '
                            RuleFound = True
                            RuleId = genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1))
                            Exit For
                        ElseIf TestRecordID = TestRecordIDLast Then
                            '
                            ' dup
                            '
                            dupRuleIdList = dupRuleIdList & "," & genericController.EncodeInteger(currentRules.Rows(Ptr).Item(1))
                            currentRules.Rows(Ptr).Item(0) = 0
                        End If
                        TestRecordIDLast = TestRecordID
                    Next
                    If SupportRuleCopy And RuleNeeded And (RuleFound) Then
                        '
                        ' Record exists and is needed, update the rule copy
                        '
                        SQL = "update " & rulesTablename & " set rulecopy=" & cpCore.db.encodeSQLText(RuleCopy) & " where id=" & RuleId
                        Call cpCore.db.executeQuery(SQL)
                    ElseIf RuleNeeded And (Not RuleFound) Then
                        '
                        ' No record exists, and one is needed
                        '
                        CSRule = cpCore.db.csInsertRecord(RulesContentName)
                        If cpCore.db.csOk(CSRule) Then
                            Call cpCore.db.csSet(CSRule, "Active", RuleNeeded)
                            Call cpCore.db.csSet(CSRule, RulesPrimaryFieldname, PrimaryRecordID)
                            Call cpCore.db.csSet(CSRule, RulesSecondaryFieldName, SecondaryRecordID)
                            If SupportRuleCopy Then
                                Call cpCore.db.csSet(CSRule, "RuleCopy", RuleCopy)
                            End If
                        End If
                        Call cpCore.db.csClose(CSRule)
                        RuleContentChanged = True
                    ElseIf (Not RuleNeeded) And RuleFound Then
                        '
                        ' Record exists and it is not needed
                        '
                        SQL = "delete from " & rulesTablename & " where id=" & RuleId
                        Call cpCore.db.executeQuery(SQL)
                        RuleContentChanged = True
                    End If
                Next
                '
                ' delete dups
                '
                If dupRuleIdList <> "" Then
                    SQL = "delete from " & rulesTablename & " where id in (" & Mid(dupRuleIdList, 2) & ")"
                    Call cpCore.db.executeQuery(SQL)
                    RuleContentChanged = True
                End If
            End If
            If RuleContentChanged Then
                Call cpCore.cache.invalidateAllObjectsInContent(RulesContentName)
            End If
        End Sub

        ''
        ''========================================================================
        '' ----- Ends an HTML page
        ''========================================================================
        ''
        'Public Function getHtmlDoc_afterBodyHtml() As String
        '    Return "" _
        '        & cr & "</body>" _
        '        & vbCrLf & "</html>"
        'End Function
        '
        '========================================================================
        ' main_GetRecordEditLink( iContentName, iRecordID )
        '
        '   iContentName The content for this link
        '   iRecordID    The ID of the record in the Table
        '========================================================================
        '
        Public Function main_GetRecordEditLink(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal AllowCut As Boolean = False) As String
            main_GetRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", cpCore.doc.authContext.isEditing(ContentName))
        End Function
    End Class
End Namespace
