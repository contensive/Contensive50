
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
        Private ReadOnly Property dbNotReady() As Boolean
            Get
                Return (cpCore.serverConfig.appConfig.appStatus <> Models.Entity.serverConfigModel.appStatusEnum.OK)
            End Get
        End Property
        '
        '====================================================================================================
        '
        Private Function integerPropertyBase(propertyName As String, defaultValue As Integer, ByRef localStore As Integer?) As Integer
            If (dbNotReady) Then
                '
                ' -- Db not available yet, return default
                Return defaultValue
            ElseIf (localStore Is Nothing) Then
                '
                ' -- load local store 
                localStore = getinteger(propertyName, defaultValue)
            End If
            Return CInt(localStore)
        End Function
        '
        '====================================================================================================
        '
        Private Function booleanPropertyBase(propertyName As String, defaultValue As Boolean, ByRef localStore As Boolean?) As Boolean
            If (dbNotReady) Then
                '
                ' -- Db not available yet, return default
                Return defaultValue
            ElseIf (localStore Is Nothing) Then
                '
                ' -- load local store 
                localStore = getBoolean(propertyName, defaultValue)
            End If
            Return CBool(localStore)
        End Function
        '
        '====================================================================================================
        '
        Private Function textPropertyBase(propertyName As String, defaultValue As String, ByRef localStore As String) As String
            If (dbNotReady) Then
                '
                ' -- Db not available yet, return default
                Return defaultValue
            ElseIf (localStore Is Nothing) Then
                '
                ' -- load local store 
                localStore = getText(propertyName, defaultValue)
            End If
            Return localStore
        End Function
        '
        '====================================================================================================
        '
        Friend ReadOnly Property landingPageID() As Integer
            Get
                Return integerPropertyBase("LandingPageID", 0, _landingPageID)
            End Get
        End Property
        Private _landingPageID As Integer? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property trackGuestsWithoutCookies() As Boolean
            Get
                Return booleanPropertyBase("track guests without cookies", False, _trackGuestsWithoutCookies)
            End Get
        End Property
        Private _trackGuestsWithoutCookies As Boolean? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property AllowAutoLogin() As Boolean
            Get
                Return booleanPropertyBase("allowAutoLogin", False, _AllowAutoLogin)
            End Get
        End Property
        Private _AllowAutoLogin As Boolean? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property maxVisitLoginAttempts() As Integer
            Get
                Return integerPropertyBase("maxVisitLoginAttempts", 20, _maxVisitLoginAttempts)
            End Get
        End Property
        Private _maxVisitLoginAttempts As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property LoginIconFilename() As String
            Get
                Return textPropertyBase("LoginIconFilename", "/ccLib/images/ccLibLogin.GIF", _LoginIconFilename)
            End Get
        End Property
        Private _LoginIconFilename As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowVisitTracking() As Boolean
            Get
                Return booleanPropertyBase("allowVisitTracking", True, _allowVisitTracking)
            End Get
        End Property
        Private _allowVisitTracking As Boolean?
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowTransactionLog() As Boolean
            Get
                Return booleanPropertyBase("UseContentWatchLink", False, _allowTransactionLog)
            End Get
        End Property
        Private _allowTransactionLog As Boolean? = Nothing
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
                Return booleanPropertyBase("TrapErrors", True, _trapErrors)
            End Get
        End Property
        Private _trapErrors? As Boolean = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property serverPageDefault() As String
            Get
                Return textPropertyBase(siteproperty_serverPageDefault_name, siteproperty_serverPageDefault_defaultValue, _ServerPageDefault_local)
            End Get
        End Property
        Private _ServerPageDefault_local As String = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property defaultWrapperID() As Integer
            Get
                Return integerPropertyBase("DefaultWrapperID", 0, _defaultWrapperID)
            End Get
        End Property
        Private _defaultWrapperID As Integer? = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' allowLinkAlias
        ''' </summary>
        ''' <returns></returns>
        Friend ReadOnly Property allowLinkAlias() As Boolean
            Get
                Return booleanPropertyBase("allowLinkAlias", True, _allowLinkAlias_Local)
            End Get
        End Property
        Private _allowLinkAlias_Local As Boolean? = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property childListAddonID() As Integer
            Get
                Try
                    If (dbNotReady) Then
                        '
                        ' -- db not ready, return 0
                        Return 0
                    Else
                        If (_childListAddonID Is Nothing) Then
                            _childListAddonID = getinteger("ChildListAddonID", 0)
                            If _childListAddonID = 0 Then
                                Dim CS As Integer = cpCore.db.csOpen(cnAddons, "ccguid='" & addonGuidChildList & "'",,,,,, "ID")
                                If cpCore.db.csOk(CS) Then
                                    _childListAddonID = cpCore.db.csGetInteger(CS, "ID")
                                End If
                                Call cpCore.db.csClose(CS)
                                If _childListAddonID = 0 Then
                                    CS = cpCore.db.csInsertRecord(cnAddons)
                                    If cpCore.db.csOk(CS) Then
                                        _childListAddonID = cpCore.db.csGetInteger(CS, "ID")
                                        Call cpCore.db.csSet(CS, "name", "Child Page List")
                                        Call cpCore.db.csSet(CS, "ArgumentList", "Name")
                                        Call cpCore.db.csSet(CS, "CopyText", "<ac type=""childlist"" name=""$name$"">")
                                        Call cpCore.db.csSet(CS, "Content", "1")
                                        Call cpCore.db.csSet(CS, "StylesFilename", "")
                                        Call cpCore.db.csSet(CS, "ccguid", addonGuidChildList)
                                    End If
                                    Call cpCore.db.csClose(CS)
                                End If
                                Call setProperty("ChildListAddonID", CStr(_childListAddonID))
                            End If
                        End If
                    End If
                Catch ex As Exception
                    cpCore.handleException(ex) : Throw
                End Try
                Return CInt(_childListAddonID)
            End Get
        End Property
        Private _childListAddonID As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property docTypeDeclaration() As String
            Get
                Return textPropertyBase("DocTypeDeclaration", DTDDefault, _docTypeDeclaration)
            End Get
        End Property
        Private _docTypeDeclaration As String = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property useContentWatchLink() As Boolean
            Get
                Return booleanPropertyBase("UseContentWatchLink", False, _useContentWatchLink)
            End Get
        End Property
        Private _useContentWatchLink As Boolean? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property allowTestPointLogging() As Boolean
            Get
                Return booleanPropertyBase("AllowTestPointLogging", False, _allowTestPointLogging)
            End Get
        End Property
        Private _allowTestPointLogging As Boolean? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property defaultFormInputWidth() As Integer
            Get
                Return integerPropertyBase("DefaultFormInputWidth", 60, _defaultFormInputWidth)
            End Get
        End Property
        Private _defaultFormInputWidth As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property selectFieldWidthLimit() As Integer
            Get
                Return integerPropertyBase("SelectFieldWidthLimit", 200, _selectFieldWidthLimit)
            End Get
        End Property
        Private _selectFieldWidthLimit As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property selectFieldLimit() As Integer
            Get
                Return integerPropertyBase("SelectFieldLimit", 1000, _selectFieldLimit)
            End Get
        End Property
        Private _selectFieldLimit As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property defaultFormInputTextHeight() As Integer
            Get
                Return integerPropertyBase("DefaultFormInputTextHeight", 1, _defaultFormInputTextHeight)
            End Get
        End Property
        Private _defaultFormInputTextHeight As Integer? = Nothing
        '
        '====================================================================================================
        '
        Public ReadOnly Property emailAdmin() As String
            Get
                Return textPropertyBase("EmailAdmin", "webmaster@" & cpCore.webServer.requestDomain, _emailAdmin)
            End Get
        End Property
        Private _emailAdmin As String = Nothing
        '
        '========================================================================
        ''' <summary>
        ''' Set a site property
        ''' </summary>
        ''' <param name="propertyName"></param>
        ''' <param name="Value"></param>
        Public Sub setProperty(ByVal propertyName As String, ByVal Value As String)
            Try
                If (dbNotReady) Then
                    '
                    ' -- cannot set property
                    Throw New ApplicationException("Cannot set site property before Db is ready.")
                Else
                    If (Not String.IsNullOrEmpty(propertyName.Trim())) Then
                        If (propertyName.ToLower.Equals("adminurl")) Then
                            '
                            ' -- intercept adminUrl for compatibility, always use admin route instead
                        Else
                            '
                            ' -- set value in Db
                            Dim SQLNow As String = cpCore.db.encodeSQLDate(Now)
                            Dim SQL As String = "UPDATE ccSetup Set FieldValue=" & cpCore.db.encodeSQLText(Value) & ",ModifiedDate=" & SQLNow & " WHERE name=" & cpCore.db.encodeSQLText(propertyName)
                            Dim recordsAffected As Integer = 0
                            Call cpCore.db.executeNonQuery(SQL,, recordsAffected)
                            If (recordsAffected = 0) Then
                                SQL = "INSERT INTO ccSetup (ACTIVE,CONTENTCONTROLID,NAME,FIELDVALUE,ModifiedDate,DateAdded)VALUES(" _
                            & SQLTrue _
                            & "," & cpCore.db.encodeSQLNumber(models.complex.cdefmodel.getcontentid(cpcore,"site properties")) _
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
                    End If
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
                If (dbNotReady) Then
                    '
                    ' -- if not ready, return default 
                    returnString = DefaultValue
                Else
                    If (PropertyName.ToLower.Equals("adminurl")) Then
                        returnString = "/" & cpCore.serverConfig.appConfig.adminRoute
                    Else
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
                    End If
                End If
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
            Return getText(PropertyName, String.Empty)
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
                If (dbNotReady) Then
                    Return False
                Else
                    If (_allowCache_notCached Is Nothing) Then
                        Dim propertyFound As Boolean = False
                        _allowCache_notCached = genericController.EncodeBoolean(getTextFromDb("AllowBake", "0", propertyFound))
                    End If
                    Return CBool(_allowCache_notCached)
                End If
            End Get
        End Property
        Private _allowCache_notCached As Boolean? = Nothing
        '
        '====================================================================================================
        ''' <summary>
        ''' The code version used to update the database last
        ''' </summary>
        ''' <returns></returns>
        Public Property dataBuildVersion As String
            Get
                Return textPropertyBase("BuildVersion", "", _buildVersion)
                'Dim returnString = ""
                'Try
                '    If Not _dataBuildVersion_Loaded Then
                '        _dataBuildVersion = getText("BuildVersion", "")
                '        If _dataBuildVersion = "" Then
                '            _dataBuildVersion = "0.0.000"
                '        End If
                '        _dataBuildVersion_Loaded = True
                '    End If
                '    returnString = _dataBuildVersion
                'Catch ex As Exception
                '    cpCore.handleException(ex) : Throw
                'End Try
                'Return returnString
            End Get
            Set(value As String)
                setProperty("BuildVersion", value)
                _buildVersion = Nothing
            End Set
        End Property
        Private _buildVersion As String = Nothing
        '
        '====================================================================================================
        '
        Friend ReadOnly Property nameValueDict() As Dictionary(Of String, String)
            Get
                If (dbNotReady) Then
                    Throw New ApplicationException("Cannot access site property collection if database is not ready.")
                Else
                    If _nameValueDict Is Nothing Then
                        _nameValueDict = New Dictionary(Of String, String)
                        Dim cs As New csController(cpCore)
                        If (cs.openSQL("select name,FieldValue from ccsetup where (active>0) order by id")) Then
                            Do
                                Dim name As String = cs.getText("name").Trim().ToLower()
                                If (Not String.IsNullOrEmpty(name)) Then
                                    If (Not _nameValueDict.ContainsKey(name)) Then
                                        _nameValueDict.Add(name, cs.getText("FieldValue"))
                                    End If
                                End If
                                cs.goNext()
                            Loop While cs.ok()
                        End If
                        cs.Close()
                    End If
                End If
                Return _nameValueDict
            End Get
        End Property
        Private _nameValueDict As Dictionary(Of String, String) = Nothing
    End Class
End Namespace