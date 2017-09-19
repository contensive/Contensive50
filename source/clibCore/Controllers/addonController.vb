
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
        '====================================================================================================
        ''' <summary>
        ''' Execute an addon because it is a dependency of another addon/page/template
        ''' </summary>
        ''' <param name="addonId"></param>
        ''' <param name="context"></param>
        ''' <returns></returns>
        ''' 
        Public Function executeDependency(addon As Models.Entity.addonModel, context As CPUtilsBaseClass.addonExecuteContext) As String
            Dim saveContextIsIncludeAddon As Boolean = context.isIncludeAddon
            context.isIncludeAddon = True
            Dim result As String = execute(addon, context)
            context.isIncludeAddon = saveContextIsIncludeAddon
            Return result
        End Function
        '====================================================================================================
        '
        Public Function execute(addon As Models.Entity.addonModel, executeContext As CPUtilsBaseClass.addonExecuteContext) As String
            Dim returnVal As String = ""
            Try
                If (addon Is Nothing) Then
                    '
                    ' -- addon not found
                    returnVal = "The Add-on could not be found. It may have been deleted or marked inactive. Please use the Add-on Manager to replace it, or edit this page and remove it."
                ElseIf (executeContext Is Nothing) Then
                    '
                    ' -- context not configured 
                Else
                    '
                    'Todo -- refactor this out
                    ' -- fake the legacy environment
                    Dim addonId As Integer = addon.id
                    Dim AddonNameOrGuid As String = addon.ccguid
                    Dim optionString As String = ""
                    For Each kvp As KeyValuePair(Of String, String) In executeContext.instanceArguments
                        optionString &= vbCrLf & kvp.Key & "=" & kvp.Value
                    Next
                    Dim addonType As CPUtilsBaseClass.addonContext = executeContext.addonType
                    Dim HostContentName As String = executeContext.hostRecord.contentName
                    Dim HostRecordID As Integer = executeContext.hostRecord.recordId
                    Dim HostFieldName As String = executeContext.hostRecord.fieldName
                    Dim ACInstanceID As String = executeContext.instanceGuid
                    Dim IsIncludeAddon As Boolean = executeContext.isIncludeAddon
                    Dim DefaultWrapperID As Integer = executeContext.wrapperID
                    Dim personalizationPeopleId As Integer = executeContext.personalizationPeopleId
                    Dim personalizationIsAuthenticated As Boolean = executeContext.personalizationAuthenticated
                    '
                    ' -- past in the legacy code
                    Dim blockJavascriptAndCss As Boolean
                    Dim JSOnLoad As String = String.Empty
                    Dim JSBodyEnd As String = String.Empty
                    Dim JSFilename As String = String.Empty
                    Dim DefaultStylesFilename As String = String.Empty
                    'Dim CustomStylesFilename As String = String.Empty
                    Dim TestString As String
                    Dim ReplaceSource As String
                    Dim ReplaceValue As String
                    Dim AddonStylesEditIcon As String
                    Dim SiteStylesEditIcon As String = String.Empty
                    Dim DialogList As String = String.Empty
                    Dim ToolBar As String
                    Dim ScriptingTimeout As Integer
                    Dim ScriptCallbackContent As String = String.Empty
                    Dim errorMessageForAdmin As String = String.Empty
                    'Dim CollectionGuid As String = String.Empty
                    'Dim DotNetClassFullName As String = String.Empty
                    Dim ScriptingEntryPoint As String = String.Empty
                    Dim scriptinglanguageid As Integer
                    Dim ScriptingLanguage As String = String.Empty
                    Dim ScriptingCode As String = String.Empty
                    Dim EditWrapperHTMLID As String
                    Dim QS As String
                    Dim QSSplit() As String
                    Dim NVPair As String
                    Dim NVSplit() As String
                    Dim FrameID As String
                    Dim AsAjaxID As String
                    Dim OptionNames() As String
                    Dim OptionValues() As String
                    Dim OptionsForCPVars As NameValuePrivateType() = {}
                    Dim OptionsForCPVars_Cnt As Integer
                    Dim RemoteAssetContent As String = String.Empty
                    Dim kmaHTTP As httpRequestController
                    Dim WorkingLink As String
                    Dim FormContent As String = String.Empty
                    Dim ExitAddonWithBlankResponse As Boolean
                    Dim RemoteAssetLink As String = String.Empty
                    Dim AsAjax As Boolean
                    Dim InFrame As Boolean
                    Dim IncludeEditWrapper As Boolean
                    Dim AddedByName As String = String.Empty
                    Dim AddonCommentName As String
                    Dim AddonOptionConstructor As String = String.Empty
                    Dim AddonOptionExpandedConstructor As String = String.Empty
                    Dim OptionString_ForObjectCall As String = String.Empty
                    Dim Pos As Integer
                    Dim Ptr As Integer
                    Dim HelpIcon As String = String.Empty
                    Dim InstanceSettingsEditIcon As String
                    Dim OptionPair As String() = {}
                    Dim OptionPtr As Integer
                    Dim OptionCnt As Integer
                    Dim Link As String = String.Empty
                    'Dim ProgramID As String = String.Empty
                    Dim Options() As String
                    Dim OptionName As String
                    Dim OptionValue As String
                    '  Dim addonCopy As String = String.Empty
                    Dim TextContent As String = String.Empty
                    Dim ObjectContent As String = String.Empty
                    Dim AssemblyContent As String = String.Empty
                    Dim ScriptContent As String = String.Empty
                    Dim helpCopy As String = String.Empty
                    Dim helpLink As String = String.Empty
                    Dim PageTitle As String = String.Empty
                    Dim MetaDescription As String = String.Empty
                    Dim MetaKeywordList As String = String.Empty
                    Dim OtherHeadTags As String = String.Empty
                    Dim AddonEditIcon As String
                    Dim FoundAddon As Boolean
                    Dim FormXML As String = String.Empty
                    Dim WrapperID As Integer
                    Dim ContainerCssID As String = String.Empty
                    Dim ContainerCssClass As String = String.Empty
                    Dim IsInline As Boolean
                    Dim WorkingOptionString As String
                    Dim HTMLViewerEditIcon As String
                    Dim AddonBlockEditTools As Boolean
                    Dim ReplaceCnt As Integer
                    Dim ReplaceNames As String() = {}
                    Dim ReplaceValues As String() = {}
                    Dim StartTickCount As Integer
                    'Dim addonCachePtr As Integer
                    '   Dim addonCollectionId As Integer
                    '
                    '
                    ' ----- OptionString and FilterInput values before this call are saved on the stack
                    '
                    Dim PushOptionString As String
                    PushOptionString = optionString
                    '
                    ' ----- Debug timer
                    '
                    StartTickCount = GetTickCount
                    WrapperID = DefaultWrapperID
                    If (personalizationPeopleId = 0) Then
                        '
                        ' just in case - during transition from cpCoreClass to csv, in case a call is missing.
                        '
                        personalizationPeopleId = cpCore.authContext.user.id
                        personalizationIsAuthenticated = cpCore.authContext.isAuthenticated()
                    End If
                    '
                    ' ----- Set WorkingOptionString to what came in from the tag of the object
                    '       This may be replaced later if the tag is empty, and the actual add-on arguments have default values
                    '
                    WorkingOptionString = optionString
                    ''
                    '' ----- Lookup the addon
                    ''
                    'If addonId <> 0 Then
                    '    addon = cpCore.addonCache.getAddonById(addonId)
                    '    If (addon Is Nothing) Then debugController.testPoint(cpCore, "execute, addon not found from id [" & addonId & "]")
                    'ElseIf (genericController.isGuid(AddonNameOrGuid)) Then
                    '    addon = cpCore.addonCache.getAddonByGuid(AddonNameOrGuid)
                    '    If (addon Is Nothing) Then debugController.testPoint(cpCore, "execute, addon not found from guid [" & AddonNameOrGuid & "]")
                    'Else
                    '    addon = cpCore.addonCache.getAddonByName(AddonNameOrGuid)
                    '    If (addon Is Nothing) Then debugController.testPoint(cpCore, "execute, addon not found from name [" & AddonNameOrGuid & "]")
                    'End If
                    FoundAddon = False
                    If (addon Is Nothing) Then
                        '
                        ' -- not found 
                        If (addonType = CPUtilsBaseClass.addonContext.ContextEmail) Or (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) Or (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (addonType = CPUtilsBaseClass.addonContext.ContextSimple) Then
                            '
                            ' Block all output even on error
                            '
                        ElseIf cpCore.authContext.isAuthenticatedAdmin(cpCore) Or cpCore.authContext.isAuthenticatedContentManager(cpCore, "Page Content") Then
                            '
                            ' Provide hint to administrators
                            '
                            If AddonNameOrGuid = "" And addonId <> 0 Then
                                AddonNameOrGuid = "Addon #" & addonId
                            End If
                            If addonType = CPUtilsBaseClass.addonContext.ContextAdmin Then
                                returnVal = "The Add-on '" & AddonNameOrGuid & "' could not be found. It may have been deleted or marked inactive. If you are receiving this message after clicking an Add-on from the Navigator, their may be a problem with this Add-on. If you are receiving this message from the main admin page, your Dashboard Add-on may be set incorrectly. Use the Admin tab under Preferences to select the Dashboard, or <a href=""?" & RequestNameDashboardReset & "=" & cpCore.authContext.visit.id & """>click here</a> to automatically reset the dashboard."
                            Else
                                returnVal = "The Add-on '" & AddonNameOrGuid & "' could not be found. It may have been deleted or marked inactive. Please use the Add-on Manager to replace it, or edit this page and remove it."
                            End If
                            returnVal = cpCore.html.html_GetAdminHintWrapper(returnVal)
                        End If
                        If (addonId > 0) Then
                            Throw New ApplicationException("The Add-on could not be found by id [" & addonId & "] or name/guid [" & AddonNameOrGuid & "]")
                        Else
                            Throw New ApplicationException("The Add-on could not be found by name/guid [" & AddonNameOrGuid & "]")
                        End If
                    Else
                        '
                        ' -- found
                        FoundAddon = True
                        debugController.testPoint(cpCore, "execute [#" & addon.id & ", " & addon.name & ", guid " & addon.ccguid & "]")
                        If Not String.IsNullOrEmpty(addon.ObjectProgramID) Then
                            '
                            ' -- addons with activeX components are deprecated

                            Dim addonDescription As String = getAddonDescription(cpCore, addon)
                            Throw New ApplicationException("Addon is no longer supported because it contains an active-X component, add-on " & addonDescription & ".")
                        Else
                            addonId = addon.id
                            Link = addon.Link
                            'DotNetClassFullName = addon.DotNetClass
                            AddonOptionConstructor = addon.ArgumentList
                            AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCrLf, vbCr)
                            AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbLf, vbCr)
                            AddonOptionConstructor = genericController.vbReplace(AddonOptionConstructor, vbCr, vbCrLf)
                            AddonBlockEditTools = False
                            TextContent = ""
                            FormXML = ""
                            TextContent = addon.CopyText
                            IsInline = genericController.EncodeBoolean(addon.IsInline)
                            DefaultStylesFilename = addon.StylesFilename.filename
                            '
                            ' Add custom styles
                            '
                            FormXML = addon.FormXML
                            RemoteAssetLink = addon.RemoteAssetLink
                            AsAjax = genericController.EncodeBoolean(addon.AsAjax)
                            InFrame = genericController.EncodeBoolean(addon.InFrame)
                            ScriptingEntryPoint = addon.ScriptingEntryPoint
                            scriptinglanguageid = genericController.EncodeInteger(addon.ScriptingLanguageID)
                            '
                            ' Get Language
                            '
                            ScriptingLanguage = ""
                            If scriptinglanguageid <> 0 Then
                                ScriptingLanguage = cpCore.db.getRecordName("Scripting Languages", scriptinglanguageid)
                            End If
                            If ScriptingLanguage = "" Then
                                ScriptingLanguage = "VBScript"
                            End If
                            ScriptingCode = addon.ScriptingCode
                            AddonBlockEditTools = genericController.EncodeBoolean(addon.BlockEditTools)
                            ScriptingTimeout = genericController.EncodeInteger(addon.ScriptingTimeout)
                            helpCopy = addon.Help
                            helpLink = addon.HelpLink
                            JSOnLoad = addon.JavaScriptOnLoad
                            JSBodyEnd = addon.JavaScriptBodyEnd
                            PageTitle = addon.PageTitle
                            MetaDescription = addon.MetaDescription
                            MetaKeywordList = addon.MetaKeywordList
                            OtherHeadTags = addon.OtherHeadTags
                            JSFilename = addon.JSFilename.filename
                            If JSFilename <> "" Then
                                JSFilename = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, JSFilename)
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
                            If genericController.vbLCase(addon.ccguid) = genericController.vbLCase(addonGuidContentBox) Then
                                AsAjax = False
                                InFrame = False
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_BlockNoAjax
                            ElseIf IsInline Then
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Inline
                            Else
                                AddonOptionConstructor = AddonOptionConstructor & AddonOptionConstructor_Block
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
                                ' Enable Edit Wrapper for Page Content edit mode
                                '-----------------------------------------------------------------
                                '
                                If True Then
                                    IncludeEditWrapper =
                                (Not AddonBlockEditTools) _
                                And (addonType <> CPUtilsBaseClass.addonContext.ContextEditor) _
                                And (addonType <> CPUtilsBaseClass.addonContext.ContextEmail) _
                                And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) _
                                And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) _
                                And (addonType <> CPUtilsBaseClass.addonContext.ContextSimple) _
                                And (Not IsIncludeAddon)
                                    If IncludeEditWrapper Then
                                        IncludeEditWrapper = IncludeEditWrapper _
                                    And (cpCore.visitProperty.getBoolean("AllowAdvancedEditor") _
                                    And ((addonType = CPUtilsBaseClass.addonContext.ContextAdmin) Or cpCore.authContext.isEditing(HostContentName)))
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
                                    Call logController.appendLog(cpCore, "addon_execute, Addon [" & addon.name & "] was called by itself. This is not allowed. AddonID [" & addonId & "]")
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
                                    optionString = OptionString_ForObjectCall
                                    '
                                    '
                                    '-----------------------------------------------------------------------------------------------------
                                    ' Process the content for each context as needed
                                    '-----------------------------------------------------------------------------------------------------
                                    '
                                    If (InFrame And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                        '
                                        '-----------------------------------------------------------------
                                        ' inFrame and this is NOT the callback - setup the iframe for a callback
                                        ' js,styles and other features are NOT added to the host page, they go to the remotemethod page
                                        '-----------------------------------------------------------------
                                        '
                                        'todo true is a check for iis driven page - move to iisController
                                        If True Then
                                            '
                                            ' web-only
                                            '
                                            Link = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & requestAppRootPath & cpCore.siteProperties.serverPageDefault
                                            If genericController.vbInstr(1, Link, "?") = 0 Then
                                                Link = Link & "?"
                                            Else
                                                Link = Link & "&"
                                            End If
                                            Link = Link _
                                        & "nocache=" & Rnd() _
                                        & "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                        & "&HostRecordID=" & HostRecordID _
                                        & "&remotemethodaddon=" & EncodeURL(addon.id.ToString) _
                                        & "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                        & ""
                                            FrameID = "frame" & GetRandomInteger()
                                            returnVal = "<iframe src=""" & Link & """ id=""" & FrameID & """ onload=""cj.setFrameHeight('" & FrameID & "');"" class=""ccAddonFrameCon"" frameborder=""0"" scrolling=""no"">This content is not visible because your browser does not support iframes</iframe>" _
                                        & cr & "<script language=javascript type=""text/javascript"">" _
                                        & cr & "// Safari and Opera need a kick-start." _
                                        & cr & "var e=document.getElementById('" & FrameID & "');if(e){var iSource=e.src;e.src='';e.src = iSource;}" _
                                        & cr & "</script>"
                                        End If
                                    ElseIf (AsAjax And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                        '
                                        '-----------------------------------------------------------------
                                        ' AsAjax and this is NOT the callback - setup the ajax callback
                                        ' js,styles and other features from the addon record are added to the host page
                                        ' during the remote method, these are blocked, but if any are added during
                                        '   DLL processing, they have to be handled
                                        '-----------------------------------------------------------------
                                        '
                                        If True Then
                                            AsAjaxID = "asajax" & GetRandomInteger()
                                            QS = "" _
                                        & RequestNameRemoteMethodAddon & "=" & EncodeRequestVariable(addon.id.ToString()) _
                                        & "&HostContentName=" & EncodeRequestVariable(HostContentName) _
                                        & "&HostRecordID=" & HostRecordID _
                                        & "&HostRQS=" & EncodeRequestVariable(cpCore.doc.refreshQueryString) _
                                        & "&HostQS=" & EncodeRequestVariable(cpCore.webServer.requestQueryString) _
                                        & "&optionstring=" & EncodeRequestVariable(WorkingOptionString) _
                                        & ""
                                            '
                                            ' -- exception made here. AsAjax is not used often, and this can create a QS too long
                                            '& "&HostForm=" & EncodeRequestVariable(cpCore.webServer.requestFormString) _
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
                                            Call cpCore.html.doc_AddPagetitle2(PageTitle, AddedByName)
                                            Call cpCore.html.doc_addMetaDescription2(MetaDescription, AddedByName)
                                            Call cpCore.html.doc_addMetaKeywordList2(MetaKeywordList, AddedByName)
                                            Call cpCore.html.doc_AddHeadTag2(OtherHeadTags, AddedByName)
                                            If Not blockJavascriptAndCss Then
                                                '
                                                ' add javascript and styles if it has not run already
                                                '
                                                Call cpCore.html.addOnLoadJavascript(JSOnLoad, AddedByName)
                                                Call cpCore.html.addBodyJavascriptCode(JSBodyEnd, AddedByName)
                                                Call cpCore.html.addJavaScriptLinkHead(JSFilename, AddedByName)
                                                If DefaultStylesFilename <> "" Then
                                                    Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, DefaultStylesFilename), addon.name & " default")
                                                End If
                                                'If CustomStylesFilename <> "" Then
                                                '    Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, CustomStylesFilename), AddonName & " custom")
                                                'End If
                                            End If
                                        End If
                                    Else
                                        '
                                        '-----------------------------------------------------------------
                                        ' otherwise - produce the content from the addon
                                        '   setup RQS as needed - RQS provides the querystring for add-ons to create links that return to the same page
                                        '-----------------------------------------------------------------------------------------------------
                                        '
                                        If (InFrame And (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                            '
                                            ' Add-on setup for InFrame, running the call-back - this page must think it is just the remotemethod
                                            '
                                            If True Then
                                                Call cpCore.doc.addRefreshQueryString(RequestNameRemoteMethodAddon, addon.id.ToString)
                                                Call cpCore.doc.addRefreshQueryString("optionstring", WorkingOptionString)
                                            End If
                                        ElseIf (AsAjax And (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                            '
                                            ' Add-on setup for AsAjax, running the call-back - put the referring page's QS as the RQS
                                            ' restore form values
                                            '
                                            If True Then
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
                                                            Call cpCore.doc.addRefreshQueryString(NVSplit(0), NVSplit(1))
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
                                                '  CustomStylesFilename = ""
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
                                        Dim addonIncludeRules As List(Of Models.Entity.addonIncludeRuleModel) = Models.Entity.addonIncludeRuleModel.createList(cpCore, "(addonid=" & addonId & ")")
                                        Dim IncludeContent As String = ""
                                        '
                                        '-----------------------------------------------------------------
                                        ' Do replacements from Option String and Pick out WrapperID, and AsAjax
                                        '-----------------------------------------------------------------
                                        '
                                        TestString = addon.Copy & TextContent & PageTitle & MetaDescription & MetaKeywordList & OtherHeadTags & FormXML
                                        If (TestString <> "") And (ReplaceCnt > 0) Then
                                            For Ptr = 0 To ReplaceCnt - 1
                                                ReplaceSource = "$" & ReplaceNames(Ptr) & "$"
                                                ' this section takes 15msec every addon, 32 addons is 480msec.
                                                ' 20131221 - 4.2.317 - try test first to save time
                                                If isInStr(1, TestString, ReplaceSource) Then
                                                    ReplaceValue = ReplaceValues(Ptr)
                                                    addon.Copy = genericController.vbReplace(addon.Copy, ReplaceSource, ReplaceValue, 1, 99, vbTextCompare)
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
                                        If (addonIncludeRules.Count > 0) Or (ScriptingCode <> "") Or (addon.DotNetClass <> "") Then
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
                                            If addonIncludeRules.Count > 0 Then
                                                For Each addonRule As Models.Entity.addonIncludeRuleModel In addonIncludeRules
                                                    If addonRule.IncludedAddonID > 0 Then
                                                        IncludeContent = IncludeContent & executeDependency(Models.Entity.addonModel.create(cpCore, addonRule.IncludedAddonID), executeContext)
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
                                                    ScriptContent = executeScript(addon, ScriptingLanguage, ScriptingCode, ScriptingEntryPoint, errorMessageForAdmin, ScriptingTimeout, "Addon [" & addon.name & "]", ReplaceCnt, ReplaceNames, ReplaceValues)
                                                Catch ex As Exception
                                                    Dim addonDescription As String = getAddonDescription(cpCore, addon)
                                                    Throw New ApplicationException("There was an error executing the script component of Add-on " & addonDescription & ", AddonOptionString [" & WorkingOptionString & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "")
                                                End Try
                                            End If
                                            '
                                            ' ----- Dot Net Addons
                                            '   Get path to the addon from the collection guid
                                            '   If no collection, just look in the /addon path
                                            '
                                            'hint = hint & ",10"
                                            If addon.DotNetClass <> "" Then
                                                '
                                                Dim addonCollection As Models.Entity.AddonCollectionModel = Models.Entity.AddonCollectionModel.create(cpCore, addon.CollectionID)

                                                'Dim csTmp As Integer
                                                'csTmp = cpCore.db.cs_openCsSql_rev("default", "select ccGuid from ccAddonCollections where id=" & addon.CollectionID)
                                                'If cpCore.db.cs_ok(csTmp) Then
                                                '    CollectionGuid = cpCore.db.cs_getText(csTmp, "ccGuid")
                                                'End If
                                                'Call cpCore.db.cs_Close(csTmp)
                                                '
                                                AssemblyContent = executeAssembly(addon, addonCollection, errorMessageForAdmin)
                                                If (errorMessageForAdmin <> "") Then
                                                    '
                                                    ' log the error
                                                    '
                                                    Dim addonDescription As String = getAddonDescription(cpCore, addon)
                                                    Throw New ApplicationException("Unexpected exception in addon " & addonDescription)
                                                    '
                                                    ' Put up an admin hint
                                                    '
                                                    If (Not True) Or (addonType = CPUtilsBaseClass.addonContext.ContextEmail) Or (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) Or (addonType = CPUtilsBaseClass.addonContext.ContextSimple) Then
                                                        '
                                                        ' Block all output even on error
                                                        '
                                                    ElseIf cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
                                                        '
                                                        ' Provide hint to administrators
                                                        '
                                                        If addon.name = "" And addonId <> 0 Then
                                                            addon.name = "Addon #" & addonId
                                                        End If
                                                        AssemblyContent = cpCore.html.html_GetAdminHintWrapper("<p>There was an error executing the assembly component of Add-on [" & addon.name & "], AddonOptionString [" & WorkingOptionString & "] with class name [" & addon.DotNetClass & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "</p>")
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
                                        'If addon.ObjectProgramID <> "" Then
                                        '    '
                                        '    ' Go ahead
                                        '    '
                                        '    Try
                                        '        ObjectContent = csv_ExecuteActiveX(addon.ObjectProgramID, AddonName, OptionString_ForObjectCall, WorkingOptionString, errorMessageForAdmin)
                                        '    Catch ex As Exception
                                        '        handleException(ex, "There was an error executing the activex component of Add-on [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "], with Program ID [" & addon.ObjectProgramID & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "")
                                        '    End Try
                                        '    AggrObject = Nothing
                                        'End If
                                        ''hint = ""
                                        '
                                        '-----------------------------------------------------------------------------------------------------
                                        '   RemoteAssetLink
                                        '-----------------------------------------------------------------------------------------------------
                                        '
                                        If (True) Then
                                            If RemoteAssetLink <> "" Then
                                                WorkingLink = RemoteAssetLink
                                                If genericController.vbInstr(1, WorkingLink, "://") = 0 Then
                                                    If True Then
                                                        '
                                                        ' use request object to build link
                                                        '
                                                        If Mid(WorkingLink, 1, 1) = "/" Then
                                                            WorkingLink = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & WorkingLink
                                                        Else
                                                            WorkingLink = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & cpCore.webServer.requestVirtualFilePath & WorkingLink
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
                                                kmaHTTP = New httpRequestController()
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
                                        If True And (FormXML <> "") Then
                                            FormContent = getFormContent(Nothing, FormXML, ExitAddonWithBlankResponse)
                                            If ExitAddonWithBlankResponse Then
                                                Return String.Empty
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
                                        If True And (Link <> "") Then
                                            If WorkingOptionString <> "" Then
                                                If genericController.vbInstr(1, Link, "?") = 0 Then
                                                    Link = Link & "?" & WorkingOptionString
                                                Else
                                                    Link = Link & "&" & WorkingOptionString
                                                End If
                                            End If
                                            Link = modifyLinkQuery(Link, RequestNameJSForm, "1", True)
                                            Link = EncodeAppRootPath(Link, cpCore.webServer.requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
                                            ScriptCallbackContent = "<SCRIPT LANGUAGE=""JAVASCRIPT"" SRC=""" & Link & """></SCRIPT>"
                                        End If
                                        '
                                        '-----------------------------------------------------------------
                                        ' Add javascripts and other features to page
                                        '-----------------------------------------------------------------
                                        '
                                        AddedByName = addon.name & " addon"
                                        '
                                        '#If traceExecuteAddon Then
                                        'ticksNow = GetTickCount : Ticks = (ticksNow - ticksLast) : ticksLast = ticksNow : Trace = Trace & vbCrLf & traceSN & "(" & Ticks & ") aa"
                                        '#End If
                                        If True Then
                                            Call cpCore.html.doc_AddPagetitle2(PageTitle, AddedByName)
                                            Call cpCore.html.doc_addMetaDescription2(MetaDescription, AddedByName)
                                            Call cpCore.html.doc_addMetaKeywordList2(MetaKeywordList, AddedByName)
                                            Call cpCore.html.doc_AddHeadTag2(OtherHeadTags, AddedByName)
                                            If Not blockJavascriptAndCss Then
                                                Call cpCore.html.addOnLoadJavascript(JSOnLoad, AddedByName)
                                                Call cpCore.html.addBodyJavascriptCode(JSBodyEnd, AddedByName)
                                                Call cpCore.html.addJavaScriptLinkHead(JSFilename, AddedByName)
                                                If DefaultStylesFilename <> "" Then
                                                    Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, DefaultStylesFilename), addon.name & " default")
                                                End If
                                                'If CustomStylesFilename <> "" Then
                                                '    Call cpCore.html.addStyleLink(cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, CustomStylesFilename), AddonName & " custom")
                                                'End If
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
                                        Dim layoutErrors As String = String.Empty
                                        If (addonType = CPUtilsBaseClass.addonContext.ContextEditor) Then
                                            '
                                            ' editor -- no encoding and no contentcommands
                                            '
                                            returnVal = TextContent & addon.Copy
                                            returnVal = returnVal & IncludeContent & ScriptCallbackContent & FormContent & RemoteAssetContent & ScriptContent & ObjectContent & AssemblyContent
                                            '
                                            ' csv_EncodeContent everything
                                            '
                                            's = csv_EncodeContent9(s, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (Context = ContextEmail),  WrapperID, ignore_TemplateCaseOnly_PageContent, Context, personalizationIsAuthenticated, nothing, False)
                                        Else
                                            '
                                            ' encode the content parts of the addon
                                            '
                                            returnVal = TextContent & addon.Copy
                                            If returnVal <> "" Then
                                                returnVal = cpCore.html.html_executeContentCommands(Nothing, returnVal, CPUtilsBaseClass.addonContext.ContextAdmin, personalizationPeopleId, personalizationIsAuthenticated, layoutErrors)
                                                's = csv_EncodeContent9(s, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (Context = ContextEmail), WrapperID, ignore_TemplateCaseOnly_PageContent, Context, personalizationIsAuthenticated, nothing, False)
                                            End If
                                            '
                                            ' add in the rest
                                            '
                                            returnVal = returnVal & IncludeContent & ScriptCallbackContent & FormContent & RemoteAssetContent & ScriptContent & ObjectContent & AssemblyContent
                                            '
                                            ' csv_EncodeContent everything
                                            '
                                            returnVal = cpCore.html.encodeContent10(returnVal, personalizationPeopleId, HostContentName, HostRecordID, 0, False, False, True, True, False, True, "", "", (addonType = CPUtilsBaseClass.addonContext.ContextEmail), WrapperID, "", addonType, personalizationIsAuthenticated, Nothing, False)
                                        End If
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
                                            Dim addonDescription As String = getAddonDescription(cpCore, addon)
                                            Throw New ApplicationException("xml structured commands are no longer supported, addon" & addonDescription)
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
                                            '    If (Not true) Or (Context = CPUtilsBaseClass.addonContext.contextEmail) Or (Context = CPUtilsBaseClass.addonContext.ContextRemoteMethod) Or (Context = CPUtilsBaseClass.addonContext.ContextSimple) Then
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
                                            '        s = s & cpCore.main_GetAdminHintWrapper("<p>There was an error executing the Layout Engine for addon [" & AddonName & "], AddonOptionString [" & WorkingOptionString & "], with Program ID [" & addon.ObjectProgramID & "]. The details of this error follow.</p><p>" & errorMessageForAdmin & "</p>")
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
                                                returnVal = cr & "<div id=""" & ContainerCssID & """ class=""" & ContainerCssClass & """>" & htmlIndent(returnVal) & cr & "</div>"
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
                                    If True Then
                                        If (InFrame And (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml)) Then
                                            '
                                            ' Return IFrame content
                                            '   Framed in content, during the remote method call
                                            '   add in the rest of the html page
                                            '
                                            Call cpCore.doc.setMetaContent(0, 0)
                                            returnVal = "" _
                                        & cpCore.siteProperties.docTypeDeclaration() _
                                        & vbCrLf & "<html>" _
                                        & cr & "<head>" _
                                        & vbCrLf & htmlIndent(cpCore.doc.getHtmlHead(False)) _
                                        & cr & "</head>" _
                                        & cr & TemplateDefaultBodyTag _
                                        & cr & "</body>" _
                                        & vbCrLf & "</html>"
                                        ElseIf (AsAjax And (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                            '
                                            ' Return Ajax content
                                            '   AsAjax addon, during the Ajax callback
                                            '   need to create an onload event that runs everything appended to onload within this content
                                            '
                                            returnVal = returnVal
                                        ElseIf ((addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) Or (addonType = CPUtilsBaseClass.addonContext.ContextRemoteMethodJson)) Then
                                            '
                                            ' Return non-ajax/non-Iframe remote method content (no wrapper)
                                            '
                                        ElseIf (addonType = CPUtilsBaseClass.addonContext.ContextEmail) Then
                                            '
                                            ' Return Email context (no wrappers)
                                            '
                                        ElseIf (addonType = CPUtilsBaseClass.addonContext.ContextSimple) Then
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
                                                EditWrapperHTMLID = "eWrapper" & cpCore.pageAddonCnt
                                                'HelpIcon = cpcore.main_GetHelpLink("", "Add-on " & AddonName, helpCopy, helpLink)
                                                '
                                                ' Edit Icon
                                                '
                                                If (addonId <> 0) Then
                                                    If cpCore.visitProperty.getBoolean("AllowAdvancedEditor") Then
                                                        AddonEditIcon = GetIconSprite("", 0, "/ccLib/images/tooledit.png", 22, 22, "Edit the " & addon.name & " Add-on", "Edit the " & addon.name & " Add-on", "", True, "")
                                                        AddonEditIcon = "<a href=""" & cpCore.siteProperties.adminURL & "?cid=" & cpCore.metaData.getContentId(cnAddons) & "&id=" & addonId & "&af=4&aa=2&ad=1"" tabindex=""-1"">" & AddonEditIcon & "</a>"
                                                        InstanceSettingsEditIcon = getInstanceBubble(addon.name, AddonOptionExpandedConstructor, HostContentName, HostRecordID, HostFieldName, ACInstanceID, addonType, DialogList)
                                                        AddonStylesEditIcon = getAddonStylesBubble(addonId, DialogList)
                                                        HTMLViewerEditIcon = getHTMLViewerBubble(addonId, "editWrapper" & cpCore.doc.editWrapperCnt, DialogList)
                                                        HelpIcon = getHelpBubble(addonId, helpCopy, addon.CollectionID, DialogList)
                                                        ToolBar = InstanceSettingsEditIcon & AddonEditIcon & AddonStylesEditIcon & SiteStylesEditIcon & HTMLViewerEditIcon & HelpIcon
                                                        ToolBar = genericController.vbReplace(ToolBar, "&nbsp;", "", 1, 99, vbTextCompare)
                                                        returnVal = cpCore.html.main_GetEditWrapper("<div class=""ccAddonEditTools"">" & ToolBar & "&nbsp;" & addon.name & DialogList & "</div>", returnVal)
                                                        's = GetEditWrapper("<div class=""ccAddonEditCaption"">" & AddonName & "</div><div class=""ccAddonEditTools"">" & ToolBar & "</div>", s)
                                                    ElseIf cpCore.visitProperty.getBoolean("AllowEditing") Then
                                                        returnVal = cpCore.html.main_GetEditWrapper("<div class=""ccAddonEditCaption"">" & addon.name & "&nbsp;" & HelpIcon & "</div>", returnVal)
                                                    End If
                                                End If
                                            End If
                                            ' moved to calling routines - so if this is called from an add-on without context, the data may not be html
                                            '
                                            ' Add Comment wrapper - to help debugging except email, remote methods and admin (empty is used to detect no result)
                                            '
                                            If True And (addonType <> CPUtilsBaseClass.addonContext.ContextAdmin) And (addonType <> CPUtilsBaseClass.addonContext.ContextEmail) And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodHtml) And (addonType <> CPUtilsBaseClass.addonContext.ContextRemoteMethodJson) And (addonType <> CPUtilsBaseClass.addonContext.ContextSimple) Then
                                                If cpCore.visitProperty.getBoolean("AllowDebugging") Then
                                                    AddonCommentName = genericController.vbReplace(addon.name, "-->", "..>")
                                                    If IsInline Then
                                                        returnVal = "<!-- Add-on " & AddonCommentName & " -->" & returnVal & "<!-- /Add-on " & AddonCommentName & " -->"
                                                    Else
                                                        returnVal = "" _
                                                    & cr & "<!-- Add-on " & AddonCommentName & " -->" _
                                                    & htmlIndent(returnVal) _
                                                    & cr & "<!-- /Add-on " & AddonCommentName & " -->"
                                                    End If
                                                End If
                                            End If
                                            '
                                            ' Add Design Wrapper
                                            '
                                            If (returnVal <> "") And (Not IsInline) And (WrapperID > 0) And (True) Then
                                                returnVal = addWrapperToResult(returnVal, WrapperID, "for Add-on " & addon.name)
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
                    End If
                    optionString = PushOptionString
                    '
                    cpCore.pageAddonCnt = cpCore.pageAddonCnt + 1
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return returnVal
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' execute the xml part of an addon, return html
        ''' </summary>
        ''' <param name="nothingObject"></param>
        ''' <param name="FormXML"></param>
        ''' <param name="return_ExitAddonBlankWithResponse"></param>
        ''' <returns></returns>
        Private Function getFormContent(ByVal nothingObject As Object, ByVal FormXML As String, ByRef return_ExitAddonBlankWithResponse As Boolean) As String
            Dim result As String = ""
            Try
                '
                'Const LoginMode_None = 1
                'Const LoginMode_AutoRecognize = 2
                'Const LoginMode_AutoLogin = 3
                Dim FieldCount As Integer
                Dim RowMax As Integer
                Dim ColumnMax As Integer
                Dim SQLPageSize As Integer
                Dim ErrorNumber As Integer
                Dim ErrorDescription As String
                Dim something As Object(,) = {}
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
                Dim loadOK As Boolean = True
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
                    return_ExitAddonBlankWithResponse = True
                    Return String.Empty
                ElseIf Not cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
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
                                                                FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                FieldValue = cpCore.docProperties.getText(FieldName)
                                                                fieldType = csv_GetXMLAttribute(IsFound, TabNode, "type", "")
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
                                                                FieldReadOnly = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                    FieldHTML = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "html", "false"))
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
                                                                        CS = cpCore.db.cs_insertRecord("Copy Content", cpCore.authContext.user.id)
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
                                                                FieldReadOnly = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                    fieldfilename = csv_GetXMLAttribute(IsFound, TabNode, "filename", "")
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
                                    Return String.Empty
                                End If
                                '
                                ' ----- Display Form
                                '
                                Content.Add(Adminui.EditTableOpen)
                                Name = csv_GetXMLAttribute(IsFound, Doc.DocumentElement, "name", "")
                                With Doc.DocumentElement
                                    For Each SettingNode In .ChildNodes
                                        Select Case genericController.vbLCase(SettingNode.Name)
                                            Case "description"
                                                Description = SettingNode.InnerText
                                            Case "tab"
                                                TabCnt = TabCnt + 1
                                                TabName = csv_GetXMLAttribute(IsFound, SettingNode, "name", "")
                                                TabDescription = csv_GetXMLAttribute(IsFound, SettingNode, "description", "")
                                                TabHeading = csv_GetXMLAttribute(IsFound, SettingNode, "heading", "")
                                                If TabHeading = "Debug and Trace Settings" Then
                                                    TabHeading = TabHeading
                                                End If
                                                TabCell = New stringBuilderLegacyController
                                                For Each TabNode In SettingNode.ChildNodes
                                                    Select Case genericController.vbLCase(TabNode.Name)
                                                        Case "heading"
                                                            '
                                                            ' Heading
                                                            '
                                                            FieldCaption = csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            Call TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption))
                                                        Case "siteproperty"
                                                            '
                                                            ' Site property
                                                            '
                                                            FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldHTML = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                                fieldType = csv_GetXMLAttribute(IsFound, TabNode, "type", "")
                                                                FieldSelector = csv_GetXMLAttribute(IsFound, TabNode, "selector", "")
                                                                FieldDescription = csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                                FieldAddon = csv_GetXMLAttribute(IsFound, TabNode, "EditorAddon", "")
                                                                FieldDefaultValue = TabNode.InnerText
                                                                FieldValue = cpCore.siteProperties.getText(FieldName, FieldDefaultValue)
                                                                If FieldAddon <> "" Then
                                                                    '
                                                                    ' Use Editor Addon
                                                                    '
                                                                    Dim arguments As New Dictionary(Of String, String)
                                                                    arguments.Add("FieldName", FieldName)
                                                                    arguments.Add("FieldValue", cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                    'OptionString = "FieldName=" & FieldName & "&FieldValue=" & encodeNvaArgument(cpCore.siteProperties.getText(FieldName, FieldDefaultValue))
                                                                    Dim addon As addonModel = addonModel.createByName(cpCore, FieldAddon)
                                                                    Copy = cpCore.addon.execute(addon, New CPUtilsBaseClass.addonExecuteContext() With {
                                                                        .addonType = CPUtilsBaseClass.addonContext.ContextAdmin,
                                                                        .instanceArguments = arguments
                                                                    })
                                                                    'Copy = execute_legacy5(0, FieldAddon, OptionString, CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", 0)
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
                                                                                    Copy = cpCore.html.html_GetFormInputHTML(FieldName, FieldValue)
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
                                                                                Dim tmp As String
                                                                                tmp = cpCore.html.html_GetFormInputHidden(FieldName, FieldValue)
                                                                                Copy = FieldValue & tmp
                                                                            Else
                                                                                If FieldHTML Then
                                                                                    Copy = cpCore.html.html_GetFormInputHTML(FieldName, FieldValue)
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
                                                            FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldDescription = csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                                FieldHTML = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                                '
                                                                CS = cpCore.db.cs_open("Copy Content", "Name=" & cpCore.db.encodeSQLText(FieldName), "ID", , , , , "id,name,Copy")
                                                                If Not cpCore.db.cs_ok(CS) Then
                                                                    Call cpCore.db.cs_Close(CS)
                                                                    CS = cpCore.db.cs_insertRecord("Copy Content", cpCore.authContext.user.id)
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
                                                                    Copy = cpCore.html.html_GetFormInputHTML3(FieldName, FieldValue)
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
                                                            FieldName = csv_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            FieldCaption = csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            fieldfilename = csv_GetXMLAttribute(IsFound, TabNode, "filename", "")
                                                            FieldReadOnly = genericController.EncodeBoolean(csv_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldDescription = csv_GetXMLAttribute(IsFound, TabNode, "description", "")
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
                                                            FieldDataSource = csv_GetXMLAttribute(IsFound, TabNode, "DataSourceName", "")
                                                            FieldSQL = TabNode.InnerText
                                                            FieldCaption = csv_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            FieldDescription = csv_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            SQLPageSize = genericController.EncodeInteger(csv_GetXMLAttribute(IsFound, TabNode, "rowmax", ""))
                                                            If SQLPageSize = 0 Then
                                                                SQLPageSize = 100
                                                            End If
                                                            '
                                                            ' Run the SQL
                                                            '
                                                            Dim dt As DataTable = Nothing
                                                            If FieldSQL <> "" Then
                                                                Try
                                                                    dt = cpCore.db.executeSql(FieldSQL, FieldDataSource, , SQLPageSize)
                                                                    'RS = app.csv_ExecuteSQLCommand(FieldDataSource, FieldSQL, 30, SQLPageSize, 1)

                                                                Catch ex As Exception

                                                                    ErrorNumber = Err.Number
                                                                    ErrorDescription = Err.Description
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
                                                                ElseIf (Not isDataTableOk(dt)) Then
                                                                    '
                                                                    ' ----- no result
                                                                    '
                                                                    Copy = "No Results"
                                                                ElseIf (dt.Rows.Count = 0) Then
                                                                    '
                                                                    ' ----- no result
                                                                    '
                                                                    Copy = "No Results"
                                                                Else
                                                                    '
                                                                    ' ----- print results
                                                                    '
                                                                    If dt.Rows.Count > 0 Then
                                                                        If dt.Rows.Count = 1 And dt.Columns.Count = 1 Then
                                                                            Copy = cpCore.html.html_GetFormInputText2("result", genericController.encodeText(something(0, 0)), , , , , True)
                                                                        Else
                                                                            For Each dr As DataRow In dt.Rows
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
                                                                                            Copy = Copy & (ColumnStart & genericController.encodeHTML(genericController.encodeText(CellData)) & ColumnEnd)
                                                                                        End If
                                                                                    Next
                                                                                    Copy = Copy & (RowEnd)
                                                                                Next
                                                                                Copy = Copy & (cr & "</table>")
                                                                            Next
                                                                        End If
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
                                    Content.Add(cpCore.html.main_GetLiveTabs())
                                End If
                            End If
                        End If
                    End If
                End If
                '
                result = Adminui.GetBody(Name, ButtonList, "", True, True, Description, "", 0, Content.Text)
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
        Private Function getFormContent_decodeSelector(ByVal nothingObject As Object, ByVal SitePropertyName As String, ByVal SitePropertyValue As String, ByVal selector As String) As String
            Dim result As String = ""
            Try
                Dim ExpandedSelector As String = ""
                Dim ignore As String = ""
                Dim OptionCaption As String
                Dim OptionValue As String
                Dim OptionValue_AddonEncoded As String
                Dim OptionPtr As Integer
                Dim OptionCnt As Integer
                Dim OptionValues() As String
                Dim OptionSuffix As String = String.Empty
                Dim LCaseOptionDefault As String
                Dim Pos As Integer
                Dim FastString As stringBuilderLegacyController
                Dim Copy As String = String.Empty
                '
                FastString = New stringBuilderLegacyController
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
                    result = ""
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
                                        result = result & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ checked=""checked"">" & OptionCaption & "</div>"
                                    Else
                                        result = result & "<div style=""white-space:nowrap""><input type=""checkbox"" name=""" & SitePropertyName & OptionPtr & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                    End If
                                Case "radio"
                                    '
                                    ' Create Radio addon_execute_getFormContent_decodeSelector
                                    '
                                    If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                        result = result & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ checked=""checked"" >" & OptionCaption & "</div>"
                                    Else
                                        result = result & "<div style=""white-space:nowrap""><input type=""radio"" name=""" & SitePropertyName & """ value=""" & OptionValue & """ >" & OptionCaption & "</div>"
                                    End If
                                Case Else
                                    '
                                    ' Create select addon_execute_result
                                    '
                                    If genericController.vbLCase(OptionValue) = LCaseOptionDefault Then
                                        result = result & "<option value=""" & OptionValue & """ selected>" & OptionCaption & "</option>"
                                    Else
                                        result = result & "<option value=""" & OptionValue & """>" & OptionCaption & "</option>"
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
                            ' Create Radio addon_execute_result
                            '
                            'addon_execute_result = "<div>" & genericController.vbReplace(addon_execute_result, "><", "></div><div><") & "</div>"
                        Case Else
                            '
                            ' Create select addon_execute_result
                            '
                            result = "<select name=""" & SitePropertyName & """>" & result & "</select>"
                    End Select
                Else
                    '
                    ' Create Text addon_execute_result
                    '

                    selector = genericController.decodeNvaArgument(selector)
                    result = cpCore.html.html_GetFormInputText2(SitePropertyName, selector, 1, 20)
                End If

                FastString = Nothing
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
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
        Private Function executeScript(ByRef addon As Models.Entity.addonModel, ByVal Language As String, ByVal Code As String, ByVal EntryPoint As String, ByRef return_errorMessage As String, ByVal ScriptingTimeout As Integer, ByVal ScriptName As String, ByVal ReplaceCnt As Integer, ByVal ReplaceNames() As String, ByVal ReplaceValues() As String) As String
            Dim returnText As String = ""
            Try
                Dim Lines() As String
                Dim sc As New MSScriptControl.ScriptControl
                Dim Args As String() = {}
                Dim Pos As Integer
                Dim EntryPointArgs As String = String.Empty
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
                    Throw New ApplicationException(return_errorMessage, ex)
                End Try
                If String.IsNullOrEmpty(return_errorMessage) Then
                    Try
                        Dim mainCsv As New mainCsvCompatibilityClass(cpCore)
                        Call sc.AddObject("ccLib", mainCsv)
                    Catch ex As Exception
                        '
                        ' Error adding cclib object
                        '
                        return_errorMessage = "Error adding cclib compatibility object to script environment"
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
                        Throw New ApplicationException(return_errorMessage, ex)
                    End Try
                    If String.IsNullOrEmpty(return_errorMessage) Then
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
                            Dim addonDescription As String = getAddonDescription(cpCore, addon)
                            return_errorMessage &= ", " & addonDescription
                            Throw New ApplicationException(return_errorMessage, ex)
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
                                            Throw New ApplicationException("Unexpected exception") ' Call cpcore.handleLegacyError6("csv_ExecuteScript4", "Scripting only supports 10 arguments.")
                                    End Select
                                End If
                            Catch ex As Exception
                                Dim addonDescription As String = getAddonDescription(cpCore, addon)
                                return_errorMessage = "Error executing script [" & ScriptName & "], " & addonDescription
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
                                Throw New ApplicationException(return_errorMessage, ex)
                            End Try
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnText
        End Function
        '
        '
        '
        Private Function executeAssembly(addon As Models.Entity.addonModel, addonCollection As Models.Entity.AddonCollectionModel, ByRef return_ErrorMessageForAdmin As String) As String
            Dim result As String = ""
            Try
                Dim AddonFound As Boolean = False
                Dim commonAssemblyPath As String = cpCore.programDataFiles.rootLocalPath & "AddonAssemblyBypass\"
                If Not IO.Directory.Exists(commonAssemblyPath) Then
                    IO.Directory.CreateDirectory(commonAssemblyPath)
                Else
                    result = executeAssembly_byFilePath(addon.id, addon.name, commonAssemblyPath, addon.DotNetClass, True, AddonFound, return_ErrorMessageForAdmin)
                End If
                If Not AddonFound Then
                    '
                    ' try app /bin folder
                    '
                    Dim addonAppRootPath As String = cpCore.privateFiles.joinPath(cpCore.appRootFiles.rootLocalPath, "bin\")
                    result = executeAssembly_byFilePath(addon.id, addon.name, addonAppRootPath, addon.DotNetClass, True, AddonFound, return_ErrorMessageForAdmin)
                    If Not AddonFound Then
                        '
                        ' legacy mode, consider eliminating this and storing addon binaries in apps /bin folder
                        '
                        If String.IsNullOrEmpty(addonCollection.ccguid) Then
                            Throw New ApplicationException("The assembly for addon [" & addon.name & "] could not be executed because it's collection has an invalid guid.")
                        Else
                            Dim AddonVersionPath As String = ""
                            Dim addonInstall As New addonInstallClass(cpCore)
                            Call addonInstall.GetCollectionConfig(addonCollection.ccguid, AddonVersionPath, New Date(), "")
                            If (String.IsNullOrEmpty(AddonVersionPath)) Then
                                Throw New ApplicationException("The assembly for addon [" & addon.name & "] could not be executed because it's assembly could not be found in cclibCommonAssemblies, and no collection folder was found.")
                            Else
                                Dim AddonPath As String = cpCore.privateFiles.joinPath(getPrivateFilesAddonPath(), AddonVersionPath)
                                Dim appAddonPath As String = cpCore.privateFiles.joinPath(cpCore.privateFiles.rootLocalPath, AddonPath)
                                result = executeAssembly_byFilePath(addon.id, addon.name, appAddonPath, addon.DotNetClass, False, AddonFound, return_ErrorMessageForAdmin)
                                If (Not AddonFound) Then
                                    '
                                    ' assembly not found in addon path and in development path, if core collection, try in local /bin nm 
                                    '
                                    If (addonCollection.ccguid <> CoreCollectionGuid) Then
                                        '
                                        ' assembly not found
                                        '
                                        Throw New ApplicationException("The addon [" & addon.name & "] could not be executed because it's assembly could not be found in the server common assembly path [" & commonAssemblyPath & "], the application binary folder [" & addonAppRootPath & "], or in the legacy collection folder [" & appAddonPath & "].")
                                    Else
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
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
                'Dim filePtr As Integer
                'Dim dllFilenames() As String
                Dim testAssembly As [Assembly] = Nothing
                Dim testAssemblyType As Type = Nothing
                Dim objInterface As Type = Nothing
                Dim AddonObj As AddonBaseClass
                'Dim addonObj As Object
                Dim IsClassFound As Boolean = False
                'Dim TestFilePathname As String = ""
                Dim AddonReturnObj As Object
                Dim detailedErrorMessage As String = ""
                Dim testFileIsValidAddonAssembly As Boolean
                Dim testAssemblyName As String
                '
                ' If not found in debug location (addon root folder), look in addon version folder provided
                '
                AddonFound = False
                '
                ' refactor -- add an argument byref dictionary cache, loaded as you go through the types in each dll. Next load, use the dictionary to locate the class faster. 
                '
                If IO.Directory.Exists(fullPath) Then
                    'dllFilenames = IO.Directory.GetFileSystemEntries(fullPath, "*.dll")
                    For Each TestFilePathname In IO.Directory.GetFileSystemEntries(fullPath, "*.dll")
                        If (Not cpCore.assemblySkipList.Contains(TestFilePathname)) Then
                            testFileIsValidAddonAssembly = True
                            Try
                                '
                                ' ##### consider using refectiononlyload first, then if it is right, do the loadfrom - so Dependencies are not loaded.
                                '
                                testAssembly = System.Reflection.Assembly.LoadFrom(TestFilePathname)
                                testAssemblyName = testAssembly.FullName
                            Catch ex As Exception
                                cpCore.assemblySkipList.Add(TestFilePathname)
                                testFileIsValidAddonAssembly = False
                            End Try
                            Try
                                If testFileIsValidAddonAssembly Then
                                    '
                                    ' problem loading types, use try to debug
                                    '
                                    Try
                                        Dim isAddonAssembly As Boolean = False
                                        For Each testAssemblyType In testAssembly.GetTypes
                                            '
                                            ' Loop through each type in the Assembly looking for our typename, public, and non-abstract
                                            '
                                            If (testAssemblyType.IsPublic) And (Not ((testAssemblyType.Attributes And TypeAttributes.Abstract) = TypeAttributes.Abstract)) And (testAssemblyType.BaseType IsNot Nothing) Then
                                                '
                                                ' -- assembly is public, not abstract, based on a base type
                                                If (testAssemblyType.BaseType.FullName IsNot Nothing) Then
                                                    '
                                                    ' -- assembly has a baseType fullname
                                                    If ((testAssemblyType.BaseType.FullName.ToLower = "addonbaseclass") Or (testAssemblyType.BaseType.FullName.ToLower = "contensive.baseclasses.addonbaseclass")) Then
                                                        '
                                                        ' -- valid addon assembly
                                                        isAddonAssembly = True
                                                        If ((testAssemblyType.FullName.Trim.ToLower = typeFullName.Trim.ToLower)) Then
                                                            '
                                                            ' -- found the correct addon assembly
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
                                                                    cpCore.handleException(Ex, detailedErrorMessage)
                                                                    'Throw New ApplicationException(detailedErrorMessage)
                                                                End Try
                                                            Catch Ex As Exception
                                                                return_userErrorMessage = "There was an error initializing the addon's Dot Net DLL."
                                                                detailedErrorMessage = AddonDisplayName & " could not be executed because there was an error creating an object from the assembly, DLL [" & testAssemblyType.FullName & "]. The error was [" & Ex.ToString() & "]"
                                                                Throw New ApplicationException(detailedErrorMessage)
                                                            End Try
                                                            '
                                                            ' -- addon was found, no need to look for more
                                                            Exit For
                                                        End If
                                                    End If
                                                End If
                                            End If
                                        Next
                                        If (Not isAddonAssembly) Then
                                            '
                                            ' -- not an addon assembly
                                            cpCore.assemblySkipList.Add(TestFilePathname)
                                        End If
                                    Catch ex As ReflectionTypeLoadException
                                        '
                                        ' exceptin thrown out of application bin folder when xunit library included -- ignore
                                        '
                                        cpCore.assemblySkipList.Add(TestFilePathname)
                                    Catch ex As Exception
                                        '
                                        ' problem loading types
                                        '
                                        cpCore.assemblySkipList.Add(TestFilePathname)
                                        detailedErrorMessage = "While locating assembly for addon [" & AddonDisplayName & "], there was an error loading types for assembly [" & testAssemblyType.FullName & "]. This assembly was skipped and should be removed from the folder [" & fullPath & "]"
                                        Throw New ApplicationException(detailedErrorMessage)
                                    End Try
                                End If
                            Catch ex As Reflection.ReflectionTypeLoadException
                                cpCore.assemblySkipList.Add(TestFilePathname)
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
                                cpCore.assemblySkipList.Add(TestFilePathname)
                                return_userErrorMessage = "There was an unknown error in the addon's Dot Net DLL [" & AddonDisplayName & "]."
                                detailedErrorMessage = "A non-load exception occured while loading the addon [" & AddonDisplayName & "], DLL [" & testAssemblyType.FullName & "]. The error was [" & ex.ToString() & "]."
                                cpCore.handleException(New ApplicationException(detailedErrorMessage))
                            End Try
                        End If
                    Next
                    'If dllFilenames.Length > 0 Then
                    '    '
                    '    ' search the list for the correct assembly
                    '    '
                    '    filePtr = 0
                    '    Do
                    '        TestFilePathname = dllFilenames(filePtr)
                    '        filePtr += 1
                    '    Loop While (Not AddonFound) And (filePtr < dllFilenames.Length)
                    'End If
                End If
            Catch ex As Exception
                '
                ' -- this exception should interrupt the caller
                cpCore.handleException(ex) : Throw
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
            Dim result As String = ""
            Try
                Dim addon As Models.Entity.addonModel = Nothing
                If (EncodeInteger(AddonIDGuidOrName) > 0) Then
                    addon = cpCore.addonCache.getAddonById(EncodeInteger(AddonIDGuidOrName))
                ElseIf (genericController.isGuid(AddonIDGuidOrName)) Then
                    addon = cpCore.addonCache.getAddonByGuid(AddonIDGuidOrName)
                Else
                    addon = cpCore.addonCache.getAddonByName(AddonIDGuidOrName)
                End If
                If (addon IsNot Nothing) Then
                    '
                    ' -- addon found
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "start: add process to background cmd queue, addon [" & addon.name & "/" & addon.id & "], optionstring [" & OptionString & "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Err.Number, Err.Source, Err.Description, False, True, "", "process", "")
                    '
                    Dim cmdQueryString As String = "" _
                        & "appname=" & encodeNvaArgument(EncodeRequestVariable(cpCore.serverConfig.appConfig.name)) _
                        & "&AddonID=" & CStr(addon.id) _
                        & "&OptionString=" & encodeNvaArgument(EncodeRequestVariable(OptionString))
                    Dim taskScheduler As New taskSchedulerController()
                    Dim cmdDetail As New cmdDetailClass
                    cmdDetail.addonId = addon.id
                    cmdDetail.addonName = addon.name
                    cmdDetail.docProperties = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, cmdQueryString)
                    Call taskScheduler.addTaskToQueue(cpCore, taskQueueCommandEnumModule.runAddon, cmdDetail, False)
                    '
                    logController.appendLogWithLegacyRow(cpCore, cpCore.serverConfig.appConfig.name, "end: add process to background cmd queue, addon [" & addon.name & "/" & addon.id & "], optionstring [" & OptionString & "]", "dll", "cpCoreClass", "csv_ExecuteAddonAsProcess", Err.Number, Err.Source, Err.Description, False, True, "", "process", "")
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        ''
        ''=============================================================================================================
        ''   cpcore.main_Get Addon Content
        '' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        ''=============================================================================================================
        ''
        'Public Function execute_legacy5(ByVal addonId As Integer, ByVal AddonName As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String, ByVal ACInstanceID As Integer) As String
        '    Dim AddonStatusOK As Boolean
        '    execute_legacy5 = execute_legacy2(addonId, AddonName, Option_String, Context, ContentName, RecordID, FieldName, CStr(ACInstanceID), False, 0, "", AddonStatusOK, Nothing)
        'End Function
        ''
        ''====================================================================================================
        '' Public Interface
        '' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        ''====================================================================================================
        ''
        'Public Function execute_legacy1(ByVal addonId As Integer, ByVal AddonNameOrGuid As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal HostContentName As String, ByVal HostRecordID As Integer, ByVal HostFieldName As String, ByVal ACInstanceID As String, ByVal DefaultWrapperID As Integer) As String
        '    Dim AddonStatusOK As Boolean
        '    Dim workingContext As CPUtilsBaseClass.addonContext
        '    '
        '    workingContext = Context
        '    If workingContext = 0 Then
        '        workingContext = CPUtilsBaseClass.addonContext.ContextPage
        '    End If
        '    execute_legacy1 = execute_legacy2(addonId, AddonNameOrGuid, Option_String, workingContext, HostContentName, HostRecordID, HostFieldName, ACInstanceID, False, DefaultWrapperID, "", AddonStatusOK, Nothing)
        'End Function
        ''
        ''====================================================================================================
        '' Public Interface to support AsProcess
        ''   Programmatic calls to executeAddon would not require Context, HostContent, etc because the host would be an add-on, and the
        ''   addon has control or settings, not the administrator
        '' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        ''====================================================================================================
        ''
        'Public Function execute_legacy3(ByVal AddonIDGuidOrName As String, Optional ByVal Option_String As String = "", Optional ByVal WrapperID As Integer = 0) As String
        '    Dim AddonStatusOK As Boolean
        '    If genericController.vbIsNumeric(AddonIDGuidOrName) Then
        '        Return execute_legacy2(EncodeInteger(AddonIDGuidOrName), "", Option_String, CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, WrapperID, "", AddonStatusOK, Nothing)
        '    Else
        '        Return execute_legacy2(0, AddonIDGuidOrName, Option_String, CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, WrapperID, "", AddonStatusOK, Nothing)
        '    End If
        'End Function
        ''
        '' Public Interface to support AsProcess
        ''
        'Public Function execute_legacy4(ByVal AddonIDGuidOrName As String, Optional ByVal Option_String As String = "", Optional ByVal Context As CPUtilsBaseClass.addonContext = CPUtilsBaseClass.addonContext.ContextPage, Optional ByVal nothingObject As Object = Nothing) As String
        '    Dim AddonStatusOK As Boolean
        '    Dim workingContext As CPUtilsBaseClass.addonContext
        '    '
        '    workingContext = Context
        '    If workingContext = 0 Then
        '        workingContext = CPUtilsBaseClass.addonContext.ContextPage
        '    End If
        '    If genericController.vbIsNumeric(AddonIDGuidOrName) Then
        '        execute_legacy4 = execute_legacy2(EncodeInteger(AddonIDGuidOrName), "", Option_String, workingContext, "", 0, "", "", False, 0, "", AddonStatusOK, nothingObject)
        '    Else
        '        execute_legacy4 = execute_legacy2(0, AddonIDGuidOrName, Option_String, workingContext, "", 0, "", "", False, 0, "", AddonStatusOK, nothingObject)
        '    End If
        'End Function
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
        ''
        ''=============================================================================================================
        ''   cpcore.main_Get Addon Content - internal (to support include add-ons)
        '' REFACTOR - unify interface, remove cpcore.main_ and csv_ class references
        ''=============================================================================================================
        ''
        'Public Function execute_legacy2(ByVal addonId As Integer, ByVal AddonNameOrGuid As String, ByVal Option_String As String, ByVal Context As CPUtilsBaseClass.addonContext, ByVal HostContentName As String, ByVal HostRecordID As Integer, ByVal HostFieldName As String, ByVal ACInstanceID As String, ByVal IsIncludeAddon As Boolean, ByVal DefaultWrapperID As Integer, ByVal ignore_TemplateCaseOnly_PageContent As String, ByRef ignore As Boolean, ByVal nothingObject As Object, Optional ByVal AddonInUseIdList As String = "") As String
        '    execute_legacy2 = execute_legacy6(addonId, AddonNameOrGuid, Option_String, Context, HostContentName, HostRecordID, HostFieldName, ACInstanceID, IsIncludeAddon, DefaultWrapperID, ignore_TemplateCaseOnly_PageContent, False, nothingObject, AddonInUseIdList, Nothing, cpCore.doc.includedAddonIDList, cpCore.authContext.user.id, cpCore.authContext.isAuthenticated)
        'End Function
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
            If cpCore.authContext.isAuthenticated() And ((ACInstanceID = "-2") Or (ACInstanceID = "-1") Or (ACInstanceID = "0") Or (RecordID <> 0)) Then
                If cpCore.authContext.isEditingAnything() Then
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
                        Call cpCore.html.addOnLoadJavascript("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
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
                If cpCore.authContext.isAuthenticated() And True Then
                    If cpCore.authContext.isEditingAnything() Then
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
                            & "<tr><td style=""padding-bottom:5px;"" class=""ccContentCon ccAdminSmall""><b>Custom Stylesheet</b>" & cpCore.html.html_GetFormInputTextExpandable2("CustomStyles", addon.StylesFilename.copy, 10, "400px") & "</td></tr>"
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
            If cpCore.authContext.isAuthenticated() Then
                If cpCore.authContext.isEditingAnything() Then
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
                        Call cpCore.html.addOnLoadJavascript("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
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
            If cpCore.authContext.isAuthenticated() Then
                If cpCore.authContext.isEditingAnything() Then
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
                        Call cpCore.html.addOnLoadJavascript("jQuery(function(){jQuery('.helpDialogCon').draggable()})", "draggable dialogs")
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
                ElseIf Not cpCore.authContext.isAuthenticatedAdmin(cpCore) Then
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
                                                                FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                FieldValue = cpCore.docProperties.getText(FieldName)
                                                                fieldType = main_GetXMLAttribute(IsFound, TabNode, "type", "")
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
                                                                FieldReadOnly = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                    FieldHTML = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "html", "false"))
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
                                                                FieldReadOnly = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                If Not FieldReadOnly Then
                                                                    FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                                    fieldfilename = main_GetXMLAttribute(IsFound, TabNode, "filename", "")
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
                                Name = main_GetXMLAttribute(IsFound, Doc.DocumentElement, "name", "")
                                With Doc.DocumentElement
                                    For Each SettingNode In .ChildNodes
                                        Select Case genericController.vbLCase(SettingNode.Name)
                                            Case "description"
                                                Description = SettingNode.InnerText
                                            Case "tab"
                                                TabCnt = TabCnt + 1
                                                TabName = main_GetXMLAttribute(IsFound, SettingNode, "name", "")
                                                TabDescription = main_GetXMLAttribute(IsFound, SettingNode, "description", "")
                                                TabHeading = main_GetXMLAttribute(IsFound, SettingNode, "heading", "")
                                                TabCell = New stringBuilderLegacyController
                                                For Each TabNode In SettingNode.ChildNodes
                                                    Select Case genericController.vbLCase(TabNode.Name)
                                                        Case "heading"
                                                            '
                                                            ' Heading
                                                            '
                                                            FieldCaption = main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            Call TabCell.Add(Adminui.GetEditSubheadRow(FieldCaption))
                                                        Case "siteproperty"
                                                            '
                                                            ' Site property
                                                            '
                                                            FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldHTML = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                                fieldType = main_GetXMLAttribute(IsFound, TabNode, "type", "")
                                                                FieldSelector = main_GetXMLAttribute(IsFound, TabNode, "selector", "")
                                                                FieldDescription = main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                                FieldAddon = main_GetXMLAttribute(IsFound, TabNode, "EditorAddon", "")
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
                                                                                    Copy = cpCore.html.html_GetFormInputHTML(FieldName, FieldValue)
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
                                                                                    Copy = cpCore.html.html_GetFormInputHTML(FieldName, FieldValue)
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
                                                            FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            If FieldName <> "" Then
                                                                FieldCaption = main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                                If FieldCaption = "" Then
                                                                    FieldCaption = FieldName
                                                                End If
                                                                FieldReadOnly = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                                FieldDescription = main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                                FieldHTML = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "html", ""))
                                                                '
                                                                CS = cpCore.db.cs_open("Copy Content", "Name=" & cpCore.db.encodeSQLText(FieldName), "ID", , , , , "Copy")
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
                                                                    Copy = cpCore.html.html_GetFormInputHTML3(FieldName, FieldValue)
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
                                                            FieldName = main_GetXMLAttribute(IsFound, TabNode, "name", "")
                                                            FieldCaption = main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            fieldfilename = main_GetXMLAttribute(IsFound, TabNode, "filename", "")
                                                            FieldReadOnly = genericController.EncodeBoolean(main_GetXMLAttribute(IsFound, TabNode, "readonly", ""))
                                                            FieldDescription = main_GetXMLAttribute(IsFound, TabNode, "description", "")
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
                                                            FieldDataSource = main_GetXMLAttribute(IsFound, TabNode, "DataSourceName", "")
                                                            FieldSQL = TabNode.InnerText
                                                            FieldCaption = main_GetXMLAttribute(IsFound, TabNode, "caption", "")
                                                            FieldDescription = main_GetXMLAttribute(IsFound, TabNode, "description", "")
                                                            SQLPageSize = genericController.EncodeInteger(main_GetXMLAttribute(IsFound, TabNode, "rowmax", ""))
                                                            If SQLPageSize = 0 Then
                                                                SQLPageSize = 100
                                                            End If
                                                            '
                                                            ' Run the SQL
                                                            '
                                                            Dim dt As DataTable = Nothing

                                                            If FieldSQL <> "" Then
                                                                Try
                                                                    dt = cpCore.db.executeSql(FieldSQL, FieldDataSource, , SQLPageSize)
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
            Dim ignore As String = String.Empty
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
        Public Sub buildAddonOptionLists2(ByRef OptionString_ForObjectCall As String, ByRef AddonOptionExpandedConstructor As String, AddonOptionConstructor As String, addonOptionString As String, InstanceID As String, IncludeSettingsBubbleOptions As Boolean)
            Try
                '
                Dim SavePtr As Integer
                Dim InstanceTypes() As String
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
                Dim ConstructorNameValues As String() = {}
                Dim ConstructorNames As String() = {}
                Dim ConstructorSelectors As String() = {}
                Dim ConstructorValues As String() = {}
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
                        AddonOptionExpandedConstructor = AddonOptionExpandedConstructor & vbCrLf & cpCore.html.getAddonSelector(ConstructorName, ConstructorValue, ConstructorSelector)
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
        Public Sub buildAddonOptionLists(ByRef Option_String_ForObjectCall As String, ByRef AddonOptionExpandedConstructor As String, AddonOptionConstructor As String, InstanceOptionList As String, InstanceID As String, IncludeEditWrapper As Boolean)
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
            CS = cpCore.db.csOpenRecord("Wrappers", WrapperID, , , SelectFieldList)
            If cpCore.db.cs_ok(CS) Then
                Wrapper = cpCore.db.cs_getText(CS, "copytext")
                wrapperName = cpCore.db.cs_getText(CS, "name")
                TargetString = cpCore.db.cs_getText(CS, "targetString")
                '
                SourceComment = "wrapper " & wrapperName
                If WrapperSourceForComment <> "" Then
                    SourceComment = SourceComment & " for " & WrapperSourceForComment
                End If
                Call cpCore.html.addOnLoadJavascript(cpCore.db.cs_getText(CS, "javascriptonload"), SourceComment)
                Call cpCore.html.addBodyJavascriptCode(cpCore.db.cs_getText(CS, "javascriptbodyend"), SourceComment)
                Call cpCore.html.doc_AddHeadTag2(cpCore.db.cs_getText(CS, "OtherHeadTags"), SourceComment)
                '
                JSFilename = cpCore.db.cs_getText(CS, "jsfilename")
                If JSFilename <> "" Then
                    JSFilename = cpCore.webServer.requestProtocol & cpCore.webServer.requestDomain & genericController.getCdnFileLink(cpCore, JSFilename)
                    Call cpCore.html.addJavaScriptLinkHead(JSFilename, SourceComment)
                End If
                Copy = cpCore.db.cs_getText(CS, "stylesfilename")
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
            Call cpCore.db.cs_Close(CS)
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
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Function csv_GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            Dim result As String = ""
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
            Return result
        End Function
        '
        '========================================================================
        ' ----- main_Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Function main_GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
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
            Dim collection As Models.Entity.AddonCollectionModel = Models.Entity.AddonCollectionModel.create(cpcore, addon.CollectionID)
            Dim addonDescription As String = "[#" & addon.id.ToString() & ", " & addon.name & "], collection [" & collection.name & "]"
            If (collection Is Nothing) Then
                addonDescription &= ", no collection set"
            Else
                addonDescription &= ", collection [" & collection.name & "]"
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