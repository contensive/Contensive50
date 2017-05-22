
Option Explicit On
Option Strict On

Imports System.Reflection
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers.genericController

Imports System.Xml

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
        ' ----- objects constructed that must be disposed
        '
        Private localObject As Object
        '
        ' ----- constants
        '
        Private Const localConstant As Integer = 100
        '
        ' ----- shared globals
        '
        '
        ' ----- private globals
        '
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '===================================================================================================
        ''' <summary>
        ''' addon install
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property addonInstall As addonInstallClass
            Get
                If (_addonInstall Is Nothing) Then
                    _addonInstall = New addonInstallClass(cpCore)
                End If
                Return _addonInstall
            End Get
        End Property
        Private _addonInstall As addonInstallClass
        '
        '=============================================================================================================
        '   Get Addon Content - internal (to support include add-ons)
        '
        '   Argument field in addons is encode as "AddonOptionConstructor"
        '   the input argument OptionString to executeAddon is encoded as "OptionString"
        '       delmited with "&" and all elements encoded with encodeNvaArgument
        '   the optionstring passed to addons is like OptionString encoding, except it is crlf delimited
        '       only ever decode with cpcore.main_GetAddonOption( name, string )
        '
        '
        '   OptionString
        '       This is a string, similar to a QueryString.
        '           It is used (htmlencoded) in AC Edit Icons to hold instance properties in the 5th comma delimited position of the AC Tag's ID attribute
        '           It is (htmlencoded) in AC Tags as the QueryString value "... Querystring=OptionString ..."
        '           It is used internally to carry properties from the csv to the websclient for:
        '               1) AC tags that have to be executed at the webclient but are interpreted in the cmc.csv_
        '                   not htmlencoded
        '                   like {{ACTextBox?name=&new name=text name}}
        '               2) Addons found in csv during csv_EncodeContent that must be executed in the webclient.
        '                   htmlencoded
        '                   <!-- Addon "acname","htmlencodedOptionString","instanceid" -->
        '       This is called "AddonOption" format, or AddonOptionEncoding
        '       It usually contains the instance prefernces for the Add-on placed in the content
        '       It's basic form is:
        '           name=value[selector]&name=value[selector]
        '           ( why are the selector's necessary, since the add-on will be looked up before arguments are prepared for any editor )
        '       To get a value from the string:
        '           value = cpcore.csv_GetAddonOption( OptionString )
        '           csv_DecodeAddonOptionArgument(RemoveSelector(GetArgument(name,string,default,"&")))
        '       To add a value to the string
        '           s = s & "&" & encodeNvaArgument(name) & "=" & encodeNvaArgument(value)
        '       "&" delimits the name=value pairs
        '       encodeNvaArgument() is used to encode each name and value
        '       csv_DecodeAddonOptionArgument() is used to decode each name and value
        '       Previously, I wrote:
        '           this is the name=value&name=value set of arguments that come from the AC tag in the content
        '           it includes all name=value pairs set the last time the page was edited.
        '           it may contain names that are not in the add-on, these may be valid
        '
        '   AddonOptionConstructor
        '       This is the format saved in the addon record in the argument field.
        '       This is as crlf delimited list of name=default[selector] that is stored in the Add-on record and controls the building of the instance selectors
        '       It's basic form is:
        '           name=DefaultValue[selector]descriptor
        '           name=DefaultValue[option:integer|option:integer]descriptor
        '           name=DefaultValue[option:integer|option:integer]descriptor
        '           name=DefaultValue[list(contentname)]descriptor
        '       for example:
        '           link=http://www.contensive.com/index.asp?i=1\&u=2
        '
        '       This is called "AddonConstructor" format, or AddonConstructorEncoding
        '           ConstructorEncoding is similiar to how javascript encodes strings
        '           This standard is used because it is visible to the end user
        '           name can not include:
        '               '\', instead use '\\'
        '               '=', instead use '\='
        '           DefaultValue can not include:
        '               '\', instead use '\\'
        '               '[', instead use '\['
        '               ']', instead use '\]'
        '               ':', instead use '\:'
        '               newline, instead use '\n'
        '           selector can not include:
        '               '['
        '               ']'
        '               '|'
        '
        '       crlf delimits arguments
        '       EncodeAddonConstructorArgument() is used to encode each name and value
        '       DecodeAddonConstructorArgument() is used to decode each name and value
        '
        '   AddonOptionNameValueList
        '       This is as crlf delimited list of name=value that is sent to the Add-ons in the OptionString
        '
        '   AddonOptionExpandedConstructor
        '       passed to the editors to create instance selectors
        '       Similar to the AddonOptionConstructor, but all the list and listid functions are expanded
        '
        '
        '
        '   Wrappers
        '       Instance WrapperID is forced into the Instance properies, choose from:
        '           none = -1
        '           default = 0, use the DefaultWrapperId passed into the call
        '           or id of a wrapper record
        '
        '   Context
        '       These values represent the situation around the call for execute cpcore.addon. This determines the
        '       type of data returned, and other actions taken. For instance, a ContextPage is used when the add-on
        '       results will be put on a page for output. In this case, javascript in the add-on will be put into
        '       the current document head.
        '       * these are in CPUtilsBaseClass.addonContext and are duplicated in the contentserver object also
        '       ContextPage = 1
        '       ContextAdmin = 2
        '       ContextTemplate = 3
        '       ContextEmail = 4
        '       ContextRemoteMethod = 5
        '       ContextOnNewVisit = 6
        '       ContextOnPageEnd = 7
        '       ContextOnPageStart = 8
        '       ContextEditor = 9
        '       ContextHelpUser = 10
        '       ContextHelpAdmin = 11
        '       ContextHelpDeveloper = 12
        '       ContextOnContentChange = 13
        '       ContextFilter = 14
        '       ContextSimple = 15
        '       ContextOnBodyStart = 16
        '       ContextOnBodyEnd = 17
        '
        '====================================================================================================
        ''' <summary>
        ''' execute addon
        ''' </summary>
        ''' <param name="addonId">The Id of the addon to execute.</param>
        ''' <param name="properties">properties are nameValue pairs consumable by the addon during execution. These properties are added to cpcore.docproperties and made available. Originally this argument was for the nameValues modified in the page instance where the addon was placed.</param>
        ''' <param name="context">member of CPUtilsBaseClass.addonContext</param>
        ''' <returns></returns>
        Public Function execute(ByVal addonId As Integer, properties As Dictionary(Of String, String), context As CPUtilsBaseClass.addonContext) As Object
            Try
                Dim optionString As String = ""
                Dim return_StatusOk As Boolean
                For Each kvp As KeyValuePair(Of String, String) In properties
                    If Not String.IsNullOrEmpty(kvp.Key) Then
                        optionString &= "&" & EncodeRequestVariable(kvp.Key) & "=" & EncodeRequestVariable(kvp.Value)
                    End If
                Next
                If Not String.IsNullOrEmpty(optionString) Then
                    optionString = optionString.Substring(1)
                End If
                Return execute(addonId, "", optionString, context, "", 0, "", "", False, 0, "", return_StatusOk, Nothing, "", Nothing, "", 0, False)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex)
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="addonId"></param>
        ''' <param name="AddonNameOrGuid"></param>
        ''' <param name="OptionString"></param>
        ''' <param name="Context"></param>
        ''' <param name="HostContentName"></param>
        ''' <param name="HostRecordID"></param>
        ''' <param name="HostFieldName"></param>
        ''' <param name="ACInstanceID"></param>
        ''' <param name="IsIncludeAddon"></param>
        ''' <param name="DefaultWrapperID"></param>
        ''' <param name="ignore_TemplateCaseOnly_PageContent"></param>
        ''' <param name="return_StatusOK"></param>
        ''' <param name="nothingObject"></param>
        ''' <param name="ignore_addonCallingItselfIdList"></param>
        ''' <param name="nothingObject2"></param>
        ''' <param name="ignore_AddonsRunOnThisPageIdList"></param>
        ''' <param name="personalizationPeopleId"></param>
        ''' <param name="personalizationIsAuthenticated"></param>
        ''' <returns></returns>
        Public Function execute(ByVal addonId As Integer, ByVal AddonNameOrGuid As String, ByVal OptionString As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal HostContentName As String, ByVal HostRecordID As Integer, ByVal HostFieldName As String, ByVal ACInstanceID As String, ByVal IsIncludeAddon As Boolean, ByVal DefaultWrapperID As Integer, ByVal ignore_TemplateCaseOnly_PageContent As String, ByRef return_StatusOK As Boolean, ByVal nothingObject As Object, ByVal ignore_addonCallingItselfIdList As String, ByVal nothingObject2 As Object, ByVal ignore_AddonsRunOnThisPageIdList As String, ByVal personalizationPeopleId As Integer, ByVal personalizationIsAuthenticated As Boolean) As String
            Dim returnVal As String
            Try
                Dim AddonName As String = "unknown"
                '
                Dim styleId As Integer
                Dim inlineScriptContent As String
                Dim inlineScript As String
                Dim blockJavascriptAndCss As Boolean
                Dim JSOnLoad As String
                Dim JSBodyEnd As String
                Dim JSFilename As String
                Dim DefaultStylesFilename As String
                Dim CustomStylesFilename As String
                Dim TestString As String
                Dim addon_IncludedAddonIDList As String
                Dim includedAddonId As Integer
                Dim includedAddonIds() As String
                Dim ReplaceSource As String
                Dim ReplaceValue As String
                Dim AddonStylesEditIcon As String
                Dim SiteStylesEditIcon As String
                Dim DialogList As String
                Dim ToolBar As String
                Dim ScriptingTimeout As Integer
                Dim ScriptCallbackContent As String
                Dim errorMessageForAdmin As String
                Dim CollectionGuid As String
                Dim DotNetClassFullName As String
                Dim CodeFilename As String
                Dim ScriptingEntryPoint As String
                Dim scriptinglanguageid As Integer
                Dim ScriptingLanguage As String
                Dim ScriptingCode As String
                Dim AddonStatusOK As Boolean
                Dim EditWrapperHTMLID As String
                Dim AddonNameOrGuid_Local As String
                Dim AddonGuid As String
                Dim QS As String
                Dim QSSplit() As String
                Dim NVPair As String
                Dim NVSplit() As String
                Dim FrameID As String
                Dim AsAjaxID As String
                Dim OptionNames() As String
                Dim OptionValues() As String
                Dim OptionsForCPVars() As NameValuePrivateType
                Dim OptionsForCPVars_Cnt As Integer
                Dim RemoteAssetContent As String
                Dim kmaHTTP As coreHttpRequestClass
                Dim WorkingLink As String
                Dim FormContent As String
                Dim ExitAddonWithBlankResponse As Boolean
                Dim RemoteAssetLink As String
                Dim AsAjax As Boolean
                Dim InFrame As Boolean
                Dim IncludeEditWrapper As Boolean
                Dim AddedByName As String
                Dim AddonCommentName As String
                Dim IncludeContent As String
                Dim GroupIDList As String
                Dim AddonOptionConstructor As String
                Dim AddonOptionExpandedConstructor As String
                Dim OptionString_ForObjectCall As String
                Dim Pos As Integer
                Dim Ptr As Integer
                Dim SQL As String
                Dim CSRules As Integer
                Dim HelpIcon As String
                Dim InstanceSettingsEditIcon As String
                Dim OptionPair() As String
                Dim OptionPtr As Integer
                Dim OptionCnt As Integer
                Dim Link As String
                Dim ProgramID As String
                Dim Options() As String
                Dim OptionName As String
                Dim OptionValue As String
                Dim HTMLContent As String
                Dim TextContent As String
                Dim ObjectContent As String
                Dim AssemblyContent As String
                Dim ScriptContent As String
                Dim helpCopy As String
                Dim helpLink As String
                Dim PageTitle As String
                Dim MetaDescription As String
                Dim MetaKeywordList As String
                Dim OtherHeadTags As String
                Dim AddonEditIcon As String
                Dim FoundAddon As Boolean
                Dim FormXML As String
                Dim WrapperID As Integer
                Dim ContainerCssID As String
                Dim ContainerCssClass As String
                Dim IsInline As Boolean
                Dim WorkingOptionString As String
                Dim HTMLViewerEditIcon As String
                Dim AddonBlockEditTools As Boolean
                Dim ReplaceCnt As Integer
                Dim ReplaceNames() As String
                Dim ReplaceValues() As String
                Dim isMainOk As Boolean
                Dim StartTickCount As Integer
                Dim addonCachePtr As Integer
                Dim addonCollectionId As Integer
                '
                ' ----- OptionString and FilterInput values before this call are saved on the stack
                '
                Dim PushOptionString As String
                PushOptionString = OptionString
                '
                isMainOk = True
                '
                ' ----- Debug timer
                '
                If isMainOk Then
                    StartTickCount = GetTickCount
                End If
                return_StatusOK = True
                WrapperID = DefaultWrapperID
                If (personalizationPeopleId = 0) And isMainOk Then
                    '
                    ' just in case - during transition from cpCoreClass to csv, in case a call is missing.
                    '
                    personalizationPeopleId = cpcore.authContext.authContextUser.id
                    personalizationIsAuthenticated = cpCore.authContext.isAuthenticated()
                End If
                '
                ' ----- Set WorkingOptionString to what came in from the tag of the object
                '       This may be replaced later if the tag is empty, and the actual add-on arguments have default values
                '
                WorkingOptionString = OptionString
                '
                ' ----- Lookup the addon
                '
                If addonId <> 0 Then
                    addonCachePtr = cpCore.addonCache.getPtr(CStr(addonId))
                Else
                    addonCachePtr = cpCore.addonCache.getPtr(AddonNameOrGuid)
                End If
                If (addonCachePtr < 0) Then
                    FoundAddon = False
                Else
                    Dim addonCacheKey As String = addonCachePtr.ToString
                    FoundAddon = True
                    ProgramID = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ObjectProgramID)
                    AddonName = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_name)
                    addonId = genericController.EncodeInteger(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_Id)
                    addonCollectionId = genericController.EncodeInteger(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_collectionid)
                    AddonGuid = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ccguid)
                    If AddonGuid <> "" Then
                        AddonNameOrGuid_Local = AddonGuid
                    Else
                        AddonNameOrGuid_Local = AddonName
                    End If
                    HTMLContent = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_Copy)
                    Link = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_Link)
                    DotNetClassFullName = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_DotNetClass)
                    AddonOptionConstructor = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ArgumentList)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                    AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                    '
                    AddonBlockEditTools = False
                    TextContent = ""
                    FormXML = ""
                    TextContent = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_CopyText)
                    IsInline = genericController.EncodeBoolean(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_IsInline)
                    '
                    ' Support BlockDefaultStyles and CustomStylesFilename
                    '
                    If Not cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_BlockDefaultStyles Then
                        '
                        ' Add default styles
                        '
                        DefaultStylesFilename = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_StylesFilename)
                    End If
                    '
                    ' Add custom styles
                    '
                    CustomStylesFilename = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_CustomStylesFilename)
                    FormXML = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_formxml)
                    RemoteAssetLink = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_RemoteAssetLink)
                    AsAjax = genericController.EncodeBoolean(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_AsAjax)
                    InFrame = genericController.EncodeBoolean(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_InFrame)
                    ScriptingEntryPoint = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ScriptingEntryPoint)
                    scriptinglanguageid = genericController.EncodeInteger(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ScriptingLanguageID)
                    '
                    ' Get Language
                    '
                    ScriptingLanguage = ""
                    If scriptinglanguageid <> 0 Then
                        ScriptingLanguage = cpCore.GetRecordName("Scripting Languages", scriptinglanguageid)
                    End If
                    If ScriptingLanguage = "" Then
                        ScriptingLanguage = "VBScript"
                    End If
                    ScriptingCode = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ScriptingCode)
                    AddonBlockEditTools = genericController.EncodeBoolean(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_BlockEditTools)
                    ScriptingTimeout = genericController.EncodeInteger(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_ScriptingTimeout)
                    inlineScript = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_inlineScript)
                    helpCopy = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_help)
                    helpLink = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_helpLink)
                    JSOnLoad = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_JavaScriptOnLoad)
                    JSBodyEnd = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_JavaScriptBodyEnd)
                    PageTitle = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_PageTitle)
                    MetaDescription = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_MetaDescription)
                    MetaKeywordList = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_MetaKeywordList)
                    OtherHeadTags = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_OtherHeadTags)
                    JSFilename = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonCacheKey).addonCache_JSFilename)
                    If JSFilename <> "" Then
                        JSFilename = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                    End If
                End If
                If Not String.IsNullOrEmpty(ProgramID) Then
                    '
                    ' addons with activeX components are deprecated
                    '
                    Throw New ApplicationException("This add-on [#" & addonId & ", " & AddonName & "] is no longer supported because it contains an active-X component.")
                Else
                    '
                    '----------------------------------------------------------------------------------------------------
                    ' add shared styles
                    '----------------------------------------------------------------------------------------------------
                    '
                    Dim addonIdKey As String
                    addonIdKey = addonId.ToString
                    Ptr = cpCore.cache_addonStyleRules.getFirstPtr(addonIdKey)
                    Do While Ptr >= 0
                        styleId = genericController.EncodeInteger(cpCore.cache_addonStyleRules.getValue(Ptr))
                        Call cpCore.htmlDoc.main_AddSharedStyleID2(styleId, AddonName)
                        Ptr = cpCore.cache_addonStyleRules.getNextPtr()
                    Loop
                    '
                    '----------------------------------------------------------------------------------------------------
                    ' Add ScriptingCode
                    '----------------------------------------------------------------------------------------------------
                    '
                    If ScriptingEntryPoint <> "" Then
                        '
                        ' Get Modules
                        '
                        SQL = "select c.code from ccScriptingModules c left join ccAddonScriptingModuleRules r on r.ScriptingModuleID=c.id where r.Addonid=" & addonId & " order by c.sortorder"
                        CSRules = cpCore.db.cs_openCsSql_rev("default", SQL)
                        Do While cpCore.db.cs_ok(CSRules)
                            CodeFilename = cpCore.db.cs_get(CSRules, "code")
                            If CodeFilename <> "" Then
                                ScriptingCode = ScriptingCode & vbCrLf & cpCore.cdnFiles.readFile(CodeFilename)
                            End If
                            Call cpCore.db.cs_goNext(CSRules)
                        Loop
                        Call cpCore.db.cs_Close(CSRules)
                    End If
                    '
                    '----------------------------------------------------------------------------------------------------
                    ' Add the common addon options to the AddonOptionConstructor
                    '----------------------------------------------------------------------------------------------------
                    '
                    If AddonOptionConstructor <> "" Then
                        AddonOptionConstructor = AddonOptionConstructor & vbCrLf
                    End If
                    '
                    ' temporary fix for Content Box not handling ajax or inframe
                    '
                    If genericController.vbLCase(AddonGuid) = genericController.vbLCase(ContentBoxGuid) Then
                        AsAjax = False
                        InFrame = False
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_BlockNoAjax
                    ElseIf IsInline Then
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                    Else
                        AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
                    End If
                    '
                    If Not FoundAddon Then
                        '
                        '-----------------------------------------------------------------------------------------------------
                        ' Build-in Add-ons
                        '-----------------------------------------------------------------------------------------------------
                        '
                        If genericController.vbLCase(AddonName) = "block text" Then
                            AddonNameOrGuid_Local = AddonName
                            FoundAddon = True
                            'IsProcess = False
                            addonId = 0
                            Link = ""
                            ProgramID = ""
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockText
                            TextContent = ""
                            DefaultStylesFilename = ""
                            CustomStylesFilename = ""
                            helpCopy = ""
                            helpLink = ""
                            JSOnLoad = ""
                            'JSInHead = ""
                            JSFilename = ""
                            JSBodyEnd = ""
                            PageTitle = ""
                            MetaDescription = ""
                            MetaKeywordList = ""
                            OtherHeadTags = ""
                            AddonEditIcon = ""
                            IsInline = True
                            GroupIDList = cpCore.csv_GetAddonOption("AllowGroups", WorkingOptionString)
                            GroupIDList = Trim(GroupIDList)
                            ' not webonly anymore
                            If Not cpCore.authContext.isMemberOfGroupIdList(cpCore, personalizationPeopleId, personalizationIsAuthenticated, GroupIDList) Then
                                HTMLContent = BlockTextStartMarker
                            End If
                            'If isMainOk Then
                            '    '
                            '    ' web-only
                            '    '
                            '    If Not cpcore.main_IsAdmin() Then
                            '        If Not csv_IsGroupIDListMember(personalizationPeopleid, personalizationIsAuthenticated, GroupIDList) Then
                            '            HTMLContent = BlockTextStartMarker
                            '        End If
                            '    End If
                            ' End If
                            '
                        ElseIf genericController.vbLCase(AddonName) = "block text end" Then
                            AddonNameOrGuid_Local = AddonName
                            FoundAddon = True
                            'IsProcess = False
                            addonId = 0
                            HTMLContent = BlockTextEndMarker
                            Link = ""
                            ProgramID = ""
                            AddonOptionConstructor = AddonOptionConstructor_ForBlockTextEnd
                            TextContent = ""
                            DefaultStylesFilename = ""
                            CustomStylesFilename = ""
                            helpCopy = ""
                            helpLink = ""
                            JSOnLoad = ""
                            'JSInHead = ""
                            JSFilename = ""
                            JSBodyEnd = ""
                            PageTitle = ""
                            MetaDescription = ""
                            MetaKeywordList = ""
                            OtherHeadTags = ""
                            AddonEditIcon = ""
                            IsInline = True
                        End If
                    End If
                    If Not FoundAddon Then
                        '
                        '-----------------------------------------------------------------------------------------------------
                        ' The add-on was not found for real
                        '-----------------------------------------------------------------------------------------------------
                        '
                        return_StatusOK = False
                        '
                        ' web-only
                        '
                        If (Context = CPUtilsBaseClass.addonContext.ContextEmail) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (Context = CPUtilsBaseClass.addonContext.ContextSimple) Then
                            '
                            ' Block all output even on error
                            '
                        ElseIf cpcore.authContext.isAuthenticatedAdmin(cpcore) Or cpcore.authContext.isAuthenticatedContentManager(cpcore, "Page Content") Then
                            '
                            ' Provide hint to administrators
                            '
                            If AddonName = "" And addonId <> 0 Then
                                AddonName = "Addon #" & addonId
                            End If
                            If Context = CPUtilsBaseClass.addonContext.ContextAdmin Then
                                returnVal = "The Add-on '" & AddonName & "' could not be found. It may have been deleted or marked inactive. If you are receiving this message after clicking an Add-on from the Navigator, their may be a problem with this Add-on. If you are receiving this message from the main admin page, your Dashboard Add-on may be set incorrectly. Use the Admin tab under Preferences to select the Dashboard, or <a href=""?" & RequestNameDashboardReset & "=" & cpCore.authContext.visit.Id & """>click here</a> to automatically reset the dashboard."
                            Else
                                returnVal = "The Add-on '" & AddonName & "' could not be found. It may have been deleted or marked inactive. Please use the Add-on Manager to replace it, or edit this page and remove it."
                            End If
                            returnVal = cpCore.htmlDoc.html_GetAdminHintWrapper(returnVal)
                        End If
                        If (addonId > 0) Then
                            Throw New ApplicationException("The Add-on could not be found by id [" & addonId & "] or name/guid [" & AddonNameOrGuid & "]")
                        Else
                            Throw New ApplicationException("The Add-on could not be found by name/guid [" & AddonNameOrGuid & "]")
                        End If
                    End If
                    '
                    '-----------------------------------------------------------------
                    ' Process the Add-on
                    '-----------------------------------------------------------------
                    '
                    If FoundAddon Then
                        '
                        'determine if it has already run once (if so, block javascript and styles)
                        '
                        If cpCore.addonsRunOnThisPageIdList.Contains(addonId) Then
                            blockJavascriptAndCss = True
                        Else
                            cpCore.addonsRunOnThisPageIdList.Add(addonId)
                        End If
                        'blockJavascriptAndCss = (InStr(1, "," & csv_addon_execute_AddonsRunOnThisPageIdList & ",", "," & addonId & ",") <> 0)
                        'csv_addon_execute_AddonsRunOnThisPageIdList = csv_addon_execute_AddonsRunOnThisPageIdList & "," & addonId
                        '
                        '-----------------------------------------------------------------
                        ' Enable Edit Wrapper for Page Content and Dynamic Menu for edit mode
                        '-----------------------------------------------------------------
                        '
                        If isMainOk Then
                            IncludeEditWrapper =
                                (Not AddonBlockEditTools) _
                                And (Context <> CPUtilsBaseClass.addonContext.ContextEditor) _
                                And (Context <> CPUtilsBaseClass.addonContext.ContextEmail) _
                                And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) _
                                And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) _
                                And (Context <> CPUtilsBaseClass.addonContext.ContextSimple) _
                                And (Not IsIncludeAddon)
                            If IncludeEditWrapper Then
                                IncludeEditWrapper = IncludeEditWrapper _
                                    And (cpCore.visitProperty.getBoolean("AllowAdvancedEditor") _
                                    And ((Context = CPUtilsBaseClass.addonContext.ContextAdmin) Or cpCore.authContext.isEditing(cpCore, HostContentName)))
                                'IncludeEditWrapper = IncludeEditWrapper _
                                '    And ( _
                                '        ( _
                                '            (csv_VisitProperty_AllowAdvancedEditor And ((Context = ContextAdmin) Or IsEditing(HostContentName))) _
                                '        ) Or ( _
                                '            (csv_VisitProperty_AllowEditing And ((AddonGuid = ContentBoxGuid) Or (AddonGuid = DynamicMenuGuid) Or (AddonGuid = TextBoxGuid))) _
                                '        ) _
                                '    )
                            End If
                        End If
                        '
                        ' ----- Test if this Addon is already in use
                        '
                        If cpCore.addonsCurrentlyRunningIdList.Contains(addonId) Then
                            '
                            ' This addon is running, can not reenter
                            '
                            Call logController.log_appendLog(cpCore, "addon_execute, Addon [" & AddonName & "] was called by itself. This is not allowed. AddonID [" & addonId & "], AddonNameOrGuid [" & AddonNameOrGuid_Local & "]")
                        Else
                            cpCore.addonsCurrentlyRunningIdList.Add(addonId)
                            'csv_addon_execute_AddonsCurrentlyRunningIdList = csv_addon_execute_AddonsCurrentlyRunningIdList & "," & addonId
                            '
                            '-----------------------------------------------------------------------------------------------------
                            ' Preprocess arguments into OptionsForCPVars, and set generic instance values wrapperid and asajax
                            '-----------------------------------------------------------------------------------------------------
                            '
                            ' Setup InstanceOptions - if InstanceOptionString is empty, use the defaults from the Addon Arguments
                            '
                            OptionCnt = 0
                            If WorkingOptionString <> "" Then
                                If genericController.vbInstr(1, WorkingOptionString, vbCrLf) <> 0 Then
                                    '
                                    ' this should never be the case
                                    '
                                    Options = genericController.SplitCRLF(WorkingOptionString)
                                    OptionCnt = UBound(Options) + 1
                                Else
                                    '
                                    '
                                    '
                                    Options = Split(WorkingOptionString, "&")
                                    OptionCnt = UBound(Options) + 1
                                End If
                                OptionsForCPVars_Cnt = OptionCnt
                                ReDim OptionsForCPVars(OptionCnt - 1)
                                ReDim OptionNames(OptionCnt - 1)
                                ReDim OptionValues(OptionCnt - 1)
                                For OptionPtr = 0 To OptionCnt - 1
                                    With OptionsForCPVars(OptionPtr)
                                        .Name = Options(OptionPtr)
                                        If genericController.vbInstr(1, .Name, "=") <> 0 Then
                                            Dim nameLc As String
                                            OptionPair = Split(.Name, "=")
                                            .Name = Trim(OptionPair(0))
                                            .Value = OptionPair(1)
                                            '
                                            ' added this because when a row of apostrophes were added to an instance argument, they showed up here
                                            ' so it appears (though not documented very well) that the WorkingOptionString argument is really
                                            ' the Addon Encoded Instance OptionString
                                            ' So, as I parse it for use in the add-on, I need to unencode it
                                            '
                                            .Name = genericController.decodeNvaArgument(.Name)
                                            .Value = genericController.decodeNvaArgument(.Value)
                                            '
                                            '
                                            nameLc = .Name.ToLower()
                                            If nameLc = "wrapper" Then
                                                WrapperID = genericController.EncodeInteger(.Value)
                                                If WrapperID = 0 Then
                                                    WrapperID = DefaultWrapperID
                                                End If
                                            ElseIf nameLc = "as ajax" Then
                                                If genericController.EncodeBoolean(.Value) Then
                                                    AsAjax = True
                                                End If
                                            ElseIf nameLc = "css container id" Then
                                                ContainerCssID = .Value
                                            ElseIf nameLc = "css container class" Then
                                                ContainerCssClass = .Value
                                            End If
                                            OptionNames(OptionPtr) = .Name
                                            OptionValues(OptionPtr) = .Value
                                        End If
                                    End With
                                Next
                            End If
                            If AddonOptionConstructor <> "" Then
                                '        If WorkingOptionString = "" Then
                                'WorkingOptionString = AddonOptionConstructor
                                '
                                ' convert from AddonConstructor format (crlf delimited, constructorincoded) to AddonOption format without selector (& delimited, addonencoded)
                                '
                                AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                                AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                                AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                                Options = genericController.SplitCRLF(AddonOptionConstructor)
                                OptionCnt = UBound(Options) + 1
                                For OptionPtr = 0 To OptionCnt - 1
                                    OptionName = Options(OptionPtr)
                                    OptionValue = ""
                                    '
                                    OptionName = genericController.vbReplace(OptionName, "\=", vbCrLf)
                                    If genericController.vbInstr(1, OptionName, "=") <> 0 Then
                                        OptionPair = Split(OptionName, "=")
                                        OptionName = OptionPair(0)
                                        OptionPair(0) = ""
                                        OptionValue = Mid(Join(OptionPair, "="), 2)
                                    End If
                                    OptionName = genericController.vbReplace(OptionName, vbCrLf, "\=")
                                    OptionValue = genericController.vbReplace(OptionValue, vbCrLf, "\=")
                                    '
                                    Do While (Mid(OptionName, 1, 1) = vbTab) And Len(OptionName) > 1
                                        OptionName = Mid(OptionName, 2)
                                    Loop
                                    OptionName = Trim(OptionName)
                                    '
                                    ' split on [, throw out the right side
                                    OptionValue = genericController.vbReplace(OptionValue, "\[", vbCrLf)
                                    If genericController.vbInstr(1, OptionValue, "[") <> 0 Then
                                        OptionValue = Left(OptionValue, genericController.vbInstr(1, OptionValue, "[") - 1)
                                    End If
                                    OptionValue = genericController.vbReplace(OptionValue, vbCrLf, "\[")
                                    '
                                    ' Decode Constructor format
                                    '
                                    OptionName = DecodeAddonConstructorArgument(OptionName)
                                    OptionValue = DecodeAddonConstructorArgument(OptionValue)
                                    '
                                    ' check for duplicates
                                    '
                                    For Ptr = 0 To OptionsForCPVars_Cnt - 1
                                        If genericController.vbLCase(OptionName) = genericController.vbLCase(OptionsForCPVars(Ptr).Name) Then
                                            Exit For
                                        End If
                                    Next
                                    If Ptr = OptionsForCPVars_Cnt Then
                                        '
                                        ' not found, add it to option pairs
                                        '
                                        ReDim Preserve OptionsForCPVars(Ptr)
                                        OptionsForCPVars(Ptr).Name = Trim(OptionName)
                                        OptionsForCPVars(Ptr).Value = OptionValue
                                        OptionsForCPVars_Cnt = OptionsForCPVars_Cnt + 1
                                    End If
                                Next
                            End If
                            '
                            ' this is a hack -- add instanceID to the OptionsForCPVars. do the same in addon_executeAsProcess
                            '   it is also added in csv_BuildAddonOptionLists() which is called by both, but does not effect OptionsForCPVars.
                            '   the cpCoreClass execute should call executeAsProcess and share all this code.
                            '
                            If ACInstanceID <> "" Then
                                ReDim Preserve OptionsForCPVars(OptionsForCPVars_Cnt)
                                OptionsForCPVars(OptionsForCPVars_Cnt).Name = "instanceid"
                                OptionsForCPVars(OptionsForCPVars_Cnt).Value = ACInstanceID
                                OptionsForCPVars_Cnt = OptionsForCPVars_Cnt + 1
                            End If
                            '
                            '-----------------------------------------------------------------------------------------------------
                            ' Build ReplaceName, ReplaceValue pairs for call to cmc.csv_ExecuteScript
                            '-----------------------------------------------------------------------------------------------------
                            '
                            ReplaceCnt = OptionsForCPVars_Cnt
                            If ReplaceCnt > 0 Then
                                ReDim ReplaceNames(ReplaceCnt - 1)
                                ReDim ReplaceValues(ReplaceCnt - 1)
                                For Ptr = 0 To ReplaceCnt - 1
                                    With OptionsForCPVars(Ptr)
                                        If .Name <> "" Then
                                            ReplaceNames(Ptr) = .Name
                                            'ReplaceNames(Ptr) = "$" & .Name & "$"
                                            ReplaceValues(Ptr) = .Value
                                        End If
                                    End With
                                Next
                            End If
                            '
                            '-----------------------------------------------------------------------------------------------------
                            '   Common to Add-ons and built-in Add-ons
                            '-----------------------------------------------------------------------------------------------------
                            '
                            ' Update the option selector from the addon record
                            '
                            '!!!!!
                            ' instanceId option pair is added here, but OptionsForCPVars() is already constructed and does not have it -- so scripts will not get it
                            ' instanceId needs to be added early in preprocess so it gets picked up in OptionsForCPVars()
                            '!!!!!
                            Call buildAddonOptionLists(OptionString_ForObjectCall, AddonOptionExpandedConstructor, AddonOptionConstructor, WorkingOptionString, ACInstanceID, IncludeEditWrapper)
                            '
                            ' set global public value that can be accessed by scripts
                            '
                            OptionString = OptionString_ForObjectCall
                            '
                            '
                            '-----------------------------------------------------------------------------------------------------
                            ' Process the content for each context as needed
                            '-----------------------------------------------------------------------------------------------------
                            '
                            If (InFrame And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                '
                                '-----------------------------------------------------------------
                                ' inFrame and this is NOT the callback - setup the iframe for a callback
                                ' js,styles and other features are NOT added to the host page, they go to the remotemethod page
                                '-----------------------------------------------------------------
                                '
                                If isMainOk Then
                                    '
                                    ' web-only
                                    '
                                    Link = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & requestAppRootPath & cpCore.siteProperties.serverPageDefault
                                    If genericController.vbInstr(1, Link, "?") = 0 Then
                                        Link = Link & "?"
                                    Else
                                        Link = Link & "&"
                                    End If
                                    Link = Link _
                                        & "nocache=" & Rnd() _
                                        & "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                        & "&HostRecordID=" & HostRecordID _
                                        & "&remotemethodaddon=" & EncodeURL(AddonNameOrGuid_Local) _
                                        & "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                        & ""
                                    FrameID = "frame" & getRandomLong()
                                    returnVal = "<iframe src=""" & Link & """ id=""" & FrameID & """ onload=""cj.setFrameHeight('" & FrameID & "');"" class=""ccAddonFrameCon"" frameborder=""0"" scrolling=""no"">This content is not visible because your browser does not support iframes</iframe>" _
                                        & cr & "<script language=javascript type=""text/javascript"">" _
                                        & cr & "// Safari and Opera need a kick-start." _
                                        & cr & "var e=document.getElementById('" & FrameID & "');if(e){var iSource=e.src;e.src='';e.src = iSource;}" _
                                        & cr & "</script>"
                                End If
                            ElseIf (AsAjax And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                '
                                '-----------------------------------------------------------------
                                ' AsAjax and this is NOT the callback - setup the ajax callback
                                ' js,styles and other features from the addon record are added to the host page
                                ' during the remote method, these are blocked, but if any are added during
                                '   DLL processing, they have to be handled
                                '-----------------------------------------------------------------
                                '
                                If isMainOk Then
                                    AsAjaxID = "asajax" & getRandomLong()
                                    QS = "" _
                                        & RequestNameRemoteMethodAddon & "=" & EncodeRequestVariable(AddonNameOrGuid_Local) _
                                        & "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                        & "&HostRecordID=" & HostRecordID _
                                        & "&HostRQS=" & EncodeRequestVariable(cpCore.web_RefreshQueryString) _
                                        & "&HostQS=" & EncodeRequestVariable(cpCore.webServer.requestQueryString) _
                                        & "&HostForm=" & EncodeRequestVariable(cpCore.webServer.requestFormString) _
                                        & "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                        & ""
                                    If IsInline Then
                                        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon"" style=""display:inline;""><img src=""/ccLib/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
                                    Else
                                        returnVal = cr & "<div ID=" & AsAjaxID & " Class=""ccAddonAjaxCon""><img src=""/ccLib/images/ajax-loader-small.gif"" width=""16"" height=""16""></div>"
                                    End If
                                    returnVal = returnVal _
                                        & cr & "<script Language=""javaScript"" type=""text/javascript"">" _
                                        & cr & "cj.ajax.qs('" & QS & "','','" & AsAjaxID & "');AdminNavPop=true;" _
                                        & cr & "</script>"
                                    '
                                    ' Problem - AsAjax addons must add styles, js and meta to the head
                                    '   Adding them to the host page covers most cases, but sometimes the DLL itself
                                    '   adds styles, etc during processing. These have to be added during the remote method processing.
                                    '   appending the .innerHTML of the head works for FF, but ie blocks it.
                                    '   using .createElement works in ie, but the tag system right now not written
                                    '   to save links, etc, it is written to store the entire tag.
                                    '   Also, OtherHeadTags can not be added this was.
                                    '
                                    ' Short Term Fix
                                    '   For Ajax, Add javascript and style features to head of host page
                                    '   Then during remotemethod, clear these strings before dll processing. Anything
                                    '   that is added must have come from the dll. So far, the only addons we have that
                                    '   do this load styles, so instead of putting in the the head (so ie fails), add styles inline.
                                    '
                                    '   This is because ie does not allow innerHTML updates to head tag
                                    '   scripts and js could be handled with .createElement if only the links were saved, but
                                    '   otherhead could not.
                                    '   The case this does not cover is if the addon itself manually adds one of these entries.
                                    '   In no case can ie handle the OtherHead, however, all the others can be done with .createElement.
                                    ' Long Term Fix
                                    '   Convert js, style, and meta tag system to use .createElement during remote method processing
                                    '
                                    Call cpCore.htmlDoc.main_AddPagetitle2(PageTitle, AddedByName)
                                    Call cpCore.htmlDoc.main_addMetaDescription2(MetaDescription, AddedByName)
                                    Call cpCore.htmlDoc.main_addMetaKeywordList2(MetaKeywordList, AddedByName)
                                    Call cpCore.htmlDoc.main_AddHeadTag2(OtherHeadTags, AddedByName)
                                    If Not blockJavascriptAndCss Then
                                        '
                                        ' add javascript and styles if it has not run already
                                        '
                                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2(JSOnLoad, AddedByName)
                                        Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(JSBodyEnd, AddedByName)
                                        Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, AddedByName)
                                        If DefaultStylesFilename <> "" Then
                                            Call cpCore.htmlDoc.main_AddStylesheetLink2(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, DefaultStylesFilename), AddonName & " default")
                                        End If
                                        If CustomStylesFilename <> "" Then
                                            Call cpCore.htmlDoc.main_AddStylesheetLink2(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, CustomStylesFilename), AddonName & " custom")
                                        End If
                                    End If
                                End If
                            Else
                                '
                                '-----------------------------------------------------------------
                                ' otherwise - produce the content from the addon
                                '   setup RQS as needed - RQS provides the querystring for add-ons to create links that return to the same page
                                '-----------------------------------------------------------------------------------------------------
                                '
                                If (InFrame And (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                    '
                                    ' Add-on setup for InFrame, running the call-back - this page must think it is just the remotemethod
                                    '
                                    If isMainOk Then
                                        Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(RequestNameRemoteMethodAddon, AddonNameOrGuid_Local)
                                        Call cpCore.htmlDoc.webServerIO_addRefreshQueryString("optionstring", WorkingOptionString)
                                    End If
                                ElseIf (AsAjax And (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                    '
                                    ' Add-on setup for AsAjax, running the call-back - put the referring page's QS as the RQS
                                    ' restore form values
                                    '
                                    If isMainOk Then
                                        QS = cpCore.docProperties.getText("Hostform")
                                        If QS <> "" Then
                                            Call cpCore.docProperties.addQueryString(QS)
                                        End If
                                        '
                                        ' restore refresh querystring values
                                        '
                                        QS = cpCore.docProperties.getText("HostRQS")
                                        QSSplit = Split(QS, "&")
                                        For Ptr = 0 To UBound(QSSplit)
                                            NVPair = QSSplit(Ptr)
                                            If NVPair <> "" Then
                                                NVSplit = Split(NVPair, "=")
                                                If UBound(NVSplit) > 0 Then
                                                    Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(NVSplit(0), NVSplit(1))
                                                End If
                                            End If
                                        Next
                                        '
                                        ' restore query string
                                        '
                                        QS = cpCore.docProperties.getText("HostQS")
                                        Call cpCore.docProperties.addQueryString(QS)
                                        '
                                        ' Clear the style,js and meta features that were delivered to the host page
                                        ' After processing, if these strings are not empty, they must have been added by the DLL
                                        '
                                        '
                                        JSOnLoad = ""
                                        JSBodyEnd = ""
                                        PageTitle = ""
                                        MetaDescription = ""
                                        MetaKeywordList = ""
                                        OtherHeadTags = ""
                                        DefaultStylesFilename = ""
                                        CustomStylesFilename = ""
                                    End If
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' gather list of included add-ons
                                ' do not run yet because CP has not been created
                                ' moved here from below to catch scripting entry
                                ' moved to within the CP check bc this call includes CP which has not been created
                                '-----------------------------------------------------------------
                                '
                                Ptr = cpCore.cache_addonIncludeRules_getFirstPtr(addonId)
                                Do While Ptr >= 0
                                    addon_IncludedAddonIDList = addon_IncludedAddonIDList & "," & cpCore.cache_addonIncludeRules.item(addonIncludeRulesCache_includedAddonId, Ptr)
                                    Ptr = cpCore.cache_addonIncludeRules.addonIdIndex.getNextPtrMatch(CStr(addonId))
                                Loop
                                '
                                '-----------------------------------------------------------------
                                ' Do replacements from Option String and Pick out WrapperID, and AsAjax
                                '-----------------------------------------------------------------
                                '
                                TestString = HTMLContent & TextContent & PageTitle & MetaDescription & MetaKeywordList & OtherHeadTags & FormXML
                                If (TestString <> "") And (ReplaceCnt > 0) Then
                                    For Ptr = 0 To ReplaceCnt - 1
                                        ReplaceSource = "$" & ReplaceNames(Ptr) & "$"
                                        ' this section takes 15msec every addon, 32 addons is 480msec.
                                        ' 20131221 - 4.2.317 - try test first to save time
                                        If isInStr(1, TestString, ReplaceSource) Then
                                            ReplaceValue = ReplaceValues(Ptr)
                                            HTMLContent = genericController.vbReplace(HTMLContent, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            TextContent = genericController.vbReplace(TextContent, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            PageTitle = genericController.vbReplace(PageTitle, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            MetaDescription = genericController.vbReplace(MetaDescription, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            MetaKeywordList = genericController.vbReplace(MetaKeywordList, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            OtherHeadTags = genericController.vbReplace(OtherHeadTags, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                            FormXML = genericController.vbReplace(FormXML, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
                                        End If
                                    Next
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' CP compatible section
                                '-----------------------------------------------------------------
                                '
                                If (addon_IncludedAddonIDList <> "") Or (ScriptingCode <> "") Or (DotNetClassFullName <> "") Then
                                    For Ptr = 0 To UBound(OptionsForCPVars)
                                        '
                                        ' REFACTOR -- REFACTOR -- REFACTOR -- REFACTOR -- REFACTOR -- REFACTOR
                                        ' all these legacy option string systems need to do -- but this was creating a problem and it needed to be fixed asap
                                        ' if an addon processes an upload, this setProperty() would crush the docproperties .isFile , etc
                                        ' so -- only add the ones that are not already there -- this is  temp fix until all this is removed
                                        '
                                        If Not cpCore.docProperties.containsKey(OptionsForCPVars(Ptr).Name) Then
                                            cpCore.docProperties.setProperty(OptionsForCPVars(Ptr).Name, OptionsForCPVars(Ptr).Value)
                                        End If
                                        'cpCore.docProperties.setProperty(OptionsForCPVars(Ptr).Name, OptionsForCPVars(Ptr).Value)
                                    Next
                                    '
                                    ' ----- run included add-ons before their parent
                                    ' should be the first executable to run so includes run first
                                    ' moved here from above because CP is needed
                                    '
                                    If addon_IncludedAddonIDList <> "" Then
                                        includedAddonIds = Split(addon_IncludedAddonIDList, ",")
                                        For Ptr = 0 To UBound(includedAddonIds)
                                            includedAddonId = genericController.EncodeInteger(includedAddonIds(Ptr))
                                            If includedAddonId <> 0 Then
                                                IncludeContent = IncludeContent & execute(includedAddonId, "", "", CPUtilsBaseClass.addonContext.ContextAdmin, HostContentName, HostRecordID, HostFieldName, ACInstanceID, True, DefaultWrapperID, ignore_TemplateCaseOnly_PageContent, AddonStatusOK, Nothing, ignore_addonCallingItselfIdList & "," & addonId, Nothing, ignore_AddonsRunOnThisPageIdList, personalizationPeopleId, personalizationIsAuthenticated)
                                            End If
                                        Next
                                    End If
                                    '
                                    ' ----- Scripting
                                    '
                                    'hint = hint & ",9"
                                    If (ScriptingCode <> "") Then
                                        'hint = "Processing Addon [" & AddonName & "], calling script component."
                                        Try
                                            ScriptContent = executeScript4(ScriptingLanguage, ScriptingCode, ScriptingEntryPoint, errorMessageForAdmin, ScriptingTimeout, "Addon [" & AddonName & "]", ReplaceCnt, ReplaceNames, ReplaceValues)
                                        Catch ex As Exception
                                            Throw New ApplicationException("There was an error executing the script component of Add-on [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "")
                                        End Try
                                    End If
                                    '
                                    ' ----- Dot Net Addons
                                    '   Get path to the addon from the collection guid
                                    '   If no collection, just look in the /addon path
                                    '
                                    'hint = hint & ",10"
                                    If DotNetClassFullName <> "" Then
                                        '
                                        Dim csTmp As Integer
                                        csTmp = cpCore.db.cs_openCsSql_rev("default", "select ccGuid from ccAddonCollections where id=" & addonCollectionId)
                                        If cpCore.db.cs_ok(csTmp) Then
                                            CollectionGuid = cpCore.db.cs_getText(csTmp, "ccGuid")
                                        End If
                                        Call cpCore.db.cs_Close(csTmp)
                                        '
                                        AssemblyContent = executeAssembly(addonId, AddonName, DotNetClassFullName, CollectionGuid, Nothing, errorMessageForAdmin)
                                        If (errorMessageForAdmin <> "") Then
                                            '
                                            ' log the error
                                            '
                                            Call cpCore.handleLegacyError8("Error during cmc.csv_ExecuteAssembly [" & errorMessageForAdmin & "]", "cpCoreClass.addon_execute_internal", True)
                                            '
                                            ' Put up an admin hint
                                            '
                                            If (Not isMainOk) Or (Context = CPUtilsBaseClass.addonContext.ContextEmail) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) Or (Context = CPUtilsBaseClass.addonContext.ContextSimple) Then
                                                '
                                                ' Block all output even on error
                                                '
                                            ElseIf cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                                                '
                                                ' Provide hint to administrators
                                                '
                                                If AddonName = "" And addonId <> 0 Then
                                                    AddonName = "Addon #" & addonId
                                                End If
                                                AssemblyContent = cpCore.htmlDoc.html_GetAdminHintWrapper("<p>There was an error executing the assembly component of Add-on [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "] with class name [" & DotNetClassFullName & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "</p>")
                                            End If
                                        End If
                                    End If
                                End If
                                ''
                                ''-----------------------------------------------------------------
                                '' ActiveX Addons
                                ''-----------------------------------------------------------------
                                ''
                                ''hint = "Processing Addon [" & AddonName & "], ActiveX Addons section"
                                'If ProgramID <> "" Then
                                '    '
                                '    ' Go ahead
                                '    '
                                '    Try
                                '        ObjectContent = csv_ExecuteActiveX(ProgramID, AddonName, OptionString_ForObjectCall, WorkingOptionString, errorMessageForAdmin)
                                '    Catch ex As Exception
                                '        handleException(ex, "There was an error executing the activex component of Add-on [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "], with Program ID [" & ProgramID & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "")
                                '    End Try
                                '    AggrObject = Nothing
                                'End If
                                ''hint = ""
                                '
                                '-----------------------------------------------------------------------------------------------------
                                '   Script Include
                                '       Adds a comment with a script include file
                                '-----------------------------------------------------------------------------------------------------
                                '
                                If (True) And (inlineScript <> "") Then
                                    inlineScriptContent = "<!-- inlineScript(" & cpCore.csv_ConnectionID & ")[" & cpCore.htmlDoc.html_EncodeHTML(inlineScript) & "] -->"
                                End If
                                '
                                '-----------------------------------------------------------------------------------------------------
                                '   RemoteAssetLink
                                '-----------------------------------------------------------------------------------------------------
                                '
                                If (True) Then
                                    If RemoteAssetLink <> "" Then
                                        WorkingLink = RemoteAssetLink
                                        If genericController.vbInstr(1, WorkingLink, "://") = 0 Then
                                            If isMainOk Then
                                                '
                                                ' use request object to build link
                                                '
                                                If Mid(WorkingLink, 1, 1) = "/" Then
                                                    WorkingLink = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & WorkingLink
                                                Else
                                                    WorkingLink = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.webServer.webServerIO_requestVirtualFilePath & WorkingLink
                                                End If
                                            Else
                                                '
                                                ' use assumptions
                                                '
                                                If Mid(WorkingLink, 1, 1) = "/" Then
                                                    WorkingLink = "http://" & cpCore.serverConfig.appConfig.domainList(0) & WorkingLink
                                                Else
                                                    WorkingLink = "http://" & cpCore.serverConfig.appConfig.domainList(0) & "/" & WorkingLink
                                                End If
                                            End If
                                        End If
                                        Dim PosStart As Integer
                                        kmaHTTP = New coreHttpRequestClass()
                                        RemoteAssetContent = kmaHTTP.getURL(WorkingLink)
                                        Pos = genericController.vbInstr(1, RemoteAssetContent, "<body", vbTextCompare)
                                        If Pos > 0 Then
                                            Pos = genericController.vbInstr(Pos, RemoteAssetContent, ">")
                                            If Pos > 0 Then
                                                PosStart = Pos + 1
                                                Pos = genericController.vbInstr(Pos, RemoteAssetContent, "</body", vbTextCompare)
                                                If Pos > 0 Then
                                                    RemoteAssetContent = Mid(RemoteAssetContent, PosStart, Pos - PosStart)
                                                End If
                                            End If
                                        End If

                                    End If
                                End If
                                '
                                '-----------------------------------------------------------------------------------------------------
                                '   FormXML
                                '-----------------------------------------------------------------------------------------------------
                                '
                                If isMainOk And (FormXML <> "") Then
                                    FormContent = getFormContent(Nothing, FormXML, ExitAddonWithBlankResponse)
                                    If ExitAddonWithBlankResponse Then
                                        Exit Function
                                    End If
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' Script Callback
                                '-----------------------------------------------------------------
                                '
                                '#If traceExecuteAddon Then
                                'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") z"
                                '#End If
                                If isMainOk And (Link <> "") Then
                                    If WorkingOptionString <> "" Then
                                        If genericController.vbInstr(1, Link, "?") = 0 Then
                                            Link = Link & "?" & WorkingOptionString
                                        Else
                                            Link = Link & "&" & WorkingOptionString
                                        End If
                                    End If
                                    Link = modifyLinkQuery(Link, RequestNameJSForm, "1", True)
                                    Link = EncodeAppRootPath(Link, cpCore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
                                    ScriptCallbackContent = "<SCRIPT LANGUAGE=""JAVASCRIPT"" SRC=""" & Link & """></SCRIPT>"
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' Add javascripts and other features to page
                                '-----------------------------------------------------------------
                                '
                                AddedByName = AddonName & " addon"
                                '
                                '#If traceExecuteAddon Then
                                'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") aa"
                                '#End If
                                If isMainOk Then
                                    Call cpCore.htmlDoc.main_AddPagetitle2(PageTitle, AddedByName)
                                    Call cpCore.htmlDoc.main_addMetaDescription2(MetaDescription, AddedByName)
                                    Call cpCore.htmlDoc.main_addMetaKeywordList2(MetaKeywordList, AddedByName)
                                    Call cpCore.htmlDoc.main_AddHeadTag2(OtherHeadTags, AddedByName)
                                    If Not blockJavascriptAndCss Then
                                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2(JSOnLoad, AddedByName)
                                        Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(JSBodyEnd, AddedByName)
                                        Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, AddedByName)
                                        If DefaultStylesFilename <> "" Then
                                            Call cpCore.htmlDoc.main_AddStylesheetLink2(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, DefaultStylesFilename), AddonName & " default")
                                        End If
                                        If CustomStylesFilename <> "" Then
                                            Call cpCore.htmlDoc.main_AddStylesheetLink2(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, CustomStylesFilename), AddonName & " custom")
                                        End If
                                    End If
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' 2012-6-8 Merge together all the pieces
                                '   - moved the encode content call here from below so the content parts can be encoded and not the rest
                                '   - below, the csv_EncodeContent addonContext was being hard-coded to contextAdmin, which makes no sense.
                                '   - now I let the original context get through, but only call executeContentCommand on the content part (that the admin controls)
                                '   - csv_executeContentCommands on only the content parts
                                '   - csv_EncodeContent on everything
                                '-----------------------------------------------------------------
                                '
                                '#If traceExecuteAddon Then
                                'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") ab"
                                '#End If
                                Dim layoutErrors As String
                                If (Context = CPUtilsBaseClass.addonContext.ContextEditor) Then
                                    '
                                    ' editor -- no encoding and no contentcommands
                                    '
                                    returnVal = TextContent & HTMLContent
                                    returnVal = returnVal & IncludeContent & ScriptCallbackContent & FormContent & RemoteAssetContent & ScriptContent & ObjectContent & AssemblyContent & inlineScriptContent
                                    '
                                    ' csv_EncodeContent everything
                                    '
                                    's = csv_EncodeContent9(s, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (Context = ContextEmail),  WrapperID, ignore_TemplateCaseOnly_PageContent, Context, personalizationIsAuthenticated, nothing, False)
                                Else
                                    '
                                    ' encode the content parts of the addon
                                    '
                                    returnVal = TextContent & HTMLContent
                                    If returnVal <> "" Then
                                        returnVal = cpCore.htmlDoc.html_executeContentCommands(Nothing, returnVal, CPUtilsBaseClass.addonContext.ContextAdmin, personalizationPeopleId, personalizationIsAuthenticated, layoutErrors)
                                        's = csv_EncodeContent9(s, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (Context = ContextEmail), WrapperID, ignore_TemplateCaseOnly_PageContent, Context, personalizationIsAuthenticated, nothing, False)
                                    End If
                                    '
                                    ' add in the rest
                                    '
                                    returnVal = returnVal & IncludeContent & ScriptCallbackContent & FormContent & RemoteAssetContent & ScriptContent & ObjectContent & AssemblyContent & inlineScriptContent
                                    '
                                    ' csv_EncodeContent everything
                                    '
                                    returnVal = cpCore.htmlDoc.html_encodeContent10(returnVal, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (Context = CPUtilsBaseClass.addonContext.ContextEmail), WrapperID, ignore_TemplateCaseOnly_PageContent, Context, personalizationIsAuthenticated, Nothing, False)
                                End If
                                ''
                                '' +++++ 9/8/2011, 4.1.482
                                ''
                                's = TextContent & HTMLContent & IncludeContent & ScriptCallbackContent & FormContent & RemoteAssetContent & ScriptContent & ObjectContent & AssemblyContent & inlineScriptContent
                                's = genericController.vbReplace(s, "{%", "{<!---->%")
                                '
                                '-----------------------------------------------------------------
                                ' check for xml contensive process instruction
                                '   This is also handled in Encode Content, but here we can return the admin error message
                                '   Once processed, it will skip the csv_EncodeContent processesing anyway
                                '-----------------------------------------------------------------
                                '
                                '#If traceExecuteAddon Then
                                'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") ac"
                                '#End If
                                Pos = genericController.vbInstr(1, returnVal, "<?contensive", vbTextCompare)
                                If Pos > 0 Then
                                    Throw New ApplicationException("xml structured commands are no longer supported")
                                    ''
                                    ''output is xml structured data
                                    '' pass the data in as an argument to the structured data processor
                                    '' and return its result
                                    ''
                                    's = Mid(s, Pos)
                                    'LayoutEngineOptionString = "data=" & encodeNvaArgument(s)
                                    'Dim structuredData As New core_primitivesStructuredDataClass(Me)
                                    's = structuredData.execute()
                                    's = csv_ExecuteActiveX("aoPrimitives.StructuredDataClass", "Structured Data Engine", LayoutEngineOptionString, "data=(structured data)", errorMessageForAdmin)
                                    'If (errorMessageForAdmin <> "") Then
                                    '    '
                                    '    ' Put up an admin hint
                                    '    '
                                    '    If (Not isMainOk) Or (Context = CPUtilsBaseClass.addonContext.contextEmail) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethod) Or (Context = CPUtilsBaseClass.addonContext.ContextSimple) Then
                                    '        '
                                    '        ' Block all output even on error
                                    '        '
                                    '    ElseIf  cpcore.authContext.user.user_isAdmin() Then
                                    '        '
                                    '        ' Provide hint to administrators
                                    '        '
                                    '        If AddonName = "" And addonId <> 0 Then
                                    '            AddonName = "Addon #" & addonId
                                    '        End If
                                    '        s = s & cpCore.main_GetAdminHintWrapper("<p>There was an error executing the Layout Engine for addon [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "], with Program ID [" & ProgramID & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "</p>")
                                    '    End If
                                    'End If
                                End If
                                '
                                '-----------------------------------------------------------------
                                ' Add Css containers
                                '-----------------------------------------------------------------
                                '
                                '#If traceExecuteAddon Then
                                'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") ad"
                                '#End If
                                If ContainerCssID <> "" Or ContainerCssClass <> "" Then
                                    If IsInline Then
                                        returnVal = cr & "<div id=""" & ContainerCssID & """ class=""" & ContainerCssClass & """ style=""display:inline;"">" & returnVal & "</div>"
                                    Else
                                        returnVal = cr & "<div id=""" & ContainerCssID & """ class=""" & ContainerCssClass & """>" & kmaIndent(returnVal) & cr & "</div>"
                                    End If
                                End If
                            End If
                            '
                            '-----------------------------------------------------------------
                            '   Add Wrappers to content
                            '-----------------------------------------------------------------
                            '
                            '#If traceExecuteAddon Then
                            'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") ae"
                            '#End If
                            If isMainOk Then
                                If (InFrame And (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                    '
                                    ' Return IFrame content
                                    '   Framed in content, during the remote method call
                                    '   add in the rest of the html page
                                    '
                                    Call cpCore.main_SetMetaContent(0, 0)
                                    returnVal = "" _
                                        & cpCore.main_docType _
                                        & vbCrLf & "<html>" _
                                        & cr & "<head>" _
                                        & vbCrLf & kmaIndent(cpCore.main_GetHTMLHead()) _
                                        & cr & "</head>" _
                                        & cr & TemplateDefaultBodyTag _
                                        & cr & "</body>" _
                                        & vbCrLf & "</html>"
                                ElseIf (AsAjax And (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                    '
                                    ' Return Ajax content
                                    '   AsAjax addon, during the Ajax callback
                                    '   need to create an onload event that runs everything appended to onload within this content
                                    '
                                    returnVal = returnVal
                                ElseIf ((Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                    '
                                    ' Return non-ajax/non-Iframe remote method content (no wrapper)
                                    '
                                ElseIf (Context = CPUtilsBaseClass.addonContext.ContextEmail) Then
                                    '
                                    ' Return Email context (no wrappers)
                                    '
                                ElseIf (Context = CPUtilsBaseClass.addonContext.ContextSimple) Then
                                    '
                                    ' Add-on called by another add-on, subroutine style (no wrappers)
                                    '
                                Else
                                    '
                                    ' Return all other types
                                    '
                                    If IncludeEditWrapper Then
                                        '
                                        ' Add Edit Wrapper
                                        '
                                        EditWrapperHTMLID = "eWrapper" & cpCore.pageManager_PageAddonCnt
                                        'HelpIcon = cpcore.main_GetHelpLink("", "Add-on " & AddonName, helpCopy, helpLink)
                                        '
                                        ' Edit Icon
                                        '
                                        If (addonId <> 0) Then
                                            If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                                                AddonEditIcon = GetIconSprite("", 0, "/ccLib/images/tooledit.png", 22, 22, "Edit the " & AddonName & " Add-on", "Edit the " & AddonName & " Add-on", "", True, "")
                                                AddonEditIcon = "<a href=""" & cpCore.siteProperties.adminURL & "?cid=" & cpCore.metaData.getContentId(cnAddons) & "&id=" & addonId & "&af=4&aa=2&ad=1"" tabindex=""-1"">" & AddonEditIcon & "</a>"
                                                InstanceSettingsEditIcon = getInstanceBubble(AddonName, AddonOptionExpandedConstructor, HostContentName, HostRecordID, HostFieldName, ACInstanceID, Context, DialogList)
                                                AddonStylesEditIcon = getAddonStylesBubble(addonId, DialogList)
                                                HTMLViewerEditIcon = getHTMLViewerBubble(addonId, "editWrapper" & cpCore.htmlDoc.html_EditWrapperCnt, DialogList)
                                                HelpIcon = getHelpBubble(addonId, helpCopy, addonCollectionId, DialogList)
                                                ToolBar = InstanceSettingsEditIcon & AddonEditIcon & AddonStylesEditIcon & SiteStylesEditIcon & HTMLViewerEditIcon & HelpIcon
                                                ToolBar = genericController.vbReplace(ToolBar, "&nbsp;", "", 1, 99, vbTextCompare)
                                                returnVal = cpCore.htmlDoc.main_GetEditWrapper("<div class=""ccAddonEditTools"">" & ToolBar & "&nbsp;" & AddonName & DialogList & "</div>", returnVal)
                                                's = GetEditWrapper("<div class=""ccAddonEditCaption"">" & AddonName & "</div><div class=""ccAddonEditTools"">" & ToolBar & "</div>", s)
                                            ElseIf cpCore.visitProperty.getBoolean("AllowEditing") Then
                                                returnVal = cpCore.htmlDoc.main_GetEditWrapper("<div class=""ccAddonEditCaption"">" & AddonName & "&nbsp;" & HelpIcon & "</div>", returnVal)
                                            End If
                                        End If
                                    End If
                                    ' moved to calling routines - so if this is called from an add-on without context, the data may not be html
                                    '
                                    ' Add Comment wrapper - to help debugging except email, remote methods and admin (empty is used to detect no result)
                                    '
                                    If isMainOk And (Context <> CPUtilsBaseClass.addonContext.ContextAdmin) And (Context <> CPUtilsBaseClass.addonContext.ContextEmail) And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (Context <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) And (Context <> CPUtilsBaseClass.addonContext.ContextSimple) Then
                                        If cpCore.visitProperty.getBoolean("AllowDebugging") Then
                                            AddonCommentName = genericController.vbReplace(AddonName, "-->", "..>")
                                            If IsInline Then
                                                returnVal = "<!-- Add-on " & AddonCommentName & " -->" & returnVal & "<!-- /Add-on " & AddonCommentName & " -->"
                                            Else
                                                returnVal = "" _
                                                    & cr & "<!-- Add-on " & AddonCommentName & " -->" _
                                                    & kmaIndent(returnVal) _
                                                    & cr & "<!-- /Add-on " & AddonCommentName & " -->"
                                            End If
                                        End If
                                    End If
                                    '
                                    ' Add Design Wrapper
                                    '
                                    If (returnVal <> "") And (Not IsInline) And (WrapperID > 0) And (True) Then
                                        returnVal = addWrapperToResult(returnVal, WrapperID, "for Add-on " & AddonName)
                                    End If
                                End If
                            End If
                            '
                            ' this completes the execute of this cpcore.addon. remove it from the 'running' list
                            '
                            cpCore.addonsCurrentlyRunningIdList.Remove(addonId)
                            'csv_addon_execute_AddonsCurrentlyRunningIdList = genericController.vbReplace(csv_addon_execute_AddonsCurrentlyRunningIdList & ",", "," & addonId & ",", ",")
                        End If
                    End If
                End If
                OptionString = PushOptionString
                '
                cpCore.pageManager_PageAddonCnt = cpCore.pageManager_PageAddonCnt + 1
            Catch ex As Exception
                '
                ' protect environment from addon error
                '
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return returnVal
        End Function
        '
        '
        '
        Private Function getFormContent(ByVal nothingObject As Object, ByVal FormXML As String, ByRef return_ExitAddonBlankWithResponse As Boolean) As String
            'Const Tn = "addon_execute_internal_getFormContent" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Const LoginMode_None = 1
            Const LoginMode_AutoRecognize = 2
            Const LoginMode_AutoLogin = 3
            '
            Dim PageSize As Integer
            Dim FieldCount As Integer
            Dim RowMax As Integer
            Dim ColumnMax As Integer
            'Dim RecordField As Field
            Dim SQLPageSize As Integer
            'dim dt as datatable
            Dim ErrorNumber As Integer
            Dim ErrorDescription As String
            Dim something As Object(,)
            Dim RecordID As Integer
            'Dim XMLTools As New xmlToolsclass(me)
            Dim fieldfilename As String
            'Dim fs As New fileSystemClass
            Dim FieldDataSource As String
            Dim FieldSQL As String
            Dim LoginMode As Integer
            Dim Help As String
            Dim Content As New coreFastStringClass
            Dim Copy As String
            Dim Button As String
            Dim PageNotFoundPageID As String
            Dim Adminui As New coreAdminUIClass(cpCore)
            Dim ButtonList As String
            Dim AllowLinkAlias As Boolean
            Dim LinkForwardAutoInsert As Boolean
            Dim SectionLandingLink As String
            Dim LandingPageID As String
            Dim AllowAutoRecognize As Boolean
            Dim AllowMobileTemplates As Boolean
            Dim Filename As String
            Dim NonEncodedLink As String
            Dim EncodedLink As String
            Dim VirtualFilePath As String
            Dim OptionString As String
            Dim TabName As String
            Dim TabDescription As String
            Dim TabHeading As String
            Dim TabCnt As Integer
            Dim TabCell As coreFastStringClass
            Dim loadOK As Boolean = True
            Dim FieldValue As String
            Dim FieldDescription As String
            Dim FieldDefaultValue As String
            Dim IsFound As Boolean
            Dim Name As String
            Dim Description As String
            Dim LoopPtr As Integer
            Dim XMLFile As String
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
                return_ExitAddonBlankWithResponse = True
                Exit Function
            ElseIf Not cpcore.authContext.isAuthenticatedAdmin(cpcore) Then
                '
                ' Not Admin Error
                '
                ButtonList = ButtonCancel
                Content.Add(Adminui.GetFormBodyAdminOnly())
            Else
                If True Then
                    loadOK = True
                    Try
                        Doc.LoadXml(FormXML)
                    Catch ex As Exception
                        ButtonList = ButtonCancel
                        Content.Add("<div class=""ccError"" style=""margin:10px;padding:10px;background-color:white;"">There was a problem with the Setting Page you requested.</div>")
                        loadOK = False
                    End Try
                    If loadOK Then
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
                                                            FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            FieldValue = cpCore.docProperties.getText(FieldName)
                                                            fieldType = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "type", "")
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
                                                                        cpCore.cdnFiles.saveUpload(FieldName, VirtualFilePath, Filename)
                                                                        Call cpCore.siteProperties.setProperty(FieldName, VirtualFilePath & Filename)
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
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            If Not FieldReadOnly Then
                                                                FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                FieldHTML = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "html", "false"))
                                                                If FieldHTML Then
                                                                    '
                                                                    ' treat html as active content for now.
                                                                    '
                                                                    FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName)
                                                                Else
                                                                    FieldValue = cpCore.docProperties.getText(FieldName)
                                                                End If

                                                                CS = cpCore.db.cs_open("Copy Content", "name=" & cpCore.db.encodeSQLText(FieldName), "ID")
                                                                If Not cpCore.db.cs_ok(CS) Then
                                                                    Call cpCore.db.cs_Close(CS)
                                                                    CS = cpCore.db.cs_insertRecord("Copy Content", cpcore.authContext.authContextUser.id)
                                                                End If
                                                                If cpCore.db.cs_ok(CS) Then
                                                                    Call cpCore.db.cs_set(CS, "name", FieldName)
                                                                    '
                                                                    ' Set copy
                                                                    '
                                                                    Call cpCore.db.cs_set(CS, "copy", FieldValue)
                                                                    '
                                                                    ' delete duplicates
                                                                    '
                                                                    Call cpCore.db.cs_goNext(CS)
                                                                    Do While cpCore.db.cs_ok(CS)
                                                                        Call cpCore.db.cs_deleteRecord(CS)
                                                                        Call cpCore.db.cs_goNext(CS)
                                                                    Loop
                                                                End If
                                                                Call cpCore.db.cs_Close(CS)
                                                            End If

                                                        Case "filecontent"
                                                            '
                                                            ' A File Content block
                                                            '
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            If Not FieldReadOnly Then
                                                                FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                fieldfilename = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "filename", "")
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
                                return_ExitAddonBlankWithResponse = True
                                Exit Function
                            End If
                            '
                            ' ----- Display Form
                            '
                            Content.Add(Adminui.EditTableOpen)
                            Name = cpCore.csv_GetXMLAttribute(IsFound, Doc.DocumentElement, "name", "")
                            With Doc.DocumentElement
                                For Each SettingNode In .ChildNodes
                                    Select Case genericController.vbLCase(SettingNode.Name)
                                        Case "description"
                                            Description = SettingNode.InnerText
                                        Case "tab"
                                            TabCnt = TabCnt + 1
                                            TabName = cpCore.csv_GetXMLAttribute(IsFound, SettingNode, "name", "")
                                            TabDescription = cpCore.csv_GetXMLAttribute(IsFound, SettingNode, "description", "")
                                            TabHeading = cpCore.csv_GetXMLAttribute(IsFound, SettingNode, "heading", "")
                                            If TabHeading = "Debug and Trace Settings" Then
                                                TabHeading = TabHeading
                                            End If
                                            TabCell = New coreFastStringClass
                                            For Each TabNode In SettingNode.ChildNodes
                                                Select Case genericController.vbLCase(TabNode.Name)
                                                    Case "heading"
                                                        '
                                                        ' Heading
                                                        '
                                                        FieldCaption = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        Call TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption))
                                                    Case "siteproperty"
                                                        '
                                                        ' Site property
                                                        '
                                                        FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        If FieldName <> "" Then
                                                            FieldCaption = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            If FieldCaption = "" Then
                                                                FieldCaption = FieldName
                                                            End If
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldHTML = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                            fieldType = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "type", "")
                                                            FieldSelector = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "selector", "")
                                                            FieldDescription = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            FieldAddon = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "EditorAddon", "")
                                                            FieldDefaultValue = TabNode.InnerText
                                                            FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue)
                                                            If FieldAddon <> "" Then
                                                                '
                                                                ' Use Editor Addon
                                                                '
                                                                OptionString = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                Copy = execute_legacy5(0, FieldAddon, OptionString, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
                                                            ElseIf FieldSelector <> "" Then
                                                                '
                                                                ' Use Selector
                                                                '
                                                                Copy = getFormContent_decodeSelector(nothingObject, FieldName, FieldValue, FieldSelector)
                                                            Else
                                                                '
                                                                ' Use default editor for each field type
                                                                '
                                                                Select Case genericController.vbLCase(fieldType)
                                                                    Case "integer"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "boolean"
                                                                        If FieldReadOnly Then
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                            Copy = genericController.vbReplace(Copy, ">", " disabled>")
                                                                            Copy = Copy & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                        End If
                                                                    Case "float"
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "date"
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputDate(FieldName, FieldValue)
                                                                        End If
                                                                    Case "file", "imagefile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            If FieldValue = "" Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputFile(FieldName)
                                                                            Else
                                                                                NonEncodedLink = cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldValue)
                                                                                EncodedLink = EncodeURL(NonEncodedLink)
                                                                                Dim FieldValuefilename As String = ""
                                                                                Dim FieldValuePath As String = ""
                                                                                cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename)
                                                                                Copy = "" _
                                                                                    & "<a href=""http://" & EncodedLink & """ target=""_blank"">[" & FieldValuefilename & "]</A>" _
                                                                                    & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName & ".DeleteFlag", False) _
                                                                                    & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & cpCore.htmlDoc.html_GetFormInputFile(FieldName)
                                                                            End If
                                                                        End If
                                                                        'Call s.Add("&nbsp;</span></nobr></td>")
                                                                    Case "currency"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            If FieldValue <> "" Then
                                                                                FieldValue = FormatCurrency(FieldValue)
                                                                            End If
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "textfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            FieldValue = cpCore.cdnFiles.readFile(FieldValue)
                                                                            If FieldHTML Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                            End If
                                                                        End If
                                                                    Case "cssfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                        End If
                                                                    Case "xmlfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                        End If
                                                                    Case "link"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case Else
                                                                        '
                                                                        ' text
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Dim tmp As String
                                                                            tmp = cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                            Copy = FieldValue & tmp
                                                                        Else
                                                                            If FieldHTML Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
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
                                                        FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        If FieldName <> "" Then
                                                            FieldCaption = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            If FieldCaption = "" Then
                                                                FieldCaption = FieldName
                                                            End If
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldDescription = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            FieldHTML = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                            '
                                                            CS = cpCore.db.cs_open("Copy Content", "Name=" & cpCore.db.encodeSQLText(FieldName), "ID", , , , , "id,name,Copy")
                                                            If Not cpCore.db.cs_ok(CS) Then
                                                                Call cpCore.db.cs_Close(CS)
                                                                CS = cpCore.db.cs_insertRecord("Copy Content", cpcore.authContext.authContextUser.id)
                                                                If cpCore.db.cs_ok(CS) Then
                                                                    RecordID = cpCore.db.cs_getInteger(CS, "ID")
                                                                    Call cpCore.db.cs_set(CS, "name", FieldName)
                                                                    Call cpCore.db.cs_set(CS, "copy", genericController.encodeText(TabNode.InnerText))
                                                                    Call cpCore.db.cs_save2(CS)
                                                                    Call cpCore.workflow.publishEdit("Copy Content", RecordID)
                                                                End If
                                                            End If
                                                            If cpCore.db.cs_ok(CS) Then
                                                                FieldValue = cpCore.db.cs_getText(CS, "copy")
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
                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML3(FieldName, FieldValue)
                                                                'Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                            Else
                                                                '
                                                                ' Text edit
                                                                '
                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue)
                                                            End If
                                                            Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                        End If
                                                    Case "filecontent"
                                                        '
                                                        ' Content from a flat file
                                                        '
                                                        FieldName = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        FieldCaption = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        fieldfilename = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "filename", "")
                                                        FieldReadOnly = genericController.EncodeBoolean(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                        FieldDescription = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                        FieldDefaultValue = TabNode.InnerText
                                                        Copy = ""
                                                        If fieldfilename <> "" Then
                                                            If cpCore.appRootFiles.fileExists(fieldfilename) Then
                                                                Copy = FieldDefaultValue
                                                            Else
                                                                Copy = cpCore.cdnFiles.readFile(fieldfilename)
                                                            End If
                                                            If Not FieldReadOnly Then
                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, Copy, 10)
                                                            End If
                                                        End If
                                                        Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                    Case "dbquery", "querydb", "query", "db"
                                                        '
                                                        ' Display the output of a query
                                                        '
                                                        Copy = ""
                                                        FieldDataSource = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "DataSourceName", "")
                                                        FieldSQL = TabNode.InnerText
                                                        FieldCaption = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        FieldDescription = cpCore.csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                        SQLPageSize = genericController.EncodeInteger(cpCore.csv_GetXMLAttribute(IsFound, TabNode, "rowmax", ""))
                                                        If SQLPageSize = 0 Then
                                                            SQLPageSize = 100
                                                        End If
                                                        '
                                                        ' Run the SQL
                                                        '
                                                        Dim rs As DataTable
                                                        If FieldSQL <> "" Then
                                                            Try
                                                                rs = cpCore.db.executeSql(FieldSQL, FieldDataSource, , SQLPageSize)
                                                                'RS = app.csv_ExecuteSQLCommand(FieldDataSource, FieldSQL, 30, SQLPageSize, 1)

                                                            Catch ex As Exception

                                                                ErrorNumber = Err.Number
                                                                ErrorDescription = Err.Description
                                                                loadOK = False
                                                            End Try
                                                        End If
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
                                                        ElseIf (Not isDataTableOk(rs)) Then
                                                            '
                                                            ' ----- no result
                                                            '
                                                            Copy = "No Results"
                                                        ElseIf (rs.Rows.Count = 0) Then
                                                            '
                                                            ' ----- no result
                                                            '
                                                            Copy = "No Results"
                                                        Else
                                                            '
                                                            ' ----- print results
                                                            '
                                                            If rs.Rows.Count > 0 Then
                                                                If rs.Rows.Count = 1 And rs.Columns.Count = 1 Then
                                                                    Copy = cpCore.htmlDoc.html_GetFormInputText2("result", genericController.encodeText(something(0, 0)), , , , , True)
                                                                Else
                                                                    For Each dr As DataRow In rs.Rows
                                                                        '
                                                                        ' Build headers
                                                                        '
                                                                        FieldCount = dr.ItemArray.Count
                                                                        Copy = Copy & (cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;"">")
                                                                        Copy = Copy & (cr & vbTab & "<tr>")
                                                                        For Each dc As DataColumn In dr.ItemArray
                                                                            Copy = Copy & (cr & vbTab & vbTab & "<td class=""ccadminsmall"" style=""border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;"">" & dr(dc).ToString & "</td>")
                                                                        Next
                                                                        Copy = Copy & (cr & vbTab & "</tr>")
                                                                        '
                                                                        ' Build output table
                                                                        '
                                                                        Dim RowStart As String
                                                                        Dim RowEnd As String
                                                                        Dim ColumnStart As String
                                                                        Dim ColumnEnd As String
                                                                        RowStart = cr & vbTab & "<tr>"
                                                                        RowEnd = cr & vbTab & "</tr>"
                                                                        ColumnStart = cr & vbTab & vbTab & "<td class=""ccadminnormal"" style=""border-top:1px solid #444;border-left:1px solid #444;background-color:white;color:#444;padding:2px"">"
                                                                        ColumnEnd = "</td>"
                                                                        Dim RowPointer As Integer
                                                                        For RowPointer = 0 To RowMax
                                                                            Copy = Copy & (RowStart)
                                                                            Dim ColumnPointer As Integer
                                                                            For ColumnPointer = 0 To ColumnMax
                                                                                Dim CellData As Object
                                                                                CellData = something(ColumnPointer, RowPointer)
                                                                                If IsNull(CellData) Then
                                                                                    Copy = Copy & (ColumnStart & "[null]" & ColumnEnd)
                                                                                ElseIf IsNothing(CellData) Then
                                                                                    Copy = Copy & (ColumnStart & "[empty]" & ColumnEnd)
                                                                                ElseIf IsArray(CellData) Then
                                                                                    Copy = Copy & ColumnStart & "[array]"
                                                                                    'Dim Cnt As Integer
                                                                                    'Cnt = UBound(CellData)
                                                                                    'Dim Ptr As Integer
                                                                                    'For Ptr = 0 To Cnt - 1
                                                                                    '    Copy = Copy & ("<br>(" & Ptr & ")&nbsp;[" & CellData(Ptr) & "]")
                                                                                    'Next
                                                                                    'Copy = Copy & (ColumnEnd)
                                                                                ElseIf genericController.encodeText(CellData) = "" Then
                                                                                    Copy = Copy & (ColumnStart & "[empty]" & ColumnEnd)
                                                                                Else
                                                                                    Copy = Copy & (ColumnStart & cpCore.htmlDoc.html_EncodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
                                                                                End If
                                                                            Next
                                                                            Copy = Copy & (RowEnd)
                                                                        Next
                                                                        Copy = Copy & (cr & "</table>")

                                                                    Next
                                                                End If
                                                            End If
                                                        End If
                                                        Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                End Select
                                            Next
                                            Copy = Adminui.GetEditPanel(True, TabHeading, TabDescription, Adminui.EditTableOpen & TabCell.Text & Adminui.EditTableClose)
                                            If Copy <> "" Then
                                                Call cpCore.htmlDoc.main_AddLiveTabEntry(Replace(TabName, " ", "&nbsp;"), Copy, "ccAdminTab")
                                            End If
                                            'Content.Add( GetForm_Edit_AddTab(TabName, Copy, True))
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
                            'Content.Add( cpcore.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormMobileBrowserControl))
                            '
                            '
                            '
                            If TabCnt > 0 Then
                                Content.Add(cpCore.htmlDoc.main_GetLiveTabs())
                            End If
                        End If
                    End If
                End If
            End If
            '
            getFormContent = Adminui.GetBody(Name, ButtonList, "", True, True, Description, "", 0, Content.Text)
            Content = Nothing
            '
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError5("addon_execute_getFormContent", "trap", Err.Number, Err.Source, Err.Description, False)
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function getFormContent_decodeSelector(ByVal nothingObject As Object, ByVal SitePropertyName As String, ByVal SitePropertyValue As String, ByVal selector As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AdminClass.addon_execute_internal_getFormContent_decodeSelector")
            '
            Dim ExpandedSelector As String = ""
            Dim ignore As String = ""
            Dim OptionCaption As String
            Dim OptionValue As String
            Dim OptionValue_AddonEncoded As String
            Dim OptionPtr As Integer
            Dim OptionCnt As Integer
            Dim OptionValues() As String
            Dim OptionSuffix As String
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
            Dim FastString As coreFastStringClass
            Dim FieldValueInteger As Integer
            Dim FieldRequired As Boolean
            Dim FieldHelp As String
            Dim AuthoringStatusMessage As String
            Dim Delimiter As String
            Dim Copy As String
            '
            Dim FieldName As String
            '
            FastString = New coreFastStringClass
            '
            Call buildAddonOptionLists(ignore, ExpandedSelector, SitePropertyName & "=" & selector, SitePropertyName & "=" & SitePropertyValue, "0", True)
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
                                ' Create checkbox addon_execute_getFormContent_decodeSelector
                                '
                                If genericController.vbInstr(1, "," & LCaseOptionDefault & ",", "," & genericController.vbLCase(OptionValue) & ",") <> 0 Then
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ checked=""checked"">" & OptionCaption & "</div>"
                                Else
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                End If
                            Case "radio"
                                '
                                ' Create Radio addon_execute_getFormContent_decodeSelector
                                '
                                If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ checked=""checked"" >" & OptionCaption & "</div>"
                                Else
                                    getFormContent_decodeSelector = getFormContent_decodeSelector & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                End If
                            Case Else
                                '
                                ' Create select addon_execute_getFormContent_decodeSelector
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
                        ' Create Radio addon_execute_getFormContent_decodeSelector
                        '
                        'addon_execute_getFormContent_decodeSelector = "<div>" & genericController.vbReplace(addon_execute_getFormContent_decodeSelector, "><", "></div><div><") & "</div>"
                    Case Else
                        '
                        ' Create select addon_execute_getFormContent_decodeSelector
                        '
                        getFormContent_decodeSelector = "<select name=""" & SitePropertyName & """>" & getFormContent_decodeSelector & "</select>"
                End Select
            Else
                '
                ' Create Text addon_execute_getFormContent_decodeSelector
                '

                selector = genericController.decodeNvaArgument(selector)
                getFormContent_decodeSelector = cpCore.htmlDoc.html_GetFormInputText2(SitePropertyName, selector, 1, 20)
            End If

            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call cpCore.handleLegacyError7("addon_execute_getFormContent_decodeSelector", "trap")
        End Function
        '
        ' ================================================================================================================
        '   Execute a script
        '   returns the results
        '
        '       - cp argument should be set during csv_OpenConnection3, not passed in here as nothingObject
        ' ================================================================================================================
        '
        Private Function executeScript(ByVal Language As String, ByVal Code As String, ByVal EntryPoint As String, ByVal cmcObj As coreClass, ByVal nothingObject As Object, ByRef return_AddonErrorMessage As String) As String
            Dim ScriptName As String
            Dim FirstLine As String
            Dim Pos As Integer
            Dim EmptyArray(0) As String
            '
            If EntryPoint <> "" Then
                ScriptName = "unnamed script with method [" & EntryPoint & "] and length [" & Len(Code) & "]"
            Else
                FirstLine = Code
                FirstLine = genericController.vbReplace(FirstLine, vbTab, "")
                Pos = genericController.vbInstr(1, FirstLine, vbCrLf)
                If (Pos <= 0) Or (Pos > 50) Then
                    FirstLine = Left(FirstLine, 50)
                Else
                    FirstLine = Left(FirstLine, Pos - 1)
                End If
                ScriptName = "unnamed script with length [" & Len(Code) & "] starting with [" & FirstLine & "]"
            End If
            Return executeScript3(Language, Code, EntryPoint, cmcObj, nothingObject, return_AddonErrorMessage, 60000, ScriptName, 0, EmptyArray, EmptyArray)
        End Function
        ''
        '' ================================================================================================================
        ''   Execute a script
        ''   returns the results
        '' ================================================================================================================
        ''
        'Private Function addon_execute_executeScript2(ByVal Language As String, ByVal Code As String, ByVal EntryPoint As String, ByVal cmcObj As coreClass, ByVal nothingObject As Object, ByRef return_AddonErrorMessage As String, ByVal ScriptingTimeout As Integer, ByVal ScriptName As String) As String
        '    Dim EmptyArray(0) As String
        '    addon_execute_executeScript2 = addon_execute_executeScript3(Language, Code, EntryPoint, cmcObj, nothingObject, return_AddonErrorMessage, ScriptingTimeout, ScriptName, 0, EmptyArray, EmptyArray)
        'End Function
        '
        ' ================================================================================================================
        '   conversion to 2005 - pass 2
        ' ================================================================================================================
        '
        Private Function executeScript3(ByVal Language As String, ByVal Code As String, ByVal EntryPoint As String, ByVal nothingObject As Object, ByVal nothingObject2 As Object, ByRef return_AddonErrorMessage As String, ByVal ScriptingTimeout As Integer, ByVal ScriptName As String, ByVal ReplaceCnt As Integer, ByVal ReplaceNames() As String, ByVal ReplaceValues() As String) As String
            Return executeScript4(Language, Code, EntryPoint, return_AddonErrorMessage, ScriptingTimeout, ScriptName, ReplaceCnt, ReplaceNames, ReplaceValues)
        End Function
        '
        ' ================================================================================================================
        ''' <summary>
        ''' execute the script section of addons. Must be 32-bit. 
        ''' </summary>
        ''' <param name="Language"></param>
        ''' <param name="Code"></param>
        ''' <param name="EntryPoint"></param>
        ''' <param name="return_errorMessage"></param>
        ''' <param name="ScriptingTimeout"></param>
        ''' <param name="ScriptName"></param>
        ''' <param name="ReplaceCnt"></param>
        ''' <param name="ReplaceNames"></param>
        ''' <param name="ReplaceValues"></param>
        ''' <returns></returns>
        ''' <remarks>long run, use either csscript.net, or use .net tools to build compile/run funtion</remarks>
        Private Function executeScript4(ByVal Language As String, ByVal Code As String, ByVal EntryPoint As String, ByRef return_errorMessage As String, ByVal ScriptingTimeout As Integer, ByVal ScriptName As String, ByVal ReplaceCnt As Integer, ByVal ReplaceNames() As String, ByVal ReplaceValues() As String) As String
            Dim returnText As String = ""
            Try
                Dim Lines() As String
                Dim sc As New MSScriptControl.ScriptControl
                Dim Args() As String
                Dim Pos As Integer
                Dim EntryPointArgs As String
                Dim EntryPointName As String
                Dim Ptr As Integer
                Dim ReplaceName As String
                Dim ReplaceValue As String
                Dim WorkingCode As String
                Dim WorkingEntryPoint As String
                '
                'Add a COM reference of "Microsoft Script Control 1.0" to your project.
                'Use this code
                'MSScriptControl.ScriptControl script = new MSScriptControl.ScriptControl();
                'script.Language = "VBScript";
                'script.AddObject("Repository", connectTocpcore.db.GetRepository);
                'addobject -AddS
                ' adds activex
                '
                WorkingEntryPoint = EntryPoint
                WorkingCode = Code
                If ReplaceCnt > 0 Then
                    For Ptr = 0 To ReplaceCnt - 1
                        ReplaceName = "$" & ReplaceNames(Ptr) & "$"
                        ReplaceValue = ReplaceValues(Ptr)
                        WorkingEntryPoint = genericController.vbReplace(WorkingEntryPoint, ReplaceName, ReplaceValue, 1, 99, vbTextCompare)
                        WorkingCode = genericController.vbReplace(WorkingCode, ReplaceName, ReplaceValue, 1, 99, vbTextCompare)
                    Next
                End If
                EntryPointName = WorkingEntryPoint
                Pos = genericController.vbInstr(1, EntryPointName, "(")
                If Pos = 0 Then
                    Pos = genericController.vbInstr(1, EntryPointName, " ")
                End If
                If Pos > 1 Then
                    EntryPointArgs = Trim(Mid(EntryPointName, Pos))
                    EntryPointName = Trim(Left(EntryPointName, Pos - 1))
                    If (Mid(EntryPointArgs, 1, 1) = "(") And (Mid(EntryPointArgs, Len(EntryPointArgs), 1) = ")") Then
                        EntryPointArgs = Mid(EntryPointArgs, 2, Len(EntryPointArgs) - 2)
                    End If
                    Args = SplitDelimited(EntryPointArgs, ",")
                End If
                '
                ' the only createObject allowed -- because there is no modern versin of 
                '
                'sc = CreateObject("ScriptControl")
                Try
                    sc.AllowUI = False
                    sc.Timeout = ScriptingTimeout
                    If Language <> "" Then
                        sc.Language = Language
                    Else
                        sc.Language = "VBScript"
                    End If
                    Call sc.AddCode(WorkingCode)
                Catch ex As Exception
                    return_errorMessage = "Error configuring scripting system"
                    If sc.Error.Number <> 0 Then
                        With sc.Error
                            return_errorMessage &= ", #" & .Number & ", " & .Description & ", line " & .Line & ", character " & .Column
                            If .Line <> 0 Then
                                Lines = Split(WorkingCode, vbCrLf)
                                If UBound(Lines) >= .Line Then
                                    return_errorMessage = return_errorMessage & ", code [" & Lines(.Line - 1) & "]"
                                End If
                            End If
                        End With
                    Else
                        return_errorMessage &= ", no scripting error"
                    End If
                    cpCore.handleExceptionAndRethrow(ex, return_errorMessage)
                End Try
                If String.IsNullOrEmpty(return_errorMessage) Then
                    If True Then
                        If True Then
                            If True Then
                                If True Then
                                    Try
                                        Call sc.AddObject("cp", cpCore.cp_forAddonExecutionOnly)
                                    Catch ex As Exception
                                        '
                                        ' Error adding cp object
                                        '
                                        return_errorMessage = "Error adding cp object to script environment"
                                        If sc.Error.Number <> 0 Then
                                            With sc.Error
                                                return_errorMessage = return_errorMessage & ", #" & .Number & ", " & .Description & ", line " & .Line & ", character " & .Column
                                                If .Line <> 0 Then
                                                    Lines = Split(WorkingCode, vbCrLf)
                                                    If UBound(Lines) >= .Line Then
                                                        return_errorMessage = return_errorMessage & ", code [" & Lines(.Line - 1) & "]"
                                                    End If
                                                End If
                                            End With
                                        Else
                                            return_errorMessage &= ", no scripting error"
                                        End If
                                        cpCore.handleExceptionAndRethrow(ex, return_errorMessage)
                                    End Try
                                    If String.IsNullOrEmpty(return_errorMessage) Then
                                        '
                                        If EntryPointName = "" Then
                                            If sc.Procedures.Count > 0 Then
                                                EntryPointName = sc.Procedures(1).Name
                                            End If
                                        End If
                                        Try
                                            If EntryPointArgs = "" Then
                                                returnText = genericController.encodeText(sc.Run(EntryPointName))

                                            Else
                                                Select Case UBound(Args)
                                                    Case 0
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0)))
                                                    Case 1
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1)))
                                                    Case 2
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2)))
                                                    Case 3
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3)))
                                                    Case 4
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4)))
                                                    Case 5
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4), Args(5)))
                                                    Case 6
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4), Args(5), Args(6)))
                                                    Case 7
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4), Args(5), Args(6), Args(7)))
                                                    Case 8
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4), Args(5), Args(6), Args(7), Args(8)))
                                                    Case 9
                                                        returnText = genericController.encodeText(sc.Run(EntryPointName, Args(0), Args(1), Args(2), Args(3), Args(4), Args(5), Args(6), Args(7), Args(8), Args(9)))
                                                    Case Else
                                                        Call cpCore.handleLegacyError6("csv_ExecuteScript4", "Scripting only supports 10 arguments.")
                                                End Select
                                            End If
                                        Catch ex As Exception
                                            return_errorMessage = "Error executing script"
                                            If sc.Error.Number <> 0 Then
                                                With sc.Error
                                                    return_errorMessage = return_errorMessage & ", #" & .Number & ", " & .Description & ", line " & .Line & ", character " & .Column
                                                    If .Line <> 0 Then
                                                        Lines = Split(WorkingCode, vbCrLf)
                                                        If UBound(Lines) >= .Line Then
                                                            return_errorMessage = return_errorMessage & ", code [" & Lines(.Line - 1) & "]"
                                                        End If
                                                    End If
                                                End With
                                            Else
                                                return_errorMessage = return_errorMessage & ", " & GetErrString()
                                            End If
                                            cpCore.handleExceptionAndRethrow(ex, return_errorMessage)
                                        End Try
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnText
        End Function
        '
        '
        '
        Private Function executeAssembly(ByVal addonId As Integer, ByVal AddonCaption As String, ByVal AssemblyClassFullName As String, ByVal CollectionGuid As String, ByVal nothingObject As Object, ByRef return_ErrorMessageForAdmin As String) As String
            Dim result As String = ""
            Try
                Dim AddonFound As Boolean = False
                Dim addonAppRootPath As String
                Dim AddonPath As String
                Dim addonInstall As addonInstallClass
                Dim AddonVersionPath As String
                Dim commonAssemblyPath As String
                Dim appAddonPath As String
                '
                ' first try debug folder -- cclibCommonAssemblies
                '
                commonAssemblyPath = cpCore.programDataFiles.rootLocalPath & "AddonAssemblyBypass\"
                If Not IO.Directory.Exists(commonAssemblyPath) Then
                    IO.Directory.CreateDirectory(commonAssemblyPath)
                Else
                    result = executeAssembly_byFilePath(addonId, AddonCaption, commonAssemblyPath, AssemblyClassFullName, True, AddonFound, return_ErrorMessageForAdmin)
                End If
                If Not AddonFound Then
                    '
                    ' try app /bin folder
                    '
                    addonAppRootPath = cpCore.privateFiles.joinPath(cpCore.appRootFiles.rootLocalPath, "bin\")
                    result = executeAssembly_byFilePath(addonId, AddonCaption, addonAppRootPath, AssemblyClassFullName, True, AddonFound, return_ErrorMessageForAdmin)
                    If Not AddonFound Then
                        '
                        ' legacy mode, consider eliminating this and storing addon binaries in apps /bin folder
                        '
                        AddonVersionPath = ""
                        If String.IsNullOrEmpty(CollectionGuid) Then
                            Throw New ApplicationException("The assembly for addon [" & AddonCaption & "] could not be executed because it's collection has an invalid guid.")
                        Else
                            addonInstall = New addonInstallClass(cpCore)
                            Call addonInstall.GetCollectionConfig(CollectionGuid, AddonVersionPath, New Date(), "")
                            If (String.IsNullOrEmpty(AddonVersionPath)) Then
                                Throw New ApplicationException("The assembly for addon [" & AddonCaption & "] could not be executed because it's assembly could not be found in cclibCommonAssemblies, and no collection folder was found.")
                            Else
                                AddonPath = cpCore.privateFiles.joinPath(getPrivateFilesAddonPath(), AddonVersionPath)
                                appAddonPath = cpCore.privateFiles.joinPath(cpCore.privateFiles.rootLocalPath, AddonPath)
                                result = executeAssembly_byFilePath(addonId, AddonCaption, appAddonPath, AssemblyClassFullName, False, AddonFound, return_ErrorMessageForAdmin)
                                If (Not AddonFound) Then
                                    '
                                    ' assembly not found in addon path and in development path, if core collection, try in local /bin nm 
                                    '
                                    If (CollectionGuid <> CoreCollectionGuid) Then
                                        '
                                        ' assembly not found
                                        '
                                        Throw New ApplicationException("The addon [" & AddonCaption & "] could not be executed because it's assembly could not be found in the cluster's common assembly path [" & commonAssemblyPath & "], the apps binary folder [" & addonAppRootPath & "], or in the legacy collection folder [" & appAddonPath & "].")
                                    Else
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
                Throw ex
            End Try
            Return result
        End Function
        '
        '==================================================================================================
        '   This is the call from the COM csv code that executes a dot net addon from com.
        '   This is not in the CP BaseClass, because it is used by addons to call back into CP for
        '   services, and they should never call this.
        '==================================================================================================
        '
        Private Function executeAssembly_byFilePath(ByVal AddonID As Integer, ByVal AddonDisplayName As String, ByVal fullPath As String, ByVal typeFullName As String, ByVal IsDevAssembliesFolder As Boolean, ByRef AddonFound As Boolean, ByRef return_userErrorMessage As String) As String
            Dim returnValue As String = ""
            Try
                Dim objTypes() As Type = Nothing
                Dim filePtr As Integer
                Dim dllFilenames() As String
                Dim testAssembly As [Assembly] = Nothing
                Dim testAssemblyType As Type = Nothing
                Dim objInterface As Type = Nothing
                Dim AddonObj As AddonBaseClass
                'Dim addonObj As Object
                Dim IsClassFound As Boolean = False
                Dim TestFilePathname As String = ""
                Dim AddonReturnObj As Object
                Dim detailedErrorMessage As String = ""
                Dim testFileIsValidAssembly As Boolean
                Dim testAssemblyName As String
                '
                ' If not found in debug location (addon root folder), look in addon version folder provided
                '
                AddonFound = False
                '
                ' refactor -- add an argument byref dictionary cache, loaded as you go through the types in each dll. Next load, use the dictionary to locate the class faster. 
                '
                If IO.Directory.Exists(fullPath) Then
                    dllFilenames = IO.Directory.GetFileSystemEntries(fullPath, "*.dll")
                    If dllFilenames.Length > 0 Then
                        '
                        ' search the list for the correct assembly
                        '
                        filePtr = 0
                        Do
                            TestFilePathname = dllFilenames(filePtr)
                            If TestFilePathname.IndexOf("\xunit.runner") >= 0 Then
                                '
                                ' unknown issue with xunit DLL, skip for now
                                '
                            Else
                                testFileIsValidAssembly = True
                                Try
                                    '
                                    ' ##### consider using refectiononlyload first, then if it is right, do the loadfrom - so dependancies are not loaded.
                                    '
                                    testAssembly = System.Reflection.Assembly.LoadFrom(TestFilePathname)
                                    testAssemblyName = testAssembly.FullName
                                Catch ex As Exception
                                    testFileIsValidAssembly = False
                                End Try
                                Try
                                    If testFileIsValidAssembly Then
                                        '
                                        ' problem loading types, use try to debug
                                        '
                                        Try
                                            For Each testAssemblyType In testAssembly.GetTypes
                                                '
                                                ' Loop through each type in the Assembly looking for our typename, public, and non-abstract
                                                '
                                                If (testAssemblyType.FullName.Trim.ToLower = typeFullName.Trim.ToLower) _
                                                And (testAssemblyType.IsPublic = True) _
                                                And (Not ((testAssemblyType.Attributes And TypeAttributes.Abstract) = TypeAttributes.Abstract)) _
                                                And Not (testAssemblyType.BaseType Is Nothing) _
                                                Then
                                                    If (testAssemblyType.BaseType.FullName.ToLower = "addonbaseclass") Or (testAssemblyType.BaseType.FullName.ToLower = "contensive.baseclasses.addonbaseclass") Then
                                                        '
                                                        ' This assembly matches the TypeFullName, use it
                                                        '
                                                        AddonFound = True
                                                        Try
                                                            '
                                                            ' Create the object from the Assembly
                                                            '
                                                            AddonObj = DirectCast(testAssembly.CreateInstance(testAssemblyType.FullName), AddonBaseClass)
                                                            Try
                                                                '
                                                                ' Call Execute
                                                                '
                                                                AddonReturnObj = AddonObj.Execute(cpCore.cp_forAddonExecutionOnly)
                                                                If Not (AddonReturnObj Is Nothing) Then
                                                                    Select Case AddonReturnObj.GetType().ToString
                                                                        Case "System.Object[,]"
                                                                '
                                                                '   a 2-D Array of objects
                                                                '   each cell can contain 
                                                                '   return array for internal use constructing data/layout merge
                                                                '   return xml as dataset to another computer
                                                                '   return json as dataset for browser
                                                                '
                                                                        Case "System.String[,]"
                                                                            '
                                                                            '   return array for internal use constructing data/layout merge
                                                                            '   return xml as dataset to another computer
                                                                            '   return json as dataset for browser
                                                                            '
                                                                        Case Else
                                                                            returnValue = AddonReturnObj.ToString
                                                                    End Select
                                                                End If
                                                            Catch Ex As Exception
                                                                '
                                                                ' Error in the addon
                                                                '
                                                                return_userErrorMessage = "There was an error executing the addon Dot Net assembly."
                                                                detailedErrorMessage = "There was an error in the addon [" & AddonDisplayName & "]. It could not be executed because there was an error in the addon assembly [" & TestFilePathname & "], in class [" & testAssemblyType.FullName.Trim.ToLower & "]. The error was [" & Ex.ToString() & "]"
                                                                cpCore.handleExceptionAndContinue(Ex, detailedErrorMessage)
                                                                'Throw New ApplicationException(detailedErrorMessage)
                                                            End Try
                                                        Catch Ex As Exception
                                                            return_userErrorMessage = "There was an error initializing the addon's Dot Net DLL."
                                                            detailedErrorMessage = AddonDisplayName & " could not be executed because there was an error creating an object from the assembly, DLL [" & testAssemblyType.FullName & "]. The error was [" & Ex.ToString() & "]"
                                                            Throw New ApplicationException(detailedErrorMessage)
                                                        End Try
                                                        '
                                                        ' addon was found, no need to look for more
                                                        '
                                                        Exit For
                                                    End If
                                                End If
                                            Next
                                        Catch ex As ReflectionTypeLoadException
                                            '
                                            ' exceptin thrown out of application bin folder when xunit library included -- ignore
                                            '
                                        Catch ex As Exception
                                            '
                                            ' problem loading types
                                            '
                                            detailedErrorMessage = "While locating assembly for addon [" & AddonDisplayName & "], there was an error loading types for assembly [" & testAssemblyType.FullName & "]. This assembly was skipped and should be removed from the folder [" & fullPath & "]"
                                            Throw New ApplicationException(detailedErrorMessage)
                                        End Try
                                    End If
                                Catch ex As Reflection.ReflectionTypeLoadException
                                    return_userErrorMessage = "The addon's Dot Net DLL does not appear to be valid [" & TestFilePathname & "]."
                                    detailedErrorMessage = "A load exception occured for addon [" & AddonDisplayName & "], DLL [" & testAssemblyType.FullName & "]. The error was [" & ex.ToString() & "] Any internal exception follow:"
                                    objTypes = ex.Types
                                    For Each exLoader As Exception In ex.LoaderExceptions
                                        detailedErrorMessage &= vbCrLf & "--LoaderExceptions: " & exLoader.Message
                                    Next
                                    Throw New ApplicationException(detailedErrorMessage)
                                Catch ex As Exception
                                    '
                                    ' ignore these errors
                                    '
                                    return_userErrorMessage = "There was an unknown error in the addon's Dot Net DLL [" & AddonDisplayName & "]."
                                    detailedErrorMessage = "A non-load exception occured while loading the addon [" & AddonDisplayName & "], DLL [" & testAssemblyType.FullName & "]. The error was [" & ex.ToString() & "]."
                                    cpCore.handleExceptionAndContinue(New ApplicationException(detailedErrorMessage))
                                End Try
                            End If
                            filePtr += 1
                        Loop While (Not AddonFound) And (filePtr < dllFilenames.Length)
                    End If
                End If
            Catch ex As Exception
                '
                ' -- this exception should interrupt the caller
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnValue
        End Function
        ''
        '' 
        ''
        'Public Function csv_ExecuteActiveX(ByVal ProgramID As String, ByVal AddonCaption As String, ByVal OptionString_ForObjectCall As String, ByVal OptionStringForDisplay As String, ByRef return_AddonErrorMessage As String) As String
        '    Dim exMsg As String = "activex addons [" & ProgramID & "] are no longer supported"
        '    handleException(New ApplicationException(exMsg))
        '    Return exMsg
        'End Function
        '
        '====================================================================================================================
        '   Execte an Addon as a process
        '
        '   OptionString
        '       can be & delimited or crlf delimited
        '       must be addonencoded with call encodeNvaArgument
        '
        '   nothingObject
        '       cp should be set during csv_OpenConnection3 -- do not pass it around in the arguments
        '
        '   WaitForReturn
        '       if true, this routine calls the addon
        '       if false, the server is called remotely, which starts a cccmd process, gets the command and calls this routine with true
        '====================================================================================================================
        '
        Public Function executeAddonAsProcess(ByVal AddonIDGuidOrName As String, Optional ByVal OptionString As String = "") As String
            On Error GoTo ErrorTrap 'Const Tn = "ExecuteAddonAsProcess" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim StatusOK As Boolean
            Dim ErrNumber As Integer
            Dim ErrSource As String
            Dim ErrDescription As String
            Dim TestName As String
            Dim TestValue As String
            Dim PairPtr As Integer
            Dim LCaseTestName As String
            Dim ScriptingTimeout As Integer
            Dim ACInstanceID As String
            Dim AddonOptionConstructor As String
            Dim AddonOptionExpandedConstructor As String
            Dim OptionString_ForObjectCall As String
            Dim CollectionGuid As String
            Dim IgnoreErrorMessage As String
            Dim DefaultWrapperID As Integer
            Dim WrapperID As Integer
            Dim OptionPair() As String
            Dim OptionsForCPVars() As NameValuePrivateType
            Dim OptionPtr As Integer
            Dim Options() As String
            Dim CodeFilename As String
            Dim CSRules As Integer
            Dim SQL As String
            Dim ScriptingLanguage As String
            Dim scriptinglanguageid As Integer
            Dim ScriptingEntryPoint As String
            Dim HTMLContent As String
            Dim AddonGuid As String
            Dim OptionCnt As Integer
            Dim OptionsForCPVars_Cnt As Integer

            Dim Ptr As Integer
            Dim hint As String
            Dim Criteria As String
            Dim CS As Integer
            Dim AddonName As String
            Dim ProgramID As String
            Dim ArgumentList As String
            Dim ScriptingCode As String
            Dim DotNetClassFullName As String
            Dim WorkingAddonOptionString As String
            Dim cmdQueryString As String
            'Dim runAtServer As runAtServerClass
            Dim addonId As Integer
            'Dim AddonGuidOrName As String
            Dim ProcessStartTick As Integer
            Dim ReplaceCnt As Integer
            Dim ReplaceNames() As String
            Dim ReplaceValues() As String
            Dim SQLName As String
            Dim addonPtr As Integer
            ' Dim taskScheduler As New taskSchedulerServiceClass()
            '
            'hint = "csv_executeAddonAsProcess, enter"
            '
            ProcessStartTick = GetTickCount
            addonPtr = cpCore.addonCache.getPtr(AddonIDGuidOrName)
            If addonPtr >= 0 Then
                addonId = genericController.EncodeInteger(cpCore.addonCache.localCache.addonList(addonPtr.ToString).addonCache_Id)
                AddonName = genericController.encodeText(cpCore.addonCache.localCache.addonList(addonPtr.ToString).addonCache_name)
                'hint = hint & ",020 addonname=[" & AddonName & "] addonid=[" & addonId & "]"
            End If
            '
            '-----------------------------------------------------------------
            '   Add to background process queue
            '-----------------------------------------------------------------
            '
            'hint = hint & ",030"
            logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "start: add process to background cmd queue, addon [" & AddonName & "/" & addonId & "], optionstring [" & OptionString & "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Err.Number, Err.Source, Err.Description, False, True, "", "process", "")
            '
            ' runAtServer = New runAtServerClass(Me)
            ' must nva encode because that is what the server-execute command expects
            cmdQueryString = "" _
                    & "appname=" & encodeNvaArgument(EncodeRequestVariable(cpCore.serverConfig.appConfig.name)) _
                    & "&AddonID=" & CStr(addonId) _
                    & "&OptionString=" & encodeNvaArgument(EncodeRequestVariable(OptionString))
            'hint = hint & ",035"
            Dim taskScheduler As New coreTaskSchedulerServiceClass()
            Dim cmdDetail As New cmdDetailClass
            cmdDetail.addonId = addonId
            cmdDetail.addonName = AddonName
            cmdDetail.docProperties = taskScheduler.convertAddonArgumentstoDocPropertiesList(cpCore, cmdQueryString)
            Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
            'Call runAtServer.executeCmd("RunProcess", cmdQueryString)
            '
            logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "end: add process to background cmd queue, addon [" & AddonName & "/" & addonId & "], optionstring [" & OptionString & "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Err.Number, Err.Source, Err.Description, False, True, "", "process", "")
            '
            Exit Function
ErrorTrap:
            ErrNumber = Err.Number
            ErrSource = Err.Source
            ErrDescription = Err.Description
            Call Err.Clear()
            logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "errortrap exit(" & (GetTickCount - ProcessStartTick) & " msec): execute now, addon [" & AddonIDGuidOrName & "], optionstring [" & OptionString & "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Err.Number, Err.Source, Err.Description, False, True, "", "process", "")
            Call cpCore.handleLegacyError4(ErrNumber, ErrSource, ErrDescription, "unknownMethodNameLegacyCall" & ", hint=" & hint, True)
        End Function
        '
        '=============================================================================================================
        '   cpcore.main_Get Addon Content
        ' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        '=============================================================================================================
        '
        Public Function execute_legacy5(ByVal addonId As Integer, ByVal AddonName As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String, ByVal ACInstanceID As Integer) As String
            Dim AddonStatusOK As Boolean
            execute_legacy5 = execute_legacy2(addonId, AddonName, Option_String, Context, ContentName, RecordID, FieldName, CStr(ACInstanceID), False, 0, "", AddonStatusOK, Nothing)
        End Function
        '
        '====================================================================================================
        ' Public Interface
        ' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        '====================================================================================================
        '
        Public Function execute_legacy1(ByVal addonId As Integer, ByVal AddonNameOrGuid As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal HostContentName As String, ByVal HostRecordID As Integer, ByVal HostFieldName As String, ByVal ACInstanceID As String, ByVal DefaultWrapperID As Integer) As String
            Dim AddonStatusOK As Boolean
            Dim workingContext As CPUtilsBaseClass.addonContext
            '
            workingContext = Context
            If workingContext = 0 Then
                workingContext = CPUtilsBaseClass.addonContext.ContextPage
            End If
            execute_legacy1 = execute_legacy2(addonId, AddonNameOrGuid, Option_String, workingContext, HostContentName, HostRecordID, HostFieldName, ACInstanceID, False, DefaultWrapperID, "", AddonStatusOK, Nothing)
        End Function
        '
        '====================================================================================================
        ' Public Interface to support AsProcess
        '   Programmatic calls to executeAddon would not require Context, HostContent, etc because the host would be an add-on, and the
        '   addon has control or settings, not the administrator
        ' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        '====================================================================================================
        '
        Public Function execute_legacy3(ByVal AddonIDGuidOrName As String, Optional ByVal Option_String As String = "", Optional ByVal WrapperID As Integer = 0, Optional ByVal nothingObject As Object = Nothing) As String
            Dim AddonStatusOK As Boolean
            If genericController.vbIsNumeric(AddonIDGuidOrName) Then
                execute_legacy3 = execute_legacy2(EncodeInteger(AddonIDGuidOrName), "", Option_String, CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, WrapperID, "", AddonStatusOK, nothingObject)
            Else
                execute_legacy3 = execute_legacy2(0, AddonIDGuidOrName, Option_String, CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, WrapperID, "", AddonStatusOK, nothingObject)
            End If
        End Function
        '
        ' Public Interface to support AsProcess
        '
        Public Function execute_legacy4(ByVal AddonIDGuidOrName As String, Optional ByVal Option_String As String = "", Optional ByVal Context As CPUtilsBaseClass.addonContext = CPUtilsBaseClass.addonContext.ContextPage, Optional ByVal nothingObject As Object = Nothing) As String
            Dim AddonStatusOK As Boolean
            Dim workingContext As CPUtilsBaseClass.addonContext
            '
            workingContext = Context
            If workingContext = 0 Then
                workingContext = CPUtilsBaseClass.addonContext.ContextPage
            End If
            If genericController.vbIsNumeric(AddonIDGuidOrName) Then
                execute_legacy4 = execute_legacy2(EncodeInteger(AddonIDGuidOrName), "", Option_String, workingContext, "", 0, "", "", False, 0, "", AddonStatusOK, nothingObject)
            Else
                execute_legacy4 = execute_legacy2(0, AddonIDGuidOrName, Option_String, workingContext, "", 0, "", "", False, 0, "", AddonStatusOK, nothingObject)
            End If
        End Function
        ''
        ''=============================================================================================================
        ''   Run Add-on as process
        '' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        ''=============================================================================================================
        ''
        'Public Function executeAddonAsProcess_legacy1(ByVal AddonIDGuidOrName As String, Optional ByVal Option_String As String = "", Optional ByVal nothingObject As Object = Nothing, Optional ByVal WaitForResults As Boolean = False) As String
        '    '
        '    executeAddonAsProcess_legacy1 = executeAddonAsProcess(AddonIDGuidOrName, Option_String, nothingObject, WaitForResults)
        '    '
        'End Function
        '
        '=============================================================================================================
        '   cpcore.main_Get Addon Content - internal (to support include add-ons)
        ' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        '=============================================================================================================
        '
        Public Function execute_legacy2(ByVal addonId As Integer, ByVal AddonNameOrGuid As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal HostContentName As String, ByVal HostRecordID As Integer, ByVal HostFieldName As String, ByVal ACInstanceID As String, ByVal IsIncludeAddon As Boolean, ByVal DefaultWrapperID As Integer, ByVal ignore_TemplateCaseOnly_PageContent As String, ByRef return_StatusOK As Boolean, ByVal nothingObject As Object, Optional ByVal AddonInUseIdList As String = "") As String
            execute_legacy2 = execute(addonId, AddonNameOrGuid, Option_String, Context, HostContentName, HostRecordID, HostFieldName, ACInstanceID, IsIncludeAddon, DefaultWrapperID, ignore_TemplateCaseOnly_PageContent, return_StatusOK, nothingObject, AddonInUseIdList, Nothing, cpCore.htmlDoc.main_page_IncludedAddonIDList, cpCore.authContext.authContextUser.id, cpCore.authContext.isAuthenticated)
        End Function
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
            Dim LocalCode As String
            Dim CopyHeader As String
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            '
            If cpCore.authContext.isAuthenticated() And ((ACInstanceID = "-2") Or (ACInstanceID = "-1") Or (ACInstanceID = "0") Or (RecordID <> 0)) Then
                If cpCore.authContext.isEditingAnything(cpCore) Then
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">Options for this instance of " & AddonName & "</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
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
                    ElseIf (Context <> CPUtilsBaseClass.addonContext.ContextAdmin) And (cpCore.siteProperties.allowWorkflowAuthoring And Not cpCore.visitProperty.getBoolean("AllowWorkflowRendering")) Then
                        '
                        ' workflow with no rendering (or within admin site)
                        '
                        CopyContent = "With Workflow editing enabled, you can not edit Add-on settings for live records. To make changes to the editable version of this page, turn on Render Workflow Authoring Changes and Advanced Edit together."
                        CopyContent = "<div style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & CopyContent & "</div>"
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
                                            & cpCore.htmlDoc.html_GetFormInputText2(OptionName, OptionDefault, 1, 20) _
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
                                FormInput = cpCore.htmlDoc.html_GetFormInputText2(OptionName, OptionSelector, 1, 20)
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
                            & cpCore.htmlDoc.html_GetFormInputHidden("Type", FormTypeAddonSettingsEditor) _
                            & cpCore.htmlDoc.html_GetFormInputHidden("ContentName", ContentName) _
                            & cpCore.htmlDoc.html_GetFormInputHidden("RecordID", RecordID) _
                            & cpCore.htmlDoc.html_GetFormInputHidden("FieldName", FieldName) _
                            & cpCore.htmlDoc.html_GetFormInputHidden("ACInstanceID", ACInstanceID)
                    End If
                    '
                    BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "',this);return false;"""
                    QueryString = cpCore.web_RefreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & cpCore.htmlDoc.html_GetUploadFormStart() _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccButtonCon"">" & cpCore.htmlDoc.html_GetFormButton("Update", "HelpBubbleButton") & "</td></tr>" _
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
                    If cpCore.htmlDoc.htmlDoc_HelpCodeCount >= cpCore.htmlDoc.htmlDoc_HelpCodeSize Then
                        cpCore.htmlDoc.htmlDoc_HelpCodeSize = cpCore.htmlDoc.htmlDoc_HelpCodeSize + 10
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = LocalCode
                    cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = AddonName
                    cpCore.htmlDoc.htmlDoc_HelpCodeCount = cpCore.htmlDoc.htmlDoc_HelpCodeCount + 1
                    '
                    If cpCore.htmlDoc.htmlDoc_HelpDialogCnt = 0 Then
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpDialogCnt = cpCore.htmlDoc.htmlDoc_HelpDialogCnt + 1
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call cpCore.handleLegacyError18("addon_execute_GetInstanceBubble")
        End Function
        '
        '===============================================================================================================================================
        '   cpcore.main_Get Addon Styles Bubble Editor
        '===============================================================================================================================================
        '
        Public Function getAddonStylesBubble(ByVal addonId As Integer, ByRef return_DialogList As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("addon_execute_GetAddonStylesBubble")
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
            Dim LocalCode As String
            Dim CopyHeader As String
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim CS As Integer
            Dim AddonName As String
            '
            If cpCore.authContext.isAuthenticated() And True Then
                If cpCore.authContext.isEditingAnything(cpCore) Then
                    CS = cpCore.csOpen(cnAddons, addonId)
                    If cpCore.db.cs_ok(CS) Then
                        AddonName = cpCore.db.cs_getText(CS, "name")
                        StyleSheet = cpCore.db.cs_get(CS, "CustomStylesFilename")
                        DefaultStylesheet = cpCore.db.cs_get(CS, "StylesFilename")
                    End If
                    Call cpCore.db.cs_Close(CS)
                    '
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">Stylesheet for " & AddonName & "</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
                        & "</tr>" _
                        & "</table>" _
                        & "</div>"
                    CopyContent = "" _
                        & "" _
                        & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                        & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccContentCon ccAdminSmall"">These stylesheets will be added to all pages that include this add-on. The default stylesheet comes with the add-on, and can not be edited.</td></tr>" _
                        & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Custom Stylesheet</b>" & cpCore.htmlDoc.html_GetFormInputTextExpandable2("CustomStyles", StyleSheet, 10, "400px") & "</td></tr>"
                    'CopyContent = "" _
                    '    & cpcore.main_GetUploadFormStart() _
                    '    & "<table border=0 cellpadding=5 cellspacing=0 width=""100%"">" _
                    '    & "<tr><td><div style=""width:400px;background-color:transparent;"" class=""ccContentCon ccAdminSmall"">These stylesheets will be added to all pages that include this add-on. The default stylesheet comes with the add-on, and can not be edited.</div></td></tr>" _
                    '    & "<tr><td><div style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Custom Stylesheet</b></div>" & cpcore.main_GetFormInputTextExpandable2( "CustomStyles", StyleSheet, 10, "400px") & "</td></tr>"
                    If DefaultStylesheet = "" Then
                        CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>There are no default styles for this add-on.</td></tr>"
                    Else
                        CopyContent = CopyContent & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Default Stylesheet</b><br>" & cpCore.htmlDoc.html_GetFormInputTextExpandable2("DefaultStyles", DefaultStylesheet, 10, "400px", , , True) & "</td></tr>"
                    End If
                    CopyContent = "" _
                        & CopyContent _
                        & "</tr>" _
                        & "</table>" _
                        & cpCore.htmlDoc.html_GetFormInputHidden("Type", FormTypeAddonStyleEditor) _
                        & cpCore.htmlDoc.html_GetFormInputHidden("AddonID", addonId) _
                        & ""
                    '
                    BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "',this);return false;"""
                    QueryString = cpCore.web_RefreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    Dim Dialog As String

                    Dialog = Dialog _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & cpCore.htmlDoc.html_GetUploadFormStart() _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccButtonCon"">" & cpCore.htmlDoc.html_GetFormButton("Update", "HelpBubbleButton") & "</td></tr>" _
                        & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                        & "</table>" _
                        & "</form>" _
                        & "</div>"
                    return_DialogList = return_DialogList & Dialog
                    getAddonStylesBubble = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 target=""_blank""" & BubbleJS & ">" _
                        & GetIconSprite("", 0, "/ccLib/images/toolstyles.png", 22, 22, "Edit " & AddonName & " Stylesheets", "Edit " & AddonName & " Stylesheets", "", True, "") _
                        & "</a>"
                    If cpCore.htmlDoc.htmlDoc_HelpCodeCount >= cpCore.htmlDoc.htmlDoc_HelpCodeSize Then
                        cpCore.htmlDoc.htmlDoc_HelpCodeSize = cpCore.htmlDoc.htmlDoc_HelpCodeSize + 10
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = LocalCode
                    cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = AddonName
                    cpCore.htmlDoc.htmlDoc_HelpCodeCount = cpCore.htmlDoc.htmlDoc_HelpCodeCount + 1
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call cpCore.handleLegacyError18("addon_execute_GetAddonStylesBubble")
        End Function
        '
        '===============================================================================================================================================
        '   cpcore.main_Get inner HTML viewer Bubble
        '===============================================================================================================================================
        '

        Public Function getHelpBubble(ByVal addonId As Integer, ByVal helpCopy As String, ByVal CollectionID As Integer, ByRef return_DialogList As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("addon_execute_GetHelpBubble")
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
            Dim LocalCode As String
            Dim CopyHeader As String
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim CS As Integer
            Dim AddonName As String
            Dim StyleSN As Integer
            Dim InnerCopy As String
            Dim CollectionCopy As String
            '
            If cpCore.authContext.isAuthenticated() Then
                If cpCore.authContext.isEditingAnything(cpCore) Then
                    StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"))
                    cpCore.htmlDoc.html_HelpViewerButtonID = "HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount
                    InnerCopy = helpCopy
                    If InnerCopy = "" Then
                        InnerCopy = "<p style=""text-align:center"">No help is available for this add-on.</p>"
                    End If
                    '
                    If CollectionID <> 0 Then
                        CollectionCopy = cpCore.content_GetRecordName("Add-on Collections", CollectionID)
                        If CollectionCopy <> "" Then
                            CollectionCopy = "This add-on is a member of the " & CollectionCopy & " collection."
                        Else
                            CollectionID = 0
                        End If
                    End If
                    If CollectionID = 0 Then
                        CollectionCopy = "This add-on is not a member of any collection."
                    End If
                    CopyHeader = CopyHeader _
                        & "<div class=""ccHeaderCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 width=""100%"">" _
                        & "<tr>" _
                        & "<td align=left class=""bbLeft"">Help Viewer</td>" _
                        & "<td align=right class=""bbRight""><a href=""#"" onClick=""HelpBubbleOff('HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "');return false;""><img alt=""close"" src=""/ccLib/images/ClosexRev1313.gif"" width=13 height=13 border=0></a></td>" _
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
                    QueryString = cpCore.web_RefreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    'QueryString = genericController.ModifyQueryString(QueryString, RequestNameInterceptpage, "", False)
                    return_DialogList = return_DialogList _
                        & "<div class=""ccCon helpDialogCon"">" _
                        & "<table border=0 cellpadding=0 cellspacing=0 class=""ccBubbleCon"" id=""HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & """ style=""display:none;visibility:hidden;"">" _
                        & "<tr><td class=""ccHeaderCon"">" & CopyHeader & "</td></tr>" _
                        & "<tr><td class=""ccContentCon"">" & CopyContent & "</td></tr>" _
                        & "</table>" _
                        & "</div>"
                    BubbleJS = " onClick=""HelpBubbleOn( 'HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount & "',this);return false;"""
                    If cpCore.htmlDoc.htmlDoc_HelpCodeCount >= cpCore.htmlDoc.htmlDoc_HelpCodeSize Then
                        cpCore.htmlDoc.htmlDoc_HelpCodeSize = cpCore.htmlDoc.htmlDoc_HelpCodeSize + 10
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = LocalCode
                    cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = AddonName
                    cpCore.htmlDoc.htmlDoc_HelpCodeCount = cpCore.htmlDoc.htmlDoc_HelpCodeCount + 1
                    '
                    If cpCore.htmlDoc.htmlDoc_HelpDialogCnt = 0 Then
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpDialogCnt = cpCore.htmlDoc.htmlDoc_HelpDialogCnt + 1
                    'SiteStylesBubbleCache = "x"
                    'End If
                    getHelpBubble = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 tarGet=""_blank""" & BubbleJS & " >" _
                        & GetIconSprite("", 0, "/ccLib/images/toolhelp.png", 22, 22, "View help resources for this Add-on", "View help resources for this Add-on", "", True, "") _
                        & "</a>"
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call cpCore.handleLegacyError18("addon_execute_GetHelpBubble")
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
            Dim LocalCode As String
            Dim CopyHeader As String
            Dim CopyContent As String
            Dim BubbleJS As String
            Dim OptionSplit() As String
            Dim OptionName As String
            Dim OptionSelector As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim CS As Integer
            Dim AddonName As String
            Dim StyleSN As Integer
            Dim HTMLViewerBubbleID As String
            '
            If cpCore.authContext.isAuthenticated() Then
                If cpCore.authContext.isEditingAnything(cpCore) Then
                    StyleSN = genericController.EncodeInteger(cpCore.siteProperties.getText("StylesheetSerialNumber", "0"))
                    HTMLViewerBubbleID = "HelpBubble" & cpCore.htmlDoc.htmlDoc_HelpCodeCount
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
                        & "<tr><td style=""width:400px;background-color:transparent;"" class=""ccAdminSmall"">" & cpCore.htmlDoc.html_GetFormInputTextExpandable2("DefaultStyles", "", 10, "400px", HTMLViewerBubbleID & "_dst", , False) & "</td></tr>" _
                        & "</tr>" _
                        & "</table>" _
                        & ""
                    '
                    QueryString = cpCore.web_RefreshQueryString
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
                    If cpCore.htmlDoc.htmlDoc_HelpCodeCount >= cpCore.htmlDoc.htmlDoc_HelpCodeSize Then
                        cpCore.htmlDoc.htmlDoc_HelpCodeSize = cpCore.htmlDoc.htmlDoc_HelpCodeSize + 10
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                        ReDim Preserve cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeSize)
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpCodes(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = LocalCode
                    cpCore.htmlDoc.htmlDoc_HelpCaptions(cpCore.htmlDoc.htmlDoc_HelpCodeCount) = AddonName
                    cpCore.htmlDoc.htmlDoc_HelpCodeCount = cpCore.htmlDoc.htmlDoc_HelpCodeCount + 1
                    'SiteStylesBubbleCache = "x"
                    '
                    If cpCore.htmlDoc.htmlDoc_HelpDialogCnt = 0 Then
                        Call cpCore.htmlDoc.main_AddOnLoadJavascript2("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
                    End If
                    cpCore.htmlDoc.htmlDoc_HelpDialogCnt = cpCore.htmlDoc.htmlDoc_HelpDialogCnt + 1
                    getHTMLViewerBubble = "" _
                        & "&nbsp;<a href=""#"" tabindex=-1 target=""_blank""" & BubbleJS & " >" _
                        & GetIconSprite("", 0, "/ccLib/images/toolhtml.png", 22, 22, "View the source HTML produced by this Add-on", "View the source HTML produced by this Add-on", "", True, "") _
                        & "</A>"
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Call cpCore.handleLegacyError18("addon_execute_GetHTMLViewerBubble")
        End Function
        '
        '
        '
        Private Function getFormContent(ByVal FormXML As String, ByRef return_ExitRequest As Boolean) As String
            ''Dim th as integer : th = profileLogMethodEnter("addon_execute_GetFormContent")
            '
            Const LoginMode_None = 1
            Const LoginMode_AutoRecognize = 2
            Const LoginMode_AutoLogin = 3
            '
            Dim PageSize As Integer
            Dim FieldCount As Integer
            Dim RowMax As Integer
            Dim ColumnMax As Integer
            'Dim RecordField As Field
            Dim SQLPageSize As Integer
            'dim dt as datatable
            Dim ErrorNumber As Integer
            Dim ErrorDescription As String
            Dim dataArray As String(,)
            Dim RecordID As Integer
            'Dim XMLTools As New xmlToolsclass(me)
            Dim fieldfilename As String
            'Dim fs As New fileSystemClass
            Dim FieldDataSource As String
            Dim FieldSQL As String
            Dim LoginMode As Integer
            Dim Help As String
            Dim Content As New coreFastStringClass
            Dim Copy As String
            Dim Button As String
            Dim PageNotFoundPageID As String
            Dim Adminui As New coreAdminUIClass(cpCore)
            Dim ButtonList As String
            Dim AllowLinkAlias As Boolean
            'Dim AllowExternalLinksInChildList As Boolean
            Dim LinkForwardAutoInsert As Boolean
            Dim SectionLandingLink As String
            'Dim app.siteProperty_ServerPageDefault As String
            Dim LandingPageID As String
            Dim AllowAutoRecognize As Boolean
            Dim AllowMobileTemplates As Boolean
            '
            '
            '
            Dim Filename As String
            Dim NonEncodedLink As String
            Dim EncodedLink As String
            Dim VirtualFilePath As String
            Dim Option_String As String
            Dim TabName As String
            Dim TabDescription As String
            Dim TabHeading As String
            Dim TabCnt As Integer
            Dim TabCell As coreFastStringClass
            Dim FieldValue As String
            Dim FieldDescription As String
            Dim FieldDefaultValue As String
            Dim IsFound As Boolean
            Dim Name As String
            Dim Description As String
            Dim LoopPtr As Integer
            Dim XMLFile As String
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
                Exit Function
            ElseIf Not cpCore.authContext.isAuthenticatedAdmin(cpcore) Then
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
                                                            FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            FieldValue = cpCore.docProperties.getText(FieldName)
                                                            fieldType = cpCore.main_GetXMLAttribute(IsFound, TabNode, "type", "")
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
                                                                        cpCore.cdnFiles.saveUpload(FieldName, VirtualFilePath, Filename)
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
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            If Not FieldReadOnly Then
                                                                FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                FieldHTML = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "html", "false"))
                                                                If FieldHTML Then
                                                                    '
                                                                    ' treat html as active content for now.
                                                                    '
                                                                    FieldValue = cpCore.docProperties.getRenderedActiveContent(FieldName)
                                                                Else
                                                                    FieldValue = cpCore.docProperties.getText(FieldName)
                                                                End If

                                                                CS = cpCore.db.cs_open("Copy Content", "name=" & cpCore.db.encodeSQLText(FieldName), "ID")
                                                                If Not cpCore.db.cs_ok(CS) Then
                                                                    Call cpCore.db.cs_Close(CS)
                                                                    CS = cpCore.db.cs_insertRecord("Copy Content")
                                                                End If
                                                                If cpCore.db.cs_ok(CS) Then
                                                                    Call cpCore.db.cs_set(CS, "name", FieldName)
                                                                    '
                                                                    ' Set copy
                                                                    '
                                                                    Call cpCore.db.cs_set(CS, "copy", FieldValue)
                                                                    '
                                                                    ' delete duplicates
                                                                    '
                                                                    Call cpCore.db.cs_goNext(CS)
                                                                    Do While cpCore.db.cs_ok(CS)
                                                                        Call cpCore.DeleteCSRecord(CS)
                                                                        Call cpCore.db.cs_goNext(CS)
                                                                    Loop
                                                                End If
                                                                Call cpCore.db.cs_Close(CS)
                                                            End If

                                                        Case "filecontent"
                                                            '
                                                            ' A File Content block
                                                            '
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            If Not FieldReadOnly Then
                                                                FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                fieldfilename = cpCore.main_GetXMLAttribute(IsFound, TabNode, "filename", "")
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
                                Exit Function
                            End If
                            '
                            ' ----- Display Form
                            '
                            Content.Add(Adminui.EditTableOpen)
                            Name = cpCore.main_GetXMLAttribute(IsFound, Doc.DocumentElement, "name", "")
                            With Doc.DocumentElement
                                For Each SettingNode In .ChildNodes
                                    Select Case genericController.vbLCase(SettingNode.Name)
                                        Case "description"
                                            Description = SettingNode.InnerText
                                        Case "tab"
                                            TabCnt = TabCnt + 1
                                            TabName = cpCore.main_GetXMLAttribute(IsFound, SettingNode, "name", "")
                                            TabDescription = cpCore.main_GetXMLAttribute(IsFound, SettingNode, "description", "")
                                            TabHeading = cpCore.main_GetXMLAttribute(IsFound, SettingNode, "heading", "")
                                            TabCell = New coreFastStringClass
                                            For Each TabNode In SettingNode.ChildNodes
                                                Select Case genericController.vbLCase(TabNode.Name)
                                                    Case "heading"
                                                        '
                                                        ' Heading
                                                        '
                                                        FieldCaption = cpCore.main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        Call TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption))
                                                    Case "siteproperty"
                                                        '
                                                        ' Site property
                                                        '
                                                        FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        If FieldName <> "" Then
                                                            FieldCaption = cpCore.main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            If FieldCaption = "" Then
                                                                FieldCaption = FieldName
                                                            End If
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldHTML = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                            fieldType = cpCore.main_GetXMLAttribute(IsFound, TabNode, "type", "")
                                                            FieldSelector = cpCore.main_GetXMLAttribute(IsFound, TabNode, "selector", "")
                                                            FieldDescription = cpCore.main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            FieldAddon = cpCore.main_GetXMLAttribute(IsFound, TabNode, "EditorAddon", "")
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
                                                                Option_String = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                Copy = execute_legacy5(0, FieldAddon, Option_String, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
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
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "boolean"
                                                                        If FieldReadOnly Then
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                            Copy = genericController.vbReplace(Copy, ">", " disabled>")
                                                                            Copy = Copy & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName, genericController.EncodeBoolean(FieldValue))
                                                                        End If
                                                                    Case "float"
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "date"
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputDate(FieldName, FieldValue)
                                                                        End If
                                                                    Case "file", "imagefile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            If FieldValue = "" Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputFile(FieldName)
                                                                            Else
                                                                                NonEncodedLink = cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, FieldValue)
                                                                                EncodedLink = EncodeURL(NonEncodedLink)
                                                                                Dim FieldValuefilename As String = ""
                                                                                Dim FieldValuePath As String = ""
                                                                                cpCore.privateFiles.splitPathFilename(FieldValue, FieldValuePath, FieldValuefilename)
                                                                                Copy = "" _
                                                                                    & "<a href=""http://" & EncodedLink & """ target=""_blank"">[" & FieldValuefilename & "]</A>" _
                                                                                    & "&nbsp;&nbsp;&nbsp;Delete:&nbsp;" & cpCore.htmlDoc.html_GetFormInputCheckBox2(FieldName & ".DeleteFlag", False) _
                                                                                    & "&nbsp;&nbsp;&nbsp;Change:&nbsp;" & cpCore.htmlDoc.html_GetFormInputFile(FieldName)
                                                                            End If
                                                                        End If
                                                                        'Call s.Add("&nbsp;</span></nobr></td>")
                                                                    Case "currency"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            If FieldValue <> "" Then
                                                                                FieldValue = FormatCurrency(FieldValue)
                                                                            End If
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case "textfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            FieldValue = cpCore.cdnFiles.readFile(FieldValue)
                                                                            If FieldHTML Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                            End If
                                                                        End If
                                                                    Case "cssfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                        End If
                                                                    Case "xmlfile"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue, 5)
                                                                        End If
                                                                    Case "link"
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
                                                                        End If
                                                                    Case Else
                                                                        '
                                                                        ' text
                                                                        '
                                                                        If FieldReadOnly Then
                                                                            Copy = FieldValue & cpCore.htmlDoc.html_GetFormInputHidden(FieldName, FieldValue)
                                                                        Else
                                                                            If FieldHTML Then
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML(FieldName, FieldValue)
                                                                            Else
                                                                                Copy = cpCore.htmlDoc.html_GetFormInputText2(FieldName, FieldValue)
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
                                                        FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        If FieldName <> "" Then
                                                            FieldCaption = cpCore.main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            If FieldCaption = "" Then
                                                                FieldCaption = FieldName
                                                            End If
                                                            FieldReadOnly = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldDescription = cpCore.main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            FieldHTML = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                            '
                                                            CS = cpCore.db.cs_open("Copy Content", "Name=" & cpCore.EncodeSQLText(FieldName), "ID", , , , , "Copy")
                                                            If Not cpCore.db.cs_ok(CS) Then
                                                                Call cpCore.db.cs_Close(CS)
                                                                CS = cpCore.db.cs_insertRecord("Copy Content")
                                                                If cpCore.db.cs_ok(CS) Then
                                                                    RecordID = cpCore.db.cs_getInteger(CS, "ID")
                                                                    Call cpCore.db.cs_set(CS, "name", FieldName)
                                                                    Call cpCore.db.cs_set(CS, "copy", genericController.encodeText(TabNode.InnerText))
                                                                    Call cpCore.db.cs_save2(CS)
                                                                    Call cpCore.workflow.publishEdit("Copy Content", RecordID)
                                                                End If
                                                            End If
                                                            If cpCore.db.cs_ok(CS) Then
                                                                FieldValue = cpCore.db.cs_getText(CS, "copy")
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
                                                                Copy = cpCore.htmlDoc.html_GetFormInputHTML3(FieldName, FieldValue)
                                                                'Copy = cpcore.main_GetFormInputActiveContent( FieldName, FieldValue)
                                                            Else
                                                                '
                                                                ' Text edit
                                                                '
                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, FieldValue)
                                                            End If
                                                            Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                        End If
                                                    Case "filecontent"
                                                        '
                                                        ' Content from a flat file
                                                        '
                                                        FieldName = cpCore.main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                        FieldCaption = cpCore.main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        fieldfilename = cpCore.main_GetXMLAttribute(IsFound, TabNode, "filename", "")
                                                        FieldReadOnly = genericController.EncodeBoolean(cpCore.main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                        FieldDescription = cpCore.main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                        FieldDefaultValue = TabNode.InnerText
                                                        Copy = ""
                                                        If fieldfilename <> "" Then
                                                            If cpCore.appRootFiles.fileExists(fieldfilename) Then
                                                                Copy = FieldDefaultValue
                                                            Else
                                                                Copy = cpCore.cdnFiles.readFile(fieldfilename)
                                                            End If
                                                            If Not FieldReadOnly Then
                                                                Copy = cpCore.htmlDoc.html_GetFormInputTextExpandable(FieldName, Copy, 10)
                                                            End If
                                                        End If
                                                        Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                    Case "dbquery", "querydb", "query", "db"
                                                        '
                                                        ' Display the output of a query
                                                        '
                                                        Copy = ""
                                                        FieldDataSource = cpCore.main_GetXMLAttribute(IsFound, TabNode, "DataSourceName", "")
                                                        FieldSQL = TabNode.InnerText
                                                        FieldCaption = cpCore.main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                        FieldDescription = cpCore.main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                        SQLPageSize = genericController.EncodeInteger(cpCore.main_GetXMLAttribute(IsFound, TabNode, "rowmax", ""))
                                                        If SQLPageSize = 0 Then
                                                            SQLPageSize = 100
                                                        End If
                                                        '
                                                        ' Run the SQL
                                                        '
                                                        Dim dt As DataTable

                                                        If FieldSQL <> "" Then
                                                            Try
                                                                dt = cpCore.db.executeSql(FieldSQL, FieldDataSource, , SQLPageSize)
                                                            Catch ex As Exception
                                                                ErrorDescription = ex.ToString
                                                                loadOK = False
                                                            End Try
                                                        End If
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
                                                                Copy = cpCore.htmlDoc.html_GetFormInputText2("result", genericController.encodeText(dataArray(0, 0)), , , , , True)
                                                            Else
                                                                '
                                                                ' Build headers
                                                                '
                                                                FieldCount = dt.Columns.Count
                                                                Copy = Copy & (cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"" style=""border-bottom:1px solid #444;border-right:1px solid #444;background-color:white;color:#444;"">")
                                                                Copy = Copy & (cr2 & "<tr>")
                                                                For Each dc As DataColumn In dt.Columns
                                                                    Copy = Copy & (cr2 & vbTab & "<td class=""ccadminsmall"" style=""border-top:1px solid #444;border-left:1px solid #444;color:black;padding:2px;padding-top:4px;padding-bottom:4px;"">" & dc.ColumnName & "</td>")
                                                                    'Copy = Copy & ("<td class=""ccHeaderCon ccAdminSmall"" style=""color:white"">" & RecordField.Name & "</td>")
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
                                                                            'Dim Cnt As Integer
                                                                            'Cnt = UBound(CellData)
                                                                            'Dim Ptr As Integer
                                                                            'For Ptr = 0 To Cnt - 1
                                                                            '    Copy = Copy & ("<br>(" & Ptr & ")&nbsp;[" & CellData(Ptr) & "]")
                                                                            'Next
                                                                            'Copy = Copy & (ColumnEnd)
                                                                        ElseIf genericController.encodeText(CellData) = "" Then
                                                                            Copy = Copy & (ColumnStart & "[empty]" & ColumnEnd)
                                                                        Else
                                                                            Copy = Copy & (ColumnStart & cpCore.htmlDoc.html_EncodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
                                                                        End If
                                                                    Next
                                                                    Copy = Copy & (RowEnd)
                                                                Next
                                                                Copy = Copy & (cr & "</table>")
                                                            End If
                                                        End If
                                                        Call TabCell.Add(Adminui.GetEditRow(Copy, FieldCaption, FieldDescription, False, False, ""))
                                                End Select
                                            Next
                                            Copy = Adminui.GetEditPanel(True, TabHeading, TabDescription, Adminui.EditTableOpen & TabCell.Text & Adminui.EditTableClose)
                                            If Copy <> "" Then
                                                Call cpCore.htmlDoc.main_AddLiveTabEntry(Replace(TabName, " ", "&nbsp;"), Copy, "ccAdminTab")
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
                            'Content.Add( cpcore.main_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormMobileBrowserControl))
                            '
                            '
                            '
                            If TabCnt > 0 Then
                                Content.Add(cpCore.htmlDoc.main_GetLiveTabs())
                            End If
                        End If
                    End If
                End If
            End If
            '
            getFormContent = Adminui.GetBody(Name, ButtonList, "", True, True, Description, "", 0, Content.Text)
            Content = Nothing
            '
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError10(Err.Number, Err.Source, Err.Description, "addon_execute_GetFormContent", True, False)
        End Function
        '
        '========================================================================
        '   Display field in the admin/edit
        '========================================================================
        '
        Private Function getFormContent_decodeSelector(SitePropertyName As String, SitePropertyValue As String, selector As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("AdminClass.addon_execute_GetFormContent_decodeSelector")
            '
            Dim ExpandedSelector As String
            Dim ignore As String
            Dim OptionCaption As String
            Dim OptionValue As String
            Dim OptionValue_AddonEncoded As String
            Dim OptionPtr As Integer
            Dim OptionCnt As Integer
            Dim OptionValues() As String
            Dim OptionSuffix As String
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
            Dim FastString As coreFastStringClass
            Dim FieldValueInteger As Integer
            Dim FieldRequired As Boolean
            Dim FieldHelp As String
            Dim AuthoringStatusMessage As String
            Dim Delimiter As String
            Dim Copy As String
            Dim Adminui As New coreAdminUIClass(cpCore)
            '
            Dim FieldName As String
            '
            FastString = New coreFastStringClass
            '
            Call buildAddonOptionLists(ignore, ExpandedSelector, SitePropertyName & "=" & selector, SitePropertyName & "=" & SitePropertyValue, "0", True)
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
                getFormContent_decodeSelector = cpCore.htmlDoc.html_GetFormInputText2(SitePropertyName, selector, 1, 20)
            End If

            FastString = Nothing
            Exit Function
            '
ErrorTrap:
            FastString = Nothing
            Call cpCore.handleLegacyError18("addon_execute_GetFormContent_decodeSelector")
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
        Public Sub buildAddonOptionLists2(ByRef OptionString_ForObjectCall As String, ByRef AddonOptionExpandedConstructor As String, AddonOptionConstructor As String, addonOptionString As String, InstanceID As String, IncludeSettingsBubbleOptions As Boolean)
            On Error GoTo ErrorTrap 'Const Tn = "BuildAddonOptionLists": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim SavePtr As Integer
            Dim InstanceTypes() As String
            Dim InstanceType As String
            Dim ConstructorTypes() As String
            Dim ConstructorType As String
            Dim ConstructorValue As String
            Dim ConstructorSelector As String
            Dim ConstructorName As String
            Dim ConstructorPtr As Integer
            Dim Pos As Integer
            Dim InstanceNameValues() As String
            Dim InstanceNames() As String
            Dim InstanceValues() As String
            Dim InstanceCnt As Integer
            Dim InstanceName As String
            Dim InstanceValue As String
            '
            Dim ConstructorNameValues() As String
            Dim ConstructorNames() As String
            Dim ConstructorSelectors() As String
            Dim ConstructorValues() As String
            '
            Dim IPtr As Integer
            Dim ConstructorCnt As Integer


            ConstructorCnt = 0
            If (AddonOptionConstructor <> "") Then
                '
                ' Initially Build Constructor from AddonOptions
                '
                ConstructorNameValues = Split(AddonOptionConstructor, vbCrLf)
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
            If addonOptionString <> "" Then
                '
                InstanceNameValues = Split(addonOptionString, "&")
                InstanceCnt = UBound(InstanceNameValues) + 1
                ReDim InstanceNames(InstanceCnt - 1)
                ReDim InstanceValues(InstanceCnt - 1)

                ReDim InstanceTypes(InstanceCnt - 1)
                SavePtr = 0
                For IPtr = 0 To InstanceCnt - 1
                    InstanceName = InstanceNameValues(IPtr)
                    InstanceValue = ""
                    Pos = genericController.vbInstr(1, InstanceName, "=")
                    If Pos > 1 Then
                        InstanceValue = Mid(InstanceName, Pos + 1)
                        InstanceName = Trim(Left(InstanceName, Pos - 1))
                        Pos = genericController.vbInstr(1, InstanceValue, "[")
                        If Pos >= 1 Then
                            InstanceValue = Mid(InstanceValue, 1, Pos - 1)
                        End If
                    End If
                    If InstanceName <> "" Then
                        'Pos = genericController.vbInstr(1, InstanceName, ",")
                        'If Pos > 1 Then
                        '    InstanceType = Mid(InstanceName, Pos + 1)
                        '    InstanceName = Left(InstanceName, Pos - 1)
                        'End If
                        InstanceNames(SavePtr) = genericController.vbLCase(InstanceName)
                        InstanceValues(SavePtr) = InstanceValue
                        'InstanceTypes(IPtr) = InstanceType
                        '
                        ' if the name is not in the Constructor, add it
                        '
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
            End If
            AddonOptionExpandedConstructor = ""
            '
            ' Build output strings from name and value found
            '
            For ConstructorPtr = 0 To ConstructorCnt - 1
                ConstructorName = ConstructorNames(ConstructorPtr)
                ConstructorValue = ConstructorValues(ConstructorPtr)
                ConstructorSelector = ConstructorSelectors(ConstructorPtr)
                ' here goes nothing!!
                OptionString_ForObjectCall = OptionString_ForObjectCall & ConstructorName & "=" & ConstructorValue & "&"
                'OptionString_ForObjectCall = OptionString_ForObjectCall & csv_DecodeAddonOptionArgument(ConstructorName) & "=" & csv_DecodeAddonOptionArgument(ConstructorValue) & vbCrLf
                If IncludeSettingsBubbleOptions Then
                    AddonOptionExpandedConstructor = AddonOptionExpandedConstructor & vbCrLf & cpCore.htmlDoc.pageManager_GetAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector)
                End If
            Next
            OptionString_ForObjectCall = OptionString_ForObjectCall & "InstanceID=" & InstanceID
            'If OptionString_ForObjectCall <> "" Then
            '    OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 1)
            '    'OptionString_ForObjectCall = Mid(OptionString_ForObjectCall, 1, Len(OptionString_ForObjectCall) - 2)
            'End If
            If AddonOptionExpandedConstructor <> "" Then
                AddonOptionExpandedConstructor = Mid(AddonOptionExpandedConstructor, 3)
            End If

            Exit Sub
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
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
        Public Sub buildAddonOptionLists(Option_String_ForObjectCall As String, AddonOptionExpandedConstructor As String, AddonOptionConstructor As String, InstanceOptionList As String, InstanceID As String, IncludeEditWrapper As Boolean)
            Call buildAddonOptionLists2(Option_String_ForObjectCall, AddonOptionExpandedConstructor, AddonOptionConstructor, InstanceOptionList, InstanceID, IncludeEditWrapper)
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
            CS = cpCore.csOpen("Wrappers", WrapperID, , , SelectFieldList)
            If cpCore.db.cs_ok(CS) Then
                Wrapper = cpCore.db.cs_getText(CS, "copytext")
                wrapperName = cpCore.db.cs_getText(CS, "name")
                TargetString = cpCore.db.cs_getText(CS, "targetString")
                '
                SourceComment = "wrapper " & wrapperName
                If WrapperSourceForComment <> "" Then
                    SourceComment = SourceComment & " for " & WrapperSourceForComment
                End If
                Call cpCore.htmlDoc.main_AddOnLoadJavascript2(cpCore.db.cs_getText(CS, "javascriptonload"), SourceComment)
                Call cpCore.htmlDoc.main_AddEndOfBodyJavascript2(cpCore.db.cs_getText(CS, "javascriptbodyend"), SourceComment)
                Call cpCore.htmlDoc.main_AddHeadTag2(cpCore.db.cs_getText(CS, "OtherHeadTags"), SourceComment)
                '
                JSFilename = cpCore.db.cs_getText(CS, "jsfilename")
                If JSFilename <> "" Then
                    JSFilename = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, JSFilename)
                    Call cpCore.htmlDoc.main_AddHeadScriptLink(JSFilename, SourceComment)
                End If
                Copy = cpCore.db.cs_getText(CS, "stylesfilename")
                If Copy <> "" Then
                    If genericController.vbInstr(1, Copy, "://") <> 0 Then
                    ElseIf Left(Copy, 1) = "/" Then
                    Else
                        Copy = cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, Copy)
                    End If
                    Call cpCore.htmlDoc.main_AddStylesheetLink2(Copy, SourceComment)
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
            Call cpCore.db.cs_Close(CS)
            '
            addWrapperToResult = s
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleLegacyError18("WrapContent")
        End Function        '
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