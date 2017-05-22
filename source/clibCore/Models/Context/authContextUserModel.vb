
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Models.Context
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' </summary>
    Public Class authContextUserModel
        '
        'Private cpCore As coreClass
        ''
        ''====================================================================================================
        '''' <summary>
        '''' id for the authcontext.user. When this property is set, all public authcontext.user. properteries are updated for this selected id
        '''' </summary>
        '''' <returns></returns>
        'Public Property id As Integer
        '    Get
        '        Return _id
        '    End Get
        '    Set(value As Integer)
        '        If (_id <> value) Then
        '            _id = initializeUser(value)
        '        End If
        '    End Set
        'End Property
        'Private _id As Integer = 0
        '
        ' simple shared properties, derived from the userId when .id set (through initilizeUser method)
        '
        Public id As Integer = 0
        Friend name As String = ""                 '
        Friend isAdmin As Boolean = False              '
        Friend isDeveloper As Boolean = False          '
        Friend organizationId As Integer = 0         ' The members Organization
        Friend languageId As Integer = 0             '
        Friend language As String = ""             '
        Friend isNew As Boolean = False                ' stored in visit record - Is this the first visit for this member
        Friend email As String = ""               '
        Friend allowBulkEmail As Boolean = False      ' Allow bulk mail
        Friend allowToolsPanel As Boolean = False    '
        Friend autoLogin As Boolean = False         ' if true, and setup AllowAutoLogin then use cookie to login
        Friend adminMenuModeID As Integer = 0     '
        Friend userAdded As Boolean = False              ' depricated - true only during the page that the join was completed - use for redirections and GroupAdds
        Friend username As String = ""
        Friend password As String = ""
        Friend contentControlID As Integer = 0
        Friend styleFilename As String = ""          ' if not empty, add to head
        Friend excludeFromAnalytics As Boolean = False   ' if true, future visits will be marked exclude from analytics
        Public main_IsEditingContentList As String = ""
        Public main_IsNotEditingContentList As String = ""
        Public active As Boolean = False           '
        Public visits As Integer = 0                '
        Public lastVisit As Date = Date.MinValue             ' The last visit by the Member (the beginning of this visit
        Public company As String = ""
        Public user_Title As String = ""
        Public main_MemberAddress As String = ""
        Public main_MemberCity As String = ""
        Public main_MemberState As String = ""
        Public main_MemberZip As String = ""
        Public main_MemberCountry As String = ""
        Public main_MemberPhone As String = ""
        Public main_MemberFax As String = ""
        Public billEmail As String = ""          ' Billing Address for purchases
        Public billPhone As String = ""          '
        Public billFax As String = ""            '
        Public billCompany As String = ""        '
        Public billAddress As String = ""        '
        Public billCity As String = ""           '
        Public billState As String = ""         '
        Public billZip As String = ""            '
        Public billCountry As String = ""       '
        Public shipName As String = ""          ' Mailing Address
        Public shipCompany As String = ""           '
        Public shipAddress As String = ""          '
        Public shipCity As String = ""          '
        Public shipState As String = ""        '
        Public shipZip As String = ""           '
        Public shipCountry As String = ""         '
        Public shipPhone As String = ""        '
        Public Const main_maxVisitLoginAttempts As Integer = 20
        Public main_loginFormDefaultProcessed As Boolean = False       ' prevent main_ProcessLoginFormDefault from running twice (multiple user messages, popups, etc.)
        Public createdByVisit As Boolean

        Public Sub New()
        End Sub
        '
        '=============================================================================
        '   main_SaveMember()
        '       Saves the member properties that are loaded during main_OpenMember
        '=============================================================================
        '
        Public Sub saveObject(cpCore As coreClass)
            Try
                Dim SQL As String
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
                        & ",ShipPhone=" & cpCore.db.encodeSQLText(shipPhone) _
                        & ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(excludeFromAnalytics) _
                        & ",createdByVisit=" & createdByVisit.ToString() _
                        & " WHERE ID=" & id & ";"
                    Call cpCore.db.executeSql(SQL)
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        ''
        ''========================================================================
        '' Member Open
        ''   Attempts to open the Member record based on the iRecordID
        ''   If successful, MemberID is set to the iRecordID
        ''========================================================================
        ''
        'Public Function loadObject(cpcore As coreClass, recordId As Integer) As Integer
        '    Dim returnRecordId As Integer = 0
        '    Try
        '        Dim CS As Integer
        '        '
        '        If recordId <> 0 Then
        '            '
        '            ' attempt to read in Member record if logged on
        '            ' dont just do main_CheckMember() -- in case a pretty login is needed
        '            '
        '            CS = cpcore.csOpenRecord("People", recordId)
        '            If cpcore.db.cs_ok(CS) Then
        '                name = cpcore.db.cs_getText(CS, "Name")
        '                isDeveloper = cpcore.db.cs_getBoolean(CS, "Developer")
        '                isAdmin = cpcore.db.cs_getBoolean(CS, "Admin")
        '                contentControlID = cpcore.db.cs_getInteger(CS, "ContentControlID")
        '                organizationId = cpcore.db.cs_getInteger(CS, "OrganizationID")
        '                languageId = cpcore.db.cs_getInteger(CS, "LanguageID")
        '                language = cpcore.main_cs_getEncodedField(CS, "LanguageID")
        '                '
        '                shipName = cpcore.db.cs_getText(CS, "ShipName")
        '                shipCompany = cpcore.db.cs_getText(CS, "ShipCompany")
        '                shipAddress = cpcore.db.cs_getText(CS, "ShipAddress")
        '                shipCity = cpcore.db.cs_getText(CS, "ShipCity")
        '                shipState = cpcore.db.cs_getText(CS, "ShipState")
        '                shipZip = cpcore.db.cs_getText(CS, "ShipZip")
        '                shipCountry = cpcore.db.cs_getText(CS, "ShipCountry")
        '                shipPhone = cpcore.db.cs_getText(CS, "ShipPhone")
        '                '
        '                billCompany = cpcore.db.cs_getText(CS, "BillCompany")
        '                billAddress = cpcore.db.cs_getText(CS, "BillAddress")
        '                billCity = cpcore.db.cs_getText(CS, "BillCity")
        '                billState = cpcore.db.cs_getText(CS, "BillState")
        '                billZip = cpcore.db.cs_getText(CS, "BillZip")
        '                billCountry = cpcore.db.cs_getText(CS, "BillCountry")
        '                billEmail = cpcore.db.cs_getText(CS, "BillEmail")
        '                billPhone = cpcore.db.cs_getText(CS, "BillPhone")
        '                billFax = cpcore.db.cs_getText(CS, "BillFax")
        '                '
        '                allowBulkEmail = cpcore.db.cs_getBoolean(CS, "AllowBulkEmail")
        '                allowToolsPanel = cpcore.db.cs_getBoolean(CS, "AllowToolsPanel")
        '                adminMenuModeID = cpcore.db.cs_getInteger(CS, "AdminMenuModeID")
        '                autoLogin = cpcore.db.cs_getBoolean(CS, "AutoLogin")
        '                '
        '                styleFilename = cpcore.db.cs_getText(CS, "StyleFilename")
        '                If styleFilename <> "" Then
        '                    Call cpcore.htmlDoc.main_AddStylesheetLink(cpcore.webServer.webServerIO_requestProtocol & cpcore.webServer.requestDomain & cpcore.csv_getVirtualFileLink(cpcore.serverConfig.appConfig.cdnFilesNetprefix, styleFilename))
        '                End If
        '                excludeFromAnalytics = cpcore.db.cs_getBoolean(CS, "ExcludeFromAnalytics")
        '                returnRecordId = recordId
        '            End If
        '            Call cpcore.db.cs_Close(CS)
        '        End If
        '    Catch ex As Exception
        '        cpcore.handleExceptionAndRethrow(ex)
        '    End Try
        '    Return returnRecordId
        'End Function
        '
        '========================================================================
        ' ----- Create a new default user and save it
        '       If failure, MemberID is 0
        '       If successful, main_VisitMemberID and main_VisitorMemberID must be set to MemberID
        '========================================================================
        '
        Public Shared Function createDefault(cpcore As coreClass, defaultName As String) As authContextUserModel
            Dim resultUser As authContextUserModel = createUserDefaults(cpcore, defaultName)
            Try
                'Dim CSMember As Integer
                'Dim CSlanguage As Integer
                '
                With resultUser
                    .active = True
                    .visits = 1
                    .userAdded = True
                    .isNew = True
                    .styleFilename = ""
                    .excludeFromAnalytics = False
                    .lastVisit = cpcore.app_startTime
                    .createdByVisit = True
                    .saveObject(cpcore)
                    'CSMember = cpcore.db.cs_insertRecord("people")
                    'If Not cpcore.db.cs_ok(CSMember) Then
                    '    Call cpcore.handleExceptionAndRethrow(New ApplicationException("main_CreateUser, Error inserting new people record, could not main_CreateUser"))
                    'Else
                    '    .id = cpcore.db.cs_getInteger(CSMember, "id")
                    '    Call cpcore.db.cs_set(CSMember, "CreatedByVisit", True)
                    '    Call cpcore.db.cs_set(CSMember, "active", .active)
                    '    Call cpcore.db.cs_set(CSMember, "Visits", .visits)
                    '    Call cpcore.db.cs_set(CSMember, "LastVisit", .lastVisit)
                    '    CSlanguage = cpcore.csOpenRecord("Languages", cpcore.web_GetBrowserLanguageID(), SelectFieldList:="Name")
                    '    If cpcore.db.cs_ok(CSlanguage) Then
                    '        .languageId = cpcore.db.cs_getInteger(CSlanguage, "ID")
                    '        .language = cpcore.db.cs_getText(CSlanguage, "Name")
                    '        Call cpcore.db.cs_set(CSMember, "LanguageID", .languageId)
                    '    End If
                    '    Call cpcore.db.cs_Close(CSlanguage)
                    'End If
                    'Call cpcore.db.cs_Close(CSMember)
                End With
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return resultUser
        End Function
        '
        '========================================================================
        '   Creates the internal records for the user, but does not create
        '   a people record to save them
        '========================================================================
        '
        Public Shared Function createUserDefaults(cpcore As coreClass, ByVal DefaultName As String) As authContextUserModel
            Dim resultUser As New authContextUserModel

            Try
                With resultUser
                    .id = 0
                    .name = DefaultName
                    .isAdmin = False
                    .isDeveloper = False
                    .organizationId = 0
                    .languageId = 0
                    .language = ""
                    .isNew = False
                    .email = ""
                    .allowBulkEmail = False
                    .allowToolsPanel = False
                    .autoLogin = False
                    .adminMenuModeID = 0
                    .userAdded = False
                    .username = ""
                    .password = ""
                    .contentControlID = 0
                    .active = True
                    .visits = 1
                    .lastVisit = cpcore.app_startTime
                    .company = ""
                    .user_Title = ""
                    .main_MemberAddress = ""
                    .main_MemberCity = ""
                    .main_MemberState = ""
                    .main_MemberZip = ""
                    .main_MemberCountry = ""
                    .main_MemberPhone = ""
                    .main_MemberFax = ""
                    .userAdded = True
                    .isNew = True
                    .styleFilename = ""
                    .excludeFromAnalytics = False
                End With
            Catch ex As Exception
                cpcore.handleExceptionAndRethrow(ex)
            End Try
            Return resultUser
        End Function
        ''
        ''=============================================================================
        ''   main_SaveMemberBase()
        ''       Saves the current Member record to the database
        ''=============================================================================
        ''
        'Public Sub saveMemberBase()
        '    Try
        '        If (id > 0) Then
        '            Dim SQL As String = "UPDATE ccMembers SET " _
        '                & " Name=" & cpCore.db.encodeSQLText(name) _
        '                & ",username=" & cpCore.db.encodeSQLText(username) _
        '                & ",email=" & cpCore.db.encodeSQLText(email) _
        '                & ",password=" & cpCore.db.encodeSQLText(password) _
        '                & ",OrganizationID=" & cpCore.db.encodeSQLNumber(organizationId) _
        '                & ",LanguageID=" & cpCore.db.encodeSQLNumber(languageId) _
        '                & ",Active=" & cpCore.db.encodeSQLBoolean(active) _
        '                & ",Company=" & cpCore.db.encodeSQLText(company) _
        '                & ",Visits=" & cpCore.db.encodeSQLNumber(visits) _
        '                & ",LastVisit=" & cpCore.db.encodeSQLDate(lastVisit) _
        '                & ",AllowBulkEmail=" & cpCore.db.encodeSQLBoolean(allowBulkEmail) _
        '                & ",AllowToolsPanel=" & cpCore.db.encodeSQLBoolean(allowToolsPanel) _
        '                & ",AdminMenuModeID=" & cpCore.db.encodeSQLNumber(adminMenuModeID) _
        '                & ",AutoLogin=" & cpCore.db.encodeSQLBoolean(autoLogin) _
        '                & ",ExcludeFromAnalytics=" & cpCore.db.encodeSQLBoolean(excludeFromAnalytics) _
        '                & " WHERE ID=" & id & ";"
        '            Call cpCore.db.executeSql(Sql)
        '        End If
        '    Catch ex As Exception
        '        cpCore.handleExceptionAndRethrow(ex)
        '    End Try
        'End Sub
        '
    End Class
End Namespace