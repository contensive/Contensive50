
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' Site Properties
    ''' </summary>
    Public Class sitePropertiesController
        '
        Private cpCore As coreClass
        '
        '====================================================================================================
        ''' <summary>
        ''' new
        ''' </summary>
        ''' <param name="cpCore"></param>
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        '
        Friend ReadOnly Property landingPageID() As Integer
            Get
                If _landingPageID Is Nothing Then
                    _landingPageID = getinteger("LandingPageID", 0)
                End If
                Return CInt(_landingPageID)
            End Get
        End Property
        Private _landingPageID As Integer? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property sitePropertyContentId() As Integer
            Get
                If _sitePropertyContentId Is Nothing Then
                    _sitePropertyContentId = cpCore.metaData.getContentId("site properties")
                End If
                Return CInt(_sitePropertyContentId)
            End Get
        End Property
        Private _sitePropertyContentId As Integer? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property nameValueDict() As Dictionary(Of String, String)
            Get
                If _nameValueDict Is Nothing Then
                    _nameValueDict = getNameValueDict(cpCore)
                End If
                Return _nameValueDict
            End Get
        End Property
        Private _nameValueDict As Dictionary(Of String, String) = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property trackGuestsWithoutCookies() As Boolean
            Get
                If _trackGuestsWithoutCookies Is Nothing Then
                    _trackGuestsWithoutCookies = getBoolean("track guests without cookies", False)
                End If
                Return CBool(_trackGuestsWithoutCookies)
            End Get
        End Property
        Private _trackGuestsWithoutCookies As Boolean?
        '
        '====================================================================================================
        '
        Friend ReadOnly Property AllowAutoLogin() As Boolean
            Get
                If _AllowAutoLogin Is Nothing Then
                    _AllowAutoLogin = getBoolean("allowAutoLogin", False)
                End If
                Return CBool(_AllowAutoLogin)
            End Get
        End Property
        Private _AllowAutoLogin As Boolean?
        '
        '====================================================================================================
        '
        Friend ReadOnly Property maxVisitLoginAttempts() As Integer
            Get
                If _maxVisitLoginAttempts Is Nothing Then
                    _maxVisitLoginAttempts = getinteger("maxVisitLoginAttempts", 20)
                End If
                Return CInt(_maxVisitLoginAttempts)
            End Get
        End Property
        Private _maxVisitLoginAttempts As Integer?
        '====================================================================================================
        '
        Public ReadOnly Property LoginIconFilename() As String
            Get
                If (_LoginIconFilename Is Nothing) Then
                    _LoginIconFilename = getText("LoginIconFilename", "/ccLib/images/ccLibLogin.GIF")
                End If
                Return _LoginIconFilename
            End Get
        End Property
        Private _LoginIconFilename As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowVisitTracking() As Boolean
            Get
                If Not _allowVisitTrackingLoaded Then
                    _allowVisitTracking = genericController.EncodeBoolean(getText("allowVisitTracking", "true"))
                    _allowVisitTrackingLoaded = True
                End If
                Return _allowVisitTracking
            End Get
        End Property
        Private _allowVisitTrackingLoaded As Boolean = False
        Private _allowVisitTracking As Boolean
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowTransactionLog() As Boolean
            Get
                If Not _allowTransactionLog_localLoaded Then
                    _allowTransactionLog_local = genericController.EncodeBoolean(getText("UseContentWatchLink", "false"))
                    _allowTransactionLog_localLoaded = True
                End If
                Return _allowTransactionLog_local
            End Get
        End Property
        Private _allowTransactionLog_localLoaded As Boolean = False
        Private _allowTransactionLog_local As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' trap errors (hide errors) - when true, errors will be logged and code resumes next. When false, errors are re-thrown
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property trapErrors() As Boolean
            Get
                If Not _trapErrorsLoaded Then
                    _trapErrors = getBoolean("TrapErrors", True)
                    _trapErrorsLoaded = True
                End If
                Return _trapErrors
            End Get
        End Property
        Private _trapErrors As Boolean
        Private _trapErrorsLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property serverPageDefault() As String
            Get
                If Not _ServerPageDefault_localLoaded Then
                    _ServerPageDefault_local = getText(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue)
                    _ServerPageDefault_localLoaded = True
                End If
                Return _ServerPageDefault_local
            End Get
        End Property
        Private _ServerPageDefault_local As String
        Private _ServerPageDefault_localLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowChildMenuHeadline() As Boolean
            Get
                Return False
            End Get
        End Property
        '
        '====================================================================================================
        '
        Friend ReadOnly Property defaultWrapperID() As Integer
            Get
                If Not _defaultWrapperID_LocalLoaded Then
                    _defaultWrapperID_LocalLoaded = True
                    _defaultWrapperID_local = getinteger("DefaultWrapperID", 0)
                End If
                Return _defaultWrapperID_local
            End Get
        End Property
        Private _defaultWrapperID_local As Integer
        Private _defaultWrapperID_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' allowLinkAlias
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property allowLinkAlias() As Boolean
            Get
                If Not _allowLinkAlias_LocalLoaded Then
                    _allowLinkAlias_Local = getBoolean("allowLinkAlias", True)
                    _allowLinkAlias_LocalLoaded = True
                End If
                Return _allowLinkAlias_Local
            End Get
        End Property
        Private _allowLinkAlias_Local As Boolean
        Private _allowLinkAlias_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Friend ReadOnly Property childListAddonID() As Integer
            Get
                Try
                    Dim CS As Integer
                    Dim BuildSupportsGuid As Boolean
                    '
                    If Not _childListAddonID_LocalLoaded Then
                        _childListAddonID_LocalLoaded = True
                        _childListAddonID_Local = getinteger("ChildListAddonID", 0)
                        If _childListAddonID_Local = 0 Then
                            BuildSupportsGuid = True
                            If BuildSupportsGuid Then
                                CS = cpCore.db.cs_open(cnAddons, "ccguid='" & addonGuidChildList & "'", , , ,,  , "ID")
                            Else
                                CS = cpCore.db.cs_open(cnAddons, "name='Child Page List'", , , , ,, "ID")
                            End If
                            If cpCore.db.cs_ok(CS) Then
                                _childListAddonID_Local = cpCore.db.cs_getInteger(CS, "ID")
                            End If
                            Call cpCore.db.cs_Close(CS)
                            If _childListAddonID_Local = 0 Then
                                CS = cpCore.db.cs_insertRecord(cnAddons)
                                If cpCore.db.cs_ok(CS) Then
                                    _childListAddonID_Local = cpCore.db.cs_getInteger(CS, "ID")
                                    Call cpCore.db.cs_set(CS, "name", "Child Page List")
                                    Call cpCore.db.cs_set(CS, "ArgumentList", "Name")
                                    Call cpCore.db.cs_set(CS, "CopyText", "<ac type=""childlist"" name=""$name$"">")
                                    Call cpCore.db.cs_set(CS, "Content", "1")
                                    Call cpCore.db.cs_set(CS, "StylesFilename", "")
                                    If BuildSupportsGuid Then
                                        Call cpCore.db.cs_set(CS, "ccguid", addonGuidChildList)
                                    End If
                                End If
                                Call cpCore.db.cs_Close(CS)
                            End If
                            Call setProperty("ChildListAddonID", CStr(_childListAddonID_Local))
                        End If
                    End If
                Catch ex As Exception
                    cpCore.handleException(ex) : Throw
                End Try
                Return _childListAddonID_Local
            End Get
        End Property
        Private _childListAddonID_Local As Integer
        Private _childListAddonID_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property docTypeDeclarationAdmin() As String
            Get
                If Not _DocTypeAdmin_LocalLoaded Then
                    _DocTypeAdmin_LocalLoaded = True
                    _DocTypeAdmin_Local = getText("DocTypeDeclarationAdmin", DTDDefaultAdmin)
                End If
                Return _DocTypeAdmin_Local
            End Get
        End Property
        Private _DocTypeAdmin_Local As String
        Private _DocTypeAdmin_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property docTypeDeclaration() As String
            Get
                If Not _DocType_LocalLoaded Then
                    _DocType_LocalLoaded = True
                    _DocType_Local = getText("DocTypeDeclaration", DTDDefault)
                    If _DocType_Local = "" Then
                        _DocType_Local = DTDDefault
                        Call setProperty("DocTypeDeclaration", DTDDefault)
                    End If
                End If
                Return _DocType_Local
            End Get
        End Property
        Private _DocType_Local As String
        Private _DocType_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property useContentWatchLink() As Boolean
            Get
                If Not _UseContentWatchLink_LocalLoaded Then
                    _UseContentWatchLink_local = getBoolean("UseContentWatchLink", False)
                    _UseContentWatchLink_LocalLoaded = True
                End If
                Return _UseContentWatchLink_local
            End Get
        End Property
        Private _UseContentWatchLink_local As Boolean
        Private _UseContentWatchLink_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowTemplateLinkVerification() As Boolean
            Get
                Dim TestString As String
                '
                If Not _AllowTemplateLinkVerification_LocalLoaded Then
                    TestString = getText("AllowTemplateLinkVerification")
                    If TestString <> "" Then
                        '
                        ' read value from property
                        '
                        _AllowTemplateLinkVerification_Local = genericController.EncodeBoolean(TestString)
                    Else
                        '
                        ' Update - template link verification is needed for Template.IsSecure, so turn it on for new sites
                        '
                        Call setProperty("AllowTemplateLinkVerification", True)
                    End If
                    _AllowTemplateLinkVerification_LocalLoaded = True
                End If
                Return _AllowTemplateLinkVerification_Local
            End Get
        End Property
        Private _AllowTemplateLinkVerification_Local As Boolean
        Private _AllowTemplateLinkVerification_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowTestPointLogging() As Boolean
            Get
                If Not _AllowTestPointLogging_LocalLoaded Then
                    _AllowTestPointLogging_Local = getBoolean("AllowTestPointLogging", False)
                    _AllowTestPointLogging_LocalLoaded = True
                End If
                Return _AllowTestPointLogging_Local
            End Get
        End Property
        Private _AllowTestPointLogging_Local As Boolean
        Private _AllowTestPointLogging_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property defaultFormInputWidth() As Integer
            Get
                If Not _DefaultFormInputWidth_LocalLoaded Then
                    _DefaultFormInputWidth_Local = getinteger("DefaultFormInputWidth", 60)
                    _DefaultFormInputWidth_LocalLoaded = True
                End If
                Return _DefaultFormInputWidth_Local
            End Get
        End Property
        Private _DefaultFormInputWidth_Local As Integer
        Private _DefaultFormInputWidth_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property selectFieldWidthLimit() As Integer
            Get
                If Not _SelectFieldWidthLimit_LocalLoaded Then
                    _SelectFieldWidthLimit_Local = getinteger("SelectFieldWidthLimit", 200)
                    _SelectFieldWidthLimit_LocalLoaded = True
                End If
                Return _SelectFieldWidthLimit_Local
            End Get
        End Property
        Private _SelectFieldWidthLimit_Local As Integer
        Private _SelectFieldWidthLimit_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property selectFieldLimit() As Integer
            Get
                If Not _SelectFieldLimit_LocalLoaded Then
                    _SelectFieldLimit_Local = getinteger("SelectFieldLimit", 1000)
                    If _SelectFieldLimit_Local = 0 Then
                        _SelectFieldLimit_Local = 1000
                        Call setProperty("SelectFieldLimit", _SelectFieldLimit_Local)
                    End If
                    _SelectFieldLimit_LocalLoaded = True
                End If
                Return _SelectFieldLimit_Local
            End Get
        End Property
        Private _SelectFieldLimit_Local As Integer
        Private _SelectFieldLimit_LocalLoaded As Boolean
        '
        '====================================================================================================
        '
        Public ReadOnly Property defaultFormInputTextHeight() As Integer
            Get
                If Not _DefaultFormInputTextHeight_LocalLoaded Then
                    _DefaultFormInputTextHeight_Local = getinteger("DefaultFormInputTextHeight", 1)
                    _DefaultFormInputTextHeight_LocalLoaded = True
                End If
                Return _DefaultFormInputTextHeight_Local
            End Get
        End Property
        Private _DefaultFormInputTextHeight_Local As Integer
        Private _DefaultFormInputTextHeight_LocalLoaded As Boolean
        '
        '====================================================================================================
        '
        Public ReadOnly Property emailAdmin() As String
            Get
                If Not _EmailAdmin_LocalLoaded Then
                    _EmailAdmin_Local = getText("main_EmailAdmin", "webmaster@" & cpCore.webServer.requestDomain)
                    _EmailAdmin_LocalLoaded = True
                End If
                Return _EmailAdmin_Local
            End Get
        End Property
        Private _EmailAdmin_Local As String
        Private _EmailAdmin_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property language() As String
            Get
                If Not _Language_LocalLoaded Then
                    _Language_Local = getText("Language", "English")
                    _Language_LocalLoaded = True
                End If
                Return _Language_Local
            End Get
        End Property
        Private _Language_Local As String
        Private _Language_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property adminURL() As String
            Get
                Dim Position As Integer
                If Not _AdminURL_LocalLoaded Then
                    _AdminURL_Local = getText("AdminURL", cpCore.serverConfig.appConfig.adminRoute)
                    Position = genericController.vbInstr(1, _AdminURL_Local, "?")
                    If Position <> 0 Then
                        _AdminURL_Local = Mid(_AdminURL_Local, 1, Position - 1)
                    End If
                    _AdminURL_LocalLoaded = True
                End If
                Return _AdminURL_Local
            End Get
        End Property
        Private _AdminURL_Local As String
        Private _AdminURL_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property calendarYearLimit() As Integer
            Get
                If Not _CalendarYearLimit_LocalLoaded Then
                    _CalendarYearLimit_Local = getinteger("CalendarYearLimit", 1)
                    _CalendarYearLimit_LocalLoaded = True
                End If
                Return _CalendarYearLimit_Local
            End Get
        End Property
        Private _CalendarYearLimit_Local As Integer
        Private _CalendarYearLimit_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property defaultFormInputHTMLHeight() As Integer
            Get
                If Not _DefaultFormInputHTMLHeight_LocalLoaded Then
                    _DefaultFormInputHTMLHeight_Local = getinteger("DefaultFormInputHTMLHeight", 500)
                    _DefaultFormInputHTMLHeight_LocalLoaded = True
                End If
                Return _DefaultFormInputHTMLHeight_Local
            End Get
        End Property
        Private _DefaultFormInputHTMLHeight_Local As Integer
        Private _DefaultFormInputHTMLHeight_LocalLoaded As Boolean = False
        ''
        ''====================================================================================================
        ''
        'Public ReadOnly Property allowWorkflowAuthoring() As Boolean
        '    Get
        '        If Not _AllowWorkflowAuthoring_LocalLoaded Then
        '            _AllowWorkflowAuthoring_Local = getBoolean("AllowWorkflowAuthoring", False)
        '            _AllowWorkflowAuthoring_LocalLoaded = True
        '        End If
        '        Return _AllowWorkflowAuthoring_Local
        '    End Get
        'End Property
        'Private _AllowWorkflowAuthoring_Local As Boolean
        'Private _AllowWorkflowAuthoring_LocalLoaded As Boolean = False
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowPathBlocking() As Boolean
            Get
                If Not _AllowPathBlocking_LocalLoaded Then
                    _AllowPathBlocking_Local = getBoolean("AllowPathBlocking", False)
                    _AllowPathBlocking_LocalLoaded = True
                End If
                Return _AllowPathBlocking_Local
            End Get
        End Property
        Private _AllowPathBlocking_Local As Boolean
        Private _AllowPathBlocking_LocalLoaded As Boolean = False
        '
        '========================================================================
        ''' <summary>
        ''' Set a site property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="Value"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal Value As String)
            Try
                If (Not String.IsNullOrEmpty(propertyName.Trim())) Then
                    '
                    ' -- set value in Db
                    Dim SQLNow As String = cpCore.db.encodeSQLDate(Now)
                    Dim SQL As String = "UPDATE ccSetup Set FieldValue=" & cpCore.db.encodeSQLText(Value) & ",ModifiedDate=" & SQLNow & " WHERE name=" & cpCore.db.encodeSQLText(propertyName)
                    Dim recordsAffected As Integer = 0
                    Call cpCore.db.executeNonQuery(SQL,, recordsAffected)
                    If (recordsAffected = 0) Then
                        SQL = "INSERT INTO ccSetup (ACTIVE,CONTENTCONTROLID,NAME,FIELDVALUE,ModifiedDate,DateAdded)VALUES(" _
                            & SQLTrue _
                            & "," & cpCore.db.encodeSQLNumber(sitePropertyContentId) _
                            & "," & cpCore.db.encodeSQLText(UCase(propertyName)) _
                            & "," & cpCore.db.encodeSQLText(Value) _
                            & "," & SQLNow _
                            & "," & SQLNow _
                            & ");"
                        Call cpCore.db.executeQuery(SQL)
                    End If
                    '
                    ' -- set simple lazy cache
                    Dim cacheName As String = "siteproperty" & propertyName.Trim().ToLower()
                    If nameValueDict.ContainsKey(cacheName) Then
                        nameValueDict.Remove(cacheName)
                    End If
                    nameValueDict.Add(cacheName, Value)
                    '
                    ' -- set cache, no memory cache not used, instead load all into local cache on load
                    'cpCore.cache.setObject(cacheName, Value)
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Set a site property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="Value"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal Value As Boolean)
            If Value Then
                setProperty(propertyName, "true")
            Else
                setProperty(propertyName, "false")
            End If
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' Set a site property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="Value"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal Value As Integer)
            setProperty(propertyName, Value.ToString)
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' get site property without a cache check, return as text. If not found, set and return default value
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <param name="memberId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getTextFromDb(ByVal PropertyName As String, ByVal DefaultValue As String, ByRef return_propertyFound As Boolean) As String
            Dim returnString As String = ""
            Try
                Using dt As DataTable = cpCore.db.executeQuery("select FieldValue from ccSetup where name=" & cpCore.db.encodeSQLText(PropertyName) & " order by id")
                    If dt.Rows.Count > 0 Then
                        returnString = genericController.encodeText(dt.Rows(0).Item("FieldValue"))
                        return_propertyFound = True
                    ElseIf (DefaultValue <> "") Then
                        ' do not set - set may have to save, and save needs contentId, which now loads ondemand, which checks cache, which does a getSiteProperty.
                        Call setProperty(PropertyName, DefaultValue)
                        returnString = DefaultValue
                        return_propertyFound = True
                    Else
                        returnString = ""
                        return_propertyFound = False
                    End If
                End Using
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnString
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get site property, return as text. If not found, set and return default value
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <param name="memberId"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getText(ByVal PropertyName As String, ByVal DefaultValue As String) As String
            Dim returnString As String = ""
            Try
                Dim cacheName As String = "siteproperty" & PropertyName.Trim().ToLower()
                If (String.IsNullOrEmpty(PropertyName.Trim())) Then
                    '
                    ' -- bad property name 
                    returnString = DefaultValue
                Else
                    '
                    ' -- test simple lazy cache to keep from reading the same property mulitple times on one doc
                    If nameValueDict.ContainsKey(cacheName) Then
                        '
                        ' -- property in memory cache
                        returnString = nameValueDict(cacheName)
                    Else
                        '
                        ' -- read property from cache, no, with preloaded local cache, this will never be used
                        If False Then
                            'Dim returnObj As Object = cpCore.cache.getObject(Of String)(cacheName)
                            'If (returnObj IsNot Nothing) Then
                            ''
                            '' -- found in cache, save in simple cache and return
                            'returnString = encodeText(returnObj)
                            'nameValueDict.Add(cacheName, returnString)
                        Else
                            '
                            ' -- not found in cache, read property from Db
                            Dim propertyFound As Boolean = False
                            returnString = getTextFromDb(PropertyName, DefaultValue, propertyFound)
                            If (propertyFound) Then
                                '
                                ' -- found in Db, already saved in local cache, memory cache not used
                                ' nameValueDict.Add(cacheName, returnString)
                                'cpCore.cache.setObject(cacheName, returnString)
                            Else
                                '
                                ' -- property not found in db, if default is not blank, write it and set cache
                                returnString = DefaultValue
                                nameValueDict.Add(cacheName, returnString)
                                If (returnString <> "") Then
                                    Call setProperty(cacheName, DefaultValue)
                                End If
                            End If
                        End If
                    End If
                End If
                'Dim cacheName As String = "siteProperty-" & PropertyName
                'returnString = cpCore.cache.getObject(Of String)(cacheName)
                'If String.IsNullOrEmpty(returnString) Then
                '    Dim propertyFound As Boolean = False
                '    returnString = getTextFromDb(PropertyName, DefaultValue, propertyFound)
                '    If (propertyFound) And (returnString <> "") Then
                '        Call cpCore.cache.setObject(cacheName, returnString)
                '    End If
                'End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnString
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get site property and return string
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <returns></returns>
        Public Function getText(ByVal PropertyName As String) As String
            Return getText(PropertyName, Nothing)
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get site property and return integer
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <returns></returns>
        Public Function getinteger(ByVal PropertyName As String, Optional ByVal DefaultValue As Integer = 0) As Integer
            Return genericController.EncodeInteger(getText(PropertyName, DefaultValue.ToString))
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get site property and return boolean
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <returns></returns>
        Public Function getBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As Boolean = False) As Boolean
            Return genericController.EncodeBoolean(getText(PropertyName, DefaultValue.ToString))
        End Function
        '
        '========================================================================
        ''' <summary>
        ''' get a site property as a date 
        ''' </summary>
        ''' <param name="PropertyName"></param>
        ''' <param name="DefaultValue"></param>
        ''' <returns></returns>
        Public Function getDate(ByVal PropertyName As String, Optional ByVal DefaultValue As Date = Nothing) As Date
            Return genericController.EncodeDate(getText(PropertyName, DefaultValue.ToString))
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' allowCache site property, not cached (to make it available to the cache process)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property allowCache_notCached() As Boolean
            Get
                '
                ' site property now runs query
                '
                'Return True
                Dim propertyFound As Boolean = False
                If Not siteProperty_AllowCache_LocalLoaded Then
                    siteProperty_AllowCache_Local = genericController.EncodeBoolean(getTextFromDb("AllowBake", "0", propertyFound))
                    siteProperty_AllowCache_LocalLoaded = True
                End If
                allowCache_notCached = siteProperty_AllowCache_Local
            End Get
        End Property
        Private siteProperty_AllowCache_LocalLoaded As Boolean = False
        Private siteProperty_AllowCache_Local As Boolean
        '
        '====================================================================================================
        ''' <summary>
        ''' The code version used to update the database last
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dataBuildVersion As String
            Get
                Dim returnString = ""
                Try

                    If Not _dataBuildVersion_Loaded Then
                        _dataBuildVersion = getText("BuildVersion", "")
                        If _dataBuildVersion = "" Then
                            _dataBuildVersion = "0.0.000"
                        End If
                        _dataBuildVersion_Loaded = True
                    End If
                    returnString = _dataBuildVersion
                Catch ex As Exception
                    cpCore.handleException(ex) : Throw
                End Try
                Return returnString
            End Get
        End Property
        Private _dataBuildVersion As String
        Friend _dataBuildVersion_Loaded As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' setup a local simple cache (not lazy because it preloads)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="someCriteria"></param>
        ''' <returns></returns>
        Public Shared Function getNameValueDict(cpCore As coreClass) As Dictionary(Of String, String)
            Dim result As New Dictionary(Of String, String)
            Try
                Dim cs As New csController(cpCore)
                If (cs.openSQL("select name,FieldValue from ccsetup where (active>0) order by id")) Then
                    Do
                        Dim name As String = cs.getText("name").Trim().ToLower()
                        If (Not String.IsNullOrEmpty(name)) Then
                            If (Not result.ContainsKey(name)) Then
                                result.Add(name, cs.getText("FieldValue"))
                            End If
                        End If
                        cs.goNext()
                    Loop While cs.ok()
                End If
                cs.Close()
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return result
        End Function
    End Class
End Namespace