
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' </summary>
    Public Class authContextUserModel
        '
        Private cpCore As coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' id for the authcontext.user. When this property is set, all public authcontext.user. properteries are updated for this selected id
        ''' </summary>
        ''' <returns></returns>
        Public Property id As Integer
            Get
                Return _id
            End Get
            Set(value As Integer)
                If (_id <> value) Then
                    _id = initializeUser(value)
                End If
            End Set
        End Property
        Private _id As Integer = 0
        '
        ' simple shared properties, derived from the userId when .id set (through initilizeUser method)
        '
        Friend name As String = ""                 '
        Friend isAdmin As Boolean = False              '
        Friend isDeveloper As Boolean = False          '
        Friend organizationId As Integer = 0         ' The members Organization
        Friend languageId As Integer = 0             '
        Friend language As String = ""             '
        Friend isNew As Boolean = False                ' stored in visit record - Is this the first visit for this member
        Friend email As String = ""               '
        '
        Friend allowBulkEmail As Boolean = False      ' Allow bulk mail
        Friend allowToolsPanel As Boolean = False    '
        Friend autoLogin As Boolean = False         ' if true, and setup AllowAutoLogin then use cookie to login
        Friend adminMenuModeID As Integer = 0     '
        '
        Friend userAdded As Boolean = False              ' depricated - true only during the page that the join was completed - use for redirections and GroupAdds
        Friend username As String = ""
        Friend password As String = ""
        Friend contentControlID As Integer = 0
        '
        Friend styleFilename As String = ""          ' if not empty, add to head
        Friend excludeFromAnalytics As Boolean = False   ' if true, future visits will be marked exclude from analytics
        '
        Public main_IsEditingContentList As String = ""
        Public main_IsNotEditingContentList As String = ""
        '
        '-----------------------------------------------------------------------
        ' ----- Member Private
        '-----------------------------------------------------------------------
        '
        Public active As Boolean = False           '
        Public visits As Integer = 0                '
        Public lastVisit As Date = Date.MinValue             ' The last visit by the Member (the beginning of this visit
        '
        Public company As String = ""
        Public user_Title As String = ""
        Public main_MemberAddress As String = ""
        Public main_MemberCity As String = ""
        Public main_MemberState As String = ""
        Public main_MemberZip As String = ""
        Public main_MemberCountry As String = ""
        '
        Public main_MemberPhone As String = ""
        Public main_MemberFax As String = ""
        '
        '-----------------------------------------------------------------------
        ' ----- Member Commerce properties
        '-----------------------------------------------------------------------
        '
        Public billEmail As String = ""          ' Billing Address for purchases
        Public billPhone As String = ""          '
        Public billFax As String = ""            '
        Public billCompany As String = ""        '
        Public billAddress As String = ""        '
        Public billCity As String = ""           '
        Public billState As String = ""         '
        Public billZip As String = ""            '
        Public billCountry As String = ""       '
        '
        Public shipName As String = ""          ' Mailing Address
        Public shipCompany As String = ""           '
        Public shipAddress As String = ""          '
        Public shipCity As String = ""          '
        Public shipState As String = ""        '
        Public shipZip As String = ""           '
        Public shipCountry As String = ""         '
        Public shipPhone As String = ""        '
        '
        '----------------------------------------------------------------------------------------------------
        '
        '
        Public Const main_maxVisitLoginAttempts As Integer = 20
        Public main_loginFormDefaultProcessed As Boolean = False       ' prevent main_ProcessLoginFormDefault from running twice (multiple user messages, popups, etc.)
        '
        '------------------------------------------------------------------------
        ' ----- local cache to speed up authcontext.user.main_IsContentManager
        '------------------------------------------------------------------------
        '
        Private contentAccessRights_NotList As String = ""                  ' If ContentId in this list, they are not a content manager
        Private contentAccessRights_List As String = ""                     ' If ContentId in this list, they are a content manager
        Private contentAccessRights_AllowAddList As String = ""             ' If in _List, test this for allowAdd
        Private contentAccessRights_AllowDeleteList As String = ""          ' If in _List, test this for allowDelete
        '
        '========================================================================
        ''' <summary>
        ''' is Guest
        ''' </summary>
        ''' <returns></returns>
        Public Function isGuest() As Boolean
            Return Not isAuthenticatedMember()
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' Is Recognized (not new and not authenticted)
        ''' </summary>
        ''' <returns></returns>
        Public Function isRecognized() As Boolean
            Return Not isNew
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' authenticated
        ''' </summary>
        ''' <returns></returns>
        Public Function isAuthenticated() As Boolean
            Return cpCore.authContext.visit.VisitAuthenticated
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if editing any content
        ''' </summary>
        ''' <returns></returns>
        Public Function isEditingAnything() As Boolean
            Return isEditing("")
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' True if editing a specific content
        ''' </summary>
        ''' <param name="ContentNameOrId"></param>
        ''' <returns></returns>
        Public Function isEditing(ByVal ContentNameOrId As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If True Then
                    Dim localContentNameOrId As String
                    Dim cacheTestName As String
                    '
                    If Not cpCore.authContext.visit_initialized Then
                        Call cpCore.debug_testPoint("...visit not initialized")
                    Else
                        '
                        ' always false until visit loaded
                        '
                        localContentNameOrId = genericController.encodeText(ContentNameOrId)
                        cacheTestName = localContentNameOrId
                        If cacheTestName = "" Then
                            cacheTestName = "iseditingall"
                        End If
                        cacheTestName = genericController.vbLCase(cacheTestName)
                        If genericController.IsInDelimitedString(main_IsEditingContentList, cacheTestName, ",") Then
                            Call cpCore.debug_testPoint("...is in main_IsEditingContentList")
                            returnResult = True
                        ElseIf genericController.IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",") Then
                            Call cpCore.debug_testPoint("...is in main_IsNotEditingContentList")
                        Else
                            If isAuthenticated() Then
                                If Not cpCore.htmlDoc.pageManager_printVersion Then
                                    If (cpCore.visitProperty.getBoolean("AllowEditing") Or cpCore.visitProperty.getBoolean("AllowAdvancedEditor")) Then
                                        If localContentNameOrId <> "" Then
                                            If genericController.vbIsNumeric(localContentNameOrId) Then
                                                localContentNameOrId = cpCore.metaData.getContentNameByID(EncodeInteger(localContentNameOrId))
                                            End If
                                        End If
                                        returnResult = isAuthenticatedContentManager(localContentNameOrId)
                                    End If
                                End If
                            End If
                            If returnResult Then
                                main_IsEditingContentList = main_IsEditingContentList & "," & cacheTestName
                            Else
                                main_IsNotEditingContentList = main_IsNotEditingContentList & "," & cacheTestName
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' true if editing with the quick editor
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function isQuickEditing(ByVal ContentName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If (Not cpCore.htmlDoc.pageManager_printVersion) Then
                    If isAuthenticatedContentManager(genericController.encodeText(ContentName)) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowQuickEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' main_IsAdvancedEditing( ContentName )
        ''' <summary>
        ''' true if advanded editing
        ''' </summary>
        ''' <param name="ContentName"></param>
        ''' <returns></returns>
        Public Function isAdvancedEditing(ByVal ContentName As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If (Not cpCore.htmlDoc.pageManager_printVersion) Then
                    If isAuthenticatedContentManager(genericController.encodeText(ContentName)) Then
                        returnResult = cpCore.visitProperty.getBoolean("AllowAdvancedEditor")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        '   main_IsAdmin
        '   true if:
        '       Is Authenticated
        '       Is Member
        '       Member has admin or developer status
        '========================================================================
        '
        Public Function isAuthenticatedAdmin() As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not isAuthenticatedAdmin_cache_isLoaded) And cpCore.authContext.visit_initialized Then
                    isAuthenticatedAdmin_cache = isAuthenticated() And (isAdmin Or isDeveloper)
                    isAuthenticatedAdmin_cache_isLoaded = True
                End If
                returnIs = isAuthenticatedAdmin_cache
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        Private isAuthenticatedAdmin_cache As Boolean = False               ' true if member is administrator
        Private isAuthenticatedAdmin_cache_isLoaded As Boolean = False              ' true if main_IsAdminCache is initialized
        '
        '========================================================================
        '   main_IsDeveloper
        '========================================================================
        '
        Public Function isAuthenticatedDeveloper() As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not isAuthenticatedDeveloper_cache_isLoaded) And cpCore.authContext.visit_initialized Then
                    isAuthenticatedDeveloper_cache = (isAuthenticated() And isDeveloper)
                    isAuthenticatedDeveloper_cache_isLoaded = True
                End If
                returnIs = isAuthenticatedDeveloper_cache
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        '
        Private isAuthenticatedDeveloper_cache As Boolean = False
        Private isAuthenticatedDeveloper_cache_isLoaded As Boolean = False
        '
        '=============================================================================
        '   main_SaveMember()
        '       Saves the member properties that are loaded during main_OpenMember
        '=============================================================================
        '
        Public Sub saveMember()
            Try
                Dim SQL As String
                '
                If cpCore.authContext.visit_initialized Then
                    If (id > 0) Then
                        SQL = "UPDATE ccMembers SET " _
                        & " Name=" & cpCore.db.encodeSQLText(name) _
                        & ",username=" & cpCore.db.encodeSQLText(username) _
                        & ",email=" & cpCore.db.encodeSQLText(email) _
                        & ",password=" & cpCore.db.encodeSQLText(password) _
                        & ",OrganizationID=" & cpCore.db.encodeSQLNumber(organizationId) _
                        & ",LanguageID=" & cpCore.db.encodeSQLNumber(languageId) _
                        & ",Active=" & cpCore.db.encodeSQLBoolean(active) _
                        & ",Company=" & cpCore.db.encodeSQLText(company) _
                        & ",Visits=" & cpCore.db.encodeSQLNumber(visits) _
                        & ",LastVisit=" & cpCore.db.encodeSQLDate(lastVisit) _
                        & ",AllowBulkEmail=" & cpCore.db.encodeSQLBoolean(allowBulkEmail) _
                        & ",AdminMenuModeID=" & cpCore.db.encodeSQLNumber(adminMenuModeID) _
                        & ",AutoLogin=" & cpCore.db.encodeSQLBoolean(autoLogin)
                        ' 6/18/2009 - removed notes from base
                        '           & ",SendNotes=" & encodeSQLBoolean(MemberSendNotes)
                        SQL &= "" _
                        & ",BillEmail=" & cpCore.db.encodeSQLText(billEmail) _
                        & ",BillPhone=" & cpCore.db.encodeSQLText(billPhone) _
                        & ",BillFax=" & cpCore.db.encodeSQLText(billFax) _
                        & ",BillCompany=" & cpCore.db.encodeSQLText(billCompany) _
                        & ",BillAddress=" & cpCore.db.encodeSQLText(billAddress) _
                        & ",BillCity=" & cpCore.db.encodeSQLText(billCity) _
                        & ",BillState=" & cpCore.db.encodeSQLText(billState) _
                        & ",BillZip=" & cpCore.db.encodeSQLText(billZip) _
                        & ",BillCountry=" & cpCore.db.encodeSQLText(billCountry)
                        SQL &= "" _
                        & ",ShipName=" & cpCore.db.encodeSQLText(shipName) _
                        & ",ShipCompany=" & cpCore.db.encodeSQLText(shipCompany) _
                        & ",ShipAddress=" & cpCore.db.encodeSQLText(shipAddress) _
                        & ",ShipCity=" & cpCore.db.encodeSQLText(shipCity) _
                        & ",ShipState=" & cpCore.db.encodeSQLText(shipState) _
                        & ",ShipZip=" & cpCore.db.encodeSQLText(shipZip) _
                        & ",ShipCountry=" & cpCore.db.encodeSQLText(shipCountry) _
                        & ",ShipPhone=" & cpCore.db.encodeSQLText(shipPhone)
                        If True Then
                            SQL &= ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(excludeFromAnalytics)
                        End If
                        SQL &= " WHERE ID=" & id & ";"
                        Call cpCore.db.executeSql(SQL)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '   admins are always returned true
        '===============================================================================================================================
        '
        Public Function isMemberOfGroupIdList(ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String) As Boolean
            Return isMemberOfGroupIdList(MemberID, isAuthenticated, GroupIDList, True)
        End Function
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '===============================================================================================================================
        '
        Public Function isMemberOfGroupIdList(ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String, ByVal adminReturnsTrue As Boolean) As Boolean
            Dim returnREsult As Boolean = False
            Try
                '
                Dim CS As Integer
                Dim SQL As String
                Dim Criteria As String
                Dim WorkingIDList As String
                '
                returnREsult = False
                If isAuthenticated Then
                    WorkingIDList = GroupIDList
                    WorkingIDList = genericController.vbReplace(WorkingIDList, " ", "")
                    Do While genericController.vbInstr(1, WorkingIDList, ",,") <> 0
                        WorkingIDList = genericController.vbReplace(WorkingIDList, ",,", ",")
                    Loop
                    If (WorkingIDList <> "") Then
                        If vbMid(WorkingIDList, 1) = "," Then
                            If vbLen(WorkingIDList) <= 1 Then
                                WorkingIDList = ""
                            Else
                                WorkingIDList = vbMid(WorkingIDList, 2)
                            End If
                        End If
                    End If
                    If (WorkingIDList <> "") Then
                        If vbRight(WorkingIDList, 1) = "," Then
                            If vbLen(WorkingIDList) <= 1 Then
                                WorkingIDList = ""
                            Else
                                WorkingIDList = genericController.vbMid(WorkingIDList, 1, vbLen(WorkingIDList) - 1)
                            End If
                        End If
                    End If
                    If (WorkingIDList = "") Then
                        If adminReturnsTrue Then
                            '
                            ' check if memberid is admin
                            '
                            SQL = "select top 1 m.id" _
                            & " from ccmembers m" _
                            & " where" _
                            & " (m.id=" & MemberID & ")" _
                            & " and(m.active<>0)" _
                            & " and(" _
                                & " (m.admin<>0)" _
                                & " or(m.developer<>0)" _
                            & " )" _
                            & " "
                            CS = cpCore.db.cs_openCsSql_rev("default", SQL)
                            returnREsult = cpCore.db.cs_ok(CS)
                            Call cpCore.db.cs_Close(CS)
                        End If
                    Else
                        '
                        ' check if they are admin or in the group list
                        '
                        If genericController.vbInstr(1, WorkingIDList, ",") <> 0 Then
                            Criteria = "r.GroupID in (" & WorkingIDList & ")"
                        Else
                            Criteria = "r.GroupID=" & WorkingIDList
                        End If
                        Criteria = "" _
                        & "(" & Criteria & ")" _
                        & " and(r.id is not null)" _
                        & " and((r.DateExpires is null)or(r.DateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))" _
                        & " "
                        If adminReturnsTrue Then
                            Criteria = "(" & Criteria & ")or(m.admin<>0)or(m.developer<>0)"
                        End If
                        Criteria = "" _
                        & "(" & Criteria & ")" _
                        & " and(m.active<>0)" _
                        & " and(m.id=" & MemberID & ")" _
                        '
                        SQL = "select top 1 m.id" _
                        & " from ccmembers m" _
                        & " left join ccMemberRules r on r.Memberid=m.id" _
                        & " where" & Criteria
                        CS = cpCore.db.cs_openCsSql_rev("default", SQL)
                        returnREsult = cpCore.db.cs_ok(CS)
                        Call cpCore.db.cs_Close(CS)
                    End If
                End If

            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' Member Open
        '   Attempts to open the Member record based on the iRecordID
        '   If successful, MemberID is set to the iRecordID
        '========================================================================
        '
        Public Function initializeUser(recordId As Integer) As Integer
            Dim returnRecordId As Integer = 0
            Try
                Dim CS As Integer
                '
                If recordId <> 0 Then
                    '
                    ' attempt to read in Member record if logged on
                    ' dont just do main_CheckMember() -- in case a pretty login is needed
                    '
                    CS = cpCore.csOpenRecord("People", recordId)
                    If cpCore.db.cs_ok(CS) Then
                        name = cpCore.db.cs_getText(CS, "Name")
                        isDeveloper = cpCore.db.cs_getBoolean(CS, "Developer")
                        isAdmin = cpCore.db.cs_getBoolean(CS, "Admin")
                        contentControlID = cpCore.db.cs_getInteger(CS, "ContentControlID")
                        organizationId = cpCore.db.cs_getInteger(CS, "OrganizationID")
                        languageId = cpCore.db.cs_getInteger(CS, "LanguageID")
                        language = cpCore.main_cs_getEncodedField(CS, "LanguageID")
                        '
                        shipName = cpCore.db.cs_getText(CS, "ShipName")
                        shipCompany = cpCore.db.cs_getText(CS, "ShipCompany")
                        shipAddress = cpCore.db.cs_getText(CS, "ShipAddress")
                        shipCity = cpCore.db.cs_getText(CS, "ShipCity")
                        shipState = cpCore.db.cs_getText(CS, "ShipState")
                        shipZip = cpCore.db.cs_getText(CS, "ShipZip")
                        shipCountry = cpCore.db.cs_getText(CS, "ShipCountry")
                        shipPhone = cpCore.db.cs_getText(CS, "ShipPhone")
                        '
                        billCompany = cpCore.db.cs_getText(CS, "BillCompany")
                        billAddress = cpCore.db.cs_getText(CS, "BillAddress")
                        billCity = cpCore.db.cs_getText(CS, "BillCity")
                        billState = cpCore.db.cs_getText(CS, "BillState")
                        billZip = cpCore.db.cs_getText(CS, "BillZip")
                        billCountry = cpCore.db.cs_getText(CS, "BillCountry")
                        billEmail = cpCore.db.cs_getText(CS, "BillEmail")
                        billPhone = cpCore.db.cs_getText(CS, "BillPhone")
                        billFax = cpCore.db.cs_getText(CS, "BillFax")
                        '
                        allowBulkEmail = cpCore.db.cs_getBoolean(CS, "AllowBulkEmail")
                        allowToolsPanel = cpCore.db.cs_getBoolean(CS, "AllowToolsPanel")
                        adminMenuModeID = cpCore.db.cs_getInteger(CS, "AdminMenuModeID")
                        autoLogin = cpCore.db.cs_getBoolean(CS, "AutoLogin")
                        '
                        styleFilename = cpCore.db.cs_getText(CS, "StyleFilename")
                        If styleFilename <> "" Then
                            Call cpCore.htmlDoc.main_AddStylesheetLink(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, styleFilename))
                        End If
                        excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics")
                        returnRecordId = recordId
                    End If
                    Call cpCore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnRecordId
        End Function
        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group named
        '========================================================================
        '
        Public Function IsMemberOfGroup2(ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim iMemberID As Integer
                iMemberID = genericController.EncodeInteger(checkMemberID)
                If iMemberID = 0 Then
                    iMemberID = id
                End If
                returnREsult = isMemberOfGroupList("," & cpCore.group_GetGroupID(genericController.encodeText(GroupName)), iMemberID, True)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Returns true if the visitor is a member, and in the group named
        '========================================================================
        '
        Public Function isMemberOfGroup(ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim iMemberID As Integer
                iMemberID = checkMemberID
                If iMemberID = 0 Then
                    iMemberID = id
                End If
                returnREsult = isMemberOfGroupList("," & cpCore.group_GetGroupID(genericController.encodeText(GroupName)), iMemberID, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function

        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group list
        '========================================================================
        '
        Public Function isMemberOfGroupList(ByVal GroupIDList As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                If checkMemberID = 0 Then
                    checkMemberID = id
                End If
                returnREsult = isMemberOfGroupIdList(checkMemberID, isAuthenticated(), GroupIDList, adminReturnsTrue)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   IsMember
        '   true if the user is authenticated and is a trusted people (member content)
        '========================================================================
        '
        Public Function isAuthenticatedMember() As Boolean
            Dim returnREsult As Boolean = False
            Try
                If (Not property_user_isMember_isLoaded) And (cpCore.authContext.visit_initialized) Then
                    property_user_isMember = isAuthenticated() And cpCore.IsWithinContent(contentControlID, cpCore.main_GetContentID("members"))
                    property_user_isMember_isLoaded = True
                End If
                returnREsult = property_user_isMember
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        Private property_user_isMember As Boolean = False
        Private property_user_isMember_isLoaded As Boolean = False

        Public Sub New()
        End Sub
        '
        '========================================================================
        ' main_IsContentManager2
        '   If ContentName is missing, returns true if this is an authenticated member with
        '       content management over anything
        '   If ContentName is given, it only tests this content
        '========================================================================
        '
        Public Function isAuthenticatedContentManager(Optional ByVal ContentName As String = "") As Boolean
            Dim returnIsContentManager As Boolean = False
            Try
                Dim SQL As String
                Dim CS As Integer
                Dim notImplemented_allowAdd As Boolean
                Dim notImplemented_allowDelete As Boolean
                '
                ' REFACTOR -- add a private dictionary with contentname=>result, plus a authenticationChange flag that makes properties like this invalid
                '
                returnIsContentManager = False
                If String.IsNullOrEmpty(ContentName) Then
                    If isAuthenticated() Then
                        If isAuthenticatedAdmin() Then
                            returnIsContentManager = True
                        Else
                            '
                            ' Is a CM for any content def
                            '
                            If (Not _isAuthenticatedContentManagerAnything_loaded) Or (_isAuthenticatedContentManagerAnything_userId <> id) Then
                                SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(id) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                                        & ")"
                                CS = cpCore.db.cs_openSql(SQL)
                                _isAuthenticatedContentManagerAnything = cpCore.db.cs_ok(CS)
                                cpCore.db.cs_Close(CS)
                                '
                                _isAuthenticatedContentManagerAnything_userId = id
                                _isAuthenticatedContentManagerAnything_loaded = True
                            End If
                            returnIsContentManager = _isAuthenticatedContentManagerAnything
                        End If
                    End If
                Else
                    '
                    ' Specific Content called out
                    '
                    Call cpCore.authContext.user.getContentAccessRights(ContentName, returnIsContentManager, notImplemented_allowAdd, notImplemented_allowDelete)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIsContentManager
        End Function
        Private _isAuthenticatedContentManagerAnything_loaded As Boolean = False
        Private _isAuthenticatedContentManagerAnything_userId As Integer
        Private _isAuthenticatedContentManagerAnything As Boolean
        '
        '========================================================================
        ' Member Login (by username and password)
        '
        '   See main_GetLoginMemberID and main_LoginMemberByID
        '========================================================================
        '
        Public Function authenticate(ByVal loginFieldValue As String, ByVal password As String, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim LocalMemberID As Integer
                '
                returnREsult = False
                LocalMemberID = authenticateGetId(loginFieldValue, password)
                If LocalMemberID <> 0 Then
                    returnREsult = authenticateById(LocalMemberID, AllowAutoLogin)
                    If returnREsult Then
                        Call cpCore.log_LogActivity2("successful password login", id, organizationId)
                        isAuthenticatedAdmin_cache_isLoaded = False
                        property_user_isMember_isLoaded = False
                    Else
                        Call cpCore.log_LogActivity2("unsuccessful login (loginField:" & loginFieldValue & "/password:" & password & ")", id, organizationId)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   Member Login By ID
        '
        '========================================================================
        '
        Public Function authenticateById(ByVal irecordID As Integer, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim CS As Integer
                '
                returnREsult = recognizeById(irecordID)
                If returnREsult Then
                    '
                    ' Log them in
                    '
                    cpCore.authContext.visit.VisitAuthenticated = True
                    Call cpCore.authContext.saveObject(cpCore)
                    isAuthenticatedAdmin_cache_isLoaded = False
                    property_user_isMember_isLoaded = False
                    isAuthenticatedDeveloper_cache_isLoaded = False
                    '
                    ' Write Cookies in case Visit Tracking is off
                    '
                    If cpCore.authContext.visit.StartTime = Date.MinValue Then
                        cpCore.authContext.visit.StartTime = cpCore.app_startTime
                    End If
                    If Not cpCore.siteProperties.allowVisitTracking Then
                        cpCore.authContext = Models.Context.authContextModel.create(cpCore, True)
                    End If
                    '
                    ' Change autologin if included, selected, and allowed
                    '
                    If AllowAutoLogin Xor autoLogin Then
                        If cpCore.siteProperties.getBoolean("AllowAutoLogin") Then
                            CS = cpCore.csOpenRecord("people", irecordID)
                            If cpCore.db.cs_ok(CS) Then
                                Call cpCore.db.cs_set(CS, "AutoLogin", AllowAutoLogin)
                            End If
                            Call cpCore.db.cs_Close(CS)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   RecognizeMember
        '
        '   the current member to be non-authenticated, but recognized
        '========================================================================
        '
        Public Function recognizeById(ByVal RecordID As Integer) As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim CS As Integer
                Dim SQL As String
                '
                SQL = "select" _
                    & " ccMembers.*" _
                    & " ,ccLanguages.name as LanguageName" _
                    & " from" _
                    & " ccMembers" _
                    & " left join ccLanguages on ccMembers.LanguageID=ccLanguages.ID" _
                    & " where" _
                    & " (ccMembers.active<>" & SQLFalse & ")" _
                    & " and(ccMembers.ID=" & RecordID & ")"
                SQL &= "" _
                    & " and((ccMembers.dateExpires is null)or(ccMembers.dateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))" _
                    & ""
                CS = cpCore.db.cs_openSql(SQL)
                If cpCore.db.cs_ok(CS) Then
                    If cpCore.authContext.visit.ID = 0 Then
                        '
                        ' Visit was blocked during init, init the visit DateTime.Now
                        '
                        cpCore.authContext = Models.Context.authContextModel.create(cpCore, cpCore.siteProperties.allowVisitTracking)
                    End If
                    '
                    ' ----- Member was recognized
                    '   REFACTOR -- when the id is set, the user object is populated, so the rest of this can be removed (verify these are all set in the load
                    '
                    id = (cpCore.db.cs_getInteger(CS, "ID"))
                    name = (cpCore.db.cs_getText(CS, "Name"))
                    username = (cpCore.db.cs_getText(CS, "username"))
                    email = (cpCore.db.cs_getText(CS, "Email"))
                    password = (cpCore.db.cs_getText(CS, "Password"))
                    organizationId = (cpCore.db.cs_getInteger(CS, "OrganizationID"))
                    languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    active = (cpCore.db.cs_getBoolean(CS, "Active"))
                    company = (cpCore.db.cs_getText(CS, "Company"))
                    visits = (cpCore.db.cs_getInteger(CS, "Visits"))
                    lastVisit = (cpCore.db.cs_getDate(CS, "LastVisit"))
                    allowBulkEmail = (cpCore.db.cs_getBoolean(CS, "AllowBulkEmail"))
                    allowToolsPanel = (cpCore.db.cs_getBoolean(CS, "AllowToolsPanel"))
                    adminMenuModeID = (cpCore.db.cs_getInteger(CS, "AdminMenuModeID"))
                    autoLogin = (cpCore.db.cs_getBoolean(CS, "AutoLogin"))
                    isDeveloper = (cpCore.db.cs_getBoolean(CS, "Developer"))
                    isAdmin = (cpCore.db.cs_getBoolean(CS, "Admin"))
                    contentControlID = (cpCore.db.cs_getInteger(CS, "ContentControlID"))
                    languageId = (cpCore.db.cs_getInteger(CS, "LanguageID"))
                    language = (cpCore.db.cs_getText(CS, "LanguageName"))
                    styleFilename = cpCore.db.cs_getText(CS, "StyleFilename")
                    If styleFilename <> "" Then
                        Call cpCore.htmlDoc.main_AddStylesheetLink(cpCore.webServer.webServerIO_requestProtocol & cpCore.webServer.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.serverConfig.appConfig.cdnFilesNetprefix, styleFilename))
                    End If
                    excludeFromAnalytics = cpCore.db.cs_getBoolean(CS, "ExcludeFromAnalytics")
                    '
                    visits = visits + 1
                    If visits = 1 Then
                        isNew = True
                    Else
                        isNew = False
                    End If
                    lastVisit = cpCore.app_startTime
                    'cpCore.main_VisitMemberID = id
                    cpCore.authContext.visit.LoginAttempts = 0
                    cpCore.authContext.visitor.memberID = id
                    cpCore.authContext.visit.ExcludeFromAnalytics = cpCore.authContext.visit.ExcludeFromAnalytics Or cpCore.authContext.visit_isBot Or excludeFromAnalytics Or isAdmin Or isDeveloper
                    Call cpCore.authContext.saveObject(cpCore)
                    Call cpCore.authContext.visitor.saveObject(cpCore)
                    Call saveMemberBase()
                    returnREsult = True
                End If
                Call cpCore.db.cs_Close(CS)
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Create a new default user and save it
        '       If failure, MemberID is 0
        '       If successful, main_VisitMemberID and main_VisitorMemberID must be set to MemberID
        '========================================================================
        '
        Public Sub createUser()
            Try
                Dim CSMember As Integer
                Dim CSlanguage As Integer
                '
                Call createUserDefaults(cpCore.authContext.visit.Name)
                '
                id = 0
                CSMember = cpCore.db.cs_insertRecord("people")
                If Not cpCore.db.cs_ok(CSMember) Then
                    Call cpCore.handleExceptionAndRethrow(New ApplicationException("main_CreateUser, Error inserting new people record, could not main_CreateUser"))
                Else
                    id = cpCore.db.cs_getInteger(CSMember, "id")
                    Call cpCore.db.cs_set(CSMember, "CreatedByVisit", True)
                    '
                    active = True
                    Call cpCore.db.cs_set(CSMember, "active", active)
                    '
                    visits = 1
                    Call cpCore.db.cs_set(CSMember, "Visits", visits)
                    '
                    lastVisit = cpCore.app_startTime
                    Call cpCore.db.cs_set(CSMember, "LastVisit", lastVisit)
                    '
                    '
                    CSlanguage = cpCore.csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID(), SelectFieldList:="Name")
                    If cpCore.db.cs_ok(CSlanguage) Then
                        languageId = cpCore.db.cs_getInteger(CSlanguage, "ID")
                        language = cpCore.db.cs_getText(CSlanguage, "Name")
                        Call cpCore.db.cs_set(CSMember, "LanguageID", languageId)
                    End If
                    Call cpCore.db.cs_Close(CSlanguage)
                    '
                    userAdded = True
                    isNew = True
                    styleFilename = ""
                    excludeFromAnalytics = False
                    '
                    Call cpCore.db.cs_Close(CSMember)
                    '
                    'cpCore.main_VisitMemberID = id
                    cpCore.authContext.visitor.memberID = id
                    cpCore.authContext.visit.VisitAuthenticated = False
                    Call cpCore.authContext.saveObject(cpCore)
                    Call cpCore.authContext.visitor.saveObject(cpCore)
                    '
                    isAuthenticatedAdmin_cache_isLoaded = False
                    property_user_isMember_isLoaded = False
                    isAuthenticatedDeveloper_cache_isLoaded = False
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Creates the internal records for the user, but does not create
        '   a people record to save them
        '========================================================================
        '
        Public Sub createUserDefaults(ByVal DefaultName As String)
            Try
                Dim CSlanguage As Integer
                '
                id = 0
                name = DefaultName
                isAdmin = False
                isDeveloper = False
                organizationId = 0
                languageId = 0
                language = ""
                isNew = False
                email = ""
                allowBulkEmail = False
                allowToolsPanel = False
                autoLogin = False
                adminMenuModeID = 0
                userAdded = False
                username = ""
                password = ""
                contentControlID = 0
                active = False
                visits = 0
                lastVisit = cpCore.authContext.visit.StartTime
                company = ""
                user_Title = ""
                main_MemberAddress = ""
                main_MemberCity = ""
                main_MemberState = ""
                main_MemberZip = ""
                main_MemberCountry = ""
                main_MemberPhone = ""
                main_MemberFax = ""
                '
                active = True
                '
                visits = 1
                '
                lastVisit = cpCore.app_startTime
                '
                '
                CSlanguage = cpCore.csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID(), SelectFieldList:="Name")
                If cpCore.db.cs_ok(CSlanguage) Then
                    languageId = cpCore.db.cs_getInteger(CSlanguage, "ID")
                    language = cpCore.db.cs_getText(CSlanguage, "Name")
                End If
                Call cpCore.db.cs_Close(CSlanguage)
                '
                userAdded = True
                isNew = True
                styleFilename = ""
                excludeFromAnalytics = False
                '
                isAuthenticatedAdmin_cache_isLoaded = False
                property_user_isMember_isLoaded = False
                isAuthenticatedDeveloper_cache_isLoaded = False
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '=============================================================================
        '   main_SaveMemberBase()
        '       Saves the current Member record to the database
        '=============================================================================
        '
        Public Sub saveMemberBase()
            Try
                Dim SQL As String
                '
                If cpCore.authContext.visit_initialized Then
                    If (id > 0) Then
                        SQL = "UPDATE ccMembers SET " _
                        & " Name=" & cpCore.db.encodeSQLText(name) _
                        & ",username=" & cpCore.db.encodeSQLText(username) _
                        & ",email=" & cpCore.db.encodeSQLText(email) _
                        & ",password=" & cpCore.db.encodeSQLText(password) _
                        & ",OrganizationID=" & cpCore.db.encodeSQLNumber(organizationId) _
                        & ",LanguageID=" & cpCore.db.encodeSQLNumber(languageId) _
                        & ",Active=" & cpCore.db.encodeSQLBoolean(active) _
                        & ",Company=" & cpCore.db.encodeSQLText(company) _
                        & ",Visits=" & cpCore.db.encodeSQLNumber(visits) _
                        & ",LastVisit=" & cpCore.db.encodeSQLDate(lastVisit) _
                        & ",AllowBulkEmail=" & cpCore.db.encodeSQLBoolean(allowBulkEmail) _
                        & ",AllowToolsPanel=" & cpCore.db.encodeSQLBoolean(allowToolsPanel) _
                        & ",AdminMenuModeID=" & cpCore.db.encodeSQLNumber(adminMenuModeID) _
                        & ",AutoLogin=" & cpCore.db.encodeSQLBoolean(autoLogin)
                        SQL &= ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(excludeFromAnalytics)
                        SQL &= " WHERE ID=" & id & ";"
                        Call cpCore.db.executeSql(SQL)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Member Logout
        '   Create and assign a guest Member identity
        '========================================================================
        '
        Public Sub logout()
            Try
                Call cpCore.log_LogActivity2("logout", id, organizationId)
                '
                ' Clear MemberID for this page
                '
                Call createUser()
                '
                ' Clear cached permissions
                '
                isAuthenticatedAdmin_cache_isLoaded = False              ' true if main_IsAdminCache is initialized
                property_user_isMember_isLoaded = False
                isAuthenticatedDeveloper_cache_isLoaded = False
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        '   Returns the ID of a member given their Username and Password
        '
        '   If the Id can not be found, user errors are added with main_AddUserError and 0 is returned (false)
        '===================================================================================================
        '
        Public Function authenticateGetId(ByVal username As String, ByVal password As String) As Integer
            Dim returnUserId As Integer = 0
            Try
                Const badLoginUserError As String = "Your login was not successful. Please try again."
                '
                Dim SQL As String
                Dim recordIsAdmin As Boolean
                Dim recordIsDeveloper As Boolean
                Dim Criteria As String
                Dim CS As Integer
                Dim iPassword As String
                Dim allowEmailLogin As Boolean
                Dim allowNoPasswordLogin As Boolean
                Dim iLoginFieldValue As String
                '
                iLoginFieldValue = genericController.encodeText(username)
                iPassword = genericController.encodeText(password)
                '
                returnUserId = 0
                allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin")
                allowNoPasswordLogin = cpCore.siteProperties.getBoolean("allowNoPasswordLogin")
                If iLoginFieldValue = "" Then
                    '
                    ' ----- loginFieldValue blank, stop here
                    '
                    If allowEmailLogin Then
                        Call cpCore.error_AddUserError("A valid login requires a non-blank username or email.")
                    Else
                        Call cpCore.error_AddUserError("A valid login requires a non-blank username.")
                    End If
                ElseIf (Not allowNoPasswordLogin) And (iPassword = "") Then
                    '
                    ' ----- password blank, stop here
                    '
                    Call cpCore.error_AddUserError("A valid login requires a non-blank password.")
                ElseIf (cpCore.authContext.visit.LoginAttempts >= main_maxVisitLoginAttempts) Then
                    '
                    ' ----- already tried 5 times
                    '
                    Call cpCore.error_AddUserError(badLoginUserError)
                Else
                    If allowEmailLogin Then
                        '
                        ' login by username or email
                        '
                        Criteria = "((username=" & cpCore.db.encodeSQLText(iLoginFieldValue) & ")or(email=" & cpCore.db.encodeSQLText(iLoginFieldValue) & "))"
                    Else
                        '
                        ' login by username only
                        '
                        Criteria = "(username=" & cpCore.db.encodeSQLText(iLoginFieldValue) & ")"
                    End If
                    If True Then
                        Criteria = Criteria & "and((dateExpires is null)or(dateExpires>" & cpCore.db.encodeSQLDate(DateTime.Now) & "))"
                    End If
                    CS = cpCore.db.cs_open("People", Criteria, "id", SelectFieldList:="ID ,password,admin,developer", PageSize:=2)
                    If Not cpCore.db.cs_ok(CS) Then
                        '
                        ' ----- loginFieldValue not found, stop here
                        '
                        Call cpCore.error_AddUserError(badLoginUserError)
                    ElseIf (Not genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowDuplicateUsernames", False))) And (cpCore.db.cs_getRowCount(CS) > 1) Then
                        '
                        ' ----- AllowDuplicates is false, and there are more then one record
                        '
                        Call cpCore.error_AddUserError("This user account can not be used because the username is not unique on this website. Please contact the site administrator.")
                    Else
                        '
                        ' ----- search all found records for the correct password
                        '
                        Do While cpCore.db.cs_ok(CS)
                            returnUserId = 0
                            '
                            ' main_Get Id if password good
                            '
                            If (iPassword = "") Then
                                '
                                ' no-password-login -- allowNoPassword + no password given + account has no password + account not admin/dev/cm
                                '
                                recordIsAdmin = cpCore.db.cs_getBoolean(CS, "admin")
                                recordIsDeveloper = Not cpCore.db.cs_getBoolean(CS, "admin")
                                If allowNoPasswordLogin And (cpCore.db.cs_getText(CS, "password") = "") And (Not recordIsAdmin) And (recordIsDeveloper) Then
                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID")
                                    '
                                    ' verify they are in no content manager groups
                                    '
                                    SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(returnUserId) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                                        & ");"
                                    CS = cpCore.db.cs_openSql(SQL)
                                    If cpCore.db.cs_ok(CS) Then
                                        returnUserId = 0
                                    End If
                                    Call cpCore.db.cs_Close(CS)
                                End If
                            Else
                                '
                                ' password login
                                '
                                If genericController.vbLCase(cpCore.db.cs_getText(CS, "password")) = genericController.vbLCase(iPassword) Then
                                    returnUserId = cpCore.db.cs_getInteger(CS, "ID")
                                End If
                            End If
                            If returnUserId <> 0 Then
                                Exit Do
                            End If
                            Call cpCore.db.cs_goNext(CS)
                        Loop
                        If returnUserId = 0 Then
                            Call cpCore.error_AddUserError(badLoginUserError)
                        End If
                    End If
                    Call cpCore.db.cs_Close(CS)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnUserId
        End Function
        '
        '====================================================================================================
        '   Checks the username and password for a new login
        '       returns true if this can be used
        '       returns false, and a User Error response if it can not be used
        '
        Public Function isNewLoginOK(ByVal Username As String, ByVal Password As String, ByRef returnErrorMessage As String, ByRef returnErrorCode As Integer) As Boolean
            Dim returnOk As Boolean = False
            Try
                Dim CSPointer As Integer
                '
                returnOk = False
                If Username = "" Then
                    '
                    ' ----- username blank, stop here
                    '
                    returnErrorCode = 1
                    returnErrorMessage = "A valid login requires a non-blank username."
                ElseIf Password = "" Then
                    '
                    ' ----- password blank, stop here
                    '
                    returnErrorCode = 4
                    returnErrorMessage = "A valid login requires a non-blank password."
                    '    ElseIf Not main_VisitCookieSupport Then
                    '        '
                    '        ' No Cookie Support, can not log in
                    '        '
                    '        errorCode = 2
                    '        errorMessage = "You currently have cookie support disabled in your browser. Without cookies, your browser can not support the level of security required to login."
                Else

                    CSPointer = cpCore.db.cs_open("People", "username=" & cpCore.db.encodeSQLText(Username), "", False, SelectFieldList:="ID", PageSize:=2)
                    If cpCore.db.cs_ok(CSPointer) Then
                        '
                        ' ----- username was found, stop here
                        '
                        returnErrorCode = 3
                        returnErrorMessage = "The username you supplied is currently in use."
                    Else
                        returnOk = True
                    End If
                    Call cpCore.db.cs_Close(CSPointer)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnOk
        End Function
        '
        '====================================================================================================
        ' main_GetContentAccessRights( ContentIdOrName, returnAllowEdit, returnAllowAdd, returnAllowDelete )
        '
        Public Sub getContentAccessRights(ByVal ContentName As String, ByRef returnAllowEdit As Boolean, ByRef returnAllowAdd As Boolean, ByRef returnAllowDelete As Boolean)
            Try
                Dim ContentID As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                '
                returnAllowEdit = False
                returnAllowAdd = False
                returnAllowDelete = False
                If True Then
                    If Not isAuthenticated() Then
                        '
                        ' no authenticated, you are not a conent manager
                        '
                    ElseIf String.IsNullOrEmpty(ContentName) Then
                        '
                        ' no content given, do not handle the general case -- use authcontext.user.main_IsContentManager2()
                        '
                    ElseIf isAuthenticatedDeveloper() Then
                        '
                        ' developers are always content managers
                        '
                        returnAllowEdit = True
                        returnAllowAdd = True
                        returnAllowDelete = True
                    ElseIf isAuthenticatedAdmin() Then
                        '
                        ' admin is content manager if the CDef is not developer only
                        '
                        CDef = cpCore.metaData.getCdef(ContentName)
                        If CDef.Id <> 0 Then
                            If Not CDef.DeveloperOnly Then
                                returnAllowEdit = True
                                returnAllowAdd = True
                                returnAllowDelete = True
                            End If
                        End If
                    Else
                        '
                        ' Authenticated and not admin or developer
                        '
                        ContentID = cpCore.main_GetContentID(ContentName)
                        Call getContentAccessRights_NonAdminByContentId(ContentID, returnAllowEdit, returnAllowAdd, returnAllowDelete, "")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ' main_GetContentAccessRights_NonAdminByContentId
        '   Checks if the member is a content manager for the specific content,
        '   Which includes transversing up the tree to find the next rule that applies'
        '   Member must be checked for authenticated and main_IsAdmin already
        '========================================================================
        '
        Private Sub getContentAccessRights_NonAdminByContentId(ByVal ContentID As Integer, ByRef returnAllowEdit As Boolean, ByRef returnAllowAdd As Boolean, ByRef returnAllowDelete As Boolean, ByVal usedContentIdList As String)
            Try
                Dim SQL As String
                Dim CSPointer As Integer
                Dim ParentID As Integer
                Dim ContentName As String
                Dim CDef As coreMetaDataClass.CDefClass
                '
                returnAllowEdit = False
                returnAllowAdd = False
                returnAllowDelete = False
                If genericController.IsInDelimitedString(usedContentIdList, CStr(ContentID), ",") Then
                    '
                    ' failed usedContentIdList test, this content id was in the child path
                    '
                    Throw New ArgumentException("ContentID [" & ContentID & "] was found to be in it's own parentid path.")
                ElseIf ContentID < 1 Then
                    '
                    ' ----- not a valid contentname
                    '
                ElseIf genericController.IsInDelimitedString(contentAccessRights_NotList, CStr(ContentID), ",") Then
                    '
                    ' ----- was previously found to not be a Content Manager
                    '
                ElseIf genericController.IsInDelimitedString(contentAccessRights_List, CStr(ContentID), ",") Then
                    '
                    ' ----- was previously found to be a Content Manager
                    '
                    returnAllowEdit = True
                    returnAllowAdd = genericController.IsInDelimitedString(contentAccessRights_AllowAddList, CStr(ContentID), ",")
                    returnAllowDelete = genericController.IsInDelimitedString(contentAccessRights_AllowDeleteList, CStr(ContentID), ",")
                Else
                    '
                    ' ----- Must test it
                    '
                    SQL = "SELECT ccGroupRules.ContentID,allowAdd,allowDelete" _
                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                    & " WHERE (" _
                        & " (ccMemberRules.MemberID=" & cpCore.db.encodeSQLNumber(id) & ")" _
                        & " AND(ccMemberRules.active<>0)" _
                        & " AND(ccGroupRules.active<>0)" _
                        & " AND(ccGroupRules.ContentID=" & ContentID & ")" _
                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.encodeSQLDate(cpCore.app_startTime) & "))" _
                        & ");"
                    CSPointer = cpCore.db.cs_openSql(SQL)
                    If cpCore.db.cs_ok(CSPointer) Then
                        returnAllowEdit = True
                        returnAllowAdd = cpCore.db.cs_getBoolean(CSPointer, "allowAdd")
                        returnAllowDelete = cpCore.db.cs_getBoolean(CSPointer, "allowDelete")
                    End If
                    cpCore.db.cs_Close(CSPointer)
                    '
                    If Not returnAllowEdit Then
                        '
                        ' ----- Not a content manager for this one, check the parent
                        '
                        ContentName = cpCore.metaData.getContentNameByID(ContentID)
                        If ContentName <> "" Then
                            CDef = cpCore.metaData.getCdef(ContentName)
                            ParentID = CDef.parentID
                            If ParentID > 0 Then
                                Call getContentAccessRights_NonAdminByContentId(ParentID, returnAllowEdit, returnAllowAdd, returnAllowDelete, usedContentIdList & "," & CStr(ContentID))
                            End If
                        End If
                    End If
                    If returnAllowEdit Then
                        '
                        ' ----- Was found to be true
                        '
                        contentAccessRights_List &= "," & CStr(ContentID)
                        If returnAllowAdd Then
                            contentAccessRights_AllowAddList &= "," & CStr(ContentID)
                        End If
                        If returnAllowDelete Then
                            contentAccessRights_AllowDeleteList &= "," & CStr(ContentID)
                        End If
                    Else
                        '
                        ' ----- Was found to be false
                        '
                        contentAccessRights_NotList &= "," & CStr(ContentID)
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
    End Class
End Namespace