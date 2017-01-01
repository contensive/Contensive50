
Option Explicit On
Option Strict On

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' </summary>
    Public Class coreUserClass
        '
        Private cpCore As cpCoreClass
        '
        '====================================================================================================
        '
        Friend userId As Integer = 0                     ' ID into member table  (memberid=0 means untracked guest)
        Friend userName As String = ""                 '
        Friend userIsAdmin As Boolean = False              '
        Friend userIsDeveloper As Boolean = False          '
        Friend userOrganizationId As Integer = 0         ' The members Organization
        Friend userLanguageId As Integer = 0             '
        Friend userLanguage As String = ""             '
        Friend userIsNew As Boolean = False                ' stored in visit record - Is this the first visit for this member
        Friend userEmail As String = ""               '
        '
        Friend user_allowBulkEmail As Boolean = False      ' Allow bulk mail
        Friend userAllowToolsPanel As Boolean = False    '
        Friend user_autoLogin As Boolean = False         ' if true, and setup AllowAutoLogin then use cookie to login
        Friend user_adminMenuModeID As Integer = 0     '
        '
        Friend userAdded As Boolean = False              ' depricated - true only during the page that the join was completed - use for redirections and GroupAdds
        Friend userUsername As String = ""
        Friend userPassword As String = ""
        Friend userContentControlID As Integer = 0
        '
        Friend userStyleFilename As String = ""          ' if not empty, add to head
        Friend userExcludeFromAnalytics As Boolean = False   ' if true, future visits will be marked exclude from analytics
        '
        ' Private iMemberLanguage as object              ' populated on demand from main_MemberLanguageID
        '
        Private property_user_isAdmin As Boolean = False               ' true if member is administrator
        Private property_user_isAdmin_isLoaded As Boolean = False              ' true if main_IsAdminCache is initialized
        '
        Private property_user_isMember As Boolean = False
        Private property_user_isMember_isLoaded As Boolean = False
        '
        Private property_user_isAuthenticated As Boolean = False
        Private property_user_isAuthenticated_isLoaded As Boolean = False
        '
        Private property_user_isDeveloper As Boolean = False
        Private property_user_isDeveloper_isLoaded As Boolean = False
        '
        Public main_IsEditingContentList As String = ""
        Public main_IsNotEditingContentList As String = ""
        '
        '-----------------------------------------------------------------------
        ' ----- Member Private
        '-----------------------------------------------------------------------
        '
        Public user_Active As Boolean = False           '
        Public user_visits As Integer = 0                '
        Public user_lastVisit As Date = New Date().MinValue             ' The last visit by the Member (the beginning of this visit
        '
        Public user_company As String = ""
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
        Public user_billEmail As String = ""          ' Billing Address for purchases
        Public user_billPhone As String = ""          '
        Public user_billFax As String = ""            '
        Public main_MemberBillCompany As String = ""        '
        Public main_MemberBillAddress As String = ""        '
        Public main_MemberBillCity As String = ""           '
        Public main_MemberBillState As String = ""         '
        Public main_MemberBillZip As String = ""            '
        Public main_MemberBillCountry As String = ""       '
        '
        Public main_MemberShipName As String = ""          ' Mailing Address
        Public main_MemberShipCompany As String = ""           '
        Public main_MemberShipAddress As String = ""          '
        Public main_MemberShipCity As String = ""          '
        Public main_MemberShipState As String = ""        '
        Public main_MemberShipZip As String = ""           '
        Public main_MemberShipCountry As String = ""         '
        Public main_MemberShipPhone As String = ""        '
        '
        '----------------------------------------------------------------------------------------------------
        '
        Public loginForm_Username As String = ""       ' Values entered with the login form
        Public loginForm_Password As String = ""       '   =
        Public loginForm_Email As String = ""          '   =
        Public loginForm_AutoLogin As Boolean = False    '   =
        '
        '
        Public Const main_maxVisitLoginAttempts = 20
        Public main_loginFormDefaultProcessed As Boolean = False       ' prevent main_ProcessLoginFormDefault from running twice (multiple user messages, popups, etc.)
        '====================================================================================================
        '
        '
        '========================================================================
        '   IsGuest
        '       A guest is a first time visitor
        '========================================================================
        '
        Public Function user_IsGuest() As Boolean
            user_IsGuest = Not user_IsMember()
        End Function
        '
        '========================================================================
        '   main_IsRecognized
        '   true if the person has a member record from a previous visit
        '   this may be because they are authenticated, or the person may
        '   have returned without authentication, but they are using thier
        '   member record because it was recognized from their cookie.
        '========================================================================
        '
        Public Function user_isRecognized() As Boolean
            user_isRecognized = (Not userIsNew)
        End Function
        '
        '========================================================================
        '   main_IsAuthenticated
        '   true if the person has logged in (must be member)
        '========================================================================
        '
        Public Function user_isAuthenticated() As Boolean
            Return cpCore.visit_isAuthenticated
        End Function
        '
        '========================================================================
        ' main_IsEditing( ContentName )
        '   True if the current visitor is authoring the content specified
        '   If main_IsContentManager2( ContentName ) and Allow Authoring then true
        '========================================================================
        '
        Public Function user_isEditingAnything() As Boolean
            Return user_isEditing("")
        End Function
        '
        '========================================================================
        ' main_IsEditing( ContentName )
        '   True if the current visitor is authoring the content specified
        '   If main_IsContentManager2( ContentName ) and Allow Authoring then true
        '========================================================================
        '
        Public Function user_isEditing(ByVal ContentNameOrId As String) As Boolean
            Dim returnResult As Boolean = False
            Try
                If True Then
                    Dim localContentNameOrId As String
                    Dim cacheTestName As String
                    '
                    If Not cpCore.visit_initialized Then
                        Call cpCore.testPoint("...visit not initialized")
                    Else
                        '
                        ' always false until visit loaded
                        '
                        localContentNameOrId = EncodeText(ContentNameOrId)
                        cacheTestName = localContentNameOrId
                        If cacheTestName = "" Then
                            cacheTestName = "iseditingall"
                        End If
                        cacheTestName = LCase(cacheTestName)
                        If IsInDelimitedString(main_IsEditingContentList, cacheTestName, ",") Then
                            Call cpCore.testPoint("...is in main_IsEditingContentList")
                            returnResult = True
                        ElseIf IsInDelimitedString(main_IsNotEditingContentList, cacheTestName, ",") Then
                            Call cpCore.testPoint("...is in main_IsNotEditingContentList")
                        Else
                            If user_isAuthenticated() Then
                                If Not cpCore.main_ServerPagePrintVersion Then
                                    If (cpCore.main_VisitProperty_AllowEditing Or cpCore.main_VisitProperty_AllowAdvancedEditor) Then
                                        If localContentNameOrId <> "" Then
                                            If IsNumeric(localContentNameOrId) Then
                                                localContentNameOrId = cpCore.main_GetContentNameByID(EncodeInteger(localContentNameOrId))
                                            End If
                                        End If
                                        returnResult = main_IsContentManager(localContentNameOrId)
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
        ' main_IsQuickEditing( ContentName )
        '   True if the current visitor is authoring the content specified
        '   If main_IsContentManager2( ContentName ) and Allow Authoring then true
        '========================================================================
        '
        Public Function user_isQuickEditing(ByVal ContentName As String) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsQuickEditing")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "main_IsQuickEditing"
            '
            ' save the visit property hit if not contentmanager
            '
            If (Not cpCore.main_ServerPagePrintVersion) Then
                If main_IsContentManager(EncodeText(ContentName)) Then
                    user_isQuickEditing = cpCore.main_VisitProperty_AllowQuickEditor
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        ' main_IsAdvancedEditing( ContentName )
        '   True if the current visitor is authoring the content specified
        '   If main_IsContentManager2( ContentName ) and Allow Authoring then true
        '========================================================================
        '
        Public Function user_IsAdvancedEditing(ByVal ContentName As String) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsAdvancedEditing")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "main_IsAdvancedEditing"
            '
            ' save the visit property hit if not contentmanager
            '
            If (Not cpCore.main_ServerPagePrintVersion) Then
                If main_IsContentManager(EncodeText(ContentName)) Then
                    user_IsAdvancedEditing = cpCore.main_VisitProperty_AllowAdvancedEditor
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
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
        Public Function user_isAdmin() As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not property_user_isAdmin_isLoaded) And cpCore.visit_initialized Then
                    property_user_isAdmin = user_isAuthenticated() And (userIsAdmin Or userIsDeveloper)
                    property_user_isAdmin_isLoaded = True
                End If
                returnIs = property_user_isAdmin
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        '
        '========================================================================
        '   main_IsDeveloper
        '========================================================================
        '
        Public Function user_isDeveloper() As Boolean
            Dim returnIs As Boolean = False
            Try
                If (Not property_user_isDeveloper_isLoaded) And cpCore.visit_initialized Then
                    property_user_isDeveloper = (user_isAuthenticated() And userIsDeveloper)
                    property_user_isDeveloper_isLoaded = True
                End If
                returnIs = property_user_isDeveloper
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIs
        End Function
        '
        '=============================================================================
        '   main_SaveMember()
        '       Saves the member properties that are loaded during main_OpenMember
        '=============================================================================
        '
        Public Sub user_SaveMember()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim SQL As String
            Dim CS As Integer
            Dim MethodName As String
            '
            MethodName = "main_SaveMember"
            '
            If cpCore.visit_initialized Then
                If (userId > 0) Then
                    SQL = "UPDATE ccMembers SET " _
                        & " Name=" & cpCore.db.encodeSQLText(userName) _
                        & ",username=" & cpCore.db.encodeSQLText(userUsername) _
                        & ",email=" & cpCore.db.encodeSQLText(userEmail) _
                        & ",password=" & cpCore.db.encodeSQLText(userPassword) _
                        & ",OrganizationID=" & cpCore.db.db_EncodeSQLNumber(userOrganizationId) _
                        & ",LanguageID=" & cpCore.db.db_EncodeSQLNumber(userLanguageId) _
                        & ",Active=" & cpCore.db.db_EncodeSQLBoolean(user_Active) _
                        & ",Company=" & cpCore.db.encodeSQLText(user_company) _
                        & ",Visits=" & cpCore.db.db_EncodeSQLNumber(user_visits) _
                        & ",LastVisit=" & cpCore.db.db_EncodeSQLDate(user_lastVisit) _
                        & ",AllowBulkEmail=" & cpCore.db.db_EncodeSQLBoolean(user_allowBulkEmail) _
                        & ",AdminMenuModeID=" & cpCore.db.db_EncodeSQLNumber(user_adminMenuModeID) _
                        & ",AutoLogin=" & cpCore.db.db_EncodeSQLBoolean(user_autoLogin)
                    ' 6/18/2009 - removed notes from base
                    '           & ",SendNotes=" & encodeSQLBoolean(MemberSendNotes)
                    SQL &= "" _
                        & ",BillEmail=" & cpCore.db.encodeSQLText(user_billEmail) _
                        & ",BillPhone=" & cpCore.db.encodeSQLText(user_billPhone) _
                        & ",BillFax=" & cpCore.db.encodeSQLText(user_billFax) _
                        & ",BillCompany=" & cpCore.db.encodeSQLText(main_MemberBillCompany) _
                        & ",BillAddress=" & cpCore.db.encodeSQLText(main_MemberBillAddress) _
                        & ",BillCity=" & cpCore.db.encodeSQLText(main_MemberBillCity) _
                        & ",BillState=" & cpCore.db.encodeSQLText(main_MemberBillState) _
                        & ",BillZip=" & cpCore.db.encodeSQLText(main_MemberBillZip) _
                        & ",BillCountry=" & cpCore.db.encodeSQLText(main_MemberBillCountry)
                    SQL &= "" _
                        & ",ShipName=" & cpCore.db.encodeSQLText(main_MemberShipName) _
                        & ",ShipCompany=" & cpCore.db.encodeSQLText(main_MemberShipCompany) _
                        & ",ShipAddress=" & cpCore.db.encodeSQLText(main_MemberShipAddress) _
                        & ",ShipCity=" & cpCore.db.encodeSQLText(main_MemberShipCity) _
                        & ",ShipState=" & cpCore.db.encodeSQLText(main_MemberShipState) _
                        & ",ShipZip=" & cpCore.db.encodeSQLText(main_MemberShipZip) _
                        & ",ShipCountry=" & cpCore.db.encodeSQLText(main_MemberShipCountry) _
                        & ",ShipPhone=" & cpCore.db.encodeSQLText(main_MemberShipPhone)
                    If True Then
                        SQL &= ",ExcludeFromAnalytics=" & cpCore.db.db_EncodeSQLBoolean(userExcludeFromAnalytics)
                    End If
                    SQL &= " WHERE ID=" & userId & ";"
                    Call cpCore.db.executeSql(SQL)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Sub
        '
        '=============================================================================
        '------- 9/3/2012
        '   main_GetLoginForm - returns either the default login form, or the one selected
        '   main_GetLoginPanel - just returns main_GetLoginForm
        '   main_GetLoginPage - wraps main_GetLoginForm
        '=============================================================================
        '
        Public Function user_GetLoginForm() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLoginForm")
            '
            Dim loginAddonID As Integer
            Dim returnHtml As String
            Dim isAddonOk As Boolean
            Dim QS As String
            '
            loginAddonID = EncodeInteger(cpCore.siteProperties.getText("Login Page AddonID", "0"))
            If loginAddonID <> 0 Then
                '
                ' Custom Login
                '
                returnHtml = cpCore.executeAddon_legacy2(loginAddonID, "", "", cpCoreClass.addonContextEnum.ContextPage, "", 0, "", "", False, 0, "", isAddonOk, Nothing)
                If Not isAddonOk Then
                    loginAddonID = 0
                ElseIf (returnHtml = "") And (isAddonOk) Then
                    '
                    ' login successful, redirect back to this page (without a method)
                    '
                    QS = cpCore.web_RefreshQueryString
                    QS = ModifyQueryString(QS, "method", "")
                    QS = ModifyQueryString(QS, "RequestBinary", "")
                    '
                    Call cpCore.web_Redirect2("?" & QS, "Login form success", False)
                End If
            End If
            If loginAddonID = 0 Then
                '
                ' ----- When page loads, set focus on login username
                '
                returnHtml = cpCore.main_GetLoginForm_Default()
            End If
            '
            user_GetLoginForm = returnHtml
            '
            Exit Function
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & "main_GetLoginForm"))
        End Function
        '
        '=============================================================================
        ' Print a simple email password form
        '=============================================================================
        '
        Public Function user_GetSendPasswordForm() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetSendPasswordForm")
            '
            'If Not (true) Then Exit Function
            '
            Dim s As String
            Dim RequestElementVariant As Object
            Dim MethodName As String
            Dim ButtonList As String
            Dim ButtonPanel As String
            Dim QueryString As String
            Dim Pointer As Integer
            '
            MethodName = "main_GetSendPasswordForm"
            '
            If EncodeBoolean(cpCore.siteProperties.getBoolean("allowPasswordEmail", True)) Then
                s = "" _
                    & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td style=""text-align:right;vertical-align:middle;width:30%;padding:4px"" align=""right"" width=""30%"">" & SpanClassAdminNormal & "Email</span></td>" _
                    & cr3 & "<td style=""text-align:left;vertical-align:middle;width:70%;padding:4px"" align=""left""  width=""70%""><input NAME=""" & "email"" VALUE=""" & html_EncodeHTML(loginForm_Email) & """ SIZE=""20"" MAXLENGTH=""50""></td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">&nbsp;</td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">" _
                    & kmaIndent(kmaIndent(cpCore.main_GetPanelButtons(ButtonSendPassword, "Button"))) _
                    & cr3 & "</td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>" _
                    & ""
                '
                ' write out all of the form input (except state) to hidden fields so they can be read after login
                '
                '
                s = "" _
                    & s _
                    & cpCore.html_GetFormInputHidden("Type", FormTypeSendPassword) _
                    & ""
                If cpCore.web_InStreamArrayCount > 0 Then
                    For Pointer = 0 To cpCore.web_InStreamArrayCount - 1
                        With cpCore.web_InStreamArray(Pointer)
                            If .IsForm Then
                                Select Case UCase(.Name)
                                    Case "S", "MA", "MB", "USERNAME", "PASSWORD", "EMAIL"
                                    Case Else
                                        s = s & cpCore.html_GetFormInputHidden(.Name, .Value)
                                End Select
                            End If
                        End With
                    Next
                End If
                '
                QueryString = cpCore.web_RefreshQueryString
                QueryString = ModifyQueryString(QueryString, "S", "")
                QueryString = ModifyQueryString(QueryString, "ccIPage", "")
                s = "" _
                    & cpCore.main_GetFormStart(QueryString) _
                    & kmaIndent(s) _
                    & cr & "</form>" _
                    & ""
            End If
            user_GetSendPasswordForm = s
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '   admins are always returned true
        '===============================================================================================================================
        '
        Public Function user_isMemberOfGroupIdList(ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String) As Boolean
            user_isMemberOfGroupIdList = user_isMemberOfGroupIdList2(MemberID, isAuthenticated, GroupIDList, True)
        End Function
        '
        '===============================================================================================================================
        '   Is Group Member of a GroupIDList
        '===============================================================================================================================
        '
        Public Function user_isMemberOfGroupIdList2(ByVal MemberID As Integer, ByVal isAuthenticated As Boolean, ByVal GroupIDList As String, ByVal adminReturnsTrue As Boolean) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "IsGroupIDListMember" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim SQL As String
            Dim Criteria As String
            Dim WorkingIDList As String
            '
            user_isMemberOfGroupIdList2 = False
            If isAuthenticated Then
                WorkingIDList = GroupIDList
                WorkingIDList = Replace(WorkingIDList, " ", "")
                Do While InStr(1, WorkingIDList, ",,") <> 0
                    WorkingIDList = Replace(WorkingIDList, ",,", ",")
                Loop
                If (WorkingIDList <> "") Then
                    If Left(WorkingIDList, 1) = "," Then
                        If Len(WorkingIDList) <= 1 Then
                            WorkingIDList = ""
                        Else
                            WorkingIDList = Mid(WorkingIDList, 2)
                        End If
                    End If
                End If
                If (WorkingIDList <> "") Then
                    If Right(WorkingIDList, 1) = "," Then
                        If Len(WorkingIDList) <= 1 Then
                            WorkingIDList = ""
                        Else
                            WorkingIDList = Mid(WorkingIDList, 1, Len(WorkingIDList) - 1)
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
                        CS = cpCore.db.db_openCsSql_rev("default", SQL)
                        user_isMemberOfGroupIdList2 = cpCore.db.db_csOk(CS)
                        Call cpCore.db.db_csClose(CS)
                    End If
                Else
                    '
                    ' check if they are admin or in the group list
                    '
                    If InStr(1, WorkingIDList, ",") <> 0 Then
                        Criteria = "r.GroupID in (" & WorkingIDList & ")"
                    Else
                        Criteria = "r.GroupID=" & WorkingIDList
                    End If
                    Criteria = "" _
                        & "(" & Criteria & ")" _
                        & " and(r.id is not null)" _
                        & " and((r.DateExpires is null)or(r.DateExpires>" & cpCore.db.db_EncodeSQLDate(Now) & "))" _
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
                    CS = cpCore.db.db_openCsSql_rev("default", SQL)
                    user_isMemberOfGroupIdList2 = cpCore.db.db_csOk(CS)
                    Call cpCore.db.db_csClose(CS)
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            cpCore.handleExceptionAndRethrow(New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================
        ' Member Open
        '   Attempts to open the Member record based on the iRecordID
        '   If successful, MemberID is set to the iRecordID
        '========================================================================
        '
        Public Sub main_OpenMember(RecordID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("OpenMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            Dim MethodName As String
            Dim iRecordID As Integer
            '
            iRecordID = EncodeInteger(RecordID)
            '
            MethodName = "main_OpenMember"
            '
            If iRecordID <> 0 Then
                '
                ' attempt to read in Member record if logged on
                ' dont just do main_CheckMember() -- in case a pretty login is needed
                '
                CS = cpCore.db_csOpenRecord("People", iRecordID)
                If Not cpCore.db.db_csOk(CS) Then
                Else
                    userId = (cpCore.db.db_GetCSInteger(CS, "ID"))
                    userName = (cpCore.db.db_GetCSText(CS, "Name"))
                    userIsDeveloper = (cpCore.db.db_GetCSBoolean(CS, "Developer"))
                    userIsAdmin = (cpCore.db.db_GetCSBoolean(CS, "Admin"))
                    userContentControlID = (cpCore.db.db_GetCSInteger(CS, "ContentControlID"))
                    userOrganizationId = (cpCore.db.db_GetCSInteger(CS, "OrganizationID"))
                    userLanguageId = (cpCore.db.db_GetCSInteger(CS, "LanguageID"))
                    userLanguage = (cpCore.main_GetCSEncodedField(CS, "LanguageID"))
                    '
                    '  main_MemberCompany = main_GetCSField_Internal(CS, "Company")
                    '  main_MemberTitle = main_GetCSField_Internal(CS, "Title")
                    '  main_MemberAddress = main_GetCSField_Internal(CS, "Address")
                    '  main_MemberCity = main_GetCSField_Internal(CS, "City")
                    '  main_MemberState = main_GetCSField_Internal(CS, "State")
                    '  main_MemberZip = main_GetCSField_Internal(CS, "Zip")
                    '  main_MemberCountry = main_GetCSField_Internal(CS, "Country")
                    '  MemberEmail = main_GetCSField_Internal(CS, "Email")
                    '  main_MemberPhone = main_GetCSField_Internal(CS, "Phone")
                    '
                    main_MemberShipName = (cpCore.db.db_GetCSText(CS, "ShipName"))
                    main_MemberShipCompany = (cpCore.db.db_GetCSText(CS, "ShipCompany"))
                    main_MemberShipAddress = (cpCore.db.db_GetCSText(CS, "ShipAddress"))
                    main_MemberShipCity = (cpCore.db.db_GetCSText(CS, "ShipCity"))
                    main_MemberShipState = (cpCore.db.db_GetCSText(CS, "ShipState"))
                    main_MemberShipZip = (cpCore.db.db_GetCSText(CS, "ShipZip"))
                    main_MemberShipCountry = (cpCore.db.db_GetCSText(CS, "ShipCountry"))
                    main_MemberShipPhone = (cpCore.db.db_GetCSText(CS, "ShipPhone"))
                    '
                    main_MemberBillCompany = (cpCore.db.db_GetCSText(CS, "BillCompany"))
                    main_MemberBillAddress = (cpCore.db.db_GetCSText(CS, "BillAddress"))
                    main_MemberBillCity = (cpCore.db.db_GetCSText(CS, "BillCity"))
                    main_MemberBillState = (cpCore.db.db_GetCSText(CS, "BillState"))
                    main_MemberBillZip = (cpCore.db.db_GetCSText(CS, "BillZip"))
                    main_MemberBillCountry = (cpCore.db.db_GetCSText(CS, "BillCountry"))
                    user_billEmail = (cpCore.db.db_GetCSText(CS, "BillEmail"))
                    user_billPhone = (cpCore.db.db_GetCSText(CS, "BillPhone"))
                    user_billFax = (cpCore.db.db_GetCSText(CS, "BillFax"))
                    '
                    user_allowBulkEmail = (cpCore.db.db_GetCSBoolean(CS, "AllowBulkEmail"))
                    userAllowToolsPanel = (cpCore.db.db_GetCSBoolean(CS, "AllowToolsPanel"))
                    user_adminMenuModeID = (cpCore.db.db_GetCSInteger(CS, "AdminMenuModeID"))
                    user_autoLogin = (cpCore.db.db_GetCSBoolean(CS, "AutoLogin"))
                    ' 6/18/2009 - removed notes from base
                    '            MemberSendNotes = (app.csv_GetCSBoolean(CS, "SendNotes"))
                    '
                    If True Then
                        userStyleFilename = cpCore.db.db_GetCSText(CS, "StyleFilename")
                        If userStyleFilename <> "" Then
                            Call cpCore.main_AddStylesheetLink(cpCore.web_requestProtocol & cpCore.web.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.appConfig.cdnFilesNetprefix, userStyleFilename))
                        End If
                    End If
                    If True Then
                        userExcludeFromAnalytics = cpCore.db.db_GetCSBoolean(CS, "ExcludeFromAnalytics")
                    End If
                    '
                    '
                    ' Removed
                    '
                    '  Member_AllowLinkAuthoring = (app.csv_GetCSBoolean(CS, "AllowAdminLinks"))
                    '  main_MemberVisits = main_GetCSField_Internal(CS, "Visits")
                    '  main_MemberLastVisit = main_GetCSField_Internal(CS, "LastVisit")
                    '  MemberActive = main_GetCSField_Internal(CS, "Active")
                    '  main_MemberUsername = main_GetCSField_Internal(CS, "username")
                    '  main_MemberPassword = main_GetCSField_Internal(CS, "Password")
                    '
                End If
                Call cpCore.db.db_csClose(CS)
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Sub
        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group named
        '========================================================================
        '
        Public Function user_IsGroupMember(ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0) As Boolean
            Dim iMemberID As Integer
            iMemberID = EncodeInteger(checkMemberID)
            If iMemberID = 0 Then
                iMemberID = userId
            End If
            user_IsGroupMember = user_IsGroupListMember2("," & cpCore.main_GetGroupID(EncodeText(GroupName)), iMemberID, True)
        End Function
        '
        '========================================================================
        ' ----- Returns true if the visitor is a member, and in the group named
        '========================================================================
        '
        Public Function user_IsGroupMember2(ByVal GroupName As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            Dim iMemberID As Integer
            iMemberID = checkMemberID
            If iMemberID = 0 Then
                iMemberID = userId
            End If
            user_IsGroupMember2 = user_IsGroupListMember2("," & cpCore.main_GetGroupID(EncodeText(GroupName)), iMemberID, adminReturnsTrue)
        End Function

        '
        '========================================================================
        ' ----- Returns true if the visitor is an admin, or authenticated and in the group list
        '========================================================================
        '
        Public Function user_IsGroupListMember2(ByVal GroupIDList As String, Optional ByVal checkMemberID As Integer = 0, Optional ByVal adminReturnsTrue As Boolean = False) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsGroupListMember2")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iCheckMemberID As Integer
            '
            MethodName = "IsGroupListMember2"
            iCheckMemberID = checkMemberID
            If iCheckMemberID = 0 Then
                iCheckMemberID = userId
            End If
            'iCheckMemberID = encodeMissingInteger(checkMemberID, memberID)
            user_IsGroupListMember2 = user_isMemberOfGroupIdList2(iCheckMemberID, user_isAuthenticated(), GroupIDList, adminReturnsTrue)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        '   IsMember
        '   true if the user is authenticated and is a trusted people (member content)
        '========================================================================
        '
        Public Function user_IsMember() As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsMember")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "IsMember"
            '
            If (Not property_user_isMember_isLoaded) And (cpCore.visit_initialized) Then
                property_user_isMember = user_isAuthenticated() And cpCore.db_IsWithinContent(userContentControlID, cpCore.main_GetContentID("members"))
                property_user_isMember_isLoaded = True
            End If
            user_IsMember = property_user_isMember
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        ' ----- Process the login form
        '========================================================================
        '
        Friend Function user_ProcessLoginFormDefault() As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessLoginFormDefault")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim Button As String
            Dim CS As Integer
            Dim LoginErrorMessage As String
            Dim LocalMemberID As Integer
            '
            MethodName = "main_ProcessLoginFormDefault()"
            user_ProcessLoginFormDefault = False
            '
            If Not main_loginFormDefaultProcessed Then
                '
                ' Processing can happen
                '   1) early in init() -- legacy
                '   2) as well as at the front of main_GetLoginForm - to support addon Login forms
                ' This flag prevents the default form from processing twice
                '
                main_loginFormDefaultProcessed = True
                loginForm_Username = cpCore.doc_getText("username")
                loginForm_Password = cpCore.doc_getText("password")
                loginForm_AutoLogin = cpCore.main_GetStreamBoolean2("autologin")
                '
                If (cpCore.main_VisitLoginAttempts < main_maxVisitLoginAttempts) And (cpCore.main_VisitCookieSupport) Then
                    LocalMemberID = cpCore.main_GetLoginMemberID(loginForm_Username, loginForm_Password)
                    If LocalMemberID = 0 Then
                        cpCore.main_VisitLoginAttempts = cpCore.main_VisitLoginAttempts + 1
                        Call cpCore.main_SaveVisit()
                    Else
                        user_ProcessLoginFormDefault = user_LoginMemberByID(LocalMemberID, loginForm_AutoLogin)
                        If user_ProcessLoginFormDefault Then
                            Call cpCore.main_LogActivity2("successful username/password login", userId, userOrganizationId)
                        Else
                            Call cpCore.main_LogActivity2("bad username/password login", userId, userOrganizationId)
                        End If
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
        End Function
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Sub user_ProcessFormSendPassword()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessFormSendPassword")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_ProcessFormSendPassword()"
            '
            ' ----- lookup a Member account and send the username/password
            '
            loginForm_Email = cpCore.doc_getText("email")
            Call security_SendMemberPassword(loginForm_Email)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Sub
        '
        '=============================================================================
        ' Send the Member his username and password
        '=============================================================================
        '
        Public Function security_SendMemberPassword(ByVal Email As Object) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SendMemberPassword")
            '
            'If Not (true) Then Exit Function
            '
            Dim sqlCriteria As String
            Dim Message As String
            Dim CS As Integer
            Dim MethodName As String
            Dim workingEmail As String
            Dim FromAddress As String
            Dim subject As String
            Dim allowEmailLogin As Boolean
            Dim Password As String
            Dim Username As String
            Dim updateUser As Boolean
            Dim atPtr As Integer
            Dim Cnt As Integer
            Dim Index As Integer
            Dim EMailName As String
            Dim usernameOK As Boolean
            Dim recordCnt As Integer
            Dim hint As String
            Dim Ptr As Integer
            '
            Const passwordChrs = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345678999999"
            Const passwordChrsLength = 62
            '
            'hint = "100"
            workingEmail = EncodeText(Email)
            '
            MethodName = "main_SendMemberPassword"
            '
            security_SendMemberPassword = False
            If workingEmail = "" Then
                'hint = "110"
                cpCore.error_AddUserError("Please enter your email address before requesting your username and password.")
            Else
                'hint = "120"
                atPtr = InStr(1, workingEmail, "@")
                If atPtr < 2 Then
                    '
                    ' email not valid
                    '
                    'hint = "130"
                    cpCore.error_AddUserError("Please enter a valid email address before requesting your username and password.")
                Else
                    'hint = "140"
                    EMailName = Mid(workingEmail, 1, atPtr - 1)
                    '
                    Call cpCore.main_LogActivity2("password request for email " & workingEmail, userId, userOrganizationId)
                    '
                    allowEmailLogin = cpCore.siteProperties.getBoolean("allowEmailLogin", False)
                    recordCnt = 0
                    sqlCriteria = "(email=" & cpCore.db.encodeSQLText(workingEmail) & ")"
                    If True Then
                        sqlCriteria = sqlCriteria & "and((dateExpires is null)or(dateExpires>" & cpCore.db.db_EncodeSQLDate(Now) & "))"
                    End If
                    CS = cpCore.db.db_csOpen("People", sqlCriteria, "ID", , , ,, "username,password", 1)
                    If Not cpCore.db.db_csOk(CS) Then
                        '
                        ' valid login account for this email not found
                        '
                        If (LCase(Mid(workingEmail, atPtr + 1)) = "contensive.com") Then
                            '
                            ' look for expired account to renew
                            '
                            Call cpCore.db.db_csClose(CS)
                            CS = cpCore.db.db_csOpen("People", "((email=" & cpCore.db.encodeSQLText(workingEmail) & "))", "ID", , , , , , 1)
                            If cpCore.db.db_csOk(CS) Then
                                '
                                ' renew this old record
                                '
                                'hint = "150"
                                Call cpCore.db.db_SetCSField(CS, "developer", "1")
                                Call cpCore.db.db_SetCSField(CS, "admin", "1")
                                Call cpCore.db.db_SetCSField(CS, "dateExpires", Now.AddDays(7).Date.ToString)
                            Else
                                '
                                ' inject support record
                                '
                                'hint = "150"
                                Call cpCore.db.db_csClose(CS)
                                CS = cpCore.db.db_csInsertRecord("people")
                                Call cpCore.db.db_SetCSField(CS, "name", "Contensive Support")
                                Call cpCore.db.db_SetCSField(CS, "email", workingEmail)
                                Call cpCore.db.db_SetCSField(CS, "developer", "1")
                                Call cpCore.db.db_SetCSField(CS, "admin", "1")
                                Call cpCore.db.db_SetCSField(CS, "dateExpires", Now.AddDays(7).Date.ToString)
                            End If
                            Call cpCore.db.db_SaveCSRecord(CS)
                        Else
                            'hint = "155"
                            cpCore.error_AddUserError("No current user was found matching this email address. Please try again. ")
                        End If
                    End If
                    If cpCore.db.db_csOk(CS) Then
                        'hint = "160"
                        FromAddress = cpCore.siteProperties.getText("EmailFromAddress", "info@" & cpCore.main_ServerDomain)
                        subject = "Password Request at " & cpCore.main_ServerDomain
                        Message = ""
                        Do While cpCore.db.db_csOk(CS)
                            'hint = "170"
                            updateUser = False
                            If Message = "" Then
                                'hint = "180"
                                Message = "This email was sent in reply to a request at " & cpCore.main_ServerDomain & " for the username and password associated with this email address. "
                                Message = Message & "If this request was made by you, please return to the login screen and use the following:" & vbCrLf
                                Message = Message & vbCrLf
                            Else
                                'hint = "190"
                                Message = Message & vbCrLf
                                Message = Message & "Additional user accounts with the same email address: " & vbCrLf
                            End If
                            '
                            ' username
                            '
                            'hint = "200"
                            Username = cpCore.db.db_GetCSText(CS, "Username")
                            usernameOK = True
                            If Not allowEmailLogin Then
                                'hint = "210"
                                If Username <> Trim(Username) Then
                                    'hint = "220"
                                    Username = Trim(Username)
                                    updateUser = True
                                End If
                                If Username = "" Then
                                    'hint = "230"
                                    'username = emailName & Int(Rnd() * 9999)
                                    usernameOK = False
                                    Ptr = 0
                                    Do While Not usernameOK And (Ptr < 100)
                                        'hint = "240"
                                        Username = EMailName & Int(Rnd() * 9999)
                                        usernameOK = Not cpCore.main_IsLoginOK(Username, "test")
                                        Ptr = Ptr + 1
                                    Loop
                                    'hint = "250"
                                    If usernameOK Then
                                        updateUser = True
                                    End If
                                End If
                                'hint = "260"
                                Message = Message & " username: " & Username & vbCrLf
                            End If
                            'hint = "270"
                            If usernameOK Then
                                '
                                ' password
                                '
                                'hint = "280"
                                Password = cpCore.db.db_GetCSText(CS, "Password")
                                If Trim(Password) <> Password Then
                                    'hint = "290"
                                    Password = Trim(Password)
                                    updateUser = True
                                End If
                                'hint = "300"
                                If Password = "" Then
                                    'hint = "310"
                                    For Ptr = 0 To 8
                                        'hint = "320"
                                        Index = CInt(Rnd() * passwordChrsLength)
                                        Password = Password & Mid(passwordChrs, Index, 1)
                                    Next
                                    'hint = "330"
                                    updateUser = True
                                End If
                                'hint = "340"
                                Message = Message & " password: " & Password & vbCrLf
                                security_SendMemberPassword = True
                                If updateUser Then
                                    'hint = "350"
                                    Call cpCore.db.db_setCS(CS, "username", Username)
                                    Call cpCore.db.db_setCS(CS, "password", Password)
                                End If
                                recordCnt = recordCnt + 1
                            End If
                            cpCore.db.db_csGoNext(CS)
                        Loop
                    End If
                End If
            End If
            'hint = "360"
            If security_SendMemberPassword Then
                Call cpCore.main_SendEmail(workingEmail, FromAddress, subject, Message, , True, False)
                '    main_ClosePageHTML = main_ClosePageHTML & main_GetPopupMessage(app.publicFiles.ReadFile("ccLib\Popup\PasswordSent.htm"), 300, 300, "no")
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function

        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '========================================================================
        ' main_IsContentManager2
        '   If ContentName is missing, returns true if this is an authenticated member with
        '       content management over anything
        '   If ContentName is given, it only tests this content
        '========================================================================
        '
        Public Function main_IsContentManager(Optional ByVal ContentName As String = "") As Boolean
            Dim returnIsContentManager = False
            Try
                Dim SQL As String
                Dim CS As Integer
                Dim ContentID As Integer
                Dim CDef As coreMetaDataClass.CDefClass
                Dim notImplemented_allowAdd As Boolean
                Dim notImplemented_allowDelete As Boolean
                '
                returnIsContentManager = False
                If ContentName = "" Then
                    If user_isAuthenticated() Then
                        If user_isAdmin() Then
                            returnIsContentManager = True
                        Else
                            '
                            ' Is a CM for any content def
                            '
                            If Not cpCore.property_user_isContentManagerOfAnything_isLoaded Then
                                SQL = "SELECT ccGroupRules.ContentID" _
                                    & " FROM ccGroupRules RIGHT JOIN ccMemberRules ON ccGroupRules.GroupID = ccMemberRules.GroupID" _
                                    & " WHERE (" _
                                        & "(ccMemberRules.MemberID=" & cpCore.db.db_EncodeSQLNumber(userId) & ")" _
                                        & " AND(ccMemberRules.active<>0)" _
                                        & " AND(ccGroupRules.active<>0)" _
                                        & " AND(ccGroupRules.ContentID Is not Null)" _
                                        & " AND((ccMemberRules.DateExpires is null)OR(ccMemberRules.DateExpires>" & cpCore.db.db_EncodeSQLDate(cpCore.main_PageStartTime) & "))" _
                                        & ");"
                                CS = cpCore.db.db_csOpenSql(SQL)
                                cpCore.property_user_isContentManagerOfAnything = cpCore.db.db_csOk(CS)
                                cpCore.db.db_csClose(CS)
                                '
                                cpCore.property_user_isContentManagerOfAnything_isLoaded = True
                            End If
                            returnIsContentManager = cpCore.property_user_isContentManagerOfAnything
                        End If
                    End If
                Else
                    '
                    ' Specific Content called out
                    '
                    Call cpCore.main_GetContentAccessRights(ContentName, returnIsContentManager, notImplemented_allowAdd, notImplemented_allowDelete)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnIsContentManager
        End Function
        '
        '========================================================================
        ' Member Login (by username and password)
        '
        '   See main_GetLoginMemberID and main_LoginMemberByID
        '========================================================================
        '
        Public Function user_LoginMember(ByVal loginFieldValue As String, ByVal Password As String, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("LoginMember")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iLoginFieldValue As String
            Dim iPassword As String
            Dim LocalMemberID As Integer
            '
            iLoginFieldValue = EncodeText(loginFieldValue)
            iPassword = EncodeText(Password)
            '
            MethodName = "LoginMember"
            '
            user_LoginMember = False
            LocalMemberID = cpCore.main_GetLoginMemberID(iLoginFieldValue, iPassword)
            If LocalMemberID <> 0 Then
                user_LoginMember = user_LoginMemberByID(LocalMemberID, AllowAutoLogin)
                If user_LoginMember Then
                    Call cpCore.main_LogActivity2("successful password login", userId, userOrganizationId)
                    cpCore.property_user_isContentManagerOfAnything_isLoaded = False
                    property_user_isAdmin_isLoaded = False
                    property_user_isMember_isLoaded = False
                    property_user_isAuthenticated_isLoaded = False
                Else
                    Call cpCore.main_LogActivity2("unsuccessful login (loginField:" & iLoginFieldValue & "/password:" & iPassword & ")", userId, userOrganizationId)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        '   Member Login By ID
        '
        '========================================================================
        '
        Public Function user_LoginMemberByID(ByVal RecordID As Integer, Optional ByVal AllowAutoLogin As Boolean = False) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("LoginMemberByID")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim iRecordID As Integer
            '
            MethodName = "main_LoginMemberByID"
            '
            iRecordID = EncodeInteger(RecordID)
            user_LoginMemberByID = user_RecognizeMemberByID(iRecordID)
            If user_LoginMemberByID Then
                '
                ' Log them in
                '
                cpCore.visit_isAuthenticated = True
                Call cpCore.main_SaveVisit()
                property_user_isAdmin_isLoaded = False
                property_user_isMember_isLoaded = False
                property_user_isAuthenticated_isLoaded = False
                property_user_isDeveloper_isLoaded = False
                '
                ' Write Cookies in case Visit Tracking is off
                '
                If cpCore.main_VisitStartTime = Date.MinValue Then
                    cpCore.main_VisitStartTime = cpCore.main_PageStartTime
                End If
                If Not EncodeBoolean(cpCore.siteProperties.getBoolean("allowVisitTracking", True)) Then
                    Call cpCore.web_init_initVisit(True)
                End If
                '
                ' Change autologin if included, selected, and allowed
                '
                If Not isMissing(AllowAutoLogin) Then
                    If AllowAutoLogin Xor user_autoLogin Then
                        If EncodeBoolean(cpCore.siteProperties.getBoolean("AllowAutoLogin", False)) Then
                            CS = cpCore.db_csOpenRecord("people", iRecordID)
                            If cpCore.db.db_csOk(CS) Then
                                Call cpCore.db.db_setCS(CS, "AutoLogin", AllowAutoLogin)
                            End If
                            Call cpCore.db.db_csClose(CS)
                        End If
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        '   main_SetMemberIdentity
        '
        '   See RecognizeMember for details
        '========================================================================
        '
        Public Function user_SetMemberIdentity(Optional ByVal Criteria As String = "") As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetMemberIdentity")
            '
            'If Not (true) Then Exit Function
            '
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim CS As Integer
            '
            MethodName = "main_SetMemberIdentity"
            '
            ' ----- Member is attempting a login
            '
            user_SetMemberIdentity = False
            CS = cpCore.db.db_csOpen("people", Criteria, , , , , , "ID")
            If cpCore.db.db_csOk(CS) Then
                user_SetMemberIdentity = user_RecognizeMemberByID(cpCore.db.db_GetCSInteger(CS, "ID"))
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        '   RecognizeMember
        '
        '   the current member to be non-authenticated, but recognized
        '========================================================================
        '
        Public Function user_RecognizeMemberByID(ByVal RecordID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("RecognizeMemberByID")
            '
            'If Not (true) Then Exit Function
            Dim CSPointer As Integer
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim SQL As String
            '
            MethodName = "main_RecognizeMemberByID"
            '
            user_RecognizeMemberByID = False
            SQL = "select" _
                & " ccMembers.*" _
                & " ,ccLanguages.name as LanguageName" _
                & " from" _
                & " ccMembers" _
                & " left join ccLanguages on ccMembers.LanguageID=ccLanguages.ID" _
                & " where" _
                & " (ccMembers.active<>" & SQLFalse & ")" _
                & " and(ccMembers.ID=" & RecordID & ")"
            If True Then
                SQL &= "" _
                    & " and((ccMembers.dateExpires is null)or(ccMembers.dateExpires>" & cpCore.db.db_EncodeSQLDate(Now) & "))" _
                    & ""
            End If
            CS = cpCore.db.db_csOpenSql(SQL)
            If cpCore.db.db_csOk(CS) Then
                If cpCore.main_VisitId = 0 Then
                    '
                    ' Visit was blocked during init, init the visit now
                    '
                    Call cpCore.web_init_initVisit(True)
                End If
                '
                ' ----- Member was recognized
                '
                userId = (cpCore.db.db_GetCSInteger(CS, "ID"))
                userName = (cpCore.db.db_GetCSText(CS, "Name"))
                userUsername = (cpCore.db.db_GetCSText(CS, "username"))
                userEmail = (cpCore.db.db_GetCSText(CS, "Email"))
                userPassword = (cpCore.db.db_GetCSText(CS, "Password"))
                userOrganizationId = (cpCore.db.db_GetCSInteger(CS, "OrganizationID"))
                userLanguageId = (cpCore.db.db_GetCSInteger(CS, "LanguageID"))
                user_Active = (cpCore.db.db_GetCSBoolean(CS, "Active"))
                user_company = (cpCore.db.db_GetCSText(CS, "Company"))
                user_visits = (cpCore.db.db_GetCSInteger(CS, "Visits"))
                user_lastVisit = (cpCore.db.db_GetCSDate(CS, "LastVisit"))
                user_allowBulkEmail = (cpCore.db.db_GetCSBoolean(CS, "AllowBulkEmail"))
                userAllowToolsPanel = (cpCore.db.db_GetCSBoolean(CS, "AllowToolsPanel"))
                user_adminMenuModeID = (cpCore.db.db_GetCSInteger(CS, "AdminMenuModeID"))
                user_autoLogin = (cpCore.db.db_GetCSBoolean(CS, "AutoLogin"))
                ' 6/18/2009 notes was removed from base
                '        MemberSendNotes = (app.csv_GetCSBoolean(CS, "SendNotes"))
                userIsDeveloper = (cpCore.db.db_GetCSBoolean(CS, "Developer"))
                userIsAdmin = (cpCore.db.db_GetCSBoolean(CS, "Admin"))
                userContentControlID = (cpCore.db.db_GetCSInteger(CS, "ContentControlID"))
                userLanguageId = (cpCore.db.db_GetCSInteger(CS, "LanguageID"))
                userLanguage = (cpCore.db.db_GetCSText(CS, "LanguageName"))
                If True Then
                    userStyleFilename = cpCore.db.db_GetCSText(CS, "StyleFilename")
                    If userStyleFilename <> "" Then
                        Call cpCore.main_AddStylesheetLink(cpCore.web_requestProtocol & cpCore.web.requestDomain & cpCore.csv_getVirtualFileLink(cpCore.appConfig.cdnFilesNetprefix, userStyleFilename))
                    End If
                End If
                If True Then
                    userExcludeFromAnalytics = cpCore.db.db_GetCSBoolean(CS, "ExcludeFromAnalytics")
                End If
                '
                user_visits = user_visits + 1
                If user_visits = 1 Then
                    userIsNew = True
                Else
                    userIsNew = False
                End If
                user_lastVisit = cpCore.main_PageStartTime
                cpCore.main_VisitMemberID = userId
                cpCore.main_VisitLoginAttempts = 0
                cpCore.main_VisitorMemberID = userId
                cpCore.main_VisitExcludeFromAnalytics = cpCore.main_VisitExcludeFromAnalytics Or cpCore.main_VisitIsBot Or userExcludeFromAnalytics Or userIsAdmin Or userIsDeveloper
                Call cpCore.main_SaveVisit()
                Call cpCore.main_SaveVisitor()
                Call user_SaveMemberBase()
                user_RecognizeMemberByID = True
            End If
            Call cpCore.db.db_csClose(CS)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Function
        '
        '========================================================================
        ' ----- Create a new default user and save it
        '       If failure, MemberID is 0
        '       If successful, main_VisitMemberID and main_VisitorMemberID must be set to MemberID
        '========================================================================
        '
        Public Sub user_CreateUser()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("CreateUser")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CSMember As Integer
            Dim CSlanguage As Integer
            Dim MethodName As String
            Dim DefaultLanguage As String
            Dim DefaultVisitName As String
            Dim ignore0 As Boolean
            Dim Ignore1 As Boolean
            '
            MethodName = "main_CreateUser"
            '
            Call user_CreateUserDefaults(cpCore.main_VisitName)
            '
            userId = 0
            CSMember = cpCore.db.db_csInsertRecord("people")
            If Not cpCore.db.db_csOk(CSMember) Then
                Call cpCore.handleExceptionAndRethrow(New ApplicationException("main_CreateUser, Error inserting new people record, could not main_CreateUser"))
            Else
                userId = cpCore.db.db_GetCSInteger(CSMember, "id")
                If DefaultVisitName <> "" Then
                    userName = DefaultVisitName
                    Call cpCore.db.db_setCS(CSMember, "name", userName)
                End If
                '
                ' Created By Visit
                '
                If True Then
                    Call cpCore.db.db_setCS(CSMember, "CreatedByVisit", True)
                End If
                '
                user_Active = True
                Call cpCore.db.db_setCS(CSMember, "active", user_Active)
                '
                user_visits = 1
                Call cpCore.db.db_setCS(CSMember, "Visits", user_visits)
                '
                user_lastVisit = cpCore.main_PageStartTime
                Call cpCore.db.db_setCS(CSMember, "LastVisit", user_lastVisit)
                '
                '
                CSlanguage = cpCore.db_csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID, , , "Name")
                If cpCore.db.db_csOk(CSlanguage) Then
                    userLanguageId = cpCore.db.db_GetCSInteger(CSlanguage, "ID")
                    userLanguage = cpCore.db.db_GetCSText(CSlanguage, "Name")
                    Call cpCore.db.db_setCS(CSMember, "LanguageID", userLanguageId)
                End If
                Call cpCore.db.db_csClose(CSlanguage)
                '
                userAdded = True
                userIsNew = True
                userStyleFilename = ""
                userExcludeFromAnalytics = False
                '
                Call cpCore.db.db_csClose(CSMember)
                '
                cpCore.main_VisitMemberID = userId
                cpCore.main_VisitorMemberID = userId
                cpCore.visit_isAuthenticated = False
                Call cpCore.main_SaveVisit()
                Call cpCore.main_SaveVisitor()
                '
                property_user_isAdmin_isLoaded = False
                property_user_isMember_isLoaded = False
                property_user_isAuthenticated_isLoaded = False
                property_user_isDeveloper_isLoaded = False
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("createUser"))
        End Sub
        '
        '========================================================================
        '   Creates the internal records for the user, but does not create
        '   a people record to save them
        '========================================================================
        '
        Friend Sub user_CreateUserDefaults(ByVal DefaultName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("CreateUserDefaults")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CSMember As Integer
            Dim CSlanguage As Integer
            Dim MethodName As String
            Dim DefaultLanguage As String
            Dim DefaultMemberName As String
            '
            MethodName = "main_CreateUserDefaults"
            '
            userId = 0
            userId = 0
            userName = DefaultName
            userIsAdmin = False
            userIsDeveloper = False
            userOrganizationId = 0
            userLanguageId = 0
            userLanguage = ""
            userIsNew = False
            userEmail = ""
            user_allowBulkEmail = False
            userAllowToolsPanel = False
            user_autoLogin = False
            user_adminMenuModeID = 0
            loginForm_Username = ""
            loginForm_Password = ""
            loginForm_Email = ""
            loginForm_AutoLogin = False
            userAdded = False
            userUsername = ""
            userPassword = ""
            userContentControlID = 0
            user_Active = False
            user_visits = 0
            user_lastVisit = cpCore.main_VisitStartTime
            user_company = ""
            user_Title = ""
            main_MemberAddress = ""
            main_MemberCity = ""
            main_MemberState = ""
            main_MemberZip = ""
            main_MemberCountry = ""
            main_MemberPhone = ""
            main_MemberFax = ""
            '
            user_Active = True
            '
            user_visits = 1
            '
            user_lastVisit = cpCore.main_PageStartTime
            '
            '
            CSlanguage = cpCore.db_csOpenRecord("Languages", cpCore.web_GetBrowserLanguageID, , , "Name")
            If cpCore.db.db_csOk(CSlanguage) Then
                userLanguageId = cpCore.db.db_GetCSInteger(CSlanguage, "ID")
                userLanguage = cpCore.db.db_GetCSText(CSlanguage, "Name")
            End If
            Call cpCore.db.db_csClose(CSlanguage)
            '
            userAdded = True
            userIsNew = True
            userStyleFilename = ""
            userExcludeFromAnalytics = False
            '
            property_user_isAdmin_isLoaded = False
            property_user_isMember_isLoaded = False
            property_user_isAuthenticated_isLoaded = False
            property_user_isDeveloper_isLoaded = False
            'End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("exception"))
        End Sub
        '
        '=============================================================================
        '   main_SaveMemberBase()
        '       Saves the current Member record to the database
        '=============================================================================
        '
        Public Sub user_SaveMemberBase()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SaveMemberBase")
            '
            'If Not (true) Then Exit Sub
            '
            Dim SQL As String
            Dim UsernameName As String
            Dim MethodName As String
            '
            MethodName = "main_SaveMemberBase"
            '
            If cpCore.visit_initialized Then
                If (userId > 0) Then
                    SQL = "UPDATE ccMembers SET " _
                        & " Name=" & cpCore.db.encodeSQLText(userName) _
                        & ",username=" & cpCore.db.encodeSQLText(userUsername) _
                        & ",email=" & cpCore.db.encodeSQLText(userEmail) _
                        & ",password=" & cpCore.db.encodeSQLText(userPassword) _
                        & ",OrganizationID=" & cpCore.db.db_EncodeSQLNumber(userOrganizationId) _
                        & ",LanguageID=" & cpCore.db.db_EncodeSQLNumber(userLanguageId) _
                        & ",Active=" & cpCore.db.db_EncodeSQLBoolean(user_Active) _
                        & ",Company=" & cpCore.db.encodeSQLText(user_company) _
                        & ",Visits=" & cpCore.db.db_EncodeSQLNumber(user_visits) _
                        & ",LastVisit=" & cpCore.db.db_EncodeSQLDate(user_lastVisit) _
                        & ",AllowBulkEmail=" & cpCore.db.db_EncodeSQLBoolean(user_allowBulkEmail) _
                        & ",AllowToolsPanel=" & cpCore.db.db_EncodeSQLBoolean(userAllowToolsPanel) _
                        & ",AdminMenuModeID=" & cpCore.db.db_EncodeSQLNumber(user_adminMenuModeID) _
                        & ",AutoLogin=" & cpCore.db.db_EncodeSQLBoolean(user_autoLogin)

                    ' 6/18/2009 notes was removed from base
                    '            & ",SendNotes=" & encodeSQLBoolean(MemberSendNotes)
                    If True Then
                        SQL &= ",ExcludeFromAnalytics=" & cpCore.db.db_EncodeSQLBoolean(userExcludeFromAnalytics)
                    End If
                    SQL &= " WHERE ID=" & userId & ";"
                    Call cpCore.db.executeSql(SQL)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
            '
        End Sub
        '
        '========================================================================
        ' Member Logout
        '   Create and assign a guest Member identity
        '========================================================================
        '
        Public Sub security_LogoutMember()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("LogoutMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            '
            Call cpCore.main_LogActivity2("logout", userId, userOrganizationId)
            '
            ' Clear MemberID for this page
            '
            Call user_CreateUser()
            '
            ' Clear cached permissions
            '
            property_user_isAdmin_isLoaded = False              ' true if main_IsAdminCache is initialized
            property_user_isMember_isLoaded = False
            property_user_isAuthenticated_isLoaded = False
            property_user_isDeveloper_isLoaded = False
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & "main_LogoutMember"))
        End Sub
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Sub user_ProcessFormJoin()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessFormJoin")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim ErrorMessage As String
            Dim CS As Integer
            Dim ChangeMade As Boolean
            Dim FirstName As String
            Dim LastName As String
            Dim FullName As String
            Dim Email As String
            '
            MethodName = "main_ProcessFormJoin()"
            '
            ' ----- add a username and password for this guest
            '
            loginForm_Username = cpCore.doc_getText("username")
            loginForm_Password = cpCore.doc_getText("password")
            '
            If Not EncodeBoolean(cpCore.siteProperties.getBoolean("AllowMemberJoin", False)) Then
                cpCore.error_AddUserError("This site does not accept public main_MemberShip.")
            Else
                If Not cpCore.main_IsNewLoginOK(loginForm_Username, loginForm_Password, ErrorMessage) Then
                    Call cpCore.error_AddUserError(ErrorMessage)
                Else
                    If Not cpCore.error_IsUserError() Then
                        CS = cpCore.db.db_csOpen("people", "ID=" & cpCore.db.db_EncodeSQLNumber(cpCore.user.userId))
                        If Not cpCore.db.db_csOk(CS) Then
                            cpCore.handleExceptionAndRethrow(New Exception("Could not open the current members account to set the username and password."))
                        Else
                            If (cpCore.db.db_GetCSText(CS, "username") <> "") Or (cpCore.db.db_GetCSText(CS, "password") <> "") Or (cpCore.db.db_GetCSBoolean(CS, "admin")) Or (cpCore.db.db_GetCSBoolean(CS, "developer")) Then
                                '
                                ' if the current account can be logged into, you can not join 'into' it
                                '
                                Call security_LogoutMember()
                            End If
                            FirstName = cpCore.doc_getText("firstname")
                            LastName = cpCore.doc_getText("firstname")
                            FullName = FirstName & " " & LastName
                            Email = cpCore.doc_getText("email")
                            Call cpCore.db.db_setCS(CS, "FirstName", FirstName)
                            Call cpCore.db.db_setCS(CS, "LastName", LastName)
                            Call cpCore.db.db_setCS(CS, "Name", FullName)
                            Call cpCore.db.db_setCS(CS, "username", loginForm_Username)
                            Call cpCore.db.db_setCS(CS, "password", loginForm_Password)
                            Call cpCore.user.user_LoginMemberByID(cpCore.user.userId)
                        End If
                        Call cpCore.db.db_csClose(CS)
                    End If
                End If
            End If
            Call cpCore.cache.invalidateTagCommaList("People")
            '
            Exit Sub
ErrorTrap:
            Call cpCore.handleExceptionAndRethrow(New ApplicationException("Exception in " & MethodName))
        End Sub
    End Class
End Namespace