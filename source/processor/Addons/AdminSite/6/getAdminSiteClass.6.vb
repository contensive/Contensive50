
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Partial Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '
        '====================================================================================================
        ' objects passed in - do not dispose
        '   sets cp from argument For use In calls To other objects, Then cpCore because cp cannot be used since that would be a circular depenancy
        '====================================================================================================
        '
        Private cp As CPClass                   ' local cp set in constructor
        Private cpCore As coreClass           ' cpCore -- short term, this is the migration solution from a built-in tool, to an addon
        '
        '====================================================================================================
        ''' <summary>
        ''' addon method, deliver complete Html admin site
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                '
                ' -- ok to cast cpbase to cp because they build from the same solution
                Me.cp = DirectCast(cp, CPClass)
                cpCore = Me.cp.core
                '
                ' -- log request
                Dim SaveContent As String = "" _
                        & Now() _
                        & vbCrLf & "member.name:" & cpCore.doc.authContext.user.name _
                        & vbCrLf & "member.id:" & cpCore.doc.authContext.user.id _
                        & vbCrLf & "visit.id:" & cpCore.doc.authContext.visit.id _
                        & vbCrLf & "url:" & cpCore.webServer.requestUrl _
                        & vbCrLf & "url source:" & cpCore.webServer.requestUrlSource _
                        & vbCrLf & "----------" _
                        & vbCrLf & "form post:"
                For Each key As String In cpCore.docProperties.getKeyList()
                    Dim docProperty As docPropertiesClass = cpCore.docProperties.getProperty(key)
                    If docProperty.IsForm Then
                        SaveContent &= vbCrLf & docProperty.NameValue
                    End If
                Next
                If Not IsNothing(cpCore.webServer.requestFormBinaryHeader) Then
                    Dim BinaryHeader() As Byte = cpCore.webServer.requestFormBinaryHeader
                    Dim BinaryHeaderString As String = genericController.kmaByteArrayToString(BinaryHeader)
                    SaveContent &= "" _
                            & vbCrLf & "----------" _
                            & vbCrLf & "binary header:" _
                            & vbCrLf & BinaryHeaderString _
                            & vbCrLf
                End If
                logController.appendLog(cpCore, SaveContent, "admin", cpCore.serverConfig.appConfig.name & "-request-")
                '
                If Not cpCore.doc.authContext.isAuthenticated Then
                    '
                    ' --- must be authenticated to continue. Force a local login
                    '
                    returnHtml = cpCore.addon.execute(
                        Models.Entity.addonModel.create(cpCore, addonGuidLoginPage),
                        New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {
                            .errorCaption = "Login Page",
                            .addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextPage
                        }
                    )
                ElseIf Not cpCore.doc.authContext.isAuthenticatedContentManager(cpCore) Then
                    '
                    ' --- member must have proper access to continue
                    '
                    returnHtml = "" _
                        & "<p>" & SpanClassAdminNormal _
                        & "You are attempting to enter an area which your account does not have access." _
                        & cr & "<ul class=""ccList"">" _
                        & cr & "<li class=""ccListItem"">To return to the public web site, use your back button, or <a href=""" & requestAppRootPath & """>Click Here</A>." _
                        & cr & "<li class=""ccListItem"">To login under a different account, <a href=""/" & cpCore.serverConfig.appConfig.adminRoute & "?method=logout"" rel=""nofollow"">Click Here</A>" _
                        & cr & "<li class=""ccListItem"">To have your account access changed to include this area, please contact the <a href=""mailto:" & cpCore.siteProperties.getText("EmailAdmin") & """>system administrator</A>. " _
                        & cr & "</ul>" _
                        & "</span></p>"
                    returnHtml = "" _
                        & cpCore.html.main_GetPanelHeader("Unauthorized Access") _
                        & cpCore.html.main_GetPanel(returnHtml, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15)
                    returnHtml = "" _
                        & cr & "<div style=""display:table;margin:100px auto auto auto;"">" _
                        & genericController.htmlIndent(returnHtml) _
                        & cr & "</div>"
                    '
                    Call cpCore.doc.setMetaContent(0, 0)
                    Call cpCore.html.addTitle("Unauthorized Access", "adminSite")
                    returnHtml = "<div class=""ccBodyAdmin ccCon"">" & returnHtml & "</div>"
                    'returnHtml = cpCore.html.getHtmlDoc(returnHtml, "<body class=""ccBodyAdmin ccCon"">", True, True, False)
                Else
                    '
                    ' get admin content
                    '
                    returnHtml = "<div class=""ccBodyAdmin ccCon"">" & getAdminBody() & "</div>"
                    'returnHtml = cpCore.html.getHtmlDoc(adminBody, "<body class=""ccBodyAdmin ccCon"">", True, True, False)
                End If
                '
                ' Log response
                '
                SaveContent &= "" _
                        & Now() _
                        & vbCrLf & "member.name:" & cpCore.doc.authContext.user.name _
                        & vbCrLf & "member.id:" & cpCore.doc.authContext.user.id _
                        & vbCrLf & "visit.id:" & cpCore.doc.authContext.visit.id _
                        & vbCrLf & "url:" & cpCore.webServer.requestUrl _
                        & vbCrLf & "url source:" & cpCore.webServer.requestUrlSource _
                        & vbCrLf & "----------" _
                        & vbCrLf & "response:" _
                        & vbCrLf & returnHtml
                Dim rightNow As Date = DateTime.Now
                Call logController.appendLog(cpCore, SaveContent, "admin", rightNow.Year & rightNow.Month.ToString("00") & rightNow.Day.ToString("00") & rightNow.Hour.ToString("00") & rightNow.Minute.ToString("00") & rightNow.Second.ToString("00"))
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' REFACTOR - Constructor for addon instances. Until refactoring, calls into other methods must be constructed with (cpCoreClass) variation.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New()
            MyBase.New()
            ClassInitialized = False
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' REFACTOR - Constructor for non-addon instances. (REFACTOR - work-around for pre-refactoring of admin remote methods currently in core classes)
        ''' </summary>
        ''' <param name="cpCore"></param>
        Public Sub New(cp As Contensive.Core.CPClass)
            MyBase.New()
            Me.cp = cp
            cpCore = Me.cp.core
            ClassInitialized = False
        End Sub
        '
        '========================================================================
        '
        Private Function getAdminBody(Optional ContentArgFromCaller As String = "") As String
            Dim result As String = ""
            Try
                Dim DefaultWrapperID As Integer
                Dim AddonHelpCopy As String
                Dim InstanceOptionString As String
                Dim HelpLevel As Integer
                Dim HelpAddonID As Integer
                Dim HelpCollectionID As Integer
                Dim CurrentLink As String
                Dim EditReferer As String
                Dim ContentCell As String
                Dim Stream As New stringBuilderLegacyController
                Dim addonId As Integer
                Dim AddonGuid As String
                Dim AddonName As String = ""
                Dim UseContentWatchLink As Boolean
                Dim editRecord As New editRecordClass
                Dim AdminContent As New Models.Complex.cdefModel
                '
                '-------------------------------------------------------------------------------
                ' Setup defaults
                '-------------------------------------------------------------------------------
                '
                cpCore.db.sqlCommandTimeout = 300
                ButtonObjectCount = 0
                ImagePreloadCount = 0
                JavaScriptString = ""
                ContentWatchLoaded = False
                editRecord.Loaded = False
                UseContentWatchLink = cpCore.siteProperties.useContentWatchLink
                Call cpCore.html.addScriptCode_onLoad("document.getElementsByTagName('BODY')[0].onclick = BodyOnClick;", "Contensive")
                Call cpCore.doc.setMetaContent(0, 0)
                '
                '-------------------------------------------------------------------------------
                ' check for member login, if logged in and no admin, lock out
                ' Do CheckMember here because we need to know who is there to create proper blocked menu
                '-------------------------------------------------------------------------------
                '
                If Not cpCore.doc.continueProcessing Then
                    '
                    ' ----- no stream anyway, do nothing
                    '
                    'ElseIf Not cpCore.doc.authContext.isAuthenticated Then
                    '    '
                    '    ' --- must be authenticated to continue
                    '    '
                    '    Dim loginAddon As New Addons.addon_loginClass(cpCore)
                    '    Stream.Add(loginAddon.getLoginForm())
                Else
                    '
                    ' -- add exception if build verison does not match code
                    If (cpCore.siteProperties.dataBuildVersion <> cp.Version) Then
                        If (cpCore.siteProperties.dataBuildVersion > cp.Version) Then
                            cpCore.handleException(New ApplicationException("Application code version is older than Db version. Run command line upgrade method on this site."))
                        Else
                            cpCore.handleException(New ApplicationException("Application code version is newer than Db version. Upgrade site code."))
                        End If
                    End If
                    '
                    '-------------------------------------------------------------------------------
                    ' Get Requests
                    '   initialize adminContent and editRecord objects 
                    '-------------------------------------------------------------------------------
                    '
                    Call GetForm_LoadControl(AdminContent, editRecord)
                    addonId = cpCore.docProperties.getInteger("addonid")
                    AddonGuid = cpCore.docProperties.getText("addonguid")
                    ''
                    ''-------------------------------------------------------------------------------
                    ''
                    ''-------------------------------------------------------------------------------
                    ''
                    'If AdminContent.fields.Count > 0 Then
                    '    ReDim EditRecordValuesObject(AdminContent.fields.Count)
                    '    ReDim EditRecordDbValues(AdminContent.fields.Count)
                    'End If
                    '
                    '-------------------------------------------------------------------------------
                    ' Process SourceForm/Button into Action/Form, and process
                    '-------------------------------------------------------------------------------
                    '
                    If cpCore.docProperties.getText("Button") = ButtonCancelAll Then
                        AdminForm = AdminFormRoot
                    Else
                        Call ProcessForms(AdminContent, editRecord)
                        Call ProcessActions(AdminContent, editRecord, UseContentWatchLink)
                    End If
                    '
                    '-------------------------------------------------------------------------------
                    ' Normalize values to be needed
                    '-------------------------------------------------------------------------------
                    '
                    If editRecord.id <> 0 Then
                        Call cpCore.workflow.ClearEditLock(AdminContent.Name, editRecord.id)
                    End If
                    '
                    If (AdminForm < 1) Then
                        '
                        ' No form was set, use default form
                        '
                        If AdminContent.Id <= 0 Then
                            AdminForm = AdminFormRoot
                        Else
                            AdminForm = AdminFormIndex
                        End If
                    End If
                    '
                    If AdminForm = AdminFormLegacyAddonManager Then
                        '
                        ' patch out any old links to the legacy addon manager
                        '
                        AdminForm = 0
                        AddonGuid = addonGuidAddonManager
                    End If
                    '
                    '-------------------------------------------------------------------------------
                    ' Edit form but not valid record case
                    ' Put this here so we can display the error without being stuck displaying the edit form
                    ' Putting the error on the edit form is confusing because there are fields to fill in
                    '-------------------------------------------------------------------------------
                    '
                    If (AdminSourceForm = AdminFormEdit) Then
                        If (Not (cpCore.doc.debug_iUserError <> "")) And ((AdminButton = ButtonOK) Or (AdminButton = ButtonCancel) Or (AdminButton = ButtonDelete)) Then
                            EditReferer = cpCore.docProperties.getText("EditReferer")
                            CurrentLink = genericController.modifyLinkQuery(cpCore.webServer.requestUrl, "editreferer", "", False)
                            CurrentLink = genericController.vbLCase(CurrentLink)
                            '
                            ' check if this editreferer includes cid=thisone and id=thisone -- if so, go to index form for this cid
                            '
                            If (EditReferer <> "") And (LCase(EditReferer) <> CurrentLink) Then
                                '
                                ' return to the page it came from
                                '
                                Return cpCore.webServer.redirect(EditReferer, "Admin Edit page returning to the EditReferer setting")
                            Else
                                '
                                ' return to the index page for this content
                                '
                                AdminForm = AdminFormIndex
                            End If
                        End If
                        If BlockEditForm Then
                            AdminForm = AdminFormIndex
                        End If
                    End If
                    HelpLevel = cpCore.docProperties.getInteger("helplevel")
                    HelpAddonID = cpCore.docProperties.getInteger("helpaddonid")
                    HelpCollectionID = cpCore.docProperties.getInteger("helpcollectionid")
                    If HelpCollectionID = 0 Then
                        HelpCollectionID = cpCore.visitProperty.getInteger("RunOnce HelpCollectionID")
                        If HelpCollectionID <> 0 Then
                            Call cpCore.visitProperty.setProperty("RunOnce HelpCollectionID", "")
                        End If
                    End If
                    '
                    '-------------------------------------------------------------------------------
                    ' build refresh string
                    '-------------------------------------------------------------------------------
                    '
                    If AdminContent.Id <> 0 Then Call cpCore.doc.addRefreshQueryString("cid", genericController.encodeText(AdminContent.Id))
                    If editRecord.id <> 0 Then Call cpCore.doc.addRefreshQueryString("id", genericController.encodeText(editRecord.id))
                    If TitleExtension <> "" Then Call cpCore.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension))
                    If RecordTop <> 0 Then Call cpCore.doc.addRefreshQueryString("rt", genericController.encodeText(RecordTop))
                    If RecordsPerPage <> RecordsPerPageDefault Then Call cpCore.doc.addRefreshQueryString("rs", genericController.encodeText(RecordsPerPage))
                    If AdminForm <> 0 Then Call cpCore.doc.addRefreshQueryString(RequestNameAdminForm, genericController.encodeText(AdminForm))
                    If MenuDepth <> 0 Then Call cpCore.doc.addRefreshQueryString(RequestNameAdminDepth, genericController.encodeText(MenuDepth))
                    '
                    ' normalize guid
                    '
                    If AddonGuid <> "" Then
                        If (Len(AddonGuid) = 38) And (Left(AddonGuid, 1) = "{") And (Right(AddonGuid, 1) = "}") Then
                            '
                            ' Good to go
                            '
                        ElseIf (Len(AddonGuid) = 36) Then
                            '
                            ' might be valid with the brackets, add them
                            '
                            AddonGuid = "{" & AddonGuid & "}"
                        ElseIf (Len(AddonGuid) = 32) Then
                            '
                            ' might be valid with the brackets and the dashes, add them
                            '
                            AddonGuid = "{" & Mid(AddonGuid, 1, 8) & "-" & Mid(AddonGuid, 9, 4) & "-" & Mid(AddonGuid, 13, 4) & "-" & Mid(AddonGuid, 17, 4) & "-" & Mid(AddonGuid, 21) & "}"
                        Else
                            '
                            ' not valid
                            '
                            AddonGuid = ""
                        End If
                    End If
                    '
                    '-------------------------------------------------------------------------------
                    ' Create the content
                    '-------------------------------------------------------------------------------
                    '
                    ContentCell = ""
                    If ContentArgFromCaller <> "" Then
                        '
                        ' Use content passed in as an argument
                        '
                        ContentCell = ContentArgFromCaller
                    ElseIf (HelpAddonID <> 0) Then
                        '
                        ' display Addon Help
                        '
                        Call cpCore.doc.addRefreshQueryString("helpaddonid", HelpAddonID.ToString)
                        ContentCell = GetAddonHelp(HelpAddonID, "")
                    ElseIf (HelpCollectionID <> 0) Then
                        '
                        ' display Collection Help
                        '
                        Call cpCore.doc.addRefreshQueryString("helpcollectionid", HelpCollectionID.ToString)
                        ContentCell = GetCollectionHelp(HelpCollectionID, "")
                    ElseIf (AdminForm <> 0) Then
                        '
                        ' No content so far, try the forms
                        '
                        Select Case Int(AdminForm)
                            Case AdminFormBuilderCollection
                                ContentCell = GetForm_BuildCollection()
                            Case AdminFormSecurityControl
                                AddonGuid = AddonGuidPreferences
                            '    ContentCell = GetForm_SecurityControl()
                            Case AdminFormMetaKeywordTool
                                ContentCell = GetForm_MetaKeywordTool()
                            Case AdminFormMobileBrowserControl, AdminFormPageControl, AdminFormEmailControl
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, AddonGuidPreferences), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "Preferences"})
                                'ContentCell = cpCore.addon.execute_legacy4(AddonGuidPreferences, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                            Case AdminFormClearCache
                                ContentCell = GetForm_ClearCache()
                            'Case AdminFormEDGControl
                            '    ContentCell = GetForm_StaticPublishControl()
                            Case AdminFormSpiderControl
                                ContentCell = cpCore.addon.execute(addonModel.createByName(cpCore, "Content Spider Control"), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "Content Spider Control"})
                                'ContentCell = cpCore.addon.execute_legacy4("Content Spider Control", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                            Case AdminFormResourceLibrary
                                ContentCell = cpCore.html.main_GetResourceLibrary2("", False, "", "", True)
                            Case AdminFormQuickStats
                                ContentCell = (GetForm_QuickStats())
                            Case AdminFormIndex
                                ContentCell = (GetForm_Index(AdminContent, editRecord, (LCase(AdminContent.ContentTableName) = "ccemail")))
                            Case AdminFormEdit
                                ContentCell = GetForm_Edit(AdminContent, editRecord)
                            Case AdminFormClose
                                Stream.Add("<Script Language=""JavaScript"" type=""text/javascript""> window.close(); </Script>")
                            Case AdminFormPublishing
                                ContentCell = (GetForm_Publish())
                            Case AdminFormContentChildTool
                                ContentCell = (GetContentChildTool())
                            Case AdminformPageContentMap
                                ContentCell = (GetForm_PageContentMap())
                            Case AdminformHousekeepingControl
                                ContentCell = (GetForm_HouseKeepingControl())
                            Case AdminFormTools, 100 To 199
                                Dim Tools As New coreToolsClass(cpCore)
                                ContentCell = Tools.GetForm()
                            Case AdminFormStyleEditor
                                ContentCell = (admin_GetForm_StyleEditor())
                            Case AdminFormDownloads
                                ContentCell = (GetForm_Downloads())
                            Case AdminformRSSControl
                                ContentCell = cpCore.webServer.redirect("?cid=" & Models.Complex.cdefModel.getContentId(cpCore, "RSS Feeds"), "RSS Control page is not longer supported. RSS Feeds are controlled from the RSS feed records.")
                            Case AdminFormImportWizard
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, addonGuidImportWizard), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "Import Wizard"})
                                'ContentCell = cpCore.addon.execute_legacy4(addonGuidImportWizard, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                            Case AdminFormCustomReports
                                ContentCell = GetForm_CustomReports()
                            Case AdminFormFormWizard
                                ContentCell = cpCore.addon.execute(addonModel.create(cpCore, addonGuidFormWizard), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, .errorCaption = "Form Wizard"})
                                'ContentCell = cpCore.addon.execute_legacy4(addonGuidFormWizard, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin)
                            Case AdminFormLegacyAddonManager
                                ContentCell = addonController.GetAddonManager(cpCore)
                            Case AdminFormEditorConfig
                                ContentCell = GetForm_EditConfig()
                            Case Else
                                ContentCell = "<p>The form requested is not supported</p>"
                        End Select
                    ElseIf ((addonId <> 0) Or (AddonGuid <> "") Or (AddonName <> "")) Then
                        '
                        ' execute an addon
                        '
                        If (AddonGuid = addonGuidAddonManager) Or (LCase(AddonName) = "add-on manager") Or (LCase(AddonName) = "addon manager") Then
                            '
                            ' Special case, call the routine that provides a backup
                            '
                            Call cpCore.doc.addRefreshQueryString("addonguid", addonGuidAddonManager)
                            ContentCell = addonController.GetAddonManager(cpCore)
                        Else
                            Dim addon As addonModel = Nothing
                            Dim executeContextErrorCaption As String = "unknown"
                            If addonId <> 0 Then
                                executeContextErrorCaption = "id:" & addonId
                                Call cpCore.doc.addRefreshQueryString("addonid", CStr(addonId))
                                addon = addonModel.create(cpCore, addonId)
                            ElseIf AddonGuid <> "" Then
                                executeContextErrorCaption = "guid:" & AddonGuid
                                Call cpCore.doc.addRefreshQueryString("addonguid", AddonGuid)
                                addon = addonModel.create(cpCore, AddonGuid)
                            ElseIf AddonName <> "" Then
                                executeContextErrorCaption = AddonName
                                Call cpCore.doc.addRefreshQueryString("addonname", AddonName)
                                addon = addonModel.createByName(cpCore, AddonName)
                            End If
                            If (addon IsNot Nothing) Then
                                addonId = addon.id
                                AddonName = addon.name
                                AddonHelpCopy = addon.Help
                                Call cpCore.doc.addRefreshQueryString(RequestNameRunAddon, addonId.ToString)
                            End If
                            InstanceOptionString = cpCore.userProperty.getText("Addon [" & AddonName & "] Options", "")
                            DefaultWrapperID = -1
                            ContentCell = cpCore.addon.execute(addon, New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {
                                .addonType = Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin,
                                .instanceGuid = adminSiteInstanceId,
                                .instanceArguments = genericController.convertAddonArgumentstoDocPropertiesList(cpCore, InstanceOptionString),
                                .wrapperID = DefaultWrapperID,
                                .errorCaption = executeContextErrorCaption
                            })
                            If String.IsNullOrEmpty(ContentCell) Then
                                '
                                ' empty returned, display desktop
                                ContentCell = GetForm_Root()
                            End If

                        End If
                    Else
                        '
                        ' nothing so far, display desktop
                        '
                        ContentCell = GetForm_Root()
                    End If
                    '
                    ' include fancybox if it was needed
                    '
                    If includeFancyBox Then
                        Call cpCore.addon.executeDependency(addonModel.create(cpCore, addonGuidjQueryFancyBox), New BaseClasses.CPUtilsBaseClass.addonExecuteContext() With {.addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin})
                        'Call cpCore.addon.execute_legacy4(addonGuidjQueryFancyBox)
                        Call cpCore.html.addScriptCode_head("jQuery(document).ready(function() {" & fancyBoxHeadJS & "});", "")
                    End If
                    '
                    ' Pickup user errors
                    '
                    If (cpCore.doc.debug_iUserError <> "") Then
                        ContentCell = "<div class=""ccAdminMsg"">" & errorController.error_GetUserError(cpCore) & "</div>" & ContentCell
                    End If
                    ''
                    '' If blank, must be an addon with a setting form that returned blank, do the dashboard again
                    ''
                    'If ContentCell = "" Then
                    '    '
                    '    ' must use the root as a default - bc forms and add-ons may return blank, meaning return to root
                    '    ' throw errors only if there is a user error
                    '    '
                    '    ContentCell = GetForm_Root()
                    '    'ContentCell = "<div class=""ccAdminMsg"">The form you requested did not return a valid response.</div>"
                    'End If
                    '
                    Call Stream.Add(cr & GetForm_Top())
                    Call Stream.Add(genericController.htmlIndent(ContentCell))
                    Call Stream.Add(cr & AdminFormBottom)
                    'Call Stream.Add(cr & "<script language=""javascript1.2"" type=""text/javascript"">" & JavaScriptString)
                    'Call Stream.Add(cr & "ButtonObjectCount = " & ButtonObjectCount & ";")
                    'Call Stream.Add(cr & "</script>")
                    JavaScriptString &= cr & "ButtonObjectCount = " & ButtonObjectCount & ";"
                    cpCore.html.addScriptCode_body(JavaScriptString, "Admin Site")
                End If
                result = errorController.getDocExceptionHtmlList(cpCore) & Stream.Text
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
            Return result
        End Function
        '
        '========================================================================
        '   read the input request
        '       If RequestBlocked get adminContent.id, AdminAction, FormID
        '       and AdminForm are the only variables accessible before reading
        '       the upl collection
        '========================================================================
        '
        Private Sub GetForm_LoadControl(ByRef adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_LoadControl")
            '
            Dim editorpreferences As String
            Dim Pos As Integer
            Dim SQL As String
            Dim Key As String
            Dim Parts() As String
            Dim Ptr As Integer
            Dim Cnt As Integer
            Dim fieldEditorFieldId As Integer
            Dim fieldEditorAddonId As Integer
            Dim dt As DataTable
            Dim editorOk As Boolean
            Dim CS As Integer
            Dim QSSplit() As String
            Dim QSPointer As Integer
            Dim NVSplit() As String
            Dim NameValue As String
            Dim WCount As Integer
            Dim FindTemp As String
            Dim FieldCount As Integer
            Dim StringTemp As String
            Dim WhereClauseContent As String
            Dim WherePairTemp As String
            Dim Position As Integer
            Dim Position2 As Integer
            Dim MethodName As String
            Dim InputText As String
            Dim Id As Integer
            'dim buildversion As String
            Dim dtTest As DataTable
            '
            MethodName = "Admin.Method()"
            '
            ' Tab Control
            '
            allowAdminTabs = genericController.EncodeBoolean(cpCore.userProperty.getText("AllowAdminTabs", "1"))
            If cpCore.docProperties.getText("tabs") <> "" Then
                If cpCore.docProperties.getBoolean("tabs") <> allowAdminTabs Then
                    allowAdminTabs = Not allowAdminTabs
                    Call cpCore.userProperty.setProperty("AllowAdminTabs", allowAdminTabs.ToString)
                End If
            End If
            '
            ' AdminContent init
            '
            requestedContentId = cpCore.docProperties.getInteger("cid")
            If requestedContentId <> 0 Then
                adminContent = Models.Complex.cdefModel.getCdef(cpCore, requestedContentId)
                If adminContent Is Nothing Then
                    adminContent = New Models.Complex.cdefModel
                    adminContent.Id = 0
                    errorController.error_AddUserError(cpCore, "There is no content with the requested id [" & requestedContentId & "]")
                    requestedContentId = 0
                End If
            End If
            If adminContent Is Nothing Then
                adminContent = New Models.Complex.cdefModel
            End If
            '
            ' determine user rights to this content
            '
            UserAllowContentEdit = True
            UserAllowContentAdd = True
            UserAllowContentDelete = True
            If Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
                If (adminContent.Id > 0) Then
                    UserAllowContentEdit = userHasContentAccess(adminContent.Id)
                End If
            End If
            '
            ' editRecord init
            '
            requestedRecordId = cpCore.docProperties.getInteger("id")
            If (UserAllowContentEdit) And (requestedRecordId <> 0) And (adminContent.Id > 0) Then
                '
                ' set AdminContent to the content definition of the requested record
                '
                CS = cpCore.db.csOpenRecord(adminContent.Name, requestedRecordId, , , "ContentControlID")
                If cpCore.db.csOk(CS) Then
                    editRecord.id = requestedRecordId
                    adminContent.Id = cpCore.db.csGetInteger(CS, "ContentControlID")
                    If adminContent.Id <= 0 Then
                        adminContent.Id = requestedContentId
                    ElseIf adminContent.Id <> requestedContentId Then
                        adminContent = Models.Complex.cdefModel.getCdef(cpCore, adminContent.Id)
                    End If
                End If
                Call cpCore.db.csClose(CS)
            End If
            '
            ' Other page control fields
            '
            TitleExtension = cpCore.docProperties.getText(RequestNameTitleExtension)
            RecordTop = cpCore.docProperties.getInteger("RT")
            RecordsPerPage = cpCore.docProperties.getInteger("RS")
            If RecordsPerPage = 0 Then
                RecordsPerPage = RecordsPerPageDefault
            End If
            '
            ' Read WherePairCount
            '
            WherePairCount = 99
            For WCount = 0 To 99
                WherePair(0, WCount) = genericController.encodeText(cpCore.docProperties.getText("WL" & WCount))
                If WherePair(0, WCount) = "" Then
                    WherePairCount = WCount
                    Exit For
                Else
                    WherePair(1, WCount) = genericController.encodeText(cpCore.docProperties.getText("WR" & WCount))
                    Call cpCore.doc.addRefreshQueryString("wl" & WCount, genericController.EncodeRequestVariable(WherePair(0, WCount)))
                    Call cpCore.doc.addRefreshQueryString("wr" & WCount, genericController.EncodeRequestVariable(WherePair(1, WCount)))
                End If
            Next
            '
            ' Read WhereClauseContent to WherePairCount
            '
            WhereClauseContent = genericController.encodeText(cpCore.docProperties.getText("wc"))
            If (WhereClauseContent <> "") Then
                '
                ' ***** really needs a server.URLDecode() function
                '
                Call cpCore.doc.addRefreshQueryString("wc", WhereClauseContent)
                'WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%3D", "=")
                'WhereClauseContent = genericController.vbReplace(WhereClauseContent, "%26", "&")
                If WhereClauseContent <> "" Then
                    QSSplit = Split(WhereClauseContent, ",")
                    For QSPointer = 0 To UBound(QSSplit)
                        NameValue = QSSplit(QSPointer)
                        If NameValue <> "" Then
                            If (Left(NameValue, 1) = "(") And (Right(NameValue, 1) = ")") And (Len(NameValue) > 2) Then
                                NameValue = Mid(NameValue, 2, Len(NameValue) - 2)
                            End If
                            NVSplit = Split(NameValue, "=")
                            WherePair(0, WherePairCount) = NVSplit(0)
                            If UBound(NVSplit) > 0 Then
                                WherePair(1, WherePairCount) = NVSplit(1)
                            End If
                            WherePairCount = WherePairCount + 1
                        End If
                    Next
                End If
            End If
            '
            ' --- If AdminMenuMode is not given locally, use the Members Preferences
            '
            Dim MenuModeVariant As Object
            '
            AdminMenuModeID = cpCore.docProperties.getInteger("mm")
            If AdminMenuModeID = 0 Then
                AdminMenuModeID = cpCore.doc.authContext.user.AdminMenuModeID
            End If
            If AdminMenuModeID = 0 Then
                AdminMenuModeID = AdminMenuModeLeft
            End If
            If cpCore.doc.authContext.user.AdminMenuModeID <> AdminMenuModeID Then
                cpCore.doc.authContext.user.AdminMenuModeID = AdminMenuModeID
                Call cpCore.doc.authContext.user.save(cpCore)
            End If
            '    '
            '    ' ----- FieldName
            '    '
            '    InputFieldName = cpCore.main_GetStreamText2(RequestNameFieldName)
            '
            ' ----- Other
            '
            AdminAction = cpCore.docProperties.getInteger(RequestNameAdminAction)
            AdminSourceForm = cpCore.docProperties.getInteger(RequestNameAdminSourceForm)
            AdminForm = cpCore.docProperties.getInteger(RequestNameAdminForm)
            AdminButton = cpCore.docProperties.getText(RequestNameButton)
            '
            ' ----- Convert misc Deletes to just delete for later processing
            '
            If (AdminButton = ButtonDeleteEmail) Or (AdminButton = ButtonDeletePage) Or (AdminButton = ButtonDeletePerson) Or (AdminButton = ButtonDeleteRecord) Then
                AdminButton = ButtonDelete
            End If
            If (AdminForm = AdminFormEdit) Then
                MenuDepth = 0
            Else
                MenuDepth = cpCore.docProperties.getInteger(RequestNameAdminDepth)
            End If
            '
            ' ----- convert fieldEditorPreference change to a refresh action
            '
            If adminContent.Id <> 0 Then
                fieldEditorPreference = cpCore.docProperties.getText("fieldEditorPreference")
                If fieldEditorPreference <> "" Then
                    '
                    ' Editor Preference change attempt. Set new preference and set this as a refresh
                    '
                    AdminButton = ""
                    AdminAction = AdminActionEditRefresh
                    AdminForm = AdminFormEdit
                    Pos = genericController.vbInstr(1, fieldEditorPreference, ":")
                    If Pos > 0 Then
                        fieldEditorFieldId = genericController.EncodeInteger(Mid(fieldEditorPreference, 1, Pos - 1))
                        fieldEditorAddonId = genericController.EncodeInteger(Mid(fieldEditorPreference, Pos + 1))
                        If (fieldEditorFieldId <> 0) Then
                            editorOk = True
                            SQL = "select id from ccfields where (active<>0) and id=" & fieldEditorFieldId
                            dtTest = cpCore.db.executeQuery(SQL)
                            If dtTest.Rows.Count = 0 Then
                                editorOk = False
                            End If
                            'RS = cpCore.app.executeSql(SQL)
                            'If (not isdatatableok(rs)) Then
                            '    editorOk = False
                            'ElseIf rs.rows.count=0 Then
                            '    editorOk = False
                            'End If
                            'If (isDataTableOk(rs)) Then
                            '    If false Then
                            '        'RS.Close()
                            '    End If
                            '    'RS = Nothing
                            'End If
                            If editorOk And (fieldEditorAddonId <> 0) Then
                                SQL = "select id from ccaggregatefunctions where (active<>0) and id=" & fieldEditorAddonId
                                dtTest = cpCore.db.executeQuery(SQL)
                                If dtTest.Rows.Count = 0 Then
                                    editorOk = False
                                End If
                                'RS = cpCore.app.executeSql(SQL)
                                'If (not isdatatableok(rs)) Then
                                '    editorOk = False
                                'ElseIf rs.rows.count=0 Then
                                '    editorOk = False
                                'End If
                                'If (isDataTableOk(rs)) Then
                                '    If false Then
                                '        'RS.Close()
                                '    End If
                                '    'RS = Nothing
                                'End If
                            End If
                            If editorOk Then
                                Key = "editorPreferencesForContent:" & adminContent.Id
                                editorpreferences = cpCore.userProperty.getText(Key, "")
                                If editorpreferences <> "" Then
                                    '
                                    ' remove current preferences for this field
                                    '
                                    Parts = Split("," & editorpreferences, "," & CStr(fieldEditorFieldId) & ":")
                                    Cnt = UBound(Parts) + 1
                                    If Cnt > 0 Then
                                        For Ptr = 1 To Cnt - 1
                                            Pos = genericController.vbInstr(1, Parts(Ptr), ",")
                                            If Pos = 0 Then
                                                Parts(Ptr) = ""
                                            ElseIf Pos > 0 Then
                                                Parts(Ptr) = Mid(Parts(Ptr), Pos + 1)
                                            End If
                                        Next
                                    End If
                                    editorpreferences = Join(Parts, "")
                                End If
                                editorpreferences = editorpreferences & "," & fieldEditorFieldId & ":" & fieldEditorAddonId
                                Call cpCore.userProperty.setProperty(Key, editorpreferences)
                            End If
                        End If
                    End If
                End If
            End If
            '
            ' --- Spell Check
            '
            ' BuildVersion = cpCore.app.GetSiteProperty("BuildVersion")

            If True Then
                SpellCheckSupported = False
                SpellCheckRequest = False
                SpellCheckResponse = False
                SpellCheckDictionaryFilename = ""
                SpellCheckIgnoreList = ""
            Else
            End If
            '
            '''Dim th as integer
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError2("GetForm_LoadControl")
            Resume Next
        End Sub
        '
    End Class
End Namespace
