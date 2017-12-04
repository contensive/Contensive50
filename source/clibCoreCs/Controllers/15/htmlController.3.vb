

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
        '
        Public Function main_GetEditorAddonListJSON(ByVal ContentType As csv_contentTypeEnum) As String
            Dim result As String = String.Empty
            Try
                Dim AddonName As String
                Dim LastAddonName As String = String.Empty
                Dim CSAddons As Integer
                Dim DefaultAddonOption_String As String
                Dim UseAjaxDefaultAddonOptions As Boolean
                Dim PtrTest As Integer
                Dim s As String
                Dim IconWidth As Integer
                Dim IconHeight As Integer
                Dim IconSprites As Integer
                Dim IsInline As Boolean
                Dim AddonGuid As String
                Dim IconIDControlString As String
                Dim IconImg As String
                Dim AddonContentName As String
                Dim ObjectProgramID2 As String
                Dim LoopPtr As Integer
                Dim FieldCaption As String
                Dim SelectList As String
                Dim IconFilename As String
                Dim FieldName As String
                Dim ArgumentList As String
                Dim Index As keyPtrController
                Dim Items() As String
                Dim ItemsSize As Integer
                Dim ItemsCnt As Integer
                Dim ItemsPtr As Integer
                Dim Criteria As String
                Dim CSLists As Integer
                Dim FieldList As String
                Dim cacheKey As String
                '
                ' can not save this because there are multiple main_versions
                '
                cacheKey = "editorAddonList:" & ContentType
                result = cpCore.docProperties.getText(cacheKey)
                If (result = "") Then
                    '
                    ' ----- AC Tags, Would like to replace these with Add-ons eventually
                    '
                    ItemsSize = 100
                    ReDim Items(100)
                    ItemsCnt = 0
                    Index = New keyPtrController
                    'Set main_cmc = main_cs_getv()
                    '
                    ' AC StartBlockText
                    '
                    IconIDControlString = "AC," & ACTypeAggregateFunction & ",0,Block Text,"
                    IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block Start", "Block text to all except selected groups starting at this point", "", 0)
                    IconImg = genericController.EncodeJavascript(IconImg)
                    Items(ItemsCnt) = "['Block Text','" & IconImg & "']"
                    Call Index.setPtr("Block Text", ItemsCnt)
                    ItemsCnt = ItemsCnt + 1
                    '
                    ' AC EndBlockText
                    '
                    IconIDControlString = "AC," & ACTypeAggregateFunction & ",0,Block Text End,"
                    IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Text Block End", "End of text block", "", 0)
                    IconImg = genericController.EncodeJavascript(IconImg)
                    Items(ItemsCnt) = "['Block Text End','" & IconImg & "']"
                    Call Index.setPtr("Block Text", ItemsCnt)
                    ItemsCnt = ItemsCnt + 1
                    '
                    If (ContentType = csv_contentTypeEnum.contentTypeEmail) Or (ContentType = csv_contentTypeEnum.contentTypeEmailTemplate) Then
                        '
                        ' ----- Email Only AC tags
                        '
                        ' Editing Email Body or Templates - Since Email can not process Add-ons, it main_Gets the legacy AC tags for now
                        '
                        ' Personalization Tag
                        '
                        FieldList = Models.Complex.cdefModel.GetContentProperty(cpCore, "people", "SelectFieldList")
                        FieldList = genericController.vbReplace(FieldList, ",", "|")
                        IconIDControlString = "AC,PERSONALIZATION,0,Personalization,field=[" & FieldList & "]"
                        IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Any Personalization Field", "Renders as any Personalization Field", "", 0)
                        IconImg = genericController.EncodeJavascript(IconImg)
                        Items(ItemsCnt) = "['Personalization','" & IconImg & "']"
                        Call Index.setPtr("Personalization", ItemsCnt)
                        ItemsCnt = ItemsCnt + 1
                        '
                        If (ContentType = csv_contentTypeEnum.contentTypeEmailTemplate) Then
                            '
                            ' Editing Email Templates
                            '   This is a special case
                            '   Email content processing can not process add-ons, and PageContentBox and TextBox are needed
                            '   So I added the old AC Tag into the menu for this case
                            '   Need a more consistant solution later
                            '
                            IconIDControlString = "AC," & ACTypeTemplateContent & ",0,Template Content,"
                            IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Content Box", "Renders as the content for a template", "", 0)
                            IconImg = genericController.EncodeJavascript(IconImg)
                            Items(ItemsCnt) = "['Content Box','" & IconImg & "']"
                            'Items(ItemsCnt) = "['Template Content','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Content"" id=""AC," & ACTypeTemplateContent & ",0,Template Content,"" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>']"
                            Call Index.setPtr("Content Box", ItemsCnt)
                            ItemsCnt = ItemsCnt + 1
                            '
                            IconIDControlString = "AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"
                            IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", cpCore.serverConfig.appConfig.cdnFilesNetprefix, "Template Text", "Renders as a template text block", "", 0)
                            IconImg = genericController.EncodeJavascript(IconImg)
                            Items(ItemsCnt) = "['Template Text','" & IconImg & "']"
                            'Items(ItemsCnt) = "['Template Text','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Text"" id=""AC," & ACTypeTemplateText & ",0,Template Text,Name=Default"" src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>']"
                            Call Index.setPtr("Template Text", ItemsCnt)
                            ItemsCnt = ItemsCnt + 1
                        End If
                    Else
                        '
                        ' ----- Web Only AC Tags
                        '
                        ' Watch Lists
                        '
                        CSLists = cpCore.db.csOpen("Content Watch Lists", , "Name,ID", , , , , "Name,ID", 20, 1)
                        If cpCore.db.csOk(CSLists) Then
                            Do While cpCore.db.csOk(CSLists)
                                FieldName = Trim(cpCore.db.csGetText(CSLists, "name"))
                                If FieldName <> "" Then
                                    FieldCaption = "Watch List [" & FieldName & "]"
                                    IconIDControlString = "AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"
                                    IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, 0, 0, 0, True, IconIDControlString, "", cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldCaption, "Rendered as the " & FieldCaption, "", 0)
                                    IconImg = genericController.EncodeJavascript(IconImg)
                                    FieldCaption = genericController.EncodeJavascript(FieldCaption)
                                    Items(ItemsCnt) = "['" & FieldCaption & "','" & IconImg & "']"
                                    'Items(ItemsCnt) = "['" & FieldCaption & "','<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the " & FieldCaption & """ id=""AC,WATCHLIST,0," & FieldName & ",ListName=" & FieldName & "&SortField=[DateAdded|Link|LinkLabel|Clicks|WhatsNewDateExpires]&SortDirection=Z-A[A-Z|Z-A]"" src=""/ccLib/images/ACWatchList.GIF"">']"
                                    Call Index.setPtr(FieldCaption, ItemsCnt)
                                    ItemsCnt = ItemsCnt + 1
                                    If ItemsCnt >= ItemsSize Then
                                        ItemsSize = ItemsSize + 100
                                        ReDim Preserve Items(ItemsSize)
                                    End If
                                End If
                                cpCore.db.csGoNext(CSLists)
                            Loop
                        End If
                        Call cpCore.db.csClose(CSLists)
                    End If
                    '
                    ' ----- Add-ons (AC Aggregate Functions)
                    '
                    If (False) And (ContentType = csv_contentTypeEnum.contentTypeEmail) Then
                        '
                        ' Email did not support add-ons
                        '
                    Else
                        '
                        ' Either non-email or > 4.0.325
                        '
                        Criteria = "(1=1)"
                        If (ContentType = csv_contentTypeEnum.contentTypeEmail) Then
                            '
                            ' select only addons with email placement (dont need to check main_version bc if email, must be >4.0.325
                            '
                            Criteria = Criteria & "and(email<>0)"
                        Else
                            If True Then
                                If (ContentType = csv_contentTypeEnum.contentTypeWeb) Then
                                    '
                                    ' Non Templates
                                    '
                                    Criteria = Criteria & "and(content<>0)"
                                Else
                                    '
                                    ' Templates
                                    '
                                    Criteria = Criteria & "and(template<>0)"
                                End If
                            End If
                        End If
                        AddonContentName = cnAddons
                        SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccguid"
                        CSAddons = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID", , , , , SelectList)
                        If cpCore.db.csOk(CSAddons) Then
                            Do While cpCore.db.csOk(CSAddons)
                                AddonGuid = cpCore.db.csGetText(CSAddons, "ccguid")
                                ObjectProgramID2 = cpCore.db.csGetText(CSAddons, "ObjectProgramID")
                                If (ContentType = csv_contentTypeEnum.contentTypeEmail) And (ObjectProgramID2 <> "") Then
                                    '
                                    ' Block activex addons from email
                                    '
                                    ObjectProgramID2 = ObjectProgramID2
                                Else
                                    AddonName = Trim(cpCore.db.csGet(CSAddons, "name"))
                                    If AddonName <> "" And (AddonName <> LastAddonName) Then
                                        '
                                        ' Icon (fieldtyperesourcelink)
                                        '
                                        IsInline = cpCore.db.csGetBoolean(CSAddons, "IsInline")
                                        IconFilename = cpCore.db.csGet(CSAddons, "Iconfilename")
                                        If IconFilename = "" Then
                                            IconWidth = 0
                                            IconHeight = 0
                                            IconSprites = 0
                                        Else
                                            IconWidth = cpCore.db.csGetInteger(CSAddons, "IconWidth")
                                            IconHeight = cpCore.db.csGetInteger(CSAddons, "IconHeight")
                                            IconSprites = cpCore.db.csGetInteger(CSAddons, "IconSprites")
                                        End If
                                        '
                                        ' Calculate DefaultAddonOption_String
                                        '
                                        UseAjaxDefaultAddonOptions = True
                                        If UseAjaxDefaultAddonOptions Then
                                            DefaultAddonOption_String = ""
                                        Else
                                            ArgumentList = Trim(cpCore.db.csGet(CSAddons, "ArgumentList"))
                                            DefaultAddonOption_String = addonController.main_GetDefaultAddonOption_String(cpCore, ArgumentList, AddonGuid, IsInline)
                                            DefaultAddonOption_String = main_encodeHTML(DefaultAddonOption_String)
                                        End If
                                        '
                                        ' Changes necessary to support commas in AddonName and OptionString
                                        '   Remove commas in Field Name
                                        '   Then in Javascript, when spliting on comma, anything past position 4, put back onto 4
                                        '
                                        LastAddonName = AddonName
                                        IconIDControlString = "AC,AGGREGATEFUNCTION,0," & AddonName & "," & DefaultAddonOption_String & "," & AddonGuid
                                        IconImg = genericController.GetAddonIconImg("/" & cpCore.serverConfig.appConfig.adminRoute, IconWidth, IconHeight, IconSprites, IsInline, IconIDControlString, IconFilename, cpCore.serverConfig.appConfig.cdnFilesNetprefix, AddonName, "Rendered as the Add-on [" & AddonName & "]", "", 0)
                                        Items(ItemsCnt) = "['" & genericController.EncodeJavascript(AddonName) & "','" & genericController.EncodeJavascript(IconImg) & "']"
                                        Call Index.setPtr(AddonName, ItemsCnt)
                                        ItemsCnt = ItemsCnt + 1
                                        If ItemsCnt >= ItemsSize Then
                                            ItemsSize = ItemsSize + 100
                                            ReDim Preserve Items(ItemsSize)
                                        End If
                                    End If
                                End If
                                cpCore.db.csGoNext(CSAddons)
                            Loop
                        End If
                        Call cpCore.db.csClose(CSAddons)
                    End If
                    '
                    ' Build output sting in alphabetical order by name
                    '
                    s = ""
                    ItemsPtr = Index.getFirstPtr
                    Do While ItemsPtr >= 0 And LoopPtr < ItemsCnt
                        s = s & vbCrLf & "," & Items(ItemsPtr)
                        PtrTest = Index.getNextPtr
                        If PtrTest < 0 Then
                            Exit Do
                        Else
                            ItemsPtr = PtrTest
                        End If
                        LoopPtr = LoopPtr + 1
                    Loop
                    If s <> "" Then
                        s = "[" & Mid(s, 4) & "]"
                    End If
                    '
                    result = s
                    Call cpCore.docProperties.setProperty(cacheKey, result, False)
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''========================================================================
        ''   deprecated - see csv_EncodeActiveContent_Internal
        ''========================================================================
        ''
        'Public Function html_EncodeActiveContent4(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String) As String
        '    html_EncodeActiveContent4 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.doc.authContext.isAuthenticated)
        'End Function
        ''
        ''========================================================================
        ''   see csv_EncodeActiveContent_Internal
        ''========================================================================
        ''
        'Public Function html_EncodeActiveContent5(ByVal Source As String, ByVal PeopleID As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal ContextContactPeopleID As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostString As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String, ByVal personalizationIsAuthenticated As Boolean, ByVal Context As CPUtilsBaseClass.addonContext) As String
        '    html_EncodeActiveContent5 = html_EncodeActiveContent_Internal(Source, PeopleID, ContextContentName, ContextRecordID, ContextContactPeopleID, AddLinkEID, EncodeCachableTags, EncodeImages, EncodeEditIcons, EncodeNonCachableTags, AddAnchorQuery, ProtocolHostString, IsEmailContent, AdminURL, cpCore.doc.authContext.isAuthenticated, Context)
        'End Function
        '
        '========================================================================
        '   encode (execute) all {% -- %} commands
        '========================================================================
        '
        Public Function executeContentCommands(ByVal nothingObject As Object, ByVal Source As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal personalizationPeopleId As Integer, ByVal personalizationIsAuthenticated As Boolean, ByRef Return_ErrorMessage As String) As String
            Dim returnValue As String = ""
            Try
                Dim LoopPtr As Integer
                Dim contentCmd As New contentCmdController(cpCore)
                '
                returnValue = Source
                LoopPtr = 0
                Do While (LoopPtr < 10) And ((InStr(1, returnValue, contentReplaceEscapeStart) <> 0))
                    returnValue = contentCmd.ExecuteCmd(returnValue, Context, personalizationPeopleId, personalizationIsAuthenticated)
                    LoopPtr = LoopPtr + 1
                Loop
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnValue
        End Function
        '
        '========================================================================
        ' csv_EncodeActiveContent_Internal
        '       ...
        '       AllowLinkEID    Boolean, if yes, the EID=000... string is added to all links in the content
        '                       Use this for email so links will include the members longin.
        '
        '       Some Active elements can not be replaced here because they incorporate content from  the wbeclient.
        '       For instance the Aggregate Function Objects. These elements create
        '       <!-- FPO1 --> placeholders in the content, and commented instructions, one per line, at the top of the content
        '       Replacement instructions
        '       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        '           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        '       <!-- Replace FPO1,AFObject,ObjectName,OptionString -->
        '           Aggregate Function Object, ProgramID=ObjectName, Optionstring=Optionstring
        '
        ' Tag descriptions:
        '
        '   primary methods
        '
        '   <Ac Type="Date">
        '   <Ac Type="Member" Field="Name">
        '   <Ac Type="Organization" Field="Name">
        '   <Ac Type="Visitor" Field="Name">
        '   <Ac Type="Visit" Field="Name">
        '   <Ac Type="Contact" Member="PeopleID">
        '       displays a For More info block of copy
        '   <Ac Type="Feedback" Member="PeopleID">
        '       displays a feedback note block
        '   <Ac Type="ChildList" Name="ChildListName">
        '       displays a list of child blocks that reference this CHildList Element
        '   <Ac Type="Language" Name="All|English|Spanish|etc.">
        '       blocks content to next language tag to eveyone without this PeopleLanguage
        '   <Ac Type="Image" Record="" Width="" Height="" Alt="" Align="">
        '   <AC Type="Download" Record="" Alt="">
        '       renders as an anchored download icon, with the alt tag
        '       the rendered anchor points back to the root/index, which increments the resource's download count
        '
        '   During Editing, AC tags are converted (Encoded) to EditIcons
        '       these are image tags with AC information stored in the ID attribute
        '       except AC-Image, which are converted into the actual image for editing
        '       during the edit save, the EditIcons are converted (Decoded) back
        '
        '   Remarks
        '
        '   First <Member.FieldName> encountered opens the Members Table, etc.
        '       ( does <OpenTable name="Member" Tablename="ccMembers" ID=(current PeopleID)> )
        '   The copy is divided into Blocks, starting at every tag and running to the next tag.
        '   BlockTag()  The tag without the braces found
        '   BlockCopy() The copy following the tag up to the next tag
        '   BlockLabel()    the string identifier for the block
        '   BlockCount  the total blocks in the message
        '   BlockPointer    the current block being examined
        '========================================================================
        '
        Private Function convertActiveContent_Internal_activeParts(ByVal Source As String, ByVal personalizationPeopleId As Integer, ByVal ContextContentName As String, ByVal ContextRecordID As Integer, ByVal moreInfoPeopleId As Integer, ByVal AddLinkEID As Boolean, ByVal EncodeCachableTags As Boolean, ByVal EncodeImages As Boolean, ByVal EncodeEditIcons As Boolean, ByVal EncodeNonCachableTags As Boolean, ByVal AddAnchorQuery As String, ByVal ProtocolHostLink As String, ByVal IsEmailContent As Boolean, ByVal AdminURL As String, ByVal personalizationIsAuthenticated As Boolean, Optional ByVal context As CPUtilsBaseClass.addonContext = CPUtilsBaseClass.addonContext.ContextPage) As String
            Dim result As String = ""
            Try
                Dim ACGuid As String
                Dim AddonFound As Boolean
                Dim ACNameCaption As String
                Dim GroupIDList As String
                Dim IDControlString As String
                Dim IconIDControlString As String
                Dim Criteria As String
                Dim AddonContentName As String
                Dim SelectList As String = ""
                Dim IconWidth As Integer
                Dim IconHeight As Integer
                Dim IconSprites As Integer
                Dim AddonIsInline As Boolean
                Dim IconAlt As String = ""
                Dim IconTitle As String = ""
                Dim IconImg As String
                Dim TextName As String
                Dim ListName As String
                Dim SrcOptionSelector As String
                Dim ResultOptionSelector As String
                Dim SrcOptionList As String
                Dim Pos As Integer
                Dim REsultOptionValue As String
                Dim SrcOptionValueSelector As String
                Dim InstanceOptionValue As String
                Dim ResultOptionListHTMLEncoded As String
                Dim UCaseACName As String
                Dim IconFilename As String
                Dim FieldName As String
                Dim Ptr As Integer
                Dim ElementPointer As Integer
                Dim ListCount As Integer
                Dim CSVisitor As Integer
                Dim CSVisit As Integer
                Dim CSVisitorSet As Boolean
                Dim CSVisitSet As Boolean
                Dim ElementTag As String
                Dim ACType As String
                Dim ACField As String
                Dim ACName As String = ""
                Dim Copy As String
                Dim KmaHTML As htmlParserController
                Dim AttributeCount As Integer
                Dim AttributePointer As Integer
                Dim Name As String
                Dim Value As String
                Dim CS As Integer
                Dim ACAttrRecordID As Integer
                Dim ACAttrWidth As Integer
                Dim ACAttrHeight As Integer
                Dim ACAttrAlt As String
                Dim ACAttrBorder As Integer
                Dim ACAttrLoop As Integer
                Dim ACAttrVSpace As Integer
                Dim ACAttrHSpace As Integer
                Dim Filename As String = ""
                Dim ACAttrAlign As String
                Dim ProcessAnchorTags As Boolean
                Dim ProcessACTags As Boolean
                Dim ACLanguageName As String
                Dim Stream As New stringBuilderLegacyController
                Dim AnchorQuery As String = ""
                Dim CSOrganization As Integer
                Dim CSOrganizationSet As Boolean
                Dim CSPeople As Integer
                Dim CSPeopleSet As Boolean
                Dim CSlanguage As Integer
                Dim PeopleLanguageSet As Boolean
                Dim PeopleLanguage As String = ""
                Dim UcasePeopleLanguage As String
                Dim serverFilePath As String = ""
                Dim ReplaceInstructions As String = String.Empty
                Dim Link As String
                Dim NotUsedID As Integer
                Dim addonOptionString As String
                Dim AddonOptionStringHTMLEncoded As String
                Dim SrcOptions() As String
                Dim SrcOptionName As String
                Dim FormCount As Integer
                Dim FormInputCount As Integer
                Dim ACInstanceID As String
                Dim PosStart As Integer
                Dim PosEnd As Integer
                Dim AllowGroups As String
                Dim workingContent As String
                Dim NewName As String
                '
                workingContent = Source
                '
                ' Fixup Anchor Query (additional AddonOptionString pairs to add to the end)
                '
                If AddLinkEID And (personalizationPeopleId <> 0) Then
                    AnchorQuery = AnchorQuery & "&EID=" & cpCore.security.encodeToken(genericController.EncodeInteger(personalizationPeopleId), Now())
                End If
                '
                If AddAnchorQuery <> "" Then
                    AnchorQuery = AnchorQuery & "&" & AddAnchorQuery
                End If
                '
                If AnchorQuery <> "" Then
                    AnchorQuery = Mid(AnchorQuery, 2)
                End If
                '
                ' ----- xml contensive process instruction
                '
                'TemplateBodyContent
                'Pos = genericController.vbInstr(1, TemplateBodyContent, "<?contensive", vbTextCompare)
                'If Pos > 0 Then
                '    '
                '    ' convert template body if provided - this is the content that replaces the content box addon
                '    '
                '    TemplateBodyContent = Mid(TemplateBodyContent, Pos)
                '    LayoutEngineOptionString = "data=" & encodeNvaArgument(TemplateBodyContent)
                '    TemplateBodyContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", nothing, LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                'End If
                Pos = genericController.vbInstr(1, workingContent, "<?contensive", vbTextCompare)
                If Pos > 0 Then
                    Throw New ApplicationException("Structured xml data commands are no longer supported")
                    ''
                    '' convert content if provided
                    ''
                    'workingContent = Mid(workingContent, Pos)
                    'LayoutEngineOptionString = "data=" & encodeNvaArgument(workingContent)
                    'Dim structuredData As New core_primitivesStructuredDataClass(Me)
                    'workingContent = structuredData.execute()
                    'workingContent = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", LayoutEngineOptionString, "data=(structured data)", LayoutErrorMessage)
                End If
                '
                ' Special Case
                ' Convert <!-- STARTGROUPACCESS 10,11,12 --> format to <AC type=GROUPACCESS AllowGroups="10,11,12">
                ' Convert <!-- ENDGROUPACCESS --> format to <AC type=GROUPACCESSEND>
                '
                PosStart = genericController.vbInstr(1, workingContent, "<!-- STARTGROUPACCESS ", vbTextCompare)
                If PosStart > 0 Then
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->")
                    If PosEnd > 0 Then
                        AllowGroups = Mid(workingContent, PosStart + 22, PosEnd - PosStart - 23)
                        workingContent = Mid(workingContent, 1, PosStart - 1) & "<AC type=""" & ACTypeAggregateFunction & """ name=""block text"" querystring=""allowgroups=" & AllowGroups & """>" & Mid(workingContent, PosEnd + 3)
                    End If
                End If
                '
                PosStart = genericController.vbInstr(1, workingContent, "<!-- ENDGROUPACCESS ", vbTextCompare)
                If PosStart > 0 Then
                    PosEnd = genericController.vbInstr(PosStart, workingContent, "-->")
                    If PosEnd > 0 Then
                        workingContent = Mid(workingContent, 1, PosStart - 1) & "<AC type=""" & ACTypeAggregateFunction & """ name=""block text end"" >" & Mid(workingContent, PosEnd + 3)
                    End If
                End If
                '
                ' ----- Special case -- if any of these are in the source, this is legacy. Convert them to icons,
                '       and they will be converted to AC tags when the icons are saved
                '
                If EncodeEditIcons Then
                    '
                    IconIDControlString = "AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","
                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as [Template Page Content]", "", 0)
                    workingContent = genericController.vbReplace(workingContent, "{{content}}", IconImg, 1, 99, vbTextCompare)
                    'WorkingContent = genericController.vbReplace(WorkingContent, "{{content}}", "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACTypeTemplateContent & "," & NotUsedID & "," & ACName & ","" src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>", 1, -1, vbTextCompare)
                    ''
                    '' replace all other {{...}}
                    ''
                    'LoopPtr = 0
                    'Pos = 1
                    'Do While Pos > 0 And LoopPtr < 100
                    '    Pos = genericController.vbInstr(Pos, workingContent, "{{" & ACTypeDynamicMenu, vbTextCompare)
                    '    If Pos > 0 Then
                    '        addonOptionString = ""
                    '        PosStart = Pos
                    '        If PosStart <> 0 Then
                    '            'PosStart = PosStart + 2 + Len(ACTypeDynamicMenu)
                    '            PosEnd = genericController.vbInstr(PosStart, workingContent, "}}", vbTextCompare)
                    '            If PosEnd <> 0 Then
                    '                Cmd = Mid(workingContent, PosStart + 2, PosEnd - PosStart - 2)
                    '                Pos = genericController.vbInstr(1, Cmd, "?")
                    '                If Pos <> 0 Then
                    '                    addonOptionString = genericController.decodeHtml(Mid(Cmd, Pos + 1))
                    '                End If
                    '                TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                    '                '
                    '                addonOptionString = "Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                    '                AddonOptionStringHTMLEncoded = html_EncodeHTML("Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu=")
                    '                '
                    '                IconIDControlString = "AC," & ACTypeDynamicMenu & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                    '                IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as [Dynamic Menu]", "", 0)
                    '                workingContent = Mid(workingContent, 1, PosStart - 1) & IconImg & Mid(workingContent, PosEnd + 2)
                    '            End If
                    '        End If
                    '    End If
                    'Loop
                End If
                '
                ' Test early if this needs to run at all
                '
                ProcessACTags = ((EncodeCachableTags Or EncodeNonCachableTags Or EncodeImages Or EncodeEditIcons)) And (InStr(1, workingContent, "<AC ", vbTextCompare) <> 0)
                ProcessAnchorTags = (AnchorQuery <> "") And (InStr(1, workingContent, "<A ", vbTextCompare) <> 0)
                If (workingContent <> "") And (ProcessAnchorTags Or ProcessACTags) Then
                    '
                    ' ----- Load the Active Elements
                    '
                    KmaHTML = New htmlParserController(cpCore)
                    Call KmaHTML.Load(workingContent)
                    '
                    ' ----- Execute and output elements
                    '
                    ElementPointer = 0
                    If KmaHTML.ElementCount > 0 Then
                        ElementPointer = 0
                        workingContent = ""
                        serverFilePath = ProtocolHostLink & "/" & cpCore.serverConfig.appConfig.name & "/files/"
                        Stream = New stringBuilderLegacyController
                        Do While ElementPointer < KmaHTML.ElementCount
                            Copy = KmaHTML.Text(ElementPointer)
                            If KmaHTML.IsTag(ElementPointer) Then
                                ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
                                UCaseACName = genericController.vbUCase(ACName)
                                Select Case ElementTag
                                    Case "FORM"
                                        '
                                        ' Form created in content
                                        ' EncodeEditIcons -> remove the
                                        '
                                        If EncodeNonCachableTags Then
                                            FormCount = FormCount + 1
                                            '
                                            ' 5/14/2009 - DM said it is OK to remove UserResponseForm Processing
                                            ' however, leave this one because it is needed to make current forms work.
                                            '
                                            If (InStr(1, Copy, "contensiveuserform=1", vbTextCompare) <> 0) Or (InStr(1, Copy, "contensiveuserform=""1""", vbTextCompare) <> 0) Then
                                                '
                                                ' if it has "contensiveuserform=1" in the form tag, remove it from the form and add the hidden that makes it work
                                                '
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=1", "", 1, 99, vbTextCompare)
                                                Copy = genericController.vbReplace(Copy, "ContensiveUserForm=""1""", "", 1, 99, vbTextCompare)
                                                If Not EncodeEditIcons Then
                                                    Copy = Copy & "<input type=hidden name=ContensiveUserForm value=1>"
                                                End If
                                            End If
                                        End If
                                    Case "INPUT"
                                        If EncodeNonCachableTags Then
                                            FormInputCount = FormInputCount + 1
                                        End If
                                    Case "A"
                                        If (AnchorQuery <> "") Then
                                            '
                                            ' ----- Add ?eid=0000 to all anchors back to the same site so emails
                                            '       can be sent that will automatically log the person in when they
                                            '       arrive.
                                            '
                                            AttributeCount = KmaHTML.ElementAttributeCount(ElementPointer)
                                            If AttributeCount > 0 Then
                                                Copy = "<A "
                                                For AttributePointer = 0 To AttributeCount - 1
                                                    Name = KmaHTML.ElementAttributeName(ElementPointer, AttributePointer)
                                                    Value = KmaHTML.ElementAttributeValue(ElementPointer, AttributePointer)
                                                    If genericController.vbUCase(Name) = "HREF" Then
                                                        Link = Value
                                                        Pos = genericController.vbInstr(1, Link, "://")
                                                        If Pos > 0 Then
                                                            Link = Mid(Link, Pos + 3)
                                                            Pos = genericController.vbInstr(1, Link, "/")
                                                            If Pos > 0 Then
                                                                Link = Left(Link, Pos - 1)
                                                            End If
                                                        End If
                                                        If (Link = "") Or (InStr(1, "," & cpCore.serverConfig.appConfig.domainList(0) & ",", "," & Link & ",", vbTextCompare) <> 0) Then
                                                            '
                                                            ' ----- link is for this site
                                                            '
                                                            If Right(Value, 1) = "?" Then
                                                                '
                                                                ' Ends in a questionmark, must be Dwayne (?)
                                                                '
                                                                Value = Value & AnchorQuery
                                                            ElseIf genericController.vbInstr(1, Value, "mailto:", vbTextCompare) <> 0 Then
                                                                '
                                                                ' catch mailto
                                                                '
                                                                'Value = Value & AnchorQuery
                                                            ElseIf genericController.vbInstr(1, Value, "?") = 0 Then
                                                                '
                                                                ' No questionmark there, add it
                                                                '
                                                                Value = Value & "?" & AnchorQuery
                                                            Else
                                                                '
                                                                ' Questionmark somewhere, add new value with amp;
                                                                '
                                                                Value = Value & "&" & AnchorQuery
                                                            End If
                                                            '    End If
                                                        End If
                                                    End If
                                                    Copy = Copy & " " & Name & "=""" & Value & """"
                                                Next
                                                Copy = Copy & ">"
                                            End If
                                        End If
                                    Case "AC"
                                        '
                                        ' ----- decode all AC tags
                                        '
                                        ListCount = 0
                                        ACType = KmaHTML.ElementAttribute(ElementPointer, "TYPE")
                                        ' if ACInstanceID=0, it can not create settings link in edit mode. ACInstanceID is added during edit save.
                                        ACInstanceID = KmaHTML.ElementAttribute(ElementPointer, "ACINSTANCEID")
                                        ACGuid = KmaHTML.ElementAttribute(ElementPointer, "GUID")
                                        Select Case genericController.vbUCase(ACType)
                                            Case ACTypeEnd
                                                '
                                                ' End Tag - Personalization
                                                '       This tag causes an end to the all tags, like Language
                                                '       It is removed by with EncodeEditIcons (on the way to the editor)
                                                '       It is added to the end of the content with Decode(activecontent)
                                                '
                                                If EncodeEditIcons Then
                                                    Copy = ""
                                                ElseIf EncodeNonCachableTags Then
                                                    Copy = "<!-- Language ANY -->"
                                                End If
                                            Case ACTypeDate
                                                '
                                                ' Date Tag
                                                '
                                                If EncodeEditIcons Then
                                                    IconIDControlString = "AC," & ACTypeDate
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "Current Date", "Renders as [Current Date]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as the current date"" ID=""AC," & ACTypeDate & """ src=""/ccLib/images/ACDate.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    Copy = DateTime.Now.ToString
                                                End If
                                            Case ACTypeMember, ACTypePersonalization
                                                '
                                                ' Member Tag works regardless of authentication
                                                ' cm must be sure not to reveal anything
                                                '
                                                ACField = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "FIELD"))
                                                If ACField = "" Then
                                                    ' compatibility for old personalization type
                                                    ACField = htmlController.getAddonOptionStringValue("FIELD", KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING"))
                                                End If
                                                FieldName = genericController.EncodeInitialCaps(ACField)
                                                If (FieldName = "") Then
                                                    FieldName = "Name"
                                                End If
                                                If EncodeEditIcons Then
                                                    Select Case genericController.vbUCase(FieldName)
                                                        Case "FIRSTNAME"
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's First Name", "Renders as [User's First Name]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                        '
                                                        Case "LASTNAME"
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's Last Name", "Renders as [User's Last Name]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                        Case Else
                                                            '
                                                            IconIDControlString = "AC," & ACType & "," & FieldName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "User's " & FieldName, "Renders as [User's " & FieldName & "]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                    End Select
                                                ElseIf EncodeNonCachableTags Then
                                                    If personalizationPeopleId <> 0 Then
                                                        If genericController.vbUCase(FieldName) = "EID" Then
                                                            Copy = cpCore.security.encodeToken(personalizationPeopleId, Now())
                                                        Else
                                                            If Not CSPeopleSet Then
                                                                CSPeople = cpCore.db.cs_openContentRecord("People", personalizationPeopleId)
                                                                CSPeopleSet = True
                                                            End If
                                                            If cpCore.db.csOk(CSPeople) And cpCore.db.cs_isFieldSupported(CSPeople, FieldName) Then
                                                                Copy = cpCore.db.csGetLookup(CSPeople, FieldName)
                                                            End If
                                                        End If
                                                    End If
                                                End If
                                            Case ACTypeChildList
                                                '
                                                ' Child List
                                                '
                                                ListName = genericController.encodeText((KmaHTML.ElementAttribute(ElementPointer, "name")))

                                                If EncodeEditIcons Then
                                                    IconIDControlString = "AC," & ACType & ",," & ACName
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "List of Child Pages", "Renders as [List of Child Pages]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                ElseIf EncodeCachableTags Then
                                                    '
                                                    ' Handle in webclient
                                                    '
                                                    ' removed sort method because all child pages are read in together in the order set by the parent - improve this later
                                                    Copy = "{{" & ACTypeChildList & "?name=" & genericController.encodeNvaArgument(ListName) & "}}"
                                                End If
                                            Case ACTypeContact
                                                '
                                                ' Formatting Tag
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "Contact Information Line", "Renders as [Contact Information Line]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a line of text with contact information for this record's primary contact"" id=""AC," & ACType & """ src=""/ccLib/images/ACContact.GIF"">"
                                                ElseIf EncodeCachableTags Then
                                                    If moreInfoPeopleId <> 0 Then
                                                        Copy = pageContentController.getMoreInfoHtml(cpCore, moreInfoPeopleId)
                                                    End If
                                                End If
                                            Case ACTypeFeedback
                                                '
                                                ' Formatting tag - change from information to be included after submission
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, False, IconIDControlString, "", serverFilePath, "Feedback Form", "Renders as [Feedback Form]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Add-on"" title=""Rendered as a feedback form, sent to this record's primary contact."" id=""AC," & ACType & """ src=""/ccLib/images/ACFeedBack.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    If (moreInfoPeopleId <> 0) And (ContextContentName <> "") And (ContextRecordID <> 0) Then
                                                        Copy = FeedbackFormNotSupportedComment
                                                    End If
                                                End If
                                            Case ACTypeLanguage
                                                '
                                                ' Personalization Tag - block languages not from the visitor
                                                '
                                                ACLanguageName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                If EncodeEditIcons Then
                                                    Select Case genericController.vbUCase(ACLanguageName)
                                                        Case "ANY"
                                                            '
                                                            IconIDControlString = "AC," & ACType & ",," & ACLanguageName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "All copy following this point is rendered, regardless of the member's language setting", "Renders as [Begin Rendering All Languages]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered, regardless of the member's language setting"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageAny.GIF"">"
                                                            'Case "ENGLISH", "FRENCH", "GERMAN", "PORTUGEUESE", "ITALIAN", "SPANISH", "CHINESE", "HINDI"
                                                            '   Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguage" & ACLanguageName & ".GIF"">"
                                                        Case Else
                                                            '
                                                            IconIDControlString = "AC," & ACType & ",," & ACLanguageName
                                                            IconImg = genericController.GetAddonIconImg(AdminURL, 0, 0, 0, True, IconIDControlString, "", serverFilePath, "All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]", "Begin Rendering for language [" & ACLanguageName & "]", ACInstanceID, 0)
                                                            Copy = IconImg
                                                            '
                                                            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""All copy following this point is rendered if the member's language setting matchs [" & ACLanguageName & "]"" id=""AC," & ACType & ",," & ACLanguageName & """ src=""/ccLib/images/ACLanguageOther.GIF"">"
                                                    End Select
                                                ElseIf EncodeNonCachableTags Then
                                                    If personalizationPeopleId = 0 Then
                                                        PeopleLanguage = "any"
                                                    Else
                                                        If Not PeopleLanguageSet Then
                                                            If Not CSPeopleSet Then
                                                                CSPeople = cpCore.db.cs_openContentRecord("people", personalizationPeopleId)
                                                                CSPeopleSet = True
                                                            End If
                                                            CSlanguage = cpCore.db.cs_openContentRecord("Languages", cpCore.db.csGetInteger(CSPeople, "LanguageID"), , , , "Name")
                                                            If cpCore.db.csOk(CSlanguage) Then
                                                                PeopleLanguage = cpCore.db.csGetText(CSlanguage, "name")
                                                            End If
                                                            Call cpCore.db.csClose(CSlanguage)
                                                            PeopleLanguageSet = True
                                                        End If
                                                    End If
                                                    UcasePeopleLanguage = genericController.vbUCase(PeopleLanguage)
                                                    If UcasePeopleLanguage = "ANY" Then
                                                        '
                                                        ' This person wants all the languages, put in language marker and continue
                                                        '
                                                        Copy = "<!-- Language " & ACLanguageName & " -->"
                                                    ElseIf (ACLanguageName <> UcasePeopleLanguage) And (ACLanguageName <> "ANY") Then
                                                        '
                                                        ' Wrong language, remove tag, skip to the end, or to the next language tag
                                                        '
                                                        Copy = ""
                                                        ElementPointer = ElementPointer + 1
                                                        Do While ElementPointer < KmaHTML.ElementCount
                                                            ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                                            If (ElementTag = "AC") Then
                                                                ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"))
                                                                If (ACType = ACTypeLanguage) Then
                                                                    ElementPointer = ElementPointer - 1
                                                                    Exit Do
                                                                ElseIf (ACType = ACTypeEnd) Then
                                                                    Exit Do
                                                                End If
                                                            End If
                                                            ElementPointer = ElementPointer + 1
                                                        Loop
                                                    Else
                                                        '
                                                        ' Right Language, remove tag
                                                        '
                                                        Copy = ""
                                                    End If
                                                End If
                                            Case ACTypeAggregateFunction
                                                '
                                                ' ----- Add-on
                                                '
                                                NotUsedID = 0
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                If IsEmailContent Then
                                                    '
                                                    ' Addon - for email
                                                    '
                                                    If EncodeNonCachableTags Then
                                                        '
                                                        ' Only hardcoded Add-ons can run in Emails
                                                        '
                                                        Select Case genericController.vbLCase(ACName)
                                                            Case "block text"
                                                                '
                                                                ' Email is always considered authenticated bc they need their login credentials to get the email.
                                                                ' Allowed to see the content that follows if you are authenticated, admin, or in the group list
                                                                ' This must be done out on the page because the csv does not know about authenticated
                                                                '
                                                                Copy = ""
                                                                GroupIDList = htmlController.getAddonOptionStringValue("AllowGroups", addonOptionString)
                                                                If (Not cpCore.doc.authContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, True, GroupIDList, True)) Then
                                                                    '
                                                                    ' Block content if not allowed
                                                                    '
                                                                    ElementPointer = ElementPointer + 1
                                                                    Do While ElementPointer < KmaHTML.ElementCount
                                                                        ElementTag = genericController.vbUCase(KmaHTML.TagName(ElementPointer))
                                                                        If (ElementTag = "AC") Then
                                                                            ACType = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "TYPE"))
                                                                            If (ACType = ACTypeAggregateFunction) Then
                                                                                If genericController.vbLCase(KmaHTML.ElementAttribute(ElementPointer, "name")) = "block text end" Then
                                                                                    Exit Do
                                                                                End If
                                                                            End If
                                                                        End If
                                                                        ElementPointer = ElementPointer + 1
                                                                    Loop
                                                                End If
                                                            Case "block text end"
                                                                '
                                                                ' always remove end tags because the block text did not remove it
                                                                '
                                                                Copy = ""
                                                            Case Else
                                                                '
                                                                ' all other add-ons, pass out to cpCoreClass to process
                                                                Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                                                                    .addonType = CPUtilsBaseClass.addonContext.ContextEmail,
                                                                    .cssContainerClass = "",
                                                                    .cssContainerId = "",
                                                                    .hostRecord = New CPUtilsBaseClass.addonExecuteHostRecordContext() With {
                                                                        .contentName = ContextContentName,
                                                                        .fieldName = "",
                                                                        .recordId = ContextRecordID
                                                                    },
                                                                    .personalizationAuthenticated = personalizationIsAuthenticated,
                                                                    .personalizationPeopleId = personalizationPeopleId,
                                                                    .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, AddonOptionStringHTMLEncoded),
                                                                    .instanceGuid = ACInstanceID
                                                                }
                                                                Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.createByName(cpCore, ACName)
                                                                Copy = cpCore.addon.execute(addon, executeContext)
                                                                'Copy = cpCore.addon.execute_legacy6(0, ACName, AddonOptionStringHTMLEncoded, CPUtilsBaseClass.addonContext.ContextEmail, "", 0, "", ACInstanceID, False, 0, "", True, Nothing, "", Nothing, "", personalizationPeopleId, personalizationIsAuthenticated)
                                                        End Select
                                                    End If
                                                Else
                                                    '
                                                    ' Addon - for web
                                                    '

                                                    If EncodeEditIcons Then
                                                        '
                                                        ' Get IconFilename, update the optionstring, and execute optionstring replacement functions
                                                        '
                                                        AddonContentName = cnAddons
                                                        If True Then
                                                            SelectList = "Name,Link,ID,ArgumentList,ObjectProgramID,IconFilename,IconWidth,IconHeight,IconSprites,IsInline,ccGuid"
                                                        End If
                                                        If ACGuid <> "" Then
                                                            Criteria = "ccguid=" & cpCore.db.encodeSQLText(ACGuid)
                                                        Else
                                                            Criteria = "name=" & cpCore.db.encodeSQLText(UCaseACName)
                                                        End If
                                                        CS = cpCore.db.csOpen(AddonContentName, Criteria, "Name,ID", , , , , SelectList)
                                                        If cpCore.db.csOk(CS) Then
                                                            AddonFound = True
                                                            ' ArgumentList comes in already encoded
                                                            IconFilename = cpCore.db.csGet(CS, "IconFilename")
                                                            SrcOptionList = cpCore.db.csGet(CS, "ArgumentList")
                                                            IconWidth = cpCore.db.csGetInteger(CS, "IconWidth")
                                                            IconHeight = cpCore.db.csGetInteger(CS, "IconHeight")
                                                            IconSprites = cpCore.db.csGetInteger(CS, "IconSprites")
                                                            AddonIsInline = cpCore.db.csGetBoolean(CS, "IsInline")
                                                            ACGuid = cpCore.db.csGetText(CS, "ccGuid")
                                                            IconAlt = ACName
                                                            IconTitle = "Rendered as the Add-on [" & ACName & "]"
                                                        Else
                                                            Select Case genericController.vbLCase(ACName)
                                                                Case "block text"
                                                                    IconFilename = ""
                                                                    SrcOptionList = AddonOptionConstructor_ForBlockText
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = True
                                                                    ACGuid = ""
                                                                Case "block text end"
                                                                    IconFilename = ""
                                                                    SrcOptionList = ""
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = True
                                                                    ACGuid = ""
                                                                Case Else
                                                                    IconFilename = ""
                                                                    SrcOptionList = ""
                                                                    IconWidth = 0
                                                                    IconHeight = 0
                                                                    IconSprites = 0
                                                                    AddonIsInline = False
                                                                    IconAlt = "Unknown Add-on [" & ACName & "]"
                                                                    IconTitle = "Unknown Add-on [" & ACName & "]"
                                                                    ACGuid = ""
                                                            End Select
                                                        End If
                                                        Call cpCore.db.csClose(CS)
                                                        '
                                                        ' Build AddonOptionStringHTMLEncoded from SrcOptionList (for names), itself (for current settings), and SrcOptionList (for select options)
                                                        '
                                                        If (InStr(1, SrcOptionList, "wrapper", vbTextCompare) = 0) Then
                                                            If AddonIsInline Then
                                                                SrcOptionList = SrcOptionList & vbCrLf & AddonOptionConstructor_Inline
                                                            Else
                                                                SrcOptionList = SrcOptionList & vbCrLf & AddonOptionConstructor_Block
                                                            End If
                                                        End If
                                                        If SrcOptionList = "" Then
                                                            ResultOptionListHTMLEncoded = ""
                                                        Else
                                                            ResultOptionListHTMLEncoded = ""
                                                            REsultOptionValue = ""
                                                            SrcOptionList = genericController.vbReplace(SrcOptionList, vbCrLf, vbCr)
                                                            SrcOptionList = genericController.vbReplace(SrcOptionList, vbLf, vbCr)
                                                            SrcOptions = Split(SrcOptionList, vbCr)
                                                            For Ptr = 0 To UBound(SrcOptions)
                                                                SrcOptionName = SrcOptions(Ptr)
                                                                Dim LoopPtr2 As Integer

                                                                LoopPtr2 = 0
                                                                Do While (Len(SrcOptionName) > 1) And (Mid(SrcOptionName, 1, 1) = vbTab) And (LoopPtr2 < 100)
                                                                    SrcOptionName = Mid(SrcOptionName, 2)
                                                                    LoopPtr2 = LoopPtr2 + 1
                                                                Loop
                                                                SrcOptionValueSelector = ""
                                                                SrcOptionSelector = ""
                                                                Pos = genericController.vbInstr(1, SrcOptionName, "=")
                                                                If Pos > 0 Then
                                                                    SrcOptionValueSelector = Mid(SrcOptionName, Pos + 1)
                                                                    SrcOptionName = Mid(SrcOptionName, 1, Pos - 1)
                                                                    SrcOptionSelector = ""
                                                                    Pos = genericController.vbInstr(1, SrcOptionValueSelector, "[")
                                                                    If Pos <> 0 Then
                                                                        SrcOptionSelector = Mid(SrcOptionValueSelector, Pos)
                                                                    End If
                                                                End If
                                                                ' all Src and Instance vars are already encoded correctly
                                                                If SrcOptionName <> "" Then
                                                                    ' since AddonOptionString is encoded, InstanceOptionValue will be also
                                                                    InstanceOptionValue = htmlController.getAddonOptionStringValue(SrcOptionName, addonOptionString)
                                                                    'InstanceOptionValue = cpcore.csv_GetAddonOption(SrcOptionName, AddonOptionString)
                                                                    ResultOptionSelector = getAddonSelector(SrcOptionName, genericController.encodeNvaArgument(InstanceOptionValue), SrcOptionSelector)
                                                                    'ResultOptionSelector = csv_GetAddonSelector(SrcOptionName, InstanceOptionValue, SrcOptionValueSelector)
                                                                    ResultOptionListHTMLEncoded = ResultOptionListHTMLEncoded & "&" & ResultOptionSelector
                                                                End If
                                                            Next
                                                            If ResultOptionListHTMLEncoded <> "" Then
                                                                ResultOptionListHTMLEncoded = Mid(ResultOptionListHTMLEncoded, 2)
                                                            End If
                                                        End If
                                                        ACNameCaption = genericController.vbReplace(ACName, """", "")
                                                        ACNameCaption = encodeHTML(ACNameCaption)
                                                        IDControlString = "AC," & ACType & "," & NotUsedID & "," & genericController.encodeNvaArgument(ACName) & "," & ResultOptionListHTMLEncoded & "," & ACGuid
                                                        Copy = genericController.GetAddonIconImg(AdminURL, IconWidth, IconHeight, IconSprites, AddonIsInline, IDControlString, IconFilename, serverFilePath, IconAlt, IconTitle, ACInstanceID, 0)
                                                    ElseIf EncodeNonCachableTags Then
                                                        '
                                                        ' Add-on Experiment - move all processing to the Webclient
                                                        ' just pass the name and arguments back in th FPO
                                                        ' HTML encode and quote the name and AddonOptionString
                                                        '
                                                        Copy = "" _
                                                        & "" _
                                                        & "<!-- ADDON " _
                                                        & """" & ACName & """" _
                                                        & ",""" & AddonOptionStringHTMLEncoded & """" _
                                                        & ",""" & ACInstanceID & """" _
                                                        & ",""" & ACGuid & """" _
                                                        & " -->" _
                                                        & ""
                                                    End If
                                                    '
                                                End If
                                            Case ACTypeImage
                                                '
                                                ' ----- Image Tag, substitute image placeholder with the link from the REsource Library Record
                                                '
                                                If EncodeImages Then
                                                    Copy = ""
                                                    ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"))
                                                    ACAttrWidth = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "WIDTH"))
                                                    ACAttrHeight = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HEIGHT"))
                                                    ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"))
                                                    ACAttrBorder = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "BORDER"))
                                                    ACAttrLoop = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "LOOP"))
                                                    ACAttrVSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "VSPACE"))
                                                    ACAttrHSpace = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "HSPACE"))
                                                    ACAttrAlign = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALIGN"))
                                                    '
                                                    Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID)
                                                    If (file IsNot Nothing) Then
                                                        Filename = file.Filename
                                                        Filename = genericController.vbReplace(Filename, "\", "/")
                                                        Filename = genericController.EncodeURL(Filename)
                                                        Copy = Copy & "<img ID=""AC,IMAGE,," & ACAttrRecordID & """ src=""" & genericController.getCdnFileLink(cpCore, Filename) & """"
                                                        '
                                                        If ACAttrWidth = 0 Then
                                                            ACAttrWidth = file.pxWidth
                                                        End If
                                                        If ACAttrWidth <> 0 Then
                                                            Copy = Copy & " width=""" & ACAttrWidth & """"
                                                        End If
                                                        '
                                                        If ACAttrHeight = 0 Then
                                                            ACAttrHeight = file.pxHeight
                                                        End If
                                                        If ACAttrHeight <> 0 Then
                                                            Copy = Copy & " height=""" & ACAttrHeight & """"
                                                        End If
                                                        '
                                                        If ACAttrVSpace <> 0 Then
                                                            Copy = Copy & " vspace=""" & ACAttrVSpace & """"
                                                        End If
                                                        '
                                                        If ACAttrHSpace <> 0 Then
                                                            Copy = Copy & " hspace=""" & ACAttrHSpace & """"
                                                        End If
                                                        '
                                                        If ACAttrAlt <> "" Then
                                                            Copy = Copy & " alt=""" & ACAttrAlt & """"
                                                        End If
                                                        '
                                                        If ACAttrAlign <> "" Then
                                                            Copy = Copy & " align=""" & ACAttrAlign & """"
                                                        End If
                                                        '
                                                        ' no, 0 is an important value
                                                        'If ACAttrBorder <> 0 Then
                                                        Copy = Copy & " border=""" & ACAttrBorder & """"
                                                        '    End If
                                                        '
                                                        If ACAttrLoop <> 0 Then
                                                            Copy = Copy & " loop=""" & ACAttrLoop & """"
                                                        End If
                                                        '
                                                        Dim attr As String
                                                        attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "STYLE"))
                                                        If attr <> "" Then
                                                            Copy = Copy & " style=""" & attr & """"
                                                        End If
                                                        '
                                                        attr = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "CLASS"))
                                                        If attr <> "" Then
                                                            Copy = Copy & " class=""" & attr & """"
                                                        End If
                                                        '
                                                        Copy = Copy & ">"
                                                    End If
                                                End If
                                            '
                                            '
                                            Case ACTypeDownload
                                                '
                                                ' ----- substitute and anchored download image for the AC-Download tag
                                                '
                                                ACAttrRecordID = genericController.EncodeInteger(KmaHTML.ElementAttribute(ElementPointer, "RECORDID"))
                                                ACAttrAlt = genericController.encodeText(KmaHTML.ElementAttribute(ElementPointer, "ALT"))
                                                '
                                                If EncodeEditIcons Then
                                                    '
                                                    ' Encoding the edit icons for the active editor form
                                                    '
                                                    IconIDControlString = "AC," & ACTypeDownload & ",," & ACAttrRecordID
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 16, 16, 0, True, IconIDControlString, "/ccLib/images/IconDownload3.gif", serverFilePath, "Download Icon with a link to a resource", "Renders as [Download Icon with a link to a resource]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ alt=""Renders as a download icon"" id=""AC," & ACTypeDownload & ",," & ACAttrRecordID & """ src=""/ccLib/images/IconDownload3.GIF"">"
                                                ElseIf EncodeImages Then
                                                    '
                                                    Dim file As Models.Entity.libraryFilesModel = Models.Entity.libraryFilesModel.create(cpCore, ACAttrRecordID)
                                                    If (file IsNot Nothing) Then
                                                        If ACAttrAlt = "" Then
                                                            ACAttrAlt = genericController.encodeText(file.AltText)
                                                        End If
                                                        Copy = "<a href=""" & ProtocolHostLink & requestAppRootPath & cpCore.siteProperties.serverPageDefault & "?" & RequestNameDownloadID & "=" & ACAttrRecordID & """ target=""_blank""><img src=""" & ProtocolHostLink & "/ccLib/images/IconDownload3.gif"" width=""16"" height=""16"" border=""0"" alt=""" & ACAttrAlt & """></a>"
                                                    End If
                                                End If
                                            Case ACTypeTemplateContent
                                                '
                                                ' ----- Create Template Content
                                                '
                                                'ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                AddonOptionStringHTMLEncoded = ""
                                                addonOptionString = ""
                                                NotUsedID = 0
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 64, 0, False, IconIDControlString, "/ccLib/images/ACTemplateContentIcon.gif", serverFilePath, "Template Page Content", "Renders as the Template Page Content", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Template Page Content"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateContentIcon.gif"" WIDTH=52 HEIGHT=64>"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    ' Add in the Content
                                                    '
                                                    Copy = fpoContentBox
                                                    'Copy = TemplateBodyContent
                                                    'Copy = "{{" & ACTypeTemplateContent & "}}"
                                                End If
                                            Case ACTypeTemplateText
                                                '
                                                ' ----- Create Template Content
                                                '
                                                'ACName = genericController.vbUCase(KmaHTML.ElementAttribute(ElementPointer, "NAME"))
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                NotUsedID = 0
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACTemplateTextIcon.gif", serverFilePath, "Template Text", "Renders as a Template Text Box", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as Template Text"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACTemplateTextIcon.gif"" WIDTH=52 HEIGHT=52>"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    ' Add in the Content Page
                                                    '
                                                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                                    'test - encoding changed
                                                    NewName = htmlController.getAddonOptionStringValue("new", addonOptionString)
                                                    'NewName =  genericController.DecodeResponseVariable(getSimpleNameValue("new", AddonOptionString, "", "&"))
                                                    TextName = htmlController.getAddonOptionStringValue("name", addonOptionString)
                                                    'TextName = getSimpleNameValue("name", AddonOptionString)
                                                    If TextName = "" Then
                                                        TextName = "Default"
                                                    End If
                                                    Copy = "{{" & ACTypeTemplateText & "?name=" & genericController.encodeNvaArgument(TextName) & "&new=" & genericController.encodeNvaArgument(NewName) & "}}"
                                                    ' ***** can not add it here, if a web hit, it must be encoded from the web client for aggr objects
                                                    'Copy = csv_GetContentCopy(TextName, "Copy Content", "", personalizationpeopleId)
                                                End If
                                            'Case ACTypeDynamicMenu
                                            '    '
                                            '    ' ----- Create Template Menu
                                            '    '
                                            '    'ACName = KmaHTML.ElementAttribute(ElementPointer, "NAME")
                                            '    AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                            '    addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                            '    '
                                            '    ' test for illegal characters (temporary patch to get around not addonencoding during the addon replacement
                                            '    '
                                            '    Pos = genericController.vbInstr(1, addonOptionString, "menunew=", vbTextCompare)
                                            '    If Pos > 0 Then
                                            '        NewName = Mid(addonOptionString, Pos + 8)
                                            '        Dim IsOK As Boolean
                                            '        IsOK = (NewName = genericController.encodeNvaArgument(NewName))
                                            '        If Not IsOK Then
                                            '            addonOptionString = Left(addonOptionString, Pos - 1) & "MenuNew=" & genericController.encodeNvaArgument(NewName)
                                            '        End If
                                            '    End If
                                            '    NotUsedID = 0
                                            '    If EncodeEditIcons Then
                                            '        If genericController.vbInstr(1, AddonOptionStringHTMLEncoded, "menu=", vbTextCompare) <> 0 Then
                                            '            '
                                            '            ' Dynamic Menu
                                            '            '
                                            '            '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                                            '            ' test - encoding changed
                                            '            TextName = cpCore.csv_GetAddonOptionStringValue("menu", addonOptionString)
                                            '            'TextName = cpcore.csv_GetAddonOption("Menu", AddonOptionString)
                                            '            '
                                            '            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & cpCore.csv_GetDynamicMenuACSelect() & "]&NewMenu="
                                            '            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                            '            Copy = IconImg
                                            '            '
                                            '            'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & ",Menu=" & TextName & "[" & csv_GetDynamicMenuACSelect & "]&NewMenu="" src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                            '        Else
                                            '            '
                                            '            ' Old Dynamic Menu - values are stored in the icon
                                            '            '
                                            '            IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                            '            IconImg = genericController.GetAddonIconImg(AdminURL, 52, 52, 0, False, IconIDControlString, "/ccLib/images/ACDynamicMenuIcon.gif", serverFilePath, "Dynamic Menu", "Renders as a Dynamic Menu", ACInstanceID, 0)
                                            '            Copy = IconImg
                                            '            '
                                            '            'Copy = "<img onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as a Dynamic Menu"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACDynamicMenuIcon.gif"" WIDTH=52 HEIGHT=52>"
                                            '        End If
                                            '    ElseIf EncodeNonCachableTags Then
                                            '        '
                                            '        ' Add in the Content Pag
                                            '        '
                                            '        Copy = "{{" & ACTypeDynamicMenu & "?" & addonOptionString & "}}"
                                            '    End If
                                            Case ACTypeWatchList
                                                '
                                                ' ----- Formatting Tag
                                                '
                                                '
                                                ' Content Watch replacement
                                                '   served by the web client because the
                                                '
                                                'UCaseACName = genericController.vbUCase(Trim(KmaHTML.ElementAttribute(ElementPointer, "NAME")))
                                                'ACName = encodeInitialCaps(UCaseACName)
                                                AddonOptionStringHTMLEncoded = KmaHTML.ElementAttribute(ElementPointer, "QUERYSTRING")
                                                addonOptionString = genericController.decodeHtml(AddonOptionStringHTMLEncoded)
                                                If EncodeEditIcons Then
                                                    '
                                                    IconIDControlString = "AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded
                                                    IconImg = genericController.GetAddonIconImg(AdminURL, 109, 10, 0, True, IconIDControlString, "/ccLib/images/ACWatchList.gif", serverFilePath, "Watch List", "Renders as the Watch List [" & ACName & "]", ACInstanceID, 0)
                                                    Copy = IconImg
                                                    '
                                                    'Copy = "<img ACInstanceID=""" & ACInstanceID & """ onDblClick=""window.parent.OpenAddonPropertyWindow(this);"" alt=""Add-on"" title=""Rendered as the Watch List [" & ACName & "]"" id=""AC," & ACType & "," & NotUsedID & "," & ACName & "," & AddonOptionStringHTMLEncoded & """ src=""/ccLib/images/ACWatchList.GIF"">"
                                                ElseIf EncodeNonCachableTags Then
                                                    '
                                                    Copy = "{{" & ACTypeWatchList & "?" & addonOptionString & "}}"
                                                End If
                                        End Select
                                End Select
                            End If
                            '
                            ' ----- Output the results
                            '
                            Stream.Add(Copy)
                            ElementPointer = ElementPointer + 1
                        Loop
                    End If
                    workingContent = Stream.Text
                    '
                    ' Add Contensive User Form if needed
                    '
                    If FormCount = 0 And FormInputCount > 0 Then
                    End If
                    workingContent = ReplaceInstructions & workingContent
                    If CSPeopleSet Then
                        Call cpCore.db.csClose(CSPeople)
                    End If
                    If CSOrganizationSet Then
                        Call cpCore.db.csClose(CSOrganization)
                    End If
                    If CSVisitorSet Then
                        Call cpCore.db.csClose(CSVisitor)
                    End If
                    If CSVisitSet Then
                        Call cpCore.db.csClose(CSVisit)
                    End If
                    KmaHTML = Nothing
                End If
                result = workingContent
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
	
	
	
    End Class
End Namespace
